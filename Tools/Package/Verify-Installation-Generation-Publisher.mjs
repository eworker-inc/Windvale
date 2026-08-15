import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const PUBLISHER = path.join(SCRIPT_DIRECTORY, "Publish-Installation-Generation.mjs");
const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");

function Run(...Arguments) {
    return spawnSync(process.execPath, [PUBLISHER, ...Arguments], {
        encoding: "utf8",
        windowsHide: true,
    });
}

function Requireˉsuccess(Result, Description) {
    assert.equal(Result.status, 0, `${Description}: ${Result.stderr}`);
    assert.equal(Result.stderr, "", Description);
}

const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-generation-publication-"));
try {
    const InstallRoot = path.join(Work, "Installed");
    const InputRoot = path.join(Work, "Input");
    fs.mkdirSync(InstallRoot);
    fs.mkdirSync(InputRoot);
    const Record = Buffer.from(
        "windvale-generation 1\n" +
        "target windows-x64\n" +
        `package windvale.example 1.0.0 ${"1".repeat(64)} ${"2".repeat(64)}\n` +
        `command example windvale.example application ${"3".repeat(64)} ${"4".repeat(64)}\n`,
        "utf8",
    );
    const Digest = Sha256(Record);
    const RecordPath = path.join(InputRoot, "Generation-1.txt");
    fs.writeFileSync(RecordPath, Record, { flag: "wx" });
    const PublishedRoot = path.join(InstallRoot, "generations", Digest);
    const PublishedRecord = path.join(PublishedRoot, "Generation-1.txt");

    process.stdout.write("generation publisher step=identity-rejection item=1/8\n");
    const WrongIdentity = Run("publish", InstallRoot, RecordPath, "0".repeat(64));
    assert.equal(WrongIdentity.status, 1);
    assert.match(WrongIdentity.stderr, /input identity differs/);
    assert.equal(fs.existsSync(path.join(InstallRoot, "generations")), false);

    process.stdout.write("generation publisher step=initial-publication item=2/8\n");
    const Initial = Run("publish", InstallRoot, RecordPath, Digest);
    Requireˉsuccess(Initial, "initial generation publication");
    assert.match(Initial.stdout, /status=Complete result=changed/);
    assert.deepEqual(fs.readFileSync(PublishedRecord), Record);

    process.stdout.write("generation publisher step=idempotent-publication item=3/8\n");
    const Idempotent = Run("publish", InstallRoot, RecordPath, Digest);
    Requireˉsuccess(Idempotent, "idempotent generation publication");
    assert.match(Idempotent.stdout, /status=Complete result=unchanged/);

    process.stdout.write("generation publisher step=exact-verification item=4/8\n");
    const Verified = Run("verify", InstallRoot, Digest);
    Requireˉsuccess(Verified, "generation verification");
    assert.match(Verified.stdout, new RegExp(`status=Valid generation=${Digest}`));

    process.stdout.write("generation publisher step=tamper-rejection item=5/8\n");
    fs.appendFileSync(PublishedRecord, "x");
    const Tampered = Run("verify", InstallRoot, Digest);
    assert.equal(Tampered.status, 1);
    assert.match(Tampered.stderr, /identity differs/);
    fs.writeFileSync(PublishedRecord, Record);

    process.stdout.write("generation publisher step=interruption-recovery item=6/8\n");
    const Candidate = path.join(InstallRoot, "generations", `.candidate-${Digest}`);
    fs.mkdirSync(Candidate);
    fs.writeFileSync(path.join(Candidate, "Generation-1.txt"), Record);
    const Recovery = Run("recover", InstallRoot);
    Requireˉsuccess(Recovery, "generation recovery");
    assert.match(Recovery.stdout, /cleaned=1/);
    assert.equal(fs.existsSync(Candidate), false);

    process.stdout.write("generation publisher step=corrupt-candidate-rejection item=7/8\n");
    fs.mkdirSync(Candidate);
    fs.writeFileSync(path.join(Candidate, "Generation-1.txt"), Buffer.from("wrong\n"));
    const Corrupt = Run("recover", InstallRoot);
    assert.equal(Corrupt.status, 1);
    assert.match(Corrupt.stderr, /candidate identity differs/);
    assert.equal(fs.existsSync(Candidate), true);
    fs.unlinkSync(path.join(Candidate, "Generation-1.txt"));
    fs.rmdirSync(Candidate);

    process.stdout.write("generation publisher step=inventory-rejection item=8/8\n");
    fs.writeFileSync(path.join(PublishedRoot, "extra"), "x");
    const Extra = Run("verify", InstallRoot, Digest);
    assert.equal(Extra.status, 1);
    assert.match(Extra.stderr, /inventory differs/);

    process.stdout.write(
        `native installation generation publication status=Passed cases=8 ` +
        `generation=${Digest} bytes=${Record.length}\n`,
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
