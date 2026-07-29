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
    Portable,
    Hosted,
    System,
    Capability,
    Data,
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
    Bool,
    Text,
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
    Bang,
    Equals,
    Equalsˉequals,
    Bangˉequals,
    Less,
    Lessˉequals,
    Greater,
    Greaterˉequals,
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
    Bool,
    Text,
    I32ˉarray,
    Invalid,
}

internal sealed record Typeˉsyntax(Typeˉsyntaxˉkind Kind, Sourceˉspan Span);

internal sealed record Moduleˉsyntax(
    Syntaxˉtoken Name,
    Syntaxˉtoken Profile,
    ImmutableArray<Capabilityˉsyntax> Capabilities,
    ImmutableArray<Dataˉsyntax> Data,
    ImmutableArray<Functionˉsyntax> Functions);

internal sealed record Capabilityˉsyntax(string Name, Sourceˉspan Span);

internal abstract record Dataˉvalueˉsyntax(Sourceˉspan Span);

internal sealed record Textˉdataˉvalueˉsyntax(string Value, Sourceˉspan Span)
    : Dataˉvalueˉsyntax(Span);

internal sealed record I32ˉarrayˉdataˉvalueˉsyntax(
    ImmutableArray<int> Values,
    Sourceˉspan Span)
    : Dataˉvalueˉsyntax(Span);

internal sealed record Dataˉsyntax(
    Syntaxˉtoken Name,
    Typeˉsyntax Type,
    Dataˉvalueˉsyntax Value,
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
    Typeˉsyntax Type,
    Expressionˉsyntax Initializer,
    Sourceˉspan Span)
    : Statementˉsyntax(Span);

internal sealed record Assignmentˉstatementˉsyntax(
    Syntaxˉtoken Name,
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

internal sealed record Returnˉstatementˉsyntax(Expressionˉsyntax? Value, Sourceˉspan Span)
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

internal sealed record Indexˉexpressionˉsyntax(
    string Name,
    Expressionˉsyntax Index,
    Sourceˉspan Span)
    : Expressionˉsyntax(Span);

internal sealed record Invalidˉexpressionˉsyntax(Sourceˉspan Span)
    : Expressionˉsyntax(Span);
