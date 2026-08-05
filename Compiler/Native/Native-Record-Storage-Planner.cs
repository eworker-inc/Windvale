using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.Bytecode;

namespace Windvale.Compiler.Native;

// Published offsets are absolute 16-byte cells in the projected frame; -1 means no owned backing.
public sealed record Nativeˉfunctionˉrecordˉstorage(
    int Functionˉindex,
    string Functionˉname,
    int Existingˉframeˉcells,
    int Recordˉparameterˉbindings,
    int Assignedˉrecordˉparameterˉbindings,
    int Recordˉlocalˉbindings,
    int Declaredˉrecordˉlocalˉfieldˉcells,
    int Persistentˉrecordˉbaseˉcell,
    int Persistentˉrecordˉfieldˉcells,
    ImmutableArray<int> Localˉrecordˉfieldˉoffsets,
    int Recordˉvalueˉidentifiers,
    int Recordˉvalueˉslots,
    int Blockˉrecordˉfieldˉcells,
    int Peakˉliveˉrecordˉfieldˉcells,
    int Scratchˉrecordˉbaseˉcell,
    int Scratchˉrecordˉfieldˉcells,
    ImmutableArray<int> Valueˉrecordˉfieldˉoffsets,
    bool Returnsˉrecord,
    int Recordˉreturnˉfieldˉcells,
    int Recordˉreturnˉpointerˉcell,
    int Projectedˉframeˉcells,
    int Maximumˉrecordˉfieldˉcells,
    bool Containsˉnestedˉrecordˉfields);

public static class Nativeˉrecordˉstorageˉplanner
{
    public static ImmutableArray<Nativeˉfunctionˉrecordˉstorage> Measure(
        Nativeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (module.Functions.IsDefault || module.Types.IsDefault)
        {
            Fail("The native record-storage planner requires initialized module metadata.");
        }

        return module.Functions
            .Select((Function, Index) => Measureˉfunction(module, Function, Index))
            .ToImmutableArray();
    }

