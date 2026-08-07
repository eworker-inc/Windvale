import {
    copyFile,
    readFile,
    writeFile,
} from "node:fs/promises";
import { constants as Fsˉconstants } from "node:fs";
import path from "node:path";
import { Decodeˉutf8, Sha256 } from "./Random-Containment-Corpus.mjs";
import {
    Decodeˉbase64,
    Forˉeachˉbounded,
    Hostˉartifact,
    Oneˉline,
    Require,
    Requireˉinputˉpreserved,
    Runˉprocess,
    Verifyˉartifact,
} from "./Random-Containment-Host.mjs";

export async function Testˉsource(Repositoryˉroot, Temporaryˉdirectory, Cases) {
    Require(Cases.length === 500, "The selected source case count differs.");
    const Compilerˉartifact = [
        "Artifacts/WebAssembly-Playground/Windvale-Compiler-Direct.wasm",
        18_349_927,
        "05dcee4e37cdd8db2e7321b01f0b9cde4d13662ba1f154830c95fda753b825e8",
    ];
    const Compiler = await Verifyˉartifact(
        Repositoryˉroot,
        Compilerˉartifact,
        false,
        "direct source compiler",
    );
    Require(WebAssembly.validate(Compiler.Content), "The direct compiler Wasm is invalid.");
    const Compilerˉmodule = await WebAssembly.compile(Compiler.Content);
    Require(
        WebAssembly.Module.imports(Compilerˉmodule).length === 0,
        "The direct compiler imports a host capability.",
    );
    Requireˉexports(Compilerˉmodule);
    for (const Case of Cases) {
        Testˉcompilerˉcase(Compilerˉmodule, Case);
        global.gc();
    }

    const Assemblerˉartifact = Hostˉartifact({
        win32: [
            "Artifacts/Native-Front-Door/windows-x64/wvasm.exe",
            2_895_360,
            "e03a1f22317fef36213d14a0a669b262f81143a54cbe334da075901987268ed4",
        ],
        linux: [
            "Artifacts/Native-Front-Door/linux-x64/wvasm.elf",
            2_895_872,
            "ebe18959f2a057db5181f4e2bbf7979fac9359d50542581b63da6dc48c4163a0",
        ],
    });
    const Assembler = await Verifyˉartifact(
        Repositoryˉroot,
        Assemblerˉartifact,
        true,
        "native assembler",
    );
    const Sentinel = Decodeˉbase64(await readFile(path.join(
        Repositoryˉroot,
        "Tests/Native/Wvo/Return-42.wvo.b64",
    ), "ascii"));
    const Sentinelˉdigest =
        "0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5";
    Require(Sha256(Sentinel) === Sentinelˉdigest, "The assembler sentinel identity differs.");
    const Sentinelˉpath = path.join(Temporaryˉdirectory, "Sentinel.wvo");
    await writeFile(Sentinelˉpath, Sentinel, { flag: "wx" });

    await Forˉeachˉbounded(Cases, 4, async Case => {
        const Destination = path.join(
            Temporaryˉdirectory,
            `Destination-${Case.Number.toString().padStart(3, "0")}.wvo`,
        );
        await copyFile(Sentinelˉpath, Destination, Fsˉconstants.COPYFILE_EXCL);
        const Result = await Runˉprocess(
            Assembler.Fileˉpath,
            [Case.Inputˉpath, Destination],
        );
        Require(Result.Code === 2, `${Case.Name}: native assembler exit differs.`);
        Require(Result.Output.byteLength === 0, `${Case.Name}: native assembler wrote output.`);
        const Diagnostic = Oneˉline(Result.Error, `${Case.Name} assembler diagnostic`);
        Require(
            Diagnostic.startsWith(`assembly status=${Case.Secondaryˉcode} `),
            `${Case.Name}: native assembler code differs.`,
        );
        Require(
            Sha256(await readFile(Destination)) === Sentinelˉdigest,
            `${Case.Name}: rejected assembly changed the destination.`,
        );
        await Requireˉinputˉpreserved(Case);
    });
}

