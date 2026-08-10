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
    private const int NATIVE_HOSTED_VERIFIER_PLATFORM_BYTES = 40_063;
    private const string NATIVE_HOSTED_VERIFIER_PLATFORM_SHA256 =
        "b9900f7f3e49f7b99e135a77c3eec09cd3ef8d07a52633e9a70fca578925bb8e";

    private static void Nativeˉhostedˉverifierˉplatformˉprocessˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-verifier-platform-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Toolˉpath = Path.Combine(Directoryˉpath, "Verifier-Platform.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Platform-Tool.wvproj"),
                Toolˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Toolˉbytes = File.ReadAllBytes(Toolˉpath);
            Equal(NATIVE_HOSTED_VERIFIER_PLATFORM_BYTES, Toolˉbytes.Length);
            Equal(
                NATIVE_HOSTED_VERIFIER_PLATFORM_SHA256,
                Moduleˉdigest.Calculateˉsha256(Toolˉbytes));
            var Tool = Moduleˉcodec.Readˉandˉverify(Toolˉbytes);
            var Nativeˉtool = X64ˉnativeˉbackend.Compile(Tool).Fragment;

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
            var Consoleˉverifier =
                Loadˉconsoleˉapplicationˉverifierˉfixture(Repository);

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
                var Application = Platform == Nativeˉserviceˉplatform.Windows
                    ? Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Verifier.Module.Capabilities,
                        Bundle,
                        Nativeˉentry)
                    : Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Verifier.Module.Capabilities,
                        Bundle,
                        Nativeˉentry);
                byte[] Expected;
                if (Platform == Nativeˉserviceˉplatform.Windows)
                {
                    var Layout = Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
                        Bundle,
                        Nativeˉentry);
                    Expected = [
                        .. Application.AsSpan(0, Layout.Headerˉbytes),
                        .. Application.AsSpan(
                            checked((int)Layout.Importˉfileˉoffset),
                            checked((int)Windowsˉhostedˉverifierˉapplicationˉcontract
                                .IMPORT_FILE_BYTES)),
                        .. Application.AsSpan(
                            checked((int)Layout.Relocationˉfileˉoffset),
                            checked((int)Windowsˉhostedˉverifierˉapplicationˉcontract
                                .RELOCATION_BYTES)),
                    ];
                }
                else
                {
                    Expected = Application.AsSpan(0, 4096).ToArray();
                }

                var Runtimeˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Runtime.wvhr");
                var Responseˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Platform.wvhb");
                File.WriteAllBytes(Runtimeˉpath, Runtime.AsSpan());
                Equal(0, Run([Runtimeˉpath, Responseˉpath]));
                var Response = File.ReadAllBytes(Responseˉpath);
                Sequenceˉequal(Expected, Response.AsSpan(32).ToArray());

                var Consoleˉprofile = Hostedˉverifierˉapplicationˉprofile
                    .Consoleˉapplicationˉverifier;
                var Consoleˉbundle = X64ˉnativeˉserviceˉbundle
                    .Buildˉhostedˉconsoleˉapplicationˉverifier(
                        Consoleˉverifier.Fragment,
                        Platform);
                var Consoleˉruntime = Hostedˉverifierˉruntimeˉdata.Build(
                    Target,
                    Consoleˉverifier.Module.Module.Capabilities,
                    Consoleˉbundle,
                    Consoleˉverifier.Entry,
                    Consoleˉprofile);
                var Consoleˉruntimeˉlayout = Hostedˉverifierˉruntimeˉdata.Plan(
                    Target,
                    Consoleˉprofile);
                Assertˉconsoleˉverifierˉruntimeˉlayout(
                    Consoleˉruntimeˉlayout,
                    Target);
                var Consoleˉapplication = Platform ==
                    Nativeˉserviceˉplatform.Windows
                    ? Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Consoleˉverifier.Module.Module.Capabilities,
                        Consoleˉbundle,
                        Consoleˉverifier.Entry,
                        Consoleˉprofile)
                    : Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Consoleˉverifier.Module.Module.Capabilities,
                        Consoleˉbundle,
                        Consoleˉverifier.Entry,
                        Consoleˉprofile);
                byte[] Consoleˉexpected;
                if (Platform == Nativeˉserviceˉplatform.Windows)
                {
                    var Consoleˉlayout = Windowsˉhostedˉverifierˉapplicationˉcontract
                        .Plan(
                            Consoleˉbundle,
                            Consoleˉverifier.Entry,
                            Consoleˉprofile);
                    Equal(1_350, Consoleˉlayout.Startupˉbytes);
                    Equal(148_975_616u, Consoleˉlayout.Runtimeˉvirtualˉbytes);
                    Consoleˉexpected = [
                        .. Consoleˉapplication.AsSpan(0, Consoleˉlayout.Headerˉbytes),
                        .. Consoleˉapplication.AsSpan(
                            checked((int)Consoleˉlayout.Importˉfileˉoffset),
                            checked((int)Windowsˉhostedˉverifierˉapplicationˉcontract
                                .IMPORT_FILE_BYTES)),
                        .. Consoleˉapplication.AsSpan(
                            checked((int)Consoleˉlayout.Relocationˉfileˉoffset),
                            checked((int)Windowsˉhostedˉverifierˉapplicationˉcontract
                                .RELOCATION_BYTES)),
                    ];
                }
                else
                {
                    var Consoleˉlayout = Linuxˉhostedˉverifierˉapplicationˉcontract
                        .Plan(
                            Consoleˉbundle,
                            Consoleˉverifier.Entry,
                            Consoleˉprofile);
                    Equal(743, Consoleˉlayout.Startupˉbytes);
                    Equal(147_927_040u, Consoleˉlayout.Dataˉvirtualˉbytes);
                    Consoleˉexpected = Consoleˉapplication.AsSpan(0, 4096).ToArray();
                }
                var Consoleˉruntimeˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Console-Runtime.wvhr");
                var Consoleˉresponseˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Console-Platform.wvhb");
                File.WriteAllBytes(Consoleˉruntimeˉpath, Consoleˉruntime.AsSpan());
                Equal(0, Run([
                    "console-verifier",
                    Consoleˉruntimeˉpath,
                    Consoleˉresponseˉpath,
                ]));
                Sequenceˉequal(
                    Consoleˉexpected,
                    File.ReadAllBytes(Consoleˉresponseˉpath).AsSpan(32).ToArray());

                var Changedˉcapacity = Consoleˉruntime.ToArray();
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Changedˉcapacity.AsSpan(288),
                    1u);
                var Changedˉcapacityˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Changed-Capacity.wvhr");
                File.WriteAllBytes(Changedˉcapacityˉpath, Changedˉcapacity);
                byte[] Capacityˉsentinel = [0x57, 0x56, 0x48, 0x42];
                File.WriteAllBytes(Consoleˉresponseˉpath, Capacityˉsentinel);
                Equal(2, Run([
                    "console-verifier",
                    Changedˉcapacityˉpath,
                    Consoleˉresponseˉpath,
                ]));
                Sequenceˉequal(
                    Capacityˉsentinel,
                    File.ReadAllBytes(Consoleˉresponseˉpath));
            }

            var Windowsˉruntimeˉpath = Path.Combine(
                Directoryˉpath,
                $"{(uint)Nativeˉserviceˉplatform.Windows}-Runtime.wvhr");
            var Changedˉruntimeˉpath = Path.Combine(
                Directoryˉpath,
                "Changed-Runtime.wvhr");
            var Changedˉruntime = File.ReadAllBytes(Windowsˉruntimeˉpath);
            Changedˉruntime[480] ^= 1;
            File.WriteAllBytes(Changedˉruntimeˉpath, Changedˉruntime);
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.wvhb");
            byte[] Sentinel = [0x57, 0x56, 0x48, 0x42];
            File.WriteAllBytes(Rejectedˉpath, Sentinel);
            Equal(2, Run([Changedˉruntimeˉpath, Rejectedˉpath]));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Rejectedˉpath));
            Equal(64, Run([Windowsˉruntimeˉpath, Windowsˉruntimeˉpath]));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
