import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { spawnSync } from "node:child_process";

function Reject(Message) {
    process.stderr.write(`${Message}\n`);
    process.exit(1);
}

if (process.argv.length !== 5) {
    process.stderr.write(
        "Usage: node Tools/Native/Verify-Language-1.0-Memory-Budget-Entry.mjs " +
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
    Valid.readUInt16LE(4) !== 1 || Valid.readUInt16LE(6) !== 21 ||
    Valid.readUInt32LE(8) !== 7) {
    Reject(`${basename(Validˉpath)} is not a WVB 1.21 seven-section module.`);
}

function Sections(Bytes) {
    const Result = [];
    let Cursor = 12;
    for (let Expectedˉkind = 1; Expectedˉkind <= 7; Expectedˉkind++) {
        if (Cursor + 8 > Bytes.length || Bytes[Cursor] !== Expectedˉkind ||
            Bytes[Cursor + 1] !== 0 || Bytes.readUInt16LE(Cursor + 2) !== 0) {
            Reject(`The valid module has no canonical section ${Expectedˉkind}.`);
        }
        const Length = Bytes.readUInt32LE(Cursor + 4);
        const Payload = Cursor + 8;
        if (Payload + Length > Bytes.length) {
            Reject(`The valid module truncates section ${Expectedˉkind}.`);
        }
        Result[Expectedˉkind] = { header: Cursor, payload: Payload, length: Length };
        Cursor = Payload + Length;
    }
    if (Cursor !== Bytes.length) {
        Reject("The valid module has trailing bytes.");
    }
    return Result;
}

function Parseˉmain(Bytes) {
    const Section = Sections(Bytes)[4];
    const Count = Bytes.readUInt32LE(Section.payload);
    let Cursor = Section.payload + 4;
    for (let Functionˉindex = 0; Functionˉindex < Count; Functionˉindex++) {
        const Nameˉlength = Bytes.readUInt32LE(Cursor);
        const Nameˉoffset = Cursor + 4;
        const Name = Bytes.subarray(Nameˉoffset, Nameˉoffset + Nameˉlength)
            .toString("utf8");
        Cursor = Nameˉoffset + Nameˉlength;
        const Parameterˉcountˉoffset = Cursor;
        const Parameterˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Parameterˉshapeˉoffset = Cursor;
        for (let Index = 0; Index < Parameterˉcount; Index++) {
            const Kind = Bytes[Cursor++];
            if ([7, 8, 11, 22, 23, 24].includes(Kind)) Cursor += 4;
        }
        const Returnˉshapeˉoffset = Cursor;
        const Returnˉkind = Bytes[Cursor++];
        if ([7, 8, 11, 22, 23, 24].includes(Returnˉkind)) Cursor += 4;
        const Localˉcountˉoffset = Cursor;
        const Localˉcount = Bytes.readUInt32LE(Cursor);
        Cursor += 4;
        const Localˉshapeˉoffset = Cursor;
        for (let Index = 0; Index < Localˉcount; Index++) {
            const Kind = Bytes[Cursor++];
            if ([7, 8, 11, 22, 23, 24].includes(Kind)) Cursor += 4;
        }
        const Codeˉoffset = Bytes.readUInt32LE(Cursor);
        const Codeˉlength = Bytes.readUInt32LE(Cursor + 4);
        Cursor += 12;
        if (Name === "Main") {
            return {
                functionIndex: Functionˉindex,
                nameOffset: Nameˉoffset,
                parameterCountOffset: Parameterˉcountˉoffset,
                parameterCount: Parameterˉcount,
                parameterShapeOffset: Parameterˉshapeˉoffset,
                returnShapeOffset: Returnˉshapeˉoffset,
                localCountOffset: Localˉcountˉoffset,
                localCount: Localˉcount,
                localShapeOffset: Localˉshapeˉoffset,
                codeOffset: Codeˉoffset,
                codeLength: Codeˉlength,
                sectionHeader: Section.header,
            };
        }
    }
    Reject("The valid module has no Main function.");
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
    Reject(`The verifier rejected the valid memory-budget module: ${Accepted.stderr}`);
}

const Main = Parseˉmain(Valid);
const Validˉsections = Sections(Valid);
if (Main.parameterCount !== 1 || Valid[Main.parameterShapeOffset] !== 25 ||
    Valid[Main.returnShapeOffset] !== 1 || Main.localCount !== 1 ||
    Valid[Main.localShapeOffset] !== 1 ||
    Valid.readUInt32LE(Validˉsections[7].payload) !== 0) {
    Reject("The valid module does not carry the exact intrinsic budget entry shape.");
}

const Code = Validˉsections[5];
if (Main.codeLength < 6 || Main.codeOffset > Code.length - Main.codeLength) {
    Reject("The valid module has no bounded Main code body.");
}

const Cases = [
    ["version-downgrade", Bytes => Bytes.writeUInt16LE(20, 6)],
    ["primitive-parameter", Bytes => { Bytes[Main.parameterShapeOffset] = 1; }],
    ["non-main-budget", Bytes => { Bytes[Main.nameOffset + 3] = 109; }],
    ["budget-return", Bytes => { Bytes[Main.returnShapeOffset] = 25; }],
    ["second-budget-parameter", Bytes => {
        const Insertˉat = Main.returnShapeOffset;
        const Result = Buffer.concat([
            Bytes.subarray(0, Insertˉat),
            Buffer.from([25]),
            Bytes.subarray(Insertˉat),
        ]);
        Result.writeUInt32LE(2, Main.parameterCountOffset);
        Result.writeUInt32LE(
            Result.readUInt32LE(Main.sectionHeader + 4) + 1,
            Main.sectionHeader + 4,
        );
        return Result;
    }],
    ["budget-local", Bytes => {
        Bytes[Main.localShapeOffset] = 25;
    }],
    ["budget-load", Bytes => {
        const Codeˉstart = Code.payload + Main.codeOffset;
        Bytes[Codeˉstart] = 4;
        Bytes.writeUInt32LE(0, Codeˉstart + 1);
    }],
    ["budget-store", Bytes => {
        const Codeˉstart = Code.payload + Main.codeOffset;
        Bytes[Codeˉstart] = 5;
        Bytes.writeUInt32LE(0, Codeˉstart + 1);
    }],
    ["missing-budget-export", Bytes => {
        const Exports = Validˉsections[6];
        const Result = Buffer.concat([
            Bytes.subarray(0, Exports.payload + 4),
            Bytes.subarray(Exports.payload + Exports.length),
        ]);
        Result.writeUInt32LE(4, Exports.header + 4);
        Result.writeUInt32LE(0, Exports.payload);
        return Result;
    }],
];

for (const [Name, Mutate] of Cases) {
    const Candidate = Buffer.from(Valid);
    const Mutation = Mutate(Candidate);
    const Mutated = Buffer.isBuffer(Mutation) ? Mutation : Candidate;
    const Candidateˉpath = resolve(Workˉdirectory, `${Name}.wvb`);
    writeFileSync(Candidateˉpath, Mutated);
    const Result = Runˉverifier(Candidateˉpath);
    if (Result.error || Result.status === 0 ||
        Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier accepted malformed memory-budget case ${Name}.`);
    }
}

process.stdout.write(
    `language 1 memory budget entry status=Passed valid=1 malformed=${Cases.length}\n`,
);
