using System.Collections.Immutable;

namespace Windvale.Bytecode;

public static class Capabilityˉcatalog
{
    public const string CONSOLE_WRITE_LINE = "console.write_line";

    private static readonly ImmutableDictionary<string, Capabilityˉdeclaration> DECLARATIONS =
        new Dictionary<string, Capabilityˉdeclaration>(StringComparer.Ordinal)
        {
            [CONSOLE_WRITE_LINE] = new(
                CONSOLE_WRITE_LINE,
                [Valueˉtype.Text],
                Valueˉtype.Void),
        }.ToImmutableDictionary(StringComparer.Ordinal);

    public static bool Tryˉget(string name, out Capabilityˉdeclaration declaration)
    {
        return DECLARATIONS.TryGetValue(name, out declaration!);
    }
}
