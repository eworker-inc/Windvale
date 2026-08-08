const WVXI_MAGIC = 0x4958_5657;
const WVXO_MAGIC = 0x4F58_5657;
const WVSS_MAGIC = 0x5353_5657;
const WVCO_MAGIC = 0x4F43_5657;
const MAXIMUM_SOURCE_BYTES = 64 * 1024;
const COMPILER_INSTRUCTION_BUDGET = 20_000_000;
const EXECUTION_OUTER_BUDGET = 200_000_000;
const MAXIMUM_CALL_DEPTH = 64;

export async function Compileˉverifyˉexecute(
    Interpreterˉbytes,
    Compilerˉbytes,
    Sourceˉbytes,
    Executionˉinstructionˉlimit = 1_000_000,
) {
    Requireˉbytes(Interpreterˉbytes, "interpreter");
    Requireˉbytes(Compilerˉbytes, "direct compiler");
    Requireˉbytes(Sourceˉbytes, "source");
    if (Sourceˉbytes.byteLength === 0 ||
        Sourceˉbytes.byteLength > MAXIMUM_SOURCE_BYTES) {
        throw new Error("The Windvale source is outside the 64 KiB browser limit.");
    }
    Requireˉinstructionˉlimit(Executionˉinstructionˉlimit);
    if (!WebAssembly.validate(Interpreterˉbytes)) {
        throw new Error("The browser rejected the packaged Windvale interpreter.");
    }
    if (!WebAssembly.validate(Compilerˉbytes)) {
        throw new Error("The browser rejected the packaged Windvale compiler.");
    }

    const [Interpreterˉmodule, Compilerˉmodule] = await Promise.all([
        WebAssembly.compile(Interpreterˉbytes),
        WebAssembly.compile(Compilerˉbytes),
    ]);
    if (WebAssembly.Module.imports(Interpreterˉmodule).length !== 0) {
        throw new Error("The packaged Windvale interpreter imports a host capability.");
    }
    if (WebAssembly.Module.imports(Compilerˉmodule).length !== 0) {
        throw new Error("The packaged Windvale compiler imports a host capability.");
    }
    Requireˉexports(Interpreterˉmodule, "interpreter");
    Requireˉexports(Compilerˉmodule, "compiler");
    const [Interpreterˉinstance, Compilerˉinstance] = await Promise.all([
        WebAssembly.instantiate(Interpreterˉmodule, {}),
        WebAssembly.instantiate(Compilerˉmodule, {}),
    ]);
    const Interpreterˉexports = Interpreterˉinstance.exports;
    const Interpreterˉmemory = Interpreterˉexports["Windvale.memory"];
    if (Readˉglobal(Interpreterˉexports, "Windvale.abi") !== 3 ||
        Readˉglobal(Interpreterˉexports, "Windvale.output_kind") !== 1 ||
        !(Interpreterˉmemory instanceof WebAssembly.Memory) ||
        Interpreterˉmemory.buffer.byteLength !== 129 * 65_536) {
        throw new Error("The packaged interpreter violates execution ABI 3.");
    }
    Requireˉfixedˉmemory(Interpreterˉmemory, "interpreter");
    Requireˉinterpreterˉmemoryˉregions(Interpreterˉexports);

    const Compilerˉexports = Compilerˉinstance.exports;
    const Compilerˉmemory = Compilerˉexports["Windvale.memory"];
    if (Readˉglobal(Compilerˉexports, "Windvale.abi") !== 4 ||
        Readˉglobal(Compilerˉexports, "Windvale.output_kind") !== 1 ||
        !(Compilerˉmemory instanceof WebAssembly.Memory) ||
        Compilerˉmemory.buffer.byteLength !== 2_497 * 65_536) {
        throw new Error("The packaged compiler violates execution ABI 4.");
    }
    Requireˉfixedˉmemory(Compilerˉmemory, "compiler");
    Requireˉcompilerˉmemoryˉregions(Compilerˉexports);

    const Sourceˉset = Buildˉsourceˉset(Sourceˉbytes);
    const Compilerˉrun = Runˉcompiler(
        Compilerˉexports,
        Sourceˉset,
        COMPILER_INSTRUCTION_BUDGET,
    );
    const Wvb = Readˉwvco(Compilerˉrun.Output);

    const Executionˉrequest = Buildˉscalarˉrequest(
        Wvb,
        Executionˉinstructionˉlimit,
    );
    const Executionˉrun = Runˉrequest(
        Interpreterˉexports,
        Executionˉrequest,
        EXECUTION_OUTER_BUDGET,
    );
    const Executionˉresponse = Readˉwvxo(Executionˉrun.Output, 1);
    if (Executionˉresponse.Result.byteLength !== 4) {
        throw new Error("The scalar execution response has an invalid result payload.");
    }

    return {
        Wvb,
        Wvbˉsha256: await Sha256(Wvb),
        Compilerˉinstructions: Compilerˉrun.Instructions,
        Executionˉstatus: Executionˉresponse.Status,
        Executionˉresult: Readˉi32(Executionˉresponse.Result, 0),
        Executionˉguestˉinstructions: Executionˉresponse.Guestˉinstructions,
        Executionˉouterˉinstructions: Executionˉrun.Outerˉinstructions,
    };
}

