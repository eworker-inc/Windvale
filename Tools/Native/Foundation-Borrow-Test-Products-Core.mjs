import { createHash } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { lstat, mkdtemp, readFile, realpath } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Acquireˉcurrentˉsplitˉcompiler,
    Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';
import {
    Getˉnativeˉprojectˉcacheˉrequest,
    Prepareˉnativeˉprojectˉcacheˉcontext,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
    REPOSITORY_ROOT,
} from './Native-Project-Cache-Key-Core.mjs';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const SCRIPT_DIRECTORY = path.dirname(SCRIPT_PATH);
const WINDOWS = process.platform === 'win32';
const SUFFIX = WINDOWS ? '.exe' : '.elf';
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_NODE_BYTES = 134_217_728;
const TARGETS = Object.freeze([
    ['Verifier', 'Tools/Windvale-Compiler-Wvb-Verifier.wvproj', '7'],
    ['Runner', 'Tools/Windvale-Wvb-Runner.wvproj', '5'],
    ['Components', 'Tests/Windvale-Native-Test-Foundation-Borrow-Components.wvproj', '1'],
]);
const COMPILER_NAMES = Object.freeze([
    'Analyzer', 'Emitter', 'Admitter', 'Authenticator', 'Reader', 'Binder',
]);

function Reject(Message) { throw new Error(Message); }
function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}

async function Directory(Candidate) {
    const Resolved = path.resolve(Candidate);
    let Current = path.parse(Resolved).root;
    for (const Part of Resolved.slice(Current.length).split(path.sep).filter(Boolean)) {
        Current = path.join(Current, Part);
        const Information = await lstat(Current);
        if (!Information.isDirectory() || Information.isSymbolicLink()) {
            Reject('Foundation test work contains a link or non-directory.');
        }
    }
    if (!Sameˉpath(await realpath(Resolved), Resolved)) {
        Reject('Foundation test work must use its canonical path.');
    }
    // Windows file identifiers may exceed Number's exact integer range.
    const Information = await lstat(Resolved, { bigint: true });
    return { Path: Resolved, Device: Information.dev, Inode: Information.ino };
}

async function Evidence(Candidate, Maximum, Executable, Check) {
    Check();
    const Information = await lstat(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        (Executable && !WINDOWS && (Information.mode & 0o111) === 0) ||
        !Sameˉpath(await realpath(Candidate), Candidate)) {
        Reject(`Foundation test input is not a bounded ordinary file: ${Candidate}`);
    }
    const Hash = createHash('sha256');
    let Bytes = 0;
    for await (const Chunk of createReadStream(Candidate, { highWaterMark: 1_048_576 })) {
        Check();
        Bytes += Chunk.length;
        if (Bytes > Information.size) Reject('Foundation test input grew while read.');
        Hash.update(Chunk);
    }
    const After = await lstat(Candidate);
    if (Bytes !== Information.size || After.size !== Information.size ||
        After.dev !== Information.dev || After.ino !== Information.ino ||
        After.mtimeMs !== Information.mtimeMs || After.ctimeMs !== Information.ctimeMs) {
        Reject('Foundation test input changed while read.');
    }
    Check();
    return Object.freeze({ Path: Candidate, Bytes, Sha256: Hash.digest('hex') });
}

async function Snapshotˉinputs(Projects, Check) {
    // These are invocation snapshots, not a new reusable cache or universal closure.
    const Context = await Prepareˉnativeˉprojectˉcacheˉcontext('foundation-borrow-test-inputs', [
        SCRIPT_PATH,
        ...['Current-Split-Compiler-Cache-Core.mjs', 'Build-Current-Split-Project-Wvb.mjs',
            'Build-Cached-Split-Project-Wvb.mjs', 'Build-Cached-Segmented-Hosted-Wvb.mjs',
            'Native-Project-Cache-Key-Core.mjs', 'Native-Hosted-Application-Cache-Core.mjs',
            'Build-Cached-Segmented-Project.mjs', 'Development-Command-Core.mjs']
            .map(Name => path.join(SCRIPT_DIRECTORY, Name)),
    ]);
    const Requests = [];
    for (const Project of Projects) {
        Check();
        Requests.push(await Getˉnativeˉprojectˉcacheˉrequest(Context, Project));
    }
    return { Requests, Node: await Evidence(process.execPath, MAXIMUM_NODE_BYTES, true, Check) };
}

