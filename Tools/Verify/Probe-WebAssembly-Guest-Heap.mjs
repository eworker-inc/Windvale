import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

if (process.argv.length !== 8) {
    throw new Error(
        "Usage: node Probe-WebAssembly-Guest-Heap.mjs " +
            "<scalar-interpreter.wasm> <ownership-pressure.wvb> " +
            "<text-bytes.wvb> <formatting-quote.wvb> <sha256.wvb> <reset.wvb>",
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
const textBytesPath = process.argv[4];
const formattingPath = process.argv[5];
const sha256Path = process.argv[6];
const resetPath = process.argv[7];
const interpreter = readFileSync(interpreterPath);
const pressure = readFileSync(pressurePath);
const textBytes = readFileSync(textBytesPath);
const formatting = readFileSync(formattingPath);
const sha256Candidate = readFileSync(sha256Path);
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

const pressureRun = run(pressure, 15_627, 100_000_000);
if (
    !pressureRun.validEnvelope ||
    pressureRun.outerStatus !== 0 ||
    pressureRun.guestStatus !== 0 ||
    pressureRun.guestInstructions !== 15_627 ||
    pressureRun.result !== 69
) {
    throw new Error(`Guest-heap pressure failed: ${JSON.stringify(pressureRun)}`);
}

const textBytesRun = run(textBytes, 4_096, 10_000_000);
if (
    !textBytesRun.validEnvelope ||
    textBytesRun.outerStatus !== 0 ||
    textBytesRun.guestStatus !== 0 ||
    textBytesRun.guestInstructions !== 298 ||
    textBytesRun.result !== 42
) {
    throw new Error(`Text/bytes run failed: ${JSON.stringify(textBytesRun)}`);
}

const formattingRun = run(formatting, 4_096, 20_000_000);
if (
    !formattingRun.validEnvelope ||
    formattingRun.outerStatus !== 0 ||
    formattingRun.guestStatus !== 0 ||
    formattingRun.guestInstructions !== 4_070 ||
    formattingRun.result !== 42
) {
    throw new Error(`Formatting run failed: ${JSON.stringify(formattingRun)}`);
}

const sha256Run = run(sha256Candidate, 4_096, 20_000_000);
if (
    !sha256Run.validEnvelope ||
    sha256Run.outerStatus !== 0 ||
    sha256Run.guestStatus !== 0 ||
    sha256Run.guestInstructions !== 3_996 ||
    sha256Run.result !== 42
) {
    throw new Error(`SHA-256 run failed: ${JSON.stringify(sha256Run)}`);
}

const oneShortRun = run(reset, 350, 10_000_000);
if (
    !oneShortRun.validEnvelope ||
    oneShortRun.outerStatus !== 0 ||
    oneShortRun.guestStatus !== 3_011 ||
    oneShortRun.guestInstructions !== 350 ||
    oneShortRun.result !== 0
) {
    throw new Error(
        `Guest one-short run failed: ${JSON.stringify(oneShortRun)}`,
    );
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
        cumulativeConstructedBytes: 143_364,
        cumulativeRecordFieldCells: 1_136,
        ...pressureRun,
    },
    textBytes: textBytesRun,
    formatting: formattingRun,
    sha256: sha256Run,
    oneShort: oneShortRun,
    reset: {
        bytes: reset.length,
        sha256: sha256(reset),
        ...resetRun,
    },
}, null, 2));
