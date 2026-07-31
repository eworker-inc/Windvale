using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉcontract
{
    public const int ABI_VERSION = 1;
    public const string X64_BASELINE_TARGET = "x86-64-wvb-baseline-v1";
    public const int MAXIMUM_CODE_BYTES = 1024 * 1024;
}

public abstract record Nativeˉoperation;

public sealed record Nativeˉi32ˉconstant(int Result, int Value) : Nativeˉoperation;

public sealed record Nativeˉreturn(int Value) : Nativeˉoperation;

public sealed record Nativeˉfunction(
    string Name,
    ImmutableArray<Nativeˉoperation> Operations);

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
