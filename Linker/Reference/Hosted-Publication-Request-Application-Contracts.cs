using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉpublicationˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-publication-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-publication-request-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉpublicationˉrequestˉtool";
    public const int MODULE_BYTES = 22_067;
    public const string MODULE_SHA256 =
        "7d525451a92d2f0969e5c9006b43f16cd5485fe7791526e4769a920ec01ad430";
    public const int WINDOWS_APPLICATION_BYTES = 240_640;
    public const string WINDOWS_APPLICATION_SHA256 =
        "18dd309f7df8009de66f7a0a11649561443774b3569e5a1f34da5d736e203759";
    public const int LINUX_APPLICATION_BYTES = 241_664;
    public const string LINUX_APPLICATION_SHA256 =
        "d2e816c7d1cb09856442173d0451bbebbdd6a393047c91f9ceca354ef2e00902";
}

public static class Hostedˉpublicationˉrequestˉapplicationˉwriter
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
            Hostedˉpublicationˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "publication-request",
            "WVW2801");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉpublicationˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉpublicationˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted publication-request tool",
            "WVW2801");
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
            Hostedˉpublicationˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "publication-request",
            "WVL2801");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉpublicationˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉpublicationˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted publication-request tool",
            "WVL2801");
    }
}
