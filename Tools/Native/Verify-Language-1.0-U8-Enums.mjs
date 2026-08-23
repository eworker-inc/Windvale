import {
    mkdirSync,
    readFileSync,
    unlinkSync,
    writeFileSync,
} from 'node:fs';
import { basename, resolve } from 'node:path';
import { spawnSync } from 'node:child_process';

function Reject(Message) {
    throw new Error(Message);
}

if (process.argv.length !== 6) {
    process.stderr.write(
        'Usage: node Tools/Native/Verify-Language-1.0-U8-Enums.mjs ' +
        '<verifier> <runner> <valid.wvb> <work-directory>\n',
    );
    process.exit(64);
}

const Verifier = resolve(process.argv[2]);
const Runner = resolve(process.argv[3]);
const Validˉpath = resolve(process.argv[4]);
const Work = resolve(process.argv[5]);
mkdirSync(Work, { recursive: true });

function Run(Tool, Arguments) {
    const Result = spawnSync(Tool, Arguments, {
        encoding: 'utf8',
        maxBuffer: 65_536,
        timeout: 30_000,
        windowsHide: true,
    });
    if (Result.error !== undefined) {
        throw Result.error;
    }
    return Result;
}

function Requireˉaccepted(Path) {
    const Result = Run(Verifier, [Path]);
    if (Result.status !== 0 || Result.stderr !== '' ||
        Result.stdout.replaceAll('\r\n', '\n') !==
            'wvb status=Valid profile=compiler-aligned\n') {
        Reject(`${basename(Path)} was not accepted by the verifier.`);
    }
}

function Requireˉrejected(Path) {
    const Result = Run(Verifier, [Path]);
    if (Result.status !== 1 || Result.stdout !== '' ||
        !/^wvb status=Invalid phase=[a-z-]+\r?\n$/u.test(Result.stderr)) {
        Reject(`${basename(Path)} was not rejected by the verifier.`);
    }
}

function Sections(Bytes) {
    const Result = [];
    let Cursor = 12;
    for (let Kind = 1; Kind <= 7; Kind += 1) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Kind ||
            Bytes[Cursor + 1] !== 0 ||
            Bytes.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The valid module has no canonical section ${Kind}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) {
            Reject(`The valid module truncates section ${Kind}.`);
        }
        Result[Kind] = { header: Cursor, payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) {
        Reject('The valid module has trailing bytes.');
    }
    return Result;
}

function Readˉname(Bytes, Cursor) {
    const Length = Bytes.readUInt32LE(Cursor);
    const Offset = Cursor + 4;
    return {
        length: Length,
        offset: Offset,
        text: Bytes.subarray(Offset, Offset + Length).toString('utf8'),
        next: Offset + Length,
    };
}

function Findˉfirstˉenumˉshape(Bytes, Functions) {
    const Count = Bytes.readUInt32LE(Functions.payload);
    let Cursor = Functions.payload + 4;
    for (let Functionˉindex = 0;
        Functionˉindex < Count;
        Functionˉindex += 1) {
        Cursor = Readˉname(Bytes, Cursor).next;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Index = 0; Index < Parameterˉcount; Index += 1) {
            const Kind = Bytes[Cursor];
            if (Kind === 8) {
                return Cursor;
            }
            Cursor += [7, 8, 11, 22, 23, 24].includes(Kind) ? 5 : 1;
        }
        const Returnˉkind = Bytes[Cursor];
        if (Returnˉkind === 8) {
            return Cursor;
        }
        Cursor += [7, 8, 11, 22, 23, 24].includes(Returnˉkind) ? 5 : 1;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Index = 0; Index < Localˉcount; Index += 1) {
            const Kind = Bytes[Cursor];
            if (Kind === 8) {
                return Cursor;
            }
            Cursor += [7, 8, 11, 22, 23, 24].includes(Kind) ? 5 : 1;
        }
        Cursor += 12;
    }
    Reject('The valid module has no enum function shape.');
}

const Valid = readFileSync(Validˉpath);
if (Valid.length < 12 || Valid.subarray(0, 4).toString('ascii') !== 'WVB1' ||
    Valid.readUInt16LE(4) !== 1 || Valid.readUInt16LE(6) !== 22 ||
    Valid.readUInt32LE(8) !== 7) {
    Reject(`${basename(Validˉpath)} is not a WVB 1.22 seven-section module.`);
}
const Validˉsections = Sections(Valid);
const Types = Validˉsections[7];
if (Valid.readUInt32LE(Types.payload) !== 1) {
    Reject('The valid module must declare exactly one type.');
}
const Typeˉkind = Types.payload + 4;
if (Valid[Typeˉkind] !== 7) {
    Reject('The valid module does not use the u8 enum descriptor kind.');
}
const Typeˉname = Readˉname(Valid, Typeˉkind + 1);
const Backing = Typeˉname.next;
const Memberˉcount = Backing + 1;
if (Typeˉname.text !== 'Deliveryˉstate' || Valid[Backing] !== 6 ||
    Valid.readUInt32LE(Memberˉcount) !== 2) {
    Reject('The valid module has an unexpected u8 enum descriptor.');
}
const Firstˉmember = Readˉname(Valid, Memberˉcount + 4);
const Firstˉvalue = Firstˉmember.next;
const Secondˉmember = Readˉname(Valid, Firstˉvalue + 1);
const Secondˉvalue = Secondˉmember.next;
if (Firstˉmember.text !== 'Pending' || Valid[Firstˉvalue] !== 1 ||
    Secondˉmember.text !== 'Complete' || Valid[Secondˉvalue] !== 2 ||
    Secondˉvalue + 1 !== Types.payload + Types.length) {
    Reject('The valid module has unexpected u8 enum members.');
}
const Enumˉshape = Findˉfirstˉenumˉshape(Valid, Validˉsections[4]);
if (Valid[Enumˉshape] !== 8 ||
    Valid.readUInt32LE(Enumˉshape + 1) !== 0) {
    Reject('The valid module does not reference its u8 enum nominal type.');
}

