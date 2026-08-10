using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-segment-manifest-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-segment-manifest-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉsegmentˉmanifestˉtool";
    public const int MODULE_BYTES = 35_605;
    public const string MODULE_SHA256 =
        "8fd1c3f0537694189928f2e78745179519546e91c3df4325fccc451bfebc3133";
    public const int WINDOWS_APPLICATION_BYTES = 411_136;
    public const string WINDOWS_APPLICATION_SHA256 =
        "a63152d9ff108b1e4ccddefbce028a64b43c1b6618efbd288bd07734e39a0ac6";
    public const int LINUX_APPLICATION_BYTES = 409_600;
    public const string LINUX_APPLICATION_SHA256 =
        "6225cfaab695f281a563b47fe39f1bd2c178c463504951d45c7e7cf73d09c828";
}

public static class Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-segment-manifest", "WVW2912");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted container segment-manifest tool", "WVW2912");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-segment-manifest", "WVL2912");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉsegmentˉmanifestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted container segment-manifest tool", "WVL2912");
    }
}
