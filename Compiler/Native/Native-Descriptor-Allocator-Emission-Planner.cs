using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.Bytecode;

namespace Windvale.Compiler.Native;

public static class Nativeˉdescriptorˉallocatorˉemissionˉcontract
{
    public const uint FORMAT_VERSION = 1;
    public const uint CANDIDATE_CONTEXT_FORMAT_VERSION = 8;
    public const uint CANDIDATE_CONTEXT_SIZE = 128;
    public const int ALLOCATOR_STATE_POINTER_OFFSET = 112;
    public const int ALLOCATOR_LEAF_POINTER_OFFSET = 120;
    public const int REQUEST_FRAME_CELLS = 3;
    public const int DESCRIPTOR_OWNER_TOKEN_OFFSET = 12;
}

public enum Nativeˉdescriptorˉownerˉaddressing : byte
{
    None = 0,
    Directˉframe = 1,
    Indirectˉrecord = 2,
}

public sealed record Nativeˉdescriptorˉownerˉlocation(
    Nativeˉdescriptorˉownerˉaddressing Addressing,
    int Baseˉframeˉcell,
    int Ownerˉbyteˉdisplacement)
{
    public static Nativeˉdescriptorˉownerˉlocation None { get; } = new(
        Nativeˉdescriptorˉownerˉaddressing.None,
        -1,
        -1);
}

public enum Nativeˉdescriptorˉallocatorˉemissionˉphase : byte
{
    Functionˉentry = 1,
    Operationˉbefore = 2,
    Operationˉallocation = 3,
    Operationˉafter = 4,
    Terminatorˉcleanup = 5,
}

public enum Nativeˉdescriptorˉallocatorˉinvocationˉsite : byte
{
    Generatedˉcode = 1,
    Runtimeˉservice = 2,
}

public sealed record Nativeˉdescriptorˉallocatorˉemission(
    int Ownershipˉactionˉindex,
    int Block,
    int Operation,
    Nativeˉdescriptorˉownershipˉactionˉkind Ownershipˉkind,
    Nativeˉdescriptorˉallocatorˉoperation Allocatorˉoperation,
    Nativeˉdescriptorˉallocatorˉemissionˉphase Phase,
    Nativeˉdescriptorˉallocatorˉinvocationˉsite Invocationˉsite,
    Nativeˉdescriptorˉownerˉlocation Target,
    Nativeˉdescriptorˉownerˉlocation Source);

public sealed record Nativeˉfunctionˉdescriptorˉallocatorˉemission(
    int Functionˉindex,
    string Functionˉname,
    int Ownershipˉactions,
    int Allocatorˉleafˉinvocations,
    int Generatedˉcodeˉinvocations,
    int Runtimeˉserviceˉinvocations,
    int Requestˉbaseˉcell,
    int Existingˉframeˉcells,
    int Projectedˉframeˉcells,
    bool Preservesˉinstructionˉbudget,
    bool Preservesˉcallˉdepth,
    ImmutableArray<Nativeˉdescriptorˉallocatorˉemission> Emissions);

public sealed record Nativeˉdescriptorˉallocatorˉemissionˉplan(
    uint Formatˉversion,
    uint Candidateˉcontextˉformatˉversion,
    uint Candidateˉcontextˉsize,
    int Allocatorˉstateˉpointerˉoffset,
    int Allocatorˉleafˉpointerˉoffset,
    int Requestˉframeˉcells,
    int Ownershipˉactions,
    int Allocatorˉleafˉinvocations,
    int Ownershipˉmovementˉactions,
    ImmutableArray<Nativeˉfunctionˉdescriptorˉallocatorˉemission> Functions);

