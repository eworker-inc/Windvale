import { createHash } from 'node:crypto';
import {
    mkdtemp,
    readFile,
    realpath,
    rm,
    stat,
    writeFile
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 64 * 1024;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const CALLABLE_WVB_BASE64 =
    'V1ZCMQEAHgAHAAAAAQAAAGAAAAABLAAAAExhbmd1YWdly4lvbmXLiWNhbGxhYmxly4lpbmRpcmVjdMuJZXhlY3V0aW9uAQECAwAAAAUAAABsaW51eAcAAAB3aW5kb3dzCAAAAHdpbmR2YWxlAAAAAAAAAAACAAAABAAAAAAAAAADAAAABAAAAAAAAAAEAAAAVwAAAAIAAAAIAAAAQWRky4lvbmUBAAAAAQEDAAAAAQEBAAAAACoAAAACAAAABAAAAE1haW4AAAAAAQUAAAAjAAAAACMAAAAAIwAAAAABASoAAABGAAAAAgAAAAUAAABwAAAABAAAAAAFAQAAAAEBAAAABQIAAAAEAQAAAAQCAAAAEAUDAAAABAMAAABR0wAAAAAAAAAABQEAAAAEAQAAAAUAAAAABAAAAAAFAgAAAAEpAAAABQMAAAAEAgAAAAQDAAAA1AAAAAAFBAAAAAQEAAAAUQYAAAARAAAAAQAAAAQAAABNYWluAQEAAAAHAAAADAAAAAEAAAAIAQEBAAAAAQ==';
const CALLABLE_WVB_SHA256 =
    '30eab353a6187ead317438d2c63a2bd6aa53d9ec682bc5c59d9d3b82530edfaf';
const CLOSURE_WVB_BASE64 =
    'V1ZCMQEAHwAHAAAAAQAAAGAAAAABLAAAAExhbmd1YWdly4lvbmXLiWNhbGxhYmxly4lpbmRpcmVjdMuJZXhlY3V0aW9uAQECAwAAAAUAAABsaW51eAcAAAB3aW5kb3dzCAAAAHdpbmR2YWxlAAAAAAAAAAACAAAABAAAAAAAAAADAAAABAAAAAAAAAAEAAAASQAAAAIAAAAIAAAAQWRky4lvbmUCAAAAAQEBAAAAAAAAAAAMAAAAAgAAAAQAAABNYWluAAAAAAEBAAAAIwAAAAAMAAAAJwAAAAIAAAAFAAAAMwAAAAQAAAAABAEAAAAQUQEoAAAA1QAAAAAAAAAAAQAAAAUAAAAABAAAAAABAgAAANQAAAAAUQYAAAARAAAAAQAAAAQAAABNYWluAQEAAAAHAAAADAAAAAEAAAAIAQEBAAAAAQ==';
const CLOSURE_WVB_SHA256 =
    '397f716af132192697c77d9f4f03e72c937e188aca78cf0474c9faaa2234e0e2';
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
        Name: 'callable-type-catalog',
        Project: 'Windvale-Native-Test-Language-1-Callable-Type-Catalog.wvproj',
        Selectors: [null]
    },
    {
        Name: 'closure-captures',
        Project: 'Windvale-Native-Test-Language-1-Closure-Capture-Semantics.wvproj',
        Selectors: [...'abcdefghi']
    },
    {
        Name: 'closure-lowering-catalog',
        Project: 'Windvale-Native-Test-Language-1-Closure-Lowering-Catalog.wvproj',
        Selectors: [null]
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

async function Requireˉprojectˉbuild(Build, Name, Project, Module) {
    const Result = await Runˉcommand(Build, [Project, Module]);
    if (Result.Exceeded) {
        Reject(`The ${Name} build exceeded the output limit.`);
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The ${Name} build failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
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

async function Requireˉhostedˉpackage(
    Packager,
    Name,
    Profile,
    Module,
    Application,
    Target
) {
    const Result = await Runˉcommand(
        Packager, [Profile, Module, Application, Target]
    );
    if (Result.Exceeded) {
        Reject(`The ${Name} native package exceeded the output limit.`);
    }
    if (Result.Code !== 0 || Result.Error.length !== 0) {
        Reject(
            `The ${Name} native package failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8')
        );
    }
    const Metadata = await stat(Application);
    if (!Metadata.isFile() || Metadata.size === 0) {
        Reject(`The ${Name} package did not publish an application.`);
    }
}

function Callableˉwvbˉlayout(Module) {
    if (Module.length !== 400 ||
        createHash('sha256').update(Module).digest('hex') !==
            CALLABLE_WVB_SHA256 ||
        Module.toString('ascii', 0, 4) !== 'WVB1' ||
        Module.readUInt16LE(4) !== 1 || Module.readUInt16LE(6) !== 30 ||
        Module.readUInt32LE(8) !== 7) {
        Reject('The callable WVB oracle identity is invalid.');
    }
    const Sections = new Map();
    let Offset = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Offset > Module.length - 8 || Module[Offset] !== Kind ||
            Module[Offset + 1] !== 0 || Module[Offset + 2] !== 0 ||
            Module[Offset + 3] !== 0) {
            Reject(`The callable WVB section ${Kind} is malformed.`);
        }
        const Length = Module.readUInt32LE(Offset + 4);
        if (Length > Module.length - Offset - 8) {
            Reject(`The callable WVB section ${Kind} is truncated.`);
        }
        Sections.set(Kind, { Offset, Length });
        Offset += 8 + Length;
    }
    const Code = Sections.get(5);
    const Types = Sections.get(7);
    if (Offset !== Module.length || Code.Offset !== 235 || Code.Length !== 112 ||
        Types.Offset !== 380 || Types.Length !== 12 ||
        Module[285] !== 211 || Module.readUInt32LE(286) !== 0 ||
        Module.readUInt32LE(290) !== 0 || Module[339] !== 212 ||
        Module.readUInt32LE(340) !== 0 ||
        Module.readUInt32LE(388) !== 1 || Module[392] !== 8 ||
        Module[393] !== 1 || Module[394] !== 1 ||
        Module.readUInt32LE(395) !== 1 || Module[399] !== 1) {
        Reject('The callable WVB executable structure is invalid.');
    }
    return {
        Referenceˉtarget: 286,
        Referenceˉtype: 290,
        Callˉtype: 340,
        Callableˉkind: 392
    };
}

function Closureˉwvbˉlayout(Module) {
    if (Module.length !== 325 ||
        createHash('sha256').update(Module).digest('hex') !==
            CLOSURE_WVB_SHA256 ||
        Module.toString('ascii', 0, 4) !== 'WVB1' ||
        Module.readUInt16LE(4) !== 1 || Module.readUInt16LE(6) !== 31 ||
        Module.readUInt32LE(8) !== 7 || Module[246] !== 213 ||
        Module[274] !== 212 || Module[317] !== 8) {
        Reject('The closure WVB oracle identity is invalid.');
    }
    return {
        Captureˉparameterˉshape: 168,
        Closureˉtarget: 247,
        Closureˉtype: 251,
        Captureˉcount: 255,
        Callˉtype: 275,
        Callableˉkind: 317
    };
}

async function Requireˉverification(Verifier, Module, Valid, Name) {
    const Result = await Runˉcommand(Verifier, [Module]);
    if (Result.Exceeded) {
        Reject(`The ${Name} verification exceeded the output limit.`);
    }
    const Output = Buffer.concat([Result.Output, Result.Error])
        .toString('utf8').replaceAll('\r\n', '\n');
    if (Valid) {
        if (Result.Code !== 0 || Result.Error.length !== 0 ||
            Output !== 'wvb status=Valid profile=compiler-aligned\n') {
            Reject(`The ${Name} was not accepted exactly.\n${Output}`);
        }
        return;
    }
    if (Result.Code === 0 || !Output.includes('wvb status=Invalid')) {
        Reject(`The ${Name} was not rejected.\n${Output}`);
    }
}

async function Requireˉcallableˉexecution(Runner, Module) {
    const Result = await Runˉcommand(Runner, [Module, '--report-steps']);
    if (Result.Exceeded) {
        Reject('The callable execution exceeded the output limit.');
    }
    const Output = Result.Output.toString('utf8').replaceAll('\r\n', '\n');
    if (Result.Code !== 0 || Result.Error.length !== 0 ||
        Output !== 'Result: 42\nInstructions: 24\n') {
        Reject(
            `The callable execution failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Output
        );
    }
}

async function Requireˉclosureˉexecution(Runner, Module) {
    const Result = await Runˉcommand(Runner, [Module, '--report-steps']);
    if (Result.Exceeded) {
        Reject('The closure execution exceeded the output limit.');
    }
    const Output = Result.Output.toString('utf8').replaceAll('\r\n', '\n');
    if (Result.Code !== 0 || Result.Error.length !== 0 ||
        Output !== 'Result: 42\nInstructions: 11\n') {
        Reject(
            `The closure execution failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Output
        );
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
    const Totalˉitems = TESTS.length * 3 + 6;
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

    const Callableˉmodule = join(Work, 'callable-indirect-execution.wvb');
    const Callableˉbytes = Buffer.from(CALLABLE_WVB_BASE64, 'base64');
    const Callableˉlayout = Callableˉwvbˉlayout(Callableˉbytes);
    await writeFile(Callableˉmodule, Callableˉbytes, { flag: 'wx' });
    Evidence.push({
        Name: 'callable-indirect-execution',
        Bytes: Callableˉbytes.length,
        Digest: CALLABLE_WVB_SHA256
    });
    const Closureˉmodule = join(Work, 'closure-environment-execution.wvb');
    const Closureˉbytes = Buffer.from(CLOSURE_WVB_BASE64, 'base64');
    const Closureˉlayout = Closureˉwvbˉlayout(Closureˉbytes);
    await writeFile(Closureˉmodule, Closureˉbytes, { flag: 'wx' });
    Evidence.push({
        Name: 'closure-environment-execution',
        Bytes: Closureˉbytes.length,
        Digest: CLOSURE_WVB_SHA256
    });

    const Hostedˉpackager = join(
        SCRIPT_DIRECTORY, `Package-Hosted-Wvb.${Extension}`
    );
    const Executableˉsuffix = WINDOWS ? '.exe' : '.elf';
    const Target = WINDOWS ? 'windows' : 'linux';
    const Verifierˉmodule = join(Work, 'verifier.wvb');
    const Verifier = join(Work, `verifier${Executableˉsuffix}`);
    const Runnerˉmodule = join(Work, 'runner.wvb');
    const Runner = join(Work, `runner${Executableˉsuffix}`);

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=verifier-build ` +
        `item=${Item}/${Totalˉitems}\n`
    );
    await Requireˉprojectˉbuild(
        Build,
        'callable verifier',
        join(
            REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Compiler-Wvb-Verifier.wvproj'
        ),
        Verifierˉmodule
    );

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=verifier-package ` +
        `item=${Item}/${Totalˉitems}\n`
    );
    await Requireˉhostedˉpackage(
        Hostedˉpackager, 'callable verifier', '2',
        Verifierˉmodule, Verifier, Target
    );

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=verify ` +
        `item=${Item}/${Totalˉitems} cases=16\n`
    );
    await Requireˉverification(
        Verifier, Callableˉmodule, true, 'callable WVB oracle'
    );
    const Malformedˉcases = [
        ['callable-version-downgrade', Bytes => {
            Bytes.writeUInt16LE(29, 6);
        }],
        ['callable-target-signature', Bytes => {
            Bytes.writeUInt32LE(1, Callableˉlayout.Referenceˉtarget);
        }],
        ['callable-reference-type', Bytes => {
            Bytes.writeUInt32LE(1, Callableˉlayout.Referenceˉtype);
        }],
        ['callable-invocation-type', Bytes => {
            Bytes.writeUInt32LE(1, Callableˉlayout.Callˉtype);
        }],
        ['callable-type-kind', Bytes => {
            Bytes[Callableˉlayout.Callableˉkind] = 7;
        }]
    ];
    for (const [Name, Mutate] of Malformedˉcases) {
        const Candidate = Buffer.from(Callableˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = join(Work, `${Name}.wvb`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉverification(Verifier, Candidateˉpath, false, Name);
    }
    await Requireˉverification(
        Verifier, Closureˉmodule, true, 'closure WVB oracle'
    );
    const Malformedˉclosureˉcases = [
        ['closure-version-downgrade', Bytes => {
            Bytes.writeUInt16LE(30, 6);
        }],
        ['closure-target-signature', Bytes => {
            Bytes.writeUInt32LE(1, Closureˉlayout.Closureˉtarget);
        }],
        ['closure-reference-type', Bytes => {
            Bytes.writeUInt32LE(1, Closureˉlayout.Closureˉtype);
        }],
        ['closure-zero-captures', Bytes => {
            Bytes.writeUInt32LE(0, Closureˉlayout.Captureˉcount);
        }],
        ['closure-capture-limit', Bytes => {
            Bytes.writeUInt32LE(65, Closureˉlayout.Captureˉcount);
        }],
        ['closure-capture-shape', Bytes => {
            Bytes[Closureˉlayout.Captureˉparameterˉshape] = 2;
        }],
        ['closure-reference-backed-capture', Bytes => {
            Bytes[Closureˉlayout.Captureˉparameterˉshape] = 3;
        }],
        ['closure-invocation-type', Bytes => {
            Bytes.writeUInt32LE(1, Closureˉlayout.Callˉtype);
        }],
        ['closure-type-kind', Bytes => {
            Bytes[Closureˉlayout.Callableˉkind] = 7;
        }]
    ];
    for (const [Name, Mutate] of Malformedˉclosureˉcases) {
        const Candidate = Buffer.from(Closureˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = join(Work, `${Name}.wvb`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉverification(Verifier, Candidateˉpath, false, Name);
    }

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=runner-build ` +
        `item=${Item}/${Totalˉitems}\n`
    );
    await Requireˉprojectˉbuild(
        Build,
        'callable runner',
        join(
            REPOSITORY_ROOT, 'Projects', 'Tools',
            'Windvale-Wvb-Runner.wvproj'
        ),
        Runnerˉmodule
    );

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=runner-package ` +
        `item=${Item}/${Totalˉitems}\n`
    );
    await Requireˉhostedˉpackage(
        Hostedˉpackager, 'callable runner', '5',
        Runnerˉmodule, Runner, Target
    );

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=execute ` +
        `item=${Item}/${Totalˉitems} cases=2\n`
    );
    await Requireˉcallableˉexecution(Runner, Callableˉmodule);
    await Requireˉclosureˉexecution(Runner, Closureˉmodule);
    Completedˉcases += 18;
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
