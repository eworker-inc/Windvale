import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    lstat,
    mkdtemp,
    readFile,
    realpath,
    rm,
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 65_536;
const HEARTBEAT_INTERVAL_MILLISECONDS = 30_000;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;
const BUILD_TIMEOUT_MILLISECONDS = 600_000;
const ACQUISITION_TIMEOUT_MILLISECONDS = 1_200_000;
const CASE_TIMEOUT_MILLISECONDS = 120_000;
const EXPECTED_WVB_BYTES = 634_819;
const EXPECTED_WVB_SHA256 =
    '7d004263b350097f8bcd82997d4210464ee2dba8937a5c0b76d32b8139f99ac1';
const SELECTORS = [...'abcdefghijklmnopqrstuvwxyzAB'];
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');

function Reject(Message) {
    throw new Error(Message);
}

function Processˉisˉlive(Child) {
    return Child.pid !== undefined &&
        Child.exitCode === null && Child.signalCode === null;
}

function Runˉboundedˉtaskkill(Processˉidentifier) {
    return new Promise(Resolveˉresult => {
        const Killer = spawn(
            'taskkill.exe',
            ['/pid', String(Processˉidentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true }
        );
        var Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolveˉresult('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', Errorˉvalue => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(`taskkill error: ${Errorˉvalue.message}`);
        });
        Killer.once('close', Code => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            Resolveˉresult(Code === 0 ? null : `taskkill exited ${Code}`);
        });
    });
}

async function Terminateˉprocessˉtree(Child) {
    if (!Processˉisˉlive(Child)) return null;
    var Diagnostic = null;
    if (WINDOWS) {
        Diagnostic = await Runˉboundedˉtaskkill(Child.pid);
    }
    else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (Errorˉvalue) {
            Diagnostic = `process-group kill error: ${Errorˉvalue.message}`;
        }
    }
    if (Processˉisˉlive(Child)) {
        try {
            if (!Child.kill('SIGKILL') && Diagnostic !== null) {
                Diagnostic += '; direct child kill returned false';
            }
        } catch (Errorˉvalue) {
            if (Diagnostic !== null) {
                Diagnostic +=
                    `; direct child kill error: ${Errorˉvalue.message}`;
            }
        }
    }
    return Diagnostic;
}

