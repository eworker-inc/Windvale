using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉsourceˉgeometryˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-source-geometry-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-source-geometry-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉsourceˉgeometryˉtool";
    public const int MODULE_BYTES = 17_802;
    public const string MODULE_SHA256 =
        "22549f1e50084b3cf20113bee6c30c3df9c4f91aad58b0a3ebe247d02a9e4a28";
    public const int WINDOWS_APPLICATION_BYTES = 198_656;
    public const string WINDOWS_APPLICATION_SHA256 =
        "209d77bd3dc10ccaec33bb0ee5351d0f4a569421ba938a26bbfc8e54d9dea996";
    public const int LINUX_APPLICATION_BYTES = 200_704;
    public const string LINUX_APPLICATION_SHA256 =
        "0457b23abbe871314eba3a91f992b1a479aa6b252f01c82a8be362670dce0f17";
}

public static class Hostedˉsourceˉgeometryˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉsourceˉgeometryˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "source-geometry", "WVW2901");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉsourceˉgeometryˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉsourceˉgeometryˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted source-geometry tool", "WVW2901");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉsourceˉgeometryˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "source-geometry", "WVL2901");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉsourceˉgeometryˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉsourceˉgeometryˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted source-geometry tool", "WVL2901");
    }
}
