using System.Collections.Immutable;
using System.Text;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉexceptionˉcontract
{
    public const int FORMAT_VERSION = 2;
    public const string TARGET_NAME = "x86-64-kernel-exceptions-v2";
    public const string INSTALL_SYMBOL = "Windvale_kernel_x64_exception_install";
    public const string TERMINAL_SYMBOL = "Windvale_kernel_x64_exception_terminal";
    public const string GENERAL_PROTECTION_ENTRY_SYMBOL = "Windvale_kernel_x64_exception_13_entry";
    public const string INVALID_OPCODE_ENTRY_SYMBOL = "Windvale_kernel_x64_exception_6_entry";
    public const uint INVALID_OPCODE_VECTOR = 6;
    public const uint GENERAL_PROTECTION_VECTOR = 13;
    public const uint IDT_PAGE_BYTES = 4_096;
    public const uint IDT_GATE_BYTES = 16;
    public const uint INVALID_OPCODE_GATE_OFFSET = INVALID_OPCODE_VECTOR * IDT_GATE_BYTES;
    public const uint GENERAL_PROTECTION_GATE_OFFSET = GENERAL_PROTECTION_VECTOR * IDT_GATE_BYTES;
    public const uint IDT_DESCRIPTOR_OFFSET = GENERAL_PROTECTION_GATE_OFFSET + IDT_GATE_BYTES;
    public const ushort IDT_LIMIT = (ushort)(GENERAL_PROTECTION_GATE_OFFSET + IDT_GATE_BYTES - 1);
    public const byte INTERRUPT_GATE_ATTRIBUTES = 0x8E;
    public const uint NORMALIZED_VECTOR_OFFSET = 0;
    public const uint NORMALIZED_ERROR_CODE_OFFSET = 8;
    public const uint NORMALIZED_INSTRUCTION_POINTER_OFFSET = 16;
    public const uint NORMALIZED_CODE_SELECTOR_OFFSET = 24;
    public const uint NORMALIZED_FLAGS_OFFSET = 32;
    public const uint NORMALIZED_FRAME_BYTES = 40;
    public const string INVALID_OPCODE_PANIC_MARKER =
        "panic=invalid-opcode\nvector=6\nerror-code=0\nstatus=panic\n";
    public const string GENERAL_PROTECTION_PANIC_MARKER =
        "panic=general-protection\nvector=13\nerror-code=0\nstatus=panic\n";
    public const string MALFORMED_FRAME_PANIC_MARKER =
        "panic=malformed-exception-frame\nstatus=panic\n";
}

public sealed record Kernelˉexceptionˉartifacts(
    ImmutableArray<byte> Objectˉbytes,
    ImmutableArray<byte> Codeˉbytes,
    uint Installerˉbytes,
    uint Terminalˉoffset);

public static class Kernelˉexceptionˉx64
{
    private const string INSTALL_FAILURE_LABEL = "exception_install_failure";
    private const string CHECK_GENERAL_PROTECTION_LABEL = "exception_check_general_protection";
    private const string MALFORMED_FRAME_LABEL = "exception_malformed_frame";
    private const string TERMINATE_LABEL = "exception_terminate";
    private const string PANIC_HALT_LABEL = "exception_panic_halt";
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;

    public static Kernelˉexceptionˉartifacts Build()
    {
        var Output = new X64ˉcodeˉbuilder();
        var Relocations = ImmutableArray.CreateBuilder<Objectˉrelocation>();
        Emitˉinstaller(Output, Relocations);
        var Installerˉbytes = Output.Position;
        Output.Align(16);
        var Terminalˉoffset = Output.Position;
        Emitˉterminalˉhandler(Output);
        var Code = Output.Build();

        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [
                new(
                    Kernelˉexceptionˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    Installerˉbytes),
                new(
                    Kernelˉexceptionˉcontract.TERMINAL_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    Terminalˉoffset,
                    checked((uint)Code.Length - Terminalˉoffset)),
                new(
                    Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
                new(
                    Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL,
                    Objectˉsymbolˉbinding.Import,
                    Objectˉsymbolˉkind.Function,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0),
            ],
            Relocations.ToImmutable());
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        Verifyˉobject(Objectˉbytes, Code, Installerˉbytes, Terminalˉoffset, Relocations.ToImmutable());
        return new(Objectˉbytes, Code, Installerˉbytes, Terminalˉoffset);
    }

