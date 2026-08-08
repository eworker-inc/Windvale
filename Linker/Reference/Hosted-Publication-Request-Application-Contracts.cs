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
        "6d382e6d3a1442fdbf0cf46ff6cc52aabfd1bd6fed86171775d8acc1fdeef0b1";
    public const int LINUX_APPLICATION_BYTES = 241_664;
    public const string LINUX_APPLICATION_SHA256 =
        "7a3c97a9e8abc36accc54e94a7abe968486ac679dd5a34b5f18b86a68ab2dd15";
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
