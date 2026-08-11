using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WVA_ASSEMBLER_WVB_BYTES = 180_071;
    private static void Nativeˉwvaˉassemblerˉruns()
    {
        var Assemblerˉbytes = Compileˉwithˉtoolˉfoundationˉsuccess(
            WVA_ASSEMBLER_CORE_SOURCE,
            "Wva-Assembler-Core.wv");
        Equal(WVA_ASSEMBLER_WVB_BYTES, Assemblerˉbytes.Length);
        Equal(WVA_ASSEMBLER_CORE_SHA256, Moduleˉdigest.Calculateˉsha256(Assemblerˉbytes));
        var Assemblerˉmodule = Moduleˉcodec.Readˉandˉverify(Assemblerˉbytes);
        var Assemblerˉnative = X64ˉnativeˉbackend.Compile(Assemblerˉmodule);
        Nativeˉfragmentˉverifier.Verify(Assemblerˉnative.Fragment);
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
                Nativeˉservice.Fileˉwriteˉbytes,
            ],
            Assemblerˉnative.Fragment.Requiredˉservices);

        var Windowsˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvaˉassembler(
            Assemblerˉnative.Fragment,
            Nativeˉserviceˉplatform.Windows);
        var Linuxˉbundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvaˉassembler(
            Assemblerˉnative.Fragment,
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

        var Windows = Hostedˉwvaˉassemblerˉapplicationˉwriter.Writeˉwindows(
            Assemblerˉnative.Fragment,
            Assemblerˉmodule.Module.Capabilities,
            Assemblerˉmodule.Module.Name);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows WVA assembler writer failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        var Repeatedˉwindows = Hostedˉwvaˉassemblerˉapplicationˉwriter.Writeˉwindows(
            Assemblerˉnative.Fragment,
            Assemblerˉmodule.Module.Capabilities,
            Assemblerˉmodule.Module.Name);
        True(
            Repeatedˉwindows.Success,
            Repeatedˉwindows.Diagnostics.IsEmpty
                ? "The repeated Windows WVA assembler writer failed without a diagnostic."
                : Repeatedˉwindows.Diagnostics[0].Message);
        Sequenceˉequal(Windows.Imageˉbytes, Repeatedˉwindows.Imageˉbytes);
        var Verifiedˉwindows = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Windows.Imageˉbytes.AsSpan(),
            Windowsˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler,
            Verifiedˉwindows.Runtime.Metadata.Profile);

        var Linux = Hostedˉwvaˉassemblerˉapplicationˉwriter.Writeˉlinux(
            Assemblerˉnative.Fragment,
            Assemblerˉmodule.Module.Capabilities,
            Assemblerˉmodule.Module.Name);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux WVA assembler writer failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        var Repeatedˉlinux = Hostedˉwvaˉassemblerˉapplicationˉwriter.Writeˉlinux(
            Assemblerˉnative.Fragment,
            Assemblerˉmodule.Module.Capabilities,
            Assemblerˉmodule.Module.Name);
        True(
            Repeatedˉlinux.Success,
            Repeatedˉlinux.Diagnostics.IsEmpty
                ? "The repeated Linux WVA assembler writer failed without a diagnostic."
                : Repeatedˉlinux.Diagnostics[0].Message);
        Sequenceˉequal(Linux.Imageˉbytes, Repeatedˉlinux.Imageˉbytes);
        var Verifiedˉlinux = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
            Linux.Imageˉbytes.AsSpan(),
            Linuxˉbundle,
            Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler);
        Equal(
            Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler,
            Verifiedˉlinux.Runtime.Metadata.Profile);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wva-assembler-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Wva-Assembler.wvb");
            var Sourceˉpath = Path.Combine(Directoryˉpath, "Hello.wva");
            var Objectˉpath = Path.Combine(Directoryˉpath, "Hello.wvo");
            var Invalidˉsourceˉpath = Path.Combine(Directoryˉpath, "Invalid.wva");
            var Invalidˉobjectˉpath = Path.Combine(Directoryˉpath, "Invalid.wvo");
            File.WriteAllBytes(Moduleˉpath, Assemblerˉbytes);
            File.WriteAllText(Sourceˉpath, HELLO_ASSEMBLY_SOURCE, new System.Text.UTF8Encoding(false));
            File.WriteAllText(Invalidˉsourceˉpath, "not assembly\n", new System.Text.UTF8Encoding(false));

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME;
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

            const string Expectedˉoutput =
                "wvasm 1\n" +
                "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1\n";
            const string Expectedˉinvalidˉdiagnostic =
                "assembly status=WVA1001 object-bytes=0 sections=0 symbols=0 relocations=0 offset=0 line=1 column=1\n";
            if (OperatingSystem.IsWindows())
            {
                var Loadedˉmodules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    Expectedˉoutput,
                    [Sourceˉpath, Objectˉpath],
                    loadedˉmodules: Loadedˉmodules));
                Equal(2, Executeˉwindowsˉapplication(
                    Windows.Imageˉbytes,
                    arguments: [Invalidˉsourceˉpath, Invalidˉobjectˉpath],
                    expectedˉerror: Expectedˉinvalidˉdiagnostic));
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
                    Expectedˉoutput,
                    [Sourceˉpath, Objectˉpath],
                    loadedˉmappings: Loadedˉmappings));
                Equal(2, Executeˉlinuxˉapplication(
                    Linux.Imageˉbytes,
                    arguments: [Invalidˉsourceˉpath, Invalidˉobjectˉpath],
                    expectedˉerror: Expectedˉinvalidˉdiagnostic));
                Equal(0, Loadedˉmappings.Count(Name =>
                    Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            Sequenceˉequal(Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE), File.ReadAllBytes(Objectˉpath));
            False(File.Exists(Invalidˉobjectˉpath), "Rejected WVA input created an output object.");
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
