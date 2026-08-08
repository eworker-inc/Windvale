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
    public const int MODULE_BYTES = 28_826;
    public const string MODULE_SHA256 =
        "9601b57c570b1cad2e14d72d815aeefda2de08a957790077aedbce438402e745";
    public const int WINDOWS_APPLICATION_BYTES = 382_976;
    public const string WINDOWS_APPLICATION_SHA256 =
        "988d390fe4d62cacd36ce810036553a3446d2cdfb9553a85337eeb03e2b53bb0";
    public const int LINUX_APPLICATION_BYTES = 385_024;
    public const string LINUX_APPLICATION_SHA256 =
        "0f666286fb8e1c8b6b0f45d0afe479134680412b5506d6bf5218c04fe8f59cb4";
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
