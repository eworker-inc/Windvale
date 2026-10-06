import { createHash, randomBytes } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { chmod, copyFile, lstat, mkdir, open, opendir, readFile, realpath, rename, rm, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { REPOSITORY_ROOT } from './Native-Project-Cache-Key-Core.mjs';
import { Addˉhostedˉkeyˉfield } from './Native-Hosted-Application-Cache-Core.mjs';

// One construction step in the existing Project 4 graph. It never selects a
// fallback compiler or publishes a final current-six checkpoint.
const NAMESPACE = 'direct-condition-analyzer-v1';
const FORMAT = 'windvale-direct-condition-analyzer-checkpoint-1';
const HOST = `${process.platform}-${process.arch}`;
const WINDOWS = process.platform === 'win32';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const MAXIMUM_SOURCE_BYTES = 4_194_304;
const MAXIMUM_SNAPSHOT_BYTES = 16_777_216;
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_IMAGE_BYTES = 67_108_864;
const MAXIMUM_RECORD_BYTES = 65_536;
const MAXIMUM_TUPLE_BYTES = 536_870_912;
const PRODUCT_NAMES = ['Analyzer.wvb', 'Analyzer' + SUFFIX, 'Analyzer.identity'];
const ROLE_NAMES = ['Analyzer', 'Analyzerˉidentity', 'Emitter', 'Emitterˉidentity',
    'Reader', 'Admitter', 'Authenticator', 'Binder'];
const CORE_PATH = 'Compiler/Windvale/Source-Wir-Core.wv';
const SCALAR_PATH = 'Compiler/Windvale/Source-Wir-Scalar-Constants.wv';
const SELF_PATH = fileURLToPath(import.meta.url);
const Hash = Bytes => createHash('sha256').update(Bytes).digest('hex');
const Identity = Bytes => ({ bytes: Bytes.length, sha256: Hash(Bytes) });
const Require = (Value, Message) => { if (!Value) throw new Error(Message); };
const Sameˉpath = (Left, Right) => WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
const LOADED_SELF_IDENTITY = Identity(await readFile(SELF_PATH));
Require(LOADED_SELF_IDENTITY.bytes <= 1_048_576, 'Direct intermediate owner exceeds 1 MiB.');

export function Requireˉdirectˉconstruction(Construction) {
    Require(Construction?.Mode === 'prepare' && typeof Construction.Key === 'string' && /^[0-9a-f]{64}$/u.test(Construction.Key) &&
        typeof Construction.Family === 'string' && Number.isSafeInteger(Construction.Deadline) &&
        Construction.Deadline > 0 && typeof Construction.Requireˉunchanged === 'function',
    'Direct intermediate requires explicit preparation, current request identity and its inherited absolute deadline.');
    Requireˉtime(Construction.Deadline, 'construction');
    for (const Name of ['WINDVALE_PREPARED_COMPILER_ONLY', 'WINDVALE_PREPARED_PRODUCTS_ONLY']) {
        Require(process.env[Name] === undefined,
            `Direct intermediate preparation refuses inherited ${Name}; behavior checks never construct products.`);
    }
    Require(process.arch === 'x64' && (WINDOWS || process.platform === 'linux'), 'Unsupported direct intermediate host.');
}

function Requireˉtime(Deadline, Label) {
    if (Date.now() >= Deadline) throw Object.assign(new Error(`Direct intermediate deadline expired before ${Label}.`), { exitCode: 124 });
}

async function Directory(Candidate, Create = false) {
    const Resolved = path.resolve(Candidate);
    const Root = path.parse(Resolved).root;
    let Current = Root;
    for (const Component of Resolved.slice(Root.length).split(path.sep).filter(Boolean)) {
        Current = path.join(Current, Component);
        if (Create) await mkdir(Current).catch(Failure => { if (Failure.code !== 'EEXIST') throw Failure; });
        const Information = await lstat(Current);
        Require(Information.isDirectory() && !Information.isSymbolicLink(), 'Direct intermediate directory contains a link or non-directory.');
    }
    Require(Sameˉpath(await realpath(Resolved), Resolved), 'Direct intermediate directory is not canonical.');
    return Resolved;
}

async function Evidence(Candidate, Maximum, Deadline, Executable = false) {
    Requireˉtime(Deadline, 'input identity');
    const Resolved = path.resolve(Candidate);
    await Directory(path.dirname(Resolved));
    const Information = await lstat(Resolved);
    Require(Information.isFile() && !Information.isSymbolicLink() && Information.size > 0 &&
        Information.size <= Maximum && (!Executable || WINDOWS || (Information.mode & 0o111) !== 0) &&
        Sameˉpath(await realpath(Resolved), Resolved), `Direct intermediate input is not a bounded ordinary file: ${Resolved}`);
    const Digest = createHash('sha256');
    let Bytes = 0;
    for await (const Chunk of createReadStream(Resolved, { highWaterMark: 1_048_576 })) {
        Requireˉtime(Deadline, 'input identity');
        Bytes += Chunk.length;
        Require(Bytes <= Information.size, 'Direct intermediate input grew while hashed.');
        Digest.update(Chunk);
    }
    Require(Bytes === Information.size, 'Direct intermediate input changed while hashed.');
    return { bytes: Bytes, sha256: Digest.digest('hex') };
}

async function Payload(Candidate, Maximum, Deadline) {
    const Original = await Evidence(Candidate, Maximum, Deadline);
    const Bytes = await readFile(Candidate);
    Require(JSON.stringify(Identity(Bytes)) === JSON.stringify(Original), 'Direct intermediate input changed while read.');
    Requireˉtime(Deadline, 'source input');
    return Bytes;
}

function Canonicalˉrelative(Value) {
    Require(typeof Value === 'string' && !Value.includes('\\') && !Value.includes(':') && !path.posix.isAbsolute(Value) &&
        Value.split('/').every(Part => Part !== '' && Part !== '.' && Part !== '..'), 'Direct intermediate declared path is not canonical.');
    return Value;
}

function Origin(Value) {
    Require(Value?.kind === 'source-edition-predecessor-1' && /^[0-9a-f]{40}$/u.test(Value.revision) &&
        /^[0-9a-f]{40}$/u.test(Value.tree) && /^[0-9a-f]{64}$/u.test(Value.key) && Value.host === HOST &&
        Object.keys(Value).sort().join(',') === 'host,key,kind,revision,tree',
    'Direct intermediate requires the actual source-edition predecessor request metadata.');
    return { kind: Value.kind, revision: Value.revision, tree: Value.tree, key: Value.key, host: Value.host };
}

function Producerˉidentity(Role, Product) {
    return Buffer.from(`windvale-split-compiler-producer 2\nrole ${Role}\n` +
        `target ${Role === 'analyzer' ? 'source-analysis-v1' : 'portable-wvb-optimized-v1'}\n` +
        `host ${HOST}\nbytes ${Product.bytes}\nsha256 ${Product.sha256}\n`, 'ascii');
}

async function Tuple(Predecessor, Deadline) {
    const Values = [];
    let Total = 0;
    for (const [Index, Name] of ROLE_NAMES.entries()) {
        Require(typeof Predecessor?.[Name] === 'string', `Missing direct intermediate predecessor ${Name}.`);
        const Isˉidentity = Name.endsWith('ˉidentity');
        const Value = await Evidence(Predecessor[Name], Isˉidentity ? 1_024 : MAXIMUM_IMAGE_BYTES, Deadline, !Isˉidentity);
        Total += Value.bytes;
        Require(Total <= MAXIMUM_TUPLE_BYTES, 'Direct intermediate predecessor tuple exceeds 512 MiB.');
        Values.push({ name: Name, ...Value });
        console.log(`direct intermediate step=predecessor-identity item=${Index + 1}/${ROLE_NAMES.length}`);
    }
    for (const [Role, Productˉindex, Identityˉindex] of [['analyzer', 0, 1], ['emitter', 2, 3]]) {
        Require((await Payload(Predecessor[ROLE_NAMES[Identityˉindex]], 1_024, Deadline)).equals(
            Producerˉidentity(Role, Values[Productˉindex])), 'Direct intermediate predecessor producer identity is not canonical.');
    }
    return Values;
}

async function Sources(Analysisˉproject, Deadline) {
    Require(path.basename(Analysisˉproject) === 'Windvale-Compiler-Analysis-Driver.wvproj', 'Unexpected direct intermediate analysis project.');
    // Physical work paths are not producer identities. The explicit constructor
    // project-directory seam may supply a byte-identical reviewed manifest.
    const Relativeˉproject = 'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj';
    const Project = await Payload(Analysisˉproject, 65_536, Deadline);
    Require(Project.equals(await Payload(path.join(REPOSITORY_ROOT, ...Relativeˉproject.split('/')), 65_536, Deadline)),
        'Direct intermediate analysis manifest differs from the selected current repository project.');
    const Text = Exactˉtext(Project, 65_536);
    Require(Text.startsWith('windvale-project 4\n'), 'Direct intermediate requires an authenticated Project 4 manifest.');
    const Declared = [];
    const Sourceˉpaths = [];
    const Admission = new Set();
    let Roots = 0;
    for (const Line of Text.split('\n')) {
        const Match = /^(root|source|source-input-lock|source-profile|target-descriptor) "([^"\r\n]+)"$/u.exec(Line);
        if (Match === null) {
            Require(!/^(root|source|source-input-lock|source-profile|target-descriptor)(?:\s|$)/u.test(Line), 'Malformed direct intermediate input declaration.');
            continue;
        }
        const Relative = Canonicalˉrelative(Match[2]);
        if (Match[1] === 'root' || Match[1] === 'source') {
            Roots += Match[1] === 'root' ? 1 : 0;
            Sourceˉpaths.push(Relative);
        } else {
            Require(!Admission.has(Match[1]), 'Repeated direct intermediate admission input.');
            Admission.add(Match[1]);
        }
        Declared.push(Relative);
        Require(Declared.length <= 62, 'Direct intermediate exceeds 64 snapshot files.');
    }
    Require(Roots === 1 && Admission.size === 3 && Sourceˉpaths.length === 24 &&
        Sourceˉpaths.includes(CORE_PATH) && Sourceˉpaths.includes(SCALAR_PATH) &&
        new Set([...Declared, Relativeˉproject, 'Windvale.wvws']).size === Declared.length + 2,
    'Direct intermediate source/admission closure differs.');
    const Inputs = [{ relative: 'Windvale.wvws', payload: await Payload(path.join(REPOSITORY_ROOT, 'Windvale.wvws'), 65_536, Deadline) },
        { relative: Relativeˉproject, path: path.resolve(Analysisˉproject), payload: Project }];
    let Total = Inputs.reduce((Sum, Value) => Sum + Value.payload.length, 0);
    let Sourceˉbytes = 16 + Sourceˉpaths.length * 8;
    for (const Relative of Declared) {
        const Candidate = path.join(REPOSITORY_ROOT, ...Relative.split('/'));
        const Information = await lstat(Candidate);
        Require(Information.size <= MAXIMUM_SOURCE_BYTES && Total + Information.size <= MAXIMUM_SNAPSHOT_BYTES,
            'Direct intermediate source snapshot exceeds its finite input bound.');
        const Bytes = await Payload(Candidate, MAXIMUM_SOURCE_BYTES, Deadline);
        Total += Bytes.length;
        if (Sourceˉpaths.includes(Relative)) Sourceˉbytes += Bytes.length;
        Require(Sourceˉbytes <= MAXIMUM_SOURCE_BYTES, 'Direct intermediate original source set exceeds 4 MiB.');
        Inputs.push({ relative: Relative, payload: Bytes });
    }
    const Adapted = Projectˉdirectˉconditionˉanalyzer(Inputs.find(Value => Value.relative === CORE_PATH).payload, Project);
    const Snapshot = Inputs.filter(Value => Value.relative !== SCALAR_PATH).map(Value => ({ relative: Value.relative,
        payload: Value.relative === CORE_PATH ? Adapted.Core : Value.relative === Relativeˉproject ? Adapted.Project : Value.payload }));
    const Rows = Values => Values.map(Value => ({ relative: Value.relative, ...Identity(Value.payload) }));
    return { Inputs, Snapshot, Relativeˉproject, Original: Rows(Inputs), Derived: Rows(Snapshot), Selectors: Adapted.Selectors };
}

