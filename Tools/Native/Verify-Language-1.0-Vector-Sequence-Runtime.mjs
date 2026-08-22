import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
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
        'Usage: node Tools/Native/Verify-Language-1.0-Vector-Sequence-Runtime.mjs ' +
        '<verifier> <runner> <types.wvb> <work-directory>',
    );
}

const Verifier = path.resolve(process.argv[2]);
const Runner = path.resolve(process.argv[3]);
const Typesˉpath = path.resolve(process.argv[4]);
const Work = path.resolve(process.argv[5]);
await Requireˉordinaryˉfile(Verifier, 134_217_728, 'verifier');
await Requireˉordinaryˉfile(Runner, 134_217_728, 'runner');
await Requireˉordinaryˉdirectory(Work, 'work directory');
const Typesˉmodule = await Readˉbounded(
    Typesˉpath, MAXIMUM_WVB_BYTES, 'types WVB input',
);
const Runtime = Buildˉruntime(Typesˉmodule);
const Runtimeˉpath = path.join(Work, 'Vector-Sequence-Runtime.wvb');
const Created = [Runtimeˉpath];

const Mutations = [
    {
        name: 'old-minor',
        accepted: false,
        mutate: Candidate => Candidate.writeUInt16LE(19, 6),
    },
    {
        name: 'out-of-range-take',
        accepted: false,
        mutate: Candidate => Candidate.writeUInt32LE(
            5, Findˉfirst(Candidate, Buffer.from([205, 1, 0, 0, 0])) + 1,
        ),
    },
    {
        name: 'uninitialized-take',
        accepted: false,
        phase: 'control-reachability',
        mutate: Candidate => Candidate.writeUInt32LE(
            4, Findˉfirst(Candidate, Buffer.from([205, 1, 0, 0, 0])) + 1,
        ),
    },
    {
        name: 'double-take',
        accepted: false,
        phase: 'control-reachability',
        mutate: Candidate => {
            const Start = Findˉfirst(
                Candidate,
                Buffer.from([
                    205, 1, 0, 0, 0,
                    1, 20, 0, 0, 0,
                    200, 0, 0, 0, 0,
                    5, 1, 0, 0, 0,
                ]),
            );
            Buffer.concat([
                Instructionˉu32(205, 1),
                Instructionˉu32(5, 4),
                Instructionˉu32(205, 1),
                Instructionˉu32(5, 1),
            ]).copy(Candidate, Start);
        },
    },
    {
        name: 'create-type-confusion',
        accepted: false,
        mutate: Candidate => Candidate.writeUInt32LE(
            1, Findˉfirst(Candidate, Buffer.from([199, 0, 0, 0, 0])) + 1,
        ),
    },
    {
        name: 'vector-local-alias',
        accepted: false,
        phase: 'typed-execution',
        mutate: Candidate => {
            const Create = Findˉfirst(
                Candidate, Buffer.from([199, 0, 0, 0, 0]),
            );
            Candidate[Create] = 4;
            Candidate.writeUInt32LE(1, Create + 1);
        },
    },
    {
        name: 'freeze-type-confusion',
        accepted: false,
        mutate: Candidate => Candidate.writeUInt32LE(
            0,
            Findˉfirst(
                Candidate, Buffer.from([201, 0, 0, 0, 0, 1, 0, 0, 0]),
            ) + 5,
        ),
    },
    {
        name: 'append-value-confusion',
        accepted: false,
        mutate: Candidate => {
            const Constant = Findˉfirst(
                Candidate,
                Buffer.from([1, 20, 0, 0, 0, 200, 0, 0, 0, 0]),
            );
            Candidate[Constant] = 3;
        },
    },
    {
        name: 'capacity-exhaustion',
        accepted: true,
        failure: 3008,
        mutate: Candidate => Candidate.writeBigUInt64LE(
            1n,
            Findˉfirst(
                Candidate,
                Buffer.from([129, 255, 7, 0, 0, 0, 0, 0, 0]),
            ) + 1,
        ),
    },
    {
        name: 'sequence-bounds',
        accepted: true,
        failure: 3008,
        mutate: Candidate => Candidate.writeBigUInt64LE(
            2n,
            Findˉfirst(
                Candidate,
                Buffer.from([129, 1, 0, 0, 0, 0, 0, 0, 0, 204, 1, 0, 0, 0]),
            ) + 1,
        ),
    },
];

