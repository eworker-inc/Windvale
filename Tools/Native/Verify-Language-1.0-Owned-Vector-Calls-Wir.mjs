import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
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

if (process.argv.length !== 6 && process.argv.length !== 7) {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Language-1.0-Owned-Vector-Calls-Wir.mjs ' +
        '<admitter> <analyzer> <emitter> <work-directory> ' +
        '[owned-aggregate-output.wvb]\n',
    );
    process.exit(64);
}

const Admitter = path.resolve(process.argv[2]);
const Analyzer = path.resolve(process.argv[3]);
const Emitter = path.resolve(process.argv[4]);
const Work = path.resolve(process.argv[5]);
const Aggregateˉoutput = process.argv.length === 7
    ? path.resolve(process.argv[6])
    : null;
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
if (Aggregateˉoutput !== null &&
    (!Sameˉpath(path.dirname(Aggregateˉoutput), Work) ||
        path.extname(Aggregateˉoutput).toLowerCase() !== '.wvb' ||
        Exists(Aggregateˉoutput))) {
    Reject(`The aggregate evidence output is not a fresh child of work: ${Aggregateˉoutput}.`);
}
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
    {
        name: 'owned-vector-loop-invariant',
        fixture: 'Owned-Vector-Loop-Invariant-Wir.wv',
        valid: true,
    },
    {
        name: 'owned-vector-loop-state-mismatch',
        fixture: 'Owned-Vector-Loop-State-Mismatch.wv',
        valid: false,
    },
    {
        name: 'owned-aggregate-vector',
        fixture: 'Owned-Aggregate-Vector-Executable.wv',
        valid: true,
    },
    {
        name: 'owned-aggregate-use-after-move',
        fixture: 'Owned-Aggregate-Use-After-Move.wv',
        valid: false,
    },
    {
        name: 'owned-aggregate-duplicate-move',
        fixture: 'Owned-Aggregate-Duplicate-Move.wv',
        valid: false,
    },
    {
        name: 'owned-aggregate-field-move',
        fixture: 'Owned-Aggregate-Field-Move.wv',
        valid: false,
    },
    {
        name: 'owned-aggregate-mutable-borrow-from-let',
        fixture: 'Owned-Aggregate-Mutable-Borrow-From-Let.wv',
        analysisInvalid:
            'source analysis status=Sourceˉwir symbol-status=Valid ' +
            'binding-status=Valid wir-status=Invalidˉborrow ' +
            'failure-module=0 related-module=0 function=9 offset=521 ' +
            'line=22 column=11\n',
    },
];

