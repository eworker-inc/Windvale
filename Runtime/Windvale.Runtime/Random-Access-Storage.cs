using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Runtime;

public enum Randomˉaccessˉstorageˉoperation : uint
{
    Describe = 0,
    Read = 1,
    Write = 2,
    Resize = 3,
    Flush = 4,
}

public enum Randomˉaccessˉstorageˉstatus : uint
{
    Valid = 0,
    Permissionˉdenied = 1,
    Unavailable = 2,
    Revoked = 3,
    Stale = 4,
    Peerˉexited = 5,
    Outsideˉstorage = 6,
    Unsupported = 7,
    Invalidˉrequest = 8,
}

public enum Randomˉaccessˉstorageˉcompletion : uint
{
    None = 0,
    Completed = 1,
    Partial = 2,
    Indeterminate = 3,
}

public enum Randomˉaccessˉstorageˉflush : uint
{
    Content = 1,
    Contentˉandˉlength = 2,
}

public sealed record Randomˉaccessˉstorageˉresult(
    Randomˉaccessˉstorageˉstatus Status,
    ulong Generation,
    ulong Storageˉlength,
    ulong Position,
    uint Progress,
    Randomˉaccessˉstorageˉcompletion Completion,
    ImmutableArray<byte> Bytes);

public interface IRandomˉaccessˉstorage
{
    Randomˉaccessˉstorageˉresult Describe();

    Randomˉaccessˉstorageˉresult Readˉat(
        ulong generation,
        ulong position,
        uint maximumˉbytes);

    Randomˉaccessˉstorageˉresult Writeˉat(
        ulong generation,
        ulong position,
        ImmutableArray<byte> bytes);

    Randomˉaccessˉstorageˉresult Resize(
        ulong generation,
        ulong length);

    Randomˉaccessˉstorageˉresult Flush(
        ulong generation,
        Randomˉaccessˉstorageˉflush flush);
}

public static class Randomˉaccessˉstorageˉcontract
{
    public const int HEADER_BYTES = 48;
    public const uint MAGIC = 0x4153_5657;
    public const uint VERSION = 1;
    public const uint MAX_TRANSFER_BYTES = 64 * 1024;

    public static ImmutableArray<byte> Invoke(
        IRandomˉaccessˉstorage storage,
        uint operation,
        ulong generation,
        ulong position,
        uint control,
        ImmutableArray<byte> value)
    {
        ArgumentNullException.ThrowIfNull(storage);
        if (value.IsDefault)
        {
            throw Invalidˉprovider("The storage capability received uninitialized byte storage.");
        }

        if (!Requestˉisˉvalid(operation, generation, position, control, value.Length))
        {
            return Buildˉandˉverify(
                operation,
                generation,
                position,
                control,
                value.Length,
                new(
                    Randomˉaccessˉstorageˉstatus.Invalidˉrequest,
                    generation,
                    0,
                    position,
                    0,
                    Randomˉaccessˉstorageˉcompletion.None,
                    []),
                localˉrejection: true);
        }

        Randomˉaccessˉstorageˉresult Result;
        try
        {
            Result = operation switch
            {
                (uint)Randomˉaccessˉstorageˉoperation.Describe => storage.Describe(),
                (uint)Randomˉaccessˉstorageˉoperation.Read =>
                    storage.Readˉat(generation, position, control),
                (uint)Randomˉaccessˉstorageˉoperation.Write =>
                    storage.Writeˉat(generation, position, value),
                (uint)Randomˉaccessˉstorageˉoperation.Resize =>
                    storage.Resize(generation, position),
                (uint)Randomˉaccessˉstorageˉoperation.Flush =>
                    storage.Flush(generation, (Randomˉaccessˉstorageˉflush)control),
                _ => throw new InvalidOperationException("An invalid operation passed request validation."),
            } ?? throw Invalidˉprovider("The storage provider returned no result.");
        }
        catch (Exception)
        {
            throw Invalidˉprovider(
                "The storage provider failed outside the typed random-access contract.");
        }

        if (Result.Status == Randomˉaccessˉstorageˉstatus.Invalidˉrequest)
        {
            throw Invalidˉprovider("The storage provider returned a host-owned invalid-request status.");
        }
        return Buildˉandˉverify(
            operation,
            generation,
            position,
            control,
            value.Length,
            Result,
            localˉrejection: false);
    }

