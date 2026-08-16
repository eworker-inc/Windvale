import {
    createCipheriv,
    createDecipheriv,
    randomBytes as DefaultRandomBytes,
    scrypt,
} from "node:crypto";
import { TextDecoder } from "node:util";
import { Boundedˉhttpsˉclient } from "../Http/Bounded-Https-Client.mjs";

const HEADER_BYTES = 160;
const SALT_BYTES = 32;
const NONCE_BYTES = 12;
const TAG_BYTES = 16;
const IDENTITY_BYTES = 16;
const KDF_N = 131_072;
const KDF_R = 8;
const KDF_P = 1;
const KDF_MAX_MEMORY = 268_435_456;
const UTF8 = new TextDecoder("utf-8", { fatal: true });

const PROVIDERS = Object.freeze({
    openai: Object.freeze({
        code: 1, service: "api.openai.com",
        header: "authorization", prefix: Buffer.from("Bearer ", "ascii"),
    }),
    anthropic: Object.freeze({
        code: 2, service: "api.anthropic.com",
        header: "x-api-key", prefix: Buffer.alloc(0),
    }),
    google: Object.freeze({
        code: 3, service: "generativelanguage.googleapis.com",
        header: "x-goog-api-key", prefix: Buffer.alloc(0),
    }),
});

export class Protectedˉcredentialˉfailure extends Error {
    constructor(Kind, Message) {
        super(Message);
        this.kind = Kind;
    }
}

function Fail(Kind, Message) {
    throw new Protectedˉcredentialˉfailure(Kind, Message);
}

function Providerˉprofile(Name) {
    const Profile = PROVIDERS[Name];
    if (!Profile) Fail("invalid_binding", "Credential provider binding is invalid.");
    return Profile;
}