function Buildˉsourceˉset(Source) {
    const Result = new Uint8Array(24 + Source.byteLength);
    const View = new DataView(Result.buffer);
    Writeˉu32(View, 0, WVSS_MAGIC);
    Writeˉu16(View, 4, 1);
    Writeˉu16(View, 6, 0);
    Writeˉu32(View, 8, 1);
    Writeˉu32(View, 12, 8);
    Writeˉu32(View, 16, 24);
    Writeˉu32(View, 20, Source.byteLength);
    Result.set(Source, 24);
    return Result;
}

function Buildˉscalarˉrequest(Candidate, Budget) {
    const Result = new Uint8Array(16 + Candidate.byteLength);
    const View = new DataView(Result.buffer);
    Writeˉu32(View, 0, WVXI_MAGIC);
    Writeˉu16(View, 4, 1);
    Writeˉu16(View, 6, 0);
    Writeˉu32(View, 8, Budget);
    Writeˉu32(View, 12, MAXIMUM_CALL_DEPTH);
    Result.set(Candidate, 16);
    return Result;
}

function Runˉcompiler(Exports, Input, Budget) {
    const Memory = Exports["Windvale.memory"];
    const Inputˉoffset = Readˉglobal(Exports, "Windvale.input_offset");
    const Inputˉcapacity = Readˉglobal(Exports, "Windvale.input_capacity");
    if (Input.byteLength > Inputˉcapacity) {
        throw new Error("The compiler source set exceeds its fixed input region.");
    }
    new Uint8Array(Memory.buffer, Inputˉoffset, Input.byteLength).set(Input);
    const Status = Exports["Windvale.run"](Budget, Input.byteLength);
    const Instructions = Readˉglobal(Exports, "Windvale.instructions");
    if (Status === 3011 && Instructions === Budget) {
        throw new Error(
            "Windvale compilation exceeded the " +
            `${Budget.toLocaleString()}-instruction browser limit. ` +
            "The browser compiler is still experimental; try a smaller source module.",
        );
    }
    if (Status !== 0 || Instructions < 0 || Instructions > Budget) {
        throw new Error(
            `Windvale compilation failed with WVR${Status} ` +
            `after ${Instructions} instructions.`,
        );
    }
    const Outputˉlength = Readˉglobal(Exports, "Windvale.output_length");
    const Outputˉcapacity = Readˉglobal(Exports, "Windvale.output_capacity");
    if (Outputˉlength < 16 || Outputˉlength > Outputˉcapacity) {
        throw new Error("The compiler returned an invalid output length.");
    }
    const Outputˉoffset = Readˉglobal(Exports, "Windvale.output_offset");
    return {
        Instructions,
        Output: new Uint8Array(
            Memory.buffer,
            Outputˉoffset,
            Outputˉlength,
        ).slice(),
    };
}

