using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉfragmentˉverifier
{
    private const ulong INTEGER_OVERFLOW_STATUS = 0x0000_0001_0000_0000UL;
    private const ulong INSTRUCTION_LIMIT_STATUS = 0x0000_0002_0000_0000UL;
    private const ulong CALL_DEPTH_STATUS = 0x0000_0003_0000_0000UL;
    private const ulong DATA_BOUNDS_STATUS = 0x0000_0004_0000_0000UL;
    private const ulong RUNTIME_SERVICE_STATUS = 0x0000_0005_0000_0000UL;
    private const int INTERNAL_FUNCTION_SUFFIX_BYTES = 109;
    private const int MAIN_FUNCTION_SUFFIX_BYTES = 121;
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

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
        if (fragment.Code.IsDefault ||
            fragment.Symbols.IsDefault ||
            fragment.Patches.IsDefault ||
            fragment.Requiredˉservices.IsDefault)
        {
            Fail("WVN3004", "Native fragment collections must be initialized.");
        }
        if (fragment.Code.Length is < 1 or > Nativeˉcontract.MAXIMUM_CODE_BYTES)
        {
            Fail("WVN3005", "The native fragment code size is outside its bounded range.");
        }
        if (fragment.Alignment != 16)
        {
            Fail("WVN3006", "The x86-64 baseline fragment requires 16-byte alignment.");
        }
        if (fragment.Symbols.Length > Objectˉlimits.MAX_SYMBOLS)
        {
            Fail("WVN3007", "The native fragment exceeds the symbol-count limit.");
        }
        if (fragment.Patches.Length > Objectˉlimits.MAX_RELOCATIONS)
        {
            Fail("WVN3008", "The native fragment exceeds the patch-count limit.");
        }
        if (fragment.Requiredˉservices.Length > 1 ||
            (fragment.Requiredˉservices.Length == 1 &&
                fragment.Requiredˉservices[0] != Nativeˉservice.Consoleˉwriteˉline))
        {
            Fail("WVN3009", "The native fragment requires unsupported or noncanonical runtime services.");
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
        var Exportˉcount = 0;
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
                Fail("WVN3012", "The x86-64 baseline fragment does not admit imports.");
            }
            if (Symbol.Offset > (uint)fragment.Code.Length ||
                Symbol.Size > (uint)fragment.Code.Length - Symbol.Offset)
            {
                Fail("WVN3013", $"Defined native symbol '{Symbol.Name}' is outside the fragment.");
            }
            if (Symbol.Kind == Nativeˉsymbolˉkind.Function && Symbol.Size == 0)
            {
                Fail("WVN3014", $"Native function '{Symbol.Name}' is empty.");
            }
            if (Symbol.Binding == Nativeˉsymbolˉbinding.Export)
            {
                Exportˉcount++;
                if (Symbol is not
                    {
                        Name: "Main",
                        Kind: Nativeˉsymbolˉkind.Function,
                    })
                {
                    Fail("WVN3015", "The sole native export must be the Main function.");
                }
            }
            else if (Symbol.Kind == Nativeˉsymbolˉkind.Data &&
                Symbol.Binding != Nativeˉsymbolˉbinding.Local)
            {
                Fail("WVN3015", "Native static data must be local.");
            }
        }
        if (Exportˉcount != 1)
        {
            Fail("WVN3015", "The x86-64 baseline fragment requires exactly one Main export.");
        }
    }

    private static void Verifyˉpatches(Nativeˉfragment fragment)
    {
        var Symbols = fragment.Symbols.ToDictionary(Symbol => Symbol.Name, StringComparer.Ordinal);
        Nativeˉpatch? Previous = null;
        foreach (var Patch in fragment.Patches)
        {
            if (Patch is null ||
                Patch.Kind != Nativeˉpatchˉkind.Relativeˉi32 ||
                Patch.Addend != -sizeof(int))
            {
                Fail("WVN3020", "A native patch is outside the canonical RIP-relative static-data form.");
            }
            if (!Symbols.TryGetValue(Patch.Symbol, out var Symbol) ||
                Symbol.Kind != Nativeˉsymbolˉkind.Data)
            {
                Fail("WVN3020", "A native patch target is not declared static data.");
            }
            if (Patch.Offset > (uint)fragment.Code.Length ||
                sizeof(int) > (uint)fragment.Code.Length - Patch.Offset)
            {
                Fail("WVN3022", "A native patch range is outside the fragment.");
            }
            if (Previous is not null && Patch.Offset < Previous.Offset + sizeof(int))
            {
                Fail("WVN3023", "Native patches must be ordered and cannot overlap.");
            }
            Previous = Patch;
            var Expected = checked((int)Symbol.Offset + Patch.Addend - (int)Patch.Offset);
            var Actual = BinaryPrimitives.ReadInt32LittleEndian(
                fragment.Code.AsSpan((int)Patch.Offset, sizeof(int)));
            if (Actual != Expected)
            {
                Fail("WVN3024", "A native static-data patch does not encode its declared target.");
            }
        }
    }

    private static void Verifyˉtargetˉshape(Nativeˉfragment fragment)
    {
        var Functions = fragment.Symbols
            .Where(Symbol => Symbol.Kind == Nativeˉsymbolˉkind.Function)
            .OrderBy(Symbol => Symbol.Offset)
            .ToArray();
        var Data = fragment.Symbols
            .Where(Symbol => Symbol.Kind == Nativeˉsymbolˉkind.Data)
            .OrderBy(Symbol => Symbol.Name, StringComparer.Ordinal)
            .ToArray();
        if (Functions.Length == 0 || Functions[0].Offset != 0)
        {
            Failˉshape();
        }

        var Functionˉend = 0;
        foreach (var Function in Functions)
        {
            if (Function.Offset != (uint)Functionˉend)
            {
                Failˉshape();
            }
            Functionˉend = checked((int)(Function.Offset + Function.Size));
        }

        var Dataˉstart = fragment.Code.Length;
        if (Data.Length != 0)
        {
            for (var Index = 0; Index < Data.Length; Index++)
            {
                if (!StringComparer.Ordinal.Equals(Data[Index].Name, $"$data_{Index:D4}"))
                {
                    Failˉshape();
                }
            }
            Dataˉstart = checked((int)Data[0].Offset);
            if ((Dataˉstart & 15) != 0 || Dataˉstart < Functionˉend || Dataˉstart - Functionˉend > 15)
            {
                Failˉshape();
            }
            for (var Offset = Functionˉend; Offset < Dataˉstart; Offset++)
            {
                if (fragment.Code[Offset] != 0x90)
                {
                    Failˉshape();
                }
            }
            var Cursor = Dataˉstart;
            foreach (var Symbol in Data)
            {
                if (Symbol.Offset != (uint)Cursor)
                {
                    Failˉshape();
                }
                Cursor = checked(Cursor + (int)Symbol.Size);
            }
            if (Cursor != fragment.Code.Length)
            {
                Failˉshape();
            }
        }
        else if (Functionˉend != fragment.Code.Length || !fragment.Patches.IsEmpty)
        {
            Failˉshape();
        }

        var Patchˉlookup = fragment.Patches.ToDictionary(Patch => checked((int)Patch.Offset));
        var Usedˉpatches = new HashSet<int>();
        var Functionˉstarts = Functions.ToDictionary(Symbol => checked((int)Symbol.Offset));
        var Dataˉsymbols = Data.ToDictionary(Symbol => Symbol.Name, StringComparer.Ordinal);
        var Decoded = new Dictionary<int, Decodedˉfunction>();
        foreach (var Function in Functions)
        {
            var Start = checked((int)Function.Offset);
            Decoded.Add(
                Start,
                Decodeˉfunction(
                    fragment,
                    Function,
                    Patchˉlookup,
                    Usedˉpatches,
                    Functionˉstarts,
                    Dataˉsymbols));
        }
        if (Usedˉpatches.Count != fragment.Patches.Length)
        {
            Failˉshape();
        }
        foreach (var Function in Decoded.Values)
        {
            foreach (var Call in Function.Calls)
            {
                if (!Decoded.TryGetValue(Call.Target, out var Callee) ||
                    Call.Argumentˉcount != Callee.Parameterˉcount)
                {
                    Failˉshape();
                }
            }
        }
    }

    private static Decodedˉfunction Decodeˉfunction(
        Nativeˉfragment fragment,
        Nativeˉsymbol symbol,
        Dictionary<int, Nativeˉpatch> patches,
        HashSet<int> usedˉpatches,
        Dictionary<int, Nativeˉsymbol> functions,
        Dictionary<string, Nativeˉsymbol> dataˉsymbols)
    {
        var Code = fragment.Code.AsSpan();
        var Start = checked((int)symbol.Offset);
        var End = checked(Start + (int)symbol.Size);
        var Isˉmain = symbol.Binding == Nativeˉsymbolˉbinding.Export;
        var Suffixˉbytes = Isˉmain ? MAIN_FUNCTION_SUFFIX_BYTES : INTERNAL_FUNCTION_SUFFIX_BYTES;
        if (End - Start <= Suffixˉbytes + 20)
        {
            Failˉshape();
        }
        var Index = Start;
        if (Isˉmain)
        {
            if (!Matches(
                Code,
                Index,
                0x41, 0x57,
                0x49, 0x89, 0xD7,
                0x4D, 0x8B, 0x5F, Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
                0x4D, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET))
            {
                Failˉshape();
            }
            Index += 13;
        }
        if (!Matches(Code, Index, 0x49, 0x83, 0xEA, 0x01, 0x0F, 0x82))
        {
            Failˉshape();
        }
        if (!Tryˉreadˉtarget(Code, Index + 6, out var Depthˉtarget))
        {
            Failˉshape();
        }
        Index += 10;
        if (!Matches(Code, Index, 0x48, 0x81, 0xEC))
        {
            Failˉshape();
        }
        var Frameˉbytes = Readˉi32(Code, Index + 3);
        if (Frameˉbytes is < 16 or > Nativeˉcontract.MAXIMUM_FRAME_BYTES ||
            (Frameˉbytes & 15) != 0)
        {
            Failˉshape();
        }
        Index += 7;
        if (!Matches(Code, Index, 0x31, 0xC0))
        {
            Failˉshape();
        }
        Index += 2;
        for (var Slot = 0; Slot < Frameˉbytes / sizeof(int); Slot++)
        {
            if (!Tryˉstoreˉeax(Code, Index, Frameˉbytes, out var Initialized) || Initialized != Slot)
            {
                Failˉshape();
            }
            Index += 7;
        }

        var Parameterˉcount = 0;
        while (Parameterˉcount < Nativeˉcontract.MAXIMUM_CALL_PARAMETERS &&
            Tryˉstoreˉargument(Code, Index, Frameˉbytes, Parameterˉcount, out var Parameterˉlength))
        {
            Parameterˉcount++;
            Index += Parameterˉlength;
        }
        if (Isˉmain && Parameterˉcount != 0)
        {
            Failˉshape();
        }

        var Restoreˉbytes = Isˉmain ? 13 : 11;
        var Statusˉbytes = Isˉmain ? 23 : 21;
        var Propagate = End - Suffixˉbytes;
        var Overflow = Propagate + Restoreˉbytes;
        var Instructionˉlimit = Overflow + Statusˉbytes;
        var Bounds = Instructionˉlimit + Statusˉbytes;
        var Runtimeˉservice = Bounds + Statusˉbytes;
        var Depth = Runtimeˉservice + Statusˉbytes;
        if (Depthˉtarget != Depth ||
            !Matchesˉpropagate(Code, Propagate, Frameˉbytes, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Overflow, Frameˉbytes, INTEGER_OVERFLOW_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Instructionˉlimit, Frameˉbytes, INSTRUCTION_LIMIT_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Bounds, Frameˉbytes, DATA_BOUNDS_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Runtimeˉservice, Frameˉbytes, RUNTIME_SERVICE_STATUS, Isˉmain) ||
            !Matches(Code, Depth, 0x49, 0xFF, 0xC2, 0x48, 0xB8) ||
            BinaryPrimitives.ReadUInt64LittleEndian(Code.Slice(Depth + 5, sizeof(ulong))) != CALL_DEPTH_STATUS ||
            (Isˉmain && !Matches(Code, Depth + 13, 0x41, 0x5F, 0xC3)) ||
            (!Isˉmain && Code[Depth + 13] != 0xC3))
        {
            Failˉshape();
        }

        var Groups = new List<Decodedˉgroup>();
        var Calls = new List<Decodedˉcall>();
        var Returns = 0;
        while (Index < Propagate)
        {
            var Groupˉstart = Index;
            if (Matches(Code, Index, 0x49, 0x83, 0xEB, 0x01, 0x0F, 0x82) &&
                Tryˉreadˉtarget(Code, Index + 6, out var Limitˉtarget) &&
                Limitˉtarget == Instructionˉlimit)
            {
                Index += 10;
                Groups.Add(new(Groupˉstart, true, false, true, []));
                continue;
            }

            if (Tryˉloadˉeax(Code, Index, Frameˉbytes, out _) &&
                Matches(Code, Index + 7, 0x48, 0x81, 0xC4) &&
                Readˉi32(Code, Index + 10) == Frameˉbytes &&
                Matchesˉrestoreˉdepthˉandˉreturn(Code, Index + 14, Isˉmain))
            {
                Index += Isˉmain ? 20 : 18;
                Returns++;
                Groups.Add(new(Groupˉstart, false, true, false, []));
                continue;
            }

            if (Tryˉdecodeˉcall(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Propagate,
                functions,
                out var Call))
            {
                Calls.Add(Call);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉdataˉload(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Bounds,
                patches,
                usedˉpatches,
                dataˉsymbols))
            {
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉconsoleˉwriteˉline(
                fragment,
                Code,
                ref Index,
                Propagate,
                Runtimeˉservice,
                patches,
                usedˉpatches,
                dataˉsymbols))
            {
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉloadˉeax(Code, Index, Frameˉbytes, out _) &&
                Matches(Code, Index + 7, 0x85, 0xC0, 0x0F, 0x85) &&
                Tryˉreadˉtarget(Code, Index + 11, out var Trueˉtarget) &&
                Code[Index + 15] == 0xE9 &&
                Tryˉreadˉtarget(Code, Index + 16, out var Falseˉtarget))
            {
                Index += 20;
                Groups.Add(new(Groupˉstart, false, false, false, [Trueˉtarget, Falseˉtarget]));
                continue;
            }
            if (Code[Index] == 0xE9 && Tryˉreadˉtarget(Code, Index + 1, out var Jumpˉtarget))
            {
                Index += 5;
                Groups.Add(new(Groupˉstart, false, false, false, [Jumpˉtarget]));
                continue;
            }
            if (Code[Index] == 0xB8 &&
                Tryˉstoreˉeax(Code, Index + 5, Frameˉbytes, out _))
            {
                Index += 12;
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }
            if (!Tryˉdecodeˉslotˉtransformation(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Overflow))
            {
                Failˉshape();
            }
            Groups.Add(new(Groupˉstart, true, false, false, []));
        }

        if (Index != Propagate || Groups.Count == 0 || Returns == 0 || !Groups[0].Isˉcharge)
        {
            Failˉshape();
        }
        var Groupˉindices = Groups
            .Select((Group, Groupˉindex) => (Group.Offset, Groupˉindex))
            .ToDictionary(Item => Item.Offset, Item => Item.Groupˉindex);
        for (var Groupˉindex = 0; Groupˉindex < Groups.Count; Groupˉindex++)
        {
            var Group = Groups[Groupˉindex];
            if (Group.Isˉcharge)
            {
                if (!Group.Fallsˉthrough || Group.Returns || Group.Targets.Length != 0 ||
                    Groupˉindex + 1 >= Groups.Count || Groups[Groupˉindex + 1].Isˉcharge)
                {
                    Failˉshape();
                }
            }
            else if (Groupˉindex == 0 || !Groups[Groupˉindex - 1].Isˉcharge)
            {
                Failˉshape();
            }
            foreach (var Target in Group.Targets)
            {
                if (!Groupˉindices.TryGetValue(Target, out var Targetˉindex) ||
                    !Groups[Targetˉindex].Isˉcharge)
                {
                    Failˉshape();
                }
            }
            if (Group.Fallsˉthrough && Groupˉindex + 1 >= Groups.Count)
            {
                Failˉshape();
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
        if (Reachable.Any(Value => !Value))
        {
            Failˉshape();
        }
        return new(Parameterˉcount, Calls.ToArray());
    }

    private static bool Tryˉdecodeˉcall(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int propagate,
        Dictionary<int, Nativeˉsymbol> functions,
        out Decodedˉcall call)
    {
        call = new(0, 0);
        var Cursor = index;
        var Arguments = 0;
        while (Arguments < Nativeˉcontract.MAXIMUM_CALL_PARAMETERS &&
            Tryˉloadˉargument(code, Cursor, frameˉbytes, Arguments, out var Argumentˉlength))
        {
            Arguments++;
            Cursor += Argumentˉlength;
        }
        if (Cursor >= end || code[Cursor] != 0xE8 ||
            !Tryˉreadˉtarget(code, Cursor + 1, out var Target) ||
            !functions.ContainsKey(Target))
        {
            return false;
        }
        Cursor += 5;
        if (!Matches(code, Cursor,
                0x48, 0x89, 0xC2,
                0x48, 0xC1, 0xEA, 0x20,
                0x48, 0x85, 0xD2,
                0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 12, out var Propagateˉtarget) ||
            Propagateˉtarget != propagate ||
            !Tryˉstoreˉeax(code, Cursor + 16, frameˉbytes, out _))
        {
            return false;
        }
        Cursor += 23;
        index = Cursor;
        call = new(Target, Arguments);
        return true;
    }

    private static bool Tryˉdecodeˉdataˉload(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int bounds,
        Dictionary<int, Nativeˉpatch> patches,
        HashSet<int> usedˉpatches,
        Dictionary<string, Nativeˉsymbol> dataˉsymbols)
    {
        var Cursor = index;
        if (!Tryˉloadˉeax(code, Cursor, frameˉbytes, out _) ||
            !Matches(code, Cursor + 7, 0x3D) ||
            !Matches(code, Cursor + 12, 0x0F, 0x83) ||
            !Tryˉreadˉtarget(code, Cursor + 14, out var Boundsˉtarget) ||
            Boundsˉtarget != bounds ||
            !Matches(code, Cursor + 18, 0x48, 0x8D, 0x15))
        {
            return false;
        }
        var Patchˉoffset = Cursor + 21;
        if (!patches.TryGetValue(Patchˉoffset, out var Patch) ||
            !usedˉpatches.Add(Patchˉoffset) ||
            !dataˉsymbols.TryGetValue(Patch.Symbol, out var Data) ||
            (Data.Size & 3) != 0 ||
            BinaryPrimitives.ReadUInt32LittleEndian(code.Slice(Cursor + 8, sizeof(uint))) !=
                Data.Size / sizeof(int) ||
            !Matches(code, Cursor + 25, 0x8B, 0x04, 0x82) ||
            !Tryˉstoreˉeax(code, Cursor + 28, frameˉbytes, out _))
        {
            return false;
        }
        Cursor += 35;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉconsoleˉwriteˉline(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int runtimeˉservice,
        Dictionary<int, Nativeˉpatch> patches,
        HashSet<int> usedˉpatches,
        Dictionary<string, Nativeˉsymbol> dataˉsymbols)
    {
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Consoleˉwriteˉline) ||
            !Matches(code, Cursor, 0x4C, 0x8D, 0x05))
        {
            return false;
        }
        var Patchˉoffset = Cursor + 3;
        if (!patches.TryGetValue(Patchˉoffset, out var Patch) ||
            !usedˉpatches.Add(Patchˉoffset) ||
            !dataˉsymbols.TryGetValue(Patch.Symbol, out var Data) ||
            !Matches(code, Cursor + 7, 0x41, 0xB9) ||
            BinaryPrimitives.ReadUInt32LittleEndian(code.Slice(Cursor + 9, sizeof(uint))) != Data.Size ||
            !Matches(
                code,
                Cursor + 13,
                0x49, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
                0x48, 0x8B, 0x40, Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
                0xFF, 0xD0,
                0x85, 0xC0,
                0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 27, out var Runtimeˉtarget) ||
            Runtimeˉtarget != runtimeˉservice)
        {
            return false;
        }
        try
        {
            _ = STRICT_UTF8.GetCharCount(fragment.Code.AsSpan(
                checked((int)Data.Offset),
                checked((int)Data.Size)));
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
        Cursor += 31;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉslotˉtransformation(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int overflow)
    {
        if (!Tryˉloadˉeax(code, index, frameˉbytes, out _))
        {
            return false;
        }
        var Cursor = index + 7;
        if (Tryˉloadˉecx(code, Cursor, frameˉbytes, out _))
        {
            Cursor += 7;
            var Arithmetic = Matches(code, Cursor, 0x01, 0xC8) || Matches(code, Cursor, 0x29, 0xC8)
                ? 2
                : Matches(code, Cursor, 0x0F, 0xAF, 0xC1) ? 3 : 0;
            if (Arithmetic != 0)
            {
                Cursor += Arithmetic;
                if (!Matches(code, Cursor, 0x0F, 0x80) ||
                    !Tryˉreadˉtarget(code, Cursor + 2, out var Overflowˉtarget) ||
                    Overflowˉtarget != overflow ||
                    !Tryˉstoreˉeax(code, Cursor + 6, frameˉbytes, out _))
                {
                    return false;
                }
                Cursor += 13;
                index = Cursor;
                return Cursor <= end;
            }
            if (Matches(code, Cursor, 0x39, 0xC8, 0x0F) &&
                Isˉcondition(code[Cursor + 3]) &&
                Matches(code, Cursor + 4, 0xC0, 0x0F, 0xB6, 0xC0) &&
                Tryˉstoreˉeax(code, Cursor + 8, frameˉbytes, out _))
            {
                Cursor += 15;
                index = Cursor;
                return Cursor <= end;
            }
        }
        Cursor = index + 7;
        if (Matches(code, Cursor, 0xF7, 0xD8) &&
            Matches(code, Cursor + 2, 0x0F, 0x80) &&
            Tryˉreadˉtarget(code, Cursor + 4, out var Negateˉoverflow) &&
            Negateˉoverflow == overflow &&
            Tryˉstoreˉeax(code, Cursor + 8, frameˉbytes, out _))
        {
            Cursor += 15;
            index = Cursor;
            return Cursor <= end;
        }
        if (Matches(code, Cursor, 0x83, 0xF0, 0x01) &&
            Tryˉstoreˉeax(code, Cursor + 3, frameˉbytes, out _))
        {
            Cursor += 10;
            index = Cursor;
            return Cursor <= end;
        }
        if (Tryˉstoreˉeax(code, Cursor, frameˉbytes, out _))
        {
            Cursor += 7;
            index = Cursor;
            return Cursor <= end;
        }
        return false;
    }

    private static bool Matchesˉpropagate(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        bool restoreˉcontext) =>
        Matches(code, offset, 0x48, 0x81, 0xC4) &&
        Readˉi32(code, offset + 3) == frameˉbytes &&
        Matchesˉrestoreˉdepthˉandˉreturn(code, offset + 7, restoreˉcontext);

    private static bool Matchesˉstatusˉtrap(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        ulong status,
        bool restoreˉcontext) =>
        Matches(code, offset, 0x48, 0x81, 0xC4) &&
        Readˉi32(code, offset + 3) == frameˉbytes &&
        Matches(code, offset + 7, 0x49, 0xFF, 0xC2, 0x48, 0xB8) &&
        BinaryPrimitives.ReadUInt64LittleEndian(code.Slice(offset + 12, sizeof(ulong))) == status &&
        Matchesˉcontextˉrestoreˉandˉreturn(code, offset + 20, restoreˉcontext);

    private static bool Matchesˉrestoreˉdepthˉandˉreturn(
        ReadOnlySpan<byte> code,
        int offset,
        bool restoreˉcontext) =>
        Matches(code, offset, 0x49, 0xFF, 0xC2) &&
        Matchesˉcontextˉrestoreˉandˉreturn(code, offset + 3, restoreˉcontext);

    private static bool Matchesˉcontextˉrestoreˉandˉreturn(
        ReadOnlySpan<byte> code,
        int offset,
        bool restoreˉcontext) =>
        restoreˉcontext
            ? Matches(code, offset, 0x41, 0x5F, 0xC3)
            : Matches(code, offset, 0xC3);

    private static bool Tryˉloadˉeax(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
    }

    private static bool Tryˉloadˉargument(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int argument,
        out int length)
    {
        var Prefix = argument switch
        {
            0 => new byte[] { 0x44, 0x8B, 0x84, 0x24 },
            1 => new byte[] { 0x44, 0x8B, 0x8C, 0x24 },
            2 => new byte[] { 0x8B, 0x8C, 0x24 },
            3 => new byte[] { 0x8B, 0x94, 0x24 },
            _ => [],
        };
        length = Prefix.Length + sizeof(int);
        return Prefix.Length != 0 &&
            Matches(code, offset, Prefix) &&
            Tryˉreadˉslot(code, offset + Prefix.Length, frameˉbytes, out _);
    }

    private static bool Tryˉstoreˉargument(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int argument,
        out int length)
    {
        var Prefix = argument switch
        {
            0 => new byte[] { 0x44, 0x89, 0x84, 0x24 },
            1 => new byte[] { 0x44, 0x89, 0x8C, 0x24 },
            2 => new byte[] { 0x89, 0x8C, 0x24 },
            3 => new byte[] { 0x89, 0x94, 0x24 },
            _ => [],
        };
        length = Prefix.Length + sizeof(int);
        return Prefix.Length != 0 &&
            Matches(code, offset, Prefix) &&
            Tryˉreadˉslot(code, offset + Prefix.Length, frameˉbytes, out var Slot) &&
            Slot == argument;
    }

    private static bool Tryˉloadˉecx(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x8C, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
    }

    private static bool Tryˉstoreˉeax(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x89, 0x84, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
    }

    private static bool Tryˉreadˉslot(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        if (offset < 0 || offset > code.Length - sizeof(int))
        {
            return false;
        }
        var Displacement = Readˉi32(code, offset);
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
        var Target = (long)displacementˉoffset + sizeof(int) + Readˉi32(code, displacementˉoffset);
        if (Target is < int.MinValue or > int.MaxValue)
        {
            return false;
        }
        target = (int)Target;
        return true;
    }

    private static int Readˉi32(ReadOnlySpan<byte> code, int offset) =>
        BinaryPrimitives.ReadInt32LittleEndian(code.Slice(offset, sizeof(int)));

    private static bool Isˉcondition(byte condition) =>
        condition is 0x94 or 0x95 or 0x9C or 0x9D or 0x9E or 0x9F;

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

    private sealed record Decodedˉfunction(int Parameterˉcount, Decodedˉcall[] Calls);

    private readonly record struct Decodedˉcall(int Target, int Argumentˉcount);

    private sealed record Decodedˉgroup(
        int Offset,
        bool Fallsˉthrough,
        bool Returns,
        bool Isˉcharge,
        int[] Targets);

    [DoesNotReturn]
    private static void Failˉshape() =>
        Fail("WVN3030", "The x86-64 baseline fragment is outside the independently decoded context, service, call, and data target shape.");

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
