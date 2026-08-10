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
    private const int NATIVE_EXECUTION_CONTEXT_CORE_SIZE = 5_530;
    private const string NATIVE_EXECUTION_CONTEXT_CORE_SHA256 =
        "dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b";

    private static void Windvaleˉnativeˉexecutionˉcontextˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Execution-Context-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Execution-Context-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Execution-Context-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Execution-Context-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native execution-context core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_EXECUTION_CONTEXT_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_EXECUTION_CONTEXT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native execution-context bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉexecutionˉcontextˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉexecutionˉcontextˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Execution-Context-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉexecutionˉcontextˉbuilder),
            "Windvale.Native.Native-Execution-Context-Bridge.wvnf");
        Equal(
            Nativeˉexecutionˉcontextˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉexecutionˉcontextˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉexecutionˉcontextˉbuilder).Assembly.GetManifestResourceNames()
                .Contains(
                    "Windvale.Native.Native-Execution-Context-Bridge.wvb",
                    StringComparer.Ordinal),
            "The normal runtime embeds the native execution-context WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The context constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Minimal = Nativeˉexecutionˉcontextˉsample(allˉoptional: false);
        var Complete = Nativeˉexecutionˉcontextˉsample(allˉoptional: true);
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Inputs in new[] { Minimal, Complete })
        {
            var Request = Nativeˉexecutionˉcontextˉbuilder.Buildˉrequest(Inputs);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉexecutionˉcontextˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            var Context = Nativeˉexecutionˉcontextˉbuilder.Verifyˉresponse(
                Inputs,
                Request.Length,
                Executed);
            Sequenceˉequal(Nativeˉstage0ˉexecutionˉcontextˉoracle.Build(Inputs), Context);
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
            var Executed = Nativeˉexecutionˉcontextˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉexecutionˉcontextˉbuilder.Buildˉrequest(Minimal);
        Expectˉfailure(Valid[..^1], 1, 119);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 119), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 32), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 76, 1), 4, 76);
        Expectˉfailure(Replaceˉu64(Valid, 16, 0), 5, 16);
        Expectˉfailure(Replaceˉu64(Valid, 24, 0), 5, 24);
        Expectˉfailure(Replaceˉu64(Valid, 32, 101), 6, 32);
        Expectˉfailure(Replaceˉu64(Valid, 40, 0), 7, 40);
        Expectˉfailure(Replaceˉu64(Valid, 56, 0), 8, 56);
        Expectˉfailure(Replaceˉu64(Valid, 80, 101), 9, 80);
        Expectˉfailure(Replaceˉu64(Valid, 96, 101), 10, 96);

        using (var Context = new Nativeˉexecutionˉcontext(Complete, serviceˉfreeˉbootstrap: false))
        {
            Equal(
                new Nativeˉexecutionˉcontextˉcompletion(
                    0,
                    0,
                    Nativeˉserviceˉfailureˉdetail.None),
                Context.Readˉverifiedˉcompletion());
            Marshal.WriteInt32(
                Context.Address,
                Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
                17);
            Marshal.WriteInt32(
                Context.Address,
                Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
                29);
            Marshal.WriteInt32(
                Context.Address,
                Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
                (int)Nativeˉserviceˉfailureˉdetail.Bytesˉu16ˉoutˉofˉrange);
            Equal(
                new Nativeˉexecutionˉcontextˉcompletion(
                    17,
                    29,
                    Nativeˉserviceˉfailureˉdetail.Bytesˉu16ˉoutˉofˉrange),
                Context.Readˉverifiedˉcompletion());
        }

        using (var Context = new Nativeˉexecutionˉcontext(Minimal, serviceˉfreeˉbootstrap: false))
        {
            Marshal.WriteInt32(
                Context.Address,
                Nativeˉexecutionˉcontextˉcontract.RESERVED_OFFSET,
                1);
            Throwsˉinvalidˉoperation(
                "The native execution context changed outside its bounded mutable fields.",
                () => _ = Context.Readˉverifiedˉcompletion());
        }

        const string Liveˉsource = """
            module Nativeˉexecutionˉcontextˉsmoke profile portable;
            export fn Main() -> i32 {
                let Value: text = Textˉconcat("a", "b");
                return 42;
            }
            """;
        var Liveˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Liveˉsource))).Fragment;
        Sequenceˉequal([Nativeˉservice.Textˉconcat], Liveˉfragment.Requiredˉservices);
        var Measurement = X64ˉnativeˉexecutor.Measureˉi32(Liveˉfragment);
        Equal(42, Measurement.Scalar);
        Equal(0u, Measurement.Recordˉarenaˉused);
        Equal(2u, Measurement.Textˉarenaˉused);

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-execution-context-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Execution-Context.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Runtime", "Windvale", "Native-Execution-Context.wvproj"),
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

    private static Nativeˉexecutionˉcontextˉinputs Nativeˉexecutionˉcontextˉsample(
        bool allˉoptional) =>
        new(
            1_000,
            17,
            allˉoptional ? 0x1111_2222_3333_4444UL : 0,
            0x5555_6666_7777_8888UL,
            Nativeˉcontract.MAXIMUM_RECORD_ARENA_BYTES,
            0x9999_AAAA_BBBB_CCCCUL,
            Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES,
            allˉoptional ? 0x1111_1111_1111_1111UL : 0,
            allˉoptional ? (uint)Hostedˉresourceˉlimits.MAX_ARGUMENTS : 0,
            allˉoptional ? 0x2222_2222_2222_2222UL : 0,
            allˉoptional ? 0x3333_3333_3333_3333UL : 0,
            allˉoptional ? 0x4444_4444_4444_4444UL : 0);
}
