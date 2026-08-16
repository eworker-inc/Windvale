import { Hostˉnetworkˉstatus } from "../Network/Host-Network-Protocol.mjs";
import { Hostˉtlsˉsupervisor } from "../Network/Host-Tls-Supervisor.mjs";
import {
    Boundedˉhttpˉfailure,
    Boundedˉhttp1ˉresponseˉdecoder,
    Buildˉboundedˉhttp1ˉrequest,
} from "./Bounded-Http1.mjs";

function Positiveˉinteger(Value, Maximum, Description) {
    if (!Number.isSafeInteger(Value) || Value < 1 || Value > Maximum) {
        throw new Boundedˉhttpˉfailure("invalid_request", `${Description} is invalid.`);
    }
    return Value;
}

function Remainingˉmilliseconds(Deadline) {
    const Remaining = Deadline - process.hrtime.bigint();
    if (Remaining <= 0n) throw new Boundedˉhttpˉfailure("deadline", "HTTPS deadline expired.");
    const Rounded = Number((Remaining + 999_999n) / 1_000_000n);
    return Math.max(1, Rounded);
}

function Transportˉfailure(Response, Mutation = false) {
    if (Response.status === Hostˉnetworkˉstatus.SubmissionIndeterminate || Mutation) {
        throw new Boundedˉhttpˉfailure(
            "submission_indeterminate", "HTTPS request submission is indeterminate.",
        );
    }
    if (Response.status === Hostˉnetworkˉstatus.Expired) {
        throw new Boundedˉhttpˉfailure("deadline", "HTTPS operation expired.");
    }
    if (Response.status === Hostˉnetworkˉstatus.Unauthorized) {
        throw new Boundedˉhttpˉfailure("denied", "HTTPS authority denied the operation.");
    }
    if (Response.status === Hostˉnetworkˉstatus.Stale) {
        throw new Boundedˉhttpˉfailure("stale", "HTTPS provider generation is stale.");
    }
    if (Response.status === Hostˉnetworkˉstatus.ProviderLost) {
        throw new Boundedˉhttpˉfailure("provider_lost", "HTTPS provider was lost.");
    }
    throw new Boundedˉhttpˉfailure("transport", "HTTPS transport failed.");
}

export class Boundedˉhttpsˉclient {
    constructor({
        service,
        port = 443,
        generation,
        trustGeneration,
        trustCertificates,
        allowedTargets,
        allowedHeaders = ["accept", "content-type"],
        maximumRequestBytes = 65_536,
        maximumHeaderBytes = 16_384,
        maximumBodyBytes = 1_048_576,
        maximumWireBytes = maximumHeaderBytes + maximumBodyBytes + 65_536,
        maximumOperationMilliseconds = 30_000,
        maximumLifetimeMilliseconds = 60_000,
        supervisorFactory = Values => new Hostˉtlsˉsupervisor(Values),
    }) {
        if (!(allowedTargets instanceof Set) || allowedTargets.size < 1 ||
            !Array.isArray(allowedHeaders) ||
            typeof supervisorFactory !== "function") {
            throw new Boundedˉhttpˉfailure("invalid_request", "HTTPS client binding is invalid.");
        }
        this.service = service;
        this.port = port;
        this.generation = generation;
        this.trustGeneration = trustGeneration;
        this.trustCertificates = trustCertificates;
        this.allowedTargets = new Set(allowedTargets);
        this.allowedHeaders = [...allowedHeaders];
        this.maximumRequestBytes = Positiveˉinteger(
            maximumRequestBytes, 65_536, "HTTPS request-byte limit",
        );
        this.maximumHeaderBytes = Positiveˉinteger(
            maximumHeaderBytes, 65_536, "HTTPS header-byte limit",
        );
        this.maximumBodyBytes = Positiveˉinteger(
            maximumBodyBytes, 16_777_216, "HTTPS body-byte limit",
        );
        this.maximumWireBytes = Positiveˉinteger(
            maximumWireBytes, 33_554_432, "HTTPS wire-byte limit",
        );
        if (this.maximumWireBytes < this.maximumHeaderBytes + this.maximumBodyBytes) {
            throw new Boundedˉhttpˉfailure("invalid_request", "HTTPS wire limit is inconsistent.");
        }
        this.maximumOperationMilliseconds = Positiveˉinteger(
            maximumOperationMilliseconds, 300_000, "HTTPS operation span",
        );
        this.maximumLifetimeMilliseconds = Positiveˉinteger(
            maximumLifetimeMilliseconds, 86_400_000, "HTTPS provider lifetime",
        );
        if (this.maximumLifetimeMilliseconds < this.maximumOperationMilliseconds) {
            throw new Boundedˉhttpˉfailure(
                "invalid_request", "HTTPS provider lifetime is shorter than its operation span.",
            );
        }
        this.supervisorFactory = supervisorFactory;
    }

