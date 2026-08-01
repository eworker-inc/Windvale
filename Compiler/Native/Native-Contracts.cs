using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉcontract
{
    public const int ABI_VERSION = 11;
    public const string X64_BASELINE_TARGET = "x86-64-wvb-baseline-v11";
    public const long DEFAULT_MAXIMUM_INSTRUCTIONS = 1_000_000;
    public const int DEFAULT_MAXIMUM_CALL_DEPTH = 1024;
    public const int MAXIMUM_CODE_BYTES = 1024 * 1024;
    public const int MAXIMUM_FRAME_SLOTS = 1024;
    public const int VALUE_SLOT_BYTES = 16;
    public const int BORROWED_BYTES_POINTER_OFFSET = 0;
    public const int BORROWED_BYTES_LENGTH_OFFSET = 8;
    public const int BORROWED_BYTES_RESERVED_OFFSET = 12;
    public const int BORROWED_TEXT_POINTER_OFFSET = 0;
    public const int BORROWED_TEXT_LENGTH_OFFSET = 8;
    public const int BORROWED_TEXT_RESERVED_OFFSET = 12;
    public const int MAXIMUM_FRAME_BYTES = MAXIMUM_FRAME_SLOTS * VALUE_SLOT_BYTES;
    public const int MAXIMUM_BLOCKS = 4096;
    public const int MAXIMUM_CALL_PARAMETERS = 4;
    public const int MAXIMUM_RECORD_ARENA_BYTES = 1024 * 1024;
    public const int MAXIMUM_TEXT_ARENA_BYTES = 16 * 1024 * 1024;
    public const int MAXIMUM_ENUM_METADATA_BYTES = 32 * 1024 * 1024;
}

public static class Nativeˉexecutionˉcontextˉcontract
{
    public const uint FORMAT_VERSION = 3;
    public const uint SIZE = 72;
    public const int FORMAT_VERSION_OFFSET = 0;
    public const int SIZE_OFFSET = 4;
    public const int INSTRUCTION_BUDGET_OFFSET = 8;
    public const int CALL_DEPTH_BUDGET_OFFSET = 16;
    public const int SERVICE_TABLE_POINTER_OFFSET = 24;
    public const int RECORD_ARENA_POINTER_OFFSET = 32;
    public const int RECORD_ARENA_LENGTH_OFFSET = 40;
    public const int RECORD_ARENA_USED_OFFSET = 44;
    public const int TEXT_ARENA_POINTER_OFFSET = 48;
    public const int TEXT_ARENA_LENGTH_OFFSET = 56;
    public const int TEXT_ARENA_USED_OFFSET = 60;
    public const int SERVICE_FAILURE_DETAIL_OFFSET = 64;
    public const int RESERVED_OFFSET = 68;
}

public enum Nativeˉserviceˉfailureˉdetail : uint
{
    None = 0,
    Textˉvalueˉlimit = 1,
    Textˉarenaˉexhausted = 2,
}

public static class Nativeˉserviceˉtableˉcontract
{
    public const uint FORMAT_VERSION = 4;
    public const uint SIZE = 96;
    public const int FORMAT_VERSION_OFFSET = 0;
    public const int SIZE_OFFSET = 4;
    public const int CONSOLE_WRITE_LINE_POINTER_OFFSET = 8;
    public const int PROCESS_ARGUMENT_COUNT_POINTER_OFFSET = 16;
    public const int PROCESS_ARGUMENT_POINTER_OFFSET = 24;
    public const int FILE_READ_BYTES_POINTER_OFFSET = 32;
    public const int TEXT_UTF8_IS_VALID_POINTER_OFFSET = 40;
    public const int DIAGNOSTIC_WRITE_LINE_POINTER_OFFSET = 48;
    public const int ENUM_NAME_POINTER_OFFSET = 56;
    public const int TEXT_CONCAT_POINTER_OFFSET = 64;
    public const int TEXT_QUOTE_POINTER_OFFSET = 72;
    public const int I32_FORMAT_POINTER_OFFSET = 80;
    public const int U32_FORMAT_POINTER_OFFSET = 88;
}

