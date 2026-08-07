using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string COMPILER_FLAT_IMAGE_STAGING_MANIFEST_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Flat-Image-Staging-Manifest.wv");
    private static readonly string COMPILER_FLAT_IMAGE_STAGING_MANIFEST_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Flat-Image-Staging-Manifest-Adapter.wv");
    private static readonly string COMPILER_FLAT_IMAGE_STAGING_MANIFEST_NATIVE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Flat-Image-Staging-Manifest-Native-Adapter.wv");

    private static void Compilerˉflatˉimageˉstagingˉmanifestˉruns()
    {
        var Adapterˉbytes = Compileˉcompilerˉflatˉimageˉmanifestˉadapter(
            "Tests/Fixtures/Native-X64/Compiler-Flat-Image-Staging-Manifest-Adapter.wv",
            COMPILER_FLAT_IMAGE_STAGING_MANIFEST_ADAPTER_SOURCE);
        var Adapter = Moduleˉcodec.Readˉandˉverify(Adapterˉbytes);
        Equal(
            "Linkerˉcompilerˉflatˉimageˉstagingˉmanifestˉtest",
            Adapter.Module.Name);

        var Entries = Buildˉcompilerˉflatˉimageˉentries(10, 6, 2, 3);
        var Buildˉrequest = new byte[13 + Entries.Length];
        Buildˉrequest[0] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(Buildˉrequest.AsSpan(1), 21);
        BinaryPrimitives.WriteUInt32LittleEndian(Buildˉrequest.AsSpan(5), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(Buildˉrequest.AsSpan(9), 4);
        Entries.CopyTo(Buildˉrequest, 13);
        var Manifest = Runˉcompilerˉflatˉimageˉmanifest(
            Adapter, Buildˉrequest);
        Equal(76, Manifest.Length);
        Sequenceˉequal("WVLI"u8.ToArray(), Manifest.AsSpan(0, 4).ToArray());
        Equal((uint)76, BinaryPrimitives.ReadUInt32LittleEndian(Manifest.AsSpan(8)));
        Equal((uint)21, BinaryPrimitives.ReadUInt32LittleEndian(Manifest.AsSpan(12)));
        Equal((uint)4, BinaryPrimitives.ReadUInt32LittleEndian(Manifest.AsSpan(16)));
        Equal((uint)4, BinaryPrimitives.ReadUInt32LittleEndian(Manifest.AsSpan(20)));
        Sequenceˉequal(Entries, Manifest.AsSpan(28).ToArray());
        Assertˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 0, 21, 4, 4, 4_194_304);

        Assertˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest[..27], 1, 0, 0, 0, 0);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 0, 0, 2);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 4, 2, 3);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 8, 0, 4);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 12, 0, 5);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 16, 21, 6);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 20, 0, 7);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 24, 1, 8);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 40, 0, 9);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 44, 11, 10);
        Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
            Adapter, Manifest, 48, 0, 11);

        var Invalidˉbuild = Buildˉrequest.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Invalidˉbuild.AsSpan(5), 21);
        Equal(0, Runˉcompilerˉflatˉimageˉmanifest(
            Adapter, Invalidˉbuild).Length);

        var Nativeˉbytes = Compileˉcompilerˉflatˉimageˉmanifestˉadapter(
            "Tests/Fixtures/Native-X64/Compiler-Flat-Image-Staging-Manifest-Native-Adapter.wv",
            COMPILER_FLAT_IMAGE_STAGING_MANIFEST_NATIVE_ADAPTER_SOURCE);
        var Nativeˉmodule = Moduleˉcodec.Readˉandˉverify(Nativeˉbytes);
        Equal(
            "Linkerˉcompilerˉflatˉimageˉstagingˉmanifestˉnativeˉtest",
            Nativeˉmodule.Module.Name);
        var Native = X64ˉnativeˉbackend.Compile(Nativeˉmodule);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(0, Native.Fragment.Requiredˉservices.Length);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
    }

    private static byte[] Compileˉcompilerˉflatˉimageˉmanifestˉadapter(
        string path,
        string source)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(path, source),
            [
                new(
                    "Linker/Windvale/Compiler-Flat-Image-Staging-Manifest.wv",
                    COMPILER_FLAT_IMAGE_STAGING_MANIFEST_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale compiler flat-image manifest compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Runˉcompilerˉflatˉimageˉmanifest(
        Verifiedˉmodule adapter,
        byte[] request) =>
        new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults)
            .Runˉmainˉbytes(request.ToImmutableArray())
            .Bytes
            .ToArray();

    private static void Assertˉcompilerˉflatˉimageˉmanifest(
        Verifiedˉmodule adapter,
        byte[] manifest,
        uint status,
        uint imageˉbytes,
        uint entryˉoffset,
        uint chunks,
        uint maximumˉchunkˉbytes)
    {
        var Request = new byte[1 + manifest.Length];
        manifest.CopyTo(Request, 1);
        var Evidence = Runˉcompilerˉflatˉimageˉmanifest(adapter, Request);
        Equal(24, Evidence.Length);
        Sequenceˉequal("WVLM"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        uint[] Expected =
        [status, imageˉbytes, entryˉoffset, chunks, maximumˉchunkˉbytes];
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
    }

    private static void Assertˉchangedˉcompilerˉflatˉimageˉmanifest(
        Verifiedˉmodule adapter,
        byte[] manifest,
        int offset,
        uint value,
        uint status)
    {
        var Changed = manifest.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Changed.AsSpan(offset), value);
        Assertˉcompilerˉflatˉimageˉmanifest(
            adapter, Changed, status, 0, 0, 0, 0);
    }

    private static byte[] Buildˉcompilerˉflatˉimageˉentries(
        params uint[] lengths)
    {
        var Result = new byte[lengths.Length * 12];
        uint Position = 0;
        for (var Index = 0; Index < lengths.Length; Index++)
        {
            var Entry = Result.AsSpan(Index * 12);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Entry, checked((uint)Index));
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[4..], Position);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[8..], lengths[Index]);
            Position = checked(Position + lengths[Index]);
        }
        return Result;
    }
}
