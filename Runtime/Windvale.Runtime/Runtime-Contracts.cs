using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Runtime;

public readonly record struct Runtimeˉvalue
{
    private Runtimeˉvalue(Valueˉtype type, int i32, bool boolean, string? text)
    {
        Type = type;
        I32ˉvalue = i32;
        Boolˉvalue = boolean;
        Textˉvalue = text;
    }

    public Valueˉtype Type { get; }

    public int I32ˉvalue { get; }

    public bool Boolˉvalue { get; }

    public string? Textˉvalue { get; }

    public static Runtimeˉvalue Fromˉi32(int value) => new(Valueˉtype.I32, value, false, null);

    public static Runtimeˉvalue Fromˉbool(bool value) => new(Valueˉtype.Bool, 0, value, null);

    public static Runtimeˉvalue Fromˉtext(string value) => new(Valueˉtype.Text, 0, false, value);

    public static Runtimeˉvalue Default(Valueˉtype type)
    {
        return type switch
        {
            Valueˉtype.I32 => Fromˉi32(0),
            Valueˉtype.Bool => Fromˉbool(false),
            Valueˉtype.Text => Fromˉtext(string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Void has no runtime value."),
        };
    }
}

public sealed record Runtimeˉoptions(
    ImmutableHashSet<string> Authorizedˉcapabilities,
    long Maximumˉinstructions = 1_000_000,
    int Maximumˉcallˉdepth = 1024)
{
    public static Runtimeˉoptions Portableˉdefaults { get; } = new(
        ImmutableHashSet.Create<string>(StringComparer.Ordinal));
}

public sealed record Runtimeˉresult(int Exitˉcode, long Executedˉinstructions);

public interface ICapabilityˉhost
{
    Runtimeˉvalue? Invoke(
        Capabilityˉdeclaration capability,
        ImmutableArray<Runtimeˉvalue> arguments);
}

public sealed class Referenceˉcapabilityˉhost(TextWriter output) : ICapabilityˉhost
{
    public Runtimeˉvalue? Invoke(
        Capabilityˉdeclaration capability,
        ImmutableArray<Runtimeˉvalue> arguments)
    {
        if (capability.Name == Capabilityˉcatalog.CONSOLE_WRITE_LINE)
        {
            output.WriteLine(arguments[0].Textˉvalue!);
            return null;
        }

        throw new Runtimeˉexception(
            "WVR3001",
            $"The host does not implement capability '{capability.Name}'.");
    }
}

public sealed class Runtimeˉexception : Exception
{
    public Runtimeˉexception(string code, string message)
        : base($"{code}: {message}")
    {
        Code = code;
    }

    public string Code { get; }
}
