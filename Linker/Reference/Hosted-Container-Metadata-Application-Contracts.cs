using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉmetadataˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-metadata-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-metadata-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉmetadataˉtool";
    public const int MODULE_BYTES = 26_748;
    public const string MODULE_SHA256 =
        "196c233ec549872204c5fcfa1c8fc275dba7ff339264de428be7ce72621a2333";
    public const int WINDOWS_APPLICATION_BYTES = 252_928;
    public const string WINDOWS_APPLICATION_SHA256 =
        "f4cb8689757f1c93c8da77fa109bcb7d0e0bfd9148de54ee7c88aa03f456955e";
    public const int LINUX_APPLICATION_BYTES = 253_952;
    public const string LINUX_APPLICATION_SHA256 =
        "d95e843f862f20c2b027cd7b335b8ccb683a2589b14818027cb6eabd689a782a";
}

public static class Hostedˉcontainerˉmetadataˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment,
            capabilities,
            moduleˉname,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-metadata",
            "WVW2291");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container metadata tool",
            "WVW2291");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment,
            capabilities,
            moduleˉname,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-metadata",
            "WVL2291");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container metadata tool",
            "WVL2291");
    }
}