function Runˉrequest(Exports, Request, Outerˉbudget) {
    const Memory = Exports["Windvale.memory"];
    const Inputˉoffset = Readˉglobal(Exports, "Windvale.input_offset");
    const Inputˉcapacity = Readˉglobal(Exports, "Windvale.input_capacity");
    if (Request.byteLength > Inputˉcapacity) {
        throw new Error("The interpreter request exceeds its fixed input region.");
    }
    new Uint8Array(Memory.buffer, Inputˉoffset, Request.byteLength).set(Request);
    const Outerˉstatus = Exports["Windvale.run"](
        Outerˉbudget,
        Request.byteLength,
    );
    const Outerˉinstructions = Readˉglobal(Exports, "Windvale.instructions");
    const Outputˉlength = Readˉglobal(Exports, "Windvale.output_length");
    const Outputˉcapacity = Readˉglobal(Exports, "Windvale.output_capacity");
    if (Outerˉstatus !== 0 ||
        Outerˉinstructions < 0 ||
        Outerˉinstructions > Outerˉbudget ||
        Outputˉlength < 20 ||
        Outputˉlength > Outputˉcapacity) {
        throw new Error(
            `The outer interpreter failed with status ${Outerˉstatus} ` +
            `after ${Outerˉinstructions} instructions.`,
        );
    }
    const Outputˉoffset = Readˉglobal(Exports, "Windvale.output_offset");
    return {
        Outerˉinstructions,
        Output: new Uint8Array(
            Memory.buffer,
            Outputˉoffset,
            Outputˉlength,
        ).slice(),
    };
}

function Readˉwvxo(Bytes, Expectedˉversion) {
    if (Bytes.byteLength < 20 ||
        Readˉu32(Bytes, 0) !== WVXO_MAGIC ||
        Readˉu16(Bytes, 4) !== Expectedˉversion ||
        Readˉu16(Bytes, 6) !== 0) {
        throw new Error("The interpreter returned an invalid WVXO envelope.");
    }
    let Result;
    if (Expectedˉversion === 1) {
        if (Bytes.byteLength !== 20) {
            throw new Error("The interpreter returned an invalid scalar WVXO length.");
        }
        Result = Bytes.slice(16, 20);
    } else {
        const Resultˉlength = Readˉu32(Bytes, 16);
        if (Bytes.byteLength !== 20 + Resultˉlength) {
            throw new Error("The interpreter returned an inconsistent WVXO result length.");
        }
        Result = Bytes.slice(20);
    }
    return {
        Status: Readˉu32(Bytes, 8),
        Guestˉinstructions: Readˉu32(Bytes, 12),
        Result,
    };
}

function Readˉwvco(Bytes) {
    if (Bytes.byteLength < 16 ||
        Readˉu32(Bytes, 0) !== WVCO_MAGIC ||
        Readˉu16(Bytes, 4) !== 1 ||
        Readˉu16(Bytes, 6) !== 0) {
        throw new Error("The compiler returned an invalid WVCO envelope.");
    }
    const Kind = Readˉu32(Bytes, 8);
    const Payloadˉlength = Readˉu32(Bytes, 12);
    if (Bytes.byteLength !== 16 + Payloadˉlength) {
        throw new Error("The compiler returned an inconsistent WVCO payload length.");
    }
    const Payload = Bytes.slice(16);
    if (Kind === 1) {
        const Diagnostic = new TextDecoder("utf-8", { fatal: true }).decode(Payload);
        throw new Error(`Windvale compilation failed: ${Diagnostic}`);
    }
    if (Kind !== 0 || Payload.byteLength < 12 || Readˉu32(Payload, 0) !== 0x3142_5657) {
        throw new Error("The compiler did not publish canonical WVB bytes.");
    }
    return Payload;
}

