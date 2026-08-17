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
    'Build-Cached-Project-Object.mjs'
);
const PROJECT = path.join(
    REPOSITORY_ROOT,
    'Projects',
    'Tests',
    'Windvale-Native-Test-Database-Tree-Node.wvproj'
);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const BUILD_DRIVER = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Compiler-Reconstruction-Candidate',
    WINDOWS ? 'windows-x64' : 'linux-x64',
    WINDOWS ? 'wvbuild.exe' : 'wvbuild.elf'
);
const LOWERER = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Wvb-To-Wvo-Candidate',
    WINDOWS ? 'Wvb-To-Wvo.exe' : 'Wvb-To-Wvo.elf'
);

function Reject(message) {
    throw new Error(message);
}

function Digest(bytes) {
    return createHash('sha256').update(bytes).digest('hex');
}

async function Runˉbuilder(cacheRoot, outputWvb, outputWvo, buildDriver = BUILD_DRIVER) {
    return new Promise((resolve, reject) => {
        const child = spawn(process.execPath, [
            BUILDER,
            PROJECT,
            buildDriver,
            LOWERER,
            outputWvb,
            outputWvo
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

function Requireˉstatus(result, code, status = '') {
    if (result.code !== code) {
        Reject(`Unexpected checkpoint test exit ${result.code}: ${result.stderr}`);
    }
    if (status !== '' && !result.stdout.startsWith(
        `native project object cache status=${status} key=`
    )) {
        Reject(`Unexpected checkpoint test status: ${result.stdout}`);
    }
}

function Keyˉfromˉstatus(result) {
    const match = / key=([0-9a-f]{64})$/.exec(result.stdout);
    if (match === null) {
        Reject(`Missing checkpoint key: ${result.stdout}`);
    }
    return match[1];
}

async function Requireˉnoˉtemporaryˉentries(cacheRoot) {
    const family = path.join(
        cacheRoot,
        'project-object-v2',
        HOST_FAMILY
    );
    const entries = await readdir(family, { withFileTypes: true }).catch(error => {
        if (error.code === 'ENOENT') {
            return [];
        }
        throw error;
    });
    if (entries.some(entry => entry.name.startsWith('.new-'))) {
        Reject(`A project-object checkpoint candidate was not cleaned: ${family}`);
    }
}

async function Requireˉsameˉproducts(firstWvb, firstWvo, secondWvb, secondWvo) {
    const firstWvbBytes = await readFile(firstWvb);
    const firstWvoBytes = await readFile(firstWvo);
    const secondWvbBytes = await readFile(secondWvb);
    const secondWvoBytes = await readFile(secondWvo);
    if (Digest(firstWvbBytes) !== Digest(secondWvbBytes) ||
        Digest(firstWvoBytes) !== Digest(secondWvoBytes)) {
        Reject('Project-object checkpoint products are not deterministic.');
    }
}

async function Removeˉtestˉroot(testRoot) {
    const temporaryRoot = await realpath(os.tmpdir());
    const canonical = await realpath(testRoot);
    if (path.dirname(canonical) !== temporaryRoot ||
        !/^windvale-project-object-checkpoint-test-[A-Za-z0-9_-]+$/.test(
            path.basename(canonical)
        )) {
        Reject(`Refusing to remove an unexpected checkpoint test root: ${canonical}`);
    }
    await rm(canonical, { recursive: true, force: false, maxRetries: 2 });
}

async function Main() {
    if (process.argv.length !== 2) {
        process.stderr.write(
            'Usage: node Tools/Native/Test-Project-Object-Checkpoint.mjs\n'
        );
        process.exit(64);
    }
    const testRoot = await mkdtemp(path.join(
        os.tmpdir(),
        'windvale-project-object-checkpoint-test-'
    ));
    try {
        const outputRoot = path.join(testRoot, 'Output');
        const normalCache = path.join(testRoot, 'Normal-Cache');
        await mkdir(outputRoot);

        const firstWvb = path.join(outputRoot, 'First.wvb');
        const firstWvo = path.join(outputRoot, 'First.wvo');
        const first = await Runˉbuilder(normalCache, firstWvb, firstWvo);
        Requireˉstatus(first, 0, 'Created');
        const secondWvb = path.join(outputRoot, 'Second.wvb');
        const secondWvo = path.join(outputRoot, 'Second.wvo');
        const second = await Runˉbuilder(normalCache, secondWvb, secondWvo);
        Requireˉstatus(second, 0, 'Hit');
        await Requireˉsameˉproducts(firstWvb, firstWvo, secondWvb, secondWvo);

        const key = Keyˉfromˉstatus(first);
        const checkpointWvo = path.join(
            normalCache,
            'project-object-v2',
            HOST_FAMILY,
            key,
            'Product.wvo'
        );
        await appendFile(checkpointWvo, Buffer.from([0xa5]));
        const rejectedWvb = path.join(outputRoot, 'Rejected.wvb');
        const rejectedWvo = path.join(outputRoot, 'Rejected.wvo');
        const sentinel = Buffer.from('project-object-sentinel\n', 'ascii');
        await writeFile(rejectedWvb, sentinel);
        await writeFile(rejectedWvo, sentinel);
        const corrupted = await Runˉbuilder(
            normalCache,
            rejectedWvb,
            rejectedWvo
        );
        Requireˉstatus(corrupted, 1);
        if (!(await readFile(rejectedWvb)).equals(sentinel) ||
            !(await readFile(rejectedWvo)).equals(sentinel)) {
            Reject('Corrupt checkpoint rejection changed an owner output.');
        }
        await Requireˉnoˉtemporaryˉentries(normalCache);

        const failureCache = path.join(testRoot, 'Failure-Cache');
        const failed = await Runˉbuilder(
            failureCache,
            path.join(outputRoot, 'Failure.wvb'),
            path.join(outputRoot, 'Failure.wvo'),
            LOWERER
        );
        Requireˉstatus(failed, 1);
        await Requireˉnoˉtemporaryˉentries(failureCache);

        const raceCache = path.join(testRoot, 'Race-Cache');
        const racers = await Promise.all([0, 1, 2, 3].map(index =>
            Runˉbuilder(
                raceCache,
                path.join(outputRoot, `Race-${index}.wvb`),
                path.join(outputRoot, `Race-${index}.wvo`)
            )
        ));
        const created = racers.filter(result =>
            result.stdout.startsWith('native project object cache status=Created ')
        ).length;
        const hits = racers.filter(result =>
            result.stdout.startsWith('native project object cache status=Hit ')
        ).length;
        if (racers.some(result => result.code !== 0) || created !== 1 || hits !== 3) {
            Reject(`Unexpected project-object race result: created=${created} hits=${hits}`);
        }
        for (let index = 1; index < racers.length; index += 1) {
            await Requireˉsameˉproducts(
                path.join(outputRoot, 'Race-0.wvb'),
                path.join(outputRoot, 'Race-0.wvo'),
                path.join(outputRoot, `Race-${index}.wvb`),
                path.join(outputRoot, `Race-${index}.wvo`)
            );
        }
        await Requireˉnoˉtemporaryˉentries(raceCache);
        process.stdout.write(
            'native project object checkpoint status=Passed ' +
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