try {
    await writeFile(Runtimeˉpath, Runtime, { flag: 'wx' });
    Requireˉvalidˉexecution(
        Run(Verifier, [Runtimeˉpath]),
        'wvb status=Valid profile=compiler-aligned\n',
        'runtime verifier',
    );
    Requireˉvalidˉexecution(
        Run(Runner, [Runtimeˉpath]),
        'Result: 42\n',
        'runtime runner',
    );

    for (const Mutation of Mutations) {
        const Candidate = Buffer.from(Runtime);
        Mutation.mutate(Candidate);
        const Candidateˉpath = path.join(
            Work, `Vector-Sequence-Runtime-${Mutation.name}.wvb`,
        );
        await writeFile(Candidateˉpath, Candidate, { flag: 'wx' });
        Created.push(Candidateˉpath);
        const Verification = Run(Verifier, [Candidateˉpath]);
        if (!Mutation.accepted) {
            const Phase = Mutation.phase ?? 'semantic';
            if (Verification.status === 0 || Verification.error !== undefined ||
                Verification.stdout.length !== 0 ||
                Verification.stderr !== `wvb status=Invalid phase=${Phase}\n`) {
                Reject(
                    `${Mutation.name} was not rejected exactly: ` +
                    `status=${Verification.status} ` +
                    `stdout=${JSON.stringify(Verification.stdout)} ` +
                    `stderr=${JSON.stringify(Verification.stderr)}.`,
                );
            }
            continue;
        }
        Requireˉvalidˉexecution(
            Verification,
            'wvb status=Valid profile=compiler-aligned\n',
            `${Mutation.name} verifier`,
        );
        const Execution = Run(Runner, [Candidateˉpath]);
        if (Execution.status !== 1 || Execution.stdout.length !== 0 ||
            Execution.error !== undefined ||
            !new RegExp(
                `^wvb run status=Failed code=${Mutation.failure} ` +
                'instructions=[0-9]+\\r?\\n$',
                'u',
            ).test(Execution.stderr)) {
            Reject(`${Mutation.name} did not fail with WVR${Mutation.failure}.`);
        }
    }

    const Digest = createHash('sha256').update(Runtime).digest('hex');
    console.log(
        'language 1 vector-sequence runtime status=Passed ' +
        `cases=${Mutations.length + 2} result=42 bytes=${Runtime.length} ` +
        `sha256=${Digest}`,
    );
} finally {
    for (const Candidate of Created) {
        await unlink(Candidate).catch(() => {});
    }
}

