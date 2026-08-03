using System.Collections.Immutable;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Linuxˉhostedˉcompilerˉapplicationˉlayout(
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

internal sealed record Verifiedˉlinuxˉhostedˉcompilerˉapplication(
    Linuxˉhostedˉcompilerˉapplicationˉlayout Layout,
    uint Nativeˉentryˉoffset,
    ImmutableArray<byte> Bundleˉimage,
    Verifiedˉhostedˉcompilerˉruntimeˉdata Runtime);

internal static class Linuxˉhostedˉcompilerˉapplicationˉcontract
{
    internal const uint FORMAT_VERSION = 3;
    internal const int HEADER_BYTES = 0x1000;
    internal const uint TEXT_ADDRESS = 0x1000;
    internal const int STARTUP_BYTES = Linuxˉhostedˉcompilerˉstartup.BYTES;
    internal const int BUNDLE_TEXT_OFFSET = 0x1000;
    internal const uint DATA_FILE_BYTES = Hostedˉcompilerˉruntimeˉdata.HEADER_BYTES;
    internal const int MAXIMUM_BUNDLE_BYTES = 64 * 1024 * 1024;

    internal static Linuxˉhostedˉcompilerˉapplicationˉlayout Plan(
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉbundle(bundle);
        if (nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nativeˉentryˉoffset),
                "The Linux hosted-compiler entry is outside its native image.");
        }

        var Textˉbytes = checked((uint)(BUNDLE_TEXT_OFFSET + bundle.Imageˉbytes.Length));
        var Dataˉoffset = Alignˉup(checked(TEXT_ADDRESS + Textˉbytes), HEADER_BYTES);
        var Runtime = Hostedˉcompilerˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Linuxˉx64);
        return new(
            checked((int)(Dataˉoffset + DATA_FILE_BYTES)),
            HEADER_BYTES,
            HEADER_BYTES,
            TEXT_ADDRESS,
            STARTUP_BYTES,
            BUNDLE_TEXT_OFFSET,
            bundle.Imageˉbytes.Length,
            Textˉbytes,
            Dataˉoffset,
            Dataˉoffset,
            DATA_FILE_BYTES,
            Runtime.Virtualˉbytes,
            checked(Dataˉoffset + Runtime.Virtualˉbytes));
    }

    internal static void Validateˉbundle(Nativeˉserviceˉbundle bundle)
    {
        if (bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Linux ||
            bundle.Nativeˉimageˉbytes <= 0 ||
            bundle.Nativeˉimageˉbytes > bundle.Imageˉbytes.Length ||
            bundle.Imageˉbytes.Length > MAXIMUM_BUNDLE_BYTES ||
            bundle.Placements.Length != Hostedˉcompilerˉapplicationˉmetadata.SERVICE_COUNT)
        {
            throw new ArgumentException("The Linux hosted-compiler bundle is invalid.");
        }
    }

    private static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));
}
