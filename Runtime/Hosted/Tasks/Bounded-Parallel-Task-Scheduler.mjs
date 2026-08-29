import { Worker } from "node:worker_threads";

const WORKER_URL = new URL(
    "./Bounded-Parallel-Task-Worker.mjs",
    import.meta.url,
);
const MAXIMUM_CHILDREN = 64;
const MAXIMUM_WORK_BYTES = 65_536;
const MAXIMUM_OUTCOME_BYTES = 65_536;
const TASK_RECORD_BYTES = 56n;

function Fail(Message) {
    throw new Error(Message);
}

function U32(Value, Minimum, Maximum, Description) {
    if (!Number.isInteger(Value) || Value < Minimum || Value > Maximum) {
        Fail(`${Description} is invalid.`);
    }
    return Value;
}

function U64(Value, Minimum, Maximum, Description) {
    if (typeof Value !== "bigint" || Value < Minimum || Value > Maximum) {
        Fail(`${Description} is invalid.`);
    }
    return Value;
}

function Executorˉidentity(Value) {
    const Candidate = Value instanceof URL ? new URL(Value.href) : new URL(Value);
    if (Candidate.protocol !== "file:" || Candidate.search !== "" ||
        Candidate.hash !== "") {
        Fail("Task executor identity is invalid.");
    }
    return Candidate.href;
}

function Workˉbytes(Value) {
    if (!(Value instanceof Uint8Array) || Value.byteLength < 1 ||
        Value.byteLength > MAXIMUM_WORK_BYTES) {
        Fail("Task work is invalid.");
    }
    return Buffer.from(Value);
}

function Coordinationˉvalue(Value) {
    if (Value === undefined || Value === null) return null;
    if (typeof Value !== "object" ||
        !(Value.state instanceof SharedArrayBuffer) ||
        Value.state.byteLength !== 8 ||
        !Number.isInteger(Value.participants) || Value.participants < 2 ||
        Value.participants > MAXIMUM_CHILDREN) {
        Fail("Task coordination is invalid.");
    }
    return Object.freeze({
        state: Value.state,
        participants: Value.participants,
    });
}

function Limits(Value) {
    if (typeof Value !== "object" || Value === null) {
        Fail("Task limits are absent.");
    }
    const Maximumˉchildren = U32(
        Value.maximumChildren, 1, MAXIMUM_CHILDREN, "Maximum children",
    );
    const Maximumˉrunnable = U32(
        Value.maximumRunnable, 1, Maximumˉchildren, "Maximum runnable children",
    );
    const Maximumˉcompleted = U32(
        Value.maximumCompleted, Maximumˉrunnable, Maximumˉchildren,
        "Maximum retained completions",
    );
    return Object.freeze({
        maximumChildren: Maximumˉchildren,
        maximumRunnable: Maximumˉrunnable,
        maximumCompleted: Maximumˉcompleted,
        maximumRetainedBytes: U64(
            Value.maximumRetainedBytes, 1n, 1_048_576n,
            "Maximum retained bytes",
        ),
        maximumWorkUnits: U64(
            Value.maximumWorkUnits, 1n, 1_000_000n,
            "Maximum work units",
        ),
        maximumWorkers: U32(
            Value.maximumWorkers, 1, Maximumˉrunnable, "Maximum workers",
        ),
        maximumWorkerMilliseconds: U32(
            Value.maximumWorkerMilliseconds ?? 30_000,
            1,
            300_000,
            "Maximum worker lifetime",
        ),
    });
}

function Handleˉkey(Handle) {
    if (typeof Handle !== "object" || Handle === null ||
        !Number.isInteger(Handle.identity) || Handle.identity < 1 ||
        Handle.identity > MAXIMUM_CHILDREN ||
        !Number.isInteger(Handle.generation) || Handle.generation < 1 ||
        Handle.generation > 0xffff_ffff) {
        Fail("Task handle is invalid.");
    }
    return `${Handle.identity}:${Handle.generation}`;
}

function Completionˉorder(Value, Maximum) {
    if (Value === undefined) return null;
    if (!Array.isArray(Value) || Value.length !== Maximum ||
        Value.some(Item => !Number.isInteger(Item) || Item < 0 || Item >= Maximum) ||
        new Set(Value).size !== Value.length) {
        Fail("Task completion policy is invalid.");
    }
    return Object.freeze([...Value]);
}

