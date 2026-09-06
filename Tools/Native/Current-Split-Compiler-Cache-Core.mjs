import { createHash, randomBytes } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { copyFile, lstat, mkdir, opendir, readFile, realpath, rename, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import {
    Getˉnativeˉprojectˉcacheˉrequest,
    Prepareˉnativeˉprojectˉcacheˉcontext,
    REPOSITORY_ROOT,
} from './Native-Project-Cache-Key-Core.mjs';
import {
    Addˉhostedˉkeyˉfield,
    Prepareˉhostedˉapplicationˉcontext,
} from './Native-Hosted-Application-Cache-Core.mjs';

const NAMESPACE = 'current-split-compiler-v1';
const HOST = `${process.platform}-${process.arch}`;
const WINDOWS = process.platform === 'win32';
const EXTENSION = WINDOWS ? 'exe' : 'elf';
const MAXIMUM_INPUT_BYTES = 134_217_728;
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_RECORD_BYTES = 4_096;
const FORMAT = 'windvale-current-split-compiler-checkpoint-1';
const PRODUCT_NAMES = Object.freeze([
    `Analyzer.${EXTENSION}`, 'Analyzer.identity',
    `Emitter.${EXTENSION}`, 'Emitter.identity',
]);

function Reject(Message) { throw new Error(Message); }
function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}
function Field(Hash, Name, Value) {
    Addˉhostedˉkeyˉfield(Hash, Name, Buffer.from(Value, 'utf8'));
}

async function Directory(Candidate, Create = false) {
    const Resolved = path.resolve(Candidate);
    const Root = path.parse(Resolved).root;
    let Current = Root;
    for (const Part of Resolved.slice(Root.length).split(path.sep).filter(Boolean)) {
        Current = path.join(Current, Part);
        if (Create) await mkdir(Current).catch(Error => {
            if (Error.code !== 'EEXIST') throw Error;
        });
        const Information = await lstat(Current);
        if (!Information.isDirectory() || Information.isSymbolicLink()) {
            Reject('The compiler checkpoint directory contains a link or non-directory.');
        }
    }
    if (!Sameˉpath(await realpath(Resolved), Resolved)) {
        Reject('The compiler checkpoint directory is not canonical.');
    }
    return Resolved;
}

async function Evidence(Candidate, Maximum = MAXIMUM_INPUT_BYTES, Executable = false) {
    const Resolved = path.resolve(Candidate);
    const Information = await lstat(Resolved);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        (Executable && !WINDOWS && (Information.mode & 0o111) === 0) ||
        !Sameˉpath(await realpath(Resolved), Resolved)) {
        Reject(`The compiler checkpoint input is not a bounded ordinary file: ${Candidate}`);
    }
    const Hash = createHash('sha256');
    let Bytes = 0;
    for await (const Chunk of createReadStream(Resolved, { highWaterMark: 1_048_576 })) {
        Bytes += Chunk.length;
        if (Bytes > Information.size) Reject('A compiler checkpoint input grew while read.');
        Hash.update(Chunk);
    }
    if (Bytes !== Information.size) Reject('A compiler checkpoint input changed while read.');
    return { bytes: Bytes, sha256: Hash.digest('hex') };
}

// Request identity describes construction, independently of requested test products.
export async function Getˉcurrentˉsplitˉcompilerˉkey() {
    const Wrapper = WINDOWS ? 'cmd' : 'sh';
    const Hostˉfamily = WINDOWS ? 'windows-x64' : 'linux-x64';
    const Hash = createHash('sha256');
    Field(Hash, 'format', FORMAT);
    Field(Hash, 'host', HOST);
    Field(Hash, 'node', process.version);
    Field(Hash, 'node-binary', JSON.stringify(await Evidence(process.execPath)));
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext(NAMESPACE, [
        'Current-Split-Compiler-Cache-Core.mjs',
        'Build-Current-Split-Project-Wvb.mjs',
        'Development-Command-Core.mjs',
        'Build-Cached-Split-Project-Wvb.mjs',
        'Native-Project-Cache-Key-Core.mjs',
        'Split-Project-Source-Ordering-Core.mjs',
        'Write-Split-Compiler-Producer-Identity.mjs',
    ].map(Name => path.join(REPOSITORY_ROOT, 'Tools', 'Native', Name)));
    for (const Name of ['Analysis', 'Emission']) {
        const Request = await Getˉnativeˉprojectˉcacheˉrequest(Context, path.join(
            REPOSITORY_ROOT, 'Projects', 'Tools', `Windvale-Compiler-${Name}-Driver.wvproj`));
        Field(Hash, `compiler:${Name}`, Request.key);
    }
    const Producers = [
        'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs',
        'Tools/Native/Build-Cached-Segmented-Project.mjs',
        'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.cmd',
        'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.sh',
        ...['Verify-Wvb', 'Package-Segmented-Compiler-Wvb', 'Stage-Compiler-Wvb',
            'Link-Staged-Compiler-Wvo', 'Transport-Compiler-Image']
            .map(Name => `Tools/Native/${Name}.${Wrapper}`),
        `Artifacts/Native-Front-Door/${Hostˉfamily}/wvverify.${EXTENSION}`,
        ...['wvstage', 'wvlinkstage', 'wvimagetransport'].map(Name =>
            `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${Hostˉfamily}-${Name}.${EXTENSION}`),
        ...['wvanalyze', 'wvemit'].map(Name =>
            `Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/${Name}.wvb`),
    ];
    for (const Relative of Producers) {
        Field(Hash, `producer:${Relative}`, JSON.stringify(
            await Evidence(path.join(REPOSITORY_ROOT, Relative))));
    }
    const Hosted = await Prepareˉhostedˉapplicationˉcontext(
        WINDOWS ? 'windows' : 'linux',
        path.join(REPOSITORY_ROOT, 'Tools', 'Native', `Package-Hosted-Wvb.${Wrapper}`));
    for (const Producer of Hosted.producerFields) {
        Addˉhostedˉkeyˉfield(Hash, Producer.label, Producer.bytes);
    }
    return Hash.digest('hex');
}

