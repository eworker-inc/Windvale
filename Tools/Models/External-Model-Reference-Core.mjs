import crypto from "node:crypto";

const UTF8 = new TextDecoder("utf-8", { fatal: true });
const MAX_REQUEST_BYTES = 65_536;
const MAX_RESPONSE_BODY_BYTES = 1_048_576;
const MAX_MODEL_BYTES = 256;
const MAX_MESSAGE_BYTES = 3_072;
const MAX_MESSAGE_SET_BYTES = 16_384;
const MAX_MESSAGES = 32;
const MAX_CATALOG_ENTRIES = 8_192;
const MAX_CATALOG_BYTES = 65_536;
const MAX_CONTINUATION_BYTES = 1_024;
const MAX_DIAGNOSTIC_BYTES = 1_024;
const MAX_OUTPUT_TOKENS = 4_096;

const STATUS = Object.freeze({
    Valid: 0,
    InvalidRequest: 1,
    Unavailable: 2,
    Unauthorized: 3,
    RateLimited: 4,
    Unsupported: 5,
    ProviderError: 6,
    Cancelled: 7,
    Revoked: 8,
    Stale: 9,
    PeerExited: 10,
    SubmissionIndeterminate: 11,
});

const PROVIDERS = Object.freeze({
    openai: Object.freeze({
        keyEnvironment: "OPENAI_API_KEY",
        catalogUrl: "https://api.openai.com/v1/models",
    }),
    anthropic: Object.freeze({
        keyEnvironment: "ANTHROPIC_API_KEY",
        catalogUrl: "https://api.anthropic.com/v1/models?limit=1000",
    }),
    google: Object.freeze({
        keyEnvironment: "GEMINI_API_KEY",
        catalogUrl: "https://generativelanguage.googleapis.com/v1beta/models?pageSize=1000",
    }),
});

class Referenceˉerror extends Error {
    constructor(Message, Status = STATUS.ProviderError) {
        super(Message);
        this.status = Status;
    }
}

function Fail(Message, Status) {
    throw new Referenceˉerror(Message, Status);
}

function U32(Bytes, Offset) {
    return Bytes.readUInt32LE(Offset);
}

function U64(Bytes, Offset) {
    return Bytes.readBigUInt64LE(Offset);
}

function Strictˉtext(Bytes, Description, MaximumBytes, AllowEmpty = false) {
    if (Bytes.length > MaximumBytes || (!AllowEmpty && Bytes.length === 0)) {
        Fail(`${Description} length is invalid.`, STATUS.InvalidRequest);
    }
    let Value;
    try {
        Value = UTF8.decode(Bytes);
    } catch {
        Fail(`${Description} is not strict UTF-8.`, STATUS.InvalidRequest);
    }
    if (Value.includes("\0")) Fail(`${Description} contains NUL.`, STATUS.InvalidRequest);
    return Value;
}

function Checkedˉend(Offset, Length, Total, Description) {
    if (!Number.isSafeInteger(Offset) || !Number.isSafeInteger(Length) || Length < 0 ||
        Offset < 0 || Offset > Total || Length > Total - Offset) {
        Fail(`${Description} exceeds its record.`, STATUS.InvalidRequest);
    }
    return Offset + Length;
}

function Decodeˉmessages(Bytes) {
    if (Bytes.length < 16 || Bytes.length > MAX_MESSAGE_SET_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVMM" || U32(Bytes, 4) !== 1 ||
        U32(Bytes, 8) !== Bytes.length) {
        Fail("The message record is malformed.", STATUS.InvalidRequest);
    }
    const Count = U32(Bytes, 12);
    if (Count < 1 || Count > MAX_MESSAGES) {
        Fail("The message count is invalid.", STATUS.InvalidRequest);
    }
    const Messages = [];
    let Offset = 16;
    for (let Index = 0; Index < Count; Index += 1) {
        const HeaderEnd = Checkedˉend(Offset, 8, Bytes.length, "Message header");
        const Role = U32(Bytes, Offset);
        const Length = U32(Bytes, Offset + 4);
        if (Role < 1 || Role > 3) Fail("The message role is invalid.", STATUS.InvalidRequest);
        const End = Checkedˉend(HeaderEnd, Length, Bytes.length, "Message content");
        Messages.push({
            role: Role,
            content: Strictˉtext(Bytes.subarray(HeaderEnd, End), "Message content", MAX_MESSAGE_BYTES),
        });
        Offset = End;
    }
    if (Offset !== Bytes.length) Fail("The message record has trailing bytes.", STATUS.InvalidRequest);
    return Messages;
}

