import assert from "node:assert/strict";
import tls from "node:tls";
import { once } from "node:events";
import { Createˉephemeralˉtlsˉfixture } from "./Ephemeral-Tls-Fixture.mjs";
import { Hostˉnetworkˉstatus } from "../../Runtime/Hosted/Network/Host-Network-Protocol.mjs";
import {
    Boundedˉhttp1ˉresponseˉdecoder,
    Buildˉboundedˉhttp1ˉrequest,
    Decodeˉboundedˉhttp1ˉresponse,
} from "../../Runtime/Hosted/Http/Bounded-Http1.mjs";
import { Boundedˉhttpsˉclient } from "../../Runtime/Hosted/Http/Bounded-Https-Client.mjs";

const Tests = [];
const SERVER_SOCKETS = Symbol("server sockets");

function Test(Name, Body) { Tests.push({ name: Name, body: Body }); }

function Response(Headers, Body = "", Status = "200 OK") {
    return Buffer.from(`HTTP/1.1 ${Status}\r\n${Headers.join("\r\n")}\r\n\r\n${Body}`, "ascii");
}

function Decode(Bytes, Values = {}) {
    return Decodeˉboundedˉhttp1ˉresponse(Buffer.from(Bytes), {
        maximumHeaderBytes: Values.maximumHeaderBytes ?? 1_024,
        maximumBodyBytes: Values.maximumBodyBytes ?? 1_024,
        maximumWireBytes: Values.maximumWireBytes ?? 4_096,
        peerClosed: Values.peerClosed ?? false,
    });
}

function Request(Values = {}) {
    return Buildˉboundedˉhttp1ˉrequest({
        method: Values.method ?? "GET",
        target: Values.target ?? "/v1/models",
        service: "localhost",
        port: Values.port ?? 443,
        headers: Values.headers ?? [{ name: "accept", value: "application/json" }],
        body: Values.body ?? Buffer.alloc(0),
        allowedTargets: new Set(["/v1/models", "/v1/responses"]),
        maximumRequestBytes: Values.maximumRequestBytes ?? 65_536,
    });
}

function Tlsˉserver(Fixture, Handler) {
    const Server = tls.createServer({
        key: Fixture.key,
        cert: Fixture.cert,
        ALPNProtocols: ["http/1.1"],
        minVersion: "TLSv1.3",
        maxVersion: "TLSv1.3",
        allowHalfOpen: true,
    });
    Server[SERVER_SOCKETS] = new Set();
    Server.on("connection", Socket => {
        Server[SERVER_SOCKETS].add(Socket);
        Socket.on("close", () => Server[SERVER_SOCKETS].delete(Socket));
    });
    Server.on("tlsClientError", () => {});
    Server.on("secureConnection", Socket => {
        let Bytes = Buffer.alloc(0);
        Socket.on("data", Chunk => {
            Bytes = Buffer.concat([Bytes, Chunk]);
            const HeaderEnd = Bytes.indexOf("\r\n\r\n", 0, "ascii");
            if (HeaderEnd < 0) return;
            const Match = /\r\nContent-Length: ([0-9]+)\r\n/i.exec(Bytes.subarray(0, HeaderEnd + 2).toString("ascii"));
            const Total = HeaderEnd + 4 + Number(Match?.[1] ?? 0);
            if (Bytes.length >= Total && !Socket.handled) {
                Socket.handled = true;
                Handler(Socket, Bytes.subarray(0, Total));
            }
        });
    });
    return Server;
}

async function Listen(Server) {
    Server.listen({ host: "127.0.0.1", port: 0 });
    await once(Server, "listening");
    return Server.address().port;
}

async function Closeˉserver(Server) {
    for (const Socket of Server[SERVER_SOCKETS]) Socket.destroy();
    await new Promise((Resolve, Reject) => Server.close(Error => Error ? Reject(Error) : Resolve()));
}

