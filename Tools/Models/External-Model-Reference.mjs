import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import process from "node:process";
import {
    Decodeˉmodelˉrequest,
    Executeˉexternalˉmodelˉrequest,
    Providerˉkeyˉenvironment,
} from "./External-Model-Reference-Core.mjs";

function Usage() {
    return "Usage: node Tools/Models/External-Model-Reference.mjs " +
        "--provider <openai|anthropic|google> --request <WVMQ-file> --output <response-file> " +
        "--generation <nonzero-u64> [--timeout-ms <1-300000>]";
}

function Parseˉarguments(Arguments) {
    const Values = new Map();
    for (let Index = 0; Index < Arguments.length; Index += 2) {
        const Name = Arguments[Index];
        const Value = Arguments[Index + 1];
        if (!Name?.startsWith("--") || Value === undefined || Values.has(Name)) throw new Error(Usage());
        Values.set(Name, Value);
    }
    const Allowed = new Set(["--provider", "--request", "--output", "--generation", "--timeout-ms"]);
    if ([...Values.keys()].some(Name => !Allowed.has(Name)) ||
        !Values.has("--provider") || !Values.has("--request") || !Values.has("--output") ||
        !Values.has("--generation")) throw new Error(Usage());
    const Provider = Values.get("--provider");
    if (!["openai", "anthropic", "google"].includes(Provider)) throw new Error(Usage());
    const GenerationText = Values.get("--generation");
    if (!/^[1-9][0-9]*$/.test(GenerationText)) throw new Error(Usage());
    const Generation = BigInt(GenerationText);
    if (Generation > 18_446_744_073_709_551_615n) throw new Error(Usage());
    const TimeoutText = Values.get("--timeout-ms") ?? "30000";
    if (!/^[1-9][0-9]*$/.test(TimeoutText)) throw new Error(Usage());
    const Timeout = Number(TimeoutText);
    if (!Number.isSafeInteger(Timeout) || Timeout > 300_000) throw new Error(Usage());
    return {
        provider: Provider,
        requestPath: path.resolve(Values.get("--request")),
        outputPath: path.resolve(Values.get("--output")),
        generation: Generation,
        timeoutMilliseconds: Timeout,
    };
}

function Readˉrequest(Path) {
    const Stat = fs.lstatSync(Path);
    if (!Stat.isFile() || Stat.isSymbolicLink() || Stat.size < 48 || Stat.size > 65_536) {
        throw new Error("Request input must be an ordinary WVMQ file of at most 65536 bytes.");
    }
    const Bytes = fs.readFileSync(Path);
    if (Bytes.length !== Stat.size) throw new Error("Request input changed while it was read.");
    return Bytes;
}

function Writeˉresponse(Path, Bytes) {
    const Directory = path.dirname(Path);
    fs.mkdirSync(Directory, { recursive: true });
    const Temporary = path.join(
        Directory,
        `.${path.basename(Path)}.${process.pid}.${crypto.randomUUID()}.tmp`,
    );
    try {
        fs.writeFileSync(Temporary, Bytes, { flag: "wx", mode: 0o600 });
        fs.renameSync(Temporary, Path);
    } finally {
        try { fs.unlinkSync(Temporary); } catch (Error) {
            if (Error?.code !== "ENOENT") throw Error;
        }
    }
}

async function Main() {
    const Arguments = Parseˉarguments(process.argv.slice(2));
    const RequestBytes = Readˉrequest(Arguments.requestPath);
    const Request = Decodeˉmodelˉrequest(RequestBytes);
    const KeyName = Providerˉkeyˉenvironment(Arguments.provider);
    const Response = await Executeˉexternalˉmodelˉrequest({
        provider: Arguments.provider,
        requestBytes: RequestBytes,
        generation: Arguments.generation,
        apiKey: process.env[KeyName],
        timeoutMilliseconds: Arguments.timeoutMilliseconds,
    });
    Writeˉresponse(Arguments.outputPath, Response);
    const Status = Response.readUInt32LE(12);
    process.stdout.write(
        `external model reference provider=${Arguments.provider} operation=${Request.operation} ` +
        `status=${Status} request=${Request.requestId} bytes=${Response.length}\n`,
    );
}

Main().catch(Error => {
    process.stderr.write(`External model reference failed: ${Error.message}\n`);
    process.exitCode = 1;
});
