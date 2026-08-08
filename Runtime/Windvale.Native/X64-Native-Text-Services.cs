using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉtextˉservices
{
    private const uint ENUM_METADATA_MAGIC = 0x4E455657;
    private const uint ENUM_METADATA_VERSION = 1;
    private const int ENUM_METADATA_HEADER_BYTES = 24;
    private const int ENUM_METADATA_TYPE_BYTES = 8;
    private const int ENUM_METADATA_MEMBER_BYTES = 16;
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public const int ENUM_NAME_CANONICAL_SIZE = 323;
    public const string ENUM_NAME_CANONICAL_SHA256 =
        "fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a";
    public const int TEXT_CONCAT_CANONICAL_SIZE = 249;
    public const string TEXT_CONCAT_CANONICAL_SHA256 =
        "75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0";
    public const int TEXT_CONCAT_CONSUMER_CANONICAL_SIZE = 10_232;
    public const string TEXT_CONCAT_CONSUMER_CANONICAL_SHA256 =
        "87bd2e3489d3a5e4b31002858f37a5f2547706fdecc9b5f9292c736c331b9a08";
    public const int I32_FORMAT_CANONICAL_SIZE = 225;
    public const string I32_FORMAT_CANONICAL_SHA256 =
        "c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e";
    public const int U32_FORMAT_CANONICAL_SIZE = 191;
    public const string U32_FORMAT_CANONICAL_SHA256 =
        "b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43";
    public const int TEXT_QUOTE_CANONICAL_SIZE = 1165;
    public const string TEXT_QUOTE_CANONICAL_SHA256 =
        "4f334af9b6349437d36fd703edb6b5882416f033fae47906a40a4bafdc083bb7";
    public const int INTEGER_FORMAT_CONSUMER_CANONICAL_SIZE = 11_598;
    public const string INTEGER_FORMAT_CONSUMER_CANONICAL_SHA256 =
        "851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9";
    private const string INTEGER_FORMAT_CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Integer-Format-Services-Bridge.wvb";
    private const string TEXT_CONCAT_CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Text-Concat-Service-Bridge.wvb";
    private static readonly Lazy<ImmutableArray<byte>> TEXT_CONCAT_RESULT = new(
        Buildˉtextˉconcatˉwithˉwindvale,
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> INTEGER_FORMAT_RESULT = new(
        Buildˉintegerˉformatˉwithˉwindvale,
        LazyThreadSafetyMode.ExecutionAndPublication);

    // ABI-13 retains the text arena and service-failure detail through R15's context.
    // These leaves return zero on success or one after publishing an exact failure detail.
    public static ImmutableArray<byte> Build(
        Nativeˉservice service,
        ImmutableArray<Nominalˉtypeˉdeclaration> types = default) => service switch
    {
        Nativeˉservice.Enumˉname => Buildˉenumˉname(Requireˉtypes(types)),
        Nativeˉservice.Textˉconcat => Readˉtextˉconcat(),
        Nativeˉservice.Textˉquote => Buildˉtextˉquote(),
        Nativeˉservice.I32ˉformat => Readˉintegerˉformat(isˉsigned: true),
        Nativeˉservice.U32ˉformat => Readˉintegerˉformat(isˉsigned: false),
        _ => throw new ArgumentOutOfRangeException(
            nameof(service),
            service,
            "The requested service is not an ABI-13 native text leaf."),
    };

    public static void Verify(
        Nativeˉservice service,
        ReadOnlySpan<byte> code,
        ImmutableArray<Nominalˉtypeˉdeclaration> types = default)
    {
        if (service == Nativeˉservice.Enumˉname)
        {
            var Types = Requireˉtypes(types);
            if (code.Length < ENUM_NAME_CANONICAL_SIZE)
            {
                throw new InvalidOperationException(
                    $"Native {service} service identity bundle is shorter than its canonical leaf.");
            }
            Verifyˉidentity(
                service,
                code[..ENUM_NAME_CANONICAL_SIZE],
                ENUM_NAME_CANONICAL_SIZE,
                ENUM_NAME_CANONICAL_SHA256);
            Verifyˉenumˉmetadata(Types, code[ENUM_NAME_CANONICAL_SIZE..]);
            var Expected = Buildˉenumˉname(Types);
            if (!code.SequenceEqual(Expected.AsSpan()))
            {
                var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
                var Expectedˉbundleˉhash = Convert.ToHexString(SHA256.HashData(Expected.AsSpan()))
                    .ToLowerInvariant();
                throw new InvalidOperationException(
                    $"Native {service} service identity bundle is {code.Length} bytes / {Hash}; " +
                    $"expected {Expected.Length} bytes / {Expectedˉbundleˉhash}.");
            }
            return;
        }

        var (Expectedˉsize, Expectedˉhash) = service switch
        {
            Nativeˉservice.Textˉconcat =>
                (TEXT_CONCAT_CANONICAL_SIZE, TEXT_CONCAT_CANONICAL_SHA256),
            Nativeˉservice.I32ˉformat =>
                (I32_FORMAT_CANONICAL_SIZE, I32_FORMAT_CANONICAL_SHA256),
            Nativeˉservice.U32ˉformat =>
                (U32_FORMAT_CANONICAL_SIZE, U32_FORMAT_CANONICAL_SHA256),
            Nativeˉservice.Textˉquote =>
                (TEXT_QUOTE_CANONICAL_SIZE, TEXT_QUOTE_CANONICAL_SHA256),
            _ => throw new ArgumentOutOfRangeException(
                nameof(service),
                service,
                "The requested service is not an ABI-13 native text leaf."),
        };
        Verifyˉidentity(service, code, Expectedˉsize, Expectedˉhash);
    }

    private static void Verifyˉidentity(
        Nativeˉservice service,
        ReadOnlySpan<byte> code,
        int expectedˉsize,
        string expectedˉhash)
    {
        var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != expectedˉsize || !StringComparer.Ordinal.Equals(Hash, expectedˉhash))
        {
            throw new InvalidOperationException(
                $"Native {service} service identity is {code.Length} bytes / {Hash}; " +
                $"expected {expectedˉsize} bytes / {expectedˉhash}.");
        }
    }

    private static ImmutableArray<Nominalˉtypeˉdeclaration> Requireˉtypes(
        ImmutableArray<Nominalˉtypeˉdeclaration> types) =>
        !types.IsDefault
            ? types
            : throw new ArgumentException(
                "Native enum-name construction requires verified nominal metadata.",
                nameof(types));

    private static ImmutableArray<byte> Readˉtextˉconcat() =>
        TEXT_CONCAT_RESULT.Value;

    private static ImmutableArray<byte> Buildˉtextˉconcatˉwithˉwindvale()
    {
        using var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(TEXT_CONCAT_CONSUMER_RESOURCE) ??
            throw Invalidˉtextˉconcatˉconsumer();
        if (Stream.Length != TEXT_CONCAT_CONSUMER_CANONICAL_SIZE)
        {
            throw Invalidˉtextˉconcatˉconsumer();
        }
        var Bytes = new byte[TEXT_CONCAT_CONSUMER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, TEXT_CONCAT_CONSUMER_CANONICAL_SHA256))
        {
            throw Invalidˉtextˉconcatˉconsumer();
        }

        var Verified = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Compilation = X64ˉnativeˉbackend.Compile(Verified);
        var Result = X64ˉnativeˉexecutor.Executeˉbytes(Compilation.Fragment);
        Verifyˉidentity(
            Nativeˉservice.Textˉconcat,
            Result.AsSpan(),
            TEXT_CONCAT_CANONICAL_SIZE,
            TEXT_CONCAT_CANONICAL_SHA256);
        return Result;
    }

    private static InvalidOperationException Invalidˉtextˉconcatˉconsumer() =>
        new("The retained Windvale native text-concatenation consumer failed its exact identity contract.");

    private static ImmutableArray<byte> Readˉintegerˉformat(bool isˉsigned)
    {
        var Result = INTEGER_FORMAT_RESULT.Value;
        if (isˉsigned)
        {
            return Result.AsSpan(0, I32_FORMAT_CANONICAL_SIZE).ToImmutableArray();
        }
        return Result.AsSpan(I32_FORMAT_CANONICAL_SIZE, U32_FORMAT_CANONICAL_SIZE)
            .ToImmutableArray();
    }

    private static ImmutableArray<byte> Buildˉintegerˉformatˉwithˉwindvale()
    {
        using var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(INTEGER_FORMAT_CONSUMER_RESOURCE) ??
            throw Invalidˉintegerˉformatˉconsumer();
        if (Stream.Length != INTEGER_FORMAT_CONSUMER_CANONICAL_SIZE)
        {
            throw Invalidˉintegerˉformatˉconsumer();
        }
        var Bytes = new byte[INTEGER_FORMAT_CONSUMER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, INTEGER_FORMAT_CONSUMER_CANONICAL_SHA256))
        {
            throw Invalidˉintegerˉformatˉconsumer();
        }

        var Verified = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Compilation = X64ˉnativeˉbackend.Compile(Verified);
        var Result = X64ˉnativeˉexecutor.Executeˉbytes(Compilation.Fragment);
        if (Result.Length != I32_FORMAT_CANONICAL_SIZE + U32_FORMAT_CANONICAL_SIZE)
        {
            throw Invalidˉintegerˉformatˉconsumer();
        }
        Verifyˉidentity(
            Nativeˉservice.I32ˉformat,
            Result.AsSpan(0, I32_FORMAT_CANONICAL_SIZE),
            I32_FORMAT_CANONICAL_SIZE,
            I32_FORMAT_CANONICAL_SHA256);
        Verifyˉidentity(
            Nativeˉservice.U32ˉformat,
            Result.AsSpan(I32_FORMAT_CANONICAL_SIZE, U32_FORMAT_CANONICAL_SIZE),
            U32_FORMAT_CANONICAL_SIZE,
            U32_FORMAT_CANONICAL_SHA256);
        return Result;
    }

    private static InvalidOperationException Invalidˉintegerˉformatˉconsumer() =>
        new("The retained Windvale native integer-format consumer failed its exact identity contract.");

    private static ImmutableArray<byte> Buildˉenumˉname(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        var Metadata = Buildˉenumˉmetadata(types);
        var Code = new Serviceˉcodeˉbuilder();
        Code.Emit(0x48, 0x83, 0xEC, 0x30);
        Code.Emit(0x4C, 0x89, 0x14, 0x24);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x08);
        Code.Emit(0x48, 0x89, 0x4C, 0x24, 0x10);
        Code.Emit(0x44, 0x89, 0x44, 0x24, 0x18);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x1C);
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8D, 0x15);
        Code.Reference("metadata");
        Code.Emit(0x81, 0x3A);
        Code.Emitˉu32(ENUM_METADATA_MAGIC);
        Code.Branch(0x85, "failure");
        Code.Emit(0x83, 0x7A, 0x04, (byte)ENUM_METADATA_VERSION);
        Code.Branch(0x85, "failure");
        Code.Emit(0x8B, 0x44, 0x24, 0x18);
        Code.Emit(0x3B, 0x42, 0x0C);
        Code.Branch(0x83, "failure");
        Code.Emit(0x4C, 0x8D, 0x54, 0xC2, ENUM_METADATA_HEADER_BYTES);
        Code.Emit(0x41, 0x8B, 0x0A);
        Code.Emit(0x45, 0x8B, 0x42, 0x04);
        Code.Emit(0x45, 0x85, 0xC0);
        Code.Branch(0x84, "failure");
        Code.Emit(0x8B, 0x42, 0x0C);
        Code.Emit(0x4C, 0x8D, 0x54, 0xC2, ENUM_METADATA_HEADER_BYTES);
        Code.Emit(0x89, 0xC8);
        Code.Emit(0x48, 0xC1, 0xE0, 0x04);
        Code.Emit(0x49, 0x01, 0xC2);

        Code.Mark("member");
        Code.Emit(0x45, 0x85, 0xC0);
        Code.Branch(0x84, "failure");
        Code.Emit(0x8B, 0x44, 0x24, 0x1C);
        Code.Emit(0x41, 0x39, 0x02);
        Code.Branch(0x84, "member_found");
        Code.Emit(0x49, 0x83, 0xC2, ENUM_METADATA_MEMBER_BYTES);
        Code.Emit(0x41, 0xFF, 0xC8);
        Code.Jump("member");

        Code.Mark("member_found");
        Code.Emit(0x45, 0x8B, 0x5A, 0x08);
        Code.Emit(0x41, 0x81, 0xFB);
        Code.Emitˉu32(Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES);
        Code.Branch(0x87, "value_failure");
        Code.Emit(0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x89, 0x44, 0x24, 0x20);
        Code.Emit(0x89, 0xC1);
        Code.Emit(0x44, 0x01, 0xD9);
        Code.Branch(0x82, "arena_failure");
        Code.Emit(0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET);
        Code.Branch(0x87, "arena_failure");
        Code.Emit(0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x4D, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x20);
        Code.Emit(0x49, 0x01, 0xC1);
        Code.Emit(0x4C, 0x89, 0x4C, 0x24, 0x28);
        Code.Emit(0x41, 0x8B, 0x42, 0x04);
        Code.Emit(0x48, 0x01, 0xC2);
        Code.Emit(0x44, 0x89, 0xD9);

        Code.Mark("copy");
        Code.Emit(0x85, 0xC9);
        Code.Branch(0x84, "written");
        Code.Emit(0x8A, 0x02);
        Code.Emit(0x41, 0x88, 0x01);
        Code.Emit(0x48, 0xFF, 0xC2);
        Code.Emit(0x49, 0xFF, 0xC1);
        Code.Emit(0xFF, 0xC9);
        Code.Jump("copy");

        Code.Mark("written");
        Code.Emit(0x48, 0x8B, 0x4C, 0x24, 0x10);
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x28);
        Code.Emit(0x48, 0x89, 0x01);
        Code.Emit(0x44, 0x89, 0x59, 0x08);
        Code.Emit(0xC7, 0x41, 0x0C, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("value_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉvalueˉlimit, 0x00, 0x00, 0x00);
        Code.Jump("failure");
        Code.Mark("arena_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted, 0x00, 0x00, 0x00);
        Code.Mark("failure");
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0x14, 0x24);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x08);
        Code.Emit(0x48, 0x83, 0xC4, 0x30);
        Code.Emit(0xC3);
        Code.Mark("metadata");
        Code.Emit(Metadata.AsSpan());
        return Code.Finish();
    }

    private static ImmutableArray<byte> Buildˉenumˉmetadata(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        if (types.Length > Bytecodeˉlimits.MAX_NOMINAL_TYPES)
        {
            throw new InvalidOperationException("Native enum metadata exceeds the nominal-type limit.");
        }

        var Directories = new List<(uint Start, uint Count)>(types.Length);
        var Members = new List<(int Value, byte[] Name)>();
        var Namesˉbytes = 0;
        foreach (var Type in types)
        {
            var Start = checked((uint)Members.Count);
            if (Type is Enumˉtypeˉdeclaration Enum)
            {
                if (Enum.Members.IsDefaultOrEmpty ||
                    Enum.Members.Length > Bytecodeˉlimits.MAX_ENUM_MEMBERS)
                {
                    throw new InvalidOperationException("Native enum metadata has an invalid member count.");
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
                        throw new InvalidOperationException("Native enum metadata is not canonical.");
                    }
                    Namesˉbytes = checked(Namesˉbytes + Name.Length);
                    Members.Add((Member.Value, Name));
                }
                Directories.Add((Start, checked((uint)Enum.Members.Length)));
            }
            else if (Type is Recordˉtypeˉdeclaration)
            {
                Directories.Add((Start, 0));
            }
            else
            {
                throw new InvalidOperationException("Native enum metadata contains an unknown nominal type.");
            }
        }
        if (Members.Count == 0)
        {
            throw new InvalidOperationException("Native enum metadata contains no enum members.");
        }

        var Memberˉoffset = checked(ENUM_METADATA_HEADER_BYTES +
            types.Length * ENUM_METADATA_TYPE_BYTES);
        var Nameˉoffset = checked(Memberˉoffset + Members.Count * ENUM_METADATA_MEMBER_BYTES);
        var Totalˉbytes = checked(Nameˉoffset + Namesˉbytes);
        if (Totalˉbytes > Nativeˉcontract.MAXIMUM_ENUM_METADATA_BYTES)
        {
            throw new InvalidOperationException(
                $"Native enum metadata exceeds {Nativeˉcontract.MAXIMUM_ENUM_METADATA_BYTES} bytes.");
        }

        var Result = new byte[Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(0), ENUM_METADATA_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), ENUM_METADATA_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), checked((uint)types.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)Members.Count));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), ENUM_METADATA_HEADER_BYTES);

        for (var Index = 0; Index < Directories.Count; Index++)
        {
            var Offset = checked(ENUM_METADATA_HEADER_BYTES + Index * ENUM_METADATA_TYPE_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Offset), Directories[Index].Start);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Offset + 4), Directories[Index].Count);
        }

        var Currentˉnameˉoffset = Nameˉoffset;
        for (var Index = 0; Index < Members.Count; Index++)
        {
            var Offset = checked(Memberˉoffset + Index * ENUM_METADATA_MEMBER_BYTES);
            BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(Offset), Members[Index].Value);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 4),
                checked((uint)Currentˉnameˉoffset));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Offset + 8),
                checked((uint)Members[Index].Name.Length));
            Members[Index].Name.CopyTo(Result, Currentˉnameˉoffset);
            Currentˉnameˉoffset = checked(Currentˉnameˉoffset + Members[Index].Name.Length);
        }
        return Result.ToImmutableArray();
    }

    private static void Verifyˉenumˉmetadata(
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
                throw Invalidˉenumˉmetadata();
            }
        }
        if (Expectedˉmembers == 0 || metadata.Length < ENUM_METADATA_HEADER_BYTES ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata) != ENUM_METADATA_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[4..]) != ENUM_METADATA_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[8..]) != metadata.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[12..]) != types.Length ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[16..]) != Expectedˉmembers ||
            BinaryPrimitives.ReadUInt32LittleEndian(metadata[20..]) != ENUM_METADATA_HEADER_BYTES)
        {
            throw Invalidˉenumˉmetadata();
        }

        int Memberˉoffset;
        int Nameˉoffset;
        try
        {
            Memberˉoffset = checked(ENUM_METADATA_HEADER_BYTES +
                types.Length * ENUM_METADATA_TYPE_BYTES);
            Nameˉoffset = checked(Memberˉoffset +
                Expectedˉmembers * ENUM_METADATA_MEMBER_BYTES);
        }
        catch (OverflowException)
        {
            throw Invalidˉenumˉmetadata();
        }
        if (Nameˉoffset > metadata.Length)
        {
            throw Invalidˉenumˉmetadata();
        }

        var Currentˉmember = 0;
        var Currentˉname = Nameˉoffset;
        for (var Typeˉindex = 0; Typeˉindex < types.Length; Typeˉindex++)
        {
            var Directoryˉoffset = checked(ENUM_METADATA_HEADER_BYTES +
                Typeˉindex * ENUM_METADATA_TYPE_BYTES);
            var Expectedˉcount = types[Typeˉindex] is Enumˉtypeˉdeclaration Enum
                ? Enum.Members.Length
                : 0;
            if (BinaryPrimitives.ReadUInt32LittleEndian(metadata[Directoryˉoffset..]) != Currentˉmember ||
                BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Directoryˉoffset + 4)..]) != Expectedˉcount)
            {
                throw Invalidˉenumˉmetadata();
            }

            if (types[Typeˉindex] is not Enumˉtypeˉdeclaration Currentˉenum)
            {
                continue;
            }
            foreach (var Member in Currentˉenum.Members)
            {
                var Entryˉoffset = checked(Memberˉoffset +
                    Currentˉmember * ENUM_METADATA_MEMBER_BYTES);
                var Name = STRICT_UTF8.GetBytes(Member.Name);
                if (BinaryPrimitives.ReadInt32LittleEndian(metadata[Entryˉoffset..]) != Member.Value ||
                    BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Entryˉoffset + 4)..]) != Currentˉname ||
                    BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Entryˉoffset + 8)..]) != Name.Length ||
                    BinaryPrimitives.ReadUInt32LittleEndian(metadata[(Entryˉoffset + 12)..]) != 0 ||
                    Currentˉname > metadata.Length - Name.Length ||
                    !metadata.Slice(Currentˉname, Name.Length).SequenceEqual(Name))
                {
                    throw Invalidˉenumˉmetadata();
                }
                Currentˉmember++;
                Currentˉname = checked(Currentˉname + Name.Length);
            }
        }
        if (Currentˉmember != Expectedˉmembers || Currentˉname != metadata.Length)
        {
            throw Invalidˉenumˉmetadata();
        }
    }

    private static InvalidOperationException Invalidˉenumˉmetadata() =>
        new("Native Enumˉname service identity metadata is invalid.");

    private static ImmutableArray<byte> Buildˉtextˉquote()
    {
        var Code = new Serviceˉcodeˉbuilder();
        Code.Emit(0x48, 0x83, 0xEC, 0x40);
        Code.Emit(0x4C, 0x89, 0x14, 0x24);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x89, 0x4C, 0x24, 0x10);
        Code.Emit(0x4C, 0x89, 0x44, 0x24, 0x18);
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x4D, 0x8B, 0x10);
        Code.Emit(0x45, 0x8B, 0x58, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
        Code.Emit(0x41, 0xB9, 0x02, 0x00, 0x00, 0x00);

        Code.Mark("measure");
        Code.Emit(0x45, 0x85, 0xDB);
        Code.Branch(0x84, "length_ready");
        Code.Emit(0x41, 0x0F, 0xB6, 0x02);
        Code.Emit(0x49, 0xFF, 0xC2);
        Code.Emit(0x41, 0xFF, 0xCB);
        Code.Emit(0xA8, 0x80);
        Code.Branch(0x85, "measure_multibyte");
        foreach (var Escaped in new byte[] { 0x22, 0x5C, 0x08, 0x0C, 0x0A, 0x0D, 0x09 })
        {
            Code.Emit(0x3C, Escaped);
            Code.Branch(0x84, "measure_two");
        }
        Code.Emit(0x3C, 0x20);
        Code.Branch(0x82, "measure_six");
        Code.Emit(0x3C, 0x7E);
        Code.Branch(0x86, "measure_one");
        Code.Jump("measure_six");

        Code.Mark("measure_one");
        Code.Emit(0x41, 0x83, 0xC1, 0x01);
        Code.Jump("measure_check");
        Code.Mark("measure_two");
        Code.Emit(0x41, 0x83, 0xC1, 0x02);
        Code.Jump("measure_check");
        Code.Mark("measure_six");
        Code.Emit(0x41, 0x83, 0xC1, 0x06);
        Code.Jump("measure_check");
        Code.Mark("measure_twelve");
        Code.Emit(0x41, 0x83, 0xC1, 0x0C);

        Code.Mark("measure_check");
        Code.Emit(0x41, 0x81, 0xF9);
        Code.Emitˉu32(Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES);
        Code.Branch(0x87, "value_failure");
        Code.Jump("measure");

        Code.Mark("measure_multibyte");
        Code.Emit(0x3C, 0xC2);
        Code.Branch(0x82, "failure");
        Code.Emit(0x3C, 0xDF);
        Code.Branch(0x86, "measure_two_byte");
        Code.Emit(0x3C, 0xEF);
        Code.Branch(0x86, "measure_three_byte");
        Code.Emit(0x3C, 0xF4);
        Code.Branch(0x86, "measure_four_byte");
        Code.Jump("failure");

        Code.Mark("measure_two_byte");
        Code.Emit(0x41, 0x83, 0xFB, 0x01);
        Code.Branch(0x82, "failure");
        Code.Emit(0x41, 0x0F, 0xB6, 0x0A);
        Code.Emit(0x81, 0xE1, 0xC0, 0x00, 0x00, 0x00);
        Code.Emit(0x81, 0xF9, 0x80, 0x00, 0x00, 0x00);
        Code.Branch(0x85, "failure");
        Code.Emit(0x49, 0xFF, 0xC2);
        Code.Emit(0x41, 0xFF, 0xCB);
        Code.Jump("measure_six");

        Code.Mark("measure_three_byte");
        Code.Emit(0x41, 0x83, 0xFB, 0x02);
        Code.Branch(0x82, "failure");
        Code.Emit(0x41, 0x0F, 0xB6, 0x0A);
        Code.Emit(0x41, 0x0F, 0xB6, 0x52, 0x01);
        Code.Emit(0x41, 0x89, 0xC8);
        Code.Emit(0x41, 0x81, 0xE0, 0xC0, 0x00, 0x00, 0x00);
        Code.Emit(0x41, 0x81, 0xF8, 0x80, 0x00, 0x00, 0x00);
        Code.Branch(0x85, "failure");
        Code.Emit(0x41, 0x89, 0xD0);
        Code.Emit(0x41, 0x81, 0xE0, 0xC0, 0x00, 0x00, 0x00);
        Code.Emit(0x41, 0x81, 0xF8, 0x80, 0x00, 0x00, 0x00);
        Code.Branch(0x85, "failure");
        Code.Emit(0x3C, 0xE0);
        Code.Branch(0x85, "measure_not_overlong_three");
        Code.Emit(0x80, 0xF9, 0xA0);
        Code.Branch(0x82, "failure");
        Code.Mark("measure_not_overlong_three");
        Code.Emit(0x3C, 0xED);
        Code.Branch(0x85, "measure_three_valid");
        Code.Emit(0x80, 0xF9, 0x9F);
        Code.Branch(0x87, "failure");
        Code.Mark("measure_three_valid");
        Code.Emit(0x49, 0x83, 0xC2, 0x02);
        Code.Emit(0x41, 0x83, 0xEB, 0x02);
        Code.Jump("measure_six");

        Code.Mark("measure_four_byte");
        Code.Emit(0x41, 0x83, 0xFB, 0x03);
        Code.Branch(0x82, "failure");
        Code.Emit(0x41, 0x0F, 0xB6, 0x0A);
        Code.Emit(0x41, 0x0F, 0xB6, 0x52, 0x01);
        Code.Emit(0x45, 0x0F, 0xB6, 0x42, 0x02);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x20);
        Code.Emit(0x41, 0x89, 0xC9);
        Code.Emit(0x41, 0x81, 0xE1, 0xC0, 0x00, 0x00, 0x00);
        Code.Emit(0x41, 0x81, 0xF9, 0x80, 0x00, 0x00, 0x00);
        Code.Branch(0x85, "failure");
        Code.Emit(0x41, 0x89, 0xD1);
        Code.Emit(0x41, 0x81, 0xE1, 0xC0, 0x00, 0x00, 0x00);
        Code.Emit(0x41, 0x81, 0xF9, 0x80, 0x00, 0x00, 0x00);
        Code.Branch(0x85, "failure");
        Code.Emit(0x45, 0x89, 0xC1);
        Code.Emit(0x41, 0x81, 0xE1, 0xC0, 0x00, 0x00, 0x00);
        Code.Emit(0x41, 0x81, 0xF9, 0x80, 0x00, 0x00, 0x00);
        Code.Branch(0x85, "failure");
        Code.Emit(0x3C, 0xF0);
        Code.Branch(0x85, "measure_not_overlong_four");
        Code.Emit(0x80, 0xF9, 0x90);
        Code.Branch(0x82, "failure");
        Code.Mark("measure_not_overlong_four");
        Code.Emit(0x3C, 0xF4);
        Code.Branch(0x85, "measure_four_valid");
        Code.Emit(0x80, 0xF9, 0x8F);
        Code.Branch(0x87, "failure");
        Code.Mark("measure_four_valid");
        Code.Emit(0x44, 0x8B, 0x4C, 0x24, 0x20);
        Code.Emit(0x49, 0x83, 0xC2, 0x03);
        Code.Emit(0x41, 0x83, 0xEB, 0x03);
        Code.Jump("measure_twelve");

        Code.Mark("length_ready");
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x28);
        Code.Emit(0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x89, 0x44, 0x24, 0x2C);
        Code.Emit(0x89, 0xC1);
        Code.Emit(0x44, 0x01, 0xC9);
        Code.Branch(0x82, "arena_failure");
        Code.Emit(0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET);
        Code.Branch(0x87, "arena_failure");
        Code.Emit(0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x4D, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x2C);
        Code.Emit(0x49, 0x01, 0xC0);
        Code.Emit(0x4C, 0x89, 0x44, 0x24, 0x30);
        Code.Emit(0x41, 0xC6, 0x00, 0x22);
        Code.Emit(0x49, 0xFF, 0xC0);
        Code.Emit(0x4C, 0x8B, 0x54, 0x24, 0x18);
        Code.Emit(0x45, 0x8B, 0x5A, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
        Code.Emit(0x4D, 0x8B, 0x12);

        Code.Mark("write");
        Code.Emit(0x45, 0x85, 0xDB);
        Code.Branch(0x84, "write_close");
        Code.Emit(0x41, 0x0F, 0xB6, 0x02);
        Code.Emit(0x49, 0xFF, 0xC2);
        Code.Emit(0x41, 0xFF, 0xCB);
        Code.Emit(0xA8, 0x80);
        Code.Branch(0x85, "write_multibyte");
        foreach (var Escape in new (byte Value, byte Output)[]
        {
            (0x22, 0x22), (0x5C, 0x5C), (0x08, 0x62), (0x0C, 0x66),
            (0x0A, 0x6E), (0x0D, 0x72), (0x09, 0x74),
        })
        {
            Code.Emit(0x3C, Escape.Value);
            Code.Branch(0x85, $"write_not_{Escape.Value:X2}");
            Code.Emit(0xB0, Escape.Output);
            Code.Jump("write_pair");
            Code.Mark($"write_not_{Escape.Value:X2}");
        }
        Code.Emit(0x3C, 0x20);
        Code.Branch(0x82, "write_unicode");
        Code.Emit(0x3C, 0x7E);
        Code.Branch(0x87, "write_unicode");
        Code.Emit(0x41, 0x88, 0x00);
        Code.Emit(0x49, 0xFF, 0xC0);
        Code.Jump("write");

        Code.Mark("write_pair");
        Code.Emit(0x41, 0xC6, 0x00, 0x5C);
        Code.Emit(0x41, 0x88, 0x40, 0x01);
        Code.Emit(0x49, 0x83, 0xC0, 0x02);
        Code.Jump("write");
        Code.Mark("write_unicode");
        Code.Call("emit_u16");
        Code.Jump("write");

        Code.Mark("write_multibyte");
        Code.Emit(0x3C, 0xDF);
        Code.Branch(0x86, "write_two_byte");
        Code.Emit(0x3C, 0xEF);
        Code.Branch(0x86, "write_three_byte");
        Code.Jump("write_four_byte");

        Code.Mark("write_two_byte");
        Code.Emit(0x83, 0xE0, 0x1F);
        Code.Emit(0xC1, 0xE0, 0x06);
        Code.Emit(0x41, 0x0F, 0xB6, 0x0A);
        Code.Emit(0x49, 0xFF, 0xC2);
        Code.Emit(0x41, 0xFF, 0xCB);
        Code.Emit(0x83, 0xE1, 0x3F);
        Code.Emit(0x09, 0xC8);
        Code.Call("emit_u16");
        Code.Jump("write");

        Code.Mark("write_three_byte");
        Code.Emit(0x83, 0xE0, 0x0F);
        Code.Emit(0xC1, 0xE0, 0x0C);
        Code.Emit(0x41, 0x0F, 0xB6, 0x0A);
        Code.Emit(0x41, 0x0F, 0xB6, 0x52, 0x01);
        Code.Emit(0x49, 0x83, 0xC2, 0x02);
        Code.Emit(0x41, 0x83, 0xEB, 0x02);
        Code.Emit(0x83, 0xE1, 0x3F);
        Code.Emit(0xC1, 0xE1, 0x06);
        Code.Emit(0x09, 0xC8);
        Code.Emit(0x83, 0xE2, 0x3F);
        Code.Emit(0x09, 0xD0);
        Code.Call("emit_u16");
        Code.Jump("write");

        Code.Mark("write_four_byte");
        Code.Emit(0x83, 0xE0, 0x07);
        Code.Emit(0xC1, 0xE0, 0x12);
        Code.Emit(0x41, 0x0F, 0xB6, 0x0A);
        Code.Emit(0x41, 0x0F, 0xB6, 0x52, 0x01);
        Code.Emit(0x45, 0x0F, 0xB6, 0x4A, 0x02);
        Code.Emit(0x49, 0x83, 0xC2, 0x03);
        Code.Emit(0x41, 0x83, 0xEB, 0x03);
        Code.Emit(0x83, 0xE1, 0x3F);
        Code.Emit(0xC1, 0xE1, 0x0C);
        Code.Emit(0x09, 0xC8);
        Code.Emit(0x83, 0xE2, 0x3F);
        Code.Emit(0xC1, 0xE2, 0x06);
        Code.Emit(0x09, 0xD0);
        Code.Emit(0x41, 0x83, 0xE1, 0x3F);
        Code.Emit(0x44, 0x09, 0xC8);
        Code.Emit(0x2D, 0x00, 0x00, 0x01, 0x00);
        Code.Emit(0x89, 0xC2);
        Code.Emit(0xC1, 0xE8, 0x0A);
        Code.Emit(0x05, 0x00, 0xD8, 0x00, 0x00);
        Code.Emit(0x81, 0xE2, 0xFF, 0x03, 0x00, 0x00);
        Code.Emit(0x81, 0xC2, 0x00, 0xDC, 0x00, 0x00);
        Code.Emit(0x41, 0x89, 0xD1);
        Code.Call("emit_u16");
        Code.Emit(0x44, 0x89, 0xC8);
        Code.Call("emit_u16");
        Code.Jump("write");

        Code.Mark("write_close");
        Code.Emit(0x41, 0xC6, 0x00, 0x22);
        Code.Emit(0x48, 0x8B, 0x4C, 0x24, 0x10);
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x30);
        Code.Emit(0x48, 0x89, 0x01);
        Code.Emit(0x8B, 0x54, 0x24, 0x28);
        Code.Emit(0x89, 0x51, 0x08);
        Code.Emit(0xC7, 0x41, 0x0C, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("value_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉvalueˉlimit, 0x00, 0x00, 0x00);
        Code.Jump("failure");
        Code.Mark("arena_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted, 0x00, 0x00, 0x00);
        Code.Mark("failure");
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0x14, 0x24);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x08);
        Code.Emit(0x48, 0x83, 0xC4, 0x40);
        Code.Emit(0xC3);

        Code.Mark("emit_u16");
        Code.Emit(0x41, 0xC6, 0x00, 0x5C);
        Code.Emit(0x41, 0xC6, 0x40, 0x01, 0x75);
        Code.Emit(0x49, 0x83, 0xC0, 0x02);
        Code.Emit(0x89, 0xC1);
        foreach (var Shift in new byte[] { 12, 8, 4, 0 })
        {
            Code.Emit(0x89, 0xCA);
            if (Shift != 0)
            {
                Code.Emit(0xC1, 0xEA, Shift);
            }
            Code.Emit(0x83, 0xE2, 0x0F);
            Code.Call("emit_nibble");
        }
        Code.Emit(0xC3);

        Code.Mark("emit_nibble");
        Code.Emit(0x80, 0xFA, 0x09);
        Code.Branch(0x86, "emit_digit");
        Code.Emit(0x80, 0xC2, 0x37);
        Code.Jump("emit_nibble_write");
        Code.Mark("emit_digit");
        Code.Emit(0x80, 0xC2, 0x30);
        Code.Mark("emit_nibble_write");
        Code.Emit(0x41, 0x88, 0x10);
        Code.Emit(0x49, 0xFF, 0xC0);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private sealed class Serviceˉcodeˉbuilder
    {
        private readonly List<byte> Bytes = [];
        private readonly Dictionary<string, int> Labels = new(StringComparer.Ordinal);
        private readonly List<(int Offset, string Label)> Patches = [];

        public void Emit(params ReadOnlySpan<byte> bytes)
        {
            foreach (var Value in bytes)
            {
                Bytes.Add(Value);
            }
        }

        public void Emitˉu32(uint value)
        {
            Span<byte> Value = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Value, value);
            Emit(Value);
        }

        public void Reference(string label)
        {
            Patches.Add((Bytes.Count, label));
            Emit(0x00, 0x00, 0x00, 0x00);
        }

        public void Mark(string label)
        {
            if (!Labels.TryAdd(label, Bytes.Count))
            {
                throw new InvalidOperationException($"Duplicate native text-service label '{label}'.");
            }
        }

        public void Branch(byte condition, string label)
        {
            Emit(0x0F, condition);
            Patches.Add((Bytes.Count, label));
            Emit(0x00, 0x00, 0x00, 0x00);
        }

        public void Jump(string label)
        {
            Emit(0xE9);
            Reference(label);
        }

        public void Call(string label)
        {
            Emit(0xE8);
            Reference(label);
        }

        public ImmutableArray<byte> Finish()
        {
            var Result = Bytes.ToArray();
            foreach (var Patch in Patches)
            {
                if (!Labels.TryGetValue(Patch.Label, out var Target))
                {
                    throw new InvalidOperationException(
                        $"Unknown native text-service label '{Patch.Label}'.");
                }
                BinaryPrimitives.WriteInt32LittleEndian(
                    Result.AsSpan(Patch.Offset, sizeof(int)),
                    checked(Target - (Patch.Offset + sizeof(int))));
            }
            return Result.ToImmutableArray();
        }
    }
}
