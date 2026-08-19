import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { spawnSync } from "node:child_process";

function Reject(Message) {
    process.stderr.write(`${Message}\n`);
    process.exit(1);
}

if (process.argv.length !== 6) {
    process.stderr.write(
        "Usage: node Tools/Native/Verify-Language-1.0-Unit-Never.mjs " +
        "<verifier> <unit.wvb> <never.wvb> <work-directory>\n",
    );
    process.exit(64);
}

const Verifier = resolve(process.argv[2]);
const Unitˉpath = resolve(process.argv[3]);
const Neverˉpath = resolve(process.argv[4]);
const Workˉdirectory = resolve(process.argv[5]);
mkdirSync(Workˉdirectory, { recursive: true });

function Parseˉmodule(Path) {
    const Bytes = readFileSync(Path);
    if (Bytes.length < 12 || Bytes.subarray(0, 4).toString("ascii") !== "WVB1" ||
        Bytes.readUInt16LE(4) !== 1 || Bytes.readUInt16LE(6) !== 15 ||
        Bytes.readUInt32LE(8) !== 7) {
        Reject(`${basename(Path)} is not a WVB 1.15 seven-section module.`);
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

function Functionˉdirectory(Module) {
    const Bytes = Module.Bytes;
    const Section = Module.Sections[4];
    const Result = [];
    let Cursor = Section.payload;
    const Count = Bytes.readUInt32LE(Cursor);
    Cursor += 4;
    for (let Index = 0; Index < Count; Index++) {
        const Nameˉlength = Bytes.readUInt32LE(Cursor);
        const Name = Bytes.subarray(Cursor + 4, Cursor + 4 + Nameˉlength).toString("utf8");
        Cursor += 4 + Nameˉlength;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Parameters = [];
        for (let Parameter = 0; Parameter < Parameterˉcount; Parameter++) {
            Parameters.push(Cursor);
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        const Return = Cursor;
        Cursor = Skipˉshape(Bytes, Cursor);
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Locals = [];
        for (let Local = 0; Local < Localˉcount; Local++) {
            Locals.push(Cursor);
            Cursor = Skipˉshape(Bytes, Cursor);
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        Result.push({ Name, Parameters, Return, Locals, Codeˉoffset, Codeˉlength });
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject("A function directory is inconsistent.");
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
        (Opcode >= 151 && Opcode <= 154)) { Width = 9; }
    if (Opcode === 192) {
        Width = Bytes[Offset + 2] === 0 ? 5 : 3;
    }
    if (Opcode === 193) {
        Width = Bytes[Offset + 1] === 0 ? 6 : 2;
    }
    if (Opcode === 194) {
        Width = 3;
        if (Bytes[Offset + 2] === 0) { Width = Bytes[Offset + 1] === 19 ? 11 : 7; }
    }
    if (Width > Remaining) { Reject("A fixture instruction is truncated."); }
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
            Cursor += Instructionˉwidth(
                Module.Bytes,
                Position,
                Function.Codeˉlength - Cursor,
            );
        }
        if (Cursor !== Function.Codeˉlength) {
            Reject("A fixture code directory is inconsistent.");
        }
    }
    return Result;
}

function Recordˉfieldˉpositions(Module, Shape) {
    const Bytes = Module.Bytes;
    const Section = Module.Sections[7];
    const Result = [];
    let Cursor = Section.payload;
    const Count = Bytes.readUInt32LE(Cursor);
    Cursor += 4;
    for (let Index = 0; Index < Count; Index++) {
        const Kind = Bytes[Cursor++];
        const Nameˉlength = Bytes.readUInt32LE(Cursor);
        Cursor += 4 + Nameˉlength;
        const Itemˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        for (let Item = 0; Item < Itemˉcount; Item++) {
            const Itemˉnameˉlength = Bytes.readUInt32LE(Cursor);
            Cursor += 4 + Itemˉnameˉlength;
            if (Kind === 1) {
                if (Bytes[Cursor] === Shape) { Result.push(Cursor); }
                Cursor = Skipˉshape(Bytes, Cursor);
            } else if (Kind === 2) {
                Cursor += 4;
            } else {
                const Present = Bytes[Cursor++];
                if (Present === 1) {
                    const Payloadˉnameˉlength = Bytes.readUInt32LE(Cursor);
                    Cursor += 4 + Payloadˉnameˉlength;
                    if (Bytes[Cursor] === Shape) { Result.push(Cursor); }
                    Cursor = Skipˉshape(Bytes, Cursor);
                }
            }
        }
    }
    if (Cursor !== Section.payload + Section.length) {
        Reject("A type directory is inconsistent.");
    }
    return Result;
}

function Runˉverifier(Path) {
    return spawnSync(Verifier, [Path], { encoding: "utf8", windowsHide: true });
}

function Requireˉaccepted(Path) {
    const Result = Runˉverifier(Path);
    if (Result.error || Result.status !== 0 ||
        !Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier rejected ${basename(Path)}: ${Result.stderr}`);
    }
}

const Unit = Parseˉmodule(Unitˉpath);
const Never = Parseˉmodule(Neverˉpath);
Requireˉaccepted(Unitˉpath);
Requireˉaccepted(Neverˉpath);

const Unitˉfunctions = Functionˉdirectory(Unit);
const Unitˉparameters = Unitˉfunctions.flatMap(Function =>
    Function.Parameters.filter(Position => Unit.Bytes[Position] === 20));
const Unitˉreturns = Unitˉfunctions
    .map(Function => Function.Return)
    .filter(Position => Unit.Bytes[Position] === 20);
const Unitˉlocals = Unitˉfunctions.flatMap(Function =>
    Function.Locals.filter(Position => Unit.Bytes[Position] === 20));
const Unitˉfields = Recordˉfieldˉpositions(Unit, 20);
const Unitˉopcodes = Opcodeˉpositions(Unit, 195);
const Neverˉreturns = Functionˉdirectory(Never)
    .map(Function => Function.Return)
    .filter(Position => Never.Bytes[Position] === 21);
if (Unitˉparameters.length === 0 || Unitˉreturns.length === 0 ||
    Unitˉlocals.length === 0 || Unitˉfields.length === 0 ||
    Unitˉopcodes.length === 0 || Neverˉreturns.length === 0) {
    Reject("The valid modules do not cover every unit/never encoding position.");
}

const Cases = [
    ["unit-version-downgrade", Unit, Bytes => Bytes.writeUInt16LE(14, 6)],
    ["unit-unknown-opcode", Unit, Bytes => { Bytes[Unitˉopcodes[0]] = 196; }],
    ["unit-unknown-shape", Unit, Bytes => { Bytes[Unitˉparameters[0]] = 22; }],
    ["unit-typed-local-mismatch", Unit, Bytes => { Bytes[Unitˉlocals[0]] = 4; }],
    ["never-parameter", Unit, Bytes => { Bytes[Unitˉparameters[0]] = 21; }],
    ["never-local", Unit, Bytes => { Bytes[Unitˉlocals[0]] = 21; }],
    ["never-record-field", Unit, Bytes => { Bytes[Unitˉfields[0]] = 21; }],
    ["never-return-instruction", Unit, Bytes => { Bytes[Unitˉreturns[0]] = 21; }],
    ["never-version-downgrade", Never, Bytes => Bytes.writeUInt16LE(14, 6)],
    ["never-unknown-shape", Never, Bytes => { Bytes[Neverˉreturns[0]] = 22; }],
    ["never-reinterpreted-as-unit", Never, Bytes => { Bytes[Neverˉreturns[0]] = 20; }],
];

for (const [Name, Module, Mutate] of Cases) {
    const Candidate = Buffer.from(Module.Bytes);
    Mutate(Candidate);
    const Candidateˉpath = resolve(Workˉdirectory, `${Name}.wvb`);
    writeFileSync(Candidateˉpath, Candidate);
    const Result = Runˉverifier(Candidateˉpath);
    if (Result.error || Result.status === 0 ||
        Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier accepted malformed unit/never case ${Name}.`);
    }
}

process.stdout.write(
    `language 1 unit-never status=Passed valid=2 malformed=${Cases.length}\n`,
);
