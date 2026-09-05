import { spawnSync } from 'node:child_process';
import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { createHash } from 'node:crypto';
import {
    lstatSync,
    mkdtempSync,
    readFileSync,
    realpathSync,
    rmSync,
    writeFileSync,
} from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_DIAGNOSTIC_BYTES = 1_048_576;
const MAXIMUM_WVB_BYTES = 16_777_216;
const TOOL_TIMEOUT_MILLISECONDS = 600_000;
const SOURCE_LOCK_SHA256 =
    '9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e';
const PINNED_ANALYZER_SHA256 =
    '5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77';
const PINNED_EMITTER_SHA256 =
    'd16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9';
const EXPECTED_SUCCESS_SHA256 =
    '5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53';
const EXPECTED_VECTOR_SUCCESS_SHA256 =
    '881bcbabc9620188964a63601490ad81acf63587f70501443d97447cdd45f7c5';
const EXPECTED_APPEND_SUCCESS_SHA256 =
    '6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f';
const EXPECTED_GROW_SUCCESS_SHA256 =
    '30de39bdd12ad7718ad1fb465b14bc42f8463b6ecfc6ba1f10494cb6e67c5b59';
const EXPECTED_OWNED_CALL_SUCCESS_SHA256 =
    'ab79d05bb03afddbe6430adc127c8cdf084ea6499b16e3e25ebb3e477c408387';
const EXPECTED_OWNED_AGGREGATE_SUCCESS_SHA256 =
    'b9810655b33c79cf980ea05f7fbca5511d3c34219f37e1b6a046a630a3e1c395';
const EXPECTED_USING_FALLTHROUGH_SHA256 =
    'f541cd186564d1e696820a53c4a17baf50ba0d393dbb4bc8b1c381960b595257';
const EXPECTED_USING_NESTED_SHA256 =
    'e0c6bc8e2d31b9322dbbfd23c9b88fe5cb2ba820423c7fdb7a447a8e43380a1c';
const EXPECTED_USING_TRY_SHA256 =
    '7ac802bc273d671672a25b28281294ee43c7af935b1fd9fa736292e695bdd192';
const EXPECTED_USING_LOOP_SHA256 =
    'ad44bd9eef0daf17d8dab0952b6af223e17395557de6844f56757285ec3bf0fe';
const EXPECTED_SOURCE_FILE_SHA256 =
    '01065b752d7ea6d64e3bf36bdd4d8a0d2e5b7faf6794de173580003ed3935d05';
const EXPECTED_STRUCTURED_TASK_SHA256 =
    '11a2bed917a9a30dc12fc565b0cc93e2731ee8b48c8bd2b6d1f54ebe97a145c8';
const EXPECTED_STRUCTURED_TASK_TRAP_SHA256 =
    'bba9d62f5b4999d9648a4ecba527c881877a765c41a886c22f2da5ae716a5f5b';
const EXPECTED_STRUCTURED_TASK_RETAINED_RESULT_SHA256 =
    'd6f941feaccfbc8a4aaa694d0c746f1850051da5251ce8204731735ee6695c94';
const EXPECTED_STRUCTURED_TASK_WORK_LIMIT_SHA256 =
    'b15cc21926e43b048fc4fe79d28febb7778bbacaf6dbbaa84c2855fb1cff10a2';
const EXPECTED_STRUCTURED_TASK_CALL_DEPTH_LIMIT_SHA256 =
    '3817f6d39346e3154845ff422ba54e1dd58dbeac4d00123f5904a0b22525d351';
const EXPECTED_STRUCTURED_TASK_MEMORY_LIMIT_SHA256 =
    '92c1c521d4bd1a3198ff01dd54a97fb5153170afe009b6c0111ce06aba51fb64';
const EXPECTED_STRUCTURED_TASK_FOUR_CHILD_CANCELLATION_SHA256 =
    'b4d9c67cee803da4fb53ef21a57ccbdf9ecc410c54c369262f3c2187599df88c';
const EXPECTED_STRUCTURED_TASK_COMPLETION_ORDER_SHA256 =
    '6b6eb29ae5b711358e582c42d2667ab21c0861ac1ca5b1bc70b3ab575711c80c';
const EXPECTED_STRUCTURED_TASK_PROVIDER_RECOVERY_SHA256 =
    'eb8dc8047fd2ddd7e7eb98c7e443396ac5e9d240fabb060acb88769888d4f067';
const EXPECTED_STRUCTURED_TASK_ENVIRONMENT_SHA256 =
    'a2dbb84ef197d10e32286a0bd38971072e200c964a6d620975fde49ba2bcb090';

const Inspectionˉmode = process.argv.length === 4 ? process.argv[2] : '';
const Foundationˉonly = (process.argv.length === 3 || process.argv.length === 5) &&
    process.argv[2] === '--foundation-borrow';
const Foundationˉplanˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-plan';
const Foundationˉdirectoriesˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-directories';
const Foundationˉownersˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-owners';
const Developmentˉonly = Foundationˉonly || Foundationˉplanˉonly ||
    Foundationˉdirectoriesˉonly || Foundationˉownersˉonly;
let Maximumˉrunˉmilliseconds = TOOL_TIMEOUT_MILLISECONDS;
if (Foundationˉonly && process.argv.length === 5) {
    if (process.argv[3] !== '--maximum-seconds' || !/^[1-9][0-9]{0,3}$/u.test(process.argv[4]) ||
        Number(process.argv[4]) > 3600) {
        process.stderr.write('The explicit development maximum must be 1 through 3600 seconds.\n');
        process.exit(64);
    }
    Maximumˉrunˉmilliseconds = Number(process.argv[4]) * 1000;
}
const Started = Date.now();
const Inspectionˉonly =
    Inspectionˉmode === '--inspect-structured-task' ||
    Inspectionˉmode === '--inspect-function-limits';
