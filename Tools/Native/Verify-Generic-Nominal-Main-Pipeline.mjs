import { createHash } from 'node:crypto';
import { lstat, readFile, realpath } from 'node:fs/promises';
import path from 'node:path';

const MAXIMUM_ARTIFACT_BYTES = 1_048_576;
const PRIVATE_TYPE_SHAPE = 0x8000_0000;

function Reject(Message) {
    throw new Error(Message);
}

function Requireˉvalue(Actual, Expected, Label) {
    if (Actual !== Expected) {
        Reject(`${Label} differs: expected ${Expected}, found ${Actual}.`);
    }
}

function Requireˉmagic(Input, Expected, Label) {
    if (Input.subarray(0, Expected.length).toString('ascii') !== Expected) {
        Reject(`${Label} magic differs.`);
    }
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
        'Usage: node Verify-Generic-Nominal-Main-Pipeline.mjs ' +
        '<source.wvss> <manifest.wvca> <bindings.wvlb> <wir.wvir> <module.wvb>'
    );
}

const Source = await Readˉartifact(process.argv[2], '.wvss', 'source set');
const Manifest = await Readˉartifact(process.argv[3], '.wvca', 'manifest');
const Bindings = await Readˉartifact(process.argv[4], '.wvlb', 'bindings');
const Wir = await Readˉartifact(process.argv[5], '.wvir', 'WIR');
const Wvb = await Readˉartifact(process.argv[6], '.wvb', 'WVB');

Requireˉmagic(Source, 'WVSS', 'source set');
Requireˉvalue(Source.length, 377, 'source-set bytes');
Requireˉvalue(Source.readUInt16LE(4), 1, 'source-set major version');
Requireˉvalue(Source.readUInt32LE(8), 1, 'source-set module count');
Requireˉvalue(Source.readUInt32LE(16), 24, 'source-set module offset');
Requireˉvalue(Source.readUInt32LE(20), 353, 'source-set source bytes');

Requireˉmagic(Manifest, 'WVCA', 'analysis manifest');
Requireˉvalue(Manifest.length, 104, 'manifest bytes');
Requireˉvalue(Manifest.readUInt32LE(12), Source.length, 'manifest source bytes');
Requireˉvalue(Manifest.readUInt32LE(16), Bindings.length, 'manifest binding bytes');
Requireˉvalue(Manifest.readUInt32LE(20), Wir.length, 'manifest WIR bytes');

Requireˉmagic(Bindings, 'WVLB', 'binding directory');
Requireˉvalue(Bindings.length, 244, 'binding-directory bytes');
Requireˉvalue(Bindings.readUInt16LE(6), 3, 'binding minor version');
Requireˉvalue(Bindings.readUInt32LE(8), 2, 'binding entry count');
Requireˉvalue(Bindings.readUInt32LE(12), 36, 'binding entry bytes');
Requireˉvalue(Bindings.readUInt32LE(16), 4, 'binding range count');
Requireˉvalue(Bindings.readUInt32LE(20), 16, 'binding range bytes');
Requireˉvalue(Bindings.readUInt32LE(24), 0, 'function catalog bytes');
Requireˉvalue(Bindings.readUInt32LE(28), 68, 'type catalog bytes');
Requireˉvalue(Bindings.readUInt32LE(32), 2, 'catalog layout version');
Requireˉvalue(Bindings.readUInt32LE(36), 0, 'binding reserved field');

const Typeˉcatalogˉoffset = 40 + Bindings.readUInt32LE(16) * 16;
Requireˉvalue(
    Bindings.subarray(Typeˉcatalogˉoffset, Typeˉcatalogˉoffset + 4)
        .toString('ascii'),
    'WVGT',
    'type catalog magic'
);
Requireˉvalue(Bindings.readUInt16LE(Typeˉcatalogˉoffset + 4), 1, 'WVGT major version');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 8), 1, 'WVGT instances');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 12), 1, 'WVGT depth');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 16), 68, 'WVGT retained bytes');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 24), 1, 'WVGT declaration');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 28), 4, 'WVGT declaration kind');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 48), 1, 'WVGT parameters');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalogˉoffset + 52), 1, 'WVGT argument kind');
Requireˉvalue(
    Bindings.readUInt32LE(Typeˉcatalogˉoffset + 60),
    65_537,
    'WVGT ordinary Point shape'
);

