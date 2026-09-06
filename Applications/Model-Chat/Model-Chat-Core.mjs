export const MODEL_CHAT_MAX_LINE_BYTES = 3_072;
export const MODEL_CHAT_MAX_MESSAGES = 32;
export const MODEL_CHAT_MAX_MESSAGE_SET_BYTES = 16_384;

const PROVIDERS = Object.freeze({
    openai: Object.freeze({ service: "api.openai.com", display: "OpenAI" }),
    anthropic: Object.freeze({ service: "api.anthropic.com", display: "Anthropic" }),
    google: Object.freeze({ service: "generativelanguage.googleapis.com", display: "Google" }),
});

export class Modelˉchatˉfailure extends Error {
    constructor(Kind, Message) {
        super(Message);
        this.kind = Kind;
    }
}

function Fail(Kind, Message) {
    throw new Modelˉchatˉfailure(Kind, Message);
}

function Parseˉoptions(Arguments, Allowed) {
    const Values = new Map();
    for (let Index = 0; Index < Arguments.length; Index += 2) {
        const Name = Arguments[Index];
        const Value = Arguments[Index + 1];
        if (typeof Name !== "string" || !Name.startsWith("--") || Value === undefined ||
            Values.has(Name) || !Allowed.has(Name)) {
            Fail("usage", "Command options are invalid. Run with --help for usage.");
        }
        Values.set(Name, Value);
    }
    return Values;
}

function Requireˉoption(Values, Name) {
    if (!Values.has(Name) || Values.get(Name).length === 0) {
        Fail("usage", `${Name} is required.`);
    }
    return Values.get(Name);
}

function Decimal(Value, Minimum, Maximum, Description) {
    if (!/^[1-9][0-9]*$/.test(Value)) Fail("usage", `${Description} is invalid.`);
    const NumberValue = Number(Value);
    if (!Number.isSafeInteger(NumberValue) || NumberValue < Minimum || NumberValue > Maximum) {
        Fail("usage", `${Description} is invalid.`);
    }
    return NumberValue;
}

function Generation(Value) {
    if (!/^[1-9][0-9]*$/.test(Value)) Fail("usage", "Credential generation is invalid.");
    const Result = BigInt(Value);
    if (Result > 0xffff_ffff_ffff_ffffn) Fail("usage", "Credential generation is invalid.");
    return Result;
}

export function Modelˉchatˉusage() {
    return [
        "Windvale hosted model chat",
        "",
        "Usage:",
        "  Windvale-Model-Chat credential create --provider <openai|anthropic|google> --output <file> [--generation <u64>]",
        "  Windvale-Model-Chat credential inspect --credential <file>",
        "  Windvale-Model-Chat models --credential <file> [--page-size <1..128>] [--timeout-seconds <1..300>]",
        "  Windvale-Model-Chat chat --credential <file> --model <id> [--max-output-tokens <1..4096>] [--timeout-seconds <1..300>]",
        "",
        "Credentials and passphrases are entered only through masked terminal prompts.",
    ].join("\n");
}

