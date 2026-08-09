using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Compilerˉimageˉtransportˉcanonicalizesˉchunks()
    {
        var Repository = Findˉrepositoryˉroot();
        var Candidateˉroot = Path.Combine(
            Repository,
            "Artifacts",
            "Native-Segmented-Compiler-Toolset-Candidate");
        var Moduleˉbytes = File.ReadAllBytes(Path.Combine(
            Candidateˉroot,
            "Compiler-Image-Canonical-Transport.wvb"));
        Equal(
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_BYTES,
            Moduleˉbytes.Length);
        Equal(
            Compilerˉimageˉtransportˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
        var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
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
            Fragment.Requiredˉservices);

        var Windows = OperatingSystem.IsWindows();
        var Target = Windows
            ? Compilerˉimageˉtransportˉapplicationˉcontract.WINDOWS_TARGET_NAME
            : Compilerˉimageˉtransportˉapplicationˉcontract.LINUX_TARGET_NAME;
        var Ordinary = Executeˉinspectorˉtool(
            "aot",
            Path.Combine(Candidateˉroot, "Compiler-Image-Canonical-Transport.wvb"),
            "--target",
            Target);
        Equal(64, Ordinary.Exitˉcode);
        Contains(Ordinary.Standardˉerror, "AOT target is Stage 0 recovery-only;");
        var Applicationˉpath = Path.Combine(
            Candidateˉroot,
            Windows
                ? "windows-x64-wvimagetransport.exe"
                : "linux-x64-wvimagetransport.elf");
        var Application = File.ReadAllBytes(Applicationˉpath).ToImmutableArray();
        Equal(
            Windows
                ? Compilerˉimageˉtransportˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES
                : Compilerˉimageˉtransportˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Application.Length);
        Equal(
            Windows
                ? Compilerˉimageˉtransportˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256
                : Compilerˉimageˉtransportˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Moduleˉdigest.Calculateˉsha256(Application.AsSpan()));

        byte[][] Sourceˉchunks =
        [
            Buildˉtransportˉchunk(1_500_000, 11),
            Buildˉtransportˉchunk(2_700_000, 37),
            Buildˉtransportˉchunk(23, 71),
        ];
        var Imageˉbytes = Sourceˉchunks.Sum(Item => Item.Length);
        const uint Entryˉoffset = 1_234_567;
        var Sourceˉmanifestˉbytes = Buildˉtransportˉmanifest(
            Sourceˉchunks.Select(Item => Item.Length).ToArray(),
            Entryˉoffset);
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-compiler-image-transport-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Sourceˉprefix = Path.Combine(Directoryˉpath, "source");
            var Sourceˉmanifest = Path.Combine(Directoryˉpath, "source.wvli");
            var Outputˉprefix = Path.Combine(Directoryˉpath, "output");
            var Outputˉmanifest = Path.Combine(Directoryˉpath, "output.wvli");
            File.WriteAllBytes(Sourceˉmanifest, Sourceˉmanifestˉbytes);
            for (var Index = 0; Index < Sourceˉchunks.Length; Index++)
            {
                File.WriteAllBytes($"{Sourceˉprefix}.chunk-{Index}", Sourceˉchunks[Index]);
            }
            var Loaded = new HashSet<string>(
                Windows ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            Equal(
                0,
                Executeˉwvoˉstagingˉapplication(
                    Application,
                    [Sourceˉprefix, Sourceˉmanifest, Outputˉprefix, Outputˉmanifest],
                    Loaded,
                    $"compiler image transport status=Complete " +
                    $"image-bytes={Imageˉbytes} entry-offset={Entryˉoffset} " +
                    "chunks=2 manifest-bytes=52\n"));
            Requireˉnoˉdotnetˉmodules(Loaded);

            var First = File.ReadAllBytes($"{Outputˉprefix}.chunk-0");
            var Final = File.ReadAllBytes($"{Outputˉprefix}.chunk-1");
            Equal(4_194_304, First.Length);
            Equal(Imageˉbytes - First.Length, Final.Length);
            var Expected = Sourceˉchunks.SelectMany(Item => Item).ToArray();
            Sequenceˉequal(Expected.AsSpan(0, First.Length).ToArray(), First);
            Sequenceˉequal(Expected.AsSpan(First.Length).ToArray(), Final);
            var Outputˉmanifestˉbytes = File.ReadAllBytes(Outputˉmanifest);
            Equal(52, Outputˉmanifestˉbytes.Length);
            Equal((uint)Imageˉbytes, BinaryPrimitives.ReadUInt32LittleEndian(
                Outputˉmanifestˉbytes.AsSpan(12)));
            Equal(Entryˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(
                Outputˉmanifestˉbytes.AsSpan(16)));
            Equal(2u, BinaryPrimitives.ReadUInt32LittleEndian(
                Outputˉmanifestˉbytes.AsSpan(20)));
            Equal(4_194_304u, BinaryPrimitives.ReadUInt32LittleEndian(
                Outputˉmanifestˉbytes.AsSpan(36)));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Buildˉtransportˉchunk(int length, int seed)
    {
        var Result = new byte[length];
        for (var Index = 0; Index < Result.Length; Index++)
        {
            Result[Index] = checked((byte)((Index + seed) % 251));
        }
        return Result;
    }

    private static byte[] Buildˉtransportˉmanifest(
        IReadOnlyList<int> lengths,
        uint entryˉoffset)
    {
        var Total = checked(lengths.Sum());
        var Result = new byte[checked(28 + lengths.Count * 12)];
        "WVLI"u8.CopyTo(Result);
        BinaryPrimitives.WriteUInt16LittleEndian(Result.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)Result.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)Total);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), entryˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), (uint)lengths.Count);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), 4_194_304);
        uint Position = 0;
        for (var Index = 0; Index < lengths.Count; Index++)
        {
            var Entry = Result.AsSpan(28 + Index * 12);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry, (uint)Index);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[4..], Position);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[8..], (uint)lengths[Index]);
            Position = checked(Position + (uint)lengths[Index]);
        }
        return Result;
    }
}
