self.onmessage = async Event => {
    const Message = Event.data;
    const Requestˉid = Message?.RequestId;

    try {
        if (!Number.isInteger(Requestˉid) ||
            !(Message.Bytes instanceof ArrayBuffer) ||
            !(Message.Input instanceof ArrayBuffer) ||
            Message.Input.byteLength > 4 * 1024 * 1024 ||
            !Number.isInteger(Message.InstructionLimit) ||
            Message.InstructionLimit < 1 ||
            Message.InstructionLimit > 2_147_483_647) {
            throw new Error("The worker request is invalid.");
        }

        const Bytes = new Uint8Array(Message.Bytes);
        const Input = new Uint8Array(Message.Input);
        if (Bytes.byteLength === 0 || Bytes.byteLength > 64 * 1024) {
            throw new Error("The WebAssembly module is outside the worker size limit.");
        }
        if (!WebAssembly.validate(Bytes)) {
            throw new Error("The browser rejected the generated WebAssembly module.");
        }

        const Module = await WebAssembly.compile(Bytes);
        if (WebAssembly.Module.imports(Module).length !== 0) {
            throw new Error("The generated WebAssembly module must not import host capabilities.");
        }

        const Instance = await WebAssembly.instantiate(Module, {});
        const Exports = Instance.exports;
        const Abiˉexport = Exports["Windvale.abi"];
        const Abi = Abiˉexport === undefined
            ? 0
            : Readˉi32ˉglobal(Exports, "Windvale.abi");
        let Result;
        if (Abi === 0) {
            Result = Executeˉabiˉzero(Module, Exports);
        }
        else if (Abi === 1) {
            Result = Executeˉabiˉone(Module, Exports);
        }
        else if (Abi === 2) {
            Result = Executeˉabiˉtwo(Module, Exports, Message.InstructionLimit);
        }
        else if (Abi === 3) {
            Result = Executeˉabiˉthree(
                Module,
                Exports,
                Message.InstructionLimit,
                Input,
            );
        }
        else {
            throw new Error("The generated module uses an unsupported Windvale execution ABI.");
        }
        if (Abi !== 3 && Input.byteLength !== 0) {
            throw new Error("This Windvale execution ABI does not accept an input buffer.");
        }
        const Response = { RequestId: Requestˉid, Succeeded: true, Error: null, ...Result };
        if (Result.Output instanceof ArrayBuffer) {
            self.postMessage(Response, [Result.Output]);
        }
        else {
            self.postMessage(Response);
        }
    }
    catch (Error) {
        self.postMessage({
            RequestId: Number.isInteger(Requestˉid) ? Requestˉid : null,
            Succeeded: false,
            ExecutionAbi: null,
            Status: null,
            Result: null,
            ExecutedInstructions: null,
            OutputKind: null,
            Output: null,
            Error: Error instanceof Error ? Error.message : "The WebAssembly worker failed.",
        });
    }
};

function Executeˉabiˉzero(Module, Exports) {
    Requireˉexports(Module, [{ name: "Main", kind: "function" }]);
    if (typeof Exports.Main !== "function") {
        throw new Error("Execution ABI 0 is missing Main().");
    }

    const Result = Exports.Main();
    Requireˉi32(Result, "Execution ABI 0 result");
    return {
        ExecutionAbi: 0,
        Status: 0,
        Result,
        ExecutedInstructions: null,
        OutputKind: null,
        Output: null,
    };
}

function Executeˉabiˉone(Module, Exports) {
    Requireˉexports(Module, [
        { name: "Windvale.run", kind: "function" },
        { name: "Windvale.abi", kind: "global" },
        { name: "Windvale.result", kind: "global" },
        { name: "Windvale.instructions", kind: "global" },
    ]);
    if (Readˉi32ˉglobal(Exports, "Windvale.abi") !== 1) {
        throw new Error("The generated module uses an unsupported Windvale execution ABI.");
    }
    if (typeof Exports["Windvale.run"] !== "function") {
        throw new Error("Execution ABI 1 is missing Windvale.run().");
    }

    const Returnedˉstatus = Exports["Windvale.run"]();
    Requireˉi32(Returnedˉstatus, "Windvale.run status");
    const Result = Readˉi32ˉglobal(Exports, "Windvale.result");
    const Instructionsˉvalue = Readˉglobal(Exports, "Windvale.instructions");
    if (!Number.isInteger(Instructionsˉvalue) || Instructionsˉvalue < 0) {
        throw new Error("The generated module published an invalid instruction count.");
    }

    return {
        ExecutionAbi: 1,
        Status: Returnedˉstatus,
        Result,
        ExecutedInstructions: Instructionsˉvalue,
        OutputKind: null,
        Output: null,
    };
}

function Executeˉabiˉtwo(Module, Exports, Instructionˉlimit) {
    Requireˉexports(Module, [
        { name: "Windvale.run", kind: "function" },
        { name: "Windvale.abi", kind: "global" },
        { name: "Windvale.result", kind: "global" },
        { name: "Windvale.instructions", kind: "global" },
    ]);
    if (typeof Exports["Windvale.run"] !== "function") {
        throw new Error("Execution ABI 2 is missing Windvale.run(limit).");
    }

    const Returnedˉstatus = Exports["Windvale.run"](Instructionˉlimit);
    Requireˉi32(Returnedˉstatus, "Windvale.run status");
    const Result = Readˉi32ˉglobal(Exports, "Windvale.result");
    const Instructionsˉvalue = Readˉglobal(Exports, "Windvale.instructions");
    if (!Number.isInteger(Instructionsˉvalue) ||
        Instructionsˉvalue < 0 ||
        Instructionsˉvalue > Instructionˉlimit) {
        throw new Error("The generated module published an invalid metered instruction count.");
    }

    return {
        ExecutionAbi: 2,
        Status: Returnedˉstatus,
        Result,
        ExecutedInstructions: Instructionsˉvalue,
        OutputKind: null,
        Output: null,
    };
}

