import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import net from "node:net";
import { once } from "node:events";
import { fileURLToPath } from "node:url";
import { Hostˉnetworkˉprovider } from "../../Runtime/Hosted/Network/Host-Network-Provider-Core.mjs";
import { Hostˉnetworkˉsupervisor } from "../../Runtime/Hosted/Network/Host-Network-Supervisor.mjs";
import {
    Decodeˉhostˉnetworkˉrequest,
    Decodeˉhostˉnetworkˉresponse,
    Encodeˉhostˉnetworkˉrequest,
    Encodeˉhostˉnetworkˉresponse,
    Hostˉnetworkˉoperation,
    Hostˉnetworkˉstatus,
} from "../../Runtime/Hosted/Network/Host-Network-Protocol.mjs";

const PROVIDER_PATH = fileURLToPath(new URL(
    "../../Runtime/Hosted/Network/Host-Network-Provider.mjs", import.meta.url,
));

function Deadline(Milliseconds = 2_000) {
    return process.hrtime.bigint() + BigInt(Milliseconds) * 1_000_000n;
}

function Request({
    operation = Hostˉnetworkˉoperation.Connect,
    requestId = 1n,
    providerGeneration = 7n,
    connectionId = 0n,
    connectionGeneration = 0n,
    deadlineNanoseconds = Deadline(),
    control = 0,
    service = "",
    payload = Buffer.alloc(0),
} = {}) {
    return Encodeˉhostˉnetworkˉrequest({
        operation, requestId, providerGeneration, connectionId, connectionGeneration,
        deadlineNanoseconds, control, service, payload,
    });
}

function Connectˉrequest(Extra = {}) {
    return Request({ control: Extra.port, service: "localhost", ...Extra });
}

function Connectionˉrequest(Operation, Connection, Extra = {}) {
    return Request({
        operation: Operation,
        requestId: Extra.requestId ?? 2n,
        connectionId: Connection.connectionId,
        connectionGeneration: Connection.connectionGeneration,
        control: Extra.control ?? 0,
        payload: Extra.payload ?? Buffer.alloc(0),
        deadlineNanoseconds: Extra.deadlineNanoseconds ?? Deadline(),
        providerGeneration: Extra.providerGeneration ?? 7n,
    });
}

function Provider(Port, Extra = {}) {
    return new Hostˉnetworkˉprovider({
        service: "localhost",
        port: Port,
        generation: 7n,
        maximumConnections: 2,
        maximumQueuedBytes: 65_536,
        maximumTransferBytes: 1_048_576n,
        maximumOperationMilliseconds: 5_000,
        maximumLifetimeMilliseconds: 30_000,
        resolver: async () => [{ address: "127.0.0.1", family: 4 }],
        ...Extra,
    });
}

async function Server(Handler = () => {}) {
    const Sockets = new Set();
    const Listener = net.createServer(Socket => {
        Sockets.add(Socket);
        Socket.on("close", () => Sockets.delete(Socket));
        Handler(Socket);
    });
    Listener.listen({ host: "127.0.0.1", port: 0, exclusive: true });
    await once(Listener, "listening");
    return {
        port: Listener.address().port,
        close: async () => {
            for (const Socket of Sockets) Socket.destroy();
            Listener.close();
            await once(Listener, "close");
        },
    };
}

async function Withˉserver(Handler, Test) {
    const Fixture = await Server(Handler);
    try { await Test(Fixture); } finally { await Fixture.close(); }
}

