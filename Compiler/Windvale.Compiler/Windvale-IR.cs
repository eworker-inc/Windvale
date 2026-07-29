using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

internal enum Wirˉoperation
{
    I32ˉconstant,
    Boolˉconstant,
    Textˉconstant,
    Loadˉlocal,
    Storeˉlocal,
    Dataˉlength,
    Dataˉloadˉi32,
    I32ˉadd,
    I32ˉsubtract,
    I32ˉmultiply,
    I32ˉnegate,
    I32ˉequal,
    I32ˉnotˉequal,
    I32ˉless,
    I32ˉlessˉequal,
    I32ˉgreater,
    I32ˉgreaterˉequal,
    Boolˉequal,
    Boolˉnotˉequal,
    Boolˉnot,
    Callˉfunction,
    Callˉcapability,
}

internal sealed record Wirˉinstruction(
    Wirˉoperation Operation,
    int? Result,
    ImmutableArray<int> Operands,
    int Integerˉoperand = 0,
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
    ImmutableArray<Valueˉtype> Parameterˉtypes,
    Valueˉtype Returnˉtype,
    ImmutableArray<Valueˉtype> Userˉlocalˉtypes,
    ImmutableArray<Valueˉtype> Temporaryˉtypes,
    ImmutableArray<Wirˉblock> Blocks,
    bool Isˉexported);

internal sealed record Wirˉmodule(
    string Name,
    Moduleˉprofile Profile,
    ImmutableArray<Capabilityˉdeclaration> Capabilities,
    ImmutableArray<Dataˉdeclaration> Data,
    ImmutableArray<Wirˉfunction> Functions);
