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
    public const int MODULE_BYTES = 26_615;
    public const string MODULE_SHA256 =
        "7eb367894051b89acee497c906c3c3282621f9d0d2a7274d79931af0ec7926e2";
    public const int WINDOWS_APPLICATION_BYTES = 271_360;
    public const string WINDOWS_APPLICATION_SHA256 =
        "0101389e7fca09905e5aa64902df6b61d07debe4735e091cf57d01af7b217c3b";
    public const int LINUX_APPLICATION_BYTES = 270_336;
    public const string LINUX_APPLICATION_SHA256 =
        "216dc362944945ba3259d6ffb0aeed094eb8ba2d475678641335d892e2c316ec";
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
