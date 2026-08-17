import { spawn } from 'node:child_process';
import { appendFile, mkdir, mkdtemp, readFile, realpath, readdir, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = path.resolve(path.dirname(SCRIPT_PATH), '..', '..');
const TOOL = path.join(
    REPOSITORY_ROOT,
    'Tools',
    'Native',
    'Build-Cached-Linked-Image-Set.mjs'
);
const FIRST_INPUT = path.join(
    REPOSITORY_ROOT,
    'Linker',
    'Reference',
    'Consumers',
    'X64-Publication-Transaction-State.wvo'
);
const SECOND_INPUT = path.join(
    REPOSITORY_ROOT,
    'Compiler',
    'Native',
    'Stencils',
    'Process-Argument-Count.wvo'
);
const ENTRY = 'Native_publication_apply';
const WINDOWS = process.platform === 'win32';
const HOST_FAMILY = WINDOWS ? 'windows-x64' : 'linux-x64';

function Reject(message) {
    throw new Error(message);
}

function Runˉtool(arguments_, cacheRoot) {
    return new Promise((resolve, reject) => {
        const child = spawn(process.execPath, [TOOL, ...arguments_], {
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
            resolve({
                code,
                stderr: stderr.trim(),
                stdout: stdout.trim()
            });
        });
    });
}

function Parseˉreport(result, expectedStatus) {
    if (result.code !== 0 || result.stderr !== '') {
        Reject(`Unexpected linked-image result ${result.code}: ${result.stderr}`);
    }
    const match = /^native linked image cache status=(Created|Hit) key=([0-9a-f]{64}) entry=(0|[1-9][0-9]*) inputs=2$/.exec(
        result.stdout
    );
    if (match === null || (expectedStatus !== null && match[1] !== expectedStatus)) {
        Reject(`Unexpected linked-image report: ${result.stdout}`);
    }
    return { key: match[2], status: match[1] };
}

async function Requireˉbytes(candidate, expected, label) {
    const actual = await readFile(candidate);
    if (!actual.equals(expected)) {
        Reject(`The ${label} bytes differ.`);
    }
}

async function Removeˉtestˉroot(testRoot) {
    const temporaryRoot = await realpath(os.tmpdir());
    const canonical = await realpath(testRoot);
    if (path.dirname(canonical) !== temporaryRoot ||
        !/^windvale-linked-image-set-test-[A-Za-z0-9_-]+$/.test(
            path.basename(canonical)
        )) {
        Reject(`Refusing to remove an unexpected linked-image test root: ${canonical}`);
    }
    await rm(canonical, { recursive: true, force: false, maxRetries: 2 });
}

async function Main() {
    if (process.argv.length !== 2) {
        process.stderr.write(
            'Usage: node Tools/Native/Test-Linked-Image-Set-Checkpoint.mjs\n'
        );
        process.exit(64);
    }
    const testRoot = await mkdtemp(path.join(
        os.tmpdir(),
        'windvale-linked-image-set-test-'
    ));
    try {
        const cacheRoot = path.join(testRoot, 'Cache');
        const outputRoot = path.join(testRoot, 'Output');
        await mkdir(cacheRoot);
        await mkdir(outputRoot);
        const argumentsFor = (image, map, inputs = [FIRST_INPUT, SECOND_INPUT]) => [
            '0',
            ENTRY,
            image,
            map,
            String(inputs.length),
            ...inputs
        ];

        const concurrent = await Promise.all([0, 1, 2, 3].map(index =>
            Runˉtool(argumentsFor(
                path.join(outputRoot, `Race-${index}.bin`),
                path.join(outputRoot, `Race-${index}.map`)
            ), cacheRoot)
        ));
        const reports = concurrent.map(result => Parseˉreport(result, null));
        if (reports.filter(report => report.status === 'Created').length !== 1 ||
            reports.some(report => report.key !== reports[0].key)) {
            Reject('The same-key linked-image race did not publish exactly one key.');
        }
        const imageBytes = await readFile(path.join(outputRoot, 'Race-0.bin'));
        const mapBytes = await readFile(path.join(outputRoot, 'Race-0.map'));
        for (let index = 1; index < 4; index += 1) {
            await Requireˉbytes(
                path.join(outputRoot, `Race-${index}.bin`),
                imageBytes,
                `race image ${index}`
            );
            await Requireˉbytes(
                path.join(outputRoot, `Race-${index}.map`),
                mapBytes,
                `race map ${index}`
            );
        }

        const hitImage = path.join(outputRoot, 'Hit.bin');
        const hitMap = path.join(outputRoot, 'Hit.map');
        const hit = Parseˉreport(await Runˉtool(
            argumentsFor(hitImage, hitMap),
            cacheRoot
        ), 'Hit');
        if (hit.key !== reports[0].key) {
            Reject('The linked-image warm hit selected another key.');
        }
        await Requireˉbytes(hitImage, imageBytes, 'warm-hit image');
        await Requireˉbytes(hitMap, mapBytes, 'warm-hit map');

        const reversed = Parseˉreport(await Runˉtool(argumentsFor(
            path.join(outputRoot, 'Reversed.bin'),
            path.join(outputRoot, 'Reversed.map'),
            [SECOND_INPUT, FIRST_INPUT]
        ), cacheRoot), 'Created');
        if (reversed.key === hit.key) {
            Reject('Reversing linked-image inputs did not change the key.');
        }

        const checkpointFamily = path.join(
            cacheRoot,
            'linked-image-v2',
            HOST_FAMILY
        );
        const checkpointProduct = path.join(
            checkpointFamily,
            hit.key,
            'Product.bin'
        );
        await appendFile(checkpointProduct, Buffer.from([0xa5]));
        const sentinel = Buffer.from('linked-image-sentinel\n', 'ascii');
        const corruptImage = path.join(outputRoot, 'Corrupt.bin');
        const corruptMap = path.join(outputRoot, 'Corrupt.map');
        await writeFile(corruptImage, sentinel);
        await writeFile(corruptMap, sentinel);
        const corrupted = await Runˉtool(
            argumentsFor(corruptImage, corruptMap),
            cacheRoot
        );
        if (corrupted.code !== 1) {
            Reject('A corrupt linked-image checkpoint was accepted.');
        }
        await Requireˉbytes(corruptImage, sentinel, 'corrupt rejection image');
        await Requireˉbytes(corruptMap, sentinel, 'corrupt rejection map');

        const malformedInput = path.join(testRoot, 'Malformed.wvo');
        await writeFile(malformedInput, Buffer.from('not a WVO\n', 'ascii'));
        const failedImage = path.join(outputRoot, 'Failed.bin');
        const failedMap = path.join(outputRoot, 'Failed.map');
        await writeFile(failedImage, sentinel);
        await writeFile(failedMap, sentinel);
        const failed = await Runˉtool(argumentsFor(
            failedImage,
            failedMap,
            [malformedInput, SECOND_INPUT]
        ), cacheRoot);
        if (failed.code !== 1) {
            Reject('A malformed linked-image input did not fail publication.');
        }
        await Requireˉbytes(failedImage, sentinel, 'failed publication image');
        await Requireˉbytes(failedMap, sentinel, 'failed publication map');
        if ((await readdir(checkpointFamily)).some(
            leaf => leaf.startsWith('.new-'))) {
            Reject('Failed or raced linked-image publication retained temporary debris.');
        }

        const malformedCount = await Runˉtool([
            '0',
            ENTRY,
            failedImage,
            failedMap,
            '02',
            FIRST_INPUT,
            SECOND_INPUT
        ], cacheRoot);
        if (malformedCount.code !== 64) {
            Reject('A noncanonical linked-image input count was accepted.');
        }
        await Requireˉbytes(failedImage, sentinel, 'malformed-count image');
        await Requireˉbytes(failedMap, sentinel, 'malformed-count map');

        process.stdout.write(
            'native linked image set checkpoint status=Passed ' +
            'race=4 key-order=Distinct hit=Exact corruption=Rejected ' +
            'failure=Clean malformed=Rejected\n'
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
