import { spawn } from 'node:child_process';
import { createHash, randomBytes } from 'node:crypto';
import {
    chmod,
    copyFile,
    lstat,
    mkdir,
    mkdtemp,
    open,
    readFile,
    realpath,
    readdir,
    rename,
    rm,
    stat,
    writeFile
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { TextDecoder } from 'node:util';

const MAXIMUM_INPUT_BYTES = 33_554_432;
const MAXIMUM_AGGREGATE_INPUT_BYTES = 33_554_432;
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';
const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const FRONT_DOOR = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    `Link-Wvo.${WINDOWS ? 'cmd' : 'sh'}`
);
const LINKER = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Wv-Linker-Candidate',
    `Wv-Linker.${WINDOWS ? 'exe' : 'elf'}`
);
const UTF8 = new TextDecoder('utf-8', { fatal: true });

function Reject(message, exitCode = 1) {
    const error = new Error(message);
    error.exitCode = exitCode;
    throw error;
}

function Isˉsameˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

function Isˉwithin(candidate, parent) {
    const relative = path.relative(parent, candidate);
    return relative !== '' && relative !== '..' &&
        !relative.startsWith(`..${path.sep}`) && !path.isAbsolute(relative);
}

async function Readˉordinaryˉfile(
    candidate,
    label,
    maximumBytes,
    allowWindowsAlias = false
) {
    const absolute = path.resolve(candidate);
    let linkInformation;
    let information;
    try {
        linkInformation = await lstat(absolute);
        information = await stat(absolute);
    } catch {
        Reject(`Missing ${label}: ${absolute}`);
    }
    if (linkInformation.isSymbolicLink() || !information.isFile() ||
        information.size < 1 || information.size > maximumBytes) {
        Reject(`The ${label} is not a bounded ordinary file: ${absolute}`);
    }
    const canonical = await realpath(absolute).catch(() => '');
    if (!Isˉsameˉpath(canonical, absolute) && !(WINDOWS && allowWindowsAlias)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
    }
    return {
        bytes: await readFile(WINDOWS && allowWindowsAlias ? canonical : absolute),
        path: absolute
    };
}

async function Requireˉcanonicalˉdirectory(candidate, label) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isDirectory() ||
        information.isSymbolicLink()) {
        Reject(`The ${label} is not an ordinary directory: ${absolute}`);
    }
    const canonical = await realpath(absolute);
    if (!Isˉsameˉpath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
    }
    return absolute;
}

async function Ensureˉcanonicalˉdirectory(candidate, label) {
    await mkdir(candidate, { recursive: true });
    return Requireˉcanonicalˉdirectory(candidate, label);
}

async function Requireˉoutputˉpath(candidate, extension, checkpointRoot) {
    const absolute = path.resolve(candidate);
    if (path.extname(absolute).toLowerCase() !== extension) {
        Reject(`The linked-image output must use the ${extension} extension.`);
    }
    if (Isˉwithin(absolute, checkpointRoot) || Isˉsameˉpath(absolute, checkpointRoot)) {
        Reject('A linked-image owner output cannot be inside the checkpoint root.');
    }
    await Requireˉcanonicalˉdirectory(
        path.dirname(absolute),
        'linked-image output parent'
    );
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information !== null && (!information.isFile() ||
        information.isSymbolicLink())) {
        Reject(`The linked-image output is not an ordinary file: ${absolute}`);
    }
    if (information !== null) {
        const canonical = await realpath(absolute);
        if (!Isˉsameˉpath(canonical, absolute)) {
            Reject(`The linked-image output must use its canonical path: ${absolute}`);
        }
    }
    return absolute;
}

function Addˉfield(hash, label, bytes) {
    const labelBytes = Buffer.from(label, 'utf8');
    const frame = Buffer.alloc(16);
    frame.writeBigUInt64LE(BigInt(labelBytes.length), 0);
    frame.writeBigUInt64LE(BigInt(bytes.length), 8);
    hash.update(frame);
    hash.update(labelBytes);
    hash.update(bytes);
}

function Requireˉcanonicalˉunsigned(value, label) {
    if (!/^(0|[1-9][0-9]{0,19})$/.test(value) ||
        BigInt(value) > 0xffff_ffff_ffff_ffffn) {
        Reject(`The ${label} is not canonical u64 decimal.`);
    }
    return value;
}

