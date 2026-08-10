using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Nativeˉhostedˉcontainerˉsegmentˉconstructor
{
    internal const int CONSUMER_CANONICAL_SIZE = 22_398;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "83e6945d99a9a006e64572bf43b6affdf70626f9454145935d52193f8e692369";
    internal const int CONSUMER_ARTIFACT_SIZE = 285_555;
    internal const string CONSUMER_ARTIFACT_SHA256 =
        "6e4351c1e8cc62b67721d4b61f5374f11fded3cf84b79ce63930c33a10e40d43";

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
