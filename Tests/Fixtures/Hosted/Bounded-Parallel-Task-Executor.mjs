const RENDEZVOUS_TIMEOUT_MILLISECONDS = 5_000;

function U32(Bytes, Offset) {
    return (Bytes[Offset] |
        (Bytes[Offset + 1] << 8) |
        (Bytes[Offset + 2] << 16) |
        (Bytes[Offset + 3] << 24)) >>> 0;
}

function Result(Kind, Value, Workˉunits, Firstˉevidence = 0n,
    Secondˉevidence = 0n) {
    return Object.freeze({
        kind: Kind,
        value: Value,
        workUnits: Workˉunits,
        firstEvidence: Firstˉevidence,
        secondEvidence: Secondˉevidence,
    });
}

export async function Executeˉboundedˉhostedˉtask({
    maximumWorkUnits,
    work,
    cancellation,
    coordination,
}) {
    if (work.byteLength !== 12) throw new Error("Hosted task work is malformed.");
    const Mode = U32(work, 0);
    const Value = U32(work, 4);
    const Requested = BigInt(U32(work, 8));
    if (Requested < 1n || Requested > maximumWorkUnits) {
        return Result(6, new Uint8Array(), 0n, 3011n);
    }
    let Checksum = 0x811c9dc5;
    let Unit = 0n;
    while (Unit < Requested) {
        if ((Unit & 1023n) === 0n && Atomics.load(cancellation, 0) !== 0) {
            return Result(2, new Uint8Array(), Unit);
        }
        Checksum ^= Number(Unit & 0xffff_ffffn) ^ Value;
        Checksum = Math.imul(Checksum, 0x01000193) >>> 0;
        Unit += 1n;
    }
    if (Mode === 1) return Result(1, Uint8Array.of(Value), Unit);
    if (Mode === 2) return Result(6, new Uint8Array(), Unit, 3007n);
    if (Mode === 3) {
        const Deadline = Date.now() + RENDEZVOUS_TIMEOUT_MILLISECONDS;
        while (Atomics.load(cancellation, 0) === 0) {
            if (Date.now() >= Deadline) {
                throw new Error("Cancellation observation expired.");
            }
            Atomics.wait(cancellation, 0, 0, 50);
        }
        return Result(2, new Uint8Array(), Unit);
    }
    if (Mode === 4) {
        while (true) Atomics.wait(cancellation, 0, 0, 1_000);
    }
    if (Mode === 5) return Object.freeze({ kind: 9 });
    if (Mode !== 0 || coordination === null) {
        throw new Error("Hosted task mode is invalid.");
    }
    const Prior = Atomics.add(coordination.state, 0, 1);
    Atomics.notify(coordination.state, 0, coordination.participants);
    const Deadline = Date.now() + RENDEZVOUS_TIMEOUT_MILLISECONDS;
    while (Prior + 1 < coordination.participants &&
        Atomics.load(coordination.state, 0) < coordination.participants) {
        if (Date.now() >= Deadline) throw new Error("Task rendezvous expired.");
        Atomics.wait(coordination.state, 0,
            Atomics.load(coordination.state, 0), 50);
    }
    Atomics.add(coordination.state, 1, 1);
    Atomics.notify(coordination.state, 1, coordination.participants);
    const Valueˉbytes = Buffer.alloc(8);
    Valueˉbytes.writeUInt32LE(Value, 0);
    Valueˉbytes.writeUInt32LE(Checksum, 4);
    return Result(0, Valueˉbytes, Unit);
}
