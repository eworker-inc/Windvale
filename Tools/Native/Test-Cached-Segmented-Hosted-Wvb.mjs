import { createHash } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import {
    chmod,
    lstat,
    mkdir,
    mkdtemp,
    readFile,
    readdir,
    realpath,
    rm,
    stat,
    writeFile,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import {
    Createˉsegmentedˉhostedˉcheckpoint,
    Materializeˉsegmentedˉhostedˉcheckpoint,
    Runˉboundedˉsegmentedˉhostedˉproducer,
    Validateˉsegmentedˉhostedˉcheckpoint,
} from './Build-Cached-Segmented-Hosted-Wvb.mjs';

const WINDOWS = process.platform === 'win32';
const PRODUCT_LEAF = WINDOWS ? 'Product.exe' : 'Product.elf';
const OUTPUT_LEAF = WINDOWS ? 'Materialized.exe' : 'Materialized.elf';
const TEMPORARY_PREFIX = 'windvale-segmented-hosted-cache-test-';
const temporaryRoot = await realpath(os.tmpdir());
const allocatedRoot = await mkdtemp(path.join(temporaryRoot, TEMPORARY_PREFIX));
const testRoot = await realpath(allocatedRoot);

try {
    const checkpointFamily = path.join(testRoot, 'checkpoint-family');
    const outputRoot = path.join(testRoot, 'output');
    await mkdir(checkpointFamily);
    await mkdir(outputRoot);
    const inputPath = path.join(testRoot, 'Input.wvb');
    const inputBytes = Buffer.from('bounded segmented hosted cache input\n', 'ascii');
    await writeFile(inputPath, inputBytes);
    const input = {
        bytes: inputBytes.length,
        path: inputPath,
        payload: inputBytes,
        sha256: createHash('sha256').update(inputBytes).digest('hex'),
    };
    const profile = '5';

    const timeoutPidPath = path.join(testRoot, 'Timeout.pid');
    const timeoutProducer = await Writeˉboundedˉproducer(
        testRoot,
        'Timeout-Producer',
        timeoutPidPath,
        false,
    );
    await Expectˉrejection(
        Runˉboundedˉsegmentedˉhostedˉproducer(
            timeoutProducer,
            [timeoutPidPath],
            'test-timeout',
            Date.now() + 500,
        ),
        'exceeded its deadline',
    );
    await Requireˉprocessˉstopped(await Readˉpid(timeoutPidPath));

    const stubbornPidPath = path.join(testRoot, 'Stubborn.pid');
    const stubbornProducer = await Writeˉboundedˉproducer(
        testRoot,
        'Stubborn-Producer',
        stubbornPidPath,
        false,
    );
    const stubbornStarted = Date.now();
    await Expectˉrejection(
        Runˉboundedˉsegmentedˉhostedˉproducer(
            stubbornProducer,
            [stubbornPidPath],
            'test-stubborn-pipe',
            Date.now() + 500,
            {
                closeGraceMilliseconds: 100,
                terminateProcessTree: async () => {
                    throw new Error('injected termination failure');
                },
            },
        ),
        'process-tree termination failed: injected termination failure',
    );
    if (Date.now() - stubbornStarted > 3_000) {
        Reject('The failed termination did not settle within its bound.');
    }
    await Forceˉstopˉtestˉprocess(await Readˉpid(stubbornPidPath));

    const overflowPidPath = path.join(testRoot, 'Overflow.pid');
    const overflowProducer = await Writeˉboundedˉproducer(
        testRoot,
        'Overflow-Producer',
        overflowPidPath,
        true,
    );
    await Expectˉrejection(
        Runˉboundedˉsegmentedˉhostedˉproducer(
            overflowProducer,
            [overflowPidPath],
            'test-overflow',
            Date.now() + 10_000,
        ),
        'stdout exceeded 64 KiB',
    );
    await Requireˉprocessˉstopped(await Readˉpid(overflowPidPath));

    const failedKey = '1'.repeat(64);
    await Expectˉrejection(
        Createˉsegmentedˉhostedˉcheckpoint(
            checkpointFamily,
            path.join(checkpointFamily, failedKey),
            failedKey,
            profile,
            input,
            async temporary => {
                await Writeˉproduct(temporary, Buffer.alloc(1_048_576, 0x31));
                throw new Error('forced producer failure');
            },
        ),
        'forced producer failure',
    );
    await Requireˉnoˉtemporaryˉcheckpoints(checkpointFamily);

    const unexpectedKey = '4'.repeat(64);
    const unexpectedDirectory = path.join(checkpointFamily, unexpectedKey);
    await Expectˉrejection(
        Createˉsegmentedˉhostedˉcheckpoint(
            checkpointFamily,
            unexpectedDirectory,
            unexpectedKey,
            profile,
            input,
            async temporary => {
                await Writeˉproduct(temporary, Buffer.from('candidate\n', 'ascii'));
                await writeFile(
                    path.join(temporary, 'Unexpected.bin'),
                    Buffer.from([0x57]),
                    { flag: 'wx' },
                );
            },
        ),
        'unexpected entries',
    );
    if (await lstat(unexpectedDirectory).catch(() => null) !== null) {
        Reject('The invalid temporary checkpoint was published.');
    }
    await Requireˉnoˉtemporaryˉcheckpoints(checkpointFamily);

    const createdKey = '2'.repeat(64);
    const createdDirectory = path.join(checkpointFamily, createdKey);
    const productBytes = Buffer.from('segmented hosted product\n', 'ascii');
    const createdStatus = await Createˉsegmentedˉhostedˉcheckpoint(
        checkpointFamily,
        createdDirectory,
        createdKey,
        profile,
        input,
        temporary => Writeˉproduct(temporary, productBytes),
    );
    if (createdStatus !== 'Created') {
        Reject(`The first checkpoint status differs: ${createdStatus}`);
    }
    await Requireˉnoˉtemporaryˉcheckpoints(checkpointFamily);
    const checkpoint = await Validateˉsegmentedˉhostedˉcheckpoint(
        createdDirectory,
        createdKey,
        profile,
        input,
    );
    const outputPath = path.join(outputRoot, OUTPUT_LEAF);
    await Materializeˉsegmentedˉhostedˉcheckpoint(
        checkpoint,
        outputPath,
    );
    if (!(await readFile(outputPath)).equals(productBytes)) {
        Reject('The materialized checkpoint bytes differ.');
    }
    if (!WINDOWS && ((await stat(outputPath)).mode & 0o111) === 0) {
        Reject('The materialized Unix product is not executable.');
    }
    if ((await readdir(outputRoot)).some(leaf =>
        leaf.startsWith('.new-materialization-'))) {
        Reject('The materialization retained a temporary file.');
    }
    await writeFile(
        path.join(createdDirectory, PRODUCT_LEAF),
        Buffer.from('corrupt product\n', 'ascii'),
    );
    await Expectˉrejection(
        Validateˉsegmentedˉhostedˉcheckpoint(
            createdDirectory,
            createdKey,
            profile,
            input,
        ),
        'manifest differs',
    );

    const raceKey = '3'.repeat(64);
    const raceDirectory = path.join(checkpointFamily, raceKey);
    const raceProduct = Buffer.alloc(2_097_152, 0x57);
    const raceStatuses = await Promise.all([
        Createˉsegmentedˉhostedˉcheckpoint(
            checkpointFamily,
            raceDirectory,
            raceKey,
            profile,
            input,
            async temporary => {
                await Delay(20);
                await Writeˉproduct(temporary, raceProduct);
            },
        ),
        Createˉsegmentedˉhostedˉcheckpoint(
            checkpointFamily,
            raceDirectory,
            raceKey,
            profile,
            input,
            temporary => Writeˉproduct(temporary, raceProduct),
        ),
    ]);
    if (raceStatuses.filter(status => status === 'Created').length !== 1 ||
        raceStatuses.filter(status => status === 'Hit').length !== 1) {
        Reject(`The publication-race statuses differ: ${raceStatuses.join(',')}`);
    }
    await Requireˉnoˉtemporaryˉcheckpoints(checkpointFamily);
    await Validateˉsegmentedˉhostedˉcheckpoint(
        raceDirectory,
        raceKey,
        profile,
        input,
    );
    const raceEntries = (await readdir(raceDirectory)).sort();
    if (raceEntries.join('\n') !== ['Checkpoint.txt', PRODUCT_LEAF].sort().join('\n')) {
        Reject(`The publication-race checkpoint entries differ: ${raceEntries}`);
    }

    console.log(
        'segmented hosted WVB cache test cases=10 status=Passed ' +
        'deadline-tree-termination=Passed output-bound-termination=Passed ' +
        'termination-failure-settle=Passed ' +
        'forced-failure-cleanup=Passed publication-cleanup=Passed ' +
        'prepublication-admission=Passed corruption-rejection=Passed race-winner=Passed ' +
        'race-cleanup=Passed executable-materialization=Passed',
    );
} finally {
    const resolved = path.resolve(testRoot);
    if (!Sameˉpath(path.dirname(resolved), temporaryRoot) ||
        !path.basename(resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected segmented hosted cache test root.');
    }
    await rm(resolved, { recursive: true, force: true });
}

async function Writeˉproduct(directory, bytes) {
    const product = path.join(directory, PRODUCT_LEAF);
    await writeFile(product, bytes, { flag: 'wx' });
    if (!WINDOWS) {
        await chmod(product, 0o755);
    }
}

async function Writeˉboundedˉproducer(
    directory,
    stem,
    pidPath,
    overflow,
) {
    const extension = WINDOWS ? '.cmd' : '.sh';
    const producer = path.join(directory, `${stem}${extension}`);
    const program = overflow
        ? "const fs=require('node:fs');fs.writeFileSync(process.argv[1]," +
            "String(process.pid));process.stdout.write('x'.repeat(70000));" +
            'setInterval(function(){},1000)'
        : "const fs=require('node:fs');fs.writeFileSync(process.argv[1]," +
            'String(process.pid));setInterval(function(){},1000)';
    const source = WINDOWS
        ? `@echo off\r\nnode -e "${program}" "%~1"\r\n`
        : `#!/usr/bin/env bash\nnode -e "${program}" "$1"\n`;
    await writeFile(producer, source, { encoding: 'ascii', flag: 'wx' });
    if (!WINDOWS) {
        await chmod(producer, 0o755);
    }
    if (path.resolve(pidPath) !== path.join(directory, path.basename(pidPath))) {
        Reject('The bounded producer PID path escaped the test root.');
    }
    return producer;
}

async function Readˉpid(pidPath) {
    const text = await readFile(pidPath, 'ascii');
    if (!/^[1-9][0-9]*$/u.test(text)) {
        Reject(`The bounded producer PID is invalid: ${text}`);
    }
    const pid = Number(text);
    if (!Number.isSafeInteger(pid)) {
        Reject(`The bounded producer PID exceeds its range: ${text}`);
    }
    return pid;
}

async function Requireˉprocessˉstopped(pid) {
    for (let attempt = 0; attempt < 20; attempt += 1) {
        try {
            process.kill(pid, 0);
        } catch (error) {
            if (error?.code === 'ESRCH') {
                return;
            }
            throw error;
        }
        await Delay(25);
    }
    Reject(`A bounded producer process remains alive: ${pid}`);
}

async function Forceˉstopˉtestˉprocess(pid) {
    if (WINDOWS) {
        const result = spawnSync(
            'taskkill.exe',
            ['/pid', String(pid), '/t', '/f'],
            {
                encoding: 'utf8',
                maxBuffer: 65_536,
                timeout: 10_000,
                windowsHide: true,
            },
        );
        if (result.error !== undefined || result.status !== 0) {
            Reject(
                'The injected stubborn process could not be stopped: ' +
                `${result.error?.message ?? result.stderr}`,
            );
        }
    } else {
        try {
            process.kill(pid, 'SIGKILL');
        } catch (error) {
            if (error?.code !== 'ESRCH') {
                throw error;
            }
        }
    }
    await Requireˉprocessˉstopped(pid);
}

async function Requireˉnoˉtemporaryˉcheckpoints(checkpointFamily) {
    const entries = await readdir(checkpointFamily, { withFileTypes: true });
    const temporary = entries.find(entry =>
        entry.isDirectory() && entry.name.startsWith('.new-'));
    if (temporary !== undefined) {
        Reject(`A temporary checkpoint was retained: ${temporary.name}`);
    }
}

async function Expectˉrejection(promise, text) {
    try {
        await promise;
    } catch (error) {
        if (String(error?.message).includes(text)) {
            return;
        }
        throw error;
    }
    Reject(`The expected rejection was not observed: ${text}`);
}

function Delay(milliseconds) {
    return new Promise(resolve => setTimeout(resolve, milliseconds));
}

function Sameˉpath(left, right) {
    return WINDOWS ? left.toLowerCase() === right.toLowerCase() : left === right;
}

function Reject(message) {
    throw new Error(message);
}
