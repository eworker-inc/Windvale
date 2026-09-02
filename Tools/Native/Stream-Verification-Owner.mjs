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
const MAXIMUM_OWNER_MILLISECONDS = 3_600_000;
const MAXIMUM_DETAIL_CHARACTERS = 2_048;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const WINDOWS = process.platform === 'win32';

class Ownerˉstreamˉfailure extends Error {
    constructor(category, message, retryable = false) {
        super(message);
        this.category = category;
        this.retryable = retryable;
    }
}

function Boundedˉdetail(Value) {
    const Text = String(Value ?? '');
    if (Text.length <= MAXIMUM_DETAIL_CHARACTERS) return Text;
    return Text.slice(0, MAXIMUM_DETAIL_CHARACTERS) +
        `...[truncated characters=${Text.length}]`;
}

function Failˉusage() {
    process.stderr.write(
        'Usage: node Tools/Native/Stream-Verification-Owner.mjs ' +
        '<stdout-log> <stderr-log> <owner-script> ' +
        '[<maximum-milliseconds> <process-status-json>]\n'
    );
    process.exitCode = 64;
}

async function Requireˉowner(Path) {
    const Resolved = resolve(Path);
    const Nativeˉroot = dirname(fileURLToPath(import.meta.url));
    if (dirname(Resolved) !== Nativeˉroot ||
        !/^[A-Za-z0-9.-]+$/.test(basename(Resolved))) {
        throw new Ownerˉstreamˉfailure(
            'owner-path',
            'Owner script must be an ordinary file in Tools/Native.',
        );
    }
    const Expectedˉextension = WINDOWS ? '.cmd' : '.sh';
    if (extname(Resolved).toLowerCase() !== Expectedˉextension) {
        throw new Ownerˉstreamˉfailure(
            'owner-path',
            `Owner script must use ${Expectedˉextension}.`,
        );
    }
    await Requireˉordinaryˉdirectoryˉpath(dirname(Resolved));
    const Metadata = await lstat(Resolved);
    if (!Metadata.isFile() || Metadata.isSymbolicLink()) {
        throw new Ownerˉstreamˉfailure(
            'owner-path',
            'Owner script must be a non-linked ordinary file.',
        );
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
            throw new Ownerˉstreamˉfailure(
                'stream-io',
                'Owner log write made no progress.',
                true,
            );
        }
        Offset += Result.bytesWritten;
    }
}

async function Teeˉstream(Source, Log, Destination, Name, Activity, Counts) {
    try {
        for await (const Chunk of Source) {
            Counts[Name] += Chunk.length;
            if (Counts[Name] > MAX_STREAM_BYTES) {
                throw new Ownerˉstreamˉfailure(
                    'output-limit',
                    `Owner ${Name} exceeded the ${MAX_STREAM_BYTES}-byte limit.`,
                );
            }
            Activity.Last = Date.now();
            Activity[Name] = Chunk.length === 0 || Chunk.at(-1) === 10;
            await Writeˉfile(Log, Chunk);
            await Writeˉstream(Destination, Chunk);
        }
    } catch (Errorˉvalue) {
        if (Errorˉvalue instanceof Ownerˉstreamˉfailure) throw Errorˉvalue;
        throw new Ownerˉstreamˉfailure(
            'stream-io',
            `Owner ${Name} streaming failed: ${Errorˉvalue.message}`,
            true,
        );
    }
}

function Delay(Milliseconds) {
    return new Promise(Resolve => setTimeout(Resolve, Milliseconds));
}

function Processˉisˉlive(Child) {
    return Child?.pid !== undefined &&
        Child.exitCode === null && Child.signalCode === null;
}

function Runˉboundedˉtaskkill(Processˉidentifier) {
    return new Promise(Resolve => {
        const Killer = spawn(
            'taskkill.exe',
            ['/pid', String(Processˉidentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true },
        );
        let Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolve('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', Errorˉvalue => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolve(`taskkill error: ${Errorˉvalue.message}`);
        });
        Killer.once('close', Code => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolve(Code === 0 ? null : `taskkill exited ${Code}`);
        });
    });
}

async function Terminateˉprocessˉtree(Child) {
    if (Child?.pid === undefined) return null;
    let Diagnostic = null;
    if (WINDOWS) {
        Diagnostic = await Runˉboundedˉtaskkill(Child.pid);
        if (!Processˉisˉlive(Child) &&
            Diagnostic?.startsWith('taskkill exited ')) {
            Diagnostic = null;
        }
    } else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (Errorˉvalue) {
            if (Errorˉvalue.code !== 'ESRCH') {
                Diagnostic = `process-group kill error: ${Errorˉvalue.message}`;
            }
        }
    }
    if (Processˉisˉlive(Child)) {
        try {
            Child.kill('SIGKILL');
        } catch (Errorˉvalue) {
            Diagnostic = Diagnostic === null
                ? `direct child kill error: ${Errorˉvalue.message}`
                : `${Diagnostic}; direct child kill error: ${Errorˉvalue.message}`;
        }
    }
    return Diagnostic;
}

