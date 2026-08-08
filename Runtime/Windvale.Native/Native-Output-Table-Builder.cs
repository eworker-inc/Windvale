using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉoutputˉtableˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 4_714;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 50_493;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "f444e80b2afbaaee251892ab7a7a6a879b3e5cffcbf029b0fc382b64bef97afb";

    private const uint REQUEST_MAGIC = 0x5149_5657;
    private const uint RESPONSE_MAGIC = 0x5249_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_BYTES = 48;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 250_000;
    private const string CONSUMER_RESOURCE = "Windvale.Native.Native-Output-Table-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(
        Nativeˉoutputˉplatform platform,
        uint flags,
        ulong consoleˉtarget,
        ulong diagnosticˉtarget,
        ulong writeˉfunction)
    {
        var Request = Buildˉrequest(
            platform,
            flags,
            consoleˉtarget,
            diagnosticˉtarget,
            writeˉfunction);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(
            platform,
            flags,
            consoleˉtarget,
            diagnosticˉtarget,
            writeˉfunction,
            Request.Length,
            Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉoutputˉplatform platform,
        uint flags,
        ulong consoleˉtarget,
        ulong diagnosticˉtarget,
        ulong writeˉfunction)
    {
        var Result = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)platform);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), flags);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(24), consoleˉtarget);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(32), diagnosticˉtarget);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(40), writeˉfunction);
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        Nativeˉoutputˉplatform platform,
        uint flags,
        ulong consoleˉtarget,
        ulong diagnosticˉtarget,
        ulong writeˉfunction,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
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
                $"Windvale rejected the native output-table request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) !=
                Nativeˉoutputˉtableˉcontract.SIZE ||
            response.Length != RESPONSE_HEADER_BYTES + Nativeˉoutputˉtableˉcontract.SIZE)
        {
            throw Invalidˉresponse();
        }

        var Table = Span[RESPONSE_HEADER_BYTES..];
        if (BinaryPrimitives.ReadUInt32LittleEndian(Table) !=
                Nativeˉoutputˉtableˉcontract.MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[4..]) !=
                Nativeˉoutputˉtableˉcontract.FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[8..]) !=
                Nativeˉoutputˉtableˉcontract.SIZE ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[12..]) != (uint)platform ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[16..]) != flags ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[20..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[24..]) != consoleˉtarget ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[32..]) != diagnosticˉtarget ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[40..]) != writeˉfunction)
        {
            throw Invalidˉresponse();
        }
        return Table.ToArray().ToImmutableArray();
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉoutputˉtableˉbuilder).Assembly
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
        new("The Windvale native output-table response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native output-table constructor failed its exact identity contract.");
}
