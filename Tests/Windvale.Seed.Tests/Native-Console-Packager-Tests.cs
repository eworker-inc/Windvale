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
    private const int CONSOLE_PACKAGER_WVB_BYTES = 60_797;
    private const string CONSOLE_PACKAGER_WVB_SHA256 =
        "f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c";
    private const int WINDOWS_CONSOLE_PACKAGER_APPLICATION_BYTES = 708_608;
    private const string WINDOWS_CONSOLE_PACKAGER_APPLICATION_SHA256 =
        "ea8e666806618cd9c230bdc88882e9b30a98182f8486456a46c75b746a0cdab9";
    private const int LINUX_CONSOLE_PACKAGER_APPLICATION_BYTES = 708_608;
    private const string LINUX_CONSOLE_PACKAGER_APPLICATION_SHA256 =
        "d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af";
    private const int WINDOWS_CONSOLE_SEGMENTED_PACKAGER_APPLICATION_BYTES = 805_376;
    private const string WINDOWS_CONSOLE_SEGMENTED_PACKAGER_APPLICATION_SHA256 =
        "a6a6fd40a6becf0f65bbf995006e8e5410832da6f5ebc906f216f9e435032ef0";
    private const int LINUX_CONSOLE_SEGMENTED_PACKAGER_APPLICATION_BYTES = 806_912;
    private const string LINUX_CONSOLE_SEGMENTED_PACKAGER_APPLICATION_SHA256 =
        "8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d";

    private static void Nativeˉconsoleˉpackagerˉtargetsˉareˉdiscoverable()
    {
        var Help = Executeˉinspectorˉtool("help");
        Equal(0, Help.Exitˉcode);
        Equal(string.Empty, Help.Standardˉerror);
        Contains(
            Help.Standardˉoutput,
            Windowsˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME);
        Contains(
            Help.Standardˉoutput,
            Linuxˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME);
        Contains(
            Help.Standardˉoutput,
            Windowsˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME);
        Contains(
            Help.Standardˉoutput,
            Linuxˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME);
    }

    private static void Nativeˉconsoleˉpackagerˉruns()
    {
        var Packagerˉbytes = Compileˉconsoleˉpackagerˉsuccess();
        Equal(CONSOLE_PACKAGER_WVB_BYTES, Packagerˉbytes.Length);
        Equal(
            CONSOLE_PACKAGER_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Packagerˉbytes));
        var Packagerˉmodule = Moduleˉcodec.Readˉandˉverify(Packagerˉbytes);
        Equal("Consoleˉapplicationˉpackager", Packagerˉmodule.Module.Name);
        var Packagerˉnative = X64ˉnativeˉbackend.Compile(Packagerˉmodule);
        Nativeˉfragmentˉverifier.Verify(Packagerˉnative.Fragment);
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
            Packagerˉnative.Fragment.Requiredˉservices);

        var Windowsˉbundle =
            X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉpackager(
                Packagerˉnative.Fragment,
                Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle =
            X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉpackager(
                Packagerˉnative.Fragment,
                Nativeˉserviceˉplatform.Linux);
        foreach (var Bundle in new[] { Windowsˉbundle, Linuxˉbundle })
        {
            Sequenceˉequal(
                [
                    Nativeˉservice.Consoleˉwriteˉline,
                    Nativeˉservice.Processˉargumentˉcount,
                    Nativeˉservice.Processˉargument,
                    Nativeˉservice.Fileˉreadˉbytes,
                    Nativeˉservice.Textˉutf8ˉisˉvalid,
                    Nativeˉservice.Diagnosticˉwriteˉline,
                    Nativeˉservice.Enumˉname,
                    Nativeˉservice.Textˉconcat,
                    Nativeˉservice.U32ˉformat,
                    Nativeˉservice.Fileˉwriteˉbytes,
                ],
                Bundle.Placements.Select(Placement => Placement.Service));
        }

        var Windows = Hostedˉconsoleˉpackagerˉapplicationˉwriter.Writeˉwindows(
            Packagerˉnative.Fragment,
            Packagerˉmodule.Module.Capabilities,
            Packagerˉmodule.Module.Name);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows console-packager writer failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        Equal(WINDOWS_CONSOLE_PACKAGER_APPLICATION_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_CONSOLE_PACKAGER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        var Verifiedˉwindows = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager,
            Verifiedˉwindows.Runtime.Metadata.Profile);

        var Linux = Hostedˉconsoleˉpackagerˉapplicationˉwriter.Writeˉlinux(
            Packagerˉnative.Fragment,
            Packagerˉmodule.Module.Capabilities,
            Packagerˉmodule.Module.Name);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux console-packager writer failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        Equal(LINUX_CONSOLE_PACKAGER_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_CONSOLE_PACKAGER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        var Verifiedˉlinux = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager,
            Verifiedˉlinux.Runtime.Metadata.Profile);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-console-packager-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Console-Packager.wvb");
            var Nativeˉimageˉpath = Path.Combine(Directoryˉpath, "Return-42.bin");
            var Applicationˉpath = Path.Combine(
                Directoryˉpath,
                OperatingSystem.IsWindows() ? "Return-42.exe" : "Return-42.elf");
            var Repeatedˉpath = Path.Combine(
                Directoryˉpath,
                OperatingSystem.IsWindows() ? "Return-42-Again.exe" : "Return-42-Again.elf");
            var Peerˉapplicationˉpath = Path.Combine(
                Directoryˉpath,
                OperatingSystem.IsWindows() ? "Return-42-Peer.elf" : "Return-42-Peer.exe");
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.bin");
            File.WriteAllBytes(Moduleˉpath, Packagerˉbytes);
            byte[] Nativeˉimage = [0xB8, 42, 0, 0, 0, 0xC3];
            File.WriteAllBytes(Nativeˉimageˉpath, Nativeˉimage);
            byte[] Sentinel = [0x57, 0x56, 0x50];
            File.WriteAllBytes(Rejectedˉpath, Sentinel);

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "aot",
                Moduleˉpath,
                "--target",
                Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Packageˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME;
            var Planned = Consoleˉapplicationˉlayout.Plan(
                OperatingSystem.IsWindows()
                    ? Consoleˉapplicationˉtarget.Windowsˉx64
                    : Consoleˉapplicationˉtarget.Linuxˉx64,
                Nativeˉimage.Length,
                0);
            var Expectedˉreport =
                $"package status=Valid target={Packageˉtarget} " +
                "native-image-bytes=6 entry-offset=0 application-bytes=" +
                $"{Planned.Applicationˉbytes}\n";
            var Arguments = new[]
            {
                Packageˉtarget,
                Nativeˉimageˉpath,
                "0",
                Applicationˉpath,
            };
            if (OperatingSystem.IsWindows())
            {
                Equal(0, Executeˉwindowsˉapplication(Windows.Imageˉbytes));
                var Loadedˉmodules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉreport,
                    Arguments,
                    loadedˉmodules: Loadedˉmodules));
                Equal(0, Loadedˉmodules.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                var Application = Windowsˉconsoleˉapplicationˉverifier.Verify(
                    File.ReadAllBytes(Applicationˉpath));
                Sequenceˉequal(Nativeˉimage, Application.Nativeˉimageˉbytes);
                Equal(0u, Application.Nativeˉentryˉoffset);
                Equal(42, Executeˉwindowsˉapplication(
                    File.ReadAllBytes(Applicationˉpath).ToImmutableArray()));
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: [Packageˉtarget, Nativeˉimageˉpath, "6", Rejectedˉpath],
                    expectedˉerror:
                        $"package status=Invalidˉrequest target={Packageˉtarget} " +
                        "native-image-bytes=6 entry-offset=6 application-bytes=0\n"));
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: ["not-a-target", Nativeˉimageˉpath, "0", Rejectedˉpath],
                    expectedˉerror:
                        "package status=Invalidˉrequest target=invalid " +
                        "native-image-bytes=0 entry-offset=0 application-bytes=0\n"));
            }
            if (OperatingSystem.IsLinux())
            {
                Equal(0, Executeˉlinuxˉapplication(Linux.Imageˉbytes));
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉreport,
                    Arguments,
                    loadedˉmappings: Loadedˉmappings));
                Equal(0, Loadedˉmappings.Count(Name =>
                    Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                var Application = Linuxˉconsoleˉapplicationˉverifier.Verify(
                    File.ReadAllBytes(Applicationˉpath));
                Sequenceˉequal(Nativeˉimage, Application.Nativeˉimageˉbytes);
                Equal(0u, Application.Nativeˉentryˉoffset);
                Equal(42, Executeˉlinuxˉapplication(
                    File.ReadAllBytes(Applicationˉpath).ToImmutableArray()));
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: [Packageˉtarget, Nativeˉimageˉpath, "6", Rejectedˉpath],
                    expectedˉerror:
                        $"package status=Invalidˉrequest target={Packageˉtarget} " +
                        "native-image-bytes=6 entry-offset=6 application-bytes=0\n"));
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: ["not-a-target", Nativeˉimageˉpath, "0", Rejectedˉpath],
                    expectedˉerror:
                        "package status=Invalidˉrequest target=invalid " +
                        "native-image-bytes=0 entry-offset=0 application-bytes=0\n"));
            }
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Rejectedˉpath));

            var Repeatˉarguments = new[]
            {
                Packageˉtarget,
                Nativeˉimageˉpath,
                "0",
                Repeatedˉpath,
            };
            if (OperatingSystem.IsWindows())
            {
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉreport,
                    Repeatˉarguments));
            }
            if (OperatingSystem.IsLinux())
            {
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉreport,
                    Repeatˉarguments));
            }
            Sequenceˉequal(
                File.ReadAllBytes(Applicationˉpath),
                File.ReadAllBytes(Repeatedˉpath));

            var Peerˉtarget = OperatingSystem.IsWindows()
                ? Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME
                : Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME;
            var Peerˉplan = Consoleˉapplicationˉlayout.Plan(
                OperatingSystem.IsWindows()
                    ? Consoleˉapplicationˉtarget.Linuxˉx64
                    : Consoleˉapplicationˉtarget.Windowsˉx64,
                Nativeˉimage.Length,
                0);
            var Peerˉreport =
                $"package status=Valid target={Peerˉtarget} " +
                "native-image-bytes=6 entry-offset=0 application-bytes=" +
                $"{Peerˉplan.Applicationˉbytes}\n";
            var Peerˉarguments = new[]
            {
                Peerˉtarget,
                Nativeˉimageˉpath,
                "0",
                Peerˉapplicationˉpath,
            };
            if (OperatingSystem.IsWindows())
            {
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Peerˉreport,
                    Peerˉarguments));
                var Peer = Linuxˉconsoleˉapplicationˉverifier.Verify(
                    File.ReadAllBytes(Peerˉapplicationˉpath));
                Sequenceˉequal(Nativeˉimage, Peer.Nativeˉimageˉbytes);
                Equal(0u, Peer.Nativeˉentryˉoffset);
            }
            if (OperatingSystem.IsLinux())
            {
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Peerˉreport,
                    Peerˉarguments));
                var Peer = Windowsˉconsoleˉapplicationˉverifier.Verify(
                    File.ReadAllBytes(Peerˉapplicationˉpath));
                Sequenceˉequal(Nativeˉimage, Peer.Nativeˉimageˉbytes);
                Equal(0u, Peer.Nativeˉentryˉoffset);
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Compileˉconsoleˉpackagerˉsuccess()
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new("Console-Application-Packager.wv", CONSOLE_APPLICATION_PACKAGER_SOURCE),
            [
                new("Foundation/Byte-Construction.wv", BYTE_CONSTRUCTION_SOURCE),
                new("Foundation/Decimal-Parsing.wv", DECIMAL_PARSING_SOURCE),
                new(
                    "Linker/Windvale/Console-Application-Plan-Core.wv",
                    CONSOLE_APPLICATION_PLAN_CORE_SOURCE),
                new(
                    "Linker/Windvale/Console-Application-Construction-Core.wv",
                    CONSOLE_APPLICATION_CONSTRUCTION_CORE_SOURCE),
                new(
                    "Linker/Windvale/Console-Application-Verification-Core.wv",
                    CONSOLE_APPLICATION_VERIFICATION_CORE_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Console-packager composition failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }
}
