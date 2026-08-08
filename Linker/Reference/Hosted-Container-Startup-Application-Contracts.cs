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
        "ae1401613548724f35f40699249963cf7e0d04cbffbb8b4a0459a7e0d493003e";
    public const int WINDOWS_APPLICATION_BYTES = 373_248;
    public const string WINDOWS_APPLICATION_SHA256 =
        "d4ba697dc124d79ed25dbb60ba17bdf84cb7a2c6650296901b99b1cb67d02929";
    public const int LINUX_APPLICATION_BYTES = 372_736;
    public const string LINUX_APPLICATION_SHA256 =
        "43eb89b3d30cd0760a492ea6431dc736807ef322158beda20501d7f1180adc48";
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
