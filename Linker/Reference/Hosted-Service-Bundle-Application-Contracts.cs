using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉserviceˉbundleˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-service-bundle-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-service-bundle-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉserviceˉbundleˉtool";
    public const int MODULE_BYTES = 20_144;
    public const string MODULE_SHA256 =
        "2284d3896b013bd81ad75ff9de658a07fa4ae0f7ad6d7522e4cdf2abf36917ec";
    public const int WINDOWS_APPLICATION_BYTES = 220_672;
    public const string WINDOWS_APPLICATION_SHA256 =
        "f585015f927d1d06de964994324f0b5dad7b32f3b3a72b306a1f55fe67268e05";
    public const int LINUX_APPLICATION_BYTES = 221_184;
    public const string LINUX_APPLICATION_SHA256 =
        "1b6f9ec568056c0f7ded939f64f0635e74970252f61b30415c5141227b52b0a8";
}

public static class Hostedˉserviceˉbundleˉapplicationˉwriter
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
            Hostedˉserviceˉbundleˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "service-bundle",
            "WVW2301");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉserviceˉbundleˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉserviceˉbundleˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted service-bundle tool",
            "WVW2301");
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
            Hostedˉserviceˉbundleˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "service-bundle",
            "WVL2301");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉserviceˉbundleˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉserviceˉbundleˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted service-bundle tool",
            "WVL2301");
    }
}
