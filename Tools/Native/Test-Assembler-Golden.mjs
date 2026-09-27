import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
import { mkdtemp, readFile, rm, stat } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';

const DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = join(DIRECTORY, '..', '..');
const EXTENSION = process.platform === 'win32' ? '.cmd' : '.sh';
const CASES = [
    {
        "Name": "hello-object",
        "Input": "Examples/Assembler/Hello-Object.wva",
        "InputDigest": "a88f748ba87df1a291752ee8bda896279edd8d9f8a7811692c2229bbaba8cea0",
        "Bytes": 218,
        "Digest": "992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85",
        "Report": "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1"
    },
    {
        "Name": "expanded-x64",
        "Input": "Examples/Assembler/Expanded-X64.wva",
        "InputDigest": "27a324b5c26c1e6a982c6f02b0a157ccfdcbb7500521dd8c95a381aa2ed20646",
        "Bytes": 238,
        "Digest": "678551e9936ca1c901e2dc5ec129d2add73427edb1ea3d086bb4badbf1b6e4ad",
        "Report": "assembly status=valid object-bytes=238 sections=2 symbols=2 relocations=1 offset=740 line=35 column=1"
    },
    {
        "Name": "scalar-x64",
        "Input": "Examples/Assembler/Scalar-X64.wva",
        "InputDigest": "e76cb94b82857e097e734f6bdf01b3383487fd8a69f05214d74a1b69e261ae0e",
        "Bytes": 199,
        "Digest": "e1cce07329b6183ebae26ebe252be7d2e754c4aeea08ffe6452c74d60d6ea64a",
        "Report": "assembly status=valid object-bytes=199 sections=1 symbols=1 relocations=0 offset=639 line=29 column=1"
    },
    {
        "Name": "typed-scalar-x64",
        "Input": "Examples/Assembler/Typed-Scalar-X64.wva",
        "InputDigest": "a66a36a06ac6375da7ed5287fe6fdae55901f5b8b236c3098723e7a6f856a4ef",
        "Bytes": 396,
        "Digest": "860680074517025c69a2a6edf1dd9ff196475e05f9c50f95b53480c848c650c5",
        "Report": "assembly status=valid object-bytes=396 sections=2 symbols=2 relocations=5 offset=942 line=52 column=1"
    }
];
const DEADLINE = Date.now() + 60_000;

async function Requireˉidentity(Path, Bytes, Digest) {
    assert.equal((await stat(Path)).size, Bytes, 'Object byte length differs.');
    const Content = await readFile(Path);
    assert.equal(createHash('sha256').update(Content).digest('hex'), Digest,
        'Object identity differs.');
    return Content;
}

async function Run(Tool, Arguments) {
    const Result = await Runˉdevelopmentˉcommand(
        join(DIRECTORY, Tool + EXTENSION), Arguments, DEADLINE);
    assert.equal(Result.Code, 0, `${Tool} failed: ${Result.Error}`);
    assert.equal(Result.Error, '', `${Tool} wrote a diagnostic.`);
    return process.platform === 'win32' ? Result.Output.replaceAll('\r\n', '\n') : Result.Output;
}

async function Main() {
    if (process.argv.length !== 2) {
        process.stderr.write('Usage: Test-Assembler-Golden\n');
        process.exitCode = 64;
        return;
    }
    assert.ok(['win32', 'linux'].includes(process.platform), 'Unsupported host.');
    const Work = await mkdtemp(join(tmpdir(), 'windvale-assembler-golden-'));
    const First = join(Work, 'First.wvo');
    const Second = join(Work, 'Second.wvo');
    var Passed = 0;
    var Cleanupˉuncertain = false;
    try {
        for (const Case of CASES) {
            process.stdout.write(`START assembler-golden item=${Passed + 1}/${CASES.length} case=${Case.Name}\n`);
            const Input = join(REPOSITORY, Case.Input);
            assert.ok((await stat(Input)).size <= 65_536, 'WVA input exceeds its bound.');
            assert.equal(createHash('sha256').update(await readFile(Input)).digest('hex'),
                Case.InputDigest, `${Case.Name}: WVA input identity differs.`);
            const Expected = `wvasm 1\n${Case.Report}\n`;
            const Firstˉreport = await Run('Assemble-Wva', [Input, First]);
            assert.equal(Firstˉreport, Expected, 'First assembly report differs.');
            const Firstˉbytes = await Requireˉidentity(First, Case.Bytes, Case.Digest);
            await Run('Verify-Wvo', [First]);
            const Secondˉreport = await Run('Assemble-Wva', [Input, Second]);
            assert.equal(Secondˉreport, Expected, 'Repeated assembly report differs.');
            assert.deepEqual(await Requireˉidentity(Second, Case.Bytes, Case.Digest),
                Firstˉbytes, 'Repeated native object differs.');
            await rm(First);
            await rm(Second);
            Passed += 1;
            process.stdout.write(`PASS  ${Case.Name}\n`);
        }
    } catch (Error) {
        Cleanupˉuncertain = Error.cleanupUncertain === true;
        throw Error;
    } finally {
        // Preserve the private directory if a child may still be writing to it.
        if (!Cleanupˉuncertain) await rm(Work, { recursive: true, force: true });
        else process.stderr.write(`Preserved temporary work: ${Work}\n`);
    }
    process.stdout.write(`Tests: ${CASES.length}, Passed: ${Passed}, Failed: 0\n`);
}

try { await Main(); }
catch (Error) {
    process.stderr.write(`FAIL assembler-golden: ${String(Error.message).slice(0, 4096)}\n`);
    process.exitCode = Error.exitCode ?? 1;
}
