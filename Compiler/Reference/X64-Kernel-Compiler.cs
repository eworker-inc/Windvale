using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler;

public sealed record X64ˉkernelˉcompilationˉresult(
    ImmutableArray<byte> Objectˉbytes,
    ImmutableArray<Compilerˉdiagnostic> Diagnostics)
{
    public bool Success => Diagnostics.IsEmpty;
}

public static class X64ˉkernelˉcontract
{
    public const int FORMAT_VERSION = 2;
    public const string TARGET_NAME = "x86-64-kernel-entry-wvo-v2";
    public const string KERNEL_ENTRY_SYMBOL = "Windvale_kernel_entry";
    public const string KERNEL_MAIN_SYMBOL = "Windvale_kernel_main";
    public const string MEMORY_ENTER_SYMBOL = "Windvale_kernel_memory_enter";
    public const string WRITE_BYTE_SYMBOL = "Windvale_kernel_write_byte";
    public const ulong HANDOFF_MAGIC = 0x3144_4E41_484B_5657;
    public const uint HANDOFF_VERSION = 1;
    public const uint HANDOFF_BYTES = 48;
    public const uint MAX_MEMORY_MAP_BYTES = 1024 * 1024;
    public const uint MINIMUM_DESCRIPTOR_BYTES = 40;
    public const uint MAXIMUM_DESCRIPTOR_BYTES = 256;
    public const uint DESCRIPTOR_VERSION = 1;
    public const int MAXIMUM_OUTPUT_BYTES = 4 * 1024;
}

public static class X64ˉkernelˉcompiler
{
    private const string PHASE = "native-backend";
    private const string FAILURE_LABEL = "handoff_failure";

    private const byte CONDITION_BELOW = 0x82;
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const byte CONDITION_ABOVE = 0x87;

