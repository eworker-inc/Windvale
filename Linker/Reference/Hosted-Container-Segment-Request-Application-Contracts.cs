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
    public const int MODULE_BYTES = 42_788;
    public const string MODULE_SHA256 =
        "f6bb1b03922296916b9afcfbe29e6ba5ce09c557a3345052272c0e58dcdfef00";
    public const int WINDOWS_APPLICATION_BYTES = 512_000;
    public const string WINDOWS_APPLICATION_SHA256 =
        "4b9cf3e689f348d2791c1eb1add11d3064bf665040999905c1484dcf79fcfe52";
    public const int LINUX_APPLICATION_BYTES = 512_000;
    public const string LINUX_APPLICATION_SHA256 =
        "487da501b797bd7285b29c034d30df4bb933b3382d632a19ac7bf6bdfd17ddfd";
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