function Executeˉabiˉthree(Module, Exports, Instructionˉlimit, Input) {
    Requireˉexports(Module, [
        { name: "Windvale.run", kind: "function" },
        { name: "Windvale.abi", kind: "global" },
        { name: "Windvale.memory", kind: "memory" },
        { name: "Windvale.input_offset", kind: "global" },
        { name: "Windvale.input_capacity", kind: "global" },
        { name: "Windvale.output_offset", kind: "global" },
        { name: "Windvale.output_capacity", kind: "global" },
        { name: "Windvale.output_length", kind: "global" },
        { name: "Windvale.output_kind", kind: "global" },
        { name: "Windvale.instructions", kind: "global" },
    ]);
    if (typeof Exports["Windvale.run"] !== "function") {
        throw new Error("Execution ABI 3 is missing Windvale.run(limit, input_length).");
    }

    const Memory = Exports["Windvale.memory"];
    if (!(Memory instanceof WebAssembly.Memory) ||
        Memory.buffer.byteLength !== 129 * 65_536) {
        throw new Error("Execution ABI 3 has an invalid linear-memory extent.");
    }
    let Grew = false;
    try {
        Memory.grow(1);
        Grew = true;
    }
    catch (Error) {
        if (!(Error instanceof RangeError)) {
            throw Error;
        }
    }
    if (Grew) {
        throw new Error("Execution ABI 3 memory is not fixed at its declared maximum.");
    }

    const Inputˉoffset = Readˉi32ˉglobal(Exports, "Windvale.input_offset");
    const Inputˉcapacity = Readˉi32ˉglobal(Exports, "Windvale.input_capacity");
    const Outputˉoffset = Readˉi32ˉglobal(Exports, "Windvale.output_offset");
    const Outputˉcapacity = Readˉi32ˉglobal(Exports, "Windvale.output_capacity");
    const Outputˉkind = Readˉi32ˉglobal(Exports, "Windvale.output_kind");
    if (Inputˉoffset !== 65_536 ||
        Inputˉcapacity !== 4_194_304 ||
        Outputˉoffset !== 4_259_840 ||
        Outputˉcapacity !== 4_194_304 ||
        (Outputˉkind !== 1 && Outputˉkind !== 2) ||
        Input.byteLength > Inputˉcapacity ||
        Inputˉoffset + Inputˉcapacity > Outputˉoffset ||
        Outputˉoffset + Outputˉcapacity > Memory.buffer.byteLength) {
        throw new Error("Execution ABI 3 has an invalid buffer-region contract.");
    }

    new Uint8Array(Memory.buffer, Inputˉoffset, Input.byteLength).set(Input);
    const Returnedˉstatus = Exports["Windvale.run"](
        Instructionˉlimit,
        Input.byteLength,
    );
    Requireˉi32(Returnedˉstatus, "Windvale.run status");
    const Instructionsˉvalue = Readˉi32ˉglobal(Exports, "Windvale.instructions");
    const Outputˉlength = Readˉi32ˉglobal(Exports, "Windvale.output_length");
    if (Instructionsˉvalue < 0 || Instructionsˉvalue > Instructionˉlimit) {
        throw new Error("The generated module published an invalid metered instruction count.");
    }
    if (Outputˉlength < 0 ||
        Outputˉlength > Outputˉcapacity ||
        (Returnedˉstatus !== 0 && Outputˉlength !== 0)) {
        throw new Error("The generated module published an invalid output descriptor.");
    }

    const Output = new Uint8Array(
        Memory.buffer,
        Outputˉoffset,
        Outputˉlength,
    ).slice();
    if (Returnedˉstatus === 0 && Outputˉkind === 2) {
        try {
            new TextDecoder("utf-8", { fatal: true }).decode(Output);
        }
        catch {
            throw new Error("The generated module published malformed UTF-8 text.");
        }
    }
    return {
        ExecutionAbi: 3,
        Status: Returnedˉstatus,
        Result: null,
        ExecutedInstructions: Instructionsˉvalue,
        OutputKind: Outputˉkind,
        Output: Output.buffer,
    };
}

function Requireˉexports(Module, Expected) {
    const Actual = WebAssembly.Module.exports(Module);
    if (Actual.length !== Expected.length ||
        Expected.some((Item, Index) =>
            Actual[Index].name !== Item.name || Actual[Index].kind !== Item.kind)) {
        throw new Error("The generated module does not match the selected export contract.");
    }
}

function Readˉi32ˉglobal(Exports, Name) {
    const Value = Readˉglobal(Exports, Name);
    Requireˉi32(Value, Name);
    return Value;
}

function Readˉglobal(Exports, Name) {
    const Global = Exports[Name];
    if (!(Global instanceof WebAssembly.Global)) {
        throw new Error(`Execution ABI export '${Name}' is not a global.`);
    }
    return Global.value;
}

function Requireˉi32(Value, Name) {
    if (!Number.isInteger(Value) || Value < -2147483648 || Value > 2147483647) {
        throw new Error(`${Name} is not an i32 value.`);
    }
}
