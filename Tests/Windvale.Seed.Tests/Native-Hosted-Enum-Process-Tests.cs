using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉenumˉprocessesˉrun()
    {
        var Directoryˉpath = Path.Combine(Path.GetTempPath(),
            $"windvale-hosted-enum-process-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Repository = Findˉrepositoryˉroot();
            var Requestˉmoduleˉpath = Path.Combine(Directoryˉpath,
                "Enum-Request.wvb");
            var Serviceˉmoduleˉpath = Path.Combine(Directoryˉpath,
                "Enum-Service.wvb");
            var Requestˉbytes = Buildˉnativeˉenumˉmodule(
                Repository,
                "Windvale-Native-Hosted-Enum-Request-Tool.wvproj",
                Requestˉmoduleˉpath,
                Hostedˉenumˉrequestˉapplicationˉcontract.MODULE_BYTES,
                Hostedˉenumˉrequestˉapplicationˉcontract.MODULE_SHA256);
            var Serviceˉbytes = Buildˉnativeˉenumˉmodule(
                Repository,
                "Windvale-Native-Hosted-Enum-Service-Tool.wvproj",
                Serviceˉmoduleˉpath,
                Hostedˉenumˉserviceˉapplicationˉcontract.MODULE_BYTES,
                Hostedˉenumˉserviceˉapplicationˉcontract.MODULE_SHA256);

            var Requestˉmodule = Moduleˉcodec.Readˉandˉverify(
                Requestˉbytes.AsSpan());
            var Serviceˉmodule = Moduleˉcodec.Readˉandˉverify(
                Serviceˉbytes.AsSpan());
            var Requestˉfragment = X64ˉnativeˉbackend.Compile(
                Requestˉmodule).Fragment;
            var Serviceˉfragment = X64ˉnativeˉbackend.Compile(
                Serviceˉmodule).Fragment;
            var Serviceˉobjectˉpath = Path.Combine(Directoryˉpath,
                "Enum-Service.wvo");
            var Expectedˉserviceˉobject = Nativeˉobjectˉsink.Writeˉwvo(
                Serviceˉfragment);
            var Loweredˉservice = Runˉnativeˉwvbˉtool(
                Repository,
                "Lower-Wvb-To-Wvo",
                Serviceˉmoduleˉpath,
                Serviceˉobjectˉpath);
            Equal(0, Loweredˉservice.Exitˉcode);
            Equal(
                "native x64 status=Valid abi=22 code-bytes=166864 " +
                    "object-bytes=168342\n",
                Loweredˉservice.Output);
            Equal(string.Empty, Loweredˉservice.Error);
            Sequenceˉequal(
                Expectedˉserviceˉobject,
                File.ReadAllBytes(Serviceˉobjectˉpath));

            var Serviceˉfragmentˉpath = Path.Combine(Directoryˉpath,
                "Enum-Service.bin");
            var Linkedˉservice = Runˉnativeˉwvbˉtool(
                Repository,
                "Link-Wvo",
                "0",
                "Main",
                Serviceˉfragmentˉpath,
                Serviceˉobjectˉpath);
            Equal(0, Linkedˉservice.Exitˉcode);
            Contains(Linkedˉservice.Output,
                "base-address=0 image-bytes=167274");
            Contains(Linkedˉservice.Output,
                "image sha256=cec5c423e32a3c0bc5602551e2b1da2e82929b2edd84b2756c4062bf0f223870");
            Equal(string.Empty, Linkedˉservice.Error);
            Sequenceˉequal(
                Serviceˉfragment.Code,
                File.ReadAllBytes(Serviceˉfragmentˉpath));
            var Requestˉwindows =
                Hostedˉenumˉrequestˉapplicationˉwriter.Writeˉwindows(
                    Requestˉfragment,
                    Requestˉmodule.Module.Capabilities,
                    Requestˉmodule.Module.Name);
            var Requestˉlinux =
                Hostedˉenumˉrequestˉapplicationˉwriter.Writeˉlinux(
                    Requestˉfragment,
                    Requestˉmodule.Module.Capabilities,
                    Requestˉmodule.Module.Name);
            var Serviceˉwindows =
                Hostedˉenumˉserviceˉapplicationˉwriter.Writeˉwindows(
                    Serviceˉfragment,
                    Serviceˉmodule.Module.Capabilities,
                    Serviceˉmodule.Module.Name);
            var Serviceˉlinux =
                Hostedˉenumˉserviceˉapplicationˉwriter.Writeˉlinux(
                    Serviceˉfragment,
                    Serviceˉmodule.Module.Capabilities,
                    Serviceˉmodule.Module.Name);
            True(Requestˉwindows.Success,
                string.Join(" | ", Requestˉwindows.Diagnostics));
            True(Requestˉlinux.Success,
                string.Join(" | ", Requestˉlinux.Diagnostics));
            True(Serviceˉwindows.Success,
                string.Join(" | ", Serviceˉwindows.Diagnostics));
            True(Serviceˉlinux.Success,
                string.Join(" | ", Serviceˉlinux.Diagnostics));
            Requireˉenumˉapplicationˉidentity(
                Requestˉwindows.Imageˉbytes,
                Hostedˉenumˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Hostedˉenumˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256);
            Requireˉenumˉapplicationˉidentity(
                Requestˉlinux.Imageˉbytes,
                Hostedˉenumˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Hostedˉenumˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256);
            Requireˉenumˉapplicationˉidentity(
                Serviceˉwindows.Imageˉbytes,
                Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256);
            Requireˉenumˉapplicationˉidentity(
                Serviceˉlinux.Imageˉbytes,
                Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_APPLICATION_SHA256);
            var Expectedˉrequests = Nativeˉenumˉmetadataˉbuilder.Buildˉrequests(
                Requestˉmodule.Module.Types);
            Equal(1, Expectedˉrequests.Length);
            var Expectedˉservice = X64ˉnativeˉtextˉservices.Build(
                Nativeˉservice.Enumˉname,
                Requestˉmodule.Module.Types);

            Requireˉenumˉcliˉtarget(
                Requestˉmoduleˉpath,
                OperatingSystem.IsWindows()
                    ? Hostedˉenumˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME
                    : Hostedˉenumˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME,
                OperatingSystem.IsWindows()
                    ? Requestˉwindows.Imageˉbytes
                    : Requestˉlinux.Imageˉbytes);
            Requireˉenumˉcliˉtarget(
                Serviceˉmoduleˉpath,
                OperatingSystem.IsWindows()
                    ? Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_TARGET_NAME
                    : Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_TARGET_NAME,
                OperatingSystem.IsWindows()
                    ? Serviceˉwindows.Imageˉbytes
                    : Serviceˉlinux.Imageˉbytes);

            var Requestˉpath = Path.Combine(Directoryˉpath, "Request.wveq");
            var Serviceˉpath = Path.Combine(Directoryˉpath, "Service.bin");
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var Requestˉapplication = OperatingSystem.IsWindows()
                ? Requestˉwindows.Imageˉbytes : Requestˉlinux.Imageˉbytes;
            var Serviceˉapplication = OperatingSystem.IsWindows()
                ? Serviceˉwindows.Imageˉbytes : Serviceˉlinux.Imageˉbytes;
            Equal(0, Executeˉhostedˉenumˉapplication(
                Requestˉapplication,
                [Requestˉmoduleˉpath, Requestˉpath],
                $"hosted enum request status=Valid bytes={Expectedˉrequests[0].Length}\n",
                Loaded));
            Sequenceˉequal(Expectedˉrequests[0], File.ReadAllBytes(Requestˉpath));
            Equal(0, Executeˉhostedˉenumˉapplication(
                Serviceˉapplication,
                [Requestˉpath, Serviceˉpath],
                $"hosted enum service status=Valid bytes={Expectedˉservice.Length}\n",
                Loaded));
            Sequenceˉequal(Expectedˉservice, File.ReadAllBytes(Serviceˉpath));
            Equal(0, Loaded.Count(Name => Name.Contains("clr",
                StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x45, 0x4E];
            var Badˉmodule = Path.Combine(Directoryˉpath, "Bad.wvb");
            File.WriteAllBytes(Badˉmodule, Requestˉbytes.AsSpan()[..^1]);
            File.WriteAllBytes(Requestˉpath, Sentinel);
            Equal(2, Executeˉhostedˉenumˉapplication(
                Requestˉapplication,
                [Badˉmodule, Requestˉpath],
                string.Empty,
                expectedˉerror: "hosted enum request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Requestˉpath));

            var Badˉrequest = Expectedˉrequests[0].ToArray();
            Badˉrequest[0] ^= 0x01;
            File.WriteAllBytes(Requestˉpath, Badˉrequest);
            File.WriteAllBytes(Serviceˉpath, Sentinel);
            Equal(2, Executeˉhostedˉenumˉapplication(
                Serviceˉapplication,
                [Requestˉpath, Serviceˉpath],
                string.Empty,
                expectedˉerror: "hosted enum service status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Serviceˉpath));

            Equal(2, Executeˉhostedˉenumˉapplication(
                Requestˉapplication,
                [Requestˉmoduleˉpath, Requestˉmoduleˉpath],
                string.Empty,
                expectedˉerror: "hosted enum request status=Rejected\n"));
            Sequenceˉequal(Requestˉbytes, File.ReadAllBytes(Requestˉmoduleˉpath));
            File.WriteAllBytes(Requestˉpath, Expectedˉrequests[0].AsSpan());
            Equal(2, Executeˉhostedˉenumˉapplication(
                Serviceˉapplication,
                [Requestˉpath, Requestˉpath],
                string.Empty,
                expectedˉerror: "hosted enum service status=Rejected\n"));
            Sequenceˉequal(Expectedˉrequests[0], File.ReadAllBytes(Requestˉpath));
        }
        finally { Directory.Delete(Directoryˉpath, recursive: true); }
    }

    private static ImmutableArray<byte> Buildˉnativeˉenumˉmodule(
        string repository,
        string project,
        string output,
        int expectedˉbytes,
        string expectedˉsha256)
    {
        var Build = Runˉnativeˉfrontˉdoor(
            repository,
            Path.Combine(repository, project),
            output);
        Equal(0, Build.Exitˉcode);
        Equal(string.Empty, Build.Error);
        var Bytes = File.ReadAllBytes(output).ToImmutableArray();
        Equal(expectedˉbytes, Bytes.Length);
        Equal(expectedˉsha256,
            Moduleˉdigest.Calculateˉsha256(Bytes.AsSpan()));
        return Bytes;
    }

    private static void Requireˉenumˉapplicationˉidentity(
        ImmutableArray<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256)
    {
        Equal(expectedˉbytes, bytes.Length);
        Equal(expectedˉsha256, Objectˉdigest.Calculateˉsha256(bytes.AsSpan()));
    }

    private static void Requireˉenumˉcliˉtarget(
        string moduleˉpath,
        string target,
        ImmutableArray<byte> expected)
    {
        var Cli = Executeˉinspectorˉtool("aot", moduleˉpath, "--target", target);
        Equal(0, Cli.Exitˉcode);
        Equal(string.Empty, Cli.Standardˉerror);
        Sequenceˉequal(expected, File.ReadAllBytes(Path.ChangeExtension(
            moduleˉpath, Windvale.Tool.Program.Targetˉoutputˉextension(target))));
    }

    private static int Executeˉhostedˉenumˉapplication(
        ImmutableArray<byte> application,
        string[] arguments,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") => OperatingSystem.IsWindows()
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
