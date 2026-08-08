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
    public const int MODULE_BYTES = 17_511;
    public const string MODULE_SHA256 =
        "2aaa45372322f39c751e6abb3062c72c14d949eb29c6edd7ca756d4378955255";
    public const int WINDOWS_APPLICATION_BYTES = 162_304;
    public const string WINDOWS_APPLICATION_SHA256 =
        "c4f2a7190ee68e39bc76f5870577be6db15e3763b18656ad40ec4ccd591cd1a8";
    public const int LINUX_APPLICATION_BYTES = 163_840;
    public const string LINUX_APPLICATION_SHA256 =
        "1c118fc24c2948a64cd9f6c1a49163cfc62333330b86b30f54998307fa6a99dc";
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
