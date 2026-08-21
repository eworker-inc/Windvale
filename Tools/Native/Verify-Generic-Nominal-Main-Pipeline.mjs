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
Requireˉvalue(Source.length, 272, 'source-set bytes');
Requireˉvalue(Source.readUInt16LE(4), 1, 'source-set major version');
Requireˉvalue(Source.readUInt32LE(8), 1, 'source-set module count');
Requireˉvalue(Source.readUInt32LE(16), 24, 'source-set module offset');
Requireˉvalue(Source.readUInt32LE(20), 248, 'source-set source bytes');

Requireˉmagic(Manifest, 'WVCA', 'analysis manifest');
Requireˉvalue(Manifest.length, 104, 'manifest bytes');
Requireˉvalue(Manifest.readUInt32LE(12), Source.length, 'manifest source bytes');
Requireˉvalue(Manifest.readUInt32LE(16), Bindings.length, 'manifest binding bytes');
Requireˉvalue(Manifest.readUInt32LE(20), Wir.length, 'manifest WIR bytes');

Requireˉmagic(Bindings, 'WVLB', 'binding directory');
Requireˉvalue(Bindings.length, 208, 'binding-directory bytes');
Requireˉvalue(Bindings.readUInt16LE(6), 3, 'binding minor version');
Requireˉvalue(Bindings.readUInt32LE(8), 1, 'binding entry count');
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

Requireˉmagic(Wir, 'WVIR', 'WIR directory');
Requireˉvalue(Wir.length, 368, 'WIR bytes');
Requireˉvalue(Wir.readUInt16LE(6), 3, 'WIR minor version');
Requireˉvalue(Wir.readUInt32LE(8), 4, 'WIR function entries');
Requireˉvalue(Wir.readUInt32LE(16), 2, 'WIR blocks');
Requireˉvalue(Wir.readUInt32LE(24), 2, 'WIR operations');
Requireˉvalue(Wir.readUInt32LE(28), 32, 'WIR operation entry bytes');
Requireˉvalue(Wir.readUInt32LE(32), 2, 'WIR temporaries');

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
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset + 4), 7, 'parameter-load operation');
Requireˉvalue(
    Wir.readUInt32LE(Operationˉoffset + 8),
    PRIVATE_TYPE_SHAPE,
    'parameter-load shape'
);
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset + 32 + 4), 1, 'constant operation');
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset + 32 + 8), 1, 'constant shape');
Requireˉvalue(Wir.readUInt32LE(Operationˉoffset + 32 + 24), 42, 'constant value');

Requireˉmagic(Wvb, 'WVB1', 'WVB');
Requireˉvalue(Wvb.length, 252, 'WVB bytes');
Requireˉvalue(Wvb.readUInt16LE(4), 1, 'WVB major version');
Requireˉvalue(Wvb.readUInt16LE(6), 11, 'WVB minor version');
Requireˉvalue(Wvb.readUInt32LE(8), 7, 'WVB section count');
Requireˉvalue(
    createHash('sha256').update(Wvb).digest('hex'),
    '8871f2876c9135e8f4f8740f7643d1ff5a5eb0e771da0dddd3357e1bed9d29aa',
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
Requireˉvalue(Wvb.readUInt32LE(Sections[4].Payload), 1, 'optimized WVB functions');

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
    'wvlb-bytes=208 wvir-bytes=368 wvb-bytes=252 ' +
    'types=2 template-types=0 private-shape=0x80000000\n'
);
