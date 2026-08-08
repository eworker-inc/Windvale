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
    public const int MODULE_BYTES = 27_843;
    public const string MODULE_SHA256 =
        "2cd2311b9053abbe92f64d533d0681b6a5438c89a0548cad5ddc5a114c1b1917";
    public const int WINDOWS_APPLICATION_BYTES = 294_912;
    public const string WINDOWS_APPLICATION_SHA256 =
        "e7fe0939f62ce2403e3e24d1f4523dbb2e63c8fe469ee6930a039b1b66cc8576";
    public const int LINUX_APPLICATION_BYTES = 294_912;
    public const string LINUX_APPLICATION_SHA256 =
        "256304761afaa42da2df66a2f0e89303a4a00a282b95a235148a2633959d8e2c";
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
