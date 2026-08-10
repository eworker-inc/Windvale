import { createHash } from "node:crypto";
import { spawnSync } from "node:child_process";
import {
    mkdir,
    mkdtemp,
    readFile,
    rm,
    stat,
} from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Toolchainˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Native-Compiler",
);
const Packageˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Playground",
);
const Projectˉpath = path.join(
    Repositoryˉroot,
    "Windvale-Compiler-Memory.wvproj",
);
const Defaultˉoutput = path.join(
    Packageˉroot,
    "Windvale-Compiler-Memory.wvb",
);
const Temporaryˉprefix = path.join(
    tmpdir(),
    "windvale-webassembly-native-compiler-",
);

const Options = Parseˉarguments(process.argv.slice(2));
const Toolchainˉmanifest = await Readˉjson(path.join(Toolchainˉroot, "Manifest.json"));
const Packageˉmanifest = await Readˉjson(path.join(Packageˉroot, "Manifest.json"));

Require(
    Toolchainˉmanifest.format === "windvale-webassembly-native-compiler-1",
    "The WebAssembly native-compiler manifest format is invalid.",
);
Require(
    Toolchainˉmanifest.normalUseRequiresDotnet === false,
    "The native-compiler package does not declare a .NET-free normal path.",
);
Require(
    process.arch === "x64" && (process.platform === "win32" || process.platform === "linux"),
    `The native compiler does not support ${process.platform}-${process.arch}.`,
);

const Compilerˉname = process.platform === "win32"
    ? "windows-x64-source-compiler"
    : "linux-x64-source-compiler";
const Publisherˉname = process.platform === "win32"
    ? "windows-x64-wvb-publisher"
    : "linux-x64-wvb-publisher";
const Compilerˉartifact = Findˉartifact(
    Toolchainˉmanifest,
    Compilerˉname,
    "native compiler",
);
const Nativeˉfrontˉdoorˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/Native-Front-Door",
);
const Nativeˉfrontˉdoorˉmanifest = await Readˉjson(path.join(
    Nativeˉfrontˉdoorˉroot,
    "Manifest.json",
));
const Publisherˉartifact = Findˉartifact(
    Nativeˉfrontˉdoorˉmanifest,
    Publisherˉname,
    "native publisher",
);
const Compilerˉpath = Resolveˉartifact(
    Toolchainˉroot,
    Compilerˉartifact.path,
    "native compiler",
);
const Publisherˉpath = Resolveˉartifact(
    Nativeˉfrontˉdoorˉroot,
    Publisherˉartifact.path,
    "native publisher",
);
await Verifyˉartifact(Compilerˉpath, Compilerˉartifact, "native compiler");
await Verifyˉartifact(Publisherˉpath, Publisherˉartifact, "native publisher");

const Sourceˉpaths = await Readˉproject(Projectˉpath);
const Expectedˉcompiler = Packageˉmanifest.sourceCompiler;
Require(
    Expectedˉcompiler?.name === "portable-source-compiler" &&
        Expectedˉcompiler.path === "Windvale-Compiler-Memory.wvb" &&
        Number.isInteger(Expectedˉcompiler.bytes) &&
        typeof Expectedˉcompiler.sha256 === "string",
    "The browser compiler inventory entry is missing or invalid.",
);
const Expectedˉpath = Resolveˉartifact(
    Packageˉroot,
    Expectedˉcompiler.path,
    "browser compiler",
);
const Outputˉpath = Options.Check
    ? null
    : path.resolve(Options.Output ?? Defaultˉoutput);
if (Outputˉpath !== null) {
    Require(
        path.extname(Outputˉpath).toLowerCase() === ".wvb",
        "The compiler output must use the .wvb extension.",
    );
    await mkdir(path.dirname(Outputˉpath), { recursive: true });
}