function Testˉcompilerˉcase(Module, Case) {
    const Instance = new WebAssembly.Instance(Module, {});
    const Exports = Instance.exports;
    const Memory = Exports["Windvale.memory"];
    Require(Readˉglobal(Exports, "Windvale.abi") === 4, "The compiler ABI differs.");
    Require(Readˉglobal(Exports, "Windvale.output_kind") === 1,
        "The compiler output kind differs.");
    Require(
        Memory instanceof WebAssembly.Memory &&
            Memory.buffer.byteLength === 2_497 * 65_536,
        "The compiler memory extent differs.",
    );
    const Input = Case.Bytes.byteLength === 0
        ? new Uint8Array()
        : Buildˉsourceˉset(Case.Bytes);
    const Inputˉoffset = Readˉglobal(Exports, "Windvale.input_offset");
    Require(Inputˉoffset === 142_671_872, "The compiler input offset differs.");
    Require(Readˉglobal(Exports, "Windvale.input_capacity") === 4_194_304,
        "The compiler input capacity differs.");
    new Uint8Array(Memory.buffer, Inputˉoffset, Input.byteLength).set(Input);
    const Status = Exports["Windvale.run"](2_000_000, Input.byteLength);
    Require(Status === 0, `${Case.Name}: compiler execution returned WVR${Status}.`);
    const Instructions = Readˉglobal(Exports, "Windvale.instructions");
    Require(Instructions >= 0 && Instructions <= 2_000_000,
        `${Case.Name}: compiler instruction evidence differs.`);
    const Outputˉlength = Readˉglobal(Exports, "Windvale.output_length");
    const Outputˉcapacity = Readˉglobal(Exports, "Windvale.output_capacity");
    Require(Outputˉcapacity === 16_777_216, "The compiler output capacity differs.");
    Require(Outputˉlength >= 16 && Outputˉlength <= Outputˉcapacity,
        `${Case.Name}: compiler output length differs.`);
    const Outputˉoffset = Readˉglobal(Exports, "Windvale.output_offset");
    Require(Outputˉoffset === 146_866_176, "The compiler output offset differs.");
    const Output = new Uint8Array(
        Memory.buffer,
        Outputˉoffset,
        Outputˉlength,
    ).slice();
    const View = new DataView(Output.buffer, Output.byteOffset, Output.byteLength);
    Require(View.getUint32(0, true) === 0x4F43_5657, `${Case.Name}: WVCO magic differs.`);
    Require(View.getUint16(4, true) === 1 && View.getUint16(6, true) === 0,
        `${Case.Name}: WVCO version or flags differ.`);
    Require(View.getUint32(8, true) === 1, `${Case.Name}: compiler outcome differs.`);
    const Payloadˉlength = View.getUint32(12, true);
    Require(Output.byteLength === 16 + Payloadˉlength,
        `${Case.Name}: WVCO payload length differs.`);
    const Diagnostic = Decodeˉutf8(Output.slice(16), `${Case.Name} compiler diagnostic`);
    Require(Diagnostic.startsWith("source-wvb status="),
        `${Case.Name}: compiler diagnostic shape differs.`);
}

function Buildˉsourceˉset(Source) {
    const Result = new Uint8Array(24 + Source.byteLength);
    const View = new DataView(Result.buffer);
    View.setUint32(0, 0x5353_5657, true);
    View.setUint16(4, 1, true);
    View.setUint16(6, 0, true);
    View.setUint32(8, 1, true);
    View.setUint32(12, 8, true);
    View.setUint32(16, 24, true);
    View.setUint32(20, Source.byteLength, true);
    Result.set(Source, 24);
    return Result;
}

function Requireˉexports(Module) {
    const Expected = [
        ["Windvale.run", "function"], ["Windvale.abi", "global"],
        ["Windvale.memory", "memory"], ["Windvale.input_offset", "global"],
        ["Windvale.input_capacity", "global"], ["Windvale.output_offset", "global"],
        ["Windvale.output_capacity", "global"], ["Windvale.output_length", "global"],
        ["Windvale.output_kind", "global"], ["Windvale.instructions", "global"],
    ];
    const Actual = WebAssembly.Module.exports(Module).map(Item => [Item.name, Item.kind]);
    Require(JSON.stringify(Actual) === JSON.stringify(Expected),
        "The direct compiler export contract differs.");
}

function Readˉglobal(Exports, Name) {
    const Global = Exports[Name];
    Require(Global instanceof WebAssembly.Global && Number.isInteger(Global.value),
        `The '${Name}' compiler export is invalid.`);
    return Global.value;
}
