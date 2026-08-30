import { createHash } from 'node:crypto';
import { mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 64 * 1024;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SELECTORS = [...'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRST'];
function Reject(Message) {
    throw new Error(Message);
}

function Runˉcommand(Tool, Argumentsˉvalue, Mirrorˉoutput = false) {
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
                if (Mirrorˉoutput) process.stdout.write(Chunk);
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
        Reject('The system-ffi build exceeded the diagnostic-output limit.');
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The system-ffi build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') +
            Result.Output.toString('utf8')
        );
    }
}

async function Requireˉpackage(Package, Module, Application) {
    const Result = await Runˉcommand(
        Package, ['7', Module, Application, '--development-cache'], true
    );
    if (Result.Exceeded) {
        Reject('The system-ffi package exceeded the diagnostic-output limit.');
    }
    if (Result.Code !== 0 || Result.Error.length !== 0 ||
        !Result.Output.toString('utf8').includes(
            'segmented compiler package status=Complete '
        )) {
        Reject(
            `The system-ffi package failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8')
        );
    }
}

async function Runˉcase(Application, Selector, Index) {
    const Result = await Runˉcommand(Application, [Selector]);
    if (Result.Exceeded) {
        Reject(`System-FFI case ${Index} exceeded the output limit.`);
    }
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `System-FFI case ${Index} wrote output.\n` +
            Result.Output.toString('utf8') +
            Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) {
        Reject(`System-FFI case ${Index} returned ${Result.Code}.`);
    }
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith('windvale-system-ffi-front-end-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await mkdtemp(join(
    Temporaryˉroot, 'windvale-system-ffi-front-end-'
));
var Passed = false;
var Productˉbytes = 0;
var Productˉsha256 = '';
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Current-Wvb.${Extension}`);
    const Package = join(
        SCRIPT_DIRECTORY, `Package-Segmented-Compiler-Wvb.${Extension}`
    );
    const Project = join(
        REPOSITORY_ROOT,
        'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-System-Ffi-Front-End.wvproj'
    );
    const First = join(Work, 'System-Ffi-A.wvb');
    const Second = join(Work, 'System-Ffi-B.wvb');
    const Application = join(
        Work, WINDOWS ? 'System-Ffi.exe' : 'System-Ffi.elf'
    );

    process.stdout.write(
        'START language 1 system FFI front end phase=build item=1/5\n'
    );
    await Requireˉbuild(Build, Project, First);
    process.stdout.write(
        'START language 1 system FFI front end phase=rebuild item=2/5\n'
    );
    await Requireˉbuild(Build, Project, Second);

    const Firstˉbytes = await readFile(First);
    const Secondˉbytes = await readFile(Second);
    if (!Firstˉbytes.equals(Secondˉbytes)) {
        Reject('The system-ffi WVB rebuild was not byte-identical.');
    }
    Productˉbytes = Firstˉbytes.length;
    Productˉsha256 = createHash('sha256').update(Firstˉbytes).digest('hex');
    process.stdout.write(
        'START language 1 system FFI front end phase=package item=3/5\n'
    );
    await Requireˉpackage(Package, First, Application);

    process.stdout.write(
        'START language 1 system FFI front end phase=execute item=4/5 cases=1-23\n'
    );
    for (var Index = 0; Index < 23; Index += 1) {
        await Runˉcase(Application, SELECTORS[Index], Index + 1);
    }
    process.stdout.write(
        'START language 1 system FFI front end phase=execute item=5/5 cases=24-46\n'
    );
    for (var Index = 23; Index < SELECTORS.length; Index += 1) {
        await Runˉcase(Application, SELECTORS[Index], Index + 1);
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    process.stdout.write(
        'native language 1 system FFI front end status=Passed cases=46 ' +
        'result=42 deterministic=Verified execution=native-package ' +
        'isolated-executions=46 ' +
        `wvb-bytes=${Productˉbytes} sha256=${Productˉsha256}\n`
    );
}
