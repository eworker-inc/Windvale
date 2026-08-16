import dns from "node:dns/promises";
import net from "node:net";
import {
    Decodeˉhostˉnetworkˉrequest,
    Encodeˉhostˉnetworkˉresponse,
    Hostˉnetworkˉoperation,
    Hostˉnetworkˉstatus,
} from "./Host-Network-Protocol.mjs";

const MAX_RESOLVED_ADDRESSES = 32;
const CONNECT_STAGGER_MILLISECONDS = 100;

class Providerˉfailure extends Error {
    constructor(Status, Diagnostic) {
        super(Diagnostic);
        this.status = Status;
    }
}

function Fail(Status, Diagnostic) {
    throw new Providerˉfailure(Status, Diagnostic);
}

function Canonicalˉservice(Value) {
    if (typeof Value !== "string" || Value.length < 1 || Value.length > 253 ||
        Buffer.byteLength(Value, "ascii") !== Value.length ||
        /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/.test(Value)) return false;
    const Labels = Value.split(".");
    return Labels.every(Label => Label.length >= 1 && Label.length <= 63 &&
        /^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/.test(Label));
}

function Positiveˉinteger(Value, Maximum, Description) {
    if (!Number.isSafeInteger(Value) || Value < 1 || Value > Maximum) {
        throw new Error(`${Description} is invalid.`);
    }
    return Value;
}

function Positiveˉbigint(Value, Description) {
    if (typeof Value !== "bigint" || Value < 1n || Value > 0xffff_ffff_ffff_ffffn) {
        throw new Error(`${Description} is invalid.`);
    }
    return Value;
}

function Canonicalˉaddress(Value, Family) {
    if (typeof Value !== "string" || net.isIP(Value) !== Family) return "";
    if (Family === 4) {
        const Canonical = Value.split(".").map(Part => String(Number(Part))).join(".");
        return net.isIP(Canonical) === 4 ? Canonical : "";
    }
    if (Value.includes("%")) return "";
    try {
        const Host = new URL(`http://[${Value}]/`).hostname;
        const Canonical = Host.slice(1, -1).toLowerCase();
        return net.isIP(Canonical) === 6 ? Canonical : "";
    } catch {
        return "";
    }
}

function Remainingˉmilliseconds(Deadline) {
    const Remaining = Deadline - process.hrtime.bigint();
    if (Remaining <= 0n) return 0;
    const Rounded = (Remaining + 999_999n) / 1_000_000n;
    return Number(Rounded > 2_147_483_647n ? 2_147_483_647n : Rounded);
}

function Failureˉdiagnostic(Status) {
    const Values = new Map([
        [Hostˉnetworkˉstatus.InvalidRequest, "Network request is invalid."],
        [Hostˉnetworkˉstatus.Unauthorized, "Network authority denied the request."],
        [Hostˉnetworkˉstatus.Stale, "Network provider or connection generation is stale."],
        [Hostˉnetworkˉstatus.Expired, "Network operation deadline or grant expired."],
        [Hostˉnetworkˉstatus.Unavailable, "Network peer is unavailable."],
        [Hostˉnetworkˉstatus.Limit, "Network provider limit was reached."],
        [Hostˉnetworkˉstatus.Reset, "Network connection was reset."],
        [Hostˉnetworkˉstatus.PeerClosed, "Network peer closed its write half."],
        [Hostˉnetworkˉstatus.SubmissionIndeterminate, "Network write acceptance is indeterminate."],
        [Hostˉnetworkˉstatus.ProviderLost, "Network provider was lost."],
        [Hostˉnetworkˉstatus.Cancelled, "Network operation was cancelled."],
    ]);
    return Values.get(Status) ?? "Network provider failed.";
}