async function Requireˉinputsˉunchanged(Snapshot, Check) {
    for (const Request of Snapshot.Requests) {
        Check();
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
    }
    const Node = await Evidence(process.execPath, MAXIMUM_NODE_BYTES, true, Check);
    if (Node.Bytes !== Snapshot.Node.Bytes || Node.Sha256 !== Snapshot.Node.Sha256) {
        Reject('Foundation test Node producer changed.');
    }
}

// Run must enforce Deadline over its entire subprocess tree, await termination,
// and reject a nonzero exit or stderr. This helper owns no process or cache writer.
// Injected cache/snapshot callbacks are only for the existing focused unit owner.
export async function Acquireˉfoundationˉborrowˉtestˉproducts({
    Work, Deadline, Run, Prepareˉcompiler = false,
    Getˉkey = Getˉcurrentˉsplitˉcompilerˉkey,
    Getˉfamily = Getˉcurrentˉsplitˉcompilerˉfamily,
    Acquire = Acquireˉcurrentˉsplitˉcompiler,
    Snapshot = Snapshotˉinputs,
    Requireˉunchanged = Requireˉinputsˉunchanged,
}) {
    if (typeof Work !== 'string' || !path.isAbsolute(Work) ||
        !Number.isSafeInteger(Deadline) || typeof Run !== 'function' ||
        typeof Prepareˉcompiler !== 'boolean' ||
        [Getˉkey, Getˉfamily, Acquire, Snapshot, Requireˉunchanged]
            .some(Callback => typeof Callback !== 'function') ||
        process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform)) {
        Reject('Invalid Foundation test product acquisition request.');
    }
    function Check() {
        if (Date.now() >= Deadline) {
            throw Object.assign(new Error('Foundation test product deadline expired.'), { exitCode: 124 });
        }
    }
    Check();
    const Parent = await Directory(Work);
    if (Sameˉpath(Parent.Path, path.parse(Parent.Path).root) ||
        Sameˉpath(Parent.Path, REPOSITORY_ROOT)) {
        Reject('Foundation test products require a scoped work directory.');
    }
    Check();
    const Workspace = await Directory(await mkdtemp(path.join(Parent.Path, 'Foundation-Borrow-Products-')));
    async function Requireˉwork() {
        Check();
        for (const Original of [Parent, Workspace]) {
            const Current = await Directory(Original.Path);
            if (Current.Device !== Original.Device || Current.Inode !== Original.Inode) {
                Reject('Foundation test work directory changed.');
            }
        }
    }
    const Projects = TARGETS.map(([, Relative]) => path.join(REPOSITORY_ROOT, 'Projects', Relative));
    const Inputs = await Snapshot(Projects, Check);
    Check();
    const Key = await Getˉkey();
    if (typeof Key !== 'string' || !/^[0-9a-f]{64}$/u.test(Key)) {
        Reject('Invalid Foundation test current compiler key.');
    }
    const Products = { Work: Workspace.Path, Compilerˉkey: Key };
    const Measurements = [];
    for (const [Name] of TARGETS) {
        Products[Name + 'ˉwvb'] = path.join(Workspace.Path, Name + '.wvb');
        Products[Name] = path.join(Workspace.Path, Name + SUFFIX);
    }
    if (Prepareˉcompiler) {
        await Requireˉwork();
        await Run('foundation-products-build', process.execPath,
            [path.join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs'),
                '--deadline-ms', String(Deadline),
                ...TARGETS.flatMap(([Name], Index) => [Projects[Index], Products[Name + 'ˉwvb']])], Deadline);
        Check();
    }
    async function Requireˉcompilerˉunchanged() {
        Check();
        if (await Getˉkey() !== Key) Reject('Foundation test compiler inputs changed.');
        Check();
    }
    const Family = await Getˉfamily();
    const Neverˉproduce = async () => Reject(
        'Current compiler checkpoint missing. Foundation verification did not rebuild the compiler. ' +
        'Prepare it explicitly with Build-Current-Split-Project-Wvb.mjs and a selected deadline, ' +
        'or use the supplied-product development selections.');
    const Checkpoint = await Acquire(Family, Key, Neverˉproduce, Requireˉcompilerˉunchanged);
    if (Checkpoint.status !== 'Hit' || !Sameˉpath(Checkpoint.directory, path.join(Family, Key))) {
        Reject('Foundation test acquisition requires the existing exact compiler checkpoint.');
    }
    for (const Name of COMPILER_NAMES) {
        Products[Name] = path.join(Checkpoint.directory, Name + SUFFIX);
        Measurements.push(await Evidence(Products[Name], MAXIMUM_PRODUCT_BYTES, true, Check));
    }
    for (const Name of ['Analyzer', 'Emitter']) {
        Products[Name + 'ˉidentity'] = path.join(Checkpoint.directory, Name + '.identity');
        Measurements.push(await Evidence(Products[Name + 'ˉidentity'], 1_024, false, Check));
    }
    // Prepared-only development builds affected test products with the retained
    // compiler; it cannot recursively initiate compiler construction on a miss.
    for (const [Index, [Name]] of TARGETS.entries()) {
        if (!Prepareˉcompiler) {
            await Requireˉwork();
            const Modern = (await readFile(Projects[Index], 'utf8')).split(/\r?\n/u)[0] === 'windvale-project 4';
            await Run('foundation-products-build-' + Name.toLowerCase(), process.execPath,
                [path.join(SCRIPT_DIRECTORY, 'Build-Cached-Split-Project-Wvb.mjs'),
                    Projects[Index], Products[Name + 'ˉwvb'],
                    Products.Analyzer, Products.Analyzerˉidentity,
                    Products.Emitter, Products.Emitterˉidentity,
                    ...(Modern ? ['--authenticated-project4',
                        Products.Admitter, Products.Authenticator, Products.Reader, Products.Binder]
                        : ['--symbol-checkpoint'])], Deadline);
            Check();
        }
        Measurements.push(await Evidence(Products[Name + 'ˉwvb'], MAXIMUM_WVB_BYTES, false, Check));
    }
    await Requireˉunchanged(Inputs, Check);
    const Failures = [];
    let Next = 0;
    async function Packageˉworker() {
        while (Failures.length === 0 && Next < TARGETS.length) {
            const [Name, , Profile] = TARGETS[Next++];
            try {
                await Requireˉwork();
                // Recheck after awaited preparation: a peer may already have failed.
                if (Failures.length !== 0) return;
                Check();
                await Run('foundation-products-package-' + Name.toLowerCase(), process.execPath,
                    [path.join(SCRIPT_DIRECTORY, 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
                        '--deadline-ms', String(Deadline),
                        Profile, Products[Name + 'ˉwvb'], Products[Name]], Deadline);
                Measurements.push(await Evidence(Products[Name], MAXIMUM_PRODUCT_BYTES, true, Check));
            } catch (Error) { Failures.push(Error); }
        }
    }
    // Each worker owns one subprocess at a time; both settle before caller cleanup.
    await Promise.all([Packageˉworker(), Packageˉworker()]);
    if (Failures.length !== 0) {
        throw Object.assign(new AggregateError(Failures, 'Foundation test packaging failed.'), {
            exitCode: Failures.some(Error => Error.exitCode === 2) ? 2 :
                Failures.some(Error => Error.exitCode === 124) ? 124 : 1,
            cleanupUncertain: Failures.some(Error => Error.cleanupUncertain || Error.exitCode === 2),
        });
    }
    await Requireˉwork();
    await Requireˉunchanged(Inputs, Check);
    const Final = await Acquire(Family, Key, Neverˉproduce, Requireˉcompilerˉunchanged);
    if (Final.status !== 'Hit' || !Sameˉpath(Final.directory, Checkpoint.directory)) {
        Reject('Foundation test compiler checkpoint changed.');
    }
    for (const Before of Measurements) {
        const After = await Evidence(Before.Path, Before.Path.endsWith('.identity') ? 1_024 :
            Before.Path.endsWith('.wvb') ? MAXIMUM_WVB_BYTES : MAXIMUM_PRODUCT_BYTES,
            Before.Path.endsWith(SUFFIX), Check);
        if (Before.Bytes !== After.Bytes || Before.Sha256 !== After.Sha256) {
            Reject('Foundation test product changed: ' + Before.Path);
        }
    }
    Check();
    Products.Evidence = Object.freeze(Measurements.sort((Left, Right) => Left.Path.localeCompare(Right.Path)));
    return Object.freeze(Products);
}
