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
        'Usage: node Tools/Native/Verify-Language-1.0-Sequence-Reads.mjs ' +
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
        mutate: Candidate => Candidate.writeUInt16LE(21, 6),
    },
    {
        name: 'length-type-out-of-range',
        mutate: Candidate => Candidate.writeUInt32LE(
            Layout.typeCount, Layout.lengthInstruction + 1,
        ),
    },
    {
        name: 'element-type-out-of-range',
        mutate: Candidate => Candidate.writeUInt32LE(
            Layout.typeCount, Layout.elementInstruction + 1,
        ),
    },
    {
        name: 'sequence-kind-confusion',
        mutate: Candidate => { Candidate[Layout.sequenceKind] = 5; },
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
            `Sequence-Read-${Case.name}.wvb`,
        );
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        Created.push(Candidateˉpath);
        const Result = Run(Verifier, [Candidateˉpath]);
        if (Result.status !== 1 || Result.error !== undefined ||
            Result.stdout.length !== 0 ||
            !/^wvb status=Invalid phase=semantic(?: step=[a-z-]+)?\n$/u.test(
                Normalize(Result.stderr),
            )) {
            Reject(`The ${Case.name} corruption was not rejected exactly.`);
        }
    }
    console.log(
        'language 1 sequence reads status=Passed ' +
        `cases=${Cases.length + 2} bytes=${Input.length}`,
    );
} finally {
    for (const Candidate of Created) {
        await unlink(Candidate).catch(() => {});
    }
}

function Inspectˉlayout(Input) {
    if (Input.length < 12 || Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== 22 ||
        Input.readUInt32LE(8) !== 7) {
        Reject('The sequence-read fixture is not canonical WVB 1.22.');
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
    if (Typeˉcount !== 4) {
        Reject('The fixture must contain three library types and one Sequence type.');
    }
    const Sequenceˉdescriptorˉbytes = 15;
    Typeˉcursor = Types.end - Sequenceˉdescriptorˉbytes;
    Requireˉrange(Input, Typeˉcursor, 5, 'Sequence descriptor');
    const Sequenceˉkind = Typeˉcursor;
    if (Input[Typeˉcursor] !== 6) Reject('The collection descriptor is not Sequence.');
    Typeˉcursor += 1;
    const Nameˉlength = Input.readUInt32LE(Typeˉcursor);
    Typeˉcursor += 4;
    if (Nameˉlength === 0) Reject('The Sequence descriptor name is empty.');
    Requireˉrange(Input, Typeˉcursor, Nameˉlength, 'Sequence descriptor name');
    Typeˉcursor += Nameˉlength;
    if (Input[Typeˉcursor] !== 1) Reject('The Sequence element is not i32.');
    Typeˉcursor = Shapeˉend(Input, Typeˉcursor, Types.end);
    if (Typeˉcursor !== Types.end) Reject('The Types section differs.');

    const Functions = Sections.get(4);
    let Functionˉcursor = Functions.payload;
    const Functionˉcount = Readˉu32(
        Input, Functionˉcursor, Functions.end, 'function count',
    );
    Functionˉcursor += 4;
    if (Functionˉcount !== 2) Reject('The fixture must retain two functions.');
    let Readˉfunction = null;
    for (let Function = 0; Function < Functionˉcount; Function += 1) {
        const Nameˉlength = Readˉu32(
            Input, Functionˉcursor, Functions.end, 'function name length',
        );
        Functionˉcursor += 4;
        Requireˉrange(Input, Functionˉcursor, Nameˉlength, 'function name');
        const Name = Input.subarray(
            Functionˉcursor, Functionˉcursor + Nameˉlength,
        ).toString('utf8');
        Functionˉcursor += Nameˉlength;
        const Parameters = Readˉu32(
            Input, Functionˉcursor, Functions.end, 'parameter count',
        );
        Functionˉcursor += 4;
        const Parameterˉshapes = [];
        for (let Parameter = 0; Parameter < Parameters; Parameter += 1) {
            const Start = Functionˉcursor;
            Functionˉcursor = Shapeˉend(Input, Functionˉcursor, Functions.end);
            Parameterˉshapes.push(Buffer.from(Input.subarray(Start, Functionˉcursor)));
        }
        const Returnˉstart = Functionˉcursor;
        Functionˉcursor = Shapeˉend(Input, Functionˉcursor, Functions.end);
        const Returnˉshape = Buffer.from(
            Input.subarray(Returnˉstart, Functionˉcursor),
        );
        const Locals = Readˉu32(
            Input, Functionˉcursor, Functions.end, 'local count',
        );
        Functionˉcursor += 4;
        for (let Local = 0; Local < Locals; Local += 1) {
            Functionˉcursor = Shapeˉend(Input, Functionˉcursor, Functions.end);
        }
        Requireˉrange(Input, Functionˉcursor, 12, 'function code metadata');
        const Codeˉoffset = Input.readUInt32LE(Functionˉcursor);
        const Codeˉlength = Input.readUInt32LE(Functionˉcursor + 4);
        Functionˉcursor += 12;
        if (Name === 'Readˉat') {
            Readˉfunction = {
                parameters: Parameterˉshapes,
                returnShape: Returnˉshape,
                codeOffset: Codeˉoffset,
                codeLength: Codeˉlength,
            };
        }
    }
    if (Functionˉcursor !== Functions.end || Readˉfunction === null) {
        Reject('The Readˉat function directory entry is absent.');
    }
    if (Readˉfunction.parameters.length !== 2 ||
        Readˉfunction.parameters[0].toString('hex') !== '1803000000' ||
        Readˉfunction.parameters[1].toString('hex') !== '0a' ||
        Readˉfunction.returnShape.toString('hex') !== '01') {
        Reject('The Readˉat function signature differs.');
    }
    const Code = Sections.get(5);
    if (Readˉfunction.codeOffset > Code.end - Code.payload ||
        Readˉfunction.codeLength >
            Code.end - Code.payload - Readˉfunction.codeOffset) {
        Reject('The Readˉat code range is invalid.');
    }
    const Readˉstart = Code.payload + Readˉfunction.codeOffset;
    const Readˉcode = Input.subarray(
        Readˉstart, Readˉstart + Readˉfunction.codeLength,
    );
    const Lengthˉrelative = Findˉunique(
        Readˉcode, Buffer.from([203, 3, 0, 0, 0]), 'sequence.length',
    );
    const Elementˉrelative = Findˉunique(
        Readˉcode, Buffer.from([204, 3, 0, 0, 0]), 'sequence.element',
    );
    Requireˉrelease(Readˉcode, Lengthˉrelative, 'sequence.length');
    Requireˉrelease(Readˉcode, Elementˉrelative, 'sequence.element');
    return {
        sequenceKind: Sequenceˉkind,
        typeCount: Typeˉcount,
        lengthInstruction: Readˉstart + Lengthˉrelative,
        elementInstruction: Readˉstart + Elementˉrelative,
    };
}

function Requireˉrelease(Code, Instruction, Label) {
    if (Instruction > Code.length - 11 || Code[Instruction + 5] !== 5 ||
        Code[Instruction + 10] !== 80) {
        Reject(`The ${Label} result-store/release sequence differs.`);
    }
}

function Findˉunique(Input, Pattern, Label) {
    const First = Input.indexOf(Pattern);
    if (First < 0 || Input.indexOf(Pattern, First + 1) >= 0) {
        Reject(`The ${Label} instruction is not unique.`);
    }
    return First;
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
