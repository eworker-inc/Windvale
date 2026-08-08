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
    private static void Nativeˉhostedˉsourceˉgeometryˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));
        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source("Runtime/Windvale/Native-Hosted-Source-Geometry-Tool.wv",
                "Native-Hosted-Source-Geometry-Tool.wv"),
            [
                Source("Foundation/Decimal-Parsing.wv", "Decimal-Parsing.wv"),
                Source("Foundation/Immutable-Source-Regions.wv",
                    "Immutable-Source-Regions.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(Hostedˉsourceˉgeometryˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(Hostedˉsourceˉgeometryˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));
        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        var Windows = Hostedˉsourceˉgeometryˉapplicationˉwriter.Writeˉwindows(
            Native, Module.Module.Capabilities, Module.Module.Name);
        var Linux = Hostedˉsourceˉgeometryˉapplicationˉwriter.Writeˉlinux(
            Native, Module.Module.Capabilities, Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(Hostedˉsourceˉgeometryˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        Equal(Hostedˉsourceˉgeometryˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Fragment = Bundle.Imageˉbytes[..Bundle.Nativeˉimageˉbytes];
        var Services = Bundle.Placements.Select(Placement => Bundle.Imageˉbytes[
            Placement.Imageˉoffset..(Placement.Imageˉoffset + Placement.Codeˉbytes)])
            .ToImmutableArray();
        var Directoryˉpath = Path.Combine(Path.GetTempPath(),
            $"windvale-hosted-source-geometry-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Geometry.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉsourceˉgeometryˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉsourceˉgeometryˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cli = Executeˉinspectorˉtool(
                "aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cli.Exitˉcode);
            Equal(string.Empty, Cli.Standardˉerror);
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Prefix = Path.Combine(Directoryˉpath, "Source");
            File.WriteAllBytes(Prefix + ".chunk-0", Fragment.AsSpan());
            for (var Index = 0; Index < Services.Length; Index++)
            {
                File.WriteAllBytes(Prefix + $".chunk-{Index + 1}",
                    Services[Index].AsSpan());
            }
            var Output = Path.Combine(Directoryˉpath, "Sources.wvsg");
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(0, Executeˉhostedˉsourceˉgeometry(Application, Prefix, "1", Output,
                "hosted source geometry status=Valid chunks=11 bytes=428\n", Loaded));
            var Manifest = File.ReadAllBytes(Output);
            uint Read(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(
                Manifest.AsSpan(offset));
            Equal(0x4753_5657u, Read(0));
            Equal(428u, Read(8));
            Equal(11u, Read(12));
            Equal(11u, Read(16));
            Equal((uint)(Fragment.Length + Services.Sum(Value => Value.Length)), Read(20));
            var Imageˉcursor = (uint)Fragment.Length;
            var Logicalˉcursor = (uint)Fragment.Length;
            Equal(0u, Read(252 + 8));
            Equal((uint)Fragment.Length, Read(252 + 12));
            for (var Index = 0; Index < Services.Length; Index++)
            {
                Imageˉcursor = (Imageˉcursor + 15u) / 16u * 16u;
                var Record = 252 + (Index + 1) * 16;
                Equal((uint)(Index + 1), Read(Record));
                Equal(Logicalˉcursor, Read(Record + 4));
                Equal(Imageˉcursor, Read(Record + 8));
                Equal((uint)Services[Index].Length, Read(Record + 12));
                Logicalˉcursor += (uint)Services[Index].Length;
                Imageˉcursor += (uint)Services[Index].Length;
            }
            Equal(Imageˉcursor, Read(24));
            Equal(0, Loaded.Count(Name => Name.Contains("clr",
                StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x53, 0x47];
            File.WriteAllBytes(Prefix + ".chunk-10", []);
            File.WriteAllBytes(Output, Sentinel);
            Equal(2, Executeˉhostedˉsourceˉgeometry(Application, Prefix, "1", Output,
                string.Empty, expectedˉerror:
                    "hosted source geometry status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Output));

            var Repository = Findˉrepositoryˉroot();
            var Frontˉdoor = Path.Combine(Directoryˉpath, "Geometry.wvb");
            var Build = Runˉnativeˉfrontˉdoor(Repository,
                Path.Combine(Repository,
                    "Windvale-Native-Hosted-Source-Geometry-Tool.wvproj"), Frontˉdoor);
            Equal(0, Build.Exitˉcode);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Frontˉdoor));
        }
        finally { Directory.Delete(Directoryˉpath, recursive: true); }
    }

    private static int Executeˉhostedˉsourceˉgeometry(
        ImmutableArray<byte> application, string prefix, string chunks,
        string output, string expectedˉoutput, ISet<string>? loaded = null,
        string expectedˉerror = "") => OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(application, expectedˉoutput,
                [prefix, chunks, output], loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(application, expectedˉoutput,
                [prefix, chunks, output], loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
