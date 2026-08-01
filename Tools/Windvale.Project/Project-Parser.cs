using System.Collections.Immutable;
using System.Text;

namespace Windvale.Project;

public static class Projectˉparser
{
    private const string HEADER = "windvale-project 1";
    private const string ROOT_PREFIX = "root \"";
    private const string SOURCE_PREFIX = "source \"";
    private const string EMIT_WVB = "emit wvb";

    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Projectˉparseˉresult Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            if (STRICT_UTF8.GetByteCount(text) > Projectˉlimits.MAX_MANIFEST_BYTES)
            {
                return Invalid(
                    "WVP1002",
                    1,
                    1,
                    $"The project manifest exceeds the {Projectˉlimits.MAX_MANIFEST_BYTES} byte limit.");
            }
        }
        catch (EncoderFallbackException)
        {
            return Invalid("WVP1002", 1, 1, "The project manifest is not valid strict UTF-8 text.");
        }

        var Lines = text.Split('\n');
        if (!Tryˉreadˉline(
            Lines[0],
            1,
            Lines.Length > 1,
            out var Header,
            out var Headerˉfailure))
        {
            return Headerˉfailure;
        }
        if (!string.Equals(Header, HEADER, StringComparison.Ordinal))
        {
            return Invalid("WVP1001", 1, 1, $"The project header must be exactly '{HEADER}'.");
        }

        Projectˉsourceˉpath? Root = null;
        var Sources = ImmutableArray.CreateBuilder<Projectˉsourceˉpath>();
        var Hasˉemission = false;

        for (var Index = 1; Index < Lines.Length; Index++)
        {
            var Lineˉnumber = Index + 1;
            if (!Tryˉreadˉline(
                Lines[Index],
                Lineˉnumber,
                Index < Lines.Length - 1,
                out var Line,
                out var Lineˉfailure))
            {
                return Lineˉfailure;
            }
            if (Line.Length == 0)
            {
                continue;
            }

            if (Line.StartsWith(ROOT_PREFIX, StringComparison.Ordinal))
            {
                if (Root is not null)
                {
                    return Invalid("WVP1004", Lineˉnumber, 1, "The project repeats its root directive.");
                }
                if (!Tryˉreadˉpath(Line, ROOT_PREFIX, Lineˉnumber, out var Path, out var Pathˉfailure))
                {
                    return Pathˉfailure;
                }

                Root = Path;
                continue;
            }

            if (Line.StartsWith(SOURCE_PREFIX, StringComparison.Ordinal))
            {
                if (Sources.Count >= Projectˉlimits.MAX_SOURCE_MODULES - 1)
                {
                    return Invalid(
                        "WVP1005",
                        Lineˉnumber,
                        1,
                        $"A project may contain at most {Projectˉlimits.MAX_SOURCE_MODULES} source modules.");
                }
                if (!Tryˉreadˉpath(Line, SOURCE_PREFIX, Lineˉnumber, out var Path, out var Pathˉfailure))
                {
                    return Pathˉfailure;
                }

                Sources.Add(Path!);
                continue;
            }

            if (string.Equals(Line, EMIT_WVB, StringComparison.Ordinal))
            {
                if (Hasˉemission)
                {
                    return Invalid("WVP1004", Lineˉnumber, 1, "The project repeats its emit directive.");
                }

                Hasˉemission = true;
                continue;
            }

            return Invalid("WVP1003", Lineˉnumber, 1, $"Unknown or malformed project directive '{Line}'.");
        }

        var Endˉline = Lines.Length;
        if (Root is null)
        {
            return Invalid("WVP1004", Endˉline, 1, "The project is missing its root directive.");
        }
        if (!Hasˉemission)
        {
            return Invalid("WVP1004", Endˉline, 1, "The project is missing its emit directive.");
        }

        return new(
            new(Root, Sources.ToImmutable(), Projectˉemissionˉkind.Wvb),
            []);
    }

    private static bool Tryˉreadˉline(
        string input,
        int lineˉnumber,
        bool followedˉbyˉlineˉfeed,
        out string line,
        out Projectˉparseˉresult failure)
    {
        if (input.EndsWith('\r'))
        {
            if (!followedˉbyˉlineˉfeed)
            {
                line = string.Empty;
                failure = Invalid(
                    "WVP1003",
                    lineˉnumber,
                    input.Length,
                    "Project lines must use LF or CRLF line endings.");
                return false;
            }
            input = input[..^1];
        }
        var Carriageˉreturn = input.IndexOf('\r');
        if (Carriageˉreturn >= 0)
        {
            line = string.Empty;
            failure = Invalid(
                "WVP1003",
                lineˉnumber,
                Carriageˉreturn + 1,
                "Project lines must use LF or CRLF line endings.");
            return false;
        }
        if (input.Length != 0 && string.IsNullOrWhiteSpace(input))
        {
            line = string.Empty;
            failure = Invalid(
                "WVP1003",
                lineˉnumber,
                1,
                "Whitespace-only project lines are noncanonical.");
            return false;
        }

        line = input;
        failure = null!;
        return true;
    }

    private static bool Tryˉreadˉpath(
        string line,
        string prefix,
        int lineˉnumber,
        out Projectˉsourceˉpath? path,
        out Projectˉparseˉresult failure)
    {
        if (!line.EndsWith('"') || line.Length <= prefix.Length)
        {
            path = null;
            failure = Invalid("WVP1003", lineˉnumber, 1, "A project path must be one quoted value.");
            return false;
        }

        var Value = line[prefix.Length..^1];
        var Quote = Value.IndexOf('"');
        if (Quote >= 0)
        {
            path = null;
            failure = Invalid(
                "WVP1003",
                lineˉnumber,
                prefix.Length + Quote + 1,
                "Project path escapes and trailing tokens are not supported in version 1.");
            return false;
        }
        if (!Tryˉvalidateˉpath(Value, out var Message, out var Columnˉoffset))
        {
            path = null;
            failure = Invalid("WVP1006", lineˉnumber, prefix.Length + Columnˉoffset + 1, Message);
            return false;
        }

        path = new(Value, lineˉnumber, prefix.Length + 1);
        failure = null!;
        return true;
    }

    private static bool Tryˉvalidateˉpath(
        string value,
        out string message,
        out int columnˉoffset)
    {
        if (value.Length == 0)
        {
            message = "A project path cannot be empty.";
            columnˉoffset = 0;
            return false;
        }

        try
        {
            if (STRICT_UTF8.GetByteCount(value) > Projectˉlimits.MAX_PATH_BYTES)
            {
                message = $"A project path exceeds the {Projectˉlimits.MAX_PATH_BYTES} byte limit.";
                columnˉoffset = 0;
                return false;
            }
        }
        catch (EncoderFallbackException)
        {
            message = "A project path is not valid strict UTF-8 text.";
            columnˉoffset = 0;
            return false;
        }

        if (!value.EndsWith(".wv", StringComparison.Ordinal))
        {
            message = "Every project source path must end in lowercase .wv.";
            columnˉoffset = Math.Max(0, value.Length - 1);
            return false;
        }

        var Segments = value.Split('/');
        var Offset = 0;
        foreach (var Segment in Segments)
        {
            if (Segment.Length == 0 ||
                !Isˉasciiˉalphanumeric(Segment[0]) ||
                !Isˉasciiˉalphanumeric(Segment[^1]))
            {
                message = "Every project path segment must begin and end with an ASCII letter or digit.";
                columnˉoffset = Offset;
                return false;
            }
            for (var Index = 0; Index < Segment.Length; Index++)
            {
                var Character = Segment[Index];
                if (!Isˉasciiˉalphanumeric(Character) && Character is not ('.' or '_' or '-'))
                {
                    message = "A project path contains a noncanonical character.";
                    columnˉoffset = Offset + Index;
                    return false;
                }
            }
            Offset += Segment.Length + 1;
        }

        message = string.Empty;
        columnˉoffset = 0;
        return true;
    }

    private static bool Isˉasciiˉalphanumeric(char value)
    {
        return value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9';
    }

    private static Projectˉparseˉresult Invalid(
        string code,
        int line,
        int column,
        string message)
    {
        return new(null, [new(code, line, column, message)]);
    }
}
