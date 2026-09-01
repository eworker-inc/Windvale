import { createHash } from 'node:crypto';
import { access, mkdtemp, readFile, realpath, rm, stat, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const WINDOWS = process.platform === 'win32';
const MAXIMUM_OUTPUT_BYTES = 1024 * 1024;
const MAXIMUM_LEAF_BYTES = 262_144;
const MAXIMUM_APPLICATION_BYTES = 64 * 1024 * 1024;
const CHILD_TIMEOUT_MILLISECONDS = 180_000;
const CONSTRUCTION_TIMEOUT_MILLISECONDS = 10 * 60_000;
const PRODUCT_TIMEOUT_MILLISECONDS = 30_000;
const OVERALL_TIMEOUT_MILLISECONDS = 20 * 60_000;
const HEARTBEAT_MILLISECONDS = 30_000;
const TASKKILL_TIMEOUT_MILLISECONDS = 2_000;
const TERMINATION_SETTLE_MILLISECONDS = 5_000;
const SCRIPT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = resolve(SCRIPT_DIRECTORY, '..', '..');
const SELECTORS = [...'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQR'];
const DYNAMIC_CASES = 12;
const EXPECTED_RUNNER = WINDOWS
    ? { bytes: 5_907_456, sha256: '2721b80158cf4825919be5a6b5c58cfa40d417dc802d5bf27b2584b822ad817b' }
    : { bytes: 5_906_432, sha256: '611cfbf9fd95e9b29df4a38e3ac392dc9eea87b760b81ff572bad8af6f235eae' };
const EXPECTED_PINNED_ANALYZER = {
    bytes: 1_552_090,
    sha256: '5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77'
};
const EXPECTED_CORE = {
    bytes: 24_292,
    sha256: '5b76731abff311ff51dd2e302da8da7bfe8439250d5f32647bda5f0ee51f9537'
};
function Reject(Message) { throw new Error(Message); }

const OwnerStarted = Date.now();

function ProcessIsLive(Child) {
    return Child.pid !== undefined &&
        Child.exitCode === null && Child.signalCode === null;
}

function RunBoundedTaskkill(ProcessIdentifier) {
    return new Promise(ResolveResult => {
        const Killer = spawn(
            'taskkill.exe',
            ['/pid', String(ProcessIdentifier), '/t', '/f'],
            { stdio: 'ignore', windowsHide: true }
        );
        var Settled = false;
        const Timer = setTimeout(() => {
            if (Settled) return;
            Settled = true;
            Killer.kill('SIGKILL');
            Killer.unref();
            ResolveResult('taskkill did not settle within 2000 ms');
        }, TASKKILL_TIMEOUT_MILLISECONDS);
        Killer.once('error', ErrorValue => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            ResolveResult(`taskkill error: ${ErrorValue.message}`);
        });
        Killer.once('close', Code => {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timer);
            ResolveResult(Code === 0 ? null : `taskkill exited ${Code}`);
        });
    });
}

async function TerminateProcessTree(Child) {
    if (!ProcessIsLive(Child)) return null;
    var Diagnostic = null;
    if (WINDOWS) {
        Diagnostic = await RunBoundedTaskkill(Child.pid);
    }
    else {
        try {
            process.kill(-Child.pid, 'SIGKILL');
        } catch (ErrorValue) {
            Diagnostic = `process-group kill error: ${ErrorValue.message}`;
        }
    }
    if (ProcessIsLive(Child)) {
        try {
            if (!Child.kill('SIGKILL') && Diagnostic !== null) {
                Diagnostic += '; direct child kill returned false';
            }
        } catch (ErrorValue) {
            if (Diagnostic !== null) {
                Diagnostic += `; direct child kill error: ${ErrorValue.message}`;
            }
        }
    }
    return Diagnostic;
}

function SelectTimeout(RequestedTimeout, Remaining) {
    return {
        Milliseconds: Math.min(RequestedTimeout, Remaining),
        OwnerBudgetLimited: Remaining < RequestedTimeout
    };
}

