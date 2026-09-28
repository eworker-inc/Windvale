import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Runˉownedˉstorageˉcases } from './Native-Owned-Storage-Cases.mjs';
import { Prepareˉbudgetˉoracle } from './Native-Budgeted-Storage-Cases.mjs';
import { Prepareˉownedˉvector, Runˉownedˉvectorˉcases } from './Native-Owned-Vector-Cases.mjs';
import { createHash } from 'node:crypto';
import { existsSync } from 'node:fs';
import { mkdtemp, readFile, realpath, rm, stat, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';

const WINDOWS = process.platform === 'win32';
const OUTPUT_LIMIT = 64 * 1024;
const FIXTURE_LIMIT = 4 * 1024 * 1024;
const COMMAND_TIMEOUT_MILLISECONDS = 120_000;
const CONSTRUCTION_TIMEOUT_MILLISECONDS = 15 * 60_000;

let Oracleˉproduct = null;
let Vectorˉproduct = null;
while (['--budget-oracle', '--owned-vector'].includes(process.argv.at(-3))) {
    if (!/^[0-9a-f]{64}$/u.test(process.argv.at(-1))) Usage();
    const Product = { Path: resolve(process.argv.at(-2)), Sha256: process.argv.at(-1) };
    if (process.argv.at(-3) === '--budget-oracle') {
        if (Oracleˉproduct !== null) Usage();
        Oracleˉproduct = Product;
    } else {
        if (Vectorˉproduct !== null) Usage();
        Vectorˉproduct = Product;
    }
    process.argv.splice(-3);
}

const Phase = process.argv[4];
const Prepareˉonly = Phase === '--prepare-only';
const Preparedˉonly = Phase === '--prepared-products-only';
let Ownerˉdeadline = null;
if (Prepareˉonly || Preparedˉonly) {
    const Seconds = Number(process.argv[6]);
    if (process.argv.length !== 7 || process.argv[5] !== '--maximum-seconds' ||
        !/^[1-9][0-9]*$/u.test(process.argv[6]) ||
        Seconds < 30 || Seconds > (Preparedˉonly ? 600 : 5400)) Usage();
    if (Prepareˉonly && process.env.WINDVALE_PREPARED_PRODUCTS_ONLY !== undefined) {
        Reject('Preparation cannot use a prepared-only environment.');
    }
    Ownerˉdeadline = Date.now() + Seconds * 1000;
    if (Preparedˉonly) process.env.WINDVALE_PREPARED_PRODUCTS_ONLY = '1';
    process.argv.splice(4);
}

if ((process.argv.length !== 4 && process.argv.length !== 5 && process.argv.length !== 7 && process.argv.length !== 8 && process.argv.length !== 10) ||
    !['windows', 'linux'].includes(process.argv[2])) Usage();
const Borrowˉonly = process.argv[4] === '--foundation-borrow-emission';
const Ownedˉonly = process.argv[4] === '--owned-storage';
if (Ownedˉonly) Ownerˉdeadline = Date.now() + 600_000;
const Recordˉonly = process.argv.length === 8 && process.argv[7] === '--record-return-memory';
if (process.argv.length === 8 && !Recordˉonly) Usage();
if (process.argv.length === 5 && !Borrowˉonly && !Ownedˉonly) Usage();
const Suppliedˉlowerer = process.argv.length === 7 || process.argv.length === 10 || Recordˉonly;
if (Suppliedˉlowerer && Ownerˉdeadline === null) Ownerˉdeadline = Date.now() + 600_000;
const Suppliedˉborrowˉprobe = process.argv.length === 10;
if (Suppliedˉlowerer && (process.argv[4] !== '--lowerer' ||
    !/^[0-9a-f]{64}$/u.test(process.argv[6]))) Usage();
if (Suppliedˉborrowˉprobe && (process.argv[7] !== '--borrow-probe' ||
    !/^[0-9a-f]{64}$/u.test(process.argv[9]))) Usage();
const Target = process.argv[2];
if ((WINDOWS && Target !== 'windows') || (!WINDOWS && Target !== 'linux')) {
    Reject('The native unsafe write-pointer target does not match this host.');
}
const Repositoryˉroot = await realpath(resolve(process.argv[3]));
const Extension = WINDOWS ? 'cmd' : 'sh';
const Nativeˉextension = WINDOWS ? 'exe' : 'elf';
const Build = join(Repositoryˉroot, 'Tools', 'Native', 'Build-Current-Split-Project-Wvb.mjs');
const Packageˉlowerer = join(
    Repositoryˉroot, 'Tools', 'Native',
    `Package-Segmented-Compiler-Wvb.${Extension}`,
);
const Assemble = join(
    Repositoryˉroot, 'Tools', 'Native', `Assemble-Wva.${Extension}`,
);
const Check = join(Repositoryˉroot, 'Tools', 'Native', `Check-Wvo.${Extension}`);
const Inspect = join(
    Repositoryˉroot, 'Tools', 'Native', `Inspect-Wvo.${Extension}`,
);
const Link = join(Repositoryˉroot, 'Tools', 'Native', `Link-Wvo.${Extension}`);
const Packageˉconsole = join(
    Repositoryˉroot, 'Tools', 'Native', `Package-Console.${Extension}`,
);
const Project = join(
    Repositoryˉroot, 'Projects', 'Compiler',
    'Windvale-Native-X64-Lowering-Tool.wvproj',
);
const Fixtureˉdirectory = join(
    Repositoryˉroot, 'Tests', 'Native', 'Wvb-To-Wvo-Rejections',
);
const Candidateˉdirectory = join(
    Repositoryˉroot, 'Artifacts', 'Native-Wvb-To-Wvo-Candidate',
);
const Foreignˉproviderˉsource = join(
    Repositoryˉroot, 'Runtime', 'Native',
    'Linux-X64-Paper-Buffer-Source.wva',
);

const Work = await realpath(await mkdtemp(join(
    tmpdir(),
    'windvale-write-pointer-lowering-',
)));
let Preserveˉwork = false;
try {
    const Ownedˉcases = !Prepareˉonly && !Borrowˉonly && !Recordˉonly ? await Runˉownedˉstorageˉcases({
        Repository: Repositoryˉroot, Work, Target, Requireˉsuccess, Runˉprocess, Oracleˉproduct,
    }) : 0;
    if (!Ownedˉonly) {
    await Verifyˉsourceˉclosures();
    if (!Recordˉonly) await Runˉfoundationˉborrowˉemission();
    if (!Borrowˉonly) {
    const Canonical = await Readˉfixture(
        join(Fixtureˉdirectory, 'Unsafe-Write-Pointer.wvb.b64'),
        '289f9e338f7922e91be3526239bf5e06d9d5ef701d4d87a003d7fab14adec47f',
    );
    const Runtime = await Readˉfixture(
        join(Fixtureˉdirectory, 'Unsafe-Write-Pointer-Runtime.wvb.b64'),
        '3754236b188a99068bb3918dc581e27ba4215b0590286a7d62039d6254dd54e3',
    );
    const Foreignˉsuccess = await Readˉfixture(
        join(Fixtureˉdirectory, 'Foreign-Runtime-Success.wvb.b64'),
        '339fa2a51236e55281ab0ccc0f3c0ec881d9d4074c1cf9fc8a1b943bba4ffa80',
    );
    const Foreignˉstale = await Readˉfixture(
        join(Fixtureˉdirectory, 'Foreign-Runtime-Stale.wvb.b64'),
        'cd924526e21b4f9ffb3d9701670b69455492675e526fd33fd27d558166f416f4',
    );
    const Lowererˉwvb = join(Work, 'Native-Lowerer.wvb');
    const Lowerer = Suppliedˉlowerer ? resolve(process.argv[5]) :
        join(Work, `Native-Lowerer.${Nativeˉextension}`);
    if (Suppliedˉlowerer) {
        const Metadata = await stat(Lowerer);
        if (!Metadata.isFile() || Metadata.size > 67_108_864) {
            Reject('The supplied native lowerer is not a bounded ordinary file.');
        }
        const Bytes = await readFile(Lowerer);
        if (Bytes.length > 67_108_864 ||
            createHash('sha256').update(Bytes).digest('hex') !== process.argv[6]) {
            Reject('The supplied native lowerer identity differs.');
        }
        process.stdout.write(`native unsafe write pointer lowering step=compiler-reuse sha256=${process.argv[6]}\n`);
    } else {
    process.stdout.write(
        'native unsafe write pointer lowering step=compiler-build status=Started\n',
    );
    await Requireˉsuccess(
        process.execPath, [Build, ...Buildˉoptions(), Project, Lowererˉwvb], 'compiler-build',
        Prepareˉonly ? Ownerˉdeadline - Date.now() : CONSTRUCTION_TIMEOUT_MILLISECONDS,
    );
    process.stdout.write(
        'native unsafe write pointer lowering step=compiler-package status=Started\n',
    );
    await Requireˉsuccess(
        Packageˉlowerer,
        ['7', Lowererˉwvb, Lowerer, '--development-cache'],
        'compiler-package',
        CONSTRUCTION_TIMEOUT_MILLISECONDS,
    );
    }
    if (!existsSync(Lowerer)) Reject('The current native lowerer was not published.');

    if (Prepareˉonly) {
        await Prepareˉbudgetˉoracle({ Repository: Repositoryˉroot, Work, Target, Requireˉsuccess, Oracleˉproduct });
        await Prepareˉownedˉvector({ Repository: Repositoryˉroot, Work, Requireˉsuccess, Vectorˉproduct });
        process.stdout.write('native x64 lowering preparation status=Ready products=4 behavior-cases=0\n');
    } else {
    const Recordˉcases = await Runˉrecordˉreturnˉmemory(Lowerer);
    if (!Recordˉonly) {
    const Vectorˉcases = await Runˉownedˉvectorˉcases({
        Repository: Repositoryˉroot, Work, Target, Requireˉsuccess, Runˉprocess, Vectorˉproduct,
    }, Lowerer);
    await Runˉoptionˉu64(Lowerer);
    await Runˉframeˉinitialization(Lowerer);

    await Lowerˉidentity(
        Lowerer,
        await Readˉbinary(
            join(Candidateˉdirectory, 'Return-42.wvb'), 174,
            '7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31',
        ),
        join(Work, 'Return-42'),
        'baseline-return-42',
        342, 415,
        '53f3218e2a9e19ce8e2d470267a3d73af569a3a918d2949042b6c926564ae5b3',
    );
    await Lowerˉidentity(
        Lowerer,
        await Readˉbinary(
            join(Candidateˉdirectory, 'Metadata.wvb'), 369,
            '94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa',
        ),
        join(Work, 'Metadata'),
        'metadata',
        734, 807,
        'f8264e4b56fea680d3456adc84b4a12b217a6001accf57ac1edb03133b097144',
    );

    const Foreignˉspecifications = [
        {
            Name: 'foreign-success',
            Input: Foreignˉsuccess,
            Codeˉbytes: 8_211,
            Objectˉbytes: 8_388,
            Sha256: '22f0ffe6b57d9fe84f2360b2802aab1aa3cf3622b8d4c417e84ceefcbc9d3013',
        },
        {
            Name: 'foreign-stale',
            Input: Foreignˉstale,
            Codeˉbytes: 8_298,
            Objectˉbytes: 8_475,
            Sha256: '3d94acc000169d18827a6f6a7e5f29f65bed58825ec00773192f375a1e4bc92b',
        },
    ];
    const Foreignˉobjects = [];
    for (let Index = 0; Index < Foreignˉspecifications.length; Index += 1) {
        const Specification = Foreignˉspecifications[Index];
        process.stdout.write(
            `native unsafe write pointer lowering item=${Index + 1}/` +
            `${Foreignˉspecifications.length} case=${Specification.Name} ` +
            'status=Started\n',
        );
        const Prefix = join(Work, Specification.Name);
        const Wvo = await Lowerˉidentity(
            Lowerer,
            Specification.Input,
            Prefix,
            Specification.Name,
            Specification.Codeˉbytes,
            Specification.Objectˉbytes,
            Specification.Sha256,
        );
        await Requireˉsuccess(Check, [Wvo], `${Specification.Name}-object-check`);
        const Inspected = await Requireˉsuccess(
            Inspect, [Wvo], `${Specification.Name}-object-inspect`,
        );
        if (!Inspected.Output.includes('Symbols (3)') ||
            !Inspected.Output.includes(
                '[2] wv_paper_buffer_source_read_v1 binding=Import ' +
                'kind=Function section=undefined offset=0 size=0',
            ) || !Inspected.Output.includes('Relocations (1)') ||
            !Inspected.Output.includes('symbol=2 addend=-4')) {
            Reject(`The ${Specification.Name} object shape differed.\n` +
                Inspected.Output);
        }
        Foreignˉobjects.push({ ...Specification, Wvo });
    }

    const Providerˉwvo = join(Work, 'Paper-Buffer-Source.wvo');
    const Assembled = await Requireˉsuccess(
        Assemble,
        [Foreignˉproviderˉsource, Providerˉwvo],
        'foreign-provider-assemble',
    );
    if (!/^wvasm 1\r?\nassembly status=valid object-bytes=223 sections=1 symbols=1 relocations=0 /u
        .test(Assembled.Output)) {
        Reject(`The Foreign provider assembly report differed.\n${Assembled.Output}`);
    }
    await Readˉbinary(
        Providerˉwvo,
        223,
        'b76bd5ff5b2824258e0f9931eaac6ec8c27a055bb207bdcacab4fc51f6b0f879',
    );
    await Requireˉsuccess(Check, [Providerˉwvo], 'foreign-provider-object-check');
    let Foreignˉexecutions = 0;
    for (const Specification of Foreignˉobjects) {
        const Image = join(Work, `${Specification.Name}.bin`);
        const Linked = await Requireˉsuccess(
            Link,
            ['0', 'Main', Image, Specification.Wvo, Providerˉwvo],
            `${Specification.Name}-link`,
        );
        const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
        if (Entry === null || !existsSync(Image) ||
            !Linked.Output.includes('imports count=1') ||
            !Linked.Output.includes(
                'name=wv_paper_buffer_source_read_v1 provider-input=1',
            ) || !Linked.Output.includes('relocations count=1')) {
            Reject(`The ${Specification.Name} link report differed.\n${Linked.Output}`);
        }
        if (Target === 'linux') {
            const Application = join(Work, `${Specification.Name}.elf`);
            await Requireˉsuccess(
                Packageˉconsole,
                ['linux-x64-console-v1', Image, Entry[1], Application],
                `${Specification.Name}-console-package`,
            );
            const Executed = await Runˉprocess(
                Application, [], COMMAND_TIMEOUT_MILLISECONDS,
                `${Specification.Name}-native-execution`,
            );
            if (Executed.Code !== 42 || Executed.Exceeded ||
                Executed.Timedˉout || Executed.Output !== '') {
                Reject(`The ${Specification.Name} native result differed.\n` +
                    Executed.Output);
            }
            Foreignˉexecutions += 1;
        }
    }

    const Runtimeˉwvb = join(Work, 'Runtime.wvb');
    const Runtimeˉwvo = join(Work, 'Runtime.wvo');
    await writeFile(Runtimeˉwvb, Runtime, { flag: 'wx' });
    const Lowered = await Runˉprocess(
        Lowerer, [Runtimeˉwvb, Runtimeˉwvo], COMMAND_TIMEOUT_MILLISECONDS,
        'valid-lowering',
    );
    if (!Passed(Lowered) || !existsSync(Runtimeˉwvo) ||
        !/^native x64 status=Valid abi=22 code-bytes=[0-9]+ object-bytes=[0-9]+\r?\n$/u
            .test(Lowered.Output)) {
        Reject(`The valid write-pointer lowering differed.\n${Lowered.Output}`);
    }
    await Requireˉsuccess(Check, [Runtimeˉwvo], 'object-check');
    const Image = join(Work, 'Runtime.bin');
    const Linked = await Requireˉsuccess(
        Link, ['0', 'Main', Image, Runtimeˉwvo], 'link',
    );
    const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
    if (Entry === null || !existsSync(Image)) {
        Reject(`The write-pointer link report differed.\n${Linked.Output}`);
    }
    const Application = join(Work, `Runtime.${Nativeˉextension}`);
    await Requireˉsuccess(
        Packageˉconsole,
        [WINDOWS ? 'windows-x64-console-v1' : 'linux-x64-console-v1',
            Image, Entry[1], Application],
        'console-package',
    );
    const Executed = await Runˉprocess(
        Application, [], COMMAND_TIMEOUT_MILLISECONDS, 'native-execution',
    );
    if (Executed.Code !== 42 || Executed.Exceeded || Executed.Timedˉout ||
        Executed.Output !== '') {
        Reject(`The write-pointer native result differed.\n${Executed.Output}`);
    }

    const Cases = [
        ['old-minor', Value => { Value[6] = 36; }],
        ['unknown-opcode', Value => { Value[230] = 224; }],
        ['invalid-region-local', Value => Value.writeUInt32LE(0xffff_ffff, 231)],
        ['invalid-pointer-type', Value => { Value[235] = 8; }],
        ['invalid-abi-type', Value => { Value[239] = 8; }],
        ['invalid-region-shape', Value => { Value[159] = 8; }],
        ['aliased-region-pointer-type', Value => { Value[235] = 1; }],
        ['pointer-take', Value => { Value[248] = 205; }],
        ['pointer-call-escape', Value => { Value[253] = 64; }],
        ['pointer-move-from-unavailable-local', Value => { Value[249] = 1; }],
    ];
    for (let Index = 0; Index < Cases.length; Index += 1) {
        const [Name, Mutate] = Cases[Index];
        process.stdout.write(
            `native unsafe write pointer lowering item=${Index + 1}/` +
            `${Cases.length} case=${Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Canonical);
        Mutate(Candidate);
        const Candidateˉpath = join(Work, `Malformed-${Name}.wvb`);
        const Destination = join(Work, `Malformed-${Name}.wvo`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        const Rejected = await Runˉprocess(
            Lowerer, [Candidateˉpath, Destination],
            COMMAND_TIMEOUT_MILLISECONDS, Name,
        );
        if (Rejected.Code !== 1 || Rejected.Exceeded || Rejected.Timedˉout ||
            existsSync(Destination) ||
            !/^native x64 status=(?:Invalidˉwvb|Unsupportedˉprofile|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u
                .test(Rejected.Output)) {
            Reject(`The malformed write-pointer case ${Name} differed.\n` +
                Rejected.Output);
        }
    }
    const Foreignˉlayout = Inspectˉforeignˉfixture(Foreignˉsuccess);
    const Foreignˉcases = [
        ['old-minor', Value => Value.writeUInt16LE(37, 6)],
        ['unknown-opcode', Value => {
            Value[Foreignˉlayout.Operation] = 225;
        }],
        ['unregistered-binding', Value => Value.writeUInt32LE(
            0, Foreignˉlayout.Operation + 1,
        )],
        ['invalid-pointer-type', Value => Value.writeUInt32LE(
            Foreignˉlayout.Typeˉcount, Foreignˉlayout.Operation + 5,
        )],
        ['invalid-abi-type', Value => Value.writeUInt32LE(
            Foreignˉlayout.Typeˉcount, Foreignˉlayout.Operation + 9,
        )],
        ['abi-as-pointer', Value => Value.writeUInt32LE(
            Foreignˉlayout.Pointerˉtype, Foreignˉlayout.Operation + 9,
        )],
        ['pointer-as-abi', Value => Value.writeUInt32LE(
            Foreignˉlayout.Abiˉtype, Foreignˉlayout.Operation + 5,
        )],
        ['pointer-stack-kind', Value => Value.writeUInt32LE(
            Foreignˉlayout.Capacityˉlocal, Foreignˉlayout.Operation - 14,
        )],
        ['capacity-stack-kind', Value => Value.writeUInt32LE(
            Foreignˉlayout.Pointerˉlocal, Foreignˉlayout.Operation - 9,
        )],
        ['generation-stack-kind', Value => Value.writeUInt32LE(
            Foreignˉlayout.Pointerˉlocal, Foreignˉlayout.Operation - 4,
        )],
    ];
    for (let Index = 0; Index < Foreignˉcases.length; Index += 1) {
        const [Name, Mutate] = Foreignˉcases[Index];
        process.stdout.write(
            `native unsafe write pointer lowering item=${Index + 1}/` +
            `${Foreignˉcases.length} case=foreign-${Name} status=Started\n`,
        );
        const Candidate = Buffer.from(Foreignˉsuccess);
        Mutate(Candidate);
        const Candidateˉpath = join(Work, `Malformed-Foreign-${Name}.wvb`);
        const Destination = join(Work, `Malformed-Foreign-${Name}.wvo`);
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        const Rejected = await Runˉprocess(
            Lowerer, [Candidateˉpath, Destination],
            COMMAND_TIMEOUT_MILLISECONDS, `foreign-${Name}`,
        );
        if (Rejected.Code !== 1 || Rejected.Exceeded || Rejected.Timedˉout ||
            existsSync(Destination) ||
            !/^native x64 status=(?:Invalidˉwvb|Unsupportedˉprofile|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u
                .test(Rejected.Output)) {
            Reject(`The malformed Foreign case ${Name} differed.\n` +
                Rejected.Output);
        }
    }
    if (Foreignˉexecutions !== (Target === 'linux' ? 2 : 0)) {
        Reject('The host-specific Foreign execution count differed.');
    }
    process.stdout.write(
        `native unsafe write pointer lowering status=Passed cases=${53 + Recordˉcases + Ownedˉcases + Vectorˉcases.Cases} ` +
        `valid=${28 + Recordˉcases + Ownedˉcases + Vectorˉcases.Valid} malformed=${25 + Vectorˉcases.Malformed} native-execution=${13 + Recordˉcases + Ownedˉcases + Vectorˉcases.Executions} ` +
        `record-return-cases=${Recordˉcases} owned-storage-cases=${Ownedˉcases} owned-vector-cases=${Vectorˉcases.Cases} foundation-borrow-cases=12 ` +
        'foreign-native-execution=linux-only foreign-links=2 compiler-source=current ' +
        'package-cache=development\n',
    );
    }
    }
    }
    }
} catch (Error) {
    Preserveˉwork = Error.cleanupUncertain === true;
    process.stderr.write(`${Error.message}\n`);
    process.exitCode = Error.exitCode ?? 1;
} finally {
    if (!Preserveˉwork) await Removeˉwork(Work);
}

async function Verifyˉsourceˉclosures() {
    const Projects = [
        'Compiler/Windvale-Native-X64-Lowering',
        'Compiler/Windvale-Native-X64-Lowering-Tool',
        'Compiler/Windvale-Native-X64-Lowering-Staging-Admission',
        'Compiler/Windvale-Native-X64-Lowering-Staging-Tool',
        'Tests/Windvale-Native-Test-Staging-Content-Native',
        'Tests/Windvale-Native-Test-X64-Foundation-Borrow-Machine-Probe',
        'Tests/Windvale-Native-Test-X64-Lowering-Data-Limit',
    ];
    for (const Projectˉname of Projects) {
        const Manifest = await readFile(join(Repositoryˉroot, 'Projects', Projectˉname + '.wvproj'), 'utf8');
        if (Buffer.byteLength(Manifest) > 65536) Reject('Lowerer source closure exceeds its bound.');
        const Sources = [...Manifest.matchAll(/^(?:root|source) "([^"]+)"\r?$/gmu)].map(Match => Match[1]);
        const Modules = new Set();
        const Imports = new Set();
        for (const Source of Sources) {
            const Text = await readFile(join(Repositoryˉroot, Source), 'utf8');
            if (Buffer.byteLength(Text) > FIXTURE_LIMIT) Reject('Lowerer source exceeds its bound.');
            const Module = /^module ([^ ;]+)[; ]/mu.exec(Text);
            if (Module === null || Modules.has(Module[1])) Reject('Lowerer module declaration differs.');
            Modules.add(Module[1]);
            for (const Match of Text.matchAll(/^import (Compilerˉnativeˉx64ˉlowering[^ ]*) as /gmu)) Imports.add(Match[1]);
        }
        for (const Imported of Imports) {
            if (!Modules.has(Imported)) Reject(`Missing lowerer source in ${Projectˉname}: ${Imported}`);
        }
    }
    process.stdout.write('native x64 lowering source-closures=7 status=Passed\n');
}

function Buildˉoptions() {
    return [...(Prepareˉonly ? ['--prepare-compiler'] : []),
        ...(Ownerˉdeadline === null ? [] : ['--deadline-ms', String(Ownerˉdeadline - 5000)])];
}

async function Runˉrecordˉreturnˉmemory(Lowerer) {
    const Template = await Readˉbinary(join(Candidateˉdirectory, 'Return-42.wvb'),
        174, '7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31');
    const U32 = Value => { const Bytes = Buffer.alloc(4); Bytes.writeUInt32LE(Value); return Bytes; };
    const Name = Value => { const Bytes = Buffer.from(Value); return Buffer.concat([U32(Bytes.length), Bytes]); };
    const Shape = (Kind, Index) => Index === undefined ? Buffer.from([Kind]) : Buffer.concat([Buffer.from([Kind]), U32(Index)]);
    const Op = (Kind, ...Values) => Buffer.concat([Buffer.from([Kind]), ...Values.map(U32)]);
    const Record = (Label, Fields) => Buffer.concat([Buffer.from([1]), Name(Label), U32(Fields.length),
        ...Fields.flatMap((Type, Index) => [Name(`Field${Index}`), Type])]);
    function Functionˉentry(Label, Return, Locals, Code, Stack = 2) {
        return { Label, Return, Locals, Code: Buffer.concat(Code), Stack };
    }
    function Module(Functions, Types, Minor = 11) {
        let Offset = 0;
        const Directory = [U32(Functions.length)];
        for (const Entry of Functions) {
            const Parameters = Entry.Parameters ?? [];
            Directory.push(Name(Entry.Label), U32(Parameters.length), ...Parameters, Entry.Return, U32(Entry.Locals.length), ...Entry.Locals,
                U32(Offset), U32(Entry.Code.length), U32(Entry.Stack));
            Offset += Entry.Code.length;
        }
        const Header = Buffer.from(Template.subarray(0, 12));
        Header.writeUInt16LE(Minor, 6);
        const Sections = new Map([
            [3, Buffer.concat([U32(2), Name('Padding'), Buffer.from([5]), U32(64), Buffer.alloc(64, 65),
                Name('Answer'), Buffer.from([5]), U32(4), U32(42)])],
            [4, Buffer.concat(Directory)], [5, Buffer.concat(Functions.map(Entry => Entry.Code))],
            [7, Buffer.concat([U32(Types.length), ...Types])],
        ]);
        const Result = [Header];
        for (let Cursor = 12; Cursor < Template.length;) {
            const Kind = Template[Cursor];
            const Length = Template.readUInt32LE(Cursor + 4);
            const Payload = Sections.get(Kind) ?? Template.subarray(Cursor + 8, Cursor + 8 + Length);
            const Section = Buffer.from(Template.subarray(Cursor, Cursor + 8));
            Section.writeUInt32LE(Payload.length, 4);
            Result.push(Section, Payload);
            Cursor += 8 + Length;
        }
        return Buffer.concat(Result);
    }
    const I32 = Shape(1), Bytes = Shape(6), Pair = Shape(7, 0);
    const Dead = [Op(10, 0), Op(10, 0), Op(0x77), Op(5, 0)];
    const Cases = [];
    for (const Iterations of [1, 256, 4096, 32768]) {
        const Code = [Op(1, 0x04030201), Op(0x7b), Op(5, 1),
            Op(4, 1), Op(9, 1), Op(9, 2), Op(12), Op(5, 2), Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(4, 0), Op(0x7b), Op(5, 1), Op(4, 2), Op(5, 2),
            Op(4, 2), Op(9, 0), Op(0x0d), Op(0x76), Op(9, 2), Op(0x60));
        const Aliasˉfailure = Op(0x31, 0); Code.push(Aliasˉfailure);
        Code.push(Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0), Op(4, 0), Op(1, Iterations), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(4, 1), Op(9, 0), Op(0x72),
            Op(1, Iterations - 1), Op(0x11), Op(1, 42), Op(0x10), Op(0x51));
        Aliasˉfailure.writeUInt32LE(Buffer.concat(Code).length, 1);
        Code.push(Op(1, 0), Op(0x51));
        Cases.push({ Name: `live-slice-replacement-${Iterations}`, Capacity: 12, Expected: 42,
            Input: Module([Functionˉentry('Main', I32, [I32, Bytes, Bytes], Code, 3)], []) });
    }
    for (const [Iterations, Fields] of [[1, 1], [256, 1], [4096, 1], [256, 64]]) {
        const Code = [Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(0x40, 1), Op(0x69, 0), Op(9, 0), Op(0x72), Op(5, 1),
            Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0), Op(4, 0), Op(1, Iterations), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(4, 1), Op(0x51));
        Cases.push({ Name: `borrowed-return-${Iterations}-fields-${Fields}`, Capacity: 1024, Expected: 42,
            Input: Module([
                Functionˉentry('Main', I32, [I32, I32], Code),
                Functionˉentry('Value', Pair, [Bytes], [...Dead,
                    ...Array.from({ length: Fields }, () => Op(10, 1)), Op(0x68, 0), Op(0x51)], Math.max(2, Fields)),
            ], [Record('Returned', Array(Fields).fill(Bytes))]) });
    }
    const Pairˉtype = Record('Aliases', [Bytes, Bytes, Bytes, Bytes, Bytes]);
    const Value = Functionˉentry('Value', Pair, [Bytes, Bytes, Bytes], [
        ...Dead, Op(1, 0x04030201), Op(0x7b), Op(5, 1), Op(1, 0x08070605), Op(0x7b), Op(5, 2),
        // Reverse address order, overlapping views, and a zero-length view.
        Op(4, 2), Op(9, 1), Op(9, 3), Op(12),
        Op(4, 1), Op(9, 1), Op(9, 3), Op(12), Op(4, 1), Op(4, 2),
        Op(4, 1), Op(9, 2), Op(9, 0), Op(12), Op(0x68, 0), Op(0x51),
    ], 7);
    const Unionˉcode = [Op(1, 0), Op(5, 0)];
    const Unionˉloop = Buffer.concat(Unionˉcode).length;
    Unionˉcode.push(Op(0x40, 1), Op(5, 1), Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0),
        Op(4, 0), Op(1, 64), Op(0x22));
    const Unionˉend = Buffer.concat(Unionˉcode).length + 10;
    Unionˉcode.push(Op(0x31, Unionˉend), Op(0x30, Unionˉloop),
        Op(4, 1), Op(0x69, 2), Op(9, 0), Op(0x72), Op(1, 0x04030201), Op(0x11), Op(1, 42), Op(0x10), Op(0x51));
    // Eight unique live bytes per return fit; copying every overlapping view
    // independently would exceed this ceiling during the same 64 calls.
    Cases.push({ Name: 'overlapping-range-union-64', Capacity: 800, Expected: 42,
        Input: Module([Functionˉentry('Main', I32, [I32, Pair], Unionˉcode), Value], [Pairˉtype]) });
    const Wrapper = Shape(7, 1);
    const Maybe = Shape(11, 1);
    const Variant = Buffer.concat([Buffer.from([3]), Name('Maybe'), U32(2),
        Name('Empty'), Buffer.from([0]), Name('Value'), Buffer.from([1]), Name('Payload'), Pair]);
    // Every loop entry preserves a live aggregate while its old sibling buffer
    // becomes dead. The fixed 12-byte arena also detects unbounded retention.
    for (const Kind of ['direct', 'nested', 'variant']) {
        const Root = Kind === 'direct' ? Pair : Kind === 'nested' ? Wrapper : Maybe;
        const Path = Kind === 'direct' ? [] : Kind === 'nested' ? [Op(0x69, 0)] : [Op(0x99, 1, 1)];
        const Code = [Op(1, 0x04030201), Op(0x7b), Op(5, 1),
            Op(4, 1), Op(9, 1), Op(9, 2), Op(12), Op(0x68, 0),
            ...(Kind === 'nested' ? [Op(0x68, 1)] : Kind === 'variant' ? [Op(0x97, 1, 1)] : []),
            Op(5, 2), Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(4, 0), Op(0x7b), Op(5, 1),
            Op(4, 2), ...Path, Op(0x69, 0), Op(9, 0), Op(0x0d), Op(0x76), Op(9, 2), Op(0x60));
        const Failure = Op(0x31, 0); Code.push(Failure);
        Code.push(Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0), Op(4, 0), Op(1, 4096), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(1, 42), Op(0x51));
        Failure.writeUInt32LE(Buffer.concat(Code).length, 1); Code.push(Op(1, 0), Op(0x51));
        const Types = [Record('Shared', [Bytes])];
        if (Kind !== 'direct') Types.push(Kind === 'nested' ? Record('Wrapper', [Pair]) : Variant);
        Cases.push({ Name: `live-${Kind}-replacement-4096`, Capacity: 12, Expected: 42,
            Input: Module([Functionˉentry('Main', I32, [I32, Bytes, Root], Code, 3)], Types, Kind === 'variant' ? 16 : 11) });
    }
    for (const Iterations of [1, 4096, 32768]) {
        const Code = [Op(1, 42), Op(0x7b), Op(5, 1), Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(4, 1), Op(9, 0), Op(0x72), Op(1, 42), Op(0x20));
        const Failure = Op(0x31, 0); Code.push(Failure);
        Code.push(Op(1, 42), Op(0x7b), Op(5, 1), Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0),
            Op(4, 0), Op(1, Iterations), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(4, 1), Op(9, 0), Op(0x72), Op(0x51));
        Failure.writeUInt32LE(Buffer.concat(Code).length, 1); Code.push(Op(1, 0), Op(0x51));
        Cases.push({ Name: `live-last-share-replacement-${Iterations}`, Capacity: 8, Expected: 42,
            Input: Module([Functionˉentry('Main', I32, [I32, Bytes], Code)], []) });
    }
    for (const Rootˉcount of [32, 33]) {
        // Keep the highest-ranked root live; rank 31 exercises the top mask bit.
        const Code = [Op(1, 99), Op(0x7b), Op(5, Rootˉcount), Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(1, 42), Op(0x7b), Op(5, 1),
            Op(4, Rootˉcount), Op(9, 0), Op(0x72), Op(1, 99), Op(0x20));
        const Failure = Op(0x31, 0); Code.push(Failure);
        Code.push(Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0),
            Op(4, 0), Op(1, 256), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(4, 1), Op(9, 0), Op(0x72), Op(0x51));
        Failure.writeUInt32LE(Buffer.concat(Code).length, 1); Code.push(Op(1, 0), Op(0x51));
        Cases.push({ Name: `live-root-bound-${Rootˉcount}`, Capacity: 8, Expected: Rootˉcount === 32 ? 42 : 1,
            Input: Module([Functionˉentry('Main', I32, [I32, ...Array(Rootˉcount).fill(Bytes)], Code)], []) });
    }
    {
        const Code = [Op(1, 42), Op(0x7b), Op(5, 1), Op(1, 42), Op(0x7b), Op(5, 2), Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(1, 7), Op(0x7b), Op(5, 3), Op(4, 0), Op(1, 128), Op(0x22));
        const Other = Op(0x31, 0); Code.push(Other);
        Code.push(Op(4, 1), Op(9, 0), Op(0x72), Op(5, 4));
        const Join = Op(0x30, 0); Code.push(Join);
        Other.writeUInt32LE(Buffer.concat(Code).length, 1);
        Code.push(Op(4, 2), Op(9, 0), Op(0x72), Op(5, 4));
        Join.writeUInt32LE(Buffer.concat(Code).length, 1);
        Code.push(Op(4, 4), Op(1, 42), Op(0x20));
        const Failure = Op(0x31, 0); Code.push(Failure);
        Code.push(Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0), Op(4, 0), Op(1, 256), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(1, 42), Op(0x51));
        Failure.writeUInt32LE(Buffer.concat(Code).length, 1); Code.push(Op(1, 0), Op(0x51));
        Cases.push({ Name: 'live-branch-join-256', Capacity: 12, Expected: 42,
            Input: Module([Functionˉentry('Main', I32, [I32, Bytes, Bytes, Bytes, I32], Code)], []) });
    }
    {
        const Main = [Op(1, 99), Op(0x7b), Op(5, 0), Op(4, 0), Op(4, 0), Op(0x68, 0), Op(5, 1),
            Op(4, 1), Op(4, 1), Op(0x40, 1), Op(5, 2)];
        const Mainˉfailures = [];
        for (const [Local, Field, Expected] of [[1, 0, 99], [2, 0, 42], [2, 1, 99]]) {
            Main.push(Op(4, Local), Op(0x69, Field), Op(9, 0), Op(0x72), Op(1, Expected), Op(0x20));
            const Failure = Op(0x31, 0); Main.push(Failure); Mainˉfailures.push(Failure);
        }
        Main.push(Op(1, 42), Op(0x51));
        for (const Failure of Mainˉfailures) Failure.writeUInt32LE(Buffer.concat(Main).length, 1);
        Main.push(Op(1, 0), Op(0x51));
        const Code = [Op(1, 0), Op(5, 2)], Loop = Buffer.concat(Code).length, Failures = [];
        Code.push(Op(1, 42), Op(0x7b), Op(5, 3));
        for (const Parameter of [0, 1]) {
            Code.push(Op(4, Parameter), Op(0x69, Parameter), Op(9, 0), Op(0x72), Op(1, 99), Op(0x20));
            const Failure = Op(0x31, 0); Code.push(Failure); Failures.push(Failure);
        }
        Code.push(Op(4, 2), Op(1, 1), Op(0x10), Op(5, 2), Op(4, 2), Op(1, 4096), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(4, 3), Op(4, 0), Op(0x69, 0), Op(0x68, 0), Op(0x51));
        for (const Failure of Failures) Failure.writeUInt32LE(Buffer.concat(Code).length, 1);
        Code.push(Op(10, 1), Op(10, 1), Op(0x68, 0), Op(0x51));
        const Helper = Functionˉentry('Value', Pair, [I32, Bytes], Code);
        Helper.Parameters = [Pair, Pair];
        Cases.push({ Name: 'live-borrowed-caller-and-return-4096', Capacity: 12, Expected: 42,
            Input: Module([Functionˉentry('Main', I32, [Bytes, Pair, Pair], Main), Helper], [Record('Shared', [Bytes, Bytes])]) });
    }
    for (const Fields of [32, 33]) {
        const Code = [...Array.from({ length: Fields }, () => Op(10, 1)), Op(0x68, 0), Op(5, 2),
            Op(4, 2), Op(5, 3), Op(1, 0), Op(5, 0)];
        const Loop = Buffer.concat(Code).length;
        Code.push(Op(1, 42), Op(0x7b), Op(5, 1));
        const Patches = [];
        for (const Local of [2, 3]) {
            Code.push(Op(4, Local), Op(0x69, Fields - 1), Op(9, 0), Op(0x72), Op(1, 42), Op(0x20));
            const Failure = Op(0x31, 0); Code.push(Failure); Patches.push(Failure);
        }
        Code.push(Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0), Op(4, 0), Op(1, 256), Op(0x22));
        const End = Buffer.concat(Code).length + 10;
        Code.push(Op(0x31, End), Op(0x30, Loop), Op(1, 42), Op(0x51));
        for (const Failure of Patches) Failure.writeUInt32LE(Buffer.concat(Code).length, 1);
        Code.push(Op(1, 0), Op(0x51));
        Cases.push({ Name: `live-field-bound-${Fields * 2}`, Capacity: 8, Expected: Fields === 32 ? 42 : 1,
            Input: Module([Functionˉentry('Main', I32, [I32, Bytes, Pair, Pair], Code, Fields)],
                [Record('Many', Array(Fields).fill(Bytes))]) });
    }
    const Emptyˉcode = [Op(1, 0), Op(5, 0)];
    const Emptyˉloop = Buffer.concat(Emptyˉcode).length;
    Emptyˉcode.push(Op(0x40, 1), Op(0x98, 1, 0));
    const Wrongˉcase = Op(0x31, 0); Emptyˉcode.push(Wrongˉcase);
    Emptyˉcode.push(Op(4, 0), Op(1, 1), Op(0x10), Op(5, 0), Op(4, 0), Op(1, 4096), Op(0x22));
    const Emptyˉend = Buffer.concat(Emptyˉcode).length + 10;
    Emptyˉcode.push(Op(0x31, Emptyˉend), Op(0x30, Emptyˉloop), Op(1, 42), Op(0x51));
    Wrongˉcase.writeUInt32LE(Buffer.concat(Emptyˉcode).length, 1);
    Emptyˉcode.push(Op(1, 0), Op(0x51));
    Cases.push({ Name: 'inactive-variant-4096', Capacity: 1024, Expected: 42,
        Input: Module([
            Functionˉentry('Main', I32, [I32], Emptyˉcode),
            Functionˉentry('Value', Maybe, [Bytes], [...Dead, Op(0x97, 1, 0), Op(0x51)]),
        ], [Pairˉtype, Variant], 16) });
    for (const Kind of ['direct', 'nested', 'variant']) {
        const Return = Kind === 'direct' ? Pair : Kind === 'nested' ? Wrapper : Maybe;
        const Path = Kind === 'direct' ? [] : Kind === 'nested' ? [Op(0x69, 0)] : [Op(0x99, 1, 1)];
        const Call = Kind === 'direct' ? 1 : 2;
        const Code = [Op(0x40, Call), Op(5, 0), Op(0x40, Call), Op(5, 1)];
        const Patches = [];
        for (const Local of [0, 1]) {
            for (const [Field, Expected] of [[0, 6], [1, 2], [2, 0x04030201], [3, 0x08070605], [4, 0]]) {
                Code.push(Op(4, Local), ...Path, Op(0x69, Field));
                if (Field === 4) Code.push(Op(0x0b), Op(9, Expected), Op(0x60));
                else if (Field < 2) Code.push(Op(9, 0), Op(0x0d), Op(0x76), Op(9, Expected), Op(0x60));
                else Code.push(Op(9, 0), Op(0x72), Op(1, Expected), Op(0x20));
                const Patch = Op(0x31, 0); Patches.push(Patch); Code.push(Patch);
            }
        }
        Code.push(Op(1, 42), Op(0x51));
        const Failure = Buffer.concat(Code).length;
        Code.push(Op(1, 0), Op(0x51));
        for (const Patch of Patches) Patch.writeUInt32LE(Failure, 1);
        const Functions = [Functionˉentry('Main', I32, [Return, Return], Code), Value];
        const Types = [Pairˉtype];
        if (Kind !== 'direct') {
            Types.push(Kind === 'nested' ? Record('Wrapper', [Pair]) : Variant);
            Functions.push(Functionˉentry('Wrap', Return, [], [Op(0x40, 1),
                ...(Kind === 'nested' ? [Op(0x68, 1)] : [Op(0x97, 1, 1)]), Op(0x51)], 1));
        }
        Cases.push({ Name: `${Kind}-overlapping-aliases`, Capacity: 1024, Expected: 42,
            Input: Module(Functions, Types, Kind === 'variant' ? 16 : 11) });
    }
    for (const Crossing of [false, true]) {
        const Code = [Op(1, 99), Op(0x7b), Op(5, 0), Op(4, 0), Op(0x40, 1), Op(5, 1),
            Op(1, 123), Op(0x7b), Op(5, 2)];
        const Patches = [];
        for (const [Field, Offset, Expected] of [[0, 0, 99], [1, 0, Crossing ? 99 : 42],
            ...(Crossing ? [[1, 4, 42]] : [])]) {
            Code.push(Op(4, 1), Op(0x69, Field), Op(9, Offset), Op(0x72), Op(1, Expected), Op(0x20));
            const Patch = Op(0x31, 0); Patches.push(Patch); Code.push(Patch);
        }
        Code.push(Op(4, 0), Op(9, 0), Op(0x72), Op(1, 99), Op(0x20));
        const Checkˉcaller = Op(0x31, 0); Patches.push(Checkˉcaller); Code.push(Checkˉcaller);
        Code.push(Op(1, 42), Op(0x51));
        const Failure = Buffer.concat(Code).length;
        for (const Patch of Patches) Patch.writeUInt32LE(Failure, 1);
        Code.push(Op(1, 0), Op(0x51));
        const Helper = Crossing
            ? Functionˉentry('Value', Pair, [], [Op(4, 0), Op(4, 0), Op(10, 1), Op(0x77), Op(0x68, 0), Op(0x51)], 3)
            : Functionˉentry('Value', Pair, [Bytes], [Op(10, 0), Op(10, 0), Op(0x77), Op(5, 1),
                Op(4, 0), Op(1, 42), Op(0x7b), Op(0x68, 0), Op(0x51)]);
        Helper.Parameters = [Bytes];
        Cases.push({ Name: Crossing ? 'checkpoint-crossing-range' : 'caller-and-callee-ranges',
            Capacity: Crossing ? 12 : 512, Expected: 42, Input: Module([
                Functionˉentry('Main', I32, [Bytes, Pair, Bytes], Code), Helper,
            ], [Record('Shared', [Bytes, Bytes])]) });
    }
    Cases.push({ ...Cases.find(Case => Case.Name === 'borrowed-return-1-fields-1'), Name: 'allocation-refusal', Capacity: 127, Expected: 1 });
    const Context = Buffer.alloc(112);
    Context.writeUInt32LE(7, 0); Context.writeUInt32LE(112, 4);
    Context.writeBigUInt64LE(1000000n, 8); Context.writeBigUInt64LE(1024n, 16);
    Context.writeUInt32LE(2097152, 40); Context.writeUInt32LE(16777216, 56);
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native record return item=${Index + 1}/${Cases.length} case=${Case.Name} arena=${Case.Capacity} status=Started\n`);
        const Prefix = join(Work, `Record-${Case.Name}`), Source = Prefix + '.wvb', Object = Prefix + '.wvo';
        await writeFile(Source, Case.Input, { flag: 'wx' });
        await Requireˉsuccess(Lowerer, [Source, Object], `record-${Case.Name}-lower`);
        await Requireˉsuccess(Lowerer, [Source, Prefix + '-repeat.wvo'], `record-${Case.Name}-repeat`);
        if (!(await readFile(Object)).equals(await readFile(Prefix + '-repeat.wvo'))) Reject('Record return lowering is not deterministic.');
        await Requireˉsuccess(Check, [Object], `record-${Case.Name}-check`);
        const Image = Prefix + '.bin';
        const Linked = await Requireˉsuccess(Link, ['0', 'Main', Image, Object], `record-${Case.Name}-link`);
        const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
        if (Entry === null) Reject('Record return test entry point is missing.');
        const Application = Prefix + '.' + Nativeˉextension;
        await Requireˉsuccess(Packageˉconsole, [`${Target}-x64-console-v1`, Image, Entry[1], Application], `record-${Case.Name}-package`);
        const Packaged = await readFile(Application), Location = Packaged.indexOf(Context);
        if (Location < 0 || Packaged.indexOf(Context, Location + 1) >= 0) Reject('Record test execution context is not unique.');
        Packaged.writeUInt32LE(Case.Capacity, Location + 56);
        await writeFile(Application, Packaged);
        const Result = await Runˉprocess(Application, [], COMMAND_TIMEOUT_MILLISECONDS, `record-${Case.Name}-execute`);
        if (Result.Code !== Case.Expected || Result.Exceeded || Result.Timedˉout || Result.Output !== '') {
            Reject(`Record return execution failed: ${Case.Name}, code=${Result.Code}, output=${Result.Output}.`);
        }
    }
    process.stdout.write(`native record return status=Passed cases=${Cases.length} iterations=1/256/4096/32768 replacement-arena=8/12 return-arena=1024 descriptor-limit=64 alias-depth=record/variant allocation-refusal=127\n`);
    return Cases.length;
}

async function Runˉframeˉinitialization(Lowerer) {
    const Baseline = join(Candidateˉdirectory, `Wvb-To-Wvo.${Nativeˉextension}`);
    await Readˉbinary(Baseline, 10_661_888, WINDOWS
        ? 'a46d73ada72fba9561e9db1fcfc5477bf19be2518ad9db2d8487184112923dfd'
        : '9c331308e5afe852d4c0441e22c1ff68a0ac0c86793c2e403f38556302c90fd3',
    10_661_888);
    const Template = await Readˉbinary(join(Candidateˉdirectory, 'Return-42.wvb'),
        174, '7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31');
    function U32(Value) {
        const Result = Buffer.alloc(4);
        Result.writeUInt32LE(Value);
        return Result;
    }
    function Op(Code, Operand) {
        return Operand === undefined ? Buffer.from([Code]) :
            Buffer.concat([Buffer.from([Code]), U32(Operand)]);
    }
    function Functionˉentry(Name, Parameters, Locals, Instructions, Stack) {
        return { Name, Parameters, Locals, Code: Buffer.concat(Instructions), Stack };
    }
    function String(Value) {
        const Encoded = Buffer.from(Value);
        return Buffer.concat([U32(Encoded.length), Encoded]);
    }
    function Module(Functions, Sections = new Map()) {
        let Offset = 0;
        const Directory = [U32(Functions.length)];
        for (const Entry of Functions) {
            const Name = Buffer.from(Entry.Name);
            Directory.push(U32(Name.length), Name, U32(Entry.Parameters),
                Buffer.alloc(Entry.Parameters, 1), Entry.Return ?? Buffer.from([1]), U32(Entry.Locals),
                Buffer.alloc(Entry.Locals, 1), U32(Offset), U32(Entry.Code.length),
                U32(Entry.Stack));
            Offset += Entry.Code.length;
        }
        const Result = [Template.subarray(0, 12)];
        for (let Cursor = 12; Cursor < Template.length;) {
            const Kind = Template[Cursor];
            const Length = Template.readUInt32LE(Cursor + 4);
            const Payload = Kind === 4 ? Buffer.concat(Directory) :
                Kind === 5 ? Buffer.concat(Functions.map(Entry => Entry.Code)) :
                    Sections.get(Kind) ?? Template.subarray(Cursor + 8, Cursor + 8 + Length);
            const Header = Buffer.from(Template.subarray(Cursor, Cursor + 8));
            Header.writeUInt32LE(Payload.length, 4);
            Result.push(Header, Payload);
            Cursor += 8 + Length;
        }
        return Buffer.concat(Result);
    }
    const Cases = [{ Name: 'one-value-cell', Expected: 42, Input: Module([
        Functionˉentry('Main', 0, 0, [Op(1, 42), Op(0x51)], 1),
    ]) }];
    for (const Locals of [1, 16, 63, 1024]) {
        // The first call poisons the last local; the same callee frame must
        // read zero on its second entry. Both sides of the branch are used.
        Cases.push({ Name: `reentry-zero-${Locals}`, Expected: 42, Input: Module([
            Functionˉentry('Main', 0, 1, [Op(1, 1), Op(0x40, 1), Op(5, 0),
                Op(1, 0), Op(0x40, 1), Op(1, 42), Op(0x10), Op(0x51)], 2),
            Functionˉentry('Value', 1, Locals, [Op(4, 0), Op(1, 0), Op(0x20),
                Op(0x31, 22), Op(4, Locals), Op(0x51), Op(1, 99),
                Op(5, Locals), Op(1, 0), Op(0x51)], 2),
        ]) });
    }
    const Sum = [];
    for (let Index = 0; Index < 8; Index += 1) {
        Sum.push(Op(4, Index));
        if (Index > 0) Sum.push(Op(0x10));
    }
    Cases.push({ Name: 'register-and-stack-parameters', Expected: 42, Input: Module([
        Functionˉentry('Main', 0, 0, [...Array.from({ length: 8 }, (_, Index) =>
            Op(1, Index + 1)), Op(0x40, 1), Op(1, 6), Op(0x10), Op(0x51)], 8),
        Functionˉentry('Value', 8, 64, [...Sum, Op(0x51)], 2),
    ]) });
    Cases.push({ Name: 'fuel-exhaustion', Expected: null, Input: Module([
        Functionˉentry('Main', 0, 64, [Op(1, 0), Op(5, 0), Op(0x30, 0)], 1),
    ]) });
    Cases.push({ Name: 'depth-exhaustion', Expected: null, Input: Module([
        Functionˉentry('Main', 0, 0, [Op(0x40, 1), Op(0x51)], 1),
        Functionˉentry('Value', 0, 64, [Op(0x40, 1), Op(0x51)], 1),
    ]) });
    Cases.push({ Name: 'hidden-record-return', Expected: 42, Input: Module([
        Functionˉentry('Main', 0, 0, [Op(0x40, 1), Op(0x69, 0),
            Op(0x40, 1), Op(0x69, 1), Op(0x10), Op(0x51)], 2),
        { ...Functionˉentry('Value', 0, 64, [Op(1, 20), Op(1, 22), Op(0x68, 0), Op(0x51)], 2),
            Return: Buffer.concat([Buffer.from([7]), U32(0)]) },
    ], new Map([[7, Buffer.concat([U32(1), Buffer.from([1]), String('Pair'),
        U32(2), String('Left'), Buffer.from([1]), String('Right'), Buffer.from([1])])]])) });
    Cases.push({ Name: 'hidden-descriptor-return', Expected: 42, Input: Module([
        Functionˉentry('Main', 0, 0, [Op(0x40, 1), Op(9, 0), Op(0x72), Op(0x51)], 2),
        { ...Functionˉentry('Value', 0, 64, [Op(0x0a, 0), Op(0x51)], 1), Return: Buffer.from([6]) },
    ], new Map([[3, Buffer.concat([U32(1), String('Answer'), Buffer.from([5]), U32(4), U32(42)])]])) });
    for (const [Index, Case] of Cases.entries()) {
        process.stdout.write(`native frame initialization item=${Index + 1}/${Cases.length} case=${Case.Name} status=Started\n`);
        const Source = join(Work, `Frame-${Case.Name}.wvb`);
        await writeFile(Source, Case.Input, { flag: 'wx' });
        const Results = [];
        for (const [Role, Producer] of [['reference', Baseline], ['current', Lowerer]]) {
            const Prefix = join(Work, `Frame-${Case.Name}-${Role}`);
            const Object = Prefix + '.wvo';
            await Requireˉsuccess(Producer, [Source, Object], `frame-${Case.Name}-${Role}-lower`);
            if (Role === 'current') {
                const Repeated = Prefix + '-repeat.wvo';
                await Requireˉsuccess(Producer, [Source, Repeated], `frame-${Case.Name}-repeat`);
                const Bytes = await readFile(Object);
                if (!Bytes.equals(await readFile(Repeated))) Reject('Frame lowering is not deterministic.');
                if (Case.Name === 'reentry-zero-1024' &&
                    (Bytes.length >= 8192 || !Bytes.includes(Buffer.from('fcf3ab595f', 'hex')))) {
                    Reject('Large-frame initialization is not compact.');
                }
            }
            await Requireˉsuccess(Check, [Object], `frame-${Case.Name}-${Role}-check`);
            const Image = Prefix + '.bin';
            const Linked = await Requireˉsuccess(Link, ['0', 'Main', Image, Object], `frame-${Case.Name}-${Role}-link`);
            const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
            if (Entry === null) Reject('Frame test entry point is missing.');
            const Application = Prefix + '.' + Nativeˉextension;
            await Requireˉsuccess(Packageˉconsole,
                [`${Target}-x64-console-v1`, Image, Entry[1], Application], `frame-${Case.Name}-${Role}-package`);
            const Result = await Runˉprocess(Application, [], COMMAND_TIMEOUT_MILLISECONDS, `frame-${Case.Name}-${Role}-execute`);
            if (Result.Exceeded || Result.Timedˉout ||
                (Case.Expected !== null && Result.Code !== Case.Expected)) Reject(`Frame execution failed: ${Case.Name}, ${Role}.`);
            Results.push(Result);
        }
        if (Results[0].Code !== Results[1].Code || Results[0].Output !== Results[1].Output ||
            (Case.Expected === null && Results[0].Code === 0)) {
            Reject(`Frame initialization changed runtime behavior: ${Case.Name}.`);
        }
    }
}

async function Runˉoptionˉu64(Lowerer) {
    process.stdout.write('native unsafe write pointer lowering case=option-u64-1.16 status=Started\n');
    const Input = await Readˉfixture(
        join(Fixtureˉdirectory, 'Option-U64-Return.wvb.b64'),
        '8adba32580fde1749011c4a1d45c4eac1f02f4eb1e3ddf5d2afd96ba03ba41ad',
    );
    if (Input.length !== 565 || Input.readUInt16LE(6) !== 16) {
        Reject('The Option<u64> WVB 1.16 fixture shape differs.');
    }
    const Source = join(Work, 'Option-U64.wvb');
    const Object = join(Work, 'Option-U64.wvo');
    const Repeated = join(Work, 'Option-U64-Repeated.wvo');
    await writeFile(Source, Input, { flag: 'wx' });
    await Requireˉsuccess(Lowerer, [Source, Object], 'option-u64-lower');
    await Requireˉsuccess(Lowerer, [Source, Repeated], 'option-u64-repeat');
    if (!(await readFile(Object)).equals(await readFile(Repeated))) {
        Reject('The Option<u64> native object is not deterministic.');
    }
    await Requireˉsuccess(Check, [Object], 'option-u64-object-check');
    const Image = join(Work, 'Option-U64.bin');
    const Linked = await Requireˉsuccess(Link,
        ['0', 'Main', Image, Object], 'option-u64-link');
    const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked.Output);
    if (Entry === null) Reject('The Option<u64> entry point is missing.');
    const Application = join(Work, `Option-U64.${Nativeˉextension}`);
    await Requireˉsuccess(Packageˉconsole,
        [`${Target}-x64-console-v1`, Image, Entry[1], Application],
        'option-u64-package');
    const Executed = await Runˉprocess(Application, [],
        COMMAND_TIMEOUT_MILLISECONDS, 'option-u64-execute');
    if (Executed.Code !== 42 || Executed.Exceeded || Executed.Timedˉout ||
        Executed.Output !== '') Reject('The Option<u64> native execution differs.');
    const Cases = [
        ['unsupported-minor', Value => { Value.writeUInt16LE(15, 6); return Value; }],
        ['truncated', Value => Value.subarray(0, Value.length - 1)],
        ['trailing', Value => Buffer.concat([Value, Buffer.from([0])])],
        ['oversized-section', Value => { Value.writeUInt32LE(0xffff_ffff, 16); return Value; }],
        ['reserved-section', Value => { Value[13] = 1; return Value; }],
    ];
    for (const [Name, Mutate] of Cases) {
        const Candidate = join(Work, `Option-U64-${Name}.wvb`);
        const Destination = join(Work, `Option-U64-${Name}.wvo`);
        await writeFile(Candidate, Mutate(Buffer.from(Input)), { flag: 'wx' });
        const Rejected = await Runˉprocess(Lowerer, [Candidate, Destination],
            COMMAND_TIMEOUT_MILLISECONDS, `option-u64-${Name}`);
        if (Rejected.Code !== 1 || Rejected.Exceeded || Rejected.Timedˉout ||
            existsSync(Destination) ||
            !/^native x64 status=(?:Invalidˉwvb|Unsupportedˉprofile|Unsupportedˉmodule|Unsupportedˉfunction|Unsupportedˉcode) /u.test(Rejected.Output)) {
            Reject(`The Option<u64> malformed case ${Name} differs.\n${Rejected.Output}`);
        }
    }
}

async function Runˉfoundationˉborrowˉemission() {
    const Probeˉproject = join(Repositoryˉroot, 'Projects', 'Tests',
        'Windvale-Native-Test-X64-Foundation-Borrow-Machine-Probe.wvproj');
    const Probeˉwvb = join(Work, 'Borrow-Probe.wvb');
    const Probe = join(Work, `Borrow-Probe.${Nativeˉextension}`);
    const Binary = join(Work, 'Borrow-Code.bin');
    const Executable = join(Work, `Borrow-Code.${Nativeˉextension}`);
    if (Suppliedˉborrowˉprobe) {
        const Source = resolve(process.argv[8]);
        const Information = await stat(Source);
        if (!Information.isFile() || Information.size < 1 || Information.size > FIXTURE_LIMIT) {
            Reject('The supplied borrow probe exceeds its file bound.');
        }
        const Bytes = await Readˉbinary(Source, Information.size, process.argv[9]);
        await writeFile(Probeˉwvb, Bytes, { flag: 'wx' });
        process.stdout.write(`native foundation borrow step=probe-reuse sha256=${process.argv[9]}\n`);
    } else {
        await Requireˉsuccess(process.execPath,
            [Build, ...Buildˉoptions(), Probeˉproject, Probeˉwvb],
            'foundation-borrow-probe-build',
            Prepareˉonly ? Ownerˉdeadline - Date.now() : CONSTRUCTION_TIMEOUT_MILLISECONDS);
    }
    await Requireˉsuccess(Packageˉlowerer,
        ['6', Probeˉwvb, Probe, '--development-cache'],
        'foundation-borrow-probe-package', CONSTRUCTION_TIMEOUT_MILLISECONDS);
    if (Prepareˉonly) return;
    await Requireˉsuccess(Probe, [Binary], 'foundation-borrow-probe-generate');
    const Code = await readFile(Binary);
    if (Code.length === 0 || Code.length > 16 * 1024) {
        Reject('The Foundation borrow machine probe exceeded its bounded size.');
    }
    await Requireˉsuccess(Packageˉconsole,
        [Target === 'windows' ? 'windows-x64-console-v1' : 'linux-x64-console-v1',
            Binary, '0', Executable], 'foundation-borrow-code-package');
    const Result = await Runˉprocess(Executable, [], 10_000,
        'foundation-borrow-code-execute');
    if (Result.Code !== 42) {
        Reject(`The Foundation borrow machine probe failed: ${Result.Code}\n${Result.Output}`);
    }
    process.stdout.write('native foundation borrow emission status=Passed cases=12 ' +
        `host=${Target} code-bytes=${Code.length} code-sha256=` +
        createHash('sha256').update(Code).digest('hex') + '\n');
}

async function Readˉbinary(Path, Expectedˉsize, Expectedˉsha256, Maximum = FIXTURE_LIMIT) {
    const Information = await stat(Path);
    if (!Information.isFile() || Information.size !== Expectedˉsize ||
        Information.size > Maximum) Reject(`The fixture ${basename(Path)} size differs.`);
    const Result = await readFile(Path);
    const Digest = createHash('sha256').update(Result).digest('hex');
    if (Result.length !== Expectedˉsize || Result.length > Maximum ||
        Digest !== Expectedˉsha256) {
        Reject(`The fixture ${basename(Path)} identity differs.`);
    }
    return Result;
}

async function Lowerˉidentity(
    Lowerer,
    Input,
    Prefix,
    Label,
    Expectedˉcodeˉbytes,
    Expectedˉobjectˉbytes,
    Expectedˉsha256,
) {
    const Source = `${Prefix}.wvb`;
    const Destination = `${Prefix}.wvo`;
    await writeFile(Source, Input, { flag: 'wx' });
    const Lowered = await Runˉprocess(
        Lowerer, [Source, Destination], COMMAND_TIMEOUT_MILLISECONDS, Label,
    );
    const Expectedˉreport =
        `native x64 status=Valid abi=22 code-bytes=${Expectedˉcodeˉbytes} ` +
        `object-bytes=${Expectedˉobjectˉbytes}\n`;
    if (!Passed(Lowered) || !existsSync(Destination) ||
        Lowered.Output.replaceAll('\r\n', '\n') !== Expectedˉreport) {
        Reject(`The ${Label} lowering differed.\n${Lowered.Output}`);
    }
    await Readˉbinary(
        Destination, Expectedˉobjectˉbytes, Expectedˉsha256,
    );
    return Destination;
}

function Inspectˉforeignˉfixture(Input) {
    if (Input.length < 12 || Input.length > FIXTURE_LIMIT ||
        Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 38 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The Foreign WVB 1.38 fixture header differs.');
    }
    const Sections = new Map();
    let Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor > Input.length - 8 || Input[Cursor] !== Kind ||
            Input[Cursor + 1] !== 0 || Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject('The Foreign WVB fixture section envelope differs.');
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Start = Cursor + 8;
        if (Length > Input.length - Start) {
            Reject('The Foreign WVB fixture section exceeds the file.');
        }
        Sections.set(Kind, { Start, End: Start + Length });
        Cursor = Start + Length;
    }
    if (Cursor !== Input.length) {
        Reject('The Foreign WVB fixture has trailing bytes.');
    }
    const Types = Sections.get(7);
    if (Types.End - Types.Start < 4) {
        Reject('The Foreign WVB fixture type directory is truncated.');
    }
    const Typeˉcount = Input.readUInt32LE(Types.Start);
    if (Typeˉcount === 0 || Typeˉcount > 65_536) {
        Reject('The Foreign WVB fixture type count is invalid.');
    }
    const Functions = Sections.get(4);
    const Code = Sections.get(5);
    if (Functions.End - Functions.Start < 4) {
        Reject('The Foreign WVB fixture function directory is truncated.');
    }
    const Functionˉcount = Input.readUInt32LE(Functions.Start);
    if (Functionˉcount === 0 || Functionˉcount > 65_536) {
        Reject('The Foreign WVB fixture function count is invalid.');
    }
    const Ranges = [];
    Cursor = Functions.Start + 4;
    for (let Function = 0; Function < Functionˉcount; Function += 1) {
        Cursor = Skipˉwvbˉstring(Input, Cursor, Functions.End);
        Cursor = Checkˉwvbˉrange(Cursor, 4, Functions.End);
        const Parameters = Input.readUInt32LE(Cursor - 4);
        if (Parameters > 2_048) {
            Reject('The Foreign WVB fixture parameter count is oversized.');
        }
        for (let Parameter = 0; Parameter < Parameters; Parameter += 1) {
            Cursor = Skipˉwvbˉshape(Input, Cursor, Functions.End);
        }
        Cursor = Skipˉwvbˉshape(Input, Cursor, Functions.End);
        Cursor = Checkˉwvbˉrange(Cursor, 4, Functions.End);
        const Locals = Input.readUInt32LE(Cursor - 4);
        if (Locals > 4_096 - Parameters) {
            Reject('The Foreign WVB fixture local count is oversized.');
        }
        for (let Local = 0; Local < Locals; Local += 1) {
            Cursor = Skipˉwvbˉshape(Input, Cursor, Functions.End);
        }
        const Metadata = Cursor;
        Cursor = Checkˉwvbˉrange(Cursor, 12, Functions.End);
        const Offset = Input.readUInt32LE(Metadata);
        const Length = Input.readUInt32LE(Metadata + 4);
        if (Offset > Code.End - Code.Start ||
            Length > Code.End - Code.Start - Offset) {
            Reject('The Foreign WVB fixture code range is invalid.');
        }
        Ranges.push({
            Start: Code.Start + Offset,
            End: Code.Start + Offset + Length,
        });
    }
    if (Cursor !== Functions.End) {
        Reject('The Foreign WVB fixture function directory has trailing bytes.');
    }
    const Matches = [];
    for (const Range of Ranges) {
        for (Cursor = Range.Start; Cursor <= Range.End - 13; Cursor += 1) {
            if (Input[Cursor] !== 224 || Input.readUInt32LE(Cursor + 1) !== 1 ||
                Cursor < Range.Start + 15 || Cursor > Range.End - 18 ||
                Input[Cursor - 15] !== 4 || Input[Cursor - 10] !== 4 ||
                Input[Cursor - 5] !== 4 || Input[Cursor + 13] !== 5) {
                continue;
            }
            Matches.push({
                Operation: Cursor,
                Pointerˉtype: Input.readUInt32LE(Cursor + 5),
                Abiˉtype: Input.readUInt32LE(Cursor + 9),
                Typeˉcount,
                Pointerˉlocal: Input.readUInt32LE(Cursor - 14),
                Capacityˉlocal: Input.readUInt32LE(Cursor - 9),
                Generationˉlocal: Input.readUInt32LE(Cursor - 4),
            });
        }
    }
    if (Matches.length !== 1) {
        Reject('The Foreign WVB fixture must contain one exact opcode 224 call.');
    }
    return Matches[0];
}

function Checkˉwvbˉrange(Cursor, Length, End) {
    if (!Number.isSafeInteger(Cursor) || !Number.isSafeInteger(Length) ||
        Cursor < 0 || Length < 0 || Cursor > End || Length > End - Cursor) {
        Reject('The Foreign WVB fixture directory is truncated.');
    }
    return Cursor + Length;
}

function Skipˉwvbˉstring(Input, Cursor, End) {
    const Lengthˉend = Checkˉwvbˉrange(Cursor, 4, End);
    return Checkˉwvbˉrange(
        Lengthˉend, Input.readUInt32LE(Cursor), End,
    );
}

function Skipˉwvbˉshape(Input, Cursor, End) {
    Checkˉwvbˉrange(Cursor, 1, End);
    if ([7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30, 35]
        .includes(Input[Cursor])) {
        return Checkˉwvbˉrange(Cursor, 5, End);
    }
    return Cursor + 1;
}

async function Readˉfixture(Path, Expectedˉsha256) {
    const Encoded = await readFile(Path, 'utf8');
    if (Encoded.length > FIXTURE_LIMIT * 2 ||
        !/^[A-Za-z0-9+/=\r\n]+$/u.test(Encoded)) {
        Reject(`The fixture ${basename(Path)} is malformed or oversized.`);
    }
    const Result = Buffer.from(Encoded.replaceAll(/\s/gu, ''), 'base64');
    const Digest = createHash('sha256').update(Result).digest('hex');
    if (Result.length === 0 || Result.length > FIXTURE_LIMIT ||
        Digest !== Expectedˉsha256) {
        Reject(`The fixture ${basename(Path)} identity differs.`);
    }
    return Result;
}

function Passed(Result) {
    return Result.Code === 0 && !Result.Exceeded && !Result.Timedˉout;
}

async function Requireˉsuccess(
    Tool, Arguments, Label, Timeout = COMMAND_TIMEOUT_MILLISECONDS,
) {
    const Result = await Runˉprocess(Tool, Arguments, Timeout, Label);
    if (!Passed(Result)) {
        throw Object.assign(new Error(`The ${Label} step failed with exit ${Result.Code}.\n${Result.Output}`),
            { exitCode: [64, 124].includes(Result.Code) ? Result.Code : 1 });
    }
    return Result;
}

async function Runˉprocess(Tool, Arguments, Timeout, Step) {
    const Deadline = Math.min(Date.now() + Timeout, Ownerˉdeadline ?? Number.MAX_SAFE_INTEGER);
    const Result = await Runˉdevelopmentˉcommand(Tool, Arguments, Deadline, Prepareˉonly, OUTPUT_LIMIT);
    return { Code: Result.Code, Output: Result.Output + Result.Error, Exceeded: false, Timedˉout: false };
}
async function Removeˉwork(Path) {
    const Temporaryˉroot = await realpath(resolve(tmpdir()));
    const Parent = await realpath(dirname(Path));
    if (Parent !== Temporaryˉroot ||
        !basename(Path).startsWith('windvale-write-pointer-lowering-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Path}`);
    }
    await rm(Path, { force: false, maxRetries: 2, recursive: true });
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs ' +
        '<windows|linux> <repository-root> [--foundation-borrow-emission|--owned-storage|' +
        '--prepare-only --maximum-seconds <30-5400>|--prepared-products-only --maximum-seconds <30-600>|' +
        '--lowerer <application> <sha256> [--record-return-memory|--borrow-probe <wvb> <sha256>]] ' +
        '[--budget-oracle <wvb> <sha256>] [--owned-vector <wvb> <sha256>]\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
