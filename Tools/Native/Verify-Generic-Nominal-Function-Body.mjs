import { createHash } from 'node:crypto';
import { lstat, readFile, realpath } from 'node:fs/promises';
import path from 'node:path';

const MAXIMUM_ARTIFACT_BYTES = 1_048_576;
const PRIVATE_TYPE_SHAPE = 0x8000_0000;
const ABSENT = 0xffff_ffff;

function Reject(Message) {
    throw new Error(Message);
}

function Requireˉvalue(Actual, Expected, Label) {
    if (Actual !== Expected) {
        Reject(`${Label} differs: expected ${Expected}, found ${Actual}.`);
    }
}

function Requireˉarray(Actual, Expected, Label) {
    Requireˉvalue(Actual.length, Expected.length, `${Label} length`);
    for (let Index = 0; Index < Expected.length; Index++) {
        Requireˉvalue(Actual[Index], Expected[Index], `${Label} item ${Index}`);
    }
}

function Requireˉmagic(Input, Expected, Label) {
    Requireˉvalue(
        Input.subarray(0, Expected.length).toString('ascii'),
        Expected,
        `${Label} magic`,
    );
}

function Requireˉhash(Input, Expected, Label) {
    Requireˉvalue(
        createHash('sha256').update(Input).digest('hex'),
        Expected,
        `${Label} SHA-256`,
    );
}

async function Readˉartifact(Candidate, Extension, Label) {
    const Absolute = path.resolve(Candidate);
    const Information = await lstat(Absolute).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > MAXIMUM_ARTIFACT_BYTES ||
        path.extname(Absolute).toLowerCase() !== Extension) {
        Reject(`${Label} is not a bounded ordinary ${Extension} file.`);
    }
    if (await realpath(Absolute) !== Absolute) {
        Reject(`${Label} does not use its canonical path.`);
    }
    return readFile(Absolute);
}

if (process.argv.length !== 7) {
    Reject(
        'Usage: node Verify-Generic-Nominal-Function-Body.mjs ' +
        '<source.wvss> <manifest.wvca> <bindings.wvlb> <wir.wvir> <module.wvb>',
    );
}

const Source = await Readˉartifact(process.argv[2], '.wvss', 'source set');
const Manifest = await Readˉartifact(process.argv[3], '.wvca', 'manifest');
const Bindings = await Readˉartifact(process.argv[4], '.wvlb', 'bindings');
const Wir = await Readˉartifact(process.argv[5], '.wvir', 'WIR');
const Wvb = await Readˉartifact(process.argv[6], '.wvb', 'WVB');

Requireˉmagic(Source, 'WVSS', 'source set');
Requireˉvalue(Source.length, 467, 'source-set bytes');
Requireˉhash(
    Source,
    'fc1a0e322b923d36e26e5bad5a26d46cb4cde77ab9b6171ed00e2f408e1dcf31',
    'source set',
);

Requireˉmagic(Manifest, 'WVCA', 'analysis manifest');
Requireˉvalue(Manifest.length, 104, 'manifest bytes');
Requireˉvalue(Manifest.readUInt32LE(12), Source.length, 'manifest source bytes');
Requireˉvalue(Manifest.readUInt32LE(16), Bindings.length, 'manifest binding bytes');
Requireˉvalue(Manifest.readUInt32LE(20), Wir.length, 'manifest WIR bytes');
Requireˉhash(
    Manifest,
    'efaaf04d4676ceadaab35b1b3269cf7b1bb71e7835e3fe7c52b2d0a6a5308202',
    'analysis manifest',
);

Requireˉmagic(Bindings, 'WVLB', 'binding directory');
Requireˉvalue(Bindings.length, 504, 'binding-directory bytes');
Requireˉvalue(Bindings.readUInt16LE(6), 3, 'binding minor version');
Requireˉvalue(Bindings.readUInt32LE(8), 5, 'binding entry count');
Requireˉvalue(Bindings.readUInt32LE(16), 7, 'binding range count');
Requireˉvalue(Bindings.readUInt32LE(24), 104, 'WVGC bytes');
Requireˉvalue(Bindings.readUInt32LE(28), 68, 'WVGT bytes');
Requireˉvalue(Bindings.readUInt32LE(32), 2, 'combined catalog layout');
Requireˉvalue(Bindings.readUInt32LE(36), 0, 'binding reserved field');

