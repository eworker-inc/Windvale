import { createHash } from "node:crypto";
import {
    Boundedˉparallelˉtaskˉscheduler,
} from "../../Runtime/Hosted/Tasks/Bounded-Parallel-Task-Scheduler.mjs";

const Executor = new URL(
    "../../Tests/Fixtures/Hosted/Bounded-Parallel-Task-Executor.mjs",
    import.meta.url,
);
const EXPECTED_COMPLETION_ORDER = Object.freeze([3, 1, 0, 2]);
const EXPECTED_TRANSCRIPT = "3\n1\n0\n2\nResult: 42\n";
const EXPECTED_TRANSCRIPT_SHA256 =
    "ec46e8f65ee74954a51180781f35d37e1d1b36dda92e4c4ac2872a1b0eefb576";

let Cases = 0;

function Require(Condition, Message) {
    Cases += 1;
    if (!Condition) throw new Error(Message);
}

function Work(Mode, Value, Units) {
    const Bytes = Buffer.alloc(12);
    Bytes.writeUInt32LE(Mode, 0);
    Bytes.writeUInt32LE(Value, 4);
    Bytes.writeUInt32LE(Units, 8);
    return Bytes;
}

function Limits(Overrides = {}) {
    return Object.freeze({
        maximumChildren: 4,
        maximumRunnable: 4,
        maximumCompleted: 4,
        maximumRetainedBytes: 300_000n,
        maximumWorkUnits: 800_000n,
        maximumWorkers: 4,
        maximumWorkerMilliseconds: 10_000,
        ...Overrides,
    });
}

function Digest(Value) {
    return createHash("sha256").update(Value).digest("hex");
}

async function Requireˉrejection(Action, Fragment, Description) {
    let Rejected = false;
    try { await Action(); } catch (Failure) {
        Rejected = Failure instanceof Error && Failure.message.includes(Fragment);
    }
    Require(Rejected, `${Description} was not rejected exactly.`);
}

async function Parallelˉsuccess() {
    const Coordination = Object.freeze({
        state: new SharedArrayBuffer(8),
        participants: 4,
    });
    const Scheduler = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits(),
        executor: Executor,
        completionOrder: EXPECTED_COMPLETION_ORDER,
    });
    const Handles = [];
    for (let Index = 0; Index < 4; Index += 1) {
        const Spawned = Scheduler.spawn({
            work: Work(0, Index, 120_000),
            maximumWorkUnits: 120_000n,
            coordination: Coordination,
        });
        Require(Spawned.accepted, `Parallel child ${Index} was rejected.`);
        Handles.push(Spawned.handle);
    }
    const Outcomes = await Promise.all(Handles.map(Handle => Scheduler.await(Handle)));
    for (let Index = 0; Index < Outcomes.length; Index += 1) {
        Require(Outcomes[Index].kind === 0, `Parallel child ${Index} did not succeed.`);
        Require(Outcomes[Index].value.readUInt32LE(0) === Index,
            `Parallel child ${Index} lost its creation identity.`);
    }
    const Snapshot = Scheduler.snapshot();
    Require(Snapshot.peakActive === 4, "Four hosted workers were not live together.");
    Require(new Int32Array(Coordination.state)[0] === 4,
        "Four task executors did not reach the shared rendezvous.");
    Require(new Int32Array(Coordination.state)[1] === 4,
        "Four task executors did not leave the shared rendezvous.");
    Require(JSON.stringify(Snapshot.completionOrder) ===
        JSON.stringify(EXPECTED_COMPLETION_ORDER),
    "Hosted completion order differs from the canonical task observation.");
    Require(Snapshot.live === 0 && Snapshot.retainedBytes === 0n &&
        Snapshot.reservedWorkUnits === 0n,
    "Creation-ordered awaits did not release scheduler state.");
    const Result = 38 + Outcomes.reduce(
        (Sum, Outcome) => Sum + Outcome.value.readUInt32LE(0), 0,
    ) - 2;
    Require(Result === 42, "Creation-ordered task results differ.");
    const Transcript = `${Snapshot.completionOrder.join("\n")}\nResult: ${Result}\n`;
    Require(Transcript === EXPECTED_TRANSCRIPT, "Canonical task transcript differs.");
    Require(Digest(Transcript) === EXPECTED_TRANSCRIPT_SHA256,
        "Canonical task transcript identity differs.");
    await Scheduler.teardown({ cancel: false });
    return Snapshot;
}

