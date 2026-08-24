import { spawnSync } from 'node:child_process';
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
const BOOTSTRAP_ANALYZER_SHA256 =
    '26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120';
const BOOTSTRAP_EMITTER_SHA256 =
    'ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94';
const EXPECTED_SUCCESS_SHA256 =
    '5678409a9b9bba47dd37a6f3d26f0666a7c27d2e86d6ff320a78b8fdcbec8f53';
const EXPECTED_VECTOR_SUCCESS_SHA256 =
    '881bcbabc9620188964a63601490ad81acf63587f70501443d97447cdd45f7c5';
const EXPECTED_APPEND_SUCCESS_SHA256 =
    '6478cc8b302e91caa54ff3aea835ef3ea1c1722161cd4f12aa587aa432b6918f';
const EXPECTED_OWNED_CALL_SUCCESS_SHA256 =
    'ab79d05bb03afddbe6430adc127c8cdf084ea6499b16e3e25ebb3e477c408387';

if (process.argv.length !== 2) {
    process.stderr.write(
        'Usage: node Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs\n',
    );
    process.exit(64);
}
if (process.platform !== 'win32' && process.platform !== 'linux') {
    Reject(`Unsupported test host: ${process.platform}.`);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Profileˉroot = path.join(
    Repositoryˉroot,
    'Documents', 'Project', 'Language-1.0-Localization-Workloads',
    '01-Source-Profile-Admission', 'Reference-Artifacts',
);
const Sourceˉlock = path.join(Profileˉroot, 'Source-Inputs.wvlock');
const Sourceˉprofile = path.join(Profileˉroot, 'En-Source-Profile.wvsp');
const Bootstrapˉanalyzerˉwvb = path.join(
    Repositoryˉroot, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb', 'wvanalyze.wvb',
);
const Bootstrapˉemitterˉwvb = path.join(
    Repositoryˉroot, 'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb', 'wvemit.wvb',
);
const Work = mkdtempSync(path.join(
    os.tmpdir(), 'windvale-memory-budget-split-execution-',
));
let Step = 0;

try {
    Requireˉordinaryˉfile(Sourceˉlock, 4_194_304, 'source lock');
    Requireˉordinaryˉfile(Sourceˉprofile, 4_194_304, 'source profile');
    Requireˉexactˉfile(
        Bootstrapˉanalyzerˉwvb, 992_412, BOOTSTRAP_ANALYZER_SHA256,
        'bootstrap analyzer',
    );
    Requireˉexactˉfile(
        Bootstrapˉemitterˉwvb, 895_787, BOOTSTRAP_EMITTER_SHA256,
        'bootstrap emitter',
    );

    const Executableˉsuffix = process.platform === 'win32' ? '.exe' : '.elf';
    const Target = process.platform === 'win32' ? 'windows' : 'linux';
    const Bootstrapˉanalyzer = path.join(Work, `Bootstrap-Analyzer${Executableˉsuffix}`);
    const Bootstrapˉemitter = path.join(Work, `Bootstrap-Emitter${Executableˉsuffix}`);
    const Bootstrapˉanalyzerˉidentity = path.join(Work, 'Bootstrap-Analyzer.identity');
    const Bootstrapˉemitterˉidentity = path.join(Work, 'Bootstrap-Emitter.identity');
    const Admitterˉwvb = path.join(Work, 'Admitter.wvb');
    const Analyzerˉwvb = path.join(Work, 'Analyzer.wvb');
    const Emitterˉwvb = path.join(Work, 'Emitter.wvb');
    const Admitter = path.join(Work, `Admitter${Executableˉsuffix}`);
    const Analyzer = path.join(Work, `Analyzer${Executableˉsuffix}`);
    const Emitter = path.join(Work, `Emitter${Executableˉsuffix}`);
    const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
    const Emitterˉidentity = path.join(Work, 'Emitter.identity');

    Runˉnative('bootstrap-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉanalyzerˉwvb, Bootstrapˉanalyzer, '--development-cache',
    ]);
    Runˉnative('bootstrap-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉemitterˉwvb, Bootstrapˉemitter, '--development-cache',
    ]);
    Runˉnode('bootstrap-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
    ]);
    Runˉnode('bootstrap-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnode('current-admitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Admission-Driver.wvproj'), Admitterˉwvb,
        Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
        Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnode('current-analyzer-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Analysis-Driver.wvproj'), Analyzerˉwvb,
        Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
        Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnative('current-admitter-package', 'Package-Segmented-Compiler-Wvb', [
        '2', Admitterˉwvb, Admitter, '--development-cache',
    ]);
    Runˉnative('current-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Analyzerˉwvb, Analyzer, '--development-cache',
    ]);
    Runˉnode('current-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Analyzer, Analyzerˉidentity,
    ]);
    Runˉnode('current-emitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Compiler-Emission-Driver.wvproj'), Emitterˉwvb,
        Analyzer, Analyzerˉidentity,
        Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    Runˉnative('current-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Emitterˉwvb, Emitter, '--development-cache',
    ]);
    Runˉnode('current-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Emitter, Emitterˉidentity,
    ]);
    Runˉnode(
        'owned-vector-calls-and-joins-wir',
        'Verify-Language-1.0-Owned-Vector-Calls-Wir.mjs',
        [Admitter, Analyzer, Emitter, Work],
    );

    const Successˉa = path.join(Work, 'Success-A.wvb');
    const Successˉb = path.join(Work, 'Success-B.wvb');
    const Failure = path.join(Work, 'Failure.wvb');
    const Vectorˉsuccessˉa = path.join(Work, 'Vector-Success-A.wvb');
    const Vectorˉsuccessˉb = path.join(Work, 'Vector-Success-B.wvb');
    const Vectorˉfailure = path.join(Work, 'Vector-Failure.wvb');
    const Vectorˉzero = path.join(Work, 'Vector-Zero.wvb');
    const Appendˉsuccessˉa = path.join(Work, 'Append-Success-A.wvb');
    const Appendˉsuccessˉb = path.join(Work, 'Append-Success-B.wvb');
    const Ownedˉcallˉsuccessˉa = path.join(Work, 'Owned-Call-Success-A.wvb');
    const Ownedˉcallˉsuccessˉb = path.join(Work, 'Owned-Call-Success-B.wvb');
    Compile('success-a-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Executable.wv', Successˉa);
    Compile('success-b-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Executable.wv', Successˉb);
    Compile('failure-compile', Admitter, Analyzer, Emitter,
        'Memory-Budget-Split-Failure-Executable.wv', Failure);
    Compileˉvector('vector-success-a-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Executable.wv', Vectorˉsuccessˉa);
    Compileˉvector('vector-success-b-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Executable.wv', Vectorˉsuccessˉb);
    Compileˉvector('vector-failure-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Failure-Executable.wv', Vectorˉfailure);
    Compileˉvector('vector-zero-compile', Admitter, Analyzer, Emitter,
        'Vector-Construct-Reserved-Zero-Executable.wv', Vectorˉzero);
    Compileˉvector('vector-append-success-a-compile', Admitter, Analyzer, Emitter,
        'Vector-Append-Executable.wv', Appendˉsuccessˉa);
    Compileˉvector('vector-append-success-b-compile', Admitter, Analyzer, Emitter,
        'Vector-Append-Executable.wv', Appendˉsuccessˉb);
    Compileˉvector('owned-call-success-a-compile', Admitter, Analyzer, Emitter,
        'Owned-Vector-Calls-And-Joins-Wir.wv', Ownedˉcallˉsuccessˉa);
    Compileˉvector('owned-call-success-b-compile', Admitter, Analyzer, Emitter,
        'Owned-Vector-Calls-And-Joins-Wir.wv', Ownedˉcallˉsuccessˉb);
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

    const Verifierˉwvb = path.join(Work, 'Verifier.wvb');
    const Verifier = path.join(Work, `Verifier${Executableˉsuffix}`);
    Runˉnative('verifier-build', 'Build-Wvb', [
        Project('Windvale-Compiler-Wvb-Verifier.wvproj'), Verifierˉwvb,
    ]);
    Runˉnative('verifier-package', 'Package-Hosted-Wvb', [
        '2', Verifierˉwvb, Verifier, Target,
    ]);
    Requireˉvalid(Verifier, Successˉa, 'successful Split module');
    Requireˉvalid(Verifier, Failure, 'refused Split module');
    Requireˉvalid(Verifier, Vectorˉsuccessˉa, 'successful Vector module');
    Requireˉvalid(Verifier, Vectorˉfailure, 'refused Vector module');
    Requireˉvalid(Verifier, Vectorˉzero, 'zero-maximum Vector module');
    Requireˉvalid(Verifier, Appendˉsuccessˉa, 'Vector append module');
    Requireˉvalid(Verifier, Ownedˉcallˉsuccessˉa, 'owned Vector call module');

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

    const Runnerˉwvb = path.join(Work, 'Runner.wvb');
    const Runner = path.join(Work, `Runner${Executableˉsuffix}`);
    Runˉnode('runner-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project('Windvale-Wvb-Runner.wvproj'), Runnerˉwvb,
        Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity,
    ]);
    Runˉnative('runner-package', 'Package-Hosted-Wvb', [
        '5', Runnerˉwvb, Runner, Target,
    ]);
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
    Requireˉresultˉ42(
        Runner, Ownedˉcallˉsuccessˉa, 'owned Vector call execution',
    );

    process.stdout.write(
        'native language 1 memory budget and vector execution status=Passed ' +
        `cases=58 valid=7 malformed=${
            Malformedˉcases.length + Vectorˉmalformedˉcases.length +
            Appendˉmalformedˉcases.length + Ownedˉcallˉmalformedˉcases.length
        } owned-call-cases=4 ` +
        `result=42 split-wvb-bytes=${Successˉbytes.length} ` +
        `split-sha256=${Successˉsha256} ` +
        `vector-wvb-bytes=${Vectorˉsuccessˉbytes.length} ` +
        `vector-sha256=${Vectorˉsha256} ` +
        `append-wvb-bytes=${Appendˉsuccessˉbytes.length} ` +
        `append-sha256=${Appendˉsha256} ` +
        `owned-call-wvb-bytes=${Ownedˉcallˉsuccessˉbytes.length} ` +
        `owned-call-sha256=${Ownedˉcallˉsha256}\n`,
    );
} finally {
    const Resolved = path.resolve(Work);
    const Temporaryˉroot = path.resolve(os.tmpdir());
    if (path.dirname(Resolved) !== Temporaryˉroot ||
        !path.basename(Resolved).startsWith('windvale-memory-budget-split-execution-')) {
        Reject(`Refusing to remove unexpected test directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

function Project(Name) {
    return path.join(Repositoryˉroot, 'Projects', 'Tools', Name);
}

function Compile(Label, Admitter, Analyzer, Emitter, Fixture, Output) {
    Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
        path.join(Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0', Fixture),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Memory', 'Memory.wv'),
        path.join(Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv'),
        Output,
    ]);
}

function Compileˉvector(Label, Admitter, Analyzer, Emitter, Fixture, Output) {
    Runˉnode(Label, 'Run-Split-Compiler.mjs', [
        Admitter, Analyzer, Emitter,
        '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
        '--source-profile', Sourceˉprofile,
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

function Runˉnative(Label, Name, Arguments) {
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, 4_194_304, `${Name} script`);
    if (process.platform === 'win32') {
        Run(Label, process.env.ComSpec ?? 'cmd.exe', [
            '/d', '/c', 'call', Script, ...Arguments,
        ]);
    } else {
        Run(Label, 'bash', [Script, ...Arguments]);
    }
}

function Runˉnode(Label, Name, Arguments) {
    Run(Label, process.execPath, [path.join(Scriptˉdirectory, Name), ...Arguments]);
}

function Run(Label, Command, Arguments) {
    Step += 1;
    process.stdout.write(
        `START language 1 memory budget split execution step=${Step} phase=${Label}\n`,
    );
    const Result = spawnSync(Command, Arguments, {
        encoding: 'utf8',
        windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: TOOL_TIMEOUT_MILLISECONDS,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Result.stderr.length !== 0) {
        Reject(
            `${Label} failed: status=${Result.status} error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout}\nstderr=${Result.stderr}`,
        );
    }
    process.stdout.write(
        `PASS  language 1 memory budget split execution step=${Step} phase=${Label}\n`,
    );
    return Result.stdout;
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
    if (Result.error !== undefined || Result.status === 0 ||
        Result.stdout.includes('wvb status=Valid')) {
        Reject(`The verifier accepted malformed case ${Label}.`);
    }
}

function Requireˉresultˉ42(Runner, Candidate, Label) {
    const Result = spawnSync(Runner, [Candidate], {
        encoding: 'utf8', windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== 'Result: 42\n' || Result.stderr.length !== 0) {
        Reject(`The ${Label} differed: status=${Result.status}.`);
    }
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
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Parsed = Readˉshape(Bytes, Cursor);
            Parameters.push(Parsed.shape);
            Parameterˉtypes.push(Parsed.typeIndex);
            Cursor = Parsed.end;
        }
        const Return = Readˉshape(Bytes, Cursor);
        Cursor = Return.end;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Locals = [];
        const Localˉtypes = [];
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Parsed = Readˉshape(Bytes, Cursor);
            Locals.push(Parsed.shape);
            Localˉtypes.push(Parsed.typeIndex);
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
                returnShape: Return.shape,
                localShapes: Locals,
                localTypeIndices: Localˉtypes,
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
                Cursor = Memberˉname.end + 1;
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
    const Nominal = [7, 8, 11, 22, 23, 24, 26, 27].includes(Shape);
    return {
        shape: Shape,
        shapeOffset: Offset,
        typeIndex: Nominal ? Bytes.readUInt32LE(Offset + 1) : null,
        end: Offset + (Nominal ? 5 : 1),
    };
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
