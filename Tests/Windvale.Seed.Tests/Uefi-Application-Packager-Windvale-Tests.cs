using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_UEFI_APPLICATION_PACKAGER_SIZE = 25_999;
    private const string NATIVE_UEFI_APPLICATION_PACKAGER_SHA256 =
        "063f95f53e39390c76bcf31fbf7bdc87eed6194388101fadc4d60ee41b2802e4";

    private static void Windvaleˉuefiˉapplicationˉpackagerˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Root = Source(
            "Linker/Windvale/Uefi-Application-Packager.wv",
            "Uefi-Application-Packager.wv");
        Sourceˉmoduleˉinput[] Dependencies =
        [
            Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
            Source("Foundation/Decimal-Parsing.wv", "Decimal-Parsing.wv"),
            Source(
                "Linker/Windvale/Uefi-Application-Construction-Core.wv",
                "Uefi-Application-Construction-Core.wv"),
            Source(
                "Linker/Windvale/Uefi-Application-Verification-Core.wv",
                "Uefi-Application-Verification-Core.wv"),
        ];
        var Compiled = Seedˉcompiler.Compileˉmodules(Root, Dependencies);
        True(
            Compiled.Success,
            "The Windvale UEFI packager did not compile: " +
                string.Join(" | ", Compiled.Diagnostics));
        Equal(NATIVE_UEFI_APPLICATION_PACKAGER_SIZE, Compiled.Moduleˉbytes.Length);
        Equal(
            NATIVE_UEFI_APPLICATION_PACKAGER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.FILE_WRITE_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Module.Module.Capabilities
                .Select(Item => Item.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());

        var Repository = Findˉrepositoryˉroot();
        var Artifactˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Uefi-Packager-Candidate");
        using var Manifest = JsonDocument.Parse(File.ReadAllBytes(
            Path.Combine(Artifactˉroot, "Manifest.json")));
        var Manifestˉroot = Manifest.RootElement;
        Equal(
            "windvale-native-uefi-packager-candidate-1",
            Manifestˉroot.GetProperty("format").GetString());
        Equal("candidate", Manifestˉroot.GetProperty("status").GetString());
        Equal("0437", Manifestˉroot.GetProperty("sourceDecision").GetString());
        Equal("0438", Manifestˉroot.GetProperty("provenanceDecision").GetString());
        Equal("pending", Manifestˉroot.GetProperty("qualification").GetString());
        Equal(
            "native-cross-target-hosted-toolset",
            Manifestˉroot.GetProperty("construction").GetString());
        Equal(3, Manifestˉroot.GetProperty("artifacts").GetArrayLength());
        foreach (var Artifact in Manifestˉroot.GetProperty("artifacts").EnumerateArray())
        {
            var Relative = Artifact.GetProperty("path").GetString() ??
                throw new InvalidDataException(
                    "A native UEFI packager artifact path is missing.");
            var Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                Artifactˉroot,
                Relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
            True(
                Path.StartsWith(
                    Artifactˉroot + System.IO.Path.DirectorySeparatorChar,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal),
                "A native UEFI packager manifest path escaped its artifact root.");
            var Bytes = File.ReadAllBytes(Path);
            Equal(Artifact.GetProperty("bytes").GetInt32(), Bytes.Length);
            Equal(
                Artifact.GetProperty("sha256").GetString(),
                Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant());
        }
        Sequenceˉequal(
            Compiled.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(Artifactˉroot, "Uefi-Packager.wvb")));
        var Windowsˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Package-Uefi.cmd"));
        var Linuxˉlauncher = File.ReadAllText(Path.Combine(
            Repository,
            "Tools",
            "Native",
            "Package-Uefi.sh"));
        Contains(
            Windowsˉlauncher,
            "326401d0e3d9e6b1c1e329a1fa0c7f5e550f4d48e073d0ab83f6f8657edff320");
        Contains(
            Linuxˉlauncher,
            "9b1c3a364e21b3fb66b246fb89df907b523272fee4e3ac5eaa39f5414e39e5b6");
        var Nativeˉapplication = File.ReadAllBytes(Path.Combine(
                Artifactˉroot,
                OperatingSystem.IsWindows() ? "Uefi-Packager.exe" : "Uefi-Packager.elf"))
            .ToImmutableArray();

        static ImmutableArray<byte> Expectedˉapplication(
            ImmutableArray<byte> code,
            uint entryˉoffset)
        {
            var Objectˉbytes = Objectˉcodec.Write(new Objectˉfile(
                Objectˉarchitecture.X86ˉ64,
                [new(".text", Objectˉsectionˉkind.Code, 16, (uint)code.Length, code)],
                [new(
                    "Main",
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    entryˉoffset,
                    (uint)code.Length - entryˉoffset)],
                [])).ToImmutableArray();
            var Linked = Linkˉcompiler.Link([new(Objectˉbytes)], new(0, "Main"));
            True(Linked.Success, "The UEFI packager fixture did not link.");
            var Application = Uefiˉapplicationˉwriter.Write(Linked);
            True(Application.Success, "The recovery UEFI writer rejected the fixture.");
            return Application.Imageˉbytes;
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-uefi-packager-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            ImmutableArray<byte> Code = [0x90, 0xC3, 3, 5, 8, 13];
            var Objectˉpath = Path.Combine(Directoryˉpath, "Input.wvo");
            var Inputˉpath = Path.Combine(Directoryˉpath, "Native.bin");
            var Outputˉpath = Path.Combine(Directoryˉpath, "Application.efi");
            var Repeatˉpath = Path.Combine(Directoryˉpath, "Application-Again.efi");
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.efi");
            var Objectˉbytes = Objectˉcodec.Write(new Objectˉfile(
                Objectˉarchitecture.X86ˉ64,
                [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
                [new(
                    "Main",
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    1,
                    (uint)Code.Length - 1u)],
                [])).ToImmutableArray();
            File.WriteAllBytes(Objectˉpath, Objectˉbytes.AsSpan());
            var Linked = Runˉnativeˉwvbˉtool(
                Repository,
                "Link-Wvo",
                "0",
                "Main",
                Inputˉpath,
                Objectˉpath);
            Equal(0, Linked.Exitˉcode);
            Equal(string.Empty, Linked.Error);
            Contains(Linked.Output, "entry name=Main address=1\n");
            Sequenceˉequal(Code, File.ReadAllBytes(Inputˉpath));
            byte[] Sentinel = [0x57, 0x56, 0x55];
            File.WriteAllBytes(Rejectedˉpath, Sentinel);
            var Expected = Expectedˉapplication(Code, 1);
            var Report =
                "uefi-package status=Valid native-image-bytes=6 " +
                "entry-offset=1 application-bytes=1536\n";
            if (OperatingSystem.IsWindows())
            {
                Equal(0, Executeˉwindowsˉapplication(Nativeˉapplication));
            }
            if (OperatingSystem.IsLinux())
            {
                Equal(0, Executeˉlinuxˉapplication(Nativeˉapplication));
            }
            var Packaged = Runˉnativeˉwvbˉtool(
                Repository,
                "Package-Uefi",
                Inputˉpath,
                "1",
                Outputˉpath);
            Equal(0, Packaged.Exitˉcode);
            Equal(Report, Packaged.Output);
            Equal(string.Empty, Packaged.Error);
            Sequenceˉequal(Expected, File.ReadAllBytes(Outputˉpath));
            var Verified = Uefiˉapplicationˉverifier.Verify(
                File.ReadAllBytes(Outputˉpath));
            Equal(1u, Verified.Entryˉcodeˉoffset);
            Sequenceˉequal(Code, Verified.Codeˉbytes);

            string[] Repeatˉarguments = [Inputˉpath, "1", Repeatˉpath];
            if (OperatingSystem.IsWindows())
            {
                var Loadedˉmodules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Nativeˉapplication,
                    Report,
                    Repeatˉarguments,
                    loadedˉmodules: Loadedˉmodules));
                Equal(0, Loadedˉmodules.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            if (OperatingSystem.IsLinux())
            {
                var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Nativeˉapplication,
                    Report,
                    Repeatˉarguments,
                    loadedˉmappings: Loadedˉmappings));
                Equal(0, Loadedˉmappings.Count(Name =>
                    Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            }
            Sequenceˉequal(Expected, File.ReadAllBytes(Repeatˉpath));

            var Invalidˉentryˉerror =
                "uefi-package status=Invalidˉentry native-image-bytes=6 " +
                "entry-offset=6 application-bytes=0\n";
            var Rejected = Runˉnativeˉwvbˉtool(
                Repository,
                "Package-Uefi",
                Inputˉpath,
                "6",
                Rejectedˉpath);
            Equal(2, Rejected.Exitˉcode);
            Equal(string.Empty, Rejected.Output);
            Equal(Invalidˉentryˉerror, Rejected.Error);
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Rejectedˉpath));

            var Nativeˉpath = Path.Combine(Directoryˉpath, "Uefi-Packager.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Uefi-Application-Packager.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Nativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