export function Decodeˉmodelˉrequest(Value) {
    const Bytes = Buffer.from(Value);
    if (Bytes.length < 48 || Bytes.length > MAX_REQUEST_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVMQ" || U32(Bytes, 4) !== 1 ||
        U32(Bytes, 8) !== Bytes.length || U32(Bytes, 44) !== 0) {
        Fail("The model request is malformed.", STATUS.InvalidRequest);
    }
    const Operation = U32(Bytes, 12);
    const RequestId = U64(Bytes, 16);
    const ProviderGeneration = U64(Bytes, 24);
    const Limit = U32(Bytes, 32);
    const ModelLength = U32(Bytes, 36);
    const PayloadLength = U32(Bytes, 40);
    const ModelEnd = Checkedˉend(48, ModelLength, Bytes.length, "Model identifier");
    const PayloadEnd = Checkedˉend(ModelEnd, PayloadLength, Bytes.length, "Request payload");
    if (PayloadEnd !== Bytes.length) Fail("The model request has trailing bytes.", STATUS.InvalidRequest);
    const Model = Strictˉtext(
        Bytes.subarray(48, ModelEnd), "Model identifier", MAX_MODEL_BYTES, Operation === 1,
    );
    const Payload = Bytes.subarray(ModelEnd, PayloadEnd);
    if (Operation === 1) {
        if (ModelLength !== 0 || Limit < 1 || Limit > 128 || Payload.length > MAX_CONTINUATION_BYTES ||
            (Payload.length > 0 && ProviderGeneration === 0n)) {
            Fail("The catalog request invariant is invalid.", STATUS.InvalidRequest);
        }
        return {
            operation: "catalog", requestId: RequestId, providerGeneration: ProviderGeneration,
            limit: Limit, continuation: Strictˉtext(Payload, "Continuation", MAX_CONTINUATION_BYTES, true),
        };
    }
    if (Operation === 2) {
        if (ProviderGeneration === 0n || ModelLength === 0 || Limit < 1 ||
            Limit > MAX_OUTPUT_TOKENS || Payload.length === 0) {
            Fail("The generation request invariant is invalid.", STATUS.InvalidRequest);
        }
        return {
            operation: "generate", requestId: RequestId, providerGeneration: ProviderGeneration,
            limit: Limit, model: Model, messages: Decodeˉmessages(Payload),
        };
    }
    Fail("The model operation is unsupported.", STATUS.InvalidRequest);
}

function Putˉheader(Bytes, Magic, Total, Status, RequestId, Generation) {
    Bytes.write(Magic, 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Total, 8);
    Bytes.writeUInt32LE(Status, 12);
    Bytes.writeBigUInt64LE(RequestId, 16);
    Bytes.writeBigUInt64LE(Generation, 24);
}

function Boundedˉdiagnostic(Value) {
    const Bytes = Buffer.from(String(Value), "utf8");
    return Bytes.length <= MAX_DIAGNOSTIC_BYTES ? Bytes : Buffer.from("Provider adapter failure.");
}

function Providerˉbytes(Value, Description, MaximumBytes, AllowEmpty = true) {
    if (typeof Value !== "string" || Value.includes("\0") || (!AllowEmpty && Value.length === 0)) {
        Fail(`Provider ${Description} is malformed.`);
    }
    const Bytes = Buffer.from(Value, "utf8");
    if (Bytes.length > MaximumBytes || Bytes.toString("utf8") !== Value) {
        Fail(`Provider ${Description} is malformed or too large.`);
    }
    return Bytes;
}

