using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

public enum Nativeˉpublicationˉstatus : uint
{
    Valid = 0,
    Invalidˉsize = 1,
    Invalidˉmagic = 2,
    Invalidˉversion = 3,
    Invalidˉreserved = 4,
    Invalidˉfragment = 5,
    Invalidˉservice = 6,
    Invalidˉorder = 7,
    Invalidˉrange = 8,
    Imageˉlimit = 9,
}

public sealed record Nativeˉpublicationˉservice(
    Nativeˉservice Service,
    int Size);

public sealed record Nativeˉpublicationˉplacement(
    Nativeˉservice Service,
    int Offset,
    int Size);

public sealed record Nativeˉpublicationˉplan(
    int Fragmentˉbytes,
    int Imageˉbytes,
    ImmutableArray<Nativeˉpublicationˉplacement> Placements);

public static class X64ˉnativeˉpublicationˉlayout
{
    public const uint REQUEST_MAGIC = 0x5150_5657;
    public const uint RESPONSE_MAGIC = 0x4C50_5657;
    public const uint FORMAT_VERSION = 1;
    public const int REQUEST_HEADER_BYTES = 24;
    public const int RESPONSE_HEADER_BYTES = 32;
    public const int SERVICE_RECORD_BYTES = 12;
    public const int MAXIMUM_SERVICES = 12;
    public const int MAXIMUM_IMAGE_BYTES = 34 * 1024 * 1024;
    public const int PLANNER_CANONICAL_SIZE = 7_105;
    public const string PLANNER_CANONICAL_SHA256 =
        "750b6134395c46c9e1c703ae2a56449bd1710f517e516397e10a1ccc951c503e";

