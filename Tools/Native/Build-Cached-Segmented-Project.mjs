import { createHash, randomBytes } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import {
    copyFile,
    lstat,
    mkdir,
    readFile,
    readdir,
    realpath,
    rename,
    rm,
    writeFile
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Getˉnativeˉprojectˉcacheˉkey,
    REPOSITORY_ROOT
} from './Native-Project-Cache-Key-Core.mjs';

const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_IMAGE_BYTES = 33_554_432;
const MAXIMUM_FRAGMENT_BYTES = 4_194_304;
const MAXIMUM_FRAGMENT_COUNT = 8;
const SCRIPT_PATH = fileURLToPath(import.meta.url);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';
const TOOL_ROOT = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Segmented-Compiler-Toolset-Candidate'
);
const TOOL_CONTRACTS = WINDOWS
    ? [
        {
            name: 'segmented WVO producer',
            path: path.join(TOOL_ROOT, 'windows-x64-wvstage.exe'),
            sha256: '6cc939dc3f3e319f036d633626e867078c490564db83814add90b31936bc2bfd'
        },
        {
            name: 'segmented compiler-image linker',
            path: path.join(TOOL_ROOT, 'windows-x64-wvlinkstage.exe'),
            sha256: 'e467d211d141ab75b838ece9b3c4625b6b5b2768b63dcacadd040368844e18db'
        },
        {
            name: 'compiler-image transport',
            path: path.join(TOOL_ROOT, 'windows-x64-wvimagetransport.exe'),
            sha256: '3d1479e286f3486c9ae4cc48a542fb7654cc8bca52ec240f8f3ee030e7c79d92'
        }
    ]
    : [
        {
            name: 'segmented WVO producer',
            path: path.join(TOOL_ROOT, 'linux-x64-wvstage.elf'),
            sha256: '7b9d1b1124b0d7cb09bc9b3d9bfd7c916e7272a40d3e029a39b444c788e1b758'
        },
        {
            name: 'segmented compiler-image linker',
            path: path.join(TOOL_ROOT, 'linux-x64-wvlinkstage.elf'),
            sha256: '7ef825a8054cb8f63c10c957b234f9c371fe1507d7ee20f3e6dbabf73e550cb2'
        },
        {
            name: 'compiler-image transport',
            path: path.join(TOOL_ROOT, 'linux-x64-wvimagetransport.elf'),
            sha256: '30386b1e571b5b444befbfb7c15ee9ce5cb30e7744cf84ddfee89cbf1e2e8108'
        }
    ];

function Reject(message, exitCode = 1) {
    const error = new Error(message);
    error.exitCode = exitCode;
    throw error;
}

function Isˉsameˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
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

async function Readˉordinaryˉfile(candidate, label, maximumBytes) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isFile() ||
        information.isSymbolicLink() || information.size < 1 ||
        information.size > maximumBytes) {
        Reject(`The ${label} is not a bounded ordinary file: ${absolute}`);
    }
    const canonical = await realpath(absolute);
    if (!Isˉsameˉpath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
    }
    return readFile(absolute);
}

async function Measureˉproduct(candidate, label, maximumBytes = MAXIMUM_PRODUCT_BYTES) {
    const bytes = await Readˉordinaryˉfile(candidate, label, maximumBytes);
    return {
        bytes: bytes.length,
        sha256: createHash('sha256').update(bytes).digest('hex')
    };
}

async function Requireˉoutputˉpath(candidate, extension, label) {
    const absolute = path.resolve(candidate);
    if (extension !== '' && path.extname(absolute).toLowerCase() !== extension) {
        Reject(`The ${label} must use the ${extension} extension.`);
    }
    await Requireˉcanonicalˉdirectory(path.dirname(absolute), `${label} parent`);
    const information = await lstat(absolute).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information !== null && (!information.isFile() ||
        information.isSymbolicLink())) {
        Reject(`The ${label} is not an ordinary file: ${absolute}`);
    }
    if (information !== null) {
        const canonical = await realpath(absolute);
        if (!Isˉsameˉpath(canonical, absolute)) {
            Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
        }
    }
    return absolute;
}

function Requireˉcanonicalˉunsigned(text, label, maximum) {
    if (!/^(0|[1-9][0-9]*)$/.test(text)) {
        Reject(`The ${label} is not a canonical unsigned decimal.`);
    }
    const value = Number(text);
    if (!Number.isSafeInteger(value) || value > maximum) {
        Reject(`The ${label} exceeds its bound.`);
    }
    return value;
}

