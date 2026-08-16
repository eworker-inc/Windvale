const UTF8 = new TextDecoder("utf-8", { fatal: true });

export const HOST_NETWORK_REQUEST_HEADER_BYTES = 72;
export const HOST_NETWORK_RESPONSE_HEADER_BYTES = 80;
export const HOST_NETWORK_MAX_REQUEST_BYTES = 131_072;
export const HOST_NETWORK_MAX_RESPONSE_BYTES = 131_072;

export const Hostˉnetworkˉoperation = Object.freeze({
    Connect: 1,
    Write: 2,
    Read: 3,
    ShutdownWrite: 4,
    Close: 5,
});

export const Hostˉnetworkˉstatus = Object.freeze({
    Valid: 0,
    InvalidRequest: 1,
    Unauthorized: 2,
    Stale: 3,
    Expired: 4,
    Unavailable: 5,
    Limit: 6,
    Reset: 7,
    PeerClosed: 8,
    SubmissionIndeterminate: 9,
    ProviderLost: 10,
    Cancelled: 11,
});

function Fail(Message) {
    throw new Error(Message);
}

function Checkedˉend(Offset, Length, Total, Description) {
    if (!Number.isSafeInteger(Length) || Length < 0 || Offset < 0 || Offset > Total ||
        Length > Total - Offset) Fail(`${Description} exceeds its record.`);
    return Offset + Length;
}

function Strictˉtext(Bytes, Description, Maximum, AllowEmpty = true) {
    if (Bytes.length > Maximum || (!AllowEmpty && Bytes.length === 0)) {
        Fail(`${Description} length is invalid.`);
    }
    let Value;
    try { Value = UTF8.decode(Bytes); } catch { Fail(`${Description} is not strict UTF-8.`); }
    if (Value.includes("\0")) Fail(`${Description} contains NUL.`);
    return Value;
}

function Textˉbytes(Value, Description, Maximum, AllowEmpty = true) {
    if (typeof Value !== "string" || Value.includes("\0") || (!AllowEmpty && Value.length === 0)) {
        Fail(`${Description} is invalid.`);
    }
    const Bytes = Buffer.from(Value, "utf8");
    if (Bytes.length > Maximum || Bytes.toString("utf8") !== Value) {
        Fail(`${Description} is invalid.`);
    }
    return Bytes;
}

export function Encodeˉhostˉnetworkˉrequest({
    operation,
    requestId,
    providerGeneration,
    connectionId = 0n,
    connectionGeneration = 0n,
    deadlineNanoseconds,
    control = 0,
    service = "",
    payload = Buffer.alloc(0),
}) {
    if (!Number.isInteger(operation) || operation < 1 || operation > 5 ||
        typeof requestId !== "bigint" || requestId === 0n ||
        typeof providerGeneration !== "bigint" || providerGeneration === 0n ||
        typeof connectionId !== "bigint" || typeof connectionGeneration !== "bigint" ||
        typeof deadlineNanoseconds !== "bigint" || deadlineNanoseconds === 0n ||
        !Number.isInteger(control) || control < 0 || control > 0xffff_ffff) {
        Fail("Host-network request scalar is invalid.");
    }
    const Service = Textˉbytes(service, "Service name", 253);
    const Payload = Buffer.from(payload);
    if (Payload.length > 65_536) Fail("Host-network request payload is too large.");
    if ((operation === Hostˉnetworkˉoperation.Connect &&
            (connectionId !== 0n || connectionGeneration !== 0n || control < 1 || control > 65_535 ||
                Service.length === 0 || Payload.length !== 0)) ||
        (operation === Hostˉnetworkˉoperation.Write &&
            (connectionId === 0n || connectionGeneration === 0n || control !== 0 ||
                Service.length !== 0 || Payload.length === 0)) ||
        (operation === Hostˉnetworkˉoperation.Read &&
            (connectionId === 0n || connectionGeneration === 0n || control < 1 || control > 65_536 ||
                Service.length !== 0 || Payload.length !== 0)) ||
        ((operation === Hostˉnetworkˉoperation.ShutdownWrite ||
            operation === Hostˉnetworkˉoperation.Close) &&
            (connectionId === 0n || connectionGeneration === 0n || control !== 0 ||
                Service.length !== 0 || Payload.length !== 0))) {
        Fail("Host-network request operation invariant is invalid.");
    }
    const Total = HOST_NETWORK_REQUEST_HEADER_BYTES + Service.length + Payload.length;
    const Bytes = Buffer.alloc(Total);
    Bytes.write("WVNR", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Total, 8);
    Bytes.writeUInt32LE(operation, 12);
    Bytes.writeBigUInt64LE(requestId, 16);
    Bytes.writeBigUInt64LE(providerGeneration, 24);
    Bytes.writeBigUInt64LE(connectionId, 32);
    Bytes.writeBigUInt64LE(connectionGeneration, 40);
    Bytes.writeBigUInt64LE(deadlineNanoseconds, 48);
    Bytes.writeUInt32LE(control, 56);
    Bytes.writeUInt32LE(Service.length, 60);
    Bytes.writeUInt32LE(Payload.length, 64);
    Service.copy(Bytes, HOST_NETWORK_REQUEST_HEADER_BYTES);
    Payload.copy(Bytes, HOST_NETWORK_REQUEST_HEADER_BYTES + Service.length);
    return Bytes;
}

