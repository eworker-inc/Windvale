using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_INTEGER_FORMAT_CORE_SIZE = 11_611;
    private const string NATIVE_INTEGER_FORMAT_CORE_SHA256 =
        "6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2";

    private static void Windvaleˉnativeˉintegerˉformatˉservicesˉrun()
    {
        var Coreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Integer-Format-Services.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Integer-Format-Services-Bridge.wv");
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Integer-Format-Services.wv",
                Coreˉsource),
            []);
        True(
            Coreˉresult.Success,
            "The Windvale integer-format service core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_INTEGER_FORMAT_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_INTEGER_FORMAT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Integer-Format-Services-Bridge.wv",
                Bridgeˉsource),
            [
                new(
                    "Runtime/Windvale/Native-X64-Integer-Format-Services.wv",
                    Coreˉsource),
            ]);
        True(
            Bridgeˉresult.Success,
            "The Windvale integer-format service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉtextˉservices.INTEGER_FORMAT_CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉtextˉservices.INTEGER_FORMAT_CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-Integer-Format-Services-Bridge.wvb")));
        var Retainedˉi32 = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉtextˉservices),
            "Windvale.Native.Native-X64-I32-Format-Service.bin");
        var Retainedˉu32 = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉtextˉservices),
            "Windvale.Native.Native-X64-U32-Format-Service.bin");
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-integer-format-services-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Integer-Format.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Runtime",
                    "Windvale",
                    "Native-X64-Integer-Format-Services.wvproj"),
                Nativeˉpath);
            Equal(0, Nativeˉbuild.Exitˉcode);
            Equal(string.Empty, Nativeˉbuild.Error);
            Sequenceˉequal(
                Bridgeˉresult.Moduleˉbytes,
                File.ReadAllBytes(Nativeˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }

        var I32 = X64ˉnativeˉtextˉservices.Build(Nativeˉservice.I32ˉformat);
        var U32 = X64ˉnativeˉtextˉservices.Build(Nativeˉservice.U32ˉformat);
        Sequenceˉequal(I32, Retainedˉi32);
        Sequenceˉequal(U32, Retainedˉu32);
        X64ˉnativeˉtextˉservices.Verify(Nativeˉservice.I32ˉformat, I32.AsSpan());
        X64ˉnativeˉtextˉservices.Verify(Nativeˉservice.U32ˉformat, U32.AsSpan());
        var Expectedˉbuilder = ImmutableArray.CreateBuilder<byte>(I32.Length + U32.Length);
        Expectedˉbuilder.AddRange(I32);
        Expectedˉbuilder.AddRange(U32);
        var Expected = Expectedˉbuilder.MoveToImmutable();

        var Bridge = Moduleˉcodec.Readˉandˉverify(
            Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Interpreted = new Referenceˉruntime(
            Bridge,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmainˉbytes();
        Sequenceˉequal(Expected, Interpreted.Bytes);

        var Native = X64ˉnativeˉbackend.Compile(Bridge);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            Expected,
            X64ˉnativeˉexecutor.Executeˉbytes(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions));
    }
}
