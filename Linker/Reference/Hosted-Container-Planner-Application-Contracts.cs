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
    public const int MODULE_BYTES = 39_162;
    public const string MODULE_SHA256 =
        "6dc9cbd852698f158cb2b445e2d7c33537b414ef3c1c6198e9bd6366aed42897";
    public const int WINDOWS_APPLICATION_BYTES = 606_720;
    public const string WINDOWS_APPLICATION_SHA256 =
        "ef39d922eacd00a3300c2796e1271e1ef1731ec943145f0af6f2ed62c8411ab5";
    public const int LINUX_APPLICATION_BYTES = 606_208;
    public const string LINUX_APPLICATION_SHA256 =
        "7a54190410317628d9b58b5e2c3b91ac4bfd71c6ec192e95409491687a52b79f";
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
