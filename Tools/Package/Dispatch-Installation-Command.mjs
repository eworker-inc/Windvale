import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

const SHA256 = /^[0-9a-f]{64}$/;
const TOKEN = /^[A-Za-z0-9._-]+$/;
const MAXIMUM_TEXT_BYTES = 65_536;
const MAXIMUM_BUNDLE_BYTES = 16 * 1_024 * 1_024;
const MAXIMUM_HOST_BYTES = 64 * 1_024 * 1_024;

function Fail(Reason) {
    throw new Error(Reason);
}

function Sha256(Value) {
    return crypto.createHash("sha256").update(Value).digest("hex");
}

function Readˉordinary(FilePath, MaximumBytes, Description) {
    const Resolved = path.resolve(FilePath);
    const Status = fs.lstatSync(Resolved);
    if (!Status.isFile() || Status.isSymbolicLink() || Status.size < 1 ||
        Status.size > MaximumBytes) {
        Fail(`${Description}-not-bounded-ordinary-file`);
    }
    const Bytes = fs.readFileSync(Resolved);
    if (Bytes.length !== Status.size) Fail(`${Description}-changed-during-read`);
    return { path: Resolved, bytes: Bytes };
}

function Readˉcanonical(FilePath, Description) {
    const Value = Readˉordinary(FilePath, MAXIMUM_TEXT_BYTES, Description);
    const Text = Value.bytes.toString("utf8");
    if (!Buffer.from(Text, "utf8").equals(Value.bytes) || Text.includes("\r") ||
        Text.includes("\0") || !Text.endsWith("\n")) {
        Fail(`${Description}-not-canonical-lf-utf8`);
    }
    return { ...Value, lines: Text.slice(0, -1).split("\n") };
}

function Openˉordinaryˉdirectory(DirectoryPath, Description) {
    const Resolved = path.resolve(DirectoryPath);
    const Status = fs.lstatSync(Resolved);
    if (!Status.isDirectory() || Status.isSymbolicLink()) {
        Fail(`${Description}-not-ordinary-directory`);
    }
    return fs.realpathSync(Resolved);
}

function Fields(Line, Prefix, Count, Description) {
    if (!Line.startsWith(`${Prefix} `)) Fail(`${Description}-missing-${Prefix}`);
    const Values = Line.slice(Prefix.length + 1).split(" ");
    if (Values.length !== Count || Values.some(Value => Value.length === 0)) {
        Fail(`${Description}-invalid-${Prefix}`);
    }
    return Values;
}

function One(Record, Prefix, Count, Description) {
    const Matches = Record.lines.filter(Line => Line.startsWith(`${Prefix} `));
    if (Matches.length !== 1) Fail(`${Description}-invalid-${Prefix}-count`);
    return Fields(Matches[0], Prefix, Count, Description);
}

function Identity(Values, Description) {
    const [Digest, BytesText] = Values;
    const Bytes = Number(BytesText);
    if (!SHA256.test(Digest) || !/^(0|[1-9][0-9]*)$/.test(BytesText) ||
        !Number.isSafeInteger(Bytes) || Bytes < 1) {
        Fail(`${Description}-invalid-identity`);
    }
    return { digest: Digest, bytes: Bytes };
}

function Requireˉidentity(Value, Expected, Description) {
    if (Value.bytes.length !== Expected.bytes || Sha256(Value.bytes) !== Expected.digest) {
        Fail(`${Description}-identity-mismatch`);
    }
}

function Resolve(ResolverPath, ActivationPath, GenerationPath, Target, Command) {
    const Result = spawnSync(
        ResolverPath,
        [ActivationPath, GenerationPath, Target, Command],
        { encoding: "utf8", windowsHide: true },
    );
    if (Result.error) Fail("resolver-unavailable");
    if (Result.status !== 0 || Result.stderr !== "") Fail("resolver-rejected-selection");
    const Match = /^command resolution status=Valid generation=([0-9a-f]{64}) target=([A-Za-z0-9._-]+) command=([A-Za-z0-9._-]+) package=([A-Za-z0-9._-]+) part=([A-Za-z0-9._-]+) approval=([0-9a-f]{64}) launch=([0-9a-f]{64})\n$/.exec(Result.stdout);
    if (!Match) Fail("resolver-report-invalid");
    return {
        generation: Match[1], target: Match[2], command: Match[3],
        package: Match[4], part: Match[5], approval: Match[6], launch: Match[7],
    };
}

