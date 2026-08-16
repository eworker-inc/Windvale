import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const UNINSTALLER = path.join(SCRIPT_DIRECTORY, "Uninstall-Offline-Package-State.mjs");
const TRANSACTION = ".windvale-uninstall-1";
const RECORD = "windvale-uninstall 1\nowned state generations store\n";
const DIGEST_A = "11".repeat(32);
const DIGEST_B = "22".repeat(32);
const MAXIMUM_OWNED_ENTRIES = 4_096;
const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");

function Run(Command, InstallRoot) {
    return spawnSync(process.execPath, [UNINSTALLER, Command, InstallRoot], {
        encoding: "utf8",
        windowsHide: true,
    });
}

function Requireˉsuccess(Result, Description) {
    assert.equal(Result.status, 0, `${Description}: ${Result.stderr}`);
    assert.equal(Result.stderr, "", Description);
}

function Requireˉrejection(Result, Pattern, Description) {
    assert.equal(Result.status, 1, Description);
    assert.equal(Result.stdout, "", Description);
    assert.match(Result.stderr, Pattern, Description);
}

function Writeˉfile(FilePath, Value) {
    fs.mkdirSync(path.dirname(FilePath), { recursive: true, mode: 0o700 });
    fs.writeFileSync(FilePath, Value, { flag: "wx", mode: 0o600 });
}

function Populate(Parent, Name) {
    const Install = path.join(Parent, Name);
    fs.mkdirSync(Install, { mode: 0o700 });
    Writeˉfile(
        path.join(Install, "state", "Activation-1.txt"),
        `windvale-activation 1\nserial 3\ncurrent ${DIGEST_A}\nprevious ${DIGEST_B}\n`,
    );
    Writeˉfile(
        path.join(Install, "generations", DIGEST_A, "Generation-1.txt"),
        "windvale-generation 1\ntarget test-x64\n",
    );
    Writeˉfile(
        path.join(Install, "generations", DIGEST_B, "Generation-1.txt"),
        "windvale-generation 1\ntarget test-x64\n",
    );
    Writeˉfile(
        path.join(Install, "store", "objects", "sha256", "aa", "bb".repeat(31)),
        "object-bytes",
    );
    Writeˉfile(
        path.join(Install, "store", "bundles", "sha256", "cc",
            `${"dd".repeat(31)}.wvbundle`),
        "bundle-bytes",
    );
    Writeˉfile(path.join(Install, "application-data", "database.wvdb"), "user-data");
    Writeˉfile(path.join(Install, "notes.txt"), "unrelated-root-file");
    return Install;
}

function Assertˉpreserved(Install) {
    assert.equal(fs.statSync(Install).isDirectory(), true);
    assert.equal(Sha256(fs.readFileSync(path.join(
        Install, "application-data", "database.wvdb"))), Sha256("user-data"));
    assert.equal(Sha256(fs.readFileSync(path.join(Install, "notes.txt"))),
        Sha256("unrelated-root-file"));
}

function Assertˉownedˉabsent(Install) {
    for (const Name of ["state", "generations", "store", TRANSACTION]) {
        assert.equal(fs.existsSync(path.join(Install, Name)), false, Name);
    }
}

