import { createHash } from "node:crypto";
import { spawnSync } from "node:child_process";
import {
    mkdir,
    mkdtemp,
    open,
    readFile,
    rename,
    rm,
    stat,
} from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Toolchainˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Native-Backend",
);

if (process.argv.length !== 4) {
    Usage();
}

const Inputˉpath = path.resolve(process.argv[2]);
const Outputˉpath = path.resolve(process.argv[3]);
Require(
    path.extname(Inputˉpath).toLowerCase() === ".wvb",
    "The WebAssembly compiler input must use the .wvb extension.",
);
Require(
    path.extname(Outputˉpath).toLowerCase() === ".wasm",
    "The WebAssembly compiler output must use the .wasm extension.",
);
Require(Inputˉpath !== Outputˉpath, "The WebAssembly input and output must differ.");

const Toolchainˉmanifest = JSON.parse(
    await readFile(path.join(Toolchainˉroot, "Manifest.json"), "utf8"),
);
Require(
    Toolchainˉmanifest.format === "windvale-webassembly-native-backend-1",
    "The WebAssembly native-backend manifest format is invalid.",
);
Require(
    Toolchainˉmanifest.normalUseRequiresDotnet === false,
    "The native WebAssembly compiler does not declare a .NET-free normal path.",
);
Require(
    process.arch === "x64" && (process.platform === "win32" || process.platform === "linux"),
    `The native WebAssembly compiler does not support ${process.platform}-${process.arch}.`,
);

const Compilerˉname = process.platform === "win32"
    ? "windows-x64-webassembly-compiler"
    : "linux-x64-webassembly-compiler";
const Compilerˉartifact = Findˉartifact(
    Toolchainˉmanifest,
    Compilerˉname,
    "native WebAssembly compiler",
);
const Compilerˉpath = Resolveˉartifact(
    Toolchainˉroot,
    Compilerˉartifact.path,
    "native WebAssembly compiler",
);
await Verifyˉartifact(
    Compilerˉpath,
    Compilerˉartifact,
    "native WebAssembly compiler",
);
const Inputˉinformation = await stat(Inputˉpath);
Require(Inputˉinformation.isFile(), "The WebAssembly compiler input is not a regular file.");

await mkdir(path.dirname(Outputˉpath), { recursive: true });
const Temporaryˉprefix = path.join(
    path.dirname(Outputˉpath),
    ".windvale-webassembly-compile-",
);
const Temporaryˉdirectory = await mkdtemp(Temporaryˉprefix);
try {
    const Candidateˉpath = path.join(Temporaryˉdirectory, "Candidate.wasm");
    Runˉnative(Compilerˉpath, [Inputˉpath, Candidateˉpath]);
    const Candidateˉbytes = await readFile(Candidateˉpath);
    Require(
        WebAssembly.validate(Candidateˉbytes),
        "The native compiler emitted invalid WebAssembly.",
    );
    const Module = await WebAssembly.compile(Candidateˉbytes);
    Require(
        WebAssembly.Module.imports(Module).length === 0,
        "The native compiler emitted a WebAssembly host import.",
    );
    const Candidateˉhandle = await open(Candidateˉpath, "r+");
    try {
        await Candidateˉhandle.sync();
    } finally {
        await Candidateˉhandle.close();
    }
    await rename(Candidateˉpath, Outputˉpath);
    console.log(
        "Native WVB-to-WebAssembly compilation passed: " +
            `${Candidateˉbytes.byteLength} bytes, SHA-256 ${Sha256(Candidateˉbytes)}.`,
    );
} finally {
    Require(
        Temporaryˉdirectory.startsWith(Temporaryˉprefix),
        "Refusing to remove an unexpected temporary directory.",
    );
    await rm(Temporaryˉdirectory, { recursive: true, force: true });
}

function Usage() {
    console.error(
        "Usage: node Tools/WebAssembly/Compile-Wvb-To-Wasm.mjs " +
            "<input.wvb> <output.wasm>",
    );
    process.exit(64);
}

function Findˉartifact(Manifest, Name, Boundary) {
    Require(Array.isArray(Manifest.artifacts), `The ${Boundary} inventory is invalid.`);
    const Matches = Manifest.artifacts.filter(Artifact => Artifact.name === Name);
    Require(Matches.length === 1, `The ${Boundary} inventory entry is missing or duplicated.`);
    return Matches[0];
}

function Resolveˉartifact(Root, Relativeˉpath, Boundary) {
    Require(
        typeof Relativeˉpath === "string" && Relativeˉpath.length !== 0,
        `The ${Boundary} path is invalid.`,
    );
    const Resolved = path.resolve(Root, Relativeˉpath);
    Require(
        Resolved.startsWith(`${path.resolve(Root)}${path.sep}`),
        `The ${Boundary} path escapes its inventory.`,
    );
    return Resolved;
}

async function Verifyˉartifact(Fileˉpath, Artifact, Boundary) {
    const Information = await stat(Fileˉpath);
    Require(Information.isFile(), `The ${Boundary} is not a regular file.`);
    Require(Information.size === Artifact.bytes, `The ${Boundary} byte length is invalid.`);
    Require(
        Sha256(await readFile(Fileˉpath)) === Artifact.sha256,
        `The ${Boundary} SHA-256 is invalid.`,
    );
}

function Runˉnative(Command, Arguments) {
    const Result = spawnSync(Command, Arguments, {
        cwd: Repositoryˉroot,
        stdio: "inherit",
        windowsHide: true,
    });
    if (Result.error !== undefined) {
        throw Result.error;
    }
    Require(Result.status === 0, `The native WebAssembly compiler exited with status ${Result.status}.`);
}

function Sha256(Bytes) {
    return createHash("sha256").update(Bytes).digest("hex");
}

function Require(Condition, Message) {
    if (!Condition) {
        throw new Error(Message);
    }
}