function Activeˉgeneration(InstallRoot) {
    const Root = Openˉordinaryˉdirectory(InstallRoot, "installation-root");
    const State = Openˉordinaryˉdirectory(path.join(Root, "state"), "state-root");
    const Generations = Openˉordinaryˉdirectory(
        path.join(Root, "generations"), "generations-root",
    );
    const Activation = Readˉcanonical(
        path.join(State, "Activation-1.txt"),
        "activation",
    );
    if (Activation.lines.length !== 4 || Activation.lines[0] !== "windvale-activation 1") {
        Fail("activation-invalid");
    }
    const Current = Fields(Activation.lines[2], "current", 1, "activation")[0];
    if (!SHA256.test(Current)) Fail("activation-invalid-current");
    const GenerationRoot = Openˉordinaryˉdirectory(
        path.join(Generations, Current), "generation-root",
    );
    const Generation = Readˉcanonical(
        path.join(GenerationRoot, "Generation-1.txt"),
        "generation",
    );
    if (Sha256(Generation.bytes) !== Current) Fail("generation-identity-mismatch");
    return { root: Root, activation: Activation, current: Current, generation: Generation };
}

function Selectedˉpackage(Generation, Selection) {
    const Matches = Generation.lines.filter(Line => Line.startsWith(`package ${Selection.package} `));
    if (Matches.length !== 1) Fail("generation-package-closure-mismatch");
    const Values = Matches[0].split(" ");
    if (Values.length !== 5 || Values[0] !== "package" ||
        !TOKEN.test(Values[1]) || !TOKEN.test(Values[2]) ||
        !SHA256.test(Values[3]) || !SHA256.test(Values[4])) {
        Fail("generation-package-invalid");
    }
    return { name: Values[1], version: Values[2], bundle: Values[3], lock: Values[4] };
}

