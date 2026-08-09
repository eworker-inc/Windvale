using System.Buffers.Binary;
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
    private static void Nativeˉhostedˉcontainerˉsegmentˉmanifestˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));
        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Segment-Manifest-Tool.wv",
                "Native-Hosted-Container-Segment-Manifest-Tool.wv"),
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
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Segment-Set-Core.wv",
                    "Native-Hosted-Container-Segment-Set-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));
        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.MODULE_NAME,
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
        var Windows =
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉwriter.Writeˉwindows(
                Native, Module.Module.Capabilities, Module.Module.Name);
        var Linux =
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉwriter.Writeˉlinux(
                Native, Module.Module.Capabilities, Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
                .WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
                .WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
                .LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
                .LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Runtime = Hostedˉcompilerˉruntimeˉdata.Build(
            Target,
            Hostedˉtoolˉtestˉcapabilities(),
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
        uint Read(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(
            Plan.AsSpan()[offset..]);
        var Requests =
            Nativeˉhostedˉcontainerˉmaterializationˉsession.Buildˉrequests(
                Plan,
                Enumerable.Repeat((byte)0x11, checked((int)Read(36)))
                    .ToImmutableArray(),
                Enumerable.Repeat((byte)0x22, checked((int)Read(44)))
                    .ToImmutableArray(),
                Bundle.Imageˉbytes,
                Enumerable.Repeat((byte)0x33, checked((int)Read(60)))
                    .ToImmutableArray(),
                Runtime,
                Enumerable.Repeat((byte)0x44, checked((int)Read(76)))
                    .ToImmutableArray());
        var Responses = Requests.Select(
            Nativeˉhostedˉcontainerˉsegmentˉconstructor.Execute)
            .ToImmutableArray();
        var Expected = Buildˉhostedˉcontainerˉsegmentˉsetˉmanifest(
            Plan, Requests, Responses);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-segment-manifest-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Manifest.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
                    .WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
                    .LINUX_TARGET_NAME;
            var Cli = Executeˉinspectorˉtool(
                "recovery-aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cli.Exitˉcode);
            Equal(string.Empty, Cli.Standardˉerror);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            Sequenceˉequal(
                Application,
                File.ReadAllBytes(Path.ChangeExtension(
                    Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Planˉpath = Path.Combine(Directoryˉpath, "Plan.wvcd");
            var Prefix = Path.Combine(Directoryˉpath, "Segments");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Segments.wvhm");
            File.WriteAllBytes(Planˉpath, Plan.AsSpan());
            for (var Index = 0; Index < Requests.Length; Index++)
            {
                File.WriteAllBytes(
                    Prefix + $".request-{Index}", Requests[Index].AsSpan());
                File.WriteAllBytes(
                    Prefix + $".response-{Index}", Responses[Index].AsSpan());
            }
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉcontainerˉsegmentˉmanifest(
                    Application,
                    [Planˉpath, Prefix, Outputˉpath],
                    $"hosted container segment manifest status=Valid segments={Requests.Length} bytes={Expected.Length}\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Outputˉpath));
            Equal(0, Loaded.Count(Name =>
                Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x48, 0x4D];
            var Changed = Responses[0].ToArray();
            Changed[28] ^= 0x01;
            File.WriteAllBytes(Prefix + ".response-0", Changed);
            File.WriteAllBytes(Outputˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉcontainerˉsegmentˉmanifest(
                    Application,
                    [Planˉpath, Prefix, Outputˉpath],
                    string.Empty,
                    expectedˉerror:
                        "hosted container segment manifest status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            var Repository = Findˉrepositoryˉroot();
            var Nativeˉoutput = Path.Combine(Directoryˉpath, "Native-Manifest.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Segment-Manifest-Tool.wvproj"),
                Nativeˉoutput);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Nativeˉoutput));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static int Executeˉhostedˉcontainerˉsegmentˉmanifest(
        ImmutableArray<byte> application,
        string[] arguments,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                arguments,
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                arguments,
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
