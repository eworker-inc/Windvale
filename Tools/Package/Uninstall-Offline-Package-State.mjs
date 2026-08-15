import fs from "node:fs";
import path from "node:path";

const OWNED_DIRECTORIES = ["state", "generations", "store"];
const TRANSACTION_DIRECTORY = ".windvale-uninstall-1";
const TRANSACTION_FILE = "Uninstall-1.txt";
const TRANSACTION_BYTES = Buffer.from(
    "windvale-uninstall 1\nowned state generations store\n",
    "utf8",
);
const SHA256 = /^[0-9a-f]{64}$/;
const FANOUT = /^[0-9a-f]{2}$/;
const OBJECT_LEAF = /^[0-9a-f]{62}$/;
const BUNDLE_LEAF = /^[0-9a-f]{62}\.wvbundle$/;
const MAXIMUM_RECORD_BYTES = 65_536;
const MAXIMUM_OWNED_FILE_BYTES = 4_194_304;
const MAXIMUM_OWNED_ENTRIES = 4_096;
const MAXIMUM_OWNED_BYTES = 268_435_456;

function Fail(Message) {
    throw new Error(Message);
}

function Exists(Value) {
    try {
        fs.lstatSync(Value);
        return true;
    } catch (ErrorValue) {
        if (ErrorValue.code === "ENOENT") return false;
        throw ErrorValue;
    }
}

function Assertˉordinaryˉdirectory(Value, Description) {
    const Resolved = path.resolve(Value);
    const Status = fs.lstatSync(Resolved);
    if (!Status.isDirectory() || Status.isSymbolicLink()) {
        Fail(`${Description} must be an ordinary directory.`);
    }
    return fs.realpathSync(Resolved);
}

function Readˉordinaryˉdirectory(DirectoryPath, Description) {
    const Status = fs.lstatSync(DirectoryPath);
    if (!Status.isDirectory() || Status.isSymbolicLink()) {
        Fail(`${Description} must be an ordinary directory.`);
    }
    return fs.readdirSync(DirectoryPath, { withFileTypes: true })
        .sort((Left, Right) => Left.name.localeCompare(Right.name, "en"));
}

function Assertˉordinaryˉfile(FilePath, Description, MaximumBytes, Budget) {
    const Status = fs.lstatSync(FilePath);
    if (!Status.isFile() || Status.isSymbolicLink() ||
        Status.size < 1 || Status.size > MaximumBytes) {
        Fail(`${Description} must be a bounded ordinary file.`);
    }
    Budget.entries += 1;
    Budget.bytes += Status.size;
    if (Budget.entries > MAXIMUM_OWNED_ENTRIES ||
        Budget.bytes > MAXIMUM_OWNED_BYTES) {
        Fail("Owned installation inventory exceeds the uninstall policy.");
    }
}

function Assertˉexactˉnames(Entries, Names, Description) {
    const Observed = Entries.map(Entry => Entry.name);
    const Expected = [...Names].sort((Left, Right) => Left.localeCompare(Right, "en"));
    if (Observed.length !== Expected.length ||
        Observed.some((Value, Index) => Value !== Expected[Index])) {
        Fail(`${Description} contains unknown inventory.`);
    }
}

function Validateˉstate(StateRoot, Budget) {
    const Entries = Readˉordinaryˉdirectory(StateRoot, "Installation state");
    Assertˉexactˉnames(Entries, ["Activation-1.txt"], "Installation state");
    Assertˉordinaryˉfile(
        path.join(StateRoot, "Activation-1.txt"),
        "Activation record",
        MAXIMUM_RECORD_BYTES,
        Budget,
    );
}

function Validateˉgenerations(GenerationsRoot, Budget) {
    const Entries = Readˉordinaryˉdirectory(GenerationsRoot, "Generation store");
    for (const Entry of Entries) {
        if (!SHA256.test(Entry.name) || !Entry.isDirectory() || Entry.isSymbolicLink()) {
            Fail("Generation store contains unknown inventory.");
        }
        Budget.entries += 1;
        const GenerationRoot = path.join(GenerationsRoot, Entry.name);
        const GenerationEntries = Readˉordinaryˉdirectory(
            GenerationRoot,
            "Immutable generation",
        );
        Assertˉexactˉnames(
            GenerationEntries,
            ["Generation-1.txt"],
            "Immutable generation",
        );
        Assertˉordinaryˉfile(
            path.join(GenerationRoot, "Generation-1.txt"),
            "Generation record",
            MAXIMUM_RECORD_BYTES,
            Budget,
        );
    }
}

