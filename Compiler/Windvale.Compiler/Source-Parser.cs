using System.Collections.Immutable;

namespace Windvale.Compiler;

internal sealed class Sourceˉparser
{
    private readonly Diagnosticˉbag Diagnostics;
    private readonly ImmutableArray<Syntaxˉtoken> Tokens;
    private int Position;

    public Sourceˉparser(string source, Diagnosticˉbag diagnostics)
    {
        Diagnostics = diagnostics;
        var Lexer = new Sourceˉlexer(source, diagnostics);
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
        Match(Tokenˉkind.Profile);
        var Profile = Current.Kind is Tokenˉkind.Portable or Tokenˉkind.Hosted or Tokenˉkind.System
            ? Nextˉtoken()
            : Match(Tokenˉkind.Portable);
        Match(Tokenˉkind.Semicolon);

        var Capabilities = ImmutableArray.CreateBuilder<Capabilityˉsyntax>();
        var Data = ImmutableArray.CreateBuilder<Dataˉsyntax>();
        var Functions = ImmutableArray.CreateBuilder<Functionˉsyntax>();

        while (Current.Kind != Tokenˉkind.End)
        {
            var Startˉposition = Position;
            switch (Current.Kind)
            {
                case Tokenˉkind.Capability:
                    Capabilities.Add(Parseˉcapability());
                    break;
                case Tokenˉkind.Data:
                    Data.Add(Parseˉdata());
                    break;
                case Tokenˉkind.Export:
                case Tokenˉkind.Fn:
                    Functions.Add(Parseˉfunction());
                    break;
                default:
                    Diagnostics.Report(
                        "WVC1100",
                        "parser",
                        Current.Span,
                        $"Expected a capability, data, or function declaration but found '{Current.Text}'.");
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
            Capabilities.ToImmutable(),
            Data.ToImmutable(),
            Functions.ToImmutable());
    }

    private Capabilityˉsyntax Parseˉcapability()
    {
        var Start = Match(Tokenˉkind.Capability);
        var Name = Parseˉqualifiedˉname(allowˉlength: false);
        var End = Match(Tokenˉkind.Semicolon);
        return new(Name.Name, Combine(Start.Span, End.Span));
    }

    private Dataˉsyntax Parseˉdata()
    {
        var Start = Match(Tokenˉkind.Data);
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
        else
        {
            Diagnostics.Report(
                "WVC1101",
                "parser",
                Type.Span,
                "Module data must have type text or [i32].");
            var Invalid = Current;
            Nextˉtoken();
            Value = new I32ˉarrayˉdataˉvalueˉsyntax([], Invalid.Span);
        }

        var End = Match(Tokenˉkind.Semicolon);
        return new(Name, Type, Value, Combine(Start.Span, End.Span));
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
            Tokenˉkind.Return => Parseˉreturn(),
            Tokenˉkind.Leftˉbrace => Parseˉblock(),
            Tokenˉkind.Identifier when Peek(1).Kind == Tokenˉkind.Equals => Parseˉassignment(),
            _ => Parseˉexpressionˉstatement(),
        };
    }

    private Localˉdeclarationˉstatementˉsyntax Parseˉlocalˉdeclaration()
    {
        var Start = Nextˉtoken();
        var Isˉmutable = Start.Kind == Tokenˉkind.Var;
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Colon);
        var Type = Parseˉtype(allowˉvoid: false, allowˉarray: false);
        Match(Tokenˉkind.Equals);
        var Initializer = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Isˉmutable, Name, Type, Initializer, Combine(Start.Span, End.Span));
    }

    private Assignmentˉstatementˉsyntax Parseˉassignment()
    {
        var Name = Match(Tokenˉkind.Identifier);
        Match(Tokenˉkind.Equals);
        var Value = Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Name, Value, Combine(Name.Span, End.Span));
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
            Else = Parseˉblock();
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

    private Returnˉstatementˉsyntax Parseˉreturn()
    {
        var Start = Match(Tokenˉkind.Return);
        var Value = Current.Kind == Tokenˉkind.Semicolon ? null : Parseˉexpression();
        var End = Match(Tokenˉkind.Semicolon);
        return new(Value, Combine(Start.Span, End.Span));
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
                return new Literalˉexpressionˉsyntax((int?)Integer.Value ?? 0, Integer.Span);
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

    private Expressionˉsyntax Parseˉnameˉorˉpostfix()
    {
        var Name = Parseˉqualifiedˉname(allowˉlength: true);
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

        var Token = Current;
        var Kind = Token.Kind switch
        {
            Tokenˉkind.I32 => Typeˉsyntaxˉkind.I32,
            Tokenˉkind.Bool => Typeˉsyntaxˉkind.Bool,
            Tokenˉkind.Text => Typeˉsyntaxˉkind.Text,
            Tokenˉkind.Void when allowˉvoid => Typeˉsyntaxˉkind.Void,
            _ => Typeˉsyntaxˉkind.Invalid,
        };

        if (Kind == Typeˉsyntaxˉkind.Invalid)
        {
            Diagnostics.Report(
                "WVC1103",
                "parser",
                Token.Span,
                allowˉvoid
                    ? "Expected type i32, bool, text, or void."
                    : "Expected type i32, bool, or text.");
            if (Current.Kind != Tokenˉkind.End)
            {
                Nextˉtoken();
            }

            return new(Typeˉsyntaxˉkind.Invalid, Token.Span);
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
        return new(expected, string.Empty, new(Current.Span.Start, 0, Current.Span.Line, Current.Span.Column));
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
        return kind is Tokenˉkind.Minus or Tokenˉkind.Bang ? 5 : 0;
    }

    private static int Getˉbinaryˉprecedence(Tokenˉkind kind)
    {
        return kind switch
        {
            Tokenˉkind.Equalsˉequals or Tokenˉkind.Bangˉequals => 1,
            Tokenˉkind.Less or Tokenˉkind.Lessˉequals or Tokenˉkind.Greater or Tokenˉkind.Greaterˉequals => 2,
            Tokenˉkind.Plus or Tokenˉkind.Minus => 3,
            Tokenˉkind.Star => 4,
            _ => 0,
        };
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
        return new(start.Start, Math.Max(0, end.Start + end.Length - start.Start), start.Line, start.Column);
    }
}
