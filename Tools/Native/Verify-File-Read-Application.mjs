import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

const [Host, WvbArgument, WindowsArgument, LinuxArgument] = process.argv.slice(2);
if (!["windows", "linux"].includes(Host) || !WvbArgument ||
    !WindowsArgument || !LinuxArgument) {
    process.stderr.write(
        "Usage: node Verify-File-Read-Application.mjs " +
        "<windows|linux> <file-read.wvb> <file-read.exe> <file-read.elf>\n",
    );
    process.exit(64);
}

const Wvb = path.resolve(WvbArgument);
const Application = path.resolve(Host === "windows" ? WindowsArgument : LinuxArgument);
const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-file-read-cases-"));
const FixedName = "Windvale-Database-Storage.bin";
const FixedPath = path.join(Work, FixedName);
const MAX_BUFFER = 5 * 1024 * 1024;

function Pattern(Bytes, Salt) {
    const Result = Buffer.alloc(Bytes);
    for (let Index = 0; Index < Bytes; Index += 1) {
        Result[Index] = (Index * 131 + Salt * 17 + (Index >>> 8)) & 0xff;
    }
    return Result;
}

function Run(Arguments) {
    return spawnSync(Application, Arguments, {
        cwd: Work,
        encoding: null,
        maxBuffer: MAX_BUFFER,
        windowsHide: true,
    });
}

function Requireˉresult(Name, Result, Status, StandardOutput, StandardError) {
    assert.equal(Result.error, undefined, `${Name}: process error`);
    assert.equal(Result.signal, null, `${Name}: signal`);
    assert.equal(Result.status, Status, `${Name}: exit status`);
    assert.deepEqual(Result.stdout, StandardOutput, `${Name}: stdout`);
    assert.deepEqual(Result.stderr, StandardError, `${Name}: stderr`);
}

function Replaceˉfixture(Value) {
    fs.rmSync(FixedPath, { force: true, recursive: false });
    fs.writeFileSync(FixedPath, Value, { flag: "wx", mode: 0o600 });
}

try {
    assert.equal(fs.statSync(Wvb).size, 76474);
    const SuccessCases = [
        ["empty", Buffer.alloc(0)],
        ["invalid-utf8", Buffer.from([0x00, 0xff, 0xc3, 0x28, 0x0a])],
        ["one-chunk", Pattern(3072, 1)],
        ["chunk-plus-one", Pattern(3073, 2)],
        ["multiple-chunks", Pattern(6145, 3)],
        ["maximum", Pattern(4194304, 4)],
    ];
    let Item = 1;
    for (const [Name, Value] of SuccessCases) {
        process.stdout.write(`file-read case item=${Item}/12 name=${Name}\n`);
        Replaceˉfixture(Value);
        Requireˉresult(Name, Run([FixedName]), 0, Value, Buffer.alloc(0));
        Item += 1;
    }

    process.stdout.write(`file-read case item=${Item}/12 name=oversize\n`);
    Replaceˉfixture(Pattern(4194305, 5));
    Requireˉresult(
        "oversize", Run([FixedName]), 3, Buffer.alloc(0),
        Buffer.from("file-read: file exceeds 4194304 bytes\n"),
    );
    Item += 1;

    process.stdout.write(`file-read case item=${Item}/12 name=no-arguments\n`);
    Replaceˉfixture(Buffer.from([42]));
    Requireˉresult(
        "no-arguments", Run([]), 64, Buffer.alloc(0),
        Buffer.from("Usage: file-read <name>\n"),
    );
    Item += 1;

    process.stdout.write(`file-read case item=${Item}/12 name=extra-argument\n`);
    Requireˉresult(
        "extra-argument", Run([FixedName, "extra"]), 64, Buffer.alloc(0),
        Buffer.from("Usage: file-read <name>\n"),
    );
    Item += 1;

    process.stdout.write(`file-read case item=${Item}/12 name=unknown-name\n`);
    Requireˉresult(
        "unknown-name", Run(["Other.bin"]), 3, Buffer.alloc(0),
        Buffer.from("file-read: directory status=Notˉfound\n"),
    );
    Item += 1;

    process.stdout.write(`file-read case item=${Item}/12 name=unavailable\n`);
    fs.rmSync(FixedPath, { force: true, recursive: false });
    Requireˉresult(
        "unavailable", Run([FixedName]), 3, Buffer.alloc(0),
        Buffer.from("file-read: directory status=Unavailable\n"),
    );
    Item += 1;

    process.stdout.write(`file-read case item=${Item}/12 name=no-link\n`);
    const LinkTarget = path.join(Work, "Link-Target");
    if (Host === "windows") {
        fs.mkdirSync(LinkTarget);
        fs.symlinkSync(LinkTarget, FixedPath, "junction");
    } else {
        fs.writeFileSync(LinkTarget, Buffer.from([42]), { flag: "wx", mode: 0o600 });
        fs.symlinkSync(LinkTarget, FixedPath, "file");
    }
    const NoLink = Run([FixedName]);
    assert.equal(NoLink.error, undefined, "no-link: process error");
    assert.equal(NoLink.signal, null, "no-link: signal");
    assert.equal(NoLink.status, 3, "no-link: exit status");
    assert.deepEqual(NoLink.stdout, Buffer.alloc(0), "no-link: stdout");
    assert.match(NoLink.stderr.toString("utf8"),
        /^file-read: directory status=/u, "no-link: stderr");

    process.stdout.write(
        `file-read application execution status=Passed host=${Host} cases=12\n`,
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
