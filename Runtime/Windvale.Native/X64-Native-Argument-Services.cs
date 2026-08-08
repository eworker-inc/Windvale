using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉargumentˉservices
{
    public const int CONSUMER_CANONICAL_SIZE = 20_800;
    public const string CONSUMER_CANONICAL_SHA256 =
        "0a4387f12674f08d91682898a27bf84494cbdf886c34542beeb52fd9c4a538da";
    public const int ARGUMENT_COUNT_CANONICAL_SIZE = 5;
    public const string ARGUMENT_COUNT_CANONICAL_SHA256 =
        "2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829";
    public const int ARGUMENT_CANONICAL_SIZE = 70;
    public const string ARGUMENT_CANONICAL_SHA256 =
        "2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1";
    private const string ARGUMENT_COUNT_LEAF_RESOURCE =
        "Windvale.Native.Native-X64-Argument-Count-Service.bin";
    private const string ARGUMENT_LEAF_RESOURCE =
        "Windvale.Native.Native-X64-Argument-Service.bin";
    private static readonly Lazy<ImmutableArray<byte>> ARGUMENT_COUNT_LEAF = new(
        () => Readˉartifact(
            Nativeˉservice.Processˉargumentˉcount,
            ARGUMENT_COUNT_LEAF_RESOURCE,
            ARGUMENT_COUNT_CANONICAL_SIZE,
            ARGUMENT_COUNT_CANONICAL_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> ARGUMENT_LEAF = new(
        () => Readˉartifact(
            Nativeˉservice.Processˉargument,
            ARGUMENT_LEAF_RESOURCE,
            ARGUMENT_CANONICAL_SIZE,
            ARGUMENT_CANONICAL_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);

    // ABI-14 retains the execution-owned immutable descriptor table through R15's context.
    // The normal runtime consumes both exact Windvale-generated leaves directly.
    public static ImmutableArray<byte> Build(Nativeˉservice service) => service switch
    {
        Nativeˉservice.Processˉargumentˉcount => ARGUMENT_COUNT_LEAF.Value,
        Nativeˉservice.Processˉargument => ARGUMENT_LEAF.Value,
        _ => throw new ArgumentOutOfRangeException(
            nameof(service),
            service,
            "The requested service is not an ABI-14 native argument leaf."),
    };

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

    private static ImmutableArray<byte> Readˉartifact(
        Nativeˉservice service,
        string resource,
        int expectedˉsize,
        string expectedˉsha256)
    {
        using var Stream = typeof(X64ˉnativeˉargumentˉservices).Assembly
            .GetManifestResourceStream(resource) ??
            throw Invalidˉartifact(service);
        if (Stream.Length != expectedˉsize)
        {
            throw Invalidˉartifact(service);
        }
        var Bytes = new byte[expectedˉsize];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, expectedˉsha256))
        {
            throw Invalidˉartifact(service);
        }
        return Bytes.ToImmutableArray();
    }

    private static InvalidOperationException Invalidˉartifact(Nativeˉservice service) =>
        new($"The retained Windvale native {service} leaf failed its exact identity contract.");
}
