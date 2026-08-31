import { spawn } from 'node:child_process';
import {
    lstat,
    mkdir,
    mkdtemp,
    readFile,
    realpath,
    rm,
    writeFile,
} from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_ANALYZER_BYTES = 134_217_728;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_MODULE_BYTES = 1_048_576;
const MAXIMUM_WIR_BYTES = 4_194_304;
const ANALYSIS_TIMEOUT_MILLISECONDS = 120_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SYSTEM_HEADER =
    'profile system; platform linux, windows, windvale; authority system; ';

if (process.argv.length !== 3) {
    Usage();
}

const Analyzer = resolve(process.argv[2]);
const Analyzerˉstatus = await lstat(Analyzer);
if (!Analyzerˉstatus.isFile() || Analyzerˉstatus.size <= 0 ||
    Analyzerˉstatus.size > MAXIMUM_ANALYZER_BYTES ||
    await realpath(Analyzer) !== Analyzer) {
    Reject('The unsafe-type Analyzer must be an ordinary canonical file.');
}

const Unsafeˉmodule = await Readˉeditionˉoneˉbody(
    join(REPOSITORY_ROOT, 'Libraries', 'Foundation', 'Unsafe', 'Unsafe.wv'),
    'Foundation unsafe module',
);
const Memoryˉmodule = await Readˉeditionˉoneˉbody(
    join(REPOSITORY_ROOT, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
    'Foundation memory module',
);

const Canonicalˉvalid =
    'module Unsafeˉtypesˉvalid; ' + SYSTEM_HEADER +
    'import Foundationˉunsafe as Unsafe; ' +
    'enum Abi: u8 { Witness = 1u8; } ' +
    'fn Pointer(Value: Unsafe.Foreignˉpointer<u8, Abi>) -> i32 { return 1; } ' +
    'fn Nullable(Value: Unsafe.Nullableˉforeignˉpointer<u8, Abi>) -> i32 ' +
    '{ return 2; } ' +
    'fn Scratch(Value: Unsafe.Foreignˉscratch<Abi>) -> i32 { return 3; } ' +
    'fn Region(Value: Unsafe.Foreignˉwriteˉregion<Abi>) -> i32 { return 4; } ' +
    'export fn Main() -> i32 { return 42; }';

const Opaqueˉtypes = [
    { Name: 'Foreignˉpointer', Arguments: '<u8, Abi>' },
    { Name: 'Nullableˉforeignˉpointer', Arguments: '<u8, Abi>' },
    { Name: 'Foreignˉscratch', Arguments: '<Abi>' },
    { Name: 'Foreignˉwriteˉregion', Arguments: '<Abi>' },
];

const Cases = [{
    Name: 'canonical-type-identities',
    Expected: 'valid',
    Modules: [Canonicalˉvalid, Memoryˉmodule, Unsafeˉmodule],
}];
for (const Type of Opaqueˉtypes) {
    Cases.push({
        Name: `construct-${Asciiˉname(Type.Name)}`,
        Expected: 'invalid-unsafe-value',
        Modules: [
            'module Unsafeˉconstruct; ' + SYSTEM_HEADER +
            'import Foundationˉunsafe as Unsafe; ' +
            'enum Abi: u8 { Witness = 1u8; } ' +
            'export fn Main() -> i32 { let Value = Unsafe.' + Type.Name +
            Type.Arguments + ' { Opaqueˉidentity: 1u64 }; return 42; }',
            Memoryˉmodule,
            Unsafeˉmodule,
        ],
    });
    Cases.push({
        Name: `observe-${Asciiˉname(Type.Name)}`,
        Expected: 'invalid-unsafe-value',
        Modules: [
            'module Unsafeˉobserve; ' + SYSTEM_HEADER +
            'import Foundationˉunsafe as Unsafe; ' +
            'enum Abi: u8 { Witness = 1u8; } ' +
            'fn Observe(Value: Unsafe.' + Type.Name + Type.Arguments +
            ') -> u64 { return Value.Opaqueˉidentity; } ' +
            'export fn Main() -> i32 { return 42; }',
            Memoryˉmodule,
            Unsafeˉmodule,
        ],
    });
}
Cases.push({
    Name: 'noncanonical-lookalike-is-ordinary',
    Expected: 'valid',
    Modules: [
        'module Unsafeˉlookalike; ' + SYSTEM_HEADER +
        'record Foreignˉpointer<T, Abi> { Opaqueˉidentity: u64; } ' +
        'enum Abi: u8 { Witness = 1u8; } ' +
        'export fn Main() -> i32 { let Value = ' +
        'Foreignˉpointer<u8, Abi> { Opaqueˉidentity: 7u64 }; ' +
        'if Value.Opaqueˉidentity == 7u64 { return 42; } return 0; }',
    ],
});

const Work = await mkdtemp(join(tmpdir(), 'windvale-unsafe-types-'));
var Valid = 0;
var Rejected = 0;
try {
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const Case = Cases[Index];
        const Caseˉdirectory = join(
            Work, `${String(Index).padStart(2, '0')}-${Case.Name}`
        );
        await mkdir(Caseˉdirectory);
        const Input = join(Caseˉdirectory, 'Source.wvss');
        const Sourceˉoutput = join(Caseˉdirectory, 'Output.wvss');
        const Manifest = join(Caseˉdirectory, 'Manifest.wvca');
        const Bindings = join(Caseˉdirectory, 'Bindings.wvlb');
        const Wir = join(Caseˉdirectory, 'Wir.wvir');
        await writeFile(Input, Sourceˉset(Case.Modules), { flag: 'wx' });
        const Result = await Runˉanalyzer([
            '--internal-source-set', Input,
            Sourceˉoutput, Manifest, Bindings, Wir,
        ]);
        if (Case.Expected === 'valid') {
            if (Result.Code !== 0 || Result.Exceeded) {
                Reject(
                    `Unsafe-type case ${Case.Name} failed with status ` +
                    `${Result.Code}.\n${Result.Diagnostic}`
                );
            }
            const Wirˉbytes = await readFile(Wir);
            if (Wirˉbytes.length < 48 ||
                Wirˉbytes.length > MAXIMUM_WIR_BYTES ||
                Wirˉbytes.subarray(0, 4).toString('ascii') !== 'WVIR') {
                Reject(`Unsafe-type case ${Case.Name} published invalid WIR.`);
            }
            Valid += 1;
        } else {
            if (Result.Code !== 1 || Result.Exceeded ||
                !Result.Diagnostic.includes(
                    'wir-status=Invalidˉunsafeˉvalue'
                )) {
                Reject(
                    `Unsafe-type rejection ${Case.Name} differed.\n` +
                    Result.Diagnostic
                );
            }
            Rejected += 1;
        }
    }
    process.stdout.write(
        'native language 1 unsafe type surface status=Passed cases=10 ' +
        `valid=${Valid} rejected=${Rejected} opaque-identities=4\n`
    );
} finally {
    await Removeˉwork(Work);
}

