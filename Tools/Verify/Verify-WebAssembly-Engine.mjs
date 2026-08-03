import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

const EXPECTED = [
    {
        name: "checked addition success",
        path: process.argv[2],
        sha256: "4057797732dd7250413f44aa71e012222591ae7e219e27a7680f246b2cedeb8a",
        bytes: 176,
        status: 0,
        result: 2147483647,
        instructions: 10,
        abi: 1,
    },
    {
        name: "checked addition overflow",
        path: process.argv[3],
        sha256: "984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52",
        bytes: 176,
        status: 3007,
        result: 0,
        instructions: 7,
        abi: 1,
    },
    {
        name: "straight-line i32 success",
        path: process.argv[4],
        sha256: "15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb",
        bytes: 432,
        status: 0,
        result: 42,
        instructions: 30,
        abi: 1,
    },
    {
        name: "checked subtraction overflow",
        path: process.argv[5],
        sha256: "757d26c2cf404cabcf5b78d2c998bc7ddc78ec4531e4571630ae2c1b5c8d7925",
        bytes: 268,
        status: 3007,
        result: 0,
        instructions: 10,
        abi: 1,
    },
    {
        name: "checked multiplication overflow",
        path: process.argv[6],
        sha256: "e924c7507a363a7b019935622abfbd4bf4ac8445cd37a0412130ce8e5c83d51a",
        bytes: 224,
        status: 3007,
        result: 0,
        instructions: 7,
        abi: 1,
    },
    {
        name: "checked negation overflow",
        path: process.argv[7],
        sha256: "3f098efd63c68d8c62a4f6b373507e12c21808ff01120d165c9dc85a047e99e2",
        bytes: 307,
        status: 3007,
        result: 0,
        instructions: 13,
        abi: 1,
    },
    {
        name: "metered loop",
        path: process.argv[8],
        sha256: "1c429ca20faa42b5018ea565ad10f148792dfbf6a8ecd438cf990cd60d664afe",
        bytes: 972,
        abi: 2,
        runs: [
            { budget: 157, status: 0, result: 42, instructions: 157 },
            { budget: 156, status: 3011, result: 0, instructions: 156 },
            { budget: 157, status: 0, result: 42, instructions: 157 },
        ],
    },
    {
        name: "nonterminating loop containment",
        path: process.argv[9],
        sha256: "325b6f8c9f8d7e2557f93c412aa85b913295dc4bfda5fbb32fb2337915109fde",
        bytes: 663,
        abi: 2,
        runs: [
            { budget: 50, status: 3011, result: 0, instructions: 50 },
        ],
    },
    {
        name: "sequential structured control",
        path: process.argv[10],
        sha256: "454e8af4f739ede63e0b2d55b8907f6075fec1495a4123df53ef5ebcf3ea2c4b",
        bytes: 1923,
        abi: 2,
        runs: [
            { budget: 184, status: 0, result: 42, instructions: 184 },
            { budget: 183, status: 3011, result: 0, instructions: 183 },
            { budget: 184, status: 0, result: 42, instructions: 184 },
        ],
    },
    {
        name: "sequential structured control else route",
        path: process.argv[11],
        sha256: "242116d69f8c28acf4886b1210ffd2b75e622ce92b44586a8a1668188930a84b",
        bytes: 1770,
        abi: 2,
        runs: [
            { budget: 331, status: 0, result: 42, instructions: 331 },
            { budget: 330, status: 3011, result: 0, instructions: 330 },
            { budget: 331, status: 0, result: 42, instructions: 331 },
        ],
    },
    {
        name: "sequential if control",
        path: process.argv[12],
        sha256: "d4fd2bf65a6b4aebf55aaf033e86984a4e882761a4c9a59d85bd7ca8353a21ba",
        bytes: 1164,
        abi: 2,
        runs: [
            { budget: 41, status: 0, result: 42, instructions: 41 },
            { budget: 40, status: 3011, result: 0, instructions: 40 },
            { budget: 41, status: 0, result: 42, instructions: 41 },
        ],
    },
    {
        name: "bounded direct calls",
        path: process.argv[13],
        sha256: "d92667752762a992bdb626e34b83b78ee9c531f167b911737dfbf5f6443f3518",
        bytes: 1185,
        abi: 2,
        runs: [
            { budget: 66, status: 0, result: 42, instructions: 66 },
            { budget: 65, status: 3011, result: 0, instructions: 65 },
            { budget: 66, status: 0, result: 42, instructions: 66 },
        ],
    },
    {
        name: "bounded direct-call overflow",
        path: process.argv[14],
        sha256: "4e936e5c4b077d1bce8719f5cc5c974961088f1171ed00158f9ac251f7652bd7",
        bytes: 737,
        abi: 2,
        runs: [
            { budget: 100, status: 3007, result: 0, instructions: 14 },
            { budget: 100, status: 3007, result: 0, instructions: 14 },
        ],
    },
    {
        name: "bounded calls with structured control",
        path: process.argv[15],
        sha256: "3be50be3c2436638973eb68743f9fdd2e00df9816e50e498b432ff36468c3a77",
        bytes: 2729,
        abi: 2,
        runs: [
            { budget: 196, status: 0, result: 42, instructions: 196 },
            { budget: 195, status: 3011, result: 0, instructions: 195 },
            { budget: 196, status: 0, result: 42, instructions: 196 },
        ],
    },
    {
        name: "bounded calls with structured-control else route",
        path: process.argv[16],
        sha256: "35d75c30ef03dbb693a976cfaa31405ce90ecca4d393c5e93de8953fcf4658da",
        bytes: 2729,
        abi: 2,
        runs: [
            { budget: 153, status: 0, result: 42, instructions: 153 },
            { budget: 152, status: 3011, result: 0, instructions: 152 },
            { budget: 153, status: 0, result: 42, instructions: 153 },
        ],
    },
    {
        name: "linear-memory bytes identity",
        path: process.argv[17],
        sha256: "b5f87bd47be7a0ce0bb6755de4ecea8bc311c9412ee28d6091092e7aa4c184f5",
        bytes: 435,
        abi: 3,
        kind: 1,
    },
    {
        name: "linear-memory UTF-8 text identity",
        path: process.argv[18],
        sha256: "c3635b8df4ed9d471faad7e653e975662099c0a2336639586915ce50b768542d",
        bytes: 791,
        abi: 3,
        kind: 2,
    },
];

