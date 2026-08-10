using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉhostedˉtoolˉmetadataˉinputs(
    uint Target,
    uint Profile,
    uint Bundleˉoffset,
    uint Nativeˉentryˉoffset,
    Nativeˉserviceˉbundle Bundle);

internal static class Nativeˉhostedˉtoolˉmetadataˉbuilder
{
    internal const int CORE_CANONICAL_SIZE = 24_174;
    internal const string CORE_CANONICAL_SHA256 =
        "ebe76d7ccc4b27f7d7135647ef4c4b11ec5d83f281c7c91cc7ca68e389c0c1fa";
    internal const int CONSUMER_CANONICAL_SIZE = 24_066;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "4adb36dd4ce821abdd29fe5766a0866847dc9052a0035dfbabd51d6a6b7c19ab";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 215_031;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "34f8d5f2f65db0fd736d6bd3557b5b9f3dcb4449366cde10e486ec97e15fbbad";

    private const uint REQUEST_MAGIC = 0x4D48_5657;
    private const uint RESPONSE_MAGIC = 0x4448_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 96;
    private const int REQUEST_PLACEMENT_BYTES = 48;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const int METADATA_BYTES = 1024;
    private const int MAXIMUM_NATIVE_BYTES = 32 * 1024 * 1024;
    private const int MAXIMUM_BUNDLE_BYTES = 34 * 1024 * 1024;
    private const int SERVICE_COUNT = 10;
    private const long MAXIMUM_INSTRUCTIONS = 15_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Hosted-Tool-Metadata-Construction-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static ImmutableArray<byte> Build(Nativeˉhostedˉtoolˉmetadataˉinputs inputs)
    {
        var Request = Buildˉrequest(inputs);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(inputs, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉhostedˉtoolˉmetadataˉinputs inputs)
    {
        Verifyˉinputs(inputs);
        var Result = new byte[
            REQUEST_HEADER_BYTES + inputs.Bundle.Placements.Length * REQUEST_PLACEMENT_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), inputs.Target);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), inputs.Profile);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), inputs.Bundleˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24),
            checked((uint)inputs.Bundle.Imageˉbytes.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(28),
            checked((uint)inputs.Bundle.Nativeˉimageˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(32), inputs.Nativeˉentryˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(36),
            checked((uint)inputs.Bundle.Placements.Length));
        SHA256.HashData(inputs.Bundle.Imageˉbytes.AsSpan(0, inputs.Bundle.Nativeˉimageˉbytes))
            .CopyTo(Result.AsSpan(40, 32));
        for (var Index = 0; Index < inputs.Bundle.Placements.Length; Index++)
        {
            var Placement = inputs.Bundle.Placements[Index];
            var Offset = REQUEST_HEADER_BYTES + Index * REQUEST_PLACEMENT_BYTES;
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset),
                checked((uint)Placement.Imageˉoffset));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                checked((uint)Placement.Codeˉbytes));
            Convert.FromHexString(Placement.Sha256).CopyTo(Result.AsSpan(Offset + 8, 32));
        }
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        Nativeˉhostedˉtoolˉmetadataˉinputs inputs,
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
                $"Windvale rejected the hosted-tool metadata request with status " +
                    $"{Status} at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != METADATA_BYTES ||
            response.Length != RESPONSE_HEADER_BYTES + METADATA_BYTES)
        {
            throw Invalidˉresponse();
        }
        return Span[RESPONSE_HEADER_BYTES..].ToArray().ToImmutableArray();
    }

    internal static void Verifyˉinputs(Nativeˉhostedˉtoolˉmetadataˉinputs inputs)
    {
        var Bundle = inputs.Bundle;
        if (inputs.Target is < 1 or > 2 ||
            inputs.Profile is < 1 or > 7 ||
            inputs.Bundleˉoffset != 4096 ||
            Bundle is null ||
            (uint)Bundle.Platform != inputs.Target ||
            Bundle.Nativeˉimageˉbytes is < 1 or > MAXIMUM_NATIVE_BYTES ||
            Bundle.Imageˉbytes.IsDefaultOrEmpty ||
            Bundle.Imageˉbytes.Length > MAXIMUM_BUNDLE_BYTES ||
            Bundle.Nativeˉimageˉbytes > Bundle.Imageˉbytes.Length ||
            inputs.Nativeˉentryˉoffset >= Bundle.Nativeˉimageˉbytes ||
            Bundle.Placements.IsDefault ||
            Bundle.Placements.Length != SERVICE_COUNT ||
            Bundle.Placements.Any(Placement =>
                Placement is null ||
                Placement.Imageˉoffset < Bundle.Nativeˉimageˉbytes ||
                Placement.Codeˉbytes < 1 ||
                Placement.Imageˉoffset > Bundle.Imageˉbytes.Length ||
                Placement.Codeˉbytes > Bundle.Imageˉbytes.Length - Placement.Imageˉoffset ||
                String.IsNullOrEmpty(Placement.Sha256) ||
                Placement.Sha256.Length != 64 ||
                Placement.Sha256.Any(Character => !Uri.IsHexDigit(Character))))
        {
            throw new ArgumentException("The hosted-tool metadata inputs are invalid.");
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉhostedˉtoolˉmetadataˉbuilder).Assembly
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
        new("The Windvale hosted-tool metadata response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale hosted-tool metadata constructor failed its exact identity contract.");
}
