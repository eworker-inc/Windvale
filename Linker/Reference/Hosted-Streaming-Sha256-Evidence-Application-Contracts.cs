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
    public const int MODULE_BYTES = 40_261;
    public const string MODULE_SHA256 =
        "1b15d4640027d415e9e8d6de9d22b04eaed7cc3c4b8d27ddd84d17fb69cda104";
    public const int WINDOWS_APPLICATION_BYTES = 664_576;
    public const string WINDOWS_APPLICATION_SHA256 =
        "4c9761003e6ff2b3040a1197762d50a603768b7b7f170bfd5e30d6cb4f939be5";
    public const int LINUX_APPLICATION_BYTES = 663_552;
    public const string LINUX_APPLICATION_SHA256 =
        "d1a5c87df95f8881c380680c7de965fdc051909691757974e68165adc83c31a1";
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
