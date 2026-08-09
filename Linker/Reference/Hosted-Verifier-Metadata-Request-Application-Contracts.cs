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
    public const int MODULE_BYTES = 17_204;
    public const string MODULE_SHA256 =
        "c5aeb2ff6f50760bd01843d43a307fb23988d9fe6c8865b4c549d21f52486f25";
    public const int WINDOWS_APPLICATION_BYTES = 187_904;
    public const string WINDOWS_APPLICATION_SHA256 =
        "dc42cd573e26ba8617a7323089f2c140f0488ec0cb3b9a6e4b77d5c4d7fbd4d5";
    public const int LINUX_APPLICATION_BYTES = 188_416;
    public const string LINUX_APPLICATION_SHA256 =
        "cfd0071c3d103ca0feedb33370b81bc3edb0b41e8bb95ee9744d1b53342fb6bd";
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