const Tests = [
    ["canonical request roundtrip", async () => {
        const Bytes = Connectˉrequest({ port: 443 });
        const Value = Decodeˉhostˉnetworkˉrequest(Bytes);
        assert.deepEqual(
            [Value.operation, Value.requestId, Value.service, Value.control],
            [1, 1n, "localhost", 443],
        );
    }],
    ["malformed request rejection", async () => {
        const Bytes = Connectˉrequest({ port: 443 });
        Bytes.writeUInt32LE(Bytes.length + 1, 8);
        assert.throws(() => Decodeˉhostˉnetworkˉrequest(Bytes), /malformed/);
    }],
    ["request operation invariants", async () => {
        assert.throws(() => Request({ operation: 2, payload: Buffer.from("x") }), /invariant/);
        assert.throws(() => Connectˉrequest({ port: 0 }), /invariant/);
    }],
    ["canonical response roundtrip", async () => {
        const Bytes = Encodeˉhostˉnetworkˉresponse({
            status: 0, requestId: 4n, providerGeneration: 7n,
            connectionId: 2n, connectionGeneration: 1n, progress: 2n,
            endpointPort: 443, endpointFamily: 4, address: "127.0.0.1",
            payload: Buffer.from("ok"),
        });
        const Value = Decodeˉhostˉnetworkˉresponse(Bytes);
        assert.deepEqual([Value.status, Value.address, Value.payload.toString()], [0, "127.0.0.1", "ok"]);
    }],
    ["response status invariants", async () => {
        assert.throws(() => Encodeˉhostˉnetworkˉresponse({
            status: 2, requestId: 1n, providerGeneration: 7n, payload: Buffer.from("leak"),
            diagnostic: "denied",
        }), /invariant/);
    }],
    ["configuration admission", async () => {
        assert.throws(() => Provider(443, { service: "LOCALHOST" }), /authority/);
        assert.throws(() => Provider(443, { maximumConnections: 65 }), /limit/);
    }],
    ["stale provider generation", async () => {
        const ProviderValue = Provider(443);
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443, providerGeneration: 6n }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Stale);
        ProviderValue.teardown();
    }],
    ["service authority denial", async () => {
        let Resolved = false;
        const ProviderValue = Provider(443, { resolver: async () => { Resolved = true; return []; } });
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Request({ operation: 1, control: 443, service: "example.com" }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Unauthorized);
        assert.equal(Resolved, false);
        ProviderValue.teardown();
    }],
    ["deadline-span denial", async () => {
        const ProviderValue = Provider(443, { maximumOperationMilliseconds: 10 });
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443, deadlineNanoseconds: Deadline(100) }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Unauthorized);
        ProviderValue.teardown();
    }],
    ["expired operation rejection", async () => {
        const ProviderValue = Provider(443);
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443, deadlineNanoseconds: 1n }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Expired);
        ProviderValue.teardown();
    }],
    ["resolver unavailability", async () => {
        const ProviderValue = Provider(443, { resolver: async () => { throw new Error("dns"); } });
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443 }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Unavailable);
        ProviderValue.teardown();
    }],
    ["uncancellable resolver debt is bounded", async () => {
        let Calls = 0;
        const ProviderValue = Provider(443, {
            maximumConnections: 1,
            resolver: async () => {
                Calls += 1;
                return new Promise(() => {});
            },
        });
        const First = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443, deadlineNanoseconds: Deadline(20) }),
        ));
        const Second = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443, requestId: 2n }),
        ));
        assert.deepEqual([First.status, Second.status, Calls], [4, 6, 1]);
        ProviderValue.teardown();
    }],
    ["resolved address admission", async () => {
        const ProviderValue = Provider(443, { resolver: async () => [
            { address: "not-an-address", family: 4 },
        ] });
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: 443 }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Unavailable);
        ProviderValue.teardown();
    }],
    ["loopback connect endpoint evidence", async () => Withˉserver(() => {}, async Fixture => {
        const ProviderValue = Provider(Fixture.port);
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        assert.deepEqual(
            [Result.status, Result.endpointFamily, Result.address, Result.endpointPort],
            [0, 4, "127.0.0.1", Fixture.port],
        );
        ProviderValue.teardown();
    })],
    ["exact write and read progress", async () => Withˉserver(Socket => {
        Socket.once("data", Value => {
            assert.equal(Value.toString(), "ping");
            Socket.write("pong");
        });
    }, async Fixture => {
        const ProviderValue = Provider(Fixture.port);
        const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        const Written = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(2, Connected, { payload: Buffer.from("ping") }),
        ));
        const Read = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(3, Connected, { requestId: 3n, control: 4 }),
        ));
        assert.deepEqual([Written.progress, Read.progress, Read.payload.toString()], [4n, 4n, "pong"]);
        ProviderValue.teardown();
    })],
    ["stale connection generation", async () => Withˉserver(() => {}, async Fixture => {
        const ProviderValue = Provider(Fixture.port);
        const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(Request({
            operation: 3, requestId: 2n, connectionId: Connected.connectionId,
            connectionGeneration: 2n, control: 1,
        })));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Stale);
        ProviderValue.teardown();
    })],
    ["connection-count limit and release", async () => Withˉserver(() => {}, async Fixture => {
        const ProviderValue = Provider(Fixture.port, { maximumConnections: 1 });
        const First = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        const Denied = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port, requestId: 2n }),
        ));
        assert.equal(Denied.status, Hostˉnetworkˉstatus.Limit);
        const Closed = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(5, First, { requestId: 3n }),
        ));
        const Second = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port, requestId: 4n }),
        ));
        assert.deepEqual([Closed.status, Second.status], [0, 0]);
        ProviderValue.teardown();
    })],
    ["transfer-budget enforcement", async () => Withˉserver(() => {}, async Fixture => {
        const ProviderValue = Provider(Fixture.port, { maximumTransferBytes: 3n });
        const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(2, Connected, { payload: Buffer.from("four") }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Limit);
        ProviderValue.teardown();
    })],
    ["read deadline without replay", async () => Withˉserver(() => {}, async Fixture => {
        const ProviderValue = Provider(Fixture.port);
        const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(3, Connected, { control: 1, deadlineNanoseconds: Deadline(30) }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.Expired);
        ProviderValue.teardown();
    })],
    ["one read in flight", async () => {
        let Peer;
        await Withˉserver(Socket => { Peer = Socket; }, async Fixture => {
            const ProviderValue = Provider(Fixture.port);
            const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
                Connectˉrequest({ port: Fixture.port }),
            ));
            const FirstPromise = ProviderValue.handle(Connectionˉrequest(3, Connected, {
                control: 1, deadlineNanoseconds: Deadline(1_000),
            }));
            const Second = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
                Connectionˉrequest(3, Connected, { requestId: 3n, control: 1 }),
            ));
            assert.equal(Second.status, Hostˉnetworkˉstatus.InvalidRequest);
            Peer.write("x");
            const First = Decodeˉhostˉnetworkˉresponse(await FirstPromise);
            assert.equal(First.payload.toString(), "x");
            ProviderValue.teardown();
        });
    }],
    ["combined read and write reservation", async () => {
        let Peer;
        await Withˉserver(Socket => { Peer = Socket; }, async Fixture => {
            const ProviderValue = Provider(Fixture.port, { maximumQueuedBytes: 4 });
            const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
                Connectˉrequest({ port: Fixture.port }),
            ));
            const ReadPromise = ProviderValue.handle(Connectionˉrequest(3, Connected, {
                control: 3, deadlineNanoseconds: Deadline(1_000),
            }));
            const Write = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
                Connectionˉrequest(2, Connected, { requestId: 3n, payload: Buffer.from("xx") }),
            ));
            assert.equal(Write.status, Hostˉnetworkˉstatus.Limit);
            Peer.write("abc");
            const Read = Decodeˉhostˉnetworkˉresponse(await ReadPromise);
            assert.equal(Read.payload.toString(), "abc");
            ProviderValue.teardown();
        });
    }],
    ["write half-close and peer close", async () => Withˉserver(Socket => {
        Socket.on("end", () => Socket.end());
    }, async Fixture => {
        const ProviderValue = Provider(Fixture.port);
        const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        const Shutdown = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(4, Connected),
        ));
        assert.equal(Shutdown.flags & 2, 2);
        const Read = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(3, Connected, { requestId: 3n, control: 1 }),
        ));
        assert.equal(Read.status, Hostˉnetworkˉstatus.PeerClosed);
        ProviderValue.teardown();
    })],
    ["provider teardown", async () => Withˉserver(() => {}, async Fixture => {
        const ProviderValue = Provider(Fixture.port);
        const Connected = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectˉrequest({ port: Fixture.port }),
        ));
        ProviderValue.teardown();
        const Result = Decodeˉhostˉnetworkˉresponse(await ProviderValue.handle(
            Connectionˉrequest(3, Connected, { control: 1 }),
        ));
        assert.equal(Result.status, Hostˉnetworkˉstatus.ProviderLost);
    })],
    ["supervised provider end to end", async () => Withˉserver(Socket => {
        Socket.once("data", Value => Socket.write(Buffer.from(Value).reverse()));
    }, async Fixture => {
        const Supervisor = new Hostˉnetworkˉsupervisor({
            service: "localhost", port: Fixture.port, generation: 12n,
            maximumOperationMilliseconds: 5_000, maximumLifetimeMilliseconds: 30_000,
        });
        try {
            const Connected = await Supervisor.connect("localhost", Fixture.port);
            assert.equal(Connected.status, 0);
            const Written = await Supervisor.write(
                Connected.connectionId, Connected.connectionGeneration, Buffer.from("abc"),
            );
            const Read = await Supervisor.read(
                Connected.connectionId, Connected.connectionGeneration, 3,
            );
            assert.deepEqual([Written.progress, Read.payload.toString()], [3n, "cba"]);
            const Closed = await Supervisor.closeConnection(
                Connected.connectionId, Connected.connectionGeneration,
            );
            assert.equal(Closed.status, 0);
        } finally {
            await Supervisor.teardown();
        }
    })],
    ["daemon malformed-input containment", async () => {
        const Child = spawn(process.execPath, [
            PROVIDER_PATH, "--service", "localhost", "--port", "443", "--generation", "1",
        ], { stdio: ["pipe", "pipe", "pipe"], windowsHide: true, env: {} });
        Child.stdin.end(Buffer.from("bad request!"));
        const [Code] = await once(Child, "exit");
        assert.equal(Code, 65);
    }],
];

for (const [Name, Test] of Tests) {
    try { await Test(); } catch (Error) {
        Error.message = `${Name}: ${Error.message}`;
        throw Error;
    }
}

assert.ok(process.platform === "win32" || process.platform === "linux");
process.stdout.write(
    `host network provider status=Passed platforms=2 cases=${Tests.length} ` +
    "current-host=Verified public-network=0 credentials=0\n",
);
