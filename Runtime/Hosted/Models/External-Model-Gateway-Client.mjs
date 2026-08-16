const UTF8 = new TextDecoder("utf-8", { fatal: true });
const MAX_REQUEST_BYTES = 65_536;
const MAX_GENERATION_RESPONSE_BYTES = 1_048_576;
const MAX_MODEL_BYTES = 256;
const MAX_MESSAGE_BYTES = 3_072;
const MAX_MESSAGE_SET_BYTES = 16_384;
const MAX_MESSAGES = 32;
const MAX_CONTINUATION_BYTES = 1_024;
const MAX_DIAGNOSTIC_BYTES = 1_024;

export const Modelˉclientˉstatus = Object.freeze({
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

export const Modelˉclientˉstatusˉname = Object.freeze([
    "valid",
    "invalid_request",
    "unavailable",
    "unauthorized",
    "rate_limited",
    "unsupported",
    "provider_error",
    "cancelled",
    "revoked",
    "stale",
    "peer_exited",
    "submission_indeterminate",
]);

export class Externalˉmodelˉclientˉfailure extends Error {
    constructor(Kind, Message) {
        super(Message);
        this.kind = Kind;
    }
}

function Fail(Kind, Message) {
    throw new Externalˉmodelˉclientˉfailure(Kind, Message);
}

function U64(Value, Description, AllowZero = false) {
    if (typeof Value !== "bigint" || Value < (AllowZero ? 0n : 1n) ||
        Value > 0xffff_ffff_ffff_ffffn) {
        Fail("invalid_request", `${Description} is invalid.`);
    }
    return Value;
}

function U32(Value, Minimum, Maximum, Description) {
    if (!Number.isInteger(Value) || Value < Minimum || Value > Maximum) {
        Fail("invalid_request", `${Description} is invalid.`);
    }
    return Value;
}

function Textˉbytes(Value, Description, Maximum, AllowEmpty = false) {
    if (typeof Value !== "string" || Value.includes("\0") || (!AllowEmpty && Value.length === 0)) {
        Fail("invalid_request", `${Description} is invalid.`);
    }
    const Bytes = Buffer.from(Value, "utf8");
    if (Bytes.length > Maximum || Bytes.toString("utf8") !== Value) {
        Fail("invalid_request", `${Description} is invalid.`);
    }
    return Bytes;
}

function Strictˉtext(Bytes, Description, Maximum, AllowEmpty = true) {
    if (Bytes.length > Maximum || (!AllowEmpty && Bytes.length === 0)) {
        Fail("invalid_response", `${Description} length is invalid.`);
    }
    let Value;
    try { Value = UTF8.decode(Bytes); } catch {
        Fail("invalid_response", `${Description} is not strict UTF-8.`);
    }
    if (Value.includes("\0")) Fail("invalid_response", `${Description} contains NUL.`);
    return Value;
}

function Checkedˉend(Offset, Length, Total, Description) {
    if (!Number.isSafeInteger(Length) || Offset > Total || Length > Total - Offset) {
        Fail("invalid_response", `${Description} exceeds its record.`);
    }
    return Offset + Length;
}

function Messageˉrole(Value) {
    const Role = Value === "system" ? 1 : Value === "user" ? 2 : Value === "assistant" ? 3 : 0;
    if (Role === 0) Fail("invalid_request", "Message role is invalid.");
    return Role;
}

function Encodeˉmessages(Values) {
    if (!Array.isArray(Values) || Values.length < 1 || Values.length > MAX_MESSAGES) {
        Fail("invalid_request", "Message count is invalid.");
    }
    const Records = Values.map(Value => {
        if (Value === null || typeof Value !== "object") {
            Fail("invalid_request", "Message is invalid.");
        }
        const Content = Textˉbytes(Value.content, "Message content", MAX_MESSAGE_BYTES);
        const Bytes = Buffer.alloc(8 + Content.length);
        Bytes.writeUInt32LE(Messageˉrole(Value.role), 0);
        Bytes.writeUInt32LE(Content.length, 4);
        Content.copy(Bytes, 8);
        return Bytes;
    });
    const Total = 16 + Records.reduce((Sum, Value) => Sum + Value.length, 0);
    if (Total > MAX_MESSAGE_SET_BYTES) {
        Fail("invalid_request", "Message set exceeds its byte limit.");
    }
    const Bytes = Buffer.alloc(Total);
    Bytes.write("WVMM", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Total, 8);
    Bytes.writeUInt32LE(Records.length, 12);
    let Offset = 16;
    for (const Record of Records) {
        Record.copy(Bytes, Offset);
        Offset += Record.length;
    }
    return Bytes;
}

function Encodeˉrequest({ operation, requestId, providerGeneration, limit, model, payload }) {
    const Total = 48 + model.length + payload.length;
    if (Total > MAX_REQUEST_BYTES) Fail("invalid_request", "Model request exceeds its byte limit.");
    const Bytes = Buffer.alloc(Total);
    Bytes.write("WVMQ", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Total, 8);
    Bytes.writeUInt32LE(operation, 12);
    Bytes.writeBigUInt64LE(requestId, 16);
    Bytes.writeBigUInt64LE(providerGeneration, 24);
    Bytes.writeUInt32LE(limit, 32);
    Bytes.writeUInt32LE(model.length, 36);
    Bytes.writeUInt32LE(payload.length, 40);
    model.copy(Bytes, 48);
    payload.copy(Bytes, 48 + model.length);
    return Bytes;
}

export function Encodeˉgatewayˉcatalogˉrequest({
    requestId,
    providerGeneration = 0n,
    limit = 128,
    continuation = "",
}) {
    const Continuation = Textˉbytes(
        continuation, "Catalog continuation", MAX_CONTINUATION_BYTES, true,
    );
    const Generation = U64(providerGeneration, "Provider generation", true);
    if (Continuation.length !== 0 && Generation === 0n) {
        Fail("invalid_request", "A continued catalog request requires its provider generation.");
    }
    return Encodeˉrequest({
        operation: 1,
        requestId: U64(requestId, "Request identity", true),
        providerGeneration: Generation,
        limit: U32(limit, 1, 128, "Catalog page size"),
        model: Buffer.alloc(0),
        payload: Continuation,
    });
}

export function Encodeˉgatewayˉgenerationˉrequest({
    requestId,
    providerGeneration,
    maximumOutputTokens,
    model,
    messages,
}) {
    return Encodeˉrequest({
        operation: 2,
        requestId: U64(requestId, "Request identity", true),
        providerGeneration: U64(providerGeneration, "Provider generation"),
        limit: U32(maximumOutputTokens, 1, 4_096, "Maximum output tokens"),
        model: Textˉbytes(model, "Model identifier", MAX_MODEL_BYTES),
        payload: Encodeˉmessages(messages),
    });
}

function Commonˉresponse(Value, Magic, Minimum, Maximum, ExpectedRequestId) {
    let Bytes;
    try { Bytes = Buffer.from(Value); } catch {
        Fail("invalid_response", "Model response is malformed.");
    }
    if (Bytes.length < Minimum || Bytes.length > Maximum ||
        Bytes.subarray(0, 4).toString("ascii") !== Magic ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length) {
        Fail("invalid_response", "Model response is malformed.");
    }
    const Status = Bytes.readUInt32LE(12);
    const RequestId = Bytes.readBigUInt64LE(16);
    const Generation = Bytes.readBigUInt64LE(24);
    if (Status > Modelˉclientˉstatus.SubmissionIndeterminate ||
        (ExpectedRequestId !== undefined && RequestId !== ExpectedRequestId)) {
        Fail("invalid_response", "Model response identity or status is invalid.");
    }
    return { bytes: Bytes, status: Status, requestId: RequestId, generation: Generation };
}

export function Decodeˉgatewayˉcatalogˉresponse(Value, ExpectedRequestId = undefined) {
    const Common = Commonˉresponse(Value, "WVMC", 48, 65_536, ExpectedRequestId);
    const { bytes: Bytes, status: Status, generation: Generation } = Common;
    if (Bytes.readUInt32LE(44) !== 0) Fail("invalid_response", "Catalog response is malformed.");
    const Count = Bytes.readUInt32LE(32);
    const ContinuationLength = Bytes.readUInt32LE(36);
    const DiagnosticLength = Bytes.readUInt32LE(40);
    if (Count > 128) Fail("invalid_response", "Catalog entry count is invalid.");
    const Entries = [];
    let Offset = 48;
    for (let Index = 0; Index < Count; Index += 1) {
        const HeaderEnd = Checkedˉend(Offset, 20, Bytes.length, "Catalog entry header");
        const EntryLength = Bytes.readUInt32LE(Offset);
        const IdentifierLength = Bytes.readUInt32LE(Offset + 4);
        const DisplayLength = Bytes.readUInt32LE(Offset + 8);
        const Features = Bytes.readUInt32LE(Offset + 12);
        const Lifecycle = Bytes.readUInt32LE(Offset + 16);
        const EntryEnd = Checkedˉend(Offset, EntryLength, Bytes.length, "Catalog entry");
        if (EntryLength !== 20 + IdentifierLength + DisplayLength || Lifecycle < 1 || Lifecycle > 3) {
            Fail("invalid_response", "Catalog entry is malformed.");
        }
        const IdentifierEnd = Checkedˉend(HeaderEnd, IdentifierLength, EntryEnd, "Model identifier");
        Entries.push(Object.freeze({
            id: Strictˉtext(Bytes.subarray(HeaderEnd, IdentifierEnd), "Model identifier", MAX_MODEL_BYTES, false),
            display: Strictˉtext(Bytes.subarray(IdentifierEnd, EntryEnd), "Model display", MAX_MODEL_BYTES),
            features: Features,
            lifecycle: Lifecycle,
        }));
        Offset = EntryEnd;
    }
    const ContinuationEnd = Checkedˉend(
        Offset, ContinuationLength, Bytes.length, "Catalog continuation",
    );
    const DiagnosticEnd = Checkedˉend(
        ContinuationEnd, DiagnosticLength, Bytes.length, "Catalog diagnostic",
    );
    if (DiagnosticEnd !== Bytes.length ||
        (Status === 0 && (Generation === 0n || DiagnosticLength !== 0)) ||
        (Status !== 0 && (Generation !== 0n || Count !== 0 || ContinuationLength !== 0))) {
        Fail("invalid_response", "Catalog response invariant is invalid.");
    }
    return Object.freeze({
        status: Status,
        statusName: Modelˉclientˉstatusˉname[Status],
        requestId: Common.requestId,
        generation: Generation,
        entries: Object.freeze(Entries),
        continuation: Strictˉtext(
            Bytes.subarray(Offset, ContinuationEnd), "Catalog continuation", MAX_CONTINUATION_BYTES,
        ),
        diagnostic: Strictˉtext(
            Bytes.subarray(ContinuationEnd, DiagnosticEnd), "Catalog diagnostic", MAX_DIAGNOSTIC_BYTES,
        ),
    });
}

export function Decodeˉgatewayˉgenerationˉresponse(Value, ExpectedRequestId = undefined) {
    const Common = Commonˉresponse(
        Value, "WVMG", 64, MAX_GENERATION_RESPONSE_BYTES, ExpectedRequestId,
    );
    const { bytes: Bytes, status: Status, generation: Generation } = Common;
    const Completion = Bytes.readUInt32LE(32);
    const ModelLength = Bytes.readUInt32LE(36);
    const TextLength = Bytes.readUInt32LE(40);
    const DiagnosticLength = Bytes.readUInt32LE(44);
    const ModelEnd = Checkedˉend(64, ModelLength, Bytes.length, "Returned model");
    const TextEnd = Checkedˉend(ModelEnd, TextLength, Bytes.length, "Model output");
    const DiagnosticEnd = Checkedˉend(TextEnd, DiagnosticLength, Bytes.length, "Model diagnostic");
    const InputTokens = Bytes.readBigUInt64LE(48);
    const OutputTokens = Bytes.readBigUInt64LE(56);
    if (DiagnosticEnd !== Bytes.length ||
        (Status === 0 && (Generation === 0n || Completion < 1 || Completion > 3 ||
            ModelLength === 0 || DiagnosticLength !== 0)) ||
        (Status !== 0 && (Generation !== 0n || Completion !== 0 || ModelLength !== 0 ||
            TextLength !== 0 || InputTokens !== 0n || OutputTokens !== 0n))) {
        Fail("invalid_response", "Generation response invariant is invalid.");
    }
    return Object.freeze({
        status: Status,
        statusName: Modelˉclientˉstatusˉname[Status],
        requestId: Common.requestId,
        generation: Generation,
        completion: Completion,
        model: Strictˉtext(Bytes.subarray(64, ModelEnd), "Returned model", MAX_MODEL_BYTES),
        text: Strictˉtext(Bytes.subarray(ModelEnd, TextEnd), "Model output", 1_048_512),
        diagnostic: Strictˉtext(
            Bytes.subarray(TextEnd, DiagnosticEnd), "Model diagnostic", MAX_DIAGNOSTIC_BYTES,
        ),
        inputTokens: InputTokens,
        outputTokens: OutputTokens,
    });
}
