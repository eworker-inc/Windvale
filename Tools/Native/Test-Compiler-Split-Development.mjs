import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import { createReadStream, realpathSync } from 'node:fs';
import {
    lstat,
    mkdtemp,
    readFile,
    realpath,
    rm,
    writeFile,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, '..', '..');
const HOST = `${process.platform}-${process.arch}`;
const TEMPORARY_PREFIX = 'windvale-compiler-split-development-';
const MAXIMUM_OUTPUT_BYTES = 4_194_304;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const PRODUCER_TIMEOUT_MILLISECONDS = 300_000;
const COMPILERS = {
    'win32-x64': {
        path: 'Artifacts/Native-Compiler-Reconstruction-Candidate/windows-x64/wvcompiler.exe',
        bytes: 28_172_800,
        sha256: 'a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d',
    },
    'linux-x64': {
        path: 'Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvcompiler.elf',
        bytes: 28_172_288,
        sha256: 'da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b',
    },
};
const EMITTER_SOURCE = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Windvale.Build',
    'Compiler-Emission-Driver.wv',
);
const EMITTER_PROJECT =
    'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj';
const PRUNING_SOURCE = path.join(
    REPOSITORY_ROOT,
    'Tests',
    'Fixtures',
    'Source-Wvb',
    'Pruning.wv',
);
const OPTIMIZED_WVB = {
    bytes: 308,
    sha256: 'd2f8b67a3a83f393fba16d4f1294000d631e401abd0c4fdde521c9654407b02a',
};
const COMPLETE_WVB = {
    bytes: 395,
    sha256: '42810451eb302f79d0c167eda3fe62b681277661b277a06badcffd177aba5f35',
};

// Reuse this owner for supplied-product diagnostics. The snapshot bundle is
// produced by normal admission/analysis; this selection never builds a compiler.
if (process.argv[2] === '--emission-diagnostics') {
    if (process.argv.length === 7) {
        await Verifyˉemissionˉsources(...process.argv.slice(3));
    } else if (process.argv.length === 5) {
        await Verifyˉemissionˉdiagnostics(process.argv[3], process.argv[4]);
    } else {
        Reject('Usage: --emission-diagnostics <admitter> <authenticator> <analyzer> <emitter> ' +
            'or --emission-diagnostics <emitter> <snapshot-directory>');
    }
    process.exit(0);
}

if (!(HOST in COMPILERS)) {
    Reject(`The compiler split development test does not support ${HOST}.`);
}
const Compiler = path.join(REPOSITORY_ROOT, COMPILERS[HOST].path);
const Compilerˉevidence = await Fileˉevidence(
    Compiler,
    134_217_728,
    'native source compiler',
);
if (Compilerˉevidence.bytes !== COMPILERS[HOST].bytes ||
    Compilerˉevidence.sha256 !== COMPILERS[HOST].sha256) {
    Reject('The native source compiler identity is invalid.');
}

