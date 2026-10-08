import { createHash, randomBytes } from 'node:crypto';
import { constants } from 'node:fs';
import { copyFile, lstat, mkdir, opendir, realpath, rename, rm, writeFile } from 'node:fs/promises';
import { basename, dirname, extname, join, parse, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Isˉsameˉhostedˉpath, Readˉboundedˉhostedˉfile } from './Native-Hosted-Application-Cache-Core.mjs';

const NAMESPACE = 'native-source-objects-v1';
const MAXIMUM_INPUT_BYTES = 4_194_304;
const MAXIMUM_OBJECT_BYTES = MAXIMUM_INPUT_BYTES - 32;
const DIRECTORY = dirname(fileURLToPath(import.meta.url));
const PRODUCERS = [fileURLToPath(import.meta.url), join(DIRECTORY, 'Native-Hosted-Application-Cache-Core.mjs')];
const LOADED = await Promise.all(PRODUCERS.map(async Path => ({ Path,
    Sha256: Digest(await Readˉboundedˉhostedˉfile(Path, 'loaded source-object cache producer')) })));

function Digest(Bytes) { return createHash('sha256').update(Bytes).digest('hex'); }
function Reject(Message, Code = 1) { throw Object.assign(new Error(Message), { exitCode: Code }); }
function Time(Deadline) {
    if (!Number.isSafeInteger(Deadline)) Reject('Invalid source-object deadline.');
    if (Date.now() >= Deadline) Reject('Source-object deadline expired.', 124);
}
async function Directory(Path, Create = false) {
    const Absolute = resolve(Path), Root = parse(Absolute).root;
    let Current = Root;
    for (const Part of Absolute.slice(Root.length).split(sep).filter(Boolean)) {
        Current = join(Current, Part);
        if (Create) await mkdir(Current).catch(Error => { if (Error.code !== 'EEXIST') throw Error; });
        const Information = await lstat(Current);
        if (!Information.isDirectory() || Information.isSymbolicLink()) Reject('Source-object cache directory is linked or not a directory.');
    }
    if (!Isˉsameˉhostedˉpath(await realpath(Absolute), Absolute)) Reject('Source-object cache directory must be canonical.');
    return Absolute;
}
async function Read(Path, Maximum) {
    const Bytes = await Readˉboundedˉhostedˉfile(Path, 'source-object cache input', Maximum);
    if (Bytes.length === 0 || (await lstat(Path)).nlink !== 1) Reject('Source-object cache input must be nonempty and unaliased.');
    return Bytes;
}
async function Absent(Path) {
    if (await lstat(Path).catch(Error => { if (Error.code === 'ENOENT') return null; throw Error; }) !== null)
        Reject('Source-object cache materialization requires a new output.');
}

export async function Prepareˉsourceˉobjectˉcache(Assembly, Host) {
    Time(Assembly?.Deadline);
    if (process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform) ||
        typeof Assembly.Family !== 'string' || typeof Assembly.Fingerprint !== 'string' ||
        typeof Assembly.Prepared !== 'boolean' || typeof Assembly.Requireˉunchanged !== 'function' ||
        typeof Host?.Path !== 'string' || !Number.isSafeInteger(Host.Bytes) || Host.Bytes < 1 ||
        ![Host.Sha256, Host.Recordˉsha256, Host.Compilerˉkey].every(Value => /^[0-9a-f]{64}$/u.test(Value)) ||
        typeof Host.Requireˉunchanged !== 'function') Reject('Source-object cache needs the exact admitted compiler host and assembly tools.');
    const Requireˉunchanged = async () => {
        Time(Assembly.Deadline);
        for (const Producer of LOADED) {
            if (Digest(await Read(Producer.Path, 1_048_576)) !== Producer.Sha256) Reject('Loaded source-object cache producer changed.');
        }
        await Assembly.Requireˉunchanged();
        const Image = await Read(Host.Path, 134_217_728);
        if (Image.length !== Host.Bytes || Digest(Image) !== Host.Sha256) Reject('Source-object compiler image changed.');
        Time(Assembly.Deadline);
    };
    // Compilation reads the executable and WVB, not the construction source
    // tree. The enclosing owner also checks the full host at batch completion.
    await Host.Requireˉunchanged();
    await Requireˉunchanged();
    const Hostˉname = process.platform === 'win32' ? 'windows-x64' : 'linux-x64';
    const Family = await Directory(join(dirname(dirname(Assembly.Family)), NAMESPACE, Hostˉname), true);
    const Fingerprint = JSON.stringify({ Format: NAMESPACE, Host: Hostˉname,
        Command: '<admitted-native-compiler> <private-source.wvb> <product.wvo>; check; inspect',
        Compiler: { Bytes: Host.Bytes, Sha256: Host.Sha256, Record: Host.Recordˉsha256, Checkpoint: Host.Compilerˉkey },
        Validation: Assembly.Fingerprint, Producers: LOADED.map(Producer => Producer.Sha256) });
    return Object.freeze({ Family, Fingerprint, Deadline: Assembly.Deadline,
        Prepared: Assembly.Prepared, Requireˉunchanged, Requireˉhostˉunchanged: Host.Requireˉunchanged });
}

