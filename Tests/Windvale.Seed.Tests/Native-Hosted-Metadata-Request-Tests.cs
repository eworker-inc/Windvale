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
    private static void Nativeˉhostedˉmetadataˉrequestˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Runtime/Windvale/Native-Hosted-Tool-Metadata-Request-Tool.wv",
                "Native-Hosted-Tool-Metadata-Request-Tool.wv"),
            [
                Source(
                    "Compiler/Windvale/Native-Publication-Core.wv",
                    "Native-Publication-Core.wv"),
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Foundation/Sha256-Compression.wv",
                    "Sha256-Compression.wv"),
                Source(
                    "Foundation/Sha256-Streaming.wv",
                    "Sha256-Streaming.wv"),
                Source(
                    "Runtime/Windvale/Streaming-Sha256-Evidence-Core.wv",
                    "Streaming-Sha256-Evidence-Core.wv"),
                Source(
                    "Runtime/Windvale/Streaming-Sha256-Resource-Evidence.wv",
                    "Streaming-Sha256-Resource-Evidence.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_BYTES,
            Compiled.Moduleˉbytes.Length);
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));

        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_NAME,
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

        var Windows = Hostedˉmetadataˉrequestˉapplicationˉwriter.Writeˉwindows(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Windows.Success, string.Join(" | ", Windows.Diagnostics));
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Hostedˉmetadataˉrequestˉapplicationˉwriter.Writeˉlinux(
            Native,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, string.Join(" | ", Linux.Diagnostics));
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Hostedˉmetadataˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Target = OperatingSystem.IsWindows()
            ? Consoleˉapplicationˉtarget.Windowsˉx64
            : Consoleˉapplicationˉtarget.Linuxˉx64;
        var Fixtureˉbundle = Hostedˉtoolˉtestˉbundle(Target);
        var Fragment = Fixtureˉbundle.Imageˉbytes[..Fixtureˉbundle.Nativeˉimageˉbytes];
        var Serviceˉcode = Fixtureˉbundle.Placements.Select(Placement =>
            new Nativeˉserviceˉcode(
                Placement.Service,
                Placement.Adapter,
                Fixtureˉbundle.Imageˉbytes[
                    Placement.Imageˉoffset..
                    (Placement.Imageˉoffset + Placement.Codeˉbytes)]))
            .ToImmutableArray();
        var Services = Serviceˉcode.Select(Service =>
            new Nativeˉpublicationˉservice(Service.Service, Service.Code.Length))
            .ToImmutableArray();
        var Plan = X64ˉnativeˉpublicationˉlayout.Plan(Fragment.Length, Services);
        var Image = X64ˉnativeˉserviceˉbundleˉmaterialization.Materialize(
            Fragment,
            Serviceˉcode,
            Plan);
        var Placements = Plan.Placements.Select((Placement, Index) =>
            new Nativeˉserviceˉbundleˉplacement(
                Placement.Service,
                Serviceˉcode[Index].Adapter,
                Fixtureˉbundle.Placements[Index].Serviceˉtableˉoffset,
                Placement.Offset,
                Placement.Size,
                Objectˉdigest.Calculateˉsha256(Serviceˉcode[Index].Code.AsSpan())))
            .ToImmutableArray();
        var Bundle = new Nativeˉserviceˉbundle(
            Fixtureˉbundle.Platform,
            Fragment.Length,
            Image,
            Placements);
        var Planˉrequest = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
            Bundle.Nativeˉimageˉbytes,
            Services);
        var Inputs = new Nativeˉhostedˉtoolˉmetadataˉinputs(
            (uint)Target,
            (uint)Hostedˉcompilerˉapplicationˉprofile.Compiler,
            Hostedˉcompilerˉruntimeˉdata.BUNDLE_TEXT_OFFSET,
            0,
            Bundle);
        var Expected = Nativeˉhostedˉtoolˉmetadataˉbuilder.Buildˉrequest(Inputs);
        var Resources = new[] { Fragment.ToArray() }
            .Concat(Serviceˉcode.Select(Service => Service.Code.ToArray()))
            .ToArray();
        var Logicalˉbytes = Resources.Sum(Resource => Resource.Length);
        True(
            Bundle.Imageˉbytes.Length > Logicalˉbytes,
            "The focused fixture must contain an image-alignment gap.");
        var Regions = new (int Offset, int Bytes)[1 + Bundle.Placements.Length];
        var Logicalˉoffset = 0;
        Regions[0] = (Logicalˉoffset, Bundle.Nativeˉimageˉbytes);
        Logicalˉoffset += Bundle.Nativeˉimageˉbytes;
        for (var Index = 0; Index < Bundle.Placements.Length; Index++)
        {
            Regions[Index + 1] = (
                Logicalˉoffset,
                Bundle.Placements[Index].Codeˉbytes);
            Logicalˉoffset += Bundle.Placements[Index].Codeˉbytes;
        }
        var Manifest = Buildˉmetadataˉrequestˉmanifest(
            Resources,
            Regions);

        var Inputˉbytes = new byte[32];
        BinaryPrimitives.WriteUInt32LittleEndian(Inputˉbytes, 0x494D_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(Inputˉbytes.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Inputˉbytes.AsSpan(8), 32);
        BinaryPrimitives.WriteUInt32LittleEndian(Inputˉbytes.AsSpan(12), (uint)Target);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Inputˉbytes.AsSpan(16),
            (uint)Hostedˉcompilerˉapplicationˉprofile.Compiler);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-metadata-request-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Metadata-Request.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉmetadataˉrequestˉapplicationˉcontract.LINUX_TARGET_NAME;
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

            var Inputsˉpath = Path.Combine(Directoryˉpath, "Inputs.wvmi");
            var Planˉpath = Path.Combine(Directoryˉpath, "Plan.wvpq");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "Bundle.wvhs");
            var Prefix = Path.Combine(Directoryˉpath, "Bundle");
            var Requestˉpath = Path.Combine(Directoryˉpath, "Request.wvhq");
            File.WriteAllBytes(Inputsˉpath, Inputˉbytes);
            File.WriteAllBytes(Planˉpath, Planˉrequest.AsSpan());
            File.WriteAllBytes(Manifestˉpath, Manifest);
            for (var Index = 0; Index < Resources.Length; Index++)
            {
                File.WriteAllBytes(Prefix + $".chunk-{Index}", Resources[Index]);
            }
            var Application = OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes;
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉmetadataˉrequest(
                    Application,
                    Inputsˉpath,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    Requestˉpath,
                    "hosted metadata request status=Valid bytes=576\n",
                    Loaded));
            Sequenceˉequal(Expected, File.ReadAllBytes(Requestˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            byte[] Sentinel = [0x57, 0x56, 0x48, 0x51];
            var Changedˉplan = Planˉrequest.ToArray();
            Changedˉplan[0] ^= 0x01;
            File.WriteAllBytes(Planˉpath, Changedˉplan);
            File.WriteAllBytes(Requestˉpath, Sentinel);
            Equal(
                2,
                Executeˉhostedˉmetadataˉrequest(
                    Application,
                    Inputsˉpath,
                    Planˉpath,
                    Manifestˉpath,
                    Prefix,
                    Requestˉpath,
                    string.Empty,
                    expectedˉerror: "hosted metadata request status=Rejected\n"));
            Sequenceˉequal(Sentinel, File.ReadAllBytes(Requestˉpath));

            var Frontˉdoorˉoutput = Path.Combine(
                Directoryˉpath,
                "Native-Metadata-Request.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Tool-Metadata-Request.wvproj"),
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

    private static byte[] Buildˉmetadataˉrequestˉmanifest(
        IReadOnlyList<byte[]> resources,
        (int Offset, int Bytes)[] regions)
    {
        var Logicalˉbytes = resources.Sum(Resource => Resource.Length);
        var Result = new byte[32 + resources.Count * 20 + regions.Length * 12];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x5348_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)Result.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), (uint)resources.Count);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(16), (uint)regions.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), (uint)Logicalˉbytes);
        var Logicalˉoffset = 0;
        for (var Index = 0; Index < resources.Count; Index++)
        {
            var Record = 32 + Index * 20;
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Record), (uint)Index);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 4),
                (uint)Logicalˉoffset);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Record + 8), 0);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 12),
                (uint)resources[Index].Length);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 16),
                (uint)resources[Index].Length);
            Logicalˉoffset += resources[Index].Length;
        }
        var Regionˉbase = 32 + resources.Count * 20;
        for (var Index = 0; Index < regions.Length; Index++)
        {
            var Record = Regionˉbase + Index * 12;
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(Record), (uint)Index);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 4),
                (uint)regions[Index].Offset);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Result.AsSpan(Record + 8),
                (uint)regions[Index].Bytes);
        }
        return Result;
    }

    private static int Executeˉhostedˉmetadataˉrequest(
        ImmutableArray<byte> application,
        string inputs,
        string plan,
        string manifest,
        string prefix,
        string request,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                [inputs, plan, manifest, prefix, request],
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                [inputs, plan, manifest, prefix, request],
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
