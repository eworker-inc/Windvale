import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

const A = "1".repeat(64);
const B = "2".repeat(64);

function Activation(Serial, Current, Previous) {
    return "windvale-activation 1\n" +
        `serial ${Serial}\ncurrent ${Current}\nprevious ${Previous}\n`;
}

function Plan(Serial, Current, Previous) {
    const Value = BigInt(Serial);
    const Low = Number(Value & 0xffff_ffffn);
    const High = Number(Value >> 32n);
    return "windvale-activation-plan 1\n" +
        `serial-low ${Low}\nserial-high ${High}\n` +
        `current ${Current}\nprevious ${Previous}\n`;
}

function Run(Executable, ...Arguments) {
    return spawnSync(Executable, Arguments, { encoding: "utf8", windowsHide: true });
}

function Requireˉsuccess(Result, Expected, Description) {
    assert.equal(Result.status, 0, `${Description}: ${Result.stderr}`);
    assert.equal(Result.stderr, "", Description);
    assert.equal(Result.stdout, Expected, Description);
}

function Requireˉfailure(Result, Expected) {
    assert.equal(Result.status, 1);
    assert.equal(Result.stdout, "");
    assert.equal(Result.stderr, `activation plan status=${Expected}\n`);
}

const [ExecutableArgument] = process.argv.slice(2);
if (process.argv.length !== 3) {
    process.stderr.write(
        "Usage: node Verify-Installation-Activation-Planner.mjs <planner-executable>\n",
    );
    process.exit(64);
}
const Executable = path.resolve(ExecutableArgument);
const Work = fs.mkdtempSync(path.join(os.tmpdir(), "windvale-activation-plan-"));
try {
    const InitialPath = path.join(Work, "Initial.txt");
    const UpdatedPath = path.join(Work, "Updated.txt");
    const ExhaustedPath = path.join(Work, "Exhausted.txt");
    const InvalidPath = path.join(Work, "Invalid.txt");
    fs.writeFileSync(InitialPath, Activation("1", A, "none"), { flag: "wx" });
    fs.writeFileSync(UpdatedPath, Activation("2", B, A), { flag: "wx" });
    fs.writeFileSync(
        ExhaustedPath,
        Activation("18446744073709551615", B, A),
        { flag: "wx" },
    );
    fs.writeFileSync(InvalidPath, "wrong\n", { flag: "wx" });

    process.stdout.write("activation planner step=activate item=1/12\n");
    Requireˉsuccess(
        Run(Executable, "activate", InitialPath, B, "present", "present"),
        Plan("2", B, A),
        "activation plan",
    );

    process.stdout.write("activation planner step=idempotent item=2/12\n");
    Requireˉsuccess(
        Run(Executable, "activate", InitialPath, A, "present", "present"),
        Plan("1", A, "none"),
        "idempotent plan",
    );

    process.stdout.write("activation planner step=rollback item=3/12\n");
    Requireˉsuccess(
        Run(Executable, "rollback", UpdatedPath, "none", "present", "present"),
        Plan("3", A, B),
        "rollback plan",
    );

    process.stdout.write("activation planner step=reject-current-missing item=4/12\n");
    Requireˉfailure(
        Run(Executable, "activate", InitialPath, B, "missing", "present"),
        "Current-generation-missing",
    );

    process.stdout.write("activation planner step=reject-requested-missing item=5/12\n");
    Requireˉfailure(
        Run(Executable, "activate", InitialPath, B, "present", "missing"),
        "Requested-generation-missing",
    );

    process.stdout.write("activation planner step=reject-requested-identity item=6/12\n");
    Requireˉfailure(
        Run(Executable, "activate", InitialPath, "wrong", "present", "present"),
        "Invalid-requested-generation",
    );

    process.stdout.write("activation planner step=reject-no-previous item=7/12\n");
    Requireˉfailure(
        Run(Executable, "rollback", InitialPath, "none", "present", "present"),
        "No-previous-generation",
    );

    process.stdout.write("activation planner step=reject-previous-missing item=8/12\n");
    Requireˉfailure(
        Run(Executable, "rollback", UpdatedPath, "none", "present", "missing"),
        "Previous-generation-missing",
    );

    process.stdout.write("activation planner step=reject-serial-exhaustion item=9/12\n");
    Requireˉfailure(
        Run(Executable, "rollback", ExhaustedPath, "none", "present", "present"),
        "Serial-exhausted",
    );

    process.stdout.write("activation planner step=reject-activation item=10/12\n");
    Requireˉfailure(
        Run(Executable, "activate", InvalidPath, B, "present", "present"),
        "Invalid-activation",
    );

    process.stdout.write("activation planner step=reject-mode item=11/12\n");
    Requireˉfailure(
        Run(Executable, "other", InitialPath, "none", "present", "present"),
        "Invalid-mode",
    );

    process.stdout.write("activation planner step=reject-invocation item=12/12\n");
    const Invocation = Run(Executable);
    assert.equal(Invocation.status, 64);
    assert.equal(Invocation.stdout, "");
    assert.match(Invocation.stderr, /^Usage: wvactivation-plan /);

    process.stdout.write(
        "native installation activation planning status=Passed cases=12 " +
        "transitions=3 rejections=9\n",
    );
} finally {
    fs.rmSync(Work, { recursive: true, force: true });
}
