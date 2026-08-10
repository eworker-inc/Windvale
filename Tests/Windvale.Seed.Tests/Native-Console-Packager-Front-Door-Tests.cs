using System.Security.Cryptography;
using System.Text.Json;
using Windvale.Linker;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉconsoleˉpackagerˉfrontˉdoorˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Console-Packager-Candidate");
        Assertˉconsoleˉpackagerˉmanifest(
            Artifactˉroot,
            "windvale-native-console-packager-candidate-2",
            "0223",
            "0303");
        Assertˉconsoleˉpackagerˉmanifest(
            Path.Combine(
                Repository,
                "Artifacts",
                "Native-Console-Segmented-Packager-Candidate"),
            "windvale-native-console-segmented-packager-candidate-2",
            "0342",
            "0342");

        var Windowsˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Package-Console.cmd"));
        var Linuxˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Package-Console.sh"));
        Contains(Windowsˉlauncher, WINDOWS_CONSOLE_PACKAGER_APPLICATION_SHA256);
        Contains(Linuxˉlauncher, LINUX_CONSOLE_PACKAGER_APPLICATION_SHA256);
        Contains(
            File.ReadAllText(Path.Combine(
                Repository,
                "Tools",
                "Native",
                "Stage-Console-Segmented.cmd")),
            WINDOWS_CONSOLE_SEGMENTED_PACKAGER_APPLICATION_SHA256);
        Contains(
            File.ReadAllText(Path.Combine(
                Repository,
                "Tools",
                "Native",
                "Stage-Console-Segmented.sh")),
            LINUX_CONSOLE_SEGMENTED_PACKAGER_APPLICATION_SHA256);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-console-packager-front-door-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            byte[] Nativeˉimage = [0xB8, 42, 0, 0, 0, 0xC3];
            var Nativeˉimageˉpath = Path.Combine(Directoryˉpath, "Return-42.bin");
            File.WriteAllBytes(Nativeˉimageˉpath, Nativeˉimage);
            var Windowsˉtarget = OperatingSystem.IsWindows();
            var Packageˉtarget = Windowsˉtarget
                ? Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME;
            var Applicationˉpath = Path.Combine(
                Directoryˉpath,
                Windowsˉtarget ? "Return-42.exe" : "Return-42.elf");
            var Planned = Consoleˉapplicationˉlayout.Plan(
                Windowsˉtarget
                    ? Consoleˉapplicationˉtarget.Windowsˉx64
                    : Consoleˉapplicationˉtarget.Linuxˉx64,
                Nativeˉimage.Length,
                0);
            var Expectedˉreport =
                $"package status=Valid target={Packageˉtarget} " +
                "native-image-bytes=6 entry-offset=0 application-bytes=" +
                $"{Planned.Applicationˉbytes}\n";

            var Packaged = Runˉnativeˉwvbˉtool(
                Repository,
                "Package-Console",
                Packageˉtarget,
                Nativeˉimageˉpath,
                "0",
                Applicationˉpath);
            Equal(0, Packaged.Exitˉcode);
            Equal(Expectedˉreport, Packaged.Output);
            Equal(string.Empty, Packaged.Error);
            if (Windowsˉtarget)
            {
                var Application = Windowsˉconsoleˉapplicationˉverifier.Verify(
                    File.ReadAllBytes(Applicationˉpath));
                Sequenceˉequal(Nativeˉimage, Application.Nativeˉimageˉbytes);
                Equal(0u, Application.Nativeˉentryˉoffset);
            }
            else
            {
                var Application = Linuxˉconsoleˉapplicationˉverifier.Verify(
                    File.ReadAllBytes(Applicationˉpath));
                Sequenceˉequal(Nativeˉimage, Application.Nativeˉimageˉbytes);
                Equal(0u, Application.Nativeˉentryˉoffset);
            }

            var Usage = Runˉnativeˉwvbˉtool(
                Repository,
                "Package-Console",
                Packageˉtarget);
            Equal(64, Usage.Exitˉcode);
            Equal(string.Empty, Usage.Output);
            Contains(Usage.Error, "Usage:");
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static void Assertˉconsoleˉpackagerˉmanifest(
        string Artifactˉroot,
        string Format,
        string Sourceˉdecision,
        string Provenanceˉdecision)
    {
        using var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json")));
        var Root = Manifest.RootElement;
        Equal(Format, Root.GetProperty("format").GetString());
        Equal("candidate", Root.GetProperty("status").GetString());
        Equal(Sourceˉdecision, Root.GetProperty("sourceDecision").GetString());
        Equal(
            Provenanceˉdecision,
            Root.GetProperty("provenanceDecision").GetString());
        Equal("pending", Root.GetProperty("qualification").GetString());
        Equal("0498", Root.GetProperty("constructionDecision").GetString());
        Equal(
            "native-cross-target-hosted-toolset",
            Root.GetProperty("construction").GetString());
        Equal(3, Root.GetProperty("artifacts").GetArrayLength());
        foreach (var Artifact in Root.GetProperty("artifacts").EnumerateArray())
        {
            var Relative = Artifact.GetProperty("path").GetString() ??
                throw new InvalidDataException(
                    "A native console-packager artifact path is missing.");
            var Artifactˉpath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Artifactˉroot,
                Relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
            True(
                Artifactˉpath.StartsWith(
                    Artifactˉroot + System.IO.Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal),
                "A native console-packager manifest path escaped its artifact root.");
            var Bytes = File.ReadAllBytes(Artifactˉpath);
            Equal(Artifact.GetProperty("bytes").GetInt32(), Bytes.Length);
            Equal(
                Artifact.GetProperty("sha256").GetString(),
                Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant());
        }
    }
}
