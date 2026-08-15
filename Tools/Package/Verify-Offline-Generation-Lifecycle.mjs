import assert from "node:assert/strict";
import crypto from "node:crypto";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const GENERATION_PUBLISHER = path.join(SCRIPT_DIRECTORY, "Publish-Installation-Generation.mjs");
const ACTIVATION_PUBLISHER = path.join(SCRIPT_DIRECTORY, "Publish-Installation-Activation.mjs");
const UNINSTALLER = path.join(SCRIPT_DIRECTORY, "Uninstall-Offline-Package-State.mjs");
const APPROVAL = {
    inspector: "32023a688e3ab4eb6dd83f72c349bf7d2b7ddb184b49253819075f8d9af7b69f",
    wvdb: "3c4a968745cde9d5073c67c6c453443d54c74e779b509c2f00131b4d47e8ef71",
};
const LAUNCH = {
    "windows-x64": {
        inspector: "eac1706bc237f60b0a843cb369f5b3f07cff794d44d07079c557e1f04f9fa47b",
        wvdb: "95d1a64007f487e57aec77f7466d091cc54247dcbec2f8534b5870e36715b0b3",
    },
    "linux-x64": {
        inspector: "f5c45df84c9624fd7579fc83947a595caf206ddb5783a9b3efba15d7ad6e379b",
        wvdb: "b0c976649936cf43cfa1ccb79a63093e584dda9b22cf905b954db6e3192eacd5",
    },
};
const CANDIDATE_PREFIX = "Activation-1.candidate-";
const Sha256 = Value => crypto.createHash("sha256").update(Value).digest("hex");

function Generation(Target, Expanded) {
    let Value = "windvale-generation 1\n" + `target ${Target}\n` +
        "package windvale.wvb-inspector 0.1.0 " +
        "a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 " +
        "eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1\n";
    if (Expanded) {
        Value += "package windvale.wvdb-query 0.1.0 " +
            "3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 " +
            "ad22e10e41dda772650123b4802518575088973aa73277889b443ad27aa25618\n";
    }
    Value += `command wvdump windvale.wvb-inspector inspector ${APPROVAL.inspector} ${LAUNCH[Target].inspector}\n`;
    if (Expanded) {
        Value += `command wvquery windvale.wvdb-query application ${APPROVAL.wvdb} ${LAUNCH[Target].wvdb}\n`;
    }
    return Buffer.from(Value, "utf8");
}

function Activation(Serial, Current, Previous) {
    return Buffer.from(
        "windvale-activation 1\n" + `serial ${Serial}\n` +
        `current ${Current}\nprevious ${Previous}\n`,
        "utf8",
    );
}

function Decodeˉplan(Output) {
    const Match = /^windvale-activation-plan 1\nserial-low (0|[1-9][0-9]*)\nserial-high (0|[1-9][0-9]*)\ncurrent ([0-9a-f]{64})\nprevious (none|[0-9a-f]{64})\n$/.exec(Output);
    assert.notEqual(Match, null, "canonical activation plan report");
    const Low = BigInt(Match[1]);
    const High = BigInt(Match[2]);
    assert.ok(Low <= 0xffff_ffffn && High <= 0xffff_ffffn);
    const Serial = (High << 32n) | Low;
    return Activation(Serial.toString(), Match[3], Match[4]);
}

function Run(Program, Arguments) {
    return spawnSync(Program, Arguments, { encoding: "utf8", windowsHide: true });
}

function Node(Command, ...Arguments) {
    return Run(process.execPath, [Command, ...Arguments]);
}

function Requireˉsuccess(Result, Description) {
    assert.equal(Result.status, 0, `${Description}: ${Result.stderr}`);
    assert.equal(Result.stderr, "", Description);
}

function Resolve(Resolver, ActivationPath, GenerationPath, Target, Command, Expected) {
    const Result = Run(Resolver, [ActivationPath, GenerationPath, Target, Command]);
    if (Expected) {
        Requireˉsuccess(Result, `${Command} resolution`);
        assert.match(Result.stdout, new RegExp(` command=${Command} `));
    } else {
        assert.equal(Result.status, 1);
        assert.equal(Result.stdout, "");
        assert.equal(Result.stderr, "command resolution status=Unknown-command\n");
    }
}