export function Encodeˉcatalogˉresponse({
    status = STATUS.Valid, requestId = 0n, generation = 0n, entries = [], continuation = "", diagnostic = "",
}) {
    const Continuation = Buffer.from(continuation, "utf8");
    const Diagnostic = Boundedˉdiagnostic(diagnostic);
    if (!Number.isInteger(status) || status < 0 || status > 11 ||
        (status === STATUS.Valid && (generation === 0n || Diagnostic.length !== 0)) ||
        (status !== STATUS.Valid && (entries.length !== 0 || Continuation.length !== 0))) {
        Fail("Catalog response invariant is invalid.");
    }
    const EncodedEntries = entries.map(Entry => {
        const Identifier = Providerˉbytes(Entry.id, "catalog identifier", MAX_MODEL_BYTES, false);
        const Display = Providerˉbytes(Entry.display ?? "", "catalog display", MAX_MODEL_BYTES);
        const Bytes = Buffer.alloc(20 + Identifier.length + Display.length);
        Bytes.writeUInt32LE(Bytes.length, 0);
        Bytes.writeUInt32LE(Identifier.length, 4);
        Bytes.writeUInt32LE(Display.length, 8);
        Bytes.writeUInt32LE(Entry.features ?? 0, 12);
        Bytes.writeUInt32LE(Entry.lifecycle ?? 2, 16);
        Identifier.copy(Bytes, 20);
        Display.copy(Bytes, 20 + Identifier.length);
        return Bytes;
    });
    const Total = 48 + EncodedEntries.reduce((Sum, Entry) => Sum + Entry.length, 0) +
        Continuation.length + Diagnostic.length;
    if (entries.length > 128 || Continuation.length > MAX_CONTINUATION_BYTES || Total > MAX_CATALOG_BYTES) {
        Fail("Provider catalog page exceeds the protocol limit.");
    }
    const Bytes = Buffer.alloc(Total);
    Putˉheader(Bytes, "WVMC", Total, status, requestId, generation);
    Bytes.writeUInt32LE(entries.length, 32);
    Bytes.writeUInt32LE(Continuation.length, 36);
    Bytes.writeUInt32LE(Diagnostic.length, 40);
    let Offset = 48;
    for (const Entry of EncodedEntries) {
        Entry.copy(Bytes, Offset);
        Offset += Entry.length;
    }
    Continuation.copy(Bytes, Offset);
    Diagnostic.copy(Bytes, Offset + Continuation.length);
    return Bytes;
}

export function Encodeˉgenerationˉresponse({
    status = STATUS.Valid, requestId = 0n, generation = 0n, completion = 0,
    model = "", text = "", diagnostic = "", inputTokens = 0n, outputTokens = 0n,
}) {
    const Model = Providerˉbytes(model, "returned model", MAX_MODEL_BYTES);
    const Text = Providerˉbytes(text, "output text", MAX_RESPONSE_BODY_BYTES);
    const Diagnostic = Boundedˉdiagnostic(diagnostic);
    if (!Number.isInteger(status) || status < 0 || status > 11 ||
        (status === STATUS.Valid && (generation === 0n || completion < 1 || completion > 3 ||
            Model.length === 0 || Diagnostic.length !== 0)) ||
        (status !== STATUS.Valid && (completion !== 0 || Model.length !== 0 || Text.length !== 0 ||
            inputTokens !== 0n || outputTokens !== 0n))) {
        Fail("Generation response invariant is invalid.");
    }
    const Total = 64 + Model.length + Text.length + Diagnostic.length;
    if (Total > MAX_RESPONSE_BODY_BYTES) Fail("Generation response exceeds the adapter limit.");
    const Bytes = Buffer.alloc(Total);
    Putˉheader(Bytes, "WVMG", Total, status, requestId, generation);
    Bytes.writeUInt32LE(completion, 32);
    Bytes.writeUInt32LE(Model.length, 36);
    Bytes.writeUInt32LE(Text.length, 40);
    Bytes.writeUInt32LE(Diagnostic.length, 44);
    Bytes.writeBigUInt64LE(inputTokens, 48);
    Bytes.writeBigUInt64LE(outputTokens, 56);
    Model.copy(Bytes, 64);
    Text.copy(Bytes, 64 + Model.length);
    Diagnostic.copy(Bytes, 64 + Model.length + Text.length);
    return Bytes;
}

function Failureˉresponse(Request, Status, Diagnostic) {
    const Common = { status: Status, requestId: Request.requestId, generation: 0n, diagnostic: Diagnostic };
    return Request.operation === "catalog" ? Encodeˉcatalogˉresponse(Common) : Encodeˉgenerationˉresponse(Common);
}

function Authˉheaders(Provider, Key) {
    const Headers = { accept: "application/json" };
    if (Provider === "openai") Headers.authorization = `Bearer ${Key}`;
    if (Provider === "anthropic") {
        Headers["x-api-key"] = Key;
        Headers["anthropic-version"] = "2023-06-01";
    }
    if (Provider === "google") Headers["x-goog-api-key"] = Key;
    return Headers;
}

