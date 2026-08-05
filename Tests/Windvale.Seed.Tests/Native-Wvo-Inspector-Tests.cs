using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WVO_INSPECTOR_WVB_BYTES = 57_297;
    private const int WINDOWS_WVO_INSPECTOR_APPLICATION_BYTES = 577_024;
    private const string WINDOWS_WVO_INSPECTOR_APPLICATION_SHA256 =
        "9f85375a9223fdc8c8bfe81f82b6b428432a21594a11179d1ab1375aa6c6886f";
    private const int LINUX_WVO_INSPECTOR_APPLICATION_BYTES = 577_536;
    private const string LINUX_WVO_INSPECTOR_APPLICATION_SHA256 =
        "dc9fff2a13256cd0dfabed4c7e9369a9d446408a00aec3eee5fd95876ce88b37";

    private static void Nativeˉwvoˉinspectorˉtargetsˉareˉdiscoverable()
    {
        var Help = Executeˉinspectorˉtool("help");
        Equal(0, Help.Exitˉcode);
        Equal(string.Empty, Help.Standardˉerror);
        Contains(
            Help.Standardˉoutput,
            Windowsˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME);
        Contains(
            Help.Standardˉoutput,
            Linuxˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME);
    }

    private static void Nativeˉwvoˉinspectorˉruns()
    {
        var Inspectorˉbytes = Compileˉwvoˉobjectˉsuccess();
        Equal(WVO_INSPECTOR_WVB_BYTES, Inspectorˉbytes.Length);
        Equal(WVO_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Inspectorˉbytes));
        var Inspectorˉmodule = Moduleˉcodec.Readˉandˉverify(Inspectorˉbytes);
        var Inspectorˉnative = X64ˉnativeˉbackend.Compile(Inspectorˉmodule);
        Nativeˉfragmentˉverifier.Verify(Inspectorˉnative.Fragment);
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
                Nativeˉservice.Textˉquote,
                Nativeˉservice.I32ˉformat,
                Nativeˉservice.U32ˉformat,
            ],
            Inspectorˉnative.Fragment.Requiredˉservices);

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
            Inspectorˉnative.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
            Inspectorˉnative.Fragment,
            Nativeˉserviceˉplatform.Linux);
        var Windows = Wvoˉinspectorˉapplicationˉwriter.Writeˉwindows(
            Inspectorˉnative.Fragment,
            Inspectorˉmodule.Module.Capabilities,
            Inspectorˉmodule.Module.Name);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows WVO inspector writer failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        Equal(WINDOWS_WVO_INSPECTOR_APPLICATION_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_WVO_INSPECTOR_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        var Verifiedˉwindows = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector);
        Equal(
            Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector,
            Verifiedˉwindows.Runtime.Metadata.Profile);

        var Linux = Wvoˉinspectorˉapplicationˉwriter.Writeˉlinux(
            Inspectorˉnative.Fragment,
            Inspectorˉmodule.Module.Capabilities,
            Inspectorˉmodule.Module.Name);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux WVO inspector writer failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        Equal(LINUX_WVO_INSPECTOR_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WVO_INSPECTOR_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        var Verifiedˉlinux = Linuxˉhostedˉverifierˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector);
        Equal(
            Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector,
            Verifiedˉlinux.Runtime.Metadata.Profile);

        foreach (var Runtime in new[] { Verifiedˉwindows.Runtime, Verifiedˉlinux.Runtime })
        {
            Equal(5, Runtime.Metadata.Capabilities.Length);
            Equal(
                Hostedˉverifierˉapplicationˉmetadata.INSPECTOR_SERVICE_COUNT,
                Runtime.Metadata.Services.Length);
            Equal(0u, Runtime.Layout.Fileˉoutputˉscratchˉbytes);
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvo-inspector-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Wvo-Object-Core.wvb");
            var Objectˉpath = Path.Combine(Directoryˉpath, "Sample.wvo");
            var Invalidˉpath = Path.Combine(Directoryˉpath, "Invalid.wvo");
            File.WriteAllBytes(Moduleˉpath, Inspectorˉbytes);
            var Objectˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject());
            File.WriteAllBytes(Objectˉpath, Objectˉbytes);
            File.WriteAllBytes(Invalidˉpath, [0]);
            var Verifiedˉobject = Objectˉcodec.Readˉandˉverify(Objectˉbytes.AsSpan());
            var Expectedˉverify =
                $"Verified object: {Verifiedˉobject.Value.Architecture}\n" +
                $"SHA-256: {Objectˉdigest.Calculateˉsha256(Objectˉbytes)}\n";
            var Expectedˉinspection = Objectˉinspector.Inspect(
                    Verifiedˉobject,
                    Objectˉbytes)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
            const string Invalidˉdiagnostic =
                "object status=Shortˉheader sections=0 symbols=0 relocations=0 offset=1\n";
            const string Usageˉdiagnostic =
                "Usage: wvo-object-core <verify|inspect> <object.wvo>\n";

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME;
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

            if (OperatingSystem.IsWindows())
            {
                Equal(0, Executeˉwindowsˉapplication(Windows.Imageˉbytes));
                var Loadedˉmodules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉverify,
                    ["verify", Objectˉpath],
                    loadedˉmodules: Loadedˉmodules));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉinspection,
                    ["inspect", Objectˉpath]));
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: ["verify", Invalidˉpath],
                    expectedˉerror: Invalidˉdiagnostic));
                Equal(64, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: ["unknown", Objectˉpath],
                    expectedˉerror: Usageˉdiagnostic));
                Equal(0, Loadedˉmodules.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            if (OperatingSystem.IsLinux())
            {
                Equal(0, Executeˉlinuxˉapplication(Linux.Imageˉbytes));
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉverify,
                    ["verify", Objectˉpath],
                    loadedˉmappings: Loadedˉmappings));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉinspection,
                    ["inspect", Objectˉpath]));
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: ["verify", Invalidˉpath],
                    expectedˉerror: Invalidˉdiagnostic));
                Equal(64, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: ["unknown", Objectˉpath],
                    expectedˉerror: Usageˉdiagnostic));
                Equal(0, Loadedˉmappings.Count(Name =>
                    Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
