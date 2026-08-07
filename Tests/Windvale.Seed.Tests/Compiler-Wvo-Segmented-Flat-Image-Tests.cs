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
    private static readonly string COMPILER_WVO_SEGMENTED_FLAT_IMAGE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Wvo-Segmented-Flat-Image.wv");
    private static readonly string COMPILER_WVO_SEGMENTED_FLAT_IMAGE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Wvo-Segmented-Flat-Image-Adapter.wv");
    private static readonly string COMPILER_WVO_SEGMENTED_FLAT_IMAGE_NATIVE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Wvo-Segmented-Flat-Image-Native-Adapter.wv");
    private static readonly string COMPILER_WVO_SEGMENTED_FLAT_IMAGE_VERIFICATION_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Wvo-Segmented-Flat-Image-Verification.wv");
    private static readonly string COMPILER_WVO_SEGMENTED_FLAT_IMAGE_VERIFICATION_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Wvo-Segmented-Flat-Image-Verification-Adapter.wv");

    private static void Compilerˉwvoˉsegmentedˉflatˉimageˉlinks()
    {
        var Adapterˉbytes = Compileˉcompilerˉwvoˉsegmentedˉflatˉimage(
            "Tests/Fixtures/Native-X64/Compiler-Wvo-Segmented-Flat-Image-Adapter.wv",
            COMPILER_WVO_SEGMENTED_FLAT_IMAGE_ADAPTER_SOURCE);
        var Adapter = Moduleˉcodec.Readˉandˉverify(Adapterˉbytes);
        Equal(
            "Linkerˉcompilerˉwvoˉsegmentedˉflatˉimageˉtest",
            Adapter.Module.Name);

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
        byte[] Text = [1, 0, 0, 0, 0, 2, 0, 0, 0, 0];
        byte[] Linkedˉtext = [1, 11, 0, 0, 0, 2, 8, 0, 0, 0];
        byte[] Padding = [144, 144, 144, 144, 144, 144];
        byte[] Dataˉfirst = [65, 66];
        byte[] Dataˉsecond = [67, 68, 69];

        var Linkedˉchunks = new byte[][]
        {
            Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
                Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
                Relocations, 1, Text, 0, 21, 4, 8, 4, 2, 2, 0, 1, 0,
                Linkedˉtext),
            Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
                Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
                Relocations, 2, Padding, 0, 21, 4, 8, 4, 2, 2, 0, 1, 10,
                Padding),
            Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
                Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
                Relocations, 4, Dataˉfirst, 0, 21, 4, 8, 4, 2, 2, 0, 2, 16,
                Dataˉfirst),
            Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
                Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
                Relocations, 5, Dataˉsecond, 0, 21, 4, 8, 4, 2, 2, 0, 2, 18,
                Dataˉsecond),
        };
        Sequenceˉequal(
            Combineˉstagingˉwvoˉrecords(
                Linkedˉtext, Padding, Dataˉfirst, Dataˉsecond),
            Combineˉstagingˉwvoˉrecords(Linkedˉchunks));

        foreach (var Metadata in new (uint Index, byte[] Value)[]
        {
            (0, Prefix),
            (3, Readˉonlyˉheader),
            (6, Symbols),
            (7, Relocations),
        })
        {
            _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
                Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
                Relocations, Metadata.Index, Metadata.Value,
                0, 21, 4, 8, 4, 2, 2, 0, 0, 0, []);
        }

        var Invalidˉmanifest = Manifest.ToArray();
        Invalidˉmanifest[0] = 0;
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Invalidˉmanifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, 1, Text, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, []);
        var Invalidˉprefix = Prefix.ToArray();
        Invalidˉprefix[24] = 2;
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Invalidˉprefix, Readˉonlyˉheader, Symbols,
            Relocations, 1, Text, 2, 0, 0, 0, 0, 0, 0, 2, 0, 0, []);
        var Invalidˉsymbols = Symbols.ToArray();
        Invalidˉsymbols[20] = (byte)'X';
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Prefix, Readˉonlyˉheader, Invalidˉsymbols,
            Relocations, 1, Text, 3, 0, 0, 0, 0, 0, 0, 3, 0, 0, []);
        var Invalidˉrelocations = Relocations.ToArray();
        Invalidˉrelocations[0] = 1;
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Invalidˉrelocations, 1, Text,
            4, 0, 0, 0, 0, 0, 0, 4, 0, 0, []);
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, 8, [], 0, 21, 4, 8, 4, 2, 2, 6, 0, 0, []);
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, 1, Text[..9],
            0, 21, 4, 8, 4, 2, 2, 7, 0, 0, []);
        var Changedˉprefix = Prefix.ToArray();
        Changedˉprefix[48] = (byte)'X';
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, 0, Changedˉprefix,
            0, 21, 4, 8, 4, 2, 2, 9, 0, 0, []);
        var Invalidˉtext = Text.ToArray();
        Invalidˉtext[1] = 1;
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, 1, Invalidˉtext,
            0, 21, 4, 8, 4, 2, 2, 10, 0, 0, []);

        var Main = Buildˉstagingˉwvoˉsymbol(2, 1, 0, 0, 10, "Main");
        var Oneˉsectionˉprefix = Buildˉstagingˉwvoˉprefix(1, 1, 0, 10);
        var Oneˉsectionˉmanifest = Buildˉstagingˉmanifest(49, 10, 24);
        _ = Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
            Adapter, Oneˉsectionˉmanifest, Oneˉsectionˉprefix, [], Main, [],
            1, Text, 0, 10, 0, 3, 1, 1, 0, 0, 1, 0, Text);

        var Verificationˉadapterˉbytes =
            Compileˉcompilerˉwvoˉsegmentedˉflatˉimage(
                "Tests/Fixtures/Native-X64/Compiler-Wvo-Segmented-Flat-Image-Verification-Adapter.wv",
                COMPILER_WVO_SEGMENTED_FLAT_IMAGE_VERIFICATION_ADAPTER_SOURCE,
                includeˉlinker: false,
                includeˉverification: true);
        var Verificationˉadapter = Moduleˉcodec.Readˉandˉverify(
            Verificationˉadapterˉbytes);
        Equal(
            "Linkerˉcompilerˉwvoˉsegmentedˉflatˉimageˉverificationˉtest",
            Verificationˉadapter.Module.Name);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Text, 0, Linkedˉtext,
            0, 8, 21, 0, 8, 2, 10, 21);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Text, 1, Linkedˉtext,
            0, 8, 21, 10, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Text, 0, Linkedˉtext[..9],
            0, 8, 21, 11, 0, 0, 0, 0);
        var Changedˉcandidate = Linkedˉtext.ToArray();
        Changedˉcandidate[5] = 3;
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Text, 0, Changedˉcandidate,
            0, 8, 21, 12, 0, 0, 0, 0);
        var Changedˉrelocation = Linkedˉtext.ToArray();
        Changedˉrelocation[1] = 12;
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Text, 0, Changedˉrelocation,
            0, 8, 21, 12, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Invalidˉtext, 0, Linkedˉtext,
            0, 8, 21, 9, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader, Symbols,
            Relocations, Text[..9], 0, Linkedˉtext,
            0, 8, 21, 7, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Invalidˉmanifest, Prefix, Readˉonlyˉheader,
            Symbols, Relocations, Text, 0, Linkedˉtext,
            2, 0, 0, 2, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Invalidˉprefix,
            Readˉonlyˉheader, Symbols, Relocations, Text, 0, Linkedˉtext,
            3, 0, 0, 3, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader,
            Invalidˉsymbols, Relocations, Text, 0, Linkedˉtext,
            4, 0, 0, 4, 0, 0, 0, 0);
        Assertˉcompilerˉwvoˉsegmentedˉverification(
            Verificationˉadapter, Manifest, Prefix, Readˉonlyˉheader,
            Symbols, Invalidˉrelocations, Text, 0, Linkedˉtext,
            5, 0, 0, 5, 0, 0, 0, 0);

        var Nativeˉadapterˉbytes = Compileˉcompilerˉwvoˉsegmentedˉflatˉimage(
            "Tests/Fixtures/Native-X64/Compiler-Wvo-Segmented-Flat-Image-Native-Adapter.wv",
            COMPILER_WVO_SEGMENTED_FLAT_IMAGE_NATIVE_ADAPTER_SOURCE,
            includeˉlinker: true,
            includeˉverification: true);
        var Nativeˉadapter = Moduleˉcodec.Readˉandˉverify(Nativeˉadapterˉbytes);
        Equal(
            "Linkerˉcompilerˉwvoˉsegmentedˉflatˉimageˉnativeˉtest",
            Nativeˉadapter.Module.Name);
        var Native = X64ˉnativeˉbackend.Compile(Nativeˉadapter);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(0, Native.Fragment.Requiredˉservices.Length);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
    }

    private static byte[] Compileˉcompilerˉwvoˉsegmentedˉflatˉimage(
        string path,
        string source,
        bool includeˉlinker = true,
        bool includeˉverification = false)
    {
        List<Sourceˉmoduleˉinput> Dependencies =
        [
            new(
                "Compiler/Windvale/Native-X64-Lowering-Staging-Manifest.wv",
                NATIVE_X64_LOWERING_STAGING_MANIFEST_SOURCE),
            new(
                "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv",
                NATIVE_X64_STAGING_WVO_ENVELOPE_SOURCE),
            new(
                "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv",
                NATIVE_X64_STAGING_WVO_SYMBOLS_SOURCE),
            new(
                "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations.wv",
                NATIVE_X64_STAGING_WVO_RELOCATIONS_SOURCE),
        ];
        if (includeˉlinker)
        {
            Dependencies.Add(new(
                "Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image.wv",
                COMPILER_WVO_SEGMENTED_FLAT_IMAGE_SOURCE));
        }
        if (includeˉverification)
        {
            Dependencies.Add(new(
                "Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Verification.wv",
                COMPILER_WVO_SEGMENTED_FLAT_IMAGE_VERIFICATION_SOURCE));
        }
        var Result = Seedˉcompiler.Compileˉmodules(
            new(path, source),
            Dependencies);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale segmented compiler-WVO linker compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }

    private static byte[] Assertˉcompilerˉwvoˉsegmentedˉflatˉimage(
        Verifiedˉmodule adapter,
        byte[] manifest,
        byte[] prefix,
        byte[] readˉonlyˉheader,
        byte[] symbols,
        byte[] relocations,
        uint chunkˉindex,
        byte[] chunk,
        uint planˉstatus,
        uint imageˉbytes,
        uint entryˉoffset,
        uint manifestˉchunks,
        uint outputˉchunks,
        uint textˉchunks,
        uint relocationˉcount,
        uint chunkˉstatus,
        uint chunkˉkind,
        uint imageˉposition,
        byte[] value)
    {
        var Request = new byte[checked(
            28 + manifest.Length + prefix.Length + readˉonlyˉheader.Length +
            symbols.Length + relocations.Length + chunk.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request, checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(4), checked((uint)prefix.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(8), checked((uint)readˉonlyˉheader.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(12), checked((uint)symbols.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(16), checked((uint)relocations.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(20), chunkˉindex);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(24), checked((uint)chunk.Length));
        var Offset = 28;
        foreach (var Input in new[]
        {
            manifest, prefix, readˉonlyˉheader, symbols, relocations, chunk,
        })
        {
            Input.CopyTo(Request, Offset);
            Offset += Input.Length;
        }
        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults)
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        Equal(48 + value.Length, Evidence.Length);
        Sequenceˉequal("WVFL"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        uint[] Expected =
        [
            planˉstatus,
            imageˉbytes,
            entryˉoffset,
            manifestˉchunks,
            outputˉchunks,
            textˉchunks,
            relocationˉcount,
            chunkˉstatus,
            chunkˉkind,
            imageˉposition,
            checked((uint)value.Length),
        ];
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
        var Actualˉvalue = Evidence.AsSpan(48).ToArray();
        Sequenceˉequal(value, Actualˉvalue);
        return Actualˉvalue;
    }

    private static void Assertˉcompilerˉwvoˉsegmentedˉverification(
        Verifiedˉmodule adapter,
        byte[] manifest,
        byte[] prefix,
        byte[] readˉonlyˉheader,
        byte[] symbols,
        byte[] relocations,
        byte[] source,
        uint candidateˉposition,
        byte[] candidate,
        uint beginˉstatus,
        uint beginˉchunks,
        uint beginˉimageˉbytes,
        uint nextˉstatus,
        uint nextˉchunks,
        uint nextˉchunk,
        uint nextˉimageˉposition,
        uint nextˉimageˉbytes)
    {
        var Request = new byte[checked(
            32 + manifest.Length + prefix.Length + readˉonlyˉheader.Length +
            symbols.Length + relocations.Length + source.Length +
            candidate.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request, checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(4), checked((uint)prefix.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(8), checked((uint)readˉonlyˉheader.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(12), checked((uint)symbols.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(16), checked((uint)relocations.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(20), candidateˉposition);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(24), checked((uint)source.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(28), checked((uint)candidate.Length));
        var Offset = 32;
        foreach (var Input in new[]
        {
            manifest, prefix, readˉonlyˉheader, symbols, relocations, source,
            candidate,
        })
        {
            Input.CopyTo(Request, Offset);
            Offset += Input.Length;
        }
        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults)
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        Equal(44, Evidence.Length);
        Sequenceˉequal("WVVF"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        uint[] Expected =
        [
            beginˉstatus,
            beginˉchunks,
            0,
            0,
            beginˉimageˉbytes,
            nextˉstatus,
            nextˉchunks,
            nextˉchunk,
            nextˉimageˉposition,
            nextˉimageˉbytes,
        ];
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
    }
}
