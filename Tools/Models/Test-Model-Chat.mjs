import assert from "node:assert/strict";
import {
    Createˉprotectedˉcredential,
    Inspectˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Decodeˉgatewayˉmodelˉrequest,
    Encodeˉgatewayˉcatalogˉresponse,
    Modelˉgatewayˉstatus,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Core.mjs";
import {
    Parseˉmodelˉchatˉarguments,
} from "../../Applications/Model-Chat/Model-Chat-Core.mjs";
import { Executeˉmodelˉchat } from "../../Applications/Model-Chat/Windvale-Model-Chat.mjs";

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

function Fakeˉterminal(Masked = []) {
    const Secrets = [...Masked];
    const Secretˉbuffers = [];
    const Output = [];
    return {
        output: Output,
        secretBuffers: Secretˉbuffers,
        write: Value => Output.push(String(Value)),
        error: Value => Output.push(String(Value)),
        readMasked: () => {
            assert.notEqual(Secrets.length, 0, "unexpected masked prompt");
            const Bytes = Buffer.from(Secrets.shift(), "utf8");
            Secretˉbuffers.push(Bytes);
            return Bytes;
        },
    };
}

function Ready(Changes = {}) {
    return Object.freeze({
        status: "ready",
        provider: "openai",
        providerGeneration: 1n,
        credentialGeneration: METADATA.generation,
        identity: METADATA.identity,
        ...Changes,
    });
}

function Dependencies(Observed = {}, Nativeˉresult = {}) {
    return {
        createCredential: Values => Createˉprotectedˉcredential({
            ...Values, randomBytes: Deterministicˉrandom(),
        }),
        readCredential: () => Buffer.from(WRAPPER),
        writeCredential: (_Path, Bytes) => {
            Observed.wrapper = Buffer.from(Bytes);
            return "C:/credentials/openai.wvsc";
        },
        gatewayFactory: () => ({
            ready: async () => Ready(),
            request: async Bytes => {
                const Request = Decodeˉgatewayˉmodelˉrequest(Buffer.from(Bytes));
                Observed.catalogRequest = Request;
                return Encodeˉgatewayˉcatalogˉresponse({
                    status: Modelˉgatewayˉstatus.Valid,
                    requestId: Request.requestId,
                    generation: 4n,
                    entries: [{ id: "gpt-test", display: "GPT Test" }],
                });
            },
            teardown: async () => { Observed.gatewayClosed = true; },
        }),
        nativeApplicationPath: () => "C:/windvale/Windvale-Model-Chat.exe",
        nativeGatewayFactory: Values => {
            Observed.nativeValues = Values;
            Observed.nativeWrapper = Values.wrapper;
            Observed.nativePassphrase = Values.passphrase;
            return {
                run: async () => ({
                    code: 0, signal: null, diagnosticBytes: 0, ready: Ready(), ...Nativeˉresult,
                }),
                teardown: async () => { Observed.nativeClosed = true; },
            };
        },
    };
}

const Tests = [];
function Test(Name, Body) { Tests.push([Name, Body]); }

Test("command families parse without secret-bearing options", () => {
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
});

Test("secret command-line options are rejected", () => {
    assert.throws(() => Parseˉmodelˉchatˉarguments([
        "chat", "--credential", "key.wvsc", "--model", "model-1", "--api-key", "secret",
    ]), Error => Error.kind === "usage");
});

Test("model identifiers and bounds are validated", () => {
    assert.throws(() => Parseˉmodelˉchatˉarguments([
        "chat", "--credential", "key.wvsc", "--model", "x".repeat(257),
    ]), Error => Error.kind === "usage");
    assert.throws(() => Parseˉmodelˉchatˉarguments([
        "chat", "--credential", "key.wvsc", "--model", "m", "--max-output-tokens", "4097",
    ]), Error => Error.kind === "usage");
});

Test("credential creation remains a masked custody operation", async () => {
    const Observed = {};
    const Terminal = Fakeˉterminal([
        SECRET_TEXT, SECRET_TEXT, PASSPHRASE_TEXT, PASSPHRASE_TEXT,
    ]);
    const Code = await Executeˉmodelˉchat({
        arguments: ["credential", "create", "--provider", "openai", "--output", "openai.wvsc"],
        terminal: Terminal,
        dependencies: Dependencies(Observed),
    });
    assert.equal(Code, 0);
    assert.equal(Inspectˉprotectedˉcredential(Observed.wrapper).provider, "openai");
    assert.ok(Terminal.secretBuffers.every(Value => Value.every(Byte => Byte === 0)));
});

Test("credential inspection reveals metadata but not the secret", async () => {
    const Terminal = Fakeˉterminal();
    assert.equal(await Executeˉmodelˉchat({
        arguments: ["credential", "inspect", "--credential", "openai.wvsc"],
        terminal: Terminal,
        dependencies: Dependencies(),
    }), 0);
    const Output = Terminal.output.join("");
    assert.match(Output, /provider=openai/);
    assert.doesNotMatch(Output, new RegExp(SECRET_TEXT));
});

Test("catalog administration remains bounded in the supervisor", async () => {
    const Observed = {};
    const Terminal = Fakeˉterminal([PASSPHRASE_TEXT]);
    assert.equal(await Executeˉmodelˉchat({
        arguments: ["models", "--credential", "openai.wvsc", "--page-size", "8"],
        terminal: Terminal,
        dependencies: Dependencies(Observed),
    }), 0);
    assert.equal(Observed.catalogRequest.limit, 8);
    assert.match(Terminal.output.join(""), /gpt-test/);
    assert.equal(Observed.gatewayClosed, true);
});

Test("chat delegates UI and history to the native Windvale application", async () => {
    const Observed = {};
    const Terminal = Fakeˉterminal([PASSPHRASE_TEXT]);
    const Code = await Executeˉmodelˉchat({
        arguments: [
            "chat", "--credential", "openai.wvsc", "--model", "gpt-test",
            "--max-output-tokens", "321", "--timeout-seconds", "7",
        ],
        terminal: Terminal,
        dependencies: Dependencies(Observed),
    });
    assert.equal(Code, 0);
    assert.deepEqual(Observed.nativeValues.applicationArguments, ["openai", "gpt-test", "321"]);
    assert.equal(Observed.nativeValues.maximumOperationMilliseconds, 7_000);
    assert.equal(Observed.nativeValues.maximumLifetimeMilliseconds, 187_000);
    assert.equal(Observed.nativeClosed, true);
    assert.ok(Observed.nativeWrapper.every(Byte => Byte === 0));
    assert.ok(Observed.nativePassphrase.every(Byte => Byte === 0));
    assert.equal(Terminal.output.join(""), "");
});

Test("native Windvale exit status is propagated exactly", async () => {
    assert.equal(await Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "openai.wvsc", "--model", "gpt-test"],
        terminal: Fakeˉterminal([PASSPHRASE_TEXT]),
        dependencies: Dependencies({}, { code: 75 }),
    }), 75);
});

Test("native readiness mismatch is rejected", async () => {
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "openai.wvsc", "--model", "gpt-test"],
        terminal: Fakeˉterminal([PASSPHRASE_TEXT]),
        dependencies: Dependencies({}, { ready: Ready({ identity: "wrong" }) }),
    }), Error => Error.kind === "gateway");
});

Test("native signal termination is rejected", async () => {
    await assert.rejects(Executeˉmodelˉchat({
        arguments: ["chat", "--credential", "openai.wvsc", "--model", "gpt-test"],
        terminal: Fakeˉterminal([PASSPHRASE_TEXT]),
        dependencies: Dependencies({}, { code: null, signal: "SIGTERM" }),
    }), Error => Error.kind === "gateway");
});

for (let Index = 0; Index < Tests.length; Index += 1) {
    const [Name, Body] = Tests[Index];
    process.stdout.write(`step=model-chat item=${Index + 1}/${Tests.length} name=${Name}\n`);
    await Body();
}

WRAPPER.fill(0);
process.stdout.write(
    `model chat status=Passed providers=3 cases=${Tests.length} ` +
    "native-ui=Windvale credential-custody=Hosted public-network=0 real-credentials=0\n",
);
