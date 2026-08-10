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
    public const int MODULE_BYTES = 17_319;
    public const string MODULE_SHA256 =
        "e7ecb1251664430055fc26bb70371065b72ff988532af8e0897fb2acae406048";
    public const int WINDOWS_APPLICATION_BYTES = 188_928;
    public const string WINDOWS_APPLICATION_SHA256 =
        "4888d4c5252164e4a2637f78dadb5e1228044ac6834525f3cd00fb3a6bbe0b0e";
    public const int LINUX_APPLICATION_BYTES = 188_416;
    public const string LINUX_APPLICATION_SHA256 =
        "3827deb66f8bb15585e02ba7e2e01a217cc9375eb05ade4c70a91930b0af8803";
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
