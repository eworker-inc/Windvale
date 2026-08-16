import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";

const IDENTITIES = Object.freeze({
    wvb: Object.freeze({
        bytes: 813,
        sha256: "5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64",
    }),
    windows: Object.freeze({
        bytes: 22016,
        sha256: "024cfac66fa760b705a48e72942103a79e24342d3e59886e9ccd127dfd3cdbcb",
    }),
    linux: Object.freeze({
        bytes: 24576,
        sha256: "0e5a91887381adb23a84d745ce06902be99e53d70e58a598465939881638b576",
    }),
});

const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");

function Readˉexact(Path, Identity, Description) {
    const Bytes = fs.readFileSync(Path);
    assert.equal(Bytes.length, Identity.bytes, `${Description} byte length`);
    assert.equal(Sha256(Bytes), Identity.sha256, `${Description} SHA-256`);
    return Bytes;
}

function Run(Host, Arguments) {
    return spawnSync(Host, Arguments, {
        encoding: null,
        maxBuffer: 131072,
        windowsHide: true,
    });
}

function Requireˉsuccess(Host, Arguments, Expected, Description) {
    const Result = Run(Host, Arguments);
    assert.equal(Result.error, undefined, `${Description} execution`);
    assert.equal(Result.signal, null, `${Description} signal`);
    assert.equal(Result.status, 0, `${Description} status`);
    assert.deepEqual(Result.stdout, Buffer.from(Expected, "utf8"), `${Description} output`);
    assert.equal(Result.stderr.length, 0, `${Description} diagnostic`);
}

function Requireˉbudgetˉrejection(Host, Arguments, Description) {
    const Result = Run(Host, Arguments);
    assert.equal(Result.error, undefined, `${Description} execution`);
    assert.equal(Result.signal, null, `${Description} signal`);
    assert.equal(Result.status, 1, `${Description} status`);
    assert.equal(Result.stdout.length, 0, `${Description} output`);
    assert.equal(Result.stderr.length, 0, `${Description} diagnostic`);
}

const [Target, WvbArgument, WindowsArgument, LinuxArgument, InspectionArgument] =
    process.argv.slice(2);
if (process.argv.length !== 7 || !["windows", "linux"].includes(Target)) {
    process.stderr.write(
        "Usage: node Verify-Echo-Application.mjs <windows|linux> " +
        "<echo.wvb> <echo.exe> <echo.elf> <inspection.txt>\n",
    );
    process.exit(64);
}

const Wvb = path.resolve(WvbArgument);
const Windows = path.resolve(WindowsArgument);
const Linux = path.resolve(LinuxArgument);
const Inspection = path.resolve(InspectionArgument);
Readˉexact(Wvb, IDENTITIES.wvb, "Echo WVB");
Readˉexact(Windows, IDENTITIES.windows, "Windows Echo application");
Readˉexact(Linux, IDENTITIES.linux, "Linux Echo application");

const Inspectionˉtext = fs.readFileSync(Inspection, "utf8");
assert.equal(Inspectionˉtext.includes("\r"), false, "inspection must use LF");
const Inspectionˉlines = Inspectionˉtext.split("\n");
assert.equal(
    Inspectionˉlines[0],
    "wvdump 1",
    "inspection header",
);
assert.equal(
    Inspectionˉlines[1],
    "module version=1.11 profile=hosted name=\"Windvale\\u02C9echo\"",
    "module identity",
);
assert.deepEqual(
    Inspectionˉlines.filter(Line => Line.startsWith("capability index=")),
    [
        "capability index=0 name=\"console.write_line\" parameters=1 result=void",
        "capability index=1 name=\"process.argument\" parameters=1 result=text",
        "capability index=2 name=\"process.argument_count\" parameters=0 result=u32",
    ],
    "capability directory",
);
assert.equal(
    Inspectionˉlines.includes(
        "type index=0 name=\"Windvale\\u02C9echo\\u02C9position\" kind=enum members=2",
    ),
    true,
    "spacing-state type",
);
assert.equal(
    Inspectionˉlines.includes("enum_member type=0 index=0 name=\"First\" value=0"),
    true,
    "first spacing state",
);
assert.equal(
    Inspectionˉlines.includes("enum_member type=0 index=1 name=\"Following\" value=1"),
    true,
    "following spacing state",
);

const Host = Target === "windows" ? Windows : Linux;
const Cases = [
    ["empty", [], "\n"],
    ["one-argument", ["Windvale"], "Windvale\n"],
    ["immutable-words", ["alpha", "two words", "omega"], "alpha two words omega\n"],
    ["empty-argument", ["", "x"], " x\n"],
    ["unicode", ["café", "λ"], "café λ\n"],
    ["argument-byte-boundary", ["x".repeat(4096)], `${"x".repeat(4096)}\n`],
    ["argument-count-boundary", Array(67).fill("x"), `${Array(67).fill("x").join(" ")}\n`],
];
let Item = 0;
for (const [Name, Arguments, Expected] of Cases) {
    Item += 1;
    process.stdout.write(`native Windvale echo case=${Name} item=${Item}/9\n`);
    Requireˉsuccess(Host, Arguments, Expected, Name);
}
process.stdout.write("native Windvale echo case=argument-byte-exhaustion item=8/9\n");
Requireˉbudgetˉrejection(Host, ["x".repeat(4097)], "argument byte exhaustion");
process.stdout.write("native Windvale echo case=argument-count-exhaustion item=9/9\n");
Requireˉbudgetˉrejection(Host, Array(68).fill("x"), "argument count exhaustion");

process.stdout.write(
    `native Windvale echo evidence target=${Target} cases=9 ` +
    `wvb=${IDENTITIES.wvb.sha256} host=${IDENTITIES[Target].sha256}\n`,
);
