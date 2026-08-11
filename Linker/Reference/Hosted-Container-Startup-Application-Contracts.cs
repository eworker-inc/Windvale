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
    public const int MODULE_BYTES = 43_716;
    public const string MODULE_SHA256 =
        "537afc1c6dba50b882f6b35cada70e9be4afccea5c1ef3eff1c82f925a010fd4";
    public const int WINDOWS_APPLICATION_BYTES = 381_440;
    public const string WINDOWS_APPLICATION_SHA256 =
        "23105912a93f627a95e7c1fc3db79aa57c324f2e90a29d1ea90dfc3eb4083ec2";
    public const int LINUX_APPLICATION_BYTES = 380_928;
    public const string LINUX_APPLICATION_SHA256 =
        "a9d44a7b613cdc76d07d79d4e8fe84e3373bbcde0960187b8c3da932043e01eb";
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