async function Prepareˉrequest() {
    if (process.argv.length < 8) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Linked-Image-Set.mjs ' +
            '<base-address> <entry> <output.bin> <output.map> ' +
            '<input-count> <input.wvo>...',
            64
        );
    }
    const baseAddress = Requireˉcanonicalˉunsigned(
        process.argv[2],
        'linked-image base address'
    );
    const entry = process.argv[3];
    if (!/^[A-Za-z_][A-Za-z0-9_]{0,127}$/.test(entry)) {
        Reject('The linked-image entry symbol is invalid.', 64);
    }
    const inputCountText = process.argv[6];
    if (!/^(?:[1-9]|[1-5][0-9]|6[0-4])$/.test(inputCountText)) {
        Reject('The linked-image input count is outside one through 64.', 64);
    }
    const inputCount = Number(inputCountText);
    if (process.argv.length !== 7 + inputCount) {
        Reject('The linked-image input count differs from its arguments.', 64);
    }

    const inputs = [];
    let aggregateBytes = 0;
    for (let index = 0; index < inputCount; index += 1) {
        const input = await Readˉordinaryˉfile(
            process.argv[7 + index],
            `linked-image input ${index}`,
            MAXIMUM_INPUT_BYTES,
            true
        );
        if (path.extname(input.path).toLowerCase() !== '.wvo') {
            Reject(`Linked-image input ${index} is not a WVO.`, 64);
        }
        aggregateBytes += input.bytes.length;
        if (aggregateBytes > MAXIMUM_AGGREGATE_INPUT_BYTES) {
            Reject('The linked-image aggregate input exceeds 32 MiB.');
        }
        inputs.push(input);
    }

    const producers = [];
    for (const [label, candidate] of [
        ['Build-Cached-Linked-Image-Set', SCRIPT_PATH],
        ['Link-Wvo', FRONT_DOOR],
        ['Wv-Linker', LINKER]
    ]) {
        producers.push({
            label,
            ...(await Readˉordinaryˉfile(
                candidate,
                `linked-image producer ${label}`,
                MAXIMUM_PRODUCT_BYTES
            ))
        });
    }

    const hash = createHash('sha256');
    Addˉfield(
        hash,
        'format',
        Buffer.from('windvale-native-linked-image-cache-key 2\n', 'ascii')
    );
    Addˉfield(hash, 'namespace', Buffer.from('linked-image-v2', 'ascii'));
    Addˉfield(hash, 'host', Buffer.from(HOST_FAMILY, 'ascii'));
    Addˉfield(hash, 'base-address', Buffer.from(baseAddress, 'ascii'));
    Addˉfield(hash, 'entry', Buffer.from(entry, 'ascii'));
    Addˉfield(hash, 'input-count', Buffer.from(inputCountText, 'ascii'));
    for (let index = 0; index < inputs.length; index += 1) {
        Addˉfield(hash, `input-wvo:${index}`, inputs[index].bytes);
    }
    for (const producer of producers) {
        Addˉfield(hash, `producer:${producer.label}`, producer.bytes);
    }

    return {
        baseAddress,
        entry,
        inputCount,
        inputs,
        key: hash.digest('hex'),
        outputImageArgument: process.argv[4],
        outputMapArgument: process.argv[5],
        producers
    };
}

async function Measureˉfile(candidate, label, maximumBytes) {
    const measured = await Readˉordinaryˉfile(candidate, label, maximumBytes);
    return {
        bytes: measured.bytes.length,
        sha256: createHash('sha256').update(measured.bytes).digest('hex')
    };
}

function Parseˉentryˉoffset(mapBytes, entry) {
    let text;
    try {
        text = UTF8.decode(mapBytes);
    } catch {
        Reject('The linked-image map is not valid UTF-8.');
    }
    if (!text.endsWith('\n') || text.includes('\r')) {
        Reject('The linked-image map does not use canonical LF text.');
    }
    const prefix = `entry name=${entry} address=`;
    const matches = text.split('\n').filter(line => line.startsWith(prefix));
    if (matches.length !== 1) {
        Reject('The linked-image map does not contain exactly one requested entry.');
    }
    return Requireˉcanonicalˉunsigned(
        matches[0].slice(prefix.length),
        'linked-image entry offset'
    );
}

function Checkpointˉrecord(request, entryOffset, image, map) {
    return Buffer.from([
        'windvale-native-linked-image-checkpoint 2',
        `key ${request.key}`,
        `input-count ${request.inputCount}`,
        `entry-offset ${entryOffset}`,
        `image-bytes ${image.bytes}`,
        `image-sha256 ${image.sha256}`,
        `map-bytes ${map.bytes}`,
        `map-sha256 ${map.sha256}`,
        ''
    ].join(RECORD_ENDING), 'ascii');
}

