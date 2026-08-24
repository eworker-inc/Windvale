import { createHash } from 'node:crypto';
import { lstat, readFile, realpath } from 'node:fs/promises';
import path from 'node:path';

const MAXIMUM_ARTIFACT_BYTES = 1_048_576;
const PRIVATE_TYPE_SHAPE = 0x8000_0000;
let Cases = 0;

function Reject(Message) {
    throw new Error(Message);
}

function Requireˉvalue(Actual, Expected, Label) {
    Cases += 1;
    if (Actual !== Expected) {
        Reject(`${Label} differs: expected ${Expected}, found ${Actual}.`);
    }
}

function Requireˉmagic(Input, Expected, Label) {
    Requireˉvalue(
        Input.subarray(0, Expected.length).toString('ascii'),
        Expected,
        `${Label} magic`,
    );
}

function Sha256(Input) {
    return createHash('sha256').update(Input).digest('hex');
}

async function Readˉartifact(Candidate, Extension, Label) {
    const Absolute = path.resolve(Candidate);
    const Information = await lstat(Absolute).catch(() => null);
    if (Information === null || !Information.isFile() ||
        Information.isSymbolicLink() || Information.size < 1 ||
        Information.size > MAXIMUM_ARTIFACT_BYTES ||
        path.extname(Absolute).toLowerCase() !== Extension ||
        await realpath(Absolute) !== Absolute) {
        Reject(`${Label} is not a bounded canonical ${Extension} file.`);
    }
    return readFile(Absolute);
}

if (process.argv.length !== 7) {
    Reject(
        'Usage: node Verify-Generic-Nominal-Variant.mjs ' +
        '<source.wvss> <manifest.wvca> <bindings.wvlb> <wir.wvir> <module.wvb>',
    );
}

const Source = await Readˉartifact(process.argv[2], '.wvss', 'source set');
const Manifest = await Readˉartifact(process.argv[3], '.wvca', 'manifest');
const Bindings = await Readˉartifact(process.argv[4], '.wvlb', 'bindings');
const Wir = await Readˉartifact(process.argv[5], '.wvir', 'WIR');
const Wvb = await Readˉartifact(process.argv[6], '.wvb', 'WVB');

Requireˉmagic(Source, 'WVSS', 'source set');
Requireˉvalue(Source.length, 771, 'source-set bytes');
Requireˉvalue(Source.readUInt16LE(4), 2, 'source-set major version');
Requireˉvalue(Source.readUInt32LE(8), 1, 'source-set module count');
Requireˉvalue(Sha256(Source), '13cd808971100b0b885a11be73952c901676dd303f41a9ba11029b07aae59fb4', 'source-set SHA-256');

Requireˉmagic(Manifest, 'WVCA', 'analysis manifest');
Requireˉvalue(Manifest.length, 104, 'manifest bytes');
Requireˉvalue(Manifest.readUInt32LE(12), Source.length, 'manifest source bytes');
Requireˉvalue(Manifest.readUInt32LE(16), Bindings.length, 'manifest binding bytes');
Requireˉvalue(Manifest.readUInt32LE(20), Wir.length, 'manifest WIR bytes');

Requireˉmagic(Bindings, 'WVLB', 'binding directory');
Requireˉvalue(Bindings.length, 316, 'binding-directory bytes');
Requireˉvalue(Bindings.readUInt16LE(6), 3, 'binding minor version');
Requireˉvalue(Bindings.readUInt32LE(8), 4, 'binding entries');
Requireˉvalue(Bindings.readUInt32LE(12), 36, 'binding entry bytes');
Requireˉvalue(Bindings.readUInt32LE(16), 4, 'binding ranges');
Requireˉvalue(Bindings.readUInt32LE(28), 68, 'generic type-catalog bytes');

const Typeˉcatalog = 40 + Bindings.readUInt32LE(16) * 16;
Requireˉvalue(Bindings.subarray(Typeˉcatalog, Typeˉcatalog + 4).toString('ascii'), 'WVGT', 'type-catalog magic');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 8), 1, 'type-catalog instances');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 24), 1, 'Outcome declaration');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 28), 8, 'Outcome declaration kind');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 48), 1, 'Outcome arguments');
Requireˉvalue(Bindings.readUInt32LE(Typeˉcatalog + 60), 65_536, 'Point argument shape');

const Bindingˉentries = Typeˉcatalog + Bindings.readUInt32LE(28);
Requireˉvalue(Bindings.readUInt32LE(Bindingˉentries + 24), PRIVATE_TYPE_SHAPE, 'Input private shape');
Requireˉvalue(Bindings.readUInt32LE(Bindingˉentries + 36 + 24), 3, 'Attempts binding shape');
Requireˉvalue(Bindings.readUInt32LE(Bindingˉentries + 72 + 24), 65_536, 'Item binding shape');
Requireˉvalue(Bindings.readUInt32LE(Bindingˉentries + 108 + 24), 3, 'Code binding shape');

Requireˉmagic(Wir, 'WVIR', 'WIR directory');
Requireˉvalue(Wir.length, 1_708, 'WIR bytes');
Requireˉvalue(Wir.readUInt16LE(6), 9, 'WIR minor version');
Requireˉvalue(Wir.readUInt32LE(8), 4, 'WIR function entries');
Requireˉvalue(Wir.readUInt32LE(16), 15, 'WIR blocks');
Requireˉvalue(Wir.readUInt32LE(24), 30, 'WIR operations');