async function Rejections() {
    const Invalidˉlimits = [
        [Limits({ maximumChildren: 0 }), "Maximum children"],
        [Limits({ maximumRunnable: 5 }), "Maximum runnable children"],
        [Limits({ maximumCompleted: 3 }), "Maximum retained completions"],
        [Limits({ maximumRetainedBytes: 0n }), "Maximum retained bytes"],
        [Limits({ maximumWorkUnits: 0n }), "Maximum work units"],
        [Limits({ maximumWorkers: 0 }), "Maximum workers"],
    ];
    for (const [limits, Message] of Invalidˉlimits) {
        await Requireˉrejection(
            () => new Boundedˉparallelˉtaskˉscheduler({ limits, executor: Executor }),
            Message,
            Message,
        );
    }
    await Requireˉrejection(
        () => new Boundedˉparallelˉtaskˉscheduler({
            limits: Limits(),
            executor: "https://invalid.example/task.mjs",
        }),
        "executor identity",
        "Non-file task executor",
    );
    await Requireˉrejection(
        () => new Boundedˉparallelˉtaskˉscheduler({
            limits: Limits(),
            executor: Executor,
            completionOrder: [3, 3],
        }),
        "completion policy",
        "Duplicate completion lane",
    );

    const Scheduler = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits({
            maximumChildren: 1,
            maximumRunnable: 1,
            maximumCompleted: 1,
            maximumWorkers: 1,
        }),
        executor: Executor,
    });
    const Accepted = Scheduler.spawn({
        work: Work(1, 7, 1),
        maximumWorkUnits: 1n,
    });
    Require(Accepted.accepted, "First bounded task was rejected.");
    const Refused = Scheduler.spawn({
        work: Work(1, 8, 1),
        maximumWorkUnits: 1n,
    });
    Require(!Refused.accepted && Refused.reason === "task_limit" &&
        Refused.work.equals(Work(1, 8, 1)),
    "Task-limit rejection did not return exact work.");
    const Outcome = await Scheduler.await(Accepted.handle);
    Require(Outcome.kind === 1 && Outcome.value[0] === 7,
        "Typed child failure did not survive await.");
    await Requireˉrejection(
        () => Scheduler.await(Accepted.handle),
        "stale or already consumed",
        "Double await",
    );
    const Cancel = Scheduler.requestCancel();
    Require(!Cancel.alreadyRequested && Cancel.liveChildren === 0,
        "First cancellation request differs.");
    const Cancelˉagain = Scheduler.requestCancel();
    Require(Cancelˉagain.alreadyRequested,
        "Second cancellation request was not idempotent.");
    const Closed = Scheduler.spawn({
        work: Work(1, 9, 1),
        maximumWorkUnits: 1n,
    });
    Require(!Closed.accepted && Closed.reason === "scope_closing",
        "Closing scope accepted new work.");
    await Scheduler.teardown();

    const Queueˉlimited = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits({
            maximumChildren: 2,
            maximumRunnable: 1,
            maximumCompleted: 1,
            maximumWorkers: 1,
        }),
        executor: Executor,
    });
    const Queueˉaccepted = Queueˉlimited.spawn({
        work: Work(3, 1, 1), maximumWorkUnits: 1n,
    });
    const Queueˉrefused = Queueˉlimited.spawn({
        work: Work(1, 2, 1), maximumWorkUnits: 1n,
    });
    Require(Queueˉaccepted.accepted && !Queueˉrefused.accepted &&
        Queueˉrefused.reason === "queue_limit",
    "Runnable/completion reservation was not enforced.");
    Queueˉlimited.requestCancel();
    const Queueˉoutcome = await Queueˉlimited.await(Queueˉaccepted.handle);
    Require(Queueˉoutcome.kind === 2,
        "Live child did not observe cooperative cancellation.");
    await Queueˉlimited.teardown();

    const Memoryˉlimited = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits({ maximumRetainedBytes: 65_000n }),
        executor: Executor,
    });
    const Memoryˉrefused = Memoryˉlimited.spawn({
        work: Work(1, 1, 1), maximumWorkUnits: 1n,
    });
    Require(!Memoryˉrefused.accepted &&
        Memoryˉrefused.reason === "memory_failure",
    "Retained-memory reservation was not enforced before transfer.");
    await Memoryˉlimited.teardown();

    const Workˉlimited = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits({
            maximumChildren: 2,
            maximumRunnable: 2,
            maximumCompleted: 2,
            maximumWorkUnits: 10n,
            maximumWorkers: 1,
        }),
        executor: Executor,
    });
    const Workˉaccepted = Workˉlimited.spawn({
        work: Work(1, 1, 10), maximumWorkUnits: 10n,
    });
    const Workˉrefused = Workˉlimited.spawn({
        work: Work(1, 2, 1), maximumWorkUnits: 1n,
    });
    Require(Workˉaccepted.accepted && !Workˉrefused.accepted &&
        Workˉrefused.reason === "work_limit",
    "Work-unit reservation was not enforced before transfer.");
    await Workˉlimited.await(Workˉaccepted.handle);
    await Workˉlimited.teardown({ cancel: false });
}

