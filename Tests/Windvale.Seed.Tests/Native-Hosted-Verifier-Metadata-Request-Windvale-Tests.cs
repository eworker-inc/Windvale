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
    private const int NATIVE_HOSTED_VERIFIER_METADATA_REQUEST_BYTES = 16_865;
    private const string NATIVE_HOSTED_VERIFIER_METADATA_REQUEST_SHA256 =
        "ad3d6871611e270c43fa70bb978d9bf5e026cf79148390dbe3aec06fc643487c";

    private static void Windvaleˉnativeˉhostedˉverifierˉmetadataˉrequestˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvhv-request-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        byte[] Bridgeˉbytes;
        try
        {
            var Bridgeˉpath = Path.Combine(Directoryˉpath, "WVHV-Request.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Metadata-Request.wvproj"),
                Bridgeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Bridgeˉbytes = File.ReadAllBytes(Bridgeˉpath);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
        Equal(NATIVE_HOSTED_VERIFIER_METADATA_REQUEST_BYTES, Bridgeˉbytes.Length);
        Equal(
            NATIVE_HOSTED_VERIFIER_METADATA_REQUEST_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉbytes));

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉbytes);
        var Bridgeˉnative = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        True(Bridgeˉnative.Requiredˉservices.IsEmpty,
            "The WVHV request constructor requires a service.");
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
        var Consoleˉverifier =
            Loadˉconsoleˉapplicationˉverifierˉfixture(Repository);

        ImmutableArray<byte>? Firstˉevidence = null;
        foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
        {
            var Platform = Target == Consoleˉapplicationˉtarget.Windowsˉx64
                ? Nativeˉserviceˉplatform.Windows
                : Nativeˉserviceˉplatform.Linux;
            foreach (var Profile in new[]
            {
                Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier,
                Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier,
            })
            {
                var Extended = Profile ==
                    Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier;
                var Fragment = Extended
                    ? Consoleˉverifier.Fragment
                    : Verifierˉfragment;
                var Entry = Extended
                    ? Consoleˉverifier.Entry
                    : Nativeˉentry;
                var Bundle = Extended
                    ? X64ˉnativeˉserviceˉbundle
                        .Buildˉhostedˉconsoleˉapplicationˉverifier(
                            Fragment,
                            Platform)
                    : X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                        Fragment,
                        Platform);
                var Evidence = Buildˉhostedˉverifierˉrequestˉevidence(
                    Target,
                    Bundle,
                    Entry,
                    Profile);
                var Interpreted = Reference.Runˉmainˉbytes(Evidence).Bytes;
                var Executed = X64ˉnativeˉexecutor.Executeˉbytes(
                    Bridgeˉnative,
                    Evidence);
                Sequenceˉequal(Interpreted, Executed);
                Equal(Extended ? 656 : 416, Executed.Length);
                Equal(1146508887u,
                    BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()));
                Equal(0u,
                    BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
                Equal(Extended ? 572u : 352u,
                    BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
                Equal(Extended ? 624u : 384u,
                    BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[20..]));
                Sequenceˉequal(
                    Buildˉhostedˉverifierˉmetadataˉrequest(
                        Target,
                        Bundle,
                        Entry,
                        Profile),
                    Executed[32..]);
                if (!Extended)
                {
                    Firstˉevidence ??= Evidence;
                }
            }
        }

        var Valid = Firstˉevidence ?? throw new InvalidOperationException();
        void Expectˉfailure(
            ImmutableArray<byte> input,
            uint status,
            uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(input).Bytes;
            var Executed = X64ˉnativeˉexecutor.Executeˉbytes(Bridgeˉnative, input);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(
                failureˉoffset,
                BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        Expectˉfailure(Valid[..^1], 1, 351);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 8, 351), 1, 8);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 16, 1), 4, 12);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 20, uint.MaxValue), 4, 20);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 24, 0), 5, 24);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 40, 5), 5, 24);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 48, 2), 5, 24);
        Expectˉfailure(Clearˉhostedˉverifierˉbytes(Valid, 120, 32), 6, 120);
        Expectˉfailure(Clearˉhostedˉverifierˉbytes(Valid, 152, 32), 6, 152);
        Expectˉfailure(Replaceˉhostedˉverifierˉu32(Valid, 344, 1), 4, 12);
    }

    private static ImmutableArray<byte> Buildˉhostedˉverifierˉrequestˉevidence(
        Consoleˉapplicationˉtarget target,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentry,
        Hostedˉverifierˉapplicationˉprofile profile)
    {
        var Serviceˉcount = profile ==
            Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier
            ? 11
            : 6;
        Equal(Serviceˉcount, bundle.Placements.Length);
        var Services = bundle.Placements.Select(Placement =>
            new Nativeˉpublicationˉservice(
                Placement.Service,
                Placement.Codeˉbytes)).ToImmutableArray();
        var Publicationˉrequest = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
            bundle.Nativeˉimageˉbytes,
            Services);
        var Planˉbytes = 24 + Serviceˉcount * 12;
        Equal(Planˉbytes, Publicationˉrequest.Length);

        var Evidenceˉbytes = 24 + Planˉbytes + (Serviceˉcount + 1) * 32 + 8;
        var Bytes = new byte[Evidenceˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes, 1163286103);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Bytes.AsSpan(8),
            checked((uint)Evidenceˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(12), (uint)target);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(16), (uint)profile);
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes.AsSpan(20), nativeˉentry);
        Publicationˉrequest.AsSpan().CopyTo(Bytes.AsSpan(24));
        SHA256.HashData(bundle.Imageˉbytes.AsSpan(0, bundle.Nativeˉimageˉbytes))
            .CopyTo(Bytes.AsSpan(24 + Planˉbytes, 32));
        for (var Index = 0; Index < bundle.Placements.Length; Index++)
        {
            Convert.FromHexString(bundle.Placements[Index].Sha256)
                .CopyTo(Bytes.AsSpan(24 + Planˉbytes + (Index + 1) * 32, 32));
        }
        return Bytes.ToImmutableArray();
    }
}
