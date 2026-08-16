import { createHash } from "node:crypto";

const UTF8 = new TextDecoder("utf-8", { fatal: true });
const MAX_REQUEST_BYTES = 65_536;
const MAX_RESPONSE_BODY_BYTES = 1_048_576;
const MAX_MODEL_BYTES = 256;
const MAX_MESSAGE_BYTES = 3_072;
const MAX_MESSAGE_SET_BYTES = 16_384;
const MAX_MESSAGES = 32;
const MAX_CATALOG_ENTRIES = 8_192;
const MAX_CONTINUATION_BYTES = 1_024;
const MAX_DIAGNOSTIC_BYTES = 1_024;
const MAX_OUTPUT_TOKENS = 4_096;

export const Modelˉgatewayˉstatus = Object.freeze({
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
        service: "api.openai.com",
        catalogTarget: "/v1/models",
        generationTarget: () => "/v1/responses",
        publicHeaders: Object.freeze([]),
    }),
    anthropic: Object.freeze({
        service: "api.anthropic.com",
        catalogTarget: "/v1/models?limit=1000",
        generationTarget: () => "/v1/messages",
        publicHeaders: Object.freeze([{ name: "anthropic-version", value: "2023-06-01" }]),
    }),
    google: Object.freeze({
        service: "generativelanguage.googleapis.com",
        catalogTarget: "/v1beta/models?pageSize=1000",
        generationTarget: Model =>
            `/v1beta/models/${encodeURIComponent(Model)}:generateContent`,
        publicHeaders: Object.freeze([]),
    }),
});

export class Externalˉmodelˉgatewayˉfailure extends Error {
    constructor(Kind, Message) {
        super(Message);
        this.kind = Kind;
    }
}

function Fail(Kind, Message) {
    throw new Externalˉmodelˉgatewayˉfailure(Kind, Message);
}

function Strictˉtext(Bytes, Description, Maximum, AllowEmpty = false) {
    if (Bytes.length > Maximum || (!AllowEmpty && Bytes.length === 0)) {
        Fail("invalid_request", `${Description} length is invalid.`);
    }
    let Value;
    try { Value = UTF8.decode(Bytes); } catch {
        Fail("invalid_request", `${Description} is not strict UTF-8.`);
    }
    if (Value.includes("\0")) Fail("invalid_request", `${Description} contains NUL.`);
    return Value;
}

function Checkedˉend(Offset, Length, Total, Description) {
    if (!Number.isSafeInteger(Length) || Offset > Total || Length > Total - Offset) {
        Fail("invalid_request", `${Description} exceeds its record.`);
    }
    return Offset + Length;
}

function Decodeˉmessages(Bytes) {
    if (Bytes.length < 16 || Bytes.length > MAX_MESSAGE_SET_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVMM" ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length) {
        Fail("invalid_request", "The message record is malformed.");
    }
    const Count = Bytes.readUInt32LE(12);
    if (Count < 1 || Count > MAX_MESSAGES) {
        Fail("invalid_request", "The message count is invalid.");
    }
    const Messages = [];
    let Offset = 16;
    for (let Index = 0; Index < Count; Index += 1) {
        const HeaderEnd = Checkedˉend(Offset, 8, Bytes.length, "Message header");
        const Role = Bytes.readUInt32LE(Offset);
        const Length = Bytes.readUInt32LE(Offset + 4);
        if (Role < 1 || Role > 3) Fail("invalid_request", "The message role is invalid.");
        const End = Checkedˉend(HeaderEnd, Length, Bytes.length, "Message content");
        Messages.push({
            role: Role,
            content: Strictˉtext(Bytes.subarray(HeaderEnd, End), "Message content", MAX_MESSAGE_BYTES),
        });
        Offset = End;
    }
    if (Offset !== Bytes.length) Fail("invalid_request", "The message record has trailing bytes.");
    return Messages;
}

