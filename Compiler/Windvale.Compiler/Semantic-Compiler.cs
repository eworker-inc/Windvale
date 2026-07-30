using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

internal static class Semanticˉcompiler
{
    public static Wirˉmodule Compile(Moduleˉsyntax syntax, Diagnosticˉbag diagnostics)
    {
        var Context = new Moduleˉcontext(syntax, diagnostics);
        return Context.Compile();
    }

    private sealed class Moduleˉcontext(Moduleˉsyntax syntax, Diagnosticˉbag diagnostics)
    {
        private readonly Dictionary<string, Capabilityˉdeclaration> Capabilities =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Dataˉdeclaration> Data =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Functionˉsymbol> Functions =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Recordˉsymbol> Records =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> Textˉdataˉbyˉvalue =
            new(StringComparer.Ordinal);
        private int Syntheticˉtextˉcounter;

        public Wirˉmodule Compile()
        {
            Bindˉmoduleˉname();
            var Profile = Bindˉprofile(syntax.Profile);
            Bindˉcapabilities(Profile);
            Bindˉdata();
            Bindˉrecords();
            Bindˉfunctionˉsignatures();

            var Wirˉfunctions = ImmutableArray.CreateBuilder<Wirˉfunction>(Functions.Count);
            foreach (var Function in Functions.Values.OrderBy(Function => Function.Name, StringComparer.Ordinal))
            {
                var Builder = new Functionˉbuilder(
                    Function,
                    diagnostics,
                    Data,
                    Functions,
                    Capabilities,
                    Records,
                    Getˉorˉaddˉtextˉdata);
                Wirˉfunctions.Add(Builder.Compile());
            }

            return new(
                syntax.Name.Text,
                Profile,
                [.. Capabilities.Values.OrderBy(Capability => Capability.Name, StringComparer.Ordinal)],
                [.. Data.Values.OrderBy(Item => Item.Name, StringComparer.Ordinal)],
                [.. Records.Values.OrderBy(Record => Record.Index).Select(Record => Record.Declaration)],
                Wirˉfunctions.ToImmutable());
        }

