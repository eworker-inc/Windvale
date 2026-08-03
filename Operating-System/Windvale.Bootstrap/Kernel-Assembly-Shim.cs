using System.Collections.Immutable;
using System.Text;
using Windvale.Assembler;
using Windvale.Compiler;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉassemblyˉcontract
{
    public const int FORMAT_VERSION = 9;
    public const string TARGET_NAME = "x86-64-kernel-wva-seam-v9";
    public const string MAIN_SHIM_SYMBOL = "Windvale_kernel_wva_main";
    public const string Q35_SHUTDOWN_SYMBOL = "Windvale_kernel_x64_q35_shutdown";
    public const string X64_WRITE_BYTE_SYMBOL = "Windvale_kernel_x64_write_byte";
}

public static class Kernelˉassemblyˉshim
{
    private const string RESOURCE_NAME = "Windvale.Os.Kernel.X64-Kernel-Shims.wva";

    public static ImmutableArray<byte> Buildˉobject()
    {
        var Assembly = Assemblyˉcompiler.Assemble(Loadˉsource());
        if (!Assembly.Success)
        {
            var Diagnostic = Assembly.Diagnostics[0];
            throw new InvalidOperationException(
                $"The kernel WVA shim did not assemble: {Diagnostic.Code}: {Diagnostic.Message}");
        }

        var Object = Objectˉcodec.Readˉandˉverify(Assembly.Objectˉbytes.AsSpan()).Value;
        var Expectedˉmarkers = Encoding.ASCII.GetBytes(
            Kernelˉexceptionˉcontract.INVALID_OPCODE_PANIC_MARKER +
            Kernelˉexceptionˉcontract.GENERAL_PROTECTION_PANIC_MARKER +
            Kernelˉexceptionˉcontract.MALFORMED_FRAME_PANIC_MARKER);
        ImmutableArray<Objectˉsymbol> Expectedˉsymbols =
        [
            new("Windvale_kernel_x64_exception_serial_write", Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Function, 0, 285, 48),
            new("Windvale_kernel_x64_general_protection_marker", Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Data, 1, 56, 61),
            new("Windvale_kernel_x64_invalid_opcode_marker", Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Data, 1, 0, 56),
            new("Windvale_kernel_x64_malformed_frame_marker", Objectˉsymbolˉbinding.Local,
                Objectˉsymbolˉkind.Data, 1, 117, 45),
            new(X64ˉkernelˉcontract.WRITE_BYTE_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 0, 5),
            new(Kernelˉassemblyˉcontract.MAIN_SHIM_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 5, 5),
            new(Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 10, 10),
            new(Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 20, 15),
            new(Kernelˉexceptionˉcontract.TERMINAL_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 121, 164),
            new(Kernelˉpagingˉcontract.PROTECTION_ENABLE_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 35, 25),
            new(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 60, 7),
            new(Kernelˉprocessˉcontract.EXCEPTION_13_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 67, 10),
            new(Kernelˉprocessˉcontract.EXCEPTION_14_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 77, 10),
            new(Kernelˉprocessˉcontract.EXCEPTION_6_ENTRY_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 87, 15),
            new(Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 102, 19),
            Import(Kernelˉprocessˉcontract.EXCEPTION_ENTRY_SYMBOL),
            Import(Kernelˉassemblyˉcontract.X64_WRITE_BYTE_SYMBOL),
            Import(Kernelˉwvbˉadmissionˉcontract.BRIDGE_SYMBOL),
        ];
        ImmutableArray<Objectˉrelocation> Expectedˉrelocations =
        [
            Relative(1, 16), Relative(6, 17), Relative(16, 8), Relative(31, 8),
            Relative(73, 15), Relative(83, 15), Relative(98, 15), Relative(117, 14),
            Relative(166, 2), Relative(176, 0), Relative(230, 1), Relative(240, 0),
            Relative(252, 3), Relative(262, 0),
        ];
        ReadOnlySpan<byte> Serialˉwriter =
        [
            0xBA, 0xFD, 0x03, 0, 0, 0xEC, 0xF6, 0xC0, 0x20,
            0x0F, 0x84, 0xF6, 0xFF, 0xFF, 0xFF,
            0xBA, 0xF8, 0x03, 0, 0, 0x8A, 0x84, 0x26, 0, 0, 0, 0, 0xEE,
            0x48, 0x81, 0xC6, 1, 0, 0, 0, 0x81, 0xE9, 1, 0, 0, 0,
            0x0F, 0x85, 0xD1, 0xFF, 0xFF, 0xFF, 0xC3,
        ];
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 2 ||
            Object.Sections[0] is not
            {
                Name: ".text",
                Kind: Objectˉsectionˉkind.Code,
                Alignment: 16,
                Memoryˉsize: 333,
            } ||
            Object.Sections[0].Data.Length != 333 ||
            Object.Sections[1] is not
            {
                Name: ".rodata",
                Kind: Objectˉsectionˉkind.Readˉonlyˉdata,
                Alignment: 1,
                Memoryˉsize: 162,
            } ||
            !Object.Sections[1].Data.AsSpan().SequenceEqual(Expectedˉmarkers) ||
            !Object.Sections[0].Data.AsSpan(285, 48).SequenceEqual(Serialˉwriter) ||
            !Object.Symbols.AsSpan().SequenceEqual(Expectedˉsymbols.AsSpan()) ||
            !Object.Relocations.AsSpan().SequenceEqual(Expectedˉrelocations.AsSpan()))
        {
            throw new InvalidOperationException(
                $"The kernel WVA shim violated '{Kernelˉassemblyˉcontract.TARGET_NAME}'.");
        }

        return Assembly.Objectˉbytes;
    }

    private static Objectˉsymbol Import(string name) =>
        new(name, Objectˉsymbolˉbinding.Import, Objectˉsymbolˉkind.Function,
            Objectˉlimits.UNDEFINED_SECTION, 0, 0);

    private static Objectˉrelocation Relative(uint offset, uint symbolˉindex) =>
        new(Objectˉrelocationˉkind.Relativeˉi32, 0, offset, symbolˉindex, -4);

    private static string Loadˉsource()
    {
        using var Stream = typeof(Kernelˉassemblyˉshim).Assembly.GetManifestResourceStream(RESOURCE_NAME) ??
            throw new InvalidOperationException($"Embedded Windvale assembly '{RESOURCE_NAME}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