function Checkpointˉrecord(key, entryOffset, wvb, manifest, fragments) {
    const lines = [
        'windvale-native-segmented-project-checkpoint 1',
        `key ${key}`,
        `entry-offset ${entryOffset}`,
        `fragments ${fragments.length}`,
        `wvb-bytes ${wvb.bytes}`,
        `wvb-sha256 ${wvb.sha256}`,
        `manifest-bytes ${manifest.bytes}`,
        `manifest-sha256 ${manifest.sha256}`
    ];
    for (let index = 0; index < fragments.length; index += 1) {
        lines.push(`fragment-${index}-bytes ${fragments[index].bytes}`);
        lines.push(`fragment-${index}-sha256 ${fragments[index].sha256}`);
    }
    lines.push('');
    return Buffer.from(lines.join(RECORD_ENDING), 'ascii');
}

function Parseˉcheckpointˉrecord(bytes, key) {
    const text = bytes.toString('ascii');
    if (!Buffer.from(text, 'ascii').equals(bytes)) {
        Reject('The segmented-project checkpoint record is not ASCII.');
    }
    const lines = text.split(RECORD_ENDING);
    if (lines[0] !== 'windvale-native-segmented-project-checkpoint 1' ||
        lines[1] !== `key ${key}` || !lines[2]?.startsWith('entry-offset ') ||
        !lines[3]?.startsWith('fragments ')) {
        Reject('The segmented-project checkpoint record header differs.');
    }
    const entryOffset = Requireˉcanonicalˉunsigned(
        lines[2].slice('entry-offset '.length),
        'checkpoint entry offset',
        MAXIMUM_IMAGE_BYTES - 1
    );
    const fragmentCount = Requireˉcanonicalˉunsigned(
        lines[3].slice('fragments '.length),
        'checkpoint fragment count',
        MAXIMUM_FRAGMENT_COUNT
    );
    if (fragmentCount < 1 || lines.length !== 9 + fragmentCount * 2 ||
        lines.at(-1) !== '') {
        Reject('The segmented-project checkpoint record length differs.');
    }
    return { entryOffset, fragmentCount };
}

function Validateˉimageˉmanifest(bytes, entryOffset, fragments) {
    if (bytes.length !== 28 + fragments.length * 12 ||
        bytes.subarray(0, 4).toString('ascii') !== 'WVLI' ||
        bytes.readUInt32LE(4) !== 1 || bytes.readUInt32LE(8) !== bytes.length ||
        bytes.readUInt32LE(16) !== entryOffset ||
        bytes.readUInt32LE(20) !== fragments.length ||
        bytes.readUInt32LE(24) !== MAXIMUM_FRAGMENT_BYTES) {
        Reject('The segmented-project image manifest header differs.');
    }
    const imageBytes = bytes.readUInt32LE(12);
    if (imageBytes < 1 || imageBytes > MAXIMUM_IMAGE_BYTES ||
        entryOffset >= imageBytes) {
        Reject('The segmented-project image manifest bounds differ.');
    }
    let position = 0;
    for (let index = 0; index < fragments.length; index += 1) {
        const offset = 28 + index * 12;
        const fragmentBytes = bytes.readUInt32LE(offset + 8);
        if (bytes.readUInt32LE(offset) !== index ||
            bytes.readUInt32LE(offset + 4) !== position ||
            fragmentBytes !== fragments[index].bytes || fragmentBytes < 1 ||
            fragmentBytes > MAXIMUM_FRAGMENT_BYTES ||
            (index + 1 < fragments.length &&
                fragmentBytes !== MAXIMUM_FRAGMENT_BYTES)) {
            Reject(`The segmented-project image fragment ${index} differs.`);
        }
        position += fragmentBytes;
    }
    if (position !== imageBytes) {
        Reject('The segmented-project image fragments do not cover the image.');
    }
}