function Buildˉruntime(Input) {
    const Sections = Inspectˉmodule(Input, 18);
    const Functions = Inspectˉfunctions(Input, Sections.get(4));
    if (Functions.entries.length !== 3 ||
        Functions.entries[0].name !== 'Acceptˉsequence' ||
        Functions.entries[1].name !== 'Acceptˉvector' ||
        Functions.entries[2].name !== 'Main' ||
        Functions.entries[2].codeOffset !== 24) {
        Reject('The source types fixture function directory differs.');
    }
    const Code = Sections.get(5);
    if (Functions.entries[2].codeOffset > Code.payload.length) {
        Reject('The source types fixture code prefix differs.');
    }

    const Runtimeˉcode = Buildˉruntimeˉcode();
    const Mainˉentry = Buffer.concat([
        Stringˉfield('Main'),
        U32(0),
        Shape(1),
        U32(5),
        Shape(1),
        Shape(23, 0),
        Shape(24, 1),
        Shape(10),
        Shape(23, 0),
        U32(Functions.entries[2].codeOffset),
        U32(Runtimeˉcode.length),
        U32(3),
    ]);
    const Functionˉpayload = Buffer.concat([
        U32(3),
        Functions.entries[0].bytes,
        Functions.entries[1].bytes,
        Mainˉentry,
    ]);
    const Codeˉpayload = Buffer.concat([
        Code.payload.subarray(0, Functions.entries[2].codeOffset),
        Runtimeˉcode,
    ]);
    const Header = Buffer.alloc(12);
    Header.write('WVB1', 0, 'ascii');
    Header.writeUInt16LE(1, 4);
    Header.writeUInt16LE(20, 6);
    Header.writeUInt32LE(7, 8);
    const Payloads = new Map();
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        Payloads.set(Kind, Sections.get(Kind).payload);
    }
    Payloads.set(4, Functionˉpayload);
    Payloads.set(5, Codeˉpayload);
    const Result = [Header];
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        Result.push(Section(Kind, Payloads.get(Kind)));
    }
    return Buffer.concat(Result);
}

function Buildˉruntimeˉcode() {
    const Operations = [];
    for (let Cycle = 0; Cycle < 6; Cycle += 1) {
        Operations.push(Instructionˉu64(129, 2047n));
        Operations.push(Instructionˉu32(199, 0));
        Operations.push(Instructionˉu32(5, 1));
        Operations.push(Instructionˉu32(205, 1));
        Operations.push(Instructionˉu32(1, 20));
        Operations.push(Instructionˉu32(200, 0));
        Operations.push(Instructionˉu32(5, 1));
        Operations.push(Instructionˉu32(205, 1));
        Operations.push(Instructionˉu32(1, 22));
        Operations.push(Instructionˉu32(200, 0));
        Operations.push(Instructionˉu32(202, 0));
        Operations.push(Instructionˉu32(5, 3));
        Operations.push(Instructionˉu32(5, 1));
        Operations.push(Instructionˉu32(205, 1));
        Operations.push(Buffer.concat([U8(201), U32(0), U32(1)]));
        Operations.push(Instructionˉu32(203, 1));
        Operations.push(Instructionˉu32(5, 3));
        Operations.push(Instructionˉu64(129, 1n));
        Operations.push(Instructionˉu32(204, 1));
        Operations.push(Instructionˉu32(1, 20));
        Operations.push(U8(16));
        Operations.push(Instructionˉu32(5, 0));
        Operations.push(U8(80));
    }
    Operations.push(Instructionˉu32(4, 0));
    Operations.push(U8(81));
    return Buffer.concat(Operations);
}

function Inspectˉmodule(Input, Minor) {
    if (Input.length < 12 || Input.subarray(0, 4).toString('ascii') !== 'WVB1' ||
        Input.readUInt16LE(4) !== 1 || Input.readUInt16LE(6) !== Minor ||
        Input.readUInt32LE(8) !== 7) {
        Reject(`The types fixture is not canonical WVB 1.${Minor}.`);
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
        Sections.set(Kind, {
            payload: Buffer.from(Input.subarray(Payload, Payload + Length)),
        });
        Cursor = Payload + Length;
    }
    if (Cursor !== Input.length) Reject('The types fixture has trailing bytes.');
    return Sections;
}

