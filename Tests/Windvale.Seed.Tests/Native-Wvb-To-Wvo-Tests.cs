using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_STATIC_DESCRIPTORS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Static-Descriptors.wv");
    private static readonly string WVB_TO_WVO_TEXT_SERVICES_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Text-Services.wv");
    private static readonly string WVB_TO_WVO_ENUMS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Enums.wv");
    private static readonly string WVB_TO_WVO_RECORDS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Records.wv");
    private static readonly string WVB_TO_WVO_RECORD_CALLS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Record-Calls.wv");
    private static readonly string WVB_TO_WVO_PROCESS_ARGUMENT_COUNT_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Process-Argument-Count.wv");
    private static readonly string WVB_TO_WVO_PROCESS_ARGUMENT_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Process-Argument.wv");
    private static readonly string WVB_TO_WVO_FILE_READ_BYTES_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-File-Read-Bytes.wv");
    private static readonly string WVB_TO_WVO_FILE_WRITE_BYTES_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-File-Write-Bytes.wv");
    private static readonly string WVB_TO_WVO_CONSOLE_WRITE_LINE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Console-Write-Line.wv");
    private static readonly string WVB_TO_WVO_DIAGNOSTIC_WRITE_LINE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Diagnostic-Write-Line.wv");
    private static readonly string WVB_TO_WVO_LARGE_ENVELOPE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Large-Envelope.wv");
    private static readonly string WVB_TO_WVO_DESCRIPTOR_CALLS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Descriptor-Calls.wv");

    private const int WVB_TO_WVO_TOOL_WVB_BYTES = 322_477;
    private const int WINDOWS_WVB_TO_WVO_APPLICATION_BYTES = 4_451_328;
    private const string WINDOWS_WVB_TO_WVO_APPLICATION_SHA256 =
        "96b30a5a0256e753774633063956f8db03e14d2feb5cf9c96212f5427d7061e4";
    private const int LINUX_WVB_TO_WVO_APPLICATION_BYTES = 4_452_352;
    private const string LINUX_WVB_TO_WVO_APPLICATION_SHA256 =
        "1ce42f94519df8ad40e3b813c89ac5f30b7dd2d010af6270029f8c8f75f327d8";
    private const int WVB_TO_WVO_FIXTURE_WVB_BYTES = 174;
    private const string WVB_TO_WVO_FIXTURE_WVB_SHA256 =
        "7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31";
    private const int WVB_TO_WVO_FIXTURE_OBJECT_BYTES = 479;
    private const string WVB_TO_WVO_FIXTURE_OBJECT_SHA256 =
        "0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5";

    private static void Nativeˉwvbˉtoˉwvoˉtargetsˉareˉdiscoverable()
    {
        var Help = Executeˉinspectorˉtool("help");
        Equal(0, Help.Exitˉcode);
        Equal(string.Empty, Help.Standardˉerror);
        Contains(
            Help.Standardˉoutput,
            Windowsˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME);
        Contains(
            Help.Standardˉoutput,
            Linuxˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME);
    }

    private static void Nativeˉwvbˉtoˉwvoˉruns()
    {
        var Toolˉbytes = Compileˉwvbˉtoˉwvoˉtoolˉsuccess();
        Equal(WVB_TO_WVO_TOOL_WVB_BYTES, Toolˉbytes.Length);
        Equal(NATIVE_X64_LOWERING_TOOL_SHA256, Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Toolˉmodule = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        Equal("Compilerˉnativeˉx64ˉloweringˉtool", Toolˉmodule.Module.Name);
        var Toolˉnative = X64ˉnativeˉbackend.Compile(Toolˉmodule);
        Nativeˉfragmentˉverifier.Verify(Toolˉnative.Fragment);
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
            Toolˉnative.Fragment.Requiredˉservices);

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvbˉtoˉwvo(
            Toolˉnative.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvbˉtoˉwvo(
            Toolˉnative.Fragment,
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

        var Windows = Hostedˉwvbˉtoˉwvoˉapplicationˉwriter.Writeˉwindows(
            Toolˉnative.Fragment,
            Toolˉmodule.Module.Capabilities,
            Toolˉmodule.Module.Name);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows WVB-to-WVO writer failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        var Verifiedˉwindows = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo,
            Verifiedˉwindows.Runtime.Metadata.Profile);

        var Linux = Hostedˉwvbˉtoˉwvoˉapplicationˉwriter.Writeˉlinux(
            Toolˉnative.Fragment,
            Toolˉmodule.Module.Capabilities,
            Toolˉmodule.Module.Name);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux WVB-to-WVO writer failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        Console.WriteLine(
            $"NATIVE_WVB_TO_WVO_APPLICATION_MEASUREMENT " +
            $"windows-bytes={Windows.Imageˉbytes.Length} " +
            $"windows-sha256={Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan())} " +
            $"linux-bytes={Linux.Imageˉbytes.Length} " +
            $"linux-sha256={Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan())}");
        var Verifiedˉlinux = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo,
            Verifiedˉlinux.Runtime.Metadata.Profile);

        var Fixtureˉwvb = Compileˉsuccess(WVB_TO_WVO_RETURN_42_SOURCE);
        Equal(WVB_TO_WVO_FIXTURE_WVB_BYTES, Fixtureˉwvb.Length);
        Equal(WVB_TO_WVO_FIXTURE_WVB_SHA256, Moduleˉdigest.Calculateˉsha256(Fixtureˉwvb));
        var Fixtureˉmodule = Moduleˉcodec.Readˉandˉverify(Fixtureˉwvb);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Fixtureˉmodule).Fragment);
        Equal(WVB_TO_WVO_FIXTURE_OBJECT_BYTES, Expectedˉobject.Length);
        Equal(
            WVB_TO_WVO_FIXTURE_OBJECT_SHA256,
            Objectˉdigest.Calculateˉsha256(Expectedˉobject.AsSpan()));
        _ = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan());

        var Nominalˉwvb = Compileˉsuccess(SOURCE_WVB_NOMINAL_TYPES_SOURCE);
        Equal(
            SOURCE_WVB_NOMINAL_TYPES_SHA256,
            Moduleˉdigest.Calculateˉsha256(Nominalˉwvb));
        var Nominalˉmodule = Moduleˉcodec.Readˉandˉverify(Nominalˉwvb);
        var Expectedˉnominalˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Nominalˉmodule).Fragment);
        var Expectedˉnominalˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉnominalˉobject.AsSpan()).Value;
        var Expectedˉnominalˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉnominalˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉnominalˉobject.Length}\n";
        var Capabilityˉwvb = Compileˉsuccess(WVB_TO_WVO_PROCESS_ARGUMENT_COUNT_SOURCE);
        var Capabilityˉmodule = Moduleˉcodec.Readˉandˉverify(Capabilityˉwvb);
        var Expectedˉcapabilityˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Capabilityˉmodule).Fragment);
        var Expectedˉcapabilityˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉcapabilityˉobject.AsSpan()).Value;
        var Expectedˉcapabilityˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉcapabilityˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉcapabilityˉobject.Length}\n";
        var Argumentˉwvb = Compileˉsuccess(WVB_TO_WVO_PROCESS_ARGUMENT_SOURCE);
        var Argumentˉmodule = Moduleˉcodec.Readˉandˉverify(Argumentˉwvb);
        var Expectedˉargumentˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Argumentˉmodule).Fragment);
        var Expectedˉargumentˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉargumentˉobject.AsSpan()).Value;
        var Expectedˉargumentˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉargumentˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉargumentˉobject.Length}\n";
        var Fileˉreadˉwvb = Compileˉsuccess(WVB_TO_WVO_FILE_READ_BYTES_SOURCE);
        var Fileˉreadˉmodule = Moduleˉcodec.Readˉandˉverify(Fileˉreadˉwvb);
        var Expectedˉfileˉreadˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Fileˉreadˉmodule).Fragment);
        var Expectedˉfileˉreadˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉfileˉreadˉobject.AsSpan()).Value;
        var Expectedˉfileˉreadˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉfileˉreadˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉfileˉreadˉobject.Length}\n";
        var Fileˉwriteˉwvb = Compileˉsuccess(WVB_TO_WVO_FILE_WRITE_BYTES_SOURCE);
        var Fileˉwriteˉmodule = Moduleˉcodec.Readˉandˉverify(Fileˉwriteˉwvb);
        var Expectedˉfileˉwriteˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Fileˉwriteˉmodule).Fragment);
        var Expectedˉfileˉwriteˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉfileˉwriteˉobject.AsSpan()).Value;
        var Expectedˉfileˉwriteˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉfileˉwriteˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉfileˉwriteˉobject.Length}\n";
        var Consoleˉwvb = Compileˉsuccess(WVB_TO_WVO_CONSOLE_WRITE_LINE_SOURCE);
        var Consoleˉmodule = Moduleˉcodec.Readˉandˉverify(Consoleˉwvb);
        var Expectedˉconsoleˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Consoleˉmodule).Fragment);
        var Expectedˉconsoleˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉconsoleˉobject.AsSpan()).Value;
        var Expectedˉconsoleˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉconsoleˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉconsoleˉobject.Length}\n";
        var Diagnosticˉwvb = Compileˉsuccess(WVB_TO_WVO_DIAGNOSTIC_WRITE_LINE_SOURCE);
        var Diagnosticˉmodule = Moduleˉcodec.Readˉandˉverify(Diagnosticˉwvb);
        var Expectedˉdiagnosticˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(Diagnosticˉmodule).Fragment);
        var Expectedˉdiagnosticˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉdiagnosticˉobject.AsSpan()).Value;
        var Expectedˉdiagnosticˉreport =
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉdiagnosticˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉdiagnosticˉobject.Length}\n";

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvb-to-wvo-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Toolˉpath = Path.Combine(Directoryˉpath, "Wvb-To-Wvo.wvb");
            var Inputˉpath = Path.Combine(Directoryˉpath, "Return-42.wvb");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Return-42.wvo");
            var Repeatedˉpath = Path.Combine(Directoryˉpath, "Return-42-Again.wvo");
            var Nominalˉinputˉpath = Path.Combine(Directoryˉpath, "Nominal-Types.wvb");
            var Nominalˉoutputˉpath = Path.Combine(Directoryˉpath, "Nominal-Types.wvo");
            var Capabilityˉinputˉpath = Path.Combine(
                Directoryˉpath,
                "Process-Argument-Count.wvb");
            var Capabilityˉoutputˉpath = Path.Combine(
                Directoryˉpath,
                "Process-Argument-Count.wvo");
            var Argumentˉinputˉpath = Path.Combine(
                Directoryˉpath,
                "Process-Argument.wvb");
            var Argumentˉoutputˉpath = Path.Combine(
                Directoryˉpath,
                "Process-Argument.wvo");
            var Fileˉreadˉinputˉpath = Path.Combine(
                Directoryˉpath,
                "File-Read-Bytes.wvb");
            var Fileˉreadˉoutputˉpath = Path.Combine(
                Directoryˉpath,
                "File-Read-Bytes.wvo");
            var Fileˉwriteˉinputˉpath = Path.Combine(
                Directoryˉpath,
                "File-Write-Bytes.wvb");
            var Fileˉwriteˉoutputˉpath = Path.Combine(
                Directoryˉpath,
                "File-Write-Bytes.wvo");
            var Consoleˉinputˉpath = Path.Combine(
                Directoryˉpath,
                "Console-Write-Line.wvb");
            var Consoleˉoutputˉpath = Path.Combine(
                Directoryˉpath,
                "Console-Write-Line.wvo");
            var Diagnosticˉinputˉpath = Path.Combine(
                Directoryˉpath,
                "Diagnostic-Write-Line.wvb");
            var Diagnosticˉoutputˉpath = Path.Combine(
                Directoryˉpath,
                "Diagnostic-Write-Line.wvo");
            var Invalidˉpath = Path.Combine(Directoryˉpath, "Invalid.wvb");
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.wvo");
            File.WriteAllBytes(Toolˉpath, Toolˉbytes);
            File.WriteAllBytes(Inputˉpath, Fixtureˉwvb);
            File.WriteAllBytes(Nominalˉinputˉpath, Nominalˉwvb);
            File.WriteAllBytes(Capabilityˉinputˉpath, Capabilityˉwvb);
            File.WriteAllBytes(Argumentˉinputˉpath, Argumentˉwvb);
            File.WriteAllBytes(Fileˉreadˉinputˉpath, Fileˉreadˉwvb);
            File.WriteAllBytes(Fileˉwriteˉinputˉpath, Fileˉwriteˉwvb);
            File.WriteAllBytes(Consoleˉinputˉpath, Consoleˉwvb);
            File.WriteAllBytes(Diagnosticˉinputˉpath, Diagnosticˉwvb);
            File.WriteAllBytes(Invalidˉpath, Fixtureˉwvb[..^1]);
            byte[] Sentinel = [0x57, 0x56, 0x4F];
            File.WriteAllBytes(Rejectedˉpath, Sentinel);

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "aot",
                Toolˉpath,
                "--target",
                Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Toolˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            const string Expectedˉreport =
                "native x64 status=Valid abi=22 code-bytes=406 object-bytes=479\n";
            var Arguments = new[] { Inputˉpath, Outputˉpath };
            if (OperatingSystem.IsWindows())
            {
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
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    expectedˉerror: "Usage: wvnative <input.wvb> <output.wvo>\n"));
                Equal(1, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: [Invalidˉpath, Rejectedˉpath],
                    expectedˉerror: "native x64 status=Invalidˉwvb\n"));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉreport,
                    [Inputˉpath, Repeatedˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉnominalˉreport,
                    [Nominalˉinputˉpath, Nominalˉoutputˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉcapabilityˉreport,
                    [Capabilityˉinputˉpath, Capabilityˉoutputˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉargumentˉreport,
                    [Argumentˉinputˉpath, Argumentˉoutputˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉfileˉreadˉreport,
                    [Fileˉreadˉinputˉpath, Fileˉreadˉoutputˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉfileˉwriteˉreport,
                    [Fileˉwriteˉinputˉpath, Fileˉwriteˉoutputˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉconsoleˉreport,
                    [Consoleˉinputˉpath, Consoleˉoutputˉpath]));
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉdiagnosticˉreport,
                    [Diagnosticˉinputˉpath, Diagnosticˉoutputˉpath]));
            }
            if (OperatingSystem.IsLinux())
            {
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
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    expectedˉerror: "Usage: wvnative <input.wvb> <output.wvo>\n"));
                Equal(1, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: [Invalidˉpath, Rejectedˉpath],
                    expectedˉerror: "native x64 status=Invalidˉwvb\n"));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉreport,
                    [Inputˉpath, Repeatedˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉnominalˉreport,
                    [Nominalˉinputˉpath, Nominalˉoutputˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉcapabilityˉreport,
                    [Capabilityˉinputˉpath, Capabilityˉoutputˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉargumentˉreport,
                    [Argumentˉinputˉpath, Argumentˉoutputˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉfileˉreadˉreport,
                    [Fileˉreadˉinputˉpath, Fileˉreadˉoutputˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉfileˉwriteˉreport,
                    [Fileˉwriteˉinputˉpath, Fileˉwriteˉoutputˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉconsoleˉreport,
                    [Consoleˉinputˉpath, Consoleˉoutputˉpath]));
                Equal(0, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    Expectedˉdiagnosticˉreport,
                    [Diagnosticˉinputˉpath, Diagnosticˉoutputˉpath]));
            }

            Sequenceˉequal(Expectedˉobject, File.ReadAllBytes(Outputˉpath));
            Sequenceˉequal(Expectedˉobject, File.ReadAllBytes(Repeatedˉpath));
            Sequenceˉequal(
                Expectedˉnominalˉobject,
                File.ReadAllBytes(Nominalˉoutputˉpath));
            Sequenceˉequal(
                Expectedˉcapabilityˉobject,
                File.ReadAllBytes(Capabilityˉoutputˉpath));
            Sequenceˉequal(
                Expectedˉargumentˉobject,
                File.ReadAllBytes(Argumentˉoutputˉpath));
            Sequenceˉequal(
                Expectedˉfileˉreadˉobject,
                File.ReadAllBytes(Fileˉreadˉoutputˉpath));
            Sequenceˉequal(
                Expectedˉfileˉwriteˉobject,
                File.ReadAllBytes(Fileˉwriteˉoutputˉpath));
            Sequenceˉequal(
                Expectedˉconsoleˉobject,
                File.ReadAllBytes(Consoleˉoutputˉpath));
            Sequenceˉequal(
                Expectedˉdiagnosticˉobject,
                File.ReadAllBytes(Diagnosticˉoutputˉpath));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Rejectedˉpath));
            _ = Objectˉcodec.Readˉandˉverify(File.ReadAllBytes(Outputˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }

        Equal(WINDOWS_WVB_TO_WVO_APPLICATION_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_WVB_TO_WVO_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Equal(LINUX_WVB_TO_WVO_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WVB_TO_WVO_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
    }

    private static byte[] Compileˉwvbˉtoˉwvoˉtoolˉsuccess()
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new("Compiler/Windvale/Native-X64-Lowering-Tool.wv", NATIVE_X64_LOWERING_TOOL_SOURCE),
            [
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Core.wv",
                    NATIVE_X64_LOWERING_CORE_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Capabilities.wv",
                    NATIVE_X64_LOWERING_CAPABILITIES_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Data.wv",
                    NATIVE_X64_LOWERING_DATA_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Static-Data-Instructions.wv",
                    NATIVE_X64_LOWERING_STATIC_DATA_INSTRUCTIONS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Types.wv",
                    NATIVE_X64_LOWERING_TYPES_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Call-Arguments.wv",
                    NATIVE_X64_LOWERING_CALL_ARGUMENTS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Enums.wv",
                    NATIVE_X64_LOWERING_ENUMS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Enum-Instructions.wv",
                    NATIVE_X64_LOWERING_ENUM_INSTRUCTIONS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Allocation.wv",
                    NATIVE_X64_LOWERING_RECORD_ALLOCATION_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Local-Liveness.wv",
                    NATIVE_X64_LOWERING_RECORD_LOCAL_LIVENESS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Storage.wv",
                    NATIVE_X64_LOWERING_RECORD_STORAGE_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Records.wv",
                    NATIVE_X64_LOWERING_RECORDS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Instructions.wv",
                    NATIVE_X64_LOWERING_RECORD_INSTRUCTIONS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Call-Instructions.wv",
                    NATIVE_X64_LOWERING_CALL_INSTRUCTIONS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Descriptors.wv",
                    NATIVE_X64_LOWERING_DESCRIPTORS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Descriptor-Calls.wv",
                    NATIVE_X64_LOWERING_DESCRIPTOR_CALLS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Descriptor-Instructions.wv",
                    NATIVE_X64_LOWERING_DESCRIPTOR_INSTRUCTIONS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Runtime-Descriptors.wv",
                    NATIVE_X64_LOWERING_RUNTIME_DESCRIPTORS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Bytes-Concatenation.wv",
                    NATIVE_X64_LOWERING_BYTES_CONCATENATION_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Layout.wv",
                    NATIVE_X64_LOWERING_LAYOUT_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Object.wv",
                    NATIVE_X64_LOWERING_OBJECT_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale native x64 tool compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }

    private static void Assertˉlargeˉmoduleˉenvelopeˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_LARGE_ENVELOPE_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(9, Module.Module.Data.Length);
        Equal(9, Module.Module.Types.Length);
        Equal(10, Module.Functions.Length);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;
        True(Expectedˉview.Symbols.Any(Symbol =>
            StringComparer.Ordinal.Equals(Symbol.Name, "$data_0008")),
            "The large-envelope WVO omitted the ninth canonical data symbol.");
        True(Expectedˉview.Symbols.Any(Symbol =>
            StringComparer.Ordinal.Equals(Symbol.Name, "$function_0008")),
            "The large-envelope WVO omitted the ninth canonical helper symbol.");

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉlargeˉfunctionˉenvelopeˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Template = Moduleˉcodec.Read(Compileˉsuccess(WVB_TO_WVO_RETURN_42_SOURCE));
        var Code = ImmutableArray.CreateBuilder<byte>();
        for (uint Local = 1; Local <= 1_024; Local++)
        {
            Code.AddRange(U32ˉinstruction(Opcode.Localˉload, Local - 1));
            Code.AddRange(U32ˉinstruction(Opcode.Localˉstore, Local));
        }
        Code.AddRange(U32ˉinstruction(Opcode.Localˉload, 1_024));
        Code.Add((byte)Opcode.Return);
        var Localˉtypes = Enumerable.Repeat<Valueˉshape>(
            Valueˉtype.I32,
            1_025).ToImmutableArray();
        var Function = Template.Functions.Single() with
        {
            Localˉtypes = Localˉtypes,
            Codeˉoffset = 0,
            Codeˉlength = Code.Count,
            Maximumˉstackˉdepth = 1,
        };
        var Wvb = Moduleˉcodec.Write(Template with
        {
            Functions = [Function],
            Code = Code.ToImmutable(),
        });
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(1_025, Module.Functions[0].Declaration.Allˉlocalˉtypes.Length);
        Equal(10_246, Module.Functions[0].Declaration.Codeˉlength);
        Equal(2_050, Module.Functions[0].Instructions.Length);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(0, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 500_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 500_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉlargeˉrecordˉplannerˉenvelopeˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        const string Source = """
            module Nativeˉrecordˉplannerˉenvelope profile portable;
            record Cell { Value: i32; }
            fn Make() -> Cell { return Cell(42); }
            export fn Main() -> i32 { return Make().Value; }
            """;
        var Template = Moduleˉcodec.Read(Compileˉsuccess(Source));
        var Helper = Template.Functions.Single(Function => Function.Name != "Main");
        var Helperˉcode = ImmutableArray.CreateBuilder<byte>();
        for (var Block = 0; Block < 129; Block++)
        {
            Helperˉcode.AddRange(U32ˉinstruction(
                Opcode.Jump,
                (uint)Helperˉcode.Count + 5u));
        }
        for (var Instruction = 0; Instruction < 450; Instruction++)
        {
            Helperˉcode.AddRange(U32ˉinstruction(Opcode.Localˉload, 0));
            Helperˉcode.Add((byte)Opcode.Pop);
        }
        Helperˉcode.AddRange(I32ˉinstruction(42));
        Helperˉcode.AddRange(U32ˉinstruction(Opcode.Recordˉcreate, 0));
        Helperˉcode.Add((byte)Opcode.Return);
        var Localˉtypes = ImmutableArray.CreateBuilder<Valueˉshape>();
        Localˉtypes.Add(Valueˉtype.I32);
        Localˉtypes.AddRange(Enumerable.Repeat(
            Valueˉshape.Forˉrecord(0),
            129));

        var Functions = ImmutableArray.CreateBuilder<Functionˉdeclaration>();
        var Code = ImmutableArray.CreateBuilder<byte>();
        foreach (var Function in Template.Functions)
        {
            var Functionˉcode = Function == Helper
                ? Helperˉcode.ToImmutable()
                : Template.Code
                    .Skip(Function.Codeˉoffset)
                    .Take(Function.Codeˉlength)
                    .ToImmutableArray();
            Functions.Add(Function with
            {
                Localˉtypes = Function == Helper
                    ? Localˉtypes.ToImmutable()
                    : Function.Localˉtypes,
                Codeˉoffset = Code.Count,
                Codeˉlength = Functionˉcode.Length,
                Maximumˉstackˉdepth = Function == Helper
                    ? 1
                    : Function.Maximumˉstackˉdepth,
            });
            Code.AddRange(Functionˉcode);
        }
        var Wvb = Moduleˉcodec.Write(Template with
        {
            Functions = Functions.ToImmutable(),
            Code = Code.ToImmutable(),
        });
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Verifiedˉhelper = Module.Functions.Single(
            Function => Function.Declaration.Name == Helper.Name);
        Equal(130, Verifiedˉhelper.Declaration.Allˉlocalˉtypes.Length);
        Equal(129, Verifiedˉhelper.Declaration.Localˉtypes.Count(
            Type => Type.Kind == Valueˉtype.Record));
        Equal(3_356, Verifiedˉhelper.Declaration.Codeˉlength);
        Equal(1_032, Verifiedˉhelper.Instructions.Length);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(130, Native.Module.Functions.Single(
            Function => Function.Name == Helper.Name).Blocks.Length);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 500_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 500_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉdescriptorˉcallˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_DESCRIPTOR_CALLS_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        True(Module.Functions.Any(Function =>
            Function.Declaration.Parameterˉtypes.Any(Type =>
                Type.Kind is Valueˉtype.Text or Valueˉtype.Bytes)),
            "The descriptor-call fixture omitted its descriptor parameters.");
        True(Module.Functions.Any(Function =>
            Function.Declaration.Returnˉtype.Kind is Valueˉtype.Text or Valueˉtype.Bytes),
            "The descriptor-call fixture omitted its descriptor returns.");
        True(Module.Functions.Any(Function =>
            Function.Declaration.Parameterˉtypes.Length >
                Nativeˉcontract.REGISTER_CALL_PARAMETERS),
            "The descriptor-call fixture omitted its stack-argument boundary.");

        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(0, Toolˉresult.Exitˉcode);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉstaticˉdescriptorˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_STATIC_DESCRIPTORS_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;
        Equal(2, Expectedˉview.Sections.Length);
        Equal(Objectˉsectionˉkind.Readˉonlyˉdata, Expectedˉview.Sections[1].Kind);
        Equal(3, Expectedˉview.Symbols.Count(Symbol =>
            Symbol.Kind == Objectˉsymbolˉkind.Data));
        Equal(3, Expectedˉview.Relocations.Length);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        if (!Expectedˉobject.SequenceEqual(Memoryˉresult.Bytes))
        {
            var Sharedˉlength = Math.Min(
                Expectedˉobject.Length,
                Memoryˉresult.Bytes.Length);
            var Firstˉdifference = Enumerable.Range(0, Sharedˉlength)
                .FirstOrDefault(
                    Index => Expectedˉobject[Index] != Memoryˉresult.Bytes[Index],
                    -1);
            throw new InvalidOperationException(
                $"Static-descriptor WVO differs at {Firstˉdifference}; " +
                $"Stage0 length={Expectedˉobject.Length}, " +
                $"Windvale length={Memoryˉresult.Bytes.Length}, " +
                $"Stage0 byte={(Firstˉdifference < 0 ? -1 : Expectedˉobject[Firstˉdifference])}, " +
                $"Windvale byte={(Firstˉdifference < 0 ? -1 : Memoryˉresult.Bytes[Firstˉdifference])}.");
        }

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉtextˉserviceˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_TEXT_SERVICES_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        if (!Expectedˉobject.SequenceEqual(Memoryˉresult.Bytes))
        {
            var Sharedˉlength = Math.Min(
                Expectedˉobject.Length,
                Memoryˉresult.Bytes.Length);
            var Firstˉdifference = Enumerable.Range(0, Sharedˉlength)
                .FirstOrDefault(
                    Index => Expectedˉobject[Index] != Memoryˉresult.Bytes[Index],
                    -1);
            throw new InvalidOperationException(
                $"Text-service WVO differs at {Firstˉdifference}; " +
                $"Stage0 length={Expectedˉobject.Length}, " +
                $"Windvale length={Memoryˉresult.Bytes.Length}, " +
                $"Stage0 byte={(Firstˉdifference < 0 ? -1 : Expectedˉobject[Firstˉdifference])}, " +
                $"Windvale byte={(Firstˉdifference < 0 ? -1 : Memoryˉresult.Bytes[Firstˉdifference])}.");
        }

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉdataˉandˉtextˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(SOURCE_WVB_DATA_AND_TEXT_SOURCE);
        Equal(SOURCE_WVB_DATA_AND_TEXT_SHA256, Moduleˉdigest.Calculateˉsha256(Wvb));
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(13, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            13,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        if (!Expectedˉobject.SequenceEqual(Memoryˉresult.Bytes))
        {
            var Sharedˉlength = Math.Min(
                Expectedˉobject.Length,
                Memoryˉresult.Bytes.Length);
            var Firstˉdifference = Enumerable.Range(0, Sharedˉlength)
                .FirstOrDefault(
                    Index => Expectedˉobject[Index] != Memoryˉresult.Bytes[Index],
                    -1);
            throw new InvalidOperationException(
                $"Data-and-text WVO differs at {Firstˉdifference}; " +
                $"Stage0 length={Expectedˉobject.Length}, " +
                $"Windvale length={Memoryˉresult.Bytes.Length}, " +
                $"Stage0 byte={(Firstˉdifference < 0 ? -1 : Expectedˉobject[Firstˉdifference])}, " +
                $"Windvale byte={(Firstˉdifference < 0 ? -1 : Memoryˉresult.Bytes[Firstˉdifference])}.");
        }

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉenumˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_ENUMS_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Keep = Module.Functions.Single(
            Function => Function.Declaration.Name == "Keep");
        Equal(1, Keep.Declaration.Parameterˉtypes.Length);
        Equal(Valueˉtype.Enum, Keep.Declaration.Parameterˉtypes[0].Kind);
        Equal(
            Keep.Declaration.Parameterˉtypes[0],
            Keep.Declaration.Returnˉtype);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉrecordˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_RECORDS_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "Record lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉrecordˉcallˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_RECORD_CALLS_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Marker = Module.Module.Types.OfType<Enumˉtypeˉdeclaration>().Single();
        Equal(2, Marker.Members[0].Value);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "Record-call lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉmultipleˉrecordˉcallˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        const string Source = """
            module Nativeˉx64ˉmultipleˉrecordˉcall profile portable;

            record Cell {
                Value: i32;
            }

            fn Sum(A: Cell, B: Cell, C: Cell, D: Cell) -> i32 {
                return A.Value + B.Value + C.Value + D.Value;
            }

            export fn Main() -> i32 {
                let A = Cell(9);
                let B = Cell(10);
                let C = Cell(11);
                let D = Cell(12);
                return Sum(A, B, C, D);
            }
            """;
        var Wvb = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Sum = Module.Functions.Single(
            Function => Function.Declaration.Name == "Sum");
        Equal(4, Sum.Declaration.Parameterˉtypes.Length);
        Equal(
            4,
            Sum.Declaration.Parameterˉtypes.Count(
                Type => Type.Kind == Valueˉtype.Record));

        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }

    private static void Assertˉnominalˉtypeˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(SOURCE_WVB_NOMINAL_TYPES_SOURCE);
        Equal(SOURCE_WVB_NOMINAL_TYPES_SHA256, Moduleˉdigest.Calculateˉsha256(Wvb));
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(11, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(
            11,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "Nominal-type lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉprocessˉargumentˉcountˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_PROCESS_ARGUMENT_COUNT_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        var Capability = Module.Module.Capabilities.Single();
        Equal(Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT, Capability.Name);
        Equal(0, Capability.Parameterˉtypes.Length);
        Equal(Valueˉtype.U32, Capability.Returnˉtype);

        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT);
        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Resources),
            new(Authorized)).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [Nativeˉservice.Processˉargumentˉcount],
            Native.Fragment.Requiredˉservices);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: new(null, Authorized, Resources)));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "process.argument_count lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉprocessˉargumentˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_PROCESS_ARGUMENT_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        var Capability = Module.Module.Capabilities.Single();
        Equal(Capabilityˉcatalog.PROCESS_ARGUMENT, Capability.Name);
        Sequenceˉequal([Valueˉtype.U32], Capability.Parameterˉtypes);
        Equal(Valueˉtype.Text, Capability.Returnˉtype);

        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.PROCESS_ARGUMENT);
        var Resources = new Hostedˉresourceˉcontext(
            ["A"],
            TextWriter.Null,
            TextWriter.Null);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Resources),
            new(Authorized)).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [Nativeˉservice.Processˉargument],
            Native.Fragment.Requiredˉservices);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: new(null, Authorized, Resources)));
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "process.argument lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉfileˉreadˉbytesˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_FILE_READ_BYTES_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Sequenceˉequal(
            [Capabilityˉcatalog.FILE_READ_BYTES, Capabilityˉcatalog.PROCESS_ARGUMENT],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Readˉcapability = Module.Module.Capabilities[0];
        Sequenceˉequal([Valueˉtype.Text], Readˉcapability.Parameterˉtypes);
        Equal(Valueˉtype.Bytes, Readˉcapability.Returnˉtype);

        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-read-lowering-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        var Inputˉpath = Path.Combine(Directoryˉpath, "input.bin");
        File.WriteAllBytes(Inputˉpath, [65]);
        try
        {
            var Reader = new Testˉfileˉreader((Resourceˉname, Maximumˉbytes) =>
            {
                Equal(Inputˉpath, Resourceˉname);
                True(Maximumˉbytes >= 1, "The file-read fixture exceeded its hosted bound.");
                return [65];
            });
            var Resources = new Hostedˉresourceˉcontext(
                [Inputˉpath],
                TextWriter.Null,
                TextWriter.Null,
                Reader);
            var Interpreted = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(Resources),
                new(Authorized)).Runˉmain();
            Equal(42, Interpreted.Exitˉcode);
            Equal(1, Reader.Readˉcount);

            var Native = X64ˉnativeˉbackend.Compile(Module);
            _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
            Sequenceˉequal(
                [Nativeˉservice.Processˉargument, Nativeˉservice.Fileˉreadˉbytes],
                Native.Fragment.Requiredˉservices);
            Equal(
                42,
                X64ˉnativeˉexecutor.Executeˉi32(
                    Native.Fragment,
                    maximumˉinstructions: Interpreted.Executedˉinstructions,
                    hostˉservices: new(
                        null,
                        Authorized,
                        Resources,
                        fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem())));
            var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
            var Expectedˉview = Objectˉcodec.Readˉandˉverify(
                Expectedˉobject.AsSpan()).Value;

            var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
            if (Toolˉresult.Exitˉcode != 0)
            {
                throw new InvalidOperationException(
                    "file.read_bytes lowering failed: " + Toolˉresult.Diagnostics);
            }
            Equal(string.Empty, Toolˉresult.Diagnostics);
            Equal(
                $"native x64 status=Valid abi=22 " +
                $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
                $"object-bytes={Expectedˉobject.Length}\n",
                Toolˉresult.Output);
            Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

            var Memoryˉresult = new Referenceˉruntime(
                memory,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
                .Runˉmainˉbytes(Wvb.ToImmutableArray());
            Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static void Assertˉfileˉwriteˉbytesˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_FILE_WRITE_BYTES_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Sequenceˉequal(
            [Capabilityˉcatalog.FILE_WRITE_BYTES, Capabilityˉcatalog.PROCESS_ARGUMENT],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Writeˉcapability = Module.Module.Capabilities[0];
        Sequenceˉequal(
            [Valueˉtype.Text, Valueˉtype.Bytes],
            Writeˉcapability.Parameterˉtypes);
        Equal(Valueˉtype.Void, Writeˉcapability.Returnˉtype);

        var Authorized = Module.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-write-lowering-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        var Outputˉpath = Path.Combine(Directoryˉpath, "output.bin");
        try
        {
            var Writer = new Capturingˉfileˉwriter();
            var Resources = new Hostedˉresourceˉcontext(
                [Outputˉpath],
                TextWriter.Null,
                TextWriter.Null,
                fileˉwriter: Writer);
            var Interpreted = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(Resources),
                new(Authorized)).Runˉmain();
            Equal(42, Interpreted.Exitˉcode);
            Equal(1, Writer.Writeˉcount);
            Equal(Outputˉpath, Writer.Resourceˉname);
            Sequenceˉequal(new byte[] { 65 }, Writer.Bytes);

            var Native = X64ˉnativeˉbackend.Compile(Module);
            _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
            Sequenceˉequal(
                [Nativeˉservice.Processˉargument, Nativeˉservice.Fileˉwriteˉbytes],
                Native.Fragment.Requiredˉservices);
            Equal(
                42,
                X64ˉnativeˉexecutor.Executeˉi32(
                    Native.Fragment,
                    maximumˉinstructions: Interpreted.Executedˉinstructions,
                    hostˉservices: new(
                        null,
                        Authorized,
                        Resources,
                        fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem())));
            Sequenceˉequal(new byte[] { 65 }, File.ReadAllBytes(Outputˉpath));
            var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
            var Expectedˉview = Objectˉcodec.Readˉandˉverify(
                Expectedˉobject.AsSpan()).Value;

            var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
            if (Toolˉresult.Exitˉcode != 0)
            {
                throw new InvalidOperationException(
                    "file.write_bytes lowering failed: " + Toolˉresult.Diagnostics);
            }
            Equal(string.Empty, Toolˉresult.Diagnostics);
            Equal(
                $"native x64 status=Valid abi=22 " +
                $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
                $"object-bytes={Expectedˉobject.Length}\n",
                Toolˉresult.Output);
            Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

            var Memoryˉresult = new Referenceˉruntime(
                memory,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
                .Runˉmainˉbytes(Wvb.ToImmutableArray());
            Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static void Assertˉconsoleˉwriteˉlineˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_CONSOLE_WRITE_LINE_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        var Capability = Module.Module.Capabilities.Single();
        Equal(Capabilityˉcatalog.CONSOLE_WRITE_LINE, Capability.Name);
        Sequenceˉequal([Valueˉtype.Text], Capability.Parameterˉtypes);
        Equal(Valueˉtype.Void, Capability.Returnˉtype);

        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.CONSOLE_WRITE_LINE);
        var Output = new StringWriter();
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                Output,
                TextWriter.Null)),
            new(Authorized)).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        Equal("A\n", Output.ToString());

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [Nativeˉservice.Consoleˉwriteˉline],
            Native.Fragment.Requiredˉservices);
        using var Nativeˉoutput = new Nativeˉoutputˉcapture();
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: new(Nativeˉoutput.Channel, Authorized)));
        Equal("A\n", Nativeˉoutput.Readˉtext());
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "console.write_line lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }

    private static void Assertˉdiagnosticˉwriteˉlineˉlowering(
        Verifiedˉmodule tool,
        Verifiedˉmodule memory)
    {
        var Wvb = Compileˉsuccess(WVB_TO_WVO_DIAGNOSTIC_WRITE_LINE_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        var Capability = Module.Module.Capabilities.Single();
        Equal(Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE, Capability.Name);
        Sequenceˉequal([Valueˉtype.Text], Capability.Parameterˉtypes);
        Equal(Valueˉtype.Void, Capability.Returnˉtype);

        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE);
        var Diagnostics = new StringWriter();
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                [],
                TextWriter.Null,
                Diagnostics)),
            new(Authorized)).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        Equal("A\n", Diagnostics.ToString());

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [Nativeˉservice.Diagnosticˉwriteˉline],
            Native.Fragment.Requiredˉservices);
        using var Nativeˉdiagnostic = new Nativeˉoutputˉcapture();
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: new(
                    null,
                    Authorized,
                    diagnosticˉoutput: Nativeˉdiagnostic.Channel)));
        Equal("A\n", Nativeˉdiagnostic.Readˉtext());
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(tool, Wvb);
        if (Toolˉresult.Exitˉcode != 0)
        {
            throw new InvalidOperationException(
                "diagnostic.write_line lowering failed: " + Toolˉresult.Diagnostics);
        }
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);

        var Memoryˉresult = new Referenceˉruntime(
            memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with { Maximumˉinstructions = 100_000_000 })
            .Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);
    }
}
