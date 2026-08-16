import assert from "node:assert/strict";
import {
    Createˉprotectedˉcredential,
    Inspectˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Decodeˉgatewayˉmodelˉrequest,
    Encodeˉgatewayˉcatalogˉresponse,
    Encodeˉgatewayˉgenerationˉresponse,
    Modelˉgatewayˉstatus,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Core.mjs";
import {
    Decodeˉgatewayˉcatalogˉresponse,
    Decodeˉgatewayˉgenerationˉresponse,
    Encodeˉgatewayˉcatalogˉrequest,
    Encodeˉgatewayˉgenerationˉrequest,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Client.mjs";
import {
    Boundedˉchatˉconversation,
    MODEL_CHAT_MAX_MESSAGE_SET_BYTES,
    Parseˉmodelˉchatˉarguments,
} from "../../Applications/Model-Chat/Model-Chat-Core.mjs";
import {
    Executeˉmodelˉchat,
} from "../../Applications/Model-Chat/Windvale-Model-Chat.mjs";

const PASSPHRASE_TEXT = "correct horse battery staple";
const SECRET_TEXT = "test-provider-credential-value";

function Deterministicˉrandom() {
    let Value = 1;
    return Length => Buffer.alloc(Length, Value++);
}

const WRAPPER = await Createˉprotectedˉcredential({
    provider: "openai",
    service: "api.openai.com",
    generation: 7n,
    credential: Buffer.from(SECRET_TEXT, "ascii"),
    passphrase: Buffer.from(PASSPHRASE_TEXT, "utf8"),
    randomBytes: Deterministicˉrandom(),
});
const METADATA = Inspectˉprotectedˉcredential(WRAPPER);

function Fakeˉterminal({ masked = [], lines = [] } = {}) {
    const Masked = [...masked];
    const Lines = [...lines];
    const SecretBuffers = [];
    const Output = [];
    return {
        output: Output,
        secretBuffers: SecretBuffers,
        write: Value => Output.push(String(Value)),
        error: Value => Output.push(String(Value)),
        readMasked: () => {
            assert.notEqual(Masked.length, 0, "unexpected masked prompt");
            const Bytes = Buffer.from(Masked.shift(), "utf8");
            SecretBuffers.push(Bytes);
            return Bytes;
        },
        readLine: () => Lines.length === 0 ? null : Lines.shift(),
    };
}

function Ready() {
    return Object.freeze({
        status: "ready",
        provider: "openai",
        providerGeneration: 1n,
        credentialGeneration: METADATA.generation,
        identity: METADATA.identity,
    });
}

function Gatewayˉdependencies(Handler, Observed = {}) {
    return {
        createCredential: Values => Createˉprotectedˉcredential({
            ...Values, randomBytes: Deterministicˉrandom(),
        }),
        readCredential: () => Buffer.from(WRAPPER),
        writeCredential: (_Path, Bytes) => {
            Observed.wrapper = Buffer.from(Bytes);
            return "C:/credentials/openai.wvsc";
        },
        gatewayFactory: Values => {
            Observed.gatewayValues = {
                providerGeneration: Values.providerGeneration,
                trustGeneration: Values.trustGeneration,
                passphrase: Buffer.from(Values.passphrase),
            };
            let Closed = false;
            return {
                ready: async () => Ready(),
                request: async Bytes => Handler(Buffer.from(Bytes)),
                teardown: async () => { Closed = true; Observed.closed = true; },
                inspect: () => ({ closed: Closed }),
            };
        },
    };
}

const Tests = [];
function Test(Name, Body) { Tests.push([Name, Body]); }

Test("help and exact command families parse without secret-bearing options", () => {
    assert.equal(Parseˉmodelˉchatˉarguments(["--help"]).command, "help");
    const Create = Parseˉmodelˉchatˉarguments([
        "credential", "create", "--provider", "anthropic", "--output", "key.wvsc",
        "--generation", "9",
    ]);
    assert.deepEqual([Create.command, Create.service, Create.credentialGeneration],
        ["credential_create", "api.anthropic.com", 9n]);
    const Chat = Parseˉmodelˉchatˉarguments([
        "chat", "--credential", "key.wvsc", "--model", "model-1",
        "--max-output-tokens", "64", "--timeout-seconds", "5",
    ]);
    assert.deepEqual([Chat.command, Chat.maximumOutputTokens, Chat.timeoutMilliseconds],
        ["chat", 64, 5_000]);
    assert.throws(() => Parseˉmodelˉchatˉarguments([
        "chat", "--credential", "key.wvsc", "--model", "model-1", "--api-key", "secret",
    ]), Error => Error.kind === "usage");
});

Test("argument and model limits reject malformed values", () => {
    assert.throws(() => Parseˉmodelˉchatˉarguments(["models", "--credential", "x", "--page-size", "129"]));
    assert.throws(() => Parseˉmodelˉchatˉarguments(["chat", "--credential", "x", "--model", ""]));
    assert.throws(() => Parseˉmodelˉchatˉarguments([
        "credential", "create", "--provider", "other", "--output", "x",
    ]));
});

Test("conversation prepares and commits exact multi-turn history", () => {
    const Conversation = new Boundedˉchatˉconversation();
    const First = Conversation.prepare("hello");
    Conversation.commit(First, "answer");
    const Second = Conversation.prepare("again");
    assert.deepEqual(Second.map(Message => [Message.role, Message.content]), [
        ["user", "hello"], ["assistant", "answer"], ["user", "again"],
    ]);
    assert.deepEqual(Conversation.inspect(), { messages: 2, turns: 1, bytes: 43 });
});

Test("conversation rolls away complete old turns within count and byte bounds", () => {
    const Conversation = new Boundedˉchatˉconversation();
    for (let Index = 0; Index < 40; Index += 1) {
        const Prepared = Conversation.prepare(`user-${Index}-${"u".repeat(900)}`);
        Conversation.commit(Prepared, `assistant-${Index}-${"a".repeat(900)}`);
    }
    const State = Conversation.inspect();
    assert.ok(State.messages <= 32);
    assert.ok(State.bytes <= MODEL_CHAT_MAX_MESSAGE_SET_BYTES);
    assert.equal(State.messages % 2, 0);
});

Test("conversation rejects empty, NUL, and oversized input", () => {
    const Conversation = new Boundedˉchatˉconversation();
    assert.throws(() => Conversation.prepare(""), Error => Error.kind === "input");
    assert.throws(() => Conversation.prepare("bad\0value"), Error => Error.kind === "input");
    assert.throws(() => Conversation.prepare("x".repeat(3_073)), Error => Error.kind === "input");
});

Test("client encodes catalog request with continuation generation", () => {
    const First = Decodeˉgatewayˉmodelˉrequest(Encodeˉgatewayˉcatalogˉrequest({ requestId: 4n }));
    assert.deepEqual([First.operation, First.requestId, First.providerGeneration], ["catalog", 4n, 0n]);
    const Next = Decodeˉgatewayˉmodelˉrequest(Encodeˉgatewayˉcatalogˉrequest({
        requestId: 5n, providerGeneration: 3n, limit: 2, continuation: "next",
    }));
    assert.deepEqual([Next.limit, Next.continuation], [2, "next"]);
    assert.throws(() => Encodeˉgatewayˉcatalogˉrequest({ requestId: 1n, continuation: "next" }));
});

Test("client encodes bounded multi-role generation request", () => {
    const Request = Decodeˉgatewayˉmodelˉrequest(Encodeˉgatewayˉgenerationˉrequest({
        requestId: 8n, providerGeneration: 2n, maximumOutputTokens: 128, model: "gpt-test",
        messages: [{ role: "user", content: "hello" }, { role: "assistant", content: "hi" },
            { role: "user", content: "again" }],
    }));
    assert.equal(Request.operation, "generate");
    assert.equal(Request.messages.length, 3);
    assert.deepEqual(Request.messages.map(Message => Message.role), [2, 3, 2]);
});

Test("client admits canonical catalog response", () => {
    const Response = Decodeˉgatewayˉcatalogˉresponse(Encodeˉgatewayˉcatalogˉresponse({
        requestId: 9n, generation: 4n,
        entries: [{ id: "model-a", display: "Model A", features: 3, lifecycle: 1 }],
        continuation: "more",
    }), 9n);
    assert.deepEqual([Response.statusName, Response.generation, Response.entries[0].id, Response.continuation],
        ["valid", 4n, "model-a", "more"]);
});

Test("client admits canonical generation success and typed failure", () => {
    const Success = Decodeˉgatewayˉgenerationˉresponse(Encodeˉgatewayˉgenerationˉresponse({
        requestId: 10n, generation: 4n, completion: 1, model: "model-a", text: "answer",
        inputTokens: 2n, outputTokens: 1n,
    }), 10n);
    assert.deepEqual([Success.statusName, Success.text, Success.outputTokens], ["valid", "answer", 1n]);
    const Failure = Decodeˉgatewayˉgenerationˉresponse(Encodeˉgatewayˉgenerationˉresponse({
        status: Modelˉgatewayˉstatus.RateLimited, requestId: 11n, diagnostic: "Rate limited.",
    }), 11n);
    assert.deepEqual([Failure.statusName, Failure.diagnostic], ["rate_limited", "Rate limited."]);
});

Test("client rejects response identity, reserved, and length corruption", () => {
    const Source = Encodeˉgatewayˉcatalogˉresponse({ requestId: 1n, generation: 2n, entries: [] });
    assert.throws(() => Decodeˉgatewayˉcatalogˉresponse(Source, 2n));
    const Reserved = Buffer.from(Source);
    Reserved.writeUInt32LE(1, 44);
    assert.throws(() => Decodeˉgatewayˉcatalogˉresponse(Reserved, 1n));
    const Length = Buffer.from(Source);
    Length.writeUInt32LE(Length.length + 1, 8);
    assert.throws(() => Decodeˉgatewayˉcatalogˉresponse(Length, 1n));
});

Test("credential create masks both values, writes WVSC, and erases prompt buffers", async () => {
    const Terminal = Fakeˉterminal({
        masked: [SECRET_TEXT, SECRET_TEXT, PASSPHRASE_TEXT, PASSPHRASE_TEXT],
    });
    const Observed = {};
    await Executeˉmodelˉchat({
        arguments: ["credential", "create", "--provider", "openai", "--output", "openai.wvsc"],
        terminal: Terminal,
        dependencies: Gatewayˉdependencies(() => assert.fail("gateway must not run"), Observed),
    });
    assert.equal(Observed.wrapper.subarray(0, 4).toString("ascii"), "WVSC");
    assert.equal(Inspectˉprotectedˉcredential(Observed.wrapper).provider, "openai");
    assert.equal(Terminal.secretBuffers.every(Bytes => Bytes.every(Byte => Byte === 0)), true);
    Observed.wrapper.fill(0);
});

Test("credential create rejects mismatched confirmation before writing", async () => {
    const Terminal = Fakeˉterminal({ masked: [SECRET_TEXT, `${SECRET_TEXT}-different`] });
    const Observed = {};
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["credential", "create", "--provider", "openai", "--output", "openai.wvsc"],
        terminal: Terminal,
        dependencies: Gatewayˉdependencies(() => {}, Observed),
    }), Error => Error.kind === "input");
    assert.equal(Object.hasOwn(Observed, "wrapper"), false);
});

