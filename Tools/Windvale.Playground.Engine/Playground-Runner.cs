using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Runtime;

namespace Windvale.Playground;

public static class Playgroundˉrunner
{
    public static ImmutableHashSet<string> Availableˉcapabilities { get; } =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE);

    public static Playgroundˉresult Run(Playgroundˉrequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Source);

        if (request.Authorizedˉcapabilities is null)
        {
            throw new ArgumentNullException(nameof(request), "Authorized capabilities cannot be null.");
        }

        var Authorized = request.Authorizedˉcapabilities
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Requestˉdiagnostic = Validateˉrequest(request, Authorized);
        if (Requestˉdiagnostic is not null)
        {
            return Emptyˉresult(
                Playgroundˉstatus.Rejected,
                [Requestˉdiagnostic],
                Authorized);
        }

        var Compilation = Seedˉcompiler.Compile(request.Source, "playground.wv");
        if (!Compilation.Success)
        {
            return Emptyˉresult(
                Playgroundˉstatus.Compilationˉfailed,
                [.. Compilation.Diagnostics.Select(Diagnostic => new Playgroundˉdiagnostic(
                    Diagnostic.Code,
                    Diagnostic.Phase,
                    Diagnostic.Message,
                    Diagnostic.Span.Line,
                    Diagnostic.Span.Column))],
                Authorized);
        }

        Verifiedˉmodule Verified;
        try
        {
            Verified = Moduleˉcodec.Readˉandˉverify(Compilation.Moduleˉbytes.AsSpan());
        }
        catch (Bytecodeˉexception Exception)
        {
            return Emptyˉresult(
                Playgroundˉstatus.Verificationˉfailed,
                [new(Exception.Code, "verification", Exception.Message, Byteˉoffset: Exception.Byteˉoffset)],
                Authorized,
                Compilation.Moduleˉbytes);
        }

        var Module = Verified.Module;
        var Required = Module.Capabilities
            .Select(Capability => Capability.Name)
            .Order(StringComparer.Ordinal)
            .ToImmutableArray();
        var Digest = Moduleˉdigest.Calculateˉsha256(Compilation.Moduleˉbytes.AsSpan());
        var Report = Boundˉreport(Moduleˉinspector.Inspect(Verified, Compilation.Moduleˉbytes.AsSpan()));

        if (Module.Profile == Moduleˉprofile.System)
        {
            return Moduleˉresult(
                Playgroundˉstatus.Rejected,
                [new(
                    "WVPG1003",
                    "playground-profile",
                    "The browser playground does not execute system-profile modules.")],
                Compilation.Moduleˉbytes,
                Digest,
                Report,
                Module.Profile,
                Required,
                Authorized);
        }

        var Unsupported = Required.FirstOrDefault(Capability => !Availableˉcapabilities.Contains(Capability));
        if (Unsupported is not null)
        {
            return Moduleˉresult(
                Playgroundˉstatus.Rejected,
                [new(
                    "WVPG1004",
                    "playground-capability",
                    $"Capability '{Unsupported}' is not exposed by the browser playground.")],
                Compilation.Moduleˉbytes,
                Digest,
                Report,
                Module.Profile,
                Required,
                Authorized);
        }

        var Standardˉoutput = new Boundedˉtextˉwriter(Playgroundˉlimits.MAXIMUM_OUTPUT_UTF8_BYTES);
        var Diagnosticˉoutput = new Boundedˉtextˉwriter(Playgroundˉlimits.MAXIMUM_OUTPUT_UTF8_BYTES);
        var Resources = new Hostedˉresourceˉcontext([], Standardˉoutput, Diagnosticˉoutput);
        var Host = new Referenceˉcapabilityˉhost(Resources);

        try
        {
            var Runtime = new Referenceˉruntime(
                Verified,
                Host,
                new(
                    Authorized,
                    request.Maximumˉinstructions,
                    Playgroundˉlimits.MAXIMUM_CALL_DEPTH));
            var Result = Runtime.Runˉmain();
            return new(
                Playgroundˉstatus.Completed,
                Standardˉoutput.ToString(),
                Diagnosticˉoutput.ToString(),
                [],
                Compilation.Moduleˉbytes,
                Digest,
                Report,
                Module.Profile,
                Required,
                [.. Authorized.Order(StringComparer.Ordinal)],
                Result.Exitˉcode,
                Result.Executedˉinstructions);
        }
        catch (Runtimeˉexception Exception)
        {
            return new(
                Playgroundˉstatus.Runtimeˉfailed,
                Standardˉoutput.ToString(),
                Diagnosticˉoutput.ToString(),
                [new(Exception.Code, "runtime", Exception.Message)],
                Compilation.Moduleˉbytes,
                Digest,
                Report,
                Module.Profile,
                Required,
                [.. Authorized.Order(StringComparer.Ordinal)],
                null,
                null);
        }
    }

    private static Playgroundˉdiagnostic? Validateˉrequest(
        Playgroundˉrequest request,
        ImmutableHashSet<string> authorized)
    {
        if (request.Source.Length > Playgroundˉlimits.MAXIMUM_SOURCE_CHARACTERS)
        {
            return new(
                "WVPG1001",
                "playground-input",
                $"Source contains {request.Source.Length} characters; the playground limit is {Playgroundˉlimits.MAXIMUM_SOURCE_CHARACTERS}.");
        }

        var Unsupported = authorized.FirstOrDefault(Capability => !Availableˉcapabilities.Contains(Capability));
        if (Unsupported is not null)
        {
            return new(
                "WVPG1002",
                "playground-capability",
                $"Capability '{Unsupported}' cannot be authorized in the browser playground.");
        }

        if (request.Maximumˉinstructions <= 0 ||
            request.Maximumˉinstructions > Playgroundˉlimits.MAXIMUM_INSTRUCTIONS)
        {
            return new(
                "WVPG1005",
                "playground-limit",
                $"The instruction budget must be between 1 and {Playgroundˉlimits.MAXIMUM_INSTRUCTIONS}.");
        }

        return null;
    }

    private static Playgroundˉresult Emptyˉresult(
        Playgroundˉstatus status,
        ImmutableArray<Playgroundˉdiagnostic> diagnostics,
        ImmutableHashSet<string> authorized,
        ImmutableArray<byte> bytecodeˉbytes = default)
    {
        return new(
            status,
            string.Empty,
            string.Empty,
            diagnostics,
            bytecodeˉbytes.IsDefault ? [] : bytecodeˉbytes,
            null,
            null,
            null,
            [],
            [.. authorized.Order(StringComparer.Ordinal)],
            null,
            null);
    }

    private static Playgroundˉresult Moduleˉresult(
        Playgroundˉstatus status,
        ImmutableArray<Playgroundˉdiagnostic> diagnostics,
        ImmutableArray<byte> bytecodeˉbytes,
        string digest,
        string report,
        Moduleˉprofile profile,
        ImmutableArray<string> required,
        ImmutableHashSet<string> authorized)
    {
        return new(
            status,
            string.Empty,
            string.Empty,
            diagnostics,
            bytecodeˉbytes,
            digest,
            report,
            profile,
            required,
            [.. authorized.Order(StringComparer.Ordinal)],
            null,
            null);
    }

    private static string Boundˉreport(string report)
    {
        if (report.Length <= Playgroundˉlimits.MAXIMUM_BYTECODE_REPORT_CHARACTERS)
        {
            return report;
        }

        return string.Concat(
            report.AsSpan(0, Playgroundˉlimits.MAXIMUM_BYTECODE_REPORT_CHARACTERS),
            "\n[bytecode report truncated by playground]\n");
    }

    private sealed class Boundedˉtextˉwriter(int maximumˉutf8ˉbytes) : TextWriter
    {
        private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
        private readonly StringBuilder Buffer = new();
        private int Usedˉutf8ˉbytes;

        public override Encoding Encoding => STRICT_UTF8;

        public override void Write(char value)
        {
            Append(value.ToString());
        }

        public override void Write(string? value)
        {
            if (value is not null)
            {
                Append(value);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            Append(new string(buffer, index, count));
        }

        public override string ToString() => Buffer.ToString();

        private void Append(string value)
        {
            var Additionalˉbytes = STRICT_UTF8.GetByteCount(value);
            if (Additionalˉbytes > maximumˉutf8ˉbytes - Usedˉutf8ˉbytes)
            {
                throw new InvalidOperationException("The playground output limit was exceeded.");
            }

            Usedˉutf8ˉbytes += Additionalˉbytes;
            Buffer.Append(value);
        }
    }
}
