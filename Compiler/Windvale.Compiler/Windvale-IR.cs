using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

internal enum Wirˉoperation
{
    I32ˉconstant,
    U8ˉconstant,
    U32ˉconstant,
    Boolˉconstant,
    Textˉconstant,
    Bytesˉconstant,
    Loadˉlocal,
    Storeˉlocal,
    Dataˉlength,
    Dataˉloadˉi32,
    Bytesˉlength,
    Bytesˉslice,
    Bytesˉreadˉu8,
    Bytesˉreadˉu16ˉlittle,
    Bytesˉreadˉu32ˉlittle,
    Recordˉcreate,
    Recordˉfield,
    Enumˉconstant,
    Enumˉequal,
    Enumˉnotˉequal,
    Enumˉname,
    I32ˉformat,
    U8ˉformat,
    U32ˉformat,
    Textˉconcat,
    I32ˉadd,
    I32ˉsubtract,
    I32ˉmultiply,
    I32ˉnegate,
    U32ˉadd,
    U32ˉsubtract,
    U32ˉmultiply,
    I32ˉequal,
    I32ˉnotˉequal,
    I32ˉless,
    I32ˉlessˉequal,
    I32ˉgreater,
    I32ˉgreaterˉequal,
    Boolˉequal,
    Boolˉnotˉequal,
    Boolˉnot,
    U32ˉequal,
    U32ˉnotˉequal,
    U32ˉless,
    U32ˉlessˉequal,
    U32ˉgreater,
    U32ˉgreaterˉequal,
    U8ˉequal,
    U8ˉnotˉequal,
    Callˉfunction,
    Callˉcapability,
}

internal sealed record Wirˉinstruction(
    Wirˉoperation Operation,
    int? Result,
    ImmutableArray<int> Operands,
    int Integerˉoperand = 0,
    uint Unsignedˉintegerˉoperand = 0,
    uint Secondˉunsignedˉintegerˉoperand = 0,
    string? Nameˉoperand = null);

internal abstract record Wirˉterminator;

internal sealed record Wirˉjump(int Targetˉblock) : Wirˉterminator;

internal sealed record Wirˉbranch(
    int Condition,
    int Trueˉblock,
    int Falseˉblock)
    : Wirˉterminator;

internal sealed record Wirˉreturn(int? Value) : Wirˉterminator;

internal sealed record Wirˉblock(
    int Id,
    ImmutableArray<Wirˉinstruction> Instructions,
    Wirˉterminator Terminator);

internal sealed record Wirˉfunction(
    string Name,
    ImmutableArray<Valueˉshape> Parameterˉtypes,
    Valueˉshape Returnˉtype,
    ImmutableArray<Valueˉshape> Userˉlocalˉtypes,
    ImmutableArray<Valueˉshape> Temporaryˉtypes,
    ImmutableArray<Wirˉblock> Blocks,
    bool Isˉexported);

internal sealed record Wirˉmodule(
    string Name,
    Moduleˉprofile Profile,
    ImmutableArray<Capabilityˉdeclaration> Capabilities,
    ImmutableArray<Dataˉdeclaration> Data,
    ImmutableArray<Nominalˉtypeˉdeclaration> Types,
    ImmutableArray<Wirˉfunction> Functions);
