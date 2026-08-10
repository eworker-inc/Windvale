using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉfileˉinputˉservice
{
    public const int WINDOWS_CANONICAL_SIZE = 1218;
    public const int LINUX_CANONICAL_SIZE = 996;
    public const string WINDOWS_CANONICAL_SHA256 =
        "3e2fd284d4991d0f713301514d3fbf6af8ec84af7bd7289698c08a41d434c52d";
    public const string LINUX_CANONICAL_SHA256 =
        "cbd78340641fa02589d0d96b73d233a67f9404ab76c3df2b1346b2e31ca43701";
    public const int CONSUMER_CANONICAL_SIZE = 51_341;
    public const string CONSUMER_CANONICAL_SHA256 =
        "09f73787a909ae35ebc1aefb05bd88e4282ff8db7152d196f83b2798ea7c2234";

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
