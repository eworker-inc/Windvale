import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const SHA256 = /^[0-9a-f]{64}$/;
const MAXIMUM_RECORD_BYTES = 65_536;
const STATE_DIRECTORY = "state";
const ACTIVATION_FILE = "Activation-1.txt";
const CANDIDATE_PREFIX = "Activation-1.candidate-";

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Fail(Message) {
    throw new Error(Message);
}

function Assertˉsha256(Value, Description) {
    if (!SHA256.test(Value ?? "")) {
        Fail(`Invalid ${Description}.`);
    }
    return Value;
}

function Assertˉordinaryˉdirectory(Value, Description) {
    const Resolved = path.resolve(Value);
    const Status = fs.lstatSync(Resolved);
    if (!Status.isDirectory() || Status.isSymbolicLink()) {
        Fail(`${Description} must be an ordinary directory.`);
    }
    return fs.realpathSync(Resolved);
}

function Readˉboundedˉordinaryˉfile(FilePath, Description, Optional = false) {
    let Status;
    try {
        Status = fs.lstatSync(FilePath);
    } catch (ErrorValue) {
        if (Optional && ErrorValue.code === "ENOENT") return undefined;
        throw ErrorValue;
    }
    if (!Status.isFile() || Status.isSymbolicLink() ||
        Status.size < 1 || Status.size > MAXIMUM_RECORD_BYTES) {
        Fail(`${Description} is not a bounded ordinary file.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== Status.size) {
        Fail(`${Description} changed while it was read.`);
    }
    return Bytes;
}

function Flushˉdirectory(DirectoryPath) {
    let Descriptor;
    try {
        Descriptor = fs.openSync(DirectoryPath, "r");
        fs.fsyncSync(Descriptor);
        return "confirmed";
    } catch (ErrorValue) {
        if (["EACCES", "EBADF", "EINVAL", "EISDIR", "ENOTSUP", "EPERM"]
            .includes(ErrorValue.code)) {
            return "unavailable";
        }
        throw ErrorValue;
    } finally {
        if (Descriptor !== undefined) fs.closeSync(Descriptor);
    }
}

function Openˉstate(InstallRoot, Create) {
    const Root = Assertˉordinaryˉdirectory(InstallRoot, "Installation root");
    const StateRoot = path.join(Root, STATE_DIRECTORY);
    try {
        const Status = fs.lstatSync(StateRoot);
        if (!Status.isDirectory() || Status.isSymbolicLink()) {
            Fail("Installation state must be an ordinary directory.");
        }
    } catch (ErrorValue) {
        if (!Create || ErrorValue.code !== "ENOENT") throw ErrorValue;
        fs.mkdirSync(StateRoot, { mode: 0o700 });
        Flushˉdirectory(Root);
    }
    return fs.realpathSync(StateRoot);
}

function Findˉcandidates(StateRoot) {
    return fs.readdirSync(StateRoot, { withFileTypes: true })
        .filter(Entry => Entry.name.startsWith(CANDIDATE_PREFIX))
        .map(Entry => {
            if (!Entry.isFile() || Entry.isSymbolicLink()) {
                Fail("An activation candidate is not an ordinary file.");
            }
            const Digest = Entry.name.slice(CANDIDATE_PREFIX.length);
            Assertˉsha256(Digest, "activation candidate identity");
            return { path: path.join(StateRoot, Entry.name), digest: Digest };
        });
}

function Publish(InstallRoot, ExpectedCurrent, NextPath, ExpectedNext) {
    if (ExpectedCurrent !== "none") {
        Assertˉsha256(ExpectedCurrent, "expected current activation identity");
    }
    Assertˉsha256(ExpectedNext, "next activation identity");
    const NextBytes = Readˉboundedˉordinaryˉfile(
        path.resolve(NextPath),
        "Next activation record",
    );
    if (Sha256(NextBytes) !== ExpectedNext) {
        Fail("The next activation record identity changed.");
    }

    const StateRoot = Openˉstate(InstallRoot, true);
    const ActivationPath = path.join(StateRoot, ACTIVATION_FILE);
    const ExistingCandidates = Findˉcandidates(StateRoot);
    if (ExistingCandidates.length !== 0) {
        Fail("An interrupted activation candidate requires recovery.");
    }

    const CurrentBytes = Readˉboundedˉordinaryˉfile(
        ActivationPath,
        "Current activation record",
        true,
    );
    const CurrentDigest = CurrentBytes === undefined ? "none" : Sha256(CurrentBytes);
    if (CurrentDigest !== ExpectedCurrent) {
        Fail(`The current activation identity is ${CurrentDigest}, not ${ExpectedCurrent}.`);
    }
    if (CurrentDigest === ExpectedNext) {
        process.stdout.write(
            `activation publish status=Complete result=unchanged current=${ExpectedNext} ` +
            "directory-durability=not-required\n",
        );
        return;
    }

    const CandidatePath = path.join(StateRoot, `${CANDIDATE_PREFIX}${ExpectedNext}`);
    let CandidateOwned = false;
    let DestinationReplaced = false;
    try {
        const Descriptor = fs.openSync(CandidatePath, "wx", 0o600);
        CandidateOwned = true;
        try {
            let Offset = 0;
            while (Offset < NextBytes.length) {
                const Written = fs.writeSync(
                    Descriptor,
                    NextBytes,
                    Offset,
                    NextBytes.length - Offset,
                    Offset,
                );
                if (Written < 1) {
                    Fail("The activation candidate write made no progress.");
                }
                Offset += Written;
            }
            fs.fsyncSync(Descriptor);
        } finally {
            fs.closeSync(Descriptor);
        }
        const VerifiedBytes = Readˉboundedˉordinaryˉfile(
            CandidatePath,
            "Activation candidate",
        );
        if (!VerifiedBytes.equals(NextBytes) || Sha256(VerifiedBytes) !== ExpectedNext) {
            Fail("The durable activation candidate differs after reread.");
        }
        fs.renameSync(CandidatePath, ActivationPath);
        CandidateOwned = false;
        DestinationReplaced = true;
        const DirectoryDurability = Flushˉdirectory(StateRoot);
        process.stdout.write(
            `activation publish status=Complete result=changed current=${ExpectedNext} ` +
            `directory-durability=${DirectoryDurability}\n`,
        );
    } catch (ErrorValue) {
        if (DestinationReplaced) {
            process.stderr.write(
                `activation publish status=Indeterminate current=${ExpectedNext} ` +
                `reason=${JSON.stringify(ErrorValue.message)}\n`,
            );
            process.exitCode = 2;
            return;
        }
        if (CandidateOwned) {
            try {
                fs.unlinkSync(CandidatePath);
                CandidateOwned = false;
            } catch (CleanupError) {
                process.stderr.write(
                    "activation publish status=Cleanup-required " +
                    `reason=${JSON.stringify(CleanupError.message)}\n`,
                );
                process.exitCode = 1;
                return;
            }
        }
        throw ErrorValue;
    }
}

function Recover(InstallRoot) {
    const StateRoot = Openˉstate(InstallRoot, false);
    const ActivationPath = path.join(StateRoot, ACTIVATION_FILE);
    const CurrentBytes = Readˉboundedˉordinaryˉfile(
        ActivationPath,
        "Current activation record",
        true,
    );
    const CurrentDigest = CurrentBytes === undefined ? "none" : Sha256(CurrentBytes);
    const Candidates = Findˉcandidates(StateRoot);
    if (Candidates.length > 1) {
        Fail("Multiple interrupted activation candidates require inspection.");
    }
    if (Candidates.length === 0) {
        process.stdout.write(
            `activation recovery status=Complete current=${CurrentDigest} cleaned=0 ` +
            "directory-durability=not-required\n",
        );
        return;
    }

    const Candidate = Candidates[0];
    const CandidateBytes = Readˉboundedˉordinaryˉfile(
        Candidate.path,
        "Activation candidate",
    );
    if (Sha256(CandidateBytes) !== Candidate.digest) {
        Fail("The interrupted activation candidate identity differs.");
    }
    fs.unlinkSync(Candidate.path);
    const DirectoryDurability = Flushˉdirectory(StateRoot);
    process.stdout.write(
        `activation recovery status=Complete current=${CurrentDigest} cleaned=1 ` +
        `directory-durability=${DirectoryDurability}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "publish" && Arguments.length === 4) {
        Publish(Arguments[0], Arguments[1], Arguments[2], Arguments[3]);
    } else if (Command === "recover" && Arguments.length === 1) {
        Recover(Arguments[0]);
    } else {
        process.stderr.write(
            "Usage: node Publish-Installation-Activation.mjs " +
            "<publish install-root expected-current-sha256|none " +
            "next-record expected-next-sha256|recover install-root>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
