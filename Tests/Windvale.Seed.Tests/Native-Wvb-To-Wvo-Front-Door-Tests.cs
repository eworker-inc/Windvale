using System.Security.Cryptography;
using System.Text.Json;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvbˉtoˉwvoˉfrontˉdoorˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Wvb-To-Wvo-Candidate");
        using var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json")));
        var Root = Manifest.RootElement;
        Equal(
            "windvale-native-wvb-to-wvo-candidate-2",
            Root.GetProperty("format").GetString());
        Equal("candidate", Root.GetProperty("status").GetString());
        Equal("0224", Root.GetProperty("sourceDecision").GetString());
        Equal("0423", Root.GetProperty("provenanceDecision").GetString());
        Equal("pending", Root.GetProperty("qualification").GetString());
        Equal(
            "native-qualified-front-door",
            Root.GetProperty("toolConstruction").GetString());
        Equal(
            "0497",
            Root.GetProperty("constructionDecision").GetString());
        Equal(
            "native-segmented-self-reconstruction",
            Root.GetProperty("applicationConstruction").GetString());
        Equal(5, Root.GetProperty("artifacts").GetArrayLength());
        foreach (var Artifact in Root.GetProperty("artifacts").EnumerateArray())
        {
            var Relative = Artifact.GetProperty("path").GetString() ??
                throw new InvalidDataException(
                    "A native WVB-to-WVO artifact path is missing.");
            var Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Artifactˉroot,
                Relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
            True(
                Path.StartsWith(
                    Artifactˉroot + System.IO.Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal),
                "A native WVB-to-WVO manifest path escaped its artifact root.");
            var Bytes = File.ReadAllBytes(Path);
            Equal(Artifact.GetProperty("bytes").GetInt32(), Bytes.Length);
            Equal(
                Artifact.GetProperty("sha256").GetString(),
                Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant());
        }

        var Windowsˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Lower-Wvb-To-Wvo.cmd"));
        var Linuxˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Lower-Wvb-To-Wvo.sh"));
        Contains(Windowsˉlauncher, WINDOWS_WVB_TO_WVO_APPLICATION_SHA256);
        Contains(Linuxˉlauncher, LINUX_WVB_TO_WVO_APPLICATION_SHA256);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvb-to-wvo-front-door-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Inputˉpath = Path.Combine(Artifactˉroot, "Return-42.wvb");
            var Expectedˉpath = Path.Combine(Artifactˉroot, "Return-42.wvo");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Return-42.wvo");
            var Lowered = Runˉnativeˉwvbˉtool(
                Repository,
                "Lower-Wvb-To-Wvo",
                Inputˉpath,
                Outputˉpath);
            Equal(0, Lowered.Exitˉcode);
            Equal(
                "native x64 status=Valid abi=22 code-bytes=406 object-bytes=479\n",
                Lowered.Output);
            Equal(string.Empty, Lowered.Error);
            var Output = File.ReadAllBytes(Outputˉpath);
            Sequenceˉequal(File.ReadAllBytes(Expectedˉpath), Output);
            var Verified = Objectˉcodec.Readˉandˉverify(Output.AsSpan()).Value;
            Equal(Objectˉarchitecture.X86ˉ64, Verified.Architecture);
            Equal(1, Verified.Sections.Length);
            Equal(1, Verified.Symbols.Length);
            Equal(0, Verified.Relocations.Length);

            var Wrongˉoutput = Path.Combine(Directoryˉpath, "Return-42.bin");
            var Rejected = Runˉnativeˉwvbˉtool(
                Repository,
                "Lower-Wvb-To-Wvo",
                Inputˉpath,
                Wrongˉoutput);
            Equal(2, Rejected.Exitˉcode);
            Equal(string.Empty, Rejected.Output);
            Contains(Rejected.Error, "must use the .wvo extension");
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