async function Requireˉinputs(Inputs, Deadline, Root = REPOSITORY_ROOT) {
    for (const Value of Inputs) {
        const Candidate = Sameˉpath(Root, REPOSITORY_ROOT) && Value.path !== undefined ? Value.path : path.join(Root, ...Value.relative.split('/'));
        Require(JSON.stringify(await Evidence(Candidate, MAXIMUM_SOURCE_BYTES, Deadline)) ===
            JSON.stringify(Identity(Value.payload)), 'Direct intermediate source input changed.');
    }
}

async function Products(Place, Deadline) {
    await Directory(Place);
    const Values = [];
    for (const Name of PRODUCT_NAMES) Values.push({ name: Name, ...await Evidence(path.join(Place, Name),
        Name.endsWith('.identity') ? 1_024 : Name.endsWith('.wvb') ? MAXIMUM_WVB_BYTES : MAXIMUM_IMAGE_BYTES,
        Deadline, !Name.endsWith('.identity') && !Name.endsWith('.wvb')) });
    const Wvb = await open(path.join(Place, PRODUCT_NAMES[0]), 'r');
    try {
        const Header = Buffer.alloc(8);
        Require((await Wvb.read(Header, 0, Header.length, 0)).bytesRead === Header.length &&
            Header.subarray(0, 4).equals(Buffer.from('WVB1')) && Header.readUInt16LE(4) === 1 && Header.readUInt16LE(6) === 11,
        'Direct intermediate source epoch requires exact WVB 1.11.');
    } finally { await Wvb.close(); }
    Require((await Payload(path.join(Place, PRODUCT_NAMES[2]), 1_024, Deadline)).equals(Producerˉidentity('analyzer', Values[1])),
        'Direct intermediate Analyzer identity differs from its native product.');
    return Values;
}

