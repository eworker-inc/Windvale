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
const Check = join(Repositoryˉroot, 'Tools', 'Native', `Check-Wvo.${Extension}`);
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

const Work = await mkdtemp(join(tmpdir(), 'windvale-write-pointer-lowering-'));
try {
    const Canonical = await Readˉfixture(
        join(Fixtureˉdirectory, 'Unsafe-Write-Pointer.wvb.b64'),
        '289f9e338f7922e91be3526239bf5e06d9d5ef701d4d87a003d7fab14adec47f',
    );
    const Runtime = await Readˉfixture(
        join(Fixtureˉdirectory, 'Unsafe-Write-Pointer-Runtime.wvb.b64'),
        '3754236b188a99068bb3918dc581e27ba4215b0590286a7d62039d6254dd54e3',
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
    process.stdout.write(
        'native unsafe write pointer lowering status=Passed cases=13 ' +
        'valid=3 malformed=10 native-execution=1 compiler-source=current ' +
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
