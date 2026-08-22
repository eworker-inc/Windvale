import { spawnSync } from 'node:child_process';
import {
    lstat,
    readFile,
    realpath,
    unlink,
    writeFile,
} from 'node:fs/promises';
import path from 'node:path';

const MAXIMUM_WVB_BYTES = 16_777_216;
const MAXIMUM_DIAGNOSTIC_BYTES = 65_536;

if (process.argv.length !== 6) {
    Reject(
        'Usage: node Tools/Native/Verify-Language-1.0-Vector-Reads-Freeze.mjs ' +
        '<verifier> <runner> <input.wvb> <work-directory>',
    );
}

const Verifier = path.resolve(process.argv[2]);
const Runner = path.resolve(process.argv[3]);
const Inputˉpath = path.resolve(process.argv[4]);
const Work = path.resolve(process.argv[5]);
await Requireˉordinaryˉfile(Verifier, 134_217_728, 'verifier');
await Requireˉordinaryˉfile(Runner, 134_217_728, 'runner');
await Requireˉordinaryˉdirectory(Work, 'work directory');
const Input = await Readˉbounded(Inputˉpath, MAXIMUM_WVB_BYTES, 'WVB input');
const Layout = Inspectˉlayout(Input);

const Cases = [
    {
        name: 'old-minor',
        mutate: Candidate => Candidate.writeUInt16LE(19, 6),
    },
    {
        name: 'acquire-shared-return',
        phase: 'typed-execution',
        mutate: Candidate => { Candidate[Layout.acquireTake] = 4; },
    },
    {
        name: 'length-shared-read',
        phase: 'typed-execution',
        mutate: Candidate => { Candidate[Layout.lengthTake] = 4; },
    },
    {
        name: 'length-type-out-of-range',
        mutate: Candidate => Candidate.writeUInt32LE(
            2, Layout.lengthInstruction + 1,
        ),
    },
    {
        name: 'freeze-vector-type-out-of-range',
        mutate: Candidate => Candidate.writeUInt32LE(
            2, Layout.freezeInstruction + 1,
        ),
    },
    {
        name: 'freeze-sequence-type-confusion',
        mutate: Candidate => Candidate.writeUInt32LE(
            0, Layout.freezeInstruction + 5,
        ),
    },
];

const Created = [];
try {
    Requireˉvalidˉexecution(
        Run(Verifier, [Inputˉpath]),
        'wvb status=Valid profile=compiler-aligned\n',
        'verifier',
    );
    Requireˉvalidˉexecution(
        Run(Runner, [Inputˉpath]),
        'Result: 42\n',
        'runner',
    );
    for (const Case of Cases) {
        const Candidate = Buffer.from(Input);
        Case.mutate(Candidate);
        const Candidateˉpath = path.join(
            Work, `Vector-Read-Freeze-${Case.name}.wvb`,
        );
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        Created.push(Candidateˉpath);
        const Result = Run(Verifier, [Candidateˉpath]);
        const Phase = Case.phase ?? 'semantic';
        if (Result.status === 0 || Result.error !== undefined ||
            Result.stdout.length !== 0 ||
            Normalize(Result.stderr) !== `wvb status=Invalid phase=${Phase}\n`) {
            Reject(
                `The ${Case.name} corruption was not rejected exactly: ` +
                `status=${Result.status} stdout=${JSON.stringify(Result.stdout)} ` +
                `stderr=${JSON.stringify(Normalize(Result.stderr))}.`,
            );
        }
    }
    console.log(
        'language 1 vector reads and freeze status=Passed ' +
        `cases=${Cases.length + 5} contextual-cases=3 bytes=${Input.length}`,
    );
} finally {
    for (const Candidate of Created) {
        await unlink(Candidate).catch(() => {});
    }
}

