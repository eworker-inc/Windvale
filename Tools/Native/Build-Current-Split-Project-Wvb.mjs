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
    CURRENT_COMPILER_PROJECTS,
    Getˉcurrentˉsplitˉcompilerˉcacheˉroot,
    Getˉcurrentˉsplitˉcompilerˉfamily,
    Getˉcurrentˉsplitˉcompilerˉkey,
    Readˉpreparedˉsplitˉcompiler,
} from './Current-Split-Compiler-Cache-Core.mjs';
import {
    Constructˉsourceˉeditionˉpredecessor,
    Hasˉuncertainˉconstructionˉcleanup,
    Runˉcompilerˉconstructionˉcommand,
} from './Source-Edition-Predecessor-Core.mjs';

import { Checkˉprojectˉconstructionˉreadiness } from './Project-Construction-Readiness-Core.mjs';

const MAXIMUM_DIAGNOSTIC_BYTES = 1_048_576;
const MAXIMUM_INPUT_BYTES = 16_777_216;
const MAXIMUM_TARGET_PROJECTS = 8;
const TOOL_TIMEOUT_MILLISECONDS = 600_000;
// Existing package: 120s verification + 900s image/container + 30s settlement.
const PREPARATION_PACKAGE_TIMEOUT_MILLISECONDS = 1_050_000;
const CLEANUP_RESERVE_MILLISECONDS = 30_000;
const PINNED_ANALYZER_BYTES = 1_552_090;
const PINNED_ANALYZER_SHA256 =
    '5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77';
const PINNED_EMITTER_BYTES = 1_556_434;
const PINNED_EMITTER_SHA256 =
    'd16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9';

export function Currentˉsplitˉcommandˉdeadline(
    Name, Preparingˉcompiler, Workˉdeadline, Started = Date.now(),
) {
    const Preparationˉpackage = Preparingˉcompiler === true &&
        (Name === 'Package-Segmented-Compiler-Wvb' ||
            Name === 'Build-Cached-Segmented-Hosted-Wvb.mjs');
    const Maximum = Preparationˉpackage
        ? PREPARATION_PACKAGE_TIMEOUT_MILLISECONDS : TOOL_TIMEOUT_MILLISECONDS;
    return Workˉdeadline === null ? Started + Maximum
        : Math.min(Started + Maximum, Workˉdeadline);
}

