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
    private const int NATIVE_HOSTED_VERIFIER_STARTUP_BYTES = 79_401;
    private const string NATIVE_HOSTED_VERIFIER_STARTUP_SHA256 =
        "d669c6d74703980785f2d070d39ef2f5b537710d4ed74b174b7f0ffa41416341";

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
            var Windowsˉapplication =
                Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
                    Nativeˉtool,
                    Tool.Module.Capabilities,
                    Tool.Module.Name,
                    "Nativeˉhostedˉverifierˉstartupˉtool",
                    Hostedˉcompilerˉapplicationˉprofile.Compiler,
                    "hosted-verifier startup tool",
                    "WVW3033");
            var Linuxˉapplication =
                Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
                    Nativeˉtool,
                    Tool.Module.Capabilities,
                    Tool.Module.Name,
                    "Nativeˉhostedˉverifierˉstartupˉtool",
                    Hostedˉcompilerˉapplicationˉprofile.Compiler,
                    "hosted-verifier startup tool",
                    "WVL3033");
            True(Windowsˉapplication.Success,
                string.Join(" | ", Windowsˉapplication.Diagnostics));
            True(Linuxˉapplication.Success,
                string.Join(" | ", Linuxˉapplication.Diagnostics));
            var Candidateˉroot = Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Container-Toolset-Candidate");
            Sequenceˉequal(
                Toolˉbytes,
                File.ReadAllBytes(Path.Combine(
                    Candidateˉroot, "Wvb", "wvhostverifierstartup.wvb")));
            Sequenceˉequal(
                Windowsˉapplication.Imageˉbytes,
                File.ReadAllBytes(Path.Combine(
                    Candidateˉroot,
                    "windows-x64",
                    "wvhostverifierstartup.exe")));
            Sequenceˉequal(
                Linuxˉapplication.Imageˉbytes,
                File.ReadAllBytes(Path.Combine(
                    Candidateˉroot,
                    "linux-x64",
                    "wvhostverifierstartup.elf")));
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
            var Consoleˉverifier =
                Loadˉconsoleˉapplicationˉverifierˉfixture(Repository);
            var Windowsˉinspectorˉobjectˉpath = Path.Combine(
                Directoryˉpath,
                "Windows-X64-Hosted-Inspector.wvo");
            var Linuxˉinspectorˉobjectˉpath = Path.Combine(
                Directoryˉpath,
                "Linux-X64-Hosted-Inspector.wvo");
            File.WriteAllBytes(
                Windowsˉinspectorˉobjectˉpath,
                Assembleˉsuccess(WINDOWS_HOSTED_INSPECTOR_STARTUP_SOURCE));
            File.WriteAllBytes(
                Linuxˉinspectorˉobjectˉpath,
                Assembleˉsuccess(LINUX_HOSTED_INSPECTOR_STARTUP_SOURCE));

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
                var Consoleˉapplicationˉlayout = Platform ==
                    Nativeˉserviceˉplatform.Windows
                    ? (object)Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
                        Consoleˉbundle,
                        Consoleˉverifier.Entry,
                        Consoleˉprofile)
                    : Linuxˉhostedˉverifierˉapplicationˉcontract.Plan(
                        Consoleˉbundle,
                        Consoleˉverifier.Entry,
                        Consoleˉprofile);
                var Consoleˉexpected = Platform == Nativeˉserviceˉplatform.Windows
                    ? Windowsˉhostedˉinspectorˉstartup.Build(
                        ((Windowsˉhostedˉverifierˉapplicationˉlayout)
                            Consoleˉapplicationˉlayout).Textˉaddress,
                        ((Windowsˉhostedˉverifierˉapplicationˉlayout)
                            Consoleˉapplicationˉlayout).Importˉaddress,
                        ((Windowsˉhostedˉverifierˉapplicationˉlayout)
                            Consoleˉapplicationˉlayout).Runtimeˉaddress,
                        Consoleˉruntimeˉlayout,
                        Consoleˉbundle,
                        Consoleˉverifier.Entry,
                        Consoleˉprofile)
                    : Linuxˉhostedˉinspectorˉstartup.Build(
                        ((Linuxˉhostedˉverifierˉapplicationˉlayout)
                            Consoleˉapplicationˉlayout).Textˉaddress,
                        ((Linuxˉhostedˉverifierˉapplicationˉlayout)
                            Consoleˉapplicationˉlayout).Dataˉaddress,
                        Consoleˉruntimeˉlayout,
                        Consoleˉbundle,
                        Consoleˉverifier.Entry,
                        Consoleˉprofile);
                var Consoleˉruntimeˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Console-Runtime.wvhr");
                var Consoleˉresponseˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Console-Response.wvsd");
                var Consoleˉobjectˉpath = Platform ==
                    Nativeˉserviceˉplatform.Windows
                    ? Windowsˉinspectorˉobjectˉpath
                    : Linuxˉinspectorˉobjectˉpath;
                File.WriteAllBytes(Consoleˉruntimeˉpath, Consoleˉruntime.AsSpan());
                Equal(0, Run([
                    "console-verifier",
                    Consoleˉruntimeˉpath,
                    Consoleˉobjectˉpath,
                    Consoleˉresponseˉpath,
                ]));
                var Consoleˉresponse = File.ReadAllBytes(Consoleˉresponseˉpath);
                Equal(32 + Consoleˉexpected.Length, Consoleˉresponse.Length);
                Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(
                    Consoleˉresponse.AsSpan(12)));
                Equal(
                    checked((uint)Consoleˉexpected.Length),
                    BinaryPrimitives.ReadUInt32LittleEndian(
                        Consoleˉresponse.AsSpan(20)));
                Sequenceˉequal(
                    Consoleˉexpected,
                    Consoleˉresponse.AsSpan(32).ToArray());

                var Changedˉcapacity = Consoleˉruntime.ToArray();
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Changedˉcapacity.AsSpan(288),
                    1u);
                var Changedˉcapacityˉpath = Path.Combine(
                    Directoryˉpath,
                    $"{(uint)Platform}-Changed-Capacity.wvhr");
                File.WriteAllBytes(Changedˉcapacityˉpath, Changedˉcapacity);
                byte[] Capacityˉsentinel = [0x57, 0x56, 0x53, 0x44];
                File.WriteAllBytes(Consoleˉresponseˉpath, Capacityˉsentinel);
                Equal(2, Run([
                    "console-verifier",
                    Changedˉcapacityˉpath,
                    Consoleˉobjectˉpath,
                    Consoleˉresponseˉpath,
                ]));
                Sequenceˉequal(
                    Capacityˉsentinel,
                    File.ReadAllBytes(Consoleˉresponseˉpath));
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

    private static void Assertˉconsoleˉverifierˉruntimeˉlayout(
        Hostedˉverifierˉruntimeˉlayout layout,
        Consoleˉapplicationˉtarget target)
    {
        Equal(target, layout.Target);
        Equal(67u, layout.Maximumˉarguments);
        Equal(4_096u, layout.Maximumˉargumentˉbytes);
        Equal(4_096u, layout.Headerˉbytes);
        Equal(4_096u, layout.Argumentˉtableˉoffset);
        Equal(1_072u, layout.Argumentˉtableˉbytes);
        Equal(5_168u, layout.Argumentˉbytesˉoffset);
        Equal(65_536u, layout.Argumentˉbytes);
        Equal(70_704u, layout.Snapshotˉtableˉoffset);
        Equal(64u, layout.Snapshotˉtableˉbytes);
        Equal(2u, layout.Snapshotˉcapacity);
        Equal(73_728u, layout.Recordˉarenaˉoffset);
        Equal(2_170_880u, layout.Textˉarenaˉoffset);
        Equal(136_388_608u, layout.Nameˉarenaˉoffset);
        Equal(138_485_760u, layout.Dataˉarenaˉoffset);
        Equal(146_874_368u, layout.Fileˉinputˉscratchˉoffset);
        var Windows = target == Consoleˉapplicationˉtarget.Windowsˉx64;
        Equal(Windows ? 2_097_154u : 1_048_577u,
            layout.Fileˉinputˉscratchˉbytes);
        Equal(Windows ? 148_975_616u : 147_927_040u,
            layout.Fileˉoutputˉscratchˉoffset);
        Equal(0u, layout.Fileˉoutputˉscratchˉbytes);
        Equal(Windows ? 148_975_616u : 147_927_040u, layout.Virtualˉbytes);
    }
}
