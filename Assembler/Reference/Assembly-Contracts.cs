using System.Collections.Immutable;

namespace Windvale.Assembler;

public sealed record Assemblyˉdiagnostic(
    string Code,
    int Line,
    int Column,
    string Message);

public sealed class Assemblyˉresult
{
    private Assemblyˉresult(
        ImmutableArray<byte> objectˉbytes,
        ImmutableArray<Assemblyˉdiagnostic> diagnostics)
    {
        Objectˉbytes = objectˉbytes;
        Diagnostics = diagnostics;
    }

    public bool Success => Diagnostics.IsEmpty;

    public ImmutableArray<byte> Objectˉbytes { get; }

    public ImmutableArray<Assemblyˉdiagnostic> Diagnostics { get; }

    internal static Assemblyˉresult Succeeded(ImmutableArray<byte> objectˉbytes) =>
        new(objectˉbytes, []);

    internal static Assemblyˉresult Failed(Assemblyˉdiagnostic diagnostic) =>
        new([], [diagnostic]);
}

public static class Assemblyˉlimits
{
    public const int MAX_SOURCE_BYTES = 1024 * 1024;
    public const int MAX_LINE_BYTES = 4 * 1024;
    public const int MAX_BYTES_PER_STATEMENT = 4 * 1024;
}

internal readonly record struct Assemblyˉspan(int Line, int Column);

internal readonly record struct Assemblyˉregister(byte Index, byte Width)
{
    public bool Isˉextended => Index >= 8;
}

internal sealed record Assemblyˉunit(
    ImmutableArray<Assemblyˉsymbol> Symbols,
    ImmutableArray<Assemblyˉsection> Sections);

internal sealed record Assemblyˉsymbol(
    string Name,
    Windvale.ObjectModel.Objectˉsymbolˉbinding Binding,
    Windvale.ObjectModel.Objectˉsymbolˉkind Kind,
    string? Sectionˉname,
    Assemblyˉspan Span);

internal sealed record Assemblyˉsection(
    string Name,
    Windvale.ObjectModel.Objectˉsectionˉkind Kind,
    uint Alignment,
    ImmutableArray<Assemblyˉdefinition> Definitions,
    Assemblyˉspan Span);

internal sealed record Assemblyˉdefinition(
    string Name,
    ImmutableArray<Assemblyˉstatement> Statements,
    Assemblyˉspan Span);

internal enum Assemblyˉstatementˉkind
{
    Nop,
    Return,
    Trap,
    Call,
    Jump,
    Moveˉi32,
    Moveˉu32,
    Disableˉinterrupts,
    Halt,
    Outˉu16,
    Pushˉi32,
    Enableˉpageˉprotection,
    Activateˉpageˉtable,
    Syscall,
    Label,
    Jumpˉlabel,
    Branch,
    Moveˉregister,
    Add,
    Subtract,
    And,
    Or,
    Xor,
    Compare,
    Test,
    Pushˉregister,
    Popˉregister,
    Callˉregister,
    Jumpˉregister,
    Loadˉu32,
    Loadˉu64,
    Storeˉu32,
    Storeˉu64,
    Loadˉaddress,
    Bytes,
    U32,
    I32,
    Addressˉu32,
    Zero,
}

internal sealed record Assemblyˉstatement(
    Assemblyˉstatementˉkind Kind,
    string? Name,
    long Number,
    byte Register,
    ImmutableArray<byte> Bytes,
    Assemblyˉspan Span,
    Assemblyˉregister Firstˉregister = default,
    Assemblyˉregister Secondˉregister = default,
    byte Condition = 0);
