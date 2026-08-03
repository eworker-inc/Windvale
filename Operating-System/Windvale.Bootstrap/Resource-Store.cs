using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace Windvale.Bootstrap;

public enum Resourceˉstoreˉkind : uint
{
    Wvbˉmodule = 1,
    U32ˉexecutionˉbudget = 2,
    Opaqueˉbytes = 3,
}

public sealed record Resourceˉstoreˉentry(
    uint Identifier,
    Resourceˉstoreˉkind Kind,
    string Name,
    ImmutableArray<byte> Data);

public sealed class Verifiedˉresourceˉstore
{
    internal Verifiedˉresourceˉstore(ImmutableArray<Resourceˉstoreˉentry> entries)
    {
        Entries = entries;
    }

    public ImmutableArray<Resourceˉstoreˉentry> Entries { get; }

    public bool Tryˉlookup(string name, out Resourceˉstoreˉentry? entry)
    {
        foreach (var Candidate in Entries)
        {
            if (StringComparer.Ordinal.Equals(Candidate.Name, name))
            {
                entry = Candidate;
                return true;
            }
        }

        entry = null;
        return false;
    }
}

public sealed class Resourceˉstoreˉexception(
    string code,
    string message,
    int offset = -1) : Exception(message)
{
    public string Code { get; } = code;
    public int Offset { get; } = offset;
}

public static class Resourceˉstoreˉcontract
{
    public const uint MAGIC = 0x5352_5657;
    public const uint FORMAT_VERSION = 1;
    public const int HEADER_BYTES = 32;
    public const int ENTRY_BYTES = 96;
    public const int DIGEST_BYTES = 64;
    public const int MAXIMUM_ENTRIES = 64;
    public const int MAXIMUM_NAME_BYTES = 1_024;
    public const int MAXIMUM_RESOURCE_BYTES = 4 * 1_024 * 1_024;
    public const int MAXIMUM_STORE_BYTES = 4 * 1_024 * 1_024;
    public const uint ENTRY_FLAGS = 0x0000_0007;

    public const int IDENTIFIER_OFFSET = 0;
    public const int KIND_OFFSET = 4;
    public const int FLAGS_OFFSET = 8;
    public const int NAME_OFFSET_OFFSET = 12;
    public const int NAME_LENGTH_OFFSET = 16;
    public const int DATA_OFFSET_OFFSET = 20;
    public const int DATA_LENGTH_OFFSET = 24;
    public const int RESERVED_OFFSET = 28;
    public const int DIGEST_OFFSET = 32;
}

