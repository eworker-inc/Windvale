using System.Text.RegularExpressions;

namespace Windvale.Bytecode;

public static partial class Seedˉnames
{
    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex Identifierˉpattern();

    [GeneratedRegex("^[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+$", RegexOptions.CultureInvariant)]
    private static partial Regex Capabilityˉpattern();

    public static bool Isˉidentifier(string value)
    {
        return Identifierˉpattern().IsMatch(value);
    }

    public static bool Isˉcapability(string value)
    {
        return Capabilityˉpattern().IsMatch(value);
    }
}
