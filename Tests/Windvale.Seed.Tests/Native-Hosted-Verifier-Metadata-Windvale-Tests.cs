using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_METADATA_BYTES = 21_566;
    private const string NATIVE_HOSTED_VERIFIER_METADATA_SHA256 =
        "dc7c88f8ec9b6ddd77695b7890eeb6292314fcabd4939239c273908f3afa894b";

    private static void Windvaleˉnativeˉhostedˉverifierˉmetadataˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvhv-metadata-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        byte[] Bridgeˉbytes;
        try
        {
            var Bridgeˉpath = Path.Combine(Directoryˉpath, "WVHV-Metadata.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Metadata.wvproj"),
                Bridgeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Bridgeˉbytes = File.ReadAllBytes(Bridgeˉpath);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
        Equal(NATIVE_HOSTED_VERIFIER_METADATA_BYTES, Bridgeˉbytes.Length);
        Equal(
            NATIVE_HOSTED_VERIFIER_METADATA_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉbytes));

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉbytes);
        var Bridgeˉnative = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        True(Bridgeˉnative.Requiredˉservices.IsEmpty,
            "The WVHV metadata constructor requires a service.");
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
            var Request = Buildˉhostedˉverifierˉmetadataˉrequest(
                Target,
                Bundle,
                Nativeˉentry);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Bridgeˉnative, Request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(1056, Executed.Length);
            Equal(1145591383u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()));
            Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(384u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
            Equal(1024u, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[20..]));
            var Metadata = Executed[32..];
            Sequenceˉequal(
                Hostedˉverifierˉapplicationˉmetadata.Build(
                    Target,
                    Verifier.Module.Capabilities,
                    Bundle,
                    Hostedˉverifierˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
                    Nativeˉentry,
                    Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier),
                Metadata);
            _ = Hostedˉverifierˉapplicationˉmetadata.Verify(
                Metadata.AsSpan(),
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

        Expectˉfailure(Valid[..^1], 1, 383);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 8, 383), 1, 8);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 16, 1), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 20, 0), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 28, 0), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 36, 5), 4, 12);
        Expectˉfailure(Clearˉhostedˉverifierˉbytes(Valid, 40, 32), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 72, 1), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 96, 0), 5, 96);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 100, 0), 5, 96);
        Expectˉfailure(Clearˉhostedˉverifierˉbytes(Valid, 104, 32), 5, 96);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 136, 1), 5, 96);
    }

    private static ImmutableArray<byte> Buildˉhostedˉverifierˉmetadataˉrequest(
        Consoleˉapplicationˉtarget target,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentry)
    {
        Equal(6, bundle.Placements.Length);
        var Bytes = new byte[384];
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes, 1381389911);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(8), 384);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(12), (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(16), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Bytes.AsSpan(20),
            Hostedˉverifierˉruntimeˉdata.BUNDLE_TEXT_OFFSET);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Bytes.AsSpan(24),
            checked((uint)bundle.Imageˉbytes.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Bytes.AsSpan(28),
            checked((uint)bundle.Nativeˉimageˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(32), nativeˉentry);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(36), 6);
        SHA256.HashData(bundle.Imageˉbytes.AsSpan(0, bundle.Nativeˉimageˉbytes))
            .CopyTo(Bytes.AsSpan(40, 32));
        for (var Index = 0; Index < bundle.Placements.Length; Index++)
        {
            var Placement = bundle.Placements[Index];
            var Offset = 96 + Index * 48;
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Offset),
                checked((uint)Placement.Imageˉoffset));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Offset + 4),
                checked((uint)Placement.Codeˉbytes));
            Convert.FromHexString(Placement.Sha256).CopyTo(Bytes.AsSpan(Offset + 8, 32));
        }
        return Bytes.ToImmutableArray();
    }

    private static ImmutableArray<byte> Replaceˉhostedˉverifierˉu32(
        ImmutableArray<byte> input,
        int offset,
        uint value)
    {
        var Result = input.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
        return Result.ToImmutableArray();
    }

    private static ImmutableArray<byte> Clearˉhostedˉverifierˉbytes(
        ImmutableArray<byte> input,
        int offset,
        int length)
    {
        var Result = input.ToArray();
        Result.AsSpan(offset, length).Clear();
        return Result.ToImmutableArray();
    }
}
