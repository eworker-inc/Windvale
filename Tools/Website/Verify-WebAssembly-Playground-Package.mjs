import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Packageˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Playground",
);
const Manifest = await Readˉjson(path.join(Packageˉroot, "Manifest.json"));

Equal("windvale-webassembly-playground-2", Manifest.format, "manifest format");
Equal("wasm32-browser-v1-experimental", Manifest.target, "manifest target");
Equal("verified-copy", Manifest.normalPublication?.mode, "normal publication mode");
Equal(false, Manifest.normalPublication?.requiresDotnet, "normal .NET dependency");
Equal(
    "Documents/Decisions/0333-Segmented-Direct-WebAssembly-Compiler.md",
    Manifest.decision,
    "package decision",
);
const Decision = await readFile(path.join(Repositoryˉroot, Manifest.decision), "utf8");
if (!Decision.startsWith("# Decision 0333: Segmented direct WebAssembly compiler\n")) {
    Fail("The WebAssembly playground package decision is invalid.");
}
Equal(
    "Documents/Decisions/0421-Import-Free-Browser-Console-Envelope.md",
    Manifest.executionDecision,
    "browser execution decision",
);
const Executionˉdecision = await readFile(
    path.join(Repositoryˉroot, Manifest.executionDecision),
    "utf8",
);
if (!Executionˉdecision.startsWith(
    "# Decision 0421: Import-free browser console envelope\n",
)) {
    Fail("The browser execution decision is invalid.");
}
if (!Array.isArray(Manifest.artifacts) || Manifest.artifacts.length !== 2) {
    Fail("The WebAssembly playground manifest must own exactly two browser artifacts.");
}

await Verifyˉreferencedˉpackage(
    Manifest.nativeCompilerManifest,
    "windvale-webassembly-native-compiler-1",
    3,
    "native compiler",
);
await Verifyˉreferencedˉpackage(
    Manifest.nativeBackendManifest,
    "windvale-webassembly-native-backend-1",
    3,
    "native backend",
);
const Segmentedˉmanifest = await Verifyˉreferencedˉpackage(
    Manifest.segmentedBackendManifest,
    "windvale-webassembly-segmented-backend-1",
    2,
    "segmented backend",
);
Equal(false, Segmentedˉmanifest.normalUseRequiresDotnet, "segmented backend normal .NET dependency");
Equal(true, Segmentedˉmanifest.recoveryRequiresDotnet, "segmented backend recovery .NET dependency");
Equal(Manifest.backendCommit, Segmentedˉmanifest.sourceCommit, "segmented backend commit");
Equal(Manifest.decision, Segmentedˉmanifest.decision, "segmented backend decision");

const Compilerˉsource = await Verifyˉpackageˉentry(
    Manifest.sourceCompiler,
    Packageˉroot,
    "portable-source-compiler",
);
const Interpreterˉsource = await Verifyˉpackageˉentry(
    Manifest.interpreterSource,
    Packageˉroot,
    "scalar-interpreter-wvb",
);
Equal("pinned-native-source-compiler", Manifest.sourceCompiler.production, "source compiler production");
Equal("pinned-native-front-door", Manifest.interpreterSource.production, "interpreter source production");

const Artifacts = new Map();
for (const Artifact of Manifest.artifacts) {
    const Bytes = await Verifyˉpackageˉentry(Artifact, Packageˉroot, Artifact.name);
    if (Artifacts.has(Artifact.name)) {
        Fail(`The '${Artifact.name}' browser artifact is duplicated.`);
    }
    Artifacts.set(Artifact.name, { Artifact, Bytes });
}
Equal(2, Artifacts.size, "browser artifact name count");
const Direct = Requireˉartifact(Artifacts, "direct-source-compiler-wasm");
const Interpreter = Requireˉartifact(Artifacts, "scalar-interpreter-wasm");
Equal("Windvale-Compiler-Memory.wvb", Direct.Artifact.sourceWvb, "direct compiler source WVB");
Equal("pinned-segmented-webassembly-compiler", Direct.Artifact.production, "direct compiler production");
Equal("Wvb-Scalar-Interpreter.wvb", Interpreter.Artifact.sourceWvb, "interpreter source WVB");
Equal("pinned-native-webassembly-compiler", Interpreter.Artifact.production, "interpreter production");
Equal(Manifest.sourceCompiler.bytes, Compilerˉsource.byteLength, "source compiler byte length");
Equal(Manifest.interpreterSource.bytes, Interpreterˉsource.byteLength, "interpreter source byte length");

