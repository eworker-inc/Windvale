import { spawnSync } from 'node:child_process';
import { mkdtemp, mkdir, readdir, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, '..', '..');
const CACHE_SCRIPT = path.join(
    SCRIPT_DIRECTORY,
    'Build-Cached-Split-Project-Wvb.mjs',
);
const PROJECT = path.join(
    REPOSITORY_ROOT,
    'Projects',
    'Tests',
    'Windvale-Native-Test-Source-Descriptor.wvproj',
);
const HOST = `${process.platform}-${process.arch}`;
const TEMPORARY_PREFIX = 'windvale-split-cache-test-';

if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`The split cache test does not support ${HOST}.`);
}

const Testˉroot = await mkdtemp(path.join(os.tmpdir(), TEMPORARY_PREFIX));
try {
    const Cacheˉroot = path.join(Testˉroot, 'cache');
    const Outputˉroot = path.join(Testˉroot, 'output');
    await mkdir(Outputˉroot);
    const Producer = path.join(Testˉroot, 'producer.bin');
    const Analyzerˉidentity = path.join(Testˉroot, 'analyzer.identity');
    const Emitterˉidentity = path.join(Testˉroot, 'emitter.identity');
    await writeFile(Producer, Buffer.from([0x57]));
    await writeFile(
        Analyzerˉidentity,
        Identity('analyzer', 2, '0'.repeat(64)),
        'ascii',
    );
    await writeFile(
        Emitterˉidentity,
        Identity('emitter', 1, '0'.repeat(64)),
        'ascii',
    );

    const Result = spawnSync(process.execPath, [
        CACHE_SCRIPT,
        PROJECT,
        path.join(Outputˉroot, 'Product.wvb'),
        Producer,
        Analyzerˉidentity,
        Producer,
        Emitterˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        env: {
            ...process.env,
            WINDVALE_NATIVE_CACHE_ROOT: Cacheˉroot,
        },
        windowsHide: true,
    });
    if (Result.status === 0 ||
        !Result.stderr.includes('analyzer producer does not match its identity')) {
        Reject('The forced producer-identity failure was not observed.');
    }
    const Debris = await Findˉtemporaryˉdirectories(Cacheˉroot);
    if (Debris.length !== 0) {
        Reject(`The failed cache publication retained debris: ${Debris[0]}`);
    }
    console.log(
        'split project cache test cases=1 status=Passed ' +
        'forced-failure-cleanup=Passed',
    );
} finally {
    const Resolved = path.resolve(Testˉroot);
    if (path.dirname(Resolved) !== path.resolve(os.tmpdir()) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected split cache test directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}

function Identity(Role, Bytes, Sha256) {
    const Target = Role === 'analyzer'
        ? 'source-analysis-v1'
        : 'portable-wvb-optimized-v1';
    return `windvale-split-compiler-producer 2\n` +
        `role ${Role}\n` +
        `target ${Target}\n` +
        `host ${HOST}\n` +
        `bytes ${Bytes}\n` +
        `sha256 ${Sha256}\n`;
}

async function Findˉtemporaryˉdirectories(Root) {
    const Found = [];
    await Visit(Root);
    return Found;

    async function Visit(Directory) {
        const Entries = await readdir(Directory, {
            withFileTypes: true,
        }).catch(error => {
            if (error?.code === 'ENOENT') {
                return [];
            }
            throw error;
        });
        for (const Entry of Entries) {
            if (!Entry.isDirectory()) {
                continue;
            }
            const Candidate = path.join(Directory, Entry.name);
            if (Entry.name.startsWith('.new-')) {
                Found.push(Candidate);
            }
            await Visit(Candidate);
        }
    }
}

function Reject(Message) {
    throw new Error(Message);
}