function Record(Key, Input, Object, Report) {
    return Buffer.from(JSON.stringify({ format: NAMESPACE, key: Key,
        input: { bytes: Input.length, sha256: Digest(Input) },
        object: { bytes: Object.length, sha256: Digest(Object) }, report: Report }) + '\n');
}
function Checkˉreport(Report) {
    if (typeof Report !== 'string' || !/^native x64 status=Valid abi=25 code-bytes=[1-9][0-9]{0,6} object-bytes=[1-9][0-9]{0,6}\n$/u.test(Report))
        Reject('Source-object compiler report differs.');
}
async function Validate(Path, Key, Input) {
    await Directory(Path);
    const Reader = await opendir(Path), Entries = [];
    for await (const Entry of Reader) {
        if (Entries.length === 2) Reject('Source-object checkpoint has unexpected entries.');
        Entries.push(Entry.name);
    }
    if (Entries.sort().join(',') !== 'Checkpoint.json,Product.wvo') Reject('Source-object checkpoint inventory differs.');
    const Object = await Read(join(Path, 'Product.wvo'), MAXIMUM_OBJECT_BYTES);
    if (Object.length < 12 || Object.toString('ascii', 0, 4) !== 'WVO1' || Object.readUInt32LE(4) !== 1)
        Reject('Source-object checkpoint header differs.');
    const Manifest = await Read(join(Path, 'Checkpoint.json'), 4096);
    let Value;
    try { Value = JSON.parse(Manifest.toString('utf8')); }
    catch { Reject('Source-object checkpoint record differs.'); }
    Checkˉreport(Value?.report);
    if (!Manifest.equals(Record(Key, Input, Object, Value.report))) Reject('Source-object checkpoint record differs.');
    return { Object, Report: Value.report };
}

// WVB admission belongs to the caller and is repeated before every acquisition.
// A hit still receives the caller's native structural and geometry validation.
export async function Acquireˉsourceˉobject(Context, Inputˉpath, Output, Produce, Check) {
    Time(Context.Deadline);
    if (extname(Inputˉpath) !== '.wvb' || extname(Output) !== '.wvo' ||
        typeof Produce !== 'function' || typeof Check !== 'function') Reject('Invalid source-object cache request.');
    await Directory(dirname(resolve(Output))); await Absent(Output);
    const Input = await Read(Inputˉpath, MAXIMUM_INPUT_BYTES);
    const Key = Digest(Buffer.concat([Buffer.from(Context.Fingerprint + '\n'), Input]));
    const Destination = join(Context.Family, Key);
    const Unchanged = async () => {
        Time(Context.Deadline);
        if (!(await Read(Inputˉpath, MAXIMUM_INPUT_BYTES)).equals(Input)) Reject('Source-object bytecode changed during acquisition.');
    };
    await Context.Requireˉunchanged();
    let Status = 'Hit', Constructed = false;
    const Existing = await lstat(Destination).catch(Error => { if (Error.code === 'ENOENT') return null; throw Error; });
    if (Existing === null) {
        if (Context.Prepared) Reject(`Prepared source object missing key=${Key}; prepare shared source products separately.`, 64);
        const Name = `.new-${process.pid}-${randomBytes(16).toString('hex')}`, Temporary = join(Context.Family, Name);
        await mkdir(Temporary);
        try {
            const Snapshot = join(Temporary, 'Source.wvb'), Product = join(Temporary, 'Product.wvo');
            await writeFile(Snapshot, Input, { flag: 'wx' });
            process.stdout.write(`source object step=compile source=${basename(Inputˉpath)} status=Started\n`);
            Constructed = true;
            const Report = await Produce(Snapshot, Product);
            Checkˉreport(Report);
            const Object = await Read(Product, MAXIMUM_OBJECT_BYTES);
            await Check(Product, Report);
            if (!(await Read(Snapshot, MAXIMUM_INPUT_BYTES)).equals(Input)) Reject('Private source-object bytecode changed.');
            await rm(Snapshot);
            await writeFile(join(Temporary, 'Checkpoint.json'), Record(Key, Input, Object, Report), { flag: 'wx' });
            await Validate(Temporary, Key, Input); await Unchanged(); await Context.Requireˉunchanged();
            try { await rename(Temporary, Destination); Status = 'Created'; }
            catch (Error) {
                if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(Error.code)) throw Error;
                const Winner = await Validate(Destination, Key, Input);
                if (!Winner.Object.equals(Object) || Winner.Report !== Report) Reject('Concurrent source-object outputs differ.');
            }
        } finally {
            if (dirname(Temporary) !== Context.Family || basename(Temporary) !== Name)
                Reject('Refusing to remove an unowned source-object cache directory.');
            if (await lstat(Temporary).catch(Error => { if (Error.code === 'ENOENT') return null; throw Error; }) !== null) {
                await Directory(Temporary); await rm(Temporary, { recursive: true, force: false });
            }
        }
    }
    const Cached = await Validate(Destination, Key, Input);
    await Unchanged();
    await copyFile(join(Destination, 'Product.wvo'), Output, constants.COPYFILE_EXCL);
    if (!(await Read(Output, MAXIMUM_OBJECT_BYTES)).equals(Cached.Object)) Reject('Source-object materialization bytes differ.');
    const Validation = await Check(Output, Cached.Report);
    await Unchanged(); await Context.Requireˉunchanged();
    process.stdout.write(`source object cache status=${Status} key=${Key} source=${basename(Inputˉpath)}\n`);
    return Object.freeze({ Key, Status, Constructed, Validation, Requireˉunchanged: Unchanged });
}
