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
const Backendˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Segmented-Backend",
);
const Packageˉroot = path.join(
    Repositoryˉroot,
    "Artifacts/WebAssembly-Playground",
);
const Defaultˉinput = path.join(Packageˉroot, "Windvale-Compiler-Memory.wvb");
const Defaultˉoutput = path.join(Packageˉroot, "Windvale-Compiler-Direct.wasm");
const Generatorˉscript = path.join(
    Repositoryˉroot,
    "Tools/WebAssembly/Build-Segmented-WebAssembly.mjs",
);

const Options = Parseˉarguments(process.argv.slice(2));
const Backendˉmanifest = await Readˉjson(path.join(Backendˉroot, "Manifest.json"));
const Packageˉmanifest = await Readˉjson(path.join(Packageˉroot, "Manifest.json"));
Require(
    Backendˉmanifest.format === "windvale-webassembly-segmented-backend-1",
    "The segmented WebAssembly backend manifest format is invalid.",
);
Require(
    Backendˉmanifest.normalUseRequiresDotnet === false,
    "The segmented WebAssembly backend does not declare a .NET-free normal path.",
);
Require(
    Packageˉmanifest.format === "windvale-webassembly-playground-2",
    "The WebAssembly playground manifest format is invalid.",
);

const Generatorˉartifact = Findˉartifact(
    Backendˉmanifest.artifacts,
    "segmented-webassembly-compiler-wasm",
    "segmented WebAssembly generator",
);
const Generatorˉpath = Resolveˉartifact(
    Backendˉroot,
    Generatorˉartifact.path,
    "segmented WebAssembly generator",
);
await Verifyˉartifact(
    Generatorˉpath,
    Generatorˉartifact,
    "segmented WebAssembly generator",
);

const Expectedˉinput = Packageˉmanifest.sourceCompiler;
Require(
    Expectedˉinput?.name === "portable-source-compiler",
    "The source compiler package entry is invalid.",
);
const Expectedˉoutput = Findˉartifact(
    Packageˉmanifest.artifacts,
    "direct-source-compiler-wasm",
    "direct compiler Wasm",
);
const Inputˉpath = path.resolve(Options.Input ?? Defaultˉinput);
const Outputˉpath = path.resolve(Options.Output ?? Defaultˉoutput);
Require(
    path.extname(Inputˉpath).toLowerCase() === ".wvb",
    "The direct compiler input must use the .wvb extension.",
);
Require(
    path.extname(Outputˉpath).toLowerCase() === ".wasm",
    "The direct compiler output must use the .wasm extension.",
);
Require(Inputˉpath !== Outputˉpath, "The direct compiler input and output must differ.");
await mkdir(path.dirname(Outputˉpath), { recursive: true });

const Inputˉbytes = await readFile(Inputˉpath);
if (Options.Check) {
    Require(
        Inputˉbytes.byteLength === Expectedˉinput.bytes &&
            Sha256(Inputˉbytes) === Expectedˉinput.sha256,
        "The checked source compiler WVB identity is invalid.",
    );
}