export function Parseˉmodelˉchatˉarguments(Arguments) {
    if (!Array.isArray(Arguments)) Fail("usage", "Command arguments are invalid.");
    if (Arguments.length === 0 ||
        (Arguments.length === 1 && ["--help", "-h", "help"].includes(Arguments[0]))) {
        return Object.freeze({ command: "help" });
    }
    if (Arguments[0] === "credential") {
        const Action = Arguments[1];
        if (Action === "create") {
            const Values = Parseˉoptions(
                Arguments.slice(2), new Set(["--provider", "--output", "--generation"]),
            );
            const Provider = Requireˉoption(Values, "--provider");
            if (!Object.hasOwn(PROVIDERS, Provider)) Fail("usage", "Credential provider is invalid.");
            return Object.freeze({
                command: "credential_create",
                provider: Provider,
                service: PROVIDERS[Provider].service,
                providerDisplay: PROVIDERS[Provider].display,
                outputPath: Requireˉoption(Values, "--output"),
                credentialGeneration: Generation(Values.get("--generation") ?? "1"),
            });
        }
        if (Action === "inspect") {
            const Values = Parseˉoptions(Arguments.slice(2), new Set(["--credential"]));
            return Object.freeze({
                command: "credential_inspect",
                credentialPath: Requireˉoption(Values, "--credential"),
            });
        }
        Fail("usage", "Credential action is invalid. Run with --help for usage.");
    }
    if (Arguments[0] === "models") {
        const Values = Parseˉoptions(
            Arguments.slice(1), new Set(["--credential", "--page-size", "--timeout-seconds"]),
        );
        return Object.freeze({
            command: "models",
            credentialPath: Requireˉoption(Values, "--credential"),
            pageSize: Decimal(Values.get("--page-size") ?? "128", 1, 128, "Catalog page size"),
            timeoutMilliseconds: Decimal(
                Values.get("--timeout-seconds") ?? "120", 1, 300, "Timeout seconds",
            ) * 1_000,
        });
    }
    if (Arguments[0] === "chat") {
        const Values = Parseˉoptions(
            Arguments.slice(1),
            new Set(["--credential", "--model", "--max-output-tokens", "--timeout-seconds"]),
        );
        const Model = Requireˉoption(Values, "--model");
        const ModelBytes = Buffer.from(Model, "utf8");
        if (Model.includes("\0") || ModelBytes.length > 256 || ModelBytes.toString("utf8") !== Model) {
            Fail("usage", "Model identifier is invalid.");
        }
        return Object.freeze({
            command: "chat",
            credentialPath: Requireˉoption(Values, "--credential"),
            model: Model,
            maximumOutputTokens: Decimal(
                Values.get("--max-output-tokens") ?? "512", 1, 4_096,
                "Maximum output tokens",
            ),
            timeoutMilliseconds: Decimal(
                Values.get("--timeout-seconds") ?? "120", 1, 300, "Timeout seconds",
            ) * 1_000,
        });
    }
    Fail("usage", "Command is invalid. Run with --help for usage.");
}

function Strictˉmessage(Value, Role) {
    if (typeof Value !== "string" || Value.length === 0 || Value.includes("\0")) {
        Fail("input", `${Role} message is empty or invalid.`);
    }
    const Bytes = Buffer.from(Value, "utf8");
    if (Bytes.length > MODEL_CHAT_MAX_LINE_BYTES || Bytes.toString("utf8") !== Value) {
        Fail("input", `${Role} message exceeds the 3072-byte UTF-8 limit.`);
    }
    return Object.freeze({ role: Role, content: Value, bytes: Bytes.length });
}

function Messageˉsetˉbytes(Messages) {
    return 16 + Messages.reduce((Sum, Message) => Sum + 8 + Message.bytes, 0);
}

function Dropˉoldestˉturn(Messages) {
    if (Messages.length < 2 || Messages[0].role !== "user" || Messages[1].role !== "assistant") {
        Fail("history", "Conversation history is internally invalid.");
    }
    Messages.splice(0, 2);
}

function Fitˉhistory(Messages, MaximumMessages) {
    while (Messages.length > MaximumMessages ||
        Messageˉsetˉbytes(Messages) > MODEL_CHAT_MAX_MESSAGE_SET_BYTES) {
        Dropˉoldestˉturn(Messages);
    }
    return Messages;
}

export class Boundedˉchatˉconversation {
    #messages = [];

    prepare(Value) {
        const Messages = [...this.#messages, Strictˉmessage(Value, "user")];
        Fitˉhistory(Messages, MODEL_CHAT_MAX_MESSAGES - 1);
        return Object.freeze(Messages.map(Message => Object.freeze({
            role: Message.role,
            content: Message.content,
            bytes: Message.bytes,
        })));
    }

    commit(Prepared, AssistantValue) {
        if (!Array.isArray(Prepared) || Prepared.length < 1 ||
            Prepared[Prepared.length - 1]?.role !== "user") {
            Fail("history", "Prepared conversation turn is invalid.");
        }
        const Messages = Prepared.map(Message => Strictˉmessage(Message.content, Message.role));
        Messages.push(Strictˉmessage(AssistantValue, "assistant"));
        Fitˉhistory(Messages, MODEL_CHAT_MAX_MESSAGES);
        this.#messages = Messages;
        return this.inspect();
    }

    clear() {
        this.#messages = [];
    }

    inspect() {
        return Object.freeze({
            messages: this.#messages.length,
            turns: this.#messages.length / 2,
            bytes: Messageˉsetˉbytes(this.#messages),
        });
    }
}

export function Modelˉchatˉproviderˉprofile(Name) {
    return PROVIDERS[Name] ?? null;
}
