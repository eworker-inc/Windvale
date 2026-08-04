using System.Collections.Immutable;

namespace Windvale.Compiler;

internal sealed class Sourceˉparser
{
    private readonly Diagnosticˉbag Diagnostics;
    private readonly ImmutableArray<Syntaxˉtoken> Tokens;
    private int Position;

    public Sourceˉparser(string source, string sourceˉname, Diagnosticˉbag diagnostics)
    {
        Diagnostics = diagnostics;
        var Lexer = new Sourceˉlexer(source, sourceˉname, diagnostics);
        var Tokensˉbuilder = ImmutableArray.CreateBuilder<Syntaxˉtoken>();
        Syntaxˉtoken Token;
        do
        {
            Token = Lexer.Lex();
            if (Token.Kind != Tokenˉkind.Bad)
            {
                Tokensˉbuilder.Add(Token);
            }
        }
        while (Token.Kind != Tokenˉkind.End);

        Tokens = Tokensˉbuilder.ToImmutable();
    }

    public Moduleˉsyntax Parseˉmodule()
    {
        Match(Tokenˉkind.Module);
        var Name = Match(Tokenˉkind.Identifier);
        Syntaxˉtoken Profile;
        Moduleˉmetadataˉsyntax? Metadata;
        if (Current.Kind == Tokenˉkind.Profile)
        {
            Nextˉtoken();
            Profile = Current.Kind is Tokenˉkind.Portable or Tokenˉkind.Hosted or Tokenˉkind.System
                ? Nextˉtoken()
                : Match(Tokenˉkind.Portable);
            Match(Tokenˉkind.Semicolon);
            Metadata = null;
        }
        else
        {
            Match(Tokenˉkind.Semicolon);
            Metadata = Parseˉmoduleˉmetadata();
            var Profileˉkind = Metadata.Authority.Text == "system"
                ? Tokenˉkind.System
                : Metadata.Requiredˉcapabilities.Length != 0 || Metadata.Optionalˉcapabilities.Length != 0
                    ? Tokenˉkind.Hosted
                    : Tokenˉkind.Portable;
            var Profileˉtext = Profileˉkind.ToString().ToLowerInvariant();
            Profile = new(Profileˉkind, Profileˉtext, Metadata.Authority.Span);
        }

        var Imports = ImmutableArray.CreateBuilder<Importˉsyntax>();
        var Capabilities = ImmutableArray.CreateBuilder<Capabilityˉsyntax>();
        if (Metadata is not null)
        {
            Capabilities.AddRange(Metadata.Requiredˉcapabilities.Select(Requirement =>
                new Capabilityˉsyntax(Requirement.Name, Requirement.Span)));
        }
        var Data = ImmutableArray.CreateBuilder<Dataˉsyntax>();
        var Constants = ImmutableArray.CreateBuilder<Constantˉsyntax>();
        var Records = ImmutableArray.CreateBuilder<Recordˉsyntax>();
        var Enums = ImmutableArray.CreateBuilder<Enumˉsyntax>();
        var Variants = ImmutableArray.CreateBuilder<Variantˉsyntax>();
        var Functions = ImmutableArray.CreateBuilder<Functionˉsyntax>();
        var Sawˉnonˉimportˉdeclaration = false;

        while (Current.Kind != Tokenˉkind.End)
        {
            var Startˉposition = Position;
            switch (Current.Kind)
            {
                case Tokenˉkind.Import:
                    if (Sawˉnonˉimportˉdeclaration)
                    {
                        Diagnostics.Report(
                            "WVC1107",
                            "parser",
                            Current.Span,
                            "Source imports must precede every capability, data, type, and function declaration.");
                    }
                    Imports.Add(Parseˉimport());
                    break;
                case Tokenˉkind.Capability:
                    Sawˉnonˉimportˉdeclaration = true;
                    Capabilities.Add(Parseˉcapability());
                    break;
                case Tokenˉkind.Data:
                    Sawˉnonˉimportˉdeclaration = true;
                    Data.Add(Parseˉdata(false));
                    break;
                case Tokenˉkind.Const:
                    Sawˉnonˉimportˉdeclaration = true;
                    Constants.Add(Parseˉconstant(false));
                    break;
                case Tokenˉkind.Record:
                    Sawˉnonˉimportˉdeclaration = true;
                    Records.Add(Parseˉrecord(false));
                    break;
                case Tokenˉkind.Enum:
                    Sawˉnonˉimportˉdeclaration = true;
                    Enums.Add(Parseˉenum(false));
                    break;
                case Tokenˉkind.Variant:
                    Sawˉnonˉimportˉdeclaration = true;
                    Variants.Add(Parseˉvariant(false));
                    break;
                case Tokenˉkind.Export:
                    Sawˉnonˉimportˉdeclaration = true;
                    switch (Peek(1).Kind)
                    {
                        case Tokenˉkind.Data:
                            Data.Add(Parseˉdata(true));
                            break;
                        case Tokenˉkind.Const:
                            Constants.Add(Parseˉconstant(true));
                            break;
                        case Tokenˉkind.Record:
                            Records.Add(Parseˉrecord(true));
                            break;
                        case Tokenˉkind.Enum:
                            Enums.Add(Parseˉenum(true));
                            break;
                        case Tokenˉkind.Variant:
                            Variants.Add(Parseˉvariant(true));
                            break;
                        case Tokenˉkind.Fn:
                            Functions.Add(Parseˉfunction());
                            break;
                        default:
                            Diagnostics.Report(
                                "WVC1108",
                                "parser",
                                Current.Span,
                                "Export must precede a data, constant, record, enum, or function declaration.");
                            Nextˉtoken();
                            break;
                    }
                    break;
                case Tokenˉkind.Fn:
                    Sawˉnonˉimportˉdeclaration = true;
                    Functions.Add(Parseˉfunction());
                    break;
                default:
                    Diagnostics.Report(
                        "WVC1100",
                        "parser",
                        Current.Span,
                        $"Expected an import, capability, data, constant, record, enum, or function declaration but found '{Current.Text}'.");
                    Nextˉtoken();
                    break;
            }

            if (Position == Startˉposition)
            {
                Nextˉtoken();
            }
        }

        return new(
            Name,
            Profile,
            Metadata,
            Imports.ToImmutable(),
            Capabilities.ToImmutable(),
            Data.ToImmutable(),
            Constants.ToImmutable(),
            Records.ToImmutable(),
            Enums.ToImmutable(),
            Variants.ToImmutable(),
            Functions.ToImmutable());
    }

