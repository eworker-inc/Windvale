using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉenumˉserviceˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-enum-service-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-enum-service-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉenumˉserviceˉtool";
    public const int MODULE_BYTES = 18_976;
    public const string MODULE_SHA256 =
        "493226f5b61894cb43e3428555e96293310c03571f6cff905eb50fabc7721676";
    public const int WINDOWS_APPLICATION_BYTES = 185_344;
    public const string WINDOWS_APPLICATION_SHA256 =
        "61d8b79ea57082c2ea85de5057a66e7c10045c44a9b8997d2ed491f3a1d90a83";
    public const int LINUX_APPLICATION_BYTES = 184_320;
    public const string LINUX_APPLICATION_SHA256 =
        "cd6f3b01df9a57bfe1acf2fa226c58f10c8ba51d2096a75572628cfbea427cf0";
}

public static class Hostedˉenumˉserviceˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉenumˉserviceˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "enum-service", "WVW3101");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted enum-service tool", "WVW3101");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉenumˉserviceˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "enum-service", "WVL3101");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted enum-service tool", "WVL3101");
    }
}
