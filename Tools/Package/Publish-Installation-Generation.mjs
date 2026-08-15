import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";

const SHA256 = /^[0-9a-f]{64}$/;
const MAXIMUM_RECORD_BYTES = 65_536;
const GENERATIONS_DIRECTORY = "generations";
const RECORD_FILE = "Generation-1.txt";
const CANDIDATE_PREFIX = ".candidate-";

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Fail(Message) {
    throw new Error(Message);
}

function Assertˉsha256(Value, Description) {
    if (!SHA256.test(Value ?? "")) Fail(`Invalid ${Description}.`);
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

function Readˉrecord(FilePath, Description) {
    const Status = fs.lstatSync(FilePath);
    if (!Status.isFile() || Status.isSymbolicLink() ||
        Status.size < 1 || Status.size > MAXIMUM_RECORD_BYTES) {
        Fail(`${Description} is not a bounded ordinary file.`);
    }
    const Bytes = fs.readFileSync(FilePath);
    if (Bytes.length !== Status.size) Fail(`${Description} changed while it was read.`);
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

function Openˉgenerations(InstallRoot, Create) {
    const Root = Assertˉordinaryˉdirectory(InstallRoot, "Installation root");
    const Generations = path.join(Root, GENERATIONS_DIRECTORY);
    try {
        Assertˉordinaryˉdirectory(Generations, "Generations root");
    } catch (ErrorValue) {
        if (!Create || ErrorValue.code !== "ENOENT") throw ErrorValue;
        fs.mkdirSync(Generations, { mode: 0o700 });
        Flushˉdirectory(Root);
    }
    return fs.realpathSync(Generations);
}

function Readˉpublished(GenerationPath, ExpectedDigest) {
    const Root = Assertˉordinaryˉdirectory(GenerationPath, "Published generation");
    const Entries = fs.readdirSync(Root, { withFileTypes: true });
    if (Entries.length !== 1 || Entries[0].name !== RECORD_FILE ||
        !Entries[0].isFile() || Entries[0].isSymbolicLink()) {
        Fail("Published generation inventory differs.");
    }
    const Bytes = Readˉrecord(path.join(Root, RECORD_FILE), "Published generation record");
    if (Sha256(Bytes) !== ExpectedDigest) {
        Fail("Published generation identity differs.");
    }
    return Bytes;
}

function Removeˉcandidate(CandidatePath) {
    const RecordPath = path.join(CandidatePath, RECORD_FILE);
    if (fs.existsSync(RecordPath)) fs.unlinkSync(RecordPath);
    fs.rmdirSync(CandidatePath);
}

function Publish(InstallRoot, RecordPath, ExpectedDigest) {
    Assertˉsha256(ExpectedDigest, "generation identity");
    const InputBytes = Readˉrecord(path.resolve(RecordPath), "Generation input");
    if (Sha256(InputBytes) !== ExpectedDigest) Fail("Generation input identity differs.");
    const Generations = Openˉgenerations(InstallRoot, true);
    const PublishedPath = path.join(Generations, ExpectedDigest);
    try {
        const Existing = Readˉpublished(PublishedPath, ExpectedDigest);
        if (!Existing.equals(InputBytes)) Fail("Published generation bytes differ.");
        process.stdout.write(
            `generation publish status=Complete result=unchanged generation=${ExpectedDigest} ` +
            "directory-durability=not-required\n",
        );
        return;
    } catch (ErrorValue) {
        if (ErrorValue.code !== "ENOENT") throw ErrorValue;
    }

    const CandidatePath = path.join(Generations, `${CANDIDATE_PREFIX}${ExpectedDigest}`);
    let CandidateOwned = false;
    let Published = false;
    try {
        fs.mkdirSync(CandidatePath, { mode: 0o700 });
        CandidateOwned = true;
        const CandidateRecord = path.join(CandidatePath, RECORD_FILE);
        const Descriptor = fs.openSync(CandidateRecord, "wx", 0o600);
        try {
            let Offset = 0;
            while (Offset < InputBytes.length) {
                const Written = fs.writeSync(
                    Descriptor,
                    InputBytes,
                    Offset,
                    InputBytes.length - Offset,
                    Offset,
                );
                if (Written < 1) Fail("The generation write made no progress.");
                Offset += Written;
            }
            fs.fsyncSync(Descriptor);
        } finally {
            fs.closeSync(Descriptor);
        }
        const Verified = Readˉrecord(CandidateRecord, "Generation candidate");
        if (!Verified.equals(InputBytes) || Sha256(Verified) !== ExpectedDigest) {
            Fail("Generation candidate differs after reread.");
        }
        fs.renameSync(CandidatePath, PublishedPath);
        CandidateOwned = false;
        Published = true;
        const DirectoryDurability = Flushˉdirectory(Generations);
        process.stdout.write(
            `generation publish status=Complete result=changed generation=${ExpectedDigest} ` +
            `directory-durability=${DirectoryDurability}\n`,
        );
    } catch (ErrorValue) {
        if (Published) {
            process.stderr.write(
                `generation publish status=Indeterminate generation=${ExpectedDigest} ` +
                `reason=${JSON.stringify(ErrorValue.message)}\n`,
            );
            process.exitCode = 2;
            return;
        }
        if (CandidateOwned) Removeˉcandidate(CandidatePath);
        throw ErrorValue;
    }
}

function Verify(InstallRoot, Digest) {
    Assertˉsha256(Digest, "generation identity");
    const Generations = Openˉgenerations(InstallRoot, false);
    const Bytes = Readˉpublished(path.join(Generations, Digest), Digest);
    process.stdout.write(
        `generation verify status=Valid generation=${Digest} bytes=${Bytes.length}\n`,
    );
}

function Recover(InstallRoot) {
    const Generations = Openˉgenerations(InstallRoot, false);
    const Candidates = fs.readdirSync(Generations, { withFileTypes: true })
        .filter(Entry => Entry.name.startsWith(CANDIDATE_PREFIX));
    if (Candidates.length > 1) Fail("Multiple generation candidates require inspection.");
    if (Candidates.length === 0) {
        process.stdout.write(
            "generation recovery status=Complete cleaned=0 directory-durability=not-required\n",
        );
        return;
    }
    const Candidate = Candidates[0];
    if (!Candidate.isDirectory() || Candidate.isSymbolicLink()) {
        Fail("Generation candidate is not an ordinary directory.");
    }
    const Digest = Assertˉsha256(
        Candidate.name.slice(CANDIDATE_PREFIX.length),
        "generation candidate identity",
    );
    const CandidatePath = path.join(Generations, Candidate.name);
    const Entries = fs.readdirSync(CandidatePath, { withFileTypes: true });
    if (Entries.length !== 1 || Entries[0].name !== RECORD_FILE ||
        !Entries[0].isFile() || Entries[0].isSymbolicLink()) {
        Fail("Generation candidate inventory differs.");
    }
    const Bytes = Readˉrecord(path.join(CandidatePath, RECORD_FILE), "Generation candidate");
    if (Sha256(Bytes) !== Digest) Fail("Generation candidate identity differs.");
    Removeˉcandidate(CandidatePath);
    const DirectoryDurability = Flushˉdirectory(Generations);
    process.stdout.write(
        `generation recovery status=Complete cleaned=1 directory-durability=${DirectoryDurability}\n`,
    );
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "publish" && Arguments.length === 3) {
        Publish(...Arguments);
    } else if (Command === "verify" && Arguments.length === 2) {
        Verify(...Arguments);
    } else if (Command === "recover" && Arguments.length === 1) {
        Recover(...Arguments);
    } else {
        process.stderr.write(
            "Usage: node Publish-Installation-Generation.mjs " +
            "<publish install-root generation-record sha256|verify install-root sha256|" +
            "recover install-root>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
