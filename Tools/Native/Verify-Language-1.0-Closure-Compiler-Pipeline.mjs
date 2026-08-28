import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    lstat,
    mkdtemp,
    readFile,
    realpath,
    rm,
} from 'node:fs/promises';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_WVB_BYTES = 16_777_216;
const PRODUCER_TIMEOUT_MILLISECONDS = 300_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const TEMPORARY_PREFIX = 'closure-compiler-pipeline-';

if (process.argv.length !== 11) {
    Reject(
        'Usage: node Verify-Language-1.0-Closure-Compiler-Pipeline.mjs ' +
        '<admitter> <analyzer> <emitter> <verifier> <runner> ' +
        '<source-lock> <source-lock-sha256> <source-profile> <scratch-directory>',
    );
}

const Admitter = resolve(process.argv[2]);
const Analyzer = resolve(process.argv[3]);
const Emitter = resolve(process.argv[4]);
const Verifier = resolve(process.argv[5]);
const Runner = resolve(process.argv[6]);
const Sourceˉlock = resolve(process.argv[7]);
const Sourceˉlockˉsha256 = process.argv[8];
const Sourceˉprofile = resolve(process.argv[9]);
const Scratch = resolve(process.argv[10]);
const Splitˉcompiler = join(SCRIPT_DIRECTORY, 'Run-Split-Compiler.mjs');

if (!/^[0-9a-f]{64}$/u.test(Sourceˉlockˉsha256)) {
    Reject('The source-lock digest is invalid.');
}
for (const [Candidate, Label, Maximum] of [
    [Admitter, 'admitter', 134_217_728],
    [Analyzer, 'analyzer', 134_217_728],
    [Emitter, 'emitter', 134_217_728],
    [Verifier, 'verifier', 134_217_728],
    [Runner, 'runner', 134_217_728],
    [Sourceˉlock, 'source lock', 65_536],
    [Sourceˉprofile, 'source profile', 65_536],
    [Splitˉcompiler, 'split compiler driver', 1_048_576],
]) {
    await Requireˉordinaryˉfile(Candidate, Label, Maximum);
}
await Requireˉordinaryˉdirectory(Scratch, 'scratch directory');

const Accepted = [
    {
        Name: 'copy',
        Source: 'Closure-Copy-Main-Pipeline.wv',
        Bytes: 451,
        Sha256: '8000144daaab85c10698e6205729f7de6798f866f69ed32861cf1e2c8daafc03',
        Compileˉreport:
            'source admission status=Published modules=1 source-bytes=354\n' +
            'source analysis status=Published source-bytes=354 manifest-bytes=104 ' +
            'binding-bytes=304 wir-bytes=644\n' +
            'source emission status=Published mode=optimized functions=2 ' +
            'code-bytes=151 module-bytes=451\n',
    },
    {
        Name: 'move',
        Source: 'Closure-Move-Main-Pipeline.wv',
        Bytes: 451,
        Sha256: 'b95e5bd8e20584f73f55f34ff9de0e5a9fe03ab9118bf48adba70b1078a17cca',
        Compileˉreport:
            'source admission status=Published modules=1 source-bytes=354\n' +
            'source analysis status=Published source-bytes=354 manifest-bytes=104 ' +
            'binding-bytes=304 wir-bytes=644\n' +
            'source emission status=Published mode=optimized functions=2 ' +
            'code-bytes=151 module-bytes=451\n',
    },
    {
        Name: 'borrow',
        Source: 'Closure-Borrow-Main-Pipeline.wv',
        Bytes: 453,
        Sha256: 'd8c6632dc52a8337af4fac4711a09c8fd4089174351f278fe8debfc51304f7dd',
        Compileˉreport:
            'source admission status=Published modules=1 source-bytes=358\n' +
            'source analysis status=Published source-bytes=358 manifest-bytes=104 ' +
            'binding-bytes=304 wir-bytes=644\n' +
            'source emission status=Published mode=optimized functions=2 ' +
            'code-bytes=151 module-bytes=453\n',
    },
];
const Rejected = [
    {
        Name: 'use-after-move',
        Source: 'Closure-Move-Use-After-Move.wv',
        Diagnostic:
            'source emission status=Invalidˉanalysis analysis-status=Invalidˉwir ' +
            'wvb-status=Sourceˉwir function=0 operation=0 source-line=0\n',
    },
    {
        Name: 'mutable-borrow',
        Source: 'Closure-Borrow-Mutable.wv',
        Diagnostic:
            'source analysis status=Sourceˉwir symbol-status=Valid ' +
            'binding-status=Valid wir-status=Invalidˉcallable failure-module=0 ' +
            'related-module=0 function=0 offset=202 line=9 column=20\n',
    },
];

