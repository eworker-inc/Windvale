using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.Bytecode;

namespace Windvale.Compiler.Native;

// This oracle deliberately does not call the planner. It reconstructs the canonical action stream
// from machine IR so published summaries and actions cross an independent validation boundary.
internal static class Nativeˉdescriptorˉownershipˉindependentˉoracle
{
    public static Nativeˉdescriptorˉownershipˉplan Reconstruct(Nativeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (module.Functions.IsDefault || module.Types.IsDefault)
        {
            Fail("The descriptor-ownership oracle requires initialized machine IR.");
        }
        var Functions = module.Functions
            .Select((Function, Index) => Reconstructˉfunction(module, Function, Index))
            .ToImmutableArray();
        var Total = Functions.Aggregate(
            0,
            (Count, Function) => checked(Count + Function.Actions.Length));
        if (Total > Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS)
        {
            Fail("The independently reconstructed descriptor-ownership plan is oversized.");
        }
        return new(
            Nativeˉdescriptorˉownershipˉplanner.FORMAT_VERSION,
            Terminalˉfailureˉdiscardsˉarena: true,
            Total,
            Functions);
    }

    private static Nativeˉfunctionˉdescriptorˉownership Reconstructˉfunction(
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
            function.Blocks.IsDefaultOrEmpty ||
            function.Allˉlocalˉnominalˉtypeˉindices.Length != function.Allˉlocalˉtypes.Length ||
            function.Valueˉnominalˉtypeˉindices.Length != function.Valueˉtypes.Length)
        {
            Fail("The descriptor-ownership oracle received inconsistent function metadata.");
        }

        var Assignedˉdescriptors = Assignedˉparameters(
            function,
            Type => Isˉdescriptor(Type));
        var Assignedˉrecords = Assignedˉparameters(
            function,
            Type => Type == Nativeˉvalueˉtype.Record);
        var Actions = new List<Nativeˉdescriptorˉownershipˉaction>();

        void Emit(
            int block,
            int operation,
            Nativeˉdescriptorˉownershipˉactionˉkind kind,
            Nativeˉdescriptorˉcarrier target,
            Nativeˉdescriptorˉcarrier source)
        {
            if (Actions.Count >= Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS)
            {
                Fail("One independently reconstructed ownership stream is oversized.");
            }
            Actions.Add(new(block, operation, kind, target, source));
        }

        foreach (var Parameter in Assignedˉdescriptors.Order())
        {
            Emit(-1, -1, Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                Carrier(Nativeˉdescriptorˉcarrierˉkind.Local, functionˉindex, Parameter),
                Carrier(Nativeˉdescriptorˉcarrierˉkind.Parameter, functionˉindex, Parameter));
        }
        foreach (var Parameter in Assignedˉrecords.Order())
        {
            foreach (var Field in Directˉdescriptorˉfields(
                module,
                function.Allˉlocalˉnominalˉtypeˉindices[Parameter]))
            {
                Emit(-1, -1, Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Fieldˉcarrier(
                        Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield,
                        functionˉindex,
                        Parameter,
                        Field),
                    Fieldˉcarrier(
                        Nativeˉdescriptorˉcarrierˉkind.Recordˉparameterˉfield,
                        functionˉindex,
                        Parameter,
                        Field));
            }
        }