    public static X64ˉkernelˉcompilationˉresult Compile(
        string source,
        string sourceˉname = "<memory>")
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceˉname);

        var Diagnostics = new Diagnosticˉbag();
        var Syntax = Sourceˉmoduleˉcomposition.Compose(
            new(sourceˉname, source),
            [],
            Diagnostics);
        if (Syntax is null || Diagnostics.Count != 0)
        {
            return new([], Diagnostics.Toˉimmutable());
        }

        var Wir = Semanticˉcompiler.Compile(Syntax, Diagnostics);
        if (Diagnostics.Count != 0)
        {
            return new([], Diagnostics.Toˉimmutable());
        }

        var Plan = Analyze(Wir, sourceˉname, Diagnostics);
        if (Plan is null || Diagnostics.Count != 0)
        {
            return new([], Diagnostics.Toˉimmutable());
        }

        try
        {
            var Object = Emit(Plan);
            var Bytes = Objectˉcodec.Write(Object);
            _ = Objectˉcodec.Readˉandˉverify(Bytes);
            return new(Bytes.ToImmutableArray(), []);
        }
        catch (Objectˉexception Exception)
        {
            Report(
                Diagnostics,
                sourceˉname,
                "WVN9000",
                $"The generated native object failed validation: {Exception.Message}");
            return new([], Diagnostics.Toˉimmutable());
        }
    }

    private static Nativeˉplan? Analyze(
        Wirˉmodule module,
        string sourceˉname,
        Diagnosticˉbag diagnostics)
    {
        if (module.Profile != Moduleˉprofile.System)
        {
            Report(
                diagnostics,
                sourceˉname,
                "WVN1001",
                $"Target '{X64ˉkernelˉcontract.TARGET_NAME}' requires the system module profile.");
        }

        if (module.Capabilities.Any(Capability =>
                !StringComparer.Ordinal.Equals(Capability.Name, Capabilityˉcatalog.CONSOLE_WRITE_LINE)))
        {
            Report(
                diagnostics,
                sourceˉname,
                "WVN1002",
                "The first kernel target supports only the declared console.write_line capability.");
        }

        if (!module.Types.IsEmpty || module.Data.Any(Data => Data is not Textˉdataˉdeclaration))
        {
            Report(
                diagnostics,
                sourceˉname,
                "WVN1002",
                "The first kernel target supports text data and no nominal types.");
        }

        if (module.Functions.Length != 1)
        {
            Report(
                diagnostics,
                sourceˉname,
                "WVN1003",
                "The first kernel target requires exactly one function.");
            return null;
        }

        var Function = module.Functions[0];
        if (!StringComparer.Ordinal.Equals(Function.Name, "Main") ||
            !Function.Isˉexported ||
            !Function.Parameterˉtypes.IsEmpty ||
            Function.Returnˉtype != Valueˉtype.I32 ||
            !Function.Userˉlocalˉtypes.IsEmpty ||
            Function.Blocks.Length != 1 ||
            Function.Blocks[0].Id != 0)
        {
            Report(
                diagnostics,
                sourceˉname,
                "WVN1003",
                "The kernel entry must be the sole exported 'Main() -> i32' function with one linear block and no locals.");
            return null;
        }

        var Data = module.Data
            .OfType<Textˉdataˉdeclaration>()
            .ToDictionary(Declaration => Declaration.Name, Declaration => Declaration.Value, StringComparer.Ordinal);
        var Textˉtemporaries = new Dictionary<int, string>();
        var Integerˉtemporaries = new Dictionary<int, int>();
        var Output = new List<byte>();
        foreach (var Instruction in Function.Blocks[0].Instructions)
        {
            switch (Instruction.Operation)
            {
                case Wirˉoperation.Textˉconstant when
                    Instruction.Result is int Textˉtemporary &&
                    Instruction.Nameˉoperand is not null &&
                    Instruction.Operands.IsEmpty &&
                    Data.TryGetValue(Instruction.Nameˉoperand, out var Text):
                    Textˉtemporaries[Textˉtemporary] = Text;
                    break;
                case Wirˉoperation.I32ˉconstant when
                    Instruction.Result is int Integerˉtemporary &&
                    Instruction.Operands.IsEmpty:
                    Integerˉtemporaries[Integerˉtemporary] = Instruction.Integerˉoperand;
                    break;
                case Wirˉoperation.Callˉcapability when
                    Instruction.Result is null &&
                    StringComparer.Ordinal.Equals(
                        Instruction.Nameˉoperand,
                        Capabilityˉcatalog.CONSOLE_WRITE_LINE) &&
                    Instruction.Operands.Length == 1 &&
                    Textˉtemporaries.TryGetValue(Instruction.Operands[0], out var Line):
                    if (!Tryˉappendˉasciiˉline(Output, Line))
                    {
                        Report(
                            diagnostics,
                            sourceˉname,
                            "WVN1005",
                            $"console.write_line output must be ASCII and at most {X64ˉkernelˉcontract.MAXIMUM_OUTPUT_BYTES} bytes.");
                    }
                    break;
                default:
                    Report(
                        diagnostics,
                        sourceˉname,
                        "WVN1004",
                        $"The first kernel target does not support WIR operation '{Instruction.Operation}' in Main.");
                    break;
            }
        }

        if (Function.Blocks[0].Terminator is not Wirˉreturn { Value: int Returnˉtemporary } ||
            !Integerˉtemporaries.TryGetValue(Returnˉtemporary, out var Returnˉvalue))
        {
            Report(
                diagnostics,
                sourceˉname,
                "WVN1004",
                "The first kernel target requires Main to return an i32 constant.");
            return null;
        }

        return diagnostics.Count == 0
            ? new(Output.ToImmutableArray(), Returnˉvalue)
            : null;
    }

    private static bool Tryˉappendˉasciiˉline(List<byte> output, string value)
    {
        foreach (var Character in value)
        {
            if (Character > 0x7F || output.Count >= X64ˉkernelˉcontract.MAXIMUM_OUTPUT_BYTES)
            {
                return false;
            }
            output.Add((byte)Character);
        }

        if (output.Count >= X64ˉkernelˉcontract.MAXIMUM_OUTPUT_BYTES)
        {
            return false;
        }
        output.Add((byte)'\n');
        return true;
    }

    private static Objectˉfile Emit(Nativeˉplan plan)
    {
        var Output = new Nativeˉcodeˉbuilder();
        Emitˉhandoffˉvalidation(Output);
        Output.Emit(0x48, 0x83, 0xEC, 0x28);
        Output.Emitˉexternalˉcall(2);
        Output.Emit(0x48, 0x83, 0xC4, 0x28, 0xC3);

        Output.Mark(FAILURE_LABEL);
        Output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3);
        Output.Align(16);
        var Mainˉoffset = Output.Position;
        Output.Emit(0x48, 0x83, 0xEC, 0x28);
        foreach (var Value in plan.Output)
        {
            Output.Emit(0xB9);
            Output.Emitˉu32(Value);
            Output.Emitˉexternalˉcall(3);
        }
        Output.Emit(0x48, 0x83, 0xC4, 0x28);
        Output.Emit(0xB8);
        Output.Emitˉu32(unchecked((uint)plan.Returnˉvalue));
        Output.Emit(0xC3);

        var Code = Output.Build();
        return new(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Bytes.Length, Code.Bytes)],
            [
                new(
                    X64ˉkernelˉcontract.KERNEL_ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    Mainˉoffset),
                new(
                    X64ˉkernelˉcontract.KERNEL_MAIN_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    Mainˉoffset,
                    checked((uint)Code.Bytes.Length - Mainˉoffset)),
                new(
                    X64ˉkernelˉcontract.MEMORY_ENTER_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    X64ˉkernelˉcontract.WRITE_BYTE_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            [.. Code.Relocations.Select(Relocation => new Objectˉrelocation(
                Objectˉrelocationˉkind.Relativeˉi32,
                0,
                Relocation.Offset,
                Relocation.Symbolˉindex,
                -4))]);
    }

    private static void Emitˉhandoffˉvalidation(Nativeˉcodeˉbuilder output)
    {
        output.Emit(0x48, 0x85, 0xC9);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(X64ˉkernelˉcontract.HANDOFF_MAGIC);
        output.Emit(0x48, 0x39, 0x01);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x83, 0x79, 0x08, (byte)X64ˉkernelˉcontract.HANDOFF_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x83, 0x79, 0x0C, (byte)X64ˉkernelˉcontract.HANDOFF_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x83, 0x79, 0x10, 0x00);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x8B, 0x41, 0x18);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x3D);
        output.Emitˉu32(X64ˉkernelˉcontract.MAX_MEMORY_MAP_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x48, 0x8B, 0x41, 0x20);
        output.Emit(0x48, 0x83, 0xF8, (byte)X64ˉkernelˉcontract.MINIMUM_DESCRIPTOR_BYTES);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        output.Emit(0x48, 0x3D);
        output.Emitˉu32(X64ˉkernelˉcontract.MAXIMUM_DESCRIPTOR_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x83, 0x79, 0x28, (byte)X64ˉkernelˉcontract.DESCRIPTOR_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x83, 0x79, 0x2C, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x8B, 0x41, 0x18);
        output.Emit(0x31, 0xD2);
        output.Emit(0x48, 0xF7, 0x71, 0x20);
        output.Emit(0x48, 0x85, 0xD2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
    }

    private static void Report(
        Diagnosticˉbag diagnostics,
        string sourceˉname,
        string code,
        string message)
    {
        diagnostics.Report(code, PHASE, new(0, 0, 1, 1, sourceˉname), message);
    }

    private sealed record Nativeˉplan(ImmutableArray<byte> Output, int Returnˉvalue);

    private sealed class Nativeˉcodeˉbuilder
    {
        private readonly List<byte> Output = [];
        private readonly Dictionary<string, int> Labels = new(StringComparer.Ordinal);
        private readonly List<Relativeˉfixup> Fixups = [];
        private readonly List<Nativeˉrelocation> Relocations = [];

        public uint Position => checked((uint)Output.Count);

        public void Emit(params byte[] bytes) => Output.AddRange(bytes);

        public void Emitˉu32(uint value)
        {
            Span<byte> Bytes = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Bytes, value);
            Output.AddRange(Bytes);
        }

        public void Emitˉu64(ulong value)
        {
            Span<byte> Bytes = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64LittleEndian(Bytes, value);
            Output.AddRange(Bytes);
        }

        public void Emitˉexternalˉcall(uint symbolˉindex)
        {
            Output.Add(0xE8);
            Relocations.Add(new(checked((uint)Output.Count), symbolˉindex));
            Output.AddRange([0, 0, 0, 0]);
        }

        public void Align(int alignment)
        {
            while (Output.Count % alignment != 0)
            {
                Output.Add(0x90);
            }
        }

        public void Mark(string label)
        {
            if (!Labels.TryAdd(label, Output.Count))
            {
                throw new InvalidOperationException($"Duplicate x86-64 kernel label '{label}'.");
            }
        }

        public void Jumpˉif(byte conditionˉopcode, string label)
        {
            Output.Add(0x0F);
            Output.Add(conditionˉopcode);
            var Offset = Output.Count;
            Output.AddRange([0, 0, 0, 0]);
            Fixups.Add(new(Offset, label));
        }

        public Nativeˉcode Build()
        {
            var Bytes = Output.ToArray();
            foreach (var Fixup in Fixups)
            {
                if (!Labels.TryGetValue(Fixup.Label, out var Target))
                {
                    throw new InvalidOperationException($"Undefined x86-64 kernel label '{Fixup.Label}'.");
                }

                var Displacement = checked(Target - (Fixup.Displacementˉoffset + sizeof(int)));
                BinaryPrimitives.WriteInt32LittleEndian(
                    Bytes.AsSpan(Fixup.Displacementˉoffset, sizeof(int)),
                    Displacement);
            }
            return new(Bytes.ToImmutableArray(), Relocations.ToImmutableArray());
        }

        private sealed record Relativeˉfixup(int Displacementˉoffset, string Label);
    }

    private sealed record Nativeˉcode(
        ImmutableArray<byte> Bytes,
        ImmutableArray<Nativeˉrelocation> Relocations);

    private sealed record Nativeˉrelocation(uint Offset, uint Symbolˉindex);
}
