using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-service-bundle-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-service-bundle-request-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉserviceˉbundleˉrequestˉtool";
    public const int MODULE_BYTES = 29_070;
    public const string MODULE_SHA256 =
        "f79852fc85b87b4484596b7aa6a41efac2365edeb3f933b32fe12797f19e43e2";
    public const int WINDOWS_APPLICATION_BYTES = 302_080;
    public const string WINDOWS_APPLICATION_SHA256 =
        "b3c7db2f5721beee13473462ce49313c41e2e6f08f98a37ce0fee6139c1810bc";
    public const int LINUX_APPLICATION_BYTES = 303_104;
    public const string LINUX_APPLICATION_SHA256 =
        "e7e90cfc824bcd345f28edbd432d4a3826fa6a21ba7a7818904de4fc90c51371";
}

public static class Hostedˉserviceˉbundleˉrequestˉapplicationˉwriter
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
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "service-bundle-request",
            "WVW2601");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted service-bundle-request tool",
            "WVW2601");
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
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "service-bundle-request",
            "WVL2601");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted service-bundle-request tool",
            "WVL2601");
    }
}