    private static void Emitˉinstaller(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        // RCX is the existing page-aligned, zeroing allocation owned by the kernel.
        output.Emit(0x57);
        output.Emit(0x48, 0x85, 0xC9);
        output.Jumpˉif(CONDITION_EQUAL, INSTALL_FAILURE_LABEL);
        output.Emit(0x48, 0xF7, 0xC1, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, INSTALL_FAILURE_LABEL);
        output.Emit(0x49, 0x89, 0xC8);

        // Publish no stale gate: clear the complete page before constructing either admitted vector.
        output.Emit(0x48, 0x89, 0xCF);
        output.Emit(0x31, 0xC0, 0xB9);
        output.Emitˉu32(Kernelˉexceptionˉcontract.IDT_PAGE_BYTES / sizeof(ulong));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB);

        // Derive the live firmware code selector instead of encoding an OVMF-specific value.
        output.Emit(0x31, 0xC0, 0x8C, 0xC8);
        output.Emit(0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, INSTALL_FAILURE_LABEL);
        output.Emit(0xA8, 0x03);
        output.Jumpˉif(CONDITION_NOT_EQUAL, INSTALL_FAILURE_LABEL);
        Emitˉentryˉaddress(Output: output, Relocations: relocations, Symbolˉindex: 3);
        Emitˉgate(output, Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET);
        Emitˉentryˉaddress(Output: output, Relocations: relocations, Symbolˉindex: 2);
        Emitˉgate(output, Kernelˉexceptionˉcontract.GENERAL_PROTECTION_GATE_OFFSET);

        // The ten-byte IDTR operand follows the admitted table and is outside its limit.
        output.Emit(0x66, 0x41, 0xC7, 0x80);
        output.Emitˉu32(Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET);
        output.Emit(
            (byte)Kernelˉexceptionˉcontract.IDT_LIMIT,
            (byte)(Kernelˉexceptionˉcontract.IDT_LIMIT >> 8));
        output.Emit(0x4D, 0x89, 0x80);
        output.Emitˉu32(Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET + 2);
        output.Emit(0xFA);
        output.Emit(0x41, 0x0F, 0x01, 0x98);
        output.Emitˉu32(Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET);
        output.Emit(0x31, 0xC0, 0x5F, 0xC3);

