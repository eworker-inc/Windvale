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
const SELECTORS = [...'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN'];
const EXPECTED_RUNNER = WINDOWS
    ? {
        bytes: 10_368_512,
        sha256: 'd5743801003ac0c43ce6b5b2b3c4bb195d8334f84f5a7f84c6e1edd04b8cf7a7'
    }
    : {
        bytes: 10_371_072,
        sha256: 'e63bce623c470418ed3bede36ce2c4c3964c245c78766e45bb71090b637e3d0b'
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
            if (Outputˉbytes <= MAXIMUM_OUTPUT_BYTES) Output.push(Chunk);
            else { Exceeded = true; Child.kill(); }
        });
        Child.stderr.on('data', Chunk => {
            Errorˉbytes += Chunk.length;
            if (Errorˉbytes <= MAXIMUM_OUTPUT_BYTES) Errorˉoutput.push(Chunk);
            else { Exceeded = true; Child.kill(); }
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
    if (Result.Exceeded) Reject('The WVFC build exceeded the output limit.');
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The WVFC build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
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
    const Result = await Runˉcommand(Runner, ['--script', Module, Selector]);
    if (Result.Exceeded) Reject(`WVFC case ${Index} exceeded the output limit.`);
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `WVFC case ${Index} wrote output.\n` +
            Result.Output.toString('utf8') + Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) Reject(`WVFC case ${Index} returned ${Result.Code}.`);
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith('windvale-foreign-catalog-format-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

const Maximumˉcount = 43_690n;
const Maximumˉlength = 48n + 96n * Maximumˉcount;
const Firstˉrejectedˉlength = 48n + 96n * (Maximumˉcount + 1n);
if (Maximumˉlength !== 4_194_288n ||
    Maximumˉlength > 4_194_304n ||
    Firstˉrejectedˉlength <= 4_194_304n) {
    Reject('The independent WVFC count/length boundary is invalid.');
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await mkdtemp(join(
    Temporaryˉroot, 'windvale-foreign-catalog-format-'
));
var Passed = false;
var Productˉbytes = 0;
var Productˉsha256 = '';
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Wvb.${Extension}`);
    const Project = join(
        REPOSITORY_ROOT,
        'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-Foreign-Catalog-Format.wvproj'
    );
    const Runner = join(
        REPOSITORY_ROOT,
        'Artifacts', 'Native-Wvb-Runner-Candidate',
        WINDOWS ? 'windows-x64-wvrun.exe' : 'linux-x64-wvrun.elf'
    );
    const First = join(Work, 'Foreign-Catalog-A.wvb');
    const Second = join(Work, 'Foreign-Catalog-B.wvb');

    process.stdout.write(
        'START language 1 foreign catalog phase=build item=1/4\n'
    );
    await Requireˉbuild(Build, Project, First);
    process.stdout.write(
        'START language 1 foreign catalog phase=rebuild item=2/4\n'
    );
    await Requireˉbuild(Build, Project, Second);

    const Firstˉbytes = await readFile(First);
    const Secondˉbytes = await readFile(Second);
    if (!Firstˉbytes.equals(Secondˉbytes)) {
        Reject('The WVFC WVB rebuild was not byte-identical.');
    }
    Productˉbytes = Firstˉbytes.length;
    Productˉsha256 = createHash('sha256').update(Firstˉbytes).digest('hex');
    await Verifyˉrunner(Runner);

    process.stdout.write(
        'START language 1 foreign catalog phase=execute item=3/4 cases=1-20\n'
    );
    for (var Index = 0; Index < 20; Index += 1) {
        await Runˉcase(Runner, First, SELECTORS[Index], Index + 1);
    }
    process.stdout.write(
        'START language 1 foreign catalog phase=execute item=4/4 cases=21-40\n'
    );
    for (var Index = 20; Index < SELECTORS.length; Index += 1) {
        await Runˉcase(Runner, First, SELECTORS[Index], Index + 1);
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    process.stdout.write(
        'native language 1 foreign catalog status=Passed cases=40 ' +
        'result=42 deterministic=Verified isolated-executions=40 ' +
        'boundary-count=43690 ' +
        `wvb-bytes=${Productˉbytes} sha256=${Productˉsha256}\n`
    );
}
