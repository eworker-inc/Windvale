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
    private static readonly string NATIVE_X64_STAGING_WVO_RELOCATIONS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Wvo-Relocations.wv");
    private static readonly string NATIVE_X64_STAGING_WVO_RELOCATIONS_NATIVE_BRIDGE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Wvo-Relocations-Native-Bridge.wv");
    private static readonly string WVO_STAGING_RELOCATIONS_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Relocations-Adapter.wv");
    private static readonly string WVO_STAGING_RELOCATIONS_NATIVE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Relocations-Native-Adapter.wv");

    private const int WVO_STAGING_RELOCATIONS_ADAPTER_WVB_BYTES = 42_003;
    private const string WVO_STAGING_RELOCATIONS_ADAPTER_SHA256 =
        "82d5764228bbf62161911378904fa94a6eeecb7c0cfedf5bb464227b6ab16780";
    private const int WVO_STAGING_RELOCATIONS_NATIVE_ADAPTER_WVB_BYTES = 40_710;
    private const string WVO_STAGING_RELOCATIONS_NATIVE_ADAPTER_SHA256 =
        "04e94b7626d76248338928ca06da6ff8cbd001f1830b2ba29b3f4c46c8a1fbdc";

    private static void Nativeˉwvoˉstagingˉrelocationsˉareˉverified()
    {
        var Adapterˉbytes = Compileˉstagingˉwvoˉrelocationsˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Relocations-Adapter.wv",
            WVO_STAGING_RELOCATIONS_ADAPTER_SOURCE);
        Equal(WVO_STAGING_RELOCATIONS_ADAPTER_WVB_BYTES, Adapterˉbytes.Length);
        Equal(
            WVO_STAGING_RELOCATIONS_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Adapterˉbytes));
        var Adapter = Moduleˉcodec.Readˉandˉverify(Adapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉwvoˉrelocationsˉtest",
            Adapter.Module.Name);

        var Emptyˉheader = Array.Empty<byte>();
        var Emptyˉrelocations = Array.Empty<byte>();
        var Main = Buildˉstagingˉwvoˉsymbol(2, 1, 0, 0, 10, "Main");
        var Prefix = Buildˉstagingˉwvoˉprefix(1, 1, 0, 10);
        var Manifest = Buildˉstagingˉmanifest(49, 10, 24);
        var Text = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assertˉstagingˉwvoˉrelocations(
            Adapter,
            0,
            0,
            Manifest,
            Prefix,
            Emptyˉheader,
            Main,
            Emptyˉrelocations,
            1,
            Text,
            codeˉbytes: 10,
            textˉchunks: 1,
            relocationsˉseen: 0);

        var Readˉonlyˉprefix = Buildˉstagingˉwvoˉprefix(2, 3, 1, 10);
        var Readˉonlyˉheader = Buildˉstagingˉwvoˉreadˉonlyˉheader(5);
        var Symbols = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉsymbol(1, 2, 1, 0, 5, "$data_0000"),
            Buildˉstagingˉwvoˉsymbol(1, 1, 0, 0, 4, "$function_0000"),
            Buildˉstagingˉwvoˉsymbol(2, 1, 0, 4, 6, "Main"));
        var Relocation = Buildˉstagingˉwvoˉrelocation(2, 0);
        var Readˉonlyˉmanifest = Buildˉstagingˉmanifest(49, 10, 27, 5, 88, 20);
        var Relocatedˉtext = new byte[] { 1, 2, 0, 0, 0, 0, 7, 8, 9, 10 };
        Assertˉstagingˉwvoˉrelocations(
            Adapter,
            0,
            0,
            Readˉonlyˉmanifest,
            Readˉonlyˉprefix,
            Readˉonlyˉheader,
            Symbols,
            Relocation,
            1,
            Relocatedˉtext,
            codeˉbytes: 10,
            textˉchunks: 1,
            relocationsˉseen: 1);

        var Paddedˉprefix = Buildˉstagingˉwvoˉprefix(2, 3, 1, 16);
        var Paddedˉmanifest = Buildˉstagingˉmanifest(49, 10, 6, 27, 5, 88, 20);
        Assertˉstagingˉwvoˉrelocations(
            Adapter,
            0,
            0,
            Paddedˉmanifest,
            Paddedˉprefix,
            Readˉonlyˉheader,
            Symbols,
            Relocation,
            1,
            Relocatedˉtext,
            codeˉbytes: 10,
            textˉchunks: 2,
            relocationsˉseen: 1);
        Assertˉstagingˉwvoˉrelocations(
            Adapter,
            0,
            0,
            Paddedˉmanifest,
            Paddedˉprefix,
            Readˉonlyˉheader,
            Symbols,
            Relocation,
            2,
            [144, 144, 144, 144, 144, 144],
            codeˉbytes: 10,
            textˉchunks: 2,
            relocationsˉseen: 1);

        var Invalidˉsymbols = Symbols.ToArray();
        Invalidˉsymbols[20] = (byte)'X';
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 1, 1, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Invalidˉsymbols, Relocation, 1, Relocatedˉtext);
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 2, 2, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Relocation[..19], 1, Relocatedˉtext);
        Assertˉchangedˉstagingˉwvoˉrelocation(
            Adapter, 3, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Relocation, 1, 1, Relocatedˉtext);
        Assertˉchangedˉstagingˉwvoˉrelocation(
            Adapter, 4, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Relocation, 0, 1, Relocatedˉtext);

        var Reversedˉrelocations = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉrelocation(4, 0),
            Buildˉstagingˉwvoˉrelocation(2, 0));
        Assertˉstagingˉwvoˉrelocations(
            Adapter,
            5,
            5,
            Buildˉstagingˉmanifest(49, 10, 27, 5, 88, 40),
            Buildˉstagingˉwvoˉprefix(2, 3, 2, 10),
            Readˉonlyˉheader,
            Symbols,
            Reversedˉrelocations,
            1,
            Relocatedˉtext);
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 6, 6, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Buildˉstagingˉwvoˉrelocation(8, 0),
            1, Relocatedˉtext);
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 7, 7, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Buildˉstagingˉwvoˉrelocation(2, 1),
            1, Relocatedˉtext);
        Assertˉstagingˉwvoˉrelocations(
            Adapter,
            8,
            8,
            Buildˉstagingˉmanifest(49, 16, 27, 5, 88, 20),
            Paddedˉprefix,
            Readˉonlyˉheader,
            Symbols,
            Relocation,
            1,
            Combineˉstagingˉwvoˉrecords(
                Relocatedˉtext,
                [144, 144, 144, 144, 144, 144]));

        Assertˉstagingˉwvoˉrelocations(
            Adapter, 0, 9, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Relocation, 0, new byte[49],
            codeˉbytes: 10, textˉchunks: 1, relocationsˉseen: 1);
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 0, 10, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Relocation, 1, Relocatedˉtext[..9],
            codeˉbytes: 10, textˉchunks: 1, relocationsˉseen: 1);
        var Invalidˉplaceholder = Relocatedˉtext.ToArray();
        Invalidˉplaceholder[2] = 1;
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 0, 11, Readˉonlyˉmanifest, Readˉonlyˉprefix,
            Readˉonlyˉheader, Symbols, Relocation, 1, Invalidˉplaceholder,
            codeˉbytes: 10, textˉchunks: 1, relocationsˉseen: 1);
        Assertˉstagingˉwvoˉrelocations(
            Adapter, 0, 12, Paddedˉmanifest, Paddedˉprefix,
            Readˉonlyˉheader, Symbols, Relocation, 2,
            [144, 144, 144, 144, 144, 0],
            codeˉbytes: 10, textˉchunks: 2, relocationsˉseen: 1);

        var Nativeˉadapterˉbytes = Compileˉstagingˉwvoˉrelocationsˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Relocations-Native-Adapter.wv",
            WVO_STAGING_RELOCATIONS_NATIVE_ADAPTER_SOURCE);
        Equal(
            WVO_STAGING_RELOCATIONS_NATIVE_ADAPTER_WVB_BYTES,
            Nativeˉadapterˉbytes.Length);
        Equal(
            WVO_STAGING_RELOCATIONS_NATIVE_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Nativeˉadapterˉbytes));
        var Nativeˉadapter = Moduleˉcodec.Readˉandˉverify(Nativeˉadapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉwvoˉrelocationsˉnativeˉtest",
            Nativeˉadapter.Module.Name);
        var Native = X64ˉnativeˉbackend.Compile(Nativeˉadapter);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(0, Native.Fragment.Requiredˉservices.Length);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
    }

    private static byte[] Compileˉstagingˉwvoˉrelocationsˉadapter(
        string path,
        string source)
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(path, source),
            [
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Manifest.wv",
                    NATIVE_X64_LOWERING_STAGING_MANIFEST_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv",
                    NATIVE_X64_STAGING_WVO_ENVELOPE_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations.wv",
                    NATIVE_X64_STAGING_WVO_RELOCATIONS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations-Native-Bridge.wv",
                    NATIVE_X64_STAGING_WVO_RELOCATIONS_NATIVE_BRIDGE_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv",
                    NATIVE_X64_STAGING_WVO_SYMBOLS_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale staging WVO relocation adapter compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }

    private static void Assertˉchangedˉstagingˉwvoˉrelocation(
        Verifiedˉmodule adapter,
        uint status,
        byte[] manifest,
        byte[] prefix,
        byte[] readˉonlyˉheader,
        byte[] symbols,
        byte[] relocations,
        int offset,
        byte value,
        byte[] chunk)
    {
        var Changed = relocations.ToArray();
        Changed[offset] = value;
        Assertˉstagingˉwvoˉrelocations(
            adapter, status, status, manifest, prefix, readˉonlyˉheader,
            symbols, Changed, 1, chunk);
    }

    private static void Assertˉstagingˉwvoˉrelocations(
        Verifiedˉmodule adapter,
        uint status,
        uint chunkˉstatus,
        byte[] manifest,
        byte[] prefix,
        byte[] readˉonlyˉheader,
        byte[] symbols,
        byte[] relocations,
        uint chunkˉindex,
        byte[] chunk,
        uint codeˉbytes = 0,
        uint textˉchunks = 0,
        uint relocationsˉseen = 0)
    {
        var Request = new byte[checked(
            28 + manifest.Length + prefix.Length + readˉonlyˉheader.Length +
            symbols.Length + relocations.Length + chunk.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Request, checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(4), checked((uint)prefix.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(8), checked((uint)readˉonlyˉheader.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(12), checked((uint)symbols.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(16), checked((uint)relocations.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(20), chunkˉindex);
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(24), checked((uint)chunk.Length));
        var Offset = 28;
        foreach (var Value in new[] { manifest, prefix, readˉonlyˉheader, symbols, relocations, chunk })
        {
            Value.CopyTo(Request, Offset);
            Offset += Value.Length;
        }
        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults)
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        Equal(24, Evidence.Length);
        Sequenceˉequal("WVRS"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        var Expected = new uint[]
        {
            status,
            codeˉbytes,
            textˉchunks,
            relocationsˉseen,
            chunkˉstatus,
        };
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
    }

    private static byte[] Buildˉstagingˉwvoˉrelocation(uint offset, uint symbol)
    {
        var Result = new byte[20];
        Result[0] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), offset);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), symbol);
        BinaryPrimitives.WriteInt32LittleEndian(Result.AsSpan(16), -4);
        return Result;
    }
}
