import { createHash } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { lstat, mkdir, mkdtemp, open, readFile, realpath, rename, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

// A temporary, source-built capacity step in the existing segmented pipeline.
// It does not reconstruct current six, select a fallback, or replace the pins.
const SELF = fileURLToPath(import.meta.url);
const NATIVE = path.dirname(SELF);
const REPOSITORY = path.resolve(NATIVE, '../..');
const WINDOWS = process.platform === 'win32';
const HOST = `${process.platform}-${process.arch}`;
const TARGET = WINDOWS ? 'windows' : 'linux';
const HOST_FAMILY = TARGET + '-x64';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const WRAPPER = WINDOWS ? '.cmd' : '.sh';
const FORMAT = 'windvale-native-staging-capacity-bridge-1';
const NAMESPACE = 'native-staging-capacity-bridge-v1';
const REVISION = 'b3e5b8e5721d126d60f3f6ce7c28a866e90f59ae';
const TREE = '2eec50f542baeebd7fbeca2178078eaa59108f83';
const STAGER_PROJECT = 'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj';
const LINKER_PROJECT = 'Projects/Linker/Windvale-Compiler-Image-Staging.wvproj';
const VERIFIER_PROJECT = 'Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj';
const LAYOUT = 'Compiler/Windvale/Native-X64-Lowering-Layout.wv';
const PREFIX = 'windvale-native-staging-capacity-';
const MAXIMUM_FILES = 512;
const MAXIMUM_BYTES = 536_870_912;
const MAXIMUM_IMAGE = 67_108_864;
const MAXIMUM_NODE = 134_217_728;
const MAXIMUM_SOURCE = 4_194_304;
const MAXIMUM_RECORD = 1_048_576;
const PRODUCT_NAMES = ['Bridge.wvb', 'Bridge' + SUFFIX, 'Linker.wvb', 'Linker' + SUFFIX];
const ADMITTED_PAIRS = new WeakSet();
const COMMAND_CONTRACTS = [
    'Build-Current-Split-Project-Wvb.mjs exact-current-key prepared-products-only current-verifier-wvb',
    'Build-Cached-Segmented-Hosted-Wvb.mjs prepared-products-only profile2 current-verifier-image',
    'Run-Split-Compiler.mjs authenticated-project4 exact-prepared-current-six historical-one-selector',
    'exact-prepared-official-current-verifier Bridge.wvb',
    'Build-Cached-Segmented-Hosted-Wvb.mjs pinned-producers profile8 Bridge.wvb',
    'Run-Split-Compiler.mjs authenticated-project4 exact-prepared-current-six current-linker',
    'exact-prepared-official-current-verifier Linker.wvb',
    'Build-Cached-Segmented-Hosted-Wvb.mjs pinned-producers profile8 Linker.wvb',
];
const SELECTOR_BEFORE = 'export fn Compilerˉnativeˉx64ˉlayoutˉfunctionˉlimit() -> u32 {\n    return 1024u32;\n}';
const SELECTOR_AFTER = SELECTOR_BEFORE.replace('1024u32', '2048u32');
const COMPILER_NAMES = ['Analyzer' + SUFFIX, 'Analyzer.identity', 'Emitter' + SUFFIX, 'Emitter.identity',
    'Reader' + SUFFIX, 'Admitter' + SUFFIX, 'Authenticator' + SUFFIX, 'Binder' + SUFFIX, 'Checkpoint.json'];
const TOOL_ROOTS = ['Native-Staging-Capacity-Bridge-Core.mjs', 'Run-Split-Compiler.mjs',
    'Verify-Wvb.mjs', 'Build-Cached-Segmented-Hosted-Wvb.mjs', 'Build-Current-Split-Project-Wvb.mjs',
    ...['Package-Hosted-Wvb', 'Stage-Compiler-Wvb', 'Link-Staged-Compiler-Wvo', 'Transport-Compiler-Image',
        'Verify-Wvb', 'Package-Segmented-Compiler-Wvb'].map(Name => Name + WRAPPER)];
const PIN_NAMES = ['Wvo-Staging-Producer.wvb', ...['wvstage', 'wvlinkstage', 'wvimagetransport']
    .map(Name => HOST_FAMILY + '-' + Name + SUFFIX)];
const Hash = Bytes => createHash('sha256').update(Bytes).digest('hex');
const Measure = Bytes => ({ bytes: Bytes.length, sha256: Hash(Bytes) });
const LOADED_SELF = Measure(await readFile(SELF));
let Loadedˉtools = null;
const Same = (A, B) => WINDOWS ? A.toLowerCase() === B.toLowerCase() : A === B;
const Equal = (A, B) => JSON.stringify(A) === JSON.stringify(B);
const Require = (Value, Message) => { if (!Value) throw new Error(Message); };
const Missing = Message => Object.assign(new Error(Message), { exitCode: 64 });
const Within = (Root, File) => { const Relative = path.relative(Root, File); return Relative !== '' &&
    Relative !== '..' && !Relative.startsWith('..' + path.sep) && !path.isAbsolute(Relative); };
function Time(Deadline) {
    if (!Number.isSafeInteger(Deadline) || Date.now() >= Deadline) {
        throw Object.assign(new Error('Staging-capacity deadline expired.'), { exitCode: 124 });
    }
}
function Relative(Value) {
    Require(typeof Value === 'string' && !path.posix.isAbsolute(Value) && !Value.includes('\\') &&
        !Value.includes(':') && Value.split('/').every(Part => Part !== '' && Part !== '.' && Part !== '..'),
    'Noncanonical staging-capacity relative path.');
    return Value;
}
async function Directory(Value, Create = false) {
    Value = path.resolve(Value);
    let Parent = path.parse(Value).root;
    for (const Part of Value.slice(Parent.length).split(path.sep).filter(Boolean)) {
        Parent = path.join(Parent, Part);
        if (Create) await mkdir(Parent).catch(Error => { if (Error.code !== 'EEXIST') throw Error; });
        const Stat = await lstat(Parent);
        Require(Stat.isDirectory() && !Stat.isSymbolicLink(), 'Capacity directory contains a link or non-directory.');
    }
    Require(Same(await realpath(Value), Value), 'Capacity directory is not canonical.');
    return Value;
}
async function Evidence(File, Deadline, Maximum = MAXIMUM_IMAGE, Executable = false) {
    Time(Deadline); File = path.resolve(File); await Directory(path.dirname(File));
    const Stat = await lstat(File);
    Require(Stat.isFile() && !Stat.isSymbolicLink() && Stat.size > 0 && Stat.size <= Maximum &&
        Same(await realpath(File), File) && (!Executable || WINDOWS || (Stat.mode & 0o111) !== 0),
    'Capacity input is not a bounded ordinary file: ' + File);
    let Count = 0; const Digest = createHash('sha256');
    for await (const Chunk of createReadStream(File, { highWaterMark: 1_048_576 })) {
        Time(Deadline); Count += Chunk.length; Require(Count <= Stat.size, 'Capacity input grew while hashed.'); Digest.update(Chunk);
    }
    Require(Count === Stat.size, 'Capacity input changed while hashed.');
    return { path: File, bytes: Count, sha256: Digest.digest('hex') };
}
async function Payload(File, Deadline, Maximum) {
    const Before = await Evidence(File, Deadline, Maximum);
    const Handle = await open(File, 'r');
    try {
        const Stat = await Handle.stat(); const Named = await lstat(File);
        Require(Stat.isFile() && Named.isFile() && !Named.isSymbolicLink() && Stat.size === Before.bytes &&
            Stat.dev === Named.dev && Stat.ino === Named.ino, 'Capacity input identity changed before bounded read.');
        const Buffer = globalThis.Buffer.alloc(Before.bytes + 1); let Count = 0;
        while (Count < Buffer.length) {
            Time(Deadline);
            const Part = await Handle.read(Buffer, Count, Buffer.length - Count, Count);
            if (Part.bytesRead === 0) break; Count += Part.bytesRead;
        }
        Require(Count === Before.bytes, 'Capacity input extent changed during bounded read.');
        const Bytes = Buffer.subarray(0, Count);
        Require(Equal(Measure(Bytes), { bytes: Before.bytes, sha256: Before.sha256 }), 'Capacity input changed while read.');
        return Bytes;
    } finally { await Handle.close(); }
}
function Text(Bytes) {
    const Value = Bytes.toString('utf8');
    Require(Buffer.from(Value).equals(Bytes) && !Value.includes('\r') && !Value.startsWith('\ufeff'),
        'Capacity source must be exact LF UTF-8.');
    return Value;
}
export function Adaptˉhistoricalˉlayout(Bytes) {
    const Before = SELECTOR_BEFORE; const After = SELECTOR_AFTER;
    const Original = Text(Bytes); const At = Original.indexOf(Before);
    Require(At >= 0 && Original.indexOf(Before, At + Before.length) < 0 && !Original.includes(After),
        'Historical capacity selector is absent, repeated or already changed.');
    const Adapted = Buffer.from(Original.slice(0, At) + After + Original.slice(At + Before.length));
    Require(Text(Adapted).replace(After, Before) === Original, 'Historical capacity inverse differs.');
    return { Bytes: Adapted, Selector: { name: 'native-function-capacity', before: Measure(Buffer.from(Before)),
        after: Measure(Buffer.from(After)) } };
}
function Projectˉpaths(Bytes, Name, Sourceˉcount) {
    const Value = Text(Bytes); Require(Value.startsWith('windvale-project 4\n'), 'Capacity requires authenticated Project4.');
    const Sources = [...Value.matchAll(/^(root|source) "([^"\r\n]+)"$/gmu)].map(M => Relative(M[2]));
    Require(Sources.length === Sourceˉcount && new Set(Sources).size === Sources.length &&
        [...Value.matchAll(/^root /gmu)].length === 1, 'Capacity project source inventory differs.');
    const Admission = [...Value.matchAll(/^(source-input-lock|source-profile|target-descriptor) "([^"\r\n]+)"$/gmu)]
        .map(M => Relative(M[2]));
    Require(Admission.length === 3 && new Set(Admission).size === 3, 'Capacity admission inventory differs.');
    return { Sources, Paths: [...new Set(['Windvale.wvws', Name, ...Sources, ...Admission])].sort() };
}
async function Modules(Deadline) {
    Time(Deadline);
    if (Loadedˉtools === null) {
        const Snapshot = await Tooling(Deadline);
        const Self = Snapshot.find(Value => Same(Value.path, SELF));
        Require(Self !== undefined && Equal({ bytes: Self.bytes, sha256: Self.sha256 }, LOADED_SELF),
            'Loaded capacity owner differs from its recorded source.');
        Loadedˉtools = Snapshot;
    }
    return { Compiler: await import('./Current-Split-Compiler-Cache-Core.mjs'),
        Segmented: await import('./Build-Cached-Segmented-Hosted-Wvb.mjs'),
        Hosted: await import('./Native-Hosted-Application-Cache-Core.mjs'),
        Keys: await import('./Native-Project-Cache-Key-Core.mjs'),
        Commands: await import('./Development-Command-Core.mjs') };
}
async function Run(Label, Command, Arguments, Deadline, Maximum = 900_000, Preparedˉonly = false, Quiet = false) {
    Time(Deadline - 30_000); const { Commands } = await Modules(Deadline);
    if (!Quiet) console.log(`staging capacity step=${Label} status=Started`);
    const Previous = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    let Result;
    try {
        if (Preparedˉonly) process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        Result = await Commands.Runˉdevelopmentˉcommand(Command, Arguments,
            Math.min(Deadline - 30_000, Date.now() + Maximum), Command !== 'git', 1_048_576);
    } finally {
        if (Previous === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previous;
    }
    if (Result.Code !== 0 || Result.Error !== '') {
        throw Object.assign(new Error(`Capacity ${Label} failed exit=${Result.Code}: ${(Result.Error || Result.Output).slice(-4_096)}`),
            { exitCode: Result.Code || 2 });
    }
    Time(Deadline); if (!Quiet) console.log(`staging capacity step=${Label} status=Complete`); return Result.Output;
}
async function Tooling(Deadline) {
    const Queue = TOOL_ROOTS.map(Name => path.join(NATIVE, Name)); const Seen = new Set(); const Values = [];
    let Bytes = 0;
    while (Queue.length !== 0) {
        const File = Queue.shift(); if (Seen.has(File)) continue;
        Require(Within(NATIVE, File) && Seen.size < 128, 'Capacity tool closure exceeds128 or leaves Native.'); Seen.add(File);
        const Value = await Payload(File, Deadline, 1_048_576); Bytes += Value.length;
        Require(Bytes <= 16_777_216, 'Capacity tool source closure exceeds16MiB.'); Values.push({ path: File, ...Measure(Value) });
        if (File.endsWith('.mjs')) {
            for (const Match of Text(Value).matchAll(/(?:\bfrom\s*|\bimport\s*\()(['"])(\.\/[^'"]+\.mjs)\1/gu)) {
                Queue.push(path.resolve(path.dirname(File), Match[2]));
            }
        }
    }
    return Values.sort((A, B) => A.path.localeCompare(B.path, 'en'));
}
async function Hostedˉfields(Hosted, Deadline) {
    Time(Deadline);
    const Context = await Hosted.Prepareˉhostedˉapplicationˉcontext(TARGET, path.join(NATIVE, 'Package-Hosted-Wvb' + WRAPPER));
    const Values = Context.producerFields.map(Field => ({ label: Field.label, ...Measure(Field.bytes) }));
    Context.producerFields.length = 0; Time(Deadline); return Values;
}
async function Verifierˉrequest(Keys) {
    const Context = await Keys.Prepareˉnativeˉprojectˉcacheˉcontext('current-wvb-verification-v1',
        [path.join(NATIVE, 'Verify-Wvb.mjs'), path.join(NATIVE, 'Development-Command-Core.mjs')]);
    return Keys.Getˉnativeˉprojectˉcacheˉrequest(Context, path.join(REPOSITORY, VERIFIER_PROJECT));
}
async function Inputs(Compiler, Deadline, Admission = null) {
    const Tools = await Tooling(Deadline);
    Require(Loadedˉtools !== null && Equal(Tools, Loadedˉtools), 'Loaded capacity producer closure changed.');
    const Values = [...Tools, await Evidence(process.execPath, Deadline, MAXIMUM_NODE, true)];
    for (const Name of COMPILER_NAMES) Values.push(await Evidence(path.join(Compiler.directory, Name), Deadline));
    for (const Name of PIN_NAMES) Values.push(await Evidence(path.join(REPOSITORY,
        'Artifacts/Native-Segmented-Compiler-Toolset-Candidate', Name), Deadline));
    if (Admission !== null) {
        const { Keys } = await Modules(Deadline); const Request = await Verifierˉrequest(Keys); Time(Deadline);
        Require(Request.key === Admission.requestKey, 'Current verifier source request changed.');
        Values.push(...Request.inputEvidence, Request.context.workspaceEvidence, ...Request.context.producerEvidence,
            await Evidence(Admission.wvb.path, Deadline, 16_777_216), await Evidence(Admission.image.path, Deadline, MAXIMUM_IMAGE, true));
    }
    const Unique = new Map();
    for (const Value of Values) {
        const Name = WINDOWS ? Value.path.toLowerCase() : Value.path;
        if (Unique.has(Name)) {
            const Prior = Unique.get(Name);
            Require(Prior.bytes === Value.bytes && Prior.sha256 === Value.sha256 && Same(Prior.path, Value.path),
                'Conflicting duplicate capacity input.');
        } else { Unique.set(Name, Value); }
    }
    const Result = [...Unique.values()].map(Value => ({ path: Value.path, bytes: Value.bytes, sha256: Value.sha256 }));
    Require(Result.length <= MAXIMUM_FILES && Result.reduce((N, V) => N + V.bytes, 0) <= MAXIMUM_BYTES,
        'Capacity input closure exceeds512files/512MiB.');
    return Result;
}
async function Unchanged(Values, Deadline) {
    for (const Value of Values) {
        Require(Equal(await Evidence(Value.path, Deadline, Math.max(Value.bytes, 1)), Value), 'Capacity input changed: ' + Value.path);
    }
}
function Identityˉkey(Value) {
    return Hash(Buffer.from(JSON.stringify({ format: FORMAT, host: Value.host, compilerCheckpoint: Value.compilerCheckpoint,
        historicalRevision: Value.historicalRevision, historicalTree: Value.historicalTree, nodeVersion: Value.nodeVersion,
        capacity: Value.capacity, profiles: Value.profiles, selector: Value.selector,
        inputs: Value.inputs.map(V => ({ ...V, path: Within(Value.work, V.path) ? path.relative(Value.work, V.path).split(path.sep).join('/') : V.path })),
        admission: { requestKey: Value.admission?.requestKey, wvb: Value.admission?.wvb.sha256, image: Value.admission?.image.sha256 },
        hostedProducers: Value.hostedProducers, sources: Value.sources.map(V => ({ workspace: V.workspace, path: V.path,
            bytes: V.bytes, sha256: V.sha256, gitObject: V.gitObject })), commands: Value.commandContracts })));
}
export function Requireˉcapacityˉrecordˉshape(Value, Compilerˉkey) {
    Require(Value?.format === FORMAT && Value.status === 'Prepared' && Value.host === HOST &&
        Value.compilerCheckpoint === Compilerˉkey && Value.historicalRevision === REVISION && Value.historicalTree === TREE &&
        Value.nodeVersion === process.version && Value.capacity === 2048 &&
        Equal(Value.profiles, { bridge: '8', linker: '8' }) && Value.qualified === false &&
        /^[0-9a-f]{64}$/u.test(Value.key) && Array.isArray(Value.sources) && Value.sources.length <= 128 &&
        Array.isArray(Value.inputs) && Value.inputs.length > 0 && Value.inputs.length <= MAXIMUM_FILES &&
        Array.isArray(Value.products) && Value.products.length === PRODUCT_NAMES.length &&
        Equal(Value.products.map(V => V.name), PRODUCT_NAMES) && Value.admission?.profile === '2' &&
        /^[0-9a-f]{64}$/u.test(Value.admission.requestKey) && Equal(Value.commandContracts, COMMAND_CONTRACTS) &&
        Equal(Value.selector, { name: 'native-function-capacity', before: Measure(Buffer.from(SELECTOR_BEFORE)),
            after: Measure(Buffer.from(SELECTOR_AFTER)) }) && Value.key === Identityˉkey(Value),
    'Unsupported, incomplete or stale prepared capacity pair.');
    Require(Value.inputs.every(Item => Item && Object.keys(Item).sort().join(',') === 'bytes,path,sha256' &&
        typeof Item.path === 'string' && path.isAbsolute(Item.path) && Same(path.resolve(Item.path), Item.path) &&
        Number.isSafeInteger(Item.bytes) && Item.bytes > 0 &&
        Item.bytes <= (Same(Item.path, process.execPath) ? MAXIMUM_NODE : MAXIMUM_IMAGE) && /^[0-9a-f]{64}$/u.test(Item.sha256)) &&
        Value.sources.every(Item => Item && Number.isSafeInteger(Item.bytes) && Item.bytes > 0 && Item.bytes <= MAXIMUM_SOURCE &&
            /^[0-9a-f]{64}$/u.test(Item.sha256)) && Value.products.every(Item => Number.isSafeInteger(Item.bytes) && Item.bytes > 0 &&
            Item.bytes <= (Item.name.endsWith('.wvb') ? 16_777_216 : MAXIMUM_IMAGE) && /^[0-9a-f]{64}$/u.test(Item.sha256)),
    'Malformed capacity input/source/product identity.');
}
async function Snapshotˉguards(Value, Deadline, Historical = true) {
    let Total = 0; const Seen = new Set();
    for (const Source of Value.sources) {
        Require(['Historical', 'Bridge', 'CurrentLinker'].includes(Source.workspace), 'Unknown capacity source workspace.');
        const Name = Source.workspace + '/' + Relative(Source.path); Require(!Seen.has(Name), 'Duplicate capacity source.'); Seen.add(Name);
        Total += Source.bytes; Require(Total <= 16_777_216, 'Capacity snapshot exceeds16MiB.');
        const File = path.join(Value.work, Source.workspace, ...Source.path.split('/'));
        Require(Equal(Measure(await Payload(File, Deadline, Math.max(Source.bytes, 1))),
            { bytes: Source.bytes, sha256: Source.sha256 }), 'Capacity source snapshot changed: ' + Name);
        if (Source.workspace === 'CurrentLinker') {
            Require(Equal(Measure(await Payload(path.join(REPOSITORY, ...Source.path.split('/')), Deadline, Math.max(Source.bytes, 1))),
                { bytes: Source.bytes, sha256: Source.sha256 }), 'Current linker construction input changed.');
        }
    }
    const Historicalˉproject = await Payload(path.join(Value.work, 'Historical', STAGER_PROJECT), Deadline, 65_536);
    const Originalˉpaths = Projectˉpaths(Historicalˉproject, STAGER_PROJECT, 41).Paths;
    const Currentˉproject = await Payload(path.join(Value.work, 'CurrentLinker', LINKER_PROJECT), Deadline, 65_536);
    const Currentˉpaths = Projectˉpaths(Currentˉproject, LINKER_PROJECT, 11).Paths;
    Require(Equal([...Seen].sort(), [...Originalˉpaths.flatMap(P => ['Historical/' + P, 'Bridge/' + P]),
        ...Currentˉpaths.map(P => 'CurrentLinker/' + P)].sort()), 'Capacity source closure is incomplete or broadened.');
    for (const Name of Originalˉpaths) {
        const Original = await Payload(path.join(Value.work, 'Historical', Name), Deadline, MAXIMUM_SOURCE);
        const Bridge = await Payload(path.join(Value.work, 'Bridge', Name), Deadline, MAXIMUM_SOURCE);
        const Expected = Name === LAYOUT ? Adaptˉhistoricalˉlayout(Original).Bytes : Original;
        Require(Bridge.equals(Expected), 'Historical capacity adaptation is not the exact one-selector inverse.');
    }
    if (Historical) {
        const Inventory = await Run('historical-object-guards', 'git', ['-C', REPOSITORY, 'ls-tree', '-rlz', REVISION,
            '--', ...Originalˉpaths], Deadline, 120_000, false, true);
        const Rows = Inventory.split('\0').filter(Boolean).map(Line => /^(100644|100755) blob ([0-9a-f]{40})\s+(\d+)\t(.+)$/u.exec(Line));
        Require(Rows.length === Originalˉpaths.length && Rows.every(R => R !== null), 'Historical Git inventory differs.');
        for (const Row of Rows) {
            const Bytes = await Payload(path.join(Value.work, 'Historical', Row[4]), Deadline, MAXIMUM_SOURCE);
            Require(Bytes.length === Number(Row[3]) && createHash('sha1').update(Buffer.from(`blob ${Bytes.length}\0`)).update(Bytes)
                .digest('hex') === Row[2], 'Historical snapshot does not match its selected Git object.');
        }
    }
}
export async function Readˉpreparedˉstagingˉcapacity(Recordˉpath, Sha256, Compilerˉkey, Deadline) {
    try {
        Time(Deadline); Require(process.arch === 'x64' && ['win32', 'linux'].includes(process.platform) &&
            /^[0-9a-f]{64}$/u.test(Sha256) && /^[0-9a-f]{64}$/u.test(Compilerˉkey), 'Malformed capacity selection/host.');
        Recordˉpath = path.resolve(Recordˉpath); Require(path.basename(Recordˉpath) === 'Staging-Capacity-Bridge.json', 'Wrong capacity record name.');
        const Original = await Payload(Recordˉpath, Deadline, MAXIMUM_RECORD); Require(Hash(Original) === Sha256, 'Capacity record digest differs.');
        const Value = JSON.parse(Original); Requireˉcapacityˉrecordˉshape(Value, Compilerˉkey);
        Require(Same(await Directory(Value.work), path.dirname(Recordˉpath)) && path.basename(Value.work).startsWith(PREFIX) &&
            !Within(REPOSITORY, Value.work) && !Same(REPOSITORY, Value.work), 'Capacity record work is not private.');
        const { Compiler: Core, Hosted } = await Modules(Deadline);
        Require(await Core.Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey, 'Capacity pair is not built with the current prepared compiler.');
        const Compiler = await Core.Readˉpreparedˉsplitˉcompiler(await Core.Getˉcurrentˉsplitˉcompilerˉfamily(), Compilerˉkey);
        Time(Deadline);
        const Requireˉunchanged = async () => {
            Time(Deadline); Require((await Payload(Recordˉpath, Deadline, MAXIMUM_RECORD)).equals(Original), 'Capacity record changed.');
            Require(await Core.Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey, 'Current compiler inputs changed.'); Time(Deadline);
            Require(Equal(await Inputs(Compiler, Deadline, Value.admission), Value.inputs), 'Capacity tool/compiler/pin/verifier closure differs.');
            await Snapshotˉguards(Value, Deadline);
            Require(Equal(await Hostedˉfields(Hosted, Deadline), Value.hostedProducers), 'Capacity hosted packager closure differs.');
            for (const Product of Value.products) {
                Require(Same(Product.path, path.join(Value.work, 'Products', Product.name)), 'Capacity product escapes exact work.');
                const Actual = await Evidence(Product.path, Deadline,
                    Product.name.endsWith('.wvb') ? 16_777_216 : MAXIMUM_IMAGE, Product.name.endsWith(SUFFIX));
                Require(Equal(Actual, { path: Product.path, bytes: Product.bytes, sha256: Product.sha256 }), 'Capacity product changed.');
            }
        };
        await Requireˉunchanged();
        Freezeˉrecord(Value);
        const Pair = Object.freeze({ Record: Value, Path: Recordˉpath, Sha256, Requireˉunchanged,
            Stager: Value.products[1], Linker: Value.products[3], Admission: Value.admission });
        ADMITTED_PAIRS.add(Pair); return Pair;
    } catch (Error) {
        if (Error.exitCode === 124) throw Error;
        throw Missing('Prepared 2048-function staging pair unavailable: ' + Error.message +
            '. Run Native-Staging-Capacity-Bridge-Core.mjs prepare with the exact current checkpoint in separate preparation.');
    }
}
function Freezeˉrecord(Value) {
    const Pending = [Value]; let Count = 0;
    while (Pending.length !== 0) {
        const Item = Pending.pop();
        if (Item === null || typeof Item !== 'object' || Object.isFrozen(Item)) continue;
        Require(++Count <= 8192, 'Capacity record object count exceeds8192.');
        Pending.push(...Object.values(Item)); Object.freeze(Item);
    }
    return Value;
}
async function Save(Work, Value) {
    const Temporary = path.join(Work, 'Staging-Capacity-Bridge.json.new');
    await writeFile(Temporary, JSON.stringify(Value, null, 2) + '\n');
    await rename(Temporary, path.join(Work, 'Staging-Capacity-Bridge.json'));
}
export async function Prepareˉstagingˉcapacity(Compilerˉkey, Parent, Deadline) {
    Time(Deadline); Require(process.arch === 'x64' && ['win32', 'linux'].includes(process.platform), 'Unsupported capacity host.');
    Require(process.env.WINDVALE_PREPARED_COMPILER_ONLY === undefined || process.env.WINDVALE_PREPARED_COMPILER_ONLY === '1',
        'WINDVALE_PREPARED_COMPILER_ONLY must be absent or1.');
    Require(process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === undefined,
        'Capacity preparation refuses inherited WINDVALE_PREPARED_PRODUCTS_ONLY; behavior must remain construction-free.');
    const { Compiler: Core, Hosted } = await Modules(Deadline);
    Require(await Core.Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey, 'Capacity preparation requires the exact current key.');
    const Compiler = await Core.Readˉpreparedˉsplitˉcompiler(await Core.Getˉcurrentˉsplitˉcompilerˉfamily(), Compilerˉkey);
    Time(Deadline);
    Parent = await Directory(Parent); Require(!Within(REPOSITORY, Parent) && !Same(Parent, REPOSITORY), 'Capacity parent must be outside repository.');
    const Work = await Directory(await mkdtemp(path.join(Parent, PREFIX))); const Sources = [];
    const Value = { format: FORMAT, status: 'Preparing', host: HOST, work: Work, compilerCheckpoint: Compilerˉkey,
        historicalRevision: REVISION, historicalTree: TREE, nodeVersion: process.version, capacity: 2048,
        profiles: { bridge: '8', linker: '8' }, qualified: false, products: [], sources: Sources, commands: [],
        commandContracts: COMMAND_CONTRACTS };
    Value.inputs = await Inputs(Compiler, Deadline); Value.hostedProducers = await Hostedˉfields(Hosted, Deadline);
    async function Step(Label, Command, Arguments, Maximum = 900_000, Bulkˉsnapshot = false, Preparedˉonly = false) {
        if (!Bulkˉsnapshot) { Require(await Core.Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey, 'Current compiler inputs changed.'); await Unchanged(Value.inputs, Deadline); }
        Value.phase = Label; Value.commands.push({ step: Label, command: [Command, ...Arguments] }); await Save(Work, Value);
        const Output = await Run(Label, Command, Arguments, Deadline, Maximum, Preparedˉonly);
        if (!Bulkˉsnapshot) { Require(await Core.Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey, 'Current compiler inputs changed.'); await Unchanged(Value.inputs, Deadline); } return Output;
    }
    try {
        await Compiler.Requireˉunchanged(); await Unchanged(Value.inputs, Deadline);
        Require((await Step('historical-tree', 'git', ['-C', REPOSITORY, 'rev-parse', REVISION + '^{tree}'], 120_000, true)).trim() === TREE,
            'Historical capacity source tree differs.');
        const Project = Buffer.from(await Step('historical-project', 'git', ['-C', REPOSITORY, 'cat-file', 'blob', REVISION + ':' + STAGER_PROJECT], 120_000, true));
        const Historicalˉpaths = Projectˉpaths(Project, STAGER_PROJECT, 41).Paths;
        const Inventory = await Step('historical-inventory', 'git', ['-C', REPOSITORY, 'ls-tree', '-rlz', REVISION, '--', ...Historicalˉpaths], 120_000, true);
        const Objects = new Map(Inventory.split('\0').filter(Boolean).map(Line => {
            const Match = /^(100644|100755) blob ([0-9a-f]{40})\s+(\d+)\t(.+)$/u.exec(Line);
            Require(Match !== null, 'Malformed historical capacity Git inventory.'); return [Relative(Match[4]), { id: Match[2], bytes: Number(Match[3]) }];
        }));
        Require(Objects.size === Historicalˉpaths.length, 'Historical capacity source closure is incomplete.');
        let Sourceˉbytes = 16 + 8 * 41;
        for (const [Index, Name] of Historicalˉpaths.entries()) {
            const Original = Buffer.from(await Step('historical-source-' + (Index + 1), 'git', ['-C', REPOSITORY, 'cat-file', 'blob', Objects.get(Name).id], 120_000, true));
            Text(Original); Require(Original.length === Objects.get(Name).bytes, 'Git source line endings/extent changed.');
            const Adapted = Name === LAYOUT ? Adaptˉhistoricalˉlayout(Original) : { Bytes: Original };
            if (Name === LAYOUT) Value.selector = Adapted.Selector;
            if (Name.endsWith('.wv')) Sourceˉbytes += Adapted.Bytes.length;
            Require(Sourceˉbytes <= MAXIMUM_SOURCE, 'Historical capacity source set exceeds4MiB.');
            for (const [Workspace, Bytes] of [['Historical', Original], ['Bridge', Adapted.Bytes]]) {
                const File = path.join(Work, Workspace, ...Name.split('/')); await Directory(path.dirname(File), true); await writeFile(File, Bytes, { flag: 'wx' });
                Sources.push({ workspace: Workspace, path: Name, ...Measure(Bytes), gitObject: Objects.get(Name).id });
            }
        }
        const Linkerˉmanifest = await Payload(path.join(REPOSITORY, LINKER_PROJECT), Deadline, 65_536);
        for (const Name of Projectˉpaths(Linkerˉmanifest, LINKER_PROJECT, 11).Paths) {
            const Bytes = await Payload(path.join(REPOSITORY, Name), Deadline, MAXIMUM_SOURCE);
            const File = path.join(Work, 'CurrentLinker', Name); await Directory(path.dirname(File), true); await writeFile(File, Bytes, { flag: 'wx' });
            Sources.push({ workspace: 'CurrentLinker', path: Name, ...Measure(Bytes) });
        }
        await Snapshotˉguards(Value, Deadline); await Compiler.Requireˉunchanged(); await Unchanged(Value.inputs, Deadline);
        await Directory(path.join(Work, 'Products'), true);
        const { Keys } = await Modules(Deadline); const Admissionˉrequest = await Verifierˉrequest(Keys); Time(Deadline);
        const Verifierˉwvb = path.join(Work, 'Products', 'Verifier.wvb'); const Verifier = path.join(Work, 'Products', 'Verifier' + SUFFIX);
        await Step('prepared-current-verifier-wvb', process.execPath, [path.join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs'),
            '--compiler-checkpoint', Compilerˉkey, '--prepared-compiler-only', '--deadline-ms', String(Deadline - 30_000),
            path.join(REPOSITORY, VERIFIER_PROJECT), Verifierˉwvb], 120_000, false, true);
        await Step('prepared-current-verifier-image', process.execPath, [path.join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
            '--deadline-ms', String(Math.min(Deadline - 30_000, Date.now() + 120_000)), '2', Verifierˉwvb, Verifier], 120_000, false, true);
        Value.admission = { profile: '2', requestKey: Admissionˉrequest.key,
            wvb: await Evidence(Verifierˉwvb, Deadline, 16_777_216), image: await Evidence(Verifier, Deadline, MAXIMUM_IMAGE, true) };
        Value.inputs = await Inputs(Compiler, Deadline, Value.admission); Value.key = Identityˉkey(Value); await Save(Work, Value);
        for (const [Workspace, Name, Base] of [['Bridge', STAGER_PROJECT, 'Bridge'], ['CurrentLinker', LINKER_PROJECT, 'Linker']]) {
            const Wvb = path.join(Work, 'Products', Base + '.wvb'); const Product = path.join(Work, 'Products', Base + SUFFIX);
            await Step(Base + '-source', process.execPath, [path.join(NATIVE, 'Run-Split-Compiler.mjs'),
                ...['Admitter', 'Authenticator', 'Analyzer', 'Emitter'].map(Role => path.join(Compiler.directory, Role + SUFFIX)),
                '--foreign-binder', path.join(Compiler.directory, 'Binder' + SUFFIX), '--workspace', path.join(Work, Workspace, 'Windvale.wvws'),
                '--project', path.join(Work, Workspace, Name), '--manifest-reader', path.join(Compiler.directory, 'Reader' + SUFFIX), Wvb]);
            const Admissionˉoutput = await Step(Base + '-admission', Verifier, [Wvb], 120_000);
            Require(Admissionˉoutput.replaceAll('\r\n', '\n') === 'wvb status=Valid profile=compiler-aligned\n', 'Complete verifier report differs.');
            await Step(Base + '-package', process.execPath, [path.join(NATIVE, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
                '--deadline-ms', String(Math.min(Deadline - 30_000, Date.now() + 900_000)), '8', Wvb, Product]);
            Value.products.push({ name: Base + '.wvb', ...await Evidence(Wvb, Deadline, 16_777_216) },
                { name: Base + SUFFIX, ...await Evidence(Product, Deadline, MAXIMUM_IMAGE, true) });
            await Snapshotˉguards(Value, Deadline); await Save(Work, Value);
        }
        Value.status = 'Prepared'; await Save(Work, Value);
        const Record = path.join(Work, 'Staging-Capacity-Bridge.json'); const Sha256 = Hash(await Payload(Record, Deadline, MAXIMUM_RECORD));
        await Readˉpreparedˉstagingˉcapacity(Record, Sha256, Compilerˉkey, Deadline);
        console.log(`staging capacity status=Prepared functions=2048 compiler-construction=disabled record=${Record} record-sha256=${Sha256}`);
        return { Path: Record, Sha256 };
    } catch (Error) {
        Value.status = 'Failed'; Value.failure = String(Error.message).slice(-4_096); await Save(Work, Value); throw Error;
    }
}
export async function Packageˉwithˉstagingˉcapacity(Pair, Input, Output, Profile, Deadline) {
    Time(Deadline); Require(['7', '8'].includes(Profile) && ADMITTED_PAIRS.has(Pair) && Pair.Record.capacity === 2048 &&
        typeof Pair.Requireˉunchanged === 'function', 'Explicit admitted capacity pair/profile required.');
    Require(process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === undefined || process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === '1',
        'WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or1.');
    await Pair.Requireˉunchanged(); const { Compiler, Segmented } = await Modules(Deadline);
    const Family = path.dirname(path.dirname(await Compiler.Getˉcurrentˉsplitˉcompilerˉfamily()));
    const Inputˉbytes = await Payload(Input, Deadline, 16_777_216); const Snapshot = { path: path.resolve(Input), payload: Inputˉbytes, ...Measure(Inputˉbytes) };
    const Geometry = Inputˉbytes;
    Require(Geometry.length >= 12 && Geometry.toString('ascii', 0, 4) === 'WVB1' && Geometry.readUInt16LE(4) === 1 &&
        Geometry.readUInt16LE(6) >= 11 && Geometry.readUInt16LE(6) <= 43, 'Capacity package accepts only admitted pre44 projected input.');
    const Imageˉkey = Hash(Buffer.from(JSON.stringify({ namespace: NAMESPACE, stage: 'image', host: HOST,
        pairKey: Pair.Record.key, stager: { bytes: Pair.Stager.bytes, sha256: Pair.Stager.sha256 },
        linker: { bytes: Pair.Linker.bytes, sha256: Pair.Linker.sha256 }, input: Measure(Inputˉbytes), command: 'stage/link/transport' })));
    const Productˉkey = Hash(Buffer.from(JSON.stringify({ namespace: NAMESPACE, stage: 'product', host: HOST,
        image: Imageˉkey, profile: Profile, command: 'Package-Hosted-Wvb image' })));
    const Imageˉfamily = await Directory(path.join(Family, NAMESPACE + '-image', HOST_FAMILY), true);
    const Productˉfamily = await Directory(path.join(Family, NAMESPACE + '-hosted', HOST_FAMILY), true);
    const Admit = async () => { await Pair.Requireˉunchanged();
        Require((await Payload(Input, Deadline, 16_777_216)).equals(Inputˉbytes), 'Capacity package input changed.'); Time(Deadline); };
    const Record = path.join(Productˉfamily, Productˉkey);
    const Acquired = await Segmented.Acquireˉsegmentedˉhostedˉcheckpoint(Record, Productˉkey, Profile, Snapshot, async () => {
        const Report = await Run('projected-complete-admission', Pair.Admission.image.path, [Input], Deadline, 120_000);
        Require(Report.replaceAll('\r\n', '\n') === 'wvb status=Valid profile=compiler-aligned\n', 'Complete projected verifier report differs.');
        const Image = await Segmented.Acquireˉsegmentedˉimageˉcheckpoint(Imageˉfamily, Imageˉkey, Snapshot, async Temporary => {
            // The existing image checkpoint permits only its manifest and fragments.
            // Keep intermediate WVOP/WVLI files in separate retained task work.
            const Work = await Directory(await mkdtemp(path.join(Pair.Record.work, 'Image-Construction-')));
            const Private = path.join(Work, 'Input.wvb'); await writeFile(Private, Inputˉbytes, { flag: 'wx' });
            const Objects = path.join(Work, 'Object'); const Linked = path.join(Work, 'Linked');
            await Run('capacity-stage', Pair.Stager.path, [Private, Objects, Objects + '.wvop'], Deadline);
            await Run('capacity-link', Pair.Linker.path, [Objects, Objects + '.wvop', Linked, Linked + '.wvli'], Deadline);
            const Script = path.join(NATIVE, 'Transport-Compiler-Image' + WRAPPER);
            await Run('capacity-transport', WINDOWS ? Script : 'bash', WINDOWS ? [Linked, Linked + '.wvli', path.join(Temporary, 'Image'), path.join(Temporary, 'Image.wvli')]
                : [Script, Linked, Linked + '.wvli', path.join(Temporary, 'Image'), path.join(Temporary, 'Image.wvli')], Deadline);
            Require((await Payload(Private, Deadline, 16_777_216)).equals(Inputˉbytes), 'Capacity staging input copy changed.');
        }, Admit);
        return Segmented.Createˉsegmentedˉhostedˉcheckpoint(Productˉfamily, Record, Productˉkey, Profile, Snapshot, async Temporary => {
            const Script = path.join(NATIVE, 'Package-Hosted-Wvb' + WRAPPER);
            const Arguments = ['image', Profile, Input, path.join(path.dirname(Image.manifestPath), 'Image'),
                String(Image.fragments.length), String(Image.entryOffset), path.join(Temporary, 'Product' + SUFFIX), TARGET];
            await Run('capacity-container', WINDOWS ? Script : 'bash', WINDOWS ? Arguments : [Script, ...Arguments], Deadline);
        }, Admit);
    }, process.env.WINDVALE_PREPARED_PRODUCTS_ONLY === '1');
    await Admit(); await Directory(path.dirname(path.resolve(Output)));
    await Segmented.Materializeˉsegmentedˉhostedˉcheckpoint(Acquired.checkpoint, path.resolve(Output), Deadline);
    await Admit(); console.log(`staging capacity package status=${Acquired.status} profile=${Profile} key=${Productˉkey}`);
    return { Key: Productˉkey, Imageˉkey, Profile, Input: Measure(Inputˉbytes), Pair: Pair.Sha256,
        Product: await Evidence(Output, Deadline, MAXIMUM_IMAGE, true) };
}
export function Parseˉcapacityˉarguments(Arguments, Now = Date.now()) {
    Require(Arguments[0] === 'prepare' && Arguments.length === 7, 'Usage: Native-Staging-Capacity-Bridge-Core.mjs prepare --compiler-checkpoint KEY --workspace-parent PATH --deadline-ms UTC_MS');
    const Values = new Map();
    for (let Index = 1; Index < Arguments.length; Index += 2) {
        Require(['--compiler-checkpoint', '--workspace-parent', '--deadline-ms'].includes(Arguments[Index]) && !Values.has(Arguments[Index]),
            'Unknown or repeated capacity preparation option.'); Values.set(Arguments[Index], Arguments[Index + 1]);
    }
    const Deadline = Number(Values.get('--deadline-ms'));
    Require(/^[0-9a-f]{64}$/u.test(Values.get('--compiler-checkpoint')) && typeof Values.get('--workspace-parent') === 'string' &&
        Values.get('--workspace-parent').length > 0 && /^[1-9][0-9]*$/u.test(Values.get('--deadline-ms')) &&
        Number.isSafeInteger(Deadline) && Deadline > Now && Deadline <= Now + 7_200_000,
    'Capacity preparation requires exact current key and inherited finite deadline.');
    return { Key: Values.get('--compiler-checkpoint'), Parent: Values.get('--workspace-parent'), Deadline };
}
if (process.argv[1] && Same(path.resolve(process.argv[1]), SELF)) {
    try { const Options = Parseˉcapacityˉarguments(process.argv.slice(2)); await Prepareˉstagingˉcapacity(Options.Key, Options.Parent, Options.Deadline); }
    catch (Error) { process.stderr.write(String(Error.message).slice(-4_096) + '\n'); process.exitCode = Error.exitCode ?? 1; }
}
