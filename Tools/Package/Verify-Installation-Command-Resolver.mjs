import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");
const APPROVAL = {
    inspector: "32023a688e3ab4eb6dd83f72c349bf7d2b7ddb184b49253819075f8d9af7b69f",
    wvdb: "13650128da9229524b35910b0fb9f3f7ed2da3de3648f0207fd1e32979b30668",
};
const LAUNCH = {
    "windows-x64": {
        inspector: "eac1706bc237f60b0a843cb369f5b3f07cff794d44d07079c557e1f04f9fa47b",
        wvdb: "1cf993a1dec8f06ea3475aab1aaf27d2ac28f4acca39d7a0f475be8fa75ab530",
        generation: "d5b55b528b35adb43eb7bc9a9fe62d2ca3c5d6578642e78854b7be31013f579d",
    },
    "linux-x64": {
        inspector: "f5c45df84c9624fd7579fc83947a595caf206ddb5783a9b3efba15d7ad6e379b",
        wvdb: "7c7f85d8d7877badc4353581be8c19a454e9ca375977d17b7605d58fd66bb70b",
        generation: "ec12ed70528e77b3809380525d9abeeed87433dcff7150c96eb9dc449e8aea57",
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
        "33bf528ef69d5b7578ec2b2c61ca5915fb2ebd7d71346fb439753bbf5f2ab70c " +
        "4e4089ad6b40f6f9b435bebdd8b3321e64db6038d745c191aa54e348ee44d926\n" +
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
