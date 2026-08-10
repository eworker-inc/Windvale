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
    public const int MODULE_BYTES = 82_254;
    public const string MODULE_SHA256 =
        "9b5a64fee4d986f0ec0c490f34bb524f2052b1fe3d028d5b6ae2f7ec12552d97";
    public const int WINDOWS_APPLICATION_BYTES = 1_285_632;
    public const string WINDOWS_APPLICATION_SHA256 =
        "7aa0cd770aa480f6dc21cf0480fed1b586e13b0b31e95d49972a1becc062373f";
    public const int LINUX_APPLICATION_BYTES = 1_286_144;
    public const string LINUX_APPLICATION_SHA256 =
        "57f44f2f53f3805de0e0f4ee358ea23a3f88e7b3594b49fe10a003b95cd46e02";
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
