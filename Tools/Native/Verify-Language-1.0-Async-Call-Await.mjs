import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import {
    existsSync,
    lstatSync,
    mkdtempSync,
    readFileSync,
    realpathSync,
    rmSync,
} from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const MAXIMUM_PRODUCT_BYTES = 16_777_216;
const TOOL_TIMEOUT_MILLISECONDS = 300_000;

if (process.argv.length !== 9) {
    process.stderr.write(
        'Usage: node Verify-Language-1.0-Async-Call-Await.mjs ' +
        '<admitter> <analyzer> <emitter> <source-lock> ' +
        '<source-lock-sha256> <source-profile> <scratch-directory>\n',
    );
    process.exit(64);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Admitter = path.resolve(process.argv[2]);
const Analyzer = path.resolve(process.argv[3]);
const Emitter = path.resolve(process.argv[4]);
const Sourceˉlock = path.resolve(process.argv[5]);
const Sourceˉlockˉsha256 = process.argv[6];
const Sourceˉprofile = path.resolve(process.argv[7]);
const Scratch = path.resolve(process.argv[8]);
const Splitˉcompiler = path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs');

if (!/^[0-9a-f]{64}$/u.test(Sourceˉlockˉsha256)) {
    Reject('The source-lock digest is invalid.');
}
for (const [Candidate, Label, Maximum] of [
    [Admitter, 'admitter', 134_217_728],
    [Analyzer, 'analyzer', 134_217_728],
    [Emitter, 'emitter', 134_217_728],
    [Sourceˉlock, 'source lock', 4_194_304],
    [Sourceˉprofile, 'source profile', 4_194_304],
    [Splitˉcompiler, 'split compiler', 1_048_576],
]) {
    Requireˉordinaryˉfile(Candidate, Maximum, Label);
}
Requireˉordinaryˉdirectory(Scratch, 'scratch directory');

const Accepted = [
    {
        Name: 'direct',
        Fixture: 'Async-Direct-Call-Executable.wv',
        Bytes: 308,
        Sha256: '744b2e3a972ce25602a5df4949877eeefeafac37c9ef4b7e2a880118c8f59d57',
        Report:
            'source admission status=Published modules=1 source-bytes=346\n' +
            'source analysis status=Published source-bytes=346 manifest-bytes=104 ' +
            'binding-bytes=112 wir-bytes=300\n' +
            'source emission status=Published mode=optimized functions=2 ' +
            'code-bytes=47 module-bytes=308\n',
    },
    {
        Name: 'direct-aggregate',
        Fixture: 'Async-Direct-Aggregate-Call-Executable.wv',
        Bytes: 418,
        Sha256: 'cfd25dd95ed66d936f00d20f34472c544d28ff7d79aa2b30c947177a2152ee8e',
        Report:
            'source admission status=Published modules=1 source-bytes=443\n' +
            'source analysis status=Published source-bytes=443 manifest-bytes=104 ' +
            'binding-bytes=156 wir-bytes=484\n' +
            'source emission status=Published mode=optimized functions=2 ' +
            'code-bytes=97 module-bytes=418\n',
    },
    {
        Name: 'indirect',
        Fixture: 'Async-Indirect-Call-Executable.wv',
        Bytes: 398,
        Sha256: 'b2f3adc1944bd316839b7ba50d5bc71a42b506ae001ab2c8c65b4e550a34e26b',
        Report:
            'source admission status=Published modules=1 source-bytes=426\n' +
            'source analysis status=Published source-bytes=426 manifest-bytes=104 ' +
            'binding-bytes=268 wir-bytes=464\n' +
            'source emission status=Published mode=optimized functions=2 ' +
            'code-bytes=90 module-bytes=398\n',
    },
];

const Rejected = [
    {
        Name: 'direct-missing-await',
        Fixture: 'Async-Direct-Call-Missing-Await.wv',
        Diagnostic:
            'source analysis status=Sourceˉwir symbol-status=Valid ' +
            'binding-status=Valid wir-status=Invalidˉtask failure-module=0 ' +
            'related-module=0 function=1 offset=287 line=12 column=12\n',
    },
    {
        Name: 'indirect-missing-await',
        Fixture: 'Async-Indirect-Call-Missing-Await.wv',
        Diagnostic:
            'source analysis status=Sourceˉwir symbol-status=Valid ' +
            'binding-status=Valid wir-status=Invalidˉtask failure-module=0 ' +
            'related-module=0 function=0 offset=374 line=12 column=12\n',
    },
    {
        Name: 'sync-call-awaited',
        Fixture: 'Sync-Call-Awaited.wv',
        Diagnostic:
            'source analysis status=Sourceˉwir symbol-status=Valid ' +
            'binding-status=Valid wir-status=Invalidˉtask failure-module=0 ' +
            'related-module=0 function=1 offset=249 line=12 column=18\n',
    },
    {
        Name: 'sync-caller-awaits-async',
        Fixture: 'Sync-Caller-Awaits-Async.wv',
        Diagnostic:
            'source analysis status=Sourceˉwir symbol-status=Valid ' +
            'binding-status=Valid wir-status=Invalidˉtask failure-module=0 ' +
            'related-module=0 function=1 offset=279 line=12 column=18\n',
    },
];

const Work = mkdtempSync(path.join(Scratch, 'async-call-await-conformance-'));
const Evidence = [];
let Passed = false;
try {
    let Item = 0;
    for (const Case of Accepted) {
        Item += 1;
        process.stdout.write(
            `START async call await conformance item=${Item}/7 ` +
            `case=${Case.Name} expected=accepted\n`,
        );
        const Output = path.join(Work, `${Case.Name}.wvb`);
        const Result = Compile(Case.Fixture, Output);
        Requireˉresult(Result, 0, Case.Report, '', `${Case.Name} compilation`);
        Requireˉordinaryˉfile(Output, MAXIMUM_PRODUCT_BYTES, `${Case.Name} product`);
        const Product = readFileSync(Output);
        const Sha256 = Digest(Product);
        if (Product.length !== Case.Bytes || Sha256 !== Case.Sha256) {
            Reject(
                `The ${Case.Name} product identity differs: ` +
                `bytes=${Product.length} sha256=${Sha256}.`,
            );
        }
        Evidence.push(`${Case.Name}:accepted:${Product.length}:${Sha256}`);
        process.stdout.write(
            `PASS  async call await conformance item=${Item}/7 ` +
            `case=${Case.Name} bytes=${Product.length}\n`,
        );
    }
    for (const Case of Rejected) {
        Item += 1;
        process.stdout.write(
            `START async call await conformance item=${Item}/7 ` +
            `case=${Case.Name} expected=rejected\n`,
        );
        const Output = path.join(Work, `${Case.Name}.wvb`);
        const Result = Compile(Case.Fixture, Output);
        Requireˉresult(
            Result, 1, '', Case.Diagnostic, `${Case.Name} rejection`,
        );
        if (existsSync(Output)) {
            Reject(`The ${Case.Name} rejection published a WVB.`);
        }
        Evidence.push(`${Case.Name}:rejected:${Case.Diagnostic.trimEnd()}`);
        process.stdout.write(
            `PASS  async call await conformance item=${Item}/7 ` +
            `case=${Case.Name}\n`,
        );
    }
    Passed = true;
} finally {
    const Resolved = path.resolve(Work);
    if (path.dirname(Resolved) !== path.resolve(Scratch) ||
        !path.basename(Resolved).startsWith('async-call-await-conformance-')) {
        Reject(`Refusing to remove unexpected work directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

if (Passed) {
    process.stdout.write(
        'async call await conformance status=Passed cases=7 accepted=3 ' +
        `rejected=4 evidence-sha256=${Digest(Buffer.from(Evidence.join('\n')))}\n`,
    );
}

function Compile(Fixture, Output) {
    return spawnSync(process.execPath, [
        Splitˉcompiler,
        Admitter, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, Sourceˉlockˉsha256,
        '--source-profile', Sourceˉprofile,
        path.join(
            Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0', Fixture,
        ),
        Output,
    ], {
        encoding: 'utf8',
        windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: TOOL_TIMEOUT_MILLISECONDS,
    });
}

function Requireˉresult(Result, Status, Stdout, Stderr, Label) {
    if (Result.error !== undefined || Result.status !== Status ||
        Result.stdout !== Stdout || Result.stderr !== Stderr) {
        Reject(
            `The ${Label} result differs: status=${Result.status} ` +
            `error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout ?? ''}stderr=${Result.stderr ?? ''}`,
        );
    }
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isDirectory() || Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}.`);
    }
}

function Digest(Value) {
    return createHash('sha256').update(Value).digest('hex');
}

function Reject(Message) {
    throw new Error(Message);
}
