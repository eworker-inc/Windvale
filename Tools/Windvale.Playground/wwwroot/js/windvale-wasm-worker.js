self.onmessage = async Event => {
    const Message = Event.data;
    const Requestˉid = Message?.RequestId;

    try {
        if (!Number.isInteger(Requestˉid) ||
            !(Message.Bytes instanceof ArrayBuffer) ||
            !Number.isInteger(Message.InstructionLimit) ||
            Message.InstructionLimit < 1 ||
            Message.InstructionLimit > 2_147_483_647) {
            throw new Error("The worker request is invalid.");
        }

        const Bytes = new Uint8Array(Message.Bytes);
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
        else {
            throw new Error("The generated module uses an unsupported Windvale execution ABI.");
        }
        self.postMessage({ RequestId: Requestˉid, Succeeded: true, Error: null, ...Result });
    }
    catch (Error) {
        self.postMessage({
            RequestId: Number.isInteger(Requestˉid) ? Requestˉid : null,
            Succeeded: false,
            ExecutionAbi: null,
            Status: null,
            Result: null,
            ExecutedInstructions: null,
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