const Temporaryˉprefix = path.join(
    path.dirname(Outputˉpath),
    ".windvale-direct-compiler-build-",
);
const Temporaryˉdirectory = await mkdtemp(Temporaryˉprefix);
try {
    const Candidateˉpath = path.join(Temporaryˉdirectory, "Candidate.wasm");
    Runˉnode([
        Generatorˉscript,
        Generatorˉpath,
        Inputˉpath,
        Candidateˉpath,
    ]);
    const Candidateˉbytes = await readFile(Candidateˉpath);
    await Verifyˉwebassembly(Candidateˉbytes);
    const Candidateˉsha256 = Sha256(Candidateˉbytes);

    if (Options.Check) {
        const Expectedˉbytes = await readFile(Defaultˉoutput);
        Require(
            Candidateˉbytes.byteLength === Expectedˉoutput.bytes,
            `The regenerated direct compiler has ${Candidateˉbytes.byteLength} bytes; ` +
                `${Expectedˉoutput.bytes} were expected.`,
        );
        Require(
            Candidateˉsha256 === Expectedˉoutput.sha256,
            `The regenerated direct compiler SHA-256 is ${Candidateˉsha256}; ` +
                `${Expectedˉoutput.sha256} was expected.`,
        );
        Require(
            Candidateˉbytes.equals(Expectedˉbytes),
            "The regenerated direct compiler is not byte-identical to the package.",
        );
        console.log(
            "Segmented WVB-to-WebAssembly reproduction passed: " +
                `${Candidateˉbytes.byteLength} bytes, SHA-256 ${Candidateˉsha256}.`,
        );
    } else {
        const Candidateˉhandle = await open(Candidateˉpath, "r+");
        try {
            await Candidateˉhandle.sync();
        } finally {
            await Candidateˉhandle.close();
        }
        await rename(Candidateˉpath, Outputˉpath);
        console.log(`Published: ${Outputˉpath}`);
        console.log(`Bytes: ${Candidateˉbytes.byteLength}`);
        console.log(`SHA-256: ${Candidateˉsha256}`);
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
    let Input = null;
    let Output = null;
    for (let Index = 0; Index < Arguments.length; Index += 1) {
        const Argument = Arguments[Index];
        if (Argument === "--check" && !Check) {
            Check = true;
            continue;
        }
        if (Argument === "-i" && Input === null && Index + 1 < Arguments.length) {
            Input = Arguments[Index + 1];
            Index += 1;
            continue;
        }
        if (Argument === "-o" && Output === null && Index + 1 < Arguments.length) {
            Output = Arguments[Index + 1];
            Index += 1;
            continue;
        }
        Usage();
    }
    if (Check && (Input !== null || Output !== null)) {
        Usage();
    }
    if ((Input === null) !== (Output === null)) {
        Usage();
    }
    return { Check, Input, Output };
}

function Usage() {
    console.error(
        "Usage: node Tools/WebAssembly/Build-Direct-Compiler-Wasm.mjs " +
            "[--check | -i <input.wvb> -o <output.wasm>]",
    );
    process.exit(64);
}

async function Readˉjson(Fileˉpath) {
    return JSON.parse(await readFile(Fileˉpath, "utf8"));
}

function Findˉartifact(Artifacts, Name, Boundary) {
    Require(Array.isArray(Artifacts), `The ${Boundary} inventory is invalid.`);
    const Matches = Artifacts.filter(Artifact => Artifact.name === Name);
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

async function Verifyˉwebassembly(Bytes) {
    Require(WebAssembly.validate(Bytes), "The segmented generator emitted invalid WebAssembly.");
    const Module = await WebAssembly.compile(Bytes);
    Require(
        WebAssembly.Module.imports(Module).length === 0,
        "The direct compiler imports a host capability.",
    );
    const Expectedˉexports = [
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
    ];
    Require(
        JSON.stringify(WebAssembly.Module.exports(Module).map(Item => [Item.name, Item.kind])) ===
            JSON.stringify(Expectedˉexports),
        "The direct compiler export contract is invalid.",
    );
    const Instance = await WebAssembly.instantiate(Module, {});
    const Exports = Instance.exports;
    Require(Exports["Windvale.abi"]?.value === 4, "The direct compiler ABI is not version 4.");
    Require(Exports["Windvale.output_kind"]?.value === 1, "The direct compiler output kind is invalid.");
    Require(
        Exports["Windvale.memory"] instanceof WebAssembly.Memory &&
            Exports["Windvale.memory"].buffer.byteLength === 2_497 * 65_536,
        "The direct compiler memory extent is invalid.",
    );
    Require(Exports["Windvale.input_offset"]?.value === 142_671_872, "The input offset is invalid.");
    Require(Exports["Windvale.input_capacity"]?.value === 4_194_304, "The input capacity is invalid.");
    Require(Exports["Windvale.output_offset"]?.value === 146_866_176, "The output offset is invalid.");
    Require(Exports["Windvale.output_capacity"]?.value === 16_777_216, "The output capacity is invalid.");
}

function Runˉnode(Arguments) {
    const Result = spawnSync(process.execPath, Arguments, {
        cwd: Repositoryˉroot,
        stdio: "inherit",
        windowsHide: true,
    });
    if (Result.error !== undefined) {
        throw Result.error;
    }
    Require(Result.status === 0, `The segmented generator exited with status ${Result.status}.`);
}

function Sha256(Bytes) {
    return createHash("sha256").update(Bytes).digest("hex");
}

function Require(Condition, Message) {
    if (!Condition) {
        throw new Error(Message);
    }
}