const Temporaryˉroot = realpathSync.native(os.tmpdir());
const Allocatedˉtestˉroot = await mkdtemp(
    path.join(Temporaryˉroot, TEMPORARY_PREFIX),
);
let Testˉroot;
try {
    Testˉroot = realpathSync.native(Allocatedˉtestˉroot);
} catch (Error) {
    await rm(Allocatedˉtestˉroot, { recursive: true, force: true });
    throw Error;
}
try {
    console.log(`compiler split development status=Started cases=4 host=${HOST}`);
    console.log(
        'compiler split development step=adapter-contract item=1/4',
    );
    await Verifyˉadapterˉcontract();
    console.log(
        'compiler split development step=adapter-contract status=Passed ' +
        'target=portable-wvb-optimized-v1',
    );

    console.log(
        'compiler split development step=optimized-oracle item=2/4',
    );
    const Optimizedˉoutput = path.join(Testˉroot, 'Optimized.wvb');
    await Runˉbounded(
        Compiler,
        [PRUNING_SOURCE, Optimizedˉoutput],
        'optimized-oracle',
    );
    const Optimizedˉevidence = await Fileˉevidence(
        Optimizedˉoutput,
        MAXIMUM_OUTPUT_BYTES,
        'optimized pruning WVB',
    );
    Requireˉevidence(
        Optimizedˉevidence,
        OPTIMIZED_WVB,
        'optimized pruning WVB',
    );
    console.log(
        'compiler split development step=optimized-oracle status=Passed ' +
        `wvb-bytes=${Optimizedˉevidence.bytes}`,
    );

    console.log(
        'compiler split development step=complete-oracle item=3/4',
    );
    const Completeˉoutput = path.join(Testˉroot, 'Complete.wvb');
    await Runˉbounded(
        Compiler,
        ['--complete', PRUNING_SOURCE, Completeˉoutput],
        'complete-oracle',
    );
    const Completeˉevidence = await Fileˉevidence(
        Completeˉoutput,
        MAXIMUM_OUTPUT_BYTES,
        'complete pruning WVB',
    );
    Requireˉevidence(
        Completeˉevidence,
        COMPLETE_WVB,
        'complete pruning WVB',
    );
    if (Optimizedˉevidence.bytes >= Completeˉevidence.bytes) {
        Reject('The optimized pruning oracle did not remove unreachable bytes.');
    }
    console.log(
        'compiler split development step=complete-oracle status=Passed ' +
        `wvb-bytes=${Completeˉevidence.bytes}`,
    );

    console.log('compiler split development step=cache-cleanup item=4/4');
    await Runˉbounded(
        process.execPath,
        [path.join(SCRIPT_DIRECTORY, 'Test-Cached-Split-Project-Wvb.mjs')],
        'cache-cleanup',
    );
    console.log(
        'compiler split development status=Passed cases=4 ' +
        'target=portable-wvb-optimized-v1 optimized-wvb-bytes=308 ' +
        'complete-wvb-bytes=395 cleanup=Verified',
    );
} finally {
    const Resolved = path.resolve(Testˉroot);
    if (!Sameˉpath(path.dirname(Resolved), Temporaryˉroot) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected compiler split test directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}

async function Verifyˉadapterˉcontract() {
    const Source = (await readFile(EMITTER_SOURCE, 'utf8')).replace(/\r\n/gu, '\n');
    const Call = 'Emission.Compilerˉemitˉsourceˉanalysis(';
    if (Source.split(Call).length !== 2 ||
        !Source.includes('\n            true\n        );') ||
        Source.includes('\n            false\n        );') ||
        !Source.includes('status=Published mode=optimized functions=')) {
        Reject('The split emitter is not fixed to optimized target emission.');
    }
    await Readˉproject(EMITTER_PROJECT);
}

async function Verifyˉemissionˉsources(Admitter, Authenticator, Analyzer, Emitter) {
    const { Runˉdevelopmentˉcommand } = await import('./Development-Command-Core.mjs');
    const Deadline = Date.now() + 180_000;
    for (const Tool of [Admitter, Authenticator, Analyzer, Emitter]) {
        await Fileˉevidence(path.resolve(Tool), 134_217_728, 'prepared diagnostic tool');
    }
    const Temporaryˉroot = realpathSync.native(os.tmpdir());
    const Work = await mkdtemp(path.join(Temporaryˉroot, TEMPORARY_PREFIX));
    const Profile = path.join(REPOSITORY_ROOT, 'Documents', 'Project',
        'Language-1.0-Localization-Workloads', '01-Source-Profile-Admission', 'Reference-Artifacts');
    const Fixtures = path.join(REPOSITORY_ROOT, 'Tests', 'Fixtures', 'Language-1.0');
    const Run = async (Tool, Arguments) => {
        const Result = await Runˉdevelopmentˉcommand(
            Tool, Arguments, Deadline, false, MAXIMUM_DIAGNOSTIC_BYTES,
        );
        if (Result.Code !== 0 || Result.Error !== '') {
            Reject(`Diagnostic fixture preparation failed: ${Result.Output}${Result.Error}`);
        }
    };
    try {
        for (const [Prefix, Sources] of [
            ['Positive', [path.join(Fixtures, 'Minimum-Program.wv')]],
            ['Negative', [path.join(Fixtures, 'Emission-Ownership-Diagnostic.wv'),
                path.join(REPOSITORY_ROOT, 'Libraries', 'Foundation', 'Values', 'Option.wv')]],
        ]) {
            console.log(`compiler split diagnostics step=prepare case=${Prefix}`);
            const Admitted = path.join(Work, `${Prefix}-Admitted.wvss`);
            await Run(process.execPath, [path.join(SCRIPT_DIRECTORY, 'Run-Authenticated-Source-Admission.mjs'),
                path.resolve(Admitter), path.resolve(Authenticator),
                '--source-input-lock', path.join(Profile, 'Source-Inputs.wvlock'),
                '9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e',
                '--source-profile', path.join(Profile, 'En-Source-Profile.wvsp'),
                '--target-descriptor', path.join(REPOSITORY_ROOT, 'Projects', 'Targets',
                    `${process.platform === 'win32' ? 'Windows' : 'Linux'}-X64-No-Foreign.wvtd`),
                ...Sources, Admitted]);
            const Analyzed = path.join(Work, `${Prefix}-Source.wvss`);
            await Run(path.resolve(Analyzer), ['--internal-source-set', Admitted, Analyzed,
                ...['Analysis.wvam', 'Bindings.wvlb', 'Wir.wvir'].map(Name => path.join(Work, `${Prefix}-${Name}`))]);
            if (!(await readFile(Admitted)).equals(await readFile(Analyzed))) {
                Reject('Diagnostic analysis changed the admitted source set.');
            }
        }
        await Verifyˉemissionˉdiagnostics(Emitter, Work, Deadline, {
            bytes: 221,
            sha256: '25a18cf13d791db1e85fd6b237f89f21d4a0c7b9460b0a72db2da5e5deb205ae',
        }, (await readFile(path.join(Fixtures, 'Emission-Ownership-Diagnostic.wv'), 'utf8'))
            .split('fn Stepˉfailure(')[0].split('\n').length);
    } finally {
        if (!Sameˉpath(path.dirname(path.resolve(Work)), Temporaryˉroot) ||
            !path.basename(Work).startsWith(TEMPORARY_PREFIX)) {
            Reject('Refusing to remove an unexpected diagnostic fixture directory.');
        }
        await rm(Work, { recursive: true, force: true });
    }
}

async function Verifyˉemissionˉdiagnostics(Emitterˉpath, Snapshotˉpath,
    Deadline = Date.now() + 180_000, Baselineˉevidence = null, Declarationˉline = null) {
    const { Runˉdevelopmentˉcommand } = await import('./Development-Command-Core.mjs');
    const Emitter = path.resolve(Emitterˉpath);
    const Snapshots = path.resolve(Snapshotˉpath);
    await Fileˉevidence(Emitter, 134_217_728, 'diagnostic emitter');
    const Names = ['Source.wvss', 'Analysis.wvam', 'Bindings.wvlb', 'Wir.wvir'];
    const Inputs = {};
    for (const Prefix of ['Positive', 'Negative']) {
        Inputs[Prefix] = [];
        for (const Name of Names) {
            const File = path.join(Snapshots, `${Prefix}-${Name}`);
            await Fileˉevidence(File, MAXIMUM_OUTPUT_BYTES, `${Prefix} ${Name}`);
            Inputs[Prefix].push(File);
        }
    }
    const Baseline = path.join(Snapshots, 'Positive-Baseline.wvb');
    if (Baselineˉevidence === null) {
        await Fileˉevidence(Baseline, MAXIMUM_OUTPUT_BYTES, 'unchanged emission baseline');
    }
    const Temporaryˉroot = realpathSync.native(os.tmpdir());
    const Work = await mkdtemp(path.join(Temporaryˉroot, TEMPORARY_PREFIX));
    let Cases = 0;
    try {
        const Positive = path.join(Work, 'Positive.wvb');
        const Success = await Runˉdevelopmentˉcommand(
            Emitter, [...Inputs.Positive, Positive], Deadline, false,
            MAXIMUM_DIAGNOSTIC_BYTES,
        );
        if (Success.Code !== 0 || Success.Error !== '' ||
            !Success.Output.startsWith('source emission status=Published ')) {
            Reject('Diagnostic changes altered successful emission or its output bytes.');
        }
        if (Baselineˉevidence === null) {
            if (!(await readFile(Positive)).equals(await readFile(Baseline))) {
                Reject('Diagnostic changes altered successful output bytes.');
            }
        } else {
            Requireˉevidence(await Fileˉevidence(Positive, MAXIMUM_OUTPUT_BYTES,
                'successful emission'), Baselineˉevidence, 'successful emission');
        }
        Cases += 1;
        console.log('compiler split diagnostics item=1/7 case=unchanged-success status=Passed');
        const Sentinel = Buffer.from('preserve rejected output\n');
        const Runˉrejection = async (Label, Arguments, Detailed, Providerˉlimit = false) => {
            const Output = path.join(Work, `${Label}.wvb`);
            await writeFile(Output, Sentinel, { flag: 'wx' });
            const Result = await Runˉdevelopmentˉcommand(
                Emitter, [...Arguments, Output], Deadline, false,
                MAXIMUM_DIAGNOSTIC_BYTES,
            );
            const Diagnostic = Result.Error.replace(/\r\n/gu, '\n');
            if (Result.Code !== (Providerˉlimit ? 73 : 1) || Result.Output !== '' ||
                !(await readFile(Output)).equals(Sentinel)) {
                Reject(`The ${Label} rejection did not preserve its failure/output contract: ` +
                    `code=${Result.Code} stdout=${JSON.stringify(Result.Output)} ` +
                    `stderr=${JSON.stringify(Result.Error)}.`);
            }
            if (Providerˉlimit) {
                // The hosted file provider rejects over 4 MiB before the
                // emitter receives bytes; no source context is available.
                if (Diagnostic !== '') Reject('Oversized input reached emission diagnostics.');
            } else if (Detailed) {
                if (!Diagnostic.includes('wvb-status=Unsupportedˉshape ') ||
                    !Diagnostic.includes('diagnostic-scope=function ') ||
                    !/function-name="[^"\n]*Stepˉfailure"/u.test(Diagnostic) ||
                    !/declaration-line=[1-9][0-9]* /u.test(Diagnostic) ||
                    !Diagnostic.includes('operation-kind=17 ') ||
                    !/type-name="[^"\n]*Lockˉstep"/u.test(Diagnostic) ||
                    !Diagnostic.endsWith('rule=operation-result-ownership-unknown\n')) {
                    Reject(`The ownership failure lacks actionable context: ${Diagnostic}`);
                }
                if (Declarationˉline !== null &&
                    (!Diagnostic.includes('module-index=0 ') ||
                     !Diagnostic.includes(`declaration-line=${Declarationˉline} `))) {
                    Reject(`The failure identifies the wrong source declaration: ${Diagnostic}`);
                }
                process.stdout.write(Diagnostic);
            } else if (!Diagnostic.startsWith('source emission status=Invalidˉanalysis ') ||
                Diagnostic.includes('function-name=')) {
                Reject(`The ${Label} failure exposed context from unvalidated evidence.`);
            }
            Cases += 1;
            console.log(`compiler split diagnostics item=${Cases}/7 case=${Label} status=Passed`);
        };
        await Runˉrejection('ownership', Inputs.Negative, true);
        for (let Index = 0; Index < Names.length; Index += 1) {
            const Arguments = [...Inputs.Negative];
            const Truncated = path.join(Work, `Truncated-${Names[Index]}`);
            await writeFile(Truncated, (await readFile(Arguments[Index])).subarray(0, 3));
            Arguments[Index] = Truncated;
            await Runˉrejection(`truncated-${Index}`, Arguments, false);
        }
        const Oversized = path.join(Work, 'Oversized.wvir');
        await writeFile(Oversized, Buffer.alloc(MAXIMUM_OUTPUT_BYTES + 1));
        await Runˉrejection('oversized', [...Inputs.Negative.slice(0, 3), Oversized], false, true);
        console.log(`compiler split diagnostics status=Passed cases=${Cases} successful-bytes=Unchanged`);
    } finally {
        const Resolved = path.resolve(Work);
        if (!Sameˉpath(path.dirname(Resolved), Temporaryˉroot) ||
            !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
            Reject('Refusing to remove an unexpected diagnostic test directory.');
        }
        await rm(Resolved, { recursive: true, force: true });
    }
}