function Inspectˉfunctions(Input, Sectionˉvalue) {
    const Payload = Sectionˉvalue.payload;
    let Cursor = 0;
    const Count = Readˉu32(Payload, Cursor, 'function count');
    Cursor += 4;
    const Entries = [];
    for (let Index = 0; Index < Count; Index += 1) {
        const Start = Cursor;
        const Nameˉlength = Readˉu32(Payload, Cursor, 'function name');
        Cursor += 4;
        Requireˉrange(Payload, Cursor, Nameˉlength, 'function name bytes');
        const Name = Payload.subarray(Cursor, Cursor + Nameˉlength).toString('utf8');
        Cursor += Nameˉlength;
        const Parameters = Readˉu32(Payload, Cursor, 'parameter count');
        Cursor += 4;
        for (let Parameter = 0; Parameter < Parameters; Parameter += 1) {
            Cursor = Shapeˉend(Payload, Cursor);
        }
        Cursor = Shapeˉend(Payload, Cursor);
        const Locals = Readˉu32(Payload, Cursor, 'local count');
        Cursor += 4;
        for (let Local = 0; Local < Locals; Local += 1) {
            Cursor = Shapeˉend(Payload, Cursor);
        }
        Requireˉrange(Payload, Cursor, 12, 'function code metadata');
        const Codeˉoffset = Payload.readUInt32LE(Cursor);
        const Codeˉlength = Payload.readUInt32LE(Cursor + 4);
        Cursor += 12;
        Entries.push({
            name: Name,
            codeOffset: Codeˉoffset,
            codeLength: Codeˉlength,
            bytes: Buffer.from(Payload.subarray(Start, Cursor)),
        });
    }
    if (Cursor !== Payload.length) Reject('The function directory has trailing bytes.');
    return { entries: Entries };
}

function Shapeˉend(Input, Cursor) {
    Requireˉrange(Input, Cursor, 1, 'value shape');
    const Kind = Input[Cursor];
    const Width = Kind === 7 || Kind === 8 || Kind === 11 || Kind === 22 ||
        Kind === 23 || Kind === 24 ? 5 : 1;
    Requireˉrange(Input, Cursor, Width, 'value shape');
    return Cursor + Width;
}

function Section(Kind, Payload) {
    const Header = Buffer.alloc(8);
    Header[0] = Kind;
    Header.writeUInt32LE(Payload.length, 4);
    return Buffer.concat([Header, Payload]);
}

function Stringˉfield(Value) {
    const Bytes = Buffer.from(Value, 'utf8');
    return Buffer.concat([U32(Bytes.length), Bytes]);
}

function Shape(Kind, Nominal = 0) {
    if (Kind === 7 || Kind === 8 || Kind === 11 || Kind === 22 ||
        Kind === 23 || Kind === 24) {
        return Buffer.concat([U8(Kind), U32(Nominal)]);
    }
    return U8(Kind);
}

function Instructionˉu32(Opcode, Value) {
    return Buffer.concat([U8(Opcode), U32(Value)]);
}

function Instructionˉu64(Opcode, Value) {
    return Buffer.concat([U8(Opcode), U64(Value)]);
}

function U8(Value) {
    return Buffer.from([Value]);
}

function U32(Value) {
    const Result = Buffer.alloc(4);
    Result.writeUInt32LE(Value);
    return Result;
}

function U64(Value) {
    const Result = Buffer.alloc(8);
    Result.writeBigUInt64LE(Value);
    return Result;
}

function Readˉu32(Input, Offset, Label) {
    Requireˉrange(Input, Offset, 4, Label);
    return Input.readUInt32LE(Offset);
}

function Findˉfirst(Input, Pattern) {
    const Offset = Input.indexOf(Pattern);
    if (Offset < 0) Reject(`The mutation pattern ${Pattern.toString('hex')} is absent.`);
    return Offset;
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
        Result.stdout.replaceAll('\r\n', '\n') !== Output ||
        Result.stderr.length !== 0) {
        Reject(
            `The valid ${Label} execution differs: status=${Result.status} ` +
            `stdout=${JSON.stringify(Result.stdout)} ` +
            `stderr=${JSON.stringify(Result.stderr)}.`,
        );
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
    if (process.platform === 'win32') {
        return Left.toLowerCase() === Right.toLowerCase();
    }
    return Left === Right;
}

function Reject(Message) {
    throw new Error(Message);
}
