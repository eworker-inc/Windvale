using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Windvale.Bootstrap;

public enum Resourceˉserviceˉstatus : uint
{
    Success = 0,
    Malformedˉrequest = 1,
    Notˉfound = 2,
    Responseˉlimit = 3,
    Invalidˉstore = 4,
}

public enum Resourceˉserviceˉfailureˉdomain : uint
{
    None = 0,
    Request = 1,
    Store = 2,
}

public sealed record Resourceˉserviceˉrequest(
    uint Requestˉid,
    uint Maximumˉdataˉbytes,
    string Name);

public sealed record Resourceˉserviceˉresponse(
    uint Requestˉid,
    Resourceˉserviceˉstatus Status,
    Resourceˉserviceˉfailureˉdomain Failureˉdomain,
    uint Failureˉoffset,
    uint Identifier,
    uint Kind,
    uint Attributes,
    ImmutableArray<byte> Data,
    string Digest);

public sealed class Resourceˉserviceˉipcˉexception(
    string code,
    string message,
    int offset = -1) : Exception(message)
{
    public string Code { get; } = code;
    public int Offset { get; } = offset;
}

public static class Resourceˉserviceˉipcˉcontract
{
    public const uint REQUEST_MAGIC = 0x5152_5657;
    public const uint RESPONSE_MAGIC = 0x5952_5657;
    public const uint FORMAT_VERSION = 1;
    public const uint LOOKUP_OPERATION = 1;
    public const int REQUEST_HEADER_BYTES = 32;
    public const int RESPONSE_HEADER_BYTES = 112;
    public const int MAXIMUM_NAME_BYTES = 1_024;
    public const int MAXIMUM_MESSAGE_BYTES =
        Boundedˉserviceˉexchangeˉcontract.MAXIMUM_MESSAGE_BYTES;
    public const int MAXIMUM_DATA_BYTES = MAXIMUM_MESSAGE_BYTES - RESPONSE_HEADER_BYTES;
    public const int DIGEST_BYTES = 64;

    public const int REQUEST_ID_OFFSET = 12;
    public const int REQUEST_OPERATION_OFFSET = 16;
    public const int REQUEST_MAXIMUM_DATA_OFFSET = 20;
    public const int REQUEST_NAME_BYTES_OFFSET = 24;
    public const int REQUEST_RESERVED_OFFSET = 28;

    public const int RESPONSE_REQUEST_ID_OFFSET = 12;
    public const int RESPONSE_STATUS_OFFSET = 16;
    public const int RESPONSE_FAILURE_DOMAIN_OFFSET = 20;
    public const int RESPONSE_FAILURE_OFFSET_OFFSET = 24;
    public const int RESPONSE_IDENTIFIER_OFFSET = 28;
    public const int RESPONSE_KIND_OFFSET = 32;
    public const int RESPONSE_ATTRIBUTES_OFFSET = 36;
    public const int RESPONSE_DATA_BYTES_OFFSET = 40;
    public const int RESPONSE_RESERVED_OFFSET = 44;
    public const int RESPONSE_DIGEST_OFFSET = 48;
}

