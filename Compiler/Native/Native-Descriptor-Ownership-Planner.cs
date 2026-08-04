using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.Bytecode;

namespace Windvale.Compiler.Native;

public static class Nativeˉdescriptorˉownershipˉplanner
{
    public const uint FORMAT_VERSION = 1;

    public static Nativeˉdescriptorˉownershipˉplan Plan(Nativeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (module.Functions.IsDefault || module.Types.IsDefault)
        {
            Fail("The native descriptor-ownership planner requires initialized module metadata.");
        }

        var Functions = module.Functions
            .Select((Function, Index) => Planˉfunction(module, Function, Index))
            .ToImmutableArray();
        var Totalˉactions = Functions.Aggregate(
            0,
            (Total, Function) => checked(Total + Function.Actions.Length));
        if (Totalˉactions > Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS)
        {
            Fail(
                $"The native descriptor-ownership plan exceeds " +
                $"{Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS} bounded actions.");
        }
        return new(
            FORMAT_VERSION,
            Terminalˉfailureˉdiscardsˉarena: true,
            Totalˉactions,
            Functions);
    }

    private static Nativeˉfunctionˉdescriptorˉownership Planˉfunction(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex)
    {
        Requireˉfunction(function);
        var Assignedˉdescriptorˉparameters = Assignedˉparameters(
            function,
            Isˉdescriptor);
        var Assignedˉrecordˉparameters = Assignedˉparameters(
            function,
            Type => Type == Nativeˉvalueˉtype.Record);
        var Actions = ImmutableArray.CreateBuilder<Nativeˉdescriptorˉownershipˉaction>();

        void Add(
            int block,
            int operation,
            Nativeˉdescriptorˉownershipˉactionˉkind kind,
            Nativeˉdescriptorˉcarrier target,
            Nativeˉdescriptorˉcarrier source)
        {
            if (Actions.Count >= Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS)
            {
                Fail("One native function exceeds the bounded descriptor-ownership action limit.");
            }
            Actions.Add(new(block, operation, kind, target, source));
        }

        foreach (var Parameter in Assignedˉdescriptorˉparameters.Order())
        {
            Add(
                -1,
                -1,
                Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                Local(functionˉindex, Parameter),
                Parameterˉcarrier(functionˉindex, Parameter));
        }
        foreach (var Parameter in Assignedˉrecordˉparameters.Order())
        {
            foreach (var Field in Descriptorˉfields(
                module,
                function.Allˉlocalˉnominalˉtypeˉindices[Parameter]))
            {
                Add(
                    -1,
                    -1,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Recordˉlocalˉfield(functionˉindex, Parameter, Field),
                    Recordˉparameterˉfield(functionˉindex, Parameter, Field));
            }
        }

        var Defined = new HashSet<int>();
        foreach (var Block in function.Blocks)
        {
            var Lastˉuses = new Dictionary<int, int>();
            for (var Operation = 0; Operation < Block.Operations.Length; Operation++)
            {
                foreach (var Value in Uses(Block.Operations[Operation]))
                {
                    if (Isˉownershipˉvalue(module, function, Value))
                    {
                        Lastˉuses[Value] = Operation;
                    }
                }
            }
            foreach (var Value in Uses(Block.Terminator))
            {
                if (Isˉownershipˉvalue(module, function, Value))
                {
                    Lastˉuses[Value] = Block.Operations.Length;
                }
            }

            var Live = new HashSet<int>();
            for (var Operationˉindex = 0;
                Operationˉindex < Block.Operations.Length;
                Operationˉindex++)
            {
                var Operation = Block.Operations[Operationˉindex];
                var Result = Resultˉof(Operation);
                Addˉsideˉeffectˉactions(
                    module,
                    function,
                    functionˉindex,
                    Assignedˉdescriptorˉparameters,
                    Assignedˉrecordˉparameters,
                    Block.Id,
                    Operationˉindex,
                    Operation,
                    Add);
                if (Result >= 0 && Isˉownershipˉvalue(module, function, Result))
                {
                    if (!Defined.Add(Result) || !Live.Add(Result))
                    {
                        Fail("The native descriptor-ownership planner received a duplicate owning result.");
                    }
                    Addˉdefinitionˉactions(
                        module,
                        function,
                        functionˉindex,
                        Assignedˉdescriptorˉparameters,
                        Assignedˉrecordˉparameters,
                        Block.Id,
                        Operationˉindex,
                        Operation,
                        Add);
                }

                Releaseˉlastˉuses(
                    module,
                    function,
                    functionˉindex,
                    Block.Id,
                    Operationˉindex,
                    Uses(Operation),
                    Lastˉuses,
                    Live,
                    Add);
                if (Result >= 0 &&
                    Isˉownershipˉvalue(module, function, Result) &&
                    !Lastˉuses.ContainsKey(Result))
                {
                    Addˉreleaseˉactions(
                        module,
                        function,
                        functionˉindex,
                        Block.Id,
                        Operationˉindex,
                        Result,
                        Add);
                    if (!Live.Remove(Result))
                    {
                        Fail("The native descriptor-ownership planner lost an unused owning result.");
                    }
                }
            }

            Addˉterminatorˉactions(
                module,
                function,
                functionˉindex,
                Assignedˉdescriptorˉparameters,
                Assignedˉrecordˉparameters,
                Block,
                Lastˉuses,
                Live,
                Add);
            if (Live.Count != 0)
            {
                Fail("The native descriptor-ownership planner found an owning value live across a block boundary.");
            }
        }

        var Expectedˉvalues = Enumerable.Range(0, function.Valueˉtypes.Length)
            .Count(Value => Isˉownershipˉvalue(module, function, Value));
        if (Defined.Count != Expectedˉvalues)
        {
            Fail("The native descriptor-ownership planner could not account for every owning value.");
        }

        var Frozen = Actions.ToImmutable();
        return Summarize(
            module,
            function,
            functionˉindex,
            Assignedˉdescriptorˉparameters,
            Assignedˉrecordˉparameters,
            Frozen);
    }