    async request({
        method,
        target,
        headers = [],
        body = Buffer.alloc(0),
        timeoutMilliseconds = this.maximumOperationMilliseconds,
    }) {
        const Timeout = Positiveˉinteger(
            timeoutMilliseconds, this.maximumOperationMilliseconds, "HTTPS request timeout",
        );
        const Request = Buildˉboundedˉhttp1ˉrequest({
            method,
            target,
            service: this.service,
            port: this.port,
            headers,
            body,
            allowedTargets: this.allowedTargets,
            allowedHeaders: this.allowedHeaders,
            maximumRequestBytes: this.maximumRequestBytes,
        });
        const Deadline = process.hrtime.bigint() + BigInt(Timeout) * 1_000_000n;
        let Supervisor;
        let Connection;
        try {
            Supervisor = this.supervisorFactory({
                service: this.service,
                port: this.port,
                generation: this.generation,
                trustGeneration: this.trustGeneration,
                ...(this.trustCertificates === undefined ? {} : { trustCertificates: this.trustCertificates }),
                alpn: "http/1.1",
                maximumConnections: 1,
                maximumQueuedBytes: 65_536,
                maximumTransferBytes: BigInt(this.maximumRequestBytes + this.maximumWireBytes),
                maximumOperationMilliseconds: this.maximumOperationMilliseconds,
                maximumLifetimeMilliseconds: this.maximumLifetimeMilliseconds,
            });
            Connection = await Supervisor.connect(
                this.service, this.port, Remainingˉmilliseconds(Deadline),
            );
            if (Connection.status !== Hostˉnetworkˉstatus.Valid) Transportˉfailure(Connection);
            const Written = await Supervisor.write(
                Connection.connectionId,
                Connection.connectionGeneration,
                Request,
                Remainingˉmilliseconds(Deadline),
            );
            if (Written.status !== Hostˉnetworkˉstatus.Valid) {
                Transportˉfailure(Written, Written.status === Hostˉnetworkˉstatus.SubmissionIndeterminate);
            }
            if (Written.progress !== BigInt(Request.length)) {
                throw new Boundedˉhttpˉfailure(
                    "submission_indeterminate", "HTTPS request was only partially accepted.",
                );
            }
            const Decoder = new Boundedˉhttp1ˉresponseˉdecoder({
                maximumHeaderBytes: this.maximumHeaderBytes,
                maximumBodyBytes: this.maximumBodyBytes,
                maximumWireBytes: this.maximumWireBytes,
            });
            let Completed = null;
            const Resultˉwithˉevidence = Result => Object.freeze({
                ...Result,
                providerGeneration: Connection.providerGeneration,
                trustGeneration: this.trustGeneration,
                endpointAddress: Connection.address,
                endpointPort: Connection.endpointPort,
                requestBytesAccepted: Written.progress,
            });
            while (true) {
                const ReadMaximum = Math.min(
                    65_536, this.maximumWireBytes - Decoder.receivedBytes,
                );
                if (ReadMaximum < 1) {
                    throw new Boundedˉhttpˉfailure("limit", "HTTPS response exhausted its wire limit.");
                }
                const Read = await Supervisor.read(
                    Connection.connectionId,
                    Connection.connectionGeneration,
                    ReadMaximum,
                    Remainingˉmilliseconds(Deadline),
                );
                let Result;
                if (Read.status === Hostˉnetworkˉstatus.Valid) {
                    if (Completed !== null) {
                        if (Read.payload.length !== 0) {
                            throw new Boundedˉhttpˉfailure(
                                "framing", "HTTPS response contains excess bytes.",
                            );
                        }
                        if ((Read.flags & 1) !== 0) return Resultˉwithˉevidence(Completed);
                        continue;
                    }
                    Result = Decoder.push(Read.payload, (Read.flags & 1) !== 0);
                } else if (Read.status === Hostˉnetworkˉstatus.PeerClosed) {
                    if (Completed !== null) return Resultˉwithˉevidence(Completed);
                    Result = Decoder.push(Buffer.alloc(0), true);
                } else {
                    Transportˉfailure(Read);
                }
                if (Result !== null) {
                    if ((Read.flags & 1) !== 0) return Resultˉwithˉevidence(Result);
                    Completed = Result;
                }
            }
        } catch (Error) {
            if (Error instanceof Boundedˉhttpˉfailure) throw Error;
            throw new Boundedˉhttpˉfailure("provider_lost", "HTTPS provider failed.");
        } finally {
            Request.fill(0);
            if (Connection?.status === Hostˉnetworkˉstatus.Valid) {
                try {
                    await Supervisor.closeConnection(
                        Connection.connectionId,
                        Connection.connectionGeneration,
                        Math.min(1_000, Remainingˉmilliseconds(Deadline)),
                    );
                } catch {}
            }
            if (Supervisor) await Supervisor.teardown().catch(() => {});
        }
    }
}
