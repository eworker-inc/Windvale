import { createHash } from 'node:crypto';
import { lstat, readFile, realpath, writeFile } from 'node:fs/promises';
import path from 'node:path';

const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const MAXIMUM_SOURCE_WVB_BYTES = 16_777_216;
const MAXIMUM_MANIFEST_BYTES = 6_244;
const MAXIMUM_CHUNK_BYTES = 4_194_304;
const MAXIMUM_IMAGE_BYTES = 67_108_864;
const MAXIMUM_CURRENT_OBJECT_BYTES = 65_536;
const FORMAT = 'windvale-current-source-wvb-publisher-binding 1';
const NATIVE_OBJECTS = Object.freeze([
    [1, 'Windows-X64-Wvb-Publisher.wvo'],
    [2, 'Linux-X64-Wvb-Publisher.wvo'],
    [3, 'Windows-X64-Wvb-Publication-Adapter.wvo'],
    [4, 'Linux-X64-Wvb-Publication-Adapter.wvo'],
    [5, 'X64-Wvb-Publication-Sha256.wvo'],
    [6, 'X64-Publication-Transaction-State.wvo'],
]);

function Reject(message, exitCode = 1) {
    const error = new Error(message);
    error.exitCode = exitCode;
    throw error;
}

function Sameˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

function Sha256(bytes) {
    return createHash('sha256').update(bytes).digest('hex');
}

async function Requireˉcanonicalˉdirectory(candidate, label) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isDirectory() ||
        information.isSymbolicLink()) {
        Reject(`The ${label} is not an ordinary directory: ${absolute}`, 64);
    }
    const canonical = await realpath(absolute);
    if (!Sameˉpath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`, 64);
    }
    return absolute;
}

async function Readˉordinaryˉfile(candidate, label, maximumBytes) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isFile() ||
        information.isSymbolicLink() || information.size < 1 ||
        information.size > maximumBytes) {
        Reject(`The ${label} is not a bounded ordinary file: ${absolute}`, 64);
    }
    const canonical = await realpath(absolute);
    if (!Sameˉpath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`, 64);
    }
    const bytes = await readFile(absolute);
    if (bytes.length !== information.size) {
        Reject(`The ${label} changed while it was read.`);
    }
    return { absolute, bytes, size: information.size, sha256: Sha256(bytes) };
}

async function Requireˉoutputˉpath(candidate, inputPaths) {
    const absolute = path.resolve(candidate);
    await Requireˉcanonicalˉdirectory(path.dirname(absolute), 'publisher binding output parent');
    for (const input of inputPaths) {
        if (Sameˉpath(absolute, input)) {
            Reject('The publisher binding output must be distinct from every input.', 64);
        }
    }
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') return null;
        throw error;
    });
    if (information !== null) {
        Reject('The publisher binding output already exists.', 64);
    }
    return absolute;
}

function Requireˉmanifestˉheader(bytes, magic, label) {
    if (bytes.length < (magic === 'WVOP' ? 24 : 28) ||
        bytes.subarray(0, 4).toString('ascii') !== magic ||
        bytes.readUInt16LE(4) !== 1 ||
        bytes.readUInt16LE(6) !== 0 ||
        bytes.readUInt32LE(8) !== bytes.length) {
        Reject(`The ${label} manifest header differs.`);
    }
}

function Readˉsegmentedˉmanifest(bytes, magic, label) {
    Requireˉmanifestˉheader(bytes, magic, label);
    const isObject = magic === 'WVOP';
    const payloadBytes = bytes.readUInt32LE(12);
    const entryOffset = isObject ? 0 : bytes.readUInt32LE(16);
    const countOffset = isObject ? 16 : 20;
    const limitOffset = isObject ? 20 : 24;
    const entryStart = isObject ? 24 : 28;
    const chunks = bytes.readUInt32LE(countOffset);
    if (payloadBytes < 1 || payloadBytes > MAXIMUM_IMAGE_BYTES ||
        chunks < 1 || chunks > 518 ||
        bytes.readUInt32LE(limitOffset) !== MAXIMUM_CHUNK_BYTES ||
        bytes.length !== entryStart + chunks * 12 ||
        (!isObject && entryOffset >= payloadBytes)) {
        Reject(`The ${label} manifest bounds differ.`);
    }
    const entries = [];
    let position = 0;
    for (let index = 0; index < chunks; index += 1) {
        const offset = entryStart + index * 12;
        const chunkBytes = bytes.readUInt32LE(offset + 8);
        if (bytes.readUInt32LE(offset) !== index ||
            bytes.readUInt32LE(offset + 4) !== position ||
            chunkBytes < 1 || chunkBytes > MAXIMUM_CHUNK_BYTES ||
            chunkBytes > payloadBytes - position) {
            Reject(`The ${label} manifest chunk ${index} differs.`);
        }
        entries.push({ bytes: chunkBytes, position });
        position += chunkBytes;
    }
    if (position !== payloadBytes) {
        Reject(`The ${label} manifest extent differs.`);
    }
    return { bytes: payloadBytes, chunks, entries, entryOffset };
}

