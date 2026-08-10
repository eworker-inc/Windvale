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
    private const int NATIVE_ARGUMENT_TABLE_CORE_SIZE = 4_362;
    private const string NATIVE_ARGUMENT_TABLE_CORE_SHA256 =
        "08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75";

    private static void Windvaleˉnativeˉargumentˉtableˉruns()
    {
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Argument-Table-Core.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Argument-Table-Core.wv"));
        var Bridgeˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-Argument-Table-Bridge.wv",
            Readˉembeddedˉsource("Windvale.Seed.Tests.Native-Argument-Table-Bridge.wv"));
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale native argument-table core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_ARGUMENT_TABLE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_ARGUMENT_TABLE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(Bridgeˉinput, [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale native argument-table bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            Nativeˉargumentˉtableˉbuilder.CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            Nativeˉargumentˉtableˉbuilder.CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-Argument-Table-Bridge.wvb")));
        var Retainedˉartifact = Readˉembeddedˉnativeˉartifact(
            typeof(Nativeˉargumentˉtableˉbuilder),
            "Windvale.Native.Native-Argument-Table-Bridge.wvnf");
        Equal(
            Nativeˉargumentˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SIZE,
            Retainedˉartifact.Length);
        Equal(
            Nativeˉargumentˉtableˉbuilder.CONSUMER_ARTIFACT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Retainedˉartifact.AsSpan()));
        False(
            typeof(Nativeˉargumentˉtableˉbuilder).Assembly.GetManifestResourceNames()
                .Contains("Windvale.Native.Native-Argument-Table-Bridge.wvb", StringComparer.Ordinal),
            "The normal runtime embeds the native argument-table WVB.");

        var Bridge = Moduleˉcodec.Readˉandˉverify(Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Native = X64ˉnativeˉbackend.Compile(Bridge).Fragment;
        Sequenceˉequal(Retainedˉartifact, Nativeˉfragmentˉartifactˉcodec.Write(Native));
        True(Native.Requiredˉservices.IsEmpty, "The argument-table constructor requires a service.");
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Native));

        var Emptyˉpayload = ImmutableArray<byte>.Empty;
        var Emptyˉentry = ImmutableArray.Create(new Nativeˉargumentˉtableˉentry(101, 0, 0));
        var Mixedˉpayload = ImmutableArray.Create<byte>(65, 66, 67, 226, 130, 172, 68, 69);
        var Mixedˉentries = ImmutableArray.Create(
            new Nativeˉargumentˉtableˉentry(101, 0, 0),
            new Nativeˉargumentˉtableˉentry(202, 3, 0),
            new Nativeˉargumentˉtableˉentry(303, 3, 3),
            new Nativeˉargumentˉtableˉentry(404, 2, 6));
        var Maximumˉpayload = Enumerable.Range(0, Hostedˉresourceˉlimits.MAX_ARGUMENTS)
            .Select(Index => checked((byte)Index))
            .ToImmutableArray();
        var Maximumˉentries = Enumerable.Range(0, Hostedˉresourceˉlimits.MAX_ARGUMENTS)
            .Select(Index => new Nativeˉargumentˉtableˉentry(
                checked((ulong)(1_000 + Index)),
                1,
                checked((uint)Index)))
            .ToImmutableArray();
        var Cases = new[]
        {
            (Entries: Emptyˉentry, Payload: Emptyˉpayload),
            (Entries: Mixedˉentries, Payload: Mixedˉpayload),
            (Entries: Maximumˉentries, Payload: Maximumˉpayload),
        };
        var Reference = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        foreach (var Case in Cases)
        {
            var Request = Nativeˉargumentˉtableˉbuilder.Buildˉrequest(
                Case.Entries,
                Case.Payload);
            var Interpreted = Reference.Runˉmainˉbytes(Request).Bytes;
            var Executed = Nativeˉargumentˉtableˉbuilder.Buildˉwithˉwindvale(Request);
            Sequenceˉequal(Interpreted, Executed);
            Sequenceˉequal(
                Expectedˉnativeˉargumentˉtable(Case.Entries),
                Nativeˉargumentˉtableˉbuilder.Verifyˉresponse(
                    Case.Entries,
                    Request.Length,
                    Executed));
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
            var Executed = Nativeˉargumentˉtableˉbuilder.Buildˉwithˉwindvale(request);
            Sequenceˉequal(Interpreted, Executed);
            Equal(32, Executed.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[12..]));
            Equal(failureˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Executed.AsSpan()[16..]));
        }

        var Valid = Nativeˉargumentˉtableˉbuilder.Buildˉrequest(Mixedˉentries, Mixedˉpayload);
        Expectˉfailure(Valid[..^1], 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 0, 0), 2, 0);
        Expectˉfailure(Replaceˉu32(Valid, 4, 2), 3, 4);
        Expectˉfailure(Replaceˉu32(Valid, 8, checked((uint)Valid.Length - 1)), 1, 8);
        Expectˉfailure(Replaceˉu32(Valid, 12, 0), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 12, 68), 4, 12);
        Expectˉfailure(Replaceˉu32(Valid, 16, 24), 5, 16);
        Expectˉfailure(Replaceˉu64(Valid, 24, 0), 6, 24);
        Expectˉfailure(Replaceˉu32(Valid, 32, 4_097), 7, 32);
        Expectˉfailure(Replaceˉu32(Valid, 52, 1), 7, 48);

        const string Liveˉsource = """
            module Nativeˉargumentˉtableˉsmoke profile hosted;
            capability process.argument;
            export fn Main() -> i32 {
                let Value: bytes = Textˉtoˉutf8(process.argument(0u32));
                if Bytesˉlength(Value) == 8u32 { return 42; }
                return 1;
            }
            """;
        var Liveˉfragment = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Liveˉsource))).Fragment;
        Sequenceˉequal([Nativeˉservice.Processˉargument], Liveˉfragment.Requiredˉservices);
        var Resources = new Hostedˉresourceˉcontext(
            ["euro-€"],
            TextWriter.Null,
            TextWriter.Null);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Liveˉfragment,
                hostˉservices: new(
                    null,
                    [Capabilityˉcatalog.PROCESS_ARGUMENT],
                    Resources)));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-argument-table-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Native-Argument-Table.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(Repository, "Runtime", "Windvale", "Native-Argument-Table.wvproj"),
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

    private static ImmutableArray<byte> Expectedˉnativeˉargumentˉtable(
        ImmutableArray<Nativeˉargumentˉtableˉentry> entries)
    {
        var Result = new byte[checked(entries.Length * Nativeˉcontract.VALUE_SLOT_BYTES)];
        for (var Index = 0; Index < entries.Length; Index++)
        {
            var Descriptor = Result.AsSpan(Index * Nativeˉcontract.VALUE_SLOT_BYTES);
            BinaryPrimitives.WriteUInt64LittleEndian(Descriptor, entries[Index].Pointer);
            BinaryPrimitives.WriteUInt32LittleEndian(Descriptor[8..], entries[Index].Length);
        }
        return Result.ToImmutableArray();
    }
}