function Inspectˉlayout(Input) {
    if (Input.length < 12 || Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 20 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The Vector read/freeze fixture is not canonical WVB 1.20.');
    }
    const Sections = new Map();
    let Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        Requireˉrange(Input, Cursor, 8, 'section header');
        if (Input[Cursor] !== Kind || Input[Cursor + 1] !== 0 ||
            Input.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The WVB section ${Kind} header differs.`);
        }
        const Length = Input.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        Requireˉrange(Input, Payload, Length, `section ${Kind}`);
        Sections.set(Kind, { payload: Payload, end: Payload + Length });
        Cursor = Payload + Length;
    }
    if (Cursor !== Input.length) Reject('The WVB has trailing bytes.');

    Requireˉcollectionˉtypes(Input, Sections.get(7));
    const Functions = Inspectˉfunctions(Input, Sections.get(4), Sections.get(5));
    const Acquire = Requireˉfunction(Functions, 'Acquire', '1700000000');
    const Acquireˉsequence = Requireˉfunction(
        Functions, 'Acquireˉsequence', '1801000000',
    );
    const Consume = Requireˉfunction(Functions, 'Consume', '0a');
    const Freeze = Requireˉfunction(Functions, 'Freeze', '1801000000');
    const Freezeˉargument = Requireˉfunction(
        Functions, 'Freezeˉargument', '0a',
    );
    const Freezeˉassignment = Requireˉfunction(
        Functions, 'Freezeˉassignment', '0a',
    );
    const Freezeˉlocal = Requireˉfunction(Functions, 'Freezeˉlocal', '0a');
    Requireˉfunction(Functions, 'Main', '01');
    const Length = Requireˉfunction(Functions, 'Readˉlength', '0a');
    if (Functions.size !== 9) {
        Reject('The fixture must retain exactly nine functions.');
    }

    const Acquireˉpattern = Buffer.from([
        64, 0, 0, 0, 0,
        5, 0, 0, 0, 0,
        205, 0, 0, 0, 0,
        81,
    ]);
    const Freezeˉpattern = Buffer.from([
        64, 0, 0, 0, 0,
        5, 1, 0, 0, 0,
        205, 1, 0, 0, 0,
        5, 0, 0, 0, 0,
        205, 0, 0, 0, 0,
        201, 0, 0, 0, 0, 1, 0, 0, 0,
        5, 2, 0, 0, 0,
        4, 2, 0, 0, 0,
        81,
    ]);
    const Lengthˉpattern = Buffer.from([
        64, 0, 0, 0, 0,
        5, 1, 0, 0, 0,
        205, 1, 0, 0, 0,
        5, 0, 0, 0, 0,
        205, 0, 0, 0, 0,
        202, 0, 0, 0, 0,
        5, 2, 0, 0, 0,
        5, 0, 0, 0, 0,
        4, 2, 0, 0, 0,
        81,
    ]);
    Requireˉexactˉcode(Input, Acquire, Acquireˉpattern);
    Requireˉexactˉcode(
        Input, Acquireˉsequence,
        Buffer.from('40010000000500000000040000000051', 'hex'),
    );
    Requireˉexactˉcode(
        Input, Consume,
        Buffer.from(
            '040000000005010000000401000000cb01000000050200000050040200000051',
            'hex',
        ),
    );
    Requireˉexactˉcode(Input, Freeze, Freezeˉpattern);
    Requireˉexactˉcode(
        Input, Freezeˉargument,
        Buffer.from(
            '40000000000501000000cd010000000500000000cd00000000' +
            'c9000000000100000005020000000402000000400200000005' +
            '03000000040300000051',
            'hex',
        ),
    );
    Requireˉexactˉcode(
        Input, Freezeˉassignment,
        Buffer.from(
            '40010000000502000000040200000005000000004000000000' +
            '0503000000cd030000000501000000cd01000000c90000000001' +
            '0000000504000000040400000005000000000400000000050500' +
            '00000405000000cb01000000050600000050040600000051',
            'hex',
        ),
    );
    Requireˉexactˉcode(
        Input, Freezeˉlocal,
        Buffer.from(
            '40000000000502000000cd020000000500000000cd00000000' +
            'c9000000000100000005030000000403000000050100000004' +
            '0100000005040000000404000000cb01000000050500000050' +
            '040500000051',
            'hex',
        ),
    );
    Requireˉexactˉcode(Input, Length, Lengthˉpattern);
    return {
        acquireTake: Acquire.codeStart + 10,
        freezeInstruction: Freeze.codeStart + 25,
        lengthTake: Length.codeStart + 20,
        lengthInstruction: Length.codeStart + 25,
    };
}

function Requireˉcollectionˉtypes(Input, Types) {
    let Cursor = Types.payload;
    const Count = Readˉu32(Input, Cursor, Types.end, 'type count');
    Cursor += 4;
    if (Count !== 2) Reject('The fixture must contain two collection types.');
    for (let Index = 0; Index < Count; Index += 1) {
        Requireˉrange(Input, Cursor, 5, 'collection descriptor');
        const Kind = Input[Cursor];
        Cursor += 1;
        const Nameˉlength = Input.readUInt32LE(Cursor);
        Cursor += 4;
        Requireˉrange(Input, Cursor, Nameˉlength, 'collection name');
        if (Nameˉlength === 0) Reject('A collection type name is empty.');
        Cursor += Nameˉlength;
        if (Kind !== 5 + Index || Input[Cursor] !== 1) {
            Reject('The exact Vector<i32>/Sequence<i32> catalog differs.');
        }
        Cursor = Shapeˉend(Input, Cursor, Types.end);
    }
    if (Cursor !== Types.end) Reject('The Types section differs.');
}

function Inspectˉfunctions(Input, Functions, Code) {
    let Cursor = Functions.payload;
    const Count = Readˉu32(Input, Cursor, Functions.end, 'function count');
    Cursor += 4;
    const Result = new Map();
    for (let Index = 0; Index < Count; Index += 1) {
        const Nameˉlength = Readˉu32(
            Input, Cursor, Functions.end, 'function name length',
        );
        Cursor += 4;
        Requireˉrange(Input, Cursor, Nameˉlength, 'function name');
        const Name = Input.subarray(Cursor, Cursor + Nameˉlength).toString('utf8');
        Cursor += Nameˉlength;
        const Parameters = Readˉu32(
            Input, Cursor, Functions.end, 'parameter count',
        );
        Cursor += 4;
        for (let Parameter = 0; Parameter < Parameters; Parameter += 1) {
            Cursor = Shapeˉend(Input, Cursor, Functions.end);
        }
        const Returnˉstart = Cursor;
        Cursor = Shapeˉend(Input, Cursor, Functions.end);
        const Returnˉshape = Input.subarray(Returnˉstart, Cursor).toString('hex');
        const Locals = Readˉu32(Input, Cursor, Functions.end, 'local count');
        Cursor += 4;
        for (let Local = 0; Local < Locals; Local += 1) {
            Cursor = Shapeˉend(Input, Cursor, Functions.end);
        }
        Requireˉrange(Input, Cursor, 12, 'function code metadata');
        const Codeˉoffset = Input.readUInt32LE(Cursor);
        const Codeˉlength = Input.readUInt32LE(Cursor + 4);
        Cursor += 12;
        if (Result.has(Name) || Codeˉoffset > Code.end - Code.payload ||
            Codeˉlength > Code.end - Code.payload - Codeˉoffset) {
            Reject('A function directory entry is invalid.');
        }
        Result.set(Name, {
            returnShape: Returnˉshape,
            codeStart: Code.payload + Codeˉoffset,
            codeLength: Codeˉlength,
        });
    }
    if (Cursor !== Functions.end) Reject('The Functions section differs.');
    return Result;
}

function Requireˉfunction(Functions, Name, Returnˉshape) {
    const Function = Functions.get(Name);
    if (Function === undefined || Function.returnShape !== Returnˉshape) {
        Reject(`The ${Name} function signature differs.`);
    }
    return Function;
}

function Requireˉexactˉcode(Input, Function, Expected) {
    if (Function.codeLength !== Expected.length ||
        !Input.subarray(
            Function.codeStart, Function.codeStart + Function.codeLength,
        ).equals(Expected)) {
        Reject('The compiler-produced Vector ownership lowering differs.');
    }
}

function Shapeˉend(Input, Cursor, End) {
    Requireˉrange(Input, Cursor, 1, 'value shape');
    const Kind = Input[Cursor];
    const Wide = Kind === 7 || Kind === 8 || Kind === 11 || Kind === 22 ||
        Kind === 23 || Kind === 24;
    const Width = Wide ? 5 : 1;
    Requireˉrange(Input, Cursor, Width, 'value shape');
    if (Cursor + Width > End) Reject('A value shape crosses its section.');
    return Cursor + Width;
}

function Readˉu32(Input, Offset, End, Label) {
    Requireˉrange(Input, Offset, 4, Label);
    if (Offset + 4 > End) Reject(`The ${Label} crosses its section.`);
    return Input.readUInt32LE(Offset);
}

function Requireˉrange(Input, Offset, Length, Label) {
    if (!Number.isSafeInteger(Offset) || !Number.isSafeInteger(Length) ||
        Offset < 0 || Length < 0 || Offset > Input.length ||
        Length > Input.length - Offset) {
        Reject(`The ${Label} range is invalid.`);
    }
}

function Run(Command, Arguments) {
    return spawnSync(Command, Arguments, {
        encoding: 'utf8',
        windowsHide: true,
        maxBuffer: MAXIMUM_DIAGNOSTIC_BYTES,
    });
}

function Requireˉvalidˉexecution(Result, Output, Label) {
    if (Result.error !== undefined || Result.status !== 0 ||
        Normalize(Result.stdout) !== Output || Result.stderr.length !== 0) {
        Reject(`The valid ${Label} execution differs.`);
    }
}

function Normalize(Value) {
    return Value.replaceAll('\r\n', '\n');
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

async function Readˉbounded(Candidate, Maximum, Label) {
    await Requireˉordinaryˉfile(Candidate, Maximum, Label);
    return readFile(Candidate);
}

function Sameˉpath(Left, Right) {
    return process.platform === 'win32'
        ? Left.toLowerCase() === path.resolve(Right).toLowerCase()
        : Left === path.resolve(Right);
}

function Reject(Message) {
    throw new Error(Message);
}
