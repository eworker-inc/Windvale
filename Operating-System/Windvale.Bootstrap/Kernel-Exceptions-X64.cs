using System.Collections.Immutable;
using System.Text;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉexceptionˉcontract
{
    public const int FORMAT_VERSION = 1;
    public const string TARGET_NAME = "x86-64-kernel-exceptions-v1";
    public const string INSTALL_SYMBOL = "Windvale_kernel_x64_exception_install";
    public const string INVALID_OPCODE_HANDLER_SYMBOL = "Windvale_kernel_x64_invalid_opcode";
    public const uint INVALID_OPCODE_VECTOR = 6;
    public const uint IDT_PAGE_BYTES = 4_096;
    public const uint IDT_GATE_BYTES = 16;
    public const uint INVALID_OPCODE_GATE_OFFSET = INVALID_OPCODE_VECTOR * IDT_GATE_BYTES;
    public const uint IDT_DESCRIPTOR_OFFSET = INVALID_OPCODE_GATE_OFFSET + IDT_GATE_BYTES;
    public const ushort IDT_LIMIT = (ushort)(INVALID_OPCODE_GATE_OFFSET + IDT_GATE_BYTES - 1);
    public const byte INTERRUPT_GATE_ATTRIBUTES = 0x8E;
    public const string INVALID_OPCODE_PANIC_MARKER =
        "panic=invalid-opcode\nvector=6\nerror-code=none\nstatus=panic\n";
}

public sealed record Kernelˉexceptionˉartifacts(
    ImmutableArray<byte> Objectˉbytes,
    ImmutableArray<byte> Codeˉbytes,
    uint Installerˉbytes,
    uint Handlerˉoffset);

public static class Kernelˉexceptionˉx64
{
    private const string HANDLER_LABEL = "invalid_opcode_handler";
    private const string INSTALL_FAILURE_LABEL = "exception_install_failure";
    private const string PANIC_HALT_LABEL = "invalid_opcode_panic_halt";
    private const string WRITE_BYTE_WAIT_LABEL_PREFIX = "invalid_opcode_write_wait_";
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;

