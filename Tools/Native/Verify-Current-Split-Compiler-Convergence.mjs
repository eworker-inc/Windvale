import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import {
    lstatSync,
    mkdirSync,
    mkdtempSync,
    readFileSync,
    realpathSync,
    rmSync,
} from 'node:fs';
import os from 'node:os';
import path from 'node:path';

const MAXIMUM_DIAGNOSTIC_BYTES = 1_048_576;
const MAXIMUM_PRODUCT_BYTES = 16_777_216;
const PRODUCER_TIMEOUT_MILLISECONDS = 900_000;
const PHASES = 18;
const BOOTSTRAP_ANALYZER = {
    bytes: 992_412,
    sha256: '26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120',
};
const BOOTSTRAP_EMITTER = {
    bytes: 895_787,
    sha256: 'ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94',
};
const BRIDGE_EMITTER = {
    bytes: 1_146_083,
    sha256: '0d838b6d983320cf22b9094ef5a4692d6833f1834292863789577e034f6febdb',
};
const CURRENT_ANALYZER = {
    bytes: 1_515_281,
    sha256: 'a8687f5ec9337d95ea105b5b2d5feea453a11686251802c14110d1f171a3983a',
};
const CURRENT_EMITTER = {
    bytes: 1_523_514,
    sha256: '61ebad24f080a78059bfe3c2812cdb04978873eb6891d063ac2090876dc06403',
};
const CURRENT_VERIFIER = {
    bytes: 399_387,
    sha256: '7da624b070b69c3a720a00df12b753ed28276b7909c48ec5e6c349bd15ed9800',
};

if (process.argv.length !== 3) {
    Usage();
}
const Host = `${process.platform}-${process.arch}`;
if (Host !== 'win32-x64' && Host !== 'linux-x64') {
    Reject(`The split compiler convergence gate does not support ${Host}.`);
}

const Sourceˉroot = Canonicalˉordinaryˉdirectory(
    process.argv[2],
    'source root',
);
const Nativeˉroot = path.join(Sourceˉroot, 'Tools', 'Native');
const Bootstrapˉroot = path.join(
    Sourceˉroot,
    'Artifacts',
    'Language-1.0-Target-Aware-Emission-Bootstrap',
    'Wvb',
);
const Bootstrapˉanalyzerˉwvb = path.join(Bootstrapˉroot, 'wvanalyze.wvb');
const Bootstrapˉemitterˉwvb = path.join(Bootstrapˉroot, 'wvemit.wvb');
const Bridgeˉemitterˉwvb = path.join(
    Bootstrapˉroot,
    'wvemit-wvir-1.9-bridge.wvb',
);
Requireˉexactˉfile(
    Bootstrapˉanalyzerˉwvb,
    BOOTSTRAP_ANALYZER,
    'bootstrap analyzer WVB',
);
Requireˉexactˉfile(
    Bootstrapˉemitterˉwvb,
    BOOTSTRAP_EMITTER,
    'bootstrap emitter WVB',
);
Requireˉexactˉfile(
    Bridgeˉemitterˉwvb,
    BRIDGE_EMITTER,
    'WVIR 1.9 bridge emitter WVB',
);
const Temporaryˉroot = Canonicalˉordinaryˉdirectory(
    os.tmpdir(),
    'temporary root',
);
const Work = mkdtempSync(path.join(
    Temporaryˉroot,
    'windvale-current-split-convergence-',
));
const Cacheˉroot = path.join(Work, 'Cache');
const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
const Bootstrapˉanalyzer = path.join(Work, `Bootstrap-Analyzer${Suffix}`);
const Bootstrapˉemitter = path.join(Work, `Bootstrap-Emitter${Suffix}`);
const Bridgeˉemitter = path.join(Work, `Bridge-Emitter${Suffix}`);
const Bootstrapˉanalyzerˉidentity = path.join(
    Work,
    'Bootstrap-Analyzer.identity',
);
const Bootstrapˉemitterˉidentity = path.join(
    Work,
    'Bootstrap-Emitter.identity',
);
const Bridgeˉemitterˉidentity = path.join(Work, 'Bridge-Emitter.identity');
const Analyzerˉstage1ˉwvb = path.join(Work, 'Analyzer-Stage1.wvb');
const Analyzer = path.join(Work, `Analyzer${Suffix}`);
const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
const Emitterˉstage1ˉwvb = path.join(Work, 'Emitter-Stage1.wvb');
const Emitter = path.join(Work, `Emitter${Suffix}`);
const Emitterˉidentity = path.join(Work, 'Emitter.identity');
const Analyzerˉstage2ˉwvb = path.join(Work, 'Analyzer-Stage2.wvb');
const Emitterˉstage2ˉwvb = path.join(Work, 'Emitter-Stage2.wvb');
const Verifierˉwvb = path.join(Work, 'Verifier.wvb');
const Verifier = path.join(Work, `Verifier${Suffix}`);
let Phase = 0;

