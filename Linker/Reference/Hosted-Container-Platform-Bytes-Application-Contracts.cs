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
    public const int MODULE_BYTES = 30_305;
    public const string MODULE_SHA256 =
        "8038e0762f06d821600eddf9963f5178e9d48bc63f455a2b519c31650623dc3e";
    public const int WINDOWS_APPLICATION_BYTES = 312_320;
    public const string WINDOWS_APPLICATION_SHA256 =
        "8fb80312395b9db8fb9a83ba6bb62530fb63e59d351efb168e81412c9960b6d7";
    public const int LINUX_APPLICATION_BYTES = 311_296;
    public const string LINUX_APPLICATION_SHA256 =
        "e4664a0975f5117fe772a610f7bd02b3e8951d9db24bb67c0436792a21dd339f";
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