function Requireˉobject(Value, Description) {
    if (Value === null || typeof Value !== "object" || Array.isArray(Value)) {
        Fail(`Provider ${Description} is malformed.`);
    }
    return Value;
}

function Tokenˉcount(Value) {
    if (Value === undefined) return 0n;
    if (!Number.isSafeInteger(Value) || Value < 0) Fail("Provider usage is malformed.");
    return BigInt(Value);
}

async function Readˉjson(Response) {
    const ContentType = Response.headers.get("content-type") ?? "";
    if (!/^application\/json(?:\s*;|$)/i.test(ContentType)) Fail("Provider returned non-JSON content.");
    if (!Response.body) Fail("Provider returned no body.");
    const Reader = Response.body.getReader();
    const Chunks = [];
    let Total = 0;
    while (true) {
        const { done, value } = await Reader.read();
        if (done) break;
        Total += value.byteLength;
        if (Total > MAX_RESPONSE_BODY_BYTES) {
            await Reader.cancel();
            Fail("Provider response exceeds the adapter limit.");
        }
        Chunks.push(Buffer.from(value));
    }
    let Value;
    try {
        Value = JSON.parse(Buffer.concat(Chunks, Total).toString("utf8"));
    } catch {
        Fail("Provider returned malformed JSON.");
    }
    return Requireˉobject(Value, "response");
}

function Httpˉfailure(Status) {
    if (Status === 400 || Status === 422) return [STATUS.InvalidRequest, "Provider rejected the request."];
    if (Status === 401 || Status === 403) return [STATUS.Unauthorized, "Provider authorization failed."];
    if (Status === 404) return [STATUS.Unsupported, "Provider model or operation is unsupported."];
    if (Status === 408 || Status === 429) return [STATUS.RateLimited, "Provider rate or time limit was reached."];
    if (Status >= 500) return [STATUS.Unavailable, "Provider is unavailable."];
    return [STATUS.ProviderError, "Provider returned an unsuccessful status."];
}

async function Invokeˉjson(Fetch, Url, Options, Signal) {
    const Response = await Fetch(Url, { ...Options, redirect: "error", signal: Signal });
    if (!Response || typeof Response.status !== "number" || !Response.headers) {
        Fail("Provider transport returned an invalid response.");
    }
    if (Response.status < 200 || Response.status > 299) {
        const [Status, Diagnostic] = Httpˉfailure(Response.status);
        throw new Referenceˉerror(Diagnostic, Status);
    }
    return Readˉjson(Response);
}

function Catalogˉentries(Provider, Json) {
    let Values;
    if (Provider === "openai" || Provider === "anthropic") Values = Json.data;
    if (Provider === "google") Values = Json.models;
    if (!Array.isArray(Values) || Values.length > MAX_CATALOG_ENTRIES) {
        Fail("Provider catalog is malformed or too large.");
    }
    const Seen = new Set();
    const Entries = [];
    for (const Raw of Values) {
        const Item = Requireˉobject(Raw, "catalog entry");
        if (Provider === "google" && Array.isArray(Item.supportedGenerationMethods) &&
            !Item.supportedGenerationMethods.includes("generateContent")) continue;
        let Id = Item.id;
        if (Provider === "google" && typeof Item.name === "string") {
            Id = Item.name.startsWith("models/") ? Item.name.slice(7) : Item.name;
        }
        if (typeof Id !== "string" || Id.length === 0 || Id.includes("\0") ||
            Buffer.byteLength(Id, "utf8") > MAX_MODEL_BYTES || Seen.has(Id)) continue;
        Seen.add(Id);
        const DisplayValue = Provider === "google" ? Item.displayName : Item.display_name;
        const Display = typeof DisplayValue === "string" && !DisplayValue.includes("\0") &&
            Buffer.byteLength(DisplayValue, "utf8") <= MAX_MODEL_BYTES ? DisplayValue : "";
        Entries.push({ id: Id, display: Display, features: Provider === "google" ? 3 : 0, lifecycle: 2 });
    }
    Entries.sort((Left, Right) => Buffer.from(Left.id).compare(Buffer.from(Right.id)));
    return Entries;
}

