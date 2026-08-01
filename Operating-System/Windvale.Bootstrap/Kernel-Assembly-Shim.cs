using System.Collections.Immutable;
using System.Text;
using Windvale.Assembler;
using Windvale.Compiler;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉassemblyˉcontract
{
    public const int FORMAT_VERSION = 6;
    public const string TARGET_NAME = "x86-64-kernel-wva-seam-v6";
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
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Sections[0].Kind != Objectˉsectionˉkind.Code ||
            Object.Sections[0].Alignment != 16 ||
            !Object.Sections[0].Data.AsSpan().SequenceEqual(
                new byte[] {
                    0xE9, 0, 0, 0, 0,
                    0xE9, 0, 0, 0, 0,
                    0x68, 0x0D, 0x00, 0x00, 0x00,
                    0xE9, 0, 0, 0, 0,
                    0x68, 0x00, 0x00, 0x00, 0x00,
                    0x68, 0x06, 0x00, 0x00, 0x00,
                    0xE9, 0, 0, 0, 0,
                    0xB9, 0x80, 0x00, 0x00, 0xC0,
                    0x0F, 0x32, 0x0F, 0xBA, 0xE8, 0x0B, 0x0F, 0x30,
                    0x0F, 0x20, 0xC0, 0x48, 0x0F, 0xBA, 0xE8, 0x10,
                    0x0F, 0x22, 0xC0, 0xC3,
                    0x0F, 0x22, 0xD8, 0x0F, 0x20, 0xD8, 0xC3,
                    0xBA, 0x04, 0x06, 0x00, 0x00,
                    0xB8, 0x00, 0x20, 0x00, 0x00,
                    0x66, 0xEF,
                    0xFA,
                    0xF4,
                    0xE9, 0, 0, 0, 0,
                }) ||
            Object.Symbols.Length != 10 ||
            Object.Symbols[0] is not
            {
                Name: X64ˉkernelˉcontract.WRITE_BYTE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
                Size: 5,
            } ||
            Object.Symbols[1] is not
            {
                Name: Kernelˉassemblyˉcontract.MAIN_SHIM_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 5,
                Size: 5,
            } ||
            Object.Symbols[2] is not
            {
                Name: Kernelˉexceptionˉcontract.GENERAL_PROTECTION_ENTRY_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 10,
                Size: 10,
            } ||
            Object.Symbols[3] is not
            {
                Name: Kernelˉexceptionˉcontract.INVALID_OPCODE_ENTRY_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 20,
                Size: 15,
            } ||
            Object.Symbols[4] is not
            {
                Name: Kernelˉpagingˉcontract.PROTECTION_ENABLE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 35,
                Size: 25,
            } ||
            Object.Symbols[5] is not
            {
                Name: Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 60,
                Size: 7,
            } ||
            Object.Symbols[6] is not
            {
                Name: Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 67,
                Size: 19,
            } ||
            Object.Symbols[7] is not
            {
                Name: Kernelˉexceptionˉcontract.TERMINAL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Symbols[8] is not
            {
                Name: Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Symbols[9] is not
            {
                Name: Kernelˉassemblyˉcontract.X64_WRITE_BYTE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Relocations.Length != 5 ||
            Object.Relocations[0] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 1,
                Symbolˉindex: 9,
                Addend: -4,
            } ||
            Object.Relocations[1] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 6,
                Symbolˉindex: 8,
                Addend: -4,
            } ||
            Object.Relocations[2] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 16,
                Symbolˉindex: 7,
                Addend: -4,
            } ||
            Object.Relocations[3] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 31,
                Symbolˉindex: 7,
                Addend: -4,
            } ||
            Object.Relocations[4] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 82,
                Symbolˉindex: 6,
                Addend: -4,
            })
        {
            throw new InvalidOperationException(
                $"The kernel WVA shim violated '{Kernelˉassemblyˉcontract.TARGET_NAME}'.");
        }

        return Assembly.Objectˉbytes;
    }

    private static string Loadˉsource()
    {
        using var Stream = typeof(Kernelˉassemblyˉshim).Assembly.GetManifestResourceStream(RESOURCE_NAME) ??
            throw new InvalidOperationException($"Embedded Windvale assembly '{RESOURCE_NAME}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