        private void Bindˉrecords()
        {
            var Seenˉnames = new HashSet<string>(StringComparer.Ordinal);
            var Valid = new List<Recordˉsyntax>();
            foreach (var Record in syntax.Records)
            {
                if (!Seedˉnames.Isˉidentifier(Record.Name.Text))
                {
                    Report("WVC2080", Record.Name.Span, $"Record name '{Record.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (Record.Name.Text == "length" ||
                    Foundationˉintrinsics.Tryˉget(Record.Name.Text, out _))
                {
                    Report("WVC2090", Record.Name.Span, $"Record name '{Record.Name.Text}' is reserved by Windvale Seed.");
                    continue;
                }

                if (!Seenˉnames.Add(Record.Name.Text))
                {
                    Report("WVC2081", Record.Name.Span, $"Record '{Record.Name.Text}' is declared more than once.");
                    continue;
                }

                Valid.Add(Record);
            }

            foreach (var Record in Valid.OrderBy(Item => Item.Name.Text, StringComparer.Ordinal))
            {
                if (Records.Count >= Bytecodeˉlimits.MAX_RECORD_TYPES)
                {
                    Report("WVC2088", Record.Span, "The module exceeds the Seed record-type limit.");
                    break;
                }

                var Fieldˉnames = new HashSet<string>(StringComparer.Ordinal);
                var Fields = ImmutableArray.CreateBuilder<Recordˉfieldˉdeclaration>(Record.Fields.Length);
                foreach (var Field in Record.Fields)
                {
                    if (Fields.Count >= Bytecodeˉlimits.MAX_RECORD_FIELDS)
                    {
                        Report("WVC2089", Field.Span, $"Record '{Record.Name.Text}' exceeds the Seed field limit.");
                        break;
                    }

                    if (!Seedˉnames.Isˉidentifier(Field.Name.Text) || !Fieldˉnames.Add(Field.Name.Text))
                    {
                        Report("WVC2082", Field.Name.Span, $"Record '{Record.Name.Text}' has an invalid or duplicate field '{Field.Name.Text}'.");
                    }

                    if (Field.Type.Kind is Typeˉsyntaxˉkind.Void or Typeˉsyntaxˉkind.Record or Typeˉsyntaxˉkind.Invalid)
                    {
                        Report("WVC2083", Field.Type.Span, "Seed record fields must use a primitive value type.");
                        Fields.Add(new(Field.Name.Text, Valueˉtype.I32));
                        continue;
                    }

                    Fields.Add(new(Field.Name.Text, Bindˉprimitiveˉtype(Field.Type)));
                }

                if (Fields.Count == 0)
                {
                    Report("WVC2084", Record.Span, $"Record '{Record.Name.Text}' must declare at least one field.");
                    Fields.Add(new("Invalid", Valueˉtype.I32));
                }

                var Index = Records.Count;
                var Declaration = new Recordˉtypeˉdeclaration(Record.Name.Text, Fields.ToImmutable());
                Records.Add(Record.Name.Text, new(Record.Name.Text, Index, Declaration));
            }
        }

        private void Bindˉmoduleˉname()
        {
            if (!Seedˉnames.Isˉidentifier(syntax.Name.Text))
            {
                Report(
                    "WVC2004",
                    syntax.Name.Span,
                    $"Module name '{syntax.Name.Text}' is not a valid Windvale identifier.");
            }
        }

        private Moduleˉprofile Bindˉprofile(Syntaxˉtoken profile)
        {
            return profile.Kind switch
            {
                Tokenˉkind.Portable => Moduleˉprofile.Portable,
                Tokenˉkind.Hosted => Moduleˉprofile.Hosted,
                Tokenˉkind.System => Moduleˉprofile.System,
                _ => Moduleˉprofile.Portable,
            };
        }

        private void Bindˉcapabilities(Moduleˉprofile profile)
        {
            foreach (var Capabilityˉsyntax in syntax.Capabilities)
            {
                if (Capabilities.ContainsKey(Capabilityˉsyntax.Name))
                {
                    Report(
                        "WVC2000",
                        Capabilityˉsyntax.Span,
                        $"Capability '{Capabilityˉsyntax.Name}' is declared more than once.");
                    continue;
                }

                if (!Seedˉnames.Isˉcapability(Capabilityˉsyntax.Name))
                {
                    Report(
                        "WVC2001",
                        Capabilityˉsyntax.Span,
                        $"Capability name '{Capabilityˉsyntax.Name}' is invalid.");
                    continue;
                }

                if (!Capabilityˉcatalog.Tryˉget(Capabilityˉsyntax.Name, out var Declaration))
                {
                    Report(
                        "WVC2002",
                        Capabilityˉsyntax.Span,
                        $"Capability '{Capabilityˉsyntax.Name}' is not defined by Windvale Seed.");
                    continue;
                }

                if (profile == Moduleˉprofile.Portable)
                {
                    Report(
                        "WVC2003",
                        Capabilityˉsyntax.Span,
                        "A portable module cannot declare hosted capabilities.");
                }

                Capabilities.Add(Capabilityˉsyntax.Name, Declaration);
            }
        }

        private void Bindˉdata()
        {
            foreach (var Dataˉsyntax in syntax.Data)
            {
                if (!Seedˉnames.Isˉidentifier(Dataˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2012",
                        Dataˉsyntax.Name.Span,
                        $"Data name '{Dataˉsyntax.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (Data.ContainsKey(Dataˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2010",
                        Dataˉsyntax.Name.Span,
                        $"Data '{Dataˉsyntax.Name.Text}' is declared more than once.");
                    continue;
                }

                Dataˉdeclaration? Declaration = (Dataˉsyntax.Type.Kind, Dataˉsyntax.Value) switch
                {
                    (Typeˉsyntaxˉkind.Text, Textˉdataˉvalueˉsyntax Textˉvalue) =>
                        new Textˉdataˉdeclaration(Dataˉsyntax.Name.Text, Textˉvalue.Value),
                    (Typeˉsyntaxˉkind.I32ˉarray, I32ˉarrayˉdataˉvalueˉsyntax Array) =>
                        new I32ˉarrayˉdataˉdeclaration(Dataˉsyntax.Name.Text, Array.Values),
                    (Typeˉsyntaxˉkind.Bytes, Bytesˉdataˉvalueˉsyntax Bytes) =>
                        new Bytesˉdataˉdeclaration(Dataˉsyntax.Name.Text, Bytes.Values),
                    _ => null,
                };

                if (Declaration is null)
                {
                    Report(
                        "WVC2011",
                        Dataˉsyntax.Span,
                        $"Data '{Dataˉsyntax.Name.Text}' has an incompatible initializer.");
                    continue;
                }

                Data.Add(Declaration.Name, Declaration);
                if (Declaration is Textˉdataˉdeclaration Textˉdeclaration &&
                    !Textˉdataˉbyˉvalue.ContainsKey(Textˉdeclaration.Value))
                {
                    Textˉdataˉbyˉvalue.Add(Textˉdeclaration.Value, Textˉdeclaration.Name);
                }
            }
        }

        private void Bindˉfunctionˉsignatures()
        {
            foreach (var Functionˉsyntax in syntax.Functions)
            {
                if (Functionˉsyntax.Name.Text == "length" ||
                    Foundationˉintrinsics.Tryˉget(Functionˉsyntax.Name.Text, out _))
                {
                    Report(
                        "WVC2024",
                        Functionˉsyntax.Name.Span,
                        $"Function name '{Functionˉsyntax.Name.Text}' is reserved by Windvale Seed.");
                    continue;
                }

                if (!Seedˉnames.Isˉidentifier(Functionˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2022",
                        Functionˉsyntax.Name.Span,
                        $"Function name '{Functionˉsyntax.Name.Text}' is not a valid Windvale identifier.");
                    continue;
                }

                if (Functions.ContainsKey(Functionˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2020",
                        Functionˉsyntax.Name.Span,
                        $"Function '{Functionˉsyntax.Name.Text}' is declared more than once.");
                    continue;
                }

                if (Records.ContainsKey(Functionˉsyntax.Name.Text))
                {
                    Report(
                        "WVC2025",
                        Functionˉsyntax.Name.Span,
                        $"Function name '{Functionˉsyntax.Name.Text}' conflicts with a record constructor.");
                    continue;
                }

                var Parameterˉnames = new HashSet<string>(StringComparer.Ordinal);
                var Parameters = ImmutableArray.CreateBuilder<Parameterˉsymbol>(Functionˉsyntax.Parameters.Length);
                for (var Index = 0; Index < Functionˉsyntax.Parameters.Length; Index++)
                {
                    var Parameter = Functionˉsyntax.Parameters[Index];
                    if (!Seedˉnames.Isˉidentifier(Parameter.Name.Text))
                    {
                        Report(
                            "WVC2023",
                            Parameter.Name.Span,
                            $"Parameter name '{Parameter.Name.Text}' is not a valid Windvale identifier.");
                    }

                    if (!Parameterˉnames.Add(Parameter.Name.Text))
                    {
                        Report(
                            "WVC2021",
                            Parameter.Name.Span,
                            $"Parameter '{Parameter.Name.Text}' is declared more than once.");
                    }

                    Parameters.Add(new(
                        Parameter.Name.Text,
                        Bindˉvalueˉshape(Parameter.Type),
                        Index,
                        Parameter.Name.Span));
                }

                Functions.Add(
                    Functionˉsyntax.Name.Text,
                    new(
                        Functionˉsyntax.Name.Text,
                        Parameters.ToImmutable(),
                        Bindˉvalueˉshape(Functionˉsyntax.Returnˉtype),
                        Functionˉsyntax.Isˉexported,
                        Functionˉsyntax));
            }
        }

        private string Getˉorˉaddˉtextˉdata(string value)
        {
            if (Textˉdataˉbyˉvalue.TryGetValue(value, out var Existing))
            {
                return Existing;
            }

            string Name;
            do
            {
                Name = $"__Text_{Syntheticˉtextˉcounter++:D6}";
            }
            while (Data.ContainsKey(Name));

            Data.Add(Name, new Textˉdataˉdeclaration(Name, value));
            Textˉdataˉbyˉvalue.Add(value, Name);
            return Name;
        }

        private Valueˉshape Bindˉvalueˉshape(Typeˉsyntax type)
        {
            if (type.Kind == Typeˉsyntaxˉkind.Record)
            {
                if (type.Name is not null && Records.TryGetValue(type.Name, out var Record))
                {
                    return Valueˉshape.Forˉrecord(Record.Index);
                }

                Report("WVC2085", type.Span, $"Record type '{type.Name}' is not declared.");
                return Valueˉtype.I32;
            }

            return Bindˉprimitiveˉtype(type);
        }

        private static Valueˉtype Bindˉprimitiveˉtype(Typeˉsyntax type)
        {
            return type.Kind switch
            {
                Typeˉsyntaxˉkind.Void => Valueˉtype.Void,
                Typeˉsyntaxˉkind.I32 => Valueˉtype.I32,
                Typeˉsyntaxˉkind.U8 => Valueˉtype.U8,
                Typeˉsyntaxˉkind.U32 => Valueˉtype.U32,
                Typeˉsyntaxˉkind.Bool => Valueˉtype.Bool,
                Typeˉsyntaxˉkind.Text => Valueˉtype.Text,
                Typeˉsyntaxˉkind.Bytes => Valueˉtype.Bytes,
                _ => Valueˉtype.I32,
            };
        }

        private void Report(string code, Sourceˉspan span, string message)
        {
            diagnostics.Report(code, "semantic", span, message);
        }
    }