public static class Resourceˉserviceˉipcˉcodec
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static ImmutableArray<byte> Writeˉrequest(
        uint requestˉid,
        string name,
        uint maximumˉdataˉbytes)
    {
        if (requestˉid == 0 || maximumˉdataˉbytes > Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES ||
            string.IsNullOrEmpty(name) || name.Contains('\0', StringComparison.Ordinal))
        {
            Fail("WVRI3001", "The resource-service request value is invalid.");
        }

        byte[] Nameˉbytes;
        try
        {
            Nameˉbytes = STRICT_UTF8.GetBytes(name);
        }
        catch (EncoderFallbackException)
        {
            throw new Resourceˉserviceˉipcˉexception(
                "WVRI3001", "The resource-service request name is not strict UTF-8.");
        }
        if (Nameˉbytes.Length is < 1 or > Resourceˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES)
        {
            Fail("WVRI3001", "The resource-service request name exceeds its byte limit.");
        }

        var Result = new byte[checked(Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES + Nameˉbytes.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, Resourceˉserviceˉipcˉcontract.REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), Resourceˉserviceˉipcˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.REQUEST_ID_OFFSET), requestˉid);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.REQUEST_OPERATION_OFFSET),
            Resourceˉserviceˉipcˉcontract.LOOKUP_OPERATION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.REQUEST_MAXIMUM_DATA_OFFSET), maximumˉdataˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.REQUEST_NAME_BYTES_OFFSET),
            checked((uint)Nameˉbytes.Length));
        Nameˉbytes.CopyTo(Result.AsSpan(Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES));
        _ = Verifyˉrequest(Result);
        return Result.ToImmutableArray();
    }

    public static Resourceˉserviceˉrequest Verifyˉrequest(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES or
            > Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES +
                Resourceˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES)
        {
            Fail("WVRI1001", "The resource-service request extent is outside its limits.", bytes.Length);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != Resourceˉserviceˉipcˉcontract.REQUEST_MAGIC)
        {
            Fail("WVRI1002", "The resource-service request magic is invalid.", 0);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) !=
            Resourceˉserviceˉipcˉcontract.FORMAT_VERSION)
        {
            Fail("WVRI1002", "The resource-service request version is unsupported.", 4);
        }

        var Totalˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]);
        var Requestˉid = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.REQUEST_ID_OFFSET..]);
        var Operation = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.REQUEST_OPERATION_OFFSET..]);
        var Maximumˉdataˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.REQUEST_MAXIMUM_DATA_OFFSET..]);
        var Nameˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.REQUEST_NAME_BYTES_OFFSET..]);
        var Reserved = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.REQUEST_RESERVED_OFFSET..]);
        if (Totalˉbytes != (uint)bytes.Length || Requestˉid == 0 ||
            Operation != Resourceˉserviceˉipcˉcontract.LOOKUP_OPERATION ||
            Maximumˉdataˉbytes > Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES ||
            Nameˉbytes is < 1 or > Resourceˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES ||
            Nameˉbytes != (uint)bytes.Length - Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES ||
            Reserved != 0)
        {
            Fail("WVRI1003", "The resource-service request header is invalid.", 8);
        }

        var Encodedˉname = bytes[Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES..];
        if (Encodedˉname.Contains((byte)0))
        {
            Fail("WVRI1004", "The resource-service request name contains NUL.",
                Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES);
        }
        string Name;
        try
        {
            Name = STRICT_UTF8.GetString(Encodedˉname);
        }
        catch (DecoderFallbackException)
        {
            throw new Resourceˉserviceˉipcˉexception(
                "WVRI1004", "The resource-service request name is not strict UTF-8.",
                Resourceˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES);
        }
        return new(Requestˉid, Maximumˉdataˉbytes, Name);
    }

    public static Resourceˉserviceˉresponse Verifyˉresponse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < Resourceˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES or
            > Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES)
        {
            Fail("WVRI2001", "The resource-service response extent is outside its limits.", bytes.Length);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != Resourceˉserviceˉipcˉcontract.RESPONSE_MAGIC)
        {
            Fail("WVRI2002", "The resource-service response magic is invalid.", 0);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) !=
            Resourceˉserviceˉipcˉcontract.FORMAT_VERSION)
        {
            Fail("WVRI2002", "The resource-service response version is unsupported.", 4);
        }

        var Totalˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]);
        var Requestˉid = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_REQUEST_ID_OFFSET..]);
        var Rawˉstatus = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_STATUS_OFFSET..]);
        var Rawˉdomain = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_FAILURE_DOMAIN_OFFSET..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_FAILURE_OFFSET_OFFSET..]);
        var Identifier = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_IDENTIFIER_OFFSET..]);
        var Kind = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_KIND_OFFSET..]);
        var Attributes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_ATTRIBUTES_OFFSET..]);
        var Dataˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_DATA_BYTES_OFFSET..]);
        var Reserved = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_RESERVED_OFFSET..]);
        if (Totalˉbytes != (uint)bytes.Length ||
            Dataˉbytes != (uint)bytes.Length - Resourceˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES ||
            Reserved != 0)
        {
            Fail("WVRI2003", "The resource-service response header is inconsistent.", 8);
        }
        if (!Enum.IsDefined((Resourceˉserviceˉstatus)Rawˉstatus) ||
            !Enum.IsDefined((Resourceˉserviceˉfailureˉdomain)Rawˉdomain))
        {
            Fail("WVRI2004", "The resource-service response status or failure domain is invalid.", 16);
        }

        var Status = (Resourceˉserviceˉstatus)Rawˉstatus;
        var Failureˉdomain = (Resourceˉserviceˉfailureˉdomain)Rawˉdomain;
        var Digestˉbytes = bytes.Slice(
            Resourceˉserviceˉipcˉcontract.RESPONSE_DIGEST_OFFSET,
            Resourceˉserviceˉipcˉcontract.DIGEST_BYTES);
        var Data = bytes[Resourceˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES..];
        if (Status == Resourceˉserviceˉstatus.Success)
        {
            if (Requestˉid == 0 || Failureˉdomain != Resourceˉserviceˉfailureˉdomain.None ||
                Failureˉoffset != 0 || Identifier == 0 || Kind is < 1 or > 3 ||
                Attributes != Resourceˉstoreˉcontract.ENTRY_FLAGS ||
                Dataˉbytes > Resourceˉserviceˉipcˉcontract.MAXIMUM_DATA_BYTES)
            {
                Fail("WVRI2005", "The successful resource-service response is noncanonical.", 12);
            }
            var Expectedˉdigest = Encoding.ASCII.GetBytes(
                Convert.ToHexString(SHA256.HashData(Data)).ToLowerInvariant());
            if (!CryptographicOperations.FixedTimeEquals(Digestˉbytes, Expectedˉdigest))
            {
                Fail("WVRI2005", "The resource-service response digest is inconsistent.",
                    Resourceˉserviceˉipcˉcontract.RESPONSE_DIGEST_OFFSET);
            }
        }
        else
        {
            var Domainˉisˉvalid = Status switch
            {
                Resourceˉserviceˉstatus.Malformedˉrequest =>
                    Failureˉdomain == Resourceˉserviceˉfailureˉdomain.Request &&
                    Failureˉoffset <= Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES,
                Resourceˉserviceˉstatus.Invalidˉstore =>
                    Failureˉdomain == Resourceˉserviceˉfailureˉdomain.Store &&
                    Failureˉoffset <= Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES,
                Resourceˉserviceˉstatus.Responseˉlimit =>
                    Failureˉdomain == Resourceˉserviceˉfailureˉdomain.Request &&
                    Failureˉoffset == Resourceˉserviceˉipcˉcontract.REQUEST_MAXIMUM_DATA_OFFSET,
                Resourceˉserviceˉstatus.Notˉfound =>
                    Failureˉdomain == Resourceˉserviceˉfailureˉdomain.None && Failureˉoffset == 0,
                _ => false,
            };
            if (!Domainˉisˉvalid || (Status != Resourceˉserviceˉstatus.Malformedˉrequest && Requestˉid == 0) ||
                Identifier != 0 || Kind != 0 || Attributes != 0 || Dataˉbytes != 0 ||
                Digestˉbytes.IndexOfAnyExcept((byte)0) >= 0)
            {
                Fail("WVRI2006", "The failed resource-service response is noncanonical.", 12);
            }
        }

        return new(
            Requestˉid,
            Status,
            Failureˉdomain,
            Failureˉoffset,
            Identifier,
            Kind,
            Attributes,
            Data.ToArray().ToImmutableArray(),
            Status == Resourceˉserviceˉstatus.Success ? STRICT_UTF8.GetString(Digestˉbytes) : string.Empty);
    }

    internal static ImmutableArray<byte> Writeˉresponse(
        uint requestˉid,
        Resourceˉserviceˉstatus status,
        Resourceˉserviceˉfailureˉdomain failureˉdomain,
        uint failureˉoffset,
        uint identifier,
        uint kind,
        uint attributes,
        ReadOnlySpan<byte> data)
    {
        var Result = new byte[checked(Resourceˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES + data.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, Resourceˉserviceˉipcˉcontract.RESPONSE_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), Resourceˉserviceˉipcˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_REQUEST_ID_OFFSET), requestˉid);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_STATUS_OFFSET), (uint)status);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_FAILURE_DOMAIN_OFFSET), (uint)failureˉdomain);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_FAILURE_OFFSET_OFFSET), failureˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_IDENTIFIER_OFFSET), identifier);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_KIND_OFFSET), kind);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_ATTRIBUTES_OFFSET), attributes);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_DATA_BYTES_OFFSET), checked((uint)data.Length));
        if (status == Resourceˉserviceˉstatus.Success)
        {
            Encoding.ASCII.GetBytes(Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant())
                .CopyTo(Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_DIGEST_OFFSET));
        }
        data.CopyTo(Result.AsSpan(Resourceˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES));
        _ = Verifyˉresponse(Result);
        return Result.ToImmutableArray();
    }

    private static void Fail(string code, string message, int offset = -1) =>
        throw new Resourceˉserviceˉipcˉexception(code, message, offset);
}