async function Readˉchunkˉset(prefix, manifest, label) {
    const absolutePrefix = path.resolve(prefix);
    const parent = await Requireˉcanonicalˉdirectory(
        path.dirname(absolutePrefix),
        `${label} chunk parent`,
    );
    if (!Sameˉpath(path.dirname(absolutePrefix), parent)) {
        Reject(`The ${label} chunk prefix must be canonical.`, 64);
    }
    const chunks = [];
    for (let index = 0; index < manifest.chunks; index += 1) {
        const chunk = await Readˉordinaryˉfile(
            `${absolutePrefix}.chunk-${index}`,
            `${label} chunk ${index}`,
            MAXIMUM_CHUNK_BYTES,
        );
        if (chunk.size !== manifest.entries[index].bytes) {
            Reject(`The ${label} chunk ${index} byte length differs.`);
        }
        chunks.push({ bytes: chunk.size, sha256: chunk.sha256 });
    }
    const trailing = await lstat(`${absolutePrefix}.chunk-${manifest.chunks}`)
        .catch(error => {
            if (error.code === 'ENOENT') return null;
            throw error;
        });
    if (trailing !== null) {
        Reject(`The ${label} chunk set has a trailing chunk.`);
    }
    return chunks;
}

function Bindingˉlines(values, bindingSha256 = null) {
    const lines = [
        FORMAT,
        `host ${HOST_FAMILY}`,
    ];
    if (bindingSha256 !== null) {
        lines.push(`binding-sha256 ${bindingSha256}`);
    }
    lines.push(
        `source-wvb-bytes ${values.source.size}`,
        `source-wvb-sha256 ${values.source.sha256}`,
        `object-manifest-bytes ${values.objectManifest.bytes}`,
        `object-manifest-sha256 ${values.objectManifest.sha256}`,
        `object-wvo-bytes ${values.object.bytes}`,
        `object-chunks ${values.object.chunks}`,
        `image-manifest-bytes ${values.imageManifest.bytes}`,
        `image-manifest-sha256 ${values.imageManifest.sha256}`,
        `image-bytes ${values.image.bytes}`,
        `image-entry-offset ${values.image.entryOffset}`,
        `image-chunks ${values.image.chunks}`,
    );
    for (let index = 0; index < values.objectChunks.length; index += 1) {
        lines.push(`object-chunk-${index}-bytes ${values.objectChunks[index].bytes}`);
        lines.push(`object-chunk-${index}-sha256 ${values.objectChunks[index].sha256}`);
    }
    for (let index = 0; index < values.imageChunks.length; index += 1) {
        lines.push(`image-chunk-${index}-bytes ${values.imageChunks[index].bytes}`);
        lines.push(`image-chunk-${index}-sha256 ${values.imageChunks[index].sha256}`);
    }
    for (const object of values.nativeObjects) {
        lines.push(
            `native-object-${object.role} ${object.leaf} ` +
            `bytes ${object.bytes} sha256 ${object.sha256}`,
        );
    }
    lines.push('');
    return lines;
}

async function Main() {
    if (process.argv.length !== 9) {
        Reject(
            'Usage: node Tools/Native/Build-Current-Publisher-Binding.mjs ' +
            '<publisher.wvb> <object-chunk-prefix> <publisher.wvop> ' +
            '<image-chunk-prefix> <publisher.wvli> <reference-object-directory> ' +
            '<output.wvcp>',
            64,
        );
    }
    const source = await Readˉordinaryˉfile(
        process.argv[2],
        'current publisher WVB',
        MAXIMUM_SOURCE_WVB_BYTES,
    );
    const objectManifestFile = await Readˉordinaryˉfile(
        process.argv[4],
        'current publisher WVOP manifest',
        MAXIMUM_MANIFEST_BYTES,
    );
    const imageManifestFile = await Readˉordinaryˉfile(
        process.argv[6],
        'current publisher WVLI manifest',
        MAXIMUM_MANIFEST_BYTES,
    );
    const objectDirectory = await Requireˉcanonicalˉdirectory(
        process.argv[7],
        'current publisher reference object directory',
    );
    const nativeObjects = [];
    for (const [role, leaf] of NATIVE_OBJECTS) {
        const object = await Readˉordinaryˉfile(
            path.join(objectDirectory, leaf),
            `current publisher native object role ${role}`,
            MAXIMUM_CURRENT_OBJECT_BYTES,
        );
        nativeObjects.push({
            role,
            leaf,
            bytes: object.size,
            sha256: object.sha256,
            path: object.absolute,
        });
    }
    const inputPaths = [
        source.absolute,
        objectManifestFile.absolute,
        imageManifestFile.absolute,
        ...nativeObjects.map(object => object.path),
    ];
    const output = await Requireˉoutputˉpath(process.argv[8], inputPaths);
    const object = Readˉsegmentedˉmanifest(
        objectManifestFile.bytes,
        'WVOP',
        'current publisher object',
    );
    const image = Readˉsegmentedˉmanifest(
        imageManifestFile.bytes,
        'WVLI',
        'current publisher image',
    );
    const objectChunks = await Readˉchunkˉset(process.argv[3], object, 'current publisher object');
    const imageChunks = await Readˉchunkˉset(process.argv[5], image, 'current publisher image');
    const values = {
        source,
        objectManifest: { bytes: objectManifestFile.size, sha256: objectManifestFile.sha256 },
        object,
        objectChunks,
        imageManifest: { bytes: imageManifestFile.size, sha256: imageManifestFile.sha256 },
        image,
        imageChunks,
        nativeObjects,
    };
    const unsignedRecord = Buffer.from(Bindingˉlines(values).join('\n'), 'ascii');
    const bindingSha256 = Sha256(unsignedRecord);
    const record = Buffer.from(Bindingˉlines(values, bindingSha256).join('\n'), 'ascii');
    await writeFile(output, record, { flag: 'wx' });
    process.stdout.write('current publisher binding status=Valid format=1 native-objects=6\n');
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exitCode = error.exitCode ?? 1;
}
