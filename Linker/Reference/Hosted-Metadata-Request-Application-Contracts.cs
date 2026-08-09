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
    public const int MODULE_BYTES = 55_175;
    public const string MODULE_SHA256 =
        "80805fe671aca5d479dba50f8fb2ac0e52850d16e2b567e307ff809ab0e1505b";
    public const int WINDOWS_APPLICATION_BYTES = 802_816;
    public const string WINDOWS_APPLICATION_SHA256 =
        "fb39a9813447864a493d27b25bc41c251a3ccbe28eb629d4706e601ae8acbed9";
    public const int LINUX_APPLICATION_BYTES = 802_816;
    public const string LINUX_APPLICATION_SHA256 =
        "a108a77de96428d45127281fd92c8ce98e8434049e8cfc6ffc1d37296de5a1e3";
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