public static class Nativeˉdescriptorˉallocatorˉemissionˉplanner
{
    public static Nativeˉdescriptorˉallocatorˉemissionˉplan Plan(Nativeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (module.Descriptorˉownership is null)
        {
            Fail("The descriptor allocator emission planner requires a verified ownership plan.");
        }
        Nativeˉdescriptorˉownershipˉverifier.Verify(module, module.Descriptorˉownership);
        var Storage = Nativeˉrecordˉstorageˉplanner.Measure(module);
        if (Storage.Length != module.Functions.Length ||
            module.Descriptorˉownership.Functions.Length != module.Functions.Length)
        {
            Fail("The descriptor allocator emission planner received inconsistent function plans.");
        }

        var Functions = module.Functions
            .Select((Function, Index) => Planˉfunction(
                module,
                Function,
                Index,
                module.Descriptorˉownership.Functions[Index],
                Storage[Index]))
            .ToImmutableArray();
        var Invocations = Functions.Aggregate(
            0,
            (Total, Function) => checked(Total + Function.Allocatorˉleafˉinvocations));
        if (Invocations > Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS ||
            Invocations > module.Descriptorˉownership.Totalˉactions)
        {
            Fail("The descriptor allocator emission plan exceeds its bounded ownership envelope.");
        }
        return new(
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.FORMAT_VERSION,
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.CANDIDATE_CONTEXT_FORMAT_VERSION,
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.CANDIDATE_CONTEXT_SIZE,
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.ALLOCATOR_STATE_POINTER_OFFSET,
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.ALLOCATOR_LEAF_POINTER_OFFSET,
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.REQUEST_FRAME_CELLS,
            module.Descriptorˉownership.Totalˉactions,
            Invocations,
            checked(module.Descriptorˉownership.Totalˉactions - Invocations),
            Functions);
    }

