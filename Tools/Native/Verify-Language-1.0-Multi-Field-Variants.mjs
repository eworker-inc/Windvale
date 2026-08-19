import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { spawnSync } from "node:child_process";

function Reject(Message) {
    process.stderr.write(`${Message}\n`);
    process.exit(1);
}

if (process.argv.length !== 6) {
    process.stderr.write(
        "Usage: node Tools/Native/Verify-Language-1.0-Multi-Field-Variants.mjs " +
        "<verifier> <multi-field.wvb> <named-single-field.wvb> " +
        "<work-directory>\n",
    );
    process.exit(64);
}

const Verifier = resolve(process.argv[2]);
const Validˉpath = resolve(process.argv[3]);
const Singleˉpath = resolve(process.argv[4]);
const Workˉdirectory = resolve(process.argv[5]);
mkdirSync(Workˉdirectory, { recursive: true });

function Parseˉmodule(Path) {
    const Bytes = readFileSync(Path);
    if (Bytes.length < 12 || Bytes.subarray(0, 4).toString("ascii") !== "WVB1" ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 16 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject(`${basename(Path)} is not a WVB 1.16 seven-section module.`);
    }
    const Sections = [];
    let Cursor = 12;
    for (let Expectedˉkind = 1; Expectedˉkind <= 7; Expectedˉkind++) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Expectedˉkind) {
            Reject(`${basename(Path)} has no canonical section ${Expectedˉkind}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) {
            Reject(`${basename(Path)} truncates section ${Expectedˉkind}.`);
        }
        Sections[Expectedˉkind] = { payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) { Reject(`${basename(Path)} has trailing bytes.`); }
    return { Bytes, Sections };
}

function Skipˉstring(Bytes, Cursor) {
    const Length = Bytes.readUInt32LE(Cursor);
    return Cursor + 4 + Length;
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

function Typeˉdirectory(Module) {
    const Bytes = Module.Bytes;
    const Section = Module.Sections[7];
    const Variants = [];
    let Cursor = Section.payload;
    const Count = Bytes.readUInt32LE(Cursor);
    Cursor += 4;
    for (let Index = 0; Index < Count; Index++) {
        const Kind = Bytes[Cursor++];
        Cursor = Skipˉstring(Bytes, Cursor);
        const Itemˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Cases = [];
        for (let Item = 0; Item < Itemˉcount; Item++) {
            Cursor = Skipˉstring(Bytes, Cursor);
            if (Kind === 1) {
                Cursor = Skipˉshape(Bytes, Cursor);
            } else if (Kind === 2) {
                Cursor += 4;
            } else if (Kind === 3) {
                const Markerˉoffset = Cursor;
                const Marker = Bytes[Cursor++];
                const Fields = [];
                let Fieldˉcount = 0;
                let Countˉoffset = -1;
                if (Marker === 1) {
                    const Nameˉoffset = Cursor;
                    Cursor = Skipˉstring(Bytes, Cursor);
                    const Shapeˉoffset = Cursor;
                    Cursor = Skipˉshape(Bytes, Cursor);
                    Fields.push({ Nameˉoffset, Shapeˉoffset });
                    Fieldˉcount = 1;
                } else if (Marker === 2) {
                    Countˉoffset = Cursor;
                    Fieldˉcount = Bytes.readUInt32LE(Cursor);
                    Cursor += 4;
                    for (let Field = 0; Field < Fieldˉcount; Field++) {
                        const Nameˉoffset = Cursor;
                        Cursor = Skipˉstring(Bytes, Cursor);
                        const Shapeˉoffset = Cursor;
                        Cursor = Skipˉshape(Bytes, Cursor);
                        Fields.push({ Nameˉoffset, Shapeˉoffset });
                    }
                } else if (Marker !== 0) {
                    Reject("The valid module contains an unknown variant field marker.");
                }
                Cases.push({ Marker, Markerˉoffset, Countˉoffset, Fieldˉcount, Fields });
            } else {
                Reject("The valid module contains an unknown nominal type kind.");
            }
        }
        if (Kind === 3) { Variants.push({ Index, Cases }); }
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject("The valid type directory is inconsistent.");
    }
    return { Count, Variants };
}

function Functionˉdirectory(Module) {
    const Bytes = Module.Bytes;
    const Section = Module.Sections[4];
    const Result = [];
    let Cursor = Section.payload;
    const Count = Bytes.readUInt32LE(Cursor);
    Cursor += 4;
    for (let Index = 0; Index < Count; Index++) {
        Cursor = Skipˉstring(Bytes, Cursor);
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter++) {
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        Cursor = Skipˉshape(Bytes, Cursor);
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Local = 0; Local < Localˉcount; Local++) {
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        Result.push({ Codeˉoffset, Codeˉlength });
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject("The valid function directory is inconsistent.");
    }
    return Result;
}

function Instructionˉwidth(Bytes, Offset, Remaining) {
    const Opcode = Bytes[Offset];
    let Width = 1;
    if (Opcode === 2 || Opcode === 8) { Width = 2; }
    if (Opcode === 1 || (Opcode >= 3 && Opcode <= 7) || Opcode === 9 ||
        Opcode === 10 || Opcode === 48 || Opcode === 49 || Opcode === 64 ||
        Opcode === 65 || Opcode === 104 || Opcode === 105) { Width = 5; }
    if (Opcode === 106 || Opcode === 128 || Opcode === 129 ||
        (Opcode >= 151 && Opcode <= 154) || Opcode === 196) { Width = 9; }
    if (Opcode === 192) { Width = Bytes[Offset + 2] === 0 ? 5 : 3; }
    if (Opcode === 193) { Width = Bytes[Offset + 1] === 0 ? 6 : 2; }
    if (Opcode === 194) {
        Width = 3;
        if (Bytes[Offset + 2] === 0) { Width = Bytes[Offset + 1] === 19 ? 11 : 7; }
    }
    if (Width > Remaining) { Reject("The valid fixture contains a truncated instruction."); }
    return Width;
}

function Opcodeˉpositions(Module, Opcode) {
    const Result = [];
    const Code = Module.Sections[5];
    for (const Function of Functionˉdirectory(Module)) {
        let Cursor = 0;
        while (Cursor < Function.Codeˉlength) {
            const Position = Code.payload + Function.Codeˉoffset + Cursor;
            if (Module.Bytes[Position] === Opcode) { Result.push(Position); }
            Cursor += Instructionˉwidth(Module.Bytes, Position, Function.Codeˉlength - Cursor);
        }
        if (Cursor !== Function.Codeˉlength) {
            Reject("The valid code directory is inconsistent.");
        }
    }
    return Result;
}

function Runˉverifier(Path) {
    return spawnSync(Verifier, [Path], { encoding: "utf8", windowsHide: true });
}

const Module = Parseˉmodule(Validˉpath);
const Types = Typeˉdirectory(Module);
const Multiˉvariants = Types.Variants.flatMap(Variant =>
    Variant.Cases
        .map((Case, Caseˉindex) => ({ Variant, Case, Caseˉindex }))
        .filter(Item => Item.Case.Marker === 2));
if (Multiˉvariants.length !== 1 || Multiˉvariants[0].Case.Fieldˉcount !== 3) {
    Reject("The valid module does not contain exactly one three-field variant case.");
}
const Multi = Multiˉvariants[0];
const Creates = Opcodeˉpositions(Module, 151).filter(Position =>
    Module.Bytes.readUInt32LE(Position + 1) === Multi.Variant.Index);
const Caseˉtests = Opcodeˉpositions(Module, 152).filter(Position =>
    Module.Bytes.readUInt32LE(Position + 1) === Multi.Variant.Index);
const Fields = Opcodeˉpositions(Module, 196).filter(Position =>
    Module.Bytes.readUInt32LE(Position + 1) === Multi.Variant.Index);
if (Creates.length === 0 || Caseˉtests.length === 0 || Fields.length !== 3) {
    Reject("The valid module does not exercise multi-field construction and extraction.");
}

const Accepted = Runˉverifier(Validˉpath);
if (Accepted.error || Accepted.status !== 0 ||
    !Accepted.stdout.includes("wvb status=Valid")) {
    Reject(`The verifier rejected the valid multi-field variant module: ${Accepted.stderr}`);
}

const Single = Parseˉmodule(Singleˉpath);
const Singleˉtypes = Typeˉdirectory(Single);
const Singleˉmulti = Singleˉtypes.Variants.flatMap(Variant =>
    Variant.Cases.filter(Case => Case.Marker === 2));
const Singleˉfields = Opcodeˉpositions(Single, 196);
if (Singleˉmulti.length !== 0 || Singleˉfields.length !== 1) {
    Reject("The named single-field module does not isolate the WVB 1.16 field opcode.");
}
const Singleˉaccepted = Runˉverifier(Singleˉpath);
if (Singleˉaccepted.error || Singleˉaccepted.status !== 0 ||
    !Singleˉaccepted.stdout.includes("wvb status=Valid")) {
    Reject(`The verifier rejected the named single-field variant module: ${Singleˉaccepted.stderr}`);
}

const Cases = [
    ["version-downgrade", Bytes => Bytes.writeUInt16LE(15, 6)],
    ["unknown-field-marker", Bytes => { Bytes[Multi.Case.Markerˉoffset] = 3; }],
    ["field-count-too-small", Bytes => Bytes.writeUInt32LE(1, Multi.Case.Countˉoffset)],
    ["field-count-too-large", Bytes => Bytes.writeUInt32LE(65, Multi.Case.Countˉoffset)],
    ["field-index-out-of-range", Bytes =>
        Bytes.writeUInt32LE(
            Multi.Caseˉindex * 64 + Multi.Case.Fieldˉcount,
            Fields[0] + 5,
        )],
    ["field-type-mismatch", Bytes => { Bytes[Multi.Case.Fields[0].Shapeˉoffset] = 4; }],
    ["field-nominal-out-of-range", Bytes =>
        Bytes.writeUInt32LE(Types.Count, Fields[0] + 1)],
    ["runtime-case-mismatch", Bytes => {
        Bytes.writeUInt32LE(Multi.Caseˉindex + 1, Creates[0] + 5);
        Bytes.writeUInt32LE(Multi.Caseˉindex + 1, Caseˉtests[0] + 5);
    }],
    ["constructor-case-out-of-range", Bytes =>
        Bytes.writeUInt32LE(Multi.Variant.Cases.length, Creates[0] + 5)],
];

for (const [Name, Mutate] of Cases) {
    const Candidate = Buffer.from(Module.Bytes);
    Mutate(Candidate);
    const Candidateˉpath = resolve(Workˉdirectory, `${Name}.wvb`);
    writeFileSync(Candidateˉpath, Candidate);
    const Result = Runˉverifier(Candidateˉpath);
    if (Result.error || Result.status === 0 ||
        Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier accepted malformed multi-field variant case ${Name}.`);
    }
}

const Truncatedˉpath = resolve(Workˉdirectory, "truncated.wvb");
writeFileSync(Truncatedˉpath, Module.Bytes.subarray(0, Module.Bytes.length - 1));
const Truncated = Runˉverifier(Truncatedˉpath);
if (Truncated.error || Truncated.status === 0 ||
    Truncated.stdout.includes("wvb status=Valid")) {
    Reject("The verifier accepted the truncated multi-field variant module.");
}

process.stdout.write(
    `language 1 multi-field variants status=Passed valid=2 malformed=${Cases.length + 1}\n`,
);
