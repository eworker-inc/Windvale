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
    public const int MODULE_BYTES = 68_641;
    public const string MODULE_SHA256 =
        "c90fc5f817454a48c76b476d68fc4460426ba3bce9a787114b693600c4dbe784";
    public const int WINDOWS_APPLICATION_BYTES = 1_100_800;
    public const string WINDOWS_APPLICATION_SHA256 =
        "97b3cbe4ba6cbcd72d625050a8755bdae2fa3fd3e7f7d4df869cff55260b11a8";
    public const int LINUX_APPLICATION_BYTES = 1_101_824;
    public const string LINUX_APPLICATION_SHA256 =
        "622f0a92a956e6451535f0d09d0328d98430c055a3e06fd7d8e506fd40efb432";
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
