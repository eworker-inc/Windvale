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
    public const int MODULE_BYTES = 30_055;
    public const string MODULE_SHA256 =
        "49d4db5a310ee2bacb0bf8d78d19d040c25ea319291e39dfc98c1ea525449faf";
    public const int WINDOWS_APPLICATION_BYTES = 310_784;
    public const string WINDOWS_APPLICATION_SHA256 =
        "875f3b2241a2c542c74b102babd1fad0af00c3fea003a6f4b06a6c8e24d8cd6c";
    public const int LINUX_APPLICATION_BYTES = 311_296;
    public const string LINUX_APPLICATION_SHA256 =
        "3412d50a5edffa86e84f4dbe4360ff7f9130de96445ff5cee451dcc88ab9bc74";
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
