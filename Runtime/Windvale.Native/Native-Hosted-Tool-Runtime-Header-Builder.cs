using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs(
    uint Target,
    uint Profile,
    ImmutableArray<byte> Metadata);

internal static class Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder
{
    internal const int METADATA_ADMISSION_CANONICAL_SIZE = 10_550;
    internal const string METADATA_ADMISSION_CANONICAL_SHA256 =
        "e43c712431e386eba159cd17f87b279cc4a4b5b99084d3a738a3718633099c78";
    internal const int CORE_CANONICAL_SIZE = 18_911;
    internal const string CORE_CANONICAL_SHA256 =
        "700efbbad9619b58d06561be3e805e18b5498f1e13881646e6e121c2b8ab7564";
    internal const int CONSUMER_CANONICAL_SIZE = 18_864;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "0bbf1c0e5c67c14b3e90bef5243d9c5aea64b3343ad11cfd3f7f93067648fe3d";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 190_709;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "31e7b98c738972b4f9b23075d48bb1724aac229e5f77d8e517877b5b5733dfe4";

    private const uint REQUEST_MAGIC = 0x5248_5657;
    private const uint RESPONSE_MAGIC = 0x5348_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 24;
    private const int METADATA_BYTES = 1024;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const int RUNTIME_HEADER_BYTES = 4096;
    private const int RUNTIME_METADATA_OFFSET = 480;
    private const long MAXIMUM_INSTRUCTIONS = 10_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Hosted-Tool-Runtime-Header-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static ImmutableArray<byte> Build(
        Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs inputs)
    {
        var Request = Buildˉrequest(inputs);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(inputs, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs inputs)
    {
        Verifyˉinputs(inputs);
        var Result = new byte[REQUEST_HEADER_BYTES + METADATA_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), inputs.Target);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), inputs.Profile);
        inputs.Metadata.CopyTo(Result, REQUEST_HEADER_BYTES);
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs inputs,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        Verifyˉinputs(inputs);
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse();
        }
        var Span = response.AsSpan();
        if (BinaryPrimitives.ReadUInt32LittleEndian(Span) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[4..]) != FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[8..]) != response.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[24..]) != 0 ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[28..]) != 0)
        {
            throw Invalidˉresponse();
        }
        var Status = BinaryPrimitives.ReadUInt32LittleEndian(Span[12..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Span[16..]);
        if (Status != 0)
        {
            throw new InvalidOperationException(
                $"Windvale rejected the hosted-tool runtime-header request with status " +
                    $"{Status} at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != RUNTIME_HEADER_BYTES ||
            response.Length != RESPONSE_HEADER_BYTES + RUNTIME_HEADER_BYTES)
        {
            throw Invalidˉresponse();
        }

        var Header = Span[RESPONSE_HEADER_BYTES..];
        if (!Header.Slice(RUNTIME_METADATA_OFFSET, METADATA_BYTES)
                .SequenceEqual(inputs.Metadata.AsSpan()))
        {
            throw Invalidˉresponse();
        }
        return Header.ToArray().ToImmutableArray();
    }

    internal static void Verifyˉinputs(
        Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs inputs)
    {
        if (inputs.Target is < 1 or > 2 ||
            inputs.Profile is < 1 or > 6 ||
            inputs.Metadata.IsDefault ||
            inputs.Metadata.Length != METADATA_BYTES)
        {
            throw new ArgumentException("The hosted-tool runtime-header inputs are invalid.");
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder).Assembly
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

    private static InvalidOperationException Invalidˉresponse() =>
        new("The Windvale hosted-tool runtime-header response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale hosted-tool runtime-header constructor failed its exact identity contract.");
}
