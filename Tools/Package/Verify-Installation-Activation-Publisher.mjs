import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const PUBLISHER = path.join(SCRIPT_DIRECTORY, "Publish-Installation-Activation.mjs");
const CANDIDATE_PREFIX = "Activation-1.candidate-";
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

const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-activation-"));
try {
    const InstallRoot = path.join(Work, "Installed");
    const InputRoot = path.join(Work, "Input");
    fs.mkdirSync(InstallRoot);
    fs.mkdirSync(InputRoot);
    const GenerationA = "1".repeat(64);
    const GenerationB = "2".repeat(64);
    const ActivationA = Buffer.from(
        "windvale-activation 1\n" +
        "serial 1\n" +
        `current ${GenerationA}\n` +
        "previous none\n",
        "utf8",
    );
    const ActivationB = Buffer.from(
        "windvale-activation 1\n" +
        "serial 2\n" +
        `current ${GenerationB}\n` +
        `previous ${GenerationA}\n`,
        "utf8",
    );
    const ActivationRollback = Buffer.from(
        "windvale-activation 1\n" +
        "serial 3\n" +
        `current ${GenerationA}\n` +
        `previous ${GenerationB}\n`,
        "utf8",
    );
    const Records = [ActivationA, ActivationB, ActivationRollback];
    const Paths = [];
    const Digests = [];
    for (let Index = 0; Index < Records.length; Index++) {
        const RecordPath = path.join(InputRoot, `Activation-${Index + 1}.txt`);
        fs.writeFileSync(RecordPath, Records[Index], { flag: "wx" });
        Paths.push(RecordPath);
        Digests.push(Sha256(Records[Index]));
    }
    const PublicPath = path.join(InstallRoot, "state", "Activation-1.txt");

    process.stdout.write("activation publisher step=initial-publication item=1/7\n");
    const Initial = Run("publish", InstallRoot, "none", Paths[0], Digests[0]);
    Requireˉsuccess(Initial, "initial publication");
    assert.match(Initial.stdout, /status=Complete result=changed/);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationA);

    process.stdout.write("activation publisher step=idempotent-publication item=2/7\n");
    const Idempotent = Run("publish", InstallRoot, Digests[0], Paths[0], Digests[0]);
    Requireˉsuccess(Idempotent, "idempotent publication");
    assert.match(Idempotent.stdout, /status=Complete result=unchanged/);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationA);

    process.stdout.write("activation publisher step=effective-replacement item=3/7\n");
    const Replacement = Run("publish", InstallRoot, Digests[0], Paths[1], Digests[1]);
    Requireˉsuccess(Replacement, "effective replacement");
    assert.match(Replacement.stdout, /status=Complete result=changed/);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationB);

    process.stdout.write("activation publisher step=stale-writer-rejection item=4/7\n");
    const Stale = Run("publish", InstallRoot, Digests[0], Paths[2], Digests[2]);
    assert.equal(Stale.status, 1);
    assert.match(Stale.stderr, /current activation identity/);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationB);

    process.stdout.write("activation publisher step=interruption-recovery item=5/7\n");
    const CandidatePath = path.join(
        InstallRoot,
        "state",
        `${CANDIDATE_PREFIX}${Digests[2]}`,
    );
    fs.writeFileSync(CandidatePath, ActivationRollback, { flag: "wx" });
    const Recovery = Run("recover", InstallRoot);
    Requireˉsuccess(Recovery, "interruption recovery");
    assert.match(Recovery.stdout, /cleaned=1/);
    assert.equal(fs.existsSync(CandidatePath), false);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationB);

    process.stdout.write("activation publisher step=corrupt-candidate-rejection item=6/7\n");
    fs.writeFileSync(CandidatePath, ActivationA, { flag: "wx" });
    const CorruptRecovery = Run("recover", InstallRoot);
    assert.equal(CorruptRecovery.status, 1);
    assert.match(CorruptRecovery.stderr, /candidate identity differs/);
    assert.equal(fs.existsSync(CandidatePath), true);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationB);
    fs.unlinkSync(CandidatePath);

    process.stdout.write("activation publisher step=rollback-publication item=7/7\n");
    const Rollback = Run("publish", InstallRoot, Digests[1], Paths[2], Digests[2]);
    Requireˉsuccess(Rollback, "rollback publication");
    assert.match(Rollback.stdout, /status=Complete result=changed/);
    assert.deepEqual(fs.readFileSync(PublicPath), ActivationRollback);
    const EmptyRecovery = Run("recover", InstallRoot);
    Requireˉsuccess(EmptyRecovery, "empty recovery");
    assert.match(EmptyRecovery.stdout, /cleaned=0/);

    process.stdout.write(
        `native installation activation status=Passed cases=16 records=3 ` +
        `current=${Digests[2]}\n`,
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
