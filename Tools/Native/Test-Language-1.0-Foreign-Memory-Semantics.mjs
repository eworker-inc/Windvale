import { createHash } from 'node:crypto';
import { lstat, mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 64 * 1024;
const COMMAND_TIMEOUT_MS = 120_000;
const CACHE_TIMEOUT_MS = 20 * 60_000;
const MAXIMUM_CANDIDATE_BYTES = 4 * 1024 * 1024;
const MAXIMUM_APPLICATION_BYTES = 64 * 1024 * 1024;
const TASKKILL_TIMEOUT_MS = 2_000;
const TERMINATION_SETTLE_MS = 5_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SEMANTIC_SELECTORS = [...'abcdeghijklmnopqrstuvwxyzABCD'];
const EXPECTED_PRODUCT = {
    bytes: 147_912,
    sha256: '1ba359cc2372a43ba941d4b2baadd926774b706bfd5e1eeaca8a287329772003'
};

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
            'taskkill.exe', ['/pid', String(Processˉidentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true }
        );
        var Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            Resolveˉresult('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MS);
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
    } else {
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
                Diagnostic += `; direct child kill error: ${Errorˉvalue.message}`;
            }
        }
    }
    return Diagnostic;
}

function Runˉcommand(
    Tool,
    Argumentsˉvalue,
    Timeoutˉmilliseconds = COMMAND_TIMEOUT_MS,
    Relayˉstdout = false
) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Isˉcommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        if (Isˉcommand && [Tool, ...Argumentsˉvalue].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument))) {
            Rejectˉpromise(new Error(
                'A Windows test argument contains shell metacharacters.'
            ));
            return;
        }
        const Executable = Isˉcommand
            ? process.env.ComSpec ?? 'cmd.exe'
            : Tool;
        const Producerˉarguments = Isˉcommand
            ? [
                '/d', '/v:off', '/s', '/c',
                `"${[Tool, ...Argumentsˉvalue]
                    .map(Argument => `"${Argument}"`)
                    .join(' ')}"`
            ]
            : Argumentsˉvalue;
        const Child = spawn(Executable, Producerˉarguments, {
            cwd: REPOSITORY_ROOT,
            detached: !WINDOWS,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand
        });
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
                Output: Buffer.concat(Output),
                Error: Buffer.concat(Errorˉoutput),
                Exceeded,
                Timedˉout,
                Forced,
                Cleanupˉfailure
            };
        }
        function Complete(Code, Forced) {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
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
                    `process tree did not close within ${TERMINATION_SETTLE_MS} ms`;
                Cleanupˉfailure = Terminationˉdiagnostic === null
                    ? Settleˉdiagnostic
                    : `${Terminationˉdiagnostic}; ${Settleˉdiagnostic}`;
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                Complete(null, true);
            }, TERMINATION_SETTLE_MS);
            Terminationˉpromise = (async () => {
                try {
                    Terminationˉdiagnostic =
                        await Terminateˉprocessˉtree(Child);
                } catch (Errorˉvalue) {
                    Terminationˉdiagnostic =
                        `tree termination error: ${Errorˉvalue.message}`;
                    try { Child.kill('SIGKILL'); } catch {
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
        }, Timeoutˉmilliseconds);
        Child.stdout.on('data', Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                Output.push(Chunk);
                if (Relayˉstdout) process.stdout.write(Chunk);
            }
            else if (!Exceeded) { Exceeded = true; Terminateˉandˉsettle(); }
        });
        Child.stderr.on('data', Chunk => {
            Errorˉbytes += Chunk.length;
            if (Errorˉbytes <= MAXIMUM_OUTPUT_BYTES) Errorˉoutput.push(Chunk);
            else if (!Exceeded) { Exceeded = true; Terminateˉandˉsettle(); }
        });
        Child.once('error', Error => {
            if (Timedˉout || Exceeded || Settled) return;
            Settled = true;
            clearTimeout(Timer);
            if (Settleˉtimer !== undefined) clearTimeout(Settleˉtimer);
            Rejectˉpromise(Error);
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

async function Requireˉbuild(Build, Project, Output, Label) {
    const Result = await Runˉcommand(Build, [Project, Output]);
    if (Result.Cleanupˉfailure !== null) {
        Reject(`${Label} build cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject(`${Label} build timed out.`);
    if (Result.Exceeded) Reject(`${Label} build exceeded the output limit.`);
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `${Label} build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
}

async function Requireˉprofileˉrejection(Build, Project, Output) {
    const Result = await Runˉcommand(Build, [Project, Output]);
    if (Result.Cleanupˉfailure !== null) {
        Reject(`The profile rejection cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject('The profile rejection timed out.');
    if (Result.Exceeded) Reject('The profile rejection exceeded the output limit.');
    if (Result.Code !== 1 || Result.Output.length !== 0) {
        Reject(
            `The profile regression returned ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
    const Report = Result.Error.toString('utf8').trim();
    const Expected =
        'build status=Compileˉrejected source-status=Sourceˉwir ' +
        'wir-status=Sourceˉbindings function=0 operation=0 ' +
        'binding-status=Sourceˉsymbols symbol-status=Sourceˉgraph ' +
        'graph-status=Dependencyˉprofile source-set-status=Valid ' +
        'parse-status=Valid body-status=Valid declaration=End module=0 ' +
        'related-module=1 line=3 column=8';
    if (Report !== Expected) {
        Reject(`The profile rejection diagnostic differs.\n${Report}`);
    }
    try {
        await lstat(Output);
        Reject('The rejected profile build published an output.');
    } catch (Error) {
        if (Error?.code !== 'ENOENT') throw Error;
    }
}

async function Acquireˉapplication(Candidate, Work) {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Helper = join(
        SCRIPT_DIRECTORY, `Build-Cached-Segmented-Hosted-Wvb.${Extension}`
    );
    const Application = join(
        Work, WINDOWS ? 'Foreign-Memory.exe' : 'Foreign-Memory.elf'
    );
    const Result = await Runˉcommand(
        Helper, ['7', Candidate, Application], CACHE_TIMEOUT_MS, true
    );
    if (Result.Cleanupˉfailure !== null) {
        Reject(`Segmented hosted acquisition cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject('Segmented hosted acquisition timed out.');
    if (Result.Exceeded) Reject('Segmented hosted acquisition exceeded output bounds.');
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `Segmented hosted acquisition returned ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
    const Report = Result.Output.toString('utf8').trim().split(/\r?\n/u).at(-1);
    const Host = WINDOWS ? 'windows-x64' : 'linux-x64';
    const Target = WINDOWS ? 'windows' : 'linux';
    const Pattern = new RegExp(
        '^segmented hosted WVB cache status=(?:Created|Hit) key=[0-9a-f]{64} ' +
        `host=${Host} target=${Target} profile=7$`,
        'u'
    );
    if (!Pattern.test(Report ?? '')) {
        Reject(`The segmented hosted acquisition report differs: ${Report ?? ''}`);
    }
    const Information = await lstat(Application);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size === 0 || Information.size > MAXIMUM_APPLICATION_BYTES) {
        Reject('The cached segmented hosted application is invalid.');
    }
    return Application;
}

async function Runˉcase(Application, Selector, Label) {
    const Result = await Runˉcommand(Application, [Selector]);
    if (Result.Cleanupˉfailure !== null) {
        Reject(`${Label} cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject(`${Label} timed out.`);
    if (Result.Exceeded) Reject(`${Label} exceeded the output limit.`);
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `${Label} wrote output.\n` + Result.Output.toString('utf8') +
            Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) Reject(`${Label} returned ${Result.Code}.`);
}

async function Runˉdispatchˉprobe(Application, Argumentsˉvalue, Label) {
    const Result = await Runˉcommand(Application, Argumentsˉvalue);
    if (Result.Cleanupˉfailure !== null) {
        Reject(`${Label} cleanup failed: ${Result.Cleanupˉfailure}.`);
    }
    if (Result.Timedˉout) Reject(`${Label} timed out.`);
    if (Result.Exceeded) Reject(`${Label} exceeded the output limit.`);
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `${Label} wrote output.\n` + Result.Output.toString('utf8') +
            Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 1) {
        Reject(`${Label} returned ${Result.Code}; expected exact rejection 1.`);
    }
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
    const Result = await Runˉcommand(process.execPath, ['-e', Source], 500);
    if (!Result.Timedˉout || Result.Exceeded || Result.Forced ||
        Result.Cleanupˉfailure !== null) {
        Reject(
            'The termination probe returned an invalid result: ' +
            `timed-out=${Result.Timedˉout} exceeded=${Result.Exceeded} ` +
            `forced=${Result.Forced} cleanup=${Result.Cleanupˉfailure}.`
        );
    }
    const Descendantˉtext = Result.Output.toString('utf8');
    if (!/^[1-9][0-9]*$/u.test(Descendantˉtext)) {
        Reject('The termination probe descendant identity is invalid.');
    }
    const Descendantˉidentifier = Number.parseInt(Descendantˉtext, 10);
    if (!await Waitˉforˉprocessˉexit(Descendantˉidentifier)) {
        Reject('The termination probe left its descendant running.');
    }
    process.stdout.write(
        'foreign memory owner process termination probe status=Passed\n'
    );
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith('windvale-foreign-memory-semantics-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

async function Main() {
const Probeˉonly = process.argv.length === 3 &&
    process.argv[2] === '--termination-probe';
if (!Probeˉonly && process.argv.length !== 2) {
    Reject('The foreign-memory owner accepts no arguments.');
}
await Runˉterminationˉprobe();
if (Probeˉonly) return;
if (SEMANTIC_SELECTORS.length !== 29 ||
    new Set(SEMANTIC_SELECTORS).size !== SEMANTIC_SELECTORS.length ||
    SEMANTIC_SELECTORS.includes('f')) {
    Reject('The semantic selector routing identity is invalid.');
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await mkdtemp(join(
    Temporaryˉroot, 'windvale-foreign-memory-semantics-'
));
var Passed = false;
var Productˉbytes = 0;
var Productˉsha256 = '';
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Current-Wvb.${Extension}`);
    const Semanticˉproject = join(
        REPOSITORY_ROOT, 'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-Foreign-Memory-Semantics.wvproj'
    );
    const Profileˉproject = join(
        REPOSITORY_ROOT, 'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-Foreign-Memory-Profile-Regression.wvproj'
    );
    const First = join(Work, 'Foreign-Memory-A.wvb');
    const Second = join(Work, 'Foreign-Memory-B.wvb');
    const Profile = join(Work, 'Foreign-Memory-Profile-Rejected.wvb');

    process.stdout.write('START foreign memory phase=build item=1/5\n');
    await Requireˉbuild(Build, Semanticˉproject, First, 'semantic oracle');
    process.stdout.write('START foreign memory phase=rebuild item=2/5\n');
    await Requireˉbuild(Build, Semanticˉproject, Second, 'semantic oracle');
    process.stdout.write('START foreign memory phase=profile item=3/5\n');
    await Requireˉprofileˉrejection(Build, Profileˉproject, Profile);

    const Firstˉinformation = await lstat(First);
    const Secondˉinformation = await lstat(Second);
    if (!Firstˉinformation.isFile() || Firstˉinformation.isSymbolicLink() ||
        !Secondˉinformation.isFile() || Secondˉinformation.isSymbolicLink() ||
        Firstˉinformation.size < 12 ||
        Firstˉinformation.size > MAXIMUM_CANDIDATE_BYTES ||
        Secondˉinformation.size !== Firstˉinformation.size) {
        Reject('The host-bound semantic WVB size is invalid.');
    }
    const Firstˉbytes = await readFile(First);
    const Secondˉbytes = await readFile(Second);
    if (!Firstˉbytes.equals(Secondˉbytes)) {
        Reject('The foreign-memory WVB rebuild was not byte-identical.');
    }
    Productˉbytes = Firstˉbytes.length;
    Productˉsha256 = createHash('sha256').update(Firstˉbytes).digest('hex');
    if (Productˉbytes !== EXPECTED_PRODUCT.bytes ||
        Productˉsha256 !== EXPECTED_PRODUCT.sha256) {
        Reject(
            `The semantic WVB identity differs: bytes=${Productˉbytes} ` +
            `sha256=${Productˉsha256}.`
        );
    }
    process.stdout.write('START foreign memory phase=acquire item=4/5\n');
    const Application = await Acquireˉapplication(First, Work);

    process.stdout.write(
        'START foreign memory phase=execute item=5/5 ' +
        'rule-level-cases=29 source-graph-rejections=1 dispatch-probes=4\n'
    );
    await Runˉdispatchˉprobe(Application, [], 'missing-selector dispatch probe');
    await Runˉdispatchˉprobe(Application, ['f'], 'omitted-selector dispatch probe');
    await Runˉdispatchˉprobe(Application, ['aa'], 'malformed-selector dispatch probe');
    await Runˉdispatchˉprobe(
        Application, ['a', 'b'], 'multiple-selector dispatch probe'
    );
    for (var Index = 0; Index < SEMANTIC_SELECTORS.length; Index += 1) {
        await Runˉcase(
            Application, SEMANTIC_SELECTORS[Index],
            `foreign-memory rule case ${Index + 1}`
        );
    }
    const Finalˉinformation = await lstat(First);
    if (!Finalˉinformation.isFile() || Finalˉinformation.isSymbolicLink() ||
        Finalˉinformation.size !== Productˉbytes ||
        !(await readFile(First)).equals(Firstˉbytes)) {
        Reject('The verified semantic WVB changed during isolated execution.');
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    process.stdout.write(
        'native language 1 foreign memory semantics status=Passed cases=30 ' +
        'result=42 deterministic=Verified execution=native-cached-profile-7 ' +
        'isolated-executions=29 ' +
        'rule-level-cases=29 source-graph-rejections=1 dispatch-probes=4 ' +
        `wvb-bytes=${Productˉbytes} sha256=${Productˉsha256}\n`
    );
}
}

await Main();
