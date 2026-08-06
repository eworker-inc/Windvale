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
    private static readonly string NATIVE_X64_STAGING_WVO_ENVELOPE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Wvo-Envelope.wv");
    private static readonly string NATIVE_X64_STAGING_WVO_NATIVE_BRIDGE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Wvo-Native-Bridge.wv");
    private static readonly string WVO_STAGING_ENVELOPE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Envelope-Adapter.wv");
    private static readonly string WVO_STAGING_ENVELOPE_NATIVE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Envelope-Native-Adapter.wv");

    private const int WVO_STAGING_ENVELOPE_ADAPTER_WVB_BYTES = 18_869;
    private const string WVO_STAGING_ENVELOPE_ADAPTER_SHA256 =
        "ab7893899e5f2a17bac735cdf6cb1f2e67a725a34cac82809eb2000026501106";
    private const int WVO_STAGING_ENVELOPE_NATIVE_ADAPTER_WVB_BYTES = 18_449;
    private const string WVO_STAGING_ENVELOPE_NATIVE_ADAPTER_SHA256 =
        "d69a70c6c79bc1bd3e898af05f26d335b449128c4b1f19f6c9a437cdf33301f9";

    private static void Nativeˉwvoˉstagingˉenvelopeˉisˉbounded()
    {
        var Adapterˉbytes = Compileˉstagingˉwvoˉenvelopeˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Envelope-Adapter.wv",
            WVO_STAGING_ENVELOPE_ADAPTER_SOURCE);
        Equal(WVO_STAGING_ENVELOPE_ADAPTER_WVB_BYTES, Adapterˉbytes.Length);
        Equal(
            WVO_STAGING_ENVELOPE_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Adapterˉbytes));
        var Adapter = Moduleˉcodec.Readˉandˉverify(Adapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉwvoˉenvelopeˉtest",
            Adapter.Module.Name);

        var Emptyˉheader = Array.Empty<byte>();
        var Prefix = Buildˉstagingˉwvoˉprefix(1, 1, 0, 10);
        var Manifest = Buildˉstagingˉmanifest(49, 10, 41);
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            0,
            Manifest,
            Prefix,
            Emptyˉheader,
            objectˉbytes: 100,
            sections: 1,
            symbols: 1,
            relocations: 0,
            textˉbytes: 10,
            readˉonlyˉbytes: 0,
            symbolˉposition: 59);

        var Readˉonlyˉprefix = Buildˉstagingˉwvoˉprefix(2, 2, 1, 10);
        var Readˉonlyˉheader = Buildˉstagingˉwvoˉreadˉonlyˉheader(5);
        var Readˉonlyˉmanifest = Buildˉstagingˉmanifest(49, 10, 27, 5, 69);
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            0,
            Readˉonlyˉmanifest,
            Readˉonlyˉprefix,
            Readˉonlyˉheader,
            objectˉbytes: 160,
            sections: 2,
            symbols: 2,
            relocations: 1,
            textˉbytes: 10,
            readˉonlyˉbytes: 5,
            symbolˉposition: 91);

        const uint Maximumˉobjectˉbytes = 32u * 1024u * 1024u;
        const uint Minimumˉsymbolˉbytes = 21;
        var Largeˉtextˉbytes = Maximumˉobjectˉbytes - 49u - Minimumˉsymbolˉbytes;
        var Largeˉchunks = new List<uint> { 49 };
        var Remaining = Largeˉtextˉbytes;
        while (Remaining != 0)
        {
            var Chunk = Math.Min(Remaining, 4u * 1024u * 1024u);
            Largeˉchunks.Add(Chunk);
            Remaining -= Chunk;
        }
        Largeˉchunks.Add(Minimumˉsymbolˉbytes);
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            0,
            Buildˉstagingˉmanifest([.. Largeˉchunks]),
            Buildˉstagingˉwvoˉprefix(1, 1, 0, Largeˉtextˉbytes),
            Emptyˉheader,
            objectˉbytes: Maximumˉobjectˉbytes,
            sections: 1,
            symbols: 1,
            relocations: 0,
            textˉbytes: Largeˉtextˉbytes,
            readˉonlyˉbytes: 0,
            symbolˉposition: Maximumˉobjectˉbytes - Minimumˉsymbolˉbytes);

        var Invalidˉmanifest = Manifest.ToArray();
        Invalidˉmanifest[0] = (byte)'X';
        Assertˉstagingˉwvoˉenvelope(
            Adapter, 1, Invalidˉmanifest, Prefix, Emptyˉheader);
        Assertˉstagingˉwvoˉenvelope(
            Adapter, 2, Buildˉstagingˉmanifest(48, 11, 41), Prefix, Emptyˉheader);
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            3,
            Buildˉstagingˉmanifest(48, 11, 41),
            Prefix[..48],
            Emptyˉheader);
        Assertˉchangedˉstagingˉwvoˉprefix(Adapter, 4, Manifest, Prefix, 0, (byte)'X');
        Assertˉchangedˉstagingˉwvoˉprefix(Adapter, 5, Manifest, Prefix, 4, 2);
        Assertˉchangedˉstagingˉwvoˉprefix(Adapter, 6, Manifest, Prefix, 8, 2);
        Assertˉchangedˉstagingˉwvoˉprefix(Adapter, 7, Manifest, Prefix, 9, 1);
        Assertˉchangedˉstagingˉwvoˉprefix(Adapter, 8, Manifest, Prefix, 12, 3);
        Assertˉchangedˉstagingˉwvoˉprefix(Adapter, 9, Manifest, Prefix, 28, 8);

        var Oversizedˉtext = Prefix.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Oversizedˉtext.AsSpan(32), 52);
        BinaryPrimitives.WriteUInt32LittleEndian(Oversizedˉtext.AsSpan(36), 52);
        Assertˉstagingˉwvoˉenvelope(
            Adapter, 10, Manifest, Oversizedˉtext, Emptyˉheader);
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            11,
            Buildˉstagingˉmanifest(49, 11, 40),
            Prefix,
            Emptyˉheader);
        var Invalidˉreadˉonly = Readˉonlyˉheader.ToArray();
        Invalidˉreadˉonly[0] = 3;
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            12,
            Readˉonlyˉmanifest,
            Readˉonlyˉprefix,
            Invalidˉreadˉonly);
        Assertˉstagingˉwvoˉenvelope(
            Adapter,
            13,
            Buildˉstagingˉmanifest(49, 10, 11),
            Prefix,
            Emptyˉheader);

        var Nativeˉadapterˉbytes = Compileˉstagingˉwvoˉenvelopeˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Envelope-Native-Adapter.wv",
            WVO_STAGING_ENVELOPE_NATIVE_ADAPTER_SOURCE);
        Equal(
            WVO_STAGING_ENVELOPE_NATIVE_ADAPTER_WVB_BYTES,
            Nativeˉadapterˉbytes.Length);
        Equal(
            WVO_STAGING_ENVELOPE_NATIVE_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Nativeˉadapterˉbytes));
        var Nativeˉadapter = Moduleˉcodec.Readˉandˉverify(Nativeˉadapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉwvoˉenvelopeˉnativeˉtest",
            Nativeˉadapter.Module.Name);
        var Native = X64ˉnativeˉbackend.Compile(Nativeˉadapter);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(0, Native.Fragment.Requiredˉservices.Length);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
    }

    private static byte[] Compileˉstagingˉwvoˉenvelopeˉadapter(
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
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Native-Bridge.wv",
                    NATIVE_X64_STAGING_WVO_NATIVE_BRIDGE_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale staging WVO envelope adapter compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }

    private static void Assertˉchangedˉstagingˉwvoˉprefix(
        Verifiedˉmodule adapter,
        uint status,
        byte[] manifest,
        byte[] prefix,
        int offset,
        byte value)
    {
        var Changed = prefix.ToArray();
        Changed[offset] = value;
        Assertˉstagingˉwvoˉenvelope(
            adapter,
            status,
            manifest,
            Changed,
            Array.Empty<byte>());
    }

    private static void Assertˉstagingˉwvoˉenvelope(
        Verifiedˉmodule adapter,
        uint status,
        byte[] manifest,
        byte[] prefix,
        byte[] readˉonlyˉheader,
        uint objectˉbytes = 0,
        uint sections = 0,
        uint symbols = 0,
        uint relocations = 0,
        uint textˉbytes = 0,
        uint readˉonlyˉbytes = 0,
        uint symbolˉposition = 0)
    {
        var Request = new byte[checked(12 + manifest.Length + prefix.Length + readˉonlyˉheader.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Request, checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(4), checked((uint)prefix.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(8),
            checked((uint)readˉonlyˉheader.Length));
        manifest.CopyTo(Request, 12);
        prefix.CopyTo(Request, 12 + manifest.Length);
        readˉonlyˉheader.CopyTo(Request, 12 + manifest.Length + prefix.Length);
        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults)
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        Equal(36, Evidence.Length);
        Sequenceˉequal("WVSE"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        var Expected = new uint[]
        {
            status,
            objectˉbytes,
            sections,
            symbols,
            relocations,
            textˉbytes,
            readˉonlyˉbytes,
            symbolˉposition,
        };
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
    }

    private static byte[] Buildˉstagingˉmanifest(params uint[] chunkˉlengths)
    {
        var Result = new byte[checked(24 + chunkˉlengths.Length * 12)];
        "WVOP"u8.CopyTo(Result);
        BinaryPrimitives.WriteUInt16LittleEndian(Result.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)chunkˉlengths.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), 4u * 1024u * 1024u);
        uint Position = 0;
        for (var Index = 0; Index < chunkˉlengths.Length; Index++)
        {
            var Entry = Result.AsSpan(24 + Index * 12);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry, checked((uint)Index));
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[4..], Position);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[8..], chunkˉlengths[Index]);
            Position = checked(Position + chunkˉlengths[Index]);
        }
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), Position);
        return Result;
    }

    private static byte[] Buildˉstagingˉwvoˉprefix(
        uint sections,
        uint symbols,
        uint relocations,
        uint textˉbytes)
    {
        var Result = new byte[49];
        "WVO1"u8.CopyTo(Result);
        BinaryPrimitives.WriteUInt16LittleEndian(Result.AsSpan(4), 1);
        Result[8] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), sections);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), symbols);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), relocations);
        Result[24] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(28), 16);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(32), textˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(36), textˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(40), 5);
        ".text"u8.CopyTo(Result.AsSpan(44));
        return Result;
    }

    private static byte[] Buildˉstagingˉwvoˉreadˉonlyˉheader(uint bytes)
    {
        var Result = new byte[27];
        Result[0] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 16);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), bytes);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), bytes);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), 7);
        ".rodata"u8.CopyTo(Result.AsSpan(20));
        return Result;
    }
}
