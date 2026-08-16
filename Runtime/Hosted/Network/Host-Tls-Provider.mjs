import process from "node:process";
import tls from "node:tls";
import { Hostˉtlsˉprovider } from "./Host-Tls-Provider-Core.mjs";
import { Runˉhostˉnetworkˉprovider } from "./Host-Network-Provider-Process.mjs";

function Usage() {
    return "Usage: node Runtime/Hosted/Network/Host-Tls-Provider.mjs " +
        "--service <canonical-name> --port <1-65535> --generation <nonzero-u64> " +
        "--trust-generation <nonzero-u64> --trust-sha256 <lowercase-hex> " +
        "--alpn <protocol> --trust-mode <node-bundled|pinned> " +
        "[--trusted-certificate-base64 <canonical-DER>] [network limits]";
}

function Parseˉinteger(Value, Maximum) {
    if (!/^[1-9][0-9]*$/.test(Value ?? "")) throw new Error(Usage());
    const NumberValue = Number(Value);
    if (!Number.isSafeInteger(NumberValue) || NumberValue > Maximum) throw new Error(Usage());
    return NumberValue;
}

function Parseˉu64(Value) {
    if (!/^[1-9][0-9]*$/.test(Value ?? "")) throw new Error(Usage());
    const Result = BigInt(Value);
    if (Result > 0xffff_ffff_ffff_ffffn) throw new Error(Usage());
    return Result;
}

function Parseˉcertificate(Value) {
    if (typeof Value !== "string" || Value.length < 1 || Value.length > 16_384 ||
        !/^[A-Za-z0-9+/]+={0,2}$/.test(Value)) throw new Error(Usage());
    const Bytes = Buffer.from(Value, "base64");
    if (Bytes.length < 1 || Bytes.toString("base64") !== Value) throw new Error(Usage());
    return Bytes;
}

function Parseˉarguments(Arguments) {
    const Values = new Map();
    for (let Index = 0; Index < Arguments.length; Index += 2) {
        const Name = Arguments[Index];
        const Value = Arguments[Index + 1];
        if (!Name?.startsWith("--") || Value === undefined || Values.has(Name)) throw new Error(Usage());
        Values.set(Name, Value);
    }
    const Allowed = new Set([
        "--service", "--port", "--generation", "--max-connections", "--max-queued-bytes",
        "--max-transfer-bytes", "--max-operation-ms", "--max-lifetime-ms",
        "--trust-generation", "--trust-sha256", "--alpn", "--trust-mode",
        "--trusted-certificate-base64",
    ]);
    const Required = [
        "--service", "--port", "--generation", "--trust-generation", "--trust-sha256",
        "--alpn", "--trust-mode",
    ];
    if ([...Values.keys()].some(Name => !Allowed.has(Name)) ||
        Required.some(Name => !Values.has(Name))) throw new Error(Usage());
    const Mode = Values.get("--trust-mode");
    const Certificate = Values.get("--trusted-certificate-base64");
    if (!(["node-bundled", "pinned"].includes(Mode)) ||
        (Mode === "node-bundled") === (Certificate !== undefined)) throw new Error(Usage());
    return {
        service: Values.get("--service"),
        port: Parseˉinteger(Values.get("--port"), 65_535),
        generation: Parseˉu64(Values.get("--generation")),
        trustGeneration: Parseˉu64(Values.get("--trust-generation")),
        expectedTrustSha256: Values.get("--trust-sha256"),
        alpn: Values.get("--alpn"),
        trustCertificates: Mode === "node-bundled" ? tls.rootCertificates : [Parseˉcertificate(Certificate)],
        maximumConnections: Parseˉinteger(Values.get("--max-connections") ?? "1", 64),
        maximumQueuedBytes: Parseˉinteger(Values.get("--max-queued-bytes") ?? "65536", 65_536),
        maximumTransferBytes: Parseˉu64(Values.get("--max-transfer-bytes") ?? "16777216"),
        maximumOperationMilliseconds: Parseˉinteger(
            Values.get("--max-operation-ms") ?? "30000", 300_000,
        ),
        maximumLifetimeMilliseconds: Parseˉinteger(
            Values.get("--max-lifetime-ms") ?? "3600000", 86_400_000,
        ),
    };
}

Runˉhostˉnetworkˉprovider(
    new Hostˉtlsˉprovider(Parseˉarguments(process.argv.slice(2))),
);
