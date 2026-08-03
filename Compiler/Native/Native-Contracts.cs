using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉcontract
{
    public const int ABI_VERSION = 22;
    public const string X64_BASELINE_TARGET = "x86-64-wvb-baseline-v22";
    public const long DEFAULT_MAXIMUM_INSTRUCTIONS = 1_000_000;
    public const int DEFAULT_MAXIMUM_CALL_DEPTH = 1024;
    public const int MAXIMUM_CODE_BYTES = 32 * 1024 * 1024;
    public const int MAXIMUM_FRAME_SLOTS = 2048;
    public const int MAXIMUM_VALUE_IDENTIFIERS = 100_000;
    public const int VALUE_SLOT_BYTES = 16;
    public const int BORROWED_BYTES_POINTER_OFFSET = 0;
    public const int BORROWED_BYTES_LENGTH_OFFSET = 8;
    public const int BORROWED_BYTES_RESERVED_OFFSET = 12;
    public const int DYNAMIC_BYTES_HEADER_BYTES = 8;
    public const int DYNAMIC_BYTES_MINIMUM_OWNED_LENGTH = 64;
    public const int DYNAMIC_BYTES_MAXIMUM_CAPACITY = 4 * 1024 * 1024;
    public const int DYNAMIC_BYTES_MAXIMUM_DOUBLED_LENGTH =
        DYNAMIC_BYTES_MAXIMUM_CAPACITY / 2;
    public const uint DYNAMIC_BYTES_FIRST_GENERATION = 1;
    public const int BORROWED_TEXT_POINTER_OFFSET = 0;
    public const int BORROWED_TEXT_LENGTH_OFFSET = 8;
    public const int BORROWED_TEXT_RESERVED_OFFSET = 12;
    public const int MAXIMUM_FRAME_BYTES = MAXIMUM_FRAME_SLOTS * VALUE_SLOT_BYTES;
    public const int MAXIMUM_BLOCKS = 4096;
    public const int REGISTER_CALL_PARAMETERS = 4;
    public const int MAXIMUM_CALL_PARAMETERS = 64;
    public const int MAXIMUM_STACK_CALL_PARAMETERS =
        MAXIMUM_CALL_PARAMETERS - REGISTER_CALL_PARAMETERS;
    public const int MAXIMUM_STACK_CALL_BYTES =
        MAXIMUM_STACK_CALL_PARAMETERS * VALUE_SLOT_BYTES;
    public const int MAXIMUM_RECORD_ARENA_BYTES = 2 * 1024 * 1024;
    public const int MAXIMUM_TEXT_ARENA_BYTES = 64 * 1024 * 1024;
    public const int MAXIMUM_ENUM_METADATA_BYTES = 32 * 1024 * 1024;
    public const int MAXIMUM_DESCRIPTOR_OWNERSHIP_ACTIONS = 4 * 1024 * 1024;
}

public static class Nativeˉexecutionˉcontextˉcontract
{
    public const uint FORMAT_VERSION = 7;
    public const uint SIZE = 112;
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
    public const int ARGUMENT_TABLE_POINTER_OFFSET = 72;
    public const int ARGUMENT_COUNT_OFFSET = 80;
    public const int ARGUMENT_RESERVED_OFFSET = 84;
    public const int OUTPUT_TABLE_POINTER_OFFSET = 88;
    public const int FILE_INPUT_TABLE_POINTER_OFFSET = 96;
    public const int FILE_OUTPUT_TABLE_POINTER_OFFSET = 104;
}

public enum Nativeˉserviceˉfailureˉdetail : uint
{
    None = 0,
    Textˉvalueˉlimit = 1,
    Textˉarenaˉexhausted = 2,
    Argumentˉindexˉoutˉofˉrange = 3,
    Outputˉwriteˉfailed = 4,
    Fileˉinvalidˉname = 5,
    Fileˉnotˉfound = 6,
    Fileˉpermissionˉdenied = 7,
    Fileˉunavailable = 8,
    Fileˉtooˉlarge = 9,
    Fileˉsnapshotˉlimit = 10,
    Bytesˉvalueˉlimit = 11,
    Bytesˉu16ˉoutˉofˉrange = 12,
}

