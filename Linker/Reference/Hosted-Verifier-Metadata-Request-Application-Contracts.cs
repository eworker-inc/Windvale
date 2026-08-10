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
    public const int MODULE_BYTES = 18_272;
    public const string MODULE_SHA256 =
        "5265436ff876131ffd593e607df5e83d30b035bcfe1ea889e939f953a7e2d8f4";
    public const int WINDOWS_APPLICATION_BYTES = 195_072;
    public const string WINDOWS_APPLICATION_SHA256 =
        "562f32e9a2d31c6852bbf4e8d8fb7904f966e525025df3106bcb332908ba232e";
    public const int LINUX_APPLICATION_BYTES = 196_608;
    public const string LINUX_APPLICATION_SHA256 =
        "c9d0f8a655daeb92539eaff6224010b422c8d4b8fb280488ba89fa01af55ac31";
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
