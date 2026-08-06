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
const Manifest = JSON.parse(await readFile(
    path.join(Packageˉroot, "Manifest.json"),
    "utf8",
));

Equal("windvale-webassembly-playground-1", Manifest.format, "manifest format");
Equal("wasm32-browser-v1-experimental", Manifest.target, "manifest target");
Equal("verified-copy", Manifest.normalPublication?.mode, "normal publication mode");
Equal(false, Manifest.normalPublication?.requiresDotnet, "normal .NET dependency");
Equal(
    "../WebAssembly-Native-Compiler/Manifest.json",
    Manifest.nativeCompilerManifest,
    "native compiler manifest",
);
Equal(
    "../WebAssembly-Native-Backend/Manifest.json",
    Manifest.nativeBackendManifest,
    "native backend manifest",
);
Equal(
    "Documents/Decisions/0273-Warmed-WebAssembly-Compiler-Worker.md",
    Manifest.decision,
    "package decision",
);
const Decision = await readFile(path.join(Repositoryˉroot, Manifest.decision), "utf8");
if (!Decision.startsWith("# Decision 0273: Warmed WebAssembly compiler worker\n")) {
    Fail("The WebAssembly playground package decision is invalid.");
}
if (!Array.isArray(Manifest.artifacts) || Manifest.artifacts.length !== 3) {
    Fail("The WebAssembly playground manifest must own exactly three artifacts.");
}

const Nativeˉcompilerˉmanifestˉpath = path.resolve(
    Packageˉroot,
    Manifest.nativeCompilerManifest,
);
const Nativeˉcompilerˉroot = path.dirname(Nativeˉcompilerˉmanifestˉpath);
const Nativeˉcompilerˉmanifest = JSON.parse(await readFile(
    Nativeˉcompilerˉmanifestˉpath,
    "utf8",
));
Equal(
    "windvale-webassembly-native-compiler-1",
    Nativeˉcompilerˉmanifest.format,
    "native compiler package format",
);
Equal(false, Nativeˉcompilerˉmanifest.normalUseRequiresDotnet, "native compiler normal .NET dependency");
Equal(true, Nativeˉcompilerˉmanifest.recoveryRequiresDotnet, "native compiler recovery .NET dependency");
Equal("Windvale-Compiler.wvproj", Nativeˉcompilerˉmanifest.sourceProject, "native compiler project");
Equal(
    "Documents/Decisions/0277-Native-WebAssembly-Compiler-Regeneration.md",
    Nativeˉcompilerˉmanifest.decision,
    "native compiler decision",
);
const Nativeˉcompilerˉdecision = await readFile(
    path.join(Repositoryˉroot, Nativeˉcompilerˉmanifest.decision),
    "utf8",
);
if (!Nativeˉcompilerˉdecision.startsWith(
    "# Decision 0277: Native WebAssembly compiler regeneration\n",
)) {
    Fail("The WebAssembly native compiler decision is invalid.");
}
if (!Array.isArray(Nativeˉcompilerˉmanifest.artifacts) ||
    Nativeˉcompilerˉmanifest.artifacts.length !== 3) {
    Fail("The WebAssembly native compiler manifest must own exactly three artifacts.");
}
for (const Artifact of Nativeˉcompilerˉmanifest.artifacts) {
    const Artifactˉpath = path.resolve(Nativeˉcompilerˉroot, Artifact.path);
    if (!Artifactˉpath.startsWith(`${Nativeˉcompilerˉroot}${path.sep}`)) {
        Fail("A WebAssembly native compiler artifact path escapes its package.");
    }
    const Bytes = await readFile(Artifactˉpath);
    Equal(Artifact.bytes, Bytes.byteLength, `${Artifact.name} byte length`);
    Equal(
        Artifact.sha256,
        createHash("sha256").update(Bytes).digest("hex"),
        `${Artifact.name} SHA-256`,
    );
}

