import { createHash, randomBytes } from 'node:crypto';
import { copyFile, lstat, mkdir, open, opendir, realpath, rename, rm, writeFile } from 'node:fs/promises';
import { constants } from 'node:fs';
import { homedir } from 'node:os';
import { basename, dirname, extname, join, parse, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Isˉsameˉhostedˉpath, Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';

const REPOSITORY = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const WINDOWS = process.platform === 'win32';
const HOST = WINDOWS ? 'windows-x64' : 'linux-x64';
const EXTENSION = WINDOWS ? '.exe' : '.elf';
const NAMESPACE = 'native-assembly-objects-v1';
const MAXIMUM_SOURCE_BYTES = 1_048_576;
const MAXIMUM_OBJECT_BYTES = 4_194_304;
const MAXIMUM_PRODUCER_BYTES = 134_217_728;
const TOOLS = Object.freeze({
    Assembler: ['Artifacts/Native-Front-Door/' + HOST + '/wvasm' + EXTENSION,
        WINDOWS ? '40a35687fb052dcd4f6d3a767436f4024d91bd5f03890b30fa4f0300184a35ed' :
            '36796a26917e699030e2987c01b74799bcdc339af578f76e02f9a1f47ca10b8c'],
    Checker: ['Artifacts/Native-Wvo-Object-Candidate/Wvo-Object' + EXTENSION,
        WINDOWS ? '182739a91046cf3563924668cf724ba1ad17ac5007d91c023e6687de7f2b83a4' :
            'b8f0367a8ced12227c9554101152bd5199ec0fd32e5e78210f5dd8a0761b81c7'],
});
const PRODUCERS = [fileURLToPath(import.meta.url),
    join(REPOSITORY, 'Tools/Native/Development-Command-Core.mjs'),
    join(REPOSITORY, 'Tools/Native/Native-Hosted-Application-Cache-Core.mjs')];
const LOADED = await Promise.all(PRODUCERS.map(async Path => ({ Path,
    Sha256: Digest(await Readˉboundedˉhostedˉfile(Path, 'loaded assembly cache producer')) })));

function Digest(Bytes) { return createHash('sha256').update(Bytes).digest('hex'); }
function Reject(Message, Code = 1) { throw Object.assign(new Error(Message), { exitCode: Code }); }
function Time(Deadline) {
    if (!Number.isSafeInteger(Deadline)) Reject('Invalid assembly-object deadline.');
    if (Date.now() >= Deadline) Reject('Assembly-object deadline expired.', 124);
}
async function Directory(Path, Create = false) {
    const Absolute = resolve(Path), Root = parse(Absolute).root;
    let Current = Root;
    for (const Part of Absolute.slice(Root.length).split(sep).filter(Boolean)) {
        Current = join(Current, Part);
        if (Create) await mkdir(Current).catch(Error => { if (Error.code !== 'EEXIST') throw Error; });
        const Information = await lstat(Current);
        if (!Information.isDirectory() || Information.isSymbolicLink()) Reject('Assembly cache directory is linked or not a directory.');
    }
    if (!Isˉsameˉhostedˉpath(await realpath(Absolute), Absolute)) Reject('Assembly cache directory must be canonical.');
    return Absolute;
}
async function Evidence(Path, Deadline) {
    Time(Deadline);
    const Information = await lstat(Path);
    if (!Information.isFile() || Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > MAXIMUM_PRODUCER_BYTES || !Isˉsameˉhostedˉpath(await realpath(Path), resolve(Path))) {
        Reject('Assembly producer is not a bounded canonical file.');
    }
    const File = await open(Path, 'r');
    try {
        const Opened = await File.stat();
        if (Opened.dev !== Information.dev || Opened.ino !== Information.ino || Opened.size !== Information.size) {
            Reject('Assembly producer changed before reading.');
        }
        const Hash = createHash('sha256');
        let Bytes = 0;
        for await (const Chunk of File.createReadStream({ highWaterMark: 1_048_576, autoClose: false })) {
            Time(Deadline);
            Bytes += Chunk.length;
            if (Bytes > Information.size) Reject('Assembly producer grew during reading.');
            Hash.update(Chunk);
        }
        if (Bytes !== Information.size) Reject('Assembly producer changed length.');
        return { Bytes, Sha256: Hash.digest('hex') };
    } finally { await File.close(); }
}

export async function Prepareˉassemblyˉobjectˉcache(Deadline) {
    Time(Deadline);
    if (process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform)) Reject('Unsupported assembly cache host.');
    const Mode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    if (Mode !== undefined && Mode !== '1') Reject('WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1.');
    const Producers = [];
    for (const Loaded of LOADED) {
        const Value = await Evidence(Loaded.Path, Deadline);
        if (Value.Sha256 !== Loaded.Sha256) Reject('Loaded assembly cache producer changed.');
        Producers.push({ Path: Loaded.Path, ...Value });
    }
    Producers.push({ Path: process.execPath, ...await Evidence(process.execPath, Deadline) });
    const Executables = {};
    for (const [Name, [Relative, Expected]] of Object.entries(TOOLS)) {
        const Path = join(REPOSITORY, Relative), Value = await Evidence(Path, Deadline);
        if (Value.Sha256 !== Expected || (!WINDOWS && ((await lstat(Path)).mode & 0o111) === 0)) {
            Reject(`The native ${Name} identity or execution permission is invalid.`);
        }
        Producers.push({ Path, ...Value }); Executables[Name] = Path;
    }
    const Root = process.env.WINDVALE_NATIVE_CACHE_ROOT ?? (WINDOWS ?
        (process.env.LOCALAPPDATA ? join(process.env.LOCALAPPDATA, 'Windvale/Native-Tool-Cache') : null) :
        join(process.env.XDG_CACHE_HOME ?? join(homedir(), '.cache'), 'windvale/native-tool-cache'));
    if (Root === null) Reject('An assembly cache root is required.');
    const Family = await Directory(join(Root, NAMESPACE, HOST), true);
    const Fingerprint = JSON.stringify({ Format: NAMESPACE, Host: HOST, Node: process.version,
        Command: 'wvasm <private-source.wva> <product.wvo>; Wvo-Object check <product.wvo>',
        Producers: Producers.map(({ Bytes, Sha256 }) => ({ Bytes, Sha256 })) });
    const Requireˉunchanged = async () => {
        for (const Producer of Producers) {
            const Current = await Evidence(Producer.Path, Deadline);
            if (Current.Bytes !== Producer.Bytes || Current.Sha256 !== Producer.Sha256) Reject('Assembly cache producer changed.');
        }
    };
    return Object.freeze({ Family, Fingerprint, Deadline, Prepared: Mode === '1',
        ...Executables, Requireˉunchanged });
}

