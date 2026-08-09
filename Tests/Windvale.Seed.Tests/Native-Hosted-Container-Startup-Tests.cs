using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉcontainerˉstartupˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Startup-Tool.wv",
                "Native-Hosted-Container-Startup-Tool.wv"),
            [
                Source("Foundation/Sha256.wv", "Foundation-Sha256.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Startup-Instantiation-Core.wv",
                    "Native-Hosted-Startup-Instantiation-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_NAME,
            Module.Module.Name);
        Sequenceˉequal(
            [
                "console.write_line",
                "diagnostic.write_line",
                "file.read_bytes",
                "file.write_bytes",
                "process.argument",
                "process.argument_count",
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
                Nativeˉservice.Fileˉwriteˉbytes,
            ],
            Native.Requiredˉservices);

        var Windows = Hostedˉcontainerˉstartupˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉcontainerˉstartupˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Capabilities = Hostedˉtoolˉtestˉcapabilities();
        var Runtime = Hostedˉcompilerˉruntimeˉdata.Build(
            Target,
            Capabilities,
            Bundle,
            0,
            Hostedˉcompilerˉapplicationˉprofile.Compiler);
        var Plan = Nativeˉhostedˉcontainerˉconstructor.Execute(
            Nativeˉhostedˉcontainerˉconstructor.Buildˉrequest(
                Target,
                Hostedˉcompilerˉapplicationˉprofile.Compiler,
                Bundle,
                0,
                Runtime));
        uint Read(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(Plan.AsSpan()[offset..]);
        var Targets = ImmutableArray.CreateBuilder<uint>(checked((int)(Read(100) / 4)));
        var Targetˉend = checked(128 + (int)Read(100));
        for (var Offset = 128; Offset < Targetˉend; Offset += sizeof(uint))
        {
            Targets.Add(BinaryPrimitives.ReadUInt32LittleEndian(Plan.AsSpan()[Offset..]));
        }
        var Object = Target == Consoleˉapplicationˉtarget.Windowsˉx64
            ? Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Windowsˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Windows-X64-Hosted-Compiler.wvo",
                Windowsˉhostedˉcompilerˉstartup.WVO_BYTES,
                Windowsˉhostedˉcompilerˉstartup.WVO_SHA256)
            : Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Linuxˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Linux-X64-Hosted-Compiler.wvo",
                Linuxˉhostedˉcompilerˉstartup.WVO_BYTES,
                Linuxˉhostedˉcompilerˉstartup.WVO_SHA256);
        var Inputs = new Nativeˉhostedˉstartupˉinputs(
            Read(80),
            Read(44),
            Target == Consoleˉapplicationˉtarget.Windowsˉx64 ? 40u : 26u,
            Targets.ToImmutable(),
            Object);
        var Expectedˉrequest = Nativeˉhostedˉstartupˉinstantiator.Buildˉrequest(Inputs);
        var Expected = Nativeˉhostedˉstartupˉinstantiator.Buildˉwithˉwindvale(
            Expectedˉrequest);
        _ = Nativeˉhostedˉstartupˉinstantiator.Verifyˉresponse(
            Inputs,
            Expectedˉrequest.Length,
            Expected);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-container-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Startup.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "recovery-aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Planˉpath = Path.Combine(Directoryˉpath, "Plan.wvcd");
            var Objectˉpath = Path.Combine(Directoryˉpath, "Startup.wvo");
            var Responseˉpath = Path.Combine(Directoryˉpath, "Response.wvsd");
            File.WriteAllBytes(Planˉpath, Plan.AsSpan());
            File.WriteAllBytes(Objectˉpath, Object.AsSpan());
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉcontainerˉstartup(
                    Application,
                    Planˉpath,
                    Objectˉpath,
                    Responseˉpath,
                    $"hosted container startup status=Valid bytes={Expected.Length}\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Responseˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x53];
            var Changedˉobject = Object.ToArray();
            Changedˉobject[0] ^= 0x01;
            File.WriteAllBytes(Objectˉpath, Changedˉobject);
            File.WriteAllBytes(Responseˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉcontainerˉstartup(
                    Application,
                    Planˉpath,
                    Objectˉpath,
                    Responseˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted container startup status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Responseˉpath));

            File.WriteAllBytes(Objectˉpath, Object.AsSpan());
            Equal(
                64,
                Executeˉhostedˉcontainerˉstartup(
                    Application,
                    Planˉpath,
                    Objectˉpath,
                    Planˉpath,
                    string.Empty,
                    expectedˉerror:
                        "Usage: wvhoststartup <plan.wvcd> <startup.wvo> <response.wvsd>\n"));
            Sequenceˉequal(Plan, File.ReadAllBytes(Planˉpath));

            var Frontˉdoorˉoutput = Path.Combine(Directoryˉpath, "Native-Startup.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Startup-Tool.wvproj"),
                Frontˉdoorˉoutput);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Frontˉdoorˉoutput));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static int Executeˉhostedˉcontainerˉstartup(
        ImmutableArray<byte> application,
        string plan,
        string startupˉobject,
        string output,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [plan, startupˉobject, output],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [plan, startupˉobject, output],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
