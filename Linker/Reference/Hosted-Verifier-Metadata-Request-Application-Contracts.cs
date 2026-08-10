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
    public const int MODULE_BYTES = 18_086;
    public const string MODULE_SHA256 =
        "150792a279b3ca080181576b446790b7f4539f07b7c4cfd35017975f3cd1d529";
    public const int WINDOWS_APPLICATION_BYTES = 194_048;
    public const string WINDOWS_APPLICATION_SHA256 =
        "75b6fb59030e1ec7d6b3c336d10aa89badd928d35b669f2fbdf7f33e243da520";
    public const int LINUX_APPLICATION_BYTES = 196_608;
    public const string LINUX_APPLICATION_SHA256 =
        "4968f2e5d8481b96d701bf6dd350a93097d086191007396ad447b504c37d7109";
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