    private static void Addˉdefinitionˉactions(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉdescriptorˉparameters,
        IReadOnlySet<int> assignedˉrecordˉparameters,
        int block,
        int operationˉindex,
        Nativeˉoperation operation,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> add)
    {
        switch (operation)
        {
            case Nativeˉstaticˉtextˉconstant Static:
                add(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic,
                    Value(functionˉindex, Static.Result), Nativeˉdescriptorˉcarrier.None);
                return;
            case Nativeˉstaticˉbytesˉconstant Static:
                add(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic,
                    Value(functionˉindex, Static.Result), Nativeˉdescriptorˉcarrier.None);
                return;
            case Nativeˉprocessˉargument Host:
                add(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost,
                    Value(functionˉindex, Host.Result), Nativeˉdescriptorˉcarrier.None);
                return;
            case Nativeˉfileˉreadˉbytes Host:
                add(block, operationˉindex, Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost,
                    Value(functionˉindex, Host.Result), Nativeˉdescriptorˉcarrier.None);
                return;
            case Nativeˉbytesˉconcat Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉbytesˉfromˉu8 Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉbytesˉfromˉu16ˉlittle Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉbytesˉfromˉu32ˉlittle Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉenumˉname Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉintegerˉformat Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉtextˉconcat Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉtextˉquote Allocate:
                Addˉacquire(Allocate.Result);
                return;
            case Nativeˉlocalˉload Load when Isˉdescriptor(Load.Type):
                add(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Value(functionˉindex, Load.Result),
                    Descriptorˉlocalˉsource(
                        function,
                        functionˉindex,
                        assignedˉdescriptorˉparameters,
                        Load.Local));
                return;
            case Nativeˉlocalˉload Load when Load.Type == Nativeˉvalueˉtype.Record:
                foreach (var Field in Descriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Load.Result]))
                {
                    add(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                        Recordˉvalueˉfield(functionˉindex, Load.Result, Field),
                        Recordˉlocalˉsource(
                            function,
                            functionˉindex,
                            assignedˉrecordˉparameters,
                            Load.Local,
                            Field));
                }
                return;
            case Nativeˉbytesˉslice Alias:
                Addˉalias(Alias.Result, Alias.Bytes);
                return;
            case Nativeˉtextˉfromˉutf8 Alias:
                Addˉalias(Alias.Result, Alias.Bytes);
                return;
            case Nativeˉtextˉtoˉutf8 Alias:
                Addˉalias(Alias.Result, Alias.Text);
                return;
            case Nativeˉrecordˉfield Field when Isˉdescriptor(
                Requireˉvalueˉtype(function, Field.Result)):
                add(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Value(functionˉindex, Field.Result),
                    Recordˉvalueˉfield(functionˉindex, Field.Record, Field.Field));
                return;
            case Nativeˉrecordˉcreate Create:
                var Record = Requireˉrecord(module, Create.Type);
                for (var Field = 0; Field < Record.Fields.Length; Field++)
                {
                    if (Isˉdescriptor(Record.Fields[Field].Type.Kind))
                    {
                        add(
                            block,
                            operationˉindex,
                            Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                            Recordˉvalueˉfield(functionˉindex, Create.Result, Field),
                            Value(functionˉindex, Create.Fields[Field]));
                    }
                }
                return;
            case Nativeˉcall Call when Isˉdescriptor(Call.Type):
                add(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn,
                    Value(functionˉindex, Call.Result),
                    Functionˉreturn(Call.Function));
                return;
            case Nativeˉcall Call when Call.Type == Nativeˉvalueˉtype.Record:
                foreach (var Field in Descriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Call.Result]))
                {
                    add(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn,
                        Recordˉvalueˉfield(functionˉindex, Call.Result, Field),
                        Functionˉreturn(Call.Function, Field));
                }
                return;
            default:
                Fail("The native descriptor-ownership planner cannot classify an owning result.");
                return;
        }

        void Addˉacquire(int result) => add(
            block,
            operationˉindex,
            Nativeˉdescriptorˉownershipˉactionˉkind.Acquire,
            Value(functionˉindex, result),
            Nativeˉdescriptorˉcarrier.None);

        void Addˉalias(int result, int source) => add(
            block,
            operationˉindex,
            Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
            Value(functionˉindex, result),
            Value(functionˉindex, source));
    }

    private static void Addˉsideˉeffectˉactions(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉdescriptorˉparameters,
        IReadOnlySet<int> assignedˉrecordˉparameters,
        int block,
        int operationˉindex,
        Nativeˉoperation operation,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> add)
    {
        switch (operation)
        {
            case Nativeˉlocalˉstore Store when Isˉdescriptor(Store.Type):
                var Descriptorˉtarget = Local(functionˉindex, Store.Local);
                add(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                    Descriptorˉtarget,
                    Value(functionˉindex, Store.Value));
                add(
                    block,
                    operationˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                    Nativeˉdescriptorˉcarrier.None,
                    Descriptorˉtarget);
                return;
            case Nativeˉlocalˉstore Store when Store.Type == Nativeˉvalueˉtype.Record:
                foreach (var Field in Descriptorˉfields(
                    module,
                    function.Allˉlocalˉnominalˉtypeˉindices[Store.Local]))
                {
                    var Target = Recordˉlocalˉfield(functionˉindex, Store.Local, Field);
                    add(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Retain,
                        Target,
                        Recordˉvalueˉfield(functionˉindex, Store.Value, Field));
                    add(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                        Nativeˉdescriptorˉcarrier.None,
                        Target);
                }
                return;
            case Nativeˉcall Call:
                Addˉcallˉborrows(Call.Function, Call.Arguments);
                return;
            case Nativeˉvoidˉcall Call:
                Addˉcallˉborrows(Call.Function, Call.Arguments);
                return;
            default:
                return;
        }

        void Addˉcallˉborrows(int calledˉfunction, ImmutableArray<int> arguments)
        {
            if ((uint)calledˉfunction >= (uint)module.Functions.Length ||
                arguments.IsDefault ||
                arguments.Length != module.Functions[calledˉfunction].Parameterˉtypes.Length)
            {
                Fail("The native descriptor-ownership planner received invalid call metadata.");
            }
            var Callee = module.Functions[calledˉfunction];
            for (var Argument = 0; Argument < arguments.Length; Argument++)
            {
                if (Isˉdescriptor(Callee.Parameterˉtypes[Argument]))
                {
                    add(
                        block,
                        operationˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall,
                        Parameterˉcarrier(calledˉfunction, Argument),
                        Value(functionˉindex, arguments[Argument]));
                }
                else if (Callee.Parameterˉtypes[Argument] == Nativeˉvalueˉtype.Record)
                {
                    foreach (var Field in Descriptorˉfields(
                        module,
                        Callee.Allˉlocalˉnominalˉtypeˉindices[Argument]))
                    {
                        add(
                            block,
                            operationˉindex,
                            Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall,
                            Recordˉparameterˉfield(calledˉfunction, Argument, Field),
                            Recordˉvalueˉfield(functionˉindex, arguments[Argument], Field));
                    }
                }
            }
        }
    }

    private static void Addˉterminatorˉactions(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉdescriptorˉparameters,
        IReadOnlySet<int> assignedˉrecordˉparameters,
        Nativeˉblock block,
        IReadOnlyDictionary<int, int> lastˉuses,
        HashSet<int> live,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> add)
    {
        var Terminatorˉindex = block.Operations.Length;
        if (block.Terminator is Nativeˉreturn Return &&
            Isˉownershipˉvalue(module, function, Return.Value))
        {
            if (!live.Remove(Return.Value) ||
                !lastˉuses.TryGetValue(Return.Value, out var Last) ||
                Last != Terminatorˉindex)
            {
                Fail("The native descriptor-ownership planner received an invalid owning return.");
            }
            if (Isˉdescriptor(Requireˉvalueˉtype(function, Return.Value)))
            {
                add(
                    block.Id,
                    Terminatorˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn,
                    Functionˉreturn(functionˉindex),
                    Value(functionˉindex, Return.Value));
            }
            else
            {
                foreach (var Field in Descriptorˉfields(
                    module,
                    function.Valueˉnominalˉtypeˉindices[Return.Value]))
                {
                    add(
                        block.Id,
                        Terminatorˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn,
                        Functionˉreturn(functionˉindex, Field),
                        Recordˉvalueˉfield(functionˉindex, Return.Value, Field));
                }
            }
        }
        else
        {
            Releaseˉlastˉuses(
                module,
                function,
                functionˉindex,
                block.Id,
                Terminatorˉindex,
                Uses(block.Terminator),
                lastˉuses,
                live,
                add);
        }

        if (block.Terminator is not (Nativeˉreturn or Nativeˉreturnˉvoid))
        {
            return;
        }
        for (var Localˉindex = 0; Localˉindex < function.Allˉlocalˉtypes.Length; Localˉindex++)
        {
            var Type = function.Allˉlocalˉtypes[Localˉindex];
            if (Isˉdescriptor(Type) &&
                (Localˉindex >= function.Parameterˉtypes.Length ||
                    assignedˉdescriptorˉparameters.Contains(Localˉindex)))
            {
                add(
                    block.Id,
                    Terminatorˉindex,
                    Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                    Nativeˉdescriptorˉcarrier.None,
                    Local(functionˉindex, Localˉindex));
            }
            else if (Type == Nativeˉvalueˉtype.Record &&
                (Localˉindex >= function.Parameterˉtypes.Length ||
                    assignedˉrecordˉparameters.Contains(Localˉindex)))
            {
                foreach (var Field in Descriptorˉfields(
                    module,
                    function.Allˉlocalˉnominalˉtypeˉindices[Localˉindex]))
                {
                    add(
                        block.Id,
                        Terminatorˉindex,
                        Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                        Nativeˉdescriptorˉcarrier.None,
                        Recordˉlocalˉfield(functionˉindex, Localˉindex, Field));
                }
            }
        }
    }

    private static void Releaseˉlastˉuses(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        int block,
        int operation,
        IEnumerable<int> uses,
        IReadOnlyDictionary<int, int> lastˉuses,
        HashSet<int> live,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> add)
    {
        foreach (var Value in uses.Distinct())
        {
            if (!Isˉownershipˉvalue(module, function, Value))
            {
                continue;
            }
            if (!live.Contains(Value))
            {
                Fail("The native descriptor-ownership planner found an owning use outside its defining block.");
            }
            if (!lastˉuses.TryGetValue(Value, out var Last))
            {
                throw Invalidˉoperation();
            }
            if (Last == operation)
            {
                Addˉreleaseˉactions(
                    module,
                    function,
                    functionˉindex,
                    block,
                    operation,
                    Value,
                    add);
                live.Remove(Value);
            }
        }
    }

    private static void Addˉreleaseˉactions(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        int block,
        int operation,
        int value,
        Action<int, int, Nativeˉdescriptorˉownershipˉactionˉkind,
            Nativeˉdescriptorˉcarrier, Nativeˉdescriptorˉcarrier> add)
    {
        if (Isˉdescriptor(Requireˉvalueˉtype(function, value)))
        {
            add(
                block,
                operation,
                Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                Nativeˉdescriptorˉcarrier.None,
                Value(functionˉindex, value));
            return;
        }
        foreach (var Field in Descriptorˉfields(
            module,
            function.Valueˉnominalˉtypeˉindices[value]))
        {
            add(
                block,
                operation,
                Nativeˉdescriptorˉownershipˉactionˉkind.Release,
                Nativeˉdescriptorˉcarrier.None,
                Recordˉvalueˉfield(functionˉindex, value, Field));
        }
    }

    private static Nativeˉfunctionˉdescriptorˉownership Summarize(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉdescriptorˉparameters,
        IReadOnlySet<int> assignedˉrecordˉparameters,
        ImmutableArray<Nativeˉdescriptorˉownershipˉaction> actions)
    {
        var Descriptorˉparameterˉbindings = function.Parameterˉtypes.Count(Isˉdescriptor);
        var Descriptorˉlocalˉbindings = function.Localˉtypes.Count(Isˉdescriptor);
        var Recordˉparameterˉdescriptorˉfields = 0;
        var Assignedˉrecordˉparameterˉdescriptorˉfields = 0;
        for (var Parameter = 0; Parameter < function.Parameterˉtypes.Length; Parameter++)
        {
            if (function.Parameterˉtypes[Parameter] != Nativeˉvalueˉtype.Record)
            {
                continue;
            }
            var Count = Descriptorˉfields(
                module,
                function.Allˉlocalˉnominalˉtypeˉindices[Parameter]).Count();
            Recordˉparameterˉdescriptorˉfields = checked(
                Recordˉparameterˉdescriptorˉfields + Count);
            if (assignedˉrecordˉparameters.Contains(Parameter))
            {
                Assignedˉrecordˉparameterˉdescriptorˉfields = checked(
                    Assignedˉrecordˉparameterˉdescriptorˉfields + Count);
            }
        }
        var Recordˉlocalˉdescriptorˉfields = 0;
        for (var Localˉindex = function.Parameterˉtypes.Length;
            Localˉindex < function.Allˉlocalˉtypes.Length;
            Localˉindex++)
        {
            if (function.Allˉlocalˉtypes[Localˉindex] == Nativeˉvalueˉtype.Record)
            {
                Recordˉlocalˉdescriptorˉfields = checked(
                    Recordˉlocalˉdescriptorˉfields + Descriptorˉfields(
                        module,
                        function.Allˉlocalˉnominalˉtypeˉindices[Localˉindex]).Count());
            }
        }
        var Recordˉvalueˉdescriptorˉfields = 0;
        for (var Valueˉindex = 0; Valueˉindex < function.Valueˉtypes.Length; Valueˉindex++)
        {
            if (function.Valueˉtypes[Valueˉindex] == Nativeˉvalueˉtype.Record)
            {
                Recordˉvalueˉdescriptorˉfields = checked(
                    Recordˉvalueˉdescriptorˉfields + Descriptorˉfields(
                        module,
                        function.Valueˉnominalˉtypeˉindices[Valueˉindex]).Count());
            }
        }
        return new(
            functionˉindex,
            function.Name,
            Descriptorˉparameterˉbindings,
            assignedˉdescriptorˉparameters.Count,
            Descriptorˉlocalˉbindings,
            Recordˉparameterˉdescriptorˉfields,
            Assignedˉrecordˉparameterˉdescriptorˉfields,
            Recordˉlocalˉdescriptorˉfields,
            function.Valueˉtypes.Count(Isˉdescriptor),
            Recordˉvalueˉdescriptorˉfields,
            actions.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acquire),
            actions.Count(Action => Action.Kind is
                Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉstatic or
                Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉhost),
            actions.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Retain),
            actions.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Release),
            actions.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Borrowˉcall),
            actions.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Acceptˉreturn),
            actions.Count(Action => Action.Kind == Nativeˉdescriptorˉownershipˉactionˉkind.Transferˉreturn),
            actions);
    }

    private static HashSet<int> Assignedˉparameters(
        Nativeˉfunction function,
        Func<Nativeˉvalueˉtype, bool> predicate) => function.Blocks
        .SelectMany(Block => Block.Operations)
        .OfType<Nativeˉlocalˉstore>()
        .Where(Store =>
            Store.Local >= 0 &&
            Store.Local < function.Parameterˉtypes.Length &&
            predicate(Store.Type))
        .Select(Store => Store.Local)
        .ToHashSet();

    private static int Resultˉof(Nativeˉoperation operation) => operation switch
    {
        Nativeˉinstructionˉcharge => -1,
        Nativeˉi32ˉconstant Value => Value.Result,
        Nativeˉboolˉconstant Value => Value.Result,
        Nativeˉu8ˉconstant Value => Value.Result,
        Nativeˉu32ˉconstant Value => Value.Result,
        Nativeˉenumˉconstant Value => Value.Result,
        Nativeˉlocalˉload Value => Value.Result,
        Nativeˉlocalˉstore => -1,
        Nativeˉi32ˉbinary Value => Value.Result,
        Nativeˉi32ˉnegate Value => Value.Result,
        Nativeˉi32ˉcomparison Value => Value.Result,
        Nativeˉboolˉcomparison Value => Value.Result,
        Nativeˉboolˉnot Value => Value.Result,
        Nativeˉu32ˉbinary Value => Value.Result,
        Nativeˉu32ˉbitwiseˉnot Value => Value.Result,
        Nativeˉu32ˉcomparison Value => Value.Result,
        Nativeˉu8ˉcomparison Value => Value.Result,
        Nativeˉenumˉcomparison Value => Value.Result,
        Nativeˉu32ˉfromˉu8 Value => Value.Result,
        Nativeˉcall Value => Value.Result,
        Nativeˉdataˉlength Value => Value.Result,
        Nativeˉdataˉloadˉi32 Value => Value.Result,
        Nativeˉstaticˉtextˉconstant Value => Value.Result,
        Nativeˉstaticˉbytesˉconstant Value => Value.Result,
        Nativeˉbytesˉlength Value => Value.Result,
        Nativeˉbytesˉslice Value => Value.Result,
        Nativeˉbytesˉread Value => Value.Result,
        Nativeˉbytesˉconcat Value => Value.Result,
        Nativeˉbytesˉfromˉu8 Value => Value.Result,
        Nativeˉbytesˉfromˉu16ˉlittle Value => Value.Result,
        Nativeˉbytesˉfromˉu32ˉlittle Value => Value.Result,
        Nativeˉtextˉutf8ˉisˉvalid Value => Value.Result,
        Nativeˉtextˉfromˉutf8 Value => Value.Result,
        Nativeˉtextˉtoˉutf8 Value => Value.Result,
        Nativeˉenumˉname Value => Value.Result,
        Nativeˉintegerˉformat Value => Value.Result,
        Nativeˉtextˉconcat Value => Value.Result,
        Nativeˉtextˉquote Value => Value.Result,
        Nativeˉrecordˉcreate Value => Value.Result,
        Nativeˉrecordˉfield Value => Value.Result,
        Nativeˉconsoleˉwriteˉline => -1,
        Nativeˉdiagnosticˉwriteˉline => -1,
        Nativeˉprocessˉargumentˉcount Value => Value.Result,
        Nativeˉprocessˉargument Value => Value.Result,
        Nativeˉfileˉreadˉbytes Value => Value.Result,
        Nativeˉfileˉwriteˉbytes => -1,
        Nativeˉvoidˉcall => -1,
        _ => throw Invalidˉoperation(),
    };

    private static IEnumerable<int> Uses(Nativeˉoperation operation) => operation switch
    {
        Nativeˉinstructionˉcharge or
        Nativeˉi32ˉconstant or
        Nativeˉboolˉconstant or
        Nativeˉu8ˉconstant or
        Nativeˉu32ˉconstant or
        Nativeˉenumˉconstant or
        Nativeˉstaticˉtextˉconstant or
        Nativeˉstaticˉbytesˉconstant or
        Nativeˉdataˉlength or
        Nativeˉprocessˉargumentˉcount => [],
        Nativeˉlocalˉload => [],
        Nativeˉlocalˉstore Store => [Store.Value],
        Nativeˉi32ˉbinary Value => [Value.Left, Value.Right],
        Nativeˉi32ˉnegate Value => [Value.Value],
        Nativeˉi32ˉcomparison Value => [Value.Left, Value.Right],
        Nativeˉboolˉcomparison Value => [Value.Left, Value.Right],
        Nativeˉboolˉnot Value => [Value.Value],
        Nativeˉu32ˉbinary Value => [Value.Left, Value.Right],
        Nativeˉu32ˉbitwiseˉnot Value => [Value.Value],
        Nativeˉu32ˉcomparison Value => [Value.Left, Value.Right],
        Nativeˉu8ˉcomparison Value => [Value.Left, Value.Right],
        Nativeˉenumˉcomparison Value => [Value.Left, Value.Right],
        Nativeˉu32ˉfromˉu8 Value => [Value.Value],
        Nativeˉcall Value => Value.Arguments,
        Nativeˉdataˉloadˉi32 Value => [Value.Index],
        Nativeˉbytesˉlength Value => [Value.Bytes],
        Nativeˉbytesˉslice Value => [Value.Bytes, Value.Offset, Value.Length],
        Nativeˉbytesˉread Value => [Value.Bytes, Value.Offset],
        Nativeˉbytesˉconcat Value => [Value.Left, Value.Right],
        Nativeˉbytesˉfromˉu8 Value => [Value.Value],
        Nativeˉbytesˉfromˉu16ˉlittle Value => [Value.Value],
        Nativeˉbytesˉfromˉu32ˉlittle Value => [Value.Value],
        Nativeˉtextˉutf8ˉisˉvalid Value => [Value.Bytes],
        Nativeˉtextˉfromˉutf8 Value => [Value.Bytes],
        Nativeˉtextˉtoˉutf8 Value => [Value.Text],
        Nativeˉenumˉname Value => [Value.Value],
        Nativeˉintegerˉformat Value => [Value.Value],
        Nativeˉtextˉconcat Value => [Value.Left, Value.Right],
        Nativeˉtextˉquote Value => [Value.Text],
        Nativeˉrecordˉcreate Value => Value.Fields,
        Nativeˉrecordˉfield Value => [Value.Record],
        Nativeˉconsoleˉwriteˉline Value => [Value.Text],
        Nativeˉdiagnosticˉwriteˉline Value => [Value.Text],
        Nativeˉprocessˉargument Value => [Value.Index],
        Nativeˉfileˉreadˉbytes Value => [Value.Resourceˉname],
        Nativeˉfileˉwriteˉbytes Value => [Value.Resourceˉname, Value.Bytes],
        Nativeˉvoidˉcall Value => Value.Arguments,
        _ => throw Invalidˉoperation(),
    };

    private static IEnumerable<int> Uses(Nativeˉterminator terminator) => terminator switch
    {
        Nativeˉjump => [],
        Nativeˉbranch Value => [Value.Condition],
        Nativeˉreturn Value => [Value.Value],
        Nativeˉreturnˉvoid => [],
        _ => throw Invalidˉoperation(),
    };

    private static bool Isˉownershipˉvalue(
        Nativeˉmodule module,
        Nativeˉfunction function,
        int value)
    {
        var Type = Requireˉvalueˉtype(function, value);
        return Isˉdescriptor(Type) ||
            (Type == Nativeˉvalueˉtype.Record && Descriptorˉfields(
                module,
                function.Valueˉnominalˉtypeˉindices[value]).Any());
    }

    private static Nativeˉdescriptorˉcarrier Descriptorˉlocalˉsource(
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉparameters,
        int local) => local < function.Parameterˉtypes.Length &&
        !assignedˉparameters.Contains(local)
            ? Parameterˉcarrier(functionˉindex, local)
            : Local(functionˉindex, local);

    private static Nativeˉdescriptorˉcarrier Recordˉlocalˉsource(
        Nativeˉfunction function,
        int functionˉindex,
        IReadOnlySet<int> assignedˉparameters,
        int local,
        int field) => local < function.Parameterˉtypes.Length &&
        !assignedˉparameters.Contains(local)
            ? Recordˉparameterˉfield(functionˉindex, local, field)
            : Recordˉlocalˉfield(functionˉindex, local, field);

    private static Nativeˉdescriptorˉcarrier Parameterˉcarrier(int function, int parameter) =>
        new(Nativeˉdescriptorˉcarrierˉkind.Parameter, function, parameter, -1);
    private static Nativeˉdescriptorˉcarrier Local(int function, int local) =>
        new(Nativeˉdescriptorˉcarrierˉkind.Local, function, local, -1);
    private static Nativeˉdescriptorˉcarrier Value(int function, int value) =>
        new(Nativeˉdescriptorˉcarrierˉkind.Value, function, value, -1);
    private static Nativeˉdescriptorˉcarrier Recordˉparameterˉfield(
        int function,
        int parameter,
        int field) => new(
            Nativeˉdescriptorˉcarrierˉkind.Recordˉparameterˉfield,
            function,
            parameter,
            field);
    private static Nativeˉdescriptorˉcarrier Recordˉlocalˉfield(
        int function,
        int local,
        int field) => new(
            Nativeˉdescriptorˉcarrierˉkind.Recordˉlocalˉfield,
            function,
            local,
            field);
    private static Nativeˉdescriptorˉcarrier Recordˉvalueˉfield(
        int function,
        int value,
        int field) => new(
            Nativeˉdescriptorˉcarrierˉkind.Recordˉvalueˉfield,
            function,
            value,
            field);
    private static Nativeˉdescriptorˉcarrier Functionˉreturn(int function, int field = -1) =>
        new(Nativeˉdescriptorˉcarrierˉkind.Functionˉreturn, function, -1, field);

    private static IEnumerable<int> Descriptorˉfields(Nativeˉmodule module, int typeˉindex)
    {
        var Record = Requireˉrecord(module, typeˉindex);
        for (var Field = 0; Field < Record.Fields.Length; Field++)
        {
            if (Record.Fields[Field].Type.Kind == Valueˉtype.Record)
            {
                Fail("The native descriptor-ownership planner requires direct non-nested records.");
            }
            if (Isˉdescriptor(Record.Fields[Field].Type.Kind))
            {
                yield return Field;
            }
        }
    }

    private static bool Isˉdescriptor(Nativeˉvalueˉtype type) => type is
        Nativeˉvalueˉtype.Borrowedˉtext or Nativeˉvalueˉtype.Borrowedˉbytes;
    private static bool Isˉdescriptor(Valueˉtype type) => type is
        Valueˉtype.Text or Valueˉtype.Bytes;

    private static Nativeˉvalueˉtype Requireˉvalueˉtype(
        Nativeˉfunction function,
        int value)
    {
        if ((uint)value >= (uint)function.Valueˉtypes.Length)
        {
            Fail("The native descriptor-ownership planner received an invalid value identifier.");
        }
        return function.Valueˉtypes[value];
    }

    private static Recordˉtypeˉdeclaration Requireˉrecord(
        Nativeˉmodule module,
        int typeˉindex)
    {
        if ((uint)typeˉindex >= (uint)module.Types.Length)
        {
            Fail("The native descriptor-ownership planner received an invalid record identity.");
        }
        return module.Types[typeˉindex] as Recordˉtypeˉdeclaration ??
            throw Invalidˉoperation();
    }

    private static void Requireˉfunction(Nativeˉfunction function)
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
            Fail("The native descriptor-ownership planner received inconsistent function metadata.");
        }
    }

    private static Nativeˉbackendˉexception Invalidˉoperation() => new(
        "WVN2903",
        "The native descriptor-ownership planner received an unsupported machine-IR operation.");

    [DoesNotReturn]
    private static void Fail(string message) =>
        throw new Nativeˉbackendˉexception("WVN2903", message);
}

