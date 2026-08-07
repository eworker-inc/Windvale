using System.Collections.Immutable;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Linuxˉhostedˉverifierˉapplicationˉlayout(
    int Applicationˉbytes,
    int Headerˉbytes,
    int Textˉfileˉoffset,
    uint Textˉaddress,
    int Startupˉbytes,
    int Bundleˉoffset,
    int Bundleˉbytes,
    uint Textˉbytes,
    uint Dataˉfileˉoffset,
    uint Dataˉaddress,
    uint Dataˉfileˉbytes,
    uint Dataˉvirtualˉbytes,
    uint Imageˉvirtualˉbytes);

internal sealed record Verifiedˉlinuxˉhostedˉverifierˉapplication(
    Linuxˉhostedˉverifierˉapplicationˉlayout Layout,
    uint Nativeˉentryˉoffset,
    ImmutableArray<byte> Bundleˉimage,
    Verifiedˉhostedˉverifierˉruntimeˉdata Runtime);

internal static class Linuxˉhostedˉverifierˉapplicationˉcontract
{
    internal const uint FORMAT_VERSION =
        Linuxˉconsoleˉapplicationˉcontract.VERIFIER_FORMAT_VERSION;
    internal const int HEADER_BYTES = 0x1000;
    internal const uint TEXT_ADDRESS = 0x1000;
    internal const int STARTUP_BYTES = Linuxˉhostedˉverifierˉstartup.BYTES;
    internal const int BUNDLE_TEXT_OFFSET = 0x1000;
    internal const uint DATA_FILE_BYTES = Hostedˉverifierˉruntimeˉdata.HEADER_BYTES;
    internal const int MAXIMUM_BUNDLE_BYTES = 64 * 1024 * 1024;

    internal static Linuxˉhostedˉverifierˉapplicationˉlayout Plan(
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        Validateˉbundle(bundle, profile);
        if (nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nativeˉentryˉoffset),
                "The Linux hosted-verifier entry is outside its native image.");
        }

        var Textˉbytes = checked((uint)(BUNDLE_TEXT_OFFSET + bundle.Imageˉbytes.Length));
        var Dataˉoffset = Alignˉup(checked(TEXT_ADDRESS + Textˉbytes), HEADER_BYTES);
        var Runtime = Hostedˉverifierˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Linuxˉx64,
            profile);
        return new(
            checked((int)(Dataˉoffset + DATA_FILE_BYTES)),
            HEADER_BYTES,
            HEADER_BYTES,
            TEXT_ADDRESS,
            Startupˉbytes(profile),
            BUNDLE_TEXT_OFFSET,
            bundle.Imageˉbytes.Length,
            Textˉbytes,
            Dataˉoffset,
            Dataˉoffset,
            DATA_FILE_BYTES,
            Runtime.Virtualˉbytes,
            checked(Dataˉoffset + Runtime.Virtualˉbytes));
    }

    internal static void Validateˉbundle(
        Nativeˉserviceˉbundle bundle,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        var Services = Hostedˉverifierˉapplicationˉmetadata.Requiredˉservices(profile);
        if (bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Linux ||
            bundle.Nativeˉimageˉbytes <= 0 ||
            bundle.Nativeˉimageˉbytes > bundle.Imageˉbytes.Length ||
            bundle.Imageˉbytes.Length > MAXIMUM_BUNDLE_BYTES ||
            bundle.Placements.Length != Services.Length ||
            !bundle.Placements.Select(Placement => Placement.Service).SequenceEqual(Services))
        {
            throw new ArgumentException("The Linux hosted-verifier bundle is invalid.");
        }
    }

    internal static int Startupˉbytes(Hostedˉverifierˉapplicationˉprofile profile) =>
        profile switch
        {
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier =>
                Linuxˉhostedˉverifierˉstartup.BYTES,
            Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector or
                Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner or
                Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector or
                Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier =>
                Linuxˉhostedˉinspectorˉstartup.BYTES,
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
        };

    private static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));
}
