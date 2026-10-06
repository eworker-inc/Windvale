import { createHash } from 'node:crypto';
import {
    lstat,
    mkdtemp,
    readFile,
    realpath,
    rm,
    stat,
    writeFile
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Getˉcurrentˉsplitˉcompilerˉkey, Getˉcurrentˉsplitˉcompilerˉfamily,
    Readˉpreparedˉsplitˉcompiler } from './Current-Split-Compiler-Cache-Core.mjs';
import { Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged } from './Native-Project-Cache-Key-Core.mjs';
import { Prepareˉhostedˉapplicationˉcontext, Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';
import { Requireˉloadedˉsegmentedˉhostedˉproducersˉunchanged } from './Build-Cached-Segmented-Hosted-Wvb.mjs';
import { fileURLToPath } from 'node:url';
import {
    CALLABLE_WVB_BASE64,
    CALLABLE_WVB_SHA256,
    CLOSURE_WVB_BASE64,
    CLOSURE_WVB_SHA256
} from './Language-1.0-Callable-Wvb-Fixtures.mjs';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 64 * 1024;
const HEARTBEAT_INTERVAL_MILLISECONDS = 30_000;
const PACKAGE_CONCURRENCY = 2;
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
        Selectors: [...'abcdefghijklmnopq']
    },
    {
        Name: 'callable-type-catalog',
        Project: 'Windvale-Native-Test-Language-1-Callable-Type-Catalog.wvproj',
        Selectors: [null]
    },
    {
        Name: 'closure-captures',
        Project: 'Windvale-Native-Test-Language-1-Closure-Capture-Semantics.wvproj',
        Selectors: [...'abcdefghijkl']
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

let Callableˉdeadline = null;
async function Runˉcommand(Tool, Argumentsˉvalue) {
    if (!Number.isSafeInteger(Callableˉdeadline) || Date.now() >= Callableˉdeadline)
        throw Object.assign(new Error('Callable command deadline expired.'), { exitCode: 124 });
    const Started = Date.now();
    const Heartbeat = setInterval(() => process.stdout.write(
        'INFO language 1 callable semantics tool=' + basename(Tool) + ' elapsed-ms=' +
        (Date.now() - Started) + ' remaining-ms=' + Math.max(0, Callableˉdeadline - Date.now()) + '\n'),
        HEARTBEAT_INTERVAL_MILLISECONDS);
    Heartbeat.unref();
    try {
        const Result = await Runˉdevelopmentˉcommand(Tool, Argumentsˉvalue, Callableˉdeadline,
            false, MAXIMUM_OUTPUT_BYTES);
        return { Code: Result.Code, Output: Buffer.from(Result.Output),
            Error: Buffer.from(Result.Error), Exceeded: false };
    } finally { clearInterval(Heartbeat); }
}

export function Parseˉcallableˉarguments(Arguments, Now = Date.now()) {
    const Usage = () => { throw Object.assign(new Error('Callable phases require --prepare-only or ' +
        '--prepared-products-only --maximum-seconds N.'), { exitCode: 64 }); };
    if (!Array.isArray(Arguments) || Arguments.some(Value => typeof Value !== 'string')) Usage();
    if (Arguments.length === 0) return Object.freeze({ Prepare: false, Prepared: false, Maximum: 3600 });
    if (![3, 5].includes(Arguments.length) || !['--prepare-only', '--prepared-products-only'].includes(Arguments[0]) ||
        Arguments[1] !== '--maximum-seconds' || !/^[1-9][0-9]{0,3}$/u.test(Arguments[2])) Usage();
    const Prepare = Arguments[0] === '--prepare-only', Maximum = Number(Arguments[2]);
    if (Maximum > (Prepare ? 4500 : 3600)) Usage();
    if (Arguments.length === 3) return Object.freeze({ Prepare, Prepared: !Prepare, Maximum });
    if (Arguments[3] !== '--deadline-ms' || !/^[1-9][0-9]*$/u.test(Arguments[4])) Usage();
    const Deadline = Number(Arguments[4]);
    if (!Number.isSafeInteger(Now) || !Number.isSafeInteger(Deadline) || Deadline <= Now || Deadline > Now + Maximum * 1000) Usage();
    return Object.freeze({ Prepare, Prepared: !Prepare, Maximum, Deadline });
}

export async function Withˉcallableˉenvironment(Request, Action) {
    if (!Request || typeof Request.Prepare !== 'boolean' || typeof Request.Prepared !== 'boolean' ||
        Request.Prepare && Request.Prepared || typeof Action !== 'function')
        throw Object.assign(new Error('Invalid callable phase environment.'), { exitCode: 64 });
    const Names = ['WINDVALE_PREPARED_COMPILER_ONLY', 'WINDVALE_PREPARED_PRODUCTS_ONLY'];
    const Before = Names.map(Name => process.env[Name]);
    if (Request.Prepare && Before.some(Value => Value !== undefined))
        throw Object.assign(new Error('Callable preparation refuses prepared behavior flags.'), { exitCode: 64 });
    try {
        if (Request.Prepared) for (const Name of Names) process.env[Name] = '1';
        return await Action();
    } finally {
        for (const [Index, Name] of Names.entries()) {
            if (Before[Index] === undefined) delete process.env[Name];
            else process.env[Name] = Before[Index];
        }
    }
}

async function Callableˉevidence(Path, Check) {
    Check();
    const Maximum = Path === process.execPath ? 134_217_728 :
        Path.endsWith('.wvb') ? 16_777_216 : 67_108_864;
    const Bytes = await Readˉboundedˉhostedˉfile(Path, 'callable input/product', Maximum);
    Check();
    return Object.freeze({ Path, Bytes: Bytes.length, Sha256: createHash('sha256').update(Bytes).digest('hex') });
}

async function Snapshotˉcallableˉinputs(Projects, Check) {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('callable-test-inputs-v1', [
        fileURLToPath(import.meta.url),
        ...['Language-1.0-Callable-Wvb-Fixtures.mjs', 'Current-Split-Compiler-Cache-Core.mjs',
            'Build-Current-Split-Project-Wvb.mjs', 'Build-Cached-Split-Project-Wvb.mjs',
            'Build-Cached-Segmented-Hosted-Wvb.mjs', 'Native-Project-Cache-Key-Core.mjs',
            'Native-Hosted-Application-Cache-Core.mjs', 'Build-Cached-Segmented-Project.mjs',
            'Development-Command-Core.mjs', `Check-Wvo.${Extension}`, `Link-Wvo.${Extension}`,
            `Package-Console.${Extension}`].map(Name => join(SCRIPT_DIRECTORY, Name)),
        join(REPOSITORY_ROOT, 'Artifacts/Native-Wvb-To-Wvo-Candidate', WINDOWS ? 'Wvb-To-Wvo.exe' : 'Wvb-To-Wvo.elf'),
    ]);
    const Requests = [];
    for (const Project of Projects) { Check(); Requests.push(await Getˉnativeˉprojectˉcacheˉrequest(Context, Project)); }
    // These immutable ABI22 tools still own the eight independent AOT checks.
    // They are distinct from the segmented hosted packager's producer inventory.
    const Aotˉcontext = await Prepareˉnativeˉprojectˉcacheˉcontext('callable-aot-inputs-v1', [
        join(SCRIPT_DIRECTORY, `Publish-Console.${Extension}`),
        join(SCRIPT_DIRECTORY, 'Check-Console-Publication-Candidate.mjs'),
        join(REPOSITORY_ROOT, 'Artifacts/Native-Wvo-Object-Candidate', WINDOWS ? 'Wvo-Object.exe' : 'Wvo-Object.elf'),
        join(REPOSITORY_ROOT, 'Artifacts/Native-Wv-Linker-Candidate', WINDOWS ? 'Wv-Linker.exe' : 'Wv-Linker.elf'),
        join(REPOSITORY_ROOT, 'Artifacts/Native-Console-Packager-Candidate', WINDOWS ? 'Console-Packager.exe' : 'Console-Packager.elf'),
        join(REPOSITORY_ROOT, 'Artifacts/Native-Console-Application-Publisher-Candidate',
            WINDOWS ? 'windows-x64-wvappublish.exe' : 'linux-x64-wvappublish.elf'),
    ]);
    Check();
    Requests.push(await Getˉnativeˉprojectˉcacheˉrequest(Aotˉcontext, Projects[0]));
    const Hosted = await Prepareˉhostedˉapplicationˉcontext(WINDOWS ? 'windows' : 'linux',
        join(SCRIPT_DIRECTORY, `Package-Hosted-Wvb.${Extension}`));
    Check();
    return { Requests, Node: await Callableˉevidence(process.execPath, Check),
        Hosted: Hosted.producerFields.map(Field => ({ label: Field.label, bytes: Buffer.from(Field.bytes) })) };
}

async function Requireˉcallableˉinputsˉunchanged(Snapshot, Check) {
    for (const Request of Snapshot.Requests) { Check(); await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request); }
    await Requireˉloadedˉsegmentedˉhostedˉproducersˉunchanged();
    const Hosted = await Prepareˉhostedˉapplicationˉcontext(WINDOWS ? 'windows' : 'linux',
        join(SCRIPT_DIRECTORY, `Package-Hosted-Wvb.${WINDOWS ? 'cmd' : 'sh'}`));
    if (Hosted.producerFields.length !== Snapshot.Hosted.length || Hosted.producerFields.some((Field, Index) =>
        Field.label !== Snapshot.Hosted[Index].label || !Field.bytes.equals(Snapshot.Hosted[Index].bytes)))
        Reject('Callable hosted packaging inputs changed.');
    const Node = await Callableˉevidence(process.execPath, Check);
    if (Node.Bytes !== Snapshot.Node.Bytes || Node.Sha256 !== Snapshot.Node.Sha256) Reject('Callable Node producer changed.');
    Check();
}

