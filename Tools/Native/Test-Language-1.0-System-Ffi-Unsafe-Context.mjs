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
const SELECTORS = [...'abcdefghijklmnopqrstuvwx'];

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

async function Requireˉbuild(Build, Project, Output, Label) {
    const Result = await Runˉcommand(Build, [Project, Output]);
    if (Result.Exceeded) {
        Reject(`The unsafe-context ${Label} build exceeded the output limit.`);
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The unsafe-context ${Label} build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
}

async function Requireˉpackage(Packager, Module, Application) {
    const Result = await Runˉcommand(
        Packager, ['2', Module, Application, '--development-cache']
    );
    if (Result.Exceeded) {
        Reject('The unsafe-context native package exceeded the output limit.');
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The unsafe-context native package failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
    const Metadata = await stat(Application);
    if (!Metadata.isFile() || Metadata.size === 0) {
        Reject('The unsafe-context native package did not publish an application.');
    }
}

async function Runˉcase(Application, Selector, Index) {
    const Result = await Runˉcommand(Application, [Selector]);
    if (Result.Exceeded) {
        Reject(`Unsafe-context case ${Index} exceeded the output limit.`);
    }
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `Unsafe-context case ${Index} wrote output.\n` +
            Result.Output.toString('utf8') + Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) {
        Reject(`Unsafe-context case ${Index} returned ${Result.Code}.`);
    }
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith('windvale-system-ffi-unsafe-context-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await realpath(await mkdtemp(join(
    Temporaryˉroot, 'windvale-system-ffi-unsafe-context-'
)));
var Passed = false;
var Moduleˉbytes = 0;
var Moduleˉdigest = '';
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Wvb.${Extension}`);
    const Packager = join(
        SCRIPT_DIRECTORY,
        `Package-Segmented-Compiler-Wvb.${Extension}`
    );
    const Project = join(
        REPOSITORY_ROOT,
        'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-System-Ffi-Unsafe-Context.wvproj'
    );
    const First = join(Work, 'Unsafe-Context-A.wvb');
    const Second = join(Work, 'Unsafe-Context-B.wvb');
    const Application = join(
        Work,
        WINDOWS ? 'Unsafe-Context.exe' : 'Unsafe-Context.elf'
    );

    process.stdout.write(
        'START language 1 system FFI unsafe context phase=build item=1/4 copies=2\n'
    );
    await Promise.all([
        Requireˉbuild(Build, Project, First, 'first'),
        Requireˉbuild(Build, Project, Second, 'second')
    ]);

    const Firstˉbytes = await readFile(First);
    const Secondˉbytes = await readFile(Second);
    if (!Firstˉbytes.equals(Secondˉbytes)) {
        Reject('The unsafe-context WVB rebuild was not byte-identical.');
    }
    Moduleˉbytes = Firstˉbytes.length;
    Moduleˉdigest = createHash('sha256').update(Firstˉbytes).digest('hex');

    process.stdout.write(
        'START language 1 system FFI unsafe context phase=package item=2/4 ' +
        'execution=native-development-cache\n'
    );
    await Requireˉpackage(Packager, First, Application);

    process.stdout.write(
        'START language 1 system FFI unsafe context phase=execute item=3/4 cases=1-12\n'
    );
    for (var Index = 0; Index < 12; Index += 1) {
        await Runˉcase(Application, SELECTORS[Index], Index + 1);
    }
    process.stdout.write(
        'START language 1 system FFI unsafe context phase=execute item=4/4 cases=13-24\n'
    );
    for (var Index = 12; Index < SELECTORS.length; Index += 1) {
        await Runˉcase(Application, SELECTORS[Index], Index + 1);
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    process.stdout.write(
        'native language 1 system FFI unsafe context status=Passed cases=24 ' +
        'result=42 deterministic=Verified execution=native-packaged ' +
        'isolated-executions=24 ' +
        `wvb-bytes=${Moduleˉbytes} sha256=${Moduleˉdigest}\n`
    );
}