public enum Nativeˉservice : byte
{
    Consoleˉwriteˉline = 1,
    Processˉargumentˉcount = 2,
    Processˉargument = 3,
    Fileˉreadˉbytes = 4,
    Textˉutf8ˉisˉvalid = 5,
    Diagnosticˉwriteˉline = 6,
    Enumˉname = 7,
    Textˉconcat = 8,
    Textˉquote = 9,
    I32ˉformat = 10,
    U32ˉformat = 11,
}

public abstract record Nativeˉoperation;

public sealed record Nativeˉinstructionˉcharge : Nativeˉoperation;

public enum Nativeˉvalueˉtype : byte
{
    Void = 0,
    I32 = 1,
    Bool = 2,
    Borrowedˉtext = 3,
    U8 = 4,
    U32 = 5,
    Borrowedˉbytes = 6,
    Enum = 7,
    Record = 8,
}

public sealed record Nativeˉi32ˉconstant(int Result, int Value) : Nativeˉoperation;

public sealed record Nativeˉboolˉconstant(int Result, bool Value) : Nativeˉoperation;

public sealed record Nativeˉu8ˉconstant(int Result, byte Value) : Nativeˉoperation;

public sealed record Nativeˉu32ˉconstant(int Result, uint Value) : Nativeˉoperation;

public sealed record Nativeˉenumˉconstant(
    int Result,
    int Type,
    int Member,
    int Value) : Nativeˉoperation;

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

public enum Nativeˉu32ˉbinaryˉkind : byte
{
    Add = 1,
    Subtract = 2,
    Multiply = 3,
}

