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
    {
        name: "linear-memory runtime values",
        path: process.argv[19],
        sha256: "ac289c87198435c41b4033c9fa4a159c69a865871eb4996cde752b2c3a362e9a",
        bytes: 6182,
        abi: 3,
        kind: 1,
        runtime: "values",
    },
    {
        name: "linear-memory bounded concatenation",
        path: process.argv[20],
        sha256: "94640304376bfcea97719de65d6e32ed085fa77ba24fc2408e1d0f4aca940d8d",
        bytes: 1461,
        abi: 3,
        kind: 1,
        runtime: "concat",
    },
    {
        name: "linear-memory u16 construction guard",
        path: process.argv[21],
        sha256: "94e6b0300df16ee11b76c917575b0795519776969fdbb4c043893cbc2c8668c2",
        bytes: 1497,
        abi: 3,
        kind: 1,
        runtime: "u16",
    },
    {
        name: "linear-memory aggregate arena guard",
        path: process.argv[22],
        sha256: "b7ed2efd6ac4946b53582a40c54eae47e6e53e6a887fd52d0f9b03fb99258b08",
        bytes: 1677,
        abi: 3,
        kind: 1,
        runtime: "arena",
    },
    {
        name: "linear-memory checked u32 arithmetic",
        path: process.argv[23],
        sha256: "1fbff62758d8747365865c71615ece0eaa26ad682d3cef96a48cc30df4261914",
        bytes: 2665,
        abi: 3,
        kind: 1,
        runtime: "u32",
    },
    {
        name: "Windvale-native WVB envelope verifier",
        path: process.argv[24],
        inputPath: process.argv[25],
        sha256: "01aab009fa7cf2a6e1b57d6fbf1e78b3caf6d3fb1c19e99db0f9c9d909d0cd58",
        bytes: 16112,
        abi: 3,
        kind: 1,
        runtime: "wvb-envelope",
    },
    {
        name: "Windvale-native WVB structural verifier",
        path: process.argv[26],
        inputPath: process.argv[27],
        acceptedInputPaths: [process.argv[28], process.argv[29], process.argv[30]],
        sha256: "e6fe4991a44350121ddcefae6235fa52c9a39a0f25a11fefcdf02c4f9fc9326e",
        bytes: 120043,
        abi: 3,
        kind: 1,
        runtime: "wvb-structural",
    },
    {
        name: "linear-memory calls with structured control",
        path: process.argv[31],
        sha256: "5ee04d5b3b33399dce61709135709f0d0ebb7d6374e14759d83986859806eadd",
        bytes: 4086,
        abi: 3,
        kind: 1,
        runtime: "calls",
    },
    {
        name: "Windvale-native WVB canonical metadata and reference verifier",
        path: process.argv[32],
        acceptedInputPaths: [process.argv[33], process.argv[34], process.argv[35]],
        sha256: "78c8c7bd43b2036d336df956f693a1421a93a6bd55d6e2fda5afcc9c8df412e0",
        bytes: 440583,
        abi: 3,
        kind: 1,
        runtime: "wvb-semantic",
    },
    {
        name: "expanded Windvale-native WVB semantic-verifier call graph",
        path: process.argv[36],
        acceptedInputPaths: [process.argv[37], process.argv[38], process.argv[39]],
        sha256: "c616ba9bd4b3bd96a546546cdad71d4a1fd4cb38e5de6f4e6acbcc46a4ed9331",
        bytes: 440823,
        abi: 3,
        kind: 1,
        runtime: "wvb-semantic-expanded",
    },
    {
        name: "Windvale-native WVB executable verifier",
        path: process.argv[40],
        acceptedInputPaths: [process.argv[41], process.argv[42], process.argv[43]],
        sha256: "c9249bd45a6ea7dcb14a11d1fbcf6dd004f6ce2bcf9eb4794ad65e2ba79a00fd",
        bytes: 723327,
        abi: 3,
        kind: 1,
        runtime: "wvb-executable",
    },
    {
        name: "Wasm-hosted Windvale WVB scalar/text/bytes/record/enum interpreter",
        path: process.argv[44],
        verifierPath: process.argv[40],
        candidatePaths: [
            process.argv[45], process.argv[46], process.argv[47], process.argv[48],
            process.argv[49], process.argv[50], process.argv[51], process.argv[52],
            process.argv[53], process.argv[54], process.argv[55], process.argv[56],
            process.argv[41], process.argv[57], process.argv[42],
            process.argv[58], process.argv[59],
        ],
        bytesEntryPath: process.argv[64],
        portableCompilerPath: process.argv[65],
        portableSourcePath: process.argv[66],
        sha256: "dbcb971cb1dedac2169035d0cf436aaed9cc5abcce0a9347932c8e0b7d1bff1e",
        bytes: 468320,
        abi: 3,
        kind: 1,
        runtime: "wvb-scalar-interpreter",
    },
    {
        name: "compiler-capacity Windvale WVB verifier bundle",
        path: process.argv[60],
        typedPath: process.argv[61],
        controlPath: process.argv[62],
        inputPath: process.argv[63],
        portableInputPath: process.argv[65],
        sha256: "d61fe8f1091429d64ba425670ad4d85608f1efcc8bc71ad8904f3a7691ded677",
        typedSha256: "6fc1e02498de6345b441d0f34e52fe9d0c014642fc637f9066623b07b4329240",
        controlSha256: "597c0f8313aac9fbcb1ba50fbcd4e25937f0cc464d090d491570f8eeb559253d",
        bytes: 440583,
        typedBytes: 282718,
        controlBytes: 282718,
        abi: 3,
        kind: 1,
        runtime: "wvb-compiler-verifier",
    },
    {
        name: "linear-memory reclaiming allocator stress",
        path: process.argv[67],
        sha256: "5a89412a9f48e883a027da497406747f1c31c8eb0e6533f7103e52a078a8827a",
        bytes: 2399,
        abi: 3,
        kind: 1,
        runtime: "reclaim",
    },
];