const Work = await mkdtemp(join(Scratch, TEMPORARY_PREFIX));
const Evidence = [];
let Passed = false;
try {
    let Item = 0;
    for (const Case of Accepted) {
        Item += 1;
        process.stdout.write(
            `START language 1 closure compiler pipeline item=${Item}/5 ` +
            `case=${Case.Name} expected=accepted\n`,
        );
        const Product = join(Work, `${Case.Name}.wvb`);
        const Compile = await Runˉsplit(Case.Source, Product);
        Requireˉresult(Compile, 0, Case.Compileˉreport, '', `${Case.Name} compilation`);
        const Identity = await Fileˉidentity(Product, `${Case.Name} WVB`);
        if (Identity.Bytes !== Case.Bytes || Identity.Sha256 !== Case.Sha256) {
            Reject(`The ${Case.Name} WVB identity differs.`);
        }
        const Verification = await Runˉbounded(Verifier, [Product]);
        Requireˉresult(
            Verification,
            0,
            'wvb status=Valid profile=compiler-aligned\n',
            '',
            `${Case.Name} verification`,
        );
        const Execution = await Runˉbounded(Runner, [Product, '--report-steps']);
        Requireˉresult(
            Execution,
            0,
            'Result: 42\nInstructions: 31\n',
            '',
            `${Case.Name} execution`,
        );
        Evidence.push(`${Case.Name}:${Identity.Bytes}:${Identity.Sha256}:31`);
        process.stdout.write(
            `PASS  language 1 closure compiler pipeline item=${Item}/5 ` +
            `case=${Case.Name} result=42 instructions=31 wvb-bytes=${Identity.Bytes}\n`,
        );
    }
    for (const Case of Rejected) {
        Item += 1;
        process.stdout.write(
            `START language 1 closure compiler pipeline item=${Item}/5 ` +
            `case=${Case.Name} expected=rejected\n`,
        );
        const Product = join(Work, `${Case.Name}.wvb`);
        const Compile = await Runˉsplit(Case.Source, Product);
        Requireˉresult(Compile, 1, '', Case.Diagnostic, `${Case.Name} rejection`);
        if (await Exists(Product)) {
            Reject(`The ${Case.Name} rejection published a WVB.`);
        }
        Evidence.push(`${Case.Name}:rejected:${Case.Diagnostic.trimEnd()}`);
        process.stdout.write(
            `PASS  language 1 closure compiler pipeline item=${Item}/5 ` +
            `case=${Case.Name} result=rejected\n`,
        );
    }
    Passed = true;
} finally {
    await Removeˉwork(Work, Scratch);
}

if (Passed) {
    const Digest = createHash('sha256').update(Evidence.join('\n')).digest('hex');
    process.stdout.write(
        'native language 1 closure compiler pipeline status=Passed ' +
        `cases=5 accepted=3 rejected=2 result=42 instructions=93 ` +
        `wvb-bytes=1355 evidence-sha256=${Digest}\n`,
    );
}

async function Runˉsplit(Sourceˉname, Product) {
    const Source = join(
        REPOSITORY_ROOT,
        'Tests',
        'Fixtures',
        'Language-1.0',
        Sourceˉname,
    );
    await Requireˉordinaryˉfile(Source, `${Sourceˉname} source`, 1_048_576);
    return Runˉbounded(process.execPath, [
        Splitˉcompiler,
        Admitter,
        Analyzer,
        Emitter,
        '--source-input-lock',
        Sourceˉlock,
        Sourceˉlockˉsha256,
        '--source-profile',
        Sourceˉprofile,
        Source,
        Product,
    ]);
}

function Requireˉresult(Result, Status, Stdout, Stderr, Label) {
    if (Result.Exceeded) {
        Reject(`The ${Label} exceeded the diagnostic limit.`);
    }
    if (Result.Status !== Status ||
        Result.Stdout.toString('utf8').replaceAll('\r\n', '\n') !== Stdout ||
        Result.Stderr.toString('utf8').replaceAll('\r\n', '\n') !== Stderr) {
        Reject(
            `The ${Label} result differs (exit ${Result.Status}).\n` +
            Result.Stdout.toString('utf8') + Result.Stderr.toString('utf8'),
        );
    }
}

async function Runˉbounded(Command, Arguments) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: REPOSITORY_ROOT,
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        let Stdout = Buffer.alloc(0);
        let Stderr = Buffer.alloc(0);
        let Exceeded = false;
        let Settled = false;
        let Timeout = null;
        const Finish = Status => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timeout);
            Resolveˉresult({ Status, Stdout, Stderr, Exceeded });
        };
        const Append = (Current, Chunk) => {
            if (Current.length + Chunk.length > MAXIMUM_DIAGNOSTIC_BYTES) {
                Exceeded = true;
                Child.kill();
                return Current;
            }
            return Buffer.concat([Current, Chunk]);
        };
        Child.stdout.on('data', Chunk => { Stdout = Append(Stdout, Chunk); });
        Child.stderr.on('data', Chunk => { Stderr = Append(Stderr, Chunk); });
        Child.once('error', Error => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timeout);
            Rejectˉpromise(Error);
        });
        Child.once('close', Finish);
        Timeout = setTimeout(() => {
            Child.kill();
            Exceeded = true;
        }, PRODUCER_TIMEOUT_MILLISECONDS);
    });
}

async function Fileˉidentity(Candidate, Label) {
    const Information = await Requireˉordinaryˉfile(
        Candidate,
        Label,
        MAXIMUM_WVB_BYTES,
    );
    const Content = await readFile(Candidate);
    if (Content.length !== Information.size) {
        Reject(`The ${Label} changed while it was read.`);
    }
    return {
        Bytes: Content.length,
        Sha256: createHash('sha256').update(Content).digest('hex'),
    };
}

async function Requireˉordinaryˉfile(Candidate, Label, Maximum) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file.`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, resolve(Candidate))) {
        Reject(`The ${Label} path is not canonical.`);
    }
    return Information;
}

async function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isDirectory() ||
        Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory.`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, resolve(Candidate))) {
        Reject(`The ${Label} path is not canonical.`);
    }
}

async function Exists(Candidate) {
    return (await lstat(Candidate).catch(() => null)) !== null;
}

async function Removeˉwork(Work, Scratchˉdirectory) {
    const Realˉscratch = await realpath(Scratchˉdirectory);
    const Realˉparent = await realpath(dirname(Work));
    if (!Sameˉpath(Realˉparent, Realˉscratch) ||
        !basename(Work).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected closure-pipeline directory.');
    }
    await rm(Work, { recursive: true, force: true });
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}
