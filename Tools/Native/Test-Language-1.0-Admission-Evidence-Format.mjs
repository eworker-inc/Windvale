import { createHash } from 'node:crypto';
import { mkdtemp, readFile, realpath, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 1024 * 1024;
const MAXIMUM_LEAF_BYTES = 262_144;
const CHILD_TIMEOUT_MILLISECONDS = 180_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SELECTORS = [...'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQR'];
const EXPECTED_RUNNER = WINDOWS
    ? { bytes: 5_907_456, sha256: '2721b80158cf4825919be5a6b5c58cfa40d417dc802d5bf27b2584b822ad817b' }
    : { bytes: 5_906_432, sha256: '611cfbf9fd95e9b29df4a38e3ac392dc9eea87b760b81ff572bad8af6f235eae' };
const EXPECTED_BOOTSTRAP_ANALYZER = {
    bytes: 992_412,
    sha256: '26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120'
};
const EXPECTED_CORE = {
    bytes: 24_292,
    sha256: '5b76731abff311ff51dd2e302da8da7bfe8439250d5f32647bda5f0ee51f9537'
};
const EXPECTED_VALIDATOR = {
    wvirBytes: 190_524,
    wvirSha256: '47c7eeb1680b6c58e791e38efaa457d90b069c31cda3aa32e8fba5fedc6ab878',
    wvbBytes: 72_060,
    wvbSha256: '868eb8c6b7fd27affad03844de2915a19a74167d75baf041e28e750111d178f4'
};
const EXPECTED_FIXTURE = {
    bytes: 94_299,
    sha256: '37772c1a75d03b2d8eb22015fde4efacbcc27718cd891f4486bc597317ebeee9'
};

function Reject(Message) { throw new Error(Message); }

function Run(Tool, Arguments) {
    return new Promise((Resolve, RejectPromise) => {
        const IsCommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        const Executable = IsCommand ? process.env.ComSpec ?? 'cmd.exe' : Tool;
        const ProducerArguments = IsCommand
            ? ['/d', '/v:off', '/s', '/c', `"${[Tool, ...Arguments].map(Value => `"${Value}"`).join(' ')}"`]
            : Arguments;
        const Child = spawn(Executable, ProducerArguments, {
            cwd: REPOSITORY_ROOT,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: IsCommand
        });
        const Output = [];
        const ErrorOutput = [];
        var Bytes = 0;
        var Exceeded = false;
        var TimedOut = false;
        const Timeout = setTimeout(() => {
            TimedOut = true;
            Child.kill('SIGKILL');
        }, CHILD_TIMEOUT_MILLISECONDS);
        for (const [Stream, Target] of [[Child.stdout, Output], [Child.stderr, ErrorOutput]]) {
            Stream.on('data', Chunk => {
                Bytes += Chunk.length;
                if (Bytes <= MAXIMUM_OUTPUT_BYTES) Target.push(Chunk);
                else { Exceeded = true; Child.kill(); }
            });
        }
        Child.once('error', Error => {
            clearTimeout(Timeout);
            RejectPromise(Error);
        });
        Child.once('close', Code => {
            clearTimeout(Timeout);
            Resolve({
                Code,
                Output: Buffer.concat(Output),
                Error: Buffer.concat(ErrorOutput),
                Exceeded,
                TimedOut
            });
        });
    });
}

async function RequireSuccess(Label, Tool, Arguments, PermitOutput = false) {
    const Result = await Run(Tool, Arguments);
    if (Result.TimedOut) Reject(`${Label} exceeded ${CHILD_TIMEOUT_MILLISECONDS} ms.`);
    if (Result.Exceeded) Reject(`${Label} exceeded the output limit.`);
    if (Result.Code !== 0 || Result.Error.length !== 0 ||
        (!PermitOutput && Result.Output.length !== 0)) {
        Reject(`${Label} failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8'));
    }
    return Result.Output.toString('utf8');
}

async function Evidence(Path) {
    const Value = await readFile(Path);
    return {
        bytes: Value.length,
        sha256: createHash('sha256').update(Value).digest('hex')
    };
}

function RequireExact(Actual, Expected, Label) {
    if (Actual.bytes !== Expected.bytes || Actual.sha256 !== Expected.sha256) {
        Reject(`${Label} identity is invalid: bytes=${Actual.bytes} sha256=${Actual.sha256}.`);
    }
}

async function VerifySourceBoundaries() {
    const AnalyzerProject = await readFile(join(
        REPOSITORY_ROOT, 'Projects', 'Tools',
        'Windvale-Compiler-Analysis-Driver.wvproj'
    ), 'utf8');
    const AnalyzerDriver = await readFile(join(
        REPOSITORY_ROOT, 'Tools', 'Windvale.Build',
        'Compiler-Analysis-Driver.wv'
    ), 'utf8');
    for (const Forbidden of [
        'Admission-Evidence-Core.wv',
        'Admission-Evidence-Validator-Core.wv',
        'Admission-Source-Set-Core.wv',
        'Compilerˉadmissionˉevidence',
        'Compilerˉadmissionˉevidenceˉvalidator',
        'Compilerˉadmissionˉsourceˉset'
    ]) {
        if (AnalyzerProject.includes(Forbidden) || AnalyzerDriver.includes(Forbidden)) {
            Reject(`The complete analyzer closure includes forbidden admission leaf ${Forbidden}.`);
        }
    }
    const ValidatorDriver = await readFile(join(
        REPOSITORY_ROOT, 'Tools', 'Windvale.Build',
        'Compiler-Admission-Evidence-Validator-Driver.wv'
    ), 'utf8');
    const Reads = [0, 2, 4, 5, 1, 3].map(Index =>
        ValidatorDriver.indexOf(`file.read_bytes(process.argument(${Index}u32))`)
    );
    if (Reads.some(Index => Index < 0) ||
        Reads.some((Index, Position) => Position !== 0 && Index <= Reads[Position - 1])) {
        Reject('The bounded sequential snapshot-read order is invalid.');
    }
    for (const [Position, Read] of Reads.entries()) {
        const End = Position + 1 < Reads.length ? Reads[Position + 1] : ValidatorDriver.length;
        const Segment = ValidatorDriver.slice(Read, End);
        if (!Segment.includes(
            'Compilerˉadmissionˉevidenceˉretainedˉadditionˉisˉbounded'
        ) || !Segment.includes('Retainedˉbytes = Retainedˉbytes +')) {
            Reject(`Snapshot read ${Position + 1} is not followed by its retained-input gate.`);
        }
    }
    for (const Mapping of [
        'Invalidˉlength',
        'Invalidˉwvssˉlength',
        'Invalidˉwvtdˉlength',
        'Invalidˉwvfcˉlength',
        'Invalidˉlockˉlength',
        'Invalidˉprofileˉlength'
    ]) {
        if (!ValidatorDriver.includes(
            `Evidence.Compilerˉadmissionˉevidenceˉstatus.${Mapping}`
        )) Reject(`The hosted input diagnostic mapping ${Mapping} is absent.`);
    }
    if (!ValidatorDriver.includes('Rejectˉresourceˉlimit')) {
        Reject('The hosted cumulative retained-input rejection is absent.');
    }
}

const TemporaryRoot = resolve(tmpdir());
const Work = await mkdtemp(join(TemporaryRoot, 'windvale-admission-evidence-'));
var Passed = false;
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Wvb.${Extension}`);
    const Package = join(SCRIPT_DIRECTORY, `Package-Segmented-Compiler-Wvb.${Extension}`);
    const Runner = join(
        REPOSITORY_ROOT, 'Artifacts', 'Native-Wvb-Runner-Candidate',
        WINDOWS ? 'windows-x64-wvrun.exe' : 'linux-x64-wvrun.elf'
    );
    const BootstrapAnalyzerWvb = join(
        REPOSITORY_ROOT, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
        'Wvb', 'wvanalyze.wvb'
    );
    RequireExact(await Evidence(Runner), EXPECTED_RUNNER, 'WVB runner');
    RequireExact(
        await Evidence(BootstrapAnalyzerWvb),
        EXPECTED_BOOTSTRAP_ANALYZER,
        'bootstrap analyzer'
    );

    process.stdout.write('START admission evidence phase=boundaries item=1/4\n');
    await VerifySourceBoundaries();

    process.stdout.write('START admission evidence phase=capacity item=2/4\n');
    const Analyzer = join(Work, WINDOWS ? 'wvanalyze.exe' : 'wvanalyze.elf');
    await RequireSuccess('analyzer packaging', Package, [
        '7', BootstrapAnalyzerWvb, Analyzer, '--development-cache'
    ], true);
    const CapacityRoot = 'Tools/Windvale.Build/Compiler-Admission-Evidence-Validator-Driver.wv';
    const CapacitySources = [
        'Compiler/Windvale/Admission-Evidence-Core.wv',
        'Compiler/Windvale/Admission-Evidence-Validator-Core.wv',
        'Compiler/Windvale/Admission-Source-Set-Core.wv',
        'Compiler/Windvale/Source-Descriptor-Core.wv',
        'Compiler/Windvale/Source-Foreign-Catalog-Core.wv',
        'Compiler/Windvale/Source-Target-Core.wv'
    ];
    const Wvss = join(Work, 'Source.wvss');
    const Wvca = join(Work, 'Source.wvca');
    const Wvlb = join(Work, 'Source.wvlb');
    const Wvir = join(Work, 'Source.wvir');
    await RequireSuccess('validator WVIR capacity', Analyzer, [
        CapacityRoot, ...CapacitySources, Wvss, Wvca, Wvlb, Wvir
    ], true);
    const WvirEvidence = await Evidence(Wvir);
    if (WvirEvidence.bytes > MAXIMUM_LEAF_BYTES) Reject('Validator WVIR exceeds its leaf ceiling.');
    RequireExact(WvirEvidence, {
        bytes: EXPECTED_VALIDATOR.wvirBytes,
        sha256: EXPECTED_VALIDATOR.wvirSha256
    }, 'validator WVIR');

    process.stdout.write('START admission evidence phase=build item=3/4\n');
    const ValidatorProject = join(
        REPOSITORY_ROOT, 'Projects', 'Tools',
        'Windvale-Compiler-Admission-Evidence-Validator.wvproj'
    );
    const FixtureProject = join(
        REPOSITORY_ROOT, 'Projects', 'Tests',
        'Windvale-Native-Test-Language-1-Admission-Evidence.wvproj'
    );
    const CoreProject = join(
        REPOSITORY_ROOT, 'Projects', 'Compiler',
        'Windvale-Admission-Evidence-Core.wvproj'
    );
    const Core = join(Work, 'Core.wvb');
    const ValidatorA = join(Work, 'Validator-A.wvb');
    const ValidatorB = join(Work, 'Validator-B.wvb');
    const Fixture = join(Work, 'Fixture.wvb');
    await RequireSuccess('core build', Build, [CoreProject, Core], true);
    await RequireSuccess('validator build A', Build, [ValidatorProject, ValidatorA], true);
    await RequireSuccess('validator build B', Build, [ValidatorProject, ValidatorB], true);
    await RequireSuccess('fixture build', Build, [FixtureProject, Fixture], true);
    const CoreEvidence = await Evidence(Core);
    if (CoreEvidence.bytes > MAXIMUM_LEAF_BYTES) Reject('Core WVB exceeds its leaf ceiling.');
    RequireExact(CoreEvidence, EXPECTED_CORE, 'admission-evidence core WVB');
    const ValidatorEvidence = await Evidence(ValidatorA);
    if (ValidatorEvidence.bytes > MAXIMUM_LEAF_BYTES) Reject('Validator WVB exceeds its leaf ceiling.');
    RequireExact(ValidatorEvidence, {
        bytes: EXPECTED_VALIDATOR.wvbBytes,
        sha256: EXPECTED_VALIDATOR.wvbSha256
    }, 'validator WVB');
    RequireExact(await Evidence(ValidatorB), ValidatorEvidence, 'deterministic validator WVB');
    RequireExact(await Evidence(Fixture), EXPECTED_FIXTURE, 'admission-evidence fixture');

    process.stdout.write('START admission evidence phase=cases item=4/4\n');
    for (const [Index, Selector] of SELECTORS.entries()) {
        const Result = await Run(Runner, ['--script', Fixture, Selector]);
        if (Result.TimedOut || Result.Code !== 42 ||
            Result.Output.length !== 0 || Result.Error.length !== 0) {
            Reject(`Admission-evidence case ${Index + 1} (${Selector}) returned ${Result.Code}.`);
        }
    }
    process.stdout.write(
        `PASS admission evidence cases=${SELECTORS.length} wvss-structure-subcases=15 ` +
        `core-wvb-bytes=${CoreEvidence.bytes} ` +
        `wvir-bytes=${WvirEvidence.bytes} wvb-bytes=${ValidatorEvidence.bytes}\n`
    );
    Passed = true;
} finally {
    const RealRoot = await realpath(TemporaryRoot);
    const RealParent = await realpath(dirname(Work));
    if (RealParent !== RealRoot || !basename(Work).startsWith('windvale-admission-evidence-')) {
        Reject(`Refusing to remove unexpected temporary path: ${Work}`);
    }
    await rm(Work, { recursive: true, force: false, maxRetries: 2 });
}
if (!Passed) Reject('Admission-evidence verification did not complete.');
