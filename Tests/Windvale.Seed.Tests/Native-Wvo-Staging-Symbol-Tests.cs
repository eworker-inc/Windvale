using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string NATIVE_X64_STAGING_WVO_SYMBOLS_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Wvo-Symbols.wv");
    private static readonly string NATIVE_X64_STAGING_WVO_SYMBOLS_NATIVE_BRIDGE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Wvo-Symbols-Native-Bridge.wv");
    private static readonly string WVO_STAGING_SYMBOLS_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Symbols-Adapter.wv");
    private static readonly string WVO_STAGING_SYMBOLS_NATIVE_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Symbols-Native-Adapter.wv");

    private const int WVO_STAGING_SYMBOLS_ADAPTER_WVB_BYTES = 33_091;
    private const string WVO_STAGING_SYMBOLS_ADAPTER_SHA256 =
        "375e906a095c1c5dd8f98a92876312af434c0d2d385be280568ed1cbf15000aa";
    private const int WVO_STAGING_SYMBOLS_NATIVE_ADAPTER_WVB_BYTES = 32_516;
    private const string WVO_STAGING_SYMBOLS_NATIVE_ADAPTER_SHA256 =
        "024c261ed2469410c095fabe8f8ddbd9a51dbc6de653f7874f2baec169201e3d";

    private static void Nativeˉwvoˉstagingˉsymbolsˉareˉverified()
    {
        var Adapterˉbytes = Compileˉstagingˉwvoˉsymbolsˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Symbols-Adapter.wv",
            WVO_STAGING_SYMBOLS_ADAPTER_SOURCE);
        Equal(WVO_STAGING_SYMBOLS_ADAPTER_WVB_BYTES, Adapterˉbytes.Length);
        Equal(
            WVO_STAGING_SYMBOLS_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Adapterˉbytes));
        var Adapter = Moduleˉcodec.Readˉandˉverify(Adapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉwvoˉsymbolsˉtest",
            Adapter.Module.Name);

        var Emptyˉheader = Array.Empty<byte>();
        var Main = Buildˉstagingˉwvoˉsymbol(2, 1, 0, 0, 10, "Main");
        var Prefix = Buildˉstagingˉwvoˉprefix(1, 1, 0, 10);
        var Manifest = Buildˉstagingˉmanifest(49, 10, 24);
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            0,
            Manifest,
            Prefix,
            Emptyˉheader,
            Main,
            dataˉsymbols: 0,
            functions: 1,
            relocationˉposition: 83,
            relocationˉbytes: 0);

        var Readˉonlyˉprefix = Buildˉstagingˉwvoˉprefix(2, 3, 1, 10);
        var Readˉonlyˉheader = Buildˉstagingˉwvoˉreadˉonlyˉheader(5);
        var Symbols = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉsymbol(1, 2, 1, 0, 5, "$data_0000"),
            Buildˉstagingˉwvoˉsymbol(1, 1, 0, 0, 4, "$function_0000"),
            Buildˉstagingˉwvoˉsymbol(2, 1, 0, 4, 6, "Main"));
        var Readˉonlyˉmanifest = Buildˉstagingˉmanifest(49, 10, 27, 5, 88, 20);
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            0,
            Readˉonlyˉmanifest,
            Readˉonlyˉprefix,
            Readˉonlyˉheader,
            Symbols,
            dataˉsymbols: 1,
            functions: 2,
            relocationˉposition: 179,
            relocationˉbytes: 20);

        var Middleˉmainˉsymbols = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉsymbol(1, 2, 1, 0, 5, "$data_0000"),
            Buildˉstagingˉwvoˉsymbol(1, 1, 0, 0, 4, "$function_0000"),
            Buildˉstagingˉwvoˉsymbol(1, 1, 0, 10, 5, "$function_0002"),
            Buildˉstagingˉwvoˉsymbol(2, 1, 0, 4, 6, "Main"));
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            0,
            Buildˉstagingˉmanifest(49, 15, 1, 27, 5, 122),
            Buildˉstagingˉwvoˉprefix(2, 4, 0, 16),
            Readˉonlyˉheader,
            Middleˉmainˉsymbols,
            dataˉsymbols: 1,
            functions: 3,
            relocationˉposition: 219,
            relocationˉbytes: 0);

        var Invalidˉprefix = Prefix.ToArray();
        Invalidˉprefix[0] = (byte)'X';
        Assertˉstagingˉwvoˉsymbols(
            Adapter, 1, Manifest, Invalidˉprefix, Emptyˉheader, Main);
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            2,
            Buildˉstagingˉmanifest(49, 10, 25),
            Prefix,
            Emptyˉheader,
            Main);
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            3,
            Buildˉstagingˉmanifest(49, 10, 21),
            Prefix,
            Emptyˉheader,
            Main[..21]);
        Assertˉchangedˉstagingˉwvoˉsymbol(
            Adapter, 4, Manifest, Prefix, Main, 2, 1);
        Assertˉchangedˉstagingˉwvoˉsymbol(
            Adapter, 5, Manifest, Prefix, Main, 0, 3);
        Assertˉchangedˉstagingˉwvoˉsymbol(
            Adapter, 6, Manifest, Prefix, Main, 20, (byte)'X');

        var Skippedˉfunction = Combineˉstagingˉwvoˉrecords(
            Buildˉstagingˉwvoˉsymbol(1, 1, 0, 0, 4, "$function_0002"),
            Buildˉstagingˉwvoˉsymbol(2, 1, 0, 4, 6, "Main"));
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            7,
            Buildˉstagingˉmanifest(49, 10, 58),
            Buildˉstagingˉwvoˉprefix(1, 2, 0, 10),
            Emptyˉheader,
            Skippedˉfunction);

        var Outˉofˉrangeˉmain = Buildˉstagingˉwvoˉsymbol(2, 1, 0, 0, 11, "Main");
        Assertˉstagingˉwvoˉsymbols(
            Adapter, 8, Manifest, Prefix, Emptyˉheader, Outˉofˉrangeˉmain);

        var Excessiveˉsymbolˉbytes = new byte[577 * 21];
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            9,
            Buildˉstagingˉmanifest(49, 10, checked((uint)Excessiveˉsymbolˉbytes.Length)),
            Buildˉstagingˉwvoˉprefix(1, 577, 0, 10),
            Emptyˉheader,
            Excessiveˉsymbolˉbytes);

        var Incompleteˉdata = Symbols.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Incompleteˉdata.AsSpan(12), 4);
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            10,
            Readˉonlyˉmanifest,
            Readˉonlyˉprefix,
            Readˉonlyˉheader,
            Incompleteˉdata);

        var Incompleteˉfunction = Buildˉstagingˉwvoˉsymbol(2, 1, 0, 0, 9, "Main");
        Assertˉstagingˉwvoˉsymbols(
            Adapter, 11, Manifest, Prefix, Emptyˉheader, Incompleteˉfunction);
        Assertˉstagingˉwvoˉsymbols(
            Adapter,
            12,
            Buildˉstagingˉmanifest(49, 10, 27, 5, 88, 19),
            Readˉonlyˉprefix,
            Readˉonlyˉheader,
            Symbols);

        var Nativeˉadapterˉbytes = Compileˉstagingˉwvoˉsymbolsˉadapter(
            "Tests/Fixtures/Native-X64/Wvo-Staging-Symbols-Native-Adapter.wv",
            WVO_STAGING_SYMBOLS_NATIVE_ADAPTER_SOURCE);
        Equal(
            WVO_STAGING_SYMBOLS_NATIVE_ADAPTER_WVB_BYTES,
            Nativeˉadapterˉbytes.Length);
        Equal(
            WVO_STAGING_SYMBOLS_NATIVE_ADAPTER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Nativeˉadapterˉbytes));
        var Nativeˉadapter = Moduleˉcodec.Readˉandˉverify(Nativeˉadapterˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉstagingˉwvoˉsymbolsˉnativeˉtest",
            Nativeˉadapter.Module.Name);
        var Native = X64ˉnativeˉbackend.Compile(Nativeˉadapter);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Equal(0, Native.Fragment.Requiredˉservices.Length);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Native.Fragment));
    }

    private static byte[] Compileˉstagingˉwvoˉsymbolsˉadapter(
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
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv",
                    NATIVE_X64_STAGING_WVO_SYMBOLS_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols-Native-Bridge.wv",
                    NATIVE_X64_STAGING_WVO_SYMBOLS_NATIVE_BRIDGE_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale staging WVO symbol adapter compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }

    private static void Assertˉchangedˉstagingˉwvoˉsymbol(
        Verifiedˉmodule adapter,
        uint status,
        byte[] manifest,
        byte[] prefix,
        byte[] symbols,
        int offset,
        byte value)
    {
        var Changed = symbols.ToArray();
        Changed[offset] = value;
        Assertˉstagingˉwvoˉsymbols(
            adapter,
            status,
            manifest,
            prefix,
            Array.Empty<byte>(),
            Changed);
    }

    private static void Assertˉstagingˉwvoˉsymbols(
        Verifiedˉmodule adapter,
        uint status,
        byte[] manifest,
        byte[] prefix,
        byte[] readˉonlyˉheader,
        byte[] symbols,
        uint dataˉsymbols = 0,
        uint functions = 0,
        uint relocationˉposition = 0,
        uint relocationˉbytes = 0)
    {
        var Request = new byte[checked(
            16 + manifest.Length + prefix.Length + readˉonlyˉheader.Length + symbols.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Request, checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(4), checked((uint)prefix.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(8),
            checked((uint)readˉonlyˉheader.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(12), checked((uint)symbols.Length));
        manifest.CopyTo(Request, 16);
        prefix.CopyTo(Request, 16 + manifest.Length);
        readˉonlyˉheader.CopyTo(Request, 16 + manifest.Length + prefix.Length);
        symbols.CopyTo(
            Request,
            16 + manifest.Length + prefix.Length + readˉonlyˉheader.Length);
        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults)
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        Equal(24, Evidence.Length);
        Sequenceˉequal("WVSS"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        var Expected = new uint[]
        {
            status,
            dataˉsymbols,
            functions,
            relocationˉposition,
            relocationˉbytes,
        };
        for (var Index = 0; Index < Expected.Length; Index++)
        {
            Equal(
                Expected[Index],
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Evidence.AsSpan(4 + Index * 4)));
        }
    }

    private static byte[] Buildˉstagingˉwvoˉsymbol(
        byte binding,
        byte kind,
        uint section,
        uint offset,
        uint size,
        string name)
    {
        var Name = Encoding.ASCII.GetBytes(name);
        var Result = new byte[checked(20 + Name.Length)];
        Result[0] = binding;
        Result[1] = kind;
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), section);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), offset);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), size);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), checked((uint)Name.Length));
        Name.CopyTo(Result, 20);
        return Result;
    }

    private static byte[] Combineˉstagingˉwvoˉrecords(params byte[][] records)
    {
        var Length = records.Sum(Record => Record.Length);
        var Result = new byte[Length];
        var Offset = 0;
        foreach (var Record in records)
        {
            Record.CopyTo(Result, Offset);
            Offset += Record.Length;
        }
        return Result;
    }
}
