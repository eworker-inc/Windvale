using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_UTF8_CORE_SIZE = 11_577;
    private const string NATIVE_UTF8_CORE_SHA256 =
        "adbd4843f3c0aaf003dc6118461278fc903fd2264be6e3b90835af49eb3cb2c7";
    private const int NATIVE_UTF8_BRIDGE_SIZE = 11_511;
    private const string NATIVE_UTF8_BRIDGE_SHA256 =
        "4d3c8d50d371147d687163c6d7ab761d32445719789f1f62f1f116f2bf268c4f";

    private static void Windvaleˉnativeˉutf8ˉserviceˉruns()
    {
        var Coreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Utf8-Service.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Utf8-Service-Bridge.wv");
        var Coreˉresult = Seedˉcompiler.Compileˉmodules(
            new("Runtime/Windvale/Native-X64-Utf8-Service.wv", Coreˉsource),
            []);
        True(
            Coreˉresult.Success,
            "The Windvale UTF-8 service core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_UTF8_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_UTF8_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Utf8-Service-Bridge.wv",
                Bridgeˉsource),
            [
                new(
                    "Runtime/Windvale/Native-X64-Utf8-Service.wv",
                    Coreˉsource),
            ]);
        True(
            Bridgeˉresult.Success,
            "The Windvale UTF-8 service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(NATIVE_UTF8_BRIDGE_SIZE, Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_UTF8_BRIDGE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service-Bridge.wvb")));
        var Retainedˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉutf8ˉservice),
            "Windvale.Native.Native-X64-Utf8-Service.bin");
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-utf8-service-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Utf8.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-X64-Utf8-Service.wvproj"),
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

        var Bridge = Moduleˉcodec.Readˉandˉverify(
            Bridgeˉresult.Moduleˉbytes.AsSpan());
        var Expected = X64ˉnativeˉutf8ˉservice.Build();
        Equal(X64ˉnativeˉutf8ˉservice.CANONICAL_SIZE, Expected.Length);
        Equal(
            X64ˉnativeˉutf8ˉservice.CANONICAL_SHA256,
            Convert.ToHexString(SHA256.HashData(Expected.AsSpan()))
                .ToLowerInvariant());
        Sequenceˉequal(Expected, Retainedˉleaf);

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

        var Corrupted = Expected.ToArray();
        Corrupted[0] ^= 0x01;
        Throwsˉinvalidˉoperation(
            "Native UTF-8 service identity",
            () => X64ˉnativeˉutf8ˉservice.Verify(Corrupted));
    }
}
