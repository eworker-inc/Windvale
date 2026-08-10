using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Windvaleˉnativeˉhostedˉtoolˉruntimeˉheaderˉruns()
    {
        var Metadataˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Tool-Metadata-Admission.wv"));
        var Foundationˉinput = new Sourceˉmoduleˉinput(
            "Foundation/Byte-Construction.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Byte-Construction.wv"));
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Tool-Runtime-Header-Core.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Tool-Runtime-Header-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Tool-Runtime-Header-Bridge.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Tool-Runtime-Header-Bridge.wv"));

        var Metadataˉresult = Seedˉcompiler.Compileˉmodules(Metadataˉinput, []);
        True(Metadataˉresult.Success, string.Join(" | ", Metadataˉresult.Diagnostics));
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.METADATA_ADMISSION_CANONICAL_SIZE,
            Metadataˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.METADATA_ADMISSION_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Metadataˉresult.Moduleˉbytes.AsSpan()));

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            Coreˉinput,
            [Foundationˉinput, Metadataˉinput]);
        True(Coreˉresult.Success, string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.CORE_CANONICAL_SIZE,
            Coreˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.CORE_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            Bridgeˉinput,
            [Foundationˉinput, Metadataˉinput, Coreˉinput]);
        True(Bridgeˉresult.Success, string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/" +
                    "Native-Hosted-Tool-Runtime-Header-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder),
            "Windvale.Native.Native-Hosted-Tool-Runtime-Header-Bridge.wvnf");
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder).Assembly
                .GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-Hosted-Tool-Runtime-Header-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the hosted-tool runtime-header WVB.");

        var Module = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The runtime-header constructor requires a service.");
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
                var Metadata = Hostedˉcompilerˉapplicationˉmetadata.Buildˉstage0(
                    Target,
                    Capabilities,
                    Bundle,
                    Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
                    0,
                    Profile);
                var Inputs = new Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs(
                    (uint)Target,
                    (uint)Profile,
                    Metadata);
                var Request = Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.Buildˉrequest(Inputs);
                var Executed = Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.Buildˉwithˉwindvale(
                    Request);
                var Header = Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.Verifyˉresponse(
                    Inputs,
                    Request.Length,
                    Executed);
                var Expectedˉtextˉarenaˉbytes =
                    Profile == Hostedˉcompilerˉapplicationˉprofile.Buildˉdriver
                        ? Hostedˉcompilerˉapplicationˉmetadata.BUILD_DRIVER_TEXT_ARENA_BYTES
                        : Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES;
                var Expectedˉnameˉstrideˉbytes =
                    Profile == Hostedˉcompilerˉapplicationˉprofile.Buildˉdriver
                        ? Hostedˉcompilerˉruntimeˉdata.BUILD_DRIVER_NAME_ARENA_STRIDE_BYTES
                        : Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES;
                Sequenceˉequal(
                    Hostedˉcompilerˉruntimeˉheaderˉstage0ˉoracle.Build(
                        Target,
                        Metadata,
                        Profile),
                    Header);
                Equal(
                    Expectedˉtextˉarenaˉbytes,
                    BinaryPrimitives.ReadUInt32LittleEndian(
                        Metadata.AsSpan()[80..]));
                Equal(
                    Expectedˉtextˉarenaˉbytes,
                    BinaryPrimitives.ReadUInt32LittleEndian(
                        Header.AsSpan()[
                            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET..]));
                Equal(
                    Expectedˉnameˉstrideˉbytes,
                    BinaryPrimitives.ReadUInt32LittleEndian(
                        Header.AsSpan()[
                            checked((int)Hostedˉcompilerˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET +
                                Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET)..]));
                Sequenceˉequal(
                    Header,
                    Hostedˉcompilerˉruntimeˉdata.Build(
                        Target,
                        Capabilities,
                        Bundle,
                        0,
                        Profile));
                _ = Hostedˉcompilerˉruntimeˉdata.Verify(
                    Header.AsSpan(),
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

        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        Expectˉfailure(Valid[..^1], 1, 1047);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 1047), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 16, 8), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 20, 1), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 24, 0), 5, 24);
        Expectˉfailure(Replaceˉu32(Valid, 24 + 12, 2), 5, 24);
        Expectˉfailure(Replaceˉu32(Valid, 24 + 864, 1), 5, 24);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-hosted-runtime-header-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Hosted-Runtime-Header.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Tool-Runtime-Header.wvproj"),
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

    private static ImmutableArray<Capabilityˉdeclaration> Hostedˉtoolˉtestˉcapabilities()
    {
        static Capabilityˉdeclaration Get(string name)
        {
            if (!Capabilityˉcatalog.Tryˉget(name, out var Declaration))
            {
                throw new InvalidOperationException($"Capability '{name}' is missing.");
            }
            return Declaration;
        }

        return
        [
            Get(Capabilityˉcatalog.CONSOLE_WRITE_LINE),
            Get(Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE),
            Get(Capabilityˉcatalog.FILE_READ_BYTES),
            Get(Capabilityˉcatalog.FILE_WRITE_BYTES),
            Get(Capabilityˉcatalog.PROCESS_ARGUMENT),
            Get(Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT),
        ];
    }

    private static Nativeˉserviceˉbundle Hostedˉtoolˉtestˉbundle(
        Consoleˉapplicationˉtarget target)
    {
        var Platform = target == Consoleˉapplicationˉtarget.Windowsˉx64
            ? Nativeˉserviceˉplatform.Windows
            : Nativeˉserviceˉplatform.Linux;
        Nativeˉservice[] Services =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Textˉutf8ˉisˉvalid,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Enumˉname,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.U32ˉformat,
            Nativeˉservice.Fileˉwriteˉbytes,
        ];
        var Image = Enumerable.Range(1, 64 + Services.Length)
            .Select(Value => checked((byte)Value))
            .ToImmutableArray();
        var Placements = Services.Select((Service, Index) =>
        {
            var Byte = Image[64 + Index];
            return new Nativeˉserviceˉbundleˉplacement(
                Service,
                Hostedˉtoolˉtestˉadapter(Service, Platform),
                Hostedˉtoolˉtestˉserviceˉoffset(Service),
                64 + Index,
                1,
                Convert.ToHexString(SHA256.HashData([Byte])).ToLowerInvariant());
        }).ToImmutableArray();
        return new(Platform, 64, Image, Placements);
    }

    private static Nativeˉserviceˉadapter Hostedˉtoolˉtestˉadapter(
        Nativeˉservice service,
        Nativeˉserviceˉplatform platform) => service switch
    {
        Nativeˉservice.Consoleˉwriteˉline => platform == Nativeˉserviceˉplatform.Windows
            ? Nativeˉserviceˉadapter.Windowsˉconsoleˉwrite
            : Nativeˉserviceˉadapter.Linuxˉconsoleˉwrite,
        Nativeˉservice.Processˉargumentˉcount or Nativeˉservice.Processˉargument =>
            Nativeˉserviceˉadapter.Argumentˉsnapshot,
        Nativeˉservice.Fileˉreadˉbytes => platform == Nativeˉserviceˉplatform.Windows
            ? Nativeˉserviceˉadapter.Windowsˉfileˉinput
            : Nativeˉserviceˉadapter.Linuxˉfileˉinput,
        Nativeˉservice.Textˉutf8ˉisˉvalid => Nativeˉserviceˉadapter.Utf8,
        Nativeˉservice.Diagnosticˉwriteˉline => platform == Nativeˉserviceˉplatform.Windows
            ? Nativeˉserviceˉadapter.Windowsˉdiagnosticˉwrite
            : Nativeˉserviceˉadapter.Linuxˉdiagnosticˉwrite,
        Nativeˉservice.Enumˉname => Nativeˉserviceˉadapter.Enumˉmetadata,
        Nativeˉservice.Textˉconcat => Nativeˉserviceˉadapter.Textˉconcat,
        Nativeˉservice.U32ˉformat => Nativeˉserviceˉadapter.U32ˉformat,
        Nativeˉservice.Fileˉwriteˉbytes => platform == Nativeˉserviceˉplatform.Windows
            ? Nativeˉserviceˉadapter.Windowsˉfileˉoutput
            : Nativeˉserviceˉadapter.Linuxˉfileˉoutput,
        _ => throw new ArgumentOutOfRangeException(nameof(service), service, null),
    };

    private static int Hostedˉtoolˉtestˉserviceˉoffset(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline => 8,
            Nativeˉservice.Processˉargumentˉcount => 16,
            Nativeˉservice.Processˉargument => 24,
            Nativeˉservice.Fileˉreadˉbytes => 32,
            Nativeˉservice.Textˉutf8ˉisˉvalid => 40,
            Nativeˉservice.Diagnosticˉwriteˉline => 48,
            Nativeˉservice.Enumˉname => 56,
            Nativeˉservice.Textˉconcat => 64,
            Nativeˉservice.U32ˉformat => 88,
            Nativeˉservice.Fileˉwriteˉbytes => 96,
            _ => throw new ArgumentOutOfRangeException(nameof(service), service, null),
        };
}
