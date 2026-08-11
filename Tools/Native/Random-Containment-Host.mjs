import { spawn, spawnSync } from "node:child_process";
import {
    access,
    readFile,
    stat,
} from "node:fs/promises";
import { constants as Fsˉconstants } from "node:fs";
import path from "node:path";
import { Decodeˉutf8, Sha256 } from "./Random-Containment-Corpus.mjs";

export function Hostˉartifact(Artifacts) {
    return Artifacts[process.platform];
}

export async function Verifyˉartifact(
    Repositoryˉroot,
    Artifact,
    Executable,
    Boundary,
) {
    const Fileˉpath = path.join(Repositoryˉroot, Artifact[0]);
    const Information = await stat(Fileˉpath);
    Require(Information.isFile(), `The ${Boundary} is not a regular file.`);
    Require(Information.size === Artifact[1], `The ${Boundary} byte length differs.`);
    const Content = await readFile(Fileˉpath);
    Require(Sha256(Content) === Artifact[2], `The ${Boundary} identity differs.`);
    if (Executable && process.platform === "linux") {
        await access(Fileˉpath, Fsˉconstants.X_OK);
    }
    return { Fileˉpath, Content };
}

export async function Requireˉinputˉpreserved(Case) {
    const Bytes = await readFile(Case.Inputˉpath);
    Require(Bytes.byteLength === Case.Byteˉlength && Sha256(Bytes) === Case.Digest,
        `${Case.Name}: native command changed its input.`);
}

export async function Forˉeachˉbounded(Items, Parallelism, Action) {
    let Next = 0;
    async function Worker() {
        while (true) {
            const Index = Next;
            Next += 1;
            if (Index >= Items.length) {
                return;
            }
            await Action(Items[Index]);
        }
    }
    await Promise.all(Array.from({ length: Parallelism }, () => Worker()));
}

export function Runˉprocess(Fileˉpath, Arguments) {
    if (process.platform === "win32") {
        // Keep each Windows status and bounded output pair in one synchronous
        // collection while the explicit native ExitProcess contract is stressed.
        const Result = spawnSync(Fileˉpath, Arguments, {
            stdio: ["ignore", "pipe", "pipe"],
            windowsHide: true,
            maxBuffer: 65_536,
        });
        if (Result.error?.code === "ENOBUFS") {
            throw new Error("A native containment command exceeded its output bound.");
        }
        if (Result.error !== undefined) {
            throw Result.error;
        }
        if (Result.signal !== null) {
            throw new Error(
                `A native containment command ended through signal ${Result.signal}.`,
            );
        }
        return {
            Code: Result.status,
            Output: Result.stdout,
            Error: Result.stderr,
        };
    }
    return new Promise((Resolve, Reject) => {
        const Child = spawn(Fileˉpath, Arguments, {
            stdio: ["ignore", "pipe", "pipe"],
            windowsHide: true,
        });
        const Output = [];
        const Error = [];
        let Outputˉbytes = 0;
        let Errorˉbytes = 0;
        let Exceeded = false;
        Child.stdout.on("data", Chunk => {
            Outputˉbytes += Chunk.byteLength;
            if (Outputˉbytes > 65_536) {
                Exceeded = true;
                Child.kill();
                return;
            }
            Output.push(Chunk);
        });
        Child.stderr.on("data", Chunk => {
            Errorˉbytes += Chunk.byteLength;
            if (Errorˉbytes > 65_536) {
                Exceeded = true;
                Child.kill();
                return;
            }
            Error.push(Chunk);
        });
        Child.on("error", Reject);
        Child.on("close", (Code, Signal) => {
            if (Exceeded) {
                Reject(new Error("A native containment command exceeded its output bound."));
                return;
            }
            if (Signal !== null) {
                Reject(new Error(`A native containment command ended through signal ${Signal}.`));
                return;
            }
            Resolve({ Code, Output: Buffer.concat(Output), Error: Buffer.concat(Error) });
        });
    });
}

export function Oneˉline(Bytes, Boundary) {
    const Text = Decodeˉutf8(Bytes, Boundary).replaceAll("\r\n", "\n");
    Require(Text.endsWith("\n"), `The ${Boundary} lacks its final line ending.`);
    const Lines = Text.slice(0, -1).split("\n");
    Require(Lines.length === 1 && Lines[0].length !== 0,
        `The ${Boundary} is not exactly one nonempty line.`);
    return Lines[0];
}

export function Decodeˉbase64(Text) {
    Require(!Text.includes("\r"), "The fixture encoding must use LF line endings.");
    Require(Text.endsWith("\n"), "The fixture encoding lacks its final LF.");
    const Compact = Text.replaceAll("\n", "");
    const Bytes = Buffer.from(Compact, "base64");
    Require(Bytes.toString("base64") === Compact, "The fixture encoding is not canonical.");
    return Bytes;
}

export function Require(Condition, Message) {
    if (!Condition) {
        throw new Error(Message);
    }
}
