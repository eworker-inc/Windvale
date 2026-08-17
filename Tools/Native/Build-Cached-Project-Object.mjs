import { createHash } from 'node:crypto';
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
    Getˉnativeˉprojectˉcacheˉrequest,
    Prepareˉnativeˉprojectˉcacheˉcontext,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
    REPOSITORY_ROOT
} from './Native-Project-Cache-Key-Core.mjs';

const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const SCRIPT_PATH = fileURLToPath(import.meta.url);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';
const INSPECTOR_RELATIVE = WINDOWS
    ? ['Artifacts', 'Native-Wvo-Object-Candidate', 'Wvo-Object.exe']
    : ['Artifacts', 'Native-Wvo-Object-Candidate', 'Wvo-Object.elf'];
const INSPECTOR_SHA256 = WINDOWS
    ? '5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03'
    : 'fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840';

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

async function Measureˉproduct(candidate, label) {
    const bytes = await Readˉordinaryˉfile(
        candidate,
        label,
        MAXIMUM_PRODUCT_BYTES
    );
    return {
        bytes: bytes.length,
        sha256: createHash('sha256').update(bytes).digest('hex')
    };
}

async function Requireˉoutputˉpath(candidate, extension, label) {
    const absolute = path.resolve(candidate);
    if (path.extname(absolute).toLowerCase() !== extension) {
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

function Checkpointˉrecord(key, wvb, wvo) {
    return Buffer.from([
        'windvale-native-project-object-checkpoint 2',
        `key ${key}`,
        `wvb-bytes ${wvb.bytes}`,
        `wvb-sha256 ${wvb.sha256}`,
        `wvo-bytes ${wvo.bytes}`,
        `wvo-sha256 ${wvo.sha256}`,
        ''
    ].join(RECORD_ENDING), 'ascii');
}

async function Validateˉcheckpoint(checkpointDirectory, key) {
    await Requireˉcanonicalˉdirectory(checkpointDirectory, 'checkpoint directory');
    const entries = (await readdir(checkpointDirectory)).sort();
    if (entries.length !== 3 || entries[0] !== 'Checkpoint.txt' ||
        entries[1] !== 'Product.wvb' || entries[2] !== 'Product.wvo') {
        Reject(`The project-object checkpoint has unexpected entries: ${checkpointDirectory}`);
    }
    const manifestPath = path.join(checkpointDirectory, 'Checkpoint.txt');
    const wvbPath = path.join(checkpointDirectory, 'Product.wvb');
    const wvoPath = path.join(checkpointDirectory, 'Product.wvo');
    const manifest = await Readˉordinaryˉfile(
        manifestPath,
        'checkpoint manifest',
        1_024
    );
    const wvb = await Measureˉproduct(wvbPath, 'checkpoint WVB');
    const wvo = await Measureˉproduct(wvoPath, 'checkpoint WVO');
    if (!manifest.equals(Checkpointˉrecord(key, wvb, wvo))) {
        Reject(`The project-object checkpoint record differs: ${checkpointDirectory}`);
    }
    return { wvbPath, wvoPath, wvb, wvo };
}

async function Removeˉtemporaryˉcheckpoint(checkpointFamily, temporary) {
    const family = await Requireˉcanonicalˉdirectory(
        checkpointFamily,
        'project-object checkpoint family'
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
}

async function Createˉcheckpoint(
    checkpointFamily,
    checkpointDirectory,
    request,
    buildDriver,
    lowerer,
    inspector
) {
    const key = request.key;
    const projectPath = request.projectPath;
    const temporary = path.join(
        checkpointFamily,
        `.new-${key}-${process.pid}-${Date.now().toString(16)}`
    );
    await mkdir(temporary, { recursive: false });
    try {
        const candidateWvb = path.join(temporary, 'Product.wvb');
        const candidateWvo = path.join(temporary, 'Product.wvo');
        Runˉproducer(buildDriver, [
            '--workspace',
            Toˉresourceˉpath(path.join(REPOSITORY_ROOT, 'Windvale.wvws')),
            '--project',
            Toˉresourceˉpath(projectPath),
            Toˉresourceˉpath(candidateWvb)
        ], 'The project-object WVB build');
        Runˉproducer(
            lowerer,
            [candidateWvb, candidateWvo],
            'The project-object lowering'
        );
        Runˉproducer(
            inspector,
            ['check', candidateWvo],
            'The project-object WVO admission'
        );
        const wvb = await Measureˉproduct(candidateWvb, 'candidate WVB');
        const wvo = await Measureˉproduct(candidateWvo, 'candidate WVO');
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(request);
        await writeFile(
            path.join(temporary, 'Checkpoint.txt'),
            Checkpointˉrecord(key, wvb, wvo),
            { flag: 'wx' }
        );
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

export async function Prepareˉprojectˉobjectˉcache(
    buildDriverInput,
    lowererInput
) {
    const buildDriver = path.resolve(buildDriverInput);
    const lowerer = path.resolve(lowererInput);
    const keyContext = await Prepareˉnativeˉprojectˉcacheˉcontext(
        'database-project-object-v2',
        [buildDriver, lowerer, SCRIPT_PATH]
    );
    const inspector = path.join(REPOSITORY_ROOT, ...INSPECTOR_RELATIVE);
    const inspectorProduct = await Measureˉproduct(inspector, 'WVO inspector');
    if (inspectorProduct.sha256 !== INSPECTOR_SHA256) {
        Reject('The native WVO inspector artifact digest is invalid.');
    }
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
        path.join(checkpointRoot, 'project-object-v2'),
        'project-object checkpoint root'
    );
    const checkpointFamily = await Ensureˉcanonicalˉdirectory(
        path.join(checkpointProductRoot, HOST_FAMILY),
        'project-object checkpoint family'
    );
    return {
        buildDriver,
        checkpointFamily,
        inspector,
        keyContext,
        lowerer
    };
}

export async function Materializeˉprojectˉobjectˉhit(
    context,
    projectInput,
    outputWvbInput,
    outputWvoInput
) {
    const outputWvb = await Requireˉoutputˉpath(
        outputWvbInput,
        '.wvb',
        'materialized WVB'
    );
    const outputWvo = await Requireˉoutputˉpath(
        outputWvoInput,
        '.wvo',
        'materialized WVO'
    );
    const request = await Getˉnativeˉprojectˉcacheˉrequest(
        context.keyContext,
        projectInput
    );
    const checkpointDirectory = path.join(context.checkpointFamily, request.key);
    const information = await lstat(checkpointDirectory).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return { request, status: 'Miss' };
    }
    const checkpoint = await Validateˉcheckpoint(
        checkpointDirectory,
        request.key
    );
    await copyFile(checkpoint.wvbPath, outputWvb);
    await copyFile(checkpoint.wvoPath, outputWvo);
    const materializedWvb = await Measureˉproduct(outputWvb, 'materialized WVB');
    const materializedWvo = await Measureˉproduct(outputWvo, 'materialized WVO');
    if (materializedWvb.bytes !== checkpoint.wvb.bytes ||
        materializedWvb.sha256 !== checkpoint.wvb.sha256 ||
        materializedWvo.bytes !== checkpoint.wvo.bytes ||
        materializedWvo.sha256 !== checkpoint.wvo.sha256) {
        Reject('A materialized project-object product differs from its checkpoint.');
    }
    return {
        key: request.key,
        report: `native project object cache status=Hit key=${request.key}`,
        status: 'Hit'
    };
}

async function Main() {
    if (process.argv.length !== 7) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Project-Object.mjs ' +
            '<project.wvproj> <build-driver> <lowerer> <output.wvb> <output.wvo>',
            64
        );
    }
    const context = await Prepareˉprojectˉobjectˉcache(
        process.argv[3],
        process.argv[4]
    );
    let result = await Materializeˉprojectˉobjectˉhit(
        context,
        process.argv[2],
        process.argv[5],
        process.argv[6]
    );
    let status = 'Hit';
    if (result.status === 'Miss') {
        status = await Createˉcheckpoint(
            context.checkpointFamily,
            path.join(context.checkpointFamily, result.request.key),
            result.request,
            context.buildDriver,
            context.lowerer,
            context.inspector
        );
        result = await Materializeˉprojectˉobjectˉhit(
            context,
            process.argv[2],
            process.argv[5],
            process.argv[6]
        );
        if (result.status !== 'Hit') {
            Reject('The published project-object checkpoint is unavailable.');
        }
    }
    process.stdout.write(
        `native project object cache status=${status} key=${result.key}\n`
    );
}

if (process.argv[1] !== undefined &&
    Isˉsameˉpath(path.resolve(process.argv[1]), SCRIPT_PATH)) {
    try {
        await Main();
    } catch (error) {
        process.stderr.write(`${error.message}\n`);
        process.exit(error.exitCode ?? 1);
    }
}