function Record(Key, Request, Values) {
    const Bytes = Buffer.from(JSON.stringify({ format: FORMAT, key: Key, host: HOST, request: Request, products: Values }) + '\n');
    Require(Bytes.length <= MAXIMUM_RECORD_BYTES, 'Direct intermediate checkpoint record exceeds 64 KiB.');
    return Bytes;
}

async function Validate(Place, Key, Request, Deadline) {
    await Directory(Place);
    const Names = [];
    for await (const Entry of await opendir(Place)) {
        Names.push(Entry.name);
        Require(Names.length <= PRODUCT_NAMES.length + 1, 'Direct intermediate checkpoint inventory exceeds its bound.');
    }
    Require(Names.length === PRODUCT_NAMES.length + 1 && JSON.stringify(Names.sort()) ===
        JSON.stringify([...PRODUCT_NAMES, 'Checkpoint.json'].sort()), 'Direct intermediate checkpoint inventory differs.');
    const Values = await Products(Place, Deadline);
    Require((await Payload(path.join(Place, 'Checkpoint.json'), MAXIMUM_RECORD_BYTES, Deadline)).equals(Record(Key, Request, Values)),
        'Direct intermediate checkpoint request or product record differs.');
    return Values;
}

async function Exists(Candidate) {
    return lstat(Candidate).then(() => true).catch(Failure => { if (Failure.code === 'ENOENT') return false; throw Failure; });
}