function Requireˉevidence(Actual, Expected, Label) {
    if (Actual.bytes !== Expected.bytes || Actual.sha256 !== Expected.sha256) {
        Reject(`The ${Label} identity differs.`);
    }
}

async function Readˉproject(Relative) {
    const Candidate = path.join(REPOSITORY_ROOT, Relative);
    const Bytes = await readFile(Candidate);
    if (Bytes.length < 1 || Bytes.length > 16_384) {
        Reject(`The focused project is not bounded: ${Relative}`);
    }
    const Lines = Bytes.toString('utf8').split(/\r?\n/u).filter(Line => Line !== '');
    if (Lines[0] === 'windvale-project 4') {
        const Admission = Lines.splice(-4);
        if (!/^source-input-lock "[^"\r\n]+\.wvlock"$/u.test(Admission[0] ?? '') ||
            !/^source-input-lock-sha256 [0-9a-f]{64}$/u.test(Admission[1] ?? '') ||
            !/^source-profile "[^"\r\n]+\.wvsp"$/u.test(Admission[2] ?? '') ||
            !/^target-descriptor "[^"\r\n]+\.wvtd"$/u.test(Admission[3] ?? '')) {
            Reject(`The focused project admission declarations are invalid: ${Relative}`);
        }
    }
    if (!['windvale-project 2', 'windvale-project 4'].includes(Lines[0]) ||
        Lines[Lines.length - 1] !== 'emit wvb') {
        Reject(`The focused project contract is invalid: ${Relative}`);
    }
    const Inputs = [];
    let Roots = 0;
    for (const Line of Lines.slice(1, -1)) {
        const Match = /^(root|source) "([^"\r\n]+)"$/u.exec(Line);
        if (Match === null || Match[2].includes('\\') ||
            path.posix.isAbsolute(Match[2]) ||
            Match[2].split('/').some(Part =>
                Part === '' || Part === '.' || Part === '..')) {
            Reject(`The focused project source is invalid: ${Relative}`);
        }
        if (Match[1] === 'root') {
            Roots += 1;
            if (Inputs.length !== 0) {
                Reject(`The focused project root is not first: ${Relative}`);
            }
        }
        const Input = path.join(REPOSITORY_ROOT, ...Match[2].split('/'));
        const Canonical = await realpath(Input).catch(() => '');
        if (!Sameˉpath(Canonical, Input)) {
            Reject(`The focused project source is not canonical: ${Match[2]}`);
        }
        Inputs.push(Input);
    }
    if (Roots !== 1 || Inputs.length < 1 || Inputs.length > 64) {
        Reject(`The focused project source count is invalid: ${Relative}`);
    }
    // The project sequence is the semantic WVSS sequence. Filename sorting is
    // not a valid proxy for declared module-identity order (`*-Main.wv` is a
    // common counterexample); the source analyzer validates that order.
    return Inputs;
}

