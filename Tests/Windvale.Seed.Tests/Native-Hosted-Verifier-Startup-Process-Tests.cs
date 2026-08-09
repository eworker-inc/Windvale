using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_VERIFIER_STARTUP_BYTES = 63_636;
    private const string NATIVE_HOSTED_VERIFIER_STARTUP_SHA256 =
        "435d464bef51cfa0c4154dbdaee24b34c8dd7fc6ef3ee8f39204edb4774358f0";

    private static void Nativeˉhostedˉverifierˉstartupˉprocessˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-verifier-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Toolˉpath = Path.Combine(Directoryˉpath, "Verifier-Startup.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Startup-Tool.wvproj"),
                Toolˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Toolˉbytes = File.ReadAllBytes(Toolˉpath);
            Equal(NATIVE_HOSTED_VERIFIER_STARTUP_BYTES, Toolˉbytes.Length);
            Equal(
                NATIVE_HOSTED_VERIFIER_STARTUP_SHA256,
                Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
            var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
            var Nativeˉtool = X64ˉnativeˉbackend.Compile(Tool).Fragment;
            Equal(
                new Nativeˉentryˉshape(
                    Nativeˉentryˉinputˉkind.None,
                    Nativeˉentryˉresultˉkind.Scalar),
                Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Nativeˉtool));

            var Verifierˉbytes = File.ReadAllBytes(Path.Combine(
                Repository,
                "Artifacts",
                "Native-Front-Door",
                "Wvb",
                "Compiler-Wvb-Verifier.wvb"));
            var Verifier = Moduleˉcodec.Readˉandˉverify(Verifierˉbytes);
            var Verifierˉfragment = X64ˉnativeˉbackend.Compile(Verifier).Fragment;
            var Nativeˉentry = Verifierˉfragment.Symbols.Single(Symbol =>
                Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main").Offset;

            int Run(ImmutableArray<string> arguments)
            {
                var Resources = new Hostedˉresourceˉcontext(
                    arguments,
                    TextWriter.Null,
                    TextWriter.Null);
                var Services = new Nativeˉhostˉservices(
                    Nativeˉoutputˉchannel.Processˉstandardˉoutput(),
                    Tool.Module.Capabilities.Select(Item => Item.Name),
                    Resources,
                    Nativeˉoutputˉchannel.Processˉdiagnosticˉoutput(),
                    Nativeˉfileˉinput.Hostˉfileˉsystem(),
                    Nativeˉfileˉoutput.Hostˉfileˉsystem());
                return X64ˉnativeˉexecutor.Executeˉi32(
                    Nativeˉtool,
                    maximumˉinstructions: 1_000_000_000,
                    hostˉservices: Services);
            }

            foreach (var Platform in Enum.GetValues<Nativeˉserviceˉplatform>())
            {
                var Target = Platform == Nativeˉserviceˉplatform.Windows
                    ? Consoleˉapplicationˉtarget.Windowsˉx64
                    : Consoleˉapplicationˉtarget.Linuxˉx64;
                var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                    Verifierˉfragment,
                    Platform);
                var Runtime = Hostedˉverifierˉruntimeˉdata.Build(
                    Target,
                    Verifier.Module.Capabilities,
                    Bundle,
                    Nativeˉentry);
                var Runtimeˉlayout = Hostedˉverifierˉruntimeˉdata.Plan(Target);
                var Objectˉname = Platform == Nativeˉserviceˉplatform.Windows
                    ? "Windows-X64-Hosted-Verifier.wvo"
                    : "Linux-X64-Hosted-Verifier.wvo";
                var Objectˉpath = Path.Combine(
                    Repository,
                    "Linker",
                    "Reference",
                    "Consumers",
                    Objectˉname);
                var Expected = Platform == Nativeˉserviceˉplatform.Windows
                    ? Windowsˉhostedˉverifierˉstartup.Build(
                        Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
                            Bundle,
                            Nativeˉentry).Textˉaddress,
                        Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
                            Bundle,
                            Nativeˉentry).Importˉaddress,
                        Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
                            Bundle,
                            Nativeˉentry).Runtimeˉaddress,
                        Runtimeˉlayout,
                        Bundle,
                        Nativeˉentry)
                    : Linuxˉhostedˉverifierˉstartup.Build(
                        Linuxˉhostedˉverifierˉapplicationˉcontract.Plan(
                            Bundle,
                            Nativeˉentry).Textˉaddress,
                        Linuxˉhostedˉverifierˉapplicationˉcontract.Plan(
                            Bundle,
                            Nativeˉentry).Dataˉaddress,
                        Runtimeˉlayout,
                        Bundle,
                        Nativeˉentry);
                var Runtimeˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Runtime.wvhr");
                var Responseˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Response.wvsd");
                File.WriteAllBytes(Runtimeˉpath, Runtime.AsSpan());

                Equal(0, Run([Runtimeˉpath, Objectˉpath, Responseˉpath]));
                var Response = File.ReadAllBytes(Responseˉpath);
                Equal(32 + Expected.Length, Response.Length);
                Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(Response.AsSpan(12)));
                Equal(
                    checked((uint)Expected.Length),
                    BinaryPrimitives.ReadUInt32LittleEndian(Response.AsSpan(20)));
                Sequenceˉequal(Expected, Response.AsSpan(32).ToArray());
            }

            var Windowsˉobjectˉpath = Path.Combine(
                Repository,
                "Linker",
                "Reference",
                "Consumers",
                "Windows-X64-Hosted-Verifier.wvo");
            var Changedˉobjectˉpath = Path.Combine(Directoryˉpath, "Changed.wvo");
            var Changedˉobject = File.ReadAllBytes(Windowsˉobjectˉpath);
            Changedˉobject[0] ^= 1;
            File.WriteAllBytes(Changedˉobjectˉpath, Changedˉobject);
            var Runtimeˉpathˉforˉrejection = Path.Combine(
                Directoryˉpath,
                $"{(uint)Nativeˉserviceˉplatform.Windows}-Runtime.wvhr");
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.wvsd");
            byte[] Sentinel = [0x57, 0x56, 0x53, 0x44];
            File.WriteAllBytes(Rejectedˉpath, Sentinel);
            Equal(2, Run([
                Runtimeˉpathˉforˉrejection,
                Changedˉobjectˉpath,
                Rejectedˉpath,
            ]));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Rejectedˉpath));
            Equal(64, Run([
                Runtimeˉpathˉforˉrejection,
                Windowsˉobjectˉpath,
                Runtimeˉpathˉforˉrejection,
            ]));
            Sequenceˉequal(
                Hostedˉverifierˉruntimeˉdata.Build(
                    Consoleˉapplicationˉtarget.Windowsˉx64,
                    Verifier.Module.Capabilities,
                    X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                        Verifierˉfragment,
                        Nativeˉserviceˉplatform.Windows),
                    Nativeˉentry),
                File.ReadAllBytes(Runtimeˉpathˉforˉrejection));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
