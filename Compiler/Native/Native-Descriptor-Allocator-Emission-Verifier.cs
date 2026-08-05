using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Windvale.Compiler.Native;

public static class Nativeˉdescriptorˉallocatorˉemissionˉverifier
{
    public static void Verify(
        Nativeˉmodule module,
        Nativeˉdescriptorˉallocatorˉemissionˉplan plan)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(plan);
        if (module.Descriptorˉownership is null)
        {
            Fail("The descriptor allocator emission verifier requires ownership evidence.");
        }
        Nativeˉdescriptorˉownershipˉverifier.Verify(module, module.Descriptorˉownership);
        if (plan.Formatˉversion != Nativeˉdescriptorˉallocatorˉemissionˉcontract.FORMAT_VERSION ||
            plan.Candidateˉcontextˉformatˉversion !=
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.CANDIDATE_CONTEXT_FORMAT_VERSION ||
            plan.Candidateˉcontextˉsize !=
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.CANDIDATE_CONTEXT_SIZE ||
            plan.Allocatorˉstateˉpointerˉoffset !=
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.ALLOCATOR_STATE_POINTER_OFFSET ||
            plan.Allocatorˉleafˉpointerˉoffset !=
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.ALLOCATOR_LEAF_POINTER_OFFSET ||
            plan.Requestˉframeˉcells !=
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.REQUEST_FRAME_CELLS ||
            plan.Candidateˉcontextˉsize != plan.Allocatorˉleafˉpointerˉoffset + sizeof(ulong) ||
            plan.Allocatorˉstateˉpointerˉoffset !=
                Nativeˉexecutionˉcontextˉcontract.SIZE ||
            plan.Allocatorˉleafˉpointerˉoffset !=
                plan.Allocatorˉstateˉpointerˉoffset + sizeof(ulong) ||
            plan.Functions.IsDefault ||
            plan.Functions.Length != module.Functions.Length ||
            plan.Ownershipˉactions != module.Descriptorˉownership.Totalˉactions ||
            plan.Ownershipˉactions is < 0 or >
                Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS ||
            plan.Allocatorˉleafˉinvocations < 0 ||
            plan.Allocatorˉleafˉinvocations > plan.Ownershipˉactions ||
            plan.Ownershipˉmovementˉactions !=
                plan.Ownershipˉactions - plan.Allocatorˉleafˉinvocations)
        {
            Fail("The descriptor allocator emission plan has an invalid envelope.");
        }

