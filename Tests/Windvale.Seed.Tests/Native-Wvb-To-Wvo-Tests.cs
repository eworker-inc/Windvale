using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WVB_TO_WVO_TOOL_WVB_BYTES = 86_741;
    private const int WINDOWS_WVB_TO_WVO_APPLICATION_BYTES = 1_127_936;
    private const string WINDOWS_WVB_TO_WVO_APPLICATION_SHA256 =
        "74fc450f042d4ef48e77c89ff7ad5f8fbf88dd19b3a9b4bae53106b536957061";
    private const int LINUX_WVB_TO_WVO_APPLICATION_BYTES = 1_126_400;
    private const string LINUX_WVB_TO_WVO_APPLICATION_SHA256 =
        "7bd6c4e0cf5e7cfeb416f3a36386722b9317204c828cc40794da2e87071e4538";
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
        Equal(WINDOWS_WVB_TO_WVO_APPLICATION_BYTES, Windows.Imageˉbytes.Length);
        Equal(
            WINDOWS_WVB_TO_WVO_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
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
        Equal(LINUX_WVB_TO_WVO_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WVB_TO_WVO_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
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
            var Invalidˉpath = Path.Combine(Directoryˉpath, "Invalid.wvb");
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.wvo");
            File.WriteAllBytes(Toolˉpath, Toolˉbytes);
            File.WriteAllBytes(Inputˉpath, Fixtureˉwvb);
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
            }

            Sequenceˉequal(Expectedˉobject, File.ReadAllBytes(Outputˉpath));
            Sequenceˉequal(Expectedˉobject, File.ReadAllBytes(Repeatedˉpath));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Rejectedˉpath));
            _ = Objectˉcodec.Readˉandˉverify(File.ReadAllBytes(Outputˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Compileˉwvbˉtoˉwvoˉtoolˉsuccess()
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new("Compiler/Windvale/Native-X64-Lowering-Tool.wv", NATIVE_X64_LOWERING_TOOL_SOURCE),
            [new("Compiler/Windvale/Native-X64-Lowering-Core.wv", NATIVE_X64_LOWERING_CORE_SOURCE)]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale native x64 tool compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }
}
