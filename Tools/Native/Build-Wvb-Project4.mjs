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
const MAXIMUM_PROJECT_BYTES = 65_536;
const MAXIMUM_PRODUCT_BYTES = 134_217_728;
const MAXIMUM_WVB_BYTES = 16_777_216;
const PRODUCT_TIMEOUT_MILLISECONDS = 600_000;
const PACKAGE_TIMEOUT_MILLISECONDS = 1_200_000;
const BUILD_TIMEOUT_MILLISECONDS = 600_000;
const WINDOWS = process.platform === 'win32';
const HOST_APPLICATION_EXTENSION = WINDOWS ? '.exe' : '.elf';

if (process.argv.length !== 4) Usage();

const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Workspace = path.join(Repositoryˉroot, 'Windvale.wvws');
const Project = path.resolve(process.argv[2]);
const Output = path.resolve(process.argv[3]);
let Step = 0;
let Totalˉsteps = 21;
let Work = '';

try {
    await Main();
} catch (Error) {
    process.stderr.write(`${Error.message}\n`);
    process.exit(Error.exitCode ?? 1);
}

async function Main() {
    if (process.arch !== 'x64' ||
        (process.platform !== 'win32' && process.platform !== 'linux')) {
        Reject(`Unsupported Project 4 build host: ${process.platform}-${process.arch}.`);
    }
    Requireˉordinaryˉfile(Workspace, MAXIMUM_PROJECT_BYTES, 'workspace marker');
    Requireˉordinaryˉfile(Project, MAXIMUM_PROJECT_BYTES, 'project manifest');
    Requireˉproject4ˉmanifest(Project);
    Requireˉordinaryˉdirectory(path.dirname(Output), 'output parent');
    if (path.extname(Project).toLowerCase() !== '.wvproj' ||
        path.extname(Output).toLowerCase() !== '.wvb') {
        Usage();
    }
    if (Exists(Output)) {
        Reject('The Project 4 build output must be a new .wvb path.');
    }

    const Temporaryˉroot = Canonicalˉordinaryˉdirectory(os.tmpdir(), 'temporary root');
    Work = mkdtempSync(path.join(Temporaryˉroot, 'windvale-project4-build-'));

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
            Totalˉsteps = 9;
        }
        process.stdout.write(
            `project4 build compiler-cache status=${Compilerˉcheckpoint.status} ` +
            `key=${Compilerˉkey}\n`,
        );
        const Analyzer = path.join(
            Compilerˉcheckpoint.directory,
            `Analyzer${HOST_APPLICATION_EXTENSION}`,
        );
        const Analyzerˉidentity = path.join(Compilerˉcheckpoint.directory, 'Analyzer.identity');
        const Emitter = path.join(
            Compilerˉcheckpoint.directory,
            `Emitter${HOST_APPLICATION_EXTENSION}`,
        );
        const Emitterˉidentity = path.join(Compilerˉcheckpoint.directory, 'Emitter.identity');
        for (const [Candidate, Label] of [
            [Analyzer, 'current analyzer'],
            [Analyzerˉidentity, 'current analyzer identity'],
            [Emitter, 'current emitter'],
            [Emitterˉidentity, 'current emitter identity'],
        ]) {
            Requireˉordinaryˉfile(Candidate, MAXIMUM_PRODUCT_BYTES, Label);
        }

        const Products = {};
        for (const Product of [
            {
                name: 'wvproject',
                project: 'Windvale-Project-Manifest.wvproj',
                role: 'Project 4 manifest reader',
            },
            {
                name: 'wvadmit',
                project: 'Windvale-Compiler-Admission-Driver.wvproj',
                role: 'source admission',
            },
            {
                name: 'wvauth',
                project: 'Windvale-Compiler-Source-Authenticator.wvproj',
                role: 'source authentication',
            },
            {
                name: 'wvbind',
                project: 'Windvale-Compiler-Foreign-Binding-Driver.wvproj',
                role: 'foreign binding',
            },
        ]) {
            Products[Product.name] = await Acquireˉproduct(
                Product.name,
                path.join(Repositoryˉroot, 'Projects', 'Tools', Product.project),
                Product.role,
                Analyzer,
                Analyzerˉidentity,
                Emitter,
                Emitterˉidentity,
            );
        }

        await Runˉnode('project4-authenticated-build', 'Run-Split-Compiler.mjs', [
            Products.wvadmit,
            Products.wvauth,
            Analyzer,
            Emitter,
            '--foreign-binder',
            Products.wvbind,
            '--workspace',
            Workspace,
            '--project',
            Project,
            '--manifest-reader',
            Products.wvproject,
            Output,
        ], BUILD_TIMEOUT_MILLISECONDS);
        const Evidence = Fileˉevidence(
            Output, 'Project 4 build output', MAXIMUM_WVB_BYTES
        );
        process.stdout.write(
            `project4 build status=Published wvb-bytes=${Evidence.bytes} ` +
            `wvb-sha256=${Evidence.sha256}\n`,
        );
    } finally {
        const Resolved = path.resolve(Work);
        if (path.dirname(Resolved) !== Temporaryˉroot ||
            !path.basename(Resolved).startsWith('windvale-project4-build-')) {
            Reject(`Refusing to remove unexpected temporary directory: ${Resolved}.`);
        }
        rmSync(Resolved, { recursive: true, force: true, maxRetries: 2 });
    }
}

