using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉenumˉserviceˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-enum-service-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-enum-service-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉenumˉserviceˉtool";
    public const int MODULE_BYTES = 18_883;
    public const string MODULE_SHA256 =
        "6e44a4c0f4d61ea9aa3d72442baba60080896c0cf7d3536b353fcd61ff48ec07";
    public const int WINDOWS_APPLICATION_BYTES = 184_832;
    public const string WINDOWS_APPLICATION_SHA256 =
        "7563f37f3c77473ce52b73506dcb54516107fc964c19860d5dcc75d3bdd52cdf";
    public const int LINUX_APPLICATION_BYTES = 184_320;
    public const string LINUX_APPLICATION_SHA256 =
        "90e99134b750231e011e2c254bac8c12d9d52d8adcbd3a9d5d03d7dc1da26f4b";
}

public static class Hostedˉenumˉserviceˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉenumˉserviceˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "enum-service", "WVW3101");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉenumˉserviceˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted enum-service tool", "WVW3101");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉenumˉserviceˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "enum-service", "WVL3101");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉenumˉserviceˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted enum-service tool", "WVL3101");
    }
}
