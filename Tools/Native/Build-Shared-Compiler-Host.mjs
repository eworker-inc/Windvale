import { createHash } from 'node:crypto';
import { chmod, lstat, mkdir, mkdtemp, readFile, realpath, rename, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Readˉpreparedˉnativeˉprojection } from './Bootstrap-Native-Compiler-Projection.mjs';
import { Getˉcurrentˉsplitˉcompilerˉfamily, Getˉcurrentˉsplitˉcompilerˉkey,
    Readˉpreparedˉsplitˉcompiler } from './Current-Split-Compiler-Cache-Core.mjs';
import { Prepareˉhostedˉapplicationˉcontext } from './Native-Hosted-Application-Cache-Core.mjs';
import { Prepareˉassemblyˉobjectˉcache, Acquireˉassemblyˉobject } from './Native-Assembly-Object-Cache-Core.mjs';
import { Prepareˉnativeˉprojectˉcacheˉcontext, Getˉnativeˉprojectˉcacheˉrequest,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged } from './Native-Project-Cache-Key-Core.mjs';
import { Bindˉpublicationˉentries } from './Native-Compiler-Publication-Bindings.mjs';

const SCRIPT = fileURLToPath(import.meta.url);
const NATIVE = path.dirname(SCRIPT);
const REPOSITORY = path.resolve(NATIVE, '..', '..');
const WINDOWS = process.platform === 'win32';
const TARGET = WINDOWS ? 'windows' : 'linux';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const MAXIMUM_CHUNK = 4_194_304;
const RESPONSE_HEADER_BYTES = 32;
const MAXIMUM_OBJECT_BYTES = MAXIMUM_CHUNK - RESPONSE_HEADER_BYTES;
const MAXIMUM_IMAGE = 67_108_864;
const MAXIMUM_STAGED_CHUNKS = 518;
const MAXIMUM_INPUT_FILES = 1024;
const PUBLICATION_PROFILE = Object.freeze({ manifestMagic: 'WVOP', manifestVersion: 1,
    maximumObjectBytes: MAXIMUM_IMAGE, maximumChunkBytes: MAXIMUM_CHUNK,
    maximumChunks: MAXIMUM_STAGED_CHUNKS, prepaidMetadataBytes: 65_536,
    maximumPrefixBytes: 4078, codeCoalescingBytes: 1_310_720,
    bindingsMagic: 'WVPC', bindingsVersion: 1, bindingsBytes: 64 });
const FORMAT = 'windvale-shared-compiler-host-1';
const DRIVER_SYMBOL = 'Windvale_shared_compiler_driver';
const CONFIGURATION_SYMBOL = 'Windvale_shared_compiler_configuration';
const PREPARATION_GUIDANCE = 'Prepare the exact current compiler, native projection and shared host separately; ' +
    'then supply --shared-compiler-host-record <Host-Bridge.json> <sha256>. Behavior does not construct compiler products.';
const CONSTRUCTION_PROJECTS = [
    'Projects/Compiler/Windvale-Native-X64-Lowering.wvproj',
    'Projects/Linker/Windvale-Compiler-Image-Staging.wvproj',
    'Projects/Linker/Windvale-Shared-Compiler-Byte-Result-Admission.wvproj',
    'Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj',
];
const COMPILER_PRODUCTS = ['Analyzer' + SUFFIX, 'Analyzer.identity', 'Emitter' + SUFFIX,
    'Emitter.identity', ...['Admitter', 'Authenticator', 'Reader', 'Binder'].map(Name => Name + SUFFIX),
    'Checkpoint.json'];
const PINNED_PRODUCTS = [
    `Artifacts/Native-Front-Door/${TARGET}-x64/wvasm${SUFFIX}`,
    `Artifacts/Native-Wvo-Object-Candidate/Wvo-Object${SUFFIX}`,
    `Artifacts/Native-Wv-Linker-Candidate/Wv-Linker${SUFFIX}`,
    `Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo${SUFFIX}`,
    `Artifacts/Native-Wvo-Publisher-Candidate/${TARGET}-x64-wvopublish${SUFFIX}`,
    `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${TARGET}-x64-wvimagetransport${SUFFIX}`,
];
const RUNTIME_SOURCES = [
    'Runtime/Native/X64-Owned-Domain.wva',
    'Runtime/Native/X64-Owned-Storage.wva',
    'Runtime/Native/X64-Budgeted-Storage.wva',
    'Runtime/Native/X64-Memory-Budget-Validation.wva',
    'Runtime/Native/X64-Shared-Storage.wva',
    'Compiler/Native/Allocator/Descriptor-Allocator.wva',
];
const SOURCE_INPUTS = [
    'Compiler/Windvale/Native-X64-Lowering-Memory-Adapter.wv',
    'Compiler/Windvale/Native-X64-Lowering-Publication-Session.wv',
    'Compiler/Windvale/Native-X64-Lowering-Metadata.wv',
    'Linker/Startup/X64-Shared-Compiler-Host.wva',
    'Linker/Startup/X64-Shared-Compiler-Publication.wva',
    'Linker/Startup/Shared-Compiler-Byte-Result-Admission-Adapter.wv',
    'Runtime/Windvale/Native-Byte-Result-Admission-Core.wv',
    'Projects/Linker/Windvale-Shared-Compiler-Byte-Result-Admission.wvproj',
    'Projects/Linker/Windvale-Compiler-Image-Staging.wvproj',
    'Compiler/Windvale/Native-X64-Lowering-Shared-Templates.wv',
    'Compiler/Windvale/Native-X64-Lowering-Sha256.wv',
    'Compiler/Windvale/Native-X64-Lowering-Staging-Manifest.wv',
    'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv',
    'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv',
    'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations.wv',
    'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image.wv',
    'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Verification.wv',
    'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Staging-Tool.wv',
    'Linker/Windvale/Compiler-Flat-Image-Staging-Resources.wv',
    'Linker/Windvale/Compiler-Flat-Image-Staging-Manifest.wv',
    ...RUNTIME_SOURCES,
];
const TOOL_INPUTS = ['Build-Shared-Compiler-Host.mjs', 'Bootstrap-Native-Compiler-Projection.mjs',
    'Native-Compiler-Publication-Bindings.mjs',
    'Native-Assembly-Object-Cache-Core.mjs', 'Native-Hosted-Application-Cache-Core.mjs',
    'Development-Command-Core.mjs', 'Verify-Wvb.mjs',
    ...['Assemble-Wva', 'Link-Wvo', 'Lower-Wvb-To-Wvo', 'Publish-Wvo', 'Transport-Compiler-Image', 'Package-Hosted-Wvb']
        .map(Name => Name + (WINDOWS ? '.cmd' : '.sh'))];