export async function Acquireˉdirectˉconditionˉanalyzer({ Work, Predecessor, Runˉnative, Runˉnode, Construction, Analysisˉproject }) {
    Requireˉdirectˉconstruction(Construction);
    Require(typeof Runˉnative === 'function' && typeof Runˉnode === 'function', 'Direct intermediate needs existing construction callbacks.');
    const Deadline = Construction.Deadline;
    Work = await Directory(Work);
    const Currentˉfamily = await Directory(Construction.Family);
    Require(path.basename(Currentˉfamily) === HOST && path.basename(path.dirname(Currentˉfamily)) === 'current-split-compiler-v2',
        'Direct intermediate requires the selected current compiler cache family.');
    const Metadata = Origin(Predecessor.Construction);
    const Values = await Tuple(Predecessor, Deadline);
    const Source = await Sources(Analysisˉproject, Deadline);
    const Self = await Evidence(SELF_PATH, 1_048_576, Deadline);
    Require(JSON.stringify(Self) === JSON.stringify(LOADED_SELF_IDENTITY), 'Loaded direct intermediate owner bytes changed before construction.');
    const Node = await Evidence(process.execPath, 134_217_728, Deadline, true);
    const { Readˉbootstrapˉverifier } = await import('./Build-Cached-Segmented-Hosted-Wvb.mjs');
    const Verifier = (await Readˉbootstrapˉverifier())?.identity ?? null;
    const Request = { format: FORMAT, host: HOST, currentKey: Construction.Key, profile: '8', selectorVersion: DIRECT_CONDITION_SELECTOR_VERSION,
        node: { version: process.version, ...Node }, owner: Self, predecessor: Metadata, roles: Values, verifier: Verifier,
        project: Source.Relativeˉproject, original: Source.Original, derived: Source.Derived, selectors: Source.Selectors,
        commands: ['Run-Split-Compiler.mjs authenticated-project4 source-emission-deadline-600000', 'Package-Segmented-Compiler-Wvb profile8 development-cache',
            'Write-Split-Compiler-Producer-Identity.mjs analyzer'] };
    const Requestˉhash = createHash('sha256');
    Addˉhostedˉkeyˉfield(Requestˉhash, 'format', Buffer.from(FORMAT));
    Addˉhostedˉkeyˉfield(Requestˉhash, 'request', Buffer.from(JSON.stringify(Request)));
    const Key = Requestˉhash.digest('hex');
    const Family = await Directory(path.join(path.dirname(path.dirname(Currentˉfamily)), NAMESPACE, HOST), true);
    const Place = path.join(Family, Key);
    async function Unchanged(Snapshot = null) {
        Requireˉtime(Deadline, 'unchanged inputs');
        await Construction.Requireˉunchanged();
        Require(JSON.stringify(Origin(Predecessor.Construction)) === JSON.stringify(Metadata), 'Direct predecessor origin changed.');
        Require(JSON.stringify(await Tuple(Predecessor, Deadline)) === JSON.stringify(Values), 'Direct predecessor product changed.');
        await Requireˉinputs(Source.Inputs, Deadline);
        if (Snapshot !== null) await Requireˉinputs(Source.Snapshot, Deadline, Snapshot);
        Require(JSON.stringify(await Evidence(SELF_PATH, 1_048_576, Deadline)) === JSON.stringify(Self) &&
            JSON.stringify(await Evidence(process.execPath, 134_217_728, Deadline, true)) === JSON.stringify(Node) &&
            JSON.stringify((await Readˉbootstrapˉverifier())?.identity ?? null) === JSON.stringify(Verifier),
        'Direct intermediate owner, Node or complete-verifier identity changed.');
        Requireˉtime(Deadline, 'unchanged inputs');
    }
    await Unchanged();
    if (await Exists(Place)) {
        await Validate(Place, Key, Request, Deadline);
        await Unchanged();
        console.log(`direct intermediate status=Hit key=${Key}`);
        return { Analyzer: path.join(Place, PRODUCT_NAMES[1]), Analyzerˉidentity: path.join(Place, PRODUCT_NAMES[2]), key: Key, request: Request };
    }
    const Stepˉwork = path.join(Work, 'Direct-Condition-Intermediate');
    Require(!await Exists(Stepˉwork), 'Direct intermediate private work already exists.');
    await mkdir(Stepˉwork);
    const Snapshot = await Directory(path.join(Stepˉwork, 'Source'), true);
    for (const [Index, Value] of Source.Snapshot.entries()) {
        Requireˉtime(Deadline, 'source snapshot materialization');
        const Destination = path.join(Snapshot, ...Value.relative.split('/'));
        await Directory(path.dirname(Destination), true);
        await writeFile(Destination, Value.payload, { flag: 'wx', mode: 0o600 });
        console.log(`direct intermediate step=source-snapshot item=${Index + 1}/${Source.Snapshot.length}`);
    }
    const Wvb = path.join(Stepˉwork, PRODUCT_NAMES[0]);
    const Product = path.join(Stepˉwork, PRODUCT_NAMES[1]);
    const Productˉidentity = path.join(Stepˉwork, PRODUCT_NAMES[2]);
    const Temporary = path.join(Family, `.new-${Key}-${process.pid}-${randomBytes(16).toString('hex')}`);
    let Failure = null;
    await mkdir(Temporary);
    try {
        for (const [Label, Invoke] of [
            ['direct-condition-analyzer-source', () => {
                const Sourceˉdeadline = Math.min(Date.now() + 600_000, Deadline);
                return Runˉnode('direct-condition-analyzer-source', 'Run-Split-Compiler.mjs', [
                    Predecessor.Admitter, Predecessor.Authenticator, Predecessor.Analyzer, Predecessor.Emitter,
                    '--foreign-binder', Predecessor.Binder, '--workspace', path.join(Snapshot, 'Windvale.wvws'),
                    '--project', path.join(Snapshot, ...Source.Relativeˉproject.split('/')), '--manifest-reader', Predecessor.Reader,
                    '--source-emission-deadline-ms', String(Sourceˉdeadline), Wvb]);
            }],
            ['direct-condition-analyzer-package', () => Runˉnative('direct-condition-analyzer-package', 'Package-Segmented-Compiler-Wvb',
                ['8', Wvb, Product, '--development-cache'])],
            ['direct-condition-analyzer-identity', () => Runˉnode('direct-condition-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs',
                ['analyzer', Product, Productˉidentity])],
        ]) {
            await Unchanged(Snapshot);
            Requireˉtime(Deadline, Label);
            await Invoke();
            await Unchanged(Snapshot);
        }
        const Produced = await Products(Stepˉwork, Deadline);
        for (const Name of PRODUCT_NAMES) {
            await copyFile(path.join(Stepˉwork, Name), path.join(Temporary, Name));
            // Run-Split publishes a read-only WVB; this private candidate must
            // remain writable until its bounded publication sync completes.
            if (Name.endsWith('.wvb')) await chmod(path.join(Temporary, Name), 0o600);
        }
        await writeFile(path.join(Temporary, 'Checkpoint.json'), Record(Key, Request, Produced), { flag: 'wx' });
        for (const Name of [...PRODUCT_NAMES, 'Checkpoint.json']) {
            const Handle = await open(path.join(Temporary, Name), 'r+');
            try { await Handle.sync(); } finally { await Handle.close(); }
        }
        await Validate(Temporary, Key, Request, Deadline);
        await Unchanged(Snapshot);
        let Status = 'Created';
        try { await rename(Temporary, Place); } catch (Problem) {
            if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(Problem.code)) throw Problem;
            Status = 'Hit';
        }
        const Published = await Validate(Place, Key, Request, Deadline);
        Require(Record(Key, Request, Published).equals(Record(Key, Request, Produced)), 'Concurrent direct intermediate products differ.');
        await Unchanged(Snapshot);
        console.log(`direct intermediate status=${Status} key=${Key} predecessor=${Metadata.key}`);
        return { Analyzer: path.join(Place, PRODUCT_NAMES[1]), Analyzerˉidentity: path.join(Place, PRODUCT_NAMES[2]), key: Key, request: Request };
    } catch (Problem) { Failure = Problem; throw Problem; }
    finally {
        // Producer uncertainty is handled by the enclosing existing constructor;
        // never delete a path possibly still used by that producer.
        if (Failure?.cleanupUncertain === true) Failure.intermediateWork = Stepˉwork;
        else {
            Require(path.dirname(path.resolve(Temporary)) === Family && path.basename(Temporary).startsWith(`.new-${Key}-`),
                'Refusing unexpected direct intermediate cache cleanup.');
            if (await Exists(Temporary)) { await Directory(Temporary); await rm(Temporary, { recursive: true, force: true, maxRetries: 2 }); }
        }
    }
}

