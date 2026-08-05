using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WVB_INSPECTOR_BYTES = 61_890;
    private const string WVB_INSPECTOR_SHA256 =
        "333fffcb26912aed969581d394bf0d3b8a093edfaafc565a43f8f700a8afb43d";
    private const int WINDOWS_WVB_INSPECTOR_APPLICATION_BYTES = 678_400;
    private const string WINDOWS_WVB_INSPECTOR_APPLICATION_SHA256 =
        "30f8c6cbb1555665063dfb70fa35f08d90818107298c6ab5b91f845814d22daa";
    private const int LINUX_WVB_INSPECTOR_APPLICATION_BYTES = 679_936;
    private const string LINUX_WVB_INSPECTOR_APPLICATION_SHA256 =
        "4f99dc43e1af4ad074cc15a38bfe44a433af9979985a600739780ac156a52791";

    private static readonly string LINUX_HOSTED_INSPECTOR_STARTUP_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Linux-X64-Hosted-Inspector.wva");
    private static readonly string WINDOWS_HOSTED_INSPECTOR_STARTUP_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Windows-X64-Hosted-Inspector.wva");

    private static void Nativeˉwvbˉreadˉonlyˉtoolsˉrun()
    {
        var Inspectorˉbytes = Compileˉsuccess(WVDUMP_CORE_SOURCE);
        Equal(WVB_INSPECTOR_BYTES, Inspectorˉbytes.Length);
        Equal(WVB_INSPECTOR_SHA256, Moduleˉdigest.Calculateˉsha256(Inspectorˉbytes));
        var Inspectorˉmodule = Moduleˉcodec.Readˉandˉverify(Inspectorˉbytes);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Inspectorˉmodule.Module.Capabilities.Select(Capability => Capability.Name));

        var Inspectorˉnative = X64ˉnativeˉbackend.Compile(Inspectorˉmodule);
        Sequenceˉequal(
            Enum.GetValues<Nativeˉservice>()
                .Where(Service => Service != Nativeˉservice.Fileˉwriteˉbytes),
            Inspectorˉnative.Fragment.Requiredˉservices);
        var Nativeˉentry = Inspectorˉnative.Fragment.Symbols.Single(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            Symbol.Name == "Main").Offset;

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
            Inspectorˉnative.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
            Inspectorˉnative.Fragment,
            Nativeˉserviceˉplatform.Linux);
        foreach (var Bundle in new[] { Windowsˉbundle, Linuxˉbundle })
        {
            Equal(Hostedˉverifierˉapplicationˉmetadata.INSPECTOR_SERVICE_COUNT,
                Bundle.Placements.Length);
            Sequenceˉequal(
                Enum.GetValues<Nativeˉservice>()
                    .Where(Service => Service != Nativeˉservice.Fileˉwriteˉbytes),
                Bundle.Placements.Select(Placement => Placement.Service));
        }
        Throwsˉnative(
            "WVN4018",
            () => _ = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                Inspectorˉnative.Fragment,
                Nativeˉserviceˉplatform.Windows));
        Throwsˉnative(
            "WVN4019",
            () => _ = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
                X64ˉnativeˉbackend.Compile(
                    Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(NATIVE_CONSTANT_SOURCE)))
                    .Fragment,
                Nativeˉserviceˉplatform.Windows));

        var Windows = Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉinspector(
            Inspectorˉnative.Fragment,
            Inspectorˉmodule.Module.Capabilities);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows WVB inspector failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        Equal(WINDOWS_WVB_INSPECTOR_APPLICATION_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_WVB_INSPECTOR_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Sequenceˉequal(
            Windows.Imageˉbytes,
            Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                Inspectorˉmodule.Module.Capabilities,
                Windowsˉbundle,
                Nativeˉentry,
                Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector));
        var Verifiedˉwindows = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector);

        var Linux = Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉinspector(
            Inspectorˉnative.Fragment,
            Inspectorˉmodule.Module.Capabilities);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux WVB inspector failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        Equal(LINUX_WVB_INSPECTOR_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WVB_INSPECTOR_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        Sequenceˉequal(
            Linux.Imageˉbytes,
            Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                Inspectorˉmodule.Module.Capabilities,
                Linuxˉbundle,
                Nativeˉentry,
                Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector));
        var Verifiedˉlinux = Linuxˉhostedˉverifierˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector);

        foreach (var Runtime in new[] { Verifiedˉwindows.Runtime, Verifiedˉlinux.Runtime })
        {
            Equal(Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector,
                Runtime.Metadata.Profile);
            Equal(5, Runtime.Metadata.Capabilities.Length);
            Equal(11, Runtime.Metadata.Services.Length);
            Equal(0u, Runtime.Layout.Fileˉoutputˉscratchˉbytes);
        }

        Verifyˉinspectorˉstartupˉsource(
            WINDOWS_HOSTED_INSPECTOR_STARTUP_SOURCE,
            Windowsˉhostedˉinspectorˉstartup.WVO_SHA256,
            Windowsˉhostedˉinspectorˉstartup.Templateˉforˉverification());
        Verifyˉinspectorˉstartupˉsource(
            LINUX_HOSTED_INSPECTOR_STARTUP_SOURCE,
            Linuxˉhostedˉinspectorˉstartup.WVO_SHA256,
            Linuxˉhostedˉinspectorˉstartup.Templateˉforˉverification());

        var Embeddedˉmodule = (Bytesˉdataˉdeclaration)Inspectorˉmodule.Module.Data.Single(
            Data => Data.Name == "Validˉmodule");
        var Expectedˉreport =
            """
            wvdump 1
            module version=1.11 profile=portable name="A"
            section name=module offset=20 bytes=7 count=1
            section name=capabilities offset=35 bytes=4 count=0
            section name=data offset=47 bytes=4 count=0
            section name=functions offset=59 bytes=4 count=0
            section name=code offset=71 bytes=0 count=0
            section name=exports offset=79 bytes=4 count=0
            section name=types offset=91 bytes=4 count=0
            """.Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvb-inspector-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Validˉpath = Path.Combine(Directoryˉpath, "Valid.wvb");
            var Invalidˉpath = Path.Combine(Directoryˉpath, "Invalid.wvb");
            var Inspectorˉpath = Path.Combine(Directoryˉpath, "Wvb-Inspector.wvb");
            File.WriteAllBytes(Validˉpath, Embeddedˉmodule.Values.AsSpan());
            var Invalidˉbytes = Embeddedˉmodule.Values.ToArray();
            Invalidˉbytes[0] = 0;
            File.WriteAllBytes(Invalidˉpath, Invalidˉbytes);
            File.WriteAllBytes(Inspectorˉpath, Inspectorˉbytes);

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "aot",
                Inspectorˉpath,
                "--target",
                Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Inspectorˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            if (OperatingSystem.IsWindows())
            {
                var Loadedˉmodules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉreport,
                    [Validˉpath],
                    loadedˉmodules: Loadedˉmodules));
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: [Invalidˉpath],
                    expectedˉerror: "Badˉmagic sections=0 offset=0\n"));
                Equal(0, Loadedˉmodules.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            if (OperatingSystem.IsLinux())
            {
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉreport,
                    [Validˉpath],
                    loadedˉmappings: Loadedˉmappings));
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: [Invalidˉpath],
                    expectedˉerror: "Badˉmagic sections=0 offset=0\n"));
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

    private static void Verifyˉinspectorˉstartupˉsource(
        string source,
        string expectedˉwvoˉsha256,
        ImmutableArray<byte> expectedˉtemplate)
    {
        var Objectˉbytes = Assembleˉsuccess(source);
        Equal(expectedˉwvoˉsha256, Objectˉdigest.Calculateˉsha256(Objectˉbytes));
        var Object = Objectˉcodec.Readˉandˉverify(Objectˉbytes).Value;
        Equal(1, Object.Sections.Length);
        Sequenceˉequal(expectedˉtemplate, Object.Sections[0].Data);
    }

    private static (int Exitˉcode, string Standardˉoutput, string Standardˉerror)
        Executeˉinspectorˉtool(params string[] arguments)
    {
        var Originalˉoutput = Console.Out;
        var Originalˉerror = Console.Error;
        using var Output = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
        using var Error = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            Console.SetOut(Output);
            Console.SetError(Error);
            var Exitˉcode = Windvale.Tool.Program.Main(arguments);
            return (Exitˉcode, Output.ToString(), Error.ToString());
        }
        finally
        {
            Console.SetOut(Originalˉoutput);
            Console.SetError(Originalˉerror);
        }
    }
}