public static class Resourceˉstoreˉcodec
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static ImmutableArray<byte> Write(IEnumerable<Resourceˉstoreˉentry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var Prepared = entries.Select(Prepare).ToArray();
        if (Prepared.Length is < 1 or > Resourceˉstoreˉcontract.MAXIMUM_ENTRIES)
        {
            throw new Resourceˉstoreˉexception(
                "WVRS2001",
                $"A resource store requires 1 through {Resourceˉstoreˉcontract.MAXIMUM_ENTRIES} entries.");
        }

        Array.Sort(Prepared, static (Left, Right) => Compareˉordinal(Left.Nameˉbytes, Right.Nameˉbytes));
        var Identifiers = new HashSet<uint>();
        for (var Index = 0; Index < Prepared.Length; Index++)
        {
            var Entry = Prepared[Index];
            if (!Identifiers.Add(Entry.Value.Identifier))
            {
                throw new Resourceˉstoreˉexception(
                    "WVRS2002",
                    $"Resource identifier {Entry.Value.Identifier} is duplicated.");
            }
            if (Index > 0 && Compareˉordinal(Prepared[Index - 1].Nameˉbytes, Entry.Nameˉbytes) == 0)
            {
                throw new Resourceˉstoreˉexception(
                    "WVRS2002",
                    $"Resource name '{Entry.Value.Name}' is duplicated.");
            }
        }

        var Directoryˉbytes = checked(Prepared.Length * Resourceˉstoreˉcontract.ENTRY_BYTES);
        var Nameˉbytes = Prepared.Aggregate(0UL, (Total, Entry) => Total + (uint)Entry.Nameˉbytes.Length);
        var Dataˉbytes = Prepared.Aggregate(0UL, (Total, Entry) => Total + (uint)Entry.Value.Data.Length);
        var Totalˉbytes = checked((ulong)Resourceˉstoreˉcontract.HEADER_BYTES +
            (uint)Directoryˉbytes + Nameˉbytes + Dataˉbytes);
        if (Totalˉbytes > Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES)
        {
            throw new Resourceˉstoreˉexception(
                "WVRS2003",
                "The encoded resource store exceeds the store-size limit.");
        }

        var Result = new byte[checked((int)Totalˉbytes)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, Resourceˉstoreˉcontract.MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), Resourceˉstoreˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), checked((uint)Prepared.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)Directoryˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), checked((uint)Nameˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), checked((uint)Dataˉbytes));

        var Nameˉcursor = checked(Resourceˉstoreˉcontract.HEADER_BYTES + Directoryˉbytes);
        var Dataˉcursor = checked(Nameˉcursor + (int)Nameˉbytes);
        for (var Index = 0; Index < Prepared.Length; Index++)
        {
            var Entry = Prepared[Index];
            var Directoryˉoffset = checked(Resourceˉstoreˉcontract.HEADER_BYTES +
                Index * Resourceˉstoreˉcontract.ENTRY_BYTES);
            var Record = Result.AsSpan(Directoryˉoffset, Resourceˉstoreˉcontract.ENTRY_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.IDENTIFIER_OFFSET..], Entry.Value.Identifier);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.KIND_OFFSET..], (uint)Entry.Value.Kind);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.FLAGS_OFFSET..], Resourceˉstoreˉcontract.ENTRY_FLAGS);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.NAME_OFFSET_OFFSET..], checked((uint)Nameˉcursor));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.NAME_LENGTH_OFFSET..], checked((uint)Entry.Nameˉbytes.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.DATA_OFFSET_OFFSET..], checked((uint)Dataˉcursor));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.DATA_LENGTH_OFFSET..], checked((uint)Entry.Value.Data.Length));
            Entry.Digestˉbytes.CopyTo(Record[Resourceˉstoreˉcontract.DIGEST_OFFSET..]);

            Entry.Nameˉbytes.CopyTo(Result.AsSpan(Nameˉcursor));
            Entry.Value.Data.AsSpan().CopyTo(Result.AsSpan(Dataˉcursor));
            Nameˉcursor = checked(Nameˉcursor + Entry.Nameˉbytes.Length);
            Dataˉcursor = checked(Dataˉcursor + Entry.Value.Data.Length);
        }

        _ = Resourceˉstoreˉverifier.Verify(Result);
        return Result.ToImmutableArray();
    }

    private static Preparedˉresourceˉentry Prepare(Resourceˉstoreˉentry value)
    {
        if (value is null || value.Identifier == 0 || !Enum.IsDefined(value.Kind) ||
            string.IsNullOrEmpty(value.Name) || value.Data.IsDefault)
        {
            throw new Resourceˉstoreˉexception(
                "WVRS2002",
                "A resource identifier, kind, name, or data value is invalid.");
        }
        if (value.Name.Contains('\0', StringComparison.Ordinal))
        {
            throw new Resourceˉstoreˉexception("WVRS2002", "A resource name contains NUL.");
        }

        byte[] Nameˉbytes;
        try
        {
            Nameˉbytes = STRICT_UTF8.GetBytes(value.Name);
        }
        catch (EncoderFallbackException)
        {
            throw new Resourceˉstoreˉexception("WVRS2002", "A resource name is not strict UTF-8.");
        }
        if (Nameˉbytes.Length is < 1 or > Resourceˉstoreˉcontract.MAXIMUM_NAME_BYTES)
        {
            throw new Resourceˉstoreˉexception("WVRS2002", "A resource name exceeds its byte limit.");
        }
        if (value.Data.Length > Resourceˉstoreˉcontract.MAXIMUM_RESOURCE_BYTES)
        {
            throw new Resourceˉstoreˉexception("WVRS2003", "A resource exceeds its byte limit.");
        }

        var Digestˉtext = Convert.ToHexString(SHA256.HashData(value.Data.AsSpan())).ToLowerInvariant();
        return new(value, Nameˉbytes, Encoding.ASCII.GetBytes(Digestˉtext));
    }

    internal static int Compareˉordinal(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        var Shared = Math.Min(left.Length, right.Length);
        for (var Index = 0; Index < Shared; Index++)
        {
            if (left[Index] != right[Index])
            {
                return left[Index].CompareTo(right[Index]);
            }
        }
        return left.Length.CompareTo(right.Length);
    }

    private sealed record Preparedˉresourceˉentry(
        Resourceˉstoreˉentry Value,
        byte[] Nameˉbytes,
        byte[] Digestˉbytes);
}

