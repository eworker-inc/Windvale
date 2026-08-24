import { spawnSync } from 'node:child_process';
import {
    lstatSync,
    readFileSync,
    realpathSync,
    unlinkSync,
} from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const MAXIMUM_INPUT_BYTES = 16_777_216;
const MAXIMUM_TOOL_BYTES = 134_217_728;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;
const TOOL_TIMEOUT_MILLISECONDS = 300_000;
const SOURCE_LOCK_SHA256 =
    '9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e';
const INVALID_WIR =
    'source emission status=Invalidˉanalysis analysis-status=Invalidˉwir ' +
    'wvb-status=Sourceˉwir function=0 operation=0 source-line=0\n';

if (process.argv.length !== 6) {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Language-1.0-Owned-Vector-Calls-Wir.mjs ' +
        '<admitter> <analyzer> <emitter> <work-directory>\n',
    );
    process.exit(64);
}

const Admitter = path.resolve(process.argv[2]);
const Analyzer = path.resolve(process.argv[3]);
const Emitter = path.resolve(process.argv[4]);
const Work = path.resolve(process.argv[5]);
const Scriptˉdirectory = path.dirname(fileURLToPath(import.meta.url));
const Repositoryˉroot = realpathSync(path.resolve(Scriptˉdirectory, '..', '..'));
const Profileˉroot = path.join(
    Repositoryˉroot,
    'Documents', 'Project', 'Language-1.0-Localization-Workloads',
    '01-Source-Profile-Admission', 'Reference-Artifacts',
);
const Sourceˉlock = path.join(Profileˉroot, 'Source-Inputs.wvlock');
const Sourceˉprofile = path.join(Profileˉroot, 'En-Source-Profile.wvsp');
const Fixtureˉroot = path.join(
    Repositoryˉroot, 'Tests', 'Fixtures', 'Language-1.0',
);
const Dependencies = [
    path.join(
        Repositoryˉroot, 'Libraries', 'Foundation', 'Collections',
        'Collections.wv',
    ),
    path.join(
        Repositoryˉroot, 'Libraries', 'Foundation', 'Memory', 'Memory.wv',
    ),
    path.join(
        Repositoryˉroot, 'Libraries', 'Foundation', 'Values', 'Result.wv',
    ),
];

Requireˉordinaryˉfile(Admitter, MAXIMUM_TOOL_BYTES, 'admitter');
Requireˉordinaryˉfile(Analyzer, MAXIMUM_TOOL_BYTES, 'analyzer');
Requireˉordinaryˉfile(Emitter, MAXIMUM_TOOL_BYTES, 'emitter');
Requireˉordinaryˉfile(Sourceˉlock, MAXIMUM_INPUT_BYTES, 'source lock');
Requireˉordinaryˉfile(Sourceˉprofile, MAXIMUM_INPUT_BYTES, 'source profile');
Requireˉordinaryˉdirectory(Work, 'work directory');
for (const Dependency of Dependencies) {
    Requireˉordinaryˉfile(Dependency, MAXIMUM_INPUT_BYTES, 'source dependency');
}

const Cases = [
    {
        name: 'owned-vector-calls-and-joins',
        fixture: 'Owned-Vector-Calls-And-Joins-Wir.wv',
        valid: true,
    },
    {
        name: 'owned-vector-call-use-after',
        fixture: 'Owned-Vector-Call-Use-After.wv',
        valid: false,
    },
    {
        name: 'owned-vector-call-duplicate',
        fixture: 'Owned-Vector-Call-Duplicate.wv',
        valid: false,
    },
    {
        name: 'owned-vector-call-asymmetric-join',
        fixture: 'Owned-Vector-Call-Asymmetric-Join.wv',
        valid: false,
    },
];

