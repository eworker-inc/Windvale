export const MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES = 64;
export const MODEL_GATEWAY_MAX_INITIALIZATION_BYTES = 4_096;
export const MODEL_GATEWAY_READY_BYTES = 56;
export const MODEL_GATEWAY_MAX_REQUEST_BYTES = 65_536;
export const MODEL_GATEWAY_MAX_RESPONSE_BYTES = 1_048_576;

const PROVIDER_CODES = Object.freeze({ openai: 1, anthropic: 2, google: 3 });

function U32(Value, Minimum, Maximum, Description) {
    if (!Number.isSafeInteger(Value) || Value < Minimum || Value > Maximum) {
        throw new Error(`${Description} is invalid.`);
    }
    return Value;
}

function U64(Value, Description) {
    if (typeof Value !== "bigint" || Value < 1n || Value > 0xffff_ffff_ffff_ffffn) {
        throw new Error(`${Description} is invalid.`);
    }
    return Value;
}

export function Encodeˉmodelˉgatewayˉinitialization({
    wrapper,
    passphrase,
    providerGeneration,
    trustGeneration,
    maximumRequestBytes = 65_536,
    maximumHeaderBytes = 16_384,
    maximumBodyBytes = 1_048_576,
    maximumWireBytes = 1_081_344,
    maximumOperationMilliseconds = 30_000,
    maximumLifetimeMilliseconds = 60_000,
}) {
    const Values = {
        providerGeneration: U64(providerGeneration, "Provider generation"),
        trustGeneration: U64(trustGeneration, "Trust generation"),
        maximumRequestBytes: U32(maximumRequestBytes, 1_024, 65_536, "Request limit"),
        maximumHeaderBytes: U32(maximumHeaderBytes, 256, 16_384, "Header limit"),
        maximumBodyBytes: U32(maximumBodyBytes, 1_024, 1_048_576, "Body limit"),
        maximumWireBytes: U32(maximumWireBytes, 1_280, 1_114_112, "Wire limit"),
        maximumOperationMilliseconds: U32(
            maximumOperationMilliseconds, 1, 300_000, "Operation span",
        ),
        maximumLifetimeMilliseconds: U32(
            maximumLifetimeMilliseconds, 1, 300_000, "Provider lifetime",
        ),
    };
    let Wrapper;
    let Passphrase;
    try {
        Wrapper = Buffer.from(wrapper ?? []);
        Passphrase = Buffer.from(passphrase ?? []);
        if (Wrapper.length < 177 || Wrapper.length > 1_437 ||
            Passphrase.length < 16 || Passphrase.length > 1_024) {
            throw new Error("Model-gateway protected startup material is invalid.");
        }
        if (Values.maximumWireBytes < Values.maximumHeaderBytes + Values.maximumBodyBytes ||
            Values.maximumLifetimeMilliseconds < Values.maximumOperationMilliseconds) {
            throw new Error("Model-gateway limits are inconsistent.");
        }
        const Total = MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES + Wrapper.length + Passphrase.length;
        const Bytes = Buffer.alloc(Total);
        Bytes.write("WVGI", 0, 4, "ascii");
        Bytes.writeUInt32LE(1, 4);
        Bytes.writeUInt32LE(Total, 8);
        Bytes.writeUInt32LE(Wrapper.length, 12);
        Bytes.writeUInt32LE(Passphrase.length, 16);
        Bytes.writeBigUInt64LE(Values.providerGeneration, 24);
        Bytes.writeBigUInt64LE(Values.trustGeneration, 32);
        Bytes.writeUInt32LE(Values.maximumRequestBytes, 40);
        Bytes.writeUInt32LE(Values.maximumHeaderBytes, 44);
        Bytes.writeUInt32LE(Values.maximumBodyBytes, 48);
        Bytes.writeUInt32LE(Values.maximumWireBytes, 52);
        Bytes.writeUInt32LE(Values.maximumOperationMilliseconds, 56);
        Bytes.writeUInt32LE(Values.maximumLifetimeMilliseconds, 60);
        Wrapper.copy(Bytes, MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES);
        Passphrase.copy(Bytes, MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES + Wrapper.length);
        return Bytes;
    } finally {
        Wrapper?.fill(0);
        Passphrase?.fill(0);
    }
}

