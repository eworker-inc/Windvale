using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Windvale.Assembler;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉmemoryˉassemblyˉshim
{
    private const string RESOURCE_NAME = "Windvale.Os.Kernel.X64-Memory-Object-Shims.wva";
    private const string OBJECT_SHA256 =
        "FE0A94461B743BE58319D2E2F8B737840EC1216E61A98EE7E210F96F97F85BEE";

    public static ImmutableArray<byte> Buildˉobject()
    {
        var Assembly = Assemblyˉcompiler.Assemble(Loadˉsource());
        if (!Assembly.Success)
        {
            var Diagnostic = Assembly.Diagnostics[0];
            throw new InvalidOperationException(
                $"The kernel memory-object WVA shim did not assemble: " +
                $"{Diagnostic.Code}: {Diagnostic.Message}");
        }

        var Object = Objectˉcodec.Readˉandˉverify(Assembly.Objectˉbytes.AsSpan()).Value;
        ImmutableArray<Objectˉsymbol> Expectedˉsymbols =
        [
            Export(Kernelˉmemoryˉcontract.ALLOCATE_MEMORY_OBJECT_SYMBOL, 0, 1_389),
            Export(Kernelˉmemoryˉcontract.RELEASE_MEMORY_OBJECT_SYMBOL, 1_389, 985),
        ];
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Sections[0] is not
            {
                Name: ".text",
                Kind: Objectˉsectionˉkind.Code,
                Alignment: 16,
                Memoryˉsize: 2_374,
                Data.Length: 2_374,
            } ||
            !Object.Symbols.AsSpan().SequenceEqual(Expectedˉsymbols.AsSpan()) ||
            !Object.Relocations.IsEmpty ||
            !Convert.ToHexString(SHA256.HashData(Assembly.Objectˉbytes.AsSpan())).Equals(
                OBJECT_SHA256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The kernel memory-object WVA shim violated its exact first-fit contract.");
        }
        return Assembly.Objectˉbytes;
    }

    private static Objectˉsymbol Export(string name, uint offset, uint size) =>
        new(name, Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, offset, size);

    private static string Loadˉsource()
    {
        using var Stream = typeof(Kernelˉmemoryˉassemblyˉshim).Assembly
            .GetManifestResourceStream(RESOURCE_NAME) ??
            throw new InvalidOperationException(
                $"Embedded Windvale assembly '{RESOURCE_NAME}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