async function Readˉeditionˉoneˉbody(Path, Label) {
    const Canonical = await realpath(Path);
    const Status = await lstat(Canonical);
    if (!Status.isFile() || Status.size <= 0 ||
        Status.size > MAXIMUM_MODULE_BYTES) {
        Reject(`The ${Label} is not a bounded ordinary file.`);
    }
    const Source = (await readFile(Canonical)).toString('utf8');
    const Descriptor = '#!wv/1 en@1\n';
    if (!Source.startsWith(Descriptor)) {
        Reject(`The ${Label} does not have the canonical edition-1 descriptor.`);
    }
    const Body = Source.slice(Descriptor.length);
    if (Buffer.byteLength(Body, 'utf8') <= 0) {
        Reject(`The ${Label} has an empty descriptor-free body.`);
    }
    return Body;
}

function Sourceˉset(Modules) {
    if (Modules.length < 1 || Modules.length > 8) {
        Reject('The unsafe-type source set module count is invalid.');
    }
    const Sources = Modules.map(Source => Buffer.from(Source, 'utf8'));
    const Headerˉbytes = 16 + Sources.length * 20;
    const Header = Buffer.alloc(Headerˉbytes);
    Header.write('WVSS', 0, 'ascii');
    Header.writeUInt16LE(2, 4);
    Header.writeUInt16LE(0, 6);
    Header.writeUInt32LE(Sources.length, 8);
    Header.writeUInt32LE(Sources.length * 20, 12);
    var Offset = Headerˉbytes;
    for (let Index = 0; Index < Sources.length; Index += 1) {
        const Entry = 16 + Index * 20;
        Header.writeUInt32LE(Offset, Entry);
        Header.writeUInt32LE(Sources[Index].length, Entry + 4);
        Header.writeUInt32LE(1, Entry + 8);
        Header.writeUInt32LE(1, Entry + 12);
        Header.writeUInt32LE(1, Entry + 16);
        Offset += Sources[Index].length;
    }
    return Buffer.concat([Header, ...Sources]);
}

function Runˉanalyzer(Arguments) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Child = spawn(Analyzer, Arguments, {
            cwd: Work,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
        });
        const Output = [];
        var Outputˉbytes = 0;
        var Exceeded = false;
        const Capture = Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= MAXIMUM_DIAGNOSTIC_BYTES) {
                Output.push(Chunk);
            } else {
                Exceeded = true;
                Child.kill();
            }
        };
        Child.stdout.on('data', Capture);
        Child.stderr.on('data', Capture);
        Child.once('error', Rejectˉpromise);
        const Timeout = setTimeout(() => {
            Exceeded = true;
            Child.kill();
        }, ANALYSIS_TIMEOUT_MILLISECONDS);
        Child.once('close', Code => {
            clearTimeout(Timeout);
            Resolveˉresult({
                Code,
                Diagnostic: Buffer.concat(Output).toString('utf8'),
                Exceeded,
            });
        });
    });
}

async function Removeˉwork(Path) {
    const Temporaryˉroot = await realpath(resolve(tmpdir()));
    const Parent = await realpath(dirname(Path));
    if (Parent !== Temporaryˉroot ||
        !basename(Path).startsWith('windvale-unsafe-types-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Path}`);
    }
    await rm(Path, {
        force: false,
        maxRetries: 2,
        recursive: true,
    });
}

function Asciiˉname(Name) {
    return Name.replaceAll('ˉ', '-').toLowerCase();
}

function Reject(Message) {
    throw new Error(Message);
}

function Usage() {
    process.stderr.write(
        `Usage: node ${basename(process.argv[1])} <wvanalyze.exe>\n`
    );
    process.exit(64);
}