function Frozenˉoutcome(Task, Kind, Value, Firstˉevidence, Secondˉevidence) {
    const Bytes = Buffer.from(Value);
    if (Bytes.byteLength > MAXIMUM_OUTCOME_BYTES) {
        Fail("Task outcome exceeds its byte bound.");
    }
    const Carriesˉvalue = Kind === 0 || Kind === 1;
    const Evidenceˉisˉvalid =
        (Carriesˉvalue && Firstˉevidence === 0n && Secondˉevidence === 0n) ||
        ((Kind === 2 || Kind === 3) && Bytes.byteLength === 0 &&
            Firstˉevidence === 0n && Secondˉevidence === 0n) ||
        (Kind === 4 && Bytes.byteLength === 0 && Firstˉevidence > 0n &&
            Secondˉevidence === 0n) ||
        (Kind === 5 && Bytes.byteLength === 0 && Firstˉevidence > 0n &&
            Secondˉevidence > 0n && Firstˉevidence !== Secondˉevidence) ||
        (Kind === 6 && Bytes.byteLength === 0 && Firstˉevidence > 0n &&
            Firstˉevidence <= 0xffff_ffffn && Secondˉevidence === 0n);
    if (!Evidenceˉisˉvalid) Fail("Task outcome evidence is invalid.");
    return Object.freeze({
        identity: Task.identity,
        generation: Task.generation,
        kind: Kind,
        value: Bytes,
        firstEvidence: Firstˉevidence,
        secondEvidence: Secondˉevidence,
    });
}

export class Boundedˉparallelˉtaskˉscheduler {
    constructor({
        limits,
        executor,
        runtimeGeneration = 1n,
        observedRuntimeGeneration = runtimeGeneration,
        completionOrder,
    }) {
        this.limits = Limits(limits);
        this.executorUrl = Executorˉidentity(executor);
        this.runtimeGeneration = U64(
            runtimeGeneration, 1n, 0xffff_ffff_ffff_ffffn,
            "Task runtime generation",
        );
        this.observedRuntimeGeneration = U64(
            observedRuntimeGeneration, 0n, 0xffff_ffff_ffff_ffffn,
            "Observed task runtime generation",
        );
        this.completionPolicy = Completionˉorder(
            completionOrder,
            this.limits.maximumChildren,
        );
        this.cancellation = new Int32Array(new SharedArrayBuffer(4));
        this.tasks = new Map();
        this.queue = [];
        this.completions = new Map();
        this.waiters = new Map();
        this.completionSequence = [];
        this.nextCompletionPolicyIndex = 0;
        this.nextIdentity = 1;
        this.accepted = 0;
        this.active = 0;
        this.peakActive = 0;
        this.retainedBytes = 0n;
        this.workUnits = 0n;
        this.closing = false;
    }

    spawn({ work, maximumWorkUnits, coordination }) {
        const Work = Workˉbytes(work);
        const Maximumˉworkˉunits = U64(
            maximumWorkUnits, 1n, this.limits.maximumWorkUnits,
            "Child work-unit limit",
        );
        const Coordination = Coordinationˉvalue(coordination);
        const Reservation = TASK_RECORD_BYTES + BigInt(Work.byteLength) +
            BigInt(MAXIMUM_OUTCOME_BYTES);
        let Reason = null;
        if (this.closing) Reason = "scope_closing";
        else if (this.accepted >= this.limits.maximumChildren) Reason = "task_limit";
        else if (this.tasks.size >= this.limits.maximumRunnable ||
            this.tasks.size >= this.limits.maximumCompleted) Reason = "queue_limit";
        else if (Reservation > this.limits.maximumRetainedBytes - this.retainedBytes) {
            Reason = "memory_failure";
        } else if (Maximumˉworkˉunits >
            this.limits.maximumWorkUnits - this.workUnits) {
            Reason = "work_limit";
        }
        if (Reason !== null) {
            return Object.freeze({
                accepted: false,
                reason: Reason,
                work: Buffer.from(Work),
            });
        }
        const Identity = this.nextIdentity;
        this.nextIdentity += 1;
        this.accepted += 1;
        const Task = {
            identity: Identity,
            generation: 1,
            ordinal: this.accepted - 1,
            work: Work,
            maximumWorkUnits: Maximumˉworkˉunits,
            coordination: Coordination,
            reservation: Reservation,
            status: "queued",
            worker: null,
            outcome: null,
            consumed: false,
        };
        this.tasks.set(`${Identity}:1`, Task);
        this.queue.push(Task);
        this.retainedBytes += Reservation;
        this.workUnits += Maximumˉworkˉunits;
        const Handle = Object.freeze({ identity: Identity, generation: 1 });
        this.#Dispatch();
        return Object.freeze({ accepted: true, handle: Handle });
    }

