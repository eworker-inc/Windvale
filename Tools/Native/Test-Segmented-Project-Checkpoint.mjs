import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    appendFile,
    mkdir,
    mkdtemp,
    readFile,
    readdir,
    realpath,
    rm,
    writeFile
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const BUILDER = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    'Build-Cached-Segmented-Project.mjs'
);
const PROJECT = path.join(
    REPOSITORY_ROOT,
    'Projects',
    'Tests',
    'Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj'
);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const BUILD_DRIVER = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Compiler-Reconstruction-Candidate',
    HOST_FAMILY,
    WINDOWS ? 'wvbuild.exe' : 'wvbuild.elf'
);
const FAILED_BUILD_DRIVER = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Segmented-Compiler-Toolset-Candidate',
    WINDOWS ? 'windows-x64-wvstage.exe' : 'linux-x64-wvstage.elf'
);

function Reject(message) {
    throw new Error(message);
}

function Digest(bytes) {
    return createHash('sha256').update(bytes).digest('hex');
}

async function Runˉbuilder(
    cacheRoot,
    outputWvb,
    outputPrefix,
    outputManifest,
    buildDriver = BUILD_DRIVER
) {
    return new Promise((resolve, reject) => {
        const child = spawn(process.execPath, [
            BUILDER,
            PROJECT,
            buildDriver,
            outputWvb,
            outputPrefix,
            outputManifest
        ], {
            cwd: REPOSITORY_ROOT,
            env: {
                ...process.env,
                WINDVALE_NATIVE_CACHE_ROOT: cacheRoot
            },
            windowsHide: true
        });
        let stdout = '';
        let stderr = '';
        child.stdout.setEncoding('utf8');
        child.stderr.setEncoding('utf8');
        child.stdout.on('data', chunk => {
            stdout += chunk;
        });
        child.stderr.on('data', chunk => {
            stderr += chunk;
        });
        child.on('error', reject);
        child.on('close', code => {
            resolve({ code, stdout: stdout.trim(), stderr: stderr.trim() });
        });
    });
}

function Parseˉstatus(result, code, status = '') {
    if (result.code !== code) {
        Reject(`Unexpected checkpoint test exit ${result.code}: ${result.stderr}`);
    }
    const match = /^native segmented project cache status=(Created|Hit) key=([0-9a-f]{64}) entry-offset=([0-9]+) fragments=([1-8])$/.exec(result.stdout);
    if (status === '') {
        if (result.stdout !== '') {
            Reject(`Unexpected failed checkpoint output: ${result.stdout}`);
        }
        return null;
    }
    if (match === null || match[1] !== status) {
        Reject(`Unexpected checkpoint test status: ${result.stdout}`);
    }
    return {
        entryOffset: Number(match[3]),
        fragmentCount: Number(match[4]),
        key: match[2]
    };
}

async function Requireˉnoˉtemporaryˉentries(cacheRoot) {
    const family = path.join(cacheRoot, 'segmented-project-v1', HOST_FAMILY);
    const entries = await readdir(family, { withFileTypes: true }).catch(error => {
        if (error.code === 'ENOENT') {
            return [];
        }
        throw error;
    });
    if (entries.some(entry => entry.name.startsWith('.new-'))) {
        Reject(`A segmented-project checkpoint candidate was not cleaned: ${family}`);
    }
}

async function Requireˉsameˉproducts(left, right, fragmentCount) {
    const pairs = [
        [`${left}.wvb`, `${right}.wvb`],
        [`${left}.wvli`, `${right}.wvli`],
        ...Array.from(
            { length: fragmentCount },
            (_, index) => [`${left}.chunk-${index}`, `${right}.chunk-${index}`]
        )
    ];
    for (const [leftPath, rightPath] of pairs) {
        if (Digest(await readFile(leftPath)) !== Digest(await readFile(rightPath))) {
            Reject('Segmented-project checkpoint products are not deterministic.');
        }
    }
}

async function Removeˉtestˉroot(testRoot) {
    const temporaryRoot = await realpath(os.tmpdir());
    const canonical = await realpath(testRoot);
    if (path.dirname(canonical) !== temporaryRoot ||
        !/^windvale-segmented-project-checkpoint-test-[A-Za-z0-9_-]+$/.test(
            path.basename(canonical)
        )) {
        Reject(`Refusing to remove an unexpected checkpoint test root: ${canonical}`);
    }
    await rm(canonical, { recursive: true, force: false, maxRetries: 2 });
}

