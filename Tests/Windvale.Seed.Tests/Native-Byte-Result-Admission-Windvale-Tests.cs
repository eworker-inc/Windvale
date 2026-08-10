using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_BYTE_RESULT_ADMISSION_CORE_SIZE = 7_078;
    private const string NATIVE_BYTE_RESULT_ADMISSION_CORE_SHA256 =
        "eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04";

    private static void Windvaleˉnativeˉbyteˉresultˉadmissionˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Byte-Result-Admission-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Byte-Result-Admission-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Byte-Result-Admission-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Byte-Result-Admission-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(Coreˉresult.Success, string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_BYTE_RESULT_ADMISSION_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_BYTE_RESULT_ADMISSION_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(Bridgeˉresult.Success, string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉbyteˉresultˉadmissionˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉbyteˉresultˉadmissionˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Byte-Result-Admission-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉbyteˉresultˉadmissionˉbuilder),
            "Windvale.Native.Native-Byte-Result-Admission-Bridge.wvnf");
        Equal(
            Nativeˉbyteˉresultˉadmissionˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉbyteˉresultˉadmissionˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉbyteˉresultˉadmissionˉbuilder).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-Byte-Result-Admission-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the native byte-result admission WVB.");

        var Module = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The byte-result admission requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Ranges = ImmutableArray.Create(
            new Nativeˉbyteˉresultˉrange(0x3000, 20),
            new Nativeˉbyteˉresultˉrange(0x0000_0001_FFFF_FFF0, 64));
        Nativeˉbyteˉresultˉadmissionˉinputs Inputs(
            ulong pointer,
            uint length,
            uint reserved = 0) =>
            new(new(pointer, length, reserved), 0x1000, 100, 0x2000, 10, Ranges);
        var Cases = new[]
        {
            (Inputs: Inputs(0, 0), Accepted: true),
            (Inputs: Inputs(0x1011, 17), Accepted: true),
            (Inputs: Inputs(0x1064, 0), Accepted: true),
            (Inputs: Inputs(0x2004, 6), Accepted: true),
            (Inputs: Inputs(0x3007, 13), Accepted: true),
            (Inputs: Inputs(0x0000_0002_0000_0010, 16), Accepted: true),
            (Inputs: Inputs(0, 1), Accepted: false),
            (Inputs: Inputs(0x1000, 1, 1), Accepted: false),
            (Inputs: Inputs(0x1000, Bytecodeˉlimits.MAX_BYTE_DATA_BYTES + 1u), Accepted: false),
            (Inputs: Inputs(0x0FFF, 1), Accepted: false),
            (Inputs: Inputs(0x105F, 6), Accepted: false),
            (Inputs: Inputs(0x4000, 0), Accepted: false),
        };
        var Reference = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Case in Cases)
        {
            var Request = Nativeˉbyteˉresultˉadmissionˉbuilder.Buildˉrequest(Case.Inputs);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉbyteˉresultˉadmissionˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(
                Case.Accepted,
                Nativeˉbyteˉresultˉadmissionˉbuilder.Verifyˉresponse(
                    Case.Inputs,
                    Request.Length,
                    Executed));
            Equal(
                Case.Accepted,
                Nativeˉstage0ˉbyteˉresultˉadmissionˉoracle.Admit(Case.Inputs));
        }

        var Maximumˉranges = Enumerable.Range(0, Objectˉlimits.MAX_SYMBOLS)
            .Select(Index => new Nativeˉbyteˉresultˉrange(
                checked((ulong)(0x1_0000 + Index * 16)),
                16))
            .ToImmutableArray();
        var Maximum = new Nativeˉbyteˉresultˉadmissionˉinputs(
            new(Maximumˉranges[^1].Start + 15, 1, 0),
            0x1000,
            0,
            0,
            0,
            Maximumˉranges);
        Equal(true, Nativeˉbyteˉresultˉadmissionˉbuilder.Admit(Maximum));
        Equal(true, Nativeˉstage0ˉbyteˉresultˉadmissionˉoracle.Admit(Maximum));

        static ImmutableArray<byte> Replaceˉu32(
            ImmutableArray<byte> input,
            int offset,
            uint value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }

        static ImmutableArray<byte> Replaceˉu64(
            ImmutableArray<byte> input,
            int offset,
            ulong value)
        {
            var Result = input.ToArray();
            BinaryPrimitives.WriteUInt64LittleEndian(Result.AsSpan(offset), value);
            return Result.ToImmutableArray();
        }

        void Expectˉfailure(ImmutableArray<byte> request, uint status, uint failureˉoffset)
        {
            var Interpreted = Reference.Runˉmainˉbytes(request).Bytes;
            var Executed = Nativeˉbyteˉresultˉadmissionˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉbyteˉresultˉadmissionˉbuilder.Buildˉrequest(Inputs(0x1010, 1));
        Expectˉfailure(Valid[..63], 1, 63);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, checked((uint)Valid.Length - 1)), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 4_097), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 60, 1), 4, 12);
        Expectˉfailure(Replaceˉu64(Valid, 32, 0), 6, 32);
        Expectˉfailure(Replaceˉu32(Valid, 40, 134_217_729), 6, 32);
        Expectˉfailure(Replaceˉu64(Replaceˉu32(Valid, 56, 1), 48, 0), 7, 48);
        Expectˉfailure(Replaceˉu64(Valid, 64, 0), 8, 64);
        Expectˉfailure(Replaceˉu32(Valid, 72, 33_554_433), 8, 64);
        Expectˉfailure(Replaceˉu32(Valid, 76, 1), 8, 64);

        var Staticˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉresultˉadmissionˉstatic profile portable;
                data Value: bytes = [1, 2, 3];
                export fn Main() -> bytes { return Value; }
                """))).Fragment;
        Sequenceˉequal(
            new byte[] { 1, 2, 3 },
            X64ˉnativeˉexecutor.Executeˉbytes(Staticˉfragment));
        var Arenaˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉresultˉadmissionˉarena profile portable;
                data Left: bytes = [4];
                data Right: bytes = [5];
                export fn Main() -> bytes { return Bytesˉconcat(Left, Right); }
                """))).Fragment;
        Sequenceˉequal(
            new byte[] { 4, 5 },
            X64ˉnativeˉexecutor.Executeˉbytes(Arenaˉfragment));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-byte-result-admission-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Byte-Result-Admission.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Runtime", "Windvale", "Native-Byte-Result-Admission.wvproj"),
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
}