    private static Nativeˉfunctionˉrecordˉstorage Measureˉfunction(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex)
    {
        if (function is null ||
            function.Parameterˉtypes.IsDefault ||
            function.Localˉtypes.IsDefault ||
            function.Allˉlocalˉnominalˉtypeˉindices.IsDefault ||
            function.Valueˉtypes.IsDefault ||
            function.Valueˉnominalˉtypeˉindices.IsDefault ||
            function.Valueˉslotˉindices.IsDefault ||
            function.Blocks.IsDefaultOrEmpty ||
            function.Valueˉslotˉcount < 0 ||
            function.Allˉlocalˉnominalˉtypeˉindices.Length != function.Allˉlocalˉtypes.Length ||
            function.Valueˉnominalˉtypeˉindices.Length != function.Valueˉtypes.Length ||
            function.Valueˉslotˉindices.Length != function.Valueˉtypes.Length)
        {
            Fail("The native record-storage planner received inconsistent function metadata.");
        }

        var Recordˉparameterˉbindings = 0;
        var Storedˉrecordˉparameters = function.Blocks
            .SelectMany(Block => Block.Operations)
            .OfType<Nativeˉlocalˉstore>()
            .Where(Store =>
                Store.Local >= 0 &&
                Store.Local < function.Parameterˉtypes.Length &&
                Store.Type == Nativeˉvalueˉtype.Record)
            .Select(Store => Store.Local)
            .ToHashSet();
        var Recordˉlocalˉbindings = 0;
        var Declaredˉrecordˉlocalˉfieldˉcells = 0;
        var Maximumˉrecordˉfieldˉcells = 0;
        var Containsˉnestedˉrecordˉfields = false;
        for (var Local = 0; Local < function.Allˉlocalˉtypes.Length; Local++)
        {
            if (function.Allˉlocalˉtypes[Local] != Nativeˉvalueˉtype.Record)
            {
                continue;
            }
            var Record = Requireˉrecord(
                module,
                function.Allˉlocalˉnominalˉtypeˉindices[Local]);
            if (Local < function.Parameterˉtypes.Length)
            {
                Recordˉparameterˉbindings++;
                if (Storedˉrecordˉparameters.Contains(Local))
                {
                    Declaredˉrecordˉlocalˉfieldˉcells = checked(
                        Declaredˉrecordˉlocalˉfieldˉcells + Record.Fields.Length);
                }
            }
            else
            {
                Recordˉlocalˉbindings++;
                Declaredˉrecordˉlocalˉfieldˉcells = checked(
                    Declaredˉrecordˉlocalˉfieldˉcells + Record.Fields.Length);
            }
            Maximumˉrecordˉfieldˉcells = Math.Max(
                Maximumˉrecordˉfieldˉcells,
                Record.Fields.Length);
            Containsˉnestedˉrecordˉfields |= Record.Fields.Any(
                Field => Field.Type.Kind == Valueˉtype.Record);
        }
        var Persistentˉallocation = Planˉpersistentˉrecordˉfields(
            module,
            function,
            Storedˉrecordˉparameters);

        var Recordˉvalueˉidentifiers = 0;
        var Recordˉslotˉwidths = new Dictionary<int, int>();
        for (var Value = 0; Value < function.Valueˉtypes.Length; Value++)
        {
            if (function.Valueˉtypes[Value] != Nativeˉvalueˉtype.Record)
            {
                continue;
            }
            var Record = Requireˉrecord(
                module,
                function.Valueˉnominalˉtypeˉindices[Value]);
            var Slot = function.Valueˉslotˉindices[Value];
            if (Slot is < 0 || Slot >= function.Valueˉslotˉcount)
            {
                Fail("The native record-storage planner received an invalid physical value slot.");
            }
            Recordˉvalueˉidentifiers++;
            Recordˉslotˉwidths[Slot] = Math.Max(
                Recordˉslotˉwidths.GetValueOrDefault(Slot),
                Record.Fields.Length);
            Maximumˉrecordˉfieldˉcells = Math.Max(
                Maximumˉrecordˉfieldˉcells,
                Record.Fields.Length);
            Containsˉnestedˉrecordˉfields |= Record.Fields.Any(
                Field => Field.Type.Kind == Valueˉtype.Record);
        }

        var Returnsˉrecord = function.Returnˉtype == Nativeˉvalueˉtype.Record;
        var Recordˉreturnˉfieldˉcells = 0;
        if (Returnsˉrecord)
        {
            var Record = Requireˉrecord(module, function.Returnˉnominalˉtypeˉindex);
            Recordˉreturnˉfieldˉcells = Record.Fields.Length;
            Maximumˉrecordˉfieldˉcells = Math.Max(
                Maximumˉrecordˉfieldˉcells,
                Record.Fields.Length);
            Containsˉnestedˉrecordˉfields |= Record.Fields.Any(
                Field => Field.Type.Kind == Valueˉtype.Record);
        }

        var Blockˉrecordˉfieldˉcells = Recordˉslotˉwidths.Values.Aggregate(
            0,
            (Total, Width) => checked(Total + Width));
        var Valueˉallocation = Planˉrecordˉvalueˉfields(
            module,
            function,
            Recordˉvalueˉidentifiers);
        var Existingˉhiddenˉresultˉcells = function.Returnˉtype is
            Nativeˉvalueˉtype.Borrowedˉtext or Nativeˉvalueˉtype.Borrowedˉbytes
                ? 1
                : 0;
        var Projectedˉhiddenˉresultˉcells = function.Returnˉtype is
            Nativeˉvalueˉtype.Borrowedˉtext or
            Nativeˉvalueˉtype.Borrowedˉbytes or
            Nativeˉvalueˉtype.Record
                ? 1
                : 0;
        var Existingˉframeˉcells = checked(
            function.Allˉlocalˉtypes.Length +
            function.Valueˉslotˉcount +
            Existingˉhiddenˉresultˉcells);
        var Persistentˉrecordˉbaseˉcell = Existingˉframeˉcells;
        var Scratchˉrecordˉbaseˉcell = checked(
            Persistentˉrecordˉbaseˉcell + Persistentˉallocation.Requiredˉfieldˉcells);
        var Recordˉreturnˉpointerˉcell = Returnsˉrecord
            ? checked(Scratchˉrecordˉbaseˉcell + Valueˉallocation.Requiredˉfieldˉcells)
            : -1;
        var Projectedˉframeˉcells = Returnsˉrecord
            ? checked(Recordˉreturnˉpointerˉcell + 1)
            : checked(Scratchˉrecordˉbaseˉcell + Valueˉallocation.Requiredˉfieldˉcells);
        if (Projectedˉhiddenˉresultˉcells - Existingˉhiddenˉresultˉcells !=
            (Returnsˉrecord ? 1 : 0))
        {
            Fail("The native record-storage planner derived an inconsistent result-cell layout.");
        }
        var Localˉrecordˉfieldˉoffsets = Persistentˉallocation.Offsets
            .Select(Offset => Offset < 0 ? -1 : checked(Persistentˉrecordˉbaseˉcell + Offset))
            .ToImmutableArray();
        var Valueˉrecordˉfieldˉoffsets = Valueˉallocation.Offsets
            .Select(Offset => Offset < 0 ? -1 : checked(Scratchˉrecordˉbaseˉcell + Offset))
            .ToImmutableArray();

        return new(
            functionˉindex,
            function.Name,
            Existingˉframeˉcells,
            Recordˉparameterˉbindings,
            Storedˉrecordˉparameters.Count,
            Recordˉlocalˉbindings,
            Declaredˉrecordˉlocalˉfieldˉcells,
            Persistentˉrecordˉbaseˉcell,
            Persistentˉallocation.Requiredˉfieldˉcells,
            Localˉrecordˉfieldˉoffsets,
            Recordˉvalueˉidentifiers,
            Recordˉslotˉwidths.Count,
            Blockˉrecordˉfieldˉcells,
            Valueˉallocation.Peakˉliveˉfieldˉcells,
            Scratchˉrecordˉbaseˉcell,
            Valueˉallocation.Requiredˉfieldˉcells,
            Valueˉrecordˉfieldˉoffsets,
            Returnsˉrecord,
            Recordˉreturnˉfieldˉcells,
            Recordˉreturnˉpointerˉcell,
            Projectedˉframeˉcells,
            Maximumˉrecordˉfieldˉcells,
            Containsˉnestedˉrecordˉfields);
    }

