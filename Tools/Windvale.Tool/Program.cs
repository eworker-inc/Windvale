using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Windvale.Assembler;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Project;
using Windvale.Runtime;

namespace Windvale.Tool;

internal static class Program
{
    private const int EXIT_SUCCESS = 0;
    private const int EXIT_COMPILATION = 1;
    private const int EXIT_VERIFICATION = 2;
    private const int EXIT_RUNTIME = 3;
    private const int EXIT_USAGE = 64;
    private const int EXIT_SOFTWARE = 70;
    private const int EXIT_IO = 74;
    private const int MAX_SOURCE_FILE_BYTES = 16 * 1024 * 1024;
    private const long MAX_SOURCE_SET_FILE_BYTES = 64L * 1024 * 1024;

    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static int Main(string[] arguments)
    {
        try
        {
            if (arguments.Length == 0 || arguments[0] is "help" or "--help" or "-h")
            {
                Writeˉhelp(Console.Out);
                return EXIT_SUCCESS;
            }

            return arguments[0] switch
            {
                "compile" => Compile(arguments[1..]),
                "build" => Build(arguments[1..]),
                "aot" => Aot(arguments[1..]),
                "assemble" => Assemble(arguments[1..]),
                "link" => Link(arguments[1..]),
                "inspect" => Inspect(arguments[1..]),
                "verify" => Verify(arguments[1..]),
                "object-inspect" => Inspectˉobject(arguments[1..]),
                "object-verify" => Verifyˉobject(arguments[1..]),
                "run" => Run(arguments[1..]),
                _ => Usageˉerror($"Unknown command '{arguments[0]}'."),
            };
        }
        catch (Moduleˉformatˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_VERIFICATION;
        }
        catch (Moduleˉverificationˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_VERIFICATION;
        }
        catch (Objectˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_VERIFICATION;
        }
        catch (Runtimeˉexception Exception)
        {
            Console.Error.WriteLine(Exception.Message);
            return EXIT_RUNTIME;
        }
        catch (DecoderFallbackException Exception)
        {
            Console.Error.WriteLine($"Input is not strict UTF-8: {Exception.Message}");
            return EXIT_IO;
        }
        catch (IOException Exception)
        {
            Console.Error.WriteLine($"I/O failed: {Exception.Message}");
            return EXIT_IO;
        }
        catch (UnauthorizedAccessException Exception)
        {
            Console.Error.WriteLine($"I/O access was denied: {Exception.Message}");
            return EXIT_IO;
        }
        catch (Exception Exception)
        {
            Console.Error.WriteLine($"Internal failure: {Exception.Message}");
            return EXIT_SOFTWARE;
        }
    }

    private static int Compile(string[] arguments)
    {
        const string Usage =
            "Usage: windvale compile <source.wv> [--module <dependency.wv>]... " +
            "[--target <wvb|windows-x64-console-v1|linux-x64-console-v1|" +
            "windows-x64-console-v2|linux-x64-console-v2|" +
            "windows-x64-console-v3|linux-x64-console-v3|" +
            "windows-x64-verifier-v1|linux-x64-verifier-v1|" +
            "windows-x64-wvb-inspector-v1|linux-x64-wvb-inspector-v1|" +
            "windows-x64-wvo-inspector-v1|linux-x64-wvo-inspector-v1|" +
            "windows-x64-console-application-verifier-v1|" +
            "linux-x64-console-application-verifier-v1|" +
            "windows-x64-wvb-runner-v1|linux-x64-wvb-runner-v1|" +
            "windows-x64-build-driver-v1|linux-x64-build-driver-v1|" +
            "windows-x64-wva-assembler-v1|linux-x64-wva-assembler-v1|" +
            "windows-x64-wv-linker-v1|linux-x64-wv-linker-v1|" +
            "windows-x64-console-packager-v1|linux-x64-console-packager-v1|" +
            "windows-x64-console-segmented-packager-v1|" +
            "linux-x64-console-segmented-packager-v1|" +
            "windows-x64-wvb-to-wvo-v1|linux-x64-wvb-to-wvo-v1|" +
            "windows-x64-hosted-container-segmenter-v1|" +
            "linux-x64-hosted-container-segmenter-v1|" +
            "windows-x64-hosted-container-planner-v1|" +
            "linux-x64-hosted-container-planner-v1|" +
            "windows-x64-hosted-container-publisher-v1|" +
            "linux-x64-hosted-container-publisher-v1|" +
            "windows-x64-wvb-publisher-v1|linux-x64-wvb-publisher-v1|" +
            "windows-x64-console-application-publisher-v1|" +
            "linux-x64-console-application-publisher-v1|" +
            "windows-x64-wvo-publisher-v1|linux-x64-wvo-publisher-v1>] [-o <artifact>]";
        if (arguments.Length == 0 || arguments[0].StartsWith("-", StringComparison.Ordinal))
        {
            return Usageˉerror(Usage);
        }

        var Sourceˉpath = Path.GetFullPath(arguments[0]);
        var Dependencyˉpaths = new List<string>();
        string? Requestedˉoutputˉpath = null;
        var Target = "wvb";
        var Sawˉtarget = false;
        for (var Index = 1; Index < arguments.Length; Index += 2)
        {
            if (Index + 1 >= arguments.Length)
            {
                return Usageˉerror(Usage);
            }
            if (arguments[Index] == "--module")
            {
                Dependencyˉpaths.Add(Path.GetFullPath(arguments[Index + 1]));
                continue;
            }
            if (arguments[Index] == "-o" && Requestedˉoutputˉpath is null)
            {
                Requestedˉoutputˉpath = Path.GetFullPath(arguments[Index + 1]);
                continue;
            }
            if (arguments[Index] == "--target" && !Sawˉtarget)
            {
                Target = arguments[Index + 1];
                Sawˉtarget = true;
                continue;
            }

            return Usageˉerror($"Unknown, duplicate, or incomplete compile option '{arguments[Index]}'.");
        }

        if (!Isˉcompileˉtarget(Target))
        {
            return Usageˉerror($"Unknown compile target '{Target}'.");
        }

        var Outputˉpath = Requestedˉoutputˉpath ?? Path.ChangeExtension(
            Sourceˉpath,
            Targetˉoutputˉextension(Target));
        return Compileˉsourceˉfiles(Sourceˉpath, Dependencyˉpaths, Outputˉpath, Target);
    }

