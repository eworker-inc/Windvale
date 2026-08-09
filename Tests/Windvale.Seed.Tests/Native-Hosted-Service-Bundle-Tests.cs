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
    private static void Nativeˉhostedˉserviceˉbundleˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Hosted-Service-Bundle-Tool.wv",
                "Native-Hosted-Service-Bundle-Tool.wv"),
            [
                Source(
                    "Compiler/Windvale/Native-Publication-Core.wv",
                    "Native-Publication-Core.wv"),
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv",
                    "Native-Service-Bundle-Materialization-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.MODULE_NAME,
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

        var Windows = Hostedˉserviceˉbundleˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉserviceˉbundleˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉserviceˉbundleˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Fragment = Bundle.Imageˉbytes[..Bundle.Nativeˉimageˉbytes];
        var Services = Bundle.Placements.Select(Placement =>
            new Nativeˉserviceˉcode(
                Placement.Service,
                Placement.Adapter,
                Bundle.Imageˉbytes[
                    Placement.Imageˉoffset..
                    (Placement.Imageˉoffset + Placement.Codeˉbytes)]))
            .ToImmutableArray();
        var Plan = X64ˉnativeˉpublicationˉlayout.Plan(
            Fragment.Length,
            Services.Select(Service => new Nativeˉpublicationˉservice(
                Service.Service,
                Service.Code.Length)).ToImmutableArray());
        var Request = Nativeˉserviceˉbundleˉmaterializationˉsession.Buildˉrequest(
            Fragment,
            Services,
            Plan,
            0);
        var Expected = X64ˉnativeˉserviceˉbundleˉmaterialization.Buildˉwithˉwindvale(
            Request);
        var Expectedˉimage = X64ˉnativeˉserviceˉbundleˉmaterialization.Materialize(
            Fragment,
            Services,
            Plan);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-service-bundle-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Service-Bundle.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉserviceˉbundleˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉserviceˉbundleˉapplicationˉcontract.LINUX_TARGET_NAME;
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

            var Requestˉpath = Path.Combine(Directoryˉpath, "Request.wvsq");
            var Responseˉpath = Path.Combine(Directoryˉpath, "Response.wvsi");
            File.WriteAllBytes(Requestˉpath, Request.AsSpan());
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉserviceˉbundle(
                    Application,
                    Requestˉpath,
                    Responseˉpath,
                    $"hosted service bundle status=Valid bytes={Expected.Length}\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Responseˉpath));
            var Image = Nativeˉserviceˉbundleˉmaterializationˉsession.Verifyˉresponse(
                Fragment,
                Services,
                Plan,
                0,
                Request.Length,
                Expected);
            Sequenceˉequal(Expectedˉimage, Image);
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x53];
            var Changedˉrequest = Request.ToArray();
            Changedˉrequest[0] ^= 0x01;
            File.WriteAllBytes(Requestˉpath, Changedˉrequest);
            File.WriteAllBytes(Responseˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉserviceˉbundle(
                    Application,
                    Requestˉpath,
                    Responseˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted service bundle status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Responseˉpath));

            File.WriteAllBytes(Requestˉpath, Request.AsSpan());
            Equal(
                64,
                Executeˉhostedˉserviceˉbundle(
                    Application,
                    Requestˉpath,
                    Requestˉpath,
                    string.Empty,
                    expectedˉerror:
                        "Usage: wvhostbundle <request.wvsq> <response.wvsi>\n"));
            Sequenceˉequal(Request, File.ReadAllBytes(Requestˉpath));

            var Frontˉdoorˉoutput = Path.Combine(Directoryˉpath, "Native-Bundle.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Service-Bundle-Tool.wvproj"),
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

    private static int Executeˉhostedˉserviceˉbundle(
        ImmutableArray<byte> application,
        string request,
        string response,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [request, response],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [request, response],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
