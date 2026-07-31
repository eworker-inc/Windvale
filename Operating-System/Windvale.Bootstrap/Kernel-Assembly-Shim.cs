using System.Collections.Immutable;
using System.Text;
using Windvale.Assembler;
using Windvale.Compiler;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉassemblyˉcontract
{
    public const int FORMAT_VERSION = 1;
    public const string TARGET_NAME = "x86-64-kernel-wva-seam-v1";
    public const string MAIN_SHIM_SYMBOL = "Windvale_kernel_wva_main";
}

public static class Kernelˉassemblyˉshim
{
    private const string RESOURCE_NAME = "Windvale.Os.Kernel.X64-Main-Shim.wva";

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
            !Object.Sections[0].Data.AsSpan().SequenceEqual(new byte[] { 0xE9, 0, 0, 0, 0 }) ||
            Object.Symbols.Length != 2 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉassemblyˉcontract.MAIN_SHIM_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
                Size: 5,
            } ||
            Object.Symbols[1] is not
            {
                Name: X64ˉkernelˉcontract.KERNEL_MAIN_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Relocations.Length != 1 ||
            Object.Relocations[0] is not
            {
                Kind: Objectˉrelocationˉkind.Relativeˉi32,
                Sectionˉindex: 0,
                Offset: 1,
                Symbolˉindex: 1,
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
