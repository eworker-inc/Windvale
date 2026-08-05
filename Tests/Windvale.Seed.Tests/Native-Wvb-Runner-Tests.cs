using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string FOUNDATION_SHA256_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Foundation-Sha256.wv");
    private static readonly string FOUNDATION_SHA256_KNOWN_ANSWERS_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Foundation-Sha256-Known-Answers.wv");
    private static readonly string WVB_RUNNER_TOOL_SOURCE =
        Readˉembeddedˉsource("Windvale.Seed.Tests.Wvb-Runner-Tool.wv");

    private static void Nativeˉwvbˉrunnerˉruns()
    {
        var Sha256ˉbytes = Compileˉrunnerˉmodules(
            new("Tests/Fixtures/Foundation/Sha256-Known-Answers.wv",
                FOUNDATION_SHA256_KNOWN_ANSWERS_SOURCE),
            [new("Foundation/Sha256.wv", FOUNDATION_SHA256_SOURCE)]);
        var Sha256ˉmodule = Moduleˉcodec.Readˉandˉverify(Sha256ˉbytes);
        Equal(0, new Referenceˉruntime(
            Sha256ˉmodule,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
        Equal(0, X64ˉnativeˉexecutor.Executeˉi32(
            X64ˉnativeˉbackend.Compile(Sha256ˉmodule).Fragment));

        var Runnerˉbytes = Compileˉrunnerˉmodules(
            new("Tools/Windvale.Run/Wvb-Runner-Tool.wv", WVB_RUNNER_TOOL_SOURCE),
            [
                new("Foundation/Sha256.wv", FOUNDATION_SHA256_SOURCE),
                new("Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Main.wv",
                    WEBASSEMBLY_WVB_SCALAR_INTERPRETER_SOURCE),
            ]);
        var Runnerˉmodule = Moduleˉcodec.Readˉandˉverify(Runnerˉbytes);
        Equal(18, Runnerˉmodule.Module.Functions.Length);

        var I32ˉcodecˉmodule = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(
            "module Nativeˉi32ˉbytes profile portable; " +
            "export fn Main() -> i32 { " +
            "return Bytesˉreadˉi32ˉlittle(Bytesˉfromˉi32ˉlittle(-7), 0u32); }"));
        Equal(-7, X64ˉnativeˉexecutor.Executeˉi32(
            X64ˉnativeˉbackend.Compile(I32ˉcodecˉmodule).Fragment));

        const string Profileˉsource =
            "module Wvbˉrunnerˉprofile profile hosted; " +
            "capability console.write_line; " +
            "capability diagnostic.write_line; " +
            "capability file.read_bytes; " +
            "capability process.argument; " +
            "capability process.argument_count; " +
            "fn Exercise(Path: text) -> i32 { " +
            "let Data: bytes = file.read_bytes(Path); " +
            "let Value: text = Textˉfromˉutf8(Data); " +
            "console.write_line(Textˉconcat(Value, U32ˉformat(Bytesˉlength(Data)))); " +
            "diagnostic.write_line(I32ˉformat(0)); return 0; } " +
            "export fn Main() -> i32 { " +
            "if process.argument_count() == 0u32 { return 64; } " +
            "return Exercise(process.argument(0u32)); }";
        var Profileˉbytes = Compileˉsuccess(Profileˉsource);
        var Profileˉmodule = Moduleˉcodec.Readˉandˉverify(Profileˉbytes);
        var Runnerˉfragment = X64ˉnativeˉbackend.Compile(Profileˉmodule).Fragment;
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.I32ˉformat,
                Nativeˉservice.U32ˉformat,
            ],
            Runnerˉfragment.Requiredˉservices);

        var Windows = Wvbˉrunnerˉapplicationˉwriter.Writeˉwindows(
            Runnerˉfragment,
            Profileˉmodule.Module.Capabilities);
        var Linux = Wvbˉrunnerˉapplicationˉwriter.Writeˉlinux(
            Runnerˉfragment,
            Profileˉmodule.Module.Capabilities);
        True(
            Windows.Success,
            Windows.Diagnostics.IsEmpty
                ? "The Windows WVB runner failed without a diagnostic."
                : Windows.Diagnostics[0].Message);
        True(
            Linux.Success,
            Linux.Diagnostics.IsEmpty
                ? "The Linux WVB runner failed without a diagnostic."
                : Linux.Diagnostics[0].Message);
        var Repository = Findˉrepositoryˉroot();
        var Pinnedˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Front-Door");
        var Pinnedˉwvbˉpath = Path.Combine(Pinnedˉroot, "Wvb", "Wvb-Runner.wvb");
        var Pinnedˉmodule = Moduleˉcodec.Readˉandˉverify(
            File.ReadAllBytes(Pinnedˉwvbˉpath));
        var Pinnedˉfragment = X64ˉnativeˉbackend.Compile(Pinnedˉmodule).Fragment;
        var Pinnedˉwindows = Wvbˉrunnerˉapplicationˉwriter.Writeˉwindows(
            Pinnedˉfragment,
            Pinnedˉmodule.Module.Capabilities);
        var Pinnedˉlinux = Wvbˉrunnerˉapplicationˉwriter.Writeˉlinux(
            Pinnedˉfragment,
            Pinnedˉmodule.Module.Capabilities);
        True(
            Pinnedˉwindows.Success,
            Pinnedˉwindows.Diagnostics.IsEmpty
                ? "The pinned Windows WVB runner failed without a diagnostic."
                : Pinnedˉwindows.Diagnostics[0].Message);
        True(
            Pinnedˉlinux.Success,
            Pinnedˉlinux.Diagnostics.IsEmpty
                ? "The pinned Linux WVB runner failed without a diagnostic."
                : Pinnedˉlinux.Diagnostics[0].Message);
        Sequenceˉequal(
            Pinnedˉwindows.Imageˉbytes,
            File.ReadAllBytes(Path.Combine(Pinnedˉroot, "windows-x64", "wvrun.exe")));
        Sequenceˉequal(
            Pinnedˉlinux.Imageˉbytes,
            File.ReadAllBytes(Path.Combine(Pinnedˉroot, "linux-x64", "wvrun.elf")));

        foreach (var Platform in new[]
        {
            Nativeˉserviceˉplatform.Windows,
            Nativeˉserviceˉplatform.Linux,
        })
        {
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉrunner(
                Runnerˉfragment,
                Platform);
            Equal(9, Bundle.Placements.Length);
            Sequenceˉequal(
                Hostedˉverifierˉapplicationˉmetadata.Requiredˉservices(
                    Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner),
                Bundle.Placements.Select(Placement => Placement.Service));
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-wvb-runner-profile-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉrunnerˉpath = Path.Combine(Directoryˉpath, "Native-Runner.wvb");
            var Nativeˉbuild = Runˉnativeˉwvbˉtool(
                Repository,
                "Build-Wvb",
                Path.Combine(Repository, "Windvale-Wvb-Runner.wvproj"),
                Nativeˉrunnerˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(
                File.ReadAllBytes(Pinnedˉwvbˉpath),
                File.ReadAllBytes(Nativeˉrunnerˉpath));

            var Moduleˉpath = Path.Combine(Directoryˉpath, "Runner.wvb");
            File.WriteAllBytes(Moduleˉpath, Profileˉbytes);
            foreach (var Target in new[]
            {
                Windowsˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME,
                Linuxˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME,
            })
            {
                var Published = Executeˉinspectorˉtool(
                    "aot",
                    Moduleˉpath,
                    "--target",
                    Target);
                Equal(0, Published.Exitˉcode);
                Equal(string.Empty, Published.Standardˉerror);
                Contains(Published.Standardˉoutput, $"Target: {Target}");
                var Expected = Target ==
                    Windowsˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME
                        ? Windows.Imageˉbytes
                        : Linux.Imageˉbytes;
                Sequenceˉequal(
                    Expected,
                    File.ReadAllBytes(Path.ChangeExtension(
                        Moduleˉpath,
                        Windvale.Tool.Program.Targetˉoutputˉextension(Target))));
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }

        if (OperatingSystem.IsWindows())
        {
            Equal(64, Executeˉwindowsˉapplication(Windows.Imageˉbytes));
        }
        if (OperatingSystem.IsLinux())
        {
            Equal(64, Executeˉlinuxˉapplication(Linux.Imageˉbytes));
        }
    }

    private static byte[] Compileˉrunnerˉmodules(
        Sourceˉmoduleˉinput root,
        ImmutableArray<Sourceˉmoduleˉinput> dependencies)
    {
        var Result = Seedˉcompiler.Compileˉmodules(root, dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "WVB runner module compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }
}
