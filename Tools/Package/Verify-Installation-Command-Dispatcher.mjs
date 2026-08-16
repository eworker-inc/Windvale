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
const APPROVAL = {
    inspector: "32023a688e3ab4eb6dd83f72c349bf7d2b7ddb184b49253819075f8d9af7b69f",
    wvdb: "13650128da9229524b35910b0fb9f3f7ed2da3de3648f0207fd1e32979b30668",
};
const LAUNCH = {
    "windows-x64": {
        inspector: "eac1706bc237f60b0a843cb369f5b3f07cff794d44d07079c557e1f04f9fa47b",
        wvdb: "1cf993a1dec8f06ea3475aab1aaf27d2ac28f4acca39d7a0f475be8fa75ab530",
        other: "linux-x64",
    },
    "linux-x64": {
        inspector: "f5c45df84c9624fd7579fc83947a595caf206ddb5783a9b3efba15d7ad6e379b",
        wvdb: "7c7f85d8d7877badc4353581be8c19a454e9ca375977d17b7605d58fd66bb70b",
        other: "windows-x64",
    },
};

const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");

function Generation(Target) {
    return Buffer.from(
        "windvale-generation 1\n" +
        `target ${Target}\n` +
        "package windvale.wvb-inspector 0.1.0 " +
        "a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 " +
        "eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1\n" +
        "package windvale.wvdb-query 0.1.0 " +
        "33bf528ef69d5b7578ec2b2c61ca5915fb2ebd7d71346fb439753bbf5f2ab70c " +
        "4e4089ad6b40f6f9b435bebdd8b3321e64db6038d745c191aa54e348ee44d926\n" +
        `command wvdump windvale.wvb-inspector inspector ${APPROVAL.inspector} ${LAUNCH[Target].inspector}\n` +
        `command wvquery windvale.wvdb-query application ${APPROVAL.wvdb} ${LAUNCH[Target].wvdb}\n`,
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
    assert.match(Result.stderr, new RegExp(`^command dispatch status=Denied reason=${Reason}`));
}

const [ResolverArgument, Target, InspectorBundleArgument, WvdbBundleArgument,
    WvdbHostArgument] = process.argv.slice(2);
if (process.argv.length !== 7 || !Object.hasOwn(LAUNCH, Target)) {
    process.stderr.write(
        "Usage: node Verify-Installation-Command-Dispatcher.mjs " +
        "<resolver> <windows-x64|linux-x64> <inspector-bundle> <wvdb-bundle> <wvdb-host>\n",
    );
    process.exit(64);
}

const Resolver = path.resolve(ResolverArgument);
const InspectorBundle = path.resolve(InspectorBundleArgument);
const WvdbBundle = path.resolve(WvdbBundleArgument);
const WvdbHost = path.resolve(WvdbHostArgument);
const ApplicationDirectory = Target === "windows-x64" ? "windows-x64" : "linux-x64";
const InspectorHost = path.join(
    REPOSITORY_ROOT, "Artifacts", "Native-Front-Door", ApplicationDirectory,
    Target === "windows-x64" ? "wvdump.exe" : "wvdump.elf",
);
const InspectorInput = path.join(
    REPOSITORY_ROOT, "Artifacts", "Native-Front-Door", "Wvb", "Wvb-Inspector.wvb",
);
const InspectorRecords = path.join(
    REPOSITORY_ROOT, "Distribution", "Applications", "Wvb-Inspector",
);
const WvdbRecords = path.join(
    REPOSITORY_ROOT, "Distribution", "Applications", "Wvdb-Query",
);
const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-command-dispatch-"));
try {
    const Install = path.join(Work, "Install");
    const RunDirectory = path.join(Work, "Run");
    fs.mkdirSync(Install, { mode: 0o700 });
    fs.mkdirSync(RunDirectory, { mode: 0o700 });
    const GenerationBytes = Generation(Target);
    const GenerationDigest = Sha256(GenerationBytes);
    const GenerationPath = path.join(Work, "Generation-1.txt");
    fs.writeFileSync(GenerationPath, GenerationBytes, { flag: "wx" });
    Requireˉsuccess(
        Run(process.execPath, [GENERATION_PUBLISHER, "publish", Install,
            GenerationPath, GenerationDigest]),
        "generation publication",
    );
    const ActivationBytes = Buffer.from(
        "windvale-activation 1\nserial 1\n" +
        `current ${GenerationDigest}\nprevious none\n`,
        "utf8",
    );
    const ActivationPath = path.join(Work, "Activation-1.txt");
    fs.writeFileSync(ActivationPath, ActivationBytes, { flag: "wx" });
    Requireˉsuccess(
        Run(process.execPath, [ACTIVATION_PUBLISHER, "publish", Install, "none",
            ActivationPath, Sha256(ActivationBytes)]),
        "activation publication",
    );
    const Common = [DISPATCHER, Resolver, Install, Target];
    const InspectorApproval = path.join(
        InspectorRecords, "Windvale-Wvb-Inspector.wvapproval",
    );
    const InspectorLaunch = path.join(
        InspectorRecords, `Windvale-Wvb-Inspector.${Target}.wvlaunch`,
    );
    const WvdbApproval = path.join(WvdbRecords, "Windvale-Wvdb-Query.wvapproval");
    const WvdbLaunch = path.join(
        WvdbRecords, `Windvale-Wvdb-Query.${Target}.wvlaunch`,
    );

    process.stdout.write("command dispatcher step=execute-inspector item=1/9\n");
    const Inspector = Run(process.execPath, [...Common, "wvdump", InspectorBundle,
        InspectorApproval, InspectorLaunch, InspectorHost, InspectorInput], { cwd: RunDirectory });
    Requireˉsuccess(Inspector, "inspector dispatch");
    assert.match(Inspector.stdout, /^command dispatch status=Verified /);
    assert.match(Inspector.stdout, /\nwvdump 1\nmodule version=1\.11 profile=hosted /);

    process.stdout.write("command dispatcher step=execute-wvdb item=2/9\n");
    const StoragePath = path.join(RunDirectory, "Windvale-Database-Storage.bin");
    Requireˉsuccess(
        Run(process.execPath, [path.join(REPOSITORY_ROOT, "Tools", "Native",
            "Create-Wvdb-Query-Fixture.mjs"), StoragePath]),
        "WVDB fixture",
    );
    const Wvdb = Run(process.execPath, [...Common, "wvquery", WvdbBundle,
        WvdbApproval, WvdbLaunch, WvdbHost, "Windvale-Database-Storage.bin", "7"],
    { cwd: RunDirectory });
    Requireˉsuccess(Wvdb, "WVDB dispatch");
    assert.match(Wvdb.stdout, /^command dispatch status=Verified /);
    assert.match(Wvdb.stdout, /\nfound key=7 value=42\n$/);

    process.stdout.write("command dispatcher step=reject-bundle-tamper item=3/9\n");
    const TamperedBundle = path.join(Work, "Tampered.wvbundle");
    fs.copyFileSync(InspectorBundle, TamperedBundle, fs.constants.COPYFILE_EXCL);
    fs.appendFileSync(TamperedBundle, "x");
    Requireˉdenied(
        Run(process.execPath, [...Common, "wvdump", TamperedBundle, InspectorApproval,
            InspectorLaunch, InspectorHost, InspectorInput], { cwd: RunDirectory }),
        "bundle-identity-mismatch",
    );

    process.stdout.write("command dispatcher step=reject-approval-substitution item=4/9\n");
    Requireˉdenied(
        Run(process.execPath, [...Common, "wvdump", InspectorBundle, WvdbApproval,
            InspectorLaunch, InspectorHost, InspectorInput], { cwd: RunDirectory }),
        "approval-identity-mismatch",
    );

    process.stdout.write("command dispatcher step=reject-launch-substitution item=5/9\n");
    const OtherLaunch = path.join(
        InspectorRecords, `Windvale-Wvb-Inspector.${LAUNCH[Target].other}.wvlaunch`,
    );
    Requireˉdenied(
        Run(process.execPath, [...Common, "wvdump", InspectorBundle, InspectorApproval,
            OtherLaunch, InspectorHost, InspectorInput], { cwd: RunDirectory }),
        "launch-identity-mismatch",
    );

    process.stdout.write("command dispatcher step=reject-host-tamper item=6/9\n");
    const TamperedHost = path.join(Work, Target === "windows-x64" ? "Tampered.exe" : "Tampered.elf");
    fs.copyFileSync(InspectorHost, TamperedHost, fs.constants.COPYFILE_EXCL);
    fs.appendFileSync(TamperedHost, Buffer.from([0]));
    fs.chmodSync(TamperedHost, 0o700);
    Requireˉdenied(
        Run(process.execPath, [...Common, "wvdump", InspectorBundle, InspectorApproval,
            InspectorLaunch, TamperedHost, InspectorInput], { cwd: RunDirectory }),
        "host-application-identity-mismatch",
    );

    process.stdout.write("command dispatcher step=reject-arguments item=7/9\n");
    Requireˉdenied(
        Run(process.execPath, [...Common, "wvquery", WvdbBundle, WvdbApproval,
            WvdbLaunch, WvdbHost, "Other.bin", "7"], { cwd: RunDirectory }),
        "argument-contract-mismatch",
    );

    process.stdout.write("command dispatcher step=reject-unknown item=8/9\n");
    Requireˉdenied(
        Run(process.execPath, [...Common, "unknown", InspectorBundle, InspectorApproval,
            InspectorLaunch, InspectorHost], { cwd: RunDirectory }),
        "resolver-rejected-selection",
    );

    process.stdout.write("command dispatcher step=reject-invocation item=9/9\n");
    const Invalid = Run(process.execPath, [DISPATCHER]);
    assert.equal(Invalid.status, 64);
    assert.equal(Invalid.stdout, "");
    assert.equal(Invalid.stderr,
        "command dispatch status=Denied reason=invalid-invocation\n");

    process.stdout.write(
        "native installation command dispatch status=Passed cases=9 commands=2 " +
        "executions=2 integrity-rejections=4 policy-rejections=3\n",
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
