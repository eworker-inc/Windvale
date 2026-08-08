using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal sealed record Nativeˉserviceˉcode(
    Nativeˉservice Service,
    Nativeˉserviceˉadapter Adapter,
    ImmutableArray<byte> Code);

internal static class X64ˉnativeˉserviceˉbundleˉmaterialization
{
    public const int CONSUMER_CANONICAL_SIZE = 17_150;
    public const string CONSUMER_CANONICAL_SHA256 =
        "327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902";
    public const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 179_452;
    public const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "d0b12e426e891f6ee78209ab817dde7c547c0f68541750d39dd665607434e7a9";

    private const long MAXIMUM_INSTRUCTIONS = 1_000_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Service-Bundle-Materialization-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Materialize(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan) =>
        Nativeˉserviceˉbundleˉmaterializationˉsession.Build(
            fragment,
            services,
            plan);

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(X64ˉnativeˉserviceˉbundleˉmaterialization).Assembly
            .GetManifestResourceStream(CONSUMER_RESOURCE) ??
            throw Invalidˉconsumer();
        if (Stream.Length != CONSUMER_ARTIFACT_CANONICAL_SIZE)
        {
            throw Invalidˉconsumer();
        }
        var Bytes = new byte[CONSUMER_ARTIFACT_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CONSUMER_ARTIFACT_CANONICAL_SHA256))
        {
            throw Invalidˉconsumer();
        }
        var Fragment = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(Bytes);
        if (!Fragment.Requiredˉservices.IsEmpty ||
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment) != new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor))
        {
            throw Invalidˉconsumer();
        }
        return Fragment;
    }

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale service-bundle materializer failed its exact identity contract.");
}
