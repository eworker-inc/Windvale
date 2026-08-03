using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace Windvale.Bootstrap;

public enum Directoryˉserviceˉrequestˉstatus
{
    Valid,
    Invalidˉsize,
    Invalidˉmagic,
    Invalidˉversion,
    Invalidˉheader,
    Invalidˉname,
    Invalidˉlimit,
}

public enum Directoryˉserviceˉstatus : uint
{
    Valid = 0,
    Notˉfound = 1,
    Notˉfile = 2,
    Permissionˉdenied = 3,
    Unavailable = 4,
    Revoked = 5,
    Stale = 6,
    Peerˉexited = 7,
    Invalidˉoffset = 8,
    Invalidˉname = 9,
    Invalidˉlimit = 10,
}

public sealed record Directoryˉserviceˉrequest(
    Directoryˉserviceˉrequestˉstatus Status,
    uint Offset,
    uint Maximumˉbytes,
    string Name,
    uint Failureˉoffset);

public sealed record Directoryˉserviceˉresponse(
    Directoryˉserviceˉstatus Status,
    uint Fileˉlength,
    uint Returnedˉoffset,
    ImmutableArray<byte> Bytes);

public sealed record Directoryˉserviceˉresult(
    Directoryˉserviceˉstatus Status,
    uint Fileˉlength,
    ImmutableArray<byte> Bytes);

public interface IDirectoryˉserviceˉsnapshot
{
    Directoryˉserviceˉresult Readˉbytes(
        string name,
        uint offset,
        uint maximumˉbytes);
}

public sealed class Directoryˉserviceˉipcˉexception(
    string code,
    string message,
    int offset = -1) : Exception(message)
{
    public string Code { get; } = code;
    public int Offset { get; } = offset;
}

public static class Directoryˉserviceˉipcˉcontract
{
    public const uint REQUEST_MAGIC = 0x5144_5657;
    public const uint RESPONSE_MAGIC = 0x5244_5657;
    public const uint FORMAT_VERSION = 1;
    public const int REQUEST_HEADER_BYTES = 28;
    public const int RESPONSE_HEADER_BYTES = 24;
    public const int MAXIMUM_NAME_BYTES = 255;
    public const uint MAXIMUM_CHUNK_BYTES = 3 * 1_024;
    public const int MAXIMUM_REQUEST_BYTES = REQUEST_HEADER_BYTES + MAXIMUM_NAME_BYTES;
    public const int MAXIMUM_RESPONSE_BYTES = RESPONSE_HEADER_BYTES + (int)MAXIMUM_CHUNK_BYTES;

    public const int REQUEST_TOTAL_BYTES_OFFSET = 8;
    public const int REQUEST_OFFSET_OFFSET = 12;
    public const int REQUEST_MAXIMUM_BYTES_OFFSET = 16;
    public const int REQUEST_NAME_BYTES_OFFSET = 20;
    public const int REQUEST_RESERVED_OFFSET = 24;
}