// Scoped phases use the exact ordinary source/image caches and only admit an
// already prepared current6 checkpoint. Injected callbacks serve the existing pure cache owner.
export async function Acquireˉcallableˉproducts({ Work, Deadline, Run,
    Prepareˉproducts = false,
    Getˉkey = Getˉcurrentˉsplitˉcompilerˉkey, Getˉfamily = Getˉcurrentˉsplitˉcompilerˉfamily,
    Readˉcompiler = Readˉpreparedˉsplitˉcompiler, Snapshot = Snapshotˉcallableˉinputs,
    Requireˉinputs = Requireˉcallableˉinputsˉunchanged, Evidence = Callableˉevidence }) {
    if (typeof Work !== 'string' || !Number.isSafeInteger(Deadline) || typeof Prepareˉproducts !== 'boolean' ||
        ![Run, Getˉkey, Getˉfamily, Readˉcompiler, Snapshot, Requireˉinputs, Evidence]
            .every(Value => typeof Value === 'function') || Work !== resolve(Work)) Reject('Invalid callable acquisition.');
    function Check() {
        if (Date.now() >= Deadline) throw Object.assign(new Error('Callable deadline expired.'), { exitCode: 124 });
    }
    Check();
    const Information = await lstat(Work, { bigint: true });
    if (!Information.isDirectory() || Information.isSymbolicLink() || await realpath(Work) !== Work || Work === REPOSITORY_ROOT)
        Reject('Callable products require an ordinary scoped work directory.');
    if (Prepareˉproducts && ['WINDVALE_PREPARED_COMPILER_ONLY', 'WINDVALE_PREPARED_PRODUCTS_ONLY']
        .some(Name => process.env[Name] !== undefined))
        throw Object.assign(new Error('Callable construction is forbidden in prepared behavior.'), { exitCode: 64 });
    const Projects = [...TESTS.map(Test => join(REPOSITORY_ROOT, 'Projects/Tests', Test.Project)),
        join(REPOSITORY_ROOT, 'Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj')];
    const Inputs = await Snapshot(Projects, Check);
    const Key = await Getˉkey();
    if (typeof Key !== 'string' || !/^[0-9a-f]{64}$/u.test(Key)) Reject('Invalid current compiler identity.');
    Check();
    const Targets = [...TESTS.map(Test => ({ Name: Test.Name, Test, Profile: '1' })),
        { Name: 'verifier', Profile: '2' }];
    const Measurements = [];
    for (const Target of Targets) {
        Target.Module = join(Work, Target.Name + '.wvb');
        Target.Application = join(Work, Target.Name + (WINDOWS ? '.exe' : '.elf'));
    }
    const Compiler = await Readˉcompiler(await Getˉfamily(), Key);
    if (Compiler.status !== 'Hit' || typeof Compiler.Requireˉunchanged !== 'function')
        throw Object.assign(new Error('Callable phases require the existing exact current6 checkpoint; ' +
            'prepare it explicitly before entering this owner.'), { exitCode: 64 });
    for (const [Index, Target] of Targets.entries()) {
        Check();
        await Run('source-' + Target.Name, process.execPath,
            [join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs'), '--deadline-ms', String(Deadline),
                '--compiler-checkpoint', Key, '--prepared-compiler-only', Projects[Index], Target.Module]);
        Measurements.push(await Evidence(Target.Module, Check));
        await Run('package-' + Target.Name, process.execPath,
            [join(SCRIPT_DIRECTORY, 'Build-Cached-Segmented-Hosted-Wvb.mjs'), '--deadline-ms', String(Deadline),
                Target.Profile, Target.Module, Target.Application]);
        Measurements.push(await Evidence(Target.Application, Check));
    }
    async function Requireˉunchanged() {
        Check();
        const Current = await lstat(Work, { bigint: true });
        if (!Current.isDirectory() || Current.isSymbolicLink() || Current.dev !== Information.dev || Current.ino !== Information.ino)
            Reject('Callable work directory changed.');
        await Requireˉinputs(Inputs, Check);
        await Compiler.Requireˉunchanged();
        if (await Getˉkey() !== Key) Reject('Callable current compiler inputs changed.');
        for (const Before of Measurements) {
            const After = await Evidence(Before.Path, Check);
            if (Before.Bytes !== After.Bytes || Before.Sha256 !== After.Sha256)
                Reject('Callable prepared product changed: ' + Before.Path);
        }
        Check();
    }
    await Requireˉunchanged();
    return Object.freeze({ Products: Targets.slice(0, -1), Verifier: Targets.at(-1),
        Compilerˉkey: Key, Requireˉunchanged });
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
        process.execPath, [Packager, '1', Module, Application]
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
        Callableˉkind: 317,
        Callableˉprofile: 318
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

async function Requireˉnativeˉexecution(
    Lowerer,
    Checker,
    Linker,
    Packager,
    Name,
    Module,
    Work,
    Target,
    Executableˉsuffix
) {
    const Object = join(Work, `${Name}.wvo`);
    const Image = join(Work, `${Name}.bin`);
    const Application = join(Work, `${Name}${Executableˉsuffix}`);
    const Lower = await Runˉcommand(Lowerer, [Module, Object]);
    const Lowerˉoutput = Lower.Output.toString('utf8').replaceAll('\r\n', '\n');
    if (Lower.Exceeded || Lower.Code !== 0 || Lower.Error.length !== 0 ||
        !/^native x64 status=Valid abi=22 code-bytes=[1-9][0-9]* object-bytes=[1-9][0-9]*\n$/u
            .test(Lowerˉoutput)) {
        Reject(
            `The ${Name} native lowering failed with exit ${Lower.Code}.\n` +
            Lower.Error.toString('utf8') + Lowerˉoutput
        );
    }
    const Check = await Runˉcommand(Checker, [Object]);
    if (Check.Exceeded || Check.Code !== 0 || Check.Error.length !== 0) {
        Reject(
            `The ${Name} native object check failed with exit ${Check.Code}.\n` +
            Check.Error.toString('utf8') + Check.Output.toString('utf8')
        );
    }
    const Link = await Runˉcommand(Linker, ['0', 'Main', Image, Object]);
    const Linkˉoutput = Link.Output.toString('utf8').replaceAll('\r\n', '\n');
    const Entryˉmatch = /^entry name=Main address=([0-9]+)$/mu.exec(Linkˉoutput);
    if (Link.Exceeded || Link.Code !== 0 || Link.Error.length !== 0 ||
        Entryˉmatch === null) {
        Reject(
            `The ${Name} native link failed with exit ${Link.Code}.\n` +
            Link.Error.toString('utf8') + Linkˉoutput
        );
    }
    const Package = await Runˉcommand(
        Packager, [Target, Image, Entryˉmatch[1], Application]
    );
    if (Package.Exceeded || Package.Code !== 0 || Package.Error.length !== 0) {
        Reject(
            `The ${Name} native package failed with exit ${Package.Code}.\n` +
            Package.Error.toString('utf8') + Package.Output.toString('utf8')
        );
    }
    const Run = await Runˉcommand(Application, []);
    if (Run.Exceeded || Run.Code !== 42 ||
        Run.Output.length !== 0 || Run.Error.length !== 0) {
        Reject(
            `The ${Name} native execution returned ${Run.Code}.\n` +
            Run.Error.toString('utf8') + Run.Output.toString('utf8')
        );
    }
    const Objectˉbytes = await readFile(Object);
    return {
        Bytes: Objectˉbytes.length,
        Digest: createHash('sha256').update(Objectˉbytes).digest('hex')
    };
}

async function Requireˉnativeˉrejection(Lowerer, Name, Module, Work) {
    const Object = join(Work, `${Name}-rejected.wvo`);
    const Result = await Runˉcommand(Lowerer, [Module, Object]);
    const Output = Buffer.concat([Result.Output, Result.Error])
        .toString('utf8').replaceAll('\r\n', '\n');
    if (Result.Exceeded || Result.Code === 0 ||
        !Output.includes('native x64 status=')) {
        Reject(`The ${Name} native lowering was not rejected.\n${Output}`);
    }
    if (await stat(Object).then(() => true, () => false)) {
        Reject(`The ${Name} native rejection published an object.`);
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

async function Main(Request) {
const Started = Date.now(), Deadline = Request.Deadline ?? Started + Request.Maximum * 1000;
Callableˉdeadline = Deadline - 30_000;
const Temporaryˉroot = resolve(tmpdir());
const Work = await realpath(await mkdtemp(join(
    Temporaryˉroot, 'windvale-callable-semantics-'
)));
const Evidence = [];
var Passed = false;
var Completedˉcases = 0;
let Scopedˉproducts = null;
const Phaseˉheartbeat = setInterval(() => process.stdout.write('INFO callable owner phase=' +
    (Request.Prepare ? 'preparation' : Request.Prepared ? 'prepared-behavior' : 'default') +
    ' elapsed-ms=' + (Date.now() - Started) + ' remaining-ms=' + Math.max(0, Deadline - Date.now()) + '\n'),
    HEARTBEAT_INTERVAL_MILLISECONDS);
Phaseˉheartbeat.unref();
try {
    if (Request.Prepare || Request.Prepared) {
        Scopedˉproducts = await Acquireˉcallableˉproducts({ Work, Deadline: Callableˉdeadline,
            Prepareˉproducts: Request.Prepare,
            Run: async (Step, Tool, Arguments) => {
                process.stdout.write('START callable products step=' + Step + '\n');
                const Result = await Runˉcommand(Tool, Arguments);
                if (Result.Code !== 0 || Result.Error.length !== 0) throw Object.assign(
                    new Error('Callable product ' + Step + ' failed: ' + Result.Code + '\n' +
                        Result.Error.toString('utf8') + Result.Output.toString('utf8')),
                    { exitCode: Number.isInteger(Result.Code) && Result.Code !== 0 ? Result.Code : 1 });
            } });
        if (Request.Prepare) {
            await Scopedˉproducts.Requireˉunchanged();
            process.stdout.write('native language 1 callable preparation status=Prepared products=8 ' +
                'behavior-execution=skipped compiler-construction=forbidden\n');
            return;
        }
    }
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Wvb.${Extension}`);
    const Packager = join(
        SCRIPT_DIRECTORY,
        'Build-Cached-Segmented-Hosted-Wvb.mjs'
    );
    const Totalˉitems = TESTS.length * 3 + 5;
    var Item = 0;
    const Products = [];
    for (const Test of TESTS) {
        const Module = join(Work, `${Test.Name}.wvb`);

        Item += 1;
        process.stdout.write(
            `START language 1 callable semantics phase=build ` +
            `item=${Item}/${Totalˉitems} test=${Test.Name}\n`
        );
        if (Scopedˉproducts === null) await Requireˉbuild(Build, Test, Module);
        const Moduleˉbytes = await readFile(Module);
        Evidence.push({
            Name: Test.Name,
            Bytes: Moduleˉbytes.length,
            Digest: createHash('sha256').update(Moduleˉbytes).digest('hex')
        });
        Products.push({
            Test,
            Module,
            Application: join(
                Work, WINDOWS ? `${Test.Name}.exe` : `${Test.Name}.elf`
            )
        });
    }

    for (var Start = 0; Scopedˉproducts === null && Start < Products.length; Start += PACKAGE_CONCURRENCY) {
        const Batch = Products.slice(Start, Start + PACKAGE_CONCURRENCY);
        await Promise.all(Batch.map(Product => {
            Item += 1;
            process.stdout.write(
                `START language 1 callable semantics phase=package ` +
                `item=${Item}/${Totalˉitems} test=${Product.Test.Name} ` +
                `parallel=${Batch.length}/${PACKAGE_CONCURRENCY}\n`
            );
            return Requireˉpackage(
                Packager, Product.Test, Product.Module, Product.Application
            );
        }));
    }

    for (const Product of Products) {
        const { Test, Application } = Product;
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

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=verifier-build ` +
        `item=${Item}/${Totalˉitems}\n`
    );
    if (Scopedˉproducts === null) await Requireˉprojectˉbuild(
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
    if (Scopedˉproducts === null) await Requireˉhostedˉpackage(
        Hostedˉpackager, 'callable verifier', '2',
        Verifierˉmodule, Verifier, Target
    );

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=verify ` +
        `item=${Item}/${Totalˉitems} cases=17\n`
    );
    await Requireˉverification(
        Verifier, Callableˉmodule, true, 'callable WVB oracle'
    );
    const Nativeˉrejections = [];
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
        if (Name === 'callable-version-downgrade' ||
            Name === 'callable-target-signature') {
            Nativeˉrejections.push([Name, Candidateˉpath]);
        }
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
        }],
        ['closure-profile-mismatch', Bytes => {
            Bytes[Closureˉlayout.Callableˉprofile] = 2;
        }]
    ];
    for (const [Name, Mutate] of Malformedˉclosureˉcases) {
        const Candidate = Buffer.from(Closureˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = join(Work, `${Name}.wvb`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        await Requireˉverification(Verifier, Candidateˉpath, false, Name);
        if (Name === 'closure-version-downgrade' ||
            Name === 'closure-target-signature' ||
            Name === 'closure-capture-shape' ||
            Name === 'closure-profile-mismatch') {
            Nativeˉrejections.push([Name, Candidateˉpath]);
        }
    }

    Completedˉcases += 17;

    const Lowerer = join(
        REPOSITORY_ROOT,
        'Artifacts',
        'Native-Wvb-To-Wvo-Candidate',
        WINDOWS ? 'Wvb-To-Wvo.exe' : 'Wvb-To-Wvo.elf'
    );
    const Checker = join(SCRIPT_DIRECTORY, `Check-Wvo.${Extension}`);
    const Linker = join(SCRIPT_DIRECTORY, `Link-Wvo.${Extension}`);
    const Consoleˉpackager = join(
        SCRIPT_DIRECTORY, `Package-Console.${Extension}`
    );
    const Consoleˉtarget = WINDOWS
        ? 'windows-x64-console-v1'
        : 'linux-x64-console-v1';

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=native-aot ` +
        `item=${Item}/${Totalˉitems} cases=2\n`
    );
    const Callableˉobject = await Requireˉnativeˉexecution(
        Lowerer, Checker, Linker, Consoleˉpackager,
        'callable-native', Callableˉmodule, Work,
        Consoleˉtarget, Executableˉsuffix
    );
    const Closureˉobject = await Requireˉnativeˉexecution(
        Lowerer, Checker, Linker, Consoleˉpackager,
        'closure-native', Closureˉmodule, Work,
        Consoleˉtarget, Executableˉsuffix
    );
    Completedˉcases += 2;

    Item += 1;
    process.stdout.write(
        `START language 1 callable semantics phase=native-rejections ` +
        `item=${Item}/${Totalˉitems} cases=${Nativeˉrejections.length}\n`
    );
    for (const [Name, Module] of Nativeˉrejections) {
        await Requireˉnativeˉrejection(Lowerer, Name, Module, Work);
    }
    Completedˉcases += Nativeˉrejections.length;
    Evidence.push({
        Name: 'callable-native-object',
        Bytes: Callableˉobject.Bytes,
        Digest: Callableˉobject.Digest
    });
    Evidence.push({
        Name: 'closure-native-object',
        Bytes: Closureˉobject.Bytes,
        Digest: Closureˉobject.Digest
    });
    if (Completedˉcases !== 64 || Evidence.length !== 11) Reject('Callable semantic case inventory differs.');
    if (Scopedˉproducts !== null) await Scopedˉproducts.Requireˉunchanged();
    Passed = true;
} finally {
    clearInterval(Phaseˉheartbeat);
    await Removeˉwork(Work, Temporaryˉroot);
    if (Date.now() >= Deadline) throw Object.assign(new Error('Callable deadline exceeded during cleanup.'), { exitCode: 124 });
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
        `INFO callable product identities evidence-bytes=${Totalˉbytes} evidence-sha256=${Evidenceˉdigest}\n` +
        'native language 1 callable semantics status=Passed cases=64 result=42 modules=11 native-aot-cases=8\n'
    );
}

}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
    try {
        const Request = Parseˉcallableˉarguments(process.argv.slice(2));
        await Withˉcallableˉenvironment(Request, () => Main(Request));
    } catch (Error) { process.stderr.write(Error.message + '\n'); process.exitCode = Error.exitCode ?? 1; }
}
