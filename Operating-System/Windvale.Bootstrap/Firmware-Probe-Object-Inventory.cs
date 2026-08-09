using System.Collections.Immutable;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public sealed record Firmwareˉprobeˉobject(
    string Fileˉname,
    ImmutableArray<byte> Bytes);

public sealed record Firmwareˉprobeˉobjectˉinventory(
    string Entryˉsymbol,
    ImmutableArray<Firmwareˉprobeˉobject> Objects);

public static partial class Firmwareˉprobe
{
    private static ImmutableArray<byte> Packˉnativeˉbridgeˉandˉsupport(
        ImmutableArray<byte> nativeˉbridgeˉbytes,
        ImmutableArray<byte> supportˉbytes)
    {
        var Nativeˉbridge = Objectˉcodec.Readˉandˉverify(nativeˉbridgeˉbytes.AsSpan()).Value;
        var Support = Objectˉcodec.Readˉandˉverify(supportˉbytes.AsSpan()).Value;
        if (Nativeˉbridge.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Support.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Nativeˉbridge.Sections.Length != 1 ||
            Support.Sections.Length != 1 ||
            Nativeˉbridge.Symbols.Length != 3 ||
            Support.Symbols.Length != 1 ||
            Nativeˉbridge.Relocations.Length != 2 ||
            !Support.Relocations.IsEmpty ||
            Nativeˉbridge.Symbols[0] is not
                {
                    Name: Kernelˉnativeˉprobeˉcontract.BRIDGE_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Export,
                    Sectionˉindex: 0,
                } ||
            Nativeˉbridge.Symbols[1] is not
                {
                    Name: Kernelˉnativeˉprobeˉcontract.NATIVE_MAIN_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Import,
                } ||
            Nativeˉbridge.Symbols[2] is not
                {
                    Name: KERNEL_ENTRY_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Import,
                } ||
            Support.Symbols[0] is not
                {
                    Name: X64_WRITE_BYTE_SYMBOL,
                    Binding: Objectˉsymbolˉbinding.Export,
                    Sectionˉindex: 0,
                } ||
            Nativeˉbridge.Relocations[0].Symbolˉindex != 1 ||
            Nativeˉbridge.Relocations[1].Symbolˉindex != 2)
        {
            throw new InvalidOperationException(
                "The Probe 40 native bridge/support packing contract changed.");
        }

        var Packed = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [
                Nativeˉbridge.Sections[0] with { Name = ".text.native" },
                Support.Sections[0] with { Name = ".text.support" },
            ],
            [
                Nativeˉbridge.Symbols[0],
                Support.Symbols[0] with { Sectionˉindex = 1 },
                Nativeˉbridge.Symbols[1],
                Nativeˉbridge.Symbols[2],
            ],
            [.. Nativeˉbridge.Relocations.Select(
                Relocation => Relocation with { Symbolˉindex = Relocation.Symbolˉindex + 1 })]);
        return Objectˉcodec.Write(Packed).ToImmutableArray();
    }

    public static Linkˉresult Buildˉlinkedˉimage(
        Firmwareˉprobeˉscenario scenario = Firmwareˉprobeˉscenario.Normal)
    {
        var Inventory = Buildˉobjectˉinventory(scenario);
        var Link = Linkˉcompiler.Link(
            [.. Inventory.Objects.Select(Object => new Linkˉinput(Object.Bytes))],
            new(
                Uefiˉapplicationˉcontract.REQUIRED_LINK_BASE_ADDRESS,
                Inventory.Entryˉsymbol));
        if (!Link.Success)
        {
            throw new InvalidOperationException(
                $"The firmware probe did not link: {Link.Diagnostics[0].Code}: " +
                Link.Diagnostics[0].Message);
        }
        if (Link.Entryˉaddress != 0 ||
            Link.Imageˉbytes.Length > (int)Kernelˉpagingˉcontract.EXECUTABLE_BYTES)
        {
            throw new InvalidOperationException(
                $"The linked firmware payload is {Link.Imageˉbytes.Length} bytes and does not fit the fixed " +
                $"{Kernelˉpagingˉcontract.EXECUTABLE_BYTES}-byte executable window.");
        }

        return Link;
    }
}
