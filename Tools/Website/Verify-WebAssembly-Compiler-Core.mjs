import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { Compileˉverifyˉexecute } from "../Windvale.Playground/wwwroot/js/windvale-compiler-core.js";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = path.resolve(Scriptˉdirectory, "../..");
const Packageˉroot = path.join(Repositoryˉroot, "Artifacts/WebAssembly-Playground");
const [Interpreter, Compiler, Source, Hostedˉsource] = await Promise.all([
    readFile(path.join(Packageˉroot, "Wvb-Scalar-Interpreter.wasm")),
    readFile(path.join(Packageˉroot, "Windvale-Compiler-Direct.wasm")),
    readFile(path.join(
        Repositoryˉroot,
        "Tests/Fixtures/Source-Wvb/WebAssembly-Compiler-Success.wv",
    )),
    readFile(path.join(
        Repositoryˉroot,
        "Examples/Seed/Hello-Windvale.wv",
    )),
]);
const Outputˉlimitˉsource = new TextEncoder().encode(`module Browserˉoutputˉlimit profile hosted;

capability console.write_line;

export fn Main() -> i32 {
    var Line: text = "x";
${"    Line = Textˉconcat(Line, Line);\n".repeat(14)}    console.write_line(Line);
    console.write_line(Line);
    console.write_line(Line);
    console.write_line(Line);
    return 0;
}
`);

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
Equal(1_186_358, Result.Compilerˉinstructions, "compiler instructions");
Equal(0, Result.Executionˉstatus, "execution status");
Equal(42, Result.Executionˉresult, "execution result");
Equal(4, Result.Executionˉguestˉinstructions, "execution guest instructions");
Equal(8_877, Result.Executionˉouterˉinstructions, "execution outer instructions");

const Denied = await Compileˉverifyˉexecute(
    Interpreter,
    Compiler,
    Hostedˉsource,
    1_000_000,
    false,
);
Equal(3010, Denied.Executionˉstatus, "denied console status");
Equal(0, Denied.Executionˉguestˉinstructions, "denied console instructions");
Equal("", Denied.Standardˉoutput, "denied console output");

const Hello = await Compileˉverifyˉexecute(
    Interpreter,
    Compiler,
    Hostedˉsource,
    1_000_000,
    true,
);
Equal(0, Hello.Executionˉstatus, "Hello execution status");
Equal(0, Hello.Executionˉresult, "Hello execution result");
Equal("Hello from Windvale\n", Hello.Standardˉoutput, "Hello standard output");
Equal("hosted", Hello.Moduleˉprofile, "Hello module profile");
Equal(253, Hello.Wvb.byteLength, "Hello WVB length");
Equal(
    "0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f",
    Hello.Wvbˉsha256,
    "Hello WVB SHA-256",
);
Equal(2_031_866, Hello.Compilerˉinstructions, "Hello compiler instructions");
Equal(8, Hello.Executionˉguestˉinstructions, "Hello guest instructions");
Equal(15_623, Hello.Executionˉouterˉinstructions, "Hello outer instructions");

const Outputˉlimited = await Compileˉverifyˉexecute(
    Interpreter,
    Compiler,
    Outputˉlimitˉsource,
    1_000_000,
    true,
);
Equal(3013, Outputˉlimited.Executionˉstatus, "output limit status");
Equal(49_155, Outputˉlimited.Standardˉoutput.length, "bounded output length");
Equal(
    `${"x".repeat(16_384)}\n`.repeat(3),
    Outputˉlimited.Standardˉoutput,
    "all-or-nothing bounded output",
);

console.log(JSON.stringify({
    wvbBytes: Result.Wvb.byteLength,
    wvbSha256: Result.Wvbˉsha256,
    compilerInstructions: Result.Compilerˉinstructions,
    executionStatus: Result.Executionˉstatus,
    executionResult: Result.Executionˉresult,
    executionGuestInstructions: Result.Executionˉguestˉinstructions,
    executionOuterInstructions: Result.Executionˉouterˉinstructions,
    helloWvbBytes: Hello.Wvb.byteLength,
    helloWvbSha256: Hello.Wvbˉsha256,
    helloCompilerInstructions: Hello.Compilerˉinstructions,
    helloGuestInstructions: Hello.Executionˉguestˉinstructions,
    helloOuterInstructions: Hello.Executionˉouterˉinstructions,
    helloStandardOutput: Hello.Standardˉoutput,
}, null, 2));

function Equal(Expected, Actual, Boundary) {
    if (Expected !== Actual) {
        throw new Error(
            `Unexpected ${Boundary}: expected ${Expected}, received ${Actual}.`,
        );
    }
}
