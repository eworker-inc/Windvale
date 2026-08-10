using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Windvaleˉnativeˉhostedˉtoolˉmetadataˉruns()
    {
        var Foundationˉinput = new Sourceˉmoduleˉinput(
            "Foundation/Byte-Construction.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Byte-Construction.wv"));
        var Admissionˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Tool-Metadata-Admission.wv"));
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Tool-Metadata-Construction-Core.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Tool-Metadata-Construction-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Tool-Metadata-Construction-Bridge.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Tool-Metadata-Construction-Bridge.wv"));

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            Coreˉinput,
            [Foundationˉinput, Admissionˉinput]);
        True(Coreˉresult.Success, string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(
            Nativeˉhostedˉtoolˉmetadataˉbuilder.CORE_CANONICAL_SIZE,
            Coreˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉtoolˉmetadataˉbuilder.CORE_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            Bridgeˉinput,
            [Foundationˉinput, Admissionˉinput, Coreˉinput]);
        True(Bridgeˉresult.Success, string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉhostedˉtoolˉmetadataˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉtoolˉmetadataˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/" +
                    "Native-Hosted-Tool-Metadata-Construction-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉhostedˉtoolˉmetadataˉbuilder),
            "Windvale.Native.Native-Hosted-Tool-Metadata-Construction-Bridge.wvnf");
        Equal(
            Nativeˉhostedˉtoolˉmetadataˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉhostedˉtoolˉmetadataˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉhostedˉtoolˉmetadataˉbuilder).Assembly
                .GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-Hosted-Tool-Metadata-Construction-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the hosted-tool metadata WVB.");

        var Module = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The metadata constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        var Reference = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        ImmutableArray<byte>? Firstˉvalidˉrequest = null;
        foreach (var Target in Enum.GetValues<Consoleˉapplicationˉtarget>())
        {
            var Bundle = Hostedˉtoolˉtestˉbundle(Target);
            foreach (var Profile in Enum.GetValues<Hostedˉcompilerˉapplicationˉprofile>())
            {
                var Inputs = new Nativeˉhostedˉtoolˉmetadataˉinputs(
                    (uint)Target,
                    (uint)Profile,
                    Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
                    0,
                    Bundle);
                var Request = Nativeˉhostedˉtoolˉmetadataˉbuilder.Buildˉrequest(Inputs);
                var Executed = Nativeˉhostedˉtoolˉmetadataˉbuilder.Buildˉwithˉwindvale(
                    Request);
                var Metadata = Nativeˉhostedˉtoolˉmetadataˉbuilder.Verifyˉresponse(
                    Inputs,
                    Request.Length,
                    Executed);
                var Expectedˉtextˉarenaˉbytes =
                    Profile == Hostedˉcompilerˉapplicationˉprofile.Buildˉdriver
                        ? Hostedˉcompilerˉapplicationˉmetadata.BUILD_DRIVER_TEXT_ARENA_BYTES
                        : Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES;
                Equal(
                    Expectedˉtextˉarenaˉbytes,
                    BinaryPrimitives.ReadUInt32LittleEndian(Metadata.AsSpan()[80..]));
                Sequenceˉequal(
                    Hostedˉcompilerˉapplicationˉmetadata.Buildˉstage0(
                        Target,
                        Capabilities,
                        Bundle,
                        Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
                        0,
                        Profile),
                    Metadata);
                _ = Hostedˉcompilerˉapplicationˉmetadata.Verify(
                    Metadata.AsSpan(),
                    Target,
                    Bundle,
                    Bundle.Imageˉbytes.AsSpan(),
                    Profile);

                if (Firstˉvalidˉrequest is null)
                {
                    Firstˉvalidˉrequest = Request;
                    Sequenceˉequal(Reference.Runˉmainˉbytes(Request).Bytes, Executed);
                }
            }
        }

        var Valid = Firstˉvalidˉrequest ?? throw new InvalidOperationException();
        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }
        static ImmutableArray<byte> Clear(
            ImmutableArray<byte> input,
            int offset,
            int length)
        {
            var Result = input.ToArray();
            Result.AsSpan(offset, length).Clear();
            return Result.ToImmutableArray();
        }

        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉhostedˉtoolˉmetadataˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        Expectˉfailure(Valid[..^1], 1, 575);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 575), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 16, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 20, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 28, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 36, 9), 4, 12);
        Expectˉfailure(Clear(Valid, 40, 32), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 72, 1), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 96, 0), 5, 96);
        Expectˉfailure(Replaceˉu32(Valid, 100, 0), 5, 96);
        Expectˉfailure(Clear(Valid, 104, 32), 5, 96);
        Expectˉfailure(Replaceˉu32(Valid, 136, 1), 5, 96);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-hosted-metadata-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Hosted-Metadata.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-Hosted-Tool-Metadata.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(Bridgeˉresult.Moduleˉbytes, File.ReadAllBytes(Nativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