        output.Mark(INSTALL_FAILURE_LABEL);
        output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00, 0x5F, 0xC3);
    }

    private static void Emitˉentryˉaddress(
        X64ˉcodeˉbuilder Output,
        ImmutableArray<Objectˉrelocation>.Builder Relocations,
        uint Symbolˉindex)
    {
        Output.Emit(0x48, 0x8D, 0x15);
        var Fieldˉoffset = Output.Position;
        Output.Emitˉu32(0);
        Relocations.Add(new(
            Objectˉrelocationˉkind.Relativeˉi32,
            0,
            Fieldˉoffset,
            Symbolˉindex,
            -4));
    }

    private static void Emitˉgate(X64ˉcodeˉbuilder output, uint offset)
    {
        Emitˉgateˉstore(output, [0x66, 0x41, 0x89], 0x50, 0x90, offset);
        Emitˉgateˉstore(output, [0x66, 0x41, 0x89], 0x40, 0x80, offset + 2);
        Emitˉgateˉstoreˉu8(output, offset + 4, 0);
        Emitˉgateˉstoreˉu8(output, offset + 5, Kernelˉexceptionˉcontract.INTERRUPT_GATE_ATTRIBUTES);
        output.Emit(0x48, 0xC1, 0xEA, 0x10);
        Emitˉgateˉstore(output, [0x66, 0x41, 0x89], 0x50, 0x90, offset + 6);
        output.Emit(0x48, 0xC1, 0xEA, 0x10);
        Emitˉgateˉstore(output, [0x41, 0x89], 0x50, 0x90, offset + 8);
        Emitˉgateˉstoreˉu32(output, offset + 12, 0);
    }

    private static void Emitˉgateˉstore(
        X64ˉcodeˉbuilder output,
        byte[] prefix,
        byte modrmˉdisp8,
        byte modrmˉdisp32,
        uint offset)
    {
        output.Emit(prefix);
        if (offset <= sbyte.MaxValue)
        {
            output.Emit(modrmˉdisp8, checked((byte)offset));
            return;
        }
        output.Emit(modrmˉdisp32);
        output.Emitˉu32(offset);
    }

    private static void Emitˉgateˉstoreˉu8(X64ˉcodeˉbuilder output, uint offset, byte value)
    {
        output.Emit(0x41, 0xC6);
        if (offset <= sbyte.MaxValue)
        {
            output.Emit(0x40, checked((byte)offset), value);
            return;
        }
        output.Emit(0x80);
        output.Emitˉu32(offset);
        output.Emit(value);
    }

    private static void Emitˉgateˉstoreˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0xC7);
        if (offset <= sbyte.MaxValue)
        {
            output.Emit(0x40, checked((byte)offset));
        }
        else
        {
            output.Emit(0x80);
            output.Emitˉu32(offset);
        }
        output.Emitˉu32(value);
    }

    private static void Emitˉterminalˉhandler(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x48, 0x83, 0x3C, 0x24, (byte)Kernelˉexceptionˉcontract.INVALID_OPCODE_VECTOR);
        output.Jumpˉif(CONDITION_NOT_EQUAL, CHECK_GENERAL_PROTECTION_LABEL);
        output.Emit(0x48, 0x83, 0x7C, 0x24, (byte)Kernelˉexceptionˉcontract.NORMALIZED_ERROR_CODE_OFFSET, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, MALFORMED_FRAME_LABEL);
        Emitˉserialˉmarker(output, Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER, "invalid_opcode");
        output.Jump(TERMINATE_LABEL);

        output.Mark(CHECK_GENERAL_PROTECTION_LABEL);
        output.Emit(0x48, 0x83, 0x3C, 0x24, (byte)Kernelˉexceptionˉcontract.GENERAL_PROTECTION_VECTOR);
        output.Jumpˉif(CONDITION_NOT_EQUAL, MALFORMED_FRAME_LABEL);
        output.Emit(0x48, 0x83, 0x7C, 0x24, (byte)Kernelˉexceptionˉcontract.NORMALIZED_ERROR_CODE_OFFSET, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, MALFORMED_FRAME_LABEL);
        Emitˉserialˉmarker(output, Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER, "general_protection");
        output.Jump(TERMINATE_LABEL);

        output.Mark(MALFORMED_FRAME_LABEL);
        Emitˉserialˉmarker(output, Kernelˉexceptionˉcontract.MALFORMED_FRAME_PANIC_MARKER, "malformed_frame");

        output.Mark(TERMINATE_LABEL);
        output.Emit(0xBA);
        output.Emitˉu32(0x00F4);
        output.Emit(0xB8);
        output.Emitˉu32(1);
        output.Emit(0xEF, 0xFA);
        output.Mark(PANIC_HALT_LABEL);
        output.Emit(0xF4);
        output.Jump(PANIC_HALT_LABEL);
    }

    private static void Emitˉserialˉmarker(X64ˉcodeˉbuilder output, string marker, string labelˉprefix)
    {
        var Index = 0;
        foreach (var Value in Encoding.ASCII.GetBytes(marker))
        {
            output.Emit(0xBA);
            output.Emitˉu32(0x03FD);
            var Waitˉlabel = labelˉprefix + "_write_wait_" +
                Index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            output.Mark(Waitˉlabel);
            output.Emit(0xEC, 0xA8, 0x20);
            output.Jumpˉif(CONDITION_EQUAL, Waitˉlabel);
            output.Emit(0xBA);
            output.Emitˉu32(0x03F8);
            output.Emit(0xB8);
            output.Emitˉu32(Value);
            output.Emit(0xEE);
            Index++;
        }
    }

    private static void Verifyˉobject(
        ImmutableArray<byte> objectˉbytes,
        ImmutableArray<byte> code,
        uint installerˉbytes,
        uint terminalˉoffset,
        ImmutableArray<Objectˉrelocation> relocations)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Sections[0] is not
            {
                Name: ".text",
                Kind: Objectˉsectionˉkind.Code,
                Alignment: 16,
            } ||
            Object.Sections[0].Memoryˉsize != (uint)code.Length ||
            !Object.Sections[0].Data.AsSpan().SequenceEqual(code.AsSpan()) ||
            Object.Symbols.Length != 4 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉexceptionˉcontract.INSTALL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
            } ||
            Object.Symbols[0].Size != installerˉbytes ||
            Object.Symbols[1] is not
            {
                Name: Kernelˉexceptionˉcontract.TERMINAL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
            } ||
            Object.Symbols[1].Offset != terminalˉoffset ||
            Object.Symbols[1].Size != checked((uint)code.Length - terminalˉoffset) ||
            Object.Symbols[2] is not
            {
                Name: Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Symbols[3] is not
            {
                Name: Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            !Object.Relocations.AsSpan().SequenceEqual(relocations.AsSpan()))
        {
            throw new InvalidOperationException(
                $"The kernel exception object violated '{Kernelˉexceptionˉcontract.TARGET_NAME}'.");
        }
    }
}
