using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Nativeˉhostedˉcontainerˉsegmentˉconstructor
{
    internal const int CONSUMER_CANONICAL_SIZE = 22_584;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "d6d74f7d27df9f04f02b8eac2e75fde4fc230ba70d198f90b31ad668a06052e6";
    internal const int CONSUMER_ARTIFACT_SIZE = 286_727;
    internal const string CONSUMER_ARTIFACT_SHA256 =
        "923f7ff4552e0774e613d5805d8fbdbfff9edaa7347108d3d23626b68fe5dee7";

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