    private static Nativeˉrecordˉoffsetˉallocation Planˉpersistentˉrecordˉfields(
        Nativeˉmodule module,
        Nativeˉfunction function,
        IReadOnlySet<int> storedˉrecordˉparameters)
    {
        var Blockˉindices = new Dictionary<int, int>();
        for (var Index = 0; Index < function.Blocks.Length; Index++)
        {
            if (!Blockˉindices.TryAdd(function.Blocks[Index].Id, Index))
            {
                Fail("The native record-storage planner received duplicate block identifiers.");
            }
        }

        var Uses = new HashSet<int>[function.Blocks.Length];
        var Definitions = new HashSet<int>[function.Blocks.Length];
        var Successors = new int[function.Blocks.Length][];
        for (var Blockˉindex = 0; Blockˉindex < function.Blocks.Length; Blockˉindex++)
        {
            var Use = new HashSet<int>();
            var Definition = new HashSet<int>();
            foreach (var Operation in function.Blocks[Blockˉindex].Operations)
            {
                switch (Operation)
                {
                    case Nativeˉlocalˉload Load when
                        Isˉframeˉrecordˉlocal(
                            function,
                            Load.Local,
                            Load.Type,
                            storedˉrecordˉparameters):
                        if (!Definition.Contains(Load.Local))
                        {
                            Use.Add(Load.Local);
                        }
                        break;
                    case Nativeˉlocalˉstore Store when
                        Isˉframeˉrecordˉlocal(
                            function,
                            Store.Local,
                            Store.Type,
                            storedˉrecordˉparameters):
                        Definition.Add(Store.Local);
                        break;
                }
            }
            Uses[Blockˉindex] = Use;
            Definitions[Blockˉindex] = Definition;
            Successors[Blockˉindex] = function.Blocks[Blockˉindex].Terminator switch
            {
                Nativeˉjump Jump => [Requireˉblock(Blockˉindices, Jump.Targetˉblock)],
                Nativeˉbranch Branch =>
                [
                    Requireˉblock(Blockˉindices, Branch.Trueˉblock),
                    Requireˉblock(Blockˉindices, Branch.Falseˉblock),
                ],
                Nativeˉreturn or Nativeˉreturnˉvoid => [],
                _ => throw new Nativeˉbackendˉexception(
                    "WVN2901",
                    "The native record-storage planner received an unsupported terminator."),
            };
        }

        var Liveˉin = Enumerable.Range(0, function.Blocks.Length)
            .Select(_ => new HashSet<int>())
            .ToArray();
        var Liveˉout = Enumerable.Range(0, function.Blocks.Length)
            .Select(_ => new HashSet<int>())
            .ToArray();
        bool Changed;
        do
        {
            Changed = false;
            for (var Blockˉindex = function.Blocks.Length - 1; Blockˉindex >= 0; Blockˉindex--)
            {
                var Newˉout = new HashSet<int>();
                foreach (var Successor in Successors[Blockˉindex])
                {
                    Newˉout.UnionWith(Liveˉin[Successor]);
                }
                var Newˉin = new HashSet<int>(Newˉout);
                Newˉin.ExceptWith(Definitions[Blockˉindex]);
                Newˉin.UnionWith(Uses[Blockˉindex]);
                if (!Newˉout.SetEquals(Liveˉout[Blockˉindex]) ||
                    !Newˉin.SetEquals(Liveˉin[Blockˉindex]))
                {
                    Liveˉout[Blockˉindex] = Newˉout;
                    Liveˉin[Blockˉindex] = Newˉin;
                    Changed = true;
                }
            }
        }
        while (Changed);

        var Interference = new Dictionary<int, HashSet<int>>();
        void Addˉinterference(HashSet<int> live)
        {
            foreach (var Local in live)
            {
                if (!Interference.ContainsKey(Local))
                {
                    Interference.Add(Local, []);
                }
                foreach (var Other in live)
                {
                    if (Local != Other)
                    {
                        Interference[Local].Add(Other);
                    }
                }
            }
        }

        void Addˉstoreˉinterference(int local, HashSet<int> live)
        {
            if (!Interference.TryGetValue(local, out var Neighbors))
            {
                Neighbors = [];
                Interference.Add(local, Neighbors);
            }
            foreach (var Other in live)
            {
                if (Other == local)
                {
                    continue;
                }
                if (!Interference.TryGetValue(Other, out var Otherˉneighbors))
                {
                    Otherˉneighbors = [];
                    Interference.Add(Other, Otherˉneighbors);
                }
                Neighbors.Add(Other);
                Otherˉneighbors.Add(local);
            }
        }

        for (var Blockˉindex = 0; Blockˉindex < function.Blocks.Length; Blockˉindex++)
        {
            var Live = new HashSet<int>(Liveˉout[Blockˉindex]);
            Addˉinterference(Live);
            var Operations = function.Blocks[Blockˉindex].Operations;
            for (var Operationˉindex = Operations.Length - 1; Operationˉindex >= 0; Operationˉindex--)
            {
                switch (Operations[Operationˉindex])
                {
                    case Nativeˉlocalˉstore Store when
                        Isˉframeˉrecordˉlocal(
                            function,
                            Store.Local,
                            Store.Type,
                            storedˉrecordˉparameters):
                        // The store writes the local's fixed backing even when this
                        // particular definition is dead. Keep it separate from every
                        // other record local whose value is live across the write.
                        Addˉstoreˉinterference(Store.Local, Live);
                        Live.Remove(Store.Local);
                        break;
                    case Nativeˉlocalˉload Load when
                        Isˉframeˉrecordˉlocal(
                            function,
                            Load.Local,
                            Load.Type,
                            storedˉrecordˉparameters):
                        Live.Add(Load.Local);
                        break;
                }
                Addˉinterference(Live);
            }
        }

        return Allocateˉrecordˉoffsets(
            function.Allˉlocalˉtypes.Length,
            Interference,
            Local => Recordˉlocalˉwidth(module, function, Local));
    }

