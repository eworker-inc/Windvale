using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Windvale.Assembler;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public static class Kernelˉtimerˉassemblyˉshim
{
    private const string RESOURCE_NAME = "Windvale.Os.Kernel.X64-Timer-Shims.wva";
    private const string OBJECT_SHA256 =
        "E331A1DB404B8B8359D35D410792496683A63ACEE621FF64F128A6EAE128C344";

    public static ImmutableArray<byte> Buildˉobject()
    {
        var Assembly = Assemblyˉcompiler.Assemble(Loadˉsource());
        if (!Assembly.Success)
        {
            var Diagnostic = Assembly.Diagnostics[0];
            throw new InvalidOperationException(
                $"The kernel timer WVA shim did not assemble: {Diagnostic.Code}: {Diagnostic.Message}");
        }

        var Object = Objectˉcodec.Readˉandˉverify(Assembly.Objectˉbytes.AsSpan()).Value;
        ImmutableArray<Objectˉsymbol> Expectedˉsymbols =
        [
            Export(Kernelˉtimerˉcontract.ARM_SYMBOL, 0, 518),
            Export(Kernelˉtimerˉcontract.IRQ_ENTRY_SYMBOL, 518, 60),
            Export(Kernelˉtimerˉcontract.READ_CLOCK_SYMBOL, 578, 66),
            Export(Kernelˉtimerˉcontract.REARM_SYMBOL, 644, 36),
            Export(Kernelˉtimerˉcontract.RESUME_SYMBOL, 680, 28),
            Export(Kernelˉtimerˉcontract.STOP_SYMBOL, 708, 56),
            new(Kernelˉtimerˉcontract.INTERRUPT_SYMBOL, Objectˉsymbolˉbinding.Import,
                Objectˉsymbolˉkind.Function, Objectˉlimits.UNDEFINED_SECTION, 0, 0),
        ];
        var Expectedˉrelocation = new Objectˉrelocation(
            Objectˉrelocationˉkind.Relativeˉi32, 0, 574, 6, -4);
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Sections[0] is not
            {
                Name: ".text",
                Kind: Objectˉsectionˉkind.Code,
                Alignment: 16,
                Memoryˉsize: 764,
                Data.Length: 764,
            } ||
            !Object.Symbols.AsSpan().SequenceEqual(Expectedˉsymbols.AsSpan()) ||
            Object.Relocations.Length != 1 ||
            Object.Relocations[0] != Expectedˉrelocation ||
            !Convert.ToHexString(SHA256.HashData(Assembly.Objectˉbytes.AsSpan())).Equals(
                OBJECT_SHA256, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The kernel timer WVA shim violated its exact HPET/local-APIC contract.");
        }
        return Assembly.Objectˉbytes;
    }

    private static Objectˉsymbol Export(string name, uint offset, uint size) =>
        new(name, Objectˉsymbolˉbinding.Export, Objectˉsymbolˉkind.Function, 0, offset, size);

    private static string Loadˉsource()
    {
        using var Stream = typeof(Kernelˉtimerˉassemblyˉshim).Assembly
            .GetManifestResourceStream(RESOURCE_NAME) ??
            throw new InvalidOperationException($"Embedded Windvale assembly '{RESOURCE_NAME}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
