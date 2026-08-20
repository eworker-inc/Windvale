import { createHash, randomBytes } from 'node:crypto';
import { spawn } from 'node:child_process';
import { createReadStream } from 'node:fs';
import {
    copyFile,
    lstat,
    mkdir,
    mkdtemp,
    open,
    readFile,
    realpath,
    rename,
    rm,
    stat,
    writeFile,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import {
    Getˉnativeˉprojectˉcacheˉrequest,
    Prepareˉnativeˉprojectˉcacheˉcontext,
    REPOSITORY_ROOT,
    Requireˉnativeˉprojectˉcacheˉrequestˉunchanged,
} from './Native-Project-Cache-Key-Core.mjs';

const MAXIMUM_VALUE_BYTES = 4_194_304;
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_MANIFEST_BYTES = 4_096;
const MAXIMUM_PROJECT_BYTES = 65_536;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const PRODUCER_TIMEOUT_MILLISECONDS = 300_000;
const HOST = `${process.platform}-${process.arch}`;

if (process.argv.length !== 8) {
    Usage();
}
if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`The split compiler cache does not support ${HOST}.`);
}

const Projectˉpath = path.resolve(process.argv[2]);
const Outputˉpath = path.resolve(process.argv[3]);
const Analyzerˉpath = path.resolve(process.argv[4]);
const Analyzerˉidentityˉpath = path.resolve(process.argv[5]);
const Emitterˉpath = path.resolve(process.argv[6]);
const Emitterˉidentityˉpath = path.resolve(process.argv[7]);
if (path.extname(Projectˉpath).toLowerCase() !== '.wvproj') {
    Reject('The split compiler input must use the .wvproj extension.');
}
if (path.extname(Outputˉpath).toLowerCase() !== '.wvb') {
    Reject('The split compiler output must use the .wvb extension.');
}
if (Outputˉpath === Projectˉpath || Outputˉpath === Analyzerˉpath ||
    Outputˉpath === Analyzerˉidentityˉpath || Outputˉpath === Emitterˉpath ||
    Outputˉpath === Emitterˉidentityˉpath) {
    Reject('The split compiler output overlaps an input or producer.');
}

const Projectˉtext = (await Readˉbounded(
    Projectˉpath,
    'project manifest',
    MAXIMUM_PROJECT_BYTES,
)).toString('utf8');
const Projectˉinputs = Parseˉprojectˉ2(Projectˉtext);
const Outputˉparent = path.dirname(Outputˉpath);
await Requireˉordinaryˉdirectory(Outputˉparent, 'output parent');
const Analyzerˉidentity = await Readˉproducerˉidentity(
    Analyzerˉidentityˉpath,
    'analyzer',
);
const Emitterˉidentity = await Readˉproducerˉidentity(
    Emitterˉidentityˉpath,
    'emitter',
);

const Cacheˉroot = await Prepareˉcacheˉroot();
const Analysisˉcontext = await Prepareˉnativeˉprojectˉcacheˉcontext(
    'project-analysis-wvca-v2',
    [Analyzerˉidentityˉpath],
);
const Analysisˉrequest = await Getˉnativeˉprojectˉcacheˉrequest(
    Analysisˉcontext,
    Projectˉpath,
);
await Requireˉproducerˉidentityˉunchanged(
    Analyzerˉidentityˉpath,
    Analyzerˉidentity,
);
const Analysisˉinputˉbyˉpath = new Map(
    Analysisˉrequest.inputEvidence.slice(1).map(Evidence => [
        Normalizedˉpath(Evidence.path),
        Evidence.path,
    ]),
);
const Orderedˉanalysisˉinputs = Projectˉinputs.map(Candidate => {
    const Input = Analysisˉinputˉbyˉpath.get(Normalizedˉpath(Candidate));
    if (Input === undefined) {
        Reject(`The split compiler project input is unavailable: ${Candidate}`);
    }
    return Input;
});
const Analysisˉfamily = await Prepareˉfamily(
    Cacheˉroot,
    'project-analysis-wvca-v2',
);
const Analysisˉcheckpoint = await Acquireˉanalysis(
    Analysisˉfamily,
    Analysisˉrequest,
    Orderedˉanalysisˉinputs,
    Analyzerˉpath,
    Analyzerˉidentity,
);