public static class Resourceˉserviceˉhandler
{
    public static ImmutableArray<byte> Handle(ReadOnlySpan<byte> store, ReadOnlySpan<byte> request)
    {
        Resourceˉserviceˉrequest Request;
        try
        {
            Request = Resourceˉserviceˉipcˉcodec.Verifyˉrequest(request);
        }
        catch (Resourceˉserviceˉipcˉexception Exception)
        {
            var Requestˉid = request.Length >= Resourceˉserviceˉipcˉcontract.REQUEST_ID_OFFSET + sizeof(uint)
                ? BinaryPrimitives.ReadUInt32LittleEndian(
                    request[Resourceˉserviceˉipcˉcontract.REQUEST_ID_OFFSET..])
                : 0;
            return Resourceˉserviceˉipcˉcodec.Writeˉresponse(
                Requestˉid,
                Resourceˉserviceˉstatus.Malformedˉrequest,
                Resourceˉserviceˉfailureˉdomain.Request,
                checked((uint)Math.Min(
                    Resourceˉserviceˉipcˉcontract.MAXIMUM_MESSAGE_BYTES,
                    Math.Max(0, Exception.Offset))),
                0, 0, 0, []);
        }

        Verifiedˉresourceˉstore Store;
        try
        {
            Store = Resourceˉstoreˉverifier.Verify(store);
        }
        catch (Resourceˉstoreˉexception Exception)
        {
            return Resourceˉserviceˉipcˉcodec.Writeˉresponse(
                Request.Requestˉid,
                Resourceˉserviceˉstatus.Invalidˉstore,
                Resourceˉserviceˉfailureˉdomain.Store,
                checked((uint)Math.Min(
                    Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES,
                    Math.Max(0, Exception.Offset))),
                0, 0, 0, []);
        }

        if (!Store.Tryˉlookup(Request.Name, out var Entry))
        {
            return Resourceˉserviceˉipcˉcodec.Writeˉresponse(
                Request.Requestˉid,
                Resourceˉserviceˉstatus.Notˉfound,
                Resourceˉserviceˉfailureˉdomain.None,
                0, 0, 0, 0, []);
        }
        if (Entry!.Data.Length > Request.Maximumˉdataˉbytes)
        {
            return Resourceˉserviceˉipcˉcodec.Writeˉresponse(
                Request.Requestˉid,
                Resourceˉserviceˉstatus.Responseˉlimit,
                Resourceˉserviceˉfailureˉdomain.Request,
                Resourceˉserviceˉipcˉcontract.REQUEST_MAXIMUM_DATA_OFFSET,
                0, 0, 0, []);
        }
        return Resourceˉserviceˉipcˉcodec.Writeˉresponse(
            Request.Requestˉid,
            Resourceˉserviceˉstatus.Success,
            Resourceˉserviceˉfailureˉdomain.None,
            0,
            Entry.Identifier,
            (uint)Entry.Kind,
            Resourceˉstoreˉcontract.ENTRY_FLAGS,
            Entry.Data.AsSpan());
    }
}
