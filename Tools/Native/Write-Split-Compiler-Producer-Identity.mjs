import { createHash, randomBytes } from 'node:crypto';
import { createReadStream } from 'node:fs';
import {
    lstat,
    open,
    readFile,
    realpath,
    rename,
    stat,
    unlink,
    writeFile,
} from 'node:fs/promises';
import path from 'node:path';

const MAXIMUM_PRODUCER_BYTES = 134_217_728;
const MAXIMUM_IDENTITY_BYTES = 1_024;
const HOST = `${process.platform}-${process.arch}`;

if (process.argv.length !== 5) {
    Usage();
}
if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`The split compiler identity does not support ${HOST}.`);
}

const Role = process.argv[2];
if (Role !== 'analyzer' && Role !== 'emitter') {
    Reject('The producer role must be analyzer or emitter.');
}
const Target = Role === 'analyzer'
    ? 'source-analysis-v1'
    : 'portable-wvb-optimized-v1';
const Producerˉpath = path.resolve(process.argv[3]);
const Identityˉpath = path.resolve(process.argv[4]);
if (Producerˉpath === Identityˉpath) {
    Reject('The split compiler producer and identity paths overlap.');
}
if (path.extname(Identityˉpath).toLowerCase() !== '.identity') {
    Reject('The split compiler producer identity must use .identity.');
}
await Requireˉordinaryˉdirectory(path.dirname(Identityˉpath), 'identity parent');

const Evidence = await Fileˉevidence(Producerˉpath);
const Identity = `windvale-split-compiler-producer 2\n` +
    `role ${Role}\n` +
    `target ${Target}\n` +
    `host ${HOST}\n` +
    `bytes ${Evidence.bytes}\n` +
    `sha256 ${Evidence.sha256}\n`;
await Publishˉidentity(Identityˉpath, Identity);
console.log(
    `split compiler identity status=Published role=${Role} target=${Target} ` +
    `host=${HOST} ` +
    `bytes=${Evidence.bytes} sha256=${Evidence.sha256}`,
);

async function Publishˉidentity(Destination, Text) {
    const Expected = Buffer.from(Text, 'ascii');
    if (await Exists(Destination)) {
        await Requireˉsameˉidentity(Destination, Expected);
        return;
    }
    const Parent = path.dirname(Destination);
    const Prefix = `.new-${path.basename(Destination)}-`;
    const Temporary = path.join(
        Parent,
        `${Prefix}${process.pid}-${randomBytes(8).toString('hex')}`,
    );
    try {
        await writeFile(Temporary, Expected, { flag: 'wx' });
        const Handle = await open(Temporary, 'r+');
        try {
            await Handle.sync();
        } finally {
            await Handle.close();
        }
        try {
            await rename(Temporary, Destination);
        } catch (error) {
            if (error?.code !== 'EEXIST' && error?.code !== 'ENOTEMPTY') {
                throw error;
            }
        }
        await Requireˉsameˉidentity(Destination, Expected);
    } finally {
        const Resolved = path.resolve(Temporary);
        if (path.dirname(Resolved) !== path.resolve(Parent) ||
            !path.basename(Resolved).startsWith(Prefix)) {
            Reject('Refusing to remove an unexpected identity temporary file.');
        }
        await unlink(Resolved).catch(error => {
            if (error?.code !== 'ENOENT') {
                throw error;
            }
        });
    }
}

async function Requireˉsameˉidentity(Candidate, Expected) {
    const Information = await Requireˉordinaryˉfile(
        Candidate,
        'producer identity',
        MAXIMUM_IDENTITY_BYTES,
    );
    if (Information.size !== Expected.length ||
        !(await readFile(Candidate)).equals(Expected)) {
        Reject('The existing split compiler producer identity is different.');
    }
}

async function Fileˉevidence(Candidate) {
    const Information = await Requireˉordinaryˉfile(
        Candidate,
        'producer',
        MAXIMUM_PRODUCER_BYTES,
    );
    const Hash = createHash('sha256');
    let Measured = 0;
    for await (const Chunk of createReadStream(Candidate, {
        highWaterMark: 1_048_576,
    })) {
        Measured += Chunk.length;
        if (Measured > Information.size) {
            Reject('The producer grew while it was hashed.');
        }
        Hash.update(Chunk);
    }
    if (Measured !== Information.size) {
        Reject('The producer changed while it was hashed.');
    }
    return { bytes: Measured, sha256: Hash.digest('hex') };
}

async function Requireˉordinaryˉfile(Candidate, Label, Maximum) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} must use its canonical non-link path: ${Candidate}`);
    }
    return Information;
}

async function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isDirectory() ||
        Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} must use its canonical non-link path: ${Candidate}`);
    }
}

async function Exists(Candidate) {
    return (await stat(Candidate).catch(() => null)) !== null;
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Write-Split-Compiler-Producer-Identity.mjs ' +
        '<analyzer|emitter> <producer> <output.identity>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