async function Main() {
    if (process.argv.length !== 2) {
        process.stderr.write(
            'Usage: node Tools/Native/Test-Segmented-Project-Checkpoint.mjs\n'
        );
        process.exit(64);
    }
    const testRoot = await mkdtemp(path.join(
        os.tmpdir(),
        'windvale-segmented-project-checkpoint-test-'
    ));
    try {
        const outputRoot = path.join(testRoot, 'Output');
        const normalCache = path.join(testRoot, 'Normal-Cache');
        await mkdir(outputRoot);

        const firstBase = path.join(outputRoot, 'First');
        const first = await Runˉbuilder(
            normalCache,
            `${firstBase}.wvb`,
            firstBase,
            `${firstBase}.wvli`
        );
        const firstStatus = Parseˉstatus(first, 0, 'Created');
        const secondBase = path.join(outputRoot, 'Second');
        const second = await Runˉbuilder(
            normalCache,
            `${secondBase}.wvb`,
            secondBase,
            `${secondBase}.wvli`
        );
        const secondStatus = Parseˉstatus(second, 0, 'Hit');
        if (firstStatus.key !== secondStatus.key ||
            firstStatus.entryOffset !== secondStatus.entryOffset ||
            firstStatus.fragmentCount !== secondStatus.fragmentCount) {
            Reject('Segmented-project checkpoint hit metadata differs.');
        }
        await Requireˉsameˉproducts(
            firstBase,
            secondBase,
            firstStatus.fragmentCount
        );

        const checkpointFragment = path.join(
            normalCache,
            'segmented-project-v1',
            HOST_FAMILY,
            firstStatus.key,
            'Product.chunk-0'
        );
        await appendFile(checkpointFragment, Buffer.from([0xa5]));
        const rejectedBase = path.join(outputRoot, 'Rejected');
        const sentinel = Buffer.from('segmented-project-sentinel\n', 'ascii');
        await writeFile(`${rejectedBase}.wvb`, sentinel);
        await writeFile(`${rejectedBase}.wvli`, sentinel);
        await writeFile(`${rejectedBase}.chunk-0`, sentinel);
        const corrupted = await Runˉbuilder(
            normalCache,
            `${rejectedBase}.wvb`,
            rejectedBase,
            `${rejectedBase}.wvli`
        );
        Parseˉstatus(corrupted, 1);
        for (const suffix of ['.wvb', '.wvli', '.chunk-0']) {
            if (!(await readFile(`${rejectedBase}${suffix}`)).equals(sentinel)) {
                Reject('Corrupt checkpoint rejection changed an owner output.');
            }
        }
        await Requireˉnoˉtemporaryˉentries(normalCache);

        const failureCache = path.join(testRoot, 'Failure-Cache');
        const failureBase = path.join(outputRoot, 'Failure');
        const failed = await Runˉbuilder(
            failureCache,
            `${failureBase}.wvb`,
            failureBase,
            `${failureBase}.wvli`,
            FAILED_BUILD_DRIVER
        );
        Parseˉstatus(failed, 1);
        await Requireˉnoˉtemporaryˉentries(failureCache);

        const raceCache = path.join(testRoot, 'Race-Cache');
        const racers = await Promise.all([0, 1, 2, 3].map(index => {
            const base = path.join(outputRoot, `Race-${index}`);
            return Runˉbuilder(
                raceCache,
                `${base}.wvb`,
                base,
                `${base}.wvli`
            );
        }));
        const parsedRacers = racers.map(result => {
            if (result.code !== 0) {
                Reject(`Unexpected segmented-project race failure: ${result.stderr}`);
            }
            return Parseˉstatus(
                result,
                0,
                result.stdout.includes('status=Created') ? 'Created' : 'Hit'
            );
        });
        const created = racers.filter(result =>
            result.stdout.startsWith('native segmented project cache status=Created ')
        ).length;
        const hits = racers.length - created;
        if (created !== 1 || hits !== 3) {
            Reject(`Unexpected segmented-project race result: created=${created} hits=${hits}`);
        }
        for (let index = 1; index < racers.length; index += 1) {
            if (parsedRacers[index].fragmentCount !== parsedRacers[0].fragmentCount) {
                Reject('Segmented-project race fragment counts differ.');
            }
            await Requireˉsameˉproducts(
                path.join(outputRoot, 'Race-0'),
                path.join(outputRoot, `Race-${index}`),
                parsedRacers[0].fragmentCount
            );
        }
        await Requireˉnoˉtemporaryˉentries(raceCache);
        process.stdout.write(
            'native segmented project checkpoint status=Passed ' +
            'creation=1 hits=1 corruption=Rejected failure=Cleaned ' +
            'race=Created-1/Hit-3\n'
        );
    } finally {
        await Removeˉtestˉroot(testRoot);
    }
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(1);
}