const Directˉinstance = await Verifyˉwebassembly(
    Direct.Bytes,
    4,
    1,
    2_497,
    142_671_872,
    4_194_304,
    146_866_176,
    16_777_216,
    "direct compiler",
);
await Verifyˉwebassembly(
    Interpreter.Bytes,
    3,
    1,
    129,
    65_536,
    4_194_304,
    4_259_840,
    4_194_304,
    "scalar interpreter",
);

const Source = await readFile(path.join(
    Repositoryˉroot,
    "Tests/Fixtures/Source-Wvb/WebAssembly-Compiler-Success.wv",
));
const Sourceˉset = Buildˉsourceˉset(Source);
const Exports = Directˉinstance.exports;
const Memory = Exports["Windvale.memory"];
new Uint8Array(
    Memory.buffer,
    Readˉglobal(Exports, "Windvale.input_offset"),
    Sourceˉset.byteLength,
).set(Sourceˉset);
Equal(0, Exports["Windvale.run"](2_000_000, Sourceˉset.byteLength), "direct compiler status");
Equal(1_186_358, Readˉglobal(Exports, "Windvale.instructions"), "direct compiler instructions");
Equal(199, Readˉglobal(Exports, "Windvale.output_length"), "direct compiler output length");
const Output = Buffer.from(new Uint8Array(
    Memory.buffer,
    Readˉglobal(Exports, "Windvale.output_offset"),
    199,
));
Equal(0x4F43_5657, Output.readUInt32LE(0), "WVCO magic");
Equal(1, Output.readUInt16LE(4), "WVCO version");
Equal(0, Output.readUInt16LE(6), "WVCO flags");
Equal(0, Output.readUInt32LE(8), "WVCO result kind");
Equal(183, Output.readUInt32LE(12), "WVB result length");
const Wvb = Output.subarray(16);
Equal(
    "3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6",
    Sha256(Wvb),
    "compiled WVB SHA-256",
);

console.log(
    "WebAssembly playground package verification passed: " +
    `${Direct.Bytes.byteLength} direct compiler bytes, ` +
    `${Interpreter.Bytes.byteLength} interpreter bytes, exact 183-byte WVB.`,
);

async function Verifyˉreferencedˉpackage(
    Relativeˉmanifest,
    Expectedˉformat,
    Expectedˉartifacts,
    Boundary,
) {
    if (typeof Relativeˉmanifest !== "string" || Relativeˉmanifest.length === 0) {
        Fail(`The ${Boundary} manifest path is invalid.`);
    }
    const Manifestˉpath = path.resolve(Packageˉroot, Relativeˉmanifest);
    if (!Manifestˉpath.startsWith(`${path.dirname(Packageˉroot)}${path.sep}`)) {
        Fail(`The ${Boundary} manifest path escapes the artifact inventory.`);
    }
    const Referenced = await Readˉjson(Manifestˉpath);
    Equal(Expectedˉformat, Referenced.format, `${Boundary} package format`);
    if (!Array.isArray(Referenced.artifacts) ||
        Referenced.artifacts.length !== Expectedˉartifacts) {
        Fail(`The ${Boundary} artifact inventory is invalid.`);
    }
    const Root = path.dirname(Manifestˉpath);
    for (const Artifact of Referenced.artifacts) {
        await Verifyˉpackageˉentry(Artifact, Root, `${Boundary} ${Artifact.name}`);
    }
    return Referenced;
}

async function Verifyˉpackageˉentry(Artifact, Root, Boundary) {
    if (Artifact === null || typeof Artifact !== "object" ||
        typeof Artifact.path !== "string" || Artifact.path.length === 0 ||
        !Number.isInteger(Artifact.bytes) || Artifact.bytes < 1 ||
        typeof Artifact.sha256 !== "string" || !/^[0-9a-f]{64}$/u.test(Artifact.sha256)) {
        Fail(`The ${Boundary} package entry is invalid.`);
    }
    const Resolved = path.resolve(Root, Artifact.path);
    if (!Resolved.startsWith(`${path.resolve(Root)}${path.sep}`)) {
        Fail(`The ${Boundary} path escapes its package.`);
    }
    const Bytes = await readFile(Resolved);
    Equal(Artifact.bytes, Bytes.byteLength, `${Boundary} byte length`);
    Equal(Artifact.sha256, Sha256(Bytes), `${Boundary} SHA-256`);
    return Bytes;
}

