import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import {
    appendFile,
    chmod,
    mkdir,
    mkdtemp,
    readFile,
    realpath,
    rm,
    stat,
    writeFile
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Getˉhostedˉapplicationˉcacheˉkey,
    Prepareˉhostedˉapplicationˉcontext
} from './Native-Hosted-Application-Cache-Core.mjs';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const SERVICE = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    'Build-Cached-Hosted-Application-Session.mjs'
);
const KEY_TOOL = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    'Get-Native-Hosted-Application-Cache-Key.mjs'
);
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';
const TARGET = WINDOWS ? 'windows' : 'linux';
const PACKAGER = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    `Package-Hosted-Wvb.${WINDOWS ? 'cmd' : 'sh'}`
);
const INPUT = path.join(
    REPOSITORY_ROOT,
    'Artifacts',
    'Native-Wvb-To-Wvo-Candidate',
    'Return-42.wvb'
);
const RECORD_ENDING = WINDOWS ? '\r\n' : '\n';

function Reject(message) {
    throw new Error(message);
}

function Checkpointˉrecord(key, product) {
    return Buffer.from([
        'windvale-native-hosted-application-checkpoint 1',
        `key ${key}`,
        `target ${TARGET}`,
        `application-bytes ${product.length}`,
        `application-sha256 ${product.sha256}`,
        ''
    ].join(RECORD_ENDING), 'ascii');
}

