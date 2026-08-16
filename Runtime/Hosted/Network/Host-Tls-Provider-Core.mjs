import { createHash, X509Certificate } from "node:crypto";
import tls from "node:tls";
import { Hostˉnetworkˉprovider } from "./Host-Network-Provider-Core.mjs";

const MAX_TRUST_CERTIFICATES = 256;
const MAX_TRUST_BYTES = 1_048_576;

function U64(Value, Description) {
    if (typeof Value !== "bigint" || Value < 1n || Value > 0xffff_ffff_ffff_ffffn) {
        throw new Error(`${Description} is invalid.`);
    }
    return Value;
}

function Alpn(Value) {
    if (typeof Value !== "string" || Buffer.byteLength(Value, "ascii") !== Value.length ||
        Value.length < 1 || Value.length > 32 || !/^[a-z0-9][a-z0-9./-]*$/.test(Value)) {
        throw new Error("TLS ALPN authority is invalid.");
    }
    return Value;
}

export function Canonicalˉtrustˉsnapshot(Values = tls.rootCertificates) {
    if (!Array.isArray(Values) || Values.length < 1 || Values.length > MAX_TRUST_CERTIFICATES) {
        throw new Error("TLS trust snapshot certificate count is invalid.");
    }
    const Certificates = [];
    let Total = 0;
    for (const Value of Values) {
        let Certificate;
        try { Certificate = new X509Certificate(Value); } catch {
            throw new Error("TLS trust snapshot certificate is invalid.");
        }
        const Der = Buffer.from(Certificate.raw);
        Total += Der.length;
        if (Der.length < 1 || Der.length > 65_536 || Total > MAX_TRUST_BYTES) {
            throw new Error("TLS trust snapshot byte limit is invalid.");
        }
        Certificates.push({ der: Der, pem: Certificate.toString() });
    }
    const Hash = createHash("sha256");
    const Header = Buffer.alloc(4);
    Header.writeUInt32LE(Certificates.length);
    Hash.update(Header);
    for (const Certificate of Certificates) {
        const Length = Buffer.alloc(4);
        Length.writeUInt32LE(Certificate.der.length);
        Hash.update(Length);
        Hash.update(Certificate.der);
    }
    return Object.freeze({
        certificates: Object.freeze(Certificates.map(Value => Value.pem)),
        sha256: Hash.digest("hex"),
    });
}

export class Hostˉtlsˉprovider extends Hostˉnetworkˉprovider {
    constructor({
        service,
        port,
        generation,
        trustGeneration,
        alpn = "http/1.1",
        trustCertificates = tls.rootCertificates,
        expectedTrustSha256,
        resolver,
        tlsConnect = tls.connect,
        ...Limits
    }) {
        const Trust = Canonicalˉtrustˉsnapshot(trustCertificates);
        if (typeof expectedTrustSha256 !== "string" ||
            !/^[0-9a-f]{64}$/.test(expectedTrustSha256) ||
            expectedTrustSha256 !== Trust.sha256) {
            throw new Error("TLS trust snapshot digest is invalid.");
        }
        if (typeof tlsConnect !== "function") throw new Error("TLS mechanism is invalid.");
        const Protocol = Alpn(alpn);
        super({
            service,
            port,
            generation,
            ...Limits,
            ...(resolver === undefined ? {} : { resolver }),
            socketReadyEvent: "secureConnect",
            socketFactory: Options => tlsConnect({
                ...Options,
                servername: service,
                minVersion: "TLSv1.3",
                maxVersion: "TLSv1.3",
                ALPNProtocols: [Protocol],
                rejectUnauthorized: true,
                ca: Trust.certificates,
            }),
            validateConnectedSocket: Socket => {
                const Peer = Socket.getPeerCertificate(true);
                if (!Socket.encrypted || !Socket.authorized || Socket.authorizationError !== null ||
                    Socket.getProtocol() !== "TLSv1.3" || Socket.alpnProtocol !== Protocol ||
                    Peer === null || typeof Peer !== "object" || !Buffer.isBuffer(Peer.raw) ||
                    Peer.raw.length < 1) {
                    throw new Error("TLS peer evidence is invalid.");
                }
            },
        });
        this.trustGeneration = U64(trustGeneration, "TLS trust generation");
        this.alpn = Protocol;
        this.trustSnapshotSha256 = Trust.sha256;
    }
}