function Resolveˉaddresses(DefaultResolver, Service, Deadline) {
    const Timeout = Remainingˉmilliseconds(Deadline);
    if (Timeout === 0) Fail(Hostˉnetworkˉstatus.Expired, Failureˉdiagnostic(Hostˉnetworkˉstatus.Expired));
    return new Promise((Resolve, Reject) => {
        let Complete = false;
        const Timer = setTimeout(() => {
            if (Complete) return;
            Complete = true;
            Reject(new Providerˉfailure(
                Hostˉnetworkˉstatus.Expired, Failureˉdiagnostic(Hostˉnetworkˉstatus.Expired),
            ));
        }, Timeout);
        Promise.resolve(DefaultResolver(Service, { all: true, verbatim: true })).then(Values => {
            if (Complete) return;
            Complete = true;
            clearTimeout(Timer);
            if (!Array.isArray(Values) || Values.length < 1 || Values.length > MAX_RESOLVED_ADDRESSES) {
                Reject(new Providerˉfailure(
                    Hostˉnetworkˉstatus.Unavailable,
                    Failureˉdiagnostic(Hostˉnetworkˉstatus.Unavailable),
                ));
                return;
            }
            const Seen = new Set();
            const Addresses = [];
            for (const Value of Values) {
                if (Value === null || typeof Value !== "object" || ![4, 6].includes(Value.family)) continue;
                const Address = Canonicalˉaddress(Value.address, Value.family);
                if (Address === "") continue;
                const Key = `${Value.family}:${Address}`;
                if (!Seen.has(Key)) {
                    Seen.add(Key);
                    Addresses.push({ address: Address, family: Value.family });
                }
            }
            if (Addresses.length === 0) {
                Reject(new Providerˉfailure(
                    Hostˉnetworkˉstatus.Unavailable,
                    Failureˉdiagnostic(Hostˉnetworkˉstatus.Unavailable),
                ));
                return;
            }
            Resolve(Addresses);
        }, () => {
            if (Complete) return;
            Complete = true;
            clearTimeout(Timer);
            Reject(new Providerˉfailure(
                Hostˉnetworkˉstatus.Unavailable, Failureˉdiagnostic(Hostˉnetworkˉstatus.Unavailable),
            ));
        });
    });
}

function Connectˉaddresses(Addresses, Port, Deadline, MaximumQueuedBytes, SocketFactory) {
    return new Promise((Resolve, Reject) => {
        const Sockets = new Set();
        const LaunchTimers = new Set();
        let Settled = false;
        let Launched = 0;
        let Failed = 0;
        const Completeˉfailure = Status => {
            if (Settled) return;
            Settled = true;
            clearTimeout(DeadlineTimer);
            for (const Timer of LaunchTimers) clearTimeout(Timer);
            for (const Socket of Sockets) Socket.destroy();
            Reject(new Providerˉfailure(Status, Failureˉdiagnostic(Status)));
        };
        const Timeout = Remainingˉmilliseconds(Deadline);
        if (Timeout === 0) {
            Reject(new Providerˉfailure(
                Hostˉnetworkˉstatus.Expired, Failureˉdiagnostic(Hostˉnetworkˉstatus.Expired),
            ));
            return;
        }
        const DeadlineTimer = setTimeout(
            () => Completeˉfailure(Hostˉnetworkˉstatus.Expired), Timeout,
        );
        const Launch = Candidate => {
            if (Settled) return;
            Launched += 1;
            let Socket;
            try {
                Socket = SocketFactory({
                    host: Candidate.address,
                    port: Port,
                    family: Candidate.family,
                    autoSelectFamily: false,
                    allowHalfOpen: true,
                    readableHighWaterMark: MaximumQueuedBytes,
                    writableHighWaterMark: MaximumQueuedBytes,
                });
            } catch {
                Failed += 1;
                if (Launched === Addresses.length && Failed === Addresses.length) {
                    Completeˉfailure(Hostˉnetworkˉstatus.Unavailable);
                }
                return;
            }
            Sockets.add(Socket);
            Socket.once("connect", () => {
                if (Settled) {
                    Socket.destroy();
                    return;
                }
                Settled = true;
                clearTimeout(DeadlineTimer);
                for (const Timer of LaunchTimers) clearTimeout(Timer);
                for (const Other of Sockets) if (Other !== Socket) Other.destroy();
                Resolve(Socket);
            });
            Socket.once("error", () => {
                Failed += 1;
                Sockets.delete(Socket);
                if (!Settled && Launched === Addresses.length && Failed === Addresses.length) {
                    Completeˉfailure(Hostˉnetworkˉstatus.Unavailable);
                }
            });
        };
        for (let Index = 0; Index < Addresses.length; Index += 1) {
            const Delay = Index * CONNECT_STAGGER_MILLISECONDS;
            if (Delay === 0) Launch(Addresses[Index]);
            else {
                const Timer = setTimeout(() => {
                    LaunchTimers.delete(Timer);
                    Launch(Addresses[Index]);
                }, Delay);
                LaunchTimers.add(Timer);
            }
        }
    });
}

