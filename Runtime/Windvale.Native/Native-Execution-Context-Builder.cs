using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉexecutionˉcontextˉinputs(
    ulong Maximumˉinstructions,
    ulong Maximumˉcallˉdepth,
    ulong Serviceˉtableˉpointer,
    ulong Recordˉarenaˉpointer,
    uint Recordˉarenaˉlength,
    ulong Textˉarenaˉpointer,
    uint Textˉarenaˉlength,
    ulong Argumentˉtableˉpointer,
    uint Argumentˉcount,
    ulong Outputˉtableˉpointer,
    ulong Fileˉinputˉtableˉpointer,
    ulong Fileˉoutputˉtableˉpointer);

internal static class Nativeˉexecutionˉcontextˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 5_531;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 58_363;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "acdfc7d71b5fc2f0c1cfd76242fddc59db2563a4026ac286313711f0e2eb05de";

    private const uint REQUEST_MAGIC = 0x5158_5657;
    private const uint RESPONSE_MAGIC = 0x5258_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_BYTES = 120;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 300_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Execution-Context-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(Nativeˉexecutionˉcontextˉinputs inputs)
    {
        var Request = Buildˉrequest(inputs);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(inputs, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Nativeˉexecutionˉcontextˉinputs inputs)
    {
        Verifyˉinputs(inputs);
        uint Flags = 0;
        if (inputs.Serviceˉtableˉpointer != 0)
        {
            Flags |= 1;
        }
        if (inputs.Argumentˉtableˉpointer != 0)
        {
            Flags |= 2;
        }
        if (inputs.Outputˉtableˉpointer != 0)
        {
            Flags |= 4;
        }
        if (inputs.Fileˉinputˉtableˉpointer != 0)
        {
            Flags |= 8;
        }
        if (inputs.Fileˉoutputˉtableˉpointer != 0)
        {
            Flags |= 16;
        }

        var Result = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), Flags);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), inputs.Maximumˉinstructions);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(24), inputs.Maximumˉcallˉdepth);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(32), inputs.Serviceˉtableˉpointer);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(40), inputs.Recordˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(48), inputs.Recordˉarenaˉlength);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(56), inputs.Textˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(64), inputs.Textˉarenaˉlength);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(80), inputs.Argumentˉtableˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(88), inputs.Argumentˉcount);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(96), inputs.Outputˉtableˉpointer);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(104),
            inputs.Fileˉinputˉtableˉpointer);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(112),
            inputs.Fileˉoutputˉtableˉpointer);
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        Nativeˉexecutionˉcontextˉinputs inputs,
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
                $"Windvale rejected the native execution-context request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) !=
                Nativeˉexecutionˉcontextˉcontract.SIZE ||
            response.Length != RESPONSE_HEADER_BYTES + Nativeˉexecutionˉcontextˉcontract.SIZE)
        {
            throw Invalidˉresponse();
        }

        var Context = Span[RESPONSE_HEADER_BYTES..];
        Verifyˉcontextˉbytes(inputs, Context);
        return Context.ToArray().ToImmutableArray();
    }

    internal static void Verifyˉinputs(Nativeˉexecutionˉcontextˉinputs inputs)
    {
        if (inputs.Maximumˉinstructions == 0 || inputs.Maximumˉcallˉdepth == 0)
        {
            throw new ArgumentException("The native execution-context budgets must be positive.");
        }
        if (inputs.Recordˉarenaˉpointer == 0 ||
            inputs.Recordˉarenaˉlength != Nativeˉcontract.MAXIMUM_RECORD_ARENA_BYTES)
        {
            throw new ArgumentException("The native execution context requires the canonical record arena.");
        }
        if (inputs.Textˉarenaˉpointer == 0 ||
            inputs.Textˉarenaˉlength != Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES)
        {
            throw new ArgumentException("The native execution context requires the canonical text arena.");
        }
        var Hasˉarguments = inputs.Argumentˉtableˉpointer != 0;
        if (Hasˉarguments != (inputs.Argumentˉcount != 0) ||
            inputs.Argumentˉcount > Hostedˉresourceˉlimits.MAX_ARGUMENTS)
        {
            throw new ArgumentException("The native execution-context argument table is invalid.");
        }
    }

    internal static void Verifyˉcontextˉbytes(
        Nativeˉexecutionˉcontextˉinputs inputs,
        ReadOnlySpan<byte> context)
    {
        if (context.Length != Nativeˉexecutionˉcontextˉcontract.SIZE ||
            BinaryPrimitives.ReadUInt32LittleEndian(context) !=
                Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[4..]) !=
                Nativeˉexecutionˉcontextˉcontract.SIZE ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[8..]) != inputs.Maximumˉinstructions ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[16..]) != inputs.Maximumˉcallˉdepth ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[24..]) != inputs.Serviceˉtableˉpointer ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[32..]) != inputs.Recordˉarenaˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[40..]) != inputs.Recordˉarenaˉlength ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[44..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[48..]) != inputs.Textˉarenaˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[56..]) != inputs.Textˉarenaˉlength ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[60..]) != 0 ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[64..]) != 0 ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[68..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[72..]) != inputs.Argumentˉtableˉpointer ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[80..]) != inputs.Argumentˉcount ||
            BinaryPrimitives.ReadUInt32LittleEndian(context[84..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[88..]) != inputs.Outputˉtableˉpointer ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[96..]) != inputs.Fileˉinputˉtableˉpointer ||
            BinaryPrimitives.ReadUInt64LittleEndian(context[104..]) !=
                inputs.Fileˉoutputˉtableˉpointer)
        {
            throw Invalidˉresponse();
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉexecutionˉcontextˉbuilder).Assembly
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
        new("The Windvale native execution-context response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native execution-context constructor failed its exact identity contract.");
}
