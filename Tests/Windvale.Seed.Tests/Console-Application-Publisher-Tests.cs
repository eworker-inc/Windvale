using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string CONSOLE_APPLICATION_PUBLISHER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Console-Application-Publisher.wv");

    private static void Consoleˉapplicationˉpublisherˉruns()
    {
        var Compilation = Seedˉcompiler.Compileˉmodules(
            new(
                "Console-Application-Publisher.wv",
                CONSOLE_APPLICATION_PUBLISHER_SOURCE),
            [
                new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE),
                new(
                    "Console-Application-Plan-Core.wv",
                    CONSOLE_APPLICATION_PLAN_CORE_SOURCE),
                new(
                    "Console-Application-Construction-Core.wv",
                    CONSOLE_APPLICATION_CONSTRUCTION_CORE_SOURCE),
                new(
                    "Console-Application-Verification-Core.wv",
                    CONSOLE_APPLICATION_VERIFICATION_CORE_SOURCE),
                new(
                    "Wvb-Publication-Transaction.wv",
                    WVB_PUBLICATION_TRANSACTION_SOURCE),
                new(
                    "Wvb-Publication-Native-Bridge.wv",
                    WVB_PUBLICATION_NATIVE_BRIDGE_SOURCE),
            ]);
        True(
            Compilation.Success,
            "The console-application publisher did not compile: " +
                string.Join(" | ", Compilation.Diagnostics));
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract.MODULE_BYTES,
            Compilation.Moduleˉbytes.Length);
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compilation.Moduleˉbytes.AsSpan()));

        var Verified = Moduleˉcodec.Readˉandˉverify(
            Compilation.Moduleˉbytes.AsSpan());
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract.MODULE_NAME,
            Verified.Module.Name);
        Equal(Moduleˉprofile.Hosted, Verified.Module.Profile);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Verified.Module.Capabilities.Select(Item => Item.Name));

        var Native = X64ˉnativeˉbackend.Compile(Verified);
        Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        var Windowsˉpublisherˉresult =
            Consoleˉapplicationˉpublisherˉapplicationˉwriter.Writeˉwindows(
                Verified,
                Native.Fragment,
                Compilation.Moduleˉbytes.AsSpan());
        True(
            Windowsˉpublisherˉresult.Success,
            "The Windows console-application publisher was rejected: " +
                string.Join(" | ", Windowsˉpublisherˉresult.Diagnostics));
        var Windowsˉpublisher = Windowsˉpublisherˉresult.Imageˉbytes;
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract
                .WINDOWS_APPLICATION_BYTES,
            Windowsˉpublisher.Length);
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract
                .WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windowsˉpublisher.AsSpan()));

        var Linuxˉpublisherˉresult =
            Consoleˉapplicationˉpublisherˉapplicationˉwriter.Writeˉlinux(
                Verified,
                Native.Fragment,
                Compilation.Moduleˉbytes.AsSpan());
        True(
            Linuxˉpublisherˉresult.Success,
            "The Linux console-application publisher was rejected: " +
                string.Join(" | ", Linuxˉpublisherˉresult.Diagnostics));
        var Linuxˉpublisher = Linuxˉpublisherˉresult.Imageˉbytes;
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract
                .LINUX_APPLICATION_BYTES,
            Linuxˉpublisher.Length);
        Equal(
            Consoleˉapplicationˉpublisherˉapplicationˉcontract
                .LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linuxˉpublisher.AsSpan()));

        var Candidateˉcompilation = Seedˉcompiler.Compile(
            "module Consoleˉapplicationˉpublisherˉcandidate profile portable; " +
            "export fn Main() -> i32 { return 42; }",
            "Console-Application-Publisher-Candidate.wv");
        True(
            Candidateˉcompilation.Success,
            "The console-application publisher candidate did not compile: " +
                string.Join(" | ", Candidateˉcompilation.Diagnostics));
        var Candidateˉverified = Moduleˉcodec.Readˉandˉverify(
            Candidateˉcompilation.Moduleˉbytes.AsSpan());
        var Candidateˉnative = X64ˉnativeˉbackend.Compile(Candidateˉverified);
        var Windowsˉcandidateˉresult = Windowsˉconsoleˉapplicationˉwriter.Write(
            Candidateˉnative.Fragment);
        var Linuxˉcandidateˉresult = Linuxˉconsoleˉapplicationˉwriter.Write(
            Candidateˉnative.Fragment);
        True(Windowsˉcandidateˉresult.Success, "The Windows candidate was rejected.");
        True(Linuxˉcandidateˉresult.Success, "The Linux candidate was rejected.");

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-console-application-publisher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Extension = OperatingSystem.IsWindows() ? ".exe" : ".elf";
            var Candidateˉpath = Path.Combine(
                Directoryˉpath,
                "Candidate" + Extension);
            var Destinationˉpath = Path.Combine(
                Directoryˉpath,
                "Destination" + Extension);
            var Publisher = OperatingSystem.IsWindows()
                ? Windowsˉpublisher
                : Linuxˉpublisher;
            var Candidate = OperatingSystem.IsWindows()
                ? Windowsˉcandidateˉresult.Imageˉbytes
                : Linuxˉcandidateˉresult.Imageˉbytes;
            File.WriteAllBytes(Candidateˉpath, Candidate.AsSpan());
            File.WriteAllBytes(Destinationˉpath, [1, 2, 3, 4]);
            var Report =
                $"publication status=Complete bytes=0x{Candidate.Length:x8} " +
                $"sha256={Objectˉdigest.Calculateˉsha256(Candidate.AsSpan())}\n";

            if (OperatingSystem.IsWindows())
            {
                var Loadedˉmodules = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
                Equal(
                    0,
                    Executeˉwindowsˉapplication(
                        Publisher.ToImmutableArray(),
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
                Equal(
                    42,
                    Executeˉwindowsˉapplication(
                        File.ReadAllBytes(Destinationˉpath).ToImmutableArray()));
            }
            else
            {
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(
                    0,
                    Executeˉlinuxˉapplication(
                        Publisher.ToImmutableArray(),
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
                Equal(
                    42,
                    Executeˉlinuxˉapplication(
                        File.ReadAllBytes(Destinationˉpath).ToImmutableArray()));
            }
            Sequenceˉequal(Candidate, File.ReadAllBytes(Destinationˉpath));
            Equal(
                0,
                Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());

            var Preserved = new byte[] { 9, 8, 7, 6 };
            File.WriteAllBytes(Candidateˉpath, [0]);
            File.WriteAllBytes(Destinationˉpath, Preserved);
            if (OperatingSystem.IsWindows())
            {
                Equal(
                    1,
                    Executeˉwindowsˉapplication(
                        Publisher.ToImmutableArray(),
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        expectedˉerror:
                            "publication status=Rejected phase=console-application\n"));
            }
            else
            {
                Equal(
                    1,
                    Executeˉlinuxˉapplication(
                        Publisher.ToImmutableArray(),
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        timeoutˉmilliseconds: 60_000,
                        expectedˉerror:
                            "publication status=Rejected phase=console-application\n"));
            }
            Sequenceˉequal(Preserved, File.ReadAllBytes(Destinationˉpath));
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
