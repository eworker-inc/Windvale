using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Nativeˉhostedˉcontainerˉbytes(
    ImmutableArray<byte> Header,
    ImmutableArray<byte> Imports,
    ImmutableArray<byte> Relocation);

internal static class Nativeˉhostedˉcontainerˉbytesˉconstructor
{
    internal const int WINDOWS_CANONICAL_SIZE = 17_409;
    internal const string WINDOWS_CANONICAL_SHA256 =
        "f3e47f2d447ac968c2b56df2fc4656ceaffbf7f3a2c84cd16c7347e29fb3b70b";
    internal const int WINDOWS_ARTIFACT_SIZE = 183_406;
    internal const string WINDOWS_ARTIFACT_SHA256 =
        "49240c2aacfe806ab786fccdc5ef248775e09dae88fb5c8cb6ee703a9bde7e7c";
    internal const int LINUX_CANONICAL_SIZE = 12_086;
    internal const string LINUX_CANONICAL_SHA256 =
        "a502fe7e6d5aaa29bf7b629e96b14bb26346c3296256b89d2aacd069a9eede5e";
    internal const int LINUX_ARTIFACT_SIZE = 124_463;
    internal const string LINUX_ARTIFACT_SHA256 =
        "0c57b13158570163b113ba8f0faf608a804725316435ecc5485aa172666bec40";

    private const long MAXIMUM_INSTRUCTIONS = 20_000_000;
    private static readonly Lazy<Nativeˉfragment> WINDOWS = new(
        () => Readˉconsumer(
            "Windvale.Linker.Native-Hosted-Container-Windows.wvnf",
            WINDOWS_ARTIFACT_SIZE,
            WINDOWS_ARTIFACT_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<Nativeˉfragment> LINUX = new(
        () => Readˉconsumer(
            "Windvale.Linker.Native-Hosted-Container-Linux.wvnf",
            LINUX_ARTIFACT_SIZE,
            LINUX_ARTIFACT_SHA256),
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static Nativeˉhostedˉcontainerˉbytes Build(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<byte> plan)
    {
        var Windows = target == Consoleˉapplicationˉtarget.Windowsˉx64;
        var Response = Execute(target, plan);
        if (Response.Length < 32)
        {
            throw Invalid();
        }
        var Span = Response.AsSpan();
        uint Read(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(Response.AsSpan().Slice(offset));
        var Headerˉbytes = Windows ? 512u : 4096u;
        var Importˉbytes = Windows ? 4096u : 0u;
        var Relocationˉbytes = Windows ? 12u : 0u;
        var Expectedˉmagic = Windows ? 0x4257_5657u : 0x424C_5657u;
        if (Read(0) != Expectedˉmagic || Read(4) != 1 || Read(8) != Response.Length ||
            Read(12) != 0 || Read(16) != plan.Length || Read(20) != Headerˉbytes ||
            Read(24) != Importˉbytes || Read(28) != Relocationˉbytes ||
            Response.Length != 32 + Headerˉbytes + Importˉbytes + Relocationˉbytes)
        {
            throw Invalid();
        }
        var Offset = 32;
        var Header = Span.Slice(Offset, checked((int)Headerˉbytes)).ToArray().ToImmutableArray();
        Offset += checked((int)Headerˉbytes);
        var Imports = Span.Slice(Offset, checked((int)Importˉbytes)).ToArray().ToImmutableArray();
        Offset += checked((int)Importˉbytes);
        var Relocation = Span.Slice(Offset, checked((int)Relocationˉbytes))
            .ToArray().ToImmutableArray();
        return new(Header, Imports, Relocation);
    }

    internal static ImmutableArray<byte> Execute(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<byte> plan) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            target == Consoleˉapplicationˉtarget.Windowsˉx64 ? WINDOWS.Value : LINUX.Value,
            plan,
            MAXIMUM_INSTRUCTIONS);

    private static Nativeˉfragment Readˉconsumer(
        string resource,
        int expectedˉbytes,
        string expectedˉsha256)
    {
        using var Stream = typeof(Nativeˉhostedˉcontainerˉbytesˉconstructor).Assembly
            .GetManifestResourceStream(resource) ?? throw Invalid();
        if (Stream.Length != expectedˉbytes) { throw Invalid(); }
        var Bytes = new byte[expectedˉbytes];
        Stream.ReadExactly(Bytes);
        if (!StringComparer.Ordinal.Equals(
            Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant(),
            expectedˉsha256))
        {
            throw Invalid();
        }
        var Fragment = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(Bytes);
        if (!Fragment.Requiredˉservices.IsEmpty ||
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment) != new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor))
        {
            throw Invalid();
        }
        return Fragment;
    }

    private static InvalidOperationException Invalid() =>
        new("The retained Windvale hosted-container byte constructor is invalid.");
}
