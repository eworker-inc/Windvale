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
const TESTS = [
    {
        Name: 'named-arguments',
        Project: 'Windvale-Native-Test-Language-1-Named-Argument-Semantics.wvproj',
        Selectors: [...'abcdef']
    },
    {
        Name: 'function-value-front-end',
        Project: 'Windvale-Native-Test-Language-1-Function-Value-Front-End.wvproj',
        Selectors: [null]
    },
    {
        Name: 'function-type-catalog',
        Project: 'Windvale-Native-Test-Language-1-Function-Type-Catalog.wvproj',
        Selectors: [null]
    },
    {
        Name: 'effects',
        Project: 'Windvale-Native-Test-Language-1-Effect-Semantics.wvproj',
        Selectors: [...'abcdefghijklm']
    },
    {
        Name: 'closure-captures',
        Project: 'Windvale-Native-Test-Language-1-Closure-Capture-Semantics.wvproj',
        Selectors: [...'abcdefghi']
    }
];

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

async function Requireˉbuild(Build, Test, Module) {
    const Project = join(REPOSITORY_ROOT, 'Projects', 'Tests', Test.Project);
    const Result = await Runˉcommand(Build, [Project, Module]);
    if (Result.Exceeded) {
        Reject(`The ${Test.Name} build exceeded the output limit.`);
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The ${Test.Name} build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') +
            Result.Output.toString('utf8')
        );
    }
}

async function Requireˉpackage(Packager, Test, Module, Application) {
    const Result = await Runˉcommand(
        Packager, ['1', Module, Application, '--development-cache']
    );
    if (Result.Exceeded) {
        Reject(`The ${Test.Name} native package exceeded the output limit.`);
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The ${Test.Name} native package failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') +
            Result.Output.toString('utf8')
        );
    }
    const Metadata = await stat(Application);
    if (!Metadata.isFile() || Metadata.size === 0) {
        Reject(`The ${Test.Name} package did not publish an application.`);
    }
}

async function Runˉcase(Application, Test, Selector, Index) {
    const Argumentsˉvalue = Selector === null ? [] : [Selector];
    const Result = await Runˉcommand(Application, Argumentsˉvalue);
    if (Result.Exceeded) {
        Reject(`${Test.Name} case ${Index} exceeded the output limit.`);
    }
    if (Result.Output.length !== 0 || Result.Error.length !== 0) {
        Reject(
            `${Test.Name} case ${Index} wrote output.\n` +
            Result.Output.toString('utf8') +
            Result.Error.toString('utf8')
        );
    }
    if (Result.Code !== 42) {
        Reject(`${Test.Name} case ${Index} returned ${Result.Code}.`);
    }
}

async function Removeˉwork(Work, Temporaryˉroot) {
    const Realˉroot = await realpath(Temporaryˉroot);
    const Realˉparent = await realpath(dirname(Work));
    if (Realˉparent !== Realˉroot ||
        !basename(Work).startsWith('windvale-callable-semantics-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}

const Temporaryˉroot = resolve(tmpdir());
const Work = await mkdtemp(join(
    Temporaryˉroot, 'windvale-callable-semantics-'
));
const Evidence = [];
var Passed = false;
var Completedˉcases = 0;
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Wvb.${Extension}`);
    const Packager = join(
        SCRIPT_DIRECTORY,
        `Package-Segmented-Compiler-Wvb.${Extension}`
    );
    const Totalˉitems = TESTS.length * 3;
    var Item = 0;
    for (const Test of TESTS) {
        const Module = join(Work, `${Test.Name}.wvb`);
        const Application = join(
            Work,
            WINDOWS ? `${Test.Name}.exe` : `${Test.Name}.elf`
        );

        Item += 1;
        process.stdout.write(
            `START language 1 callable semantics phase=build ` +
            `item=${Item}/${Totalˉitems} test=${Test.Name}\n`
        );
        await Requireˉbuild(Build, Test, Module);
        const Moduleˉbytes = await readFile(Module);
        Evidence.push({
            Name: Test.Name,
            Bytes: Moduleˉbytes.length,
            Digest: createHash('sha256').update(Moduleˉbytes).digest('hex')
        });

        Item += 1;
        process.stdout.write(
            `START language 1 callable semantics phase=package ` +
            `item=${Item}/${Totalˉitems} test=${Test.Name}\n`
        );
        await Requireˉpackage(Packager, Test, Module, Application);

        Item += 1;
        process.stdout.write(
            `START language 1 callable semantics phase=execute ` +
            `item=${Item}/${Totalˉitems} test=${Test.Name} ` +
            `cases=${Test.Selectors.length}\n`
        );
        await Promise.all(Test.Selectors.map(
            (Selector, Index) => Runˉcase(
                Application, Test, Selector, Completedˉcases + Index + 1
            )
        ));
        Completedˉcases += Test.Selectors.length;
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Temporaryˉroot);
}

if (Passed) {
    const Totalˉbytes = Evidence.reduce(
        (Total, Item) => Total + Item.Bytes, 0
    );
    const Evidenceˉdigest = createHash('sha256')
        .update(Evidence.map(Item => (
            `${Item.Name}:${Item.Bytes}:${Item.Digest}`
        )).join('\n'))
        .digest('hex');
    process.stdout.write(
        'native language 1 callable semantics status=Passed ' +
        `cases=${Completedˉcases} result=42 modules=${Evidence.length} ` +
        `wvb-bytes=${Totalˉbytes} evidence-sha256=${Evidenceˉdigest}\n`
    );
}
