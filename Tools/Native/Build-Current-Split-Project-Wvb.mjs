import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import {
    lstatSync,
    mkdtempSync,
    readFileSync,
    realpathSync,
    rmSync,
} from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_DIAGNOSTIC_BYTES = 1_048_576;
const MAXIMUM_INPUT_BYTES = 16_777_216;
const TOOL_TIMEOUT_MILLISECONDS = 600_000;
const BOOTSTRAP_ANALYZER_BYTES = 992_412;
const BOOTSTRAP_ANALYZER_SHA256 =
    '26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120';
const BOOTSTRAP_EMITTER_BYTES = 895_787;
const BOOTSTRAP_EMITTER_SHA256 =
    'ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94';
const BRIDGE_EMITTER_BYTES = 1_146_083;
const BRIDGE_EMITTER_SHA256 =
    '0d838b6d983320cf22b9094ef5a4692d6833f1834292863789577e034f6febdb';

if (process.argv.length !== 4) {
    Usage();
}
if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`Unsupported current split-compiler host: ${process.platform}-${process.arch}.`);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Project = path.resolve(process.argv[2]);
const Output = path.resolve(process.argv[3]);
if (path.extname(Project).toLowerCase() !== '.wvproj' ||
    path.extname(Output).toLowerCase() !== '.wvb') {
    Usage();
}
Requireˉordinaryˉfile(Project, 65_536, 'project manifest');
Requireˉordinaryˉdirectory(path.dirname(Output), 'output parent');

const Bootstrapˉroot = path.join(
    Repositoryˉroot,
    'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap', 'Wvb',
);
const Bootstrapˉanalyzerˉwvb = path.join(Bootstrapˉroot, 'wvanalyze.wvb');
const Bootstrapˉemitterˉwvb = path.join(Bootstrapˉroot, 'wvemit.wvb');
const Bridgeˉemitterˉwvb = path.join(Bootstrapˉroot, 'wvemit-wvir-1.9-bridge.wvb');
Requireˉexactˉfile(
    Bootstrapˉanalyzerˉwvb,
    BOOTSTRAP_ANALYZER_BYTES,
    BOOTSTRAP_ANALYZER_SHA256,
    'bootstrap analyzer',
);
Requireˉexactˉfile(
    Bootstrapˉemitterˉwvb,
    BOOTSTRAP_EMITTER_BYTES,
    BOOTSTRAP_EMITTER_SHA256,
    'bootstrap emitter',
);
Requireˉexactˉfile(
    Bridgeˉemitterˉwvb,
    BRIDGE_EMITTER_BYTES,
    BRIDGE_EMITTER_SHA256,
    'WVIR 1.9 bridge emitter',
);

const Work = mkdtempSync(path.join(os.tmpdir(), 'windvale-current-split-project-'));
const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
const Bootstrapˉanalyzer = path.join(Work, `Bootstrap-Analyzer${Suffix}`);
const Bootstrapˉemitter = path.join(Work, `Bootstrap-Emitter${Suffix}`);
const Bridgeˉemitter = path.join(Work, `Bridge-Emitter${Suffix}`);
const Analyzerˉwvb = path.join(Work, 'Analyzer.wvb');
const Analyzer = path.join(Work, `Analyzer${Suffix}`);
const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
const Emitterˉwvb = path.join(Work, 'Emitter.wvb');
const Emitter = path.join(Work, `Emitter${Suffix}`);
const Emitterˉidentity = path.join(Work, 'Emitter.identity');
let Step = 0;

