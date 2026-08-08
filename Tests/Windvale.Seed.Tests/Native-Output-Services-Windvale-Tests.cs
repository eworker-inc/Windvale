using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_WINDOWS_OUTPUT_CORE_SIZE = 9_435;
    private const string NATIVE_WINDOWS_OUTPUT_CORE_SHA256 =
        "a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983";
    private const int NATIVE_LINUX_OUTPUT_CORE_SIZE = 8_908;
    private const string NATIVE_LINUX_OUTPUT_CORE_SHA256 =
        "d3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad";

    private static void Windvaleˉnativeˉoutputˉservicesˉrun()
    {
        var Builderˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Service-Code-Builder.wv");
        var Windowsˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Output-Service-Windows.wv");
        var Linuxˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Output-Service-Linux.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Output-Services-Bridge.wv");
        var Builderˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Service-Code-Builder.wv",
            Builderˉsource);
        var Windowsˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Output-Service-Windows.wv",
            Windowsˉsource);
        var Linuxˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Output-Service-Linux.wv",
            Linuxˉsource);

        var Builderˉresult = Seedˉcompiler.Compileˉmodules(Builderˉinput, []);
        True(
            Builderˉresult.Success,
            "The Windvale native service-code builder did not compile: " +
                string.Join(" | ", Builderˉresult.Diagnostics));
        Equal(NATIVE_SERVICE_CODE_BUILDER_SIZE, Builderˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_SERVICE_CODE_BUILDER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Builderˉresult.Moduleˉbytes.AsSpan()));

        var Windowsˉresult = Seedˉcompiler.Compileˉmodules(
            Windowsˉinput,
            [Builderˉinput]);
        True(
            Windowsˉresult.Success,
            "The Windvale Windows output-service core did not compile: " +
                string.Join(" | ", Windowsˉresult.Diagnostics));
        Equal(NATIVE_WINDOWS_OUTPUT_CORE_SIZE, Windowsˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_WINDOWS_OUTPUT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Windowsˉresult.Moduleˉbytes.AsSpan()));

        var Linuxˉresult = Seedˉcompiler.Compileˉmodules(
            Linuxˉinput,
            [Builderˉinput]);
        True(
            Linuxˉresult.Success,
            "The Windvale Linux output-service core did not compile: " +
                string.Join(" | ", Linuxˉresult.Diagnostics));
        Equal(NATIVE_LINUX_OUTPUT_CORE_SIZE, Linuxˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_LINUX_OUTPUT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Linuxˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Output-Services-Bridge.wv",
                Bridgeˉsource),
            [Builderˉinput, Linuxˉinput, Windowsˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale output-service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉoutputˉservices.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉoutputˉservices.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-Output-Services-Bridge.wvb")));
        var Retainedˉleaves = new[]
        {
            Readˉembeddedˉnativeˉartifact(
                typeof(X64ˉnativeˉoutputˉservices),
                "Windvale.Native.Native-X64-Windows-Console-Output-Service.bin"),
            Readˉembeddedˉnativeˉartifact(
                typeof(X64ˉnativeˉoutputˉservices),
                "Windvale.Native.Native-X64-Windows-Diagnostic-Output-Service.bin"),
            Readˉembeddedˉnativeˉartifact(
                typeof(X64ˉnativeˉoutputˉservices),
                "Windvale.Native.Native-X64-Linux-Console-Output-Service.bin"),
            Readˉembeddedˉnativeˉartifact(
                typeof(X64ˉnativeˉoutputˉservices),
                "Windvale.Native.Native-X64-Linux-Diagnostic-Output-Service.bin"),
        };

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Interpreted = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmainˉbytes().Bytes;
        Equal(942, Interpreted.Length);
        var Offset = 0;
        foreach (var Leaf in Retainedˉleaves)
        {
            Sequenceˉequal(
                Leaf,
                Interpreted.AsSpan(Offset, Leaf.Length).ToArray());
            Offset += Leaf.Length;
        }

        Sequenceˉequal(
            Retainedˉleaves[0],
            X64ˉnativeˉoutputˉservices.Build(
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉoutputˉplatform.Windows));
        Sequenceˉequal(
            Retainedˉleaves[1],
            X64ˉnativeˉoutputˉservices.Build(
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉoutputˉplatform.Windows));
        Sequenceˉequal(
            Retainedˉleaves[2],
            X64ˉnativeˉoutputˉservices.Build(
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉoutputˉplatform.Linux));
        Sequenceˉequal(
            Retainedˉleaves[3],
            X64ˉnativeˉoutputˉservices.Build(
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉoutputˉplatform.Linux));
        False(
            typeof(X64ˉnativeˉoutputˉservices).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-X64-Output-Services-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime still embeds the output-service generator WVB.");

        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        _ = Nativeˉfragmentˉverifier.Verify(Native);
        Sequenceˉequal(
            Interpreted,
            X64ˉnativeˉexecutor.Executeˉbytes(Native));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-output-services-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Output-Services.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-X64-Output-Services.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(
                Bridgeˉresult.Moduleˉbytes,
                File.ReadAllBytes(Nativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
