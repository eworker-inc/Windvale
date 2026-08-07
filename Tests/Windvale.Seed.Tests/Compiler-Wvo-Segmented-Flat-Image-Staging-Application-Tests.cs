using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Compilerˉwvoˉsegmentedˉflatˉimageˉapplicationˉruns()
    {
        var Toolˉbytes = Compileˉcompilerˉwvoˉsegmentedˉstagingˉtool();
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        var Native = X64ˉnativeˉbackend.Compile(Tool);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);

        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES,
            Toolˉbytes.Length);
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
        var Windows =
            Compilerˉimageˉstagingˉapplicationˉwriter.Writeˉwindows(
                Tool,
                Native.Fragment,
                Toolˉbytes);
        True(
            Windows.Success,
            "The Windows compiler-image staging package was rejected: " +
                string.Join(" | ", Windows.Diagnostics));
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract
                .WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract
                .WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));
        var Linux = Compilerˉimageˉstagingˉapplicationˉwriter.Writeˉlinux(
            Tool,
            Native.Fragment,
            Toolˉbytes);
        True(
            Linux.Success,
            "The Linux compiler-image staging package was rejected: " +
                string.Join(" | ", Linux.Diagnostics));
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract
                .LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Compilerˉimageˉstagingˉapplicationˉcontract
                .LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Fixture = Buildˉcompilerˉimageˉapplicationˉfixture();
        Assertˉcurrentˉhostˉcompilerˉimageˉapplication(
            OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes,
            Fixture.Manifest,
            Fixture.Sourceˉchunks,
            Fixture.Expectedˉchunks);
    }

    private sealed record Compilerˉimageˉapplicationˉfixture(
        byte[] Manifest,
        byte[][] Sourceˉchunks,
        byte[][] Expectedˉchunks);

    private static Compilerˉimageˉapplicationˉfixture
        Buildˉcompilerˉimageˉapplicationˉfixture()
    {
        var Manifest = Buildˉstagingˉmanifest(49, 10, 6, 27, 2, 3, 118, 40);
        var Prefix = Buildˉstagingˉwvoˉprefix(2, 4, 2, 16);
        var Readˉonlyˉheader = Buildˉstagingˉwvoˉreadˉonlyˉheader(5);
        var Symbols = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉsymbol(1, 2, 1, 0, 2, "$data_0000"),
            Buildˉstagingˉwvoˉsymbol(1, 2, 1, 2, 3, "$data_0001"),
            Buildˉstagingˉwvoˉsymbol(1, 1, 0, 0, 4, "$function_0000"),
            Buildˉstagingˉwvoˉsymbol(2, 1, 0, 4, 6, "Main"));
        var Relocations = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉrelocation(1, 0),
            Buildˉstagingˉwvoˉrelocation(6, 1));
        return new(
            Manifest,
            [
                Prefix,
                [1, 0, 0, 0, 0, 2, 0, 0, 0, 0],
                [144, 144, 144, 144, 144, 144],
                Readˉonlyˉheader,
                [65, 66],
                [67, 68, 69],
                Symbols,
                Relocations,
            ],
            [
                [1, 11, 0, 0, 0, 2, 8, 0, 0, 0],
                [144, 144, 144, 144, 144, 144],
                [65, 66],
                [67, 68, 69],
            ]);
    }

    private static void Assertˉcurrentˉhostˉcompilerˉimageˉapplication(
        ImmutableArray<byte> application,
        byte[] manifest,
        byte[][] sourceˉchunks,
        byte[][] expectedˉchunks)
    {
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-compiler-image-staging-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Sourceˉprefix = Path.Combine(Directoryˉpath, "compiler");
            var Sourceˉmanifest = Path.Combine(
                Directoryˉpath,
                "compiler.wvop");
            var Outputˉprefix = Path.Combine(Directoryˉpath, "image");
            var Outputˉmanifest = Path.Combine(Directoryˉpath, "image.wvli");
            File.WriteAllBytes(Sourceˉmanifest, manifest);
            Writeˉstagingˉchunks(Sourceˉprefix, sourceˉchunks);
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var Arguments = new[]
            {
                Sourceˉprefix,
                Sourceˉmanifest,
                Outputˉprefix,
                Outputˉmanifest,
            };
            var Expectedˉoutput =
                "segmented compiler image staging status=Complete " +
                "image-bytes=21 chunks=4 manifest-bytes=76\n";
            var Exitˉcode = OperatingSystem.IsWindows()
                ? Executeˉwindowsˉapplication(
                    application,
                    Expectedˉoutput,
                    Arguments,
                    timeoutˉmilliseconds: 60_000,
                    loadedˉmodules: Loaded)
                : Executeˉlinuxˉapplication(
                    application,
                    Expectedˉoutput,
                    Arguments,
                    timeoutˉmilliseconds: 60_000,
                    loadedˉmappings: Loaded);
            Equal(0, Exitˉcode);
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            Sequenceˉequal(manifest, File.ReadAllBytes(Sourceˉmanifest));
            for (var Index = 0; Index < sourceˉchunks.Length; Index++)
            {
                Sequenceˉequal(
                    sourceˉchunks[Index],
                    File.ReadAllBytes($"{Sourceˉprefix}.chunk-{Index}"));
            }
            for (var Index = 0; Index < expectedˉchunks.Length; Index++)
            {
                Sequenceˉequal(
                    expectedˉchunks[Index],
                    File.ReadAllBytes($"{Outputˉprefix}.chunk-{Index}"));
            }
            var Imageˉmanifest = File.ReadAllBytes(Outputˉmanifest);
            Equal(76, Imageˉmanifest.Length);
            Sequenceˉequal("WVLI"u8.ToArray(), Imageˉmanifest[..4]);
            Equal(
                21u,
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Imageˉmanifest.AsSpan(12)));
            Equal(
                4u,
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Imageˉmanifest.AsSpan(16)));
            Equal(
                4u,
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Imageˉmanifest.AsSpan(20)));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