async function Validateˉcheckpoint(checkpointDirectory, key) {
    await Requireˉcanonicalˉdirectory(checkpointDirectory, 'checkpoint directory');
    const recordPath = path.join(checkpointDirectory, 'Checkpoint.txt');
    const record = await Readˉordinaryˉfile(
        recordPath,
        'checkpoint record',
        4_096
    );
    const parsed = Parseˉcheckpointˉrecord(record, key);
    const expectedEntries = [
        'Checkpoint.txt',
        'Product.wvb',
        'Product.wvli',
        ...Array.from(
            { length: parsed.fragmentCount },
            (_, index) => `Product.chunk-${index}`
        )
    ].sort();
    const entries = (await readdir(checkpointDirectory)).sort();
    if (entries.length !== expectedEntries.length ||
        entries.some((entry, index) => entry !== expectedEntries[index])) {
        Reject(`The segmented-project checkpoint has unexpected entries: ${checkpointDirectory}`);
    }
    const wvbPath = path.join(checkpointDirectory, 'Product.wvb');
    const manifestPath = path.join(checkpointDirectory, 'Product.wvli');
    const fragmentPaths = Array.from(
        { length: parsed.fragmentCount },
        (_, index) => path.join(checkpointDirectory, `Product.chunk-${index}`)
    );
    const wvb = await Measureˉproduct(wvbPath, 'checkpoint WVB');
    const manifestBytes = await Readˉordinaryˉfile(
        manifestPath,
        'checkpoint image manifest',
        28 + MAXIMUM_FRAGMENT_COUNT * 12
    );
    const manifest = {
        bytes: manifestBytes.length,
        sha256: createHash('sha256').update(manifestBytes).digest('hex')
    };
    const fragments = [];
    for (let index = 0; index < fragmentPaths.length; index += 1) {
        fragments.push(await Measureˉproduct(
            fragmentPaths[index],
            `checkpoint image fragment ${index}`,
            MAXIMUM_FRAGMENT_BYTES
        ));
    }
    Validateˉimageˉmanifest(manifestBytes, parsed.entryOffset, fragments);
    if (!record.equals(Checkpointˉrecord(
        key,
        parsed.entryOffset,
        wvb,
        manifest,
        fragments
    ))) {
        Reject(`The segmented-project checkpoint record differs: ${checkpointDirectory}`);
    }
    return {
        entryOffset: parsed.entryOffset,
        fragmentPaths,
        fragments,
        manifest,
        manifestPath,
        wvb,
        wvbPath
    };
}

async function Removeˉtemporaryˉcheckpoint(checkpointFamily, temporary) {
    const family = await Requireˉcanonicalˉdirectory(
        checkpointFamily,
        'segmented-project checkpoint family'
    );
    const candidate = path.resolve(temporary);
    if (!Isˉsameˉpath(path.dirname(candidate), family) ||
        !/^\.new-[0-9a-f]{64}-[0-9]+-[0-9a-f]+$/.test(path.basename(candidate))) {
        Reject(`Refusing to remove an unowned checkpoint candidate: ${candidate}`);
    }
    const information = await lstat(candidate).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return;
    }
    if (!information.isDirectory() || information.isSymbolicLink()) {
        Reject(`The checkpoint candidate is not an ordinary directory: ${candidate}`);
    }
    const canonical = await realpath(candidate);
    if (!Isˉsameˉpath(path.dirname(canonical), family) ||
        !Isˉsameˉpath(canonical, candidate)) {
        Reject(`The checkpoint candidate escaped its family: ${candidate}`);
    }
    await rm(candidate, { recursive: true, force: false, maxRetries: 2 });
}

function Toˉresourceˉpath(candidate) {
    return WINDOWS ? candidate.replaceAll('\\', '/') : candidate;
}

function Runˉproducer(application, arguments_, label) {
    const execution = spawnSync(application, arguments_, {
        encoding: 'utf8',
        maxBuffer: 4_194_304,
        windowsHide: true
    });
    if (execution.error !== undefined || execution.status !== 0) {
        const detail = (execution.stderr ?? '').trim();
        Reject(`${label} failed${detail === '' ? '.' : `: ${detail}`}`);
    }
    return (execution.stdout ?? '').trim();
}