function Record(Key, Source, Objectˉbytes) {
    return Buffer.from(JSON.stringify({ format: NAMESPACE, key: Key,
        source: { bytes: Source.length, sha256: Digest(Source) },
        object: { bytes: Objectˉbytes.length, sha256: Digest(Objectˉbytes) } }) + '\n');
}
async function Validate(Path, Key, Source) {
    await Directory(Path);
    const Reader = await opendir(Path);
    const Entries = [];
    for await (const Entry of Reader) {
        if (Entries.length === 2) Reject('Assembly checkpoint has unexpected entries.');
        Entries.push(Entry.name);
    }
    if (Entries.sort().join(',') !== 'Checkpoint.json,Product.wvo') Reject('Assembly checkpoint inventory differs.');
    const Object = await Readˉboundedˉhostedˉfile(join(Path, 'Product.wvo'), 'cached assembly object', MAXIMUM_OBJECT_BYTES);
    const Information = await lstat(join(Path, 'Product.wvo'));
    if (Information.nlink !== 1) Reject('Cached assembly object must be unaliased.');
    if (Object.length < 12 || Object.toString('ascii', 0, 4) !== 'WVO1' || Object.readUInt32LE(4) !== 1) {
        Reject('Cached assembly object header differs.');
    }
    if ((await lstat(join(Path, 'Checkpoint.json'))).nlink !== 1) Reject('Assembly checkpoint must be unaliased.');
    const Manifest = await Readˉboundedˉhostedˉfile(join(Path, 'Checkpoint.json'), 'assembly checkpoint', 4096);
    if (!Manifest.equals(Record(Key, Source, Object))) Reject('Assembly checkpoint record differs.');
    return Object;
}