        var Defined = new HashSet<int>();
        foreach (var Block in function.Blocks)
        {
            var Last = new Dictionary<int, int>();
            for (var Operation = 0; Operation < Block.Operations.Length; Operation++)
            {
                foreach (var Use in Readˉuses(Block.Operations[Operation]))
                {
                    if (Carriesˉownership(module, function, Use))
                    {
                        Last[Use] = Operation;
                    }
                }
            }
            foreach (var Use in Readˉuses(Block.Terminator))
            {
                if (Carriesˉownership(module, function, Use))
                {
                    Last[Use] = Block.Operations.Length;
                }
            }

            var Live = new HashSet<int>();
            for (var Operationˉindex = 0;
                Operationˉindex < Block.Operations.Length;
                Operationˉindex++)
            {
                var Operation = Block.Operations[Operationˉindex];
                var Result = Readˉresult(Operation);
                Emitˉmutationˉandˉcalls(
                    module,
                    function,
                    functionˉindex,
                    Block.Id,
                    Operationˉindex,
                    Operation,
                    Emit);
                if (Result >= 0 && Carriesˉownership(module, function, Result))
                {
                    if (!Defined.Add(Result) || !Live.Add(Result))
                    {
                        Fail("Independent reconstruction found a duplicate owning result.");
                    }
                    Emitˉdefinition(
                        module,
                        function,
                        functionˉindex,
                        Assignedˉdescriptors,
                        Assignedˉrecords,
                        Block.Id,
                        Operationˉindex,
                        Operation,
                        Emit);
                }

                Dropˉlastˉuses(
                    module,
                    function,
                    functionˉindex,
                    Block.Id,
                    Operationˉindex,
                    Readˉuses(Operation),
                    Last,
                    Live,
                    Emit);
                if (Result >= 0 &&
                    Carriesˉownership(module, function, Result) &&
                    !Last.ContainsKey(Result))
                {
                    Emitˉrelease(
                        module,
                        function,
                        functionˉindex,
                        Block.Id,
                        Operationˉindex,
                        Result,
                        Emit);
                    if (!Live.Remove(Result))
                    {
                        Fail("Independent reconstruction lost an unused owning result.");
                    }
                }
            }

            Emitˉterminator(
                module,
                function,
                functionˉindex,
                Assignedˉdescriptors,
                Assignedˉrecords,
                Block,
                Last,
                Live,
                Emit);
            if (Live.Count != 0)
            {
                Fail("Independent reconstruction found ownership crossing a block boundary.");
            }
        }

        var Expectedˉdefined = Enumerable.Range(0, function.Valueˉtypes.Length)
            .Count(Value => Carriesˉownership(module, function, Value));
        if (Defined.Count != Expectedˉdefined)
        {
            Fail("Independent reconstruction did not cover every owning result.");
        }

