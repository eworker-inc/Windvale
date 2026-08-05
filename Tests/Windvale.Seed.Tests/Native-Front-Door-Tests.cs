using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Windvale.Bytecode;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉsourceˉtoˉwvbˉfrontˉdoorˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Front-Door");
        using var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json")));
        var Root = Manifest.RootElement;
        Equal(
            "windvale-native-front-door-1",
            Root.GetProperty("format").GetString());
        Equal(
            "3aa5ba27f0ae4f96bc80d8bd521363015e884ab3",
            Root.GetProperty("assemblerCommit").GetString());
        Equal(
            "https://github.com/eworker-inc/Windvale/actions/runs/31004212797",
            Root.GetProperty("assemblerQualification").GetString());
        Equal(18, Root.GetProperty("artifacts").GetArrayLength());
        foreach (var Artifact in Root.GetProperty("artifacts").EnumerateArray())
        {
            var Relative = Artifact.GetProperty("path").GetString() ??
                throw new InvalidDataException(
                    "A native-front-door artifact path is missing.");
            var Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Artifactˉroot,
                Relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
            True(
                Path.StartsWith(
                    Artifactˉroot + System.IO.Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal),
                "A native-front-door manifest path escaped its artifact root.");
            var Bytes = File.ReadAllBytes(Path);
            Equal(Artifact.GetProperty("bytes").GetInt32(), Bytes.Length);
            Equal(
                Artifact.GetProperty("sha256").GetString(),
                Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant());
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-front-door-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Outputˉpath = Path.Combine(Directoryˉpath, "Output.wvb");
            File.WriteAllBytes(Outputˉpath, [1, 2, 3, 4]);
            var Success = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Examples",
                    "Foundation",
                    "Module-Composition-Demo.wvproj"),
                Outputˉpath);
            Equal(0, Success.Exitˉcode);
            Contains(Success.Output, "build status=Published verification=compiler-aligned");
            Contains(Success.Output, "publication status=Complete bytes=0x00000294");
            Equal(string.Empty, Success.Error);
            var Published = File.ReadAllBytes(Outputˉpath);
            Equal(660, Published.Length);
            Equal(
                "030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607",
                Convert.ToHexString(SHA256.HashData(Published)).ToLowerInvariant());
            var Verified = Moduleˉcodec.Readˉandˉverify(Published);
            Equal("Compositionˉdemo", Verified.Module.Name);
            Equal(Moduleˉprofile.Portable, Verified.Module.Profile);
            Equal(0, Verified.Module.Capabilities.Length);
            Equal(4, Verified.Module.Functions.Length);

            var Nativeˉverify = Runˉnativeˉwvbˉtool(
                Repository,
                "Verify-Wvb",
                Outputˉpath);
            Equal(0, Nativeˉverify.Exitˉcode);
            Equal("wvb status=Valid profile=compiler-aligned\n", Nativeˉverify.Output);
            Equal(string.Empty, Nativeˉverify.Error);
            var Nativeˉinspect = Runˉnativeˉwvbˉtool(
                Repository,
                "Inspect-Wvb",
                Outputˉpath);
            Equal(0, Nativeˉinspect.Exitˉcode);
            Contains(Nativeˉinspect.Output, "wvdump 1\n");
            Contains(Nativeˉinspect.Output,
                "module version=1.11 profile=portable name=\"Composition\\u02C9demo\"");
            Equal(string.Empty, Nativeˉinspect.Error);
            var Nativeˉrun = Runˉnativeˉwvbˉtool(
                Repository,
                "Run-Wvb",
                Outputˉpath);
            Equal(0, Nativeˉrun.Exitˉcode);
            Equal("Result: 42\n", Nativeˉrun.Output);
            Equal(string.Empty, Nativeˉrun.Error);

            var Assemblyˉpath = Path.Combine(Directoryˉpath, "Hello.wva");
            var Objectˉpath = Path.Combine(Directoryˉpath, "Hello.wvo");
            File.WriteAllText(
                Assemblyˉpath,
                HELLO_ASSEMBLY_SOURCE,
                new System.Text.UTF8Encoding(false));
            var Nativeˉassemble = Runˉnativeˉwvbˉtool(
                Repository,
                "Assemble-Wva",
                Assemblyˉpath,
                Objectˉpath);
            Equal(0, Nativeˉassemble.Exitˉcode);
            Contains(Nativeˉassemble.Output, "wvasm 1\n");
            Contains(Nativeˉassemble.Output, "assembly status=valid object-bytes=218");
            Equal(string.Empty, Nativeˉassemble.Error);
            Sequenceˉequal(
                Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE),
                File.ReadAllBytes(Objectˉpath));

            var Invalidˉassemblyˉpath = Path.Combine(Directoryˉpath, "Invalid.wva");
            File.WriteAllText(
                Invalidˉassemblyˉpath,
                "not assembly\n",
                new System.Text.UTF8Encoding(false));
            ReadOnlySpan<byte> Preservedˉobject = [4, 3, 2, 1];
            File.WriteAllBytes(Objectˉpath, Preservedˉobject);
            var Rejectedˉassembly = Runˉnativeˉwvbˉtool(
                Repository,
                "Assemble-Wva",
                Invalidˉassemblyˉpath,
                Objectˉpath);
            Equal(2, Rejectedˉassembly.Exitˉcode);
            Equal(string.Empty, Rejectedˉassembly.Output);
            Contains(Rejectedˉassembly.Error, "assembly status=WVA1001");
            Sequenceˉequal(Preservedˉobject.ToArray(), File.ReadAllBytes(Objectˉpath));

            Published[0] = 0;
            var Invalidˉwvbˉpath = Path.Combine(Directoryˉpath, "Invalid.wvb");
            File.WriteAllBytes(Invalidˉwvbˉpath, Published);
            var Invalidˉverify = Runˉnativeˉwvbˉtool(
                Repository,
                "Verify-Wvb",
                Invalidˉwvbˉpath);
            Equal(1, Invalidˉverify.Exitˉcode);
            Equal(string.Empty, Invalidˉverify.Output);
            Equal("wvb status=Invalid phase=semantic\n", Invalidˉverify.Error);
            var Invalidˉinspect = Runˉnativeˉwvbˉtool(
                Repository,
                "Inspect-Wvb",
                Invalidˉwvbˉpath);
            Equal(1, Invalidˉinspect.Exitˉcode);
            Equal(string.Empty, Invalidˉinspect.Output);
            Equal("wvb status=Invalid phase=semantic\n", Invalidˉinspect.Error);

            var Invalidˉproject = Path.Combine(Directoryˉpath, "Invalid.wvproj");
            File.WriteAllText(
                Invalidˉproject,
                "windvale-project 1\nroot \"../Missing.wv\"\nemit wvb\n");
            ReadOnlySpan<byte> Preserved = [9, 8, 7, 6];
            File.WriteAllBytes(Outputˉpath, Preserved);
            var Rejected = Runˉnativeˉfrontˉdoor(
                Repository,
                Invalidˉproject,
                Outputˉpath);
            Equal(1, Rejected.Exitˉcode);
            Contains(Rejected.Error, "WVP1006");
            Sequenceˉequal(Preserved.ToArray(), File.ReadAllBytes(Outputˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static (
        int Exitˉcode,
        string Output,
        string Error) Runˉnativeˉwvbˉtool(
        string repository,
        string tool,
        params string[] arguments)
    {
        var Startˉinfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows()
                ? Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe"
                : "/usr/bin/env",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = repository,
        };
        var Extension = OperatingSystem.IsWindows() ? ".cmd" : ".sh";
        var Launcher = Path.Combine(repository, "Tools", "Native", tool + Extension);
        if (OperatingSystem.IsWindows())
        {
            Startˉinfo.ArgumentList.Add("/d");
            Startˉinfo.ArgumentList.Add("/c");
            Startˉinfo.ArgumentList.Add(Launcher);
        }
        else
        {
            Startˉinfo.ArgumentList.Add("bash");
            Startˉinfo.ArgumentList.Add(Launcher);
        }
        foreach (var Argument in arguments)
        {
            Startˉinfo.ArgumentList.Add(Argument);
        }
        using var Process = System.Diagnostics.Process.Start(Startˉinfo) ??
            throw new InvalidOperationException($"The native {tool} launcher did not start.");
        var Outputˉtask = Process.StandardOutput.ReadToEndAsync();
        var Errorˉtask = Process.StandardError.ReadToEndAsync();
        if (!Process.WaitForExit(60_000))
        {
            Process.Kill(entireProcessTree: true);
            throw new InvalidOperationException($"The native {tool} launcher did not exit.");
        }
        return (
            Process.ExitCode,
            Outputˉtask.GetAwaiter().GetResult().Replace(
                "\r\n", "\n", StringComparison.Ordinal),
            Errorˉtask.GetAwaiter().GetResult().Replace(
                "\r\n", "\n", StringComparison.Ordinal));
    }

    private static (
        int Exitˉcode,
        string Output,
        string Error) Runˉnativeˉfrontˉdoor(
        string repository,
        string project,
        string output)
    {
        var Startˉinfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ?
                Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe" :
                "/usr/bin/env",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = repository,
        };
        if (OperatingSystem.IsWindows())
        {
            Startˉinfo.ArgumentList.Add("/d");
            Startˉinfo.ArgumentList.Add("/c");
            Startˉinfo.ArgumentList.Add(Path.Combine(
                repository,
                "Tools",
                "Native",
                "Build-Wvb.cmd"));
        }
        else
        {
            Startˉinfo.ArgumentList.Add("bash");
            Startˉinfo.ArgumentList.Add(Path.Combine(
                repository,
                "Tools",
                "Native",
                "Build-Wvb.sh"));
        }
        Startˉinfo.ArgumentList.Add(project);
        Startˉinfo.ArgumentList.Add(output);
        using var Process = System.Diagnostics.Process.Start(Startˉinfo) ??
            throw new InvalidOperationException(
                "The native source-to-WVB front door did not start.");
        var Outputˉtask = Process.StandardOutput.ReadToEndAsync();
        var Errorˉtask = Process.StandardError.ReadToEndAsync();
        if (!Process.WaitForExit(60_000))
        {
            Process.Kill(entireProcessTree: true);
            throw new InvalidOperationException(
                "The native source-to-WVB front door did not exit.");
        }
        return (
            Process.ExitCode,
            Outputˉtask.GetAwaiter().GetResult(),
            Errorˉtask.GetAwaiter().GetResult());
    }

    private static string Findˉrepositoryˉroot()
    {
        var Directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (Directory is not null)
        {
            if (File.Exists(Path.Combine(Directory.FullName, "Windvale.slnx")) &&
                File.Exists(Path.Combine(Directory.FullName, "AGENTS.md")))
            {
                return Directory.FullName;
            }
            Directory = Directory.Parent;
        }
        throw new DirectoryNotFoundException(
            "The Windvale repository root was not found.");
    }
}