function Client(Port, Fixture, Values = {}) {
    return new Boundedˉhttpsˉclient({
        service: "localhost",
        port: Port,
        generation: Values.generation ?? 61n,
        trustGeneration: Values.trustGeneration ?? 11n,
        trustCertificates: [Fixture.ca],
        allowedTargets: new Set(["/v1/models", "/v1/responses"]),
        maximumRequestBytes: Values.maximumRequestBytes ?? 16_384,
        maximumHeaderBytes: Values.maximumHeaderBytes ?? 4_096,
        maximumBodyBytes: Values.maximumBodyBytes ?? 65_536,
        maximumWireBytes: Values.maximumWireBytes ?? 131_072,
        maximumOperationMilliseconds: Values.maximumOperationMilliseconds ?? 2_000,
        maximumLifetimeMilliseconds: 10_000,
        ...(Values.supervisorFactory ? { supervisorFactory: Values.supervisorFactory } : {}),
    });
}

Test("canonical GET request owns host and connection framing", () => {
    const Bytes = Request();
    assert.equal(Bytes.toString(),
        "GET /v1/models HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n" +
        "accept: application/json\r\n\r\n");
});

Test("canonical POST request owns exact content length", () => {
    const Bytes = Request({
        method: "POST",
        target: "/v1/responses",
        body: Buffer.from("{}"),
        headers: [
            { name: "accept", value: "application/json" },
            { name: "content-type", value: "application/json" },
        ],
    });
    assert.match(Bytes.toString(), /Content-Length: 2\r\n\r\n\{\}$/);
});

Test("request rejects a target outside the exact binding", () => {
    assert.throws(() => Request({ target: "/other" }), Error => Error.kind === "invalid_request");
});

Test("request rejects owned, duplicate, and injected headers", () => {
    for (const Headers of [
        [{ name: "host", value: "other" }],
        [{ name: "accept", value: "a" }, { name: "accept", value: "b" }],
        [{ name: "accept", value: "a\r\nX: b" }],
    ]) assert.throws(() => Request({ headers: Headers }));
});

Test("request rejects GET bodies and POST bodies without content type", () => {
    assert.throws(() => Request({ body: Buffer.from("x") }));
    assert.throws(() => Request({ method: "POST", target: "/v1/responses", body: Buffer.from("x") }));
});

Test("request enforces its complete byte limit", () => {
    assert.throws(() => Request({
        method: "POST", target: "/v1/responses", body: Buffer.alloc(200),
        headers: [{ name: "content-type", value: "application/json" }], maximumRequestBytes: 128,
    }), Error => Error.kind === "limit");
});

Test("content-length response decodes exactly", () => {
    const Result = Decode(Response(["Content-Length: 5", "Content-Type: text/plain"], "hello"));
    assert.equal(Result.status, 200);
    assert.equal(Result.body.toString(), "hello");
    assert.equal(Result.redirect, false);
});

Test("fragmented content-length response remains incremental", () => {
    const Bytes = Response(["Content-Length: 5"], "hello");
    const Decoder = new Boundedˉhttp1ˉresponseˉdecoder({
        maximumHeaderBytes: 1_024, maximumBodyBytes: 1_024, maximumWireBytes: 4_096,
    });
    let Result = null;
    for (const Byte of Bytes) Result = Decoder.push(Buffer.from([Byte]));
    assert.equal(Result.body.toString(), "hello");
});

Test("selected chunked response decodes fragmented chunks", () => {
    const Bytes = Response(["Transfer-Encoding: chunked"], "4\r\nWiki\r\n5\r\npedia\r\n0\r\n\r\n");
    const Decoder = new Boundedˉhttp1ˉresponseˉdecoder({
        maximumHeaderBytes: 1_024, maximumBodyBytes: 1_024, maximumWireBytes: 4_096,
    });
    let Result = null;
    for (const Byte of Bytes) Result = Decoder.push(Buffer.from([Byte]));
    assert.equal(Result.body.toString(), "Wikipedia");
});

Test("redirect is surfaced without being followed", () => {
    const Result = Decode(Response(["Content-Length: 0", "Location: https://other.example/"], "", "302 Found"));
    assert.equal(Result.redirect, true);
    assert.equal(Result.status, 302);
});

Test("content-length plus transfer-encoding is rejected", () => {
    assert.throws(() => Decode(Response(["Content-Length: 0", "Transfer-Encoding: chunked"], "0\r\n\r\n")),
        Error => Error.kind === "framing");
});

Test("duplicate response fields are rejected", () => {
    assert.throws(() => Decode(Response(["Content-Length: 0", "content-length: 0"])),
        Error => Error.kind === "framing");
});

