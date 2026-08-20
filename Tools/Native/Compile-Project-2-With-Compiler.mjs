import { spawn } from 'node:child_process';
import { lstat, readFile, realpath, rm } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, '..', '..');
const MAXIMUM_PROJECT_BYTES = 65_536;
const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const PRODUCER_TIMEOUT_MILLISECONDS = 300_000;

if (process.argv.length !== 5) {
    Reject('Usage: node Compile-Project-2-With-Compiler.mjs <compiler> <project.wvproj> <output.wvb>');
}

const Compiler = path.resolve(process.argv[2]);
const Project = path.resolve(process.argv[3]);
const Output = path.resolve(process.argv[4]);
await Requireˉordinaryˉfile(Compiler, 134_217_728, 'compiler');
const Projectˉbytes = await Requireˉordinaryˉfile(
    Project, MAXIMUM_PROJECT_BYTES, 'Project 2 manifest'
);
if (path.extname(Project).toLowerCase() !== '.wvproj' ||
    path.extname(Output).toLowerCase() !== '.wvb') {
    Reject('The compiler project or output extension is invalid.');
}
const Inputs = Parseˉproject(Projectˉbytes.toString('utf8'), Project);
const Outputˉparent = path.dirname(Output);
await Requireˉordinaryˉdirectory(Outputˉparent, 'output parent');
if (await Exists(Output) || Sameˉpath(Output, Compiler) ||
    Sameˉpath(Output, Project) || Inputs.some(Input => Sameˉpath(Output, Input))) {
    Reject('The compiler output must be a new path distinct from every input.');
}
let Sourceˉbytes = 0;
for (const Input of Inputs) {
    const Bytes = await Requireˉordinaryˉfile(
        Input, 4_194_304, `project source ${path.basename(Input)}`
    );
    Sourceˉbytes += Bytes.length;
    if (Sourceˉbytes > 4_194_304) {
        Reject('The aggregate Project 2 source closure exceeds 4 MiB.');
    }
}
console.log(
    `project compiler status=Started project=${path.basename(Project)} ` +
    `modules=${Inputs.length}`,
);
let Result;
try {
    Result = await Runˉbounded(Compiler, [...Inputs, Output]);
} catch (Error) {
    await rm(Output, { force: true });
    throw Error;
}
if (Result.stdout.length !== 0) {
    process.stdout.write(Result.stdout);
}
if (Result.status !== 0) {
    await rm(Output, { force: true });
    if (Result.stderr.length !== 0) {
        process.stderr.write(Result.stderr);
    }
    process.exitCode = Result.status === null ? 1 : Result.status;
} else {
    if (Result.stderr.length !== 0) {
        await rm(Output, { force: true });
        Reject('The compiler wrote diagnostics after success.');
    }
    let Product;
    try {
        Product = await Requireˉordinaryˉfile(
            Output, MAXIMUM_WVB_BYTES, 'compiled WVB'
        );
    } catch (Error) {
        await rm(Output, { force: true });
        throw Error;
    }
    console.log(`project compiler status=Published wvb-bytes=${Product.length}`);
}

function Parseˉproject(Text, Manifest) {
    const Lines = Text.split(/\r?\n/u);
    if (Lines.at(-1) === '') {
        Lines.pop();
    }
    if (Lines.length < 3 || Lines[0] !== 'windvale-project 2' ||
        Lines.at(-1) !== 'emit wvb') {
        Reject('Only strict Project 2 WVB manifests are accepted.');
    }
    const Inputs = [];
    const Identities = new Set();
    let Roots = 0;
    for (const Line of Lines.slice(1, -1)) {
        const Match = /^(root|source) "([^"\r\n]+)"$/u.exec(Line);
        if (Match === null || Match[2].includes('\\') ||
            path.posix.isAbsolute(Match[2]) ||
            Match[2].split('/').some(Part =>
                Part === '' || Part === '.' || Part === '..')) {
            Reject(`The project source is invalid: ${Line}`);
        }
        if (Match[1] === 'root') {
            Roots += 1;
            if (Inputs.length !== 0) {
                Reject('The Project 2 root must be first.');
            }
        }
        const Input = path.join(REPOSITORY_ROOT, ...Match[2].split('/'));
        const Identity = process.platform === 'win32'
            ? Input.toLowerCase()
            : Input;
        if (Identities.has(Identity)) {
            Reject(`The project source is duplicated: ${Match[2]}`);
        }
        Identities.add(Identity);
        Inputs.push(Input);
    }
    if (Roots !== 1 || Inputs.length < 1 || Inputs.length > 64) {
        Reject(`The project source count is invalid: ${Manifest}`);
    }
    return Inputs;
}

async function Runˉbounded(Command, Arguments) {
    return await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: REPOSITORY_ROOT,
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        let Stdout = Buffer.alloc(0);
        let Stderr = Buffer.alloc(0);
        let Settled = false;
        let Progress = null;
        let Timeout = null;
        const Finish = Result => {
            if (Settled) return;
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            Resolve(Result);
        };
        const Fail = Error => {
            if (Settled) return;
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            Rejectˉpromise(Error);
        };
        const Append = (Current, Chunk) => {
            if (Current.length + Chunk.length > MAXIMUM_DIAGNOSTIC_BYTES) {
                Child.kill();
                Fail(new Error('Compiler diagnostics exceed 64 KiB.'));
                return Current;
            }
            return Buffer.concat([Current, Chunk]);
        };
        Child.stdout.on('data', Chunk => { Stdout = Append(Stdout, Chunk); });
        Child.stderr.on('data', Chunk => { Stderr = Append(Stderr, Chunk); });
        const Started = Date.now();
        Progress = setInterval(() => {
            console.log(
                `project compiler status=Active elapsed-seconds=${Math.floor((Date.now() - Started) / 1_000)}`,
            );
        }, 30_000);
        Timeout = setTimeout(() => {
            Child.kill();
            Finish({ status: 1, stdout: Stdout, stderr: Buffer.from('Compiler timeout.\n') });
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        Child.on('error', Fail);
        Child.on('close', Status => Finish({ status: Status, stdout: Stdout, stderr: Stderr }));
    });
}

async function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, Candidate)) {
        Reject(`The ${Label} path is not canonical: ${Candidate}`);
    }
    return await readFile(Candidate);
}

async function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isDirectory() ||
        Information.isSymbolicLink()) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, Candidate)) {
        Reject(`The ${Label} path is not canonical: ${Candidate}`);
    }
}

async function Exists(Candidate) {
    return await lstat(Candidate).then(() => true, () => false);
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Reject(Message) {
    throw new Error(Message);
}
