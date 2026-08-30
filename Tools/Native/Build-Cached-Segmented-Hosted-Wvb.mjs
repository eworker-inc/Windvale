import { createHash, randomBytes } from 'node:crypto';
import { spawn, spawnSync } from 'node:child_process';
import { constants as FS_CONSTANTS } from 'node:fs';
import {
    chmod,
    copyFile,
    lstat,
    mkdir,
    mkdtemp,
    readdir,
    realpath,
    rename,
    rm,
    unlink,
    writeFile,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Addˉhostedˉkeyˉfield,
    HOSTED_REPOSITORY_ROOT,
    Isˉsameˉhostedˉpath,
    MAXIMUM_HOSTED_INPUT_BYTES,
    Prepareˉhostedˉapplicationˉcontext,
    Readˉboundedˉhostedˉfile,
} from './Native-Hosted-Application-Cache-Core.mjs';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const SCRIPT_DIRECTORY = path.dirname(SCRIPT_PATH);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const TARGET = WINDOWS ? 'windows' : 'linux';
const PRODUCT_LEAF = WINDOWS ? 'Product.exe' : 'Product.elf';
const OUTPUT_EXTENSION = WINDOWS ? '.exe' : '.elf';
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';
const CACHE_NAMESPACE = 'segmented-hosted-wvb-v1';
const MAXIMUM_PRODUCT_BYTES = 67_108_864;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS = 4_096;
const VERIFICATION_DEADLINE_MILLISECONDS = 120_000;
const COLD_DEADLINE_MILLISECONDS = 15 * 60_000;
const HEARTBEAT_MILLISECONDS = 30_000;
const TERMINATION_GRACE_MILLISECONDS = 500;
const POST_TERMINATION_CLOSE_GRACE_MILLISECONDS = 1_000;
const VERIFICATION_TEMPORARY_PREFIX = 'windvale-segmented-hosted-verification-';
const LOADED_PRODUCER_RELATIVES = [
    'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs',
    'Tools/Native/Native-Hosted-Application-Cache-Core.mjs',
];
const LOADED_PRODUCER_SNAPSHOTS = await Promise.all(
    LOADED_PRODUCER_RELATIVES.map(async relative => ({
        bytes: await Readˉproducer(relative),
        relative,
    })),
);

function Reject(message, exitCode = 1) {
    const error = new Error(message);
    error.exitCode = exitCode;
    throw error;
}

function Delay(milliseconds) {
    return new Promise(resolve => setTimeout(resolve, milliseconds));
}

function Sha256(bytes) {
    return createHash('sha256').update(bytes).digest('hex');
}

function Productˉmeasurement(bytes) {
    return { bytes: bytes.length, sha256: Sha256(bytes) };
}

async function Requireˉcanonicalˉdirectory(candidate, label) {
    const absolute = path.resolve(candidate);
    const information = await lstat(absolute).catch(() => null);
    if (information === null || !information.isDirectory() ||
        information.isSymbolicLink()) {
        Reject(`The ${label} is not an ordinary directory: ${absolute}`);
    }
    const canonical = await realpath(absolute);
    if (!Isˉsameˉhostedˉpath(canonical, absolute)) {
        Reject(`The ${label} must use its canonical non-link path: ${absolute}`);
    }
    return absolute;
}

async function Ensureˉcanonicalˉdirectory(candidate, label) {
    await mkdir(candidate, { recursive: true });
    return Requireˉcanonicalˉdirectory(candidate, label);
}

async function Requireˉinputˉsnapshot(candidate) {
    const absolute = path.resolve(candidate);
    if (path.extname(absolute).toLowerCase() !== '.wvb') {
        Reject('The segmented hosted cache input must use the .wvb extension.', 64);
    }
    const payload = await Readˉboundedˉhostedˉfile(
        absolute,
        'segmented hosted cache input WVB',
        MAXIMUM_HOSTED_INPUT_BYTES,
    );
    return {
        path: absolute,
        payload,
        ...Productˉmeasurement(payload),
    };
}

