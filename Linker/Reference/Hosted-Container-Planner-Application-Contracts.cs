using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉplannerˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-planner-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-planner-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉcontainerˉplannerˉtool";
    public const int MODULE_BYTES = 39_534;
    public const string MODULE_SHA256 =
        "e9c6eaa87574e2ee472dcc8c177bfe4d63baf9c34838894096f490bdde465bc0";
    public const int WINDOWS_APPLICATION_BYTES = 609_280;
    public const string WINDOWS_APPLICATION_SHA256 =
        "fbbbb70c5fa91b9f41551b25ed991dcc489aa92bf44ab0802a6307313bc8d064";
    public const int LINUX_APPLICATION_BYTES = 610_304;
    public const string LINUX_APPLICATION_SHA256 =
        "edb8f6bf257215309fdad2eee4a18a67e58d2cb8eb9605cbb2067fad3d18ccc9";
}

public static class Hostedˉcontainerˉplannerˉapplicationˉwriter
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
            Hostedˉcontainerˉplannerˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-planner",
            "WVW2251");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container planner",
            "WVW2251");
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
            Hostedˉcontainerˉplannerˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-planner",
            "WVL2251");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉplannerˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container planner",
            "WVL2251");
    }
}