    internal static string Targetˉoutputˉextension(string target) => target switch
    {
        "wvb" => ".wvb",
        Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME => ".exe",
        Windowsˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME => ".exe",
        Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        Wvbˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        Consoleˉapplicationˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        Wvoˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        Hostedˉcontainerˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME => ".exe",
        _ => ".elf",
    };

    private static int Build(string[] arguments)
    {
        const string Usage = "Usage: windvale build <project.wvproj> [-o <module.wvb>]";
        if (arguments.Length is not (1 or 3) ||
            arguments[0].StartsWith("-", StringComparison.Ordinal) ||
            (arguments.Length == 3 && arguments[1] != "-o"))
        {
            return Usageˉerror(Usage);
        }

        var Manifestˉpath = Path.GetFullPath(arguments[0]);
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Manifestˉpath), ".wvproj"))
        {
            return Usageˉerror("The build input must use the .wvproj project extension.");
        }

        var Outputˉpath = arguments.Length == 3
            ? Path.GetFullPath(arguments[2])
            : Path.ChangeExtension(Manifestˉpath, ".wvb");
        var Project = Projectˉreader.Read(Manifestˉpath);
        if (!Project.Success)
        {
            foreach (var Diagnostic in Project.Diagnostics)
            {
                Console.Error.WriteLine(
                    $"{Manifestˉpath}({Diagnostic.Line},{Diagnostic.Column}): " +
                    $"error {Diagnostic.Code} [project]: {Diagnostic.Message}");
            }
            return EXIT_COMPILATION;
        }

        var Plan = Project.Plan!;
        if (Plan.Emission != Projectˉemissionˉkind.Wvb)
        {
            Console.Error.WriteLine("The project emission kind is not supported by this tool.");
            return EXIT_COMPILATION;
        }

        return Compileˉsourceˉfiles(Plan.Rootˉpath, Plan.Sourceˉpaths, Outputˉpath, "wvb");
    }

    private static int Aot(string[] arguments)
    {
        const string Usage =
            "Usage: windvale aot <module.wvb> " +
            "--target <windows-x64-console-v1|linux-x64-console-v1|" +
            "windows-x64-console-v2|linux-x64-console-v2|" +
            "windows-x64-console-v3|linux-x64-console-v3|" +
            "windows-x64-verifier-v1|linux-x64-verifier-v1|" +
            "windows-x64-wvb-inspector-v1|linux-x64-wvb-inspector-v1|" +
            "windows-x64-wvo-inspector-v1|linux-x64-wvo-inspector-v1|" +
            "windows-x64-console-application-verifier-v1|" +
            "linux-x64-console-application-verifier-v1|" +
            "windows-x64-wvb-runner-v1|linux-x64-wvb-runner-v1|" +
            "windows-x64-build-driver-v1|linux-x64-build-driver-v1|" +
            "windows-x64-wva-assembler-v1|linux-x64-wva-assembler-v1|" +
            "windows-x64-wv-linker-v1|linux-x64-wv-linker-v1|" +
            "windows-x64-console-packager-v1|linux-x64-console-packager-v1|" +
            "windows-x64-console-segmented-packager-v1|" +
            "linux-x64-console-segmented-packager-v1|" +
            "windows-x64-wvb-to-wvo-v1|linux-x64-wvb-to-wvo-v1|" +
            "windows-x64-hosted-container-segmenter-v1|" +
            "linux-x64-hosted-container-segmenter-v1|" +
            "windows-x64-hosted-container-planner-v1|" +
            "linux-x64-hosted-container-planner-v1|" +
            "windows-x64-hosted-container-publisher-v1|" +
            "linux-x64-hosted-container-publisher-v1|" +
            "windows-x64-wvb-publisher-v1|linux-x64-wvb-publisher-v1|" +
            "windows-x64-console-application-publisher-v1|" +
            "linux-x64-console-application-publisher-v1|" +
            "windows-x64-wvo-publisher-v1|linux-x64-wvo-publisher-v1> [-o <artifact>]";
        if (arguments.Length is not (3 or 5) ||
            arguments[0].StartsWith("-", StringComparison.Ordinal))
        {
            return Usageˉerror(Usage);
        }

        var Moduleˉpath = Path.GetFullPath(arguments[0]);
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Moduleˉpath), ".wvb"))
        {
            return Usageˉerror("The AOT input must use the .wvb module extension.");
        }

        string? Requestedˉoutputˉpath = null;
        string? Target = null;
        for (var Index = 1; Index < arguments.Length; Index += 2)
        {
            if (arguments[Index] == "-o" && Requestedˉoutputˉpath is null)
            {
                Requestedˉoutputˉpath = Path.GetFullPath(arguments[Index + 1]);
                continue;
            }
            if (arguments[Index] == "--target" && Target is null)
            {
                Target = arguments[Index + 1];
                continue;
            }

            return Usageˉerror($"Unknown, duplicate, or incomplete AOT option '{arguments[Index]}'.");
        }
        if (Target is null || Target == "wvb" || !Isˉcompileˉtarget(Target))
        {
            return Usageˉerror($"Unknown or missing AOT target '{Target}'.");
        }

        var Outputˉpath = Requestedˉoutputˉpath ?? Path.ChangeExtension(
            Moduleˉpath,
            Targetˉoutputˉextension(Target));
        var Expectedˉextension = Targetˉoutputˉextension(Target);
        if (!StringComparer.OrdinalIgnoreCase.Equals(
            Path.GetExtension(Outputˉpath),
            Expectedˉextension))
        {
            return Usageˉerror(
                $"The {Target} AOT output must use the {Expectedˉextension} extension.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        if (Pathˉcomparer.Equals(Moduleˉpath, Outputˉpath))
        {
            return Usageˉerror("The AOT output path must differ from its module input.");
        }

        return Writeˉcompiledˉartifact(
            Moduleˉpath,
            Outputˉpath,
            Target,
            Readˉmoduleˉbytes(Moduleˉpath));
    }

    private static bool Isˉcompileˉtarget(string target) => target is
        "wvb" or
        Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME or
        Windowsˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME or
        Linuxˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME or
        Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_TARGET_NAME or
        Wvbˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Wvbˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
        Consoleˉapplicationˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Consoleˉapplicationˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
        Wvoˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Wvoˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
        Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_TARGET_NAME or
        Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
        Hostedˉcontainerˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
        Hostedˉcontainerˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME;

    private static int Compileˉsourceˉfiles(
        string sourceˉpath,
        IReadOnlyList<string> dependencyˉpaths,
        string outputˉpath,
        string target)
    {
        var Sourceˉpath = Path.GetFullPath(sourceˉpath);
        var Dependencyˉpaths = dependencyˉpaths.Select(Path.GetFullPath).ToList();
        var Outputˉpath = Path.GetFullPath(outputˉpath);
        if (Dependencyˉpaths.Count >= Seedˉcompiler.MAX_SOURCE_MODULES)
        {
            Console.Error.WriteLine(
                $"A compilation may contain at most {Seedˉcompiler.MAX_SOURCE_MODULES} source modules.");
            return EXIT_COMPILATION;
        }
        var Expectedˉextension = Targetˉoutputˉextension(target);
        if (!StringComparer.OrdinalIgnoreCase.Equals(
            Path.GetExtension(Outputˉpath),
            Expectedˉextension))
        {
            return Usageˉerror(
                $"The {target} compile output must use the {Expectedˉextension} extension.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var Sourceˉpaths = new List<string>(Dependencyˉpaths.Count + 1) { Sourceˉpath };
        Sourceˉpaths.AddRange(Dependencyˉpaths);
        var Uniqueˉsourceˉpaths = new HashSet<string>(Pathˉcomparer);
        foreach (var Path in Sourceˉpaths)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(System.IO.Path.GetExtension(Path), ".wv"))
            {
                return Usageˉerror("Every compile input must use the .wv source extension.");
            }
            if (!Uniqueˉsourceˉpaths.Add(Path))
            {
                return Usageˉerror($"The compile source path is supplied more than once: {Path}");
            }
            if (Pathˉcomparer.Equals(Path, Outputˉpath))
            {
                return Usageˉerror("The compile output path must differ from every source path.");
            }
        }

        var Sourceˉinputs = new List<Sourceˉmoduleˉinput>(Sourceˉpaths.Count);
        long Totalˉsourceˉbytes = 0;
        foreach (var Path in Sourceˉpaths)
        {
            var Sourceˉlength = new FileInfo(Path).Length;
            if (Sourceˉlength > MAX_SOURCE_FILE_BYTES)
            {
                Console.Error.WriteLine(
                    $"Source file '{Path}' exceeds the {MAX_SOURCE_FILE_BYTES} byte input limit.");
                return EXIT_COMPILATION;
            }
            Totalˉsourceˉbytes += Sourceˉlength;
            if (Totalˉsourceˉbytes > MAX_SOURCE_SET_FILE_BYTES)
            {
                Console.Error.WriteLine(
                    $"The source-file set exceeds the {MAX_SOURCE_SET_FILE_BYTES} byte input limit.");
                return EXIT_COMPILATION;
            }
            Sourceˉinputs.Add(new(Path, File.ReadAllText(Path, STRICT_UTF8)));
        }

        var Result = Seedˉcompiler.Compileˉmodules(Sourceˉinputs[0], Sourceˉinputs.Skip(1).ToArray());
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                var Diagnosticˉsource = string.IsNullOrEmpty(Diagnostic.Span.Sourceˉname)
                    ? Sourceˉpath
                    : Diagnostic.Span.Sourceˉname;
                Console.Error.WriteLine(
                    $"{Diagnosticˉsource}({Diagnostic.Span.Line},{Diagnostic.Span.Column}): " +
                    $"error {Diagnostic.Code} [{Diagnostic.Phase}]: {Diagnostic.Message}");
            }

            return EXIT_COMPILATION;
        }

        return Writeˉcompiledˉartifact(
            Sourceˉpath,
            Outputˉpath,
            target,
            Result.Moduleˉbytes.ToArray());
    }

    private static int Writeˉcompiledˉartifact(
        string diagnosticˉpath,
        string outputˉpath,
        string target,
        byte[] moduleˉbytes)
    {
        var Bytes = moduleˉbytes;
        if (target != "wvb")
        {
            Nativeˉfragment Fragment;
            ImmutableArray<Capabilityˉdeclaration> Capabilities;
            string Moduleˉname;
            Verifiedˉmodule Module;
            try
            {
                Module = Moduleˉcodec.Readˉandˉverify(Bytes);
                Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
                Capabilities = Module.Module.Capabilities;
                Moduleˉname = Module.Module.Name;
            }
            catch (Nativeˉbackendˉexception Exception)
            {
                Console.Error.WriteLine(
                    $"{diagnosticˉpath}: error {Exception.Code} " +
                    $"[native compiler]: {Exception.Message}");
                return EXIT_COMPILATION;
            }

            if (target is Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME or
                Windowsˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME or
                Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_TARGET_NAME or
                Wvbˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
                Consoleˉapplicationˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
                Wvoˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
                Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME or
                Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME or
                Hostedˉcontainerˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME)
            {
                var Application = target switch
                {
                    Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME =>
                        Windowsˉconsoleˉapplicationˉwriter.Write(Fragment),
                    Windowsˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME =>
                        Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉconsole(Fragment),
                    Windowsˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME =>
                        Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉverifier(
                            Fragment,
                            Capabilities),
                    Windowsˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME =>
                        Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉinspector(
                            Fragment,
                            Capabilities),
                    Windowsˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME =>
                        Wvoˉinspectorˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME =>
                        Consoleˉapplicationˉverifierˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME =>
                        Wvbˉrunnerˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities),
                    Windowsˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME =>
                        Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉbuildˉdriver(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME =>
                        Hostedˉwvaˉassemblerˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME =>
                        Hostedˉwvˉlinkerˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME =>
                        Hostedˉconsoleˉpackagerˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME =>
                        Hostedˉconsoleˉsegmentedˉpackagerˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME =>
                        Hostedˉwvbˉtoˉwvoˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Windowsˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME =>
                        Hostedˉcontainerˉsegmenterˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Hostedˉcontainerˉplannerˉapplicationˉwriter.Writeˉwindows(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Wvbˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Wvbˉpublisherˉapplicationˉwriter.Writeˉwindows(
                            Module,
                            Fragment,
                            Bytes),
                    Consoleˉapplicationˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Consoleˉapplicationˉpublisherˉapplicationˉwriter.Writeˉwindows(
                            Module,
                            Fragment,
                            Bytes),
                    Wvoˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Wvoˉpublisherˉapplicationˉwriter.Writeˉwindows(
                            Module,
                            Fragment,
                            Bytes),
                    Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Wvoˉstagingˉproducerˉapplicationˉwriter.Writeˉwindows(
                            Module,
                            Fragment,
                            Bytes),
                    Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Wvoˉstagingˉpublisherˉapplicationˉwriter.Writeˉwindows(
                            Module,
                            Fragment,
                            Bytes),
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME =>
                        Hostedˉcontainerˉpublisherˉapplicationˉwriter.Writeˉwindows(
                            Module,
                            Fragment,
                            Bytes),
                    _ => Windowsˉconsoleˉapplicationˉwriter.Writeˉhostedˉcompiler(
                        Fragment,
                        Capabilities),
                };
                if (!Application.Success)
                {
                    foreach (var Diagnostic in Application.Diagnostics)
                    {
                        Console.Error.WriteLine(
                            $"{diagnosticˉpath}: error {Diagnostic.Code} " +
                            $"[{target}]: {Diagnostic.Message}");
                    }
                    return EXIT_COMPILATION;
                }
                Bytes = Application.Imageˉbytes.ToArray();
            }
            else
            {
                var Application = target switch
                {
                    Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME =>
                        Linuxˉconsoleˉapplicationˉwriter.Write(Fragment),
                    Linuxˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME =>
                        Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉconsole(Fragment),
                    Linuxˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME =>
                        Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉverifier(
                            Fragment,
                            Capabilities),
                    Linuxˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME =>
                        Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉinspector(
                            Fragment,
                            Capabilities),
                    Linuxˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME =>
                        Wvoˉinspectorˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME =>
                        Consoleˉapplicationˉverifierˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME =>
                        Wvbˉrunnerˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities),
                    Linuxˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME =>
                        Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉbuildˉdriver(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME =>
                        Hostedˉwvaˉassemblerˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME =>
                        Hostedˉwvˉlinkerˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME =>
                        Hostedˉconsoleˉpackagerˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME =>
                        Hostedˉconsoleˉsegmentedˉpackagerˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME =>
                        Hostedˉwvbˉtoˉwvoˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Linuxˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME =>
                        Hostedˉcontainerˉsegmenterˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Hostedˉcontainerˉplannerˉapplicationˉwriter.Writeˉlinux(
                            Fragment,
                            Capabilities,
                            Moduleˉname),
                    Wvbˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Wvbˉpublisherˉapplicationˉwriter.Writeˉlinux(
                            Module,
                            Fragment,
                            Bytes),
                    Consoleˉapplicationˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Consoleˉapplicationˉpublisherˉapplicationˉwriter.Writeˉlinux(
                            Module,
                            Fragment,
                            Bytes),
                    Wvoˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Wvoˉpublisherˉapplicationˉwriter.Writeˉlinux(
                            Module,
                            Fragment,
                            Bytes),
                    Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Wvoˉstagingˉproducerˉapplicationˉwriter.Writeˉlinux(
                            Module,
                            Fragment,
                            Bytes),
                    Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Wvoˉstagingˉpublisherˉapplicationˉwriter.Writeˉlinux(
                            Module,
                            Fragment,
                            Bytes),
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME =>
                        Hostedˉcontainerˉpublisherˉapplicationˉwriter.Writeˉlinux(
                            Module,
                            Fragment,
                            Bytes),
                    _ => Linuxˉconsoleˉapplicationˉwriter.Writeˉhostedˉcompiler(
                        Fragment,
                        Capabilities),
                };
                if (!Application.Success)
                {
                    foreach (var Diagnostic in Application.Diagnostics)
                    {
                        Console.Error.WriteLine(
                            $"{diagnosticˉpath}: error {Diagnostic.Code} " +
                            $"[{target}]: {Diagnostic.Message}");
                    }
                    return EXIT_COMPILATION;
                }
                Bytes = Application.Imageˉbytes.ToArray();
            }
        }

        if (target != "wvb")
        {
            Action<string>? Prepareˉtemporary = null;
            if ((target is Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.HOSTED_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.COMPILER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.VERIFIER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.INSPECTOR_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.WVO_INSPECTOR_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.CONSOLE_APPLICATION_VERIFIER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.WVB_RUNNER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.BUILD_DRIVER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.WVA_ASSEMBLER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.WV_LINKER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.CONSOLE_PACKAGER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.CONSOLE_SEGMENTED_PACKAGER_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.WVB_TO_WVO_TARGET_NAME or
                    Linuxˉconsoleˉapplicationˉcontract.HOSTED_CONTAINER_SEGMENTER_TARGET_NAME or
                    Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_TARGET_NAME or
                    Wvbˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
                    Consoleˉapplicationˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
                    Wvoˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
                    Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_TARGET_NAME or
                    Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME or
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME) &&
                OperatingSystem.IsLinux())
            {
                Prepareˉtemporary = Prepareˉlinuxˉexecutable;
            }
            Atomicˉfileˉpublisher.Publish(outputˉpath, Bytes, Prepareˉtemporary);
        }
        else
        {
            File.WriteAllBytes(outputˉpath, Bytes);
        }
        Console.WriteLine($"Compiled: {outputˉpath}");
        if (target != "wvb")
        {
            Console.WriteLine($"Target: {target}");
        }
        Console.WriteLine(
            $"SHA-256: {Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant()}");
        return EXIT_SUCCESS;
    }

    private static void Prepareˉlinuxˉexecutable(string path)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux executable mode requires Linux.");
        }
        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute |
                UnixFileMode.GroupRead |
                UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead |
                UnixFileMode.OtherExecute);
    }

    private static int Inspect(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale inspect <module.wvb>");
        }

        var Bytes = Readˉmoduleˉbytes(arguments[0]);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Console.Write(Moduleˉinspector.Inspect(Module, Bytes));
        return EXIT_SUCCESS;
    }

    private static int Assemble(string[] arguments)
    {
        if (arguments.Length is not (1 or 3) ||
            (arguments.Length == 3 && arguments[1] != "-o"))
        {
            return Usageˉerror("Usage: windvale assemble <source.wva> [-o <object.wvo>]");
        }

        var Sourceˉpath = Path.GetFullPath(arguments[0]);
        var Outputˉpath = arguments.Length == 3
            ? Path.GetFullPath(arguments[2])
            : Path.ChangeExtension(Sourceˉpath, ".wvo");
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Sourceˉpath), ".wva"))
        {
            return Usageˉerror("The assemble input must use the .wva source extension.");
        }
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Outputˉpath), ".wvo"))
        {
            return Usageˉerror("The assemble output must use the .wvo object extension.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        if (Pathˉcomparer.Equals(Sourceˉpath, Outputˉpath))
        {
            return Usageˉerror("The assemble output path must differ from the source path.");
        }
        if (new FileInfo(Sourceˉpath).Length > Assemblyˉlimits.MAX_SOURCE_BYTES)
        {
            Console.Error.WriteLine(
                $"Assembly source exceeds the {Assemblyˉlimits.MAX_SOURCE_BYTES} byte input limit.");
            return EXIT_COMPILATION;
        }

        var Source = File.ReadAllText(Sourceˉpath, STRICT_UTF8);
        var Result = Assemblyˉcompiler.Assemble(Source);
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                Console.Error.WriteLine(
                    $"{Sourceˉpath}({Diagnostic.Line},{Diagnostic.Column}): " +
                    $"error {Diagnostic.Code} [assembler]: {Diagnostic.Message}");
            }
            return EXIT_COMPILATION;
        }

        File.WriteAllBytes(Outputˉpath, Result.Objectˉbytes.AsSpan());
        Console.WriteLine($"Assembled: {Outputˉpath}");
        Console.WriteLine($"SHA-256: {Objectˉdigest.Calculateˉsha256(Result.Objectˉbytes.AsSpan())}");
        return EXIT_SUCCESS;
    }

    private static int Verify(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale verify <module.wvb>");
        }

        var Bytes = Readˉmoduleˉbytes(arguments[0]);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        Console.WriteLine($"Verified: {Module.Module.Name}");
        Console.WriteLine($"SHA-256: {Moduleˉdigest.Calculateˉsha256(Bytes)}");
        return EXIT_SUCCESS;
    }

    private static int Link(string[] arguments)
    {
        if (arguments.Length < 7 ||
            arguments[0] != "--base-address" ||
            arguments[2] != "--entry" ||
            arguments[4] != "-o" ||
            !uint.TryParse(
                arguments[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var Baseˉaddress))
        {
            return Usageˉerror(
                "Usage: windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...");
        }

        var Entryˉsymbol = arguments[3];
        var Outputˉpath = Path.GetFullPath(arguments[5]);
        var Inputˉpaths = arguments[6..].Select(Path.GetFullPath).ToArray();
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Outputˉpath), ".bin"))
        {
            return Usageˉerror("The link output must use the .bin flat-image extension.");
        }
        if (Inputˉpaths.Any(Inputˉpath =>
            !StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(Inputˉpath), ".wvo")))
        {
            return Usageˉerror("Every link input must use the .wvo object extension.");
        }
        if (Inputˉpaths.Length > Linkˉlimits.MAX_INPUT_OBJECTS)
        {
            return Usageˉerror($"A link accepts at most {Linkˉlimits.MAX_INPUT_OBJECTS} object inputs.");
        }

        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        if (Inputˉpaths.Any(Inputˉpath => Pathˉcomparer.Equals(Inputˉpath, Outputˉpath)))
        {
            return Usageˉerror("The link output path must differ from every input path.");
        }

        var Inputs = ImmutableArray.CreateBuilder<Linkˉinput>(Inputˉpaths.Length);
        for (var Inputˉindex = 0; Inputˉindex < Inputˉpaths.Length; Inputˉindex++)
        {
            var Inputˉpath = Inputˉpaths[Inputˉindex];
            if (new FileInfo(Inputˉpath).Length > Objectˉlimits.MAX_OBJECT_BYTES)
            {
                Console.Error.WriteLine(
                    $"input[{Inputˉindex}]: error WVL1002 [linker]: " +
                    $"The input object exceeds {Objectˉlimits.MAX_OBJECT_BYTES} bytes.");
                return EXIT_COMPILATION;
            }
            Inputs.Add(new(File.ReadAllBytes(Inputˉpath).ToImmutableArray()));
        }
        var Result = Linkˉcompiler.Link(Inputs.ToImmutable(), new(Baseˉaddress, Entryˉsymbol));
        if (!Result.Success)
        {
            foreach (var Diagnostic in Result.Diagnostics)
            {
                var Scope = Diagnostic.Inputˉindex < 0
                    ? "link"
                    : $"input[{Diagnostic.Inputˉindex}]";
                Console.Error.WriteLine(
                    $"{Scope}: error {Diagnostic.Code} [linker]: {Diagnostic.Message}");
            }
            return EXIT_COMPILATION;
        }

        File.WriteAllBytes(Outputˉpath, Result.Imageˉbytes.AsSpan());
        Console.Write(STRICT_UTF8.GetString(Result.Mapˉbytes.AsSpan()));
        return EXIT_SUCCESS;
    }

    private static int Run(string[] arguments)
    {
        if (arguments.Length == 0)
        {
            return Usageˉerror(
                "Usage: windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>] " +
                "[--bind-read-only-directory <path>] " +
                "[--bind-random-access-storage <path>] " +
                "[--report-steps] [--report-function-steps] [--report-function-record-fields] " +
                "[--report-function-dynamic-values] [--report-dynamic-lifetime] " +
                "[--report-dynamic-allocator] " +
                "[-- <argument>...]");
        }

        var Moduleˉpath = arguments[0];
        var Authorized = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        var Programˉarguments = ImmutableArray.CreateBuilder<string>();
        string? Readˉonlyˉdirectoryˉpath = null;
        string? Randomˉaccessˉstorageˉpath = null;
        long Maximumˉsteps = 1_000_000;
        var Reportˉsteps = false;
        var Reportˉfunctionˉsteps = false;
        var Reportˉfunctionˉrecordˉfields = false;
        var Reportˉfunctionˉdynamicˉvalues = false;
        var Reportˉdynamicˉlifetime = false;
        var Reportˉdynamicˉallocator = false;
        for (var Index = 1; Index < arguments.Length; Index++)
        {
            switch (arguments[Index])
            {
                case "--allow" when Index + 1 < arguments.Length:
                    var Capability = arguments[++Index];
                    if (!Capabilityˉcatalog.Tryˉget(Capability, out _))
                    {
                        return Usageˉerror($"Unknown Seed capability '{Capability}'.");
                    }

                    Authorized.Add(Capability);
                    break;
                case "--max-steps" when Index + 1 < arguments.Length:
                    if (!long.TryParse(arguments[++Index], out Maximumˉsteps) || Maximumˉsteps <= 0)
                    {
                        return Usageˉerror("--max-steps requires a positive integer.");
                    }

                    break;
                case "--bind-read-only-directory" when
                    Index + 1 < arguments.Length && Readˉonlyˉdirectoryˉpath is null:
                    Readˉonlyˉdirectoryˉpath = arguments[++Index];
                    break;
                case "--bind-random-access-storage" when
                    Index + 1 < arguments.Length && Randomˉaccessˉstorageˉpath is null:
                    Randomˉaccessˉstorageˉpath = arguments[++Index];
                    break;
                case "--report-steps":
                    Reportˉsteps = true;
                    break;
                case "--report-function-steps":
                    Reportˉfunctionˉsteps = true;
                    break;
                case "--report-function-record-fields":
                    Reportˉfunctionˉrecordˉfields = true;
                    break;
                case "--report-function-dynamic-values":
                    Reportˉfunctionˉdynamicˉvalues = true;
                    break;
                case "--report-dynamic-lifetime":
                    Reportˉdynamicˉlifetime = true;
                    break;
                case "--report-dynamic-allocator":
                    Reportˉdynamicˉallocator = true;
                    break;
                case "--":
                    Programˉarguments.AddRange(arguments[(Index + 1)..]);
                    Index = arguments.Length;
                    break;
                default:
                    return Usageˉerror($"Unknown or incomplete run option '{arguments[Index]}'.");
            }
        }

        var Bytes = Readˉmoduleˉbytes(Moduleˉpath);
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Readˉonlyˉdirectory = Readˉonlyˉdirectoryˉpath is null
            ? null
            : new Nativeˉreadˉonlyˉdirectory(Readˉonlyˉdirectoryˉpath);
        using var Randomˉaccessˉstorage = Randomˉaccessˉstorageˉpath is null
            ? null
            : new Nativeˉrandomˉaccessˉstorage(Randomˉaccessˉstorageˉpath);
        var Runtime = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                Programˉarguments.ToImmutable(),
                Console.Out,
                Console.Error,
                new Nativeˉhostedˉfileˉreader(),
                new Nativeˉhostedˉfileˉwriter(),
                Readˉonlyˉdirectory,
                Randomˉaccessˉstorage)),
            new(
                Authorized.ToImmutable(),
                Maximumˉsteps,
                Collectˉfunctionˉsteps: Reportˉfunctionˉsteps,
                Collectˉfunctionˉrecordˉfields: Reportˉfunctionˉrecordˉfields,
                Collectˉfunctionˉdynamicˉvalues: Reportˉfunctionˉdynamicˉvalues,
                Collectˉdynamicˉvalueˉlifetime: Reportˉdynamicˉlifetime,
                Collectˉdynamicˉallocatorˉtrace: Reportˉdynamicˉallocator));
        Runtimeˉresult Result;
        try
        {
            Result = Runtime.Runˉmain();
        }
        finally
        {
            if (Reportˉfunctionˉsteps)
            {
                foreach (var Function in Runtime.Readˉfunctionˉsteps())
                {
                    Console.Error.WriteLine(
                        $"Function instructions={Function.Executedˉinstructions} " +
                        $"index={Function.Functionˉindex} name={Function.Functionˉname}");
                }
            }
            if (Reportˉfunctionˉrecordˉfields)
            {
                foreach (var Function in Runtime.Readˉfunctionˉrecordˉfields())
                {
                    Console.Error.WriteLine(
                        $"Function record-fields={Function.Constructedˉfields} " +
                        $"index={Function.Functionˉindex} name={Function.Functionˉname}");
                }
            }
            if (Reportˉfunctionˉdynamicˉvalues)
            {
                foreach (var Function in Runtime.Readˉfunctionˉdynamicˉvalues())
                {
                    Console.Error.WriteLine(
                        $"Function dynamic-bytes={Function.Constructedˉbytes} " +
                        $"values={Function.Constructedˉvalues} " +
                        $"kind={Dynamicˉvalueˉkindˉname(Function.Kind)} " +
                        $"index={Function.Functionˉindex} name={Function.Functionˉname}");
                }
            }
            if (Reportˉdynamicˉlifetime)
            {
                var Lifetime = Runtime.Readˉdynamicˉvalueˉlifetime()!;
                var Peakˉkind = Lifetime.Peakˉoperationˉkind is { } Kind
                    ? Dynamicˉvalueˉkindˉname(Kind)
                    : "none";
                Console.Error.WriteLine(
                    $"Dynamic lifetime constructed-bytes={Lifetime.Constructedˉbytes} " +
                    $"constructed-values={Lifetime.Constructedˉvalues} " +
                    $"peak-live-bytes={Lifetime.Peakˉliveˉbytes} " +
                    $"peak-live-values={Lifetime.Peakˉliveˉvalues} " +
                    $"peak-operation-bytes={Lifetime.Peakˉoperationˉbytes} " +
                    $"peak-operation-values={Lifetime.Peakˉoperationˉvalues} " +
                    $"retained-bytes={Lifetime.Retainedˉbytes} " +
                    $"retained-values={Lifetime.Retainedˉvalues} " +
                    $"kind={Peakˉkind} " +
                    $"index={Lifetime.Peakˉoperationˉfunctionˉindex} " +
                    $"name={Lifetime.Peakˉoperationˉfunctionˉname ?? "none"}");
            }
            if (Reportˉdynamicˉallocator)
            {
                var Allocator = Runtime.Readˉdynamicˉallocatorˉtrace()!;
                Console.Error.WriteLine(
                    $"Dynamic allocator arena-bytes={Allocator.Arenaˉbytes} " +
                    $"header-bytes={Allocator.Headerˉbytes} " +
                    $"alignment-bytes={Allocator.Alignmentˉbytes} " +
                    $"allocations={Allocator.Allocations} " +
                    $"reused={Allocator.Reusedˉallocations} " +
                    $"peak-payload-bytes={Allocator.Peakˉpayloadˉbytes} " +
                    $"peak-charged-bytes={Allocator.Peakˉchargedˉbytes} " +
                    $"peak-blocks={Allocator.Peakˉblocks} " +
                    $"maximum-addressed-bytes={Allocator.Maximumˉaddressedˉbytes} " +
                    $"peak-fragmentation-bytes={Allocator.Peakˉexternalˉfragmentationˉbytes} " +
                    $"maximum-free-spans={Allocator.Maximumˉfreeˉspans} " +
                    $"failed={Allocator.Failedˉallocations} " +
                    $"first-failure-payload-bytes={Allocator.Firstˉfailureˉpayloadˉbytes} " +
                    $"first-failure-charged-bytes={Allocator.Firstˉfailureˉchargedˉbytes} " +
                    $"first-failure-largest-free-span-bytes=" +
                    $"{Allocator.Firstˉfailureˉlargestˉfreeˉspanˉbytes} " +
                    $"retained-blocks={Allocator.Retainedˉblocks} " +
                    $"retained-charged-bytes={Allocator.Retainedˉchargedˉbytes}");
            }
        }
        Console.WriteLine($"Result: {Result.Exitˉcode}");
        if (Reportˉsteps)
        {
            Console.WriteLine($"Instructions: {Result.Executedˉinstructions}");
        }
        return EXIT_SUCCESS;
    }

    private static string Dynamicˉvalueˉkindˉname(Runtimeˉdynamicˉvalueˉkind kind) =>
        kind switch
        {
            Runtimeˉdynamicˉvalueˉkind.Enumˉname => "enum.name",
            Runtimeˉdynamicˉvalueˉkind.I32ˉformat => "i32.format",
            Runtimeˉdynamicˉvalueˉkind.U8ˉformat => "u8.format",
            Runtimeˉdynamicˉvalueˉkind.U32ˉformat => "u32.format",
            Runtimeˉdynamicˉvalueˉkind.Textˉconcat => "text.concat",
            Runtimeˉdynamicˉvalueˉkind.Textˉquote => "text.quote",
            Runtimeˉdynamicˉvalueˉkind.Bytesˉconcat => "bytes.concat",
            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉu8 => "bytes.from_u8",
            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉu16ˉlittle => "bytes.from_u16_little",
            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉu32ˉlittle => "bytes.from_u32_little",
            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉi32ˉlittle => "bytes.from_i32_little",
            Runtimeˉdynamicˉvalueˉkind.I64ˉformat => "i64.format",
            Runtimeˉdynamicˉvalueˉkind.U64ˉformat => "u64.format",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown dynamic-value kind."),
        };

    private static int Inspectˉobject(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale object-inspect <object.wvo>");
        }

        var Bytes = Readˉobjectˉbytes(arguments[0]);
        var Value = Objectˉcodec.Readˉandˉverify(Bytes);
        Console.Write(Objectˉinspector.Inspect(Value, Bytes));
        return EXIT_SUCCESS;
    }

    private static int Verifyˉobject(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            return Usageˉerror("Usage: windvale object-verify <object.wvo>");
        }

        var Bytes = Readˉobjectˉbytes(arguments[0]);
        var Value = Objectˉcodec.Readˉandˉverify(Bytes);
        Console.WriteLine($"Verified object: {Value.Value.Architecture}");
        Console.WriteLine($"SHA-256: {Objectˉdigest.Calculateˉsha256(Bytes)}");
        return EXIT_SUCCESS;
    }

    private static byte[] Readˉmoduleˉbytes(string path)
    {
        var Fullˉpath = Path.GetFullPath(path);
        var Length = new FileInfo(Fullˉpath).Length;
        if (Length > Bytecodeˉlimits.MAX_MODULE_BYTES)
        {
            throw new Moduleˉformatˉexception(
                "WVB1001",
                "The module exceeds the module-size limit.");
        }

        return File.ReadAllBytes(Fullˉpath);
    }

    private static byte[] Readˉobjectˉbytes(string path)
    {
        var Fullˉpath = Path.GetFullPath(path);
        var Length = new FileInfo(Fullˉpath).Length;
        if (Length > Objectˉlimits.MAX_OBJECT_BYTES)
        {
            throw new Objectˉformatˉexception("WVO1001", "The object exceeds the object-size limit.");
        }

        return File.ReadAllBytes(Fullˉpath);
    }

    private static int Usageˉerror(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine("Run 'windvale help' for command details.");
        return EXIT_USAGE;
    }

    private static void Writeˉhelp(TextWriter output)
    {
        output.WriteLine("Windvale Seed tool");
        output.WriteLine();
        output.WriteLine("Commands:");
        output.WriteLine(
            "  windvale compile <source.wv> [--module <dependency.wv>]... " +
            "[--target <wvb|windows-x64-console-v1|linux-x64-console-v1|" +
            "windows-x64-console-v2|linux-x64-console-v2|" +
            "windows-x64-console-v3|linux-x64-console-v3|" +
            "windows-x64-verifier-v1|linux-x64-verifier-v1|" +
            "windows-x64-wvb-inspector-v1|linux-x64-wvb-inspector-v1|" +
            "windows-x64-wvo-inspector-v1|linux-x64-wvo-inspector-v1|" +
            "windows-x64-console-application-verifier-v1|" +
            "linux-x64-console-application-verifier-v1|" +
            "windows-x64-wvb-runner-v1|linux-x64-wvb-runner-v1|" +
            "windows-x64-build-driver-v1|linux-x64-build-driver-v1|" +
            "windows-x64-wva-assembler-v1|linux-x64-wva-assembler-v1|" +
            "windows-x64-wv-linker-v1|linux-x64-wv-linker-v1|" +
            "windows-x64-console-packager-v1|linux-x64-console-packager-v1|" +
            "windows-x64-console-segmented-packager-v1|" +
            "linux-x64-console-segmented-packager-v1|" +
            "windows-x64-wvb-to-wvo-v1|linux-x64-wvb-to-wvo-v1|" +
            "windows-x64-hosted-container-segmenter-v1|" +
            "linux-x64-hosted-container-segmenter-v1|" +
            "windows-x64-hosted-container-planner-v1|" +
            "linux-x64-hosted-container-planner-v1|" +
            "windows-x64-hosted-container-publisher-v1|" +
            "linux-x64-hosted-container-publisher-v1|" +
            "windows-x64-wvb-publisher-v1|linux-x64-wvb-publisher-v1|" +
            "windows-x64-console-application-publisher-v1|" +
            "linux-x64-console-application-publisher-v1|" +
            "windows-x64-wvo-publisher-v1|linux-x64-wvo-publisher-v1|" +
            "windows-x64-wvo-staging-producer-v1|" +
            "linux-x64-wvo-staging-producer-v1|" +
            "windows-x64-wvo-staging-publisher-v1|" +
            "linux-x64-wvo-staging-publisher-v1>] [-o <artifact>]");
        output.WriteLine("  windvale build <project.wvproj> [-o <module.wvb>]");
        output.WriteLine(
            "  windvale aot <module.wvb> " +
            "--target <windows-x64-console-v1|linux-x64-console-v1|" +
            "windows-x64-console-v2|linux-x64-console-v2|" +
            "windows-x64-console-v3|linux-x64-console-v3|" +
            "windows-x64-verifier-v1|linux-x64-verifier-v1|" +
            "windows-x64-wvb-inspector-v1|linux-x64-wvb-inspector-v1|" +
            "windows-x64-wvo-inspector-v1|linux-x64-wvo-inspector-v1|" +
            "windows-x64-console-application-verifier-v1|" +
            "linux-x64-console-application-verifier-v1|" +
            "windows-x64-wvb-runner-v1|linux-x64-wvb-runner-v1|" +
            "windows-x64-build-driver-v1|linux-x64-build-driver-v1|" +
            "windows-x64-wva-assembler-v1|linux-x64-wva-assembler-v1|" +
            "windows-x64-wv-linker-v1|linux-x64-wv-linker-v1|" +
            "windows-x64-console-packager-v1|linux-x64-console-packager-v1|" +
            "windows-x64-console-segmented-packager-v1|" +
            "linux-x64-console-segmented-packager-v1|" +
            "windows-x64-wvb-to-wvo-v1|linux-x64-wvb-to-wvo-v1|" +
            "windows-x64-hosted-container-segmenter-v1|" +
            "linux-x64-hosted-container-segmenter-v1|" +
            "windows-x64-hosted-container-planner-v1|" +
            "linux-x64-hosted-container-planner-v1|" +
            "windows-x64-hosted-container-publisher-v1|" +
            "linux-x64-hosted-container-publisher-v1|" +
            "windows-x64-wvb-publisher-v1|linux-x64-wvb-publisher-v1|" +
            "windows-x64-console-application-publisher-v1|" +
            "linux-x64-console-application-publisher-v1|" +
            "windows-x64-wvo-publisher-v1|linux-x64-wvo-publisher-v1|" +
            "windows-x64-wvo-staging-producer-v1|" +
            "linux-x64-wvo-staging-producer-v1|" +
            "windows-x64-wvo-staging-publisher-v1|" +
            "linux-x64-wvo-staging-publisher-v1> [-o <artifact>]");
        output.WriteLine("  windvale assemble <source.wva> [-o <object.wvo>]");
        output.WriteLine("  windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...");
        output.WriteLine("  windvale inspect <module.wvb>");
        output.WriteLine("  windvale verify <module.wvb>");
        output.WriteLine("  windvale object-inspect <object.wvo>");
        output.WriteLine("  windvale object-verify <object.wvo>");
        output.WriteLine(
            "  windvale run <module.wvb> [--allow <capability>]... [--max-steps <count>] " +
            "[--bind-read-only-directory <path>] " +
            "[--bind-random-access-storage <path>] " +
            "[--report-steps] [--report-function-steps] [--report-function-record-fields] " +
            "[--report-function-dynamic-values] [--report-dynamic-lifetime] " +
            "[--report-dynamic-allocator] " +
            "[-- <argument>...]");
        output.WriteLine("  windvale help");
    }
}
