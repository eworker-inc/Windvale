using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Runtime;

public enum Readˉonlyˉdirectoryˉstatus : uint
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
}

public sealed record Readˉonlyˉdirectoryˉresult(
    Readˉonlyˉdirectoryˉstatus Status,
    uint Fileˉlength,
    ImmutableArray<byte> Bytes);

public interface IReadˉonlyˉdirectory
{
    Readˉonlyˉdirectoryˉresult Readˉbytes(
        string name,
        uint offset,
        uint maximumˉbytes);
}

public static class Readˉonlyˉdirectoryˉcontract
{
    public const int HEADER_BYTES = 24;
    public const uint MAGIC = 0x5244_5657;
    public const uint VERSION = 1;
    public const uint MAX_NAME_BYTES = 255;
    public const uint MAX_CHUNK_BYTES = 3 * 1024;

    private const uint INVALID_NAME_STATUS = 9;
    private const uint INVALID_LIMIT_STATUS = 10;

    public static ImmutableArray<byte> Read(
        IReadˉonlyˉdirectory directory,
        string name,
        uint offset,
        uint maximumˉbytes)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(name);
        if (Tryˉrejectˉrequest(name, offset, maximumˉbytes, out var Rejection))
        {
            return Rejection;
        }

        Readˉonlyˉdirectoryˉresult Result;
        try
        {
            Result = directory.Readˉbytes(name, offset, maximumˉbytes) ??
                throw Invalidˉprovider("The directory provider returned no result.");
        }
        catch (Exception)
        {
            throw Invalidˉprovider(
                "The directory provider failed outside the typed directory-read contract.");
        }

        if (!Enum.IsDefined(Result.Status) || Result.Bytes.IsDefault)
        {
            throw Invalidˉprovider("The directory provider returned invalid status or byte storage.");
        }

        var Status = (uint)Result.Status;
        if (Result.Status == Readˉonlyˉdirectoryˉstatus.Valid)
        {
            if (offset > Result.Fileˉlength)
            {
                throw Invalidˉprovider("A successful directory read starts beyond the file length.");
            }
            var Expected = Math.Min(maximumˉbytes, Result.Fileˉlength - offset);
            if (Result.Bytes.Length != Expected)
            {
                throw Invalidˉprovider(
                    "A successful directory read did not return the exact bounded chunk.");
            }
        }
        else if (Result.Status == Readˉonlyˉdirectoryˉstatus.Invalidˉoffset)
        {
            if (offset <= Result.Fileˉlength || !Result.Bytes.IsEmpty)
            {
                throw Invalidˉprovider("An invalid-offset directory result is inconsistent.");
            }
        }
        else if (Result.Fileˉlength != 0 || !Result.Bytes.IsEmpty)
        {
            throw Invalidˉprovider("A failed directory read returned file data or length.");
        }

        var Response = new byte[checked(HEADER_BYTES + Result.Bytes.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(0), MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(4), VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(8), Status);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(12), Result.Fileˉlength);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(16), offset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Response.AsSpan(20),
            checked((uint)Result.Bytes.Length));
        Result.Bytes.AsSpan().CopyTo(Response.AsSpan(HEADER_BYTES));
        return ImmutableArray.Create(Response);
    }

    public static bool Isˉnameˉvalid(string name)
    {
        if (name.Length is 0 or > (int)MAX_NAME_BYTES || name is "." or "..")
        {
            return false;
        }
        foreach (var Character in name)
        {
            if (Character > 0x7F ||
                !(Character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '_' or '-'))
            {
                return false;
            }
        }
        return true;
    }

    public static bool Tryˉrejectˉrequest(
        string name,
        uint offset,
        uint maximumˉbytes,
        out ImmutableArray<byte> response)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!Isˉnameˉvalid(name))
        {
            response = Buildˉfailure(INVALID_NAME_STATUS, 0, offset);
            return true;
        }
        if (maximumˉbytes > MAX_CHUNK_BYTES || offset > uint.MaxValue - maximumˉbytes)
        {
            response = Buildˉfailure(INVALID_LIMIT_STATUS, 0, offset);
            return true;
        }
        response = default;
        return false;
    }

    public static void Verifyˉresponse(
        ReadOnlySpan<byte> response,
        string name,
        uint offset,
        uint maximumˉbytes)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (response.Length is < HEADER_BYTES or > HEADER_BYTES + (int)MAX_CHUNK_BYTES)
        {
            throw Invalidˉprovider("The directory capability returned an invalid response size.");
        }
        var Magic = BinaryPrimitives.ReadUInt32LittleEndian(response);
        var Version = BinaryPrimitives.ReadUInt32LittleEndian(response[4..]);
        var Status = BinaryPrimitives.ReadUInt32LittleEndian(response[8..]);
        var Fileˉlength = BinaryPrimitives.ReadUInt32LittleEndian(response[12..]);
        var Returnedˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(response[16..]);
        var Chunkˉlength = BinaryPrimitives.ReadUInt32LittleEndian(response[20..]);
        if (Magic != MAGIC || Version != VERSION || Status > INVALID_LIMIT_STATUS ||
            Returnedˉoffset != offset || Chunkˉlength != response.Length - HEADER_BYTES ||
            Chunkˉlength > maximumˉbytes)
        {
            throw Invalidˉprovider("The directory capability returned an invalid response header.");
        }
        var Validˉname = Isˉnameˉvalid(name);
        var Validˉlimit = maximumˉbytes <= MAX_CHUNK_BYTES &&
            offset <= uint.MaxValue - maximumˉbytes;
        if (Status <= (uint)Readˉonlyˉdirectoryˉstatus.Invalidˉoffset &&
            (!Validˉname || !Validˉlimit))
        {
            throw Invalidˉprovider("The directory capability accepted an invalid request.");
        }

        switch (Status)
        {
            case (uint)Readˉonlyˉdirectoryˉstatus.Valid:
                if (offset > Fileˉlength ||
                    Chunkˉlength != Math.Min(maximumˉbytes, Fileˉlength - offset))
                {
                    throw Invalidˉprovider("The directory capability returned an invalid successful chunk.");
                }
                break;
            case (uint)Readˉonlyˉdirectoryˉstatus.Invalidˉoffset:
                if (offset <= Fileˉlength || Chunkˉlength != 0)
                {
                    throw Invalidˉprovider("The directory capability returned an invalid offset failure.");
                }
                break;
            case INVALID_NAME_STATUS:
                if (Validˉname || Fileˉlength != 0 || Chunkˉlength != 0)
                {
                    throw Invalidˉprovider("The directory capability returned an invalid-name mismatch.");
                }
                break;
            case INVALID_LIMIT_STATUS:
                if (!Validˉname || Validˉlimit ||
                    Fileˉlength != 0 || Chunkˉlength != 0)
                {
                    throw Invalidˉprovider("The directory capability returned an invalid-limit mismatch.");
                }
                break;
            default:
                if (Fileˉlength != 0 || Chunkˉlength != 0)
                {
                    throw Invalidˉprovider("The directory capability returned payload with a failed result.");
                }
                break;
        }
    }

    private static ImmutableArray<byte> Buildˉfailure(uint status, uint fileˉlength, uint offset)
    {
        var Response = new byte[HEADER_BYTES];
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(0), MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(4), VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(8), status);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(12), fileˉlength);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(16), offset);
        return ImmutableArray.Create(Response);
    }

    private static Runtimeˉexception Invalidˉprovider(string message) =>
        new("WVR3030", message);
}
