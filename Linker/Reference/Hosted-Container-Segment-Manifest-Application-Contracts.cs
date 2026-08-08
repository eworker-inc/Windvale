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
    public const int MODULE_BYTES = 34_853;
    public const string MODULE_SHA256 =
        "28299931809e61bb80848e28e0621b670df2d13330f284dc77dac843b0138049";
    public const int WINDOWS_APPLICATION_BYTES = 406_016;
    public const string WINDOWS_APPLICATION_SHA256 =
        "ff8028aebdaeda1c305225f2d6c3883d22af3ab5bd440e71b50837e4400c334f";
    public const int LINUX_APPLICATION_BYTES = 405_504;
    public const string LINUX_APPLICATION_SHA256 =
        "48a60917b0693457441c15c121bf42e489067b8e69356f355dba5ec184ad533e";
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
