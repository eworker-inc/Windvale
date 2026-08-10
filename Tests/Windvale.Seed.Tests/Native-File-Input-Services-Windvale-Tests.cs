using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_FILE_INPUT_CODE_SIZE = 7_869;
    private const string NATIVE_FILE_INPUT_CODE_SHA256 =
        "e2bfd4521b8f22529f3747eef196bdf7fa7aa0e97644db23ed45939aa10a1a7a";
    private const int NATIVE_WINDOWS_FILE_INPUT_CORE_SIZE = 32_085;
    private const string NATIVE_WINDOWS_FILE_INPUT_CORE_SHA256 =
        "6155c4ebb8f4ea76a5d1f22c1bb788aec51e731ceb4a1c5a4ceb7551ba8f409a";
    private const int NATIVE_LINUX_FILE_INPUT_CORE_SIZE = 26_718;
    private const string NATIVE_LINUX_FILE_INPUT_CORE_SHA256 =
        "04533e8ecade1f29e0b706c75ec949f5b4c300074cfd65feacb86f5107dcaeba";

    private static void Windvaleˉnativeˉfileˉinputˉservicesˉrun()
    {
        var Builderˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Service-Code-Builder.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-Service-Code-Builder.wv"));
        var Codeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-File-Input-Service-Code.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-File-Input-Service-Code.wv"));
        var Windowsˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-File-Input-Service-Windows.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-File-Input-Service-Windows.wv"));
        var Linuxˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-File-Input-Service-Linux.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-X64-File-Input-Service-Linux.wv"));
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-File-Input-Services-Bridge.wv");

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
            "The Windvale native file-input code module did not compile: " +
                string.Join(" | ", Codeˉresult.Diagnostics));
        Equal(NATIVE_FILE_INPUT_CODE_SIZE, Codeˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_FILE_INPUT_CODE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Codeˉresult.Moduleˉbytes.AsSpan()));

        var Coreˉdependencies = new[] { Builderˉinput, Codeˉinput };
        var Windowsˉresult = Seedˉcompiler.Compileˉmodules(
            Windowsˉinput,
            Coreˉdependencies);
        True(
            Windowsˉresult.Success,
            "The Windvale Windows file-input core did not compile: " +
                string.Join(" | ", Windowsˉresult.Diagnostics));
        Equal(NATIVE_WINDOWS_FILE_INPUT_CORE_SIZE, Windowsˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_WINDOWS_FILE_INPUT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Windowsˉresult.Moduleˉbytes.AsSpan()));

        var Linuxˉresult = Seedˉcompiler.Compileˉmodules(
            Linuxˉinput,
            Coreˉdependencies);
        True(
            Linuxˉresult.Success,
            "The Windvale Linux file-input core did not compile: " +
                string.Join(" | ", Linuxˉresult.Diagnostics));
        Equal(NATIVE_LINUX_FILE_INPUT_CORE_SIZE, Linuxˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_LINUX_FILE_INPUT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Linuxˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-File-Input-Services-Bridge.wv",
                Bridgeˉsource),
            [Builderˉinput, Codeˉinput, Linuxˉinput, Windowsˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale file-input bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉfileˉinputˉservice.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉfileˉinputˉservice.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-File-Input-Services-Bridge.wvb")));
        var Windowsˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉfileˉinputˉservice),
            "Windvale.Native.Native-X64-Windows-File-Input-Service.bin");
        var Linuxˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉfileˉinputˉservice),
            "Windvale.Native.Native-X64-Linux-File-Input-Service.bin");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Interpreted = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmainˉbytes().Bytes;
        Equal(2_214, Interpreted.Length);
        Sequenceˉequal(
            Windowsˉleaf,
            Interpreted.AsSpan(0, Windowsˉleaf.Length).ToArray());
        Sequenceˉequal(
            Linuxˉleaf,
            Interpreted.AsSpan(Windowsˉleaf.Length, Linuxˉleaf.Length).ToArray());
        Sequenceˉequal(
            Windowsˉleaf,
            X64ˉnativeˉfileˉinputˉservice.Build(Nativeˉfileˉinputˉplatform.Windows));
        Sequenceˉequal(
            Linuxˉleaf,
            X64ˉnativeˉfileˉinputˉservice.Build(Nativeˉfileˉinputˉplatform.Linux));
        False(
            typeof(X64ˉnativeˉfileˉinputˉservice).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-X64-File-Input-Services-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime still embeds the file-input generator WVB.");

        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        _ = Nativeˉfragmentˉverifier.Verify(Native);
        Sequenceˉequal(Interpreted, X64ˉnativeˉexecutor.Executeˉbytes(Native));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-input-services-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "File-Input-Services.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Runtime",
                    "Windvale",
                    "Native-X64-File-Input-Services.wvproj"),
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

        Nativeˉfileˉinputˉservicesˉhonorˉdeclaredˉnameˉstride();
    }

    private static void Nativeˉfileˉinputˉservicesˉhonorˉdeclaredˉnameˉstride()
    {
        // With 64 8192-byte name slots, a stale 1 MiB second-slot offset lands
        // exactly here in snapshot zero's data and corrupts its retained tail.
        const int Collisionˉoffset = 512 * 1024;
        const uint Firstˉtail = 67_305_985u;
        const string Source = """
            module Windvaleˉcompilerˉbuildˉdriver profile hosted;

            capability console.write_line;
            capability diagnostic.write_line;
            capability file.read_bytes;
            capability file.write_bytes;
            capability process.argument;
            capability process.argument_count;

            enum Snapshotˉstate { Valid = 0; }

            data Empty: bytes = [];

            fn Failure(Code: u32) -> i32 {
                diagnostic.write_line(
                    Textˉconcat(Enumˉname(Snapshotˉstate.Valid), U32ˉformat(Code))
                );
                return 1;
            }

            export fn Main() -> i32 {
                if process.argument_count() != 3u32 { return Failure(1u32); }
                let First: bytes = file.read_bytes(process.argument(0u32));
                let Second: bytes = file.read_bytes(process.argument(1u32));
                if Bytesˉlength(First) != 524292u32 { return Failure(2u32); }
                if Bytesˉreadˉu32ˉlittle(First, 524288u32) != 67305985u32 {
                    return Failure(3u32);
                }
                if !Textˉutf8ˉisˉvalid(Second) { return Failure(4u32); }
                file.write_bytes(process.argument(2u32), Empty);
                console.write_line(
                    Textˉconcat(
                        Enumˉname(Snapshotˉstate.Valid),
                        Textˉconcat(":", U32ˉformat(Bytesˉlength(Second)))
                    )
                );
                return 0;
            }
            """;

        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(
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
            ],
            Fragment.Requiredˉservices);

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Build(
            Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Build(
            Fragment,
            Nativeˉserviceˉplatform.Linux);
        var Windows = Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉbuildˉdriver(
            Fragment,
            Module.Module.Capabilities,
            Module.Module.Name);
        var Linux = Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉbuildˉdriver(
            Fragment,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        var Verifiedˉwindows = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Buildˉdriver);
        var Verifiedˉlinux = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Buildˉdriver);
        Equal(8_192u, Verifiedˉwindows.Runtime.Layout.Nameˉarenaˉstrideˉbytes);
        Equal(8_192u, Verifiedˉlinux.Runtime.Layout.Nameˉarenaˉstrideˉbytes);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-input-stride-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Firstˉpath = Path.Combine(Directoryˉpath, "first.bin");
            var Secondˉpath = Path.Combine(Directoryˉpath, "second.bin");
            var Outputˉpath = Path.Combine(Directoryˉpath, "output.bin");
            var Firstˉbytes = new byte[Collisionˉoffset + sizeof(uint)];
            Firstˉbytes[Collisionˉoffset] = 1;
            Firstˉbytes[Collisionˉoffset + 1] = 2;
            Firstˉbytes[Collisionˉoffset + 2] = 3;
            Firstˉbytes[Collisionˉoffset + 3] = 4;
            Equal(
                Firstˉtail,
                System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(
                    Firstˉbytes.AsSpan(Collisionˉoffset)));
            File.WriteAllBytes(Firstˉpath, Firstˉbytes);
            File.WriteAllBytes(Secondˉpath, "second"u8.ToArray());

            var Exitˉcode = OperatingSystem.IsWindows()
                ? Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    "Valid:6\n",
                    [Firstˉpath, Secondˉpath, Outputˉpath])
                : Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    "Valid:6\n",
                    [Firstˉpath, Secondˉpath, Outputˉpath]);
            Equal(0, Exitˉcode);
            Equal(0L, new FileInfo(Outputˉpath).Length);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