const [PlannerArgument, ResolverArgument, Target] = process.argv.slice(2);
if (process.argv.length !== 5 || !Object.hasOwn(LAUNCH, Target)) {
    process.stderr.write(
        "Usage: node Verify-Offline-Generation-Lifecycle.mjs " +
        "<planner-executable> <resolver-executable> <windows-x64|linux-x64>\n",
    );
    process.exit(64);
}
const Planner = path.resolve(PlannerArgument);
const Resolver = path.resolve(ResolverArgument);
const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-generation-lifecycle-"));
try {
    const Install = path.join(Work, "Install");
    fs.mkdirSync(Install, { mode: 0o700 });
    const GenerationA = Generation(Target, false);
    const GenerationB = Generation(Target, true);
    const GenerationAIdentity = Sha256(GenerationA);
    const GenerationBIdentity = Sha256(GenerationB);
    const GenerationAInput = path.join(Work, "Generation-A.txt");
    const GenerationBInput = path.join(Work, "Generation-B.txt");
    fs.writeFileSync(GenerationAInput, GenerationA, { flag: "wx" });
    fs.writeFileSync(GenerationBInput, GenerationB, { flag: "wx" });

    process.stdout.write("generation lifecycle step=publish-generations item=1/10 generations=2\n");
    Requireˉsuccess(
        Node(GENERATION_PUBLISHER, "publish", Install, GenerationAInput, GenerationAIdentity),
        "Generation A publication",
    );
    Requireˉsuccess(
        Node(GENERATION_PUBLISHER, "publish", Install, GenerationBInput, GenerationBIdentity),
        "Generation B publication",
    );
    const PublishedA = path.join(
        Install, "generations", GenerationAIdentity, "Generation-1.txt",
    );
    const PublishedB = path.join(
        Install, "generations", GenerationBIdentity, "Generation-1.txt",
    );

    process.stdout.write("generation lifecycle step=bootstrap-first-generation item=2/10\n");
    const ActivationA = Activation("1", GenerationAIdentity, "none");
    const ActivationAInput = path.join(Work, "Activation-A.txt");
    fs.writeFileSync(ActivationAInput, ActivationA, { flag: "wx" });
    Requireˉsuccess(
        Node(ACTIVATION_PUBLISHER, "publish", Install, "none", ActivationAInput,
            Sha256(ActivationA)),
        "initial activation",
    );
    const PublicActivation = path.join(Install, "state", "Activation-1.txt");

    process.stdout.write("generation lifecycle step=observe-first-generation item=3/10 commands=2\n");
    Resolve(Resolver, PublicActivation, PublishedA, Target, "wvdump", true);
    Resolve(Resolver, PublicActivation, PublishedA, Target, "wvquery", false);

    process.stdout.write("generation lifecycle step=plan-update item=4/10\n");
    const PlannedB = Run(Planner, ["activate", PublicActivation,
        GenerationBIdentity, "present", "present"]);
    Requireˉsuccess(PlannedB, "update plan");
    const ActivationB = Activation("2", GenerationBIdentity, GenerationAIdentity);
    assert.deepEqual(Decodeˉplan(PlannedB.stdout), ActivationB);
    const ActivationBInput = path.join(Work, "Activation-B.txt");
    fs.writeFileSync(ActivationBInput, ActivationB, { flag: "wx" });
    const AdmittedB = Run(Planner, ["activate", ActivationBInput,
        GenerationBIdentity, "present", "present"]);
    Requireˉsuccess(AdmittedB, "constructed update admission");
    assert.deepEqual(Decodeˉplan(AdmittedB.stdout), ActivationB);

    process.stdout.write("generation lifecycle step=recover-interruption item=5/10\n");
    const Candidate = path.join(
        Install, "state", `${CANDIDATE_PREFIX}${Sha256(ActivationB)}`,
    );
    fs.writeFileSync(Candidate, ActivationB, { flag: "wx" });
    const Recovery = Node(ACTIVATION_PUBLISHER, "recover", Install);
    Requireˉsuccess(Recovery, "interrupted update recovery");
    assert.match(Recovery.stdout, /cleaned=1/);
    assert.deepEqual(fs.readFileSync(PublicActivation), ActivationA);
    Resolve(Resolver, PublicActivation, PublishedA, Target, "wvquery", false);

    process.stdout.write("generation lifecycle step=activate-expanded-generation item=6/10\n");
    Requireˉsuccess(
        Node(ACTIVATION_PUBLISHER, "publish", Install, Sha256(ActivationA),
            ActivationBInput, Sha256(ActivationB)),
        "expanded activation",
    );
    Resolve(Resolver, PublicActivation, PublishedB, Target, "wvdump", true);
    Resolve(Resolver, PublicActivation, PublishedB, Target, "wvquery", true);

    process.stdout.write("generation lifecycle step=rollback item=7/10\n");
    const PlannedRollback = Run(Planner, ["rollback", PublicActivation,
        "none", "present", "present"]);
    Requireˉsuccess(PlannedRollback, "rollback plan");
    const ActivationRollback = Activation("3", GenerationAIdentity, GenerationBIdentity);
    assert.deepEqual(Decodeˉplan(PlannedRollback.stdout), ActivationRollback);
    const RollbackInput = path.join(Work, "Activation-Rollback.txt");
    fs.writeFileSync(RollbackInput, ActivationRollback, { flag: "wx" });
    const AdmittedRollback = Run(Planner, ["activate", RollbackInput,
        GenerationAIdentity, "present", "present"]);
    Requireˉsuccess(AdmittedRollback, "constructed rollback admission");
    assert.deepEqual(Decodeˉplan(AdmittedRollback.stdout), ActivationRollback);
    Requireˉsuccess(
        Node(ACTIVATION_PUBLISHER, "publish", Install, Sha256(ActivationB),
            RollbackInput, Sha256(ActivationRollback)),
        "rollback publication",
    );
    Resolve(Resolver, PublicActivation, PublishedA, Target, "wvdump", true);
    Resolve(Resolver, PublicActivation, PublishedA, Target, "wvquery", false);

    process.stdout.write("generation lifecycle step=verify-retained-state item=8/10\n");
    assert.equal(Sha256(fs.readFileSync(PublishedA)), GenerationAIdentity);
    assert.equal(Sha256(fs.readFileSync(PublishedB)), GenerationBIdentity);
    assert.deepEqual(fs.readFileSync(PublicActivation), ActivationRollback);
    const EmptyRecovery = Node(ACTIVATION_PUBLISHER, "recover", Install);
    Requireˉsuccess(EmptyRecovery, "empty recovery");
    assert.match(EmptyRecovery.stdout, /cleaned=0/);

    process.stdout.write("generation lifecycle step=prepare-uninstall item=9/10\n");
    fs.mkdirSync(path.join(Install, "store", "objects", "sha256"), { recursive: true });
    fs.mkdirSync(path.join(Install, "store", "bundles", "sha256"), { recursive: true });
    const ApplicationData = path.join(Install, "application-data", "database.wvdb");
    fs.mkdirSync(path.dirname(ApplicationData), { mode: 0o700 });
    fs.writeFileSync(ApplicationData, "preserve-user-data", { flag: "wx" });
    const Unrelated = path.join(Install, "notes.txt");
    fs.writeFileSync(Unrelated, "preserve-unrelated-file", { flag: "wx" });

    process.stdout.write("generation lifecycle step=uninstall item=10/10\n");
    const Uninstall = Node(UNINSTALLER, "uninstall", Install);
    Requireˉsuccess(Uninstall, "offline installation uninstall");
    assert.match(Uninstall.stdout, /result=removed/);
    for (const OwnedName of ["state", "generations", "store"]) {
        assert.equal(fs.existsSync(path.join(Install, OwnedName)), false);
    }
    assert.equal(fs.readFileSync(ApplicationData, "utf8"), "preserve-user-data");
    assert.equal(fs.readFileSync(Unrelated, "utf8"), "preserve-unrelated-file");
    const RepeatUninstall = Node(UNINSTALLER, "uninstall", Install);
    Requireˉsuccess(RepeatUninstall, "repeated offline installation uninstall");
    assert.match(RepeatUninstall.stdout, /result=already-absent/);

    process.stdout.write(
        "native offline generation lifecycle status=Passed cases=15 generations=2 " +
        "activations=3 recoveries=2 command-observations=8 rollback=Verified " +
        "uninstall=Verified preservation=2\n",
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
