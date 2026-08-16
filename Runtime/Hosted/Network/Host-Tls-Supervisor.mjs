import tls from "node:tls";
import { X509Certificate } from "node:crypto";
import { fileURLToPath } from "node:url";
import { Canonicalˉtrustˉsnapshot } from "./Host-Tls-Provider-Core.mjs";
import { Hostˉnetworkˉsupervisor } from "./Host-Network-Supervisor.mjs";

const PROVIDER_PATH = fileURLToPath(new URL("./Host-Tls-Provider.mjs", import.meta.url));

function U64(Value, Description) {
    if (typeof Value !== "bigint" || Value < 1n || Value > 0xffff_ffff_ffff_ffffn) {
        throw new Error(`${Description} is invalid.`);
    }
    return String(Value);
}

export class Hostˉtlsˉsupervisor extends Hostˉnetworkˉsupervisor {
    constructor({
        trustGeneration,
        alpn = "http/1.1",
        trustCertificates,
        ...Network
    }) {
        const Pinned = trustCertificates !== undefined;
        const Values = Pinned ? trustCertificates : tls.rootCertificates;
        const Trust = Canonicalˉtrustˉsnapshot(Values);
        if (Pinned && Values.length !== 1) {
            throw new Error("The first pinned TLS profile requires exactly one certificate.");
        }
        const ProviderArguments = [
            "--trust-generation", U64(trustGeneration, "TLS trust generation"),
            "--trust-sha256", Trust.sha256,
            "--alpn", alpn,
            "--trust-mode", Pinned ? "pinned" : "node-bundled",
        ];
        if (Pinned) {
            const Der = new X509Certificate(Values[0]).raw;
            const Base64 = Buffer.from(Der).toString("base64");
            if (Base64.length > 16_384) {
                throw new Error("The pinned TLS certificate exceeds the launch boundary.");
            }
            ProviderArguments.push("--trusted-certificate-base64", Base64);
        }
        super({ ...Network, providerPath: PROVIDER_PATH, providerArguments: ProviderArguments });
        this.trustGeneration = trustGeneration;
        this.trustSnapshotSha256 = Trust.sha256;
        this.alpn = alpn;
    }
}
