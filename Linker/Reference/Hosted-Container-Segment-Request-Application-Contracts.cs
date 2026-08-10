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
    public const int MODULE_BYTES = 45_109;
    public const string MODULE_SHA256 =
        "ae47ede7efb28af2a17c1c1530035e5fd9b1cc1f4d4ba416801b61d8f3d1da89";
    public const int WINDOWS_APPLICATION_BYTES = 526_848;
    public const string WINDOWS_APPLICATION_SHA256 =
        "b9477d3fb5206bb5df4a4e8ee4f48b081ef201d667edda99854cb857dd62c2d7";
    public const int LINUX_APPLICATION_BYTES = 528_384;
    public const string LINUX_APPLICATION_SHA256 =
        "34026f3f03bbda40ebf5be08f0ca0d2a9e9f1bf13abc29e0db6e81e837884f38";
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