export async function Getˉcurrentˉsplitˉcompilerˉfamily() {
    const Configured = process.env.WINDVALE_NATIVE_CACHE_ROOT;
    let Root = Configured;
    if (!Root) {
        if (WINDOWS) {
            if (!process.env.LOCALAPPDATA) Reject('LOCALAPPDATA is unavailable.');
            Root = path.join(process.env.LOCALAPPDATA, 'Windvale', 'Native-Tool-Cache');
        } else {
            Root = path.join(process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache'),
                'windvale', 'native-tool-cache');
        }
    }
    return Directory(path.join(Root, NAMESPACE, HOST), true);
}

function Identity(Role, Product) {
    const Target = Role === 'analyzer' ? 'source-analysis-v1' : 'portable-wvb-optimized-v1';
    return Buffer.from(`windvale-split-compiler-producer 2\nrole ${Role}\n` +
        `target ${Target}\nhost ${HOST}\nbytes ${Product.bytes}\nsha256 ${Product.sha256}\n`);
}

async function Products(Place) {
    await Directory(Place);
    const Values = [];
    for (const Name of PRODUCT_NAMES) {
        Values.push({ name: Name, ...await Evidence(path.join(Place, Name),
            Name.endsWith('.identity') ? 1_024 : MAXIMUM_PRODUCT_BYTES,
            !Name.endsWith('.identity')) });
    }
    for (const [Index, Role] of [[0, 'analyzer'], [2, 'emitter']]) {
        const Actual = await readFile(path.join(Place, PRODUCT_NAMES[Index + 1]));
        if (!Actual.equals(Identity(Role, Values[Index]))) {
            Reject(`The current compiler ${Role} identity differs from its product.`);
        }
    }
    return Values;
}

function Record(Key, Values) {
    return Buffer.from(JSON.stringify({ format: FORMAT, key: Key, host: HOST, products: Values }) + '\n');
}

async function Validate(Place, Key) {
    await Directory(Place);
    const Names = [];
    for await (const Entry of await opendir(Place)) {
        Names.push(Entry.name);
        if (Names.length > PRODUCT_NAMES.length + 1) {
            Reject('The compiler checkpoint exceeds its product inventory bound.');
        }
    }
    Names.sort();
    if (JSON.stringify(Names) !== JSON.stringify([...PRODUCT_NAMES, 'Checkpoint.json'].sort())) {
        Reject('The compiler checkpoint product inventory differs.');
    }
    const Values = await Products(Place);
    const Recordˉpath = path.join(Place, 'Checkpoint.json');
    await Evidence(Recordˉpath, MAXIMUM_RECORD_BYTES);
    if (!(await readFile(Recordˉpath)).equals(Record(Key, Values))) {
        Reject('The current compiler checkpoint record differs.');
    }
    return Values;
}

async function Exists(Candidate) {
    return lstat(Candidate).then(() => true).catch(Error => {
        if (Error.code === 'ENOENT') return false;
        throw Error;
    });
}

// The producer writes only the four final files; intermediates retain their own owners.
export async function Acquireˉcurrentˉsplitˉcompiler(Family, Key, Produce, Requireˉunchanged) {
    if (typeof Key !== 'string' || !/^[0-9a-f]{64}$/.test(Key) || typeof Produce !== 'function' ||
        typeof Requireˉunchanged !== 'function') Reject('The compiler checkpoint request is invalid.');
    Family = await Directory(Family);
    const Place = path.join(Family, Key);
    if (await Exists(Place)) {
        await Validate(Place, Key);
        await Requireˉunchanged();
        return { directory: Place, status: 'Hit' };
    }
    const Temporary = path.join(Family, `.new-${Key}-${process.pid}-${randomBytes(16).toString('hex')}`);
    await mkdir(Temporary);
    try {
        await Produce(Temporary);
        const Values = await Products(Temporary);
        await Requireˉunchanged();
        await writeFile(path.join(Temporary, 'Checkpoint.json'), Record(Key, Values), { flag: 'wx' });
        await Validate(Temporary, Key);
        let Status = 'Created';
        try { await rename(Temporary, Place); }
        catch (Error) {
            if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(Error.code)) throw Error;
            Status = 'Hit';
        }
        const Published = await Validate(Place, Key);
        if (!Record(Key, Published).equals(Record(Key, Values))) {
            Reject('Concurrent current compiler products differ.');
        }
        await Requireˉunchanged();
        return { directory: Place, status: Status };
    } finally {
        if (path.dirname(path.resolve(Temporary)) !== Family ||
            !path.basename(Temporary).startsWith(`.new-${Key}-`)) {
            Reject('Refusing to remove an unexpected compiler checkpoint temporary directory.');
        }
        if (await Exists(Temporary)) {
            await Directory(Temporary);
            await rm(Temporary, { recursive: true, force: true, maxRetries: 2 });
        }
    }
}

