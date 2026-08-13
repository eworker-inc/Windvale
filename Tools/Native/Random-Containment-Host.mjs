import { spawn } from "node:child_process";
import {
    access,
    readFile,
    stat,
} from "node:fs/promises";
import { constants as Fsˉconstants } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { Decodeˉutf8, Sha256 } from "./Random-Containment-Corpus.mjs";

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Windowsˉcollector = path.join(
    Scriptˉdirectory,
    "Collect-Windows-Containment-Process.cmd",
);

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
    const Collectˉwindowsˉstatus = process.platform === "win32";
    if (Collectˉwindowsˉstatus) {
        Require(
            Arguments.length >= 1 && Arguments.length <= 2,
            "The Windows containment collector supports one or two child arguments.",
        );
    }
    const Program = Collectˉwindowsˉstatus ? Windowsˉcollector : Fileˉpath;
    const Processˉarguments = Collectˉwindowsˉstatus
        ? []
        : Arguments;
    const Environment = Collectˉwindowsˉstatus
        ? {
            ...process.env,
            WINDVALE_CONTAINMENT_CHILD: Fileˉpath,
            WINDVALE_CONTAINMENT_ARGUMENT_COUNT: Arguments.length.toString(),
            WINDVALE_CONTAINMENT_ARGUMENT_0: Arguments[0],
            WINDVALE_CONTAINMENT_ARGUMENT_1: Arguments[1] ?? "",
        }
        : process.env;
    return new Promise((Resolve, Reject) => {
        const Child = spawn(Program, Processˉarguments, {
            stdio: ["ignore", "pipe", "pipe"],
            windowsHide: true,
            env: Environment,
            shell: Collectˉwindowsˉstatus,
        });
        const Output = [];
        const Diagnostic = [];
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
            Diagnostic.push(Chunk);
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
            const Outputˉbytes = Buffer.concat(Output);
            if (Collectˉwindowsˉstatus) {
                if (Code !== 0) {
                    Reject(new Error(
                        `The Windows containment collector exited ${Code}; ` +
                            `diagnostic=${JSON.stringify(Buffer.concat(Diagnostic).toString("utf8"))}.`,
                    ));
                    return;
                }
                const Marker = Buffer.from("windvale-child-exit=", "ascii");
                const Markerˉoffset = Outputˉbytes.lastIndexOf(Marker);
                if (Markerˉoffset < 0) {
                    Reject(new Error("The Windows containment collector omitted its status."));
                    return;
                }
                const Statusˉtext = Outputˉbytes.subarray(Markerˉoffset).toString("ascii");
                const Statusˉmatch = /^windvale-child-exit=([0-9]+)\r\n$/u.exec(Statusˉtext);
                if (Statusˉmatch === null) {
                    Reject(new Error("The Windows containment collector status is malformed."));
                    return;
                }
                const Status = Number(Statusˉmatch[1]);
                if (!Number.isSafeInteger(Status) || Status > 4_294_967_295) {
                    Reject(new Error("The Windows containment collector status is out of range."));
                    return;
                }
                Resolve({
                    Code: Status,
                    Output: Outputˉbytes.subarray(0, Markerˉoffset),
                    Error: Buffer.concat(Diagnostic),
                });
                return;
            }
            Resolve({ Code, Output: Outputˉbytes, Error: Buffer.concat(Diagnostic) });
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