function Validateˉarguments(Selection, Approval, Launch, Arguments) {
    const Application = One(Launch, "application", 2, "launch")[0];
    if (Selection.command === "echo" && Selection.package === "windvale.echo" &&
        Selection.part === "application" && Application === "windvale.echo") {
        const ExpectedApprovals = [
            "approve 0 console.write_line standard-output-line-v1",
            "approve 1 process.argument immutable-argument-snapshot-v1",
            "approve 2 process.argument_count immutable-argument-snapshot-v1",
        ];
        const ExpectedBindings = [
            "bind 0 console.write_line host-standard-output line-lf",
            "bind 1 process.argument immutable-launch-arguments 0 67 4096 65536",
            "bind 2 process.argument_count immutable-launch-arguments 0 67 4096 65536",
        ];
        const Approvals = Approval.lines.filter(Line => Line.startsWith("approve "));
        const Bindings = Launch.lines.filter(Line => Line.startsWith("bind "));
        if (Approval.lines[0] !== "windvale-capability-approval 1" ||
            !Approval.lines.includes("capability-count 3") ||
            Approvals.length !== ExpectedApprovals.length ||
            ExpectedApprovals.some((Line, Index) => Approvals[Index] !== Line) ||
            Launch.lines[0] !== "windvale-launch-record 3" ||
            !Launch.lines.includes("entry Main") ||
            !Launch.lines.includes("provider-table 3 3") ||
            Bindings.length !== ExpectedBindings.length ||
            ExpectedBindings.some((Line, Index) => Bindings[Index] !== Line) ||
            !Launch.lines.includes("argument-vector strict-utf8 0 67 4096 65536")) {
            Fail("capability-contract-mismatch");
        }
        if (Arguments.length > 67) Fail("argument-contract-mismatch");
        let Total = 0;
        for (const Argument of Arguments) {
            const Encoded = Buffer.from(Argument, "utf8");
            if (Argument.includes("\0") || Encoded.toString("utf8") !== Argument ||
                Encoded.length > 4096) {
                Fail("argument-contract-mismatch");
            }
            Total += Encoded.length;
            if (Total > 65_536) Fail("argument-contract-mismatch");
        }
        return;
    }
    if (Selection.command === "wvdump" && Selection.package === "windvale.wvb-inspector" &&
        Selection.part === "inspector" && Application === "windvale.wvb-inspector") {
        if (Launch.lines[0] !== "windvale-launch-record 2" || Arguments.length !== 1 ||
            !Launch.lines.includes("argument-count 1") ||
            !Launch.lines.includes("argument 0 host-path-utf8 1 4096")) {
            Fail("argument-contract-mismatch");
        }
        const Length = Buffer.byteLength(Arguments[0], "utf8");
        if (Length < 1 || Length > 4096 || Arguments[0].includes("\0")) {
            Fail("argument-contract-mismatch");
        }
        Readˉordinary(Arguments[0], MAXIMUM_BUNDLE_BYTES, "inspector-input");
        return;
    }
    if (Selection.command === "wvquery" && Selection.package === "windvale.wvdb-query" &&
        Selection.part === "application" && Application === "windvale.wvdb-query") {
        if (Launch.lines[0] !== "windvale-launch-record 1" || Arguments.length !== 2 ||
            !Launch.lines.includes("argument 0 exact-utf8 Windvale-Database-Storage.bin") ||
            !Launch.lines.includes("argument 1 unsigned-decimal-u64 1 20") ||
            Arguments[0] !== "Windvale-Database-Storage.bin" ||
            !/^(0|[1-9][0-9]{0,19})$/.test(Arguments[1])) {
            Fail("argument-contract-mismatch");
        }
        const Key = BigInt(Arguments[1]);
        if (Key > 18_446_744_073_709_551_615n) Fail("argument-contract-mismatch");
        return;
    }
    Fail("unsupported-command-contract");
}