async function Validateˉcheckpoint(directory, request) {
    await Requireˉcanonicalˉdirectory(directory, 'linked-image checkpoint');
    const entries = (await readdir(directory)).sort();
    const expectedEntries = ['Checkpoint.txt', 'Product.bin', 'Product.map'];
    if (entries.length !== expectedEntries.length ||
        entries.some((entry, index) => entry !== expectedEntries[index])) {
        Reject(`The linked-image checkpoint has unexpected entries: ${directory}`);
    }
    const recordPath = path.join(directory, 'Checkpoint.txt');
    const imagePath = path.join(directory, 'Product.bin');
    const mapPath = path.join(directory, 'Product.map');
    const record = (await Readˉordinaryˉfile(
        recordPath,
        'linked-image checkpoint record',
        1_024
    )).bytes;
    const image = await Measureˉfile(
        imagePath,
        'linked-image checkpoint image',
        MAXIMUM_PRODUCT_BYTES
    );
    const mapFile = await Readˉordinaryˉfile(
        mapPath,
        'linked-image checkpoint map',
        MAXIMUM_PRODUCT_BYTES
    );
    const map = {
        bytes: mapFile.bytes.length,
        sha256: createHash('sha256').update(mapFile.bytes).digest('hex')
    };
    const entryOffset = Parseˉentryˉoffset(mapFile.bytes, request.entry);
    if (!record.equals(Checkpointˉrecord(request, entryOffset, image, map))) {
        Reject(`The linked-image checkpoint record differs: ${directory}`);
    }
    return { entryOffset, image, imagePath, map, mapPath };
}

