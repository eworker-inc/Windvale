using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉfragmentˉverifier
{
    private const ulong INTEGER_OVERFLOW_STATUS = 0x0000_0001_0000_0000UL;

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
                Fail("WVN3014", "The baseline native fragment supports only code function symbols.");
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
        if (!fragment.Patches.IsEmpty ||
            (!Isˉconstantˉshape(fragment) && !Isˉcheckedˉarithmeticˉshape(fragment)))
        {
            Fail(
                "WVN3030",
                "The x86-64 baseline fragment is outside the independently decoded constant/checked-i32 target shapes.");
        }
    }

    private static bool Isˉconstantˉshape(Nativeˉfragment fragment) =>
        fragment.Code.Length == 6 &&
        fragment.Code[0] == 0xB8 &&
        fragment.Code[5] == 0xC3 &&
        fragment.Symbols.Length == 1 &&
        fragment.Symbols[0] is
        {
            Name: "Main",
            Binding: Nativeˉsymbolˉbinding.Export,
            Kind: Nativeˉsymbolˉkind.Function,
            Offset: 0,
            Size: 6,
        };

    private static bool Isˉcheckedˉarithmeticˉshape(Nativeˉfragment fragment)
    {
        if (fragment.Symbols.Length != 2 ||
            fragment.Symbols[0] is not
            {
                Name: "$overflow",
                Binding: Nativeˉsymbolˉbinding.Local,
                Kind: Nativeˉsymbolˉkind.Function,
            } Trapˉsymbol ||
            fragment.Symbols[1] is not
            {
                Name: "Main",
                Binding: Nativeˉsymbolˉbinding.Export,
                Kind: Nativeˉsymbolˉkind.Function,
                Offset: 0,
            } Mainˉsymbol)
        {
            return false;
        }

        var Code = fragment.Code.AsSpan();
        var Trapˉoffset = checked((int)Trapˉsymbol.Offset);
        if (Trapˉoffset < 7 ||
            Trapˉoffset > Code.Length - 18 ||
            Mainˉsymbol.Size != Trapˉsymbol.Offset ||
            Trapˉsymbol.Size != (uint)(Code.Length - Trapˉoffset) ||
            !Matches(Code, 0, 0x48, 0x81, 0xEC))
        {
            return false;
        }
        var Frameˉbytes = BinaryPrimitives.ReadInt32LittleEndian(Code.Slice(3, sizeof(int)));
        if (Frameˉbytes is < 16 or > Nativeˉcontract.MAXIMUM_FRAME_BYTES ||
            (Frameˉbytes & 15) != 0 ||
            !Matches(Code, Trapˉoffset, 0x48, 0x81, 0xC4) ||
            BinaryPrimitives.ReadInt32LittleEndian(Code.Slice(Trapˉoffset + 3, sizeof(int))) != Frameˉbytes ||
            !Matches(Code, Trapˉoffset + 7, 0x48, 0xB8) ||
            BinaryPrimitives.ReadUInt64LittleEndian(Code.Slice(Trapˉoffset + 9, sizeof(ulong))) != INTEGER_OVERFLOW_STATUS ||
            Code[Trapˉoffset + 17] != 0xC3 ||
            Trapˉoffset + 18 != Code.Length)
        {
            return false;
        }

        var Initialized = new HashSet<int>();
        var Checkedˉoperations = 0;
        var Index = 7;
        var Returned = false;
        while (Index < Trapˉoffset)
        {
            if (Index + 15 == Trapˉoffset &&
                Matches(Code, Index, 0x8B, 0x84, 0x24) &&
                Tryˉreadˉslot(Code, Index + 3, Frameˉbytes, out var Returnˉslot) &&
                Initialized.Contains(Returnˉslot) &&
                Matches(Code, Index + 7, 0x48, 0x81, 0xC4) &&
                BinaryPrimitives.ReadInt32LittleEndian(Code.Slice(Index + 10, sizeof(int))) == Frameˉbytes &&
                Code[Index + 14] == 0xC3)
            {
                Returned = true;
                Index = Trapˉoffset;
                break;
            }

            if (Index + 12 <= Trapˉoffset &&
                Code[Index] == 0xB8 &&
                Matches(Code, Index + 5, 0x89, 0x84, 0x24) &&
                Tryˉreadˉslot(Code, Index + 8, Frameˉbytes, out var Constantˉslot) &&
                Constantˉslot == Initialized.Count &&
                Initialized.Add(Constantˉslot))
            {
                Index += 12;
                continue;
            }

            if (Index + 7 > Trapˉoffset ||
                !Matches(Code, Index, 0x8B, 0x84, 0x24) ||
                !Tryˉreadˉslot(Code, Index + 3, Frameˉbytes, out var Leftˉslot) ||
                !Initialized.Contains(Leftˉslot))
            {
                return false;
            }
            var Cursor = Index + 7;
            if (Cursor + 7 <= Trapˉoffset && Matches(Code, Cursor, 0x8B, 0x8C, 0x24))
            {
                if (!Tryˉreadˉslot(Code, Cursor + 3, Frameˉbytes, out var Rightˉslot) ||
                    !Initialized.Contains(Rightˉslot))
                {
                    return false;
                }
                Cursor += 7;
                if (Matches(Code, Cursor, 0x01, 0xC8) || Matches(Code, Cursor, 0x29, 0xC8))
                {
                    Cursor += 2;
                }
                else if (Matches(Code, Cursor, 0x0F, 0xAF, 0xC1))
                {
                    Cursor += 3;
                }
                else
                {
                    return false;
                }
            }
            else if (Matches(Code, Cursor, 0xF7, 0xD8))
            {
                Cursor += 2;
            }
            else
            {
                return false;
            }

            if (!Matches(Code, Cursor, 0x0F, 0x80) || Cursor + 13 > Trapˉoffset)
            {
                return false;
            }
            var Displacementˉoffset = Cursor + 2;
            var Displacement = BinaryPrimitives.ReadInt32LittleEndian(
                Code.Slice(Displacementˉoffset, sizeof(int)));
            if ((long)Displacementˉoffset + sizeof(int) + Displacement != Trapˉoffset)
            {
                return false;
            }
            Cursor += 6;
            if (!Matches(Code, Cursor, 0x89, 0x84, 0x24) ||
                !Tryˉreadˉslot(Code, Cursor + 3, Frameˉbytes, out var Resultˉslot) ||
                Resultˉslot != Initialized.Count ||
                !Initialized.Add(Resultˉslot))
            {
                return false;
            }
            Checkedˉoperations++;
            Index = Cursor + 7;
        }

        var Requiredˉframe = checked((Initialized.Count * sizeof(int) + 15) & ~15);
        return Returned && Checkedˉoperations > 0 && Requiredˉframe == Frameˉbytes;
    }

    private static bool Tryˉreadˉslot(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        out int slot)
    {
        slot = 0;
        if (offset < 0 || offset > code.Length - sizeof(int))
        {
            return false;
        }
        var Displacement = BinaryPrimitives.ReadInt32LittleEndian(code.Slice(offset, sizeof(int)));
        if (Displacement < 0 ||
            (Displacement & (sizeof(int) - 1)) != 0 ||
            Displacement > frameˉbytes - sizeof(int))
        {
            return false;
        }
        slot = Displacement / sizeof(int);
        return true;
    }

    private static bool Matches(ReadOnlySpan<byte> code, int offset, params byte[] expected) =>
        offset >= 0 &&
        offset <= code.Length - expected.Length &&
        code.Slice(offset, expected.Length).SequenceEqual(expected);

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
