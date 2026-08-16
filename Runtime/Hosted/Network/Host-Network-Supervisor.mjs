import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";
import {
    Decodeˉhostˉnetworkˉresponse,
    Encodeˉhostˉnetworkˉrequest,
    HOST_NETWORK_MAX_RESPONSE_BYTES,
    HOST_NETWORK_RESPONSE_HEADER_BYTES,
    Hostˉnetworkˉoperation,
    Readˉframedˉrecordˉlength,
} from "./Host-Network-Protocol.mjs";

const PROVIDER_PATH = fileURLToPath(new URL("./Host-Network-Provider.mjs", import.meta.url));

function Decimal(Value, Maximum, Description) {
    if (!Number.isSafeInteger(Value) || Value < 1 || Value > Maximum) {
        throw new Error(`${Description} is invalid.`);
    }
    return String(Value);
}

function U64(Value, Description) {
    if (typeof Value !== "bigint" || Value < 1n || Value > 0xffff_ffff_ffff_ffffn) {
        throw new Error(`${Description} is invalid.`);
    }
    return String(Value);
}

export class Hostˉnetworkˉsupervisor {
    constructor({
        service,
        port,
        generation,
        maximumConnections = 1,
        maximumQueuedBytes = 65_536,
        maximumTransferBytes = 16_777_216n,
        maximumOperationMilliseconds = 30_000,
        maximumLifetimeMilliseconds = 3_600_000,
        providerPath = PROVIDER_PATH,
        providerArguments = [],
    }) {
        if (typeof providerPath !== "string" || providerPath.length < 1 ||
            !Array.isArray(providerArguments) ||
            providerArguments.some(Value => typeof Value !== "string")) {
            throw new Error("Host-network provider launch is invalid.");
        }
        this.generation = generation;
        this.maximumOperationMilliseconds = maximumOperationMilliseconds;
        this.nextRequestId = 1n;
        this.pending = new Map();
        this.responseBytes = Buffer.alloc(0);
        this.closed = false;
        this.stderrBytes = 0;
        const Arguments = [
            providerPath,
            "--service", service,
            "--port", Decimal(port, 65_535, "Port authority"),
            "--generation", U64(generation, "Provider generation"),
            "--max-connections", Decimal(maximumConnections, 64, "Connection limit"),
            "--max-queued-bytes", Decimal(maximumQueuedBytes, 65_536, "Queued-byte limit"),
            "--max-transfer-bytes", U64(maximumTransferBytes, "Transfer limit"),
            "--max-operation-ms", Decimal(maximumOperationMilliseconds, 300_000, "Operation span"),
            "--max-lifetime-ms", Decimal(maximumLifetimeMilliseconds, 86_400_000, "Provider lifetime"),
            ...providerArguments,
        ];
        this.child = spawn(process.execPath, Arguments, {
            stdio: ["pipe", "pipe", "pipe"],
            windowsHide: true,
            env: {},
        });
        this.child.stdout.on("data", Chunk => this.#Receive(Chunk));
        this.child.stderr.on("data", Chunk => {
            this.stderrBytes += Chunk.length;
            if (this.stderrBytes > 4_096) this.child.kill();
        });
        this.child.on("error", () => this.#Lose());
        this.child.on("exit", () => this.#Lose());
    }

    #Lose() {
        if (this.closed) return;
        this.closed = true;
        for (const Pending of this.pending.values()) {
            clearTimeout(Pending.timer);
            Pending.reject(new Error("Host-network provider exited."));
        }
        this.pending.clear();
    }

    #Receive(Chunk) {
        if (this.closed) return;
        this.responseBytes = Buffer.concat([this.responseBytes, Chunk]);
        if (this.responseBytes.length > HOST_NETWORK_MAX_RESPONSE_BYTES * 2) {
            this.child.kill();
            return;
        }
        while (this.responseBytes.length >= 12) {
            let Total;
            try {
                Total = Readˉframedˉrecordˉlength(
                    this.responseBytes,
                    "WVNS",
                    HOST_NETWORK_RESPONSE_HEADER_BYTES,
                    HOST_NETWORK_MAX_RESPONSE_BYTES,
                );
            } catch {
                this.child.kill();
                return;
            }
            if (this.responseBytes.length < Total) break;
            const Bytes = this.responseBytes.subarray(0, Total);
            this.responseBytes = this.responseBytes.subarray(Total);
            let Response;
            try { Response = Decodeˉhostˉnetworkˉresponse(Bytes); } catch {
                this.child.kill();
                return;
            }
            const Pending = this.pending.get(Response.requestId.toString());
            if (!Pending || Response.providerGeneration !== this.generation) {
                this.child.kill();
                return;
            }
            this.pending.delete(Response.requestId.toString());
            clearTimeout(Pending.timer);
            Pending.resolve(Response);
        }
    }

    #Request(Operation, {
        connectionId = 0n,
        connectionGeneration = 0n,
        control = 0,
        service = "",
        payload = Buffer.alloc(0),
        timeoutMilliseconds = this.maximumOperationMilliseconds,
    } = {}) {
        if (this.closed) return Promise.reject(new Error("Host-network supervisor is closed."));
        const Timeout = Number(Decimal(
            timeoutMilliseconds, this.maximumOperationMilliseconds, "Request timeout",
        ));
        const RequestId = this.nextRequestId;
        this.nextRequestId += 1n;
        const Bytes = Encodeˉhostˉnetworkˉrequest({
            operation: Operation,
            requestId: RequestId,
            providerGeneration: this.generation,
            connectionId,
            connectionGeneration,
            deadlineNanoseconds: process.hrtime.bigint() + BigInt(Timeout) * 1_000_000n,
            control,
            service,
            payload,
        });
        return new Promise((Resolve, Reject) => {
            const Timer = setTimeout(() => {
                this.pending.delete(RequestId.toString());
                this.child.kill();
                Reject(new Error("Host-network supervisor response deadline expired."));
            }, Timeout + 1_000);
            this.pending.set(RequestId.toString(), { resolve: Resolve, reject: Reject, timer: Timer });
            const Completeˉwrite = Error => {
                Bytes.fill(0);
                if (!Error) return;
                const Pending = this.pending.get(RequestId.toString());
                if (!Pending) return;
                this.pending.delete(RequestId.toString());
                clearTimeout(Timer);
                Reject(new Error("Host-network provider input failed."));
            };
            try { this.child.stdin.write(Bytes, Completeˉwrite); } catch (Error) {
                Completeˉwrite(Error);
            }
        });
    }

    connect(Service, Port, TimeoutMilliseconds) {
        return this.#Request(Hostˉnetworkˉoperation.Connect, {
            service: Service, control: Port, timeoutMilliseconds: TimeoutMilliseconds,
        });
    }

    write(ConnectionId, ConnectionGeneration, Payload, TimeoutMilliseconds) {
        return this.#Request(Hostˉnetworkˉoperation.Write, {
            connectionId: ConnectionId,
            connectionGeneration: ConnectionGeneration,
            payload: Payload,
            timeoutMilliseconds: TimeoutMilliseconds,
        });
    }

    read(ConnectionId, ConnectionGeneration, MaximumBytes, TimeoutMilliseconds) {
        return this.#Request(Hostˉnetworkˉoperation.Read, {
            connectionId: ConnectionId,
            connectionGeneration: ConnectionGeneration,
            control: MaximumBytes,
            timeoutMilliseconds: TimeoutMilliseconds,
        });
    }

    shutdownWrite(ConnectionId, ConnectionGeneration, TimeoutMilliseconds) {
        return this.#Request(Hostˉnetworkˉoperation.ShutdownWrite, {
            connectionId: ConnectionId,
            connectionGeneration: ConnectionGeneration,
            timeoutMilliseconds: TimeoutMilliseconds,
        });
    }

    closeConnection(ConnectionId, ConnectionGeneration, TimeoutMilliseconds) {
        return this.#Request(Hostˉnetworkˉoperation.Close, {
            connectionId: ConnectionId,
            connectionGeneration: ConnectionGeneration,
            timeoutMilliseconds: TimeoutMilliseconds,
        });
    }

    async teardown() {
        if (this.closed) return;
        this.child.stdin.end();
        await new Promise(Resolve => {
            const Timer = setTimeout(() => {
                this.child.kill();
                Resolve();
            }, 2_000);
            this.child.once("exit", () => {
                clearTimeout(Timer);
                Resolve();
            });
        });
        this.closed = true;
    }
}
