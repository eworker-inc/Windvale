using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉentryˉbridgeˉinputs(
    Nativeˉentryˉinputˉkind Input,
    ulong Inputˉpointer,
    uint Inputˉlength);

internal readonly record struct Nativeˉentryˉresultˉdescriptor(
    ulong Pointer,
    uint Length,
    uint Reserved);

internal static class Nativeˉentryˉbridgeˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 3_401;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "d66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 37_374;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "2abde6462aa470f4037aa87ae486f16f2a106932d3022344e85fa5763d44623b";

    private const uint REQUEST_MAGIC = 0x514A_5657;
    private const uint RESPONSE_MAGIC = 0x524A_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_BYTES = 32;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 150_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Entry-Bridge-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(Nativeˉentryˉbridgeˉinputs inputs)
    {
        var Request = Buildˉrequest(inputs);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(inputs, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉentryˉbridgeˉinputs inputs)
    {
        Verifyˉinputs(inputs);
        var Result = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            inputs.Input == Nativeˉentryˉinputˉkind.Bytes ? 1u : 0u);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), inputs.Inputˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), inputs.Inputˉlength);
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        Nativeˉentryˉbridgeˉinputs inputs,
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
                $"Windvale rejected the native entry-bridge request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        var Bridgeˉbytes = inputs.Input == Nativeˉentryˉinputˉkind.Bytes
            ? 2 * Nativeˉcontract.VALUE_SLOT_BYTES
            : Nativeˉcontract.VALUE_SLOT_BYTES;
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != Bridgeˉbytes ||
            response.Length != RESPONSE_HEADER_BYTES + Bridgeˉbytes)
        {
            throw Invalidˉresponse();
        }

        var Bridge = Span[RESPONSE_HEADER_BYTES..];
        Verifyˉbridgeˉbytes(inputs, Bridge);
        return Bridge.ToArray().ToImmutableArray();
    }

    internal static void Verifyˉinputs(Nativeˉentryˉbridgeˉinputs inputs)
    {
        if (inputs.Input == Nativeˉentryˉinputˉkind.None)
        {
            if (inputs.Inputˉpointer != 0 || inputs.Inputˉlength != 0)
            {
                throw Invalidˉinputs();
            }
            return;
        }
        if (inputs.Input != Nativeˉentryˉinputˉkind.Bytes ||
            inputs.Inputˉpointer == 0 ||
            inputs.Inputˉlength > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw Invalidˉinputs();
        }
    }

    internal static void Verifyˉbridgeˉbytes(
        Nativeˉentryˉbridgeˉinputs inputs,
        ReadOnlySpan<byte> bridge)
    {
        Verifyˉinputs(inputs);
        var Expectedˉbytes = inputs.Input == Nativeˉentryˉinputˉkind.Bytes
            ? 2 * Nativeˉcontract.VALUE_SLOT_BYTES
            : Nativeˉcontract.VALUE_SLOT_BYTES;
        if (bridge.Length != Expectedˉbytes ||
            !bridge[..Nativeˉcontract.VALUE_SLOT_BYTES].SequenceEqual(
                new byte[Nativeˉcontract.VALUE_SLOT_BYTES]))
        {
            throw Invalidˉresponse();
        }
        if (inputs.Input == Nativeˉentryˉinputˉkind.Bytes)
        {
            var Descriptor = bridge[Nativeˉcontract.VALUE_SLOT_BYTES..];
            if (BinaryPrimitives.ReadUInt64LittleEndian(Descriptor) != inputs.Inputˉpointer ||
                BinaryPrimitives.ReadUInt32LittleEndian(Descriptor[8..]) != inputs.Inputˉlength ||
                BinaryPrimitives.ReadUInt32LittleEndian(Descriptor[12..]) != 0)
            {
                throw Invalidˉresponse();
            }
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉentryˉbridgeˉbuilder).Assembly
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

    private static ArgumentException Invalidˉinputs() =>
        new("The native entry bridge inputs are invalid.");

    private static InvalidOperationException Invalidˉresponse() =>
        new("The Windvale native entry-bridge response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native entry-bridge constructor failed its exact identity contract.");
}
