import { open, lstat } from 'node:fs/promises';
import { dirname, extname, basename, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { fileURLToPath } from 'node:url';
import {
    Requireˉordinaryˉdirectoryˉpath,
    Requireˉordinaryˉnewˉpath,
} from './Verification-Owner-Stream-Path.mjs';

const MAX_STREAM_BYTES = 8 * 1024 * 1024;

function Failˉusage() {
    process.stderr.write(
        'Usage: node Tools/Native/Stream-Verification-Owner.mjs ' +
        '<stdout-log> <stderr-log> <owner-script>\n'
    );
    process.exitCode = 64;
}

async function Requireˉowner(Path) {
    const Resolved = resolve(Path);
    const Nativeˉroot = dirname(fileURLToPath(import.meta.url));
    if (dirname(Resolved) !== Nativeˉroot ||
        !/^[A-Za-z0-9.-]+$/.test(basename(Resolved))) {
        throw new Error('Owner script must be an ordinary file in Tools/Native.');
    }
    const Expectedˉextension = process.platform === 'win32' ? '.cmd' : '.sh';
    if (extname(Resolved).toLowerCase() !== Expectedˉextension) {
        throw new Error(`Owner script must use ${Expectedˉextension}.`);
    }
    await Requireˉordinaryˉdirectoryˉpath(dirname(Resolved));
    const Metadata = await lstat(Resolved);
    if (!Metadata.isFile() || Metadata.isSymbolicLink()) {
        throw new Error('Owner script must be a non-linked ordinary file.');
    }
    return Resolved;
}

async function Writeˉstream(Stream, Chunk) {
    if (!Stream.write(Chunk)) {
        await once(Stream, 'drain');
    }
}

async function Writeˉfile(File, Value) {
    const Bytes = Buffer.isBuffer(Value) ? Value : Buffer.from(Value);
    let Offset = 0;
    while (Offset < Bytes.length) {
        const Result = await File.write(
            Bytes,
            Offset,
            Bytes.length - Offset,
            null
        );
        if (Result.bytesWritten <= 0) {
            throw new Error('Owner log write made no progress.');
        }
        Offset += Result.bytesWritten;
    }
}

async function Teeˉstream(Source, Log, Destination, Name, Child, Activity) {
    let Bytes = 0;
    for await (const Chunk of Source) {
        Bytes += Chunk.length;
        if (Bytes > MAX_STREAM_BYTES) {
            Child.kill();
            throw new Error(
                `Owner ${Name} exceeded the ${MAX_STREAM_BYTES}-byte limit.`
            );
        }
        Activity.Last = Date.now();
        Activity[Name] = Chunk.length === 0 || Chunk.at(-1) === 10;
        await Writeˉfile(Log, Chunk);
        await Writeˉstream(Destination, Chunk);
    }
}

if (process.argv.length !== 5) {
    Failˉusage();
} else {
    let Outputˉlog;
    let Errorˉlog;
    let Heartbeat;
    try {
        const Outputˉpath = await Requireˉordinaryˉnewˉpath(process.argv[2]);
        const Errorˉpath = await Requireˉordinaryˉnewˉpath(process.argv[3]);
        if (Outputˉpath === Errorˉpath) {
            throw new Error('Owner stdout and stderr logs must differ.');
        }
        const Owner = await Requireˉowner(process.argv[4]);
        Outputˉlog = await open(Outputˉpath, 'wx', 0o600);
        Errorˉlog = await open(Errorˉpath, 'wx', 0o600);

        const Child = process.platform === 'win32'
            ? spawn(process.env.ComSpec ?? 'cmd.exe', ['/d', '/s', '/c', Owner], {
                stdio: ['ignore', 'pipe', 'pipe'],
                windowsHide: true
            })
            : spawn(Owner, [], { stdio: ['ignore', 'pipe', 'pipe'] });
        const Activity = {
            Last: Date.now(),
            stdout: true,
            stderr: true,
            Heartbeats: 0
        };
        Heartbeat = setInterval(() => {
            if (Date.now() - Activity.Last >= 30000 &&
                Activity.stdout && Activity.stderr &&
                Activity.Heartbeats < 240) {
                Activity.Heartbeats += 1;
                process.stdout.write(
                    `Progress: step=native-owner-child status=active ` +
                    `owner=${basename(Owner)} heartbeat=${Activity.Heartbeats}/240\n`
                );
            }
        }, 30000);
        Heartbeat.unref();
        const Completion = new Promise((Resolve, Reject) => {
            Child.once('error', Reject);
            Child.once('close', (Code, Signal) => Resolve({ Code, Signal }));
        });
        const Results = await Promise.allSettled([
            Completion,
            Teeˉstream(
                Child.stdout, Outputˉlog, process.stdout, 'stdout', Child, Activity
            ),
            Teeˉstream(
                Child.stderr, Errorˉlog, process.stderr, 'stderr', Child, Activity
            )
        ]);
        const Failure = Results.find(Result => Result.status === 'rejected');
        if (Failure) {
            Child.kill();
            throw Failure.reason;
        }
        const { Code, Signal } = Results[0].value;
        process.exitCode = Number.isInteger(Code) ? Code : 1;
        if (Signal !== null) {
            throw new Error(`Owner process ended with signal ${Signal}.`);
        }
    } catch (Errorˉvalue) {
        const Message = `Verification owner stream failure: ${Errorˉvalue.message}\n`;
        if (Errorˉlog) {
            await Writeˉfile(Errorˉlog, Message);
        }
        process.stderr.write(Message);
        process.exitCode = process.exitCode || 1;
    } finally {
        clearInterval(Heartbeat);
        await Promise.allSettled([Outputˉlog?.close(), Errorˉlog?.close()]);
    }
}
