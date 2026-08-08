using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉcontainerˉsourceˉsetˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));
        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Source-Set-Tool.wv",
                "Native-Hosted-Container-Source-Set-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Foundation/Immutable-Source-Regions.wv",
                    "Immutable-Source-Regions.wv"),
                Source(
                    "Foundation/Sha256-Compression.wv",
                    "Sha256-Compression.wv"),
                Source(
                    "Foundation/Sha256-Streaming.wv",
                    "Sha256-Streaming.wv"),
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
                    "Linker/Windvale/Native-Hosted-Container-Source-Set-Core.wv",
                    "Native-Hosted-Container-Source-Set-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));
        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.MODULE_NAME,
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
        var Windows = Hostedˉcontainerˉsourceˉsetˉapplicationˉwriter.Writeˉwindows(
            Native, Module.Module.Capabilities, Module.Module.Name);
        var Linux = Hostedˉcontainerˉsourceˉsetˉapplicationˉwriter.Writeˉlinux(
            Native, Module.Module.Capabilities, Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Fixtureˉbundle = Hostedˉtoolˉtestˉbundle(Target);
        var Fragment =
            Fixtureˉbundle.Imageˉbytes[..Fixtureˉbundle.Nativeˉimageˉbytes];
        var Services = Fixtureˉbundle.Placements.Select(Placement =>
            new Nativeˉserviceˉcode(
                Placement.Service,
                Placement.Adapter,
                Fixtureˉbundle.Imageˉbytes[
                    Placement.Imageˉoffset..
                    (Placement.Imageˉoffset + Placement.Codeˉbytes)]))
            .ToImmutableArray();
        var Publicationˉplan = X64ˉnativeˉpublicationˉlayout.Plan(
            Fragment.Length,
            Services.Select(Service => new Nativeˉpublicationˉservice(
                Service.Service, Service.Code.Length)).ToImmutableArray());
        var Bundleˉimage = X64ˉnativeˉserviceˉbundleˉmaterialization.Materialize(
            Fragment, Services, Publicationˉplan);
        var Placements = Publicationˉplan.Placements.Select((Placement, Index) =>
            new Nativeˉserviceˉbundleˉplacement(
                Placement.Service,
                Services[Index].Adapter,
                Fixtureˉbundle.Placements[Index].Serviceˉtableˉoffset,
                Placement.Offset,
                Placement.Size,
                Objectˉdigest.Calculateˉsha256(Services[Index].Code.AsSpan())))
            .ToImmutableArray();
        var Bundle = new Nativeˉserviceˉbundle(
            Fixtureˉbundle.Platform,
            Fragment.Length,
            Bundleˉimage,
            Placements);
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
        uint Readˉplan(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(
            Plan.AsSpan()[offset..]);
        var Platform = Nativeˉhostedˉcontainerˉbytesˉconstructor.Execute(
            Target, Plan);
        var Startup = Buildˉhostedˉstartupˉresponse(Target, Plan);
        var Bundleˉresponses =
            Nativeˉserviceˉbundleˉmaterializationˉsession.Buildˉrequests(
                Fragment, Services, Publicationˉplan)
            .Select(Request =>
                X64ˉnativeˉserviceˉbundleˉmaterialization.Buildˉwithˉwindvale(
                    Request))
            .ToImmutableArray();
        Equal((uint)Bundle.Imageˉbytes.Length, Readˉplan(52));
        Sequenceˉequal(
            Bundle.Imageˉbytes,
            Bundleˉresponses.SelectMany(Response => Response.AsSpan()[40..].ToArray())
                .ToArray());
        Sequenceˉequal(
            SHA256.HashData(Fragment.AsSpan()), Runtime.AsSpan(576, 32).ToArray());
        for (var Index = 0; Index < Services.Length; Index++)
        {
            Sequenceˉequal(
                SHA256.HashData(Services[Index].Code.AsSpan()),
                Runtime.AsSpan(736 + Index * 64, 32).ToArray());
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(), $"windvale-hosted-source-set-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Source-Set.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cli = Executeˉinspectorˉtool(
                "aot", Moduleˉpath, "--target", Cliˉtarget);
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
            var Platformˉpath = Path.Combine(Directoryˉpath, "Platform.wvhb");
            var Startupˉpath = Path.Combine(Directoryˉpath, "Startup.wvsd");
            var Bundleˉprefix = Path.Combine(Directoryˉpath, "Bundle");
            var Runtimeˉpath = Path.Combine(Directoryˉpath, "Runtime.wvhr");
            var Chunkˉprefix = Path.Combine(Directoryˉpath, "Sources");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "Sources.wvsg");
            File.WriteAllBytes(Planˉpath, Plan.AsSpan());
            File.WriteAllBytes(Platformˉpath, Platform.AsSpan());
            File.WriteAllBytes(Startupˉpath, Startup.AsSpan());
            File.WriteAllBytes(Runtimeˉpath, Runtime.AsSpan());
            for (var Index = 0; Index < Bundleˉresponses.Length; Index++)
            {
                File.WriteAllBytes(
                    Bundleˉprefix + $".response-{Index}",
                    Bundleˉresponses[Index].AsSpan());
            }
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var Expectedˉchunks = Bundleˉresponses.Length +
                (Target == Consoleˉapplicationˉtarget.Windowsˉx64 ? 5 : 3);
            var Expectedˉmanifestˉbytes = 32 + Expectedˉchunks * 20 + 6 * 16;
            Equal(
                0,
                Executeˉhostedˉcontainerˉsourceˉset(
                    Application,
                    [
                        Planˉpath,
                        Platformˉpath,
                        Startupˉpath,
                        Bundleˉprefix,
                        Runtimeˉpath,
                        Chunkˉprefix,
                        Manifestˉpath,
                    ],
                    $"hosted container source set status=Valid chunks={Expectedˉchunks} bytes={Expectedˉmanifestˉbytes}\n",
                    Loaded));
            Equal(0, Loaded.Count(Name =>
                Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            var Expectedˉresources = ImmutableArray.CreateBuilder<byte[]>();
            Expectedˉresources.Add(
                Platform.AsSpan(32, checked((int)Readˉplan(36))).ToArray());
            Expectedˉresources.Add(
                Startup.AsSpan(32, checked((int)Readˉplan(44))).ToArray());
            foreach (var Response in Bundleˉresponses)
            {
                Expectedˉresources.Add(Response.AsSpan()[40..].ToArray());
            }
            if (Readˉplan(60) > 0)
            {
                Expectedˉresources.Add(Platform.AsSpan(
                    checked(32 + (int)Readˉplan(36)),
                    checked((int)Readˉplan(60))).ToArray());
            }
            Expectedˉresources.Add(Runtime.ToArray());
            if (Readˉplan(76) > 0)
            {
                Expectedˉresources.Add(Platform.AsSpan(
                    checked(32 + (int)Readˉplan(36) + (int)Readˉplan(60)),
                    checked((int)Readˉplan(76))).ToArray());
            }
            Equal(Expectedˉchunks, Expectedˉresources.Count);
            for (var Index = 0; Index < Expectedˉresources.Count; Index++)
            {
                Sequenceˉequal(
                    Expectedˉresources[Index],
                    File.ReadAllBytes(Chunkˉprefix + $".chunk-{Index}"));
            }
            Verifyˉhostedˉcontainerˉsourceˉmanifest(
                File.ReadAllBytes(Manifestˉpath), Plan, Expectedˉresources);

            byte[] Sentinel = [0x57, 0x56, 0x53, 0x47];
            var Changed = Bundleˉresponses[0].ToArray();
            Changed[40] ^= 0x01;
            File.WriteAllBytes(Bundleˉprefix + ".response-0", Changed);
            File.WriteAllBytes(Manifestˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉcontainerˉsourceˉset(
                    Application,
                    [
                        Planˉpath,
                        Platformˉpath,
                        Startupˉpath,
                        Bundleˉprefix,
                        Runtimeˉpath,
                        Chunkˉprefix,
                        Manifestˉpath,
                    ],
                    string.Empty,
                    expectedˉerror:
                        "hosted container source set status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Manifestˉpath));
            Sequenceˉequal(
                Expectedˉresources[2],
                File.ReadAllBytes(Chunkˉprefix + ".chunk-2"));

            File.WriteAllBytes(
                Bundleˉprefix + ".response-0", Bundleˉresponses[0].AsSpan());
            Equal(
                2,
                Executeˉhostedˉcontainerˉsourceˉset(
                    Application,
                    [
                        Planˉpath,
                        Platformˉpath,
                        Startupˉpath,
                        Bundleˉprefix,
                        Runtimeˉpath,
                        Chunkˉprefix,
                        Planˉpath,
                    ],
                    string.Empty,
                    expectedˉerror:
                        "hosted container source set status=Rejected\n"));
            Sequenceˉequal(Plan, File.ReadAllBytes(Planˉpath));

            var Repository = Findˉrepositoryˉroot();
            var Nativeˉoutput = Path.Combine(Directoryˉpath, "Native-Source-Set.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Source-Set-Tool.wvproj"),
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
}
