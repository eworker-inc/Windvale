import { readFile } from "node:fs/promises";

async function Probeˉfailure(Moduleˉpath, Expectedˉstatus, Label) {
    const Bytes = await readFile(Moduleˉpath);
    if (!WebAssembly.validate(Bytes)) {
        throw new Error(`The ${Label} module failed WebAssembly validation.`);
    }
    const Instance = new WebAssembly.Instance(new WebAssembly.Module(Bytes), {});
    const Run = Instance.exports["Windvale.run"];
    const Result = Instance.exports["Windvale.result"];
    const Instructions = Instance.exports["Windvale.instructions"];
    if (typeof Run !== "function" ||
        !(Result instanceof WebAssembly.Global) ||
        !(Instructions instanceof WebAssembly.Global)) {
        throw new Error(`The ${Label} module omitted execution exports.`);
    }
    const Status = Run(1_000_000);
    const Steps = Instructions.value;
    if (Status !== Expectedˉstatus || Result.value !== 0 || Steps <= 1) {
        throw new Error(
            `The ${Label} result was ${Status}/${Result.value}/${Steps}; ` +
            `expected ${Expectedˉstatus}/0/positive.`,
        );
    }
    const Limited = Run(Steps - 1);
    if (Limited !== 3011 || Result.value !== 0 ||
        Instructions.value !== Steps - 1) {
        throw new Error(`The ${Label} budget boundary was not exact.`);
    }
    return { Status, Steps };
}

if (process.argv.length < 3 || process.argv.length > 6) {
    process.stderr.write(
        "Usage: node Tools/Verify/Probe-WebAssembly-Scalar-Dispatcher.mjs " +
        "<success.wasm> [overflow.wasm] [divide-zero.wasm] " +
        "[shift-failure.wasm]\n",
    );
    process.exitCode = 64;
} else {
    try {
        const Moduleˉpath = process.argv[2];
        const Bytes = await readFile(Moduleˉpath);
        if (!WebAssembly.validate(Bytes)) {
            throw new Error("The dispatcher module failed WebAssembly validation.");
        }
        const Module = new WebAssembly.Module(Bytes);
        const Instance = new WebAssembly.Instance(Module, {});
        const Run = Instance.exports["Windvale.run"];
        if (typeof Run !== "function") {
            throw new Error("The dispatcher module omitted Windvale.run.");
        }
        const Resultˉglobal = Instance.exports["Windvale.result"];
        const Instructionsˉglobal = Instance.exports["Windvale.instructions"];
        if (!(Resultˉglobal instanceof WebAssembly.Global) ||
            !(Instructionsˉglobal instanceof WebAssembly.Global)) {
            throw new Error("The dispatcher module omitted its result globals.");
        }
        const Status = Run(1_000_000);
        const Result = Resultˉglobal.value;
        const Instructions = Instructionsˉglobal.value;
        if (Status !== 0) {
            throw new Error(`The dispatcher returned status ${Status}; expected 0.`);
        }
        if (Result !== 42) {
            throw new Error(`The dispatcher returned ${Result}; expected 42.`);
        }
        if (Instructions <= 1) {
            throw new Error(`The dispatcher charged only ${Instructions} instructions.`);
        }
        const Limitedˉstatus = Run(Instructions - 1);
        const Limitedˉresult = Resultˉglobal.value;
        const Limitedˉinstructions = Instructionsˉglobal.value;
        if (Limitedˉstatus !== 3011 || Limitedˉresult !== 0 ||
            Limitedˉinstructions !== Instructions - 1) {
            throw new Error(
                `The dispatcher budget boundary was ${Limitedˉstatus}/` +
                `${Limitedˉresult}/${Limitedˉinstructions}; expected ` +
                `3011/0/${Instructions - 1}.`,
            );
        }
        const Repeatˉstatus = Run(Instructions);
        if (Repeatˉstatus !== 0 || Resultˉglobal.value !== Result ||
            Instructionsˉglobal.value !== Instructions) {
            throw new Error("The dispatcher did not reset deterministically.");
        }
        var Failureˉreport = "";
        const Failureˉcases = [
            [3, 3007, "overflow"],
            [4, 3032, "divide-zero"],
            [5, 3033, "shift-failure"],
        ];
        for (const [Argument, Expectedˉstatus, Label] of Failureˉcases) {
            if (process.argv.length <= Argument) continue;
            const Failure = await Probeˉfailure(
                process.argv[Argument],
                Expectedˉstatus,
                Label,
            );
            Failureˉreport += ` ${Label}-status=${Failure.Status}` +
                ` ${Label}-instructions=${Failure.Steps}`;
        }
        process.stdout.write(
            `dispatcher-engine status=Valid module-bytes=${Bytes.length} ` +
            `result=${Result} instructions=${Instructions} ` +
            `limited-status=${Limitedˉstatus}${Failureˉreport}\n`,
        );
    } catch (Errorˉvalue) {
        const Message = Errorˉvalue instanceof Error
            ? Errorˉvalue.stack ?? Errorˉvalue.message
            : String(Errorˉvalue);
        process.stderr.write(`dispatcher-engine status=Invalid ${Message}\n`);
        process.exitCode = 1;
    }
}
