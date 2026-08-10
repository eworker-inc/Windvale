using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_CONTAINER_SEGMENTER_WVB_BYTES = 24_462;
    private const string NATIVE_HOSTED_CONTAINER_SEGMENTER_WVB_SHA256 =
        "3dab1cbcd584c1130cb0973012d3507127accb1d949c33136935191010a34046";
    private const int WINDOWS_HOSTED_CONTAINER_SEGMENTER_BYTES = 316_416;
    private const string WINDOWS_HOSTED_CONTAINER_SEGMENTER_SHA256 =
        "a3fa46befcd2214927db34c3d18fba96cb3b7ab7a46e47175f6bff4db2865801";
    private const int LINUX_HOSTED_CONTAINER_SEGMENTER_BYTES = 315_392;
    private const string LINUX_HOSTED_CONTAINER_SEGMENTER_SHA256 =
        "7477ded87ef6cb524eeed7ac8593282d10eb2537b5db297b87e060e891955735";

    private static void Nativeˉhostedˉcontainerˉsegmenterˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Segmenter-Tool.wv",
                "Native-Hosted-Container-Segmenter-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv",
                    "Native-Hosted-Container-Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Layout.wv",
                    "Native-Hosted-Container-Layout.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Segmentation-Core.wv",
                    "Native-Hosted-Container-Segmentation-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(NATIVE_HOSTED_CONTAINER_SEGMENTER_WVB_BYTES, Compiled.Moduleˉbytes.Length);
        Equal(
            NATIVE_HOSTED_CONTAINER_SEGMENTER_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal("Nativeˉhostedˉcontainerˉsegmenterˉtool", Module.Module.Name);
        Sequenceˉequal(
            [
                "console.write_line",
                "diagnostic.write_line",
                "file.read_bytes",
                "file.write_bytes",
                "process.argument",
                "process.argument_count",
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
                Nativeˉservice.Fileˉwriteˉbytes,
            ],
            Native.Requiredˉservices);

        var Windows = Hostedˉcontainerˉsegmenterˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows hosted-container segmenter failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        Equal(WINDOWS_HOSTED_CONTAINER_SEGMENTER_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_HOSTED_CONTAINER_SEGMENTER_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉcontainerˉsegmenterˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux hosted-container segmenter failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        Equal(LINUX_HOSTED_CONTAINER_SEGMENTER_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_HOSTED_CONTAINER_SEGMENTER_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        var Runtime = Hostedˉcompilerˉruntimeˉdata.Build(
            Target,
            Capabilities,
            Bundle,
            0,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);
        var Plan = Nativeˉhostedˉcontainerˉconstructor.Execute(
            Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
                Target,
                Hostedˉcompilerˉapplicationˉprofile.Compiler,
                Bundle,
                0,
                Runtime));
        uint Read(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.AsSpan()[offset..]);
        var Header = Enumerable.Repeat((byte)0x11, checked((int)Read(36)))
            .ToImmutableArray();
        var Startup = Enumerable.Repeat((byte)0x22, checked((int)Read(44)))
            .ToImmutableArray();
        var Imports = Enumerable.Repeat((byte)0x33, checked((int)Read(60)))
            .ToImmutableArray();
        var Relocation = Enumerable.Repeat((byte)0x44, checked((int)Read(76)))
            .ToImmutableArray();
        var Request = Nativeˉhostedˉcontainerˉmaterializationˉsession.Buildˉrequests(
            Plan,
            Header,
            Startup,
            Bundle.Imageˉbytes,
            Imports,
            Runtime,
            Relocation)[0];
        var Expected = Nativeˉhostedˉcontainerˉsegmentˉconstructor.Execute(Request);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-container-segmenter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Requestˉpath = Path.Combine(Directoryˉpath, "Request.wvht");
            var Responseˉpath = Path.Combine(Directoryˉpath, "Response.wvhu");
            File.WriteAllBytes(Requestˉpath, Request.AsSpan());
            var Image = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Expectedˉline = $"hosted container segment status=Valid bytes={Expected.Length}\n";
            var Exitˉcode = OperatingSystem.IsWindows()
                ? Executeˉwindowsˉapplication(
                    Image,
                    Expectedˉline,
                    [Requestˉpath, Responseˉpath])
                : Executeˉlinuxˉapplication(
                    Image,
                    Expectedˉline,
                    [Requestˉpath, Responseˉpath]);
            Equal(0, Exitˉcode);
            Sequenceˉequal(Expected, File.ReadAllBytes(Responseˉpath));

            byte[] Sentinel = [0x51, 0x52, 0x53];
            File.WriteAllBytes(Requestˉpath, Request.AsSpan()[..159]);
            File.WriteAllBytes(Responseˉpath, Sentinel);
            Exitˉcode = OperatingSystem.IsWindows()
                ? Executeˉwindowsˉapplication(
                    Image,
                    string.Empty,
                    [Requestˉpath, Responseˉpath],
                    expectedˉerror: "hosted container segment status=Rejected\n")
                : Executeˉlinuxˉapplication(
                    Image,
                    string.Empty,
                    [Requestˉpath, Responseˉpath],
                    expectedˉerror: "hosted container segment status=Rejected\n");
            Equal(2, Exitˉcode);
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Responseˉpath));

            var Frontˉdoorˉoutput = Path.Combine(Directoryˉpath, "Segmenter.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Segmenter-Tool.wvproj"),
                Frontˉdoorˉoutput);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Frontˉdoorˉoutput));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