function Pageˉtoken(Value, Description) {
    if (typeof Value !== "string" || Value.length === 0 || Value.includes("\0") ||
        Buffer.byteLength(Value, "utf8") > MAX_CONTINUATION_BYTES) {
        Fail(`Provider ${Description} is malformed.`);
    }
    return Value;
}

async function Fetchˉcatalog(Fetch, Provider, Key, Signal) {
    let Url = PROVIDERS[Provider].catalogUrl;
    const Combined = [];
    for (let Page = 0; Page < 16; Page += 1) {
        const Json = await Invokeˉjson(Fetch, Url, {
            method: "GET", headers: Authˉheaders(Provider, Key),
        }, Signal);
        Combined.push(...Catalogˉentries(Provider, Json));
        if (Combined.length > MAX_CATALOG_ENTRIES) Fail("Provider catalog is too large.");
        let Next = "";
        if (Provider === "anthropic" && Json.has_more === true) {
            const LastId = Pageˉtoken(Json.last_id, "catalog cursor");
            Next = `https://api.anthropic.com/v1/models?limit=1000&after_id=${encodeURIComponent(LastId)}`;
        }
        if (Provider === "google" && typeof Json.nextPageToken === "string" &&
            Json.nextPageToken !== "") {
            const Token = Pageˉtoken(Json.nextPageToken, "catalog cursor");
            Next = `https://generativelanguage.googleapis.com/v1beta/models?pageSize=1000&pageToken=${encodeURIComponent(Token)}`;
        }
        if (Next === "") {
            const Unique = new Map();
            for (const Entry of Combined) if (!Unique.has(Entry.id)) Unique.set(Entry.id, Entry);
            return [...Unique.values()].sort(
                (Left, Right) => Buffer.from(Left.id).compare(Buffer.from(Right.id)),
            );
        }
        Url = Next;
    }
    Fail("Provider catalog pagination exceeds the adapter limit.");
}

function Continuationˉstate(Provider, Entries, Continuation) {
    const Identity = crypto.createHash("sha256").update(
        Entries.map(Entry => Entry.id).join("\0"), "utf8",
    ).digest("hex");
    if (Continuation === "") return { identity: Identity, offset: 0 };
    const Match = /^v1:(openai|anthropic|google):([0-9a-f]{64}):([1-9][0-9]*)$/.exec(Continuation);
    if (!Match || Match[1] !== Provider || Match[2] !== Identity) {
        Fail("Catalog continuation is stale.", STATUS.Stale);
    }
    const Offset = Number(Match[3]);
    if (!Number.isSafeInteger(Offset) || Offset < 1 || Offset >= Entries.length) {
        Fail("Catalog continuation is invalid.", STATUS.InvalidRequest);
    }
    return { identity: Identity, offset: Offset };
}

function Providerˉconversation(Messages) {
    let System = "";
    let Offset = 0;
    if (Messages[0].role === 1) {
        System = Messages[0].content;
        Offset = 1;
    }
    const Conversation = Messages.slice(Offset);
    if (Conversation.length === 0 || Conversation[0].role !== 2 ||
        Conversation[Conversation.length - 1].role !== 2) {
        Fail("Conversation must start and end with a user message.", STATUS.InvalidRequest);
    }
    for (let Index = 0; Index < Conversation.length; Index += 1) {
        const Expected = Index % 2 === 0 ? 2 : 3;
        if (Conversation[Index].role !== Expected) {
            Fail("Conversation roles must alternate.", STATUS.InvalidRequest);
        }
    }
    return { system: System, conversation: Conversation };
}

function Generationˉinvocation(Provider, Request, Key) {
    const Headers = { ...Authˉheaders(Provider, Key), "content-type": "application/json" };
    if (Provider === "openai") {
        return {
            url: "https://api.openai.com/v1/responses",
            options: { method: "POST", headers: Headers, body: JSON.stringify({
                model: Request.model,
                input: Request.messages.map(Message => ({
                    role: Message.role === 1 ? "system" : Message.role === 2 ? "user" : "assistant",
                    content: Message.content,
                })),
                max_output_tokens: Request.limit,
                store: false,
            }) },
        };
    }
    const { system, conversation } = Providerˉconversation(Request.messages);
    if (Provider === "anthropic") {
        const Body = {
            model: Request.model,
            max_tokens: Request.limit,
            messages: conversation.map(Message => ({
                role: Message.role === 2 ? "user" : "assistant", content: Message.content,
            })),
        };
        if (system !== "") Body.system = system;
        return {
            url: "https://api.anthropic.com/v1/messages",
            options: { method: "POST", headers: Headers, body: JSON.stringify(Body) },
        };
    }
    const Body = {
        contents: conversation.map(Message => ({
            role: Message.role === 2 ? "user" : "model",
            parts: [{ text: Message.content }],
        })),
        generationConfig: { maxOutputTokens: Request.limit },
    };
    if (system !== "") Body.system_instruction = { parts: [{ text: system }] };
    return {
        url: `https://generativelanguage.googleapis.com/v1beta/models/${encodeURIComponent(Request.model)}:generateContent`,
        options: { method: "POST", headers: Headers, body: JSON.stringify(Body) },
    };
}