public static class Directoryˉserviceˉipcˉcodec
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static ImmutableArray<byte> Writeˉrequest(
        string name,
        uint offset,
        uint maximumˉbytes)
    {
        ArgumentNullException.ThrowIfNull(name);
        byte[] Nameˉbytes;
        try
        {
            Nameˉbytes = STRICT_UTF8.GetBytes(name);
        }
        catch (EncoderFallbackException)
        {
            throw new Directoryˉserviceˉipcˉexception(
                "WVDI3001", "The directory-service name is not valid Unicode.");
        }
        if (Nameˉbytes.Length > Directoryˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES)
        {
            throw new Directoryˉserviceˉipcˉexception(
                "WVDI3001", "The directory-service name cannot fit the bounded request.");
        }

        var Result = new byte[checked(
            Directoryˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES + Nameˉbytes.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result, Directoryˉserviceˉipcˉcontract.REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Directoryˉserviceˉipcˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉserviceˉipcˉcontract.REQUEST_TOTAL_BYTES_OFFSET),
            checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉserviceˉipcˉcontract.REQUEST_OFFSET_OFFSET), offset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉserviceˉipcˉcontract.REQUEST_MAXIMUM_BYTES_OFFSET),
            maximumˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉserviceˉipcˉcontract.REQUEST_NAME_BYTES_OFFSET),
            checked((uint)Nameˉbytes.Length));
        Nameˉbytes.CopyTo(Result.AsSpan(Directoryˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES));
        return Result.ToImmutableArray();
    }

    public static Directoryˉserviceˉrequest Parseˉrequest(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < Directoryˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES or
            > Directoryˉserviceˉipcˉcontract.MAXIMUM_REQUEST_BYTES)
        {
            return Invalid(
                Directoryˉserviceˉrequestˉstatus.Invalidˉsize,
                failureˉoffset: checked((uint)Math.Min(
                    bytes.Length,
                    Boundedˉserviceˉexchangeˉcontract.MAXIMUM_MESSAGE_BYTES)));
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) !=
            Directoryˉserviceˉipcˉcontract.REQUEST_MAGIC)
        {
            return Invalid(Directoryˉserviceˉrequestˉstatus.Invalidˉmagic, 0);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) !=
            Directoryˉserviceˉipcˉcontract.FORMAT_VERSION)
        {
            return Invalid(Directoryˉserviceˉrequestˉstatus.Invalidˉversion, 4);
        }

        var Totalˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉserviceˉipcˉcontract.REQUEST_TOTAL_BYTES_OFFSET..]);
        var Offset = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉserviceˉipcˉcontract.REQUEST_OFFSET_OFFSET..]);
        var Maximumˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉserviceˉipcˉcontract.REQUEST_MAXIMUM_BYTES_OFFSET..]);
        var Nameˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉserviceˉipcˉcontract.REQUEST_NAME_BYTES_OFFSET..]);
        var Reserved = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉserviceˉipcˉcontract.REQUEST_RESERVED_OFFSET..]);
        if (Totalˉbytes != (uint)bytes.Length ||
            Nameˉbytes > Directoryˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES ||
            Nameˉbytes != (uint)bytes.Length - Directoryˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES ||
            Reserved != 0)
        {
            return Invalid(
                Directoryˉserviceˉrequestˉstatus.Invalidˉheader,
                Directoryˉserviceˉipcˉcontract.REQUEST_TOTAL_BYTES_OFFSET,
                Offset,
                Maximumˉbytes);
        }

        var Encodedˉname = bytes[Directoryˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES..];
        if (!Isˉnameˉvalid(Encodedˉname))
        {
            return Invalid(
                Directoryˉserviceˉrequestˉstatus.Invalidˉname,
                Directoryˉserviceˉipcˉcontract.REQUEST_HEADER_BYTES,
                Offset,
                Maximumˉbytes);
        }
        var Name = Encoding.ASCII.GetString(Encodedˉname);
        if (Maximumˉbytes > Directoryˉserviceˉipcˉcontract.MAXIMUM_CHUNK_BYTES ||
            Offset > uint.MaxValue - Maximumˉbytes)
        {
            return new(
                Directoryˉserviceˉrequestˉstatus.Invalidˉlimit,
                Offset,
                Maximumˉbytes,
                Name,
                Directoryˉserviceˉipcˉcontract.REQUEST_MAXIMUM_BYTES_OFFSET);
        }
        return new(
            Directoryˉserviceˉrequestˉstatus.Valid,
            Offset,
            Maximumˉbytes,
            Name,
            0);
    }

    public static Directoryˉserviceˉrequest Verifyˉrequest(ReadOnlySpan<byte> bytes)
    {
        var Request = Parseˉrequest(bytes);
        if (Request.Status == Directoryˉserviceˉrequestˉstatus.Valid)
        {
            return Request;
        }
        var Code = Request.Status switch
        {
            Directoryˉserviceˉrequestˉstatus.Invalidˉsize => "WVDI1001",
            Directoryˉserviceˉrequestˉstatus.Invalidˉmagic or
                Directoryˉserviceˉrequestˉstatus.Invalidˉversion => "WVDI1002",
            Directoryˉserviceˉrequestˉstatus.Invalidˉheader => "WVDI1003",
            Directoryˉserviceˉrequestˉstatus.Invalidˉname => "WVDI1004",
            Directoryˉserviceˉrequestˉstatus.Invalidˉlimit => "WVDI1005",
            _ => throw new InvalidOperationException("Unknown directory-service request status."),
        };
        throw new Directoryˉserviceˉipcˉexception(
            Code,
            "The directory-service request is invalid.",
            checked((int)Request.Failureˉoffset));
    }

    public static Directoryˉserviceˉresponse Verifyˉresponse(
        ReadOnlySpan<byte> bytes,
        string name,
        uint offset,
        uint maximumˉbytes)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (bytes.Length is < Directoryˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES or
            > Directoryˉserviceˉipcˉcontract.MAXIMUM_RESPONSE_BYTES)
        {
            Fail("WVDI2001", "The directory-service response extent is outside its limits.", bytes.Length);
        }
        var Magic = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        var Version = BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]);
        var Rawˉstatus = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]);
        var Fileˉlength = BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..]);
        var Returnedˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(bytes[16..]);
        var Chunkˉlength = BinaryPrimitives.ReadUInt32LittleEndian(bytes[20..]);
        if (Magic != Directoryˉserviceˉipcˉcontract.RESPONSE_MAGIC ||
            Version != Directoryˉserviceˉipcˉcontract.FORMAT_VERSION)
        {
            Fail("WVDI2002", "The directory-service response identity is invalid.", 0);
        }
        if (Rawˉstatus > (uint)Directoryˉserviceˉstatus.Invalidˉlimit ||
            Returnedˉoffset != offset ||
            Chunkˉlength != (uint)bytes.Length - Directoryˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES)
        {
            Fail("WVDI2003", "The directory-service response header is inconsistent.", 8);
        }

        var Status = (Directoryˉserviceˉstatus)Rawˉstatus;
        var Nameˉisˉvalid = Isˉnameˉvalid(name);
        var Limitˉisˉvalid = maximumˉbytes <=
                Directoryˉserviceˉipcˉcontract.MAXIMUM_CHUNK_BYTES &&
            offset <= uint.MaxValue - maximumˉbytes;
        if (Status == Directoryˉserviceˉstatus.Valid)
        {
            if (!Nameˉisˉvalid || !Limitˉisˉvalid || offset > Fileˉlength ||
                Chunkˉlength != Math.Min(maximumˉbytes, Fileˉlength - offset))
            {
                Fail("WVDI2004", "The successful directory-service response is noncanonical.", 8);
            }
        }
        else if (Status is >= Directoryˉserviceˉstatus.Notˉfound and
            <= Directoryˉserviceˉstatus.Peerˉexited)
        {
            if (!Nameˉisˉvalid || !Limitˉisˉvalid || Fileˉlength != 0 || Chunkˉlength != 0)
            {
                Fail("WVDI2005", "The failed directory-service response is noncanonical.", 8);
            }
        }
        else if (Status == Directoryˉserviceˉstatus.Invalidˉoffset)
        {
            if (!Nameˉisˉvalid || !Limitˉisˉvalid ||
                offset <= Fileˉlength || Chunkˉlength != 0)
            {
                Fail("WVDI2005", "The invalid-offset directory response is noncanonical.", 8);
            }
        }
        else if (Status == Directoryˉserviceˉstatus.Invalidˉname)
        {
            if (Nameˉisˉvalid || Fileˉlength != 0 || Chunkˉlength != 0)
            {
                Fail("WVDI2005", "The invalid-name directory response is noncanonical.", 8);
            }
        }
        else if (Nameˉisˉvalid && !Limitˉisˉvalid)
        {
            if (Fileˉlength != 0 || Chunkˉlength != 0)
            {
                Fail("WVDI2005", "The invalid-limit directory response is noncanonical.", 8);
            }
        }
        else
        {
            Fail("WVDI2005", "The directory-service response status does not match its request.", 8);
        }

        return new(
            Status,
            Fileˉlength,
            Returnedˉoffset,
            bytes[Directoryˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES..]
                .ToArray().ToImmutableArray());
    }

    public static bool Isˉnameˉvalid(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (name.Length is 0 or > Directoryˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES ||
            name is "." or "..")
        {
            return false;
        }
        foreach (var Character in name)
        {
            if (Character > 0x7F || !Isˉnameˉbyte((byte)Character))
            {
                return false;
            }
        }
        return true;
    }

    internal static ImmutableArray<byte> Buildˉresponse(
        Directoryˉserviceˉstatus status,
        uint fileˉlength,
        uint offset,
        ReadOnlySpan<byte> bytes)
    {
        var Result = new byte[checked(
            Directoryˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES + bytes.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result, Directoryˉserviceˉipcˉcontract.RESPONSE_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Directoryˉserviceˉipcˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)status);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), fileˉlength);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), offset);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), checked((uint)bytes.Length));
        bytes.CopyTo(Result.AsSpan(Directoryˉserviceˉipcˉcontract.RESPONSE_HEADER_BYTES));
        return Result.ToImmutableArray();
    }

    private static Directoryˉserviceˉrequest Invalid(
        Directoryˉserviceˉrequestˉstatus status,
        uint failureˉoffset,
        uint offset = 0,
        uint maximumˉbytes = 0) =>
        new(status, offset, maximumˉbytes, string.Empty, failureˉoffset);

    private static bool Isˉnameˉvalid(ReadOnlySpan<byte> name)
    {
        if (name.Length is 0 or > Directoryˉserviceˉipcˉcontract.MAXIMUM_NAME_BYTES ||
            name.SequenceEqual("."u8) || name.SequenceEqual(".."u8))
        {
            return false;
        }
        foreach (var Value in name)
        {
            if (!Isˉnameˉbyte(Value))
            {
                return false;
            }
        }
        return true;
    }

    private static bool Isˉnameˉbyte(byte value) =>
        value is >= (byte)'A' and <= (byte)'Z' or
            >= (byte)'a' and <= (byte)'z' or
            >= (byte)'0' and <= (byte)'9' or
            (byte)'.' or (byte)'_' or (byte)'-';

    private static void Fail(string code, string message, int offset = -1) =>
        throw new Directoryˉserviceˉipcˉexception(code, message, offset);
}

