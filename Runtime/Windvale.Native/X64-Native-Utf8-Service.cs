using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉutf8ˉservice
{
    public const int CANONICAL_SIZE = 800;
    public const string CANONICAL_SHA256 =
        "4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf";
    public const int CONSUMER_CANONICAL_SIZE = 11_511;
    public const string CONSUMER_CANONICAL_SHA256 =
        "4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f";
    private const string LEAF_RESOURCE =
        "Windvale.Native.Native-X64-Utf8-Service.bin";
    private static readonly Lazy<ImmutableArray<byte>> LEAF = new(
        Readˉartifact,
        LazyThreadSafetyMode.ExecutionAndPublication);

    // ABI-10 supplies a proven immutable range in R8/R9D and a verified bool cell in RCX.
    // The normal runtime consumes the exact Windvale-generated artifact directly.
    public static ImmutableArray<byte> Build() => LEAF.Value;

    public static void Verify(ReadOnlySpan<byte> code)
    {
        var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != CANONICAL_SIZE ||
            !StringComparer.Ordinal.Equals(Hash, CANONICAL_SHA256) ||
            !code.SequenceEqual(Build().AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native UTF-8 service identity is {code.Length} bytes / {Hash}; " +
                $"expected {CANONICAL_SIZE} bytes / {CANONICAL_SHA256}.");
        }
    }

    private static ImmutableArray<byte> Readˉartifact()
    {
        using var Stream = typeof(X64ˉnativeˉutf8ˉservice).Assembly
            .GetManifestResourceStream(LEAF_RESOURCE) ??
            throw Invalidˉartifact();
        if (Stream.Length != CANONICAL_SIZE)
        {
            throw Invalidˉartifact();
        }
        var Bytes = new byte[CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CANONICAL_SHA256))
        {
            throw Invalidˉartifact();
        }
        return Bytes.ToImmutableArray();
    }

    private static InvalidOperationException Invalidˉartifact() =>
        new("The retained Windvale native UTF-8 service leaf failed its exact identity contract.");
}
