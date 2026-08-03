using System.Collections.Immutable;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Windowsˉhostedˉcompilerˉapplicationˉlayout(
    int Applicationˉbytes,
    int Headerˉbytes,
    int Textˉfileˉoffset,
    uint Textˉaddress,
    int Startupˉbytes,
    int Bundleˉoffset,
    int Bundleˉbytes,
    uint Textˉvirtualˉbytes,
    uint Textˉfileˉbytes,
    uint Dataˉfileˉoffset,
    uint Dataˉsectionˉaddress,
    uint Dataˉfileˉbytes,
    uint Dataˉvirtualˉbytes,
    uint Importˉfileˉoffset,
    uint Importˉaddress,
    uint Runtimeˉfileˉoffset,
    uint Runtimeˉaddress,
    uint Runtimeˉfileˉbytes,
    uint Runtimeˉvirtualˉbytes,
    uint Relocationˉfileˉoffset,
    uint Relocationˉaddress,
    uint Imageˉvirtualˉbytes);

internal sealed record Verifiedˉwindowsˉhostedˉcompilerˉapplication(
    Windowsˉhostedˉcompilerˉapplicationˉlayout Layout,
    uint Nativeˉentryˉoffset,
    ImmutableArray<byte> Bundleˉimage,
    Verifiedˉhostedˉcompilerˉruntimeˉdata Runtime);

internal static class Windowsˉhostedˉcompilerˉapplicationˉcontract
{
    internal const byte FORMAT_VERSION = 3;
    internal const int HEADER_BYTES = 0x200;
    internal const uint TEXT_ADDRESS = 0x1000;
    internal const int STARTUP_BYTES = Windowsˉhostedˉcompilerˉstartup.BYTES;
    internal const int BUNDLE_TEXT_OFFSET = 0x1000;
    internal const uint IMPORT_FILE_BYTES = Windowsˉhostedˉcompilerˉimports.PAGE_BYTES;
    internal const uint RUNTIME_FILE_BYTES = Hostedˉcompilerˉruntimeˉdata.HEADER_BYTES;
    internal const uint DATA_FILE_BYTES = IMPORT_FILE_BYTES + RUNTIME_FILE_BYTES;
    internal const uint RELOCATION_BYTES = 12;
    internal const uint RELOCATION_FILE_BYTES = 0x200;
    internal const int MAXIMUM_BUNDLE_BYTES = 64 * 1024 * 1024;
    internal const ulong IMAGE_BASE = 0x0000_0001_4000_0000;

    internal static Windowsˉhostedˉcompilerˉapplicationˉlayout Plan(
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉbundle(bundle);
        if (nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nativeˉentryˉoffset),
                "The Windows hosted-compiler entry is outside its native image.");
        }

        var Textˉvirtual = checked((uint)(BUNDLE_TEXT_OFFSET + bundle.Imageˉbytes.Length));
        var Textˉfile = Alignˉup(Textˉvirtual, 0x200);
        var Dataˉfile = checked((uint)HEADER_BYTES + Textˉfile);
        var Dataˉsection = Alignˉup(checked(TEXT_ADDRESS + Textˉvirtual), 0x1000);
        var Runtime = Hostedˉcompilerˉruntimeˉdata.Plan(
            Consoleˉapplicationˉtarget.Windowsˉx64);
        var Dataˉvirtual = checked(IMPORT_FILE_BYTES + Runtime.Virtualˉbytes);
        var Relocationˉfile = checked(Dataˉfile + DATA_FILE_BYTES);
        var Relocationˉaddress = Alignˉup(checked(Dataˉsection + Dataˉvirtual), 0x1000);
        return new(
            checked((int)(Relocationˉfile + RELOCATION_FILE_BYTES)),
            HEADER_BYTES,
            HEADER_BYTES,
            TEXT_ADDRESS,
            STARTUP_BYTES,
            BUNDLE_TEXT_OFFSET,
            bundle.Imageˉbytes.Length,
            Textˉvirtual,
            Textˉfile,
            Dataˉfile,
            Dataˉsection,
            DATA_FILE_BYTES,
            Dataˉvirtual,
            Dataˉfile,
            Dataˉsection,
            checked(Dataˉfile + IMPORT_FILE_BYTES),
            checked(Dataˉsection + IMPORT_FILE_BYTES),
            RUNTIME_FILE_BYTES,
            Runtime.Virtualˉbytes,
            Relocationˉfile,
            Relocationˉaddress,
            Alignˉup(checked(Relocationˉaddress + RELOCATION_BYTES), 0x1000));
    }

    internal static void Validateˉbundle(Nativeˉserviceˉbundle bundle)
    {
        if (bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Windows ||
            bundle.Nativeˉimageˉbytes <= 0 ||
            bundle.Nativeˉimageˉbytes > bundle.Imageˉbytes.Length ||
            bundle.Imageˉbytes.Length > MAXIMUM_BUNDLE_BYTES ||
            bundle.Placements.Length != Hostedˉcompilerˉapplicationˉmetadata.SERVICE_COUNT)
        {
            throw new ArgumentException("The Windows hosted-compiler bundle is invalid.");
        }
    }

    private static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));
}