    private static bool Isˉframeˉrecordˉlocal(
        Nativeˉfunction function,
        int local,
        Nativeˉvalueˉtype operationˉtype,
        IReadOnlySet<int> storedˉrecordˉparameters)
    {
        if ((uint)local >= (uint)function.Allˉlocalˉtypes.Length)
        {
            Fail("The native record-storage planner received an invalid local identifier.");
        }
        var Type = function.Allˉlocalˉtypes[local];
        if (Type != operationˉtype)
        {
            Fail("The native record-storage planner received inconsistent local type metadata.");
        }
        return Type == Nativeˉvalueˉtype.Record &&
            (local >= function.Parameterˉtypes.Length ||
                storedˉrecordˉparameters.Contains(local));
    }

    private static int Requireˉblock(
        IReadOnlyDictionary<int, int> blockˉindices,
        int block)
    {
        if (!blockˉindices.TryGetValue(block, out var Index))
        {
            Fail("The native record-storage planner received an invalid branch target.");
        }
        return Index;
    }

    private static int Recordˉlocalˉwidth(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int local) =>
        Requireˉrecord(module, function.Allˉlocalˉnominalˉtypeˉindices[local]).Fields.Length;

    private static Nativeˉrecordˉvalueˉallocation Planˉrecordˉvalueˉfields(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int expectedˉrecordˉvalues)
    {
        var Definedˉrecordˉvalues = new HashSet<int>();
        var Interference = new Dictionary<int, HashSet<int>>();
        var Peak = 0;
        foreach (var Block in function.Blocks)
        {
            var Lastˉuses = new Dictionary<int, int>();
            for (var Operationˉindex = 0;
                Operationˉindex < Block.Operations.Length;
                Operationˉindex++)
            {
                foreach (var Value in Recordˉuses(function, Block.Operations[Operationˉindex]))
                {
                    Lastˉuses[Value] = Operationˉindex;
                }
            }
            foreach (var Value in Recordˉuses(function, Block.Terminator))
            {
                Lastˉuses[Value] = Block.Operations.Length;
            }

            var Live = new HashSet<int>();
            var Liveˉfieldˉcells = 0;
            for (var Operationˉindex = 0;
                Operationˉindex < Block.Operations.Length;
                Operationˉindex++)
            {
                var Operation = Block.Operations[Operationˉindex];
                var Result = Recordˉresult(function, Operation);
                if (Result >= 0)
                {
                    if (!Definedˉrecordˉvalues.Add(Result) ||
                        !Interference.TryAdd(Result, []))
                    {
                        Fail("The native record-storage planner received a duplicate record result.");
                    }
                    foreach (var Other in Live)
                    {
                        // A destination must coexist with every operand consumed by this operation.
                        Interference[Result].Add(Other);
                        Interference[Other].Add(Result);
                    }
                    if (!Live.Add(Result))
                    {
                        Fail("The native record-storage planner received an already-live record result.");
                    }
                    Liveˉfieldˉcells = checked(
                        Liveˉfieldˉcells + Recordˉwidth(module, function, Result));
                    Peak = Math.Max(Peak, Liveˉfieldˉcells);
                }

                Releaseˉlastˉuses(
                    module,
                    function,
                    Recordˉuses(function, Operation),
                    Operationˉindex,
                    Lastˉuses,
                    Live,
                    ref Liveˉfieldˉcells);

                if (Result >= 0 && !Lastˉuses.ContainsKey(Result))
                {
                    Releaseˉrecord(module, function, Result, Live, ref Liveˉfieldˉcells);
                }
            }

            Releaseˉlastˉuses(
                module,
                function,
                Recordˉuses(function, Block.Terminator),
                Block.Operations.Length,
                Lastˉuses,
                Live,
                ref Liveˉfieldˉcells);
            if (Live.Count != 0 || Liveˉfieldˉcells != 0)
            {
                Fail("The native record-storage planner found a record value live across a block boundary.");
            }
        }

        if (Definedˉrecordˉvalues.Count != expectedˉrecordˉvalues)
        {
            Fail("The native record-storage planner could not account for every record result.");
        }
        var Allocation = Allocateˉrecordˉoffsets(
            function.Valueˉtypes.Length,
            Interference,
            Value => Recordˉwidth(module, function, Value));
        return new(
            Peak,
            Allocation.Requiredˉfieldˉcells,
            Allocation.Offsets);
    }