    private sealed record Parameterˉsymbol(
        string Name,
        Valueˉshape Type,
        int Slot,
        Sourceˉspan Span);

    private sealed record Functionˉsymbol(
        string Name,
        ImmutableArray<Parameterˉsymbol> Parameters,
        Valueˉshape Returnˉtype,
        bool Isˉexported,
        Functionˉsyntax Syntax)
    {
        public ImmutableArray<Valueˉshape> Parameterˉtypes => [.. Parameters.Select(Parameter => Parameter.Type)];
    }

    private sealed record Recordˉsymbol(
        string Name,
        int Index,
        Recordˉtypeˉdeclaration Declaration);

    private sealed record Localˉsymbol(string Name, Valueˉshape Type, int Slot, bool Isˉmutable);

    private readonly record struct Boundˉvalue(Valueˉshape Type, int Temporary)
    {
        public static Boundˉvalue Void => new(Valueˉtype.Void, -1);
    }

    private sealed class Mutableˉblock(int id)
    {
        public int Id { get; } = id;

        public List<Wirˉinstruction> Instructions { get; } = [];

        public Wirˉterminator? Terminator { get; set; }
    }

    private sealed class Functionˉbuilder
    {
        private readonly Functionˉsymbol Function;
        private readonly Diagnosticˉbag Diagnostics;
        private readonly IReadOnlyDictionary<string, Dataˉdeclaration> Data;
        private readonly IReadOnlyDictionary<string, Functionˉsymbol> Functions;
        private readonly IReadOnlyDictionary<string, Capabilityˉdeclaration> Capabilities;
        private readonly IReadOnlyDictionary<string, Recordˉsymbol> Records;
        private readonly Func<string, string> Getˉtextˉdata;
        private readonly List<Mutableˉblock> Blocks = [];
        private readonly List<Valueˉshape> Userˉlocalˉtypes = [];
        private readonly List<Valueˉshape> Temporaryˉtypes = [];
        private readonly Stack<Dictionary<string, Localˉsymbol>> Scopes = [];
        private readonly HashSet<string> Allˉlocalˉnames = new(StringComparer.Ordinal);
        private Mutableˉblock? Currentˉblock;

        public Functionˉbuilder(
            Functionˉsymbol function,
            Diagnosticˉbag diagnostics,
            IReadOnlyDictionary<string, Dataˉdeclaration> data,
            IReadOnlyDictionary<string, Functionˉsymbol> functions,
            IReadOnlyDictionary<string, Capabilityˉdeclaration> capabilities,
            IReadOnlyDictionary<string, Recordˉsymbol> records,
            Func<string, string> getˉtextˉdata)
        {
            Function = function;
            Diagnostics = diagnostics;
            Data = data;
            Functions = functions;
            Capabilities = capabilities;
            Records = records;
            Getˉtextˉdata = getˉtextˉdata;
        }

