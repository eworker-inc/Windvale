using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉstreamingˉsha256ˉevidenceˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Streaming-Sha256-Evidence-Tool.wv",
                "Native-Streaming-Sha256-Evidence-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Foundation/Sha256-Compression.wv",
                    "Sha256-Compression.wv"),
                Source(
                    "Foundation/Sha256-Streaming.wv",
                    "Sha256-Streaming.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.MODULE_NAME,
            Module.Module.Name);
        Sequenceˉequal(
            [
                "console.write_line",
                "diagnostic.write_line",
                "file.read_bytes",
                "file.write_bytes",
                "process.argument",
                "process.argument_count",
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
                Nativeˉservice.Fileˉwriteˉbytes,
            ],
            Native.Requiredˉservices);

        var Windows =
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉwriter.Writeˉwindows(
                Native,
                Module.Module.Capabilities,
                Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
                .WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
                .WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux =
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉwriter.Writeˉlinux(
                Native,
                Module.Module.Capabilities,
                Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
                .LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
                .LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        const int Firstˉlogicalˉbytes = 4_194_144;
        const int Secondˉlogicalˉbytes = 257;
        var First = new byte[40 + Firstˉlogicalˉbytes];
        var Second = new byte[40 + Secondˉlogicalˉbytes];
        for (var Index = 0; Index < Firstˉlogicalˉbytes; Index++)
        {
            First[40 + Index] = (byte)((Index * 37 + 11) & 0xFF);
        }
        for (var Index = 0; Index < Secondˉlogicalˉbytes; Index++)
        {
            Second[40 + Index] = (byte)((Index * 19 + 7) & 0xFF);
        }
        var Logical = First.AsSpan(40).ToArray()
            .Concat(Second.AsSpan(40).ToArray())
            .ToArray();
        int[] Regionˉbytes = [4_194_200, 100, 101];
        var Manifest = Buildˉstreamingˉsha256ˉmanifest(
            [(40, Firstˉlogicalˉbytes, First.Length),
             (40, Secondˉlogicalˉbytes, Second.Length)],
            Regionˉbytes);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-streaming-sha256-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Streaming-Sha256.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Target = OperatingSystem.IsWindows()
                ? Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
                    .WINDOWS_TARGET_NAME
                : Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
                    .LINUX_TARGET_NAME;
            var Cli = Executeˉinspectorˉtool("aot", Moduleˉpath, "--target", Target);
            Equal(0, Cli.Exitˉcode);
            Equal(string.Empty, Cli.Standardˉerror);
            Contains(Cli.Standardˉoutput, $"Target: {Target}");

            var Manifestˉpath = Path.Combine(Directoryˉpath, "Sequence.wvhs");
            var Prefix = Path.Combine(Directoryˉpath, "Bundle");
            var Evidenceˉpath = Path.Combine(Directoryˉpath, "Evidence.wvhe");
            File.WriteAllBytes(Manifestˉpath, Manifest);
            File.WriteAllBytes(Prefix + ".chunk-0", First);
            File.WriteAllBytes(Prefix + ".chunk-1", Second);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉstreamingˉsha256ˉevidence(
                    Application,
                    Manifestˉpath,
                    Prefix,
                    Evidenceˉpath,
                    "streaming sha256 evidence status=Valid bytes=196\n",
                    Loaded,
                    timeoutˉmilliseconds: 60_000));
            var Evidence = File.ReadAllBytes(Evidenceˉpath);
            Equal(196, Evidence.Length);
            Equal(0x4548_5657u, BinaryPrimitives.ReadUInt32LittleEndian(Evidence));
            Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(4)));
            Equal(196u, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(8)));
            Equal((uint)Logical.Length,
                BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(12)));
            Equal(3u, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(16)));
            Equal((uint)Manifest.Length,
                BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(20)));
            Sequenceˉequal(
                SHA256.HashData(Manifest),
                Evidence.AsSpan(24, 32).ToArray());
            var Regionˉoffset = 0;
            for (var Index = 0; Index < Regionˉbytes.Length; Index++)
            {
                var Record = 64 + Index * 44;
                Equal((uint)Index,
                    BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(Record)));
                Equal((uint)Regionˉoffset,
                    BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(Record + 4)));
                Equal((uint)Regionˉbytes[Index],
                    BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(Record + 8)));
                Sequenceˉequal(
                    SHA256.HashData(Logical.AsSpan(Regionˉoffset, Regionˉbytes[Index])),
                    Evidence.AsSpan(Record + 12, 32).ToArray());
                Regionˉoffset += Regionˉbytes[Index];
            }
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x48, 0x45];
            var Changed = Manifest.ToArray();
            Changed[0] ^= 0x01;
            File.WriteAllBytes(Manifestˉpath, Changed);
            File.WriteAllBytes(Evidenceˉpath, Sentinel);
            Equal(
                2,
                Executeˉstreamingˉsha256ˉevidence(
                    Application,
                    Manifestˉpath,
                    Prefix,
                    Evidenceˉpath,
                    string.Empty,
                    expectedˉerror: "streaming sha256 evidence status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Evidenceˉpath));

            var Nativeˉoutput = Path.Combine(Directoryˉpath, "Native-Build.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Streaming-Sha256-Evidence-Tool.wvproj"),
                Nativeˉoutput);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Nativeˉoutput));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Buildˉstreamingˉsha256ˉmanifest(
        (int Resourceˉoffset, int Logicalˉbytes, int Resourceˉbytes)[] chunks,
        int[] regions)
    {
        var Result = new byte[32 + chunks.Length * 20 + regions.Length * 12];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x5348_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)Result.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)chunks.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), (uint)regions.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(20),
            (uint)chunks.Sum(Chunk => Chunk.Logicalˉbytes));
        var Logicalˉoffset = 0;
        for (var Index = 0; Index < chunks.Length; Index++)
        {
            var Record = 32 + Index * 20;
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Record), (uint)Index);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 4), (uint)Logicalˉoffset);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 8), (uint)chunks[Index].Resourceˉoffset);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 12), (uint)chunks[Index].Logicalˉbytes);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 16), (uint)chunks[Index].Resourceˉbytes);
            Logicalˉoffset += chunks[Index].Logicalˉbytes;
        }
        var Regionˉoffset = 0;
        for (var Index = 0; Index < regions.Length; Index++)
        {
            var Record = 32 + chunks.Length * 20 + Index * 12;
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Record), (uint)Index);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 4), (uint)Regionˉoffset);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 8), (uint)regions[Index]);
            Regionˉoffset += regions[Index];
        }
        return Result;
    }

    private static int Executeˉstreamingˉsha256ˉevidence(
        ImmutableArray<byte> application,
        string manifest,
        string prefix,
        string evidence,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "",
        int timeoutˉmilliseconds = 10_000) =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [manifest, prefix, evidence],
                timeoutˉmilliseconds,
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [manifest, prefix, evidence],
                timeoutˉmilliseconds,
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
