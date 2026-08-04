using System.Collections.Immutable;

namespace Windvale.Bytecode;

public enum Moduleˉprofile : byte
{
    Portable = 1,
    Hosted = 2,
    System = 3,
}

public enum Moduleˉauthority : byte
{
    Library = 1,
    Application = 2,
    Service = 3,
    System = 4,
}

public sealed record Capabilityˉrequirement(
    string Name,
    uint Majorˉversion);

public sealed record Moduleˉmetadata(
    Moduleˉauthority Authority,
    ImmutableArray<string> Platformˉscopes,
    ImmutableArray<Capabilityˉrequirement> Requiredˉcapabilities,
    ImmutableArray<Capabilityˉrequirement> Optionalˉcapabilities);

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
    Enum = 8,
    I64 = 9,
    U64 = 10,
    Variant = 11,
    Sequence = 12,
    Builder = 13,
}

public readonly record struct Valueˉshape(
    Valueˉtype Kind,
    int Nominalˉtypeˉindex = -1,
    Valueˉtype Elementˉkind = Valueˉtype.Void,
    int Elementˉnominalˉtypeˉindex = -1,
    uint Maximum = 0)
{
    public static implicit operator Valueˉshape(Valueˉtype kind) => new(kind);

    public static Valueˉshape Forˉrecord(int recordˉtypeˉindex) =>
        new(Valueˉtype.Record, recordˉtypeˉindex);

    public static Valueˉshape Forˉenum(int enumˉtypeˉindex) =>
        new(Valueˉtype.Enum, enumˉtypeˉindex);

    public static Valueˉshape Forˉvariant(int variantˉtypeˉindex) =>
        new(Valueˉtype.Variant, variantˉtypeˉindex);

    public static Valueˉshape Forˉsequence(Valueˉshape elementˉshape, uint maximum) =>
        new(
            Valueˉtype.Sequence,
            Elementˉkind: elementˉshape.Kind,
            Elementˉnominalˉtypeˉindex: elementˉshape.Nominalˉtypeˉindex,
            Maximum: maximum);

    public static Valueˉshape Forˉbuilder(Valueˉshape elementˉshape, uint maximum) =>
        new(
            Valueˉtype.Builder,
            Elementˉkind: elementˉshape.Kind,
            Elementˉnominalˉtypeˉindex: elementˉshape.Nominalˉtypeˉindex,
            Maximum: maximum);

    public Valueˉshape Elementˉshape => new(Elementˉkind, Elementˉnominalˉtypeˉindex);

    public override string ToString()
    {
        if (Kind is Valueˉtype.Record or Valueˉtype.Enum or Valueˉtype.Variant)
        {
            return $"{Kind.ToString().ToLowerInvariant()}[{Nominalˉtypeˉindex}]";
        }

        return Kind is Valueˉtype.Sequence or Valueˉtype.Builder
            ? $"{Kind.ToString().ToLowerInvariant()}<{Elementˉshape},{Maximum}>"
            : Kind.ToString();
    }
}

public enum Nominalˉtypeˉkind : byte
{
    Record = 1,
    Enum = 2,
    Variant = 3,
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
    Enumˉconst = 0x6A,
    Enumˉequal = 0x6B,
    Enumˉnotˉequal = 0x6C,
    Enumˉname = 0x6D,
    I32ˉformat = 0x6E,
    U8ˉformat = 0x6F,
    U32ˉformat = 0x70,
    Textˉconcat = 0x71,
    Bytesˉreadˉi32ˉlittle = 0x72,
    Textˉutf8ˉisˉvalid = 0x73,
    Textˉfromˉutf8 = 0x74,
    Textˉquote = 0x75,
    U32ˉfromˉu8 = 0x76,
    Bytesˉconcat = 0x77,
    Bytesˉfromˉu8 = 0x78,
    Bytesˉfromˉu16ˉlittle = 0x79,
    Bytesˉfromˉu32ˉlittle = 0x7A,
    Bytesˉfromˉi32ˉlittle = 0x7B,
    Textˉtoˉutf8 = 0x7C,
    Bytesˉsha256ˉhex = 0x7D,

