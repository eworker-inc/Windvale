import { createHash } from 'node:crypto';
import { spawnSync } from 'node:child_process';
import { realpathSync } from 'node:fs';
import {
    mkdtemp,
    mkdir,
    readFile,
    readdir,
    rm,
    writeFile,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    Orderˉsplitˉprojectˉsourceˉpayloads,
} from './Split-Project-Source-Ordering-Core.mjs';

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, '..', '..');
const CACHE_SCRIPT = path.join(
    SCRIPT_DIRECTORY,
    'Build-Cached-Split-Project-Wvb.mjs',
);
const IDENTITY_WRITER = path.join(
    SCRIPT_DIRECTORY,
    'Write-Split-Compiler-Producer-Identity.mjs',
);
const PROJECT = path.join(
    REPOSITORY_ROOT,
    'Projects',
    'Tests',
    'Windvale-Native-Test-Source-Descriptor.wvproj',
);
const HOST = `${process.platform}-${process.arch}`;
const TEMPORARY_PREFIX = 'windvale-split-cache-test-';
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS = 4_096;
const FAILURE_TIMEOUT_MILLISECONDS = 30_000;

if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`The split cache test does not support ${HOST}.`);
}

const Temporaryˉroot = realpathSync.native(os.tmpdir());
const Allocatedˉtestˉroot = await mkdtemp(
    path.join(Temporaryˉroot, TEMPORARY_PREFIX),
);
let Testˉroot;
try {
    Testˉroot = realpathSync.native(Allocatedˉtestˉroot);
} catch (Error) {
    await rm(Allocatedˉtestˉroot, { recursive: true, force: true });
    throw Error;
}
try {
    const Root = Buffer.from('module Root;\n', 'utf8');
    const Mainˉfile = Buffer.from(
        'module WebAssemblyˉinterpreter;\n',
        'utf8',
    );
    const Envelopeˉfile = Buffer.from(
        'module WebAssemblyˉinterpreterˉenvelope;\n',
        'utf8',
    );
    const Ordered = Orderˉsplitˉprojectˉsourceˉpayloads([
        Root,
        Envelopeˉfile,
        Mainˉfile,
    ]);
    if (Ordered.length !== 3 || Ordered[0] !== Root ||
        Ordered[1] !== Mainˉfile || Ordered[2] !== Envelopeˉfile) {
        Reject('Declared module identities did not determine source order.');
    }
    const Cacheˉroot = path.join(Testˉroot, 'cache');
    const Outputˉroot = path.join(Testˉroot, 'output');
    await mkdir(Outputˉroot);
    const Producer = path.join(Testˉroot, 'producer.bin');
    const Analyzerˉidentity = path.join(Testˉroot, 'analyzer.identity');
    const Emitterˉidentity = path.join(Testˉroot, 'emitter.identity');
    await writeFile(Producer, Buffer.from([0x57]));
    const Producerˉsha256 = createHash('sha256')
        .update(Buffer.from([0x57])).digest('hex');
    const Identityˉresult = spawnSync(process.execPath, [
        IDENTITY_WRITER,
        'analyzer',
        Producer,
        Analyzerˉidentity,
    ], {
        cwd: REPOSITORY_ROOT,
        encoding: 'utf8',
        windowsHide: true,
    });
    if (Identityˉresult.status !== 0 || Identityˉresult.stderr !== '' ||
        !(await readFile(Analyzerˉidentity)).equals(Buffer.from(
            Identity('analyzer', 1, Producerˉsha256), 'ascii'
        ))) {
        Reject(
            'The split producer identity was not published through the ' +
            'temporary directory path.',
        );
    }
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
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: FAILURE_TIMEOUT_MILLISECONDS,
        windowsHide: true,
    });
    if (Result.status === 0 || typeof Result.stderr !== 'string' ||
        !Result.stderr.includes('analyzer producer does not match its identity')) {
        Reject(
            'The forced producer-identity failure was not observed: ' +
            Childˉdiagnostic(Result),
        );
    }
    const Debris = await Findˉtemporaryˉdirectories(Cacheˉroot);
    if (Debris.length !== 0) {
        Reject(`The failed cache publication retained debris: ${Debris[0]}`);
    }
    console.log(
        'split project cache test cases=3 status=Passed ' +
        'module-order=Passed identity-publication=Passed ' +
        'forced-failure-cleanup=Passed',
    );
} finally {
    const Resolved = path.resolve(Testˉroot);
    if (!Sameˉpath(path.dirname(Resolved), Temporaryˉroot) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected split cache test directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}

function Childˉdiagnostic(Result) {
    const Status = Result.status === null ? 'null' : String(Result.status);
    const Signal = Result.signal === null ? 'none' : String(Result.signal);
    const Spawnˉerror = Result.error === undefined
        ? 'none'
        : Diagnosticˉtext(`${Result.error.name}:${Result.error.message}`);
    return `status=${Status} signal=${Signal} ` +
        `spawn-error=${JSON.stringify(Spawnˉerror)} ` +
        `stdout=${JSON.stringify(Diagnosticˉtext(Result.stdout))} ` +
        `stderr=${JSON.stringify(Diagnosticˉtext(Result.stderr))}`;
}

function Diagnosticˉtext(Value) {
    const Text = typeof Value === 'string' ? Value : String(Value ?? '');
    if (Text.length <= MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS) {
        return Text;
    }
    return Text.slice(0, MAXIMUM_REPORTED_DIAGNOSTIC_CHARACTERS) +
        `...[truncated characters=${Text.length}]`;
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

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}