    private static Nativeˉfunctionˉdescriptorˉallocatorˉemission Planˉfunction(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        Nativeˉfunctionˉdescriptorˉownership ownership,
        Nativeˉfunctionˉrecordˉstorage storage)
    {
        if (ownership.Functionˉindex != functionˉindex ||
            !StringComparer.Ordinal.Equals(ownership.Functionˉname, function.Name) ||
            storage.Functionˉindex != functionˉindex ||
            !StringComparer.Ordinal.Equals(storage.Functionˉname, function.Name) ||
            ownership.Actions.IsDefault)
        {
            Fail("The descriptor allocator emission planner received mismatched function evidence.");
        }

        var Blocks = function.Blocks.ToDictionary(Block => Block.Id);
        var Emissions = ImmutableArray.CreateBuilder<Nativeˉdescriptorˉallocatorˉemission>();
        for (var Actionˉindex = 0; Actionˉindex < ownership.Actions.Length; Actionˉindex++)
        {
            var Action = ownership.Actions[Actionˉindex];
            if (Action.Kind is not (
                Nativeˉdescriptorˉownershipˉactionˉkind.Acquire or
                Nativeˉdescriptorˉownershipˉactionˉkind.Retain or
                Nativeˉdescriptorˉownershipˉactionˉkind.Release))
            {
                continue;
            }
            var Operation = Requireˉoperation(Blocks, Action);
            var Phase = Classifyˉphase(ownership.Actions, Actionˉindex, Action, Operation);
            var Site = Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acquire
                ? Classifyˉacquisitionˉsite(Operation)
                : Nativeˉdescriptorˉallocatorˉinvocationˉsite.Generatedˉcode;
            Emissions.Add(new(
                Actionˉindex,
                Action.Block,
                Action.Operation,
                Action.Kind,
                Action.Kind switch
                {
                    Nativeˉdescriptorˉownershipˉactionˉkind.Acquire =>
                        Nativeˉdescriptorˉallocatorˉoperation.Acquire,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain =>
                        Nativeˉdescriptorˉallocatorˉoperation.Retain,
                    _ => Nativeˉdescriptorˉallocatorˉoperation.Release,
                },
                Phase,
                Site,
                Resolveˉlocation(function, functionˉindex, storage, Action.Target),
                Resolveˉlocation(function, functionˉindex, storage, Action.Source)));
        }

        var Frozen = Emissions.ToImmutable();
        var Generated = Frozen.Count(Emission =>
            Emission.Invocationˉsite ==
                Nativeˉdescriptorˉallocatorˉinvocationˉsite.Generatedˉcode);
        var Services = checked(Frozen.Length - Generated);
        var Requestˉbase = Generated == 0 ? -1 : storage.Projectedˉframeˉcells;
        var Projectedˉframe = Generated == 0
            ? storage.Projectedˉframeˉcells
            : checked(
                storage.Projectedˉframeˉcells +
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.REQUEST_FRAME_CELLS);
        if (Projectedˉframe > Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
        {
            Fail(
                $"Native function '{function.Name}' requires {Projectedˉframe} successor-ABI " +
                $"frame cells; the limit is {Nativeˉcontract.MAXIMUM_FRAME_SLOTS}.");
        }
        return new(
            functionˉindex,
            function.Name,
            ownership.Actions.Length,
            Frozen.Length,
            Generated,
            Services,
            Requestˉbase,
            storage.Projectedˉframeˉcells,
            Projectedˉframe,
            Preservesˉinstructionˉbudget: Generated != 0,
            Preservesˉcallˉdepth: Generated != 0,
            Frozen);
    }

    private static Nativeˉoperation? Requireˉoperation(
        IReadOnlyDictionary<int, Nativeˉblock> blocks,
        Nativeˉdescriptorˉownershipˉaction action)
    {
        if (action.Block == -1 && action.Operation == -1)
        {
            return null;
        }
        if (!blocks.TryGetValue(action.Block, out var Block) ||
            action.Operation < 0 ||
            action.Operation > Block.Operations.Length)
        {
            Fail("The descriptor allocator emission planner received an invalid action position.");
        }
        return action.Operation == Block.Operations.Length
            ? null
            : Block.Operations[action.Operation];
    }

    private static Nativeˉdescriptorˉallocatorˉemissionˉphase Classifyˉphase(
        ImmutableArray<Nativeˉdescriptorˉownershipˉaction> actions,
        int actionˉindex,
        Nativeˉdescriptorˉownershipˉaction action,
        Nativeˉoperation? operation)
    {
        if (action.Block == -1)
        {
            if (action.Operation != -1 ||
                action.Kind != Nativeˉdescriptorˉownershipˉactionˉkind.Retain)
            {
                Fail("Only retained assigned parameters may invoke the allocator at entry.");
            }
            return Nativeˉdescriptorˉallocatorˉemissionˉphase.Functionˉentry;
        }
        if (operation is null)
        {
            if (action.Kind != Nativeˉdescriptorˉownershipˉactionˉkind.Release)
            {
                Fail("Only release may invoke the allocator during terminator cleanup.");
            }
            return Nativeˉdescriptorˉallocatorˉemissionˉphase.Terminatorˉcleanup;
        }
        if (action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acquire)
        {
            return Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉallocation;
        }
        if (action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Retain)
        {
            return action.Target.Kind is
                Nativeˉdescriptorˉcarrierˉkind.Local or
                Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield
                    ? Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉbefore
                    : Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉafter;
        }
        var Replacesˉlocal = (action.Source.Kind is
            Nativeˉdescriptorˉcarrierˉkind.Local or
            Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield) &&
            actions.Take(actionˉindex).Any(Previous =>
                Previous.Block == action.Block &&
                Previous.Operation == action.Operation &&
                Previous.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Retain &&
                Previous.Target == action.Source);
        return Replacesˉlocal
            ? Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉbefore
            : Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉafter;
    }

    private static Nativeˉdescriptorˉallocatorˉinvocationˉsite Classifyˉacquisitionˉsite(
        Nativeˉoperation? operation) => operation switch
    {
        Nativeˉbytesˉconcat or
        Nativeˉbytesˉfromˉu8 or
        Nativeˉbytesˉfromˉu16ˉlittle or
        Nativeˉbytesˉfromˉu32ˉlittle =>
            Nativeˉdescriptorˉallocatorˉinvocationˉsite.Generatedˉcode,
        Nativeˉenumˉname or
        Nativeˉintegerˉformat or
        Nativeˉtextˉconcat or
        Nativeˉtextˉquote =>
            Nativeˉdescriptorˉallocatorˉinvocationˉsite.Runtimeˉservice,
        _ => throw Invalidˉoperation(
            "The descriptor allocator emission planner cannot place an acquisition."),
    };

    private static Nativeˉdescriptorˉownerˉlocation Resolveˉlocation(
        Nativeˉfunction function,
        int functionˉindex,
        Nativeˉfunctionˉrecordˉstorage storage,
        Nativeˉdescriptorˉcarrier carrier)
    {
        if (carrier.Kind == Nativeˉdescriptorˉcarrierˉkind.None)
        {
            return Nativeˉdescriptorˉownerˉlocation.None;
        }
        if (carrier.Function != functionˉindex || carrier.Binding < 0)
        {
            Fail("An allocator invocation references a descriptor outside its function frame.");
        }
        return carrier.Kind switch
        {
            Nativeˉdescriptorˉcarrierˉkind.Parameter or
            Nativeˉdescriptorˉcarrierˉkind.Local => Direct(carrier.Binding),
            Nativeˉdescriptorˉcarrierˉkind.Value => Direct(Valueˉcell(carrier.Binding)),
            Nativeˉdescriptorˉcarrierˉkind.Recordˉparameterˉfield =>
                Indirect(carrier.Binding, carrier.Field),
            Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield =>
                Recordˉlocal(carrier.Binding, carrier.Field),
            Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield =>
                Recordˉvalue(carrier.Binding, carrier.Field),
            _ => throw Invalidˉoperation(
                "An allocator invocation cannot address the function-return boundary."),
        };

        Nativeˉdescriptorˉownerˉlocation Direct(int cell)
        {
            if (cell < 0 || cell >= storage.Projectedˉframeˉcells)
            {
                Fail("A direct descriptor owner location escapes its projected frame.");
            }
            return new(
                Nativeˉdescriptorˉownerˉaddressing.Directˉframe,
                cell,
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.DESCRIPTOR_OWNER_TOKEN_OFFSET);
        }

        Nativeˉdescriptorˉownerˉlocation Indirect(int handle, int field)
        {
            if (handle < 0 || handle >= function.Allˉlocalˉtypes.Length || field < 0)
            {
                Fail("An indirect descriptor owner location is invalid.");
            }
            return new(
                Nativeˉdescriptorˉownerˉaddressing.Indirectˉrecord,
                handle,
                checked(
                    field * Nativeˉcontract.VALUE_SLOT_BYTES +
                    Nativeˉdescriptorˉallocatorˉemissionˉcontract.DESCRIPTOR_OWNER_TOKEN_OFFSET));
        }

        int Valueˉcell(int value)
        {
            if ((uint)value >= (uint)function.Valueˉslotˉindices.Length)
            {
                Fail("A descriptor value owner location uses an invalid identity.");
            }
            return checked(function.Allˉlocalˉtypes.Length + function.Valueˉslotˉindices[value]);
        }

        Nativeˉdescriptorˉownerˉlocation Recordˉlocal(int local, int field)
        {
            if ((uint)local >= (uint)storage.Localˉrecordˉfieldˉoffsets.Length)
            {
                Fail("A record-local descriptor owner location uses an invalid binding.");
            }
            var Base = storage.Localˉrecordˉfieldˉoffsets[local];
            return Base >= 0 ? Direct(checked(Base + field)) : Indirect(local, field);
        }

        Nativeˉdescriptorˉownerˉlocation Recordˉvalue(int value, int field)
        {
            if ((uint)value >= (uint)storage.Valueˉrecordˉfieldˉoffsets.Length ||
                storage.Valueˉrecordˉfieldˉoffsets[value] < 0)
            {
                Fail("A record-value descriptor owner location lacks direct frame backing.");
            }
            return Direct(checked(storage.Valueˉrecordˉfieldˉoffsets[value] + field));
        }
    }

    [DoesNotReturn]
    private static void Fail(string message) => throw new Nativeˉbackendˉexception("WVN2904", message);

    private static Nativeˉbackendˉexception Invalidˉoperation(string message) =>
        new("WVN2904", message);
}
