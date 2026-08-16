import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";

const IDENTITIES = Object.freeze({
    wvb: Object.freeze({
        bytes: 927,
        sha256: "b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713",
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

function Inspectˉechoˉwvb(Bytes) {
    let Cursor = 0;
    const Need = Count => {
        assert.equal(Cursor + Count <= Bytes.length, true, "bounded WVB field");
    };
    const U8 = () => { Need(1); return Bytes[Cursor++]; };
    const U16 = () => { Need(2); const Value = Bytes.readUInt16LE(Cursor); Cursor += 2; return Value; };
    const U32 = () => { Need(4); const Value = Bytes.readUInt32LE(Cursor); Cursor += 4; return Value; };
    const Text = () => {
        const Length = U32();
        Need(Length);
        const Value = new TextDecoder("utf-8", { fatal: true }).decode(
            Bytes.subarray(Cursor, Cursor + Length),
        );
        Cursor += Length;
        return Value;
    };
    assert.deepEqual(Bytes.subarray(0, 4), Buffer.from("WVB1"), "WVB magic");
    Cursor = 4;
    assert.equal(U16(), 1, "WVB major");
    assert.equal(U16(), 11, "WVB minor");
    assert.equal(U32(), 7, "WVB section count");
    const Sections = [];
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        assert.equal(U8(), Kind, `section ${Kind} kind`);
        assert.equal(U8(), 0, `section ${Kind} flags`);
        assert.equal(U16(), 0, `section ${Kind} reserved`);
        const Length = U32();
        Need(Length);
        Sections.push(Bytes.subarray(Cursor, Cursor + Length));
        Cursor += Length;
    }
    assert.equal(Cursor, Bytes.length, "WVB trailing bytes");

    const Readˉpayload = (Payload, Inspect) => {
        const Saved = Bytes;
        Bytes = Payload;
        Cursor = 0;
        Inspect({ U8, U32, Text });
        assert.equal(Cursor, Bytes.length, "section payload trailing bytes");
        Bytes = Saved;
    };
    Readˉpayload(Sections[0], ({ U8, U32, Text }) => {
        assert.equal(U8(), 2, "derived hosted profile");
        assert.equal(Text(), "Windvaleˉecho", "module name");
        assert.equal(U8(), 1, "metadata presence");
        assert.equal(U8(), 1, "metadata version");
        assert.equal(U8(), 2, "application authority");
        assert.equal(U32(), 2, "platform count");
        assert.deepEqual([Text(), Text()], ["linux", "windows"], "platform scopes");
        assert.equal(U32(), 3, "required capability count");
        const Required = [];
        for (let Index = 0; Index < 3; Index += 1) Required.push([Text(), U32()]);
        assert.deepEqual(Required, [
            ["console.write_line", 1],
            ["process.argument", 1],
            ["process.argument_count", 1],
        ], "required capabilities");
        assert.equal(U32(), 0, "optional capability count");
    });
    Readˉpayload(Sections[1], ({ U8, U32, Text }) => {
        assert.equal(U32(), 3, "capability count");
        const Capabilities = [];
        for (let Index = 0; Index < 3; Index += 1) {
            const Name = Text();
            const Count = U32();
            const Parameters = [];
            for (let Parameter = 0; Parameter < Count; Parameter += 1) Parameters.push(U8());
            Capabilities.push([Name, Parameters, U8()]);
        }
        assert.deepEqual(Capabilities, [
            ["console.write_line", [3], 0],
            ["process.argument", [5], 3],
            ["process.argument_count", [], 5],
        ], "executable capability directory");
    });
    Readˉpayload(Sections[6], ({ U8, U32, Text }) => {
        assert.equal(U32(), 1, "type count");
        assert.equal(U8(), 2, "spacing-state enum kind");
        assert.equal(Text(), "Windvaleˉechoˉposition", "spacing-state type");
        assert.equal(U32(), 2, "spacing-state member count");
        assert.deepEqual([Text(), U32(), Text(), U32()],
            ["First", 0, "Following", 1], "spacing-state members");
    });
}

const Arguments = process.argv.slice(2);
if (Arguments[0] === "inspect") {
    if (Arguments.length !== 2) {
        process.stderr.write("Usage: node Verify-Echo-Application.mjs inspect <echo.wvb>\n");
        process.exit(64);
    }
    Inspectˉechoˉwvb(Readˉexact(path.resolve(Arguments[1]), IDENTITIES.wvb, "Echo WVB"));
    process.stdout.write("native Windvale echo inspection status=Valid metadata=Present platforms=2 capabilities=3\n");
    process.exit(0);
}

const [Target, WvbArgument, WindowsArgument, LinuxArgument] = Arguments;
if (Arguments.length !== 4 || !["windows", "linux"].includes(Target)) {
    process.stderr.write(
        "Usage: node Verify-Echo-Application.mjs <windows|linux> " +
        "<echo.wvb> <echo.exe> <echo.elf>\n",
    );
    process.exit(64);
}

const Wvb = path.resolve(WvbArgument);
const Windows = path.resolve(WindowsArgument);
const Linux = path.resolve(LinuxArgument);
Inspectˉechoˉwvb(Readˉexact(Wvb, IDENTITIES.wvb, "Echo WVB"));
Readˉexact(Windows, IDENTITIES.windows, "Windows Echo application");
Readˉexact(Linux, IDENTITIES.linux, "Linux Echo application");

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