if (process.argv.length !== 68) {
    throw new Error(
        "Usage: node Verify-WebAssembly-Engine.mjs " +
            "<add-success.wasm> <add-overflow.wasm> <straight-i32.wasm> " +
            "<subtract-overflow.wasm> <multiply-overflow.wasm> <negate-overflow.wasm> " +
            "<metered-loop.wasm> <nonterminating-loop.wasm> " +
            "<structured-control.wasm> <structured-control-else.wasm> " +
            "<sequential-if.wasm> <bounded-calls.wasm> <bounded-calls-overflow.wasm> " +
            "<calls-with-control.wasm> <calls-with-control-else.wasm> " +
            "<memory-bytes.wasm> <memory-text.wasm> <runtime-values.wasm> " +
            "<runtime-concat.wasm> <runtime-u16.wasm> <runtime-arena.wasm> " +
            "<runtime-u32.wasm> <wvb-envelope-verifier.wasm> " +
            "<wvb-envelope-verifier.wvb> <wvb-structural-verifier.wasm> " +
            "<wvb-structural-verifier.wvb> <data.wvb> <types.wvb> <capabilities.wvb> " +
            "<runtime-calls.wasm> <wvb-semantic-verifier.wasm> " +
            "<semantic-data.wvb> <semantic-types.wvb> <semantic-capabilities.wvb> " +
            "<wvb-semantic-expanded.wasm> <expanded-data.wvb> " +
            "<expanded-types.wvb> <expanded-capabilities.wvb> " +
            "<wvb-executable.wasm> <executable-data.wvb> " +
            "<executable-types.wvb> <executable-capabilities.wvb> " +
            "<wvb-scalar-interpreter.wasm> <function-only.wvb> " +
            "<scalar-guest.wvb> <i32-overflow.wvb> <u32-overflow.wvb> " +
            "<text-bytes-guest.wvb> <utf8-boundaries.wvb> <invalid-utf8.wvb> " +
            "<range-failure.wvb> <u16-failure.wvb> <value-failure.wvb> " +
            "<heap-failure.wvb> <formatting-quote.wvb> <sha256.wvb> " +
            "<nominal-defaults.wvb> <record-arena-failure.wvb> " +
            "<compiler-semantic-verifier.wasm> <compiler-typed-verifier.wasm> " +
            "<compiler-control-verifier.wasm> <windvale-compiler.wvb> " +
            "<bytes-entry-guest.wvb> <windvale-compiler-memory.wvb> " +
            "<function-only.wv> <runtime-reclaim.wasm>",
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
    const instructions = exports["Windvale.instructions"].value >>> 0;
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

function verifyMemoryContract(expected, module, exports) {
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
}

function verifyMemory(expected, module, exports, digest) {
    verifyMemoryContract(expected, module, exports);
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

function verifyRuntime(expected, module, exports, digest) {
    verifyMemoryContract(expected, module, exports);
    const capacity = exports["Windvale.input_capacity"].value;
    const ordinary = Uint8Array.from([0xAA, 0x34, 0x12, 0x78, 0x56, 0x34, 0x12]);

    if (expected.runtime === "values") {
        const output = Uint8Array.from([
            0x07, 0x00, 0x00, 0x00,
            0xAA,
            0xAA, 0x00, 0x00, 0x00,
            0x34, 0x12,
            0x34, 0x12, 0x78, 0x56,
            0x78, 0x56, 0x34, 0x12,
        ]);
        requireMemoryResult(expected.path, runMemory(exports, ordinary, 155), 0, 155, output);
        requireMemoryResult(expected.path, runMemory(exports, ordinary, 154), 3011, 154);
        requireMemoryResult(expected.path, runMemory(exports, new Uint8Array(), 1_000), 3008, 26);
    } else if (expected.runtime === "concat") {
        const small = Uint8Array.from([1, 2, 3]);
        requireMemoryResult(
            expected.path,
            runMemory(exports, small, 10),
            0,
            10,
            Uint8Array.from([1, 2, 3, 1, 2, 3]),
        );
        requireMemoryResult(expected.path, runMemory(exports, small, 9), 3011, 9);

        const boundary = new Uint8Array(2_097_152);
        const boundaryOutput = new Uint8Array(4_194_304);
        requireMemoryResult(
            expected.path,
            runMemory(exports, boundary, 10),
            0,
            10,
            boundaryOutput,
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, new Uint8Array(2_097_153), 10),
            3015,
            7,
        );
    } else if (expected.runtime === "u16") {
        const small = Uint8Array.from([0x34, 0x12, 0x00, 0x00]);
        requireMemoryResult(
            expected.path,
            runMemory(exports, small, 13),
            0,
            13,
            Uint8Array.from([0x34, 0x12]),
        );
        requireMemoryResult(expected.path, runMemory(exports, small, 12), 3011, 12);
        requireMemoryResult(
            expected.path,
            runMemory(exports, Uint8Array.from([0x00, 0x00, 0x01, 0x00]), 13),
            3016,
            10,
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, Uint8Array.from([1, 2, 3]), 13),
            3008,
            7,
        );
    } else if (expected.runtime === "arena") {
        const small = Uint8Array.from([2, 3]);
        requireMemoryResult(
            expected.path,
            runMemory(exports, small, 17),
            0,
            17,
            Uint8Array.from([1, 2, 3]),
        );
        requireMemoryResult(expected.path, runMemory(exports, small, 16), 3011, 16);
        requireMemoryResult(
            expected.path,
            runMemory(exports, new Uint8Array(capacity - 1), 17),
            3018,
            14,
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, new Uint8Array(capacity), 17),
            3015,
            14,
        );
    } else if (expected.runtime === "reclaim") {
        const input = Uint8Array.from({ length: 1_024 }, (_, index) => index & 255);
        const output = new Uint8Array(2_048);
        output.set(input);
        output.set(input, input.length);
        requireMemoryResult(
            expected.path,
            runMemory(exports, input, 262_167),
            0,
            262_167,
            output,
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, input, 262_166),
            3011,
            262_166,
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, input, 262_167),
            0,
            262_167,
            output,
        );
    } else if (expected.runtime === "u32") {
        requireMemoryResult(
            expected.path,
            runMemory(
                exports,
                Uint8Array.from([42, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0]),
                57,
            ),
            0,
            57,
            Uint8Array.from([42, 0, 0, 0]),
        );
        requireMemoryResult(
            expected.path,
            runMemory(
                exports,
                Uint8Array.from([255, 255, 255, 255, 1, 0, 0, 0, 0, 0, 0, 0]),
                57,
            ),
            3007,
            37,
        );
        requireMemoryResult(
            expected.path,
            runMemory(
                exports,
                Uint8Array.from([0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0]),
                57,
            ),
            3007,
            47,
        );
        requireMemoryResult(expected.path, runMemory(exports, Uint8Array.from([1, 2, 3]), 57), 3008, 7);
    } else if (expected.runtime === "calls") {
        const small = Uint8Array.from([9, 8]);
        requireMemoryResult(
            expected.path,
            runMemory(exports, small, 127),
            0,
            127,
            Uint8Array.from([9, 9, 8]),
        );
        requireMemoryResult(expected.path, runMemory(exports, small, 126), 3011, 126);
        requireMemoryResult(
            expected.path,
            runMemory(exports, new Uint8Array(), 63),
            0,
            63,
            Uint8Array.from([0, 0]),
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, small, 127),
            0,
            127,
            Uint8Array.from([9, 9, 8]),
        );
    } else if (expected.runtime === "wvb-envelope") {
        const valid = readFileSync(expected.inputPath);
        requireMemoryResult(
            expected.path,
            runMemory(exports, valid, 2_206),
            0,
            2_206,
            Uint8Array.from([1]),
        );
        requireMemoryResult(expected.path, runMemory(exports, valid, 2_205), 3011, 2_205);

        const badMagic = Buffer.from(valid);
        badMagic[0] ^= 0xFF;
        const hostileLength = Buffer.from(valid);
        hostileLength.writeUInt32LE(0xFFFFFFFF, 16);
        for (const [name, input, instructions] of [
            ["bad magic", badMagic, 112],
            ["truncated header", valid.subarray(0, 11), 92],
            ["trailing byte", Buffer.concat([valid, Buffer.from([0])]), 2_201],
            ["hostile section length", hostileLength, 460],
        ]) {
            const result = runMemory(exports, input, 2_206);
            try {
                requireMemoryResult(
                    expected.path,
                    result,
                    0,
                    instructions,
                    Uint8Array.from([0]),
                );
            } catch (error) {
                throw new Error(`${expected.path}: ${name}: ${error.message}`);
            }
        }
    } else if (expected.runtime === "wvb-structural") {
        const valid = readFileSync(expected.inputPath);
        requireMemoryResult(
            expected.path,
            runMemory(exports, valid, 1_446_276),
            0,
            1_446_276,
            Uint8Array.from([1]),
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, valid, 1_446_275),
            3011,
            1_446_275,
        );

        const acceptedSteps = [103_696, 94_466, 28_803];
        for (let index = 0; index < expected.acceptedInputPaths.length; index++) {
            requireMemoryResult(
                expected.path,
                runMemory(
                    exports,
                    readFileSync(expected.acceptedInputPaths[index]),
                    acceptedSteps[index],
                ),
                0,
                acceptedSteps[index],
                Uint8Array.from([1]),
            );
        }

        function findSections(bytes) {
            const sections = [];
            let cursor = 12;
            for (let kind = 1; kind <= 7; kind++) {
                const length = bytes.readUInt32LE(cursor + 4);
                sections.push({ payload: cursor + 8, length });
                cursor += 8 + length;
            }
            return sections;
        }

        const sections = findSections(valid);
        const malformed = [];
        let value = Buffer.from(valid);
        value[sections[0].payload] = 0;
        malformed.push(["bad module profile", value, 432]);
        value = Buffer.from(valid);
        value.writeUInt32LE(33, sections[1].payload);
        malformed.push(["capability count", value, 834]);
        value = Buffer.from(valid);
        value.writeUInt32LE(4_097, sections[2].payload);
        malformed.push(["data count", value, 1_171]);
        value = Buffer.from(valid);
        value.writeUInt32LE(4_097, sections[3].payload);
        malformed.push(["function count", value, 1_508]);
        value = Buffer.from(valid);
        value[sections[4].payload] = 0xFF;
        malformed.push(["unknown opcode", value, 100_118]);
        value = Buffer.from(valid);
        value.writeUInt32LE(1, sections[5].payload + 13);
        malformed.push(["export target", value, 1_445_857]);
        value = Buffer.from(valid);
        value.writeUInt32LE(1, sections[6].payload);
        malformed.push(["truncated type payload", value, 1_446_254]);
        for (const [name, input, instructions] of malformed) {
            const result = runMemory(exports, input, 1_446_276);
            try {
                requireMemoryResult(
                    expected.path,
                    result,
                    0,
                    instructions,
                    Uint8Array.from([0]),
                );
            } catch (error) {
                throw new Error(`${expected.path}: ${name}: ${error.message}`);
            }
        }
    } else if (expected.runtime === "wvb-semantic-expanded") {
        const acceptedSteps = [1_122_125, 912_991, 113_541];
        for (let index = 0; index < expected.acceptedInputPaths.length; index++) {
            requireMemoryResult(
                expected.path,
                runMemory(
                    exports,
                    readFileSync(expected.acceptedInputPaths[index]),
                    acceptedSteps[index],
                ),
                0,
                acceptedSteps[index],
                Uint8Array.from([1]),
            );
        }
        requireMemoryResult(
            expected.path,
            runMemory(
                exports,
                readFileSync(expected.acceptedInputPaths[0]),
                acceptedSteps[0] - 1,
            ),
            3011,
            acceptedSteps[0] - 1,
        );
    } else if (expected.runtime === "wvb-executable") {
        const acceptedSteps = [4_181_612, 3_250_615, 241_684];
        for (let index = 0; index < expected.acceptedInputPaths.length; index++) {
            requireMemoryResult(
                expected.path,
                runMemory(
                    exports,
                    readFileSync(expected.acceptedInputPaths[index]),
                    acceptedSteps[index],
                ),
                0,
                acceptedSteps[index],
                Uint8Array.from([1]),
            );
        }
        requireMemoryResult(
            expected.path,
            runMemory(
                exports,
                readFileSync(expected.acceptedInputPaths[0]),
                acceptedSteps[0] - 1,
            ),
            3011,
            acceptedSteps[0] - 1,
        );

        const [data, types, capabilities] = expected.acceptedInputPaths.map(path =>
            readFileSync(path));
        function executableSections(bytes) {
            const sections = [];
            let cursor = 12;
            for (let kind = 1; kind <= 7; kind++) {
                const length = bytes.readUInt32LE(cursor + 4);
                sections[kind] = { payload: cursor + 8, length };
                cursor += 8 + length;
            }
            return sections;
        }
        function executableShape(bytes, cursor) {
            const kind = bytes[cursor++];
            let nominal = -1;
            if (kind >= 7) {
                nominal = bytes.readUInt32LE(cursor);
                cursor += 4;
            }
            return { kind, nominal, cursor };
        }
        function executableInstructionWidth(opcode) {
            if (opcode === 2 || opcode === 8) { return 2; }
            if (opcode === 106 || opcode === 128 || opcode === 129) { return 9; }
            if ([1, 3, 4, 5, 6, 7, 9, 10, 48, 49, 64, 65, 104, 105]
                .includes(opcode)) {
                return 5;
            }
            return 1;
        }
        function executableFunctions(bytes) {
            const sections = executableSections(bytes);
            let cursor = sections[4].payload;
            const count = bytes.readUInt32LE(cursor);
            cursor += 4;
            const functions = [];
            for (let functionIndex = 0; functionIndex < count; functionIndex++) {
                const nameLength = bytes.readUInt32LE(cursor);
                cursor += 4;
                const name = bytes.subarray(cursor, cursor + nameLength).toString("utf8");
                cursor += nameLength;
                const parameterCount = bytes.readUInt32LE(cursor);
                cursor += 4;
                const parameters = [];
                for (let index = 0; index < parameterCount; index++) {
                    const shape = executableShape(bytes, cursor);
                    parameters.push(shape);
                    cursor = shape.cursor;
                }
                const result = executableShape(bytes, cursor);
                cursor = result.cursor;
                const localCount = bytes.readUInt32LE(cursor);
                cursor += 4;
                const locals = [];
                for (let index = 0; index < localCount; index++) {
                    const shape = executableShape(bytes, cursor);
                    locals.push(shape);
                    cursor = shape.cursor;
                }
                const codeOffset = bytes.readUInt32LE(cursor);
                const codeLength = bytes.readUInt32LE(cursor + 4);
                const maximumOffset = cursor + 8;
                const maximum = bytes.readUInt32LE(maximumOffset);
                cursor += 12;
                const instructions = [];
                let instructionOffset = 0;
                while (instructionOffset < codeLength) {
                    const absolute = sections[5].payload + codeOffset + instructionOffset;
                    const opcode = bytes[absolute];
                    const width = executableInstructionWidth(opcode);
                    const operand = width >= 5 ? bytes.readUInt32LE(absolute + 1) : 0;
                    instructions.push({ absolute, offset: instructionOffset, opcode, operand });
                    instructionOffset += width;
                }
                functions.push({
                    name,
                    parameters,
                    locals,
                    maximumOffset,
                    maximum,
                    instructions,
                });
            }
            return functions;
        }
        function executableMatch(functionDeclaration, wanted) {
            const index = functionDeclaration.instructions.findIndex(wanted);
            if (index < 0) {
                throw new Error(
                    `${expected.path}: executable input has no requested instruction.`,
                );
            }
            return { index, instruction: functionDeclaration.instructions[index] };
        }

        const nominalFunctions = executableFunctions(types);
        const nominalMain = nominalFunctions.find(item => item.name === "Main");
        const envelopeLocal = nominalMain.parameters.length + nominalMain.locals.findIndex(
            shape => shape.kind === 7 && shape.nominal === 0);
        const i32Local = nominalMain.parameters.length + nominalMain.locals.findIndex(
            shape => shape.kind === 1);
        const signalEnumLocal = nominalMain.parameters.length + nominalMain.locals.findIndex(
            shape => shape.kind === 8 && shape.nominal === 2);
        if (envelopeLocal < 0 || i32Local < 0 || signalEnumLocal < 0) {
            throw new Error(`${expected.path}: executable mutation locals are absent.`);
        }

        const malformed = [];
        function executableCorrupt(name, original, mutate) {
            const value = Buffer.from(original);
            mutate(value);
            malformed.push([name, value]);
        }
        executableCorrupt("operator stack kind", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 16,
            );
            value[found.instruction.absolute] = 38;
        });
        executableCorrupt("local store kind", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 104 && instruction.operand === 1,
            );
            const store = nominalMain.instructions[found.index + 1];
            value.writeUInt32LE(envelopeLocal, store.absolute + 1);
        });
        executableCorrupt("call argument identity", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 64 && instruction.operand === 0,
            );
            const load = nominalMain.instructions[found.index - 1];
            value.writeUInt32LE(envelopeLocal, load.absolute + 1);
        });
        executableCorrupt("record receiver identity", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 105 && instruction.operand === 1,
            );
            const load = nominalMain.instructions[found.index - 1];
            value.writeUInt32LE(envelopeLocal, load.absolute + 1);
        });
        executableCorrupt("enum operand identity", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 108,
            );
            const load = nominalMain.instructions[found.index - 1];
            value.writeUInt32LE(signalEnumLocal, load.absolute + 1);
        });
        executableCorrupt("branch condition kind", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 49,
            );
            const load = nominalMain.instructions[found.index - 1];
            value.writeUInt32LE(i32Local, load.absolute + 1);
        });
        executableCorrupt("unreachable instruction region", types, value => {
            const found = executableMatch(
                nominalMain,
                instruction => instruction.opcode === 49,
            );
            const jump = nominalMain.instructions[found.index + 1];
            value.writeUInt32LE(found.instruction.operand, jump.absolute + 1);
        });
        executableCorrupt("declared maximum stack", types, value => {
            value.writeUInt32LE(nominalMain.maximum + 1, nominalMain.maximumOffset);
        });
        const capabilityFunctions = executableFunctions(capabilities);
        const writeBytes = capabilityFunctions.find(item => item.name === "Writeˉbytes");
        executableCorrupt("capability argument kind", capabilities, value => {
            const found = executableMatch(
                writeBytes,
                instruction => instruction.opcode === 65,
            );
            const load = writeBytes.instructions[found.index - 2];
            value.writeUInt32LE(3, load.absolute + 1);
        });

        const malformedSteps = [
            1_489_732,
            962_975,
            972_910,
            1_008_012,
            1_018_894,
            1_024_756,
            1_586_867,
            1_501_962,
            172_629,
        ];
        for (let index = 0; index < malformed.length; index++) {
            const [name, input] = malformed[index];
            try {
                requireMemoryResult(
                    expected.path,
                    runMemory(exports, input, malformedSteps[index]),
                    0,
                    malformedSteps[index],
                    new Uint8Array(),
                );
            } catch (error) {
                throw new Error(`${expected.path}: ${name}: ${error.message}`);
            }
        }
    } else if (expected.runtime === "wvb-compiler-verifier") {
        function compilerPhase(path, expectedDigest, expectedBytes) {
            const bytes = readFileSync(path);
            const actualDigest = createHash("sha256").update(bytes).digest("hex");
            if (actualDigest !== expectedDigest || bytes.length !== expectedBytes) {
                throw new Error(
                    `${path}: expected ${expectedBytes}/${expectedDigest}, found ` +
                        `${bytes.length}/${actualDigest}.`,
                );
            }
            if (!WebAssembly.validate(bytes)) {
                throw new Error(`${path}: WebAssembly.validate rejected the module.`);
            }
            const phaseModule = new WebAssembly.Module(bytes);
            const phaseExports = new WebAssembly.Instance(phaseModule).exports;
            if (phaseExports["Windvale.abi"].value !== 3) {
                throw new Error(`${path}: execution ABI is not 3.`);
            }
            verifyMemoryContract(expected, phaseModule, phaseExports);
            return phaseExports;
        }

        const typedExports = compilerPhase(
            expected.typedPath,
            expected.typedSha256,
            expected.typedBytes,
        );
        const controlExports = compilerPhase(
            expected.controlPath,
            expected.controlSha256,
            expected.controlBytes,
        );
        const compiler = readFileSync(expected.inputPath);
        if (
            compiler.length !== 599_868 ||
            createHash("sha256").update(compiler).digest("hex") !==
                "9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066"
        ) {
            throw new Error(`${expected.inputPath}: the exact compiler WVB identity changed.`);
        }
        requireMemoryResult(
            expected.path,
            runMemory(exports, compiler, 1_381_756_663),
            0,
            1_381_756_663,
            Uint8Array.from([1]),
        );
        requireMemoryResult(
            expected.typedPath,
            runMemory(typedExports, compiler, 2_434_833_692),
            0,
            2_434_833_692,
            Uint8Array.from([1]),
        );
        requireMemoryResult(
            expected.controlPath,
            runMemory(controlExports, compiler, 1_952_101_000),
            0,
            1_952_101_000,
            Uint8Array.from([1]),
        );
        const portableCompiler = readFileSync(expected.portableInputPath);
        if (
            portableCompiler.length !== 597_545 ||
            createHash("sha256").update(portableCompiler).digest("hex") !==
                "5b819f86ffa05feaae1e27feb0b6fe6eda5034f1b229d4d9917ac7fa8041a0d4"
        ) {
            throw new Error(
                `${expected.portableInputPath}: the exact portable compiler WVB identity changed.`,
            );
        }
        requireMemoryResult(
            expected.path,
            runMemory(exports, portableCompiler, 1_380_577_333),
            0,
            1_380_577_333,
            Uint8Array.from([1]),
        );
        requireMemoryResult(
            expected.typedPath,
            runMemory(typedExports, portableCompiler, 2_430_056_746),
            0,
            2_430_056_746,
            Uint8Array.from([1]),
        );
        requireMemoryResult(
            expected.controlPath,
            runMemory(controlExports, portableCompiler, 1_951_031_795),
            0,
            1_951_031_795,
            Uint8Array.from([1]),
        );
    } else if (expected.runtime === "wvb-scalar-interpreter") {
        const verifierBytes = readFileSync(expected.verifierPath);
        const verifierModule = new WebAssembly.Module(verifierBytes);
        const verifierExports = new WebAssembly.Instance(verifierModule).exports;
        const candidates = expected.candidatePaths.map(path => readFileSync(path));
        const verifierSteps = [
            609_695, 3_056_274, 52_239, 170_636,
            7_204_240, 5_394_400, 64_361, 72_831, 44_612, 405_035, 603_525,
            2_479_539, 4_181_612, 1_914_037, 3_250_615,
            523_330, 6_120_785,
        ];
        for (let index = 0; index < candidates.length; index++) {
            requireMemoryResult(
                expected.verifierPath,
                runMemory(verifierExports, candidates[index], verifierSteps[index]),
                0,
                verifierSteps[index],
                Uint8Array.from([1]),
            );
        }
        requireMemoryResult(
            expected.verifierPath,
            runMemory(verifierExports, candidates[0], verifierSteps[0] - 1),
            3011,
            verifierSteps[0] - 1,
        );
        const bytesEntry = readFileSync(expected.bytesEntryPath);
        if (
            bytesEntry.length !== 209 ||
            createHash("sha256").update(bytesEntry).digest("hex") !==
                "b27ca1d0afcd379273f35e755eff12ccc5674762fe01381232eea36714d09bf1"
        ) {
            throw new Error(`${expected.bytesEntryPath}: the bytes-entry WVB identity changed.`);
        }
        requireMemoryResult(
            expected.verifierPath,
            runMemory(verifierExports, bytesEntry, 46_296),
            0,
            46_296,
            Uint8Array.from([1]),
        );

        function scalarRequest(candidate, guestBudget = 1_000, maximumCallDepth = 8) {
            const request = Buffer.alloc(16 + candidate.length);
            request.writeUInt32LE(0x49585657, 0);
            request.writeUInt16LE(1, 4);
            request.writeUInt16LE(0, 6);
            request.writeUInt32LE(guestBudget, 8);
            request.writeUInt32LE(maximumCallDepth, 12);
            candidate.copy(request, 16);
            return request;
        }

        function bytesRequest(
            candidate,
            input,
            guestBudget = 1_000,
            maximumCallDepth = 8,
        ) {
            const request = Buffer.alloc(24 + candidate.length + input.length);
            request.writeUInt32LE(0x49585657, 0);
            request.writeUInt16LE(2, 4);
            request.writeUInt16LE(0, 6);
            request.writeUInt32LE(guestBudget, 8);
            request.writeUInt32LE(maximumCallDepth, 12);
            request.writeUInt32LE(candidate.length, 16);
            request.writeUInt32LE(input.length, 20);
            candidate.copy(request, 24);
            Buffer.from(input).copy(request, 24 + candidate.length);
            return request;
        }

        function singleSourceSet(source) {
            const sourceSet = Buffer.alloc(24 + source.length);
            sourceSet.writeUInt32LE(0x53535657, 0);
            sourceSet.writeUInt16LE(1, 4);
            sourceSet.writeUInt16LE(0, 6);
            sourceSet.writeUInt32LE(1, 8);
            sourceSet.writeUInt32LE(8, 12);
            sourceSet.writeUInt32LE(24, 16);
            sourceSet.writeUInt32LE(source.length, 20);
            source.copy(sourceSet, 24);
            return sourceSet;
        }

        function requireScalarResult(
            name,
            actual,
            outerStatus,
            outerInstructions,
            guestStatus,
            guestInstructions,
            result,
        ) {
            const outputLength = outerStatus === 0 ? 20 : 0;
            if (
                actual.status !== outerStatus ||
                actual.instructions !== outerInstructions ||
                actual.output.length !== outputLength
            ) {
                throw new Error(
                    `${expected.path}: ${name}: expected outer status/instructions/length ` +
                        `${outerStatus}/${outerInstructions}/${outputLength}, found ` +
                        `${actual.status}/${actual.instructions}/${actual.output.length}.`,
                );
            }
            if (outerStatus !== 0) { return; }
            const output = Buffer.from(actual.output);
            if (
                output.readUInt32LE(0) !== 0x4F585657 ||
                output.readUInt16LE(4) !== 1 ||
                output.readUInt16LE(6) !== 0 ||
                output.readUInt32LE(8) !== guestStatus ||
                output.readUInt32LE(12) !== guestInstructions ||
                output.readInt32LE(16) !== result
            ) {
                throw new Error(`${expected.path}: ${name}: the WVXO response is invalid.`);
            }
        }

        function requireBytesResult(name, actual, outerInstructions, guestInstructions, result) {
            if (
                actual.status !== 0 ||
                actual.instructions !== outerInstructions ||
                actual.output.length !== 20 + result.length
            ) {
                throw new Error(
                    `${expected.path}: ${name}: expected outer status/instructions/length ` +
                        `0/${outerInstructions}/${20 + result.length}, found ` +
                        `${actual.status}/${actual.instructions}/${actual.output.length}.`,
                );
            }
            const output = Buffer.from(actual.output);
            if (
                output.readUInt32LE(0) !== 0x4F585657 ||
                output.readUInt16LE(4) !== 2 ||
                output.readUInt16LE(6) !== 0 ||
                output.readUInt32LE(8) !== 0 ||
                output.readUInt32LE(12) !== guestInstructions ||
                output.readUInt32LE(16) !== result.length ||
                !output.subarray(20).equals(Buffer.from(result))
            ) {
                throw new Error(`${expected.path}: ${name}: the WVXO2 response is invalid.`);
            }
        }

        function requireBytesFailure(
            name,
            actual,
            outerInstructions,
            guestStatus,
            guestInstructions,
        ) {
            if (
                actual.status !== 0 ||
                actual.instructions !== outerInstructions ||
                actual.output.length !== 20
            ) {
                throw new Error(
                    `${expected.path}: ${name}: expected outer status/instructions/length ` +
                        `0/${outerInstructions}/20, found ` +
                        `${actual.status}/${actual.instructions}/${actual.output.length}.`,
                );
            }
            const output = Buffer.from(actual.output);
            if (
                output.readUInt32LE(0) !== 0x4F585657 ||
                output.readUInt16LE(4) !== 2 ||
                output.readUInt16LE(6) !== 0 ||
                output.readUInt32LE(8) !== guestStatus ||
                output.readUInt32LE(12) !== guestInstructions ||
                output.readUInt32LE(16) !== 0
            ) {
                throw new Error(`${expected.path}: ${name}: the WVXO2 failure is invalid.`);
            }
        }

        const functionRequest = scalarRequest(candidates[0]);
        requireScalarResult(
            "function/control guest",
            runMemory(exports, functionRequest, 151_647),
            0,
            151_647,
            0,
            199,
            6,
        );
        requireScalarResult(
            "function/control guest repeat",
            runMemory(exports, functionRequest, 151_647),
            0,
            151_647,
            0,
            199,
            6,
        );
        requireScalarResult(
            "outer instruction exhaustion",
            runMemory(exports, functionRequest, 151_646),
            3011,
            151_646,
            0,
            0,
            0,
        );
        const bytesEntryRequest = bytesRequest(bytesEntry, Buffer.from([1, 2, 3]));
        requireBytesResult(
            "bytes entry and return",
            runMemory(exports, bytesEntryRequest, 15_567),
            15_567,
            13,
            Buffer.from([1, 2, 3, 42]),
        );
        const truncatedBytesRequest = bytesEntryRequest.subarray(
            0,
            bytesEntryRequest.length - 1,
        );
        requireMemoryResult(
            expected.path,
            runMemory(exports, truncatedBytesRequest, 255),
            0,
            255,
        );
        const portableCompiler = readFileSync(expected.portableCompilerPath);
        const portableCompilerInput = singleSourceSet(
            readFileSync(expected.portableSourcePath),
        );
        requireBytesFailure(
            "portable compiler enters execution",
            runMemory(
                exports,
                bytesRequest(portableCompiler, portableCompilerInput, 1, 64),
                50_000_000,
            ),
            45_177_347,
            3011,
            1,
        );
        requireBytesFailure(
            "portable compiler former allocation boundary",
            runMemory(
                exports,
                bytesRequest(portableCompiler, portableCompilerInput, 1_511, 64),
                50_000_000,
            ),
            46_155_795,
            3011,
            1_511,
        );
        requireBytesFailure(
            "portable compiler crosses former allocation boundary",
            runMemory(
                exports,
                bytesRequest(portableCompiler, portableCompilerInput, 1_512, 64),
                50_000_000,
            ),
            46_156_318,
            3011,
            1_512,
        );
        requireBytesFailure(
            "portable compiler crosses guest record reclamation boundary",
            runMemory(
                exports,
                bytesRequest(portableCompiler, portableCompilerInput, 100_000, 64),
                200_000_000,
            ),
            96_797_247,
            3011,
            100_000,
        );
        requireMemoryResult(
            expected.path,
            runMemory(
                exports,
                bytesRequest(portableCompiler, portableCompilerInput, 20_000_000, 64),
                50_000_000,
            ),
            3011,
            50_000_000,
        );
        requireScalarResult(
            "complete scalar guest",
            runMemory(exports, scalarRequest(candidates[1]), 352_335),
            0,
            352_335,
            0,
            351,
            42,
        );
        requireScalarResult(
            "guest instruction exhaustion",
            runMemory(exports, scalarRequest(candidates[0], 198, 8), 151_442),
            0,
            151_442,
            3011,
            198,
            0,
        );
        requireScalarResult(
            "guest call-depth exhaustion",
            runMemory(exports, scalarRequest(candidates[0], 1_000, 1), 70_247),
            0,
            70_247,
            3004,
            27,
            0,
        );
        requireScalarResult(
            "checked i32 overflow",
            runMemory(exports, scalarRequest(candidates[2]), 14_105),
            3007,
            14_105,
            0,
            0,
            0,
        );
        requireScalarResult(
            "checked u32 overflow",
            runMemory(exports, scalarRequest(candidates[3]), 23_029),
            3007,
            23_029,
            0,
            0,
            0,
        );
        requireScalarResult(
            "text/bytes values and descriptor calls",
            runMemory(exports, scalarRequest(candidates[4], 4_096), 314_497),
            0,
            314_497,
            0,
            298,
            42,
        );
        requireScalarResult(
            "strict UTF-8 boundaries",
            runMemory(exports, scalarRequest(candidates[5], 4_096), 199_681),
            0,
            199_681,
            0,
            153,
            42,
        );
        requireScalarResult(
            "invalid UTF-8 decoding",
            runMemory(exports, scalarRequest(candidates[6], 4_096), 16_988),
            0,
            16_988,
            3014,
            11,
            0,
        );
        requireScalarResult(
            "byte range failure",
            runMemory(exports, scalarRequest(candidates[7], 4_096), 19_443),
            0,
            19_443,
            3008,
            14,
            0,
        );
        requireScalarResult(
            "u16 narrowing failure",
            runMemory(exports, scalarRequest(candidates[8], 4_096), 9_817),
            0,
            9_817,
            3016,
            4,
            0,
        );
        requireScalarResult(
            "per-value byte limit",
            runMemory(exports, scalarRequest(candidates[9], 4_096), 186_027),
            0,
            186_027,
            3015,
            256,
            0,
        );
        requireScalarResult(
            "SHA-256 aggregate heap limit",
            runMemory(exports, scalarRequest(candidates[10], 4_096), 276_511),
            0,
            276_511,
            3018,
            388,
            0,
        );
        requireScalarResult(
            "integer formatting and UTF-16-compatible text quoting",
            runMemory(exports, scalarRequest(candidates[11], 4_096), 2_088_540),
            0,
            2_088_540,
            0,
            4_070,
            42,
        );
        requireScalarResult(
            "compiler-produced data/text and quoting",
            runMemory(exports, scalarRequest(candidates[12], 4_096), 247_919),
            0,
            247_919,
            0,
            233,
            13,
        );
        requireScalarResult(
            "SHA-256 padding and multi-block vectors",
            runMemory(exports, scalarRequest(candidates[13], 4_096), 2_015_310),
            0,
            2_015_310,
            0,
            3_996,
            42,
        );
        requireScalarResult(
            "compiler-produced records and enums",
            runMemory(exports, scalarRequest(candidates[14], 4_096), 271_546),
            0,
            271_546,
            0,
            197,
            11,
        );
        requireScalarResult(
            "record and enum construction",
            runMemory(exports, scalarRequest(candidates[15], 4_096), 131_109),
            0,
            131_109,
            0,
            67,
            42,
        );

        const defaultLocal = Buffer.from(candidates[15]);
        let defaultCursor = 12;
        let defaultCode = 0;
        for (let kind = 1; kind <= 7; kind++) {
            const length = defaultLocal.readUInt32LE(defaultCursor + 4);
            if (kind === 5) { defaultCode = defaultCursor + 8; }
            defaultCursor += 8 + length;
        }
        defaultLocal.writeUInt32LE(1, defaultCode + 0x61);
        requireMemoryResult(
            expected.verifierPath,
            runMemory(verifierExports, defaultLocal, 523_397),
            0,
            523_397,
            Uint8Array.from([1]),
        );
        requireScalarResult(
            "default record and first enum member",
            runMemory(exports, scalarRequest(defaultLocal, 4_096), 86_744),
            0,
            86_744,
            0,
            37,
            2,
        );
        const recordArenaRequest = scalarRequest(candidates[16], 10_000);
        requireScalarResult(
            "record arena live-set exhaustion after reclamation",
            runMemory(exports, recordArenaRequest, 4_071_115),
            0,
            4_071_115,
            3017,
            4_332,
            0,
        );
        requireScalarResult(
            "record arena reset",
            runMemory(exports, recordArenaRequest, 4_071_115),
            0,
            4_071_115,
            3017,
            4_332,
            0,
        );
    } else if (expected.runtime === "wvb-semantic") {
        const [data, types, capabilities] = expected.acceptedInputPaths.map(path =>
            readFileSync(path));
        const acceptedSteps = [1_122_118, 912_984, 113_534];
        for (let index = 0; index < expected.acceptedInputPaths.length; index++) {
            requireMemoryResult(
                expected.path,
                runMemory(
                    exports,
                    readFileSync(expected.acceptedInputPaths[index]),
                    acceptedSteps[index],
                ),
                0,
                acceptedSteps[index],
                Uint8Array.from([1]),
            );
        }
        requireMemoryResult(
            expected.path,
            runMemory(exports, data, acceptedSteps[0] - 1),
            3011,
            acceptedSteps[0] - 1,
        );

        const readU32 = (bytes, offset) => bytes.readUInt32LE(offset);
        function findSections(bytes) {
            const result = [];
            let cursor = 12;
            for (let kind = 1; kind <= 7; kind++) {
                const length = readU32(bytes, cursor + 4);
                result[kind] = { payload: cursor + 8, length };
                cursor += 8 + length;
            }
            return result;
        }
        function findUtf8(bytes, value) {
            const offset = bytes.indexOf(Buffer.from(value, "utf8"));
            if (offset < 0) {
                throw new Error(`${expected.path}: semantic input does not contain '${value}'.`);
            }
            return offset;
        }
        function skipShape(bytes, cursor) {
            return cursor + (bytes[cursor] >= 7 ? 5 : 1);
        }
        function parseFunctions(bytes) {
            const sections = findSections(bytes);
            let cursor = sections[4].payload;
            const count = readU32(bytes, cursor);
            cursor += 4;
            const functions = [];
            for (let functionIndex = 0; functionIndex < count; functionIndex++) {
                const nameLength = readU32(bytes, cursor);
                cursor += 4 + nameLength;
                const parameterCount = readU32(bytes, cursor);
                cursor += 4;
                for (let index = 0; index < parameterCount; index++) {
                    cursor = skipShape(bytes, cursor);
                }
                cursor = skipShape(bytes, cursor);
                const localCount = readU32(bytes, cursor);
                cursor += 4;
                for (let index = 0; index < localCount; index++) {
                    cursor = skipShape(bytes, cursor);
                }
                functions.push({
                    codeOffset: readU32(bytes, cursor),
                    codeLength: readU32(bytes, cursor + 4),
                });
                cursor += 12;
            }
            return { functions, codePayload: sections[5].payload };
        }
        function firstNominalShapeIndex(bytes) {
            const sections = findSections(bytes);
            let cursor = sections[4].payload;
            const count = readU32(bytes, cursor);
            cursor += 4;
            for (let functionIndex = 0; functionIndex < count; functionIndex++) {
                const nameLength = readU32(bytes, cursor);
                cursor += 4 + nameLength;
                const parameterCount = readU32(bytes, cursor);
                cursor += 4;
                for (let index = 0; index < parameterCount; index++) {
                    if (bytes[cursor] >= 7) { return cursor + 1; }
                    cursor = skipShape(bytes, cursor);
                }
                if (bytes[cursor] >= 7) { return cursor + 1; }
                cursor = skipShape(bytes, cursor);
                const localCount = readU32(bytes, cursor);
                cursor += 4;
                for (let index = 0; index < localCount; index++) {
                    if (bytes[cursor] >= 7) { return cursor + 1; }
                    cursor = skipShape(bytes, cursor);
                }
                cursor += 12;
            }
            throw new Error(`${expected.path}: semantic nominal input has no nominal shape.`);
        }
        function instructionWidth(opcode) {
            if (opcode === 2 || opcode === 8) { return 2; }
            if (opcode === 106) { return 9; }
            if ([1, 3, 4, 5, 6, 7, 9, 10, 48, 49, 64, 65, 104, 105]
                .includes(opcode)) {
                return 5;
            }
            return 1;
        }
        function mutateInstruction(bytes, wanted, mutate) {
            const parsed = parseFunctions(bytes);
            for (const functionDeclaration of parsed.functions) {
                let instructionOffset = 0;
                while (instructionOffset < functionDeclaration.codeLength) {
                    const absolute = parsed.codePayload +
                        functionDeclaration.codeOffset + instructionOffset;
                    const opcode = bytes[absolute];
                    if (wanted(opcode)) {
                        mutate(bytes, absolute, instructionOffset);
                        return;
                    }
                    instructionOffset += instructionWidth(opcode);
                }
            }
            throw new Error(`${expected.path}: semantic input has no requested instruction.`);
        }
        function duplicateEnumValue(bytes) {
            const sections = findSections(bytes);
            let cursor = sections[7].payload;
            const count = readU32(bytes, cursor);
            cursor += 4;
            for (let typeIndex = 0; typeIndex < count; typeIndex++) {
                const kind = bytes[cursor++];
                const nameLength = readU32(bytes, cursor);
                cursor += 4 + nameLength;
                const itemCount = readU32(bytes, cursor);
                cursor += 4;
                if (kind === 1) {
                    for (let itemIndex = 0; itemIndex < itemCount; itemIndex++) {
                        const itemLength = readU32(bytes, cursor);
                        cursor = skipShape(bytes, cursor + 4 + itemLength);
                    }
                    continue;
                }
                let firstValue = 0;
                for (let itemIndex = 0; itemIndex < itemCount; itemIndex++) {
                    const itemLength = readU32(bytes, cursor);
                    cursor += 4 + itemLength;
                    if (itemIndex === 0) {
                        firstValue = readU32(bytes, cursor);
                    } else if (itemIndex === 1) {
                        bytes.writeUInt32LE(firstValue, cursor);
                        return;
                    }
                    cursor += 4;
                }
            }
            throw new Error(`${expected.path}: semantic nominal input has no two-member enum.`);
        }
        function redirectRecordEnumFieldToRecord(bytes) {
            const sections = findSections(bytes);
            let cursor = sections[7].payload;
            const count = readU32(bytes, cursor);
            cursor += 4;
            for (let typeIndex = 0; typeIndex < count; typeIndex++) {
                const kind = bytes[cursor++];
                const nameLength = readU32(bytes, cursor);
                cursor += 4 + nameLength;
                const itemCount = readU32(bytes, cursor);
                cursor += 4;
                for (let itemIndex = 0; itemIndex < itemCount; itemIndex++) {
                    const itemLength = readU32(bytes, cursor);
                    cursor += 4 + itemLength;
                    if (kind === 1) {
                        if (bytes[cursor] === 8) {
                            bytes.writeUInt32LE(0, cursor + 1);
                            return;
                        }
                        cursor = skipShape(bytes, cursor);
                    } else {
                        cursor += 4;
                    }
                }
            }
            throw new Error(`${expected.path}: semantic nominal input has no enum field.`);
        }

        const malformed = [];
        function corrupt(name, original, mutate, instructions) {
            const value = Buffer.from(original);
            mutate(value);
            malformed.push([name, value, instructions]);
        }
        corrupt("module identifier", data, value => {
            value[findSections(value)[1].payload + 5] = 45;
        }, 103_959);
        corrupt("portable capabilities", capabilities, value => {
            value[findSections(value)[1].payload] = 1;
        }, 33_211);
        corrupt("capability signature", capabilities, value => {
            let cursor = findSections(value)[2].payload + 4;
            const nameLength = readU32(value, cursor);
            cursor += 4 + nameLength;
            const parameterCount = readU32(value, cursor);
            cursor += 4 + parameterCount;
            value[cursor] = value[cursor] === 0 ? 1 : 0;
        }, 35_029);
        corrupt("data order", data, value => {
            value[findUtf8(value, "__Text_000001") + 12] = 48;
        }, 117_006);
        corrupt("text UTF-8", data, value => {
            value[findUtf8(value, "same")] = 0xC0;
        }, 109_928);
        corrupt("function order", data, value => {
            Buffer.from("Alpha").copy(value, findUtf8(value, "Zebra"));
        }, 127_568);
        corrupt("nominal shape kind", types, value => {
            value.writeUInt32LE(2, firstNominalShapeIndex(value));
        }, 104_018);
        corrupt("text data kind", data, value => {
            mutateInstruction(value, opcode => opcode === 3, (bytes, absolute) => {
                bytes.writeUInt32LE(0, absolute + 1);
            });
        }, 149_390);
        corrupt("branch boundary", data, value => {
            mutateInstruction(
                value,
                opcode => opcode === 48 || opcode === 49,
                (bytes, absolute, instructionOffset) => {
                    bytes.writeUInt32LE(instructionOffset + 1, absolute + 1);
                },
            );
        }, 202_355);
        corrupt("export identity", data, value => {
            value[findSections(value)[6].payload + 8] = 78;
        }, 1_121_329);
        corrupt("type identity", types, value => {
            Buffer.from("Reading").copy(value, findUtf8(value, "Weather"));
        }, 908_204);
        corrupt("enum backing value", types, duplicateEnumValue, 906_695);
        corrupt("record enum target", types, redirectRecordEnumFieldToRecord, 890_880);

        for (const [name, input, instructions] of malformed) {
            const result = runMemory(exports, input, 2_000_000);
            try {
                requireMemoryResult(
                    expected.path,
                    result,
                    0,
                    instructions,
                    new Uint8Array(),
                );
            } catch (error) {
                throw new Error(`${expected.path}: ${name}: ${error.message}`);
            }
        }
    } else {
        throw new Error(`${expected.path}: unknown runtime-value verification profile.`);
    }

    requireMemoryResult(
        expected.path,
        {
            status: exports["Windvale.run"](1_000, capacity + 1),
            instructions: exports["Windvale.instructions"].value,
            outputLength: exports["Windvale.output_length"].value,
            output: new Uint8Array(),
        },
        3008,
        0,
    );
    console.log(
        `${expected.path}: ${expected.name}; ABI 3 runtime=${expected.runtime} ` +
            `capacity=${capacity} SHA-256=${digest}`,
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
        if (expected.runtime) {
            verifyRuntime(expected, module, exports, digest);
        } else {
            verifyMemory(expected, module, exports, digest);
        }
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
