using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉnativeˉprobeˉcontract
{
    public const int FORMAT_VERSION = 5;
    public const string TARGET_NAME = "x86-64-kernel-native-wvb-probe-v5";
    public const string BRIDGE_SYMBOL = "Windvale_kernel_x64_native_probe";
    public const string NATIVE_MAIN_SYMBOL = "Main";
    public const int EXPECTED_RESULT = 29;
    public const uint EXACT_INSTRUCTION_BUDGET = 271;
    public const uint EXACT_CALL_DEPTH_BUDGET = 2;
}

public sealed record Kernelˉnativeˉprobeˉartifacts(
    ImmutableArray<byte> Moduleˉbytes,
    ImmutableArray<byte> Nativeˉobjectˉbytes,
    ImmutableArray<byte> Bridgeˉobjectˉbytes);

public static class Kernelˉnativeˉprobe
{
    private const string RESOURCE_NAME = "Windvale.Os.Kernel.Native-Wvb-Probe.wv";
    private const string FAILURE_LABEL = "native_probe_failure";
    private const byte CONDITION_NOT_EQUAL = 0x85;

    public static Kernelˉnativeˉprobeˉartifacts Build()
    {
        var Compilation = Seedˉcompiler.Compile(Loadˉsource(), "Native-Wvb-Probe.wv");
        if (!Compilation.Success)
        {
            throw new InvalidOperationException(
                $"The portable Windvale kernel probe did not compile: {Compilation.Diagnostics[0]}");
        }

        var Verifiedˉmodule = Moduleˉcodec.Readˉandˉverify(Compilation.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Verifiedˉmodule);
        if (Native.Module.Functions.Length != 3 ||
            Native.Module.Data.Length != 2 ||
            !Native.Module.Requiredˉservices.IsEmpty ||
            !Native.Fragment.Requiredˉservices.IsEmpty ||
            !Native.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉcall { Arguments.Length: 2 }) ||
            !Native.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉdataˉloadˉi32) ||
            !Native.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉbytesˉslice) ||
            !Native.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉbytesˉread) ||
            !Native.Module.Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .Any(Operation => Operation is Nativeˉbytesˉlength))
        {
            throw new InvalidOperationException(
                $"The portable kernel probe violated '{Kernelˉnativeˉprobeˉcontract.TARGET_NAME}'.");
        }

        var Nativeˉobjectˉbytes = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Nativeˉobject = Objectˉcodec.Readˉandˉverify(Nativeˉobjectˉbytes.AsSpan()).Value;
        if (Nativeˉobject.Sections.Length != 2 ||
            Nativeˉobject.Sections[0].Kind != Objectˉsectionˉkind.Code ||
            Nativeˉobject.Sections[1].Kind != Objectˉsectionˉkind.Readˉonlyˉdata ||
            Nativeˉobject.Relocations.Length != 4 ||
            Nativeˉobject.Symbols.Count(Symbol =>
                Symbol.Binding == Objectˉsymbolˉbinding.Export &&
                Symbol.Kind == Objectˉsymbolˉkind.Function &&
                Symbol.Name == Kernelˉnativeˉprobeˉcontract.NATIVE_MAIN_SYMBOL) != 1)
        {
            throw new InvalidOperationException(
                $"The native kernel probe object violated '{Kernelˉnativeˉprobeˉcontract.TARGET_NAME}'.");
        }

        return new(
            Compilation.Moduleˉbytes,
            Nativeˉobjectˉbytes,
            Buildˉbridgeˉobject());
    }

    private static ImmutableArray<byte> Buildˉbridgeˉobject()
    {
        var Output = new X64ˉcodeˉbuilder();
        Output.Emit(0x48, 0x83, 0xEC, 0x38);
        Output.Emit(0x48, 0x89, 0x0C, 0x24);
        Output.Emit(0x48, 0xB8);
        Output.Emitˉu64(
            ((ulong)Nativeˉexecutionˉcontextˉcontract.SIZE << 32) |
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x08);
        Output.Emit(0xB8);
        Output.Emitˉu32(Kernelˉnativeˉprobeˉcontract.EXACT_INSTRUCTION_BUDGET);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x10);
        Output.Emit(0xB8);
        Output.Emitˉu32(Kernelˉnativeˉprobeˉcontract.EXACT_CALL_DEPTH_BUDGET);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x18);
        Output.Emit(0x31, 0xC0);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x20);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x28);
        Output.Emit(0x48, 0x89, 0x44, 0x24, 0x30);
        Output.Emit(0x48, 0x8D, 0x54, 0x24, 0x08);
        var Nativeˉcallˉoffset = Output.Emitˉcallˉplaceholder();
        Output.Emit(0x48, 0x83, 0xF8, Kernelˉnativeˉprobeˉcontract.EXPECTED_RESULT);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        Output.Emit(0x48, 0x8B, 0x0C, 0x24);
        Output.Emit(0x48, 0x83, 0xC4, 0x38);
        var Kernelˉjumpˉoffset = Output.Emitˉjumpˉplaceholder();
        Output.Mark(FAILURE_LABEL);
        Output.Emit(0x48, 0x83, 0xC4, 0x38);
        Output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3);
        var Code = Output.Build();

        var Objectˉbytes = Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text.native", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [
                new(
                    Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    (uint)Code.Length),
                new(
                    Kernelˉnativeˉprobeˉcontract.NATIVE_MAIN_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    X64ˉkernelˉcontract.KERNEL_MAIN_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Nativeˉcallˉoffset, 1, -4),
                new(Objectˉrelocationˉkind.Relativeˉi32, 0, Kernelˉjumpˉoffset, 2, -4),
            ])).ToImmutableArray();
        Verifyˉbridge(Objectˉbytes);
        return Objectˉbytes;
    }

    private static void Verifyˉbridge(ImmutableArray<byte> objectˉbytes)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        ReadOnlySpan<byte> Expectedˉcode =
        [
            0x48, 0x83, 0xEC, 0x38, 0x48, 0x89, 0x0C, 0x24,
            0x48, 0xB8, 0x02, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00,
            0x48, 0x89, 0x44, 0x24, 0x08,
            0xB8, 0x0F, 0x01, 0x00, 0x00, 0x48, 0x89, 0x44, 0x24, 0x10,
            0xB8, 0x02, 0x00, 0x00, 0x00, 0x48, 0x89, 0x44, 0x24, 0x18,
            0x31, 0xC0, 0x48, 0x89, 0x44, 0x24, 0x20,
            0x48, 0x89, 0x44, 0x24, 0x28,
            0x48, 0x89, 0x44, 0x24, 0x30,
            0x48, 0x8D, 0x54, 0x24, 0x08,
            0xE8, 0x00, 0x00, 0x00, 0x00, 0x48, 0x83, 0xF8, 0x1D,
            0x0F, 0x85, 0x0D, 0x00, 0x00, 0x00,
            0x48, 0x8B, 0x0C, 0x24, 0x48, 0x83, 0xC4, 0x38,
            0xE9, 0x00, 0x00, 0x00, 0x00,
            0x48, 0x83, 0xC4, 0x38, 0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3,
        ];
        if (Object.Sections.Length != 1 ||
            Object.Sections[0] is not
            {
                Name: ".text.native",
                Kind: Objectˉsectionˉkind.Code,
                Alignment: 16,
            } ||
            !Object.Sections[0].Data.AsSpan().SequenceEqual(Expectedˉcode) ||
            Object.Symbols.Length != 3 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
                Size: 103,
            } ||
            Object.Symbols[1] is not
            {
                Name: Kernelˉnativeˉprobeˉcontract.NATIVE_MAIN_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Symbols[2] is not
            {
                Name: X64ˉkernelˉcontract.KERNEL_MAIN_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Relocations.Length != 2 ||
            Object.Relocations[0] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 66,
                Symbolˉindex: 1,
                Addend: -4,
            } ||
            Object.Relocations[1] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 89,
                Symbolˉindex: 2,
                Addend: -4,
            })
        {
            throw new InvalidOperationException(
                $"The kernel native probe bridge violated '{Kernelˉnativeˉprobeˉcontract.TARGET_NAME}'.");
        }
    }

    private static string Loadˉsource()
    {
        using var Stream = typeof(Kernelˉnativeˉprobe).Assembly.GetManifestResourceStream(RESOURCE_NAME) ??
            throw new InvalidOperationException($"Embedded Windvale source '{RESOURCE_NAME}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
