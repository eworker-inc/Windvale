import assert from 'node:assert/strict';
import { constants } from 'node:fs';
import { copyFile, lstat, mkdir, mkdtemp, open, opendir, readFile, readdir, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Parseˉcurrentˉverification } from './Verify-Wvb.mjs';
import { Withˉcurrentˉverification } from './Current-Wvb-Verification-Batch-Core.mjs';
import { Readˉbootstrapˉverifier, Validateˉsegmentedˉhostedˉcheckpoint } from './Build-Cached-Segmented-Hosted-Wvb.mjs';
import {
    Acquireˉcurrentˉsplitˉcompiler, Getˉcurrentˉsplitˉcompilerˉfamily, Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';

const NATIVE = dirname(fileURLToPath(import.meta.url));
const REPOSITORY = resolve(NATIVE, '..', '..');
const TEMPORARY_PREFIX = 'windvale-current-verifier-cases-';
const CLI = join(NATIVE, 'Verify-Wvb.mjs');
const BUILD = join(NATIVE, 'Build-Current-Split-Project-Wvb.mjs');
const FIXTURE = join(REPOSITORY, 'Projects/Tests/Windvale-Native-Test-Owned-Helper-Cleanup.wvproj');
const MAXIMUM_BEHAVIOR_MILLISECONDS = 600_000;
const MAXIMUM_WARM_MILLISECONDS = process.platform === 'win32' ? 30_000 : 90_000;

async function Workˉwith(Action) {
    const Temporary = await realpath(tmpdir());
    const Work = await realpath(await mkdtemp(join(Temporary, TEMPORARY_PREFIX)));
    try { return await Action(Work); }
    finally {
        assert.equal(dirname(await realpath(Work)), Temporary);
        assert(basename(Work).startsWith(TEMPORARY_PREFIX));
        assert(!(await lstat(Work)).isSymbolicLink());
        await rm(Work, { recursive: true, force: false });
    }
}

function Remaining(Deadline, Maximum = Deadline - Date.now()) {
    const Value = Math.min(Deadline - Date.now(), Maximum);
    assert(Value > 0, 'The current verification owner deadline is exhausted.');
    return Value;
}

export async function Prepareˉcurrentˉverification(Context, Maximumˉseconds, Constructˉcompiler = true) {
    assert(Number.isInteger(Maximumˉseconds) && Maximumˉseconds >= 60 && Maximumˉseconds <= 5400);
    assert.equal(typeof Constructˉcompiler, 'boolean');
    assert.equal(process.env.WINDVALE_PREPARED_PRODUCTS_ONLY, undefined,
        'Preparation cannot override inherited prepared-product mode.');
    const Deadline = Date.now() + Maximumˉseconds * 1000;
    await Workˉwith(async Work => {
        if (Constructˉcompiler) {
            process.stdout.write('current verification preparation step=compiler-checkpoint status=Started\n');
            await Context.Success('current-verifier-compiler-preparation', process.execPath,
                [BUILD, '--prepare-only', '--deadline-ms', String(Deadline)], Remaining(Deadline), true);
        } else {
            process.stdout.write('current verification preparation step=compiler-checkpoint status=Required constructor=disabled\n');
        }
        process.stdout.write('current verification preparation step=verifier-products status=Started\n');
        await Context.Success('current-verifier-product-preparation', process.execPath,
            [CLI, '--prepare', '--deadline-ms', String(Math.min(Deadline, Date.now() + 600_000))],
            Remaining(Deadline), true);
        process.stdout.write('current verification preparation step=source-fixture status=Started\n');
        await Context.Success('current-verifier-fixture-preparation', process.execPath,
            [BUILD, '--prepared-compiler-only', '--deadline-ms', String(Deadline), FIXTURE, join(Work, 'Fixture.wvb')],
            Remaining(Deadline), true);
    });
    process.stdout.write('current verification preparation status=Complete behavior-cases=0\n');
}

export async function Runˉcurrentˉverificationˉcases(Context) {
    const Deadline = Date.now() + MAXIMUM_BEHAVIOR_MILLISECONDS;
    const Environment = { ...process.env, WINDVALE_PREPARED_PRODUCTS_ONLY: '1' };
    let Cases = 0;
    const Pass = Name => {
        Cases += 1;
        process.stdout.write(`current verification case=${Name} status=Passed item=${Cases}/23\n`);
    };
    await Workˉwith(async Work => {
        const Now = 1_000_000;
        assert.equal(Parseˉcurrentˉverification(['--current', 'Program.wvb'], Now).Deadline, Now + 120_000);
        assert.equal(Parseˉcurrentˉverification(['--prepare', '--deadline-ms', String(Now + 600_000)], Now).Prepare, true);
        for (const Arguments of [[], ['--current', 'Program.wvo'], ['--current'],
            ['--current', null], ['--prepare', '--deadline-ms', String(Now)],
            ['--prepare', '--deadline-ms', String(Now + 600_001)],
            ['--prepare', '--deadline-ms', '9007199254740992']]) {
            assert.throws(() => Parseˉcurrentˉverification(Arguments, Now), Error => Error.exitCode === 64);
        }
        Pass('argument-contract');
        const Input = join(Work, 'Current-Owned.wvb');
        await Context.Success('current-verifier-fixture-materialization', process.execPath,
            [BUILD, '--prepared-compiler-only', '--deadline-ms', String(Deadline), FIXTURE, Input],
            Remaining(Deadline), false, Environment);
        const Canonical = await readFile(Input);
        async function Invoke(Name, Arguments, Expected, Pattern, Selectedˉenvironment = Environment) {
            const Result = await Context.Run(process.execPath, [CLI, ...Arguments],
                'current-verifier-' + Name, Remaining(Deadline, 120_000), Selectedˉenvironment);
            Context.Clean(Result, Name);
            assert(!Result.timedOut && !Result.exceeded, `${Name} did not finish within its bounds.`);
            assert.equal(Result.code, Expected, `${Name}: ${Result.error.toString('utf8')}`);
            const Captured = Result.output.toString('utf8').replaceAll('\r\n', '\n');
            const Activity = /^INFO development command step=active tool=(?:node|Verifier\.(?:exe|elf))\n/gmu;
            assert([...Captured.matchAll(Activity)].length <= 4, `${Name} exceeded bounded progress.`);
            const Output = Captured.replace(Activity, '');
            const Error = Result.error.toString('utf8').replaceAll('\r\n', '\n');
            assert.match(Expected === 0 ? Output : Error, Pattern, Name);
            assert.equal(Expected === 0 ? Error : Output, '', `${Name} wrote to the wrong stream.`);
            Pass(Name);
            return Result;
        }
        const Valid = /^wvb status=Valid profile=compiler-aligned\n$/u;
        const Invalid = /^wvb status=Invalid phase=[a-z-]+(?: step=[a-z-]+)?\n$/u;
        await Invoke('current-owned-module', ['--current', Input], 0, Valid);
        const Badˉmagic = Buffer.from(Canonical); Badˉmagic[0] ^= 1;
        const Oversizedˉsection = Buffer.from(Canonical); Oversizedˉsection.writeUInt32LE(0xffff_ffff, 16);
        const Typed = Buffer.from((await readFile(join(REPOSITORY,
            'Tests/Native/Malformed-Wvb/Typed-Local-Store-Kind.wvb.b64'), 'utf8')).trim(), 'base64');
        for (const [Name, Value] of [['bad-magic', Badˉmagic], ['truncated-header', Canonical.subarray(0, 11)],
            ['oversized-section', Oversizedˉsection], ['typed-local-mismatch', Typed]]) {
            const Candidate = join(Work, Name + '.wvb');
            await writeFile(Candidate, Value, { flag: 'wx' });
            await Invoke(Name, ['--current', Candidate], 1, Invalid);
        }
        const Empty = join(Work, 'Empty.wvb');
        await writeFile(Empty, Buffer.alloc(0), { flag: 'wx' });
        await Invoke('empty-input', ['--current', Empty], 1, /not a bounded ordinary file/u);
        const Large = join(Work, 'Oversized.wvb');
        const Handle = await open(Large, 'wx');
        try { await Handle.truncate(16_777_217); } finally { await Handle.close(); }
        await Invoke('input-size-limit', ['--current', Large], 1, /not a bounded ordinary file/u);
        const Bootstrap = await Readˉbootstrapˉverifier();
        async function Retainˉbootstrap(Root) {
            if (Bootstrap === null) return;
            // A cache-miss fixture removes its selected product, while retaining
            // the explicit bootstrap dependency needed to compute that key.
            const Directory = join(Root, 'segmented-hosted-wvb-v1', Bootstrap.identity.host, Bootstrap.identity.key);
            await mkdir(Directory, { recursive: true });
            for (const Name of ['Checkpoint.txt', basename(Bootstrap.productPath)]) {
                await copyFile(join(dirname(Bootstrap.productPath), Name), join(Directory, Name), constants.COPYFILE_EXCL);
            }
            const Copied = await Validateˉsegmentedˉhostedˉcheckpoint(Directory, Bootstrap.identity.key, '8',
                { bytes: Bootstrap.identity.inputBytes, sha256: Bootstrap.identity.inputSha256 });
            assert.equal(Copied.product.bytes, Bootstrap.identity.productBytes);
            assert.equal(Copied.product.sha256, Bootstrap.identity.productSha256);
        }
        const Missing = join(Work, 'Missing-Compiler');
        await Retainˉbootstrap(Missing);
        await Invoke('missing-compiler', ['--current', Input], 64, /Current compiler checkpoint missing/u,
            { ...Environment, WINDVALE_NATIVE_CACHE_ROOT: Missing });
        const Key = await Getˉcurrentˉsplitˉcompilerˉkey();
        const Compiler = await Acquireˉcurrentˉsplitˉcompiler(await Getˉcurrentˉsplitˉcompilerˉfamily(), Key,
            () => { throw new Error('Behavior cannot construct a compiler.'); }, async () => {
                assert.equal(await Getˉcurrentˉsplitˉcompilerˉkey(), Key);
            });
        const Partial = join(Work, 'Compiler-Only');
        await Retainˉbootstrap(Partial);
        const Copy = join(Partial, 'current-split-compiler-v2', `${process.platform}-${process.arch}`, Key);
        await mkdir(Copy, { recursive: true });
        const Products = await readdir(Compiler.directory);
        assert(Products.length <= 16);
        let Total = 0;
        for (const Product of Products) {
            const Source = join(Compiler.directory, Product);
            const Information = await lstat(Source);
            assert(Information.isFile() && !Information.isSymbolicLink() && Information.size <= 67_108_864);
            Total += Information.size; assert(Total <= 134_217_728);
            await copyFile(Source, join(Copy, Product));
        }
        await Invoke('missing-source-product', ['--current', Input], 64, /Prepared split-project product missing/u,
            { ...Environment, WINDVALE_NATIVE_CACHE_ROOT: Partial });
        await Invoke('prepared-mode-forbids-preparation', ['--prepare', '--deadline-ms', String(Date.now() + 60_000)],
            64, /Verifier preparation is disabled/u);
        const Repeated = await Invoke('repeated-warm-current-verification', ['--current', Input], 0, Valid);
        assert(Repeated.elapsed <= MAXIMUM_WARM_MILLISECONDS,
            `Warm verification exceeded the named ${MAXIMUM_WARM_MILLISECONDS}-millisecond feedback bound.`);
        assert(Canonical.equals(await readFile(Input)), 'Verification changed its input.');
        process.stdout.write(`current verification warm elapsed-ms=${Repeated.elapsed} maximum-ms=${MAXIMUM_WARM_MILLISECONDS} input-bytes=${Canonical.length}\n`);
        Cases += await Runˉcurrentˉverificationˉbatchˉcases(Input, Deadline);
    });
    assert.equal(Cases, 23);
    process.stdout.write(`native current WVB verification status=Passed cases=${Cases} host=${process.platform} qualification=false\n`);
}

export async function Runˉcurrentˉverificationˉbatchˉcases(Input, Deadline) {
    let Cases = 0;
    const Pass = Name => {
        Cases++;
        process.stdout.write(`current verification batch case=${Name} status=Passed item=${Cases}/11\n`);
    };
    const Temporary = await realpath(tmpdir()), Prefix = 'windvale-current-verify-';
    async function Directories() {
        const Values = [], Directory = await opendir(Temporary); let Items = 0;
        for await (const Entry of Directory) {
            assert(++Items <= 4_096, 'Verifier batch test temporary inventory exceeds its bound.');
            if (Entry.name.startsWith(Prefix)) Values.push(Entry.name);
        }
        return Values.sort();
    }
    async function Batch(Action) {
        const Before = await Directories();
        try { return await Withˉcurrentˉverification({ Prepare: false, Deadline },
            Verifier => Action(Verifier, Before)); }
        finally { assert.deepEqual(await Directories(), Before, 'Verifier batch retained its private directory.'); }
    }
    async function Corruptˉprivateˉimage(Before) {
        const Added = (await Directories()).filter(Name => !Before.includes(Name));
        assert.equal(Added.length, 1, 'The test must own exactly one new verifier batch directory.');
        const Directory = await realpath(join(Temporary, Added[0]));
        assert.equal(dirname(Directory), Temporary);
        const Image = join(Directory, process.platform === 'win32' ? 'Verifier.exe' : 'Verifier.elf');
        const Information = await lstat(Image);
        assert(Information.isFile() && !Information.isSymbolicLink() && Information.nlink === 1);
        const Bytes = await readFile(Image); Bytes[0] ^= 1;
        await writeFile(Image, Bytes);
    }
    for (const Selection of [{ Prepare: false, Deadline: Date.now() - 1 },
        { Prepare: false, Deadline: 1.5 }, { Prepare: false, Deadline: Number.MAX_SAFE_INTEGER },
        { Prepare: 'false', Deadline }, { Prepare: true, Deadline }])
        await assert.rejects(() => Withˉcurrentˉverification(Selection, async () => {}), /Invalid current verification batch/u);
    await assert.rejects(() => Withˉcurrentˉverification({ Prepare: false, Deadline }, null), /Invalid current verification batch/u);
    Pass('invalid-selection');
    let Borrowed;
    const Value = await Batch(async Verifier => {
        Borrowed = Verifier;
        for (const Name of ['valid-admission', 'repeated-admission']) {
            const Result = await Verifier.Verify(Input);
            assert.equal(Result.Code, 0); assert.equal(Result.Error, '');
            assert.equal(Result.Output.replaceAll('\r\n', '\n'), 'wvb status=Valid profile=compiler-aligned\n'); Pass(Name);
        }
        const Invalid = join(dirname(Input), 'Batch-Malformed.wvb');
        const Bytes = await readFile(Input); Bytes[0] ^= 1; await writeFile(Invalid, Bytes, { flag: 'wx' });
        const Rejected = await Verifier.Verify(Invalid);
        assert.equal(Rejected.Code, 1); assert.equal(Rejected.Output, '');
        assert.match(Rejected.Error, /^wvb status=Invalid phase=[a-z-]+(?: step=[a-z-]+)?\r?\n$/u);
        Pass('malformed-admission');
        await assert.rejects(() => Withˉcurrentˉverification({ Prepare: false, Deadline }, async () => {}),
            /batch is already active/u);
        const Pending = Verifier.Verify(Input);
        assert.throws(() => Verifier.Verify(Input), /busy/u);
        assert.equal((await Pending).Code, 0); Pass('concurrent-use-refusal');
        return 123;
    });
    assert.equal(Value, 123);
    assert.throws(() => Borrowed.Verify(Input), /closed/u); Pass('closed-use-refusal');
    Pass('successful-batch-cleanup');
    await assert.rejects(() => Batch(async () => { throw new Error('Selected callback failure.'); }),
        /Selected callback failure/u); Pass('callback-failure-cleanup');
    await assert.rejects(() => Batch(async (Verifier, Before) => {
        await Corruptˉprivateˉimage(Before); await Verifier.Verify(Input);
    }), /Current verifier executable changed/u); Pass('executable-change-refusal');
    await assert.rejects(() => Batch(async (_Verifier, Before) => { await Corruptˉprivateˉimage(Before); }),
        /Current verifier executable changed/u); Pass('final-executable-change-refusal');
    let Outstanding;
    await assert.rejects(() => Batch(async Verifier => { Outstanding = Verifier.Verify(Input); }),
        /unfinished input/u);
    assert.equal((await Outstanding).Code, 0); Pass('unfinished-input-drained');
    assert.equal(Cases, 11);
    process.stdout.write(`current verification batch status=Passed cases=${Cases} native-admissions=5 qualification=false\n`);
    return Cases;
}
