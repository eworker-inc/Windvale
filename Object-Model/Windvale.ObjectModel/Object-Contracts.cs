using System.Collections.Immutable;

namespace Windvale.ObjectModel;

public enum Objectˉarchitecture : byte
{
    X86ˉ64 = 1,
}

public enum Objectˉadmissionˉprofile : byte
{
    Standard = 1,
    Largeˉnative = 2,
}

public enum Objectˉsectionˉkind : byte
{
    Code = 1,
    Readˉonlyˉdata = 2,
    Writableˉdata = 3,
    Zeroˉfill = 4,
}

public enum Objectˉsymbolˉbinding : byte
{
    Local = 1,
    Export = 2,
    Import = 3,
}

public enum Objectˉsymbolˉkind : byte
{
    Function = 1,
    Data = 2,
}

public enum Objectˉrelocationˉkind : byte
{
    Absoluteˉu32 = 1,
    Relativeˉi32 = 2,
}

public sealed record Objectˉsection(
    string Name,
    Objectˉsectionˉkind Kind,
    uint Alignment,
    uint Memoryˉsize,
    ImmutableArray<byte> Data);

public sealed record Objectˉsymbol(
    string Name,
    Objectˉsymbolˉbinding Binding,
    Objectˉsymbolˉkind Kind,
    uint Sectionˉindex,
    uint Offset,
    uint Size);

public sealed record Objectˉrelocation(
    Objectˉrelocationˉkind Kind,
    uint Sectionˉindex,
    uint Offset,
    uint Symbolˉindex,
    int Addend);

public sealed record Objectˉfile(
    Objectˉarchitecture Architecture,
    ImmutableArray<Objectˉsection> Sections,
    ImmutableArray<Objectˉsymbol> Symbols,
    ImmutableArray<Objectˉrelocation> Relocations);

public sealed class Verifiedˉobject
{
    internal Verifiedˉobject(Objectˉfile value)
    {
        Value = value;
    }

    public Objectˉfile Value { get; }
}

public static class Objectˉlimits
{
    public const int MAX_OBJECT_BYTES = 4 * 1024 * 1024;
    public const int LARGE_NATIVE_MAX_OBJECT_BYTES = 20 * 1024 * 1024;
    public const int MAX_SECTIONS = 64;
    public const int MAX_SYMBOLS = 4_096;
    public const int MAX_RELOCATIONS = 65_536;
    public const int MAX_NAME_BYTES = 255;
    public const uint MAX_ALIGNMENT = 4_096;
    public const uint MAX_MEMORY_BYTES = 16 * 1024 * 1024;
    public const uint LARGE_NATIVE_MAX_MEMORY_BYTES = 20 * 1024 * 1024;
    public const uint UNDEFINED_SECTION = uint.MaxValue;

    public static int Maximumˉobjectˉbytes(Objectˉadmissionˉprofile profile) => profile switch
    {
        Objectˉadmissionˉprofile.Standard => MAX_OBJECT_BYTES,
        Objectˉadmissionˉprofile.Largeˉnative => LARGE_NATIVE_MAX_OBJECT_BYTES,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };

    public static uint Maximumˉmemoryˉbytes(Objectˉadmissionˉprofile profile) => profile switch
    {
        Objectˉadmissionˉprofile.Standard => MAX_MEMORY_BYTES,
        Objectˉadmissionˉprofile.Largeˉnative => LARGE_NATIVE_MAX_MEMORY_BYTES,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };
}