    public static void Verifyˉresponse(
        ReadOnlySpan<byte> response,
        uint operation,
        ulong generation,
        ulong position,
        uint control,
        int valueˉlength)
    {
        if (response.Length is < HEADER_BYTES or > HEADER_BYTES + (int)MAX_TRANSFER_BYTES)
        {
            throw Invalidˉprovider("The storage capability returned an invalid response size.");
        }

        var Magic = BinaryPrimitives.ReadUInt32LittleEndian(response);
        var Version = BinaryPrimitives.ReadUInt32LittleEndian(response[4..]);
        var Returnedˉoperation = BinaryPrimitives.ReadUInt32LittleEndian(response[8..]);
        var Rawˉstatus = BinaryPrimitives.ReadUInt32LittleEndian(response[12..]);
        var Returnedˉgeneration = BinaryPrimitives.ReadUInt64LittleEndian(response[16..]);
        var Storageˉlength = BinaryPrimitives.ReadUInt64LittleEndian(response[24..]);
        var Returnedˉposition = BinaryPrimitives.ReadUInt64LittleEndian(response[32..]);
        var Progress = BinaryPrimitives.ReadUInt32LittleEndian(response[40..]);
        var Rawˉcompletion = BinaryPrimitives.ReadUInt32LittleEndian(response[44..]);
        if (Magic != MAGIC || Version != VERSION || Returnedˉoperation != operation ||
            Rawˉstatus > (uint)Randomˉaccessˉstorageˉstatus.Invalidˉrequest ||
            Rawˉcompletion > (uint)Randomˉaccessˉstorageˉcompletion.Indeterminate)
        {
            throw Invalidˉprovider("The storage capability returned an invalid response header.");
        }

        var Result = new Randomˉaccessˉstorageˉresult(
            (Randomˉaccessˉstorageˉstatus)Rawˉstatus,
            Returnedˉgeneration,
            Storageˉlength,
            Returnedˉposition,
            Progress,
            (Randomˉaccessˉstorageˉcompletion)Rawˉcompletion,
            ImmutableArray.Create(response[HEADER_BYTES..].ToArray()));
        Validateˉresult(
            operation,
            generation,
            position,
            control,
            valueˉlength,
            Result,
            localˉrejection: Result.Status == Randomˉaccessˉstorageˉstatus.Invalidˉrequest);
    }

    public static bool Requestˉisˉvalid(
        uint operation,
        ulong generation,
        ulong position,
        uint control,
        int valueˉlength)
    {
        if (valueˉlength < 0 || valueˉlength > MAX_TRANSFER_BYTES)
        {
            return false;
        }

        return operation switch
        {
            (uint)Randomˉaccessˉstorageˉoperation.Describe =>
                generation == 0 && position == 0 && control == 0 && valueˉlength == 0,
            (uint)Randomˉaccessˉstorageˉoperation.Read =>
                generation != 0 && control <= MAX_TRANSFER_BYTES && valueˉlength == 0,
            (uint)Randomˉaccessˉstorageˉoperation.Write =>
                generation != 0 && control == 0 && position <= ulong.MaxValue - (ulong)valueˉlength,
            (uint)Randomˉaccessˉstorageˉoperation.Resize =>
                generation != 0 && control == 0 && valueˉlength == 0,
            (uint)Randomˉaccessˉstorageˉoperation.Flush =>
                generation != 0 && position == 0 && valueˉlength == 0 &&
                control is (uint)Randomˉaccessˉstorageˉflush.Content or
                    (uint)Randomˉaccessˉstorageˉflush.Contentˉandˉlength,
            _ => false,
        };
    }

