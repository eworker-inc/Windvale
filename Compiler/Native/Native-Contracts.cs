using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉcontract
{
    public const int ABI_VERSION = 4;
    public const string X64_BASELINE_TARGET = "x86-64-wvb-baseline-v4";
    public const long DEFAULT_MAXIMUM_INSTRUCTIONS = 1_000_000;
    public const int MAXIMUM_CODE_BYTES = 1024 * 1024;
    public const int MAXIMUM_FRAME_SLOTS = 1024;
    public const int MAXIMUM_FRAME_BYTES = MAXIMUM_FRAME_SLOTS * sizeof(int);
    public const int MAXIMUM_BLOCKS = 4096;
}

public abstract record Nativeˉoperation;

public sealed record Nativeˉinstructionˉcharge : Nativeˉoperation;

public enum Nativeˉvalueˉtype : byte
{
    I32 = 1,
    Bool = 2,
}

public sealed record Nativeˉi32ˉconstant(int Result, int Value) : Nativeˉoperation;

public sealed record Nativeˉboolˉconstant(int Result, bool Value) : Nativeˉoperation;

public sealed record Nativeˉlocalˉload(
    int Result,
    int Local,
    Nativeˉvalueˉtype Type) : Nativeˉoperation;

public sealed record Nativeˉlocalˉstore(
    int Local,
    Nativeˉvalueˉtype Type,
    int Value) : Nativeˉoperation;

public enum Nativeˉi32ˉbinaryˉkind : byte
{
    Add = 1,
    Subtract = 2,
    Multiply = 3,
}

public sealed record Nativeˉi32ˉbinary(
    int Result,
    Nativeˉi32ˉbinaryˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public sealed record Nativeˉi32ˉnegate(int Result, int Value) : Nativeˉoperation;

public enum Nativeˉi32ˉcomparisonˉkind : byte
{
    Equal = 1,
    Notˉequal = 2,
    Less = 3,
    Lessˉequal = 4,
    Greater = 5,
    Greaterˉequal = 6,
}

public sealed record Nativeˉi32ˉcomparison(
    int Result,
    Nativeˉi32ˉcomparisonˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public enum Nativeˉboolˉcomparisonˉkind : byte
{
    Equal = 1,
    Notˉequal = 2,
}

public sealed record Nativeˉboolˉcomparison(
    int Result,
    Nativeˉboolˉcomparisonˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public sealed record Nativeˉboolˉnot(int Result, int Value) : Nativeˉoperation;

public abstract record Nativeˉterminator;

public sealed record Nativeˉjump(int Targetˉblock) : Nativeˉterminator;

public sealed record Nativeˉbranch(
    int Condition,
    int Trueˉblock,
    int Falseˉblock) : Nativeˉterminator;

public sealed record Nativeˉreturn(int Value) : Nativeˉterminator;

public sealed record Nativeˉblock(
    int Id,
    ImmutableArray<Nativeˉoperation> Operations,
    Nativeˉterminator Terminator);

public sealed record Nativeˉfunction(
    string Name,
    ImmutableArray<Nativeˉvalueˉtype> Localˉtypes,
    ImmutableArray<Nativeˉvalueˉtype> Valueˉtypes,
    ImmutableArray<Nativeˉblock> Blocks);

public sealed record Nativeˉmodule(ImmutableArray<Nativeˉfunction> Functions);

public enum Nativeˉsymbolˉbinding : byte
{
    Local = 1,
    Export = 2,
    Import = 3,
}

public enum Nativeˉsymbolˉkind : byte
{
    Function = 1,
    Data = 2,
}

public enum Nativeˉpatchˉkind : byte
{
    Absoluteˉu32 = 1,
    Relativeˉi32 = 2,
}

public sealed record Nativeˉsymbol(
    string Name,
    Nativeˉsymbolˉbinding Binding,
    Nativeˉsymbolˉkind Kind,
    uint Offset,
    uint Size);

public sealed record Nativeˉpatch(
    Nativeˉpatchˉkind Kind,
    uint Offset,
    string Symbol,
    int Addend);

public sealed record Nativeˉfragment(
    string Target,
    int Abiˉversion,
    Objectˉarchitecture Architecture,
    uint Alignment,
    ImmutableArray<byte> Code,
    ImmutableArray<Nativeˉsymbol> Symbols,
    ImmutableArray<Nativeˉpatch> Patches);

public sealed record Nativeˉcompilation(
    Nativeˉmodule Module,
    Nativeˉfragment Fragment);

public sealed class Nativeˉbackendˉexception(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