    requestCancel() {
        const Already = Atomics.exchange(this.cancellation, 0, 1) !== 0;
        this.closing = true;
        this.#Publish();
        return Object.freeze({
            alreadyRequested: Already,
            liveChildren: this.tasks.size,
        });
    }

    await(Handle) {
        let Key;
        try { Key = Handleˉkey(Handle); } catch (Error) {
            return Promise.reject(Error);
        }
        const Task = this.tasks.get(Key);
        if (!Task || Task.consumed) {
            return Promise.reject(new Error("Task handle is stale or already consumed."));
        }
        Task.consumed = true;
        const Completion = this.completions.get(Key);
        if (Completion) return Promise.resolve(this.#Consume(Task, Completion));
        return new Promise((Resolve, Reject) => {
            this.waiters.set(Key, { resolve: Resolve, reject: Reject });
        });
    }

    async teardown({ cancel = true } = {}) {
        this.closing = true;
        if (cancel) Atomics.store(this.cancellation, 0, 1);
        this.#Publish();
        const Pending = [];
        for (const Task of this.tasks.values()) {
            if (Task.consumed) continue;
            Pending.push(this.await(Object.freeze({
                identity: Task.identity,
                generation: Task.generation,
            })).catch(() => null));
        }
        await Promise.all(Pending);
        for (const Waiter of this.waiters.values()) {
            Waiter.reject(new Error("Task scope tore down before observation."));
        }
        this.waiters.clear();
        if (this.tasks.size !== 0 || this.retainedBytes !== 0n) {
            Fail("Task scope teardown did not release all retained state.");
        }
    }

    snapshot() {
        return Object.freeze({
            accepted: this.accepted,
            live: this.tasks.size,
            active: this.active,
            peakActive: this.peakActive,
            retainedBytes: this.retainedBytes,
            reservedWorkUnits: this.workUnits,
            completionOrder: Object.freeze([...this.completionSequence]),
            cancellationRequested: Atomics.load(this.cancellation, 0) !== 0,
        });
    }

    #Dispatch() {
        while (this.active < this.limits.maximumWorkers &&
            this.queue.length !== 0) {
            const Task = this.queue.shift();
            if (this.observedRuntimeGeneration === 0n) {
                this.#Ready(Task, Frozenˉoutcome(
                    Task, 4, new Uint8Array(), this.runtimeGeneration, 0n,
                ));
                continue;
            }
            if (this.observedRuntimeGeneration !== this.runtimeGeneration) {
                this.#Ready(Task, Frozenˉoutcome(
                    Task, 5, new Uint8Array(), this.runtimeGeneration,
                    this.observedRuntimeGeneration,
                ));
                continue;
            }
            Task.status = "running";
            const Workerˉinstance = new Worker(WORKER_URL, {
                workerData: {
                    identity: Task.identity,
                    generation: Task.generation,
                    runtimeGeneration: this.runtimeGeneration,
                    maximumWorkUnits: Task.maximumWorkUnits,
                    work: Task.work,
                    cancellation: this.cancellation.buffer,
                    coordination: Task.coordination,
                    executorUrl: this.executorUrl,
                },
            });
            Task.worker = Workerˉinstance;
            this.active += 1;
            if (this.active > this.peakActive) this.peakActive = this.active;
            let Settled = false;
            const Lose = () => {
                if (Settled) return;
                Settled = true;
                clearTimeout(Timer);
                this.#Ready(Task, Frozenˉoutcome(
                    Task, 4, new Uint8Array(), this.runtimeGeneration, 0n,
                ));
            };
            const Timer = setTimeout(() => {
                Workerˉinstance.terminate().catch(() => {});
                Lose();
            }, this.limits.maximumWorkerMilliseconds);
            Workerˉinstance.on("message", Message => {
                if (Settled || typeof Message !== "object" || Message === null) return;
                if (Message.kind === "started") {
                    if (Message.identity !== Task.identity ||
                        Message.generation !== Task.generation) {
                        Workerˉinstance.terminate();
                        Lose();
                    }
                    return;
                }
                Settled = true;
                clearTimeout(Timer);
                if (Message.kind === "ready" &&
                    Message.identity === Task.identity &&
                    Message.generation === Task.generation) {
                    try {
                        const Outcome = Frozenˉoutcome(
                            Task,
                            U32(Message.outcomeKind, 0, 6, "Task outcome kind"),
                            Message.value,
                            U64(Message.firstEvidence, 0n,
                                0xffff_ffff_ffff_ffffn, "Task first evidence"),
                            U64(Message.secondEvidence, 0n,
                                0xffff_ffff_ffff_ffffn, "Task second evidence"),
                        );
                        if (U64(Message.workUnits, 0n, Task.maximumWorkUnits,
                            "Task consumed work units") > Task.maximumWorkUnits) {
                            Fail("Task consumed too many work units.");
                        }
                        this.#Ready(Task, Outcome);
                    } catch {
                        this.#Ready(Task, Frozenˉoutcome(
                            Task, 4, new Uint8Array(), this.runtimeGeneration, 0n,
                        ));
                    }
                    return;
                }
                this.#Ready(Task, Frozenˉoutcome(
                    Task, 4, new Uint8Array(), this.runtimeGeneration, 0n,
                ));
            });
            Workerˉinstance.on("error", Lose);
            Workerˉinstance.on("exit", Lose);
        }
    }

    #Ready(Task, Outcome) {
        if (Task.status === "running") this.active -= 1;
        Task.status = "ready";
        Task.outcome = Outcome;
        Task.worker = null;
        this.#Publish();
        this.#Dispatch();
    }

    #Publish() {
        let Progress = true;
        while (Progress) {
            Progress = false;
            let Candidate = null;
            if (this.completionPolicy === null) {
                for (const Task of this.tasks.values()) {
                    if (Task.status === "ready") {
                        Candidate = Task;
                        break;
                    }
                }
            } else if (this.nextCompletionPolicyIndex <
                this.completionPolicy.length) {
                const Ordinal = this.completionPolicy[
                    this.nextCompletionPolicyIndex
                ];
                for (const Task of this.tasks.values()) {
                    if (Task.ordinal === Ordinal && Task.status === "ready") {
                        Candidate = Task;
                        break;
                    }
                }
                if (Candidate === null && this.closing) {
                    let Exists = false;
                    for (const Task of this.tasks.values()) {
                        if (Task.ordinal === Ordinal) {
                            Exists = true;
                            break;
                        }
                    }
                    if (!Exists) {
                        this.nextCompletionPolicyIndex += 1;
                        Progress = true;
                        continue;
                    }
                }
            }
            if (Candidate === null) return;
            Candidate.status = "completed";
            const Key = `${Candidate.identity}:${Candidate.generation}`;
            this.completions.set(Key, Candidate.outcome);
            this.completionSequence.push(Candidate.ordinal);
            if (this.completionPolicy !== null) this.nextCompletionPolicyIndex += 1;
            const Waiter = this.waiters.get(Key);
            if (Waiter) {
                this.waiters.delete(Key);
                try { Waiter.resolve(this.#Consume(Candidate, Candidate.outcome)); }
                catch (Error) { Waiter.reject(Error); }
            }
            Progress = true;
        }
    }

    #Consume(Task, Outcome) {
        const Key = `${Task.identity}:${Task.generation}`;
        this.completions.delete(Key);
        this.tasks.delete(Key);
        this.retainedBytes -= Task.reservation;
        this.workUnits -= Task.maximumWorkUnits;
        return Outcome;
    }
}