function Dispatch(Arguments) {
    if (Arguments.length < 8) Fail("invalid-invocation");
    const [ResolverPath, InstallRoot, Target, Command, BundlePath, ApprovalPath,
        LaunchPath, HostPath, ...CommandArguments] = Arguments;
    if (!TOKEN.test(Target) || !TOKEN.test(Command)) Fail("invalid-invocation");
    const Active = Activeˉgeneration(InstallRoot);
    const Selection = Resolve(
        path.resolve(ResolverPath), Active.activation.path, Active.generation.path,
        Target, Command,
    );
    if (Selection.generation !== Active.current || Selection.target !== Target ||
        Selection.command !== Command) Fail("resolver-selection-mismatch");

    const Package = Selectedˉpackage(Active.generation, Selection);
    const Bundle = Readˉordinary(BundlePath, MAXIMUM_BUNDLE_BYTES, "bundle");
    Requireˉidentity(Bundle, { digest: Package.bundle, bytes: Bundle.bytes.length }, "bundle");

    const Approval = Readˉcanonical(ApprovalPath, "approval");
    if (Sha256(Approval.bytes) !== Selection.approval ||
        Approval.lines[0] !== "windvale-capability-approval 1") {
        Fail("approval-identity-mismatch");
    }
    const ApprovalApplication = One(Approval, "application", 2, "approval");
    if (ApprovalApplication[0] !== Package.name || ApprovalApplication[1] !== Package.version ||
        One(Approval, "target", 1, "approval")[0] !== "hosted-wvb-v1") {
        Fail("approval-package-mismatch");
    }
    const ApprovalBundle = Identity(One(Approval, "bundle", 2, "approval"), "approval-bundle");
    const ApprovalLock = Identity(One(Approval, "lock", 2, "approval"), "approval-lock");
    if (ApprovalBundle.digest !== Package.bundle || ApprovalBundle.bytes !== Bundle.bytes.length ||
        ApprovalLock.digest !== Package.lock) {
        Fail("approval-package-mismatch");
    }
    const ApprovalExecutable = Identity(
        One(Approval, "executable", 2, "approval"), "approval-executable",
    );

    const Launch = Readˉcanonical(LaunchPath, "launch");
    if (Sha256(Launch.bytes) !== Selection.launch) Fail("launch-identity-mismatch");
    const LaunchApplication = One(Launch, "application", 2, "launch");
    if (LaunchApplication[0] !== Package.name || LaunchApplication[1] !== Package.version ||
        One(Launch, "target", 1, "launch")[0] !== Target) {
        Fail("launch-package-mismatch");
    }
    const LaunchApproval = Identity(One(Launch, "approval", 2, "launch"), "launch-approval");
    const LaunchBundle = Identity(One(Launch, "bundle", 2, "launch"), "launch-bundle");
    const LaunchWvb = Identity(One(Launch, "wvb", 2, "launch"), "launch-wvb");
    if (LaunchApproval.digest !== Selection.approval ||
        LaunchApproval.bytes !== Approval.bytes.length ||
        LaunchBundle.digest !== Package.bundle || LaunchBundle.bytes !== Bundle.bytes.length ||
        LaunchWvb.digest !== ApprovalExecutable.digest ||
        LaunchWvb.bytes !== ApprovalExecutable.bytes) {
        Fail("launch-closure-mismatch");
    }
    const LaunchLocks = Launch.lines.filter(Line => Line.startsWith("lock "));
    if (LaunchLocks.length > 1) Fail("launch-closure-mismatch");
    if (LaunchLocks.length === 1) {
        const LaunchLock = Identity(
            Fields(LaunchLocks[0], "lock", 2, "launch"), "launch-lock",
        );
        if (LaunchLock.digest !== ApprovalLock.digest ||
            LaunchLock.bytes !== ApprovalLock.bytes) {
            Fail("launch-closure-mismatch");
        }
    }

    const Host = Readˉordinary(HostPath, MAXIMUM_HOST_BYTES, "host-application");
    const HostIdentity = Identity(
        One(Launch, "host-application", 2, "launch"), "launch-host-application",
    );
    Requireˉidentity(Host, HostIdentity, "host-application");
    Validateˉarguments(Selection, Approval, Launch, CommandArguments);

    const PrivateRoot = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-command-host-"));
    const PrivateHost = path.join(
        PrivateRoot,
        process.platform === "win32" ? "Application.exe" : "Application.elf",
    );
    try {
        const Descriptor = fs.openSync(PrivateHost, "wx", 0o700);
        try {
            let Offset = 0;
            while (Offset < Host.bytes.length) {
                const Written = fs.writeSync(
                    Descriptor, Host.bytes, Offset, Host.bytes.length - Offset, Offset,
                );
                if (Written < 1) Fail("private-host-write-no-progress");
                Offset += Written;
            }
            fs.fsyncSync(Descriptor);
        } finally {
            fs.closeSync(Descriptor);
        }
        fs.chmodSync(PrivateHost, 0o700);
        const Private = Readˉordinary(
            PrivateHost, MAXIMUM_HOST_BYTES, "private-host-application",
        );
        Requireˉidentity(Private, HostIdentity, "private-host-application");

        process.stdout.write(
            `command dispatch status=Verified generation=${Active.current} target=${Target} ` +
            `command=${Command} host=${HostIdentity.digest}\n`,
        );
        const Result = spawnSync(Private.path, CommandArguments, {
            stdio: "inherit",
            windowsHide: true,
        });
        if (Result.error) Fail("host-execution-unavailable");
        if (Result.signal !== null) Fail("host-execution-signalled");
        process.exitCode = Result.status;
    } finally {
        fs.rmSync(PrivateRoot, { recursive: true, force: true });
    }
}

try {
    Dispatch(process.argv.slice(2));
} catch (ErrorValue) {
    process.stderr.write(`command dispatch status=Denied reason=${ErrorValue.message}\n`);
    process.exitCode = ErrorValue.message === "invalid-invocation" ? 64 : 1;
}
