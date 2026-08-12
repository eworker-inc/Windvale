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
    public const int MODULE_BYTES = 43_902;
    public const string MODULE_SHA256 =
        "f01ea2c4f851350ac70faf0be690d9695acb946fd9138b4f9577c57ea12b8598";
    public const int WINDOWS_APPLICATION_BYTES = 382_464;
    public const string WINDOWS_APPLICATION_SHA256 =
        "ebc3acd6cbfae0b473d74f18eb32a3dd873891400e557ef882312fa01dd13103";
    public const int LINUX_APPLICATION_BYTES = 385_024;
    public const string LINUX_APPLICATION_SHA256 =
        "3d0c204fa8c8fc2b48ade1e1f4126d687707d061d7b06a5601629df57539c0d2";
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
