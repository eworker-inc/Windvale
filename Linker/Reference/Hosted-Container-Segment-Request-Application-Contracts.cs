using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-segment-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-segment-request-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉsegmentˉrequestˉtool";
    public const int MODULE_BYTES = 44_543;
    public const string MODULE_SHA256 =
        "09802f31927bc3120476001ff3733c15dcf3072537c109f3b044b170cee8b27f";
    public const int WINDOWS_APPLICATION_BYTES = 523_264;
    public const string WINDOWS_APPLICATION_SHA256 =
        "f9c2236f747d3737567681dacf5335ea06e412186255c3cd205bc45b7b6f42e6";
    public const int LINUX_APPLICATION_BYTES = 524_288;
    public const string LINUX_APPLICATION_SHA256 =
        "f53bdff4f3e00c373a4f82c43266bada4f699d3776a4657e1de4558ea0b5dd2f";
}

public static class Hostedˉcontainerˉsegmentˉrequestˉapplicationˉwriter
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
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-segment-request",
            "WVW2701");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted container-segment-request tool",
            "WVW2701");
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
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-segment-request",
            "WVL2701");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted container-segment-request tool",
            "WVL2701");
    }
}