async function Acquireˉproduct(
    Name,
    Projectˉpath,
    Role,
    Analyzer,
    Analyzerˉidentity,
    Emitter,
    Emitterˉidentity,
) {
    Requireˉordinaryˉfile(Projectˉpath, MAXIMUM_PROJECT_BYTES, `${Role} project`);
    const Wvb = path.join(Work, `${Name}.wvb`);
    const Application = path.join(Work, `${Name}${HOST_APPLICATION_EXTENSION}`);
    await Runˉnode(`project4-${Name}-wvb`, 'Build-Cached-Split-Project-Wvb.mjs', [
        Projectˉpath,
        Wvb,
        Analyzer,
        Analyzerˉidentity,
        Emitter,
        Emitterˉidentity,
        '--symbol-checkpoint',
    ], PRODUCT_TIMEOUT_MILLISECONDS);
    await Runˉnode(`project4-${Name}-package`, 'Build-Cached-Segmented-Hosted-Wvb.mjs', [
        '7',
        Wvb,
        Application,
    ], PACKAGE_TIMEOUT_MILLISECONDS);
    Requireˉordinaryˉfile(Application, MAXIMUM_PRODUCT_BYTES, Role);
    return Application;
}

async function Runˉnative(Label, Name, Arguments, Timeout = PRODUCT_TIMEOUT_MILLISECONDS) {
    const Extension = WINDOWS ? '.cmd' : '.sh';
    const Script = path.join(Scriptˉdirectory, `${Name}${Extension}`);
    Requireˉordinaryˉfile(Script, MAXIMUM_PRODUCT_BYTES, `${Name} script`);
    if (WINDOWS) {
        await Run(Label, Script, Arguments, Timeout);
        return;
    }
    await Run(Label, 'bash', [Script, ...Arguments], Timeout);
}

async function Runˉnode(Label, Name, Arguments, Timeout = PRODUCT_TIMEOUT_MILLISECONDS) {
    await Run(
        Label,
        process.execPath,
        [path.join(Scriptˉdirectory, Name), ...Arguments],
        Timeout,
    );
}

async function Run(Label, Command, Arguments, Timeout) {
    const Currentˉstep = ++Step;
    const Started = Date.now();
    process.stdout.write(
        `START project4 build step=${Currentˉstep}/${Totalˉsteps} phase=${Label}\n`,
    );
    const Result = await Runˉdevelopmentˉcommand(
        Command,
        Arguments,
        Started + Timeout,
        true,
        MAXIMUM_DIAGNOSTIC_BYTES,
    );
    if (Result.Code !== 0 || Result.Error !== '') {
        if (Result.Error !== '') process.stderr.write(Result.Error);
        Reject(`${Label} failed: status=${Result.Code}.`);
    }
    process.stdout.write(
        `PASS  project4 build step=${Currentˉstep}/${Totalˉsteps} phase=${Label} ` +
        `elapsed-ms=${Date.now() - Started}\n`,
    );
}

function Requireˉproject4ˉmanifest(Candidate) {
    const Bytes = readFileSync(Candidate);
    if (Bytes.length < 'windvale-project 4'.length ||
        Bytes.length > MAXIMUM_PROJECT_BYTES ||
        Bytes.includes(0)) {
        Reject('The Project 4 manifest has invalid text bytes.');
    }
    const Firstˉlineˉend = Bytes.indexOf(10);
    const Header = Bytes.subarray(
        0,
        Firstˉlineˉend === -1 ? Bytes.length : Firstˉlineˉend,
    ).toString('utf8').replace(/\r$/u, '');
    if (Header !== 'windvale-project 4') {
        Reject('The Project 4 build helper requires windvale-project 4.');
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

function Exists(Candidate) {
    return lstatSync(Candidate, { throwIfNoEntry: false }) !== undefined;
}

function Sameˉpath(Left, Right) {
    return WINDOWS ? Left.toLowerCase() === Right.toLowerCase() : Left === Right;
}

function Usage() {
    process.stderr.write(
        'Usage: node Tools/Native/Build-Wvb-Project4.mjs ' +
        '<project.wvproj> <new-output.wvb>\n',
    );
    process.exit(64);
}

function Reject(Message) {
    throw new Error(Message);
}
