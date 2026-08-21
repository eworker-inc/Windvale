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
        'Usage: node Verify-Generic-Nominal-Declaration-Dependency.mjs ' +
        '<source.wvss> <manifest.wvca> <bindings.wvlb> <wir.wvir> <module.wvb>',
    );
}

const Source = await Readˉartifact(process.argv[2], '.wvss', 'source set');
const Manifest = await Readˉartifact(process.argv[3], '.wvca', 'manifest');
const Bindings = await Readˉartifact(process.argv[4], '.wvlb', 'bindings');
const Wir = await Readˉartifact(process.argv[5], '.wvir', 'WIR');
const Wvb = await Readˉartifact(process.argv[6], '.wvb', 'WVB');

Requireˉmagic(Source, 'WVSS', 'source set');
Requireˉvalue(Source.length, 526, 'source-set bytes');
Requireˉhash(
    Source,
    '26860c62d61c6467c6b7120909c8d0cc81a19d5eed9e0742fb6c39958970dee3',
    'source set',
);

Requireˉmagic(Manifest, 'WVCA', 'analysis manifest');
Requireˉvalue(Manifest.length, 104, 'manifest bytes');
Requireˉarray(
    [12, 16, 20].map(Offset => Manifest.readUInt32LE(Offset)),
    [Source.length, Bindings.length, Wir.length],
    'manifest artifact lengths',
);
Requireˉhash(
    Manifest,
    '68c4ef6602a34498c7912f4c51e714e3aa1f8c33fb8cc3665ce249d4be9072e3',
    'analysis manifest',
);

Requireˉmagic(Bindings, 'WVLB', 'binding directory');
Requireˉvalue(Bindings.length, 564, 'binding-directory bytes');
Requireˉvalue(Bindings.readUInt16LE(6), 3, 'binding minor version');
Requireˉarray(
    [8, 16, 24, 28, 32, 36].map(Offset => Bindings.readUInt32LE(Offset)),
    [5, 8, 104, 112, 2, 0],
    'binding directory counts',
);

const Rangeˉoffset = 40;
Requireˉarray(
    Array.from({ length: 5 }, (_, Index) =>
        Bindings.readUInt32LE(Rangeˉoffset + Index * 16 + 12)),
    [ABSENT, ABSENT, ABSENT, ABSENT, ABSENT],
    'template and ordinary range instances',
);
Requireˉarray(
    Array.from({ length: 4 }, (_, Field) =>
        Bindings.readUInt32LE(Rangeˉoffset + 6 * 16 + Field * 4)),
    [2, 2, 3, 0],
    'Wrap specialization range',
);
Requireˉarray(
    Array.from({ length: 4 }, (_, Field) =>
        Bindings.readUInt32LE(Rangeˉoffset + 7 * 16 + Field * 4)),
    [4, 1, 4, 1],
    'Read specialization range',
);

const Functionˉcatalog = Rangeˉoffset + 8 * 16;
Requireˉvalue(
    Bindings.subarray(Functionˉcatalog, Functionˉcatalog + 4).toString('ascii'),
    'WVGC',
    'function catalog magic',
);
Requireˉarray(
    [8, 12, 16].map(Offset => Bindings.readUInt32LE(Functionˉcatalog + Offset)),
    [2, 1, 104],
    'function catalog header',
);
for (let Instance = 0; Instance < 2; Instance++) {
    const Entry = Functionˉcatalog + 24 + Instance * 40;
    Requireˉvalue(
        Bindings.readUInt32LE(Entry),
        Instance + 3,
        `function declaration ${Instance}`,
    );
    Requireˉarray(
        Array.from({ length: 4 }, (_, Field) =>
            Bindings.readUInt32LE(Entry + 24 + Field * 4)),
        [1, 0, 65_538, 0],
        `function Point argument ${Instance}`,
    );
}

const Typeˉcatalog = Functionˉcatalog + 104;
Requireˉvalue(
    Bindings.subarray(Typeˉcatalog, Typeˉcatalog + 4).toString('ascii'),
    'WVGT',
    'type catalog magic',
);
Requireˉarray(
    [8, 12, 16].map(Offset => Bindings.readUInt32LE(Typeˉcatalog + Offset)),
    [2, 1, 112],
    'type catalog header',
);
for (let Instance = 0; Instance < 2; Instance++) {
    const Entry = Typeˉcatalog + 24 + Instance * 44;
    Requireˉarray(
        [0, 4, 8, 24].map(Offset => Bindings.readUInt32LE(Entry + Offset)),
        [Instance + 1, 4, 1, 1],
        `type dependency header ${Instance}`,
    );
    Requireˉarray(
        Array.from({ length: 4 }, (_, Field) =>
            Bindings.readUInt32LE(Entry + 28 + Field * 4)),
        [1, 0, 65_538, 0],
        `type Point argument ${Instance}`,
    );
}

