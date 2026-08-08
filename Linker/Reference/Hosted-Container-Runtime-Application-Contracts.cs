using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉruntimeˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-runtime-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-runtime-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉruntimeˉtool";
    public const int MODULE_BYTES = 22_956;
    public const string MODULE_SHA256 =
        "be7db77c3171c042ab2a740eb9b3e7492d5624d50e35625b9ad07015f5c013e3";
    public const int WINDOWS_APPLICATION_BYTES = 244_736;
    public const string WINDOWS_APPLICATION_SHA256 =
        "b1a653d4fa00bdfd4964e8a2911317b25801484f06462a2d11572d481c3cb198";
    public const int LINUX_APPLICATION_BYTES = 245_760;
    public const string LINUX_APPLICATION_SHA256 =
        "ca0e23a717b9252b40847e7c976d64178252678db47f0472b6e958186a8466cc";
}

public static class Hostedˉcontainerˉruntimeˉapplicationˉwriter
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
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-runtime",
            "WVW2281");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container runtime tool",
            "WVW2281");
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
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-runtime",
            "WVL2281");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container runtime tool",
            "WVL2281");
    }
}
