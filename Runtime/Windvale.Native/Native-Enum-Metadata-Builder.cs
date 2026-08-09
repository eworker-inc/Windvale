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
    private const long MAXIMUM_CONSUMER_INSTRUCTIONS = 100_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-Enum-Metadata-Bridge.wvnf";
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Loadˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal const int CONSUMER_CANONICAL_SIZE = 15_292;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "052be4402df26ed542107d666ed894cadb04a46ba6b2428bafc9f1879e38a072";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 137_964;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "004db29841eeaf5a448ec67c438a820832ed4af3ede0a8ae1b1d672565ea0999";

    public static ImmutableArray<byte> Build(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        var Model = Measure(types);
        var Result = Nativeˉenumˉmetadataˉsession.Build(Model);
        Verify(types, Result.AsSpan());
        return Result;
    }

    internal static ImmutableArray<ImmutableArray<byte>> Buildˉrequests(
        ImmutableArray<Nominalˉtypeˉdeclaration> types) =>
        Nativeˉenumˉmetadataˉsession.Buildˉrequests(Measure(types));

    internal static ImmutableArray<byte> Buildˉwithˉwindvale(
        ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉbytes(
            CONSUMER.Value,
            request,
            maximumˉinstructions: MAXIMUM_CONSUMER_INSTRUCTIONS);

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
            else if (Type is not Recordˉtypeˉdeclaration and
                not Variantˉtypeˉdeclaration)
            {
                throw Invalidˉmetadata();
            }
        }
        if (metadata.Length is < METADATA_HEADER_BYTES or > Nativeˉcontract.MAXIMUM_ENUM_METADATA_BYTES ||
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
                var Nameˉranks = new uint[Enum.Members.Length];
                var Orderedˉnames = Enumerable.Range(0, Enum.Members.Length)
                    .OrderBy(
                        Index => Enum.Members[Index].Name,
                        StringComparer.Ordinal)
                    .ToArray();
                for (var Rank = 0; Rank < Orderedˉnames.Length; Rank++)
                {
                    Nameˉranks[Orderedˉnames[Rank]] = checked((uint)Rank);
                }
                var Typeˉnamesˉbytes = 0;
                for (var Memberˉindex = 0; Memberˉindex < Enum.Members.Length; Memberˉindex++)
                {
                    var Member = Enum.Members[Memberˉindex];
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
                    Typeˉnamesˉbytes = checked(Typeˉnamesˉbytes + Name.Length);
                    Members.Add(new(
                        Member.Value,
                        Name.ToImmutableArray(),
                        Nameˉranks[Memberˉindex]));
                }
                Directories.Add(new(
                    1,
                    Start,
                    checked((uint)Enum.Members.Length),
                    Typeˉnamesˉbytes));
            }
            else if (Type is Recordˉtypeˉdeclaration or Variantˉtypeˉdeclaration)
            {
                Directories.Add(new(0, Start, 0, 0));
            }
            else
            {
                throw new InvalidOperationException(
                    "Native enum metadata contains an unknown nominal type.");
            }
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

    private static Nativeˉfragment Loadˉconsumer()
    {
        using var Stream = typeof(Nativeˉenumˉmetadataˉbuilder).Assembly
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
        new("The retained Windvale native enum-metadata fragment failed its exact identity contract.");

    private static InvalidOperationException Invalidˉmetadata() =>
        new("Native Enumˉname service identity metadata is invalid.");

}

internal readonly record struct Nativeˉenumˉmetadataˉdirectory(
    uint Kind,
    uint Start,
    uint Count,
    int Namesˉbytes);

internal readonly record struct Nativeˉenumˉmetadataˉmember(
    int Value,
    ImmutableArray<byte> Name,
    uint Nameˉrank);

internal sealed record Nativeˉenumˉmetadataˉmodel(
    ImmutableArray<Nativeˉenumˉmetadataˉdirectory> Directories,
    ImmutableArray<Nativeˉenumˉmetadataˉmember> Members,
    int Namesˉbytes,
    int Totalˉbytes);
