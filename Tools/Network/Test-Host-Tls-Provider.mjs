import assert from "node:assert/strict";
import net from "node:net";
import tls from "node:tls";
import { once } from "node:events";
import { Createˉephemeralˉtlsˉfixture } from "./Ephemeral-Tls-Fixture.mjs";
import {
    Canonicalˉtrustˉsnapshot,
    Hostˉtlsˉprovider,
} from "../../Runtime/Hosted/Network/Host-Tls-Provider-Core.mjs";
import { Hostˉtlsˉsupervisor } from "../../Runtime/Hosted/Network/Host-Tls-Supervisor.mjs";
import {
    Decodeˉhostˉnetworkˉresponse,
    Encodeˉhostˉnetworkˉrequest,
    Hostˉnetworkˉoperation,
    Hostˉnetworkˉstatus,
} from "../../Runtime/Hosted/Network/Host-Network-Protocol.mjs";

const Tests = [];
const SERVER_SOCKETS = Symbol("server sockets");

function Test(Name, Body) { Tests.push({ name: Name, body: Body }); }

function Deadline(Milliseconds = 2_000) {
    return process.hrtime.bigint() + BigInt(Milliseconds) * 1_000_000n;
}

function Request(Operation, RequestId, ProviderGeneration, Values = {}) {
    return Encodeˉhostˉnetworkˉrequest({
        operation: Operation,
        requestId: BigInt(RequestId),
        providerGeneration: ProviderGeneration,
        deadlineNanoseconds: Values.deadline ?? Deadline(),
        connectionId: Values.connectionId ?? 0n,
        connectionGeneration: Values.connectionGeneration ?? 0n,
        control: Values.control ?? 0,
        service: Values.service ?? "",
        payload: Values.payload ?? Buffer.alloc(0),
    });
}

async function Handle(Provider, Bytes) {
    return Decodeˉhostˉnetworkˉresponse(await Provider.handle(Bytes));
}

async function Listen(Server) {
    if (!Server[SERVER_SOCKETS]) {
        Server[SERVER_SOCKETS] = new Set();
        Server.on("connection", Socket => {
            Server[SERVER_SOCKETS].add(Socket);
            Socket.on("close", () => Server[SERVER_SOCKETS].delete(Socket));
        });
    }
    Server.listen({ host: "127.0.0.1", port: 0 });
    await once(Server, "listening");
    return Server.address().port;
}

function Tlsˉserver(Fixture, Alpn = "http/1.1", Versions = {}) {
    const Server = tls.createServer({
        key: Fixture.key,
        cert: Fixture.cert,
        ALPNProtocols: [Alpn],
        minVersion: Versions.minimum ?? "TLSv1.3",
        maxVersion: Versions.maximum ?? "TLSv1.3",
        allowHalfOpen: true,
    });
    Server.on("tlsClientError", () => {});
    return Server;
}

function Provider(Port, Fixture, Values = {}) {
    const Trust = Canonicalˉtrustˉsnapshot([Fixture.ca]);
    return new Hostˉtlsˉprovider({
        service: Values.service ?? "localhost",
        port: Port,
        generation: Values.generation ?? 41n,
        trustGeneration: Values.trustGeneration ?? 7n,
        expectedTrustSha256: Values.expectedTrustSha256 ?? Trust.sha256,
        trustCertificates: Values.trustCertificates ?? [Fixture.ca],
        alpn: Values.alpn ?? "http/1.1",
        resolver: Values.resolver ?? (async () => [{ address: "127.0.0.1", family: 4 }]),
        maximumConnections: Values.maximumConnections ?? 1,
        maximumQueuedBytes: Values.maximumQueuedBytes ?? 1_024,
        maximumTransferBytes: Values.maximumTransferBytes ?? 4_096n,
        maximumOperationMilliseconds: Values.maximumOperationMilliseconds ?? 2_000,
        maximumLifetimeMilliseconds: Values.maximumLifetimeMilliseconds ?? 10_000,
    });
}

async function Closeˉserver(Server) {
    if (Server[SERVER_SOCKETS]) {
        for (const Socket of Server[SERVER_SOCKETS]) Socket.destroy();
    }
    await new Promise((Resolve, Reject) => {
        Server.close(Error => Error ? Reject(Error) : Resolve());
    });
}

