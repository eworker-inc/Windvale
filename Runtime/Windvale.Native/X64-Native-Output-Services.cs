using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉoutputˉservices
{
    public const int WINDOWS_CANONICAL_SIZE = 258;
    public const int LINUX_CANONICAL_SIZE = 213;
    public const string WINDOWS_CONSOLE_SHA256 =
        "10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48";
    public const string WINDOWS_DIAGNOSTIC_SHA256 =
        "1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2";
    public const string LINUX_CONSOLE_SHA256 =
        "c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226";
    public const string LINUX_DIAGNOSTIC_SHA256 =
        "1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe";
    public const int CONSUMER_CANONICAL_SIZE = 14_930;
    public const string CONSUMER_CANONICAL_SHA256 =
        "209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed";

    private static readonly Lazy<ImmutableArray<byte>> WINDOWS_CONSOLE = new(
        () => Readˉartifact(
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉoutputˉplatform.Windows,
            "Windvale.Native.Native-X64-Windows-Console-Output-Service.bin",
            WINDOWS_CANONICAL_SIZE,
            WINDOWS_CONSOLE_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> WINDOWS_DIAGNOSTIC = new(
        () => Readˉartifact(
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉoutputˉplatform.Windows,
            "Windvale.Native.Native-X64-Windows-Diagnostic-Output-Service.bin",
            WINDOWS_CANONICAL_SIZE,
            WINDOWS_DIAGNOSTIC_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> LINUX_CONSOLE = new(
        () => Readˉartifact(
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉoutputˉplatform.Linux,
            "Windvale.Native.Native-X64-Linux-Console-Output-Service.bin",
            LINUX_CANONICAL_SIZE,
            LINUX_CONSOLE_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImmutableArray<byte>> LINUX_DIAGNOSTIC = new(
        () => Readˉartifact(
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉoutputˉplatform.Linux,
            "Windvale.Native.Native-X64-Linux-Diagnostic-Output-Service.bin",
            LINUX_CANONICAL_SIZE,
            LINUX_DIAGNOSTIC_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);

    // The normal runtime consumes the exact Windvale-generated platform leaves directly.
    public static ImmutableArray<byte> Build(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform) => (service, platform) switch
        {
            (Nativeˉservice.Consoleˉwriteˉline, Nativeˉoutputˉplatform.Windows) =>
                WINDOWS_CONSOLE.Value,
            (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉoutputˉplatform.Windows) =>
                WINDOWS_DIAGNOSTIC.Value,
            (Nativeˉservice.Consoleˉwriteˉline, Nativeˉoutputˉplatform.Linux) =>
                LINUX_CONSOLE.Value,
            (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉoutputˉplatform.Linux) =>
                LINUX_DIAGNOSTIC.Value,
            (Nativeˉservice.Consoleˉwriteˉline or Nativeˉservice.Diagnosticˉwriteˉline, _) =>
                throw new ArgumentOutOfRangeException(
                    nameof(platform),
                    platform,
                    "The native output service supports Windows and Linux."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(service),
                service,
                "Only native output services have an output leaf."),
        };

    public static void Verify(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform,
        ReadOnlySpan<byte> code)
    {
        var Expected = Build(service, platform);
        var Expectedˉsize = Canonicalˉsize(platform);
        var Expectedˉsha256 = Canonicalˉsha256(service, platform);
        var Actualˉsha256 = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Expectedˉsize ||
            !StringComparer.Ordinal.Equals(Actualˉsha256, Expectedˉsha256) ||
            !code.SequenceEqual(Expected.AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {platform} {service} service identity is not canonical.");
        }
    }

    public static int Canonicalˉsize(Nativeˉoutputˉplatform platform) =>
        platform switch
        {
            Nativeˉoutputˉplatform.Windows => WINDOWS_CANONICAL_SIZE,
            Nativeˉoutputˉplatform.Linux => LINUX_CANONICAL_SIZE,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    public static string Canonicalˉsha256(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform) =>
        (service, platform) switch
        {
            (Nativeˉservice.Consoleˉwriteˉline, Nativeˉoutputˉplatform.Windows) =>
                WINDOWS_CONSOLE_SHA256,
            (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉoutputˉplatform.Windows) =>
                WINDOWS_DIAGNOSTIC_SHA256,
            (Nativeˉservice.Consoleˉwriteˉline, Nativeˉoutputˉplatform.Linux) =>
                LINUX_CONSOLE_SHA256,
            (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉoutputˉplatform.Linux) =>
                LINUX_DIAGNOSTIC_SHA256,
            _ => throw new ArgumentOutOfRangeException(nameof(service)),
        };

    private static ImmutableArray<byte> Readˉartifact(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform,
        string resource,
        int expectedˉsize,
        string expectedˉsha256)
    {
        using var Stream = typeof(X64ˉnativeˉoutputˉservices).Assembly
            .GetManifestResourceStream(resource) ??
            throw Invalidˉartifact(service, platform);
        if (Stream.Length != expectedˉsize)
        {
            throw Invalidˉartifact(service, platform);
        }
        var Bytes = new byte[expectedˉsize];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, expectedˉsha256))
        {
            throw Invalidˉartifact(service, platform);
        }
        return Bytes.ToImmutableArray();
    }

    private static InvalidOperationException Invalidˉartifact(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform) =>
        new($"The retained Windvale native {platform} {service} leaf failed its exact identity contract.");
}
