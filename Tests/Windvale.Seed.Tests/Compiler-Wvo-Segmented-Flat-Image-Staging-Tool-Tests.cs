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
    private static readonly string COMPILER_FLAT_IMAGE_STAGING_RESOURCES_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Flat-Image-Staging-Resources.wv");
    private static readonly string COMPILER_WVO_SEGMENTED_FLAT_IMAGE_STAGING_TOOL_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Compiler-Wvo-Segmented-Flat-Image-Staging-Tool.wv");

    private static void Compilerˉwvoˉsegmentedˉflatˉimageˉstagingˉruns()
    {
        const string Sourceˉprefix = "stage/compiler";
        const string Sourceˉmanifestˉresource = "stage/compiler.wvop";
        const string Outputˉprefix = "stage/image";
        const string Outputˉmanifestˉresource = "stage/image.wvli";

        var Toolˉbytes = Compileˉcompilerˉwvoˉsegmentedˉstagingˉtool();
        var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
        Equal(
            "Linkerˉcompilerˉwvoˉsegmentedˉflatˉimageˉstagingˉtool",
            Tool.Module.Name);

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
        byte[] Readˉonlyˉfirst = [65, 66];
        byte[] Readˉonlyˉsecond = [67, 68, 69];
        byte[][] Sourceˉchunks =
        [
            Prefix,
            Text,
            Padding,
            Readˉonlyˉheader,
            Readˉonlyˉfirst,
            Readˉonlyˉsecond,
            Symbols,
            Relocations,
        ];
        byte[][] Expectedˉchunks =
        [
            Linkedˉtext,
            Padding,
            Readˉonlyˉfirst,
            Readˉonlyˉsecond,
        ];
        var Inputs = new Dictionary<string, ImmutableArray<byte>>(
            StringComparer.Ordinal)
        {
            [Sourceˉmanifestˉresource] = Manifest.ToImmutableArray(),
        };
        for (var Index = 0; Index < Sourceˉchunks.Length; Index++)
        {
            Inputs[$"{Sourceˉprefix}.chunk-{Index}"] =
                Sourceˉchunks[Index].ToImmutableArray();
        }

        var Reader = new Testˉfileˉreader((Resourceˉname, Maximumˉbytes) =>
        {
            True(
                Inputs.TryGetValue(Resourceˉname, out var Value),
                $"Unexpected staged compiler resource '{Resourceˉname}'.");
            True(
                Value.Length <= Maximumˉbytes,
                "A staged compiler resource exceeded the hosted read bound.");
            return Value;
        });
        var Writer = new Capturingˉstagingˉwriter();
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Context = new Hostedˉresourceˉcontext(
            [
                Sourceˉprefix,
                Sourceˉmanifestˉresource,
                Outputˉprefix,
                Outputˉmanifestˉresource,
            ],
            Output,
            Diagnostics,
            Reader,
            Writer);
        var Authorized = Tool.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(Context),
            new Runtimeˉoptions(Authorized) with
            {
                Maximumˉinstructions = 500_000_000,
            })
            .Runˉmain();
        Equal(0, Result.Exitˉcode);
        Equal(string.Empty, Diagnostics.ToString());
        Equal(1 + Sourceˉchunks.Length, Reader.Readˉcount);
        Equal(1 + Expectedˉchunks.Length, Writer.Writes.Count);

        for (var Index = 0; Index < Expectedˉchunks.Length; Index++)
        {
            var Write = Writer.Writes[Index];
            Equal($"{Outputˉprefix}.chunk-{Index}", Write.Resourceˉname);
            Sequenceˉequal(Expectedˉchunks[Index], Write.Bytes.ToArray());
        }
        var Manifestˉwrite = Writer.Writes[^1];
        Equal(Outputˉmanifestˉresource, Manifestˉwrite.Resourceˉname);
        var Imageˉmanifest = Manifestˉwrite.Bytes.AsSpan();
        Equal(76, Imageˉmanifest.Length);
        Sequenceˉequal("WVLI"u8.ToArray(), Imageˉmanifest[..4].ToArray());
        Equal(21u, BinaryPrimitives.ReadUInt32LittleEndian(Imageˉmanifest[12..]));
        Equal(4u, BinaryPrimitives.ReadUInt32LittleEndian(Imageˉmanifest[16..]));
        Equal(4u, BinaryPrimitives.ReadUInt32LittleEndian(Imageˉmanifest[20..]));
        uint Position = 0;
        for (var Index = 0; Index < Expectedˉchunks.Length; Index++)
        {
            var Entry = Imageˉmanifest.Slice(28 + Index * 12, 12);
            Equal((uint)Index, BinaryPrimitives.ReadUInt32LittleEndian(Entry));
            Equal(Position, BinaryPrimitives.ReadUInt32LittleEndian(Entry[4..]));
            Equal(
                (uint)Expectedˉchunks[Index].Length,
                BinaryPrimitives.ReadUInt32LittleEndian(Entry[8..]));
            Position += (uint)Expectedˉchunks[Index].Length;
        }
        Equal(21u, Position);
        Equal(
            "segmented compiler image staging status=Complete " +
            "image-bytes=21 chunks=4 manifest-bytes=76\n",
            Output.ToString());

        var Native = X64ˉnativeˉbackend.Compile(Tool);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
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
            Native.Fragment.Requiredˉservices);

        var Rejectedˉreader = new Testˉfileˉreader((_, _) =>
            throw new InvalidOperationException(
                "Resource collision must fail before file acquisition."));
        var Rejectedˉwriter = new Capturingˉstagingˉwriter();
        var Rejectedˉdiagnostics = new StringWriter();
        var Rejectedˉcontext = new Hostedˉresourceˉcontext(
            [
                Sourceˉprefix,
                Sourceˉmanifestˉresource,
                Sourceˉprefix,
                Outputˉmanifestˉresource,
            ],
            TextWriter.Null,
            Rejectedˉdiagnostics,
            Rejectedˉreader,
            Rejectedˉwriter);
        var Rejected = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(Rejectedˉcontext),
            new Runtimeˉoptions(Authorized) with
            {
                Maximumˉinstructions = 500_000_000,
            })
            .Runˉmain();
        Equal(1, Rejected.Exitˉcode);
        Equal(0, Rejectedˉreader.Readˉcount);
        Equal(0, Rejectedˉwriter.Writes.Count);
        Equal(
            "segmented compiler image staging resource=Collision\n",
            Rejectedˉdiagnostics.ToString());
    }

    private static byte[] Compileˉcompilerˉwvoˉsegmentedˉstagingˉtool()
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(
                "Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Staging-Tool.wv",
                COMPILER_WVO_SEGMENTED_FLAT_IMAGE_STAGING_TOOL_SOURCE),
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
                new(
                    "Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image.wv",
                    COMPILER_WVO_SEGMENTED_FLAT_IMAGE_SOURCE),
                new(
                    "Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Verification.wv",
                    COMPILER_WVO_SEGMENTED_FLAT_IMAGE_VERIFICATION_SOURCE),
                new(
                    "Linker/Windvale/Compiler-Flat-Image-Staging-Manifest.wv",
                    COMPILER_FLAT_IMAGE_STAGING_MANIFEST_SOURCE),
                new(
                    "Linker/Windvale/Compiler-Flat-Image-Staging-Resources.wv",
                    COMPILER_FLAT_IMAGE_STAGING_RESOURCES_SOURCE),
            ]);
        if (!Result.Success)
        {
            throw new InvalidOperationException(
                "Windvale segmented compiler-image staging compilation failed: " +
                string.Join(" | ", Result.Diagnostics));
        }
        return Result.Moduleˉbytes.ToArray();
    }
}