async function Waitˉforˉboundedˉclose(Completion, Child) {
    const Result = await Promise.race([
        Completion.then(
            Value => ({ closed: true, value: Value }),
            Errorˉvalue => ({ closed: true, error: Errorˉvalue }),
        ),
        Delay(TERMINATION_SETTLE_MILLISECONDS).then(() => ({ closed: false })),
    ]);
    if (Result.closed) return Result.value ?? null;
    try { Child.kill('SIGKILL'); } catch { /* reported by the caller */ }
    Child.stdout.destroy();
    Child.stderr.destroy();
    Child.unref();
    return null;
}

async function Writeˉstatus(Path, Status) {
    const Handle = await open(Path, 'wx', 0o600);
    try {
        await Writeˉfile(Handle, Buffer.from(
            JSON.stringify(Status) + '\n',
            'utf8',
        ));
        await Handle.sync();
    } finally {
        await Handle.close();
    }
}

async function Runˉowner(Outputˉpath, Errorˉpath, Owner, Maximumˉmilliseconds) {
    const Started = Date.now();
    const Counts = { stdout: 0, stderr: 0 };
    let Outputˉlog;
    let Errorˉlog;
    let Heartbeat;
    let Child;
    try {
        Outputˉlog = await open(Outputˉpath, 'wx', 0o600);
        Errorˉlog = await open(Errorˉpath, 'wx', 0o600);
        const Executable = WINDOWS
            ? process.env.ComSpec ?? 'cmd.exe'
            : Owner;
        const Arguments = WINDOWS ? ['/d', '/s', '/c', Owner] : [];
        Child = spawn(Executable, Arguments, {
            detached: !WINDOWS,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
        });
        const Activity = {
            Started,
            Last: Started,
            stdout: true,
            stderr: true,
            Heartbeats: 0,
        };
        Heartbeat = setInterval(() => {
            const Now = Date.now();
            if (Now - Activity.Last >= 30000 &&
                Activity.stdout && Activity.stderr &&
                Activity.Heartbeats < 120) {
                Activity.Heartbeats += 1;
                process.stdout.write(
                    `Progress: step=native-owner-child status=working-silently ` +
                    `owner=${basename(Owner)} ` +
                    `owner-elapsed-seconds=${Math.floor((Now - Started) / 1000)} ` +
                    `silent-seconds=${Math.floor((Now - Activity.Last) / 1000)} ` +
                    `silence-notice=${Activity.Heartbeats}\n`
                );
            }
        }, 30000);
        Heartbeat.unref();

        const Completion = new Promise((Resolve, Reject) => {
            Child.once('error', Reject);
            Child.once('close', (Code, Signal) => Resolve({ Code, Signal }));
        });
        const Outputˉstream = Teeˉstream(
            Child.stdout, Outputˉlog, process.stdout, 'stdout', Activity, Counts
        );
        const Errorˉstream = Teeˉstream(
            Child.stderr, Errorˉlog, process.stderr, 'stderr', Activity, Counts
        );
        const Streamˉfailure = Promise.race([
            Outputˉstream.then(
                () => new Promise(() => {}),
                Errorˉvalue => ({ kind: 'stream-failure', error: Errorˉvalue }),
            ),
            Errorˉstream.then(
                () => new Promise(() => {}),
                Errorˉvalue => ({ kind: 'stream-failure', error: Errorˉvalue }),
            ),
        ]);
        const Completionˉevent = Completion.then(
            Value => ({ kind: 'completed', value: Value }),
            Errorˉvalue => ({ kind: 'launch-failure', error: Errorˉvalue }),
        );
        let Timeout;
        const Timeoutˉevent = Maximumˉmilliseconds === null
            ? new Promise(() => {})
            : new Promise(Resolve => {
                Timeout = setTimeout(
                    () => Resolve({ kind: 'timed-out' }),
                    Maximumˉmilliseconds,
                );
            });
        const First = await Promise.race([
            Completionˉevent,
            Streamˉfailure,
            Timeoutˉevent,
        ]);
        clearTimeout(Timeout);

        if (First.kind === 'completed') {
            const Streams = await Promise.allSettled([Outputˉstream, Errorˉstream]);
            const Failed = Streams.find(Result => Result.status === 'rejected');
            if (Failed !== undefined) throw Failed.reason;
            if (!Number.isInteger(First.value.Code) || First.value.Signal !== null) {
                throw new Ownerˉstreamˉfailure(
                    'process-exit',
                    `Owner process ended without a normal exit code ` +
                    `(signal=${First.value.Signal ?? 'none'}).`,
                );
            }
            return {
                format: 'windvale-verification-owner-process-1',
                outcome: 'exited',
                category: 'process-exit',
                retryable: false,
                exitCode: First.value.Code,
                elapsedMilliseconds: Date.now() - Started,
                stdoutBytes: Counts.stdout,
                stderrBytes: Counts.stderr,
                detail: null,
            };
        }

        const Terminationˉdiagnostic = await Terminateˉprocessˉtree(Child);
        const Close = await Waitˉforˉboundedˉclose(Completion, Child);
        await Promise.allSettled([Outputˉstream, Errorˉstream]);
        if (Terminationˉdiagnostic !== null ||
            (Close === null && First.kind !== 'launch-failure')) {
            throw new Ownerˉstreamˉfailure(
                'tree-termination',
                Terminationˉdiagnostic ??
                    `Owner process tree did not close within ` +
                    `${TERMINATION_SETTLE_MILLISECONDS} ms.`,
            );
        }
        if (First.kind === 'timed-out') {
            return {
                format: 'windvale-verification-owner-process-1',
                outcome: 'timed-out',
                category: 'deadline',
                retryable: false,
                exitCode: null,
                elapsedMilliseconds: Date.now() - Started,
                stdoutBytes: Counts.stdout,
                stderrBytes: Counts.stderr,
                detail: `Owner exceeded ${Maximumˉmilliseconds} ms.`,
            };
        }
        if (First.kind === 'launch-failure') {
            throw new Ownerˉstreamˉfailure(
                'process-launch',
                `Owner process launch failed: ${First.error.message}`,
                true,
            );
        }
        throw First.error;
    } finally {
        clearInterval(Heartbeat);
        await Promise.allSettled([Outputˉlog?.close(), Errorˉlog?.close()]);
    }
}