Test("credential inspect reports metadata without prompting or ciphertext", async () => {
    const Terminal = Fakeˉterminal();
    await Executeˉmodelˉchat({
        arguments: ["credential", "inspect", "--credential", "openai.wvsc"],
        terminal: Terminal,
        dependencies: Gatewayˉdependencies(() => assert.fail("gateway must not run")),
    });
    const Output = Terminal.output.join("");
    assert.match(Output, /provider=openai/);
    assert.match(Output, new RegExp(`identity=${METADATA.identity}`));
    assert.equal(Output.includes(SECRET_TEXT), false);
});

Test("models unlocks once and walks bounded catalog pages", async () => {
    const Terminal = Fakeˉterminal({ masked: [PASSPHRASE_TEXT] });
    const Observed = { requests: 0 };
    const Dependencies = Gatewayˉdependencies(RequestBytes => {
        const Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes);
        Observed.requests += 1;
        return Encodeˉgatewayˉcatalogˉresponse({
            requestId: Request.requestId,
            generation: 1n,
            entries: [{ id: Observed.requests === 1 ? "model-a" : "model-b" }],
            continuation: Observed.requests === 1 ? "page-2" : "",
        });
    }, Observed);
    await Executeˉmodelˉchat({
        arguments: ["models", "--credential", "openai.wvsc", "--page-size", "1"],
        terminal: Terminal, dependencies: Dependencies,
    });
    assert.equal(Observed.requests, 2);
    assert.equal(Observed.closed, true);
    assert.match(Terminal.output.join(""), /model-a\nmodel-b\nListed 2 models from openai\./);
    assert.equal(Terminal.secretBuffers[0].every(Byte => Byte === 0), true);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("chat submits and retains an exact two-turn conversation", async () => {
    const Terminal = Fakeˉterminal({
        masked: [PASSPHRASE_TEXT], lines: ["hello", "again", ":quit"],
    });
    const Observed = { requests: [] };
    const Dependencies = Gatewayˉdependencies(RequestBytes => {
        const Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes);
        Observed.requests.push(Request);
        return Encodeˉgatewayˉgenerationˉresponse({
            requestId: Request.requestId, generation: 1n, completion: 1,
            model: "gpt-test", text: `answer-${Observed.requests.length}`,
        });
    }, Observed);
    await Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "openai.wvsc", "--model", "gpt-test"],
        terminal: Terminal, dependencies: Dependencies,
    });
    assert.equal(Observed.requests.length, 2);
    assert.deepEqual(Observed.requests[1].messages.map(Message => [Message.role, Message.content]), [
        [2, "hello"], [3, "answer-1"], [2, "again"],
    ]);
    assert.match(Terminal.output.join(""), /model> answer-1\nmodel> answer-2\nChat closed\./);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("chat clear removes retained conversation before the next request", async () => {
    const Terminal = Fakeˉterminal({
        masked: [PASSPHRASE_TEXT], lines: ["one", ":clear", "two", ":quit"],
    });
    const Observed = { requests: [] };
    await Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "x", "--model", "gpt-test"], terminal: Terminal,
        dependencies: Gatewayˉdependencies(RequestBytes => {
            const Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes);
            Observed.requests.push(Request);
            return Encodeˉgatewayˉgenerationˉresponse({
                requestId: Request.requestId, generation: 1n, completion: 1,
                model: "gpt-test", text: "answer",
            });
        }, Observed),
    });
    assert.deepEqual(Observed.requests[1].messages.map(Message => Message.content), ["two"]);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("oversized response is displayed but never retained", async () => {
    const Terminal = Fakeˉterminal({
        masked: [PASSPHRASE_TEXT], lines: ["one", "two", ":quit"],
    });
    const Observed = { requests: [] };
    await Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "x", "--model", "gpt-test"], terminal: Terminal,
        dependencies: Gatewayˉdependencies(RequestBytes => {
            const Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes);
            Observed.requests.push(Request);
            return Encodeˉgatewayˉgenerationˉresponse({
                requestId: Request.requestId, generation: 1n, completion: 1,
                model: "gpt-test", text: Observed.requests.length === 1 ? "x".repeat(3_073) : "small",
            });
        }, Observed),
    });
    assert.deepEqual(Observed.requests[1].messages.map(Message => Message.content), ["two"]);
    assert.match(Terminal.output.join(""), /turn was not retained/);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("definite provider failure is typed, not retained, and gateway closes", async () => {
    const Terminal = Fakeˉterminal({ masked: [PASSPHRASE_TEXT], lines: ["hello"] });
    const Observed = { requests: 0 };
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "x", "--model", "gpt-test"], terminal: Terminal,
        dependencies: Gatewayˉdependencies(RequestBytes => {
            const Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes);
            Observed.requests += 1;
            return Encodeˉgatewayˉgenerationˉresponse({
                status: Modelˉgatewayˉstatus.Unauthorized,
                requestId: Request.requestId,
                diagnostic: "Provider authorization failed.",
            });
        }, Observed),
    }), Error => Error.kind === "provider_unauthorized");
    assert.deepEqual([Observed.requests, Observed.closed], [1, true]);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("indeterminate submission is typed and never retried", async () => {
    const Terminal = Fakeˉterminal({ masked: [PASSPHRASE_TEXT], lines: ["hello"] });
    const Observed = { requests: 0 };
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "x", "--model", "gpt-test"], terminal: Terminal,
        dependencies: Gatewayˉdependencies(RequestBytes => {
            const Request = Decodeˉgatewayˉmodelˉrequest(RequestBytes);
            Observed.requests += 1;
            return Encodeˉgatewayˉgenerationˉresponse({
                status: Modelˉgatewayˉstatus.SubmissionIndeterminate,
                requestId: Request.requestId,
                diagnostic: "Submission completion is indeterminate.",
            });
        }, Observed),
    }), Error => Error.kind === "submission_indeterminate" && /not retried/.test(Error.message));
    assert.deepEqual([Observed.requests, Observed.closed], [1, true]);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("generation transport loss is indeterminate and never retried", async () => {
    const Terminal = Fakeˉterminal({ masked: [PASSPHRASE_TEXT], lines: ["hello"] });
    const Observed = { requests: 0 };
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "x", "--model", "gpt-test"], terminal: Terminal,
        dependencies: Gatewayˉdependencies(() => {
            Observed.requests += 1;
            throw new Error("peer lost");
        }, Observed),
    }), Error => Error.kind === "submission_indeterminate" && /not retried/.test(Error.message));
    assert.deepEqual([Observed.requests, Observed.closed], [1, true]);
    Observed.gatewayValues.passphrase.fill(0);
});

Test("malformed gateway response is rejected and supervisor is closed", async () => {
    const Terminal = Fakeˉterminal({ masked: [PASSPHRASE_TEXT], lines: ["hello"] });
    const Observed = {};
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "x", "--model", "gpt-test"], terminal: Terminal,
        dependencies: Gatewayˉdependencies(() => Buffer.from("malformed"), Observed),
    }), Error => Error.kind === "invalid_response");
    assert.equal(Observed.closed, true);
    Observed.gatewayValues.passphrase.fill(0);
});

for (let Index = 0; Index < Tests.length; Index += 1) {
    process.stdout.write(`step=model-chat item=${Index + 1}/${Tests.length}\n`);
    const [Name, Body] = Tests[Index];
    try { await Body(); } catch (Error) {
        Error.message = `${Name}: ${Error.message}`;
        throw Error;
    }
}

WRAPPER.fill(0);
process.stdout.write(
    `model chat status=Passed providers=3 cases=${Tests.length} ` +
    "live-calls=0 real-credentials=0 secrets-in-arguments=0 automatic-retries=0\n",
);