public static class Nativeˉdescriptorˉownershipˉverifier
{
    public static void Verify(
        Nativeˉmodule module,
        Nativeˉdescriptorˉownershipˉplan? plan)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (plan is null ||
            plan.Formatˉversion != Nativeˉdescriptorˉownershipˉplanner.FORMAT_VERSION ||
            !plan.Terminalˉfailureˉdiscardsˉarena ||
            plan.Functions.IsDefault ||
            plan.Functions.Length != module.Functions.Length ||
            plan.Totalˉactions is < 0 or > Nativeˉcontract.MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS)
        {
            Fail("The native descriptor-ownership plan envelope is invalid.");
        }

        // Reconstruct from the machine IR rather than trusting any published summary or action.
        var Oracle = Nativeˉdescriptorˉownershipˉindependentˉoracle.Reconstruct(module);
        if (Oracle.Formatˉversion != plan.Formatˉversion ||
            Oracle.Terminalˉfailureˉdiscardsˉarena != plan.Terminalˉfailureˉdiscardsˉarena ||
            Oracle.Totalˉactions != plan.Totalˉactions ||
            Oracle.Functions.Length != plan.Functions.Length ||
            !Oracle.Functions.Zip(plan.Functions).All(Pair => Functionˉequals(
                Pair.First,
                Pair.Second)))
        {
            Fail("The native descriptor-ownership plan does not match independent reconstruction.");
        }
    }

    private static bool Functionˉequals(
        Nativeˉfunctionˉdescriptorˉownership left,
        Nativeˉfunctionˉdescriptorˉownership right) =>
        left.Functionˉindex == right.Functionˉindex &&
        StringComparer.Ordinal.Equals(left.Functionˉname, right.Functionˉname) &&
        left.Descriptorˉparameterˉbindings == right.Descriptorˉparameterˉbindings &&
        left.Assignedˉdescriptorˉparameterˉbindings == right.Assignedˉdescriptorˉparameterˉbindings &&
        left.Descriptorˉlocalˉbindings == right.Descriptorˉlocalˉbindings &&
        left.Recordˉparameterˉdescriptorˉfields == right.Recordˉparameterˉdescriptorˉfields &&
        left.Assignedˉrecordˉparameterˉdescriptorˉfields == right.Assignedˉrecordˉparameterˉdescriptorˉfields &&
        left.Recordˉlocalˉdescriptorˉfields == right.Recordˉlocalˉdescriptorˉfields &&
        left.Descriptorˉvalueˉidentifiers == right.Descriptorˉvalueˉidentifiers &&
        left.Recordˉvalueˉdescriptorˉfields == right.Recordˉvalueˉdescriptorˉfields &&
        left.Acquireˉactions == right.Acquireˉactions &&
        left.Borrowˉactions == right.Borrowˉactions &&
        left.Retainˉactions == right.Retainˉactions &&
        left.Releaseˉactions == right.Releaseˉactions &&
        left.Callˉborrowˉactions == right.Callˉborrowˉactions &&
        left.Acceptedˉreturnˉactions == right.Acceptedˉreturnˉactions &&
        left.Transferredˉreturnˉactions == right.Transferredˉreturnˉactions &&
        left.Actions.SequenceEqual(right.Actions);

    [DoesNotReturn]
    private static void Fail(string message) =>
        throw new Nativeˉbackendˉexception("WVN2903", message);

}
