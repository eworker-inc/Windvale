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
    private static void Nativeˉhostedˉpublicationˉrequestˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Hosted-Publication-Request-Tool.wv",
                "Native-Hosted-Publication-Request-Tool.wv"),
            [
                Source(
                    "Compiler/Windvale/Native-Publication-Core.wv",
                    "Native-Publication-Core.wv"),
                Source(
                    "Foundation/Immutable-Source-Regions.wv",
                    "Immutable-Source-Regions.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.MODULE_NAME,
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

        var Windows = Hostedˉpublicationˉrequestˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉpublicationˉrequestˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉpublicationˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Fragment = Bundle.Imageˉbytes[..Bundle.Nativeˉimageˉbytes];
        var Serviceˉcode = Bundle.Placements.Select(Placement =>
            new Nativeˉserviceˉcode(
                Placement.Service,
                Placement.Adapter,
                Bundle.Imageˉbytes[
                    Placement.Imageˉoffset..
                    (Placement.Imageˉoffset + Placement.Codeˉbytes)]))
            .ToImmutableArray();
        var Services = Serviceˉcode.Select(Service =>
            new Nativeˉpublicationˉservice(Service.Service, Service.Code.Length))
            .ToImmutableArray();
        var Plan = X64ˉnativeˉpublicationˉlayout.Plan(Fragment.Length, Services);
        var Expected = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
            Fragment.Length,
            Services);
        var Sources = Buildˉserviceˉbundleˉsourceˉmanifest(
            Fragment,
            Serviceˉcode,
            Plan);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-publication-request-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Publication-Request.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉpublicationˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉpublicationˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME;
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

            var Manifestˉpath = Path.Combine(Directoryˉpath, "Sources.wvsg");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Request.wvpq");
            File.WriteAllBytes(Manifestˉpath, Sources.Manifest);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉpublicationˉrequest(
                    Application,
                    Manifestˉpath,
                    Outputˉpath,
                    "hosted publication request status=Valid bytes=144\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Outputˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x50, 0x51];
            var Changedˉmanifest = Sources.Manifest.ToArray();
            Changedˉmanifest[0] ^= 0x01;
            File.WriteAllBytes(Manifestˉpath, Changedˉmanifest);
            File.WriteAllBytes(Outputˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉpublicationˉrequest(
                    Application,
                    Manifestˉpath,
                    Outputˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted publication request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            Changedˉmanifest = Sources.Manifest.ToArray();
            Changedˉmanifest[96] ^= 0x01;
            File.WriteAllBytes(Manifestˉpath, Changedˉmanifest);
            Equal(
                2,
                Executeˉhostedˉpublicationˉrequest(
                    Application,
                    Manifestˉpath,
                    Outputˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted publication request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            File.WriteAllBytes(Manifestˉpath, Sources.Manifest);
            Equal(
                2,
                Executeˉhostedˉpublicationˉrequest(
                    Application,
                    Manifestˉpath,
                    Manifestˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted publication request status=Rejected\n"));
            Sequenceˉequal(Sources.Manifest, File.ReadAllBytes(Manifestˉpath));

            var Frontˉdoorˉoutput = Path.Combine(
                Directoryˉpath,
                "Native-Publication-Request.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Publication-Request-Tool.wvproj"),
                Frontˉdoorˉoutput);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(
                Compiled.Moduleˉbytes,
                File.ReadAllBytes(Frontˉdoorˉoutput));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static int Executeˉhostedˉpublicationˉrequest(
        ImmutableArray<byte> application,
        string manifest,
        string output,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [manifest, output],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [manifest, output],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
