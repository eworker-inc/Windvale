using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉutf8ˉservice
{
    public const int CANONICAL_SIZE = 800;
    public const string CANONICAL_SHA256 =
        "4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf";
    public const int CONSUMER_CANONICAL_SIZE = 11_511;
    public const string CONSUMER_CANONICAL_SHA256 =
        "4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f";
    private const string CONSUMER_RESOURCE =
        "Windvale.Native.Native-X64-Utf8-Service-Bridge.wvb";
    private static readonly Lazy<ImmutableArray<byte>> CONSUMER_RESULT = new(
        Buildˉwithˉwindvale,
        LazyThreadSafetyMode.ExecutionAndPublication);

    // ABI-10 supplies a proven immutable range in R8/R9D and a verified bool cell in RCX.
    // Windvale constructs the exact shared leaf once; this recovery wrapper verifies,
    // lowers, executes, and caches only its accepted immutable bytes.
    public static ImmutableArray<byte> Build() => CONSUMER_RESULT.Value;

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

    private static ImmutableArray<byte> Buildˉwithˉwindvale()
    {
        using var Stream = typeof(X64ˉnativeˉutf8ˉservice).Assembly
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
        var Compilation = X64ˉnativeˉbackend.Compile(Verified);
        var Result = X64ˉnativeˉexecutor.Executeˉbytes(Compilation.Fragment);
        var Resultˉhash = Convert.ToHexString(SHA256.HashData(Result.AsSpan()))
            .ToLowerInvariant();
        if (Result.Length != CANONICAL_SIZE ||
            !StringComparer.Ordinal.Equals(Resultˉhash, CANONICAL_SHA256))
        {
            throw Invalidˉconsumer();
        }
        return Result;
    }

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale native UTF-8 service consumer failed its exact identity contract.");
}
