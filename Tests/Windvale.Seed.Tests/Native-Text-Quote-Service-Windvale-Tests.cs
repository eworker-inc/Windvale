using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_TEXT_QUOTE_CORE_SIZE = 1_471;
    private const string NATIVE_TEXT_QUOTE_CORE_SHA256 =
        "b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453";

    private static void Windvaleˉnativeˉtextˉquoteˉserviceˉruns()
    {
        var Coreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Text-Quote-Service.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Text-Quote-Service-Bridge.wv");
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Text-Quote-Service.wv",
            Coreˉsource);

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale text-quote service core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_TEXT_QUOTE_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_TEXT_QUOTE_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Text-Quote-Service-Bridge.wv",
                Bridgeˉsource),
            [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale text-quote service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_QUOTE_CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_QUOTE_CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-Text-Quote-Service-Bridge.wvb")));
        var Retainedˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉtextˉservices),
            "Windvale.Native.Native-X64-Text-Quote-Service.bin");
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-text-quote-service-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Text-Quote.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Runtime",
                    "Windvale",
                    "Native-X64-Text-Quote-Service.wvproj"),
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

        var Expected = X64ˉnativeˉtextˉservices.Build(Nativeˉservice.Textˉquote);
        Equal(X64ˉnativeˉtextˉservices.TEXT_QUOTE_CANONICAL_SIZE, Expected.Length);
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_QUOTE_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Expected.AsSpan()));
        Sequenceˉequal(Expected, Retainedˉleaf);
        X64ˉnativeˉtextˉservices.Verify(
            Nativeˉservice.Textˉquote,
            Expected.AsSpan());

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