const Created = [];
try {
    let Positiveˉwvirˉbytes = 0;
    let Positiveˉwvbˉbytes = 0;
    let Positiveˉwvbˉsha256 = '';
    let Positiveˉcalls = 0;
    let Aggregateˉwvbˉbytes = 0;
    let Aggregateˉwvbˉsha256 = '';
    for (const Case of Cases) {
        const Fixture = path.join(Fixtureˉroot, Case.fixture);
        const Prefix = path.join(Work, Case.name);
        const Source = `${Prefix}.wvss`;
        const Manifest = `${Prefix}.wvca`;
        const Bindings = `${Prefix}.wvlb`;
        const Wir = `${Prefix}.wvir`;
        const Persistˉaggregate = Case.name === 'owned-aggregate-vector' &&
            Aggregateˉoutput !== null;
        const Product = Persistˉaggregate ? Aggregateˉoutput : `${Prefix}.wvb`;
        Created.push(Source, Manifest, Bindings, Wir);
        if (!Persistˉaggregate) Created.push(Product);
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
        if (Case.analysisInvalid !== undefined) {
            Requireˉrejection(
                Analysis, Case.analysisInvalid,
                `${Case.name} analysis`, Product,
            );
            continue;
        }
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
        const Wirˉbytes = readFileSync(Wir);
        if (Case.name === 'owned-vector-calls-and-joins') {
            const Evidence = Inspectˉownedˉcallˉwvir(Wirˉbytes);
            Positiveˉwvirˉbytes = Evidence.bytes;
            Positiveˉcalls = Evidence.calls;
        }
        Requireˉownedˉcallˉproduct(
            Run(Emitter, [Source, Manifest, Bindings, Wir, Product]), Product,
        );
        const Productˉbytes = readFileSync(Product);
        if (Case.name === 'owned-vector-calls-and-joins') {
            Inspectˉownedˉcallˉwvb(Productˉbytes);
            Positiveˉwvbˉbytes = Productˉbytes.length;
            Positiveˉwvbˉsha256 = createHash('sha256')
                .update(Productˉbytes).digest('hex');
        }
        if (Case.name === 'owned-aggregate-vector') {
            Inspectˉownedˉaggregateˉwvb(Productˉbytes);
            Aggregateˉwvbˉbytes = Productˉbytes.length;
            Aggregateˉwvbˉsha256 = createHash('sha256')
                .update(Productˉbytes).digest('hex');
        }
    }
    process.stdout.write(
        'language 1 owned Vector calls and joins WVIR status=Passed ' +
        `cases=${Cases.length} calls=${Positiveˉcalls} ` +
        `wvir-bytes=${Positiveˉwvirˉbytes} ` +
        `wvb-bytes=${Positiveˉwvbˉbytes} ` +
        `wvb-sha256=${Positiveˉwvbˉsha256} ` +
        `aggregate-wvb-bytes=${Aggregateˉwvbˉbytes} ` +
        `aggregate-wvb-sha256=${Aggregateˉwvbˉsha256}\n`,
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
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 11) {
        Reject('The owned-call fixture is not bounded WVIR 1.11.');
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
        Operations < 20 || Operations > 512 || Operationˉbytes !== 28 ||
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
        if (Input.readUInt16LE(Entry + 4) === 62) Calls += 1;
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

function Requireˉownedˉcallˉproduct(Result, Product) {
    if (Result.error !== undefined || Result.status !== 0 ||
        Result.stderr.length !== 0 || !Exists(Product)) {
        Reject(
            `The owned-call WVB publication differs: status=${Result.status} ` +
            `stdout=${JSON.stringify(Result.stdout)} ` +
            `stderr=${JSON.stringify(Normalize(Result.stderr))}.`,
        );
    }
}

function Inspectˉownedˉcallˉwvb(Input) {
    if (Input.length < 64 || Input.length > MAXIMUM_INPUT_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 26) {
        Reject('The owned-call product is not bounded WVB 1.26.');
    }
    let Cursor = 12;
    let Functions = null;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor + 8 > Input.length || Input[Cursor] !== Kind ||
            Input[Cursor + 1] !== 0 || Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The owned-call product has no canonical section ${Kind}.`);
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Input.length) {
            Reject('The owned-call product contains a truncated section.');
        }
        if (Kind === 4) Functions = { payload: Payload, end: Payload + Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Input.length || Functions === null) {
        Reject('The owned-call product has non-canonical trailing bytes.');
    }
    const Count = Input.readUInt32LE(Functions.payload);
    if (Count < 6 || Count > 256) {
        Reject(`The owned-call function count is outside its bound: ${Count}.`);
    }
    Cursor = Functions.payload + 4;
    const Entries = [];
    for (let Index = 0; Index < Count; Index += 1) {
        const Name = Readˉwvbˉstring(Input, Cursor, Functions.end);
        Cursor = Name.end;
        if (Cursor + 4 > Functions.end) Reject('A function entry is truncated.');
        const Parameterˉcount = Input.readUInt32LE(Cursor);
        Cursor += 4;
        if (Parameterˉcount > 64) {
            Reject('The owned-call parameter directory is outside its bound.');
        }
        const Parameterˉshapes = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Shape = Readˉwvbˉshape(Input, Cursor, Functions.end);
            Parameterˉshapes.push(Shape.kind);
            Cursor = Shape.end;
        }
        Cursor = Readˉwvbˉshape(Input, Cursor, Functions.end).end;
        if (Cursor + 4 > Functions.end) Reject('A local directory is truncated.');
        const Localˉcount = Input.readUInt32LE(Cursor);
        Cursor += 4;
        if (Localˉcount > 2048) Reject('A local directory is outside its bound.');
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            Cursor = Readˉwvbˉshape(Input, Cursor, Functions.end).end;
        }
        if (Cursor + 12 > Functions.end) Reject('Function code metadata is truncated.');
        Cursor += 12;
        Entries.push({
            name: Name.value,
            parameterShapes: Parameterˉshapes,
        });
    }
    if (Cursor !== Functions.end) {
        Reject('The WVB 1.26 function directory contains trailing bytes.');
    }
    const Expected = [
        { name: 'Forward', shapes: [23] },
        { name: 'Observe', shapes: [26] },
        { name: 'Release', shapes: [23] },
        { name: 'Borrowˉthenˉforward', shapes: [23] },
        { name: 'Consumeˉonˉbothˉpaths', shapes: [23, 2] },
        { name: 'Main', shapes: [25] },
    ];
    for (const Expectation of Expected) {
        const Entry = Entries.find(
            Candidate => Candidate.name === Expectation.name,
        );
        if (Entry === undefined ||
            Entry.parameterShapes.length !== Expectation.shapes.length ||
            Expectation.shapes.some(
                (Shape, Index) => Entry.parameterShapes[Index] !== Shape,
            )) {
            Reject(`The ${Expectation.name} WVB parameter shapes differ.`);
        }
    }
}

function Inspectˉownedˉaggregateˉwvb(Input) {
    if (Input.length < 64 || Input.length > MAXIMUM_INPUT_BYTES ||
        Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 28) {
        Reject('The owned aggregate product is not bounded WVB 1.28.');
    }
    let Cursor = 12;
    let Functions = null;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor + 8 > Input.length || Input[Cursor] !== Kind ||
            Input[Cursor + 1] !== 0 || Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The owned aggregate product has no canonical section ${Kind}.`);
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Input.length) {
            Reject('The owned aggregate product contains a truncated section.');
        }
        if (Kind === 4) Functions = { payload: Payload, end: Payload + Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Input.length || Functions === null) {
        Reject('The owned aggregate product has non-canonical trailing bytes.');
    }
    const Count = Input.readUInt32LE(Functions.payload);
    Cursor = Functions.payload + 4;
    let Ownedˉparameters = 0;
    let Borrowedˉlocals = 0;
    for (let Index = 0; Index < Count; Index += 1) {
        Cursor = Readˉwvbˉstring(Input, Cursor, Functions.end).end;
        const Parameterˉcount = Input.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter += 1) {
            const Shape = Readˉwvbˉshape(Input, Cursor, Functions.end);
            if (Shape.kind === 7 || Shape.kind === 11 || Shape.kind === 22) {
                Ownedˉparameters += 1;
            }
            Cursor = Shape.end;
        }
        Cursor = Readˉwvbˉshape(Input, Cursor, Functions.end).end;
        const Localˉcount = Input.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Local = 0; Local < Localˉcount; Local += 1) {
            const Shape = Readˉwvbˉshape(Input, Cursor, Functions.end);
            if (Shape.kind === 28 || Shape.kind === 29 || Shape.kind === 30) {
                Borrowedˉlocals += 1;
            }
            Cursor = Shape.end;
        }
        if (Cursor + 12 > Functions.end) {
            Reject('The owned aggregate function metadata is truncated.');
        }
        Cursor += 12;
    }
    if (Cursor !== Functions.end || Ownedˉparameters === 0 ||
        Borrowedˉlocals === 0) {
        Reject(
            `The owned aggregate evidence differs: parameters=${Ownedˉparameters} ` +
            `borrowed-locals=${Borrowedˉlocals}.`,
        );
    }
}

function Readˉwvbˉstring(Input, Cursor, End) {
    if (Cursor + 4 > End) Reject('A WVB string length is truncated.');
    const Length = Input.readUInt32LE(Cursor);
    if (Length > 16_384 || Cursor + 4 + Length > End) {
        Reject('A WVB string is outside its bound.');
    }
    return {
        value: Input.subarray(Cursor + 4, Cursor + 4 + Length).toString('utf8'),
        end: Cursor + 4 + Length,
    };
}

function Readˉwvbˉshape(Input, Cursor, End) {
    if (Cursor >= End) Reject('A WVB shape is truncated.');
    const Kind = Input[Cursor];
    let Next = Cursor + 1;
    if ([7, 8, 11, 22, 23, 24, 26, 27, 28, 29, 30].includes(Kind)) Next += 4;
    if (Next > End) Reject('A WVB nominal shape is truncated.');
    return { kind: Kind, end: Next };
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
