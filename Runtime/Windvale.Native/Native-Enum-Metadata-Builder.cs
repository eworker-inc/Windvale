using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉenumˉmetadataˉbuilder
{
    private const uint METADATA_MAGIC = 0x4E455657;
    private const uint METADATA_VERSION = 1;
    private const int METADATA_HEADER_BYTES = 24;
    private const int METADATA_TYPE_BYTES = 8;
    private const int METADATA_MEMBER_BYTES = 16;
    private const uint REQUEST_MAGIC = 0x51455657;
    private const uint REQUEST_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 24;
    private const int REQUEST_TYPE_BYTES = 8;
    private const int REQUEST_MEMBER_BYTES = 12;
    private const long MAXIMUM_CONSUMER_INSTRUCTIONS = 100_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Enum-Metadata-Bridge.wvb";
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Loadˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal const int CONSUMER_CANONICAL_SIZE = 9_619;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "595dc56d36ed75bd9857bf5011e59d17271cf03ea6a346079474291842bd5a47";

    public static ImmutableArray<byte> Build(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        var Model = Measure(types);
        var Result = Model.Totalˉbytes <= Bytecodeˉlimits.MAX_BYTE_DATA_BYTES
            ? Buildˉwithˉwindvale(Buildˉrequest(Model))
            : Buildˉrecovery(Model);
        Verify(types, Result.AsSpan());
        return Result;
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        ImmutableArray<Nominalˉtypeˉdeclaration> types) =>
        Buildˉrequest(Measure(types));

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉbytes(
            CONSUMER.Value,
            request,
            maximumˉinstructions: MAXIMUM_CONSUMER_INSTRUCTIONS);

    internal static ImmutableArray<byte> Buildˉrecovery(
        ImmutableArray<Nominalˉtypeˉdeclaration> types) =>
        Buildˉrecovery(Measure(types));

    public static void Verify(
        ImmutableArray<Nominalˉtypeˉdeclaration> types,
        ReadOnlySpan<byte> metadata)
    {
        var Expectedˉmembers = 0;
        foreach (var Type in types)
        {
            if (Type is Enumˉtypeˉdeclaration Enum)
            {
                Expectedˉmembers = checked(Expectedˉmembers + Enum.Members.Length);
            }
            else if (Type is not Recordˉtypeˉdeclaration)
            {
                throw Invalidˉmetadata();
            }
        }
        if (Expectedˉmembers == 0 ||
            metadata.Length is < METADATA_HEADER_BYTES or > Nativeˉcontract.MAXIMUM_ENUM_METADATA_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata) != METADATA_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[4..]) != METADATA_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[8..]) != metadata.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[12..]) != types.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[16..]) != Expectedˉmembers ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[20..]) != METADATA_HEADER_BYTES)
        {
            throw Invalidˉmetadata();
        }

        int Memberˉoffset;
        int Nameˉoffset;
        try
        {
            Memberˉoffset = checked(METADATA_HEADER_BYTES +
                types.Length * METADATA_TYPE_BYTES);
            Nameˉoffset = checked(Memberˉoffset +
                Expectedˉmembers * METADATA_MEMBER_BYTES);
        }
        catch (OverflowException)
        {
            throw Invalidˉmetadata();
        }
        if (Nameˉoffset > metadata.Length)
        {
            throw Invalidˉmetadata();
        }

        var Currentˉmember = 0;
        var Currentˉname = Nameˉoffset;
        for (var Typeˉindex = 0; Typeˉindex < types.Length; Typeˉindex++)
        {
            var Directoryˉoffset = checked(METADATA_HEADER_BYTES +
                Typeˉindex * METADATA_TYPE_BYTES);
            var Expectedˉcount = types[Typeˉindex] is Enumˉtypeˉdeclaration Enum
                ? Enum.Members.Length
                : 0;
            if (BinaryPrimitives.ReadUInt32LittleEndian(metadata[Directoryˉoffset..]) != Currentˉmember ||
                BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Directoryˉoffset + 4)..]) != Expectedˉcount)
            {
                throw Invalidˉmetadata();
            }

            if (types[Typeˉindex] is not Enumˉtypeˉdeclaration Currentˉenum)
            {
                continue;
            }
            foreach (var Member in Currentˉenum.Members)
            {
                var Entryˉoffset = checked(Memberˉoffset +
                    Currentˉmember * METADATA_MEMBER_BYTES);
                var Name = STRICT_UTF8.GetBytes(Member.Name);
                if (BinaryPrimitives.ReadInt32LittleEndian(metadata[Entryˉoffset..]) != Member.Value ||
                    BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Entryˉoffset + 4)..]) != Currentˉname ||
                    BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Entryˉoffset + 8)..]) != Name.Length ||
                    BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Entryˉoffset + 12)..]) != 0 ||
                    Currentˉname > metadata.Length - Name.Length ||
                    !metadata.Slice(Currentˉname, Name.Length).SequenceEqual(Name))
                {
                    throw Invalidˉmetadata();
                }
                Currentˉmember++;
                Currentˉname = checked(Currentˉname + Name.Length);
            }
        }
        if (Currentˉmember != Expectedˉmembers || Currentˉname != metadata.Length)
        {
            throw Invalidˉmetadata();
        }
    }

    private static Nativeˉenumˉmetadataˉmodel Measure(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        if (types.IsDefault || types.Length > Bytecodeˉlimits.MAX_NOMINAL_TYPES)
        {
            throw new InvalidOperationException(
                "Native enum metadata exceeds the nominal-type limit.");
        }

        var Directories = ImmutableArray.CreateBuilder<Nativeˉenumˉmetadataˉdirectory>(
            types.Length);
        var Members = ImmutableArray.CreateBuilder<Nativeˉenumˉmetadataˉmember>();
        var Namesˉbytes = 0;
        foreach (var Type in types)
        {
            var Start = checked((uint)Members.Count);
            if (Type is Enumˉtypeˉdeclaration Enum)
            {
                if (Enum.Members.IsDefaultOrEmpty ||
                    Enum.Members.Length > Bytecodeˉlimits.MAX_ENUM_MEMBERS)
                {
                    throw new InvalidOperationException(
                        "Native enum metadata has an invalid member count.");
                }
                var Values = new HashSet<int>();
                var Names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var Member in Enum.Members)
                {
                    var Name = STRICT_UTF8.GetBytes(Member.Name);
                    if (Name.Length is 0 or > Bytecodeˉlimits.MAX_NAME_BYTES ||
                        !Seedˉnames.Isˉidentifier(Member.Name) ||
                        !Names.Add(Member.Name) ||
                        !Values.Add(Member.Value))
                    {
                        throw new InvalidOperationException(
                            "Native enum metadata is not canonical.");
                    }
                    Namesˉbytes = checked(Namesˉbytes + Name.Length);
                    Members.Add(new(Member.Value, Name.ToImmutableArray()));
                }
                Directories.Add(new(
                    1,
                    Start,
                    checked((uint)Enum.Members.Length)));
            }
            else if (Type is Recordˉtypeˉdeclaration)
            {
                Directories.Add(new(0, Start, 0));
            }
            else
            {
                throw new InvalidOperationException(
                    "Native enum metadata contains an unknown nominal type.");
            }
        }
        if (Members.Count == 0)
        {
            throw new InvalidOperationException(
                "Native enum metadata contains no enum members.");
        }

        var Totalˉbytes = checked(METADATA_HEADER_BYTES +
            types.Length * METADATA_TYPE_BYTES +
            Members.Count * METADATA_MEMBER_BYTES +
            Namesˉbytes);
        if (Totalˉbytes > Nativeˉcontract.MAXIMUM_ENUM_METADATA_BYTES)
        {
            throw new InvalidOperationException(
                $"Native enum metadata exceeds {Nativeˉcontract.MAXIMUM_ENUM_METADATA_BYTES} bytes.");
        }
        return new(
            Directories.MoveToImmutable(),
            Members.ToImmutable(),
            Namesˉbytes,
            Totalˉbytes);
    }

    private static ImmutableArray<byte> Buildˉrequest(Nativeˉenumˉmetadataˉmodel model)
    {
        var Nameˉoffset = checked(REQUEST_HEADER_BYTES +
            model.Directories.Length * REQUEST_TYPE_BYTES +
            model.Members.Length * REQUEST_MEMBER_BYTES);
        var Totalˉbytes = checked(Nameˉoffset + model.Namesˉbytes);
        if (Totalˉbytes > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new InvalidOperationException(
                "Native enum metadata request exceeds the Windvale byte-input limit.");
        }

        var Result = new byte[Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), REQUEST_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            checked((uint)model.Directories.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(16),
            checked((uint)model.Members.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), REQUEST_HEADER_BYTES);

        for (var Index = 0; Index < model.Directories.Length; Index++)
        {
            var Offset = checked(REQUEST_HEADER_BYTES + Index * REQUEST_TYPE_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset),
                model.Directories[Index].Kind);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                model.Directories[Index].Count);
        }

        var Currentˉname = Nameˉoffset;
        for (var Index = 0; Index < model.Members.Length; Index++)
        {
            var Offset = checked(REQUEST_HEADER_BYTES +
                model.Directories.Length * REQUEST_TYPE_BYTES +
                Index * REQUEST_MEMBER_BYTES);
            var Member = model.Members[Index];
            BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(Offset), Member.Value);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                checked((uint)Currentˉname));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 8),
                checked((uint)Member.Name.Length));
            Member.Name.CopyTo(Result, Currentˉname);
            Currentˉname = checked(Currentˉname + Member.Name.Length);
        }
        return Result.ToImmutableArray();
    }

    private static ImmutableArray<byte> Buildˉrecovery(Nativeˉenumˉmetadataˉmodel model)
    {
        var Memberˉoffset = checked(METADATA_HEADER_BYTES +
            model.Directories.Length * METADATA_TYPE_BYTES);
        var Nameˉoffset = checked(Memberˉoffset +
            model.Members.Length * METADATA_MEMBER_BYTES);
        var Result = new byte[model.Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, METADATA_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), METADATA_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(8),
            checked((uint)model.Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            checked((uint)model.Directories.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(16),
            checked((uint)model.Members.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), METADATA_HEADER_BYTES);

        for (var Index = 0; Index < model.Directories.Length; Index++)
        {
            var Offset = checked(METADATA_HEADER_BYTES + Index * METADATA_TYPE_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset),
                model.Directories[Index].Start);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                model.Directories[Index].Count);
        }

        var Currentˉname = Nameˉoffset;
        for (var Index = 0; Index < model.Members.Length; Index++)
        {
            var Offset = checked(Memberˉoffset + Index * METADATA_MEMBER_BYTES);
            var Member = model.Members[Index];
            BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(Offset), Member.Value);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                checked((uint)Currentˉname));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 8),
                checked((uint)Member.Name.Length));
            Member.Name.CopyTo(Result, Currentˉname);
            Currentˉname = checked(Currentˉname + Member.Name.Length);
        }
        return Result.ToImmutableArray();
    }

    private static Nativeˉfragment Loadˉconsumer()
    {
        using var Stream = typeof(Nativeˉenumˉmetadataˉbuilder).Assembly
            .GetManifestResourceStream(CONSUMER_RESOURCE) ??
            throw Invalidˉconsumer();
        if (Stream.Length != CONSUMER_CANONICAL_SIZE)
        {
            throw Invalidˉconsumer();
        }
        var Bytes = new byte[CONSUMER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CONSUMER_CANONICAL_SHA256))
        {
            throw Invalidˉconsumer();
        }

        var Verified = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Fragment = X64ˉnativeˉbackend.Compile(Verified).Fragment;
        var Shape = Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment);
        if (Shape != new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor))
        {
            throw Invalidˉconsumer();
        }
        return Fragment;
    }

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native enum-metadata consumer failed its exact identity contract.");

    private static InvalidOperationException Invalidˉmetadata() =>
        new("Native Enumˉname service identity metadata is invalid.");

    private readonly record struct Nativeˉenumˉmetadataˉdirectory(
        uint Kind,
        uint Start,
        uint Count);

    private readonly record struct Nativeˉenumˉmetadataˉmember(
        int Value,
        ImmutableArray<byte> Name);

    private sealed record Nativeˉenumˉmetadataˉmodel(
        ImmutableArray<Nativeˉenumˉmetadataˉdirectory> Directories,
        ImmutableArray<Nativeˉenumˉmetadataˉmember> Members,
        int Namesˉbytes,
        int Totalˉbytes);
}
