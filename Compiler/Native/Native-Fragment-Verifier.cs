using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉfragmentˉverifier
{
    private const ulong INTEGER_OVERFLOW_STATUS = 0x0000_0001_0000_0000UL;
    private const ulong INSTRUCTION_LIMIT_STATUS = 0x0000_0002_0000_0000UL;
    private const ulong CALL_DEPTH_STATUS = 0x0000_0003_0000_0000UL;
    private const ulong DATA_BOUNDS_STATUS = 0x0000_0004_0000_0000UL;
    private const ulong RUNTIME_SERVICE_STATUS = 0x0000_0005_0000_0000UL;
    private const ulong BYTE_BOUNDS_STATUS = 0x0000_0006_0000_0000UL;
    private const ulong RECORD_ARENA_STATUS = 0x0000_0007_0000_0000UL;
    private const ulong INVALID_UTF8_STATUS = 0x0000_0008_0000_0000UL;
    private const int INTERNAL_FUNCTION_SUFFIX_BYTES = 172;
    private const int MAIN_FUNCTION_SUFFIX_BYTES = 190;
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
            fragment.Types.IsDefault ||
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
        if (fragment.Requiredˉservices.Length > 11 ||
            fragment.Requiredˉservices.Any(Service => !Enum.IsDefined(Service)) ||
            fragment.Requiredˉservices.Distinct().Count() != fragment.Requiredˉservices.Length ||
            !fragment.Requiredˉservices.SequenceEqual(fragment.Requiredˉservices.Order()))
        {
            Fail("WVN3009", "The native fragment requires unsupported or noncanonical runtime services.");
        }
        if (!Isˉvalidˉnominalˉmetadata(fragment.Types))
        {
            Fail("WVN3009", "The native fragment contains invalid nominal metadata.");
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

    private static bool Isˉvalidˉnominalˉmetadata(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        for (var Typeˉindex = 0; Typeˉindex < types.Length; Typeˉindex++)
        {
            var Type = types[Typeˉindex];
            if (Type is null || !Seedˉnames.Isˉidentifier(Type.Name))
            {
                return false;
            }
            if (Type is Enumˉtypeˉdeclaration Enum)
            {
                if (Enum.Members.IsDefaultOrEmpty ||
                    Enum.Members[0].Value != 0 ||
                    Enum.Members.Any(Member =>
                        Member is null || !Seedˉnames.Isˉidentifier(Member.Name)) ||
                    Enum.Members.Select(Member => Member.Name).Distinct(StringComparer.Ordinal).Count() !=
                        Enum.Members.Length ||
                    Enum.Members.Select(Member => Member.Value).Distinct().Count() != Enum.Members.Length)
                {
                    return false;
                }
                continue;
            }
            if (Type is not Recordˉtypeˉdeclaration Record ||
                Record.Fields.IsDefaultOrEmpty ||
                Record.Fields.Any(Field =>
                    Field is null ||
                    !Seedˉnames.Isˉidentifier(Field.Name) ||
                    Field.Type.Kind is not (
                        Valueˉtype.I32 or
                        Valueˉtype.Bool or
                        Valueˉtype.U8 or
                        Valueˉtype.U32 or
                        Valueˉtype.Record or
                        Valueˉtype.Enum) ||
                    (Field.Type.Kind is Valueˉtype.Record or Valueˉtype.Enum
                        ? (uint)Field.Type.Nominalˉtypeˉindex >= (uint)types.Length
                        : Field.Type.Nominalˉtypeˉindex != -1)) ||
                Record.Fields.Select(Field => Field.Name).Distinct(StringComparer.Ordinal).Count() !=
                    Record.Fields.Length)
            {
                return false;
            }
        }
        return types.Select(Type => Type.Name).Distinct(StringComparer.Ordinal).Count() == types.Length;
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
                    !Call.Argumentˉkinds.SequenceEqual(Callee.Parameterˉkinds) ||
                    Call.Returnˉkind != Callee.Returnˉkind)
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
        if (Frameˉbytes is < 0 or > Nativeˉcontract.MAXIMUM_FRAME_BYTES ||
            (Frameˉbytes & 15) != 0)
        {
            Failˉshape();
        }
        Index += 7;
        var Hasˉhiddenˉresult = Tryˉstoreˉrax(Code, Index, Frameˉbytes, out var Hiddenˉresultˉslot) &&
            Hiddenˉresultˉslot == Frameˉbytes / Nativeˉcontract.VALUE_SLOT_BYTES - 1;
        if (Hasˉhiddenˉresult)
        {
            Index += 8;
        }
        if (!Matches(Code, Index, 0x31, 0xC0))
        {
            Failˉshape();
        }
        Index += 2;
        var Initializedˉbytes = Frameˉbytes - (Hasˉhiddenˉresult ? Nativeˉcontract.VALUE_SLOT_BYTES : 0);
        for (var Dword = 0; Dword < Initializedˉbytes / sizeof(int); Dword++)
        {
            if (!Tryˉstoreˉeaxˉdword(Code, Index, Frameˉbytes, out var Initialized) ||
                Initialized != Dword)
            {
                Failˉshape();
            }
            Index += 7;
        }

        var Parameterˉkinds = new List<Decodedˉargumentˉkind>();
        while (Parameterˉkinds.Count < Nativeˉcontract.MAXIMUM_CALL_PARAMETERS &&
            Tryˉstoreˉargument(
                Code,
                Index,
                Frameˉbytes,
                Parameterˉkinds.Count,
                out var Parameterˉlength,
                out var Parameterˉkind))
        {
            Parameterˉkinds.Add(Parameterˉkind);
            Index += Parameterˉlength;
        }
        if (Isˉmain && Parameterˉkinds.Count != 0)
        {
            Failˉshape();
        }
        var Borrowedˉbytesˉslots = Parameterˉkinds
            .Select((Kind, Slot) => (Kind, Slot))
            .Where(Item => Item.Kind == Decodedˉargumentˉkind.Borrowedˉbytes)
            .Select(Item => Item.Slot)
            .ToHashSet();
        var Staticˉdescriptorˉdata = new Dictionary<int, Nativeˉsymbol>();

        var Restoreˉbytes = Isˉmain ? 13 : 11;
        var Statusˉbytes = Isˉmain ? 23 : 21;
        var Propagate = End - Suffixˉbytes;
        var Overflow = Propagate + Restoreˉbytes;
        var Instructionˉlimit = Overflow + Statusˉbytes;
        var Bounds = Instructionˉlimit + Statusˉbytes;
        var Byteˉbounds = Bounds + Statusˉbytes;
        var Runtimeˉservice = Byteˉbounds + Statusˉbytes;
        var Invalidˉutf8 = Runtimeˉservice + Statusˉbytes;
        var Recordˉarena = Invalidˉutf8 + Statusˉbytes;
        var Depth = Recordˉarena + Statusˉbytes;
        if (Depthˉtarget != Depth ||
            !Matchesˉpropagate(Code, Propagate, Frameˉbytes, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Overflow, Frameˉbytes, INTEGER_OVERFLOW_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Instructionˉlimit, Frameˉbytes, INSTRUCTION_LIMIT_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Bounds, Frameˉbytes, DATA_BOUNDS_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Byteˉbounds, Frameˉbytes, BYTE_BOUNDS_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Runtimeˉservice, Frameˉbytes, RUNTIME_SERVICE_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Invalidˉutf8, Frameˉbytes, INVALID_UTF8_STATUS, Isˉmain) ||
            !Matchesˉstatusˉtrap(Code, Recordˉarena, Frameˉbytes, RECORD_ARENA_STATUS, Isˉmain) ||
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
        Decodedˉreturnˉkind? Returnˉkind = null;
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

            if (Hasˉhiddenˉresult &&
                Tryˉloadˉrdx(Code, Index, Frameˉbytes, out var Hiddenˉreturnˉslot) &&
                Hiddenˉreturnˉslot == Hiddenˉresultˉslot &&
                Tryˉloadˉrax(Code, Index + 8, Frameˉbytes, out var Descriptorˉreturnˉslot) &&
                Borrowedˉbytesˉslots.Contains(Descriptorˉreturnˉslot) &&
                Matches(Code, Index + 16, 0x48, 0x89, 0x02) &&
                Tryˉloadˉraxˉatˉfield(
                    Code,
                    Index + 19,
                    Frameˉbytes,
                    sizeof(ulong),
                    out var Descriptorˉreturnˉfieldˉslot) &&
                Descriptorˉreturnˉfieldˉslot == Descriptorˉreturnˉslot &&
                Matches(Code, Index + 27, 0x48, 0x89, 0x42, 0x08, 0x31, 0xC0) &&
                Matches(Code, Index + 33, 0x48, 0x81, 0xC4) &&
                Readˉi32(Code, Index + 36) == Frameˉbytes &&
                Matchesˉrestoreˉdepthˉandˉreturn(Code, Index + 40, Isˉmain))
            {
                Index += Isˉmain ? 46 : 44;
                Returnˉkind = Mergeˉreturnˉkind(Returnˉkind, Decodedˉreturnˉkind.Descriptor);
                Returns++;
                Groups.Add(new(Groupˉstart, false, true, false, []));
                continue;
            }

            if (!Hasˉhiddenˉresult && Tryˉloadˉeax(Code, Index, Frameˉbytes, out _) &&
                Matches(Code, Index + 7, 0x48, 0x81, 0xC4) &&
                Readˉi32(Code, Index + 10) == Frameˉbytes &&
                Matchesˉrestoreˉdepthˉandˉreturn(Code, Index + 14, Isˉmain))
            {
                Index += Isˉmain ? 20 : 18;
                Returnˉkind = Mergeˉreturnˉkind(Returnˉkind, Decodedˉreturnˉkind.Scalar);
                Returns++;
                Groups.Add(new(Groupˉstart, false, true, false, []));
                continue;
            }

            if (!Hasˉhiddenˉresult &&
                Matches(Code, Index, 0x31, 0xC0, 0x48, 0x81, 0xC4) &&
                Readˉi32(Code, Index + 5) == Frameˉbytes &&
                Matchesˉrestoreˉdepthˉandˉreturn(Code, Index + 9, Isˉmain))
            {
                Index += Isˉmain ? 15 : 13;
                Returnˉkind = Mergeˉreturnˉkind(Returnˉkind, Decodedˉreturnˉkind.Void);
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
                if (Call.Returnˉkind == Decodedˉreturnˉkind.Descriptor)
                {
                    if (!Borrowedˉbytesˉslots.Add(Call.Resultˉslot))
                    {
                        Failˉshape();
                    }
                }
                else if (Call.Returnˉkind == Decodedˉreturnˉkind.Scalar &&
                    Borrowedˉbytesˉslots.Contains(Call.Resultˉslot))
                {
                    Failˉshape();
                }
                for (var Argument = 0; Argument < Call.Argumentˉkinds.Length; Argument++)
                {
                    if (Call.Argumentˉkinds[Argument] == Decodedˉargumentˉkind.Borrowedˉbytes &&
                        !Borrowedˉbytesˉslots.Contains(Call.Argumentˉslots[Argument]))
                    {
                        Failˉshape();
                    }
                }
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
                dataˉsymbols,
                out var Dataˉresult))
            {
                if (Borrowedˉbytesˉslots.Contains(Dataˉresult))
                {
                    Failˉshape();
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉconsoleˉwriteˉline(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Borrowedˉbytesˉslots,
                Staticˉdescriptorˉdata))
            {
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉdiagnosticˉwriteˉline(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Borrowedˉbytesˉslots,
                Staticˉdescriptorˉdata))
            {
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉprocessˉargumentˉcount(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                out var Argumentˉcountˉslot))
            {
                if (Borrowedˉbytesˉslots.Contains(Argumentˉcountˉslot))
                {
                    Failˉshape();
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉprocessˉargument(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Borrowedˉbytesˉslots,
                out var Argumentˉslot))
            {
                Borrowedˉbytesˉslots.Add(Argumentˉslot);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉtextˉfromˉutf8(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Invalidˉutf8,
                Borrowedˉbytesˉslots,
                out var Decodedˉtextˉslot))
            {
                Borrowedˉbytesˉslots.Add(Decodedˉtextˉslot);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉtextˉutf8ˉisˉvalid(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Borrowedˉbytesˉslots,
                out var Utf8ˉresultˉslot))
            {
                if (Borrowedˉbytesˉslots.Contains(Utf8ˉresultˉslot))
                {
                    Failˉshape();
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉenumˉname(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Borrowedˉbytesˉslots,
                out var Enumˉnameˉslot) ||
                Tryˉdecodeˉintegerˉformat(
                    fragment,
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out Enumˉnameˉslot) ||
                Tryˉdecodeˉtextˉconcat(
                    fragment,
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out Enumˉnameˉslot) ||
                Tryˉdecodeˉtextˉquote(
                    fragment,
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out Enumˉnameˉslot))
            {
                Borrowedˉbytesˉslots.Add(Enumˉnameˉslot);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉfileˉreadˉbytes(
                fragment,
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Runtimeˉservice,
                Borrowedˉbytesˉslots,
                Staticˉdescriptorˉdata,
                out var Fileˉslot))
            {
                Borrowedˉbytesˉslots.Add(Fileˉslot);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉstaticˉbytes(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                patches,
                usedˉpatches,
                dataˉsymbols,
                out var Staticˉbytesˉslot,
                out var Staticˉdata))
            {
                Borrowedˉbytesˉslots.Add(Staticˉbytesˉslot);
                Staticˉdescriptorˉdata.Add(Staticˉbytesˉslot, Staticˉdata);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉbytesˉcopy(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Borrowedˉbytesˉslots,
                out var Copiedˉbytesˉslot,
                out var Copiedˉbytesˉsource))
            {
                Borrowedˉbytesˉslots.Add(Copiedˉbytesˉslot);
                if (Staticˉdescriptorˉdata.TryGetValue(Copiedˉbytesˉsource, out var Copiedˉdata))
                {
                    Staticˉdescriptorˉdata[Copiedˉbytesˉslot] = Copiedˉdata;
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉbytesˉlength(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Borrowedˉbytesˉslots,
                out var Bytesˉlengthˉresult))
            {
                if (Borrowedˉbytesˉslots.Contains(Bytesˉlengthˉresult))
                {
                    Failˉshape();
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉbytesˉslice(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Byteˉbounds,
                Borrowedˉbytesˉslots,
                out var Sliceˉslot))
            {
                Borrowedˉbytesˉslots.Add(Sliceˉslot);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉbytesˉread(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Byteˉbounds,
                Borrowedˉbytesˉslots,
                out var Bytesˉreadˉresult))
            {
                if (Borrowedˉbytesˉslots.Contains(Bytesˉreadˉresult))
                {
                    Failˉshape();
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉrecordˉcreate(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Recordˉarena,
                out var Recordˉcreateˉresult))
            {
                if (Borrowedˉbytesˉslots.Contains(Recordˉcreateˉresult))
                {
                    Failˉshape();
                }
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉrecordˉfield(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Recordˉarena,
                out var Recordˉfieldˉresult))
            {
                if (Borrowedˉbytesˉslots.Contains(Recordˉfieldˉresult))
                {
                    Failˉshape();
                }
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
                Tryˉstoreˉeax(Code, Index + 5, Frameˉbytes, out var Constantˉresult))
            {
                if (Borrowedˉbytesˉslots.Contains(Constantˉresult))
                {
                    Failˉshape();
                }
                Index += 12;
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }
            if (!Tryˉdecodeˉslotˉtransformation(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                Overflow,
                out var Scalarˉresult))
            {
                Failˉshape();
            }
            if (Borrowedˉbytesˉslots.Contains(Scalarˉresult))
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
        return new(
            Parameterˉkinds.ToArray(),
            Returnˉkind ?? throw new Nativeˉbackendˉexception("WVN3030", "Native function return shape is missing."),
            Calls.ToArray());
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
        call = new(0, [], [], Decodedˉreturnˉkind.Void, -1);
        var Cursor = index;
        var Argumentˉkinds = new List<Decodedˉargumentˉkind>();
        var Argumentˉslots = new List<int>();
        while (Argumentˉkinds.Count < Nativeˉcontract.MAXIMUM_CALL_PARAMETERS &&
            Tryˉloadˉargument(
                code,
                Cursor,
                frameˉbytes,
                Argumentˉkinds.Count,
                out var Argumentˉlength,
                out var Argumentˉkind,
                out var Argumentˉslot))
        {
            Argumentˉkinds.Add(Argumentˉkind);
            Argumentˉslots.Add(Argumentˉslot);
            Cursor += Argumentˉlength;
        }
        var Returnˉkind = Decodedˉreturnˉkind.Void;
        var Resultˉslot = -1;
        if (Matches(code, Cursor, 0x48, 0x8D, 0x84, 0x24) &&
            Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out Resultˉslot))
        {
            Returnˉkind = Decodedˉreturnˉkind.Descriptor;
            Cursor += 8;
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
            Propagateˉtarget != propagate)
        {
            return false;
        }
        Cursor += 16;
        if (Returnˉkind != Decodedˉreturnˉkind.Descriptor &&
            Tryˉstoreˉeax(code, Cursor, frameˉbytes, out Resultˉslot))
        {
            Returnˉkind = Decodedˉreturnˉkind.Scalar;
            Cursor += 7;
        }
        index = Cursor;
        call = new(Target, Argumentˉkinds.ToArray(), Argumentˉslots.ToArray(), Returnˉkind, Resultˉslot);
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
        Dictionary<string, Nativeˉsymbol> dataˉsymbols,
        out int resultˉslot)
    {
        resultˉslot = 0;
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
            !Tryˉstoreˉeax(code, Cursor + 28, frameˉbytes, out resultˉslot))
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
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        Dictionary<int, Nativeˉsymbol> staticˉdescriptorˉdata)
    {
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Consoleˉwriteˉline) ||
            !Tryˉdecodeˉdescriptorˉserviceˉinput(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out var Textˉslot) ||
            !Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        if (staticˉdescriptorˉdata.TryGetValue(Textˉslot, out var Textˉdata) &&
            !Isˉvalidˉutf8(fragment.Code, Textˉdata))
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉdiagnosticˉwriteˉline(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        Dictionary<int, Nativeˉsymbol> staticˉdescriptorˉdata)
    {
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Diagnosticˉwriteˉline) ||
            !Tryˉdecodeˉdescriptorˉserviceˉinput(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out var Textˉslot) ||
            !Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.DIAGNOSTIC_WRITE_LINE_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        if (staticˉdescriptorˉdata.TryGetValue(Textˉslot, out var Textˉdata) &&
            !Isˉvalidˉutf8(fragment.Code, Textˉdata))
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉprocessˉargumentˉcount(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Processˉargumentˉcount) ||
            !Tryˉdecodeˉserviceˉpointer(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_COUNT_POINTER_OFFSET) ||
            !Matches(code, Cursor, 0xFF, 0xD0) ||
            !Tryˉstoreˉeax(code, Cursor + 2, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        Cursor += 9;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉprocessˉargument(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Processˉargument) ||
            !Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Indexˉslot) ||
            borrowedˉdescriptorˉslots.Contains(Indexˉslot) ||
            !Matches(code, Cursor + 7, 0x41, 0x89, 0xC0) ||
            !Matches(code, Cursor + 10, 0x4C, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 14, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        Cursor += 18;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉfileˉreadˉbytes(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        Dictionary<int, Nativeˉsymbol> staticˉdescriptorˉdata,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Fileˉreadˉbytes) ||
            !Tryˉdecodeˉdescriptorˉserviceˉinput(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out var Nameˉslot) ||
            !Matches(code, Cursor, 0x48, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        if (staticˉdescriptorˉdata.TryGetValue(Nameˉslot, out var Nameˉdata) &&
            !Isˉvalidˉutf8(fragment.Code, Nameˉdata))
        {
            return false;
        }
        Cursor += 8;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉtextˉutf8ˉisˉvalid(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Textˉutf8ˉisˉvalid) ||
            !Tryˉdecodeˉdescriptorˉserviceˉinput(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out _) ||
            !Matches(code, Cursor, 0x48, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        Cursor += 8;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.TEXT_UTF8_IS_VALID_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉtextˉfromˉutf8(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        int invalidˉutf8,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Textˉutf8ˉisˉvalid) ||
            !Tryˉdecodeˉdescriptorˉserviceˉinput(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out var Sourceˉslot) ||
            !Matches(code, Cursor, 0x48, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        Cursor += 8;
        if (borrowedˉdescriptorˉslots.Contains(resultˉslot) ||
            !Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.TEXT_UTF8_IS_VALID_POINTER_OFFSET,
                runtimeˉservice) ||
            !Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Validationˉslot) ||
            Validationˉslot != resultˉslot ||
            !Matches(code, Cursor + 7, 0x85, 0xC0, 0x0F, 0x84) ||
            !Tryˉreadˉtarget(code, Cursor + 11, out var Invalidˉtarget) ||
            Invalidˉtarget != invalidˉutf8)
        {
            return false;
        }
        Cursor += 15;
        var Copyˉcursor = Cursor;
        if (!Tryˉdecodeˉbytesˉcopy(
                code,
                ref Copyˉcursor,
                end,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out var Copyˉtarget,
                out var Copyˉsource) ||
            Copyˉtarget != resultˉslot ||
            Copyˉsource != Sourceˉslot)
        {
            return false;
        }
        index = Copyˉcursor;
        return true;
    }

    private static bool Tryˉdecodeˉenumˉname(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Enumˉname) ||
            !Matches(code, Cursor, 0x41, 0xB8))
        {
            return false;
        }
        var Type = Readˉi32(code, Cursor + 2);
        if ((uint)Type >= (uint)fragment.Types.Length ||
            fragment.Types[Type] is not Enumˉtypeˉdeclaration ||
            !Tryˉloadˉeax(code, Cursor + 6, frameˉbytes, out var Valueˉslot) ||
            borrowedˉdescriptorˉslots.Contains(Valueˉslot) ||
            !Matches(code, Cursor + 13, 0x41, 0x89, 0xC1) ||
            !Matches(code, Cursor + 16, 0x48, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 20, frameˉbytes, out resultˉslot) ||
            borrowedˉdescriptorˉslots.Contains(resultˉslot))
        {
            return false;
        }
        Cursor += 24;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.ENUM_NAME_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉintegerˉformat(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Valueˉslot) ||
            borrowedˉdescriptorˉslots.Contains(Valueˉslot) ||
            !Matches(code, Cursor + 7, 0x41, 0x89, 0xC0) ||
            !Matches(code, Cursor + 10, 0x4C, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 14, frameˉbytes, out resultˉslot) ||
            borrowedˉdescriptorˉslots.Contains(resultˉslot))
        {
            return false;
        }
        Cursor += 18;
        var Isˉi32 = fragment.Requiredˉservices.Contains(Nativeˉservice.I32ˉformat) &&
            Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.I32_FORMAT_POINTER_OFFSET,
                runtimeˉservice);
        if (!Isˉi32)
        {
            Cursor = index + 18;
            if (!fragment.Requiredˉservices.Contains(Nativeˉservice.U32ˉformat) ||
                !Tryˉdecodeˉserviceˉcall(
                    code,
                    ref Cursor,
                    Nativeˉserviceˉtableˉcontract.U32_FORMAT_POINTER_OFFSET,
                    runtimeˉservice))
            {
                return false;
            }
        }
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉtextˉconcat(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Textˉconcat) ||
            !Tryˉdecodeˉdescriptorˉaddress(code, Cursor, frameˉbytes, 0x84, borrowedˉdescriptorˉslots, out _) ||
            !Tryˉdecodeˉdescriptorˉaddress(code, Cursor + 8, frameˉbytes, 0x8C, borrowedˉdescriptorˉslots, out _) ||
            !Matches(code, Cursor + 16, 0x48, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 20, frameˉbytes, out resultˉslot) ||
            borrowedˉdescriptorˉslots.Contains(resultˉslot))
        {
            return false;
        }
        Cursor += 24;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.TEXT_CONCAT_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉtextˉquote(
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Textˉquote) ||
            !Tryˉdecodeˉdescriptorˉaddress(code, Cursor, frameˉbytes, 0x84, borrowedˉdescriptorˉslots, out _) ||
            !Matches(code, Cursor + 8, 0x4C, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 12, frameˉbytes, out resultˉslot) ||
            borrowedˉdescriptorˉslots.Contains(resultˉslot))
        {
            return false;
        }
        Cursor += 16;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.TEXT_QUOTE_POINTER_OFFSET,
                runtimeˉservice) ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉdescriptorˉaddress(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        byte registerˉmodrm,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x4C, 0x8D, registerˉmodrm, 0x24) &&
            Tryˉreadˉslot(code, offset + 4, frameˉbytes, out slot) &&
            borrowedˉdescriptorˉslots.Contains(slot);
    }

    private static bool Tryˉdecodeˉdescriptorˉserviceˉinput(
        ReadOnlySpan<byte> code,
        ref int index,
        int frameˉbytes,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int descriptorˉslot)
    {
        descriptorˉslot = 0;
        var Cursor = index;
        if (!Matches(code, Cursor, 0x4C, 0x8B, 0x84, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out descriptorˉslot) ||
            !borrowedˉdescriptorˉslots.Contains(descriptorˉslot) ||
            !Matches(code, Cursor + 8, 0x44, 0x8B, 0x8C, 0x24) ||
            !Tryˉreadˉslotˉfield(
                code,
                Cursor + 12,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                sizeof(int),
                out var Lengthˉslot) ||
            Lengthˉslot != descriptorˉslot)
        {
            return false;
        }
        index = Cursor + 16;
        return true;
    }

    private static bool Isˉvalidˉutf8(ImmutableArray<byte> code, Nativeˉsymbol data)
    {
        try
        {
            _ = STRICT_UTF8.GetCharCount(code.AsSpan(
                checked((int)data.Offset),
                checked((int)data.Size)));
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static bool Tryˉdecodeˉserviceˉcall(
        ReadOnlySpan<byte> code,
        ref int index,
        int pointerˉoffset,
        int runtimeˉservice)
    {
        var Cursor = index;
        if (!Tryˉdecodeˉserviceˉpointer(code, ref Cursor, pointerˉoffset) ||
            !Matches(code, Cursor, 0xFF, 0xD0, 0x85, 0xC0, 0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 6, out var Runtimeˉtarget) ||
            Runtimeˉtarget != runtimeˉservice)
        {
            return false;
        }
        index = Cursor + 10;
        return true;
    }

    private static bool Tryˉdecodeˉserviceˉpointer(
        ReadOnlySpan<byte> code,
        ref int index,
        int pointerˉoffset)
    {
        if (!Matches(
            code,
            index,
            0x49, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
            0x48, 0x8B, 0x40, checked((byte)pointerˉoffset)))
        {
            return false;
        }
        index += 8;
        return true;
    }

    private static bool Tryˉdecodeˉstaticˉbytes(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        Dictionary<int, Nativeˉpatch> patches,
        HashSet<int> usedˉpatches,
        Dictionary<string, Nativeˉsymbol> dataˉsymbols,
        out int resultˉslot,
        [NotNullWhen(true)] out Nativeˉsymbol? data)
    {
        resultˉslot = 0;
        data = null;
        var Cursor = index;
        if (!Matches(code, Cursor, 0x48, 0x8D, 0x05))
        {
            return false;
        }
        var Patchˉoffset = Cursor + 3;
        if (!patches.TryGetValue(Patchˉoffset, out var Patch) ||
            !usedˉpatches.Add(Patchˉoffset) ||
            !dataˉsymbols.TryGetValue(Patch.Symbol, out data) ||
            !Tryˉstoreˉrax(code, Cursor + 7, frameˉbytes, out resultˉslot) ||
            code[Cursor + 15] != 0xB8 ||
            BinaryPrimitives.ReadUInt32LittleEndian(code.Slice(Cursor + 16, sizeof(uint))) != data.Size ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 20,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉslot) ||
            Lengthˉslot != resultˉslot)
        {
            return false;
        }
        Cursor += 27;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉcopy(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        HashSet<int> borrowedˉbytesˉslots,
        out int targetˉslot,
        out int sourceˉslot)
    {
        targetˉslot = 0;
        sourceˉslot = 0;
        var Cursor = index;
        if (!Tryˉloadˉrax(code, Cursor, frameˉbytes, out sourceˉslot) ||
            !borrowedˉbytesˉslots.Contains(sourceˉslot) ||
            !Tryˉstoreˉrax(code, Cursor + 8, frameˉbytes, out targetˉslot) ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 16,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉsource) ||
            Lengthˉsource != sourceˉslot ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 23,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉtarget) ||
            Lengthˉtarget != targetˉslot)
        {
            return false;
        }
        Cursor += 30;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉlength(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        HashSet<int> borrowedˉbytesˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Sourceˉslot) ||
            !borrowedˉbytesˉslots.Contains(Sourceˉslot) ||
            !Tryˉstoreˉeax(code, Cursor + 7, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        Cursor += 14;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉslice(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int bounds,
        HashSet<int> borrowedˉbytesˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Offsetˉslot) ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 7,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Bytesˉslot) ||
            !borrowedˉbytesˉslots.Contains(Bytesˉslot) ||
            !Matches(code, Cursor + 14, 0x39, 0xC8, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 18, out var Firstˉbounds) ||
            Firstˉbounds != bounds ||
            !Matches(code, Cursor + 22, 0x29, 0xC1) ||
            !Tryˉloadˉedx(code, Cursor + 24, frameˉbytes, out var Lengthˉslot) ||
            !Matches(code, Cursor + 31, 0x39, 0xCA, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 35, out var Secondˉbounds) ||
            Secondˉbounds != bounds ||
            !Tryˉloadˉrax(code, Cursor + 39, frameˉbytes, out var Pointerˉslot) ||
            Pointerˉslot != Bytesˉslot ||
            !Tryˉloadˉecx(code, Cursor + 47, frameˉbytes, out var Addedˉoffset) ||
            Addedˉoffset != Offsetˉslot ||
            !Matches(code, Cursor + 54, 0x48, 0x01, 0xC8) ||
            !Tryˉstoreˉrax(code, Cursor + 57, frameˉbytes, out resultˉslot) ||
            !Tryˉloadˉeax(code, Cursor + 65, frameˉbytes, out var Storedˉlength) ||
            Storedˉlength != Lengthˉslot ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 72,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉresult) ||
            Lengthˉresult != resultˉslot)
        {
            return false;
        }
        Cursor += 79;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉread(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int bounds,
        HashSet<int> borrowedˉbytesˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Offsetˉslot))
        {
            return false;
        }
        Cursor += 7;
        if (!Tryˉloadˉecxˉatˉfield(
                code,
                Cursor,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Bytesˉslot) ||
            !borrowedˉbytesˉslots.Contains(Bytesˉslot))
        {
            return false;
        }
        Cursor += 7;
        if (!Matches(code, Cursor, 0x39, 0xC8, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 4, out var Firstˉbounds) ||
            Firstˉbounds != bounds)
        {
            return false;
        }
        Cursor += 8;
        if (!Matches(code, Cursor, 0x29, 0xC1, 0x83, 0xF9) ||
            code[Cursor + 4] is not (1 or 2 or 4) ||
            !Matches(code, Cursor + 5, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 7, out var Secondˉbounds) ||
            Secondˉbounds != bounds)
        {
            return false;
        }
        var Width = code[Cursor + 4];
        Cursor += 11;
        if (!Tryˉloadˉrdx(code, Cursor, frameˉbytes, out var Pointerˉslot) ||
            Pointerˉslot != Bytesˉslot)
        {
            return false;
        }
        Cursor += 8;
        if (!Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Addedˉoffset) ||
            Addedˉoffset != Offsetˉslot ||
            !Matches(code, Cursor + 7, 0x48, 0x01, 0xC2))
        {
            return false;
        }
        Cursor += 10;
        var Readˉlength = Width switch
        {
            1 when Matches(code, Cursor, 0x0F, 0xB6, 0x02) => 3,
            2 when Matches(code, Cursor, 0x0F, 0xB7, 0x02) => 3,
            4 when Matches(code, Cursor, 0x8B, 0x02) => 2,
            _ => 0,
        };
        if (Readˉlength == 0)
        {
            return false;
        }
        Cursor += Readˉlength;
        if (!Tryˉstoreˉeax(code, Cursor, frameˉbytes, out resultˉslot))
        {
            return false;
        }
        Cursor += 7;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉrecordˉcreate(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int arenaˉtrap,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!Matches(
                code,
                Cursor,
                0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
                0x89, 0xC1,
                0x81, 0xC1))
        {
            return false;
        }
        var Allocationˉbytes = Readˉi32(code, Cursor + 8);
        if (Allocationˉbytes is < Nativeˉcontract.VALUE_SLOT_BYTES or
                > Bytecodeˉlimits.MAX_RECORD_FIELDS * Nativeˉcontract.VALUE_SLOT_BYTES ||
            Allocationˉbytes % Nativeˉcontract.VALUE_SLOT_BYTES != 0 ||
            !Matches(code, Cursor + 12, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 14, out var Overflowˉtarget) ||
            Overflowˉtarget != arenaˉtrap ||
            !Matches(
                code,
                Cursor + 18,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 24, out var Capacityˉtarget) ||
            Capacityˉtarget != arenaˉtrap ||
            !Matches(
                code,
                Cursor + 28,
                0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET) ||
            !Tryˉstoreˉeax(code, Cursor + 32, frameˉbytes, out resultˉslot) ||
            !Matches(
                code,
                Cursor + 39,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET,
                0x48, 0x01, 0xC2))
        {
            return false;
        }

        Cursor += 46;
        var Fieldˉcount = Allocationˉbytes / Nativeˉcontract.VALUE_SLOT_BYTES;
        for (var Field = 0; Field < Fieldˉcount; Field++)
        {
            var Fieldˉoffset = Field * Nativeˉcontract.VALUE_SLOT_BYTES;
            if (!Tryˉloadˉrax(code, Cursor, frameˉbytes, out var Sourceˉslot) ||
                !Matches(code, Cursor + 8, 0x48, 0x89, 0x82) ||
                Readˉi32(code, Cursor + 11) != Fieldˉoffset ||
                !Tryˉloadˉraxˉatˉfield(
                    code,
                    Cursor + 15,
                    frameˉbytes,
                    sizeof(ulong),
                    out var Highˉsource) ||
                Highˉsource != Sourceˉslot ||
                !Matches(code, Cursor + 23, 0x48, 0x89, 0x82) ||
                Readˉi32(code, Cursor + 26) != Fieldˉoffset + sizeof(ulong))
            {
                return false;
            }
            Cursor += 30;
        }
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉrecordˉfield(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int arenaˉtrap,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (!Tryˉloadˉeax(code, Cursor, frameˉbytes, out _) ||
            !Matches(code, Cursor + 7, 0x89, 0xC1, 0x81, 0xC1))
        {
            return false;
        }
        var Endˉoffset = Readˉi32(code, Cursor + 11);
        if (Endˉoffset is < Nativeˉcontract.VALUE_SLOT_BYTES or
                > Bytecodeˉlimits.MAX_RECORD_FIELDS * Nativeˉcontract.VALUE_SLOT_BYTES ||
            Endˉoffset % Nativeˉcontract.VALUE_SLOT_BYTES != 0 ||
            !Matches(code, Cursor + 15, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 17, out var Overflowˉtarget) ||
            Overflowˉtarget != arenaˉtrap ||
            !Matches(
                code,
                Cursor + 21,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 27, out var Boundsˉtarget) ||
            Boundsˉtarget != arenaˉtrap ||
            !Matches(
                code,
                Cursor + 31,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET,
                0x48, 0x01, 0xC2,
                0x48, 0x8B, 0x82) ||
            Readˉi32(code, Cursor + 41) != Endˉoffset - Nativeˉcontract.VALUE_SLOT_BYTES ||
            !Tryˉstoreˉrax(code, Cursor + 45, frameˉbytes, out resultˉslot) ||
            !Matches(code, Cursor + 53, 0x48, 0x8B, 0x82) ||
            Readˉi32(code, Cursor + 56) != Endˉoffset - sizeof(ulong) ||
            !Tryˉstoreˉraxˉatˉfield(
                code,
                Cursor + 60,
                frameˉbytes,
                sizeof(ulong),
                out var Highˉresult) ||
            Highˉresult != resultˉslot)
        {
            return false;
        }
        Cursor += 68;
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
        int overflow,
        out int resultˉslot)
    {
        resultˉslot = 0;
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
                if (!(Matches(code, Cursor, 0x0F, 0x80) ||
                        Matches(code, Cursor, 0x0F, 0x82)) ||
                    !Tryˉreadˉtarget(code, Cursor + 2, out var Overflowˉtarget) ||
                    Overflowˉtarget != overflow ||
                    !Tryˉstoreˉeax(code, Cursor + 6, frameˉbytes, out resultˉslot))
                {
                    return false;
                }
                Cursor += 13;
                index = Cursor;
                return Cursor <= end;
            }
            if (Matches(code, Cursor, 0xF7, 0xE1, 0x85, 0xD2, 0x0F, 0x85) &&
                Tryˉreadˉtarget(code, Cursor + 6, out var Multiplyˉoverflow) &&
                Multiplyˉoverflow == overflow &&
                Tryˉstoreˉeax(code, Cursor + 10, frameˉbytes, out resultˉslot))
            {
                Cursor += 17;
                index = Cursor;
                return Cursor <= end;
            }
            if (Matches(code, Cursor, 0x39, 0xC8, 0x0F) &&
                Isˉcondition(code[Cursor + 3]) &&
                Matches(code, Cursor + 4, 0xC0, 0x0F, 0xB6, 0xC0) &&
                Tryˉstoreˉeax(code, Cursor + 8, frameˉbytes, out resultˉslot))
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
            Tryˉstoreˉeax(code, Cursor + 8, frameˉbytes, out resultˉslot))
        {
            Cursor += 15;
            index = Cursor;
            return Cursor <= end;
        }
        if (Matches(code, Cursor, 0x83, 0xF0, 0x01) &&
            Tryˉstoreˉeax(code, Cursor + 3, frameˉbytes, out resultˉslot))
        {
            Cursor += 10;
            index = Cursor;
            return Cursor <= end;
        }
        if (Tryˉstoreˉeax(code, Cursor, frameˉbytes, out resultˉslot))
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

    private static bool Tryˉloadˉeaxˉatˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int field,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉslotˉfield(code, offset + 3, frameˉbytes, field, sizeof(int), out slot);
    }

    private static bool Tryˉloadˉecxˉatˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int field,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x8C, 0x24) &&
            Tryˉreadˉslotˉfield(code, offset + 3, frameˉbytes, field, sizeof(int), out slot);
    }

    private static bool Tryˉloadˉedx(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x8B, 0x94, 0x24) &&
            Tryˉreadˉslot(code, offset + 3, frameˉbytes, out slot);
    }

    private static bool Tryˉloadˉrax(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x48, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉslot(code, offset + 4, frameˉbytes, out slot);
    }

    private static bool Tryˉloadˉraxˉatˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int field,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x48, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉslotˉfield(code, offset + 4, frameˉbytes, field, sizeof(ulong), out slot);
    }

    private static bool Tryˉloadˉrdx(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x48, 0x8B, 0x94, 0x24) &&
            Tryˉreadˉslot(code, offset + 4, frameˉbytes, out slot);
    }

    private static bool Tryˉloadˉargument(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int argument,
        out int length,
        out Decodedˉargumentˉkind kind,
        out int slot)
    {
        var Scalarˉprefix = argument switch
        {
            0 => new byte[] { 0x44, 0x8B, 0x84, 0x24 },
            1 => new byte[] { 0x44, 0x8B, 0x8C, 0x24 },
            2 => new byte[] { 0x8B, 0x8C, 0x24 },
            3 => new byte[] { 0x8B, 0x94, 0x24 },
            _ => [],
        };
        var Bytesˉprefix = argument switch
        {
            0 => new byte[] { 0x4C, 0x8D, 0x84, 0x24 },
            1 => new byte[] { 0x4C, 0x8D, 0x8C, 0x24 },
            2 => new byte[] { 0x48, 0x8D, 0x8C, 0x24 },
            3 => new byte[] { 0x48, 0x8D, 0x94, 0x24 },
            _ => [],
        };
        length = Scalarˉprefix.Length + sizeof(int);
        kind = Decodedˉargumentˉkind.Scalar;
        slot = 0;
        if (Scalarˉprefix.Length != 0 &&
            Matches(code, offset, Scalarˉprefix) &&
            Tryˉreadˉslot(code, offset + Scalarˉprefix.Length, frameˉbytes, out slot))
        {
            return true;
        }
        length = Bytesˉprefix.Length + sizeof(int);
        kind = Decodedˉargumentˉkind.Borrowedˉbytes;
        return Bytesˉprefix.Length != 0 &&
            Matches(code, offset, Bytesˉprefix) &&
            Tryˉreadˉslot(code, offset + Bytesˉprefix.Length, frameˉbytes, out slot);
    }

    private static bool Tryˉstoreˉargument(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int argument,
        out int length,
        out Decodedˉargumentˉkind kind)
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
        kind = Decodedˉargumentˉkind.Scalar;
        if (Prefix.Length != 0 &&
            Matches(code, offset, Prefix) &&
            Tryˉreadˉslot(code, offset + Prefix.Length, frameˉbytes, out var Slot) &&
            Slot == argument)
        {
            return true;
        }

        var Loadˉpointer = argument switch
        {
            0 => new byte[] { 0x49, 0x8B, 0x00 },
            1 => new byte[] { 0x49, 0x8B, 0x01 },
            2 => new byte[] { 0x48, 0x8B, 0x01 },
            3 => new byte[] { 0x48, 0x8B, 0x02 },
            _ => [],
        };
        var Loadˉlength = argument switch
        {
            0 => new byte[] { 0x41, 0x8B, 0x40, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET },
            1 => new byte[] { 0x41, 0x8B, 0x41, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET },
            2 => new byte[] { 0x8B, 0x41, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET },
            3 => new byte[] { 0x8B, 0x42, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET },
            _ => [],
        };
        var Cursor = offset + Loadˉpointer.Length;
        if (Loadˉpointer.Length == 0 ||
            !Matches(code, offset, Loadˉpointer) ||
            !Tryˉstoreˉrax(code, Cursor, frameˉbytes, out var Pointerˉslot) ||
            Pointerˉslot != argument)
        {
            return false;
        }
        Cursor += 8;
        if (!Matches(code, Cursor, Loadˉlength) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + Loadˉlength.Length,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉslot) ||
            Lengthˉslot != argument)
        {
            return false;
        }
        Cursor += Loadˉlength.Length + 7;
        length = Cursor - offset;
        kind = Decodedˉargumentˉkind.Borrowedˉbytes;
        return true;
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

    private static bool Tryˉstoreˉrax(ReadOnlySpan<byte> code, int offset, int frameˉbytes, out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x48, 0x89, 0x84, 0x24) &&
            Tryˉreadˉslot(code, offset + 4, frameˉbytes, out slot);
    }

    private static bool Tryˉstoreˉraxˉatˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int field,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x48, 0x89, 0x84, 0x24) &&
            Tryˉreadˉslotˉfield(code, offset + 4, frameˉbytes, field, sizeof(ulong), out slot);
    }

    private static bool Tryˉstoreˉeaxˉatˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int field,
        out int slot)
    {
        slot = 0;
        return Matches(code, offset, 0x89, 0x84, 0x24) &&
            Tryˉreadˉslotˉfield(code, offset + 3, frameˉbytes, field, sizeof(int), out slot);
    }

    private static bool Tryˉstoreˉeaxˉdword(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        out int dword)
    {
        dword = 0;
        if (!Matches(code, offset, 0x89, 0x84, 0x24) ||
            offset + 3 > code.Length - sizeof(int))
        {
            return false;
        }
        var Displacement = Readˉi32(code, offset + 3);
        if (Displacement < 0 ||
            (Displacement & (sizeof(int) - 1)) != 0 ||
            Displacement > frameˉbytes - sizeof(int))
        {
            return false;
        }
        dword = Displacement / sizeof(int);
        return true;
    }

    private static bool Tryˉreadˉslotˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int field,
        int width,
        out int slot)
    {
        slot = 0;
        if (offset < 0 || offset > code.Length - sizeof(int))
        {
            return false;
        }
        var Displacement = Readˉi32(code, offset);
        if (field < 0 ||
            width < 1 ||
            field > Nativeˉcontract.VALUE_SLOT_BYTES - width ||
            Displacement < field ||
            (Displacement - field) % Nativeˉcontract.VALUE_SLOT_BYTES != 0 ||
            Displacement > frameˉbytes - width)
        {
            return false;
        }
        slot = (Displacement - field) / Nativeˉcontract.VALUE_SLOT_BYTES;
        return true;
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
            (Displacement & (Nativeˉcontract.VALUE_SLOT_BYTES - 1)) != 0 ||
            Displacement > frameˉbytes - Nativeˉcontract.VALUE_SLOT_BYTES)
        {
            return false;
        }
        slot = Displacement / Nativeˉcontract.VALUE_SLOT_BYTES;
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
        condition is 0x92 or 0x93 or 0x94 or 0x95 or 0x96 or 0x97 or 0x9C or 0x9D or 0x9E or 0x9F;

    private static void Enqueue(int index, bool[] reachable, Queue<int> pending)
    {
        if (!reachable[index])
        {
            reachable[index] = true;
            pending.Enqueue(index);
        }
    }

    private static Decodedˉreturnˉkind Mergeˉreturnˉkind(
        Decodedˉreturnˉkind? current,
        Decodedˉreturnˉkind next)
    {
        if (current is not null && current != next)
        {
            Failˉshape();
        }
        return next;
    }

    private static bool Matches(ReadOnlySpan<byte> code, int offset, params byte[] expected) =>
        offset >= 0 &&
        offset <= code.Length - expected.Length &&
        code.Slice(offset, expected.Length).SequenceEqual(expected);

    private enum Decodedˉargumentˉkind : byte
    {
        Scalar = 1,
        Borrowedˉbytes = 2,
    }

    private enum Decodedˉreturnˉkind : byte
    {
        Void = 0,
        Scalar = 1,
        Descriptor = 2,
    }

    private sealed record Decodedˉfunction(
        Decodedˉargumentˉkind[] Parameterˉkinds,
        Decodedˉreturnˉkind Returnˉkind,
        Decodedˉcall[] Calls);

    private readonly record struct Decodedˉcall(
        int Target,
        Decodedˉargumentˉkind[] Argumentˉkinds,
        int[] Argumentˉslots,
        Decodedˉreturnˉkind Returnˉkind,
        int Resultˉslot);

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
