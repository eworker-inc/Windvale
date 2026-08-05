using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WV_LINKER_WVB_BYTES = 127_482;
    private const int WINDOWS_WV_LINKER_APPLICATION_BYTES = 1_655_296;
    private const string WINDOWS_WV_LINKER_APPLICATION_SHA256 =
        "ca88735061d7e36e79813346621a867a9293d04d3c01ffb0336f4ee32cbe316d";
    private const int LINUX_WV_LINKER_APPLICATION_BYTES = 1_654_784;
    private const string LINUX_WV_LINKER_APPLICATION_SHA256 =
        "994f27f5a2449990b767c0ed8c8c367e2676d41d652ee9a61eab1de36de82dc2";

    private static void Nativeˉwvˉlinkerˉtargetsˉareˉdiscoverable()
    {
        var Help = Executeˉinspectorˉtool("help");
        Equal(0, Help.Exitˉcode);
        Equal(string.Empty, Help.Standardˉerror);
        Contains(Help.Standardˉoutput, Windowsˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME);
        Contains(Help.Standardˉoutput, Linuxˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME);
    }

    private static void Nativeˉwvˉlinkerˉruns()
    {
        var Linkerˉbytes = Compileˉwvˉlinkerˉsuccess();
        Equal(WV_LINKER_WVB_BYTES, Linkerˉbytes.Length);
        Equal(WVLINK_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Linkerˉbytes));
        var Linkerˉmodule = Moduleˉcodec.Readˉandˉverify(Linkerˉbytes);
        var Linkerˉnative = X64ˉnativeˉbackend.Compile(Linkerˉmodule);
        Nativeˉfragmentˉverifier.Verify(Linkerˉnative.Fragment);
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
            Linkerˉnative.Fragment.Requiredˉservices);

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvˉlinker(
            Linkerˉnative.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvˉlinker(
            Linkerˉnative.Fragment,
            Nativeˉserviceˉplatform.Linux);
        foreach (var Bundle in new[] { Windowsˉbundle, Linuxˉbundle })
        {
            Sequenceˉequal(
                Linkerˉnative.Fragment.Requiredˉservices,
                Bundle.Placements.Select(Placement => Placement.Service));
        }

        var Windows = Hostedˉwvˉlinkerˉapplicationˉwriter.Writeˉwindows(
            Linkerˉnative.Fragment,
            Linkerˉmodule.Module.Capabilities,
            Linkerˉmodule.Module.Name);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows Windvale linker writer failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        Equal(WINDOWS_WV_LINKER_APPLICATION_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_WV_LINKER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        var Verifiedˉwindows = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Wvˉlinker);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Wvˉlinker,
            Verifiedˉwindows.Runtime.Metadata.Profile);

        var Linux = Hostedˉwvˉlinkerˉapplicationˉwriter.Writeˉlinux(
            Linkerˉnative.Fragment,
            Linkerˉmodule.Module.Capabilities,
            Linkerˉmodule.Module.Name);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux Windvale linker writer failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        Equal(LINUX_WV_LINKER_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WV_LINKER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        var Verifiedˉlinux = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Wvˉlinker);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Wvˉlinker,
            Verifiedˉlinux.Runtime.Metadata.Profile);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wv-linker-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Wv-Linker.wvb");
            var Mainˉpath = Path.Combine(Directoryˉpath, "Main.wvo");
            var Providerˉpath = Path.Combine(Directoryˉpath, "Provider.wvo");
            var Invalidˉpath = Path.Combine(Directoryˉpath, "Invalid.wvo");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Application.bin");
            File.WriteAllBytes(Moduleˉpath, Linkerˉbytes);
            var Mainˉbytes = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE);
            var Providerˉbytes = Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE);
            File.WriteAllBytes(Mainˉpath, Mainˉbytes);
            File.WriteAllBytes(Providerˉpath, Providerˉbytes);
            File.WriteAllBytes(Invalidˉpath, [0]);

            var Oracle = Linkˉsuccess(
                [Mainˉbytes, Providerˉbytes],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            var Expectedˉmap = System.Text.Encoding.UTF8.GetString(Oracle.Mapˉbytes.AsSpan());
            Contains(Expectedˉmap, " addend=-4 value=6");

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME;
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

            var Arguments = new[]
            {
                Linkˉcontract.DEFAULT_BASE_ADDRESS.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "Main",
                Outputˉpath,
                Mainˉpath,
                Providerˉpath,
            };
            const string Invalidˉdiagnostic =
                "link status=WVL1002 inputs=1 sections=0 symbols=0 relocations=0 " +
                "image-bytes=0 entry-address=0 input=0\n";
            byte[] Existingˉoutput = [0x57, 0x56, 0x4C];
            if (OperatingSystem.IsWindows())
            {
                Equal(0, Executeˉwindowsˉapplication(Windows.Imageˉbytes));
                var Loadedˉmodules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉmap,
                    Arguments,
                    loadedˉmodules: Loadedˉmodules));
                Equal(0, Loadedˉmodules.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                Sequenceˉequal(Oracle.Imageˉbytes, File.ReadAllBytes(Outputˉpath));
                File.WriteAllBytes(Outputˉpath, Existingˉoutput);
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments:
                    [
                        "1048576",
                        "Main",
                        Outputˉpath,
                        Invalidˉpath,
                    ],
                    expectedˉerror: Invalidˉdiagnostic));
            }
            if (OperatingSystem.IsLinux())
            {
                Equal(0, Executeˉlinuxˉapplication(Linux.Imageˉbytes));
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉmap,
                    Arguments,
                    loadedˉmappings: Loadedˉmappings));
                Equal(0, Loadedˉmappings.Count(Name =>
                    Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                Sequenceˉequal(Oracle.Imageˉbytes, File.ReadAllBytes(Outputˉpath));
                File.WriteAllBytes(Outputˉpath, Existingˉoutput);
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments:
                    [
                        "1048576",
                        "Main",
                        Outputˉpath,
                        Invalidˉpath,
                    ],
                    expectedˉerror: Invalidˉdiagnostic));
            }
            Sequenceˉequal(Existingˉoutput, File.ReadAllBytes(Outputˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