const Hash = Value => createHash('sha256').update(Value).digest('hex');
function Require(Condition, Message) { if (!Condition) throw new Error(Message); }
function Sameˉpath(A, B) { return WINDOWS ? A.toLowerCase() === B.toLowerCase() : A === B; }
function Requireˉprivateˉparent(Place) {
    const Relative = path.relative(REPOSITORY, Place);
    Require(Relative !== '' && (Relative === '..' || Relative.startsWith('..' + path.sep) || path.isAbsolute(Relative)),
        'Shared native construction needs a resolved private workspace outside the repository.');
}
async function Directory(Name) {
    const Place = path.resolve(Name), Information = await lstat(Place);
    Require(Information.isDirectory() && !Information.isSymbolicLink() &&
        Sameˉpath(Place, await realpath(Place)), 'Noncanonical host bridge directory: ' + Place);
    return Place;
}
async function Read(Name, Maximum = MAXIMUM_CHUNK) {
    const Place = path.resolve(Name);
    await Directory(path.dirname(Place));
    const Information = await lstat(Place);
    Require(Information.isFile() && !Information.isSymbolicLink() && Information.nlink === 1 &&
        Information.size > 0 && Information.size <= Maximum &&
        Sameˉpath(Place, await realpath(Place)), 'Nonordinary or oversized host bridge input: ' + Place);
    const Value = await readFile(Place);
    Require(Value.length === Information.size, 'Host bridge input changed while reading.');
    return Value;
}
async function Absent(Name) {
    Require(await lstat(Name).then(() => false, Error => {
        if (Error.code === 'ENOENT') return true; throw Error;
    }), 'Host bridge output already exists: ' + Name);
}
export function Parseˉhostˉarguments(Arguments, Now = Date.now()) {
    const Keys = ['--deadline-ms', '--workspace-parent', '--projection-record', '--projection-sha256',
        '--staging-linker', '--staging-linker-sha256', '--admission-wvb', '--admission-wvb-sha256',
        '--carrier-wvb', '--carrier-wvb-sha256', '--output'];
    const Values = new Map();
    Require(Arguments.length === Keys.length * 2, 'Every explicit host bridge input is required.');
    for (let Index = 0; Index < Arguments.length; Index += 2) {
        Require(Keys.includes(Arguments[Index]) && !Values.has(Arguments[Index]) &&
            typeof Arguments[Index + 1] === 'string' && Arguments[Index + 1].length > 0 &&
            Arguments[Index + 1].length <= 32768, 'Invalid or repeated host bridge argument.');
        Values.set(Arguments[Index], Arguments[Index + 1]);
    }
    for (const Key of Keys.filter(Key => Key.endsWith('-sha256'))) {
        Require(/^[0-9a-f]{64}$/u.test(Values.get(Key)), 'An exact host bridge input identity is required.');
    }
    const Deadline = Number(Values.get('--deadline-ms'));
    Require(/^[1-9][0-9]*$/u.test(Values.get('--deadline-ms')) && Number.isSafeInteger(Deadline) &&
        Deadline > Now && Deadline <= Now + 7_200_000, 'Host bridge needs a finite deadline within two hours.');
    Values.Deadline = Deadline;
    return Values;
}
export function Providerˉpacket(Configuration, Unitˉbytes, Entry, Budgeted, Shared) {
    Require(Configuration.length === 64 && Configuration.toString('ascii', 0, 4) === 'WVSC' &&
        Configuration.readUInt32LE(4) === 1 && Configuration.readUInt32LE(8) === 64,
    'Invalid admitted compiler configuration.');
    const Moduleˉbytes = Configuration.readUInt32LE(12);
    const Position = Math.ceil(Moduleˉbytes / 16) * 16;
    Require(Moduleˉbytes > 0 && Moduleˉbytes <= MAXIMUM_IMAGE && Number.isSafeInteger(Unitˉbytes) &&
        Unitˉbytes > 0 && Unitˉbytes <= MAXIMUM_CHUNK && Position + Unitˉbytes <= MAXIMUM_IMAGE &&
        [Entry, Budgeted, Shared].every(Value => Number.isSafeInteger(Value) &&
            Value >= Position && Value < Position + Unitˉbytes) &&
        new Set([Entry, Budgeted, Shared]).size === 3, 'Invalid companion provider geometry.');
    const Value = Buffer.alloc(40); Value.write('WVSP', 0, 'ascii');
    [1, 40, Position, Unitˉbytes, Entry, Budgeted, Shared].forEach((Word, Index) =>
        Value.writeUInt32LE(Word, 4 + Index * 4));
    return Value;
}
export function Configurationˉassembly(Value) {
    Require(Value.length === 64, 'Configuration source must contain exactly64 bytes.');
    return 'windvale-assembly 1\n\nsymbol export data ' + CONFIGURATION_SYMBOL +
        ' in .rodata\nsection rodata .rodata align 16\ndefine ' + CONFIGURATION_SYMBOL +
        '\nbytes ' + [...Value].join(' ') + '\nend define\nend section\n';
}
export function Linkˉexports(Report, Names) {
    const Result = new Map();
    for (const Line of Report.split(/\r?\n/u)) {
        const Match = /^symbol index=[0-9]+ input=[0-9]+ source-index=[0-9]+ binding=export kind=(?:function|data) name=([^ ]+) address=([0-9]+) size=([0-9]+)$/u.exec(Line);
        if (!Match || !Names.includes(Match[1])) continue;
        const Address = Number(Match[2]), Bytes = Number(Match[3]);
        Require(!Result.has(Match[1]) && Number.isSafeInteger(Address) &&
            Address >= 0 && Address <= MAXIMUM_IMAGE && Number.isSafeInteger(Bytes) && Bytes > 0,
        'Duplicate or invalid companion export: ' + Match[1]);
        Result.set(Match[1], { address: Address, bytes: Bytes });
    }
    Require(Names.every(Name => Result.has(Name)), 'The ordinary linker did not resolve every companion export.');
    return Result;
}
async function Constructionˉbindings() {
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('shared-compiler-host-construction-v1',
        [SCRIPT, path.join(NATIVE, 'Bootstrap-Native-Compiler-Projection.mjs')]);
    const Requests = [];
    for (const Name of CONSTRUCTION_PROJECTS)
        Requests.push(await Getˉnativeˉprojectˉcacheˉrequest(Context, path.join(REPOSITORY, Name)));
    const Hosted = await Prepareˉhostedˉapplicationˉcontext(TARGET,
        path.join(NATIVE, 'Package-Hosted-Wvb' + (WINDOWS ? '.cmd' : '.sh')));
    Require(Hosted.producerFields.length > 0 && Hosted.producerFields.length <= 128,
        'Hosted construction producer inventory exceeds its bound.');
    return { Requests, Projects: Requests.map((Request, Index) =>
        ({ path: CONSTRUCTION_PROJECTS[Index], key: Request.key })),
    Hostedˉproducers: Hosted.producerFields.map(Item => ({ label: Item.label,
        bytes: Item.bytes.readBigUInt64LE(0).toString(), sha256: Item.bytes.subarray(8).toString('hex') })) };
}
function Preparedˉfailure(Message) {
    return Object.assign(new Error(Message + ' ' + PREPARATION_GUIDANCE), { exitCode: 64 });
}
const Preparedˉhosts = new WeakSet();
const Preparedˉexecutionˉchecks = new WeakMap();
// This is a read-only selection boundary. Every input is revalidated before
// behavior and again on completion; no preparation callback exists here.
export async function Readˉpreparedˉsharedˉcompilerˉhost(Recordˉpath, Sha256, Compilerˉkey, Deadline = Date.now() + 600_000) {
    try {
        function Checkˉdeadline() {
            Require(Number.isSafeInteger(Deadline) && Date.now() < Deadline, 'Prepared host selection deadline expired.');
        }
        Checkˉdeadline();
        const Currentˉkey = await Getˉcurrentˉsplitˉcompilerˉkey();
        if (Compilerˉkey === undefined) Compilerˉkey = Currentˉkey;
        Require(typeof Recordˉpath === 'string' && /^[0-9a-f]{64}$/u.test(Sha256) &&
            /^[0-9a-f]{64}$/u.test(Compilerˉkey) && Compilerˉkey === Currentˉkey,
        'Invalid or noncurrent prepared host selection.');
        Recordˉpath = path.resolve(Recordˉpath);
        Require(path.basename(Recordˉpath) === 'Host-Bridge.json', 'Unexpected host record name.');
        const Original = await Read(Recordˉpath, 1_048_576);
        Require(Hash(Original) === Sha256, 'Prepared host record identity differs.');
        const Value = JSON.parse(Original.toString('utf8'));
        Require(Value.format === FORMAT && Value.status === 'Produced' && Value.host === TARGET + '-x64' &&
            Sameˉpath(await Directory(Value.work), path.dirname(Recordˉpath)) &&
            Value.qualified === false && Value.maximumInputBytes === MAXIMUM_CHUNK &&
            Value.maximumResultBytes === MAXIMUM_CHUNK && Value.maximumObjectBytes === MAXIMUM_OBJECT_BYTES &&
            Object.entries(PUBLICATION_PROFILE).every(([Name, Expected]) => Value.stagedPublication?.[Name] === Expected) &&
            Value.response?.magic === 'WVNR' && Value.response.version === 1 && Value.response.headerBytes === 32 &&
            Value.physicalArenaBytes === 16_777_216 && Value.runtimeMaximum === 16_777_216 &&
            Value.applicationMaximum === 16_777_216 && Value.rootMaximum === 41_943_040 &&
            Value.compilerCheckpoint === Compilerˉkey && Value.nodeVersion === process.version &&
            Array.isArray(Value.inputs) && Value.inputs.length > 0 && Value.inputs.length <= MAXIMUM_INPUT_FILES &&
            Array.isArray(Value.products) && Value.products.length === 1,
        'Unsupported, incomplete or stale prepared host record.');
        Requireˉprivateˉparent(Value.work);
        const Compiler = await Readˉpreparedˉsplitˉcompiler(await Getˉcurrentˉsplitˉcompilerˉfamily(), Compilerˉkey);
        const Projection = await Readˉpreparedˉnativeˉprojection(Value.projectionRecord.path,
            Value.projectionRecord.sha256, Compilerˉkey, Deadline);
        const Bindings = await Constructionˉbindings();
        Require(JSON.stringify(Value.projects) === JSON.stringify(Bindings.Projects) &&
            JSON.stringify(Value.hostedProducers) === JSON.stringify(Bindings.Hostedˉproducers),
        'Prepared host construction inputs differ from current inputs.');
        const Expected = new Set([...SOURCE_INPUTS.map(Name => path.join(REPOSITORY, Name)),
            ...TOOL_INPUTS.map(Name => path.join(NATIVE, Name)), ...PINNED_PRODUCTS.map(Name => path.join(REPOSITORY, Name)),
            ...COMPILER_PRODUCTS.map(Name => path.join(Compiler.directory, Name)),
            ...Bindings.Requests.flatMap(Request => Request.inputEvidence.map(Item => Item.path))]);
        const Actual = new Set(); let Total = 0;
        async function Check(Item) {
            Checkˉdeadline();
            Require(typeof Item.path === 'string' && path.isAbsolute(Item.path) &&
                Sameˉpath(path.resolve(Item.path), Item.path) && Number.isSafeInteger(Item.bytes) &&
                Item.bytes > 0 && Item.bytes <= MAXIMUM_IMAGE && /^[0-9a-f]{64}$/u.test(Item.sha256),
            'Malformed prepared host file identity.');
            const Bytes = await Read(Item.path, Math.min(MAXIMUM_IMAGE, Item.bytes));
            Require(Bytes.length === Item.bytes && Hash(Bytes) === Item.sha256,
                'Prepared host file changed: ' + Item.path);
        }
        for (const Item of Value.inputs) {
            const Name = WINDOWS ? Item.path.toLowerCase() : Item.path;
            Require(!Actual.has(Name), 'Repeated prepared host input.'); Actual.add(Name);
            Total += Item.bytes; Require(Total <= 536_870_912, 'Prepared host input inventory exceeds512 MiB.');
            await Check(Item);
        }
        Require([...Expected].every(Name => Actual.has(WINDOWS ? Name.toLowerCase() : Name)),
            'Prepared host omitted a declared construction input.');
        for (const Item of Projection.Record.products.filter(Item => Item.path === 'Successor.wvb' ||
            Item.path === 'Successor-Object.wvop' || Item.path.startsWith('Successor-Object.chunk-')))
            Require(Value.inputs.some(Input => Sameˉpath(Input.path,
                path.join(Projection.Record.work, 'Products', Item.path)) && Input.bytes === Item.bytes &&
                Input.sha256 === Item.sha256), 'Prepared host successor input differs.');
        const Product = Value.products[0]; await Check(Product);
        Require(Sameˉpath(Product.path, Value.output) && Product.path.toLowerCase().endsWith(SUFFIX),
            'Prepared host output differs from its product.');
        async function Checkˉconfiguration() { for (const [Item, Name] of
            [[Value.providerPacket, 'Providers.wvsp'], [Value.configuration, 'Final.wvsc']]) {
            Require(typeof Item?.path === 'string' && /^[0-9a-f]{64}$/u.test(Item.sha256),
                'Malformed host configuration identity.');
            Require(Sameˉpath(Item.path, path.join(Value.work, Name)), 'Host configuration path escapes its exact workspace.');
            Require(Hash(await Read(Item.path, 64)) === Item.sha256, 'Prepared host configuration changed.');
        }
            const Packet = await Read(Value.providerPacket.path, 40), Final = await Read(Value.configuration.path, 64);
            Require(Packet.length === 40 && Final.length === 64 && Final.toString('ascii', 0, 4) === 'WVSC' &&
                Final.readUInt32LE(4) === 1 && Final.readUInt32LE(8) === 64 && Final.readUInt32LE(60) === 0 &&
                Final.readUInt32LE(48) === Packet.readUInt32LE(12) &&
                Final.readUInt32LE(52) === Packet.readUInt32LE(16) && Final.readUInt32LE(56) === Packet.readUInt32LE(20) &&
                Providerˉpacket(Final, Final.readUInt32LE(52), Final.readUInt32LE(56),
                    Packet.readUInt32LE(24), Packet.readUInt32LE(28)).equals(Packet),
            'Prepared source/provider configuration geometry differs.');
        }
        await Checkˉconfiguration();
        const Segmented = {};
        for (const [Name, Key] of [['Stager', 'stager'], ['Linker', 'linker'], ['Carrier', 'carrier']]) {
            const Item = Value.segmentedConsumer?.[Key];
            Require(Item && Value.inputs.some(Input => Sameˉpath(Input.path, Item.path) &&
                Input.bytes === Item.bytes && Input.sha256 === Item.sha256), 'Missing exact segmented consumer ' + Name);
            Segmented[Name] = Object.freeze({ Path: Item.path, Sha256: Item.sha256 });
        }
        Require(Projection.Record.products.some(Item => Item.path === 'Projected-Stager' + SUFFIX &&
            Sameˉpath(path.join(Projection.Record.work, 'Products', Item.path), Segmented.Stager.Path) &&
            Item.sha256 === Segmented.Stager.Sha256), 'Segmented stager is outside the admitted projection.');
        const Selected = Object.freeze({ Path: Product.path, Sha256: Product.sha256, Bytes: Product.bytes,
            Recordˉsha256: Sha256, Compilerˉkey, Compilerˉcheckpoint: Compiler,
            Segmentedˉconsumer: Object.freeze(Segmented), Requireˉunchanged: async () => {
                Checkˉdeadline();
                Require((await Read(Recordˉpath, 1_048_576)).equals(Original), 'Prepared host record changed.');
                for (const Item of [...Value.inputs, Product]) await Check(Item);
                await Checkˉconfiguration();
                await Projection.Requireˉunchanged(); await Compiler.Requireˉunchanged();
                for (const Request of Bindings.Requests) await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
                Require(JSON.stringify((await Constructionˉbindings()).Hostedˉproducers) ===
                    JSON.stringify(Value.hostedProducers) && await Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey,
                'Prepared host producers or current compiler changed.');
                Checkˉdeadline();
            } });
        Preparedˉexecutionˉchecks.set(Selected, async () => {
            Checkˉdeadline();
            Require((await Read(Recordˉpath, 1_048_576)).equals(Original), 'Prepared host record changed.');
            const Items = [Product, ...Object.values(Segmented).map(Component =>
                Value.inputs.find(Item => Sameˉpath(Item.path, Component.Path)))];
            const Seen = new Set();
            for (const Item of Items) {
                const Name = WINDOWS ? Item.path.toLowerCase() : Item.path;
                if (!Seen.has(Name)) { Seen.add(Name); await Check(Item); }
            }
            await Checkˉconfiguration(); Checkˉdeadline();
        });
        Checkˉdeadline(); Preparedˉhosts.add(Selected); return Selected;
    } catch (Error) { throw Preparedˉfailure(Error.message); }
}

