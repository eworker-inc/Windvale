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
const MAXIMUM_TARGET_PROJECTS = 8;
const TOOL_TIMEOUT_MILLISECONDS = 600_000;
const PINNED_ANALYZER_BYTES = 1_552_090;
const PINNED_ANALYZER_SHA256 =
    '5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77';
const PINNED_EMITTER_BYTES = 1_556_434;
const PINNED_EMITTER_SHA256 =
    'd16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9';

const Argumentˉcount = process.argv.length - 2;
if (Argumentˉcount < 2 || Argumentˉcount % 2 !== 0 ||
    Argumentˉcount / 2 > MAXIMUM_TARGET_PROJECTS) {
    Usage();
}
if (process.arch !== 'x64' ||
    (process.platform !== 'win32' && process.platform !== 'linux')) {
    Reject(`Unsupported current split-compiler host: ${process.platform}-${process.arch}.`);
}

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Targets = [];
const Outputˉidentities = new Set();
for (let Index = 2; Index < process.argv.length; Index += 2) {
    const Project = path.resolve(process.argv[Index]);
    const Outputˉargument = path.resolve(process.argv[Index + 1]);
    if (path.extname(Project).toLowerCase() !== '.wvproj' ||
        path.extname(Outputˉargument).toLowerCase() !== '.wvb') {
        Usage();
    }
    Requireˉordinaryˉfile(Project, 65_536, 'project manifest');
    const Outputˉparent = Canonicalˉordinaryˉdirectory(
        path.dirname(Outputˉargument),
        'output parent',
    );
    const Output = path.join(Outputˉparent, path.basename(Outputˉargument));
    const Outputˉidentity = process.platform === 'win32'
        ? Output.toLowerCase()
        : Output;
    if (Outputˉidentities.has(Outputˉidentity)) {
        Reject(`Duplicate current split-project output: ${Output}.`);
    }
    Outputˉidentities.add(Outputˉidentity);
    Targets.push({ Project, Output });
}

const Bootstrapˉroot = path.join(
    Repositoryˉroot,
    'Artifacts', 'Language-1.0-Target-Aware-Emission-Bootstrap', 'Wvb',
);
const Pinnedˉanalyzerˉwvb = path.join(Bootstrapˉroot, 'wvanalyze.wvb');
const Pinnedˉemitterˉwvb = path.join(Bootstrapˉroot, 'wvemit.wvb');
Requireˉexactˉfile(
    Pinnedˉanalyzerˉwvb,
    PINNED_ANALYZER_BYTES,
    PINNED_ANALYZER_SHA256,
    'pinned analyzer',
);
Requireˉexactˉfile(
    Pinnedˉemitterˉwvb,
    PINNED_EMITTER_BYTES,
    PINNED_EMITTER_SHA256,
    'pinned emitter',
);

const Temporaryˉroot = Canonicalˉordinaryˉdirectory(
    os.tmpdir(),
    'temporary root',
);
const Work = mkdtempSync(path.join(
    Temporaryˉroot,
    'windvale-current-split-project-',
));
const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
const Pinnedˉanalyzer = path.join(Work, `Pinned-Analyzer${Suffix}`);
const Pinnedˉemitter = path.join(Work, `Pinned-Emitter${Suffix}`);
const Analyzerˉwvb = path.join(Work, 'Analyzer.wvb');
const Analyzer = path.join(Work, `Analyzer${Suffix}`);
const Analyzerˉidentity = path.join(Work, 'Analyzer.identity');
const Checkpointˉanalyzer = path.join(Work, `Checkpoint-Analyzer${Suffix}`);
const Checkpointˉanalyzerˉidentity = path.join(
    Work, 'Checkpoint-Analyzer.identity',
);
const Emitterˉwvb = path.join(Work, 'Emitter.wvb');
const Emitter = path.join(Work, `Emitter${Suffix}`);
const Emitterˉidentity = path.join(Work, 'Emitter.identity');
let Step = 0;
const Totalˉsteps = 12 + Targets.length;

