import { mkdtemp, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 64 * 1024;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');

function Reject(Message) {
    throw new Error(Message);
}

function Runˉcommand(Tool, Argumentsˉvalue, Capture) {
    return new Promise((Resolve, Rejectˉpromise) => {
        if (WINDOWS && [Tool, ...Argumentsˉvalue].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument))) {
            Rejectˉpromise(new Error('A Windows test argument contains shell metacharacters.'));
            return;
        }
        const Executable = WINDOWS
            ? process.env.ComSpec ?? 'cmd.exe'
            : Tool;
        const Producerˉarguments = WINDOWS
            ? [
                '/d', '/v:off', '/s', '/c',
                `"${[Tool, ...Argumentsˉvalue]
                    .map(Argument => `"${Argument}"`)
                    .join(' ')}"`
            ]
            : Argumentsˉvalue;
        const Child = spawn(Executable, Producerˉarguments, {
            cwd: REPOSITORY_ROOT,
            stdio: Capture ? ['ignore', 'pipe', 'pipe'] : 'inherit',
            windowsHide: true,
            windowsVerbatimArguments: WINDOWS
        });
        const Output = [];
        const Errorˉoutput = [];
        var Outputˉbytes = 0;
        var Errorˉbytes = 0;
        if (Capture) {
            Child.stdout.on('data', Chunk => {
                Outputˉbytes += Chunk.length;
                if (Outputˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                    Output.push(Chunk);
                } else {
                    Child.kill();
                }
            });
            Child.stderr.on('data', Chunk => {
                Errorˉbytes += Chunk.length;
                if (Errorˉbytes <= MAXIMUM_OUTPUT_BYTES) {
                    Errorˉoutput.push(Chunk);
                } else {
                    Child.kill();
                }
            });
        }
        Child.once('error', Rejectˉpromise);
        Child.once('close', Code => Resolve({
            Code,
            Output: Buffer.concat(Output),
            Error: Buffer.concat(Errorˉoutput),
            Outputˉbytes,
            Errorˉbytes
        }));
    });
}

async function Requireˉsuccess(Tool, Argumentsˉvalue) {
    const Result = await Runˉcommand(Tool, Argumentsˉvalue, false);
    if (Result.Code !== 0) {
        Reject(`${basename(Tool)} exited ${Result.Code}.`);
    }
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith(
            'windvale-generic-nominal-type-materialization-'
        )) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await mkdtemp(join(
    Temporaryˉroot, 'windvale-generic-nominal-type-materialization-'
));
var Passed = false;
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(
        SCRIPT_DIRECTORY, `Build-Cached-Project-Wvb.${Extension}`
    );
    const Package = join(
        SCRIPT_DIRECTORY, `Package-Segmented-Compiler-Wvb.${Extension}`
    );
    const Project = join(
        REPOSITORY_ROOT,
        'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-Generic-Nominal-Type-Materialization.wvproj'
    );
    const Wvb = join(Work, 'Generic-Nominal-Type-Materialization.wvb');
    const Application = join(
        Work, WINDOWS
            ? 'Generic-Nominal-Type-Materialization.exe'
            : 'Generic-Nominal-Type-Materialization.elf'
    );
    process.stdout.write(
        'START generic nominal type materialization step=build item=1/3\n'
    );
    await Requireˉsuccess(Build, [Project, Wvb]);
    process.stdout.write(
        'START generic nominal type materialization step=package item=2/3\n'
    );
    await Requireˉsuccess(
        Package, ['6', Wvb, Application, '--development-cache']
    );
    process.stdout.write(
        'START generic nominal type materialization step=execute item=3/3\n'
    );
    const Execution = await Runˉcommand(Application, [], true);
    if (Execution.Outputˉbytes !== 0 || Execution.Errorˉbytes !== 0) {
        Reject('The generic nominal type-materialization test wrote output.');
    }
    if (Execution.Code !== 42) {
        Reject(
            `The generic nominal type-materialization test returned ${Execution.Code}.`
        );
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    process.stdout.write(
        'native generic nominal type materialization status=Passed cases=28 result=42\n'
    );
}
