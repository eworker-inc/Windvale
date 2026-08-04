using System.Collections.Immutable;

namespace Windvale.Compiler;

internal enum Tokenˉkind
{
    End,
    Bad,
    Identifier,
    Integer,
    String,

    Module,
    Profile,
    Platform,
    Authority,
    Requires,
    Optional,
    Version,
    Portable,
    Hosted,
    System,
    Import,
    Capability,
    Data,
    Record,
    Enum,
    Variant,
    Export,
    Fn,
    Let,
    Var,
    If,
    Else,
    While,
    Return,
    True,
    False,
    I32,
    I64,
    U8,
    U32,
    U64,
    Bool,
    Text,
    Bytes,
    Void,
    Length,

    Leftˉparenthesis,
    Rightˉparenthesis,
    Leftˉbrace,
    Rightˉbrace,
    Leftˉbracket,
    Rightˉbracket,
    Semicolon,
    Colon,
    Comma,
    Dot,
    Arrow,
    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    Ampersand,
    Pipe,
    Caret,
    Tilde,
    Shiftˉleft,
    Shiftˉright,
    Bang,
    Equals,
    Equalsˉequals,
    Bangˉequals,
    Less,
    Lessˉequals,
    Greater,
    Greaterˉequals,
    Const,
    Break,
    Continue,
    Andˉand,
    Orˉor,
    Plusˉequals,
    Minusˉequals,
    Starˉequals,
    As,
    Match,
    Case,
    Sequence,
    Builder,
    Freeze,
    Push,
    For,
    In,
}

internal sealed record Syntaxˉtoken(
    Tokenˉkind Kind,
    string Text,
    Sourceˉspan Span,
    object? Value = null);

internal enum Typeˉsyntaxˉkind
{
    Void,
    I32,
    I64,
    U8,
    U32,
    U64,
    Bool,
    Text,
    Bytes,
    I32ˉarray,
    Named,
    Sequence,
    Builder,
    Invalid,
}

internal sealed record Typeˉsyntax(
    Typeˉsyntaxˉkind Kind,
    Sourceˉspan Span,
    string? Name = null,
    Typeˉsyntax? Elementˉtype = null,
    uint Maximum = 0);

internal sealed record Moduleˉsyntax(
    Syntaxˉtoken Name,
    Syntaxˉtoken Profile,
    Moduleˉmetadataˉsyntax? Metadata,
    ImmutableArray<Importˉsyntax> Imports,
    ImmutableArray<Capabilityˉsyntax> Capabilities,
    ImmutableArray<Dataˉsyntax> Data,
    ImmutableArray<Constantˉsyntax> Constants,
    ImmutableArray<Recordˉsyntax> Records,
    ImmutableArray<Enumˉsyntax> Enums,
    ImmutableArray<Variantˉsyntax> Variants,
    ImmutableArray<Functionˉsyntax> Functions);

internal sealed record Platformˉscopeˉsyntax(
    string Name,
    Sourceˉspan Span);

internal sealed record Capabilityˉrequirementˉsyntax(
    string Name,
    uint Majorˉversion,
    Sourceˉspan Span);

internal sealed record Moduleˉmetadataˉsyntax(
    Syntaxˉtoken Authority,
    ImmutableArray<Platformˉscopeˉsyntax> Platformˉscopes,
    ImmutableArray<Capabilityˉrequirementˉsyntax> Requiredˉcapabilities,
    ImmutableArray<Capabilityˉrequirementˉsyntax> Optionalˉcapabilities);

internal sealed record Importˉsyntax(
    Syntaxˉtoken Name,
    Syntaxˉtoken Alias,
    Sourceˉspan Span);

internal sealed record Capabilityˉsyntax(string Name, Sourceˉspan Span);

internal abstract record Dataˉvalueˉsyntax(Sourceˉspan Span);

internal sealed record Textˉdataˉvalueˉsyntax(string Value, Sourceˉspan Span)
    : Dataˉvalueˉsyntax(Span);

internal sealed record I32ˉarrayˉdataˉvalueˉsyntax(
    ImmutableArray<int> Values,
    Sourceˉspan Span)
    : Dataˉvalueˉsyntax(Span);

internal sealed record Bytesˉdataˉvalueˉsyntax(
    ImmutableArray<byte> Values,
    Sourceˉspan Span)
    : Dataˉvalueˉsyntax(Span);

internal sealed record Dataˉsyntax(
    bool Isˉexported,
    Syntaxˉtoken Name,
    Typeˉsyntax Type,
    Dataˉvalueˉsyntax Value,
    Sourceˉspan Span);

internal sealed record Constantˉsyntax(
    bool Isˉexported,
    Syntaxˉtoken Name,
    Typeˉsyntax Type,
    Expressionˉsyntax Initializer,
    Sourceˉspan Span);