if (process.argv.length !== 19) {
    throw new Error(
        "Usage: node Verify-WebAssembly-Engine.mjs " +
            "<add-success.wasm> <add-overflow.wasm> <straight-i32.wasm> " +
            "<subtract-overflow.wasm> <multiply-overflow.wasm> <negate-overflow.wasm> " +
            "<metered-loop.wasm> <nonterminating-loop.wasm> " +
            "<structured-control.wasm> <structured-control-else.wasm> " +
            "<sequential-if.wasm> <bounded-calls.wasm> <bounded-calls-overflow.wasm> " +
            "<calls-with-control.wasm> <calls-with-control-else.wasm> " +
            "<memory-bytes.wasm> <memory-text.wasm>",
    );
}

const MEMORY_EXPORTS = [
    "Windvale.run",
    "Windvale.abi",
    "Windvale.memory",
    "Windvale.input_offset",
    "Windvale.input_capacity",
    "Windvale.output_offset",
    "Windvale.output_capacity",
    "Windvale.output_length",
    "Windvale.output_kind",
    "Windvale.instructions",
];

function runMemory(exports, input, budget = 4) {
    const inputOffset = exports["Windvale.input_offset"].value;
    const outputOffset = exports["Windvale.output_offset"].value;
    const memory = exports["Windvale.memory"];
    new Uint8Array(memory.buffer, inputOffset, input.length).set(input);
    exports["Windvale.output_length"].value = -123;
    exports["Windvale.instructions"].value = 99;
    const status = exports["Windvale.run"](budget, input.length);
    const instructions = exports["Windvale.instructions"].value;
    const outputLength = exports["Windvale.output_length"].value;
    const output = new Uint8Array(memory.buffer, outputOffset, outputLength).slice();
    return { status, instructions, outputLength, output };
}