    private static ImmutableArray<byte> Buildˉandˉverify(
        uint operation,
        ulong generation,
        ulong position,
        uint control,
        int valueˉlength,
        Randomˉaccessˉstorageˉresult result,
        bool localˉrejection)
    {
        Validateˉresult(
            operation,
            generation,
            position,
            control,
            valueˉlength,
            result,
            localˉrejection);
        var Response = new byte[checked(HEADER_BYTES + result.Bytes.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(0), MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(4), VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(8), operation);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(12), (uint)result.Status);
        BinaryPrimitives.WriteUInt64LittleEndian(Response.AsSpan(16), result.Generation);
        BinaryPrimitives.WriteUInt64LittleEndian(Response.AsSpan(24), result.Storageˉlength);
        BinaryPrimitives.WriteUInt64LittleEndian(Response.AsSpan(32), result.Position);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(40), result.Progress);
        BinaryPrimitives.WriteUInt32LittleEndian(Response.AsSpan(44), (uint)result.Completion);
        result.Bytes.AsSpan().CopyTo(Response.AsSpan(HEADER_BYTES));
        Verifyˉresponse(Response, operation, generation, position, control, valueˉlength);
        return ImmutableArray.Create(Response);
    }

    private static void Validateˉresult(
        uint operation,
        ulong generation,
        ulong position,
        uint control,
        int valueˉlength,
        Randomˉaccessˉstorageˉresult result,
        bool localˉrejection)
    {
        if (!Enum.IsDefined(result.Status) || !Enum.IsDefined(result.Completion) ||
            result.Bytes.IsDefault || result.Bytes.Length > MAX_TRANSFER_BYTES ||
            result.Position != position)
        {
            throw Invalidˉprovider("The storage capability returned invalid typed fields.");
        }

        var Validˉrequest = Requestˉisˉvalid(
            operation, generation, position, control, valueˉlength);
        if (result.Status == Randomˉaccessˉstorageˉstatus.Invalidˉrequest)
        {
            if (!localˉrejection || Validˉrequest || result.Generation != generation ||
                result.Storageˉlength != 0 || result.Progress != 0 ||
                result.Completion != Randomˉaccessˉstorageˉcompletion.None ||
                !result.Bytes.IsEmpty)
            {
                throw Invalidˉprovider("The storage capability returned an invalid request rejection.");
            }
            return;
        }
        if (localˉrejection || !Validˉrequest)
        {
            throw Invalidˉprovider("The storage capability accepted an invalid request.");
        }

        if (result.Status == Randomˉaccessˉstorageˉstatus.Stale)
        {
            if (result.Generation == 0 || result.Generation == generation ||
                result.Storageˉlength != 0 || result.Progress != 0 ||
                result.Completion != Randomˉaccessˉstorageˉcompletion.None ||
                !result.Bytes.IsEmpty)
            {
                throw Invalidˉprovider("The storage capability returned an invalid stale result.");
            }
            return;
        }

        if (result.Status == Randomˉaccessˉstorageˉstatus.Outsideˉstorage)
        {
            if (operation != (uint)Randomˉaccessˉstorageˉoperation.Read ||
                result.Generation != generation || position <= result.Storageˉlength ||
                result.Progress != 0 ||
                result.Completion != Randomˉaccessˉstorageˉcompletion.None ||
                !result.Bytes.IsEmpty)
            {
                throw Invalidˉprovider("The storage capability returned an invalid outside-storage result.");
            }
            return;
        }

        if (result.Status != Randomˉaccessˉstorageˉstatus.Valid)
        {
            if (result.Generation != generation || result.Storageˉlength != 0 ||
                result.Progress != 0 ||
                result.Completion != Randomˉaccessˉstorageˉcompletion.None ||
                !result.Bytes.IsEmpty)
            {
                throw Invalidˉprovider("The storage capability returned data with a failed result.");
            }
            return;
        }

        if (result.Generation == 0 ||
            operation != (uint)Randomˉaccessˉstorageˉoperation.Describe &&
                result.Generation != generation)
        {
            throw Invalidˉprovider("The storage capability returned an invalid generation.");
        }

        switch (operation)
        {
            case (uint)Randomˉaccessˉstorageˉoperation.Describe:
                if (result.Position != 0 || result.Progress != 0 ||
                    result.Completion != Randomˉaccessˉstorageˉcompletion.None ||
                    !result.Bytes.IsEmpty)
                {
                    throw Invalidˉprovider("The storage capability returned an invalid description.");
                }
                break;
            case (uint)Randomˉaccessˉstorageˉoperation.Read:
                if (position > result.Storageˉlength ||
                    (ulong)result.Bytes.Length != Math.Min(
                        (ulong)control, result.Storageˉlength - position) ||
                    result.Progress != (uint)result.Bytes.Length ||
                    result.Completion != Randomˉaccessˉstorageˉcompletion.None)
                {
                    throw Invalidˉprovider("The storage capability returned an invalid exact read.");
                }
                break;
            case (uint)Randomˉaccessˉstorageˉoperation.Write:
                Validateˉwriteˉresult(position, valueˉlength, result);
                break;
            case (uint)Randomˉaccessˉstorageˉoperation.Resize:
                Validateˉmutationˉresult(
                    result,
                    completedˉlength: position,
                    allowˉpartial: false,
                    expectedˉprogress: 0);
                break;
            case (uint)Randomˉaccessˉstorageˉoperation.Flush:
                Validateˉmutationˉresult(
                    result,
                    completedˉlength: null,
                    allowˉpartial: false,
                    expectedˉprogress: 0);
                break;
            default:
                throw Invalidˉprovider("The storage capability returned an unknown successful operation.");
        }
    }

    private static void Validateˉwriteˉresult(
        ulong position,
        int valueˉlength,
        Randomˉaccessˉstorageˉresult result)
    {
        if (!result.Bytes.IsEmpty)
        {
            throw Invalidˉprovider("The storage capability returned payload bytes for a write.");
        }
        switch (result.Completion)
        {
            case Randomˉaccessˉstorageˉcompletion.Completed:
                if (result.Progress != valueˉlength ||
                    result.Storageˉlength < position + (ulong)valueˉlength)
                {
                    throw Invalidˉprovider("The storage capability returned an invalid completed write.");
                }
                break;
            case Randomˉaccessˉstorageˉcompletion.Partial:
                if (result.Progress == 0 || result.Progress >= valueˉlength ||
                    result.Storageˉlength < position + result.Progress)
                {
                    throw Invalidˉprovider("The storage capability returned invalid partial progress.");
                }
                break;
            case Randomˉaccessˉstorageˉcompletion.Indeterminate:
                if (result.Progress != 0 || result.Storageˉlength != 0)
                {
                    throw Invalidˉprovider("The storage capability quantified an indeterminate write.");
                }
                break;
            default:
                throw Invalidˉprovider("The storage capability omitted a write completion outcome.");
        }
    }

    private static void Validateˉmutationˉresult(
        Randomˉaccessˉstorageˉresult result,
        ulong? completedˉlength,
        bool allowˉpartial,
        uint expectedˉprogress)
    {
        if (!result.Bytes.IsEmpty)
        {
            throw Invalidˉprovider("The storage capability returned payload bytes for a mutation.");
        }
        if (result.Completion == Randomˉaccessˉstorageˉcompletion.Completed)
        {
            if (result.Progress != expectedˉprogress ||
                completedˉlength is { } Length && result.Storageˉlength != Length)
            {
                throw Invalidˉprovider("The storage capability returned an invalid completed mutation.");
            }
            return;
        }
        if (allowˉpartial && result.Completion == Randomˉaccessˉstorageˉcompletion.Partial)
        {
            return;
        }
        if (result.Completion != Randomˉaccessˉstorageˉcompletion.Indeterminate ||
            result.Progress != 0 || result.Storageˉlength != 0)
        {
            throw Invalidˉprovider("The storage capability returned an invalid mutation outcome.");
        }
    }

    private static Runtimeˉexception Invalidˉprovider(string message) =>
        new("WVR3031", message);
}
