using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉbyteˉresultˉrange(
    ulong Start,
    uint Available);

internal readonly record struct Nativeˉbyteˉresultˉadmissionˉinputs(
    Nativeˉentryˉresultˉdescriptor Descriptor,
    ulong Arenaˉstart,
    uint Arenaˉused,
    ulong Inputˉstart,
    uint Inputˉlength,
    ImmutableArray<Nativeˉbyteˉresultˉrange> Staticˉranges);

internal static class Nativeˉbyteˉresultˉadmissionˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 7_057;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 68_608;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "35c29fa9bbc41a00e8797f7812eb1bbf0f95c7f07b96227ca666cc5bf8fd38c2";

    private const uint REQUEST_MAGIC = 0x5152_5657;
    private const uint RESPONSE_MAGIC = 0x5252_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 64;
    private const int REQUEST_RANGE_BYTES = 16;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 5_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Byte-Result-Admission-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool Admit(Nativeˉbyteˉresultˉadmissionˉinputs inputs)
    {
        var Request = Buildˉrequest(inputs);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(inputs, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉbyteˉresultˉadmissionˉinputs inputs)
    {
        Verifyˉinputs(inputs);
        var Result = new byte[checked(
            REQUEST_HEADER_BYTES + inputs.Staticˉranges.Length * REQUEST_RANGE_BYTES)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            checked((uint)inputs.Staticˉranges.Length));
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), inputs.Descriptor.Pointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), inputs.Descriptor.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), inputs.Descriptor.Reserved);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(32), inputs.Arenaˉstart);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(40), inputs.Arenaˉused);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(48), inputs.Inputˉstart);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(56), inputs.Inputˉlength);
        for (var Index = 0; Index < inputs.Staticˉranges.Length; Index++)
        {
            var Offset = REQUEST_HEADER_BYTES + Index * REQUEST_RANGE_BYTES;
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(Offset),
                inputs.Staticˉranges[Index].Start);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 8),
                inputs.Staticˉranges[Index].Available);
        }
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static bool Verifyˉresponse(
        Nativeˉbyteˉresultˉadmissionˉinputs inputs,
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
        var Descriptorˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]);
        if (Status is 5 or 9)
        {
            if (response.Length != RESPONSE_HEADER_BYTES ||
                Failureˉoffset != 16 || Descriptorˉbytes != 0)
            {
                throw Invalidˉresponse();
            }
            return false;
        }
        if (Status != 0)
        {
            throw new InvalidOperationException(
                $"Windvale rejected the native byte-result admission request with status " +
                    $"{Status} at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            Descriptorˉbytes != Nativeˉcontract.VALUE_SLOT_BYTES ||
            response.Length != RESPONSE_HEADER_BYTES + Nativeˉcontract.VALUE_SLOT_BYTES)
        {
            throw Invalidˉresponse();
        }
        var Descriptor = Span[RESPONSE_HEADER_BYTES..];
        if (BinaryPrimitives.ReadUInt64LittleEndian(Descriptor) != inputs.Descriptor.Pointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(Descriptor[8..]) != inputs.Descriptor.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(Descriptor[12..]) != inputs.Descriptor.Reserved)
        {
            throw Invalidˉresponse();
        }
        return true;
    }

    internal static void Verifyˉinputs(
        Nativeˉbyteˉresultˉadmissionˉinputs inputs)
    {
        if (inputs.Arenaˉstart == 0 ||
            inputs.Arenaˉused > Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES ||
            (inputs.Inputˉstart == 0 && inputs.Inputˉlength != 0) ||
            inputs.Inputˉlength > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES ||
            inputs.Staticˉranges.IsDefault ||
            inputs.Staticˉranges.Length > Objectˉlimits.MAX_SYMBOLS ||
            inputs.Staticˉranges.Any(Range =>
                Range.Start == 0 || Range.Available > Nativeˉcontract.MAXIMUM_CODE_BYTES))
        {
            throw new ArgumentException("The native byte-result admission inputs are invalid.");
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉbyteˉresultˉadmissionˉbuilder).Assembly
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
        new("The Windvale native byte-result admission response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native byte-result admission fragment failed its exact identity contract.");
}
