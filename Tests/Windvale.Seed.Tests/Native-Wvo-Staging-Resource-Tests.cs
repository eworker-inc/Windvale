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
    private static readonly string NATIVE_X64_STAGING_RESOURCES_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Resources.wv");
    private static readonly string NATIVE_X64_STAGING_ADMISSION_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Lowering-Staging-Admission-Tool.wv");
    private static readonly string WVO_STAGING_RESOURCES_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvo-Staging-Resources-Adapter.wv");

    private static void Nativeˉwvoˉstagingˉresourcesˉareˉbound()
    {
        var Resourceˉadapter = Compileˉstagingˉresourceˉadapter();
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "stage/object",
            "stage/object.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(49, 406, 24),
            1,
            status: 0,
            chunks: 3,
            snapshots: 5,
            ordinal: 3,
            chunkˉresource: "stage/object.chunk-1");
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "",
            "stage/object.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(1),
            0,
            status: 2);
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "stage/object",
            "input.wvb",
            "output.wvo",
            Buildˉstagingˉmanifest(1),
            0,
            status: 3);
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "stage/object.chunk-0",
            "stage/object",
            "stage/object.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(1),
            0,
            status: 3);
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "stage/object",
            "stage/object.wvop",
            "stage/object.chunk-0",
            Buildˉstagingˉmanifest(1),
            0,
            status: 3);
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            new string('p', 4_078),
            "manifest.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(1),
            0,
            status: 0,
            chunks: 1,
            snapshots: 3,
            ordinal: 2,
            chunkˉresource: new string('p', 4_078) + ".chunk-0");
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            new string('p', 4_079),
            "manifest.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(1),
            0,
            status: 2);
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "stage/object",
            "manifest.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(
                Enumerable.Repeat(1u, 62).ToArray()),
            61,
            status: 0,
            chunks: 62,
            snapshots: 64,
            ordinal: 63,
            chunkˉresource: "stage/object.chunk-61");
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "stage/object",
            "manifest.wvop",
            "output.wvo",
            Buildˉstagingˉmanifest(
                Enumerable.Repeat(1u, 63).ToArray()),
            62,
            status: 5);
        Assertˉresourceˉplan(
            Resourceˉadapter,
            "input.wvb",
            "stage/object",
            "manifest.wvop",
            "output.wvo",
            [0],
            0,
            status: 4);

        var Admissionˉbytes = Compileˉwvbˉtoˉwvoˉapplicationˉsuccess(
            "Compiler/Windvale/Native-X64-Lowering-Staging-Admission-Tool.wv",
            NATIVE_X64_STAGING_ADMISSION_SOURCE,
            "staging-resource admission tool",
            includeˉpublication: true,
            includeˉstagingˉmanifest: true,
            includeˉstagingˉcontent: true,
            includeˉstagingˉresources: true);
        var Admission = Moduleˉcodec.Readˉandˉverify(Admissionˉbytes);
        Equal(
            "Compilerˉnativeˉx64ˉloweringˉstagingˉadmissionˉtool",
            Admission.Module.Name);
        Equal(Moduleˉprofile.Hosted, Admission.Module.Profile);
        Sequenceˉequal(
            [
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
                Capabilityˉcatalog.FILE_READ_BYTES,
                Capabilityˉcatalog.PROCESS_ARGUMENT,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            ],
            Admission.Module.Capabilities.Select(Item => Item.Name));

        var Fixture = Buildˉstagingˉcontentˉfixture(
            WVB_TO_WVO_RETURN_42_SOURCE);
        var Manifest = Buildˉstagingˉmanifest(
            Fixture.Chunks.Select(Chunk => checked((uint)Chunk.Length)).ToArray());
        var Reference = Runˉstagingˉadmission(
            Admission,
            Fixture.Wvb,
            Manifest,
            Fixture.Chunks,
            changeˉchunk: -1);
        Equal(0, Reference.Result.Exitˉcode);
        Equal(string.Empty, Reference.Diagnostics);
        Sequenceˉequal(
            [
                "input.wvb",
                "stage/object.wvop",
                "stage/object.chunk-0",
                "stage/object.chunk-1",
                "stage/object.chunk-2",
            ],
            Reference.Reads);

        var Changed = Runˉstagingˉadmission(
            Admission,
            Fixture.Wvb,
            Manifest,
            Fixture.Chunks,
            changeˉchunk: Fixture.Codeˉchunk);
        Equal(1, Changed.Result.Exitˉcode);
        Equal(
            "native x64 staging admission content=Content\n",
            Changed.Diagnostics);
        Equal(Fixture.Codeˉchunk + 3, Changed.Reads.Length);

        Assertˉnativeˉstagingˉadmission(
            Admission,
            Fixture.Wvb,
            Manifest,
            Fixture.Chunks,
            Reference.Result.Executedˉinstructions);
    }

    private static Verifiedˉmodule Compileˉstagingˉresourceˉadapter()
    {
        var Result = Seedˉcompiler.Compileˉmodules(
            new(
                "Tests/Fixtures/Native-X64/Wvo-Staging-Resources-Adapter.wv",
                WVO_STAGING_RESOURCES_ADAPTER_SOURCE),
            [
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Manifest.wv",
                    NATIVE_X64_LOWERING_STAGING_MANIFEST_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Staging-Resources.wv",
                    NATIVE_X64_STAGING_RESOURCES_SOURCE),
            ]);
        True(
            Result.Success,
            "The staging-resource adapter did not compile: " +
                string.Join(" | ", Result.Diagnostics));
        return Moduleˉcodec.Readˉandˉverify(Result.Moduleˉbytes.AsSpan());
    }

    private static void Assertˉresourceˉplan(
        Verifiedˉmodule adapter,
        string inputˉresource,
        string prefix,
        string manifestˉresource,
        string destinationˉresource,
        byte[] manifest,
        uint query,
        uint status,
        uint chunks = 0,
        uint snapshots = 0,
        uint ordinal = uint.MaxValue,
        string chunkˉresource = "")
    {
        var Values = new[]
        {
            Encoding.UTF8.GetBytes(inputˉresource),
            Encoding.UTF8.GetBytes(prefix),
            Encoding.UTF8.GetBytes(manifestˉresource),
            Encoding.UTF8.GetBytes(destinationˉresource),
        };
        var Request = new byte[checked(
            24 + Values.Sum(Value => Value.Length) + manifest.Length)];
        for (var Index = 0; Index < Values.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(
                Request.AsSpan(Index * 4),
                checked((uint)Values[Index].Length));
        }
        BinaryPrimitives.WriteUInt32LittleEndian(
            Request.AsSpan(16),
            checked((uint)manifest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Request.AsSpan(20), query);
        var Position = 24;
        foreach (var Value in Values.Append(manifest))
        {
            Value.CopyTo(Request, Position);
            Position += Value.Length;
        }

        var Evidence = new Referenceˉruntime(
            adapter,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with
            {
                Maximumˉinstructions = 50_000_000,
            })
            .Runˉmainˉbytes(Request.ToImmutableArray())
            .Bytes
            .ToArray();
        True(Evidence.Length >= 24, "The staging-resource evidence is truncated.");
        Sequenceˉequal("WVRI"u8.ToArray(), Evidence.AsSpan(0, 4).ToArray());
        Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(4)));
        Equal(chunks, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(8)));
        Equal(snapshots, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(12)));
        Equal(ordinal, BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(16)));
        var Nameˉlength = BinaryPrimitives.ReadUInt32LittleEndian(Evidence.AsSpan(20));
        Equal(checked((uint)Encoding.UTF8.GetByteCount(chunkˉresource)), Nameˉlength);
        Equal(checked(24 + (int)Nameˉlength), Evidence.Length);
        Sequenceˉequal(
            Encoding.UTF8.GetBytes(chunkˉresource),
            Evidence.AsSpan(24).ToArray());
    }

    private static (
        Runtimeˉresult Result,
        string Diagnostics,
        string[] Reads) Runˉstagingˉadmission(
            Verifiedˉmodule admission,
            byte[] wvb,
            byte[] manifest,
            byte[][] chunks,
            int changeˉchunk)
    {
        var Values = new Dictionary<string, ImmutableArray<byte>>(
            StringComparer.Ordinal)
        {
            ["input.wvb"] = wvb.ToImmutableArray(),
            ["stage/object.wvop"] = manifest.ToImmutableArray(),
        };
        for (var Index = 0; Index < chunks.Length; Index++)
        {
            var Value = chunks[Index].ToArray();
            if (Index == changeˉchunk)
            {
                Value[0] ^= 1;
            }
            Values[$"stage/object.chunk-{Index}"] = Value.ToImmutableArray();
        }
        var Reads = new List<string>();
        var Reader = new Testˉfileˉreader((Resourceˉname, Maximumˉbytes) =>
        {
            Reads.Add(Resourceˉname);
            True(
                Values.TryGetValue(Resourceˉname, out var Value),
                $"The admission tool requested unexpected resource '{Resourceˉname}'.");
            True(
                Value.Length <= Maximumˉbytes,
                "The admission tool exceeded the hosted file-read bound.");
            return Value;
        });
        using var Diagnostics = new StringWriter();
        var Resources = new Hostedˉresourceˉcontext(
            ["input.wvb", "stage/object", "stage/object.wvop", "output.wvo"],
            TextWriter.Null,
            Diagnostics,
            Reader);
        var Authorized = admission.Module.Capabilities
            .Select(Item => Item.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            admission,
            new Referenceˉcapabilityˉhost(Resources),
            new Runtimeˉoptions(Authorized) with
            {
                Maximumˉinstructions = 200_000_000,
            })
            .Runˉmain();
        return (Result, Diagnostics.ToString(), [.. Reads]);
    }

    private static void Assertˉnativeˉstagingˉadmission(
        Verifiedˉmodule admission,
        byte[] wvb,
        byte[] manifest,
        byte[][] chunks,
        long maximumˉinstructions)
    {
        var Native = X64ˉnativeˉbackend.Compile(admission);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-staging-admission-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Inputˉpath = Path.Combine(Directoryˉpath, "input.wvb");
            var Prefix = Path.Combine(Directoryˉpath, "object");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "object.wvop");
            var Destinationˉpath = Path.Combine(Directoryˉpath, "output.wvo");
            File.WriteAllBytes(Inputˉpath, wvb);
            File.WriteAllBytes(Manifestˉpath, manifest);
            for (var Index = 0; Index < chunks.Length; Index++)
            {
                File.WriteAllBytes($"{Prefix}.chunk-{Index}", chunks[Index]);
            }
            using var Diagnostic = new Nativeˉoutputˉcapture();
            var Resources = new Hostedˉresourceˉcontext(
                [Inputˉpath, Prefix, Manifestˉpath, Destinationˉpath],
                TextWriter.Null,
                TextWriter.Null);
            var Authorized = admission.Module.Capabilities
                .Select(Item => Item.Name)
                .ToImmutableHashSet(StringComparer.Ordinal);
            var Host = new Nativeˉhostˉservices(
                null,
                Authorized,
                Resources,
                Diagnostic.Channel,
                Nativeˉfileˉinput.Hostˉfileˉsystem());
            Equal(
                0,
                X64ˉnativeˉexecutor.Executeˉi32(
                    Native.Fragment,
                    maximumˉinstructions: maximumˉinstructions,
                    hostˉservices: Host));
            Equal(string.Empty, Diagnostic.Readˉtext());
            False(File.Exists(Destinationˉpath),
                "Admission must not publish the destination before the platform transaction.");
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
