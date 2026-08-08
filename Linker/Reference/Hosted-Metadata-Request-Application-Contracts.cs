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
    public const int MODULE_BYTES = 54_397;
    public const string MODULE_SHA256 =
        "683538840f21325469324a62e3296582c43f4df9f396908263dd9f074c5b19b9";
    public const int WINDOWS_APPLICATION_BYTES = 784_896;
    public const string WINDOWS_APPLICATION_SHA256 =
        "8261f3092c95bdc16bbf0444e96208a47ba142edd5955582e49a5bbddab24ef8";
    public const int LINUX_APPLICATION_BYTES = 786_432;
    public const string LINUX_APPLICATION_SHA256 =
        "2311b8a237e0001a7437c6767d88153bbe57c100f696f4b045ec232a11faa73b";
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