const SCALAR_IMPORT = 'import Compilerˉsourceˉwirˉscalarˉconstants as Scalarˉconstants;\n';
const SCALAR_SOURCE_LINE = 'source "Compiler/Windvale/Source-Wir-Scalar-Constants.wv"\n';
const PAYLOAD_HEADER = 'fn Compilerˉsourceˉwirˉappendˉfunctionˉentryˉpayload(';
const DIRECT_HEADER = 'export fn Compilerˉsourceˉwirˉappendˉfunctionˉentry(';
export const DIRECT_CONDITION_SELECTOR_VERSION = 'direct-condition-analyzer-source-1';
export const DIRECT_CONDITION_SELECTORS = Object.freeze([
    Object.freeze({ name: 'scalar-import', bytes: 70, sha256: '6e0e0aed42c6d5db8f22ff26b18f985fc2410e21834aeb3a0ea10ac183ead456' }),
    Object.freeze({ name: 'scalar-finalization-integration', bytes: 7539, sha256: '2dd5e7ca4f9dd041a1690bf5ab941e09f4723ea1b9bd93955ed975d646632a0b', replacementBytes: 3264, replacementSha256: '9df7e0ba31a94c315dc3372fa6f5572297eb638893436d7b92a8ea2904ade1a0' }),
    Object.freeze({ name: 'scalar-module-manifest-entry', bytes: 58, sha256: 'd0d2b969475910aee92a1cc5aa616b272293e8a5e9b6301a4bbe9a0ad2bcbfc0' }),
]);

