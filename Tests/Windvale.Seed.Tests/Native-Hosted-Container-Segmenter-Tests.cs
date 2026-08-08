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
    private const int NATIVE_HOSTED_CONTAINER_SEGMENTER_WVB_BYTES = 23_896;
    private const string NATIVE_HOSTED_CONTAINER_SEGMENTER_WVB_SHA256 =
        "5fed7bad6fca036297b97b3a5ec2eea999e52682b2055abe278592b46b3b0d3f";
    private const int WINDOWS_HOSTED_CONTAINER_SEGMENTER_BYTES = 312_320;
    private const string WINDOWS_HOSTED_CONTAINER_SEGMENTER_SHA256 =
        "931b0bff9138f5ce7ef0142f7bb29e84d72055802325f9fe5fe3d217548ef4ad";
    private const int LINUX_HOSTED_CONTAINER_SEGMENTER_BYTES = 311_296;
    private const string LINUX_HOSTED_CONTAINER_SEGMENTER_SHA256 =
        "d33d0a0f4aacf8dc0276451aeaf0794a5723f1c57cc435a9b8751bab3021b4e6";

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
