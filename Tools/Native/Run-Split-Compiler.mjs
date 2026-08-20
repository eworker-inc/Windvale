import { spawn } from 'node:child_process';
import { constants } from 'node:fs';
import {
    copyFile,
    lstat,
    mkdtemp,
    open,
    realpath,
    rm,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';

const TEMPORARY_PREFIX = 'windvale-split-compiler-';
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const PRODUCER_TIMEOUT_MILLISECONDS = 300_000;

if (process.argv.length < 7) {
    Reject(
        'Usage: node Run-Split-Compiler.mjs <admitter> <analyzer> <emitter> ' +
        '[--source-input-lock <lock> <sha256> --source-profile <profile>] ' +
        '<root.wv> [dependency.wv ...] <output.wvb>',
    );
}
const Admitter = path.resolve(process.argv[2]);
const Analyzer = path.resolve(process.argv[3]);
const Emitter = path.resolve(process.argv[4]);
await Requireˉordinaryˉfile(Admitter, 134_217_728, 'source admission product');
await Requireˉordinaryˉfile(Analyzer, 134_217_728, 'source analyzer product');
await Requireˉordinaryˉfile(Emitter, 134_217_728, 'source emitter product');
const Arguments = process.argv.slice(5);
const Profiled = Arguments[0] === '--source-input-lock';
const Sourceˉstart = Profiled ? 5 : 0;
if ((Profiled && (Arguments.length < 7 || Arguments[3] !== '--source-profile')) ||
    Arguments.length - Sourceˉstart < 2) {
    Reject('The split compiler invocation is invalid.');
}
const Output = path.resolve(Arguments.at(-1));
if (path.extname(Output).toLowerCase() !== '.wvb' || await Exists(Output)) {
    Reject('The split compiler output must be a new .wvb path.');
}
await Requireˉordinaryˉdirectory(path.dirname(Output), 'output parent');

class Splitˉcompilerˉfailure extends Error {
    constructor(Status, Diagnostics) {
        super('A split compiler phase rejected its input.');
        this.status = Status === null || Status === 0 ? 1 : Status;
        this.diagnostics = Diagnostics;
    }
}

const Temporary = await mkdtemp(path.join(os.tmpdir(), TEMPORARY_PREFIX));
let Failure = null;
let Publicationˉattempted = false;
try {
    const Sourceˉset = path.join(Temporary, 'Source.wvss');
    const Manifest = path.join(Temporary, 'Manifest.wvca');
    const Bindings = path.join(Temporary, 'Bindings.wvlb');
    const Wir = path.join(Temporary, 'Wir.wvir');
    const Product = path.join(Temporary, 'Product.wvb');
    const Reports = [];
    if (Profiled) {
        Reports.push(await Runˉrequired(
            Admitter,
            [...Arguments.slice(0, -1), Sourceˉset],
            'source admission',
        ));
        Reports.push(await Runˉrequired(
            Analyzer,
            ['--admitted-source-set', Sourceˉset, Sourceˉset, Manifest, Bindings, Wir],
            'source analysis',
        ));
    } else {
        Reports.push(await Runˉrequired(
            Analyzer,
            [...Arguments.slice(0, -1), Sourceˉset, Manifest, Bindings, Wir],
            'source analysis',
        ));
    }
    Reports.push(await Runˉrequired(
        Emitter, [Sourceˉset, Manifest, Bindings, Wir, Product], 'source emission'
    ));
    await Requireˉordinaryˉfile(Product, MAXIMUM_WVB_BYTES, 'split compiler product');
    Publicationˉattempted = true;
    await copyFile(Product, Output, constants.COPYFILE_EXCL);
    await Syncˉfile(Output);
    await Requireˉordinaryˉfile(Output, MAXIMUM_WVB_BYTES, 'published split compiler product');
    for (const Report of Reports) {
        process.stdout.write(Report);
    }
} catch (Error) {
    Failure = Error;
} finally {
    if (Failure !== null && Publicationˉattempted) {
        await rm(Output, { force: true });
    }
    const Resolved = path.resolve(Temporary);
    if (path.dirname(Resolved) !== path.resolve(os.tmpdir()) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected split compiler directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}
if (Failure !== null) {
    if (Failure instanceof Splitˉcompilerˉfailure) {
        if (Failure.diagnostics.length !== 0) {
            process.stderr.write(Failure.diagnostics);
        }
        process.exitCode = Failure.status;
    } else {
        throw Failure;
    }
}

async function Runˉrequired(Command, Arguments, Step) {
    const Result = await Runˉbounded(Command, Arguments);
    if (Result.status !== 0) {
        throw new Splitˉcompilerˉfailure(Result.status, Result.stderr);
    }
    if (Result.stderr.length !== 0 || Result.stdout.length === 0) {
        Reject(`The ${Step} produced invalid success diagnostics.`);
    }
    return Result.stdout;
}

async function Runˉbounded(Command, Arguments) {
    return await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        let Stdout = Buffer.alloc(0);
        let Stderr = Buffer.alloc(0);
        let Settled = false;
        let Timeout = null;
        const Finish = Result => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timeout);
            Resolve(Result);
        };
        const Fail = Error => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timeout);
            Rejectˉpromise(Error);
        };
        const Append = (Current, Chunk) => {
            if (Current.length + Chunk.length > MAXIMUM_DIAGNOSTIC_BYTES) {
                Child.kill();
                Fail(new Error('Split compiler diagnostics exceed 64 KiB.'));
                return Current;
            }
            return Buffer.concat([Current, Chunk]);
        };
        Child.stdout.on('data', Chunk => { Stdout = Append(Stdout, Chunk); });
        Child.stderr.on('data', Chunk => { Stderr = Append(Stderr, Chunk); });
        Timeout = setTimeout(() => {
            Child.kill();
            Finish({ status: 1, stdout: Stdout, stderr: Buffer.from('Split compiler timeout.\n') });
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        Child.on('error', Fail);
        Child.on('close', Status => Finish({ status: Status, stdout: Stdout, stderr: Stderr }));
    });
}

async function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (Canonical !== path.resolve(Candidate) &&
        !(process.platform === 'win32' &&
          Canonical.toLowerCase() === path.resolve(Candidate).toLowerCase())) {
        Reject(`The ${Label} path is not canonical: ${Candidate}`);
    }
}

async function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isDirectory() ||
        Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (Canonical !== path.resolve(Candidate) &&
        !(process.platform === 'win32' &&
          Canonical.toLowerCase() === path.resolve(Candidate).toLowerCase())) {
        Reject(`The ${Label} path is not canonical: ${Candidate}`);
    }
}

async function Syncˉfile(Candidate) {
    const Handle = await open(Candidate, 'r+');
    try {
        await Handle.sync();
    } finally {
        await Handle.close();
    }
}

async function Exists(Candidate) {
    return await lstat(Candidate).then(() => true, () => false);
}

function Reject(Message) {
    throw new Error(Message);
}