    private Moduleˉmetadataˉsyntax Parseˉmoduleˉmetadata()
    {
        Match(Tokenˉkind.Platform);
        var Platforms = ImmutableArray.CreateBuilder<Platformˉscopeˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Semicolon and not Tokenˉkind.End)
        {
            var Scope = Parseˉqualifiedˉname(allowˉlength: false);
            Platforms.Add(new(Scope.Name, Scope.Span));
            if (Current.Kind != Tokenˉkind.Comma)
            {
                break;
            }
            Nextˉtoken();
            if (Current.Kind == Tokenˉkind.Semicolon)
            {
                break;
            }
        }
        Match(Tokenˉkind.Semicolon);

        Match(Tokenˉkind.Authority);
        var Authority = Current.Kind is Tokenˉkind.Identifier or Tokenˉkind.System
            ? Nextˉtoken()
            : Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Semicolon);

        var Required = ImmutableArray.CreateBuilder<Capabilityˉrequirementˉsyntax>();
        var Optional = ImmutableArray.CreateBuilder<Capabilityˉrequirementˉsyntax>();
        while (Current.Kind is Tokenˉkind.Requires or Tokenˉkind.Optional)
        {
            var Isˉoptional = Nextˉtoken().Kind == Tokenˉkind.Optional;
            var Start = Match(Tokenˉkind.Capability);
            var Name = Parseˉqualifiedˉname(allowˉlength: false);
            Match(Tokenˉkind.Version);
            var Version = Match(Tokenˉkind.Integer);
            var End = Match(Tokenˉkind.Semicolon);
            uint Majorˉversion = Version.Value is int Value && Value > 0
                ? (uint)Value
                : 0;
            if (Majorˉversion == 0)
            {
                Diagnostics.Report(
                    "WVC1110",
                    "parser",
                    Version.Span,
                    "A capability requirement needs a positive unsuffixed major version.");
            }

            var Requirement = new Capabilityˉrequirementˉsyntax(
                Name.Name,
                Majorˉversion,
                Combine(Start.Span, End.Span));
            (Isˉoptional ? Optional : Required).Add(Requirement);
        }

        return new(
            Authority,
            Platforms.ToImmutable(),
            Required.ToImmutable(),
            Optional.ToImmutable());
    }

    private Importˉsyntax Parseˉimport()
    {
        var Start = Match(Tokenˉkind.Import);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.As);
        var Alias = Match(Tokenˉkind.Identifier);
        var End = Match(Tokenˉkind.Semicolon);
        return new(Name, Alias, Combine(Start.Span, End.Span));
    }

    private Recordˉsyntax Parseˉrecord(bool isˉexported)
    {
        var Start = isˉexported ? Match(Tokenˉkind.Export) : Current;
        Match(Tokenˉkind.Record);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Leftˉbrace);
        var Fields = ImmutableArray.CreateBuilder<Recordˉfieldˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Rightˉbrace and not Tokenˉkind.End)
        {
            var Fieldˉname = Match(Tokenˉkind.Identifier);
            Match(Tokenˉkind.Colon);
            var Fieldˉtype = Parseˉtype(allowˉvoid: false, allowˉarray: false);
            var End = Match(Tokenˉkind.Semicolon);
            Fields.Add(new(Fieldˉname, Fieldˉtype, Combine(Fieldˉname.Span, End.Span)));
        }

        var Recordˉend = Match(Tokenˉkind.Rightˉbrace);
        return new(isˉexported, Name, Fields.ToImmutable(), Combine(Start.Span, Recordˉend.Span));
    }

    private Enumˉsyntax Parseˉenum(bool isˉexported)
    {
        var Start = isˉexported ? Match(Tokenˉkind.Export) : Current;
        Match(Tokenˉkind.Enum);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Leftˉbrace);
        var Members = ImmutableArray.CreateBuilder<Enumˉmemberˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Rightˉbrace and not Tokenˉkind.End)
        {
            var Memberˉname = Match(Tokenˉkind.Identifier);
            Match(Tokenˉkind.Equals);
            var Value = Match(Tokenˉkind.Integer);
            var End = Match(Tokenˉkind.Semicolon);
            Members.Add(new(Memberˉname, Value, Combine(Memberˉname.Span, End.Span)));
        }

        var Enumˉend = Match(Tokenˉkind.Rightˉbrace);
        return new(isˉexported, Name, Members.ToImmutable(), Combine(Start.Span, Enumˉend.Span));
    }

    private Variantˉsyntax Parseˉvariant(bool isˉexported)
    {
        var Start = isˉexported ? Match(Tokenˉkind.Export) : Current;
        Match(Tokenˉkind.Variant);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Leftˉbrace);
        var Cases = ImmutableArray.CreateBuilder<Variantˉcaseˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Rightˉbrace and not Tokenˉkind.End)
        {
            var Caseˉname = Match(Tokenˉkind.Identifier);
            Syntaxˉtoken? Payloadˉname = null;
            Typeˉsyntax? Payloadˉtype = null;
            if (Current.Kind == Tokenˉkind.Leftˉparenthesis)
            {
                Nextˉtoken();
                Payloadˉname = Match(Tokenˉkind.Identifier);
                Match(Tokenˉkind.Colon);
                Payloadˉtype = Parseˉtype(allowˉvoid: false, allowˉarray: false);
                Match(Tokenˉkind.Rightˉparenthesis);
            }
            var End = Match(Tokenˉkind.Semicolon);
            Cases.Add(new(
                Caseˉname,
                Payloadˉname,
                Payloadˉtype,
                Combine(Caseˉname.Span, End.Span)));
        }

        var Variantˉend = Match(Tokenˉkind.Rightˉbrace);
        return new(isˉexported, Name, Cases.ToImmutable(), Combine(Start.Span, Variantˉend.Span));
    }

    private Capabilityˉsyntax Parseˉcapability()
    {
        var Start = Match(Tokenˉkind.Capability);
        var Name = Parseˉqualifiedˉname(allowˉlength: false);
        var End = Match(Tokenˉkind.Semicolon);
        return new(Name.Name, Combine(Start.Span, End.Span));
    }

    private Dataˉsyntax Parseˉdata(bool isˉexported)
    {
        var Start = isˉexported ? Match(Tokenˉkind.Export) : Current;
        Match(Tokenˉkind.Data);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Colon);
        var Type = Parseˉtype(allowˉvoid: false, allowˉarray: true);
        Match(Tokenˉkind.Equals);
        Dataˉvalueˉsyntax Value;
        if (Type.Kind == Typeˉsyntaxˉkind.Text)
        {
            var String = Match(Tokenˉkind.String);
            Value = new Textˉdataˉvalueˉsyntax((string?)String.Value ?? string.Empty, String.Span);
        }
        else if (Type.Kind == Typeˉsyntaxˉkind.I32ˉarray)
        {
            Value = Parseˉi32ˉarray();
        }
        else if (Type.Kind == Typeˉsyntaxˉkind.Bytes)
        {
            Value = Parseˉbytes();
        }
        else
        {
            Diagnostics.Report(
                "WVC1101",
                "parser",
                Type.Span,
                "Module data must have type text, bytes, or [i32].");
            var Invalid = Current;
            Nextˉtoken();
            Value = new I32ˉarrayˉdataˉvalueˉsyntax([], Invalid.Span);
        }

        var End = Match(Tokenˉkind.Semicolon);
        return new(isˉexported, Name, Type, Value, Combine(Start.Span, End.Span));
    }

    private Constantˉsyntax Parseˉconstant(bool isˉexported)
    {
        var Start = isˉexported ? Match(Tokenˉkind.Export) : Current;
        Match(Tokenˉkind.Const);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Colon);
        var Type = Parseˉtype(allowˉvoid: false, allowˉarray: false);
        Match(Tokenˉkind.Equals);
        var Initializer = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(isˉexported, Name, Type, Initializer, Combine(Start.Span, End.Span));
    }

    private I32ˉarrayˉdataˉvalueˉsyntax Parseˉi32ˉarray()
    {
        var Start = Match(Tokenˉkind.Leftˉbracket);
        var Values = ImmutableArray.CreateBuilder<int>();
        while (Current.Kind is not Tokenˉkind.Rightˉbracket and not Tokenˉkind.End)
        {
            var Isˉnegative = Current.Kind == Tokenˉkind.Minus;
            if (Isˉnegative)
            {
                Nextˉtoken();
            }

            var Integer = Match(Tokenˉkind.Integer);
            var Value = Integer.Value is int Parsed ? Parsed : 0;
            if (Integer.Value is not int)
            {
                Diagnostics.Report(
                    "WVC1105",
                    "parser",
                    Integer.Span,
                    "An [i32] data element requires an unsuffixed i32 literal.");
            }

            Values.Add(Isˉnegative ? -Value : Value);
            if (Current.Kind != Tokenˉkind.Comma)
            {
                break;
            }

            Nextˉtoken();
        }

        var End = Match(Tokenˉkind.Rightˉbracket);
        return new(Values.ToImmutable(), Combine(Start.Span, End.Span));
    }

    private Bytesˉdataˉvalueˉsyntax Parseˉbytes()
    {
        var Start = Match(Tokenˉkind.Leftˉbracket);
        var Values = ImmutableArray.CreateBuilder<byte>();
        while (Current.Kind is not Tokenˉkind.Rightˉbracket and not Tokenˉkind.End)
        {
            var Integer = Match(Tokenˉkind.Integer);
            byte Value;
            switch (Integer.Value)
            {
                case byte Byte:
                    Value = Byte;
                    break;
                case int I32 when I32 is >= byte.MinValue and <= byte.MaxValue:
                    Value = (byte)I32;
                    break;
                default:
                    Diagnostics.Report(
                        "WVC1106",
                        "parser",
                        Integer.Span,
                        "A bytes data element must be an unsuffixed or u8 literal from 0 through 255.");
                    Value = 0;
                    break;
            }

            Values.Add(Value);
            if (Current.Kind != Tokenˉkind.Comma)
            {
                break;
            }

            Nextˉtoken();
        }

        var End = Match(Tokenˉkind.Rightˉbracket);
        return new(Values.ToImmutable(), Combine(Start.Span, End.Span));
    }

    private Functionˉsyntax Parseˉfunction()
    {
        var Isˉexported = Current.Kind == Tokenˉkind.Export;
        var Start = Isˉexported ? Nextˉtoken() : Current;
        if (!Isˉexported)
        {
            Start = Current;
        }

        Match(Tokenˉkind.Fn);
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Leftˉparenthesis);
        var Parameters = ImmutableArray.CreateBuilder<Parameterˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Rightˉparenthesis and not Tokenˉkind.End)
        {
            var Parameterˉname = Match(Tokenˉkind.Identifier);
            Match(Tokenˉkind.Colon);
            var Parameterˉtype = Parseˉtype(allowˉvoid: false, allowˉarray: false);
            Parameters.Add(new(
                Parameterˉname,
                Parameterˉtype,
                Combine(Parameterˉname.Span, Parameterˉtype.Span)));
            if (Current.Kind != Tokenˉkind.Comma)
            {
                break;
            }

            Nextˉtoken();
        }

        Match(Tokenˉkind.Rightˉparenthesis);
        Match(Tokenˉkind.Arrow);
        var Returnˉtype = Parseˉtype(allowˉvoid: true, allowˉarray: false);
        var Body = Parseˉblock();
        return new(
            Isˉexported,
            Name,
            Parameters.ToImmutable(),
            Returnˉtype,
            Body,
            Combine(Start.Span, Body.Span));
    }

    private Blockˉstatementˉsyntax Parseˉblock()
    {
        var Start = Match(Tokenˉkind.Leftˉbrace);
        var Statements = ImmutableArray.CreateBuilder<Statementˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Rightˉbrace and not Tokenˉkind.End)
        {
            var Startˉposition = Position;
            Statements.Add(Parseˉstatement());
            if (Position == Startˉposition)
            {
                Nextˉtoken();
            }
        }

        var End = Match(Tokenˉkind.Rightˉbrace);
        return new(Statements.ToImmutable(), Combine(Start.Span, End.Span));
    }

    private Statementˉsyntax Parseˉstatement()
    {
        return Current.Kind switch
        {
            Tokenˉkind.Let or Tokenˉkind.Var => Parseˉlocalˉdeclaration(),
            Tokenˉkind.If => Parseˉif(),
            Tokenˉkind.While => Parseˉwhile(),
            Tokenˉkind.Push => Parseˉpush(),
            Tokenˉkind.For => Parseˉfor(),
            Tokenˉkind.Return => Parseˉreturn(),
            Tokenˉkind.Break => Parseˉbreak(),
            Tokenˉkind.Continue => Parseˉcontinue(),
            Tokenˉkind.Match => Parseˉmatch(),
            Tokenˉkind.Leftˉbrace => Parseˉblock(),
            Tokenˉkind.Identifier when Isˉassignmentˉoperator(Peek(1).Kind) => Parseˉassignment(),
            _ => Parseˉexpressionˉstatement(),
        };
    }

    private Localˉdeclarationˉstatementˉsyntax Parseˉlocalˉdeclaration()
    {
        var Start = Nextˉtoken();
        var Isˉmutable = Start.Kind == Tokenˉkind.Var;
        var Name = Match(Tokenˉkind.Identifier);
        Typeˉsyntax? Type = null;
        if (Current.Kind == Tokenˉkind.Colon)
        {
            Nextˉtoken();
            Type = Parseˉtype(allowˉvoid: false, allowˉarray: false);
        }
        Match(Tokenˉkind.Equals);
        var Initializer = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Isˉmutable, Name, Type, Initializer, Combine(Start.Span, End.Span));
    }

    private Assignmentˉstatementˉsyntax Parseˉassignment()
    {
        var Name = Match(Tokenˉkind.Identifier);
        var Operator = Isˉassignmentˉoperator(Current.Kind)
            ? Nextˉtoken()
            : Match(Tokenˉkind.Equals);
        var Value = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Name, Operator.Kind, Value, Combine(Name.Span, End.Span));
    }

    private Ifˉstatementˉsyntax Parseˉif()
    {
        var Start = Match(Tokenˉkind.If);
        var Condition = Parseˉexpression();
        var Then = Parseˉblock();
        Blockˉstatementˉsyntax? Else = null;
        if (Current.Kind == Tokenˉkind.Else)
        {
            Nextˉtoken();
            if (Current.Kind == Tokenˉkind.If)
            {
                var Nested = Parseˉif();
                Else = new([Nested], Nested.Span);
            }
            else
            {
                Else = Parseˉblock();
            }
        }

        return new(Condition, Then, Else, Combine(Start.Span, (Else ?? Then).Span));
    }

    private Whileˉstatementˉsyntax Parseˉwhile()
    {
        var Start = Match(Tokenˉkind.While);
        var Condition = Parseˉexpression();
        var Body = Parseˉblock();
        return new(Condition, Body, Combine(Start.Span, Body.Span));
    }

    private Pushˉstatementˉsyntax Parseˉpush()
    {
        var Start = Match(Tokenˉkind.Push);
        var Builder = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Comma);
        var Value = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Builder, Value, Combine(Start.Span, End.Span));
    }

    private Forˉstatementˉsyntax Parseˉfor()
    {
        var Start = Match(Tokenˉkind.For);
        var Binding = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.In);
        var Sequence = Parseˉexpression();
        var Body = Parseˉblock();
        return new(Binding, Sequence, Body, Combine(Start.Span, Body.Span));
    }

    private Returnˉstatementˉsyntax Parseˉreturn()
    {
        var Start = Match(Tokenˉkind.Return);
        var Value = Current.Kind == Tokenˉkind.Semicolon ? null : Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Value, Combine(Start.Span, End.Span));
    }

    private Breakˉstatementˉsyntax Parseˉbreak()
    {
        var Start = Match(Tokenˉkind.Break);
        var End = Match(Tokenˉkind.Semicolon);
        return new(Combine(Start.Span, End.Span));
    }

    private Continueˉstatementˉsyntax Parseˉcontinue()
    {
        var Start = Match(Tokenˉkind.Continue);
        var End = Match(Tokenˉkind.Semicolon);
        return new(Combine(Start.Span, End.Span));
    }

    private Matchˉstatementˉsyntax Parseˉmatch()
    {
        var Start = Match(Tokenˉkind.Match);
        Expressionˉsyntax Value;
        if (Current.Kind == Tokenˉkind.Identifier && Peek(1).Kind == Tokenˉkind.Leftˉbrace)
        {
            var Name = Nextˉtoken();
            Value = new Nameˉexpressionˉsyntax(Name.Text, Name.Span);
        }
        else
        {
            Value = Parseˉexpression();
        }

        Match(Tokenˉkind.Leftˉbrace);
        var Cases = ImmutableArray.CreateBuilder<Matchˉcaseˉsyntax>();
        while (Current.Kind is not Tokenˉkind.Rightˉbrace and not Tokenˉkind.End)
        {
            var Caseˉstart = Match(Tokenˉkind.Case);
            var Pattern = Parseˉqualifiedˉname(allowˉlength: false);
            var Separator = Pattern.Name.LastIndexOf(".", StringComparison.Ordinal);
            if (Separator <= 0 || Separator == Pattern.Name.Length - 1)
            {
                Diagnostics.Report(
                    "WVC1111",
                    "parser",
                    Pattern.Span,
                    "A match case requires an enum type and member in the form Enum.Member.");
            }
            var Nominalˉname = Separator > 0 ? Pattern.Name[..Separator] : Pattern.Name;
            var Memberˉname = Separator > 0 ? Pattern.Name[(Separator + 1)..] : string.Empty;
            Syntaxˉtoken? Binding = null;
            if (Current.Kind == Tokenˉkind.Leftˉparenthesis)
            {
                Nextˉtoken();
                Binding = Match(Tokenˉkind.Identifier);
                Match(Tokenˉkind.Rightˉparenthesis);
            }
            var Body = Parseˉblock();
            Cases.Add(new(
                Nominalˉname,
                Memberˉname,
                Binding,
                Body,
                Combine(Caseˉstart.Span, Body.Span)));
        }
        var End = Match(Tokenˉkind.Rightˉbrace);
        return new(Value, Cases.ToImmutable(), Combine(Start.Span, End.Span));
    }

    private Expressionˉstatementˉsyntax Parseˉexpressionˉstatement()
    {
        var Expression = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Expression, Combine(Expression.Span, End.Span));
    }

    private Expressionˉsyntax Parseˉexpression(int parentˉprecedence = 0)
    {
        Expressionˉsyntax Left;
        var Unaryˉprecedence = Getˉunaryˉprecedence(Current.Kind);
        if (Unaryˉprecedence != 0 && Unaryˉprecedence >= parentˉprecedence)
        {
            var Operator = Nextˉtoken();
            var Operand = Parseˉexpression(Unaryˉprecedence);
            Left = new Unaryˉexpressionˉsyntax(
                Operator.Kind,
                Operand,
                Combine(Operator.Span, Operand.Span));
        }
        else
        {
            Left = Parseˉprimary();
        }

        while (true)
        {
            var Precedence = Getˉbinaryˉprecedence(Current.Kind);
            if (Precedence == 0 || Precedence <= parentˉprecedence)
            {
                break;
            }

            var Operator = Nextˉtoken();
            var Right = Parseˉexpression(Precedence);
            Left = new Binaryˉexpressionˉsyntax(
                Left,
                Operator.Kind,
                Right,
                Combine(Left.Span, Right.Span));
        }

        return Left;
    }

    private Expressionˉsyntax Parseˉprimary()
    {
        switch (Current.Kind)
        {
            case Tokenˉkind.Integer:
                var Integer = Nextˉtoken();
                return new Literalˉexpressionˉsyntax(Integer.Value ?? 0, Integer.Span);
            case Tokenˉkind.String:
                var String = Nextˉtoken();
                return new Literalˉexpressionˉsyntax((string?)String.Value ?? string.Empty, String.Span);
            case Tokenˉkind.True:
            case Tokenˉkind.False:
                var Boolean = Nextˉtoken();
                return new Literalˉexpressionˉsyntax(Boolean.Kind == Tokenˉkind.True, Boolean.Span);
            case Tokenˉkind.Leftˉparenthesis:
                var Start = Nextˉtoken();
                var Expression = Parseˉexpression();
                var End = Match(Tokenˉkind.Rightˉparenthesis);
                return Expression with { Span = Combine(Start.Span, End.Span) };
            case Tokenˉkind.Builder:
                return Parseˉbuilderˉconstructor();
            case Tokenˉkind.Identifier:
            case Tokenˉkind.Length:
                return Parseˉnameˉorˉpostfix();
            default:
                var Invalid = Current;
                Diagnostics.Report(
                    "WVC1102",
                    "parser",
                    Invalid.Span,
                    $"Expected an expression but found '{Invalid.Text}'.");
                if (Current.Kind != Tokenˉkind.End)
                {
                    Nextˉtoken();
                }

                return new Invalidˉexpressionˉsyntax(Invalid.Span);
        }
    }

    private Builderˉexpressionˉsyntax Parseˉbuilderˉconstructor()
    {
        var Type = Parseˉtype(allowˉvoid: false, allowˉarray: false);
        Match(Tokenˉkind.Leftˉparenthesis);
        var End = Match(Tokenˉkind.Rightˉparenthesis);
        return new(Type, Combine(Type.Span, End.Span));
    }

    private Expressionˉsyntax Parseˉnameˉorˉpostfix()
    {
        var Name = Parseˉqualifiedˉname(allowˉlength: true);
        if (Current.Kind == Tokenˉkind.Leftˉbrace &&
            Peek(1).Kind == Tokenˉkind.Identifier &&
            Peek(2).Kind == Tokenˉkind.Colon)
        {
            Nextˉtoken();
            var Fields = ImmutableArray.CreateBuilder<Recordˉfieldˉinitializerˉsyntax>();
            while (Current.Kind is not Tokenˉkind.Rightˉbrace and not Tokenˉkind.End)
            {
                var Fieldˉname = Match(Tokenˉkind.Identifier);
                Match(Tokenˉkind.Colon);
                var Value = Parseˉexpression();
                Fields.Add(new(Fieldˉname, Value, Combine(Fieldˉname.Span, Value.Span)));
                if (Current.Kind != Tokenˉkind.Comma)
                {
                    break;
                }

                Nextˉtoken();
            }

            var End = Match(Tokenˉkind.Rightˉbrace);
            return new Recordˉexpressionˉsyntax(
                Name.Name,
                Fields.ToImmutable(),
                Combine(Name.Span, End.Span));
        }

        if (Current.Kind == Tokenˉkind.Leftˉparenthesis)
        {
            Nextˉtoken();
            var Arguments = ImmutableArray.CreateBuilder<Expressionˉsyntax>();
            while (Current.Kind is not Tokenˉkind.Rightˉparenthesis and not Tokenˉkind.End)
            {
                Arguments.Add(Parseˉexpression());
                if (Current.Kind != Tokenˉkind.Comma)
                {
                    break;
                }

                Nextˉtoken();
            }

            var End = Match(Tokenˉkind.Rightˉparenthesis);
            return new Callˉexpressionˉsyntax(
                Name.Name,
                Arguments.ToImmutable(),
                Combine(Name.Span, End.Span));
        }

        if (Current.Kind == Tokenˉkind.Leftˉbracket)
        {
            Nextˉtoken();
            var Index = Parseˉexpression();
            var End = Match(Tokenˉkind.Rightˉbracket);
            return new Indexˉexpressionˉsyntax(Name.Name, Index, Combine(Name.Span, End.Span));
        }

        var Dotˉindex = Name.Name.IndexOf('.', StringComparison.Ordinal);
        if (Dotˉindex >= 0 && Name.Name.IndexOf('.', Dotˉindex + 1) < 0)
        {
            return new Fieldˉexpressionˉsyntax(
                Name.Name[..Dotˉindex],
                Name.Name[(Dotˉindex + 1)..],
                Name.Span);
        }

        return new Nameˉexpressionˉsyntax(Name.Name, Name.Span);
    }

    private (string Name, Sourceˉspan Span) Parseˉqualifiedˉname(bool allowˉlength)
    {
        Syntaxˉtoken First;
        if (allowˉlength && Current.Kind == Tokenˉkind.Length)
        {
            First = Nextˉtoken();
        }
        else
        {
            First = Match(Tokenˉkind.Identifier);
        }

        var Parts = new List<string> { First.Text };
        var End = First.Span;
        while (Current.Kind == Tokenˉkind.Dot)
        {
            Nextˉtoken();
            var Part = Match(Tokenˉkind.Identifier);
            Parts.Add(Part.Text);
            End = Part.Span;
        }

        return (string.Join('.', Parts), Combine(First.Span, End));
    }

    private Typeˉsyntax Parseˉtype(bool allowˉvoid, bool allowˉarray)
    {
        if (allowˉarray && Current.Kind == Tokenˉkind.Leftˉbracket)
        {
            var Start = Nextˉtoken();
            Match(Tokenˉkind.I32);
            var End = Match(Tokenˉkind.Rightˉbracket);
            return new(Typeˉsyntaxˉkind.I32ˉarray, Combine(Start.Span, End.Span));
        }

        if (Current.Kind is Tokenˉkind.Sequence or Tokenˉkind.Builder)
        {
            var Start = Nextˉtoken();
            Match(Tokenˉkind.Less);
            var Element = Parseˉtype(allowˉvoid: false, allowˉarray: false);
            Match(Tokenˉkind.Comma);
            var Maximumˉtoken = Match(Tokenˉkind.Integer);
            var Maximum = Maximumˉtoken.Value switch
            {
                int Value when Value >= 0 => (uint)Value,
                uint Value => Value,
                _ => 0u,
            };
            var End = Match(Tokenˉkind.Greater);
            return new(
                Start.Kind == Tokenˉkind.Sequence
                    ? Typeˉsyntaxˉkind.Sequence
                    : Typeˉsyntaxˉkind.Builder,
                Combine(Start.Span, End.Span),
                Elementˉtype: Element,
                Maximum: Maximum);
        }

        var Token = Current;
        var Kind = Token.Kind switch
        {
            Tokenˉkind.I32 => Typeˉsyntaxˉkind.I32,
            Tokenˉkind.I64 => Typeˉsyntaxˉkind.I64,
            Tokenˉkind.U8 => Typeˉsyntaxˉkind.U8,
            Tokenˉkind.U32 => Typeˉsyntaxˉkind.U32,
            Tokenˉkind.U64 => Typeˉsyntaxˉkind.U64,
            Tokenˉkind.Bool => Typeˉsyntaxˉkind.Bool,
            Tokenˉkind.Text => Typeˉsyntaxˉkind.Text,
            Tokenˉkind.Bytes => Typeˉsyntaxˉkind.Bytes,
            Tokenˉkind.Void when allowˉvoid => Typeˉsyntaxˉkind.Void,
            Tokenˉkind.Identifier => Typeˉsyntaxˉkind.Named,
            _ => Typeˉsyntaxˉkind.Invalid,
        };

        if (Kind == Typeˉsyntaxˉkind.Invalid)
        {
            Diagnostics.Report(
                "WVC1103",
                "parser",
                Token.Span,
                allowˉvoid
                    ? "Expected a primitive type, declared record name, or void."
                    : "Expected a primitive type or declared record name.");
            if (Current.Kind != Tokenˉkind.End)
            {
                Nextˉtoken();
            }

            return new(Typeˉsyntaxˉkind.Invalid, Token.Span);
        }

        if (Kind == Typeˉsyntaxˉkind.Named)
        {
            var Name = Parseˉqualifiedˉname(allowˉlength: false);
            return new(Kind, Name.Span, Name.Name);
        }

        Nextˉtoken();
        return new(Kind, Token.Span);
    }

    private Syntaxˉtoken Match(Tokenˉkind expected)
    {
        if (Current.Kind == expected)
        {
            return Nextˉtoken();
        }

        Diagnostics.Report(
            "WVC1104",
            "parser",
            Current.Span,
            $"Expected {Formatˉkind(expected)} but found '{Current.Text}'.");
        return new(
            expected,
            string.Empty,
            new(Current.Span.Start, 0, Current.Span.Line, Current.Span.Column, Current.Span.Sourceˉname));
    }

    private Syntaxˉtoken Nextˉtoken()
    {
        var Currentˉtoken = Current;
        if (Position < Tokens.Length - 1)
        {
            Position++;
        }

        return Currentˉtoken;
    }

    private Syntaxˉtoken Current => Peek(0);

    private Syntaxˉtoken Peek(int offset)
    {
        var Index = Math.Min(Position + offset, Tokens.Length - 1);
        return Tokens[Index];
    }

    private static int Getˉunaryˉprecedence(Tokenˉkind kind)
    {
        return kind is Tokenˉkind.Minus or Tokenˉkind.Bang or Tokenˉkind.Tilde or Tokenˉkind.Freeze ? 13 : 0;
    }

    private static int Getˉbinaryˉprecedence(Tokenˉkind kind)
    {
        return kind switch
        {
            Tokenˉkind.Orˉor => 3,
            Tokenˉkind.Andˉand => 4,
            Tokenˉkind.Pipe => 5,
            Tokenˉkind.Caret => 6,
            Tokenˉkind.Ampersand => 7,
            Tokenˉkind.Equalsˉequals or Tokenˉkind.Bangˉequals => 8,
            Tokenˉkind.Less or Tokenˉkind.Lessˉequals or Tokenˉkind.Greater or Tokenˉkind.Greaterˉequals => 9,
            Tokenˉkind.Shiftˉleft or Tokenˉkind.Shiftˉright => 10,
            Tokenˉkind.Plus or Tokenˉkind.Minus => 11,
            Tokenˉkind.Star or Tokenˉkind.Slash or Tokenˉkind.Percent => 12,
            _ => 0,
        };
    }

    private static bool Isˉassignmentˉoperator(Tokenˉkind kind)
    {
        return kind is
            Tokenˉkind.Equals or
            Tokenˉkind.Plusˉequals or
            Tokenˉkind.Minusˉequals or
            Tokenˉkind.Starˉequals;
    }

    private static string Formatˉkind(Tokenˉkind kind)
    {
        return kind switch
        {
            Tokenˉkind.Identifier => "an identifier",
            Tokenˉkind.Integer => "an integer literal",
            Tokenˉkind.String => "a string literal",
            _ => $"'{kind}'",
        };
    }

    private static Sourceˉspan Combine(Sourceˉspan start, Sourceˉspan end)
    {
        return new(
            start.Start,
            Math.Max(0, end.Start + end.Length - start.Start),
            start.Line,
            start.Column,
            start.Sourceˉname);
    }
}