    public static Kernelˉexceptionˉartifacts Build()
    {
        var Output = new X64ˉcodeˉbuilder();
        Emitˉinstaller(Output);
        var Installerˉbytes = Output.Position;
        Output.Align(16);
        var Handlerˉoffset = Output.Position;
        Output.Mark(HANDLER_LABEL);
        Emitˉinvalidˉopcodeˉhandler(Output);
        var Code = Output.Build();

        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [
                new(
                    Kernelˉexceptionˉcontract.INVALID_OPCODE_HANDLER_SYMBOL,
                    Objectˉsymbolˉbinding.Local,
                    Objectˉsymbolˉkind.Function,
                    0,
                    Handlerˉoffset,
                    checked((uint)Code.Length - Handlerˉoffset)),
                new(
                    Kernelˉexceptionˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    Installerˉbytes),
            ],
            []);
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        Verifyˉobject(Objectˉbytes, Code, Installerˉbytes, Handlerˉoffset);
        return new(Objectˉbytes, Code, Installerˉbytes, Handlerˉoffset);
    }

    private static void Emitˉinstaller(X64ˉcodeˉbuilder output)
    {
        // RCX is the existing page-aligned, zeroing allocation owned by the kernel.
        output.Emit(0x57);
        output.Emit(0x48, 0x85, 0xC9);
        output.Jumpˉif(CONDITION_EQUAL, INSTALL_FAILURE_LABEL);
        output.Emit(0x48, 0xF7, 0xC1, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, INSTALL_FAILURE_LABEL);
        output.Emit(0x49, 0x89, 0xC8);

        // Publish no stale gate: clear the complete page before constructing vector 6.
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
        output.Loadˉripˉrelativeˉrdx(HANDLER_LABEL);

        // Encode one present DPL-0 interrupt gate at vector 6.
        output.Emit(0x66, 0x41, 0x89, 0x50, (byte)Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET);
        output.Emit(0x66, 0x41, 0x89, 0x40, (byte)(Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET + 2));
        output.Emit(0x41, 0xC6, 0x40, (byte)(Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET + 4), 0x00);
        output.Emit(
            0x41,
            0xC6,
            0x40,
            (byte)(Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET + 5),
            Kernelˉexceptionˉcontract.INTERRUPT_GATE_ATTRIBUTES);
        output.Emit(0x48, 0xC1, 0xEA, 0x10);
        output.Emit(0x66, 0x41, 0x89, 0x50, (byte)(Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET + 6));
        output.Emit(0x48, 0xC1, 0xEA, 0x10);
        output.Emit(0x41, 0x89, 0x50, (byte)(Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET + 8));
        output.Emit(0x41, 0xC7, 0x40, (byte)(Kernelˉexceptionˉcontract.INVALID_OPCODE_GATE_OFFSET + 12));
        output.Emitˉu32(0);

        // The ten-byte IDTR operand follows the admitted table and is outside its limit.
        output.Emit(0x66, 0x41, 0xC7, 0x40, (byte)Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET);
        output.Emit(
            (byte)Kernelˉexceptionˉcontract.IDT_LIMIT,
            (byte)(Kernelˉexceptionˉcontract.IDT_LIMIT >> 8));
        output.Emit(0x4D, 0x89, 0x40, (byte)(Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET + 2));
        output.Emit(0xFA);
        output.Emit(0x41, 0x0F, 0x01, 0x58, (byte)Kernelˉexceptionˉcontract.IDT_DESCRIPTOR_OFFSET);
        output.Emit(0x31, 0xC0, 0x5F, 0xC3);

        output.Mark(INSTALL_FAILURE_LABEL);
        output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00, 0x5F, 0xC3);
    }

    private static void Emitˉinvalidˉopcodeˉhandler(X64ˉcodeˉbuilder output)
    {
        var Index = 0;
        foreach (var Value in Encoding.ASCII.GetBytes(Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER))
        {
            output.Emit(0xBA);
            output.Emitˉu32(0x03FD);
            output.Mark(WRITE_BYTE_WAIT_LABEL_PREFIX + Index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            output.Emit(0xEC, 0xA8, 0x20);
            output.Jumpˉif(CONDITION_EQUAL, WRITE_BYTE_WAIT_LABEL_PREFIX + Index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            output.Emit(0xBA);
            output.Emitˉu32(0x03F8);
            output.Emit(0xB8);
            output.Emitˉu32(Value);
            output.Emit(0xEE);
            Index++;
        }

        output.Emit(0xBA);
        output.Emitˉu32(0x00F4);
        output.Emit(0xB8);
        output.Emitˉu32(1);
        output.Emit(0xEF, 0xFA);
        output.Mark(PANIC_HALT_LABEL);
        output.Emit(0xF4);
        output.Jump(PANIC_HALT_LABEL);
    }

    private static void Verifyˉobject(
        ImmutableArray<byte> objectˉbytes,
        ImmutableArray<byte> code,
        uint installerˉbytes,
        uint handlerˉoffset)
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
            Object.Symbols.Length != 2 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉexceptionˉcontract.INVALID_OPCODE_HANDLER_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Local,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
            } ||
            Object.Symbols[0].Offset != handlerˉoffset ||
            Object.Symbols[0].Size != checked((uint)code.Length - handlerˉoffset) ||
            Object.Symbols[1] is not
            {
                Name: Kernelˉexceptionˉcontract.INSTALL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
            } ||
            Object.Symbols[1].Size != installerˉbytes ||
            !Object.Relocations.IsEmpty)
        {
            throw new InvalidOperationException(
                $"The kernel exception object violated '{Kernelˉexceptionˉcontract.TARGET_NAME}'.");
        }
    }
}
