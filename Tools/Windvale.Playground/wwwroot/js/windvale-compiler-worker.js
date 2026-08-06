import { Compileˉverifyˉexecute } from "./windvale-compiler-core.js";

const Packageˉmanifestˉurl = new URL(
    "../webassembly-compiler/artifacts/Manifest.json",
    import.meta.url,
);
let Packageˉpromise = null;

self.onmessage = async Event => {
    const Message = Event.data;
    const Requestˉid = Message?.RequestId;
    try {
        if (!Number.isInteger(Requestˉid) ||
            !(Message.Source instanceof ArrayBuffer) ||
            !Number.isInteger(Message.ExecutionInstructionLimit)) {
            throw new Error("The compiler worker request is invalid.");
        }
        const Package = await (Packageˉpromise ??= Loadˉpackage());
        const Result = await Compileˉverifyˉexecute(
            Package.Interpreter,
            Package.Compiler,
            Package.Warmup,
            new Uint8Array(Message.Source),
            Message.ExecutionInstructionLimit,
        );
        const Wvb = Result.Wvb.slice();
        self.postMessage({
            RequestId: Requestˉid,
            Succeeded: true,
            Error: null,
            Wvb: Wvb.buffer,
            WvbSha256: Result.Wvbˉsha256,
            WarmupGuestInstructions: Result.Warmupˉguestˉinstructions,
            WarmupOuterInstructions: Result.Warmupˉouterˉinstructions,
            CompilerGuestInstructions: Result.Compilerˉguestˉinstructions,
            CompilerOuterInstructions: Result.Compilerˉouterˉinstructions,
            ExecutionStatus: Result.Executionˉstatus,
            ExecutionResult: Result.Executionˉresult,
            ExecutionGuestInstructions: Result.Executionˉguestˉinstructions,
            ExecutionOuterInstructions: Result.Executionˉouterˉinstructions,
        }, [Wvb.buffer]);
    }
    catch (Failure) {
        self.postMessage({
            RequestId: Number.isInteger(Requestˉid) ? Requestˉid : null,
            Succeeded: false,
            Error: Failure instanceof Error
                ? Failure.message
                : "The Windvale compiler worker failed.",
            Wvb: null,
            WvbSha256: null,
            WarmupGuestInstructions: null,
            WarmupOuterInstructions: null,
            CompilerGuestInstructions: null,
            CompilerOuterInstructions: null,
            ExecutionStatus: null,
            ExecutionResult: null,
            ExecutionGuestInstructions: null,
            ExecutionOuterInstructions: null,
        });
    }
};

async function Loadˉpackage() {
    const Manifestˉresponse = await fetch(
        Packageˉmanifestˉurl,
        { cache: "no-cache" },
    );
    if (!Manifestˉresponse.ok) {
        throw new Error("The Windvale compiler package manifest is unavailable.");
    }
    const Manifestˉcontentˉtype = Manifestˉresponse.headers.get("content-type") ?? "";
    if (!Manifestˉcontentˉtype.toLowerCase().includes("application/json")) {
        throw new Error(
            `The Windvale compiler package manifest at '${Manifestˉresponse.url}' ` +
            "did not return JSON.",
        );
    }
    const Manifest = await Manifestˉresponse.json();
    if (Manifest.format !== "windvale-webassembly-playground-1" ||
        Manifest.target !== "wasm32-browser-v1-experimental" ||
        !Array.isArray(Manifest.artifacts) ||
        Manifest.artifacts.length !== 4) {
        throw new Error("The Windvale compiler package manifest is invalid.");
    }
    const Interpreter = await Loadˉartifact(
        Manifest,
        Manifestˉresponse.url,
        "scalar-interpreter-wasm",
    );
    const Compiler = await Loadˉartifact(
        Manifest,
        Manifestˉresponse.url,
        "portable-source-compiler",
    );
    const Warmup = await Loadˉartifact(
        Manifest,
        Manifestˉresponse.url,
        "interpreter-tier-warmup",
    );
    return { Interpreter, Compiler, Warmup };
}

async function Loadˉartifact(Manifest, Manifestˉurl, Name) {
    const Artifact = Manifest.artifacts.find(Item => Item.name === Name);
    if (Artifact === undefined ||
        typeof Artifact.path !== "string" ||
        Artifact.path.includes("/") ||
        Artifact.path.includes("\\") ||
        !Number.isInteger(Artifact.bytes) ||
        typeof Artifact.sha256 !== "string") {
        throw new Error(`The '${Name}' package entry is invalid.`);
    }
    const Response = await fetch(
        new URL(encodeURIComponent(Artifact.path), Manifestˉurl),
        { cache: "no-cache" },
    );
    if (!Response.ok) {
        throw new Error(`The '${Name}' package artifact is unavailable.`);
    }
    const Bytes = new Uint8Array(await Response.arrayBuffer());
    if (Bytes.byteLength !== Artifact.bytes ||
        await Sha256(Bytes) !== Artifact.sha256) {
        throw new Error(`The '${Name}' package identity is invalid.`);
    }
    return Bytes;
}

async function Sha256(Bytes) {
    const Digest = await crypto.subtle.digest("SHA-256", Bytes);
    return Array.from(new Uint8Array(Digest))
        .map(Byte => Byte.toString(16).padStart(2, "0"))
        .join("");
}
