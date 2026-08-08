using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int STAGING_PUBLISHER_WVO_BYTES = 6_355_569;
    private const string STAGING_PUBLISHER_WVO_SHA256 =
        "727e7da06f11340dcee4552f119de3422dee17968c49438906242bbf1166e7e5";

    private static void Nativeˉwvoˉstagingˉpublisherˉselfˉlowers()
    {
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
        var Publisherˉmoduleˉbytes =
            Compileˉwvbˉtoˉwvoˉapplicationˉsuccess(
                "Compiler/Windvale/Native-X64-Lowering-Staging-Admission-Tool.wv",
                NATIVE_X64_STAGING_ADMISSION_SOURCE,
                "staged-WVO publisher tool",
                includeˉpublication: true,
                includeˉstagingˉmanifest: true,
                includeˉstagingˉcontent: true,
                includeˉstagingˉresources: true);
        var Publisherˉmodule = Moduleˉcodec.Readˉandˉverify(
            Publisherˉmoduleˉbytes);
        var Publisherˉnative = X64ˉnativeˉbackend.Compile(Publisherˉmodule);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(
            Publisherˉnative.Fragment,
            Objectˉadmissionˉprofile.Largeˉnative);
        Equal(STAGING_PUBLISHER_WVO_BYTES, Expectedˉobject.Length);
        Equal(
            STAGING_PUBLISHER_WVO_SHA256,
            Objectˉdigest.Calculateˉsha256(Expectedˉobject.AsSpan()));

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
        True(Windowsˉpublisher.Success,
            string.Join(" | ", Windowsˉpublisher.Diagnostics));
        True(Linuxˉpublisher.Success,
            string.Join(" | ", Linuxˉpublisher.Diagnostics));

        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-wvo-staging-self-lowering-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Inputˉpath = Path.Combine(Directoryˉpath, "publisher.wvb");
            var Prefix = Path.Combine(Directoryˉpath, "publisher");
            var Manifestˉpath = Path.Combine(
                Directoryˉpath,
                "publisher.wvop");
            var Destinationˉpath = Path.Combine(
                Directoryˉpath,
                "publisher.wvo");
            File.WriteAllBytes(Inputˉpath, Publisherˉmoduleˉbytes);
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
                    expectedˉoutput: null,
                    timeoutˉmilliseconds: 600_000));
            var Producedˉchunks = Directory.EnumerateFiles(
                Directoryˉpath,
                "publisher.chunk-*").Count();
            True(
                Producedˉchunks <= 62,
                $"Publisher self-lowering produced {Producedˉchunks} chunks.");

            var Publisher = OperatingSystem.IsWindows()
                ? Windowsˉpublisher.Imageˉbytes
                : Linuxˉpublisher.Imageˉbytes;
            Equal(
                0,
                Executeˉwvoˉstagingˉapplication(
                    Publisher,
                    [Inputˉpath, Prefix, Manifestˉpath, Destinationˉpath],
                    Loaded,
                    timeoutˉmilliseconds: 600_000));
            Requireˉnoˉdotnetˉmodules(Loaded);

            var Publishedˉobject = File.ReadAllBytes(Destinationˉpath);
            Equal(Expectedˉobject.Length, Publishedˉobject.Length);
            Equal(
                STAGING_PUBLISHER_WVO_SHA256,
                Objectˉdigest.Calculateˉsha256(Publishedˉobject));
            _ = Objectˉcodec.Readˉandˉverify(
                Publishedˉobject,
                Objectˉadmissionˉprofile.Largeˉnative);
            True(
                Directory.EnumerateFiles(
                    Directoryˉpath,
                    "publisher.chunk-*").Skip(1).Any(),
                "Publisher self-lowering did not cross a staging chunk boundary.");
            Equal(0, Directory.EnumerateFiles(
                Directoryˉpath,
                ".wvo-*").Count());
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

}
