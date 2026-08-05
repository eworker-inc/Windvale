import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

if (process.argv.length !== 3) {
    throw new Error(
        "Usage: node Probe-WebAssembly-Reclaiming-Calls.mjs " +
            "<runtime-calls-reclaim.wasm>",
    );
}

const path = process.argv[2];
const bytes = readFileSync(path);
if (!WebAssembly.validate(bytes)) {
    throw new Error(`${path}: WebAssembly.validate rejected the module.`);
}
const module = await WebAssembly.compile(bytes);
if (WebAssembly.Module.imports(module).length !== 0) {
    throw new Error(`${path}: the module imports a host capability.`);
}
const instance = await WebAssembly.instantiate(module);
const exports = instance.exports;
const memory = exports["Windvale.memory"];
if (
    exports["Windvale.abi"]?.value !== 3 ||
    !(memory instanceof WebAssembly.Memory) ||
    memory.buffer.byteLength !== 129 * 65_536
) {
    throw new Error(`${path}: the fixed ABI 3 memory contract is invalid.`);
}

const input = Uint8Array.from({ length: 1_024 }, (_, index) => index & 255);
const linearMemory = new Uint8Array(memory.buffer);
const inputOffset = exports["Windvale.input_offset"].value;
const outputOffset = exports["Windvale.output_offset"].value;
const runs = [];
for (const budget of [393_239, 393_238, 393_239]) {
    linearMemory.set(input, inputOffset);
    const status = exports["Windvale.run"](budget, input.length);
    const instructions = exports["Windvale.instructions"].value >>> 0;
    const outputLength = exports["Windvale.output_length"].value;
    const output = linearMemory.slice(outputOffset, outputOffset + outputLength);
    const success = budget === 393_239;
    if (
        status !== (success ? 0 : 3011) ||
        instructions !== budget ||
        outputLength !== (success ? input.length : 0) ||
        (success && !output.every((value, index) => value === input[index]))
    ) {
        throw new Error(
            `${path}: expected budget/status/instructions/length ` +
                `${budget}/${success ? 0 : 3011}/${budget}/` +
                `${success ? input.length : 0}, found ` +
                `${budget}/${status}/${instructions}/${outputLength}.`,
        );
    }
    runs.push({ budget, status, instructions, outputLength });
}

console.log(JSON.stringify({
    bytes: bytes.length,
    sha256: createHash("sha256").update(bytes).digest("hex"),
    inputLength: input.length,
    callCount: 8_192,
    cumulativeConstructedBytes: input.length * 8_192,
    runs,
}, null, 2));
