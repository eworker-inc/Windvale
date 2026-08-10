using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-verifier-metadata-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-verifier-metadata-request-v1";
    public const string MODULE_NAME =
        "Runtimeˉnativeˉhostedˉverifierˉmetadataˉrequestˉtool";
    public const int MODULE_BYTES = 18_999;
    public const string MODULE_SHA256 =
        "db39724575c5984be646725fad5857f27c2ae116850888b629deb81ef9137d33";
    public const int WINDOWS_APPLICATION_BYTES = 200_192;
    public const string WINDOWS_APPLICATION_SHA256 =
        "32ae4e859fc373acee698e7295837694a859808868232bf2f6328294a6e90e28";
    public const int LINUX_APPLICATION_BYTES = 200_704;
    public const string LINUX_APPLICATION_SHA256 =
        "4492bcaa51983185d8e9681bacca1770f9117e5b7c28806aa1eaf629497b09c4";
}

public static class Hostedˉverifierˉmetadataˉrequestˉapplicationˉwriter
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
            Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "verifier-metadata-request",
            "WVW2502");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted verifier metadata-request tool",
            "WVW2502");
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
            Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "verifier-metadata-request",
            "WVL2502");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉverifierˉmetadataˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted verifier metadata-request tool",
            "WVL2502");
    }
}
