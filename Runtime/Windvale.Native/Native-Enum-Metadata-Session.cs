using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Runtime.Native;

internal static class Nativeˉenumˉmetadataˉsession
{
    private const uint REQUEST_MAGIC = 0x51455657;
    private const uint REQUEST_VERSION = 2;
    private const int REQUEST_HEADER_BYTES = 48;
    private const int REQUEST_TYPE_BYTES = 8;
    private const int REQUEST_MEMBER_BYTES = 16;
    private const uint RESPONSE_MAGIC = 0x43455657;
    private const uint RESPONSE_VERSION = 1;
    private const int RESPONSE_HEADER_BYTES = 32;
    private const int METADATA_HEADER_BYTES = 24;
    private const int METADATA_TYPE_BYTES = 8;
    private const int METADATA_MEMBER_BYTES = 16;
    private const int MAXIMUM_GROUP_MEMBERS = 2_048;

    public static ImmutableArray<byte> Build(Nativeˉenumˉmetadataˉmodel model)
    {
        var Groups = Planˉgroups(model);
        var Header = ImmutableArray.CreateBuilder<byte>(METADATA_HEADER_BYTES);
        var Directories = ImmutableArray.CreateBuilder<byte>(checked(
            model.Directories.Length * METADATA_TYPE_BYTES));
        var Members = ImmutableArray.CreateBuilder<byte>(checked(
            model.Members.Length * METADATA_MEMBER_BYTES));
        var Names = ImmutableArray.CreateBuilder<byte>(model.Namesˉbytes);

        foreach (var Group in Groups)
        {
            var Request = Buildˉrequest(model, Group);
            var Response = Nativeˉenumˉmetadataˉbuilder.Buildˉwithˉwindvale(Request);
            Appendˉresponse(Group, Response, Header, Directories, Members, Names);
        }
        if (Header.Count != Header.Capacity ||
            Directories.Count != Directories.Capacity ||
            Members.Count != Members.Capacity ||
            Names.Count != Names.Capacity)
        {
            throw Invalidˉsession();
        }

        var Result = ImmutableArray.CreateBuilder<byte>(model.Totalˉbytes);
        Result.AddRange(Header);
        Result.AddRange(Directories);
        Result.AddRange(Members);
        Result.AddRange(Names);
        if (Result.Count != model.Totalˉbytes)
        {
            throw Invalidˉsession();
        }
        return Result.MoveToImmutable();
    }

    internal static ImmutableArray<ImmutableArray<byte>> Buildˉrequests(
        Nativeˉenumˉmetadataˉmodel model)
    {
        var Groups = Planˉgroups(model);
        var Result = ImmutableArray.CreateBuilder<ImmutableArray<byte>>(Groups.Length);
        foreach (var Group in Groups)
        {
            Result.Add(Buildˉrequest(model, Group));
        }
        return Result.MoveToImmutable();
    }

    private static ImmutableArray<Nativeˉenumˉmetadataˉgroup> Planˉgroups(
        Nativeˉenumˉmetadataˉmodel model)
    {
        var Result = ImmutableArray.CreateBuilder<Nativeˉenumˉmetadataˉgroup>();
        var Typeˉstart = 0;
        var Memberˉstart = 0;
        var Nameˉstart = checked(METADATA_HEADER_BYTES +
            model.Directories.Length * METADATA_TYPE_BYTES +
            model.Members.Length * METADATA_MEMBER_BYTES);
        while (Typeˉstart < model.Directories.Length)
        {
            var Typeˉcount = 0;
            var Memberˉcount = 0;
            var Nameˉbytes = 0;
            while (Typeˉstart + Typeˉcount < model.Directories.Length)
            {
                var Directory = model.Directories[Typeˉstart + Typeˉcount];
                var Nextˉtypeˉcount = checked(Typeˉcount + 1);
                var Nextˉmemberˉcount = checked(Memberˉcount + (int)Directory.Count);
                var Nextˉnameˉbytes = checked(Nameˉbytes + Directory.Namesˉbytes);
                if (!Fitsˉgroup(
                        Typeˉstart,
                        Nextˉtypeˉcount,
                        Nextˉmemberˉcount,
                        Nextˉnameˉbytes))
                {
                    if (Typeˉcount == 0)
                    {
                        throw Invalidˉsession();
                    }
                    break;
                }
                Typeˉcount = Nextˉtypeˉcount;
                Memberˉcount = Nextˉmemberˉcount;
                Nameˉbytes = Nextˉnameˉbytes;
            }

            if (model.Directories[Typeˉstart].Start != Memberˉstart)
            {
                throw Invalidˉsession();
            }
            Result.Add(new(
                Typeˉstart,
                Typeˉcount,
                Memberˉstart,
                Memberˉcount,
                Nameˉstart,
                Nameˉbytes));
            Typeˉstart = checked(Typeˉstart + Typeˉcount);
            Memberˉstart = checked(Memberˉstart + Memberˉcount);
            Nameˉstart = checked(Nameˉstart + Nameˉbytes);
        }
        if (Memberˉstart != model.Members.Length || Nameˉstart != model.Totalˉbytes)
        {
            throw Invalidˉsession();
        }
        return Result.ToImmutable();
    }