const Nativeˉbackendˉmanifestˉpath = path.resolve(
    Packageˉroot,
    Manifest.nativeBackendManifest,
);
const Nativeˉbackendˉroot = path.dirname(Nativeˉbackendˉmanifestˉpath);
const Nativeˉbackendˉmanifest = JSON.parse(await readFile(
    Nativeˉbackendˉmanifestˉpath,
    "utf8",
));
Equal(
    "windvale-webassembly-native-backend-1",
    Nativeˉbackendˉmanifest.format,
    "native backend package format",
);
Equal(false, Nativeˉbackendˉmanifest.normalUseRequiresDotnet, "native backend normal .NET dependency");
Equal(true, Nativeˉbackendˉmanifest.recoveryRequiresDotnet, "native backend recovery .NET dependency");
Equal(
    "Windvale-WebAssembly-Artifact-Tool.wvproj",
    Nativeˉbackendˉmanifest.sourceProject,
    "native backend project",
);
Equal(
    "compiler-family-wvha-1",
    Nativeˉbackendˉmanifest.containerProfile,
    "native backend container profile",
);
Equal(
    "Documents/Decisions/0278-Native-WebAssembly-Artifact-Regeneration.md",
    Nativeˉbackendˉmanifest.decision,
    "native backend decision",
);
const Nativeˉbackendˉdecision = await readFile(
    path.join(Repositoryˉroot, Nativeˉbackendˉmanifest.decision),
    "utf8",
);
if (!Nativeˉbackendˉdecision.startsWith(
    "# Decision 0278: Native WebAssembly artifact regeneration\n",
)) {
    Fail("The WebAssembly native backend decision is invalid.");
}
if (!Array.isArray(Nativeˉbackendˉmanifest.artifacts) ||
    Nativeˉbackendˉmanifest.artifacts.length !== 3) {
    Fail("The WebAssembly native backend manifest must own exactly three artifacts.");
}
for (const Artifact of Nativeˉbackendˉmanifest.artifacts) {
    const Artifactˉpath = path.resolve(Nativeˉbackendˉroot, Artifact.path);
    if (!Artifactˉpath.startsWith(`${Nativeˉbackendˉroot}${path.sep}`)) {
        Fail("A WebAssembly native backend artifact path escapes its package.");
    }
    const Bytes = await readFile(Artifactˉpath);
    Equal(Artifact.bytes, Bytes.byteLength, `${Artifact.name} byte length`);
    Equal(
        Artifact.sha256,
        createHash("sha256").update(Bytes).digest("hex"),
        `${Artifact.name} SHA-256`,
    );
}

const Artifacts = new Map();
for (const Artifact of Manifest.artifacts) {
    if (typeof Artifact.path !== "string" ||
        path.basename(Artifact.path) !== Artifact.path) {
        Fail("A WebAssembly playground artifact path escapes its package.");
    }
    const Bytes = await readFile(path.join(Packageˉroot, Artifact.path));
    Equal(Artifact.bytes, Bytes.byteLength, `${Artifact.name} byte length`);
    Equal(
        Artifact.sha256,
        createHash("sha256").update(Bytes).digest("hex"),
        `${Artifact.name} SHA-256`,
    );
    Artifacts.set(Artifact.name, Bytes);
}

const Interpreter = Artifacts.get("scalar-interpreter-wasm");
const Compiler = Artifacts.get("portable-source-compiler");
Equal(
    "pinned-native-source-compiler",
    Manifest.artifacts.find(Artifact => Artifact.name === "portable-source-compiler")?.production,
    "portable compiler production route",
);
Equal(
    "pinned-native-webassembly-compiler",
    Manifest.artifacts.find(Artifact => Artifact.name === "scalar-interpreter-wasm")?.production,
    "interpreter WebAssembly production route",
);
if (!WebAssembly.validate(Interpreter)) {
    Fail("The packaged scalar interpreter is not valid WebAssembly.");
}
const Module = await WebAssembly.compile(Interpreter);
Equal(0, WebAssembly.Module.imports(Module).length, "interpreter import count");
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
    JSON.stringify(WebAssembly.Module.exports(Module).map(
        Item => [Item.name, Item.kind],
    )),
    "interpreter export contract",
);