const Bindingˉoffset = Typeˉcatalogˉoffset + Bindings.readUInt32LE(28);
Requireˉvalue(Bindings.readUInt32LE(Bindingˉoffset + 4), 2, 'parameter function');
Requireˉvalue(Bindings.readUInt32LE(Bindingˉoffset + 8), 1, 'parameter binding kind');
Requireˉvalue(
    Bindings.readUInt32LE(Bindingˉoffset + 24),
    PRIVATE_TYPE_SHAPE,
    'parameter private shape'
);
const Localˉbindingˉoffset = Bindingˉoffset + Bindings.readUInt32LE(12);
Requireˉvalue(Bindings.readUInt32LE(Localˉbindingˉoffset + 4), 3, 'local function');
Requireˉvalue(Bindings.readUInt32LE(Localˉbindingˉoffset + 8), 2, 'local binding kind');
Requireˉvalue(Bindings.readUInt32LE(Localˉbindingˉoffset + 12), 0, 'local slot');
Requireˉvalue(
    Bindings.readUInt32LE(Localˉbindingˉoffset + 24),
    PRIVATE_TYPE_SHAPE,
    'local private shape'
);

Requireˉmagic(Wir, 'WVIR', 'WIR directory');
Requireˉvalue(Wir.length, 604, 'WIR bytes');
Requireˉvalue(Wir.readUInt16LE(6), 9, 'WIR minor version');
Requireˉvalue(Wir.readUInt32LE(8), 4, 'WIR function entries');
Requireˉvalue(Wir.readUInt32LE(16), 2, 'WIR blocks');
Requireˉvalue(Wir.readUInt32LE(24), 9, 'WIR operations');
Requireˉvalue(Wir.readUInt32LE(28), 28, 'WIR operation entry bytes');
Requireˉvalue(Wir.readUInt32LE(32), 8, 'WIR temporaries');
Requireˉvalue(Wir.readUInt32LE(40), 6, 'WIR operands');

const Identityˉfunction = 48 + 2 * 48;
Requireˉvalue(Wir.readUInt32LE(Identityˉfunction + 36), 1, 'identity parameters');
Requireˉvalue(
    Wir.readUInt32LE(Identityˉfunction + 44),
    PRIVATE_TYPE_SHAPE,
    'identity return shape'
);
const Mainˉfunction = Identityˉfunction + 48;
Requireˉvalue(Wir.readUInt32LE(Mainˉfunction + 44), 1, 'main return shape');

const Operationˉoffset = 48 + 4 * 48 + 2 * 28;
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset), 0, 'parameter-load block');
Requireˉvalue(Wir.readUInt16LE(Operationˉoffset + 4), 7, 'parameter-load operation');
Requireˉvalue(
    Wir.readUInt32LE(Operationˉoffset + 8),
    PRIVATE_TYPE_SHAPE,
    'parameter-load shape'
);
Requireˉvalue(Wir.readUInt16LE(Operationˉoffset + 28 + 4), 1, 'constant operation');
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset + 28 + 8), 1, 'constant shape');
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset + 28 + 20), 42, 'constant value');

const Pointˉconstruction = Operationˉoffset + 2 * 28;
Requireˉvalue(Wir.readUInt16LE(Pointˉconstruction + 4), 17, 'Point construction');
Requireˉvalue(Wir.readUInt32LE(Pointˉconstruction + 8), 65_537, 'Point result shape');
Requireˉvalue(Wir.readUInt32LE(Pointˉconstruction + 20), 1, 'Point nominal target');

const Boxˉconstruction = Operationˉoffset + 3 * 28;
Requireˉvalue(Wir.readUInt16LE(Boxˉconstruction + 4), 17, 'Box construction');
Requireˉvalue(
    Wir.readUInt32LE(Boxˉconstruction + 8),
    PRIVATE_TYPE_SHAPE,
    'Box result shape'
);
Requireˉvalue(
    Wir.readUInt32LE(Boxˉconstruction + 20),
    PRIVATE_TYPE_SHAPE,
    'Box private target'
);
Requireˉvalue(Wir.readUInt32LE(Boxˉconstruction + 24), 1, 'Box declaration');

const Identityˉcall = Operationˉoffset + 4 * 28;
Requireˉvalue(Wir.readUInt16LE(Identityˉcall + 4), 62, 'Identity call');
Requireˉvalue(
    Wir.readUInt32LE(Identityˉcall + 8),
    PRIVATE_TYPE_SHAPE,
    'Identity call result shape'
);
Requireˉvalue(Wir.readUInt32LE(Identityˉcall + 20), 2, 'Identity call target');

const Boxˉfield = Operationˉoffset + 7 * 28;
Requireˉvalue(Wir.readUInt16LE(Boxˉfield + 4), 18, 'Box field operation');
Requireˉvalue(Wir.readUInt32LE(Boxˉfield + 8), 65_537, 'Box.Value result shape');
Requireˉvalue(
    Wir.readUInt32LE(Boxˉfield + 20),
    PRIVATE_TYPE_SHAPE,
    'Box.Value private target'
);

const Pointˉfield = Operationˉoffset + 8 * 28;
Requireˉvalue(Wir.readUInt16LE(Pointˉfield + 4), 18, 'Point field operation');
Requireˉvalue(Wir.readUInt32LE(Pointˉfield + 8), 1, 'Point.X result shape');
Requireˉvalue(Wir.readUInt32LE(Pointˉfield + 20), 1, 'Point.X nominal target');

