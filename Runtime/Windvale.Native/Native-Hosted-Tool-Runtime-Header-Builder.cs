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
    internal const int METADATA_ADMISSION_CANONICAL_SIZE = 10_779;
    internal const string METADATA_ADMISSION_CANONICAL_SHA256 =
        "c0c61a601085c9422e5391023e047d7b0b3c88cb49bd52b24ccfd7c3eb7f1e35";
    internal const int CORE_CANONICAL_SIZE = 19_330;
    internal const string CORE_CANONICAL_SHA256 =
        "abe7140c46af12c0f99056b8fbaf844f6ab5f51dd9e9f72bc215be86d977585f";
    internal const int CONSUMER_CANONICAL_SIZE = 19_273;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "d9add4e9608fa1b2b2771de4f29d91793920a8a4d8c6f191ba9a5b1c9bedf3fa";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 194_222;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "b726e57277ef81990856159fca428a45c3513ca9218dec5f9fdebc2531637e31";

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
            inputs.Profile is < 1 or > 7 ||
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
