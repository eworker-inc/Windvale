using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_SERVICE_CODE_BUILDER_SIZE = 4_135;
    private const string NATIVE_SERVICE_CODE_BUILDER_SHA256 =
        "adfb19e5a0668d06d40e0d6cadfadb34a729a0b0d1c12a11d03af722bd53cb06";
    private const int NATIVE_TEXT_CONCAT_CORE_SIZE = 10_253;
    private const string NATIVE_TEXT_CONCAT_CORE_SHA256 =
        "6b03161b9b3f112c6641474e321b2764522eb57a949d1b6bfc3d7b73ac91cc73";

    private static void Windvaleˉnativeˉtextˉconcatˉserviceˉruns()
    {
        var Builderˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Service-Code-Builder.wv");
        var Coreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Text-Concat-Service.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Text-Concat-Service-Bridge.wv");
        var Builderˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Service-Code-Builder.wv",
            Builderˉsource);
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Text-Concat-Service.wv",
            Coreˉsource);

        var Builderˉresult = Seedˉcompiler.Compileˉmodules(Builderˉinput, []);
        True(
            Builderˉresult.Success,
            "The Windvale native service-code builder did not compile: " +
                string.Join(" | ", Builderˉresult.Diagnostics));
        Equal(NATIVE_SERVICE_CODE_BUILDER_SIZE, Builderˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_SERVICE_CODE_BUILDER_SHA256,
            Moduleˉdigest.Calculateˉsha256(Builderˉresult.Moduleˉbytes.AsSpan()));

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, [Builderˉinput]);
        True(
            Coreˉresult.Success,
            "The Windvale text-concatenation service core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_TEXT_CONCAT_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_TEXT_CONCAT_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Text-Concat-Service-Bridge.wv",
                Bridgeˉsource),
            [Builderˉinput, Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale text-concatenation service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_CONCAT_CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_CONCAT_CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        Sequenceˉequal(
            Bridgeˉresult.Moduleˉbytes,
            File.ReadAllBytes(Path.Combine(
                Repository,
                "Runtime/Windvale.Native/Consumers/Native-X64-Text-Concat-Service-Bridge.wvb")));
        var Retainedˉleaf = Readˉembeddedˉnativeˉartifact(
            typeof(X64ˉnativeˉtextˉservices),
            "Windvale.Native.Native-X64-Text-Concat-Service.bin");
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-text-concat-service-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Text-Concat.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-X64-Text-Concat-Service.wvproj"),
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

        var Expected = X64ˉnativeˉtextˉservices.Build(Nativeˉservice.Textˉconcat);
        Equal(X64ˉnativeˉtextˉservices.TEXT_CONCAT_CANONICAL_SIZE, Expected.Length);
        Equal(
            X64ˉnativeˉtextˉservices.TEXT_CONCAT_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Expected.AsSpan()));
        Sequenceˉequal(Expected, Retainedˉleaf);
        X64ˉnativeˉtextˉservices.Verify(
            Nativeˉservice.Textˉconcat,
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