const Rangeˉoffset = 40;
Requireˉarray(
    [0, 1, 2, 3].map(Index => Bindings.readUInt32LE(Rangeˉoffset + Index * 16 + 12)),
    [ABSENT, ABSENT, ABSENT, ABSENT],
    'ordinary range instances',
);
Requireˉarray(
    Array.from({ length: 4 }, (_, Field) =>
        Bindings.readUInt32LE(Rangeˉoffset + 5 * 16 + Field * 4)),
    [2, 2, 2, 0],
    'Wrap specialization range',
);
Requireˉarray(
    Array.from({ length: 4 }, (_, Field) =>
        Bindings.readUInt32LE(Rangeˉoffset + 6 * 16 + Field * 4)),
    [4, 1, 3, 1],
    'Read specialization range',
);

const Functionˉcatalog = Rangeˉoffset + 7 * 16;
Requireˉvalue(
    Bindings.subarray(Functionˉcatalog, Functionˉcatalog + 4).toString('ascii'),
    'WVGC',
    'function catalog magic',
);
Requireˉvalue(Bindings.readUInt32LE(Functionˉcatalog + 8), 2, 'WVGC instances');
Requireˉvalue(Bindings.readUInt32LE(Functionˉcatalog + 12), 1, 'WVGC depth');
Requireˉvalue(Bindings.readUInt32LE(Functionˉcatalog + 16), 104, 'WVGC retained bytes');
for (let Instance = 0; Instance < 2; Instance++) {
    const Entry = Functionˉcatalog + 24 + Instance * 40;
    Requireˉvalue(Bindings.readUInt32LE(Entry), Instance + 2, `WVGC declaration ${Instance}`);
    Requireˉvalue(Bindings.readUInt32LE(Entry + 20), 1, `WVGC arguments ${Instance}`);
    Requireˉarray(
        Array.from({ length: 4 }, (_, Field) => Bindings.readUInt32LE(Entry + 24 + Field * 4)),
        [1, 0, 65_537, 0],
        `WVGC Point argument ${Instance}`,
    );
}

const Typeˉcatalog = Functionˉcatalog + 104;
Requireˉvalue(
    Bindings.subarray(Typeˉcatalog, Typeˉcatalog + 4).toString('ascii'),
    'WVGT',
    'type catalog magic',
);
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 8), 1, 'WVGT instances');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 12), 1, 'WVGT depth');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 16), 68, 'WVGT retained bytes');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 24), 1, 'Box declaration');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 28), 4, 'Box record kind');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 48), 1, 'Box parameter count');
Requireˉarray(
    Array.from({ length: 4 }, (_, Field) =>
        Bindings.readUInt32LE(Typeˉcatalog + 52 + Field * 4)),
    [1, 0, 65_537, 0],
    'Box Point argument',
);

const Bindingˉoffset = Typeˉcatalog + 68;
Requireˉarray(
    Array.from({ length: 5 }, (_, Index) => Bindings.readUInt32LE(Bindingˉoffset + Index * 36 + 24)),
    [PRIVATE_TYPE_SHAPE, 65_537, 65_537, PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE],
    'concrete binding shapes',
);
Requireˉhash(
    Bindings,
    '7f2a71995ae00bc9a56d01ec9543dc2a1bcfa4df5f08396822398a1010dafcb4',
    'binding directory',
);

Requireˉmagic(Wir, 'WVIR', 'WIR directory');
Requireˉvalue(Wir.length, 1_040, 'WIR bytes');
Requireˉvalue(Wir.readUInt16LE(6), 4, 'WIR minor version');
Requireˉarray(
    [8, 16, 24, 32, 40, 48, 52].map(Offset => Wir.readUInt32LE(Offset)),
    [7, 3, 15, 12, 9, 2, 1],
    'WIR directory counts',
);

const Functionˉoffset = 56;
for (let Entry = 0; Entry < 4; Entry++) {
    for (let Field = 0; Field < 12; Field++) {
        Requireˉvalue(
            Wir.readUInt32LE(Functionˉoffset + Entry * 48 + Field * 4),
            0,
            `generic template placeholder ${Entry}.${Field}`,
        );
    }
}
Requireˉarray(
    Array.from({ length: 12 }, (_, Field) =>
        Wir.readUInt32LE(Functionˉoffset + 5 * 48 + Field * 4)),
    [0, 1, 1, 9, 4, 7, 3, 6, 2, 1, 1, PRIVATE_TYPE_SHAPE],
    'Wrap Point WIR function',
);
Requireˉarray(
    Array.from({ length: 12 }, (_, Field) =>
        Wir.readUInt32LE(Functionˉoffset + 6 * 48 + Field * 4)),
    [0, 2, 1, 13, 2, 10, 2, 8, 1, 1, 0, 65_537],
    'Read Point WIR function',
);