const Emissionˉcontext = await Prepareˉnativeˉprojectˉcacheˉcontext(
    'project-split-wvb-optimized-v2',
    [Analyzerˉidentityˉpath, Emitterˉidentityˉpath],
);
const Emissionˉrequest = await Getˉnativeˉprojectˉcacheˉrequest(
    Emissionˉcontext,
    Projectˉpath,
);
await Requireˉproducerˉidentityˉunchanged(
    Analyzerˉidentityˉpath,
    Analyzerˉidentity,
);
await Requireˉproducerˉidentityˉunchanged(
    Emitterˉidentityˉpath,
    Emitterˉidentity,
);
const Emissionˉfamily = await Prepareˉfamily(
    Cacheˉroot,
    'project-split-wvb-optimized-v2',
);
const Emissionˉcheckpoint = await Acquireˉemission(
    Emissionˉfamily,
    Emissionˉrequest,
    Analysisˉrequest.key,
    Analysisˉcheckpoint,
    Emitterˉpath,
    Emitterˉidentity,
);

const Productˉpath = path.join(Emissionˉcheckpoint, 'Product.wvb');
await copyFile(Productˉpath, Outputˉpath);
await Syncˉfile(Outputˉpath);
const Productˉevidence = await Fileˉevidence(
    Productˉpath,
    'cached WVB',
    MAXIMUM_WVB_BYTES,
);
const Outputˉevidence = await Fileˉevidence(
    Outputˉpath,
    'published WVB',
    MAXIMUM_WVB_BYTES,
);
if (Productˉevidence.bytes !== Outputˉevidence.bytes ||
    Productˉevidence.sha256 !== Outputˉevidence.sha256) {
    Reject('The published split WVB does not match its checkpoint.');
}
console.log(
    `split project status=Published target=portable-wvb-optimized-v1 ` +
    `wvb-bytes=${Outputˉevidence.bytes} wvb-sha256=${Outputˉevidence.sha256}`,
);

async function Acquireˉanalysis(Family, Request, Inputs, Analyzer, Identity) {
    const Checkpoint = path.join(Family, Request.key);
    if (await Exists(Checkpoint)) {
        await Validateˉanalysis(Checkpoint, Request.key);
        console.log(`split project step=analysis cache=Hit key=${Request.key}`);
        return Checkpoint;
    }
    let Temporary = '';
    try {
        Temporary = await Allocateˉtemporary(Family, Request.key);
        await Verifyˉproducer(Analyzer, Identity);
        await Run(Analyzer, [
            ...Inputs,
            path.join(Temporary, 'Source.wvss'),
            path.join(Temporary, 'Manifest.wvca'),
            path.join(Temporary, 'Bindings.wvlb'),
            path.join(Temporary, 'Wir.wvir'),
        ], 'analysis');
        await Verifyˉproducer(Analyzer, Identity);
        const Evidence = await Analysisˉevidence(Temporary);
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
        await Writeˉcheckpoint(
            Temporary,
            Analysisˉmanifest(Request.key, Evidence),
        );
        try {
            await rename(Temporary, Checkpoint);
            Temporary = '';
        } catch (error) {
            if (error?.code !== 'EEXIST' && error?.code !== 'ENOTEMPTY') {
                throw error;
            }
        }
        await Validateˉanalysis(Checkpoint, Request.key);
        console.log(`split project step=analysis cache=Created key=${Request.key}`);
        return Checkpoint;
    } finally {
        await Removeˉtemporary(Family, Request.key, Temporary);
    }
}

