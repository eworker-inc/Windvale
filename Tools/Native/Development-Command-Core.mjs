import { spawn, spawnSync } from 'node:child_process';
import { closeSync, openSync, opendirSync, readSync } from 'node:fs';
import { basename, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const REPOSITORY = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const WINDOWS = process.platform === 'win32';
const LINUX = process.platform === 'linux';
const MAXIMUM_PROCESSES = 256;
const MAXIMUM_THREAD_READS = 4_096;
const MAXIMUM_PROC_BYTES = 65_536;
const TERMINATION_MILLISECONDS = 3_000;

export function Requireˉwindowsˉterminationˉresult(Result) {
    if (Result?.status !== 0 || Result?.error != null || Result?.signal != null) {
        const Detail = String(Result?.error?.message ?? Result?.signal ?? Result?.status ?? 'unavailable')
            .replace(/[\r\n\t]/gu, ' ').slice(0, 256);
        throw Object.assign(new Error(`Windows development process tree termination failed: ${Detail}`),
            { exitCode: 2, cleanupUncertain: true });
    }
}

function Readˉprocˉtext(Path, Maximumˉbytes = 4_096) {
    const Descriptor = openSync(Path, 'r');
    try {
        const Bufferˉvalue = Buffer.alloc(Maximumˉbytes + 1);
        var Length = 0;
        while (Length < Bufferˉvalue.length) {
            const Read = readSync(Descriptor, Bufferˉvalue, Length, Bufferˉvalue.length - Length, null);
            if (Read === 0) return Bufferˉvalue.subarray(0, Length).toString('utf8');
            Length += Read;
        }
        throw new Error('Development process inspection exceeded its byte limit.');
    } finally { closeSync(Descriptor); }
}

function Readˉprocessˉidentity(Pid, Thread = null) {
    try {
        const Text = Readˉprocˉtext(Thread === null ? `/proc/${Pid}/stat` : `/proc/${Pid}/task/${Thread}/stat`);
        const End = Text.lastIndexOf(') ');
        const Fields = Text.slice(End + 2).trim().split(/\s+/u);
        if (End < 0 || Fields.length < 20 || !/^\d+$/u.test(Fields[19]) ||
            !Fields.slice(1, 4).every(Value => /^\d+$/u.test(Value) && Number.isSafeInteger(Number(Value)))) {
            throw new Error('Development process identity is malformed.');
        }
        return { Pid, Parent: Number(Fields[1]), Group: Number(Fields[2]), Session: Number(Fields[3]),
            Start: Fields[19], State: Fields[0] };
    } catch (Error) {
        if (Error.code === 'ENOENT' || Error.code === 'ESRCH') return null;
        throw Error;
    }
}

function Sameˉprocess(Current, Expected) {
    return Current !== null && Current.Pid === Expected.Pid && Current.Start === Expected.Start;
}

async function Stopˉlinuxˉtree(Root) {
    const Deadline = Date.now() + TERMINATION_MILLISECONDS;
    const Processes = [];
    const Seen = new Set();
    var Threadˉreads = 0;
    var Failure = null;
    function Checkˉbudget() {
        if (Date.now() >= Deadline) throw new Error('Development process termination exceeded its time limit.');
    }
    function Threads(Pid) {
        const Directory = opendirSync(`/proc/${Pid}/task`);
        const Result = [];
        try {
            for (var Entry = Directory.readSync(); Entry !== null; Entry = Directory.readSync()) {
                Checkˉbudget();
                if (!/^\d+$/u.test(Entry.name)) continue;
                if (++Threadˉreads > MAXIMUM_THREAD_READS) {
                    throw new Error('Development process termination exceeded its thread limit.');
                }
                Result.push(Number(Entry.name));
            }
        } finally { Directory.closeSync(); }
        return Result.sort((Left, Right) => Left - Right);
    }
    async function Freeze(Identity) {
        Checkˉbudget();
        if (Seen.has(Identity.Pid)) return;
        if (Processes.length >= MAXIMUM_PROCESSES) {
            throw new Error('Development process termination exceeded its descendant limit.');
        }
        const Current = Readˉprocessˉidentity(Identity.Pid);
        if (!Sameˉprocess(Current, Identity) || Current.Parent !== Identity.Parent) {
            throw new Error('Development descendant changed before it could be stopped.');
        }
        Seen.add(Identity.Pid);
        Processes.push(Identity);
        if (Current.State === 'Z' || Current.State === 'X') {
            throw new Error('Development descendant died before containment inventory.');
        }
        process.kill(Identity.Pid, 'SIGSTOP');
        var Previous = null;
        var Stoppedˉthreads;
        while (true) {
            Checkˉbudget();
            const Leader = Readˉprocessˉidentity(Identity.Pid);
            if (!Sameˉprocess(Leader, Identity) || Leader.State === 'Z' || Leader.State === 'X') {
                throw new Error('Development descendant exited during containment.');
            }
            Stoppedˉthreads = Threads(Identity.Pid);
            const Stopped = Stoppedˉthreads.length > 0 && Stoppedˉthreads.every(Thread => {
                const State = Readˉprocessˉidentity(Identity.Pid, Thread)?.State;
                if (State === undefined || State === 'Z' || State === 'X') {
                    throw new Error('Development thread exited during containment inventory.');
                }
                return State === 'T' || State === 't';
            });
            const Key = Stoppedˉthreads.join(',');
            if (Stopped && Previous === Key) break;
            Previous = Stopped ? Key : null;
            await new Promise(Complete => setTimeout(Complete, 5));
        }
        // Freeze each ancestor before reading all its threads' children. Detached
        // sessions remain descendants even when a process-group kill cannot reach them.
        const Children = new Set();
        for (const Thread of Stoppedˉthreads) {
            Checkˉbudget();
            for (const Token of Readˉprocˉtext(`/proc/${Identity.Pid}/task/${Thread}/children`, MAXIMUM_PROC_BYTES).trim().split(/\s+/u)) {
                if (Token === '') continue;
                const Pid = Number(Token);
                if (!/^\d+$/u.test(Token) || !Number.isSafeInteger(Pid) || Pid <= 1) {
                    throw new Error('Development descendant identity is malformed.');
                }
                Children.add(Pid);
                if (Children.size + Processes.length > MAXIMUM_PROCESSES) {
                    throw new Error('Development process termination exceeded its descendant limit.');
                }
            }
        }
        for (const Pid of Children) {
            const Child = Readˉprocessˉidentity(Pid);
            if (Child === null || Child.Parent !== Identity.Pid) {
                throw new Error('Development descendant changed during enumeration.');
            }
            await Freeze(Child);
        }
        const Confirmed = Readˉprocessˉidentity(Identity.Pid);
        if (!Sameˉprocess(Confirmed, Identity) || (Confirmed.State !== 'T' && Confirmed.State !== 't')) {
            throw new Error('Development ancestor changed before containment inventory completed.');
        }
    }
    try {
        if (Root === null) throw new Error('Development root identity is unavailable for containment.');
        await Freeze(Root);
    } catch (Error) { Failure = Error; }
    // Always release collected stopped processes, even after a bound or inspection
    // failure. Start-time checks avoid signaling a reused PID; no global PID search.
    for (const Identity of [...Processes].reverse()) {
        try {
            const Current = Readˉprocessˉidentity(Identity.Pid);
            if (!Sameˉprocess(Current, Identity)) continue;
            if (Identity === Root && Current.Group === Root.Pid && Current.Session === Root.Pid) {
                process.kill(-Root.Pid, 'SIGKILL');
            } else process.kill(Identity.Pid, 'SIGKILL');
        } catch (Error) {
            if (Error.code !== 'ESRCH') Failure ??= Error;
        }
    }
    while (Processes.length > 0) {
        try {
            const Live = Processes.some(Identity => {
                const Current = Readˉprocessˉidentity(Identity.Pid);
                return Sameˉprocess(Current, Identity) && Current.State !== 'Z' && Current.State !== 'X';
            });
            if (!Live) break;
            Checkˉbudget();
            await new Promise(Complete => setTimeout(Complete, 10));
        } catch (Error) { Failure ??= Error; break; }
    }
    if (Failure !== null) throw Failure;
}

export async function Runˉdevelopmentˉcommand(
    Tool, Arguments, Deadline, Streamˉoutput = false, Maximumˉoutputˉbytes = 65_536,
) {
    if (!Number.isSafeInteger(Deadline) || !Number.isSafeInteger(Maximumˉoutputˉbytes) ||
        Maximumˉoutputˉbytes < 1 || Maximumˉoutputˉbytes > 1_048_576) {
        throw new Error('Invalid development command bounds.');
    }
    const Remaining = Deadline - Date.now();
    if (Remaining <= 0) throw Object.assign(new Error('Development command timed out.'), { exitCode: 124 });
    const Shell = WINDOWS && Tool.endsWith('.cmd');
    if (Shell && [Tool, ...Arguments].some(Value => /[\r\n&|<>^%!"]/u.test(Value))) {
        throw new Error('Unsafe Windows development command argument.');
    }
    const Command = Shell ? process.env.ComSpec ?? 'cmd.exe' : Tool;
    const Parameters = Shell ? ['/d', '/v:off', '/s', '/c',
        `"${[Tool, ...Arguments].map(Value => `"${Value}"`).join(' ')}"`] : Arguments;
    return new Promise((Complete, Reject) => {
        const Child = spawn(Command, Parameters, {
            cwd: REPOSITORY, windowsHide: true, windowsVerbatimArguments: Shell,
            detached: !WINDOWS, stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Output = [];
        const Errors = [];
        var Bytes = 0;
        var Failure = null;
        var Settle = null;
        var Closed = false;
        var Completed = false;
        var Terminationˉcomplete = true;
        var Childˉcode = null;
        var Rootˉidentity = null;
        if (LINUX && Child.pid !== undefined) {
            try { Rootˉidentity = Readˉprocessˉidentity(Child.pid); } catch { /* Report only if cleanup is needed. */ }
        }
        function Stop(Message, ExitCode) {
            if (Failure !== null) return;
            Failure = Object.assign(new Error(Message), { exitCode: ExitCode });
            Terminationˉcomplete = false;
            Settle = setTimeout(() => {
                Failure = Object.assign(new Error('Development process termination did not settle.'),
                    { exitCode: 2, cleanupUncertain: true });
                Cleanup();
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                if (!Completed) { Completed = true; Reject(Failure); }
            }, 5_000);
            void (async () => {
                try {
                    if (Child.pid !== undefined) {
                        if (WINDOWS) {
                            const Result = spawnSync('taskkill.exe', ['/pid', String(Child.pid), '/t', '/f'], {
                                windowsHide: true, timeout: 2_000, stdio: 'ignore',
                            });
                            Child.kill('SIGKILL');
                            Requireˉwindowsˉterminationˉresult(Result);
                        } else if (LINUX) await Stopˉlinuxˉtree(Rootˉidentity);
                        else {
                            try { process.kill(-Child.pid, 'SIGKILL'); } catch (Error) {
                                if (Error.code !== 'ESRCH') throw Error;
                            }
                            Child.kill('SIGKILL');
                        }
                    }
                } catch (Error) {
                    const Cause = String(Error?.message ?? Error).replace(/[\r\n\t]/gu, ' ').slice(0, 256);
                    Failure = Object.assign(new globalThis.Error(
                        `Development process termination could not prove bounded cleanup: ${Cause}`),
                    { exitCode: 2, cleanupUncertain: true });
                } finally {
                    Terminationˉcomplete = true;
                    Finish();
                }
            })();
        }
        const Timer = setTimeout(() => Stop('Development command timed out.', 124), Remaining);
        const Activity = setInterval(() => process.stdout.write(
            `INFO development command step=active tool=${basename(Tool)}\n`), 30_000);
        for (const [Stream, Chunks] of [[Child.stdout, Output], [Child.stderr, Errors]]) {
            Stream.on('data', Chunk => {
                Bytes += Chunk.length;
                if (Bytes > Maximumˉoutputˉbytes) Stop('Development diagnostic limit exceeded.', 2);
                else {
                    Chunks.push(Chunk);
                    if (Streamˉoutput && Stream === Child.stdout) process.stdout.write(Chunk);
                }
            });
        }
        function Cleanup() { clearTimeout(Timer); clearTimeout(Settle); clearInterval(Activity); }
        function Finish() {
            if (Completed || !Closed || !Terminationˉcomplete) return;
            Completed = true;
            Cleanup();
            if (Failure) Reject(Failure);
            else Complete({ Code: Childˉcode, Output: Buffer.concat(Output).toString('utf8'),
                Error: Buffer.concat(Errors).toString('utf8') });
        }
        Child.once('error', Error => {
            if (Failure !== null) {
                const Cause = String(Error?.message ?? Error).replace(/[\r\n\t]/gu, ' ').slice(0, 256);
                Failure = Object.assign(new globalThis.Error(`Development process error during termination: ${Cause}`),
                    { exitCode: 2, cleanupUncertain: true });
                Finish();
                return;
            }
            Cleanup(); Completed = true; Reject(Error);
        });
        Child.once('close', Code => {
            Closed = true;
            Childˉcode = Code;
            Finish();
        });
    });
}
