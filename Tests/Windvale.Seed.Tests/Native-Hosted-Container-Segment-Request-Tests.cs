using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉcontainerˉsegmentˉrequestˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Segment-Request-Tool.wv",
                "Native-Hosted-Container-Segment-Request-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source("Foundation/Decimal-Parsing.wv", "Decimal-Parsing.wv"),
                Source(
                    "Foundation/Immutable-Source-Regions.wv",
                    "Immutable-Source-Regions.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv",
                    "Native-Hosted-Container-Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Layout.wv",
                    "Native-Hosted-Container-Layout.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Segmentation-Core.wv",
                    "Native-Hosted-Container-Segmentation-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_NAME,
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

        var Windows =
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉwriter.Writeˉwindows(
                Native,
                Module.Module.Capabilities,
                Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux =
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉwriter.Writeˉlinux(
                Native,
                Module.Module.Capabilities,
                Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
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
        var Header = Enumerable.Repeat((byte)0x11, checked((int)Read(36)))
            .ToImmutableArray();
        var Startup = Enumerable.Repeat((byte)0x22, checked((int)Read(44)))
            .ToImmutableArray();
        var Imports = Enumerable.Repeat((byte)0x33, checked((int)Read(60)))
            .ToImmutableArray();
        var Relocation = Enumerable.Repeat((byte)0x44, checked((int)Read(76)))
            .ToImmutableArray();
        var Expected = Nativeˉhostedˉcontainerˉmaterializationˉsession.Buildˉrequests(
            Plan,
            Header,
            Startup,
            Bundle.Imageˉbytes,
            Imports,
            Runtime,
            Relocation)[0];
        var Regions = new (int Imageˉoffset, ImmutableArray<byte> Bytes)[]
        {
            (0, Header),
            (checked((int)Read(40)), Startup),
            (checked((int)Read(48)), Bundle.Imageˉbytes),
            (checked((int)Read(56)), Imports),
            (checked((int)Read(64)), Runtime),
            (checked((int)Read(72)), Relocation),
        };
        var Sources = Buildˉimmutableˉsourceˉgeometry(
            Regions,
            checked((int)Read(28)),
            Header.Length + Startup.Length + Bundle.Imageˉbytes.Length / 2);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-segment-request-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Segment-Request.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "aot", Moduleˉpath, "--target", Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows() ? Windows.Imageˉbytes : Linux.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Planˉpath = Path.Combine(Directoryˉpath, "Plan.wvcd");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "Sources.wvsg");
            var Prefix = Path.Combine(Directoryˉpath, "Sources");
            var Resourceˉzeroˉpath = Prefix + ".chunk-0";
            var Resourceˉoneˉpath = Prefix + ".chunk-1";
            var Outputˉpath = Path.Combine(Directoryˉpath, "Request.wvht");
            File.WriteAllBytes(Planˉpath, Plan.AsSpan());
            File.WriteAllBytes(Manifestˉpath, Sources.Manifest);
            File.WriteAllBytes(Resourceˉzeroˉpath, Sources.Resourceˉzero);
            File.WriteAllBytes(Resourceˉoneˉpath, Sources.Resourceˉone);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉcontainerˉsegmentˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "0",
                    Outputˉpath,
                    $"hosted container segment request status=Valid segment=0 bytes={Expected.Length}\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Outputˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x48, 0x54];
            var Changedˉmanifest = Sources.Manifest.ToArray();
            Changedˉmanifest[0] ^= 0x01;
            File.WriteAllBytes(Manifestˉpath, Changedˉmanifest);
            File.WriteAllBytes(Outputˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉcontainerˉsegmentˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "0",
                    Outputˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted container segment request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            File.WriteAllBytes(Manifestˉpath, Sources.Manifest);
            Equal(
                2,
                Executeˉhostedˉcontainerˉsegmentˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "1",
                    Outputˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted container segment request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            Equal(
                2,
                Executeˉhostedˉcontainerˉsegmentˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "0",
                    Resourceˉzeroˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted container segment request status=Rejected\n"));
            Sequenceˉequal(
                Sources.Resourceˉzero,
                File.ReadAllBytes(Resourceˉzeroˉpath));

            var Frontˉdoorˉoutput = Path.Combine(
                Directoryˉpath,
                "Native-Segment-Request.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Segment-Request-Tool.wvproj"),
                Frontˉdoorˉoutput);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(
                Compiled.Moduleˉbytes,
                File.ReadAllBytes(Frontˉdoorˉoutput));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static int Executeˉhostedˉcontainerˉsegmentˉrequest(
        ImmutableArray<byte> application,
        string plan,
        string manifest,
        string prefix,
        string segment,
        string output,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [plan, manifest, prefix, segment, output],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [plan, manifest, prefix, segment, output],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