function requireMemoryResult(path, actual, expectedStatus, expectedInstructions, expected) {
    if (
        actual.status !== expectedStatus ||
        actual.instructions !== expectedInstructions ||
        actual.outputLength !== (expected?.length ?? 0)
    ) {
        throw new Error(
            `${path}: expected memory status/instructions/length ` +
                `${expectedStatus}/${expectedInstructions}/${expected?.length ?? 0}, found ` +
                `${actual.status}/${actual.instructions}/${actual.outputLength}.`,
        );
    }
    if (expected && !Buffer.from(actual.output).equals(Buffer.from(expected))) {
        throw new Error(`${path}: the memory output bytes differ from the input.`);
    }
}

function verifyMemory(expected, module, exports, digest) {
    if (WebAssembly.Module.imports(module).length !== 0) {
        throw new Error(`${expected.path}: the memory module unexpectedly imports a host capability.`);
    }
    const names = Object.keys(exports);
    if (
        names.length !== MEMORY_EXPORTS.length ||
        names.some((name, index) => name !== MEMORY_EXPORTS[index])
    ) {
        throw new Error(`${expected.path}: the memory ABI export set or order is invalid.`);
    }
    const memory = exports["Windvale.memory"];
    if (!(memory instanceof WebAssembly.Memory) || memory.buffer.byteLength !== 129 * 65_536) {
        throw new Error(`${expected.path}: the fixed linear memory extent is invalid.`);
    }
    if (
        exports["Windvale.input_offset"].value !== 65_536 ||
        exports["Windvale.input_capacity"].value !== 4_194_304 ||
        exports["Windvale.output_offset"].value !== 4_259_840 ||
        exports["Windvale.output_capacity"].value !== 4_194_304 ||
        exports["Windvale.output_kind"].value !== expected.kind
    ) {
        throw new Error(`${expected.path}: the memory ABI region globals are invalid.`);
    }
    let growthRejected = false;
    try {
        memory.grow(1);
    } catch (error) {
        growthRejected = error instanceof RangeError;
    }
    if (!growthRejected || memory.buffer.byteLength !== 129 * 65_536) {
        throw new Error(`${expected.path}: the fixed memory unexpectedly grew.`);
    }

    const ordinary = expected.kind === 1
        ? Uint8Array.from([0x00, 0xFF, 0x01, 0x02, 0x03, 0x80, 0x40])
        : new TextEncoder().encode("Hello, 世界 🌬️");
    requireMemoryResult(expected.path, runMemory(exports, ordinary), 0, 4, ordinary);
    requireMemoryResult(expected.path, runMemory(exports, ordinary, 3), 3011, 3);

    const capacity = exports["Windvale.input_capacity"].value;
    const boundary = new Uint8Array(capacity);
    if (expected.kind === 1) {
        for (let index = 0; index < boundary.length; index++) {
            boundary[index] = (index * 31 + 17) & 0xFF;
        }
    } else {
        boundary.fill(0x61);
    }
    requireMemoryResult(expected.path, runMemory(exports, boundary), 0, 4, boundary);
    requireMemoryResult(
        expected.path,
        {
            status: exports["Windvale.run"](4, capacity + 1),
            instructions: exports["Windvale.instructions"].value,
            outputLength: exports["Windvale.output_length"].value,
            output: new Uint8Array(),
        },
        3008,
        0,
    );
    requireMemoryResult(
        expected.path,
        {
            status: exports["Windvale.run"](4, -1),
            instructions: exports["Windvale.instructions"].value,
            outputLength: exports["Windvale.output_length"].value,
            output: new Uint8Array(),
        },
        3008,
        0,
    );

    if (expected.kind === 2) {
        const valid = [
            [],
            [0x00, 0x7F],
            [0xC2, 0x80],
            [0xDF, 0xBF],
            [0xE0, 0xA0, 0x80],
            [0xED, 0x9F, 0xBF],
            [0xEE, 0x80, 0x80],
            [0xEF, 0xBF, 0xBF],
            [0xF0, 0x90, 0x80, 0x80],
            [0xF4, 0x8F, 0xBF, 0xBF],
        ];
        const invalid = [
            [0x80],
            [0xC0, 0x80],
            [0xC1, 0xBF],
            [0xC2],
            [0xC2, 0x7F],
            [0xE0, 0x9F, 0xBF],
            [0xED, 0xA0, 0x80],
            [0xE1, 0x80],
            [0xF0, 0x8F, 0xBF, 0xBF],
            [0xF4, 0x90, 0x80, 0x80],
            [0xF5, 0x80, 0x80, 0x80],
            [0xFF],
        ];
        for (const values of valid) {
            const input = Uint8Array.from(values);
            requireMemoryResult(expected.path, runMemory(exports, input), 0, 4, input);
        }
        for (const values of invalid) {
            requireMemoryResult(
                expected.path,
                runMemory(exports, Uint8Array.from(values)),
                3014,
                0,
            );
        }

        const decoder = new TextDecoder("utf-8", { fatal: true, ignoreBOM: true });
        let state = 0x6D2B79F5;
        for (let sample = 0; sample < 20_000; sample++) {
            state = (Math.imul(state, 1_664_525) + 1_013_904_223) >>> 0;
            const input = new Uint8Array(state % 9);
            for (let index = 0; index < input.length; index++) {
                state = (Math.imul(state, 1_664_525) + 1_013_904_223) >>> 0;
                input[index] = state >>> 24;
            }
            let validUtf8 = true;
            try {
                decoder.decode(input);
            } catch {
                validUtf8 = false;
            }
            const result = runMemory(exports, input);
            requireMemoryResult(
                expected.path,
                result,
                validUtf8 ? 0 : 3014,
                validUtf8 ? 4 : 0,
                validUtf8 ? input : undefined,
            );
        }
    }

    console.log(
        `${expected.path}: ${expected.name}; ABI 3 kind=${expected.kind} ` +
            `capacity=${capacity} status=0 instructions=4 SHA-256=${digest}`,
    );
}

