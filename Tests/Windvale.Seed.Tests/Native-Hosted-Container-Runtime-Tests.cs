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
    private static void Nativeˉhostedˉcontainerˉruntimeˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Hosted-Container-Runtime-Tool.wv",
                "Native-Hosted-Container-Runtime-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wv",
                    "Native-Hosted-Tool-Metadata-Admission.wv"),
                Source(
                    "Runtime/Windvale/Native-Hosted-Tool-Runtime-Header-Core.wv",
                    "Native-Hosted-Tool-Runtime-Header-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_NAME,
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

        var Windows = Hostedˉcontainerˉruntimeˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉcontainerˉruntimeˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Metadata = Nativeˉhostedˉtoolˉmetadataˉbuilder.Build(
            new Nativeˉhostedˉtoolˉmetadataˉinputs(
                (uint)Target,
                (uint)Hostedˉcompilerˉapplicationˉprofile.Compiler,
                Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
                0,
                Bundle));
        var Inputs = new Nativeˉhostedˉtoolˉruntimeˉheaderˉinputs(
            (uint)Target,
            (uint)Hostedˉcompilerˉapplicationˉprofile.Compiler,
            Metadata);
        var Expected = Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.Build(Inputs);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-container-runtime-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Runtime.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_TARGET_NAME;
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

            var Metadataˉpath = Path.Combine(Directoryˉpath, "Metadata.wvhm");
            var Runtimeˉpath = Path.Combine(Directoryˉpath, "Runtime.wvhr");
            File.WriteAllBytes(Metadataˉpath, Metadata.AsSpan());
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉcontainerˉruntime(
                    Application,
                    Metadataˉpath,
                    Runtimeˉpath,
                    "hosted container runtime status=Valid bytes=4096\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Runtimeˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x52];
            var Changedˉmetadata = Metadata.ToArray();
            Changedˉmetadata[0] ^= 0x01;
            File.WriteAllBytes(Metadataˉpath, Changedˉmetadata);
            File.WriteAllBytes(Runtimeˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉcontainerˉruntime(
                    Application,
                    Metadataˉpath,
                    Runtimeˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted container runtime status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Runtimeˉpath));

            File.WriteAllBytes(Metadataˉpath, Metadata.AsSpan());
            Equal(
                64,
                Executeˉhostedˉcontainerˉruntime(
                    Application,
                    Metadataˉpath,
                    Metadataˉpath,
                    string.Empty,
                    expectedˉerror:
                        "Usage: wvhostruntime <metadata.wvhm> <runtime.wvhr>\n"));
            Sequenceˉequal(Metadata, File.ReadAllBytes(Metadataˉpath));

            var Frontˉdoorˉoutput = Path.Combine(Directoryˉpath, "Native-Runtime.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Runtime-Tool.wvproj"),
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

    private static int Executeˉhostedˉcontainerˉruntime(
        ImmutableArray<byte> application,
        string metadata,
        string output,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [metadata, output],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [metadata, output],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