function Runˉlinker(arguments_, mapHandle) {
    return new Promise((resolve, reject) => {
        if (WINDOWS && [FRONT_DOOR, ...arguments_].some(
            argument => /[\r\n&|<>^%!"]/u.test(argument))) {
            Reject('A Windows linked-image producer argument contains shell metacharacters.');
        }
        const executable = WINDOWS
            ? process.env.ComSpec ?? 'cmd.exe'
            : FRONT_DOOR;
        const producerArguments = WINDOWS
            ? [
                '/d',
                '/v:off',
                '/s',
                '/c',
                `"${[FRONT_DOOR, ...arguments_]
                    .map(argument => `"${argument}"`)
                    .join(' ')}"`
            ]
            : arguments_;
        const child = spawn(executable, producerArguments, {
            cwd: REPOSITORY_ROOT,
            stdio: ['ignore', mapHandle.fd, 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: WINDOWS
        });
        const diagnostics = [];
        let diagnosticBytes = 0;
        child.stderr.on('data', chunk => {
            diagnosticBytes += chunk.length;
            if (diagnosticBytes <= MAXIMUM_DIAGNOSTIC_BYTES) {
                diagnostics.push(chunk);
            } else {
                child.kill();
            }
        });
        child.on('error', reject);
        child.on('close', code => {
            const diagnostic = Buffer.concat(diagnostics).toString('utf8').trim();
            if (diagnosticBytes > MAXIMUM_DIAGNOSTIC_BYTES) {
                reject(new Error('The linked-image producer diagnostic is oversized.'));
            } else if (code !== 0) {
                reject(new Error(
                    `The linked-image producer failed with ${code}` +
                    `${diagnostic === '' ? '.' : `: ${diagnostic}`}`
                ));
            } else {
                resolve();
            }
        });
    });
}

async function Requireˉunchangedˉproducers(producers) {
    for (const producer of producers) {
        const current = await Readˉordinaryˉfile(
            producer.path,
            `linked-image producer ${producer.label}`,
            MAXIMUM_PRODUCT_BYTES
        );
        if (!current.bytes.equals(producer.bytes)) {
            Reject(`Linked-image producer ${producer.label} changed during publication.`);
        }
    }
}

async function Removeˉownedˉtemporary(temporary, checkpointFamily, key) {
    if (temporary === null) {
        return;
    }
    const absolute = path.resolve(temporary);
    if (!Isˉsameˉpath(path.dirname(absolute), checkpointFamily) ||
        !path.basename(absolute).startsWith(`.new-${key}-`)) {
        Reject(`Refusing to remove an unexpected linked-image temporary: ${absolute}`);
    }
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return;
    }
    if (!information.isDirectory() || information.isSymbolicLink()) {
        Reject(`The linked-image temporary is not an owned directory: ${absolute}`);
    }
    await rm(absolute, { recursive: true, force: false, maxRetries: 2 });
}

async function Createˉcheckpoint(request, checkpointFamily, checkpointDirectory) {
    let temporary = null;
    try {
        temporary = await mkdtemp(path.join(
            checkpointFamily,
            `.new-${request.key}-${randomBytes(8).toString('hex')}-`
        ));
        const snapshotInputs = [];
        for (let index = 0; index < request.inputs.length; index += 1) {
            const snapshot = path.join(
                temporary,
                `Input-${String(index).padStart(2, '0')}.wvo`
            );
            await writeFile(snapshot, request.inputs[index].bytes, { flag: 'wx' });
            snapshotInputs.push(snapshot);
        }
        const imagePath = path.join(temporary, 'Product.bin');
        const mapPath = path.join(temporary, 'Product.map');
        const mapHandle = await open(mapPath, 'wx');
        try {
            await Runˉlinker([
                request.baseAddress,
                request.entry,
                imagePath,
                ...snapshotInputs
            ], mapHandle);
        } finally {
            await mapHandle.close();
        }
        for (const snapshot of snapshotInputs) {
            await rm(snapshot, { force: false });
        }
        const image = await Measureˉfile(
            imagePath,
            'linked-image candidate image',
            MAXIMUM_PRODUCT_BYTES
        );
        const mapFile = await Readˉordinaryˉfile(
            mapPath,
            'linked-image candidate map',
            MAXIMUM_PRODUCT_BYTES
        );
        const map = {
            bytes: mapFile.bytes.length,
            sha256: createHash('sha256').update(mapFile.bytes).digest('hex')
        };
        const entryOffset = Parseˉentryˉoffset(mapFile.bytes, request.entry);
        await Requireˉunchangedˉproducers(request.producers);
        await writeFile(
            path.join(temporary, 'Checkpoint.txt'),
            Checkpointˉrecord(request, entryOffset, image, map),
            { flag: 'wx' }
        );
        try {
            await rename(temporary, checkpointDirectory);
            temporary = null;
            return 'Created';
        } catch (error) {
            const winner = await lstat(checkpointDirectory).catch(
                lookupError => {
                    if (lookupError.code === 'ENOENT') {
                        return null;
                    }
                    throw lookupError;
                }
            );
            if (winner === null) {
                throw error;
            }
            await Validateˉcheckpoint(checkpointDirectory, request);
            return 'Hit';
        }
    } finally {
        await Removeˉownedˉtemporary(temporary, checkpointFamily, request.key);
    }
}

async function Materializeˉcheckpoint(checkpoint, outputImage, outputMap) {
    await copyFile(checkpoint.imagePath, outputImage);
    await copyFile(checkpoint.mapPath, outputMap);
    if (!WINDOWS) {
        await chmod(outputImage, (await stat(checkpoint.imagePath)).mode & 0o777);
        await chmod(outputMap, (await stat(checkpoint.mapPath)).mode & 0o777);
    }
    const materializedImage = await Measureˉfile(
        outputImage,
        'materialized linked-image image',
        MAXIMUM_PRODUCT_BYTES
    );
    const materializedMap = await Measureˉfile(
        outputMap,
        'materialized linked-image map',
        MAXIMUM_PRODUCT_BYTES
    );
    if (materializedImage.bytes !== checkpoint.image.bytes ||
        materializedImage.sha256 !== checkpoint.image.sha256 ||
        materializedMap.bytes !== checkpoint.map.bytes ||
        materializedMap.sha256 !== checkpoint.map.sha256) {
        Reject('The materialized linked-image outputs differ from the checkpoint.');
    }
}

async function Main() {
    const request = await Prepareˉrequest();
    if (WINDOWS && process.env.WINDVALE_NATIVE_CACHE_ROOT === undefined &&
        process.env.LOCALAPPDATA === undefined) {
        Reject('The native linked-image cache root is unavailable.');
    }
    const configuredRoot = process.env.WINDVALE_NATIVE_CACHE_ROOT ?? (
        WINDOWS
            ? path.join(process.env.LOCALAPPDATA, 'Windvale', 'Native-Tool-Cache')
            : path.join(
                process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache'),
                'windvale',
                'native-tool-cache'
            )
    );
    const checkpointRoot = await Ensureˉcanonicalˉdirectory(
        path.resolve(configuredRoot),
        'linked-image checkpoint root'
    );
    const outputImage = await Requireˉoutputˉpath(
        request.outputImageArgument,
        '.bin',
        checkpointRoot
    );
    const outputMap = await Requireˉoutputˉpath(
        request.outputMapArgument,
        '.map',
        checkpointRoot
    );
    if (Isˉsameˉpath(outputImage, outputMap)) {
        Reject('The linked-image image and map outputs must be distinct.', 64);
    }
    const productRoot = await Ensureˉcanonicalˉdirectory(
        path.join(checkpointRoot, 'linked-image-v2'),
        'linked-image checkpoint product root'
    );
    const checkpointFamily = await Ensureˉcanonicalˉdirectory(
        path.join(productRoot, HOST_FAMILY),
        'linked-image checkpoint family'
    );
    const checkpointDirectory = path.join(checkpointFamily, request.key);
    let status = 'Hit';
    const existing = await lstat(checkpointDirectory).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (existing === null) {
        status = await Createˉcheckpoint(
            request,
            checkpointFamily,
            checkpointDirectory
        );
    }
    const checkpoint = await Validateˉcheckpoint(checkpointDirectory, request);
    await Materializeˉcheckpoint(checkpoint, outputImage, outputMap);
    process.stdout.write(
        `native linked image cache status=${status} key=${request.key} ` +
        `entry=${checkpoint.entryOffset} inputs=${request.inputCount}\n`
    );
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(error.exitCode ?? 1);
}
