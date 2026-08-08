using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Nativeˉhostedˉcontainerˉconstructor
{
    // Transitional managed materializer; native recipe publication owns its removal.
    internal const int CONSUMER_CANONICAL_SIZE = 33_591;
    internal const string CONSUMER_CANONICAL_SHA256 =
        "c62b671c06212fb7450bd4d1335284988bd825402713565f94e45f5592330483";
    internal const int CONSUMER_ARTIFACT_CANONICAL_SIZE = 536_691;
    internal const string CONSUMER_ARTIFACT_CANONICAL_SHA256 =
        "58f4e2553ee423c2fcf492f69dcb494f7bd618c47a0ecd54939e04c75e87279b";

    private const uint REQUEST_MAGIC = 0x5243_5657;
    private const uint RESPONSE_MAGIC = 0x4443_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 32;
    private const int RESPONSE_HEADER_BYTES = 128;
    private const long MAXIMUM_INSTRUCTIONS = 100_000_000;
    private const string CONSUMER_RESOURCE =
        "Windvale.Linker.Native-Hosted-Container-Construction.wvnf";
    private static readonly Lazy<Nativeˉfragment> CONSUMER = new(
        Readˉconsumer,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static ImmutableArray<byte> Build(
        Consoleˉapplicationˉtarget target,
        Hostedˉcompilerˉapplicationˉprofile profile,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        ImmutableArray<byte> runtime)
    {
        var Request = Buildˉrequest(target, profile, bundle, nativeˉentryˉoffset, runtime);
        var Response = X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            Request,
            MAXIMUM_INSTRUCTIONS);
        return Materialize(target, profile, bundle, nativeˉentryˉoffset, runtime, Request, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Consoleˉapplicationˉtarget target,
        Hostedˉcompilerˉapplicationˉprofile profile,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        ImmutableArray<byte> runtime)
    {
        Verifyˉinputs(target, profile, bundle, nativeˉentryˉoffset, runtime);
        var Result = new byte[checked(REQUEST_HEADER_BYTES + runtime.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), checked((uint)target));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)profile));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), checked((uint)runtime.Length));
        runtime.AsSpan().CopyTo(Result.AsSpan(REQUEST_HEADER_BYTES));
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Execute(ImmutableArray<byte> request) =>
        X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            CONSUMER.Value,
            request,
            MAXIMUM_INSTRUCTIONS);

    internal static ImmutableArray<byte> Materialize(
        Consoleˉapplicationˉtarget target,
        Hostedˉcompilerˉapplicationˉprofile profile,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        ImmutableArray<byte> runtime,
        ImmutableArray<byte> request,
        ImmutableArray<byte> response)
    {
        Verifyˉinputs(target, profile, bundle, nativeˉentryˉoffset, runtime);
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse();
        }
        var Span = response.AsSpan();
        uint Read(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(response.AsSpan().Slice(offset));
        if (Read(0) != RESPONSE_MAGIC || Read(4) != FORMAT_VERSION ||
            Read(8) != response.Length || Read(12) != 0 || Read(16) != request.Length ||
            Read(20) != (uint)target || Read(24) != (uint)profile)
        {
            throw Invalidˉresponse();
        }

        var Windows = target == Consoleˉapplicationˉtarget.Windowsˉx64;
        var Applicationˉbytes = Read(28);
        var Headerˉbytes = Read(36);
        var Textˉfile = Read(40);
        var Startupˉbytes = Read(44);
        var Bundleˉfile = Read(48);
        var Importˉfile = Read(56);
        var Importˉbytes = Read(60);
        var Runtimeˉfile = Read(64);
        var Runtimeˉbytes = Read(68);
        var Relocationˉfile = Read(72);
        var Relocationˉbytes = Read(76);
        var Targetˉpayload = Read(96);
        var Targetˉbytes = Read(100);
        var Expectedˉheader = Windows ? 512u : 4096u;
        var Expectedˉstartup = Windows
            ? checked((uint)Windowsˉhostedˉcompilerˉstartup.BYTES)
            : checked((uint)Linuxˉhostedˉcompilerˉstartup.BYTES);
        var Expectedˉimport = Windows ? 4096u : 0u;
        var Expectedˉrelocation = Windows ? 12u : 0u;
        var Expectedˉtargetˉbytes = Windows ? 232u : 124u;
        var Bundleˉbytes = checked((uint)bundle.Imageˉbytes.Length);
        if (Applicationˉbytes < Bundleˉbytes ||
            Applicationˉbytes > checked(Bundleˉbytes + 16_384u) ||
            Applicationˉbytes > int.MaxValue || Read(32) != 0 ||
            Headerˉbytes != Expectedˉheader || Textˉfile != Headerˉbytes ||
            Startupˉbytes != Expectedˉstartup || Bundleˉfile != Textˉfile + 4096u ||
            Read(52) != Bundleˉbytes || Importˉbytes != Expectedˉimport ||
            Runtimeˉbytes != runtime.Length || Relocationˉbytes != Expectedˉrelocation ||
            Read(80) != 4096 || Read(84) == 0 || Read(88) == 0 || Read(92) == 0 ||
            Targetˉpayload != RESPONSE_HEADER_BYTES || Targetˉbytes != Expectedˉtargetˉbytes ||
            (Windows && (Importˉfile == 0 || Relocationˉfile == 0 ||
                Read(104) == 0 || Read(108) != Read(84))) ||
            (!Windows && (Importˉfile != 0 || Relocationˉfile != 0 ||
                Read(104) != 0 || Read(108) != 0)) ||
            response.Length != Targetˉpayload + Targetˉbytes ||
            Read(112) == 0 || Read(116) == 0 || Read(120) == 0 || Read(124) == 0 ||
            !Regionˉfits(0, Headerˉbytes, Applicationˉbytes) ||
            !Regionˉfits(Textˉfile, Startupˉbytes, Applicationˉbytes) ||
            !Regionˉfits(Bundleˉfile, Bundleˉbytes, Applicationˉbytes) ||
            !Regionˉfits(Importˉfile, Importˉbytes, Applicationˉbytes) ||
            !Regionˉfits(Runtimeˉfile, Runtimeˉbytes, Applicationˉbytes) ||
            !Regionˉfits(Relocationˉfile, Relocationˉbytes, Applicationˉbytes) ||
            (Windows && (!Regionˉfollows(Importˉfile, Bundleˉfile, Bundleˉbytes) ||
                !Regionˉfollows(Runtimeˉfile, Importˉfile, Importˉbytes) ||
                !Regionˉfollows(Relocationˉfile, Runtimeˉfile, Runtimeˉbytes))) ||
            (!Windows && !Regionˉfollows(Runtimeˉfile, Bundleˉfile, Bundleˉbytes)))
        {
            throw Invalidˉresponse();
        }

        var Containerˉbytes = Nativeˉhostedˉcontainerˉbytesˉconstructor.Build(target, response);
        var Targets = ImmutableArray.CreateBuilder<uint>(checked((int)(Targetˉbytes / 4)));
        for (var Offset = Targetˉpayload;
            Offset < Targetˉpayload + Targetˉbytes;
            Offset += sizeof(uint))
        {
            Targets.Add(BinaryPrimitives.ReadUInt32LittleEndian(Span[checked((int)Offset)..]));
        }
        var Object = Windows
            ? Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Windowsˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Windows-X64-Hosted-Compiler.wvo",
                Windowsˉhostedˉcompilerˉstartup.WVO_BYTES,
                Windowsˉhostedˉcompilerˉstartup.WVO_SHA256)
            : Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Linuxˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Linux-X64-Hosted-Compiler.wvo",
                Linuxˉhostedˉcompilerˉstartup.WVO_BYTES,
                Linuxˉhostedˉcompilerˉstartup.WVO_SHA256);
        var Startup = Nativeˉhostedˉstartupˉinstantiator.Build(new(
            4096,
            Startupˉbytes,
            Windows
                ? checked((uint)Windowsˉhostedˉcompilerˉstartup.SYMBOL_COUNT)
                : checked((uint)Linuxˉhostedˉcompilerˉstartup.SYMBOL_COUNT),
            Targets.ToImmutable(),
            Object));
        return Nativeˉhostedˉcontainerˉmaterializationˉsession.Build(
            response,
            Containerˉbytes.Header,
            Startup,
            bundle.Imageˉbytes,
            Containerˉbytes.Imports,
            runtime,
            Containerˉbytes.Relocation);
    }

    private static bool Regionˉfits(uint offset, uint bytes, uint total) =>
        (ulong)offset + bytes <= total;

    private static bool Regionˉfollows(uint offset, uint previousˉoffset, uint previousˉbytes) =>
        offset >= (ulong)previousˉoffset + previousˉbytes;

    private static void Verifyˉinputs(
        Consoleˉapplicationˉtarget target,
        Hostedˉcompilerˉapplicationˉprofile profile,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        ImmutableArray<byte> runtime)
    {
        if (!Enum.IsDefined(target) || !Enum.IsDefined(profile) ||
            bundle is null || runtime.IsDefault || runtime.Length != 4096)
        {
            throw new ArgumentException("The hosted-container construction inputs are invalid.");
        }
        if (target == Consoleˉapplicationˉtarget.Windowsˉx64)
        {
            Windowsˉhostedˉcompilerˉapplicationˉcontract.Validateˉbundle(bundle);
        }
        else
        {
            Linuxˉhostedˉcompilerˉapplicationˉcontract.Validateˉbundle(bundle);
        }
        if (nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentOutOfRangeException(nameof(nativeˉentryˉoffset));
        }
    }

    private static Nativeˉfragment Readˉconsumer()
    {
        using var Stream = typeof(Nativeˉhostedˉcontainerˉconstructor).Assembly
            .GetManifestResourceStream(CONSUMER_RESOURCE) ?? throw Invalidˉconsumer();
        if (Stream.Length != CONSUMER_ARTIFACT_CANONICAL_SIZE)
        {
            throw Invalidˉconsumer();
        }
        var Bytes = new byte[CONSUMER_ARTIFACT_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, CONSUMER_ARTIFACT_CANONICAL_SHA256))
        {
            throw Invalidˉconsumer();
        }
        var Fragment = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(Bytes);
        if (!Fragment.Requiredˉservices.IsEmpty ||
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment) != new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor))
        {
            throw Invalidˉconsumer();
        }
        return Fragment;
    }

    private static InvalidOperationException Invalidˉresponse() =>
        new("The Windvale hosted-container construction response is invalid.");

    private static InvalidOperationException Invalidˉconsumer() =>
        new("The retained Windvale hosted-container constructor failed its exact identity contract.");
}