const Created = [];
try {
    let Positiveˉwvirˉbytes = 0;
    let Positiveˉcalls = 0;
    for (const Case of Cases) {
        const Fixture = path.join(Fixtureˉroot, Case.fixture);
        const Prefix = path.join(Work, Case.name);
        const Source = `${Prefix}.wvss`;
        const Manifest = `${Prefix}.wvca`;
        const Bindings = `${Prefix}.wvlb`;
        const Wir = `${Prefix}.wvir`;
        const Product = `${Prefix}.wvb`;
        Created.push(Source, Manifest, Bindings, Wir, Product);
        Requireˉordinaryˉfile(Fixture, MAXIMUM_INPUT_BYTES, Case.fixture);

        Requireˉsuccess(
            Run(Admitter, [
                '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
                '--source-profile', Sourceˉprofile,
                Fixture, ...Dependencies, Source,
            ]),
            'source admission status=Published ',
            `${Case.name} admission`,
        );
        const Analysis = Run(Analyzer, [
            '--admitted-source-set', Source,
            Source, Manifest, Bindings, Wir,
        ]);
        Requireˉsuccess(
            Analysis, 'source analysis status=Published ',
            `${Case.name} analysis`,
        );
        Requireˉordinaryˉfile(Source, MAXIMUM_INPUT_BYTES, 'source set');
        Requireˉordinaryˉfile(
            Manifest, MAXIMUM_INPUT_BYTES, 'analysis manifest',
        );
        Requireˉordinaryˉfile(Bindings, MAXIMUM_INPUT_BYTES, 'bindings');
        Requireˉordinaryˉfile(Wir, MAXIMUM_INPUT_BYTES, 'owned-call WVIR');
        if (!Case.valid) {
            Requireˉrejection(
                Run(Emitter, [Source, Manifest, Bindings, Wir, Product]),
                INVALID_WIR, `${Case.name} WVB boundary`, Product,
            );
            continue;
        }
        const Evidence = Inspectˉownedˉcallˉwvir(readFileSync(Wir));
        Positiveˉwvirˉbytes = Evidence.bytes;
        Positiveˉcalls = Evidence.calls;
        Requireˉownedˉcallˉboundary(
            Run(Emitter, [Source, Manifest, Bindings, Wir, Product]), Product,
        );
    }
    process.stdout.write(
        'language 1 owned Vector calls and joins WVIR status=Passed ' +
        `cases=${Cases.length} calls=${Positiveˉcalls} ` +
        `wvir-bytes=${Positiveˉwvirˉbytes}\n`,
    );
} finally {
    for (const Candidate of Created) {
        if (path.dirname(Candidate) !== Work) {
            Reject(`Refusing to remove unexpected verifier path: ${Candidate}.`);
        }
        try { unlinkSync(Candidate); } catch (Error) {
            if (Error?.code !== 'ENOENT') throw Error;
        }
    }
}

function Inspectˉownedˉcallˉwvir(Input) {
    if (Input.length < 48 || Input.length > MAXIMUM_INPUT_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVIR' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 5) {
        Reject('The owned-call fixture is not bounded WVIR 1.5.');
    }
    const Functions = Input.readUInt32LE(8);
    const Functionˉbytes = Input.readUInt32LE(12);
    const Blocks = Input.readUInt32LE(16);
    const Blockˉbytes = Input.readUInt32LE(20);
    const Operations = Input.readUInt32LE(24);
    const Operationˉbytes = Input.readUInt32LE(28);
    const Temporaries = Input.readUInt32LE(32);
    const Temporaryˉbytes = Input.readUInt32LE(36);
    const Operands = Input.readUInt32LE(40);
    const Operandˉbytes = Input.readUInt32LE(44);
    if (Functions < 7 || Functions > 64 || Functionˉbytes !== 48 ||
        Blocks < 9 || Blocks > 128 || Blockˉbytes !== 28 ||
        Operations < 20 || Operations > 512 || Operationˉbytes !== 32 ||
        Temporaries < 10 || Temporaries > 512 || Temporaryˉbytes !== 4 ||
        Operands < 5 || Operands > 512 || Operandˉbytes !== 4) {
        Reject('The bounded owned-call WVIR directory shape differs.');
    }
    const Blocksˉoffset = 48 + Functions * Functionˉbytes;
    const Operationsˉoffset = Blocksˉoffset + Blocks * Blockˉbytes;
    const Temporariesˉoffset = Operationsˉoffset + Operations * Operationˉbytes;
    const Operandsˉoffset = Temporariesˉoffset + Temporaries * Temporaryˉbytes;
    if (Operandsˉoffset + Operands * Operandˉbytes !== Input.length) {
        Reject('The owned-call WVIR directory length is inconsistent.');
    }
    let Calls = 0;
    let Branches = 0;
    for (let Index = 0; Index < Operations; Index += 1) {
        const Entry = Operationsˉoffset + Index * Operationˉbytes;
        if (Input.readUInt32LE(Entry + 4) === 62) Calls += 1;
    }
    for (let Index = 0; Index < Blocks; Index += 1) {
        const Entry = Blocksˉoffset + Index * Blockˉbytes;
        if (Input.readUInt32LE(Entry + 12) === 2) Branches += 1;
    }
    if (Calls !== 6 || Branches !== 2) {
        Reject(
            `The owned-call WVIR evidence differs: calls=${Calls} ` +
            `branches=${Branches}.`,
        );
    }
    return { bytes: Input.length, calls: Calls };
}