function Validateˉdigestˉtree(Root, Bundle, Budget) {
    const RootEntries = Readˉordinaryˉdirectory(Root, "Package-store class");
    Assertˉexactˉnames(RootEntries, ["sha256"], "Package-store class");
    const ShaRoot = path.join(Root, "sha256");
    for (const FanoutEntry of Readˉordinaryˉdirectory(ShaRoot, "SHA-256 store")) {
        if (!FANOUT.test(FanoutEntry.name) ||
            !FanoutEntry.isDirectory() || FanoutEntry.isSymbolicLink()) {
            Fail("SHA-256 store contains unknown inventory.");
        }
        Budget.entries += 1;
        const FanoutRoot = path.join(ShaRoot, FanoutEntry.name);
        for (const LeafEntry of Readˉordinaryˉdirectory(FanoutRoot, "SHA-256 fanout")) {
            const AcceptedName = Bundle
                ? BUNDLE_LEAF.test(LeafEntry.name)
                : OBJECT_LEAF.test(LeafEntry.name);
            if (!AcceptedName || !LeafEntry.isFile() || LeafEntry.isSymbolicLink()) {
                Fail("SHA-256 fanout contains unknown inventory.");
            }
            Assertˉordinaryˉfile(
                path.join(FanoutRoot, LeafEntry.name),
                Bundle ? "Stored bundle" : "Stored object",
                MAXIMUM_OWNED_FILE_BYTES,
                Budget,
            );
        }
    }
}

function Validateˉstore(StoreRoot, Budget) {
    const Entries = Readˉordinaryˉdirectory(StoreRoot, "Package store");
    Assertˉexactˉnames(Entries, ["bundles", "objects"], "Package store");
    Validateˉdigestˉtree(path.join(StoreRoot, "objects"), false, Budget);
    Validateˉdigestˉtree(path.join(StoreRoot, "bundles"), true, Budget);
}