    private const string REQUEST_NAME = "native-publication-request.bin";
    private const string PLANNER_RESOURCE = "Windvale.Native.Native-Publication-Bridge.wvb";
    private const long MAXIMUM_PLANNER_INSTRUCTIONS = 250_000;
    private static readonly ImmutableHashSet<string> AUTHORIZED_CAPABILITIES =
        ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES);
    private static readonly Lazy<Verifiedˉmodule> PLANNER = new(
        Readˉplanner,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Nativeˉpublicationˉplan Plan(
        int fragmentˉbytes,
        ImmutableArray<Nativeˉpublicationˉservice> services)
    {
        Validateˉinput(fragmentˉbytes, services);
        var Request = Buildˉrequest(fragmentˉbytes, services);
        var Response = Evaluateˉrequest(Request);
        return Verifyˉresponse(fragmentˉbytes, services, Response);
    }

    public static ImmutableArray<byte> Buildˉrequest(
        int fragmentˉbytes,
        ImmutableArray<Nativeˉpublicationˉservice> services)
    {
        Validateˉinput(fragmentˉbytes, services);
        var Bytes = new byte[checked(REQUEST_HEADER_BYTES + services.Length * SERVICE_RECORD_BYTES)];
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(0), REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(8), checked((uint)Bytes.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(12), checked((uint)fragmentˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(16), checked((uint)services.Length));
        for (var Index = 0; Index < services.Length; Index++)
        {
            var Offset = checked(REQUEST_HEADER_BYTES + Index * SERVICE_RECORD_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Offset),
                (uint)services[Index].Service);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Offset + sizeof(uint)),
                checked((uint)services[Index].Size));
        }
        return Bytes.ToImmutableArray();
    }

    public static ImmutableArray<byte> Evaluateˉrequest(ImmutableArray<byte> request)
    {
        if (request.IsDefault || request.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new ArgumentException(
                "The native publication request must be an initialized bounded byte value.",
                nameof(request));
        }

        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Publicationˉrequestˉreader(request));
        var Result = new Referenceˉruntime(
            PLANNER.Value,
            new Referenceˉcapabilityˉhost(Resources),
            new Runtimeˉoptions(
                AUTHORIZED_CAPABILITIES,
                MAXIMUM_PLANNER_INSTRUCTIONS,
                Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH))
            .Runˉmainˉbytes();
        return Result.Bytes;
    }

    public static Nativeˉpublicationˉplan Verifyˉresponse(
        int fragmentˉbytes,
        ImmutableArray<Nativeˉpublicationˉservice> services,
        ImmutableArray<byte> response)
    {
        Validateˉinput(fragmentˉbytes, services);
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse("The Windvale publication response is truncated.");
        }

        var Span = response.AsSpan();
        if (BinaryPrimitives.ReadUInt32LittleEndian(Span) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[4..]) != FORMAT_VERSION)
        {
            throw Invalidˉresponse("The Windvale publication response envelope is invalid.");
        }
        var Declaredˉsize = BinaryPrimitives.ReadUInt32LittleEndian(Span[8..]);
        var Status = (Nativeˉpublicationˉstatus)BinaryPrimitives.ReadUInt32LittleEndian(Span[12..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Span[16..]);
        if (!Enum.IsDefined(Status))
        {
            throw Invalidˉresponse("The Windvale publication response status is unknown.");
        }
        if (Status != Nativeˉpublicationˉstatus.Valid)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4013",
                $"The Windvale publication planner rejected its verified host request with " +
                $"status {Status} at offset {Failureˉoffset}.");
        }

        var Expectedˉsize = checked(RESPONSE_HEADER_BYTES + services.Length * SERVICE_RECORD_BYTES);
        var Expectedˉrequestˉsize = checked(REQUEST_HEADER_BYTES + services.Length * SERVICE_RECORD_BYTES);
        if (Declaredˉsize != (uint)response.Length ||
            response.Length != Expectedˉsize ||
            Failureˉoffset != (uint)Expectedˉrequestˉsize ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != (uint)fragmentˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[28..]) != (uint)services.Length)
        {
            throw Invalidˉresponse("The Windvale publication response shape is inconsistent.");
        }

        var Cursor = Alignˉtoˉsixteen(fragmentˉbytes);
        var Placements = ImmutableArray.CreateBuilder<Nativeˉpublicationˉplacement>(services.Length);
        for (var Index = 0; Index < services.Length; Index++)
        {
            var Recordˉoffset = checked(RESPONSE_HEADER_BYTES + Index * SERVICE_RECORD_BYTES);
            var Service = (Nativeˉservice)BinaryPrimitives.ReadUInt32LittleEndian(
                Span[Recordˉoffset..]);
            var Offsetˉvalue = BinaryPrimitives.ReadUInt32LittleEndian(
                Span[(Recordˉoffset + sizeof(uint))..]);
            var Sizeˉvalue = BinaryPrimitives.ReadUInt32LittleEndian(
                Span[(Recordˉoffset + 2 * sizeof(uint))..]);
            if (Offsetˉvalue > MAXIMUM_IMAGE_BYTES || Sizeˉvalue > MAXIMUM_IMAGE_BYTES)
            {
                throw Invalidˉresponse("A Windvale publication service placement exceeds the image limit.");
            }
            var Offset = (int)Offsetˉvalue;
            var Size = (int)Sizeˉvalue;
            var Expected = services[Index];
            Cursor = Alignˉtoˉsixteen(Cursor);
            if (Service != Expected.Service || Offset != Cursor || Size != Expected.Size)
            {
                throw Invalidˉresponse("A Windvale publication service placement is inconsistent.");
            }
            Placements.Add(new(Service, Offset, Size));
            Cursor = checked(Offset + Size);
        }

        var Imageˉvalue = BinaryPrimitives.ReadUInt32LittleEndian(Span[24..]);
        if (Imageˉvalue > MAXIMUM_IMAGE_BYTES)
        {
            throw Invalidˉresponse("The Windvale publication image extent exceeds its limit.");
        }
        var Imageˉbytes = (int)Imageˉvalue;
        if (Imageˉbytes != Cursor || Imageˉbytes > MAXIMUM_IMAGE_BYTES)
        {
            throw Invalidˉresponse("The Windvale publication image extent is inconsistent.");
        }
        return new(fragmentˉbytes, Imageˉbytes, Placements.MoveToImmutable());
    }

    private static void Validateˉinput(
        int fragmentˉbytes,
        ImmutableArray<Nativeˉpublicationˉservice> services)
    {
        if (fragmentˉbytes is < 1 or > Nativeˉcontract.MAXIMUM_CODE_BYTES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4013",
                "The native publication fragment size is outside its bounded range.");
        }
        if (services.IsDefault || services.Length > MAXIMUM_SERVICES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4013",
                "The native publication service list is uninitialized or too large.");
        }

        var Previous = 0;
        foreach (var Service in services)
        {
            if (Service is null ||
                !Enum.IsDefined(Service.Service) ||
                (int)Service.Service <= Previous ||
                Service.Size <= 0)
            {
                throw new Nativeˉbackendˉexception(
                    "WVN4013",
                    "The native publication service list is invalid or noncanonical.");
            }
            Previous = (int)Service.Service;
        }
    }

    private static int Alignˉtoˉsixteen(int value) => checked((value + 15) & ~15);

    private static Verifiedˉmodule Readˉplanner()
    {
        using var Stream = typeof(X64ˉnativeˉpublicationˉlayout).Assembly
            .GetManifestResourceStream(PLANNER_RESOURCE) ??
            throw Invalidˉplanner();
        if (Stream.Length != PLANNER_CANONICAL_SIZE)
        {
            throw Invalidˉplanner();
        }
        var Bytes = new byte[PLANNER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, PLANNER_CANONICAL_SHA256))
        {
            throw Invalidˉplanner();
        }
        return Moduleˉcodec.Readˉandˉverify(Bytes);
    }

    private static Nativeˉbackendˉexception Invalidˉresponse(string message) =>
        new("WVN4014", message);

    private static InvalidOperationException Invalidˉplanner() =>
        new("The retained Windvale native-publication planner failed its exact identity contract.");

    private sealed class Publicationˉrequestˉreader(ImmutableArray<byte> request)
        : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (!StringComparer.Ordinal.Equals(resourceˉname, REQUEST_NAME))
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The native publication planner requested an unknown resource.");
            }
            if (request.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "The native publication request exceeds the hosted byte limit.");
            }
            return request;
        }
    }
}