        var Storage = Nativeˉrecordˉstorageˉplanner.Measure(module);
        var Totalˉinvocations = 0;
        for (var Functionˉindex = 0; Functionˉindex < module.Functions.Length; Functionˉindex++)
        {
            var Function = module.Functions[Functionˉindex];
            var Ownership = module.Descriptorˉownership.Functions[Functionˉindex];
            var Actual = plan.Functions[Functionˉindex];
            var Expected = Reconstructˉfunction(
                module,
                Function,
                Functionˉindex,
                Ownership,
                Storage[Functionˉindex]);
            if (Actual is null ||
                Actual.Functionˉindex != Expected.Functionˉindex ||
                !StringComparer.Ordinal.Equals(Actual.Functionˉname, Expected.Functionˉname) ||
                Actual.Ownershipˉactions != Expected.Ownershipˉactions ||
                Actual.Allocatorˉleafˉinvocations != Expected.Allocatorˉleafˉinvocations ||
                Actual.Generatedˉcodeˉinvocations != Expected.Generatedˉcodeˉinvocations ||
                Actual.Runtimeˉserviceˉinvocations != Expected.Runtimeˉserviceˉinvocations ||
                Actual.Requestˉbaseˉcell != Expected.Requestˉbaseˉcell ||
                Actual.Existingˉframeˉcells != Expected.Existingˉframeˉcells ||
                Actual.Projectedˉframeˉcells != Expected.Projectedˉframeˉcells ||
                Actual.Preservesˉinstructionˉbudget != Expected.Preservesˉinstructionˉbudget ||
                Actual.Preservesˉcallˉdepth != Expected.Preservesˉcallˉdepth ||
                Actual.Emissions.IsDefault ||
                !Actual.Emissions.SequenceEqual(Expected.Emissions))
            {
                Fail(
                    $"Descriptor allocator emission differs for function {Functionˉindex} " +
                    $"'{Function.Name}'.");
            }
            Totalˉinvocations = checked(
                Totalˉinvocations + Actual.Allocatorˉleafˉinvocations);
        }
        if (Totalˉinvocations != plan.Allocatorˉleafˉinvocations)
        {
            Fail("The descriptor allocator emission aggregate is inconsistent.");
        }
    }

    private static Nativeˉfunctionˉdescriptorˉallocatorˉemission Reconstructˉfunction(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        Nativeˉfunctionˉdescriptorˉownership ownership,
        Nativeˉfunctionˉrecordˉstorage storage)
    {
        if (function is null ||
            ownership.Functionˉindex != functionˉindex ||
            storage.Functionˉindex != functionˉindex ||
            ownership.Actions.IsDefault)
        {
            Fail("The descriptor allocator emission verifier received mismatched function input.");
        }
        var Blocks = function.Blocks.ToDictionary(Block => Block.Id);
        var Expected = ImmutableArray.CreateBuilder<Nativeˉdescriptorˉallocatorˉemission>();
        for (var Index = 0; Index < ownership.Actions.Length; Index++)
        {
            var Action = ownership.Actions[Index];
            if (Action.Kind is not (
                Nativeˉdescriptorˉownershipˉactionˉkind.Acquire or
                Nativeˉdescriptorˉownershipˉactionˉkind.Retain or
                Nativeˉdescriptorˉownershipˉactionˉkind.Release))
            {
                continue;
            }
            Nativeˉoperation? Operation = null;
            var Terminator = false;
            if (Action.Block == -1)
            {
                if (Action.Operation != -1)
                {
                    Fail("An entry allocator action has an invalid operation index.");
                }
            }
            else
            {
                if (!Blocks.TryGetValue(Action.Block, out var Block) ||
                    Action.Operation < 0 ||
                    Action.Operation > Block.Operations.Length)
                {
                    Fail("An allocator action has an invalid block position.");
                }
                Terminator = Action.Operation == Block.Operations.Length;
                if (!Terminator)
                {
                    Operation = Block.Operations[Action.Operation];
                }
            }

            var Phase = Reconstructˉphase(ownership.Actions, Index, Action, Terminator);
            var Site = Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acquire
                ? Reconstructˉsite(Operation)
                : Nativeˉdescriptorˉallocatorˉinvocationˉsite.Generatedˉcode;
            Expected.Add(new(
                Index,
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
                Locate(function, functionˉindex, storage, Action.Target),
                Locate(function, functionˉindex, storage, Action.Source)));
        }

        var Emissions = Expected.ToImmutable();
        var Generated = Emissions.Count(Emission =>
            Emission.Invocationˉsite ==
                Nativeˉdescriptorˉallocatorˉinvocationˉsite.Generatedˉcode);
        var Service = checked(Emissions.Length - Generated);
        var Hasˉrequest = Generated != 0;
        var Projected = Hasˉrequest
            ? checked(
                storage.Projectedˉframeˉcells +
                Nativeˉdescriptorˉallocatorˉemissionˉcontract.REQUEST_FRAME_CELLS)
            : storage.Projectedˉframeˉcells;
        if (Projected > Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
        {
            Fail("A reconstructed descriptor allocator request escapes the frame limit.");
        }
        return new(
            functionˉindex,
            function.Name,
            ownership.Actions.Length,
            Emissions.Length,
            Generated,
            Service,
            Hasˉrequest ? storage.Projectedˉframeˉcells : -1,
            storage.Projectedˉframeˉcells,
            Projected,
            Hasˉrequest,
            Hasˉrequest,
            Emissions);
    }

    private static Nativeˉdescriptorˉallocatorˉemissionˉphase Reconstructˉphase(
        ImmutableArray<Nativeˉdescriptorˉownershipˉaction> actions,
        int index,
        Nativeˉdescriptorˉownershipˉaction action,
        bool terminator)
    {
        if (action.Block == -1)
        {
            if (action.Kind != Nativeˉdescriptorˉownershipˉactionˉkind.Retain)
            {
                Fail("A non-retain allocator invocation appears at function entry.");
            }
            return Nativeˉdescriptorˉallocatorˉemissionˉphase.Functionˉentry;
        }
        if (terminator)
        {
            if (action.Kind != Nativeˉdescriptorˉownershipˉactionˉkind.Release)
            {
                Fail("A non-release allocator invocation appears at a terminator.");
            }
            return Nativeˉdescriptorˉallocatorˉemissionˉphase.Terminatorˉcleanup;
        }
        if (action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acquire)
        {
            return Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉallocation;
        }
        if (action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Retain)
        {
            var Writesˉlocal = action.Target.Kind == Nativeˉdescriptorˉcarrierˉkind.Local ||
                action.Target.Kind == Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield;
            return Writesˉlocal
                ? Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉbefore
                : Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉafter;
        }
        var Replaced = false;
        if (action.Source.Kind == Nativeˉdescriptorˉcarrierˉkind.Local ||
            action.Source.Kind == Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield)
        {
            for (var Previousˉindex = 0; Previousˉindex < index; Previousˉindex++)
            {
                var Previous = actions[Previousˉindex];
                Replaced |= Previous.Block == action.Block &&
                    Previous.Operation == action.Operation &&
                    Previous.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Retain &&
                    Previous.Target == action.Source;
            }
        }
        return Replaced
            ? Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉbefore
            : Nativeˉdescriptorˉallocatorˉemissionˉphase.Operationˉafter;
    }

    private static Nativeˉdescriptorˉallocatorˉinvocationˉsite Reconstructˉsite(
        Nativeˉoperation? operation)
    {
        if (operation is
            Nativeˉbytesˉconcat or
            Nativeˉbytesˉfromˉu8 or
            Nativeˉbytesˉfromˉu16ˉlittle or
            Nativeˉbytesˉfromˉu32ˉlittle or
            Nativeˉbytesˉfromˉi32ˉlittle)
        {
            return Nativeˉdescriptorˉallocatorˉinvocationˉsite.Generatedˉcode;
        }
        if (operation is
            Nativeˉenumˉname or
            Nativeˉintegerˉformat or
            Nativeˉtextˉconcat or
            Nativeˉtextˉquote)
        {
            return Nativeˉdescriptorˉallocatorˉinvocationˉsite.Runtimeˉservice;
        }
        Fail("The descriptor allocator emission verifier found an unknown acquisition site.");
        return default;
    }

    private static Nativeˉdescriptorˉownerˉlocation Locate(
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
            Fail("A reconstructed allocator owner is outside its function.");
        }
        int Directˉcell;
        switch (carrier.Kind)
        {
            case Nativeˉdescriptorˉcarrierˉkind.Parameter:
            case Nativeˉdescriptorˉcarrierˉkind.Local:
                Directˉcell = carrier.Binding;
                break;
            case Nativeˉdescriptorˉcarrierˉkind.Value:
                if ((uint)carrier.Binding >= (uint)function.Valueˉslotˉindices.Length)
                {
                    Fail("A reconstructed descriptor value has an invalid identity.");
                }
                Directˉcell = checked(
                    function.Allˉlocalˉtypes.Length +
                    function.Valueˉslotˉindices[carrier.Binding]);
                break;
            case Nativeˉdescriptorˉcarrierˉkind.Recordˉparameterˉfield:
                return Indirect(carrier.Binding, carrier.Field);
            case Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield:
                if ((uint)carrier.Binding >=
                    (uint)storage.Localˉrecordˉfieldˉoffsets.Length)
                {
                    Fail("A reconstructed record-local owner has an invalid binding.");
                }
                var Localˉbase = storage.Localˉrecordˉfieldˉoffsets[carrier.Binding];
                if (Localˉbase < 0)
                {
                    return Indirect(carrier.Binding, carrier.Field);
                }
                Directˉcell = checked(Localˉbase + carrier.Field);
                break;
            case Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield:
                if ((uint)carrier.Binding >=
                        (uint)storage.Valueˉrecordˉfieldˉoffsets.Length ||
                    storage.Valueˉrecordˉfieldˉoffsets[carrier.Binding] < 0)
                {
                    Fail("A reconstructed record-value owner lacks direct backing.");
                }
                Directˉcell = checked(
                    storage.Valueˉrecordˉfieldˉoffsets[carrier.Binding] + carrier.Field);
                break;
            default:
                Fail("A reconstructed allocator invocation references a return boundary.");
                return Nativeˉdescriptorˉownerˉlocation.None;
        }
        if (Directˉcell < 0 || Directˉcell >= storage.Projectedˉframeˉcells)
        {
            Fail("A reconstructed direct owner escapes its projected frame.");
        }
        return new(
            Nativeˉdescriptorˉownerˉaddressing.Directˉframe,
            Directˉcell,
            Nativeˉdescriptorˉallocatorˉemissionˉcontract.DESCRIPTOR_OWNER_TOKEN_OFFSET);

        Nativeˉdescriptorˉownerˉlocation Indirect(int handle, int field)
        {
            if (handle < 0 || handle >= function.Allˉlocalˉtypes.Length || field < 0)
            {
                Fail("A reconstructed indirect record owner is invalid.");
            }
            return new(
                Nativeˉdescriptorˉownerˉaddressing.Indirectˉrecord,
                handle,
                checked(
                    field * Nativeˉcontract.VALUE_SLOT_BYTES +
                    Nativeˉdescriptorˉallocatorˉemissionˉcontract.DESCRIPTOR_OWNER_TOKEN_OFFSET));
        }
    }

    [DoesNotReturn]
    private static void Fail(string message) =>
        throw new Nativeˉbackendˉexception("WVN2905", message);
}
