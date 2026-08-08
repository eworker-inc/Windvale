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
    private const int NATIVE_ENTRY_BRIDGE_CORE_SIZE = 3_385;
    private const string NATIVE_ENTRY_BRIDGE_CORE_SHA256 =
        "8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae";

    private static void Windvaleˉnativeˉentryˉbridgeˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Entry-Bridge-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Entry-Bridge-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Entry-Bridge-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Entry-Bridge-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native entry-bridge core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_ENTRY_BRIDGE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_ENTRY_BRIDGE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native entry bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉentryˉbridgeˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉentryˉbridgeˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Entry-Bridge-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉentryˉbridgeˉbuilder),
            "Windvale.Native.Native-Entry-Bridge-Bridge.wvnf");
        Equal(
            Nativeˉentryˉbridgeˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉentryˉbridgeˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉentryˉbridgeˉbuilder).Assembly.GetManifestResourceNames()
                .Contains("Windvale.Native.Native-Entry-Bridge-Bridge.wvb", StringComparer.Ordinal),
            "The normal runtime embeds the native entry-bridge WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The entry-bridge constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Cases = new[]
        {
            new Nativeˉentryˉbridgeˉinputs(Nativeˉentryˉinputˉkind.None, 0, 0),
            new Nativeˉentryˉbridgeˉinputs(Nativeˉentryˉinputˉkind.Bytes, 101, 0),
            new Nativeˉentryˉbridgeˉinputs(
                Nativeˉentryˉinputˉkind.Bytes,
                0x1111_2222_3333_4444,
                Bytecodeˉlimits.MAX_BYTE_DATA_BYTES),
        };
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Inputs in Cases)
        {
            var Request = Nativeˉentryˉbridgeˉbuilder.Buildˉrequest(Inputs);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉentryˉbridgeˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            var Result = Nativeˉentryˉbridgeˉbuilder.Verifyˉresponse(
                Inputs,
                Request.Length,
                Executed);
            Sequenceˉequal(Nativeˉstage0ˉentryˉbridgeˉoracle.Build(Inputs), Result);
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
            var Executed = Nativeˉentryˉbridgeˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉentryˉbridgeˉbuilder.Buildˉrequest(Cases[1]);
        Expectˉfailure(Valid[..^1], 1, 31);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 31), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 2), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 28, 1), 4, 12);
        Expectˉfailure(Replaceˉu64(Valid, 16, 0), 5, 16);
        Expectˉfailure(
            Replaceˉu32(Valid, 24, Bytecodeˉlimits.MAX_BYTE_DATA_BYTES + 1u),
            6,
            24);

        var Liveˉinputs = Cases[1];
        using (var Owner = new Nativeˉentryˉbridge(Liveˉinputs, serviceˉfreeˉbootstrap: false))
        {
            Marshal.WriteInt64(Owner.Address, 0, 0x1234_5678);
            Marshal.WriteInt32(Owner.Address, 8, 17);
            Equal(
                new Nativeˉentryˉresultˉdescriptor(0x1234_5678, 17, 0),
                Owner.Readˉverifiedˉresultˉdescriptor());
            Marshal.WriteInt32(Owner.Address, Nativeˉcontract.VALUE_SLOT_BYTES, 102);
            Throwsˉinvalidˉoperation(
                "The native entry changed its immutable input descriptor.",
                () => _ = Owner.Readˉverifiedˉresultˉdescriptor());
        }

        var Staticˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉentryˉbridgeˉstatic profile portable;
                data Value: bytes = [1, 2, 3];
                export fn Main() -> bytes { return Value; }
                """))).Fragment;
        Sequenceˉequal(
            new byte[] { 1, 2, 3 },
            X64ˉnativeˉexecutor.Executeˉbytes(Staticˉfragment));

        var Inputˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉentryˉbridgeˉinput profile portable;
                export fn Main(Input: bytes) -> bytes { return Input; }
                """))).Fragment;
        var Inputˉbytes = ImmutableArray.Create<byte>(4, 5, 6, 7);
        Sequenceˉequal(Inputˉbytes, X64ˉnativeˉexecutor.Executeˉbytes(Inputˉfragment, Inputˉbytes));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-entry-bridge-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Entry-Bridge.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-Entry-Bridge.wvproj"),
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
