import { createHash } from "node:crypto";
import { mkdir, mkdtemp, open, readFile, rename, rm, writeFile } from "node:fs/promises";
import path from "node:path";

const [Generatorˉargument, Inputˉargument, Outputˉargument] = process.argv.slice(2);
if (Generatorˉargument === undefined || Inputˉargument === undefined ||
    Outputˉargument === undefined || process.argv.length !== 5) {
    Usage();
}

const Generatorˉpath = path.resolve(Generatorˉargument);
const Inputˉpath = path.resolve(Inputˉargument);
const Outputˉpath = path.resolve(Outputˉargument);
Require(path.extname(Generatorˉpath).toLowerCase() === ".wasm",
    "The segmented generator must use the .wasm extension.");
Require(path.extname(Inputˉpath).toLowerCase() === ".wvb",
    "The segmented generator input must use the .wvb extension.");
Require(path.extname(Outputˉpath).toLowerCase() === ".wasm",
    "The segmented generator output must use the .wasm extension.");
Require(Generatorˉpath !== Outputˉpath,
    "The segmented generator and output paths must differ.");

const [Generatorˉbytes, Inputˉbytes] = await Promise.all([
    readFile(Generatorˉpath),
    readFile(Inputˉpath),
]);
Require(WebAssembly.validate(Generatorˉbytes),
    "The segmented generator is invalid WebAssembly.");
const Module = await WebAssembly.compile(Generatorˉbytes);
Require(WebAssembly.Module.imports(Module).length === 0,
    "The segmented generator imports a host capability.");
Requireˉexports(Module);
const Instance = await WebAssembly.instantiate(Module, {});
const Exports = Instance.exports;
const Memory = Exports["Windvale.memory"];
Require(Readˉglobal(Exports, "Windvale.abi") === 4,
    "The segmented generator does not implement execution ABI 4.");
Require(Readˉglobal(Exports, "Windvale.output_kind") === 3,
    "The segmented generator does not publish a segment manifest.");
Require(Memory instanceof WebAssembly.Memory,
    "The segmented generator omitted its linear memory.");
Require(Memory.buffer.byteLength === 2_497 * 65_536,
    "The segmented generator memory size is invalid.");

const Inputˉoffset = Readˉglobal(Exports, "Windvale.input_offset");
const Inputˉcapacity = Readˉglobal(Exports, "Windvale.input_capacity");
const Outputˉoffset = Readˉglobal(Exports, "Windvale.output_offset");
Require(Inputˉoffset === 142_671_872 && Inputˉcapacity === 4_194_304,
    "The segmented generator input region is invalid.");
Require(Outputˉoffset === 146_866_176 &&
    Readˉglobal(Exports, "Windvale.output_capacity") === 16_777_216,
    "The segmented generator output region is invalid.");
Require(Inputˉbytes.byteLength <= Inputˉcapacity,
    "The WVB input exceeds the segmented generator input region.");
new Uint8Array(Memory.buffer, Inputˉoffset, Inputˉbytes.byteLength)
    .set(Inputˉbytes);

const Started = performance.now();
const Status = Exports["Windvale.run"](-1, Inputˉbytes.byteLength);
const Elapsedˉmilliseconds = performance.now() - Started;
Require(Status === 0,
    `The segmented generator failed with status ${Status}.`);
Require(Readˉglobal(Exports, "Windvale.output_length") === 288,
    "The segmented generator manifest length is invalid.");

const View = new DataView(Memory.buffer, Outputˉoffset, 288);
Require(View.getUint32(0, true) === 1,
    "The segmented generator rejected the WVB input.");
const Totalˉlength = View.getUint32(8, true);
Require(Totalˉlength <= 67_108_864,
    "The segmented artifact exceeds its aggregate bound.");
const Segments = [];
let Reconstructedˉlength = 0;
for (let Index = 0; Index < 34; Index += 1) {
    const Descriptorˉoffset = (2 + Index) * 8;
    const Pointer = View.getUint32(Descriptorˉoffset, true);
    const Length = View.getUint32(Descriptorˉoffset + 4, true);
    Require(Length <= 4_194_304,
        `Segment ${Index} exceeds the Windvale bytes-value bound.`);
    Require(Pointer <= Memory.buffer.byteLength &&
        Length <= Memory.buffer.byteLength - Pointer,
        `Segment ${Index} escapes generator memory.`);
    Require(Reconstructedˉlength <= 67_108_864 - Length,
        "The segmented artifact length overflows its aggregate bound.");
    Segments.push(new Uint8Array(Memory.buffer, Pointer, Length).slice());
    Reconstructedˉlength += Length;
}
Require(Reconstructedˉlength === Totalˉlength,
    "The segmented artifact total length is inconsistent.");

const Result = new Uint8Array(Totalˉlength);
let Cursor = 0;
for (const Segment of Segments) {
    Result.set(Segment, Cursor);
    Cursor += Segment.byteLength;
}
Require(WebAssembly.validate(Result),
    "The reconstructed artifact is invalid WebAssembly.");

await mkdir(path.dirname(Outputˉpath), { recursive: true });
const Temporaryˉprefix = path.join(path.dirname(Outputˉpath), ".windvale-segmented-");
const Temporaryˉdirectory = await mkdtemp(Temporaryˉprefix);
try {
    const Candidateˉpath = path.join(Temporaryˉdirectory, "Candidate.wasm");
    await writeFile(Candidateˉpath, Result);
    const Handle = await open(Candidateˉpath, "r+");
    try {
        await Handle.sync();
    } finally {
        await Handle.close();
    }
    await rename(Candidateˉpath, Outputˉpath);
} finally {
    Require(Temporaryˉdirectory.startsWith(Temporaryˉprefix),
        "Refusing to remove an unexpected temporary directory.");
    await rm(Temporaryˉdirectory, { recursive: true, force: true });
}

const Digest = createHash("sha256").update(Result).digest("hex");
console.log(`Published: ${Outputˉpath}`);
console.log(`Bytes: ${Result.byteLength}`);
console.log(`SHA-256: ${Digest}`);
console.log(`Generator milliseconds: ${Elapsedˉmilliseconds.toFixed(1)}`);
console.log(`Instructions modulo 2^32: ${Readˉglobal(Exports, "Windvale.instructions") >>> 0}`);

function Requireˉexports(Module) {
    const Expected = [
        ["Windvale.run", "function"], ["Windvale.abi", "global"],
        ["Windvale.memory", "memory"], ["Windvale.input_offset", "global"],
        ["Windvale.input_capacity", "global"], ["Windvale.output_offset", "global"],
        ["Windvale.output_capacity", "global"], ["Windvale.output_length", "global"],
        ["Windvale.output_kind", "global"], ["Windvale.instructions", "global"],
    ];
    const Actual = WebAssembly.Module.exports(Module);
    Require(Actual.length === Expected.length && Expected.every((Item, Index) =>
        Actual[Index].name === Item[0] && Actual[Index].kind === Item[1]),
    "The segmented generator export contract is invalid.");
}

function Readˉglobal(Exports, Name) {
    const Value = Exports[Name];
    Require(Value instanceof WebAssembly.Global && Number.isInteger(Value.value),
        `The '${Name}' export is not an integer global.`);
    return Value.value;
}

function Require(Condition, Message) {
    if (!Condition) {
        throw new Error(Message);
    }
}

function Usage() {
    console.error(
        "Usage: node Tools/WebAssembly/Build-Segmented-WebAssembly.mjs " +
        "<generator.wasm> <input.wvb> <output.wasm>",
    );
    process.exit(64);
}