if (process.argv.length !== 2 && !Inspectionˉonly && !Developmentˉonly) {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs ' +
        '[--foundation-borrow-plan|--foundation-borrow-directories|--foundation-borrow-owners|' +
        '--foundation-borrow [--maximum-seconds <seconds>]|' +
        '(--inspect-structured-task|--inspect-function-limits) <module.wvb>]\n',
    );
    process.exit(64);
}
if (process.platform !== 'win32' && process.platform !== 'linux') {
    Reject(`Unsupported test host: ${process.platform}.`);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
if (Inspectionˉonly) {
    const Candidate = path.resolve(process.argv[3]);
    Requireˉordinaryˉfile(
        Candidate,
        MAXIMUM_WVB_BYTES,
        'structured-task inspection module',
    );
    const Bytes = readFileSync(Candidate);
    if (Inspectionˉmode === '--inspect-structured-task') {
        const Layout = Inspectˉstructuredˉtaskˉmodule(Bytes);
        process.stdout.write(
            'structured task inspection status=Valid ' +
            `construct=${Layout.construct} context=${Layout.context} ` +
            `spawn=${Layout.spawn} await=${Layout.await} exit=${Layout.exit}\n`,
        );
    } else {
        const Entries = Parseˉfunctionˉentries(Bytes, Parseˉsections(Bytes)[4]);
        let Largest = Entries[0];
        for (const Entry of Entries) {
            if (Entry.codeLength > Largest.codeLength) Largest = Entry;
        }
        process.stdout.write(
            'function limits inspection status=Valid ' +
            `functions=${Entries.length} largest-index=${Largest.index} ` +
            `largest-name=${Largest.name} code-bytes=${Largest.codeLength} ` +
            `parameters=${Largest.parameterCount} locals=${Largest.localCount} ` +
            `total-slots=${Largest.parameterCount + Largest.localCount} ` +
            `maximum-stack=${Largest.maximumStack}\n`,
        );
    }
    process.exit(0);
}
const Profileˉroot = path.join(
    Repositoryˉroot,
    'Documents', 'Project', 'Language-1.0-Localization-Workloads',
    '01-Source-Profile-Admission', 'Reference-Artifacts',
);
const Sourceˉlock = path.join(Profileˉroot, 'Source-Inputs.wvlock');
const Sourceˉprofile = path.join(Profileˉroot, 'En-Source-Profile.wvsp');
const Pinnedˉanalyzerˉwvb = path.join(
    Repositoryˉroot, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb', 'wvanalyze.wvb',
);
const Pinnedˉemitterˉwvb = path.join(
    Repositoryˉroot, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb', 'wvemit.wvb',
);
const Temporaryˉallocationˉroot = realpathSync.native(os.tmpdir());
const Work = realpathSync.native(mkdtempSync(path.join(
    Temporaryˉallocationˉroot, 'windvale-memory-budget-split-execution-',
)));
const Temporaryˉroot = path.dirname(Work);
let Step = 0;
let Validator = null;
let Targetˉdescriptor = null;
let Borrowˉplanˉbytes = null;
let Borrowˉdirectoryˉbytes = null;
let Borrowˉownerˉbytes = null;

try {
    if (!Foundationˉdirectoriesˉonly && !Foundationˉownersˉonly) await Runˉfoundationˉplan();
    if (!Foundationˉplanˉonly && !Foundationˉownersˉonly) await Runˉfoundationˉdirectories();
    if (!Foundationˉplanˉonly && !Foundationˉdirectoriesˉonly) await Runˉfoundationˉowners();
    if (!Foundationˉplanˉonly && !Foundationˉdirectoriesˉonly && !Foundationˉownersˉonly) {
        await Runˉpublicationˉandˉexecution();
    }
} finally {
    const Resolved = path.resolve(Work);
    if (path.dirname(Resolved) !== Temporaryˉroot ||
        !path.basename(Resolved).startsWith('windvale-memory-budget-split-execution-')) {
        Reject(`Refusing to remove unexpected test directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}
if (Developmentˉonly) {
    const Elapsed = Date.now() - Started;
    if (Elapsed > Maximumˉrunˉmilliseconds) {
        Reject('The focused Foundation borrow development budget expired during cleanup.');
    }
    process.stdout.write(
        `native language 1 foundation borrow development status=Passed cases=${Foundationˉonly ? 81 : Foundationˉplanˉonly ? 16 : Foundationˉdirectoriesˉonly ? 24 : 18} ` +
        `selection=${Foundationˉonly ? 'publication' : Foundationˉplanˉonly ? 'plan' : Foundationˉdirectoriesˉonly ? 'directories' : 'owners'} qualification=false candidate-execution=false ` +
        (Borrowˉplanˉbytes === null ? '' :
            `plan-wvb-bytes=${Borrowˉplanˉbytes.length} plan-wvb-sha256=${Digest(Borrowˉplanˉbytes)} `) +
        (Borrowˉdirectoryˉbytes === null ? '' :
            `directory-wvb-bytes=${Borrowˉdirectoryˉbytes.length} directory-wvb-sha256=${Digest(Borrowˉdirectoryˉbytes)} `) +
        (Borrowˉownerˉbytes === null ? '' :
            `owner-wvb-bytes=${Borrowˉownerˉbytes.length} owner-wvb-sha256=${Digest(Borrowˉownerˉbytes)} `) +
        `elapsed-ms=${Elapsed}\n`,
    );
}

async function Runˉfoundationˉplan() {
    const Planˉwvb = path.join(Work, 'Borrow-Plan.wvb');
    await Runˉnative('foundation-borrow-plan-build', 'Build-Cached-Project-Wvb', [
        Testˉproject('Windvale-Native-Test-Foundation-Value-Borrow-Plan.wvproj'), Planˉwvb,
    ]);
    Borrowˉplanˉbytes = readFileSync(Planˉwvb);
    const Planˉapplication = path.join(Work, `Borrow-Plan.${process.platform === 'win32' ? 'exe' : 'elf'}`);
    await Runˉnative('foundation-borrow-plan-package', 'Package-Segmented-Compiler-Wvb', [
        '1', Planˉwvb, Planˉapplication, '--development-cache',
    ]);
    const Planˉresult = await Run('foundation-borrow-plan-execute', Planˉapplication, [], 42);
    if (Planˉresult !== '') Reject('The Foundation borrow plan self-test emitted unexpected output.');
}

async function Runˉfoundationˉdirectories() {
    const Directoryˉwvb = path.join(Work, 'Borrow-Directories.wvb');
    await Runˉnative('foundation-borrow-directories-build', 'Build-Cached-Project-Wvb', [
        Testˉproject('Windvale-Native-Test-Wvb-Typed-Directories.wvproj'), Directoryˉwvb,
    ]);
    Borrowˉdirectoryˉbytes = readFileSync(Directoryˉwvb);
    const Application = path.join(Work, `Borrow-Directories.${process.platform === 'win32' ? 'exe' : 'elf'}`);
    await Runˉnative('foundation-borrow-directories-package', 'Package-Segmented-Compiler-Wvb', [
        '1', Directoryˉwvb, Application, '--development-cache',
    ]);
    const Result = await Run('foundation-borrow-directories-execute', Application, [], 42);
    if (Result !== '') Reject('The WVB typed-directory self-test emitted unexpected output.');
}

async function Runˉfoundationˉowners() {
    const Fixture = path.join(Repositoryˉroot,
        'Tests', 'Fixtures', 'Source-Wvb', 'Foundation-Owner-Flow-Self-Test.wv');
    Requireˉordinaryˉfile(Fixture, 32_768, 'Foundation owner-flow self-test');
    const Match = /^data Candidate: bytes = \[([0-9,\s]+)\];$/mu.exec(readFileSync(Fixture, 'utf8'));
    if (Match === null) Reject('The owner-flow published candidate snapshot is missing.');
    const Values = Match[1].trim().split(/\s*,\s*/u).map(Number);
    if (Values.length !== 1966 || Values.some(Value => !Number.isInteger(Value) || Value < 0 || Value > 255) ||
        Digest(Buffer.from(Values)) !== '470df34f087a5e52674c7d24f51a0734e56759193756962df0805c6f4792b821') {
        Reject('The owner-flow published candidate snapshot identity differs.');
    }
    const Wvb = path.join(Work, 'Borrow-Owners.wvb');
    await Runˉnative('foundation-borrow-owners-build', 'Build-Cached-Project-Wvb', [
        Testˉproject('Windvale-Native-Test-Foundation-Owner-Flow.wvproj'), Wvb,
    ]);
    Borrowˉownerˉbytes = readFileSync(Wvb);
    const Application = path.join(Work, `Borrow-Owners.${process.platform === 'win32' ? 'exe' : 'elf'}`);
    await Runˉnative('foundation-borrow-owners-package', 'Package-Segmented-Compiler-Wvb', [
        '1', Wvb, Application, '--development-cache',
    ]);
    const Result = await Run('foundation-borrow-owners-execute', Application, [], 42);
    if (Result !== '') Reject('The Foundation owner-flow self-test emitted unexpected output.');
}

async function Runˉpublicationˉandˉexecution() {
    Requireˉordinaryˉfile(Sourceˉlock, 4_194_304, 'source lock');
    Requireˉordinaryˉfile(Sourceˉprofile, 4_194_304, 'source profile');
    Requireˉexactˉfile(
        Pinnedˉanalyzerˉwvb, 1_552_090, PINNED_ANALYZER_SHA256,
        'pinned analyzer',
    );
    Requireˉexactˉfile(
        Pinnedˉemitterˉwvb, 1_556_434, PINNED_EMITTER_SHA256,
        'pinned emitter',
    );

    const Executableˉsuffix = process.platform === 'win32' ? '.exe' : '.elf';
    const Hostˉtarget = process.platform === 'win32' ? 'windows' : 'linux';
    const Pinnedˉanalyzer = path.join(Work, `Pinned-Analyzer${Executableˉsuffix}`);
    const Pinnedˉemitter = path.join(Work, `Pinned-Emitter${Executableˉsuffix}`);
    const Pinnedˉanalyzerˉidentity = path.join(Work, 'Pinned-Analyzer.identity');
    const Pinnedˉemitterˉidentity = path.join(Work, 'Pinned-Emitter.identity');
    const Admitterˉwvb = path.join(Work, 'Admitter.wvb');
    const Validatorˉwvb = path.join(Work, 'Validator.wvb');
    const Analyzerˉwvb = path.join(Work, 'Analyzer.wvb');
    const Emitterˉwvb = path.join(Work, 'Emitter.wvb');
    const Admitter = path.join(Work, `Admitter${Executableˉsuffix}`);
    Validator = path.join(Work, `Validator${Executableˉsuffix}`);
    const Analyzer = path.join(Work, `Analyzer${Executableˉsuffix}`);
    const Emitter = path.join(Work, `Emitter${Executableˉsuffix}`);
    const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
    const Emitterˉidentity = path.join(Work, 'Emitter.identity');
    const Ownedˉaggregateˉsuccessˉa = path.join(
        Work, 'Owned-Aggregate-Success-A.wvb',
    );
    const Foundationˉvalueˉborrowˉa = path.join(
        Work, 'Foundation-Value-Borrow-A.wvb',
    );
    const Foundationˉvalueˉborrowˉb = path.join(
        Work, 'Foundation-Value-Borrow-B.wvb',
    );
    Targetˉdescriptor = path.join(Work, 'Target.wvtd');
    writeFileSync(Targetˉdescriptor, Constructˉtargetˉdescriptor(), { flag: 'wx' });

    await Runˉnative('pinned-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Pinnedˉanalyzerˉwvb, Pinnedˉanalyzer, '--development-cache',
    ]);
    await Runˉnative('pinned-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Pinnedˉemitterˉwvb, Pinnedˉemitter, '--development-cache',
    ]);
    await Runˉnode('pinned-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Pinnedˉanalyzer, Pinnedˉanalyzerˉidentity,
    ]);
    await Runˉnode('pinned-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnode('current-admitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Admission-Driver.wvproj'), Admitterˉwvb,
        Pinnedˉanalyzer, Pinnedˉanalyzerˉidentity,
        Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnode('current-validator-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Source-Authenticator.wvproj'),
        Validatorˉwvb,
        Pinnedˉanalyzer, Pinnedˉanalyzerˉidentity,
        Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnode('current-analyzer-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Analysis-Driver.wvproj'), Analyzerˉwvb,
        Pinnedˉanalyzer, Pinnedˉanalyzerˉidentity,
        Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnative('current-admitter-package', 'Package-Segmented-Compiler-Wvb', [
        '2', Admitterˉwvb, Admitter, '--development-cache',
    ]);
    await Runˉnative('current-validator-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Validatorˉwvb, Validator, '--development-cache',
    ]);
    await Runˉnative('current-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Analyzerˉwvb, Analyzer, '--development-cache',
    ]);
    await Runˉnode('current-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Analyzer, Analyzerˉidentity,
    ]);
    await Runˉnode('current-emitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Emission-Driver.wvproj'), Emitterˉwvb,
        Analyzer, Analyzerˉidentity,
        Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnative('current-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Emitterˉwvb, Emitter, '--development-cache',
    ]);
    await Runˉnode('current-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Emitter, Emitterˉidentity,
    ]);
    if (!Foundationˉonly) {
        await Runˉnode(
            'async-call-await-conformance',
            'Verify-Language-1.0-Async-Call-Await.mjs',
            [
                Admitter, Validator, Analyzer, Emitter,
                Sourceˉlock, SOURCE_LOCK_SHA256, Sourceˉprofile,
                Targetˉdescriptor, Work,
            ],
        );
        await Runˉnode(
            'owned-vector-calls-and-joins-wir',
            'Verify-Language-1.0-Owned-Vector-Calls-Wir.mjs',
            [
                Admitter, Validator, Analyzer, Emitter,
                Targetˉdescriptor, Work, Ownedˉaggregateˉsuccessˉa,
            ],
        );
        await Runˉnode(
            'using-semantics-wir',
            'Verify-Language-1.0-Using-Wir.mjs',
            [Admitter, Validator, Analyzer, Emitter, Targetˉdescriptor, Work],
        );
    }
    await Compileˉfoundationˉvalueˉborrow(
        'foundation-value-borrow-a-compile', Admitter, Analyzer, Emitter,
        Foundationˉvalueˉborrowˉa,
    );
    await Compileˉfoundationˉvalueˉborrow(
        'foundation-value-borrow-b-compile', Admitter, Analyzer, Emitter,
        Foundationˉvalueˉborrowˉb,
    );
    await Runˉnode(
        'foundation-value-borrow-wvb-inspection',
        'Verify-Language-1.0-Foundation-Value-Borrow-Wvb.mjs',
        [Foundationˉvalueˉborrowˉa, Foundationˉvalueˉborrowˉb],
    );
    await Verifyˉlargeˉborrowˉfreeˉfunctions(Admitter, Analyzer, Emitter, Pinnedˉemitter);

    if (Foundationˉonly) {
        const Legacy = path.join(Work, 'Unchanged-Memory-Budget.wvb');
        await Compile('unchanged-earlier-bytecode', Admitter, Analyzer, Emitter,
            'Memory-Budget-Split-Executable.wv', Legacy);
        const Bytes = readFileSync(Legacy);
        if (Bytes.length !== 752 || Digest(Bytes) !== EXPECTED_SUCCESS_SHA256) {
            Reject('An unaffected earlier WVB contract changed.');
        }
        return;
    }
    const Successˉa = path.join(Work, 'Success-A.wvb');
    const Successˉb = path.join(Work, 'Success-B.wvb');
    const Failure = path.join(Work, 'Failure.wvb');
    const Vectorˉsuccessˉa = path.join(Work, 'Vector-Success-A.wvb');
    const Vectorˉsuccessˉb = path.join(Work, 'Vector-Success-B.wvb');
    const Vectorˉfailure = path.join(Work, 'Vector-Failure.wvb');
    const Vectorˉzero = path.join(Work, 'Vector-Zero.wvb');
    const Appendˉsuccessˉa = path.join(Work, 'Append-Success-A.wvb');
    const Appendˉsuccessˉb = path.join(Work, 'Append-Success-B.wvb');
    const Growˉsuccessˉa = path.join(Work, 'Grow-Success-A.wvb');
    const Growˉsuccessˉb = path.join(Work, 'Grow-Success-B.wvb');
    const Ownedˉcallˉsuccessˉa = path.join(Work, 'Owned-Call-Success-A.wvb');
    const Ownedˉcallˉsuccessˉb = path.join(Work, 'Owned-Call-Success-B.wvb');
    const Ownedˉaggregateˉsuccessˉb = path.join(
        Work, 'Owned-Aggregate-Success-B.wvb',
    );
    const Usingˉfallthrough = path.join(Work, 'Using-Fallthrough.wvb');
    const Usingˉnested = path.join(Work, 'Using-Nested.wvb');
    const Usingˉtry = path.join(Work, 'Using-Try.wvb');
    const Usingˉloop = path.join(Work, 'Using-Loop.wvb');
    const Sourceˉfileˉa = path.join(Work, 'Source-File-A.wvb');
    const Sourceˉfileˉb = path.join(Work, 'Source-File-B.wvb');
    const Structuredˉtaskˉa = path.join(Work, 'Structured-Task-A.wvb');
    const Structuredˉtaskˉb = path.join(Work, 'Structured-Task-B.wvb');
    const Structuredˉtaskˉtrap = path.join(Work, 'Structured-Task-Trap.wvb');
    const Structuredˉtaskˉretainedˉresult = path.join(
        Work, 'Structured-Task-Retained-Result.wvb',
    );
    const Structuredˉtaskˉworkˉlimit = path.join(
        Work, 'Structured-Task-Work-Limit.wvb',
    );
    const Structuredˉtaskˉcallˉdepthˉlimit = path.join(
        Work, 'Structured-Task-Call-Depth-Limit.wvb',
    );
    const Structuredˉtaskˉmemoryˉlimit = path.join(
        Work, 'Structured-Task-Memory-Limit.wvb',
    );
    const Structuredˉtaskˉfourˉchildˉcancellation = path.join(
        Work, 'Structured-Task-Four-Child-Cancellation.wvb',
    );
    const Structuredˉtaskˉcompletionˉorder = path.join(
        Work, 'Structured-Task-Completion-Order.wvb',
    );
    const Structuredˉtaskˉproviderˉrecovery = path.join(
        Work, 'Structured-Task-Provider-Recovery.wvb',
    );
    const Structuredˉtaskˉenvironment = path.join(
        Work, 'Structured-Task-Environment.wvb',
    );
    const Structuredˉtaskˉruntimeˉselfˉtest = path.join(
        Work, 'Structured-Task-Runtime-Self-Test.wvb',
    );
    const Structuredˉtaskˉruntimeˉselfˉtestˉexecutable = path.join(
        Work,
        process.platform === 'win32'
            ? 'Structured-Task-Runtime-Self-Test.exe'
            : 'Structured-Task-Runtime-Self-Test.elf',
    );
    await Compile('success-a-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Executable.wv', Successˉa);
    await Compile('success-b-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Executable.wv', Successˉb);
    await Compile('failure-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Failure-Executable.wv', Failure);
    await Compileˉvector('vector-success-a-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Executable.wv', Vectorˉsuccessˉa);
    await Compileˉvector('vector-success-b-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Executable.wv', Vectorˉsuccessˉb);
    await Compileˉvector('vector-failure-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Failure-Executable.wv', Vectorˉfailure);
    await Compileˉvector('vector-zero-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Zero-Executable.wv', Vectorˉzero);
    await Compileˉvector('vector-append-success-a-compile', Admitter, Analyzer, Emitter,
        'Vector-Append-Executable.wv', Appendˉsuccessˉa);
    await Compileˉvector('vector-append-success-b-compile', Admitter, Analyzer, Emitter,
        'Vector-Append-Executable.wv', Appendˉsuccessˉb);
    await Compileˉvector('vector-grow-success-a-compile', Admitter, Analyzer, Emitter,
        'Vector-Grow-Reserved-Executable.wv', Growˉsuccessˉa);
    await Compileˉvector('vector-grow-success-b-compile', Admitter, Analyzer, Emitter,
        'Vector-Grow-Reserved-Executable.wv', Growˉsuccessˉb);
    await Compileˉvector('owned-call-success-a-compile', Admitter, Analyzer, Emitter,
        'Owned-Vector-Calls-And-Joins-Wir.wv', Ownedˉcallˉsuccessˉa);
    await Compileˉvector('owned-call-success-b-compile', Admitter, Analyzer, Emitter,
        'Owned-Vector-Calls-And-Joins-Wir.wv', Ownedˉcallˉsuccessˉb);
    await Compileˉvector(
        'owned-aggregate-success-b-compile', Admitter, Analyzer, Emitter,
        'Owned-Aggregate-Vector-Executable.wv', Ownedˉaggregateˉsuccessˉb,
    );
    await Compileˉvector('using-fallthrough-compile', Admitter, Analyzer, Emitter,
        'Using-Vector-Fallthrough-Wir.wv', Usingˉfallthrough);
    await Compileˉvector('using-nested-compile', Admitter, Analyzer, Emitter,
        'Using-Vector-Nested-Return-Wir.wv', Usingˉnested);
    await Compileˉvector('using-try-compile', Admitter, Analyzer, Emitter,
        'Using-Vector-Try-Propagation-Wir.wv', Usingˉtry);
    await Compileˉvector('using-loop-compile', Admitter, Analyzer, Emitter,
        'Using-Vector-Loop-Exits-Wir.wv', Usingˉloop);
    await Compileˉsourceˉfile(
        'source-file-a-compile', Admitter, Analyzer, Emitter, Sourceˉfileˉa,
    );
    await Compileˉsourceˉfile(
        'source-file-b-compile', Admitter, Analyzer, Emitter, Sourceˉfileˉb,
    );
    await Compileˉtask(
        'structured-task-a-compile', Admitter, Analyzer, Emitter,
        'Structured-Tasks-Executable.wv', Structuredˉtaskˉa,
    );
    await Compileˉtask(
        'structured-task-b-compile', Admitter, Analyzer, Emitter,
        'Structured-Tasks-Executable.wv', Structuredˉtaskˉb,
    );
    await Compileˉtask(
        'structured-task-trap-compile', Admitter, Analyzer, Emitter,
        'Structured-Task-Trap-Executable.wv', Structuredˉtaskˉtrap,
    );
    await Compileˉtask(
        'structured-task-retained-result-compile', Admitter, Analyzer, Emitter,
        'Structured-Task-Retained-Result-Executable.wv',
        Structuredˉtaskˉretainedˉresult,
    );
    await Compileˉtask(
        'structured-task-work-limit-compile', Admitter, Analyzer, Emitter,
        'Structured-Task-Work-Limit-Executable.wv',
        Structuredˉtaskˉworkˉlimit,
    );
    await Compileˉtask(
        'structured-task-call-depth-limit-compile',
        Admitter, Analyzer, Emitter,
        'Structured-Task-Call-Depth-Limit-Executable.wv',
        Structuredˉtaskˉcallˉdepthˉlimit,
    );
    await Compileˉtask(
        'structured-task-memory-limit-compile',
        Admitter, Analyzer, Emitter,
        'Structured-Task-Memory-Limit-Executable.wv',
        Structuredˉtaskˉmemoryˉlimit,
    );
    await Compileˉtask(
        'structured-task-four-child-cancellation-compile',
        Admitter, Analyzer, Emitter,
        'Structured-Task-Four-Child-Cancellation-Executable.wv',
        Structuredˉtaskˉfourˉchildˉcancellation,
    );
    await Compileˉtask(
        'structured-task-completion-order-compile',
        Admitter, Analyzer, Emitter,
        'Structured-Task-Completion-Order-Executable.wv',
        Structuredˉtaskˉcompletionˉorder,
    );
    const Structuredˉtaskˉcompletionˉorderˉbytes = readFileSync(
        Structuredˉtaskˉcompletionˉorder,
    );
    Requireˉexactˉdigest(
        Structuredˉtaskˉcompletionˉorderˉbytes,
        EXPECTED_STRUCTURED_TASK_COMPLETION_ORDER_SHA256,
        'structured-task completion-order fixture',
    );
    await Compileˉtask(
        'structured-task-provider-recovery-compile',
        Admitter, Analyzer, Emitter,
        'Structured-Task-Provider-Recovery-Executable.wv',
        Structuredˉtaskˉproviderˉrecovery,
    );
    const Structuredˉtaskˉproviderˉrecoveryˉbytes = readFileSync(
        Structuredˉtaskˉproviderˉrecovery,
    );
    Requireˉexactˉdigest(
        Structuredˉtaskˉproviderˉrecoveryˉbytes,
        EXPECTED_STRUCTURED_TASK_PROVIDER_RECOVERY_SHA256,
        'structured-task provider-recovery fixture',
    );
    await Compileˉtask(
        'structured-task-environment-compile',
        Admitter, Analyzer, Emitter,
        'Structured-Task-Environment-Executable.wv',
        Structuredˉtaskˉenvironment,
    );
    const Structuredˉtaskˉenvironmentˉbytes = readFileSync(
        Structuredˉtaskˉenvironment,
    );
    Inspectˉstructuredˉtaskˉmodule(Structuredˉtaskˉenvironmentˉbytes);
    Requireˉexactˉdigest(
        Structuredˉtaskˉenvironmentˉbytes,
        EXPECTED_STRUCTURED_TASK_ENVIRONMENT_SHA256,
        'structured-task environment fixture',
    );
    await Runˉnode(
        'structured-task-runtime-self-test-build',
        'Build-Cached-Split-Project-Wvb.mjs',
        [
            Testˉproject(
                'Windvale-Native-Test-Language-1-Structured-Task-Runtime.wvproj',
            ),
            Structuredˉtaskˉruntimeˉselfˉtest,
            Analyzer, Analyzerˉidentity,
            Emitter, Emitterˉidentity,
        ],
    );
    await Runˉnative(
        'structured-task-runtime-self-test-package',
        'Package-Hosted-Wvb',
        [
            '1',
            Structuredˉtaskˉruntimeˉselfˉtest,
            Structuredˉtaskˉruntimeˉselfˉtestˉexecutable,
            Hostˉtarget,
        ],
    );
    Requireˉexitˉ42(
        Structuredˉtaskˉruntimeˉselfˉtestˉexecutable,
        'structured-task runtime core self-test',
    );
    const Successˉbytes = readFileSync(Successˉa);
    const Successˉbˉbytes = readFileSync(Successˉb);
    if (!Successˉbytes.equals(Successˉbˉbytes)) {
        Reject('The executable Split fixture is not deterministic.');
    }
    const Layout = Inspectˉexactˉmodule(Successˉbytes);
    const Successˉsha256 = Digest(Successˉbytes);
    if (Successˉsha256 !== EXPECTED_SUCCESS_SHA256) {
        Reject(`The executable Split fixture digest differs: ${Successˉsha256}.`);
    }
    Inspectˉexactˉmodule(readFileSync(Failure), false);
    const Vectorˉsuccessˉbytes = readFileSync(Vectorˉsuccessˉa);
    if (!Vectorˉsuccessˉbytes.equals(readFileSync(Vectorˉsuccessˉb))) {
        Reject('The executable Vector construction fixture is not deterministic.');
    }
    if (Vectorˉsuccessˉbytes.readUInt16LE(6) !== 24) {
        Reject('The executable Vector construction fixture is not WVB 1.24.');
    }
    const Vectorˉsha256 = Digest(Vectorˉsuccessˉbytes);
    if (Vectorˉsha256 !== EXPECTED_VECTOR_SUCCESS_SHA256) {
        Reject(`The executable Vector fixture digest differs: ${Vectorˉsha256}.`);
    }
    const Vectorˉlayout = Inspectˉexactˉvectorˉmodule(Vectorˉsuccessˉbytes);
    Inspectˉexactˉvectorˉmodule(readFileSync(Vectorˉfailure), false);
    Inspectˉexactˉvectorˉmodule(readFileSync(Vectorˉzero), false);
    const Appendˉsuccessˉbytes = readFileSync(Appendˉsuccessˉa);
    if (!Appendˉsuccessˉbytes.equals(readFileSync(Appendˉsuccessˉb))) {
        Reject('The executable Vector append fixture is not deterministic.');
    }
    if (Appendˉsuccessˉbytes.readUInt16LE(6) !== 25) {
        Reject('The executable Vector append fixture is not WVB 1.25.');
    }
    const Appendˉsha256 = Digest(Appendˉsuccessˉbytes);
    if (Appendˉsha256 !== EXPECTED_APPEND_SUCCESS_SHA256) {
        Reject(`The executable Vector append fixture digest differs: ${Appendˉsha256}.`);
    }
    const Appendˉlayout = Inspectˉexactˉappendˉmodule(Appendˉsuccessˉbytes);
    const Growˉsuccessˉbytes = readFileSync(Growˉsuccessˉa);
    if (!Growˉsuccessˉbytes.equals(readFileSync(Growˉsuccessˉb))) {
        Reject('The executable Vector growth fixture is not deterministic.');
    }
    const Growˉsha256 = Digest(Growˉsuccessˉbytes);
    if (Growˉsha256 !== EXPECTED_GROW_SUCCESS_SHA256) {
        Reject(`The executable Vector growth fixture digest differs: ${Growˉsha256}.`);
    }
    const Growˉlayout = Inspectˉexactˉgrowˉmodule(Growˉsuccessˉbytes);
    const Ownedˉcallˉsuccessˉbytes = readFileSync(Ownedˉcallˉsuccessˉa);
    if (!Ownedˉcallˉsuccessˉbytes.equals(readFileSync(Ownedˉcallˉsuccessˉb))) {
        Reject('The executable owned Vector call fixture is not deterministic.');
    }
    const Ownedˉcallˉlayout = Inspectˉownedˉcallˉmodule(
        Ownedˉcallˉsuccessˉbytes,
    );
    const Ownedˉcallˉsha256 = Digest(Ownedˉcallˉsuccessˉbytes);
    if (Ownedˉcallˉsha256 !== EXPECTED_OWNED_CALL_SUCCESS_SHA256) {
        Reject(`The executable owned Vector call fixture digest differs: ${Ownedˉcallˉsha256}.`);
    }
    const Ownedˉaggregateˉsuccessˉbytes = readFileSync(
        Ownedˉaggregateˉsuccessˉa,
    );
    if (!Ownedˉaggregateˉsuccessˉbytes.equals(
        readFileSync(Ownedˉaggregateˉsuccessˉb),
    )) {
        Reject('The executable owned aggregate fixture is not deterministic.');
    }
    const Ownedˉaggregateˉlayout = Inspectˉownedˉaggregateˉmodule(
        Ownedˉaggregateˉsuccessˉbytes,
    );
    const Ownedˉaggregateˉsha256 = Digest(Ownedˉaggregateˉsuccessˉbytes);
    if (Ownedˉaggregateˉsha256 !== EXPECTED_OWNED_AGGREGATE_SUCCESS_SHA256) {
        Reject(
            'The executable owned aggregate fixture digest differs: ' +
            `${Ownedˉaggregateˉsha256}.`,
        );
    }
    const Usingˉfallthroughˉbytes = readFileSync(Usingˉfallthrough);
    const Usingˉnestedˉbytes = readFileSync(Usingˉnested);
    const Usingˉtryˉbytes = readFileSync(Usingˉtry);
    const Usingˉloopˉbytes = readFileSync(Usingˉloop);
    Requireˉusingˉidentity(
        Usingˉfallthroughˉbytes, 1211, EXPECTED_USING_FALLTHROUGH_SHA256,
        'Main', [3], 'fallthrough',
    );
    Requireˉusingˉidentity(
        Usingˉnestedˉbytes, 945, EXPECTED_USING_NESTED_SHA256,
        'Exercise', [3, 2], 'nested return',
    );
    Requireˉusingˉidentity(
        Usingˉtryˉbytes, 1100, EXPECTED_USING_TRY_SHA256,
        'Exercise', [2, 2], 'try propagation',
    );
    const Usingˉloopˉlayout = Requireˉusingˉidentity(
        Usingˉloopˉbytes, 1027, EXPECTED_USING_LOOP_SHA256,
        'Exercise', [1, 1], 'loop exits', 2,
    );
    const Sourceˉfileˉbytes = readFileSync(Sourceˉfileˉa);
    if (!Sourceˉfileˉbytes.equals(readFileSync(Sourceˉfileˉb))) {
        Reject('The source-file snapshot fixture is not deterministic.');
    }
    const Sourceˉfileˉlayout = Inspectˉsourceˉfileˉmodule(
        Sourceˉfileˉbytes,
    );
    const Sourceˉfileˉsha256 = Digest(Sourceˉfileˉbytes);
    if (Sourceˉfileˉsha256 !== EXPECTED_SOURCE_FILE_SHA256) {
        Reject(
            'The source-file snapshot fixture digest differs: ' +
            `${Sourceˉfileˉsha256}.`,
        );
    }
    const Structuredˉtaskˉbytes = readFileSync(Structuredˉtaskˉa);
    if (!Structuredˉtaskˉbytes.equals(readFileSync(Structuredˉtaskˉb))) {
        Reject('The structured-task fixture is not deterministic.');
    }
    const Structuredˉtaskˉlayout = Inspectˉstructuredˉtaskˉmodule(
        Structuredˉtaskˉbytes,
    );
    Requireˉexactˉdigest(
        Structuredˉtaskˉbytes, EXPECTED_STRUCTURED_TASK_SHA256,
        'structured-task fixture',
    );
    Requireˉexactˉdigest(
        readFileSync(Structuredˉtaskˉtrap),
        EXPECTED_STRUCTURED_TASK_TRAP_SHA256,
        'structured-task trap fixture',
    );
    Requireˉexactˉdigest(
        readFileSync(Structuredˉtaskˉretainedˉresult),
        EXPECTED_STRUCTURED_TASK_RETAINED_RESULT_SHA256,
        'structured-task retained-result fixture',
    );
    Requireˉexactˉdigest(
        readFileSync(Structuredˉtaskˉworkˉlimit),
        EXPECTED_STRUCTURED_TASK_WORK_LIMIT_SHA256,
        'structured-task work-limit fixture',
    );
    Requireˉexactˉdigest(
        readFileSync(Structuredˉtaskˉcallˉdepthˉlimit),
        EXPECTED_STRUCTURED_TASK_CALL_DEPTH_LIMIT_SHA256,
        'structured-task call-depth-limit fixture',
    );
    Requireˉexactˉdigest(
        readFileSync(Structuredˉtaskˉmemoryˉlimit),
        EXPECTED_STRUCTURED_TASK_MEMORY_LIMIT_SHA256,
        'structured-task memory-limit fixture',
    );
    Requireˉexactˉdigest(
        readFileSync(Structuredˉtaskˉfourˉchildˉcancellation),
        EXPECTED_STRUCTURED_TASK_FOUR_CHILD_CANCELLATION_SHA256,
        'structured-task four-child cancellation fixture',
    );
    const Verifierˉwvb = path.join(Work, 'Verifier.wvb');
    const Verifier = path.join(Work, `Verifier${Executableˉsuffix}`);
    await Runˉnative('verifier-build', 'Build-Wvb', [
        Project('Windvale-Compiler-Wvb-Verifier.wvproj'), Verifierˉwvb,
    ]);
    await Runˉnative('verifier-package', 'Package-Hosted-Wvb', [
        '2', Verifierˉwvb, Verifier, Hostˉtarget,
    ]);
    Requireˉvalid(Verifier, Successˉa, 'successful Split module');
    Requireˉvalid(Verifier, Failure, 'refused Split module');
    Requireˉvalid(Verifier, Vectorˉsuccessˉa, 'successful Vector module');
    Requireˉvalid(Verifier, Vectorˉfailure, 'refused Vector module');
    Requireˉvalid(Verifier, Vectorˉzero, 'zero-maximum Vector module');
    Requireˉvalid(Verifier, Appendˉsuccessˉa, 'Vector append module');
    Requireˉvalid(Verifier, Growˉsuccessˉa, 'Vector growth module');
    Requireˉvalid(Verifier, Ownedˉcallˉsuccessˉa, 'owned Vector call module');
    Requireˉvalid(
        Verifier, Ownedˉaggregateˉsuccessˉa, 'owned aggregate module',
    );
    Requireˉvalid(Verifier, Usingˉfallthrough, 'using fallthrough module');
    Requireˉvalid(Verifier, Usingˉnested, 'using nested-return module');
    Requireˉvalid(Verifier, Usingˉtry, 'using try-propagation module');
    Requireˉvalid(Verifier, Usingˉloop, 'using loop-exit module');
    Requireˉvalid(Verifier, Sourceˉfileˉa, 'source-file snapshot module');
    Requireˉvalid(Verifier, Structuredˉtaskˉa, 'structured-task module');
    Requireˉvalid(
        Verifier, Structuredˉtaskˉtrap, 'structured-task trap module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉretainedˉresult,
        'structured-task retained-result module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉworkˉlimit,
        'structured-task work-limit module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉcallˉdepthˉlimit,
        'structured-task call-depth-limit module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉmemoryˉlimit,
        'structured-task memory-limit module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉfourˉchildˉcancellation,
        'structured-task four-child cancellation module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉcompletionˉorder,
        'structured-task completion-order module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉproviderˉrecovery,
        'structured-task provider-recovery module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉruntimeˉselfˉtest,
        'structured-task runtime self-test module',
    );
    Requireˉvalid(
        Verifier, Structuredˉtaskˉenvironment,
        'structured-task environment module',
    );

    const Malformedˉcases = [
        ['version-downgrade', Bytes => Bytes.writeUInt16LE(22, 6)],
        ['unknown-split-opcode', Bytes => { Bytes[Layout.opcode] = 207; }],
        ['entry-budget-parent', Bytes => Bytes.writeUInt32LE(0, Layout.opcode + 1)],
        ['non-budget-parent', Bytes => Bytes.writeUInt32LE(2, Layout.opcode + 1)],
        ['missing-result-type', Bytes => Bytes.writeUInt32LE(3, Layout.opcode + 5)],
        ['record-result-type', Bytes => Bytes.writeUInt32LE(0, Layout.opcode + 5)],
        ['primitive-valid-payload', Bytes => {
            Bytes[Layout.validPayloadShape] = 1;
        }],
        ['budget-in-failure-record', Bytes => {
            Bytes[Layout.requestedBytesShape] = 25;
        }],
        ['wrong-allocation-field', Bytes => {
            Bytes[Layout.availableBytesShape] = 5;
        }],
    ];
    for (const [Name, Mutate] of Malformedˉcases) {
        const Candidate = Buffer.from(Successˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Vectorˉmalformedˉcases = [
        ['vector-version-downgrade', Bytes => Bytes.writeUInt16LE(23, 6)],
        ['vector-unknown-opcode', Bytes => {
            Bytes[Vectorˉlayout.opcode] = 208;
        }],
        ['vector-non-budget-local', Bytes => {
            Bytes.writeUInt32LE(1, Vectorˉlayout.opcode + 1);
        }],
        ['vector-missing-budget-local', Bytes => {
            Bytes.writeUInt32LE(4, Vectorˉlayout.opcode + 1);
        }],
        ['vector-missing-result-type', Bytes => {
            Bytes.writeUInt32LE(99, Vectorˉlayout.opcode + 5);
        }],
        ['vector-record-result-type', Bytes => {
            Bytes.writeUInt32LE(0, Vectorˉlayout.opcode + 5);
        }],
        ['vector-primitive-valid-payload', Bytes => {
            Bytes[Vectorˉlayout.validPayloadShape] = 1;
        }],
        ['vector-wrong-valid-type', Bytes => {
            Bytes.writeUInt32LE(1, Vectorˉlayout.validPayloadShape + 1);
        }],
        ['vector-budget-in-failure-record', Bytes => {
            Bytes[Vectorˉlayout.requestedBytesShape] = 25;
        }],
        ['vector-wrong-allocation-field', Bytes => {
            Bytes[Vectorˉlayout.availableBytesShape] = 5;
        }],
    ];
    for (const [Name, Mutate] of Vectorˉmalformedˉcases) {
        const Candidate = Buffer.from(Vectorˉsuccessˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Appendˉmalformedˉcases = [
        ['append-version-downgrade', Bytes => Bytes.writeUInt16LE(24, 6)],
        ['append-unknown-opcode', Bytes => {
            Bytes[Appendˉlayout.opcodes[0]] = 207;
        }],
        ['append-parameter-local', Bytes => {
            Bytes.writeUInt32LE(0, Appendˉlayout.opcodes[0] + 1);
        }],
        ['append-non-vector-local', Bytes => {
            Bytes.writeUInt32LE(2, Appendˉlayout.opcodes[0] + 1);
        }],
        ['append-missing-local', Bytes => {
            Bytes.writeUInt32LE(99, Appendˉlayout.opcodes[0] + 1);
        }],
        ['append-missing-result-type', Bytes => {
            Bytes.writeUInt32LE(99, Appendˉlayout.opcodes[0] + 5);
        }],
        ['append-record-result-type', Bytes => {
            Bytes.writeUInt32LE(0, Appendˉlayout.opcodes[0] + 5);
        }],
        ['append-non-unit-valid-payload', Bytes => {
            Bytes[Appendˉlayout.validPayloadShape] = 1;
        }],
        ['append-wrong-result-failure-type', Bytes => {
            Bytes.writeUInt32LE(0, Appendˉlayout.resultFailureType);
        }],
        ['append-primitive-failure-error', Bytes => {
            Bytes[Appendˉlayout.failureErrorShape] = 1;
        }],
        ['append-wrong-failure-value', Bytes => {
            Bytes[Appendˉlayout.failureValueShape] = 2;
        }],
        ['append-wrong-capacity-field', Bytes => {
            Bytes[Appendˉlayout.capacityMaximumShape] = 5;
        }],
    ];
    for (const [Name, Mutate] of Appendˉmalformedˉcases) {
        const Candidate = Buffer.from(Appendˉsuccessˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Growˉmalformedˉcases = [
        ['grow-version-downgrade', Bytes => Bytes.writeUInt16LE(26, 6)],
        ['grow-unknown-opcode', Bytes => {
            Bytes[Growˉlayout.opcodes[0]] = 210;
        }],
        ['grow-vector-parameter', Bytes => {
            Bytes.writeUInt32LE(0, Growˉlayout.opcodes[0] + 1);
        }],
        ['grow-vector-non-vector', Bytes => {
            Bytes.writeUInt32LE(6, Growˉlayout.opcodes[0] + 1);
        }],
        ['grow-vector-missing', Bytes => {
            Bytes.writeUInt32LE(999, Growˉlayout.opcodes[0] + 1);
        }],
        ['grow-budget-non-budget', Bytes => {
            Bytes.writeUInt32LE(13, Growˉlayout.opcodes[0] + 5);
        }],
        ['grow-budget-missing', Bytes => {
            Bytes.writeUInt32LE(999, Growˉlayout.opcodes[0] + 5);
        }],
        ['grow-same-vector-budget', Bytes => {
            Bytes.writeUInt32LE(12, Growˉlayout.opcodes[0] + 5);
        }],
        ['grow-result-missing', Bytes => {
            Bytes.writeUInt32LE(99, Growˉlayout.opcodes[0] + 9);
        }],
        ['grow-result-record', Bytes => {
            Bytes.writeUInt32LE(0, Growˉlayout.opcodes[0] + 9);
        }],
        ['grow-non-unit-valid-payload', Bytes => {
            Bytes[Growˉlayout.validPayloadShape] = 1;
        }],
        ['grow-wrong-result-failure-type', Bytes => {
            Bytes.writeUInt32LE(1, Growˉlayout.resultFailureType);
        }],
        ['grow-budget-in-failure-record', Bytes => {
            Bytes[Growˉlayout.requestedBytesShape] = 25;
        }],
        ['grow-wrong-allocation-field', Bytes => {
            Bytes[Growˉlayout.availableBytesShape] = 5;
        }],
        ['grow-truncated-instruction', Bytes => {
            Bytes[Growˉlayout.lastInstruction] = 209;
        }],
    ];
    for (const [Name, Mutate] of Growˉmalformedˉcases) {
        const Candidate = Buffer.from(Growˉsuccessˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Ownedˉcallˉmalformedˉcases = [
        ['owned-call-version-downgrade', Bytes => {
            Bytes.writeUInt16LE(25, 6);
            return Bytes;
        }],
        ['owned-call-invalid-borrowed-shape', Bytes => {
            Bytes[Ownedˉcallˉlayout.observeParameter] = 28;
            return Bytes;
        }],
        ['owned-call-value-mode-borrowed', Bytes => {
            Bytes[Ownedˉcallˉlayout.forwardParameter] = 26;
            return Bytes;
        }],
        ['owned-call-borrow-mode-value', Bytes => {
            Bytes[Ownedˉcallˉlayout.observeParameter] = 23;
            return Bytes;
        }],
        ['owned-call-borrowed-return', Bytes => {
            Bytes[Ownedˉcallˉlayout.forwardReturn] = 26;
            return Bytes;
        }],
        ['owned-call-borrowed-local', Bytes => {
            Bytes[Ownedˉcallˉlayout.vectorLocal] = 27;
            return Bytes;
        }],
    ];
    for (const [Name, Mutate] of Ownedˉcallˉmalformedˉcases) {
        const Candidate = Mutate(Buffer.from(Ownedˉcallˉsuccessˉbytes));
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Ownedˉaggregateˉmalformedˉcases = [
        ['owned-aggregate-version-downgrade', Bytes => {
            Bytes.writeUInt16LE(27, 6);
        }],
        ['owned-aggregate-borrowed-parameter', Bytes => {
            Bytes[Ownedˉaggregateˉlayout.ownerParameter] = 28;
        }],
        ['owned-aggregate-wrong-view-nominal', Bytes => {
            Bytes.writeUInt32LE(0, Ownedˉaggregateˉlayout.borrowedLocal + 1);
        }],
        ['owned-aggregate-view-owner-local', Bytes => {
            Bytes[Ownedˉaggregateˉlayout.ownerLocal] = 28;
        }],
        ['owned-aggregate-take-before-view', Bytes => {
            Bytes[Ownedˉaggregateˉlayout.ownerLoadOpcode] = 205;
        }],
        ['owned-aggregate-take-borrowed-view', Bytes => {
            Bytes[Ownedˉaggregateˉlayout.borrowedLoadOpcode] = 205;
        }],
    ];
    for (const [Name, Mutate] of Ownedˉaggregateˉmalformedˉcases) {
        const Candidate = Buffer.from(Ownedˉaggregateˉsuccessˉbytes);
        Mutate(Candidate);
        if (Candidate.equals(Ownedˉaggregateˉsuccessˉbytes)) {
            Reject(`The malformed aggregate mutation ${Name} changed no bytes.`);
        }
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Usingˉloopˉmismatch = Buffer.from(Usingˉloopˉbytes);
    Usingˉloopˉmismatch[Usingˉloopˉlayout.backedgeRelease] = 4;
    const Usingˉloopˉmismatchˉpath = path.join(
        Work, 'using-loop-backedge-state-mismatch.wvb',
    );
    writeFileSync(
        Usingˉloopˉmismatchˉpath, Usingˉloopˉmismatch, { flag: 'wx' },
    );
    Requireˉinvalid(
        Verifier, Usingˉloopˉmismatchˉpath,
        'using loop backedge ownership mismatch',
    );
    const Sourceˉfileˉmalformedˉcases = [
        ['source-file-version-downgrade', Bytes => {
            Bytes.writeUInt16LE(28, 6);
        }],
        ['source-file-forgeable-parameter', Bytes => {
            Bytes[Sourceˉfileˉlayout.parameterShape] = 1;
        }],
        ['source-file-forgeable-local', Bytes => {
            Bytes[Sourceˉfileˉlayout.localShape] = 1;
        }],
        ['source-file-unknown-length-opcode', Bytes => {
            Bytes[Sourceˉfileˉlayout.sourceLengthOpcode] = 211;
        }],
        ['source-file-length-from-parameter', Bytes => {
            Bytes.writeUInt32LE(0, Sourceˉfileˉlayout.sourceLengthOpcode + 1);
        }],
        ['source-file-copied-parameter', Bytes => {
            Bytes[Sourceˉfileˉlayout.parameterTakeOpcode] = 4;
        }],
    ];
    for (const [Name, Mutate] of Sourceˉfileˉmalformedˉcases) {
        const Candidate = Buffer.from(Sourceˉfileˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }
    const Structuredˉtaskˉmalformedˉcases = [
        ['structured-task-version-downgrade', Bytes => {
            Bytes.writeUInt16LE(31, 6);
        }],
        ['structured-task-spawn-task-type-mismatch', Bytes => {
            Bytes.writeUInt32LE(0, Structuredˉtaskˉlayout.spawn + 9);
        }],
        ['structured-task-await-origin-is-handle', Bytes => {
            Bytes.writeUInt32LE(
                Bytes.readUInt32LE(Structuredˉtaskˉlayout.await + 1),
                Structuredˉtaskˉlayout.await + 5,
            );
        }],
        ['structured-task-invalid-exit-policy', Bytes => {
            Bytes[Structuredˉtaskˉlayout.exit + 5] = 3;
        }],
    ];
    for (const [Name, Mutate] of Structuredˉtaskˉmalformedˉcases) {
        const Candidate = Buffer.from(Structuredˉtaskˉbytes);
        Mutate(Candidate);
        const Candidateˉpath = path.join(Work, `${Name}.wvb`);
        writeFileSync(Candidateˉpath, Candidate, { flag: 'wx' });
        Requireˉinvalid(Verifier, Candidateˉpath, Name);
    }

    const Runnerˉwvb = path.join(Work, 'Runner.wvb');
    const Runner = path.join(Work, `Runner${Executableˉsuffix}`);
    const Runnerˉstagerˉwvb = path.join(Work, 'Runner-Stager.wvb');
    const Runnerˉstager = path.join(
        Work, `Runner-Stager${Executableˉsuffix}`,
    );
    const Runnerˉobjectˉprefix = path.join(Work, 'Runner-Object');
    const Runnerˉobjectˉmanifest = path.join(Work, 'Runner-Object.wvop');
    const Runnerˉimageˉprefix = path.join(Work, 'Runner-Image');
    const Runnerˉimageˉmanifest = path.join(Work, 'Runner-Image.wvli');
    const Runnerˉcanonicalˉprefix = path.join(Work, 'Runner-Canonical');
    const Runnerˉcanonicalˉmanifest = path.join(Work, 'Runner-Canonical.wvli');
    await Runˉnode('runner-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Wvb-Runner.wvproj'), Runnerˉwvb,
        Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity,
    ]);
    Requireˉnativeˉfunctionˉlimits(readFileSync(Runnerˉwvb));
    await Runˉnode('runner-stager-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        path.join(
            Repositoryˉroot, 'Projects', 'Compiler',
            'Windvale-Native-X64-Lowering-Staging-Tool.wvproj',
        ),
        Runnerˉstagerˉwvb,
        Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity,
    ]);
    await Runˉnative('runner-stager-package', 'Package-Segmented-Compiler-Wvb', [
        '6', Runnerˉstagerˉwvb, Runnerˉstager, '--development-cache',
    ]);
    await Run('runner-stage', Runnerˉstager, [
        Runnerˉwvb, Runnerˉobjectˉprefix, Runnerˉobjectˉmanifest,
    ]);
    await Runˉnative('runner-link', 'Link-Staged-Compiler-Wvo', [
        Runnerˉobjectˉprefix, Runnerˉobjectˉmanifest,
        Runnerˉimageˉprefix, Runnerˉimageˉmanifest,
    ]);
    const Runnerˉtransportˉreport = await Runˉnative(
        'runner-transport', 'Transport-Compiler-Image', [
            Runnerˉimageˉprefix, Runnerˉimageˉmanifest,
            Runnerˉcanonicalˉprefix, Runnerˉcanonicalˉmanifest,
        ],
    );
    const Runnerˉtransportˉline = Normalize(Runnerˉtransportˉreport)
        .trimEnd().split('\n').find(Line =>
            Line.startsWith('compiler image transport status=Complete '),
        );
    const Runnerˉtransportˉmatch = Runnerˉtransportˉline?.match(
        / entry-offset=([0-9]+) chunks=([0-9]+) manifest-bytes=/,
    );
    if (Runnerˉtransportˉmatch === undefined) {
        Reject('The runner compiler-image transport report differs.');
    }
    const Runnerˉentry = Number(Runnerˉtransportˉmatch[1]);
    const Runnerˉfragments = Number(Runnerˉtransportˉmatch[2]);
    if (!Number.isSafeInteger(Runnerˉentry) || Runnerˉentry < 0 ||
        Runnerˉentry > 0xffff_ffff ||
        !Number.isSafeInteger(Runnerˉfragments) ||
        Runnerˉfragments < 1 || Runnerˉfragments > 16) {
        Reject('The runner compiler-image transport bounds differ.');
    }
    await Runˉnative('runner-package', 'Package-Hosted-Wvb', [
        'image', '5', Runnerˉwvb, Runnerˉcanonicalˉprefix,
        String(Runnerˉfragments), String(Runnerˉentry), Runner, Hostˉtarget,
    ]);
    await Runˉnode(
        'callable-runner-compatibility',
        'Verify-Language-1.0-Callable-Runner.mjs',
        [Runner],
    );
    Requireˉresultˉ42(Runner, Successˉa, 'successful Split execution');
    Requireˉresultˉ42(Runner, Failure, 'refused Split execution');
    Requireˉresultˉ42(
        Runner, Vectorˉsuccessˉa, 'successful Vector construction execution',
    );
    Requireˉresultˉ42(
        Runner, Vectorˉfailure, 'refused Vector construction execution',
    );
    Requireˉruntimeˉfailure(
        Runner, Vectorˉzero, 3008, 'zero-maximum Vector construction execution',
    );
    Requireˉresultˉ42(Runner, Appendˉsuccessˉa, 'Vector append execution');
    Requireˉresultˉ42(Runner, Growˉsuccessˉa, 'Vector growth execution');
    Requireˉresultˉ42(
        Runner, Ownedˉcallˉsuccessˉa, 'owned Vector call execution',
    );
    Requireˉresultˉ42(
        Runner, Ownedˉaggregateˉsuccessˉa, 'owned aggregate execution',
    );
    Requireˉresultˉ42(
        Runner, Usingˉfallthrough, 'using fallthrough release execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉa, 'structured-task success execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉtrap, 'structured-task trap observation',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉretainedˉresult,
        'structured-task retained-result execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉworkˉlimit,
        'structured-task work-limit execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉcallˉdepthˉlimit,
        'structured-task call-depth-limit execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉmemoryˉlimit,
        'structured-task memory-limit execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉfourˉchildˉcancellation,
        'structured-task four-child cancellation execution',
    );
    Requireˉtaskˉcompletionˉorder(
        Runner, Structuredˉtaskˉcompletionˉorder,
        'structured-task completion-order execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉproviderˉrecovery,
        'structured-task provider-recovery execution',
    );
    Requireˉresultˉ42(
        Runner, Structuredˉtaskˉenvironment,
        'structured-task default environment execution',
    );
    Requireˉtaskˉenvironmentˉresult(
        Runner, Structuredˉtaskˉenvironment,
        ['7', '3', '100', '9', '9', '100', '0'],
        45, 'structured-task exact deadline priority',
    );
    Requireˉtaskˉenvironmentˉresult(
        Runner, Structuredˉtaskˉenvironment,
        ['7', '3', '101', '9', '9', '100', '0'],
        46, 'structured-task runtime loss',
    );
    Requireˉtaskˉenvironmentˉresult(
        Runner, Structuredˉtaskˉenvironment,
        ['7', '3', '101', '9', '9', '100', '10'],
        48, 'structured-task runtime restart',
    );
    Requireˉtaskˉenvironmentˉresult(
        Runner, Structuredˉtaskˉenvironment,
        ['7', '3', '101', '9', '0', '100', '9'],
        55, 'structured-task initial runtime loss',
    );
    Requireˉtaskˉenvironmentˉresult(
        Runner, Structuredˉtaskˉenvironment,
        ['7', '3', '101', '9', '10', '100', '9'],
        56, 'structured-task initial runtime restart',
    );
    Requireˉtaskˉenvironmentˉresult(
        Runner, Structuredˉtaskˉenvironment,
        [
            '4294967295', '18446744073709551615',
            '18446744073709551615', '18446744073709551615',
            '18446744073709551615', '18446744073709551614',
            '18446744073709551615',
        ],
        42, 'structured-task maximum environment values',
    );
    const Taskˉenvironmentˉmalformedˉcases = [
        ['missing-observed-generation', ['1', '1', '2', '1', '1', '0']],
        ['leading-zero-context', ['01', '1', '2', '1', '1', '0', '1']],
        ['context-overflow', ['4294967296', '1', '2', '1', '1', '0', '1']],
        ['zero-clock', ['1', '0', '2', '1', '1', '0', '1']],
        [
            'deadline-overflow',
            ['1', '1', '18446744073709551616', '1', '1', '0', '1'],
        ],
        ['zero-expected-runtime', ['1', '1', '2', '0', '1', '0', '1']],
        ['non-decimal-admitted-runtime', ['1', '1', '2', '1', 'x', '0', '1']],
        ['negative-observation-tick', ['1', '1', '2', '1', '1', '-1', '1']],
        ['leading-zero-observed-runtime', ['1', '1', '2', '1', '1', '0', '01']],
    ];
    for (const [Name, Environment] of Taskˉenvironmentˉmalformedˉcases) {
        Requireˉtaskˉenvironmentˉrejection(
            Runner, Structuredˉtaskˉenvironment, Environment, Name,
        );
    }
    const Sourceˉsnapshotˉ42 = path.join(Work, 'Source-Snapshot-42.bin');
    const Sourceˉsnapshotˉ41 = path.join(Work, 'Source-Snapshot-41.bin');
    const Sourceˉsnapshotˉoversized = path.join(
        Work, 'Source-Snapshot-Oversized.bin',
    );
    writeFileSync(Sourceˉsnapshotˉ42, Buffer.alloc(42, 0x5a), { flag: 'wx' });
    writeFileSync(Sourceˉsnapshotˉ41, Buffer.alloc(41, 0x5a), { flag: 'wx' });
    writeFileSync(
        Sourceˉsnapshotˉoversized, Buffer.alloc(1_048_577, 0x5a),
        { flag: 'wx' },
    );
    Requireˉsourceˉfileˉresult(
        Runner, Sourceˉfileˉa, Sourceˉsnapshotˉ42, 42,
        'source-file length-match execution',
    );
    Requireˉsourceˉfileˉresult(
        Runner, Sourceˉfileˉa, Sourceˉsnapshotˉ41, 1,
        'source-file length-mismatch execution',
    );
    Requireˉsourceˉfileˉoversizedˉrejection(
        Runner, Sourceˉfileˉa, Sourceˉsnapshotˉoversized,
    );

    process.stdout.write(
        'native language 1 memory budget, Vector, using, resource, and structured task execution status=Passed ' +
        `cases=${231 + Growˉmalformedˉcases.length +
            Ownedˉaggregateˉmalformedˉcases.length} valid=24 malformed=${
            Malformedˉcases.length + Vectorˉmalformedˉcases.length +
            Appendˉmalformedˉcases.length + Growˉmalformedˉcases.length +
            Ownedˉcallˉmalformedˉcases.length +
            Ownedˉaggregateˉmalformedˉcases.length +
            Sourceˉfileˉmalformedˉcases.length +
            Structuredˉtaskˉmalformedˉcases.length + 1
        } owned-call-cases=4 owned-aggregate-source-cases=5 ` +
        'using-cases=12 using-releases=7 source-file-cases=12 ' +
        'structured-task-cases=33 structured-task-runtime-cases=46 ' +
        'task-environment-cases=17 task-environment-rejections=9 ' +
        'callable-runner-cases=2 async-call-await-cases=7 ' +
        'foundation-borrow-plan-cases=16 foundation-borrow-directory-cases=24 foundation-borrow-owner-cases=18 foundation-value-borrow-wvb-cases=20 foundation-value-borrow-opcodes=3 large-borrow-free-cases=2 ' +
        `result=42 split-wvb-bytes=${Successˉbytes.length} ` +
        `split-sha256=${Successˉsha256} ` +
        `vector-wvb-bytes=${Vectorˉsuccessˉbytes.length} ` +
        `vector-sha256=${Vectorˉsha256} ` +
        `append-wvb-bytes=${Appendˉsuccessˉbytes.length} ` +
        `append-sha256=${Appendˉsha256} ` +
        `grow-wvb-bytes=${Growˉsuccessˉbytes.length} ` +
        `grow-sha256=${Growˉsha256} ` +
        `owned-call-wvb-bytes=${Ownedˉcallˉsuccessˉbytes.length} ` +
        `owned-call-sha256=${Ownedˉcallˉsha256} ` +
        `owned-aggregate-wvb-bytes=${Ownedˉaggregateˉsuccessˉbytes.length} ` +
        `owned-aggregate-sha256=${Ownedˉaggregateˉsha256} ` +
        `using-fallthrough-wvb-bytes=${Usingˉfallthroughˉbytes.length} ` +
        `using-fallthrough-sha256=${Digest(Usingˉfallthroughˉbytes)} ` +
        `source-file-wvb-bytes=${Sourceˉfileˉbytes.length} ` +
        `source-file-sha256=${Sourceˉfileˉsha256} ` +
        `structured-task-wvb-bytes=${Structuredˉtaskˉbytes.length} ` +
        `structured-task-sha256=${Digest(Structuredˉtaskˉbytes)} ` +
        `task-completion-order-wvb-bytes=${
            Structuredˉtaskˉcompletionˉorderˉbytes.length} ` +
        `task-completion-order-sha256=${
            Digest(Structuredˉtaskˉcompletionˉorderˉbytes)} ` +
        `task-provider-recovery-wvb-bytes=${
            Structuredˉtaskˉproviderˉrecoveryˉbytes.length} ` +
        `task-provider-recovery-sha256=${
            Digest(Structuredˉtaskˉproviderˉrecoveryˉbytes)} ` +
        `task-environment-wvb-bytes=${Structuredˉtaskˉenvironmentˉbytes.length} ` +
        `task-environment-sha256=${Digest(Structuredˉtaskˉenvironmentˉbytes)}\n`,
    );
}

function Project(Name) {
    return path.join(Repositoryˉroot, 'Projects', 'Tools', Name);
}

function Testˉproject(Name) {
    return path.join(Repositoryˉroot, 'Projects', 'Tests', Name);
}

function Constructˉtargetˉdescriptor() {
    const Result = Buffer.alloc(64);
    Result.write('WVTD', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt32LE(64, 8);
    const Values = [4, 2, 1, 2, 64, 1, 1];
    Values.forEach((Value, Index) => {
        Result.writeUInt32LE(Value, 12 + Index * 4);
    });
    return Result;
}

async function Compile(Label, Admitter, Analyzer, Emitter, Fixture, Output) {
    await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Validator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        '--target-descriptor', Targetˉdescriptor,
        path.join(Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0', Fixture),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
        Output,
    ]);
}

async function Compileˉfoundationˉvalueˉborrow(
    Label,
    Admitter,
    Analyzer,
    Emitter,
    Output,
) {
    await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Validator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        '--target-descriptor', Targetˉdescriptor,
        path.join(
            Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0',
            'Foundation-Value-Payload-Borrow-Wvb.wv',
        ),
        path.join(
            Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Option.wv',
        ),
        path.join(
            Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv',
        ),
        Output,
    ]);
}

async function Verifyˉlargeˉborrowˉfreeˉfunctions(Admitter, Analyzer, Emitter, Referenceˉemitter) {
    // 1,100 assignments produce more than 4,096 WVIR operations without a borrow.
    const Body = '    var Value: i32 = 42;\n' +
        '    Value = Value + 0;\n'.repeat(1_100) + '    return Value;\n';
    const Plainˉsource = path.join(Work, 'Large-Borrow-Free.wv');
    const Mixedˉsource = path.join(Work, 'Large-Borrow-Free-Mixed.wv');
    const Plainˉoutput = path.join(Work, 'Large-Borrow-Free.wvb');
    const Referenceˉoutput = path.join(Work, 'Large-Borrow-Free-Reference.wvb');
    const Mixedˉoutput = path.join(Work, 'Large-Borrow-Free-Mixed.wvb');
    writeFileSync(Plainˉsource,
        '#!wv/1 en@1\nmodule Largeˉborrowˉfreeˉtest;\nprofile core;\n' +
        'platform linux, windows, windvale;\nauthority application;\n' +
        `export fn Main() -> i32 {\n${Body}}\n`, { flag: 'wx' });
    const Borrowˉsource = readFileSync(path.join(Repositoryˉroot,
        'Tests', 'Fixtures', 'Language-1.0', 'Foundation-Value-Payload-Borrow-Wvb.wv'), 'utf8');
    writeFileSync(Mixedˉsource,
        Borrowˉsource + `\nexport fn Largeˉborrowˉfree() -> i32 {\n${Body}}\n`, { flag: 'wx' });
    for (const [Label, Source, Selectedˉemitter, Output] of [
        ['large-borrow-free-current', Plainˉsource, Emitter, Plainˉoutput],
        ['large-borrow-free-reference', Plainˉsource, Referenceˉemitter, Referenceˉoutput],
        ['large-borrow-free-mixed', Mixedˉsource, Emitter, Mixedˉoutput],
    ]) {
        const Dependencies = Source === Mixedˉsource ? [
            path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Option.wv'),
            path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
        ] : [];
        await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
            Admitter, Validator, Analyzer, Selectedˉemitter,
            '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
            '--source-profile', Sourceˉprofile,
            '--target-descriptor', Targetˉdescriptor,
            Source, ...Dependencies, Output,
        ]);
    }
    const Plain = readFileSync(Plainˉoutput);
    const Reference = readFileSync(Referenceˉoutput);
    const Mixed = readFileSync(Mixedˉoutput);
    const Main = Parseˉfunctionˉentries(Plain, Parseˉsections(Plain)[4])
        .find(Function => Function.name === 'Main');
    if (!Plain.equals(Reference) || Plain.readUInt16LE(6) >= 39 ||
        !Main || Main.codeLength < 33_000 || Main.localCount < 3_000 ||
        Main.localCount > 4_096) {
        Reject('Borrow planning changed a large unaffected function or weakened the regression workload.');
    }
    if (Mixed.readUInt16LE(6) !== 39 || Mixed.length <= Plain.length) {
        Reject('A borrow-free function cannot coexist with the candidate borrow feature.');
    }
    process.stdout.write('PASS foundation borrow writer large-borrow-free-cases=2 ' +
        `assignments=1100 code-bytes=${Main.codeLength} local-slots=${Main.localCount} ` +
        `reference-byte-identical=true wvb-bytes=${Plain.length} sha256=${Digest(Plain)}\n`);
}

async function Compileˉvector(Label, Admitter, Analyzer, Emitter, Fixture, Output) {
    await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Validator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        '--target-descriptor', Targetˉdescriptor,
        path.join(Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0', Fixture),
        path.join(
            Repositoryˉroot, 'Libraries', 'Foundation', 'Collections',
            'Collections.wv',
        ),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
        Output,
    ]);
}

async function Compileˉsourceˉfile(Label, Admitter, Analyzer, Emitter, Output) {
    await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Validator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        '--target-descriptor', Targetˉdescriptor,
        path.join(
            Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0',
            'Source-File-Snapshot-Executable.wv',
        ),
        path.join(
            Repositoryˉroot, 'Libraries', 'Platform', 'Filesystem', 'File.wv',
        ),
        Output,
    ]);
}

async function Compileˉtask(Label, Admitter, Analyzer, Emitter, Fixture, Output) {
    await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Validator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        '--target-descriptor', Targetˉdescriptor,
        path.join(Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0', Fixture),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
        path.join(
            Repositoryˉroot, 'Libraries', 'Foundation', 'Operations',
            'Operation.wv',
        ),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
        path.join(
            Repositoryˉroot, 'Libraries', 'Foundation', 'Tasks', 'Task.wv',
        ),
        Output,
    ]);
}

async function Runˉnative(Label, Name, Arguments) {
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, 4_194_304, `${Name} script`);
    if (process.platform === 'win32') {
        return await Run(Label, Script, Arguments);
    }
    return await Run(Label, 'bash', [Script, ...Arguments]);
}

async function Runˉnode(Label, Name, Arguments) {
    await Run(Label, process.execPath, [path.join(Scriptˉdirectory, Name), ...Arguments]);
}

async function Run(Label, Command, Arguments, Expected = 0) {
    const Remaining = Developmentˉonly ? Maximumˉrunˉmilliseconds - (Date.now() - Started) :
        TOOL_TIMEOUT_MILLISECONDS;
    if (Remaining <= 0) Reject('The focused Foundation borrow development budget expired.');
    Step += 1;
    const Stepˉnumber = Step;
    const Start = Date.now();
    process.stdout.write(
        `START language 1 memory budget split execution step=${Stepˉnumber} phase=${Label}\n`,
    );
    const Result = await Runˉdevelopmentˉcommand(
        Command, Arguments, Start + Remaining, Developmentˉonly, MAXIMUM_DIAGNOSTIC_BYTES,
    );
    if (Result.Code !== Expected || Result.Error.length !== 0) {
        Reject(
            `${Label} failed: status=${Result.Code}\n` +
            `stdout=${Result.Output}\nstderr=${Result.Error}`,
        );
    }
    process.stdout.write(
        `PASS  language 1 memory budget split execution step=${Stepˉnumber} phase=${Label} elapsed-ms=${Date.now() - Start}\n`,
    );
    return Result.Output;
}

function Requireˉvalid(Verifier, Candidate, Label) {
    const Result = spawnSync(Verifier, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== 'wvb status=Valid profile=compiler-aligned\n' ||
        Result.stderr.length !== 0) {
        Reject(
            `The verifier rejected the ${Label}: status=${Result.status} ` +
            `error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout}\nstderr=${Result.stderr}`,
        );
    }
}

function Requireˉinvalid(Verifier, Candidate, Label) {
    const Result = spawnSync(Verifier, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined) {
        Reject(
            `The verifier could not inspect malformed case ${Label}: ` +
            `${Result.error.message}.`,
        );
    }
    if (Result.status === 0 ||
        (Result.stdout ?? '').includes('wvb status=Valid')) {
        Reject(
            `The verifier accepted malformed case ${Label}: ` +
            `status=${Result.status}\nstdout=${Result.stdout ?? ''}` +
            `\nstderr=${Result.stderr ?? ''}`,
        );
    }
}

function Requireˉresultˉ42(Runner, Candidate, Label) {
    const Result = spawnSync(Runner, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== 'Result: 42\n' || Result.stderr.length !== 0) {
        Reject(
            `The ${Label} differed: status=${Result.status} ` +
            `error=${Result.error?.message ?? 'none'}\n` +
            `stdout=${Normalize(Result.stdout)}\n` +
            `stderr=${Normalize(Result.stderr)}`,
        );
    }
}

function Requireˉtaskˉcompletionˉorder(Runner, Candidate, Label) {
    const Result = spawnSync(Runner, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== '3\n1\n0\n2\nResult: 42\n' ||
        Result.stderr.length !== 0) {
        Reject(
            `The ${Label} differed: status=${Result.status} ` +
            `error=${Result.error?.message ?? 'none'}\n` +
            `stdout=${Normalize(Result.stdout)}\n` +
            `stderr=${Normalize(Result.stderr)}`,
        );
    }
}

function Requireˉtaskˉenvironmentˉresult(
    Runner, Candidate, Environment, Expected, Label,
) {
    if (Environment.length !== 7 || !Number.isInteger(Expected) ||
        Expected < 0 || Expected > 255) {
        Reject(`The ${Label} test inputs are invalid.`);
    }
    const Result = spawnSync(
        Runner, ['--task-environment', Candidate, ...Environment],
        {
            encoding: 'utf8', windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        },
    );
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== `Result: ${Expected}\n` ||
        Result.stderr.length !== 0) {
        Reject(
            `The ${Label} differed: status=${Result.status} ` +
            `error=${Result.error?.message ?? 'none'}\n` +
            `stdout=${Normalize(Result.stdout)}\n` +
            `stderr=${Normalize(Result.stderr)}`,
        );
    }
}

function Requireˉtaskˉenvironmentˉrejection(
    Runner, Candidate, Environment, Label,
) {
    const Result = spawnSync(
        Runner, ['--task-environment', Candidate, ...Environment],
        {
            encoding: 'utf8', windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        },
    );
    const Expectedˉdiagnostic = Environment.length === 7
        ? 'wvb run status=Invalidˉtaskˉenvironment\n'
        : 'Usage: wvrun --task-environment <module.wvb> ' +
            '<context-generation> <clock-generation> <deadline> ' +
            '<expected-runtime-generation> <admitted-runtime-generation> ' +
            '<observation-tick> <observed-runtime-generation>\n';
    if (Result.error !== undefined || Result.status !== 64 ||
        Result.stdout.length !== 0 ||
        Normalize(Result.stderr) !== Expectedˉdiagnostic) {
        Reject(
            `The ${Label} rejection differed: status=${Result.status} ` +
            `error=${Result.error?.message ?? 'none'}\n` +
            `stdout=${Normalize(Result.stdout)}\n` +
            `stderr=${Normalize(Result.stderr)}`,
        );
    }
}

function Requireˉexitˉ42(Executable, Label) {
    Step += 1;
    process.stdout.write(
        `START language 1 memory budget split execution step=${Step} phase=${Label}\n`,
    );
    const Result = spawnSync(Executable, [], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: TOOL_TIMEOUT_MILLISECONDS,
    });
    if (Result.error !== undefined || Result.status !== 42 ||
        Result.stdout.length !== 0 || Result.stderr.length !== 0) {
        Reject(
            `The ${Label} differed: status=${Result.status} ` +
            `error=${Result.error?.message ?? 'none'}\n` +
            `stdout=${Normalize(Result.stdout)}\n` +
            `stderr=${Normalize(Result.stderr)}`,
        );
    }
    process.stdout.write(
        `PASS  language 1 memory budget split execution step=${Step} phase=${Label}\n`,
    );
}

function Requireˉruntimeˉfailure(Runner, Candidate, Status, Label) {
    const Result = spawnSync(Runner, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status === 0 ||
        Result.stdout.length !== 0 ||
        !Normalize(Result.stderr).startsWith(
            `wvb run status=Failed code=${Status} instructions=`,
        )) {
        Reject(`The ${Label} failure differed: status=${Result.status}.`);
    }
}

function Requireˉsourceˉfileˉresult(
    Runner, Candidate, Snapshot, Expected, Label,
) {
    const Result = spawnSync(
        Runner, ['--source-file', Candidate, Snapshot],
        {
            encoding: 'utf8', windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        },
    );
    if (Result.error !== undefined || Result.status !== Expected ||
        Result.stdout.length !== 0 || Result.stderr.length !== 0) {
        Reject(
            `The ${Label} differed: status=${Result.status} ` +
            `error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout}\nstderr=${Result.stderr}`,
        );
    }
}

function Requireˉsourceˉfileˉoversizedˉrejection(
    Runner, Candidate, Snapshot,
) {
    const Result = spawnSync(
        Runner, ['--source-file', Candidate, Snapshot],
        {
            encoding: 'utf8', windowsHide: true,
            maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        },
    );
    if (Result.error !== undefined || Result.status !== 64 ||
        Result.stdout.length !== 0 ||
        Normalize(Result.stderr) !==
            'wvb run status=Sourceˉsnapshotˉtooˉlarge\n') {
        Reject(`The oversized source-file rejection differed: status=${Result.status}.`);
    }
}

function Inspectˉexactˉmodule(Bytes, Requireˉsuccessˉsize = true) {
    if ((Requireˉsuccessˉsize && Bytes.length !== 752) ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 23 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable Split fixture is not the exact WVB 1.23 module.');
    }
    const Sections = Parseˉsections(Bytes);
    const Function = Parseˉmain(Bytes, Sections[4]);
    const Types = Parseˉtypes(Bytes, Sections[7]);
    if (Function.parameterCount !== 1 || Function.parameterShape !== 25 ||
        Function.returnShape !== 1 || Function.localShapes[0] !== 25 ||
        Types.length !== 3 || Types[0].kind !== 1 || Types[1].kind !== 7 ||
        Types[2].kind !== 3 || Types[2].cases.length !== 2 ||
        Types[2].cases[0].fields.length !== 1 ||
        Types[2].cases[0].fields[0].shape !== 25 ||
        Types[2].cases[1].fields.length !== 1 ||
        Types[2].cases[1].fields[0].shape !== 7 ||
        Types[2].cases[1].fields[0].typeIndex !== 0 ||
        Types[0].fields.length !== 3 || Types[0].fields[1].shape !== 10) {
        Reject('The executable Split fixture nominal layout differs.');
    }
    const Codeˉstart = Sections[5].payload + Function.codeOffset;
    const Codeˉend = Codeˉstart + Function.codeLength;
    const Matches = [];
    for (let Cursor = Codeˉstart; Cursor < Codeˉend; Cursor += 1) {
        if (Bytes[Cursor] === 206) Matches.push(Cursor);
    }
    if (Matches.length !== 1 || Bytes.readUInt32LE(Matches[0] + 1) !== 1 ||
        Bytes.readUInt32LE(Matches[0] + 5) !== 2) {
        Reject('The executable Split fixture opcode differs.');
    }
    return {
        opcode: Matches[0],
        validPayloadShape: Types[2].cases[0].fields[0].shapeOffset,
        requestedBytesShape: Types[0].fields[1].shapeOffset,
        availableBytesShape: Types[0].fields[2].shapeOffset,
    };
}

function Inspectˉexactˉvectorˉmodule(Bytes, Requireˉsuccessˉsize = true) {
    if ((Requireˉsuccessˉsize && Bytes.length !== 1107) ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 24 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable Vector fixture is not exact WVB 1.24.');
    }
    const Sections = Parseˉsections(Bytes);
    const Function = Parseˉmain(Bytes, Sections[4]);
    const Types = Parseˉtypes(Bytes, Sections[7]);
    const Expectedˉlocalˉshapes = [11, 23, 7, 10, 11, 11, 2, 23, 1, 7, 1, 1];
    if (Function.parameterCount !== 1 || Function.parameterShape !== 25 ||
        Function.returnShape !== 1 ||
        Function.localShapes.length !== Expectedˉlocalˉshapes.length ||
        Expectedˉlocalˉshapes.some(
            (Shape, Index) => Function.localShapes[Index] !== Shape,
        ) ||
        Types.length !== 5 || Types[0].kind !== 1 || Types[1].kind !== 7 ||
        Types[2].kind !== 3 || Types[2].cases.length !== 10 ||
        Types[2].cases[0].fields.length !== 4 ||
        Types[2].cases[0].fields[0].shape !== 5 ||
        Types[2].cases[1].fields.length !== 1 ||
        Types[2].cases[1].fields[0].shape !== 7 ||
        Types[2].cases[1].fields[0].typeIndex !== 0 ||
        Types[2].cases[2].fields.length !== 1 ||
        Types[2].cases[2].fields[0].shape !== 10 ||
        Types[3].kind !== 3 || Types[3].cases.length !== 2 ||
        Types[3].cases[0].fields.length !== 1 ||
        Types[3].cases[0].fields[0].shape !== 23 ||
        Types[3].cases[0].fields[0].typeIndex !== 4 ||
        Types[3].cases[1].fields.length !== 1 ||
        Types[3].cases[1].fields[0].shape !== 7 ||
        Types[3].cases[1].fields[0].typeIndex !== 0 ||
        Types[4].kind !== 5 || Types[4].element.shape !== 1 ||
        Types[0].fields.length !== 3 || Types[0].fields[1].shape !== 10) {
        Reject(
            'The executable Vector fixture nominal layout differs: ' +
            `function=${JSON.stringify(Function)} types=${JSON.stringify(Types)}.`,
        );
    }
    const Codeˉstart = Sections[5].payload + Function.codeOffset;
    const Codeˉend = Codeˉstart + Function.codeLength;
    const Matches = [];
    for (let Cursor = Codeˉstart; Cursor < Codeˉend; Cursor += 1) {
        if (Bytes[Cursor] === 207) Matches.push(Cursor);
    }
    if (Matches.length !== 1 || Bytes.readUInt32LE(Matches[0] + 1) !== 0 ||
        Bytes.readUInt32LE(Matches[0] + 5) !== 3) {
        Reject('The executable Vector fixture opcode differs.');
    }
    return {
        opcode: Matches[0],
        validPayloadShape: Types[3].cases[0].fields[0].shapeOffset,
        requestedBytesShape: Types[0].fields[1].shapeOffset,
        availableBytesShape: Types[0].fields[2].shapeOffset,
    };
}

function Inspectˉexactˉappendˉmodule(Bytes) {
    if (Bytes.length !== 3096 ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 25 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable Vector append fixture is not exact WVB 1.25.');
    }
    const Sections = Parseˉsections(Bytes);
    const Function = Parseˉmain(Bytes, Sections[4]);
    const Types = Parseˉtypes(Bytes, Sections[7]);
    const Expectedˉcollectionˉfields = [4, 1, 1, 0, 1, 0, 2, 1, 2, 1];
    if (Function.parameterCount !== 1 || Function.parameterShape !== 25 ||
        Function.parameterTypeIndices[0] !== null || Function.returnShape !== 1 ||
        Function.localShapes.length !== 37 || Function.localShapes[1] !== 23 ||
        Function.localTypeIndices[1] !== 6 || Function.localShapes[2] !== 23 ||
        Function.localTypeIndices[2] !== 6 ||
        Types.length !== 7 || Types[0].kind !== 1 || Types[1].kind !== 1 ||
        Types[2].kind !== 7 || Types[3].kind !== 3 || Types[4].kind !== 3 ||
        Types[5].kind !== 3 || Types[6].kind !== 5 ||
        Types[0].fields.length !== 3 || Types[0].fields[0].shape !== 8 ||
        Types[0].fields[0].typeIndex !== 2 ||
        Types[0].fields[1].shape !== 10 || Types[0].fields[2].shape !== 10 ||
        Types[1].fields.length !== 2 || Types[1].fields[0].shape !== 11 ||
        Types[1].fields[0].typeIndex !== 3 || Types[1].fields[1].shape !== 1 ||
        Types[3].cases.length !== Expectedˉcollectionˉfields.length ||
        Expectedˉcollectionˉfields.some(
            (Fields, Index) => Types[3].cases[Index].fields.length !== Fields,
        ) || Types[3].cases[0].fields[0].shape !== 5 ||
        Types[3].cases[1].fields[0].shape !== 7 ||
        Types[3].cases[1].fields[0].typeIndex !== 0 ||
        Types[3].cases[2].fields[0].shape !== 10 ||
        Types[4].cases.length !== 2 ||
        Types[4].cases[0].fields.length !== 1 ||
        Types[4].cases[0].fields[0].shape !== 23 ||
        Types[4].cases[0].fields[0].typeIndex !== 6 ||
        Types[4].cases[1].fields.length !== 1 ||
        Types[4].cases[1].fields[0].shape !== 7 ||
        Types[4].cases[1].fields[0].typeIndex !== 0 ||
        Types[5].cases.length !== 2 ||
        Types[5].cases[0].fields.length !== 1 ||
        Types[5].cases[0].fields[0].shape !== 20 ||
        Types[5].cases[1].fields.length !== 1 ||
        Types[5].cases[1].fields[0].shape !== 7 ||
        Types[5].cases[1].fields[0].typeIndex !== 1 ||
        Types[6].element.shape !== 1) {
        Reject(
            'The executable Vector append fixture nominal layout differs: ' +
            `function=${JSON.stringify(Function)} types=${JSON.stringify(Types)}.`,
        );
    }
    const Codeˉstart = Sections[5].payload + Function.codeOffset;
    const Codeˉend = Codeˉstart + Function.codeLength;
    const Matches = [];
    for (let Cursor = Codeˉstart; Cursor < Codeˉend; Cursor += 1) {
        if (Bytes[Cursor] === 208) Matches.push(Cursor);
    }
    if (Matches.length !== 2 || Matches.some(
        Offset => Bytes.readUInt32LE(Offset + 1) !== 3 ||
            Bytes.readUInt32LE(Offset + 5) !== 5,
    )) {
        Reject('The executable Vector append fixture opcode differs.');
    }
    return {
        opcodes: Matches,
        capacityMaximumShape: Types[3].cases[2].fields[0].shapeOffset,
        failureErrorShape: Types[1].fields[0].shapeOffset,
        failureValueShape: Types[1].fields[1].shapeOffset,
        validPayloadShape: Types[5].cases[0].fields[0].shapeOffset,
        resultFailureType: Types[5].cases[1].fields[0].shapeOffset + 1,
    };
}

function Inspectˉexactˉgrowˉmodule(Bytes) {
    if (Bytes.length !== 3628 ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 27 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable Vector growth fixture is not exact WVB 1.27.');
    }
    const Sections = Parseˉsections(Bytes);
    const Function = Parseˉmain(Bytes, Sections[4]);
    const Types = Parseˉtypes(Bytes, Sections[7]);
    if (Function.parameterCount !== 1 || Function.parameterShape !== 25 ||
        Function.returnShape !== 1 || Function.localShapes.length !== 128 ||
        Function.localShapes[4] !== 25 ||
        Function.localShapes[11] !== 23 ||
        Function.localTypeIndices[11] !== 8 ||
        Types.length !== 9 || Types[0].kind !== 1 ||
        Types[0].fields.length !== 3 || Types[0].fields[0].shape !== 8 ||
        Types[0].fields[0].typeIndex !== 2 ||
        Types[0].fields[1].shape !== 10 || Types[0].fields[2].shape !== 10 ||
        Types[2].kind !== 7 || Types[2].cases.length !== 4 ||
        Types[7].kind !== 3 || Types[7].cases.length !== 2 ||
        Types[7].cases[0].fields.length !== 1 ||
        Types[7].cases[0].fields[0].shape !== 20 ||
        Types[7].cases[1].fields.length !== 1 ||
        Types[7].cases[1].fields[0].shape !== 7 ||
        Types[7].cases[1].fields[0].typeIndex !== 0 ||
        Types[8].kind !== 5 || Types[8].element.shape !== 1) {
        Reject(
            'The executable Vector growth fixture nominal layout differs: ' +
            `function=${JSON.stringify(Function)} types=${JSON.stringify(Types)}.`,
        );
    }
    const Codeˉstart = Sections[5].payload + Function.codeOffset;
    const Codeˉend = Codeˉstart + Function.codeLength;
    const Matches = [];
    let Lastˉinstruction = null;
    let Cursor = Codeˉstart;
    while (Cursor < Codeˉend) {
        Lastˉinstruction = Cursor;
        const Opcode = Bytes[Cursor];
        const Width = Wvbˉinstructionˉwidth(Opcode);
        if (Width > Codeˉend - Cursor) {
            Reject('The executable Vector growth instruction stream is truncated.');
        }
        if (Opcode === 209) Matches.push(Cursor);
        Cursor += Width;
    }
    if (Cursor !== Codeˉend || Lastˉinstruction === null ||
        Matches.length !== 2 || Matches.some(
            Offset => Bytes.readUInt32LE(Offset + 1) !== 12 ||
                Bytes.readUInt32LE(Offset + 5) !== 5 ||
                Bytes.readUInt32LE(Offset + 9) !== 7,
        )) {
        Reject('The executable Vector growth opcode layout differs.');
    }
    return {
        opcodes: Matches,
        lastInstruction: Lastˉinstruction,
        requestedBytesShape: Types[0].fields[1].shapeOffset,
        availableBytesShape: Types[0].fields[2].shapeOffset,
        validPayloadShape: Types[7].cases[0].fields[0].shapeOffset,
        resultFailureType: Types[7].cases[1].fields[0].shapeOffset + 1,
    };
}

function Inspectˉownedˉcallˉmodule(Bytes) {
    if (Bytes.length < 64 || Bytes.length > MAXIMUM_WVB_BYTES ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 26 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable owned Vector call fixture is not WVB 1.26.');
    }
    const Sections = Parseˉsections(Bytes);
    const Section = Sections[4];
    const Count = Bytes.readUInt32LE(Section.payload);
    if (Count < 6 || Count > 256) {
        Reject(`The owned Vector call function count differs: ${Count}.`);
    }
    let Cursor = Section.payload + 4;
    let Parameterˉcount = 0;
    const Entries = [];
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Entryˉparameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        if (Entryˉparameterˉcount > 64 ||
            Parameterˉcount > 1_048_576 - Entryˉparameterˉcount) {
            Reject('The owned Vector call parameter count exceeds its bound.');
        }
        Parameterˉcount += Entryˉparameterˉcount;
        const Parameterˉshapes = [];
        for (let Parameter = 0; Parameter < Entryˉparameterˉcount; Parameter += 1) {
            const Shape = Readˉshape(Bytes, Cursor);
            Parameterˉshapes.push(Shape);
            Cursor = Shape.end;
        }
        const Return = Readˉshape(Bytes, Cursor);
        Cursor = Return.end;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        if (Localˉcount > 2048) {
            Reject('The owned Vector call local count exceeds its bound.');
        }
        const Localˉshapes = [];
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Shape = Readˉshape(Bytes, Cursor);
            Localˉshapes.push(Shape);
            Cursor = Shape.end;
        }
        if (Cursor + 12 > Section.payload + Section.length) {
            Reject('The owned Vector call function entry is truncated.');
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        Entries.push({
            name: Name.value,
            parameterShapes: Parameterˉshapes,
            returnShape: Return,
            localShapes: Localˉshapes,
            codeOffset: Codeˉoffset,
            codeLength: Codeˉlength,
        });
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject('The owned Vector call function directory has trailing bytes.');
    }
    const Expected = [
        { name: 'Forward', shapes: [23] },
        { name: 'Observe', shapes: [26] },
        { name: 'Release', shapes: [23] },
        { name: 'Borrowˉthenˉforward', shapes: [23] },
        { name: 'Consumeˉonˉbothˉpaths', shapes: [23, 2] },
        { name: 'Main', shapes: [25] },
    ];
    for (const Expectation of Expected) {
        const Entry = Entries.find(
            Candidate => Candidate.name === Expectation.name,
        );
        if (Entry === undefined ||
            Entry.parameterShapes.length !== Expectation.shapes.length ||
            Expectation.shapes.some(
                (Shape, Index) => Entry.parameterShapes[Index].shape !== Shape,
            )) {
            Reject(`The ${Expectation.name} owned-call contract differs.`);
        }
    }
    const Forward = Entries.find(Entry => Entry.name === 'Forward');
    const Observe = Entries.find(Entry => Entry.name === 'Observe');
    const Vectorˉlocal = Entries.flatMap(Entry => Entry.localShapes)
        .find(Shape => Shape.shape === 23);
    if (Vectorˉlocal === undefined) {
        Reject('The owned Vector call fixture has no Vector local evidence.');
    }
    return {
        forwardParameter: Forward.parameterShapes[0].shapeOffset,
        observeParameter: Observe.parameterShapes[0].shapeOffset,
        forwardReturn: Forward.returnShape.shapeOffset,
        vectorLocal: Vectorˉlocal.shapeOffset,
    };
}

function Inspectˉownedˉaggregateˉmodule(Bytes) {
    if (Bytes.length !== 1538 ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 28 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The executable owned aggregate fixture is not exact WVB 1.28.');
    }
    const Sections = Parseˉsections(Bytes);
    const Functionˉsection = Sections[4];
    const Codeˉsection = Sections[5];
    const Count = Bytes.readUInt32LE(Functionˉsection.payload);
    if (Count !== 4) {
        Reject(`The owned aggregate function count differs: ${Count}.`);
    }
    let Cursor = Functionˉsection.payload + 4;
    const Entries = [];
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        if (Parameterˉcount > 64) {
            Reject('The owned aggregate parameter count exceeds its bound.');
        }
        const Parameterˉshapes = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Shape = Readˉshape(Bytes, Cursor);
            Parameterˉshapes.push(Shape);
            Cursor = Shape.end;
        }
        const Return = Readˉshape(Bytes, Cursor);
        Cursor = Return.end;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        if (Localˉcount > 2048) {
            Reject('The owned aggregate local count exceeds its bound.');
        }
        const Localˉshapes = [];
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Shape = Readˉshape(Bytes, Cursor);
            Localˉshapes.push(Shape);
            Cursor = Shape.end;
        }
        if (Cursor + 12 > Functionˉsection.payload + Functionˉsection.length) {
            Reject('The owned aggregate function entry is truncated.');
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        Entries.push({
            name: Name.value,
            parameterShapes: Parameterˉshapes,
            returnShape: Return,
            localShapes: Localˉshapes,
            codeOffset: Codeˉoffset,
            codeLength: Codeˉlength,
        });
    }
    if (Cursor !== Functionˉsection.payload + Functionˉsection.length) {
        Reject('The owned aggregate function directory has trailing bytes.');
    }
    const Types = Parseˉtypes(Bytes, Sections[7]);
    if (Types.length < 2 || Types[1].kind !== 1 ||
        Types[1].fields.length !== 2 ||
        Types[1].fields[0].shape !== 23 ||
        Types[1].fields[0].typeIndex !== 5 ||
        Types[1].fields[1].shape !== 1) {
        Reject('The owned aggregate record layout differs.');
    }
    const Ownerˉparameter = Entries.flatMap(Entry => Entry.parameterShapes)
        .find(Shape => Shape.shape === 7 && Shape.typeIndex === 1);
    const Ownerˉlocal = Entries.flatMap(Entry => Entry.localShapes)
        .find(Shape => Shape.shape === 7 && Shape.typeIndex === 1);
    if (Ownerˉparameter === undefined || Ownerˉlocal === undefined) {
        Reject('The owned aggregate owner/view shapes differ.');
    }
    const Viewˉsequences = [];
    for (const Entry of Entries) {
        const Localˉspace = [
            ...Entry.parameterShapes,
            ...Entry.localShapes,
        ];
        const Codeˉstart = Codeˉsection.payload + Entry.codeOffset;
        const Codeˉend = Codeˉstart + Entry.codeLength;
        if (Codeˉstart < Codeˉsection.payload ||
            Codeˉend > Codeˉsection.payload + Codeˉsection.length) {
            Reject(`The owned aggregate ${Entry.name} code range differs.`);
        }
        const Instructions = [];
        let Codeˉcursor = Codeˉstart;
        while (Codeˉcursor < Codeˉend) {
            const Opcode = Bytes[Codeˉcursor];
            const Width = Wvbˉinstructionˉwidth(Opcode);
            if (Width > Codeˉend - Codeˉcursor) {
                Reject(`The owned aggregate ${Entry.name} code is truncated.`);
            }
            Instructions.push({ absolute: Codeˉcursor, opcode: Opcode });
            Codeˉcursor += Width;
        }
        if (Codeˉcursor !== Codeˉend) {
            Reject(`The owned aggregate ${Entry.name} code length differs.`);
        }
        for (let Position = 0; Position + 3 < Instructions.length; Position += 1) {
            const Loadˉowner = Instructions[Position];
            const Storeˉview = Instructions[Position + 1];
            const Loadˉview = Instructions[Position + 2];
            const Observeˉfield = Instructions[Position + 3];
            if (Loadˉowner.opcode !== 4 || Storeˉview.opcode !== 5 ||
                Loadˉview.opcode !== 4 || Observeˉfield.opcode !== 105) {
                continue;
            }
            const Ownerˉindex = Bytes.readUInt32LE(Loadˉowner.absolute + 1);
            const Storedˉviewˉindex = Bytes.readUInt32LE(
                Storeˉview.absolute + 1,
            );
            const Loadedˉviewˉindex = Bytes.readUInt32LE(
                Loadˉview.absolute + 1,
            );
            const Ownerˉshape = Localˉspace[Ownerˉindex];
            const Viewˉshape = Localˉspace[Storedˉviewˉindex];
            if (Storedˉviewˉindex === Loadedˉviewˉindex &&
                Ownerˉshape?.shape === 7 && Ownerˉshape.typeIndex === 1 &&
                Viewˉshape?.shape === 28 && Viewˉshape.typeIndex === 1) {
                Viewˉsequences.push({
                    ownerLoad: Loadˉowner.absolute,
                    viewLoad: Loadˉview.absolute,
                    viewShapeOffset: Viewˉshape.shapeOffset,
                });
            }
        }
    }
    if (Viewˉsequences.length !== 2) {
        Reject(
            `The owned aggregate view sequence count differs: ${Viewˉsequences.length}.`,
        );
    }
    return {
        ownerParameter: Ownerˉparameter.shapeOffset,
        ownerLocal: Ownerˉlocal.shapeOffset,
        borrowedLocal: Viewˉsequences[0].viewShapeOffset,
        ownerLoadOpcode: Viewˉsequences[0].ownerLoad,
        borrowedLoadOpcode: Viewˉsequences[0].viewLoad,
    };
}

function Requireˉusingˉidentity(
    Bytes,
    Expectedˉbytes,
    Expectedˉsha256,
    Functionˉname,
    Expectedˉtargets,
    Label,
    Expectedˉbackedgeˉreleases = 0,
) {
    if (Bytes.length !== Expectedˉbytes ||
        Digest(Bytes) !== Expectedˉsha256 ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 ||
        (Bytes.readUInt16LE(6) !== 22 && Bytes.readUInt16LE(6) !== 26) ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject(`The using ${Label} WVB identity differs.`);
    }
    const Sections = Parseˉsections(Bytes);
    const Function = Parseˉfunction(Bytes, Sections[4], Functionˉname);
    const Codeˉstart = Sections[5].payload + Function.codeOffset;
    const Codeˉend = Codeˉstart + Function.codeLength;
    if (Codeˉstart < Sections[5].payload ||
        Codeˉend > Sections[5].payload + Sections[5].length) {
        Reject(`The using ${Label} code range differs.`);
    }
    const Instructions = [];
    let Cursor = Codeˉstart;
    while (Cursor < Codeˉend) {
        const Opcode = Bytes[Cursor];
        const Width = Wvbˉinstructionˉwidth(Opcode);
        if (Cursor + Width > Codeˉend) {
            Reject(`The using ${Label} instruction stream is truncated.`);
        }
        Instructions.push({
            absolute: Cursor,
            relative: Cursor - Codeˉstart,
            opcode: Opcode,
            width: Width,
        });
        Cursor += Width;
    }
    if (Cursor !== Codeˉend) {
        Reject(`The using ${Label} instruction stream has trailing bytes.`);
    }
    const Releases = [];
    for (let Index = 0; Index + 1 < Instructions.length; Index += 1) {
        const Instruction = Instructions[Index];
        const Next = Instructions[Index + 1];
        if (Instruction.opcode === 205 && Next.opcode === 80 &&
            Next.relative === Instruction.relative + 5) {
            Releases.push({
                index: Index,
                offset: Instruction.absolute,
                target: Bytes.readUInt32LE(Instruction.absolute + 1),
            });
        }
    }
    if (Releases.length !== Expectedˉtargets.length ||
        Expectedˉtargets.some(
            (Target, Index) => Releases[Index].target !== Target,
        )) {
        Reject(
            `The using ${Label} release sequence differs: ` +
            `${JSON.stringify(Releases.map(Release => Release.target))}.`,
        );
    }
    const Backedgeˉreleases = [];
    for (const Release of Releases) {
        for (let Index = Release.index + 2; Index < Instructions.length; Index += 1) {
            const Instruction = Instructions[Index];
            if (Instruction.opcode === 48 || Instruction.opcode === 49) {
                const Target = Bytes.readUInt32LE(Instruction.absolute + 1);
                if (Target <= Instruction.relative) {
                    Backedgeˉreleases.push(Release.offset);
                    break;
                }
                if (Instruction.opcode === 48) break;
            }
            if (Instruction.opcode === 81) break;
        }
    }
    if (Backedgeˉreleases.length !== Expectedˉbackedgeˉreleases) {
        Reject(
            `The using ${Label} backedge release count differs: ` +
            `${Backedgeˉreleases.length}.`,
        );
    }
    return { backedgeRelease: Backedgeˉreleases[0] ?? null };
}

function Inspectˉsourceˉfileˉmodule(Bytes) {
    if (Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 29 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The source-file fixture is not canonical WVB 1.29.');
    }
    const Sections = Parseˉsections(Bytes);
    const Main = Parseˉmain(Bytes, Sections[4]);
    const Sourceˉlocalˉindex = Main.localShapes.indexOf(34);
    if (Main.parameterCount !== 1 || Main.parameterShape !== 34 ||
        Main.returnShape !== 1 || Sourceˉlocalˉindex < 0) {
        Reject('The source-file Main signature or local ownership differs.');
    }
    const Codeˉstart = Sections[5].payload + Main.codeOffset;
    const Codeˉend = Codeˉstart + Main.codeLength;
    if (Codeˉstart < Sections[5].payload ||
        Codeˉend > Sections[5].payload + Sections[5].length) {
        Reject('The source-file Main code range differs.');
    }
    let Cursor = Codeˉstart;
    let Sourceˉlengthˉopcode = -1;
    let Parameterˉtakeˉopcode = -1;
    let Releaseˉcount = 0;
    while (Cursor < Codeˉend) {
        const Opcode = Bytes[Cursor];
        const Width = Wvbˉinstructionˉwidth(Opcode);
        if (Width > Codeˉend - Cursor) {
            Reject('The source-file Main instruction stream is truncated.');
        }
        if (Opcode === 210) {
            if (Sourceˉlengthˉopcode !== -1 ||
                Bytes.readUInt32LE(Cursor + 1) !==
                    Main.parameterCount + Sourceˉlocalˉindex) {
                Reject('The source-file length observation target differs.');
            }
            Sourceˉlengthˉopcode = Cursor;
        }
        if (Opcode === 205 && Bytes.readUInt32LE(Cursor + 1) === 0) {
            Parameterˉtakeˉopcode = Cursor;
        }
        if (Opcode === 205 && Cursor + 5 < Codeˉend &&
            Bytes[Cursor + 5] === 80) {
            Releaseˉcount += 1;
        }
        Cursor += Width;
    }
    if (Cursor !== Codeˉend || Sourceˉlengthˉopcode < 0 ||
        Parameterˉtakeˉopcode < 0 || Releaseˉcount !== 2) {
        Reject('The source-file move, observation, or release sequence differs.');
    }
    return {
        parameterShape: Main.parameterShapeOffsets[0],
        localShape: Main.localShapeOffsets[Sourceˉlocalˉindex],
        sourceLengthOpcode: Sourceˉlengthˉopcode,
        parameterTakeOpcode: Parameterˉtakeˉopcode,
    };
}

function Inspectˉstructuredˉtaskˉmodule(Bytes) {
    if (Bytes.length < 64 || Bytes.length > MAXIMUM_WVB_BYTES ||
        Bytes.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 32 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject('The structured-task fixture is not canonical WVB 1.32.');
    }
    const Sections = Parseˉsections(Bytes);
    const Entries = Parseˉfunctionˉentries(Bytes, Sections[4]);
    const Matches = new Map([
        [214, []], [215, []], [216, []],
        [217, []], [218, []], [219, []],
    ]);
    for (const Entry of Entries) {
        const Codeˉstart = Sections[5].payload + Entry.codeOffset;
        const Codeˉend = Codeˉstart + Entry.codeLength;
        if (Codeˉstart < Sections[5].payload ||
            Codeˉend > Sections[5].payload + Sections[5].length) {
            Reject(`The structured-task ${Entry.name} code range differs.`);
        }
        let Cursor = Codeˉstart;
        while (Cursor < Codeˉend) {
            const Opcode = Bytes[Cursor];
            const Width = Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
            if (Width > Codeˉend - Cursor) {
                Reject(`The structured-task ${Entry.name} code is truncated.`);
            }
            if (Matches.has(Opcode)) Matches.get(Opcode).push(Cursor);
            Cursor += Width;
        }
        if (Cursor !== Codeˉend) {
            Reject(`The structured-task ${Entry.name} code length differs.`);
        }
    }
    const Expected = new Map([
        [214, 1], [215, 1], [216, 1],
        [217, 1], [218, 0], [219, 1],
    ]);
    for (const [Opcode, Count] of Expected) {
        if (Matches.get(Opcode).length !== Count) {
            Reject(
                `The structured-task opcode ${Opcode} count differs: ` +
                `${Matches.get(Opcode).length}.`,
            );
        }
    }
    return {
        construct: Matches.get(214)[0],
        context: Matches.get(215)[0],
        spawn: Matches.get(216)[0],
        await: Matches.get(217)[0],
        exit: Matches.get(219)[0],
    };
}

function Parseˉfunctionˉentries(Bytes, Section) {
    const Count = Bytes.readUInt32LE(Section.payload);
    if (Count < 1 || Count > 65_536) {
        Reject(`The function count exceeds its bound: ${Count}.`);
    }
    const Result = [];
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        if (Parameterˉcount > 64) Reject('A function has too many parameters.');
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            Cursor = Readˉshape(Bytes, Cursor).end;
        }
        Cursor = Readˉshape(Bytes, Cursor).end;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        if (Localˉcount > 65_536) Reject('A function has too many locals.');
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            Cursor = Readˉshape(Bytes, Cursor).end;
        }
        if (Cursor + 12 > Section.payload + Section.length) {
            Reject('A function directory entry is truncated.');
        }
        Result.push({
            index: Index,
            name: Name.value,
            parameterCount: Parameterˉcount,
            localCount: Localˉcount,
            codeOffset: Bytes.readUInt32LE(Cursor),
            codeLength: Bytes.readUInt32LE(Cursor + 4),
            maximumStack: Bytes.readUInt32LE(Cursor + 8),
        });
        Cursor += 12;
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject('The function directory length differs.');
    }
    return Result;
}

function Requireˉnativeˉfunctionˉlimits(Bytes) {
    const Entries = Parseˉfunctionˉentries(Bytes, Parseˉsections(Bytes)[4]);
    if (Entries.length > 1_024) {
        Reject(`The native runner function count differs: ${Entries.length}.`);
    }
    for (const Entry of Entries) {
        const Slots = Entry.parameterCount + Entry.localCount;
        if (Slots >= 2_048 || Entry.codeLength > 131_072 ||
            Entry.maximumStack > 1_024) {
            Reject(
                `The native runner function ${Entry.index} ${Entry.name} ` +
                `exceeds a lowering bound: parameters=${Entry.parameterCount} ` +
                `locals=${Entry.localCount} total-slots=${Slots} ` +
                `code-bytes=${Entry.codeLength} ` +
                `maximum-stack=${Entry.maximumStack}.`,
            );
        }
    }
}

function Wvbˉinstructionˉwidthˉat(Bytes, Cursor) {
    const Opcode = Bytes[Cursor];
    if (Opcode === 192) return Bytes[Cursor + 2] === 0 ? 5 : 3;
    if (Opcode === 193) return Bytes[Cursor + 1] === 0 ? 6 : 2;
    if (Opcode === 194) {
        if (Bytes[Cursor + 2] !== 0) return 3;
        return Bytes[Cursor + 1] === 19 ? 11 : 7;
    }
    if (Opcode === 214 || Opcode === 215 || Opcode === 218) return 9;
    if (Opcode === 216 || Opcode === 217) return 13;
    if (Opcode === 219) return 6;
    return Wvbˉinstructionˉwidth(Opcode);
}

function Wvbˉinstructionˉwidth(Opcode) {
    if (Opcode === 1 || (Opcode >= 3 && Opcode <= 7) ||
        Opcode === 9 || Opcode === 10 || Opcode === 48 || Opcode === 49 ||
        Opcode === 64 || Opcode === 65 || Opcode === 104 || Opcode === 105 ||
        Opcode === 197 || Opcode === 199 || Opcode === 200 ||
        (Opcode >= 202 && Opcode <= 205) || Opcode === 210) {
        return 5;
    }
    if (Opcode === 2 || Opcode === 8) return 2;
    if (Opcode === 106 || Opcode === 128 || Opcode === 129 ||
        (Opcode >= 151 && Opcode <= 154) || Opcode === 196 ||
        Opcode === 201 || (Opcode >= 206 && Opcode <= 208)) {
        return 9;
    }
    if (Opcode === 209) return 13;
    return 1;
}

function Parseˉsections(Bytes) {
    const Result = [];
    let Cursor = 12;
    for (let Expected = 1; Expected <= 7; Expected += 1) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Expected ||
            Bytes[Cursor + 1] !== 0 || Bytes.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The module has no canonical section ${Expected}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) Reject('A WVB section is truncated.');
        Result[Expected] = { header: Cursor, payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) Reject('The WVB module has trailing bytes.');
    return Result;
}

function Parseˉmain(Bytes, Section) {
    return Parseˉfunction(Bytes, Section, 'Main');
}

function Parseˉfunction(Bytes, Section, Wanted) {
    const Count = Bytes.readUInt32LE(Section.payload);
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Parameters = [];
        const Parameterˉtypes = [];
        const Parameterˉshapeˉoffsets = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Parsed = Readˉshape(Bytes, Cursor);
            Parameters.push(Parsed.shape);
            Parameterˉtypes.push(Parsed.typeIndex);
            Parameterˉshapeˉoffsets.push(Parsed.shapeOffset);
            Cursor = Parsed.end;
        }
        const Return = Readˉshape(Bytes, Cursor);
        Cursor = Return.end;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Locals = [];
        const Localˉtypes = [];
        const Localˉshapeˉoffsets = [];
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Parsed = Readˉshape(Bytes, Cursor);
            Locals.push(Parsed.shape);
            Localˉtypes.push(Parsed.typeIndex);
            Localˉshapeˉoffsets.push(Parsed.shapeOffset);
            Cursor = Parsed.end;
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        if (Name.value === Wanted) {
            return {
                parameterCount: Parameterˉcount,
                parameterShape: Parameters[0],
                parameterTypeIndices: Parameterˉtypes,
                parameterShapeOffsets: Parameterˉshapeˉoffsets,
                returnShape: Return.shape,
                localShapes: Locals,
                localTypeIndices: Localˉtypes,
                localShapeOffsets: Localˉshapeˉoffsets,
                codeOffset: Codeˉoffset,
                codeLength: Codeˉlength,
            };
        }
    }
    Reject(`The module has no ${Wanted} function.`);
}

function Parseˉtypes(Bytes, Section) {
    const Count = Bytes.readUInt32LE(Section.payload);
    const Result = [];
    let Cursor = Section.payload + 4;
    for (let Index = 0; Index < Count; Index += 1) {
        const Kind = Bytes[Cursor++];
        const Name = Readˉstring(Bytes, Cursor);
        Cursor = Name.end;
        const Entry = { kind: Kind, fields: [], cases: [], element: null };
        if (Kind === 1) {
            const Fieldˉcount = Bytes.readUInt32LE(Cursor);
            Cursor += 4;
            for (let Field = 0; Field < Fieldˉcount; Field += 1) {
                const Fieldˉname = Readˉstring(Bytes, Cursor);
                Cursor = Fieldˉname.end;
                const Shape = Readˉshape(Bytes, Cursor);
                Entry.fields.push(Shape);
                Cursor = Shape.end;
            }
        } else if (Kind === 5) {
            Entry.element = Readˉshape(Bytes, Cursor);
            Cursor = Entry.element.end;
        } else if (Kind === 7) {
            Cursor += 1;
            const Memberˉcount = Bytes.readUInt32LE(Cursor);
            Cursor += 4;
            for (let Member = 0; Member < Memberˉcount; Member += 1) {
                const Memberˉname = Readˉstring(Bytes, Cursor);
                Cursor = Memberˉname.end;
                Entry.cases.push({ fields: [], value: Bytes[Cursor++] });
            }
        } else if (Kind === 3) {
            const Caseˉcount = Bytes.readUInt32LE(Cursor);
            Cursor += 4;
            for (let Case = 0; Case < Caseˉcount; Case += 1) {
                const Caseˉname = Readˉstring(Bytes, Cursor);
                Cursor = Caseˉname.end;
                const Encoding = Bytes[Cursor++];
                const Fields = [];
                const Fieldˉcount = Encoding === 0 ? 0 :
                    Encoding === 1 ? 1 : Bytes.readUInt32LE(Cursor);
                if (Encoding === 2) Cursor += 4;
                for (let Field = 0; Field < Fieldˉcount; Field += 1) {
                    const Fieldˉname = Readˉstring(Bytes, Cursor);
                    Cursor = Fieldˉname.end;
                    const Shape = Readˉshape(Bytes, Cursor);
                    Fields.push(Shape);
                    Cursor = Shape.end;
                }
                Entry.cases.push({ fields: Fields });
            }
        } else {
            Reject(`Unexpected exact fixture type kind ${Kind}.`);
        }
        Result.push(Entry);
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject('The exact Types directory length differs.');
    }
    return Result;
}

function Readˉstring(Bytes, Offset) {
    const Length = Bytes.readUInt32LE(Offset);
    const Start = Offset + 4;
    const End = Start + Length;
    if (End > Bytes.length) Reject('A WVB string is truncated.');
    return { value: Bytes.subarray(Start, End).toString('utf8'), end: End };
}

function Readˉshape(Bytes, Offset) {
    const Shape = Bytes[Offset];
    const Nominal = [7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30, 35]
        .includes(Shape);
    return {
        shape: Shape,
        shapeOffset: Offset,
        typeIndex: Nominal ? Bytes.readUInt32LE(Offset + 1) : null,
        end: Offset + (Nominal ? 5 : 1),
    };
}

function Requireˉexactˉdigest(Bytes, Sha256, Label) {
    if (Bytes.length < 1 || Bytes.length > MAXIMUM_WVB_BYTES ||
        Digest(Bytes) !== Sha256) {
        Reject(`The ${Label} digest differs: ${Digest(Bytes)}.`);
    }
}

function Requireˉexactˉfile(Candidate, Size, Sha256, Label) {
    Requireˉordinaryˉfile(Candidate, MAXIMUM_WVB_BYTES, Label);
    const Bytes = readFileSync(Candidate);
    if (Bytes.length !== Size || Digest(Bytes) !== Sha256) {
        Reject(`The ${Label} identity differs.`);
    }
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Digest(Bytes) {
    return createHash('sha256').update(Bytes).digest('hex');
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Normalize(Value) {
    return Value.replaceAll('\r\n', '\n');
}

function Reject(Message) {
    throw new Error(Message);
}