export function Decodeˉhostˉnetworkˉrequest(Value) {
    const Bytes = Buffer.from(Value);
    if (Bytes.length < HOST_NETWORK_REQUEST_HEADER_BYTES ||
        Bytes.length > HOST_NETWORK_MAX_REQUEST_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVNR" ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length ||
        Bytes.readUInt32LE(68) !== 0) Fail("Host-network request header is malformed.");
    const Operation = Bytes.readUInt32LE(12);
    const ServiceLength = Bytes.readUInt32LE(60);
    const PayloadLength = Bytes.readUInt32LE(64);
    const ServiceEnd = Checkedˉend(
        HOST_NETWORK_REQUEST_HEADER_BYTES, ServiceLength, Bytes.length, "Service name",
    );
    const PayloadEnd = Checkedˉend(ServiceEnd, PayloadLength, Bytes.length, "Request payload");
    if (PayloadEnd !== Bytes.length) Fail("Host-network request has trailing bytes.");
    const Request = {
        operation: Operation,
        requestId: Bytes.readBigUInt64LE(16),
        providerGeneration: Bytes.readBigUInt64LE(24),
        connectionId: Bytes.readBigUInt64LE(32),
        connectionGeneration: Bytes.readBigUInt64LE(40),
        deadlineNanoseconds: Bytes.readBigUInt64LE(48),
        control: Bytes.readUInt32LE(56),
        service: Strictˉtext(
            Bytes.subarray(HOST_NETWORK_REQUEST_HEADER_BYTES, ServiceEnd), "Service name", 253,
        ),
        payload: Bytes.subarray(ServiceEnd, PayloadEnd),
    };
    return Encodeˉhostˉnetworkˉrequest(Request).equals(Bytes) ?
        Request : Fail("Host-network request is not canonical.");
}

