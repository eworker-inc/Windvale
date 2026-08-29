import { parentPort, workerData } from "node:worker_threads";

const MAXIMUM_RESULT_BYTES = 65_536;
const MAXIMUM_DIAGNOSTIC_BYTES = 4_096;

function Fail(Message) {
    throw new Error(Message);
}

function U32(Value, Description) {
    if (!Number.isInteger(Value) || Value < 0 || Value > 0xffff_ffff) {
        Fail(`${Description} is invalid.`);
    }
    return Value;
}

function U64(Value, Description, AllowZero = false) {
    const Minimum = AllowZero ? 0n : 1n;
    if (typeof Value !== "bigint" || Value < Minimum ||
        Value > 0xffff_ffff_ffff_ffffn) {
        Fail(`${Description} is invalid.`);
    }
    return Value;
}

function Exactˉbytes(Value, Maximum, Description) {
    if (!(Value instanceof Uint8Array) || Value.byteLength > Maximum) {
        Fail(`${Description} is invalid.`);
    }
    return Buffer.from(Value);
}

function Exactˉcoordination(Value) {
    if (Value === null) return null;
    if (typeof Value !== "object" ||
        !(Value.state instanceof SharedArrayBuffer) ||
        Value.state.byteLength !== 8 ||
        !Number.isInteger(Value.participants) || Value.participants < 2 ||
        Value.participants > 64) {
        Fail("Task coordination is invalid.");
    }
    return Object.freeze({
        state: new Int32Array(Value.state),
        participants: Value.participants,
    });
}

function Diagnostic(Error) {
    const Message = Error instanceof Error ? Error.message : String(Error);
    return Buffer.from(Message, "utf8").subarray(0, MAXIMUM_DIAGNOSTIC_BYTES);
}

async function Main() {
    if (parentPort === null || typeof workerData !== "object" ||
        workerData === null) {
        Fail("Task worker launch is invalid.");
    }
    const Identity = U32(workerData.identity, "Task identity");
    const Generation = U32(workerData.generation, "Task generation");
    const Runtimeˉgeneration = U64(
        workerData.runtimeGeneration,
        "Task runtime generation",
    );
    const Maximumˉworkˉunits = U64(
        workerData.maximumWorkUnits,
        "Task work-unit limit",
    );
    const Work = Exactˉbytes(workerData.work, 65_536, "Task work");
    const Cancellation = workerData.cancellation instanceof SharedArrayBuffer &&
        workerData.cancellation.byteLength === 4
        ? new Int32Array(workerData.cancellation)
        : Fail("Task cancellation state is invalid.");
    const Coordination = Exactˉcoordination(workerData.coordination ?? null);
    const Executorˉurl = new URL(workerData.executorUrl);
    if (Executorˉurl.protocol !== "file:" || Executorˉurl.search !== "" ||
        Executorˉurl.hash !== "") {
        Fail("Task executor identity is invalid.");
    }
    const Executor = await import(Executorˉurl.href);
    if (typeof Executor.Executeˉboundedˉhostedˉtask !== "function") {
        Fail("Task executor entry is absent.");
    }
    parentPort.postMessage(Object.freeze({
        kind: "started",
        identity: Identity,
        generation: Generation,
    }));
    const Result = await Executor.Executeˉboundedˉhostedˉtask(Object.freeze({
        identity: Identity,
        generation: Generation,
        runtimeGeneration: Runtimeˉgeneration,
        maximumWorkUnits: Maximumˉworkˉunits,
        work: Work,
        cancellation: Cancellation,
        coordination: Coordination,
    }));
    if (typeof Result !== "object" || Result === null ||
        !Number.isInteger(Result.kind) || Result.kind < 0 || Result.kind > 6) {
        Fail("Task executor outcome is invalid.");
    }
    const Value = Exactˉbytes(
        Result.value ?? new Uint8Array(),
        MAXIMUM_RESULT_BYTES,
        "Task result",
    );
    const Firstˉevidence = U64(
        Result.firstEvidence ?? 0n,
        "Task first evidence",
        true,
    );
    const Secondˉevidence = U64(
        Result.secondEvidence ?? 0n,
        "Task second evidence",
        true,
    );
    const Workˉunits = U64(
        Result.workUnits,
        "Task consumed work units",
        true,
    );
    if (Workˉunits > Maximumˉworkˉunits) {
        Fail("Task executor exceeded its work-unit limit.");
    }
    parentPort.postMessage(Object.freeze({
        kind: "ready",
        identity: Identity,
        generation: Generation,
        outcomeKind: Result.kind,
        value: Value,
        firstEvidence: Firstˉevidence,
        secondEvidence: Secondˉevidence,
        workUnits: Workˉunits,
    }));
}

Main().catch(Error => {
    try {
        parentPort?.postMessage(Object.freeze({
            kind: "trapped",
            diagnostic: Diagnostic(Error),
        }));
    } finally {
        process.exitCode = 1;
    }
});