Requireˉaccepted(Validˉpath);
const Validˉrun = Run(Runner, [Validˉpath]);
if (Validˉrun.status !== 0 || Validˉrun.stderr !== '' ||
    Validˉrun.stdout.replaceAll('\r\n', '\n') !== 'Result: 42\n') {
    Reject('The valid u8 enum fixture did not return exact result 42.');
}

function Removeˉbyte(Bytes, Offset) {
    const Result = Buffer.concat([
        Bytes.subarray(0, Offset),
        Bytes.subarray(Offset + 1),
    ]);
    Result.writeUInt32LE(
        Result.readUInt32LE(Types.header + 4) - 1,
        Types.header + 4,
    );
    return Result;
}

function Withoutˉu8ˉenum(Bytes) {
    const Name = Bytes.subarray(Typeˉname.offset, Typeˉname.next);
    const Firstˉname = Bytes.subarray(
        Firstˉmember.offset,
        Firstˉmember.next,
    );
    const Secondˉname = Bytes.subarray(
        Secondˉmember.offset,
        Secondˉmember.next,
    );
    const Descriptor = Buffer.alloc(
        1 + 4 + Name.length + 4 +
        4 + Firstˉname.length + 4 +
        4 + Secondˉname.length + 4,
    );
    let Cursor = 0;
    Descriptor[Cursor++] = 2;
    Descriptor.writeUInt32LE(Name.length, Cursor);
    Cursor += 4;
    Name.copy(Descriptor, Cursor);
    Cursor += Name.length;
    Descriptor.writeUInt32LE(2, Cursor);
    Cursor += 4;
    Descriptor.writeUInt32LE(Firstˉname.length, Cursor);
    Cursor += 4;
    Firstˉname.copy(Descriptor, Cursor);
    Cursor += Firstˉname.length;
    Descriptor.writeUInt32LE(1, Cursor);
    Cursor += 4;
    Descriptor.writeUInt32LE(Secondˉname.length, Cursor);
    Cursor += 4;
    Secondˉname.copy(Descriptor, Cursor);
    Cursor += Secondˉname.length;
    Descriptor.writeUInt32LE(2, Cursor);
    const Result = Buffer.concat([
        Bytes.subarray(0, Typeˉkind),
        Descriptor,
        Bytes.subarray(Types.payload + Types.length),
    ]);
    Result.writeUInt32LE(4 + Descriptor.length, Types.header + 4);
    return Result;
}

const Cases = [
    ['old-minor', Bytes => { Bytes.writeUInt16LE(21, 6); }],
    ['future-minor', Bytes => { Bytes.writeUInt16LE(23, 6); }],
    ['wrong-backing', Bytes => { Bytes[Backing] = 5; }],
    ['duplicate-value', Bytes => { Bytes[Secondˉvalue] = 1; }],
    ['truncated-backing', Bytes => Removeˉbyte(Bytes, Backing)],
    ['truncated-value', Bytes => Removeˉbyte(Bytes, Secondˉvalue)],
    ['missing-u8-enum', Bytes => Withoutˉu8ˉenum(Bytes)],
    ['unknown-type-kind', Bytes => { Bytes[Typeˉkind] = 8; }],
    ['unknown-shape-token', Bytes => {
        Bytes.writeUInt32LE(1, Enumˉshape + 1);
    }],
];
const Mutations = [];
try {
    for (const [Name, Mutate] of Cases) {
        const Candidate = Buffer.from(Valid);
        const Mutation = Mutate(Candidate);
        const Mutated = Buffer.isBuffer(Mutation) ? Mutation : Candidate;
        const Path = resolve(Work, `${Name}.wvb`);
        Mutations.push(Path);
        writeFileSync(Path, Mutated);
        Requireˉrejected(Path);
    }
    process.stdout.write(
        `language 1 u8 enum status=Passed valid=1 malformed=${Cases.length} ` +
        'version=1.22 backing=u8 result=42\n',
    );
} finally {
    for (const Path of Mutations) {
        try {
            unlinkSync(Path);
        } catch (Error) {
            if (Error?.code !== 'ENOENT') {
                throw Error;
            }
        }
    }
}
