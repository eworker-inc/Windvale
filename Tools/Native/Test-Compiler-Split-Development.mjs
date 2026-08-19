import { createHash } from 'node:crypto';
import { spawn } from 'node:child_process';
import { createReadStream } from 'node:fs';
import {
    lstat,
    mkdtemp,
    readFile,
    realpath,
    rm,
} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, '..', '..');
const HOST = `${process.platform}-${process.arch}`;
const TEMPORARY_PREFIX = 'windvale-compiler-split-development-';
const MAXIMUM_OUTPUT_BYTES = 4_194_304;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const PRODUCER_TIMEOUT_MILLISECONDS = 300_000;
const COMPILERS = {
    'win32-x64': {
        path: 'Artifacts/Native-Compiler-Reconstruction-Candidate/windows-x64/wvcompiler.exe',
        bytes: 28_172_800,
        sha256: 'a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d',
    },
    'linux-x64': {
        path: 'Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvcompiler.elf',
        bytes: 28_172_288,
        sha256: 'da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b',
    },
};
const CASES = [
    {
        name: 'source-analysis',
        project: 'Projects/Tests/Language-1.0-Source-Analysis-Self-Test.wvproj',
        bytes: 956_883,
        sha256: 'ab01da6e1085381b9025f76c2035a61f7f0cbbffcba52e73b9dd24a42c035577',
    },
    {
        name: 'analyzer',
        project: 'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
        bytes: 949_355,
        sha256: 'bd8541fc51d87e12265055786df656048510102ced86c6672cabe6ba45bb27cb',
    },
    {
        name: 'emitter',
        project: 'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj',
        bytes: 746_557,
        sha256: '9a1806db232364882b3fc513692cd7d7ed8568363ae5c85cfa5e9b5521dc0557',
    },
];

if (!(HOST in COMPILERS)) {
    Reject(`The compiler split development test does not support ${HOST}.`);
}
const Compiler = path.join(REPOSITORY_ROOT, COMPILERS[HOST].path);
const Compilerˉevidence = await Fileˉevidence(
    Compiler,
    134_217_728,
    'native source compiler',
);
if (Compilerˉevidence.bytes !== COMPILERS[HOST].bytes ||
    Compilerˉevidence.sha256 !== COMPILERS[HOST].sha256) {
    Reject('The native source compiler identity is invalid.');
}

const Testˉroot = await mkdtemp(path.join(os.tmpdir(), TEMPORARY_PREFIX));
try {
    console.log(`compiler split development status=Started cases=4 host=${HOST}`);
    for (let Index = 0; Index < CASES.length; Index += 1) {
        const Case = CASES[Index];
        console.log(
            `compiler split development step=compile item=${Index + 1}/4 ` +
            `product=${Case.name}`,
        );
        const Inputs = await Readˉproject(Case.project);
        const Output = path.join(Testˉroot, `${Case.name}.wvb`);
        await Runˉbounded(
            Compiler,
            [...Inputs, Output],
            `compile-${Case.name}`,
        );
        const Evidence = await Fileˉevidence(
            Output,
            MAXIMUM_OUTPUT_BYTES,
            `${Case.name} WVB`,
        );
        if (Evidence.bytes !== Case.bytes || Evidence.sha256 !== Case.sha256) {
            Reject(`The ${Case.name} reachable WVB identity differs.`);
        }
        console.log(
            `compiler split development step=compile status=Passed ` +
            `product=${Case.name} wvb-bytes=${Evidence.bytes}`,
        );
    }

    console.log('compiler split development step=cache-cleanup item=4/4');
    await Runˉbounded(
        process.execPath,
        [path.join(SCRIPT_DIRECTORY, 'Test-Cached-Split-Project-Wvb.mjs')],
        'cache-cleanup',
    );
    console.log(
        'compiler split development status=Passed cases=4 ' +
        'source-analysis-wvb-bytes=956883 analyzer-wvb-bytes=949355 ' +
        'emitter-wvb-bytes=746557 cleanup=Verified',
    );
} finally {
    const Resolved = path.resolve(Testˉroot);
    if (path.dirname(Resolved) !== path.resolve(os.tmpdir()) ||
        !path.basename(Resolved).startsWith(TEMPORARY_PREFIX)) {
        Reject('Refusing to remove an unexpected compiler split test directory.');
    }
    await rm(Resolved, { recursive: true, force: true });
}

