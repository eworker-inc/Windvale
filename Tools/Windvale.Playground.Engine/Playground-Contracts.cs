using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Playground;

public enum Playgroundˉstatus
{
    Rejected,
    Compilationˉfailed,
    Verificationˉfailed,
    Runtimeˉfailed,
    Completed,
}

public sealed record Playgroundˉrequest(
    string Source,
    ImmutableHashSet<string> Authorizedˉcapabilities,
    long Maximumˉinstructions = Playgroundˉlimits.DEFAULT_MAXIMUM_INSTRUCTIONS);

public sealed record Playgroundˉdiagnostic(
    string Code,
    string Phase,
    string Message,
    int? Line = null,
    int? Column = null,
    int? Byteˉoffset = null);

public sealed record Playgroundˉresult(
    Playgroundˉstatus Status,
    string Standardˉoutput,
    string Diagnosticˉoutput,
    ImmutableArray<Playgroundˉdiagnostic> Diagnostics,
    ImmutableArray<byte> Bytecodeˉbytes,
    string? Moduleˉsha256,
    string? Bytecodeˉreport,
    Moduleˉprofile? Profile,
    ImmutableArray<string> Requiredˉcapabilities,
    ImmutableArray<string> Authorizedˉcapabilities,
    int? Exitˉcode,
    long? Executedˉinstructions);

public sealed record Playgroundˉexample(
    string Id,
    string Title,
    string Description,
    string Source,
    ImmutableHashSet<string> Recommendedˉcapabilities);

public static class Playgroundˉlimits
{
    public const int MAXIMUM_SOURCE_CHARACTERS = 64 * 1024;
    public const long DEFAULT_MAXIMUM_INSTRUCTIONS = 250_000;
    public const long MAXIMUM_INSTRUCTIONS = 1_000_000;
    public const int MAXIMUM_CALL_DEPTH = 128;
    public const int MAXIMUM_OUTPUT_UTF8_BYTES = 64 * 1024;
    public const int MAXIMUM_BYTECODE_REPORT_CHARACTERS = 256 * 1024;
}
