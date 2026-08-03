using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Runtime;

namespace Windvale.Playground;

public enum Playgroundˉwebassemblyˉloweringˉstatus
{
    Lowered,
    Unsupported,
    Failed,
}

public sealed record Playgroundˉwebassemblyˉloweringˉresult(
    Playgroundˉwebassemblyˉloweringˉstatus Status,
    ImmutableArray<byte> Webassemblyˉbytes,
    string? Webassemblyˉsha256,
    string? Selectorˉstatus,
    long? Loweringˉinstructions,
    string? Failure);

public static class Playgroundˉwebassemblyˉlowerer
{
    public const string BACKEND_WVB_SHA256 =
        "fddf873732a2979c4a5fac25c02d10f91dcc71d7920b77293aed62354bb3e0f7";
    public const int MAXIMUM_WEBASSEMBLY_BYTES = 1024 * 1024;
    public const long MAXIMUM_LOWERING_INSTRUCTIONS = 225_000_000;

    private const string INPUT_RESOURCE = "input.wvb";
    private const string OUTPUT_RESOURCE = "output.wasm";
    private const string STATUS_PREFIX = "webassembly status=";
    private static readonly Lazy<Verifiedˉmodule> BACKEND = new(
        Buildˉbackend,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Playgroundˉwebassemblyˉloweringˉresult Lower(
        ImmutableArray<byte> verifiedˉwvb)
    {
        if (verifiedˉwvb.IsDefault)
        {
            throw new ArgumentException(
                "WebAssembly lowering requires initialized WVB bytes.",
                nameof(verifiedˉwvb));
        }

        try
        {
            var Backend = BACKEND.Value;
            var Output = new StringWriter();
            var Diagnostics = new StringWriter();
            var Reader = new Inputˉreader(verifiedˉwvb);
            var Writer = new Outputˉwriter();
            var Authorized = Backend.Module.Capabilities
                .Select(Capability => Capability.Name)
                .ToImmutableHashSet(StringComparer.Ordinal);
            var Runtime = new Referenceˉruntime(
                Backend,
                new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                    [INPUT_RESOURCE, OUTPUT_RESOURCE],
                    Output,
                    Diagnostics,
                    Reader,
                    Writer)),
                new(
                    Authorized,
                    Maximumˉinstructions: MAXIMUM_LOWERING_INSTRUCTIONS,
                    Maximumˉcallˉdepth: Playgroundˉlimits.MAXIMUM_CALL_DEPTH));
            var Runtimeˉresult = Runtime.Runˉmain();

            if (Runtimeˉresult.Exitˉcode == 0)
            {
                if (Writer.Writeˉcount != 1 || Writer.Bytes.IsDefaultOrEmpty)
                {
                    return Failed("The Windvale backend completed without publishing one WebAssembly module.");
                }

                return new(
                    Playgroundˉwebassemblyˉloweringˉstatus.Lowered,
                    Writer.Bytes,
                    Moduleˉdigest.Calculateˉsha256(Writer.Bytes.AsSpan()),
                    "Valid",
                    Runtimeˉresult.Executedˉinstructions,
                    null);
            }

            var Selectorˉstatus = Readˉselectorˉstatus(Diagnostics.ToString());
            if (Runtimeˉresult.Exitˉcode == 1 &&
                Writer.Writeˉcount == 0 &&
                Selectorˉstatus is not null &&
                Selectorˉstatus != "Invalidˉwvb")
            {
                return new(
                    Playgroundˉwebassemblyˉloweringˉstatus.Unsupported,
                    [],
                    null,
                    Selectorˉstatus,
                    Runtimeˉresult.Executedˉinstructions,
                    null);
            }

            return Failed(
                $"The Windvale backend exited with {Runtimeˉresult.Exitˉcode}: " +
                Normalizeˉmessage(Diagnostics.ToString()));
        }
        catch (Exception Exception)
        {
            return Failed(Exception.Message);
        }
    }

    private static Verifiedˉmodule Buildˉbackend()
    {
        var Compilation = Seedˉcompiler.Compileˉmodules(
            new(
                "WebAssembly-Tool.wv",
                Readˉsource("Windvale.Playground.WebAssembly-Tool.wv")),
            [
                new(
                    "Compiler/Windvale/WebAssembly-Core.wv",
                    Readˉsource("Windvale.Playground.WebAssembly-Core.wv")),
            ]);
        if (!Compilation.Success)
        {
            throw new InvalidOperationException(
                "The embedded Windvale WebAssembly backend did not compile: " +
                string.Join(" | ", Compilation.Diagnostics));
        }

        var Digest = Moduleˉdigest.Calculateˉsha256(Compilation.Moduleˉbytes.AsSpan());
        if (Digest != BACKEND_WVB_SHA256)
        {
            throw new InvalidOperationException(
                $"The embedded Windvale WebAssembly backend digest is {Digest}; expected {BACKEND_WVB_SHA256}.");
        }

        return Moduleˉcodec.Readˉandˉverify(Compilation.Moduleˉbytes.AsSpan());
    }

    private static string Readˉsource(string resourceˉname)
    {
        using var Stream = typeof(Playgroundˉwebassemblyˉlowerer)
            .Assembly
            .GetManifestResourceStream(resourceˉname) ??
            throw new InvalidOperationException(
                $"Embedded WebAssembly backend source '{resourceˉname}' is missing.");
        using var Reader = new StreamReader(
            Stream,
            new UTF8Encoding(false, true),
            detectEncodingFromByteOrderMarks: true);
        return Reader.ReadToEnd();
    }

    private static string? Readˉselectorˉstatus(string diagnostics)
    {
        var Normalized = Normalizeˉmessage(diagnostics);
        return Normalized.StartsWith(STATUS_PREFIX, StringComparison.Ordinal)
            ? Normalized[STATUS_PREFIX.Length..]
            : null;
    }

    private static string Normalizeˉmessage(string message) =>
        message.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();

    private static Playgroundˉwebassemblyˉloweringˉresult Failed(string failure) =>
        new(
            Playgroundˉwebassemblyˉloweringˉstatus.Failed,
            [],
            null,
            null,
            null,
            failure);

    private sealed class Inputˉreader(ImmutableArray<byte> bytes) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (resourceˉname != INPUT_RESOURCE)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Invalidˉname,
                    $"The WebAssembly backend requested unexpected resource '{resourceˉname}'.");
            }
            if (bytes.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "The verified WVB exceeds the backend input limit.");
            }

            return bytes;
        }
    }

    private sealed class Outputˉwriter : IHostedˉfileˉwriter
    {
        public int Writeˉcount { get; private set; }

        public ImmutableArray<byte> Bytes { get; private set; } = [];

        public void Writeˉbytes(
            string resourceˉname,
            ImmutableArray<byte> bytes,
            int maximumˉbytes)
        {
            if (resourceˉname != OUTPUT_RESOURCE || Writeˉcount != 0)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Invalidˉname,
                    "The WebAssembly backend attempted an invalid output publication.");
            }
            if (bytes.IsDefault ||
                bytes.Length > maximumˉbytes ||
                bytes.Length > MAXIMUM_WEBASSEMBLY_BYTES)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "The WebAssembly backend output exceeds the playground limit.");
            }

            Writeˉcount = 1;
            Bytes = bytes;
        }
    }
}
