using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvˉlinkerˉfrontˉdoorˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Wv-Linker-Candidate");
        using var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json")));
        var Root = Manifest.RootElement;
        Equal("windvale-native-wv-linker-candidate-1", Root.GetProperty("format").GetString());
        Equal("candidate", Root.GetProperty("status").GetString());
        Equal("0221", Root.GetProperty("sourceDecision").GetString());
        Equal("0302", Root.GetProperty("provenanceDecision").GetString());
        Equal("pending", Root.GetProperty("qualification").GetString());
        Equal(3, Root.GetProperty("artifacts").GetArrayLength());
        foreach (var Artifact in Root.GetProperty("artifacts").EnumerateArray())
        {
            var Relative = Artifact.GetProperty("path").GetString() ??
                throw new InvalidDataException("A native linker artifact path is missing.");
            var Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Artifactˉroot,
                Relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
            True(
                Path.StartsWith(
                    Artifactˉroot + System.IO.Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal),
                "A native linker manifest path escaped its artifact root.");
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
            "Link-Wvo.cmd"));
        var Linuxˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Link-Wvo.sh"));
        Contains(Windowsˉlauncher, WINDOWS_WV_LINKER_APPLICATION_SHA256);
        Contains(Linuxˉlauncher, LINUX_WV_LINKER_APPLICATION_SHA256);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wv-linker-front-door-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Mainˉbytes = Assembleˉsuccess(HELLO_ASSEMBLY_SOURCE);
            var Providerˉbytes = Assembleˉsuccess(CONSOLE_PROVIDER_ASSEMBLY_SOURCE);
            var Mainˉpath = Path.Combine(Directoryˉpath, "Main.wvo");
            var Providerˉpath = Path.Combine(Directoryˉpath, "Provider.wvo");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Application.bin");
            File.WriteAllBytes(Mainˉpath, Mainˉbytes);
            File.WriteAllBytes(Providerˉpath, Providerˉbytes);
            var Oracle = Linkˉsuccess(
                [Mainˉbytes, Providerˉbytes],
                new(Linkˉcontract.DEFAULT_BASE_ADDRESS, "Main"));
            var Expectedˉmap = Encoding.UTF8.GetString(Oracle.Mapˉbytes.AsSpan())
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');

            var Nativeˉlink = Runˉnativeˉwvbˉtool(
                Repository,
                "Link-Wvo",
                Linkˉcontract.DEFAULT_BASE_ADDRESS.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "Main",
                Outputˉpath,
                Mainˉpath,
                Providerˉpath);
            Equal(0, Nativeˉlink.Exitˉcode);
            Equal(Expectedˉmap, Nativeˉlink.Output);
            Equal(string.Empty, Nativeˉlink.Error);
            Sequenceˉequal(Oracle.Imageˉbytes, File.ReadAllBytes(Outputˉpath));

            var Usage = Runˉnativeˉwvbˉtool(Repository, "Link-Wvo", Mainˉpath);
            Equal(64, Usage.Exitˉcode);
            Equal(string.Empty, Usage.Output);
            Contains(Usage.Error, "Usage:");
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
