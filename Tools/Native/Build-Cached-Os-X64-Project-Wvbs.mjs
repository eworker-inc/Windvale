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
import { TextDecoder } from 'node:util';
import {
    Getˉnativeˉprojectˉcacheˉrequest,
    Prepareˉnativeˉprojectˉcacheˉcontext,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
    REPOSITORY_ROOT
} from './Native-Project-Cache-Key-Core.mjs';

const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const UTF8 = new TextDecoder('utf-8', { fatal: true });
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';

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

function Checkpointˉrecord(key, product) {
    return Buffer.from([
        'windvale-native-project-wvb-checkpoint 1',
        `key ${key}`,
        `wvb-bytes ${product.bytes}`,
        `wvb-sha256 ${product.sha256}`,
        ''
    ].join(RECORD_ENDING), 'ascii');
}

async function Validateˉcheckpoint(checkpointDirectory, key) {
    await Requireˉcanonicalˉdirectory(checkpointDirectory, 'checkpoint directory');
    const entries = (await readdir(checkpointDirectory)).sort();
    if (entries.length !== 2 || entries[0] !== 'Checkpoint.txt' ||
        entries[1] !== 'Product.wvb') {
        Reject(`The project-WVB checkpoint has unexpected entries: ${checkpointDirectory}`);
    }
    const manifestPath = path.join(checkpointDirectory, 'Checkpoint.txt');
    const productPath = path.join(checkpointDirectory, 'Product.wvb');
    const manifest = await Readˉordinaryˉfile(
        manifestPath,
        'checkpoint manifest',
        1_024
    );
    const product = await Measureˉproduct(productPath, 'checkpoint WVB');
    if (!manifest.equals(Checkpointˉrecord(key, product))) {
        Reject(`The project-WVB checkpoint record differs: ${checkpointDirectory}`);
    }
    return { productPath, product };
}

