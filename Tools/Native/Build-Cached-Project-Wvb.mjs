import { createHash, randomBytes } from 'node:crypto';
import { createReadStream } from 'node:fs';
import { copyFile, lstat, mkdir, realpath, rename, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { Getˉnativeˉprojectˉcacheˉkey, REPOSITORY_ROOT } from './Native-Project-Cache-Key-Core.mjs';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';

const WINDOWS = process.platform === 'win32';
const HOST = WINDOWS ? 'windows-x64' : 'linux-x64';
const ENDING = WINDOWS ? '\r\n' : '\n';
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const NAMESPACE = 'project-wvb-v2';
const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));

function Reject(Message) { throw new Error(Message); }
function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}

async function Directory(Candidate, Create = false) {
    const Resolved = path.resolve(Candidate);
    const Root = path.parse(Resolved).root;
    let Current = Root;
    for (const Part of Resolved.slice(Root.length).split(path.sep).filter(Boolean)) {
        Current = path.join(Current, Part);
        let Information;
        try { Information = await lstat(Current); }
        catch (Error) {
            if (!Create || Error.code !== 'ENOENT') throw Error;
            await mkdir(Current).catch(Error => {
                if (Error.code !== 'EEXIST') throw Error;
            });
            Information = await lstat(Current);
        }
        if (!Information.isDirectory() || Information.isSymbolicLink()) {
            Reject('The project-WVB cache path contains a link or non-directory.');
        }
    }
    if (!Sameˉpath(await realpath(Resolved), Resolved)) Reject('The project-WVB path is not canonical.');
    return Resolved;
}

async function Evidence(Candidate, Maximum = MAXIMUM_PRODUCT_BYTES, Retain = false) {
    const Information = await lstat(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        !Sameˉpath(await realpath(Candidate), path.resolve(Candidate))) {
        Reject('The project-WVB cache file is not a bounded ordinary file.');
    }
    const Hash = createHash('sha256');
    const Chunks = [];
    let Bytes = 0;
    for await (const Chunk of createReadStream(Candidate, { highWaterMark: Math.min(Maximum, 1_048_576) })) {
        Bytes += Chunk.length;
        if (Bytes > Information.size) Reject('The project-WVB file grew while read.');
        Hash.update(Chunk);
        if (Retain) Chunks.push(Chunk);
    }
    if (Bytes !== Information.size) Reject('The project-WVB file changed while read.');
    return { bytes: Bytes, sha256: Hash.digest('hex'),
        contents: Retain ? Buffer.concat(Chunks, Bytes) : null };
}

function Manifest(Key, Product) {
    return Buffer.from(['windvale-native-project-wvb-checkpoint 1', `key ${Key}`,
        `wvb-bytes ${Product.bytes}`, `wvb-sha256 ${Product.sha256}`, ''].join(ENDING));
}

async function Validate(Place, Key) {
    await Directory(Place);
    const Record = path.join(Place, 'Checkpoint.txt');
    const [Product, Metadata] = await Promise.all([
        Evidence(path.join(Place, 'Product.wvb')),
        Evidence(Record, 1_024, true),
    ]);
    if (!Metadata.contents.equals(Manifest(Key, Product))) {
        Reject('The project-WVB checkpoint manifest differs.');
    }
    return Product;
}

async function Exists(Candidate) {
    return lstat(Candidate).then(() => true).catch(Error => {
        if (Error.code === 'ENOENT') return false;
        throw Error;
    });
}

async function Outputˉtarget(Candidate) {
    if (!await Exists(Candidate)) return;
    const Information = await lstat(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        !Sameˉpath(await realpath(Candidate), path.resolve(Candidate))) {
        Reject('The project-WVB output is not an ordinary non-link file.');
    }
}