const Temporaryˉdirectory = await mkdtemp(Temporaryˉprefix);
try {
    const Candidateˉpath = path.join(Temporaryˉdirectory, "Candidate.wvb");
    const Publishedˉpath = Options.Check
        ? path.join(Temporaryˉdirectory, "Published.wvb")
        : Outputˉpath;
    Runˉnative(
        Compilerˉpath,
        [...Sourceˉpaths, Candidateˉpath],
        "native compiler",
    );
    Runˉnative(
        Publisherˉpath,
        [Candidateˉpath, Publishedˉpath],
        "native publisher",
    );

    const Publishedˉbytes = await readFile(Publishedˉpath);
    const Publishedˉsha256 = Sha256(Publishedˉbytes);
    if (Options.Check) {
        const Expectedˉbytes = await readFile(Expectedˉpath);
        Require(
            Publishedˉbytes.byteLength === Expectedˉcompiler.bytes,
            `The regenerated compiler has ${Publishedˉbytes.byteLength} bytes; ` +
                `${Expectedˉcompiler.bytes} were expected.`,
        );
        Require(
            Publishedˉsha256 === Expectedˉcompiler.sha256,
            `The regenerated compiler SHA-256 is ${Publishedˉsha256}; ` +
                `${Expectedˉcompiler.sha256} was expected.`,
        );
        Require(
            Publishedˉbytes.equals(Expectedˉbytes),
            "The regenerated compiler is not byte-identical to the packaged compiler.",
        );
        console.log(
            "WebAssembly native compiler reproduction passed: " +
                `${Publishedˉbytes.byteLength} bytes, SHA-256 ${Publishedˉsha256}.`,
        );
    } else {
        console.log(`Published: ${Publishedˉpath}`);
        console.log(`Bytes: ${Publishedˉbytes.byteLength}`);
        console.log(`SHA-256: ${Publishedˉsha256}`);
    }
} finally {
    Require(
        Temporaryˉdirectory.startsWith(Temporaryˉprefix),
        "Refusing to remove an unexpected temporary directory.",
    );
    await rm(Temporaryˉdirectory, { recursive: true, force: true });
}

function Parseˉarguments(Arguments) {
    let Check = false;
    let Output = null;
    for (let Index = 0; Index < Arguments.length; Index += 1) {
        const Argument = Arguments[Index];
        if (Argument === "--check" && !Check) {
            Check = true;
            continue;
        }
        if (Argument === "-o" && Output === null && Index + 1 < Arguments.length) {
            Output = Arguments[Index + 1];
            Index += 1;
            continue;
        }
        Usage();
    }
    if (Check && Output !== null) {
        Usage();
    }
    return { Check, Output };
}

function Usage() {
    console.error(
        "Usage: node Tools/WebAssembly/Build-Compiler-Wvb.mjs " +
            "[--check | -o <output.wvb>]",
    );
    process.exit(64);
}

async function Readˉjson(Fileˉpath) {
    return JSON.parse(await readFile(Fileˉpath, "utf8"));
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
    Require(
        Information.size === Artifact.bytes,
        `The ${Boundary} byte length is invalid.`,
    );
    const Digest = Sha256(await readFile(Fileˉpath));
    Require(Digest === Artifact.sha256, `The ${Boundary} SHA-256 is invalid.`);
}

async function Readˉproject(Fileˉpath) {
    const Text = (await readFile(Fileˉpath, "utf8")).replace(/^\uFEFF/, "");
    const Lines = Text.split(/\r?\n/u).map(Line => Line.trim()).filter(Boolean);
    Require(Lines.shift() === "windvale-project 1", "The compiler project header is invalid.");
    const Rootˉmatch = /^root "([^"\r\n]+)"$/u.exec(Lines.shift() ?? "");
    Require(Rootˉmatch !== null, "The compiler project root is invalid.");
    Require(Lines.pop() === "emit wvb", "The compiler project emission is invalid.");
    const Sources = [];
    for (const Line of Lines) {
        const Match = /^source "([^"\r\n]+)"$/u.exec(Line);
        Require(Match !== null, `The compiler project line is invalid: ${Line}`);
        Sources.push(Match[1]);
    }
    Require(Sources.length < 64, "The compiler project exceeds the native source limit.");
    const Projectˉdirectory = path.dirname(Fileˉpath);
    const Resolved = [Rootˉmatch[1], ...Sources].map(Relativeˉpath => {
        Require(!path.isAbsolute(Relativeˉpath), "The compiler project source must be relative.");
        const Sourceˉpath = path.resolve(Projectˉdirectory, Relativeˉpath);
        Require(
            Sourceˉpath.startsWith(`${Repositoryˉroot}${path.sep}`),
            "The compiler project source escapes the repository.",
        );
        return Sourceˉpath;
    });
    Require(
        new Set(Resolved.map(Item => process.platform === "win32" ? Item.toLowerCase() : Item)).size ===
            Resolved.length,
        "The compiler project contains duplicate sources.",
    );
    return Resolved;
}

function Runˉnative(Command, Arguments, Boundary) {
    const Result = spawnSync(Command, Arguments, {
        cwd: Repositoryˉroot,
        stdio: "inherit",
        windowsHide: true,
    });
    if (Result.error !== undefined) {
        throw Result.error;
    }
    Require(Result.status === 0, `The ${Boundary} exited with status ${Result.status}.`);
}

function Sha256(Bytes) {
    return createHash("sha256").update(Bytes).digest("hex");
}

function Require(Condition, Message) {
    if (!Condition) {
        throw new Error(Message);
    }
}
