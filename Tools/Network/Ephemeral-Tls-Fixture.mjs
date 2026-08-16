import { generateKeyPairSync, sign } from "node:crypto";

function Length(Value) {
    if (Value < 128) return Buffer.from([Value]);
    const Bytes = [];
    let Remaining = Value;
    while (Remaining > 0) {
        Bytes.unshift(Remaining & 0xff);
        Remaining = Math.floor(Remaining / 256);
    }
    return Buffer.from([0x80 | Bytes.length, ...Bytes]);
}

function Der(Tag, ...Values) {
    const Content = Buffer.concat(Values.map(Value => Buffer.from(Value)));
    return Buffer.concat([Buffer.from([Tag]), Length(Content.length), Content]);
}

function Sequence(...Values) { return Der(0x30, ...Values); }
function Set(...Values) { return Der(0x31, ...Values); }
function Explicit(Number, ...Values) { return Der(0xa0 + Number, ...Values); }
function Boolean(Value) { return Der(0x01, Buffer.from([Value ? 0xff : 0x00])); }
function Octets(Value) { return Der(0x04, Value); }
function Utf8(Value) { return Der(0x0c, Buffer.from(Value, "utf8")); }

function Integer(Value) {
    const Bytes = [];
    let Remaining = Value;
    do {
        Bytes.unshift(Remaining & 0xff);
        Remaining = Math.floor(Remaining / 256);
    } while (Remaining > 0);
    if ((Bytes[0] & 0x80) !== 0) Bytes.unshift(0);
    return Der(0x02, Buffer.from(Bytes));
}

function Base128(Value) {
    const Bytes = [Value & 0x7f];
    let Remaining = Math.floor(Value / 128);
    while (Remaining > 0) {
        Bytes.unshift(0x80 | (Remaining & 0x7f));
        Remaining = Math.floor(Remaining / 128);
    }
    return Bytes;
}

function Oid(Value) {
    const Parts = Value.split(".").map(Number);
    const Bytes = [...Base128(Parts[0] * 40 + Parts[1])];
    for (const Part of Parts.slice(2)) Bytes.push(...Base128(Part));
    return Der(0x06, Buffer.from(Bytes));
}

function BitString(Value, Unused = 0) {
    return Der(0x03, Buffer.from([Unused]), Value);
}

function Time(Value) {
    const Text = [
        String(Value.getUTCFullYear()).slice(2),
        String(Value.getUTCMonth() + 1).padStart(2, "0"),
        String(Value.getUTCDate()).padStart(2, "0"),
        String(Value.getUTCHours()).padStart(2, "0"),
        String(Value.getUTCMinutes()).padStart(2, "0"),
        String(Value.getUTCSeconds()).padStart(2, "0"),
        "Z",
    ].join("");
    return Der(0x17, Buffer.from(Text, "ascii"));
}

function Name(CommonName) {
    return Sequence(Set(Sequence(Oid("2.5.4.3"), Utf8(CommonName))));
}

function Extension(Identity, Critical, Value) {
    return Sequence(Oid(Identity), ...(Critical ? [Boolean(true)] : []), Octets(Value));
}

function Pem(Label, Bytes) {
    const Lines = Buffer.from(Bytes).toString("base64").match(/.{1,64}/g);
    return `-----BEGIN ${Label}-----\n${Lines.join("\n")}\n-----END ${Label}-----\n`;
}

function Certificate({
    serial,
    subject,
    issuer,
    subjectPublicKey,
    issuerPrivateKey,
    extensions,
    notBefore,
    notAfter,
}) {
    const Algorithm = Sequence(Oid("1.2.840.10045.4.3.2"));
    const Tbs = Sequence(
        Explicit(0, Integer(2)),
        Integer(serial),
        Algorithm,
        issuer,
        Sequence(Time(notBefore), Time(notAfter)),
        subject,
        subjectPublicKey,
        Explicit(3, Sequence(...extensions)),
    );
    const Signature = sign("sha256", Tbs, issuerPrivateKey);
    return Sequence(Tbs, Algorithm, BitString(Signature));
}

export function Createˉephemeralˉtlsˉfixture(Service = "localhost") {
    if (typeof Service !== "string" || !/^[a-z0-9.-]{1,253}$/.test(Service)) {
        throw new Error("Ephemeral TLS service is invalid.");
    }
    const CaKeys = generateKeyPairSync("ec", { namedCurve: "prime256v1" });
    const ServerKeys = generateKeyPairSync("ec", { namedCurve: "prime256v1" });
    const CaName = Name("Windvale ephemeral TLS test CA");
    const ServerName = Name(Service);
    const Now = Date.now();
    const NotBefore = new Date(Now - 3_600_000);
    const NotAfter = new Date(Now + 86_400_000);
    const CaCertificate = Certificate({
        serial: 1,
        subject: CaName,
        issuer: CaName,
        subjectPublicKey: CaKeys.publicKey.export({ type: "spki", format: "der" }),
        issuerPrivateKey: CaKeys.privateKey,
        notBefore: NotBefore,
        notAfter: NotAfter,
        extensions: [
            Extension("2.5.29.19", true, Sequence(Boolean(true))),
            Extension("2.5.29.15", true, BitString(Buffer.from([0x06]), 1)),
        ],
    });
    const ServerCertificate = Certificate({
        serial: 2,
        subject: ServerName,
        issuer: CaName,
        subjectPublicKey: ServerKeys.publicKey.export({ type: "spki", format: "der" }),
        issuerPrivateKey: CaKeys.privateKey,
        notBefore: NotBefore,
        notAfter: NotAfter,
        extensions: [
            Extension("2.5.29.19", true, Sequence()),
            Extension("2.5.29.15", true, BitString(Buffer.from([0x80]), 7)),
            Extension("2.5.29.37", false, Sequence(Oid("1.3.6.1.5.5.7.3.1"))),
            Extension("2.5.29.17", false, Sequence(
                Der(0x82, Buffer.from(Service, "ascii")),
                Der(0x87, Buffer.from([127, 0, 0, 1])),
            )),
        ],
    });
    return Object.freeze({
        ca: Pem("CERTIFICATE", CaCertificate),
        cert: Pem("CERTIFICATE", ServerCertificate),
        key: ServerKeys.privateKey.export({ type: "pkcs8", format: "pem" }),
    });
}
