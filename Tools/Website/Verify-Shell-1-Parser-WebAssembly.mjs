import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { Verifyˉexecuteˉwvb } from
    "../Windvale.Playground/wwwroot/js/windvale-compiler-core.js";

if (process.argv.length !== 3) {
    throw new Error(
        "Usage: node Tools/Website/Verify-Shell-1-Parser-WebAssembly.mjs " +
        "<smoke.wvb>",
    );
}

const Expectedˉbytes = 27_088;
const Expectedˉsha256 =
    "ffa2723513b4f3846beabbd89b7a4d67fb8bb7999ad79c7684e72756b1ea302f";

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
const Interpreterˉentry = Manifest.artifacts?.find(
    Entry => Entry.name === "scalar-interpreter-wasm",
);
if (!Interpreterˉentry) {
    throw new Error("The playground package has no scalar interpreter entry.");
}

const [Interpreter, Candidate] = await Promise.all([
    readFile(path.join(Packageˉroot, Interpreterˉentry.path)),
    readFile(path.resolve(process.argv[2])),
]);
Requireˉidentity(
    Interpreter,
    Interpreterˉentry.bytes,
    Interpreterˉentry.sha256,
    "packaged interpreter",
);
Requireˉidentity(
    Candidate,
    Expectedˉbytes,
    Expectedˉsha256,
    "Shell 1 parser WebAssembly smoke WVB",
);
const Result = await Verifyˉexecuteˉwvb(
    Interpreter,
    Candidate,
    20_000_000,
    false,
);
Equal(0, Result.Executionˉstatus, "execution status");
Equal(42, Result.Executionˉresult, "execution result");
Equal("", Result.Standardˉoutput, "standard output");
Equal("portable", Result.Moduleˉprofile, "module profile");
Equal(81_619, Result.Executionˉguestˉinstructions, "guest instructions");
Equal(80_257_283, Result.Executionˉouterˉinstructions, "outer instructions");

console.log(JSON.stringify({
    cases: 11,
    wvbBytes: Candidate.byteLength,
    wvbSha256: Result.Wvbˉsha256,
    executionStatus: Result.Executionˉstatus,
    executionResult: Result.Executionˉresult,
    executionGuestInstructions: Result.Executionˉguestˉinstructions,
    executionOuterInstructions: Result.Executionˉouterˉinstructions,
}, null, 2));

function Requireˉidentity(Bytes, Expectedˉbytes, Expectedˉsha256, Name) {
    Equal(Expectedˉbytes, Bytes.byteLength, `${Name} byte length`);
    Equal(
        Expectedˉsha256,
        createHash("sha256").update(Bytes).digest("hex"),
        `${Name} SHA-256`,
    );
}

function Equal(Expected, Actual, Boundary) {
    if (Expected !== Actual) {
        throw new Error(
            `Unexpected ${Boundary}: expected ${Expected}, received ${Actual}.`,
        );
    }
}