        var Frozen = Actions.ToImmutableArray();
        var Recordˉparameterˉfields = 0;
        var Assignedˉrecordˉfields = 0;
        for (var Parameter = 0; Parameter < function.Parameterˉtypes.Length; Parameter++)
        {
            if (function.Parameterˉtypes[Parameter] != Nativeˉvalueˉtype.Record)
            {
                continue;
            }
            var Count = Directˉdescriptorˉfields(
                module,
                function.Allˉlocalˉnominalˉtypeˉindices[Parameter]).Count();
            Recordˉparameterˉfields = checked(Recordˉparameterˉfields + Count);
            if (Assignedˉrecords.Contains(Parameter))
            {
                Assignedˉrecordˉfields = checked(Assignedˉrecordˉfields + Count);
            }
        }
        var Recordˉlocalˉfields = Enumerable.Range(
                function.Parameterˉtypes.Length,
                function.Localˉtypes.Length)
            .Where(Local => function.Allˉlocalˉtypes[Local] == Nativeˉvalueˉtype.Record)
            .Aggregate(
                0,
                (Count, Local) => checked(Count + Directˉdescriptorˉfields(
                    module,
                    function.Allˉlocalˉnominalˉtypeˉindices[Local]).Count()));
        var Recordˉvalueˉfields = Enumerable.Range(0, function.Valueˉtypes.Length)
            .Where(Value => function.Valueˉtypes[Value] == Nativeˉvalueˉtype.Record)
            .Aggregate(
                0,
                (Count, Value) => checked(Count + Directˉdescriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Value]).Count()));

        return new(
            functionˉindex,
            function.Name,
            function.Parameterˉtypes.Count(Type => Isˉdescriptor(Type)),
            Assignedˉdescriptors.Count,
            function.Localˉtypes.Count(Type => Isˉdescriptor(Type)),
            Recordˉparameterˉfields,
            Assignedˉrecordˉfields,
            Recordˉlocalˉfields,
            function.Valueˉtypes.Count(Type => Isˉdescriptor(Type)),
            Recordˉvalueˉfields,
            Frozen.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acquire),
            Frozen.Count(Action => Action.Kind is
                Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic or
                Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost),
            Frozen.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Retain),
            Frozen.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Release),
            Frozen.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall),
            Frozen.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn),
            Frozen.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn),
            Frozen);
    }

    private static void Emitˉdefinition(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉdescriptors,
        IReadOnlySet<int> assignedˉrecords,
        int block,
        int operationˉindex,
        Nativeˉoperation operation,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> emit)
    {
        var None = Nativeˉdescriptorˉcarrier.None;
        switch (operation)
        {
            case Nativeˉstaticˉtextˉconstant Static:
                emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Static.Result), None);
                break;
            case Nativeˉstaticˉbytesˉconstant Static:
                emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Static.Result), None);
                break;
            case Nativeˉprocessˉargument Host:
                emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Host.Result), None);
                break;
            case Nativeˉfileˉreadˉbytes Host:
                emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Host.Result), None);
                break;
            case Nativeˉbytesˉconcat Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉbytesˉfromˉu8 Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉbytesˉfromˉu16ˉlittle Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉbytesˉfromˉu32ˉlittle Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉenumˉname Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉintegerˉformat Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉtextˉconcat Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉtextˉquote Allocate:
                Acquire(Allocate.Result);
                break;
            case Nativeˉbytesˉslice Alias:
                Retainˉvalue(Alias.Result, Alias.Bytes);
                break;
            case Nativeˉtextˉfromˉutf8 Alias:
                Retainˉvalue(Alias.Result, Alias.Bytes);
                break;
            case Nativeˉtextˉtoˉutf8 Alias:
                Retainˉvalue(Alias.Result, Alias.Text);
                break;
            case Nativeˉlocalˉload Load when Isˉdescriptor(Load.Type):
                var Descriptorˉsource = Load.Local < function.Parameterˉtypes.Length &&
                    !assignedˉdescriptors.Contains(Load.Local)
                        ? Nativeˉdescriptorˉcarrierˉkind.Parameter
                        : Nativeˉdescriptorˉcarrierˉkind.Local;
                emit(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Load.Result),
                    Carrier(Descriptorˉsource, functionˉindex, Load.Local));
                break;
            case Nativeˉlocalˉload Load when Load.Type == Nativeˉvalueˉtype.Record:
                foreach (var Field in Directˉdescriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Load.Result]))
                {
                    var Recordˉsource = Load.Local < function.Parameterˉtypes.Length &&
                        !assignedˉrecords.Contains(Load.Local)
                            ? Nativeˉdescriptorˉcarrierˉkind.Recordˉparameterˉfield
                            : Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield;
                    emit(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                            functionˉindex,
                            Load.Result,
                            Field),
                        Fieldˉcarrier(Recordˉsource, functionˉindex, Load.Local, Field));
                }
                break;
            case Nativeˉrecordˉcreate Create:
                var Record = Recordˉat(module, Create.Type);
                for (var Field = 0; Field < Record.Fields.Length; Field++)
                {
                    if (Isˉdescriptor(Record.Fields[Field].Type.Kind))
                    {
                        emit(
                            block,
                            operationˉindex,
                            Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                            Fieldˉcarrier(
                                Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                                functionˉindex,
                                Create.Result,
                                Field),
                            Carrier(
                                Nativeˉdescriptorˉcarrierˉkind.Value,
                                functionˉindex,
                                Create.Fields[Field]));
                    }
                }
                break;
            case Nativeˉrecordˉfield Field when Isˉdescriptor(
                Valueˉtypeˉat(function, Field.Result)):
                emit(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Field.Result),
                    Fieldˉcarrier(
                        Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                        functionˉindex,
                        Field.Record,
                        Field.Field));
                break;
            case Nativeˉcall Call when Isˉdescriptor(Call.Type):
                emit(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Call.Result),
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Functionˉreturn, Call.Function, -1));
                break;
            case Nativeˉcall Call when Call.Type == Nativeˉvalueˉtype.Record:
                foreach (var Field in Directˉdescriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Call.Result]))
                {
                    emit(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn,
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                            functionˉindex,
                            Call.Result,
                            Field),
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Functionˉreturn,
                            Call.Function,
                            -1,
                            Field));
                }
                break;
            default:
                Fail("Independent reconstruction cannot classify an owning result.");
                break;
        }

        void Acquire(int result) => emit(
            block,
            operationˉindex,
            Nativeˉdescriptorˉownershipˉactionˉkind.Acquire,
            Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, result),
            None);
        void Retainˉvalue(int result, int source) => emit(
            block,
            operationˉindex,
            Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
            Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, result),
            Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, source));
    }

    private static void Emitˉmutationˉandˉcalls(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        int block,
        int operationˉindex,
        Nativeˉoperation operation,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> emit)
    {
        switch (operation)
        {
            case Nativeˉlocalˉstore Store when Isˉdescriptor(Store.Type):
                var Target = Carrier(
                    Nativeˉdescriptorˉcarrierˉkind.Local,
                    functionˉindex,
                    Store.Local);
                emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Target,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Store.Value));
                emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                    Nativeˉdescriptorˉcarrier.None,
                    Target);
                break;
            case Nativeˉlocalˉstore Store when Store.Type == Nativeˉvalueˉtype.Record:
                foreach (var Field in Directˉdescriptorˉfields(
                    module,
                    function.Allˉlocalˉnominalˉtypeˉindices[Store.Local]))
                {
                    var Recordˉtarget = Fieldˉcarrier(
                        Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield,
                        functionˉindex,
                        Store.Local,
                        Field);
                    emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                        Recordˉtarget,
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                            functionˉindex,
                            Store.Value,
                            Field));
                    emit(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                        Nativeˉdescriptorˉcarrier.None,
                        Recordˉtarget);
                }
                break;
            case Nativeˉcall Call:
                Emitˉcall(Call.Function, Call.Arguments);
                break;
            case Nativeˉvoidˉcall Call:
                Emitˉcall(Call.Function, Call.Arguments);
                break;
        }

        void Emitˉcall(int called, ImmutableArray<int> arguments)
        {
            if ((uint)called >= (uint)module.Functions.Length ||
                arguments.IsDefault ||
                arguments.Length != module.Functions[called].Parameterˉtypes.Length)
            {
                Fail("Independent reconstruction received an invalid call.");
            }
            var Callee = module.Functions[called];
            for (var Argument = 0; Argument < arguments.Length; Argument++)
            {
                if (Isˉdescriptor(Callee.Parameterˉtypes[Argument]))
                {
                    emit(block, operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall,
                        Carrier(Nativeˉdescriptorˉcarrierˉkind.Parameter, called, Argument),
                        Carrier(
                            Nativeˉdescriptorˉcarrierˉkind.Value,
                            functionˉindex,
                            arguments[Argument]));
                }
                else if (Callee.Parameterˉtypes[Argument] == Nativeˉvalueˉtype.Record)
                {
                    foreach (var Field in Directˉdescriptorˉfields(
                        module,
                        Callee.Allˉlocalˉnominalˉtypeˉindices[Argument]))
                    {
                        emit(block, operationˉindex,
                            Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall,
                            Fieldˉcarrier(
                                Nativeˉdescriptorˉcarrierˉkind.Recordˉparameterˉfield,
                                called,
                                Argument,
                                Field),
                            Fieldˉcarrier(
                                Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                                functionˉindex,
                                arguments[Argument],
                                Field));
                    }
                }
            }
        }
    }

    private static void Emitˉterminator(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉdescriptors,
        IReadOnlySet<int> assignedˉrecords,
        Nativeˉblock block,
        IReadOnlyDictionary<int, int> last,
        HashSet<int> live,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> emit)
    {
        var Position = block.Operations.Length;
        if (block.Terminator is Nativeˉreturn Return &&
            Carriesˉownership(module, function, Return.Value))
        {
            if (!live.Remove(Return.Value) ||
                !last.TryGetValue(Return.Value, out var Last) ||
                Last != Position)
            {
                Fail("Independent reconstruction found an invalid owning return.");
            }
            if (Isˉdescriptor(Valueˉtypeˉat(function, Return.Value)))
            {
                emit(block.Id, Position,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Functionˉreturn, functionˉindex, -1),
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, Return.Value));
            }
            else
            {
                foreach (var Field in Directˉdescriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Return.Value]))
                {
                    emit(block.Id, Position,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn,
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Functionˉreturn,
                            functionˉindex,
                            -1,
                            Field),
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                            functionˉindex,
                            Return.Value,
                            Field));
                }
            }
        }
        else
        {
            Dropˉlastˉuses(
                module,
                function,
                functionˉindex,
                block.Id,
                Position,
                Readˉuses(block.Terminator),
                last,
                live,
                emit);
        }

        if (block.Terminator is not (Nativeˉreturn or Nativeˉreturnˉvoid))
        {
            return;
        }
        for (var Local = 0; Local < function.Allˉlocalˉtypes.Length; Local++)
        {
            var Type = function.Allˉlocalˉtypes[Local];
            if (Isˉdescriptor(Type) &&
                (Local >= function.Parameterˉtypes.Length || assignedˉdescriptors.Contains(Local)))
            {
                emit(block.Id, Position, Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                    Nativeˉdescriptorˉcarrier.None,
                    Carrier(Nativeˉdescriptorˉcarrierˉkind.Local, functionˉindex, Local));
            }
            else if (Type == Nativeˉvalueˉtype.Record &&
                (Local >= function.Parameterˉtypes.Length || assignedˉrecords.Contains(Local)))
            {
                foreach (var Field in Directˉdescriptorˉfields(
                    module,
                    function.Allˉlocalˉnominalˉtypeˉindices[Local]))
                {
                    emit(block.Id, Position, Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                        Nativeˉdescriptorˉcarrier.None,
                        Fieldˉcarrier(
                            Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield,
                            functionˉindex,
                            Local,
                            Field));
                }
            }
        }
    }

    private static void Dropˉlastˉuses(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        int block,
        int operation,
        IEnumerable<int> uses,
        IReadOnlyDictionary<int, int> last,
        HashSet<int> live,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> emit)
    {
        foreach (var Use in uses.Distinct())
        {
            if (!Carriesˉownership(module, function, Use))
            {
                continue;
            }
            if (!live.Contains(Use))
            {
                Fail("Independent reconstruction found a use outside its owning lifetime.");
            }
            if (!last.TryGetValue(Use, out var Last))
            {
                throw Unsupported();
            }
            if (Last == operation)
            {
                Emitˉrelease(
                    module,
                    function,
                    functionˉindex,
                    block,
                    operation,
                    Use,
                    emit);
                live.Remove(Use);
            }
        }
    }

    private static void Emitˉrelease(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        int block,
        int operation,
        int value,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> emit)
    {
        if (Isˉdescriptor(Valueˉtypeˉat(function, value)))
        {
            emit(block, operation, Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                Nativeˉdescriptorˉcarrier.None,
                Carrier(Nativeˉdescriptorˉcarrierˉkind.Value, functionˉindex, value));
            return;
        }
        foreach (var Field in Directˉdescriptorˉfields(
            module,
            function.Valueˉnominalˉtypeˉindices[value]))
        {
            emit(block, operation, Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                Nativeˉdescriptorˉcarrier.None,
                Fieldˉcarrier(
                    Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
                    functionˉindex,
                    value,
                    Field));
        }
    }

    private static int Readˉresult(Nativeˉoperation operation) => operation switch
    {
        Nativeˉinstructionˉcharge or Nativeˉlocalˉstore or
        Nativeˉconsoleˉwriteˉline or Nativeˉdiagnosticˉwriteˉline or
        Nativeˉfileˉwriteˉbytes or Nativeˉvoidˉcall => -1,
        Nativeˉi32ˉconstant Result => Result.Result,
        Nativeˉboolˉconstant Result => Result.Result,
        Nativeˉu8ˉconstant Result => Result.Result,
        Nativeˉu32ˉconstant Result => Result.Result,
        Nativeˉenumˉconstant Result => Result.Result,
        Nativeˉlocalˉload Result => Result.Result,
        Nativeˉi32ˉbinary Result => Result.Result,
        Nativeˉi32ˉnegate Result => Result.Result,
        Nativeˉi32ˉcomparison Result => Result.Result,
        Nativeˉboolˉcomparison Result => Result.Result,
        Nativeˉboolˉnot Result => Result.Result,
        Nativeˉu32ˉbinary Result => Result.Result,
        Nativeˉu32ˉcomparison Result => Result.Result,
        Nativeˉu8ˉcomparison Result => Result.Result,
        Nativeˉenumˉcomparison Result => Result.Result,
        Nativeˉu32ˉfromˉu8 Result => Result.Result,
        Nativeˉcall Result => Result.Result,
        Nativeˉdataˉlength Result => Result.Result,
        Nativeˉdataˉloadˉi32 Result => Result.Result,
        Nativeˉstaticˉtextˉconstant Result => Result.Result,
        Nativeˉstaticˉbytesˉconstant Result => Result.Result,
        Nativeˉbytesˉlength Result => Result.Result,
        Nativeˉbytesˉslice Result => Result.Result,
        Nativeˉbytesˉread Result => Result.Result,
        Nativeˉbytesˉconcat Result => Result.Result,
        Nativeˉbytesˉfromˉu8 Result => Result.Result,
        Nativeˉbytesˉfromˉu16ˉlittle Result => Result.Result,
        Nativeˉbytesˉfromˉu32ˉlittle Result => Result.Result,
        Nativeˉtextˉutf8ˉisˉvalid Result => Result.Result,
        Nativeˉtextˉfromˉutf8 Result => Result.Result,
        Nativeˉtextˉtoˉutf8 Result => Result.Result,
        Nativeˉenumˉname Result => Result.Result,
        Nativeˉintegerˉformat Result => Result.Result,
        Nativeˉtextˉconcat Result => Result.Result,
        Nativeˉtextˉquote Result => Result.Result,
        Nativeˉrecordˉcreate Result => Result.Result,
        Nativeˉrecordˉfield Result => Result.Result,
        Nativeˉprocessˉargumentˉcount Result => Result.Result,
        Nativeˉprocessˉargument Result => Result.Result,
        Nativeˉfileˉreadˉbytes Result => Result.Result,
        _ => throw Unsupported(),
    };

    private static IEnumerable<int> Readˉuses(Nativeˉoperation operation) => operation switch
    {
        Nativeˉinstructionˉcharge or Nativeˉi32ˉconstant or Nativeˉboolˉconstant or
        Nativeˉu8ˉconstant or Nativeˉu32ˉconstant or Nativeˉenumˉconstant or
        Nativeˉlocalˉload or Nativeˉdataˉlength or Nativeˉstaticˉtextˉconstant or
        Nativeˉstaticˉbytesˉconstant or Nativeˉprocessˉargumentˉcount => [],
        Nativeˉlocalˉstore Use => [Use.Value],
        Nativeˉi32ˉbinary Use => [Use.Left, Use.Right],
        Nativeˉi32ˉnegate Use => [Use.Value],
        Nativeˉi32ˉcomparison Use => [Use.Left, Use.Right],
        Nativeˉboolˉcomparison Use => [Use.Left, Use.Right],
        Nativeˉboolˉnot Use => [Use.Value],
        Nativeˉu32ˉbinary Use => [Use.Left, Use.Right],
        Nativeˉu32ˉcomparison Use => [Use.Left, Use.Right],
        Nativeˉu8ˉcomparison Use => [Use.Left, Use.Right],
        Nativeˉenumˉcomparison Use => [Use.Left, Use.Right],
        Nativeˉu32ˉfromˉu8 Use => [Use.Value],
        Nativeˉcall Use => Use.Arguments,
        Nativeˉdataˉloadˉi32 Use => [Use.Index],
        Nativeˉbytesˉlength Use => [Use.Bytes],
        Nativeˉbytesˉslice Use => [Use.Bytes, Use.Offset, Use.Length],
        Nativeˉbytesˉread Use => [Use.Bytes, Use.Offset],
        Nativeˉbytesˉconcat Use => [Use.Left, Use.Right],
        Nativeˉbytesˉfromˉu8 Use => [Use.Value],
        Nativeˉbytesˉfromˉu16ˉlittle Use => [Use.Value],
        Nativeˉbytesˉfromˉu32ˉlittle Use => [Use.Value],
        Nativeˉtextˉutf8ˉisˉvalid Use => [Use.Bytes],
        Nativeˉtextˉfromˉutf8 Use => [Use.Bytes],
        Nativeˉtextˉtoˉutf8 Use => [Use.Text],
        Nativeˉenumˉname Use => [Use.Value],
        Nativeˉintegerˉformat Use => [Use.Value],
        Nativeˉtextˉconcat Use => [Use.Left, Use.Right],
        Nativeˉtextˉquote Use => [Use.Text],
        Nativeˉrecordˉcreate Use => Use.Fields,
        Nativeˉrecordˉfield Use => [Use.Record],
        Nativeˉconsoleˉwriteˉline Use => [Use.Text],
        Nativeˉdiagnosticˉwriteˉline Use => [Use.Text],
        Nativeˉprocessˉargument Use => [Use.Index],
        Nativeˉfileˉreadˉbytes Use => [Use.Resourceˉname],
        Nativeˉfileˉwriteˉbytes Use => [Use.Resourceˉname, Use.Bytes],
        Nativeˉvoidˉcall Use => Use.Arguments,
        _ => throw Unsupported(),
    };

    private static IEnumerable<int> Readˉuses(Nativeˉterminator terminator) => terminator switch
    {
        Nativeˉjump or Nativeˉreturnˉvoid => [],
        Nativeˉbranch Use => [Use.Condition],
        Nativeˉreturn Use => [Use.Value],
        _ => throw Unsupported(),
    };

    private static HashSet<int> Assignedˉparameters(
        Nativeˉfunction function,
        Func<Nativeˉvalueˉtype, bool> select) => function.Blocks
        .SelectMany(Block => Block.Operations)
        .OfType<Nativeˉlocalˉstore>()
        .Where(Store =>
            Store.Local >= 0 &&
            Store.Local < function.Parameterˉtypes.Length &&
            select(Store.Type))
        .Select(Store => Store.Local)
        .ToHashSet();

    private static bool Carriesˉownership(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int value)
    {
        var Type = Valueˉtypeˉat(function, value);
        return Isˉdescriptor(Type) ||
            (Type == Nativeˉvalueˉtype.Record && Directˉdescriptorˉfields(
                module,
                function.Valueˉnominalˉtypeˉindices[value]).Any());
    }

    private static Nativeˉvalueˉtype Valueˉtypeˉat(Nativeˉfunction function, int value)
    {
        if ((uint)value >= (uint)function.Valueˉtypes.Length)
        {
            Fail("The descriptor-ownership oracle received an invalid value identity.");
        }
        return function.Valueˉtypes[value];
    }

    private static IEnumerable<int> Directˉdescriptorˉfields(
        Nativeˉmodule module,
        int typeˉindex)
    {
        var Record = Recordˉat(module, typeˉindex);
        for (var Field = 0; Field < Record.Fields.Length; Field++)
        {
            if (Record.Fields[Field].Type.Kind == Valueˉtype.Record)
            {
                Fail("The descriptor-ownership oracle rejects nested native records.");
            }
            if (Isˉdescriptor(Record.Fields[Field].Type.Kind))
            {
                yield return Field;
            }
        }
    }

    private static Recordˉtypeˉdeclaration Recordˉat(Nativeˉmodule module, int type)
    {
        if ((uint)type >= (uint)module.Types.Length)
        {
            Fail("The descriptor-ownership oracle received an invalid record identity.");
        }
        return module.Types[type] as Recordˉtypeˉdeclaration ?? throw Unsupported();
    }

    private static bool Isˉdescriptor(Nativeˉvalueˉtype type) => type is
        Nativeˉvalueˉtype.Borrowedˉtext or Nativeˉvalueˉtype.Borrowedˉbytes;
    private static bool Isˉdescriptor(Valueˉtype type) => type is
        Valueˉtype.Text or Valueˉtype.Bytes;

    private static Nativeˉdescriptorˉcarrier Carrier(
        Nativeˉdescriptorˉcarrierˉkind kind,
        int function,
        int binding) => new(kind, function, binding, -1);
    private static Nativeˉdescriptorˉcarrier Fieldˉcarrier(
        Nativeˉdescriptorˉcarrierˉkind kind,
        int function,
        int binding,
        int field) => new(kind, function, binding, field);

    private static Nativeˉbackendˉexception Unsupported() => new(
        "WVN2903",
        "The descriptor-ownership oracle received unsupported machine IR.");

    [DoesNotReturn]
    private static void Fail(string message) =>
        throw new Nativeˉbackendˉexception("WVN2903", message);
}