        public Wirˉfunction Compile()
        {
            Enterˉscope();
            foreach (var Parameter in Function.Parameters)
            {
                if (Allˉlocalˉnames.Add(Parameter.Name))
                {
                    Scopes.Peek().Add(Parameter.Name, new(Parameter.Name, Parameter.Type, Parameter.Slot, false));
                }
            }

            Currentˉblock = Createˉblock();
            Compileˉblock(Function.Syntax.Body, createˉscope: false);
            if (Currentˉblock is not null)
            {
                if (Function.Returnˉtype == Valueˉtype.Void)
                {
                    Currentˉblock.Terminator = new Wirˉreturn(null);
                    Currentˉblock = null;
                }
                else
                {
                    Report(
                        "WVC2030",
                        Function.Syntax.Body.Span,
                        $"Function '{Function.Name}' can reach the end without returning {Formatˉtype(Function.Returnˉtype)}.");
                    var Fallbackˉblock = Currentˉblock;
                    var Temporary = Emitˉresult(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
                    Fallbackˉblock.Terminator = new Wirˉreturn(Temporary);
                    Currentˉblock = null;
                }
            }

            Exitˉscope();
            var Frozenˉblocks = Blocks.Select(Block => new Wirˉblock(
                Block.Id,
                [.. Block.Instructions],
                Block.Terminator ?? throw new InvalidOperationException(
                    $"WIR block {Block.Id} in function '{Function.Name}' has no terminator.")));

            return new(
                Function.Name,
                Function.Parameterˉtypes,
                Function.Returnˉtype,
                [.. Userˉlocalˉtypes],
                [.. Temporaryˉtypes],
                [.. Frozenˉblocks],
                Function.Isˉexported);
        }

        private void Compileˉblock(Blockˉstatementˉsyntax block, bool createˉscope = true)
        {
            if (createˉscope)
            {
                Enterˉscope();
            }

            foreach (var Statement in block.Statements)
            {
                if (Currentˉblock is null)
                {
                    Report("WVC2031", Statement.Span, "This statement is unreachable.");
                    continue;
                }

                Compileˉstatement(Statement);
            }

            if (createˉscope)
            {
                Exitˉscope();
            }
        }

        private void Compileˉstatement(Statementˉsyntax statement)
        {
            switch (statement)
            {
                case Blockˉstatementˉsyntax Block:
                    Compileˉblock(Block);
                    break;
                case Localˉdeclarationˉstatementˉsyntax Localˉdeclaration:
                    Compileˉlocalˉdeclaration(Localˉdeclaration);
                    break;
                case Assignmentˉstatementˉsyntax Assignment:
                    Compileˉassignment(Assignment);
                    break;
                case Expressionˉstatementˉsyntax Expression:
                    _ = Compileˉexpression(Expression.Expression);
                    break;
                case Ifˉstatementˉsyntax If:
                    Compileˉif(If);
                    break;
                case Whileˉstatementˉsyntax While:
                    Compileˉwhile(While);
                    break;
                case Returnˉstatementˉsyntax Return:
                    Compileˉreturn(Return);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown statement syntax '{statement.GetType().Name}'.");
            }
        }

        private void Compileˉlocalˉdeclaration(Localˉdeclarationˉstatementˉsyntax statement)
        {
            var Type = Bindˉvalueˉshape(statement.Type);
            var Initializer = Compileˉexpression(statement.Initializer);
            Requireˉtype(Initializer, Type, statement.Initializer.Span, "local initializer");

            if (!Seedˉnames.Isˉidentifier(statement.Name.Text))
            {
                Report(
                    "WVC2043",
                    statement.Name.Span,
                    $"Local name '{statement.Name.Text}' is not a valid Windvale identifier.");
                return;
            }

            if (!Allˉlocalˉnames.Add(statement.Name.Text))
            {
                Report(
                    "WVC2040",
                    statement.Name.Span,
                    $"Local or parameter '{statement.Name.Text}' is already declared in this function.");
                return;
            }

            var Slot = Function.Parameters.Length + Userˉlocalˉtypes.Count;
            Userˉlocalˉtypes.Add(Type);
            Scopes.Peek().Add(statement.Name.Text, new(statement.Name.Text, Type, Slot, statement.Isˉmutable));
            Emit(
                new(
                    Wirˉoperation.Storeˉlocal,
                    null,
                    [Initializer.Temporary],
                    Integerˉoperand: Slot));
        }

        private void Compileˉassignment(Assignmentˉstatementˉsyntax statement)
        {
            if (!Tryˉlookupˉlocal(statement.Name.Text, out var Local))
            {
                Report(
                    "WVC2041",
                    statement.Name.Span,
                    $"Local or parameter '{statement.Name.Text}' is not declared in this scope.");
                _ = Compileˉexpression(statement.Value);
                return;
            }

            if (!Local.Isˉmutable)
            {
                Report(
                    "WVC2042",
                    statement.Name.Span,
                    $"Local or parameter '{statement.Name.Text}' is immutable; copy it to a 'var' local before assigning.");
                _ = Compileˉexpression(statement.Value);
                return;
            }

            var Value = Compileˉexpression(statement.Value);
            Requireˉtype(Value, Local.Type, statement.Value.Span, "assignment");
            Emit(new(
                Wirˉoperation.Storeˉlocal,
                null,
                [Value.Temporary],
                Integerˉoperand: Local.Slot));
        }

        private void Compileˉif(Ifˉstatementˉsyntax statement)
        {
            var Condition = Compileˉexpression(statement.Condition);
            Requireˉtype(Condition, Valueˉtype.Bool, statement.Condition.Span, "if condition");
            var Branchˉsource = Requireˉcurrentˉblock();
            var Thenˉblock = Createˉblock();
            var Elseˉblock = Createˉblock();
            Branchˉsource.Terminator = new Wirˉbranch(Condition.Temporary, Thenˉblock.Id, Elseˉblock.Id);

            Currentˉblock = Thenˉblock;
            Compileˉblock(statement.Then);
            var Thenˉend = Currentˉblock;

            Currentˉblock = Elseˉblock;
            if (statement.Else is not null)
            {
                Compileˉblock(statement.Else);
            }

            var Elseˉend = Currentˉblock;
            if (Thenˉend is null && Elseˉend is null)
            {
                Currentˉblock = null;
                return;
            }

            var Joinˉblock = Createˉblock();
            if (Thenˉend is not null)
            {
                Thenˉend.Terminator = new Wirˉjump(Joinˉblock.Id);
            }

            if (Elseˉend is not null)
            {
                Elseˉend.Terminator = new Wirˉjump(Joinˉblock.Id);
            }

            Currentˉblock = Joinˉblock;
        }

        private void Compileˉwhile(Whileˉstatementˉsyntax statement)
        {
            var Entry = Requireˉcurrentˉblock();
            var Header = Createˉblock();
            var Body = Createˉblock();
            var After = Createˉblock();
            Entry.Terminator = new Wirˉjump(Header.Id);

            Currentˉblock = Header;
            var Condition = Compileˉexpression(statement.Condition);
            Requireˉtype(Condition, Valueˉtype.Bool, statement.Condition.Span, "while condition");
            Header.Terminator = new Wirˉbranch(Condition.Temporary, Body.Id, After.Id);

            Currentˉblock = Body;
            Compileˉblock(statement.Body);
            if (Currentˉblock is not null)
            {
                Currentˉblock.Terminator = new Wirˉjump(Header.Id);
            }

            Currentˉblock = After;
        }

        private void Compileˉreturn(Returnˉstatementˉsyntax statement)
        {
            var Block = Requireˉcurrentˉblock();
            if (Function.Returnˉtype == Valueˉtype.Void)
            {
                if (statement.Value is not null)
                {
                    Report("WVC2050", statement.Value.Span, "A void function cannot return a value.");
                    _ = Compileˉexpression(statement.Value);
                }

                Block.Terminator = new Wirˉreturn(null);
            }
            else if (statement.Value is null)
            {
                Report(
                    "WVC2051",
                    statement.Span,
                    $"Function '{Function.Name}' must return {Formatˉtype(Function.Returnˉtype)}.");
                var Fallback = Emitˉresult(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
                Block.Terminator = new Wirˉreturn(Fallback);
            }
            else
            {
                var Value = Compileˉexpression(statement.Value);
                Requireˉtype(Value, Function.Returnˉtype, statement.Value.Span, "return value");
                Block.Terminator = new Wirˉreturn(Value.Temporary);
            }

            Currentˉblock = null;
        }

        private Boundˉvalue Compileˉexpression(Expressionˉsyntax expression)
        {
            return expression switch
            {
                Literalˉexpressionˉsyntax Literal => Compileˉliteral(Literal),
                Nameˉexpressionˉsyntax Name => Compileˉname(Name),
                Unaryˉexpressionˉsyntax Unary => Compileˉunary(Unary),
                Binaryˉexpressionˉsyntax Binary => Compileˉbinary(Binary),
                Callˉexpressionˉsyntax Call => Compileˉcall(Call),
                Indexˉexpressionˉsyntax Index => Compileˉindex(Index),
                Fieldˉexpressionˉsyntax Field => Compileˉfield(Field),
                Invalidˉexpressionˉsyntax Invalid => Invalidˉvalue(Invalid.Span),
                _ => throw new InvalidOperationException($"Unknown expression syntax '{expression.GetType().Name}'."),
            };
        }

        private Boundˉvalue Compileˉliteral(Literalˉexpressionˉsyntax expression)
        {
            return expression.Value switch
            {
                int Integer => Result(
                    Wirˉoperation.I32ˉconstant,
                    Valueˉtype.I32,
                    integerˉoperand: Integer),
                byte U8 => Result(
                    Wirˉoperation.U8ˉconstant,
                    Valueˉtype.U8,
                    unsignedˉintegerˉoperand: U8),
                uint U32 => Result(
                    Wirˉoperation.U32ˉconstant,
                    Valueˉtype.U32,
                    unsignedˉintegerˉoperand: U32),
                bool Boolean => Result(
                    Wirˉoperation.Boolˉconstant,
                    Valueˉtype.Bool,
                    integerˉoperand: Boolean ? 1 : 0),
                string Text => Result(
                    Wirˉoperation.Textˉconstant,
                    Valueˉtype.Text,
                    nameˉoperand: Getˉtextˉdata(Text)),
                _ => Invalidˉvalue(expression.Span),
            };
        }

        private Boundˉvalue Compileˉname(Nameˉexpressionˉsyntax expression)
        {
            if (Tryˉlookupˉlocal(expression.Name, out var Local))
            {
                return Result(
                    Wirˉoperation.Loadˉlocal,
                    Local.Type,
                    integerˉoperand: Local.Slot);
            }

            if (Data.TryGetValue(expression.Name, out var Declaration))
            {
                if (Declaration is Textˉdataˉdeclaration)
                {
                    return Result(
                        Wirˉoperation.Textˉconstant,
                        Valueˉtype.Text,
                        nameˉoperand: Declaration.Name);
                }

                if (Declaration is Bytesˉdataˉdeclaration)
                {
                    return Result(
                        Wirˉoperation.Bytesˉconstant,
                        Valueˉtype.Bytes,
                        nameˉoperand: Declaration.Name);
                }

                Report(
                    "WVC2060",
                    expression.Span,
                    $"Array data '{expression.Name}' must be indexed or passed to length().");
                return Invalidˉvalue(expression.Span);
            }

            Report("WVC2061", expression.Span, $"Name '{expression.Name}' is not declared.");
            return Invalidˉvalue(expression.Span);
        }

        private Boundˉvalue Compileˉunary(Unaryˉexpressionˉsyntax expression)
        {
            var Operand = Compileˉexpression(expression.Operand);
            if (expression.Operator == Tokenˉkind.Minus)
            {
                Requireˉtype(Operand, Valueˉtype.I32, expression.Operand.Span, "unary '-' operand");
                return Result(Wirˉoperation.I32ˉnegate, Valueˉtype.I32, [Operand.Temporary]);
            }

            Requireˉtype(Operand, Valueˉtype.Bool, expression.Operand.Span, "unary '!' operand");
            return Result(Wirˉoperation.Boolˉnot, Valueˉtype.Bool, [Operand.Temporary]);
        }

        private Boundˉvalue Compileˉbinary(Binaryˉexpressionˉsyntax expression)
        {
            var Left = Compileˉexpression(expression.Left);
            var Right = Compileˉexpression(expression.Right);
            var Operands = ImmutableArray.Create(Left.Temporary, Right.Temporary);

            switch (expression.Operator)
            {
                case Tokenˉkind.Plus:
                case Tokenˉkind.Minus:
                case Tokenˉkind.Star:
                    if (Left.Type != Right.Type || Left.Type.Kind is not (Valueˉtype.I32 or Valueˉtype.U32))
                    {
                        Report(
                            "WVC2068",
                            expression.Span,
                            "Arithmetic requires two i32 values or two u32 values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }

                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Plus) => Wirˉoperation.I32ˉadd,
                            (Valueˉtype.I32, Tokenˉkind.Minus) => Wirˉoperation.I32ˉsubtract,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉmultiply,
                            (Valueˉtype.U32, Tokenˉkind.Plus) => Wirˉoperation.U32ˉadd,
                            (Valueˉtype.U32, Tokenˉkind.Minus) => Wirˉoperation.U32ˉsubtract,
                            _ => Wirˉoperation.U32ˉmultiply,
                        },
                        Left.Type,
                        Operands);
                case Tokenˉkind.Less:
                case Tokenˉkind.Lessˉequals:
                case Tokenˉkind.Greater:
                case Tokenˉkind.Greaterˉequals:
                    if (Left.Type != Right.Type || Left.Type.Kind is not (Valueˉtype.I32 or Valueˉtype.U32))
                    {
                        Report(
                            "WVC2069",
                            expression.Span,
                            "Ordering requires two i32 values or two u32 values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }

                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Less) => Wirˉoperation.I32ˉless,
                            (Valueˉtype.I32, Tokenˉkind.Lessˉequals) => Wirˉoperation.I32ˉlessˉequal,
                            (Valueˉtype.I32, Tokenˉkind.Greater) => Wirˉoperation.I32ˉgreater,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉgreaterˉequal,
                            (Valueˉtype.U32, Tokenˉkind.Less) => Wirˉoperation.U32ˉless,
                            (Valueˉtype.U32, Tokenˉkind.Lessˉequals) => Wirˉoperation.U32ˉlessˉequal,
                            (Valueˉtype.U32, Tokenˉkind.Greater) => Wirˉoperation.U32ˉgreater,
                            _ => Wirˉoperation.U32ˉgreaterˉequal,
                        },
                        Valueˉtype.Bool,
                        Operands);
                case Tokenˉkind.Equalsˉequals:
                case Tokenˉkind.Bangˉequals:
                    if (Left.Type != Right.Type ||
                        Left.Type.Kind is not (Valueˉtype.I32 or Valueˉtype.U8 or Valueˉtype.U32 or Valueˉtype.Bool))
                    {
                        Report(
                            "WVC2062",
                            expression.Span,
                            "Equality requires two i32, u8, u32, or bool values of the same type.");
                        return Invalidˉvalue(expression.Span);
                    }

                    return Result(
                        (Left.Type.Kind, expression.Operator) switch
                        {
                            (Valueˉtype.I32, Tokenˉkind.Equalsˉequals) => Wirˉoperation.I32ˉequal,
                            (Valueˉtype.I32, _) => Wirˉoperation.I32ˉnotˉequal,
                            (Valueˉtype.U8, Tokenˉkind.Equalsˉequals) => Wirˉoperation.U8ˉequal,
                            (Valueˉtype.U8, _) => Wirˉoperation.U8ˉnotˉequal,
                            (Valueˉtype.U32, Tokenˉkind.Equalsˉequals) => Wirˉoperation.U32ˉequal,
                            (Valueˉtype.U32, _) => Wirˉoperation.U32ˉnotˉequal,
                            (Valueˉtype.Bool, Tokenˉkind.Equalsˉequals) => Wirˉoperation.Boolˉequal,
                            _ => Wirˉoperation.Boolˉnotˉequal,
                        },
                        Valueˉtype.Bool,
                        Operands);
                default:
                    throw new InvalidOperationException($"Unknown binary operator '{expression.Operator}'.");
            }
        }

