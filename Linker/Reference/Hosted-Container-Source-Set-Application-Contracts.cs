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
    public const int MODULE_BYTES = 82_068;
    public const string MODULE_SHA256 =
        "7f110c0e7fe9a4a50627e9c600f19c61850e12a265cc44c26ad704353f4b2a74";
    public const int WINDOWS_APPLICATION_BYTES = 1_284_096;
    public const string WINDOWS_APPLICATION_SHA256 =
        "c4626edcc40c2b0c8aff4f4eec8af494034d9bf42fb04959dca393945f7eadfb";
    public const int LINUX_APPLICATION_BYTES = 1_286_144;
    public const string LINUX_APPLICATION_SHA256 =
        "a2a4687804e063d6f2d9b9c965b07893f749d34da53f271373bc0b41ae671e63";
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
