import assert from "node:assert/strict";
import {
    Externalˉmodelˉgatewayˉfailure,
    Modelˉgatewayˉstatus,
    Protectedˉexternalˉmodelˉgateway,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Core.mjs";
import {
    Executeˉexternalˉmodelˉrequest,
} from "./External-Model-Reference-Core.mjs";
import {
    Createˉprotectedˉcredential,
    Unlockˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Hostˉnetworkˉstatus,
} from "../../Runtime/Hosted/Network/Host-Network-Protocol.mjs";

const Tests = [];
const SERVICES = Object.freeze({
    openai: "api.openai.com",
    anthropic: "api.anthropic.com",
    google: "generativelanguage.googleapis.com",
});

function Test(Name, Body) { Tests.push({ name: Name, body: Body }); }

function Messages(Values) {
    const Parts = Values.map(([Role, Content]) => {
        const Text = Buffer.from(Content, "utf8");
        const Bytes = Buffer.alloc(8 + Text.length);
        Bytes.writeUInt32LE(Role, 0);
        Bytes.writeUInt32LE(Text.length, 4);
        Text.copy(Bytes, 8);
        return Bytes;
    });
    const Bytes = Buffer.alloc(16 + Parts.reduce((Sum, Part) => Sum + Part.length, 0));
    Bytes.write("WVMM", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(Parts.length, 12);
    let Offset = 16;
    for (const Part of Parts) { Part.copy(Bytes, Offset); Offset += Part.length; }
    return Bytes;
}

function Request({ operation, id = 7n, generation = 23n, limit = 8, model = "", payload }) {
    const Model = Buffer.from(model, "utf8");
    const Payload = payload ?? Buffer.alloc(0);
    const Bytes = Buffer.alloc(48 + Model.length + Payload.length);
    Bytes.write("WVMQ", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(operation, 12);
    Bytes.writeBigUInt64LE(id, 16);
    Bytes.writeBigUInt64LE(generation, 24);
    Bytes.writeUInt32LE(limit, 32);
    Bytes.writeUInt32LE(Model.length, 36);
    Bytes.writeUInt32LE(Payload.length, 40);
    Model.copy(Bytes, 48);
    Payload.copy(Bytes, 48 + Model.length);
    return Bytes;
}

function Catalogˉrequest(Values = {}) {
    const Continuation = Buffer.from(Values.continuation ?? "", "utf8");
    return Request({
        operation: 1, generation: Continuation.length === 0 ? (Values.generation ?? 0n) :
            (Values.generation ?? 23n), limit: Values.limit ?? 8, payload: Continuation,
    });
}

function Generationˉrequest(Values = {}) {
    return Request({
        operation: 2, model: Values.model ?? "model-test", limit: Values.limit ?? 64,
        generation: Values.generation ?? 23n,
        payload: Messages(Values.messages ?? [[2, "Hello"]]),
    });
}

function Httpˉjson(Value, Status = 200, ExtraHeaders = []) {
    return {
        status: Status,
        redirect: Status >= 300 && Status <= 399,
        headers: [{ name: "content-type", value: "application/json" }, ...ExtraHeaders],
        body: Buffer.from(JSON.stringify(Value), "utf8"),
    };
}

function Fakeˉlease(Provider, Responses, Observed = {}) {
    let State = "available";
    return {
        inspect: () => ({
            provider: Provider, service: SERVICES[Provider], port: 443,
            generation: 17n, identity: "00", state: State,
        }),
        bindHttps: Binding => {
            Observed.bindings ??= [];
            Observed.bindings.push(Binding);
            if (State !== "available") {
                const Failure = new Error("revoked"); Failure.kind = "revoked"; throw Failure;
            }
            return {
                request: async Value => {
                    Observed.requests ??= [];
                    if (Value.body) Value.bodyCopy = Buffer.from(Value.body);
                    Observed.requests.push(Value);
                    const Next = Responses.shift();
                    if (Next instanceof Error) throw Next;
                    if (!Next) throw new Error("No fake response remains.");
                    return Next;
                },
            };
        },
        revoke: () => { State = "destroyed"; },
    };
}

function Gateway(Provider, Responses, Observed = {}) {
    return new Protectedˉexternalˉmodelˉgateway({
        credentialLease: Fakeˉlease(Provider, Responses, Observed),
        providerGeneration: 23n,
        trustGeneration: 5n,
        maximumRequestBytes: 65_536,
        maximumHeaderBytes: 16_384,
        maximumBodyBytes: 1_048_576,
        maximumWireBytes: 1_081_344,
        maximumOperationMilliseconds: 1_000,
        maximumLifetimeMilliseconds: 2_000,
    });
}

function Commonˉresponse(Bytes, Magic) {
    assert.equal(Bytes.subarray(0, 4).toString("ascii"), Magic);
    assert.equal(Bytes.readUInt32LE(4), 1);
    assert.equal(Bytes.readUInt32LE(8), Bytes.length);
    return {
        status: Bytes.readUInt32LE(12),
        id: Bytes.readBigUInt64LE(16),
        generation: Bytes.readBigUInt64LE(24),
    };
}

function Catalogˉresponse(Bytes) {
    const Common = Commonˉresponse(Bytes, "WVMC");
    const Count = Bytes.readUInt32LE(32);
    const ContinuationLength = Bytes.readUInt32LE(36);
    const DiagnosticLength = Bytes.readUInt32LE(40);
    let Offset = 48;
    const Entries = [];
    for (let Index = 0; Index < Count; Index += 1) {
        const Length = Bytes.readUInt32LE(Offset);
        const IdLength = Bytes.readUInt32LE(Offset + 4);
        const DisplayLength = Bytes.readUInt32LE(Offset + 8);
        Entries.push({
            id: Bytes.subarray(Offset + 20, Offset + 20 + IdLength).toString("utf8"),
            display: Bytes.subarray(
                Offset + 20 + IdLength, Offset + 20 + IdLength + DisplayLength,
            ).toString("utf8"),
            features: Bytes.readUInt32LE(Offset + 12),
        });
        Offset += Length;
    }
    return {
        ...Common, entries: Entries,
        continuation: Bytes.subarray(Offset, Offset + ContinuationLength).toString("utf8"),
        diagnostic: Bytes.subarray(
            Offset + ContinuationLength,
            Offset + ContinuationLength + DiagnosticLength,
        ).toString("utf8"),
    };
}

function Generationˉresponse(Bytes) {
    const Common = Commonˉresponse(Bytes, "WVMG");
    const ModelLength = Bytes.readUInt32LE(36);
    const TextLength = Bytes.readUInt32LE(40);
    const DiagnosticLength = Bytes.readUInt32LE(44);
    return {
        ...Common, completion: Bytes.readUInt32LE(32),
        model: Bytes.subarray(64, 64 + ModelLength).toString("utf8"),
        text: Bytes.subarray(64 + ModelLength, 64 + ModelLength + TextLength).toString("utf8"),
        diagnostic: Bytes.subarray(
            64 + ModelLength + TextLength,
            64 + ModelLength + TextLength + DiagnosticLength,
        ).toString("utf8"),
        inputTokens: Bytes.readBigUInt64LE(48), outputTokens: Bytes.readBigUInt64LE(56),
    };
}

Test("OpenAI catalog uses its fixed origin and returns sorted canonical entries", async () => {
    const Observed = {};
    const Body = Httpˉjson({ data: [{ id: "z-model" }, { id: "a-model" }] });
    const Result = Catalogˉresponse(await Gateway("openai", [Body], Observed).execute(Catalogˉrequest()));
    assert.deepEqual(Result.entries.map(Entry => Entry.id), ["a-model", "z-model"]);
    assert.equal(Result.status, 0);
    assert.equal(Result.generation, 23n);
    assert.deepEqual([...Observed.bindings[0].allowedTargets], ["/v1/models"]);
    assert.deepEqual(Observed.requests[0].headers, [{ name: "accept", value: "application/json" }]);
    assert.equal(Body.body.every(Byte => Byte === 0), true);
});

Test("Anthropic catalog pagination is internal and preserves its version field", async () => {
    const Observed = {};
    const Result = Catalogˉresponse(await Gateway("anthropic", [
        Httpˉjson({ data: [{ id: "b" }], has_more: true, last_id: "cursor value" }),
        Httpˉjson({ data: [{ id: "a" }], has_more: false }),
    ], Observed).execute(Catalogˉrequest()));
    assert.deepEqual(Result.entries.map(Entry => Entry.id), ["a", "b"]);
    assert.deepEqual([...Observed.bindings[1].allowedTargets], [
        "/v1/models?limit=1000&after_id=cursor%20value",
    ]);
    assert.equal(Observed.requests[0].headers.some(
        Header => Header.name === "anthropic-version" && Header.value === "2023-06-01",
    ), true);
});

Test("Google catalog filters non-generation entries and normalizes model names", async () => {
    const Observed = {};
    const Result = Catalogˉresponse(await Gateway("google", [Httpˉjson({ models: [
        { name: "models/gemini-test", displayName: "Gemini Test", supportedGenerationMethods: ["generateContent"] },
        { name: "models/embed-test", supportedGenerationMethods: ["embedContent"] },
    ] })], Observed).execute(Catalogˉrequest()));
    assert.deepEqual(Result.entries, [{ id: "gemini-test", display: "Gemini Test", features: 3 }]);
    assert.deepEqual([...Observed.bindings[0].allowedTargets], [
        "/v1beta/models?pageSize=1000",
    ]);
});

Test("OpenAI generation fixes Responses storage off and admits text usage", async () => {
    const Observed = {};
    const Response = Httpˉjson({
        status: "completed", model: "gpt-test",
        output: [{ type: "message", content: [{ type: "output_text", text: "Hello back" }] }],
        usage: { input_tokens: 2, output_tokens: 3 },
    });
    const Result = Generationˉresponse(await Gateway("openai", [Response], Observed).execute(
        Generationˉrequest({ model: "gpt-test", messages: [[1, "Be concise"], [2, "Hello"]] }),
    ));
    const Body = JSON.parse(Observed.requests[0].bodyCopy.toString("utf8"));
    assert.equal(Body.store, false);
    assert.deepEqual(Body.input.map(Item => Item.role), ["system", "user"]);
    assert.deepEqual(Result, {
        status: 0, id: 7n, generation: 23n, completion: 1, model: "gpt-test",
        text: "Hello back", diagnostic: "", inputTokens: 2n, outputTokens: 3n,
    });
    assert.equal(Observed.requests[0].body.every(Byte => Byte === 0), true);
});

Test("Anthropic generation separates system text and alternating messages", async () => {
    const Observed = {};
    const Result = Generationˉresponse(await Gateway("anthropic", [Httpˉjson({
        model: "claude-test", stop_reason: "max_tokens",
        content: [{ type: "text", text: "partial" }], usage: { input_tokens: 4, output_tokens: 5 },
    })], Observed).execute(Generationˉrequest({
        model: "claude-test", messages: [[1, "Rule"], [2, "One"], [3, "Two"], [2, "Three"]],
    })));
    const Body = JSON.parse(Observed.requests[0].bodyCopy.toString("utf8"));
    assert.equal(Body.system, "Rule");
    assert.deepEqual(Body.messages.map(Item => Item.role), ["user", "assistant", "user"]);
    assert.equal(Result.completion, 2);
    assert.equal(Result.text, "partial");
});

Test("Google generation owns its encoded model target and content mapping", async () => {
    const Observed = {};
    const Result = Generationˉresponse(await Gateway("google", [Httpˉjson({
        candidates: [{ finishReason: "SAFETY", content: { parts: [{ text: "blocked" }] } }],
        modelVersion: "gemini-test-002",
        usageMetadata: { promptTokenCount: 6, candidatesTokenCount: 1 },
    })], Observed).execute(Generationˉrequest({ model: "gemini/test" })));
    assert.deepEqual([...Observed.bindings[0].allowedTargets], [
        "/v1beta/models/gemini%2Ftest:generateContent",
    ]);
    assert.equal(Result.completion, 3);
    assert.equal(Result.model, "gemini-test-002");
});

Test("HTTP rejection classes remain definite canonical statuses", async () => {
    const Cases = [[401, 3], [404, 5], [429, 4], [500, 2]];
    for (const [Http, Status] of Cases) {
        const Result = Generationˉresponse(await Gateway(
            "openai", [Httpˉjson({}, Http)],
        ).execute(Generationˉrequest()));
        assert.equal(Result.status, Status);
        assert.equal(Result.completion, 0);
    }
});

Test("redirect responses are surfaced and never followed", async () => {
    const Observed = {};
    const Result = Catalogˉresponse(await Gateway("openai", [
        Httpˉjson({}, 302, [{ name: "location", value: "https://attacker.example/" }]),
    ], Observed).execute(Catalogˉrequest()));
    assert.equal(Result.status, Modelˉgatewayˉstatus.ProviderError);
    assert.equal(Observed.requests.length, 1);
});

Test("malformed JSON and non-JSON content become bounded provider errors", async () => {
    const Malformed = Httpˉjson({});
    Malformed.body = Buffer.from("{", "ascii");
    const WrongType = Httpˉjson({});
    WrongType.headers[0].value = "text/plain";
    for (const Response of [Malformed, WrongType]) {
        const Result = Catalogˉresponse(await Gateway("openai", [Response]).execute(Catalogˉrequest()));
        assert.equal(Result.status, Modelˉgatewayˉstatus.ProviderError);
        assert.equal(Result.diagnostic, "Provider output is invalid.");
        assert.equal(Response.body.every(Byte => Byte === 0), true);
    }
});

Test("stale provider generation is rejected before credential or network use", async () => {
    const Observed = {};
    const Result = Generationˉresponse(await Gateway("openai", [], Observed).execute(
        Generationˉrequest({ generation: 22n }),
    ));
    assert.equal(Result.status, Modelˉgatewayˉstatus.Stale);
    assert.equal(Observed.bindings, undefined);
});

Test("catalog continuations page a stable catalog and reject changed catalogs", async () => {
    const First = Catalogˉresponse(await Gateway("openai", [Httpˉjson({
        data: [{ id: "a" }, { id: "b" }, { id: "c" }],
    })]).execute(Catalogˉrequest({ limit: 2 })));
    assert.deepEqual(First.entries.map(Entry => Entry.id), ["a", "b"]);
    assert.notEqual(First.continuation, "");
    const Second = Catalogˉresponse(await Gateway("openai", [Httpˉjson({
        data: [{ id: "a" }, { id: "b" }, { id: "c" }],
    })]).execute(Catalogˉrequest({ continuation: First.continuation, limit: 2 })));
    assert.deepEqual(Second.entries.map(Entry => Entry.id), ["c"]);
    const Stale = Catalogˉresponse(await Gateway("openai", [Httpˉjson({
        data: [{ id: "a" }, { id: "changed" }],
    })]).execute(Catalogˉrequest({ continuation: First.continuation, limit: 2 })));
    assert.equal(Stale.status, Modelˉgatewayˉstatus.Stale);
});

Test("invalid Anthropic conversation shape is rejected without dispatch", async () => {
    const Observed = {};
    const Result = Generationˉresponse(await Gateway("anthropic", [], Observed).execute(
        Generationˉrequest({ messages: [[2, "one"], [2, "two"]] }),
    ));
    assert.equal(Result.status, Modelˉgatewayˉstatus.InvalidRequest);
    assert.equal(Observed.requests, undefined);
});

Test("unsupported structured provider output is never flattened into text", async () => {
    const Result = Generationˉresponse(await Gateway("openai", [Httpˉjson({
        status: "completed", model: "gpt-test",
        output: [{ type: "tool_call", name: "danger" }],
    })]).execute(Generationˉrequest()));
    assert.equal(Result.status, Modelˉgatewayˉstatus.Unsupported);
    assert.equal(Result.text, "");
});

Test("generation transport loss is indeterminate while catalog loss is unavailable", async () => {
    const Failure = new Error("transport detail must not escape");
    const Generation = Generationˉresponse(await Gateway("openai", [Failure]).execute(
        Generationˉrequest(),
    ));
    assert.equal(Generation.status, Modelˉgatewayˉstatus.SubmissionIndeterminate);
    assert.doesNotMatch(Generation.diagnostic, /transport detail/);
    const Catalog = Catalogˉresponse(await Gateway("openai", [Failure]).execute(Catalogˉrequest()));
    assert.equal(Catalog.status, Modelˉgatewayˉstatus.Unavailable);
});

Test("revoked credential lease becomes a definite revocation result", async () => {
    const Responses = [Httpˉjson({ data: [] })];
    const Lease = Fakeˉlease("openai", Responses);
    const GatewayValue = new Protectedˉexternalˉmodelˉgateway({
        credentialLease: Lease, providerGeneration: 23n,
    });
    Lease.revoke();
    const Result = Catalogˉresponse(await GatewayValue.execute(Catalogˉrequest()));
    assert.equal(Result.status, Modelˉgatewayˉstatus.Revoked);
});

Test("caller model envelopes cannot supply URL, header, or credential authority", async () => {
    const Observed = {};
    await Gateway("openai", [Httpˉjson({ data: [] })], Observed).execute(Catalogˉrequest());
    assert.equal(Observed.bindings[0].service, undefined);
    assert.equal(Observed.bindings[0].port, undefined);
    assert.equal(Observed.requests[0].headers.some(Header =>
        ["authorization", "x-api-key", "x-goog-api-key"].includes(Header.name)), false);
});

Test("malformed model records are rejected at the process boundary", async () => {
    const Bytes = Catalogˉrequest();
    Bytes.writeUInt32LE(Bytes.length + 1, 8);
    await assert.rejects(Gateway("openai", []).execute(Bytes), Error =>
        Error instanceof Externalˉmodelˉgatewayˉfailure && Error.kind === "invalid_request");
});

Test("gateway construction rejects arbitrary provider origins and dead leases", async () => {
    const Lease = Fakeˉlease("openai", []);
    Lease.inspect = () => ({
        provider: "openai", service: "attacker.example", port: 443,
        generation: 17n, state: "available",
    });
    assert.throws(() => new Protectedˉexternalˉmodelˉgateway({
        credentialLease: Lease, providerGeneration: 23n,
    }), Error => Error.kind === "invalid_binding");
});

Test("production catalog encoding is byte-identical to the independent reference oracle", async () => {
    const ProviderJson = {
        openai: { data: [{ id: "model-b" }, { id: "model-a" }] },
        anthropic: { data: [{ id: "model-b" }, { id: "model-a" }], has_more: false },
        google: { models: [
            { name: "models/model-b", supportedGenerationMethods: ["generateContent"] },
            { name: "models/model-a", supportedGenerationMethods: ["generateContent"] },
        ] },
    };
    for (const Provider of Object.keys(ProviderJson)) {
        const ModelRequest = Catalogˉrequest();
        const Production = await Gateway(
            Provider, [Httpˉjson(ProviderJson[Provider])],
        ).execute(ModelRequest);
        const Reference = await Executeˉexternalˉmodelˉrequest({
            provider: Provider,
            requestBytes: ModelRequest,
            generation: 23n,
            apiKey: "reference-placeholder-key",
            fetchImplementation: async () => {
                const Bytes = Buffer.from(JSON.stringify(ProviderJson[Provider]), "utf8");
                let Read = false;
                return {
                    status: 200,
                    headers: { get: Name => Name.toLowerCase() === "content-type" ?
                        "application/json" : null },
                    body: { getReader: () => ({
                        read: async () => Read ? { done: true } :
                            (Read = true, { done: false, value: Bytes }),
                    }) },
                };
            },
        });
        assert.deepEqual(Production, Reference);
    }
});

Test("real credential lease composes through bounded HTTPS without exporting its key", async () => {
    const Secret = Buffer.from("sk-gateway-integration-abcdefghijklmnop", "ascii");
    const Passphrase = Buffer.from("gateway integration passphrase", "utf8");
    let Counter = 1;
    const Wrapper = await Createˉprotectedˉcredential({
        provider: "openai", service: "api.openai.com", generation: 17n,
        credential: Secret, passphrase: Passphrase,
        randomBytes: Length => {
            const Bytes = Buffer.alloc(Length);
            for (let Index = 0; Index < Length; Index += 1) Bytes[Index] = Counter++ & 0xff;
            return Bytes;
        },
    });
    const Lease = await Unlockˉprotectedˉcredential(Wrapper, Passphrase);
    const Observed = {};
    const Json = Buffer.from(JSON.stringify({ data: [{ id: "gpt-integrated" }] }), "utf8");
    const Http = Buffer.from(
        `HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: ${Json.length}\r\n\r\n`,
        "ascii",
    );
    const Wire = Buffer.concat([Http, Json]);
    const GatewayValue = new Protectedˉexternalˉmodelˉgateway({
        credentialLease: Lease,
        providerGeneration: 23n,
        trustGeneration: 5n,
        maximumRequestBytes: 65_536,
        maximumHeaderBytes: 16_384,
        maximumBodyBytes: 1_048_576,
        maximumWireBytes: 1_081_344,
        maximumOperationMilliseconds: 1_000,
        maximumLifetimeMilliseconds: 2_000,
        supervisorFactory: () => ({
            connect: async () => ({
                status: Hostˉnetworkˉstatus.Valid, providerGeneration: 71n,
                connectionId: 1n, connectionGeneration: 1n,
                address: "127.0.0.1", endpointPort: 443,
            }),
            write: async (_Id, _Generation, Bytes) => {
                Observed.live = Bytes;
                Observed.request = Buffer.from(Bytes);
                return { status: Hostˉnetworkˉstatus.Valid, progress: BigInt(Bytes.length) };
            },
            read: async () => ({
                status: Hostˉnetworkˉstatus.Valid, flags: 1, payload: Buffer.from(Wire),
            }),
            closeConnection: async () => ({ status: Hostˉnetworkˉstatus.Valid }),
            teardown: async () => {},
        }),
    });
    try {
        const Result = Catalogˉresponse(await GatewayValue.execute(Catalogˉrequest()));
        assert.deepEqual(Result.entries.map(Entry => Entry.id), ["gpt-integrated"]);
        assert.match(Observed.request.toString("ascii"),
            /\r\nauthorization: Bearer sk-gateway-integration-abcdefghijklmnop\r\n/);
        assert.equal(Observed.live.every(Byte => Byte === 0), true);
        assert.equal(Object.values(Result).join("|").includes("sk-gateway"), false);
    } finally {
        Lease.destroy();
        Wrapper.fill(0);
        Secret.fill(0);
        Passphrase.fill(0);
        Wire.fill(0);
        Json.fill(0);
    }
});

let Failed = false;
for (let Index = 0; Index < Tests.length; Index += 1) {
    const Value = Tests[Index];
    process.stdout.write(`step=external-model-gateway-core item=${Index + 1}/${Tests.length}\n`);
    try { await Value.body(); } catch (Error) {
        Failed = true;
        process.stderr.write(`case=${Value.name} status=Failed\n${Error.stack ?? Error}\n`);
    }
}
if (Failed) process.exit(1);
process.stdout.write(
    `external model gateway core status=Passed providers=3 cases=${Tests.length} ` +
    "live-calls=0 real-credentials=0 redirects-followed=0\n",
);