async function Runtimeˉoutcomes() {
    const Lost = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits(),
        executor: Executor,
        runtimeGeneration: 41n,
        observedRuntimeGeneration: 0n,
    });
    const Lostˉspawn = Lost.spawn({
        work: Work(1, 1, 1), maximumWorkUnits: 1n,
    });
    Require(Lostˉspawn.accepted, "Runtime-loss work was not accepted.");
    const Lostˉoutcome = await Lost.await(Lostˉspawn.handle);
    Require(Lostˉoutcome.kind === 4 && Lostˉoutcome.firstEvidence === 41n,
        "Runtime-loss generations differ.");
    await Lost.teardown({ cancel: false });

    const Restarted = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits(),
        executor: Executor,
        runtimeGeneration: 41n,
        observedRuntimeGeneration: 42n,
    });
    const Restartedˉspawn = Restarted.spawn({
        work: Work(1, 1, 1), maximumWorkUnits: 1n,
    });
    Require(Restartedˉspawn.accepted, "Runtime-restart work was not accepted.");
    const Restartedˉoutcome = await Restarted.await(Restartedˉspawn.handle);
    Require(Restartedˉoutcome.kind === 5 &&
        Restartedˉoutcome.firstEvidence === 41n &&
        Restartedˉoutcome.secondEvidence === 42n,
    "Runtime-restart generations differ.");
    await Restarted.teardown({ cancel: false });

    const Trapped = new Boundedˉparallelˉtaskˉscheduler({
        limits: Limits(), executor: Executor,
    });
    const Trappedˉspawn = Trapped.spawn({
        work: Work(2, 1, 1), maximumWorkUnits: 1n,
    });
    Require(Trappedˉspawn.accepted, "Trap work was not accepted.");
    const Trappedˉoutcome = await Trapped.await(Trappedˉspawn.handle);
    Require(Trappedˉoutcome.kind === 6 &&
        Trappedˉoutcome.firstEvidence === 3007n,
    "Contained worker trap differs.");
    await Trapped.teardown({ cancel: false });

    for (const [Mode, Name] of [[4, "lifetime"], [5, "protocol"]]) {
        const Lostˉworker = new Boundedˉparallelˉtaskˉscheduler({
            limits: Limits({ maximumWorkerMilliseconds: 100 }),
            executor: Executor,
        });
        const Lostˉworkerˉspawn = Lostˉworker.spawn({
            work: Work(Mode, 1, 1), maximumWorkUnits: 1n,
        });
        Require(Lostˉworkerˉspawn.accepted,
            `Worker ${Name} work was not accepted.`);
        const Lostˉworkerˉoutcome = await Lostˉworker.await(
            Lostˉworkerˉspawn.handle,
        );
        Require(Lostˉworkerˉoutcome.kind === 4 &&
            Lostˉworkerˉoutcome.firstEvidence === 1n,
        `Worker ${Name} failure was not exact runtime loss.`);
        await Lostˉworker.teardown({ cancel: false });
    }
}

async function Main() {
    if (process.argv.length !== 2) {
        process.stderr.write(
            "Usage: node Tools/Native/Test-Bounded-Parallel-Task-Scheduler.mjs\n",
        );
        process.exitCode = 64;
        return;
    }
    if (process.platform !== "win32" && process.platform !== "linux") {
        throw new Error(`Unsupported hosted task scheduler: ${process.platform}.`);
    }
    await Rejections();
    const Snapshot = await Parallelˉsuccess();
    await Runtimeˉoutcomes();
    process.stdout.write(
        "bounded parallel task scheduler status=Passed " +
        `cases=${Cases} workers=${Snapshot.peakActive} ` +
        `completion-order=${Snapshot.completionOrder.join(",")} ` +
        `join-order=0,1,2,3 result=42 ` +
        `transcript-sha256=${EXPECTED_TRANSCRIPT_SHA256}\n`,
    );
}

Main().catch(Failure => {
    process.stderr.write(`${Failure instanceof Error ? Failure.stack : Failure}\n`);
    process.exitCode = 1;
});
