using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉcontainerˉmetadataˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Hosted-Container-Metadata-Tool.wv",
                "Native-Hosted-Container-Metadata-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wv",
                    "Native-Hosted-Tool-Metadata-Admission.wv"),
                Source(
                    "Runtime/Windvale/Native-Hosted-Tool-Metadata-Construction-Core.wv",
                    "Native-Hosted-Tool-Metadata-Construction-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_NAME,
            Module.Module.Name);
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

        var Windows = Hostedˉcontainerˉmetadataˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉcontainerˉmetadataˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        var Inputs = new Nativeˉhostedˉtoolˉmetadataˉinputs(
            (uint)Target,
            (uint)Hostedˉcompilerˉapplicationˉprofile.Compiler,
            Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
            0,
            Bundle);
        var Request = Nativeˉhostedˉtoolˉmetadataˉbuilder.Buildˉrequest(Inputs);
        var Expected = Hostedˉcompilerˉapplicationˉmetadata.Buildˉstage0(
            Target,
            Capabilities,
            Bundle,
            Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
            0);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-container-metadata-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Metadata.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "recovery-aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Requestˉpath = Path.Combine(Directoryˉpath, "Request.wvhq");
            var Metadataˉpath = Path.Combine(Directoryˉpath, "Metadata.wvhm");
            File.WriteAllBytes(Requestˉpath, Request.AsSpan());
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉcontainerˉmetadata(
                    Application,
                    Requestˉpath,
                    Metadataˉpath,
                    "hosted container metadata status=Valid bytes=1024\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Metadataˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x4D];
            var Changedˉrequest = Request.ToArray();
            Changedˉrequest[0] ^= 0x01;
            File.WriteAllBytes(Requestˉpath, Changedˉrequest);
            File.WriteAllBytes(Metadataˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉcontainerˉmetadata(
                    Application,
                    Requestˉpath,
                    Metadataˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted container metadata status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Metadataˉpath));

            File.WriteAllBytes(Requestˉpath, Request.AsSpan());
            Equal(
                64,
                Executeˉhostedˉcontainerˉmetadata(
                    Application,
                    Requestˉpath,
                    Requestˉpath,
                    string.Empty,
                    expectedˉerror:
                        "Usage: wvhostmetadata <request.wvhq> <metadata.wvhm>\n"));
            Sequenceˉequal(Request, File.ReadAllBytes(Requestˉpath));

            var Frontˉdoorˉoutput = Path.Combine(Directoryˉpath, "Native-Metadata.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Metadata-Tool.wvproj"),
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

    private static int Executeˉhostedˉcontainerˉmetadata(
        ImmutableArray<byte> application,
        string request,
        string output,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [request, output],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [request, output],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
