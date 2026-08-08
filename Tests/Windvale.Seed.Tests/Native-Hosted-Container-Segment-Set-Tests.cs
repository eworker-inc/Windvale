using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_HOSTED_CONTAINER_SEGMENT_SET_WVB_BYTES = 31_271;
    private const string NATIVE_HOSTED_CONTAINER_SEGMENT_SET_WVB_SHA256 =
        "6ce0c3a4bf48b6d0db4c50574805655777be93f6a10555a4d423947b00bd0018";

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
            var Output = Path.Combine(Directoryˉpath, "Admission.wvb");
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