        private Boundˉvalue Compileˉindex(Indexˉexpressionˉsyntax expression)
        {
            var Index = Compileˉexpression(expression.Index);
            Requireˉtype(Index, Valueˉtype.I32, expression.Index.Span, "data index");
            if (!Data.TryGetValue(expression.Name, out var Declaration) ||
                Declaration is not I32ˉarrayˉdataˉdeclaration)
            {
                Report(
                    "WVC2063",
                    expression.Span,
                    $"'{expression.Name}' is not immutable [i32] data.");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Dataˉloadˉi32,
                Valueˉtype.I32,
                [Index.Temporary],
                nameˉoperand: expression.Name);
        }

        private Boundˉvalue Compileˉcall(Callˉexpressionˉsyntax expression)
        {
            if (expression.Name == "length")
            {
                return Compileˉlength(expression);
            }

            if (Foundationˉintrinsics.Tryˉget(expression.Name, out var Intrinsic))
            {
                return Compileˉfoundationˉintrinsic(expression, Intrinsic);
            }

            if (Records.TryGetValue(expression.Name, out var Record))
            {
                var Recordˉarguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
                Checkˉarguments(
                    expression,
                    Recordˉarguments,
                    [.. Record.Declaration.Fields.Select(Field => (Valueˉshape)Field.Type)]);
                return Result(
                    Wirˉoperation.Recordˉcreate,
                    Valueˉshape.Forˉrecord(Record.Index),
                    Recordˉarguments.Select(Argument => Argument.Temporary).ToImmutableArray(),
                    unsignedˉintegerˉoperand: (uint)Record.Index);
            }

            var Arguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
            if (Functions.TryGetValue(expression.Name, out var Calledˉfunction))
            {
                Checkˉarguments(expression, Arguments, Calledˉfunction.Parameterˉtypes);
                return Callˉresult(
                    Wirˉoperation.Callˉfunction,
                    Calledˉfunction.Returnˉtype,
                    Arguments,
                    expression.Name);
            }

            if (Capabilities.TryGetValue(expression.Name, out var Capability))
            {
                Checkˉarguments(expression, Arguments, Capability.Parameterˉtypes);
                return Callˉresult(
                    Wirˉoperation.Callˉcapability,
                    Capability.Returnˉtype,
                    Arguments,
                    expression.Name);
            }

            if (Capabilityˉcatalog.Tryˉget(expression.Name, out _))
            {
                Report(
                    "WVC2064",
                    expression.Span,
                    $"Capability '{expression.Name}' must be declared by the module before it is called.");
            }
            else
            {
                Report("WVC2065", expression.Span, $"Function or capability '{expression.Name}' is not declared.");
            }

            return Invalidˉvalue(expression.Span);
        }