function Canonicalˉservice(Value) {
    if (typeof Value !== "string" || Value.length < 1 || Value.length > 253 ||
        Buffer.byteLength(Value, "ascii") !== Value.length ||
        /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$/.test(Value)) return false;
    return Value.split(".").every(Label => Label.length >= 1 && Label.length <= 63 &&
        /^[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$/.test(Label));
}

function U64(Value, Description) {
    if (typeof Value !== "bigint" || Value < 1n || Value > 0xffff_ffff_ffff_ffffn) {
        Fail("invalid_binding", `${Description} is invalid.`);
    }
    return Value;
}

function Passphraseˉbytes(Value) {
    const Bytes = Buffer.from(Value ?? []);
    if (Bytes.length < 16 || Bytes.length > 1_024) {
        Bytes.fill(0);
        Fail("invalid_passphrase", "Credential passphrase length is invalid.");
    }
    let Text;
    try { Text = UTF8.decode(Bytes); } catch {
        Bytes.fill(0);
        Fail("invalid_passphrase", "Credential passphrase is not strict UTF-8.");
    }
    if (Text.includes("\0")) {
        Bytes.fill(0);
        Fail("invalid_passphrase", "Credential passphrase contains NUL.");
    }
    return Bytes;
}

function Secretˉbytes(Value) {
    const Bytes = Buffer.from(Value ?? []);
    if (Bytes.length < 16 || Bytes.length > 1_024) {
        Bytes.fill(0);
        Fail("invalid_credential", "Credential byte length is invalid.");
    }
    for (const Byte of Bytes) {
        if (Byte < 0x21 || Byte > 0x7e) {
            Bytes.fill(0);
            Fail("invalid_credential", "Credential contains unsupported bytes.");
        }
    }
    return Bytes;
}

function Nonzero(Bytes) {
    return Bytes.some(Byte => Byte !== 0);
}

function Random(Default, Length, Description) {
    let Bytes;
    try { Bytes = Buffer.from(Default(Length)); } catch {
        Fail("entropy_unavailable", "Credential entropy source failed.");
    }
    if (Bytes.length !== Length || !Nonzero(Bytes)) {
        Fail("entropy_unavailable", `${Description} entropy is invalid.`);
    }
    return Bytes;
}

function Deriveˉkey(Passphrase, Salt) {
    return new Promise((Resolve, Reject) => {
        scrypt(Passphrase, Salt, 32, {
            N: KDF_N,
            r: KDF_R,
            p: KDF_P,
            maxmem: KDF_MAX_MEMORY,
        }, (Error, Key) => Error ? Reject(Error) : Resolve(Buffer.from(Key)));
    });
}

function Header({
    total,
    providerCode,
    generation,
    serviceLength,
    credentialLength,
    identity,
    salt,
    nonce,
    tag = Buffer.alloc(TAG_BYTES),
}) {
    const Bytes = Buffer.alloc(HEADER_BYTES);
    Bytes.write("WVSC", 0, 4, "ascii");
    Bytes.writeUInt32LE(1, 4);
    Bytes.writeUInt32LE(total, 8);
    Bytes.writeUInt32LE(providerCode, 12);
    Bytes.writeBigUInt64LE(generation, 16);
    Bytes.writeUInt32LE(serviceLength, 24);
    Bytes.writeUInt32LE(credentialLength, 28);
    Bytes.writeUInt32LE(credentialLength, 32);
    Bytes.writeUInt32LE(HEADER_BYTES, 36);
    Bytes.writeUInt32LE(KDF_N, 40);
    Bytes.writeUInt32LE(KDF_R, 44);
    Bytes.writeUInt32LE(KDF_P, 48);
    Bytes.writeUInt32LE(KDF_MAX_MEMORY, 52);
    Bytes.writeUInt32LE(SALT_BYTES, 56);
    Bytes.writeUInt32LE(NONCE_BYTES, 60);
    Bytes.writeUInt32LE(TAG_BYTES, 64);
    Bytes.writeUInt32LE(IDENTITY_BYTES, 68);
    Bytes.writeUInt32LE(1, 72);
    identity.copy(Bytes, 80);
    salt.copy(Bytes, 96);
    nonce.copy(Bytes, 128);
    tag.copy(Bytes, 140);
    return Bytes;
}

function Parse(Value) {
    const Invalid = () => Fail("invalid_wrapper", "Protected credential wrapper is invalid.");
    let Bytes;
    try { Bytes = Buffer.from(Value); } catch { Invalid(); }
    if (Bytes.length < HEADER_BYTES + 17 || Bytes.length > HEADER_BYTES + 253 + 1_024 ||
        Bytes.subarray(0, 4).toString("ascii") !== "WVSC" ||
        Bytes.readUInt32LE(4) !== 1 || Bytes.readUInt32LE(8) !== Bytes.length ||
        Bytes.readUInt32LE(36) !== HEADER_BYTES || Bytes.readUInt32LE(40) !== KDF_N ||
        Bytes.readUInt32LE(44) !== KDF_R || Bytes.readUInt32LE(48) !== KDF_P ||
        Bytes.readUInt32LE(52) !== KDF_MAX_MEMORY || Bytes.readUInt32LE(56) !== SALT_BYTES ||
        Bytes.readUInt32LE(60) !== NONCE_BYTES || Bytes.readUInt32LE(64) !== TAG_BYTES ||
        Bytes.readUInt32LE(68) !== IDENTITY_BYTES || Bytes.readUInt32LE(72) !== 1 ||
        Bytes.readUInt32LE(76) !== 0 || Bytes.readUInt32LE(156) !== 0) Invalid();
    const ProviderCode = Bytes.readUInt32LE(12);
    const Provider = Object.keys(PROVIDERS).find(Name => PROVIDERS[Name].code === ProviderCode);
    const Generation = Bytes.readBigUInt64LE(16);
    const ServiceLength = Bytes.readUInt32LE(24);
    const CredentialLength = Bytes.readUInt32LE(28);
    const CiphertextLength = Bytes.readUInt32LE(32);
    if (!Provider || Generation === 0n || ServiceLength < 1 || ServiceLength > 253 ||
        CredentialLength < 16 || CredentialLength > 1_024 ||
        CiphertextLength !== CredentialLength ||
        HEADER_BYTES + ServiceLength + CiphertextLength !== Bytes.length) Invalid();
    const Service = Bytes.subarray(HEADER_BYTES, HEADER_BYTES + ServiceLength).toString("ascii");
    if (!Canonicalˉservice(Service) || Buffer.byteLength(Service, "ascii") !== ServiceLength ||
        Service !== PROVIDERS[Provider].service) Invalid();
    const Identity = Bytes.subarray(80, 96);
    const Salt = Bytes.subarray(96, 128);
    const Nonce = Bytes.subarray(128, 140);
    if (!Nonzero(Identity) || !Nonzero(Salt) || !Nonzero(Nonce)) Invalid();
    return Object.freeze({
        bytes: Bytes,
        provider: Provider,
        profile: PROVIDERS[Provider],
        generation: Generation,
        service: Service,
        serviceLength: ServiceLength,
        credentialLength: CredentialLength,
        identity: Buffer.from(Identity),
        salt: Buffer.from(Salt),
        nonce: Buffer.from(Nonce),
        tag: Buffer.from(Bytes.subarray(140, 156)),
        ciphertext: Buffer.from(Bytes.subarray(HEADER_BYTES + ServiceLength)),
    });
}

export function Inspectˉprotectedˉcredential(Value) {
    const Parsed = Parse(Value);
    return Object.freeze({
        provider: Parsed.provider,
        service: Parsed.service,
        port: 443,
        generation: Parsed.generation,
        identity: Parsed.identity.toString("hex"),
        credentialBytes: Parsed.credentialLength,
    });
}

export async function Createˉprotectedˉcredential({
    provider,
    service,
    generation,
    credential,
    passphrase,
    randomBytes = DefaultRandomBytes,
}) {
    const Profile = Providerˉprofile(provider);
    if (!Canonicalˉservice(service) || service !== Profile.service ||
        typeof randomBytes !== "function") {
        Fail("invalid_binding", "Credential service binding is invalid.");
    }
    const Generation = U64(generation, "Credential generation");
    let Secret;
    let Password;
    let Key;
    try {
        Secret = Secretˉbytes(credential);
        Password = Passphraseˉbytes(passphrase);
        const Service = Buffer.from(service, "ascii");
        const Identity = Random(randomBytes, IDENTITY_BYTES, "Credential identity");
        const Salt = Random(randomBytes, SALT_BYTES, "Credential salt");
        const Nonce = Random(randomBytes, NONCE_BYTES, "Credential nonce");
        const Total = HEADER_BYTES + Service.length + Secret.length;
        Key = await Deriveˉkey(Password, Salt);
        const AadHeader = Header({
            total: Total,
            providerCode: Profile.code,
            generation: Generation,
            serviceLength: Service.length,
            credentialLength: Secret.length,
            identity: Identity,
            salt: Salt,
            nonce: Nonce,
        });
        const Aad = Buffer.concat([AadHeader, Service]);
        const Cipher = createCipheriv("aes-256-gcm", Key, Nonce, { authTagLength: TAG_BYTES });
        Cipher.setAAD(Aad, { plaintextLength: Secret.length });
        const Ciphertext = Buffer.concat([Cipher.update(Secret), Cipher.final()]);
        const FinalHeader = Header({
            total: Total,
            providerCode: Profile.code,
            generation: Generation,
            serviceLength: Service.length,
            credentialLength: Secret.length,
            identity: Identity,
            salt: Salt,
            nonce: Nonce,
            tag: Cipher.getAuthTag(),
        });
        return Buffer.concat([FinalHeader, Service, Ciphertext]);
    } catch (Error) {
        if (Error instanceof Protectedˉcredentialˉfailure) throw Error;
        Fail("crypto_unavailable", "Protected credential creation failed.");
    } finally {
        Secret?.fill(0);
        Password?.fill(0);
        Key?.fill(0);
    }
}

class Credentialˉlease {
    #bytes;
    #destroyed;
    #identity;

    constructor(Parsed, Bytes) {
        this.provider = Parsed.provider;
        this.service = Parsed.service;
        this.port = 443;
        this.generation = Parsed.generation;
        this.#identity = Parsed.identity.toString("hex");
        this.#bytes = Bytes;
        this.#destroyed = false;
    }

    inspect() {
        return Object.freeze({
            provider: this.provider,
            service: this.service,
            port: this.port,
            generation: this.generation,
            identity: this.#identity,
            state: this.#destroyed ? "destroyed" : "available",
        });
    }

    bindHttps(Values) {
        if (this.#destroyed) Fail("revoked", "Credential lease is revoked.");
        const Profile = PROVIDERS[this.provider];
        if (Values === null || typeof Values !== "object") {
            Fail("invalid_binding", "Credential HTTPS binding is invalid.");
        }
        if (Values.port !== undefined && Values.port !== this.port) {
            Fail("invalid_binding", "Credential HTTPS port binding is invalid.");
        }
        const PublicHeaders = Values.allowedHeaders ?? ["accept", "content-type"];
        if (!Array.isArray(PublicHeaders) || PublicHeaders.some(Name =>
            typeof Name === "string" && Name.toLowerCase() === Profile.header)) {
            Fail("invalid_binding", "Credential authorization header cannot be delegated.");
        }
        const Client = new Boundedˉhttpsˉclient({
            ...Values,
            service: this.service,
            port: this.port,
            allowedHeaders: [...PublicHeaders, Profile.header],
        });
        return Object.freeze({
            request: async ({ expectedCredentialGeneration, headers = [], ...Request }) => {
                if (this.#destroyed) Fail("revoked", "Credential lease is revoked.");
                if (expectedCredentialGeneration !== this.generation) {
                    Fail("stale", "Credential generation is stale.");
                }
                if (!Array.isArray(headers) || headers.some(Header =>
                    Header?.name?.toLowerCase() === Profile.header)) {
                    Fail("denied", "Credential authorization header is provider-owned.");
                }
                const Authorization = Buffer.concat([Profile.prefix, this.#bytes]);
                try {
                    return await Client.request({
                        ...Request,
                        headers: [...headers, { name: Profile.header, value: Authorization }],
                    });
                } finally {
                    Authorization.fill(0);
                }
            },
        });
    }

    destroy() {
        if (this.#destroyed) return;
        this.#bytes.fill(0);
        this.#destroyed = true;
    }
}

export async function Unlockˉprotectedˉcredential(Value, Passphrase) {
    const Parsed = Parse(Value);
    const Password = Passphraseˉbytes(Passphrase);
    let Key;
    let Plaintext;
    try {
        Key = await Deriveˉkey(Password, Parsed.salt);
        const AadHeader = Buffer.from(Parsed.bytes.subarray(0, HEADER_BYTES));
        AadHeader.fill(0, 140, 156);
        const Aad = Buffer.concat([
            AadHeader,
            Parsed.bytes.subarray(HEADER_BYTES, HEADER_BYTES + Parsed.serviceLength),
        ]);
        const Decipher = createDecipheriv(
            "aes-256-gcm", Key, Parsed.nonce, { authTagLength: TAG_BYTES },
        );
        Decipher.setAAD(Aad, { plaintextLength: Parsed.credentialLength });
        Decipher.setAuthTag(Parsed.tag);
        Plaintext = Buffer.concat([Decipher.update(Parsed.ciphertext), Decipher.final()]);
        const Admitted = Secretˉbytes(Plaintext);
        Plaintext.fill(0);
        Plaintext = null;
        return new Credentialˉlease(Parsed, Admitted);
    } catch (Error) {
        Plaintext?.fill(0);
        if (Error instanceof Protectedˉcredentialˉfailure && Error.kind !== "invalid_credential") {
            throw Error;
        }
        Fail("unlock_failed", "Protected credential unlock failed.");
    } finally {
        Password.fill(0);
        Key?.fill(0);
    }
}
