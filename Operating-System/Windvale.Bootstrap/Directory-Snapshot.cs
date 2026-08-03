using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace Windvale.Bootstrap;

public enum Directoryˉsnapshotˉkind : uint
{
    File = 1,
    Other = 2,
}

public sealed record Directoryˉsnapshotˉentry(
    Directoryˉsnapshotˉkind Kind,
    string Name,
    ImmutableArray<byte> Data);

public sealed class Verifiedˉdirectoryˉsnapshot
{
    internal Verifiedˉdirectoryˉsnapshot(ImmutableArray<Directoryˉsnapshotˉentry> entries)
    {
        Entries = entries;
    }

    public ImmutableArray<Directoryˉsnapshotˉentry> Entries { get; }

    public bool Tryˉlookup(string name, out Directoryˉsnapshotˉentry? entry)
    {
        entry = Entries.FirstOrDefault(
            Candidate => Candidate.Name.Equals(name, StringComparison.Ordinal));
        return entry is not null;
    }
}

public sealed class Directoryˉsnapshotˉexception(
    string code,
    string message,
    int offset = -1) : Exception(message)
{
    public string Code { get; } = code;
    public int Offset { get; } = offset;
}

public static class Directoryˉsnapshotˉcontract
{
    public const uint MAGIC = 0x5344_5657;
    public const uint FORMAT_VERSION = 1;
    public const int HEADER_BYTES = 32;
    public const int ENTRY_BYTES = 32;
    public const int MAXIMUM_ENTRY_COUNT = 64;
    public const int MAXIMUM_SNAPSHOT_BYTES = 4_096;
    public const int ENTRY_REGION_OFFSET = HEADER_BYTES;

    public const int TOTAL_BYTES_OFFSET = 8;
    public const int ENTRY_COUNT_OFFSET = 12;
    public const int ENTRY_REGION_OFFSET_OFFSET = 16;
    public const int ENTRY_BYTES_OFFSET = 20;
    public const int NAME_REGION_OFFSET_OFFSET = 24;
    public const int DATA_REGION_OFFSET_OFFSET = 28;

    public const int ENTRY_KIND_OFFSET = 0;
    public const int ENTRY_NAME_OFFSET_OFFSET = 4;
    public const int ENTRY_NAME_LENGTH_OFFSET = 8;
    public const int ENTRY_DATA_OFFSET_OFFSET = 12;
    public const int ENTRY_DATA_LENGTH_OFFSET = 16;
    public const int ENTRY_RESERVED_OFFSET = 20;
}

