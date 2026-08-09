using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-streaming-sha256-evidence-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-streaming-sha256-evidence-v1";
    public const string MODULE_NAME =
        "Nativeˉstreamingˉsha256ˉevidenceˉtool";
    public const int MODULE_BYTES = 48_364;
    public const string MODULE_SHA256 =
        "95a112cc469c7667e8158cd57770a806501ede1bdea9a82a797b770b9e59dea4";
    public const int WINDOWS_APPLICATION_BYTES = 914_432;
    public const string WINDOWS_APPLICATION_SHA256 =
        "16719d10c539c8950b620c7eee73e23d82a915b5e395977da9eabdf88e18e9a9";
    public const int LINUX_APPLICATION_BYTES = 913_408;
    public const string LINUX_APPLICATION_SHA256 =
        "e0383452a56712748a17ffbe1f780817c338bf1900d3ef914706604ce592b6ea";
}

public static class Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉwriter
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
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "streaming-sha256-evidence",
            "WVW2401");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted streaming SHA-256 evidence tool",
            "WVW2401");
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
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "streaming-sha256-evidence",
            "WVL2401");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉstreamingˉsha256ˉevidenceˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted streaming SHA-256 evidence tool",
            "WVL2401");
    }
}