function Parseˉtransportˉreport(report) {
    const match = /^compiler image transport status=Complete image-bytes=([0-9]+) entry-offset=([0-9]+) chunks=([1-8]) manifest-bytes=([0-9]+)$/.exec(report);
    if (match === null) {
        Reject('The compiler-image transport report differs.');
    }
    const imageBytes = Requireˉcanonicalˉunsigned(
        match[1],
        'transport image bytes',
        MAXIMUM_IMAGE_BYTES
    );
    const entryOffset = Requireˉcanonicalˉunsigned(
        match[2],
        'transport entry offset',
        MAXIMUM_IMAGE_BYTES - 1
    );
    const fragmentCount = Number(match[3]);
    const manifestBytes = Requireˉcanonicalˉunsigned(
        match[4],
        'transport manifest bytes',
        28 + MAXIMUM_FRAGMENT_COUNT * 12
    );
    if (imageBytes < 1 || entryOffset >= imageBytes ||
        manifestBytes !== 28 + fragmentCount * 12) {
        Reject('The compiler-image transport report bounds differ.');
    }
    return { entryOffset, fragmentCount };
}

async function Createˉcheckpoint(
    checkpointFamily,
    checkpointDirectory,
    key,
    projectPath,
    buildDriver,
    tools
) {
    const temporary = path.join(
        checkpointFamily,
        `.new-${key}-${process.pid}-${randomBytes(8).toString('hex')}`
    );
    await mkdir(temporary, { recursive: false });
    try {
        const work = path.join(temporary, 'Work');
        await mkdir(work, { recursive: false });
        const candidateWvb = path.join(temporary, 'Product.wvb');
        const objectPrefix = path.join(work, 'Object');
        const objectManifest = path.join(work, 'Object.wvop');
        const imagePrefix = path.join(work, 'Image');
        const imageManifest = path.join(work, 'Image.wvli');
        const canonicalPrefix = path.join(temporary, 'Product');
        const canonicalManifest = path.join(temporary, 'Product.wvli');

        Runˉproducer(buildDriver, [
            '--workspace',
            Toˉresourceˉpath(path.join(REPOSITORY_ROOT, 'Windvale.wvws')),
            '--project',
            Toˉresourceˉpath(projectPath),
            Toˉresourceˉpath(candidateWvb)
        ], 'The segmented-project WVB build');
        Runˉproducer(
            tools[0].path,
            [candidateWvb, objectPrefix, objectManifest],
            'The segmented-project WVO staging'
        );
        Runˉproducer(
            tools[1].path,
            [objectPrefix, objectManifest, imagePrefix, imageManifest],
            'The segmented-project image linking'
        );
        const transport = Parseˉtransportˉreport(Runˉproducer(
            tools[2].path,
            [imagePrefix, imageManifest, canonicalPrefix, canonicalManifest],
            'The segmented-project image transport'
        ));
        await rm(work, { recursive: true, force: false, maxRetries: 2 });

        const wvb = await Measureˉproduct(candidateWvb, 'candidate WVB');
        const manifestBytes = await Readˉordinaryˉfile(
            canonicalManifest,
            'candidate image manifest',
            28 + MAXIMUM_FRAGMENT_COUNT * 12
        );
        const manifest = {
            bytes: manifestBytes.length,
            sha256: createHash('sha256').update(manifestBytes).digest('hex')
        };
        const fragments = [];
        for (let index = 0; index < transport.fragmentCount; index += 1) {
            fragments.push(await Measureˉproduct(
                `${canonicalPrefix}.chunk-${index}`,
                `candidate image fragment ${index}`,
                MAXIMUM_FRAGMENT_BYTES
            ));
        }
        Validateˉimageˉmanifest(manifestBytes, transport.entryOffset, fragments);
        await writeFile(
            path.join(temporary, 'Checkpoint.txt'),
            Checkpointˉrecord(key, transport.entryOffset, wvb, manifest, fragments),
            { flag: 'wx' }
        );
        await Validateˉcheckpoint(temporary, key);
        try {
            await rename(temporary, checkpointDirectory);
            return 'Created';
        } catch (error) {
            if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(error.code)) {
                throw error;
            }
            await Validateˉcheckpoint(checkpointDirectory, key);
            return 'Hit';
        }
    } finally {
        await Removeˉtemporaryˉcheckpoint(checkpointFamily, temporary);
    }
}