internal sealed record Recordˉfieldˉsyntax(
    Syntaxˉtoken Name,
    Typeˉsyntax Type,
    Sourceˉspan Span);

internal sealed record Recordˉsyntax(
    bool Isˉexported,
    Syntaxˉtoken Name,
    ImmutableArray<Recordˉfieldˉsyntax> Fields,
    Sourceˉspan Span);

internal sealed record Enumˉmemberˉsyntax(
    Syntaxˉtoken Name,
    Syntaxˉtoken Value,
    Sourceˉspan Span);

internal sealed record Enumˉsyntax(
    bool Isˉexported,
    Syntaxˉtoken Name,
    ImmutableArray<Enumˉmemberˉsyntax> Members,
    Sourceˉspan Span);

internal sealed record Variantˉcaseˉsyntax(
    Syntaxˉtoken Name,
    Syntaxˉtoken? Payloadˉname,
    Typeˉsyntax? Payloadˉtype,
    Sourceˉspan Span);

internal sealed record Variantˉsyntax(
    bool Isˉexported,
    Syntaxˉtoken Name,
    ImmutableArray<Variantˉcaseˉsyntax> Cases,
    Sourceˉspan Span);

internal sealed record Parameterˉsyntax(
    Syntaxˉtoken Name,
    Typeˉsyntax Type,
    Sourceˉspan Span);

internal sealed record Functionˉsyntax(
    bool Isˉexported,
    Syntaxˉtoken Name,
    ImmutableArray<Parameterˉsyntax> Parameters,
    Typeˉsyntax Returnˉtype,
    Blockˉstatementˉsyntax Body,
    Sourceˉspan Span);

internal abstract record Statementˉsyntax(Sourceˉspan Span);

internal sealed record Blockˉstatementˉsyntax(
    ImmutableArray<Statementˉsyntax> Statements,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Localˉdeclarationˉstatementˉsyntax(
    bool Isˉmutable,
    Syntaxˉtoken Name,
    Typeˉsyntax? Type,
    Expressionˉsyntax Initializer,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Assignmentˉstatementˉsyntax(
    Syntaxˉtoken Name,
    Tokenˉkind Operator,
    Expressionˉsyntax Value,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Expressionˉstatementˉsyntax(Expressionˉsyntax Expression, Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Ifˉstatementˉsyntax(
    Expressionˉsyntax Condition,
    Blockˉstatementˉsyntax Then,
    Blockˉstatementˉsyntax? Else,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Whileˉstatementˉsyntax(
    Expressionˉsyntax Condition,
    Blockˉstatementˉsyntax Body,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Pushˉstatementˉsyntax(
    Syntaxˉtoken Builder,
    Expressionˉsyntax Value,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Forˉstatementˉsyntax(
    Syntaxˉtoken Binding,
    Expressionˉsyntax Sequence,
    Blockˉstatementˉsyntax Body,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Returnˉstatementˉsyntax(Expressionˉsyntax? Value, Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Breakˉstatementˉsyntax(Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Continueˉstatementˉsyntax(Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Matchˉcaseˉsyntax(
    string Nominalˉname,
    string Memberˉname,
    Syntaxˉtoken? Binding,
    Blockˉstatementˉsyntax Body,
    Sourceˉspan Span);

internal sealed record Matchˉstatementˉsyntax(
    Expressionˉsyntax Value,
    ImmutableArray<Matchˉcaseˉsyntax> Cases,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal abstract record Expressionˉsyntax(Sourceˉspan Span);

internal sealed record Literalˉexpressionˉsyntax(object Value, Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Nameˉexpressionˉsyntax(string Name, Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Unaryˉexpressionˉsyntax(
    Tokenˉkind Operator,
    Expressionˉsyntax Operand,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Binaryˉexpressionˉsyntax(
    Expressionˉsyntax Left,
    Tokenˉkind Operator,
    Expressionˉsyntax Right,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Callˉexpressionˉsyntax(
    string Name,
    ImmutableArray<Expressionˉsyntax> Arguments,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Builderˉexpressionˉsyntax(
    Typeˉsyntax Type,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Recordˉfieldˉinitializerˉsyntax(
    Syntaxˉtoken Name,
    Expressionˉsyntax Value,
    Sourceˉspan Span);

internal sealed record Recordˉexpressionˉsyntax(
    string Name,
    ImmutableArray<Recordˉfieldˉinitializerˉsyntax> Fields,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Indexˉexpressionˉsyntax(
    string Name,
    Expressionˉsyntax Index,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Fieldˉexpressionˉsyntax(
    string Target,
    string Field,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Invalidˉexpressionˉsyntax(Sourceˉspan Span)
    : Expressionˉsyntax(Span);
