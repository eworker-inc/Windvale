import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import { existsSync } from 'node:fs';
import { mkdtemp, readFile, realpath, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';

const WINDOWS = process.platform === 'win32';
const OUTPUT_LIMIT = 64 * 1024;
const FIXTURE_LIMIT = 4 * 1024 * 1024;
const COMMAND_TIMEOUT_MILLISECONDS = 120_000;
const CONSTRUCTION_TIMEOUT_MILLISECONDS = 15 * 60_000;
const PROGRESS_INTERVAL_MILLISECONDS = 30_000;

if (process.argv.length !== 4 ||
    !['windows', 'linux'].includes(process.argv[2])) Usage();
const Target = process.argv[2];
if ((WINDOWS && Target !== 'windows') || (!WINDOWS && Target !== 'linux')) {
    Reject('The native unsafe write-pointer target does not match this host.');
}
const Repositoryˉroot = await realpath(resolve(process.argv[3]));
const Extension = WINDOWS ? 'cmd' : 'sh';
const Nativeˉextension = WINDOWS ? 'exe' : 'elf';
const Build = join(Repositoryˉroot, 'Tools', 'Native', `Build-Wvb.${Extension}`);
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
try {
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
    const Lowerer = join(Work, `Native-Lowerer.${Nativeˉextension}`);
    process.stdout.write(
        'native unsafe write pointer lowering step=compiler-build status=Started\n',
    );
    await Requireˉsuccess(
        Build, [Project, Lowererˉwvb], 'compiler-build',
        CONSTRUCTION_TIMEOUT_MILLISECONDS,
    );
    process.stdout.write(
        'native unsafe write pointer lowering step=compiler-package status=Started\n',
    );
    await Requireˉsuccess(
        Packageˉlowerer,
        ['6', Lowererˉwvb, Lowerer, '--development-cache'],
        'compiler-package',
        CONSTRUCTION_TIMEOUT_MILLISECONDS,
    );
    if (!existsSync(Lowerer)) Reject('The current native lowerer was not published.');

    await Lowerˉexact(
        Lowerer,
        await Readˉbinary(
            join(Candidateˉdirectory, 'Return-42.wvb'), 174,
            '7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31',
        ),
        await Readˉbinary(
            join(Candidateˉdirectory, 'Return-42.wvo'), 479,
            '0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5',
        ),
        join(Work, 'Return-42'),
        'baseline-return-42',
    );
    await Lowerˉexact(
        Lowerer,
        await Readˉbinary(
            join(Candidateˉdirectory, 'Metadata.wvb'), 369,
            '94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa',
        ),
        await Readˉbinary(
            join(Candidateˉdirectory, 'Metadata.wvo'), 1151,
            '6f1cb53ec55448a7552f2ff5b380446964d16ed32a60aa28b8e55a9ca590845d',
        ),
        join(Work, 'Metadata'),
        'metadata',
    );

    const Foreignˉspecifications = [
        {
            Name: 'foreign-success',
            Input: Foreignˉsuccess,
            Codeˉbytes: 11_195,
            Objectˉbytes: 11_372,
            Sha256: 'dc0fccf525916d882dee3c6e000870c3d625ef36964c6cd0c69247385b3b89ef',
        },
        {
            Name: 'foreign-stale',
            Input: Foreignˉstale,
            Codeˉbytes: 11_366,
            Objectˉbytes: 11_543,
            Sha256: '8615a902c01f019460a3276b58ccff16f5fcbc329d6792dc5850eb30542dfa9b',
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
        'native unsafe write pointer lowering status=Passed cases=25 ' +
        'valid=5 malformed=20 native-execution=1 ' +
        'foreign-native-execution=linux-only foreign-links=2 compiler-source=current ' +
        'package-cache=development\n',
    );
} finally {
    await Removeˉwork(Work);
}

async function Readˉbinary(Path, Expectedˉsize, Expectedˉsha256) {
    const Result = await readFile(Path);
    const Digest = createHash('sha256').update(Result).digest('hex');
    if (Result.length !== Expectedˉsize || Result.length > FIXTURE_LIMIT ||
        Digest !== Expectedˉsha256) {
        Reject(`The fixture ${basename(Path)} identity differs.`);
    }
    return Result;
}

async function Lowerˉexact(Lowerer, Input, Expected, Prefix, Label) {
    process.stdout.write(
        `native unsafe write pointer lowering case=${Label} status=Started\n`,
    );
    const Source = `${Prefix}.wvb`;
    const Destination = `${Prefix}.wvo`;
    await writeFile(Source, Input, { flag: 'wx' });
    const Lowered = await Runˉprocess(
        Lowerer, [Source, Destination], COMMAND_TIMEOUT_MILLISECONDS, Label,
    );
    if (!Passed(Lowered) || !existsSync(Destination) ||
        !/^native x64 status=Valid abi=22 code-bytes=[0-9]+ object-bytes=[0-9]+\r?\n$/u
            .test(Lowered.Output)) {
        Reject(`The ${Label} lowering differed.\n${Lowered.Output}`);
    }
    const Actual = await readFile(Destination);
    if (!Actual.equals(Expected)) {
        Reject(`The ${Label} WVO bytes differed.`);
    }
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
        Reject(`The ${Label} step failed with exit ${Result.Code}.\n${Result.Output}`);
    }
    return Result;
}

function Runˉprocess(Tool, Arguments, Timeout, Step) {
    return new Promise((Resolveˉresult, Rejectˉpromise) => {
        const Isˉcommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        if (Isˉcommand && [Tool, ...Arguments].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument))) {
            Rejectˉpromise(new Error('A Windows test argument is unsafe.'));
            return;
        }
        const Executable = Isˉcommand ? process.env.ComSpec ?? 'cmd.exe' : Tool;
        const Toolˉarguments = Isˉcommand ? [
            '/d', '/v:off', '/s', '/c',
            `"${[Tool, ...Arguments].map(Value => `"${Value}"`).join(' ')}"`,
        ] : Arguments;
        const Child = spawn(Executable, Toolˉarguments, {
            cwd: Repositoryˉroot,
            detached: !WINDOWS,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: Isˉcommand,
        });
        const Output = [];
        let Outputˉbytes = 0;
        let Exceeded = false;
        let Timedˉout = false;
        const Capture = Chunk => {
            Outputˉbytes += Chunk.length;
            if (Outputˉbytes <= OUTPUT_LIMIT) Output.push(Chunk);
            else {
                Exceeded = true;
                Terminate(Child);
            }
        };
        Child.stdout.on('data', Capture);
        Child.stderr.on('data', Capture);
        Child.once('error', Rejectˉpromise);
        const Started = Date.now();
        const Progress = setInterval(() => {
            process.stdout.write(
                `native unsafe write pointer lowering step=${Step} ` +
                `status=Active elapsed-seconds=${Math.floor(
                    (Date.now() - Started) / 1000,
                )}\n`,
            );
        }, PROGRESS_INTERVAL_MILLISECONDS);
        Progress.unref();
        const Timer = setTimeout(() => {
            Timedˉout = true;
            Terminate(Child);
        }, Timeout);
        Child.once('close', Code => {
            clearInterval(Progress);
            clearTimeout(Timer);
            Resolveˉresult({
                Code,
                Output: Buffer.concat(Output).toString('utf8'),
                Exceeded,
                Timedˉout,
            });
        });
    });
}

function Terminate(Child) {
    if (Child.pid === undefined) return;
    if (WINDOWS) {
        const Killer = spawn(
            'taskkill.exe', ['/pid', String(Child.pid), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true },
        );
        Killer.unref();
    } else {
        try { process.kill(-Child.pid, 'SIGKILL'); } catch { Child.kill('SIGKILL'); }
    }
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
        '<windows|linux> <repository-root>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
