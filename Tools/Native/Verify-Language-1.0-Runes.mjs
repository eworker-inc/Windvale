import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { spawnSync } from "node:child_process";

function Reject(Message) {
    process.stderr.write(`${Message}\n`);
    process.exit(1);
}

if (process.argv.length !== 5) {
    process.stderr.write(
        "Usage: node Tools/Native/Verify-Language-1.0-Runes.mjs " +
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
    Valid.readUInt16LE(4) !== 1 || Valid.readUInt16LE(6) !== 13 ||
    Valid.readUInt32LE(8) !== 7) {
    Reject(`${basename(Validˉpath)} is not a WVB 1.13 seven-section module.`);
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
        Reject(`The valid module lacks rune code ${Needle.join("-")}.`);
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

function Runeˉshapeˉpositions(Bytes) {
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
            if (Bytes[Cursor] === 17) { Result.push(Cursor); }
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        if (Bytes[Cursor] === 17) { Result.push(Cursor); }
        Cursor = Skipˉshape(Bytes, Cursor);
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Local = 0; Local < Localˉcount; Local++) {
            if (Bytes[Cursor] === 17) { Result.push(Cursor); }
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        Cursor += 12;
    }
    if (Cursor !== Functions.payload + Functions.length) {
        Reject("The valid rune function directory is inconsistent.");
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
    Reject(`The verifier rejected the valid rune module: ${Accepted.stderr}`);
}

const Answerˉconstant = Findˉcode([0xC1, 0x00, 0x2A, 0x00, 0x00, 0x00]);
const Equal = Findˉcode([0xC1, 0x01]);
Findˉcode([0xC1, 0x02]);
const Runeˉshapes = Runeˉshapeˉpositions(Valid);
if (Runeˉshapes.length === 0) { Reject("The valid module has no rune shape."); }

const Cases = [
    ["version-downgrade", Bytes => Bytes.writeUInt16LE(12, 6)],
    ["unknown-rune-operation", Bytes => { Bytes[Equal + 1] = 3; }],
    ["surrogate-scalar", Bytes => Bytes.writeUInt32LE(0xD800, Answerˉconstant + 2)],
    ["out-of-range-scalar", Bytes => Bytes.writeUInt32LE(0x110000, Answerˉconstant + 2)],
    ["unknown-envelope", Bytes => { Bytes[Answerˉconstant] = 0xC2; }],
    ["rune-shape-mismatch", Bytes => { Bytes[Runeˉshapes[0]] = 16; }],
];

for (const [Name, Mutate] of Cases) {
    const Candidate = Buffer.from(Valid);
    Mutate(Candidate);
    const Candidateˉpath = resolve(Workˉdirectory, `${Name}.wvb`);
    writeFileSync(Candidateˉpath, Candidate);
    const Result = Runˉverifier(Candidateˉpath);
    if (Result.error || Result.status === 0 ||
        Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier accepted malformed rune case ${Name}.`);
    }
}

process.stdout.write(
    `language 1 runes status=Passed valid=1 malformed=${Cases.length}\n`,
);
