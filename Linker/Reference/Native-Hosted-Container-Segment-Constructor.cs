using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Nativeˉhostedˉcontainerˉsegmentˉconstructor
{
    internal const int CONSUMER_CANONICAL_SIZE = 21_832;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "af869ba326f99eaa8d1a2c0898c14145a62c4f046da7bbcccf511d7918e79056";
    internal const int CONSUMER_ARTIFACT_SIZE = 281_719;
    internal const string CONSUMER_ARTIFACT_SHA256 =
        "ab96ecad8d37f9383626d24c2e97c7e6615dd3c92c2ed5f9dc816cf77f3dc7d7";

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