    private static Nativeˉrecordˉoffsetˉallocation Allocateˉrecordˉoffsets(
        int identifierˉcount,
        IReadOnlyDictionary<int, HashSet<int>> interference,
        Func<int, int> width)
    {
        var Offsets = Enumerable.Repeat(-1, identifierˉcount).ToArray();
        var Allocations = new Dictionary<int, (int Offset, int Width)>();
        var Requiredˉfieldˉcells = 0;
        // Width-first first-fit is deterministic; only proven non-interfering identities may overlap.
        foreach (var Identifier in interference.Keys
            .OrderByDescending(width)
            .ThenBy(Identifier => Identifier))
        {
            var Width = width(Identifier);
            var Candidate = 0;
            foreach (var Allocation in interference[Identifier]
                .Where(Allocations.ContainsKey)
                .Select(Other => Allocations[Other])
                .OrderBy(Allocation => Allocation.Offset)
                .ThenBy(Allocation => Allocation.Width))
            {
                if (checked(Candidate + Width) <= Allocation.Offset)
                {
                    break;
                }
                if (Candidate < checked(Allocation.Offset + Allocation.Width))
                {
                    Candidate = checked(Allocation.Offset + Allocation.Width);
                }
            }
            Allocations.Add(Identifier, (Candidate, Width));
            Offsets[Identifier] = Candidate;
            Requiredˉfieldˉcells = Math.Max(
                Requiredˉfieldˉcells,
                checked(Candidate + Width));
        }
        return new(Requiredˉfieldˉcells, Offsets.ToImmutableArray());
    }

