using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉorchestrationˉcontrolˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));
        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source("Runtime/Windvale/Native-Hosted-Orchestration-Control-Tool.wv",
                "Native-Hosted-Orchestration-Control-Tool.wv"),
            [
                Source("Foundation/Decimal-Parsing.wv", "Decimal-Parsing.wv"),
                Source("Foundation/Immutable-Source-Regions.wv",
                    "Immutable-Source-Regions.wv"),
                Source("Runtime/Windvale/Native-Hosted-Orchestration-Control-Core.wv",
                    "Native-Hosted-Orchestration-Control-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(Hostedˉorchestrationˉcontrolˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(Hostedˉorchestrationˉcontrolˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));
        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native);
        var Windows =
            Hostedˉorchestrationˉcontrolˉapplicationˉwriter.Writeˉwindows(
                Native, Module.Module.Capabilities, Module.Module.Name);
        var Linux = Hostedˉorchestrationˉcontrolˉapplicationˉwriter.Writeˉlinux(
            Native, Module.Module.Capabilities, Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Equal(
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(Path.GetTempPath(),
            $"windvale-hosted-orchestration-control-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Control.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository,
                    "Windvale-Native-Hosted-Orchestration-Control-Tool.wvproj"),
                Moduleˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Moduleˉpath));

            var Objectˉpath = Path.Combine(Directoryˉpath, "Control.wvo");
            var Lower = Runˉnativeˉwvbˉtool(
                Repository, "Lower-Wvb-To-Wvo", Moduleˉpath, Objectˉpath);
            Equal(0, Lower.Exitˉcode);
            Equal(
                "native x64 status=Valid abi=22 code-bytes=217968 " +
                    "object-bytes=219635\n",
                Lower.Output);
            Equal(string.Empty, Lower.Error);
            Sequenceˉequal(Expectedˉobject, File.ReadAllBytes(Objectˉpath));

            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉorchestrationˉcontrolˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉorchestrationˉcontrolˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cli = Executeˉinspectorˉtool(
                "aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cli.Exitˉcode);
            Equal(string.Empty, Cli.Standardˉerror);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes : Linux.Imageˉbytes;
            Sequenceˉequal(
                Application,
                File.ReadAllBytes(Path.ChangeExtension(Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Target = OperatingSystem.IsWindows() ? 1u : 2u;
            var Bundle = Hostedˉtoolˉtestˉbundle(
                OperatingSystem.IsWindows()
                    ? Consoleˉapplicationˉtarget.Windowsˉx64
                    : Consoleˉapplicationˉtarget.Linuxˉx64);
            var Sources = Buildˉorchestrationˉsourceˉgeometry(Bundle);
            var Expectedˉevidence = Buildˉorchestrationˉevidence(Sources);
            var Expectedˉmetadata = Buildˉorchestrationˉmetadata(Target);
            var Sourcesˉpath = Path.Combine(Directoryˉpath, "Sources.wvsg");
            var Evidenceˉpath = Path.Combine(Directoryˉpath, "Evidence.wvhs");
            var Metadataˉpath = Path.Combine(Directoryˉpath, "Metadata.wvmi");
            File.WriteAllBytes(Sourcesˉpath, Sources);
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(0, Executeˉhostedˉorchestrationˉcontrol(
                Application,
                ["evidence", Sourcesˉpath, Evidenceˉpath],
                "hosted orchestration control status=Valid kind=evidence bytes=384\n",
                Loaded));
            Sequenceˉequal(Expectedˉevidence, File.ReadAllBytes(Evidenceˉpath));
            Equal(0, Executeˉhostedˉorchestrationˉcontrol(
                Application,
                ["metadata", OperatingSystem.IsWindows() ? "windows" : "linux",
                    "1", "0", Metadataˉpath],
                "hosted orchestration control status=Valid kind=metadata bytes=32\n",
                Loaded));
            Sequenceˉequal(Expectedˉmetadata, File.ReadAllBytes(Metadataˉpath));
            Equal(0, Loaded.Count(Name => Name.Contains("clr",
                StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x43, 0x54];
            Equal(2, Executeˉhostedˉorchestrationˉcontrol(
                Application,
                ["evidence", Sourcesˉpath, Sourcesˉpath],
                string.Empty,
                expectedˉerror:
                    "hosted orchestration control status=Rejected\n"));
            Sequenceˉequal(Sources, File.ReadAllBytes(Sourcesˉpath));

            var Badˉsources = Sources.ToArray();
            Badˉsources[0] ^= 0x01;
            File.WriteAllBytes(Sourcesˉpath, Badˉsources);
            File.WriteAllBytes(Evidenceˉpath, Sentinel);
            Equal(2, Executeˉhostedˉorchestrationˉcontrol(
                Application,
                ["evidence", Sourcesˉpath, Evidenceˉpath],
                string.Empty,
                expectedˉerror:
                    "hosted orchestration control status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Evidenceˉpath));

            File.WriteAllBytes(Metadataˉpath, Sentinel);
            Equal(2, Executeˉhostedˉorchestrationˉcontrol(
                Application,
                ["metadata", "windows", "8", "0", Metadataˉpath],
                string.Empty,
                expectedˉerror:
                    "hosted orchestration control status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Metadataˉpath));
        }
        finally { Directory.Delete(Directoryˉpath, recursive: true); }
    }

    private static byte[] Buildˉorchestrationˉsourceˉgeometry(
        Nativeˉserviceˉbundle bundle)
    {
        var Regions = new (int Image, ImmutableArray<byte> Bytes)[11];
        Regions[0] = (0, bundle.Imageˉbytes[..bundle.Nativeˉimageˉbytes]);
        for (var Index = 0; Index < bundle.Placements.Length; Index++)
        {
            var Placement = bundle.Placements[Index];
            Regions[Index + 1] = (
                Placement.Imageˉoffset,
                bundle.Imageˉbytes[Placement.Imageˉoffset..
                    (Placement.Imageˉoffset + Placement.Codeˉbytes)]);
        }
        var Logicalˉbytes = Regions.Sum(Region => Region.Bytes.Length);
        var Result = new byte[32 + Regions.Length * 20 + Regions.Length * 16];
        void Write(int offset, uint value) =>
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
        Write(0, 0x4753_5657);
        Write(4, 1);
        Write(8, (uint)Result.Length);
        Write(12, (uint)Regions.Length);
        Write(16, (uint)Regions.Length);
        Write(20, (uint)Logicalˉbytes);
        Write(24, (uint)bundle.Imageˉbytes.Length);
        var Logical = 0;
        for (var Index = 0; Index < Regions.Length; Index++)
        {
            var Chunk = 32 + Index * 20;
            Write(Chunk, (uint)Index);
            Write(Chunk + 4, (uint)Logical);
            Write(Chunk + 12, (uint)Regions[Index].Bytes.Length);
            Write(Chunk + 16, (uint)Regions[Index].Bytes.Length);
            var Region = 32 + Regions.Length * 20 + Index * 16;
            Write(Region, (uint)Index);
            Write(Region + 4, (uint)Logical);
            Write(Region + 8, (uint)Regions[Index].Image);
            Write(Region + 12, (uint)Regions[Index].Bytes.Length);
            Logical += Regions[Index].Bytes.Length;
        }
        Equal(Logicalˉbytes, Logical);
        return Result;
    }

    private static byte[] Buildˉorchestrationˉevidence(byte[] sources)
    {
        var Chunks = (int)BinaryPrimitives.ReadUInt32LittleEndian(sources.AsSpan(12));
        var Regions = (int)BinaryPrimitives.ReadUInt32LittleEndian(sources.AsSpan(16));
        var Result = new byte[32 + Chunks * 20 + Regions * 12];
        void Write(int offset, uint value) =>
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
        Write(0, 0x5348_5657);
        Write(4, 1);
        Write(8, (uint)Result.Length);
        Write(12, (uint)Chunks);
        Write(16, (uint)Regions);
        Write(20, BinaryPrimitives.ReadUInt32LittleEndian(sources.AsSpan(20)));
        sources.AsSpan(32, Chunks * 20).CopyTo(Result.AsSpan(32));
        for (var Index = 0; Index < Regions; Index++)
        {
            var Source = 32 + Chunks * 20 + Index * 16;
            var Target = 32 + Chunks * 20 + Index * 12;
            Write(Target, (uint)Index);
            Write(Target + 4,
                BinaryPrimitives.ReadUInt32LittleEndian(sources.AsSpan(Source + 4)));
            Write(Target + 8,
                BinaryPrimitives.ReadUInt32LittleEndian(sources.AsSpan(Source + 12)));
        }
        return Result;
    }

    private static byte[] Buildˉorchestrationˉmetadata(uint target)
    {
        var Result = new byte[32];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x494D_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), 32);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), target);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), 1);
        return Result;
    }

    private static int Executeˉhostedˉorchestrationˉcontrol(
        ImmutableArray<byte> application,
        IReadOnlyList<string> arguments,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") => OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(application, expectedˉoutput,
                arguments, loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(application, expectedˉoutput,
                arguments, loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