if (![5, 7].includes(process.argv.length)) {
    Failˉusage();
} else {
    const Started = Date.now();
    let Statusˉpath = null;
    let Status;
    try {
        const Outputˉpath = await Requireˉordinaryˉnewˉpath(process.argv[2]);
        const Errorˉpath = await Requireˉordinaryˉnewˉpath(process.argv[3]);
        if (Outputˉpath === Errorˉpath) {
            throw new Ownerˉstreamˉfailure(
                'log-path',
                'Owner stdout and stderr logs must differ.',
            );
        }
        const Owner = await Requireˉowner(process.argv[4]);
        let Maximumˉmilliseconds = null;
        if (process.argv.length === 7) {
            if (!/^[1-9][0-9]{0,6}$/u.test(process.argv[5])) {
                throw new Ownerˉstreamˉfailure(
                    'duration-policy',
                    'Owner maximum duration is invalid.',
                );
            }
            Maximumˉmilliseconds = Number.parseInt(process.argv[5], 10);
            if (Maximumˉmilliseconds > MAXIMUM_OWNER_MILLISECONDS) {
                throw new Ownerˉstreamˉfailure(
                    'duration-policy',
                    'Owner maximum duration exceeds one hour.',
                );
            }
            Statusˉpath = await Requireˉordinaryˉnewˉpath(process.argv[6]);
            if ([Outputˉpath, Errorˉpath].includes(Statusˉpath)) {
                throw new Ownerˉstreamˉfailure(
                    'status-path',
                    'Owner process-status path must differ from its logs.',
                );
            }
        }
        Status = await Runˉowner(
            Outputˉpath,
            Errorˉpath,
            Owner,
            Maximumˉmilliseconds,
        );
        process.exitCode = Status.outcome === 'timed-out'
            ? 124
            : Status.exitCode;
    } catch (Errorˉvalue) {
        const Category = Errorˉvalue instanceof Ownerˉstreamˉfailure
            ? Errorˉvalue.category
            : 'framework';
        const Retryable = Errorˉvalue instanceof Ownerˉstreamˉfailure
            ? Errorˉvalue.retryable
            : false;
        const Message = Boundedˉdetail(
            `Verification owner stream failure: ${Errorˉvalue.message}`
        );
        process.stderr.write(Message + '\n');
        Status = {
            format: 'windvale-verification-owner-process-1',
            outcome: 'framework-error',
            category: Category,
            retryable: Retryable,
            exitCode: null,
            elapsedMilliseconds: Date.now() - Started,
            stdoutBytes: 0,
            stderrBytes: 0,
            detail: Message,
        };
        process.exitCode = 70;
    }

    if (Statusˉpath !== null) {
        try {
            await Writeˉstatus(Statusˉpath, Status);
        } catch (Errorˉvalue) {
            process.stderr.write(
                `Verification owner status write failure: ${Errorˉvalue.message}\n`
            );
            process.exitCode = 70;
        }
    }
}
