using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-service-bundle-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-service-bundle-request-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉserviceˉbundleˉrequestˉtool";
    public const int MODULE_BYTES = 29_070;
    public const string MODULE_SHA256 =
        "f79852fc85b87b4484596b7aa6a41efac2365edeb3f933b32fe12797f19e43e2";
    public const int WINDOWS_APPLICATION_BYTES = 302_080;
    public const string WINDOWS_APPLICATION_SHA256 =
        "a01435dd9c41f41174ae17b02528321c6a51d128ebd7cf7ce6d8923dc23a460a";
    public const int LINUX_APPLICATION_BYTES = 303_104;
    public const string LINUX_APPLICATION_SHA256 =
        "b9573334c811daf7c2d8bdd1701a14a236951644dac82b56056ce1905d5ecad2";
}

public static class Hostedˉserviceˉbundleˉrequestˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment,
            capabilities,
            moduleˉname,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "service-bundle-request",
            "WVW2601");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted service-bundle-request tool",
            "WVW2601");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment,
            capabilities,
            moduleˉname,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "service-bundle-request",
            "WVL2601");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉserviceˉbundleˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted service-bundle-request tool",
            "WVL2601");
    }
}
