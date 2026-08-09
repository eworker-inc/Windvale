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
    public const int MODULE_BYTES = 26_167;
    public const string MODULE_SHA256 =
        "1471775aab260d48db4852cd055f04698b036224f877fcab958f3e1bd9814b83";
    public const int WINDOWS_APPLICATION_BYTES = 292_352;
    public const string WINDOWS_APPLICATION_SHA256 =
        "44e11d1105ab685e51ccce2dc6f800b0c2c1d7e897539cd7b65a436d4ff67f21";
    public const int LINUX_APPLICATION_BYTES = 294_912;
    public const string LINUX_APPLICATION_SHA256 =
        "c767e5f0c509e803dbcfe3fc1283f8bcf1208c80a0fee478d5348116f9187040";
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