export async function Buildˉcachedˉprojectˉwvb(
    Projectˉargument, Outputˉargument, Execute = Runˉdevelopmentˉcommand,
) {
    const Deadline = Date.now() + 600_000;
    if (process.arch !== 'x64' || !['win32', 'linux'].includes(process.platform)) {
        Reject('Unsupported project-WVB cache host.');
    }
    const Project = path.resolve(Projectˉargument);
    const Output = path.resolve(Outputˉargument);
    await Directory(path.dirname(Project));
    await Directory(path.dirname(Output));
    const Projectˉevidence = await Evidence(Project, 65_536, true);
    await Outputˉtarget(Output);
    const Header = Projectˉevidence.contents.toString('utf8').split(/\r?\n/u)[0];
    // Project 4 retains the existing current compiler and prepared-only policy.
    if (Header === 'windvale-project 4') {
        const Result = await Execute(process.execPath,
            [path.join(SCRIPT_DIRECTORY, 'Build-Current-Split-Project-Wvb.mjs'),
                '--deadline-ms', String(Deadline - 7_500), Project, Output],
            Deadline, true, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 0) throw Object.assign(new Error(Result.Error || 'Current project build failed.'),
            { exitCode: Result.Code ?? 1 });
        return;
    }
    const Driver = path.join(REPOSITORY_ROOT, 'Artifacts', 'Native-Compiler-Reconstruction-Candidate',
        HOST, WINDOWS ? 'wvbuild.exe' : 'wvbuild.elf');
    const Inventory = path.join(REPOSITORY_ROOT, 'Artifacts', 'Native-Front-Door', 'SHA256SUMS');
    const Getˉkey = () => Getˉnativeˉprojectˉcacheˉkey(NAMESPACE, Project, [Inventory, Driver]);
    const Key = await Getˉkey();
    const Root = process.env.WINDVALE_NATIVE_CACHE_ROOT || (WINDOWS
        ? process.env.LOCALAPPDATA && path.join(process.env.LOCALAPPDATA, 'Windvale', 'Native-Tool-Cache')
        : path.join(process.env.XDG_CACHE_HOME || path.join(os.homedir(), '.cache'), 'windvale', 'native-tool-cache'));
    if (!Root) Reject('The project-WVB cache root is unavailable.');
    const Family = await Directory(path.join(Root, NAMESPACE, HOST), true);
    const Place = path.join(Family, Key);
    let Status = 'Hit';
    if (!await Exists(Place)) {
        const Temporary = path.join(Family, `.new-${Key}-${process.pid}-${randomBytes(16).toString('hex')}`);
        await mkdir(Temporary);
        let Primaryˉfailure = null;
        try {
            const Product = path.join(Temporary, 'Product.wvb');
            const Resource = Value => WINDOWS ? Value.replaceAll('\\', '/') : Value;
            const Result = await Execute(Driver, ['--workspace',
                Resource(path.join(REPOSITORY_ROOT, 'Windvale.wvws')), '--project',
                Resource(Project), Resource(Product)], Deadline - 7_500, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 0) Reject(`The project-WVB cache build failed: ${Result.Output}${Result.Error}`);
            const Built = await Evidence(Product);
            if (await Getˉkey() !== Key) Reject('Project-WVB construction inputs changed.');
            await writeFile(path.join(Temporary, 'Checkpoint.txt'), Manifest(Key, Built), { flag: 'wx' });
            await Validate(Temporary, Key);
            try { await rename(Temporary, Place); Status = 'Created'; }
            catch (Error) {
                if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(Error.code)) throw Error;
            }
            if (!Manifest(Key, await Validate(Place, Key)).equals(Manifest(Key, Built))) {
                Reject('Concurrent project-WVB construction differs.');
            }
        } catch (Failure) {
            Primaryˉfailure = Failure;
            throw Failure;
        } finally {
            if (path.dirname(path.resolve(Temporary)) !== Family ||
                !path.basename(Temporary).startsWith(`.new-${Key}-`)) {
                Reject('Refusing to remove an unexpected project-WVB temporary directory.');
            }
            if (Primaryˉfailure?.cleanupUncertain === true) {
                process.stderr.write(`Preserved project-WVB work after uncertain process termination: ${Temporary}\n`);
            } else {
                try {
                    if (await Exists(Temporary)) {
                        await Directory(Temporary);
                        await rm(Temporary, { recursive: true, force: true, maxRetries: 2 });
                    }
                } catch (Failure) {
                    if (Primaryˉfailure === null) throw Failure;
                    Primaryˉfailure.cleanupFailure = Failure;
                }
            }
        }
    }
    const Product = await Validate(Place, Key);
    if (Date.now() >= Deadline) {
        throw Object.assign(new Error('Project-WVB deadline expired before publication.'), { exitCode: 124 });
    }
    await Outputˉtarget(Output);
    await copyFile(path.join(Place, 'Product.wvb'), Output);
    const Published = await Evidence(Output);
    if (Published.bytes !== Product.bytes || Published.sha256 !== Product.sha256) {
        Reject('Published project-WVB bytes differ.');
    }
    process.stdout.write(`native project wvb cache status=${Status} key=${Key}\n`);
}

if (process.argv[1] !== undefined && Sameˉpath(path.resolve(process.argv[1]), fileURLToPath(import.meta.url))) {
    const Arguments = process.argv.slice(2);
    const Extension = (Value, Expected) => WINDOWS
        ? path.extname(Value).toLowerCase() === Expected : path.extname(Value) === Expected;
    if (Arguments.length !== 2 || !Extension(Arguments[0], '.wvproj') || !Extension(Arguments[1], '.wvb')) {
        process.stderr.write('Usage: Build-Cached-Project-Wvb <project.wvproj> <output.wvb>\n');
        process.exitCode = 64;
    } else {
        try { await Buildˉcachedˉprojectˉwvb(...Arguments); }
        catch (Error) {
            const Cleanup = Error.cleanupFailure === undefined ? '' :
                `; temporary cleanup failed: ${Error.cleanupFailure.message}`;
            process.stderr.write((String(Error.message) + Cleanup).slice(0, MAXIMUM_DIAGNOSTIC_BYTES) + '\n');
            process.exitCode = Number.isInteger(Error.exitCode) ? Error.exitCode : 1;
        }
    }
}