function Requireˉinterpreterˉmemoryˉregions(Exports) {
    if (Readˉglobal(Exports, "Windvale.input_offset") !== 65_536 ||
        Readˉglobal(Exports, "Windvale.input_capacity") !== 4_194_304 ||
        Readˉglobal(Exports, "Windvale.output_offset") !== 4_259_840 ||
        Readˉglobal(Exports, "Windvale.output_capacity") !== 4_194_304) {
        throw new Error("The interpreter exposes invalid fixed ABI regions.");
    }
}

function Requireˉcompilerˉmemoryˉregions(Exports) {
    if (Readˉglobal(Exports, "Windvale.input_offset") !== 142_671_872 ||
        Readˉglobal(Exports, "Windvale.input_capacity") !== 4_194_304 ||
        Readˉglobal(Exports, "Windvale.output_offset") !== 146_866_176 ||
        Readˉglobal(Exports, "Windvale.output_capacity") !== 16_777_216) {
        throw new Error("The compiler exposes invalid fixed ABI regions.");
    }
}

function Requireˉfixedˉmemory(Memory, Name) {
    let Memoryˉgrew = false;
    try {
        Memory.grow(1);
        Memoryˉgrew = true;
    }
    catch (Growthˉfailure) {
        if (!(Growthˉfailure instanceof RangeError)) {
            throw Growthˉfailure;
        }
    }
    if (Memoryˉgrew) {
        throw new Error(`The packaged ${Name} memory is not fixed.`);
    }
}

function Requireˉexports(Module, Name) {
    const Expected = [
        ["Windvale.run", "function"], ["Windvale.abi", "global"],
        ["Windvale.memory", "memory"], ["Windvale.input_offset", "global"],
        ["Windvale.input_capacity", "global"], ["Windvale.output_offset", "global"],
        ["Windvale.output_capacity", "global"], ["Windvale.output_length", "global"],
        ["Windvale.output_kind", "global"], ["Windvale.instructions", "global"],
    ];
    const Actual = WebAssembly.Module.exports(Module);
    if (Actual.length !== Expected.length || Expected.some((Item, Index) =>
        Actual[Index].name !== Item[0] || Actual[Index].kind !== Item[1])) {
        throw new Error(`The ${Name} export contract is invalid.`);
    }
}

function Requireˉbytes(Value, Name) {
    if (!(Value instanceof Uint8Array)) {
        throw new TypeError(`The ${Name} must be a Uint8Array.`);
    }
}

function Requireˉinstructionˉlimit(Value) {
    if (!Number.isInteger(Value) || Value < 1 || Value > 20_000_000) {
        throw new Error("The execution instruction limit is invalid.");
    }
}

function Readˉglobal(Exports, Name) {
    const Value = Exports[Name];
    if (!(Value instanceof WebAssembly.Global) || !Number.isInteger(Value.value)) {
        throw new Error(`The '${Name}' export is not an integer global.`);
    }
    return Value.value;
}

function Readˉu16(Bytes, Offset) {
    return new DataView(Bytes.buffer, Bytes.byteOffset, Bytes.byteLength)
        .getUint16(Offset, true);
}

function Readˉu32(Bytes, Offset) {
    return new DataView(Bytes.buffer, Bytes.byteOffset, Bytes.byteLength)
        .getUint32(Offset, true);
}

function Readˉi32(Bytes, Offset) {
    return new DataView(Bytes.buffer, Bytes.byteOffset, Bytes.byteLength)
        .getInt32(Offset, true);
}

function Writeˉu16(View, Offset, Value) {
    View.setUint16(Offset, Value, true);
}

function Writeˉu32(View, Offset, Value) {
    View.setUint32(Offset, Value, true);
}

async function Sha256(Bytes) {
    const Digest = await crypto.subtle.digest(
        "SHA-256",
        Bytes.buffer.slice(Bytes.byteOffset, Bytes.byteOffset + Bytes.byteLength),
    );
    return Array.from(new Uint8Array(Digest))
        .map(Byte => Byte.toString(16).padStart(2, "0"))
        .join("");
}
