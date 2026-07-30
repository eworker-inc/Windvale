using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Runtime;

public readonly struct Runtimeˉbyteˉslice
{
    internal Runtimeˉbyteˉslice(ImmutableArray<byte> storage, int offset, int length)
    {
        if (storage.IsDefault || offset < 0 || length < 0 || length > storage.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        Storage = storage;
        Offset = offset;
        Length = length;
    }

    internal ImmutableArray<byte> Storage { get; }

    internal int Offset { get; }

    public int Length { get; }

    internal ReadOnlySpan<byte> Asˉspan()
    {
        return Storage.AsSpan(Offset, Length);
    }
}

public readonly record struct Runtimeˉvalue
{
    private Runtimeˉvalue(
        Valueˉtype type,
        int i32,
        bool boolean,
        string? text,
        byte u8,
        uint u32,
        Runtimeˉbyteˉslice bytes)
    {
        Type = type;
        I32ˉvalue = i32;
        Boolˉvalue = boolean;
        Textˉvalue = text;
        U8ˉvalue = u8;
        U32ˉvalue = u32;
        Bytesˉvalue = bytes;
    }

    public Valueˉtype Type { get; }

    public int I32ˉvalue { get; }

    public bool Boolˉvalue { get; }

    public string? Textˉvalue { get; }

    public byte U8ˉvalue { get; }

    public uint U32ˉvalue { get; }

    public Runtimeˉbyteˉslice Bytesˉvalue { get; }

    public static Runtimeˉvalue Fromˉi32(int value) =>
        new(Valueˉtype.I32, value, false, null, 0, 0, default);

    public static Runtimeˉvalue Fromˉbool(bool value) =>
        new(Valueˉtype.Bool, 0, value, null, 0, 0, default);

    public static Runtimeˉvalue Fromˉtext(string value) =>
        new(Valueˉtype.Text, 0, false, value, 0, 0, default);

    public static Runtimeˉvalue Fromˉu8(byte value) =>
        new(Valueˉtype.U8, 0, false, null, value, 0, default);

    public static Runtimeˉvalue Fromˉu32(uint value) =>
        new(Valueˉtype.U32, 0, false, null, 0, value, default);

    public static Runtimeˉvalue Fromˉbytes(ImmutableArray<byte> values) =>
        Fromˉbytes(new Runtimeˉbyteˉslice(values, 0, values.Length));

    public static Runtimeˉvalue Fromˉbytes(Runtimeˉbyteˉslice value) =>
        new(Valueˉtype.Bytes, 0, false, null, 0, 0, value);

    public static Runtimeˉvalue Default(Valueˉtype type)
    {
        return type switch
        {
            Valueˉtype.I32 => Fromˉi32(0),
            Valueˉtype.Bool => Fromˉbool(false),
            Valueˉtype.Text => Fromˉtext(string.Empty),
            Valueˉtype.U8 => Fromˉu8(0),
            Valueˉtype.U32 => Fromˉu32(0),
            Valueˉtype.Bytes => Fromˉbytes(ImmutableArray<byte>.Empty),
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
