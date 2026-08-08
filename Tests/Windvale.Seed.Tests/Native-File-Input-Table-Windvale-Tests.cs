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
    private const int NATIVE_FILE_INPUT_TABLE_CORE_SIZE = 5_078;
    private const string NATIVE_FILE_INPUT_TABLE_CORE_SHA256 =
        "0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438";

    private static void Windvaleˉnativeˉfileˉinputˉtableˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-File-Input-Table-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-File-Input-Table-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-File-Input-Table-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-File-Input-Table-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native file-input-table core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_FILE_INPUT_TABLE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_FILE_INPUT_TABLE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native file-input-table bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉfileˉinputˉtableˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉfileˉinputˉtableˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-File-Input-Table-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉfileˉinputˉtableˉbuilder),
            "Windvale.Native.Native-File-Input-Table-Bridge.wvnf");
        Equal(
            Nativeˉfileˉinputˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉfileˉinputˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉfileˉinputˉtableˉbuilder).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-File-Input-Table-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the native file-input-table WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The file-input-table constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Windowsˉfunctions = ImmutableArray.Create(
            0x1000_0000_0000_0001UL,
            0x2000_0000_0000_0002UL,
            0x3000_0000_0000_0003UL,
            0x4000_0000_0000_0004UL,
            0x5000_0000_0000_0005UL,
            0x6000_0000_0000_0006UL,
            0x7000_0000_0000_0007UL);
        var Linuxˉfunctions = ImmutableArray.Create<ulong>(0, 0, 0, 0, 0, 0, 0);
        var Cases = new[]
        {
            (Platform: Nativeˉfileˉinputˉplatform.Windows,
                Snapshot: 0x0102_0304_0506_0708UL,
                Names: 0x1112_1314_1516_1718UL,
                Data: 0x2122_2324_2526_2728UL,
                Scratch: 0x3132_3334_3536_3738UL,
                Bytes: 2_097_154u,
                Functions: Windowsˉfunctions),
            (Platform: Nativeˉfileˉinputˉplatform.Linux,
                Snapshot: 0x4142_4344_4546_4748UL,
                Names: 0x5152_5354_5556_5758UL,
                Data: 0x6162_6364_6566_6768UL,
                Scratch: 0x7172_7374_7576_7778UL,
                Bytes: 1_048_577u,
                Functions: Linuxˉfunctions),
        };
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Case in Cases)
        {
            var Request = Nativeˉfileˉinputˉtableˉbuilder.Buildˉrequest(
                Case.Platform,
                Case.Snapshot,
                Case.Names,
                Case.Data,
                Case.Scratch,
                Case.Bytes,
                Case.Functions);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉfileˉinputˉtableˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            var Table = Nativeˉfileˉinputˉtableˉbuilder.Verifyˉresponse(
                Case.Platform,
                Case.Snapshot,
                Case.Names,
                Case.Data,
                Case.Scratch,
                Case.Bytes,
                Case.Functions,
                Request.Length,
                Executed);
            Sequenceˉequal(
                Expectedˉnativeˉfileˉinputˉtable(
                    Case.Platform,
                    Case.Snapshot,
                    Case.Names,
                    Case.Data,
                    Case.Scratch,
                    Case.Bytes,
                    Case.Functions),
                Table);
            Sequenceˉequal(
                Table,
                Nativeˉfileˉinputˉtableˉbuilder.Build(
                    Case.Platform,
                    Case.Snapshot,
                    Case.Names,
                    Case.Data,
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

        static ImmutableArray<byte> Clearˉu64(ImmutableArray<byte> input, int offset) =>
            Replaceˉu32(Replaceˉu32(input, offset, 0), offset + 4, 0);

        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉfileˉinputˉtableˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉfileˉinputˉtableˉbuilder.Buildˉrequest(
            Nativeˉfileˉinputˉplatform.Windows,
            11,
            12,
            13,
            14,
            2_097_154,
            Windowsˉfunctions);
        Expectˉfailure(Valid[..^1], 1, 135);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 135), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 28, 1), 4, 28);
        Expectˉfailure(Replaceˉu32(Valid, 44, 1), 4, 44);
        Expectˉfailure(Replaceˉu32(Valid, 76, 1), 4, 76);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 5, 12);
        Expectˉfailure(Clearˉu64(Valid, 16), 6, 16);
        Expectˉfailure(Replaceˉu32(Valid, 24, 63), 6, 16);
        Expectˉfailure(Clearˉu64(Valid, 32), 7, 32);
        Expectˉfailure(Replaceˉu32(Valid, 40, 1), 7, 32);
        Expectˉfailure(Clearˉu64(Valid, 48), 8, 48);
        Expectˉfailure(Replaceˉu32(Valid, 56, 1), 8, 48);
        Expectˉfailure(Replaceˉu32(Valid, 60, 1), 8, 48);
        Expectˉfailure(Clearˉu64(Valid, 64), 9, 64);
        Expectˉfailure(Replaceˉu32(Valid, 72, 1), 9, 72);
        Expectˉfailure(Clearˉu64(Valid, 80), 10, 80);
        var Invalidˉlinux = Nativeˉfileˉinputˉtableˉbuilder.Buildˉrequest(
            Nativeˉfileˉinputˉplatform.Linux,
            11,
            12,
            13,
            14,
            1_048_577,
            Linuxˉfunctions);
        Expectˉfailure(Replaceˉu32(Invalidˉlinux, 80, 1), 10, 80);

        var Host = new Nativeˉhostˉservices(
            null,
            fileˉinput: Nativeˉfileˉinput.Hostˉfileˉsystem());
        using (var Context = new Nativeˉfileˉinputˉcontext(Host, required: true))
        {
            Context.Verifyˉcompleted();
            Marshal.WriteByte(
                Context.Address,
                Nativeˉfileˉinputˉtableˉcontract.RESERVED_OFFSET,
                1);
            Throwsˉinvalidˉoperation(
                "The native file-input table violated its independently verified static layout.",
                Context.Verifyˉcompleted);
        }

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-file-input-table-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-File-Input-Table.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-File-Input-Table.wvproj"),
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

    private static ImmutableArray<byte> Expectedˉnativeˉfileˉinputˉtable(
        Nativeˉfileˉinputˉplatform platform,
        ulong snapshotˉtableˉpointer,
        ulong nameˉarenaˉpointer,
        ulong dataˉarenaˉpointer,
        ulong scratchˉpointer,
        uint scratchˉbytes,
        ImmutableArray<ulong> functions)
    {
        var Result = new byte[Nativeˉfileˉinputˉtableˉcontract.SIZE];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result, Nativeˉfileˉinputˉtableˉcontract.MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(8), Nativeˉfileˉinputˉtableˉcontract.SIZE);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)platform);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(16), snapshotˉtableˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24), Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(32), nameˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(40), Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(48), dataˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(56), Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(60), Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(64), scratchˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(72), scratchˉbytes);
        for (var Index = 0; Index < functions.Length; Index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(80 + Index * sizeof(ulong)), functions[Index]);
        }
        return Result.ToImmutableArray();
    }
}
