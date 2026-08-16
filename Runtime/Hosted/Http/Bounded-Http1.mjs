const TOKEN = /^[!#$%&'*+\-.^_`|~0-9A-Za-z]+$/;
const DECIMAL = /^(?:0|[1-9][0-9]*)$/;
const HEXADECIMAL = /^(?:0|[1-9A-Fa-f][0-9A-Fa-f]*)$/;

export class Boundedˉhttpˉfailure extends Error {
    constructor(Kind, Message) {
        super(Message);
        this.kind = Kind;
    }
}

function Fail(Kind, Message) {
    throw new Boundedˉhttpˉfailure(Kind, Message);
}

function Positiveˉinteger(Value, Maximum, Description) {
    if (!Number.isSafeInteger(Value) || Value < 1 || Value > Maximum) {
        Fail("invalid_request", `${Description} is invalid.`);
    }
    return Value;
}

function Canonicalˉservice(Value) {
    if (typeof Value !== "string" || Value.length < 1 || Value.length > 253 ||
        Buffer.byteLength(Value, "ascii") !== Value.length ||
        /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/.test(Value)) return false;
    return Value.split(".").every(Label => Label.length >= 1 && Label.length <= 63 &&
        /^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/.test(Label));
}

function Canonicalˉtarget(Value) {
    if (typeof Value !== "string" || Value.length < 1 || Value.length > 2_048 ||
        !Value.startsWith("/") || Value.includes("#") || Value.includes("\\") ||
        Buffer.byteLength(Value, "ascii") !== Value.length) return false;
    for (const Byte of Buffer.from(Value, "ascii")) {
        if (Byte < 0x21 || Byte > 0x7e) return false;
    }
    return true;
}

function Headerˉvalueˉbytes(Value) {
    let Bytes;
    if (typeof Value === "string") {
        if (Buffer.byteLength(Value, "ascii") !== Value.length) return null;
        Bytes = Buffer.from(Value, "ascii");
    } else if (Buffer.isBuffer(Value)) {
        Bytes = Value;
    } else if (Value instanceof Uint8Array) {
        Bytes = Buffer.from(Value.buffer, Value.byteOffset, Value.byteLength);
    } else return null;
    if (Bytes.length < 1 || Bytes.length > 4_096 || Bytes[0] === 0x20 || Bytes[0] === 0x09 ||
        Bytes.at(-1) === 0x20 || Bytes.at(-1) === 0x09) return null;
    for (const Byte of Bytes) {
        if (Byte !== 0x09 && (Byte < 0x20 || Byte > 0x7e)) return null;
    }
    return Bytes;
}

function Limits(Values = {}) {
    const MaximumHeaderBytes = Positiveˉinteger(
        Values.maximumHeaderBytes ?? 16_384, 65_536, "HTTP header-byte limit",
    );
    const MaximumBodyBytes = Positiveˉinteger(
        Values.maximumBodyBytes ?? 1_048_576, 16_777_216, "HTTP body-byte limit",
    );
    const MaximumWireBytes = Positiveˉinteger(
        Values.maximumWireBytes ?? MaximumHeaderBytes + MaximumBodyBytes + 65_536,
        33_554_432,
        "HTTP wire-byte limit",
    );
    if (MaximumWireBytes < MaximumHeaderBytes + MaximumBodyBytes) {
        Fail("invalid_request", "HTTP wire-byte limit is smaller than its content bounds.");
    }
    return Object.freeze({
        maximumHeaderBytes: MaximumHeaderBytes,
        maximumBodyBytes: MaximumBodyBytes,
        maximumWireBytes: MaximumWireBytes,
    });
}

export function Buildˉboundedˉhttp1ˉrequest({
    method,
    target,
    service,
    port,
    headers = [],
    body = Buffer.alloc(0),
    allowedTargets,
    allowedHeaders = ["accept", "content-type"],
    maximumRequestBytes = 65_536,
}) {
    if (!["GET", "POST"].includes(method) || !Canonicalˉtarget(target) ||
        !Canonicalˉservice(service) || !Number.isInteger(port) || port < 1 || port > 65_535 ||
        !(allowedTargets instanceof Set) || !allowedTargets.has(target) ||
        !Array.isArray(headers) || !Array.isArray(allowedHeaders)) {
        Fail("invalid_request", "HTTP request authority or target is invalid.");
    }
    const Payload = Buffer.from(body);
    const Maximum = Positiveˉinteger(maximumRequestBytes, 65_536, "HTTP request-byte limit");
    if ((method === "GET" && Payload.length !== 0) || Payload.length > Maximum) {
        Fail("limit", "HTTP request body exceeds its admitted method or limit.");
    }
    const Allowed = new Set(allowedHeaders);
    if (Allowed.size !== allowedHeaders.length ||
        [...Allowed].some(Name => typeof Name !== "string" || Name !== Name.toLowerCase() ||
            !TOKEN.test(Name) || ["host", "connection", "content-length", "transfer-encoding",
                "expect", "upgrade", "cookie"].includes(Name))) {
        Fail("invalid_request", "HTTP header authority is invalid.");
    }
    if (headers.length > 16) Fail("limit", "HTTP request has too many headers.");
    const Seen = new Set();
    const Parts = [
        Buffer.from(
            `${method} ${target} HTTP/1.1\r\n` +
            `Host: ${service}${port === 443 ? "" : `:${port}`}\r\n` +
            "Connection: close\r\n",
            "ascii",
        ),
    ];
    for (const Header of headers) {
        if (Header === null || typeof Header !== "object" ||
            typeof Header.name !== "string" || Header.name !== Header.name.toLowerCase() ||
            !TOKEN.test(Header.name) || !Allowed.has(Header.name) || Seen.has(Header.name)) {
            Fail("invalid_request", "HTTP request header is invalid or unauthorized.");
        }
        const Value = Headerˉvalueˉbytes(Header.value);
        if (Value === null) Fail("invalid_request", "HTTP request header value is invalid.");
        Seen.add(Header.name);
        Parts.push(Buffer.from(`${Header.name}: `, "ascii"), Value, Buffer.from("\r\n", "ascii"));
    }
    if (Payload.length !== 0 && !Seen.has("content-type")) {
        Fail("invalid_request", "HTTP request body requires an admitted content type.");
    }
    if (method === "POST") Parts.push(Buffer.from(`Content-Length: ${Payload.length}\r\n`, "ascii"));
    Parts.push(Buffer.from("\r\n", "ascii"));
    const Prefix = Buffer.concat(Parts);
    if (Prefix.length > 16_384 || Prefix.length + Payload.length > Maximum) {
        Fail("limit", "HTTP request exceeds its byte limit.");
    }
    return Buffer.concat([Prefix, Payload]);
}

function Parseˉheaders(Bytes, HeaderEnd, MaximumHeaderBytes) {
    if (HeaderEnd + 4 > MaximumHeaderBytes) Fail("limit", "HTTP response headers are too large.");
    const Text = Bytes.subarray(0, HeaderEnd).toString("latin1");
    const Lines = Text.split("\r\n");
    const Match = /^HTTP\/1\.1 ([0-9]{3})(?: ([\x20-\x7e]*))?$/.exec(Lines[0]);
    if (!Match || Match[2]?.length > 128) Fail("framing", "HTTP status line is invalid.");
    const Status = Number(Match[1]);
    if (Status < 200 || Status > 599) Fail("framing", "HTTP informational or invalid status is unsupported.");
    if (Lines.length - 1 > 64) Fail("limit", "HTTP response has too many fields.");
    const Headers = new Map();
    for (const Line of Lines.slice(1)) {
        const Colon = Line.indexOf(":");
        if (Colon <= 0 || Line.startsWith(" ") || Line.startsWith("\t")) {
            Fail("framing", "HTTP response field line is malformed.");
        }
        const OriginalName = Line.slice(0, Colon);
        const Name = OriginalName.toLowerCase();
        const Value = Line.slice(Colon + 1).replace(/^[ \t]+|[ \t]+$/g, "");
        if (!TOKEN.test(OriginalName) || Headers.has(Name) || Value.length > 4_096) {
            Fail("framing", "HTTP response field is invalid or repeated.");
        }
        for (const Byte of Buffer.from(Value, "latin1")) {
            if (Byte !== 0x09 && (Byte < 0x20 || Byte > 0x7e)) {
                Fail("framing", "HTTP response field contains unsupported bytes.");
            }
        }
        Headers.set(Name, Value);
    }
    if (Headers.has("set-cookie") || Headers.has("upgrade") ||
        (Headers.has("content-encoding") && Headers.get("content-encoding").toLowerCase() !== "identity")) {
        Fail("unsupported", "HTTP response requires unsupported state or content transformation.");
    }
    return { status: Status, reason: Match[2] ?? "", headers: Headers };
}

function Incomplete(PeerClosed, Message) {
    if (PeerClosed) Fail("truncated", Message);
    return null;
}

function Decodeˉchunked(Bytes, Offset, PeerClosed, MaximumBodyBytes) {
    const Chunks = [];
    let Total = 0;
    let Cursor = Offset;
    while (true) {
        const LineEnd = Bytes.indexOf("\r\n", Cursor, "ascii");
        if (LineEnd < 0) return Incomplete(PeerClosed, "HTTP chunk size is truncated.");
        if (LineEnd - Cursor < 1 || LineEnd - Cursor > 16) {
            Fail("framing", "HTTP chunk size is invalid.");
        }
        const Text = Bytes.subarray(Cursor, LineEnd).toString("ascii");
        if (!HEXADECIMAL.test(Text)) Fail("framing", "HTTP chunk extensions or size are invalid.");
        const Size = Number.parseInt(Text, 16);
        if (!Number.isSafeInteger(Size) || Size > MaximumBodyBytes - Total) {
            Fail("limit", "HTTP chunked body exceeds its limit.");
        }
        Cursor = LineEnd + 2;
        if (Size === 0) {
            if (Bytes.length < Cursor + 2) {
                return Incomplete(PeerClosed, "HTTP chunk terminator is truncated.");
            }
            if (Bytes[Cursor] !== 13 || Bytes[Cursor + 1] !== 10) {
                Fail("unsupported", "HTTP trailers are unsupported.");
            }
            return { body: Buffer.concat(Chunks, Total), end: Cursor + 2 };
        }
        if (Bytes.length < Cursor + Size + 2) {
            return Incomplete(PeerClosed, "HTTP chunk data is truncated.");
        }
        if (Bytes[Cursor + Size] !== 13 || Bytes[Cursor + Size + 1] !== 10) {
            Fail("framing", "HTTP chunk data terminator is invalid.");
        }
        Chunks.push(Bytes.subarray(Cursor, Cursor + Size));
        Total += Size;
        Cursor += Size + 2;
    }
}

function Selectˉframing(Parsed, MaximumBodyBytes) {
    const ContentLength = Parsed.headers.get("content-length");
    const TransferEncoding = Parsed.headers.get("transfer-encoding");
    if (ContentLength !== undefined && TransferEncoding !== undefined) {
        Fail("framing", "HTTP response has ambiguous length framing.");
    }
    const Bodyless = Parsed.status === 204 || Parsed.status === 304;
    if (Bodyless) {
        if (TransferEncoding !== undefined ||
            (ContentLength !== undefined && ContentLength !== "0")) {
            Fail("framing", "HTTP bodyless status carries body framing.");
        }
        return { kind: "bodyless", length: 0 };
    }
    if (ContentLength !== undefined) {
        if (!DECIMAL.test(ContentLength)) Fail("framing", "HTTP content length is invalid.");
        const Length = Number(ContentLength);
        if (!Number.isSafeInteger(Length) || Length > MaximumBodyBytes) {
            Fail("limit", "HTTP response body exceeds its limit.");
        }
        return { kind: "content-length", length: Length };
    }
    if (TransferEncoding !== undefined) {
        if (TransferEncoding.toLowerCase() !== "chunked") {
            Fail("unsupported", "HTTP transfer coding is unsupported.");
        }
        return { kind: "chunked", length: 0 };
    }
    Fail("framing", "HTTP response has no admitted body length framing.");
}

function Responseˉresult(Parsed, Body) {
    return Object.freeze({
        status: Parsed.status,
        reason: Parsed.reason,
        headers: Object.freeze([...Parsed.headers].map(([name, value]) => Object.freeze({ name, value }))),
        body: Buffer.from(Body),
        redirect: Parsed.status >= 300 && Parsed.status <= 399,
    });
}

export function Decodeˉboundedˉhttp1ˉresponse(Value, {
    peerClosed = false,
    maximumHeaderBytes,
    maximumBodyBytes,
    maximumWireBytes,
} = {}) {
    const Bytes = Buffer.from(Value);
    const Bound = Limits({ maximumHeaderBytes, maximumBodyBytes, maximumWireBytes });
    if (Bytes.length > Bound.maximumWireBytes) Fail("limit", "HTTP response exceeds its wire limit.");
    const HeaderEnd = Bytes.indexOf("\r\n\r\n", 0, "ascii");
    if (HeaderEnd < 0) {
        if (Bytes.length >= Bound.maximumHeaderBytes) Fail("limit", "HTTP response headers are too large.");
        return Incomplete(peerClosed, "HTTP response headers are truncated.");
    }
    const Parsed = Parseˉheaders(Bytes, HeaderEnd, Bound.maximumHeaderBytes);
    const BodyOffset = HeaderEnd + 4;
    const Framing = Selectˉframing(Parsed, Bound.maximumBodyBytes);
    let Body;
    let End;
    if (Framing.kind === "bodyless") {
        Body = Buffer.alloc(0);
        End = BodyOffset;
    } else if (Framing.kind === "content-length") {
        End = BodyOffset + Framing.length;
        if (Bytes.length < End) return Incomplete(peerClosed, "HTTP response body is truncated.");
        Body = Bytes.subarray(BodyOffset, End);
    } else {
        const Chunked = Decodeˉchunked(Bytes, BodyOffset, peerClosed, Bound.maximumBodyBytes);
        if (Chunked === null) return null;
        Body = Chunked.body;
        End = Chunked.end;
    }
    if (Bytes.length !== End) Fail("framing", "HTTP response contains excess bytes.");
    return Responseˉresult(Parsed, Body);
}

export class Boundedˉhttp1ˉresponseˉdecoder {
    #bodyChunks;
    #bodyLength;
    #chunkLine;
    #chunkRemaining;
    #complete;
    #expectedCrlf;
    #headerChunks;
    #headerMatch;
    #headerBytes;
    #parsed;
    #remaining;
    #state;
    #wireBytes;

    constructor(Values = {}) {
        this.limits = Limits(Values);
        this.#bodyChunks = [];
        this.#bodyLength = 0;
        this.#chunkLine = [];
        this.#chunkRemaining = 0;
        this.#complete = false;
        this.#expectedCrlf = 0;
        this.#headerChunks = [];
        this.#headerMatch = 0;
        this.#headerBytes = 0;
        this.#parsed = null;
        this.#remaining = 0;
        this.#state = "headers";
        this.#wireBytes = 0;
    }

    get receivedBytes() {
        return this.#wireBytes;
    }

    #Finish(Cursor, Length) {
        if (Cursor !== Length) Fail("framing", "HTTP response contains excess bytes.");
        this.#complete = true;
        return Responseˉresult(
            this.#parsed,
            Buffer.concat(this.#bodyChunks, this.#bodyLength),
        );
    }

    #Selectˉafterˉheaders(Chunk, Cursor) {
        const Header = Buffer.concat(this.#headerChunks, this.#headerBytes);
        this.#parsed = Parseˉheaders(Header, Header.length - 4, this.limits.maximumHeaderBytes);
        const Framing = Selectˉframing(this.#parsed, this.limits.maximumBodyBytes);
        this.#headerChunks = [];
        if (Framing.kind === "bodyless" ||
            (Framing.kind === "content-length" && Framing.length === 0)) {
            return this.#Finish(Cursor, Chunk.length);
        }
        if (Framing.kind === "content-length") {
            this.#state = "content-length";
            this.#remaining = Framing.length;
        } else {
            this.#state = "chunk-size";
        }
        return null;
    }

    #Process(Chunk, Start) {
        let Cursor = Start;
        while (Cursor < Chunk.length) {
            if (this.#state === "headers") {
                const Begin = Cursor;
                while (Cursor < Chunk.length && this.#headerMatch < 4) {
                    const Byte = Chunk[Cursor];
                    const Pattern = [13, 10, 13, 10];
                    if (Byte === Pattern[this.#headerMatch]) this.#headerMatch += 1;
                    else this.#headerMatch = Byte === 13 ? 1 : 0;
                    Cursor += 1;
                }
                const Part = Chunk.subarray(Begin, Cursor);
                this.#headerChunks.push(Part);
                this.#headerBytes += Part.length;
                if (this.#headerBytes > this.limits.maximumHeaderBytes) {
                    Fail("limit", "HTTP response headers are too large.");
                }
                if (this.#headerMatch === 4) {
                    const Result = this.#Selectˉafterˉheaders(Chunk, Cursor);
                    if (Result !== null) return Result;
                }
                continue;
            }
            if (this.#state === "content-length") {
                const Count = Math.min(this.#remaining, Chunk.length - Cursor);
                if (Count !== 0) {
                    this.#bodyChunks.push(Chunk.subarray(Cursor, Cursor + Count));
                    this.#bodyLength += Count;
                    this.#remaining -= Count;
                    Cursor += Count;
                }
                if (this.#remaining === 0) return this.#Finish(Cursor, Chunk.length);
                continue;
            }
            if (this.#state === "chunk-size") {
                const Byte = Chunk[Cursor];
                Cursor += 1;
                this.#chunkLine.push(Byte);
                if (this.#chunkLine.length > 18) Fail("framing", "HTTP chunk size is invalid.");
                if (Byte !== 10) continue;
                if (this.#chunkLine.length < 3 || this.#chunkLine.at(-2) !== 13) {
                    Fail("framing", "HTTP chunk size terminator is invalid.");
                }
                const Text = Buffer.from(this.#chunkLine.slice(0, -2)).toString("ascii");
                if (!HEXADECIMAL.test(Text)) {
                    Fail("framing", "HTTP chunk extensions or size are invalid.");
                }
                const Size = Number.parseInt(Text, 16);
                if (!Number.isSafeInteger(Size) || Size > this.limits.maximumBodyBytes - this.#bodyLength) {
                    Fail("limit", "HTTP chunked body exceeds its limit.");
                }
                this.#chunkLine = [];
                this.#chunkRemaining = Size;
                this.#expectedCrlf = 0;
                this.#state = Size === 0 ? "chunk-final-crlf" : "chunk-data";
                continue;
            }
            if (this.#state === "chunk-data") {
                const Count = Math.min(this.#chunkRemaining, Chunk.length - Cursor);
                if (Count !== 0) {
                    this.#bodyChunks.push(Chunk.subarray(Cursor, Cursor + Count));
                    this.#bodyLength += Count;
                    this.#chunkRemaining -= Count;
                    Cursor += Count;
                }
                if (this.#chunkRemaining === 0) {
                    this.#state = "chunk-data-crlf";
                    this.#expectedCrlf = 0;
                }
                continue;
            }
            const Expected = this.#expectedCrlf === 0 ? 13 : 10;
            if (Chunk[Cursor] !== Expected) {
                Fail(
                    this.#state === "chunk-final-crlf" ? "unsupported" : "framing",
                    this.#state === "chunk-final-crlf" ?
                        "HTTP trailers are unsupported." : "HTTP chunk data terminator is invalid.",
                );
            }
            Cursor += 1;
            this.#expectedCrlf += 1;
            if (this.#expectedCrlf === 2) {
                if (this.#state === "chunk-final-crlf") return this.#Finish(Cursor, Chunk.length);
                this.#state = "chunk-size";
                this.#expectedCrlf = 0;
            }
        }
        return null;
    }

    push(Value, PeerClosed = false) {
        if (this.#complete) Fail("framing", "HTTP response is already complete.");
        const Chunk = Buffer.from(Value);
        if (Chunk.length > this.limits.maximumWireBytes - this.#wireBytes) {
            Fail("limit", "HTTP response exceeds its wire limit.");
        }
        this.#wireBytes += Chunk.length;
        const Result = this.#Process(Chunk, 0);
        if (Result !== null) return Result;
        if (PeerClosed) Fail("truncated", "HTTP response is truncated.");
        return null;
    }
}
