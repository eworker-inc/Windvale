using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉfileˉoutputˉtableˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 3_930;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 42_302;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "9333d4573b87b829e6e577d8a27c937bf2fb433a93d4a4b11b783b372d31d08a";

    private const uint REQUEST_MAGIC = 0x5146_5657;
    private const uint RESPONSE_MAGIC = 0x5246_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_BYTES = 80;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 250_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-File-Output-Table-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(
        Nativeˉfileˉinputˉplatform platform,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions)
    {
        var Request = Buildˉrequest(platform, scratchˉpointer, scratchˉbytes, functions);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(
            platform,
            scratchˉpointer,
            scratchˉbytes,
            functions,
            Request.Length,
            Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉfileˉinputˉplatform platform,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions)
    {
        if (functions.IsDefault || functions.Length != 6)
        {
            throw new ArgumentException("The native file-output function list must contain six entries.",
                nameof(functions));
        }
        var Result = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)platform);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), scratchˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), scratchˉbytes);
        for (var Index = 0; Index < functions.Length; Index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(32 + Index * sizeof(ulong)),
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
                $"Windvale rejected the native file-output-table request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) !=
                Nativeˉfileˉoutputˉtableˉcontract.SIZE ||
            response.Length !=
                RESPONSE_HEADER_BYTES + Nativeˉfileˉoutputˉtableˉcontract.SIZE)
        {
            throw Invalidˉresponse();
        }

        var Table = Span[RESPONSE_HEADER_BYTES..];
        if (BinaryPrimitives.ReadUInt32LittleEndian(Table) !=
                Nativeˉfileˉoutputˉtableˉcontract.MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[4..]) !=
                Nativeˉfileˉoutputˉtableˉcontract.FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[8..]) !=
                Nativeˉfileˉoutputˉtableˉcontract.SIZE ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[12..]) != (uint)platform ||
            BinaryPrimitives.ReadUInt64LittleEndian(Table[16..]) != scratchˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[24..]) != scratchˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[28..]) != 0)
        {
            throw Invalidˉresponse();
        }
        for (var Index = 0; Index < functions.Length; Index++)
        {
            if (BinaryPrimitives.ReadUInt64LittleEndian(Table[(32 + Index * sizeof(ulong))..]) !=
                functions[Index])
            {
                throw Invalidˉresponse();
            }
        }
        return Table.ToArray().ToImmutableArray();
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉfileˉoutputˉtableˉbuilder).Assembly
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
        new("The Windvale native file-output-table response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native file-output-table constructor failed its exact identity contract.");
}
