using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_RUNTIME_BYTES = 17_941;
    private const string NATIVE_HOSTED_VERIFIER_RUNTIME_SHA256 =
        "cf27254409ab5d574f6b6b19feb5958d97c3076a5f3b0806208437cfde04114e";

    private static void Windvaleˉnativeˉhostedˉverifierˉruntimeˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvhv-runtime-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        byte[] Bridgeˉbytes;
        try
        {
            var Bridgeˉpath = Path.Combine(Directoryˉpath, "WVHV-Runtime.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Runtime.wvproj"),
                Bridgeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Bridgeˉbytes = File.ReadAllBytes(Bridgeˉpath);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
        Equal(NATIVE_HOSTED_VERIFIER_RUNTIME_BYTES, Bridgeˉbytes.Length);
        Equal(
            NATIVE_HOSTED_VERIFIER_RUNTIME_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉbytes));

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉbytes);
        var Bridgeˉnative = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        True(Bridgeˉnative.Requiredˉservices.IsEmpty,
            "The WVHV runtime-header constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Bridgeˉnative));
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);

        var Verifierˉbytes = File.ReadAllBytes(Path.Combine(
            Repository,
            "Artifacts",
            "Native-Front-Door",
            "Wvb",
            "Compiler-Wvb-Verifier.wvb"));
        var Verifier = Moduleˉcodec.Readˉandˉverify(Verifierˉbytes);
        var Verifierˉfragment = X64ˉnativeˉbackend.Compile(Verifier).Fragment;
        var Nativeˉentry = Verifierˉfragment.Symbols.Single(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            Symbol.Name == "Main").Offset;

        ImmutableArray<byte>? Firstˉrequest = null;
        foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
        {
            var Platform = Target == Consoleˉapplicationˉtarget.Windowsˉx64
                ? Nativeˉserviceˉplatform.Windows
                : Nativeˉserviceˉplatform.Linux;
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                Verifierˉfragment,
                Platform);
            var Metadata = Hostedˉverifierˉapplicationˉmetadata.Build(
                Target,
                Verifier.Module.Capabilities,
                Bundle,
                Hostedˉverifierˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
                Nativeˉentry,
                Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier);
            var Request = Buildˉhostedˉverifierˉruntimeˉrequest(Target, Metadata);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Bridgeˉnative, Request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(4128, Executed.Length);
            Equal(1397249623u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()));
            Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(1048u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
            Equal(4096u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[20..]));
            var Header = Executed[32..];
            Sequenceˉequal(
                Hostedˉverifierˉruntimeˉdata.Build(
                    Target,
                    Verifier.Module.Capabilities,
                    Bundle,
                    Nativeˉentry,
                    Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier),
                Header);
            _ = Hostedˉverifierˉruntimeˉdata.Verify(
                Header.AsSpan(),
                Target,
                Bundle,
                Bundle.Imageˉbytes.AsSpan(),
                Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier);
            Firstˉrequest ??= Request;
        }

        var Valid = Firstˉrequest ?? throw new InvalidOperationException();
        void Expectˉfailure(
            ImmutableArray<byte> request,
            uint status,
            uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Bridgeˉnative, request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(
                failureˉoffset,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        Expectˉfailure(Valid[..^1], 1, 1047);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 8, 1047), 1, 8);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 16, 1), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 20, 1), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 24, 0), 5, 24);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 24 + 12, 2), 5, 24);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 24 + 864, 1), 5, 24);
    }

    private static ImmutableArray<byte> Buildˉhostedˉverifierˉruntimeˉrequest(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<byte> metadata)
    {
        Equal(1024, metadata.Length);
        var Bytes = new byte[1048];
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes, 1380472407);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(8), 1048);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(12), (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(16), 2);
        metadata.AsSpan().CopyTo(Bytes.AsSpan(24));
        return Bytes.ToImmutableArray();
    }
}