try {
    await Runˉnative('bootstrap-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉanalyzerˉwvb, Bootstrapˉanalyzer, '--development-cache',
    ]);
    await Runˉnative('bootstrap-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bootstrapˉemitterˉwvb, Bootstrapˉemitter, '--development-cache',
    ]);
    const Bootstrapˉanalyzerˉidentity = path.join(
        Work, 'Bootstrap-Analyzer.identity',
    );
    const Bootstrapˉemitterˉidentity = path.join(
        Work, 'Bootstrap-Emitter.identity',
    );
    await Runˉnode('bootstrap-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Bootstrapˉanalyzer, Bootstrapˉanalyzerˉidentity,
    ]);
    await Runˉnode('bootstrap-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Bootstrapˉemitter, Bootstrapˉemitterˉidentity,
    ]);
    await Runˉnative('bridge-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Bridgeˉemitterˉwvb, Bridgeˉemitter, '--development-cache',
    ]);
    const Bridgeˉemitterˉidentity = path.join(Work, 'Bridge-Emitter.identity');
    await Runˉnode('bridge-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Bridgeˉemitter, Bridgeˉemitterˉidentity,
    ]);
    await Runˉnode('current-analyzer-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Analysis-Driver.wvproj'),
        Analyzerˉwvb,
        Bootstrapˉanalyzer,
        Bootstrapˉanalyzerˉidentity,
        Bootstrapˉemitter,
        Bootstrapˉemitterˉidentity,
    ]);
    await Runˉnative('current-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Analyzerˉwvb, Analyzer, '--development-cache',
    ]);
    await Runˉnode('current-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Analyzer, Analyzerˉidentity,
    ]);
    await Runˉnode('current-emitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Emission-Driver.wvproj'),
        Emitterˉwvb,
        Analyzer,
        Analyzerˉidentity,
        Bridgeˉemitter,
        Bridgeˉemitterˉidentity,
    ]);
    await Runˉnative('current-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Emitterˉwvb, Emitter, '--development-cache',
    ]);
    await Runˉnode('current-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Emitter, Emitterˉidentity,
    ]);
    await Runˉnode('target-project-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Project, Output, Analyzer, Analyzerˉidentity, Emitter, Emitterˉidentity,
    ]);
    const Evidence = Fileˉevidence(Output, 'published WVB', MAXIMUM_INPUT_BYTES);
    process.stdout.write(
        `current split project status=Complete steps=${Step} ` +
        `wvb-bytes=${Evidence.bytes} wvb-sha256=${Evidence.sha256}\n`,
    );
} finally {
    const Resolved = path.resolve(Work);
    const Temporaryˉroot = path.resolve(os.tmpdir());
    if (path.dirname(Resolved) !== Temporaryˉroot ||
        !path.basename(Resolved).startsWith('windvale-current-split-project-')) {
        Reject(`Refusing to remove unexpected temporary directory: ${Resolved}.`);
    }
    rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
}

function Projectˉpath(Name) {
    return path.join(Repositoryˉroot, 'Projects', 'Tools', Name);
}

async function Runˉnative(Label, Name, Arguments) {
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, MAXIMUM_INPUT_BYTES, `${Name} script`);
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
        [path.join(Scriptˉdirectory, Name), ...Arguments],
    );
}

async function Run(Label, Command, Arguments) {
    Step += 1;
    process.stdout.write(
        `START current split project step=${Step}/13 phase=${Label}\n`,
    );
    const Result = await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: Repositoryˉroot,
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Started = Date.now();
        let Diagnosticˉbytes = 0;
        let Stderr = Buffer.alloc(0);
        let Timedˉout = false;
        const Progress = setInterval(() => {
            process.stdout.write(
                `PROGRESS current split project step=${Step}/13 phase=${Label} ` +
                `elapsed-seconds=${Math.floor((Date.now() - Started) / 1_000)}\n`,
            );
        }, 30_000);
        const Timeout = setTimeout(() => {
            Timedˉout = true;
            Child.kill();
        }, TOOL_TIMEOUT_MILLISECONDS);
        const Append = Chunk => {
            Diagnosticˉbytes += Chunk.length;
            if (Diagnosticˉbytes > MAXIMUM_DIAGNOSTIC_BYTES) {
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
            clearInterval(Progress);
            clearTimeout(Timeout);
            Rejectˉpromise(Error);
        });
        Child.on('close', Status => {
            clearInterval(Progress);
            clearTimeout(Timeout);
            Resolve({ status: Status, stderr: Stderr, timedOut: Timedˉout });
        });
    });
    if (Result.timedOut || Result.status !== 0 || Result.stderr.length !== 0) {
        if (Result.stderr.length !== 0) {
            process.stderr.write(Result.stderr);
        }
        Reject(
            `${Label} failed: status=${Result.status} ` +
            `timeout=${Result.timedOut}.`,
        );
    }
    process.stdout.write(
        `PASS  current split project step=${Step}/13 phase=${Label}\n`,
    );
}

function Fileˉevidence(Candidate, Label, Maximum) {
    const Information = Requireˉordinaryˉfile(Candidate, Maximum, Label);
    const Bytes = readFileSync(Candidate);
    if (Bytes.length !== Information.size) {
        Reject(`The ${Label} changed while it was read.`);
    }
    return {
        bytes: Bytes.length,
        sha256: createHash('sha256').update(Bytes).digest('hex'),
    };
}

function Requireˉexactˉfile(Candidate, Bytes, Sha256, Label) {
    const Evidence = Fileˉevidence(Candidate, Label, Bytes);
    if (Evidence.bytes !== Bytes || Evidence.sha256 !== Sha256) {
        Reject(`The ${Label} identity differs.`);
    }
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate, { throwIfNoEntry: false });
    if (Information === undefined || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum ||
        !Sameˉpath(realpathSync(Candidate), path.resolve(Candidate))) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    return Information;
}

function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = lstatSync(Candidate, { throwIfNoEntry: false });
    if (Information === undefined || !Information.isDirectory() ||
        Information.isSymbolicLink() ||
        !Sameˉpath(realpathSync(Candidate), path.resolve(Candidate))) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}`);
    }
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Build-Current-Split-Project-Wvb.mjs ' +
        '<project.wvproj> <output.wvb>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