async function Requireˉinputˉunchanged(snapshot) {
    const current = await Requireˉinputˉsnapshot(snapshot.path);
    if (current.bytes !== snapshot.bytes ||
        !current.payload.equals(snapshot.payload)) {
        Reject('The segmented hosted cache input changed during production.');
    }
}

async function Removeˉverificationˉtemporary(
    temporaryRoot,
    temporaryDirectory,
) {
    const root = await Requireˉcanonicalˉdirectory(
        temporaryRoot,
        'segmented hosted verification temporary root',
    );
    const candidate = path.resolve(temporaryDirectory);
    if (!Isˉsameˉhostedˉpath(path.dirname(candidate), root) ||
        !/^windvale-segmented-hosted-verification-[A-Za-z0-9]{6}$/u.test(
            path.basename(candidate),
        )) {
        Reject(`Refusing to remove an unowned verification temporary: ${candidate}`);
    }
    const information = await lstat(candidate).catch(error => {
        if (error?.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return;
    }
    if (!information.isDirectory() || information.isSymbolicLink()) {
        Reject(`The verification temporary is not an ordinary directory: ${candidate}`);
    }
    const canonical = await realpath(candidate);
    if (!Isˉsameˉhostedˉpath(canonical, candidate) ||
        !Isˉsameˉhostedˉpath(path.dirname(canonical), root)) {
        Reject(`The verification temporary escaped its root: ${candidate}`);
    }
    await rm(candidate, { recursive: true, force: false, maxRetries: 2 });
}

async function Requireˉoutputˉpath(candidate) {
    const absolute = path.resolve(candidate);
    if (path.extname(absolute).toLowerCase() !== OUTPUT_EXTENSION) {
        Reject(
            `The current-host segmented output must use ${OUTPUT_EXTENSION}.`,
            64,
        );
    }
    await Requireˉcanonicalˉdirectory(
        path.dirname(absolute),
        'segmented hosted output parent',
    );
    const information = await lstat(absolute).catch(error => {
        if (error?.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information !== null && (!information.isFile() ||
        information.isSymbolicLink())) {
        Reject(`The segmented hosted output is not an ordinary file: ${absolute}`);
    }
    if (information !== null) {
        const canonical = await realpath(absolute);
        if (!Isˉsameˉhostedˉpath(canonical, absolute)) {
            Reject(`The segmented hosted output must use its canonical path: ${absolute}`);
        }
    }
    return absolute;
}

async function Measureˉproduct(candidate, label) {
    const bytes = await Readˉboundedˉhostedˉfile(
        candidate,
        label,
        MAXIMUM_PRODUCT_BYTES,
        false,
        !WINDOWS,
    );
    return Productˉmeasurement(bytes);
}

function Checkpointˉrecord(key, profile, input, product) {
    return Buffer.from([
        'windvale-native-segmented-hosted-wvb-checkpoint 1',
        `key ${key}`,
        `host ${HOST_FAMILY}`,
        `target ${TARGET}`,
        `profile ${profile}`,
        `input-bytes ${input.bytes}`,
        `input-sha256 ${input.sha256}`,
        `product-bytes ${product.bytes}`,
        `product-sha256 ${product.sha256}`,
        'product-mode executable',
        '',
    ].join(RECORD_ENDING), 'ascii');
}

export async function Validateˉsegmentedˉhostedˉcheckpoint(
    directory,
    key,
    profile,
    input,
) {
    await Requireˉcanonicalˉdirectory(
        directory,
        'segmented hosted checkpoint',
    );
    const entries = (await readdir(directory)).sort();
    const expectedEntries = ['Checkpoint.txt', PRODUCT_LEAF].sort();
    if (entries.length !== expectedEntries.length ||
        entries.some((entry, index) => entry !== expectedEntries[index])) {
        Reject(`The segmented hosted checkpoint has unexpected entries: ${directory}`);
    }
    const manifestPath = path.join(directory, 'Checkpoint.txt');
    const productPath = path.join(directory, PRODUCT_LEAF);
    const manifest = await Readˉboundedˉhostedˉfile(
        manifestPath,
        'segmented hosted checkpoint manifest',
        2_048,
    );
    const product = await Measureˉproduct(
        productPath,
        'segmented hosted checkpoint product',
    );
    if (!manifest.equals(Checkpointˉrecord(key, profile, input, product))) {
        Reject(`The segmented hosted checkpoint manifest differs: ${directory}`);
    }
    return { product, productPath };
}

async function Removeˉtemporaryˉcheckpoint(checkpointFamily, temporary) {
    const family = await Requireˉcanonicalˉdirectory(
        checkpointFamily,
        'segmented hosted checkpoint family',
    );
    const candidate = path.resolve(temporary);
    if (!Isˉsameˉhostedˉpath(path.dirname(candidate), family) ||
        !/^\.new-[0-9a-f]{64}-[1-9][0-9]*-[0-9a-f]{32}$/u.test(
            path.basename(candidate),
        )) {
        Reject(`Refusing to remove an unowned checkpoint candidate: ${candidate}`);
    }
    const information = await lstat(candidate).catch(error => {
        if (error?.code === 'ENOENT') {
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
    if (!Isˉsameˉhostedˉpath(canonical, candidate) ||
        !Isˉsameˉhostedˉpath(path.dirname(canonical), family)) {
        Reject(`The checkpoint candidate escaped its family: ${candidate}`);
    }
    await rm(candidate, { recursive: true, force: false, maxRetries: 2 });
}

async function Removeˉmaterializationˉtemporary(outputParent, temporary) {
    const parent = await Requireˉcanonicalˉdirectory(
        outputParent,
        'segmented hosted output parent',
    );
    const candidate = path.resolve(temporary);
    if (!Isˉsameˉhostedˉpath(path.dirname(candidate), parent) ||
        !/^\.new-materialization-[1-9][0-9]*-[0-9a-f]{32}$/u.test(
            path.basename(candidate),
        )) {
        Reject(`Refusing to remove an unowned materialization: ${candidate}`);
    }
    const information = await lstat(candidate).catch(error => {
        if (error?.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    if (information === null) {
        return;
    }
    if (!information.isFile() || information.isSymbolicLink()) {
        Reject(`The materialization candidate is not an ordinary file: ${candidate}`);
    }
    const canonical = await realpath(candidate);
    if (!Isˉsameˉhostedˉpath(canonical, candidate) ||
        !Isˉsameˉhostedˉpath(path.dirname(canonical), parent)) {
        Reject(`The materialization candidate escaped its parent: ${candidate}`);
    }
    await unlink(candidate);
}

export async function Materializeˉsegmentedˉhostedˉcheckpoint(
    checkpoint,
    outputPath,
) {
    const outputParent = path.dirname(outputPath);
    const temporary = path.join(
        outputParent,
        `.new-materialization-${process.pid}-${randomBytes(16).toString('hex')}`,
    );
    try {
        await copyFile(
            checkpoint.productPath,
            temporary,
            FS_CONSTANTS.COPYFILE_EXCL,
        );
        if (!WINDOWS) {
            await chmod(temporary, 0o755);
        }
        const copied = await Measureˉproduct(
            temporary,
            'segmented hosted materialization candidate',
        );
        if (copied.bytes !== checkpoint.product.bytes ||
            copied.sha256 !== checkpoint.product.sha256) {
            Reject('The segmented hosted materialization candidate differs.');
        }
        await rename(temporary, outputPath);
        const materialized = await Measureˉproduct(
            outputPath,
            'materialized segmented hosted product',
        );
        if (materialized.bytes !== checkpoint.product.bytes ||
            materialized.sha256 !== checkpoint.product.sha256) {
            Reject('The materialized segmented hosted product differs.');
        }
    } finally {
        await Removeˉmaterializationˉtemporary(outputParent, temporary);
    }
}

function Diagnosticˉtext(value) {
    const text = Buffer.isBuffer(value)
        ? value.toString('utf8')
        : String(value ?? '');
    if (text.length <= MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS) {
        return text;
    }
    return text.slice(0, MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS) +
        `...[truncated characters=${text.length}]`;
}

function Windowsˉcommand(wrapper, arguments_) {
    const values = [wrapper, ...arguments_];
    for (const value of values) {
        if (/["%\r\n!&|<>^]/u.test(value)) {
            Reject(`A Windows producer argument contains a command metacharacter: ${value}`);
        }
    }
    return `call ${values.map(value => `"${value}"`).join(' ')}`;
}

async function Terminateˉprocessˉtree(child) {
    if (WINDOWS) {
        if (child.exitCode !== null || child.signalCode !== null) {
            return;
        }
        const result = spawnSync(
            'taskkill.exe',
            ['/pid', String(child.pid), '/t', '/f'],
            {
                encoding: 'utf8',
                maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
                timeout: 10_000,
                windowsHide: true,
            },
        );
        if (result.error !== undefined || result.status !== 0) {
            if (child.exitCode === null && child.signalCode === null) {
                try {
                    child.kill('SIGKILL');
                } catch (error) {
                    Reject(
                        `Windows process-tree termination and parent fallback failed: ` +
                        `${error.message}`,
                    );
                }
            }
            Reject(
                'Windows process-tree termination failed ' +
                `status=${result.status ?? 'null'} ` +
                `error=${result.error?.message ?? 'none'} ` +
                `stderr=${JSON.stringify(Diagnosticˉtext(result.stderr))}`,
            );
        }
        return;
    }
    try {
        process.kill(-child.pid, 'SIGTERM');
    } catch (error) {
        if (error?.code !== 'ESRCH') {
            throw error;
        }
        return;
    }
    await Delay(TERMINATION_GRACE_MILLISECONDS);
    try {
        process.kill(-child.pid, 'SIGKILL');
    } catch (error) {
        if (error?.code !== 'ESRCH') {
            throw error;
        }
    }
}

export async function Runˉboundedˉsegmentedˉhostedˉproducer(
    wrapper,
    arguments_,
    label,
    deadline,
    options = {},
) {
    const terminateProcessTree = options.terminateProcessTree ??
        Terminateˉprocessˉtree;
    const closeGraceMilliseconds = options.closeGraceMilliseconds ??
        POST_TERMINATION_CLOSE_GRACE_MILLISECONDS;
    if (typeof terminateProcessTree !== 'function' ||
        !Number.isSafeInteger(closeGraceMilliseconds) ||
        closeGraceMilliseconds < 1 || closeGraceMilliseconds > 10_000) {
        Reject('The segmented hosted producer termination policy is invalid.');
    }
    const remaining = deadline - Date.now();
    if (remaining <= 0) {
        Reject(`The segmented hosted producer deadline expired before ${label}.`);
    }
    const started = Date.now();
    process.stdout.write(
        `segmented hosted WVB cache step=${label} status=Started\n`,
    );
    const application = WINDOWS
        ? Windowsˉcommand(wrapper, arguments_)
        : wrapper;
    const childArguments = WINDOWS ? [] : arguments_;
    const child = spawn(application, childArguments, {
        detached: !WINDOWS,
        shell: WINDOWS ? (process.env.ComSpec ?? 'cmd.exe') : false,
        stdio: ['ignore', 'pipe', 'pipe'],
        windowsHide: true,
    });
    const closeOutcome = new Promise(resolve => {
        child.once('error', error => resolve({ error }));
        child.once('close', (status, signal) => resolve({ status, signal }));
    });
    let Resolveˉforcedˉoutcome;
    const forcedOutcome = new Promise(resolve => {
        Resolveˉforcedˉoutcome = resolve;
    });
    const stdoutChunks = [];
    const stderrChunks = [];
    let stdoutBytes = 0;
    let stderrBytes = 0;
    let forcedFailure = null;
    let termination = null;
    const Failˉandˉterminate = message => {
        if (forcedFailure !== null) {
            return;
        }
        forcedFailure = message;
        termination = Promise.resolve().then(() =>
            terminateProcessTree(child)).catch(error => {
                forcedFailure +=
                    `; process-tree termination failed: ${error.message}`;
            });
        Resolveˉforcedˉoutcome((async () => {
            await termination;
            const settled = await Promise.race([
                closeOutcome.then(outcome => ({ outcome, settled: true })),
                Delay(closeGraceMilliseconds).then(() => ({ settled: false })),
            ]);
            if (settled.settled) {
                return settled.outcome;
            }
            child.stdout.destroy();
            child.stderr.destroy();
            child.unref();
            return {
                status: null,
                signal: 'termination-unsettled',
                synthetic: true,
            };
        })());
    };
    child.stdout.on('data', chunk => {
        stdoutBytes += chunk.length;
        if (stdoutBytes > MAXIMUM_DIAGNOSTIC_BYTES) {
            Failˉandˉterminate(`${label} stdout exceeded 64 KiB`);
            return;
        }
        stdoutChunks.push(chunk);
    });
    child.stderr.on('data', chunk => {
        stderrBytes += chunk.length;
        if (stderrBytes > MAXIMUM_DIAGNOSTIC_BYTES) {
            Failˉandˉterminate(`${label} stderr exceeded 64 KiB`);
            return;
        }
        stderrChunks.push(chunk);
    });
    const timeout = setTimeout(() => {
        Failˉandˉterminate(`${label} exceeded its deadline`);
    }, remaining);
    const heartbeat = setInterval(() => {
        const elapsedSeconds = Math.floor((Date.now() - started) / 1_000);
        process.stdout.write(
            `segmented hosted WVB cache step=${label} status=Active ` +
            `elapsed-seconds=${elapsedSeconds}\n`,
        );
    }, HEARTBEAT_MILLISECONDS);
    const outcome = await Promise.race([closeOutcome, forcedOutcome]);
    clearTimeout(timeout);
    clearInterval(heartbeat);
    if (termination !== null) {
        await termination;
    }
    const stdout = Buffer.concat(stdoutChunks);
    const stderr = Buffer.concat(stderrChunks);
    if (outcome.error !== undefined) {
        Reject(`${label} could not start: ${outcome.error.message}`);
    }
    if (forcedFailure !== null || outcome.status !== 0) {
        const status = outcome.status === null ? 'null' : String(outcome.status);
        const signal = outcome.signal ?? 'none';
        Reject(
            `${forcedFailure ?? `${label} failed`} status=${status} signal=${signal} ` +
            `stdout=${JSON.stringify(Diagnosticˉtext(stdout))} ` +
            `stderr=${JSON.stringify(Diagnosticˉtext(stderr))}`,
        );
    }
    process.stdout.write(
        `segmented hosted WVB cache step=${label} status=Complete ` +
        `elapsed-seconds=${Math.floor((Date.now() - started) / 1_000)}\n`,
    );
    return { stderr, stdout };
}

function Nativeˉwrapper(leaf) {
    return path.join(SCRIPT_DIRECTORY, `${leaf}.${WINDOWS ? 'cmd' : 'sh'}`);
}

function Normalizeˉlines(bytes) {
    return bytes.toString('utf8').replaceAll('\r\n', '\n');
}

async function Completeˉverifyˉinput(snapshot) {
    const temporaryRoot = await realpath(os.tmpdir());
    const allocated = await mkdtemp(
        path.join(temporaryRoot, VERIFICATION_TEMPORARY_PREFIX),
    );
    const temporaryDirectory = await realpath(allocated);
    try {
        const verificationInput = path.join(temporaryDirectory, 'Input.wvb');
        await writeFile(verificationInput, snapshot.payload, { flag: 'wx' });
        const result = await Runˉboundedˉsegmentedˉhostedˉproducer(
            Nativeˉwrapper('Verify-Wvb'),
            [verificationInput],
            'complete-verification',
            Date.now() + VERIFICATION_DEADLINE_MILLISECONDS,
        );
        if (result.stderr.length !== 0 ||
            Normalizeˉlines(result.stdout) !==
                'wvb status=Valid profile=compiler-aligned\n') {
            Reject('The complete WVB verifier report differs.');
        }
        const verified = await Readˉboundedˉhostedˉfile(
            verificationInput,
            'complete-verification input copy',
            MAXIMUM_HOSTED_INPUT_BYTES,
        );
        if (!verified.equals(snapshot.payload)) {
            Reject('The complete-verification input copy changed.');
        }
    } finally {
        await Removeˉverificationˉtemporary(
            temporaryRoot,
            temporaryDirectory,
        );
    }
    await Requireˉinputˉunchanged(snapshot);
}

async function Buildˉcandidate(temporary, profile, input, deadline) {
    const stagedInput = path.join(temporary, 'Input.wvb');
    await writeFile(stagedInput, input.payload, { flag: 'wx' });
    const productPath = path.join(temporary, PRODUCT_LEAF);
    await Runˉboundedˉsegmentedˉhostedˉproducer(
        Nativeˉwrapper('Package-Segmented-Compiler-Wvb'),
        [profile, stagedInput, productPath],
        'cold-build',
        deadline,
    );
    const stagedBytes = await Readˉboundedˉhostedˉfile(
        stagedInput,
        'segmented hosted cold-build input copy',
        MAXIMUM_HOSTED_INPUT_BYTES,
    );
    if (!stagedBytes.equals(input.payload)) {
        Reject('The segmented hosted cold-build input copy changed.');
    }
    await unlink(stagedInput);
    if (!WINDOWS) {
        await chmod(productPath, 0o755);
    }
}

export async function Createˉsegmentedˉhostedˉcheckpoint(
    checkpointFamily,
    checkpointDirectory,
    key,
    profile,
    input,
    producer,
    admit,
) {
    if (admit !== undefined && typeof admit !== 'function') {
        Reject('The segmented hosted checkpoint admission is invalid.');
    }
    const temporary = path.join(
        checkpointFamily,
        `.new-${key}-${process.pid}-${randomBytes(16).toString('hex')}`,
    );
    await mkdir(temporary, { recursive: false });
    try {
        await producer(temporary);
        const productPath = path.join(temporary, PRODUCT_LEAF);
        if (!WINDOWS) {
            await chmod(productPath, 0o755);
        }
        const product = await Measureˉproduct(
            productPath,
            'candidate segmented hosted product',
        );
        await Requireˉinputˉunchanged(input);
        await writeFile(
            path.join(temporary, 'Checkpoint.txt'),
            Checkpointˉrecord(key, profile, input, product),
            { flag: 'wx' },
        );
        await Validateˉsegmentedˉhostedˉcheckpoint(
            temporary,
            key,
            profile,
            input,
        );
        if (admit !== undefined) {
            await admit();
        }
        try {
            await rename(temporary, checkpointDirectory);
            return 'Created';
        } catch (error) {
            if (!['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES'].includes(error?.code)) {
                throw error;
            }
            await Validateˉsegmentedˉhostedˉcheckpoint(
                checkpointDirectory,
                key,
                profile,
                input,
            );
            return 'Hit';
        }
    } finally {
        await Removeˉtemporaryˉcheckpoint(checkpointFamily, temporary);
    }
}

async function Readˉproducer(relative) {
    return Readˉboundedˉhostedˉfile(
        path.join(HOSTED_REPOSITORY_ROOT, ...relative.split('/')),
        `segmented hosted producer ${relative}`,
    );
}

export async function Requireˉloadedˉsegmentedˉhostedˉproducersˉunchanged(
    snapshots = LOADED_PRODUCER_SNAPSHOTS,
    reader = Readˉproducer,
) {
    if (!Array.isArray(snapshots) || typeof reader !== 'function' ||
        snapshots.some(snapshot =>
            snapshot === null || typeof snapshot !== 'object' ||
            typeof snapshot.relative !== 'string' ||
            !Buffer.isBuffer(snapshot.bytes))) {
        Reject('The loaded segmented hosted producer snapshots are invalid.');
    }
    for (const snapshot of snapshots) {
        const current = await reader(snapshot.relative);
        if (!Buffer.isBuffer(current) || !current.equals(snapshot.bytes)) {
            Reject(
                `The loaded segmented hosted producer changed: ${snapshot.relative}`,
            );
        }
    }
}

async function Getˉcacheˉkey(profile, input, hostedContext) {
    const hash = createHash('sha256');
    Addˉhostedˉkeyˉfield(
        hash,
        'format',
        Buffer.from('windvale-native-segmented-hosted-wvb-cache-key 1\n', 'ascii'),
    );
    for (const [label, value] of [
        ['namespace', CACHE_NAMESPACE],
        ['host', HOST_FAMILY],
        ['target', TARGET],
        ['profile', profile],
    ]) {
        Addˉhostedˉkeyˉfield(hash, label, Buffer.from(value, 'ascii'));
    }
    Addˉhostedˉkeyˉfield(hash, 'input-wvb', input.payload);
    const currentExtension = WINDOWS ? 'cmd' : 'sh';
    const currentArtifactExtension = WINDOWS ? 'exe' : 'elf';
    const producerPaths = [
        'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.cmd',
        'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.sh',
        `Tools/Native/Verify-Wvb.${currentExtension}`,
        `Tools/Native/Package-Segmented-Compiler-Wvb.${currentExtension}`,
        `Tools/Native/Stage-Compiler-Wvb.${currentExtension}`,
        `Tools/Native/Link-Staged-Compiler-Wvo.${currentExtension}`,
        `Tools/Native/Transport-Compiler-Image.${currentExtension}`,
        `Artifacts/Native-Front-Door/${HOST_FAMILY}/wvverify.${currentArtifactExtension}`,
        `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${HOST_FAMILY}-wvstage.${currentArtifactExtension}`,
        `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${HOST_FAMILY}-wvlinkstage.${currentArtifactExtension}`,
        `Artifacts/Native-Segmented-Compiler-Toolset-Candidate/${HOST_FAMILY}-wvimagetransport.${currentArtifactExtension}`,
    ];
    for (const snapshot of LOADED_PRODUCER_SNAPSHOTS) {
        Addˉhostedˉkeyˉfield(
            hash,
            `producer:${snapshot.relative}`,
            snapshot.bytes,
        );
    }
    for (const relative of producerPaths) {
        Addˉhostedˉkeyˉfield(
            hash,
            `producer:${relative}`,
            await Readˉproducer(relative),
        );
    }
    for (const field of hostedContext.producerFields) {
        Addˉhostedˉkeyˉfield(hash, field.label, field.bytes);
    }
    return hash.digest('hex');
}

async function Getˉcurrentˉcacheˉkey(profile, input) {
    const packager = Nativeˉwrapper('Package-Hosted-Wvb');
    const hostedContext = await Prepareˉhostedˉapplicationˉcontext(
        TARGET,
        packager,
    );
    try {
        return await Getˉcacheˉkey(profile, input, hostedContext);
    } finally {
        hostedContext.producerFields.length = 0;
    }
}

async function Requireˉproducersˉunchanged(profile, input, expectedKey) {
    const currentKey = await Getˉcurrentˉcacheˉkey(profile, input);
    await Requireˉloadedˉsegmentedˉhostedˉproducersˉunchanged();
    if (currentKey !== expectedKey) {
        Reject('A segmented hosted cache producer changed during production.');
    }
}

async function Getˉcheckpointˉfamily() {
    if (WINDOWS && process.env.WINDVALE_NATIVE_CACHE_ROOT === undefined &&
        process.env.LOCALAPPDATA === undefined) {
        Reject('The native segmented hosted cache root is unavailable.');
    }
    const configuredRoot = process.env.WINDVALE_NATIVE_CACHE_ROOT ?? (
        WINDOWS
            ? path.join(process.env.LOCALAPPDATA, 'Windvale', 'Native-Tool-Cache')
            : path.join(
                process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache'),
                'windvale',
                'native-tool-cache',
            )
    );
    const root = await Ensureˉcanonicalˉdirectory(
        path.resolve(configuredRoot),
        'segmented hosted cache root',
    );
    const productRoot = await Ensureˉcanonicalˉdirectory(
        path.join(root, CACHE_NAMESPACE),
        'segmented hosted cache product root',
    );
    return Ensureˉcanonicalˉdirectory(
        path.join(productRoot, HOST_FAMILY),
        'segmented hosted checkpoint family',
    );
}

async function Main() {
    if (process.argv.length !== 5 || !/^[1-7]$/u.test(process.argv[2])) {
        Reject(
            'Usage: node Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs ' +
            `<profile-1-through-7> <input.wvb> <output${OUTPUT_EXTENSION}>`,
            64,
        );
    }
    const profile = process.argv[2];
    const input = await Requireˉinputˉsnapshot(process.argv[3]);
    const outputPath = await Requireˉoutputˉpath(process.argv[4]);
    await Completeˉverifyˉinput(input);
    const key = await Getˉcurrentˉcacheˉkey(profile, input);
    await Requireˉinputˉunchanged(input);
    const checkpointFamily = await Getˉcheckpointˉfamily();
    const checkpointDirectory = path.join(checkpointFamily, key);
    let checkpointInformation = await lstat(checkpointDirectory).catch(error => {
        if (error?.code === 'ENOENT') {
            return null;
        }
        throw error;
    });
    let status = 'Hit';
    if (checkpointInformation === null) {
        status = await Createˉsegmentedˉhostedˉcheckpoint(
            checkpointFamily,
            checkpointDirectory,
            key,
            profile,
            input,
            temporary => Buildˉcandidate(
                temporary,
                profile,
                input,
                Date.now() + COLD_DEADLINE_MILLISECONDS,
            ),
            () => Requireˉproducersˉunchanged(profile, input, key),
        );
        checkpointInformation = await lstat(checkpointDirectory).catch(() => null);
        if (checkpointInformation === null) {
            Reject('The published segmented hosted checkpoint is unavailable.');
        }
    }
    const checkpoint = await Validateˉsegmentedˉhostedˉcheckpoint(
        checkpointDirectory,
        key,
        profile,
        input,
    );
    await Materializeˉsegmentedˉhostedˉcheckpoint(
        checkpoint,
        outputPath,
    );
    process.stdout.write(
        `segmented hosted WVB cache status=${status} key=${key} ` +
        `host=${HOST_FAMILY} target=${TARGET} profile=${profile}\n`,
    );
}

if (process.argv[1] !== undefined &&
    Isˉsameˉhostedˉpath(path.resolve(process.argv[1]), SCRIPT_PATH)) {
    try {
        await Main();
    } catch (error) {
        process.stderr.write(`${error.message}\n`);
        process.exit(error.exitCode ?? 1);
    }
}
