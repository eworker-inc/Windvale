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
    public const int MODULE_BYTES = 81_502;
    public const string MODULE_SHA256 =
        "d8cb87c7c8b1da83572d13ff92c4555c16b19f44c1d649c5b5cb35f9e9fd60ce";
    public const int WINDOWS_APPLICATION_BYTES = 1_280_512;
    public const string WINDOWS_APPLICATION_SHA256 =
        "a84dbdc7f96eafaab2ed17b076897338cfc86271be4ffddf4bef627d17d12083";
    public const int LINUX_APPLICATION_BYTES = 1_282_048;
    public const string LINUX_APPLICATION_SHA256 =
        "872cca3fa39763a58b2183f9cf145d60c666d57fe0c7cd5984070cd55e1b6786";
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
