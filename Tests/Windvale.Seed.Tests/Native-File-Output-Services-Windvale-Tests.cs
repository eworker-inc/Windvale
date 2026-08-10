using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_FILE_OUTPUT_CODE_SIZE = 6_576;
    private const string NATIVE_FILE_OUTPUT_CODE_SHA256 =
        "7ed9baf3a21912933045b99cb82d22d73620a318a716931db86670e5ea2212c6";
    private const int NATIVE_WINDOWS_FILE_OUTPUT_CORE_SIZE = 21_129;
    private const string NATIVE_WINDOWS_FILE_OUTPUT_CORE_SHA256 =
        "9ca03bf6f5b8678389c81e281438160ff4c96c86f11a048aba90238fdc81a45d";
    private const int NATIVE_LINUX_FILE_OUTPUT_CORE_SIZE = 18_658;
    private const string NATIVE_LINUX_FILE_OUTPUT_CORE_SHA256 =
        "834d0c45b85b26ffd3ee43e49a85c8c4ffa08f36581c02785729b276eeccdb48";

    private static void Windvaleˉnativeˉfileˉoutputˉservicesˉrun()
    {
        var Builderˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Service-Code-Builder.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-Service-Code-Builder.wv"));
        var Codeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-File-Output-Service-Code.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-File-Output-Service-Code.wv"));
        var Windowsˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-File-Output-Service-Windows.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-File-Output-Service-Windows.wv"));
        var Linuxˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-File-Output-Service-Linux.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-File-Output-Service-Linux.wv"));
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-File-Output-Services-Bridge.wv");

        var Builderˉresult = Seedˉcompiler.Compileˉmodules(Builderˉinput, []);
        True(
            Builderˉresult.Success,
            "The Windvale native service-code builder did not compile: " +
                string.Join(" | ", Builderˉresult.Diagnostics));
        Equal(NATIVE_SERVICE_CODE_BUILDER_SIZE, Builderˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_SERVICE_CODE_BUILDER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Builderˉresult.Moduleˉbytes.AsSpan()));

        var Codeˉresult = Seedˉcompiler.Compileˉmodules(
            Codeˉinput,
            [Builderˉinput]);
        True(
            Codeˉresult.Success,
            "The Windvale native file-output code module did not compile: " +
                string.Join(" | ", Codeˉresult.Diagnostics));
        Equal(NATIVE_FILE_OUTPUT_CODE_SIZE, Codeˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_FILE_OUTPUT_CODE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Codeˉresult.Moduleˉbytes.AsSpan()));

        var Coreˉdependencies = new[] { Builderˉinput, Codeˉinput };
        var Windowsˉresult = Seedˉcompiler.Compileˉmodules(
            Windowsˉinput,
            Coreˉdependencies);
        True(
            Windowsˉresult.Success,
            "The Windvale Windows file-output core did not compile: " +
                string.Join(" | ", Windowsˉresult.Diagnostics));
        Equal(NATIVE_WINDOWS_FILE_OUTPUT_CORE_SIZE, Windowsˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_WINDOWS_FILE_OUTPUT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Windowsˉresult.Moduleˉbytes.AsSpan()));

        var Linuxˉresult = Seedˉcompiler.Compileˉmodules(
            Linuxˉinput,
            Coreˉdependencies);
        True(
            Linuxˉresult.Success,
            "The Windvale Linux file-output core did not compile: " +
                string.Join(" | ", Linuxˉresult.Diagnostics));
        Equal(NATIVE_LINUX_FILE_OUTPUT_CORE_SIZE, Linuxˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_LINUX_FILE_OUTPUT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Linuxˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-File-Output-Services-Bridge.wv",
                Bridgeˉsource),
            [Builderˉinput, Codeˉinput, Linuxˉinput, Windowsˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale file-output bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉfileˉoutputˉservice.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉfileˉoutputˉservice.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-File-Output-Services-Bridge.wvb")));
        var Windowsˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉfileˉoutputˉservice),
            "Windvale.Native.Native-X64-Windows-File-Output-Service.bin");
        var Linuxˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉfileˉoutputˉservice),
            "Windvale.Native.Native-X64-Linux-File-Output-Service.bin");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Interpreted = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmainˉbytes().Bytes;
        Equal(1_610, Interpreted.Length);
        Sequenceˉequal(
            Windowsˉleaf,
            Interpreted.AsSpan(0, Windowsˉleaf.Length).ToArray());
        Sequenceˉequal(
            Linuxˉleaf,
            Interpreted.AsSpan(Windowsˉleaf.Length, Linuxˉleaf.Length).ToArray());
        Sequenceˉequal(
            Windowsˉleaf,
            X64ˉnativeˉfileˉoutputˉservice.Build(Nativeˉfileˉinputˉplatform.Windows));
        Sequenceˉequal(
            Linuxˉleaf,
            X64ˉnativeˉfileˉoutputˉservice.Build(Nativeˉfileˉinputˉplatform.Linux));
        False(
            typeof(X64ˉnativeˉfileˉoutputˉservice).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-X64-File-Output-Services-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime still embeds the file-output generator WVB.");

        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        _ = Nativeˉfragmentˉverifier.Verify(Native);
        Sequenceˉequal(Interpreted, X64ˉnativeˉexecutor.Executeˉbytes(Native));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-output-services-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "File-Output-Services.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Runtime",
                    "Windvale",
                    "Native-X64-File-Output-Services.wvproj"),
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
