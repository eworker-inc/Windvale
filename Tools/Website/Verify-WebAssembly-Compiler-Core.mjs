import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { Compileˉverifyˉexecute } from "../Windvale.Playground/wwwroot/js/windvale-compiler-core.js";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Packageˉroot = path.join(Repositoryˉroot, "Artifacts/WebAssembly-Playground");
const [Interpreter, Compiler, Source] = await Promise.all([
    readFile(path.join(Packageˉroot, "Wvb-Scalar-Interpreter.wasm")),
    readFile(path.join(Packageˉroot, "Windvale-Compiler-Memory.wvb")),
    readFile(path.join(
        Repositoryˉroot,
        "Tests/Fixtures/Source-Wvb/WebAssembly-Compiler-Success.wv",
    )),
]);

const Result = await Compileˉverifyˉexecute(
    Interpreter,
    Compiler,
    Source,
    1_000_000,
);
Equal(183, Result.Wvb.byteLength, "compiled WVB length");
Equal(
    "3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6",
    createHash("sha256").update(Result.Wvb).digest("hex"),
    "compiled WVB SHA-256",
);
Equal(1_183_292, Result.Compilerˉguestˉinstructions, "compiler guest instructions");
Equal(1_513_529_072, Result.Compilerˉouterˉinstructions, "compiler outer instructions");
Equal(0, Result.Executionˉstatus, "execution status");
Equal(42, Result.Executionˉresult, "execution result");
Equal(4, Result.Executionˉguestˉinstructions, "execution guest instructions");
Equal(8_554, Result.Executionˉouterˉinstructions, "execution outer instructions");

console.log(JSON.stringify({
    wvbBytes: Result.Wvb.byteLength,
    wvbSha256: Result.Wvbˉsha256,
    compilerGuestInstructions: Result.Compilerˉguestˉinstructions,
    compilerOuterInstructions: Result.Compilerˉouterˉinstructions,
    executionStatus: Result.Executionˉstatus,
    executionResult: Result.Executionˉresult,
    executionGuestInstructions: Result.Executionˉguestˉinstructions,
    executionOuterInstructions: Result.Executionˉouterˉinstructions,
}, null, 2));

function Equal(Expected, Actual, Boundary) {
    if (Expected !== Actual) {
        throw new Error(
            `Unexpected ${Boundary}: expected ${Expected}, received ${Actual}.`,
        );
    }
}
