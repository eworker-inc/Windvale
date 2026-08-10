using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉfixedˉservicesˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-fixed-services-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-fixed-services-v1";
    public const string MODULE_NAME = "Nativeˉhostedˉfixedˉservicesˉtool";
    public const int MODULE_BYTES = 7_491;
    public const string MODULE_SHA256 =
        "048deb0818f11c61c2dd16b6bbcde8f7f58eb351c59149332d12bac6256797c0";
    public const int WINDOWS_APPLICATION_BYTES = 75_264;
    public const string WINDOWS_APPLICATION_SHA256 =
        "1c1c70348885d68db75fcff43ce500245266806085c3115f64d75de6cf529720";
    public const int LINUX_APPLICATION_BYTES = 77_824;
    public const string LINUX_APPLICATION_SHA256 =
        "8a659f15c8d3f11812655a47013133ed6fd66e804c1a304c18efdc174606ea15";
}

public static class Hostedˉfixedˉservicesˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉfixedˉservicesˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "fixed-services", "WVW2909");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉfixedˉservicesˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉfixedˉservicesˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted fixed-services tool", "WVW2909");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉfixedˉservicesˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "fixed-services", "WVL2909");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉfixedˉservicesˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉfixedˉservicesˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted fixed-services tool", "WVL2909");
    }
}