// Fixed construction graph: serial preparation, then at most two producer branches.
export async function Constructˉcurrentˉsplitˉcompiler(
    Work, Candidate, Runˉnative, Runˉnode,
) {
    const Suffix = '.' + EXTENSION;
    const Bootstrap = path.join(REPOSITORY_ROOT, 'Artifacts',
        'Language-1.0-Target-Aware-Emission-Bootstrap', 'Wvb');
    const Pinnedˉanalyzerˉwvb = path.join(Bootstrap, 'wvanalyze.wvb');
    const Pinnedˉemitterˉwvb = path.join(Bootstrap, 'wvemit.wvb');
    const Projectˉpath = Name => path.join(REPOSITORY_ROOT, 'Projects', 'Tools', Name);
    const Pinnedˉanalyzer = path.join(Work, `Pinned-Analyzer${Suffix}`);
    const Pinnedˉemitter = path.join(Work, `Pinned-Emitter${Suffix}`);
    const Analyzerˉwvb = path.join(Work, 'Analyzer.wvb');
    const Analyzer = path.join(Work, `Analyzer${Suffix}`);
    const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
    const Checkpointˉanalyzer = path.join(Work, `Checkpoint-Analyzer${Suffix}`);
    const Checkpointˉanalyzerˉidentity = path.join(
        Work, 'Checkpoint-Analyzer.identity',
    );
    const Emitterˉwvb = path.join(Work, 'Emitter.wvb');
    const Emitter = path.join(Work, `Emitter${Suffix}`);
    const Emitterˉidentity = path.join(Work, 'Emitter.identity');

    await Runˉnative('pinned-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Pinnedˉanalyzerˉwvb, Pinnedˉanalyzer, '--development-cache',
    ]);
    await Runˉnative('pinned-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Pinnedˉemitterˉwvb, Pinnedˉemitter, '--development-cache',
    ]);
    const Pinnedˉanalyzerˉidentity = path.join(
        Work, 'Pinned-Analyzer.identity',
    );
    const Pinnedˉemitterˉidentity = path.join(
        Work, 'Pinned-Emitter.identity',
    );
    await Runˉnode('pinned-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Pinnedˉanalyzer, Pinnedˉanalyzerˉidentity,
    ]);
    await Runˉnode('pinned-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnode('stage1-analyzer-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Analysis-Driver.wvproj'),
        Analyzerˉwvb,
        Pinnedˉanalyzer,
        Pinnedˉanalyzerˉidentity,
        Pinnedˉemitter,
        Pinnedˉemitterˉidentity,
    ]);
    await Runˉnative('stage1-checkpoint-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Analyzerˉwvb, Checkpointˉanalyzer, '--development-cache',
    ]);
    await Runˉnode('stage1-checkpoint-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Checkpointˉanalyzer, Checkpointˉanalyzerˉidentity,
    ]);
    // Both branches consume the completed native image. Each owns distinct outputs.
    // Join failures too, so no producer outlives the work-directory cleanup.
    const Results = await Promise.allSettled([
        (async () => {
            await Runˉnative('stage1-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
                '7', Analyzerˉwvb, Analyzer, '--development-cache',
            ]);
            await Runˉnode('stage1-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
                'analyzer', Analyzer, Analyzerˉidentity,
            ]);
        })(),
        (async () => {
            await Runˉnode('stage1-emitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
                Projectˉpath('Windvale-Compiler-Emission-Driver.wvproj'),
                Emitterˉwvb,
                Checkpointˉanalyzer,
                Checkpointˉanalyzerˉidentity,
                Pinnedˉemitter,
                Pinnedˉemitterˉidentity,
                '--symbol-checkpoint',
            ]);
            await Runˉnative('stage1-emitter-package', 'Package-Segmented-Compiler-Wvb', [
                '8', Emitterˉwvb, Emitter, '--development-cache',
            ]);
            await Runˉnode('stage1-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
                'emitter', Emitter, Emitterˉidentity,
            ]);
        })(),
    ]);
    const Failures = Results.filter(Result => Result.status === 'rejected');
    if (Failures.length !== 0) {
        throw new AggregateError(Failures.map(Result => Result.reason),
            'Current compiler preparation failed.');
    }
    for (const Product of [Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity]) {
        await copyFile(Product, path.join(Candidate, path.basename(Product)));
    }
}
