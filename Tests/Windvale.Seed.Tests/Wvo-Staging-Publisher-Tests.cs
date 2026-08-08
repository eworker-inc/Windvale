using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WVO_STAGING_PUBLISHER_TOOL_BYTES = 440_994;
    private const string WVO_STAGING_PUBLISHER_TOOL_SHA256 =
        "6ef23e0db58ecd788ca97218428dc7a131662f90f5875f7644f76592a7664acc";
    private const string LINUX_WVO_STAGING_PUBLISHER_WVO_SHA256 =
        "8cb479d958881b8fa74b67dc3de6bc5b669adfd38d699735a2ab62aee610ccba";
    private const string LINUX_WVO_STAGING_ADAPTER_WVO_SHA256 =
        "d0a3cb41b6ffcc0fe6e616e1d2ac3b067252fe1ae20c8c40532505bcd6491be5";
    private const string LINUX_IMMUTABLE_SNAPSHOT_SHELL_WVO_SHA256 =
        "423bd086f68c03b3fd26c296a1789392ebd72a74e5fd10adf0d2e596d2fd2e6d";
    private const string LINUX_HOSTED_CONTAINER_ADAPTER_WVO_SHA256 =
        "fe6b4d60fcf459d2f3f624b58b461b95fc9bf325421712e19ac9aa72dcebf527";
    private const string LINUX_DURABLE_MULTI_CHUNK_WVO_SHA256 =
        "47a22cd108702d6427fe5be9fca00c3c05f38cb26dd69e51c8648544b3f98e76";
    private const string WINDOWS_WVO_STAGING_PUBLISHER_WVO_SHA256 =
        "7e4ef5d1565aed7dddb325faa74f800f5d006567d0de84a84e8bc9b898f420ab";
    private const string WINDOWS_WVO_STAGING_ADAPTER_WVO_SHA256 =
        "86dd44e921418a82c69aa155b671662fa2961041d0ead661a0328f3371f7f045";
    private const string WINDOWS_IMMUTABLE_SNAPSHOT_SHELL_WVO_SHA256 =
        "d5233eb678b1c96eb6c8c4108ff10d7bcc263678defb81915ab1c67a6b398110";
    private const string WINDOWS_HOSTED_CONTAINER_ADAPTER_WVO_SHA256 =
        "bfb42ca6a679a25c7a45660bf0743ee3f4e64febceeede6b079126e1df0aab75";
    private const string WINDOWS_DURABLE_MULTI_CHUNK_WVO_SHA256 =
        "3795ab62b6dc5008748ba7c4332b885419a14479c9c11369bcc13885cad8974b";
    private const string X64_WVO_STAGING_SNAPSHOT_WVO_SHA256 =
        "03ff27e8a8fce7b3eddfb0191b6626c20971df32790f8f7274cd9091a4b69628";
    private const string X64_IMMUTABLE_SNAPSHOT_SEQUENCE_WVO_SHA256 =
        "7c6ea6b16ac8cfcfed9e0983b7e6aedc3ead4aab3a54cb207b75d22a228db676";
    private const string X64_HOSTED_CONTAINER_SNAPSHOT_WVO_SHA256 =
        "390ee99e24e02cfa904f64d1ab772d76f5de358783c3f75e0310e37750cc5e86";

    private static readonly string LINUX_WVO_STAGING_PUBLISHER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Linux-X64-Wvo-Staging-Publisher.wva");
    private static readonly string LINUX_WVO_STAGING_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Linux-X64-Wvo-Staging-Publication-Adapter.wva");
    private static readonly string LINUX_IMMUTABLE_SNAPSHOT_SHELL_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Linux-X64-Immutable-Snapshot-Publisher.wva");
    private static readonly string LINUX_HOSTED_CONTAINER_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Linux-X64-Hosted-Container-Publication-Adapter.wva");
    private static readonly string LINUX_DURABLE_MULTI_CHUNK_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Linux-X64-Durable-Multi-Chunk-Publication.wva");
    private static readonly string WINDOWS_WVO_STAGING_PUBLISHER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Windows-X64-Wvo-Staging-Publisher.wva");
    private static readonly string WINDOWS_WVO_STAGING_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Windows-X64-Wvo-Staging-Publication-Adapter.wva");
    private static readonly string WINDOWS_IMMUTABLE_SNAPSHOT_SHELL_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Windows-X64-Immutable-Snapshot-Publisher.wva");
    private static readonly string WINDOWS_HOSTED_CONTAINER_ADAPTER_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Windows-X64-Hosted-Container-Publication-Adapter.wva");
    private static readonly string WINDOWS_DURABLE_MULTI_CHUNK_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Windows-X64-Durable-Multi-Chunk-Publication.wva");
    private static readonly string X64_WVO_STAGING_SNAPSHOT_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.X64-Wvo-Staging-Snapshot-Table.wva");
    private static readonly string X64_IMMUTABLE_SNAPSHOT_SEQUENCE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.X64-Immutable-Snapshot-Sequence.wva");
    private static readonly string X64_HOSTED_CONTAINER_SNAPSHOT_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.X64-Hosted-Container-Snapshot-Table.wva");

    private static void Nativeˉwvoˉstagingˉpublisherˉruns()
    {
        var Moduleˉbytes = Compileˉwvbˉtoˉwvoˉapplicationˉsuccess(
            "Compiler/Windvale/Native-X64-Lowering-Staging-Admission-Tool.wv",
            NATIVE_X64_STAGING_ADMISSION_SOURCE,
            "staged-WVO publisher tool",
            includeˉpublication: true,
            includeˉstagingˉmanifest: true,
            includeˉstagingˉcontent: true,
            includeˉstagingˉresources: true,
            includeˉpublicationˉtransaction: true);
        Equal(WVO_STAGING_PUBLISHER_TOOL_BYTES, Moduleˉbytes.Length);
        Equal(
            WVO_STAGING_PUBLISHER_TOOL_SHA256,
            Moduleˉdigest.Calculateˉsha256(Moduleˉbytes));
        var Module = Moduleˉcodec.Readˉandˉverify(Moduleˉbytes);
        var Native = X64ˉnativeˉbackend.Compile(Module);
        Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        Sequenceˉequal(
            [
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
            ],
            Native.Fragment.Requiredˉservices);

        var Linuxˉstartup = Assembleˉsuccess(LINUX_WVO_STAGING_PUBLISHER_SOURCE);
        var Linuxˉadapter = Assembleˉsuccess(LINUX_WVO_STAGING_ADAPTER_SOURCE);
        var Linuxˉshell = Assembleˉsuccess(
            LINUX_IMMUTABLE_SNAPSHOT_SHELL_SOURCE);
        var Linuxˉhostedˉadapter = Assembleˉsuccess(
            LINUX_HOSTED_CONTAINER_ADAPTER_SOURCE);
        var Linuxˉtransaction = Assembleˉsuccess(
            LINUX_DURABLE_MULTI_CHUNK_SOURCE);
        var Windowsˉstartup = Assembleˉsuccess(WINDOWS_WVO_STAGING_PUBLISHER_SOURCE);
        var Windowsˉadapter = Assembleˉsuccess(WINDOWS_WVO_STAGING_ADAPTER_SOURCE);
        var Windowsˉshell = Assembleˉsuccess(
            WINDOWS_IMMUTABLE_SNAPSHOT_SHELL_SOURCE);
        var Windowsˉhostedˉadapter = Assembleˉsuccess(
            WINDOWS_HOSTED_CONTAINER_ADAPTER_SOURCE);
        var Windowsˉtransaction = Assembleˉsuccess(
            WINDOWS_DURABLE_MULTI_CHUNK_SOURCE);
        var Snapshot = Assembleˉsuccess(X64_WVO_STAGING_SNAPSHOT_SOURCE);
        var Sequence = Assembleˉsuccess(X64_IMMUTABLE_SNAPSHOT_SEQUENCE_SOURCE);
        var Hostedˉsnapshot = Assembleˉsuccess(
            X64_HOSTED_CONTAINER_SNAPSHOT_SOURCE);
        Assertˉstagingˉobject(
            Linuxˉstartup,
            180,
            LINUX_WVO_STAGING_PUBLISHER_WVO_SHA256,
            "Linux_wvo_staging_publisher_startup");
        Assertˉstagingˉobject(
            Linuxˉadapter,
            281,
            LINUX_WVO_STAGING_ADAPTER_WVO_SHA256,
            "Linux_wvo_staging_publisher_run");
        Assertˉstagingˉobject(
            Linuxˉshell,
            3_485,
            LINUX_IMMUTABLE_SNAPSHOT_SHELL_WVO_SHA256,
            "Linux_immutable_snapshot_publisher_run");
        Assertˉstagingˉobject(
            Linuxˉhostedˉadapter,
            294,
            LINUX_HOSTED_CONTAINER_ADAPTER_WVO_SHA256,
            "Linux_hosted_container_publisher_run");
        Assertˉstagingˉobject(
            Linuxˉtransaction,
            2_432,
            LINUX_DURABLE_MULTI_CHUNK_WVO_SHA256,
            "Linux_durable_multi_chunk_publication_run");
        Assertˉstagingˉobject(
            Windowsˉstartup,
            184,
            WINDOWS_WVO_STAGING_PUBLISHER_WVO_SHA256,
            "Windows_wvo_staging_publisher_startup");
        Assertˉstagingˉobject(
            Windowsˉadapter,
            285,
            WINDOWS_WVO_STAGING_ADAPTER_WVO_SHA256,
            "Windows_wvo_staging_publisher_run");
        Assertˉstagingˉobject(
            Windowsˉshell,
            6_116,
            WINDOWS_IMMUTABLE_SNAPSHOT_SHELL_WVO_SHA256,
            "Windows_immutable_snapshot_publisher_run");
        Assertˉstagingˉobject(
            Windowsˉhostedˉadapter,
            298,
            WINDOWS_HOSTED_CONTAINER_ADAPTER_WVO_SHA256,
            "Windows_hosted_container_publisher_run");
        Assertˉstagingˉobject(
            Windowsˉtransaction,
            4_001,
            WINDOWS_DURABLE_MULTI_CHUNK_WVO_SHA256,
            "Windows_durable_multi_chunk_publication_run");
        Assertˉstagingˉobject(
            Snapshot,
            224,
            X64_WVO_STAGING_SNAPSHOT_WVO_SHA256,
            "X64_wvo_staging_snapshot_table_validate");
        Assertˉstagingˉobject(
            Sequence,
            1_282,
            X64_IMMUTABLE_SNAPSHOT_SEQUENCE_WVO_SHA256,
            "X64_immutable_snapshot_sequence_validate");
        Assertˉstagingˉobject(
            Hostedˉsnapshot,
            256,
            X64_HOSTED_CONTAINER_SNAPSHOT_WVO_SHA256,
            "X64_hosted_container_snapshot_table_validate");

        var Windows = Wvoˉstagingˉpublisherˉapplicationˉwriter.Writeˉwindows(
            Module,
            Native.Fragment,
            Moduleˉbytes);
        True(
            Windows.Success,
            "The Windows staged-WVO publisher package was rejected: " +
                string.Join(" | ", Windows.Diagnostics));
        Equal(
            Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Windows.Imageˉbytes.Length);
        Equal(
            Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Windows.Imageˉbytes.AsSpan()));

        var Linux = Wvoˉstagingˉpublisherˉapplicationˉwriter.Writeˉlinux(
            Module,
            Native.Fragment,
            Moduleˉbytes);
        True(
            Linux.Success,
            "The Linux staged-WVO publisher package was rejected: " +
                string.Join(" | ", Linux.Diagnostics));
        Equal(
            Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Linux.Imageˉbytes.Length);
        Equal(
            Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));

        var Fixture = Buildˉstagingˉcontentˉfixture(
            WVB_TO_WVO_RETURN_42_SOURCE);
        var Manifest = Buildˉstagingˉmanifest(
            Fixture.Chunks.Select(Chunk => checked((uint)Chunk.Length)).ToArray());
        var Expectedˉobject = Fixture.Chunks.SelectMany(Chunk => Chunk).ToArray();
        Assertˉcurrentˉhostˉstagingˉpublisher(
            OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes,
            Fixture.Wvb,
            Manifest,
            Fixture.Chunks,
            Fixture.Codeˉchunk,
            Expectedˉobject);

        var Chunked = Buildˉstagingˉcontentˉfixture(
            Buildˉchunkedˉfunctionˉfixture());
        True(
            Chunked.Chunks[Chunked.Codeˉchunk].Length > 8_192,
            "The staged publisher regression fixture stayed inside one iteration.");
        Assertˉcurrentˉhostˉstagingˉpublisher(
            OperatingSystem.IsWindows()
                ? Windows.Imageˉbytes
                : Linux.Imageˉbytes,
            Chunked.Wvb,
            Buildˉstagingˉmanifest(
                Chunked.Chunks.Select(Chunk => checked((uint)Chunk.Length)).ToArray()),
            Chunked.Chunks,
            Chunked.Codeˉchunk,
            Chunked.Chunks.SelectMany(Chunk => Chunk).ToArray(),
            verifyˉfailureˉpaths: false);
    }

    private static void Assertˉstagingˉobject(
        byte[] bytes,
        int expectedˉbytes,
        string expectedˉsha256,
        string export)
    {
        Equal(expectedˉbytes, bytes.Length);
        Equal(expectedˉsha256, Objectˉdigest.Calculateˉsha256(bytes));
        var Value = Objectˉcodec.Readˉandˉverify(bytes).Value;
        Equal(
            export,
            Value.Symbols.Single(Item =>
                Item.Binding == Objectˉsymbolˉbinding.Export).Name);
    }

    private static void Assertˉcurrentˉhostˉstagingˉpublisher(
        ImmutableArray<byte> application,
        byte[] input,
        byte[] manifest,
        byte[][] chunks,
        int changedˉchunk,
        byte[] expectedˉobject,
        bool verifyˉfailureˉpaths = true)
    {
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-wvo-staging-publisher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Inputˉpath = Path.Combine(Directoryˉpath, "input.wvb");
            var Prefix = Path.Combine(Directoryˉpath, "object");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "object.wvop");
            var Destinationˉpath = Path.Combine(Directoryˉpath, "output.wvo");
            File.WriteAllBytes(Inputˉpath, input);
            File.WriteAllBytes(Manifestˉpath, manifest);
            Writeˉstagingˉchunks(Prefix, chunks);
            File.WriteAllBytes(Destinationˉpath, [9, 8, 7, 6]);
            var Loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Equal(
                0,
                Executeˉstagingˉpublisher(
                    application,
                    [Inputˉpath, Prefix, Manifestˉpath, Destinationˉpath],
                    Loaded));
            Sequenceˉequal(expectedˉobject, File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvo-*").Count());
            Equal(
                0,
                Loaded.Count(Name =>
                    Name.Contains("clr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
                    Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));

            if (!verifyˉfailureˉpaths)
            {
                return;
            }

            var Preserved = new byte[] { 4, 3, 2, 1 };
            var Changed = Cloneˉstagingˉchunks(chunks);
            Changed[changedˉchunk][0] ^= 0x01;
            Writeˉstagingˉchunks(Prefix, Changed);
            File.WriteAllBytes(Destinationˉpath, Preserved);
            Equal(
                1,
                Executeˉstagingˉpublisher(
                    application,
                    [Inputˉpath, Prefix, Manifestˉpath, Destinationˉpath],
                    expectedˉerror:
                        "native x64 staging admission content=Content\n"));
            Sequenceˉequal(Preserved, File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvo-*").Count());

            Writeˉstagingˉchunks(Prefix, chunks);
            File.Delete(Destinationˉpath);
            Createˉtestˉhardˉlink(Destinationˉpath, Inputˉpath);
            Equal(
                1,
                Executeˉstagingˉpublisher(
                    application,
                    [Inputˉpath, Prefix, Manifestˉpath, Destinationˉpath]));
            Sequenceˉequal(input, File.ReadAllBytes(Inputˉpath));
            Sequenceˉequal(input, File.ReadAllBytes(Destinationˉpath));
            Equal(0, Directory.EnumerateFiles(Directoryˉpath, ".wvo-*").Count());
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static int Executeˉstagingˉpublisher(
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

    private static void Writeˉstagingˉchunks(string prefix, byte[][] chunks)
    {
        for (var Index = 0; Index < chunks.Length; Index++)
        {
            File.WriteAllBytes($"{prefix}.chunk-{Index}", chunks[Index]);
        }
    }
}
