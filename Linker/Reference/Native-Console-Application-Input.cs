using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

internal enum Nativeˉconsoleˉapplicationˉinputˉfailure : byte
{
    None = 0,
    Unverifiedˉfragment = 1,
    Unsupportedˉentry = 2,
    Linkˉfailure = 3,
}

internal sealed record Nativeˉconsoleˉapplicationˉinput(
    ImmutableArray<byte> Imageˉbytes,
    uint Entryˉoffset,
    ImmutableArray<Nativeˉservice> Requiredˉservices);

internal sealed record Nativeˉconsoleˉapplicationˉinputˉresult(
    Nativeˉconsoleˉapplicationˉinput? Input,
    Nativeˉconsoleˉapplicationˉinputˉfailure Failure,
    string Message)
{
    public bool Success => Input is not null && Failure == Nativeˉconsoleˉapplicationˉinputˉfailure.None;
}

internal static class Nativeˉconsoleˉapplicationˉpreparer
{
    public static Nativeˉconsoleˉapplicationˉinputˉresult Prepare(Nativeˉfragment fragment)
        => Prepare(fragment, hostedˉconsole: false);

    public static Nativeˉconsoleˉapplicationˉinputˉresult Prepareˉhostedˉconsole(
        Nativeˉfragment fragment) =>
        Prepare(fragment, hostedˉconsole: true);

    private static Nativeˉconsoleˉapplicationˉinputˉresult Prepare(
        Nativeˉfragment fragment,
        bool hostedˉconsole)
    {
        Nativeˉentryˉresultˉkind Entryˉresult;
        try
        {
            Entryˉresult = Nativeˉfragmentˉverifier.Verifyˉentryˉresultˉkind(fragment);
        }
        catch (Exception Exception) when (
            Exception is ArgumentNullException or Nativeˉbackendˉexception)
        {
            return Failed(
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unverifiedˉfragment,
                $"The application target requires a verified native fragment: {Exception.Message}");
        }

        var Requiredˉservicesˉvalid = hostedˉconsole
            ? fragment.Requiredˉservices.SequenceEqual([Nativeˉservice.Consoleˉwriteˉline])
            : fragment.Requiredˉservices.IsEmpty;
        if (Entryˉresult != Nativeˉentryˉresultˉkind.Scalar || !Requiredˉservicesˉvalid)
        {
            return Failed(
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unsupportedˉentry,
                hostedˉconsole
                    ? "Hosted console version 2 requires a scalar Main entry and exactly console.write_line."
                    : "Version 1 requires a capability-free scalar Main entry and no runtime services.");
        }

        var Entry = fragment.Symbols.SingleOrDefault(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            StringComparer.Ordinal.Equals(Symbol.Name, "Main"));
        if (Entry is null || Entry.Size == 0)
        {
            return Failed(
                Nativeˉconsoleˉapplicationˉinputˉfailure.Unsupportedˉentry,
                hostedˉconsole
                    ? "Hosted console version 2 requires one non-empty exported Main function."
                    : "Version 1 requires one non-empty exported Main function.");
        }

        Linkˉresult Link;
        try
        {
            var Objectˉbytes = Nativeˉobjectˉsink.Writeˉwvo(fragment);
            Link = Linkˉcompiler.Link(
                [new(Objectˉbytes)],
                new(0, "Main"));
        }
        catch (Exception Exception) when (
            Exception is Nativeˉbackendˉexception or
                ObjectModel.Objectˉexception or
                OverflowException)
        {
            return Failed(
                Nativeˉconsoleˉapplicationˉinputˉfailure.Linkˉfailure,
                $"The native fragment could not enter the bounded WVO/AOT path: {Exception.Message}");
        }

        if (!Link.Success ||
            Link.Baseˉaddress != 0 ||
            Link.Sectionˉcount < 1 ||
            Link.Codeˉsectionˉcount < 1 ||
            Link.Codeˉsectionˉcount + Link.Readˉonlyˉsectionˉcount != Link.Sectionˉcount ||
            Link.Absoluteˉrelocationˉcount != 0 ||
            Link.Relativeˉrelocationˉcount != Link.Relocationˉcount ||
            Link.Entryˉaddress != Entry.Offset ||
            !Link.Imageˉbytes.AsSpan().SequenceEqual(fragment.Code.AsSpan()))
        {
            var Detail = Link.Diagnostics.IsEmpty
                ? "The linked image did not reproduce the verified fragment and entry."
                : Link.Diagnostics[0].Message;
            return Failed(Nativeˉconsoleˉapplicationˉinputˉfailure.Linkˉfailure, Detail);
        }

        return new(
            new(Link.Imageˉbytes, Link.Entryˉaddress, fragment.Requiredˉservices),
            Nativeˉconsoleˉapplicationˉinputˉfailure.None,
            string.Empty);
    }

    private static Nativeˉconsoleˉapplicationˉinputˉresult Failed(
        Nativeˉconsoleˉapplicationˉinputˉfailure failure,
        string message) =>
        new(null, failure, message);
}

public static class Nativeˉconsoleˉapplicationˉcontract
{
    public const uint MAXIMUM_PORTABLE_PROCESS_RESULT = 255;
    public const uint FAILURE_PROCESS_RESULT = 1;
    public const uint RECORD_ARENA_BYTES = 2 * 1024 * 1024;
    public const uint TEXT_ARENA_BYTES = 16 * 1024 * 1024;
    public const uint DATA_VIRTUAL_BYTES =
        112 + RECORD_ARENA_BYTES + TEXT_ARENA_BYTES;
    public const ulong STACK_BYTES = 64UL * 1024 * 1024;
    public const uint HOSTED_DATA_HEADER_BYTES = 1024;
    public const uint HOSTED_TEXT_ARENA_BYTES = 64 * 1024 * 1024;
    public const uint HOSTED_DATA_VIRTUAL_BYTES =
        HOSTED_DATA_HEADER_BYTES + RECORD_ARENA_BYTES + HOSTED_TEXT_ARENA_BYTES;
    public const uint HOSTED_SERVICE_TABLE_OFFSET = 112;
    public const uint HOSTED_OUTPUT_TABLE_OFFSET = 216;
    public const uint HOSTED_METADATA_OFFSET = 272;
    public const uint HOSTED_RECORD_ARENA_OFFSET = HOSTED_DATA_HEADER_BYTES;
    public const uint HOSTED_TEXT_ARENA_OFFSET =
        HOSTED_RECORD_ARENA_OFFSET + RECORD_ARENA_BYTES;
}
