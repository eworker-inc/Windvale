import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
import { spawnSync } from "node:child_process";

function Reject(Message) {
    process.stderr.write(`${Message}\n`);
    process.exit(1);
}

if (process.argv.length !== 5) {
    process.stderr.write(
        "Usage: node Tools/Native/Verify-Language-1.0-Fixed-Integers.mjs " +
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
    Valid.readUInt16LE(4) !== 1 || Valid.readUInt16LE(6) !== 12 ||
    Valid.readUInt32LE(8) !== 7) {
    Reject(`${basename(Validˉpath)} is not a WVB 1.12 seven-section module.`);
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
    if (Cursor !== Bytes.length) {
        Reject("The valid module has trailing bytes.");
    }
    return Result;
}

const Code = Sections(Valid)[5];

function Findˉcode(Needle) {
    const Relative = Valid.subarray(Code.payload, Code.payload + Code.length)
        .indexOf(Buffer.from(Needle));
    if (Relative < 0) {
        Reject(`The valid module lacks fixed-integer code ${Needle.join("-")}.`);
    }
    return Code.payload + Relative;
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
    Reject(`The verifier rejected the valid fixed-integer module: ${Accepted.stderr}`);
}

const I8ˉconstant = Findˉcode([0xC0, 0x0E, 0x00, 0x78, 0x00]);
const I8ˉadd = Findˉcode([0xC0, 0x0E, 0x01]);
const U16ˉadd = Findˉcode([0xC0, 0x10, 0x01]);

const Cases = [
    ["version-downgrade", Bytes => Bytes.writeUInt16LE(11, 6)],
    ["unknown-fixed-kind", Bytes => { Bytes[I8ˉconstant + 1] = 13; }],
    ["unknown-fixed-operation", Bytes => { Bytes[I8ˉadd + 2] = 19; }],
    ["i8-constant-high-byte", Bytes => { Bytes[I8ˉconstant + 4] = 1; }],
    ["u16-negate", Bytes => { Bytes[U16ˉadd + 2] = 6; }],
    ["i8-bitwise", Bytes => { Bytes[I8ˉadd + 2] = 13; }],
];

for (const [Name, Mutate] of Cases) {
    const Candidate = Buffer.from(Valid);
    Mutate(Candidate);
    const Candidateˉpath = resolve(Workˉdirectory, `${Name}.wvb`);
    writeFileSync(Candidateˉpath, Candidate);
    const Result = Runˉverifier(Candidateˉpath);
    if (Result.error || Result.status === 0 ||
        Result.stdout.includes("wvb status=Valid")) {
        Reject(`The verifier accepted malformed fixed-integer case ${Name}.`);
    }
}

process.stdout.write(
    `language 1 fixed integers status=Passed valid=1 malformed=${Cases.length}\n`,
);
