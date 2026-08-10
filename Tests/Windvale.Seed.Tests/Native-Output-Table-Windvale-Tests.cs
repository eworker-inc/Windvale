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
    private const int NATIVE_OUTPUT_TABLE_CORE_SIZE = 4_710;
    private const string NATIVE_OUTPUT_TABLE_CORE_SHA256 =
        "ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8";

    private static void Windvaleˉnativeˉoutputˉtableˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Output-Table-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Output-Table-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Output-Table-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Output-Table-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native output-table core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_OUTPUT_TABLE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_OUTPUT_TABLE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native output-table bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(Nativeˉoutputˉtableˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉoutputˉtableˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Output-Table-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉoutputˉtableˉbuilder),
            "Windvale.Native.Native-Output-Table-Bridge.wvnf");
        Equal(
            Nativeˉoutputˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉoutputˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉoutputˉtableˉbuilder).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-Output-Table-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the native output-table WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The output-table constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Cases = new[]
        {
            (Platform: Nativeˉoutputˉplatform.Windows, Flags: 1u,
                Console: 0x1122_3344_5566_7788UL, Diagnostic: 0UL,
                Writer: 0x8877_6655_4433_2211UL),
            (Platform: Nativeˉoutputˉplatform.Windows, Flags: 2u,
                Console: 0UL, Diagnostic: 0x0102_0304_0506_0708UL,
                Writer: 0x8877_6655_4433_2211UL),
            (Platform: Nativeˉoutputˉplatform.Windows, Flags: 3u,
                Console: 0x1122_3344_5566_7788UL, Diagnostic: 0x0102_0304_0506_0708UL,
                Writer: 0x8877_6655_4433_2211UL),
            (Platform: Nativeˉoutputˉplatform.Linux, Flags: 1u,
                Console: 1UL, Diagnostic: 0UL, Writer: 0UL),
            (Platform: Nativeˉoutputˉplatform.Linux, Flags: 2u,
                Console: 0UL, Diagnostic: 2UL, Writer: 0UL),
            (Platform: Nativeˉoutputˉplatform.Linux, Flags: 3u,
                Console: 1UL, Diagnostic: 2UL, Writer: 0UL),
        };
        foreach (var Case in Cases)
        {
            var Request = Nativeˉoutputˉtableˉbuilder.Buildˉrequest(
                Case.Platform,
                Case.Flags,
                Case.Console,
                Case.Diagnostic,
                Case.Writer);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉoutputˉtableˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            var Table = Nativeˉoutputˉtableˉbuilder.Verifyˉresponse(
                Case.Platform,
                Case.Flags,
                Case.Console,
                Case.Diagnostic,
                Case.Writer,
                Request.Length,
                Executed);
            Sequenceˉequal(
                Expectedˉnativeˉoutputˉtable(
                    Case.Platform,
                    Case.Flags,
                    Case.Console,
                    Case.Diagnostic,
                    Case.Writer),
                Table);
            Sequenceˉequal(
                Table,
                Nativeˉoutputˉtableˉbuilder.Build(
                    Case.Platform,
                    Case.Flags,
                    Case.Console,
                    Case.Diagnostic,
                    Case.Writer));
        }

        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }

        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉoutputˉtableˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉoutputˉtableˉbuilder.Buildˉrequest(
            Nativeˉoutputˉplatform.Windows,
            3,
            11,
            12,
            13);
        Expectˉfailure(Valid[..^1], 1, 47);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 47), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 20, 1), 4, 20);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 5, 12);
        Expectˉfailure(Replaceˉu32(Valid, 16, 0), 6, 16);
        Expectˉfailure(Replaceˉu32(Valid, 24, 0), 7, 24);
        Expectˉfailure(Replaceˉu32(Valid, 40, 0), 8, 40);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-output-table-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Output-Table.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Runtime", "Windvale", "Native-Output-Table.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(Bridgeˉresult.Moduleˉbytes, File.ReadAllBytes(Nativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static ImmutableArray<byte> Expectedˉnativeˉoutputˉtable(
        Nativeˉoutputˉplatform platform,
        uint flags,
        ulong consoleˉtarget,
        ulong diagnosticˉtarget,
        ulong writeˉfunction)
    {
        var Result = new byte[Nativeˉoutputˉtableˉcontract.SIZE];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, Nativeˉoutputˉtableˉcontract.MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Nativeˉoutputˉtableˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), Nativeˉoutputˉtableˉcontract.SIZE);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)platform);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), flags);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(24), consoleˉtarget);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(32), diagnosticˉtarget);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(40), writeˉfunction);
        return Result.ToImmutableArray();
    }
}
