using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal sealed record Nativeˉserviceˉcode(
    Nativeˉservice Service,
    Nativeˉserviceˉadapter Adapter,
    ImmutableArray<byte> Code);

internal static class X64ˉnativeˉserviceˉbundleˉmaterialization
{
    public const int CONSUMER_CANONICAL_SIZE = 15_253;
    public const string CONSUMER_CANONICAL_SHA256 =
        "25512a7c3e6eae0dd060426d5a51a93abfc7a7127f59538fd2a315242ed2b660";
    public const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 157_174;
    public const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "8bb1f06bd8b25d9a5ff78971ad4af36b609c618b080ed0fa9b17fe4b51669629";

    private const uint REQUEST_MAGIC = 0x5153_5657;
    private const uint RESPONSE_MAGIC = 0x4953_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 24;
    private const int RESPONSE_HEADER_BYTES = 36;
    private const int SERVICE_RECORD_BYTES = 12;
    private const long MAXIMUM_INSTRUCTIONS = 1_000_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Service-Bundle-Materialization-Bridge.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool Canˉmaterialize(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan)
    {
        if (fragment.IsDefault ||
            services.IsDefault ||
            plan is null ||
            plan.Fragmentˉbytes != fragment.Length ||
            plan.Placements.Length != services.Length ||
            services.Any(Service => Service is null || Service.Code.IsDefault))
        {
            return false;
        }

        try
        {
            var Planˉbytes = checked(
                X64ˉnativeˉpublicationˉlayout.REQUEST_HEADER_BYTES +
                services.Length * X64ˉnativeˉpublicationˉlayout.SERVICE_RECORD_BYTES);
            var Payloadˉbytes = checked(
                fragment.Length + services.Sum(Service => Service.Code.Length));
            var Requestˉbytes = checked(REQUEST_HEADER_BYTES + Planˉbytes + Payloadˉbytes);
            var Responseˉbytes = checked(
                RESPONSE_HEADER_BYTES +
                services.Length * SERVICE_RECORD_BYTES +
                plan.Imageˉbytes);
            return Requestˉbytes <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES &&
                Responseˉbytes <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    public static ImmutableArray<byte> Materialize(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan)
    {
        if (!Canˉmaterialize(fragment, services, plan))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                "The native service bundle exceeds the bounded Windvale materialization contract.");
        }

        var Request = Buildˉrequest(fragment, services);
        var Response = X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            Request,
            MAXIMUM_INSTRUCTIONS);
        return Verifyˉresponse(fragment, services, plan, Request.Length, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services)
    {
        var Planˉrequest = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
            fragment.Length,
            services.Select(Service => new Nativeˉpublicationˉservice(
                Service.Service,
                Service.Code.Length)).ToImmutableArray());
        var Payloadˉbytes = checked(
            fragment.Length + services.Sum(Service => Service.Code.Length));
        var Totalˉbytes = checked(REQUEST_HEADER_BYTES + Planˉrequest.Length + Payloadˉbytes);
        if (Totalˉbytes > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                "The native service-bundle materialization request exceeds the byte-value limit.");
        }

        var Result = new byte[Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            checked((uint)Planˉrequest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)Payloadˉbytes));
        Planˉrequest.CopyTo(Result, REQUEST_HEADER_BYTES);
        var Offset = checked(REQUEST_HEADER_BYTES + Planˉrequest.Length);
        fragment.CopyTo(Result, Offset);
        Offset = checked(Offset + fragment.Length);
        foreach (var Service in services)
        {
            Service.Code.CopyTo(Result, Offset);
            Offset = checked(Offset + Service.Code.Length);
        }
        if (Offset != Result.Length)
        {
            throw Invalidˉresponse("The service-bundle request payload is incomplete.");
        }
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Verifyˉresponse(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse("The Windvale service-bundle response is truncated.");
        }

        var Span = response.AsSpan();
        if (BinaryPrimitives.ReadUInt32LittleEndian(Span) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[4..]) != FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[8..]) != (uint)response.Length)
        {
            throw Invalidˉresponse("The Windvale service-bundle response envelope is invalid.");
        }
        var Status = BinaryPrimitives.ReadUInt32LittleEndian(Span[12..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Span[16..]);
        if (Status != 0)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                $"Windvale rejected the service-bundle request with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }

        var Expectedˉplanˉbytes = checked(
            X64ˉnativeˉpublicationˉlayout.REQUEST_HEADER_BYTES +
            services.Length * X64ˉnativeˉpublicationˉlayout.SERVICE_RECORD_BYTES);
        var Imageˉoffset = checked(RESPONSE_HEADER_BYTES + services.Length * SERVICE_RECORD_BYTES);
        if (Failureˉoffset != (uint)requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != (uint)Expectedˉplanˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[24..]) != (uint)fragment.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[28..]) != (uint)plan.Imageˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[32..]) != (uint)services.Length ||
            response.Length != checked(Imageˉoffset + plan.Imageˉbytes))
        {
            throw Invalidˉresponse("The Windvale service-bundle response shape is inconsistent.");
        }

        for (var Index = 0; Index < services.Length; Index++)
        {
            var Recordˉoffset = checked(RESPONSE_HEADER_BYTES + Index * SERVICE_RECORD_BYTES);
            var Placement = plan.Placements[Index];
            if (BinaryPrimitives.ReadUInt32LittleEndian(Span[Recordˉoffset..]) !=
                    (uint)Placement.Service ||
                BinaryPrimitives.ReadUInt32LittleEndian(Span[(Recordˉoffset + 4)..]) !=
                    (uint)Placement.Offset ||
                BinaryPrimitives.ReadUInt32LittleEndian(Span[(Recordˉoffset + 8)..]) !=
                    (uint)Placement.Size ||
                Placement.Service != services[Index].Service ||
                Placement.Size != services[Index].Code.Length)
            {
                throw Invalidˉresponse(
                    "A Windvale service-bundle placement is inconsistent with its verified input.");
            }
        }

        var Image = response.AsSpan(Imageˉoffset, plan.Imageˉbytes);
        if (!Image[..fragment.Length].SequenceEqual(fragment.AsSpan()))
        {
            throw Invalidˉresponse("The Windvale service bundle changed the native fragment bytes.");
        }
        var Previousˉend = fragment.Length;
        for (var Index = 0; Index < services.Length; Index++)
        {
            var Placement = plan.Placements[Index];
            var Fill = Index == 0 ? (byte)0 : (byte)0x90;
            if (!Image[Previousˉend..Placement.Offset].IsEmpty &&
                !Image[Previousˉend..Placement.Offset].ToArray().All(Value => Value == Fill))
            {
                throw Invalidˉresponse("The Windvale service-bundle alignment fill is invalid.");
            }
            if (!Image.Slice(Placement.Offset, Placement.Size).SequenceEqual(
                    services[Index].Code.AsSpan()))
            {
                throw Invalidˉresponse("A Windvale service-bundle leaf changed during materialization.");
            }
            Previousˉend = checked(Placement.Offset + Placement.Size);
        }
        if (services.IsEmpty &&
            !Image[Previousˉend..].ToArray().All(Value => Value == 0))
        {
            throw Invalidˉresponse("The Windvale service-free bundle has nonzero trailing alignment.");
        }
        return Image.ToArray().ToImmutableArray();
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(X64ˉnativeˉserviceˉbundleˉmaterialization).Assembly
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

    private static Nativeˉbackendˉexception Invalidˉresponse(string message) =>
        new("WVN4016", message);

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale service-bundle materializer failed its exact identity contract.");
}
