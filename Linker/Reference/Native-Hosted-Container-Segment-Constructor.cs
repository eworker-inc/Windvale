using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Nativeˉhostedˉcontainerˉsegmentˉconstructor
{
    internal const int CONSUMER_CANONICAL_SIZE = 21_806;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "c1c446d22e578eac330a0bead108d4d759b7c346c48c335601df62e19538bca4";
    internal const int CONSUMER_ARTIFACT_SIZE = 278_243;
    internal const string CONSUMER_ARTIFACT_SHA256 =
        "f80570a216cbf99e04b83f8e5c8f576f0f8f9d179fdc907715b7f80a57e43c3a";

    private const long MAXIMUM_INSTRUCTIONS = 1_000_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Linker.Native-Hosted-Container-Segmentation.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static ImmutableArray<byte> Execute(ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉhostedˉcontainerˉsegmentˉconstructor).Assembly
            .GetManifestResourceStream(CONSUMER_RESOURCE) ?? throw Invalid();
        if (Stream.Length != CONSUMER_ARTIFACT_SIZE)
        {
            throw Invalid();
        }
        var Bytes = new byte[CONSUMER_ARTIFACT_SIZE];
        Stream.ReadExactly(Bytes);
        if (!StringComparer.Ordinal.Equals(
            Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant(),
            CONSUMER_ARTIFACT_SHA256))
        {
            throw Invalid();
        }
        var Fragment = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(Bytes);
        if (!Fragment.Requiredˉservices.IsEmpty ||
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment) != new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor))
        {
            throw Invalid();
        }
        return Fragment;
    }

    private static InvalidOperationException Invalid() =>
        new("The retained Windvale hosted-container segment constructor is invalid.");
}