export function Decodeˉgatewayˉmodelˉrequest(Value) {
    let Bytes;
    try { Bytes = Buffer.from(Value); } catch {
        Fail("invalid_request", "The model request is malformed.");
    }
    if (Bytes.length < 48 || Bytes.length > MAX_REQUEST_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVMQ" ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length ||
        Bytes.readUInt32LE(44) !== 0) {
        Fail("invalid_request", "The model request is malformed.");
    }
    const Operation = Bytes.readUInt32LE(12);
    const RequestId = Bytes.readBigUInt64LE(16);
    const ProviderGeneration = Bytes.readBigUInt64LE(24);
    const Limit = Bytes.readUInt32LE(32);
    const ModelLength = Bytes.readUInt32LE(36);
    const PayloadLength = Bytes.readUInt32LE(40);
    const ModelEnd = Checkedˉend(48, ModelLength, Bytes.length, "Model identifier");
    const PayloadEnd = Checkedˉend(ModelEnd, PayloadLength, Bytes.length, "Request payload");
    if (PayloadEnd !== Bytes.length) Fail("invalid_request", "The model request has trailing bytes.");
    const Model = Strictˉtext(
        Bytes.subarray(48, ModelEnd), "Model identifier", MAX_MODEL_BYTES, Operation === 1,
    );
    const Payload = Bytes.subarray(ModelEnd, PayloadEnd);
    if (Operation === 1) {
        if (ModelLength !== 0 || Limit < 1 || Limit > 128 ||
            Payload.length > MAX_CONTINUATION_BYTES ||
            (Payload.length !== 0 && ProviderGeneration === 0n)) {
            Fail("invalid_request", "The catalog request invariant is invalid.");
        }
        return Object.freeze({
            operation: "catalog", requestId: RequestId, providerGeneration: ProviderGeneration,
            limit: Limit,
            continuation: Strictˉtext(Payload, "Continuation", MAX_CONTINUATION_BYTES, true),
        });
    }
    if (Operation === 2) {
        if (ProviderGeneration === 0n || ModelLength === 0 || Limit < 1 ||
            Limit > MAX_OUTPUT_TOKENS || Payload.length === 0) {
            Fail("invalid_request", "The generation request invariant is invalid.");
        }
        return Object.freeze({
            operation: "generate", requestId: RequestId, providerGeneration: ProviderGeneration,
            limit: Limit, model: Model, messages: Decodeˉmessages(Payload),
        });
    }
    Fail("invalid_request", "The model operation is unsupported.");
}

function Providerˉbytes(Value, Description, Maximum, AllowEmpty = true) {
    if (typeof Value !== "string" || Value.includes("\0") || (!AllowEmpty && Value.length === 0)) {
        Fail("provider_output", `Provider ${Description} is malformed.`);
    }
    const Bytes = Buffer.from(Value, "utf8");
    if (Bytes.length > Maximum || Bytes.toString("utf8") !== Value) {
        Fail("provider_output", `Provider ${Description} is malformed or too large.`);
    }
    return Bytes;
}

function Diagnosticˉbytes(Value) {
    const Bytes = Buffer.from(String(Value), "utf8");
    return Bytes.length <= MAX_DIAGNOSTIC_BYTES ? Bytes : Buffer.from("Provider adapter failure.");
}