function Exactˉtext(Bytes, Maximum) {
    Require(Buffer.isBuffer(Bytes) && Bytes.length > 0 && Bytes.length <= Maximum, 'Bounded source input required.');
    const Text = Bytes.toString('utf8');
    Require(Buffer.from(Text).equals(Bytes) && !Text.includes('\r') && !Text.startsWith('\ufeff'), 'Exact LF UTF8 source required.');
    return Text;
}

function Unique(Text, Needle) {
    const At = Text.indexOf(Needle);
    Require(At >= 0 && Text.indexOf(Needle, At + Needle.length) < 0, 'Missing or repeated exact direct-condition selector.');
    return At;
}

function Functionˉextent(Text, Name) {
    const Escaped = Name.replace(/[.*+?^${}()|[\]\\]/gu, '\\$&');
    const Matches = [...Text.matchAll(new RegExp('^(?:export )?fn ' + Escaped + '\\s*\\(', 'gmu'))];
    Require(Matches.length === 1, 'Missing or repeated function selector: ' + Name);
    const Start = Matches[0].index;
    let Index = Text.indexOf('{', Start), Depth = 0, Quote = '', Line = false, Comment = false;
    Require(Index >= Start, 'Missing selected function body.');
    for (; Index < Text.length; Index += 1) {
        const Character = Text[Index], Next = Text[Index + 1];
        if (Line) { if (Character === '\n') Line = false; continue; }
        if (Comment) { if (Character === '*' && Next === '/') { Comment = false; Index += 1; } continue; }
        if (Quote !== '') { if (Character === '\\') Index += 1; else if (Character === Quote) Quote = ''; continue; }
        if (Character === '/' && Next === '/') { Line = true; Index += 1; continue; }
        if (Character === '/' && Next === '*') { Comment = true; Index += 1; continue; }
        if (Character === '"' || Character === "'") { Quote = Character; continue; }
        if (Character === '{') { Depth += 1; Require(Depth <= 256, 'Selected function nesting bound.'); }
        if (Character === '}') {
            Require(Depth > 0, 'Unbalanced selected function.');
            Depth -= 1;
            if (Depth === 0) {
                Require(Text.slice(Index + 1, Index + 3) === '\n\n', 'Selected function boundary drift.');
                return { start: Start, end: Index + 3 };
            }
        }
    }
    throw new Error('Unclosed selected function.');
}