public static class Directoryˉserviceˉhandler
{
    public static ImmutableArray<byte> Handle(
        IDirectoryˉserviceˉsnapshot snapshot,
        ReadOnlySpan<byte> request)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var Request = Directoryˉserviceˉipcˉcodec.Parseˉrequest(request);
        if (Request.Status is >= Directoryˉserviceˉrequestˉstatus.Invalidˉsize and
            <= Directoryˉserviceˉrequestˉstatus.Invalidˉheader)
        {
            return ImmutableArray<byte>.Empty;
        }
        if (Request.Status == Directoryˉserviceˉrequestˉstatus.Invalidˉname)
        {
            return Directoryˉserviceˉipcˉcodec.Buildˉresponse(
                Directoryˉserviceˉstatus.Invalidˉname, 0, Request.Offset, []);
        }
        if (Request.Status == Directoryˉserviceˉrequestˉstatus.Invalidˉlimit)
        {
            return Directoryˉserviceˉipcˉcodec.Buildˉresponse(
                Directoryˉserviceˉstatus.Invalidˉlimit, 0, Request.Offset, []);
        }

        Directoryˉserviceˉresult Result;
        try
        {
            Result = snapshot.Readˉbytes(Request.Name, Request.Offset, Request.Maximumˉbytes) ??
                throw Invalidˉprovider("The directory service returned no result.");
        }
        catch (Exception Exception) when (Exception is not Directoryˉserviceˉipcˉexception)
        {
            throw Invalidˉprovider("The directory service failed outside its typed contract.");
        }
        if (Result.Bytes.IsDefault || !Enum.IsDefined(Result.Status) ||
            Result.Status is Directoryˉserviceˉstatus.Invalidˉname or
                Directoryˉserviceˉstatus.Invalidˉlimit)
        {
            throw Invalidˉprovider("The directory service returned invalid typed storage or status.");
        }
        if (Result.Status == Directoryˉserviceˉstatus.Valid)
        {
            if (Request.Offset > Result.Fileˉlength ||
                Result.Bytes.Length != Math.Min(
                    Request.Maximumˉbytes,
                    Result.Fileˉlength - Request.Offset))
            {
                throw Invalidˉprovider("The directory service returned an inconsistent chunk.");
            }
        }
        else if (Result.Status == Directoryˉserviceˉstatus.Invalidˉoffset)
        {
            if (Request.Offset <= Result.Fileˉlength || !Result.Bytes.IsEmpty)
            {
                throw Invalidˉprovider("The directory service returned an inconsistent offset failure.");
            }
        }
        else if (Result.Fileˉlength != 0 || !Result.Bytes.IsEmpty)
        {
            throw Invalidˉprovider("The directory service returned data with a failed result.");
        }

        var Response = Directoryˉserviceˉipcˉcodec.Buildˉresponse(
            Result.Status,
            Result.Fileˉlength,
            Request.Offset,
            Result.Bytes.AsSpan());
        _ = Directoryˉserviceˉipcˉcodec.Verifyˉresponse(
            Response.AsSpan(), Request.Name, Request.Offset, Request.Maximumˉbytes);
        return Response;
    }

    private static Directoryˉserviceˉipcˉexception Invalidˉprovider(string message) =>
        new("WVDI3002", message);
}
