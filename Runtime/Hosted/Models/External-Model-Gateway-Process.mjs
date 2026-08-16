import process from "node:process";
import { Unlockˉprotectedˉcredential } from "../Credentials/Protected-Credential.mjs";
import { Protectedˉexternalˉmodelˉgateway } from "./External-Model-Gateway-Core.mjs";
import {
    Decodeˉmodelˉgatewayˉinitialization,
    Encodeˉmodelˉgatewayˉready,
    MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES,
    MODEL_GATEWAY_MAX_INITIALIZATION_BYTES,
    MODEL_GATEWAY_MAX_REQUEST_BYTES,
    Readˉmodelˉgatewayˉframeˉlength,
} from "./External-Model-Gateway-Protocol.mjs";

let Pending = Buffer.alloc(0);
let Gateway = null;
let Lease = null;
let MaximumRequestBytes = MODEL_GATEWAY_MAX_REQUEST_BYTES;
let Busy = false;
let Closing = false;
let Failed = false;

function Destroy() {
    Pending.fill(0);
    Pending = Buffer.alloc(0);
    Lease?.destroy();
    Lease = null;
    Gateway = null;
}

function Publish(Bytes) {
    return new Promise((Resolve, Reject) => {
        const Complete = Error => {
            Bytes.fill(0);
            Error ? Reject(Error) : Resolve();
        };
        try { process.stdout.write(Bytes, Complete); } catch (Error) { Complete(Error); }
    });
}

async function Failˉstartup() {
    if (Failed) return;
    Failed = true;
    process.stdin.pause();
    await Publish(Encodeˉmodelˉgatewayˉready({ status: 1 })).catch(() => {});
    Destroy();
    process.exitCode = 65;
    process.stdin.destroy();
}

function Failˉactive() {
    if (Failed) return;
    Failed = true;
    process.stdin.pause();
    Destroy();
    process.exitCode = 65;
    process.stdin.destroy();
}

function Take(Total) {
    const Bytes = Buffer.from(Pending.subarray(0, Total));
    const Remainder = Buffer.from(Pending.subarray(Total));
    Pending.fill(0);
    Pending = Remainder;
    return Bytes;
}

async function Initialize(Bytes) {
    let Values;
    try {
        Values = Decodeˉmodelˉgatewayˉinitialization(Bytes);
        Lease = await Unlockˉprotectedˉcredential(Values.wrapper, Values.passphrase);
        const Metadata = Lease.inspect();
        Gateway = new Protectedˉexternalˉmodelˉgateway({
            credentialLease: Lease,
            providerGeneration: Values.providerGeneration,
            trustGeneration: Values.trustGeneration,
            maximumRequestBytes: Values.maximumRequestBytes,
            maximumHeaderBytes: Values.maximumHeaderBytes,
            maximumBodyBytes: Values.maximumBodyBytes,
            maximumWireBytes: Values.maximumWireBytes,
            maximumOperationMilliseconds: Values.maximumOperationMilliseconds,
            maximumLifetimeMilliseconds: Values.maximumLifetimeMilliseconds,
        });
        MaximumRequestBytes = Values.maximumRequestBytes;
        await Publish(Encodeˉmodelˉgatewayˉready({
            provider: Metadata.provider,
            providerGeneration: Values.providerGeneration,
            credentialGeneration: Metadata.generation,
            identity: Metadata.identity,
        }));
    } catch {
        await Failˉstartup();
    } finally {
        Bytes.fill(0);
        Values?.wrapper.fill(0);
        Values?.passphrase.fill(0);
    }
}

async function Dispatch(Bytes) {
    try {
        const Response = await Gateway.execute(Bytes);
        await Publish(Response);
    } catch {
        Failˉactive();
    } finally {
        Bytes.fill(0);
    }
}

async function Pump() {
    if (Busy || Failed) return;
    if (Gateway === null) {
        let Total;
        try {
            Total = Readˉmodelˉgatewayˉframeˉlength(
                Pending, "WVGI", MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES,
                MODEL_GATEWAY_MAX_INITIALIZATION_BYTES,
            );
        } catch {
            await Failˉstartup();
            return;
        }
        if (Total === null || Pending.length < Total) return;
        if (Pending.length !== Total) {
            await Failˉstartup();
            return;
        }
        Busy = true;
        process.stdin.pause();
        await Initialize(Take(Total));
        Busy = false;
        if (!Failed) process.stdin.resume();
        if (Closing) Finish();
        return;
    }
    let Total;
    try {
        Total = Readˉmodelˉgatewayˉframeˉlength(Pending, "WVMQ", 48, MaximumRequestBytes);
    } catch {
        Failˉactive();
        return;
    }
    if (Total === null || Pending.length < Total) return;
    if (Pending.length !== Total) {
        Failˉactive();
        return;
    }
    Busy = true;
    process.stdin.pause();
    await Dispatch(Take(Total));
    Busy = false;
    if (!Failed) process.stdin.resume();
    if (Closing) Finish();
}

function Finish() {
    if (Busy) return;
    if (Pending.length !== 0 && !Failed) process.exitCode = 65;
    Destroy();
}

process.stdin.on("data", Chunk => {
    if (Failed) return;
    const Combined = Buffer.concat([Pending, Chunk]);
    Pending.fill(0);
    Chunk.fill(0);
    Pending = Combined;
    const Maximum = Gateway === null ? MODEL_GATEWAY_MAX_INITIALIZATION_BYTES : MaximumRequestBytes;
    if (Pending.length > Maximum) {
        Gateway === null ? Failˉstartup() : Failˉactive();
        return;
    }
    Pump();
});

process.stdin.on("end", () => {
    Closing = true;
    Finish();
});

process.stdin.on("error", () => {
    Failed = true;
    process.exitCode = 74;
    Destroy();
});

for (const Signal of ["SIGINT", "SIGTERM"]) {
    process.on(Signal, () => {
        Destroy();
        process.exit(0);
    });
}