Test("trust snapshot is canonical and order-sensitive", () => {
    const A = Createˉephemeralˉtlsˉfixture();
    const B = Createˉephemeralˉtlsˉfixture();
    const Bundled = Canonicalˉtrustˉsnapshot();
    const First = Canonicalˉtrustˉsnapshot([A.ca, B.ca]);
    assert.ok(Bundled.certificates.length >= 1);
    assert.match(Bundled.sha256, /^[0-9a-f]{64}$/);
    assert.equal(First.sha256, Canonicalˉtrustˉsnapshot([A.ca, B.ca]).sha256);
    assert.notEqual(First.sha256, Canonicalˉtrustˉsnapshot([B.ca, A.ca]).sha256);
    assert.match(First.sha256, /^[0-9a-f]{64}$/);
});

Test("configuration rejects invalid trust and ALPN bindings", () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Trust = Canonicalˉtrustˉsnapshot([Fixture.ca]);
    assert.throws(() => Provider(443, Fixture, { trustGeneration: 0n }));
    assert.throws(() => Provider(443, Fixture, { expectedTrustSha256: "0".repeat(64) }));
    assert.throws(() => Provider(443, Fixture, { alpn: "HTTP 1.1" }));
    assert.equal(Trust.certificates.length, 1);
});

Test("authority denial happens before resolution", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    let Resolved = false;
    const Value = Provider(443, Fixture, {
        resolver: async () => { Resolved = true; return []; },
    });
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 1, 41n, {
        service: "other.example", control: 443,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Unauthorized);
    assert.equal(Resolved, false);
    Value.teardown();
});

Test("TLS 1.3 verifies identity and exact ALPN before exposing bytes", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture);
    Server.on("secureConnection", Socket => {
        assert.equal(Socket.getProtocol(), "TLSv1.3");
        assert.equal(Socket.alpnProtocol, "http/1.1");
        Socket.on("data", Bytes => {
            assert.equal(Bytes.toString(), "ping");
            Socket.write("pong");
        });
        Socket.on("end", () => Socket.end("tail"));
    });
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture);
    const Connected = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 2, 41n, {
        service: "localhost", control: Port,
    }));
    assert.equal(Connected.status, Hostˉnetworkˉstatus.Valid);
    assert.equal(Connected.address, "127.0.0.1");
    const Identity = {
        connectionId: Connected.connectionId,
        connectionGeneration: Connected.connectionGeneration,
    };
    const Written = await Handle(Value, Request(Hostˉnetworkˉoperation.Write, 3, 41n, {
        ...Identity, payload: Buffer.from("ping"),
    }));
    assert.equal(Written.progress, 4n);
    const Read = await Handle(Value, Request(Hostˉnetworkˉoperation.Read, 4, 41n, {
        ...Identity, control: 4,
    }));
    assert.equal(Read.payload.toString(), "pong");
    const Shutdown = await Handle(Value, Request(Hostˉnetworkˉoperation.ShutdownWrite, 5, 41n, Identity));
    assert.equal(Shutdown.status, Hostˉnetworkˉstatus.Valid);
    const Tail = await Handle(Value, Request(Hostˉnetworkˉoperation.Read, 6, 41n, {
        ...Identity, control: 4,
    }));
    assert.equal(Tail.payload.toString(), "tail");
    const Closed = await Handle(Value, Request(Hostˉnetworkˉoperation.Close, 7, 41n, Identity));
    assert.equal(Closed.status, Hostˉnetworkˉstatus.Valid);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("wrong service identity is unavailable", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture("localhost");
    const Server = Tlsˉserver(Fixture);
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture, { service: "wrong.example" });
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 7, 41n, {
        service: "wrong.example", control: Port,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Unavailable);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("untrusted certificate is unavailable", async () => {
    const ServerFixture = Createˉephemeralˉtlsˉfixture();
    const OtherTrust = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(ServerFixture);
    const Port = await Listen(Server);
    const Value = Provider(Port, OtherTrust);
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 8, 41n, {
        service: "localhost", control: Port,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Unavailable);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("TLS 1.2 peer is unavailable", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture, "http/1.1", {
        minimum: "TLSv1.2", maximum: "TLSv1.2",
    });
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture);
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 9, 41n, {
        service: "localhost", control: Port,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Unavailable);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("ALPN mismatch is unavailable", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture, "h2");
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture);
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 10, 41n, {
        service: "localhost", control: Port,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Unavailable);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("plaintext peer is unavailable", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = net.createServer(Socket => Socket.end("plaintext"));
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture);
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 11, 41n, {
        service: "localhost", control: Port,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Unavailable);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("stalled handshake expires", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Sockets = new Set();
    const Server = net.createServer(Socket => {
        Sockets.add(Socket);
        Socket.on("close", () => Sockets.delete(Socket));
    });
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture, { maximumOperationMilliseconds: 200 });
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 12, 41n, {
        service: "localhost", control: Port, deadline: Deadline(50),
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Expired);
    Value.teardown();
    for (const Socket of Sockets) Socket.destroy();
    await Closeˉserver(Server);
});

