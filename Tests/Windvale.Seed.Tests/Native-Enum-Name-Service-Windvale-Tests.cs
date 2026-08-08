using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_ENUM_NAME_CORE_SIZE = 625;
    private const string NATIVE_ENUM_NAME_CORE_SHA256 =
        "b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948";

    private static void Windvaleˉnativeˉenumˉnameˉserviceˉruns()
    {
        var Coreˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Enum-Name-Service.wv");
        var Bridgeˉsource = Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-X64-Enum-Name-Service-Bridge.wv");
        var Coreˉinput = new Sourceˉmoduleˉinput(
            "Runtime/Windvale/Native-X64-Enum-Name-Service.wv",
            Coreˉsource);

        var Coreˉresult = Seedˉcompiler.Compileˉmodules(Coreˉinput, []);
        True(
            Coreˉresult.Success,
            "The Windvale enum-name service core did not compile: " +
                string.Join(" | ", Coreˉresult.Diagnostics));
        Equal(NATIVE_ENUM_NAME_CORE_SIZE, Coreˉresult.Moduleˉbytes.Length);
        Equal(
            NATIVE_ENUM_NAME_CORE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Coreˉresult.Moduleˉbytes.AsSpan()));

        var Bridgeˉresult = Seedˉcompiler.Compileˉmodules(
            new(
                "Runtime/Windvale/Native-X64-Enum-Name-Service-Bridge.wv",
                Bridgeˉsource),
            [Coreˉinput]);
        True(
            Bridgeˉresult.Success,
            "The Windvale enum-name service bridge did not compile: " +
                string.Join(" | ", Bridgeˉresult.Diagnostics));
        Equal(
            X64ˉnativeˉtextˉservices.ENUM_NAME_CONSUMER_CANONICAL_SIZE,
            Bridgeˉresult.Moduleˉbytes.Length);
        Equal(
            X64ˉnativeˉtextˉservices.ENUM_NAME_CONSUMER_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bridgeˉresult.Moduleˉbytes.AsSpan()));

        using (var Stream = typeof(X64ˉnativeˉtextˉservices).Assembly
            .GetManifestResourceStream(
                "Windvale.Native.Native-X64-Enum-Name-Service-Bridge.wvb") ??
            throw new InvalidOperationException(
                "The retained Windvale enum-name service bridge was not embedded."))
        {
            var Retained = new byte[checked((int)Stream.Length)];
            Stream.ReadExactly(Retained);
            Sequenceˉequal(Bridgeˉresult.Moduleˉbytes, Retained);
        }

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-enum-name-service-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Nativeˉpath = Path.Combine(Directoryˉpath, "Enum-Name.wvb");
            var Nativeˉbuild = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-X64-Enum-Name-Service.wvproj"),
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

        var Types = ImmutableArray.Create<Nominalˉtypeˉdeclaration>(
            new Recordˉtypeˉdeclaration(
                "Nativeˉrecord",
                ImmutableArray<Recordˉfieldˉdeclaration>.Empty),
            new Enumˉtypeˉdeclaration(
                "Nativeˉstate",
                ImmutableArray.Create(
                    new Enumˉmemberˉdeclaration("Stopped", -1),
                    new Enumˉmemberˉdeclaration("Running", 2))));
        var Bundle = X64ˉnativeˉtextˉservices.Build(
            Nativeˉservice.Enumˉname,
            Types);
        X64ˉnativeˉtextˉservices.Verify(
            Nativeˉservice.Enumˉname,
            Bundle.AsSpan(),
            Types);
        var Expected = Bundle.AsSpan(
            0,
            X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE).ToImmutableArray();
        Equal(X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SIZE, Expected.Length);
        Equal(
            X64ˉnativeˉtextˉservices.ENUM_NAME_CANONICAL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Expected.AsSpan()));

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
