using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉwvbˉadmissionˉcontract
{
    public const int FORMAT_VERSION = 2;
    public const string TARGET_NAME = "x86-64-kernel-wvb-admission-v2";
    public const string BRIDGE_SYMBOL = "Windvale_kernel_x64_wvb_admission";
    public const string ADMISSION_SYMBOL = "Windvale_kernel_wvb_admit";
    public const string EMBEDDED_MAIN_SYMBOL = "Windvale_kernel_embedded_main";
    public const string NATIVE_MAIN_SYMBOL = "Main";
    public const int ADMISSION_TOKEN = 73;
    public const int EXPECTED_RESULT = 29;
    public const uint EXACT_INSTRUCTION_BUDGET = 8_944;
    public const uint EXACT_CALL_DEPTH_BUDGET = 2;
}

public sealed record Kernelˉwvbˉadmissionˉartifacts(
    ImmutableArray<byte> Embeddedˉmoduleˉbytes,
    ImmutableArray<byte> Admissionˉmoduleˉbytes,
    ImmutableArray<byte> Embeddedˉnativeˉobjectˉbytes,
    ImmutableArray<byte> Admissionˉnativeˉobjectˉbytes,
    ImmutableArray<byte> Bridgeˉobjectˉbytes);

public static class Kernelˉwvbˉadmission
{
    private const string EMBEDDED_RESOURCE_NAME = "Windvale.Os.Kernel.Embedded-Wvb-Program.wv";
    private const string ADMISSION_RESOURCE_NAME = "Windvale.Os.Kernel.Wvb-Admission.wv";
    private const string EMBEDDED_DATA_PREFIX = "data Embeddedˉmodule: bytes = [";
    private const string FAILURE_LABEL = "wvb_admission_failure";
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const int MAXIMUM_PROFILE_MODULE_BYTES = 4_096;

