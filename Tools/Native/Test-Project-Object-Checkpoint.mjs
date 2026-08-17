import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    appendFile,
    chmod,
    copyFile,
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
const SESSION = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    'Build-Cached-Hosted-Application-Session.mjs'
);
const KEY_TOOL = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    'Get-Native-Project-Cache-Key.mjs'
);
const PROJECT = path.join(
    REPOSITORY_ROOT,
    'Projects',
    'Tests',
    'Windvale-Native-Test-Database-Tree-Node.wvproj'
);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const TARGET = WINDOWS ? 'windows' : 'linux';
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

function Runˉnode(arguments_, cacheRoot) {
    return new Promise((resolve, reject) => {
        const child = spawn(process.execPath, arguments_, {
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

function Startˉserver(
    readyPath,
    cacheRoot,
    buildDriver = BUILD_DRIVER,
    lowerer = LOWERER
) {
    const child = spawn(process.execPath, [
        SESSION,
        'serve',
        readyPath,
        TARGET,
        buildDriver,
        lowerer
    ], {
        cwd: REPOSITORY_ROOT,
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: cacheRoot
        },
        windowsHide: true
    });
    let stderr = '';
    child.stderr.setEncoding('utf8');
    child.stderr.on('data', chunk => {
        stderr += chunk;
    });
    return {
        child,
        result: new Promise((resolve, reject) => {
            child.on('error', reject);
            child.on('close', code => {
                resolve({ code, stderr: stderr.trim() });
            });
        })
    };
}

function Digest(bytes) {
    return createHash('sha256').update(bytes).digest('hex');
}

async function Runˉbuilder(
    cacheRoot,
    outputWvb,
    outputWvo,
    buildDriver = BUILD_DRIVER,
    lowerer = LOWERER
) {
    return new Promise((resolve, reject) => {
        const child = spawn(process.execPath, [
            BUILDER,
            PROJECT,
            buildDriver,
            lowerer,
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
    let server = null;
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
        const readyPath = path.join(testRoot, 'Session.txt');
        server = Startˉserver(readyPath, normalCache);
        const ready = await Runˉnode([SESSION, 'wait', readyPath], normalCache);
        Requireˉstatus(ready, 0);
        const sessionHits = await Promise.all([0, 1, 2, 3].map(index =>
            Runˉnode([
                SESSION,
                'project-request',
                readyPath,
                PROJECT,
                path.join(outputRoot, `Session-${index}.wvb`),
                path.join(outputRoot, `Session-${index}.wvo`)
            ], normalCache)
        ));
        for (let index = 0; index < sessionHits.length; index += 1) {
            Requireˉstatus(sessionHits[index], 0, 'Hit');
            await Requireˉsameˉproducts(
                firstWvb,
                firstWvo,
                path.join(outputRoot, `Session-${index}.wvb`),
                path.join(outputRoot, `Session-${index}.wvo`)
            );
        }

        const sentinel = Buffer.from('project-object-sentinel\n', 'ascii');
        const missWvb = path.join(outputRoot, 'Session-Miss.wvb');
        const missWvo = path.join(outputRoot, 'Session-Miss.wvo');
        await writeFile(missWvb, sentinel);
        await writeFile(missWvo, sentinel);
        const missProject = path.join(
            REPOSITORY_ROOT,
            'Projects',
            'Tests',
            'Windvale-Native-Test-Database-Logical-Record.wvproj'
        );
        const miss = await Runˉnode([
            SESSION,
            'project-request',
            readyPath,
            missProject,
            missWvb,
            missWvo
        ], normalCache);
        Requireˉstatus(miss, 75);
        if (!(await readFile(missWvb)).equals(sentinel) ||
            !(await readFile(missWvo)).equals(sentinel)) {
            Reject('A project-object session miss changed an owner output.');
        }

        await appendFile(checkpointWvo, Buffer.from([0xa5]));
        const sessionRejectedWvb = path.join(
            outputRoot,
            'Session-Rejected.wvb'
        );
        const sessionRejectedWvo = path.join(
            outputRoot,
            'Session-Rejected.wvo'
        );
        await writeFile(sessionRejectedWvb, sentinel);
        await writeFile(sessionRejectedWvo, sentinel);
        const sessionCorrupted = await Runˉnode([
            SESSION,
            'project-request',
            readyPath,
            PROJECT,
            sessionRejectedWvb,
            sessionRejectedWvo
        ], normalCache);
        Requireˉstatus(sessionCorrupted, 1);
        if (!(await readFile(sessionRejectedWvb)).equals(sentinel) ||
            !(await readFile(sessionRejectedWvo)).equals(sentinel)) {
            Reject('Corrupt project-object session changed an owner output.');
        }
        const shutdown = await Runˉnode([
            SESSION,
            'shutdown',
            readyPath
        ], normalCache);
        Requireˉstatus(shutdown, 0);
        const serverResult = await server.result;
        Requireˉstatus(serverResult, 0);
        server = null;

        const trustedCache = path.join(testRoot, 'Trusted-Cache');
        const trustedLowerer = path.join(
            testRoot,
            WINDOWS ? 'Trusted-Lowerer.exe' : 'Trusted-Lowerer.elf'
        );
        await copyFile(LOWERER, trustedLowerer);
        if (!WINDOWS) {
            await chmod(trustedLowerer, 0o755);
        }
        const trustedWvb = path.join(outputRoot, 'Trusted.wvb');
        const trustedWvo = path.join(outputRoot, 'Trusted.wvo');
        const trusted = await Runˉbuilder(
            trustedCache,
            trustedWvb,
            trustedWvo,
            BUILD_DRIVER,
            trustedLowerer
        );
        Requireˉstatus(trusted, 0, 'Created');
        const trustedReady = path.join(testRoot, 'Trusted-Session.txt');
        server = Startˉserver(
            trustedReady,
            trustedCache,
            BUILD_DRIVER,
            trustedLowerer
        );
        const trustedSession = await Runˉnode(
            [SESSION, 'wait', trustedReady],
            trustedCache
        );
        Requireˉstatus(trustedSession, 0);
        await appendFile(trustedLowerer, Buffer.from([0xa5]));
        const trustedHitWvb = path.join(outputRoot, 'Trusted-Hit.wvb');
        const trustedHitWvo = path.join(outputRoot, 'Trusted-Hit.wvo');
        const trustedHit = await Runˉnode([
            SESSION,
            'project-request',
            trustedReady,
            PROJECT,
            trustedHitWvb,
            trustedHitWvo
        ], trustedCache);
        Requireˉstatus(trustedHit, 0, 'Hit');
        await Requireˉsameˉproducts(
            trustedWvb,
            trustedWvo,
            trustedHitWvb,
            trustedHitWvo
        );
        const trustedShutdown = await Runˉnode(
            [SESSION, 'shutdown', trustedReady],
            trustedCache
        );
        Requireˉstatus(trustedShutdown, 0);
        const trustedServerResult = await server.result;
        Requireˉstatus(trustedServerResult, 0);
        server = null;

        const excessiveProducers = await Runˉnode([
            KEY_TOOL,
            'database-project-object-v2',
            PROJECT,
            ...Array(17).fill(BUILDER)
        ], normalCache);
        Requireˉstatus(excessiveProducers, 1);
        if (!excessiveProducers.stderr.includes(
            'requires one through 16 producers')) {
            Reject('The excessive producer rejection differs.');
        }

        const rejectedWvb = path.join(outputRoot, 'Rejected.wvb');
        const rejectedWvo = path.join(outputRoot, 'Rejected.wvo');
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
            'race=Created-1/Hit-3 session=Hit-4/Miss-Unchanged/' +
            'Corruption-Rejected/Lifecycle-Clean producer-snapshot=Exact ' +
            'producer-bound=Rejected\n'
        );
    } finally {
        if (server !== null && !server.child.killed) {
            server.child.kill();
        }
        await Removeˉtestˉroot(testRoot);
    }
}

try {
    await Main();
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(1);
}