Test("stale provider generation is rejected before handshake", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Value = Provider(443, Fixture);
    const Response = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 13, 40n, {
        service: "localhost", control: 443,
    }));
    assert.equal(Response.status, Hostˉnetworkˉstatus.Stale);
    Value.teardown();
});

Test("queue and transfer limits remain enforced over TLS", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture);
    Server.on("secureConnection", Socket => Socket.on("data", () => {}));
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture, { maximumQueuedBytes: 4, maximumTransferBytes: 4n });
    const Connected = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 14, 41n, {
        service: "localhost", control: Port,
    }));
    const Identity = {
        connectionId: Connected.connectionId,
        connectionGeneration: Connected.connectionGeneration,
    };
    const Written = await Handle(Value, Request(Hostˉnetworkˉoperation.Write, 15, 41n, {
        ...Identity, payload: Buffer.from("four"),
    }));
    assert.equal(Written.status, Hostˉnetworkˉstatus.Valid);
    const Limited = await Handle(Value, Request(Hostˉnetworkˉoperation.Write, 16, 41n, {
        ...Identity, payload: Buffer.from("x"),
    }));
    assert.equal(Limited.status, Hostˉnetworkˉstatus.Limit);
    Value.teardown();
    await Closeˉserver(Server);
});

Test("provider teardown invalidates secure connections", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture);
    const Port = await Listen(Server);
    const Value = Provider(Port, Fixture);
    const Connected = await Handle(Value, Request(Hostˉnetworkˉoperation.Connect, 17, 41n, {
        service: "localhost", control: Port,
    }));
    assert.equal(Connected.status, Hostˉnetworkˉstatus.Valid);
    Value.teardown();
    const Lost = await Handle(Value, Request(Hostˉnetworkˉoperation.Read, 18, 41n, {
        connectionId: Connected.connectionId,
        connectionGeneration: Connected.connectionGeneration,
        control: 1,
    }));
    assert.equal(Lost.status, Hostˉnetworkˉstatus.ProviderLost);
    await Closeˉserver(Server);
});

Test("supervised child performs a complete pinned TLS round-trip", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture);
    Server.on("secureConnection", Socket => Socket.once("data", Bytes => Socket.end(Bytes)));
    const Port = await Listen(Server);
    const Supervisor = new Hostˉtlsˉsupervisor({
        service: "localhost",
        port: Port,
        generation: 43n,
        trustGeneration: 9n,
        trustCertificates: [Fixture.ca],
        maximumConnections: 1,
        maximumQueuedBytes: 1_024,
        maximumTransferBytes: 4_096n,
        maximumOperationMilliseconds: 2_000,
        maximumLifetimeMilliseconds: 10_000,
    });
    const Connected = await Supervisor.connect("localhost", Port, 2_000);
    assert.equal(Connected.status, Hostˉnetworkˉstatus.Valid);
    const Payload = Buffer.from("supervised tls");
    const Written = await Supervisor.write(
        Connected.connectionId, Connected.connectionGeneration, Payload, 2_000,
    );
    assert.equal(Written.progress, BigInt(Payload.length));
    const Read = await Supervisor.read(
        Connected.connectionId, Connected.connectionGeneration, Payload.length, 2_000,
    );
    assert.deepEqual(Read.payload, Payload);
    await Supervisor.closeConnection(
        Connected.connectionId, Connected.connectionGeneration, 2_000,
    );
    await Supervisor.teardown();
    await Closeˉserver(Server);
});

Test("TLS fixture material is generated only in memory", () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    assert.match(Fixture.ca, /^-----BEGIN CERTIFICATE-----/);
    assert.match(Fixture.cert, /^-----BEGIN CERTIFICATE-----/);
    assert.match(Fixture.key, /^-----BEGIN PRIVATE KEY-----/);
    assert.equal(Object.keys(Fixture).sort().join(","), "ca,cert,key");
});

let Failed = false;
for (let Index = 0; Index < Tests.length; Index += 1) {
    const Value = Tests[Index];
    process.stdout.write(`step=host-tls item=${Index + 1}/${Tests.length}\n`);
    try { await Value.body(); } catch (Error) {
        Failed = true;
        process.stderr.write(`case=${Value.name} status=Failed\n${Error.stack ?? Error}\n`);
    }
}
if (Failed) process.exit(1);
process.stdout.write(
    `host tls provider status=Passed platforms=2 cases=${Tests.length} ` +
    "current-host=Verified public-network=0 credentials=0 tls=1.3\n",
);
