using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_FILE_OUTPUT_TABLE_CORE_SIZE = 3_926;
    private const string NATIVE_FILE_OUTPUT_TABLE_CORE_SHA256 =
        "fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f";

    private static void Windvaleˉnativeˉfileˉoutputˉtableˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-File-Output-Table-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-File-Output-Table-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-File-Output-Table-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-File-Output-Table-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native file-output-table core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_FILE_OUTPUT_TABLE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_FILE_OUTPUT_TABLE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native file-output-table bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉfileˉoutputˉtableˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉfileˉoutputˉtableˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-File-Output-Table-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉfileˉoutputˉtableˉbuilder),
            "Windvale.Native.Native-File-Output-Table-Bridge.wvnf");
        Equal(
            Nativeˉfileˉoutputˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉfileˉoutputˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉfileˉoutputˉtableˉbuilder).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-File-Output-Table-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the native file-output-table WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The file-output-table constructor requires a service.");

        var Windowsˉfunctions = ImmutableArray.Create(
            0x1000_0000_0000_0001UL,
            0x2000_0000_0000_0002UL,
            0x3000_0000_0000_0003UL,
            0x4000_0000_0000_0004UL,
            0x5000_0000_0000_0005UL,
            0x6000_0000_0000_0006UL);
        var Linuxˉfunctions = ImmutableArray.Create<ulong>(0, 0, 0, 0, 0, 0);
        var Cases = new[]
        {
            (Platform: Nativeˉfileˉinputˉplatform.Windows,
                Scratch: 0x0102_0304_0506_0708UL, Bytes: 2_097_154u,
                Functions: Windowsˉfunctions),
            (Platform: Nativeˉfileˉinputˉplatform.Linux,
                Scratch: 0x1112_1314_1516_1718UL, Bytes: 1_048_577u,
                Functions: Linuxˉfunctions),
        };
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Case in Cases)
        {
            var Request = Nativeˉfileˉoutputˉtableˉbuilder.Buildˉrequest(
                Case.Platform,
                Case.Scratch,
                Case.Bytes,
                Case.Functions);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed =
                Nativeˉfileˉoutputˉtableˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            var Table = Nativeˉfileˉoutputˉtableˉbuilder.Verifyˉresponse(
                Case.Platform,
                Case.Scratch,
                Case.Bytes,
                Case.Functions,
                Request.Length,
                Executed);
            Sequenceˉequal(
                Expectedˉnativeˉfileˉoutputˉtable(
                    Case.Platform,
                    Case.Scratch,
                    Case.Bytes,
                    Case.Functions),
                Table);
            Sequenceˉequal(
                Table,
                Nativeˉfileˉoutputˉtableˉbuilder.Build(
                    Case.Platform,
                    Case.Scratch,
                    Case.Bytes,
                    Case.Functions));
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
            var Executed =
                Nativeˉfileˉoutputˉtableˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉfileˉoutputˉtableˉbuilder.Buildˉrequest(
            Nativeˉfileˉinputˉplatform.Windows,
            11,
            2_097_154,
            Windowsˉfunctions);
        Expectˉfailure(Valid[..^1], 1, 79);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 79), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 28, 1), 4, 28);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 5, 12);
        Expectˉfailure(Replaceˉu32(Valid, 16, 0), 6, 16);
        Expectˉfailure(Replaceˉu32(Valid, 24, 1), 6, 24);
        Expectˉfailure(Replaceˉu32(Replaceˉu32(Valid, 32, 0), 36, 0), 7, 32);
        var Invalidˉlinux = Nativeˉfileˉoutputˉtableˉbuilder.Buildˉrequest(
            Nativeˉfileˉinputˉplatform.Linux,
            11,
            1_048_577,
            Linuxˉfunctions);
        Expectˉfailure(Replaceˉu32(Invalidˉlinux, 32, 1), 7, 32);

        var Host = new Nativeˉhostˉservices(
            null,
            fileˉoutput: Nativeˉfileˉoutput.Hostˉfileˉsystem());
        using (var Context = new Nativeˉfileˉoutputˉcontext(Host, required: true))
        {
            Context.Verifyˉcompleted();
            Marshal.WriteByte(
                Context.Address,
                Nativeˉfileˉoutputˉtableˉcontract.RESERVED_OFFSET,
                1);
            Throwsˉinvalidˉoperation(
                "The native file-output table violated its independently verified static layout.",
                Context.Verifyˉcompleted);
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-output-table-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-File-Output-Table.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-File-Output-Table.wvproj"),
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

    private static ImmutableArray<byte> Expectedˉnativeˉfileˉoutputˉtable(
        Nativeˉfileˉinputˉplatform platform,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions)
    {
        var Result = new byte[Nativeˉfileˉoutputˉtableˉcontract.SIZE];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result, Nativeˉfileˉoutputˉtableˉcontract.MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Nativeˉfileˉoutputˉtableˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(8), Nativeˉfileˉoutputˉtableˉcontract.SIZE);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)platform);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), scratchˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(24), scratchˉbytes);
        for (var Index = 0; Index < functions.Length; Index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(32 + Index * sizeof(ulong)), functions[Index]);
        }
        return Result.ToImmutableArray();
    }
}