async function Removeˉtemporaryˉcheckpoint(checkpointFamily, temporary) {
    const family = await Requireˉcanonicalˉdirectory(
        checkpointFamily,
        'project-WVB checkpoint family'
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

async function Createˉcheckpoint(
    checkpointFamily,
    checkpointDirectory,
    request,
    buildDriver
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
        const build = spawnSync(buildDriver, [
            '--workspace',
            Toˉresourceˉpath(path.join(REPOSITORY_ROOT, 'Windvale.wvws')),
            '--project',
            Toˉresourceˉpath(projectPath),
            Toˉresourceˉpath(candidateWvb)
        ], {
            encoding: 'utf8',
            maxBuffer: 4_194_304,
            windowsHide: true
        });
        if (build.error !== undefined || build.status !== 0) {
            const detail = (build.stderr ?? '').trim();
            Reject(
                `The project-WVB checkpoint build failed for ${projectPath}` +
                (detail === '' ? '.' : `: ${detail}`)
            );
        }
        const product = await Measureˉproduct(candidateWvb, 'candidate WVB');
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(request);
        await writeFile(
            path.join(temporary, 'Checkpoint.txt'),
            Checkpointˉrecord(key, product),
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

async function Materializeˉproject(
    projectPath,
    outputPath,
    buildDriver,
    keyContext,
    checkpointFamily
) {
    const request = await Getˉnativeˉprojectˉcacheˉrequest(
        keyContext,
        projectPath
    );
    const key = request.key;
    const checkpointDirectory = path.join(checkpointFamily, key);
    let status = 'Hit';
    const information = await lstat(checkpointDirectory).catch(error => {
        if (error.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        status = await Createˉcheckpoint(
            checkpointFamily,
            checkpointDirectory,
            request,
            buildDriver
        );
    }
    const checkpoint = await Validateˉcheckpoint(checkpointDirectory, key);
    await copyFile(checkpoint.productPath, outputPath);
    const materialized = await Measureˉproduct(outputPath, 'materialized WVB');
    if (materialized.bytes !== checkpoint.product.bytes ||
        materialized.sha256 !== checkpoint.product.sha256) {
        Reject(`The materialized project WVB differs: ${outputPath}`);
    }
    return status;
}

function Parseˉtargetˉplan(bytes, selectedTarget) {
    let text;
    try {
        text = UTF8.decode(bytes);
    } catch {
        Reject('The OS x64 code-emission target manifest is not valid UTF-8.');
    }
    const lines = text.split(/\r?\n/u);
    if (lines.at(-1) === '') {
        lines.pop();
    }
    if (lines.length !== 57 ||
        lines[0] !== 'windvale-os-x64-code-emission-development-targets 2') {
        Reject('The OS x64 code-emission target manifest differs.');
    }
    const targets = new Set();
    const artifacts = new Set();
    const selected = [];
    for (let index = 1; index < lines.length; index += 1) {
        const fields = lines[index].split('|');
        const target = fields[0];
        const project = fields[1];
        const artifact = fields[2];
        if ((fields.length !== 16 && fields.length !== 17) ||
            !/^[a-z0-9][a-z0-9-]*$/.test(target) ||
            !/^Projects\/Tests\/Windvale-Native-Test-Os-X64-.+-Emission\.wvproj$/.test(project) ||
            !/^[A-Za-z][A-Za-z0-9]*$/.test(artifact) ||
            fields[3] !== String(49 + index) ||
            [4, 6, 8, 10, 12].some(field => !/^[1-9][0-9]*$/.test(fields[field])) ||
            [5, 7, 9, 11, 13].some(field => !/^[0-9a-f]{64}$/.test(fields[field])) ||
            fields.slice(14).some(field => field === '') ||
            targets.has(target) || artifacts.has(artifact)) {
            Reject(`Invalid OS x64 code-emission target manifest entry: ${target}`);
        }
        targets.add(target);
        artifacts.add(artifact);
        if (selectedTarget === 'all' || selectedTarget === target) {
            selected.push({ target, project, artifact });
        }
    }
    if (selected.length !== (selectedTarget === 'all' ? 56 : 1)) {
        Reject(`Unknown OS x64 code-emission development target: ${selectedTarget}`, 64);
    }
    return selected;
}

async function Main() {
    if (process.argv.length !== 6 ||
        (process.argv[5] !== 'all' &&
            !/^[a-z0-9][a-z0-9-]*$/.test(process.argv[5]))) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Os-X64-Project-Wvbs.mjs ' +
            '<target-manifest> <output-directory> <build-driver> <target|all>',
            64
        );
    }
    const outputDirectory = await Requireˉcanonicalˉdirectory(
        process.argv[3],
        'batch output directory'
    );
    const buildDriver = path.resolve(process.argv[4]);
    await Readˉordinaryˉfile(buildDriver, 'build driver', MAXIMUM_PRODUCT_BYTES);
    const targetBytes = await Readˉordinaryˉfile(
        process.argv[2],
        'OS x64 target manifest',
        1_048_576
    );
    const selected = Parseˉtargetˉplan(targetBytes, process.argv[5]);
    const inventory = path.join(
        REPOSITORY_ROOT,
        'Artifacts',
        'Native-Front-Door',
        'SHA256SUMS'
    );
    await Readˉordinaryˉfile(inventory, 'native-front-door inventory', 1_048_576);
    const keyContext = await Prepareˉnativeˉprojectˉcacheˉcontext(
        'project-wvb-v2',
        [inventory, buildDriver]
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
        path.join(checkpointRoot, 'project-wvb-v2'),
        'project-WVB checkpoint root'
    );
    const checkpointFamily = await Ensureˉcanonicalˉdirectory(
        path.join(checkpointProductRoot, HOST_FAMILY),
        'project-WVB checkpoint family'
    );

    let hits = 0;
    let created = 0;
    for (const request of selected) {
        const status = await Materializeˉproject(
            path.join(REPOSITORY_ROOT, ...request.project.split('/')),
            path.join(outputDirectory, `${request.artifact}.candidate.wvb`),
            buildDriver,
            keyContext,
            checkpointFamily
        );
        if (status === 'Hit') {
            hits += 1;
        } else {
            created += 1;
        }
    }
    process.stdout.write(
        `native project wvb batch cache status=Passed projects=${selected.length} ` +
        `hits=${hits} created=${created}\n`
    );
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(error.exitCode ?? 1);
}
