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
        _ = Verifyˉentryˉresultˉkind(fragment);
        return fragment;
    }

    public static Nativeˉentryˉresultˉkind Verifyˉentryˉresultˉkind(
        Nativeˉfragment fragment)
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
        if (fragment.Requiredˉservices.Length > 12 ||
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
        return Verifyˉtargetˉshape(fragment);
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
                        Valueˉtype.Text or
                        Valueˉtype.Bytes or
                        Valueˉtype.Enum) ||
                    (Field.Type.Kind == Valueˉtype.Enum
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

    private static Nativeˉentryˉresultˉkind Verifyˉtargetˉshape(
        Nativeˉfragment fragment)
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
        var Main = Functions.Single(Function => Function.Binding == Nativeˉsymbolˉbinding.Export);
        return Decoded[checked((int)Main.Offset)].Returnˉkind switch
        {
            Decodedˉreturnˉkind.Void => Nativeˉentryˉresultˉkind.Void,
            Decodedˉreturnˉkind.Scalar => Nativeˉentryˉresultˉkind.Scalar,
            Decodedˉreturnˉkind.Descriptor => Nativeˉentryˉresultˉkind.Descriptor,
            _ => throw new InvalidOperationException("Verified native entry result became invalid."),
        };
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
        var Hasˉentryˉresultˉbridge = Isˉmain && Matches(Code, Index, 0x48, 0x89, 0xC8);
        if (Hasˉentryˉresultˉbridge)
        {
            Index += 3;
        }
        var Hasˉhiddenˉresult = Tryˉstoreˉrax(
            Code,
            Index,
            Frameˉbytes,
            out var Hiddenˉresultˉslot);
        if (Isˉmain && Hasˉentryˉresultˉbridge != Hasˉhiddenˉresult)
        {
            Failˉshape();
        }
        if (Hasˉhiddenˉresult)
        {
            Index += 8;
        }
        if (!Matches(Code, Index, 0x31, 0xC0))
        {
            Failˉshape();
        }
        Index += 2;
        for (var Dword = 0; Dword < Frameˉbytes / sizeof(int); Dword++)
        {
            if (Hasˉhiddenˉresult &&
                Dword >= Hiddenˉresultˉslot * (Nativeˉcontract.VALUE_SLOT_BYTES / sizeof(int)) &&
                Dword < (Hiddenˉresultˉslot + 1) *
                    (Nativeˉcontract.VALUE_SLOT_BYTES / sizeof(int)))
            {
                continue;
            }
            if (!Tryˉstoreˉeaxˉdword(Code, Index, Frameˉbytes, out var Initialized) ||
                Initialized != Dword)
            {
                Failˉshape();
            }
            Index += 7;
        }

        var Hasˉarenaˉcheckpoint = false;
        if (!Isˉmain &&
            Hasˉhiddenˉresult &&
            Matches(
                Code,
                Index,
                0x41, 0x8B, 0x47,
                Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET) &&
            Tryˉstoreˉeaxˉatˉfield(
                Code,
                Index + 4,
                Frameˉbytes,
                sizeof(ulong),
                out var Checkpointˉslot) &&
            Checkpointˉslot == Hiddenˉresultˉslot)
        {
            Hasˉarenaˉcheckpoint = true;
            Index += 11;
        }

        var Parameterˉkinds = new List<Decodedˉargumentˉkind>();
        while (Parameterˉkinds.Count < Nativeˉcontract.MAXIMUM_CALL_PARAMETERS &&
            Tryˉstoreˉargument(
                Code[..End],
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
        var Recordˉslots = Parameterˉkinds
            .Select((Kind, Slot) => (Kind, Slot))
            .Where(Item => Item.Kind == Decodedˉargumentˉkind.Record)
            .Select(Item => Item.Slot)
            .ToHashSet();
        while (Tryˉdecodeˉrecordˉcopy(
            Code,
            ref Index,
            End,
            Frameˉbytes,
            fragment.Types,
            Recordˉslots,
            out var Parameterˉcopyˉsource,
            out var Parameterˉcopyˉtarget,
            out _))
        {
            if (Parameterˉcopyˉsource != Parameterˉcopyˉtarget ||
                (uint)Parameterˉcopyˉtarget >= (uint)Parameterˉkinds.Count ||
                Parameterˉkinds[Parameterˉcopyˉtarget] != Decodedˉargumentˉkind.Record)
            {
                Failˉshape();
            }
            Recordˉslots.Add(Parameterˉcopyˉtarget);
        }
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
                Hasˉarenaˉcheckpoint &&
                Tryˉdecodeˉdescriptorˉcheckpointˉreturn(
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Hiddenˉresultˉslot,
                    out var Checkpointˉreturnˉslot))
            {
                if (!Borrowedˉbytesˉslots.Contains(Checkpointˉreturnˉslot))
                {
                    Failˉshape();
                }
                Returnˉkind = Mergeˉreturnˉkind(
                    Returnˉkind,
                    Decodedˉreturnˉkind.Descriptor);
                Returns++;
                Groups.Add(new(Groupˉstart, false, true, false, []));
                continue;
            }

            if (Hasˉhiddenˉresult &&
                Tryˉdecodeˉrecordˉtypeˉtag(
                    Code,
                    Index,
                    fragment.Types,
                    out _,
                    out var Returnˉrecord) &&
                Tryˉloadˉrdx(
                    Code,
                    Index + 6,
                    Frameˉbytes,
                    out var Recordˉhiddenˉreturnˉslot) &&
                Recordˉhiddenˉreturnˉslot == Hiddenˉresultˉslot &&
                Tryˉloadˉrax(
                    Code,
                    Index + 14,
                    Frameˉbytes,
                    out var Recordˉreturnˉslot) &&
                Recordˉslots.Contains(Recordˉreturnˉslot) &&
                Tryˉdecodeˉrecordˉreturnˉfields(
                    Code,
                    Index + 22,
                    Propagate,
                    Returnˉrecord.Fields.Length,
                    out var Afterˉrecordˉfields))
            {
                var Afterˉrecordˉcheckpoint = Afterˉrecordˉfields;
                var Requiresˉrecordˉcheckpoint = Hasˉarenaˉcheckpoint &&
                    Returnˉrecord.Fields.All(Field =>
                        Field.Type.Kind is not Valueˉtype.Text and not Valueˉtype.Bytes);
                if (Requiresˉrecordˉcheckpoint)
                {
                    if (!Tryˉloadˉeaxˉatˉfield(
                            Code,
                            Afterˉrecordˉcheckpoint,
                            Frameˉbytes,
                            sizeof(ulong),
                            out var Recordˉcheckpointˉslot) ||
                        Recordˉcheckpointˉslot != Hiddenˉresultˉslot ||
                        !Matches(
                            Code,
                            Afterˉrecordˉcheckpoint + 7,
                            0x41, 0x89, 0x47,
                            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET))
                    {
                        Failˉshape();
                    }
                    Afterˉrecordˉcheckpoint += 11;
                }
                if (!Matches(
                        Code,
                        Afterˉrecordˉcheckpoint,
                        0x31, 0xC0,
                        0x48, 0x81, 0xC4) ||
                    Readˉi32(Code, Afterˉrecordˉcheckpoint + 5) != Frameˉbytes ||
                    !Matchesˉrestoreˉdepthˉandˉreturn(
                        Code,
                        Afterˉrecordˉcheckpoint + 9,
                        Isˉmain))
                {
                    Failˉshape();
                }
                Index = checked(Afterˉrecordˉcheckpoint + (Isˉmain ? 15 : 13));
                Returnˉkind = Mergeˉreturnˉkind(
                    Returnˉkind,
                    Decodedˉreturnˉkind.Record);
                Returns++;
                Groups.Add(new(Groupˉstart, false, true, false, []));
                continue;
            }
            if (Hasˉhiddenˉresult &&
                Tryˉloadˉrdx(Code, Index, Frameˉbytes, out var Hiddenˉreturnˉslot) &&
                Hiddenˉreturnˉslot == Hiddenˉresultˉslot &&
                Tryˉloadˉrax(Code, Index + 8, Frameˉbytes, out var Descriptorˉreturnˉslot) &&
                Borrowedˉbytesˉslots.Contains(Descriptorˉreturnˉslot) &&
                Matches(Code, Index + 16, 0x48, 0x89, 0x02))
            {
                var Descriptorˉreturnˉlength = 0;
                if (Isˉmain &&
                    Tryˉloadˉeaxˉatˉfield(
                        Code,
                        Index + 19,
                        Frameˉbytes,
                        Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                        out var Mainˉdescriptorˉreturnˉfieldˉslot) &&
                    Mainˉdescriptorˉreturnˉfieldˉslot == Descriptorˉreturnˉslot &&
                    Matches(
                        Code,
                        Index + 26,
                        0x89, 0x42, 0x08,
                        0x31, 0xC0,
                        0x89, 0x42, 0x0C) &&
                    Matches(Code, Index + 34, 0x48, 0x81, 0xC4) &&
                    Readˉi32(Code, Index + 37) == Frameˉbytes &&
                    Matchesˉrestoreˉdepthˉandˉreturn(Code, Index + 41, Isˉmain))
                {
                    Descriptorˉreturnˉlength = 47;
                }
                else if (!Isˉmain &&
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
                    Descriptorˉreturnˉlength = 44;
                }

                if (Descriptorˉreturnˉlength != 0)
                {
                    Index += Descriptorˉreturnˉlength;
                    Returnˉkind = Mergeˉreturnˉkind(Returnˉkind, Decodedˉreturnˉkind.Descriptor);
                    Returns++;
                    Groups.Add(new(Groupˉstart, false, true, false, []));
                    continue;
                }
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
                fragment.Types,
                out var Call))
            {
                if (Call.Returnˉkind == Decodedˉreturnˉkind.Descriptor)
                {
                    Borrowedˉbytesˉslots.Add(Call.Resultˉslot);
                    Staticˉdescriptorˉdata.Remove(Call.Resultˉslot);
                }
                else if (Call.Returnˉkind == Decodedˉreturnˉkind.Scalar &&
                    (Borrowedˉbytesˉslots.Contains(Call.Resultˉslot) ||
                        Recordˉslots.Contains(Call.Resultˉslot)))
                {
                    Failˉshape();
                }
                else if (Call.Returnˉkind == Decodedˉreturnˉkind.Record)
                {
                    Recordˉslots.Add(Call.Resultˉslot);
                    Borrowedˉbytesˉslots.Remove(Call.Resultˉslot);
                    Staticˉdescriptorˉdata.Remove(Call.Resultˉslot);
                }
                for (var Argument = 0; Argument < Call.Argumentˉkinds.Length; Argument++)
                {
                    if (Call.Argumentˉkinds[Argument] == Decodedˉargumentˉkind.Borrowedˉbytes &&
                        !Borrowedˉbytesˉslots.Contains(Call.Argumentˉslots[Argument]))
                    {
                        Failˉshape();
                    }
                    if (Call.Argumentˉkinds[Argument] == Decodedˉargumentˉkind.Record &&
                        !Recordˉslots.Contains(Call.Argumentˉslots[Argument]))
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
                Staticˉdescriptorˉdata.Remove(Argumentˉslot);
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
                Staticˉdescriptorˉdata.Remove(Decodedˉtextˉslot);
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
                Staticˉdescriptorˉdata.Remove(Enumˉnameˉslot);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉfileˉwriteˉbytes(
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
                Staticˉdescriptorˉdata.Remove(Fileˉslot);
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
                Staticˉdescriptorˉdata[Staticˉbytesˉslot] = Staticˉdata;
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉbytesˉconcat(
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out var Concatˉbytesˉslot) ||
                Tryˉdecodeˉbytesˉfromˉu8(
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out Concatˉbytesˉslot) ||
                Tryˉdecodeˉbytesˉfromˉu16ˉlittle(
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out Concatˉbytesˉslot) ||
                Tryˉdecodeˉbytesˉfromˉu32ˉlittle(
                    Code,
                    ref Index,
                    Propagate,
                    Frameˉbytes,
                    Runtimeˉservice,
                    Borrowedˉbytesˉslots,
                    out Concatˉbytesˉslot))
            {
                Borrowedˉbytesˉslots.Add(Concatˉbytesˉslot);
                Staticˉdescriptorˉdata.Remove(Concatˉbytesˉslot);
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
                else
                {
                    Staticˉdescriptorˉdata.Remove(Copiedˉbytesˉslot);
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
                Staticˉdescriptorˉdata.Remove(Sliceˉslot);
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
                fragment.Types,
                Borrowedˉbytesˉslots,
                out var Recordˉcreateˉresult,
                out _))
            {
                if (Borrowedˉbytesˉslots.Contains(Recordˉcreateˉresult))
                {
                    Failˉshape();
                }
                Recordˉslots.Add(Recordˉcreateˉresult);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉrecordˉcopy(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                fragment.Types,
                Recordˉslots,
                out _,
                out var Recordˉcopyˉtarget,
                out _))
            {
                Recordˉslots.Add(Recordˉcopyˉtarget);
                Borrowedˉbytesˉslots.Remove(Recordˉcopyˉtarget);
                Staticˉdescriptorˉdata.Remove(Recordˉcopyˉtarget);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            var Recordˉreferenceˉafter = Index;
            if (Tryˉdecodeˉrecordˉreferenceˉcopy(
                Code,
                ref Recordˉreferenceˉafter,
                Propagate,
                Frameˉbytes,
                fragment.Types,
                Recordˉslots,
                out var Recordˉreferenceˉtarget) &&
                (Recordˉreferenceˉafter == Propagate ||
                    Matches(
                        Code,
                        Recordˉreferenceˉafter,
                        0x49, 0x83, 0xEB, 0x01, 0x0F, 0x82)))
            {
                Index = Recordˉreferenceˉafter;
                Recordˉslots.Add(Recordˉreferenceˉtarget);
                Borrowedˉbytesˉslots.Remove(Recordˉreferenceˉtarget);
                Staticˉdescriptorˉdata.Remove(Recordˉreferenceˉtarget);
                Groups.Add(new(Groupˉstart, true, false, false, []));
                continue;
            }

            if (Tryˉdecodeˉrecordˉfield(
                Code,
                ref Index,
                Propagate,
                Frameˉbytes,
                fragment.Types,
                Recordˉslots,
                out var Recordˉfieldˉresult,
                out var Recordˉfieldˉisˉdescriptor))
            {
                if (Recordˉslots.Contains(Recordˉfieldˉresult))
                {
                    Failˉshape();
                }
                if (Recordˉfieldˉisˉdescriptor)
                {
                    Borrowedˉbytesˉslots.Add(Recordˉfieldˉresult);
                    Staticˉdescriptorˉdata.Remove(Recordˉfieldˉresult);
                }
                else if (Borrowedˉbytesˉslots.Contains(Recordˉfieldˉresult))
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
                Fail(
                    "WVN3030",
                    $"The x86-64 baseline fragment contains an undecodable ABI-22 group " +
                    $"in function '{symbol.Name}' at code offset {Groupˉstart}: " +
                    Convert.ToHexString(
                        Code.Slice(
                            Groupˉstart,
                            Math.Min(48, Propagate - Groupˉstart))));
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
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        out Decodedˉcall call)
    {
        call = new(0, [], [], Decodedˉreturnˉkind.Void, -1);
        if (end < 0 || end > code.Length)
        {
            return false;
        }
        var Functionˉcode = code[..end];
        var Cursor = index;
        Recordˉtypeˉdeclaration? Recordˉreturn = null;
        if (Tryˉdecodeˉrecordˉtypeˉtag(
            Functionˉcode,
            Cursor,
            types,
            out _,
            out var Taggedˉreturn))
        {
            Recordˉreturn = Taggedˉreturn;
            Cursor += 6;
        }
        var Argumentˉkinds = new List<Decodedˉargumentˉkind>();
        var Argumentˉslots = new List<int>();
        while (Argumentˉkinds.Count < Nativeˉcontract.REGISTER_CALL_PARAMETERS &&
            Tryˉloadˉargument(
                Functionˉcode,
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
        var Stackˉbytes = 0;
        if (Argumentˉkinds.Count == Nativeˉcontract.REGISTER_CALL_PARAMETERS &&
            Matches(Functionˉcode, Cursor, 0x48, 0x81, 0xEC))
        {
            if (Cursor > Functionˉcode.Length - 7)
            {
                return false;
            }
            Stackˉbytes = Readˉi32(Functionˉcode, Cursor + 3);
            if (Stackˉbytes is <= 0 or > Nativeˉcontract.MAXIMUM_STACK_CALL_BYTES ||
                (Stackˉbytes & (Nativeˉcontract.VALUE_SLOT_BYTES - 1)) != 0)
            {
                return false;
            }
            Cursor += 7;
            while (Argumentˉkinds.Count < Nativeˉcontract.MAXIMUM_CALL_PARAMETERS &&
                Tryˉloadˉstackˉargument(
                    Functionˉcode,
                    Cursor,
                    frameˉbytes,
                    Stackˉbytes,
                    Argumentˉkinds.Count,
                    out var Argumentˉlength,
                    out var Argumentˉkind,
                    out var Argumentˉslot))
            {
                Argumentˉkinds.Add(Argumentˉkind);
                Argumentˉslots.Add(Argumentˉslot);
                Cursor += Argumentˉlength;
            }
            if (Stackˉbytes != Stackˉcallˉbytes(Argumentˉkinds.Count))
            {
                return false;
            }
        }
        var Returnˉkind = Decodedˉreturnˉkind.Void;
        var Resultˉslot = -1;
        var Resultˉbackingˉslot = -1;
        if (Matches(Functionˉcode, Cursor, 0x48, 0x8D, 0x84, 0x24) &&
            Tryˉreadˉadjustedˉslot(
                Functionˉcode,
                Cursor + 4,
                frameˉbytes,
                Stackˉbytes,
                out Resultˉbackingˉslot))
        {
            Returnˉkind = Recordˉreturn is null
                ? Decodedˉreturnˉkind.Descriptor
                : Decodedˉreturnˉkind.Record;
            if (Recordˉreturn is null)
            {
                Resultˉslot = Resultˉbackingˉslot;
            }
            if (Recordˉreturn is not null &&
                !Rangeˉfitsˉframe(
                    Resultˉbackingˉslot,
                    Recordˉreturn.Fields.Length,
                    frameˉbytes))
            {
                return false;
            }
            Cursor += 8;
        }
        else if (Recordˉreturn is not null)
        {
            return false;
        }
        if (Cursor >= end || Functionˉcode[Cursor] != 0xE8 ||
            !Tryˉreadˉtarget(Functionˉcode, Cursor + 1, out var Target) ||
            !functions.ContainsKey(Target))
        {
            return false;
        }
        Cursor += 5;
        if (Stackˉbytes != 0)
        {
            if (Cursor > Functionˉcode.Length - 7 ||
                !Matches(Functionˉcode, Cursor, 0x48, 0x81, 0xC4) ||
                Readˉi32(Functionˉcode, Cursor + 3) != Stackˉbytes)
            {
                return false;
            }
            Cursor += 7;
        }
        if (!Matches(Functionˉcode, Cursor,
                0x48, 0x89, 0xC2,
                0x48, 0xC1, 0xEA, 0x20,
                0x48, 0x85, 0xD2,
                0x0F, 0x85) ||
            !Tryˉreadˉtarget(Functionˉcode, Cursor + 12, out var Propagateˉtarget) ||
            Propagateˉtarget != propagate)
        {
            return false;
        }
        Cursor += 16;
        if (Returnˉkind == Decodedˉreturnˉkind.Record)
        {
            if (!Matches(Functionˉcode, Cursor, 0x48, 0x8D, 0x84, 0x24) ||
                !Tryˉreadˉslot(
                    Functionˉcode,
                    Cursor + 4,
                    frameˉbytes,
                    out var Repeatedˉbacking) ||
                Repeatedˉbacking != Resultˉbackingˉslot ||
                !Tryˉstoreˉrax(
                    Functionˉcode,
                    Cursor + 8,
                    frameˉbytes,
                    out Resultˉslot))
            {
                return false;
            }
            Cursor += 16;
        }
        else if (Returnˉkind != Decodedˉreturnˉkind.Descriptor &&
            Tryˉstoreˉeax(Functionˉcode, Cursor, frameˉbytes, out Resultˉslot))
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

    private static bool Tryˉdecodeˉfileˉwriteˉbytes(
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
        if (!fragment.Requiredˉservices.Contains(Nativeˉservice.Fileˉwriteˉbytes) ||
            !Tryˉdecodeˉdescriptorˉserviceˉinput(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉdescriptorˉslots,
                out var Nameˉslot) ||
            !Matches(code, Cursor, 0x48, 0x8B, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out var Bytesˉslot) ||
            !borrowedˉdescriptorˉslots.Contains(Bytesˉslot) ||
            !Matches(code, Cursor + 8, 0x8B, 0x94, 0x24) ||
            !Tryˉreadˉslotˉfield(
                code,
                Cursor + 11,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                sizeof(int),
                out var Lengthˉslot) ||
            Lengthˉslot != Bytesˉslot)
        {
            return false;
        }
        if (staticˉdescriptorˉdata.TryGetValue(Nameˉslot, out var Nameˉdata) &&
            !Isˉvalidˉutf8(fragment.Code, Nameˉdata))
        {
            return false;
        }
        Cursor += 15;
        if (!Tryˉdecodeˉserviceˉcall(
                code,
                ref Cursor,
                Nativeˉserviceˉtableˉcontract.FILE_WRITE_BYTES_POINTER_OFFSET,
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
        if (resultˉslot == Sourceˉslot ||
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
            resultˉslot == Valueˉslot)
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
            resultˉslot == Valueˉslot)
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
            !Tryˉdecodeˉdescriptorˉaddress(code, Cursor, frameˉbytes, 0x84, borrowedˉdescriptorˉslots, out var Leftˉslot) ||
            !Tryˉdecodeˉdescriptorˉaddress(code, Cursor + 8, frameˉbytes, 0x8C, borrowedˉdescriptorˉslots, out var Rightˉslot) ||
            !Matches(code, Cursor + 16, 0x48, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 20, frameˉbytes, out resultˉslot) ||
            resultˉslot == Leftˉslot ||
            resultˉslot == Rightˉslot)
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
            !Tryˉdecodeˉdescriptorˉaddress(code, Cursor, frameˉbytes, 0x84, borrowedˉdescriptorˉslots, out var Textˉslot) ||
            !Matches(code, Cursor + 8, 0x4C, 0x8D, 0x8C, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 12, frameˉbytes, out resultˉslot) ||
            resultˉslot == Textˉslot)
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
            Lengthˉslot != resultˉslot ||
            !Matches(code, Cursor + 27, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 29,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Reservedˉslot) ||
            Reservedˉslot != resultˉslot)
        {
            return false;
        }
        Cursor += 36;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉdescriptorˉcheckpointˉreturn(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int hiddenˉresultˉslot,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (Cursor < 0 ||
            Cursor > end - 306 ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor,
                frameˉbytes,
                sizeof(ulong),
                out var Checkpointˉslot) ||
            Checkpointˉslot != hiddenˉresultˉslot ||
            !Matches(
                code,
                Cursor + 7,
                0x41, 0x89, 0xC1,
                0x4D, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET) ||
            !Tryˉloadˉrax(code, Cursor + 14, frameˉbytes, out resultˉslot) ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 22,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉslot) ||
            Lengthˉslot != resultˉslot ||
            !Matches(code, Cursor + 29, 0x4C, 0x39, 0xC0, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 34, out var Resetˉtarget) ||
            !Matches(code, Cursor + 38, 0x48, 0x89, 0xC2, 0x48, 0x01, 0xCA, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 46, out var Preserveˉtarget) ||
            !Matches(
                code,
                Cursor + 50,
                0x4C, 0x29, 0xC0,
                0x4C, 0x29, 0xC2,
                0x45, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x4C, 0x39, 0xC0,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 65, out var Secondˉresetˉtarget) ||
            Secondˉresetˉtarget != Resetˉtarget ||
            !Matches(code, Cursor + 69, 0x4C, 0x39, 0xC2, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 74, out var Thirdˉresetˉtarget) ||
            Thirdˉresetˉtarget != Resetˉtarget ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 78,
                frameˉbytes,
                sizeof(ulong),
                out var Secondˉcheckpointˉslot) ||
            Secondˉcheckpointˉslot != hiddenˉresultˉslot ||
            !Matches(code, Cursor + 85, 0x41, 0x89, 0xC1, 0x4C, 0x39, 0xC8, 0x0F, 0x83) ||
            !Tryˉreadˉtarget(code, Cursor + 93, out var Internalˉtarget) ||
            !Matches(code, Cursor + 97, 0x4C, 0x39, 0xCA, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 102, out var Secondˉpreserveˉtarget) ||
            Secondˉpreserveˉtarget != Preserveˉtarget ||
            Resetˉtarget != Cursor + 106 ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 106,
                frameˉbytes,
                sizeof(ulong),
                out var Resetˉcheckpointˉslot) ||
            Resetˉcheckpointˉslot != hiddenˉresultˉslot ||
            !Matches(
                code,
                Cursor + 113,
                0x41, 0x89, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 118, out var Externalˉtarget) ||
            Preserveˉtarget != Cursor + 122 ||
            Externalˉtarget != Cursor + 122 ||
            !Tryˉloadˉrdx(code, Cursor + 122, frameˉbytes, out var Externalˉhiddenˉslot) ||
            Externalˉhiddenˉslot != hiddenˉresultˉslot ||
            !Tryˉloadˉrax(code, Cursor + 130, frameˉbytes, out var Externalˉresultˉslot) ||
            Externalˉresultˉslot != resultˉslot ||
            !Matches(code, Cursor + 138, 0x48, 0x89, 0x02) ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 141,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Externalˉlengthˉslot) ||
            Externalˉlengthˉslot != resultˉslot ||
            !Matches(
                code,
                Cursor + 148,
                0x89, 0x42, 0x08,
                0x31, 0xC0,
                0x89, 0x42, 0x0C,
                0x48, 0x81, 0xC4) ||
            Readˉi32(code, Cursor + 159) != frameˉbytes ||
            !Matchesˉrestoreˉdepthˉandˉreturn(code, Cursor + 163, false) ||
            Internalˉtarget != Cursor + 167 ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 167,
                frameˉbytes,
                sizeof(ulong),
                out var Internalˉcheckpointˉslot) ||
            Internalˉcheckpointˉslot != hiddenˉresultˉslot ||
            !Matches(
                code,
                Cursor + 174,
                0x41, 0x89, 0xC1,
                0x4D, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x49, 0x01, 0xC1) ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 184,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Internalˉlengthˉslot) ||
            Internalˉlengthˉslot != resultˉslot ||
            !Matches(code, Cursor + 191, 0x89, 0xC2, 0x01, 0xCA, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 197, out var Thirdˉpreserveˉtarget) ||
            Thirdˉpreserveˉtarget != Preserveˉtarget ||
            !Matches(
                code,
                Cursor + 201,
                0x41, 0x3B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 207, out var Fourthˉpreserveˉtarget) ||
            Fourthˉpreserveˉtarget != Preserveˉtarget ||
            !Matches(
                code,
                Cursor + 211,
                0x41, 0x89, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET) ||
            !Tryˉloadˉrax(code, Cursor + 215, frameˉbytes, out var Copyˉsourceˉslot) ||
            Copyˉsourceˉslot != resultˉslot ||
            !Matches(code, Cursor + 223, 0x85, 0xC9, 0x0F, 0x84) ||
            !Tryˉreadˉtarget(code, Cursor + 227, out var Copyˉendˉtarget) ||
            !Matches(
                code,
                Cursor + 231,
                0x44, 0x0F, 0xB6, 0x00,
                0x45, 0x88, 0x01,
                0x48, 0xFF, 0xC0,
                0x49, 0xFF, 0xC1,
                0xFF, 0xC9,
                0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 248, out var Copyˉtarget) ||
            Copyˉtarget != Cursor + 231 ||
            Copyˉendˉtarget != Cursor + 252 ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 252,
                frameˉbytes,
                sizeof(ulong),
                out var Finalˉcheckpointˉslot) ||
            Finalˉcheckpointˉslot != hiddenˉresultˉslot ||
            !Matches(
                code,
                Cursor + 259,
                0x41, 0x89, 0xC1,
                0x49, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x4C, 0x01, 0xC8) ||
            !Tryˉloadˉrdx(code, Cursor + 269, frameˉbytes, out var Finalˉhiddenˉslot) ||
            Finalˉhiddenˉslot != hiddenˉresultˉslot ||
            !Matches(code, Cursor + 277, 0x48, 0x89, 0x02) ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 280,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Finalˉlengthˉslot) ||
            Finalˉlengthˉslot != resultˉslot ||
            !Matches(
                code,
                Cursor + 287,
                0x89, 0x42, 0x08,
                0x31, 0xC0,
                0x89, 0x42, 0x0C,
                0x48, 0x81, 0xC4) ||
            Readˉi32(code, Cursor + 298) != frameˉbytes ||
            !Matchesˉrestoreˉdepthˉandˉreturn(code, Cursor + 302, false))
        {
            return false;
        }

        index = Cursor + 306;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉconcat(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
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
                out var Leftˉslot) ||
            !borrowedˉbytesˉslots.Contains(Leftˉslot) ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 7,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Rightˉslot) ||
            !borrowedˉbytesˉslots.Contains(Rightˉslot) ||
            !Matches(code, Cursor + 14, 0x01, 0xC8, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 18, out var Limitˉtarget) ||
            code[Cursor + 22] != 0x3D ||
            Readˉi32(code, Cursor + 23) != Bytecodeˉlimits.MAX_BYTE_DATA_BYTES ||
            !Matches(code, Cursor + 27, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 29, out var Secondˉlimitˉtarget) ||
            Secondˉlimitˉtarget != Limitˉtarget)
        {
            return false;
        }
        Cursor += 33;
        if (!Matches(
                code,
                Cursor,
                0x41, 0x89, 0xC0,
                0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x41, 0x89, 0xC1,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x89, 0xC1,
                0x48, 0x01, 0xCA))
        {
            return false;
        }
        Cursor += 19;
        if (!Matches(code, Cursor, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 2,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Clearedˉresult))
        {
            return false;
        }
        Cursor += 9;
        if (!Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Ownedˉleft) ||
            Ownedˉleft != Leftˉslot ||
            !Matches(code, Cursor + 7, 0x85, 0xC0, 0x0F, 0x84) ||
            !Tryˉreadˉtarget(code, Cursor + 11, out var Tailˉprobeˉtarget) ||
            !Matches(code, Cursor + 15, 0x89, 0xC1) ||
            !Tryˉloadˉrax(code, Cursor + 17, frameˉbytes, out var Ownedˉleftˉpointer) ||
            Ownedˉleftˉpointer != Leftˉslot ||
            !Matches(
                code,
                Cursor + 25,
                0x48, 0x83, 0xE8, Nativeˉcontract.DYNAMIC_BYTES_HEADER_BYTES,
                0x49, 0x3B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 35, out var Secondˉtailˉprobeˉtarget) ||
            Secondˉtailˉprobeˉtarget != Tailˉprobeˉtarget ||
            !Matches(
                code,
                Cursor + 39,
                0x48, 0x83, 0xC0, Nativeˉcontract.DYNAMIC_BYTES_HEADER_BYTES,
                0x48, 0x39, 0xD0,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 48, out var Thirdˉtailˉprobeˉtarget) ||
            Thirdˉtailˉprobeˉtarget != Tailˉprobeˉtarget ||
            !Matches(code, Cursor + 52, 0x3B, 0x48, 0xFC, 0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 57, out var Fourthˉtailˉprobeˉtarget) ||
            Fourthˉtailˉprobeˉtarget != Tailˉprobeˉtarget ||
            !Matches(
                code,
                Cursor + 61,
                0x8B, 0x48, 0xF8,
                0x48, 0x01, 0xC1,
                0x48, 0x39, 0xD1,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 72, out var Fifthˉtailˉprobeˉtarget) ||
            Fifthˉtailˉprobeˉtarget != Tailˉprobeˉtarget ||
            !Matches(code, Cursor + 76, 0x8B, 0x50, 0xF8, 0x44, 0x39, 0xC2, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 84, out var Growˉownedˉtarget) ||
            !Matches(code, Cursor + 88, 0x8B, 0x48, 0xFC, 0x83, 0xC1, 0x01, 0x0F, 0x84) ||
            !Tryˉreadˉtarget(code, Cursor + 96, out var Sixthˉtailˉprobeˉtarget) ||
            Sixthˉtailˉprobeˉtarget != Tailˉprobeˉtarget ||
            !Matches(code, Cursor + 100, 0x89, 0x48, 0xFC) ||
            !Matches(code, Cursor + 103, 0x49, 0x89, 0xC1) ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 106,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Ownedˉleftˉlength) ||
            Ownedˉleftˉlength != Leftˉslot ||
            !Matches(code, Cursor + 113, 0x49, 0x01, 0xC9, 0x8B, 0x48, 0xFC) ||
            !Tryˉstoreˉrax(code, Cursor + 119, frameˉbytes, out resultˉslot) ||
            Clearedˉresult != resultˉslot ||
            resultˉslot == Leftˉslot ||
            resultˉslot == Rightˉslot ||
            !Matches(code, Cursor + 127, 0x44, 0x89, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 130,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Ownedˉlengthˉresult) ||
            Ownedˉlengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 137, 0x89, 0xC8) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 139,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Ownedˉgenerationˉresult) ||
            Ownedˉgenerationˉresult != resultˉslot)
        {
            return false;
        }
        Cursor += 146;
        if (!Tryˉdecodeˉbytesˉcopyˉloop(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉbytesˉslots,
                Rightˉslot) ||
            code[Cursor] != 0xE9 ||
            !Tryˉreadˉtarget(code, Cursor + 1, out var Ownedˉendˉtarget))
        {
            return false;
        }
        Cursor += 5;
        if (Growˉownedˉtarget != Cursor ||
            !Matches(code, Cursor, 0x48, 0x39, 0xD1, 0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 5, out var Promoteˉtarget) ||
            !Matches(code, Cursor + 9, 0x8B, 0x48, 0xFC, 0x83, 0xC1, 0x01, 0x0F, 0x84) ||
            !Tryˉreadˉtarget(code, Cursor + 17, out var Growˉtailˉprobeˉtarget) ||
            Growˉtailˉprobeˉtarget != Tailˉprobeˉtarget ||
            !Matches(
                code,
                Cursor + 21,
                0x8B, 0x48, 0xF8,
                0x44, 0x89, 0xC2,
                0x29, 0xCA,
                0x45, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x41, 0x01, 0xD1,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 38, out var Growˉarenaˉtarget) ||
            !Matches(
                code,
                Cursor + 42,
                0x45, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 48, out var Secondˉgrowˉarenaˉtarget) ||
            Secondˉgrowˉarenaˉtarget != Growˉarenaˉtarget ||
            !Matches(
                code,
                Cursor + 52,
                0x45, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x44, 0x89, 0x40, 0xF8,
                0x8B, 0x48, 0xFC,
                0x83, 0xC1, 0x01,
                0x89, 0x48, 0xFC) ||
            !Tryˉstoreˉrax(code, Cursor + 69, frameˉbytes, out var Grownˉresult) ||
            Grownˉresult != resultˉslot ||
            !Matches(code, Cursor + 77, 0x49, 0x89, 0xC1, 0x44, 0x89, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 83,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Grownˉlengthˉresult) ||
            Grownˉlengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 90, 0x89, 0xC8) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 92,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Grownˉgenerationˉresult) ||
            Grownˉgenerationˉresult != resultˉslot ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 99,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Grownˉleftˉlength) ||
            Grownˉleftˉlength != Leftˉslot ||
            !Matches(code, Cursor + 106, 0x49, 0x01, 0xC9))
        {
            return false;
        }
        Cursor += 109;
        if (!Tryˉdecodeˉbytesˉcopyˉloop(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉbytesˉslots,
                Rightˉslot) ||
            code[Cursor] != 0xE9 ||
            !Tryˉreadˉtarget(code, Cursor + 1, out var Grownˉendˉtarget))
        {
            return false;
        }
        Cursor += 5;
        if (Promoteˉtarget != Cursor ||
            code[Cursor] != 0xB8 ||
            Readˉi32(code, Cursor + 1) != 1 ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 5,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Promotedˉresult) ||
            Promotedˉresult != resultˉslot)
        {
            return false;
        }
        Cursor += 12;
        if (Tailˉprobeˉtarget != Cursor)
        {
            return false;
        }
        if (!Tryˉloadˉrax(code, Cursor, frameˉbytes, out var Reusedˉleft) ||
            Reusedˉleft != Leftˉslot ||
            !Matches(
                code,
                Cursor + 8,
                0x49, 0x3B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 14, out var Fallbackˉtarget))
        {
            return false;
        }
        Cursor += 18;
        if (!Tryˉloadˉecxˉatˉfield(
                code,
                Cursor,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Reusedˉleftˉlength) ||
            Reusedˉleftˉlength != Leftˉslot ||
            !Matches(code, Cursor + 7, 0x48, 0x01, 0xC8))
        {
            return false;
        }
        Cursor += 10;
        if (!Matches(code, Cursor, 0x48, 0x39, 0xD0, 0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 5, out var Secondˉfallbackˉtarget) ||
            Secondˉfallbackˉtarget != Fallbackˉtarget)
        {
            return false;
        }
        Cursor += 9;
        if (!Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor,
                frameˉbytes,
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Tailˉrightˉlength) ||
            Tailˉrightˉlength != Rightˉslot ||
            !Matches(
                code,
                Cursor + 7,
                0x45, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x44, 0x89, 0xC9,
                0x01, 0xC1,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 18, out var Arenaˉtarget) ||
            !Matches(
                code,
                Cursor + 22,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 28, out var Secondˉarenaˉtarget) ||
            Secondˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 32,
                0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET) ||
            !Tryˉloadˉrax(code, Cursor + 36, frameˉbytes, out var Tailˉleft) ||
            Tailˉleft != Leftˉslot ||
            !Tryˉstoreˉrax(code, Cursor + 44, frameˉbytes, out var Tailˉresult) ||
            Tailˉresult != resultˉslot ||
            !Matches(code, Cursor + 52, 0x44, 0x89, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 55,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Tailˉlengthˉresult) ||
            Tailˉlengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 62, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 64,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Tailˉreservedˉresult) ||
            Tailˉreservedˉresult != resultˉslot ||
            !Matches(code, Cursor + 71, 0x49, 0x89, 0xD1))
        {
            return false;
        }
        Cursor += 74;
        if (!Tryˉdecodeˉbytesˉcopyˉloop(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉbytesˉslots,
                Rightˉslot) ||
            code[Cursor] != 0xE9 ||
            !Tryˉreadˉtarget(code, Cursor + 1, out var Tailˉendˉtarget))
        {
            return false;
        }
        Cursor += 5;
        if (Fallbackˉtarget != Cursor ||
            !Matches(
                code,
                Cursor,
                0x45, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x44, 0x89, 0xC1,
                0x81, 0xF9) ||
            Readˉi32(code, Cursor + 9) != Nativeˉcontract.DYNAMIC_BYTES_MINIMUM_OWNED_LENGTH ||
            !Matches(code, Cursor + 13,
                0x0F, 0x83) ||
            !Tryˉreadˉtarget(code, Cursor + 15, out var Ownedˉcapacityˉtarget) ||
            !Matches(code, Cursor + 19, 0x44, 0x89, 0xC8, 0x44, 0x01, 0xC0, 0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 27, out var Thirdˉarenaˉtarget) ||
            Thirdˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 31,
                0x41, 0x3B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 37, out var Fourthˉarenaˉtarget) ||
            Fourthˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 41,
                0x41, 0x89, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x44, 0x89, 0xC9,
                0x48, 0x01, 0xCA,
                0x48, 0x89, 0xD0) ||
            !Tryˉstoreˉrax(code, Cursor + 58, frameˉbytes, out var Exactˉresult) ||
            Exactˉresult != resultˉslot ||
            !Matches(code, Cursor + 66, 0x44, 0x89, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 69,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Exactˉlengthˉresult) ||
            Exactˉlengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 76, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 78,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Exactˉreservedˉresult) ||
            Exactˉreservedˉresult != resultˉslot ||
            !Matches(code, Cursor + 85, 0x49, 0x89, 0xD1, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 89, out var Copyˉfromˉexactˉtarget) ||
            Ownedˉcapacityˉtarget != Cursor + 93 ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 93,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Candidateˉmarkerˉresult) ||
            Candidateˉmarkerˉresult != resultˉslot ||
            !Matches(code, Cursor + 100, 0x85, 0xC0, 0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Cursor + 104, out var Promotedˉcapacityˉtarget) ||
            !Matches(code, Cursor + 108, 0x44, 0x89, 0xC1, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 112, out var Capacityˉreadyˉfromˉcandidate) ||
            Promotedˉcapacityˉtarget != Cursor + 116 ||
            !Matches(
                code,
                Cursor + 116,
                0x44, 0x89, 0xC1,
                0x81, 0xF9) ||
            Readˉi32(code, Cursor + 121) != Nativeˉcontract.DYNAMIC_BYTES_MAXIMUM_DOUBLED_LENGTH ||
            !Matches(code, Cursor + 125, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 127, out var Capacityˉceilingˉtarget) ||
            !Matches(code, Cursor + 131, 0x01, 0xC9, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 134, out var Capacityˉreadyˉfromˉdouble) ||
            Capacityˉceilingˉtarget != Cursor + 138 ||
            !Matches(code, Cursor + 138, 0x81, 0xF9) ||
            Readˉi32(code, Cursor + 140) != Nativeˉcontract.DYNAMIC_BYTES_MAXIMUM_CAPACITY ||
            !Matches(code, Cursor + 144, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 146, out var Capacityˉexactˉtarget) ||
            code[Cursor + 150] != 0xB9 ||
            Readˉi32(code, Cursor + 151) != Nativeˉcontract.DYNAMIC_BYTES_MAXIMUM_CAPACITY ||
            code[Cursor + 155] != 0xE9 ||
            !Tryˉreadˉtarget(code, Cursor + 156, out var Capacityˉreadyˉfromˉceiling) ||
            Capacityˉexactˉtarget != Cursor + 160 ||
            !Matches(code, Cursor + 160, 0x44, 0x89, 0xC1) ||
            Capacityˉreadyˉfromˉcandidate != Cursor + 163 ||
            Capacityˉreadyˉfromˉdouble != Cursor + 163 ||
            Capacityˉreadyˉfromˉceiling != Cursor + 163 ||
            !Matches(code, Cursor + 163, 0x89, 0xC8) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 165,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Fallbackˉcapacityˉresult) ||
            Fallbackˉcapacityˉresult != resultˉslot ||
            !Matches(
                code,
                Cursor + 172,
                0x44, 0x89, 0xC8,
                0x83, 0xC1, Nativeˉcontract.DYNAMIC_BYTES_HEADER_BYTES,
                0x01, 0xC1,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 182, out var Fifthˉarenaˉtarget) ||
            Fifthˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 186,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 192, out var Sixthˉarenaˉtarget) ||
            Sixthˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 196,
                0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x44, 0x89, 0xC9,
                0x48, 0x01, 0xCA) ||
            !Tryˉloadˉeaxˉatˉfield(
                code,
                Cursor + 210,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Storedˉfallbackˉcapacity) ||
            Storedˉfallbackˉcapacity != resultˉslot ||
            !Matches(code, Cursor + 217, 0x89, 0x02, 0xC7, 0x42, 0x04) ||
            Readˉi32(code, Cursor + 222) != checked((int)Nativeˉcontract.DYNAMIC_BYTES_FIRST_GENERATION) ||
            !Matches(
                code,
                Cursor + 226,
                0x48, 0x83, 0xC2, Nativeˉcontract.DYNAMIC_BYTES_HEADER_BYTES,
                0x48, 0x89, 0xD0) ||
            !Tryˉstoreˉrax(code, Cursor + 233, frameˉbytes, out var Fallbackˉresult) ||
            Fallbackˉresult != resultˉslot ||
            !Matches(code, Cursor + 241, 0x44, 0x89, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 244,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Fallbackˉlengthˉresult) ||
            Fallbackˉlengthˉresult != resultˉslot ||
            code[Cursor + 251] != 0xB8 ||
            Readˉi32(code, Cursor + 252) != checked((int)Nativeˉcontract.DYNAMIC_BYTES_FIRST_GENERATION) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 256,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Fallbackˉgenerationˉresult) ||
            Fallbackˉgenerationˉresult != resultˉslot ||
            !Matches(code, Cursor + 263, 0x49, 0x89, 0xD1, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 267, out var Copyˉfromˉownedˉtarget) ||
            Copyˉfromˉexactˉtarget != Cursor + 271 ||
            Copyˉfromˉownedˉtarget != Cursor + 271)
        {
            return false;
        }
        Cursor += 271;
        if (!Tryˉdecodeˉbytesˉcopyˉloop(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉbytesˉslots,
                Leftˉslot) ||
            !Tryˉdecodeˉbytesˉcopyˉloop(
                code,
                ref Cursor,
                frameˉbytes,
                borrowedˉbytesˉslots,
                Rightˉslot) ||
            code[Cursor] != 0xE9 ||
            !Tryˉreadˉtarget(code, Cursor + 1, out var Fallbackˉendˉtarget))
        {
            return false;
        }
        Cursor += 5;
        if (Limitˉtarget != Cursor ||
            !Tryˉdecodeˉruntimeˉfailure(
                code,
                ref Cursor,
                Nativeˉserviceˉfailureˉdetail.Bytesˉvalueˉlimit,
                runtimeˉservice) ||
            Arenaˉtarget != Cursor ||
            !Tryˉdecodeˉruntimeˉfailure(
                code,
                ref Cursor,
                Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
                runtimeˉservice) ||
            Growˉarenaˉtarget != Arenaˉtarget ||
            Ownedˉendˉtarget != Cursor ||
            Grownˉendˉtarget != Cursor ||
            Tailˉendˉtarget != Cursor ||
            Fallbackˉendˉtarget != Cursor ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉfromˉu32ˉlittle(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉbytesˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (Cursor < 0 ||
            Cursor > code.Length - 85 ||
            !Matches(
                code,
                Cursor,
                0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x41, 0x89, 0xC1,
                0x89, 0xC1,
                0x83, 0xC1, 0x04,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 14, out var Arenaˉtarget) ||
            !Matches(
                code,
                Cursor + 18,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 24, out var Secondˉarenaˉtarget) ||
            Secondˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 28,
                0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x4C, 0x01, 0xCA,
                0x48, 0x89, 0xD0) ||
            !Tryˉstoreˉrax(code, Cursor + 42, frameˉbytes, out resultˉslot) ||
            code[Cursor + 50] != 0xB8 ||
            Readˉi32(code, Cursor + 51) != sizeof(uint) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 55,
                frameˉbytes,
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉresult) ||
            Lengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 62, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 64,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Reservedˉresult) ||
            Reservedˉresult != resultˉslot ||
            !Tryˉloadˉeax(code, Cursor + 71, frameˉbytes, out var Valueˉslot) ||
            Valueˉslot == resultˉslot ||
            borrowedˉbytesˉslots.Contains(Valueˉslot) ||
            !Matches(code, Cursor + 78, 0x89, 0x02, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 81, out var Endˉtarget))
        {
            return false;
        }
        Cursor += 85;
        if (Arenaˉtarget != Cursor ||
            !Tryˉdecodeˉruntimeˉfailure(
                code,
                ref Cursor,
                Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
                runtimeˉservice) ||
            Endˉtarget != Cursor ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉfromˉu16ˉlittle(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉbytesˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (Cursor < 0 ||
            Cursor > code.Length - 104 ||
            !Tryˉloadˉeax(code, Cursor, frameˉbytes, out var Valueˉslot) ||
            borrowedˉbytesˉslots.Contains(Valueˉslot) ||
            code[Cursor + 7] != 0x3D ||
            Readˉi32(code, Cursor + 8) != ushort.MaxValue ||
            !Matches(code, Cursor + 12, 0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 14, out var Rangeˉtarget) ||
            !Matches(
                code,
                Cursor + 18,
                0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x41, 0x89, 0xC1,
                0x89, 0xC1,
                0x83, 0xC1, 0x02,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 32, out var Arenaˉtarget) ||
            !Matches(
                code,
                Cursor + 36,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 42, out var Secondˉarenaˉtarget) ||
            Secondˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 46,
                0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x4C, 0x01, 0xCA,
                0x48, 0x89, 0xD0) ||
            !Tryˉstoreˉrax(code, Cursor + 60, frameˉbytes, out resultˉslot) ||
            resultˉslot == Valueˉslot ||
            code[Cursor + 68] != 0xB8 ||
            Readˉi32(code, Cursor + 69) != sizeof(ushort) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 73,
                frameˉbytes,
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉresult) ||
            Lengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 80, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 82,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Reservedˉresult) ||
            Reservedˉresult != resultˉslot ||
            !Tryˉloadˉeax(code, Cursor + 89, frameˉbytes, out var Storedˉvalue) ||
            Storedˉvalue != Valueˉslot ||
            !Matches(code, Cursor + 96, 0x66, 0x89, 0x02, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 100, out var Endˉtarget))
        {
            return false;
        }
        Cursor += 104;
        if (Rangeˉtarget != Cursor ||
            !Tryˉdecodeˉruntimeˉfailure(
                code,
                ref Cursor,
                Nativeˉserviceˉfailureˉdetail.Bytesˉu16ˉoutˉofˉrange,
                runtimeˉservice) ||
            Arenaˉtarget != Cursor ||
            !Tryˉdecodeˉruntimeˉfailure(
                code,
                ref Cursor,
                Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
                runtimeˉservice) ||
            Endˉtarget != Cursor ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉfromˉu8(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        int runtimeˉservice,
        HashSet<int> borrowedˉbytesˉslots,
        out int resultˉslot)
    {
        resultˉslot = 0;
        var Cursor = index;
        if (Cursor < 0 ||
            Cursor > code.Length - 85 ||
            !Matches(
                code,
                Cursor,
                0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x41, 0x89, 0xC1,
                0x89, 0xC1,
                0x83, 0xC1, 0x01,
                0x0F, 0x82) ||
            !Tryˉreadˉtarget(code, Cursor + 14, out var Arenaˉtarget) ||
            !Matches(
                code,
                Cursor + 18,
                0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
                0x0F, 0x87) ||
            !Tryˉreadˉtarget(code, Cursor + 24, out var Secondˉarenaˉtarget) ||
            Secondˉarenaˉtarget != Arenaˉtarget ||
            !Matches(
                code,
                Cursor + 28,
                0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
                0x4C, 0x01, 0xCA,
                0x48, 0x89, 0xD0) ||
            !Tryˉstoreˉrax(code, Cursor + 42, frameˉbytes, out resultˉslot) ||
            code[Cursor + 50] != 0xB8 ||
            Readˉi32(code, Cursor + 51) != sizeof(byte) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 55,
                frameˉbytes,
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉresult) ||
            Lengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 62, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 64,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Reservedˉresult) ||
            Reservedˉresult != resultˉslot ||
            !Tryˉloadˉeax(code, Cursor + 71, frameˉbytes, out var Valueˉslot) ||
            Valueˉslot == resultˉslot ||
            borrowedˉbytesˉslots.Contains(Valueˉslot) ||
            !Matches(code, Cursor + 78, 0x88, 0x02, 0xE9) ||
            !Tryˉreadˉtarget(code, Cursor + 81, out var Endˉtarget))
        {
            return false;
        }
        Cursor += 85;
        if (Arenaˉtarget != Cursor ||
            !Tryˉdecodeˉruntimeˉfailure(
                code,
                ref Cursor,
                Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
                runtimeˉservice) ||
            Endˉtarget != Cursor ||
            Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉbytesˉcopyˉloop(
        ReadOnlySpan<byte> code,
        ref int index,
        int frameˉbytes,
        HashSet<int> borrowedˉbytesˉslots,
        int expectedˉsource)
    {
        var Cursor = index;
        if (!Tryˉloadˉrax(code, Cursor, frameˉbytes, out var Sourceˉslot) ||
            Sourceˉslot != expectedˉsource ||
            !borrowedˉbytesˉslots.Contains(Sourceˉslot) ||
            !Tryˉloadˉecxˉatˉfield(
                code,
                Cursor + 8,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET,
                out var Lengthˉsource) ||
            Lengthˉsource != Sourceˉslot ||
            !Matches(code, Cursor + 15, 0x85, 0xC9, 0x0F, 0x84) ||
            !Tryˉreadˉtarget(code, Cursor + 19, out var Endˉtarget))
        {
            return false;
        }
        var Loop = Cursor + 23;
        if (!Matches(
                code,
                Loop,
                0x44, 0x0F, 0xB6, 0x00,
                0x45, 0x88, 0x01,
                0x48, 0xFF, 0xC0,
                0x49, 0xFF, 0xC1,
                0xFF, 0xC9,
                0x0F, 0x85) ||
            !Tryˉreadˉtarget(code, Loop + 17, out var Loopˉtarget) ||
            Loopˉtarget != Loop ||
            Endˉtarget != Loop + 21)
        {
            return false;
        }
        index = Loop + 21;
        return true;
    }

    private static bool Tryˉdecodeˉruntimeˉfailure(
        ReadOnlySpan<byte> code,
        ref int index,
        Nativeˉserviceˉfailureˉdetail detail,
        int runtimeˉservice)
    {
        var Cursor = index;
        if (!Matches(
                code,
                Cursor,
                0x41, 0xC7, 0x47,
                Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET) ||
            Readˉi32(code, Cursor + 4) != checked((int)detail) ||
            code[Cursor + 8] != 0xE9 ||
            !Tryˉreadˉtarget(code, Cursor + 9, out var Runtimeˉtarget) ||
            Runtimeˉtarget != runtimeˉservice)
        {
            return false;
        }
        index = Cursor + 13;
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
            !Tryˉstoreˉrax(code, Cursor + 8, frameˉbytes, out targetˉslot))
        {
            return false;
        }
        if (!Tryˉloadˉraxˉatˉfield(
                code,
                Cursor + 16,
                frameˉbytes,
                sizeof(ulong),
                out var Highˉsource) ||
            Highˉsource != sourceˉslot ||
            !Tryˉstoreˉraxˉatˉfield(
                code,
                Cursor + 24,
                frameˉbytes,
                sizeof(ulong),
                out var Highˉtarget) ||
            Highˉtarget != targetˉslot)
        {
            return false;
        }
        Cursor += 32;
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
            Lengthˉresult != resultˉslot ||
            !Matches(code, Cursor + 79, 0x31, 0xC0) ||
            !Tryˉstoreˉeaxˉatˉfield(
                code,
                Cursor + 81,
                frameˉbytes,
                Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
                out var Reservedˉresult) ||
            Reservedˉresult != resultˉslot)
        {
            return false;
        }
        Cursor += 88;
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
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        HashSet<int> borrowedˉdescriptorˉslots,
        out int resultˉslot,
        out int backingˉslot)
    {
        resultˉslot = 0;
        backingˉslot = 0;
        var Cursor = index;
        if (!Tryˉdecodeˉrecordˉtypeˉtag(
            code,
            Cursor,
            types,
            out _,
            out var Record))
        {
            return false;
        }
        Cursor += 6;
        for (var Field = 0; Field < Record.Fields.Length; Field++)
        {
            var Isˉdescriptor = Record.Fields[Field].Type.Kind is
                Valueˉtype.Text or Valueˉtype.Bytes;
            if (!Tryˉloadˉrax(code, Cursor, frameˉbytes, out var Sourceˉslot) ||
                Isˉdescriptor != borrowedˉdescriptorˉslots.Contains(Sourceˉslot) ||
                !Tryˉstoreˉrax(code, Cursor + 8, frameˉbytes, out var Lowˉtarget) ||
                !Tryˉloadˉraxˉatˉfield(
                    code,
                    Cursor + 16,
                    frameˉbytes,
                    sizeof(ulong),
                    out var Highˉsource) ||
                Highˉsource != Sourceˉslot ||
                !Tryˉstoreˉraxˉatˉfield(
                    code,
                    Cursor + 24,
                    frameˉbytes,
                    sizeof(ulong),
                    out var Highˉtarget) ||
                Highˉtarget != Lowˉtarget ||
                (Field == 0 ? (backingˉslot = Lowˉtarget) : Lowˉtarget) !=
                    checked(backingˉslot + Field))
            {
                return false;
            }
            Cursor += 32;
        }
        if (!Matches(code, Cursor, 0x48, 0x8D, 0x84, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out var Addressˉslot) ||
            Addressˉslot != backingˉslot ||
            !Tryˉstoreˉrax(code, Cursor + 8, frameˉbytes, out resultˉslot) ||
            !Rangeˉfitsˉframe(backingˉslot, Record.Fields.Length, frameˉbytes) ||
            Isˉcellˉinˉrange(resultˉslot, backingˉslot, Record.Fields.Length))
        {
            return false;
        }
        Cursor += 16;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉrecordˉcopy(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        HashSet<int> recordˉslots,
        out int sourceˉslot,
        out int targetˉslot,
        out int backingˉslot)
    {
        sourceˉslot = 0;
        targetˉslot = 0;
        backingˉslot = 0;
        var Cursor = index;
        if (!Tryˉdecodeˉrecordˉtypeˉtag(
                code,
                Cursor,
                types,
                out _,
                out var Record) ||
            !Tryˉloadˉrdx(code, Cursor + 6, frameˉbytes, out sourceˉslot) ||
            !recordˉslots.Contains(sourceˉslot))
        {
            return false;
        }
        Cursor += 14;
        for (var Field = 0; Field < Record.Fields.Length; Field++)
        {
            var Sourceˉoffset = checked(Field * Nativeˉcontract.VALUE_SLOT_BYTES);
            if (!Matches(code, Cursor, 0x48, 0x8B, 0x82) ||
                Readˉi32(code, Cursor + 3) != Sourceˉoffset ||
                !Tryˉstoreˉrax(code, Cursor + 7, frameˉbytes, out var Lowˉtarget) ||
                !Matches(code, Cursor + 15, 0x48, 0x8B, 0x82) ||
                Readˉi32(code, Cursor + 18) != checked(Sourceˉoffset + sizeof(ulong)) ||
                !Tryˉstoreˉraxˉatˉfield(
                    code,
                    Cursor + 22,
                    frameˉbytes,
                    sizeof(ulong),
                    out var Highˉtarget) ||
                Highˉtarget != Lowˉtarget ||
                (Field == 0 ? (backingˉslot = Lowˉtarget) : Lowˉtarget) !=
                    checked(backingˉslot + Field))
            {
                return false;
            }
            Cursor += 30;
        }
        if (!Matches(code, Cursor, 0x48, 0x8D, 0x84, 0x24) ||
            !Tryˉreadˉslot(code, Cursor + 4, frameˉbytes, out var Addressˉslot) ||
            Addressˉslot != backingˉslot ||
            !Tryˉstoreˉrax(code, Cursor + 8, frameˉbytes, out targetˉslot) ||
            !Rangeˉfitsˉframe(backingˉslot, Record.Fields.Length, frameˉbytes) ||
            Isˉcellˉinˉrange(targetˉslot, backingˉslot, Record.Fields.Length))
        {
            return false;
        }
        Cursor += 16;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉrecordˉreferenceˉcopy(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        HashSet<int> recordˉslots,
        out int targetˉslot)
    {
        targetˉslot = 0;
        if (!Tryˉdecodeˉrecordˉtypeˉtag(code, index, types, out _, out _) ||
            !Tryˉloadˉrax(code, index + 6, frameˉbytes, out var Sourceˉslot) ||
            !recordˉslots.Contains(Sourceˉslot) ||
            !Tryˉstoreˉrax(code, index + 14, frameˉbytes, out targetˉslot) ||
            index + 22 > end)
        {
            return false;
        }
        index += 22;
        return true;
    }

    private static bool Tryˉdecodeˉrecordˉfield(
        ReadOnlySpan<byte> code,
        ref int index,
        int end,
        int frameˉbytes,
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        HashSet<int> recordˉslots,
        out int resultˉslot,
        out bool isˉdescriptor)
    {
        resultˉslot = 0;
        isˉdescriptor = false;
        var Cursor = index;
        if (!Tryˉdecodeˉrecordˉtypeˉtag(
                code,
                Cursor,
                types,
                out _,
                out var Record) ||
            !Tryˉloadˉrdx(code, Cursor + 6, frameˉbytes, out var Recordˉslot) ||
            !recordˉslots.Contains(Recordˉslot))
        {
            return false;
        }
        Cursor += 14;
        if (!Matches(code, Cursor, 0x48, 0x8B, 0x82))
        {
            return false;
        }
        var Sourceˉoffset = Readˉi32(code, Cursor + 3);
        if (Sourceˉoffset < 0 ||
            Sourceˉoffset % Nativeˉcontract.VALUE_SLOT_BYTES != 0)
        {
            return false;
        }
        var Fieldˉindex = Sourceˉoffset / Nativeˉcontract.VALUE_SLOT_BYTES;
        if ((uint)Fieldˉindex >= (uint)Record.Fields.Length ||
            !Tryˉstoreˉrax(code, Cursor + 7, frameˉbytes, out resultˉslot) ||
            !Matches(code, Cursor + 15, 0x48, 0x8B, 0x82) ||
            Readˉi32(code, Cursor + 18) != checked(Sourceˉoffset + sizeof(ulong)) ||
            !Tryˉstoreˉraxˉatˉfield(
                code,
                Cursor + 22,
                frameˉbytes,
                sizeof(ulong),
                out var Highˉresult) ||
            Highˉresult != resultˉslot)
        {
            return false;
        }
        Cursor += 30;
        isˉdescriptor = Record.Fields[Fieldˉindex].Type.Kind is
            Valueˉtype.Text or Valueˉtype.Bytes;
        if (Cursor > end)
        {
            return false;
        }
        index = Cursor;
        return true;
    }

    private static bool Tryˉdecodeˉrecordˉreturnˉfields(
        ReadOnlySpan<byte> code,
        int index,
        int end,
        int fieldˉcount,
        out int after)
    {
        after = index;
        for (var Field = 0; Field < fieldˉcount; Field++)
        {
            var Fieldˉoffset = checked(Field * Nativeˉcontract.VALUE_SLOT_BYTES);
            if (!Matches(code, after, 0x48, 0x8B, 0x88) ||
                Readˉi32(code, after + 3) != Fieldˉoffset ||
                !Matches(code, after + 7, 0x48, 0x89, 0x8A) ||
                Readˉi32(code, after + 10) != Fieldˉoffset ||
                !Matches(code, after + 14, 0x48, 0x8B, 0x88) ||
                Readˉi32(code, after + 17) != checked(Fieldˉoffset + sizeof(ulong)) ||
                !Matches(code, after + 21, 0x48, 0x89, 0x8A) ||
                Readˉi32(code, after + 24) != checked(Fieldˉoffset + sizeof(ulong)))
            {
                return false;
            }
            after += 28;
        }
        return after <= end;
    }

    private static bool Tryˉdecodeˉrecordˉtypeˉtag(
        ReadOnlySpan<byte> code,
        int offset,
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        out int type,
        out Recordˉtypeˉdeclaration record)
    {
        type = -1;
        record = null!;
        if (!Matches(code, offset, 0x41, 0xB8))
        {
            return false;
        }
        type = Readˉi32(code, offset + 2);
        if ((uint)type >= (uint)types.Length ||
            types[type] is not Recordˉtypeˉdeclaration Record ||
            Record.Fields.IsDefaultOrEmpty ||
            Record.Fields.Any(Field => Field.Type.Kind == Valueˉtype.Record))
        {
            return false;
        }
        record = Record;
        return true;
    }

    private static bool Rangeˉfitsˉframe(int startˉcell, int cells, int frameˉbytes) =>
        startˉcell >= 0 &&
        cells > 0 &&
        ((long)startˉcell + cells) * Nativeˉcontract.VALUE_SLOT_BYTES <= frameˉbytes;

    private static bool Isˉcellˉinˉrange(int cell, int start, int cells) =>
        cell >= start && cell < checked(start + cells);

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
        var Recordˉprefix = argument switch
        {
            0 => new byte[] { 0x4C, 0x8B, 0x84, 0x24 },
            1 => new byte[] { 0x4C, 0x8B, 0x8C, 0x24 },
            2 => new byte[] { 0x48, 0x8B, 0x8C, 0x24 },
            3 => new byte[] { 0x48, 0x8B, 0x94, 0x24 },
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
        length = Recordˉprefix.Length + sizeof(int);
        kind = Decodedˉargumentˉkind.Record;
        if (Recordˉprefix.Length != 0 &&
            Matches(code, offset, Recordˉprefix) &&
            Tryˉreadˉslot(code, offset + Recordˉprefix.Length, frameˉbytes, out slot))
        {
            return true;
        }
        length = Bytesˉprefix.Length + sizeof(int);
        kind = Decodedˉargumentˉkind.Borrowedˉbytes;
        return Bytesˉprefix.Length != 0 &&
            Matches(code, offset, Bytesˉprefix) &&
            Tryˉreadˉslot(code, offset + Bytesˉprefix.Length, frameˉbytes, out slot);
    }

    private static bool Tryˉloadˉstackˉargument(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int stackˉbytes,
        int argument,
        out int length,
        out Decodedˉargumentˉkind kind,
        out int slot)
    {
        length = 14;
        kind = Decodedˉargumentˉkind.Scalar;
        slot = 0;
        if (offset < 0 || offset > code.Length - 14)
        {
            return false;
        }
        var Outgoingˉoffset = checked(
            (argument - Nativeˉcontract.REGISTER_CALL_PARAMETERS) *
            Nativeˉcontract.VALUE_SLOT_BYTES);
        if (Matches(code, offset, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉadjustedˉslot(
                code,
                offset + 3,
                frameˉbytes,
                stackˉbytes,
                out slot) &&
            Matches(code, offset + 7, 0x89, 0x84, 0x24) &&
            Readˉi32(code, offset + 10) == Outgoingˉoffset)
        {
            return true;
        }

        length = 32;
        kind = Decodedˉargumentˉkind.Borrowedˉbytes;
        if (offset <= code.Length - 32 &&
            Matches(code, offset, 0x48, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉadjustedˉslot(
                code,
                offset + 4,
                frameˉbytes,
                stackˉbytes,
                out slot) &&
            Matches(code, offset + 8, 0x48, 0x89, 0x84, 0x24) &&
            Readˉi32(code, offset + 12) == Outgoingˉoffset &&
            Matches(code, offset + 16, 0x48, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉadjustedˉslotˉfield(
                code,
                offset + 20,
                frameˉbytes,
                stackˉbytes,
                sizeof(ulong),
                sizeof(ulong),
                out var Highˉslot) &&
            Highˉslot == slot &&
            Matches(code, offset + 24, 0x48, 0x89, 0x84, 0x24) &&
            Readˉi32(code, offset + 28) == checked(Outgoingˉoffset + sizeof(ulong)))
        {
            return true;
        }

        length = 16;
        kind = Decodedˉargumentˉkind.Record;
        return offset <= code.Length - 16 &&
            Matches(code, offset, 0x48, 0x8B, 0x84, 0x24) &&
            Tryˉreadˉadjustedˉslot(
                code,
                offset + 4,
                frameˉbytes,
                stackˉbytes,
                out slot) &&
            Matches(code, offset + 8, 0x48, 0x89, 0x84, 0x24) &&
            Readˉi32(code, offset + 12) == Outgoingˉoffset;
    }

    private static bool Tryˉstoreˉargument(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int argument,
        out int length,
        out Decodedˉargumentˉkind kind)
    {
        if (argument >= Nativeˉcontract.REGISTER_CALL_PARAMETERS)
        {
            var Incomingˉoffset = checked(
                frameˉbytes + sizeof(ulong) +
                (argument - Nativeˉcontract.REGISTER_CALL_PARAMETERS) *
                Nativeˉcontract.VALUE_SLOT_BYTES);
            length = 14;
            kind = Decodedˉargumentˉkind.Scalar;
            if (offset >= 0 &&
                offset <= code.Length - 14 &&
                Matches(code, offset, 0x8B, 0x84, 0x24) &&
                Readˉi32(code, offset + 3) == Incomingˉoffset &&
                Tryˉstoreˉeax(code, offset + 7, frameˉbytes, out var Scalarˉslot) &&
                Scalarˉslot == argument)
            {
                return true;
            }

            length = 32;
            kind = Decodedˉargumentˉkind.Borrowedˉbytes;
            if (offset >= 0 &&
                offset <= code.Length - 32 &&
                Matches(code, offset, 0x48, 0x8B, 0x84, 0x24) &&
                Readˉi32(code, offset + 4) == Incomingˉoffset &&
                Tryˉstoreˉrax(code, offset + 8, frameˉbytes, out var Stackˉpointerˉslot) &&
                Stackˉpointerˉslot == argument &&
                Matches(code, offset + 16, 0x48, 0x8B, 0x84, 0x24) &&
                Readˉi32(code, offset + 20) == checked(Incomingˉoffset + sizeof(ulong)) &&
                Tryˉstoreˉraxˉatˉfield(
                    code,
                    offset + 24,
                    frameˉbytes,
                    sizeof(ulong),
                    out var Stackˉhighˉslot) &&
                Stackˉhighˉslot == argument)
            {
                return true;
            }

            length = 16;
            kind = Decodedˉargumentˉkind.Record;
            return offset >= 0 &&
                offset <= code.Length - 16 &&
                Matches(code, offset, 0x48, 0x8B, 0x84, 0x24) &&
                Readˉi32(code, offset + 4) == Incomingˉoffset &&
                Tryˉstoreˉrax(code, offset + 8, frameˉbytes, out var Stackˉrecordˉslot) &&
                Stackˉrecordˉslot == argument;
        }

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

        var Storeˉrecord = argument switch
        {
            0 => new byte[] { 0x4C, 0x89, 0x84, 0x24 },
            1 => new byte[] { 0x4C, 0x89, 0x8C, 0x24 },
            2 => new byte[] { 0x48, 0x89, 0x8C, 0x24 },
            3 => new byte[] { 0x48, 0x89, 0x94, 0x24 },
            _ => [],
        };
        length = Storeˉrecord.Length + sizeof(int);
        kind = Decodedˉargumentˉkind.Record;
        if (Storeˉrecord.Length != 0 &&
            Matches(code, offset, Storeˉrecord) &&
            Tryˉreadˉslot(code, offset + Storeˉrecord.Length, frameˉbytes, out var Recordˉslot) &&
            Recordˉslot == argument)
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
        var Loadˉhigh = argument switch
        {
            0 => new byte[] { 0x49, 0x8B, 0x40, sizeof(ulong) },
            1 => new byte[] { 0x49, 0x8B, 0x41, sizeof(ulong) },
            2 => new byte[] { 0x48, 0x8B, 0x41, sizeof(ulong) },
            3 => new byte[] { 0x48, 0x8B, 0x42, sizeof(ulong) },
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
        if (!Matches(code, Cursor, Loadˉhigh) ||
            !Tryˉstoreˉraxˉatˉfield(
                code,
                Cursor + Loadˉhigh.Length,
                frameˉbytes,
                sizeof(ulong),
                out var Highˉslot) ||
            Highˉslot != argument)
        {
            return false;
        }
        Cursor += Loadˉhigh.Length + 8;
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

    private static bool Tryˉreadˉadjustedˉslot(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int stackˉadjustment,
        out int slot) =>
        Tryˉreadˉadjustedˉslotˉfield(
            code,
            offset,
            frameˉbytes,
            stackˉadjustment,
            0,
            Nativeˉcontract.VALUE_SLOT_BYTES,
            out slot);

    private static bool Tryˉreadˉadjustedˉslotˉfield(
        ReadOnlySpan<byte> code,
        int offset,
        int frameˉbytes,
        int stackˉadjustment,
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
        var Adjustedˉdisplacement = (long)Displacement - stackˉadjustment;
        if (stackˉadjustment < 0 ||
            field < 0 ||
            width < 1 ||
            field > Nativeˉcontract.VALUE_SLOT_BYTES - width ||
            Adjustedˉdisplacement < field ||
            (Adjustedˉdisplacement - field) % Nativeˉcontract.VALUE_SLOT_BYTES != 0 ||
            Adjustedˉdisplacement > frameˉbytes - width)
        {
            return false;
        }
        slot = checked((int)((Adjustedˉdisplacement - field) / Nativeˉcontract.VALUE_SLOT_BYTES));
        return true;
    }

    private static int Stackˉcallˉbytes(int parameters) =>
        checked(
            Math.Max(0, parameters - Nativeˉcontract.REGISTER_CALL_PARAMETERS) *
            Nativeˉcontract.VALUE_SLOT_BYTES);

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
        Record = 3,
    }

    private enum Decodedˉreturnˉkind : byte
    {
        Void = 0,
        Scalar = 1,
        Descriptor = 2,
        Record = 3,
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
        Fail(
            "WVN3030",
            "The x86-64 baseline fragment is outside the independently decoded context, " +
            "service, call, record, and data target shape.");

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
