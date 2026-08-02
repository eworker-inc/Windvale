import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";

const EXPECTED = [
    {
        path: process.argv[2],
        sha256: "4057797732dd7250413f44aa71e012222591ae7e219e27a7680f246b2cedeb8a",
        status: 0,
        result: 2147483647,
        instructions: 10,
    },
    {
        path: process.argv[3],
        sha256: "984139ccb136981e4d6382e4c547012be13df38af056cd09abebec10cc1a6f52",
        status: 3007,
        result: 0,
        instructions: 7,
    },
];

if (process.argv.length !== 4) {
    throw new Error(
        "Usage: node Verify-WebAssembly-Engine.mjs <success.wasm> <overflow.wasm>",
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
        `${expected.path}: ABI 1 status=${status} result=${result} ` +
            `instructions=${instructions} SHA-256=${digest}`,
    );
}

console.log(`WebAssembly engine verification passed under Node.js ${process.version}.`);