Test("noncanonical content lengths are rejected", () => {
    for (const Length of ["00", "+1", "1, 1", "-1", "x"]) {
        assert.throws(() => Decode(Response([`Content-Length: ${Length}`], "x")));
    }
});

Test("unsupported transfer coding and missing length are rejected", () => {
    assert.throws(() => Decode(Response(["Transfer-Encoding: gzip"], "x")),
        Error => Error.kind === "unsupported");
    assert.throws(() => Decode(Response(["Content-Type: text/plain"], "x")),
        Error => Error.kind === "framing");
});

Test("truncated headers and bodies are distinct", () => {
    assert.throws(() => Decode("HTTP/1.1 200 OK\r\nContent-Length: 1\r\n", { peerClosed: true }),
        Error => Error.kind === "truncated");
    assert.throws(() => Decode(Response(["Content-Length: 2"], "x"), { peerClosed: true }),
        Error => Error.kind === "truncated");
});

Test("excess bytes after a framed body are rejected", () => {
    assert.throws(() => Decode(Response(["Content-Length: 1"], "xy")),
        Error => Error.kind === "framing");
});

Test("response body and wire limits are enforced", () => {
    assert.throws(() => Decode(Response(["Content-Length: 5"], "hello"), { maximumBodyBytes: 4 }),
        Error => Error.kind === "limit");
    const Decoder = new Boundedˉhttp1ˉresponseˉdecoder({
        maximumHeaderBytes: 32, maximumBodyBytes: 1, maximumWireBytes: 64,
    });
    assert.throws(() => Decoder.push(Buffer.alloc(65)), Error => Error.kind === "limit");
});

Test("oversized response headers are rejected before completion", () => {
    assert.throws(() => Decode(`HTTP/1.1 200 OK\r\nX: ${"a".repeat(100)}`, {
        maximumHeaderBytes: 64,
    }), Error => Error.kind === "limit");
});

Test("cookies, upgrades, and compression are rejected", () => {
    for (const Header of ["Set-Cookie: a=b", "Upgrade: websocket", "Content-Encoding: gzip"]) {
        assert.throws(() => Decode(Response(["Content-Length: 0", Header])),
            Error => Error.kind === "unsupported");
    }
});

Test("chunk extensions and trailers are rejected", () => {
    assert.throws(() => Decode(Response(["Transfer-Encoding: chunked"], "1;x=1\r\na\r\n0\r\n\r\n")));
    assert.throws(() => Decode(Response(["Transfer-Encoding: chunked"], "1\r\na\r\n0\r\nX: y\r\n\r\n")),
        Error => Error.kind === "unsupported");
});

Test("bodyless statuses accept only zero framing", () => {
    const Result = Decode(Response(["Content-Length: 0"], "", "204 No Content"));
    assert.equal(Result.body.length, 0);
    assert.throws(() => Decode(Response(["Content-Length: 1"], "x", "204 No Content")));
});

Test("informational and invalid statuses are rejected", () => {
    for (const Status of ["100 Continue", "199 Unknown", "600 Invalid"]) {
        assert.throws(() => Decode(Response(["Content-Length: 0"], "", Status)));
    }
});

Test("obs-fold, control bytes, and bare line feeds are rejected", () => {
    assert.throws(() => Decode("HTTP/1.1 200 OK\r\n X: y\r\nContent-Length: 0\r\n\r\n"));
    assert.throws(() => Decode("HTTP/1.1 200 OK\r\nX: a\u0001b\r\nContent-Length: 0\r\n\r\n"));
    assert.throws(() => Decode("HTTP/1.1 200 OK\nContent-Length: 0\r\n\r\n"));
});

Test("real TLS peer returns a fragmented bounded GET response", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture, (Socket, Bytes) => {
        assert.match(Bytes.toString(), /^GET \/v1\/models HTTP\/1\.1\r\n/);
        const Reply = Response(["Content-Length: 11", "Content-Type: application/json"], "{\"ok\":true}");
        Socket.write(Reply.subarray(0, 17));
        setTimeout(() => Socket.end(Reply.subarray(17)), 5);
    });
    const Port = await Listen(Server);
    const Result = await Client(Port, Fixture).request({ method: "GET", target: "/v1/models" });
    assert.equal(Result.body.toString(), "{\"ok\":true}");
    assert.equal(Result.endpointAddress, "127.0.0.1");
    await Closeˉserver(Server);
});

