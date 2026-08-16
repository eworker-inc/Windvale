import { spawn } from "node:child_process";
import { isAbsolute } from "node:path";
import {
    Decodeˉgatewayˉmodelˉrequest,
    Encodeˉgatewayˉcatalogˉresponse,
    Encodeˉgatewayˉgenerationˉresponse,
    Modelˉgatewayˉstatus,
} from "./External-Model-Gateway-Core.mjs";
import {
    Externalˉmodelˉgatewayˉsupervisor,
} from "./External-Model-Gateway-Supervisor.mjs";
import {
    MODEL_GATEWAY_MAX_REQUEST_BYTES,
    Readˉmodelˉgatewayˉframeˉlength,
} from "./External-Model-Gateway-Protocol.mjs";

const MAX_DIAGNOSTIC_BYTES = 4_096;

function Failureˉresponse(Request, Error) {
    const Diagnostic = Error?.message === "External-model gateway response deadline expired."
        ? "Model gateway deadline expired."
        : "Model gateway peer exited.";
    if (Request.operation === "generate") {
        return Encodeˉgatewayˉgenerationˉresponse({
            status: Modelˉgatewayˉstatus.SubmissionIndeterminate,
            requestId: Request.requestId,
            diagnostic: "Submission completion is indeterminate.",
        });
    }
    return Encodeˉgatewayˉcatalogˉresponse({
        status: Modelˉgatewayˉstatus.Unavailable,
        requestId: Request.requestId,
        diagnostic: Diagnostic,
    });
}

export class Nativeˉexternalˉmodelˉgatewayˉsupervisor {
    constructor({
        applicationPath,
        applicationArguments = [],
        maximumOperationMilliseconds = 30_000,
        maximumLifetimeMilliseconds = 60_000,
        gatewayFactory = Values => new Externalˉmodelˉgatewayˉsupervisor(Values),
        ...gatewayValues
    }) {
        if (typeof applicationPath !== "string" || !isAbsolute(applicationPath) ||
            !Array.isArray(applicationArguments) ||
            applicationArguments.some(Value => typeof Value !== "string") ||
            !Number.isSafeInteger(maximumOperationMilliseconds) ||
            maximumOperationMilliseconds < 1 || maximumOperationMilliseconds > 300_000 ||
            !Number.isSafeInteger(maximumLifetimeMilliseconds) ||
            maximumLifetimeMilliseconds < maximumOperationMilliseconds ||
            maximumLifetimeMilliseconds > 300_000 || typeof gatewayFactory !== "function") {
            throw new Error("Native model-gateway launch configuration is invalid.");
        }
        this.applicationPath = applicationPath;
        this.applicationArguments = [...applicationArguments];
        this.applicationEnvironment = Object.freeze({});
        this.maximumOperationMilliseconds = maximumOperationMilliseconds;
        this.maximumLifetimeMilliseconds = maximumLifetimeMilliseconds;
        this.gateway = gatewayFactory({
            ...gatewayValues, maximumOperationMilliseconds, maximumLifetimeMilliseconds,
        });
        this.application = null;
        this.inputBytes = Buffer.alloc(0);
        this.pending = false;
        this.closed = false;
        this.stderrBytes = 0;
        this.lifetimeTimer = null;
    }

    async run() {
        if (this.application !== null || this.closed) {
            throw new Error("Native model-gateway supervisor cannot be run again.");
        }
        let Ready;
        try { Ready = await this.gateway.ready(); } catch (Error) {
            await this.teardown();
            throw Error;
        }
        if (this.closed) throw new Error("Native model-gateway supervisor is closed.");
        const Child = spawn(this.applicationPath, this.applicationArguments, {
            stdio: ["pipe", "pipe", "pipe"], windowsHide: true,
            env: this.applicationEnvironment,
        });
        this.application = Child;
        this.lifetimeTimer = setTimeout(() => this.#Terminate(), this.maximumLifetimeMilliseconds);
        Child.stdout.on("data", Chunk => this.#Receive(Chunk));
        Child.stderr.on("data", Chunk => {
            this.stderrBytes += Chunk.length;
            if (this.stderrBytes > MAX_DIAGNOSTIC_BYTES) this.#Terminate();
        });
        try {
            const Result = await new Promise((Resolve, Reject) => {
                Child.once("error", Error => Reject(Error));
                Child.once("exit", (Code, Signal) => Resolve({ code: Code, signal: Signal }));
            });
            return Object.freeze({ ...Result, ready: Ready, diagnosticBytes: this.stderrBytes });
        } finally {
            await this.teardown();
        }
    }

    #Terminate() {
        if (this.closed) return;
        this.application?.kill();
        this.gateway.teardown().catch(() => {});
    }

    #Receive(Chunk) {
        if (this.closed) {
            Chunk.fill(0);
            return;
        }
        const Combined = Buffer.concat([this.inputBytes, Chunk]);
        this.inputBytes.fill(0);
        Chunk.fill(0);
        this.inputBytes = Combined;
        if (this.pending) {
            this.#Terminate();
            return;
        }
        let Total;
        try {
            Total = Readˉmodelˉgatewayˉframeˉlength(
                this.inputBytes, "WVMQ", 48, MODEL_GATEWAY_MAX_REQUEST_BYTES,
            );
        } catch {
            this.#Terminate();
            return;
        }
        if (Total === null || this.inputBytes.length < Total) return;
        if (this.inputBytes.length !== Total) {
            this.#Terminate();
            return;
        }
        const RequestBytes = Buffer.from(this.inputBytes);
        this.inputBytes.fill(0);
        this.inputBytes = Buffer.alloc(0);
        let Request;
        try { Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes); } catch {
            RequestBytes.fill(0);
            this.#Terminate();
            return;
        }
        this.pending = true;
        this.gateway.request(RequestBytes, this.maximumOperationMilliseconds)
            .catch(Error => Failureˉresponse(Request, Error))
            .then(Response => this.#Reply(Response))
            .catch(() => this.#Terminate())
            .finally(() => RequestBytes.fill(0));
    }

    #Reply(Value) {
        if (this.closed || !this.pending || !this.application?.stdin.writable) {
            if (Buffer.isBuffer(Value)) Value.fill(0);
            this.#Terminate();
            return;
        }
        const Bytes = Buffer.isBuffer(Value) ? Value : Buffer.from(Value);
        this.pending = false;
        const Complete = Error => {
            Bytes.fill(0);
            if (Error) this.#Terminate();
        };
        try { this.application.stdin.write(Bytes, Complete); } catch (Error) { Complete(Error); }
    }

    async teardown() {
        if (this.closed) return;
        this.closed = true;
        clearTimeout(this.lifetimeTimer);
        this.inputBytes.fill(0);
        this.inputBytes = Buffer.alloc(0);
        if (this.application && this.application.exitCode === null) this.application.kill();
        this.application?.stdin.destroy();
        await this.gateway.teardown();
    }
}
