using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉenumˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-enum-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-enum-request-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉenumˉrequestˉtool";
    public const int MODULE_BYTES = 25_098;
    public const string MODULE_SHA256 =
        "cd3332893277fbdc5c64e90e62900458bad506ec10be5d8b381ea9ca61a14b97";
    public const int WINDOWS_APPLICATION_BYTES = 279_040;
    public const string WINDOWS_APPLICATION_SHA256 =
        "64b6cad08646204af01dc6b6d06b581f54cfc2993ddb8f3d28b22b6f3f9cf032";
    public const int LINUX_APPLICATION_BYTES = 278_528;
    public const string LINUX_APPLICATION_SHA256 =
        "e601e3e9a9259f48c0f8d7e59f9212422d4f520ce4d4b5bbe30f6381e4970a9f";
}

public static class Hostedˉenumˉrequestˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉenumˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "enum-request", "WVW3001");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉenumˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉenumˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted enum-request tool", "WVW3001");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉenumˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "enum-request", "WVL3001");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉenumˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉenumˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted enum-request tool", "WVL3001");
    }
}
