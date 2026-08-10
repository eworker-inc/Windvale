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
    public const int MODULE_BYTES = 35_419;
    public const string MODULE_SHA256 =
        "4553a80bb425c0f79fd20c52ed41745279b5c9313f0bf669d5e749f62d6de33c";
    public const int WINDOWS_APPLICATION_BYTES = 409_600;
    public const string WINDOWS_APPLICATION_SHA256 =
        "78d0ef5eb0ba798359d663a4cfcd108677ecb10fb068656c038d8f8096133710";
    public const int LINUX_APPLICATION_BYTES = 409_600;
    public const string LINUX_APPLICATION_SHA256 =
        "d289d1c649ab58e84f34640eb36f599b60badf2883f8325359ae12be965c6603";
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
