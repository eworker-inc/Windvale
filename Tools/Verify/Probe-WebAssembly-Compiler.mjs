import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

if (process.argv.length < 5 || process.argv.length > 8) {
    throw new Error(
        "Usage: node Probe-WebAssembly-Compiler.mjs " +
            "<scalar-interpreter.wasm> <compiler.wvb> <source.wv> " +
            "[guest-budget] [outer-budget] [maximum-call-depth]",
    );
}

function parseU32(name, text) {
    if (!/^[0-9]+$/.test(text)) {
        throw new Error(`${name} must be an unsigned decimal integer.`);
    }
    const value = Number(text);
    if (!Number.isSafeInteger(value) || value < 1 || value > 0xFFFF_FFFF) {
        throw new Error(`${name} must be between 1 and 4294967295.`);
    }
    return value;
}

function sha256(bytes) {
    return createHash("sha256").update(bytes).digest("hex");
}

function singleSourceSet(source) {
    const sourceSet = Buffer.alloc(24 + source.length);
    sourceSet.writeUInt32LE(0x53535657, 0);
    sourceSet.writeUInt16LE(1, 4);
    sourceSet.writeUInt16LE(0, 6);
    sourceSet.writeUInt32LE(1, 8);
    sourceSet.writeUInt32LE(8, 12);
    sourceSet.writeUInt32LE(24, 16);
    sourceSet.writeUInt32LE(source.length, 20);
    source.copy(sourceSet, 24);
    return sourceSet;
}

function bytesRequest(candidate, input, guestBudget, maximumCallDepth) {
    const request = Buffer.alloc(24 + candidate.length + input.length);
    request.writeUInt32LE(0x49585657, 0);
    request.writeUInt16LE(2, 4);
    request.writeUInt16LE(0, 6);
    request.writeUInt32LE(guestBudget, 8);
    request.writeUInt32LE(maximumCallDepth, 12);
    request.writeUInt32LE(candidate.length, 16);
    request.writeUInt32LE(input.length, 20);
    candidate.copy(request, 24);
    input.copy(request, 24 + candidate.length);
    return request;
}

const interpreterPath = process.argv[2];
const compilerPath = process.argv[3];
const sourcePath = process.argv[4];
const guestBudget = parseU32("guest budget", process.argv[5] ?? "1");
const outerBudget = parseU32("outer budget", process.argv[6] ?? "200000000");
const maximumCallDepth = parseU32(
    "maximum call depth",
    process.argv[7] ?? "64",
);
const interpreter = readFileSync(interpreterPath);
const compiler = readFileSync(compilerPath);
const source = readFileSync(sourcePath);
const request = bytesRequest(
    compiler,
    singleSourceSet(source),
    guestBudget,
    maximumCallDepth,
);

if (!WebAssembly.validate(interpreter)) {
    throw new Error(`${interpreterPath}: WebAssembly.validate rejected the module.`);
}
const module = await WebAssembly.compile(interpreter);
if (WebAssembly.Module.imports(module).length !== 0) {
    throw new Error(`${interpreterPath}: the interpreter imports a host capability.`);
}
const instance = await WebAssembly.instantiate(module);
const exports = instance.exports;
const memory = exports["Windvale.memory"];
if (
    exports["Windvale.abi"]?.value !== 3 ||
    !(memory instanceof WebAssembly.Memory) ||
    memory.buffer.byteLength !== 129 * 65_536
) {
    throw new Error(`${interpreterPath}: the fixed ABI 3 memory contract is invalid.`);
}
const inputOffset = exports["Windvale.input_offset"].value;
const inputCapacity = exports["Windvale.input_capacity"].value;
const outputOffset = exports["Windvale.output_offset"].value;
const outputCapacity = exports["Windvale.output_capacity"].value;
if (request.length > inputCapacity) {
    throw new Error(
        `${compilerPath}: the ${request.length}-byte request exceeds the ` +
            `${inputCapacity}-byte interpreter input capacity.`,
    );
}

const linearMemory = new Uint8Array(memory.buffer);
linearMemory.set(request, inputOffset);
const started = performance.now();
const outerStatus = exports["Windvale.run"](outerBudget, request.length);
const elapsedMilliseconds = Math.round(performance.now() - started);
const outerInstructions = exports["Windvale.instructions"].value >>> 0;
const outputLength = exports["Windvale.output_length"].value;
if (outputLength < 0 || outputLength > outputCapacity) {
    throw new Error(`${interpreterPath}: the output length is outside the ABI region.`);
}
const output = Buffer.from(
    linearMemory.slice(outputOffset, outputOffset + outputLength),
);
const report = {
    interpreter: {
        bytes: interpreter.length,
        sha256: sha256(interpreter),
    },
    compiler: {
        bytes: compiler.length,
        sha256: sha256(compiler),
    },
    source: {
        bytes: source.length,
        sha256: sha256(source),
    },
    guestBudget,
    maximumCallDepth,
    outerBudget,
    outerStatus,
    outerInstructions,
    outputLength,
    guestStatus: outputLength >= 20 ? output.readUInt32LE(8) : null,
    guestInstructions: outputLength >= 20 ? output.readUInt32LE(12) : null,
    resultLength: outputLength >= 20 ? output.readUInt32LE(16) : null,
    elapsedMilliseconds,
};

if (outerStatus !== 0) {
    throw new Error(`The compiler did not enter guest execution: ${JSON.stringify(report)}`);
}
if (
    outputLength !== 20 ||
    output.readUInt32LE(0) !== 0x4F585657 ||
    output.readUInt16LE(4) !== 2 ||
    output.readUInt16LE(6) !== 0 ||
    report.guestStatus !== 3011 ||
    report.guestInstructions !== guestBudget ||
    report.resultLength !== 0
) {
    throw new Error(`The compiler entry response is invalid: ${JSON.stringify(report)}`);
}

console.log(JSON.stringify(report, null, 2));
