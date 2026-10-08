import { createHash } from 'node:crypto';
import { lstat, mkdir, mkdtemp, opendir, readFile, realpath, rename, rm, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉpreparedˉstagingˉcapacity, Packageˉwithˉstagingˉcapacity } from './Native-Staging-Capacity-Bridge-Core.mjs';
import {
    Getˉcurrentˉsplitˉcompilerˉfamily, Getˉcurrentˉsplitˉcompilerˉkey, Readˉpreparedˉsplitˉcompiler,
} from './Current-Split-Compiler-Cache-Core.mjs';

const SCRIPT = fileURLToPath(import.meta.url);
const NATIVE = path.dirname(SCRIPT);
const REPOSITORY = path.resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const HOST = `${process.platform}-${process.arch}`;
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const PREFIX = 'windvale-native-bootstrap-projection-';
const FORMAT = 'windvale-native-bootstrap-projection-2';
const CORE = 'Compiler/Windvale/Native-X64-Lowering-Core.wv';
const LAYOUT = 'Compiler/Windvale/Native-X64-Lowering-Layout.wv';
const WRITER = 'Compiler/Windvale/Native-X64-Lowering-Layout-Writer.wv';
const HISTORICAL_SERIALIZER_REVISION = 'ed37c35c8cf328f7797241eaac39aaae6190b312';
const ENTRY = 'Compiler/Windvale/Native-X64-Lowering-Staging-Tool.wv';
const PROJECT = 'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj';
const SUCCESSOR_PROJECT = 'Projects/Compiler/Windvale-Native-X64-Lowering.wvproj';
const LIBRARIES = new Set([
    'Libraries/Foundation/Bytes/Bytes.wv', 'Libraries/Foundation/Memory/Memory.wv',
    'Libraries/Foundation/Collections/Collections.wv', 'Libraries/Foundation/Values/Result.wv',
]);
const MAXIMUM_SNAPSHOT_BYTES = 16_777_216;
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_SOURCE_BYTES = 4_194_304;
const MAXIMUM_OBJECT_BYTES = 67_108_864;
const MAXIMUM_RESULT_BYTES = 4_194_304;
const MAXIMUM_DEADLINE_MILLISECONDS = 7_200_000;
const TOOLING_ROOTS = [
    'Bootstrap-Native-Compiler-Projection.mjs', 'Run-Split-Compiler.mjs',
    'Verify-Wvb.mjs', 'Build-Current-Split-Project-Wvb.mjs',
    'Build-Cached-Split-Project-Wvb.mjs', 'Build-Cached-Segmented-Hosted-Wvb.mjs',
    ...['Verify-Wvb', 'Package-Hosted-Wvb', 'Stage-Compiler-Wvb',
        'Link-Staged-Compiler-Wvo', 'Transport-Compiler-Image'].map(Name => Name + (WINDOWS ? '.cmd' : '.sh')),
];
const Hash = Bytes => createHash('sha256').update(Bytes).digest('hex');
const Text = Bytes => {
    const Value = Bytes.toString('utf8');
    Require(Buffer.from(Value).equals(Bytes) && !Value.includes('\r') &&
        !Value.startsWith('\ufeff'), 'Projection source must be exact LF UTF-8 without BOM.');
    return Value;
};
function Require(Condition, Message) { if (!Condition) throw new Error(Message); }
function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}
function Within(Root, Candidate) {
    const Relative = path.relative(Root, Candidate);
    return Relative !== '' && Relative !== '..' && !Relative.startsWith(`..${path.sep}`) &&
        !path.isAbsolute(Relative);
}
async function Directory(Place) {
    const Absolute = path.resolve(Place);
    const Information = await lstat(Absolute);
    Require(Information.isDirectory() && !Information.isSymbolicLink() &&
        Sameˉpath(await realpath(Absolute), Absolute), 'Noncanonical bootstrap directory: ' + Absolute);
    return Absolute;
}
async function Read(Place, Maximum = MAXIMUM_SOURCE_BYTES) {
    const Absolute = path.resolve(Place);
    await Directory(path.dirname(Absolute));
    const Information = await lstat(Absolute);
    Require(Information.isFile() && !Information.isSymbolicLink() && Information.nlink === 1 &&
        Information.size > 0 && Information.size <= Maximum &&
        Sameˉpath(await realpath(Absolute), Absolute), 'Nonordinary or oversized bootstrap input: ' + Absolute);
    const Bytes = await readFile(Absolute);
    Require(Bytes.length === Information.size, 'Bootstrap input changed during reading: ' + Absolute);
    return Bytes;
}
async function Writeˉnew(Root, Relative, Bytes) {
    Require(typeof Relative === 'string' && !Relative.includes('\\') &&
        !path.posix.isAbsolute(Relative) && Relative.split('/').every(Part =>
            Part !== '' && Part !== '.' && Part !== '..'), 'Invalid snapshot relative path.');
    const Destination = path.join(Root, ...Relative.split('/'));
    Require(Within(Root, Destination), 'Snapshot destination escapes its root.');
    await mkdir(path.dirname(Destination), { recursive: true });
    await Directory(path.dirname(Destination));
    await writeFile(Destination, Bytes, { flag: 'wx' });
}
function Unique(Value, Needle, Label) {
    const At = Value.indexOf(Needle);
    Require(At >= 0 && Value.indexOf(Needle, At + Needle.length) < 0,
        'Missing or repeated projection selector: ' + Label);
    return At;
}