        private Boundˉvalue Compileˉfield(Fieldˉexpressionˉsyntax expression)
        {
            var Target = Compileˉname(new(expression.Target, expression.Span));
            if (Target.Type.Kind != Valueˉtype.Record)
            {
                Report("WVC2086", expression.Span, $"'{expression.Target}' is not a record value.");
                return Invalidˉvalue(expression.Span);
            }

            var Record = Records.Values.Single(Item => Item.Index == Target.Type.Recordˉtypeˉindex);
            var Fieldˉindex = -1;
            for (var Index = 0; Index < Record.Declaration.Fields.Length; Index++)
            {
                if (StringComparer.Ordinal.Equals(Record.Declaration.Fields[Index].Name, expression.Field))
                {
                    Fieldˉindex = Index;
                    break;
                }
            }

            if (Fieldˉindex < 0)
            {
                Report("WVC2087", expression.Span, $"Record '{Record.Name}' has no field '{expression.Field}'.");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Recordˉfield,
                Record.Declaration.Fields[Fieldˉindex].Type,
                [Target.Temporary],
                unsignedˉintegerˉoperand: (uint)Fieldˉindex);
        }

        private Boundˉvalue Compileˉfoundationˉintrinsic(
            Callˉexpressionˉsyntax expression,
            Foundationˉintrinsicˉdeclaration intrinsic)
        {
            var Arguments = expression.Arguments.Select(Compileˉexpression).ToImmutableArray();
            Checkˉarguments(expression, Arguments, intrinsic.Parameterˉtypes);
            return Result(
                intrinsic.Operation,
                intrinsic.Returnˉtype,
                Arguments.Select(Argument => Argument.Temporary).ToImmutableArray());
        }

