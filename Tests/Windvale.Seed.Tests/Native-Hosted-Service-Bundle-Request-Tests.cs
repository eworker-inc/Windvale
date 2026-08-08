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
    private static void Nativeˉhostedˉserviceˉbundleˉrequestˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Hosted-Service-Bundle-Request-Tool.wv",
                "Native-Hosted-Service-Bundle-Request-Tool.wv"),
            [
                Source(
                    "Compiler/Windvale/Native-Publication-Core.wv",
                    "Native-Publication-Core.wv"),
                Source("Foundation/Decimal-Parsing.wv", "Decimal-Parsing.wv"),
                Source(
                    "Foundation/Immutable-Source-Regions.wv",
                    "Immutable-Source-Regions.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_NAME,
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
            Hostedˉserviceˉbundleˉrequestˉapplicationˉwriter.Writeˉwindows(
                Native,
                Module.Module.Capabilities,
                Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux =
            Hostedˉserviceˉbundleˉrequestˉapplicationˉwriter.Writeˉlinux(
                Native,
                Module.Module.Capabilities,
                Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Bundle = Hostedˉtoolˉtestˉbundle(Target);
        var Fragment = Bundle.Imageˉbytes[..Bundle.Nativeˉimageˉbytes];
        var Serviceˉcode = Bundle.Placements.Select(Placement =>
            new Nativeˉserviceˉcode(
                Placement.Service,
                Placement.Adapter,
                Bundle.Imageˉbytes[
                    Placement.Imageˉoffset..
                    (Placement.Imageˉoffset + Placement.Codeˉbytes)]))
            .ToImmutableArray();
        var Services = Serviceˉcode.Select(Service =>
            new Nativeˉpublicationˉservice(Service.Service, Service.Code.Length))
            .ToImmutableArray();
        var Plan = X64ˉnativeˉpublicationˉlayout.Plan(Fragment.Length, Services);
        var Planˉrequest = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
            Fragment.Length,
            Services);
        var Expected = Nativeˉserviceˉbundleˉmaterializationˉsession.Buildˉrequest(
            Fragment,
            Serviceˉcode,
            Plan,
            0);
        var Sources = Buildˉserviceˉbundleˉsourceˉmanifest(
            Fragment,
            Serviceˉcode,
            Plan);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-bundle-request-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Bundle-Request.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME;
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

            var Planˉpath = Path.Combine(Directoryˉpath, "Plan.wvpq");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "Sources.wvsg");
            var Prefix = Path.Combine(Directoryˉpath, "Sources");
            var Resourceˉzeroˉpath = Prefix + ".chunk-0";
            var Resourceˉoneˉpath = Prefix + ".chunk-1";
            var Outputˉpath = Path.Combine(Directoryˉpath, "Request.wvsq");
            File.WriteAllBytes(Planˉpath, Planˉrequest.AsSpan());
            File.WriteAllBytes(Manifestˉpath, Sources.Manifest);
            File.WriteAllBytes(Resourceˉzeroˉpath, Sources.Resourceˉzero);
            File.WriteAllBytes(Resourceˉoneˉpath, Sources.Resourceˉone);
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉserviceˉbundleˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "0",
                    Outputˉpath,
                    $"hosted service-bundle request status=Valid segment=0 bytes={Expected.Length}\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Outputˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x53, 0x51];
            var Changedˉmanifest = Sources.Manifest.ToArray();
            Changedˉmanifest[0] ^= 0x01;
            File.WriteAllBytes(Manifestˉpath, Changedˉmanifest);
            File.WriteAllBytes(Outputˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉserviceˉbundleˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "0",
                    Outputˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted service-bundle request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            File.WriteAllBytes(Manifestˉpath, Sources.Manifest);
            Equal(
                2,
                Executeˉhostedˉserviceˉbundleˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "1",
                    Outputˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted service-bundle request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));

            Equal(
                2,
                Executeˉhostedˉserviceˉbundleˉrequest(
                    Application,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    "0",
                    Resourceˉzeroˉpath,
                    string.Empty,
                    expectedˉerror:
                        "hosted service-bundle request status=Rejected\n"));
            Sequenceˉequal(
                Sources.Resourceˉzero,
                File.ReadAllBytes(Resourceˉzeroˉpath));

            var Frontˉdoorˉoutput = Path.Combine(
                Directoryˉpath,
                "Native-Bundle-Request.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Service-Bundle-Request-Tool.wvproj"),
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

    private static (byte[] Manifest, byte[] Resourceˉzero, byte[] Resourceˉone)
        Buildˉserviceˉbundleˉsourceˉmanifest(
            ImmutableArray<byte> fragment,
            ImmutableArray<Nativeˉserviceˉcode> services,
            Nativeˉpublicationˉplan plan)
    {
        var Logicalˉbytes = checked(
            fragment.Length + services.Sum(Service => Service.Code.Length));
        var Logicalˉsource = new byte[Logicalˉbytes];
        var Logicalˉcursor = 0;
        fragment.AsSpan().CopyTo(Logicalˉsource);
        Logicalˉcursor += fragment.Length;
        foreach (var Service in services)
        {
            Service.Code.AsSpan().CopyTo(
                Logicalˉsource.AsSpan(Logicalˉcursor));
            Logicalˉcursor += Service.Code.Length;
        }
        Equal(Logicalˉbytes, Logicalˉcursor);

        var Split = fragment.Length / 2;
        True(
            Split > 0 && Split < Logicalˉbytes,
            "The source fixture must cross an immutable resource boundary.");
        const int Resourceˉzeroˉoffset = 9;
        const int Resourceˉoneˉoffset = 7;
        var Resourceˉzero = new byte[Resourceˉzeroˉoffset + Split + 5];
        var Resourceˉone = new byte[
            Resourceˉoneˉoffset + Logicalˉbytes - Split + 3];
        Logicalˉsource.AsSpan(0, Split).CopyTo(
            Resourceˉzero.AsSpan(Resourceˉzeroˉoffset));
        Logicalˉsource.AsSpan(Split).CopyTo(
            Resourceˉone.AsSpan(Resourceˉoneˉoffset));

        var Manifest = new byte[32 + 2 * 20 + 11 * 16];
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest, 0x4753_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(8), (uint)Manifest.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(12), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(16), 11);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(20),
            (uint)Logicalˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(24),
            (uint)plan.Imageˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(32), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(36), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(40),
            Resourceˉzeroˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(44),
            (uint)Split);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(48),
            (uint)Resourceˉzero.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Manifest.AsSpan(52), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(56),
            (uint)Split);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(60),
            Resourceˉoneˉoffset);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(64),
            (uint)(Logicalˉbytes - Split));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Manifest.AsSpan(68),
            (uint)Resourceˉone.Length);

        Logicalˉcursor = 0;
        for (var Index = 0; Index < 11; Index++)
        {
            var Record = 72 + Index * 16;
            var Imageˉoffset = Index == 0 ? 0 : plan.Placements[Index - 1].Offset;
            var Regionˉbytes = Index == 0
                ? fragment.Length
                : services[Index - 1].Code.Length;
            BinaryPrimitives.WriteUInt32LittleEndian(
                Manifest.AsSpan(Record),
                (uint)Index);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Manifest.AsSpan(Record + 4),
                (uint)Logicalˉcursor);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Manifest.AsSpan(Record + 8),
                (uint)Imageˉoffset);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Manifest.AsSpan(Record + 12),
                (uint)Regionˉbytes);
            Logicalˉcursor += Regionˉbytes;
        }
        Equal(Logicalˉbytes, Logicalˉcursor);
        return (Manifest, Resourceˉzero, Resourceˉone);
    }

    private static int Executeˉhostedˉserviceˉbundleˉrequest(
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