const Operationˉoffset = Functionˉoffset + 7 * 48 + 3 * 28;
const Operationˉkinds = Array.from(
    { length: 15 },
    (_, Index) => Wir.readUInt32LE(Operationˉoffset + Index * 32 + 4),
);
Requireˉarray(
    Operationˉkinds,
    [1, 17, 62, 8, 7, 62, 8, 7, 18, 7, 17, 8, 7, 7, 18],
    'WIR operation kinds',
);
Requireˉarray(
    [2, 4, 10, 12, 13, 14].map(Index =>
        Wir.readUInt32LE(Operationˉoffset + Index * 32 + 8)),
    [PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE,
        PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE, 65_537],
    'generic nominal WIR result shapes',
);
Requireˉarray(
    [2, 5].map(Index => Wir.readUInt32LE(Operationˉoffset + Index * 32 + 24)),
    [5, 6],
    'specialized WIR call targets',
);
Requireˉvalue(
    Wir.readUInt32LE(Operationˉoffset + 10 * 32 + 24),
    PRIVATE_TYPE_SHAPE,
    'Wrap Box construction target',
);
Requireˉvalue(
    Wir.readUInt32LE(Operationˉoffset + 14 * 32 + 24),
    PRIVATE_TYPE_SHAPE,
    'Read Box field target',
);
Requireˉhash(
    Wir,
    'df23d2a64848c9f21bbfc92d6ae1ac41b859449fd09b8353705d0b6b6486d3ea',
    'WIR directory',
);

Requireˉmagic(Wvb, 'WVB1', 'WVB');
Requireˉvalue(Wvb.length, 600, 'WVB bytes');
Requireˉvalue(Wvb.readUInt16LE(6), 11, 'WVB minor version');
Requireˉvalue(Wvb.readUInt32LE(8), 7, 'WVB section count');
Requireˉhash(
    Wvb,
    'a27f28ed39ba407c196f461723d1232563372e7684203ee29e151fdb383dacc6',
    'WVB',
);

const Sections = [];
let Sectionˉcursor = 12;
for (let Kind = 1; Kind <= 7; Kind++) {
    Requireˉvalue(Wvb.readUInt32LE(Sectionˉcursor), Kind, `WVB section ${Kind}`);
    const Length = Wvb.readUInt32LE(Sectionˉcursor + 4);
    const Payload = Sectionˉcursor + 8;
    if (Payload + Length > Wvb.length) { Reject(`WVB section ${Kind} is truncated.`); }
    Sections[Kind] = { Payload, Length };
    Sectionˉcursor = Payload + Length;
}
Requireˉvalue(Sectionˉcursor, Wvb.length, 'WVB canonical length');

function Readˉstring(Cursor, Label) {
    const Length = Wvb.readUInt32LE(Cursor);
    const Start = Cursor + 4;
    if (Start + Length > Wvb.length) { Reject(`${Label} is truncated.`); }
    return { Value: Wvb.subarray(Start, Start + Length).toString('utf8'), Cursor: Start + Length };
}

function Readˉshape(Cursor, Label) {
    const Kind = Wvb[Cursor++];
    let Target = null;
    if (Kind === 7 || Kind === 8 || Kind === 11) {
        Target = Wvb.readUInt32LE(Cursor);
        Cursor += 4;
    } else if (Kind === 12 || Kind === 13) {
        const Element = Readˉshape(Cursor, `${Label} element`);
        Cursor = Element.Cursor + 4;
    }
    return { Kind, Target, Cursor };
}