try {
    mkdirSync(Cacheˉroot);
    await Runˉnative('bootstrap-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉanalyzerˉwvb, Bootstrapˉanalyzer,
    ]);
    await Runˉnative('bootstrap-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉemitterˉwvb, Bootstrapˉemitter,
    ]);
    await Runˉnative('bridge-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bridgeˉemitterˉwvb, Bridgeˉemitter,
    ]);
    await Runˉnode('bootstrap-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
    ]);
    await Runˉnode('bootstrap-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    await Runˉnode('bridge-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Bridgeˉemitter, Bridgeˉemitterˉidentity,
    ]);
    await Runˉnode('current-analyzer-stage1', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Analysis-Driver.wvproj'),
        Analyzerˉstage1ˉwvb,
        Bootstrapˉanalyzer,
        Bootstrapˉanalyzerˉidentity,
        Bootstrapˉemitter,
        Bootstrapˉemitterˉidentity,
    ]);
    Requireˉordinaryˉfile(
        Analyzerˉstage1ˉwvb,
        MAXIMUM_PRODUCT_BYTES,
        'current analyzer Stage 1 WVB',
    );
    await Runˉnative('current-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Analyzerˉstage1ˉwvb, Analyzer,
    ]);
    await Runˉnode('current-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Analyzer, Analyzerˉidentity,
    ]);
    await Runˉnode('current-emitter-stage1', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Emission-Driver.wvproj'),
        Emitterˉstage1ˉwvb,
        Analyzer,
        Analyzerˉidentity,
        Bridgeˉemitter,
        Bridgeˉemitterˉidentity,
    ]);
    Requireˉordinaryˉfile(
        Emitterˉstage1ˉwvb,
        MAXIMUM_PRODUCT_BYTES,
        'current emitter Stage 1 WVB',
    );
    await Runˉnative('current-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Emitterˉstage1ˉwvb, Emitter,
    ]);
    await Runˉnode('current-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Emitter, Emitterˉidentity,
    ]);
    await Runˉnode('current-analyzer-stage2', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Analysis-Driver.wvproj'),
        Analyzerˉstage2ˉwvb,
        Analyzer,
        Analyzerˉidentity,
        Emitter,
        Emitterˉidentity,
    ]);
    await Runˉnode('current-emitter-stage2', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Emission-Driver.wvproj'),
        Emitterˉstage2ˉwvb,
        Analyzer,
        Analyzerˉidentity,
        Emitter,
        Emitterˉidentity,
    ]);
    await Runˉnode('current-verifier-wvb', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Wvb-Verifier.wvproj'),
        Verifierˉwvb,
        Analyzer,
        Analyzerˉidentity,
        Emitter,
        Emitterˉidentity,
    ]);
    Requireˉexactˉfile(
        Verifierˉwvb,
        CURRENT_VERIFIER,
        'current WVB verifier',
    );
    await Runˉnative('current-verifier-package', 'Package-Segmented-Compiler-Wvb', [
        '2', Verifierˉwvb, Verifier,
    ]);
    Requireˉordinaryˉfile(
        Verifier,
        MAXIMUM_PRODUCT_BYTES,
        'current native WVB verifier',
    );
    await Runˉtool('analyzer-verification', Verifier, [Analyzerˉstage2ˉwvb]);
    await Runˉtool('emitter-verification', Verifier, [Emitterˉstage2ˉwvb]);

    const Analyzerˉstage1 = Fileˉevidence(
        Analyzerˉstage1ˉwvb,
        'current analyzer Stage 1 WVB',
        MAXIMUM_PRODUCT_BYTES,
    );
    const Analyzerˉstage2 = Fileˉevidence(
        Analyzerˉstage2ˉwvb,
        'current analyzer Stage 2 WVB',
        MAXIMUM_PRODUCT_BYTES,
    );
    const Emitterˉstage1 = Fileˉevidence(
        Emitterˉstage1ˉwvb,
        'current emitter Stage 1 WVB',
        MAXIMUM_PRODUCT_BYTES,
    );
    const Emitterˉstage2 = Fileˉevidence(
        Emitterˉstage2ˉwvb,
        'current emitter Stage 2 WVB',
        MAXIMUM_PRODUCT_BYTES,
    );
    if (!Analyzerˉstage1.value.equals(Analyzerˉstage2.value) ||
        !Emitterˉstage1.value.equals(Emitterˉstage2.value)) {
        Reject('The current split compiler did not reach an exact fixed point.');
    }
    if (!Sameˉevidence(Analyzerˉstage1, CURRENT_ANALYZER) ||
        !Sameˉevidence(Emitterˉstage1, CURRENT_EMITTER)) {
        Reject(
            'The fixed-point compiler identity differs: ' +
            `analyzer-bytes=${Analyzerˉstage1.bytes} ` +
            `analyzer-sha256=${Analyzerˉstage1.sha256} ` +
            `emitter-bytes=${Emitterˉstage1.bytes} ` +
            `emitter-sha256=${Emitterˉstage1.sha256}.`,
        );
    }
    process.stdout.write(
        'native compiler convergence status=Complete products=2 ' +
        `analyzer-bytes=${CURRENT_ANALYZER.bytes} ` +
        `analyzer-sha256=${CURRENT_ANALYZER.sha256} ` +
        `emitter-bytes=${CURRENT_EMITTER.bytes} ` +
        `emitter-sha256=${CURRENT_EMITTER.sha256} cache=Isolated\n`,
    );
} finally {
    const Resolved = path.resolve(Work);
    if (!Sameˉpath(path.dirname(Resolved), Temporaryˉroot) ||
        !path.basename(Resolved).startsWith(
            'windvale-current-split-convergence-',
        )) {
        Reject(`Refusing to remove unexpected temporary directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

function Projectˉpath(Name) {
    return path.join(Sourceˉroot, 'Projects', 'Tools', Name);
}

async function Runˉnative(Label, Name, Arguments) {
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Script = path.join(Nativeˉroot, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, MAXIMUM_PRODUCT_BYTES, `${Name} script`);
    if (process.platform === 'win32') {
        await Run(Label, process.env.ComSpec ?? 'cmd.exe', [
            '/d', '/c', 'call', Script, ...Arguments,
        ]);
        return;
    }
    await Run(Label, 'bash', [Script, ...Arguments]);
}

async function Runˉnode(Label, Name, Arguments) {
    await Run(
        Label,
        process.execPath,
        [path.join(Nativeˉroot, Name), ...Arguments],
    );
}

async function Runˉtool(Label, Command, Arguments) {
    await Run(Label, Command, Arguments);
}

async function Run(Label, Command, Arguments) {
    Phase += 1;
    process.stdout.write(
        `START native compiler convergence phase=${Phase}/${PHASES} ` +
        `step=${Label}\n`,
    );
    const Result = await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: Sourceˉroot,
            env: {
                ...process.env,
                WINDVALE_NATIVE_CACHE_ROOT: Cacheˉroot,
            },
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Started = Date.now();
        let Diagnosticˉbytes = 0;
        let Stderr = Buffer.alloc(0);
        let Timedˉout = false;
        let Settled = false;
        const Progress = setInterval(() => {
            process.stdout.write(
                `PROGRESS native compiler convergence phase=${Phase}/${PHASES} ` +
                `step=${Label} elapsed-seconds=${Math.floor(
                    (Date.now() - Started) / 1_000,
                )}\n`,
            );
        }, 30_000);
        const Timeout = setTimeout(() => {
            Timedˉout = true;
            Child.kill();
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        const Finish = Value => {
            if (Settled) return;
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            Resolve(Value);
        };
        const Append = Chunk => {
            Diagnosticˉbytes += Chunk.length;
            if (Diagnosticˉbytes > MAXIMUM_DIAGNOSTIC_BYTES) {
                Settled = true;
                clearInterval(Progress);
                clearTimeout(Timeout);
                Child.kill();
                Rejectˉpromise(new Error(
                    `${Label} diagnostics exceed 1 MiB.`,
                ));
                return false;
            }
            return true;
        };
        Child.stdout.on('data', Chunk => {
            if (Append(Chunk)) process.stdout.write(Chunk);
        });
        Child.stderr.on('data', Chunk => {
            if (Append(Chunk)) Stderr = Buffer.concat([Stderr, Chunk]);
        });
        Child.on('error', Error => {
            if (Settled) return;
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            Rejectˉpromise(Error);
        });
        Child.on('close', Status => Finish({
            status: Status,
            stderr: Stderr,
            timedOut: Timedˉout,
        }));
    });
    if (Result.stderr.length !== 0) {
        process.stderr.write(Result.stderr);
    }
    if (Result.timedOut || Result.status !== 0 || Result.stderr.length !== 0) {
        Reject(
            `${Label} failed: status=${Result.status} ` +
            `timeout=${Result.timedOut}.`,
        );
    }
    process.stdout.write(
        `PASS  native compiler convergence phase=${Phase}/${PHASES} ` +
        `step=${Label}\n`,
    );
}

function Requireˉexactˉfile(Candidate, Expected, Label) {
    const Evidence = Fileˉevidence(Candidate, Label, MAXIMUM_PRODUCT_BYTES);
    if (!Sameˉevidence(Evidence, Expected)) {
        Reject(
            `The ${Label} identity differs: ` +
            `expected-bytes=${Expected.bytes} ` +
            `expected-sha256=${Expected.sha256} ` +
            `found-bytes=${Evidence.bytes} ` +
            `found-sha256=${Evidence.sha256}.`,
        );
    }
    return Evidence;
}

function Sameˉevidence(Actual, Expected) {
    return Actual.bytes === Expected.bytes &&
        Actual.sha256 === Expected.sha256;
}

function Fileˉevidence(Candidate, Label, Maximum) {
    const Information = Requireˉordinaryˉfile(Candidate, Maximum, Label);
    const Value = readFileSync(Candidate);
    if (Value.length !== Information.size) {
        Reject(`The ${Label} changed while it was read.`);
    }
    return {
        bytes: Value.length,
        sha256: createHash('sha256').update(Value).digest('hex'),
        value: Value,
    };
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate, { throwIfNoEntry: false });
    if (Information === undefined || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum ||
        !Sameˉpath(realpathSync.native(Candidate), path.resolve(Candidate))) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    return Information;
}

function Canonicalˉordinaryˉdirectory(Candidate, Label) {
    const Resolved = path.resolve(Candidate);
    const Root = path.parse(Resolved).root;
    let Current = Root;
    for (const Component of path.relative(Root, Resolved)
            .split(path.sep).filter(Value => Value.length !== 0)) {
        Current = path.join(Current, Component);
        const Information = lstatSync(Current, { throwIfNoEntry: false });
        if (Information === undefined || !Information.isDirectory() ||
            Information.isSymbolicLink()) {
            Reject(
                `The ${Label} traverses a missing, linked, or ` +
                `non-directory path: ${Current}`,
            );
        }
    }
    return realpathSync.native(Resolved);
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Current-Split-Compiler-Convergence.mjs ' +
        '<source-root>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
