using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-source-set-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-source-set-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉsourceˉsetˉtool";
    public const int MODULE_BYTES = 73_387;
    public const string MODULE_SHA256 =
        "4b519338e12b852efa1df2a97ce09deb02c2ace4a708ce4b60025cf13083762c";
    public const int WINDOWS_APPLICATION_BYTES = 1_030_656;
    public const string WINDOWS_APPLICATION_SHA256 =
        "b54effc87ff43dd5871712555ce6afa800ce3a2d535048a40fc1b79cf094d87f";
    public const int LINUX_APPLICATION_BYTES = 1_032_192;
    public const string LINUX_APPLICATION_SHA256 =
        "ceaa9546c8520b32892a97906d04a827754483dfaa4df86ed8d54af846cb31ed";
}

public static class Hostedˉcontainerˉsourceˉsetˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-source-set", "WVW2911");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted container source-set tool", "WVW2911");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-source-set", "WVL2911");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉsourceˉsetˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted container source-set tool", "WVL2911");
    }
}
