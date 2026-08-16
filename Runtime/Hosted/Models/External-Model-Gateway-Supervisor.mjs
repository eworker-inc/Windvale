import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";
import {
    Decodeˉmodelˉgatewayˉready,
    Encodeˉmodelˉgatewayˉinitialization,
    MODEL_GATEWAY_MAX_REQUEST_BYTES,
    MODEL_GATEWAY_MAX_RESPONSE_BYTES,
    MODEL_GATEWAY_READY_BYTES,
    Readˉmodelˉgatewayˉframeˉlength,
} from "./External-Model-Gateway-Protocol.mjs";

const GATEWAY_PATH = fileURLToPath(
    new URL("./External-Model-Gateway-Process.mjs", import.meta.url),
);

export class Externalˉmodelˉgatewayˉsupervisor {
    constructor({
        wrapper,
        passphrase,
        providerGeneration,
        trustGeneration,
        maximumRequestBytes = 65_536,
        maximumHeaderBytes = 16_384,
        maximumBodyBytes = 1_048_576,
        maximumWireBytes = 1_081_344,
        maximumOperationMilliseconds = 30_000,
        maximumLifetimeMilliseconds = 60_000,
        gatewayPath = GATEWAY_PATH,
    }) {
        if (typeof gatewayPath !== "string" || gatewayPath.length === 0) {
            throw new Error("External-model gateway launch path is invalid.");
        }
        const Initialization = Encodeˉmodelˉgatewayˉinitialization({
            wrapper, passphrase, providerGeneration, trustGeneration,
            maximumRequestBytes, maximumHeaderBytes, maximumBodyBytes, maximumWireBytes,
            maximumOperationMilliseconds, maximumLifetimeMilliseconds,
        });
        this.maximumRequestBytes = maximumRequestBytes;
        this.maximumOperationMilliseconds = maximumOperationMilliseconds;
        this.responseBytes = Buffer.alloc(0);
        this.readyRecord = null;
        this.pending = null;
        this.closed = false;
        this.stderrBytes = 0;
        this.child = spawn(process.execPath, [gatewayPath], {
            stdio: ["pipe", "pipe", "pipe"], windowsHide: true, env: {},
        });
        this.readyPromise = new Promise((Resolve, Reject) => {
            const Timer = setTimeout(() => {
                this.child.kill();
                Reject(new Error("External-model gateway startup deadline expired."));
            }, maximumOperationMilliseconds + 2_000);
            this.readyWaiter = {
                resolve: Value => { clearTimeout(Timer); Resolve(Value); },
                reject: Error => { clearTimeout(Timer); Reject(Error); },
            };
        });
        this.readyPromise.catch(() => {});
        this.child.stdout.on("data", Chunk => this.#Receive(Chunk));
        this.child.stderr.on("data", Chunk => {
            this.stderrBytes += Chunk.length;
            if (this.stderrBytes > 4_096) this.child.kill();
        });
        this.child.on("error", () => this.#Lose());
        this.child.on("exit", () => this.#Lose());
        const Complete = Error => {
            Initialization.fill(0);
            if (Error) this.#Lose();
        };
        try { this.child.stdin.write(Initialization, Complete); } catch (Error) { Complete(Error); }
    }

    #Lose() {
        if (this.closed) return;
        this.closed = true;
        const Failure = new Error("External-model gateway exited.");
        this.readyWaiter?.reject(Failure);
        this.readyWaiter = null;
        if (this.pending) {
            clearTimeout(this.pending.timer);
            this.pending.reject(Failure);
            this.pending = null;
        }
        this.responseBytes.fill(0);
        this.responseBytes = Buffer.alloc(0);
    }

    #Receive(Chunk) {
        if (this.closed) return;
        const Combined = Buffer.concat([this.responseBytes, Chunk]);
        this.responseBytes.fill(0);
        Chunk.fill(0);
        this.responseBytes = Combined;
        if (this.readyRecord === null) {
            if (this.responseBytes.length < MODEL_GATEWAY_READY_BYTES) return;
            if (this.responseBytes.length !== MODEL_GATEWAY_READY_BYTES) {
                this.child.kill();
                return;
            }
            let Ready;
            try { Ready = Decodeˉmodelˉgatewayˉready(this.responseBytes); } catch {
                this.child.kill();
                return;
            }
            this.responseBytes.fill(0);
            this.responseBytes = Buffer.alloc(0);
            if (Ready.status !== "ready") {
                this.readyWaiter?.reject(new Error("External-model gateway startup failed."));
                this.readyWaiter = null;
                this.child.kill();
                return;
            }
            this.readyRecord = Ready;
            this.readyWaiter?.resolve(Ready);
            this.readyWaiter = null;
            return;
        }
        if (this.responseBytes.length > MODEL_GATEWAY_MAX_RESPONSE_BYTES) {
            this.child.kill();
            return;
        }
        if (!this.pending || this.responseBytes.length < 12) return;
        let Total;
        try {
            const Magic = this.responseBytes.subarray(0, 4).toString("ascii");
            if (Magic !== "WVMC" && Magic !== "WVMG") throw new Error("Unexpected response.");
            Total = Readˉmodelˉgatewayˉframeˉlength(
                this.responseBytes, Magic, Magic === "WVMC" ? 48 : 64,
                MODEL_GATEWAY_MAX_RESPONSE_BYTES,
            );
        } catch {
            this.child.kill();
            return;
        }
        if (this.responseBytes.length < Total) return;
        if (this.responseBytes.length !== Total ||
            this.responseBytes.readBigUInt64LE(16) !== this.pending.requestId) {
            this.child.kill();
            return;
        }
        const Response = Buffer.from(this.responseBytes);
        this.responseBytes.fill(0);
        this.responseBytes = Buffer.alloc(0);
        const Pending = this.pending;
        this.pending = null;
        clearTimeout(Pending.timer);
        Pending.resolve(Response);
    }

    ready() { return this.readyPromise; }

    async request(Value, TimeoutMilliseconds = this.maximumOperationMilliseconds) {
        await this.readyPromise;
        if (this.closed) throw new Error("External-model gateway supervisor is closed.");
        if (this.pending) throw new Error("External-model gateway already has an active request.");
        const Bytes = Buffer.from(Value);
        if (Bytes.length < 48 || Bytes.length > this.maximumRequestBytes ||
            Bytes.length > MODEL_GATEWAY_MAX_REQUEST_BYTES ||
            Bytes.subarray(0, 4).toString("ascii") !== "WVMQ" ||
            Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length ||
            !Number.isSafeInteger(TimeoutMilliseconds) || TimeoutMilliseconds < 1 ||
            TimeoutMilliseconds > this.maximumOperationMilliseconds) {
            Bytes.fill(0);
            throw new Error("External-model gateway request is invalid.");
        }
        const RequestId = Bytes.readBigUInt64LE(16);
        return new Promise((Resolve, Reject) => {
            const Timer = setTimeout(() => {
                this.pending = null;
                this.child.kill();
                Reject(new Error("External-model gateway response deadline expired."));
            }, TimeoutMilliseconds + 1_000);
            this.pending = { requestId: RequestId, resolve: Resolve, reject: Reject, timer: Timer };
            const Complete = Error => {
                Bytes.fill(0);
                if (!Error) return;
                if (this.pending?.requestId !== RequestId) return;
                clearTimeout(this.pending.timer);
                this.pending = null;
                Reject(new Error("External-model gateway input failed."));
            };
            try { this.child.stdin.write(Bytes, Complete); } catch (Error) { Complete(Error); }
        });
    }

    async teardown() {
        if (this.closed) return;
        this.child.stdin.end();
        await new Promise(Resolve => {
            const Timer = setTimeout(() => { this.child.kill(); Resolve(); }, 2_000);
            this.child.once("exit", () => { clearTimeout(Timer); Resolve(); });
        });
        this.closed = true;
        this.responseBytes.fill(0);
        this.responseBytes = Buffer.alloc(0);
    }
}