function Openaiˉgeneration(Json, RequestedModel) {
    if (!Array.isArray(Json.output)) Fail("Provider generation response is malformed.");
    const Text = [];
    for (const ItemValue of Json.output) {
        const Item = Requireˉobject(ItemValue, "output item");
        if (Item.type === "reasoning") continue;
        if (Item.type !== "message" || !Array.isArray(Item.content)) {
            Fail("Provider returned unsupported output.", STATUS.Unsupported);
        }
        for (const PartValue of Item.content) {
            const Part = Requireˉobject(PartValue, "output part");
            if (Part.type === "refusal") {
                return { completion: 3, model: Json.model ?? RequestedModel, text: "", inputTokens: 0n, outputTokens: 0n };
            }
            if (Part.type !== "output_text" || typeof Part.text !== "string") {
                Fail("Provider returned unsupported output.", STATUS.Unsupported);
            }
            Text.push(Part.text);
        }
    }
    const Usage = Json.usage && typeof Json.usage === "object" ? Json.usage : {};
    const Reason = Json.incomplete_details && typeof Json.incomplete_details === "object" ?
        Json.incomplete_details.reason : "";
    if (Json.status !== "completed" && Json.status !== "incomplete") {
        Fail("Provider generation did not complete.");
    }
    const Completion = Json.status === "completed" ? 1 :
        Reason === "max_output_tokens" ? 2 : Reason === "content_filter" ? 3 : 0;
    if (Completion === 0) Fail("Provider returned an unsupported completion reason.", STATUS.Unsupported);
    return {
        completion: Completion,
        model: typeof Json.model === "string" ? Json.model : RequestedModel,
        text: Text.join(""), inputTokens: Tokenˉcount(Usage.input_tokens),
        outputTokens: Tokenˉcount(Usage.output_tokens),
    };
}

function Anthropicˉgeneration(Json, RequestedModel) {
    if (!Array.isArray(Json.content)) Fail("Provider generation response is malformed.");
    const Text = Json.content.map(Value => {
        const Part = Requireˉobject(Value, "content block");
        if (Part.type !== "text" || typeof Part.text !== "string") {
            Fail("Provider returned unsupported output.", STATUS.Unsupported);
        }
        return Part.text;
    }).join("");
    const Completions = { end_turn: 1, max_tokens: 2, refusal: 3 };
    const Completion = Completions[Json.stop_reason];
    if (!Completion) Fail("Provider returned an unsupported completion reason.", STATUS.Unsupported);
    const Usage = Json.usage && typeof Json.usage === "object" ? Json.usage : {};
    return {
        completion: Completion, model: typeof Json.model === "string" ? Json.model : RequestedModel,
        text: Text, inputTokens: Tokenˉcount(Usage.input_tokens), outputTokens: Tokenˉcount(Usage.output_tokens),
    };
}

