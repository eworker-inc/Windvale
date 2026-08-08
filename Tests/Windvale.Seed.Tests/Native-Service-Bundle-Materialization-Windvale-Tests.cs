using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_SIZE = 15_245;
    private const string NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_SHA256 =
        "54a0cb83cba3c9c9118cfc209aaef43938f9f9a9f4212ccb9d4657ce6a139ba1";

    private static void Windvaleˉnativeˉserviceˉbundleˉmaterializationˉruns()
    {
        var Publicationˉinput = new Sourceˉmoduleˉinput(
            "Compiler/Windvale/Native-Publication-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Publication-Core.wv"));
        var Byteˉconstructionˉinput = new Sourceˉmoduleˉinput(
            "Foundation/Byte-Construction.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Byte-Construction.wv"));
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Service-Bundle-Materialization-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Service-Bundle-Materialization-Bridge.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Service-Bundle-Materialization-Bridge.wv"));

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            Coreˉinput,
            [Publicationˉinput, Byteˉconstructionˉinput]);
        True(
            Coreˉresult.Success,
            "The Windvale service-bundle materialization core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_SERVICE_BUNDLE_MATERIALIZATION_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            Bridgeˉinput,
            [Publicationˉinput, Byteˉconstructionˉinput, Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale service-bundle materialization bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉserviceˉbundleˉmaterialization.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉserviceˉbundleˉmaterialization.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Service-Bundle-Materialization-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉserviceˉbundle),
            "Windvale.Native.Native-Service-Bundle-Materialization-Bridge.wvnf");
        Equal(
            X64ˉnativeˉserviceˉbundleˉmaterialization.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            X64ˉnativeˉserviceˉbundleˉmaterialization.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(X64ˉnativeˉserviceˉbundle).Assembly.GetManifestResourceNames().Contains(
                "Windvale.Native.Native-Service-Bundle-Materialization-Bridge.wvb",
                StringComparer.Ordinal),
            "The normal runtime embeds the service-bundle materializer WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));
        True(Native.Requiredˉservices.IsEmpty, "The materializer unexpectedly requires a service.");

        var Fragment = ImmutableArray.Create<byte>(1, 2, 3, 4, 5);
        var Services = ImmutableArray.Create(
            new Nativeˉserviceˉcode(
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉserviceˉadapter.Argumentˉsnapshot,
                ImmutableArray.Create<byte>(10, 11, 12, 13, 14)),
            new Nativeˉserviceˉcode(
                Nativeˉservice.Processˉargument,
                Nativeˉserviceˉadapter.Argumentˉsnapshot,
                Enumerable.Range(20, 70).Select(Value => checked((byte)Value)).ToImmutableArray()));
        var Plan = X64ˉnativeˉpublicationˉlayout.Plan(
            Fragment.Length,
            Services.Select(Service => new Nativeˉpublicationˉservice(
                Service.Service,
                Service.Code.Length)).ToImmutableArray());
        var Request = X64ˉnativeˉserviceˉbundleˉmaterialization.Buildˉrequest(
            Fragment,
            Services);
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
        var Executed = X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
            Native,
            Request,
            1_000_000_000);
        Sequenceˉequal(Interpreted, Executed);
        var Image = X64ˉnativeˉserviceˉbundleˉmaterialization.Verifyˉresponse(
            Fragment,
            Services,
            Plan,
            Request.Length,
            Executed);
        Equal(102, Image.Length);
        Sequenceˉequal(Fragment, Image[..Fragment.Length]);
        True(Image[Fragment.Length..16].All(Value => Value == 0), "The initial gap is not zero-filled.");
        Sequenceˉequal(Services[0].Code, Image[16..21]);
        True(Image[21..32].All(Value => Value == 0x90), "The later gap is not NOP-filled.");
        Sequenceˉequal(Services[1].Code, Image[32..]);

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
            var Referenceˉresult = Reference.Runˉmainˉbytes(request).Bytes;
            var Nativeˉresult = X64ˉnativeˉexecutor.Executeˉserviceˉfreeˉbootstrapˉbytes(
                Native,
                request,
                1_000_000_000);
            Sequenceˉequal(Referenceˉresult, Nativeˉresult);
            Equal(36, Nativeˉresult.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Nativeˉresult.AsSpan()[12..]));
            Equal(
                failureˉoffset,
                BinaryPrimitives.ReadUInt32LittleEndian(Nativeˉresult.AsSpan()[16..]));
        }

        Expectˉfailure(Replaceˉu32(Request, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Request, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Request, 8, checked((uint)Request.Length - 1)), 1, 8);
        Expectˉfailure(Replaceˉu32(Request, 20, 1), 4, 20);
        Expectˉfailure(Replaceˉu32(Request, 24, 0), 5, 24);
        Expectˉfailure(Replaceˉu32(Request, 16, checked((uint)Request.Length)), 1, 16);

        var Hosted = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉboundedˉbundle profile hosted;
            capability console.write_line;
            export fn Main() -> i32 {
                console.write_line("bounded bundle");
                return 0;
            }
            """));
        var Hostedˉfragment = X64ˉnativeˉbackend.Compile(Hosted).Fragment;
        foreach (var Case in new[]
        {
            (Platform: Nativeˉserviceˉplatform.Windows,
                Output: Nativeˉoutputˉplatform.Windows),
            (Platform: Nativeˉserviceˉplatform.Linux,
                Output: Nativeˉoutputˉplatform.Linux),
        })
        {
            var Bundle = X64ˉnativeˉserviceˉbundle.Build(Hostedˉfragment, Case.Platform);
            var Leaf = X64ˉnativeˉoutputˉservices.Build(
                Nativeˉservice.Consoleˉwriteˉline,
                Case.Output);
            var Components = ImmutableArray.Create(new Nativeˉserviceˉcode(
                Nativeˉservice.Consoleˉwriteˉline,
                Case.Platform == Nativeˉserviceˉplatform.Windows
                    ? Nativeˉserviceˉadapter.Windowsˉconsoleˉwrite
                    : Nativeˉserviceˉadapter.Linuxˉconsoleˉwrite,
                Leaf));
            var Hostedˉplan = X64ˉnativeˉpublicationˉlayout.Plan(
                Hostedˉfragment.Code.Length,
                [new(Nativeˉservice.Consoleˉwriteˉline, Leaf.Length)]);
            True(
                X64ˉnativeˉserviceˉbundleˉmaterialization.Canˉmaterialize(
                    Hostedˉfragment.Code,
                    Components,
                    Hostedˉplan),
                "The bounded hosted bundle did not select Windvale materialization.");
            Sequenceˉequal(
                Bundle.Imageˉbytes,
                X64ˉnativeˉserviceˉbundleˉmaterialization.Materialize(
                    Hostedˉfragment.Code,
                    Components,
                    Hostedˉplan));
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-service-bundle-materialization-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Materialization.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Service-Bundle-Materialization.wvproj"),
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
