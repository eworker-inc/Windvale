using System.Collections.Immutable;

namespace Windvale.Bytecode;

public enum Moduleˉprofile : byte
{
    Portable = 1,
    Hosted = 2,
    System = 3,
}

public enum Valueˉtype : byte
{
    Void = 0,
    I32 = 1,
    Bool = 2,
    Text = 3,
    U8 = 4,
    U32 = 5,
    Bytes = 6,
    Record = 7,
}

public readonly record struct Valueˉshape(Valueˉtype Kind, int Recordˉtypeˉindex = -1)
{
    public static implicit operator Valueˉshape(Valueˉtype kind) => new(kind);

    public static Valueˉshape Forˉrecord(int recordˉtypeˉindex) =>
        new(Valueˉtype.Record, recordˉtypeˉindex);

    public override string ToString()
    {
        return Kind == Valueˉtype.Record ? $"record[{Recordˉtypeˉindex}]" : Kind.ToString();
    }
}

public enum Dataˉtype : byte
{
    Text = 3,
    I32ˉarray = 4,
    Bytes = 5,
}

public enum Sectionˉkind : byte
{
    Module = 1,
    Capabilities = 2,
    Data = 3,
    Functions = 4,
    Code = 5,
    Exports = 6,
    Types = 7,
}

public enum Exportˉkind : byte
{
    Function = 1,
}

public enum Opcode : byte
{
    I32ˉconst = 0x01,
    Boolˉconst = 0x02,
    Textˉconst = 0x03,
    Localˉload = 0x04,
    Localˉstore = 0x05,
    Dataˉlength = 0x06,
    Dataˉloadˉi32 = 0x07,
    U8ˉconst = 0x08,
    U32ˉconst = 0x09,
    Bytesˉconst = 0x0A,
    Bytesˉlength = 0x0B,
    Bytesˉslice = 0x0C,
    Bytesˉreadˉu8 = 0x0D,
    Bytesˉreadˉu16ˉlittle = 0x0E,
    Bytesˉreadˉu32ˉlittle = 0x0F,

    I32ˉadd = 0x10,
    I32ˉsubtract = 0x11,
    I32ˉmultiply = 0x12,
    I32ˉnegate = 0x13,
    U32ˉadd = 0x14,
    U32ˉsubtract = 0x15,
    U32ˉmultiply = 0x16,

    I32ˉequal = 0x20,
    I32ˉnotˉequal = 0x21,
    I32ˉless = 0x22,
    I32ˉlessˉequal = 0x23,
    I32ˉgreater = 0x24,
    I32ˉgreaterˉequal = 0x25,
    Boolˉequal = 0x26,
    Boolˉnotˉequal = 0x27,
    Boolˉnot = 0x28,

    U32ˉequal = 0x60,
    U32ˉnotˉequal = 0x61,
    U32ˉless = 0x62,
    U32ˉlessˉequal = 0x63,
    U32ˉgreater = 0x64,
    U32ˉgreaterˉequal = 0x65,
    U8ˉequal = 0x66,
    U8ˉnotˉequal = 0x67,
    Recordˉcreate = 0x68,
    Recordˉfield = 0x69,

    Jump = 0x30,
    Branchˉfalse = 0x31,

    Call = 0x40,
    Callˉcapability = 0x41,

    Pop = 0x50,
    Return = 0x51,
}

public abstract record Dataˉdeclaration(string Name, Dataˉtype Type);

public sealed record Textˉdataˉdeclaration(string Name, string Value)
    : Dataˉdeclaration(Name, Dataˉtype.Text);

public sealed record I32ˉarrayˉdataˉdeclaration(string Name, ImmutableArray<int> Values)
    : Dataˉdeclaration(Name, Dataˉtype.I32ˉarray);

public sealed record Bytesˉdataˉdeclaration(string Name, ImmutableArray<byte> Values)
    : Dataˉdeclaration(Name, Dataˉtype.Bytes);

public sealed record Recordˉfieldˉdeclaration(string Name, Valueˉtype Type);

public sealed record Recordˉtypeˉdeclaration(
    string Name,
    ImmutableArray<Recordˉfieldˉdeclaration> Fields);

public sealed record Capabilityˉdeclaration(
    string Name,
    ImmutableArray<Valueˉtype> Parameterˉtypes,
    Valueˉtype Returnˉtype);

public sealed record Functionˉdeclaration(
    string Name,
    ImmutableArray<Valueˉshape> Parameterˉtypes,
    Valueˉshape Returnˉtype,
    ImmutableArray<Valueˉshape> Localˉtypes,
    int Codeˉoffset,
    int Codeˉlength,
    int Maximumˉstackˉdepth)
{
    public ImmutableArray<Valueˉshape> Allˉlocalˉtypes => [.. Parameterˉtypes, .. Localˉtypes];
}

public sealed record Exportˉdeclaration(
    string Name,
    Exportˉkind Kind,
    int Targetˉindex);

public sealed record Bytecodeˉmodule(
    string Name,
    Moduleˉprofile Profile,
    ImmutableArray<Capabilityˉdeclaration> Capabilities,
    ImmutableArray<Dataˉdeclaration> Data,
    ImmutableArray<Functionˉdeclaration> Functions,
    ImmutableArray<byte> Code,
    ImmutableArray<Exportˉdeclaration> Exports)
{
    public ImmutableArray<Recordˉtypeˉdeclaration> Types { get; init; } = [];
}

public sealed record Decodedˉinstruction(
    int Offset,
    int Size,
    Opcode Opcode,
    int Signedˉoperand = 0,
    uint Unsignedˉoperand = 0);

public sealed record Verifiedˉfunction(
    Functionˉdeclaration Declaration,
    ImmutableArray<Decodedˉinstruction> Instructions);

public sealed class Verifiedˉmodule
{
    internal Verifiedˉmodule(
        Bytecodeˉmodule module,
        ImmutableArray<Verifiedˉfunction> functions)
    {
        Module = module;
        Functions = functions;
    }

    public Bytecodeˉmodule Module { get; }

    public ImmutableArray<Verifiedˉfunction> Functions { get; }
}
