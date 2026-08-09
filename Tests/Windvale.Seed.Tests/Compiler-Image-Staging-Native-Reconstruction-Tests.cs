using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Compilerˉimageˉstagingˉreconstructsˉnativeˉimage()
    {
        var Repository = Findˉrepositoryˉroot();
        var Candidateˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Segmented-Compiler-Toolset-Candidate");
        Requireˉstage0ˉrecoveryˉtarget(
            Path.Combine(Candidateˉroot, "Wvo-Staging-Producer.wvb"),
            OperatingSystem.IsWindows()
                ? Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_TARGET_NAME);
        Requireˉstage0ˉrecoveryˉtarget(
            Path.Combine(Candidateˉroot, "Compiler-Image-Staging.wvb"),
            OperatingSystem.IsWindows()
                ? Compilerˉimageˉstagingˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Compilerˉimageˉstagingˉapplicationˉcontract.LINUX_TARGET_NAME);
        var Producerˉapplication = Readˉcurrentˉhostˉsegmentedˉcandidate(
            Repository,
            "windows-x64-wvstage.exe",
            "linux-x64-wvstage.elf",
            Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_APPLICATION_SHA256);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-compiler-image-native-reconstruction-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Toolˉpath = Path.Combine(Directoryˉpath, "staging.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Compiler-Image-Staging.wvproj"),
                Toolˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            var Toolˉbytes = File.ReadAllBytes(Toolˉpath);
            Equal(
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES,
                Toolˉbytes.Length);
            Equal(
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256,
                Moduleˉdigest.Calculateˉsha256(Toolˉbytes));

            var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
            var Stageˉzero = X64ˉnativeˉbackend.Compile(Tool);
            _ = Nativeˉfragmentˉverifier.Verify(Stageˉzero.Fragment);
            var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(
                Stageˉzero.Fragment);
            var Expectedˉlink = Linkˉsuccess(
                [Expectedˉobject.ToArray()],
                new(0, "Main"));
            var Stagingˉapplication = Readˉcurrentˉhostˉsegmentedˉcandidate(
                Repository,
                "windows-x64-wvlinkstage.exe",
                "linux-x64-wvlinkstage.elf",
                Compilerˉimageˉstagingˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Compilerˉimageˉstagingˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
                Compilerˉimageˉstagingˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Compilerˉimageˉstagingˉapplicationˉcontract.LINUX_APPLICATION_SHA256);

            var Objectˉprefix = Path.Combine(Directoryˉpath, "object");
            var Objectˉmanifest = Path.Combine(
                Directoryˉpath,
                "object.wvop");
            var Imageˉprefix = Path.Combine(Directoryˉpath, "image");
            var Imageˉmanifest = Path.Combine(
                Directoryˉpath,
                "image.wvli");
            var Loaded = new HashSet<string>(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);
            Equal(
                0,
                Executeˉwvoˉstagingˉapplication(
                    Producerˉapplication,
                    [Toolˉpath, Objectˉprefix, Objectˉmanifest],
                    Loaded,
                    expectedˉoutput: null));
            var Producedˉobject = Readˉstagedˉsequence(
                Objectˉprefix,
                Objectˉmanifest,
                "WVOP"u8.ToArray(),
                24,
                16,
                62);
            Sequenceˉequal(Expectedˉobject, Producedˉobject);
            _ = Objectˉcodec.Readˉandˉverify(Producedˉobject);
            True(
                Readˉstagedˉcount(Objectˉmanifest, 16) <= 62,
                "The compiler-image staging WVO exceeded the immutable snapshot table.");

            Equal(
                0,
                Executeˉwvoˉstagingˉapplication(
                    Stagingˉapplication,
                    [
                        Objectˉprefix,
                        Objectˉmanifest,
                        Imageˉprefix,
                        Imageˉmanifest,
                    ],
                    Loaded,
                    expectedˉoutput: null));
            var Producedˉimage = Readˉstagedˉsequence(
                Imageˉprefix,
                Imageˉmanifest,
                "WVLI"u8.ToArray(),
                28,
                20,
                62);
            Sequenceˉequal(Expectedˉlink.Imageˉbytes, Producedˉimage);
            Equal(
                Expectedˉlink.Entryˉaddress,
                BinaryPrimitives.ReadUInt32LittleEndian(
                    File.ReadAllBytes(Imageˉmanifest).AsSpan(16)));
            Requireˉnoˉdotnetˉmodules(Loaded);
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static void Requireˉstage0ˉrecoveryˉtarget(
        string moduleˉpath,
        string target)
    {
        var Ordinary = Executeˉinspectorˉtool(
            "aot",
            moduleˉpath,
            "--target",
            target);
        Equal(64, Ordinary.Exitˉcode);
        Equal(string.Empty, Ordinary.Standardˉoutput);
        Contains(
            Ordinary.Standardˉerror,
            $"The {target} AOT target is Stage 0 recovery-only;");
    }

    private static ImmutableArray<byte> Readˉcurrentˉhostˉsegmentedˉcandidate(
        string repository,
        string windowsˉname,
        string linuxˉname,
        int windowsˉbytes,
        string windowsˉsha256,
        int linuxˉbytes,
        string linuxˉsha256)
    {
        var Windows = OperatingSystem.IsWindows();
        var Pathˉvalue = Path.Combine(
            repository,
            "Artifacts",
            "Native-Segmented-Compiler-Toolset-Candidate",
            Windows ? windowsˉname : linuxˉname);
        var Bytes = File.ReadAllBytes(Pathˉvalue).ToImmutableArray();
        Equal(Windows ? windowsˉbytes : linuxˉbytes, Bytes.Length);
        Equal(
            Windows ? windowsˉsha256 : linuxˉsha256,
            Moduleˉdigest.Calculateˉsha256(Bytes.AsSpan()));
        return Bytes;
    }

    private static uint Readˉstagedˉcount(string manifestˉpath, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(
            File.ReadAllBytes(manifestˉpath).AsSpan(offset));

    private static byte[] Readˉstagedˉsequence(
        string prefix,
        string manifestˉpath,
        byte[] magic,
        int headerˉbytes,
        int countˉoffset,
        uint maximumˉchunks)
    {
        var Manifest = File.ReadAllBytes(manifestˉpath);
        True(
            Manifest.Length >= headerˉbytes,
            "The staged manifest is truncated.");
        Sequenceˉequal(magic, Manifest[..magic.Length]);
        var Totalˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
            Manifest.AsSpan(12));
        var Count = BinaryPrimitives.ReadUInt32LittleEndian(
            Manifest.AsSpan(countˉoffset));
        True(
            Totalˉbytes is > 0 and <= 33_554_432,
            "The staged value length is outside its bounded profile.");
        True(
            Count > 0 && Count <= maximumˉchunks,
            "The staged chunk count is outside its bounded profile.");
        Equal(
            checked(headerˉbytes + (int)Count * 12),
            Manifest.Length);
        var Result = new byte[checked((int)Totalˉbytes)];
        var Position = 0;
        for (uint Index = 0; Index < Count; Index++)
        {
            var Entry = checked(headerˉbytes + (int)Index * 12);
            Equal(Index, BinaryPrimitives.ReadUInt32LittleEndian(
                Manifest.AsSpan(Entry)));
            Equal((uint)Position, BinaryPrimitives.ReadUInt32LittleEndian(
                Manifest.AsSpan(Entry + 4)));
            var Length = BinaryPrimitives.ReadUInt32LittleEndian(
                Manifest.AsSpan(Entry + 8));
            True(
                Length is > 0 and <= 4_194_304 &&
                    Length <= checked((uint)(Result.Length - Position)),
                "The staged chunk length is outside its bounded profile.");
            var Chunk = File.ReadAllBytes($"{prefix}.chunk-{Index}");
            Equal(checked((int)Length), Chunk.Length);
            Chunk.CopyTo(Result, Position);
            Position = checked(Position + Chunk.Length);
        }
        Equal(Result.Length, Position);
        return Result;
    }
}