    I64ˉconst = 0x80,
    U64ˉconst = 0x81,
    I64ˉadd = 0x82,
    I64ˉsubtract = 0x83,
    I64ˉmultiply = 0x84,
    I64ˉnegate = 0x85,
    U64ˉadd = 0x86,
    U64ˉsubtract = 0x87,
    U64ˉmultiply = 0x88,
    I64ˉequal = 0x89,
    I64ˉnotˉequal = 0x8A,
    I64ˉless = 0x8B,
    I64ˉlessˉequal = 0x8C,
    I64ˉgreater = 0x8D,
    I64ˉgreaterˉequal = 0x8E,
    U64ˉequal = 0x8F,
    U64ˉnotˉequal = 0x90,
    U64ˉless = 0x91,
    U64ˉlessˉequal = 0x92,
    U64ˉgreater = 0x93,
    U64ˉgreaterˉequal = 0x94,
    I64ˉformat = 0x95,
    U64ˉformat = 0x96,
    Variantˉcreate = 0x97,
    Variantˉisˉcase = 0x98,
    Variantˉpayload = 0x99,
    Builderˉcreate = 0x9A,
    Builderˉpush = 0x9B,
    Builderˉfreeze = 0x9C,
    Sequenceˉlength = 0x9D,
    Sequenceˉelement = 0x9E,
    I32ˉdivide = 0x9F,
    I32ˉremainder = 0xA0,
    U32ˉdivide = 0xA1,
    U32ˉremainder = 0xA2,
    I64ˉdivide = 0xA3,
    I64ˉremainder = 0xA4,
    U64ˉdivide = 0xA5,
    U64ˉremainder = 0xA6,
    U8ˉbitwiseˉand = 0xA7,
    U8ˉbitwiseˉor = 0xA8,
    U8ˉbitwiseˉxor = 0xA9,
    U8ˉbitwiseˉnot = 0xAA,
    U8ˉshiftˉleft = 0xAB,
    U8ˉshiftˉright = 0xAC,
    U32ˉbitwiseˉand = 0xAD,
    U32ˉbitwiseˉor = 0xAE,
    U32ˉbitwiseˉxor = 0xAF,
    U32ˉbitwiseˉnot = 0xB0,
    U32ˉshiftˉleft = 0xB1,
    U32ˉshiftˉright = 0xB2,
    U64ˉbitwiseˉand = 0xB3,
    U64ˉbitwiseˉor = 0xB4,
    U64ˉbitwiseˉxor = 0xB5,
    U64ˉbitwiseˉnot = 0xB6,
    U64ˉshiftˉleft = 0xB7,
    U64ˉshiftˉright = 0xB8,
    Textˉequal = 0xB9,
    Textˉnotˉequal = 0xBA,
    Bytesˉequal = 0xBB,
    Bytesˉnotˉequal = 0xBC,
    Bytesˉreadˉu64ˉlittle = 0xBD,
    Bytesˉfromˉu64ˉlittle = 0xBE,

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

public abstract record Nominalˉtypeˉdeclaration(string Name, Nominalˉtypeˉkind Kind);

public sealed record Recordˉfieldˉdeclaration(string Name, Valueˉshape Type);

public sealed record Recordˉtypeˉdeclaration(
    string Name,
    ImmutableArray<Recordˉfieldˉdeclaration> Fields)
    : Nominalˉtypeˉdeclaration(Name, Nominalˉtypeˉkind.Record);

public sealed record Enumˉmemberˉdeclaration(string Name, int Value);

public sealed record Enumˉtypeˉdeclaration(
    string Name,
    ImmutableArray<Enumˉmemberˉdeclaration> Members)
    : Nominalˉtypeˉdeclaration(Name, Nominalˉtypeˉkind.Enum);

public sealed record Variantˉcaseˉdeclaration(
    string Name,
    string? Payloadˉname,
    Valueˉshape? Payloadˉtype);

public sealed record Variantˉtypeˉdeclaration(
    string Name,
    ImmutableArray<Variantˉcaseˉdeclaration> Cases)
    : Nominalˉtypeˉdeclaration(Name, Nominalˉtypeˉkind.Variant);

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
    public ImmutableArray<Nominalˉtypeˉdeclaration> Types { get; init; } = [];

    public Moduleˉmetadata? Metadata { get; init; }

    public ushort Formatˉminorˉversion { get; init; } = Moduleˉcodec.BASE_MINOR_VERSION;
}

public sealed record Decodedˉinstruction(
    int Offset,
    int Size,
    Opcode Opcode,
    int Signedˉoperand = 0,
    uint Unsignedˉoperand = 0,
    uint Secondˉunsignedˉoperand = 0,
    long Signedˉwideˉoperand = 0,
    ulong Unsignedˉwideˉoperand = 0);

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