function Runˉcommand(
    Tool,
    Argumentsˉvalue,
    Timeout,
    Relayˉoutput = false,
    Activity = 'command'
) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Isˉcommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        if (Isˉcommand && [Tool, ...Argumentsˉvalue].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument)
        )) {
            Rejectˉpromise(new Error(
                'A Windows coordinator-owner argument contains shell metacharacters.'
            ));
            return;
        }
        const Executable = Isˉcommand
            ? process.env.ComSpec ?? 'cmd.exe'
            : Tool;
        const Commandˉarguments = Isˉcommand
            ? [
                '/d', '/v:off', '/s', '/c',
                `"${[Tool, ...Argumentsˉvalue]
                    .map(Argument => `"${Argument}"`)
                    .join(' ')}"`,
            ]
            : Argumentsˉvalue;
        const Started = Date.now();
        const Child = spawn(Executable, Commandˉarguments, {
            cwd: REPOSITORY_ROOT,
            detached: !WINDOWS,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand,
        });
        const Heartbeat = setInterval(() => {
            process.stdout.write(
                `INFO  language 1 source admission coordinator active ` +
                `step=${Activity} elapsed-ms=${Date.now() - Started}\n`
            );
        }, HEARTBEAT_INTERVAL_MILLISECONDS);
        Heartbeat.unref();
        const Output = [];
        const Errorˉoutput = [];
        var Outputˉbytes = 0;
        var Errorˉbytes = 0;
        var Exceeded = false;
        var Timedˉout = false;
        var Settled = false;
        var Cleanupˉfailure = null;
        var Terminationˉdiagnostic = null;
        var Terminationˉpromise = null;
        var Closeˉcode = null;
        var Closeˉreceived = false;
        var Settleˉtimer;
        function Result(Code, Forced) {
            return {
                Code,
                Elapsed: Date.now() - Started,
                Error: Buffer.concat(Errorˉoutput),
                Cleanupˉfailure,
                Exceeded,
                Forced,
                Output: Buffer.concat(Output),
                Timedˉout,
            };
        }
        function Complete(Code, Forced) {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            clearInterval(Heartbeat);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Resolveˉresult(Result(Code, Forced));
        }
        function Terminateˉandˉsettle() {
            if (Terminationˉpromise !== null) return;
            Settleˉtimer = setTimeout(() => {
                try {
                    Child.kill('SIGKILL');
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic = Terminationˉdiagnostic ??
                        `final direct child kill error: ${Errorˉvalue.message}`;
                }
                const Settleˉdiagnostic =
                    `process tree did not close within ` +
                    `${TERMINATION_SETTLE_MILLISECONDS} ms`;
                Cleanupˉfailure = Terminationˉdiagnostic === null
                    ? Settleˉdiagnostic
                    : `${Terminationˉdiagnostic}; ${Settleˉdiagnostic}`;
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                Complete(null, true);
            }, TERMINATION_SETTLE_MILLISECONDS);
            Terminationˉpromise = (async () => {
                try {
                    Terminationˉdiagnostic =
                        await Terminateˉprocessˉtree(Child);
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic =
                        `tree termination error: ${Errorˉvalue.message}`;
                    try {
                        Child.kill('SIGKILL');
                    } catch {
                        // The bounded settle path records the diagnostic.
                    }
                }
                Cleanupˉfailure = Terminationˉdiagnostic;
                if (Closeˉreceived) Complete(Closeˉcode, false);
            })();
        }
        const Timer = setTimeout(() => {
            Timedˉout = true;
            Terminateˉandˉsettle();
        }, Timeout);
        Child.stdout.on('data', Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                Output.push(Chunk);
                if (Relayˉoutput) process.stdout.write(Chunk);
            }
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        });
        Child.stderr.on('data', Chunk => {
            Errorˉbytes += Chunk.length;
            if (Errorˉbytes <= MAXIMUM_OUTPUT_BYTES) Errorˉoutput.push(Chunk);
            else if (!Exceeded) {
                Exceeded = true;
                Terminateˉandˉsettle();
            }
        });
        Child.once('error', Errorˉvalue => {
            if (Timedˉout || Exceeded) return;
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            clearInterval(Heartbeat);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Rejectˉpromise(Errorˉvalue);
        });
        Child.once('close', Code => {
            Closeˉreceived = true;
            Closeˉcode = Code;
            if (Terminationˉpromise === null) {
                Complete(Code, false);
                return;
            }
            void Terminationˉpromise.then(() => Complete(Code, false));
        });
    });
}

