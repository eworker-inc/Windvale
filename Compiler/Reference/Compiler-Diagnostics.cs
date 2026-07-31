using System.Collections.Immutable;

namespace Windvale.Compiler;

public readonly record struct Sourceˉspan(
    int Start,
    int Length,
    int Line,
    int Column,
    string Sourceˉname = "");

public sealed record Compilerˉdiagnostic(
    string Code,
    string Phase,
    Sourceˉspan Span,
    string Message)
{
    public override string ToString()
    {
        var Location = string.IsNullOrEmpty(Span.Sourceˉname)
            ? $"({Span.Line},{Span.Column})"
            : $"{Span.Sourceˉname}({Span.Line},{Span.Column})";
        return $"{Code} {Phase} {Location}: {Message}";
    }
}

public sealed record Compilationˉresult(
    ImmutableArray<byte> Moduleˉbytes,
    ImmutableArray<Compilerˉdiagnostic> Diagnostics)
{
    public bool Success => Diagnostics.IsEmpty;
}

internal sealed class Diagnosticˉbag
{
    private readonly List<Compilerˉdiagnostic> Diagnostics = [];

    public int Count => Diagnostics.Count;

    public void Report(string code, string phase, Sourceˉspan span, string message)
    {
        Diagnostics.Add(new(code, phase, span, message));
    }

    public ImmutableArray<Compilerˉdiagnostic> Toˉimmutable()
    {
        return [.. Diagnostics
            .OrderBy(Diagnostic => Diagnostic.Span.Sourceˉname, StringComparer.Ordinal)
            .ThenBy(Diagnostic => Diagnostic.Span.Start)
            .ThenBy(Diagnostic => Diagnostic.Code, StringComparer.Ordinal)];
    }
}
