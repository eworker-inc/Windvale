import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, "../..");
const DISPATCHER = path.join(SCRIPT_DIRECTORY, "Dispatch-Installation-Command.mjs");
const GENERATION_PUBLISHER = path.join(SCRIPT_DIRECTORY, "Publish-Installation-Generation.mjs");
const ACTIVATION_PUBLISHER = path.join(SCRIPT_DIRECTORY, "Publish-Installation-Activation.mjs");
const APPROVAL = "386b8c983be8f4c633f27beb0d60b0d135ff3df88819a9c20262c1a8ce257790";
const BUNDLE = "0502051930bddd016924e7858e0c32c0c481774edae9e755ca926f3cc3b3e966";
const LOCK = "212e5c4ddf28fb347b482c73d5c38d6df8273be4bcf14ce1b581084d7be1652d";
const LAUNCH = {
    "windows-x64": "493bac26e83edf995f87e31939a981fef7a1c021494bc23e154f61922dc2aa5b",
    "linux-x64": "447df010898a98022a915c46d11c42c41a1099024e5fcbba3009735347459099",
};

const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");

function Generation(Target, LaunchDigest) {
    return Buffer.from(
        "windvale-generation 1\n" +
        `target ${Target}\n` +
        `package windvale.echo 0.1.0 ${BUNDLE} ${LOCK}\n` +
        `command echo windvale.echo application ${APPROVAL} ${LaunchDigest}\n`,
        "utf8",
    );
}

function Run(Program, Arguments, Options = {}) {
    return spawnSync(Program, Arguments, {
        cwd: Options.cwd,
        encoding: "utf8",
        windowsHide: true,
    });
}

function Requireˉsuccess(Result, Description) {
    assert.equal(Result.status, 0, `${Description}: ${Result.stderr}`);
    assert.equal(Result.stderr, "", Description);
}

function Requireˉdenied(Result, Reason) {
    assert.equal(Result.status, 1);
    assert.equal(Result.stdout, "");
    assert.equal(Result.stderr, `command dispatch status=Denied reason=${Reason}\n`);
}

function Privateˉhosts() {
    return fs.readdirSync(os.tmpdir())
        .filter(Name => Name.startsWith("windvale-command-host-"))
        .sort();
}

function Runˉwithoutˉprivateˉleak(Program, Arguments, Options = {}) {
    const Before = Privateˉhosts();
    const Result = Run(Program, Arguments, Options);
    assert.deepEqual(Privateˉhosts(), Before, "dispatcher left private host storage");
    return Result;
}

function Publishˉgeneration(Work, Name, Target, LaunchDigest) {
    const Install = path.join(Work, Name);
    fs.mkdirSync(Install, { mode: 0o700 });
    const GenerationBytes = Generation(Target, LaunchDigest);
    const GenerationDigest = Sha256(GenerationBytes);
    const GenerationPath = path.join(Work, `${Name}-Generation-1.txt`);
    fs.writeFileSync(GenerationPath, GenerationBytes, { flag: "wx" });
    Requireˉsuccess(
        Run(process.execPath, [GENERATION_PUBLISHER, "publish", Install,
            GenerationPath, GenerationDigest]),
        `${Name} generation publication`,
    );
    const ActivationBytes = Buffer.from(
        "windvale-activation 1\nserial 1\n" +
        `current ${GenerationDigest}\nprevious none\n`,
        "utf8",
    );
    const ActivationPath = path.join(Work, `${Name}-Activation-1.txt`);
    fs.writeFileSync(ActivationPath, ActivationBytes, { flag: "wx" });
    Requireˉsuccess(
        Run(process.execPath, [ACTIVATION_PUBLISHER, "publish", Install, "none",
            ActivationPath, Sha256(ActivationBytes)]),
        `${Name} activation publication`,
    );
    return Install;
}

const [ResolverArgument, Target, BundleArgument, HostArgument] = process.argv.slice(2);
if (process.argv.length !== 6 || !Object.hasOwn(LAUNCH, Target)) {
    process.stderr.write(
        "Usage: node Verify-Echo-Command-Launch.mjs " +
        "<resolver> <windows-x64|linux-x64> <echo-bundle> <echo-host>\n",
    );
    process.exit(64);
}

