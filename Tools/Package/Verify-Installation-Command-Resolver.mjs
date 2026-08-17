import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");
const APPROVAL = {
    inspector: "32023a688e3ab4eb6dd83f72c349bf7d2b7ddb184b49253819075f8d9af7b69f",
    wvdb: "0b58e435e08045a7118353e2a454e92f4eadfdf36458693e5dd1cf85b58dfdb2",
};
const LAUNCH = {
    "windows-x64": {
        inspector: "eac1706bc237f60b0a843cb369f5b3f07cff794d44d07079c557e1f04f9fa47b",
        wvdb: "9012947e64e3650bba1e5dd5213a1f4c78c56318e5b5ff40804d7ca902aa3348",
        generation: "7ab98618e589b58850f94a1390c9a2d887f51127c6e6d6cc4c674b319d7a4999",
    },
    "linux-x64": {
        inspector: "f5c45df84c9624fd7579fc83947a595caf206ddb5783a9b3efba15d7ad6e379b",
        wvdb: "1e61ebeac166c9b35cb852dc290ae5df97cf289b22fa8e057b4df74953786dfc",
        generation: "d09fa80957b663d3f6d2e85fa8b1ccf7cb3a79c35e250a94d4a6e9f86fec4bd5",
    },
};

function Generation(Target) {
    return Buffer.from(
        "windvale-generation 1\n" +
        `target ${Target}\n` +
        "package windvale.wvb-inspector 0.1.0 " +
        "a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 " +
        "eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1\n" +
        "package windvale.wvdb-query 0.1.0 " +
        "40c09378e20b5ac49d41fada61c24e786363e89bf839925cac8d9f3c715a9378 " +
        "7fe9552317e0845b693b8a4ade1882c4a492cecf46c1bfcaaf26b45ed067be50\n" +
        `command wvdump windvale.wvb-inspector inspector ${APPROVAL.inspector} ${LAUNCH[Target].inspector}\n` +
        `command wvquery windvale.wvdb-query application ${APPROVAL.wvdb} ${LAUNCH[Target].wvdb}\n`,
        "utf8",
    );
}

function Activation(Current) {
    return Buffer.from(
        "windvale-activation 1\n" +
        "serial 1\n" +
        `current ${Current}\n` +
        "previous none\n",
        "utf8",
    );
}

function Run(Executable, ...Arguments) {
    return spawnSync(Executable, Arguments, { encoding: "utf8", windowsHide: true });
}

function Requireˉsuccess(Result, Description) {
    assert.equal(Result.status, 0, `${Description}: ${Result.stderr}`);
    assert.equal(Result.stderr, "", Description);
}

const [ExecutableArgument, Target] = process.argv.slice(2);
if (process.argv.length !== 4 || !Object.hasOwn(LAUNCH, Target)) {
    process.stderr.write(
        "Usage: node Verify-Installation-Command-Resolver.mjs " +
        "<resolver-executable> <windows-x64|linux-x64>\n",
    );
    process.exit(64);
}
const Executable = path.resolve(ExecutableArgument);
const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-command-resolution-"));
try {
    const GenerationBytes = Generation(Target);
    const Digest = Sha256(GenerationBytes);
    assert.equal(Digest, LAUNCH[Target].generation);
    const GenerationPath = path.join(Work, "Generation-1.txt");
    const ActivationPath = path.join(Work, "Activation-1.txt");
    fs.writeFileSync(GenerationPath, GenerationBytes, { flag: "wx" });
    fs.writeFileSync(ActivationPath, Activation(Digest), { flag: "wx" });

    process.stdout.write("command resolver step=resolve-inspector item=1/8\n");
    const Inspector = Run(Executable, ActivationPath, GenerationPath, Target, "wvdump");
    Requireˉsuccess(Inspector, "inspector command resolution");
    assert.equal(
        Inspector.stdout,
        `command resolution status=Valid generation=${Digest} target=${Target} ` +
        `command=wvdump package=windvale.wvb-inspector part=inspector ` +
        `approval=${APPROVAL.inspector} launch=${LAUNCH[Target].inspector}\n`,
    );

    process.stdout.write("command resolver step=resolve-wvdb item=2/8\n");
    const Wvdb = Run(Executable, ActivationPath, GenerationPath, Target, "wvquery");
    Requireˉsuccess(Wvdb, "WVDB command resolution");
    assert.equal(
        Wvdb.stdout,
        `command resolution status=Valid generation=${Digest} target=${Target} ` +
        `command=wvquery package=windvale.wvdb-query part=application ` +
        `approval=${APPROVAL.wvdb} launch=${LAUNCH[Target].wvdb}\n`,
    );

    process.stdout.write("command resolver step=reject-unknown item=3/8\n");
    const Unknown = Run(Executable, ActivationPath, GenerationPath, Target, "missing");
    assert.equal(Unknown.status, 1);
    assert.equal(Unknown.stderr, "command resolution status=Unknown-command\n");

    process.stdout.write("command resolver step=reject-target item=4/8\n");
    const OtherTarget = Target === "windows-x64" ? "linux-x64" : "windows-x64";
    const WrongTarget = Run(Executable, ActivationPath, GenerationPath, OtherTarget, "wvdump");
    assert.equal(WrongTarget.status, 1);
    assert.equal(WrongTarget.stderr, "command resolution status=Wrong-target\n");

    process.stdout.write("command resolver step=reject-inactive item=5/8\n");
    fs.writeFileSync(ActivationPath, Activation("0".repeat(64)));
    const Inactive = Run(Executable, ActivationPath, GenerationPath, Target, "wvdump");
    assert.equal(Inactive.status, 1);
    assert.equal(Inactive.stderr, "command resolution status=Inactive-generation\n");

    process.stdout.write("command resolver step=reject-activation item=6/8\n");
    fs.writeFileSync(ActivationPath, "wrong\n");
    const InvalidActivation = Run(Executable, ActivationPath, GenerationPath, Target, "wvdump");
    assert.equal(InvalidActivation.status, 1);
    assert.equal(InvalidActivation.stderr, "command resolution status=Invalid-activation\n");

    process.stdout.write("command resolver step=reject-generation item=7/8\n");
    fs.writeFileSync(ActivationPath, Activation(Digest));
    fs.writeFileSync(GenerationPath, "wrong\n");
    const InvalidGeneration = Run(Executable, ActivationPath, GenerationPath, Target, "wvdump");
    assert.equal(InvalidGeneration.status, 1);
    assert.equal(InvalidGeneration.stderr, "command resolution status=Invalid-generation\n");

    process.stdout.write("command resolver step=reject-arguments item=8/8\n");
    const Arguments = Run(Executable);
    assert.equal(Arguments.status, 64);
    assert.match(Arguments.stderr, /^Usage: wvcommand-resolve/);

    process.stdout.write(
        "native installation command resolution status=Passed cases=8 commands=2 " +
        "cross-host-generations=Verified\n",
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
