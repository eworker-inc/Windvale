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
    public const int MODULE_BYTES = 39_483;
    public const string MODULE_SHA256 =
        "9ea136c44f76d9a53474d81e989f38cf15ffa8dc267638e2224e20f733205e6b";
    public const int WINDOWS_APPLICATION_BYTES = 646_656;
    public const string WINDOWS_APPLICATION_SHA256 =
        "e69a9a4c0a33d4bc05d98e5c977ba537715081d08d8c8493da87990989547af3";
    public const int LINUX_APPLICATION_BYTES = 647_168;
    public const string LINUX_APPLICATION_SHA256 =
        "eb1ba149a94741801732e53773f566aee752a19510d22f8e2768def7614d396c";
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