Test("real TLS peer receives one exact bounded POST", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    let Requests = 0;
    const Server = Tlsˉserver(Fixture, (Socket, Bytes) => {
        Requests += 1;
        assert.match(Bytes.toString(), /^POST \/v1\/responses HTTP\/1\.1\r\n/);
        assert.match(Bytes.toString(), /Content-Length: 7\r\n\r\n\{\"x\":1\}$/);
        Socket.end(Response(["Content-Length: 2"], "ok"));
    });
    const Port = await Listen(Server);
    const Result = await Client(Port, Fixture).request({
        method: "POST",
        target: "/v1/responses",
        headers: [{ name: "content-type", value: "application/json" }],
        body: Buffer.from("{\"x\":1}"),
    });
    assert.equal(Result.body.toString(), "ok");
    assert.equal(Requests, 1);
    await Closeˉserver(Server);
});

Test("real redirect response causes no second connection", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    let Requests = 0;
    const Server = Tlsˉserver(Fixture, Socket => {
        Requests += 1;
        Socket.end(Response(["Content-Length: 0", "Location: https://other.example/"], "", "307 Temporary Redirect"));
    });
    const Port = await Listen(Server);
    const Result = await Client(Port, Fixture).request({ method: "GET", target: "/v1/models" });
    assert.equal(Result.redirect, true);
    assert.equal(Requests, 1);
    await Closeˉserver(Server);
});

Test("excess bytes in a later TLS record are rejected", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    const Server = Tlsˉserver(Fixture, Socket => {
        Socket.write(Response(["Content-Length: 2"], "ok"));
        setTimeout(() => Socket.end("x"), 5);
    });
    const Port = await Listen(Server);
    await assert.rejects(
        Client(Port, Fixture).request({ method: "GET", target: "/v1/models" }),
        Error => Error.kind === "framing",
    );
    await Closeˉserver(Server);
});

Test("stalled HTTPS response expires without retry", async () => {
    const Fixture = Createˉephemeralˉtlsˉfixture();
    let Requests = 0;
    const Server = Tlsˉserver(Fixture, () => { Requests += 1; });
    const Port = await Listen(Server);
    await assert.rejects(
        Client(Port, Fixture, { maximumOperationMilliseconds: 1_000 }).request({
            method: "GET", target: "/v1/models", timeoutMilliseconds: 500,
        }),
        Error => Error.kind === "deadline",
    );
    assert.equal(Requests, 1);
    await Closeˉserver(Server);
});

Test("partial local request acceptance is indeterminate and not replayed", async () => {
    let Writes = 0;
    let Teardowns = 0;
    const Factory = () => ({
        connect: async () => ({
            status: Hostˉnetworkˉstatus.Valid,
            providerGeneration: 61n,
            connectionId: 1n,
            connectionGeneration: 1n,
            address: "127.0.0.1",
            endpointPort: 443,
        }),
        write: async () => { Writes += 1; return { status: Hostˉnetworkˉstatus.Valid, progress: 1n }; },
        read: async () => assert.fail("partial request must not read"),
        closeConnection: async () => ({ status: Hostˉnetworkˉstatus.Valid }),
        teardown: async () => { Teardowns += 1; },
    });
    const Fixture = Createˉephemeralˉtlsˉfixture();
    await assert.rejects(
        Client(443, Fixture, { supervisorFactory: Factory }).request({
            method: "GET", target: "/v1/models",
        }),
        Error => Error.kind === "submission_indeterminate",
    );
    assert.equal(Writes, 1);
    assert.equal(Teardowns, 1);
});

let Failed = false;
for (let Index = 0; Index < Tests.length; Index += 1) {
    const Value = Tests[Index];
    process.stdout.write(`step=bounded-https item=${Index + 1}/${Tests.length}\n`);
    try { await Value.body(); } catch (Error) {
        Failed = true;
        process.stderr.write(`case=${Value.name} status=Failed\n${Error.stack ?? Error}\n`);
    }
}
if (Failed) process.exit(1);
process.stdout.write(
    `bounded https status=Passed platforms=2 cases=${Tests.length} ` +
    "current-host=Verified public-network=0 credentials=0 redirects-followed=0\n",
);