export function Decodeˉmodelˉgatewayˉinitialization(Value) {
    let Bytes;
    try { Bytes = Buffer.from(Value); } catch {
        throw new Error("Model-gateway initialization is malformed.");
    }
    try {
        if (Bytes.length < MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES + 193 ||
            Bytes.length > MODEL_GATEWAY_MAX_INITIALIZATION_BYTES ||
            Bytes.subarray(0, 4).toString("ascii") !== "WVGI" ||
            Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length ||
            Bytes.readUInt32LE(20) !== 0) {
            throw new Error("Model-gateway initialization is malformed.");
        }
        const WrapperLength = Bytes.readUInt32LE(12);
        const PassphraseLength = Bytes.readUInt32LE(16);
        if (WrapperLength < 177 || WrapperLength > 1_437 || PassphraseLength < 16 ||
            PassphraseLength > 1_024 ||
            MODEL_GATEWAY_INITIALIZATION_HEADER_BYTES + WrapperLength + PassphraseLength !== Bytes.length) {
            throw new Error("Model-gateway initialization geometry is invalid.");
        }
        const Values = {
            providerGeneration: Bytes.readBigUInt64LE(24),
            trustGeneration: Bytes.readBigUInt64LE(32),
            maximumRequestBytes: Bytes.readUInt32LE(40),
            maximumHeaderBytes: Bytes.readUInt32LE(44),
            maximumBodyBytes: Bytes.readUInt32LE(48),
            maximumWireBytes: Bytes.readUInt32LE(52),
            maximumOperationMilliseconds: Bytes.readUInt32LE(56),
            maximumLifetimeMilliseconds: Bytes.readUInt32LE(60),
        };
        U64(Values.providerGeneration, "Provider generation");
        U64(Values.trustGeneration, "Trust generation");
        U32(Values.maximumRequestBytes, 1_024, 65_536, "Request limit");
        U32(Values.maximumHeaderBytes, 256, 16_384, "Header limit");
        U32(Values.maximumBodyBytes, 1_024, 1_048_576, "Body limit");
        U32(Values.maximumWireBytes, 1_280, 1_114_112, "Wire limit");
        U32(Values.maximumOperationMilliseconds, 1, 300_000, "Operation span");
        U32(Values.maximumLifetimeMilliseconds, 1, 300_000, "Provider lifetime");
        if (Values.maximumWireBytes < Values.maximumHeaderBytes + Values.maximumBodyBytes ||
            Values.maximumLifetimeMilliseconds < Values.maximumOperationMilliseconds) {
            throw new Error("Model-gateway initialization limits are inconsistent.");
        }
        return {
            ...Values,
            wrapper: Buffer.from(Bytes.subarray(64, 64 + WrapperLength)),
            passphrase: Buffer.from(Bytes.subarray(64 + WrapperLength)),
        };
    } finally {
        Bytes.fill(0);
    }
}

export function Encodeˉmodelˉgatewayˉready({
    status = 0,
    provider = "",
    providerGeneration = 0n,
    credentialGeneration = 0n,
    identity = "",
}) {
    if (status !== 0 && status !== 1) throw new Error("Gateway ready status is invalid.");
    const Bytes = Buffer.alloc(MODEL_GATEWAY_READY_BYTES);
    Bytes.write("WVGR", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(Bytes.length, 8);
    Bytes.writeUInt32LE(status, 12);
    if (status === 0) {
        const ProviderCode = PROVIDER_CODES[provider];
        const Identity = Buffer.from(identity, "hex");
        if (!ProviderCode || Identity.length !== 16) throw new Error("Gateway identity is invalid.");
        Bytes.writeUInt32LE(ProviderCode, 16);
        Bytes.writeBigUInt64LE(U64(providerGeneration, "Provider generation"), 24);
        Bytes.writeBigUInt64LE(U64(credentialGeneration, "Credential generation"), 32);
        Identity.copy(Bytes, 40);
    }
    return Bytes;
}

export function Decodeˉmodelˉgatewayˉready(Value) {
    const Bytes = Buffer.from(Value);
    if (Bytes.length !== MODEL_GATEWAY_READY_BYTES ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVGR" ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length ||
        Bytes.readUInt32LE(20) !== 0) {
        throw new Error("Model-gateway ready record is malformed.");
    }
    const Status = Bytes.readUInt32LE(12);
    if (Status === 1) {
        if (Bytes.subarray(16).some(Byte => Byte !== 0)) {
            throw new Error("Failed model-gateway ready record contains metadata.");
        }
        return Object.freeze({ status: "failed" });
    }
    const Provider = Object.keys(PROVIDER_CODES).find(
        Name => PROVIDER_CODES[Name] === Bytes.readUInt32LE(16),
    );
    const ProviderGeneration = Bytes.readBigUInt64LE(24);
    const CredentialGeneration = Bytes.readBigUInt64LE(32);
    const Identity = Bytes.subarray(40);
    if (Status !== 0 || !Provider || ProviderGeneration === 0n ||
        CredentialGeneration === 0n || !Identity.some(Byte => Byte !== 0)) {
        throw new Error("Model-gateway ready metadata is invalid.");
    }
    return Object.freeze({
        status: "ready", provider: Provider, providerGeneration: ProviderGeneration,
        credentialGeneration: CredentialGeneration, identity: Identity.toString("hex"),
    });
}

export function Readˉmodelˉgatewayˉframeˉlength(Value, Magic, Minimum, Maximum) {
    if (Value.length < 12) return null;
    if (Value.subarray(0, 4).toString("ascii") !== Magic || Value.readUInt32LE(4) !== 1) {
        throw new Error("Model-gateway frame prefix is invalid.");
    }
    const Total = Value.readUInt32LE(8);
    if (Total < Minimum || Total > Maximum) throw new Error("Model-gateway frame size is invalid.");
    return Total;
}
