import process from "node:process";
import { Hostˉnetworkˉprovider } from "./Host-Network-Provider-Core.mjs";
import {
    Decodeˉhostˉnetworkˉrequest,
    HOST_NETWORK_MAX_REQUEST_BYTES,
    HOST_NETWORK_REQUEST_HEADER_BYTES,
    Readˉframedˉrecordˉlength,
} from "./Host-Network-Protocol.mjs";

function Usage() {
    return "Usage: node Runtime/Hosted/Network/Host-Network-Provider.mjs " +
        "--service <canonical-name> --port <1-65535> --generation <nonzero-u64> " +
        "[--max-connections <1-64>] [--max-queued-bytes <1-65536>] " +
        "[--max-transfer-bytes <nonzero-u64>] [--max-operation-ms <1-300000>] " +
        "[--max-lifetime-ms <1-86400000>]";
}

function Parseˉinteger(Value, Maximum) {
    if (!/^[1-9][0-9]*$/.test(Value ?? "")) throw new Error(Usage());
    const NumberValue = Number(Value);
    if (!Number.isSafeInteger(NumberValue) || NumberValue > Maximum) throw new Error(Usage());
    return NumberValue;
}

function Parseˉu64(Value) {
    if (!/^[1-9][0-9]*$/.test(Value ?? "")) throw new Error(Usage());
    const Result = BigInt(Value);
    if (Result > 0xffff_ffff_ffff_ffffn) throw new Error(Usage());
    return Result;
}

function Parseˉarguments(Arguments) {
    const Values = new Map();
    for (let Index = 0; Index < Arguments.length; Index += 2) {
        const Name = Arguments[Index];
        const Value = Arguments[Index + 1];
        if (!Name?.startsWith("--") || Value === undefined || Values.has(Name)) throw new Error(Usage());
        Values.set(Name, Value);
    }
    const Allowed = new Set([
        "--service", "--port", "--generation", "--max-connections", "--max-queued-bytes",
        "--max-transfer-bytes", "--max-operation-ms", "--max-lifetime-ms",
    ]);
    if ([...Values.keys()].some(Name => !Allowed.has(Name)) ||
        !Values.has("--service") || !Values.has("--port") || !Values.has("--generation")) {
        throw new Error(Usage());
    }
    return {
        service: Values.get("--service"),
        port: Parseˉinteger(Values.get("--port"), 65_535),
        generation: Parseˉu64(Values.get("--generation")),
        maximumConnections: Parseˉinteger(Values.get("--max-connections") ?? "1", 64),
        maximumQueuedBytes: Parseˉinteger(Values.get("--max-queued-bytes") ?? "65536", 65_536),
        maximumTransferBytes: Parseˉu64(Values.get("--max-transfer-bytes") ?? "16777216"),
        maximumOperationMilliseconds: Parseˉinteger(
            Values.get("--max-operation-ms") ?? "30000", 300_000,
        ),
        maximumLifetimeMilliseconds: Parseˉinteger(
            Values.get("--max-lifetime-ms") ?? "3600000", 86_400_000,
        ),
    };
}

const Provider = new Hostˉnetworkˉprovider(Parseˉarguments(process.argv.slice(2)));
let Pending = Buffer.alloc(0);
let Publishing = Promise.resolve();
const Active = new Set();

function Publish(Bytes) {
    Publishing = Publishing.then(() => new Promise((Resolve, Reject) => {
        if (process.stdout.write(Bytes)) Resolve();
        else {
            process.stdout.once("drain", Resolve);
            process.stdout.once("error", Reject);
        }
    }));
    Publishing.catch(() => {
        Provider.teardown();
        process.exitCode = 74;
    });
}

function Dispatch(Bytes) {
    let Request;
    try { Request = Decodeˉhostˉnetworkˉrequest(Bytes); } catch {
        Provider.teardown();
        process.exitCode = 65;
        process.stdin.destroy();
        return;
    }
    const Key = Request.requestId.toString();
    if (Active.has(Key)) {
        Provider.teardown();
        process.exitCode = 65;
        process.stdin.destroy();
        return;
    }
    Active.add(Key);
    Provider.handle(Bytes).then(Publish, () => {
        Provider.teardown();
        process.exitCode = 70;
    }).finally(() => Active.delete(Key));
}

process.stdin.on("data", Chunk => {
    if (process.exitCode) return;
    Pending = Buffer.concat([Pending, Chunk]);
    if (Pending.length > HOST_NETWORK_MAX_REQUEST_BYTES * 2) {
        Provider.teardown();
        process.exitCode = 65;
        process.stdin.destroy();
        return;
    }
    while (Pending.length >= 12) {
        let Total;
        try {
            Total = Readˉframedˉrecordˉlength(
                Pending, "WVNR", HOST_NETWORK_REQUEST_HEADER_BYTES, HOST_NETWORK_MAX_REQUEST_BYTES,
            );
        } catch {
            Provider.teardown();
            process.exitCode = 65;
            process.stdin.destroy();
            return;
        }
        if (Pending.length < Total) break;
        const Request = Pending.subarray(0, Total);
        Pending = Pending.subarray(Total);
        Dispatch(Request);
    }
});

process.stdin.on("end", async () => {
    if (Pending.length !== 0 && !process.exitCode) process.exitCode = 65;
    while (Active.size !== 0) await new Promise(Resolve => setTimeout(Resolve, 1));
    await Publishing.catch(() => {});
    Provider.teardown();
});

process.stdin.on("error", () => {
    Provider.teardown();
    process.exitCode = 74;
});

for (const Signal of ["SIGINT", "SIGTERM"]) {
    process.on(Signal, () => {
        Provider.teardown();
        process.exit(0);
    });
}
