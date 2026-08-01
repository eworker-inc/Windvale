using System.Collections.Immutable;
using System.Text;
using Windvale.Assembler;
using Windvale.Compiler;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉassemblyˉcontract
{
    public const int FORMAT_VERSION = 4;
    public const string TARGET_NAME = "x86-64-kernel-wva-seam-v4";
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
                    0xBA, 0x04, 0x06, 0x00, 0x00,
                    0xB8, 0x00, 0x20, 0x00, 0x00,
                    0x66, 0xEF,
                    0xFA,
                    0xF4,
                    0xE9, 0, 0, 0, 0,
                }) ||
            Object.Symbols.Length != 5 ||
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
                Name: Kernelˉassemblyˉcontract.Q35_SHUTDOWN_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 10,
                Size: 19,
            } ||
            Object.Symbols[3] is not
            {
                Name: Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Symbols[4] is not
            {
                Name: Kernelˉassemblyˉcontract.X64_WRITE_BYTE_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Relocations.Length != 3 ||
            Object.Relocations[0] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 1,
                Symbolˉindex: 4,
                Addend: -4,
            } ||
            Object.Relocations[1] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 6,
                Symbolˉindex: 3,
                Addend: -4,
            } ||
            Object.Relocations[2] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 25,
                Symbolˉindex: 2,
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