function Matchˉidentity(Bytes, Selector, Replacement = false) {
    Require(Bytes.length === Selector[Replacement ? 'replacementBytes' : 'bytes'] &&
        Hash(Bytes) === Selector[Replacement ? 'replacementSha256' : 'sha256'], 'Exact selected body changed: ' + Selector.name);
}

export function Projectˉdirectˉconditionˉanalyzer(Coreˉbytes, Projectˉbytes) {
    const Core = Exactˉtext(Coreˉbytes, MAXIMUM_SOURCE_BYTES), Project = Exactˉtext(Projectˉbytes, 65_536);
    Require(Project.startsWith('windvale-project 4\n'), 'Direct intermediate requires current authenticated Project 4.');
    const Importˉat = Unique(Core, SCALAR_IMPORT), Manifestˉat = Unique(Project, SCALAR_SOURCE_LINE);
    const Pool = Functionˉextent(Core, 'Compilerˉsourceˉwirˉpoolˉfunction');
    const Wrapper = Functionˉextent(Core, 'Compilerˉsourceˉwirˉappendˉfunctionˉentry');
    const Payloadˉextent = Functionˉextent(Core, 'Compilerˉsourceˉwirˉappendˉfunctionˉentryˉpayload');
    Require(Importˉat < Pool.start && Pool.end === Wrapper.start && Wrapper.end === Payloadˉextent.start,
        'Direct intermediate selected functions are not contiguous.');
    const Before = Core.slice(Pool.start, Payloadˉextent.end), Payloadˉtext = Core.slice(Payloadˉextent.start, Payloadˉextent.end);
    Require(Payloadˉtext.startsWith(PAYLOAD_HEADER), 'Selected private payload header changed.');
    const Direct = DIRECT_HEADER + Payloadˉtext.slice(PAYLOAD_HEADER.length);
    Matchˉidentity(Buffer.from(SCALAR_IMPORT), DIRECT_CONDITION_SELECTORS[0]);
    Matchˉidentity(Buffer.from(Before), DIRECT_CONDITION_SELECTORS[1]);
    Matchˉidentity(Buffer.from(Direct), DIRECT_CONDITION_SELECTORS[1], true);
    Matchˉidentity(Buffer.from(SCALAR_SOURCE_LINE), DIRECT_CONDITION_SELECTORS[2]);
    const Adaptedˉcore = Core.slice(0, Importˉat) + Core.slice(Importˉat + SCALAR_IMPORT.length, Pool.start) + Direct + Core.slice(Payloadˉextent.end);
    const Adaptedˉproject = Project.slice(0, Manifestˉat) + Project.slice(Manifestˉat + SCALAR_SOURCE_LINE.length);
    Require(!Adaptedˉcore.includes('Scalarˉconstants') && !Adaptedˉcore.includes('Compilerˉsourceˉwirˉpoolˉfunction') &&
        !Adaptedˉcore.includes('Compilerˉsourceˉwirˉappendˉfunctionˉentryˉpayload'), 'Scalar integration remains in direct intermediate.');
    const Directˉat = Unique(Adaptedˉcore, Direct);
    const Restore = Adaptedˉcore.slice(0, Directˉat) + Before + Adaptedˉcore.slice(Directˉat + Direct.length);
    Require(Restore.slice(0, Importˉat) + SCALAR_IMPORT + Restore.slice(Importˉat) === Core &&
        Adaptedˉproject.slice(0, Manifestˉat) + SCALAR_SOURCE_LINE + Adaptedˉproject.slice(Manifestˉat) === Project,
    'Direct intermediate inverse is not byte exact.');
    const Sourceˉpaths = [...Adaptedˉproject.matchAll(/^(?:root|source) "([^"\r\n]+)"$/gmu)].map(Match => Match[1]);
    Require(Sourceˉpaths.length === 23 && new Set(Sourceˉpaths).size === Sourceˉpaths.length && !Sourceˉpaths.includes(SCALAR_PATH),
        'Direct intermediate source closure drift.');
    return { Core: Buffer.from(Adaptedˉcore), Project: Buffer.from(Adaptedˉproject), Selectors: DIRECT_CONDITION_SELECTORS, Inverseˉexact: true };
}
