import { Runˉdevelopmentˉcommand } from './Development-Command-Core.mjs';
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
import {
    Acquireˉcurrentˉsplitˉcompiler,
    Constructˉcurrentˉsplitˉcompiler,
    Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
} from './Current-Split-Compiler-Cache-Core.mjs';

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
let Step = 0;
let Totalˉsteps = 12 + Targets.length;

try {
    const Compilerˉkey = await Getˉcurrentˉsplitˉcompilerˉkey();
    const Compilerˉcheckpoint = await Acquireˉcurrentˉsplitˉcompiler(
        await Getˉcurrentˉsplitˉcompilerˉfamily(),
        Compilerˉkey,
        Candidate => Constructˉcurrentˉsplitˉcompiler(
            Work, Candidate, Runˉnative, Runˉnode,
        ),
        async () => {
            if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey) {
                Reject('Current compiler construction inputs changed.');
            }
        },
    );
    if (Compilerˉcheckpoint.status === 'Hit' && Step === 0) {
        Totalˉsteps = Targets.length;
    }
    process.stdout.write(
        'current split compiler cache status=' + Compilerˉcheckpoint.status +
        ' key=' + Compilerˉkey + '\n',
    );
    const Evidence = [];
    for (const [Index, Target] of Targets.entries()) {
        const Label = Targets.length === 1
            ? 'target-project-build'
            : `target-project-build-${Index + 1}`;
        await Runˉnode(Label, 'Build-Cached-Split-Project-Wvb.mjs', [
            Target.Project,
            Target.Output,
            path.join(Compilerˉcheckpoint.directory, 'Analyzer' + Suffix),
            path.join(Compilerˉcheckpoint.directory, 'Analyzer.identity'),
            path.join(Compilerˉcheckpoint.directory, 'Emitter' + Suffix),
            path.join(Compilerˉcheckpoint.directory, 'Emitter.identity'),
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

async function Runˉnative(Label, Name, Arguments) {
    const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
    const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, MAXIMUM_INPUT_BYTES, `${Name} script`);
    if (process.platform === 'win32') {
        await Run(Label, Script, Arguments);
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
    const Currentˉstep = ++Step;
    const Started = Date.now();
    process.stdout.write(
        `START current split project step=${Currentˉstep}/${Totalˉsteps} phase=${Label}\n`,
    );
    const Result = await Runˉdevelopmentˉcommand(
        Command, Arguments, Started + TOOL_TIMEOUT_MILLISECONDS,
        true, MAXIMUM_DIAGNOSTIC_BYTES,
    );
    if (Result.Code !== 0 || Result.Error !== '') {
        if (Result.Error !== '') process.stderr.write(Result.Error);
        Reject(`${Label} failed: status=${Result.Code}.`);
    }
    process.stdout.write(
        `PASS  current split project step=${Currentˉstep}/${Totalˉsteps} phase=${Label} ` +
        `elapsed-ms=${Date.now() - Started}\n`,
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
