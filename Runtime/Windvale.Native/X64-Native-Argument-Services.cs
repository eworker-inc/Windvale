using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉargumentˉservices
{
    public const int CONSUMER_CANONICAL_SIZE = 20_799;
    public const string CONSUMER_CANONICAL_SHA256 =
        "fca2a0ba6c3ec864a2f77295f39326b1196a675dc6defd7a749c0d5541499770";
    public const int ARGUMENT_COUNT_CANONICAL_SIZE = 5;
    public const string ARGUMENT_COUNT_CANONICAL_SHA256 =
        "2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829";
    public const int ARGUMENT_CANONICAL_SIZE = 70;
    public const string ARGUMENT_CANONICAL_SHA256 =
        "2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1";
    private const string CONSUMER_RESOURCE = "Windvale.Native.Native-Stencil-Bridge.wvb";
    private static readonly Lazy<ImmutableArray<byte>> CONSUMER_RESULT = new(
        Buildˉwithˉwindvale,
        LazyThreadSafetyMode.ExecutionAndPublication);

    // ABI-14 retains the execution-owned immutable descriptor table through R15's context.
    // Windvale validates and instantiates both platform-neutral leaves once; the runtime only
    // performs the bounded split needed to publish them to the native service table.
    public static ImmutableArray<byte> Build(Nativeˉservice service)
    {
        if (service is not (
            Nativeˉservice.Processˉargumentˉcount or
            Nativeˉservice.Processˉargument))
        {
            throw new ArgumentOutOfRangeException(
                nameof(service),
                service,
                "The requested service is not an ABI-14 native argument leaf.");
        }
        var Result = CONSUMER_RESULT.Value;
        return service switch
        {
            Nativeˉservice.Processˉargumentˉcount =>
                Result.AsSpan(0, ARGUMENT_COUNT_CANONICAL_SIZE).ToImmutableArray(),
            Nativeˉservice.Processˉargument =>
                Result.AsSpan(ARGUMENT_COUNT_CANONICAL_SIZE, ARGUMENT_CANONICAL_SIZE)
                    .ToImmutableArray(),
            _ => throw new InvalidOperationException("A checked native argument service became invalid."),
        };
    }

    public static void Verify(Nativeˉservice service, ReadOnlySpan<byte> code)
    {
        var (Expectedˉsize, Expectedˉhash) = service switch
        {
            Nativeˉservice.Processˉargumentˉcount =>
                (ARGUMENT_COUNT_CANONICAL_SIZE, ARGUMENT_COUNT_CANONICAL_SHA256),
            Nativeˉservice.Processˉargument =>
                (ARGUMENT_CANONICAL_SIZE, ARGUMENT_CANONICAL_SHA256),
            _ => throw new ArgumentOutOfRangeException(
                nameof(service),
                service,
                "The requested service is not an ABI-14 native argument leaf."),
        };
        var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Expectedˉsize ||
            !StringComparer.Ordinal.Equals(Hash, Expectedˉhash) ||
            !code.SequenceEqual(Build(service).AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {service} service identity is {code.Length} bytes / {Hash}; " +
                $"expected {Expectedˉsize} bytes / {Expectedˉhash}.");
        }
    }

    private static ImmutableArray<byte> Buildˉwithˉwindvale()
    {
        using var Stream = typeof(X64ˉnativeˉargumentˉservices).Assembly
            .GetManifestResourceStream(CONSUMER_RESOURCE) ??
            throw new InvalidOperationException("The Windvale native-stencil consumer is unavailable.");
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
        var Compilation = X64ˉnativeˉbackend.Compile(Verified);
        var Result = X64ˉnativeˉexecutor.Executeˉbytes(Compilation.Fragment);
        if (Result.Length != ARGUMENT_COUNT_CANONICAL_SIZE + ARGUMENT_CANONICAL_SIZE ||
            !Hasˉidentity(
                Result.AsSpan(0, ARGUMENT_COUNT_CANONICAL_SIZE),
                ARGUMENT_COUNT_CANONICAL_SHA256) ||
            !Hasˉidentity(
                Result.AsSpan(ARGUMENT_COUNT_CANONICAL_SIZE, ARGUMENT_CANONICAL_SIZE),
                ARGUMENT_CANONICAL_SHA256))
        {
            throw Invalidˉconsumer();
        }
        return Result;
    }

    private static bool Hasˉidentity(ReadOnlySpan<byte> bytes, string expectedˉsha256) =>
        StringComparer.Ordinal.Equals(
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            expectedˉsha256);

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native-stencil consumer failed its exact identity contract.");
}
