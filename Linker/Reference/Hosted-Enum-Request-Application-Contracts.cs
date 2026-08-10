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
    public const int MODULE_BYTES = 30_759;
    public const string MODULE_SHA256 =
        "682c2bf76569ba0ec6c58dfd3ade64d7582a9d22c397c55a22e1785fe8521fb6";
    public const int WINDOWS_APPLICATION_BYTES = 334_336;
    public const string WINDOWS_APPLICATION_SHA256 =
        "47394d8982403c3f473e2f62f33790fab9d12e4607f58e2ba603027738410908";
    public const int LINUX_APPLICATION_BYTES = 335_872;
    public const string LINUX_APPLICATION_SHA256 =
        "b428ca6305422bcd168029d451840db513543b7a3d578a9f989d8f6f9635fef0";
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