async function Requireˉcommand(
    Label,
    Tool,
    Argumentsˉvalue,
    Timeout,
    Allowˉoutput = true,
    Relayˉoutput = false
) {
    const Result = await Runˉcommand(
        Tool, Argumentsˉvalue, Timeout, Relayˉoutput, Label
    );
    if (Result.Cleanupˉfailure !== null) {
        Reject(`${Label} cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject(`${Label} exceeded its time bound.`);
    if (Result.Exceeded) Reject(`${Label} exceeded its output bound.`);
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `${Label} failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
    if (!Allowˉoutput && Result.Output.length !== 0) {
        Reject(`${Label} wrote unexpected output.`);
    }
    return Result;
}

async function Fileˉidentity(Candidate, Label) {
    const Information = await lstat(Candidate);
    if (!Information.isFile() || Information.size < 1 ||
        Information.size > 67_108_864) {
        Reject(`The ${Label} is not a bounded ordinary file.`);
    }
    const Bytes = await readFile(Candidate);
    return {
        bytes: Bytes.length,
        sha256: createHash('sha256').update(Bytes).digest('hex'),
        value: Bytes,
    };
}

function Requireˉexpectedˉwvb(Identity, Label) {
    if (Identity.bytes !== EXPECTED_WVB_BYTES ||
        Identity.sha256 !== EXPECTED_WVB_SHA256) {
        Reject(
            `The ${Label} identity differs: bytes=${Identity.bytes} ` +
            `sha256=${Identity.sha256}.`
        );
    }
}

async function Runˉcase(Application, Selector, Index) {
    const Result = await Runˉcommand(
        Application,
        [Selector],
        CASE_TIMEOUT_MILLISECONDS,
        false,
        `execute-${Index}-selector-${Selector}`
    );
    if (Result.Cleanupˉfailure !== null) {
        Reject(
            `Coordinator case ${Index} cleanup failed: ` +
            `${Result.Cleanupˉfailure}.`
        );
    }
    if (Result.Timedˉout) Reject(`Coordinator case ${Index} timed out.`);
    if (Result.Exceeded) Reject(`Coordinator case ${Index} exceeded output bounds.`);
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `Coordinator case ${Index} wrote output.\n` +
            Result.Output.toString('utf8') + Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) {
        Reject(`Coordinator case ${Index} returned ${Result.Code}.`);
    }
    return Result.Elapsed;
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    const Information = await lstat(Work);
    const Realˉwork = await realpath(Work);
    if (Realˉparent !== Realˉroot ||
        !Information.isDirectory() || Information.isSymbolicLink() ||
        Realˉwork !== resolve(Work) ||
        !basename(Work).startsWith('windvale-source-admission-coordinator-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

function Processˉidentifierˉisˉlive(Processˉidentifier) {
    try {
        process.kill(Processˉidentifier, 0);
        return true;
    } catch (Errorˉvalue) {
        if (Errorˉvalue.code === 'ESRCH') return false;
        throw Errorˉvalue;
    }
}

async function Waitˉforˉprocessˉexit(Processˉidentifier) {
    const Deadline = Date.now() + 1_000;
    while (Processˉidentifierˉisˉlive(Processˉidentifier)) {
        if (Date.now() >= Deadline) return false;
        await new Promise(Resolveˉwait => setTimeout(Resolveˉwait, 25));
    }
    return true;
}

async function Runˉterminationˉprobe() {
    const Descendantˉsource = 'setInterval(()=>{},1000)';
    const Source =
        "const{spawn}=require('node:child_process');" +
        `const c=spawn(process.execPath,['-e',${JSON.stringify(
            Descendantˉsource
        )}],{stdio:'ignore'});` +
        'process.stdout.write(String(c.pid));setInterval(()=>{},1000)';
    const Result = await Runˉcommand(
        process.execPath,
        ['-e', Source],
        500
    );
    if (!Result.Timedˉout || Result.Exceeded || Result.Forced ||
        Result.Cleanupˉfailure !== null) {
        Reject(
            `The termination probe returned an invalid result: ` +
            `timed-out=${Result.Timedˉout} exceeded=${Result.Exceeded} ` +
            `forced=${Result.Forced} cleanup=${Result.Cleanupˉfailure}.`
        );
    }
    const Descendantˉtext = Result.Output.toString('utf8');
    if (!/^[1-9][0-9]*$/u.test(Descendantˉtext)) {
        Reject(`The termination probe descendant identity is invalid.`);
    }
    const Descendantˉidentifier = Number.parseInt(Descendantˉtext, 10);
    if (!await Waitˉforˉprocessˉexit(Descendantˉidentifier)) {
        Reject(`The termination probe left its descendant running.`);
    }
    process.stdout.write(
        `source admission coordinator process termination probe status=Passed ` +
        `elapsed-ms=${Result.Elapsed}\n`
    );
}

async function Main() {
    const Probeˉonly = process.argv.length === 3 &&
        process.argv[2] === '--termination-probe';
    if (!Probeˉonly && process.argv.length !== 2) {
        Reject('The source-admission coordinator owner accepts no arguments.');
    }
    await Runˉterminationˉprobe();
    if (Probeˉonly) return;

    if (SELECTORS.length !== 28 ||
        new Set(SELECTORS).size !== SELECTORS.length) {
        Reject('The source-admission selector inventory is invalid.');
    }

    const Temporaryˉroot = resolve(tmpdir());
    const Work = await mkdtemp(join(
        Temporaryˉroot,
        'windvale-source-admission-coordinator-'
    ));
    var Passed = false;
    try {
        const Extension = WINDOWS ? 'cmd' : 'sh';
        const Target = WINDOWS ? 'windows' : 'linux';
        const Executableˉsuffix = WINDOWS ? '.exe' : '.elf';
        const Build = join(SCRIPT_DIRECTORY, `Build-Current-Wvb.${Extension}`);
        const Acquire = join(
            SCRIPT_DIRECTORY,
            `Build-Cached-Segmented-Hosted-Wvb.${Extension}`
        );
        const Project = join(
            REPOSITORY_ROOT,
            'Projects', 'Tests',
            'Windvale-Native-Test-Language-1-Source-Admission-Coordinator.wvproj'
        );
        const First = join(Work, 'Coordinator-A.wvb');
        const Second = join(Work, 'Coordinator-B.wvb');
        const Application = join(Work, `Coordinator${Executableˉsuffix}`);

        process.stdout.write(
            'START language 1 source admission coordinator phase=build item=1/2\n'
        );
        const Firstˉbuild = await Requireˉcommand(
            'first coordinator build',
            Build,
            [Project, First],
            BUILD_TIMEOUT_MILLISECONDS
        );
        process.stdout.write(
            `PASS  language 1 source admission coordinator phase=build item=1/2 ` +
            `elapsed-ms=${Firstˉbuild.Elapsed}\n`
        );
        process.stdout.write(
            'START language 1 source admission coordinator phase=build item=2/2\n'
        );
        const Secondˉbuild = await Requireˉcommand(
            'second coordinator build',
            Build,
            [Project, Second],
            BUILD_TIMEOUT_MILLISECONDS
        );
        const Firstˉidentity = await Fileˉidentity(First, 'first coordinator WVB');
        const Secondˉidentity = await Fileˉidentity(Second, 'second coordinator WVB');
        Requireˉexpectedˉwvb(Firstˉidentity, 'first coordinator WVB');
        Requireˉexpectedˉwvb(Secondˉidentity, 'second coordinator WVB');
        if (!Firstˉidentity.value.equals(Secondˉidentity.value)) {
            Reject('The two coordinator WVB builds are not byte-identical.');
        }
        process.stdout.write(
            `PASS  language 1 source admission coordinator phase=build item=2/2 ` +
            `elapsed-ms=${Secondˉbuild.Elapsed} deterministic=Verified\n`
        );

        process.stdout.write(
            'START language 1 source admission coordinator phase=acquire item=1/1\n'
        );
        const Acquisition = await Requireˉcommand(
            'cached segmented hosted coordinator acquisition',
            Acquire,
            ['7', First, Application],
            ACQUISITION_TIMEOUT_MILLISECONDS,
            true,
            true
        );
        const Acquisitionˉlines = Acquisition.Output.toString('utf8').trim().split(
            /\r?\n/u
        );
        const Acquisitionˉreport = Acquisitionˉlines[Acquisitionˉlines.length - 1];
        const Acquisitionˉmatch = /^segmented hosted WVB cache status=(Created|Hit) key=([0-9a-f]{64}) host=(windows-x64|linux-x64) target=(windows|linux) profile=7$/.exec(
            Acquisitionˉreport
        );
        const Expectedˉhost = `${Target}-x64`;
        if (Acquisitionˉmatch === null || Acquisitionˉmatch[3] !== Expectedˉhost ||
            Acquisitionˉmatch[4] !== Target) {
            Reject(`The coordinator acquisition report differs: ${Acquisitionˉreport}`);
        }
        const Applicationˉidentity = await Fileˉidentity(
            Application,
            'coordinator application'
        );
        process.stdout.write(
            `PASS  language 1 source admission coordinator phase=acquire item=1/1 ` +
            `elapsed-ms=${Acquisition.Elapsed} cache=${Acquisitionˉmatch[1]} ` +
            `application-bytes=${Applicationˉidentity.bytes} ` +
            `application-sha256=${Applicationˉidentity.sha256}\n`
        );

        for (var Index = 0; Index < SELECTORS.length; Index += 1) {
            const Selector = SELECTORS[Index];
            process.stdout.write(
                `START language 1 source admission coordinator phase=execute ` +
                `item=${Index + 1}/28 selector=${Selector}\n`
            );
            const Elapsed = await Runˉcase(
                Application,
                Selector,
                Index + 1
            );
            process.stdout.write(
                `PASS  language 1 source admission coordinator phase=execute ` +
                `item=${Index + 1}/28 selector=${Selector} elapsed-ms=${Elapsed}\n`
            );
        }
        Passed = true;
    } finally {
        await Removeˉwork(Work, Temporaryˉroot);
    }

    if (Passed) {
        process.stdout.write(
            'native language 1 source admission coordinator status=Passed ' +
            'cases=28 result=42 deterministic=Verified ' +
            'four-value-output=Verified empty-on-failure=Verified ' +
            'execution=native-cached-profile-7 isolated-executions=28 ' +
            'wvb-bytes=634819 ' +
            'sha256=7d004263b350097f8bcd82997d4210464ee2dba8937a5c0b76d32b8139f99ac1\n'
        );
    }
}

await Main();