async function Runˉbounded(Command, Arguments, Step) {
    await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: REPOSITORY_ROOT,
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Started = Date.now();
        let Output = Buffer.alloc(0);
        let Errorˉoutput = Buffer.alloc(0);
        let Settled = false;
        const Finish = (Error) => {
            if (Settled) {
                return;
            }
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            if (Error === null) {
                Resolve();
            } else {
                Rejectˉpromise(Error);
            }
        };
        const Append = (Current, Chunk) => {
            if (Current.length + Chunk.length > MAXIMUM_DIAGNOSTIC_BYTES) {
                Child.kill();
                Finish(new Error(`The ${Step} diagnostics exceed 64 KiB.`));
                return Current;
            }
            return Buffer.concat([Current, Chunk]);
        };
        Child.stdout.on('data', Chunk => { Output = Append(Output, Chunk); });
        Child.stderr.on('data', Chunk => {
            Errorˉoutput = Append(Errorˉoutput, Chunk);
        });
        const Progress = setInterval(() => {
            const Seconds = Math.floor((Date.now() - Started) / 1_000);
            console.log(
                `compiler split development step=${Step} status=Active ` +
                `elapsed-seconds=${Seconds}`,
            );
        }, 30_000);
        const Timeout = setTimeout(() => {
            Child.kill();
            Finish(new Error(`The ${Step} producer exceeded five minutes.`));
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        Child.on('error', Error => {
            Finish(Error);
        });
        Child.on('close', Status => {
            if (Settled) {
                return;
            }
            if (Status !== 0) {
                const Standardˉoutput = Output.toString('utf8').trim();
                const Standardˉerror = Errorˉoutput.toString('utf8').trim();
                Finish(new Error(
                    `The ${Step} producer exited ${Status}: ` +
                    `stdout=${JSON.stringify(Standardˉoutput)} ` +
                    `stderr=${JSON.stringify(Standardˉerror)}`,
                ));
                return;
            }
            if (Errorˉoutput.length !== 0) {
                Finish(new Error(
                    `The ${Step} producer wrote diagnostics after success.`,
                ));
                return;
            }
            if (Output.length === 0) {
                Finish(new Error(`The ${Step} producer was silent.`));
                return;
            }
            Finish(null);
        });
    });
}

async function Fileˉevidence(Candidate, Maximum, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} must use its canonical non-link path: ${Candidate}`);
    }
    const Hash = createHash('sha256');
    let Measured = 0;
    for await (const Chunk of createReadStream(Candidate, {
        highWaterMark: 1_048_576,
    })) {
        Measured += Chunk.length;
        if (Measured > Information.size) {
            Reject(`The ${Label} grew while it was hashed.`);
        }
        Hash.update(Chunk);
    }
    if (Measured !== Information.size) {
        Reject(`The ${Label} changed while it was hashed.`);
    }
    return { bytes: Measured, sha256: Hash.digest('hex') };
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}
