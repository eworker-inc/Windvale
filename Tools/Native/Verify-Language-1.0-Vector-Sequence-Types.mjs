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
        'Usage: node Tools/Native/Verify-Language-1.0-Vector-Sequence-Types.mjs ' +
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
        mutate: Candidate => Candidate.writeUInt16LE(17, 6),
    },
    {
        name: 'invalid-vector-element',
        mutate: Candidate => { Candidate[Layout.vectorElement] = 21; },
    },
    {
        name: 'vector-kind-confusion',
        mutate: Candidate => { Candidate[Layout.vectorKind] = 6; },
    },
    {
        name: 'vector-target-confusion',
        mutate: Candidate => Candidate.writeUInt32LE(
            Layout.sequenceTypeIndex,
            Layout.vectorTarget,
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
            Work,
            `Vector-Sequence-Types-${Case.name}.wvb`,
        );
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        Created.push(Candidateˉpath);
        const Result = Run(Verifier, [Candidateˉpath]);
        if (Result.status !== 1 || Result.error !== undefined ||
            Result.stdout.length !== 0 ||
            !/^wvb status=Invalid phase=semantic(?: step=[a-z-]+)?\r?\n$/u.test(
                Result.stderr,
            )) {
            Reject(`The ${Case.name} corruption was not rejected exactly.`);
        }
    }
    console.log(
        'language 1 vector-sequence WVB types status=Passed ' +
        `cases=${Cases.length + 2} bytes=${Input.length}`,
    );
} finally {
    for (const Candidate of Created) {
        await unlink(Candidate).catch(() => {});
    }
}

function Inspectˉlayout(Input) {
    if (Input.length < 12 || Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 26 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The fixture is not canonical WVB 1.26.');
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

    const Types = Sections.get(7);
    let Typeˉcursor = Types.payload;
    const Typeˉcount = Readˉu32(Input, Typeˉcursor, Types.end, 'type count');
    Typeˉcursor += 4;
    if (Typeˉcount !== 5) {
        Reject('The fixture must contain three library and two collection types.');
    }
    const Collectionˉdescriptorˉbytes = 15;
    Typeˉcursor = Types.end - (2 * Collectionˉdescriptorˉbytes);
    const Typeˉentries = [];
    for (let Index = 3; Index < Typeˉcount; Index += 1) {
        Requireˉrange(Input, Typeˉcursor, 5, 'type descriptor');
        const Kindˉoffset = Typeˉcursor;
        const Kind = Input[Typeˉcursor];
        Typeˉcursor += 1;
        const Nameˉlength = Input.readUInt32LE(Typeˉcursor);
        Typeˉcursor += 4;
        Requireˉrange(Input, Typeˉcursor, Nameˉlength, 'type name');
        const Name = Input.subarray(
            Typeˉcursor,
            Typeˉcursor + Nameˉlength,
        ).toString('ascii');
        Typeˉcursor += Nameˉlength;
        const Element = Typeˉcursor;
        Typeˉcursor = Shapeˉend(Input, Typeˉcursor, Types.end);
        Typeˉentries.push({ kind: Kind, kindOffset: Kindˉoffset, name: Name, element: Element });
    }
    if (Typeˉcursor !== Types.end || Typeˉentries[0].kind !== 5 ||
        Typeˉentries[1].kind !== 6 || Typeˉentries[0].name.length === 0 ||
        Typeˉentries[0].name === Typeˉentries[1].name ||
        Input[Typeˉentries[0].element] !== 1 ||
        Input[Typeˉentries[1].element] !== 1) {
        Reject('The Vector/Sequence type descriptors differ.');
    }

    const Functions = Sections.get(4);
    let Functionˉcursor = Functions.payload;
    const Functionˉcount = Readˉu32(
        Input, Functionˉcursor, Functions.end, 'function count',
    );
    Functionˉcursor += 4;
    if (Functionˉcount !== 3) Reject('The fixture must retain three functions.');
    let Vectorˉtarget = -1;
    let Sequenceˉtarget = -1;
    for (let Function = 0; Function < Functionˉcount; Function += 1) {
        const Nameˉlength = Readˉu32(
            Input, Functionˉcursor, Functions.end, 'function name',
        );
        Functionˉcursor += 4;
        Requireˉrange(Input, Functionˉcursor, Nameˉlength, 'function name bytes');
        Functionˉcursor += Nameˉlength;
        const Parameters = Readˉu32(
            Input, Functionˉcursor, Functions.end, 'parameter count',
        );
        Functionˉcursor += 4;
        for (let Parameter = 0; Parameter < Parameters; Parameter += 1) {
            const Shape = Functionˉcursor;
            const Kind = Input[Shape];
            Functionˉcursor = Shapeˉend(Input, Shape, Functions.end);
            if (Kind === 23) Vectorˉtarget = Shape + 1;
            if (Kind === 24) Sequenceˉtarget = Shape + 1;
        }
        Functionˉcursor = Shapeˉend(Input, Functionˉcursor, Functions.end);
        const Locals = Readˉu32(
            Input, Functionˉcursor, Functions.end, 'local count',
        );
        Functionˉcursor += 4;
        for (let Local = 0; Local < Locals; Local += 1) {
            Functionˉcursor = Shapeˉend(Input, Functionˉcursor, Functions.end);
        }
        Requireˉrange(Input, Functionˉcursor, 12, 'function code metadata');
        Functionˉcursor += 12;
    }
    if (Functionˉcursor !== Functions.end || Vectorˉtarget < 0 ||
        Sequenceˉtarget < 0 || Input.readUInt32LE(Vectorˉtarget) !== 3 ||
        Input.readUInt32LE(Sequenceˉtarget) !== 4) {
        Reject('The Vector/Sequence function shapes differ.');
    }
    return {
        vectorElement: Typeˉentries[0].element,
        vectorKind: Typeˉentries[0].kindOffset,
        vectorTarget: Vectorˉtarget,
        sequenceTypeIndex: 4,
    };
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
        Result.stdout !== Output || Result.stderr.length !== 0) {
        Reject(`The valid ${Label} execution differs.`);
    }
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