try {
    await Runˉnative('pinned-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Pinnedˉanalyzerˉwvb, Pinnedˉanalyzer, '--development-cache',
    ]);
    await Runˉnative('pinned-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Pinnedˉemitterˉwvb, Pinnedˉemitter, '--development-cache',
    ]);
    const Pinnedˉanalyzerˉidentity = path.join(
        Work, 'Pinned-Analyzer.identity',
    );
    const Pinnedˉemitterˉidentity = path.join(
        Work, 'Pinned-Emitter.identity',
    );
    await Runˉnode('pinned-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Pinnedˉanalyzer, Pinnedˉanalyzerˉidentity,
    ]);
    await Runˉnode('pinned-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Pinnedˉemitter, Pinnedˉemitterˉidentity,
    ]);
    await Runˉnode('stage1-analyzer-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Analysis-Driver.wvproj'),
        Analyzerˉwvb,
        Pinnedˉanalyzer,
        Pinnedˉanalyzerˉidentity,
        Pinnedˉemitter,
        Pinnedˉemitterˉidentity,
    ]);
    await Runˉnative('stage1-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '7', Analyzerˉwvb, Analyzer, '--development-cache',
    ]);
    await Runˉnode('stage1-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Analyzer, Analyzerˉidentity,
    ]);
    await Runˉnative('stage1-checkpoint-analyzer-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Analyzerˉwvb, Checkpointˉanalyzer, '--development-cache',
    ]);
    await Runˉnode('stage1-checkpoint-analyzer-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'analyzer', Checkpointˉanalyzer, Checkpointˉanalyzerˉidentity,
    ]);
    await Runˉnode('stage1-emitter-build', 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath('Windvale-Compiler-Emission-Driver.wvproj'),
        Emitterˉwvb,
        Checkpointˉanalyzer,
        Checkpointˉanalyzerˉidentity,
        Pinnedˉemitter,
        Pinnedˉemitterˉidentity,
        '--symbol-checkpoint',
    ]);
    await Runˉnative('stage1-emitter-package', 'Package-Segmented-Compiler-Wvb', [
        '8', Emitterˉwvb, Emitter, '--development-cache',
    ]);
    await Runˉnode('stage1-emitter-identity', 'Write-Split-Compiler-Producer-Identity.mjs', [
        'emitter', Emitter, Emitterˉidentity,
    ]);
    const Evidence = [];
    for (const [Index, Target] of Targets.entries()) {
        const Label = Targets.length === 1
            ? 'target-project-build'
            : `target-project-build-${Index + 1}`;
        await Runˉnode(Label, 'Build-Cached-Split-Project-Wvb.mjs', [
            Target.Project,
            Target.Output,
            Analyzer,
            Analyzerˉidentity,
            Emitter,
            Emitterˉidentity,
            '--symbol-checkpoint',
        ]);
        const Targetˉevidence = Fileˉevidence(
            Target.Output,
            `published WVB ${Index + 1}`,
            MAXIMUM_INPUT_BYTES,
        );
        Evidence.push(Targetˉevidence);
        if (Targets.length > 1) {
            process.stdout.write(
                `current split project target=${Index + 1}/${Targets.length} ` +
                `wvb-bytes=${Targetˉevidence.bytes} ` +
                `wvb-sha256=${Targetˉevidence.sha256}\n`,
            );
        }
    }
    if (Targets.length === 1) {
        process.stdout.write(
            `current split project status=Complete steps=${Step} ` +
            `wvb-bytes=${Evidence[0].bytes} ` +
            `wvb-sha256=${Evidence[0].sha256}\n`,
        );
    } else {
        process.stdout.write(
            `current split projects status=Complete steps=${Step} ` +
            `projects=${Targets.length}\n`,
        );
    }
} finally {
    const Resolved = path.resolve(Work);
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
        `START current split project step=${Step}/${Totalˉsteps} phase=${Label}\n`,
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
                `PROGRESS current split project step=${Step}/${Totalˉsteps} ` +
                `phase=${Label} ` +
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
        `PASS  current split project step=${Step}/${Totalˉsteps} phase=${Label}\n`,
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

function Canonicalˉordinaryˉdirectory(Candidate, Label) {
    const Resolved = path.resolve(Candidate);
    const Root = path.parse(Resolved).root;
    let Current = Root;
    for (const Component of Resolved.slice(Root.length).split(path.sep)) {
        if (Component.length === 0) continue;
        Current = path.join(Current, Component);
        const Information = lstatSync(Current, { throwIfNoEntry: false });
        if (Information === undefined || !Information.isDirectory() ||
            Information.isSymbolicLink()) {
            Reject(
                `The ${Label} contains a missing, linked, or ` +
                `non-directory path: ${Current}`,
            );
        }
    }
    const Canonical = realpathSync.native(Resolved);
    Requireˉordinaryˉdirectory(Canonical, Label);
    return Canonical;
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Build-Current-Split-Project-Wvb.mjs ' +
        '<project.wvproj> <output.wvb> ' +
        '[<project.wvproj> <output.wvb> ...]\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
