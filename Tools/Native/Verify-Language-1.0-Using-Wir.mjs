import { spawnSync } from 'node:child_process';
import {
    lstatSync,
    readFileSync,
    realpathSync,
    unlinkSync,
    writeFileSync,
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
const INVALID_RESOURCE =
    'source analysis status=Sourceˉwir symbol-status=Valid ' +
    'binding-status=Valid wir-status=Invalidˉresource ' +
    'failure-module=0 related-module=0 function=0 offset=291 ' +
    'line=12 column=20\n';

if (process.argv.length !== 8) {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Language-1.0-Using-Wir.mjs ' +
        '<admitter> <validator> <analyzer> <emitter> <target.wvtd> ' +
        '<work-directory>\n',
    );
    process.exit(64);
}

const Admitter = path.resolve(process.argv[2]);
const Validator = path.resolve(process.argv[3]);
const Analyzer = path.resolve(process.argv[4]);
const Emitter = path.resolve(process.argv[5]);
const Target = path.resolve(process.argv[6]);
const Work = path.resolve(process.argv[7]);
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
const Cases = [
    {
        name: 'using-vector-fallthrough',
        fixture: 'Using-Vector-Fallthrough-Wir.wv',
        releases: [{ block: 1, target: 3 }],
    },
    {
        name: 'using-vector-nested-return',
        fixture: 'Using-Vector-Nested-Return-Wir.wv',
        releases: [{ block: 0, target: 3 }, { block: 0, target: 2 }],
    },
    {
        name: 'using-vector-try-propagation',
        fixture: 'Using-Vector-Try-Propagation-Wir.wv',
        releases: [{ block: 2, target: 2 }, { block: 1, target: 2 }],
    },
    {
        name: 'using-vector-loop-exits',
        fixture: 'Using-Vector-Loop-Exits-Wir.wv',
        releases: [{ block: 4, target: 1 }, { block: 5, target: 1 }],
    },
    {
        name: 'using-non-resource',
        fixture: 'Using-Non-Resource.wv',
        analysisRejection: INVALID_RESOURCE,
    },
    {
        name: 'using-vector-moved-before-release',
        fixture: 'Using-Vector-Moved-Before-Release.wv',
        emissionRejection: INVALID_WIR,
    },
];

Requireˉordinaryˉfile(Admitter, MAXIMUM_TOOL_BYTES, 'admitter');
Requireˉordinaryˉfile(Validator, MAXIMUM_TOOL_BYTES, 'validator');
Requireˉordinaryˉfile(Analyzer, MAXIMUM_TOOL_BYTES, 'analyzer');
Requireˉordinaryˉfile(Emitter, MAXIMUM_TOOL_BYTES, 'emitter');
Requireˉordinaryˉfile(Target, 320, 'target descriptor');
Requireˉordinaryˉfile(Sourceˉlock, MAXIMUM_INPUT_BYTES, 'source lock');
Requireˉordinaryˉfile(Sourceˉprofile, MAXIMUM_INPUT_BYTES, 'source profile');
Requireˉordinaryˉdirectory(Work, 'work directory');
for (const Dependency of Dependencies) {
    Requireˉordinaryˉfile(Dependency, MAXIMUM_INPUT_BYTES, 'source dependency');
}

