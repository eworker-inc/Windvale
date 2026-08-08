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
    public const int MODULE_BYTES = 37_289;
    public const string MODULE_SHA256 =
        "81cf3932c5e1d4f711b779c515a718ec1acd32c09ae17031aa63b8a66f5ce788";
    public const int WINDOWS_APPLICATION_BYTES = 584_704;
    public const string WINDOWS_APPLICATION_SHA256 =
        "e401ad5aef792a49be72cf711cfc427a859fe4a534aa780ad47d3b4a2c12a5dc";
    public const int LINUX_APPLICATION_BYTES = 585_728;
    public const string LINUX_APPLICATION_SHA256 =
        "8032370c7391bbc6afa94c1e8804db78f682da4e57144a2907394e202806c0d3";
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