const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-offline-uninstall-"));
try {
    process.stdout.write("offline uninstall verification step=complete item=1/6\n");
    const Complete = Populate(Work, "Complete");
    const CompleteResult = Run("uninstall", Complete);
    Requireˉsuccess(CompleteResult, "complete uninstall");
    assert.match(CompleteResult.stdout, /result=removed/);
    Assertˉownedˉabsent(Complete);
    Assertˉpreserved(Complete);
    const RepeatResult = Run("uninstall", Complete);
    Requireˉsuccess(RepeatResult, "idempotent uninstall");
    assert.match(RepeatResult.stdout, /result=already-absent/);

    process.stdout.write("offline uninstall verification step=recovery item=2/6\n");
    const Interrupted = Populate(Work, "Interrupted");
    const InterruptedTransaction = path.join(Interrupted, TRANSACTION);
    fs.mkdirSync(InterruptedTransaction, { mode: 0o700 });
    Writeˉfile(path.join(InterruptedTransaction, "Uninstall-1.txt"), RECORD);
    fs.renameSync(path.join(Interrupted, "state"),
        path.join(InterruptedTransaction, "state"));
    const RecoveryResult = Run("recover", Interrupted);
    Requireˉsuccess(RecoveryResult, "interrupted uninstall recovery");
    assert.match(RecoveryResult.stdout, /recovery status=Complete result=removed/);
    Assertˉownedˉabsent(Interrupted);
    Assertˉpreserved(Interrupted);

    process.stdout.write("offline uninstall verification step=empty-transaction item=3/6\n");
    const Empty = Populate(Work, "Empty");
    fs.mkdirSync(path.join(Empty, TRANSACTION), { mode: 0o700 });
    const EmptyResult = Run("recover", Empty);
    Requireˉsuccess(EmptyResult, "empty transaction recovery");
    assert.match(EmptyResult.stdout, /result=aborted-empty/);
    assert.equal(fs.existsSync(path.join(Empty, "state")), true);
    assert.equal(fs.existsSync(path.join(Empty, TRANSACTION)), false);

    process.stdout.write("offline uninstall verification step=reject-link item=4/6\n");
    const Linked = Populate(Work, "Linked");
    const External = path.join(Work, "External-State");
    fs.renameSync(path.join(Linked, "state"), External);
    fs.symlinkSync(External, path.join(Linked, "state"),
        process.platform === "win32" ? "junction" : "dir");
    const LinkResult = Run("uninstall", Linked);
    Requireˉrejection(LinkResult, /ordinary directory/, "linked state rejection");
    assert.equal(fs.readFileSync(path.join(External, "Activation-1.txt"), "utf8")
        .startsWith("windvale-activation 1\n"), true);

    process.stdout.write("offline uninstall verification step=reject-inventory item=5/6\n");
    const UnknownState = Populate(Work, "Unknown-State");
    Writeˉfile(path.join(UnknownState, "state", "unknown.txt"), "unknown");
    Requireˉrejection(Run("uninstall", UnknownState), /unknown inventory/,
        "unknown state rejection");
    const UnknownGeneration = Populate(Work, "Unknown-Generation");
    fs.mkdirSync(path.join(UnknownGeneration, "generations", "unknown"));
    Requireˉrejection(Run("uninstall", UnknownGeneration), /unknown inventory/,
        "unknown generation rejection");
    const UnknownStore = Populate(Work, "Unknown-Store");
    Writeˉfile(path.join(UnknownStore, "store", "unknown.txt"), "unknown");
    Requireˉrejection(Run("uninstall", UnknownStore), /unknown inventory/,
        "unknown store rejection");
    const OversizedState = Populate(Work, "Oversized-State");
    for (let Index = 0; Index < MAXIMUM_OWNED_ENTRIES; Index += 1) {
        fs.writeFileSync(
            path.join(OversizedState, "state", `unknown-${Index.toString(16)}.txt`),
            "unknown",
            { flag: "wx", mode: 0o600 },
        );
    }
    Requireˉrejection(
        Run("uninstall", OversizedState),
        /exceeds the uninstall policy/,
        "oversized state rejection",
    );

    process.stdout.write("offline uninstall verification step=reject-transaction item=6/6\n");
    const Malformed = Populate(Work, "Malformed");
    fs.mkdirSync(path.join(Malformed, TRANSACTION), { mode: 0o700 });
    Writeˉfile(path.join(Malformed, TRANSACTION, "Uninstall-1.txt"),
        "windvale-uninstall 2\n");
    Requireˉrejection(Run("recover", Malformed), /not canonical/,
        "malformed transaction rejection");
    assert.equal(fs.existsSync(path.join(Malformed, "state")), true);

    process.stdout.write(
        "native offline package uninstall status=Passed cases=14 transactions=2 " +
        "recoveries=2 safety-rejections=6 preservation=2 idempotent=Verified\n",
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