export async function Acquireˉassemblyˉobject(Context, Sourceˉpath, Output) {
    Time(Context.Deadline);
    if (extname(Sourceˉpath) !== '.wva' || extname(Output) !== '.wvo') Reject('Assembly cache extensions differ.');
    await Directory(dirname(resolve(Output)));
    if (await lstat(Output).catch(Error => { if (Error.code === 'ENOENT') return null; throw Error; }) !== null) {
        Reject('Assembly cache materialization requires a new output.');
    }
    const Source = await Readˉboundedˉhostedˉfile(Sourceˉpath, 'assembly source', MAXIMUM_SOURCE_BYTES);
    const Key = Digest(Buffer.concat([Buffer.from(Context.Fingerprint + '\n'), Source]));
    const Destination = join(Context.Family, Key);
    const Unchanged = async () => {
        Time(Context.Deadline);
        if (!(await Readˉboundedˉhostedˉfile(Sourceˉpath, 'assembly source', MAXIMUM_SOURCE_BYTES)).equals(Source)) {
            Reject('Assembly source changed during construction.');
        }
    };
    let Status = 'Hit';
    const Existing = await lstat(Destination).catch(Error => { if (Error.code === 'ENOENT') return null; throw Error; });
    if (Existing === null) {
        if (Context.Prepared) Reject(`Prepared assembly object missing key=${Key}; prepare owned-console tools separately.`, 64);
        const Temporary = join(Context.Family, `.new-${Key}-${process.pid}-${randomBytes(16).toString('hex')}`);
        await mkdir(Temporary);
        try {
            const Snapshot = join(Temporary, 'Source.wva'), Product = join(Temporary, 'Product.wvo');
            await writeFile(Snapshot, Source, { flag: 'wx' });
            for (const [Step, Tool, Arguments] of [
                ['assemble', Context.Assembler, [Snapshot, Product]],
                ['admit', Context.Checker, ['check', Product]],
            ]) {
                process.stdout.write(`assembly object step=${Step} source=${basename(Sourceˉpath)} status=Started\n`);
                const Result = await Runˉdevelopmentˉcommand(Tool, Arguments, Context.Deadline, true);
                if (Result.Code !== 0 || Result.Error !== '') Reject(`Assembly ${Step} failed: ${Result.Code} ${Result.Error}`);
            }
            if (!(await Readˉboundedˉhostedˉfile(Snapshot, 'private assembly source', MAXIMUM_SOURCE_BYTES)).equals(Source)) {
                Reject('Private assembly source changed.');
            }
            const Object = await Readˉboundedˉhostedˉfile(Product, 'assembled object', MAXIMUM_OBJECT_BYTES);
            await rm(Snapshot);
            await writeFile(join(Temporary, 'Checkpoint.json'), Record(Key, Source, Object), { flag: 'wx' });
            await Validate(Temporary, Key, Source);
            await Unchanged(); await Context.Requireˉunchanged();
            try { await rename(Temporary, Destination); Status = 'Created'; }
            catch (Error) {
                if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(Error.code)) throw Error;
                const Winner = await Validate(Destination, Key, Source);
                if (!Winner.equals(Object)) Reject('Concurrent assembly outputs differ.');
            }
        } finally {
            if (dirname(Temporary) !== Context.Family || !basename(Temporary).startsWith(`.new-${Key}-`)) {
                Reject('Refusing to remove an unowned assembly cache directory.');
            }
            if (await lstat(Temporary).catch(Error => { if (Error.code === 'ENOENT') return null; throw Error; }) !== null) {
                await Directory(Temporary); await rm(Temporary, { recursive: true, force: false });
            }
        }
    }
    const Objectˉbytes = await Validate(Destination, Key, Source);
    await Unchanged();
    await copyFile(join(Destination, 'Product.wvo'), Output, constants.COPYFILE_EXCL);
    if (!(await Readˉboundedˉhostedˉfile(Output, 'materialized assembly object', MAXIMUM_OBJECT_BYTES)).equals(Objectˉbytes)) {
        Reject('Assembly materialization bytes differ.');
    }
    process.stdout.write(`assembly object cache status=${Status} key=${Key} source=${basename(Sourceˉpath)}\n`);
    return Object.freeze({ Key, Status, Requireˉunchanged: Unchanged });
}
