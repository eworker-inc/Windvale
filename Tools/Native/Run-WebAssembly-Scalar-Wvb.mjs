import { lstat, readFile } from "node:fs/promises";

const WVXI_MAGIC = 0x49585657;
const WVXO_MAGIC = 0x4f585657;
const GUEST_INSTRUCTION_BUDGET = 4_096;
const MAXIMUM_CALL_DEPTH = 8;
const OUTER_INSTRUCTION_BUDGET = 100_000_000;
const MAXIMUM_INTERPRETER_BYTES = 16_777_216;
const MAXIMUM_CANDIDATE_BYTES = 4_194_288;

function Fail(Message, Exitˉcode = 1) {
    process.stderr.write(`${Message}\n`);
    process.exitCode = Exitˉcode;
}

function Requireˉi32(Value, Label) {
    if (!Number.isInteger(Value) || Value < -2_147_483_648 || Value > 2_147_483_647) {
        throw new Error(`${Label} is not an i32.`);
    }
    return Value;
}

if (process.argv.length !== 5) {
    Fail(
        "Usage: node Tools/Native/Run-WebAssembly-Scalar-Wvb.mjs " +
            "<scalar-interpreter.wasm> <candidate.wvb> <expected-i32>",
        64,
    );
} else {
    try {
        const Interpreterˉpath = process.argv[2];
        const Candidateˉpath = process.argv[3];
        if (!/^-?(?:0|[1-9][0-9]*)$/.test(process.argv[4])) {
            throw new Error("Expected result is not canonical decimal i32 text.");
        }
        const Expectedˉresult = Requireˉi32(Number(process.argv[4]), "Expected result");
        const [Interpreterˉstat, Candidateˉstat] = await Promise.all([
            lstat(Interpreterˉpath),
            lstat(Candidateˉpath),
        ]);
        if (!Interpreterˉstat.isFile() || Interpreterˉstat.size < 8 ||
            Interpreterˉstat.size > MAXIMUM_INTERPRETER_BYTES) {
            throw new Error("The scalar interpreter is not a bounded ordinary file.");
        }
        if (!Candidateˉstat.isFile() || Candidateˉstat.size < 12 ||
            Candidateˉstat.size > MAXIMUM_CANDIDATE_BYTES) {
            throw new Error("The WVB candidate is not a bounded ordinary file.");
        }
        const [Interpreter, Candidate] = await Promise.all([
            readFile(Interpreterˉpath),
            readFile(Candidateˉpath),
        ]);
        if (!WebAssembly.validate(Interpreter)) {
            throw new Error("The scalar interpreter failed WebAssembly validation.");
        }
        const Module = await WebAssembly.compile(Interpreter);
        if (WebAssembly.Module.imports(Module).length !== 0) {
            throw new Error("The scalar interpreter imports a host capability.");
        }
        const Instance = await WebAssembly.instantiate(Module);
        const Exports = Instance.exports;
        const Memory = Exports["Windvale.memory"];
        if (
            Exports["Windvale.abi"]?.value !== 3 ||
            !(Memory instanceof WebAssembly.Memory) ||
            Memory.buffer.byteLength !== 129 * 65_536 ||
            typeof Exports["Windvale.run"] !== "function"
        ) {
            throw new Error("The scalar interpreter ABI 3 contract is invalid.");
        }
        const Inputˉoffset = Exports["Windvale.input_offset"]?.value;
        const Inputˉcapacity = Exports["Windvale.input_capacity"]?.value;
        const Outputˉoffset = Exports["Windvale.output_offset"]?.value;
        const Outputˉcapacity = Exports["Windvale.output_capacity"]?.value;
        if (
            !Number.isInteger(Inputˉoffset) ||
            !Number.isInteger(Inputˉcapacity) ||
            !Number.isInteger(Outputˉoffset) ||
            !Number.isInteger(Outputˉcapacity) ||
            Inputˉoffset < 0 ||
            Inputˉcapacity < 0 ||
            Outputˉoffset < Inputˉoffset + Inputˉcapacity ||
            Outputˉcapacity < 20 ||
            Outputˉoffset + Outputˉcapacity > Memory.buffer.byteLength
        ) {
            throw new Error("The scalar interpreter buffer contract is invalid.");
        }
        const Request = Buffer.alloc(16 + Candidate.length);
        if (Request.length > Inputˉcapacity) {
            throw new Error("The candidate exceeds the scalar interpreter input capacity.");
        }
        Request.writeUInt32LE(WVXI_MAGIC, 0);
        Request.writeUInt16LE(1, 4);
        Request.writeUInt16LE(0, 6);
        Request.writeUInt32LE(GUEST_INSTRUCTION_BUDGET, 8);
        Request.writeUInt32LE(MAXIMUM_CALL_DEPTH, 12);
        Candidate.copy(Request, 16);
        new Uint8Array(Memory.buffer).set(Request, Inputˉoffset);

        const Outerˉstatus = Exports["Windvale.run"](
            OUTER_INSTRUCTION_BUDGET,
            Request.length,
        );
        const Outerˉinstructions = Exports["Windvale.instructions"]?.value;
        const Outputˉlength = Exports["Windvale.output_length"]?.value;
        if (
            Outerˉstatus !== 0 ||
            !Number.isInteger(Outerˉinstructions) ||
            Outerˉinstructions < 1 ||
            Outerˉinstructions > OUTER_INSTRUCTION_BUDGET ||
            Outputˉlength !== 20
        ) {
            throw new Error(
                `The scalar interpreter returned ${Outerˉstatus}/` +
                    `${Outerˉinstructions}/${Outputˉlength}.`,
            );
        }
        const Output = Buffer.from(
            new Uint8Array(Memory.buffer).slice(
                Outputˉoffset,
                Outputˉoffset + Outputˉlength,
            ),
        );
        const Guestˉstatus = Output.readUInt32LE(8);
        const Guestˉinstructions = Output.readUInt32LE(12);
        const Result = Output.readInt32LE(16);
        if (
            Output.readUInt32LE(0) !== WVXO_MAGIC ||
            Output.readUInt16LE(4) !== 1 ||
            Output.readUInt16LE(6) !== 0 ||
            Guestˉstatus !== 0 ||
            Guestˉinstructions < 1 ||
            Guestˉinstructions > GUEST_INSTRUCTION_BUDGET ||
            Result !== Expectedˉresult
        ) {
            throw new Error(
                `The scalar result was ${Guestˉstatus}/` +
                    `${Guestˉinstructions}/${Result}; expected 0/bounded/` +
                    `${Expectedˉresult}.`,
            );
        }
        process.stdout.write(
            `webassembly scalar status=Valid result=${Result} ` +
                `guest-instructions=${Guestˉinstructions} ` +
                `outer-instructions=${Outerˉinstructions}\n`,
        );
    } catch (Errorˉvalue) {
        Fail(
            Errorˉvalue instanceof Error
                ? Errorˉvalue.stack ?? Errorˉvalue.message
                : String(Errorˉvalue),
        );
    }
}
