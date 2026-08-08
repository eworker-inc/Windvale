using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉfixedˉservicesˉrun()
    {
        var Compiled = Seedˉcompiler.Compileˉmodules(new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Hosted-Fixed-Services-Tool.wv",
            Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Native-Hosted-Fixed-Services-Tool.wv")),
            []);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(Hostedˉfixedˉservicesˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(Hostedˉfixedˉservicesˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native);
        var Windows = Hostedˉfixedˉservicesˉapplicationˉwriter.Writeˉwindows(
            Native, Module.Module.Capabilities, Module.Module.Name);
        var Linux = Hostedˉfixedˉservicesˉapplicationˉwriter.Writeˉlinux(
            Native, Module.Module.Capabilities, Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(Hostedˉfixedˉservicesˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(Hostedˉfixedˉservicesˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Equal(Hostedˉfixedˉservicesˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(Hostedˉfixedˉservicesˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Directoryˉpath = Path.Combine(Path.GetTempPath(),
            $"windvale-hosted-fixed-services-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Repository = Findˉrepositoryˉroot();
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Fixed-Services.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository,
                    "Windvale-Native-Hosted-Fixed-Services-Tool.wvproj"),
                Moduleˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Moduleˉpath));

            var Objectˉpath = Path.Combine(Directoryˉpath, "Fixed-Services.wvo");
            var Nativeˉlower = Runˉnativeˉwvbˉtool(
                Repository, "Lower-Wvb-To-Wvo", Moduleˉpath, Objectˉpath);
            Equal(0, Nativeˉlower.Exitˉcode);
            Equal(
                "native x64 status=Valid abi=22 code-bytes=57344 " +
                    "object-bytes=58340\n",
                Nativeˉlower.Output);
            Equal(string.Empty, Nativeˉlower.Error);
            Sequenceˉequal(Expectedˉobject, File.ReadAllBytes(Objectˉpath));

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉfixedˉservicesˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉfixedˉservicesˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cli = Executeˉinspectorˉtool(
                "aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cli.Exitˉcode);
            Equal(string.Empty, Cli.Standardˉerror);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes : Linux.Imageˉbytes;
            Sequenceˉequal(
                Application,
                File.ReadAllBytes(Path.ChangeExtension(Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Target = OperatingSystem.IsWindows() ? "windows" : "linux";
            var Inputs = Fixedˉserviceˉpaths(Repository, Target);
            var Prefix = Path.Combine(Directoryˉpath, "Source");
            File.WriteAllBytes(Prefix + ".chunk-0", [0xC3]);
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(0, Executeˉhostedˉfixedˉservices(
                Application, Target, Prefix, "1", Inputs,
                "hosted fixed services status=Valid resources=9\n", Loaded));
            int[] Outputˉindices = [1, 2, 3, 4, 5, 6, 8, 9, 10];
            for (var Index = 0; Index < Inputs.Length; Index++)
            {
                Sequenceˉequal(
                    File.ReadAllBytes(Inputs[Index]),
                    File.ReadAllBytes(Prefix + $".chunk-{Outputˉindices[Index]}"));
            }
            False(File.Exists(Prefix + ".chunk-7"),
                "Fixed-service acquisition wrote the variable enum-service slot.");
            Equal(0, Loaded.Count(Name => Name.Contains("clr",
                StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x46, 0x53];
            var Aliasˉinputs = Inputs.ToArray();
            Aliasˉinputs[0] = Prefix + ".chunk-1";
            File.WriteAllBytes(Aliasˉinputs[0], Sentinel);
            Equal(2, Executeˉhostedˉfixedˉservices(
                Application, Target, Prefix, "1", Aliasˉinputs,
                string.Empty, expectedˉerror:
                    "hosted fixed services status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Aliasˉinputs[0]));

            var Duplicateˉinputs = Inputs.ToArray();
            Duplicateˉinputs[5] = Duplicateˉinputs[0];
            File.WriteAllBytes(Prefix + ".chunk-6", Sentinel);
            Equal(2, Executeˉhostedˉfixedˉservices(
                Application, Target, Prefix, "1", Duplicateˉinputs,
                string.Empty, expectedˉerror:
                    "hosted fixed services status=Rejected\n"));
            Sequenceˉequal(Sentinel,
                File.ReadAllBytes(Prefix + ".chunk-6"));

            var Badˉpath = Path.Combine(Directoryˉpath, "Bad.bin");
            File.WriteAllBytes(Badˉpath, []);
            var Badˉinputs = Inputs.ToArray();
            Badˉinputs[1] = Badˉpath;
            File.WriteAllBytes(Prefix + ".chunk-2", Sentinel);
            Equal(2, Executeˉhostedˉfixedˉservices(
                Application, Target, Prefix, "1", Badˉinputs,
                string.Empty, expectedˉerror:
                    "hosted fixed services status=Rejected\n"));
            Sequenceˉequal(Sentinel,
                File.ReadAllBytes(Prefix + ".chunk-2"));
        }
        finally { Directory.Delete(Directoryˉpath, recursive: true); }
    }

    private static string[] Fixedˉserviceˉpaths(
        string repository,
        string target)
    {
        var Consumer = Path.Combine(repository, "Runtime", "Windvale.Native",
            "Consumers");
        var Platform = target == "windows" ? "Windows" : "Linux";
        return
        [
            Path.Combine(Consumer,
                $"Native-X64-{Platform}-Console-Output-Service.bin"),
            Path.Combine(Consumer, "Native-X64-Argument-Count-Service.bin"),
            Path.Combine(Consumer, "Native-X64-Argument-Service.bin"),
            Path.Combine(Consumer,
                $"Native-X64-{Platform}-File-Input-Service.bin"),
            Path.Combine(Consumer, "Native-X64-Utf8-Service.bin"),
            Path.Combine(Consumer,
                $"Native-X64-{Platform}-Diagnostic-Output-Service.bin"),
            Path.Combine(Consumer, "Native-X64-Text-Concat-Service.bin"),
            Path.Combine(Consumer, "Native-X64-U32-Format-Service.bin"),
            Path.Combine(Consumer,
                $"Native-X64-{Platform}-File-Output-Service.bin"),
        ];
    }

    private static int Executeˉhostedˉfixedˉservices(
        ImmutableArray<byte> application,
        string target,
        string prefix,
        string chunks,
        IReadOnlyList<string> inputs,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "")
    {
        var Arguments = new[] { target, prefix, chunks }
            .Concat(inputs)
            .ToArray();
        return OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(application, expectedˉoutput,
                Arguments, loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(application, expectedˉoutput,
                Arguments, loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
    }
}