public static class Directoryˉsnapshotˉcodec
{
    public static ImmutableArray<byte> Write(ImmutableArray<Directoryˉsnapshotˉentry> entries)
    {
        if (entries.IsDefault || entries.Length is < 1 or > Directoryˉsnapshotˉcontract.MAXIMUM_ENTRY_COUNT ||
            entries.Any(Entry => Entry is null || Entry.Name is null))
        {
            Fail("WVDS2001", "The directory snapshot requires a bounded initialized entry set.");
        }

        var Ordered = entries
            .OrderBy(Entry => Entry.Name, StringComparer.Ordinal)
            .ToImmutableArray();
        var Names = ImmutableArray.CreateBuilder<byte>();
        var Data = ImmutableArray.CreateBuilder<byte>();
        var Nameˉoffsets = new int[Ordered.Length];
        var Dataˉoffsets = new int[Ordered.Length];
        string? Previousˉname = null;
        for (var Index = 0; Index < Ordered.Length; Index++)
        {
            var Entry = Ordered[Index];
            if (!Directoryˉserviceˉipcˉcodec.Isˉnameˉvalid(Entry.Name) ||
                Entry.Data.IsDefault || !Enum.IsDefined(Entry.Kind) ||
                (Entry.Kind == Directoryˉsnapshotˉkind.Other && !Entry.Data.IsEmpty) ||
                (Previousˉname is not null && Entry.Name.Equals(Previousˉname, StringComparison.Ordinal)))
            {
                Fail("WVDS2001", "The directory snapshot entry is invalid or duplicated.");
            }
            Previousˉname = Entry.Name;
            Nameˉoffsets[Index] = Names.Count;
            Names.AddRange(Encoding.ASCII.GetBytes(Entry.Name));
            if (Entry.Kind == Directoryˉsnapshotˉkind.File)
            {
                Dataˉoffsets[Index] = Data.Count;
                Data.AddRange(Entry.Data);
            }
        }

        var Nameˉregion = checked(
            Directoryˉsnapshotˉcontract.HEADER_BYTES +
            Ordered.Length * Directoryˉsnapshotˉcontract.ENTRY_BYTES);
        var Dataˉregion = Alignˉfour(checked(Nameˉregion + Names.Count));
        var Total = checked(Dataˉregion + Data.Count);
        if (Total > Directoryˉsnapshotˉcontract.MAXIMUM_SNAPSHOT_BYTES)
        {
            Fail("WVDS2001", "The directory snapshot exceeds one page.");
        }

        var Result = new byte[Total];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, Directoryˉsnapshotˉcontract.MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Directoryˉsnapshotˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉsnapshotˉcontract.TOTAL_BYTES_OFFSET), checked((uint)Total));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉsnapshotˉcontract.ENTRY_COUNT_OFFSET),
            checked((uint)Ordered.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉsnapshotˉcontract.ENTRY_REGION_OFFSET_OFFSET),
            Directoryˉsnapshotˉcontract.ENTRY_REGION_OFFSET);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉsnapshotˉcontract.ENTRY_BYTES_OFFSET),
            Directoryˉsnapshotˉcontract.ENTRY_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉsnapshotˉcontract.NAME_REGION_OFFSET_OFFSET),
            checked((uint)Nameˉregion));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Directoryˉsnapshotˉcontract.DATA_REGION_OFFSET_OFFSET),
            checked((uint)Dataˉregion));

        for (var Index = 0; Index < Ordered.Length; Index++)
        {
            var Entry = Ordered[Index];
            var Offset = Directoryˉsnapshotˉcontract.ENTRY_REGION_OFFSET +
                Index * Directoryˉsnapshotˉcontract.ENTRY_BYTES;
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + Directoryˉsnapshotˉcontract.ENTRY_KIND_OFFSET),
                (uint)Entry.Kind);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + Directoryˉsnapshotˉcontract.ENTRY_NAME_OFFSET_OFFSET),
                checked((uint)(Nameˉregion + Nameˉoffsets[Index])));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + Directoryˉsnapshotˉcontract.ENTRY_NAME_LENGTH_OFFSET),
                checked((uint)Entry.Name.Length));
            if (Entry.Kind == Directoryˉsnapshotˉkind.File)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Result.AsSpan(Offset + Directoryˉsnapshotˉcontract.ENTRY_DATA_OFFSET_OFFSET),
                    checked((uint)(Dataˉregion + Dataˉoffsets[Index])));
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Result.AsSpan(Offset + Directoryˉsnapshotˉcontract.ENTRY_DATA_LENGTH_OFFSET),
                    checked((uint)Entry.Data.Length));
            }
        }
        Names.CopyTo(Result, Nameˉregion);
        Data.CopyTo(Result, Dataˉregion);
        _ = Verify(Result);
        return Result.ToImmutableArray();
    }

    public static Verifiedˉdirectoryˉsnapshot Verify(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < Directoryˉsnapshotˉcontract.HEADER_BYTES or
            > Directoryˉsnapshotˉcontract.MAXIMUM_SNAPSHOT_BYTES)
        {
            Fail("WVDS1001", "The directory snapshot extent is outside one page.", bytes.Length);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != Directoryˉsnapshotˉcontract.MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) !=
                Directoryˉsnapshotˉcontract.FORMAT_VERSION)
        {
            Fail("WVDS1002", "The directory snapshot identity is invalid.", 0);
        }

        var Total = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉsnapshotˉcontract.TOTAL_BYTES_OFFSET..]);
        var Count = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉsnapshotˉcontract.ENTRY_COUNT_OFFSET..]);
        var Entryˉregion = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉsnapshotˉcontract.ENTRY_REGION_OFFSET_OFFSET..]);
        var Entryˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉsnapshotˉcontract.ENTRY_BYTES_OFFSET..]);
        var Nameˉregion = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉsnapshotˉcontract.NAME_REGION_OFFSET_OFFSET..]);
        var Dataˉregion = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes[Directoryˉsnapshotˉcontract.DATA_REGION_OFFSET_OFFSET..]);
        if (Total != (uint)bytes.Length ||
            Count is < 1 or > Directoryˉsnapshotˉcontract.MAXIMUM_ENTRY_COUNT ||
            Entryˉregion != Directoryˉsnapshotˉcontract.ENTRY_REGION_OFFSET ||
            Entryˉbytes != Directoryˉsnapshotˉcontract.ENTRY_BYTES ||
            Nameˉregion != checked((uint)(Directoryˉsnapshotˉcontract.HEADER_BYTES +
                Count * Directoryˉsnapshotˉcontract.ENTRY_BYTES)) ||
            Dataˉregion < Nameˉregion || Dataˉregion > Total || (Dataˉregion & 3) != 0)
        {
            Fail("WVDS1003", "The directory snapshot header is inconsistent.", 8);
        }

        var Entries = ImmutableArray.CreateBuilder<Directoryˉsnapshotˉentry>(checked((int)Count));
        var Expectedˉname = Nameˉregion;
        var Expectedˉdata = Dataˉregion;
        ReadOnlySpan<byte> Previousˉname = default;
        for (var Index = 0U; Index < Count; Index++)
        {
            var Offset = checked((int)(Entryˉregion + Index * Entryˉbytes));
            var Entry = bytes[Offset..(Offset + Directoryˉsnapshotˉcontract.ENTRY_BYTES)];
            var Rawˉkind = BinaryPrimitives.ReadUInt32LittleEndian(
                Entry[Directoryˉsnapshotˉcontract.ENTRY_KIND_OFFSET..]);
            var Nameˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(
                Entry[Directoryˉsnapshotˉcontract.ENTRY_NAME_OFFSET_OFFSET..]);
            var Nameˉlength = BinaryPrimitives.ReadUInt32LittleEndian(
                Entry[Directoryˉsnapshotˉcontract.ENTRY_NAME_LENGTH_OFFSET..]);
            var Dataˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(
                Entry[Directoryˉsnapshotˉcontract.ENTRY_DATA_OFFSET_OFFSET..]);
            var Dataˉlength = BinaryPrimitives.ReadUInt32LittleEndian(
                Entry[Directoryˉsnapshotˉcontract.ENTRY_DATA_LENGTH_OFFSET..]);
            if (!Enum.IsDefined((Directoryˉsnapshotˉkind)Rawˉkind) ||
                Nameˉoffset != Expectedˉname || Nameˉlength is < 1 or > 255 ||
                Nameˉlength > Dataˉregion - Nameˉoffset ||
                Entry[Directoryˉsnapshotˉcontract.ENTRY_RESERVED_OFFSET..]
                    .IndexOfAnyExcept((byte)0) >= 0)
            {
                Fail("WVDS1004", "The directory snapshot entry is malformed.", Offset);
            }
            var Encodedˉname = bytes.Slice(checked((int)Nameˉoffset), checked((int)Nameˉlength));
            var Name = Encoding.ASCII.GetString(Encodedˉname);
            if (!Directoryˉserviceˉipcˉcodec.Isˉnameˉvalid(Name) ||
                (!Previousˉname.IsEmpty && Previousˉname.SequenceCompareTo(Encodedˉname) >= 0))
            {
                Fail("WVDS1005", "The directory snapshot names are invalid or unordered.",
                    checked((int)Nameˉoffset));
            }
            Previousˉname = Encodedˉname;
            Expectedˉname = checked(Nameˉoffset + Nameˉlength);

            var Kind = (Directoryˉsnapshotˉkind)Rawˉkind;
            ImmutableArray<byte> Data;
            if (Kind == Directoryˉsnapshotˉkind.File)
            {
                if (Dataˉoffset != Expectedˉdata || Dataˉlength > Total - Dataˉoffset)
                {
                    Fail("WVDS1006", "The directory snapshot file extent is inconsistent.", Offset + 12);
                }
                Data = bytes.Slice(checked((int)Dataˉoffset), checked((int)Dataˉlength))
                    .ToArray().ToImmutableArray();
                Expectedˉdata = checked(Dataˉoffset + Dataˉlength);
            }
            else
            {
                if (Dataˉoffset != 0 || Dataˉlength != 0)
                {
                    Fail("WVDS1006", "A non-file snapshot entry contains file data.", Offset + 12);
                }
                Data = ImmutableArray<byte>.Empty;
            }
            Entries.Add(new(Kind, Name, Data));
        }

        if (Expectedˉname > Dataˉregion ||
            bytes.Slice(checked((int)Expectedˉname), checked((int)(Dataˉregion - Expectedˉname)))
                .IndexOfAnyExcept((byte)0) >= 0 ||
            Expectedˉdata != Total)
        {
            Fail("WVDS1007", "The directory snapshot regions do not have exact coverage.",
                checked((int)Expectedˉname));
        }
        return new(Entries.ToImmutable());
    }

    private static int Alignˉfour(int value) => checked((value + 3) & ~3);

    private static void Fail(string code, string message, int offset = -1) =>
        throw new Directoryˉsnapshotˉexception(code, message, offset);
}

public sealed class Directoryˉsnapshotˉprovider(ImmutableArray<byte> snapshot) : IDirectoryˉserviceˉsnapshot
{
    private readonly Verifiedˉdirectoryˉsnapshot Snapshot =
        Directoryˉsnapshotˉcodec.Verify(snapshot.AsSpan());

    public Directoryˉserviceˉresult Readˉbytes(
        string name,
        uint offset,
        uint maximumˉbytes)
    {
        if (!Snapshot.Tryˉlookup(name, out var Entry))
        {
            return new(Directoryˉserviceˉstatus.Notˉfound, 0, []);
        }
        if (Entry!.Kind != Directoryˉsnapshotˉkind.File)
        {
            return new(Directoryˉserviceˉstatus.Notˉfile, 0, []);
        }
        var Length = checked((uint)Entry.Data.Length);
        if (offset > Length)
        {
            return new(Directoryˉserviceˉstatus.Invalidˉoffset, Length, []);
        }
        var Returned = Math.Min(maximumˉbytes, Length - offset);
        return new(
            Directoryˉserviceˉstatus.Valid,
            Length,
            Entry.Data.AsSpan(checked((int)offset), checked((int)Returned)).ToArray().ToImmutableArray());
    }
}