export class Hostˉnetworkˉprovider {
    constructor({
        service,
        port,
        generation,
        maximumConnections = 1,
        maximumQueuedBytes = 65_536,
        maximumTransferBytes = 16_777_216n,
        maximumOperationMilliseconds = 30_000,
        maximumLifetimeMilliseconds = 3_600_000,
        resolver = dns.lookup,
        socketFactory = net.createConnection,
    }) {
        if (!Canonicalˉservice(service)) throw new Error("Service authority is invalid.");
        this.service = service;
        this.port = Positiveˉinteger(port, 65_535, "Port authority");
        this.generation = Positiveˉbigint(generation, "Provider generation");
        this.maximumConnections = Positiveˉinteger(maximumConnections, 64, "Connection limit");
        this.maximumQueuedBytes = Positiveˉinteger(maximumQueuedBytes, 65_536, "Queued-byte limit");
        this.maximumTransferBytes = Positiveˉbigint(maximumTransferBytes, "Transfer limit");
        this.maximumOperationNanoseconds = BigInt(Positiveˉinteger(
            maximumOperationMilliseconds, 300_000, "Operation span",
        )) * 1_000_000n;
        this.expiresAt = process.hrtime.bigint() + BigInt(Positiveˉinteger(
            maximumLifetimeMilliseconds, 86_400_000, "Provider lifetime",
        )) * 1_000_000n;
        if (typeof resolver !== "function" || typeof socketFactory !== "function") {
            throw new Error("Host-network mechanism is invalid.");
        }
        this.resolver = resolver;
        this.socketFactory = socketFactory;
        this.connections = new Map();
        this.nextConnectionId = 1n;
        this.activeConnects = 0;
        this.activeResolutions = 0;
        this.closed = false;
    }