const Instance = await WebAssembly.instantiate(Module, {});
const Exports = Instance.exports;
const Memory = Exports["Windvale.memory"];
Equal(3, Readˉglobal(Exports, "Windvale.abi"), "execution ABI");
if (!(Memory instanceof WebAssembly.Memory)) {
    Fail("The packaged interpreter memory export is invalid.");
}
Equal(129 * 65_536, Memory.buffer.byteLength, "memory extent");
Equal(65_536, Readˉglobal(Exports, "Windvale.input_offset"), "input offset");
Equal(4_194_304, Readˉglobal(Exports, "Windvale.input_capacity"), "input capacity");
Equal(4_259_840, Readˉglobal(Exports, "Windvale.output_offset"), "output offset");
Equal(4_194_304, Readˉglobal(Exports, "Windvale.output_capacity"), "output capacity");

const Source = await readFile(path.join(
    Repositoryˉroot,
    "Tests/Fixtures/Source-Wvb/WebAssembly-Compiler-Success.wv",
));
const Request = Bytesˉrequest(Compiler, Source, 1, 64);
new Uint8Array(
    Memory.buffer,
    Readˉglobal(Exports, "Windvale.input_offset"),
    Request.byteLength,
).set(Request);
Equal(0, Exports["Windvale.run"](100_000_000, Request.byteLength), "outer status");
Equal(77_098_382, Readˉglobal(Exports, "Windvale.instructions"), "outer instructions");
Equal(20, Readˉglobal(Exports, "Windvale.output_length"), "response length");
const Output = Buffer.from(new Uint8Array(
    Memory.buffer,
    Readˉglobal(Exports, "Windvale.output_offset"),
    20,
));
Equal(0x4F58_5657, Output.readUInt32LE(0), "WVXO magic");
Equal(2, Output.readUInt16LE(4), "WVXO version");
Equal(0, Output.readUInt16LE(6), "WVXO flags");
Equal(3_011, Output.readUInt32LE(8), "guest budget status");
Equal(1, Output.readUInt32LE(12), "guest instruction count");
Equal(0, Output.readUInt32LE(16), "guest result length");

console.log(
    "WebAssembly playground package verification passed: " +
    `${Interpreter.byteLength} Wasm bytes, ${Compiler.byteLength} compiler bytes, ` +
    "exact one-instruction compiler admission.",
);

function Bytesˉrequest(Compilerˉbytes, Sourceˉbytes, Budget, Callˉdepth) {
    const Sourceˉset = Buffer.alloc(24 + Sourceˉbytes.length);
    Sourceˉset.writeUInt32LE(0x5353_5657, 0);
    Sourceˉset.writeUInt16LE(1, 4);
    Sourceˉset.writeUInt16LE(0, 6);
    Sourceˉset.writeUInt32LE(1, 8);
    Sourceˉset.writeUInt32LE(8, 12);
    Sourceˉset.writeUInt32LE(24, 16);
    Sourceˉset.writeUInt32LE(Sourceˉbytes.length, 20);
    Sourceˉbytes.copy(Sourceˉset, 24);

    const Request = Buffer.alloc(24 + Compilerˉbytes.length + Sourceˉset.length);
    Request.writeUInt32LE(0x4958_5657, 0);
    Request.writeUInt16LE(2, 4);
    Request.writeUInt16LE(0, 6);
    Request.writeUInt32LE(Budget, 8);
    Request.writeUInt32LE(Callˉdepth, 12);
    Request.writeUInt32LE(Compilerˉbytes.length, 16);
    Request.writeUInt32LE(Sourceˉset.length, 20);
    Compilerˉbytes.copy(Request, 24);
    Sourceˉset.copy(Request, 24 + Compilerˉbytes.length);
    return Request;
}

function Readˉglobal(Exports, Name) {
    const Global = Exports[Name];
    if (!(Global instanceof WebAssembly.Global) || !Number.isInteger(Global.value)) {
        Fail(`The '${Name}' export is not an integer global.`);
    }
    return Global.value;
}

function Equal(Expected, Actual, Boundary) {
    if (Expected !== Actual) {
        Fail(`Unexpected ${Boundary}: expected ${Expected}, received ${Actual}.`);
    }
}

function Fail(Message) {
    throw new Error(Message);
}