public sealed record Nativeˉu32ˉbinary(
    int Result,
    Nativeˉu32ˉbinaryˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public enum Nativeˉu32ˉcomparisonˉkind : byte
{
    Equal = 1,
    Notˉequal = 2,
    Less = 3,
    Lessˉequal = 4,
    Greater = 5,
    Greaterˉequal = 6,
}

public sealed record Nativeˉu32ˉcomparison(
    int Result,
    Nativeˉu32ˉcomparisonˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public enum Nativeˉu8ˉcomparisonˉkind : byte
{
    Equal = 1,
    Notˉequal = 2,
}

public sealed record Nativeˉu8ˉcomparison(
    int Result,
    Nativeˉu8ˉcomparisonˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public enum Nativeˉenumˉcomparisonˉkind : byte
{
    Equal = 1,
    Notˉequal = 2,
}

public sealed record Nativeˉenumˉcomparison(
    int Result,
    Nativeˉenumˉcomparisonˉkind Kind,
    int Left,
    int Right) : Nativeˉoperation;

public sealed record Nativeˉu32ˉfromˉu8(int Result, int Value) : Nativeˉoperation;

public sealed record Nativeˉcall(
    int Result,
    Nativeˉvalueˉtype Type,
    int Function,
    ImmutableArray<int> Arguments) : Nativeˉoperation;

public sealed record Nativeˉdataˉlength(
    int Result,
    int Data,
    int Length) : Nativeˉoperation;

public sealed record Nativeˉdataˉloadˉi32(
    int Result,
    int Data,
    int Index) : Nativeˉoperation;

public sealed record Nativeˉstaticˉtextˉconstant(
    int Result,
    int Data) : Nativeˉoperation;

public sealed record Nativeˉstaticˉbytesˉconstant(
    int Result,
    int Data) : Nativeˉoperation;

public sealed record Nativeˉbytesˉlength(
    int Result,
    int Bytes) : Nativeˉoperation;

public sealed record Nativeˉbytesˉslice(
    int Result,
    int Bytes,
    int Offset,
    int Length) : Nativeˉoperation;

public enum Nativeˉbytesˉreadˉkind : byte
{
    U8 = 1,
    U16ˉlittle = 2,
    U32ˉlittle = 3,
    I32ˉlittle = 4,
}

public sealed record Nativeˉbytesˉread(
    int Result,
    Nativeˉbytesˉreadˉkind Kind,
    int Bytes,
    int Offset) : Nativeˉoperation;

public sealed record Nativeˉtextˉutf8ˉisˉvalid(
    int Result,
    int Bytes) : Nativeˉoperation;

public sealed record Nativeˉtextˉfromˉutf8(
    int Result,
    int Bytes) : Nativeˉoperation;

public sealed record Nativeˉenumˉname(
    int Result,
    int Type,
    int Value) : Nativeˉoperation;

public enum Nativeˉintegerˉformatˉkind : byte
{
    I32 = 1,
    U8 = 2,
    U32 = 3,
}

public sealed record Nativeˉintegerˉformat(
    int Result,
    Nativeˉintegerˉformatˉkind Kind,
    int Value) : Nativeˉoperation;

public sealed record Nativeˉtextˉconcat(
    int Result,
    int Left,
    int Right) : Nativeˉoperation;

public sealed record Nativeˉtextˉquote(
    int Result,
    int Text) : Nativeˉoperation;

public sealed record Nativeˉrecordˉcreate(
    int Result,
    int Type,
    ImmutableArray<int> Fields) : Nativeˉoperation;

public sealed record Nativeˉrecordˉfield(
    int Result,
    int Type,
    int Field,
    int Record) : Nativeˉoperation;

public sealed record Nativeˉconsoleˉwriteˉline(
    int Text) : Nativeˉoperation;

public sealed record Nativeˉdiagnosticˉwriteˉline(
    int Text) : Nativeˉoperation;

public sealed record Nativeˉprocessˉargumentˉcount(int Result) : Nativeˉoperation;

public sealed record Nativeˉprocessˉargument(
    int Result,
    int Index) : Nativeˉoperation;

public sealed record Nativeˉfileˉreadˉbytes(
    int Result,
    int Resourceˉname) : Nativeˉoperation;

public sealed record Nativeˉvoidˉcall(
    int Function,
    ImmutableArray<int> Arguments) : Nativeˉoperation;

public abstract record Nativeˉterminator;

public sealed record Nativeˉjump(int Targetˉblock) : Nativeˉterminator;

public sealed record Nativeˉbranch(
    int Condition,
    int Trueˉblock,
    int Falseˉblock) : Nativeˉterminator;

public sealed record Nativeˉreturn(int Value) : Nativeˉterminator;

public sealed record Nativeˉreturnˉvoid : Nativeˉterminator;

public sealed record Nativeˉblock(
    int Id,
    ImmutableArray<Nativeˉoperation> Operations,
    Nativeˉterminator Terminator);

public sealed record Nativeˉfunction(
    string Name,
    ImmutableArray<Nativeˉvalueˉtype> Parameterˉtypes,
    Nativeˉvalueˉtype Returnˉtype,
    ImmutableArray<Nativeˉvalueˉtype> Localˉtypes,
    ImmutableArray<Nativeˉvalueˉtype> Valueˉtypes,
    ImmutableArray<Nativeˉblock> Blocks)
{
    public ImmutableArray<Nativeˉvalueˉtype> Allˉlocalˉtypes => [.. Parameterˉtypes, .. Localˉtypes];
}

public abstract record Nativeˉdata(string Name);

public sealed record Nativeˉi32ˉdata(string Name, ImmutableArray<int> Values)
    : Nativeˉdata(Name);

public sealed record Nativeˉutf8ˉdata(string Name, ImmutableArray<byte> Bytes)
    : Nativeˉdata(Name);

public sealed record Nativeˉbytesˉdata(string Name, ImmutableArray<byte> Bytes)
    : Nativeˉdata(Name);

public sealed record Nativeˉmodule(
    ImmutableArray<Nativeˉfunction> Functions,
    ImmutableArray<Nativeˉdata> Data,
    ImmutableArray<Nominalˉtypeˉdeclaration> Types,
    ImmutableArray<Nativeˉservice> Requiredˉservices);

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
    ImmutableArray<Nativeˉpatch> Patches,
    ImmutableArray<Nominalˉtypeˉdeclaration> Types,
    ImmutableArray<Nativeˉservice> Requiredˉservices);

public sealed record Nativeˉcompilation(
    Nativeˉmodule Module,
    Nativeˉfragment Fragment);

public sealed class Nativeˉbackendˉexception(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
