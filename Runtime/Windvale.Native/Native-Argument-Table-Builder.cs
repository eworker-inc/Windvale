using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉargumentˉtableˉentry(
    ulong Pointer,
    uint Length,
    uint Sourceˉoffset);

internal static class Nativeˉargumentˉtableˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 4_374;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 44_775;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "4a4cc1d6171126a821c1f96de11c4ffcb78ea83e98d06d5e0802e5921e9062d8";

    private const uint REQUEST_MAGIC = 0x5141_5657;
    private const uint RESPONSE_MAGIC = 0x5241_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 24;
    private const int REQUEST_ENTRY_BYTES = 16;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 300_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Argument-Table-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(
        ImmutableArray<Nativeˉargumentˉtableˉentry> entries,
        ImmutableArray<byte> packedˉarguments)
    {
        var Request = Buildˉrequest(entries, packedˉarguments);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(entries, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        ImmutableArray<Nativeˉargumentˉtableˉentry> entries,
        ImmutableArray<byte> packedˉarguments)
    {
        Verifyˉinputs(entries, packedˉarguments);
        var Payloadˉoffset = checked(REQUEST_HEADER_BYTES + entries.Length * REQUEST_ENTRY_BYTES);
        var Result = new byte[checked(Payloadˉoffset + packedˉarguments.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), checked((uint)entries.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)Payloadˉoffset));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(20),
            checked((uint)packedˉarguments.Length));
        for (var Index = 0; Index < entries.Length; Index++)
        {
            var Offset = REQUEST_HEADER_BYTES + Index * REQUEST_ENTRY_BYTES;
            BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(Offset), entries[Index].Pointer);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Offset + 8), entries[Index].Length);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 12),
                entries[Index].Sourceˉoffset);
        }
        packedˉarguments.CopyTo(Result, Payloadˉoffset);
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Verifyˉresponse(
        ImmutableArray<Nativeˉargumentˉtableˉentry> entries,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        if (entries.IsDefaultOrEmpty || entries.Length > Hostedˉresourceˉlimits.MAX_ARGUMENTS ||
            response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
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
                $"Windvale rejected the native argument-table request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        var Tableˉbytes = checked(entries.Length * Nativeˉcontract.VALUE_SLOT_BYTES);
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != checked((uint)Tableˉbytes) ||
            response.Length != RESPONSE_HEADER_BYTES + Tableˉbytes)
        {
            throw Invalidˉresponse();
        }

        var Table = Span[RESPONSE_HEADER_BYTES..];
        for (var Index = 0; Index < entries.Length; Index++)
        {
            var Descriptor = Table[(Index * Nativeˉcontract.VALUE_SLOT_BYTES)..];
            if (BinaryPrimitives.ReadUInt64LittleEndian(Descriptor) != entries[Index].Pointer ||
                BinaryPrimitives.ReadUInt32LittleEndian(Descriptor[8..]) != entries[Index].Length ||
                BinaryPrimitives.ReadUInt32LittleEndian(Descriptor[12..]) != 0)
            {
                throw Invalidˉresponse();
            }
        }
        return Table.ToArray().ToImmutableArray();
    }

    internal static void Verifyˉinputs(
        ImmutableArray<Nativeˉargumentˉtableˉentry> entries,
        ImmutableArray<byte> packedˉarguments)
    {
        if (entries.IsDefaultOrEmpty ||
            entries.Length > Hostedˉresourceˉlimits.MAX_ARGUMENTS ||
            packedˉarguments.IsDefault ||
            packedˉarguments.Length > Hostedˉresourceˉlimits.MAX_ARGUMENT_TOTAL_UTF8_BYTES)
        {
            throw new ArgumentException("The native argument-table inputs exceed their bounded envelope.");
        }
        var Packedˉlength = checked((uint)packedˉarguments.Length);
        uint Runningˉoffset = 0;
        foreach (var Entry in entries)
        {
            if (Entry.Pointer == 0 ||
                Entry.Length > Hostedˉresourceˉlimits.MAX_ARGUMENT_UTF8_BYTES ||
                Entry.Sourceˉoffset != Runningˉoffset ||
                Runningˉoffset > Packedˉlength ||
                Entry.Length > Packedˉlength - Runningˉoffset)
            {
                throw new ArgumentException("The native argument-table entry sequence is invalid.");
            }
            Runningˉoffset = checked(Runningˉoffset + Entry.Length);
        }
        if (Runningˉoffset != Packedˉlength)
        {
            throw new ArgumentException("The native argument-table entries do not cover the packed input.");
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉargumentˉtableˉbuilder).Assembly
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
        new("The Windvale native argument-table response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native argument-table constructor failed its exact identity contract.");
}