function Googleˉgeneration(Json, RequestedModel) {
    if (!Array.isArray(Json.candidates) || Json.candidates.length !== 1) {
        Fail("Provider generation response must contain one candidate.");
    }
    const Candidate = Requireˉobject(Json.candidates[0], "candidate");
    const Content = Requireˉobject(Candidate.content, "candidate content");
    if (!Array.isArray(Content.parts)) Fail("Provider candidate parts are malformed.");
    const Text = Content.parts.map(Value => {
        const Part = Requireˉobject(Value, "candidate part");
        if (typeof Part.text !== "string" || Object.keys(Part).some(Key => Key !== "text")) {
            Fail("Provider returned unsupported output.", STATUS.Unsupported);
        }
        return Part.text;
    }).join("");
    const Reason = Candidate.finishReason;
    const Completion = Reason === "STOP" ? 1 : Reason === "MAX_TOKENS" ? 2 :
        ["SAFETY", "RECITATION", "BLOCKLIST", "PROHIBITED_CONTENT", "SPII"].includes(Reason) ? 3 : 0;
    if (Completion === 0) Fail("Provider returned an unsupported completion reason.", STATUS.Unsupported);
    const Usage = Json.usageMetadata && typeof Json.usageMetadata === "object" ? Json.usageMetadata : {};
    return {
        completion: Completion,
        model: typeof Json.modelVersion === "string" ? Json.modelVersion : RequestedModel,
        text: Text, inputTokens: Tokenˉcount(Usage.promptTokenCount),
        outputTokens: Tokenˉcount(Usage.candidatesTokenCount),
    };
}

function Validateˉkey(Key) {
    return typeof Key === "string" && Key.length > 0 && Key.length <= 16_384 &&
        !Key.includes("\0") && !Key.includes("\r") && !Key.includes("\n");
}

export async function Executeˉexternalˉmodelˉrequest({
    provider, requestBytes, generation, apiKey, fetchImplementation = globalThis.fetch, timeoutMilliseconds = 30_000,
}) {
    if (!Object.hasOwn(PROVIDERS, provider)) throw new Error("Unsupported provider name.");
    if (typeof generation !== "bigint" || generation < 1n) throw new Error("Provider generation must be nonzero.");
    const Request = Decodeˉmodelˉrequest(requestBytes);
    if (!Validateˉkey(apiKey)) return Failureˉresponse(Request, STATUS.Unauthorized, "Provider credential is unavailable.");
    if (typeof fetchImplementation !== "function") throw new Error("HTTPS fetch implementation is unavailable.");
    if (!Number.isSafeInteger(timeoutMilliseconds) || timeoutMilliseconds < 1 || timeoutMilliseconds > 300_000) {
        throw new Error("Timeout must be from 1 through 300000 milliseconds.");
    }
    if (Request.providerGeneration !== 0n && Request.providerGeneration !== generation) {
        return Failureˉresponse(Request, STATUS.Stale, "Provider generation is stale.");
    }
    const Controller = new AbortController();
    const Timer = setTimeout(() => Controller.abort(), timeoutMilliseconds);
    let DispatchBegan = false;
    try {
        if (Request.operation === "catalog") {
            DispatchBegan = true;
            const Entries = await Fetchˉcatalog(
                fetchImplementation, provider, apiKey, Controller.signal,
            );
            const State = Continuationˉstate(provider, Entries, Request.continuation);
            const Page = Entries.slice(State.offset, State.offset + Request.limit);
            const NextOffset = State.offset + Page.length;
            const Continuation = NextOffset < Entries.length ?
                `v1:${provider}:${State.identity}:${NextOffset}` : "";
            return Encodeˉcatalogˉresponse({
                requestId: Request.requestId, generation, entries: Page, continuation: Continuation,
            });
        }
        const Invocation = Generationˉinvocation(provider, Request, apiKey);
        DispatchBegan = true;
        const Json = await Invokeˉjson(
            fetchImplementation, Invocation.url, Invocation.options, Controller.signal,
        );
        const Result = provider === "openai" ? Openaiˉgeneration(Json, Request.model) :
            provider === "anthropic" ? Anthropicˉgeneration(Json, Request.model) :
                Googleˉgeneration(Json, Request.model);
        return Encodeˉgenerationˉresponse({
            requestId: Request.requestId, generation, ...Result,
        });
    } catch (Error) {
        if (Error instanceof Referenceˉerror) {
            return Failureˉresponse(Request, Error.status, Error.message);
        }
        const Indeterminate = Request.operation === "generate" && DispatchBegan;
        return Failureˉresponse(
            Request,
            Indeterminate ? STATUS.SubmissionIndeterminate : STATUS.Unavailable,
            Indeterminate ? "Generation submission outcome is indeterminate." : "Provider transport is unavailable.",
        );
    } finally {
        clearTimeout(Timer);
    }
}

export function Providerˉkeyˉenvironment(Provider) {
    if (!Object.hasOwn(PROVIDERS, Provider)) throw new Error("Unsupported provider name.");
    return PROVIDERS[Provider].keyEnvironment;
}

export { STATUS };
