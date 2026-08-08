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
    public const int MODULE_BYTES = 54_135;
    public const string MODULE_SHA256 =
        "db433d551ac3530c8b9c36e8bf035177181c3d403912030ef9fd5bba37698034";
    public const int WINDOWS_APPLICATION_BYTES = 782_848;
    public const string WINDOWS_APPLICATION_SHA256 =
        "73fac9bc9d023f9ad4dca1f8c7fbcad899b26a92227f4ca32eaae6eeb36a5596";
    public const int LINUX_APPLICATION_BYTES = 782_336;
    public const string LINUX_APPLICATION_SHA256 =
        "86fc9a3860b68eabe8500ba0256c5d01dbf6918baed3fbc4e3711c6670258443";
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