function Requireˉartifact(Artifacts, Name) {
    const Artifact = Artifacts.get(Name);
    if (Artifact === undefined) {
        Fail(`The '${Name}' browser artifact is missing.`);
    }
    return Artifact;
}

async function Verifyˉwebassembly(
    Bytes,
    Abi,
    Outputˉkind,
    Pages,
    Inputˉoffset,
    Inputˉcapacity,
    Outputˉoffset,
    Outputˉcapacity,
    Boundary,
) {
    if (!WebAssembly.validate(Bytes)) {
        Fail(`The packaged ${Boundary} is not valid WebAssembly.`);
    }
    const Module = await WebAssembly.compile(Bytes);
    Equal(0, WebAssembly.Module.imports(Module).length, `${Boundary} import count`);
    Equal(
        JSON.stringify([
            ["Windvale.run", "function"],
            ["Windvale.abi", "global"],
            ["Windvale.memory", "memory"],
            ["Windvale.input_offset", "global"],
            ["Windvale.input_capacity", "global"],
            ["Windvale.output_offset", "global"],
            ["Windvale.output_capacity", "global"],
            ["Windvale.output_length", "global"],
            ["Windvale.output_kind", "global"],
            ["Windvale.instructions", "global"],
        ]),
        JSON.stringify(WebAssembly.Module.exports(Module).map(Item => [Item.name, Item.kind])),
        `${Boundary} export contract`,
    );
    const Instance = await WebAssembly.instantiate(Module, {});
    const Exports = Instance.exports;
    const Memory = Exports["Windvale.memory"];
    Equal(Abi, Readˉglobal(Exports, "Windvale.abi"), `${Boundary} ABI`);
    Equal(Outputˉkind, Readˉglobal(Exports, "Windvale.output_kind"), `${Boundary} output kind`);
    if (!(Memory instanceof WebAssembly.Memory)) {
        Fail(`The ${Boundary} memory export is invalid.`);
    }
    Equal(Pages * 65_536, Memory.buffer.byteLength, `${Boundary} memory extent`);
    Equal(Inputˉoffset, Readˉglobal(Exports, "Windvale.input_offset"), `${Boundary} input offset`);
    Equal(Inputˉcapacity, Readˉglobal(Exports, "Windvale.input_capacity"), `${Boundary} input capacity`);
    Equal(Outputˉoffset, Readˉglobal(Exports, "Windvale.output_offset"), `${Boundary} output offset`);
    Equal(Outputˉcapacity, Readˉglobal(Exports, "Windvale.output_capacity"), `${Boundary} output capacity`);
    return Instance;
}

function Buildˉsourceˉset(Source) {
    const Result = Buffer.alloc(24 + Source.byteLength);
    Result.writeUInt32LE(0x5353_5657, 0);
    Result.writeUInt16LE(1, 4);
    Result.writeUInt16LE(0, 6);
    Result.writeUInt32LE(1, 8);
    Result.writeUInt32LE(8, 12);
    Result.writeUInt32LE(24, 16);
    Result.writeUInt32LE(Source.byteLength, 20);
    Source.copy(Result, 24);
    return Result;
}

async function Readˉjson(Fileˉpath) {
    return JSON.parse(await readFile(Fileˉpath, "utf8"));
}

function Readˉglobal(Exports, Name) {
    const Global = Exports[Name];
    if (!(Global instanceof WebAssembly.Global) || !Number.isInteger(Global.value)) {
        Fail(`The '${Name}' export is not an integer global.`);
    }
    return Global.value;
}

function Sha256(Bytes) {
    return createHash("sha256").update(Bytes).digest("hex");
}

function Equal(Expected, Actual, Boundary) {
    if (Expected !== Actual) {
        Fail(`Unexpected ${Boundary}: expected ${Expected}, received ${Actual}.`);
    }
}

function Fail(Message) {
    throw new Error(Message);
}