public static class Resourceˉstoreˉverifier
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Verifiedˉresourceˉstore Verify(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is < Resourceˉstoreˉcontract.HEADER_BYTES or
            > Resourceˉstoreˉcontract.MAXIMUM_STORE_BYTES)
        {
            Fail("WVRS1001", "The resource-store extent is outside its limits.", bytes.Length);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes) != Resourceˉstoreˉcontract.MAGIC)
        {
            Fail("WVRS1002", "The resource-store magic is invalid.", 0);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) != Resourceˉstoreˉcontract.FORMAT_VERSION)
        {
            Fail("WVRS1002", "The resource-store version is unsupported.", 4);
        }
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]) != (uint)bytes.Length)
        {
            Fail("WVRS1003", "The resource-store total length is inconsistent.", 8);
        }

        var Entryˉcount = BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..]);
        var Directoryˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(bytes[16..]);
        var Nameˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(bytes[20..]);
        var Dataˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(bytes[24..]);
        var Reserved = BinaryPrimitives.ReadUInt32LittleEndian(bytes[28..]);
        if (Entryˉcount is < 1 or > Resourceˉstoreˉcontract.MAXIMUM_ENTRIES ||
            Directoryˉbytes != Entryˉcount * Resourceˉstoreˉcontract.ENTRY_BYTES ||
            Reserved != 0)
        {
            Fail("WVRS1003", "The resource-store header layout is invalid.", 12);
        }

        var Expectedˉbytes = (ulong)Resourceˉstoreˉcontract.HEADER_BYTES +
            Directoryˉbytes + Nameˉbytes + Dataˉbytes;
        if (Expectedˉbytes != (uint)bytes.Length)
        {
            Fail("WVRS1003", "The resource-store regions do not exactly cover the image.", 16);
        }

        var Namesˉstart = checked(Resourceˉstoreˉcontract.HEADER_BYTES + (int)Directoryˉbytes);
        var Dataˉstart = checked(Namesˉstart + (int)Nameˉbytes);
        var Nameˉcursor = Namesˉstart;
        var Dataˉcursor = Dataˉstart;
        var Entries = ImmutableArray.CreateBuilder<Resourceˉstoreˉentry>(checked((int)Entryˉcount));
        var Identifiers = new HashSet<uint>();
        ReadOnlySpan<byte> Previousˉname = default;
        for (var Index = 0; Index < Entryˉcount; Index++)
        {
            var Directoryˉoffset = checked(Resourceˉstoreˉcontract.HEADER_BYTES +
                (int)Index * Resourceˉstoreˉcontract.ENTRY_BYTES);
            var Record = bytes.Slice(Directoryˉoffset, Resourceˉstoreˉcontract.ENTRY_BYTES);
            var Identifier = BinaryPrimitives.ReadUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.IDENTIFIER_OFFSET..]);
            if (Identifier == 0 || !Identifiers.Add(Identifier))
            {
                Fail("WVRS1004", "A resource identifier is zero or duplicated.", Directoryˉoffset);
            }

            var Rawˉkind = BinaryPrimitives.ReadUInt32LittleEndian(Record[Resourceˉstoreˉcontract.KIND_OFFSET..]);
            var Flags = BinaryPrimitives.ReadUInt32LittleEndian(Record[Resourceˉstoreˉcontract.FLAGS_OFFSET..]);
            var Entryˉreserved = BinaryPrimitives.ReadUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.RESERVED_OFFSET..]);
            if (Rawˉkind is < 1 or > 3 || Flags != Resourceˉstoreˉcontract.ENTRY_FLAGS || Entryˉreserved != 0)
            {
                Fail("WVRS1005", "A resource kind, attribute, or reserved field is invalid.", Directoryˉoffset + 4);
            }

            var Nameˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.NAME_OFFSET_OFFSET..]);
            var Nameˉlength = BinaryPrimitives.ReadUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.NAME_LENGTH_OFFSET..]);
            if (Nameˉoffset != (uint)Nameˉcursor || Nameˉlength is < 1 or
                > Resourceˉstoreˉcontract.MAXIMUM_NAME_BYTES || Nameˉlength > (uint)(Dataˉstart - Nameˉcursor))
            {
                Fail("WVRS1006", "A resource name range is invalid or noncanonical.", Directoryˉoffset + 12);
            }
            var Encodedˉname = bytes.Slice(Nameˉcursor, checked((int)Nameˉlength));
            if (Encodedˉname.Contains((byte)0) ||
                (Index > 0 && Resourceˉstoreˉcodec.Compareˉordinal(Previousˉname, Encodedˉname) >= 0))
            {
                Fail("WVRS1006", "Resource names are invalid, duplicated, or out of order.", Nameˉcursor);
            }

            string Name;
            try
            {
                Name = STRICT_UTF8.GetString(Encodedˉname);
            }
            catch (DecoderFallbackException)
            {
                throw new Resourceˉstoreˉexception("WVRS1006", "A resource name is not strict UTF-8.", Nameˉcursor);
            }

            var Dataˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.DATA_OFFSET_OFFSET..]);
            var Dataˉlength = BinaryPrimitives.ReadUInt32LittleEndian(
                Record[Resourceˉstoreˉcontract.DATA_LENGTH_OFFSET..]);
            if (Dataˉoffset != (uint)Dataˉcursor ||
                Dataˉlength > Resourceˉstoreˉcontract.MAXIMUM_RESOURCE_BYTES ||
                Dataˉlength > (uint)(bytes.Length - Dataˉcursor))
            {
                Fail("WVRS1007", "A resource data range is invalid or noncanonical.", Directoryˉoffset + 20);
            }
            var Data = bytes.Slice(Dataˉcursor, checked((int)Dataˉlength));
            var Expectedˉdigest = Encoding.ASCII.GetBytes(
                Convert.ToHexString(SHA256.HashData(Data)).ToLowerInvariant());
            var Digest = Record.Slice(Resourceˉstoreˉcontract.DIGEST_OFFSET, Resourceˉstoreˉcontract.DIGEST_BYTES);
            if (!CryptographicOperations.FixedTimeEquals(Digest, Expectedˉdigest))
            {
                Fail("WVRS1008", "A resource digest is malformed or inconsistent.",
                    Directoryˉoffset + Resourceˉstoreˉcontract.DIGEST_OFFSET);
            }

            Entries.Add(new(
                Identifier,
                (Resourceˉstoreˉkind)Rawˉkind,
                Name,
                Data.ToArray().ToImmutableArray()));
            Previousˉname = Encodedˉname;
            Nameˉcursor = checked(Nameˉcursor + (int)Nameˉlength);
            Dataˉcursor = checked(Dataˉcursor + (int)Dataˉlength);
        }

        if (Nameˉcursor != Dataˉstart || Dataˉcursor != bytes.Length)
        {
            Fail("WVRS1003", "The resource-store payload contains a gap or trailing bytes.",
                Math.Min(Nameˉcursor, Dataˉcursor));
        }
        return new(Entries.ToImmutable());
    }

    private static void Fail(string code, string message, int offset) =>
        throw new Resourceˉstoreˉexception(code, message, offset);
}