    #Base(Request) {
        return {
            requestId: Request.requestId,
            providerGeneration: this.generation,
        };
    }

    #Failure(Request, Status) {
        return Encodeˉhostˉnetworkˉresponse({
            ...this.#Base(Request),
            status: Status,
            connectionId: Request.connectionId,
            connectionGeneration: Request.connectionGeneration,
            diagnostic: Failureˉdiagnostic(Status),
        });
    }

    #Admitˉdeadline(Request) {
        const Now = process.hrtime.bigint();
        if (Now >= this.expiresAt || Request.deadlineNanoseconds <= Now) {
            Fail(Hostˉnetworkˉstatus.Expired, Failureˉdiagnostic(Hostˉnetworkˉstatus.Expired));
        }
        if (Request.deadlineNanoseconds - Now > this.maximumOperationNanoseconds ||
            Request.deadlineNanoseconds > this.expiresAt) {
            Fail(Hostˉnetworkˉstatus.Unauthorized, Failureˉdiagnostic(Hostˉnetworkˉstatus.Unauthorized));
        }
    }

    #Connection(Request, AllowTerminal = false) {
        const Connection = this.connections.get(Request.connectionId);
        if (!Connection || Connection.generation !== Request.connectionGeneration) {
            Fail(Hostˉnetworkˉstatus.Stale, Failureˉdiagnostic(Hostˉnetworkˉstatus.Stale));
        }
        if (!AllowTerminal && Connection.terminalStatus !== 0) {
            Fail(Connection.terminalStatus, Failureˉdiagnostic(Connection.terminalStatus));
        }
        return Connection;
    }

    async #Connect(Request) {
        if (Request.service !== this.service || Request.control !== this.port) {
            Fail(Hostˉnetworkˉstatus.Unauthorized, Failureˉdiagnostic(Hostˉnetworkˉstatus.Unauthorized));
        }
        if (this.connections.size + this.activeConnects + this.activeResolutions >=
            this.maximumConnections) {
            Fail(Hostˉnetworkˉstatus.Limit, Failureˉdiagnostic(Hostˉnetworkˉstatus.Limit));
        }
        let Socket;
        const TrackedResolver = (...Arguments) => {
            this.activeResolutions += 1;
            return Promise.resolve(this.resolver(...Arguments)).then(Value => {
                this.activeResolutions -= 1;
                return Value;
            }, Error => {
                this.activeResolutions -= 1;
                throw Error;
            });
        };
        const Addresses = await Resolveˉaddresses(
            TrackedResolver, Request.service, Request.deadlineNanoseconds,
        );
        this.activeConnects += 1;
        try {
            Socket = await Connectˉaddresses(
                Addresses,
                Request.control,
                Request.deadlineNanoseconds,
                this.maximumQueuedBytes,
                this.socketFactory,
            );
        } finally {
            this.activeConnects -= 1;
        }
        const EndpointFamily = Socket.remoteFamily === "IPv6" ? 6 :
            Socket.remoteFamily === "IPv4" ? 4 : 0;
        const EndpointAddress = Canonicalˉaddress(Socket.remoteAddress, EndpointFamily);
        if (EndpointFamily === 0 || EndpointAddress === "" || Socket.remotePort !== Request.control) {
            Socket.destroy();
            Fail(Hostˉnetworkˉstatus.Unavailable, Failureˉdiagnostic(Hostˉnetworkˉstatus.Unavailable));
        }
        const ConnectionId = this.nextConnectionId;
        this.nextConnectionId += 1n;
        const Connection = {
            id: ConnectionId,
            generation: 1n,
            socket: Socket,
            transferBytes: 0n,
            readReservation: 0,
            writeReservation: 0,
            readActive: false,
            writeActive: false,
            peerClosed: false,
            localClosed: false,
            terminalStatus: 0,
        };
        Socket.on("end", () => { Connection.peerClosed = true; });
        Socket.on("error", () => {
            if (Connection.terminalStatus === 0) Connection.terminalStatus = Hostˉnetworkˉstatus.Reset;
        });
        Socket.on("close", HadError => {
            if (HadError && Connection.terminalStatus === 0) {
                Connection.terminalStatus = Hostˉnetworkˉstatus.Reset;
            }
        });
        Socket.pause();
        this.connections.set(ConnectionId, Connection);
        return Encodeˉhostˉnetworkˉresponse({
            ...this.#Base(Request),
            status: Hostˉnetworkˉstatus.Valid,
            connectionId: ConnectionId,
            connectionGeneration: Connection.generation,
            endpointPort: Socket.remotePort,
            endpointFamily: EndpointFamily,
            address: EndpointAddress,
        });
    }

    async #Write(Request) {
        const Connection = this.#Connection(Request);
        if (Connection.writeActive || Connection.localClosed) {
            Fail(Hostˉnetworkˉstatus.InvalidRequest, Failureˉdiagnostic(Hostˉnetworkˉstatus.InvalidRequest));
        }
        if (Request.payload.length + Connection.readReservation > this.maximumQueuedBytes ||
            BigInt(Request.payload.length + Connection.readReservation) >
                this.maximumTransferBytes - Connection.transferBytes) {
            Fail(Hostˉnetworkˉstatus.Limit, Failureˉdiagnostic(Hostˉnetworkˉstatus.Limit));
        }
        Connection.writeActive = true;
        Connection.writeReservation = Request.payload.length;
        let DispatchBegan = false;
        try {
            await new Promise((Resolve, Reject) => {
                let Complete = false;
                const Finish = Error => {
                    if (Complete) return;
                    Complete = true;
                    clearTimeout(Timer);
                    Connection.socket.off("error", OnError);
                    if (Error) Reject(Error); else Resolve();
                };
                const OnError = () => Finish(new Error("socket"));
                const Timer = setTimeout(() => {
                    Connection.socket.destroy();
                    Finish(new Error("deadline"));
                }, Remainingˉmilliseconds(Request.deadlineNanoseconds));
                Connection.socket.once("error", OnError);
                try {
                    DispatchBegan = true;
                    Connection.socket.write(Request.payload, Error => Finish(Error));
                } catch (Error) {
                    Finish(Error);
                }
            });
        } catch {
            if (DispatchBegan) {
                Connection.terminalStatus = Hostˉnetworkˉstatus.SubmissionIndeterminate;
                Fail(
                    Hostˉnetworkˉstatus.SubmissionIndeterminate,
                    Failureˉdiagnostic(Hostˉnetworkˉstatus.SubmissionIndeterminate),
                );
            }
            Fail(Hostˉnetworkˉstatus.Reset, Failureˉdiagnostic(Hostˉnetworkˉstatus.Reset));
        } finally {
            Connection.writeActive = false;
            Connection.writeReservation = 0;
        }
        Connection.transferBytes += BigInt(Request.payload.length);
        return Encodeˉhostˉnetworkˉresponse({
            ...this.#Base(Request), status: Hostˉnetworkˉstatus.Valid,
            connectionId: Connection.id, connectionGeneration: Connection.generation,
            progress: BigInt(Request.payload.length), flags: Connection.peerClosed ? 1 : 0,
        });
    }

    async #Read(Request) {
        const Connection = this.#Connection(Request);
        if (Connection.readActive) {
            Fail(Hostˉnetworkˉstatus.InvalidRequest, Failureˉdiagnostic(Hostˉnetworkˉstatus.InvalidRequest));
        }
        if (Request.control + Connection.writeReservation > this.maximumQueuedBytes ||
            BigInt(Request.control + Connection.writeReservation) >
                this.maximumTransferBytes - Connection.transferBytes) {
            Fail(Hostˉnetworkˉstatus.Limit, Failureˉdiagnostic(Hostˉnetworkˉstatus.Limit));
        }
        const Publish = Chunk => {
            Connection.transferBytes += BigInt(Chunk.length);
            return Encodeˉhostˉnetworkˉresponse({
                ...this.#Base(Request), status: Hostˉnetworkˉstatus.Valid,
                connectionId: Connection.id, connectionGeneration: Connection.generation,
                progress: BigInt(Chunk.length), payload: Chunk,
                flags: Connection.peerClosed && Connection.socket.readableLength === 0 ? 1 : 0,
            });
        };
        const Immediate = Connection.socket.read(Request.control);
        if (Immediate !== null) return Publish(Immediate);
        if (Connection.peerClosed) {
            Fail(Hostˉnetworkˉstatus.PeerClosed, Failureˉdiagnostic(Hostˉnetworkˉstatus.PeerClosed));
        }
        Connection.readActive = true;
        Connection.readReservation = Request.control;
        try {
            const Chunk = await new Promise((Resolve, Reject) => {
                let Complete = false;
                const Finish = (Error, Value) => {
                    if (Complete) return;
                    Complete = true;
                    clearTimeout(Timer);
                    Connection.socket.off("readable", OnReadable);
                    Connection.socket.off("end", OnEnd);
                    Connection.socket.off("error", OnError);
                    if (Error) Reject(Error); else Resolve(Value);
                };
                const OnReadable = () => {
                    if (Connection.socket.readableLength > this.maximumQueuedBytes) {
                        Connection.socket.destroy();
                        Finish(new Providerˉfailure(
                            Hostˉnetworkˉstatus.Limit, Failureˉdiagnostic(Hostˉnetworkˉstatus.Limit),
                        ));
                        return;
                    }
                    const Value = Connection.socket.read(Request.control);
                    if (Value !== null) Finish(null, Value);
                };
                const OnEnd = () => Finish(new Providerˉfailure(
                    Hostˉnetworkˉstatus.PeerClosed, Failureˉdiagnostic(Hostˉnetworkˉstatus.PeerClosed),
                ));
                const OnError = () => Finish(new Providerˉfailure(
                    Hostˉnetworkˉstatus.Reset, Failureˉdiagnostic(Hostˉnetworkˉstatus.Reset),
                ));
                const Timer = setTimeout(() => Finish(new Providerˉfailure(
                    Hostˉnetworkˉstatus.Expired, Failureˉdiagnostic(Hostˉnetworkˉstatus.Expired),
                )), Remainingˉmilliseconds(Request.deadlineNanoseconds));
                Connection.socket.on("readable", OnReadable);
                Connection.socket.once("end", OnEnd);
                Connection.socket.once("error", OnError);
            });
            return Publish(Chunk);
        } finally {
            Connection.readActive = false;
            Connection.readReservation = 0;
        }
    }

    async #Shutdownˉwrite(Request) {
        const Connection = this.#Connection(Request);
        if (Connection.writeActive || Connection.localClosed) {
            Fail(Hostˉnetworkˉstatus.InvalidRequest, Failureˉdiagnostic(Hostˉnetworkˉstatus.InvalidRequest));
        }
        Connection.writeActive = true;
        try {
            await new Promise((Resolve, Reject) => {
                let Complete = false;
                const Finish = Error => {
                    if (Complete) return;
                    Complete = true;
                    clearTimeout(Timer);
                    Connection.socket.off("error", OnError);
                    if (Error) Reject(Error); else Resolve();
                };
                const OnError = () => Finish(new Error("socket"));
                const Timer = setTimeout(() => {
                    Connection.socket.destroy();
                    Finish(new Error("deadline"));
                }, Remainingˉmilliseconds(Request.deadlineNanoseconds));
                Connection.socket.once("error", OnError);
                try { Connection.socket.end(Error => Finish(Error)); } catch (Error) { Finish(Error); }
            });
        } catch {
            Connection.terminalStatus = Hostˉnetworkˉstatus.SubmissionIndeterminate;
            Fail(
                Hostˉnetworkˉstatus.SubmissionIndeterminate,
                Failureˉdiagnostic(Hostˉnetworkˉstatus.SubmissionIndeterminate),
            );
        } finally {
            Connection.writeActive = false;
        }
        Connection.localClosed = true;
        return Encodeˉhostˉnetworkˉresponse({
            ...this.#Base(Request), status: Hostˉnetworkˉstatus.Valid,
            connectionId: Connection.id, connectionGeneration: Connection.generation,
            flags: (Connection.peerClosed ? 1 : 0) | 2,
        });
    }

    #Close(Request) {
        const Connection = this.#Connection(Request, true);
        Connection.socket.destroy();
        this.connections.delete(Connection.id);
        return Encodeˉhostˉnetworkˉresponse({
            ...this.#Base(Request), status: Hostˉnetworkˉstatus.Valid,
            connectionId: Connection.id, connectionGeneration: Connection.generation,
            flags: (Connection.peerClosed ? 1 : 0) | (Connection.localClosed ? 2 : 0),
        });
    }

    async handle(Value) {
        let Request;
        try { Request = Decodeˉhostˉnetworkˉrequest(Value); } catch { throw new Error("Malformed WVNR request."); }
        if (this.closed) return this.#Failure(Request, Hostˉnetworkˉstatus.ProviderLost);
        if (Request.providerGeneration !== this.generation) {
            return this.#Failure(Request, Hostˉnetworkˉstatus.Stale);
        }
        try {
            this.#Admitˉdeadline(Request);
            if (Request.operation === Hostˉnetworkˉoperation.Connect) return await this.#Connect(Request);
            if (Request.operation === Hostˉnetworkˉoperation.Write) return await this.#Write(Request);
            if (Request.operation === Hostˉnetworkˉoperation.Read) return await this.#Read(Request);
            if (Request.operation === Hostˉnetworkˉoperation.ShutdownWrite) {
                return await this.#Shutdownˉwrite(Request);
            }
            if (Request.operation === Hostˉnetworkˉoperation.Close) return this.#Close(Request);
            return this.#Failure(Request, Hostˉnetworkˉstatus.InvalidRequest);
        } catch (Error) {
            if (Error instanceof Providerˉfailure) return this.#Failure(Request, Error.status);
            return this.#Failure(Request, Hostˉnetworkˉstatus.ProviderLost);
        }
    }

    teardown() {
        if (this.closed) return;
        this.closed = true;
        for (const Connection of this.connections.values()) {
            Connection.terminalStatus = Hostˉnetworkˉstatus.ProviderLost;
            Connection.socket.destroy();
        }
        this.connections.clear();
    }
}