function Parseˉprojectˉ2(Text) {
    const Lines = Text.split(/\r?\n/u);
    if (Lines[Lines.length - 1] === '') {
        Lines.pop();
    }
    if (Lines.length < 3 || Lines[0] !== 'windvale-project 2' ||
        Lines.some(Line => Line.length === 0)) {
        Reject('The split compiler currently admits only strict Project 2 manifests.');
    }
    let Root = '';
    let Emit = 0;
    const Sources = [];
    const Identities = new Set();
    for (const Line of Lines.slice(1)) {
        if (Line === 'emit wvb') {
            Emit += 1;
            continue;
        }
        const Match = /^(root|source) "([^"\r\n]+)"$/u.exec(Line);
        if (Match === null || !Validˉprojectˉsourceˉpath(Match[2])) {
            Reject('The split compiler Project 2 manifest has an unsupported directive.');
        }
        const Candidate = path.resolve(
            REPOSITORY_ROOT,
            ...Match[2].split('/'),
        );
        const Identity = Normalizedˉpath(Candidate);
        if (Identities.has(Identity)) {
            Reject('The split compiler Project 2 manifest repeats a source path.');
        }
        Identities.add(Identity);
        if (Match[1] === 'root') {
            if (Root !== '') {
                Reject('The split compiler Project 2 manifest repeats its root.');
            }
            Root = Candidate;
        } else {
            Sources.push({ path: Candidate, text: Match[2] });
        }
    }
    if (Root === '' || Emit !== 1 || Sources.length > 63) {
        Reject('The split compiler Project 2 manifest has invalid cardinality.');
    }
    Sources.sort((Left, Right) => Left.text < Right.text
        ? -1
        : Left.text > Right.text ? 1 : 0);
    return [Root, ...Sources.map(Source => Source.path)];
}

