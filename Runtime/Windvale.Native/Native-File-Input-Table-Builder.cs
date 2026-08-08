using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉfileˉinputˉtableˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 5_084;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 52_334;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "378240d8f8770a4707d7f2ae86daae24036fc2eb9fd273d5ab737c9c03e3e70d";

    private const uint REQUEST_MAGIC = 0x514E_5657;
    private const uint RESPONSE_MAGIC = 0x524E_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_BYTES = 136;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 300_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-File-Input-Table-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(
        Nativeˉfileˉinputˉplatform platform,
        ulong snapshotˉtableˉpointer,
        ulong nameˉarenaˉpointer,
        ulong dataˉarenaˉpointer,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions)
    {
        var Request = Buildˉrequest(
            platform,
            snapshotˉtableˉpointer,
            nameˉarenaˉpointer,
            dataˉarenaˉpointer,
            scratchˉpointer,
            scratchˉbytes,
            functions);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(
            platform,
            snapshotˉtableˉpointer,
            nameˉarenaˉpointer,
            dataˉarenaˉpointer,
            scratchˉpointer,
            scratchˉbytes,
            functions,
            Request.Length,
            Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉfileˉinputˉplatform platform,
        ulong snapshotˉtableˉpointer,
        ulong nameˉarenaˉpointer,
        ulong dataˉarenaˉpointer,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions)
    {
        if (functions.IsDefault || functions.Length != 7)
        {
            throw new ArgumentException("The native file-input function list must contain seven entries.",
                nameof(functions));
        }
        var Result = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)platform);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), snapshotˉtableˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24), Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(32), nameˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(40), Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(48), dataˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(56), Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(60), Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(64), scratchˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(72), scratchˉbytes);
        for (var Index = 0; Index < functions.Length; Index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(80 + Index * sizeof(ulong)),
                functions[Index]);
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
        Nativeˉfileˉinputˉplatform platform,
        ulong snapshotˉtableˉpointer,
        ulong nameˉarenaˉpointer,
        ulong dataˉarenaˉpointer,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions,
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
                $"Windvale rejected the native file-input-table request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) !=
                Nativeˉfileˉinputˉtableˉcontract.SIZE ||
            response.Length != RESPONSE_HEADER_BYTES + Nativeˉfileˉinputˉtableˉcontract.SIZE)
        {
            throw Invalidˉresponse();
        }

        var Table = Span[RESPONSE_HEADER_BYTES..];
        if (BinaryPrimitives.ReadUInt32LittleEndian(Table) !=
                Nativeˉfileˉinputˉtableˉcontract.MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[4..]) !=
                Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[8..]) !=
                Nativeˉfileˉinputˉtableˉcontract.SIZE ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[12..]) != (uint)platform ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[16..]) != snapshotˉtableˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[24..]) !=
                Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[28..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[32..]) != nameˉarenaˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[40..]) !=
                Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[44..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[48..]) != dataˉarenaˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[56..]) !=
                Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[60..]) !=
                Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[64..]) != scratchˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[72..]) != scratchˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[76..]) != 0)
        {
            throw Invalidˉresponse();
        }
        for (var Index = 0; Index < functions.Length; Index++)
        {
            if (BinaryPrimitives.ReadUInt64LittleEndian(Table[(80 + Index * sizeof(ulong))..]) !=
                functions[Index])
            {
                throw Invalidˉresponse();
            }
        }
        return Table.ToArray().ToImmutableArray();
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉfileˉinputˉtableˉbuilder).Assembly
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
        new("The Windvale native file-input-table response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native file-input-table constructor failed its exact identity contract.");
}