const Concreteˉbindings = Typeˉcatalog + 112;
Requireˉarray(
    Array.from({ length: 5 }, (_, Index) =>
        Bindings.readUInt32LE(Concreteˉbindings + Index * 36 + 24)),
    [PRIVATE_TYPE_SHAPE + 1, 65_538, 65_538,
        PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE + 1],
    'concrete binding shapes',
);
Requireˉhash(
    Bindings,
    '3048dcd632975a54580b3ce5b33d263fcf584bdd097e3f4d9352756c9c1bb48e',
    'binding directory',
);

Requireˉmagic(Wir, 'WVIR', 'WIR directory');
Requireˉvalue(Wir.length, 1_168, 'WIR bytes');
Requireˉvalue(Wir.readUInt16LE(6), 4, 'WIR minor version');
Requireˉarray(
    [8, 16, 24, 32, 40, 48, 52].map(Offset => Wir.readUInt32LE(Offset)),
    [8, 3, 17, 14, 11, 2, 1],
    'WIR directory counts',
);

const Functionˉoffset = 56;
for (let Entry = 0; Entry < 5; Entry++) {
    Requireˉarray(
        Array.from({ length: 12 }, (_, Field) =>
            Wir.readUInt32LE(Functionˉoffset + Entry * 48 + Field * 4)),
        Array(12).fill(0),
        `template placeholder ${Entry}`,
    );
}
Requireˉarray(
    Array.from({ length: 12 }, (_, Field) =>
        Wir.readUInt32LE(Functionˉoffset + 6 * 48 + Field * 4)),
    [0, 1, 1, 9, 5, 7, 4, 6, 3, 1, 1, PRIVATE_TYPE_SHAPE + 1],
    'Wrap Point WIR function',
);
Requireˉarray(
    Array.from({ length: 12 }, (_, Field) =>
        Wir.readUInt32LE(Functionˉoffset + 7 * 48 + Field * 4)),
    [0, 2, 1, 14, 3, 11, 3, 9, 2, 1, 0, 65_538],
    'Read Point WIR function',
);

const Operationˉoffset = Functionˉoffset + 8 * 48 + 3 * 28;
Requireˉarray(
    Array.from({ length: 17 }, (_, Index) =>
        Wir.readUInt32LE(Operationˉoffset + Index * 32 + 4)),
    [1, 17, 62, 8, 7, 62, 8, 7, 18, 7, 17, 8, 7, 17, 7, 18, 18],
    'WIR operation kinds',
);
Requireˉarray(
    [10, 13, 15, 16].map(Index =>
        Wir.readUInt32LE(Operationˉoffset + Index * 32 + 24)),
    [PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE + 1,
        PRIVATE_TYPE_SHAPE + 1, PRIVATE_TYPE_SHAPE],
    'dependency-ordered WIR nominal targets',
);
Requireˉhash(
    Wir,
    '2676ecdbc4e9f930a1d4a467f9b3bbd8460705a60952950b5e77ae0d5ca826f3',
    'WIR directory',
);

Requireˉmagic(Wvb, 'WVB1', 'WVB');
Requireˉvalue(Wvb.length, 668, 'WVB bytes');
Requireˉvalue(Wvb.readUInt16LE(6), 11, 'WVB minor version');
Requireˉhash(
    Wvb,
    '5ec54be82a84a0bea60fd6cb8146c08ddf8fb934aaf9560734250eadd20ee046',
    'WVB',
);

const Sections = [];
let Sectionˉcursor = 12;
for (let Kind = 1; Kind <= 7; Kind++) {
    Requireˉvalue(Wvb.readUInt32LE(Sectionˉcursor), Kind, `WVB section ${Kind}`);
    const Length = Wvb.readUInt32LE(Sectionˉcursor + 4);
    const Payload = Sectionˉcursor + 8;
    if (Payload + Length > Wvb.length) {
        Reject(`WVB section ${Kind} is truncated.`);
    }
    Sections[Kind] = { Payload, Length };
    Sectionˉcursor = Payload + Length;
}
Requireˉvalue(Sectionˉcursor, Wvb.length, 'WVB canonical length');

