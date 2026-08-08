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
    public const int MODULE_BYTES = 72_997;
    public const string MODULE_SHA256 =
        "5d5b7c36643bbe29f19e9e31d49d635abe7b0a46260aa9ded541239c0bd0eda9";
    public const int WINDOWS_APPLICATION_BYTES = 1_021_952;
    public const string WINDOWS_APPLICATION_SHA256 =
        "378110b7961b374803e0f541f8ffc643672942e1ad7535aa1a3f22af56b4771a";
    public const int LINUX_APPLICATION_BYTES = 1_024_000;
    public const string LINUX_APPLICATION_SHA256 =
        "aa519c28dc8a0010bdc891899031c0ce6b5f8c30a7ae7f623c5fb53582922831";
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
