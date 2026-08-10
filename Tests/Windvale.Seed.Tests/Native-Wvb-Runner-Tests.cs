using System.Buffers.Binary;
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
    private const int CURRENT_WVB_RUNNER_BYTES = 121_593;
    private const string CURRENT_WVB_RUNNER_SHA256 =
        "5042a57e3281621ee126a64cadef70834800524de60ed0521cedba043bd271f1";
    private const int STAGE0_WVB_RUNNER_BYTES = 126_271;
    private const string STAGE0_WVB_RUNNER_SHA256 =
        "00b87804c047b626b00c167bf99ea9834bc77ab8e88e454d39a738b2787e2bcf";
    private const int CURRENT_WINDOWS_WVB_RUNNER_APPLICATION_BYTES = 1_094_656;
    private const string CURRENT_WINDOWS_WVB_RUNNER_APPLICATION_SHA256 =
        "ab0c2384ecdfd07bc7351562732ae4b1f97e07dcbd2c92e96dc8cb3dee4d3ff7";
    private const int CURRENT_LINUX_WVB_RUNNER_APPLICATION_BYTES = 1_093_632;
    private const string CURRENT_LINUX_WVB_RUNNER_APPLICATION_SHA256 =
        "ffc0ad10e0e1dcffc8344bb040885535f5ab67a50cbebb1980c980888c1b5322";

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
                new("Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Envelope.wv",
                    WEBASSEMBLY_WVB_SCALAR_INTERPRETER_ENVELOPE_SOURCE),
                new("Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Formatting.wv",
                    WEBASSEMBLY_WVB_SCALAR_INTERPRETER_FORMATTING_SOURCE),
            ]);
        var Runnerˉmodule = Moduleˉcodec.Readˉandˉverify(Runnerˉbytes);
        Equal(9, Runnerˉmodule.Module.Functions.Length);
        Equal(STAGE0_WVB_RUNNER_BYTES, Runnerˉbytes.Length);
        Equal(STAGE0_WVB_RUNNER_SHA256, Moduleˉdigest.Calculateˉsha256(Runnerˉbytes));

        var Interpreterˉbytes = Compileˉrunnerˉmodules(
            new(
                "Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Main.wv",
                WEBASSEMBLY_WVB_SCALAR_INTERPRETER_SOURCE),
            [
                new("Foundation/Sha256.wv", FOUNDATION_SHA256_SOURCE),
                new("Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Envelope.wv",
                    WEBASSEMBLY_WVB_SCALAR_INTERPRETER_ENVELOPE_SOURCE),
                new("Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Formatting.wv",
                    WEBASSEMBLY_WVB_SCALAR_INTERPRETER_FORMATTING_SOURCE),
            ]);
        var Interpreterˉmodule = Moduleˉcodec.Readˉandˉverify(Interpreterˉbytes);
        var Heapˉguest = Compileˉsuccess(
            SOURCE_WVB_TEXT_BYTES_INTERPRETER_HEAP_SOURCE);
        var Heapˉrequest = new byte[16 + Heapˉguest.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Heapˉrequest.AsSpan(0, 4),
            0x4958_5657u);
        BinaryPrimitives.WriteUInt16LittleEndian(Heapˉrequest.AsSpan(4, 2), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Heapˉrequest.AsSpan(8, 4), 4_096u);
        BinaryPrimitives.WriteUInt32LittleEndian(Heapˉrequest.AsSpan(12, 4), 8u);
        Heapˉguest.CopyTo(Heapˉrequest, 16);
        var Heapˉresult = new Referenceˉruntime(
            Interpreterˉmodule,
            new Referenceˉcapabilityˉhost(new StringWriter()),
            Runtimeˉoptions.Portableˉdefaults with
            {
                Maximumˉinstructions = 29_535_345,
            }).Runˉmainˉbytes(ImmutableArray.Create(Heapˉrequest));
        Equal(29_535_345L, Heapˉresult.Executedˉinstructions);
        Equal(20, Heapˉresult.Bytes.Length);
        Equal(
            0x4F58_5657u,
            BinaryPrimitives.ReadUInt32LittleEndian(Heapˉresult.Bytes.AsSpan(0, 4)));
        Equal(
            3018u,
            BinaryPrimitives.ReadUInt32LittleEndian(Heapˉresult.Bytes.AsSpan(8, 4)));
        Equal(
            388u,
            BinaryPrimitives.ReadUInt32LittleEndian(Heapˉresult.Bytes.AsSpan(12, 4)));

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
            "console.write_line(Textˉconcat(\"bytes=\", U32ˉformat(Bytesˉlength(Data)))); " +
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
            "Native-Wvb-Runner-Candidate");
        var Pinnedˉwvbˉpath = Path.Combine(Pinnedˉroot, "Wvb-Runner.wvb");
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
        Equal(CURRENT_WINDOWS_WVB_RUNNER_APPLICATION_BYTES, Pinnedˉwindows.Imageˉbytes.Length);
        Equal(
            CURRENT_WINDOWS_WVB_RUNNER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Pinnedˉwindows.Imageˉbytes.AsSpan()));
        Equal(CURRENT_LINUX_WVB_RUNNER_APPLICATION_BYTES, Pinnedˉlinux.Imageˉbytes.Length);
        Equal(
            CURRENT_LINUX_WVB_RUNNER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Pinnedˉlinux.Imageˉbytes.AsSpan()));

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
            Equal(
                "build status=Published verification=compiler-aligned functions=9 " +
                "code-bytes=117395 module-bytes=121593\n" +
                "publication status=Complete bytes=0x0001daf9 " +
                "sha256=5042a57e3281621ee126a64cadef70834800524de60ed0521cedba043bd271f1\n",
                Nativeˉbuild.Output);
            Equal(string.Empty, Nativeˉbuild.Error);
            var Nativeˉrunnerˉbytes = File.ReadAllBytes(Nativeˉrunnerˉpath);
            Equal(CURRENT_WVB_RUNNER_BYTES, Nativeˉrunnerˉbytes.Length);
            Equal(CURRENT_WVB_RUNNER_SHA256, Moduleˉdigest.Calculateˉsha256(Nativeˉrunnerˉbytes));
            Sequenceˉequal(File.ReadAllBytes(Pinnedˉwvbˉpath), Nativeˉrunnerˉbytes);

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