let Directoryˉcursor = Sections[4].Payload;
Requireˉvalue(Wvb.readUInt32LE(Directoryˉcursor), 3, 'optimized WVB functions');
Directoryˉcursor += 4;
const Functions = [];
for (let Index = 0; Index < 3; Index++) {
    const Name = Readˉstring(Directoryˉcursor, `function ${Index} name`);
    Directoryˉcursor = Name.Cursor;
    const Parameterˉcount = Wvb.readUInt32LE(Directoryˉcursor);
    Directoryˉcursor += 4;
    const Parameters = [];
    for (let Parameter = 0; Parameter < Parameterˉcount; Parameter++) {
        const Shape = Readˉshape(Directoryˉcursor, `function ${Index} parameter`);
        Parameters.push(Shape);
        Directoryˉcursor = Shape.Cursor;
    }
    const Return = Readˉshape(Directoryˉcursor, `function ${Index} return`);
    Directoryˉcursor = Return.Cursor;
    const Localˉcount = Wvb.readUInt32LE(Directoryˉcursor);
    Directoryˉcursor += 4;
    const Locals = [];
    for (let Local = 0; Local < Localˉcount; Local++) {
        const Shape = Readˉshape(Directoryˉcursor, `function ${Index} local`);
        Locals.push(Shape);
        Directoryˉcursor = Shape.Cursor;
    }
    const Codeˉoffset = Wvb.readUInt32LE(Directoryˉcursor);
    const Codeˉlength = Wvb.readUInt32LE(Directoryˉcursor + 4);
    Directoryˉcursor += 12;
    Functions.push({ Name: Name.Value, Parameters, Return, Locals, Codeˉoffset, Codeˉlength });
}
Requireˉvalue(Directoryˉcursor, Sections[4].Payload + Sections[4].Length, 'function directory');
Requireˉarray(Functions.map(Function => Function.Name), ['Main', '__Generic_000000', '__Generic_000001'], 'function names');
Requireˉarray(Functions.map(Function => Function.Codeˉlength), [116, 51, 31], 'function code lengths');
Requireˉarray(
    [Functions[1].Parameters[0].Target, Functions[1].Return.Target,
        Functions[2].Parameters[0].Target, Functions[2].Return.Target],
    [0, 1, 1, 0],
    'materialized generic signatures',
);

const Code = Sections[5].Payload;
Requireˉvalue(Sections[5].Length, 198, 'WVB code bytes');
Requireˉvalue(Wvb[Code + Functions[0].Codeˉoffset + 30], 64, 'Wrap call opcode');
Requireˉvalue(Wvb.readUInt32LE(Code + 31), 1, 'Wrap call target');
Requireˉvalue(Wvb[Code + Functions[0].Codeˉoffset + 65], 64, 'Read call opcode');
Requireˉvalue(Wvb.readUInt32LE(Code + 66), 2, 'Read call target');
Requireˉvalue(Wvb[Code + Functions[1].Codeˉoffset + 15], 104, 'Box construction opcode');
Requireˉvalue(Wvb.readUInt32LE(Code + Functions[1].Codeˉoffset + 16), 1, 'Box type target');
Requireˉvalue(Wvb[Code + Functions[2].Codeˉoffset + 15], 105, 'Box field opcode');
Requireˉvalue(Wvb.readUInt32LE(Code + Functions[2].Codeˉoffset + 16), 0, 'Box field target');

let Typeˉcursor = Sections[7].Payload;
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 2, 'concrete WVB nominal types');
Typeˉcursor += 4;
const Typeˉnames = [];
for (let Index = 0; Index < 2; Index++) {
    Requireˉvalue(Wvb[Typeˉcursor++], 1, `type ${Index} record kind`);
    const Name = Readˉstring(Typeˉcursor, `type ${Index} name`);
    Typeˉnames.push(Name.Value);
    Typeˉcursor = Name.Cursor;
    Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 1, `type ${Index} fields`);
    Typeˉcursor += 4;
    const Field = Readˉstring(Typeˉcursor, `type ${Index} field`);
    Typeˉcursor = Field.Cursor;
    const Shape = Readˉshape(Typeˉcursor, `type ${Index} field shape`);
    Typeˉcursor = Shape.Cursor;
}
Requireˉarray(Typeˉnames, ['Point', '__WvY0000'], 'materialized type names');
Requireˉvalue(Typeˉcursor, Sections[7].Payload + Sections[7].Length, 'type directory');

process.stdout.write(
    'generic nominal function body status=Passed cases=28 ' +
    'wvlb-bytes=504 wvir-bytes=1040 wvb-bytes=600 ' +
    'function-specializations=2 generic-types=1 template-types=0 execution=42\n',
);