function Requireˉownedˉcallˉboundary(Result, Product) {
    const Diagnostic = Normalize(Result.stderr);
    if (Result.error !== undefined || Result.status !== 1 ||
        Result.stdout.length !== 0 || Exists(Product) ||
        !/^source emission status=Valid analysis-status=Valid wvb-status=Unsupportedˉshape function=\d+ operation=\d+ source-line=0\n$/.test(
            Diagnostic,
        )) {
        Reject(
            `The owned-call WVB boundary differs: status=${Result.status} ` +
            `stdout=${JSON.stringify(Result.stdout)} ` +
            `stderr=${JSON.stringify(Diagnostic)}.`,
        );
    }
}

function Requireˉsuccess(Result, Prefix, Label) {
    const Output = Normalize(Result.stdout);
    if (Result.error !== undefined || Result.status !== 0 ||
        Result.stderr.length !== 0 || !Output.startsWith(Prefix)) {
        Reject(
            `The ${Label} failed: status=${Result.status} ` +
            `stdout=${JSON.stringify(Output)} ` +
            `stderr=${JSON.stringify(Normalize(Result.stderr))}.`,
        );
    }
}

function Requireˉrejection(Result, Expected, Label, Product) {
    if (Result.error !== undefined || Result.status !== 1 ||
        Result.stdout.length !== 0 || Normalize(Result.stderr) !== Expected ||
        Exists(Product)) {
        Reject(
            `The ${Label} rejection differs: status=${Result.status} ` +
            `stdout=${JSON.stringify(Result.stdout)} ` +
            `stderr=${JSON.stringify(Normalize(Result.stderr))}.`,
        );
    }
}

function Run(Command, Arguments) {
    return spawnSync(Command, Arguments, {
        encoding: 'utf8',
        windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
        timeout: TOOL_TIMEOUT_MILLISECONDS,
    });
}

function Exists(Candidate) {
    try { lstatSync(Candidate); return true; } catch (Error) {
        if (Error?.code === 'ENOENT') return false;
        throw Error;
    }
}

function Normalize(Value) {
    return Value.replaceAll('\r\n', '\n');
}

function Requireˉordinaryˉfile(Candidate, Maximum, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isFile() || Information.isSymbolicLink() ||
        Information.size < 1 || Information.size > Maximum ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not a bounded ordinary file: ${Candidate}.`);
    }
}

function Requireˉordinaryˉdirectory(Candidate, Label) {
    const Information = lstatSync(Candidate);
    if (!Information.isDirectory() || Information.isSymbolicLink() ||
        !Sameˉpath(realpathSync(Candidate), Candidate)) {
        Reject(`The ${Label} is not an ordinary directory: ${Candidate}.`);
    }
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Reject(Message) {
    throw new Error(Message);
}
