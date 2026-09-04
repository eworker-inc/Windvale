import { spawn, spawnSync } from 'node:child_process';
import { basename, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const REPOSITORY = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const WINDOWS = process.platform === 'win32';

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
        function Stop(Message, ExitCode) {
            if (Failure !== null) return;
            Failure = Object.assign(new Error(Message), { exitCode: ExitCode });
            if (Child.pid !== undefined) {
                if (WINDOWS) {
                    spawnSync('taskkill.exe', ['/pid', String(Child.pid), '/t', '/f'], {
                        windowsHide: true, timeout: 2_000, stdio: 'ignore',
                    });
                } else {
                    try { process.kill(-Child.pid, 'SIGKILL'); } catch (Error) {
                        if (Error.code !== 'ESRCH') Failure = Error;
                    }
                }
                Child.kill('SIGKILL');
            }
            Settle = setTimeout(() => {
                Cleanup();
                Child.stdout.destroy();
                Child.stderr.destroy();
                Reject(Failure);
            }, 5_000);
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
        Child.once('error', Error => { Cleanup(); Reject(Error); });
        Child.once('close', Code => {
            Cleanup();
            if (Failure) Reject(Failure);
            else Complete({ Code, Output: Buffer.concat(Output).toString('utf8'),
                Error: Buffer.concat(Errors).toString('utf8') });
        });
    });
}