public static class Nativeˉserviceˉtableˉcontract
{
    public const uint FORMAT_VERSION = 5;
    public const uint SIZE = 104;
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
    public const int FILE_WRITE_BYTES_POINTER_OFFSET = 96;
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
    Fileˉwriteˉbytes = 12,
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

public sealed record Nativeˉbytesˉconcat(
    int Result,
    int Left,
    int Right) : Nativeˉoperation;

public sealed record Nativeˉbytesˉfromˉu8(
    int Result,
    int Value) : Nativeˉoperation;

public sealed record Nativeˉbytesˉfromˉu16ˉlittle(
    int Result,
    int Value) : Nativeˉoperation;

public sealed record Nativeˉbytesˉfromˉu32ˉlittle(
    int Result,
    int Value) : Nativeˉoperation;

public sealed record Nativeˉtextˉutf8ˉisˉvalid(
    int Result,
    int Bytes) : Nativeˉoperation;

public sealed record Nativeˉtextˉfromˉutf8(
    int Result,
    int Bytes) : Nativeˉoperation;

public sealed record Nativeˉtextˉtoˉutf8(
    int Result,
    int Text) : Nativeˉoperation;

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

public sealed record Nativeˉfileˉwriteˉbytes(
    int Resourceˉname,
    int Bytes) : Nativeˉoperation;

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

public enum Nativeˉdescriptorˉcarrierˉkind : byte
{
    None = 0,
    Parameter = 1,
    Local = 2,
    Value = 3,
    Recordˉparameterˉfield = 4,
    Recordˉlocalˉfield = 5,
    Recordˉvalueˉfield = 6,
    Functionˉreturn = 7,
}

public sealed record Nativeˉdescriptorˉcarrier(
    Nativeˉdescriptorˉcarrierˉkind Kind,
    int Function,
    int Binding,
    int Field)
{
    public static Nativeˉdescriptorˉcarrier None { get; } = new(
        Nativeˉdescriptorˉcarrierˉkind.None,
        -1,
        -1,
        -1);
}

public enum Nativeˉdescriptorˉownershipˉactionˉkind : byte
{
    Borrowˉstatic = 1,
    Borrowˉhost = 2,
    Acquire = 3,
    Retain = 4,
    Release = 5,
    Borrowˉcall = 6,
    Acceptˉreturn = 7,
    Transferˉreturn = 8,
}

// Operation -1 is function entry; Operations.Length identifies the terminator.
public sealed record Nativeˉdescriptorˉownershipˉaction(
    int Block,
    int Operation,
    Nativeˉdescriptorˉownershipˉactionˉkind Kind,
    Nativeˉdescriptorˉcarrier Target,
    Nativeˉdescriptorˉcarrier Source);

public sealed record Nativeˉfunctionˉdescriptorˉownership(
    int Functionˉindex,
    string Functionˉname,
    int Descriptorˉparameterˉbindings,
    int Assignedˉdescriptorˉparameterˉbindings,
    int Descriptorˉlocalˉbindings,
    int Recordˉparameterˉdescriptorˉfields,
    int Assignedˉrecordˉparameterˉdescriptorˉfields,
    int Recordˉlocalˉdescriptorˉfields,
    int Descriptorˉvalueˉidentifiers,
    int Recordˉvalueˉdescriptorˉfields,
    int Acquireˉactions,
    int Borrowˉactions,
    int Retainˉactions,
    int Releaseˉactions,
    int Callˉborrowˉactions,
    int Acceptedˉreturnˉactions,
    int Transferredˉreturnˉactions,
    ImmutableArray<Nativeˉdescriptorˉownershipˉaction> Actions);

public sealed record Nativeˉdescriptorˉownershipˉplan(
    uint Formatˉversion,
    bool Terminalˉfailureˉdiscardsˉarena,
    int Totalˉactions,
    ImmutableArray<Nativeˉfunctionˉdescriptorˉownership> Functions);

public sealed record Nativeˉfunction(
    string Name,
    ImmutableArray<Nativeˉvalueˉtype> Parameterˉtypes,
    Nativeˉvalueˉtype Returnˉtype,
    int Returnˉnominalˉtypeˉindex,
    ImmutableArray<Nativeˉvalueˉtype> Localˉtypes,
    ImmutableArray<int> Allˉlocalˉnominalˉtypeˉindices,
    ImmutableArray<Nativeˉvalueˉtype> Valueˉtypes,
    ImmutableArray<int> Valueˉnominalˉtypeˉindices,
    ImmutableArray<int> Valueˉslotˉindices,
    int Valueˉslotˉcount,
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
    ImmutableArray<Nativeˉservice> Requiredˉservices)
{
    public Nativeˉdescriptorˉownershipˉplan? Descriptorˉownership { get; init; }
}

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

public enum Nativeˉentryˉresultˉkind : byte
{
    Void = 0,
    Scalar = 1,
    Descriptor = 2,
}

public sealed record Nativeˉcompilation(
    Nativeˉmodule Module,
    Nativeˉfragment Fragment);

public sealed class Nativeˉbackendˉexception(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