function Run(Tool, Arguments, Label = 'child', RequestedTimeout = CHILD_TIMEOUT_MILLISECONDS) {
    return new Promise((Resolve, RejectPromise) => {
        const Remaining = OVERALL_TIMEOUT_MILLISECONDS - (Date.now() - OwnerStarted);
        if (Remaining <= 0) {
            RejectPromise(new Error(
                `Admission-evidence verification exceeded ${OVERALL_TIMEOUT_MILLISECONDS} ms.`
            ));
            return;
        }
        const TimeoutSelection = SelectTimeout(RequestedTimeout, Remaining);
        const TimeoutMilliseconds = TimeoutSelection.Milliseconds;
        const IsCommand = WINDOWS && Tool.toLowerCase().endsWith('.cmd');
        if (IsCommand && [Tool, ...Arguments].some(
            Argument => /[\r\n&|<>^%!"]/u.test(Argument)
        )) {
            RejectPromise(new Error(
                'An admission-evidence owner argument contains shell metacharacters.'
            ));
            return;
        }
        const Executable = IsCommand ? process.env.ComSpec ?? 'cmd.exe' : Tool;
        const ProducerArguments = IsCommand
            ? ['/d', '/v:off', '/s', '/c', `"${[Tool, ...Arguments].map(Value => `"${Value}"`).join(' ')}"`]
            : Arguments;
        const Started = Date.now();
        const Child = spawn(Executable, ProducerArguments, {
            cwd: REPOSITORY_ROOT,
            detached: !WINDOWS,
            stdio: ['ignore', 'pipe', 'pipe'],
            windowsHide: true,
            windowsVerbatimArguments: IsCommand
        });
        const Output = [];
        const ErrorOutput = [];
        var Bytes = 0;
        var Exceeded = false;
        var TimedOut = false;
        var Settled = false;
        var CleanupFailure = null;
        var TerminationDiagnostic = null;
        var TerminationPromise = null;
        var CloseCode = null;
        var CloseReceived = false;
        var SettleTimer;
        const Heartbeat = setInterval(() => {
            const ElapsedSeconds = Math.floor((Date.now() - OwnerStarted) / 1000);
            process.stdout.write(
                `HEARTBEAT admission evidence child=${Label} elapsed-seconds=${ElapsedSeconds}\n`
            );
        }, HEARTBEAT_MILLISECONDS);
        Heartbeat.unref();
        function Result(Code, Forced) {
            return {
                Code,
                CleanupFailure,
                Elapsed: Date.now() - Started,
                Error: Buffer.concat(ErrorOutput),
                Exceeded,
                Forced,
                Output: Buffer.concat(Output),
                TimeoutLimitedByOwnerBudget: TimeoutSelection.OwnerBudgetLimited,
                TimeoutMilliseconds,
                TimedOut
            };
        }
        function Complete(Code, Forced) {
            if (Settled) return;
            Settled = true;
            clearTimeout(Timeout);
            clearInterval(Heartbeat);
            if (SettleTimer !== undefined) clearTimeout(SettleTimer);
            Resolve(Result(Code, Forced));
        }
        function TerminateAndSettle() {
            if (TerminationPromise !== null) return;
            SettleTimer = setTimeout(() => {
                try {
                    Child.kill('SIGKILL');
                } catch (ErrorValue) {
                    TerminationDiagnostic = TerminationDiagnostic ??
                        `final direct child kill error: ${ErrorValue.message}`;
                }
                const SettleDiagnostic =
                    `process tree did not close within ` +
                    `${TERMINATION_SETTLE_MILLISECONDS} ms`;
                CleanupFailure = TerminationDiagnostic === null
                    ? SettleDiagnostic
                    : `${TerminationDiagnostic}; ${SettleDiagnostic}`;
                Child.stdout.destroy();
                Child.stderr.destroy();
                Child.unref();
                Complete(null, true);
            }, TERMINATION_SETTLE_MILLISECONDS);
            TerminationPromise = (async () => {
                try {
                    TerminationDiagnostic = await TerminateProcessTree(Child);
                } catch (ErrorValue) {
                    TerminationDiagnostic =
                        `tree termination error: ${ErrorValue.message}`;
                    try {
                        Child.kill('SIGKILL');
                    } catch {
                        // The bounded settle path records the diagnostic.
                    }
                }
                CleanupFailure = TerminationDiagnostic;
                if (CloseReceived) Complete(CloseCode, false);
            })();
        }
        const Timeout = setTimeout(() => {
            TimedOut = true;
            TerminateAndSettle();
        }, TimeoutMilliseconds);
        for (const [Stream, Target] of [[Child.stdout, Output], [Child.stderr, ErrorOutput]]) {
            Stream.on('data', Chunk => {
                Bytes += Chunk.length;
                if (Bytes <= MAXIMUM_OUTPUT_BYTES) Target.push(Chunk);
                else if (!Exceeded) {
                    Exceeded = true;
                    TerminateAndSettle();
                }
            });
        }
        Child.once('error', ErrorValue => {
            if (TimedOut || Exceeded) return;
            if (Settled) return;
            Settled = true;
            clearTimeout(Timeout);
            clearInterval(Heartbeat);
            if (SettleTimer !== undefined) clearTimeout(SettleTimer);
            RejectPromise(ErrorValue);
        });
        Child.once('close', Code => {
            CloseReceived = true;
            CloseCode = Code;
            if (TerminationPromise === null) {
                Complete(Code, false);
                return;
            }
            void TerminationPromise.then(() => Complete(Code, false));
        });
    });
}

function RequireCleanTermination(Result, Label) {
    if (Result.CleanupFailure !== null || Result.Forced) {
        Reject(
            `${Label} process cleanup failed: forced=${Result.Forced} ` +
            `diagnostic=${Result.CleanupFailure ?? 'none'}.`
        );
    }
}

function TimeoutDescription(Result) {
    const Scope = Result.TimeoutLimitedByOwnerBudget
        ? 'remaining overall child-execution budget'
        : 'requested child timeout';
    return `the ${Scope} of ${Result.TimeoutMilliseconds} ms`;
}

function ProcessIdentifierIsLive(ProcessIdentifier) {
    try {
        process.kill(ProcessIdentifier, 0);
        return true;
    } catch (ErrorValue) {
        if (ErrorValue.code === 'ESRCH') return false;
        throw ErrorValue;
    }
}

async function WaitForProcessExit(ProcessIdentifier) {
    const Deadline = Date.now() + 1_000;
    while (ProcessIdentifierIsLive(ProcessIdentifier)) {
        if (Date.now() >= Deadline) return false;
        await new Promise(ResolveWait => setTimeout(ResolveWait, 25));
    }
    return true;
}

async function RunTerminationProbe() {
    const RequestedSelection = SelectTimeout(500, 1_000);
    const OwnerSelection = SelectTimeout(500, 100);
    if (RequestedSelection.Milliseconds !== 500 ||
        RequestedSelection.OwnerBudgetLimited ||
        OwnerSelection.Milliseconds !== 100 ||
        !OwnerSelection.OwnerBudgetLimited) {
        Reject('The timeout-selection contract is invalid.');
    }
    const DescendantSource = 'setInterval(()=>{},1000)';
    const Source =
        "const{spawn}=require('node:child_process');" +
        `const c=spawn(process.execPath,['-e',${JSON.stringify(
            DescendantSource
        )}],{stdio:'ignore'});` +
        'process.stdout.write(String(c.pid));setInterval(()=>{},1000)';
    const Result = await Run(
        process.execPath,
        ['-e', Source],
        'termination-probe',
        500
    );
    if (!Result.TimedOut || Result.Exceeded || Result.Forced ||
        Result.CleanupFailure !== null) {
        Reject(
            `The termination probe returned an invalid result: ` +
            `timed-out=${Result.TimedOut} exceeded=${Result.Exceeded} ` +
            `forced=${Result.Forced} cleanup=${Result.CleanupFailure}.`
        );
    }
    const DescendantText = Result.Output.toString('utf8');
    if (!/^[1-9][0-9]*$/u.test(DescendantText)) {
        Reject('The termination probe descendant identity is invalid.');
    }
    const DescendantIdentifier = Number.parseInt(DescendantText, 10);
    if (!await WaitForProcessExit(DescendantIdentifier)) {
        Reject('The termination probe left its descendant running.');
    }
    process.stdout.write(
        `admission evidence process termination probe status=Passed ` +
        `elapsed-ms=${Result.Elapsed}\n`
    );
}

async function RequireSuccess(
    Label,
    Tool,
    Arguments,
    PermitOutput = false,
    TimeoutMilliseconds = CHILD_TIMEOUT_MILLISECONDS
) {
    const Result = await Run(
        Tool, Arguments, Label.replaceAll(' ', '-'), TimeoutMilliseconds
    );
    RequireCleanTermination(Result, Label);
    if (Result.TimedOut) Reject(`${Label} exceeded ${TimeoutDescription(Result)}.`);
    if (Result.Exceeded) Reject(`${Label} exceeded the output limit.`);
    if (Result.Code !== 0 || Result.Error.length !== 0 ||
        (!PermitOutput && Result.Output.length !== 0)) {
        Reject(`${Label} failed with exit ${Result.Code}.\n` +
            Result.Error.toString('utf8') + Result.Output.toString('utf8'));
    }
    return Result.Output.toString('utf8');
}

function Normalize(Value) { return Value.toString('utf8').replaceAll('\r', ''); }

function Sha256(Value) { return createHash('sha256').update(Value).digest(); }

function ConstructWvss() {
    const Result = Buffer.alloc(37);
    Result.write('WVSS', 0, 4, 'ascii');
    Result.writeUInt16LE(2, 4);
    Result.writeUInt16LE(0, 6);
    Result.writeUInt32LE(1, 8);
    Result.writeUInt32LE(20, 12);
    Result.writeUInt32LE(36, 16);
    Result.writeUInt32LE(1, 20);
    Result.writeUInt32LE(1, 24);
    Result.writeUInt32LE(1, 28);
    Result.writeUInt32LE(1, 32);
    Result[36] = 120;
    return Result;
}

function ConstructWvtd() {
    const Result = Buffer.alloc(64);
    Result.write('WVTD', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt16LE(0, 6);
    Result.writeUInt32LE(Result.length, 8);
    Result.writeUInt32LE(1, 12);
    Result.writeUInt32LE(1, 16);
    Result.writeUInt32LE(1, 20);
    Result.writeUInt32LE(1, 24);
    Result.writeUInt32LE(64, 28);
    Result.writeUInt32LE(1, 32);
    return Result;
}

function ConstructWvfc(ModuleCount = 1) {
    const Result = Buffer.alloc(48);
    Result.write('WVFC', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt16LE(0, 6);
    Result.writeUInt32LE(Result.length, 8);
    Result.writeUInt32LE(0, 12);
    Result.writeUInt32LE(96, 16);
    Result.writeUInt32LE(48, 20);
    Result.writeUInt32LE(ModuleCount, 24);
    return Result;
}

function ConstructWvae(Wvss, Wvtd, Wvfc, Lock, Profile) {
    const Result = Buffer.alloc(224);
    Result.write('WVAE', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt16LE(0, 6);
    Result.writeUInt32LE(Result.length, 8);
    Result.writeUInt32LE(1, 12);
    Result.writeUInt32LE(0, 16);
    Result.writeUInt32LE(Wvss.length, 20);
    Result.writeUInt32LE(Wvtd.length, 24);
    Result.writeUInt32LE(Wvfc.length, 28);
    Result.writeUInt32LE(1, 32);
    Result.writeUInt32LE(0, 36);
    Result.writeUInt32LE(1, 40);
    Result.writeUInt32LE(1, 44);
    for (const [Offset, Snapshot] of [
        [64, Wvss], [96, Wvtd], [128, Wvfc], [160, Lock], [192, Profile]
    ]) Sha256(Snapshot).copy(Result, Offset);
    return Result;
}

async function WriteSnapshot(Path, Value) {
    if (!Buffer.isBuffer(Value) || Value.length > 4_194_304) {
        Reject(`Refusing to write an invalid dynamic snapshot: ${basename(Path)}.`);
    }
    await writeFile(Path, Value, { flag: 'wx' });
}

async function RequireProductCase(Application, Index, Name, Arguments, Code, Output, Error) {
    const Result = await Run(
        Application,
        Arguments,
        `product-case-${Index}-${Name}`,
        PRODUCT_TIMEOUT_MILLISECONDS
    );
    RequireCleanTermination(Result, `Product case ${Index} (${Name})`);
    if (Result.TimedOut) {
        Reject(`Product case ${Index} (${Name}) exceeded ${TimeoutDescription(Result)}.`);
    }
    if (Result.Exceeded) Reject(`Product case ${Index} (${Name}) exceeded the output limit.`);
    if (Result.Code !== Code || Normalize(Result.Output) !== Output ||
        Normalize(Result.Error) !== Error) {
        Reject(
            `Product case ${Index} (${Name}) differed: exit=${Result.Code} ` +
            `stdout=${JSON.stringify(Normalize(Result.Output))} ` +
            `stderr=${JSON.stringify(Normalize(Result.Error))}.`
        );
    }
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

const ProbeOnly = process.argv.length === 3 &&
    process.argv[2] === '--termination-probe';
if (!ProbeOnly && process.argv.length !== 2) {
    Reject('The admission-evidence owner accepts no arguments.');
}
await RunTerminationProbe();
if (!ProbeOnly) {
const TemporaryRoot = resolve(tmpdir());
const Work = await mkdtemp(join(TemporaryRoot, 'windvale-admission-evidence-'));
var Passed = false;
try {
    const Extension = WINDOWS ? 'cmd' : 'sh';
    const Build = join(SCRIPT_DIRECTORY, `Build-Current-Wvb.${Extension}`);
    const Package = join(SCRIPT_DIRECTORY, `Package-Segmented-Compiler-Wvb.${Extension}`);
    const Runner = join(
        REPOSITORY_ROOT, 'Artifacts', 'Native-Wvb-Runner-Candidate',
        WINDOWS ? 'windows-x64-wvrun.exe' : 'linux-x64-wvrun.elf'
    );
    const PinnedAnalyzerWvb = join(
        REPOSITORY_ROOT, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
        'Wvb', 'wvanalyze.wvb'
    );
    RequireExact(await Evidence(Runner), EXPECTED_RUNNER, 'WVB runner');
    RequireExact(
        await Evidence(PinnedAnalyzerWvb),
        EXPECTED_PINNED_ANALYZER,
        'pinned analyzer'
    );

    process.stdout.write('START admission evidence phase=boundaries item=1/6\n');
    await VerifySourceBoundaries();

    process.stdout.write('START admission evidence phase=capacity item=2/6\n');
    const Analyzer = join(Work, WINDOWS ? 'wvanalyze.exe' : 'wvanalyze.elf');
    await RequireSuccess('analyzer packaging', Package, [
        '7', PinnedAnalyzerWvb, Analyzer, '--development-cache'
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

    process.stdout.write('START admission evidence phase=build item=3/6\n');
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
    RequireExact(await Evidence(ValidatorB), ValidatorEvidence, 'deterministic validator WVB');
    const FixtureEvidence = await Evidence(Fixture);
    if (FixtureEvidence.bytes > MAXIMUM_LEAF_BYTES) Reject('Fixture WVB exceeds its leaf ceiling.');

    process.stdout.write('START admission evidence phase=package item=4/6\n');
    const ApplicationExtension = WINDOWS ? 'exe' : 'elf';
    const Validator = join(
        Work, `wvverify-admission-evidence.${ApplicationExtension}`
    );
    await RequireSuccess('validator packaging', Package, [
        '7', ValidatorA, Validator, '--development-cache'
    ], true, CONSTRUCTION_TIMEOUT_MILLISECONDS);
    const ValidatorApplication = await stat(Validator);
    if (!ValidatorApplication.isFile() || ValidatorApplication.size === 0 ||
        ValidatorApplication.size > MAXIMUM_APPLICATION_BYTES) {
        Reject('The packaged validator application exceeds its file bound.');
    }

    process.stdout.write('START admission evidence phase=product-cases item=5/6\n');
    const WvssBytes = ConstructWvss();
    const WvtdBytes = ConstructWvtd();
    const WvfcBytes = ConstructWvfc();
    const LockBytes = Buffer.from('lock snapshot\n', 'utf8');
    const ProfileBytes = Buffer.from('profile snapshot\n', 'utf8');
    const WvaeBytes = ConstructWvae(
        WvssBytes, WvtdBytes, WvfcBytes, LockBytes, ProfileBytes
    );
    const SnapshotPaths = {
        Wvae: join(Work, 'Evidence.wvae'),
        Wvss: join(Work, 'Admitted.wvss'),
        Wvtd: join(Work, 'Target.wvtd'),
        Wvfc: join(Work, 'Catalog.wvfc'),
        Lock: join(Work, 'Source-Inputs.wvlock'),
        Profile: join(Work, 'Source-Profile.wvsp')
    };
    await Promise.all([
        WriteSnapshot(SnapshotPaths.Wvae, WvaeBytes),
        WriteSnapshot(SnapshotPaths.Wvss, WvssBytes),
        WriteSnapshot(SnapshotPaths.Wvtd, WvtdBytes),
        WriteSnapshot(SnapshotPaths.Wvfc, WvfcBytes),
        WriteSnapshot(SnapshotPaths.Lock, LockBytes),
        WriteSnapshot(SnapshotPaths.Profile, ProfileBytes)
    ]);
    const ValidArguments = [
        SnapshotPaths.Wvae, SnapshotPaths.Wvss, SnapshotPaths.Wvtd,
        SnapshotPaths.Wvfc, SnapshotPaths.Lock, SnapshotPaths.Profile
    ];
    const Usage =
        'Usage: wvverify-admission-evidence <evidence.wvae> <admitted.wvss> ' +
        '<target.wvtd> <catalog.wvfc> <lock.wvlock> <profile.wvsp>\n';
    const Rejections = {
        TruncatedWvae:
            'admission-evidence validation status=Rejected phase=Input-WVAE ' +
            'evidence-status=Invalidˉlength offset=223\n',
        TrailingWvae:
            'admission-evidence validation status=Rejected phase=Input-WVAE ' +
            'evidence-status=Invalidˉlength offset=224\n',
        Wvss:
            'admission-evidence validation status=Rejected phase=WVSS2 ' +
            'structure-status=Noncanonicalˉlayout offset=16\n',
        Wvtd:
            'admission-evidence validation status=Rejected phase=WVTD ' +
            'structure-status=10 offset=56\n',
        Wvfc:
            'admission-evidence validation status=Rejected phase=WVFC ' +
            'structure-status=8 offset=28\n',
        ModuleCount:
            'admission-evidence validation status=Rejected phase=WVFC-WVSS ' +
            'structure-status=1 offset=24\n',
        Digest:
            'admission-evidence validation status=Rejected phase=WVAE ' +
            'evidence-status=Digestˉmismatch offset=64\n',
        Lock:
            'admission-evidence validation status=Rejected phase=Input-Lock ' +
            'evidence-status=Invalidˉlockˉlength offset=160\n',
        Profile:
            'admission-evidence validation status=Rejected phase=Input-Profile ' +
            'evidence-status=Invalidˉprofileˉlength offset=192\n'
    };
    const Mutations = {
        TruncatedWvae: WvaeBytes.subarray(0, 223),
        TrailingWvae: Buffer.concat([WvaeBytes, Buffer.from([0])]),
        Wvss: Buffer.from(WvssBytes),
        Wvtd: Buffer.from(WvtdBytes),
        Wvfc: Buffer.from(WvfcBytes),
        ModuleCount: ConstructWvfc(2),
        Digest: Buffer.from(WvaeBytes),
        Lock: Buffer.alloc(0),
        Profile: Buffer.alloc(0)
    };
    Mutations.Wvss.writeUInt32LE(37, 16);
    Mutations.Wvtd.writeUInt32LE(1, 56);
    Mutations.Wvfc.writeUInt32LE(1, 28);
    Mutations.Digest[64] ^= 1;
    const MutationPaths = {};
    for (const [Name, Value] of Object.entries(Mutations)) {
        const Path = join(Work, `Rejected-${Name}.bin`);
        await WriteSnapshot(Path, Value);
        MutationPaths[Name] = Path;
    }
    const With = (Position, Path) => {
        const Arguments = [...ValidArguments];
        Arguments[Position] = Path;
        return Arguments;
    };
    const ProductCases = [
        ['valid-six-snapshot-set', ValidArguments, 0,
            'admission-evidence validation status=Accepted format=WVAE-1\n', ''],
        ['missing-argument', ValidArguments.slice(0, 5), 64, '', Usage],
        ['surplus-argument', [...ValidArguments, SnapshotPaths.Wvae], 64, '', Usage],
        ['truncated-wvae', With(0, MutationPaths.TruncatedWvae), 1, '',
            Rejections.TruncatedWvae],
        ['trailing-wvae', With(0, MutationPaths.TrailingWvae), 1, '',
            Rejections.TrailingWvae],
        ['wvss-contiguity', With(1, MutationPaths.Wvss), 1, '', Rejections.Wvss],
        ['wvtd-inner-status', With(2, MutationPaths.Wvtd), 1, '', Rejections.Wvtd],
        ['wvfc-inner-status', With(3, MutationPaths.Wvfc), 1, '', Rejections.Wvfc],
        ['module-count-mismatch', With(3, MutationPaths.ModuleCount), 1, '',
            Rejections.ModuleCount],
        ['digest-mismatch', With(0, MutationPaths.Digest), 1, '', Rejections.Digest],
        ['lock-length', With(4, MutationPaths.Lock), 1, '', Rejections.Lock],
        ['profile-length', With(5, MutationPaths.Profile), 1, '', Rejections.Profile]
    ];
    if (ProductCases.length !== DYNAMIC_CASES) Reject('The dynamic case count differs.');
    for (const [Index, Case] of ProductCases.entries()) {
        await RequireProductCase(Validator, Index + 1, ...Case);
    }

    process.stdout.write('START admission evidence phase=portable-cases item=6/6\n');
    for (const [Index, Selector] of SELECTORS.entries()) {
        const Result = await Run(
            Runner,
            ['--script', Fixture, Selector],
            `portable-case-${Index + 1}`,
            PRODUCT_TIMEOUT_MILLISECONDS
        );
        RequireCleanTermination(Result, `Admission-evidence case ${Index + 1} (${Selector})`);
        if (Result.TimedOut) {
            Reject(
                `Admission-evidence case ${Index + 1} (${Selector}) exceeded ` +
                `${TimeoutDescription(Result)}.`
            );
        }
        if (Result.Exceeded) {
            Reject(`Admission-evidence case ${Index + 1} (${Selector}) exceeded the output limit.`);
        }
        if (Result.Code !== 42 ||
            Result.Output.length !== 0 || Result.Error.length !== 0) {
            Reject(`Admission-evidence case ${Index + 1} (${Selector}) returned ${Result.Code}.`);
        }
    }
    process.stdout.write(
        `PASS admission evidence cases=${SELECTORS.length + DYNAMIC_CASES} ` +
        `portable-selectors=${SELECTORS.length} dynamic-cases=${DYNAMIC_CASES} ` +
        `wvss-structure-subcases=15 execution=native-packaged ` +
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
    var TemporaryStillExists = true;
    try { await access(Work); } catch (Error) {
        if (Error.code !== 'ENOENT') throw Error;
        TemporaryStillExists = false;
    }
    if (TemporaryStillExists) Reject(`Temporary path remains after cleanup: ${Work}`);
}
if (!Passed) Reject('Admission-evidence verification did not complete.');
}
