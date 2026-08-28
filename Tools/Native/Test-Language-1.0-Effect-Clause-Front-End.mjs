import { createHash } from 'node:crypto';
import { mkdtemp, readFile, realpath, rm, stat } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 64 * 1024;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SELECTORS = [...'abcdefghijklmnopqrst'];
const EXPECTED_RUNNER = WINDOWS
    ? {
        bytes: 5_659_136,
        sha256: '2292555c4dad03d646d7e14d0bf716bd663d95b1d0e224f9f6c11d598b519114'
    }
    : {
        bytes: 5_660_672,
        sha256: 'ccaaa6cbb76c557e65c169ef8bad7ca3396c0a38e3e4b18adf303f94077e83d1'
    };

function Reject(Message) {
    throw new Error(Message);
}

function Runˉcommand(Tool, Argumentsˉvalue) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        if (WINDOWS && [Tool, ...Argumentsˉvalue].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument))) {
            Rejectˉpromise(new Error(
                'A Windows test argument contains shell metacharacters.'
            ));
            return;
        }
        const Isˉcommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
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
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand
        });
        const Output = [];
        const Errorˉoutput = [];
        var Outputˉbytes = 0;
        var Errorˉbytes = 0;
        var Exceeded = false;
        Child.stdout.on('data', Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                Output.push(Chunk);
            } else {
                Exceeded = true;
                Child.kill();
            }
        });
        Child.stderr.on('data', Chunk => {
            Errorˉbytes += Chunk.length;
            if (Errorˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                Errorˉoutput.push(Chunk);
            } else {
                Exceeded = true;
                Child.kill();
            }
        });
        Child.once('error', Rejectˉpromise);
        Child.once('close', Code => Resolveˉresult({
            Code,
            Output: Buffer.concat(Output),
            Error: Buffer.concat(Errorˉoutput),
            Exceeded
        }));
    });
}

async function Requireˉbuild(Build, Project, Output) {
    const Result = await Runˉcommand(Build, [Project, Output]);
    if (Result.Exceeded) {
        Reject('The effect-clause build exceeded the diagnostic-output limit.');
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The effect-clause build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') +
            Result.Output.toString('utf8')
        );
    }
}

async function Verifyˉrunner(Runner) {
    const Metadata = await stat(Runner);
    if (Metadata.size !== EXPECTED_RUNNER.bytes) {
        Reject('The WVB runner size is invalid.');
    }
    const Digest = createHash('sha256')
        .update(await readFile(Runner))
        .digest('hex');
    if (Digest !== EXPECTED_RUNNER.sha256) {
        Reject('The WVB runner digest is invalid.');
    }
}

async function Runˉcase(Runner, Module, Selector, Index) {
    const Result = await Runˉcommand(
        Runner, ['--script', Module, Selector]
    );
    if (Result.Exceeded) {
        Reject(`Effect-clause case ${Index} exceeded the output limit.`);
    }
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `Effect-clause case ${Index} wrote output.\n` +
            Result.Output.toString('utf8') +
            Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) {
        Reject(`Effect-clause case ${Index} returned ${Result.Code}.`);
    }
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith('windvale-effect-clause-front-end-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await mkdtemp(join(
    Temporaryˉroot, 'windvale-effect-clause-front-end-'
));
var Passed = false;
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Wvb.${Extension}`);
    const Project = join(
        REPOSITORY_ROOT,
        'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-Effect-Clause-Front-End.wvproj'
    );
    const Runner = join(
        REPOSITORY_ROOT,
        'Artifacts', 'Native-Wvb-Runner-Candidate',
        WINDOWS ? 'windows-x64-wvrun.exe' : 'linux-x64-wvrun.elf'
    );
    const First = join(Work, 'Effect-Clause-A.wvb');
    const Second = join(Work, 'Effect-Clause-B.wvb');

    process.stdout.write(
        'START language 1 effect clause front end phase=build item=1/4\n'
    );
    await Requireˉbuild(Build, Project, First);
    process.stdout.write(
        'START language 1 effect clause front end phase=rebuild item=2/4\n'
    );
    await Requireˉbuild(Build, Project, Second);

    const Firstˉbytes = await readFile(First);
    const Secondˉbytes = await readFile(Second);
    if (!Firstˉbytes.equals(Secondˉbytes)) {
        Reject('The effect-clause WVB rebuild was not byte-identical.');
    }
    await Verifyˉrunner(Runner);

    process.stdout.write(
        'START language 1 effect clause front end phase=execute item=3/4 cases=1-10\n'
    );
    for (var Index = 0; Index < 10; Index += 1) {
        await Runˉcase(Runner, First, SELECTORS[Index], Index + 1);
    }
    process.stdout.write(
        'START language 1 effect clause front end phase=execute item=4/4 cases=11-20\n'
    );
    for (var Index = 10; Index < SELECTORS.length; Index += 1) {
        await Runˉcase(Runner, First, SELECTORS[Index], Index + 1);
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    process.stdout.write(
        'native language 1 effect clause front end status=Passed cases=20 ' +
        'result=42 deterministic=Verified isolated-executions=20\n'
    );
}
