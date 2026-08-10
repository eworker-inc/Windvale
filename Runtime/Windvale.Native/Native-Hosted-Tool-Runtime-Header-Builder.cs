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
    internal const int METADATA_ADMISSION_CANONICAL_SIZE = 10_872;
    internal const string METADATA_ADMISSION_CANONICAL_SHA256 =
        "d7b0084ed2c69ee03ad65ee4bfffa72550fd8d9ef2889efa0be116350b80b8b5";
    internal const int CORE_CANONICAL_SIZE = 19_516;
    internal const string CORE_CANONICAL_SHA256 =
        "f1c156def9fa6f00bb0401097435bb1d1429d9d4be247b8d11f0de0b5ea51be2";
    internal const int CONSUMER_CANONICAL_SIZE = 19_459;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "3cc8d0850b888911ee3338600bc7699578b163e7400c2b3631ef14649b9a3f18";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 195_394;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "91590986b8c3421ffdca9ecffb8a1798718f868614b77c581c266f4a2061b632";

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
