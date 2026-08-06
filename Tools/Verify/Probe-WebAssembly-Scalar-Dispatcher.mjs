import { readFile } from "node:fs/promises";

if (process.argv.length !== 3) {
    process.stderr.write(
        "Usage: node Tools/Verify/Probe-WebAssembly-Scalar-Dispatcher.mjs <module.wasm>\n",
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
        const Result = Run();
        if (Result !== 42) {
            throw new Error(`The dispatcher returned ${Result}; expected 42.`);
        }
        process.stdout.write(
            `dispatcher-engine status=Valid module-bytes=${Bytes.length} result=${Result}\n`,
        );
    } catch (Errorˉvalue) {
        const Message = Errorˉvalue instanceof Error
            ? Errorˉvalue.stack ?? Errorˉvalue.message
            : String(Errorˉvalue);
        process.stderr.write(`dispatcher-engine status=Invalid ${Message}\n`);
        process.exitCode = 1;
    }
}
