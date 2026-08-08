using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_CONTAINER_SEGMENT_SET_WVB_BYTES = 31_271;
    private const string NATIVE_HOSTED_CONTAINER_SEGMENT_SET_WVB_SHA256 =
        "6ce0c3a4bf48b6d0db4c50574805655777be93f6a10555a4d423947b00bd0018";
    private const string LINUX_HOSTED_CONTAINER_PUBLISHER_STARTUP_SHA256 =
        "88d45c0936a81d1727a36a6013353e4b01da2ac3c3e121baa7cf21ee17234965";
    private const string WINDOWS_HOSTED_CONTAINER_PUBLISHER_STARTUP_SHA256 =
        "84475183f21b69abde8d73cc9748cca7b7c8377335d4a8ddabe8a9dfc88ea57b";

    private static void Nativeˉhostedˉcontainerˉsegmentˉsetˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));
        var Compiled = Seedˉcompiler.Compileˉmodules(
            Source(
                "Linker/Windvale/Native-Hosted-Container-Segment-Set-Admission-Tool.wv",
                "Native-Hosted-Container-Segment-Set-Admission-Tool.wv"),
            [
                Source("Foundation/Byte-Construction.wv", "Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv",
                    "Native-Hosted-Container-Byte-Construction.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Layout.wv",
                    "Native-Hosted-Container-Layout.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Segmentation-Core.wv",
                    "Native-Hosted-Container-Segmentation-Core.wv"),
                Source(
                    "Linker/Windvale/Native-Hosted-Container-Segment-Set-Core.wv",
                    "Native-Hosted-Container-Segment-Set-Core.wv"),
            ]);
        True(Compiled.Success, string.Join(" | ", Compiled.Diagnostics));
        Equal(NATIVE_HOSTED_CONTAINER_SEGMENT_SET_WVB_BYTES, Compiled.Moduleˉbytes.Length);
        Equal(
            NATIVE_HOSTED_CONTAINER_SEGMENT_SET_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Compiled.Moduleˉbytes.AsSpan()));
        var Module = Moduleˉcodec.Readˉandˉverify(Compiled.Moduleˉbytes.AsSpan());
        Sequenceˉequal(
            [
                "diagnostic.write_line",
                "file.read_bytes",
                "process.argument",
                "process.argument_count",
            ],
            Module.Module.Capabilities.Select(Capability => Capability.Name));
        var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(
            [
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
            ],
            Native.Requiredˉservices);

        Assertˉstagingˉobject(
            Assembleˉsuccess(Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Linux-X64-Hosted-Container-Publisher.wva")),
            190,
            LINUX_HOSTED_CONTAINER_PUBLISHER_STARTUP_SHA256,
            "Linux_hosted_container_publisher_startup");
        Assertˉstagingˉobject(
            Assembleˉsuccess(Readˉembeddedˉsource(
                "Windvale.Seed.Tests.Windows-X64-Hosted-Container-Publisher.wva")),
            194,
            WINDOWS_HOSTED_CONTAINER_PUBLISHER_STARTUP_SHA256,
            "Windows_hosted_container_publisher_startup");

        var Windowsˉpublisher =
            Hostedˉcontainerˉpublisherˉapplicationˉwriter.Writeˉwindows(
                Module,
                Native,
                Compiled.Moduleˉbytes.AsSpan());
        True(
            Windowsˉpublisher.Success,
            "The Windows hosted-container publisher package was rejected: " +
                string.Join(" | ", Windowsˉpublisher.Diagnostics));
        Equal(
            Hostedˉcontainerˉpublisherˉapplicationˉcontract
                .WINDOWS_APPLICATION_BYTES,
            Windowsˉpublisher.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉpublisherˉapplicationˉcontract
                .WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(
                Windowsˉpublisher.Imageˉbytes.AsSpan()));

        var Linuxˉpublisher =
            Hostedˉcontainerˉpublisherˉapplicationˉwriter.Writeˉlinux(
                Module,
                Native,
                Compiled.Moduleˉbytes.AsSpan());
        True(
            Linuxˉpublisher.Success,
            "The Linux hosted-container publisher package was rejected: " +
                string.Join(" | ", Linuxˉpublisher.Diagnostics));
        Equal(
            Hostedˉcontainerˉpublisherˉapplicationˉcontract
                .LINUX_APPLICATION_BYTES,
            Linuxˉpublisher.Imageˉbytes.Length);
        Equal(
            Hostedˉcontainerˉpublisherˉapplicationˉcontract
                .LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(
                Linuxˉpublisher.Imageˉbytes.AsSpan()));

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
        var Request = Nativeˉhostedˉcontainerˉmaterializationˉsession.Buildˉrequests(
            Plan,
            Header,
            Startup,
            Bundle.Imageˉbytes,
            Imports,
            Runtime,
            Relocation)[0];
        var Response = Nativeˉhostedˉcontainerˉsegmentˉconstructor.Execute(Request);
        var Manifest = Buildˉhostedˉcontainerˉsegmentˉsetˉmanifest(
            Plan,
            [Request],
            [Response]);

        const string PLAN_RESOURCE = "container.wvcd";
        const string PREFIX = "container";
        const string MANIFEST_RESOURCE = "container.wvhm";
        const string DESTINATION_RESOURCE = "application.exe";
        var Resources = new Dictionary<string, ImmutableArray<byte>>(StringComparer.Ordinal)
        {
            [PLAN_RESOURCE] = Plan,
            [MANIFEST_RESOURCE] = Manifest,
            [$"{PREFIX}.request-0"] = Request,
            [$"{PREFIX}.response-0"] = Response,
        };

        (int Exitˉcode, string Diagnostics, int Reads) Run(
            IReadOnlyDictionary<string, ImmutableArray<byte>> resources,
            IReadOnlyList<string>? arguments = null)
        {
            var Reader = new Testˉfileˉreader((Name, Maximum) =>
            {
                True(resources.TryGetValue(Name, out var Bytes),
                    $"Unexpected hosted-container segment resource '{Name}'.");
                True(Bytes.Length <= Maximum,
                    "A hosted-container segment resource exceeded the read bound.");
                return Bytes;
            });
            var Diagnostics = new StringWriter();
            var Context = new Hostedˉresourceˉcontext(
                (arguments ??
                    [PLAN_RESOURCE, PREFIX, MANIFEST_RESOURCE, DESTINATION_RESOURCE])
                    .ToImmutableArray(),
                TextWriter.Null,
                Diagnostics,
                Reader);
            var Authorized = Module.Module.Capabilities
                .Select(Capability => Capability.Name)
                .ToImmutableHashSet(StringComparer.Ordinal);
            var Result = new Referenceˉruntime(
                Module,
                new Referenceˉcapabilityˉhost(Context),
                new Runtimeˉoptions(Authorized) with
                {
                    Maximumˉinstructions = 1_000_000_000,
                })
                .Runˉmain();
            return (Result.Exitˉcode, Diagnostics.ToString(), Reader.Readˉcount);
        }

        var Accepted = Run(Resources);
        Equal(0, Accepted.Exitˉcode);
        Equal(string.Empty, Accepted.Diagnostics);
        Equal(4, Accepted.Reads);

        var Corruptedˉresponse = Response.ToArray();
        Corruptedˉresponse[^1] ^= 1;
        var Corruptedˉresources = new Dictionary<string, ImmutableArray<byte>>(Resources)
        {
            [$"{PREFIX}.response-0"] = Corruptedˉresponse.ToImmutableArray(),
        };
        var Corrupted = Run(Corruptedˉresources);
        Equal(1, Corrupted.Exitˉcode);
        Equal(
            "hosted container segment set content=Invalidˉresponse\n",
            Corrupted.Diagnostics);
        Equal(4, Corrupted.Reads);

        var Mismatchedˉrequest = Request.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Mismatchedˉrequest.AsSpan(32 + 24),
            Read(24) == 1 ? 2u : 1u);
        var Mismatchedˉresources = new Dictionary<string, ImmutableArray<byte>>(Resources)
        {
            [$"{PREFIX}.request-0"] = Mismatchedˉrequest.ToImmutableArray(),
        };
        var Mismatched = Run(Mismatchedˉresources);
        Equal(1, Mismatched.Exitˉcode);
        Equal(
            "hosted container segment set content=Invalidˉrequest\n",
            Mismatched.Diagnostics);
        Equal(3, Mismatched.Reads);

        var Invalidˉmanifest = Manifest.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Invalidˉmanifest.AsSpan(32), 1);
        var Invalidˉresources = new Dictionary<string, ImmutableArray<byte>>(Resources)
        {
            [MANIFEST_RESOURCE] = Invalidˉmanifest.ToImmutableArray(),
        };
        var Invalid = Run(Invalidˉresources);
        Equal(1, Invalid.Exitˉcode);
        Equal(
            "hosted container segment set status=Invalidˉsegmentˉindex\n",
            Invalid.Diagnostics);
        Equal(2, Invalid.Reads);

        var Alias = Run(
            Resources,
            [PLAN_RESOURCE, PREFIX, PLAN_RESOURCE, DESTINATION_RESOURCE]);
        Equal(1, Alias.Exitˉcode);
        Equal(
            "hosted container segment set content=Invalidˉresource\n",
            Alias.Diagnostics);
        Equal(0, Alias.Reads);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-hosted-segment-set-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Moduleˉpath = Path.Combine(Directoryˉpath, "Admission.wvb");
            File.WriteAllBytes(Moduleˉpath, Compiled.Moduleˉbytes.AsSpan());
            var Cliˉtarget = OperatingSystem.IsWindows()
                ? Hostedˉcontainerˉpublisherˉapplicationˉcontract.WINDOWS_TARGET_NAME
                : Hostedˉcontainerˉpublisherˉapplicationˉcontract.LINUX_TARGET_NAME;
            var Cliˉapplication = Executeˉinspectorˉtool(
                "aot",
                Moduleˉpath,
                "--target",
                Cliˉtarget);
            Equal(0, Cliˉapplication.Exitˉcode);
            Equal(string.Empty, Cliˉapplication.Standardˉerror);
            Contains(Cliˉapplication.Standardˉoutput, $"Target: {Cliˉtarget}");
            Sequenceˉequal(
                OperatingSystem.IsWindows()
                    ? Windowsˉpublisher.Imageˉbytes
                    : Linuxˉpublisher.Imageˉbytes,
                File.ReadAllBytes(Path.ChangeExtension(
                    Moduleˉpath,
                    Windvale.Tool.Program.Targetˉoutputˉextension(Cliˉtarget))));

            var Planˉpath = Path.Combine(Directoryˉpath, "container.wvcd");
            var Prefixˉpath = Path.Combine(Directoryˉpath, "container");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "container.wvhm");
            var Requestˉpath = $"{Prefixˉpath}.request-0";
            var Responseˉpath = $"{Prefixˉpath}.response-0";
            var Destinationˉpath = Path.Combine(Directoryˉpath, "application.bin");
            File.WriteAllBytes(Planˉpath, Plan.AsSpan());
            File.WriteAllBytes(Manifestˉpath, Manifest.AsSpan());
            File.WriteAllBytes(Requestˉpath, Request.AsSpan());
            File.WriteAllBytes(Responseˉpath, Response.AsSpan());
            File.WriteAllBytes(Destinationˉpath, [9, 8, 7, 6]);
            var Application = OperatingSystem.IsWindows()
                ? Windowsˉpublisher.Imageˉbytes
                : Linuxˉpublisher.Imageˉbytes;
            var Arguments = new[]
            {
                Planˉpath,
                Prefixˉpath,
                Manifestˉpath,
                Destinationˉpath,
            };
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉhostedˉcontainerˉpublisher(
                    Application,
                    Arguments,
                    Loaded));
            var Payloadˉbytes = checked((int)
                BinaryPrimitives.ReadUInt32LittleEndian(Response.AsSpan()[32..]));
            Sequenceˉequal(
                Response.AsSpan(40, Payloadˉbytes).ToArray(),
                File.ReadAllBytes(Destinationˉpath));
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvpub-*").Count());

            byte[] Preserved = [4, 3, 2, 1];
            var Changedˉresponse = Response.ToArray();
            Changedˉresponse[^1] ^= 1;
            File.WriteAllBytes(Responseˉpath, Changedˉresponse);
            File.WriteAllBytes(Destinationˉpath, Preserved);
            Equal(
                1,
                Executeˉhostedˉcontainerˉpublisher(
                    Application,
                    Arguments,
                    expectedˉerror:
                        "hosted container segment set content=Invalidˉresponse\n"));
            Sequenceˉequal(Preserved, File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvpub-*").Count());

            File.WriteAllBytes(Responseˉpath, Response.AsSpan());
            File.Delete(Destinationˉpath);
            Createˉtestˉhardˉlink(Destinationˉpath, Planˉpath);
            Equal(
                1,
                Executeˉhostedˉcontainerˉpublisher(Application, Arguments));
            Sequenceˉequal(Plan, File.ReadAllBytes(Planˉpath));
            Sequenceˉequal(Plan, File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvpub-*").Count());

            var Output = Path.Combine(Directoryˉpath, "Native-Admission.wvb");
            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Hosted-Container-Segment-Set-Admission-Tool.wvproj"),
                Output);
            Equal(0, Build.Exitˉcode);
            Equal(string.Empty, Build.Error);
            Sequenceˉequal(Compiled.Moduleˉbytes, File.ReadAllBytes(Output));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static int Executeˉhostedˉcontainerˉpublisher(
        ImmutableArray<byte> application,
        IReadOnlyList<string> arguments,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                arguments: arguments,
                timeoutˉmilliseconds: 60_000,
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                string.Empty,
                arguments,
                timeoutˉmilliseconds: 60_000,
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);

    private static ImmutableArray<byte> Buildˉhostedˉcontainerˉsegmentˉsetˉmanifest(
        ImmutableArray<byte> plan,
        IReadOnlyList<ImmutableArray<byte>> requests,
        IReadOnlyList<ImmutableArray<byte>> responses)
    {
        Equal(requests.Count, responses.Count);
        var Result = new byte[checked(32 + requests.Count * 20)];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, 0x4D48_5657);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(8), checked((uint)Result.Length));
        var Applicationˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(plan.AsSpan()[28..]);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), Applicationˉbytes);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(16), checked((uint)requests.Count));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(20), 4_194_144);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24), checked((uint)plan.Length));
        uint Position = 0;
        for (var Index = 0; Index < requests.Count; Index++)
        {
            var Entry = Result.AsSpan(32 + Index * 20, 20);
            var Segmentˉbytes = BinaryPrimitives.ReadUInt32LittleEndian(
                responses[Index].AsSpan()[32..]);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry, checked((uint)Index));
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[4..], Position);
            BinaryPrimitives.WriteUInt32LittleEndian(Entry[8..], Segmentˉbytes);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Entry[12..], checked((uint)requests[Index].Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Entry[16..], checked((uint)responses[Index].Length));
            Position = checked(Position + Segmentˉbytes);
        }
        Equal(Applicationˉbytes, Position);
        return Result.ToImmutableArray();
    }
}
