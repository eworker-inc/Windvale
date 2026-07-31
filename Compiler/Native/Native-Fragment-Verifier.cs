using System.Diagnostics.CodeAnalysis;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉfragmentˉverifier
{
    public static Nativeˉfragment Verify(Nativeˉfragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        if (!StringComparer.Ordinal.Equals(fragment.Target, Nativeˉcontract.X64_BASELINE_TARGET))
        {
            Fail("WVN3001", $"Unknown native target '{fragment.Target}'.");
        }
        if (fragment.Abiˉversion != Nativeˉcontract.ABI_VERSION)
        {
            Fail("WVN3002", $"Unsupported native ABI version {fragment.Abiˉversion}.");
        }
        if (fragment.Architecture != Objectˉarchitecture.X86ˉ64)
        {
            Fail("WVN3003", "The baseline native fragment must target x86-64.");
        }
        if (fragment.Code.IsDefault || fragment.Symbols.IsDefault || fragment.Patches.IsDefault)
        {
            Fail("WVN3004", "Native fragment collections must be initialized.");
        }
        if (fragment.Code.Length is < 1 or > Nativeˉcontract.MAXIMUM_CODE_BYTES)
        {
            Fail("WVN3005", "The native fragment code size is outside its bounded range.");
        }
        if (fragment.Alignment is 0 or > Objectˉlimits.MAX_ALIGNMENT ||
            (fragment.Alignment & (fragment.Alignment - 1)) != 0)
        {
            Fail("WVN3006", "The native fragment alignment is invalid.");
        }
        if (fragment.Symbols.Length > Objectˉlimits.MAX_SYMBOLS)
        {
            Fail("WVN3007", "The native fragment exceeds the symbol-count limit.");
        }
        if (fragment.Patches.Length > Objectˉlimits.MAX_RELOCATIONS)
        {
            Fail("WVN3008", "The native fragment exceeds the patch-count limit.");
        }

        Verifyˉsymbols(fragment);
        Verifyˉpatches(fragment);
        Verifyˉtargetˉshape(fragment);
        return fragment;
    }

    private static void Verifyˉsymbols(Nativeˉfragment fragment)
    {
        var Names = new HashSet<string>(StringComparer.Ordinal);
        Nativeˉsymbol? Previous = null;
        foreach (var Symbol in fragment.Symbols)
        {
            if (Symbol is null ||
                !Enum.IsDefined(Symbol.Binding) ||
                !Enum.IsDefined(Symbol.Kind) ||
                !Objectˉverifier.Isˉmachineˉname(Symbol.Name) ||
                !Names.Add(Symbol.Name))
            {
                Fail("WVN3010", "A native symbol is invalid or duplicated.");
            }
            if (Previous is not null &&
                (Symbol.Binding < Previous.Binding ||
                    (Symbol.Binding == Previous.Binding &&
                        StringComparer.Ordinal.Compare(Previous.Name, Symbol.Name) >= 0)))
            {
                Fail("WVN3011", "Native symbols must be strictly ordered by binding and name.");
            }
            Previous = Symbol;

            if (Symbol.Binding == Nativeˉsymbolˉbinding.Import)
            {
                if (Symbol.Offset != 0 || Symbol.Size != 0)
                {
                    Fail("WVN3012", $"Imported native symbol '{Symbol.Name}' must use a zero range.");
                }
                continue;
            }
            if (Symbol.Offset > (uint)fragment.Code.Length ||
                Symbol.Size > (uint)fragment.Code.Length - Symbol.Offset)
            {
                Fail("WVN3013", $"Defined native symbol '{Symbol.Name}' is outside the code fragment.");
            }
            if (Symbol.Kind != Nativeˉsymbolˉkind.Function)
            {
                Fail("WVN3014", "The first native fragment supports only code function symbols.");
            }
        }
    }

    private static void Verifyˉpatches(Nativeˉfragment fragment)
    {
        var Symbols = fragment.Symbols.ToDictionary(Symbol => Symbol.Name, StringComparer.Ordinal);
        Nativeˉpatch? Previous = null;
        foreach (var Patch in fragment.Patches)
        {
            if (Patch is null || !Enum.IsDefined(Patch.Kind))
            {
                Fail("WVN3020", "A native patch kind is invalid.");
            }
            if (!Symbols.ContainsKey(Patch.Symbol))
            {
                Fail("WVN3021", $"Native patch target '{Patch.Symbol}' is undefined.");
            }
            if (Patch.Offset > (uint)fragment.Code.Length ||
                sizeof(uint) > (uint)fragment.Code.Length - Patch.Offset)
            {
                Fail("WVN3022", "A native patch range is outside the code fragment.");
            }
            if (Previous is not null && Patch.Offset < Previous.Offset + sizeof(uint))
            {
                Fail("WVN3023", "Native patches must be ordered and cannot overlap.");
            }
            Previous = Patch;
            if (!fragment.Code.AsSpan((int)Patch.Offset, sizeof(uint)).SequenceEqual(new byte[] { 0, 0, 0, 0 }))
            {
                Fail("WVN3024", "Native patch placeholder bytes must be zero.");
            }
        }
    }

    private static void Verifyˉtargetˉshape(Nativeˉfragment fragment)
    {
        if (fragment.Code.Length != 6 ||
            fragment.Code[0] != 0xB8 ||
            fragment.Code[5] != 0xC3 ||
            fragment.Symbols.Length != 1 ||
            fragment.Symbols[0] is not
            {
                Name: "Main",
                Binding: Nativeˉsymbolˉbinding.Export,
                Kind: Nativeˉsymbolˉkind.Function,
                Offset: 0,
                Size: 6,
            } ||
            !fragment.Patches.IsEmpty)
        {
            Fail(
                "WVN3030",
                "The first x86-64 baseline fragment must be one exact mov-eax-i32/return Main function without patches.");
        }
    }

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