async function Main() {
    if (process.argv.length !== 7) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Segmented-Project.mjs ' +
            '<project.wvproj> <build-driver> <output.wvb> ' +
            '<canonical-chunk-prefix> <canonical.wvli>',
            64
        );
    }
    const projectPath = path.resolve(process.argv[2]);
    const buildDriver = path.resolve(process.argv[3]);
    await Readˉordinaryˉfile(buildDriver, 'build driver', MAXIMUM_PRODUCT_BYTES);
    const outputWvb = await Requireˉoutputˉpath(
        process.argv[4],
        '.wvb',
        'materialized WVB'
    );
    const outputPrefix = path.resolve(process.argv[5]);
    await Requireˉcanonicalˉdirectory(
        path.dirname(outputPrefix),
        'materialized image parent'
    );
    const outputManifest = await Requireˉoutputˉpath(
        process.argv[6],
        '.wvli',
        'materialized image manifest'
    );

    const tools = [];
    for (const contract of TOOL_CONTRACTS) {
        const product = await Measureˉproduct(contract.path, contract.name);
        if (product.sha256 !== contract.sha256) {
            Reject(`The ${contract.name} artifact digest is invalid.`);
        }
        tools.push(contract);
    }
    const key = await Getˉnativeˉprojectˉcacheˉkey(
        'database-segmented-project-v1',
        projectPath,
        [buildDriver, ...tools.map(tool => tool.path), SCRIPT_PATH]
    );

    if (WINDOWS && process.env.WINDVALE_NATIVE_CACHE_ROOT === undefined &&
        process.env.LOCALAPPDATA === undefined) {
        Reject('The native project cache root is unavailable.');
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
        'checkpoint root'
    );
    const checkpointProductRoot = await Ensureˉcanonicalˉdirectory(
        path.join(checkpointRoot, 'segmented-project-v1'),
        'segmented-project checkpoint root'
    );
    const checkpointFamily = await Ensureˉcanonicalˉdirectory(
        path.join(checkpointProductRoot, HOST_FAMILY),
        'segmented-project checkpoint family'
    );
    const checkpointDirectory = path.join(checkpointFamily, key);
    let status = 'Hit';
    if (await lstat(checkpointDirectory).catch(() => null) === null) {
        status = await Createˉcheckpoint(
            checkpointFamily,
            checkpointDirectory,
            key,
            projectPath,
            buildDriver,
            tools
        );
    }
    const checkpoint = await Validateˉcheckpoint(checkpointDirectory, key);
    const outputFragments = [];
    for (let index = 0; index < checkpoint.fragmentPaths.length; index += 1) {
        outputFragments.push(await Requireˉoutputˉpath(
            `${outputPrefix}.chunk-${index}`,
            '',
            `materialized image fragment ${index}`
        ));
    }
    const outputs = [outputWvb, outputManifest, ...outputFragments];
    const outputKeys = outputs.map(output =>
        WINDOWS ? output.toLowerCase() : output
    );
    if (new Set(outputKeys).size !== outputKeys.length) {
        Reject('The segmented-project output paths must be distinct.');
    }

    await copyFile(checkpoint.wvbPath, outputWvb);
    await copyFile(checkpoint.manifestPath, outputManifest);
    for (let index = 0; index < outputFragments.length; index += 1) {
        await copyFile(checkpoint.fragmentPaths[index], outputFragments[index]);
    }
    const materializedWvb = await Measureˉproduct(outputWvb, 'materialized WVB');
    const materializedManifest = await Measureˉproduct(
        outputManifest,
        'materialized image manifest',
        28 + MAXIMUM_FRAGMENT_COUNT * 12
    );
    if (materializedWvb.bytes !== checkpoint.wvb.bytes ||
        materializedWvb.sha256 !== checkpoint.wvb.sha256 ||
        materializedManifest.bytes !== checkpoint.manifest.bytes ||
        materializedManifest.sha256 !== checkpoint.manifest.sha256) {
        Reject('A materialized segmented-project product differs from its checkpoint.');
    }
    for (let index = 0; index < outputFragments.length; index += 1) {
        const materialized = await Measureˉproduct(
            outputFragments[index],
            `materialized image fragment ${index}`,
            MAXIMUM_FRAGMENT_BYTES
        );
        if (materialized.bytes !== checkpoint.fragments[index].bytes ||
            materialized.sha256 !== checkpoint.fragments[index].sha256) {
            Reject('A materialized segmented-project fragment differs from its checkpoint.');
        }
    }
    process.stdout.write(
        `native segmented project cache status=${status} key=${key} ` +
        `entry-offset=${checkpoint.entryOffset} ` +
        `fragments=${checkpoint.fragmentPaths.length}\n`
    );
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(error.exitCode ?? 1);
}