function Validateˉowned(OwnedName, OwnedPath) {
    const Budget = { entries: 1, bytes: 0 };
    if (OwnedName === "state") Validateˉstate(OwnedPath, Budget);
    else if (OwnedName === "generations") Validateˉgenerations(OwnedPath, Budget);
    else if (OwnedName === "store") Validateˉstore(OwnedPath, Budget);
    else Fail("Unknown owned installation class.");
    if (Budget.entries > MAXIMUM_OWNED_ENTRIES) {
        Fail("Owned installation inventory exceeds the uninstall policy.");
    }
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

function Flushˉrecord(FilePath) {
    const Descriptor = fs.openSync(FilePath, "wx", 0o600);
    try {
        let Offset = 0;
        while (Offset < TRANSACTION_BYTES.length) {
            const Written = fs.writeSync(
                Descriptor,
                TRANSACTION_BYTES,
                Offset,
                TRANSACTION_BYTES.length - Offset,
                Offset,
            );
            if (Written < 1) Fail("Uninstall transaction write made no progress.");
            Offset += Written;
        }
        fs.fsyncSync(Descriptor);
    } finally {
        fs.closeSync(Descriptor);
    }
}

function Readˉtransaction(TransactionRoot) {
    const Entries = Readˉordinaryˉdirectory(
        TransactionRoot,
        "Uninstall transaction",
    );
    const RecordPath = path.join(TransactionRoot, TRANSACTION_FILE);
    if (!Exists(RecordPath)) {
        if (Entries.length === 0) return undefined;
        Fail("Uninstall transaction has inventory but no durable record.");
    }
    const Allowed = new Set([TRANSACTION_FILE, ...OWNED_DIRECTORIES]);
    if (Entries.some(Entry => !Allowed.has(Entry.name))) {
        Fail("Uninstall transaction contains unknown inventory.");
    }
    const Budget = { entries: 0, bytes: 0 };
    Assertˉordinaryˉfile(
        RecordPath,
        "Uninstall transaction record",
        MAXIMUM_RECORD_BYTES,
        Budget,
    );
    if (!fs.readFileSync(RecordPath).equals(TRANSACTION_BYTES)) {
        Fail("Uninstall transaction record is not canonical.");
    }
    return RecordPath;
}

function Finishˉtransaction(InstallRoot, TransactionRoot, Recovery) {
    const RecordPath = Readˉtransaction(TransactionRoot);
    if (RecordPath === undefined) {
        fs.rmdirSync(TransactionRoot);
        const Durability = Flushˉdirectory(InstallRoot);
        process.stdout.write(
            `offline uninstall recovery status=Complete result=aborted-empty ` +
            `directory-durability=${Durability}\n`,
        );
        return;
    }

    for (const OwnedName of OWNED_DIRECTORIES) {
        const Source = path.join(InstallRoot, OwnedName);
        const Destination = path.join(TransactionRoot, OwnedName);
        if (Exists(Source) && Exists(Destination)) {
            Fail(`Uninstall transaction has duplicate ${OwnedName} inventory.`);
        }
        if (Exists(Source)) Validateˉowned(OwnedName, Source);
        if (Exists(Destination)) Validateˉowned(OwnedName, Destination);
    }

    let Moved = 0;
    let Durability = "confirmed";
    for (let Index = 0; Index < OWNED_DIRECTORIES.length; Index += 1) {
        const OwnedName = OWNED_DIRECTORIES[Index];
        const Source = path.join(InstallRoot, OwnedName);
        const Destination = path.join(TransactionRoot, OwnedName);
        process.stdout.write(
            `offline uninstall step=quarantine item=${Index + 1}/${OWNED_DIRECTORIES.length} ` +
            `owned=${OwnedName}\n`,
        );
        if (Exists(Source)) {
            fs.renameSync(Source, Destination);
            Moved += 1;
            if (Flushˉdirectory(TransactionRoot) === "unavailable" ||
                Flushˉdirectory(InstallRoot) === "unavailable") {
                Durability = "unavailable";
            }
        }
    }

    let Removed = 0;
    for (const OwnedName of OWNED_DIRECTORIES) {
        const Destination = path.join(TransactionRoot, OwnedName);
        if (!Exists(Destination)) continue;
        Validateˉowned(OwnedName, Destination);
        fs.rmSync(Destination, { recursive: true, force: false });
        Removed += 1;
        if (Flushˉdirectory(TransactionRoot) === "unavailable") {
            Durability = "unavailable";
        }
    }
    fs.unlinkSync(RecordPath);
    fs.rmdirSync(TransactionRoot);
    if (Flushˉdirectory(InstallRoot) === "unavailable") Durability = "unavailable";
    process.stdout.write(
        `offline uninstall ${Recovery ? "recovery " : ""}status=Complete ` +
        `result=removed moved=${Moved} removed=${Removed} preserved-root=yes ` +
        `directory-durability=${Durability}\n`,
    );
}

function Uninstall(InstallRootArgument) {
    const InstallRoot = Assertˉordinaryˉdirectory(
        InstallRootArgument,
        "Installation root",
    );
    const TransactionRoot = path.join(InstallRoot, TRANSACTION_DIRECTORY);
    if (Exists(TransactionRoot)) {
        Assertˉordinaryˉdirectory(TransactionRoot, "Uninstall transaction");
        Finishˉtransaction(InstallRoot, TransactionRoot, true);
        return;
    }

    let Present = 0;
    for (const OwnedName of OWNED_DIRECTORIES) {
        const OwnedPath = path.join(InstallRoot, OwnedName);
        if (!Exists(OwnedPath)) continue;
        Validateˉowned(OwnedName, OwnedPath);
        Present += 1;
    }
    if (Present === 0) {
        process.stdout.write(
            "offline uninstall status=Complete result=already-absent " +
            "preserved-root=yes directory-durability=not-required\n",
        );
        return;
    }

    fs.mkdirSync(TransactionRoot, { mode: 0o700 });
    const RecordPath = path.join(TransactionRoot, TRANSACTION_FILE);
    try {
        Flushˉrecord(RecordPath);
    } catch (ErrorValue) {
        if (Readˉordinaryˉdirectory(TransactionRoot, "Uninstall transaction").length === 0) {
            fs.rmdirSync(TransactionRoot);
        }
        throw ErrorValue;
    }
    Flushˉdirectory(TransactionRoot);
    Flushˉdirectory(InstallRoot);
    process.stdout.write(`offline uninstall step=transaction-created owned=${Present}\n`);
    Finishˉtransaction(InstallRoot, TransactionRoot, false);
}

function Recover(InstallRootArgument) {
    const InstallRoot = Assertˉordinaryˉdirectory(
        InstallRootArgument,
        "Installation root",
    );
    const TransactionRoot = path.join(InstallRoot, TRANSACTION_DIRECTORY);
    if (!Exists(TransactionRoot)) {
        process.stdout.write(
            "offline uninstall recovery status=Complete result=none " +
            "directory-durability=not-required\n",
        );
        return;
    }
    Assertˉordinaryˉdirectory(TransactionRoot, "Uninstall transaction");
    Finishˉtransaction(InstallRoot, TransactionRoot, true);
}

const [Command, ...Arguments] = process.argv.slice(2);
try {
    if (Command === "uninstall" && Arguments.length === 1) {
        Uninstall(Arguments[0]);
    } else if (Command === "recover" && Arguments.length === 1) {
        Recover(Arguments[0]);
    } else {
        process.stderr.write(
            "Usage: node Uninstall-Offline-Package-State.mjs " +
            "<uninstall|recover> <install-root>\n",
        );
        process.exitCode = 64;
    }
} catch (ErrorValue) {
    process.stderr.write(`${ErrorValue.message}\n`);
    process.exitCode = 1;
}