const Created = [];
let Releases = 0;
try {
    for (const Case of Cases) {
        const Fixture = path.join(Fixtureˉroot, Case.fixture);
        const Prefix = path.join(Work, Case.name);
        const Input = `${Prefix}-input.wvss`;
        const Source = `${Prefix}.wvss`;
        const Analyzedˉsource = `${Prefix}-analysis.wvss`;
        const Admittedˉtarget = `${Prefix}.wvtd`;
        const Catalog = `${Prefix}.wvfc`;
        const Evidence = `${Prefix}.wvae`;
        const Manifest = `${Prefix}.wvca`;
        const Bindings = `${Prefix}.wvlb`;
        const Wir = `${Prefix}.wvir`;
        const Product = `${Prefix}.wvb`;
        Created.push(
            Input, Source, Analyzedˉsource, Admittedˉtarget, Catalog, Evidence,
            Manifest, Bindings, Wir, Product,
        );
        Requireˉordinaryˉfile(Fixture, MAXIMUM_INPUT_BYTES, Case.fixture);

        writeFileSync(
            Input, Constructˉsourceˉset([Fixture, ...Dependencies]),
            { flag: 'wx' },
        );

        Requireˉsuccess(
            Run(Admitter, [
                '--source-input-lock', Sourceˉlock, SOURCE_LOCK_SHA256,
                '--source-profile', Sourceˉprofile,
                '--target-descriptor', Target,
                '--source-set', Input,
                Source, Admittedˉtarget, Catalog, Evidence,
            ]),
            'source admission status=Published ',
            `${Case.name} admission`,
        );
        Requireˉsuccess(
            Run(Validator, [
                Evidence, Source, Admittedˉtarget, Catalog,
                Sourceˉlock, Sourceˉprofile,
            ]),
            'wvauth status=Accepted ',
            `${Case.name} authentication`,
        );
        const Analysis = Run(Analyzer, [
            '--internal-source-set', Source,
            Analyzedˉsource, Manifest, Bindings, Wir,
        ]);
        if (Case.analysisRejection !== undefined) {
            Requireˉrejection(
                Analysis, Case.analysisRejection,
                `${Case.name} analysis`,
                Analyzedˉsource, Manifest, Bindings, Wir,
            );
            continue;
        }
        Requireˉsuccess(
            Analysis, 'source analysis status=Published ',
            `${Case.name} analysis`,
        );
        if (!readFileSync(Analyzedˉsource).equals(readFileSync(Source))) {
            Reject(`${Case.name} analysis republished a different source set.`);
        }
        Requireˉordinaryˉfile(Wir, MAXIMUM_INPUT_BYTES, 'using WVIR');
        if (Case.emissionRejection === undefined) {
            Inspectˉreleases(readFileSync(Wir), Case.releases, Case.name);
            Releases += Case.releases.length;
        }

        const Emission = Run(Emitter, [
            Analyzedˉsource, Manifest, Bindings, Wir, Product,
        ]);
        if (Case.emissionRejection !== undefined) {
            Requireˉrejection(
                Emission, Case.emissionRejection,
                `${Case.name} WVB boundary`, Product,
            );
            continue;
        }
        Requireˉsuccess(
            Emission, 'source emission status=Published ',
            `${Case.name} emission`,
        );
        Requireˉordinaryˉfile(Product, MAXIMUM_INPUT_BYTES, 'using WVB');
    }
    process.stdout.write(
        'language 1 using semantics WVIR status=Passed ' +
        `cases=${Cases.length} valid=4 rejected=2 releases=${Releases}\n`,
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

function Constructˉsourceˉset(Paths) {
    const Sources = Paths.map(Candidate => readFileSync(Candidate));
    const Headerˉbytes = 16 + Sources.length * 8;
    const Payloadˉbytes = Sources.reduce((Total, Source) => Total + Source.length, 0);
    if (Sources.length < 1 || Sources.length > 64 ||
        Headerˉbytes + Payloadˉbytes > MAXIMUM_INPUT_BYTES) {
        Reject('The verifier source closure is outside the canonical WVSS bound.');
    }
    const Result = Buffer.alloc(Headerˉbytes + Payloadˉbytes);
    Result.write('WVSS', 0, 4, 'ascii');
    Result.writeUInt16LE(1, 4);
    Result.writeUInt32LE(Sources.length, 8);
    Result.writeUInt32LE(Sources.length * 8, 12);
    let Offset = Headerˉbytes;
    Sources.forEach((Source, Index) => {
        Result.writeUInt32LE(Offset, 16 + Index * 8);
        Result.writeUInt32LE(Source.length, 20 + Index * 8);
        Source.copy(Result, Offset);
        Offset += Source.length;
    });
    return Result;
}

function Inspectˉreleases(Input, Expected, Label) {
    if (Input.length < 48 || Input.length > MAXIMUM_INPUT_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVIR' ||
        Input.readUInt16LE(4) !== 1 ||
        Input.readUInt16LE(6) < 9 || Input.readUInt16LE(6) > 14) {
        Reject(`The ${Label} input is not bounded current WVIR.`);
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
    if (Functions < 1 || Functions > 64 || Functionˉbytes !== 48 ||
        Blocks < 1 || Blocks > 64 || Blockˉbytes !== 28 ||
        Operations < 1 || Operations > 4096 || Operationˉbytes !== 28 ||
        Temporaries > 4096 || Temporaryˉbytes !== 4 ||
        Operands > 16_384 || Operandˉbytes !== 4) {
        Reject(`The ${Label} WVIR directory shape differs.`);
    }
    const Blocksˉoffset = 48 + Functions * Functionˉbytes;
    const Operationsˉoffset = Blocksˉoffset + Blocks * Blockˉbytes;
    const Temporariesˉoffset = Operationsˉoffset +
        Operations * Operationˉbytes;
    const Operandsˉoffset = Temporariesˉoffset +
        Temporaries * Temporaryˉbytes;
    if (Operandsˉoffset + Operands * Operandˉbytes !== Input.length) {
        Reject(`The ${Label} WVIR directory length is inconsistent.`);
    }
    const Actual = [];
    for (let Index = 0; Index < Operations; Index += 1) {
        const Entry = Operationsˉoffset + Index * Operationˉbytes;
        if (Input.readUInt16LE(Entry + 4) !== 174) continue;
        if (Input.readUInt16LE(Entry + 6) !== 0 ||
            Input.readUInt32LE(Entry + 8) !== 0 ||
            Input.readUInt32LE(Entry + 12) !== 0xffffffff ||
            Input.readUInt32LE(Entry + 16) > Operands ||
            Input.readUInt32LE(Entry + 24) !== 0) {
            Reject(`The ${Label} release record is not canonical.`);
        }
        Actual.push({
            block: Input.readUInt32LE(Entry),
            target: Input.readUInt32LE(Entry + 20),
        });
    }
    if (Actual.length !== Expected.length || Expected.some(
        (Release, Index) => Release.block !== Actual[Index].block ||
            Release.target !== Actual[Index].target,
    )) {
        Reject(
            `The ${Label} release order differs: ${JSON.stringify(Actual)}.`,
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

function Requireˉsuccess(Result, Prefix, Label) {
    if (Result.error !== undefined || Result.status !== 0 ||
        Result.stderr.length !== 0 || !Normalize(Result.stdout).startsWith(Prefix)) {
        Reject(
            `${Label} failed: status=${Result.status} ` +
            `error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout}\nstderr=${Result.stderr}`,
        );
    }
}

function Requireˉrejection(Result, Diagnostic, Label, ...Outputs) {
    if (Result.error !== undefined || Result.status !== 1 ||
        Result.stdout.length !== 0 || Normalize(Result.stderr) !== Diagnostic ||
        Outputs.some(Exists)) {
        Reject(
            `${Label} rejection differs: status=${Result.status} ` +
            `error=${Result.error?.message ?? ''}\n` +
            `stdout=${Result.stdout}\nstderr=${Result.stderr}`,
        );
    }
}

function Exists(Candidate) {
    try { lstatSync(Candidate); return true; } catch (Error) {
        if (Error?.code === 'ENOENT') return false;
        throw Error;
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

function Normalize(Value) {
    return Value.replaceAll('\r\n', '\n');
}

function Reject(Message) {
    throw new Error(Message);
}
