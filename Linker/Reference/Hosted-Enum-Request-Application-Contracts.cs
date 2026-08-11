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
    public const int MODULE_BYTES = 31_153;
    public const string MODULE_SHA256 =
        "ac4e873b5d09b3a4bed510d4e24a73dc029c132261d7d8bd8c390acbca8d5221";
    public const int WINDOWS_APPLICATION_BYTES = 338_944;
    public const string WINDOWS_APPLICATION_SHA256 =
        "6d9027d8875c63574d5637cd6977478cddea3ce94e27ddd4e462184aba7fcf40";
    public const int LINUX_APPLICATION_BYTES = 339_968;
    public const string LINUX_APPLICATION_SHA256 =
        "21f20d29277835858f7f979daaef072717291f708211035cf0a0cc4e1466c2bb";
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
