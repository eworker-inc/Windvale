import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

if (process.argv.length !== 5) {
    throw new Error(
        "Usage: node Probe-WebAssembly-Guest-Heap.mjs " +
            "<scalar-interpreter.wasm> <pressure.wvb> <reset.wvb>",
    );
}

function sha256(bytes) {
    return createHash("sha256").update(bytes).digest("hex");
}

function scalarRequest(candidate, budget) {
    const request = Buffer.alloc(16 + candidate.length);
    request.writeUInt32LE(0x49585657, 0);
    request.writeUInt16LE(1, 4);
    request.writeUInt16LE(0, 6);
    request.writeUInt32LE(budget, 8);
    request.writeUInt32LE(64, 12);
    candidate.copy(request, 16);
    return request;
}

const interpreterPath = process.argv[2];
const pressurePath = process.argv[3];
const resetPath = process.argv[4];
const interpreter = readFileSync(interpreterPath);
const pressure = readFileSync(pressurePath);
const reset = readFileSync(resetPath);
if (!WebAssembly.validate(interpreter)) {
    throw new Error(`${interpreterPath}: WebAssembly.validate rejected the module.`);
}
const module = await WebAssembly.compile(interpreter);
if (WebAssembly.Module.imports(module).length !== 0) {
    throw new Error(`${interpreterPath}: the interpreter imports a host capability.`);
}
const instance = await WebAssembly.instantiate(module);
const exports = instance.exports;
const memory = exports["Windvale.memory"];
if (
    exports["Windvale.abi"]?.value !== 3 ||
    !(memory instanceof WebAssembly.Memory) ||
    memory.buffer.byteLength !== 129 * 65_536
) {
    throw new Error(`${interpreterPath}: the fixed ABI 3 memory contract is invalid.`);
}
const inputOffset = exports["Windvale.input_offset"].value;
const inputCapacity = exports["Windvale.input_capacity"].value;
const outputOffset = exports["Windvale.output_offset"].value;
const outputCapacity = exports["Windvale.output_capacity"].value;
const linearMemory = new Uint8Array(memory.buffer);

function run(candidate, guestBudget, outerBudget) {
    const request = scalarRequest(candidate, guestBudget);
    if (request.length > inputCapacity) {
        throw new Error(`The ${request.length}-byte request exceeds input capacity.`);
    }
    linearMemory.set(request, inputOffset);
    const started = performance.now();
    const outerStatus = exports["Windvale.run"](outerBudget, request.length);
    const outerInstructions = exports["Windvale.instructions"].value >>> 0;
    const outputLength = exports["Windvale.output_length"].value;
    if (outputLength < 0 || outputLength > outputCapacity) {
        throw new Error("The output length is outside the ABI region.");
    }
    const output = Buffer.from(
        linearMemory.slice(outputOffset, outputOffset + outputLength),
    );
    return {
        outerStatus,
        outerInstructions,
        outputLength,
        guestStatus: outputLength >= 20 ? output.readUInt32LE(8) : null,
        guestInstructions: outputLength >= 20 ? output.readUInt32LE(12) : null,
        result: outputLength >= 20 ? output.readInt32LE(16) : null,
        elapsedMilliseconds: Math.round(performance.now() - started),
        validEnvelope:
            outputLength === 20 &&
            output.readUInt32LE(0) === 0x4F585657 &&
            output.readUInt16LE(4) === 1 &&
            output.readUInt16LE(6) === 0,
    };
}

const pressureRun = run(pressure, 205_032, 500_000_000);
if (
    !pressureRun.validEnvelope ||
    pressureRun.outerStatus !== 0 ||
    pressureRun.guestStatus !== 0 ||
    pressureRun.guestInstructions !== 205_032 ||
    pressureRun.result !== 4_099
) {
    throw new Error(`Guest-heap pressure failed: ${JSON.stringify(pressureRun)}`);
}

const resetRun = run(reset, 1_000, 10_000_000);
if (
    !resetRun.validEnvelope ||
    resetRun.outerStatus !== 0 ||
    resetRun.guestStatus !== 0 ||
    resetRun.guestInstructions !== 351 ||
    resetRun.result !== 42
) {
    throw new Error(`Post-pressure reset failed: ${JSON.stringify(resetRun)}`);
}

console.log(JSON.stringify({
    interpreter: {
        bytes: interpreter.length,
        sha256: sha256(interpreter),
    },
    pressure: {
        bytes: pressure.length,
        sha256: sha256(pressure),
        cumulativeConstructedBytes: 65_604,
        ...pressureRun,
    },
    reset: {
        bytes: reset.length,
        sha256: sha256(reset),
        ...resetRun,
    },
}, null, 2));