function Readˉstring(Cursor, Label) {
    const Length = Wvb.readUInt32LE(Cursor);
    const Start = Cursor + 4;
    if (Start + Length > Wvb.length) { Reject(`${Label} is truncated.`); }
    return {
        Value: Wvb.subarray(Start, Start + Length).toString('utf8'),
        Cursor: Start + Length,
    };
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

let Functionˉcursor = Sections[4].Payload;
Requireˉvalue(Wvb.readUInt32LE(Functionˉcursor), 3, 'optimized WVB functions');
Functionˉcursor += 4;
const Functions = [];
for (let Index = 0; Index < 3; Index++) {
    const Name = Readˉstring(Functionˉcursor, `function ${Index} name`);
    Functionˉcursor = Name.Cursor;
    const Parameterˉcount = Wvb.readUInt32LE(Functionˉcursor);
    Functionˉcursor += 4;
    const Parameters = [];
    for (let Parameter = 0; Parameter < Parameterˉcount; Parameter++) {
        const Shape = Readˉshape(Functionˉcursor, `function ${Index} parameter`);
        Parameters.push(Shape);
        Functionˉcursor = Shape.Cursor;
    }
    const Return = Readˉshape(Functionˉcursor, `function ${Index} return`);
    Functionˉcursor = Return.Cursor;
    const Localˉcount = Wvb.readUInt32LE(Functionˉcursor);
    Functionˉcursor += 4;
    for (let Local = 0; Local < Localˉcount; Local++) {
        Functionˉcursor = Readˉshape(
            Functionˉcursor,
            `function ${Index} local ${Local}`,
        ).Cursor;
    }
    const Codeˉlength = Wvb.readUInt32LE(Functionˉcursor + 4);
    Functionˉcursor += 12;
    Functions.push({ Name: Name.Value, Parameters, Return, Codeˉlength });
}
Requireˉarray(
    Functions.map(Function => Function.Name),
    ['Main', '__Generic_000000', '__Generic_000001'],
    'function names',
);
Requireˉarray(
    Functions.map(Function => Function.Codeˉlength),
    [116, 66, 46],
    'function code lengths',
);
Requireˉarray(
    [Functions[1].Parameters[0].Target, Functions[1].Return.Target,
        Functions[2].Parameters[0].Target, Functions[2].Return.Target],
    [0, 2, 2, 0],
    'materialized generic signatures',
);

let Typeˉcursor = Sections[7].Payload;
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 3, 'concrete WVB nominal types');
Typeˉcursor += 4;
const Types = [];
for (let Index = 0; Index < 3; Index++) {
    Requireˉvalue(Wvb[Typeˉcursor++], 1, `type ${Index} record kind`);
    const Name = Readˉstring(Typeˉcursor, `type ${Index} name`);
    Typeˉcursor = Name.Cursor;
    Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 1, `type ${Index} fields`);
    Typeˉcursor += 4;
    const Field = Readˉstring(Typeˉcursor, `type ${Index} field`);
    Typeˉcursor = Field.Cursor;
    const Shape = Readˉshape(Typeˉcursor, `type ${Index} field shape`);
    Typeˉcursor = Shape.Cursor;
    Types.push({ Name: Name.Value, Field: Field.Value, Shape });
}
Requireˉarray(
    Types.map(Type => Type.Name),
    ['Point', '__WvY0000', '__WvY0001'],
    'materialized type names',
);
Requireˉarray(
    Types.map(Type => Type.Field),
    ['X', 'Value', 'Wrapped'],
    'materialized field names',
);
Requireˉarray(
    Types.map(Type => Type.Shape.Target),
    [null, 0, 1],
    'materialized dependency targets',
);
Requireˉvalue(
    Typeˉcursor,
    Sections[7].Payload + Sections[7].Length,
    'type directory',
);

process.stdout.write(
    'generic nominal declaration dependency status=Passed cases=32 ' +
    'wvlb-bytes=564 wvir-bytes=1168 wvb-bytes=668 ' +
    'generic-types=2 materialized-types=3 execution=42\n',
);