const Operations = 48 + Wir.readUInt32LE(8) * 48 + Wir.readUInt32LE(16) * 28;
function Requireˉoperation(Index, Operation, Shape, Target, Auxiliary, Label) {
    const Entry = Operations + Index * 28;
    Requireˉvalue(Wir.readUInt16LE(Entry + 4), Operation, `${Label} operation`);
    Requireˉvalue(Wir.readUInt32LE(Entry + 8), Shape, `${Label} shape`);
    Requireˉvalue(Wir.readUInt32LE(Entry + 20), Target, `${Label} target`);
    Requireˉvalue(Wir.readUInt32LE(Entry + 24), Auxiliary, `${Label} selector`);
}
Requireˉoperation(1, 66, 4, PRIVATE_TYPE_SHAPE, 0, 'Value case test');
Requireˉoperation(2, 164, 3, PRIVATE_TYPE_SHAPE, 1, 'Attempts field');
Requireˉoperation(4, 164, 65_536, PRIVATE_TYPE_SHAPE, 0, 'Item field');
Requireˉoperation(13, 66, 4, PRIVATE_TYPE_SHAPE, 1, 'Failure case test');
Requireˉoperation(14, 164, 3, PRIVATE_TYPE_SHAPE, 64, 'Code field');
Requireˉoperation(28, 65, PRIVATE_TYPE_SHAPE, PRIVATE_TYPE_SHAPE, 0, 'Value construction');

Requireˉmagic(Wvb, 'WVB1', 'WVB');
Requireˉvalue(Wvb.length, 947, 'WVB bytes');
Requireˉvalue(Wvb.readUInt16LE(6), 16, 'WVB minor version');
Requireˉvalue(Sha256(Wvb), '5dda1cc2c65bd8af7d9a1f8b52f83002c083014303c8604ad667d217880971f7', 'WVB SHA-256');

const Sections = [];
let Sectionˉcursor = 12;
for (let Kind = 1; Kind <= 7; Kind += 1) {
    Requireˉvalue(Wvb.readUInt32LE(Sectionˉcursor), Kind, `WVB section ${Kind} kind`);
    const Length = Wvb.readUInt32LE(Sectionˉcursor + 4);
    Sections[Kind] = { offset: Sectionˉcursor + 8, length: Length };
    Sectionˉcursor += 8 + Length;
}
Requireˉvalue(Sectionˉcursor, Wvb.length, 'WVB section consumption');
Requireˉvalue(Wvb.readUInt32LE(Sections[4].offset), 2, 'WVB functions');

const Types = Sections[7];
let Typeˉcursor = Types.offset;
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 2, 'WVB nominal types');
Typeˉcursor += 4;
function Readˉstring() {
    const Length = Wvb.readUInt32LE(Typeˉcursor);
    Typeˉcursor += 4;
    const Value = Wvb.subarray(Typeˉcursor, Typeˉcursor + Length).toString('utf8');
    Typeˉcursor += Length;
    return Value;
}
Requireˉvalue(Wvb[Typeˉcursor++], 1, 'Point kind');
Requireˉvalue(Readˉstring(), 'Point', 'Point name');
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 1, 'Point fields');
Typeˉcursor += 4;
Requireˉvalue(Readˉstring(), 'X', 'Point.X name');
Requireˉvalue(Wvb[Typeˉcursor++], 1, 'Point.X shape');
Requireˉvalue(Wvb[Typeˉcursor++], 3, 'Outcome kind');
Requireˉvalue(Readˉstring(), '__WvY0000', 'Outcome private name');
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 3, 'Outcome cases');
Typeˉcursor += 4;
Requireˉvalue(Readˉstring(), 'Value', 'Value case name');
Requireˉvalue(Wvb[Typeˉcursor++], 2, 'Value field encoding');
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 2, 'Value fields');
Typeˉcursor += 4;
Requireˉvalue(Readˉstring(), 'Item', 'Value.Item name');
Requireˉvalue(Wvb[Typeˉcursor++], 7, 'Value.Item shape kind');
Requireˉvalue(Wvb.readUInt32LE(Typeˉcursor), 0, 'Value.Item target');
Typeˉcursor += 4;
Requireˉvalue(Readˉstring(), 'Attempts', 'Value.Attempts name');
Requireˉvalue(Wvb[Typeˉcursor++], 5, 'Value.Attempts shape');
Requireˉvalue(Readˉstring(), 'Failure', 'Failure case name');
Requireˉvalue(Wvb[Typeˉcursor++], 1, 'Failure field encoding');
Requireˉvalue(Readˉstring(), 'Code', 'Failure.Code name');
Requireˉvalue(Wvb[Typeˉcursor++], 5, 'Failure.Code shape');
Requireˉvalue(Readˉstring(), 'Empty', 'Empty case name');
Requireˉvalue(Wvb[Typeˉcursor++], 0, 'Empty field encoding');
Requireˉvalue(Typeˉcursor, Types.offset + Types.length, 'WVB Types consumption');

process.stdout.write(
    `generic nominal variant status=Passed cases=${Cases} ` +
    `wvlb-bytes=${Bindings.length} wvir-bytes=${Wir.length} ` +
    `wvb-bytes=${Wvb.length} result=42 private-shape=0x80000000\n`,
);
