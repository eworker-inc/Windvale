using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉserviceˉtableˉbuilder
{
    internal const int CONSUMER_CANONICAL_SIZE = 3_079;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 34_830;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "e1b838652150999d13b84cd6f1c527b4e82923190530f707ef8d163d39a1f58e";

    private const uint REQUEST_MAGIC = 0x5154_5657;
    private const uint RESPONSE_MAGIC = 0x5254_5657;
    private const uint FORMAT_VERSION = 1;
    private const int SERVICE_COUNT = 12;
    private const int REQUEST_BYTES = 112;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const long MAXIMUM_INSTRUCTIONS = 200_000;
    private const string CONSUMER_RESOURCE = "Windvale.Native.Native-Service-Table-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<byte> Build(
        ImmutableArray<Nativeˉservice> requiredˉservices,
        ulong imageˉaddress,
        IReadOnlyDictionary<Nativeˉservice, int> serviceˉoffsets)
    {
        var Bindings = Projectˉbindings(requiredˉservices, imageˉaddress, serviceˉoffsets);
        var Request = Buildˉrequest(Bindings.Requiredˉmask, Bindings.Targets);
        var Response = Buildˉwithˉwindvale(Request);
        return Verifyˉresponse(
            Bindings.Requiredˉmask,
            Bindings.Targets,
            Request.Length,
            Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        uint requiredˉmask,
        ImmutableArray<ulong> targets)
    {
        if (targets.IsDefault || targets.Length != SERVICE_COUNT)
        {
            throw new ArgumentException("The native service target list must contain twelve entries.",
                nameof(targets));
        }
        var Result = new byte[REQUEST_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), REQUEST_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), requiredˉmask);
        for (var Index = 0; Index < targets.Length; Index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(16 + Index * sizeof(ulong)),
                targets[Index]);
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
        uint requiredˉmask,
        ImmutableArray<ulong> targets,
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
                $"Windvale rejected the native service-table request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }
        if (Failureˉoffset != requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) !=
                Nativeˉserviceˉtableˉcontract.SIZE ||
            response.Length != RESPONSE_HEADER_BYTES + Nativeˉserviceˉtableˉcontract.SIZE)
        {
            throw Invalidˉresponse();
        }

        var Table = Span[RESPONSE_HEADER_BYTES..];
        if (BinaryPrimitives.ReadUInt32LittleEndian(Table) !=
                Nativeˉserviceˉtableˉcontract.FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Table[4..]) !=
                Nativeˉserviceˉtableˉcontract.SIZE)
        {
            throw Invalidˉresponse();
        }
        for (var Index = 0; Index < targets.Length; Index++)
        {
            var Required = (requiredˉmask & (1u << Index)) != 0;
            var Target = BinaryPrimitives.ReadUInt64LittleEndian(
                Table[(8 + Index * sizeof(ulong))..]);
            if (Target != targets[Index] || Required != (Target != 0))
            {
                throw Invalidˉresponse();
            }
        }
        return Table.ToArray().ToImmutableArray();
    }

    private static (uint Requiredˉmask, ImmutableArray<ulong> Targets) Projectˉbindings(
        ImmutableArray<Nativeˉservice> requiredˉservices,
        ulong imageˉaddress,
        IReadOnlyDictionary<Nativeˉservice, int> serviceˉoffsets)
    {
        if (requiredˉservices.IsDefaultOrEmpty || requiredˉservices.Length > SERVICE_COUNT)
        {
            throw new ArgumentException("The native service table requires a nonempty canonical subset.",
                nameof(requiredˉservices));
        }
        ArgumentNullException.ThrowIfNull(serviceˉoffsets);
        if (imageˉaddress == 0 || serviceˉoffsets.Count != requiredˉservices.Length)
        {
            throw new ArgumentException("The native service table bindings are incomplete.",
                nameof(serviceˉoffsets));
        }

        uint Requiredˉmask = 0;
        var Targets = new ulong[SERVICE_COUNT];
        var Previous = 0;
        foreach (var Service in requiredˉservices)
        {
            var Value = (int)Service;
            if (Value is < 1 or > SERVICE_COUNT || Value <= Previous ||
                !serviceˉoffsets.TryGetValue(Service, out var Offset) || Offset < 0)
            {
                throw new ArgumentException(
                    "The native service table bindings are noncanonical.",
                    nameof(requiredˉservices));
            }
            var Target = checked(imageˉaddress + (ulong)Offset);
            if (Target == 0)
            {
                throw new ArgumentException("A native service target is zero.", nameof(serviceˉoffsets));
            }
            Requiredˉmask |= 1u << (Value - 1);
            Targets[Value - 1] = Target;
            Previous = Value;
        }
        return (Requiredˉmask, Targets.ToImmutableArray());
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉserviceˉtableˉbuilder).Assembly
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
        new("The Windvale native service-table response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native service-table constructor failed its exact identity contract.");
}