    private static bool Fitsˉgroup(
        int typeˉstart,
        int typeˉcount,
        int memberˉcount,
        int nameˉbytes)
    {
        if (memberˉcount > MAXIMUM_GROUP_MEMBERS)
        {
            return false;
        }
        var Requestˉbytes = checked(REQUEST_HEADER_BYTES +
            typeˉcount * REQUEST_TYPE_BYTES +
            memberˉcount * REQUEST_MEMBER_BYTES +
            nameˉbytes);
        var Headerˉbytes = typeˉstart == 0 ? METADATA_HEADER_BYTES : 0;
        var Responseˉbytes = checked(RESPONSE_HEADER_BYTES + Headerˉbytes +
            typeˉcount * METADATA_TYPE_BYTES +
            memberˉcount * METADATA_MEMBER_BYTES +
            nameˉbytes);
        return Requestˉbytes <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES &&
            Responseˉbytes <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES;
    }

    private static ImmutableArray<byte> Buildˉrequest(
        Nativeˉenumˉmetadataˉmodel model,
        Nativeˉenumˉmetadataˉgroup group)
    {
        var Nameˉoffset = checked(REQUEST_HEADER_BYTES +
            group.Typeˉcount * REQUEST_TYPE_BYTES +
            group.Memberˉcount * REQUEST_MEMBER_BYTES);
        var Totalˉbytes = checked(Nameˉoffset + group.Nameˉbytes);
        var Result = new byte[Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), REQUEST_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            checked((uint)model.Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(16),
            checked((uint)model.Directories.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(20),
            checked((uint)model.Members.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24),
            checked((uint)group.Typeˉstart));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(28),
            checked((uint)group.Typeˉcount));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(32),
            checked((uint)group.Memberˉstart));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(36),
            checked((uint)group.Memberˉcount));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(40),
            checked((uint)group.Nameˉstart));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(44), REQUEST_HEADER_BYTES);

        for (var Index = 0; Index < group.Typeˉcount; Index++)
        {
            var Directory = model.Directories[group.Typeˉstart + Index];
            var Offset = checked(REQUEST_HEADER_BYTES + Index * REQUEST_TYPE_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Offset), Directory.Kind);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                Directory.Count);
        }

        var Currentˉname = Nameˉoffset;
        for (var Index = 0; Index < group.Memberˉcount; Index++)
        {
            var Member = model.Members[group.Memberˉstart + Index];
            var Offset = checked(REQUEST_HEADER_BYTES +
                group.Typeˉcount * REQUEST_TYPE_BYTES +
                Index * REQUEST_MEMBER_BYTES);
            BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(Offset), Member.Value);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                checked((uint)Currentˉname));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 8),
                checked((uint)Member.Name.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 12),
                Member.Nameˉrank);
            Member.Name.CopyTo(Result, Currentˉname);
            Currentˉname = checked(Currentˉname + Member.Name.Length);
        }
        if (Currentˉname != Result.Length)
        {
            throw Invalidˉsession();
        }
        return Result.ToImmutableArray();
    }

    private static void Appendˉresponse(
        Nativeˉenumˉmetadataˉgroup group,
        ImmutableArray<byte> response,
        ImmutableArray<byte>.Builder header,
        ImmutableArray<byte>.Builder directories,
        ImmutableArray<byte>.Builder members,
        ImmutableArray<byte>.Builder names)
    {
        var Value = response.AsSpan();
        if (Value.Length < RESPONSE_HEADER_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(Value) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Value[4..]) != RESPONSE_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Value[8..]) != Value.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(Value[28..]) != 0)
        {
            throw Invalidˉsession();
        }

        var Headerˉbytes = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(Value[12..]));
        var Directoryˉbytes = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(Value[16..]));
        var Memberˉbytes = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(Value[20..]));
        var Nameˉbytes = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(Value[24..]));
        if (Headerˉbytes != (group.Typeˉstart == 0 ? METADATA_HEADER_BYTES : 0) ||
            Directoryˉbytes != checked(group.Typeˉcount * METADATA_TYPE_BYTES) ||
            Memberˉbytes != checked(group.Memberˉcount * METADATA_MEMBER_BYTES) ||
            Nameˉbytes != group.Nameˉbytes ||
            checked(RESPONSE_HEADER_BYTES + Headerˉbytes + Directoryˉbytes +
                Memberˉbytes + Nameˉbytes) != Value.Length)
        {
            throw Invalidˉsession();
        }

        var Offset = RESPONSE_HEADER_BYTES;
        header.AddRange(Value.Slice(Offset, Headerˉbytes));
        Offset = checked(Offset + Headerˉbytes);
        directories.AddRange(Value.Slice(Offset, Directoryˉbytes));
        Offset = checked(Offset + Directoryˉbytes);
        members.AddRange(Value.Slice(Offset, Memberˉbytes));
        Offset = checked(Offset + Memberˉbytes);
        names.AddRange(Value.Slice(Offset, Nameˉbytes));
    }

    private static InvalidOperationException Invalidˉsession() =>
        new("The Windvale native enum-metadata session is not canonical.");

    private readonly record struct Nativeˉenumˉmetadataˉgroup(
        int Typeˉstart,
        int Typeˉcount,
        int Memberˉstart,
        int Memberˉcount,
        int Nameˉstart,
        int Nameˉbytes);
}