    private static void Releaseˉlastˉuses(
        Nativeˉmodule module,
        Nativeˉfunction function,
        IEnumerable<int> uses,
        int operationˉindex,
        IReadOnlyDictionary<int, int> lastˉuses,
        HashSet<int> live,
        ref int liveˉfieldˉcells)
    {
        foreach (var Value in uses.Distinct())
        {
            if (!live.Contains(Value))
            {
                Fail("The native record-storage planner found a record use outside its defining block.");
            }
            if (lastˉuses[Value] == operationˉindex)
            {
                Releaseˉrecord(module, function, Value, live, ref liveˉfieldˉcells);
            }
        }
    }

    private static void Releaseˉrecord(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int value,
        HashSet<int> live,
        ref int liveˉfieldˉcells)
    {
        if (!live.Remove(value))
        {
            Fail("The native record-storage planner attempted to release a non-live record value.");
        }
        liveˉfieldˉcells = checked(
            liveˉfieldˉcells - Recordˉwidth(module, function, value));
    }

    private static int Recordˉresult(
        Nativeˉfunction function,
        Nativeˉoperation operation)
    {
        var Result = operation switch
        {
            Nativeˉlocalˉload Load => Load.Result,
            Nativeˉrecordˉcreate Create => Create.Result,
            Nativeˉrecordˉfield Field => Field.Result,
            Nativeˉcall Call => Call.Result,
            _ => -1,
        };
        return Result >= 0 && Requireˉvalueˉtype(function, Result) == Nativeˉvalueˉtype.Record
            ? Result
            : -1;
    }

    private static IEnumerable<int> Recordˉuses(
        Nativeˉfunction function,
        Nativeˉoperation operation)
    {
        var Values = operation switch
        {
            Nativeˉlocalˉstore Store => [Store.Value],
            Nativeˉrecordˉcreate Create => Create.Fields,
            Nativeˉrecordˉfield Field => [Field.Record],
            Nativeˉcall Call => Call.Arguments,
            Nativeˉvoidˉcall Call => Call.Arguments,
            _ => [],
        };
        return Values.Where(
            Value => Requireˉvalueˉtype(function, Value) == Nativeˉvalueˉtype.Record);
    }

    private static IEnumerable<int> Recordˉuses(
        Nativeˉfunction function,
        Nativeˉterminator terminator)
    {
        if (terminator is Nativeˉreturn Return &&
            Requireˉvalueˉtype(function, Return.Value) == Nativeˉvalueˉtype.Record)
        {
            return [Return.Value];
        }
        return [];
    }

    private static Nativeˉvalueˉtype Requireˉvalueˉtype(
        Nativeˉfunction function,
        int value)
    {
        if ((uint)value >= (uint)function.Valueˉtypes.Length)
        {
            Fail("The native record-storage planner received an invalid value identifier.");
        }
        return function.Valueˉtypes[value];
    }

    private static int Recordˉwidth(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int value) =>
        Requireˉrecord(module, function.Valueˉnominalˉtypeˉindices[value]).Fields.Length;

    private static Recordˉtypeˉdeclaration Requireˉrecord(
        Nativeˉmodule module,
        int typeˉindex)
    {
        if ((uint)typeˉindex >= (uint)module.Types.Length)
        {
            Fail("The native record-storage planner received an invalid record identity.");
        }
        return module.Types[typeˉindex] as Recordˉtypeˉdeclaration ??
            throw new Nativeˉbackendˉexception(
                "WVN2901",
                "The native record-storage planner received a non-record identity.");
    }

    [DoesNotReturn]
    private static void Fail(string message) =>
        throw new Nativeˉbackendˉexception("WVN2901", message);

    private sealed record Nativeˉrecordˉoffsetˉallocation(
        int Requiredˉfieldˉcells,
        ImmutableArray<int> Offsets);

    private sealed record Nativeˉrecordˉvalueˉallocation(
        int Peakˉliveˉfieldˉcells,
        int Requiredˉfieldˉcells,
        ImmutableArray<int> Offsets);
}
