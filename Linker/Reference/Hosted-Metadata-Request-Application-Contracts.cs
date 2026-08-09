using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉmetadataˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-metadata-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-metadata-request-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉtoolˉmetadataˉrequestˉtool";
    public const int MODULE_BYTES = 63_278;
    public const string MODULE_SHA256 =
        "55edb3633ee13f4ed7b02781e469c2d0325d8a0a8e274658a3bb06cc580bac04";
    public const int WINDOWS_APPLICATION_BYTES = 1_052_672;
    public const string WINDOWS_APPLICATION_SHA256 =
        "4d1d5c114f9b022e594dd7d4abef2408143f9de60e4fa4bb00810316b5557366";
    public const int LINUX_APPLICATION_BYTES = 1_052_672;
    public const string LINUX_APPLICATION_SHA256 =
        "8a4fb176439e2b71f98c244a98c04deec7985453038f3b2813de6fd6e179d4dd";
}

public static class Hostedˉmetadataˉrequestˉapplicationˉwriter
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
            Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "metadata-request",
            "WVW2501");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted metadata-request tool",
            "WVW2501");
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
            Hostedˉmetadataˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "metadata-request",
            "WVL2501");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉmetadataˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉmetadataˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted metadata-request tool",
            "WVL2501");
    }
}