function Putˉheader(Bytes, Magic, Status, RequestId, Generation) {
    Bytes.write(Magic, 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(Status, 12);
    Bytes.writeBigUInt64LE(RequestId, 16);
    Bytes.writeBigUInt64LE(Generation, 24);
}

export function Encodeˉgatewayˉcatalogˉresponse({
    status = Modelˉgatewayˉstatus.Valid, requestId = 0n, generation = 0n,
    entries = [], continuation = "", diagnostic = "",
}) {
    const Continuation = Providerˉbytes(continuation, "continuation", MAX_CONTINUATION_BYTES);
    const Diagnostic = Diagnosticˉbytes(diagnostic);
    if (!Number.isInteger(status) || status < 0 || status > 11 ||
        (status === 0 && (generation === 0n || Diagnostic.length !== 0)) ||
        (status !== 0 && (entries.length !== 0 || Continuation.length !== 0))) {
        Fail("provider_output", "Catalog response invariant is invalid.");
    }
    const Encoded = entries.map(Entry => {
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
    const Total = 48 + Encoded.reduce((Sum, Entry) => Sum + Entry.length, 0) +
        Continuation.length + Diagnostic.length;
    if (entries.length > 128 || Total > 65_536) {
        Fail("provider_output", "Provider catalog page exceeds the protocol limit.");
    }
    const Bytes = Buffer.alloc(Total);
    Putˉheader(Bytes, "WVMC", status, requestId, generation);
    Bytes.writeUInt32LE(entries.length, 32);
    Bytes.writeUInt32LE(Continuation.length, 36);
    Bytes.writeUInt32LE(Diagnostic.length, 40);
    let Offset = 48;
    for (const Entry of Encoded) { Entry.copy(Bytes, Offset); Offset += Entry.length; }
    Continuation.copy(Bytes, Offset);
    Diagnostic.copy(Bytes, Offset + Continuation.length);
    return Bytes;
}

export function Encodeˉgatewayˉgenerationˉresponse({
    status = Modelˉgatewayˉstatus.Valid, requestId = 0n, generation = 0n,
    completion = 0, model = "", text = "", diagnostic = "",
    inputTokens = 0n, outputTokens = 0n,
}) {
    const Model = Providerˉbytes(model, "returned model", MAX_MODEL_BYTES);
    const Text = Providerˉbytes(text, "output text", MAX_RESPONSE_BODY_BYTES);
    const Diagnostic = Diagnosticˉbytes(diagnostic);
    if (!Number.isInteger(status) || status < 0 || status > 11 ||
        (status === 0 && (generation === 0n || completion < 1 || completion > 3 ||
            Model.length === 0 || Diagnostic.length !== 0)) ||
        (status !== 0 && (completion !== 0 || Model.length !== 0 || Text.length !== 0 ||
            inputTokens !== 0n || outputTokens !== 0n))) {
        Fail("provider_output", "Generation response invariant is invalid.");
    }
    const Total = 64 + Model.length + Text.length + Diagnostic.length;
    if (Total > MAX_RESPONSE_BODY_BYTES) {
        Fail("provider_output", "Generation response exceeds the protocol limit.");
    }
    const Bytes = Buffer.alloc(Total);
    Putˉheader(Bytes, "WVMG", status, requestId, generation);
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
    const Values = { status: Status, requestId: Request.requestId, diagnostic: Diagnostic };
    return Request.operation === "catalog" ? Encodeˉgatewayˉcatalogˉresponse(Values) :
        Encodeˉgatewayˉgenerationˉresponse(Values);
}

function Requireˉobject(Value, Description) {
    if (Value === null || typeof Value !== "object" || Array.isArray(Value)) {
        Fail("provider_output", `Provider ${Description} is malformed.`);
    }
    return Value;
}

function Tokenˉcount(Value) {
    if (Value === undefined) return 0n;
    if (!Number.isSafeInteger(Value) || Value < 0) {
        Fail("provider_output", "Provider usage is malformed.");
    }
    return BigInt(Value);
}

function Jsonˉbody(Result) {
    const ContentType = Result.headers.find(Header => Header.name === "content-type")?.value ?? "";
    if (!/^application\/json(?:\s*;|$)/i.test(ContentType)) {
        Fail("provider_output", "Provider returned non-JSON content.");
    }
    let Text;
    try { Text = UTF8.decode(Result.body); } catch {
        Fail("provider_output", "Provider JSON is not strict UTF-8.");
    }
    let Value;
    try { Value = JSON.parse(Text); } catch {
        Fail("provider_output", "Provider returned malformed JSON.");
    }
    return Requireˉobject(Value, "response");
}

function Httpˉfailure(Status) {
    if (Status === 400 || Status === 422) {
        return [Modelˉgatewayˉstatus.InvalidRequest, "Provider rejected the request."];
    }
    if (Status === 401 || Status === 403) {
        return [Modelˉgatewayˉstatus.Unauthorized, "Provider authorization failed."];
    }
    if (Status === 404) {
        return [Modelˉgatewayˉstatus.Unsupported, "Provider model or operation is unsupported."];
    }
    if (Status === 408 || Status === 429) {
        return [Modelˉgatewayˉstatus.RateLimited, "Provider rate or time limit was reached."];
    }
    if (Status >= 500) {
        return [Modelˉgatewayˉstatus.Unavailable, "Provider is unavailable."];
    }
    return [Modelˉgatewayˉstatus.ProviderError, "Provider returned an unsuccessful status."];
}

function Catalogˉentries(Provider, Json) {
    const Values = Provider === "google" ? Json.models : Json.data;
    if (!Array.isArray(Values) || Values.length > MAX_CATALOG_ENTRIES) {
        Fail("provider_output", "Provider catalog is malformed or too large.");
    }
    const Seen = new Set();
    const Entries = [];
    for (const Raw of Values) {
        const Item = Requireˉobject(Raw, "catalog entry");
        if (Provider === "google" && Array.isArray(Item.supportedGenerationMethods) &&
            !Item.supportedGenerationMethods.includes("generateContent")) continue;
        let Identifier = Item.id;
        if (Provider === "google" && typeof Item.name === "string") {
            Identifier = Item.name.startsWith("models/") ? Item.name.slice(7) : Item.name;
        }
        if (typeof Identifier !== "string" || Identifier.length === 0 || Identifier.includes("\0") ||
            Buffer.byteLength(Identifier, "utf8") > MAX_MODEL_BYTES || Seen.has(Identifier)) continue;
        Seen.add(Identifier);
        const DisplayValue = Provider === "google" ? Item.displayName : Item.display_name;
        const Display = typeof DisplayValue === "string" && !DisplayValue.includes("\0") &&
            Buffer.byteLength(DisplayValue, "utf8") <= MAX_MODEL_BYTES ? DisplayValue : "";
        Entries.push({
            id: Identifier, display: Display,
            features: Provider === "google" ? 3 : 0, lifecycle: 2,
        });
    }
    Entries.sort((Left, Right) => Buffer.from(Left.id).compare(Buffer.from(Right.id)));
    return Entries;
}

function Pageˉtoken(Value, Description) {
    if (typeof Value !== "string" || Value.length === 0 || Value.includes("\0") ||
        Buffer.byteLength(Value, "utf8") > MAX_CONTINUATION_BYTES) {
        Fail("provider_output", `Provider ${Description} is malformed.`);
    }
    return Value;
}

function Nextˉcatalogˉtarget(Provider, Json) {
    let Target = "";
    if (Provider === "anthropic" && Json.has_more === true) {
        Target = `/v1/models?limit=1000&after_id=${encodeURIComponent(
            Pageˉtoken(Json.last_id, "catalog cursor"),
        )}`;
    }
    if (Provider === "google" && typeof Json.nextPageToken === "string" &&
        Json.nextPageToken !== "") {
        Target = `/v1beta/models?pageSize=1000&pageToken=${encodeURIComponent(
            Pageˉtoken(Json.nextPageToken, "catalog cursor"),
        )}`;
    }
    if (Target.length > 2_048) Fail("provider_output", "Provider catalog cursor is too large.");
    return Target;
}

function Continuationˉstate(Provider, Entries, Continuation) {
    const Identity = createHash("sha256").update(
        Entries.map(Entry => Entry.id).join("\0"), "utf8",
    ).digest("hex");
    if (Continuation === "") return { identity: Identity, offset: 0 };
    const Match = /^v1:(openai|anthropic|google):([0-9a-f]{64}):([1-9][0-9]*)$/.exec(Continuation);
    if (!Match || Match[1] !== Provider || Match[2] !== Identity) {
        Fail("stale", "Catalog continuation is stale.");
    }
    const Offset = Number(Match[3]);
    if (!Number.isSafeInteger(Offset) || Offset < 1 || Offset >= Entries.length) {
        Fail("invalid_request", "Catalog continuation is invalid.");
    }
    return { identity: Identity, offset: Offset };
}

function Providerˉconversation(Messages) {
    let System = "";
    let Offset = 0;
    if (Messages[0].role === 1) { System = Messages[0].content; Offset = 1; }
    const Conversation = Messages.slice(Offset);
    if (Conversation.length === 0 || Conversation[0].role !== 2 ||
        Conversation.at(-1).role !== 2) {
        Fail("invalid_request", "Conversation must start and end with a user message.");
    }
    for (let Index = 0; Index < Conversation.length; Index += 1) {
        if (Conversation[Index].role !== (Index % 2 === 0 ? 2 : 3)) {
            Fail("invalid_request", "Conversation roles must alternate.");
        }
    }
    return { system: System, conversation: Conversation };
}

function Generationˉinvocation(Provider, Request) {
    const Profile = PROVIDERS[Provider];
    if (Provider === "openai") {
        return {
            target: Profile.generationTarget(Request.model),
            body: {
                model: Request.model,
                input: Request.messages.map(Message => ({
                    role: Message.role === 1 ? "system" : Message.role === 2 ? "user" : "assistant",
                    content: Message.content,
                })),
                max_output_tokens: Request.limit,
                store: false,
            },
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
        return { target: Profile.generationTarget(Request.model), body: Body };
    }
    const Body = {
        contents: conversation.map(Message => ({
            role: Message.role === 2 ? "user" : "model",
            parts: [{ text: Message.content }],
        })),
        generationConfig: { maxOutputTokens: Request.limit },
    };
    if (system !== "") Body.system_instruction = { parts: [{ text: system }] };
    return { target: Profile.generationTarget(Request.model), body: Body };
}

function Openaiˉgeneration(Json, RequestedModel) {
    if (!Array.isArray(Json.output)) Fail("provider_output", "Provider generation response is malformed.");
    const Text = [];
    for (const Raw of Json.output) {
        const Item = Requireˉobject(Raw, "output item");
        if (Item.type === "reasoning") continue;
        if (Item.type !== "message" || !Array.isArray(Item.content)) {
            Fail("unsupported", "Provider returned unsupported output.");
        }
        for (const RawPart of Item.content) {
            const Part = Requireˉobject(RawPart, "output content");
            if (Part.type !== "output_text" || typeof Part.text !== "string") {
                Fail("unsupported", "Provider returned unsupported output.");
            }
            Text.push(Part.text);
        }
    }
    const Usage = Json.usage && typeof Json.usage === "object" ? Json.usage : {};
    const Reason = Json.incomplete_details && typeof Json.incomplete_details === "object" ?
        Json.incomplete_details.reason : "";
    if (Json.status !== "completed" && Json.status !== "incomplete") {
        Fail("provider_output", "Provider generation did not complete.");
    }
    const Completion = Json.status === "completed" ? 1 :
        Reason === "max_output_tokens" ? 2 : Reason === "content_filter" ? 3 : 0;
    if (Completion === 0) Fail("unsupported", "Provider returned an unsupported completion reason.");
    return {
        completion: Completion,
        model: typeof Json.model === "string" ? Json.model : RequestedModel,
        text: Text.join(""), inputTokens: Tokenˉcount(Usage.input_tokens),
        outputTokens: Tokenˉcount(Usage.output_tokens),
    };
}

function Anthropicˉgeneration(Json, RequestedModel) {
    if (!Array.isArray(Json.content)) Fail("provider_output", "Provider generation response is malformed.");
    const Text = Json.content.map(Value => {
        const Part = Requireˉobject(Value, "content block");
        if (Part.type !== "text" || typeof Part.text !== "string") {
            Fail("unsupported", "Provider returned unsupported output.");
        }
        return Part.text;
    }).join("");
    const Completion = { end_turn: 1, max_tokens: 2, refusal: 3 }[Json.stop_reason];
    if (!Completion) Fail("unsupported", "Provider returned an unsupported completion reason.");
    const Usage = Json.usage && typeof Json.usage === "object" ? Json.usage : {};
    return {
        completion: Completion, model: typeof Json.model === "string" ? Json.model : RequestedModel,
        text: Text, inputTokens: Tokenˉcount(Usage.input_tokens),
        outputTokens: Tokenˉcount(Usage.output_tokens),
    };
}

function Googleˉgeneration(Json, RequestedModel) {
    if (!Array.isArray(Json.candidates) || Json.candidates.length !== 1) {
        Fail("provider_output", "Provider generation response must contain one candidate.");
    }
    const Candidate = Requireˉobject(Json.candidates[0], "candidate");
    const Content = Requireˉobject(Candidate.content, "candidate content");
    if (!Array.isArray(Content.parts)) Fail("provider_output", "Provider candidate parts are malformed.");
    const Text = Content.parts.map(Value => {
        const Part = Requireˉobject(Value, "candidate part");
        if (typeof Part.text !== "string" || Object.keys(Part).some(Key => Key !== "text")) {
            Fail("unsupported", "Provider returned unsupported output.");
        }
        return Part.text;
    }).join("");
    const Reason = Candidate.finishReason;
    const Completion = Reason === "STOP" ? 1 : Reason === "MAX_TOKENS" ? 2 :
        ["SAFETY", "RECITATION", "BLOCKLIST", "PROHIBITED_CONTENT", "SPII"].includes(Reason) ? 3 : 0;
    if (Completion === 0) Fail("unsupported", "Provider returned an unsupported completion reason.");
    const Usage = Json.usageMetadata && typeof Json.usageMetadata === "object" ? Json.usageMetadata : {};
    return {
        completion: Completion,
        model: typeof Json.modelVersion === "string" ? Json.modelVersion : RequestedModel,
        text: Text, inputTokens: Tokenˉcount(Usage.promptTokenCount),
        outputTokens: Tokenˉcount(Usage.candidatesTokenCount),
    };
}

function Failureˉstatus(Error, Operation, DispatchBegan) {
    if (Error?.kind === "revoked") return Modelˉgatewayˉstatus.Revoked;
    if (Error?.kind === "stale") return Modelˉgatewayˉstatus.Stale;
    if (Error?.kind === "invalid_request") return Modelˉgatewayˉstatus.InvalidRequest;
    if (Error?.kind === "unsupported") return Modelˉgatewayˉstatus.Unsupported;
    if (Error?.kind === "provider_output") return Modelˉgatewayˉstatus.ProviderError;
    if (Operation === "generate" && DispatchBegan) {
        return Modelˉgatewayˉstatus.SubmissionIndeterminate;
    }
    return Modelˉgatewayˉstatus.Unavailable;
}

function Failureˉdiagnostic(Status) {
    if (Status === Modelˉgatewayˉstatus.Revoked) return "Provider credential is revoked.";
    if (Status === Modelˉgatewayˉstatus.Stale) return "Provider generation is stale.";
    if (Status === Modelˉgatewayˉstatus.InvalidRequest) return "Model request is invalid.";
    if (Status === Modelˉgatewayˉstatus.Unsupported) return "Provider output is unsupported.";
    if (Status === Modelˉgatewayˉstatus.ProviderError) return "Provider output is invalid.";
    if (Status === Modelˉgatewayˉstatus.SubmissionIndeterminate) {
        return "Generation submission outcome is indeterminate.";
    }
    return "Provider transport is unavailable.";
}

export class Protectedˉexternalˉmodelˉgateway {
    constructor({ credentialLease, providerGeneration, ...Https }) {
        if (credentialLease === null || typeof credentialLease !== "object" ||
            typeof credentialLease.inspect !== "function" ||
            typeof credentialLease.bindHttps !== "function") {
            Fail("invalid_binding", "Protected credential lease is required.");
        }
        const Metadata = credentialLease.inspect();
        const Profile = PROVIDERS[Metadata.provider];
        if (!Profile || Metadata.service !== Profile.service || Metadata.port !== 443 ||
            Metadata.state !== "available" || typeof providerGeneration !== "bigint" ||
            providerGeneration < 1n) {
            Fail("invalid_binding", "External-model gateway binding is invalid.");
        }
        this.credentialLease = credentialLease;
        this.credentialGeneration = Metadata.generation;
        this.provider = Metadata.provider;
        this.providerGeneration = providerGeneration;
        this.profile = Profile;
        this.https = Object.freeze({ ...Https });
    }

    async #invoke(Target, Method, Body = Buffer.alloc(0)) {
        const Headers = [
            { name: "accept", value: "application/json" },
            ...this.profile.publicHeaders,
        ];
        if (Body.length !== 0) Headers.push({ name: "content-type", value: "application/json" });
        const AllowedHeaders = [...new Set(Headers.map(Header => Header.name))];
        const Client = this.credentialLease.bindHttps({
            ...this.https,
            allowedTargets: new Set([Target]),
            allowedHeaders: AllowedHeaders,
        });
        const Result = await Client.request({
            expectedCredentialGeneration: this.credentialGeneration,
            method: Method,
            target: Target,
            headers: Headers,
            ...(Body.length === 0 ? {} : { body: Body }),
        });
        if (Result.redirect || Result.status < 200 || Result.status > 299) {
            const [Status, Diagnostic] = Httpˉfailure(Result.status);
            return { failure: { status: Status, diagnostic: Diagnostic }, result: Result };
        }
        try { return { json: Jsonˉbody(Result), result: Result }; } catch (Error) {
            Result.body.fill(0);
            throw Error;
        }
    }

    async #catalog() {
        let Target = this.profile.catalogTarget;
        const Combined = [];
        for (let Page = 0; Page < 16; Page += 1) {
            const Invocation = await this.#invoke(Target, "GET");
            try {
                if (Invocation.failure) return Invocation.failure;
                Combined.push(...Catalogˉentries(this.provider, Invocation.json));
                if (Combined.length > MAX_CATALOG_ENTRIES) {
                    Fail("provider_output", "Provider catalog is too large.");
                }
                Target = Nextˉcatalogˉtarget(this.provider, Invocation.json);
                if (Target === "") {
                    const Unique = new Map();
                    for (const Entry of Combined) if (!Unique.has(Entry.id)) Unique.set(Entry.id, Entry);
                    return { entries: [...Unique.values()].sort(
                        (Left, Right) => Buffer.from(Left.id).compare(Buffer.from(Right.id)),
                    ) };
                }
            } finally {
                Invocation.result.body.fill(0);
            }
        }
        Fail("provider_output", "Provider catalog pagination exceeds its limit.");
    }

    async #generate(Request) {
        const Invocation = Generationˉinvocation(this.provider, Request);
        const Body = Buffer.from(JSON.stringify(Invocation.body), "utf8");
        try {
            const Response = await this.#invoke(Invocation.target, "POST", Body);
            try {
                if (Response.failure) return Response.failure;
                const Result = this.provider === "openai" ?
                    Openaiˉgeneration(Response.json, Request.model) :
                    this.provider === "anthropic" ?
                        Anthropicˉgeneration(Response.json, Request.model) :
                        Googleˉgeneration(Response.json, Request.model);
                return { generation: Result };
            } finally {
                Response.result.body.fill(0);
            }
        } finally {
            Body.fill(0);
        }
    }

    async execute(Value) {
        let Bytes;
        let Request;
        try {
            try { Bytes = Buffer.from(Value); } catch {
                Fail("invalid_request", "The model request is malformed.");
            }
            Request = Decodeˉgatewayˉmodelˉrequest(Bytes);
            if (Request.providerGeneration !== 0n &&
                Request.providerGeneration !== this.providerGeneration) {
                return Failureˉresponse(
                    Request, Modelˉgatewayˉstatus.Stale, "Provider generation is stale.",
                );
            }
            let DispatchBegan = false;
            try {
                if (Request.operation === "catalog") {
                    DispatchBegan = true;
                    const Catalog = await this.#catalog();
                    if (Catalog.status !== undefined) {
                        return Failureˉresponse(Request, Catalog.status, Catalog.diagnostic);
                    }
                    const State = Continuationˉstate(this.provider, Catalog.entries, Request.continuation);
                    const Page = Catalog.entries.slice(State.offset, State.offset + Request.limit);
                    const NextOffset = State.offset + Page.length;
                    const Continuation = NextOffset < Catalog.entries.length ?
                        `v1:${this.provider}:${State.identity}:${NextOffset}` : "";
                    return Encodeˉgatewayˉcatalogˉresponse({
                        requestId: Request.requestId, generation: this.providerGeneration,
                        entries: Page, continuation: Continuation,
                    });
                }
                DispatchBegan = true;
                const Generation = await this.#generate(Request);
                if (Generation.status !== undefined) {
                    return Failureˉresponse(Request, Generation.status, Generation.diagnostic);
                }
                return Encodeˉgatewayˉgenerationˉresponse({
                    requestId: Request.requestId, generation: this.providerGeneration,
                    ...Generation.generation,
                });
            } catch (Error) {
                const Status = Failureˉstatus(Error, Request.operation, DispatchBegan);
                return Failureˉresponse(Request, Status, Failureˉdiagnostic(Status));
            }
        } finally {
            Bytes?.fill(0);
        }
    }
}