// These are preimages of the selected adaptation, not hashes of backend code.
// A source change in an adapted region requires a reviewed selector amendment.
export function Projectˉnativeˉsources(Current, Historicalˉlayout) {
    Require(Current instanceof Map && [CORE, WRITER, ENTRY].every(Name => Current.has(Name)),
        'The projection requires its three selected current source owners.');
    const Projected = new Map(Current);
    const Changes = [];
    function Replace(File, Selector, Before, After) {
        const Value = Text(Projected.get(File));
        const At = Unique(Value, Before, Selector);
        Projected.set(File, Buffer.from(Value.slice(0, At) + After + Value.slice(At + Before.length)));
        Changes.push({ file: File, selector: Selector, beforeBytes: Buffer.byteLength(Before),
            beforeSha256: Hash(Before), afterBytes: Buffer.byteLength(After), afterSha256: Hash(After) });
    }
    function Region(File, Start, End, Expected, Selector) {
        const Value = Text(Projected.get(File));
        const At = Unique(Value, Start, Selector + ':start');
        const Stop = Value.indexOf(End, At + Start.length);
        Require(Stop > At, 'Missing projection region end: ' + Selector);
        const Before = Value.slice(At, Stop);
        Require(Hash(Before) === Expected, 'Changed projection preimage: ' + Selector);
        return Before;
    }
    const Historical = Text(Historicalˉlayout);
    const Historicalˉstart = Unique(Historical, 'export fn Compilerˉnativeˉx64ˉlayoutˉappend(', 'historical-append');
    const Historicalˉend = Unique(Historical, 'export fn Compilerˉnativeˉx64ˉlayoutˉmachineˉoffset(', 'historical-next');
    const Oldˉappend = Historical.slice(Historicalˉstart, Historicalˉend);
    Require(Hash(Oldˉappend) === 'd8f976c8573603bd200cd3d3a23d6a949eaeb56d5a0550ee2c43753e69915d26',
        'The selected recovery input does not contain the historical 76-byte serializer.');
    const Wrapper = Region(CORE, 'export fn Compilerˉnativeˉx64ˉplanˉbuild(',
        'fn Compilerˉnativeˉx64ˉplanˉinputsˉfailure(',
        'fde67a0e8a4116c9c87cd4552b381461744ce1eae833387515363092af4fa259', 'reserved-directory-wrapper');
    Replace(CORE, 'reserved-directory-wrapper', Wrapper, "export fn Compilerˉnativeˉx64ˉplanˉbuild(Input: borrow bytes)\n    -> Compilerˉnativeˉx64ˉplan {\n    let Context: Compilerˉnativeˉx64ˉplanˉinputs = Compilerˉnativeˉx64ˉplanˉinputsˉread(Input);\n    if Context.Status != Compilerˉnativeˉx64ˉstatus.Valid {\n        return Compilerˉnativeˉx64ˉplanˉfunctionˉfailure(\n            Input, Context.Status, Context.Failureˉfunction, Context.Failureˉdetail);\n    }\n    let Table: Compilerˉnativeˉx64ˉsignatureˉtable = Compilerˉnativeˉx64ˉsignatureˉtableˉbuild(\n        Input, Context);\n    if Table.Status != Compilerˉnativeˉx64ˉstatus.Valid {\n        return Compilerˉnativeˉx64ˉplanˉfunctionˉfailure(\n            Input, Table.Status, Table.Failureˉfunction, Table.Failureˉdetail);\n    }\n    return Compilerˉnativeˉx64ˉmachineˉdirectoryˉbuild(Input, Context, Table);\n}\n\n");
    for (const [Alias, Module] of [['Foundationˉbytes', 'Foundationˉbytes'],
        ['Foundationˉmemory', 'Foundationˉmemory'], ['Foundationˉresults', 'Foundationˉresult']]) {
        Replace(CORE, 'serializer-import-' + Alias, `import ${Module} as ${Alias};\n`, '');
    }
    Replace(CORE, "signatures-builder-parameter", "    Input: borrow bytes, Context: Compilerˉnativeˉx64ˉplanˉinputs,\n    Initialˉsignatures: Foundationˉbytes.Bytesˉbuilder\n", "    Input: borrow bytes, Context: Compilerˉnativeˉx64ˉplanˉinputs\n");
    Replace(CORE, "signatures-builder-store", "    var Signatureˉbuilder: Foundationˉbytes.Bytesˉbuilder = Initialˉsignatures;\n", "");
    Replace(CORE, "directory-builder-parameter", "    Table: Compilerˉnativeˉx64ˉsignatureˉtable, Initialˉdirectory: Foundationˉbytes.Bytesˉbuilder\n", "    Table: Compilerˉnativeˉx64ˉsignatureˉtable\n");
    Replace(CORE, "directory-builder-store", "    var Directoryˉbuilder: Foundationˉbytes.Bytesˉbuilder = Initialˉdirectory;\n", "");
    const Signatureˉanchor = '    var Functionˉcursors: bytes = Compilerˉnativeˉx64ˉempty(Input);\n';
    Replace(CORE, 'signatures-initialize', Signatureˉanchor,
        '    var Signatures: bytes = Compilerˉnativeˉx64ˉempty(Input);\n' + Signatureˉanchor);
    const Directoryˉanchor = '    var Relocationˉlengths: bytes = Compilerˉnativeˉx64ˉempty(Input);\n';
    Replace(CORE, 'directory-initialize', Directoryˉanchor,
        '    var Directory: bytes = Compilerˉnativeˉx64ˉempty(Input);\n' + Directoryˉanchor);
    for (const [Owner, Expected, End] of [
        ['Signature', '6d8366d2f1e7a1ada3b502be1a755b47cc5618e1d1fa7d11a87525f65e40e962',
            '        Expectedˉwvbˉcodeˉoffset ='],
        ['Directory', '94471967f3bcf7646b22d41b7b2b22913a27d0978f0ff2f446dfc350cc12e565',
            '        Expectedˉmachineˉcodeˉbytes ='],
    ]) {
        const Appended = Owner === 'Signature' ? 'Appended' : 'Directoryˉappended';
        const Start = `        let ${Appended} = Layoutˉwriter.Compilerˉnativeˉx64ˉlayoutˉappend(\n` +
            `            borrow mut ${Owner}ˉbuilder,\n`;
        const Block = Region(CORE, Start, End, Expected, Owner + '-append');
        const Arguments = Block.slice(Start.length, Block.indexOf('        );\n') + 11);
        const Borrowedˉargument = Owner === 'Signature'
            ? '            borrow Function.Parameterˉtypes' : '            borrow Bytesˉslice(';
        Unique(Arguments, Borrowedˉargument, Owner + '-append-parameter-borrow');
        const Historicalˉarguments = Arguments.replace(Borrowedˉargument, Borrowedˉargument.replace('borrow ', ''));
        const Variable = Owner === 'Signature' ? 'Signatures' : 'Directory';
        Replace(CORE, Owner + '-append', Block,
            `        ${Variable} = Layoutˉwriter.Compilerˉnativeˉx64ˉlayoutˉappend(\n` +
            `            ${Variable},\n` + Historicalˉarguments);
        Replace(CORE, Owner + '-freeze',
            `    let ${Variable}: bytes = Foundationˉbytes.Freeze(${Owner}ˉbuilder);\n`, '');
    }
    Replace(CORE, 'lower-budget-signature',
        'export fn Compilerˉlowerˉwvbˉnativeˉx64(\n    Input: borrow bytes,\n' +
        '    Budget: borrow mut Foundationˉmemory.Memoryˉbudget\n' +
        ') -> Compilerˉnativeˉx64ˉsummary effects(memory.allocate) {\n',
        'export fn Compilerˉlowerˉwvbˉnativeˉx64(Input: borrow bytes)\n' +
        '    -> Compilerˉnativeˉx64ˉsummary {\n');
    Replace(CORE, 'lower-bounded-budget-signature',
        'export fn Compilerˉlowerˉwvbˉnativeˉx64ˉbounded(\n    Input: borrow bytes,\n' +
        '    Budget: borrow mut Foundationˉmemory.Memoryˉbudget,\n' +
        '    Maximumˉobjectˉbytes: u32\n' +
        ') -> Compilerˉnativeˉx64ˉsummary effects(memory.allocate) {\n',
        'export fn Compilerˉlowerˉwvbˉnativeˉx64ˉbounded(\n    Input: borrow bytes,\n' +
        '    Maximumˉobjectˉbytes: u32\n' +
        ') -> Compilerˉnativeˉx64ˉsummary {\n');
    Replace(CORE, 'lower-bounded-budget-argument',
        '        Input, borrow mut Budget, 0u32\n',
        '        Input, 0u32\n');
    Replace(CORE, 'lower-plan-budget-argument',
        '        Compilerˉnativeˉx64ˉplanˉbuild(Input, borrow mut Budget);',
        '        Compilerˉnativeˉx64ˉplanˉbuild(Input);');
    const Writerˉsource = Text(Projected.get(WRITER));
    const Append = Writerˉsource.slice(Unique(Writerˉsource,
        'export fn Compilerˉnativeˉx64ˉlayoutˉappend(', 'layout-writer-append'));
    Require(Hash(Append) === '0d5f600719f01e78f4fa407afe1596fea12daa3bea6104429084064306ddb40f',
        'Changed projection preimage: layout-writer-append');
    const Parameterˉlimit = 'Compilerˉnativeˉx64ˉlayoutˉparameterˉlimit()';
    Unique(Oldˉappend, Parameterˉlimit, 'historical-writer-layout-import');
    Replace(WRITER, 'layout-append', Append, Oldˉappend.replace(
        Parameterˉlimit, 'Layout.' + Parameterˉlimit));
    for (const Line of ['import Foundationˉbytes as Bytes;\n',
        'import Foundationˉmemory as Memory;\n', 'import Foundationˉresult as Results;\n']) {
        Replace(WRITER, 'layout-import-' + Line.trim(), Line, '');
    }
    Replace(ENTRY, 'staging-entry-import', 'import Foundationˉmemory as Memory;\n', '');
    Replace(ENTRY, 'staging-budget-entry',
        'export fn Main(Budget: Memory.Memoryˉbudget) -> i32 effects(memory.allocate) {\n' +
        '    var Parent: Memory.Memoryˉbudget = Budget;\n', 'export fn Main() -> i32 {\n');
    Replace(ENTRY, 'staging-plan-budget-argument',
        '        Nativeˉx64.Compilerˉnativeˉx64ˉplanˉbuild(borrow Input, borrow mut Parent);',
        '        Nativeˉx64.Compilerˉnativeˉx64ˉplanˉbuild(borrow Input);');
    Require(Changes.length === 25 && new Set(Changes.map(Change => Change.selector)).size === 25,
        'The selected projection must contain exactly 25 distinct reviewed adaptations.');
    for (const [Name, Bytes] of Projected) {
        if (![CORE, WRITER, ENTRY].includes(Name)) {
            Require(Bytes.equals(Current.get(Name)), 'Unselected source changed: ' + Name);
        }
        if (Name.endsWith('.wv') && !LIBRARIES.has(Name)) {
            const Source = Text(Bytes).replace(/"(?:[^"\\]|\\.)*"|\/\/[^\n]*|\/\*[\s\S]*?\*\//gu, '');
            Require(!/Foundationˉbytes\.|Bytes\.Appendˉ|Bytes\.Constructˉreserved|Bytes\.Freeze|Bytesˉbuilder|Slice\s*</u.test(Source),
                'An actual new source API remains in the projection: ' + Name);
        }
    }
    return { Sources: Projected, Changes, Historicalˉappendˉsha256: Hash(Oldˉappend) };
}

