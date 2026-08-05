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

    private const int WVB_TO_WVO_TOOL_WVB_BYTES = 284_359;
    private const int WINDOWS_WVB_TO_WVO_APPLICATION_BYTES = 3_997_184;
    private const string WINDOWS_WVB_TO_WVO_APPLICATION_SHA256 =
        "3556b23a969f841286d2f43526d6c4e38f448a48ab244171afa9771ad56d9e8f";
    private const int LINUX_WVB_TO_WVO_APPLICATION_BYTES = 3_997_696;
    private const string LINUX_WVB_TO_WVO_APPLICATION_SHA256 =
        "b0a5babd6f77834909c4daec4d1d9843cb1a28fef1c01c149952bdf76d2b611b";
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
            var Invalidˉpath = Path.Combine(Directoryˉpath, "Invalid.wvb");
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.wvo");
            File.WriteAllBytes(Toolˉpath, Toolˉbytes);
            File.WriteAllBytes(Inputˉpath, Fixtureˉwvb);
            File.WriteAllBytes(Nominalˉinputˉpath, Nominalˉwvb);
            File.WriteAllBytes(Capabilityˉinputˉpath, Capabilityˉwvb);
            File.WriteAllBytes(Argumentˉinputˉpath, Argumentˉwvb);
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
}
