using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVO_PUBLISHER_SOURCE = Readˉembeddedˉsource(
        "Windvale.Seed.Tests.Wvo-Publisher-Tool.wv");

    private static void Wvoˉpublisherˉruns()
    {
        var Compilation = Seedˉcompiler.Compileˉmodules(
            new("Wvo-Publisher-Tool.wv", WVO_PUBLISHER_SOURCE),
            [
                new("Foundation/Byte-Ordering.wv", BYTE_ORDERING_SOURCE),
                new(
                    "Object-Model/Windvale/Wvo-Object-Verification.wv",
                    WVO_VERIFICATION_SOURCE),
                new(
                    "Wvb-Publication-Transaction.wv",
                    WVB_PUBLICATION_TRANSACTION_SOURCE),
                new(
                    "Wvb-Publication-Native-Bridge.wv",
                    WVB_PUBLICATION_NATIVE_BRIDGE_SOURCE),
            ]);
        True(
            Compilation.Success,
            "The WVO publisher did not compile: " +
                string.Join(" | ", Compilation.Diagnostics));
        Equal(
            Wvoˉpublisherˉapplicationˉcontract.MODULE_BYTES,
            Compilation.Moduleˉbytes.Length);
        Equal(
            Wvoˉpublisherˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compilation.Moduleˉbytes.AsSpan()));

        var Verified = Moduleˉcodec.Readˉandˉverify(
            Compilation.Moduleˉbytes.AsSpan());
        Equal(Wvoˉpublisherˉapplicationˉcontract.MODULE_NAME, Verified.Module.Name);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Verified.Module.Capabilities.Select(Item => Item.Name));
        var Native = X64ˉnativeˉbackend.Compile(Verified);
        Nativeˉfragmentˉverifier.Verify(Native.Fragment);

        var Windowsˉresult = Wvoˉpublisherˉapplicationˉwriter.Writeˉwindows(
            Verified,
            Native.Fragment,
            Compilation.Moduleˉbytes.AsSpan());
        True(
            Windowsˉresult.Success,
            "The Windows WVO publisher was rejected: " +
                string.Join(" | ", Windowsˉresult.Diagnostics));
        Equal(
            Wvoˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windowsˉresult.Imageˉbytes.Length);
        Equal(
            Wvoˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windowsˉresult.Imageˉbytes.AsSpan()));

        var Linuxˉresult = Wvoˉpublisherˉapplicationˉwriter.Writeˉlinux(
            Verified,
            Native.Fragment,
            Compilation.Moduleˉbytes.AsSpan());
        True(
            Linuxˉresult.Success,
            "The Linux WVO publisher was rejected: " +
                string.Join(" | ", Linuxˉresult.Diagnostics));
        Equal(
            Wvoˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linuxˉresult.Imageˉbytes.Length);
        Equal(
            Wvoˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linuxˉresult.Imageˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Wvo-Publisher-Candidate");
        using (var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json"))))
        {
            var Root = Manifest.RootElement;
            Equal(
                "windvale-native-wvo-publisher-candidate-1",
                Root.GetProperty("format").GetString());
            Equal("0308", Root.GetProperty("provenanceDecision").GetString());
            Equal("stage0-recovery", Root.GetProperty("construction").GetString());
            foreach (var Artifact in Root.GetProperty("artifacts").EnumerateArray())
            {
                var Relative = Artifact.GetProperty("path").GetString() ??
                    throw new InvalidDataException("A WVO publisher artifact path is missing.");
                var Path = System.IO.Path.Combine(Artifactˉroot, Relative);
                var Bytes = File.ReadAllBytes(Path);
                Equal(Artifact.GetProperty("bytes").GetInt32(), Bytes.Length);
                Equal(
                    Artifact.GetProperty("sha256").GetString(),
                    Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant());
            }
        }
        Contains(
            File.ReadAllText(Path.Combine(Repository, "Tools", "Native", "Lower-Wvb-To-Wvo.cmd")),
            "Publish-Wvo.cmd");
        Contains(
            File.ReadAllText(Path.Combine(Repository, "Tools", "Native", "Lower-Wvb-To-Wvo.sh")),
            "Publish-Wvo.sh");

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-wvo-publisher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Candidate = Objectˉcodec.Write(Buildˉsampleˉobject()).ToImmutableArray();
            var Candidateˉpath = Path.Combine(Directoryˉpath, "Candidate.wvo");
            var Destinationˉpath = Path.Combine(Directoryˉpath, "Destination.wvo");
            File.WriteAllBytes(Candidateˉpath, Candidate.AsSpan());
            File.WriteAllBytes(Destinationˉpath, [1, 2, 3, 4]);
            var Report =
                $"publication status=Complete bytes=0x{Candidate.Length:x8} " +
                $"sha256={Objectˉdigest.Calculateˉsha256(Candidate.AsSpan())}\n";

            if (OperatingSystem.IsWindows())
            {
                var Loadedˉmodules = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
                Equal(
                    0,
                    Executeˉwindowsˉapplication(
                        Windowsˉresult.Imageˉbytes,
                        Report,
                        [Candidateˉpath, Destinationˉpath],
                        loadedˉmodules: Loadedˉmodules));
                Equal(
                    0,
                    Loadedˉmodules.Count(Name =>
                        Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(
                    0,
                    Executeˉlinuxˉapplication(
                        Linuxˉresult.Imageˉbytes,
                        Report,
                        [Candidateˉpath, Destinationˉpath],
                        loadedˉmappings: Loadedˉmappings));
                Equal(
                    0,
                    Loadedˉmappings.Count(Name =>
                        Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                        Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            Sequenceˉequal(Candidate, File.ReadAllBytes(Destinationˉpath));
            _ = Objectˉcodec.Readˉandˉverify(File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());

            var Preserved = new byte[] { 9, 8, 7, 6 };
            File.WriteAllBytes(Candidateˉpath, [0]);
            File.WriteAllBytes(Destinationˉpath, Preserved);
            if (OperatingSystem.IsWindows())
            {
                Equal(
                    1,
                    Executeˉwindowsˉapplication(
                        Windowsˉresult.Imageˉbytes,
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        expectedˉerror: "publication status=Rejected phase=wvo\n"));
            }
            else
            {
                Equal(
                    1,
                    Executeˉlinuxˉapplication(
                        Linuxˉresult.Imageˉbytes,
                        string.Empty,
                        [Candidateˉpath, Destinationˉpath],
                        expectedˉerror: "publication status=Rejected phase=wvo\n"));
            }
            Sequenceˉequal(Preserved, File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvpublish-*").Count());
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
