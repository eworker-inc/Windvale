import { spawnSync } from 'node:child_process';
import { Runˉdevelopmentˉcommand as Executeˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
import { Acquireˉfoundationˉborrowˉtestˉproducts } from './Foundation-Borrow-Test-Products-Core.mjs';
import { createHash } from 'node:crypto';
import {
    copyFileSync,
    existsSync,
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
    '8a4a00c48fb2743ecb3d6d1c4ab532a6de0613b9374cfeae3dc031d159ffbde5';
const EXPECTED_OWNED_AGGREGATE_SUCCESS_SHA256 =
    'b9810655b33c79cf980ea05f7fbca5511d3c34219f37e1b6a046a630a3e1c395';
const EXPECTED_USING_FALLTHROUGH_SHA256 =
    '74bd91bf9764874562ebec6a61a2232e528ac7bfedc82107f05b59e7aff9e170';
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

// The fast loop must never initiate compiler construction on a cache miss.
const Preparedˉcompilerˉonly = process.argv[2] === '--vector-borrow-integration' &&
    process.argv.at(-1) === '--prepared-compiler-only';
if (Preparedˉcompilerˉonly) process.argv.pop();
const Inspectionˉmode = process.argv.length === 4 ? process.argv[2] : '';
const Foundationˉsourceˉonly = process.argv.length === 8 &&
    process.argv[2] === '--foundation-source-ownership';
const Foundationˉownedˉonly = process.argv.length === 10 &&
    process.argv[2] === '--foundation-owned-payloads';
const Vectorˉparameterˉonly = process.argv.length === 10 &&
    process.argv[2] === '--vector-parameter-reads';
const Recordˉvectorˉonly = process.argv.length === 10 &&
    process.argv[2] === '--record-vector-elements';
const Vectorˉintegrationˉonly = (process.argv.length === 3 || process.argv.length === 5) &&
    process.argv[2] === '--vector-borrow-integration';
const Foundationˉruntimeˉonly = process.argv.length === 4 &&
    process.argv[2] === '--foundation-borrow-runtime';
const Foundationˉenumˉonly = process.argv.length === 4 &&
    process.argv[2] === '--foundation-enum-metadata';
const Foundationˉnativeˉonly = process.argv.length === 5 &&
    process.argv[2] === '--foundation-native-execution';
const Foundationˉstagingˉonly = (process.argv.length === 5 ||
    (process.argv.length === 7 && process.argv[5] === '--admitter')) &&
    process.argv[2] === '--foundation-native-staging';
const Foundationˉonly = (process.argv.length === 3 || process.argv.length === 5 || process.argv.length === 7) &&
    process.argv[2] === '--foundation-borrow';
const Foundationˉplanˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-plan';
const Foundationˉdirectoriesˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-directories';
const Foundationˉownersˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-owners';
const Foundationˉcomponentsˉonly = process.argv.length === 3 &&
    process.argv[2] === '--foundation-borrow-components';
const Developmentˉonly = Recordˉvectorˉonly || Vectorˉintegrationˉonly || Vectorˉparameterˉonly || Foundationˉownedˉonly || Foundationˉsourceˉonly || Foundationˉonly || Foundationˉplanˉonly ||
    Foundationˉdirectoriesˉonly || Foundationˉownersˉonly || Foundationˉcomponentsˉonly || Foundationˉruntimeˉonly || Foundationˉenumˉonly || Foundationˉnativeˉonly || Foundationˉstagingˉonly;
let Maximumˉrunˉmilliseconds = TOOL_TIMEOUT_MILLISECONDS;
if ((Foundationˉonly || Vectorˉintegrationˉonly) && process.argv.length >= 5) {
    if (process.argv[3] !== '--maximum-seconds' || !/^[1-9][0-9]{0,3}$/u.test(process.argv[4]) ||
        Number(process.argv[4]) > 3600) {
        process.stderr.write('The explicit development maximum must be 1 through 3600 seconds.\n');
        process.exit(64);
    }
    Maximumˉrunˉmilliseconds = Number(process.argv[4]) * 1000;
}
const Foundationˉsourceˉrunner = Foundationˉonly && process.argv.length === 7 && process.argv[5] === '--runner' ?
    path.resolve(process.argv[6]) : null;
const Foundationˉnativeˉlowerer = Foundationˉonly && process.argv.length === 7 && process.argv[5] === '--native-lowerer' ?
    path.resolve(process.argv[6]) : null;
if (Foundationˉonly && process.argv.length === 7 && Foundationˉsourceˉrunner === null && Foundationˉnativeˉlowerer === null) {
    process.stderr.write('Expected --runner <current-source-runner> or --native-lowerer <current-source-lowerer> after the development maximum.\n');
    process.exit(64);
}
const Started = Date.now();
// Keep process settlement and scoped cleanup inside the selected integration total.
const Developmentˉdeadline = Started + Maximumˉrunˉmilliseconds -
    (Vectorˉintegrationˉonly ? 7_500 : 0);
const Inspectionˉonly =
    Inspectionˉmode === '--inspect-structured-task' ||
    Inspectionˉmode === '--inspect-function-limits';
if (process.argv.length !== 2 && !Inspectionˉonly && !Developmentˉonly) {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs ' +
        '[--foundation-borrow-plan|--foundation-borrow-directories|--foundation-borrow-owners|--foundation-borrow-components|' +
        '--foundation-borrow [--maximum-seconds <seconds> [--runner <runner>|--native-lowerer <lowerer>]]|--foundation-borrow-runtime <runner>|' +
        '--foundation-enum-metadata <text-fixture.wvb>|--foundation-native-execution <lowerer> <text-fixture.wvb>|' +
        '--foundation-native-staging <producer> <text-fixture.wvb> [--admitter <checker>]|' +
        '--foundation-source-ownership <admitter> <validator> <analyzer> <emitter> <target.wvtd>|' +
        '--foundation-owned-payloads <admitter> <validator> <analyzer> <emitter> <target.wvtd> <verifier> <runner>|' +
        '--vector-parameter-reads <admitter> <validator> <analyzer> <emitter> <target.wvtd> <verifier> <runner>|' +
        '--record-vector-elements <admitter> <validator> <analyzer> <emitter> <target.wvtd> <verifier> <runner>|' +
        '--vector-borrow-integration [--maximum-seconds <seconds>] [--prepared-compiler-only]|' +
        '(--inspect-structured-task|--inspect-function-limits) <module.wvb>]\n',
    );
    process.exit(64);
}
if (process.platform !== 'win32' && process.platform !== 'linux') {
    Reject(`Unsupported test host: ${process.platform}.`);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
if (Foundationˉsourceˉrunner !== null) {
    Requireˉordinaryˉfile(Foundationˉsourceˉrunner, 134_217_728, 'current-source scalar runner');
}
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
        const Entries = Requireˉnativeˉfunctionˉlimits(Bytes);
        let Largest = Entries[0];
        let Mostˉslots = Entries[0];
        for (const Entry of Entries) {
            if (Entry.codeLength > Largest.codeLength) Largest = Entry;
            if (Entry.parameterCount + Entry.localCount >
                Mostˉslots.parameterCount + Mostˉslots.localCount) Mostˉslots = Entry;
        }
        process.stdout.write(
            'function limits inspection status=Valid ' +
            `functions=${Entries.length} largest-index=${Largest.index} ` +
            `largest-name=${Largest.name} code-bytes=${Largest.codeLength} ` +
            `parameters=${Largest.parameterCount} locals=${Largest.localCount} ` +
            `total-slots=${Largest.parameterCount + Largest.localCount} ` +
            `maximum-stack=${Largest.maximumStack} ` +
            `most-slots-index=${Mostˉslots.index} most-slots-name=${Mostˉslots.name} ` +
            `most-slots=${Mostˉslots.parameterCount + Mostˉslots.localCount}\n`,
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
let Borrowˉcomponentˉbytes = null;

await Main().catch(Error => {
    const Causes = Error instanceof AggregateError ? Error.errors.slice(0, 8) : [];
    const Diagnostic = [Error.message ?? String(Error),
        ...Causes.map(Cause => `Cause: ${Cause.message ?? String(Cause)}`)].join('\n');
    process.stderr.write(Buffer.from(Diagnostic + '\n', 'utf8').subarray(0, MAXIMUM_DIAGNOSTIC_BYTES));
    process.exitCode = [1, 2, 64, 124].includes(Error.exitCode) ? Error.exitCode :
        typeof Error.code === 'string' ? 2 : 1;
});

async function Runˉdevelopmentˉcommand(...Arguments) {
    const Result = await Executeˉdevelopmentˉcommand(...Arguments);
    if (Result.Code === 124) Reject(`Development child timed out.\n${Result.Error}`, 124);
    return Result;
}

async function Main() {
    let Primaryˉfailure = null;
    try {
        if (Recordˉvectorˉonly) {
            await Verifyˉrecordˉvectorˉelements(...process.argv.slice(3).map(Value => path.resolve(Value)));
        } else if (Vectorˉintegrationˉonly) {
            await Runˉvectorˉborrowˉintegration();
        } else if (Vectorˉparameterˉonly) {
            await Verifyˉvectorˉparameterˉreads(...process.argv.slice(3).map(Value => path.resolve(Value)));
        } else if (Foundationˉownedˉonly) {
            Validator = path.resolve(process.argv[4]);
            Targetˉdescriptor = path.resolve(process.argv[7]);
            await Verifyˉfoundationˉownedˉpayloads(...process.argv.slice(3).map(Value => path.resolve(Value)));
        } else if (Foundationˉsourceˉonly) {
            Validator = path.resolve(process.argv[4]);
            Targetˉdescriptor = path.resolve(process.argv[7]);
            await Verifyˉfoundationˉsourceˉownership(path.resolve(process.argv[3]),
                path.resolve(process.argv[5]), path.resolve(process.argv[6]));
        } else if (Foundationˉstagingˉonly) {
            await Verifyˉfoundationˉstaging(path.resolve(process.argv[3]), path.resolve(process.argv[4]),
                process.argv.length === 7 ? path.resolve(process.argv[6]) : null);
        } else if (Foundationˉnativeˉonly) {
            const Lowerer = path.resolve(process.argv[3]);
            const Record = path.join(Work, 'Foundation-Record.wvb');
            writeFileSync(Record, Requireˉfoundationˉcandidate(), { flag: 'wx' });
            await Verifyˉfoundationˉnative(Lowerer, Record, 'record-u32');
            await Verifyˉfoundationˉnative(Lowerer, path.resolve(process.argv[4]), 'text');
            await Verifyˉfoundationˉnativeˉrejections(Lowerer);
            process.stdout.write('native Foundation execution status=Passed cases=27 qualification=false\n');
        } else if (Foundationˉenumˉonly) {
            await Verifyˉfoundationˉenumˉmetadata(path.resolve(process.argv[3]));
        } else if (Foundationˉruntimeˉonly) {
            await Runˉfoundationˉruntime(path.resolve(process.argv[3]));
        } else if (Foundationˉcomponentsˉonly) {
            await Runˉfoundationˉcomponents();
        } else {
            if (!Foundationˉdirectoriesˉonly && !Foundationˉownersˉonly) await Runˉfoundationˉplan();
            if (!Foundationˉplanˉonly && !Foundationˉownersˉonly) await Runˉfoundationˉdirectories();
            if (!Foundationˉplanˉonly && !Foundationˉdirectoriesˉonly) await Runˉfoundationˉowners();
            if (!Foundationˉplanˉonly && !Foundationˉdirectoriesˉonly && !Foundationˉownersˉonly) {
                await Runˉpublicationˉandˉexecution();
            }
        }
    } catch (Error) {
        Primaryˉfailure = Error;
        throw Error;
    } finally {
        try {
            // A subprocess boundary cannot carry the typed uncertainty marker.
            // Retain diagnostic work on framework failure rather than race a
            // producer whose termination could not be confirmed.
            if (Primaryˉfailure?.cleanupUncertain || Primaryˉfailure?.exitCode === 2) {
                process.stderr.write(`Preserved test work after uncertain infrastructure cleanup: ${Work}\n`);
            } else {
                const Resolved = path.resolve(Work);
                if (path.dirname(Resolved) !== Temporaryˉroot ||
                    !path.basename(Resolved).startsWith('windvale-memory-budget-split-execution-') ||
                    lstatSync(Resolved).isSymbolicLink() || realpathSync.native(Resolved) !== Work) {
                    Reject(`Refusing to remove unexpected test directory: ${Resolved}.`);
                }
                rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
            }
        } catch (Cleanupˉerror) {
            if (Primaryˉfailure !== null) {
                Primaryˉfailure.message += `\nTest cleanup also failed: ${Cleanupˉerror.message}`;
                throw Primaryˉfailure;
            }
            throw Object.assign(Cleanupˉerror, { exitCode: 2 });
        }
    }
    if (Vectorˉintegrationˉonly) {
        const Elapsed = Date.now() - Started;
        if (Elapsed > Maximumˉrunˉmilliseconds) Reject('Vector borrow integration exceeded its total budget during cleanup.', 124);
        process.stdout.write('native Vector borrow integration status=Passed cases=537 ' +
            'components=388 vector-groups=80 record-collection-groups=40 owned-payload-groups=19 runtime-groups=10 ' +
            `qualification=false elapsed-ms=${Elapsed}\n`);
    }
    if (Developmentˉonly && !Recordˉvectorˉonly && !Vectorˉintegrationˉonly && !Vectorˉparameterˉonly && !Foundationˉownedˉonly && !Foundationˉsourceˉonly && !Foundationˉruntimeˉonly && !Foundationˉenumˉonly && !Foundationˉnativeˉonly && !Foundationˉstagingˉonly) {
        const Elapsed = Date.now() - Started;
        if (Elapsed > Maximumˉrunˉmilliseconds) {
            Reject('The focused Foundation borrow development budget expired during cleanup.', 124);
        }
        process.stdout.write(
            `native language 1 foundation borrow development status=Passed cases=${Foundationˉcomponentsˉonly ? 388 : Foundationˉonly ? (Foundationˉnativeˉlowerer !== null ? 441 : Foundationˉsourceˉrunner === null ? 414 : 417) : Foundationˉplanˉonly ? 27 : Foundationˉdirectoriesˉonly ? 27 : 334} ` +
            `selection=${Foundationˉcomponentsˉonly ? 'components' : Foundationˉonly ? 'publication' : Foundationˉplanˉonly ? 'plan' : Foundationˉdirectoriesˉonly ? 'directories' : 'owners'} qualification=false candidate-execution=${Foundationˉsourceˉrunner !== null || Foundationˉnativeˉlowerer !== null} ` +
            (Foundationˉnativeˉlowerer === null ? '' : 'execution=native-x64 ') +
            (Borrowˉcomponentˉbytes === null ? '' :
                `component-wvb-bytes=${Borrowˉcomponentˉbytes.length} component-wvb-sha256=${Digest(Borrowˉcomponentˉbytes)} `) +
            (Borrowˉplanˉbytes === null ? '' :
                `plan-wvb-bytes=${Borrowˉplanˉbytes.length} plan-wvb-sha256=${Digest(Borrowˉplanˉbytes)} `) +
            (Borrowˉdirectoryˉbytes === null ? '' :
                `directory-wvb-bytes=${Borrowˉdirectoryˉbytes.length} directory-wvb-sha256=${Digest(Borrowˉdirectoryˉbytes)} `) +
            (Borrowˉownerˉbytes === null ? '' :
                `owner-wvb-bytes=${Borrowˉownerˉbytes.length} owner-wvb-sha256=${Digest(Borrowˉownerˉbytes)} `) +
            `elapsed-ms=${Elapsed}\n`,
        );
    }
}

async function Runˉvectorˉborrowˉintegration() {
    const Deadline = Developmentˉdeadline;
    process.stdout.write('native Vector borrow integration phase=construction status=Started ' +
        `maximum-seconds=${Maximumˉrunˉmilliseconds / 1000} ` +
        `compiler-preparation=${Preparedˉcompilerˉonly ? 'forbidden' : 'allowed'}\n`);
    const Products = await Acquireˉfoundationˉborrowˉtestˉproducts({
        Work, Deadline, Prepareˉcompiler: !Preparedˉcompilerˉonly,
        Run: (Label, Command, Arguments, Childˉdeadline) =>
            Run(Label, Command, Arguments, 0, Childˉdeadline),
    });
    const Target = path.join(Repositoryˉroot, 'Projects/Targets/Windows-X64-No-Foreign.wvtd');
    process.stdout.write('native Vector borrow integration phase=verification status=Started\n');
    const Components = await Run('foundation-borrow-components-execute', Products.Components, [], 42);
    if (Components !== '') Reject('The combined Foundation component test emitted unexpected output.');
    await Verifyˉvectorˉparameterˉreads(Products.Admitter, Products.Authenticator,
        Products.Analyzer, Products.Emitter, Target, Products.Verifier, Products.Runner);
    await Verifyˉrecordˉvectorˉelements(Products.Admitter, Products.Authenticator,
        Products.Analyzer, Products.Emitter, Target, Products.Verifier, Products.Runner);
    Validator = Products.Authenticator;
    Targetˉdescriptor = Target;
    await Verifyˉfoundationˉownedˉpayloads(Products.Admitter, Products.Authenticator,
        Products.Analyzer, Products.Emitter, Target, Products.Verifier, Products.Runner);
    await Runˉfoundationˉruntime(Products.Runner);
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

function Requireˉfoundationˉcandidate() {
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
    return Buffer.from(Values);
}

function Foundationˉruntimeˉcases(Candidate) {
    const Code = Parseˉsections(Candidate)[5].payload;
    const Projection = Code + 62 + 483;
    if (Candidate[Projection] !== 225) Reject('The published first projection moved.');
    const Cases = [['published', Candidate, true]];
    for (const [Label, Offset, Value] of [
        ['unknown-version', 6, 40],
        ['zero-projection', Projection + 9, 0],
        ['unknown-projection', Projection + 9, 4],
        ['wrong-owner', Projection + 1, 62],
        ['wrong-view-type', Projection + 5, 4],
        ['unknown-opcode', Projection, 226],
        ['borrowed-parameter-read', Code + 1152, 1],
        ['borrowed-take', Code + 62 + 501, 205],
    ]) {
        const Mutated = Buffer.from(Candidate);
        Mutated[Offset] = Value;
        Cases.push([Label, Mutated, false]);
    }
    Cases.push(['truncated', Candidate.subarray(0, Candidate.length - 1), false]);
    return Cases;
}

async function Runˉfoundationˉruntime(Runner) {
    Requireˉordinaryˉfile(Runner, 134_217_728, 'current-source scalar runner');
    const Candidate = Requireˉfoundationˉcandidate();
    const Cases = Foundationˉruntimeˉcases(Candidate);
    for (const [Label, Bytes, Valid] of Cases) {
        const File = path.join(Work, `Borrow-Runtime-${Label}.wvb`);
        writeFileSync(File, Bytes, { flag: 'wx' });
        process.stdout.write(`START Foundation runtime case=${Label}\n`);
        const Result = await Runˉdevelopmentˉcommand(Runner, [File],
            Developmentˉonly ? Math.min(Developmentˉdeadline, Date.now() + 60_000) :
                Date.now() + 60_000, true,
            MAXIMUM_DIAGNOSTIC_BYTES);
        const Accepted = Valid ? Result.Code === 0 && Normalize(Result.Output) === 'Result: 42\n' &&
            Result.Error === '' : Result.Code === 1 && Result.Output === '' &&
            /^(wvb run status=Unsupported profile=portable-main-i32 phase=envelope|wvb run status=Invalid phase=compiler-verification)\n$/u.test(Normalize(Result.Error));
        if (!Accepted) Reject(`Foundation runtime ${Label} failed: status=${Result.Code}\nstdout=${Result.Output}\nstderr=${Result.Error}`);
        process.stdout.write(`PASS Foundation runtime case=${Label}\n`);
    }
    process.stdout.write(`native Foundation borrow runtime status=Passed cases=${Cases.length} ` +
        `candidate-execution=true qualification=false wvb-sha256=${Digest(Candidate)} ` +
        `runner-sha256=${Digest(readFileSync(Runner))} elapsed-ms=${Date.now() - Started}\n`);
}

async function Verifyˉfoundationˉfresh(Runner, Candidate, Label) {
    const Output = await Run(`foundation-fresh-${Label}-execute`, Runner, [Candidate]);
    if (Normalize(Output) !== 'Result: 42\n') Reject(`Fresh Foundation ${Label} execution differs.`);
    process.stdout.write(`PASS Foundation fresh source runtime fixture=${Label} ` +
        `wvb-bytes=${readFileSync(Candidate).length} wvb-sha256=${Digest(readFileSync(Candidate))} ` +
        `runner-sha256=${Digest(readFileSync(Runner))}\n`);
}

async function Runˉfoundationˉcomponents() {
    Requireˉfoundationˉcandidate();
    const Wvb = path.join(Work, 'Borrow-Components.wvb');
    await Runˉnative('foundation-borrow-components-build', 'Build-Cached-Project-Wvb', [
        Testˉproject('Windvale-Native-Test-Foundation-Borrow-Components.wvproj'), Wvb,
    ]);
    Borrowˉcomponentˉbytes = readFileSync(Wvb);
    const Application = path.join(Work, `Borrow-Components.${process.platform === 'win32' ? 'exe' : 'elf'}`);
    await Runˉnative('foundation-borrow-components-package', 'Package-Segmented-Compiler-Wvb', [
        '1', Wvb, Application, '--development-cache',
    ]);
    const Result = await Run('foundation-borrow-components-execute', Application, [], 42);
    if (Result !== '') Reject('The Foundation component self-test emitted unexpected output.');
}

async function Runˉfoundationˉowners() {
    Requireˉfoundationˉcandidate();
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
    const Foundationˉtextˉa = path.join(Work, 'Foundation-Text-A.wvb');
    const Foundationˉtextˉb = path.join(Work, 'Foundation-Text-B.wvb');
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
    await Verifyˉfoundationˉsourceˉownership(Admitter, Analyzer, Emitter);

    if (!Foundationˉonly || Foundationˉsourceˉrunner !== null || Foundationˉnativeˉlowerer !== null) {
        for (const [Label, Output] of [['a', Foundationˉtextˉa], ['b', Foundationˉtextˉb]]) {
            await Compileˉfoundationˉvalueˉborrow(`foundation-text-${Label}-compile`,
                Admitter, Analyzer, Emitter, Output, 'Foundation-Value-Borrow-Text-Executable.wv');
        }
        const Textˉbytes = readFileSync(Foundationˉtextˉa);
        if (Textˉbytes.readUInt16LE(6) !== 39 || !Textˉbytes.equals(readFileSync(Foundationˉtextˉb))) {
            Reject('Foundation text borrow publication is not deterministic WVB 1.39.');
        }
    }

    if (Foundationˉonly) {
        const Legacy = path.join(Work, 'Unchanged-Memory-Budget.wvb');
        await Compile('unchanged-earlier-bytecode', Admitter, Analyzer, Emitter,
            'Memory-Budget-Split-Executable.wv', Legacy);
        const Bytes = readFileSync(Legacy);
        if (Bytes.length !== 752 || Digest(Bytes) !== EXPECTED_SUCCESS_SHA256) {
            Reject('An unaffected earlier WVB contract changed.');
        }
        if (Foundationˉsourceˉrunner !== null) {
            await Verifyˉfoundationˉfresh(Foundationˉsourceˉrunner, Foundationˉvalueˉborrowˉa, 'record-u32');
            await Verifyˉfoundationˉfresh(Foundationˉsourceˉrunner, Foundationˉtextˉa, 'text');
        }
        if (Foundationˉnativeˉlowerer !== null) {
            await Verifyˉfoundationˉnative(Foundationˉnativeˉlowerer, Foundationˉvalueˉborrowˉa, 'record-u32');
            await Verifyˉfoundationˉnative(Foundationˉnativeˉlowerer, Foundationˉtextˉa, 'text');
            await Verifyˉfoundationˉnativeˉrejections(Foundationˉnativeˉlowerer);
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
        Usingˉfallthroughˉbytes, 1197, EXPECTED_USING_FALLTHROUGH_SHA256,
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
    await Runˉfoundationˉruntime(Runner);
    await Verifyˉfoundationˉfresh(Runner, Foundationˉvalueˉborrowˉa, 'record-u32');
    await Verifyˉfoundationˉfresh(Runner, Foundationˉtextˉa, 'text');
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
        `cases=${577 + Growˉmalformedˉcases.length +
            Ownedˉaggregateˉmalformedˉcases.length} valid=26 malformed=${
            Malformedˉcases.length + Vectorˉmalformedˉcases.length +
            Appendˉmalformedˉcases.length + Growˉmalformedˉcases.length +
            Ownedˉcallˉmalformedˉcases.length +
            Ownedˉaggregateˉmalformedˉcases.length +
            Sourceˉfileˉmalformedˉcases.length +
            Structuredˉtaskˉmalformedˉcases.length + 2
        } owned-call-cases=4 owned-aggregate-source-cases=5 ` +
        'using-cases=12 using-releases=7 source-file-cases=12 ' +
        'structured-task-cases=33 structured-task-runtime-cases=46 ' +
        'task-environment-cases=17 task-environment-rejections=9 ' +
        'callable-runner-cases=2 async-call-await-cases=7 ' +
        'foundation-borrow-plan-cases=27 foundation-borrow-directory-cases=27 foundation-borrow-owner-cases=18 foundation-borrow-call-cases=25 foundation-borrow-metadata-cases=37 foundation-borrow-stack-cases=120 foundation-borrow-lifetime-cases=40 foundation-borrow-view-cases=36 foundation-borrow-frame-cases=58 foundation-value-borrow-wvb-cases=20 foundation-value-borrow-opcodes=3 large-borrow-free-cases=2 ' +
        'foundation-source-ownership-cases=3 ' +
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
    Fixture = 'Foundation-Value-Payload-Borrow-Wvb.wv',
) {
    await Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Validator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        '--target-descriptor', Targetˉdescriptor,
        path.join(
            Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0',
            Fixture,
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

async function Verifyˉvectorˉparameterˉreads(Admitter, Authenticator, Analyzer, Emitter, Target, Verifier, Runner) {
    for (const Product of [Admitter, Authenticator, Analyzer, Emitter, Verifier, Runner]) {
        Requireˉordinaryˉfile(Product, 134_217_728, 'Vector parameter predecessor');
    }
    const Fixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Vector-Parameter-Length-Executable.wv');
    Requireˉordinaryˉfile(Fixture, 8192, 'Vector parameter fixture');
    const Source = readFileSync(Fixture, 'utf8');
    function Arguments(Input, Output, Includeˉoption = false) {
        return [Admitter, Authenticator, Analyzer, Emitter,
            '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
            '--source-profile', Sourceˉprofile, '--target-descriptor', Target,
            Input, ...['Collections/Collections.wv', 'Memory/Memory.wv',
                ...(Includeˉoption ? ['Values/Option.wv'] : []), 'Values/Result.wv']
                .map(Name => path.join(Repositoryˉroot, 'Libraries/Foundation', Name)), Output];
    }
    const Modules = [];
    for (const Generation of ['a', 'b']) {
        const Output = path.join(Work, 'Vector-Parameters-' + Generation + '.wvb');
        await Runˉnode('vector-parameters-' + Generation, 'Run-Split-Compiler.mjs', Arguments(Fixture, Output));
        const Bytes = readFileSync(Output);
        if (Bytes.readUInt16LE(6) !== 40 || Bytes.length > 8192) Reject('Vector parameter version or size differs.');
        Modules.push(Bytes);
        if (Generation === 'a') {
            if (Normalize(await Run('vector-parameters-verify', Verifier, [Output])) !==
                'wvb status=Valid profile=compiler-aligned\n') Reject('Vector parameter verification differs.');
            if (Normalize(await Run('vector-parameters-execute', Runner, [Output])) !==
                'Result: 42\n') Reject('Vector parameter execution differs.');
        }
    }
    if (!Modules[0].equals(Modules[1])) Reject('Vector parameter publication is not deterministic.');
    const Bytes = Modules[0];
    const Sections = Parseˉsections(Bytes);
    const Reads = [];
    for (const Function of Parseˉfunctionˉentries(Bytes, Sections[4])) {
        const Begin = Sections[5].payload + Function.codeOffset;
        const End = Begin + Function.codeLength;
        for (let Cursor = Begin; Cursor < End;) {
            const Width = Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
            if (Cursor + Width > End) Reject('Vector parameter instruction is truncated.');
            if (Bytes[Cursor] === 226) Reads.push({ offset: Cursor, function: Function });
            Cursor += Width;
        }
    }
    if (Reads.length !== 3 || new Set(Reads.map(Read => Read.function.index)).size !== 3) {
        Reject('Expected separate by-value, immutable and exclusive parameter reads.');
    }
    for (const [Name, Mode, Slot] of [['Read', 26, 1], ['Readˉexclusive', 27, 0], ['Consume', 23, 0]]) {
        const Function = Parseˉfunction(Bytes, Sections[4], Name);
        if (Function.parameterCount !== Slot + 1 || Bytes[Function.parameterShapeOffsets[Slot]] !== Mode ||
            !Reads.some(Read => Read.function.name === Name && Bytes.readUInt32LE(Read.offset + 1) === Slot)) {
            Reject(`Vector parameter mode differs for ${Name}.`);
        }
    }
    // Forwarding must load the original borrowed parameter at the call;
    // storing it in an ordinary Vector temporary would manufacture an owner.
    const Forward = Parseˉfunction(Bytes, Sections[4], 'Forward');
    const Readˉfunction = Reads.find(Read => Read.function.name === 'Read').function;
    if (Forward.localShapes.includes(23)) Reject('Vector forwarding invented an owned local.');
    const Forwardˉbegin = Sections[5].payload + Forward.codeOffset;
    const Forwardˉend = Forwardˉbegin + Forward.codeLength;
    let Previous = null;
    let Forwardˉcalls = 0;
    for (let Cursor = Forwardˉbegin; Cursor < Forwardˉend;) {
        if (Bytes[Cursor] === 64 && Bytes.readUInt32LE(Cursor + 1) === Readˉfunction.index) {
            if (Previous === null || Bytes[Previous] !== 4 || Bytes.readUInt32LE(Previous + 1) !== 0) {
                Reject('Vector forwarding did not preserve the original parameter slot.');
            }
            Forwardˉcalls += 1;
        }
        Previous = Cursor;
        Cursor += Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
    }
    if (Forwardˉcalls !== 1) Reject('Expected exactly one forwarded Vector call.');
    const First = Reads[0].offset;
    const Types = Parseˉtypes(Bytes, Sections[7]);
    const Wrongˉtype = Types.findIndex(Type => Type.kind !== 5);
    if (Wrongˉtype < 0) Reject('The fixture lacks a non-Vector type control.');
    const Owned = Reads.find(Read => Read.function.name === 'Consume');
    if (Owned === undefined) Reject('The by-value Vector observer is missing.');
    const Ownerˉslot = Bytes.readUInt32LE(Owned.offset + 1);
    const Ownedˉend = Sections[5].payload + Owned.function.codeOffset + Owned.function.codeLength;
    let Transfer = null;
    for (let Cursor = Owned.offset + 9; Cursor < Ownedˉend;) {
        const Opcode = Bytes[Cursor];
        if (Opcode === 205 && Bytes.readUInt32LE(Cursor + 1) === Ownerˉslot && Bytes[Cursor + 5] === 5) {
            Transfer = Cursor;
            break;
        }
        if ([48, 49, 64, 65, 81].includes(Opcode)) Reject('The owner transfer is not in the read block.');
        Cursor += Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
    }
    if (Transfer === null) Reject('The owned parameter has no explicit transfer.');
    const Immutable = Parseˉfunction(Bytes, Sections[4], 'Read');
    const Mutations = [
        ['old-minor', Broken => Broken.writeUInt16LE(39, 6)],
        ['unknown-opcode', Broken => { Broken[First] = 228; }],
        ['parameter-boundary', Broken => Broken.writeUInt32LE(Reads[0].function.parameterCount, First + 1)],
        ['parameter-overflow', Broken => Broken.writeUInt32LE(0xffffffff, First + 1)],
        ['type-boundary', Broken => Broken.writeUInt32LE(Types.length, First + 5)],
        ['type-overflow', Broken => Broken.writeUInt32LE(0xffffffff, First + 5)],
        ['wrong-type', Broken => Broken.writeUInt32LE(Wrongˉtype, First + 5)],
        ['truncated', Broken => Broken.subarray(0, First + 8)],
        ['old-owned-length', Broken => { Broken[First] = 202; }],
        ['read-after-move', Broken => Broken.set(Buffer.concat([
            Bytes.subarray(Transfer, Transfer + 10), Bytes.subarray(Owned.offset, Transfer),
        ]), Owned.offset)],
        ['wrong-call-mode', Broken => { Broken[Immutable.parameterShapeOffsets[1]] = 23; }],
    ];
    for (const [Label, Mutate] of Mutations) {
        let Broken = Buffer.from(Bytes);
        const Replacement = Mutate(Broken);
        if (Buffer.isBuffer(Replacement)) Broken = Replacement;
        if (Broken.equals(Bytes)) Reject(`Vector mutation did not change bytes: ${Label}.`);
        const Input = path.join(Work, 'Vector-parameter-malformed-' + Label + '.wvb');
        writeFileSync(Input, Broken, { flag: 'wx' });
        // A damaged instruction must not pass merely because an unrelated
        // admission boundary or an internal verifier error rejected the file.
        const Verifierˉpattern = Label === 'read-after-move'
            ? /^wvb status=Invalid phase=control-reachability\n$/u
            : Label === 'wrong-call-mode'
                ? /^wvb status=Invalid phase=typed-execution\n$/u
                : /^wvb status=Invalid phase=(?:semantic step=[a-z-]+|typed-execution|control-reachability)\n$/u;
        for (const [Tool, Pattern] of [[Verifier, Verifierˉpattern],
            [Runner, /^wvb run status=Unsupported profile=portable-main-i32 phase=envelope\n$/u]]) {
            const Result = await Runˉdevelopmentˉcommand(Tool, [Input],
                Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 1 || Result.Output !== '' || !Pattern.test(Normalize(Result.Error))) {
                Reject(`Malformed Vector parameter read did not reject: ${Label}\n${Result.Output}${Result.Error}`);
            }
        }
        process.stdout.write(`PASS Vector parameter malformed case=${Label}\n`);
    }
    const Pendingˉmove = Source.slice(0, Source.indexOf('export fn Main(')) + `
fn Consumeˉmarker(Value: Collections.Vector<i32>) -> u32 { return 7u32; }
export fn Main(Budget: Memory.Memoryˉbudget) -> i32 {
    let Created: Result.Result<Collections.Vector<i32>, Memory.Allocationˉfailure> =
        Collections.Vectorˉconstructˉreserved::<i32>(Budget, 4u64);
    return match Created {
        case Result.Result.Valid { Value: Values } {
            if Forward(borrow Values, Consumeˉmarker(Values)) != 0u64 { return 1; }
            42
        }
        case Result.Result.Failure { Error: Failure } { 2 }
    };
}
`;
    const Aliasedˉmove = Pendingˉmove.replace(
        'fn Consumeˉmarker(Value: Collections.Vector<i32>) -> u32 { return 7u32; }',
        'fn Consumeˉmarker(View: borrow Collections.Vector<i32>, Value: Collections.Vector<i32>) -> u64 { return 0u64; }',
    ).replace('Forward(borrow Values, Consumeˉmarker(Values))', 'Consumeˉmarker(borrow Values, Values)');
    const Pendingˉmutation = Pendingˉmove
        .replace('fn Consumeˉmarker(Value: Collections.Vector<i32>)',
            'fn Consumeˉmarker(Value: borrow mut Collections.Vector<i32>)')
        .replace('case Result.Result.Valid { Value: Values } {',
            'case Result.Result.Valid { Value: Initial } {\n            var Values: Collections.Vector<i32> = Initial;')
        .replace('Consumeˉmarker(Values)', 'Consumeˉmarker(borrow mut Values)');
    const Movedˉbeforeˉread = Pendingˉmove.replace(
        'fn Consumeˉmarker(Value: Collections.Vector<i32>) -> u32 { return 7u32; }',
        'fn Consumeˉmarker(Value: Collections.Vector<i32>, Marker: u64) -> u64 { return 0u64; }',
    ).replace('Forward(borrow Values, Consumeˉmarker(Values))',
        'Consumeˉmarker(Values, Forward(borrow Values, 7u32))');
    const Movedˉbeforeˉlength = Movedˉbeforeˉread.replace(
        'Consumeˉmarker(Values, Forward(borrow Values, 7u32))',
        'Consumeˉmarker(Values, Collections.Vectorˉlength(borrow Values))');
    const Pendingˉnamedˉmove = Pendingˉmove.replace(
        'Forward(borrow Values, Consumeˉmarker(Values))',
        'Read(Value: borrow Values, Marker: Consumeˉmarker(Values))');
    const Invalidˉsources = [
        ['consumed-parameter', Source.replace('fn Consume(Value: Collections.Vector<i32>) -> i32 {',
            'fn Consume(Value: Collections.Vector<i32>) -> i32 {\n    let Consumedˉbeforeˉread: Collections.Vector<i32> = Value;')],
        ['consume-borrowed', Source.replace('return Collections.Vectorˉlength(borrow Value);',
            'let Moved: Collections.Vector<i32> = Value;\n    return 1u64;')],
        ['forward-after-move', Source.replace('if Forward(borrow Moved, 7u32) != 1u64',
            'if Forward(borrow Value, 7u32) != 1u64')],
        ['move-forwarded-parameter', Source.replace('return Read(Marker, borrow Value);',
            'let Moved: Collections.Vector<i32> = Value;\n    return Read(Marker, borrow Moved);')],
        ['pending-owner-move', Pendingˉmove, /Invalidˉwir|Unsupportedˉoperation/u],
        ['aliased-owner-move', Aliasedˉmove, /Invalidˉwir|Unsupportedˉoperation/u],
        ['pending-exclusive-borrow', Pendingˉmutation, /Invalidˉwir|Unsupportedˉoperation/u],
        ['move-before-nested-read', Movedˉbeforeˉread, /Invalidˉwir|Unsupportedˉoperation/u],
        ['move-before-direct-length', Movedˉbeforeˉlength, /Invalidˉwir|Unsupportedˉoperation/u],
        ['named-pending-owner-move', Pendingˉnamedˉmove, /Invalidˉwir|Unsupportedˉoperation/u],
    ];
    for (const [Label, Text, Diagnostic = /Invalidˉwir/u] of Invalidˉsources) {
        if (Text === Source) Reject('Vector source mutation did not change its input.');
        const Input = path.join(Work, Label + '.wv');
        const Output = path.join(Work, Label + '.wvb');
        writeFileSync(Input, Text, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(process.execPath,
            [path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'),
                ...Arguments(Input, Output)],
            Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || existsSync(Output) || !Diagnostic.test(Normalize(Result.Error))) {
            Reject(`Invalid Vector ownership did not reject before publication: ${Label}\n${Result.Output}${Result.Error}`);
        }
        const Phase = Normalize(Result.Error).match(/Invalidˉwir|Unsupportedˉoperation/u)?.[0];
        process.stdout.write(`PASS Vector parameter source rejection case=${Label} diagnostic=${Phase}\n`);
    }
    // The fix applies to the existing borrowed-call contract as well as minor
    // 40. Keep transfer after observation and explicit scope release covered.
    for (const [Name, Minor] of [['Owned-Vector-Calls-And-Joins-Wir', 26], ['Using-Vector-Fallthrough-Wir', 26]]) {
        const Input = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0', Name + '.wv');
        let First = null;
        for (const Generation of ['a', 'b']) {
            const Output = path.join(Work, Name + '-' + Generation + '.wvb');
            await Runˉnode(Name + '-' + Generation, 'Run-Split-Compiler.mjs', Arguments(Input, Output));
            const Payload = readFileSync(Output);
            if (Payload.length > 8192 || Payload.readUInt16LE(6) !== Minor) {
                Reject(`Legacy Vector call version or bound differs: ${Name}.`);
            }
            if (Generation === 'a') {
                First = Payload;
                if (Normalize(await Run(Name + '-verify', Verifier, [Output])) !==
                    'wvb status=Valid profile=compiler-aligned\n' ||
                    Normalize(await Run(Name + '-execute', Runner, [Output])) !== 'Result: 42\n') {
                    Reject(`Legacy Vector call verification/execution differs: ${Name}.`);
                }
            } else if (!First.equals(Payload)) {
                Reject(`Legacy Vector call publication is not deterministic: ${Name}.`);
            }
        }
        if (Name === 'Owned-Vector-Calls-And-Joins-Wir') {
            Inspectˉownedˉcallˉmodule(First);
            if (First.length !== 1719 || Digest(First) !== EXPECTED_OWNED_CALL_SUCCESS_SHA256) {
                Reject('Owned Vector call regression identity differs.');
            }
        } else {
            Requireˉusingˉidentity(First, 1197, EXPECTED_USING_FALLTHROUGH_SHA256,
                'Main', [3], 'fallthrough');
        }
        process.stdout.write(`PASS Vector call regression fixture=${Name} wvb-bytes=${First.length} wvb-sha256=${Digest(First)}\n`);
    }
    const Payloads = await Verifyˉvectorˉpayloadˉborrows(Arguments, Verifier, Runner);
    const Indexed = await Verifyˉvectorˉindexedˉborrows(Arguments, Verifier, Runner);
    process.stdout.write(`native Vector parameter reads status=Passed reads=${Reads.length} malformed=${Mutations.length} source-rejections=${Invalidˉsources.length} ` +
        `payload-cases=${Payloads.cases} payload-malformed=${Payloads.malformed} payload-source-rejections=${Payloads.rejections} ` +
        `indexed-cases=${Indexed.cases} indexed-malformed=${Indexed.malformed} indexed-source-rejections=${Indexed.rejections} indexed-bounds=${Indexed.bounds} ` +
        `wvb-bytes=${Bytes.length} wvb-sha256=${Digest(Bytes)} qualification=false elapsed-ms=${Date.now() - Started}\n`);
}

async function Verifyˉrecordˉvectorˉelements(Admitter, Authenticator, Analyzer, Emitter, Target, Verifier, Runner) {
    for (const Product of [Admitter, Authenticator, Analyzer, Emitter, Verifier, Runner]) {
        Requireˉordinaryˉfile(Product, 134_217_728, 'record Vector predecessor');
    }
    const Fixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Foundation-Record-Vector-Executable.wv');
    Requireˉordinaryˉfile(Fixture, 8192, 'record Vector fixture');
    const Source = readFileSync(Fixture, 'utf8');
    function Arguments(Input, Output) {
        const Text = readFileSync(Input, 'utf8');
        const Imports = [
            ['collections', 'Collections/Collections.wv'], ['memory', 'Memory/Memory.wv'],
            ['operation', 'Operations/Operation.wv'],
            ['option', 'Values/Option.wv'], ['result', 'Values/Result.wv'],
            ['task', 'Tasks/Task.wv'],
        ].filter(([Module]) => Text.includes(`import Foundationˉ${Module} as `)).map(([, Name]) =>
            path.join(Repositoryˉroot, 'Libraries/Foundation', Name));
        return [Admitter, Authenticator, Analyzer, Emitter,
            '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
            '--source-profile', Sourceˉprofile, '--target-descriptor', Target,
            Input, ...Imports, Output];
    }
    function Replace(Text, Before, After) {
        if (Text.split(Before).length !== 2) Reject('Record Vector mutation is ambiguous: ' + Before);
        return Text.replace(Before, After);
    }
    const Observeˉstart = Source.indexOf('fn Observe(');
    const Observeˉend = Source.indexOf('export fn Main(');
    if (Observeˉstart < 0 || Observeˉend <= Observeˉstart) Reject('Record Vector fixture boundaries differ.');
    const Frozen = Source.slice(0, Observeˉstart) + `fn Freeze(Values: Collections.Vector<Entry>) -> Collections.Sequence<Entry> {
    let Owned: Collections.Vector<Entry> = Values;
    return Collections.Vectorˉfreeze(Owned);
}

fn Observe(Values: Collections.Vector<Entry>) -> i32 {
    let Frozen: Collections.Sequence<Entry> = Freeze(Values);
    let Alias: Collections.Sequence<Entry> = Frozen;
    if Collections.Sequenceˉlength(borrow Alias) != 2u64 { return 7; }
    let Frozenˉfirst: Entry = Collections.Sequenceˉat(borrow Frozen, 0u64);
    let Frozenˉsecond: Entry = Collections.Sequenceˉat(borrow Alias, 1u64);
    if Check(Frozenˉfirst) != 7u32 || Check(Frozenˉsecond) != 42u32 { return 8; }
    return 42;
}

` + Source.slice(Observeˉend);
    const Refused = Replace(Source, '    return Observe(Values);', `    let Refused: Result.Result<unit, Collections.Vectorˉappendˉfailure<Entry> > =
        Collections.Vectorˉappend(borrow mut Values, Entry(Span(17u32, 25u32), 18446744073709551615u64));
    match Refused {
        case Result.Result.Valid { Value: Unexpected } { return 9; }
        case Result.Result.Failure { Error: Failure } {
            if Check(Failure.Value) != 42u32 { return 10; }
        }
    }
    return Observe(Values);`);
    const Churn = Replace(Source, '                Iteration = Iteration + 1u32;',
        '                if Check(Entry(Span(17u32, 25u32), 18446744073709551615u64)) != 42u32 { return 11; }\n                Iteration = Iteration + 1u32;')
        .replace('while Iteration < 4u32', 'while Iteration < 900u32');
    const Grown = Source.slice(0, Source.indexOf('export fn Main(')) + `export fn Main(Budget: Memory.Memoryˉbudget) -> i32 {
    var Root: Memory.Memoryˉbudget = Budget;
    let Split: Result.Result<Memory.Memoryˉbudget, Memory.Allocationˉfailure> = Memory.Split(borrow mut Root, 16u64, 0u32);
    return match Split {
        case Result.Result.Failure { Error: Splitˉfailure } { 12 }
        case Result.Result.Valid { Value: Child } {
            let Created: Result.Result<Collections.Vector<Entry>, Memory.Allocationˉfailure> =
                Collections.Vectorˉconstructˉreserved::<Entry>(Child, 1u64);
            match Created {
                case Result.Result.Failure { Error: Createˉfailure } { 13 }
                case Result.Result.Valid { Value: Initial } {
                    var Values: Collections.Vector<Entry> = Initial;
                    let First: Result.Result<unit, Collections.Vectorˉappendˉfailure<Entry> > =
                        Collections.Vectorˉappend(borrow mut Values, Entry(Span(3u32, 4u32), 18446744073709551615u64));
                    match First {
                        case Result.Result.Failure { Error: Firstˉfailure } { return 14; }
                        case Result.Result.Valid { Value: Firstˉaccepted } { }
                    }
                    let Grown: Result.Result<unit, Memory.Allocationˉfailure> =
                        Collections.Vectorˉgrowˉreserved::<Entry>(borrow mut Values, borrow mut Root, 2u64);
                    match Grown {
                        case Result.Result.Failure { Error: Growˉfailure } { return 15; }
                        case Result.Result.Valid { Value: Growˉaccepted } { }
                    }
                    let Second: Result.Result<unit, Collections.Vectorˉappendˉfailure<Entry> > =
                        Collections.Vectorˉappend(borrow mut Values, Entry(Span(17u32, 25u32), 18446744073709551615u64));
                    match Second {
                        case Result.Result.Failure { Error: Secondˉfailure } { 16 }
                        case Result.Result.Valid { Value: Secondˉaccepted } { Observe(Values) }
                    }
                }
            }
        }
    };
}
`;
    function Genericˉrecord(Text) {
        let Generic = Text.replaceAll(/\bEntry\b/gu, 'Entry<Span>').replaceAll('Span>>', 'Span> >');
        Generic = Replace(Generic, 'record Entry<Span> { Range: Span; Marker: u64; }',
            'record Entry<T> { Range: T; Marker: u64; }');
        for (const [Offset, Length] of [[3, 4], [17, 25]]) {
            Generic = Replace(Generic, `Entry<Span>(Span(${Offset}u32, ${Length}u32), 18446744073709551615u64)`,
                `Entry<Span> { Range: Span(${Offset}u32, ${Length}u32), Marker: 18446744073709551615u64 }`);
        }
        return Replace(Generic, 'Collections.Vectorˉborrowˉat(Index:',
            'Collections.Vectorˉborrowˉat::<Entry<Span> >(Index:');
    }
    let Emptyˉrecord = Replace(Source, 'record Entry { Range: Span; Marker: u64; }', 'record Entry {}');
    Emptyˉrecord = Replace(Emptyˉrecord, '    if Value.Marker != 18446744073709551615u64 { return 1u32; }\n    return Value.Range.Offset + Value.Range.Length;', '    return 42u32;');
    Emptyˉrecord = Emptyˉrecord.replaceAll(/Entry\(Span\((?:3u32, 4u32|17u32, 25u32)\), 18446744073709551615u64\)/gu, 'Entry()')
        .replaceAll('!= 7u32', '!= 42u32');
    const Cases = [['nested-record-indexed', Source], ['generic-record-indexed', Genericˉrecord(Source)],
        ['freeze-alias-indexed', Frozen], ['growth-retains-records', Grown],
        ['generic-growth-retains-records', Genericˉrecord(Grown)],
        ['append-refusal-retains-record', Refused], ['record-reclamation', Churn]];
    function Inˉhelper(Text) {
        return Replace(Text, 'export fn Main(Budget:', 'fn Build(Budget:') +
            '\nexport fn Main(Budget: Memory.Memoryˉbudget) -> i32 { return Build(Budget); }\n';
    }
    const Budgetˉfixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Memory-Budget-Helper-Lifetime-Executable.wv');
    Requireˉordinaryˉfile(Budgetˉfixture, 8192, 'budget helper fixture');
    const Budgetˉsource = readFileSync(Budgetˉfixture, 'utf8');
    Cases.push(['helper-record-construction', Inˉhelper(Source)],
        ['helper-record-growth', Inˉhelper(Grown)],
        ['helper-record-freeze', Inˉhelper(Frozen)],
        ['helper-record-refusal', Inˉhelper(Refused)],
        ['helper-unused-and-returned-budget', Budgetˉsource]);
    const Taskˉfixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Structured-Tasks-Executable.wv');
    Requireˉordinaryˉfile(Taskˉfixture, 16384, 'task scope budget fixture');
    Cases.push(['helper-task-scope-budget', readFileSync(Taskˉfixture, 'utf8')]);
    const Trapˉfixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Structured-Task-Trap-Executable.wv');
    Requireˉordinaryˉfile(Trapˉfixture, 16384, 'task trap budget fixture');
    Cases.push(['helper-task-trap-budget', readFileSync(Trapˉfixture, 'utf8')]);
    const Scalarˉfixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Scalar-Collection-Helper-Executable.wv');
    Requireˉordinaryˉfile(Scalarˉfixture, 8192, 'scalar collection helper fixture');
    Cases.push(['helper-scalar-construction', readFileSync(Scalarˉfixture, 'utf8')]);
    let Candidate = null;
    const Helperˉmodules = new Map();
    for (const [Label, Text] of Cases) {
        const Input = path.join(Work, 'Record-' + Label + '.wv');
        writeFileSync(Input, Text, { flag: 'wx' });
        let Previous = null;
        for (const Generation of ['a', 'b']) {
            const Output = path.join(Work, 'Record-' + Label + '-' + Generation + '.wvb');
            await Runˉnode('record-' + Label + '-' + Generation, 'Run-Split-Compiler.mjs', Arguments(Input, Output));
            const Bytes = readFileSync(Output);
            if (Bytes.length > 16384 || Bytes.readUInt16LE(6) !== 42) Reject('Record collection version or size differs.');
            if (Previous !== null && !Previous.equals(Bytes)) Reject('Record collection publication is not deterministic.');
            Previous = Bytes;
            if (Generation === 'a') {
                if (Normalize(await Run('record-' + Label + '-verify', Verifier, [Output])) !==
                    'wvb status=Valid profile=compiler-aligned\n') Reject('Record collection verification differs: ' + Label);
                const Execution = Normalize(await Run('record-' + Label + '-execute', Runner, [Output, '--report-steps']));
                const Report = /^Result: 42\nInstructions: ([1-9][0-9]*)\n$/u.exec(Execution);
                const Taskˉcase = Label.startsWith('helper-task-');
                if (Taskˉcase ? Execution !== 'Result: 42\n' :
                    Report === null || Number(Report[1]) > 500000) Reject('Record collection execution differs: ' + Label);
                process.stdout.write(`PASS record collection case=${Label} instructions=${Taskˉcase ? 'not-reported' : Report[1]} wvb-bytes=${Bytes.length} wvb-sha256=${Digest(Bytes)}\n`);
            }
        }
        if (Label === 'nested-record-indexed') Candidate = Previous;
        if (Label.startsWith('helper-')) Helperˉmodules.set(Label, Previous);
    }
    const Budgetˉmodule = Helperˉmodules.get('helper-unused-and-returned-budget');
    const Budgetˉsections = Parseˉsections(Budgetˉmodule);
    const Budgetˉmain = Parseˉfunction(Budgetˉmodule, Budgetˉsections[4], 'Main');
    const Pulse = Parseˉfunctionˉentries(Budgetˉmodule, Budgetˉsections[4]).find(Entry => Entry.name === 'Pulse');
    function Opcodes(Bytes, Name, Opcode) {
        const Layout = Parseˉsections(Bytes);
        const Function = Parseˉfunction(Bytes, Layout[4], Name);
        const Result = [];
        const Start = Layout[5].payload + Function.codeOffset;
        for (let Cursor = Start; Cursor < Start + Function.codeLength; Cursor += Wvbˉinstructionˉwidthˉat(Bytes, Cursor)) {
            if (Bytes[Cursor] === Opcode) Result.push(Cursor);
        }
        if (Result.length === 0) Reject(`Missing helper mutation opcode ${Opcode} in ${Name}.`);
        return Result;
    }
    const Firstˉtake = Opcodes(Budgetˉmodule, 'Main', 205)[0];
    if (Pulse === undefined || Budgetˉmodule.readUInt32LE(Firstˉtake + 1) !== 0) Reject('Budget stack witness has no entry take.');
    const Insert = Firstˉtake + 5;
    const Insertˉrelative = Insert - Budgetˉsections[5].payload - Budgetˉmain.codeOffset;
    if (Insertˉrelative !== 5) Reject('Budget stack witness must follow the initial owner take.');
    const Prefix = Buffer.from(Budgetˉmodule.subarray(0, Insert));
    for (const Entry of Parseˉfunctionˉentries(Budgetˉmodule, Budgetˉsections[4])) {
        const Function = Parseˉfunction(Budgetˉmodule, Budgetˉsections[4], Entry.name);
        if (Entry.name === 'Main') {
            Prefix.writeUInt32LE(Function.codeLength + 6, Function.metadataOffset + 4);
            // The inserted no-argument call peaks at two values, then pops its
            // result. Later stack peaks are unchanged and must remain exact.
            Prefix.writeUInt32LE(Math.max(Entry.maximumStack, 2), Function.metadataOffset + 8);
        } else if (Function.codeOffset > Budgetˉmain.codeOffset) {
            Prefix.writeUInt32LE(Function.codeOffset + 6, Function.metadataOffset);
        }
    }
    Prefix.writeUInt32LE(Budgetˉsections[5].length + 6, Budgetˉsections[5].header + 4);
    const Call = Buffer.alloc(6); Call[0] = 64; Call.writeUInt32LE(Pulse.index, 1); Call[5] = 80;
    const Stackˉwitness = Buffer.concat([Prefix, Call, Budgetˉmodule.subarray(Insert)]);
    const Stackˉstart = Budgetˉsections[5].payload + Budgetˉmain.codeOffset;
    for (let Cursor = Stackˉstart; Cursor < Stackˉstart + Budgetˉmain.codeLength + 6; Cursor += Wvbˉinstructionˉwidthˉat(Stackˉwitness, Cursor)) {
        if ((Stackˉwitness[Cursor] === 48 || Stackˉwitness[Cursor] === 49) && Stackˉwitness.readUInt32LE(Cursor + 1) >= Insertˉrelative) {
            Stackˉwitness.writeUInt32LE(Stackˉwitness.readUInt32LE(Cursor + 1) + 6, Cursor + 1);
        }
    }
    const Stackˉfile = path.join(Work, 'Budget-stack-owner-across-call.wvb');
    writeFileSync(Stackˉfile, Stackˉwitness, { flag: 'wx' });
    if (Normalize(await Run('budget-stack-owner-verify', Verifier, [Stackˉfile])) !== 'wvb status=Valid profile=compiler-aligned\n' ||
        Normalize(await Run('budget-stack-owner-execute', Runner, [Stackˉfile])) !== 'Result: 42\n') {
        Reject('Budget operand-stack owner did not survive a nested call.');
    }
    process.stdout.write('PASS budget helper operand-stack-owner\n');
    const Scalarˉmodule = Helperˉmodules.get('helper-scalar-construction');
    const Growˉmodule = Helperˉmodules.get('helper-record-growth');
    const Ignore = Parseˉfunction(Budgetˉmodule, Budgetˉsections[4], 'Ignore');
    const Growˉopcode = Opcodes(Growˉmodule, 'Build', 209)[0];
    const Scalarˉopcode = Opcodes(Scalarˉmodule, 'Build', 207)[0];
    const Helperˉmutations = [
        ['immutable-view-as-owned-argument', Budgetˉmodule, Bytes => { Bytes[Ignore.parameterShapeOffsets[0]] = 36; }],
        ['copy-owned-budget', Budgetˉmodule, Bytes => { Bytes[Firstˉtake] = 4; }],
        ['split-wrong-local-kind', Budgetˉmodule, Bytes => { Bytes.writeUInt32LE(Budgetˉmain.parameterCount + Budgetˉmain.localShapes.findIndex(Kind => Kind === 2), Opcodes(Bytes, 'Main', 206)[0] + 1); }],
        ['helper-construction-previous-minor', Scalarˉmodule, Bytes => Bytes.writeUInt16LE(39, 6)],
        ['helper-construction-local-boundary', Scalarˉmodule, Bytes => Bytes.writeUInt32LE(0xffffffff, Scalarˉopcode + 1)],
        ['helper-growth-aliased-budget', Growˉmodule, Bytes => Bytes.writeUInt32LE(Bytes.readUInt32LE(Growˉopcode + 1), Growˉopcode + 5)],
        ['task-core-profile', Helperˉmodules.get('helper-task-scope-budget'), Bytes => { Bytes[20] = 1; }],
    ];
    for (const [Label, Original, Mutate] of Helperˉmutations) {
        const Bytes = Buffer.from(Original); Mutate(Bytes);
        if (Bytes.equals(Original)) Reject('Helper mutation did not change bytes: ' + Label);
        const Input = path.join(Work, 'Helper-malformed-' + Label + '.wvb');
        writeFileSync(Input, Bytes, { flag: 'wx' });
        for (const Tool of [Verifier, Runner]) {
            const Result = await Runˉdevelopmentˉcommand(Tool, [Input], Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 1 || Result.Output !== '' || !/^wvb (?:status=Invalid|run status=Unsupported).*\n$/u.test(Normalize(Result.Error))) {
                Reject(`Malformed budget helper did not reject: ${Label}\n${Result.Output}${Result.Error}`);
            }
        }
        process.stdout.write(`PASS budget helper malformed case=${Label}\n`);
    }
    const Helperˉrejections = [
        ['reuse-consumed-budget', Replace(Budgetˉsource, 'if Ignore(Owner) != 42 { return 4; }', 'if Ignore(Owner) != 42 { return 4; }\n                        if Ignore(Owner) != 42 { return 5; }')],
        ['split-immutable-budget', Replace(Budgetˉsource, 'fn Observe(Value: borrow Memory.Memoryˉbudget) -> i32 { return 42; }', 'fn Observe(Value: borrow Memory.Memoryˉbudget) -> i32 { let Split = Memory.Split(borrow mut Value, 16u64, 0u32); return 42; }')],
    ];
    for (const [Label, Text] of Helperˉrejections) {
        const Input = path.join(Work, 'Helper-rejected-' + Label + '.wv');
        const Output = path.join(Work, 'Helper-rejected-' + Label + '.wvb');
        writeFileSync(Input, Text, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(process.execPath, [path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'), ...Arguments(Input, Output)], Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || existsSync(Output) || !/source (?:analysis|emission) status=/u.test(Normalize(Result.Error))) {
            Reject(`Invalid helper source was not refused before publication: ${Label}\n${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS budget helper source rejection case=${Label}\n`);
    }
    const Sections = Parseˉsections(Candidate);
    const Types = Parseˉtypes(Candidate, Sections[7]);
    const Vectors = Types.filter(Type => Type.kind === 5 && Type.element?.shape === 7);
    if (Vectors.length !== 2 || Vectors[0].element.typeIndex === Vectors[1].element.typeIndex) {
        Reject('Expected two distinct record element nominal identities.');
    }
    const Forward = Parseˉfunction(Candidate, Sections[4], 'Forward');
    const Primary = Types[Candidate.readUInt32LE(Forward.parameterShapeOffsets[0] + 1)];
    const Other = Vectors.find(Type => Type.element.typeIndex !== Primary.element.typeIndex);
    if (Other === undefined || Primary.element?.shape !== 7) Reject('Record collection mutation targets differ.');
    const Mutations = [
        ['old-minor', Bytes => Bytes.writeUInt16LE(41, 6)],
        ['future-minor', Bytes => Bytes.writeUInt16LE(43, 6)],
        ['wrong-record-nominal', Bytes => Bytes.writeUInt32LE(Other.element.typeIndex, Primary.element.shapeOffset + 1)],
        ['record-nominal-boundary', Bytes => Bytes.writeUInt32LE(Types.length, Primary.element.shapeOffset + 1)],
        ['record-nominal-overflow', Bytes => Bytes.writeUInt32LE(0xffffffff, Primary.element.shapeOffset + 1)],
        ['shared-record-field', Bytes => { Bytes[Types[Primary.element.typeIndex].fields[1].shapeOffset] = 6; }],
        ['truncated-record-type', Bytes => Bytes.subarray(0, Primary.element.shapeOffset + 4)],
    ];
    for (const [Label, Mutate] of Mutations) {
        let Bytes = Buffer.from(Candidate);
        const Changed = Mutate(Bytes);
        if (Buffer.isBuffer(Changed)) Bytes = Changed;
        if (Bytes.equals(Candidate)) Reject('Record mutation did not change bytes: ' + Label);
        const Input = path.join(Work, 'Record-malformed-' + Label + '.wvb');
        writeFileSync(Input, Bytes, { flag: 'wx' });
        for (const [Tool, Pattern] of [[Verifier, /^wvb status=Invalid phase=(?:envelope|metadata|semantic step=[a-z-]+|typed-execution|control-reachability)\n$/u],
            [Runner, /^wvb run status=Unsupported profile=portable-main-i32 phase=envelope\n$/u]]) {
            const Result = await Runˉdevelopmentˉcommand(Tool, [Input], Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 1 || Result.Output !== '' || !Pattern.test(Normalize(Result.Error))) {
                Reject(`Malformed record collection did not reject: ${Label}\n${Result.Output}${Result.Error}`);
            }
        }
        process.stdout.write(`PASS record collection malformed case=${Label}\n`);
    }
    const Header = Source.slice(0, Source.indexOf('record Span'));
    const Invalidˉfields = [['shared-bytes', 'bytes'], ['shared-text', 'text'],
        ['owned-vector', 'Collections.Vector<u32>'], ['shared-sequence', 'Collections.Sequence<u32>'],
        ['empty-record', null]];
    for (const [Label, Field] of Invalidˉfields) {
        const Input = path.join(Work, 'Record-rejected-' + Label + '.wv');
        const Output = path.join(Work, 'Record-rejected-' + Label + '.wvb');
        writeFileSync(Input, Field === null ? Emptyˉrecord : Header + `record Nested<T> { Value: T; }
record Bad<T> { Inner: T; }
export fn Main(Budget: Memory.Memoryˉbudget) -> i32 {
    let Created: Result.Result<Collections.Vector<Bad<Nested<${Field} > > >, Memory.Allocationˉfailure> =
        Collections.Vectorˉconstructˉreserved::<Bad<Nested<${Field} > > >(Budget, 2u64);
    return 42;
}
`, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(process.execPath,
            [path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'), ...Arguments(Input, Output)],
            Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        const Expected = Field === null ? /symbol-status=Emptyˉrecord/u : /wir-status=Genericˉresolution/u;
        if (Result.Code !== 1 || existsSync(Output) || !Expected.test(Normalize(Result.Error))) {
            Reject(`Non-Copy record collection was not refused before publication: ${Label}\n${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS record collection source rejection case=${Label}\n`);
    }
    const Bounds = [['length', '2u64'], ['high-half-only', '4294967296u64'], ['maximum', '18446744073709551615u64']];
    for (const [Label, Index] of Bounds) {
        const Input = path.join(Work, 'Record-bounds-' + Label + '.wv');
        const Output = path.join(Work, 'Record-bounds-' + Label + '.wvb');
        writeFileSync(Input, Replace(Source, 'Forward(borrow Item, 1u64)', `Forward(borrow Item, ${Index})`), { flag: 'wx' });
        await Runˉnode('record-bounds-' + Label, 'Run-Split-Compiler.mjs', Arguments(Input, Output));
        if (Normalize(await Run('record-bounds-' + Label + '-verify', Verifier, [Output])) !==
            'wvb status=Valid profile=compiler-aligned\n') Reject('Record bounds module is not structurally valid.');
        const Result = await Runˉdevelopmentˉcommand(Runner, [Output], Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || Result.Output !== '' ||
            !/^wvb run status=Failed code=3008 instructions=[1-9][0-9]*\n$/u.test(Normalize(Result.Error))) {
            Reject(`Record bounds violation did not terminate: ${Label}\n${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS record collection bounds rejection case=${Label}\n`);
    }
    process.stdout.write(`native record collection elements status=Passed cases=${Cases.length} malformed=${Mutations.length + Helperˉmutations.length} source-rejections=${Invalidˉfields.length + Helperˉrejections.length} bounds=${Bounds.length} stack-owners=1 qualification=false elapsed-ms=${Date.now() - Started}\n`);
}

async function Verifyˉvectorˉindexedˉborrows(Arguments, Verifier, Runner) {
    const Fixture = path.join(Repositoryˉroot,
        'Tests/Fixtures/Language-1.0/Foundation-Vector-Indexed-Borrow-Executable.wv');
    Requireˉordinaryˉfile(Fixture, 8192, 'indexed Vector borrow fixture');
    const Source = readFileSync(Fixture, 'utf8');
    function Replace(Text, Before, After) {
        if (Text.split(Before).length !== 2) Reject('Indexed Vector mutation is ambiguous: ' + Before);
        return Text.replace(Before, After);
    }
    function Resultˉsource(Failure) {
        let Text = Replace(Source, 'let Owner: Option.Option<Collections.Vector<i32> >',
            Failure ? 'let Owner: Result.Result<u32, Collections.Vector<i32> >' :
                'let Owner: Result.Result<Collections.Vector<i32>, u32>');
        Text = Replace(Text, 'Option.Option.Present<Collections.Vector<i32> > { Value: Values }',
            Failure ? 'Result.Result.Failure<u32, Collections.Vector<i32> > { Error: Values }' :
                'Result.Result.Valid<Collections.Vector<i32>, u32> { Value: Values }');
        return Replace(Text, 'Option.Borrow(borrow Owner)',
            Failure ? 'Result.Borrowˉfailure(borrow Owner)' : 'Result.Borrowˉvalid(borrow Owner)');
    }
    const Localˉread = 'let Localˉcopy: i32 = Collections.Vectorˉborrowˉat(borrow Values, 0u64);';
    const Local = Replace(Source, 'Observe(Values)', `if Forward(borrow Values, 0u64) != 42 { return 16; }
                    ${Localˉread}
                    if Localˉcopy != 42 { return 15; }
                    if Forward(borrow Values, 0u64) != 42 { return 18; }
                    Readˉscalar(Collections.Vectorˉborrowˉat::<i32>(borrow Values, 0u64))`);
    let Multiple = Replace(Source, 'Collections.Vectorˉappend::<i32>(borrow mut Values, 42)',
        'Collections.Vectorˉappend::<i32>(borrow mut Values, 7)');
    Multiple = Replace(Multiple, 'if Readˉexclusive(borrow mut Values) != 42 { return 3; }\n                    Observe(Values)',
        `let Second: Result.Result<unit, Collections.Vectorˉappendˉfailure<i32> > =
                        Collections.Vectorˉappend::<i32>(borrow mut Values, 42);
                    match Second {
                        case Result.Result.Valid { Value: Secondˉaccepted } {
                            if Readˉexclusive(borrow mut Values) != 42 { return 3; }
                            Observe(Values)
                        }
                        case Result.Result.Failure { Error: Secondˉfailure } { 4 }
                    }`);
    Multiple = Replace(Multiple, 'Length(borrow Value) != 1u64', 'Length(borrow Value) != 2u64')
        .replaceAll('0u64', '1u64');
    // Keep Main and its failure codes i32; only the collection element and
    // borrowed scalar helpers widen. Distinct cells expose wrong-index reads.
    let Wide = Multiple.replaceAll('Vector<i32>', 'Vector<u64>')
        .replaceAll('Vectorˉappendˉfailure<i32>', 'Vectorˉappendˉfailure<u64>')
        .replaceAll('::<i32>', '::<u64>')
        .replaceAll('!= 42', '!= 18446744073709551615u64');
    for (const [Before, After] of [
        ['fn Readˉscalar(Value: borrow i32) -> i32', 'fn Readˉscalar(Value: borrow u64) -> u64'],
        ['fn Read(Value: borrow Collections.Vector<u64>, Index: u64) -> i32',
            'fn Read(Value: borrow Collections.Vector<u64>, Index: u64) -> u64'],
        ['fn Forward(Value: borrow Collections.Vector<u64>, Index: u64) -> i32',
            'fn Forward(Value: borrow Collections.Vector<u64>, Index: u64) -> u64'],
        ['fn Readˉexclusive(Value: borrow mut Collections.Vector<u64>) -> i32',
            'fn Readˉexclusive(Value: borrow mut Collections.Vector<u64>) -> u64'],
        ['export fn Copyˉbeforeˉexclusive(Left: borrow Collections.Vector<u64>, Right: borrow mut Collections.Vector<u64>) -> i32',
            'export fn Copyˉbeforeˉexclusive(Left: borrow Collections.Vector<u64>, Right: borrow mut Collections.Vector<u64>) -> u64'],
        ['export fn Copyˉloop(Left: borrow Collections.Vector<u64>, Right: borrow mut Collections.Vector<u64>) -> i32',
            'export fn Copyˉloop(Left: borrow Collections.Vector<u64>, Right: borrow mut Collections.Vector<u64>) -> u64'],
        ['var Sum: i32 = 0;', 'var Sum: u64 = 0u64;'],
        ['let Observed: i32 =', 'let Observed: u64 ='],
        ['let Copied: i32 =', 'let Copied: u64 ='],
        ['return 17;', 'return 17u64;'],
        ['Collections.Vectorˉappend::<u64>(borrow mut Values, 7)',
            'Collections.Vectorˉappend::<u64>(borrow mut Values, 7u64)'],
        ['Collections.Vectorˉappend::<u64>(borrow mut Values, 42)',
            'Collections.Vectorˉappend::<u64>(borrow mut Values, 18446744073709551615u64)'],
    ]) Wide = Replace(Wide, Before, After);
    const Mixedˉsource = Replace(Source, 'fn Readˉscalar(', `fn Mixedˉread(Value: borrow i32, Right: borrow Collections.Vector<i32>) -> i32 {
    return Value;
}
export fn Mixedˉreadˉcall(Left: borrow Collections.Vector<i32>, Right: borrow mut Collections.Vector<i32>) -> i32 {
    return Mixedˉread(Collections.Vectorˉborrowˉat(borrow Left, 0u64), borrow Right);
}
fn Readˉscalar(`);
    let Enumˉsource = Source.replaceAll('Vector<i32>', 'Vector<Indexedˉenum>')
        .replaceAll('Vectorˉappendˉfailure<i32>', 'Vectorˉappendˉfailure<Indexedˉenum>')
        .replaceAll('::<i32>', '::<Indexedˉenum>');
    Enumˉsource = Replace(Enumˉsource,
        'fn Readˉscalar(Value: borrow i32) -> i32 { return Value; }',
        `enum Indexedˉenum: u8 { Selected = 2u8; }
enum Otherˉenum: u8 { Different = 2u8; }
export fn Otherˉenumˉtype(Value: Otherˉenum) -> i32 { return 0; }
fn Readˉscalar(Value: borrow Indexedˉenum) -> i32 {
    let Copied: Indexedˉenum = Value;
    if Copied != Indexedˉenum.Selected { return 19; }
    let Name: bytes = Textˉtoˉutf8(Enumˉname(Copied));
    if Bytesˉlength(Name) != 8u32 { return 20; }
    if Bytesˉreadˉu8(Name, 0u32) != 83u8 { return 21; }
    return 42;
}`);
    Enumˉsource = Replace(Enumˉsource,
        'return Collections.Vectorˉborrowˉat(borrow Left, 0u64) + Readˉexclusive(borrow mut Right);',
        'return Readˉscalar(Collections.Vectorˉborrowˉat(borrow Left, 0u64)) + Readˉexclusive(borrow mut Right);');
    Enumˉsource = Replace(Enumˉsource, 'let Copied: i32 = Collections.Vectorˉborrowˉat(borrow Item, 0u64);',
        'let Copied: Indexedˉenum = Collections.Vectorˉborrowˉat(borrow Item, 0u64);');
    Enumˉsource = Replace(Enumˉsource, 'if Copied != 42 { return 11; }',
        'if Copied != Indexedˉenum.Selected { return 11; }');
    Enumˉsource = Replace(Enumˉsource, 'Collections.Vectorˉappend::<Indexedˉenum>(borrow mut Values, 42)',
        'Collections.Vectorˉappend::<Indexedˉenum>(borrow mut Values, Indexedˉenum.Selected)');
    const Cases = [
        ['option-projected', Mixedˉsource],
        ['result-valid-projected', Resultˉsource(false)],
        ['result-failure-projected', Resultˉsource(true)],
        ['owned-local', Local],
        ['repeated-helper-loop', Replace(Source, 'while Iteration < 4u32', 'while Iteration < 64u32')],
        ['nonzero-index-multiple-elements', Multiple],
        ['full-width-u64-nonzero-index', Wide],
        ['enum-nominal-element', Enumˉsource],
    ];
    function Inspect(Bytes, Requireˉprojection = true) {
        if (Bytes.length > 16384 || Bytes.readUInt16LE(6) !== 41) {
            Reject('Indexed Vector version or module bound differs.');
        }
        const Sections = Parseˉsections(Bytes);
        const Reads = [];
        for (const Entry of Parseˉfunctionˉentries(Bytes, Sections[4])) {
            const Function = Parseˉfunction(Bytes, Sections[4], Entry.name);
            const Begin = Sections[5].payload + Entry.codeOffset;
            const End = Begin + Entry.codeLength;
            for (let Cursor = Begin; Cursor < End;) {
                const Width = Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
                if (Cursor + Width > End) Reject('Indexed Vector instruction is truncated.');
                if (Bytes[Cursor] === 227) Reads.push({ offset: Cursor, function: Function, name: Entry.name });
                Cursor += Width;
            }
        }
        for (const [Name, Mode] of [['Read', 26], ['Readˉexclusive', 27]]) {
            const Read = Reads.find(Value => Value.name === Name);
            if (Read === undefined || Bytes[Read.function.parameterShapeOffsets[0]] !== Mode ||
                Bytes.readUInt32LE(Read.offset + 1) !== 0) {
                Reject('Indexed Vector parameter read mode differs: ' + Name);
            }
        }
        const Projected = Reads.find(Value => Value.name === 'Observe');
        if (Requireˉprojection && (Projected === undefined || !Projected.function.localShapeOffsets.some(Offset =>
            Bytes[Offset] === 37 && Bytes[Offset + 1] === 23))) {
            Reject('Indexed Vector projection did not retain a borrowed Vector owner.');
        }
        const Scalar = Parseˉfunction(Bytes, Sections[4], 'Readˉscalar');
        const Borrow = Readˉshape(Bytes, Scalar.parameterShapeOffsets[0]);
        const Types = Parseˉtypes(Bytes, Sections[7]);
        const Primary = Reads.find(Value => Value.name === 'Read');
        const Vector = Types[Bytes.readUInt32LE(Primary.offset + 5)];
        const Element = Vector?.element;
        if (Vector?.kind !== 5 || Element === null || Element === undefined ||
            Borrow.shape !== 37 || Borrow.inner.shape !== Element.shape ||
            Borrow.inner.typeIndex !== Element.typeIndex ||
            (Element.shape === 8 ? Types[Element.typeIndex]?.kind !== 7 || Scalar.returnShape !== 1 :
                Borrow.inner.shape !== Scalar.returnShape)) {
            Reject('Indexed Vector helper does not consume the exact scalar borrow shape.');
        }
        for (const Read of Reads) {
            const Vectorˉtype = Bytes.readUInt32LE(Read.offset + 5);
            const Readˉelement = Types[Vectorˉtype]?.element;
            const Slot = Bytes.readUInt32LE(Read.offset + 1);
            const Shapeˉoffset = Slot < Read.function.parameterCount
                ? Read.function.parameterShapeOffsets[Slot]
                : Read.function.localShapeOffsets[Slot - Read.function.parameterCount];
            if (Shapeˉoffset === undefined) Reject('Indexed Vector owner slot is absent.');
            let Owner = Readˉshape(Bytes, Shapeˉoffset);
            if (Owner.shape === 37) Owner = Owner.inner;
            if (Types[Vectorˉtype]?.kind !== 5 || ![23, 26, 27].includes(Owner.shape) ||
                Owner.typeIndex !== Vectorˉtype || Readˉelement?.shape !== Element.shape ||
                Readˉelement?.typeIndex !== Element.typeIndex) {
                Reject('Indexed Vector owner or element nominal identity differs.');
            }
        }
        if (!Reads.some(Value => Value.name === 'Copyˉbeforeˉexclusive') ||
            !Reads.some(Value => Value.name === 'Copyˉloop')) {
            Reject('Indexed Vector Copy/exclusive complete-verifier guard was not retained.');
        }
        return { sections: Sections, reads: Reads, scalar: Scalar, types: Types, element: Element };
    }
    function Resultˉtemporaryˉshape(Bytes, Read) {
        const Store = Read.offset + 9;
        if (Store + 5 > Bytes.length || Bytes[Store] !== 5) {
            Reject('Indexed Vector result is not stored in a temporary.');
        }
        const Slot = Bytes.readUInt32LE(Store + 1);
        const Offset = Read.function.localShapeOffsets[Slot - Read.function.parameterCount];
        if (Slot < Read.function.parameterCount || Offset === undefined || Bytes[Offset] !== 37) {
            Reject('Indexed Vector immutable helper result lacks a borrowed temporary.');
        }
        return Offset;
    }
    let Candidate = null;
    let Enumˉcandidate = null;
    for (const [Index, [Label, Text]] of Cases.entries()) {
        const Input = path.join(Work, 'Indexed-' + Label + '.wv');
        writeFileSync(Input, Text, { flag: 'wx' });
        let First = null;
        let Instructions = 0;
        for (const Generation of ['a', 'b']) {
            const Output = path.join(Work, 'Indexed-' + Label + '-' + Generation + '.wvb');
            await Runˉnode('indexed-' + Label + '-' + Generation, 'Run-Split-Compiler.mjs', Arguments(Input, Output, true));
            const Bytes = readFileSync(Output);
            const Layout = Inspect(Bytes, Label !== 'owned-local');
            if (Label === 'owned-local' && !Layout.reads.some(Read => Read.name === 'Main')) {
                Reject('Indexed Vector owned-local read is absent.');
            }
            if (First !== null && !First.equals(Bytes)) Reject('Indexed Vector publication is not deterministic.');
            First = Bytes;
            if (Generation === 'a') {
                if (Normalize(await Run('indexed-' + Label + '-verify', Verifier, [Output])) !==
                    'wvb status=Valid profile=compiler-aligned\n') Reject('Indexed Vector verification differs: ' + Label);
                const Execution = Normalize(await Run('indexed-' + Label + '-execute', Runner, [Output, '--report-steps']));
                const Report = /^Result: 42\nInstructions: ([1-9][0-9]*)\n$/u.exec(Execution);
                if (Report === null || Number(Report[1]) > 80000) {
                    Reject('Indexed Vector execution or instruction bound differs: ' + Label);
                }
                Instructions = Number(Report[1]);
            }
        }
        if (Label === 'option-projected') Candidate = First;
        if (Label === 'enum-nominal-element') Enumˉcandidate = First;
        process.stdout.write(`PASS Vector indexed borrow item=${Index + 1}/${Cases.length} case=${Label} ` +
            `instructions=${Instructions} wvb-bytes=${First.length} wvb-sha256=${Digest(First)}\n`);
    }
    const Layout = Inspect(Candidate);
    const Read = Layout.reads.find(Value => Value.name === 'Read');
    const First = Read.offset;
    const Types = Parseˉtypes(Candidate, Layout.sections[7]);
    const Exactˉtype = Candidate.readUInt32LE(First + 5);
    const Otherˉvector = Types.findIndex((Type, Index) => Type.kind === 5 && Index !== Exactˉtype);
    const Otherˉkind = Types.findIndex(Type => Type.kind !== 5);
    if (Otherˉvector < 0 || Otherˉkind < 0) Reject('Indexed Vector nominal mutation controls are absent.');
    const Resultˉshape = Resultˉtemporaryˉshape(Candidate, Read);
    if (Candidate[Resultˉshape + 1] !== Layout.element.shape) {
        Reject('Indexed Vector scalar result mutation control differs.');
    }
    const Mixed = Parseˉfunction(Candidate, Layout.sections[4], 'Mixedˉread');
    const Mixedˉcaller = Parseˉfunction(Candidate, Layout.sections[4], 'Mixedˉreadˉcall');
    const Mixedˉread = Layout.reads.find(Value => Value.name === 'Mixedˉreadˉcall');
    if (Mixed.parameterCount !== 2 || Mixedˉcaller.parameterCount !== 2 ||
        Candidate[Mixed.parameterShapeOffsets[0]] !== 37 ||
        Candidate[Mixed.parameterShapeOffsets[0] + 1] !== Layout.element.shape ||
        Candidate[Mixed.parameterShapeOffsets[1]] !== 26 ||
        Candidate[Mixedˉcaller.parameterShapeOffsets[1]] !== 27 ||
        Mixed.parameterTypeIndices[1] !== Mixedˉcaller.parameterTypeIndices[1] ||
        Mixedˉread === undefined ||
        Candidate[Resultˉtemporaryˉshape(Candidate, Mixedˉread) + 1] !== Layout.element.shape) {
        Reject('Indexed Vector mixed-call mutation controls differ.');
    }
    const Enumˉlayout = Inspect(Enumˉcandidate);
    const Enumˉread = Enumˉlayout.reads.find(Value => Value.name === 'Read');
    const Enumˉresultˉshape = Resultˉtemporaryˉshape(Enumˉcandidate, Enumˉread);
    const Otherˉenum = Parseˉfunction(Enumˉcandidate, Enumˉlayout.sections[4], 'Otherˉenumˉtype');
    const Otherˉenumˉtype = Otherˉenum.parameterTypeIndices[0];
    if (Enumˉcandidate[Enumˉresultˉshape + 1] !== 8 ||
        Enumˉcandidate.readUInt32LE(Enumˉresultˉshape + 2) !== Enumˉlayout.element.typeIndex ||
        Enumˉcandidate[Otherˉenum.parameterShapeOffsets[0]] !== 8 ||
        Otherˉenumˉtype === Enumˉlayout.element.typeIndex ||
        Enumˉlayout.types[Otherˉenumˉtype]?.kind !== 7) {
        Reject('Indexed Vector enum result mutation controls differ.');
    }
    const Mutations = [
        ['old-minor', Bytes => Bytes.writeUInt16LE(40, 6)],
        ['unknown-opcode', Bytes => { Bytes[First] = 228; }],
        ['slot-boundary', Bytes => Bytes.writeUInt32LE(Read.function.parameterCount + Read.function.localShapes.length, First + 1)],
        ['slot-overflow', Bytes => Bytes.writeUInt32LE(0xffffffff, First + 1)],
        ['scalar-owner-slot', Bytes => Bytes.writeUInt32LE(1, First + 1)],
        ['wrong-vector-type', Bytes => Bytes.writeUInt32LE(Otherˉvector, First + 5)],
        ['wrong-nominal-kind', Bytes => Bytes.writeUInt32LE(Otherˉkind, First + 5)],
        ['type-boundary', Bytes => Bytes.writeUInt32LE(Types.length, First + 5)],
        ['type-overflow', Bytes => Bytes.writeUInt32LE(0xffffffff, First + 5)],
        ['wrong-index-type', Bytes => { Bytes[Read.function.parameterShapeOffsets[1]] = Layout.scalar.returnShape; }],
        ['wrong-borrow-result', Bytes => {
            Bytes[Resultˉshape + 1] = Candidate[Read.function.parameterShapeOffsets[1]];
        }],
        ['mixed-exclusive-call-live-scalar', Bytes => { Bytes[Mixed.parameterShapeOffsets[1]] = 27; }],
        ['wrong-enum-borrow-result', Bytes => Bytes.writeUInt32LE(Otherˉenumˉtype, Enumˉresultˉshape + 2), Enumˉcandidate],
        ['truncated', Bytes => Bytes.subarray(0, First + 8)],
    ];
    for (const [Label, Mutate, Original = Candidate] of Mutations) {
        let Broken = Buffer.from(Original);
        const Replacement = Mutate(Broken);
        if (Buffer.isBuffer(Replacement)) Broken = Replacement;
        if (Broken.equals(Original)) Reject('Indexed Vector mutation changed no bytes: ' + Label);
        const Input = path.join(Work, 'Indexed-malformed-' + Label + '.wvb');
        writeFileSync(Input, Broken, { flag: 'wx' });
        const Typedˉonly = ['wrong-borrow-result', 'mixed-exclusive-call-live-scalar', 'wrong-enum-borrow-result'].includes(Label);
        for (const [Tool, Pattern] of [
            [Verifier, Typedˉonly ? /^wvb status=Invalid phase=typed-execution\n$/u :
                /^wvb status=Invalid phase=(?:semantic step=[a-z-]+|typed-execution|control-reachability)\n$/u],
            [Runner, /^wvb run status=Unsupported profile=portable-main-i32 phase=envelope\n$/u],
        ]) {
            const Result = await Runˉdevelopmentˉcommand(Tool, [Input],
                Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 1 || Result.Output !== '' || !Pattern.test(Normalize(Result.Error))) {
                Reject(`Malformed indexed Vector borrow did not reject: ${Label}\n${Result.Output}${Result.Error}`);
            }
        }
        process.stdout.write(`PASS Vector indexed borrow malformed case=${Label}\n`);
    }
    const Borrow = 'Collections.Vectorˉborrowˉat::<i32>(borrow Value, Index)';
    const Invalidˉsources = [
        ['missing-explicit-borrow', Replace(Source, Borrow, 'Collections.Vectorˉborrowˉat::<i32>(Value, Index)')],
        ['exclusive-borrow-argument', Replace(Source, Borrow, 'Collections.Vectorˉborrowˉat::<i32>(borrow mut Value, Index)')],
        ['wrong-index-type', Replace(Source, Borrow, 'Collections.Vectorˉborrowˉat::<i32>(borrow Value, 0u32)'), /Invalidˉindex|Typeˉmismatch/u],
        ['wrong-element-type', Replace(Source, Borrow, 'Collections.Vectorˉborrowˉat::<u32>(borrow Value, Index)'), /Genericˉresolution/u],
        ['live-loan-append', Replace(Local, Localˉread, `${Localˉread}
                    let Rejected: Result.Result<unit, Collections.Vectorˉappendˉfailure<i32> > =
                        Collections.Vectorˉappend::<i32>(borrow mut Values, 7);`)],
        ['live-loan-consume', Replace(Local, Localˉread, `${Localˉread}
                    let Moved: Collections.Vector<i32> = Values;`)],
        ['live-loan-freeze', Replace(Local, Localˉread, `${Localˉread}
                    let Frozen: Collections.Sequence<i32> = Collections.Vectorˉfreeze(Values);`)],
        ['live-loan-owned-length', Replace(Local, Localˉread, `${Localˉread}
                    let Length: u64 = Collections.Vectorˉlength(borrow Values);`)],
        ['borrow-return-escape', Replace(Source, 'fn Readˉscalar(',
            `export fn Escape(Value: borrow Collections.Vector<i32>) -> borrow i32 {
    return Collections.Vectorˉborrowˉat(borrow Value, 0u64);
}
fn Readˉscalar(`)],
        ['retained-borrow-mixed-exclusive-call', Replace(Source, 'fn Readˉscalar(',
            `fn Mixed(Value: borrow i32, Right: borrow mut Collections.Vector<i32>) -> i32 {
    return Value;
}
export fn Mixedˉcall(Left: borrow Collections.Vector<i32>, Right: borrow mut Collections.Vector<i32>) -> i32 {
    return Mixed(Collections.Vectorˉborrowˉat(borrow Left, 0u64), borrow mut Right);
}
fn Readˉscalar(`), /Unsupportedˉoperation/u],
    ];
    for (const [Label, Text, Diagnostic = /Invalidˉborrow|Invalidˉcollection|Invalidˉwir|Unsupportedˉoperation/u] of Invalidˉsources) {
        const Input = path.join(Work, 'Indexed-invalid-' + Label + '.wv');
        const Output = path.join(Work, 'Indexed-invalid-' + Label + '.wvb');
        writeFileSync(Input, Text, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(process.execPath,
            [path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'), ...Arguments(Input, Output, true)],
            Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || existsSync(Output) || !Diagnostic.test(Normalize(Result.Error))) {
            Reject(`Invalid indexed Vector borrow did not reject before publication: ${Label}\n${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS Vector indexed borrow source rejection case=${Label}\n`);
    }
    const Bounds = [['length', '1u64'], ['high-half-only', '4294967296u64'], ['maximum-index', '18446744073709551615u64']];
    for (const [Label, Index] of Bounds) {
        const Text = Replace(Source, 'return Readˉscalar(Collections.Vectorˉborrowˉat(borrow Value, 0u64));',
            `return Readˉscalar(Collections.Vectorˉborrowˉat(borrow Value, ${Index}));`);
        const Input = path.join(Work, 'Indexed-bounds-' + Label + '.wv');
        const Output = path.join(Work, 'Indexed-bounds-' + Label + '.wvb');
        writeFileSync(Input, Text, { flag: 'wx' });
        await Runˉnode('indexed-bounds-' + Label, 'Run-Split-Compiler.mjs', Arguments(Input, Output, true));
        Inspect(readFileSync(Output));
        if (Normalize(await Run('indexed-bounds-' + Label + '-verify', Verifier, [Output])) !==
            'wvb status=Valid profile=compiler-aligned\n') Reject('Indexed Vector bounds module is not structurally valid.');
        const Result = await Runˉdevelopmentˉcommand(Runner, [Output],
            Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || Result.Output !== '' ||
            !/^wvb run status=Failed code=3008 instructions=[1-9][0-9]*\n$/u.test(Normalize(Result.Error))) {
            Reject(`Indexed Vector bounds violation did not terminate: ${Label}\n${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS Vector indexed borrow bounds rejection case=${Label}\n`);
    }
    return { cases: Cases.length, malformed: Mutations.length, rejections: Invalidˉsources.length, bounds: Bounds.length };
}

async function Verifyˉvectorˉpayloadˉborrows(Arguments, Verifier, Runner) {
    const Fixture = path.join(Repositoryˉroot,
        'Tests/Fixtures/Language-1.0/Foundation-Vector-Payload-Borrow-Executable.wv');
    Requireˉordinaryˉfile(Fixture, 8192, 'borrowed Vector payload fixture');
    const Source = readFileSync(Fixture, 'utf8');
    function Replace(Text, Before, After) {
        if (Text.split(Before).length !== 2) Reject('Vector payload mutation is ambiguous: ' + Before);
        return Text.replace(Before, After);
    }
    const Owner = 'Option.Option.Present<Collections.Vector<i32> > { Value: Values }';
    const Absent = Replace(Replace(Source, Owner,
        'Option.Option.Absent<Collections.Vector<i32> > {}'),
        'case Option.Option.Absent { return 15; }', 'case Option.Option.Absent { return 42; }');
    function Resultˉsource(Failure, Missing) {
        const Shape = Failure ? 'Result.Result<u32, Collections.Vector<i32> >' :
            'Result.Result<Collections.Vector<i32>, u32>';
        let Text = Replace(Source, 'let Owner: Option.Option<Collections.Vector<i32> >', 'let Owner: ' + Shape);
        const Constructor = Failure
            ? Missing ? 'Result.Result.Valid<u32, Collections.Vector<i32> > { Value: 7u32 }'
                : 'Result.Result.Failure<u32, Collections.Vector<i32> > { Error: Values }'
            : Missing ? 'Result.Result.Failure<Collections.Vector<i32>, u32> { Error: 7u32 }'
                : 'Result.Result.Valid<Collections.Vector<i32>, u32> { Value: Values }';
        Text = Replace(Text, Owner, Constructor);
        Text = Replace(Text, 'Option.Borrow(borrow Owner)',
            Failure ? 'Result.Borrowˉfailure(borrow Owner)' : 'Result.Borrowˉvalid(borrow Owner)');
        if (Missing) Text = Replace(Text, 'case Option.Option.Absent { return 15; }',
            'case Option.Option.Absent { return 42; }');
        return Text;
    }
    const Noˉread = Replace(Replace(Source,
        'return Collections.Vectorˉlength(borrow Value);', 'return 1u64;'),
        'Observe(Values, 0u64)', 'Observe(Values, 1u64)');
    // Separate guest-budget reuse from helper-loop work to stay within the
    // hosted interpreter's fixed text arena; neither limit is raised.
    const Repeated = Replace(Replace(Source, 'while Iteration < 1u32',
        'while Iteration < 4u32'), 'while Iteration < 128u32', 'while Iteration < 16u32');
    const Phantom = Replace(Replace(Source, 'fn Read(', `record Copyˉmarker<T> { Marker: u32; }
fn Copyˉtwice(Value: Copyˉmarker<Collections.Vector<i32> >) -> u32 {
    let First: Copyˉmarker<Collections.Vector<i32> > = Value;
    let Second: Copyˉmarker<Collections.Vector<i32> > = Value;
    return First.Marker + Second.Marker;
}
fn Read(`), 'fn Observe(Values: Collections.Vector<i32>, Expected: u64) -> i32 {',
        `fn Observe(Values: Collections.Vector<i32>, Expected: u64) -> i32 {
    let Control: Copyˉmarker<Collections.Vector<i32> > =
        Copyˉmarker<Collections.Vector<i32> > { Marker: 21u32 };
    if Copyˉtwice(Control) != 42u32 { return 16; }`);
    const Cases = [
        ['option-present', Source, true], ['option-absent', Absent, true],
        ['result-valid', Resultˉsource(false, false), true],
        ['result-valid-absent', Resultˉsource(false, true), true],
        ['result-failure', Resultˉsource(true, false), true],
        ['result-failure-absent', Resultˉsource(true, true), true],
        ['without-parameter-read', Noˉread, false],
        ['repeated-helper-loop', Repeated, true, 16],
        ['phantom-vector-copy', Phantom, true],
    ];
    let Candidate = null;
    let Withoutˉread = null;
    let Completed = 0;
    for (const [Label, Text, Hasˉread, Trips = 128] of Cases) {
        const Input = path.join(Work, 'Payload-' + Label + '.wv');
        writeFileSync(Input, Text, { flag: 'wx' });
        let First = null;
        let Instructions = 0;
        for (const Generation of ['a', 'b']) {
            const Output = path.join(Work, 'Payload-' + Label + '-' + Generation + '.wvb');
            await Runˉnode('payload-' + Label + '-' + Generation, 'Run-Split-Compiler.mjs', Arguments(Input, Output, true));
            const Bytes = readFileSync(Output);
            if (Bytes.length > 16384 || Bytes.readUInt16LE(6) !== 40) Reject('Vector payload version or bound differs.');
            const Sections = Parseˉsections(Bytes);
            let Reads = 0;
            let Projections = 0;
            let Borrowedˉvectors = 0;
            for (const Entry of Parseˉfunctionˉentries(Bytes, Sections[4])) {
                const Function = Parseˉfunction(Bytes, Sections[4], Entry.name);
                for (const Offset of Function.localShapeOffsets) {
                    const Shape = Readˉshape(Bytes, Offset);
                    if (Shape.shape === 37 && Shape.inner.shape === 23) Borrowedˉvectors += 1;
                }
                const Begin = Sections[5].payload + Entry.codeOffset;
                const End = Begin + Entry.codeLength;
                for (let Cursor = Begin; Cursor < End;) {
                    const Width = Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
                    if (Cursor + Width > End) Reject('Vector payload instruction is truncated.');
                    if (Bytes[Cursor] === 226) Reads += 1;
                    if (Bytes[Cursor] === 225) Projections += 1;
                    Cursor += Width;
                }
            }
            if ((Reads !== 0) !== Hasˉread || Projections === 0 || Borrowedˉvectors === 0) {
                Reject('Vector payload projection, borrowed local, or parameter-read evidence differs.');
            }
            if (First !== null && !First.equals(Bytes)) Reject('Vector payload publication is not deterministic.');
            First = Bytes;
            if (Generation === 'a') {
                if (Normalize(await Run('payload-' + Label + '-verify', Verifier, [Output])) !==
                    'wvb status=Valid profile=compiler-aligned\n') {
                    Reject('Vector payload verification differs: ' + Label);
                }
                const Execution = Normalize(await Run('payload-' + Label + '-execute', Runner,
                    [Output, '--report-steps']));
                const Report = /^Result: 42\nInstructions: ([1-9][0-9]*)\n$/u.exec(Execution);
                if (Report === null || Number(Report[1]) > 80000) {
                    Reject('Vector payload execution or instruction bound differs: ' + Label);
                }
                Instructions = Number(Report[1]);
            }
        }
        if (Label === 'option-present') Candidate = First;
        if (!Hasˉread) Withoutˉread = First;
        Completed += 1;
        process.stdout.write(`PASS Vector payload item=${Completed}/${Cases.length} case=${Label} child-budget-trips=${Trips} ` +
            `instructions=${Instructions} wvb-bytes=${First.length} wvb-sha256=${Digest(First)}\n`);
    }
    const Sections = Parseˉsections(Candidate);
    const Observe = Parseˉfunction(Candidate, Sections[4], 'Observe');
    const Forward = Parseˉfunction(Candidate, Sections[4], 'Forward');
    const Borrowed = Observe.localShapeOffsets.find(Offset =>
        Candidate[Offset] === 37 && Candidate[Offset + 1] === 23);
    if (Borrowed === undefined || Candidate[Forward.parameterShapeOffsets[0]] !== 26) {
        Reject('Vector payload mutation targets differ.');
    }
    const Exactˉtype = Candidate.readUInt32LE(Borrowed + 2);
    const Types = Parseˉtypes(Candidate, Sections[7]);
    const Otherˉvector = Types.findIndex((Type, Index) => Type.kind === 5 && Index !== Exactˉtype);
    const Otherˉkind = Types.findIndex(Type => Type.kind !== 5);
    if (Otherˉvector < 0 || Otherˉkind < 0) Reject('Vector payload exact-type controls are missing.');
    function Borrowedˉparameter(Bytes) {
        const Offset = Forward.parameterShapeOffsets[0];
        const Result = Buffer.concat([Bytes.subarray(0, Offset), Buffer.from([37]), Bytes.subarray(Offset)]);
        Result[Offset + 1] = 23;
        Result.writeUInt32LE(Sections[4].length + 1, Sections[4].header + 4);
        return Result;
    }
    const Mutations = [
        ['old-minor', Candidate, Bytes => Bytes.writeUInt16LE(39, 6)],
        ['old-minor-without-read', Withoutˉread, Bytes => Bytes.writeUInt16LE(39, 6)],
        ['wrong-exact-vector', Candidate, Bytes => Bytes.writeUInt32LE(Otherˉvector, Borrowed + 2)],
        ['wrong-nominal-kind', Candidate, Bytes => Bytes.writeUInt32LE(Otherˉkind, Borrowed + 2)],
        ['nominal-boundary', Candidate, Bytes => Bytes.writeUInt32LE(Types.length, Borrowed + 2)],
        ['nominal-overflow', Candidate, Bytes => Bytes.writeUInt32LE(0xffffffff, Borrowed + 2)],
        ['borrowed-vector-parameter', Candidate, Borrowedˉparameter],
        ['consuming-call', Candidate, Bytes => { Bytes[Forward.parameterShapeOffsets[0]] = 23; }],
        ['exclusive-call', Candidate, Bytes => { Bytes[Forward.parameterShapeOffsets[0]] = 27; }],
    ];
    for (const [Label, Original, Mutate] of Mutations) {
        let Broken = Buffer.from(Original);
        const Replacement = Mutate(Broken);
        if (Buffer.isBuffer(Replacement)) Broken = Replacement;
        if (Broken.equals(Original)) Reject('Vector payload mutation changed no bytes: ' + Label);
        const Input = path.join(Work, 'Payload-malformed-' + Label + '.wvb');
        writeFileSync(Input, Broken, { flag: 'wx' });
        for (const [Tool, Pattern] of [
            [Verifier, /^wvb status=Invalid phase=(?:semantic step=[a-z-]+|typed-execution|control-reachability)\n$/u],
            [Runner, /^wvb run status=Unsupported profile=portable-main-i32 phase=envelope\n$/u],
        ]) {
            const Result = await Runˉdevelopmentˉcommand(Tool, [Input],
                Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 1 || Result.Output !== '' || !Pattern.test(Normalize(Result.Error))) {
                Reject(`Malformed Vector payload did not reject: ${Label}\n${Result.Output}${Result.Error}`);
            }
        }
        process.stdout.write(`PASS Vector payload malformed case=${Label}\n`);
    }
    const Firstˉcall = 'if Forward(borrow Item, 7u32) != Expected { return 11; }';
    function Invalidˉhelper(Declaration, Call) {
        return Replace(Replace(Source, 'fn Observe(', Declaration + '\nfn Observe('), Firstˉcall, Call);
    }
    const Invalidˉsources = [
        ['consume-projected-vector', Invalidˉhelper(
            'fn Consume(Value: Collections.Vector<i32>) -> u64 { return 0u64; }',
            'if Consume(Item) != Expected { return 11; }')],
        ['exclusive-projected-vector', Invalidˉhelper(
            'fn Mutate(Value: borrow mut Collections.Vector<i32>) -> u64 { return 0u64; }',
            'if Mutate(borrow mut Item) != Expected { return 11; }'),
            /^source analysis status=Sourceˉwir symbol-status=Valid binding-status=Valid wir-status=Invalidˉborrow failure-module=0 related-module=0 function=[0-9]+ offset=[0-9]+ line=[0-9]+ column=[0-9]+\n$/u],
        ['consume-frozen-owner', Invalidˉhelper(
            'fn Consumeˉowner(Value: Option.Option<Collections.Vector<i32> >) -> u32 { return 7u32; }',
            'if Forward(borrow Item, Consumeˉowner(Owner)) != Expected { return 11; }')],
    ];
    for (const [Label, Text, Diagnostic = /Invalidˉwir/u] of Invalidˉsources) {
        const Input = path.join(Work, 'Payload-invalid-' + Label + '.wv');
        const Output = path.join(Work, 'Payload-invalid-' + Label + '.wvb');
        writeFileSync(Input, Text, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(process.execPath,
            [path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'), ...Arguments(Input, Output, true)],
            Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || existsSync(Output) || !Diagnostic.test(Normalize(Result.Error))) {
            Reject(`Invalid Vector payload ownership did not reject: ${Label}\n${Result.Output}${Result.Error}`);
        }
        process.stdout.write(`PASS Vector payload source rejection case=${Label}\n`);
    }
    return { cases: Cases.length, malformed: Mutations.length, rejections: Invalidˉsources.length };
}

async function Verifyˉfoundationˉownedˉpayloads(Admitter, Authenticator, Analyzer, Emitter, Target, Verifier, Runner) {
    // Explicit products keep cold compiler/runner construction out of this selection.
    for (const Product of [Admitter, Authenticator, Analyzer, Emitter, Verifier, Runner]) {
        Requireˉordinaryˉfile(Product, 134_217_728, 'owned payload predecessor');
    }
    const Fixture = path.join(Repositoryˉroot, 'Tests/Fixtures/Language-1.0/Foundation-Value-Borrow-Vector-Executable.wv');
    Requireˉordinaryˉfile(Fixture, 8192, 'owned payload fixture');
    const Source = readFileSync(Fixture, 'utf8');
    function Replace(Source, Before, After) {
        if (Source.split(Before).length !== 2) Reject('Owned payload mutation is ambiguous.');
        return Source.replace(Before, After);
    }
    function Resultˉsource(Failure) {
        let Text = Replace(Source, 'Option.Option<Payload<T> >',
            Failure ? 'Result.Result<u32, Payload<T> >' : 'Result.Result<Payload<T>, u32>');
        Text = Replace(Text, 'Option.Option.Present<Payload<T> >', Failure
            ? 'Result.Result.Failure<u32, Payload<T> >' : 'Result.Result.Valid<Payload<T>, u32>');
        if (Failure) Text = Replace(Text, 'Value: Payload<T>', 'Error: Payload<T>');
        return Replace(Text, 'Option.Borrow(borrow Owner)',
            Failure ? 'Result.Borrowˉfailure(borrow Owner)' : 'Result.Borrowˉvalid(borrow Owner)');
    }
    function Reclaimˉsource(Text) {
        // 128 sequential child budgets exceed both the root's bytes and child
        // limit if returning from Observe retains the owned Vector or its lease.
        return Text.slice(0, Text.indexOf('export fn Main(')) + `
export fn Main(Budget: Memory.Memoryˉbudget) -> i32 {
    var Parent: Memory.Memoryˉbudget = Budget;
    var Iteration: u32 = 0u32;
    while Iteration < 128u32 {
        let Split: Result.Result<Memory.Memoryˉbudget, Memory.Allocationˉfailure> =
            Memory.Split(borrow mut Parent, 8192u64, 0u32);
        let Observed: i32 = match Split {
            case Result.Result.Valid { Value: Child } {
                let Constructed: Result.Result<Collections.Vector<i32>, Memory.Allocationˉfailure> =
                    Collections.Vectorˉconstructˉreserved::<i32>(Child, 4u64);
                match Constructed {
                    case Result.Result.Valid { Value: Values } { Observe(Values) }
                    case Result.Result.Failure { Error: Failure } { 1 }
                }
            }
            case Result.Result.Failure { Error: Splitˉfailure } { 3 }
        };
        if Observed != 42 { return Observed; }
        Iteration = Iteration + 1u32;
    }
    return 42;
}
`;
    }
    const Refusedˉreclaim = Replace(Replace(Replace(Reclaimˉsource(Source),
        '8192u64', '16u64'),
        '{ Observe(Values) }', '{ Observe(Values) + 1 }'),
        'case Result.Result.Failure { Error: Failure } { 1 }',
        `case Result.Result.Failure { Error: Failure } {
            if Failure.Reason == Memory.Allocationˉreason.Budgetˉexhausted &&
                Failure.Requestedˉbytes == 40u64 && Failure.Availableˉbytes == 16u64 {
                42
            } else { 4 }
        }`);
    const Unaddressable = Replace(Replace(Replace(Source,
        '(Budget, 4u64)', '(Budget, 2048u64)'),
        '            Observe(Values)', '            Observe(Values) + 1'),
        'case Result.Result.Failure { Error: Failure } { 1 }',
        `case Result.Result.Failure { Error: Failure } {
            if Failure.Reason == Memory.Allocationˉreason.Targetˉunaddressable &&
                Failure.Requestedˉbytes == 16392u64 && Failure.Availableˉbytes == 0u64 {
                42
            } else { 4 }
        }`);
    const Cases = [
        ['option-record-vector', Source, true],
        ['result-valid-record-vector', Resultˉsource(false), true],
        ['result-failure-record-vector', Resultˉsource(true), true],
        ['option-record-copy', Source.slice(0, Source.indexOf('export fn Main(')) +
            'export fn Main() -> i32 { return Observe(7); }\n', false],
        ['option-record-reclaim', Reclaimˉsource(Source), true],
        ['result-valid-record-reclaim', Reclaimˉsource(Resultˉsource(false)), true],
        ['result-failure-record-reclaim', Reclaimˉsource(Resultˉsource(true)), true],
        ['option-record-refused-reclaim', Refusedˉreclaim, true],
        ['option-record-unaddressable', Unaddressable, true],
    ];
    let Completed = 0;
    for (const [Label, Text, Owned] of Cases) {
        const Input = path.join(Work, Label + '.wv');
        writeFileSync(Input, Text, { flag: 'wx' });
        let First = null;
        for (const Generation of ['a', 'b']) {
            const Output = path.join(Work, Label + '-' + Generation + '.wvb');
            await Runˉnode(Label + '-' + Generation, 'Run-Split-Compiler.mjs', [
                Admitter, Authenticator, Analyzer, Emitter,
                '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
                '--source-profile', Sourceˉprofile, '--target-descriptor', Target,
                Input, ...['Collections/Collections.wv', 'Memory/Memory.wv',
                    'Values/Option.wv', 'Values/Result.wv'].map(Name =>
                    path.join(Repositoryˉroot, 'Libraries/Foundation', Name)), Output,
            ]);
            const Bytes = readFileSync(Output);
            if (Bytes.readUInt16LE(6) !== 39 || Bytes.length > 8192) {
                Reject('Owned payload fixture has an unexpected version or size.');
            }
            const Sections = Parseˉsections(Bytes);
            const Functions = Parseˉfunctionˉentries(Bytes, Sections[4]);
            if (Functions.length < 2) Reject('Owned payload helper was not emitted.');
            if (First !== null && !First.equals(Bytes)) Reject('Owned payload publication is not deterministic.');
            First = Bytes;
            if (Generation === 'a') {
                const Verified = await Run(Label + '-verify', Verifier, [Output]);
                if (Normalize(Verified) !== 'wvb status=Valid profile=compiler-aligned\n') {
                    Reject('Owned payload verifier output differs.');
                }
                const Executed = await Run(Label + '-execute', Runner, [Output]);
                if (Normalize(Executed) !== 'Result: 42\n') Reject('Payload execution result differs.');
                const Transfers = [];
                for (const Function of Functions) {
                    const Begin = Sections[5].payload + Function.codeOffset;
                    const End = Begin + Function.codeLength;
                    for (let Cursor = Begin; Cursor < End;) {
                        const Width = Wvbˉinstructionˉwidthˉat(Bytes, Cursor);
                        if (Width < 1 || Cursor + Width > End) Reject('Invalid fixture instruction extent.');
                        if (Bytes[Cursor] === 205 && Cursor + 5 < End && Bytes[Cursor + 5] === 151) {
                            Transfers.push(Cursor);
                        }
                        Cursor += Width;
                    }
                }
                if (Transfers.length !== (Owned ? 1 : 0)) Reject('Variant payload transfer disagrees with its ownership.');
                if (Owned) {
                    const Copied = Buffer.from(Bytes);
                    Copied[Transfers[0]] = 4;
                    const Broken = path.join(Work, Label + '-copied.wvb');
                    writeFileSync(Broken, Copied, { flag: 'wx' });
                    for (const [Tool, Diagnostic] of [
                        [Verifier, 'wvb status=Invalid phase=typed-execution\n'],
                        [Runner, 'wvb run status=Unsupported profile=portable-main-i32 phase=envelope\n'],
                    ]) {
                        const Rejected = await Runˉdevelopmentˉcommand(Tool, [Broken],
                            Developmentˉdeadline, false, MAXIMUM_DIAGNOSTIC_BYTES);
                        if (Rejected.Code !== 1 || Rejected.Output !== '' || Normalize(Rejected.Error) !== Diagnostic) {
                            Reject(`Owned payload copy did not reject before execution: ${Rejected.Output}${Rejected.Error}`);
                        }
                    }
                }
            }
        }
        Completed += 1;
        process.stdout.write(`PASS owned payload item=${Completed}/${Cases.length} case=${Label} wvb-bytes=${First.length} wvb-sha256=${Digest(First)}\n`);
    }
    // The loop-indexing fix must not admit a second use of a consumed budget,
    // or a branch that consumes the parent before the loop backedge.
    const Invalidˉbudgets = [
        ['duplicate-budget-move', Replace(Reclaimˉsource(Source),
            'var Parent: Memory.Memoryˉbudget = Budget;',
            'var Parent: Memory.Memoryˉbudget = Budget;\n    var Again: Memory.Memoryˉbudget = Budget;')],
        ['consumed-loop-parent', Replace(Reclaimˉsource(Source),
            '(Child, 4u64)', '(Parent, 4u64)')],
    ];
    for (const [Label, Text] of Invalidˉbudgets) {
        const Input = path.join(Work, Label + '.wv');
        const Output = path.join(Work, Label + '.wvb');
        writeFileSync(Input, Text, { flag: 'wx' });
        const Rejected = await Runˉdevelopmentˉcommand(process.execPath, [
            path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'),
            Admitter, Authenticator, Analyzer, Emitter,
            '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
            '--source-profile', Sourceˉprofile, '--target-descriptor', Target,
            Input, ...['Collections/Collections.wv', 'Memory/Memory.wv',
                'Values/Option.wv', 'Values/Result.wv'].map(Name =>
                path.join(Repositoryˉroot, 'Libraries/Foundation', Name)), Output,
        ], Math.min(Developmentˉdeadline, Date.now() + 30_000),
        false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Rejected.Code !== 1 || existsSync(Output) || Normalize(Rejected.Error) !==
            'source emission status=Invalidˉanalysis analysis-status=Invalidˉwir wvb-status=Sourceˉwir function=0 operation=0 source-line=0\n') {
            Reject(`Invalid budget ownership did not reject at WIR validation: ${Label}\n${Rejected.Output}${Rejected.Error}`);
        }
        process.stdout.write(`PASS owned budget rejection case=${Label}\n`);
    }
    const Traceˉfixture = path.join(Repositoryˉroot,
        'Tests/Fixtures/WebAssembly/Wvb-Record-Vector-Trace-Probe.wv');
    Requireˉordinaryˉfile(Traceˉfixture, 8192, 'record Vector trace fixture');
    const Traceˉboundaries = path.join(Repositoryˉroot,
        'Tests/Fixtures/WebAssembly/Wvb-Record-Vector-Trace-Boundaries.wv');
    Requireˉordinaryˉfile(Traceˉboundaries, 16384, 'record Vector trace boundaries');
    const Traceˉoutput = path.join(Work, 'record-vector-trace.wvb');
    await Runˉnode('record-vector-trace-compile', 'Run-Split-Compiler.mjs', [
        Admitter, Authenticator, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile, '--target-descriptor', Target,
        Traceˉfixture,
        Traceˉboundaries,
        ...[
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Collection-Core.wv',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Value-Core.wv',
            'Runtime/Windvale/Foundation-Borrow-Frames-Core.wv',
            'Runtime/Windvale/Foundation-Borrow-View-Core.wv',
        ].map(Name => path.join(Repositoryˉroot, Name)),
        Traceˉoutput,
    ]);
    const Traceˉverified = await Run('record-vector-trace-verify',
        Verifier, [Traceˉoutput]);
    if (Normalize(Traceˉverified) !== 'wvb status=Valid profile=compiler-aligned\n') {
        Reject('Record Vector trace verifier output differs.');
    }
    const Traceˉexecuted = await Run('record-vector-trace-execute',
        Runner, [Traceˉoutput]);
    if (Normalize(Traceˉexecuted) !== 'Result: 42\n') {
        Reject('Record Vector trace result differs.');
    }
    process.stdout.write('PASS record Vector trace graph=record-vector-record stale=rejected wrong-type=rejected boundaries=passed\n');
    process.stdout.write(`native Foundation owned payloads publication=Passed cases=${Completed} trace-cases=1 budget-rejections=${Invalidˉbudgets.length} owned-execution=Passed qualification=false elapsed-ms=${Date.now() - Started}\n`);
}

async function Verifyˉfoundationˉsourceˉownership(Admitter, Analyzer, Emitter) {
    const Fixture = path.join(Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0',
        'Foundation-Value-Owned-Borrow-Control.wv');
    Requireˉordinaryˉfile(Fixture, 4096, 'Foundation owned borrow control');
    const Source = readFileSync(Fixture, 'utf8');
    const Present = 'case Option.Option.Present { Value: Item } { return Fallback; }';
    if (Source.split(Present).length !== 2) Reject('The owned borrow control mutation is ambiguous.');
    const Escaping = path.join(Work, 'Owned-Borrow-Escape.wv');
    const Returningˉpayload = Source.replace(Present,
        'case Option.Option.Present { Value: Item } { return Item; }');
    writeFileSync(Escaping, Returningˉpayload, { flag: 'wx' });
    const Copyˉreturn = path.join(Work, 'Copy-Borrow-Return.wv');
    if (Returningˉpayload.split('Collections.Vector<u32>').length !== 4) {
        Reject('The Copy borrow return type substitution is ambiguous.');
    }
    writeFileSync(Copyˉreturn,
        Returningˉpayload.replaceAll('Collections.Vector<u32>', 'u32'),
        { flag: 'wx' });
    for (const [Label, Input, Valid] of [
        ['owned-borrow-control', Fixture, true],
        ['owned-borrow-escape', Escaping, false],
        ['copy-borrow-return', Copyˉreturn, true],
    ]) {
        const Output = path.join(Work, `${Label}.wvb`);
        const Arguments = [path.join(Scriptˉdirectory, 'Run-Split-Compiler.mjs'),
            Admitter, Validator, Analyzer, Emitter,
            '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
            '--source-profile', Sourceˉprofile, '--target-descriptor', Targetˉdescriptor,
            Input,
            path.join(Repositoryˉroot, 'Libraries/Foundation/Collections/Collections.wv'),
            path.join(Repositoryˉroot, 'Libraries/Foundation/Memory/Memory.wv'),
            path.join(Repositoryˉroot, 'Libraries/Foundation/Values/Option.wv'), Output];
        process.stdout.write(`START Foundation source ownership case=${Label}\n`);
        const Result = await Runˉdevelopmentˉcommand(process.execPath, Arguments,
            Math.min(Developmentˉdeadline, Date.now() + 60_000),
            false, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Valid) {
            if (Result.Code !== 0 || Result.Error !== '' || !existsSync(Output) ||
                !Result.Output.includes('source emission status=Published')) {
                Reject(`The owned borrow control failed.\n${Result.Output}\n${Result.Error}`);
            }
            Requireˉordinaryˉfile(Output, 4096, 'owned borrow control WVB');
            const Bytes = readFileSync(Output);
            const Sections = Parseˉsections(Bytes);
            const Functions = Parseˉfunctionˉentries(Bytes, Sections[4]);
            if (Functions.length !== 1 || Functions[0].name !== 'Main' ||
                Functions[0].codeLength === 0) Reject('The owned control publication shape differs.');
            process.stdout.write(`Foundation source ownership control wvb-bytes=${Bytes.length} wvb-sha256=${Digest(Bytes)}\n`);
        } else if (Result.Code !== 1 || existsSync(Output) ||
            Normalize(Result.Error) !== 'source emission status=Invalidˉanalysis analysis-status=Invalidˉwir wvb-status=Sourceˉwir function=0 operation=0 source-line=0\n') {
            Reject(`The owned borrow escape failed to reject at emitter WIR validation.\n${Result.Output}\n${Result.Error}`);
        }
        process.stdout.write(`PASS Foundation source ownership case=${Label}\n`);
    }
    process.stdout.write('native Foundation source ownership status=Passed cases=3 qualification=false\n');
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

async function Verifyˉfoundationˉnativeˉrejections(Lowerer) {
    for (const [Label, Bytes, Valid] of Foundationˉruntimeˉcases(Requireˉfoundationˉcandidate())) {
        if (Valid) continue;
        const File = path.join(Work, `Borrow-Native-Rejection-${Label}.wvb`);
        const Object = path.join(Work, `Borrow-Native-Rejection-${Label}.wvo`);
        writeFileSync(File, Bytes, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(Lowerer, [File, Object],
            Math.min(Developmentˉdeadline, Date.now() + 60_000),
            true, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Result.Code !== 1 || Result.Output !== '' ||
            !Normalize(Result.Error).startsWith('native x64 status=Invalidˉwvb ') ||
            lstatSync(Object, { throwIfNoEntry: false }) !== undefined) {
            Reject(`Foundation native rejection ${Label} failed: status=${Result.Code}\n${Result.Output}\n${Result.Error}`);
        }
        process.stdout.write(`PASS Foundation native rejection case=${Label}\n`);
    }
}

async function Verifyˉfoundationˉstaging(Producer, Textˉcandidate, Admitter = null) {
    Requireˉordinaryˉfile(Producer, 67_108_864, 'native staging producer');
    if (Admitter !== null) Requireˉordinaryˉfile(Admitter, 67_108_864, 'native staging admission checker');
    Requireˉordinaryˉfile(Textˉcandidate, 4096, 'Foundation text fixture');
    const Textˉbytes = readFileSync(Textˉcandidate);
    if (Digest(Textˉbytes) !== 'f6dcb37f75eaca281322961cc5498d2de88fc97f2213bd9fa93963dc1c569018') {
        Reject('The Foundation text fixture identity differs.');
    }
    const Cases = Foundationˉruntimeˉcases(Requireˉfoundationˉcandidate());
    Cases.push(['text', Textˉbytes, true]);
    for (const [Label, Bytes, Valid] of Cases) {
        const Input = path.join(Work, `Stage-${Label}.wvb`);
        const Prefix = path.join(Work, `Stage-${Label}`);
        const Manifest = Prefix + '.wvop';
        writeFileSync(Input, Bytes, { flag: 'wx' });
        if (Valid) {
            await Run(`foundation-stage-${Label}`, Producer, [Input, Prefix, Manifest]);
            Requireˉordinaryˉfile(Manifest, 1024, 'Foundation staging manifest');
            const Directory = readFileSync(Manifest);
            if (Directory.length < 24 || Directory.toString('ascii', 0, 4) !== 'WVOP' ||
                Directory.readUInt32LE(4) !== 1 || Directory.readUInt32LE(8) !== Directory.length) {
                Reject('Foundation staging manifest envelope differs.');
            }
            const Count = Directory.readUInt32LE(16);
            if (Count < 1 || Count > 16 || Directory.length !== 24 + Count * 12 ||
                Directory.readUInt32LE(20) !== 4_194_304 || Directory.readUInt32LE(12) > 65_536) {
                Reject('Foundation staging manifest limits differ.');
            }
            const Chunks = [];
            let Position = 0;
            for (let Index = 0; Index < Count; Index += 1) {
                const Entry = 24 + Index * 12;
                const Length = Directory.readUInt32LE(Entry + 8);
                if (Directory.readUInt32LE(Entry) !== Index ||
                    Directory.readUInt32LE(Entry + 4) !== Position || Length < 1 || Length > 65_536 - Position) {
                    Reject('Foundation staging chunk range differs.');
                }
                const File = Prefix + `.chunk-${Index}`;
                Requireˉordinaryˉfile(File, Length, 'Foundation object chunk');
                const Chunk = readFileSync(File);
                if (Chunk.length !== Length) Reject('Foundation staging chunk extent differs.');
                Chunks.push(Chunk);
                Position += Length;
            }
            const Expected = Label === 'text' ?
                '24ff3002ce08dc89a85a8171872e170d4b7fc3fbcb9c46f8a3f7d6da76bc3883' :
                'c88237b300da23b8ea37ed87e5acb014d3ce64715bc0ffe4fc75e53bb66b33f1';
            if (Position !== Directory.readUInt32LE(12) || Digest(Buffer.concat(Chunks)) !== Expected) {
                Reject('Foundation staged object differs from direct lowering.');
            }
            if (Admitter !== null) await Verifyˉfoundationˉstagingˉadmission(
                Admitter, Input, Prefix, Manifest, Chunks[0], Directory);
        } else {
            const Result = await Runˉdevelopmentˉcommand(Producer, [Input, Prefix, Manifest],
                Math.min(Developmentˉdeadline, Date.now() + 10_000),
                true, MAXIMUM_DIAGNOSTIC_BYTES);
            if (Result.Code !== 1 || Result.Output !== '' ||
                !Normalize(Result.Error).startsWith('native x64 staging status=Invalidˉwvb ') ||
                lstatSync(Manifest, { throwIfNoEntry: false }) !== undefined ||
                lstatSync(Prefix + '.chunk-0', { throwIfNoEntry: false }) !== undefined) {
                Reject(`Foundation staging rejection ${Label} failed: ${Result.Error}`);
            }
        }
        process.stdout.write(`PASS Foundation staging case=${Label}\n`);
    }
    process.stdout.write(`native Foundation staging status=Passed cases=${Cases.length + (Admitter === null ? 0 : 12)} qualification=false\n`);
}

async function Verifyˉfoundationˉstagingˉadmission(Admitter, Input, Prefix, Manifest, Chunk, Directory) {
    const Destination = Prefix + '.wvo';
    const Sentinel = Buffer.from('admission must not publish', 'utf8');
    const Original = readFileSync(Input);
    writeFileSync(Destination, Sentinel, { flag: 'wx' });
    for (const Label of ['valid', 'content', 'chunk-length', 'manifest', 'source-version', 'alias']) {
        writeFileSync(Prefix + '.chunk-0', Chunk);
        writeFileSync(Manifest, Directory);
        writeFileSync(Input, Original);
        if (Label === 'content') {
            const Changed = Buffer.from(Chunk);
            Changed[0] ^= 1;
            writeFileSync(Prefix + '.chunk-0', Changed);
        } else if (Label === 'chunk-length') {
            writeFileSync(Prefix + '.chunk-0', Chunk.subarray(0, -1));
        } else if (Label === 'manifest') {
            const Changed = Buffer.from(Directory);
            Changed[6] = 1;
            writeFileSync(Manifest, Changed);
        } else if (Label === 'source-version') {
            const Changed = Buffer.from(Original);
            Changed[6] = 40;
            writeFileSync(Input, Changed);
        }
        const Result = await Runˉdevelopmentˉcommand(Admitter,
            [Input, Prefix, Manifest, Label === 'alias' ? Input : Destination],
            Math.min(Developmentˉdeadline, Date.now() + 10_000),
            true, MAXIMUM_DIAGNOSTIC_BYTES);
        const Expected = {
            content: 'content=Content', 'chunk-length': 'content=Chunkˉlength',
            manifest: 'status=Invalidˉmanifest', 'source-version': 'content=Invalidˉplan',
            alias: 'status=Duplicateˉresource',
        }[Label];
        if (Result.Code !== (Label === 'valid' ? 0 : 1) || Result.Output !== '' ||
            Normalize(Result.Error) !== (Label === 'valid' ? '' : `native x64 staging admission ${Expected}\n`) ||
            !readFileSync(Destination).equals(Sentinel)) {
            Reject(`Foundation staging admission ${Label} failed: ${Result.Code} ${Result.Error}`);
        }
        process.stdout.write(`PASS Foundation staging admission case=${Label}\n`);
    }
    writeFileSync(Prefix + '.chunk-0', Chunk);
    writeFileSync(Manifest, Directory);
    writeFileSync(Input, Original);
}

async function Verifyˉfoundationˉenumˉmetadata(Candidate) {
    Requireˉordinaryˉfile(Candidate, 4096, 'Foundation allocated-text fixture');
    const Bytes = readFileSync(Candidate);
    // Retained borrow-only publication and the current shared-read publication
    // have identical enum metadata; retain both exact diagnostic inputs.
    if (![
        'f6dcb37f75eaca281322961cc5498d2de88fc97f2213bd9fa93963dc1c569018',
        '1b1293e962655d904b61b9d8b0348ddc9da13886764387c0512b6f64daac0c4e',
    ].includes(Digest(Bytes))) {
        Reject('The Foundation allocated-text fixture identity differs.');
    }
    const Sections = Parseˉsections(Bytes);
    const Reader = path.join(Repositoryˉroot, 'Artifacts',
        'Native-Hosted-Enum-Request-Candidate',
        process.platform === 'win32' ? 'windows-x64/wvhostenumrequest.exe' : 'linux-x64/wvhostenumrequest.elf');
    Requireˉordinaryˉfile(Reader, 4_194_304, 'hosted enum metadata reader');
    const Cases = [];
    // These are metadata-envelope tests, not executable-version qualification.
    for (const Version of [11, 16, 30, 31, 39]) {
        const Value = Buffer.from(Bytes);
        Value.writeUInt16LE(Version, 6);
        Cases.push([`version-${Version}`, Value, true]);
    }
    for (const [Label, Offset, Value] of [
        ['unknown-version', 6, 40], ['magic', 0, 0], ['major', 4, 2],
        ['section-count', 8, 6], ['section-reserved', 13, 1],
        ['type-kind', Sections[7].payload + 4, 255],
    ]) {
        const Mutated = Buffer.from(Bytes);
        Mutated[Offset] = Value;
        Cases.push([Label, Mutated, false]);
    }
    const Oversized = Buffer.from(Bytes);
    Oversized.writeUInt32LE(0xffffffff, Sections[7].header + 4);
    Cases.push(['oversized-section', Oversized, false]);
    const Typeˉlimit = Buffer.from(Bytes);
    Typeˉlimit.writeUInt32LE(257, Sections[7].payload);
    Cases.push(['type-limit', Typeˉlimit, false]);
    Cases.push(['short-header', Bytes.subarray(0, 11), false]);
    Cases.push(['truncated', Bytes.subarray(0, -1), false]);
    Cases.push(['trailing', Buffer.concat([Bytes, Buffer.from([0])]), false]);
    for (const [Label, Input, Valid] of Cases) {
        const File = path.join(Work, `Enum-${Label}.wvb`);
        const Output = path.join(Work, `Enum-${Label}.wveq`);
        const Sentinel = Buffer.from('preserve rejected destination', 'utf8');
        writeFileSync(File, Input, { flag: 'wx' });
        writeFileSync(Output, Sentinel, { flag: 'wx' });
        const Result = await Runˉdevelopmentˉcommand(Reader, [File, Output],
            Math.min(Developmentˉdeadline, Date.now() + 10_000),
            true, MAXIMUM_DIAGNOSTIC_BYTES);
        if (Valid) {
            if (Result.Code !== 0 || Result.Error !== '' ||
                Normalize(Result.Output) !== 'hosted enum request status=Valid bytes=64\n' ||
                Digest(readFileSync(Output)) !== '7af5d827197fd6674bd88655c21dbc98d1886e9c7a30641b8df5460016ef1564') {
                Reject(`Foundation enum metadata ${Label} failed.`);
            }
        } else if (Result.Code !== 2 || Result.Output !== '' ||
            Normalize(Result.Error) !== 'hosted enum request status=Rejected\n' ||
            !readFileSync(Output).equals(Sentinel)) {
            Reject(`Foundation enum metadata rejection ${Label} failed.`);
        }
        process.stdout.write(`PASS Foundation enum metadata case=${Label}\n`);
    }
    process.stdout.write(`native Foundation enum metadata status=Passed cases=${Cases.length} qualification=false\n`);
}

async function Verifyˉfoundationˉnative(Lowerer, Candidate, Label) {
    Requireˉordinaryˉfile(Lowerer, 67_108_864, 'current native lowerer');
    const Prefix = path.join(Work, `Foundation-Native-${Label}`);
    const Object = Prefix + '.wvo';
    const Image = Prefix + '.bin';
    const Application = Prefix + (process.platform === 'win32' ? '.exe' : '.elf');
    await Run(`foundation-native-${Label}-lower`, Lowerer, [Candidate, Object]);
    await Runˉnative(`foundation-native-${Label}-check`, 'Check-Wvo', [Object]);
    const Linked = await Runˉnative(`foundation-native-${Label}-link`, 'Link-Wvo', ['0', 'Main', Image, Object]);
    const Entry = /^entry name=Main address=([0-9]+)$/mu.exec(Linked);
    if (Entry === null) Reject('Foundation native link omitted Main.');
    if (Label === 'text') {
        await Verifyˉfoundationˉenumˉmetadata(Candidate);
        // Text concatenation requires the ABI service table, not only an arena.
        // Preserve the candidate lowerer's linked image through hosted packaging.
        const Sources = Prefix + '-Sources';
        copyFileSync(Image, Sources + '.chunk-0');
        await Runˉnative(`foundation-native-${Label}-package`, 'Package-Hosted-Wvb', [
            'image', '1', Candidate, Sources, '1', Entry[1], Application,
            process.platform === 'win32' ? 'windows' : 'linux',
        ]);
    } else {
        await Runˉnative(`foundation-native-${Label}-package`, 'Package-Console', [
            process.platform === 'win32' ? 'windows-x64-console-v1' : 'linux-x64-console-v1',
            Image, Entry[1], Application,
        ]);
    }
    const Output = await Run(`foundation-native-${Label}-execute`, Application, [], 42);
    if (Output !== '') Reject('Foundation native execution emitted unexpected output.');
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

async function Run(Label, Command, Arguments, Expected = 0, Deadline = null) {
    const Defaultˉdeadline = Developmentˉonly ? Developmentˉdeadline :
        Date.now() + TOOL_TIMEOUT_MILLISECONDS;
    if (Deadline !== null && !Number.isSafeInteger(Deadline)) Reject('The child deadline is invalid.');
    const Selectedˉdeadline = Deadline === null ? Defaultˉdeadline : Math.min(Deadline, Defaultˉdeadline);
    const Remaining = Selectedˉdeadline - Date.now();
    if (Remaining <= 0) Reject('The focused Foundation borrow development budget expired.', 124);
    Step += 1;
    const Stepˉnumber = Step;
    const Start = Date.now();
    process.stdout.write(
        `START language 1 memory budget split execution step=${Stepˉnumber} phase=${Label}\n`,
    );
    const Result = await Runˉdevelopmentˉcommand(
        Command, Arguments, Selectedˉdeadline, Developmentˉonly, MAXIMUM_DIAGNOSTIC_BYTES,
    );
    if (Result.Code !== Expected || Result.Error.length !== 0) {
        const Component = Label === 'foundation-borrow-components-execute'
            ? { 1: 'plan', 2: 'directories', 3: 'owners' }[Result.Code] : undefined;
        Reject(
            `${Label} failed: status=${Result.Code}\n` +
            (Component === undefined ? '' :
                `Failed component=${Component}; --foundation-borrow-${Component} retains its original group diagnostic.\n`) +
            `stdout=${Result.Output}\nstderr=${Result.Error}`,
            Result.Code === 124 ? 124 : Result.Code === null ? 2 :
                Label.startsWith('foundation-products-') && [2, 64].includes(Result.Code) ? Result.Code : 1,
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
    return Entries;
}

function Wvbˉinstructionˉwidthˉat(Bytes, Cursor) {
    const Opcode = Bytes[Cursor];
    if (Opcode === 226 || Opcode === 227) return 9;
    if (Opcode === 225) return 13;
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
                metadataOffset: Cursor - 12,
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
        } else if (Kind === 5 || Kind === 6) {
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
    if (Offset >= Bytes.length) Reject('Truncated WVB shape.');
    const Shape = Bytes[Offset];
    if (Shape === 37) {
        if (Offset + 1 >= Bytes.length || Bytes[Offset + 1] === 37) {
            Reject('Invalid borrowed payload wrapper.');
        }
        const Inner = Readˉshape(Bytes, Offset + 1);
        return { shape: Shape, shapeOffset: Offset, typeIndex: null, inner: Inner, end: Inner.end };
    }
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

function Reject(Message, Exitˉcode = 1) {
    throw Object.assign(new Error(Message), { exitCode: Exitˉcode });
}