Requireˉmagic(Wvb, 'WVB1', 'WVB');
Requireˉvalue(Wvb.length, 441, 'WVB bytes');
Requireˉvalue(Wvb.readUInt16LE(4), 1, 'WVB major version');
Requireˉvalue(Wvb.readUInt16LE(6), 11, 'WVB minor version');
Requireˉvalue(Wvb.readUInt32LE(8), 7, 'WVB section count');
Requireˉvalue(
    createHash('sha256').update(Wvb).digest('hex'),
    '71c8e08b2a736ebbc2042f4188c8ed813091dfd72ced93226f5467bd507e73ed',
    'WVB SHA-256'
);

const Sections = [];
let Sectionˉcursor = 12;
for (let Kind = 1; Kind <= 7; Kind++) {
    if (Sectionˉcursor + 8 > Wvb.length) {
        Reject(`WVB section ${Kind} is truncated.`);
    }
    Requireˉvalue(Wvb.readUInt32LE(Sectionˉcursor), Kind, `WVB section ${Kind} kind`);
    const Length = Wvb.readUInt32LE(Sectionˉcursor + 4);
    const Payload = Sectionˉcursor + 8;
    if (Payload + Length > Wvb.length) {
        Reject(`WVB section ${Kind} payload is truncated.`);
    }
    Sections[Kind] = { Payload, Length };
    Sectionˉcursor = Payload + Length;
}
Requireˉvalue(Sectionˉcursor, Wvb.length, 'WVB canonical length');
Requireˉvalue(Wvb.readUInt32LE(Sections[4].Payload), 2, 'optimized WVB functions');
Requireˉvalue(Sections[5].Length, 127, 'WVB code bytes');

const Mainˉcode = Sections[5].Payload + 16;
Requireˉvalue(Wvb[Mainˉcode + 15], 0x68, 'Point record.create opcode');
Requireˉvalue(Wvb.readUInt32LE(Mainˉcode + 16), 0, 'Point WVB type target');
Requireˉvalue(Wvb[Mainˉcode + 30], 0x68, 'Box record.create opcode');
Requireˉvalue(Wvb.readUInt32LE(Mainˉcode + 31), 1, 'Box WVB type target');
Requireˉvalue(Wvb[Mainˉcode + 45], 0x40, 'Identity call opcode');
Requireˉvalue(Wvb.readUInt32LE(Mainˉcode + 46), 0, 'Identity call target');
Requireˉvalue(Wvb[Mainˉcode + 80], 0x69, 'Box.Value record.field opcode');
Requireˉvalue(Wvb.readUInt32LE(Mainˉcode + 81), 0, 'Box.Value field target');
Requireˉvalue(Wvb[Mainˉcode + 95], 0x69, 'Point.X record.field opcode');
Requireˉvalue(Wvb.readUInt32LE(Mainˉcode + 96), 0, 'Point.X field target');

let Typeˉcursor = Sections[7].Payload;
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 2, 'WVB concrete nominal types');
Typeˉcursor += 4;

function Readˉstring(Label) {
    if (Typeˉcursor + 4 > Wvb.length) { Reject(`${Label} length is truncated.`); }
    const Length = Wvb.readUInt32LE(Typeˉcursor);
    Typeˉcursor += 4;
    if (Typeˉcursor + Length > Wvb.length) { Reject(`${Label} bytes are truncated.`); }
    const Value = Wvb.subarray(Typeˉcursor, Typeˉcursor + Length).toString('utf8');
    Typeˉcursor += Length;
    return Value;
}

function Requireˉrecord(Name, Field, Shapeˉkind, Shapeˉtarget) {
    Requireˉvalue(Wvb[Typeˉcursor++], 1, `${Name} nominal kind`);
    Requireˉvalue(Readˉstring(`${Name} name`), Name, `${Name} name`);
    Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 1, `${Name} field count`);
    Typeˉcursor += 4;
    Requireˉvalue(Readˉstring(`${Name}.${Field} name`), Field, `${Name}.${Field} name`);
    Requireˉvalue(Wvb[Typeˉcursor++], Shapeˉkind, `${Name}.${Field} shape kind`);
    if (Shapeˉtarget !== null) {
        Requireˉvalue(
            Wvb.readUInt32LE(Typeˉcursor),
            Shapeˉtarget,
            `${Name}.${Field} shape target`
        );
        Typeˉcursor += 4;
    }
}

Requireˉrecord('Point', 'X', 1, null);
Requireˉrecord('__WvY0000', 'Value', 7, 0);
Requireˉvalue(
    Typeˉcursor,
    Sections[7].Payload + Sections[7].Length,
    'WVB Types section consumption'
);

process.stdout.write(
    'generic nominal main pipeline status=Passed cases=20 ' +
    'wvlb-bytes=244 wvir-bytes=604 wvb-bytes=441 ' +
    'types=2 template-types=0 private-shape=0x80000000\n'
);
