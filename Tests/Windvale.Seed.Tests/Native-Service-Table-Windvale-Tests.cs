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
    private const int NATIVE_SERVICE_TABLE_CORE_SIZE = 3_065;
    private const string NATIVE_SERVICE_TABLE_CORE_SHA256 =
        "ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26";

    private static void Windvaleˉnativeˉserviceˉtableˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Service-Table-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Service-Table-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Service-Table-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Service-Table-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native service-table core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_SERVICE_TABLE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_SERVICE_TABLE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native service-table bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(Nativeˉserviceˉtableˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉserviceˉtableˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Service-Table-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉserviceˉtableˉbuilder),
            "Windvale.Native.Native-Service-Table-Bridge.wvnf");
        Equal(
            Nativeˉserviceˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉserviceˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉserviceˉtableˉbuilder).Assembly.GetManifestResourceNames()
                .Contains("Windvale.Native.Native-Service-Table-Bridge.wvb", StringComparer.Ordinal),
            "The normal runtime embeds the native service-table WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The service-table constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var One = ImmutableArray.Create<ulong>(101, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        var Sparse = ImmutableArray.Create<ulong>(
            101, 0, 0, 0, 0, 606, 0, 0, 0, 0, 0, 1_212);
        var Complete = Enumerable.Range(1, 12)
            .Select(Index => checked((ulong)(Index * 101)))
            .ToImmutableArray();
        var Cases = new[]
        {
            (Mask: 1u, Targets: One),
            (Mask: 2_081u, Targets: Sparse),
            (Mask: 4_095u, Targets: Complete),
        };
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Case in Cases)
        {
            var Request = Nativeˉserviceˉtableˉbuilder.Buildˉrequest(Case.Mask, Case.Targets);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉserviceˉtableˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            var Table = Nativeˉserviceˉtableˉbuilder.Verifyˉresponse(
                Case.Mask,
                Case.Targets,
                Request.Length,
                Executed);
            Sequenceˉequal(Expectedˉnativeˉserviceˉtable(Case.Targets), Table);
        }

        var Required = ImmutableArray.Create(
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Fileˉwriteˉbytes);
        var Offsets = new Dictionary<Nativeˉservice, int>
        {
            [Nativeˉservice.Consoleˉwriteˉline] = 16,
            [Nativeˉservice.Diagnosticˉwriteˉline] = 32,
            [Nativeˉservice.Fileˉwriteˉbytes] = 48,
        };
        var Projected = ImmutableArray.Create<ulong>(
            1_016, 0, 0, 0, 0, 1_032, 0, 0, 0, 0, 0, 1_048);
        Sequenceˉequal(
            Expectedˉnativeˉserviceˉtable(Projected),
            Nativeˉserviceˉtableˉbuilder.Build(Required, 1_000, Offsets));

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
            var Executed = Nativeˉserviceˉtableˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉserviceˉtableˉbuilder.Buildˉrequest(1, One);
        Expectˉfailure(Valid[..^1], 1, 111);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, 111), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 12, 4_096), 4, 12);
        Expectˉfailure(Replaceˉu64(Valid, 16, 0), 5, 16);
        Expectˉfailure(Replaceˉu64(Valid, 24, 202), 5, 24);

        const string Liveˉsource = """
            module Nativeˉserviceˉtableˉsmoke profile portable;
            data Valid: bytes = [65, 226, 130, 172];
            export fn Main() -> i32 {
                if Textˉutf8ˉisˉvalid(Valid) { return 42; }
                return 1;
            }
            """;
        var Liveˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Liveˉsource))).Fragment;
        Sequenceˉequal(
            [Nativeˉservice.Textˉutf8ˉisˉvalid],
            Liveˉfragment.Requiredˉservices);
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Liveˉfragment));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-service-table-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Service-Table.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Windvale-Native-Service-Table.wvproj"),
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

    private static ImmutableArray<byte> Expectedˉnativeˉserviceˉtable(
        ImmutableArray<ulong> targets)
    {
        var Result = new byte[Nativeˉserviceˉtableˉcontract.SIZE];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result, Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(4), Nativeˉserviceˉtableˉcontract.SIZE);
        for (var Index = 0; Index < targets.Length; Index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(
                Result.AsSpan(8 + Index * sizeof(ulong)),
                targets[Index]);
        }
        return Result.ToImmutableArray();
    }
}