        private Boundˉvalue Compileˉlength(Callˉexpressionˉsyntax expression)
        {
            if (expression.Arguments.Length != 1 ||
                expression.Arguments[0] is not Nameˉexpressionˉsyntax Name ||
                !Data.TryGetValue(Name.Name, out var Declaration) ||
                Declaration is not I32ˉarrayˉdataˉdeclaration)
            {
                Report(
                    "WVC2066",
                    expression.Span,
                    "length() requires one immutable [i32] data name.");
                return Invalidˉvalue(expression.Span);
            }

            return Result(
                Wirˉoperation.Dataˉlength,
                Valueˉtype.I32,
                nameˉoperand: Name.Name);
        }

        private Boundˉvalue Callˉresult(
            Wirˉoperation operation,
            Valueˉshape returnˉtype,
            ImmutableArray<Boundˉvalue> arguments,
            string name)
        {
            var Operands = arguments.Select(Argument => Argument.Temporary).ToImmutableArray();
            if (returnˉtype == Valueˉtype.Void)
            {
                Emit(new(operation, null, Operands, Nameˉoperand: name));
                return Boundˉvalue.Void;
            }

            return Result(operation, returnˉtype, Operands, nameˉoperand: name);
        }

        private void Checkˉarguments(
            Callˉexpressionˉsyntax expression,
            ImmutableArray<Boundˉvalue> arguments,
            ImmutableArray<Valueˉshape> parameterˉtypes)
        {
            if (arguments.Length != parameterˉtypes.Length)
            {
                Report(
                    "WVC2067",
                    expression.Span,
                    $"Call to '{expression.Name}' has {arguments.Length} arguments; {parameterˉtypes.Length} are required.");
            }

            var Count = Math.Min(arguments.Length, parameterˉtypes.Length);
            for (var Index = 0; Index < Count; Index++)
            {
                Requireˉtype(
                    arguments[Index],
                    parameterˉtypes[Index],
                    expression.Arguments[Index].Span,
                    $"argument {Index + 1}");
            }
        }

