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
}

public enum Dataˉtype : byte
{
    Text = 3,
    I32ˉarray = 4,
}

public enum Sectionˉkind : byte
{
    Module = 1,
    Capabilities = 2,
    Data = 3,
    Functions = 4,
    Code = 5,
    Exports = 6,
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

    I32ˉadd = 0x10,
    I32ˉsubtract = 0x11,
    I32ˉmultiply = 0x12,
    I32ˉnegate = 0x13,

    I32ˉequal = 0x20,
    I32ˉnotˉequal = 0x21,
    I32ˉless = 0x22,
    I32ˉlessˉequal = 0x23,
    I32ˉgreater = 0x24,
    I32ˉgreaterˉequal = 0x25,
    Boolˉequal = 0x26,
    Boolˉnotˉequal = 0x27,
    Boolˉnot = 0x28,

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

public sealed record Capabilityˉdeclaration(
    string Name,
    ImmutableArray<Valueˉtype> Parameterˉtypes,
    Valueˉtype Returnˉtype);

public sealed record Functionˉdeclaration(
    string Name,
    ImmutableArray<Valueˉtype> Parameterˉtypes,
    Valueˉtype Returnˉtype,
    ImmutableArray<Valueˉtype> Localˉtypes,
    int Codeˉoffset,
    int Codeˉlength,
    int Maximumˉstackˉdepth)
{
    public ImmutableArray<Valueˉtype> Allˉlocalˉtypes => [.. Parameterˉtypes, .. Localˉtypes];
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
    ImmutableArray<Exportˉdeclaration> Exports);

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
