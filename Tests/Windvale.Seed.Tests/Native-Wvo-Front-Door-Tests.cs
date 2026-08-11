using System.Security.Cryptography;
using System.Text.Json;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const string WINDOWS_WVO_INSPECTOR_APPLICATION_SHA256 =
        "a534b1c7a5ff9112c221a9576141842c4bb50c28b1d43d0ab02a8679bba6f366";
    private const string LINUX_WVO_INSPECTOR_APPLICATION_SHA256 =
        "f94d2e16da76c949e15978bd879bff38205685be08d7afa1670f48d3f6592ea1";

    private static void Nativeˉwvoˉfrontˉdoorˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Wvo-Object-Candidate");
        using var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json")));
        var Root = Manifest.RootElement;
        Equal("windvale-native-wvo-object-candidate-3", Root.GetProperty("format").GetString());
        Equal("candidate", Root.GetProperty("status").GetString());
        Equal("0222", Root.GetProperty("sourceDecision").GetString());
        Equal("0308", Root.GetProperty("provenanceDecision").GetString());
        Equal("0522", Root.GetProperty("constructionDecision").GetString());
        Equal("pending", Root.GetProperty("qualification").GetString());
        Equal(
            "windvale-native-profile-6-enum-complete-reconstruction",
            Root.GetProperty("construction").GetString());
        Equal(4, Root.GetProperty("artifacts").GetArrayLength());
        foreach (var Artifact in Root.GetProperty("artifacts").EnumerateArray())
        {
            var Relative = Artifact.GetProperty("path").GetString() ??
                throw new InvalidDataException("A native WVO artifact path is missing.");
            var Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Artifactˉroot,
                Relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
            True(
                Path.StartsWith(
                    Artifactˉroot + System.IO.Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal),
                "A native WVO manifest path escaped its artifact root.");
            var Bytes = File.ReadAllBytes(Path);
            Equal(Artifact.GetProperty("bytes").GetInt32(), Bytes.Length);
            Equal(
                Artifact.GetProperty("sha256").GetString(),
                Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant());
        }

        var Windowsˉverify = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Verify-Wvo.cmd"));
        var Windowsˉinspect = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Inspect-Wvo.cmd"));
        var Linuxˉverify = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Verify-Wvo.sh"));
        var Linuxˉinspect = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Inspect-Wvo.sh"));
        Contains(Windowsˉverify, WINDOWS_WVO_INSPECTOR_APPLICATION_SHA256);
        Contains(Windowsˉinspect, WINDOWS_WVO_INSPECTOR_APPLICATION_SHA256);
        Contains(Linuxˉverify, LINUX_WVO_INSPECTOR_APPLICATION_SHA256);
        Contains(Linuxˉinspect, LINUX_WVO_INSPECTOR_APPLICATION_SHA256);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-wvo-front-door-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Objectˉbytes = Objectˉcodec.Write(Buildˉsampleˉobject());
            var Objectˉpath = Path.Combine(Directoryˉpath, "Sample.wvo");
            File.WriteAllBytes(Objectˉpath, Objectˉbytes);
            var Verifiedˉobject = Objectˉcodec.Readˉandˉverify(Objectˉbytes.AsSpan());
            var Expectedˉverify =
                $"Verified object: {Verifiedˉobject.Value.Architecture}\n" +
                $"SHA-256: {Objectˉdigest.Calculateˉsha256(Objectˉbytes)}\n";
            var Expectedˉinspection = Objectˉinspector.Inspect(
                    Verifiedˉobject,
                    Objectˉbytes)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');

            var Nativeˉverify = Runˉnativeˉwvbˉtool(
                Repository,
                "Verify-Wvo",
                Objectˉpath);
            Equal(0, Nativeˉverify.Exitˉcode);
            Equal(Expectedˉverify, Nativeˉverify.Output);
            Equal(string.Empty, Nativeˉverify.Error);

            var Nativeˉinspect = Runˉnativeˉwvbˉtool(
                Repository,
                "Inspect-Wvo",
                Objectˉpath);
            Equal(0, Nativeˉinspect.Exitˉcode);
            Equal(Expectedˉinspection, Nativeˉinspect.Output);
            Equal(string.Empty, Nativeˉinspect.Error);

            var Wrongˉextension = Path.Combine(Directoryˉpath, "Sample.bin");
            File.WriteAllBytes(Wrongˉextension, Objectˉbytes);
            var Rejected = Runˉnativeˉwvbˉtool(
                Repository,
                "Verify-Wvo",
                Wrongˉextension);
            Equal(64, Rejected.Exitˉcode);
            Equal(string.Empty, Rejected.Output);
            Contains(Rejected.Error, "must use the .wvo extension");
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
