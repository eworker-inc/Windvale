using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉfileˉinputˉservice
{
    public const int WINDOWS_CANONICAL_SIZE = 1218;
    public const int LINUX_CANONICAL_SIZE = 996;
    public const string WINDOWS_CANONICAL_SHA256 =
        "3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8";
    public const string LINUX_CANONICAL_SHA256 =
        "55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb";
    public const int CONSUMER_CANONICAL_SIZE = 51_341;
    public const string CONSUMER_CANONICAL_SHA256 =
        "81cb5ed76e0e885055b13ae23bfbca118c99c7ea905d3ae75a5bc87ccb35269b";

    private static readonly Lazy<ImmutableArray<byte>> WINDOWS = new(
        () => Readˉartifact(
            Nativeˉfileˉinputˉplatform.Windows,
            "Windvale.Native.Native-X64-Windows-File-Input-Service.bin",
            WINDOWS_CANONICAL_SIZE,
            WINDOWS_CANONICAL_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> LINUX = new(
        () => Readˉartifact(
            Nativeˉfileˉinputˉplatform.Linux,
            "Windvale.Native.Native-X64-Linux-File-Input-Service.bin",
            LINUX_CANONICAL_SIZE,
            LINUX_CANONICAL_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);

    // The normal runtime consumes the exact Windvale-generated platform leaves directly.
    public static ImmutableArray<byte> Build(Nativeˉfileˉinputˉplatform platform) =>
        platform switch
        {
            Nativeˉfileˉinputˉplatform.Windows => WINDOWS.Value,
            Nativeˉfileˉinputˉplatform.Linux => LINUX.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    public static void Verify(Nativeˉfileˉinputˉplatform platform, ReadOnlySpan<byte> code)
    {
        var Expected = Build(platform);
        var Actualˉhash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Canonicalˉsize(platform) ||
            !StringComparer.Ordinal.Equals(Actualˉhash, Canonicalˉsha256(platform)) ||
            !code.SequenceEqual(Expected.AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {platform} file-input service identity is " +
                $"{code.Length} bytes / {Actualˉhash}; expected " +
                $"{Expected.Length} bytes / {Canonicalˉsha256(platform)}.");
        }
    }

    public static int Canonicalˉsize(Nativeˉfileˉinputˉplatform platform) =>
        platform switch
        {
            Nativeˉfileˉinputˉplatform.Windows => WINDOWS_CANONICAL_SIZE,
            Nativeˉfileˉinputˉplatform.Linux => LINUX_CANONICAL_SIZE,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    public static string Canonicalˉsha256(Nativeˉfileˉinputˉplatform platform) =>
        platform switch
        {
            Nativeˉfileˉinputˉplatform.Windows => WINDOWS_CANONICAL_SHA256,
            Nativeˉfileˉinputˉplatform.Linux => LINUX_CANONICAL_SHA256,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    private static ImmutableArray<byte> Readˉartifact(
        Nativeˉfileˉinputˉplatform platform,
        string resource,
        int expectedˉsize,
        string expectedˉsha256)
    {
        using var Stream = typeof(X64ˉnativeˉfileˉinputˉservice).Assembly
            .GetManifestResourceStream(resource) ??
            throw Invalidˉartifact(platform);
        if (Stream.Length != expectedˉsize)
        {
            throw Invalidˉartifact(platform);
        }
        var Bytes = new byte[expectedˉsize];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, expectedˉsha256))
        {
            throw Invalidˉartifact(platform);
        }
        return Bytes.ToImmutableArray();
    }

    private static InvalidOperationException Invalidˉartifact(
        Nativeˉfileˉinputˉplatform platform) =>
        new($"The retained Windvale native {platform} file-input leaf failed its exact identity contract.");
}