// The caller owns this batch and must close it before publishing success.
// Full construction provenance surrounds the batch; nested operations retain
// exact execution/configuration checks without replaying the recovery graph.
export async function Openˉpreparedˉsharedˉcompilerˉhost(Recordˉpath, Sha256, Compilerˉkey, Deadline) {
    const Original = await Readˉpreparedˉsharedˉcompilerˉhost(Recordˉpath, Sha256, Compilerˉkey, Deadline);
    const Check = Preparedˉexecutionˉchecks.get(Original);
    Require(typeof Check === 'function', 'Prepared host has no execution guard.');
    let Active = true;
    const Host = Object.freeze({ ...Original, Requireˉunchanged: async () => {
        Require(Active, 'Prepared shared compiler host batch is closed.');
        await Check();
    } });
    Preparedˉhosts.add(Host);
    return Object.freeze({ Host, Close: async () => {
        Require(Active, 'Prepared shared compiler host batch is already closed.');
        Active = false; Preparedˉhosts.delete(Host);
        await Original.Requireˉunchanged();
    } });
}
export function Parseˉstagedˉnativeˉmanifest(Manifest) {
    Require(Buffer.isBuffer(Manifest) && Manifest.length >= 24 &&
        Manifest.length <= 24 + MAXIMUM_STAGED_CHUNKS * 12 && Manifest.toString('ascii', 0, 4) === 'WVOP' &&
        Manifest.readUInt32LE(4) === 1 && Manifest.readUInt32LE(8) === Manifest.length &&
        Manifest.readUInt32LE(12) > 0 && Manifest.readUInt32LE(12) <= MAXIMUM_IMAGE &&
        Manifest.readUInt32LE(16) > 0 && Manifest.readUInt32LE(16) <= MAXIMUM_STAGED_CHUNKS &&
        Manifest.readUInt32LE(20) === MAXIMUM_CHUNK &&
        Manifest.length === 24 + Manifest.readUInt32LE(16) * 12,
    'Invalid bounded compiler staging manifest.');
    const Lengths = [];
    let Position = 0;
    for (let Index = 0; Index < Manifest.readUInt32LE(16); Index++) {
        const At = 24 + Index * 12, Length = Manifest.readUInt32LE(At + 8);
        Require(Manifest.readUInt32LE(At) === Index && Manifest.readUInt32LE(At + 4) === Position &&
            Length > 0 && Length <= MAXIMUM_CHUNK && Position + Length <= Manifest.readUInt32LE(12),
        'Invalid compiler staged chunk geometry.');
        Lengths.push(Length); Position += Length;
    }
    Require(Position === Manifest.readUInt32LE(12), 'Incomplete compiler staged object.');
    return Lengths;
}
async function Readˉstagedˉnativeˉobject(Prefix, Readˉpart = Read) {
    const Manifest = await Readˉpart(Prefix + '.wvop', 24 + MAXIMUM_STAGED_CHUNKS * 12);
    // Admit all geometry before reading or retaining any object payload.
    const Lengths = Parseˉstagedˉnativeˉmanifest(Manifest), Chunks = [];
    for (const [Index, Length] of Lengths.entries()) {
        const Value = await Readˉpart(Prefix + '.chunk-' + Index, MAXIMUM_CHUNK);
        Require(Value.length === Length, 'Compiler staging chunk length differs.');
        Chunks.push(Value);
    }
    return { Manifest, Chunks };
}
async function Buildˉnativeˉcompanion(Options) {
    const { Work, Step, Sourceˉprefix, Stagingˉlinker } = Options;
    const Imageˉprefix = path.join(Work, 'Module-Image'), Imageˉmanifest = Imageˉprefix + '.wvli';
    const Initialˉconfiguration = path.join(Work, 'Initial.wvsc');
    await Step('admit-module-object', Stagingˉlinker,
        [Sourceˉprefix, Sourceˉprefix + '.wvop', Imageˉprefix, Imageˉmanifest, 'inspect', Initialˉconfiguration]);
    const Configuration = await Read(Initialˉconfiguration, 64);
    Require(Configuration.length === 64 && Configuration.toString('ascii', 0, 4) === 'WVSC' &&
        Configuration.readUInt32LE(4) === 1 && Configuration.readUInt32LE(8) === 64 &&
        Configuration.readUInt32LE(48) === 0 && Configuration.readUInt32LE(52) === 0 &&
        Configuration.readUInt32LE(56) === 0 && Configuration.readUInt32LE(60) === 0,
    'Independent compiler inspection did not return a preliminary configuration.');
    const Unitˉposition = Math.ceil(Configuration.readUInt32LE(12) / 16) * 16;
    const Objects = [];
    const Assemble = async (Name, Source) => {
        const Object = path.join(Work, Name + '.wvo');
        await Step('assemble-' + Name, WINDOWS ? path.join(NATIVE, 'Assemble-Wva.cmd') : 'bash',
            WINDOWS ? [Source, Object] : [path.join(NATIVE, 'Assemble-Wva.sh'), Source, Object]);
        Objects.push(Object); return Object;
    };
    let Publication = null;
    if (Options.Publicationˉentries === true) {
        const Object = await Readˉstagedˉnativeˉobject(Sourceˉprefix);
        Publication = Bindˉpublicationˉentries(Options.Sourceˉbytecode, Object.Chunks, Configuration);
        const Source = path.join(Work, 'Publication.wva');
        await writeFile(Source, Publication.Assembly, { flag: 'wx' });
        await Assemble('Publication', Source);
    }
    if (Options.Publicationˉsource !== undefined) {
        Require(Publication !== null, 'Publication caller requires admitted source bindings.');
        await Assemble('PublicationDriver', Options.Publicationˉsource);
    }
    const Companion = await Options.Companion(Buffer.from(Configuration), Publication);
    Require(Companion && /^[A-Za-z_][A-Za-z0-9_]{0,127}$/u.test(Companion.Entry), 'Invalid source companion entry.');
    const Driverˉsymbol = Companion.Entry;
    let Companionˉsource = Companion.Path;
    if (typeof Companion.Source === 'string') {
        const Bytes = Buffer.from(Companion.Source);
        Require(Bytes.length > 0 && Bytes.length <= 1_048_576 && !Companion.Source.includes('\r') &&
            !Companion.Source.startsWith('\ufeff'), 'Companion source must be bounded LF UTF-8 without BOM.');
        Companionˉsource = path.join(Work, 'Companion.wva');
        await writeFile(Companionˉsource, Bytes, { flag: 'wx' });
    }
    await Assemble('Driver', Companionˉsource);
    const Assemblyˉcache = await Prepareˉassemblyˉobjectˉcache(Options.Deadline);
    for (let Index = 0; Index < Options.Runtimeˉpaths.length; Index++) {
        process.stdout.write(`shared native companion step=runtime-object item=${Index + 1}/${Options.Runtimeˉpaths.length} status=Started\n`);
        const Object = path.join(Work, 'Runtime-' + Index + '.wvo');
        await Acquireˉassemblyˉobject(Assemblyˉcache, Options.Runtimeˉpaths[Index], Object);
        Objects.push(Object);
    }
    await Assemblyˉcache.Requireˉunchanged();
    if (Options.Admissionˉwvb !== null) {
        const Admissionˉobject = path.join(Work, 'Admission.wvo');
        await Step('lower-independent-reader', WINDOWS ? path.join(NATIVE, 'Lower-Wvb-To-Wvo.cmd') : 'bash',
            WINDOWS ? [Options.Admissionˉwvb, Admissionˉobject] :
                [path.join(NATIVE, 'Lower-Wvb-To-Wvo.sh'), Options.Admissionˉwvb, Admissionˉobject]);
        Objects.push(Admissionˉobject);
    }
    const Configurationˉsource = path.join(Work, 'Configuration.wva');
    await writeFile(Configurationˉsource, Configurationˉassembly(Configuration), { flag: 'wx' });
    const Configurationˉobject = await Assemble('Configuration', Configurationˉsource);
    const Names = [Driverˉsymbol, 'Windvale_budgeted_storage', 'Windvale_shared_storage', CONFIGURATION_SYMBOL];
    const Unit = path.join(Work, 'Companion.bin');
    async function Link(Name) {
        const Report = await Step(Name, WINDOWS ? path.join(NATIVE, 'Link-Wvo.cmd') : 'bash',
            WINDOWS ? [String(Unitˉposition), Driverˉsymbol, Unit, ...Objects] :
                [path.join(NATIVE, 'Link-Wvo.sh'), String(Unitˉposition), Driverˉsymbol, Unit, ...Objects]);
        await writeFile(path.join(Work, Name + '.txt'), Report);
        return Linkˉexports(Report, Names);
    }
    const Initial = await Link('link-companion-preliminary');
    const Initialˉunit = await Read(Unit, MAXIMUM_CHUNK);
    const Packet = Providerˉpacket(Configuration, Initialˉunit.length,
        Initial.get(Driverˉsymbol).address, Initial.get('Windvale_budgeted_storage').address,
        Initial.get('Windvale_shared_storage').address);
    const Providerˉpath = path.join(Work, 'Providers.wvsp'), Finalˉconfiguration = path.join(Work, 'Final.wvsc');
    await writeFile(Providerˉpath, Packet, { flag: 'wx' });
    await Step('link-verified-module', Stagingˉlinker,
        [Sourceˉprefix, Sourceˉprefix + '.wvop', Imageˉprefix, Imageˉmanifest, Providerˉpath, Finalˉconfiguration]);
    const Final = await Read(Finalˉconfiguration, 64);
    Require(Final.length === 64 && Final.subarray(0, 48).equals(Configuration.subarray(0, 48)) &&
        Final.readUInt32LE(48) === Unitˉposition && Final.readUInt32LE(52) === Initialˉunit.length &&
        Final.readUInt32LE(56) === Initial.get(Driverˉsymbol).address && Final.readUInt32LE(60) === 0,
    'Final compiler configuration differs from admitted source/provider geometry.');
    await writeFile(Configurationˉsource, Configurationˉassembly(Final));
    await Step('assemble-final-configuration', WINDOWS ? path.join(NATIVE, 'Assemble-Wva.cmd') : 'bash',
        WINDOWS ? [Configurationˉsource, Configurationˉobject] :
            [path.join(NATIVE, 'Assemble-Wva.sh'), Configurationˉsource, Configurationˉobject]);
    const Finalˉexports = await Link('link-companion-final');
    const Finalˉunit = await Read(Unit, MAXIMUM_CHUNK);
    Require(Finalˉunit.length === Initialˉunit.length && [...Initial].every(([Name, Item]) =>
        Finalˉexports.get(Name).address === Item.address && Finalˉexports.get(Name).bytes === Item.bytes),
    'Companion layout changed between configuration passes.');
    const Configˉoffset = Finalˉexports.get(CONFIGURATION_SYMBOL).address - Unitˉposition;
    Require(Finalˉexports.get(CONFIGURATION_SYMBOL).bytes === 64 &&
        Finalˉunit.subarray(Configˉoffset, Configˉoffset + 64).equals(Final) &&
        Initialˉunit.subarray(0, Configˉoffset).equals(Finalˉunit.subarray(0, Configˉoffset)) &&
        Initialˉunit.subarray(Configˉoffset + 64).equals(Finalˉunit.subarray(Configˉoffset + 64)),
    'Companion changed outside its exact64-byte immutable configuration.');
    const Canonical = path.join(Work, 'Canonical-Module'), Canonicalˉmanifest = Canonical + '.wvli';
    await Step('transport-module-image', WINDOWS ? path.join(NATIVE, 'Transport-Compiler-Image.cmd') : 'bash',
        WINDOWS ? [Imageˉprefix, Imageˉmanifest, Canonical, Canonicalˉmanifest] :
            [path.join(NATIVE, 'Transport-Compiler-Image.sh'), Imageˉprefix, Imageˉmanifest, Canonical, Canonicalˉmanifest]);
    const Wvli = await Read(Canonicalˉmanifest, 28 + 16 * 12);
    Require(Wvli.length >= 40 && Wvli.toString('ascii', 0, 4) === 'WVLI' &&
        Wvli.readUInt32LE(4) === 1 && Wvli.readUInt32LE(8) === Wvli.length &&
        Wvli.readUInt32LE(12) === Configuration.readUInt32LE(12) &&
        Wvli.readUInt32LE(16) === Configuration.readUInt32LE(16) &&
        Wvli.readUInt32LE(20) > 0 && Wvli.readUInt32LE(20) <= 16 &&
        Wvli.length === 28 + Wvli.readUInt32LE(20) * 12 && Wvli.readUInt32LE(24) === MAXIMUM_CHUNK,
    'Canonical verified compiler image manifest differs.');
    const Bundle = path.join(Work, 'Bundle');
    let Buffered = Buffer.alloc(0), Fragments = 0, Imageˉbytes = 0;
    async function Append(Value) {
        let At = 0;
        while (At < Value.length) {
            const Take = Math.min(MAXIMUM_CHUNK - Buffered.length, Value.length - At);
            Buffered = Buffer.concat([Buffered, Value.subarray(At, At + Take)]);
            At += Take; Imageˉbytes += Take;
            if (Buffered.length === MAXIMUM_CHUNK) {
                await writeFile(Bundle + '.chunk-' + Fragments++, Buffered, { flag: 'wx' }); Buffered = Buffer.alloc(0);
            }
        }
    }
    let Moduleˉposition = 0;
    for (let Index = 0; Index < Wvli.readUInt32LE(20); Index++) {
        const At = 28 + Index * 12, Value = await Read(Canonical + '.chunk-' + Index);
        Require(Wvli.readUInt32LE(At) === Index && Wvli.readUInt32LE(At + 4) === Moduleˉposition &&
            Wvli.readUInt32LE(At + 8) === Value.length &&
            (Index + 1 === Wvli.readUInt32LE(20) || Value.length === MAXIMUM_CHUNK),
        'Canonical compiler chunk differs.');
        Moduleˉposition += Value.length; await Append(Value);
    }
    Require(Moduleˉposition === Configuration.readUInt32LE(12), 'Canonical compiler image coverage differs.');
    await Append(Buffer.alloc(Unitˉposition - Moduleˉposition)); await Append(Finalˉunit);
    if (Buffered.length > 0) await writeFile(Bundle + '.chunk-' + Fragments++, Buffered, { flag: 'wx' });
    Require(Fragments > 0 && Fragments <= 16 && Imageˉbytes <= MAXIMUM_IMAGE &&
        Imageˉbytes === Unitˉposition + Finalˉunit.length, 'Host bundle exceeds existing image bounds.');
    const Candidate = path.join(Work, 'Product' + SUFFIX);
    await Step('package-outer-io-carrier', WINDOWS ? path.join(NATIVE, 'Package-Hosted-Wvb.cmd') : 'bash',
        WINDOWS ? ['image', '7', Options.Carrier, Bundle, String(Fragments),
            String(Initial.get(Driverˉsymbol).address), Candidate, TARGET] :
            [path.join(NATIVE, 'Package-Hosted-Wvb.sh'), 'image', '7', Options.Carrier,
                Bundle, String(Fragments), String(Initial.get(Driverˉsymbol).address), Candidate, TARGET], true);
    return { Candidate, Packet, Providerˉpath, Finalˉconfiguration, Final };
}
const Consumerˉstages = new WeakMap();
// Focused consumers use the same object inspection, provider binding and image
// packaging path as the compiler host. The stage token is private to this run.
export async function Buildˉsharedˉnativeˉconsumer(Context) {
    Require(Context && Number.isSafeInteger(Context.Deadline) && Context.Deadline > Date.now() &&
        Context.Deadline <= Date.now() + 7_200_000 && typeof Context.Companion === 'function' &&
        Preparedˉhosts.has(Context.Preparedˉsharedˉhost) && Context.Preparedˉsharedˉhost.Segmentedˉconsumer &&
        typeof Context.Preparedˉsharedˉhost.Requireˉunchanged === 'function', 'Invalid shared consumer construction request.');
    const Parent = await Directory(Context.Work), Host = Context.Preparedˉsharedˉhost;
    Requireˉprivateˉparent(Parent);
    const Source = await Read(Context.Input.Path, 16_777_216);
    Require(/^[0-9a-f]{64}$/u.test(Context.Input.Sha256) && Hash(Source) === Context.Input.Sha256 &&
        Source.length >= 12 && Source.readUInt32LE(0) === 0x31425657 &&
        Source.readUInt16LE(4) === 1 && [44, 45].includes(Source.readUInt16LE(6)), 'Shared consumer input identity/version differs.');
    await Host.Requireˉunchanged();
    async function Step(Name, Tool, Parameters, Stream = false) {
        process.stdout.write('shared native consumer step=' + Name + ' status=Started\n');
        const Result = await Runˉdevelopmentˉcommand(Tool, Parameters, Context.Deadline, Stream, 1_048_576);
        Require(Result.Code === 0 && Result.Error === '', Name + ' failed (' + Result.Code + '): ' + Result.Output + Result.Error);
        process.stdout.write('shared native consumer step=' + Name + ' status=Complete\n');
        return Result.Output;
    }
    let Token = Context.Stagedˉmodule, Stage = Token === undefined ? undefined : Consumerˉstages.get(Token);
    if (Token !== undefined) Require(Stage && Stage.Host === Host && Stage.Input === Context.Input.Sha256 &&
        Sameˉpath(Stage.Parent, Parent), 'Shared consumer stage token differs from this exact run.');
    if (Stage === undefined) {
        const Place = await mkdtemp(path.join(Parent, 'shared-native-stage-'));
        const Input = path.join(Place, 'Input.wvb'), Prefix = path.join(Place, 'Module-Object');
        await writeFile(Input, Source, { flag: 'wx' });
        const Previous = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
        Require(Previous === undefined || Previous === '1', 'Prepared verifier mode must be absent or1.');
        process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
        try { await Step('admit-source-module', process.execPath,
            [path.join(NATIVE, 'Verify-Wvb.mjs'), '--current', Input]); }
        finally { if (Previous === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
            else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previous; }
        await Step('stage-source-module', Host.Segmentedˉconsumer.Stager.Path, [Input, Prefix, Prefix + '.wvop']);
        const Files = [];
        await Readˉstagedˉnativeˉobject(Prefix, async (Name, Maximum) => {
            const Bytes = await Read(Name, Maximum);
            Files.push({ path: Name, bytes: Bytes.length, sha256: Hash(Bytes) }); return Bytes;
        });
        Stage = { Host, Input: Context.Input.Sha256, Parent, Prefix, Files };
        Token = Object.freeze({}); Consumerˉstages.set(Token, Stage);
    }
    async function Checkˉstage() {
        for (const Item of Stage.Files) {
            const Bytes = await Read(Item.path, Item.bytes);
            Require(Bytes.length === Item.bytes && Hash(Bytes) === Item.sha256, 'Staged consumer object changed.');
        }
    }
    await Checkˉstage();
    const Work = await mkdtemp(path.join(Parent, 'shared-native-consumer-'));
    const Built = await Buildˉnativeˉcompanion({ Work, Step, Deadline: Context.Deadline, Sourceˉprefix: Stage.Prefix,
        Stagingˉlinker: Host.Segmentedˉconsumer.Linker.Path, Carrier: Host.Segmentedˉconsumer.Carrier.Path,
        Admissionˉwvb: null, Runtimeˉpaths: RUNTIME_SOURCES.map(Name => path.join(REPOSITORY, Name)),
        Companion: Context.Companion });
    Require((await Read(Context.Input.Path, 16_777_216)).equals(Source), 'Shared consumer source changed.');
    await Checkˉstage(); await Host.Requireˉunchanged();
    const Product = await Read(Built.Candidate, MAXIMUM_IMAGE);
    if (!WINDOWS) await chmod(Built.Candidate, 0o755);
    return { Path: Built.Candidate, Sha256: Hash(Product), Stagedˉmodule: Token };
}
async function Main(Arguments) {
    const Options = Parseˉhostˉarguments(Arguments);
    Require(process.arch === 'x64' && ['win32', 'linux'].includes(process.platform),
        'Host bridge supports Windows and real Linux x64 only.');
    const Parent = await Directory(Options.get('--workspace-parent'));
    Requireˉprivateˉparent(Parent);
    const Output = path.resolve(Options.get('--output'));
    await Directory(path.dirname(Output)); await Absent(Output);
    Require(Output.toLowerCase().endsWith(SUFFIX), 'Host bridge output extension differs from the host target.');
    const Recordˉpath = path.resolve(Options.get('--projection-record'));
    const Recordˉbytes = await Read(Recordˉpath, 1_048_576);
    Require(Hash(Recordˉbytes) === Options.get('--projection-sha256'), 'Projection record identity differs.');
    const Projection = JSON.parse(Recordˉbytes.toString('utf8'));
    Require(Projection.format === 'windvale-native-bootstrap-projection-2' &&
        Projection.status === 'AwaitingSuccessorAndQualification' &&
        Projection.host === process.platform + '-' + process.arch &&
        Sameˉpath(await Directory(Projection.work), path.dirname(Recordˉpath)) &&
        Array.isArray(Projection.products) && Projection.products.length <= 1024,
    'The selected true compiler has incomplete or mismatched bootstrap provenance.');
    const Compilerˉkey = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Compiler = await Readˉpreparedˉsplitˉcompiler(await Getˉcurrentˉsplitˉcompilerˉfamily(), Compilerˉkey);
    const Preparedˉprojection = await Readˉpreparedˉnativeˉprojection(Recordˉpath, Hash(Recordˉbytes), Compilerˉkey, Options.Deadline);
    const Bindings = await Constructionˉbindings();
    const Products = path.join(Projection.work, 'Products');
    const Inputs = [];
    let Inputˉbytes = 0;
    async function Capture(Name, Maximum, Expected, Label) {
        Require(Date.now() < Options.Deadline, 'Host bridge deadline expired while reading inputs.');
        const Value = await Read(Name, Maximum);
        Require(Expected === null || Hash(Value) === Expected, Label + ' identity differs.');
        const Place = path.resolve(Name), Digest = Hash(Value);
        const Prior = Inputs.find(Item => Sameˉpath(Item.path, Place));
        if (Prior) Require(Prior.bytes === Value.length && Prior.sha256 === Digest, 'Repeated input changed: ' + Label);
        else {
            Inputˉbytes += Value.length;
            Require(Inputs.length < MAXIMUM_INPUT_FILES && Inputˉbytes <= 536_870_912,
                'Host construction input inventory exceeds its512 MiB/1024file bound.');
            Inputs.push({ path: Place, bytes: Value.length, sha256: Digest });
        }
        return Value;
    }
    async function Projectionˉproduct(Name, Maximum) {
        const Items = Projection.products.filter(Item => Item.workspace === 'Products' && Item.path === Name);
        Require(Items.length === 1 && /^[0-9a-f]{64}$/u.test(Items[0].sha256), 'Missing exact true compiler product: ' + Name);
        const Value = await Capture(path.join(Products, Name), Maximum, Items[0].sha256, Name);
        Require(Value.length === Items[0].bytes, 'True compiler product length differs: ' + Name);
        return Value;
    }
    const Module = await Projectionˉproduct('Successor.wvb', 16_777_216);
    Require(Module.length >= 12 && Module.readUInt32LE(0) === 0x31425657 &&
        Module.readUInt16LE(4) === 1 && Module.readUInt16LE(6) === 45, 'True compiler must be WVB1.45.');
    const { Manifest, Chunks } = await Readˉstagedˉnativeˉobject(path.join(Products, 'Successor-Object'),
        (Name, Maximum) => Projectionˉproduct(path.basename(Name), Maximum));
    const Stagingˉlinker = path.resolve(Options.get('--staging-linker'));
    await Capture(Stagingˉlinker, MAXIMUM_IMAGE, Options.get('--staging-linker-sha256'), 'Current staging linker');
    const Admission = await Capture(Options.get('--admission-wvb'), MAXIMUM_CHUNK,
        Options.get('--admission-wvb-sha256'), 'Independent source result reader WVB');
    const Carrier = await Capture(Options.get('--carrier-wvb'), MAXIMUM_CHUNK,
        Options.get('--carrier-wvb-sha256'), 'Outer hosted metadata carrier WVB');
    for (const [Name, Value] of [['reader', Admission], ['carrier', Carrier]]) {
        Require(Value.length >= 12 && Value.toString('ascii', 0, 4) === 'WVB1' &&
            Value.readUInt16LE(4) === 1 && [11, 16, 30, 31, 39].includes(Value.readUInt16LE(6)),
        'The selected ' + Name + ' is outside retained host/lowerer admission.');
    }
    const Work = await mkdtemp(path.join(Parent, 'windvale-shared-compiler-host-'));
    const State = { format: FORMAT, status: 'Constructing', phase: 'snapshot', host: TARGET + '-x64',
        work: Work, output: Output, sourceRevision: Projection.sourceRevision,
        compilerCheckpoint: Compilerˉkey, nodeVersion: process.version,
        projects: Bindings.Projects, hostedProducers: Bindings.Hostedˉproducers,
        projectionRecord: { path: Recordˉpath, sha256: Hash(Recordˉbytes) },
        maximumInputBytes: MAXIMUM_CHUNK, maximumResultBytes: MAXIMUM_CHUNK,
        maximumObjectBytes: MAXIMUM_OBJECT_BYTES,
        stagedPublication: PUBLICATION_PROFILE,
        response: { magic: 'WVNR', version: 1, headerBytes: RESPONSE_HEADER_BYTES },
        physicalArenaBytes: 16_777_216, runtimeMaximum: 16_777_216, applicationMaximum: 16_777_216,
        rootMaximum: 41_943_040, sourceInputs: [], commandInputs: [], products: [], qualified: false,
        selfLowering: 'RetainedPublicationSessionExecutionPending' };
    async function Save() { await writeFile(path.join(Work, 'Host-Bridge.json'), JSON.stringify(State, null, 2) + '\n'); }
    async function Step(Name, Tool, Parameters, Stream = false) {
        State.phase = Name; await Save();
        process.stdout.write('shared compiler host step=' + Name + ' status=Started\n');
        const Result = await Runˉdevelopmentˉcommand(Tool, Parameters, Options.Deadline, Stream, 1_048_576);
        Require(Result.Code === 0 && Result.Error === '', Name + ' failed (' + Result.Code + '): ' + Result.Output + Result.Error);
        process.stdout.write('shared compiler host step=' + Name + ' status=Complete\n');
        return Result.Output;
    }
    await Save();
    process.stdout.write('shared compiler host workspace=' + Work + ' qualified=false\n');
    try {
    const Snapshots = path.join(Work, 'Sources'); await mkdir(Snapshots);
    let Sourceˉbytes = 0;
    for (const Relative of SOURCE_INPUTS) {
        const Value = await Capture(path.join(REPOSITORY, Relative), MAXIMUM_CHUNK, null, Relative);
        Sourceˉbytes += Value.length;
        Require(Sourceˉbytes <= 16_777_216, 'Host bridge source snapshot exceeds16 MiB.');
        const Destination = path.join(Snapshots, Relative);
        await mkdir(path.dirname(Destination), { recursive: true }); await writeFile(Destination, Value, { flag: 'wx' });
        State.sourceInputs.push({ path: Relative, bytes: Value.length, sha256: Hash(Value) });
    }
    for (const Name of TOOL_INPUTS) {
        const Value = await Capture(path.join(NATIVE, Name), MAXIMUM_CHUNK, null, Name);
        State.commandInputs.push({ path: 'Tools/Native/' + Name, bytes: Value.length, sha256: Hash(Value) });
    }
    for (const Name of PINNED_PRODUCTS)
        await Capture(path.join(REPOSITORY, Name), MAXIMUM_IMAGE, null, 'Pinned producer ' + Name);
    for (const Name of COMPILER_PRODUCTS)
        await Capture(path.join(Compiler.directory, Name), MAXIMUM_IMAGE, null, 'Current compiler ' + Name);
    for (const Request of Bindings.Requests) for (const Item of Request.inputEvidence)
        await Capture(Item.path, MAXIMUM_CHUNK, Item.sha256, 'Project construction input');
    await Projectionˉproduct('Projected-Stager' + SUFFIX, MAXIMUM_IMAGE);
    State.segmentedConsumer = { stager: Inputs.find(Item => Sameˉpath(Item.path,
        path.join(Products, 'Projected-Stager' + SUFFIX))),
        linker: Inputs.find(Item => Sameˉpath(Item.path, Stagingˉlinker)),
        carrier: Inputs.find(Item => Sameˉpath(Item.path, path.resolve(Options.get('--carrier-wvb')))) };
    const Sourceˉprefix = path.join(Work, 'Module-Object');
    await writeFile(Sourceˉprefix + '.wvop', Manifest, { flag: 'wx' });
    for (let Index = 0; Index < Chunks.length; Index++)
        await writeFile(Sourceˉprefix + '.chunk-' + Index, Chunks[Index], { flag: 'wx' });
    for (const [Name, Value] of [['Module.wvb', Module], ['Admission.wvb', Admission], ['Carrier.wvb', Carrier]])
        await writeFile(path.join(Work, Name), Value, { flag: 'wx' });
    const Previous = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    Require(Previous === undefined || Previous === '1', 'Prepared verifier mode must be absent or1.');
    process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
    try {
        for (const Name of ['Module.wvb', 'Admission.wvb', 'Carrier.wvb'])
            await Step('admit-' + Name, process.execPath, [path.join(NATIVE, 'Verify-Wvb.mjs'), '--current', path.join(Work, Name)], true);
    } finally { if (Previous === undefined) delete process.env.WINDVALE_PREPARED_PRODUCTS_ONLY; else process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = Previous; }
    const Built = await Buildˉnativeˉcompanion({ Work, Step, Deadline: Options.Deadline, Sourceˉprefix, Stagingˉlinker,
        Carrier: path.join(Work, 'Carrier.wvb'), Admissionˉwvb: path.join(Work, 'Admission.wvb'),
        Runtimeˉpaths: RUNTIME_SOURCES.map(Name => path.join(Snapshots, Name)),
        Publicationˉentries: true, Sourceˉbytecode: Module,
        Publicationˉsource: path.join(Snapshots, 'Linker/Startup/X64-Shared-Compiler-Publication.wva'),
        Companion: () => ({ Path: path.join(Snapshots, 'Linker/Startup/X64-Shared-Compiler-Host.wva'), Entry: DRIVER_SYMBOL }) });
    const { Candidate, Packet, Providerˉpath, Finalˉconfiguration, Final } = Built;
    for (const Input of Inputs) {
        const Bytes = await Read(Input.path, Math.max(MAXIMUM_CHUNK, Input.bytes));
        Require(Bytes.length === Input.bytes && Hash(Bytes) === Input.sha256,
            'A declared host bridge input changed during construction: ' + Input.path);
    }
    await Preparedˉprojection.Requireˉunchanged(); await Compiler.Requireˉunchanged();
    for (const Request of Bindings.Requests) await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
    Require(await Getˉcurrentˉsplitˉcompilerˉkey() === Compilerˉkey &&
        JSON.stringify((await Constructionˉbindings()).Hostedˉproducers) === JSON.stringify(Bindings.Hostedˉproducers),
    'Compiler or hosted construction producers changed.');
    Require((await Read(Recordˉpath, 1_048_576)).equals(Recordˉbytes), 'Projection record changed during host construction.');
    const Product = await Read(Candidate, MAXIMUM_IMAGE);
    if (!WINDOWS) await chmod(Candidate, 0o755);
    await Absent(Output); await rename(Candidate, Output);
    State.status = 'Produced'; State.phase = 'bounded-consumer-execution-pending';
    State.inputs = Inputs;
    State.products.push({ path: Output, bytes: Product.length, sha256: Hash(Product) });
    State.providerPacket = { path: Providerˉpath, sha256: Hash(Packet) };
    State.configuration = { path: Finalˉconfiguration, sha256: Hash(Final) };
    await Save();
    const Hostˉrecord = path.join(Work, 'Host-Bridge.json');
    process.stdout.write('shared compiler host status=Produced target=' + TARGET + ' maximum-result-bytes=' +
        MAXIMUM_CHUNK + ' maximum-object-bytes=' + MAXIMUM_OBJECT_BYTES +
        ' maximum-staged-object-bytes=' + MAXIMUM_IMAGE +
        ' qualified=false self-lowering=pending workspace=' + Work +
        ' host-record=' + Hostˉrecord + ' host-record-sha256=' + Hash(await Read(Hostˉrecord, 1_048_576)) + '\n');
    } catch (Error) {
        State.status = 'Incomplete'; State.failure = Error.message.slice(0, 4096);
        await Save(); throw Error;
    }
}
if (process.argv[1] !== undefined && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
    Main(process.argv.slice(2)).catch(Error => {
        process.stderr.write('shared compiler host: ' + Error.message + '\n'); process.exitCode = Error.exitCode ?? 1;
    });
}
