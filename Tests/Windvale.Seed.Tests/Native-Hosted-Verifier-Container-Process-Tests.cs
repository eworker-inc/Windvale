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
    private const int NATIVE_HOSTED_VERIFIER_CONTAINER_BYTES = 87_965;
    private const string NATIVE_HOSTED_VERIFIER_CONTAINER_SHA256 =
        "ef300a56e70c7fbe9b47c623bc0ed408d151e1aee967cba214ef3c8601617b0d";

    private static void Nativeˉhostedˉverifierˉcontainerˉprocessˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-verifier-container-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Toolˉpath = Path.Combine(Directoryˉpath, "Verifier-Container.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Verifier-Container-Tool.wvproj"),
                Toolˉpath);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            var Toolˉbytes = File.ReadAllBytes(Toolˉpath);
            Equal(NATIVE_HOSTED_VERIFIER_CONTAINER_BYTES, Toolˉbytes.Length);
            Equal(
                NATIVE_HOSTED_VERIFIER_CONTAINER_SHA256,
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
                    maximumˉinstructions: 48_000_000_000,
                    hostˉservices: Services);
            }

            var Paths = new List<string>();
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
                    Nativeˉentry).ToArray();
                var Expected = Platform == Nativeˉserviceˉplatform.Windows
                    ? Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Verifier.Module.Capabilities,
                        Bundle,
                        Nativeˉentry)
                    : Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Verifier.Module.Capabilities,
                        Bundle,
                        Nativeˉentry);
                var Platformˉresponse = Platformˉresponseˉfrom(
                    Platform,
                    Bundle,
                    Nativeˉentry,
                    Expected);
                var Startupˉresponse = Startupˉresponseˉfrom(
                    Platform,
                    Bundle,
                    Nativeˉentry,
                    Expected);
                var Bundleˉresponse = Responseˉwithˉpayload(
                    1230198359,
                    2,
                    [1u, 24u, checked((uint)Bundle.Imageˉbytes.Length), 0u,
                        checked((uint)Bundle.Imageˉbytes.Length), 6u],
                    Bundle.Imageˉbytes.AsSpan());

                var Prefix = Path.Combine(Directoryˉpath, $"{(uint)Platform}-");
                var Runtimeˉpath = Prefix + "Runtime.wvhr";
                var Platformˉpath = Prefix + "Platform.wvhb";
                var Startupˉpath = Prefix + "Startup.wvsd";
                var Bundleˉpath = Prefix + "Bundle.wvsi";
                var Outputˉpath = Prefix + "Application";
                File.WriteAllBytes(Runtimeˉpath, Runtime);
                File.WriteAllBytes(Platformˉpath, Platformˉresponse);
                File.WriteAllBytes(Startupˉpath, Startupˉresponse);
                File.WriteAllBytes(Bundleˉpath, Bundleˉresponse);
                Equal(0, Run([
                    Runtimeˉpath, Platformˉpath, Startupˉpath,
                    Bundleˉpath, Outputˉpath,
                ]));
                Sequenceˉequal(Expected, File.ReadAllBytes(Outputˉpath));

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
                    Consoleˉprofile).ToArray();
                var Consoleˉruntimeˉlayout = Hostedˉverifierˉruntimeˉdata.Plan(
                    Target,
                    Consoleˉprofile);
                Assertˉconsoleˉverifierˉruntimeˉlayout(
                    Consoleˉruntimeˉlayout,
                    Target);
                var Consoleˉexpected = Platform == Nativeˉserviceˉplatform.Windows
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
                var Consoleˉplatformˉresponse = Platformˉresponseˉfrom(
                    Platform,
                    Consoleˉbundle,
                    Consoleˉverifier.Entry,
                    Consoleˉexpected,
                    Consoleˉprofile);
                var Consoleˉstartupˉresponse = Startupˉresponseˉfrom(
                    Platform,
                    Consoleˉbundle,
                    Consoleˉverifier.Entry,
                    Consoleˉexpected,
                    Consoleˉprofile);
                var Consoleˉbundleˉresponse = Responseˉwithˉpayload(
                    1230198359,
                    2,
                    [1u, 24u, checked((uint)Consoleˉbundle.Imageˉbytes.Length), 0u,
                        checked((uint)Consoleˉbundle.Imageˉbytes.Length), 11u],
                    Consoleˉbundle.Imageˉbytes.AsSpan());
                var Consoleˉruntimeˉpath = Prefix + "Console-Runtime.wvhr";
                var Consoleˉplatformˉpath = Prefix + "Console-Platform.wvhb";
                var Consoleˉstartupˉpath = Prefix + "Console-Startup.wvsd";
                var Consoleˉbundleˉpath = Prefix + "Console-Bundle.wvsi";
                var Consoleˉoutputˉpath = Prefix + "Console-Application";
                File.WriteAllBytes(Consoleˉruntimeˉpath, Consoleˉruntime);
                File.WriteAllBytes(Consoleˉplatformˉpath, Consoleˉplatformˉresponse);
                File.WriteAllBytes(Consoleˉstartupˉpath, Consoleˉstartupˉresponse);
                File.WriteAllBytes(Consoleˉbundleˉpath, Consoleˉbundleˉresponse);
                Equal(0, Run([
                    "console-verifier",
                    Consoleˉruntimeˉpath,
                    Consoleˉplatformˉpath,
                    Consoleˉstartupˉpath,
                    Consoleˉbundleˉpath,
                    Consoleˉoutputˉpath,
                ]));
                Sequenceˉequal(
                    Consoleˉexpected,
                    File.ReadAllBytes(Consoleˉoutputˉpath));

                var Changedˉcapacity = Consoleˉruntime.ToArray();
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Changedˉcapacity.AsSpan(288),
                    1u);
                var Changedˉcapacityˉpath = Prefix + "Changed-Capacity.wvhr";
                File.WriteAllBytes(Changedˉcapacityˉpath, Changedˉcapacity);
                byte[] Capacityˉsentinel = [0x57, 0x56, 0x43, 0x41];
                File.WriteAllBytes(Consoleˉoutputˉpath, Capacityˉsentinel);
                Equal(2, Run([
                    "console-verifier",
                    Changedˉcapacityˉpath,
                    Consoleˉplatformˉpath,
                    Consoleˉstartupˉpath,
                    Consoleˉbundleˉpath,
                    Consoleˉoutputˉpath,
                ]));
                Sequenceˉequal(
                    Capacityˉsentinel,
                    File.ReadAllBytes(Consoleˉoutputˉpath));

                if ((OperatingSystem.IsWindows() &&
                        Platform == Nativeˉserviceˉplatform.Windows) ||
                    (OperatingSystem.IsLinux() &&
                        Platform == Nativeˉserviceˉplatform.Linux))
                {
                    var Extension = OperatingSystem.IsWindows() ? ".exe" : ".elf";
                    var Packagedˉtoolˉpath = Path.Combine(
                        Repository,
                        "Artifacts",
                        "Native-Hosted-Container-Toolset-Candidate",
                        OperatingSystem.IsWindows() ? "windows-x64" : "linux-x64",
                        "wvhostverifiercompose" + Extension);
                    var Directˉoutputˉpath = Prefix + "Direct" + Extension;
                    var Arguments = new[]
                    {
                        Runtimeˉpath,
                        Platformˉpath,
                        Startupˉpath,
                        Bundleˉpath,
                        Directˉoutputˉpath,
                    };
                    if (OperatingSystem.IsWindows())
                    {
                        var Loadedˉmodules = new HashSet<string>(
                            StringComparer.OrdinalIgnoreCase);
                        Equal(0, Executeˉwindowsˉapplication(
                            File.ReadAllBytes(Packagedˉtoolˉpath).ToImmutableArray(),
                            $"verifier container status=Valid bytes={Expected.Length}\n",
                            Arguments,
                            timeoutˉmilliseconds: 60_000,
                            loadedˉmodules: Loadedˉmodules));
                        Equal(0, Loadedˉmodules.Count(Name =>
                            Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                        Sequenceˉequal(Expected, File.ReadAllBytes(Directˉoutputˉpath));
                        Loadedˉmodules.Clear();
                        Equal(0, Executeˉwindowsˉapplication(
                            File.ReadAllBytes(Directˉoutputˉpath).ToImmutableArray(),
                            "wvb status=Valid profile=compiler-aligned\n",
                            [Path.Combine(
                                Repository,
                                "Artifacts",
                                "Native-Front-Door",
                                "Wvb",
                                "Compiler-Wvb-Verifier.wvb")],
                            timeoutˉmilliseconds: 60_000,
                            loadedˉmodules: Loadedˉmodules));
                        Equal(0, Loadedˉmodules.Count(Name =>
                            Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                    }
                    else
                    {
                        var Loadedˉmappings = new HashSet<string>(StringComparer.Ordinal);
                        Equal(0, Executeˉlinuxˉapplication(
                            File.ReadAllBytes(Packagedˉtoolˉpath).ToImmutableArray(),
                            $"verifier container status=Valid bytes={Expected.Length}\n",
                            Arguments,
                            timeoutˉmilliseconds: 60_000,
                            loadedˉmappings: Loadedˉmappings));
                        Equal(0, Loadedˉmappings.Count(Name =>
                            Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                        Sequenceˉequal(Expected, File.ReadAllBytes(Directˉoutputˉpath));
                        Loadedˉmappings.Clear();
                        Equal(0, Executeˉlinuxˉapplication(
                            File.ReadAllBytes(Directˉoutputˉpath).ToImmutableArray(),
                            "wvb status=Valid profile=compiler-aligned\n",
                            [Path.Combine(
                                Repository,
                                "Artifacts",
                                "Native-Front-Door",
                                "Wvb",
                                "Compiler-Wvb-Verifier.wvb")],
                            timeoutˉmilliseconds: 60_000,
                            loadedˉmappings: Loadedˉmappings));
                        Equal(0, Loadedˉmappings.Count(Name =>
                            Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                            Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
                    }
                }
                Paths.AddRange([
                    Runtimeˉpath, Platformˉpath, Startupˉpath,
                    Bundleˉpath, Outputˉpath,
                ]);
            }

            var Rejectedˉbundle = File.ReadAllBytes(Paths[3]);
            Rejectedˉbundle[40] ^= 1;
            var Rejectedˉpath = Path.Combine(Directoryˉpath, "Rejected.wvsi");
            var Sentinelˉpath = Path.Combine(Directoryˉpath, "Sentinel.exe");
            byte[] Sentinel = [0x57, 0x56, 0x43, 0x41];
            File.WriteAllBytes(Rejectedˉpath, Rejectedˉbundle);
            File.WriteAllBytes(Sentinelˉpath, Sentinel);
            Equal(2, Run([
                Paths[0], Paths[1], Paths[2], Rejectedˉpath, Sentinelˉpath,
            ]));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Sentinelˉpath));

            var Rejectedˉstartup = File.ReadAllBytes(Paths[2]);
            Rejectedˉstartup[57] ^= 1;
            var Rejectedˉstartupˉpath = Path.Combine(
                Directoryˉpath,
                "Rejected-Startup.wvsd");
            File.WriteAllBytes(Rejectedˉstartupˉpath, Rejectedˉstartup);
            File.WriteAllBytes(Sentinelˉpath, Sentinel);
            Equal(2, Run([
                Paths[0], Paths[1], Rejectedˉstartupˉpath, Paths[3], Sentinelˉpath,
            ]));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Sentinelˉpath));
            Equal(64, Run([Paths[0], Paths[1], Paths[2], Paths[3], Paths[0]]));
            Sequenceˉequal(
                Hostedˉverifierˉruntimeˉdata.Build(
                    Consoleˉapplicationˉtarget.Windowsˉx64,
                    Verifier.Module.Capabilities,
                    X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                        Verifierˉfragment,
                        Nativeˉserviceˉplatform.Windows),
                    Nativeˉentry),
                File.ReadAllBytes(Paths[0]));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static byte[] Platformˉresponseˉfrom(
        Nativeˉserviceˉplatform platform,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentry,
        ImmutableArray<byte> application,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        if (platform == Nativeˉserviceˉplatform.Windows)
        {
            var Layout = Windowsˉhostedˉverifierˉapplicationˉcontract.Plan(
                bundle,
                nativeˉentry,
                profile);
            byte[] Payload = [
                .. application.AsSpan(0, checked((int)Layout.Headerˉbytes)),
                .. application.AsSpan(checked((int)Layout.Importˉfileˉoffset), 4096),
                .. application.AsSpan(checked((int)Layout.Relocationˉfileˉoffset), 12),
            ];
            return Responseˉwithˉpayload(
                1113019991,
                1,
                [4096u, 512u, 4096u, 12u],
                Payload);
        }
        return Responseˉwithˉpayload(
            1112299095,
            1,
            [4096u, 4096u, 0u, 0u],
            application.AsSpan(0, 4096));
    }

    private static byte[] Startupˉresponseˉfrom(
        Nativeˉserviceˉplatform platform,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentry,
        ImmutableArray<byte> application,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        var Offset = platform == Nativeˉserviceˉplatform.Windows ? 512 : 4096;
        var Count = platform == Nativeˉserviceˉplatform.Windows
            ? Windowsˉhostedˉverifierˉapplicationˉcontract.Startupˉbytes(profile)
            : Linuxˉhostedˉverifierˉapplicationˉcontract.Startupˉbytes(profile);
        return Responseˉwithˉpayload(
            1146312279,
            1,
            [1u, checked((uint)Count), 0u, 0u],
            application.AsSpan(Offset, Count));
    }

    private static byte[] Responseˉwithˉpayload(
        uint magic,
        uint version,
        uint[] fields,
        ReadOnlySpan<byte> payload)
    {
        var Headerˉbytes = checked(16 + fields.Length * 4);
        var Result = new byte[checked(Headerˉbytes + payload.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(0), magic);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), version);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(8),
            checked((uint)Result.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), 0);
        for (var Index = 0; Index < fields.Length; Index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(16 + Index * 4),
                fields[Index]);
        }
        payload.CopyTo(Result.AsSpan(Headerˉbytes));
        return Result;
    }
}
