using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-platform-bytes-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-platform-bytes-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉplatformˉbytesˉtool";
    public const int MODULE_BYTES = 29_793;
    public const string MODULE_SHA256 =
        "3cce3e2d548be4f9304a6e6ae62355d42b2879c4fe837283fb8415ea4d715732";
    public const int WINDOWS_APPLICATION_BYTES = 309_760;
    public const string WINDOWS_APPLICATION_SHA256 =
        "46db452f1356dadb93bf80d4a81a34cf73e02d0f45342309700b5892ea571f7b";
    public const int LINUX_APPLICATION_BYTES = 311_296;
    public const string LINUX_APPLICATION_SHA256 =
        "cf09c62056d4960e914504779973a7227bcb2d9879c4328496adb859f83c526d";
}

public static class Hostedˉcontainerˉplatformˉbytesˉapplicationˉwriter
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
            Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-platform-bytes",
            "WVW2261");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract
                .WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract
                .WINDOWS_APPLICATION_SHA256,
            "hosted-container platform-bytes tool",
            "WVW2261");
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
            Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-platform-bytes",
            "WVL2261");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract
                .LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉplatformˉbytesˉapplicationˉcontract
                .LINUX_APPLICATION_SHA256,
            "hosted-container platform-bytes tool",
            "WVL2261");
    }
}