const Resolver = path.resolve(ResolverArgument);
const EchoBundle = path.resolve(BundleArgument);
const EchoHost = path.resolve(HostArgument);
const Records = path.join(REPOSITORY_ROOT, "Distribution", "Applications", "Echo");
const ApprovalPath = path.join(Records, "Windvale-Echo.wvapproval");
const LaunchPath = path.join(Records, `Windvale-Echo.${Target}.wvlaunch`);
const InspectorApproval = path.join(
    REPOSITORY_ROOT, "Distribution", "Applications", "Wvb-Inspector",
    "Windvale-Wvb-Inspector.wvapproval",
);
const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-echo-command-launch-"));
try {
    const RunDirectory = path.join(Work, "Run");
    fs.mkdirSync(RunDirectory, { mode: 0o700 });
    const Install = Publishˉgeneration(Work, "Install", Target, LAUNCH[Target]);
    const Common = [DISPATCHER, Resolver, Install, Target, "echo", EchoBundle,
        ApprovalPath, LaunchPath, EchoHost];

    process.stdout.write("echo command launch step=execute-arguments item=1/10\n");
    const Arguments = Runˉwithoutˉprivateˉleak(
        process.execPath, [...Common, "one", "", "雪"], { cwd: RunDirectory },
    );
    Requireˉsuccess(Arguments, "argument dispatch");
    assert.match(Arguments.stdout, /^command dispatch status=Verified /);
    assert.match(Arguments.stdout, /\none  雪\n$/);

    process.stdout.write("echo command launch step=execute-empty item=2/10\n");
    const Empty = Runˉwithoutˉprivateˉleak(
        process.execPath, Common, { cwd: RunDirectory },
    );
    Requireˉsuccess(Empty, "empty dispatch");
    assert.match(Empty.stdout, /^command dispatch status=Verified /);
    assert.match(Empty.stdout, /\n\n$/);

    process.stdout.write("echo command launch step=reject-bundle-substitution item=3/10\n");
    const TamperedBundle = path.join(Work, "Tampered.wvbundle");
    fs.copyFileSync(EchoBundle, TamperedBundle, fs.constants.COPYFILE_EXCL);
    fs.appendFileSync(TamperedBundle, "x");
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [DISPATCHER, Resolver, Install, Target, "echo", TamperedBundle,
                ApprovalPath, LaunchPath, EchoHost], { cwd: RunDirectory }),
        "bundle-identity-mismatch",
    );

    process.stdout.write("echo command launch step=reject-host-substitution item=4/10\n");
    const TamperedHost = path.join(
        Work, Target === "windows-x64" ? "Tampered.exe" : "Tampered.elf",
    );
    fs.copyFileSync(EchoHost, TamperedHost, fs.constants.COPYFILE_EXCL);
    fs.appendFileSync(TamperedHost, Buffer.from([0]));
    fs.chmodSync(TamperedHost, 0o700);
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [DISPATCHER, Resolver, Install, Target, "echo", EchoBundle,
                ApprovalPath, LaunchPath, TamperedHost], { cwd: RunDirectory }),
        "host-application-identity-mismatch",
    );

    process.stdout.write("echo command launch step=reject-approval-substitution item=5/10\n");
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [DISPATCHER, Resolver, Install, Target, "echo", EchoBundle,
                InspectorApproval, LaunchPath, EchoHost], { cwd: RunDirectory }),
        "approval-identity-mismatch",
    );

    process.stdout.write("echo command launch step=reject-argument-bytes item=6/10\n");
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [...Common, "x".repeat(4097)], { cwd: RunDirectory }),
        "argument-contract-mismatch",
    );

    process.stdout.write("echo command launch step=reject-argument-count item=7/10\n");
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [...Common, ...Array(68).fill("x")], { cwd: RunDirectory }),
        "argument-contract-mismatch",
    );

    process.stdout.write("echo command launch step=reject-argument-aggregate item=8/10\n");
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [...Common, ...Array(17).fill("雪".repeat(1365))], { cwd: RunDirectory }),
        "argument-contract-mismatch",
    );

    process.stdout.write("echo command launch step=reject-capability-substitution item=9/10\n");
    const TamperedLaunch = path.join(Work, "Tampered.wvlaunch");
    const TamperedLaunchBytes = fs.readFileSync(LaunchPath, "utf8").replace(
        "bind 0 console.write_line host-standard-output line-lf",
        "bind 0 console.write_line host-standard-diagnostic line-lf",
    );
    assert.notEqual(TamperedLaunchBytes, fs.readFileSync(LaunchPath, "utf8"));
    fs.writeFileSync(TamperedLaunch, TamperedLaunchBytes, { flag: "wx" });
    const TamperedInstall = Publishˉgeneration(
        Work, "Tampered-Install", Target, Sha256(Buffer.from(TamperedLaunchBytes, "utf8")),
    );
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [DISPATCHER, Resolver, TamperedInstall, Target, "echo", EchoBundle,
                ApprovalPath, TamperedLaunch, EchoHost], { cwd: RunDirectory }),
        "capability-contract-mismatch",
    );

    process.stdout.write("echo command launch step=reject-unknown item=10/10\n");
    Requireˉdenied(
        Runˉwithoutˉprivateˉleak(process.execPath,
            [DISPATCHER, Resolver, Install, Target, "unknown", EchoBundle,
                ApprovalPath, LaunchPath, EchoHost], { cwd: RunDirectory }),
        "resolver-rejected-selection",
    );

    process.stdout.write(
        "native echo command launch status=Passed cases=10 executions=2 " +
        "integrity-rejections=3 policy-rejections=4 resolution-rejections=1 " +
        "private-cleanup=Verified\n",
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
