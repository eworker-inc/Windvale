import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { spawnSync } from "node:child_process";

function Reject(Message) {
    process.stderr.write(`${Message}\n`);
    process.exit(1);
}

if (process.argv.length !== 5) {
    process.stderr.write(
        "Usage: node Tools/Native/Verify-Language-1.0-Floating.mjs " +
        "<verifier> <valid.wvb> <work-directory>\n",
    );
    process.exit(64);
}

const Verifier = resolve(process.argv[2]);
const Validˉpath = resolve(process.argv[3]);
const Workˉdirectory = resolve(process.argv[4]);
mkdirSync(Workˉdirectory, { recursive: true });

const Valid = readFileSync(Validˉpath);
if (Valid.length < 12 || Valid.subarray(0, 4).toString("ascii") !== "WVB1" ||
    Valid.readUInt16LE(4) !== 1 || Valid.readUInt16LE(6) !== 14 ||
    Valid.readUInt32LE(8) !== 7) {
    Reject(`${basename(Validˉpath)} is not a WVB 1.14 seven-section module.`);
}

function Sections(Bytes) {
    const Result = [];
    let Cursor = 12;
    for (let Expectedˉkind = 1; Expectedˉkind <= 7; Expectedˉkind++) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Expectedˉkind) {
            Reject(`The valid module has no canonical section ${Expectedˉkind}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) {
            Reject(`The valid module truncates section ${Expectedˉkind}.`);
        }
        Result[Expectedˉkind] = { payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) { Reject("The valid module has trailing bytes."); }
    return Result;
}

const Parsedˉsections = Sections(Valid);
const Code = Parsedˉsections[5];
const Functions = Parsedˉsections[4];

function Findˉcode(Needle) {
    const Relative = Valid.subarray(Code.payload, Code.payload + Code.length)
        .indexOf(Buffer.from(Needle));
    if (Relative < 0) {
        Reject(`The valid module lacks floating code ${Needle.join("-")}.`);
    }
    return Code.payload + Relative;
}

function Skipˉshape(Bytes, Cursor) {
    const Kind = Bytes[Cursor];
    Cursor++;
    if (Kind === 7 || Kind === 8 || Kind === 11) { return Cursor + 4; }
    if (Kind === 12 || Kind === 13) {
        Cursor = Skipˉshape(Bytes, Cursor);
        return Cursor + 4;
    }
    return Cursor;
}

function Floatingˉshapeˉpositions(Bytes) {
    const Result = [];
    let Cursor = Functions.payload;
    const Count = Bytes.readUInt32LE(Cursor);
    Cursor += 4;
    for (let Index = 0; Index < Count; Index++) {
        const Nameˉlength = Bytes.readUInt32LE(Cursor);
        Cursor += 4 + Nameˉlength;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter++) {
            if (Bytes[Cursor] === 18 || Bytes[Cursor] === 19) { Result.push(Cursor); }
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        if (Bytes[Cursor] === 18 || Bytes[Cursor] === 19) { Result.push(Cursor); }
        Cursor = Skipˉshape(Bytes, Cursor);
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Local = 0; Local < Localˉcount; Local++) {
            if (Bytes[Cursor] === 18 || Bytes[Cursor] === 19) { Result.push(Cursor); }
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        Cursor += 12;
    }
    if (Cursor !== Functions.payload + Functions.length) {
        Reject("The valid floating function directory is inconsistent.");
    }
    return Result;
}

function Runˉverifier(Path) {
    return spawnSync(Verifier, [Path], {
        encoding: "utf8",
        windowsHide: true,
    });
}

const Accepted = Runˉverifier(Validˉpath);
if (Accepted.error || Accepted.status !== 0 ||
    !Accepted.stdout.includes("wvb status=Valid")) {
    Reject(`The verifier rejected the valid floating module: ${Accepted.stderr}`);
}

const F32ˉconstant = Findˉcode([0xC2, 18, 0, 0, 0, 0x80, 0x3F]);
const F64ˉconstant = Findˉcode([
    0xC2, 19, 0, 0, 0, 0, 0, 0, 0, 0xF0, 0x3F,
]);
const F32ˉadd = Findˉcode([0xC2, 18, 1]);
const Floatingˉshapes = Floatingˉshapeˉpositions(Valid);
if (Floatingˉshapes.length === 0) {
    Reject("The valid module has no floating shape.");
}

const Cases = [
    ["version-downgrade", Bytes => Bytes.writeUInt16LE(13, 6)],
    ["unknown-floating-kind", Bytes => { Bytes[F32ˉconstant + 1] = 20; }],
    ["rune-floating-kind", Bytes => { Bytes[F32ˉconstant + 1] = 17; }],
    ["unknown-floating-operation", Bytes => { Bytes[F32ˉadd + 2] = 12; }],
    ["f32-constant-wide-width", Bytes => { Bytes[F32ˉconstant + 1] = 19; }],
    ["f64-constant-narrow-width", Bytes => { Bytes[F64ˉconstant + 1] = 18; }],
    ["typed-floating-mismatch", Bytes => { Bytes[F32ˉadd + 1] = 19; }],
    ["floating-shape-mismatch", Bytes => { Bytes[Floatingˉshapes[0]] = 17; }],
];

for (const [Name, Mutate] of Cases) {
    const Candidate = Buffer.from(Valid);
    Mutate(Candidate);
    const Candidateˉpath = resolve(Workˉdirectory, `${Name}.wvb`);
    writeFileSync(Candidateˉpath, Candidate);
    const Result = Runˉverifier(Candidateˉpath);
    if (Result.error || Result.status === 0 ||
        Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier accepted malformed floating case ${Name}.`);
    }
}

process.stdout.write(
    `language 1 floating status=Passed valid=1 malformed=${Cases.length}\n`,
);
