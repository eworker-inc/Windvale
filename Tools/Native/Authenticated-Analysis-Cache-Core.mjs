import { createHash } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { lstat, mkdir, mkdtemp, opendir, realpath, rename, rmdir, unlink, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { Addˉhostedˉkeyˉfield, Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';

const NAMESPACE = 'authenticated-analysis-v1';
const WINDOWS = process.platform === 'win32';
const FILES = ['Source.wvss', 'Manifest.wvca', 'Bindings.wvlb', 'Wir.wvir', 'Report.txt'];
const LIMITS = [4_194_304, 104, 4_194_304, 4_194_304, 65_536];
const SCRIPT = fileURLToPath(import.meta.url);
const IMPLEMENTATION = await Readˉboundedˉhostedˉfile(SCRIPT, 'analysis cache implementation', 1_048_576);
const READER = path.join(path.dirname(SCRIPT), 'Native-Hosted-Application-Cache-Core.mjs');
const READER_IMPLEMENTATION = await Readˉboundedˉhostedˉfile(READER, 'analysis cache reader', 1_048_576);
function Digest(Bytes) { return createHash('sha256').update(Bytes).digest('hex'); }
function Sameˉpath(Left, Right) { return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right; }
async function Exists(Name) {
    return lstat(Name).then(() => true, Error => { if (Error.code === 'ENOENT') return false; throw Error; });
}
async function Names(Place) {
    const Result = [];
    for await (const Entry of await opendir(Place)) {
        if (Result.length >= FILES.length + 1) throw new Error('Analysis inventory exceeds its bound.');
        Result.push(Entry.name);
    }
    return Result;
}
async function Directory(Name, Create = false) {
    Name = path.resolve(Name);
    let Cursor = path.parse(Name).root;
    for (const Part of Name.slice(Cursor.length).split(path.sep).filter(Boolean)) {
        Cursor = path.join(Cursor, Part);
        if (Create) await mkdir(Cursor).catch(Error => { if (Error.code !== 'EEXIST') throw Error; });
        const Information = await lstat(Cursor);
        if (!Information.isDirectory() || Information.isSymbolicLink()) throw new Error('Analysis cache directory contains a link or non-directory.');
    }
    if (!Sameˉpath(await realpath(Name), Name)) throw new Error('Analysis cache directory is not canonical.');
    return Name;
}
async function Fingerprint(Name) {
    const Information = await lstat(Name);
    if (!Information.isFile() || Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > 134_217_728 || !Sameˉpath(await realpath(Name), path.resolve(Name))) {
        throw new Error('Analysis producer is not a bounded ordinary file.');
    }
    const Hash = createHash('sha256');
    let Bytes = 0;
    for await (const Chunk of createReadStream(Name, { highWaterMark: 1_048_576 })) {
        Bytes += Chunk.length;
        if (Bytes > Information.size) throw new Error('Analysis producer grew while read.');
        Hash.update(Chunk);
    }
    if (Bytes !== Information.size) throw new Error('Analysis producer changed length.');
    return { bytes: Bytes, sha256: Hash.digest('hex') };
}
function Record(Key, Values) {
    return Buffer.from(JSON.stringify({ format: NAMESPACE, key: Key,
        files: Values.map((Bytes, Index) => ({ name: FILES[Index], bytes: Bytes.length, sha256: Digest(Bytes) })) }) + '\n');
}
async function Readˉvalues(Place) {
    const Values = [];
    for (const [Index, Name] of FILES.entries()) Values.push(
        await Readˉboundedˉhostedˉfile(path.join(Place, Name), 'analysis checkpoint ' + Name, LIMITS[Index]));
    if (Values[0].length < 37 || Values[1].length !== 104) throw new Error('Analysis checkpoint geometry differs.');
    return Values;
}
async function Validate(Place, Key) {
    await Directory(Place);
    const Inventory = await Names(Place);
    if (JSON.stringify(Inventory.sort()) !== JSON.stringify([...FILES, 'Checkpoint.json'].sort())) {
        throw new Error('Analysis checkpoint inventory differs.');
    }
    const Values = await Readˉvalues(Place);
    const Manifest = await Readˉboundedˉhostedˉfile(path.join(Place, 'Checkpoint.json'), 'analysis checkpoint record', 4096);
    if (!Manifest.equals(Record(Key, Values))) throw new Error('Analysis checkpoint record differs.');
    return Values;
}

// Admission and authentication run before this call on every outer WVB miss.
// Only analyzer output is reusable; binding and emission remain later phases.
export async function Acquireˉauthenticatedˉanalysis({ Analyzer, Mode, Inputs, Outputs, Produce, Writeˉprivate, Coordinator }) {
    if (!['--internal-source-set', '--internal-foreign-source-set'].includes(Mode) ||
        Inputs.length !== 6 || Outputs.length !== 4 || !Buffer.isBuffer(Coordinator) ||
        Coordinator.length > 1_048_576 || Inputs.some(Bytes => !Buffer.isBuffer(Bytes) || Bytes.length > 4_194_304)) {
        throw new Error('Invalid authenticated analysis request.');
    }
    if (process.env.WINDVALE_PREPARED_PRODUCTS_ONLY !== undefined && process.env.WINDVALE_PREPARED_PRODUCTS_ONLY !== '1') {
        throw new Error('WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1.');
    }
    const Root = process.env.WINDVALE_NATIVE_CACHE_ROOT || (WINDOWS
        ? path.join(process.env.LOCALAPPDATA ?? os.tmpdir(), 'Windvale', 'Native-Tool-Cache')
        : path.join(process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache'), 'windvale', 'native-tool-cache'));
    const Family = await Directory(path.join(Root, NAMESPACE, `${process.platform}-${process.arch}`), true);
    const Producer = await Fingerprint(Analyzer);
    const Runtime = await Fingerprint(process.execPath);
    const Hash = createHash('sha256');
    const Fields = [Buffer.from(NAMESPACE), IMPLEMENTATION, READER_IMPLEMENTATION, Coordinator,
        Buffer.from(JSON.stringify([process.platform, process.arch, process.version, Runtime, Producer, Mode])), ...Inputs];
    Fields.forEach((Bytes, Index) => Addˉhostedˉkeyˉfield(Hash, String(Index), Bytes));
    const Key = Hash.digest('hex'), Place = path.join(Family, Key);
    async function Unchanged() {
        if (JSON.stringify(await Fingerprint(Analyzer)) !== JSON.stringify(Producer) ||
            !(await Readˉboundedˉhostedˉfile(SCRIPT, 'analysis cache implementation', 1_048_576)).equals(IMPLEMENTATION) ||
            !(await Readˉboundedˉhostedˉfile(READER, 'analysis cache reader', 1_048_576)).equals(READER_IMPLEMENTATION)) {
            throw new Error('Analysis producer or cache implementation changed.');
        }
    }
    let Values, Status;
    if (await Exists(Place)) {
        Values = await Validate(Place, Key);
        await Unchanged();
        for (let Index = 0; Index < 4; Index += 1) await Writeˉprivate(Outputs[Index], Values[Index]);
        Status = 'Hit';
    } else {
        if (process.env.WINDVALE_PREPARED_PRODUCTS_ONLY !== undefined) {
            throw new Error('Authenticated analysis checkpoint is not prepared.');
        }
        const Report = await Produce();
        Values = [];
        for (let Index = 0; Index < 4; Index += 1) Values.push(
            await Readˉboundedˉhostedˉfile(Outputs[Index], 'analysis output', LIMITS[Index]));
        Values.push(Report);
        if (!Buffer.isBuffer(Report) || Report.length < 1 || Report.length > LIMITS[4] ||
            Values[0].length < 37 || Values[1].length !== 104) throw new Error('Invalid analysis phase output.');
        if (!Values[0].equals(Inputs[0])) throw new Error('The Analyzer republished a different admitted source set.');
        await Unchanged();
        const Temporary = await mkdtemp(path.join(Family, '.new-' + Key + '-'));
        const Identity = await lstat(Temporary);
        try {
            for (let Index = 0; Index < FILES.length; Index += 1) await writeFile(path.join(Temporary, FILES[Index]), Values[Index], { flag: 'wx', mode: 0o600 });
            await writeFile(path.join(Temporary, 'Checkpoint.json'), Record(Key, Values), { flag: 'wx', mode: 0o600 });
            await Validate(Temporary, Key);
            Status = 'Created';
            try { await rename(Temporary, Place); }
            catch (Error) {
                if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(Error.code)) throw Error;
                Status = 'Hit';
            }
            if (!Record(Key, await Validate(Place, Key)).equals(Record(Key, Values))) throw new Error('Concurrent analysis outputs differ.');
        } finally {
            if (await Exists(Temporary)) {
                await Directory(Temporary);
                const Current = await lstat(Temporary);
                if (Current.dev !== Identity.dev || Current.ino !== Identity.ino) throw new Error('Analysis temporary directory identity changed.');
                const Inventory = await Names(Temporary);
                if (Inventory.some(Name => ![...FILES, 'Checkpoint.json'].includes(Name))) throw new Error('Analysis temporary inventory changed.');
                for (const Name of Inventory) await unlink(path.join(Temporary, Name));
                await rmdir(Temporary);
            }
        }
    }
    await Unchanged();
    console.log(`split compiler step=authenticated-analysis cache=${Status} key=${Key}`);
    return Values[4];
}
