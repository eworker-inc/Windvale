using System.Globalization;
using System.Text;

namespace Windvale.Compiler;

internal sealed class Sourceˉlexer(
    string source,
    string sourceˉname,
    Diagnosticˉbag diagnostics)
{
    private static readonly IReadOnlyDictionary<string, Tokenˉkind> KEYWORDS =
        new Dictionary<string, Tokenˉkind>(StringComparer.Ordinal)
        {
            ["module"] = Tokenˉkind.Module,
            ["profile"] = Tokenˉkind.Profile,
            ["portable"] = Tokenˉkind.Portable,
            ["hosted"] = Tokenˉkind.Hosted,
            ["system"] = Tokenˉkind.System,
            ["import"] = Tokenˉkind.Import,
            ["capability"] = Tokenˉkind.Capability,
            ["data"] = Tokenˉkind.Data,
            ["record"] = Tokenˉkind.Record,
            ["enum"] = Tokenˉkind.Enum,
            ["export"] = Tokenˉkind.Export,
            ["fn"] = Tokenˉkind.Fn,
            ["let"] = Tokenˉkind.Let,
            ["var"] = Tokenˉkind.Var,
            ["if"] = Tokenˉkind.If,
            ["else"] = Tokenˉkind.Else,
            ["while"] = Tokenˉkind.While,
            ["return"] = Tokenˉkind.Return,
            ["true"] = Tokenˉkind.True,
            ["false"] = Tokenˉkind.False,
            ["i32"] = Tokenˉkind.I32,
            ["u8"] = Tokenˉkind.U8,
            ["u32"] = Tokenˉkind.U32,
            ["bool"] = Tokenˉkind.Bool,
            ["text"] = Tokenˉkind.Text,
            ["bytes"] = Tokenˉkind.Bytes,
            ["void"] = Tokenˉkind.Void,
            ["length"] = Tokenˉkind.Length,
        };

    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    private int Position;
    private int Line = 1;
    private int Column = 1;

    public Syntaxˉtoken Lex()
    {
        Skipˉtrivia();
        var Start = Position;
        var Startˉline = Line;
        var Startˉcolumn = Column;

        if (Isˉend)
        {
            return Token(Tokenˉkind.End, Start, Startˉline, Startˉcolumn);
        }

        if (Isˉidentifierˉstart(Current))
        {
            Advance();
            while (!Isˉend && Isˉidentifierˉpart(Current))
            {
                Advance();
            }

            var Text = source[Start..Position];
            var Kind = KEYWORDS.GetValueOrDefault(Text, Tokenˉkind.Identifier);
            return Token(Kind, Start, Startˉline, Startˉcolumn);
        }

        if (char.IsAsciiDigit(Current))
        {
            Advance();
            while (!Isˉend && char.IsAsciiDigit(Current))
            {
                Advance();
            }

            var Digitˉend = Position;
            if (Hasˉnumericˉsuffix("u8"))
            {
                Advance();
                Advance();
                var Digits = source[Start..Digitˉend];
                if (!uint.TryParse(Digits, NumberStyles.None, CultureInfo.InvariantCulture, out var Rawˉu8) ||
                    Rawˉu8 > byte.MaxValue)
                {
                    diagnostics.Report(
                        "WVC1001",
                        "lexer",
                        Span(Start, Startˉline, Startˉcolumn),
                        "The decimal u8 literal is outside the range 0 through 255.");
                    return Token(Tokenˉkind.Integer, Start, Startˉline, Startˉcolumn, (byte)0);
                }

                return Token(Tokenˉkind.Integer, Start, Startˉline, Startˉcolumn, (byte)Rawˉu8);
            }

            if (Hasˉnumericˉsuffix("u32"))
            {
                Advance();
                Advance();
                Advance();
                var Digits = source[Start..Digitˉend];
                if (!uint.TryParse(Digits, NumberStyles.None, CultureInfo.InvariantCulture, out var Rawˉu32))
                {
                    diagnostics.Report(
                        "WVC1001",
                        "lexer",
                        Span(Start, Startˉline, Startˉcolumn),
                        "The decimal u32 literal is outside the range 0 through 4294967295.");
                    return Token(Tokenˉkind.Integer, Start, Startˉline, Startˉcolumn, 0U);
                }

                return Token(Tokenˉkind.Integer, Start, Startˉline, Startˉcolumn, Rawˉu32);
            }

            var Text = source[Start..Digitˉend];
            if (!long.TryParse(Text, NumberStyles.None, CultureInfo.InvariantCulture, out var Rawˉvalue) ||
                Rawˉvalue > int.MaxValue)
            {
                diagnostics.Report(
                    "WVC1001",
                    "lexer",
                    Span(Start, Startˉline, Startˉcolumn),
                    "The decimal integer literal is outside the positive i32 range.");
                return Token(Tokenˉkind.Integer, Start, Startˉline, Startˉcolumn, 0);
            }

            return Token(Tokenˉkind.Integer, Start, Startˉline, Startˉcolumn, (int)Rawˉvalue);
        }

        if (Current == '"')
        {
            return Lexˉstring(Start, Startˉline, Startˉcolumn);
        }

        switch (Current)
        {
            case '(':
                Advance();
                return Token(Tokenˉkind.Leftˉparenthesis, Start, Startˉline, Startˉcolumn);
            case ')':
                Advance();
                return Token(Tokenˉkind.Rightˉparenthesis, Start, Startˉline, Startˉcolumn);
            case '{':
                Advance();
                return Token(Tokenˉkind.Leftˉbrace, Start, Startˉline, Startˉcolumn);
            case '}':
                Advance();
                return Token(Tokenˉkind.Rightˉbrace, Start, Startˉline, Startˉcolumn);
            case '[':
                Advance();
                return Token(Tokenˉkind.Leftˉbracket, Start, Startˉline, Startˉcolumn);
            case ']':
                Advance();
                return Token(Tokenˉkind.Rightˉbracket, Start, Startˉline, Startˉcolumn);
            case ';':
                Advance();
                return Token(Tokenˉkind.Semicolon, Start, Startˉline, Startˉcolumn);
            case ':':
                Advance();
                return Token(Tokenˉkind.Colon, Start, Startˉline, Startˉcolumn);
            case ',':
                Advance();
                return Token(Tokenˉkind.Comma, Start, Startˉline, Startˉcolumn);
            case '.':
                Advance();
                return Token(Tokenˉkind.Dot, Start, Startˉline, Startˉcolumn);
            case '+':
                Advance();
                return Token(Tokenˉkind.Plus, Start, Startˉline, Startˉcolumn);
            case '*':
                Advance();
                return Token(Tokenˉkind.Star, Start, Startˉline, Startˉcolumn);
            case '-':
                Advance();
                if (Current == '>')
                {
                    Advance();
                    return Token(Tokenˉkind.Arrow, Start, Startˉline, Startˉcolumn);
                }

                return Token(Tokenˉkind.Minus, Start, Startˉline, Startˉcolumn);
            case '!':
                Advance();
                if (Current == '=')
                {
                    Advance();
                    return Token(Tokenˉkind.Bangˉequals, Start, Startˉline, Startˉcolumn);
                }

                return Token(Tokenˉkind.Bang, Start, Startˉline, Startˉcolumn);
            case '=':
                Advance();
                if (Current == '=')
                {
                    Advance();
                    return Token(Tokenˉkind.Equalsˉequals, Start, Startˉline, Startˉcolumn);
                }

                return Token(Tokenˉkind.Equals, Start, Startˉline, Startˉcolumn);
            case '<':
                Advance();
                if (Current == '=')
                {
                    Advance();
                    return Token(Tokenˉkind.Lessˉequals, Start, Startˉline, Startˉcolumn);
                }

                return Token(Tokenˉkind.Less, Start, Startˉline, Startˉcolumn);
            case '>':
                Advance();
                if (Current == '=')
                {
                    Advance();
                    return Token(Tokenˉkind.Greaterˉequals, Start, Startˉline, Startˉcolumn);
                }

                return Token(Tokenˉkind.Greater, Start, Startˉline, Startˉcolumn);
            default:
                var Badˉcharacter = Current;
                Advance();
                diagnostics.Report(
                    "WVC1002",
                    "lexer",
                    Span(Start, Startˉline, Startˉcolumn),
                    $"Unexpected character U+{(int)Badˉcharacter:X4}.");
                return Token(Tokenˉkind.Bad, Start, Startˉline, Startˉcolumn);
        }
    }

    private Syntaxˉtoken Lexˉstring(int start, int startˉline, int startˉcolumn)
    {
        var Value = new StringBuilder();
        Advance();
        var Terminated = false;

        while (!Isˉend)
        {
            if (Current == '"')
            {
                Advance();
                Terminated = true;
                break;
            }

            if (Current is '\r' or '\n')
            {
                break;
            }

            if (Current != '\\')
            {
                Value.Append(Current);
                Advance();
                continue;
            }

            Advance();
            if (Isˉend)
            {
                break;
            }

            switch (Current)
            {
                case '"':
                    Value.Append('"');
                    Advance();
                    break;
                case '\\':
                    Value.Append('\\');
                    Advance();
                    break;
                case 'n':
                    Value.Append('\n');
                    Advance();
                    break;
                case 'r':
                    Value.Append('\r');
                    Advance();
                    break;
                case 't':
                    Value.Append('\t');
                    Advance();
                    break;
                case 'u':
                    Advance();
                    Lexˉunicodeˉescape(Value, start, startˉline, startˉcolumn);
                    break;
                default:
                    diagnostics.Report(
                        "WVC1003",
                        "lexer",
                        Span(Position - 1, Line, Math.Max(1, Column - 1)),
                        $"Unsupported string escape '\\{Current}'.");
                    Value.Append(Current);
                    Advance();
                    break;
            }
        }

        if (!Terminated)
        {
            diagnostics.Report(
                "WVC1004",
                "lexer",
                Span(start, startˉline, startˉcolumn),
                "The string literal is not terminated.");
        }

        try
        {
            _ = STRICT_UTF8.GetByteCount(Value.ToString());
        }
        catch (EncoderFallbackException)
        {
            diagnostics.Report(
                "WVC1005",
                "lexer",
                Span(start, startˉline, startˉcolumn),
                "The string literal contains an unpaired Unicode surrogate.");
        }

        return Token(Tokenˉkind.String, start, startˉline, startˉcolumn, Value.ToString());
    }

    private void Lexˉunicodeˉescape(
        StringBuilder value,
        int stringˉstart,
        int stringˉline,
        int stringˉcolumn)
    {
        if (source.Length - Position < 4)
        {
            diagnostics.Report(
                "WVC1006",
                "lexer",
                Span(stringˉstart, stringˉline, stringˉcolumn),
                "A Unicode escape requires four hexadecimal digits.");
            Position = source.Length;
            return;
        }

        var Digits = source.AsSpan(Position, 4);
        if (!ushort.TryParse(Digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var Codeˉunit))
        {
            diagnostics.Report(
                "WVC1007",
                "lexer",
                new(Position, 4, Line, Column, sourceˉname),
                "A Unicode escape contains non-hexadecimal digits.");
            for (var Index = 0; Index < 4; Index++)
            {
                Advance();
            }

            return;
        }

        value.Append((char)Codeˉunit);
        for (var Index = 0; Index < 4; Index++)
        {
            Advance();
        }
    }

    private void Skipˉtrivia()
    {
        while (!Isˉend)
        {
            if (char.IsWhiteSpace(Current))
            {
                Advance();
                continue;
            }

            if (Current == '/' && Peek(1) == '/')
            {
                while (!Isˉend && Current != '\n')
                {
                    Advance();
                }

                continue;
            }

            break;
        }
    }

    private Syntaxˉtoken Token(
        Tokenˉkind kind,
        int start,
        int line,
        int column,
        object? value = null)
    {
        return new(kind, source[start..Position], Span(start, line, column), value);
    }

    private Sourceˉspan Span(int start, int line, int column)
    {
        return new(start, Position - start, line, column, sourceˉname);
    }

    private char Current => Isˉend ? '\0' : source[Position];

    private bool Isˉend => Position >= source.Length;

    private char Peek(int distance)
    {
        var Index = Position + distance;
        return Index >= source.Length ? '\0' : source[Index];
    }

    private void Advance()
    {
        if (Isˉend)
        {
            return;
        }

        if (source[Position] == '\n')
        {
            Line++;
            Column = 1;
        }
        else
        {
            Column++;
        }

        Position++;
    }

    private static bool Isˉidentifierˉstart(char value)
    {
        return char.IsAsciiLetter(value) || value == '_';
    }

    private static bool Isˉidentifierˉpart(char value)
    {
        return Isˉidentifierˉstart(value) || char.IsAsciiDigit(value) || value == '\u02C9';
    }

    private bool Hasˉnumericˉsuffix(string suffix)
    {
        if (Position + suffix.Length > source.Length ||
            !source.AsSpan(Position, suffix.Length).SequenceEqual(suffix.AsSpan()))
        {
            return false;
        }

        var Followingˉposition = Position + suffix.Length;
        return Followingˉposition == source.Length || !Isˉidentifierˉpart(source[Followingˉposition]);
    }
}
