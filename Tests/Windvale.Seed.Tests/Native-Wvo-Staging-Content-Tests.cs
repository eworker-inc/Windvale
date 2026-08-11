using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string NATIVE_X64_STAGING_CONTENT_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Content.wv");
    private static readonly string WVO_STAGING_CONTENT_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Content-Adapter.wv");
    private static readonly string WVO_STAGING_CONTENT_NATIVE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Content-Native-Adapter.wv");

    private const int WVO_STAGING_CONTENT_ADAPTER_WVB_BYTES = 450_115;
    private const string WVO_STAGING_CONTENT_ADAPTER_SHA256 =
        "15c4299d72f845a44b91f8d829be10413c7be2df365603b132db31f66963c86f";
    private const int WVO_STAGING_CONTENT_NATIVE_ADAPTER_WVB_BYTES = 451_710;
    private const string WVO_STAGING_CONTENT_NATIVE_ADAPTER_SHA256 =
        "a9b42315cd0c2cef71914568a3c77a7467011b6e25f1d42c3ddaa73ec53fc620";

    private static void Nativeˉwvoˉstagingˉcontentˉisˉverified()
    {
        var Adapterˉbytes = Compileˉstagingˉcontentˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Content-Adapter.wv",
            WVO_STAGING_CONTENT_ADAPTER_SOURCE);
        Equal(WVO_STAGING_CONTENT_ADAPTER_WVB_BYTES, Adapterˉbytes.Length);
        Equal(
            WVO_STAGING_CONTENT_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Adapterˉbytes));
        var Adapter = Moduleˉcodec.Readˉandˉverify(Adapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉcontentˉtest",
            Adapter.Module.Name);

        var Scalar = Buildˉstagingˉcontentˉfixture(
            WVB_TO_WVO_RETURN_42_SOURCE);
        var Scalarˉmanifest = Buildˉstagingˉmanifest(
            Scalar.Chunks.Select(Chunk => checked((uint)Chunk.Length)).ToArray());
        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb,
            Scalarˉmanifest,
            Scalar.Chunks,
            beginˉstatus: 0,
            stepˉstatus: 0,
            finishˉstatus: 1,
            processed: checked((uint)Scalar.Chunks.Length),
            expectedˉchunks: checked((uint)Scalar.Chunks.Length));

        var Changedˉcode = Cloneˉstagingˉchunks(Scalar.Chunks);
        Changedˉcode[Scalar.Codeˉchunk][0] ^= 1;
        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb,
            Scalarˉmanifest,
            Changedˉcode,
            0,
            8,
            8,
            checked((uint)Scalar.Codeˉchunk),
            checked((uint)Scalar.Chunks.Length));

        var Short = Cloneˉstagingˉchunks(Scalar.Chunks);
        Short[0] = Short[0][..^1];
        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb,
            Scalarˉmanifest,
            Short,
            0,
            7,
            7,
            0,
            checked((uint)Scalar.Chunks.Length));

        var Shiftedˉlengths = Scalar.Chunks
            .Select(Chunk => checked((uint)Chunk.Length))
            .ToArray();
        Shiftedˉlengths[0] -= 1;
        Shiftedˉlengths[1] += 1;
        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb,
            Buildˉstagingˉmanifest(Shiftedˉlengths),
            Scalar.Chunks,
            0,
            6,
            6,
            0,
            checked((uint)Scalar.Chunks.Length));

        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb[..^1],
            Scalarˉmanifest,
            Scalar.Chunks,
            2,
            2,
            2,
            0,
            0);
        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb,
            Scalarˉmanifest[..^1],
            Scalar.Chunks,
            3,
            3,
            3,
            0,
            0);
        Assertˉstagingˉcontent(
            Adapter,
            Scalar.Wvb,
            Scalarˉmanifest,
            Scalar.Chunks,
            0,
            8,
            8,
            checked((uint)Scalar.Chunks.Length),
            checked((uint)Scalar.Chunks.Length),
            [0]);

        var Data = Buildˉstagingˉcontentˉfixture(
            WVB_TO_WVO_STATIC_DESCRIPTORS_SOURCE);
        True(Data.Dataˉchunk >= 0, "The data fixture has no data chunk.");
        var Dataˉmanifest = Buildˉstagingˉmanifest(
            Data.Chunks.Select(Chunk => checked((uint)Chunk.Length)).ToArray());
        Assertˉstagingˉcontent(
            Adapter,
            Data.Wvb,
            Dataˉmanifest,
            Data.Chunks,
            0,
            0,
            1,
            checked((uint)Data.Chunks.Length),
            checked((uint)Data.Chunks.Length));
        var Changedˉdata = Cloneˉstagingˉchunks(Data.Chunks);
        Changedˉdata[Data.Dataˉchunk][0] ^= 1;
        Assertˉstagingˉcontent(
            Adapter,
            Data.Wvb,
            Dataˉmanifest,
            Changedˉdata,
            0,
            8,
            8,
            checked((uint)Data.Dataˉchunk),
            checked((uint)Data.Chunks.Length));

        var Nativeˉadapterˉbytes = Compileˉstagingˉcontentˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Content-Native-Adapter.wv",
            WVO_STAGING_CONTENT_NATIVE_ADAPTER_SOURCE);
        Equal(
            WVO_STAGING_CONTENT_NATIVE_ADAPTER_WVB_BYTES,
            Nativeˉadapterˉbytes.Length);
        Equal(
            WVO_STAGING_CONTENT_NATIVE_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Nativeˉadapterˉbytes));
        var Nativeˉadapter = Moduleˉcodec.Readˉandˉverify(Nativeˉadapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉcontentˉnativeˉtest",
            Nativeˉadapter.Module.Name);
        var Native = X64ˉnativeˉbackend.Compile(Nativeˉadapter);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
            ],
            Native.Fragment.Requiredˉservices);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: 100_000_000));
    }

    private static byte[] Compileˉstagingˉcontentˉadapter(
        string path,
        string source) =>
        Compileˉwvbˉtoˉwvoˉapplicationˉsuccess(
            path,
            source,
            "staging-content adapter",
            includeˉpublication: true,
            includeˉstagingˉmanifest: true,
            includeˉstagingˉcontent: true);

    private static Stagingˉcontentˉfixture Buildˉstagingˉcontentˉfixture(
        string source) =>
        Buildˉstagingˉcontentˉfixture(Compileˉsuccess(source));

    private static Stagingˉcontentˉfixture Buildˉstagingˉcontentˉfixture(
        byte[] wvb)
    {
        var Module = Moduleˉcodec.Readˉandˉverify(wvb);
        var Native = X64ˉnativeˉbackend.Compile(Module);
        var Object = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment).ToArray();
        var View = Objectˉcodec.Readˉandˉverify(Object).Value;
        True(
            View.Sections.Length is 1 or 2,
            "The content fixture has an unsupported section count.");

        var Lengths = new List<int>();
        Lengths.Add(checked(
            24 + 20 + Encoding.UTF8.GetByteCount(View.Sections[0].Name)));
        var Codeˉchunk = Lengths.Count;
        var Codeˉbytes = checked((int)View.Symbols
            .Where(Symbol => Symbol.Kind == Objectˉsymbolˉkind.Function)
            .Aggregate(0u, (Total, Symbol) => checked(Total + Symbol.Size)));
        Lengths.Add(Codeˉbytes);
        var Paddingˉbytes = View.Sections[0].Data.Length - Codeˉbytes;
        if (Paddingˉbytes > 0)
        {
            Lengths.Add(Paddingˉbytes);
        }

        var Dataˉchunk = -1;
        if (View.Sections.Length == 2)
        {
            var Readˉonly = View.Sections[1];
            Equal(Objectˉsectionˉkind.Readˉonlyˉdata, Readˉonly.Kind);
            Lengths.Add(checked(
                20 + Encoding.UTF8.GetByteCount(Readˉonly.Name)));
            Dataˉchunk = Lengths.Count;
            Lengths.Add(Readˉonly.Data.Length);
        }

        var Symbolˉbytes = View.Symbols.Sum(Symbol => checked(
            20 + Encoding.UTF8.GetByteCount(Symbol.Name)));
        Lengths.Add(Symbolˉbytes);
        var Relocationˉbytes = checked(View.Relocations.Length * 20);
        if (Relocationˉbytes > 0)
        {
            Lengths.Add(Relocationˉbytes);
        }
        Equal(Object.Length, Lengths.Sum());

        var Chunks = new byte[Lengths.Count][];
        var Position = 0;
        for (var Index = 0; Index < Lengths.Count; Index++)
        {
            Chunks[Index] = Object.AsSpan(Position, Lengths[Index]).ToArray();
            Position += Lengths[Index];
        }
        return new(wvb, Chunks, Codeˉchunk, Dataˉchunk);
    }

    private static byte[][] Cloneˉstagingˉchunks(byte[][] chunks) =>
        chunks.Select(Chunk => Chunk.ToArray()).ToArray();

    private static void Assertˉstagingˉcontent(
        Verifiedˉmodule adapter,
        byte[] wvb,
        byte[] manifest,
        byte[][] chunks,
        uint beginˉstatus,
        uint stepˉstatus,
        uint finishˉstatus,
        uint processed,
        uint expectedˉchunks,
        byte[]? trailing = null)
    {
        trailing ??= [];
        var Payloadˉbytes = chunks.Sum(Chunk => Chunk.Length);
        var Request = new byte[checked(
            12 + chunks.Length * 4 + wvb.Length + manifest.Length +
            Payloadˉbytes + trailing.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request,
            checked((uint)wvb.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(4),
            checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(8),
            checked((uint)chunks.Length));
        for (var Index = 0; Index < chunks.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(
                Request.AsSpan(12 + Index * 4),
                checked((uint)chunks[Index].Length));
        }
        var Position = 12 + chunks.Length * 4;
        foreach (var Value in new[] { wvb, manifest }.Concat(chunks).Append(trailing))
        {
            Value.CopyTo(Request, Position);
            Position += Value.Length;
        }

        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with
            {
                Maximumˉinstructions = 100_000_000,
            })
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        Equal(24, Evidence.Length);
        Sequenceˉequal("WVCE"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        var Expected = new uint[]
        {
            beginˉstatus,
            stepˉstatus,
            finishˉstatus,
            processed,
            expectedˉchunks,
        };
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
    }

    private sealed record Stagingˉcontentˉfixture(
        byte[] Wvb,
        byte[][] Chunks,
        int Codeˉchunk,
        int Dataˉchunk);
}
