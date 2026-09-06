#!/usr/bin/env node

import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import process from "node:process";
import { pathToFileURL } from "node:url";
import {
    Createˉprotectedˉcredential,
    Inspectˉprotectedˉcredential,
} from "../../Runtime/Hosted/Credentials/Protected-Credential.mjs";
import {
    Decodeˉgatewayˉcatalogˉresponse,
    Encodeˉgatewayˉcatalogˉrequest,
    Modelˉclientˉstatus,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Client.mjs";
import {
    Externalˉmodelˉgatewayˉsupervisor,
} from "../../Runtime/Hosted/Models/External-Model-Gateway-Supervisor.mjs";
import {
    Modelˉchatˉfailure,
    Modelˉchatˉusage,
    Parseˉmodelˉchatˉarguments,
} from "./Model-Chat-Core.mjs";
import {
    Nativeˉexternalˉmodelˉgatewayˉsupervisor,
} from "../../Runtime/Hosted/Models/Native-External-Model-Gateway-Supervisor.mjs";

const MAX_CREDENTIAL_FILE_BYTES = 1_437;
const MAX_SECRET_BYTES = 1_024;
const PROVIDER_GENERATION = 1n;
const TRUST_GENERATION = 1n;

function Fail(Kind, Message) {
    throw new Modelˉchatˉfailure(Kind, Message);
}

function Readˉterminalˉbytes(Prompt, MaximumBytes, Masked, AllowEnd = false) {
    if (!process.stdin.isTTY || !process.stdout.isTTY ||
        typeof process.stdin.setRawMode !== "function") {
        Fail("terminal", "Protected interactive input requires a terminal.");
    }
    const Storage = Buffer.alloc(MaximumBytes);
    const Byte = Buffer.alloc(1);
    let Length = 0;
    process.stdout.write(Prompt);
    process.stdin.setRawMode(true);
    try {
        for (;;) {
            if (fs.readSync(process.stdin.fd, Byte, 0, 1, null) !== 1) {
                Fail("terminal", "Terminal input ended unexpectedly.");
            }
            const Value = Byte[0];
            if (Value === 3) {
                process.stdout.write("\n");
                Fail("cancelled", "Input was cancelled.");
            }
            if (Value === 4 && Length === 0 && AllowEnd) {
                process.stdout.write("\n");
                return null;
            }
            if (Value === 10 || Value === 13) {
                process.stdout.write("\n");
                break;
            }
            if (Value === 8 || Value === 127) {
                if (Length > 0) {
                    Storage[--Length] = 0;
                    process.stdout.write("\b \b");
                }
                continue;
            }
            if (Value < 32) Fail("terminal", "Unsupported control byte in terminal input.");
            if (Length >= MaximumBytes) Fail("input", `Input exceeds its ${MaximumBytes}-byte limit.`);
            Storage[Length++] = Value;
            process.stdout.write(Masked ? "*" : Buffer.from([Value]));
        }
        return Buffer.from(Storage.subarray(0, Length));
    } finally {
        process.stdin.setRawMode(false);
        Byte.fill(0);
        Storage.fill(0);
    }
}

const HOST_TERMINAL = Object.freeze({
    write: Value => process.stdout.write(String(Value)),
    error: Value => process.stderr.write(String(Value)),
    readMasked: (Prompt, MaximumBytes = MAX_SECRET_BYTES) =>
        Readˉterminalˉbytes(Prompt, MaximumBytes, true),
});

function Readˉcredentialˉfile(Value) {
    const FilePath = path.resolve(Value);
    let Stat;
    try { Stat = fs.lstatSync(FilePath); } catch {
        Fail("credential_file", "Credential file cannot be read.");
    }
    if (!Stat.isFile() || Stat.isSymbolicLink() || Stat.size < 177 ||
        Stat.size > MAX_CREDENTIAL_FILE_BYTES) {
        Fail("credential_file", "Credential path must be an ordinary WVSC file.");
    }
    let Bytes;
    try { Bytes = fs.readFileSync(FilePath); } catch {
        Fail("credential_file", "Credential file cannot be read.");
    }
    if (Bytes.length !== Stat.size) {
        Bytes.fill(0);
        Fail("credential_file", "Credential file changed while it was read.");
    }
    return Bytes;
}

function Writeˉnewˉcredentialˉfile(Value, Bytes) {
    const FilePath = path.resolve(Value);
    let Descriptor;
    let Created = false;
    try {
        Descriptor = fs.openSync(FilePath, "wx", 0o600);
        Created = true;
        let Offset = 0;
        while (Offset < Bytes.length) {
            const Written = fs.writeSync(Descriptor, Bytes, Offset, Bytes.length - Offset);
            if (Written < 1) throw new Error("Credential write made no progress.");
            Offset += Written;
        }
        fs.fsyncSync(Descriptor);
        fs.closeSync(Descriptor);
        Descriptor = undefined;
        return FilePath;
    } catch {
        if (Descriptor !== undefined) {
            try { fs.closeSync(Descriptor); } catch {}
        }
        if (Created) {
            try { fs.unlinkSync(FilePath); } catch {}
        }
        Fail("credential_file", "New credential file could not be written; existing files are never replaced.");
    }
}

function Matchingˉsecret(First, Second, Description) {
    const Matches = First.length === Second.length && crypto.timingSafeEqual(First, Second);
    Second.fill(0);
    if (!Matches) {
        First.fill(0);
        Fail("input", `${Description} values do not match.`);
    }
    return First;
}

async function Createˉcredentialˉcommand(Options, Terminal, Dependencies) {
    let Credential;
    let Passphrase;
    let Wrapper;
    try {
        Credential = Matchingˉsecret(
            Terminal.readMasked(`${Options.providerDisplay} API credential: `),
            Terminal.readMasked(`Confirm ${Options.providerDisplay} API credential: `),
            "API credential",
        );
        Passphrase = Matchingˉsecret(
            Terminal.readMasked("Credential passphrase: "),
            Terminal.readMasked("Confirm credential passphrase: "),
            "Credential passphrase",
        );
        Wrapper = await Dependencies.createCredential({
            provider: Options.provider,
            service: Options.service,
            generation: Options.credentialGeneration,
            credential: Credential,
            passphrase: Passphrase,
        });
        const FilePath = Dependencies.writeCredential(Options.outputPath, Wrapper);
        const Metadata = Inspectˉprotectedˉcredential(Wrapper);
        Terminal.write(
            `Protected ${Metadata.provider} credential created at ${FilePath}\n` +
            `service=${Metadata.service} generation=${Metadata.generation} identity=${Metadata.identity}\n`,
        );
    } finally {
        Credential?.fill(0);
        Passphrase?.fill(0);
        Wrapper?.fill(0);
    }
}

function Inspectˉcredentialˉcommand(Options, Terminal, Dependencies) {
    const Wrapper = Dependencies.readCredential(Options.credentialPath);
    try {
        const Metadata = Inspectˉprotectedˉcredential(Wrapper);
        Terminal.write(
            `provider=${Metadata.provider}\nservice=${Metadata.service}\nport=${Metadata.port}\n` +
            `generation=${Metadata.generation}\nidentity=${Metadata.identity}\n` +
            `credential-bytes=${Metadata.credentialBytes}\n`,
        );
    } finally {
        Wrapper.fill(0);
    }
}

async function Openˉgateway(Options, Terminal, Dependencies) {
    const Wrapper = Dependencies.readCredential(Options.credentialPath);
    let Passphrase;
    let Gateway;
    try {
        const Metadata = Inspectˉprotectedˉcredential(Wrapper);
        Passphrase = Terminal.readMasked(`Unlock ${Metadata.provider} credential: `);
        const Lifetime = Math.min(300_000, Options.timeoutMilliseconds + 30_000);
        Gateway = Dependencies.gatewayFactory({
            wrapper: Wrapper,
            passphrase: Passphrase,
            providerGeneration: PROVIDER_GENERATION,
            trustGeneration: TRUST_GENERATION,
            maximumOperationMilliseconds: Options.timeoutMilliseconds,
            maximumLifetimeMilliseconds: Lifetime,
        });
        const Ready = await Gateway.ready();
        if (Ready.provider !== Metadata.provider || Ready.providerGeneration !== PROVIDER_GENERATION ||
            Ready.credentialGeneration !== Metadata.generation || Ready.identity !== Metadata.identity) {
            await Gateway.teardown();
            Fail("gateway", "Model gateway readiness does not match the protected credential.");
        }
        return Object.freeze({ gateway: Gateway, ready: Ready, metadata: Metadata });
    } catch (Error) {
        await Gateway?.teardown().catch(() => {});
        if (Error instanceof Modelˉchatˉfailure) throw Error;
        Fail("gateway", "Protected credential unlock or model gateway startup failed.");
    } finally {
        Wrapper.fill(0);
        Passphrase?.fill(0);
    }
}

function Throwˉproviderˉfailure(Response) {
    const Message = Response.diagnostic.length === 0
        ? `Model provider returned ${Response.statusName}.`
        : Response.diagnostic;
    if (Response.status === Modelˉclientˉstatus.SubmissionIndeterminate) {
        Fail("submission_indeterminate", `${Message} The request was not retried.`);
    }
    Fail(`provider_${Response.statusName}`, Message);
}

async function Modelsˉcommand(Options, Terminal, Dependencies) {
    const Bound = await Openˉgateway(Options, Terminal, Dependencies);
    let RequestId = 1n;
    let Generation = 0n;
    let Continuation = "";
    let Count = 0;
    try {
        do {
            const CurrentId = RequestId++;
            const Request = Encodeˉgatewayˉcatalogˉrequest({
                requestId: CurrentId,
                providerGeneration: Generation,
                limit: Options.pageSize,
                continuation: Continuation,
            });
            let ResponseBytes;
            let Response;
            try {
                try {
                    ResponseBytes = await Bound.gateway.request(Request, Options.timeoutMilliseconds);
                } catch {
                    Fail("provider_unavailable", "Model gateway became unavailable; the catalog request was not retried.");
                }
                Response = Decodeˉgatewayˉcatalogˉresponse(ResponseBytes, CurrentId);
            } finally {
                Request.fill(0);
                ResponseBytes?.fill(0);
            }
            if (Response.status !== Modelˉclientˉstatus.Valid) Throwˉproviderˉfailure(Response);
            if (Generation !== 0n && Response.generation !== Generation) {
                Fail("provider_stale", "Provider catalog generation changed during listing.");
            }
            Generation = Response.generation;
            Continuation = Response.continuation;
            for (const Entry of Response.entries) {
                Terminal.write(`${Entry.id}${Entry.display.length === 0 ? "" : `\t${Entry.display}`}\n`);
                Count += 1;
            }
        } while (Continuation.length !== 0);
        Terminal.write(`Listed ${Count} model${Count === 1 ? "" : "s"} from ${Bound.ready.provider}.\n`);
    } finally {
        await Bound.gateway.teardown();
    }
}

async function Chatˉcommand(Options, Terminal, Dependencies) {
    const Wrapper = Dependencies.readCredential(Options.credentialPath);
    let Passphrase;
    let Native;
    try {
        const Metadata = Inspectˉprotectedˉcredential(Wrapper);
        Passphrase = Terminal.readMasked(`Unlock ${Metadata.provider} credential: `);
        const Lifetime = Math.min(300_000, Options.timeoutMilliseconds + 180_000);
        Native = Dependencies.nativeGatewayFactory({
            applicationPath: Dependencies.nativeApplicationPath(),
            applicationArguments: [
                Metadata.provider, Options.model, String(Options.maximumOutputTokens),
            ],
            wrapper: Wrapper,
            passphrase: Passphrase,
            providerGeneration: PROVIDER_GENERATION,
            trustGeneration: TRUST_GENERATION,
            maximumOperationMilliseconds: Options.timeoutMilliseconds,
            maximumLifetimeMilliseconds: Lifetime,
        });
        const Result = await Native.run();
        if (Result.ready.provider !== Metadata.provider ||
            Result.ready.providerGeneration !== PROVIDER_GENERATION ||
            Result.ready.credentialGeneration !== Metadata.generation ||
            Result.ready.identity !== Metadata.identity) {
            Fail("gateway", "Native model gateway readiness does not match the protected credential.");
        }
        if (Result.signal !== null || !Number.isInteger(Result.code)) {
            Fail("gateway", "Windvale model chat terminated unexpectedly.");
        }
        return Result.code;
    } finally {
        await Native?.teardown().catch(() => {});
        Wrapper.fill(0);
        Passphrase?.fill(0);
    }
}

function Nativeˉapplicationˉpath() {
    const Value = process.env.WINDVALE_MODEL_CHAT_APPLICATION;
    if (typeof Value !== "string" || !path.isAbsolute(Value)) {
        Fail(
            "native_application",
            "The Windvale model-chat application is not built. Run the platform launcher or set WINDVALE_MODEL_CHAT_APPLICATION to its absolute path.",
        );
    }
    let Stat;
    try { Stat = fs.lstatSync(Value); } catch {
        Fail("native_application", "The Windvale model-chat native application cannot be read.");
    }
    if (!Stat.isFile() || Stat.isSymbolicLink()) {
        Fail("native_application", "The Windvale model-chat native application must be an ordinary file.");
    }
    return Value;
}

const DEFAULT_DEPENDENCIES = Object.freeze({
    createCredential: Createˉprotectedˉcredential,
    readCredential: Readˉcredentialˉfile,
    writeCredential: Writeˉnewˉcredentialˉfile,
    gatewayFactory: Values => new Externalˉmodelˉgatewayˉsupervisor(Values),
    nativeApplicationPath: Nativeˉapplicationˉpath,
    nativeGatewayFactory: Values => new Nativeˉexternalˉmodelˉgatewayˉsupervisor(Values),
});

export async function Executeˉmodelˉchat({
    arguments: Arguments,
    terminal = HOST_TERMINAL,
    dependencies = DEFAULT_DEPENDENCIES,
}) {
    const Options = Parseˉmodelˉchatˉarguments(Arguments);
    if (Options.command === "help") {
        terminal.write(`${Modelˉchatˉusage()}\n`);
        return 0;
    }
    if (Options.command === "credential_create") {
        await Createˉcredentialˉcommand(Options, terminal, dependencies);
        return 0;
    }
    if (Options.command === "credential_inspect") {
        Inspectˉcredentialˉcommand(Options, terminal, dependencies);
        return 0;
    }
    if (Options.command === "models") {
        await Modelsˉcommand(Options, terminal, dependencies);
        return 0;
    }
    return await Chatˉcommand(Options, terminal, dependencies);
}

function Exitˉcode(Error) {
    if (Error?.kind === "usage") return 64;
    if (Error?.kind === "cancelled") return 130;
    if (Error?.kind === "submission_indeterminate") return 75;
    if (Error?.kind?.startsWith("provider_")) return 69;
    if (Error?.kind === "credential_file" || Error?.kind === "input" || Error?.kind === "terminal") return 65;
    if (Error?.kind === "native_application") return 69;
    return 70;
}

async function Main() {
    try {
        process.exitCode = await Executeˉmodelˉchat({ arguments: process.argv.slice(2) });
    } catch (Error) {
        process.stderr.write(`Windvale model chat failed: ${Error?.message ?? "Unexpected failure."}\n`);
        process.exitCode = Exitˉcode(Error);
    }
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
    Main();
}