async function Runˉnode(arguments_, environment = process.env) {
    return new Promise((resolve, reject) => {
        const child = spawn(process.execPath, arguments_, {
            cwd: REPOSITORY_ROOT,
            env: environment,
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

function Startˉserver(readyPath, cacheRoot) {
    const child = spawn(process.execPath, [
        SERVICE,
        'serve',
        readyPath,
        TARGET
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
    return {
        child,
        result: new Promise((resolve, reject) => {
            child.on('error', reject);
            child.on('close', code => {
                resolve({ code, stdout: stdout.trim(), stderr: stderr.trim() });
            });
        })
    };
}

function Requireˉresult(result, code) {
    if (result.code !== code) {
        Reject(`Unexpected session exit ${result.code}: ${result.stderr}`);
    }
}

async function Removeˉtestˉroot(testRoot) {
    const temporaryRoot = await realpath(os.tmpdir());
    const canonical = await realpath(testRoot);
    if (path.dirname(canonical) !== temporaryRoot ||
        !/^windvale-hosted-application-session-test-[A-Za-z0-9_-]+$/.test(
            path.basename(canonical)
        )) {
        Reject(`Refusing to remove an unexpected session test root: ${canonical}`);
    }
    await rm(canonical, { recursive: true, force: false, maxRetries: 2 });
}

async function Main() {
    if (process.argv.length !== 2) {
        process.stderr.write(
            'Usage: node Tools/Native/Test-Hosted-Application-Session.mjs\n'
        );
        process.exit(64);
    }
    const testRoot = await mkdtemp(path.join(
        os.tmpdir(),
        'windvale-hosted-application-session-test-'
    ));
    let server = null;
    try {
        const cacheRoot = path.join(testRoot, 'Cache');
        const outputRoot = path.join(testRoot, 'Output');
        const readyPath = path.join(testRoot, 'Session.txt');
        const prefix = path.join(testRoot, 'Image');
        await mkdir(outputRoot);
        const fragment = Buffer.from([0xc3]);
        await writeFile(`${prefix}.chunk-0`, fragment);

        const context = await Prepareˉhostedˉapplicationˉcontext(
            TARGET,
            PACKAGER
        );
        const request = {
            namespace: 'hosted-application-v1',
            profile: '1',
            inputPath: INPUT,
            chunkPrefix: prefix,
            fragmentCountText: '1',
            entry: '0'
        };
        const key = await Getˉhostedˉapplicationˉcacheˉkey(
            context,
            request
        );
        const standalone = await Runˉnode([
            KEY_TOOL,
            request.namespace,
            TARGET,
            request.profile,
            INPUT,
            prefix,
            request.fragmentCountText,
            request.entry,
            PACKAGER
        ]);
        Requireˉresult(standalone, 0);
        if (standalone.stdout !== key) {
            Reject('The session and standalone hosted application keys differ.');
        }

        const checkpointDirectory = path.join(
            cacheRoot,
            'hosted-application-v1',
            HOST_FAMILY,
            key
        );
        await mkdir(checkpointDirectory, { recursive: true });
        const productLeaf = TARGET === 'windows' ? 'Product.exe' : 'Product.elf';
        const productPath = path.join(checkpointDirectory, productLeaf);
        const productBytes = Buffer.from('hosted-application-session-product\n', 'ascii');
        await writeFile(productPath, productBytes);
        if (!WINDOWS) {
            await chmod(productPath, 0o755);
        }
        const product = {
            length: productBytes.length,
            sha256: createHash('sha256').update(productBytes).digest('hex')
        };
        await writeFile(
            path.join(checkpointDirectory, 'Checkpoint.txt'),
            Checkpointˉrecord(key, product)
        );

        server = Startˉserver(readyPath, cacheRoot);
        const ready = await Runˉnode([SERVICE, 'wait', readyPath]);
        Requireˉresult(ready, 0);
        const readyBytes = await readFile(readyPath);

        const concurrent = await Promise.all([0, 1, 2, 3].map(index => {
            const extension = TARGET === 'windows' ? '.exe' : '.elf';
            return Runˉnode([
                SERVICE,
                'request',
                readyPath,
                request.profile,
                INPUT,
                prefix,
                request.fragmentCountText,
                request.entry,
                path.join(outputRoot, `Hit-${index}${extension}`),
                TARGET
            ]);
        }));
        for (let index = 0; index < concurrent.length; index += 1) {
            Requireˉresult(concurrent[index], 0);
            if (concurrent[index].stdout !==
                `native hosted application cache status=Hit key=${key} target=${TARGET}`) {
                Reject(`Unexpected session hit report: ${concurrent[index].stdout}`);
            }
            const extension = TARGET === 'windows' ? '.exe' : '.elf';
            const outputPath = path.join(outputRoot, `Hit-${index}${extension}`);
            if (!(await readFile(outputPath)).equals(productBytes)) {
                Reject('A hosted application session hit changed product bytes.');
            }
            if (!WINDOWS && ((await stat(outputPath)).mode & 0o111) === 0) {
                Reject('A hosted application session hit lost executable mode.');
            }
        }

        const malformedOutput = path.join(
            outputRoot,
            `Malformed.${TARGET === 'windows' ? 'exe' : 'elf'}`
        );
        const sentinel = Buffer.from('hosted-application-session-sentinel\n', 'ascii');
        await writeFile(malformedOutput, sentinel);
        if (!WINDOWS) {
            await chmod(malformedOutput, 0o755);
        }
        const malformed = await Runˉnode([
            SERVICE,
            'request',
            readyPath,
            '01',
            INPUT,
            prefix,
            request.fragmentCountText,
            request.entry,
            malformedOutput,
            TARGET
        ]);
        Requireˉresult(malformed, 1);
        if (!(await readFile(malformedOutput)).equals(sentinel)) {
            Reject('Malformed session rejection changed the owner output.');
        }

        await appendFile(productPath, Buffer.from([0xa5]));
        const rejectedOutput = path.join(
            outputRoot,
            `Rejected.${TARGET === 'windows' ? 'exe' : 'elf'}`
        );
        await writeFile(rejectedOutput, sentinel);
        if (!WINDOWS) {
            await chmod(rejectedOutput, 0o755);
        }
        const corrupted = await Runˉnode([
            SERVICE,
            'request',
            readyPath,
            request.profile,
            INPUT,
            prefix,
            request.fragmentCountText,
            request.entry,
            rejectedOutput,
            TARGET
        ]);
        Requireˉresult(corrupted, 1);
        if (!(await readFile(rejectedOutput)).equals(sentinel)) {
            Reject('Corrupt session checkpoint rejection changed the owner output.');
        }

        await writeFile(`${prefix}.chunk-0`, Buffer.from([0x90, 0xc3]));
        const missOutput = path.join(
            outputRoot,
            `Miss.${TARGET === 'windows' ? 'exe' : 'elf'}`
        );
        await writeFile(missOutput, sentinel);
        if (!WINDOWS) {
            await chmod(missOutput, 0o755);
        }
        const miss = await Runˉnode([
            SERVICE,
            'request',
            readyPath,
            request.profile,
            INPUT,
            prefix,
            request.fragmentCountText,
            request.entry,
            missOutput,
            TARGET
        ]);
        Requireˉresult(miss, 75);
        if (miss.stdout !== '' || miss.stderr !== '' ||
            !(await readFile(missOutput)).equals(sentinel)) {
            Reject('A hosted application session miss changed an owner output.');
        }

        await writeFile(readyPath, Buffer.from('damaged session record\n', 'ascii'));
        const malformedShutdown = await Runˉnode([
            SERVICE,
            'shutdown',
            readyPath
        ]);
        Requireˉresult(malformedShutdown, 1);
        await writeFile(readyPath, readyBytes);

        const shutdown = await Runˉnode([SERVICE, 'shutdown', readyPath]);
        Requireˉresult(shutdown, 0);
        const serverResult = await server.result;
        Requireˉresult(serverResult, 0);
        server = null;
        const readyInformation = await stat(readyPath).catch(error => {
            if (error.code === 'ENOENT') {
                return null;
            }
            throw error;
        });
        if (readyInformation !== null) {
            Reject('The hosted application session retained its readiness record.');
        }
        process.stdout.write(
            'native hosted application session status=Passed ' +
            'key=Equivalent concurrent-hits=4 corruption=Rejected ' +
            'malformed=Rejected miss=Unchanged record=Rejected lifecycle=Clean\n'
        );
    } finally {
        if (server !== null) {
            server.child.kill();
            await server.result.catch(() => {});
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