        private void Checkˉarguments(
            Callˉexpressionˉsyntax expression,
            ImmutableArray<Boundˉvalue> arguments,
            ImmutableArray<Valueˉtype> parameterˉtypes)
        {
            Checkˉarguments(
                expression,
                arguments,
                [.. parameterˉtypes.Select(Type => (Valueˉshape)Type)]);
        }

        private Boundˉvalue Invalidˉvalue(Sourceˉspan span)
        {
            _ = span;
            return Result(Wirˉoperation.I32ˉconstant, Valueˉtype.I32, integerˉoperand: 0);
        }

        private Boundˉvalue Result(
            Wirˉoperation operation,
            Valueˉshape type,
            ImmutableArray<int> operands = default,
            int integerˉoperand = 0,
            uint unsignedˉintegerˉoperand = 0,
            string? nameˉoperand = null)
        {
            var Temporary = Emitˉresult(
                operation,
                type,
                operands.IsDefault ? [] : operands,
                integerˉoperand,
                unsignedˉintegerˉoperand,
                nameˉoperand);
            return new(type, Temporary);
        }

        private int Emitˉresult(
            Wirˉoperation operation,
            Valueˉshape type,
            ImmutableArray<int> operands = default,
            int integerˉoperand = 0,
            uint unsignedˉintegerˉoperand = 0,
            string? nameˉoperand = null)
        {
            var Temporary = Temporaryˉtypes.Count;
            Temporaryˉtypes.Add(type);
            Emit(new(
                operation,
                Temporary,
                operands.IsDefault ? [] : operands,
                integerˉoperand,
                unsignedˉintegerˉoperand,
                nameˉoperand));
            return Temporary;
        }

        private void Emit(Wirˉinstruction instruction)
        {
            Requireˉcurrentˉblock().Instructions.Add(instruction);
        }

        private void Requireˉtype(
            Boundˉvalue value,
            Valueˉshape required,
            Sourceˉspan span,
            string role)
        {
            if (value.Type != required)
            {
                Report(
                    "WVC2070",
                    span,
                    $"The {role} has type {Formatˉtype(value.Type)}; {Formatˉtype(required)} is required.");
            }
        }

        private bool Tryˉlookupˉlocal(string name, out Localˉsymbol local)
        {
            foreach (var Scope in Scopes)
            {
                if (Scope.TryGetValue(name, out local!))
                {
                    return true;
                }
            }

            local = null!;
            return false;
        }

        private void Enterˉscope()
        {
            Scopes.Push(new(StringComparer.Ordinal));
        }

        private void Exitˉscope()
        {
            Scopes.Pop();
        }

        private Mutableˉblock Createˉblock()
        {
            var Block = new Mutableˉblock(Blocks.Count);
            Blocks.Add(Block);
            return Block;
        }

        private Mutableˉblock Requireˉcurrentˉblock()
        {
            return Currentˉblock ?? throw new InvalidOperationException(
                $"Function '{Function.Name}' has no current WIR block.");
        }

        private Valueˉshape Bindˉvalueˉshape(Typeˉsyntax type)
        {
            if (type.Kind == Typeˉsyntaxˉkind.Record)
            {
                if (type.Name is not null && Records.TryGetValue(type.Name, out var Record))
                {
                    return Valueˉshape.Forˉrecord(Record.Index);
                }

                Report("WVC2085", type.Span, $"Record type '{type.Name}' is not declared.");
                return Valueˉtype.I32;
            }

            return type.Kind switch
            {
                Typeˉsyntaxˉkind.Void => Valueˉtype.Void,
                Typeˉsyntaxˉkind.I32 => Valueˉtype.I32,
                Typeˉsyntaxˉkind.U8 => Valueˉtype.U8,
                Typeˉsyntaxˉkind.U32 => Valueˉtype.U32,
                Typeˉsyntaxˉkind.Bool => Valueˉtype.Bool,
                Typeˉsyntaxˉkind.Text => Valueˉtype.Text,
                Typeˉsyntaxˉkind.Bytes => Valueˉtype.Bytes,
                _ => Valueˉtype.I32,
            };
        }

        private void Report(string code, Sourceˉspan span, string message)
        {
            Diagnostics.Report(code, "semantic", span, message);
        }

        private string Formatˉtype(Valueˉshape type)
        {
            if (type.Kind == Valueˉtype.Record &&
                Records.Values.FirstOrDefault(Record => Record.Index == type.Recordˉtypeˉindex) is { } Record)
            {
                return Record.Name;
            }

            return type.Kind switch
            {
                Valueˉtype.Void => "void",
                Valueˉtype.I32 => "i32",
                Valueˉtype.U8 => "u8",
                Valueˉtype.U32 => "u32",
                Valueˉtype.Bool => "bool",
                Valueˉtype.Text => "text",
                Valueˉtype.Bytes => "bytes",
                _ => type.ToString(),
            };
        }
    }
}
