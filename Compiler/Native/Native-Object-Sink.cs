using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉobjectˉsink
{
    public static ImmutableArray<byte> Writeˉwvo(Nativeˉfragment fragment)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        var Symbolˉindices = fragment.Symbols
            .Select((Symbol, Index) => (Symbol.Name, Index))
            .ToDictionary(Entry => Entry.Name, Entry => Entry.Index, StringComparer.Ordinal);
        var Object = new Objectˉfile(
            fragment.Architecture,
            [new(".text", Objectˉsectionˉkind.Code, fragment.Alignment, (uint)fragment.Code.Length, fragment.Code)],
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
                Symbol.Binding == Nativeˉsymbolˉbinding.Import ? Objectˉlimits.UNDEFINED_SECTION : 0,
                Symbol.Offset,
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
        var Bytes = Objectˉcodec.Write(Object);
        _ = Objectˉcodec.Readˉandˉverify(Bytes);
        return Bytes.ToImmutableArray();
    }
}