async function Buildˉcurrentˉsplitˉprojects() {
    const Targetˉarguments = [];
    let Deadline = null;
    let Prepareˉonly = false;
    let Preflightˉonly = false;
    let Preparedˉonly = false;
    let Prepareˉcompiler = false;
    let Selectedˉkey = null;
    for (let Index = 2; Index < process.argv.length; Index += 1) {
        if (process.argv[Index] === '--deadline-ms') {
            if (Deadline !== null || Index + 1 >= process.argv.length) Usage();
            const Value = process.argv[++Index];
            if (!/^[1-9][0-9]*$/u.test(Value) || !Number.isSafeInteger(Number(Value))) {
                Reject('Invalid current split-project deadline.');
            }
            Deadline = Number(Value);
        } else if (process.argv[Index] === '--prepare-only') {
            if (Prepareˉonly) Usage();
            Prepareˉonly = true;
        } else if (process.argv[Index] === '--preflight-only') {
            if (Preflightˉonly) Usage();
            Preflightˉonly = true;
        } else if (process.argv[Index] === '--compiler-checkpoint') {
            if (Selectedˉkey !== null || Index + 1 >= process.argv.length) Usage();
            Selectedˉkey = process.argv[++Index];
            if (!/^[0-9a-f]{64}$/u.test(Selectedˉkey)) Usage();
        } else if (process.argv[Index] === '--prepare-compiler') {
            if (Prepareˉcompiler) Usage();
            Prepareˉcompiler = true;
        } else if (process.argv[Index] === '--prepared-compiler-only') {
            if (Preparedˉonly) Usage();
            Preparedˉonly = true;
        } else {
            Targetˉarguments.push(process.argv[Index]);
        }
    }
    const Environmentˉmode = process.env.WINDVALE_PREPARED_COMPILER_ONLY;
    if (Environmentˉmode !== undefined && Environmentˉmode !== '1') {
        Reject('WINDVALE_PREPARED_COMPILER_ONLY must be absent or 1.');
    }
    Preparedˉonly ||= Environmentˉmode === '1';
    const Productˉmode = process.env.WINDVALE_PREPARED_PRODUCTS_ONLY;
    if (Productˉmode !== undefined && Productˉmode !== '1') {
        Reject('WINDVALE_PREPARED_PRODUCTS_ONLY must be absent or 1.');
    }
    Preparedˉonly ||= Productˉmode === '1';
    if ((Prepareˉcompiler && (Preparedˉonly || Prepareˉonly || Deadline === null)) ||
        (Selectedˉkey !== null && (Prepareˉonly || Prepareˉcompiler))) Usage();
    const Argumentˉcount = Targetˉarguments.length;
    if ((Prepareˉonly ? Argumentˉcount !== 0 || Deadline === null : Argumentˉcount < 2) ||
        Argumentˉcount % 2 !== 0 ||
        Argumentˉcount / 2 > MAXIMUM_TARGET_PROJECTS) {
        Usage();
    }
    Deadline ??= Date.now() + 600_000;
    const Workˉdeadline = Deadline === null ? null : Deadline - CLEANUP_RESERVE_MILLISECONDS;
    Requireˉtime(Workˉdeadline, 'construction');
    if (process.arch !== 'x64' ||
        (process.platform !== 'win32' && process.platform !== 'linux')) {
        Reject(`Unsupported current split-compiler host: ${process.platform}-${process.arch}.`);
    }

    const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
    const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
    const Targets = [];
    const Outputˉidentities = new Set();
    for (let Index = 0; Index < Targetˉarguments.length; Index += 2) {
        const Project = path.resolve(Targetˉarguments[Index]);
        const Outputˉargument = path.resolve(Targetˉarguments[Index + 1]);
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
    if (Prepareˉonly || Prepareˉcompiler) {
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
    }

    const Temporaryˉroot = Canonicalˉordinaryˉdirectory(
        os.tmpdir(),
        'temporary root',
    );
    Requireˉtime(Workˉdeadline, 'readiness');
    const Readiness = await Checkˉprojectˉconstructionˉreadiness({
        Repository: Repositoryˉroot,
        Projects: Prepareˉonly || Prepareˉcompiler
            ? CURRENT_COMPILER_PROJECTS.map(Name => path.join(Repositoryˉroot, 'Projects', 'Tools', Name))
            : Targets.map(Value => Value.Project),
        Temporary: Temporaryˉroot, Cache: Getˉcurrentˉsplitˉcompilerˉcacheˉroot(),
        Outputs: Targets.map(Value => Value.Output),
        Prepare: Prepareˉonly || Prepareˉcompiler, Preparedˉonly, Deadline: Workˉdeadline,
    });
    console.log(`current-split-readiness ${JSON.stringify(Readiness)}`);
    if (Preflightˉonly) return;
    Requireˉtime(Workˉdeadline, 'work-directory creation');
    const Work = mkdtempSync(path.join(
        Temporaryˉroot,
        'windvale-current-split-project-',
    ));
    const Suffix = process.platform === 'win32' ? '.exe' : '.elf';
    let Step = 0;
    let Totalˉsteps = 20 + Targets.length;
    let Preserveˉwork = false;
    let Primaryˉfailure = null;

    try {
        const Compilerˉkey = Selectedˉkey ?? await Getˉcurrentˉsplitˉcompilerˉkey();
        const Family = await Getˉcurrentˉsplitˉcompilerˉfamily();
        const Compilerˉcheckpoint = Prepareˉonly || Prepareˉcompiler ? await Acquireˉcurrentˉsplitˉcompiler(
            Family,
            Compilerˉkey,
            Candidate => {
                if (Preparedˉonly) {
                    throw Object.assign(new Error(
                        'Current compiler checkpoint missing. Prepared-only execution does not construct it. ' +
                        'Run Build-Current-Split-Project-Wvb.mjs --prepare-only --deadline-ms <absolute-unix-ms> ' +
                        'in a separately budgeted preparation phase.'
                    ), { exitCode: 64 });
                }
                return Constructˉcurrentˉsplitˉcompiler(
                    Work, Candidate,
                    (Label, Name, Arguments) => Runˉnative(Label, Name, Arguments, true),
                    (Label, Name, Arguments) => Runˉnode(Label, Name, Arguments, true),
                    async Place => {
                        try {
                            return await Constructˉsourceˉeditionˉpredecessor(Place, Workˉdeadline);
                        } catch (Error) {
                            // Keep a failed bounded worktree release recoverable; deleting
                            // its parent would leave an orphaned Git registration.
                            if (Error.predecessorCheckout !== undefined) Preserveˉwork = true;
                            throw Error;
                        }
                    },
                    undefined,
                    { Mode: 'prepare', Key: Compilerˉkey, Family, Deadline: Workˉdeadline,
                        Requireˉunchanged: async () => {
                            Requireˉtime(Workˉdeadline, 'intermediate identity check');
                            if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey) {
                                Reject('Current compiler construction inputs changed.');
                            }
                            Requireˉtime(Workˉdeadline, 'intermediate identity check');
                        } },
                );
            },
            async () => {
                Requireˉtime(Workˉdeadline, 'compiler identity check');
                if (await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey) {
                    Reject('Current compiler construction inputs changed.');
                }
                Requireˉtime(Workˉdeadline, 'compiler identity check');
            },
        ) : await Readˉpreparedˉsplitˉcompiler(Family, Compilerˉkey);
        async function Requireˉcompilerˉunchanged() {
            if (Compilerˉcheckpoint.Requireˉunchanged) await Compilerˉcheckpoint.Requireˉunchanged();
            if (Selectedˉkey === null && await Getˉcurrentˉsplitˉcompilerˉkey() !== Compilerˉkey) {
                Reject('Current compiler construction inputs changed.');
            }
            Requireˉtime(Workˉdeadline, 'compiler identity check');
        }
        await Requireˉcompilerˉunchanged();
        Requireˉtime(Workˉdeadline, 'compiler construction');
        if (Compilerˉcheckpoint.status === 'Hit' && Step === 0) {
            Totalˉsteps = Targets.length;
        }
        process.stdout.write(
            'current split compiler cache status=' + Compilerˉcheckpoint.status +
            ' key=' + Compilerˉkey + '\n',
        );
        process.stdout.write(`compiler selection=${Selectedˉkey === null ? 'current-source' : 'explicit-checkpoint'} ` +
            `construction=${Prepareˉonly || Prepareˉcompiler ? 'allowed' : 'disabled'}\n`);
        const Evidence = [];
        for (const [Index, Target] of Targets.entries()) {
            const Label = Targets.length === 1
                ? 'target-project-build'
                : `target-project-build-${Index + 1}`;
            const Modern = readFileSync(Target.Project, 'utf8').split(/\r?\n/u)[0] === 'windvale-project 4';
            await Runˉnode(Label, 'Build-Cached-Split-Project-Wvb.mjs', [
                Target.Project,
                Target.Output,
                path.join(Compilerˉcheckpoint.directory, 'Analyzer' + Suffix),
                path.join(Compilerˉcheckpoint.directory, 'Analyzer.identity'),
                path.join(Compilerˉcheckpoint.directory, 'Emitter' + Suffix),
                path.join(Compilerˉcheckpoint.directory, 'Emitter.identity'),
                ...(Modern ? ['--authenticated-project4',
                    ...['Admitter', 'Authenticator', 'Reader', 'Binder'].map(Name =>
                        path.join(Compilerˉcheckpoint.directory, Name + Suffix))] : ['--symbol-checkpoint']),
            ]);
            const Targetˉevidence = Fileˉevidence(
                Target.Output,
                `published WVB ${Index + 1}`,
                MAXIMUM_INPUT_BYTES,
            );
            Evidence.push(Targetˉevidence);
            await Requireˉcompilerˉunchanged();
            Requireˉtime(Workˉdeadline, 'target publication');
            if (Targets.length > 1) {
                process.stdout.write(
                    `current split project target=${Index + 1}/${Targets.length} ` +
                    `wvb-bytes=${Targetˉevidence.bytes} ` +
                    `wvb-sha256=${Targetˉevidence.sha256}\n`,
                );
            }
        }
        if (Prepareˉonly) {
            process.stdout.write(
                `current split compiler preparation status=Complete steps=${Step} key=${Compilerˉkey}\n`,
            );
        } else if (Targets.length === 1) {
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
    } catch (Failure) {
        Primaryˉfailure = Failure instanceof Error ? Failure : new Error(String(Failure));
        if (Hasˉuncertainˉconstructionˉcleanup(Primaryˉfailure)) Preserveˉwork = true;
        throw Primaryˉfailure;
    } finally {
        try {
            const Resolved = path.resolve(Work);
            if (path.dirname(Resolved) !== Temporaryˉroot ||
                !path.basename(Resolved).startsWith('windvale-current-split-project-')) {
                Reject(`Refusing to remove unexpected temporary directory: ${Resolved}.`);
            }
            if (Preserveˉwork) {
                process.stderr.write(`Preserved current split-project work after uncertain construction cleanup: ${Resolved}\n`);
            } else {
                Canonicalˉordinaryˉdirectory(Resolved, 'current split-project cleanup');
                rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
                Requireˉtime(Deadline, 'cleanup');
            }
        } catch (Failure) {
            const Cleanupˉfailure = Failure instanceof Error ? Failure : new Error(String(Failure));
            if (Primaryˉfailure === null) throw Cleanupˉfailure;
            Primaryˉfailure.cleanupFailure = Cleanupˉfailure;
        }
    }

    async function Runˉnative(Label, Name, Arguments, Preparingˉcompiler = false) {
        const Extension = process.platform === 'win32' ? '.cmd' : '.sh';
        const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
        Requireˉordinaryˉfile(Script, MAXIMUM_INPUT_BYTES, `${Name} script`);
        if (process.platform === 'win32') {
            await Run(Label, Script, Arguments, Name, Preparingˉcompiler);
            return;
        }
        await Run(Label, 'bash', [Script, ...Arguments], Name, Preparingˉcompiler);
    }

    async function Runˉnode(Label, Name, Arguments, Preparingˉcompiler = false) {
        await Run(
            Label,
            process.execPath,
            [path.join(Scriptˉdirectory, Name), ...Arguments],
            Name, Preparingˉcompiler,
        );
    }

    async function Run(Label, Command, Arguments, Name, Preparingˉcompiler) {
        Requireˉtime(Workˉdeadline, Label);
        const Currentˉstep = ++Step;
        const Started = Date.now();
        process.stdout.write(
            `START current split project step=${Currentˉstep}/${Totalˉsteps} phase=${Label}\n`,
        );
        await Runˉcompilerˉconstructionˉcommand(
            Label, Command, Arguments,
            Currentˉsplitˉcommandˉdeadline(Name, Preparingˉcompiler, Workˉdeadline, Started),
            true,
        );
        Requireˉtime(Workˉdeadline, Label);
        process.stdout.write(
            `PASS  current split project step=${Currentˉstep}/${Totalˉsteps} phase=${Label} ` +
            `elapsed-ms=${Date.now() - Started}\n`,
        );
    }
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
    throw Object.assign(new Error(
        'Usage: node Tools/Native/Build-Current-Split-Project-Wvb.mjs ' +
        '[--deadline-ms <absolute-unix-ms>] [--preflight-only] ' +
        '[--compiler-checkpoint <sha256-key> | --prepare-compiler] ' +
        '[--prepared-compiler-only] <project.wvproj> <output.wvb> ' +
        '[<project.wvproj> <output.wvb> ...] or ' +
        '--prepare-only --deadline-ms <absolute-unix-ms> [--prepared-compiler-only]\n',
    ), { exitCode: 64 });
}

function Requireˉtime(Boundary, Phase) {
    if (Boundary !== null && Date.now() >= Boundary) {
        throw Object.assign(new Error(`Current split-project deadline expired before ${Phase}.`), { exitCode: 124 });
    }
}

function Reject(Message) {
    throw new Error(Message);
}

function Failureˉcode(Failure) {
    if (Hasˉuncertainˉconstructionˉcleanup(Failure)) return 2;
    if (Failure === null || typeof Failure !== 'object') return 1;
    if (Number.isInteger(Failure.exitCode) && Failure.exitCode >= 1 && Failure.exitCode <= 255) {
        return Failure.exitCode;
    }
    // Construction joins independent branches. A timed-out or broken producer
    // must not become an ordinary assertion failure when wrapped by that join.
    const Pending = Failure instanceof AggregateError ? Failure.errors.slice(0, 8) : [];
    const Seen = new Set();
    let Status = 1;
    while (Pending.length !== 0 && Seen.size < 32) {
        const Child = Pending.shift();
        if (Seen.has(Child)) continue;
        Seen.add(Child);
        if (Child === null || typeof Child !== 'object') continue;
        const Code = Child.exitCode;
        if (Number.isInteger(Code) && Code >= 1 && Code <= 255) {
            if (Code === 124) return 124;
            if (Code === 2 || Status === 1) Status = Code;
        } else if (Child instanceof AggregateError) {
            Pending.push(...Child.errors.slice(0, 8));
        }
    }
    return Status;
}

function Failureˉdetails(Failure) {
    if (Failure === null || typeof Failure !== 'object') return '';
    const Fields = [];
    for (const [Name, Maximum] of [['code', 64], ['syscall', 64], ['path', 1_024]]) {
        const Value = Failure[Name];
        if (typeof Value === 'string' || typeof Value === 'number') {
            Fields.push(Name + '=' + String(Value).slice(0, Maximum).replace(/[\r\n\t]/gu, ' '));
        }
    }
    const Frames = typeof Failure.stack === 'string'
        ? Failure.stack.slice(0, 16_384).split('\n').filter(Line => /^\s+at /u.test(Line)).slice(0, 8)
            .map(Line => Line.slice(0, 256)) : [];
    return (Fields.length === 0 ? '' : '\nDetails: ' + Fields.join(' ')) +
        (Frames.length === 0 ? '' : '\n' + Frames.join('\n'));
}

function Failureˉdiagnostic(Failure) {
    const Pending = [{ Value: Failure, Label: '' }];
    const Seen = new Set();
    const Parts = [];
    let Remaining = MAXIMUM_DIAGNOSTIC_BYTES;
    while (Pending.length !== 0 && Seen.size < 32 && Remaining > 0) {
        const { Value, Label } = Pending.shift();
        if (Seen.has(Value)) continue;
        Seen.add(Value);
        const Message = Label + (Value instanceof Error ? Value.message : String(Value));
        const Detail = Failureˉdetails(Value);
        const Part = (Message.slice(0, Remaining) + Detail).slice(0, Remaining);
        Parts.push(Part);
        Remaining -= Part.length + 1;
        if (Value instanceof AggregateError) {
            for (const Child of Value.errors.slice(0, 8)) Pending.push({ Value: Child, Label: 'Cause: ' });
        }
        if (Value !== null && typeof Value === 'object' && Value.cleanupFailure !== undefined) {
            Pending.push({ Value: Value.cleanupFailure, Label: 'Cleanup: ' });
        }
        if (Value !== null && typeof Value === 'object' && Value.cause !== undefined) {
            Pending.push({ Value: Value.cause, Label: 'Cause: ' });
        }
    }
    // Bytes, not JS character count, are the external diagnostic boundary.
    return Buffer.from(Parts.join('\n') + '\n', 'utf8').subarray(0, MAXIMUM_DIAGNOSTIC_BYTES);
}

export async function Runˉcurrentˉsplitˉprojectˉcli(Action = Buildˉcurrentˉsplitˉprojects) {
    try { await Action(); }
    catch (Failure) {
        process.stderr.write(Failureˉdiagnostic(Failure));
        process.exitCode = Failureˉcode(Failure);
    }
}

if (process.argv[1] !== undefined &&
    Sameˉpath(path.resolve(process.argv[1]), fileURLToPath(import.meta.url))) {
    await Runˉcurrentˉsplitˉprojectˉcli();
}
