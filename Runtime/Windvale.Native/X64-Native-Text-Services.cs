using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉtextˉservices
{
    public const int ENUM_NAME_CANONICAL_SIZE = 323;
    public const string ENUM_NAME_CANONICAL_SHA256 =
        "fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a";
    public const int ENUM_NAME_CONSUMER_CANONICAL_SIZE = 592;
    public const string ENUM_NAME_CONSUMER_CANONICAL_SHA256 =
        "46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c";
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
    public const int TEXT_QUOTE_CONSUMER_CANONICAL_SIZE = 1_435;
    public const string TEXT_QUOTE_CONSUMER_CANONICAL_SHA256 =
        "306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631";
    public const int INTEGER_FORMAT_CONSUMER_CANONICAL_SIZE = 11_598;
    public const string INTEGER_FORMAT_CONSUMER_CANONICAL_SHA256 =
        "851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9";
    private const string INTEGER_FORMAT_CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Integer-Format-Services-Bridge.wvb";
    private const string TEXT_CONCAT_CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Text-Concat-Service-Bridge.wvb";
    private const string TEXT_QUOTE_CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Text-Quote-Service-Bridge.wvb";
    private const string ENUM_NAME_CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Enum-Name-Service-Bridge.wvb";
    private static readonly Lazy<ImmutableArray<byte>> ENUM_NAME_RESULT = new(
        Buildˉenumˉnameˉwithˉwindvale,
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> TEXT_CONCAT_RESULT = new(
        Buildˉtextˉconcatˉwithˉwindvale,
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> TEXT_QUOTE_RESULT = new(
        Buildˉtextˉquoteˉwithˉwindvale,
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
        Nativeˉservice.Textˉquote => Readˉtextˉquote(),
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
            Nativeˉenumˉmetadataˉbuilder.Verify(
                Types,
                code[ENUM_NAME_CANONICAL_SIZE..]);
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

    private static ImmutableArray<byte> Readˉtextˉquote() =>
        TEXT_QUOTE_RESULT.Value;

    private static ImmutableArray<byte> Buildˉtextˉquoteˉwithˉwindvale()
    {
        using var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(TEXT_QUOTE_CONSUMER_RESOURCE) ??
            throw Invalidˉtextˉquoteˉconsumer();
        if (Stream.Length != TEXT_QUOTE_CONSUMER_CANONICAL_SIZE)
        {
            throw Invalidˉtextˉquoteˉconsumer();
        }
        var Bytes = new byte[TEXT_QUOTE_CONSUMER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, TEXT_QUOTE_CONSUMER_CANONICAL_SHA256))
        {
            throw Invalidˉtextˉquoteˉconsumer();
        }

        var Verified = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Compilation = X64ˉnativeˉbackend.Compile(Verified);
        var Result = X64ˉnativeˉexecutor.Executeˉbytes(Compilation.Fragment);
        Verifyˉidentity(
            Nativeˉservice.Textˉquote,
            Result.AsSpan(),
            TEXT_QUOTE_CANONICAL_SIZE,
            TEXT_QUOTE_CANONICAL_SHA256);
        return Result;
    }

    private static InvalidOperationException Invalidˉtextˉquoteˉconsumer() =>
        new("The retained Windvale native text-quote consumer failed its exact identity contract.");

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

    private static ImmutableArray<byte> Readˉenumˉname() =>
        ENUM_NAME_RESULT.Value;

    private static ImmutableArray<byte> Buildˉenumˉnameˉwithˉwindvale()
    {
        using var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(ENUM_NAME_CONSUMER_RESOURCE) ??
            throw Invalidˉenumˉnameˉconsumer();
        if (Stream.Length != ENUM_NAME_CONSUMER_CANONICAL_SIZE)
        {
            throw Invalidˉenumˉnameˉconsumer();
        }
        var Bytes = new byte[ENUM_NAME_CONSUMER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, ENUM_NAME_CONSUMER_CANONICAL_SHA256))
        {
            throw Invalidˉenumˉnameˉconsumer();
        }

        var Verified = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Compilation = X64ˉnativeˉbackend.Compile(Verified);
        var Result = X64ˉnativeˉexecutor.Executeˉbytes(Compilation.Fragment);
        Verifyˉidentity(
            Nativeˉservice.Enumˉname,
            Result.AsSpan(),
            ENUM_NAME_CANONICAL_SIZE,
            ENUM_NAME_CANONICAL_SHA256);
        return Result;
    }

    private static InvalidOperationException Invalidˉenumˉnameˉconsumer() =>
        new("The retained Windvale native enum-name consumer failed its exact identity contract.");

    private static ImmutableArray<byte> Buildˉenumˉname(
        ImmutableArray<Nominalˉtypeˉdeclaration> types)
    {
        var Metadata = Nativeˉenumˉmetadataˉbuilder.Build(types);
        var Code = Readˉenumˉname();
        var Result = ImmutableArray.CreateBuilder<byte>(Code.Length + Metadata.Length);
        Result.AddRange(Code);
        Result.AddRange(Metadata);
        return Result.MoveToImmutable();
    }

}
