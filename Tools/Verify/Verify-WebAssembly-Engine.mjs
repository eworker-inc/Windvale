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
];

if (process.argv.length !== 13) {
    throw new Error(
        "Usage: node Verify-WebAssembly-Engine.mjs " +
            "<add-success.wasm> <add-overflow.wasm> <straight-i32.wasm> " +
            "<subtract-overflow.wasm> <multiply-overflow.wasm> <negate-overflow.wasm> " +
            "<metered-loop.wasm> <nonterminating-loop.wasm> " +
            "<structured-control.wasm> <structured-control-else.wasm> " +
            "<sequential-if.wasm>",
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

    const { instance } = await WebAssembly.instantiate(bytes);
    const exports = instance.exports;
    if (exports["Windvale.abi"].value !== expected.abi) {
        throw new Error(`${expected.path}: execution ABI is not ${expected.abi}.`);
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