for (const expected of EXPECTED) {
    const bytes = readFileSync(expected.path);
    const digest = createHash("sha256").update(bytes).digest("hex");
    if (digest !== expected.sha256) {
        throw new Error(
            `${expected.path}: expected SHA-256 ${expected.sha256}, found ${digest}.`,
        );
    }
    if (bytes.length !== expected.bytes) {
        throw new Error(
            `${expected.path}: expected ${expected.bytes} bytes, found ${bytes.length}.`,
        );
    }
    if (!WebAssembly.validate(bytes)) {
        throw new Error(`${expected.path}: WebAssembly.validate rejected the module.`);
    }

    const module = await WebAssembly.compile(bytes);
    const instance = await WebAssembly.instantiate(module);
    const exports = instance.exports;
    if (exports["Windvale.abi"].value !== expected.abi) {
        throw new Error(`${expected.path}: execution ABI is not ${expected.abi}.`);
    }
    if (expected.abi === 3) {
        verifyMemory(expected, module, exports, digest);
        continue;
    }

    const runs = expected.runs ?? [{
        status: expected.status,
        result: expected.result,
        instructions: expected.instructions,
    }];
    for (const run of runs) {
        exports["Windvale.result"].value = -123;
        exports["Windvale.instructions"].value = 99;
        const status = expected.abi === 2
            ? exports["Windvale.run"](run.budget)
            : exports["Windvale.run"]();
        const result = exports["Windvale.result"].value;
        const instructions = exports["Windvale.instructions"].value;
        if (
            status !== run.status ||
            result !== run.result ||
            instructions !== run.instructions
        ) {
            throw new Error(
                `${expected.path}: expected status/result/instructions ` +
                    `${run.status}/${run.result}/${run.instructions}, found ` +
                    `${status}/${result}/${instructions}.`,
            );
        }
        console.log(
            `${expected.path}: ${expected.name}; ABI ${expected.abi} ` +
                `budget=${run.budget ?? "static"} status=${status} result=${result} ` +
                `instructions=${instructions} SHA-256=${digest}`,
        );
    }
}

console.log(`WebAssembly engine verification passed under Node.js ${process.version}.`);
