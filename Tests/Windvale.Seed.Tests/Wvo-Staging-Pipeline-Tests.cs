using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvoˉstagingˉpipelineˉruns()
    {
        var Help = Executeˉinspectorˉtool("help");
        Equal(0, Help.Exitˉcode);
        Equal(string.Empty, Help.Standardˉerror);
        Contains(
            Help.Standardˉoutput,
            Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_TARGET_NAME);
        Contains(
            Help.Standardˉoutput,
            Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_TARGET_NAME);

        var Producerˉmoduleˉbytes = Compileˉwvbˉtoˉwvoˉstagingˉsuccess();
        var Producerˉmodule = Moduleˉcodec.Readˉandˉverify(
            Producerˉmoduleˉbytes);
        var Producerˉnative = X64ˉnativeˉbackend.Compile(Producerˉmodule);
        var Windowsˉproducer =
            Wvoˉstagingˉproducerˉapplicationˉwriter.Writeˉwindows(
                Producerˉmodule,
                Producerˉnative.Fragment,
                Producerˉmoduleˉbytes);
        var Linuxˉproducer =
            Wvoˉstagingˉproducerˉapplicationˉwriter.Writeˉlinux(
                Producerˉmodule,
                Producerˉnative.Fragment,
                Producerˉmoduleˉbytes);
        Requireˉstagingˉproducerˉapplication(
            Windowsˉproducer.Success,
            string.Join(" | ", Windowsˉproducer.Diagnostics),
            Windowsˉproducer.Imageˉbytes,
            Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "Windows");
        Requireˉstagingˉproducerˉapplication(
            Linuxˉproducer.Success,
            string.Join(" | ", Linuxˉproducer.Diagnostics),
            Linuxˉproducer.Imageˉbytes,
            Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "Linux");

        var Publisherˉmoduleˉbytes =
            Compileˉwvbˉtoˉwvoˉapplicationˉsuccess(
                "Compiler/Windvale/Native-X64-Lowering-Staging-Admission-Tool.wv",
                NATIVE_X64_STAGING_ADMISSION_SOURCE,
                "staged-WVO publisher tool",
                includeˉpublication: true,
                includeˉstagingˉmanifest: true,
                includeˉstagingˉcontent: true,
                includeˉstagingˉresources: true,
                includeˉpublicationˉtransaction: true);
        var Publisherˉmodule = Moduleˉcodec.Readˉandˉverify(
            Publisherˉmoduleˉbytes);
        var Publisherˉnative = X64ˉnativeˉbackend.Compile(Publisherˉmodule);
        var Windowsˉpublisher =
            Wvoˉstagingˉpublisherˉapplicationˉwriter.Writeˉwindows(
                Publisherˉmodule,
                Publisherˉnative.Fragment,
                Publisherˉmoduleˉbytes);
        var Linuxˉpublisher =
            Wvoˉstagingˉpublisherˉapplicationˉwriter.Writeˉlinux(
                Publisherˉmodule,
                Publisherˉnative.Fragment,
                Publisherˉmoduleˉbytes);
        True(Windowsˉpublisher.Success,
            string.Join(" | ", Windowsˉpublisher.Diagnostics));
        True(Linuxˉpublisher.Success,
            string.Join(" | ", Linuxˉpublisher.Diagnostics));

        var Input = Compileˉsuccess(WVB_TO_WVO_RETURN_42_SOURCE);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            X64ˉnativeˉbackend.Compile(
                Moduleˉcodec.Readˉandˉverify(Input)).Fragment);
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-wvo-staging-pipeline-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Inputˉpath = Path.Combine(Directoryˉpath, "input.wvb");
            var Prefix = Path.Combine(Directoryˉpath, "object");
            var Manifestˉpath = Path.Combine(Directoryˉpath, "object.wvop");
            var Destinationˉpath = Path.Combine(Directoryˉpath, "output.wvo");
            File.WriteAllBytes(Inputˉpath, Input);
            File.WriteAllBytes(Destinationˉpath, [9, 8, 7, 6]);

            var Loaded = new HashSet<string>(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);
            var Producer = OperatingSystem.IsWindows()
                ? Windowsˉproducer.Imageˉbytes
                : Linuxˉproducer.Imageˉbytes;
            Equal(
                0,
                Executeˉwvoˉstagingˉapplication(
                    Producer,
                    [Inputˉpath, Prefix, Manifestˉpath],
                    Loaded,
                    $"native x64 staging status=Complete " +
                    $"object-bytes={Expectedˉobject.Length} chunks=3 " +
                    "manifest-bytes=60\n"));

            var Publisher = OperatingSystem.IsWindows()
                ? Windowsˉpublisher.Imageˉbytes
                : Linuxˉpublisher.Imageˉbytes;
            Equal(
                0,
                Executeˉwvoˉstagingˉapplication(
                    Publisher,
                    [Inputˉpath, Prefix, Manifestˉpath, Destinationˉpath],
                    Loaded));
            Requireˉnoˉdotnetˉmodules(Loaded);
            Sequenceˉequal(
                Expectedˉobject,
                File.ReadAllBytes(Destinationˉpath));
            _ = Objectˉcodec.Readˉandˉverify(
                File.ReadAllBytes(Destinationˉpath));
            Equal(3, Directory.EnumerateFiles(
                Directoryˉpath,
                "object.chunk-*").Count());
            Equal(0, Directory.EnumerateFiles(
                Directoryˉpath,
                ".wvo-*").Count());
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static void Requireˉstagingˉproducerˉapplication(
        bool success,
        string diagnostics,
        ImmutableArray<byte> image,
        int expectedˉbytes,
        string expectedˉsha256,
        string platform)
    {
        True(success,
            $"The {platform} staged-WVO producer package was rejected: " +
                diagnostics);
        Equal(expectedˉbytes, image.Length);
        Equal(expectedˉsha256,
            Objectˉdigest.Calculateˉsha256(image.AsSpan()));
    }

    private static int Executeˉwvoˉstagingˉapplication(
        ImmutableArray<byte> application,
        IReadOnlyList<string> arguments,
        ISet<string> loaded,
        string? expectedˉoutput = "",
        int timeoutˉmilliseconds = 60_000) =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                arguments,
                timeoutˉmilliseconds: timeoutˉmilliseconds,
                loadedˉmodules: loaded)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                arguments,
                timeoutˉmilliseconds: timeoutˉmilliseconds,
                loadedˉmappings: loaded);
}