    public static Kernelˉwvbˉadmissionˉartifacts Build()
    {
        var Embeddedˉcompilation = Seedˉcompiler.Compile(
            Loadˉsource(EMBEDDED_RESOURCE_NAME),
            "Embedded-Wvb-Program.wv");
        if (!Embeddedˉcompilation.Success)
        {
            throw new InvalidOperationException(
                $"The embedded portable Windvale module did not compile: {Embeddedˉcompilation.Diagnostics[0]}");
        }

        var Embeddedˉmodule = Moduleˉcodec.Readˉandˉverify(Embeddedˉcompilation.Moduleˉbytes.AsSpan());
        Verifyˉembeddedˉmodule(Embeddedˉmodule);

        var Admissionˉsource = Loadˉsource(ADMISSION_RESOURCE_NAME);
        if (!StringComparer.Ordinal.Equals(
                Admissionˉsource,
                Injectˉembeddedˉmodule(Admissionˉsource, Embeddedˉcompilation.Moduleˉbytes)))
        {
            throw new InvalidOperationException(
                "Wvb-Admission.wv does not embed the exact canonical WVB produced from Embedded-Wvb-Program.wv.");
        }

        var Admissionˉmoduleˉbytes = Compileˉadmissionˉmodule(Admissionˉsource);
        var Admissionˉmodule = Moduleˉcodec.Readˉandˉverify(Admissionˉmoduleˉbytes.AsSpan());
        Verifyˉadmissionˉmodule(Admissionˉmodule);

        var Embeddedˉnative = X64ˉnativeˉbackend.Compile(Embeddedˉmodule);
        var Admissionˉnative = X64ˉnativeˉbackend.Compile(Admissionˉmodule);
        var Embeddedˉobjectˉbytes = Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Embeddedˉnative.Fragment),
            Kernelˉwvbˉadmissionˉcontract.EMBEDDED_MAIN_SYMBOL);
        var Admissionˉobjectˉbytes = Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Admissionˉnative.Fragment),
            Kernelˉwvbˉadmissionˉcontract.ADMISSION_SYMBOL);

        return new(
            Embeddedˉcompilation.Moduleˉbytes,
            Admissionˉmoduleˉbytes,
            Embeddedˉobjectˉbytes,
            Admissionˉobjectˉbytes,
            Buildˉbridgeˉobject());
    }

    // Stage 0 test seam: replace only the candidate bytes while retaining the
    // Windvale-owned canonical policy encoded by Expectedˉmodule.
    public static ImmutableArray<byte> Compileˉadmissionˉmodule(ImmutableArray<byte> embeddedˉmoduleˉbytes)
    {
        if (embeddedˉmoduleˉbytes.IsDefault || embeddedˉmoduleˉbytes.Length > MAXIMUM_PROFILE_MODULE_BYTES)
        {
            throw new ArgumentOutOfRangeException(nameof(embeddedˉmoduleˉbytes));
        }

        return Compileˉadmissionˉmodule(Injectˉembeddedˉmodule(
            Loadˉsource(ADMISSION_RESOURCE_NAME),
            embeddedˉmoduleˉbytes));
    }

    private static ImmutableArray<byte> Compileˉadmissionˉmodule(string source)
    {
        var Compilation = Seedˉcompiler.Compile(source, "Wvb-Admission.wv");
        if (!Compilation.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale WVB admission module did not compile: {Compilation.Diagnostics[0]}");
        }
        return Compilation.Moduleˉbytes;
    }

    private static void Verifyˉembeddedˉmodule(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Embeddedˉwvbˉprogram",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 0,
                Functions.Length: 1,
                Exports.Length: 1,
                Types.Length: 0,
            } ||
            module.Functions[0].Declaration is not
            {
                Name: "Main",
                Parameterˉtypes.Length: 0,
                Returnˉtype.Kind: Valueˉtype.I32,
                Maximumˉstackˉdepth: 1,
            } ||
            module.Functions[0].Instructions.Select(Instruction => Instruction.Opcode).ToArray() is not
                [Opcode.I32ˉconst, Opcode.Localˉstore, Opcode.Localˉload, Opcode.Return] ||
            module.Functions[0].Instructions[0].Signedˉoperand != Kernelˉwvbˉadmissionˉcontract.EXPECTED_RESULT)
        {
            throw new InvalidOperationException(
                $"The embedded module violated '{Kernelˉwvbˉadmissionˉcontract.TARGET_NAME}'.");
        }
    }

    private static void Verifyˉadmissionˉmodule(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Wvbˉadmission",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 2,
                Functions.Length: 3,
                Exports.Length: 1,
                Types.Length: 0,
            } ||
            module.Module.Data.Any(Data => Data is not Bytesˉdataˉdeclaration) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Bytesˉreadˉu8) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Bytesˉreadˉu32ˉlittle) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Call))
        {
            throw new InvalidOperationException(
                $"The Windvale admission module violated '{Kernelˉwvbˉadmissionˉcontract.TARGET_NAME}'.");
        }
    }

    internal static ImmutableArray<byte> Renameˉmainˉexport(
        ImmutableArray<byte> objectˉbytes,
        string replacementˉsymbol)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Symbols.Count(Symbol =>
                Symbol.Binding == Objectˉsymbolˉbinding.Export &&
                Symbol.Kind == Objectˉsymbolˉkind.Function &&
                Symbol.Name == Kernelˉwvbˉadmissionˉcontract.NATIVE_MAIN_SYMBOL) != 1 ||
            Object.Symbols.Any(Symbol => StringComparer.Ordinal.Equals(Symbol.Name, replacementˉsymbol)))
        {
            throw new InvalidOperationException(
                $"The Stage 0 native object cannot publish '{replacementˉsymbol}'.");
        }

        var Orderedˉsymbols = Object.Symbols
            .Select((Symbol, Oldˉindex) => new
            {
                Symbol = Symbol.Binding == Objectˉsymbolˉbinding.Export &&
                    Symbol.Kind == Objectˉsymbolˉkind.Function &&
                    Symbol.Name == Kernelˉwvbˉadmissionˉcontract.NATIVE_MAIN_SYMBOL
                        ? Symbol with { Name = replacementˉsymbol }
                        : Symbol,
                Oldˉindex,
            })
            .OrderBy(Entry => Entry.Symbol.Binding)
            .ThenBy(Entry => Entry.Symbol.Name, StringComparer.Ordinal)
            .ToImmutableArray();
        var Newˉindices = Orderedˉsymbols
            .Select((Entry, Newˉindex) => (Entry.Oldˉindex, Newˉindex))
            .ToDictionary(Entry => Entry.Oldˉindex, Entry => Entry.Newˉindex);
        var Renamed = Object with
        {
            Symbols = [.. Orderedˉsymbols.Select(Entry => Entry.Symbol)],
            Relocations = [.. Object.Relocations.Select(Relocation => Relocation with
            {
                Symbolˉindex = checked((uint)Newˉindices[checked((int)Relocation.Symbolˉindex)]),
            })],
        };
        var Bytes = Objectˉcodec.Write(Renamed).ToImmutableArray();
        var Verified = Objectˉcodec.Readˉandˉverify(Bytes.AsSpan()).Value;
        if (Verified.Symbols.Count(Symbol =>
                Symbol.Binding == Objectˉsymbolˉbinding.Export &&
                Symbol.Kind == Objectˉsymbolˉkind.Function &&
                Symbol.Name == replacementˉsymbol) != 1 ||
            Verified.Symbols.Any(Symbol =>
                Symbol.Binding == Objectˉsymbolˉbinding.Export &&
                Symbol.Name == Kernelˉwvbˉadmissionˉcontract.NATIVE_MAIN_SYMBOL))
        {
            throw new InvalidOperationException(
                $"The Stage 0 native object did not publish '{replacementˉsymbol}'.");
        }
        return Bytes;
    }

    private static ImmutableArray<byte> Buildˉbridgeˉobject()
    {
        var Output = new X64ˉcodeˉbuilder();
        Output.Emit(0x48, 0x83, 0xEC, 0x78);
        Output.Emit(0x48, 0x89, 0x0C, 0x24);
        Output.Emit(0x48, 0xB8);
        Output.Emitˉu64(
            ((ulong)Nativeˉexecutionˉcontextˉcontract.SIZE << 32) |
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x08);
        Output.Emit(0xB8);
        Output.Emitˉu32(Kernelˉwvbˉadmissionˉcontract.EXACT_INSTRUCTION_BUDGET);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x10);
        Output.Emit(0xB8);
        Output.Emitˉu32(Kernelˉwvbˉadmissionˉcontract.EXACT_CALL_DEPTH_BUDGET);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x18);
        Output.Emit(0x31, 0xC0);
        for (byte Offset = 0x20; Offset <= 0x70; Offset += 0x08)
        {
            Output.Emit(0x48, 0x89, 0x44, 0x24, Offset);
        }

        Output.Emit(0x48, 0x8D, 0x54, 0x24, 0x08);
        var Admissionˉcallˉoffset = Output.Emitˉcallˉplaceholder();
        Output.Emit(0x48, 0x83, 0xF8, Kernelˉwvbˉadmissionˉcontract.ADMISSION_TOKEN);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Output.Emit(0x48, 0x8B, 0x0C, 0x24);
        var Processˉcallˉoffset = Output.Emitˉcallˉplaceholder();
        Output.Emit(0x48, 0x83, 0xF8, Kernelˉwvbˉadmissionˉcontract.EXPECTED_RESULT);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Output.Emit(0x48, 0x8B, 0x0C, 0x24);
        Output.Emit(0x48, 0x83, 0xC4, 0x78);
        var Nativeˉprobeˉjumpˉoffset = Output.Emitˉjumpˉplaceholder();
        Output.Mark(FAILURE_LABEL);
        Output.Emit(0x48, 0x83, 0xC4, 0x78);
        Output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3);
        var Code = Output.Build();

        var Objectˉbytes = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text.admission", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [
                new(
                    Kernelˉwvbˉadmissionˉcontract.BRIDGE_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    (uint)Code.Length),
                Import(Kernelˉwvbˉadmissionˉcontract.ADMISSION_SYMBOL),
                Import(Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL),
                Import(Kernelˉprocessˉcontract.ENTER_SYMBOL),
            ],
            [
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Admissionˉcallˉoffset, 1, -4),
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Processˉcallˉoffset, 3, -4),
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Nativeˉprobeˉjumpˉoffset, 2, -4),
            ])).ToImmutableArray();
        Verifyˉbridge(Objectˉbytes);
        return Objectˉbytes;
    }

    private static Objectˉsymbol Import(string name) => new(
        name,
        Objectˉsymbolˉbinding.Import,
        Objectˉsymbolˉkind.Function,
        Objectˉlimits.UNDEFINED_SECTION,
        0,
        0);

    private static void Verifyˉbridge(ImmutableArray<byte> objectˉbytes)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Sections.Length != 1 ||
            Object.Sections[0] is not
            {
                Name: ".text.admission",
                Kind: Objectˉsectionˉkind.Code,
                Alignment: 16,
                Memoryˉsize: 162,
            } ||
            Object.Symbols.Length != 4 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉwvbˉadmissionˉcontract.BRIDGE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
                Size: 162,
            } ||
            Object.Symbols[1].Name != Kernelˉwvbˉadmissionˉcontract.ADMISSION_SYMBOL ||
            Object.Symbols[2].Name != Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL ||
            Object.Symbols[3].Name != Kernelˉprocessˉcontract.ENTER_SYMBOL ||
            Object.Symbols.Skip(1).Any(Symbol =>
                Symbol.Binding != Objectˉsymbolˉbinding.Import ||
                Symbol.Kind != Objectˉsymbolˉkind.Function) ||
            Object.Relocations is not
            [
                { Kind: Objectˉrelocationˉkind.Relativeˉi32, Sectionˉindex: 0, Offset: 106, Symbolˉindex: 1, Addend: -4 },
                { Kind: Objectˉrelocationˉkind.Relativeˉi32, Sectionˉindex: 0, Offset: 125, Symbolˉindex: 3, Addend: -4 },
                { Kind: Objectˉrelocationˉkind.Relativeˉi32, Sectionˉindex: 0, Offset: 148, Symbolˉindex: 2, Addend: -4 },
            ])
        {
            throw new InvalidOperationException(
                $"The WVB admission bridge violated '{Kernelˉwvbˉadmissionˉcontract.TARGET_NAME}': " +
                $"code={Object.Sections[0].Memoryˉsize}, relocations=" +
                string.Join(",", Object.Relocations.Select(Relocation =>
                    $"{Relocation.Offset}:{Relocation.Symbolˉindex}")) + ".");
        }
    }

    private static string Injectˉembeddedˉmodule(
        string source,
        ImmutableArray<byte> moduleˉbytes)
    {
        var Valuesˉstart = source.IndexOf(EMBEDDED_DATA_PREFIX, StringComparison.Ordinal);
        if (Valuesˉstart < 0)
        {
            throw new InvalidOperationException("Wvb-Admission.wv is missing its embedded-module declaration.");
        }
        Valuesˉstart += EMBEDDED_DATA_PREFIX.Length;
        var Valuesˉend = source.IndexOf("];", Valuesˉstart, StringComparison.Ordinal);
        if (Valuesˉend < 0 ||
            source.IndexOf(EMBEDDED_DATA_PREFIX, Valuesˉstart, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException("Wvb-Admission.wv has an ambiguous embedded-module declaration.");
        }

        return string.Concat(
            source.AsSpan(0, Valuesˉstart),
            string.Join(", ", moduleˉbytes),
            source.AsSpan(Valuesˉend));
    }

    private static string Loadˉsource(string resourceˉname)
    {
        using var Stream = typeof(Kernelˉwvbˉadmission).Assembly.GetManifestResourceStream(resourceˉname) ??
            throw new InvalidOperationException($"Embedded Windvale source '{resourceˉname}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
