import assert from "node:assert/strict";
import {
    Decodeˉmodelˉrequest,
    Executeˉexternalˉmodelˉrequest,
    STATUS,
} from "./External-Model-Reference-Core.mjs";

function Messages(Values) {
    const Parts = Values.map(([Role, Text]) => {
        const Content = Buffer.from(Text, "utf8");
        const Part = Buffer.alloc(8 + Content.length);
        Part.writeUInt32LE(Role, 0);
        Part.writeUInt32LE(Content.length, 4);
        Content.copy(Part, 8);
        return Part;
    });
    const Total = 16 + Parts.reduce((Sum, Part) => Sum + Part.length, 0);
    const Bytes = Buffer.alloc(Total);
    Bytes.write("WVMM", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Total, 8);
    Bytes.writeUInt32LE(Parts.length, 12);
    let Offset = 16;
    for (const Part of Parts) {
        Part.copy(Bytes, Offset);
        Offset += Part.length;
    }
    return Bytes;
}

function Request({ operation = 1, requestId = 7n, generation = 0n, limit = 2, model = "", payload = Buffer.alloc(0) } = {}) {
    const Model = Buffer.from(model, "utf8");
    const Payload = Buffer.from(payload);
    const Bytes = Buffer.alloc(48 + Model.length + Payload.length);
    Bytes.write("WVMQ", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(operation, 12);
    Bytes.writeBigUInt64LE(requestId, 16);
    Bytes.writeBigUInt64LE(generation, 24);
    Bytes.writeUInt32LE(limit, 32);
    Bytes.writeUInt32LE(Model.length, 36);
    Bytes.writeUInt32LE(Payload.length, 40);
    Model.copy(Bytes, 48);
    Payload.copy(Bytes, 48 + Model.length);
    return Bytes;
}

function Jsonˉresponse(Value, Status = 200) {
    return new Response(JSON.stringify(Value), {
        status: Status,
        headers: { "content-type": "application/json; charset=utf-8" },
    });
}

function Catalogˉresponse(Bytes) {
    assert.equal(Bytes.subarray(0, 4).toString("ascii"), "WVMC");
    const Count = Bytes.readUInt32LE(32);
    const ContinuationLength = Bytes.readUInt32LE(36);
    const DiagnosticLength = Bytes.readUInt32LE(40);
    let Offset = 48;
    const Entries = [];
    for (let Index = 0; Index < Count; Index += 1) {
        const EntryLength = Bytes.readUInt32LE(Offset);
        const IdLength = Bytes.readUInt32LE(Offset + 4);
        const DisplayLength = Bytes.readUInt32LE(Offset + 8);
        Entries.push({
            id: Bytes.subarray(Offset + 20, Offset + 20 + IdLength).toString("utf8"),
            display: Bytes.subarray(Offset + 20 + IdLength, Offset + 20 + IdLength + DisplayLength).toString("utf8"),
            features: Bytes.readUInt32LE(Offset + 12),
            lifecycle: Bytes.readUInt32LE(Offset + 16),
        });
        Offset += EntryLength;
    }
    return {
        status: Bytes.readUInt32LE(12), requestId: Bytes.readBigUInt64LE(16),
        generation: Bytes.readBigUInt64LE(24), entries: Entries,
        continuation: Bytes.subarray(Offset, Offset + ContinuationLength).toString("utf8"),
        diagnostic: Bytes.subarray(Offset + ContinuationLength, Offset + ContinuationLength + DiagnosticLength).toString("utf8"),
    };
}

function Generationˉresponse(Bytes) {
    assert.equal(Bytes.subarray(0, 4).toString("ascii"), "WVMG");
    const ModelLength = Bytes.readUInt32LE(36);
    const TextLength = Bytes.readUInt32LE(40);
    const DiagnosticLength = Bytes.readUInt32LE(44);
    return {
        status: Bytes.readUInt32LE(12), requestId: Bytes.readBigUInt64LE(16),
        generation: Bytes.readBigUInt64LE(24), completion: Bytes.readUInt32LE(32),
        model: Bytes.subarray(64, 64 + ModelLength).toString("utf8"),
        text: Bytes.subarray(64 + ModelLength, 64 + ModelLength + TextLength).toString("utf8"),
        diagnostic: Bytes.subarray(64 + ModelLength + TextLength,
            64 + ModelLength + TextLength + DiagnosticLength).toString("utf8"),
        inputTokens: Bytes.readBigUInt64LE(48), outputTokens: Bytes.readBigUInt64LE(56),
    };
}

function Generateˉrequest(ProviderModel = "test-model", Values = [[2, "Hello"]]) {
    return Request({
        operation: 2, generation: 9n, limit: 64, model: ProviderModel, payload: Messages(Values),
    });
}

async function Execute(Provider, Bytes, Fetch, Extra = {}) {
    return Executeˉexternalˉmodelˉrequest({
        provider: Provider, requestBytes: Bytes, generation: 9n, apiKey: "test-secret",
        fetchImplementation: Fetch, timeoutMilliseconds: 5_000, ...Extra,
    });
}

const Tests = [
    ["decode catalog", async () => {
        const Value = Decodeˉmodelˉrequest(Request());
        assert.deepEqual({ operation: Value.operation, id: Value.requestId, limit: Value.limit },
            { operation: "catalog", id: 7n, limit: 2 });
    }],
    ["decode generation", async () => {
        const Value = Decodeˉmodelˉrequest(Generateˉrequest());
        assert.equal(Value.messages[0].content, "Hello");
    }],
    ["reject malformed request", async () => {
        const Bytes = Request();
        Bytes.writeUInt32LE(Bytes.length + 1, 8);
        assert.throws(() => Decodeˉmodelˉrequest(Bytes), /malformed/);
    }],
    ["missing credential", async () => {
        const Bytes = await Executeˉexternalˉmodelˉrequest({
            provider: "openai", requestBytes: Request(), generation: 9n, apiKey: "",
            fetchImplementation: () => assert.fail("fetch must not run"),
        });
        assert.equal(Catalogˉresponse(Bytes).status, STATUS.Unauthorized);
    }],
    ["stale provider generation", async () => {
        const Bytes = await Execute("openai", Generateˉrequest(), () => assert.fail("fetch must not run"), { generation: 10n });
        assert.equal(Generationˉresponse(Bytes).status, STATUS.Stale);
    }],
    ["OpenAI catalog and first page", async () => {
        const Bytes = await Execute("openai", Request({ limit: 1 }), async (Url, Options) => {
            assert.equal(Url, "https://api.openai.com/v1/models");
            assert.equal(Options.headers.authorization, "Bearer test-secret");
            assert.equal(Options.redirect, "error");
            return Jsonˉresponse({ data: [{ id: "z-model" }, { id: "a-model" }] });
        });
        const Value = Catalogˉresponse(Bytes);
        assert.deepEqual(Value.entries.map(Entry => Entry.id), ["a-model"]);
        assert.match(Value.continuation, /^v1:openai:[0-9a-f]{64}:1$/);
    }],
    ["catalog continuation", async () => {
        const Fetch = async () => Jsonˉresponse({ data: [{ id: "z-model" }, { id: "a-model" }] });
        const First = Catalogˉresponse(await Execute("openai", Request({ limit: 1 }), Fetch));
        const SecondRequest = Request({ limit: 1, generation: 9n, payload: Buffer.from(First.continuation) });
        const Second = Catalogˉresponse(await Execute("openai", SecondRequest, Fetch));
        assert.deepEqual(Second.entries.map(Entry => Entry.id), ["z-model"]);
        assert.equal(Second.continuation, "");
    }],
    ["stale catalog continuation", async () => {
        const Bytes = await Execute("openai", Request({
            generation: 9n, payload: Buffer.from(`v1:openai:${"0".repeat(64)}:1`),
        }), async () => Jsonˉresponse({ data: [{ id: "a" }, { id: "b" }] }));
        assert.equal(Catalogˉresponse(Bytes).status, STATUS.Stale);
    }],
    ["Anthropic catalog", async () => {
        const Bytes = await Execute("anthropic", Request(), async (Url, Options) => {
            assert.match(Url, /^https:\/\/api\.anthropic\.com\/v1\/models/);
            assert.equal(Options.headers["x-api-key"], "test-secret");
            assert.equal(Options.headers["anthropic-version"], "2023-06-01");
            return Jsonˉresponse({ data: [{ id: "claude-test", display_name: "Claude Test" }] });
        });
        assert.deepEqual(Catalogˉresponse(Bytes).entries[0], {
            id: "claude-test", display: "Claude Test", features: 0, lifecycle: 2,
        });
    }],
    ["Google catalog filtering", async () => {
        const Bytes = await Execute("google", Request(), async (Url, Options) => {
            assert.match(Url, /^https:\/\/generativelanguage\.googleapis\.com\/v1beta\/models/);
            assert.equal(Options.headers["x-goog-api-key"], "test-secret");
            return Jsonˉresponse({ models: [
                { name: "models/gemini-test", displayName: "Gemini Test", supportedGenerationMethods: ["generateContent"] },
                { name: "models/embed-test", supportedGenerationMethods: ["embedContent"] },
            ] });
        });
        assert.deepEqual(Catalogˉresponse(Bytes).entries[0], {
            id: "gemini-test", display: "Gemini Test", features: 3, lifecycle: 2,
        });
    }],
    ["Anthropic remote pagination", async () => {
        let Calls = 0;
        const Bytes = await Execute("anthropic", Request(), async Url => {
            Calls += 1;
            if (Calls === 1) return Jsonˉresponse({
                data: [{ id: "claude-b" }], has_more: true, last_id: "cursor/value",
            });
            assert.equal(Url, "https://api.anthropic.com/v1/models?limit=1000&after_id=cursor%2Fvalue");
            return Jsonˉresponse({ data: [{ id: "claude-a" }], has_more: false });
        });
        assert.deepEqual(Catalogˉresponse(Bytes).entries.map(Entry => Entry.id), ["claude-a", "claude-b"]);
    }],
    ["OpenAI generation", async () => {
        const Bytes = await Execute("openai", Generateˉrequest("gpt-test"), async (Url, Options) => {
            assert.equal(Url, "https://api.openai.com/v1/responses");
            const Body = JSON.parse(Options.body);
            assert.equal(Body.store, false);
            assert.equal(Body.max_output_tokens, 64);
            return Jsonˉresponse({
                status: "completed", model: "gpt-test-2026", output: [
                    { type: "reasoning", summary: [] },
                    { type: "message", content: [{ type: "output_text", text: "OpenAI answer" }] },
                ], usage: { input_tokens: 3, output_tokens: 2 },
            });
        });
        assert.deepEqual(Generationˉresponse(Bytes), {
            status: 0, requestId: 7n, generation: 9n, completion: 1, model: "gpt-test-2026",
            text: "OpenAI answer", diagnostic: "", inputTokens: 3n, outputTokens: 2n,
        });
    }],
    ["Anthropic generation", async () => {
        const Values = [[1, "Be brief"], [2, "Hello"], [3, "Hi"], [2, "Again"]];
        const Bytes = await Execute("anthropic", Generateˉrequest("claude-test", Values), async (Url, Options) => {
            assert.equal(Url, "https://api.anthropic.com/v1/messages");
            const Body = JSON.parse(Options.body);
            assert.equal(Body.system, "Be brief");
            assert.deepEqual(Body.messages.map(Message => Message.role), ["user", "assistant", "user"]);
            return Jsonˉresponse({
                model: "claude-test-2026", content: [{ type: "text", text: "Anthropic answer" }],
                stop_reason: "max_tokens", usage: { input_tokens: 4, output_tokens: 64 },
            });
        });
        const Value = Generationˉresponse(Bytes);
        assert.deepEqual([Value.completion, Value.text, Value.inputTokens], [2, "Anthropic answer", 4n]);
    }],
    ["Google generation", async () => {
        const Bytes = await Execute("google", Generateˉrequest("gemini/test", [[1, "Rules"], [2, "Hello"]]), async (Url, Options) => {
            assert.equal(Url, "https://generativelanguage.googleapis.com/v1beta/models/gemini%2Ftest:generateContent");
            const Body = JSON.parse(Options.body);
            assert.equal(Body.system_instruction.parts[0].text, "Rules");
            assert.equal(Body.contents[0].role, "user");
            return Jsonˉresponse({
                candidates: [{ content: { parts: [{ text: "Google answer" }] }, finishReason: "STOP" }],
                modelVersion: "gemini-test-2026", usageMetadata: { promptTokenCount: 2, candidatesTokenCount: 3 },
            });
        });
        const Value = Generationˉresponse(Bytes);
        assert.deepEqual([Value.completion, Value.model, Value.text, Value.outputTokens],
            [1, "gemini-test-2026", "Google answer", 3n]);
    }],
    ["OpenAI refusal", async () => {
        const Bytes = await Execute("openai", Generateˉrequest(), async () => Jsonˉresponse({
            status: "completed", model: "test-model", output: [
                { type: "message", content: [{ type: "refusal", refusal: "No" }] },
            ],
        }));
        assert.equal(Generationˉresponse(Bytes).completion, 3);
    }],
    ["unsupported provider output", async () => {
        const Bytes = await Execute("anthropic", Generateˉrequest(), async () => Jsonˉresponse({
            model: "test-model", content: [{ type: "tool_use", id: "1" }], stop_reason: "tool_use",
        }));
        assert.equal(Generationˉresponse(Bytes).status, STATUS.Unsupported);
    }],
    ["HTTP status mapping", async () => {
        const Expected = new Map([[400, 1], [401, 3], [404, 5], [429, 4], [503, 2]]);
        for (const [HttpStatus, ProtocolStatus] of Expected) {
            const Bytes = await Execute("openai", Request(), async () => Jsonˉresponse({}, HttpStatus));
            assert.equal(Catalogˉresponse(Bytes).status, ProtocolStatus);
        }
    }],
    ["generation transport uncertainty", async () => {
        const Bytes = await Execute("openai", Generateˉrequest(), async () => { throw new Error("network"); });
        const Value = Generationˉresponse(Bytes);
        assert.equal(Value.status, STATUS.SubmissionIndeterminate);
        assert.equal(Value.text, "");
    }],
    ["catalog transport unavailability", async () => {
        const Bytes = await Execute("google", Request(), async () => { throw new Error("network"); });
        assert.equal(Catalogˉresponse(Bytes).status, STATUS.Unavailable);
    }],
    ["malformed provider JSON", async () => {
        const Bytes = await Execute("openai", Request(), async () => new Response("{", {
            status: 200, headers: { "content-type": "application/json" },
        }));
        assert.equal(Catalogˉresponse(Bytes).status, STATUS.ProviderError);
    }],
    ["non-JSON provider body", async () => {
        const Bytes = await Execute("openai", Request(), async () => new Response("not json", {
            status: 200, headers: { "content-type": "text/plain" },
        }));
        assert.equal(Catalogˉresponse(Bytes).status, STATUS.ProviderError);
    }],
    ["oversized provider body", async () => {
        const Bytes = await Execute("openai", Request(), async () => new Response(
            `{"data":[],"padding":"${"x".repeat(1_048_576)}"}`,
            { status: 200, headers: { "content-type": "application/json" } },
        ));
        assert.equal(Catalogˉresponse(Bytes).status, STATUS.ProviderError);
    }],
    ["invalid provider text and usage", async () => {
        const TextBytes = await Execute("openai", Request(), async () => Jsonˉresponse({
            data: [{ id: "bad\ud800" }],
        }));
        assert.equal(Catalogˉresponse(TextBytes).status, STATUS.ProviderError);
        const UsageBytes = await Execute("openai", Generateˉrequest(), async () => Jsonˉresponse({
            status: "completed", model: "test-model", output: [],
            usage: { input_tokens: Number.MAX_SAFE_INTEGER + 1, output_tokens: 0 },
        }));
        assert.equal(Generationˉresponse(UsageBytes).status, STATUS.ProviderError);
    }],
    ["conversation-role rejection", async () => {
        const Bytes = await Execute(
            "anthropic", Generateˉrequest("claude-test", [[2, "One"], [2, "Two"]]),
            () => assert.fail("fetch must not run"),
        );
        assert.equal(Generationˉresponse(Bytes).status, STATUS.InvalidRequest);
    }],
];

for (const [Name, Test] of Tests) {
    try {
        await Test();
    } catch (Error) {
        Error.message = `${Name}: ${Error.message}`;
        throw Error;
    }
}

process.stdout.write("external model reference status=Passed providers=3 cases=24 live-calls=0 secrets=0\n");
