import { readFile } from "node:fs/promises";

if (process.argv.length !== 3 && process.argv.length !== 4) {
    process.stderr.write(
        "Usage: node Tools/Verify/Probe-WebAssembly-Scalar-Dispatcher.mjs " +
        "<success.wasm> [overflow.wasm]\n",
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
        var Overflowˉreport = "";
        if (process.argv.length === 4) {
            const Overflowˉbytes = await readFile(process.argv[3]);
            if (!WebAssembly.validate(Overflowˉbytes)) {
                throw new Error("The overflow module failed WebAssembly validation.");
            }
            const Overflowˉinstance = new WebAssembly.Instance(
                new WebAssembly.Module(Overflowˉbytes),
                {},
            );
            const Overflowˉrun = Overflowˉinstance.exports["Windvale.run"];
            const Overflowˉresult = Overflowˉinstance.exports["Windvale.result"];
            const Overflowˉinstructions =
                Overflowˉinstance.exports["Windvale.instructions"];
            if (typeof Overflowˉrun !== "function" ||
                !(Overflowˉresult instanceof WebAssembly.Global) ||
                !(Overflowˉinstructions instanceof WebAssembly.Global)) {
                throw new Error("The overflow module omitted execution exports.");
            }
            const Overflowˉstatus = Overflowˉrun(1_000_000);
            const Overflowˉsteps = Overflowˉinstructions.value;
            if (Overflowˉstatus !== 3007 || Overflowˉresult.value !== 0 ||
                Overflowˉsteps <= 1) {
                throw new Error(
                    `The overflow result was ${Overflowˉstatus}/` +
                    `${Overflowˉresult.value}/${Overflowˉsteps}; expected ` +
                    "3007/0/positive.",
                );
            }
            const Overflowˉlimited = Overflowˉrun(Overflowˉsteps - 1);
            if (Overflowˉlimited !== 3011 || Overflowˉresult.value !== 0 ||
                Overflowˉinstructions.value !== Overflowˉsteps - 1) {
                throw new Error("The overflow budget boundary was not exact.");
            }
            Overflowˉreport = ` overflow-status=${Overflowˉstatus}` +
                ` overflow-instructions=${Overflowˉsteps}`;
        }
        process.stdout.write(
            `dispatcher-engine status=Valid module-bytes=${Bytes.length} ` +
            `result=${Result} instructions=${Instructions} ` +
            `limited-status=${Limitedˉstatus}${Overflowˉreport}\n`,
        );
    } catch (Errorˉvalue) {
        const Message = Errorˉvalue instanceof Error
            ? Errorˉvalue.stack ?? Errorˉvalue.message
            : String(Errorˉvalue);
        process.stderr.write(`dispatcher-engine status=Invalid ${Message}\n`);
        process.exitCode = 1;
    }
}
