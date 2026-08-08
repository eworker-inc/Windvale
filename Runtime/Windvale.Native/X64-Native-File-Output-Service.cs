using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉfileˉoutputˉservice
{
    public const int WINDOWS_CANONICAL_SIZE = 787;
    public const int LINUX_CANONICAL_SIZE = 823;
    public const string WINDOWS_CANONICAL_SHA256 =
        "a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1";
    public const string LINUX_CANONICAL_SHA256 =
        "fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422";
    public const int CONSUMER_CANONICAL_SIZE = 33_437;
    public const string CONSUMER_CANONICAL_SHA256 =
        "441db0e0e5a90f98c7e4b12b17086f56487e7d754d7b6378a0eb2972591e64f6";

    private static readonly Lazy<ImmutableArray<byte>> WINDOWS = new(
        () => Readˉartifact(
            Nativeˉfileˉinputˉplatform.Windows,
            "Windvale.Native.Native-X64-Windows-File-Output-Service.bin",
            WINDOWS_CANONICAL_SIZE,
            WINDOWS_CANONICAL_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> LINUX = new(
        () => Readˉartifact(
            Nativeˉfileˉinputˉplatform.Linux,
            "Windvale.Native.Native-X64-Linux-File-Output-Service.bin",
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
                $"Native {platform} file-output service identity is " +
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
        using var Stream = typeof(X64ˉnativeˉfileˉoutputˉservice).Assembly
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
        new($"The retained Windvale native {platform} file-output leaf failed its exact identity contract.");
}