function Validˉprojectˉsourceˉpath(Value) {
    if (!Value.endsWith('.wv') || Value.includes('\\') ||
        path.posix.isAbsolute(Value)) {
        return false;
    }
    const Parts = Value.split('/');
    return Parts.length > 0 && Parts.every(Part =>
        /^[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?$/u.test(Part));
}

function Normalizedˉpath(Candidate) {
    const Resolved = path.resolve(Candidate);
    return process.platform === 'win32' ? Resolved.toLowerCase() : Resolved;
}

async function Acquireˉemission(
    Family,
    Request,
    Analysisˉkey,
    Analysisˉcheckpoint,
    Emitter,
    Identity,
) {
    const Checkpoint = path.join(Family, Request.key);
    if (await Exists(Checkpoint)) {
        await Validateˉemission(Checkpoint, Request.key, Analysisˉkey);
        console.log(`split project step=emission cache=Hit key=${Request.key}`);
        return Checkpoint;
    }
    let Temporary = '';
    try {
        Temporary = await Allocateˉtemporary(Family, Request.key);
        await Verifyˉproducer(Emitter, Identity);
        await Run(Emitter, [
            path.join(Analysisˉcheckpoint, 'Source.wvss'),
            path.join(Analysisˉcheckpoint, 'Manifest.wvca'),
            path.join(Analysisˉcheckpoint, 'Bindings.wvlb'),
            path.join(Analysisˉcheckpoint, 'Wir.wvir'),
            path.join(Temporary, 'Product.wvb'),
        ], 'emission');
        await Verifyˉproducer(Emitter, Identity);
        const Evidence = await Fileˉevidence(
            path.join(Temporary, 'Product.wvb'),
            'emitted WVB',
            MAXIMUM_WVB_BYTES,
        );
        await Requireˉnativeˉprojectˉcacheˉrequestˉunchanged(Request);
        await Writeˉcheckpoint(
            Temporary,
            Emissionˉmanifest(Request.key, Analysisˉkey, Evidence),
        );
        try {
            await rename(Temporary, Checkpoint);
            Temporary = '';
        } catch (error) {
            if (error?.code !== 'EEXIST' && error?.code !== 'ENOTEMPTY') {
                throw error;
            }
        }
        await Validateˉemission(Checkpoint, Request.key, Analysisˉkey);
        console.log(`split project step=emission cache=Created key=${Request.key}`);
        return Checkpoint;
    } finally {
        await Removeˉtemporary(Family, Request.key, Temporary);
    }
}

async function Validateˉanalysis(Checkpoint, Key) {
    await Requireˉordinaryˉdirectory(Checkpoint, 'analysis checkpoint');
    const Evidence = await Analysisˉevidence(Checkpoint);
    const Actual = await Readˉbounded(
        path.join(Checkpoint, 'Checkpoint.txt'),
        'analysis checkpoint manifest',
        MAXIMUM_MANIFEST_BYTES,
    );
    if (!Actual.equals(Buffer.from(Analysisˉmanifest(Key, Evidence), 'ascii'))) {
        Reject('The analysis checkpoint manifest is invalid.');
    }
}

async function Validateˉemission(Checkpoint, Key, Analysisˉkey) {
    await Requireˉordinaryˉdirectory(Checkpoint, 'emission checkpoint');
    const Evidence = await Fileˉevidence(
        path.join(Checkpoint, 'Product.wvb'),
        'cached WVB',
        MAXIMUM_WVB_BYTES,
    );
    const Actual = await Readˉbounded(
        path.join(Checkpoint, 'Checkpoint.txt'),
        'emission checkpoint manifest',
        MAXIMUM_MANIFEST_BYTES,
    );
    if (!Actual.equals(Buffer.from(
        Emissionˉmanifest(Key, Analysisˉkey, Evidence),
        'ascii',
    ))) {
        Reject('The emission checkpoint manifest is invalid.');
    }
}

async function Analysisˉevidence(Directory) {
    const Source = await Fileˉevidence(
        path.join(Directory, 'Source.wvss'),
        'WVSS analysis value',
        MAXIMUM_VALUE_BYTES,
    );
    const Manifest = await Fileˉevidence(
        path.join(Directory, 'Manifest.wvca'),
        'WVCA analysis value',
        104,
    );
    if (Manifest.bytes !== 104) {
        Reject('The WVCA analysis value is not exactly 104 bytes.');
    }
    return {
        Source,
        Manifest,
        Bindings: await Fileˉevidence(
            path.join(Directory, 'Bindings.wvlb'),
            'WVLB analysis value',
            MAXIMUM_VALUE_BYTES,
        ),
        Wir: await Fileˉevidence(
            path.join(Directory, 'Wir.wvir'),
            'WVIR analysis value',
            MAXIMUM_VALUE_BYTES,
        ),
    };
}

function Analysisˉmanifest(Key, Evidence) {
    return `windvale-project-analysis-checkpoint 1\n` +
        `key ${Key}\n` +
        Evidenceˉline('source', Evidence.Source) +
        Evidenceˉline('manifest', Evidence.Manifest) +
        Evidenceˉline('bindings', Evidence.Bindings) +
        Evidenceˉline('wir', Evidence.Wir);
}

function Emissionˉmanifest(Key, Analysisˉkey, Evidence) {
    return `windvale-project-split-wvb-checkpoint 1\n` +
        `key ${Key}\n` +
        `analysis-key ${Analysisˉkey}\n` +
        Evidenceˉline('wvb', Evidence);
}

function Evidenceˉline(Name, Evidence) {
    return `${Name}-bytes ${Evidence.bytes}\n` +
        `${Name}-sha256 ${Evidence.sha256}\n`;
}

async function Prepareˉcacheˉroot() {
    let Candidate = process.env.WINDVALE_NATIVE_CACHE_ROOT;
    if (Candidate === undefined || Candidate.length === 0) {
        if (process.platform === 'win32') {
            const Local = process.env.LOCALAPPDATA;
            if (Local === undefined || Local.length === 0) {
                Reject('LOCALAPPDATA is unavailable for the native cache.');
            }
            Candidate = path.join(Local, 'Windvale', 'Native-Tool-Cache');
        } else {
            const Base = process.env.XDG_CACHE_HOME ?? path.join(os.homedir(), '.cache');
            Candidate = path.join(Base, 'windvale', 'native-tool-cache');
        }
    }
    const Resolved = path.resolve(Candidate);
    await mkdir(Resolved, { recursive: true });
    await Requireˉordinaryˉdirectory(Resolved, 'cache root');
    return Resolved;
}

async function Prepareˉfamily(Root, Name) {
    const Product = path.join(Root, Name);
    await mkdir(Product, { recursive: true });
    await Requireˉordinaryˉdirectory(Product, 'cache product');
    const Family = path.join(Product, HOST);
    await mkdir(Family, { recursive: true });
    await Requireˉordinaryˉdirectory(Family, 'cache family');
    return Family;
}

async function Allocateˉtemporary(Family, Key) {
    const Suffix = randomBytes(8).toString('hex');
    return mkdtemp(path.join(Family, `.new-${Key}-${Suffix}-`));
}

async function Removeˉtemporary(Family, Key, Candidate) {
    if (Candidate === '') {
        return;
    }
    const Resolved = path.resolve(Candidate);
    const Relative = path.relative(Family, Resolved);
    if (path.dirname(Resolved) !== path.resolve(Family) ||
        Relative.startsWith('..') || path.isAbsolute(Relative) ||
        !path.basename(Resolved).startsWith(`.new-${Key}-`)) {
        Reject('Refusing to remove an unexpected cache temporary directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}

async function Writeˉcheckpoint(Directory, Text) {
    const Destination = path.join(Directory, 'Checkpoint.txt');
    await writeFile(Destination, Text, { encoding: 'ascii', flag: 'wx' });
    const Entries = [
        'Source.wvss',
        'Manifest.wvca',
        'Bindings.wvlb',
        'Wir.wvir',
        'Product.wvb',
        'Checkpoint.txt',
    ];
    for (const Entry of Entries) {
        const Candidate = path.join(Directory, Entry);
        if (await Exists(Candidate)) {
            await Syncˉfile(Candidate);
        }
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

async function Run(Command, Arguments, Step) {
    console.log(`split project producer step=${Step} status=Started`);
    const Result = await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: path.dirname(Projectˉpath),
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Started = Date.now();
        let Stdout = Buffer.alloc(0);
        let Stderr = Buffer.alloc(0);
        let Settled = false;
        const Finish = Value => {
            if (Settled) return;
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            Resolve(Value);
        };
        const Fail = Error => {
            if (Settled) return;
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            Rejectˉpromise(Error);
        };
        const Append = (Current, Chunk) => {
            if (Current.length + Chunk.length > MAXIMUM_DIAGNOSTIC_BYTES) {
                Child.kill();
                Fail(new Error(
                    `The split compiler ${Step} diagnostics exceed 64 KiB.`,
                ));
                return Current;
            }
            return Buffer.concat([Current, Chunk]);
        };
        Child.stdout.on('data', Chunk => { Stdout = Append(Stdout, Chunk); });
        Child.stderr.on('data', Chunk => { Stderr = Append(Stderr, Chunk); });
        const Progress = setInterval(() => {
            console.log(
                `split project producer step=${Step} status=Active ` +
                `elapsed-seconds=${Math.floor((Date.now() - Started) / 1_000)}`,
            );
        }, 30_000);
        const Timeout = setTimeout(() => {
            Child.kill();
            Finish({ status: null, stdout: Stdout, stderr: Stderr, timeout: true });
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        Child.on('error', Fail);
        Child.on('close', Status => Finish({
            status: Status,
            stdout: Stdout,
            stderr: Stderr,
            timeout: false,
        }));
    });
    if (Result.stdout.length !== 0) {
        process.stdout.write(Result.stdout);
    }
    if (Result.timeout) {
        Reject(`The split compiler ${Step} producer exceeded five minutes.`);
    }
    if (Result.status !== 0) {
        if (Result.stderr.length !== 0) {
            process.stderr.write(Result.stderr);
        }
        Reject(
            `The split compiler ${Step} producer exited with status ${Result.status}.`,
        );
    }
    if (Result.stderr.length !== 0) {
        Reject(`The split compiler ${Step} producer wrote diagnostics after success.`);
    }
    console.log(`split project producer step=${Step} status=Complete`);
}

async function Fileˉevidence(Candidate, Label, Maximum) {
    const Information = await Requireˉordinaryˉfile(Candidate, Label, Maximum);
    const Hash = createHash('sha256');
    let Measured = 0;
    for await (const Chunk of createReadStream(Candidate, {
        highWaterMark: 1_048_576,
    })) {
        Measured += Chunk.length;
        if (Measured > Information.size) {
            Reject(`The ${Label} grew while it was hashed.`);
        }
        Hash.update(Chunk);
    }
    if (Measured !== Information.size) {
        Reject(`The ${Label} changed while it was hashed.`);
    }
    return { bytes: Measured, sha256: Hash.digest('hex') };
}

async function Readˉproducerˉidentity(Candidate, Role) {
    const Bytes = await Readˉbounded(
        Candidate,
        `${Role} producer identity`,
        1_024,
    );
    const Text = Bytes.toString('ascii');
    const Lines = Text.split('\n');
    const Target = Role === 'analyzer'
        ? 'source-analysis-v1'
        : 'portable-wvb-optimized-v1';
    if (Lines.length !== 7 || Lines[0] !==
            'windvale-split-compiler-producer 2' ||
        Lines[1] !== `role ${Role}` ||
        Lines[2] !== `target ${Target}` ||
        Lines[3] !== `host ${HOST}` ||
        !/^bytes [1-9][0-9]*$/u.test(Lines[4]) ||
        !/^sha256 [0-9a-f]{64}$/u.test(Lines[5]) || Lines[6] !== '') {
        Reject(`The ${Role} producer identity is invalid.`);
    }
    const Expectedˉbytes = Number(Lines[4].slice('bytes '.length));
    if (!Number.isSafeInteger(Expectedˉbytes) || Expectedˉbytes > 134_217_728) {
        Reject(`The ${Role} producer identity byte length is invalid.`);
    }
    return {
        bytes: Expectedˉbytes,
        sha256: Lines[5].slice('sha256 '.length),
        role: Role,
        text: Text,
    };
}

async function Requireˉproducerˉidentityˉunchanged(Candidate, Identity) {
    const Actual = await Readˉbounded(
        Candidate,
        `${Identity.role} producer identity`,
        1_024,
    );
    if (!Actual.equals(Buffer.from(Identity.text, 'ascii'))) {
        Reject(`The ${Identity.role} producer identity changed during the request.`);
    }
}

async function Verifyˉproducer(Candidate, Identity) {
    const Evidence = await Fileˉevidence(
        Candidate,
        `${Identity.role} producer`,
        134_217_728,
    );
    if (Evidence.bytes !== Identity.bytes ||
        Evidence.sha256 !== Identity.sha256) {
        Reject(`The ${Identity.role} producer does not match its identity.`);
    }
}

async function Requireˉordinaryˉfile(Candidate, Label, Maximum) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} must use its canonical non-link path: ${Candidate}`);
    }
    return Information;
}

async function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isDirectory() ||
        Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} must use its canonical non-link path: ${Candidate}`);
    }
}

async function Readˉbounded(Candidate, Label, Maximum) {
    await Requireˉordinaryˉfile(Candidate, Label, Maximum);
    return readFile(Candidate);
}

async function Exists(Candidate) {
    return (await stat(Candidate).catch(() => null)) !== null;
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Build-Cached-Split-Project-Wvb.mjs ' +
        '<project.wvproj> <output.wvb> <analyzer> <analyzer.identity> ' +
        '<emitter> <emitter.identity>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