export function Encodeˉhostˉnetworkˉresponse({
    status,
    requestId,
    providerGeneration,
    connectionId = 0n,
    connectionGeneration = 0n,
    progress = 0n,
    endpointPort = 0,
    endpointFamily = 0,
    flags = 0,
    address = "",
    payload = Buffer.alloc(0),
    diagnostic = "",
}) {
    if (!Number.isInteger(status) || status < 0 || status > 11 ||
        typeof requestId !== "bigint" || requestId === 0n ||
        typeof providerGeneration !== "bigint" || providerGeneration === 0n ||
        typeof connectionId !== "bigint" || typeof connectionGeneration !== "bigint" ||
        typeof progress !== "bigint" || progress < 0n || progress > 65_536n ||
        !Number.isInteger(endpointPort) || endpointPort < 0 || endpointPort > 65_535 ||
        ![0, 4, 6].includes(endpointFamily) || !Number.isInteger(flags) || flags < 0 || flags > 3) {
        Fail("Host-network response scalar is invalid.");
    }
    const Address = Textˉbytes(address, "Endpoint address", 64);
    const Payload = Buffer.from(payload);
    const Diagnostic = Textˉbytes(diagnostic, "Diagnostic", 256);
    if (Payload.length > 65_536) Fail("Host-network response payload is too large.");
    if ((status === Hostˉnetworkˉstatus.Valid && Diagnostic.length !== 0) ||
        (status !== Hostˉnetworkˉstatus.Valid &&
            (progress !== 0n || Payload.length !== 0 || Address.length !== 0 || endpointPort !== 0 ||
                endpointFamily !== 0))) {
        Fail("Host-network response status invariant is invalid.");
    }
    const Total = HOST_NETWORK_RESPONSE_HEADER_BYTES + Address.length + Payload.length + Diagnostic.length;
    if (Total > HOST_NETWORK_MAX_RESPONSE_BYTES) Fail("Host-network response is too large.");
    const Bytes = Buffer.alloc(Total);
    Bytes.write("WVNS", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Total, 8);
    Bytes.writeUInt32LE(status, 12);
    Bytes.writeBigUInt64LE(requestId, 16);
    Bytes.writeBigUInt64LE(providerGeneration, 24);
    Bytes.writeBigUInt64LE(connectionId, 32);
    Bytes.writeBigUInt64LE(connectionGeneration, 40);
    Bytes.writeBigUInt64LE(progress, 48);
    Bytes.writeUInt32LE(endpointPort, 56);
    Bytes.writeUInt32LE(endpointFamily, 60);
    Bytes.writeUInt32LE(flags, 64);
    Bytes.writeUInt32LE(Address.length, 68);
    Bytes.writeUInt32LE(Payload.length, 72);
    Bytes.writeUInt32LE(Diagnostic.length, 76);
    Address.copy(Bytes, HOST_NETWORK_RESPONSE_HEADER_BYTES);
    Payload.copy(Bytes, HOST_NETWORK_RESPONSE_HEADER_BYTES + Address.length);
    Diagnostic.copy(Bytes, HOST_NETWORK_RESPONSE_HEADER_BYTES + Address.length + Payload.length);
    return Bytes;
}

export function Decodeˉhostˉnetworkˉresponse(Value) {
    const Bytes = Buffer.from(Value);
    if (Bytes.length < HOST_NETWORK_RESPONSE_HEADER_BYTES ||
        Bytes.length > HOST_NETWORK_MAX_RESPONSE_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVNS" ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length) {
        Fail("Host-network response header is malformed.");
    }
    const AddressLength = Bytes.readUInt32LE(68);
    const PayloadLength = Bytes.readUInt32LE(72);
    const DiagnosticLength = Bytes.readUInt32LE(76);
    const AddressEnd = Checkedˉend(
        HOST_NETWORK_RESPONSE_HEADER_BYTES, AddressLength, Bytes.length, "Endpoint address",
    );
    const PayloadEnd = Checkedˉend(AddressEnd, PayloadLength, Bytes.length, "Response payload");
    const DiagnosticEnd = Checkedˉend(PayloadEnd, DiagnosticLength, Bytes.length, "Diagnostic");
    if (DiagnosticEnd !== Bytes.length) Fail("Host-network response has trailing bytes.");
    const Response = {
        status: Bytes.readUInt32LE(12),
        requestId: Bytes.readBigUInt64LE(16),
        providerGeneration: Bytes.readBigUInt64LE(24),
        connectionId: Bytes.readBigUInt64LE(32),
        connectionGeneration: Bytes.readBigUInt64LE(40),
        progress: Bytes.readBigUInt64LE(48),
        endpointPort: Bytes.readUInt32LE(56),
        endpointFamily: Bytes.readUInt32LE(60),
        flags: Bytes.readUInt32LE(64),
        address: Strictˉtext(
            Bytes.subarray(HOST_NETWORK_RESPONSE_HEADER_BYTES, AddressEnd), "Endpoint address", 64,
        ),
        payload: Bytes.subarray(AddressEnd, PayloadEnd),
        diagnostic: Strictˉtext(Bytes.subarray(PayloadEnd, DiagnosticEnd), "Diagnostic", 256),
    };
    return Encodeˉhostˉnetworkˉresponse(Response).equals(Bytes) ?
        Response : Fail("Host-network response is not canonical.");
}

export function Readˉframedˉrecordˉlength(Bytes, Magic, Minimum, Maximum) {
    if (Bytes.length < 12) return null;
    if (Bytes.subarray(0, 4).toString("ascii") !== Magic || Bytes.readUInt32LE(4) !== 1) {
        Fail(`Framed ${Magic} prefix is malformed.`);
    }
    const Total = Bytes.readUInt32LE(8);
    if (Total < Minimum || Total > Maximum) Fail(`Framed ${Magic} length is invalid.`);
    return Total;
}
