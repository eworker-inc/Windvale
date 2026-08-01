using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉfragmentˉverifier
{
    private const ulong INTEGER_OVERFLOW_STATUS = 0x0000_0001_0000_0000UL;
    private const ulong INSTRUCTION_LIMIT_STATUS = 0x0000_0002_0000_0000UL;

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
        if (!fragment.Patches.IsEmpty || !Isˉstructuredˉshape(fragment))
        {
            Fail(
                "WVN3030",
                "The x86-64 baseline fragment is outside the independently decoded budgeted-control target shape.");
        }
    }

    private static bool Isˉstructuredˉshape(Nativeˉfragment fragment)
    {
        if (fragment.Symbols.Length != 3 ||
            fragment.Symbols[0] is not
            {
                Name: "$instruction_limit",
                Binding: Nativeˉsymbolˉbinding.Local,
                Kind: Nativeˉsymbolˉkind.Function,
            } Instructionˉlimitˉsymbol ||
            fragment.Symbols[1] is not
            {
                Name: "$overflow",
                Binding: Nativeˉsymbolˉbinding.Local,
                Kind: Nativeˉsymbolˉkind.Function,
            } Trapˉsymbol ||
            fragment.Symbols[2] is not
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
        var Instructionˉlimitˉoffset = checked((int)Instructionˉlimitˉsymbol.Offset);
        if (Trapˉoffset < 7 ||
            Trapˉoffset > Code.Length - 36 ||
            Instructionˉlimitˉoffset != Trapˉoffset + 18 ||
            Mainˉsymbol.Size != Trapˉsymbol.Offset ||
            Trapˉsymbol.Size != 18 ||
            Instructionˉlimitˉsymbol.Size != 18 ||
            Instructionˉlimitˉoffset + 18 != Code.Length ||
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
            !Matches(Code, Instructionˉlimitˉoffset, 0x48, 0x81, 0xC4) ||
            BinaryPrimitives.ReadInt32LittleEndian(Code.Slice(Instructionˉlimitˉoffset + 3, sizeof(int))) != Frameˉbytes ||
            !Matches(Code, Instructionˉlimitˉoffset + 7, 0x48, 0xB8) ||
            BinaryPrimitives.ReadUInt64LittleEndian(Code.Slice(Instructionˉlimitˉoffset + 9, sizeof(ulong))) != INSTRUCTION_LIMIT_STATUS ||
            Code[Instructionˉlimitˉoffset + 17] != 0xC3)
        {
            return false;
        }

        var Index = 7;
        if (!Matches(Code, Index, 0x49, 0x89, 0xD3))
        {
            return false;
        }
        Index += 3;
        if (!Matches(Code, Index, 0x31, 0xC0))
        {
            return false;
        }
        Index += 2;
        var Frameˉslots = Frameˉbytes / sizeof(int);
        for (var Slot = 0; Slot < Frameˉslots; Slot++)
        {
            if (!Matches(Code, Index, 0x89, 0x84, 0x24) ||
                !Tryˉreadˉslot(Code, Index + 3, Frameˉbytes, out var Initializedˉslot) ||
                Initializedˉslot != Slot)
            {
                return false;
            }
            Index += 7;
        }
        var Bodyˉstart = Index;
        if (Bodyˉstart >= Trapˉoffset)
        {
            return false;
        }

        var Groups = new List<Nativeˉdecodedˉgroup>();
        var Maximumˉbodyˉslot = -1;
        var Transformations = 0;
        var Returns = 0;
        while (Index < Trapˉoffset)
        {
            var Start = Index;

            if (Index + 10 <= Trapˉoffset &&
                Matches(Code, Index, 0x49, 0x83, 0xEB, 0x01, 0x0F, 0x82) &&
                Tryˉreadˉtarget(Code, Index + 6, out var Limitˉtarget) &&
                Limitˉtarget == Instructionˉlimitˉoffset)
            {
                Index += 10;
                Groups.Add(new(Start, Index - Start, true, false, true, []));
                continue;
            }

            if (Index + 15 <= Trapˉoffset &&
                Tryˉloadˉeax(Code, Index, Frameˉbytes, out var Returnˉslot) &&
                Matches(Code, Index + 7, 0x48, 0x81, 0xC4) &&
                BinaryPrimitives.ReadInt32LittleEndian(Code.Slice(Index + 10, sizeof(int))) == Frameˉbytes &&
                Code[Index + 14] == 0xC3)
            {
                Trackˉslot(Returnˉslot, ref Maximumˉbodyˉslot);
                Index += 15;
                Groups.Add(new(Start, Index - Start, false, true, false, []));
                Returns++;
                continue;
            }

            if (Index + 20 <= Trapˉoffset &&
                Tryˉloadˉeax(Code, Index, Frameˉbytes, out var Conditionˉslot) &&
                Matches(Code, Index + 7, 0x85, 0xC0, 0x0F, 0x85) &&
                Tryˉreadˉtarget(Code, Index + 11, out var Trueˉtarget) &&
                Code[Index + 15] == 0xE9 &&
                Tryˉreadˉtarget(Code, Index + 16, out var Falseˉtarget))
            {
                Trackˉslot(Conditionˉslot, ref Maximumˉbodyˉslot);
                Index += 20;
                Groups.Add(new(Start, Index - Start, false, false, false, [Trueˉtarget, Falseˉtarget]));
                continue;
            }

            if (Code[Index] == 0xE9 && Index + 5 <= Trapˉoffset &&
                Tryˉreadˉtarget(Code, Index + 1, out var Jumpˉtarget))
            {
                Index += 5;
                Groups.Add(new(Start, Index - Start, false, false, false, [Jumpˉtarget]));
                continue;
            }

            if (Index + 12 <= Trapˉoffset && Code[Index] == 0xB8 &&
                Tryˉstoreˉeax(Code, Index + 5, Frameˉbytes, out var Constantˉslot))
            {
                Trackˉslot(Constantˉslot, ref Maximumˉbodyˉslot);
                Index += 12;
                Groups.Add(new(Start, Index - Start, true, false, false, []));
                Transformations++;
                continue;
            }

            if (!Tryˉloadˉeax(Code, Index, Frameˉbytes, out var Leftˉslot))
            {
                return false;
            }
            Trackˉslot(Leftˉslot, ref Maximumˉbodyˉslot);
            var Cursor = Index + 7;

            if (Tryˉloadˉecx(Code, Cursor, Frameˉbytes, out var Rightˉslot))
            {
                Trackˉslot(Rightˉslot, ref Maximumˉbodyˉslot);
                Cursor += 7;

                var Arithmeticˉlength = 0;
                if (Matches(Code, Cursor, 0x01, 0xC8) || Matches(Code, Cursor, 0x29, 0xC8))
                {
                    Arithmeticˉlength = 2;
                }
                else if (Matches(Code, Cursor, 0x0F, 0xAF, 0xC1))
                {
                    Arithmeticˉlength = 3;
                }
                if (Arithmeticˉlength != 0)
                {
                    Cursor += Arithmeticˉlength;
                    if (!Matches(Code, Cursor, 0x0F, 0x80) ||
                        !Tryˉreadˉtarget(Code, Cursor + 2, out var Overflowˉtarget) ||
                        Overflowˉtarget != Trapˉoffset)
                    {
                        return false;
                    }
                    Cursor += 6;
                    if (!Tryˉstoreˉeax(Code, Cursor, Frameˉbytes, out var Resultˉslot))
                    {
                        return false;
                    }
                    Trackˉslot(Resultˉslot, ref Maximumˉbodyˉslot);
                    Cursor += 7;
                    Index = Cursor;
                    Groups.Add(new(Start, Index - Start, true, false, false, []));
                    Transformations++;
                    continue;
                }

                if (Matches(Code, Cursor, 0x39, 0xC8, 0x0F) &&
                    Cursor + 15 <= Trapˉoffset &&
                    Isˉcondition(Code[Cursor + 3]) &&
                    Matches(Code, Cursor + 4, 0xC0, 0x0F, 0xB6, 0xC0) &&
                    Tryˉstoreˉeax(Code, Cursor + 8, Frameˉbytes, out var Comparisonˉslot))
                {
                    Trackˉslot(Comparisonˉslot, ref Maximumˉbodyˉslot);
                    Cursor += 15;
                    Index = Cursor;
                    Groups.Add(new(Start, Index - Start, true, false, false, []));
                    Transformations++;
                    continue;
                }
            }

            Cursor = Index + 7;
            if (Matches(Code, Cursor, 0xF7, 0xD8))
            {
                Cursor += 2;
                if (!Matches(Code, Cursor, 0x0F, 0x80) ||
                    !Tryˉreadˉtarget(Code, Cursor + 2, out var Overflowˉtarget) ||
                    Overflowˉtarget != Trapˉoffset)
                {
                    return false;
                }
                Cursor += 6;
                if (!Tryˉstoreˉeax(Code, Cursor, Frameˉbytes, out var Negateˉslot))
                {
                    return false;
                }
                Trackˉslot(Negateˉslot, ref Maximumˉbodyˉslot);
                Cursor += 7;
                Index = Cursor;
                Groups.Add(new(Start, Index - Start, true, false, false, []));
                Transformations++;
                continue;
            }

            if (Matches(Code, Cursor, 0x83, 0xF0, 0x01) &&
                Tryˉstoreˉeax(Code, Cursor + 3, Frameˉbytes, out var Notˉslot))
            {
                Trackˉslot(Notˉslot, ref Maximumˉbodyˉslot);
                Cursor += 10;
                Index = Cursor;
                Groups.Add(new(Start, Index - Start, true, false, false, []));
                Transformations++;
                continue;
            }

            if (Tryˉstoreˉeax(Code, Cursor, Frameˉbytes, out var Copyˉslot))
            {
                Trackˉslot(Copyˉslot, ref Maximumˉbodyˉslot);
                Cursor += 7;
                Index = Cursor;
                Groups.Add(new(Start, Index - Start, true, false, false, []));
                Transformations++;
                continue;
            }

            return false;
        }

        if (Index != Trapˉoffset ||
            Groups.Count == 0 ||
            Transformations == 0 ||
            Returns == 0 ||
            Maximumˉbodyˉslot < 0)
        {
            return false;
        }
        var Requiredˉframe = checked(((Maximumˉbodyˉslot + 1) * sizeof(int) + 15) & ~15);
        if (Requiredˉframe != Frameˉbytes)
        {
            return false;
        }

        var Groupˉindices = Groups
            .Select((Group, Groupˉindex) => (Group.Offset, Groupˉindex))
            .ToDictionary(Item => Item.Offset, Item => Item.Groupˉindex);
        if (!Groups[0].Isˉinstructionˉcharge)
        {
            return false;
        }
        for (var Groupˉindex = 0; Groupˉindex < Groups.Count; Groupˉindex++)
        {
            var Group = Groups[Groupˉindex];
            if (Group.Isˉinstructionˉcharge)
            {
                if (!Group.Fallsˉthrough || Group.Returns || Group.Targets.Length != 0 ||
                    Groupˉindex + 1 >= Groups.Count || Groups[Groupˉindex + 1].Isˉinstructionˉcharge)
                {
                    return false;
                }
            }
            else if (Groupˉindex == 0 || !Groups[Groupˉindex - 1].Isˉinstructionˉcharge)
            {
                return false;
            }
            foreach (var Target in Group.Targets)
            {
                if (!Groupˉindices.TryGetValue(Target, out var Targetˉindex) ||
                    !Groups[Targetˉindex].Isˉinstructionˉcharge)
                {
                    return false;
                }
            }
            if (Group.Fallsˉthrough && Groupˉindex + 1 >= Groups.Count)
            {
                return false;
            }
        }

        var Reachable = new bool[Groups.Count];
        var Pending = new Queue<int>();
        Reachable[0] = true;
        Pending.Enqueue(0);
        while (Pending.TryDequeue(out var Groupˉindex))
        {
            var Group = Groups[Groupˉindex];
            if (Group.Fallsˉthrough)
            {
                Enqueue(Groupˉindex + 1, Reachable, Pending);
            }
            foreach (var Target in Group.Targets)
            {
                Enqueue(Groupˉindices[Target], Reachable, Pending);
            }
        }
        return Reachable.All(Value => Value);
    }

    private static bool Tryˉloadˉeax(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
    }

    private static bool Tryˉloadˉecx(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x8C, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
    }

    private static bool Tryˉstoreˉeax(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x89, 0x84, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
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

    private static bool Tryˉreadˉtarget(ReadOnlySpan<byte> code, int displacementˉoffset, out int target)
    {
        target = 0;
        if (displacementˉoffset < 0 || displacementˉoffset > code.Length - sizeof(int))
        {
            return false;
        }
        var Displacement = BinaryPrimitives.ReadInt32LittleEndian(
            code.Slice(displacementˉoffset, sizeof(int)));
        var Target = (long)displacementˉoffset + sizeof(int) + Displacement;
        if (Target is < int.MinValue or > int.MaxValue)
        {
            return false;
        }
        target = (int)Target;
        return true;
    }

    private static bool Isˉcondition(byte condition) =>
        condition is 0x94 or 0x95 or 0x9C or 0x9D or 0x9E or 0x9F;

    private static void Trackˉslot(int slot, ref int maximum) => maximum = Math.Max(maximum, slot);

    private static void Enqueue(int index, bool[] reachable, Queue<int> pending)
    {
        if (!reachable[index])
        {
            reachable[index] = true;
            pending.Enqueue(index);
        }
    }

    private static bool Matches(ReadOnlySpan<byte> code, int offset, params byte[] expected) =>
        offset >= 0 &&
        offset <= code.Length - expected.Length &&
        code.Slice(offset, expected.Length).SequenceEqual(expected);

    private sealed record Nativeˉdecodedˉgroup(
        int Offset,
        int Length,
        bool Fallsˉthrough,
        bool Returns,
        bool Isˉinstructionˉcharge,
        int[] Targets);

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