async function Run(Step, Tool, Arguments, Deadline, Stream = true) {
    process.stdout.write(`native bootstrap projection step=${Step} status=Started\n`);
    const Result = await Runˉdevelopmentˉcommand(Tool, Arguments, Deadline, Stream, 1_048_576);
    Require(Result.Code === 0 && Result.Error === '', `${Step} failed (${Result.Code}): ${Result.Output}${Result.Error}`);
    process.stdout.write(`native bootstrap projection step=${Step} status=Complete\n`);
    return Result.Output;
}
async function Inventory(Reader, Project, Deadline) {
    const Report = await Run('project-inventory', Reader, ['--inventory', Project], Deadline, false);
    const Value = JSON.parse(Report.trim());
    Require(Value.inventoryVersion === 1 && Value.projectVersion === 4 &&
        Array.isArray(Value.sources) && Value.sources.length > 0 && Value.sources.length <= 64 &&
        typeof Value.sourceInputLockSha256 === 'string' && /^[0-9a-f]{64}$/u.test(Value.sourceInputLockSha256),
    'Unsupported authenticated project inventory.');
    for (const Name of [Value.sourceInputLock, Value.sourceProfile, Value.targetDescriptor, ...Value.sources]) {
        Require(typeof Name === 'string' && !Name.includes('\\') && !path.posix.isAbsolute(Name) &&
            Name.split('/').every(Part => Part !== '' && Part !== '.' && Part !== '..'), 'Invalid inventory path.');
    }
    Require(new Set(Value.sources).size === Value.sources.length, 'Repeated project source.');
    return Value;
}
async function Record(Work, Value) {
    const Temporary = path.join(Work, 'Projection.json.new');
    await writeFile(Temporary, JSON.stringify(Value, null, 2) + '\n', { flag: 'wx' });
    await rename(Temporary, path.join(Work, 'Projection.json'));
}
function Projectionˉmanifest(Bytes) {
    let Value = Text(Bytes);
    Require(Value.startsWith('windvale-project 4\n') &&
        Value.split('\n').filter(Line => Line.startsWith('root ')).join('\n') === `root "${ENTRY}"`,
    'Changed staging project root.');
    for (const Name of LIBRARIES) {
        const Line = `source "${Name}"\n`;
        const At = Unique(Value, Line, 'unused-canonical-module-' + Name);
        Value = Value.slice(0, At) + Value.slice(At + Line.length);
    }
    return Buffer.from(Value);
}
async function Toolingˉclosure() {
    const Queue = TOOLING_ROOTS.map(Name => path.join(NATIVE, Name));
    const Seen = new Set(), Values = [];
    let Total = 0;
    while (Queue.length > 0) {
        const File = Queue.shift();
        if (Seen.has(File)) continue;
        Require(Within(NATIVE, File) && Seen.size < 64, 'Unbounded native tooling closure.');
        Seen.add(File);
        const Bytes = await Read(File, 1_048_576);
        Total += Bytes.length;
        Require(Total <= MAXIMUM_SNAPSHOT_BYTES, 'Native tooling closure exceeds 16 MiB.');
        Values.push({ path: File, bytes: Bytes.length, sha256: Hash(Bytes) });
        if (File.endsWith('.mjs')) {
            for (const Match of Bytes.toString('utf8').matchAll(/\bfrom\s+['"](\.\/?[^'"]+)['"]/gu)) {
                const Imported = path.resolve(path.dirname(File), Match[1]);
                Require(Imported.endsWith('.mjs'), 'Unsupported native tooling import.');
                Queue.push(Imported);
            }
        }
    }
    return Values;
}
async function Prepare(Options, Compiler) {
    const Capacity = await Readˉpreparedˉstagingˉcapacity(Options.get('--capacity-record'),
        Options.get('--capacity-sha256'), Options.get('--compiler-checkpoint'), Options.Deadline);
    const Parent = await Directory(Options.get('--workspace-parent'));
    Require(!Within(REPOSITORY, Parent) && !Sameˉpath(Parent, REPOSITORY), 'Projection must be outside the active repository.');
    const Work = await realpath(await mkdtemp(path.join(Parent, PREFIX)));
    const Currentˉworkspace = path.join(Work, 'Current');
    const Projectionˉworkspace = path.join(Work, 'Projection');
    await mkdir(Currentˉworkspace);
    await mkdir(Projectionˉworkspace);
    const Inventories = await Promise.all([PROJECT, SUCCESSOR_PROJECT].map(Name =>
        Inventory(path.join(Compiler.directory, 'Reader' + SUFFIX), path.join(REPOSITORY, Name), Options.Deadline)));
    Require(Inventories[0].sources[0] === ENTRY &&
        Inventories[1].sources[0] === 'Compiler/Windvale/Native-X64-Lowering-Memory-Adapter.wv',
    'The selected project roots changed.');
    const Names = new Set(['Windvale.wvws', PROJECT, SUCCESSOR_PROJECT]);
    for (const Item of Inventories) {
        for (const Name of [Item.sourceInputLock, Item.sourceProfile, Item.targetDescriptor, ...Item.sources]) Names.add(Name);
    }
    Require(Names.size <= 128, 'Bootstrap closure exceeds 128 files.');
    const Current = new Map();
    let Total = 0;
    for (const Name of [...Names].sort()) {
        const Bytes = await Read(path.join(REPOSITORY, Name));
        Total += Bytes.length;
        Require(Total <= MAXIMUM_SNAPSHOT_BYTES, 'Bootstrap closure exceeds 16 MiB.');
        Current.set(Name, Bytes);
    }
    await Compiler.Requireˉunchanged();
    const Revision = (await Run('current-source-revision', 'git', ['rev-parse', 'HEAD'], Options.Deadline, false)).trim();
    Require(/^[0-9a-f]{40}$/u.test(Revision), 'Invalid current source revision.');
    // Exactly one recovery source is retrieved at the selected immutable revision.
    const Historical = Buffer.from(await Run('historical-layout', 'git',
        ['show', HISTORICAL_SERIALIZER_REVISION + ':' + LAYOUT], Options.Deadline, false));
    const Projectionˉinput = new Map(Inventories[0].sources.map(Name => [Name, Current.get(Name)]));
    const Projected = Projectˉnativeˉsources(Projectionˉinput, Historical);
    const Retainedˉsources = Inventories[0].sources.filter(Name => !LIBRARIES.has(Name));
    for (const [Name, Bytes] of Current) await Writeˉnew(Currentˉworkspace, Name, Bytes);
    const Projectionˉfiles = new Map(Retainedˉsources.map(Name => [Name, Projected.Sources.get(Name)]));
    for (const Name of ['Windvale.wvws', Inventories[0].sourceInputLock,
        Inventories[0].sourceProfile, Inventories[0].targetDescriptor]) Projectionˉfiles.set(Name, Current.get(Name));
    Projectionˉfiles.set(PROJECT, Projectionˉmanifest(Current.get(PROJECT)));
    for (const [Name, Bytes] of Projectionˉfiles) await Writeˉnew(Projectionˉworkspace, Name, Bytes);
    await Writeˉnew(Work, 'Historical-Layout.wv', Historical);
    const Files = (Entries, Workspace) => [...Entries].map(([Name, Bytes]) => ({
        workspace: Workspace, path: Name, bytes: Bytes.length, sha256: Hash(Bytes),
    }));
    const Value = { format: FORMAT, host: HOST, work: Work, repository: REPOSITORY,
        compilerCheckpoint: Options.get('--compiler-checkpoint'), sourceRevision: Revision,
        capacityBridge: { path: Capacity.Path, sha256: Capacity.Sha256, key: Capacity.Record.key },
        historicalRevision: HISTORICAL_SERIALIZER_REVISION,
        historicalFileSha256: Hash(Historical), historicalAppendSha256: Projected.Historicalˉappendˉsha256,
        coordinatorSha256: Hash(await Read(SCRIPT)), status: 'Prepared', phase: 'source-projection',
        qualification: false, changes: Projected.Changes, inventories: Inventories,
        tooling: await Toolingˉclosure(), nodeVersion: process.version,
        files: [...Files(Current, 'Current'), ...Files(Projectionˉfiles, 'Projection')], products: [],
        projectionProject: PROJECT, successorProject: SUCCESSOR_PROJECT };
    await Record(Work, Value);
    await Requireˉsnapshot(Value, true);
    await Capacity.Requireˉunchanged();
    process.stdout.write(`native bootstrap projection status=Prepared workspace=${Work} compiler-construction=disabled\n`);
}
async function Load(Place) {
    const Work = await Directory(Place);
    Require(path.basename(Work).startsWith(PREFIX) && !Within(REPOSITORY, Work) &&
        !Sameˉpath(Work, REPOSITORY), 'Unowned projection workspace name or location.');
    const Value = JSON.parse((await Read(path.join(Work, 'Projection.json'), 262_144)).toString('utf8'));
    Require(Value.format === FORMAT && Value.host === HOST && Sameˉpath(Value.work, Work) &&
        Sameˉpath(Value.repository, REPOSITORY) && Value.coordinatorSha256 === Hash(await Read(SCRIPT)) &&
        /^[0-9a-f]{40}$/u.test(Value.sourceRevision) &&
        Value.historicalRevision === HISTORICAL_SERIALIZER_REVISION &&
        Array.isArray(Value.files) && Value.files.length <= 256 &&
        Array.isArray(Value.products) && Value.products.length <= 1024 &&
        Array.isArray(Value.inventories) && Value.inventories.length === 2 &&
        Array.isArray(Value.tooling) && Value.tooling.length > 0 && Value.tooling.length <= 64 &&
        Value.nodeVersion === process.version &&
        Value.projectionProject === PROJECT && Value.successorProject === SUCCESSOR_PROJECT,
    'Unsupported or changed projection record.');
    return Value;
}
async function Requireˉsnapshot(Value, Liveˉsource = false) {
    const Tools = await Toolingˉclosure();
    Require(JSON.stringify(Tools) === JSON.stringify(Value.tooling), 'Bootstrap tooling closure changed.');
    let Total = 0;
    for (const Item of [...Value.files, ...Value.products]) {
        Require(['Current', 'Projection', 'Products'].includes(Item.workspace) &&
            typeof Item.path === 'string' && !Item.path.includes('\\') && !path.posix.isAbsolute(Item.path) &&
            Item.path.split('/').every(Part => Part !== '' && Part !== '.' && Part !== '..') &&
            Number.isSafeInteger(Item.bytes) && Item.bytes > 0 && /^[0-9a-f]{64}$/u.test(Item.sha256),
        'Malformed snapshot file identity.');
        const Bytes = await Read(path.join(Value.work, Item.workspace, Item.path), MAXIMUM_OBJECT_BYTES);
        Require(Bytes.length === Item.bytes && Hash(Bytes) === Item.sha256, 'Snapshot file changed: ' + Item.path);
        if (Item.workspace !== 'Products') Total += Bytes.length;
        if (Liveˉsource && Item.workspace === 'Current') {
            Require(Bytes.equals(await Read(path.join(REPOSITORY, Item.path))), 'Current source changed: ' + Item.path);
        }
    }
    Require(Total <= MAXIMUM_SNAPSHOT_BYTES * 2, 'Oversized combined bootstrap snapshot.');
    const Historical = await Read(path.join(Value.work, 'Historical-Layout.wv'));
    Require(Hash(Historical) === Value.historicalFileSha256, 'Historical serializer snapshot changed.');
    const Current = new Map();
    for (const Item of Value.files.filter(Item => Item.workspace === 'Current' && Item.path.endsWith('.wv'))) {
        Current.set(Item.path, await Read(path.join(Value.work, 'Current', Item.path)));
    }
    // Reconstruct the exact selector result; stored file hashes alone do not
    // authorize an altered manifest or an edited machine-emission source.
    const Input = new Map(Value.files.filter(Item => Item.workspace === 'Projection' && Item.path.endsWith('.wv'))
        .map(Item => [Item.path, Current.get(Item.path)]));
    const Expected = Projectˉnativeˉsources(Input, Historical);
    Require(JSON.stringify(Expected.Changes) === JSON.stringify(Value.changes), 'Projection selector record changed.');
    for (const [Name, Bytes] of Expected.Sources) {
        Require(Bytes.equals(await Read(path.join(Value.work, 'Projection', Name))), 'Projection adaptation changed: ' + Name);
    }
    Require(Projectionˉmanifest(await Read(path.join(Value.work, 'Current', PROJECT)))
        .equals(await Read(path.join(Value.work, 'Projection', PROJECT))), 'Projected manifest changed.');
    const Retained = Value.inventories[0].sources.filter(Name => !LIBRARIES.has(Name));
    const Expectedˉfiles = new Set([...Retained, 'Windvale.wvws', PROJECT,
        Value.inventories[0].sourceInputLock, Value.inventories[0].sourceProfile, Value.inventories[0].targetDescriptor]);
    const Actualˉfiles = Value.files.filter(Item => Item.workspace === 'Projection');
    Require(Expectedˉfiles.size === Actualˉfiles.length &&
        Actualˉfiles.every(Item => Expectedˉfiles.has(Item.path)), 'Projected source closure changed.');
    for (const Name of Expectedˉfiles) {
        if (Name !== PROJECT && !Name.endsWith('.wv')) {
            Require((await Read(path.join(Value.work, 'Current', Name)))
                .equals(await Read(path.join(Value.work, 'Projection', Name))), 'Projected admission input changed.');
        }
    }
}
async function Product(Value, Relative) {
    const Bytes = await Read(path.join(Value.work, 'Products', Relative), MAXIMUM_OBJECT_BYTES);
    Require(!Value.products.some(Item => Item.path === Relative), 'Repeated product identity.');
    Value.products.push({ workspace: 'Products', path: Relative, bytes: Bytes.length, sha256: Hash(Bytes) });
}
// Prepared consumers reuse the exact construction snapshot; selection never
// constructs a compiler or authorizes source by its Git revision alone.
export async function Readˉpreparedˉnativeˉprojection(Recordˉpath, Sha256, Compilerˉkey, Deadline) {
    Require(typeof Recordˉpath === 'string' && /^[0-9a-f]{64}$/u.test(Sha256) &&
        /^[0-9a-f]{64}$/u.test(Compilerˉkey), 'Invalid prepared projection selection.');
    Recordˉpath = path.resolve(Recordˉpath);
    Require(path.basename(Recordˉpath) === 'Projection.json', 'Unexpected projection record name.');
    const Original = await Read(Recordˉpath, 262_144);
    Require(Hash(Original) === Sha256, 'Prepared projection record identity differs.');
    const Value = await Load(path.dirname(Recordˉpath));
    Require(typeof Value.capacityBridge?.path === 'string' && /^[0-9a-f]{64}$/u.test(Value.capacityBridge.sha256),
        'Prepared projection omitted its separately prepared 2048-function staging pair.');
    const Capacity = await Readˉpreparedˉstagingˉcapacity(Value.capacityBridge.path,
        Value.capacityBridge.sha256, Compilerˉkey, Deadline);
    Require(Capacity.Record.key === Value.capacityBridge.key, 'Prepared projection capacity pair request differs.');
    Require(Value.status === 'AwaitingSuccessorAndQualification' &&
        Value.compilerCheckpoint === Compilerˉkey, 'Projection does not use the selected current compiler.');
    const Names = new Set(['Windvale.wvws', PROJECT, SUCCESSOR_PROJECT]);
    for (const [Index, Item] of Value.inventories.entries()) {
        Require(Item.inventoryVersion === 1 && Item.projectVersion === 4 &&
            Array.isArray(Item.sources) && Item.sources.length > 0 && Item.sources.length <= 64 &&
            Item.sources[0] === (Index === 0 ? ENTRY : 'Compiler/Windvale/Native-X64-Lowering-Memory-Adapter.wv') &&
            new Set(Item.sources).size === Item.sources.length, 'Prepared project inventory differs.');
        for (const Name of [Item.sourceInputLock, Item.sourceProfile, Item.targetDescriptor, ...Item.sources]) {
            Require(typeof Name === 'string' && !Name.includes('\\') && !path.posix.isAbsolute(Name) &&
                Name.split('/').every(Part => Part !== '' && Part !== '.' && Part !== '..'),
            'Malformed prepared project inventory path.');
            Names.add(Name);
        }
    }
    const Current = Value.files.filter(Item => Item.workspace === 'Current');
    Require(Names.size <= 128 && Current.length === Names.size &&
        Current.every(Item => Names.has(Item.path)) &&
        new Set([...Value.files, ...Value.products].map(Item => Item.workspace + '/' + Item.path)).size ===
            Value.files.length + Value.products.length, 'Prepared projection input inventory differs.');
    await Requireˉsnapshot(Value, true);
    return { Record: Value, Sha256, Requireˉunchanged: async () => {
        Require((await Read(Recordˉpath, 262_144)).equals(Original), 'Prepared projection record changed.');
        await Requireˉsnapshot(Value, true);
        await Capacity.Requireˉunchanged();
    } };
}
async function Admitˉwvb(Place, Projected, Deadline) {
    const Bytes = await Read(Place, MAXIMUM_WVB_BYTES);
    Require(Bytes.length >= 12 && Bytes.toString('ascii', 0, 4) === 'WVB1' && Bytes.readUInt16LE(4) === 1 &&
        (Projected ? Bytes.readUInt16LE(6) >= 11 && Bytes.readUInt16LE(6) <= 43 : (Bytes.readUInt16LE(6) === 44 || Bytes.readUInt16LE(6) === 45)),
    'The selected projected/successor WVB version differs.');
    // Complete admission parses every shape and instruction. Its older-minor
    // refusal proves absence of tags39/40/41 and ops229..241 at parsed positions.
    // Literal bytes and immediates are deliberately never searched for opcodes.
    await Run('complete-wvb-admission', process.execPath,
        [path.join(NATIVE, 'Verify-Wvb.mjs'), '--current', Place], Deadline);
}
async function Construct(Options, Compiler) {
    const Value = await Load(Options.get('--workspace'));
    Require(Value.status === 'Prepared' && Value.compilerCheckpoint === Options.get('--compiler-checkpoint'),
        'Construct requires the exact prepared, not partially constructed, projection.');
    await Requireˉsnapshot(Value, true);
    const Capacity = await Readˉpreparedˉstagingˉcapacity(Value.capacityBridge?.path,
        Value.capacityBridge?.sha256, Options.get('--compiler-checkpoint'), Options.Deadline);
    Require(Capacity.Record.key === Value.capacityBridge.key, 'Projection capacity pair request differs.');
    const Products = path.join(Value.work, 'Products');
    await mkdir(Products);
    async function Phase(Name, Action) {
        Value.phase = Name; Value.status = 'Constructing'; await Record(Value.work, Value);
        await Compiler.Requireˉunchanged();
        await Action();
        await Requireˉsnapshot(Value, true);
        await Compiler.Requireˉunchanged();
        await Record(Value.work, Value);
    }
    async function Step(Name, Tool, Arguments) {
        await Phase(Name, () => Run(Name, Tool, Arguments, Options.Deadline));
    }
    const Sourceˉarguments = (Workspace, Project, Output) => [path.join(NATIVE, 'Run-Split-Compiler.mjs'),
        ...['Admitter', 'Authenticator', 'Analyzer', 'Emitter'].map(Name => path.join(Compiler.directory, Name + SUFFIX)),
        '--foreign-binder', path.join(Compiler.directory, 'Binder' + SUFFIX),
        '--workspace', path.join(Value.work, Workspace, 'Windvale.wvws'),
        '--project', path.join(Value.work, Workspace, Project),
        '--manifest-reader', path.join(Compiler.directory, 'Reader' + SUFFIX), Output];
    // Refuse the actual compiler's bytecode before constructing its temporary
    // stager or packaging native code. Source compilation needs only the six.
    const Successorˉwvb = path.join(Products, 'Successor.wvb');
    await Step('true-successor-source', process.execPath, Sourceˉarguments('Current', SUCCESSOR_PROJECT, Successorˉwvb));
    await Phase('true-successor-wvb-admission', async () => {
        await Admitˉwvb(Successorˉwvb, false, Options.Deadline);
        await Product(Value, 'Successor.wvb');
    });
    const Projectedˉwvb = path.join(Products, 'Projected-Stager.wvb');
    await Step('projected-source', process.execPath, Sourceˉarguments('Projection', PROJECT, Projectedˉwvb));
    await Phase('projected-wvb-admission', async () => {
        await Admitˉwvb(Projectedˉwvb, true, Options.Deadline);
        await Product(Value, 'Projected-Stager.wvb');
    });
    const Stager = path.join(Products, 'Projected-Stager' + SUFFIX);
    await Phase('capacity-native-package', async () => {
        // Compiler-scale serializer consumers exhaust profile 7's instruction bound.
        Value.capacityPackage = await Packageˉwithˉstagingˉcapacity(Capacity, Projectedˉwvb, Stager, '8', Options.Deadline);
    });
    await Product(Value, 'Projected-Stager' + SUFFIX);
    await Step('true-successor-object', Stager, [Successorˉwvb,
        path.join(Products, 'Successor-Object'), path.join(Products, 'Successor-Object.wvop')]);
    const Object = await Readˉobject(Products, 'Successor-Object');
    await Product(Value, 'Successor-Object.wvop');
    for (let Index = 0; Index < Object.Chunks; Index += 1) await Product(Value, `Successor-Object.chunk-${Index}`);
    Value.objectBytes = Object.Bytes;
    Value.status = 'AwaitingSuccessorAndQualification'; Value.phase = 'true-successor-object';
    Value.successorEntry = 'capability-free-Main-borrow-bytes-budget-to-bytes';
    Value.byteResultLimit = MAXIMUM_RESULT_BYTES; Value.qualification = false;
    await Record(Value.work, Value);
    await Requireˉsnapshot(Value, true);
    process.stdout.write(`native bootstrap projection status=${Value.status} object-bytes=${Object.Bytes} ` +
        `byte-result-fit=${Object.Bytes <= MAXIMUM_RESULT_BYTES} qualification=false workspace=${Value.work}\n`);
}
async function Readˉobject(Place, Prefixˉname) {
    const Manifest = await Read(path.join(Place, Prefixˉname + '.wvop'), 6_240);
    Require(Manifest.length >= 24 && Manifest.toString('ascii', 0, 4) === 'WVOP' &&
        Manifest.readUInt32LE(4) === 1 && Manifest.readUInt32LE(8) === Manifest.length,
    'Invalid staged object manifest header.');
    const Bytes = Manifest.readUInt32LE(12), Chunks = Manifest.readUInt32LE(16);
    Require(Bytes > 0 && Bytes <= MAXIMUM_OBJECT_BYTES && Chunks > 0 && Chunks <= 518 &&
        Manifest.length === 24 + Chunks * 12 && Manifest.readUInt32LE(20) === MAXIMUM_RESULT_BYTES,
    'Invalid staged object manifest geometry.');
    const Hashˉstate = createHash('sha256');
    let Position = 0;
    for (let Index = 0; Index < Chunks; Index += 1) {
        const At = 24 + Index * 12, Length = Manifest.readUInt32LE(At + 8);
        Require(Manifest.readUInt32LE(At) === Index && Manifest.readUInt32LE(At + 4) === Position &&
            Length > 0 && Length <= MAXIMUM_RESULT_BYTES && Length <= Bytes - Position,
        'Invalid staged object chunk extent.');
        const Chunk = await Read(path.join(Place, `${Prefixˉname}.chunk-${Index}`), MAXIMUM_RESULT_BYTES);
        Require(Chunk.length === Length, 'Staged object chunk length differs.');
        Hashˉstate.update(Chunk); Position += Length;
    }
    Require(Position === Bytes && await lstat(path.join(Place, `${Prefixˉname}.chunk-${Chunks}`))
        .then(() => false, Error => { if (Error.code === 'ENOENT') return true; throw Error; }),
    'Staged object is incomplete or has a trailing chunk.');
    return { Bytes, Chunks, Sha256: Hashˉstate.digest('hex') };
}
async function Requireˉretirementˉinventory(Value) {
    const Allowed = new Set(['Projection.json', 'Historical-Layout.wv',
        'Products/True-Successor-Comparison.wvo',
        ...[...Value.files, ...Value.products].map(Item => Item.workspace + '/' + Item.path)]);
    const Queue = [Value.work];
    let Entries = 0;
    while (Queue.length > 0) {
        const Place = Queue.shift();
        await Directory(Place);
        for await (const Entry of await opendir(Place)) {
            Require(++Entries <= 4096 && !Entry.isSymbolicLink(), 'Unbounded or linked retirement inventory.');
            const Absolute = path.join(Place, Entry.name);
            const Relative = path.relative(Value.work, Absolute).split(path.sep).join('/');
            Require(Within(Value.work, Absolute), 'Retirement entry escapes workspace.');
            if (Entry.isDirectory()) {
                Require([...Allowed].some(Name => Name.startsWith(Relative + '/')), 'Unowned retirement directory.');
                Queue.push(Absolute);
            } else Require(Entry.isFile() && Allowed.has(Relative), 'Unowned retirement file: ' + Relative);
        }
    }
}
async function Compareˉretire(Options, Compiler) {
    const Value = await Load(Options.get('--workspace'));
    Require(Value.status === 'AwaitingSuccessorAndQualification' &&
        Value.compilerCheckpoint === Options.get('--compiler-checkpoint'), 'Successor construction is incomplete.');
    await Requireˉsnapshot(Value, true);
    await Compiler.Requireˉunchanged();
    const Driver = path.resolve(Options.get('--successor-driver'));
    const Driverˉbytes = await Read(Driver, 134_217_728);
    Require(Hash(Driverˉbytes) === Options.get('--driver-sha256'), 'Successor driver identity differs.');
    const Qualification = path.resolve(Options.get('--qualification-record'));
    const Qualificationˉbytes = await Read(Qualification, 1_048_576);
    Require(Hash(Qualificationˉbytes) === Options.get('--qualification-sha256'), 'Qualification record identity differs.');
    const Output = path.resolve(Options.get('--retirement-record'));
    Require(!Within(Value.work, Output) && !Sameˉpath(Output, Value.work), 'Retirement record must survive snapshot removal.');
    await Directory(path.dirname(Output));
    Require(await lstat(Output).then(() => false, Error => { if (Error.code === 'ENOENT') return true; throw Error; }),
        'Retirement record already exists.');
    const Products = path.join(Value.work, 'Products');
    const Successorˉwvb = path.join(Products, 'Successor.wvb');
    const Wvb = await Read(Successorˉwvb, MAXIMUM_WVB_BYTES);
    Require(Hash(Wvb) === Options.get('--successor-wvb-sha256'), 'Driver source WVB selection differs.');
    await Admitˉwvb(Successorˉwvb, false, Options.Deadline);
    const Projected = await Readˉobject(Products, 'Successor-Object');
    Require(Projected.Bytes <= MAXIMUM_RESULT_BYTES, 'True byte-result bridge requires a bounded staged protocol for this object.');
    const Candidate = path.join(Products, 'True-Successor-Comparison.wvo');
    Require(!Within(Value.work, Driver) && !Within(Value.work, Qualification),
        'Successor driver and qualification evidence must survive retirement.');
    Value.status = 'Comparing'; Value.phase = 'true-successor-comparison';
    await Record(Value.work, Value);
    // The caller-selected real ABI25 driver owns executing the exact successor,
    // validating/copying its result while live, and closing its resource domain.
    const Arguments = [Successorˉwvb, Candidate];
    if (Driver.endsWith('.mjs')) {
        await Run('true-successor-comparison', process.execPath, [Driver, ...Arguments], Options.Deadline);
    } else if (!WINDOWS && Driver.endsWith('.sh')) {
        await Run('true-successor-comparison', 'bash', [Driver, ...Arguments], Options.Deadline);
    } else await Run('true-successor-comparison', Driver, Arguments, Options.Deadline);
    const Actual = await Read(Candidate, MAXIMUM_RESULT_BYTES);
    Require(Actual.length === Projected.Bytes && Hash(Actual) === Projected.Sha256,
        'Projected and true successor native object bytes differ.');
    let Compared = 0;
    for (let Index = 0; Index < Projected.Chunks; Index += 1) {
        const Chunk = await Read(path.join(Products, `Successor-Object.chunk-${Index}`), MAXIMUM_RESULT_BYTES);
        Require(Chunk.equals(Actual.subarray(Compared, Compared + Chunk.length)),
            'Projected and true successor object chunk bytes differ.');
        Compared += Chunk.length;
    }
    Require(Compared === Actual.length, 'Object comparison did not cover every byte.');
    Require(Driverˉbytes.equals(await Read(Driver, 134_217_728)) &&
        Qualificationˉbytes.equals(await Read(Qualification, 1_048_576)), 'External successor/qualification identity changed.');
    await Requireˉsnapshot(Value, true); await Compiler.Requireˉunchanged();
    Value.status = 'Compared'; Value.phase = 'exact-object-comparison';
    await Record(Value.work, Value);
    const Retirement = { format: FORMAT, status: 'Compared', host: HOST,
        projection: Value, successorWvbSha256: Hash(Wvb),
        driver: { path: Driver, bytes: Driverˉbytes.length, sha256: Hash(Driverˉbytes) },
        qualificationRecord: { path: Qualification, bytes: Qualificationˉbytes.length, sha256: Hash(Qualificationˉbytes) },
        comparison: { bytes: Actual.length, sha256: Hash(Actual), exactEqual: true },
        qualificationInterpretedByCoordinator: false, retiredWorkspace: Value.work };
    // Native semantics and host qualification remain the existing owners' proof;
    // this record proves only selector identity and same-input object equality.
    await Requireˉretirementˉinventory(Value);
    await writeFile(Output, JSON.stringify(Retirement, null, 2) + '\n', { flag: 'wx' });
    const Parent = await Directory(path.dirname(Value.work));
    Require(path.dirname(Value.work) === Parent && path.basename(Value.work).startsWith(PREFIX) &&
        Sameˉpath(await Directory(Value.work), Value.work), 'Refusing unowned recursive retirement.');
    await rm(Value.work, { recursive: true, force: false });
    process.stdout.write(`native bootstrap projection status=Retired comparison=Exact qualification-record=${Qualification}\n`);
}
export function Parseˉprojectionˉarguments(Arguments, Now = Date.now()) {
    Require(Array.isArray(Arguments) && Arguments.length >= 7 && Arguments.length <= 21 &&
        Arguments.every(Value => typeof Value === 'string' && Value.length > 0 && Value.length <= 32_768), 'Invalid bootstrap arguments.');
    const Action = Arguments[0];
    Require(['prepare', 'construct', 'compare-retire'].includes(Action) && Arguments.length % 2 === 1, 'Invalid bootstrap action.');
    const Allowed = new Set(['--compiler-checkpoint', '--deadline-ms',
        ...(Action === 'prepare' ? ['--workspace-parent', '--capacity-record', '--capacity-sha256'] : ['--workspace']),
        ...(Action === 'compare-retire' ? ['--successor-driver', '--driver-sha256', '--successor-wvb-sha256',
            '--qualification-record', '--qualification-sha256', '--retirement-record'] : [])]);
    const Options = new Map();
    for (let Index = 1; Index < Arguments.length; Index += 2) {
        Require(Allowed.has(Arguments[Index]) && !Options.has(Arguments[Index]), 'Unknown or repeated bootstrap option.');
        Options.set(Arguments[Index], Arguments[Index + 1]);
    }
    Require(Options.size === Allowed.size && [...Allowed].every(Name => Options.has(Name)) &&
        /^[0-9a-f]{64}$/u.test(Options.get('--compiler-checkpoint')) &&
        /^[1-9][0-9]*$/u.test(Options.get('--deadline-ms')), 'Missing or malformed bootstrap selection.');
    const Deadline = Number(Options.get('--deadline-ms'));
    Require(Number.isSafeInteger(Deadline) && Deadline > Now &&
        Deadline <= Now + MAXIMUM_DEADLINE_MILLISECONDS, 'Bootstrap deadline must be finite and within two hours.');
    for (const Name of ['--driver-sha256', '--successor-wvb-sha256', '--qualification-sha256', '--capacity-sha256']) {
        if (Options.has(Name)) Require(/^[0-9a-f]{64}$/u.test(Options.get(Name)), 'Malformed external identity.');
    }
    Options.Action = Action; Options.Deadline = Deadline;
    return Options;
}
async function Main() {
    const Options = Parseˉprojectionˉarguments(process.argv.slice(2));
    Require(process.arch === 'x64' && ['win32', 'linux'].includes(process.platform), 'Unsupported bootstrap host.');
    Require(await Getˉcurrentˉsplitˉcompilerˉkey() === Options.get('--compiler-checkpoint'),
        'The selected prepared compiler does not match current compiler construction inputs.');
    const Compiler = await Readˉpreparedˉsplitˉcompiler(await Getˉcurrentˉsplitˉcompilerˉfamily(),
        Options.get('--compiler-checkpoint'));
    Require(Date.now() < Options.Deadline, 'Bootstrap deadline expired while checking prepared inputs.');
    if (Options.Action === 'prepare') await Prepare(Options, Compiler);
    if (Options.Action === 'construct') await Construct(Options, Compiler);
    if (Options.Action === 'compare-retire') await Compareˉretire(Options, Compiler);
}
if (process.argv[1] && pathToFileURL(path.resolve(process.argv[1])).href === import.meta.url) {
    try { await Main(); }
    catch (Error) {
        process.stderr.write(String(Error.message).slice(-4_096) + '\n');
        process.exitCode = Error.exitCode ?? 1;
    }
}
