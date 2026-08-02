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
    },
    {
        name: "checked addition overflow",
        path: process.argv[3],
        sha256: "984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52",
        bytes: 176,
        status: 3007,
        result: 0,
        instructions: 7,
    },
    {
        name: "straight-line i32 success",
        path: process.argv[4],
        sha256: "15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb",
        bytes: 432,
        status: 0,
        result: 42,
        instructions: 30,
    },
    {
        name: "checked subtraction overflow",
        path: process.argv[5],
        sha256: "757d26c2cf404cabcf5b78d2c998bc7ddc78ec4531e4571630ae2c1b5c8d7925",
        bytes: 268,
        status: 3007,
        result: 0,
        instructions: 10,
    },
    {
        name: "checked multiplication overflow",
        path: process.argv[6],
        sha256: "e924c7507a363a7b019935622abfbd4bf4ac8445cd37a0412130ce8e5c83d51a",
        bytes: 224,
        status: 3007,
        result: 0,
        instructions: 7,
    },
    {
        name: "checked negation overflow",
        path: process.argv[7],
        sha256: "3f098efd63c68d8c62a4f6b373507e12c21808ff01120d165c9dc85a047e99e2",
        bytes: 307,
        status: 3007,
        result: 0,
        instructions: 13,
    },
];

if (process.argv.length !== 8) {
    throw new Error(
        "Usage: node Verify-WebAssembly-Engine.mjs " +
            "<add-success.wasm> <add-overflow.wasm> <straight-i32.wasm> " +
            "<subtract-overflow.wasm> <multiply-overflow.wasm> <negate-overflow.wasm>",
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
    if (exports["Windvale.abi"].value !== 1) {
        throw new Error(`${expected.path}: execution ABI is not 1.`);
    }

    exports["Windvale.result"].value = -123;
    exports["Windvale.instructions"].value = 99;
    const status = exports["Windvale.run"]();
    const result = exports["Windvale.result"].value;
    const instructions = exports["Windvale.instructions"].value;
    if (
        status !== expected.status ||
        result !== expected.result ||
        instructions !== expected.instructions
    ) {
        throw new Error(
            `${expected.path}: expected status/result/instructions ` +
                `${expected.status}/${expected.result}/${expected.instructions}, found ` +
                `${status}/${result}/${instructions}.`,
        );
    }

    console.log(
        `${expected.path}: ${expected.name}; ABI 1 status=${status} result=${result} ` +
            `instructions=${instructions} SHA-256=${digest}`,
    );
}

console.log(`WebAssembly engine verification passed under Node.js ${process.version}.`);