async function Readˉproject(Relative) {
    const Candidate = path.join(REPOSITORY_ROOT, Relative);
    const Bytes = await readFile(Candidate);
    if (Bytes.length < 1 || Bytes.length > 16_384) {
        Reject(`The focused project is not bounded: ${Relative}`);
    }
    const Lines = Bytes.toString('utf8').split(/\r?\n/u).filter(Line => Line !== '');
    if (Lines[0] !== 'windvale-project 2' ||
        Lines[Lines.length - 1] !== 'emit wvb') {
        Reject(`The focused project contract is invalid: ${Relative}`);
    }
    const Inputs = [];
    let Roots = 0;
    for (const Line of Lines.slice(1, -1)) {
        const Match = /^(root|source) "([^"\r\n]+)"$/u.exec(Line);
        if (Match === null || Match[2].includes('\\') ||
            path.posix.isAbsolute(Match[2]) ||
            Match[2].split('/').some(Part =>
                Part === '' || Part === '.' || Part === '..')) {
            Reject(`The focused project source is invalid: ${Relative}`);
        }
        if (Match[1] === 'root') {
            Roots += 1;
            if (Inputs.length !== 0) {
                Reject(`The focused project root is not first: ${Relative}`);
            }
        }
        const Input = path.join(REPOSITORY_ROOT, ...Match[2].split('/'));
        const Canonical = await realpath(Input).catch(() => '');
        if (!Sameˉpath(Canonical, Input)) {
            Reject(`The focused project source is not canonical: ${Match[2]}`);
        }
        Inputs.push(Input);
    }
    if (Roots !== 1 || Inputs.length < 1 || Inputs.length > 64) {
        Reject(`The focused project source count is invalid: ${Relative}`);
    }
    return Inputs;
}

async function Runˉbounded(Command, Arguments, Step) {
    await new Promise((Resolve, Rejectˉpromise) => {
        const Child = spawn(Command, Arguments, {
            cwd: REPOSITORY_ROOT,
            windowsHide: true,
            stdio: ['ignore', 'pipe', 'pipe'],
        });
        const Started = Date.now();
        let Output = Buffer.alloc(0);
        let Errorˉoutput = Buffer.alloc(0);
        let Settled = false;
        const Finish = (Error) => {
            if (Settled) {
                return;
            }
            Settled = true;
            clearInterval(Progress);
            clearTimeout(Timeout);
            if (Error === null) {
                Resolve();
            } else {
                Rejectˉpromise(Error);
            }
        };
        const Append = (Current, Chunk) => {
            if (Current.length + Chunk.length > MAXIMUM_DIAGNOSTIC_BYTES) {
                Child.kill();
                Finish(new Error(`The ${Step} diagnostics exceed 64 KiB.`));
                return Current;
            }
            return Buffer.concat([Current, Chunk]);
        };
        Child.stdout.on('data', Chunk => { Output = Append(Output, Chunk); });
        Child.stderr.on('data', Chunk => {
            Errorˉoutput = Append(Errorˉoutput, Chunk);
        });
        const Progress = setInterval(() => {
            const Seconds = Math.floor((Date.now() - Started) / 1_000);
            console.log(
                `compiler split development step=${Step} status=Active ` +
                `elapsed-seconds=${Seconds}`,
            );
        }, 30_000);
        const Timeout = setTimeout(() => {
            Child.kill();
            Finish(new Error(`The ${Step} producer exceeded five minutes.`));
        }, PRODUCER_TIMEOUT_MILLISECONDS);
        Child.on('error', Error => {
            Finish(Error);
        });
        Child.on('close', Status => {
            if (Settled) {
                return;
            }
            if (Status !== 0) {
                Finish(new Error(
                    `The ${Step} producer exited ${Status}: ` +
                    Errorˉoutput.toString('utf8').trim(),
                ));
                return;
            }
            if (Errorˉoutput.length !== 0) {
                Finish(new Error(
                    `The ${Step} producer wrote diagnostics after success.`,
                ));
                return;
            }
            if (Output.length === 0) {
                Finish(new Error(`The ${Step} producer was silent.`));
                return;
            }
            Finish(null);
        });
    });
}

async function Fileˉevidence(Candidate, Maximum, Label) {
    const Information = await lstat(Candidate).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > Maximum) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}`);
    }
    const Canonical = await realpath(Candidate);
    if (!Sameˉpath(Canonical, path.resolve(Candidate))) {
        Reject(`The ${Label} must use its canonical non-link path: ${Candidate}`);
    }
    const Hash = createHash('sha256');
    let Measured = 0;
    for await (const Chunk of createReadStream(Candidate, {
        highWaterMark: 1_048_576,
    })) {
        Measured += Chunk.length;
        if (Measured > Information.size) {
            Reject(`The ${Label} grew while it was hashed.`);
        }
        Hash.update(Chunk);
    }
    if (Measured !== Information.size) {
        Reject(`The ${Label} changed while it was hashed.`);
    }
    return { bytes: Measured, sha256: Hash.digest('hex') };
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === Right.toLowerCase()
        : Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}
