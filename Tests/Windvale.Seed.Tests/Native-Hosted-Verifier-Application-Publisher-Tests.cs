using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_APPLICATION_ADMISSION_BYTES = 18_091;
    private const string NATIVE_HOSTED_VERIFIER_APPLICATION_ADMISSION_SHA256 =
        "382f0e23711400d94a843324a34b43347a782a893b1f13d4f417ee20554fad17";

    private static void Nativeˉhostedˉverifierˉapplicationˉpublisherˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-verifier-application-publisher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Admissionˉpath = Path.Combine(Directoryˉpath, "Admission.wvb");
            var Admissionˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Application-Tool.wvproj"),
                Admissionˉpath);
            Equal(0, Admissionˉbuild.Exitˉcode);
            Equal(string.Empty, Admissionˉbuild.Error);
            var Admissionˉbytes = File.ReadAllBytes(Admissionˉpath);
            Equal(
                NATIVE_HOSTED_VERIFIER_APPLICATION_ADMISSION_BYTES,
                Admissionˉbytes.Length);
            Equal(
                NATIVE_HOSTED_VERIFIER_APPLICATION_ADMISSION_SHA256,
                Moduleˉdigest.Calculateˉsha256(Admissionˉbytes));

            var Moduleˉpath = Path.Combine(Directoryˉpath, "Publisher.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Application-Publisher.wvproj"),
                Moduleˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Moduleˉbytes = File.ReadAllBytes(Moduleˉpath);
            Equal(
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .MODULE_BYTES,
                Moduleˉbytes.Length);
            Equal(
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .MODULE_SHA256,
                Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
            var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
            var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;

            var Windowsˉresult =
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉwriter
                    .Writeˉwindows(
                Module,
                Native,
                Moduleˉbytes);
            True(
                Windowsˉresult.Success,
                "The Windows hosted-verifier publisher was rejected: " +
                    string.Join(" | ", Windowsˉresult.Diagnostics));
            var Windows = Windowsˉresult.Imageˉbytes;
            Equal(
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .WINDOWS_APPLICATION_BYTES,
                Windows.Length);
            Equal(
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .WINDOWS_APPLICATION_SHA256,
                Objectˉdigest.Calculateˉsha256(Windows.AsSpan()));

            var Linuxˉresult =
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉwriter
                    .Writeˉlinux(
                Module,
                Native,
                Moduleˉbytes);
            True(
                Linuxˉresult.Success,
                "The Linux hosted-verifier publisher was rejected: " +
                    string.Join(" | ", Linuxˉresult.Diagnostics));
            var Linux = Linuxˉresult.Imageˉbytes;
            Equal(
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .LINUX_APPLICATION_BYTES,
                Linux.Length);
            Equal(
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .LINUX_APPLICATION_SHA256,
                Objectˉdigest.Calculateˉsha256(Linux.AsSpan()));
            Sequenceˉequal(
                Windows,
                File.ReadAllBytes(Path.Combine(
                    Repository,
                    "Artifacts",
                    "Native-Hosted-Verifier-Application-Publisher-Candidate",
                    "windows-x64-wvhostverifierpublish.exe")));
            Sequenceˉequal(
                Linux,
                File.ReadAllBytes(Path.Combine(
                    Repository,
                    "Artifacts",
                    "Native-Hosted-Verifier-Application-Publisher-Candidate",
                    "linux-x64-wvhostverifierpublish.elf")));

            var Extension = OperatingSystem.IsWindows() ? ".exe" : ".elf";
            var Candidateˉpath = Path.Combine(
                Directoryˉpath,
                "Candidate" + Extension);
            var Destinationˉpath = Path.Combine(
                Directoryˉpath,
                "Destination" + Extension);
            var Candidate = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Candidate",
                OperatingSystem.IsWindows()
                    ? "windows-x64-wvverify.exe"
                    : "linux-x64-wvverify.elf"));
            var Publisher = OperatingSystem.IsWindows() ? Windows : Linux;
            File.WriteAllBytes(Candidateˉpath, Candidate);
            File.WriteAllBytes(Destinationˉpath, [1, 2, 3, 4]);
            var Report =
                $"publication status=Complete bytes=0x{Candidate.Length:x8} " +
                $"sha256={Objectˉdigest.Calculateˉsha256(Candidate)}\n";

            if (OperatingSystem.IsWindows())
            {
                var Loadedˉmodules = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
                Equal(
                    0,
                    Executeˉwindowsˉapplication(
                        Publisher,
                        Report,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        loadedˉmodules: Loadedˉmodules));
                Equal(
                    0,
                    Loadedˉmodules.Count(Name =>
                        Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                Loadedˉmodules.Clear();
                Equal(
                    0,
                    Executeˉwindowsˉapplication(
                        File.ReadAllBytes(Destinationˉpath).ToImmutableArray(),
                        "wvb status=Valid profile=compiler-aligned\n",
                        [Path.Combine(
                            Repository,
                            "Artifacts",
                            "Native-Front-Door",
                            "Wvb",
                            "Compiler-Wvb-Verifier.wvb")],
                        timeoutˉmilliseconds: 60_000,
                        loadedˉmodules: Loadedˉmodules));
                Equal(
                    0,
                    Loadedˉmodules.Count(Name =>
                        Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(
                    0,
                    Executeˉlinuxˉapplication(
                        Publisher,
                        Report,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        loadedˉmappings: Loadedˉmappings));
                Equal(
                    0,
                    Loadedˉmappings.Count(Name =>
                        Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                Loadedˉmappings.Clear();
                Equal(
                    0,
                    Executeˉlinuxˉapplication(
                        File.ReadAllBytes(Destinationˉpath).ToImmutableArray(),
                        "wvb status=Valid profile=compiler-aligned\n",
                        [Path.Combine(
                            Repository,
                            "Artifacts",
                            "Native-Front-Door",
                            "Wvb",
                            "Compiler-Wvb-Verifier.wvb")],
                        timeoutˉmilliseconds: 60_000,
                        loadedˉmappings: Loadedˉmappings));
                Equal(
                    0,
                    Loadedˉmappings.Count(Name =>
                        Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            Sequenceˉequal(Candidate, File.ReadAllBytes(Destinationˉpath));
            Equal(
                0,
                Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());

            var Rejected = Candidate.ToArray();
            Rejected[^1] ^= 1;
            byte[] Sentinel = [9, 8, 7, 6];
            File.WriteAllBytes(Candidateˉpath, Rejected);
            File.WriteAllBytes(Destinationˉpath, Sentinel);
            const string Rejection =
                "publication status=Rejected " +
                "phase=native-hosted-verifier-application\n";
            if (OperatingSystem.IsWindows())
            {
                Equal(
                    1,
                    Executeˉwindowsˉapplication(
                        Publisher,
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        expectedˉerror: Rejection));
            }
            else
            {
                Equal(
                    1,
                    Executeˉlinuxˉapplication(
                        Publisher,
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        expectedˉerror: Rejection));
            }
            Sequenceˉequal(Rejected, File.ReadAllBytes(Candidateˉpath));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Destinationˉpath));
            Equal(
                0,
                Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
