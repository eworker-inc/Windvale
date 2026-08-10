using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉstartupˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-startup-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-startup-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉstartupˉtool";
    public const int MODULE_BYTES = 42_508;
    public const string MODULE_SHA256 =
        "7c68e998940600ecb56534e05635510643ba1fd218bcd3fca9e23300e1380807";
    public const int WINDOWS_APPLICATION_BYTES = 373_248;
    public const string WINDOWS_APPLICATION_SHA256 =
        "ec1fecf3c05b130554537be6d03a064e75c833ded55677abcbde0c13d0264b3e";
    public const int LINUX_APPLICATION_BYTES = 372_736;
    public const string LINUX_APPLICATION_SHA256 =
        "6a58768a3aa137ffd1ba49f318fe0f129a5d7430a5d06d8126af31b4411f5e94";
}

public static class Hostedˉcontainerˉstartupˉapplicationˉwriter
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
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-startup",
            "WVW2271");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container startup tool",
            "WVW2271");
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
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-startup",
            "WVL2271");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container startup tool",
            "WVL2271");
    }
}
