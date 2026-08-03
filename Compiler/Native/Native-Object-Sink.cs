using System.Collections.Immutable;
using System.Text;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public sealed record Nativeˉobjectˉmeasurement(
    int Encodedˉobjectˉbytes,
    int Materializedˉsectionˉbytes,
    int Linkedˉimageˉbytes,
    int Textˉbytes,
    int Readˉonlyˉdataˉbytes,
    int Sections,
    int Symbols,
    int Relocations);

public static class Nativeˉobjectˉsink
{
    public static Nativeˉobjectˉmeasurement Measureˉwvo(Nativeˉfragment fragment)
    {
        var Projection = Projectˉwvo(fragment);
        long Encodedˉbytes = 24;
        long Materializedˉbytes = 0;
        // Native packaging links at base zero; a nonzero base can add leading alignment padding.
        ulong Linkedˉbytes = 0;
        foreach (var Section in Projection.Object.Sections)
        {
            Encodedˉbytes = checked(
                Encodedˉbytes + 20 + Encoding.UTF8.GetByteCount(Section.Name) + Section.Data.Length);
            Materializedˉbytes = checked(Materializedˉbytes + Section.Memoryˉsize);
            Linkedˉbytes = Alignˉup(Linkedˉbytes, Section.Alignment);
            Linkedˉbytes = checked(Linkedˉbytes + Section.Memoryˉsize);
        }
        foreach (var Symbol in Projection.Object.Symbols)
        {
            Encodedˉbytes = checked(
                Encodedˉbytes + 20 + Encoding.UTF8.GetByteCount(Symbol.Name));
        }
        Encodedˉbytes = checked(Encodedˉbytes + Projection.Object.Relocations.Length * 20L);
        return new(
            checked((int)Encodedˉbytes),
            checked((int)Materializedˉbytes),
            checked((int)Linkedˉbytes),
            Projection.Textˉbytes,
            Projection.Readˉonlyˉdataˉbytes,
            Projection.Object.Sections.Length,
            Projection.Object.Symbols.Length,
            Projection.Object.Relocations.Length);
    }

    public static ImmutableArray<byte> Writeˉwvo(
        Nativeˉfragment fragment,
        Objectˉadmissionˉprofile admissionˉprofile = Objectˉadmissionˉprofile.Standard)
    {
        var Projection = Projectˉwvo(fragment);
        var Bytes = Objectˉcodec.Write(Projection.Object, admissionˉprofile);
        _ = Objectˉcodec.Readˉandˉverify(Bytes, admissionˉprofile);
        return Bytes.ToImmutableArray();
    }

    private static Nativeˉobjectˉprojection Projectˉwvo(Nativeˉfragment fragment)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        var Symbolˉindices = fragment.Symbols
            .Select((Symbol, Index) => (Symbol.Name, Index))
            .ToDictionary(Entry => Entry.Name, Entry => Entry.Index, StringComparer.Ordinal);
        var Dataˉsymbols = fragment.Symbols
            .Where(Symbol => Symbol.Kind == Nativeˉsymbolˉkind.Data)
            .ToImmutableArray();
        var Dataˉstart = Dataˉsymbols.IsEmpty
            ? fragment.Code.Length
            : checked((int)Dataˉsymbols.Min(Symbol => Symbol.Offset));
        var Text = fragment.Code[..Dataˉstart].ToArray();
        foreach (var Patch in fragment.Patches)
        {
            Text.AsSpan(checked((int)Patch.Offset), sizeof(int)).Clear();
        }
        var Sections = ImmutableArray.CreateBuilder<Objectˉsection>();
        Sections.Add(new(
            ".text",
            Objectˉsectionˉkind.Code,
            fragment.Alignment,
            (uint)Text.Length,
            Text.ToImmutableArray()));
        if (!Dataˉsymbols.IsEmpty)
        {
            var Readˉonlyˉdata = fragment.Code[Dataˉstart..];
            Sections.Add(new(
                ".rodata",
                Objectˉsectionˉkind.Readˉonlyˉdata,
                fragment.Alignment,
                (uint)Readˉonlyˉdata.Length,
                Readˉonlyˉdata));
        }
        var Object = new Objectˉfile(
            fragment.Architecture,
            Sections.ToImmutable(),
            [.. fragment.Symbols.Select(Symbol => new Objectˉsymbol(
                Symbol.Name,
                Symbol.Binding switch
                {
                    Nativeˉsymbolˉbinding.Local => Objectˉsymbolˉbinding.Local,
                    Nativeˉsymbolˉbinding.Export => Objectˉsymbolˉbinding.Export,
                    Nativeˉsymbolˉbinding.Import => Objectˉsymbolˉbinding.Import,
                    _ => throw new InvalidOperationException("Verified native symbol binding became invalid."),
                },
                Symbol.Kind switch
                {
                    Nativeˉsymbolˉkind.Function => Objectˉsymbolˉkind.Function,
                    Nativeˉsymbolˉkind.Data => Objectˉsymbolˉkind.Data,
                    _ => throw new InvalidOperationException("Verified native symbol kind became invalid."),
                },
                Symbol.Binding == Nativeˉsymbolˉbinding.Import
                    ? Objectˉlimits.UNDEFINED_SECTION
                    : Symbol.Kind == Nativeˉsymbolˉkind.Data ? 1u : 0u,
                Symbol.Kind == Nativeˉsymbolˉkind.Data
                    ? checked(Symbol.Offset - (uint)Dataˉstart)
                    : Symbol.Offset,
                Symbol.Size))],
            [.. fragment.Patches.Select(Patch => new Objectˉrelocation(
                Patch.Kind switch
                {
                    Nativeˉpatchˉkind.Absoluteˉu32 => Objectˉrelocationˉkind.Absoluteˉu32,
                    Nativeˉpatchˉkind.Relativeˉi32 => Objectˉrelocationˉkind.Relativeˉi32,
                    _ => throw new InvalidOperationException("Verified native patch kind became invalid."),
                },
                0,
                Patch.Offset,
                checked((uint)Symbolˉindices[Patch.Symbol]),
                Patch.Addend))]);
        return new(Object, Text.Length, fragment.Code.Length - Dataˉstart);
    }

    private static ulong Alignˉup(ulong value, uint alignment)
    {
        var Mask = alignment - 1UL;
        return checked((value + Mask) & ~Mask);
    }

    private sealed record Nativeˉobjectˉprojection(
        Objectˉfile Object,
        int Textˉbytes,
        int Readˉonlyˉdataˉbytes);
}
