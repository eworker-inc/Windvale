using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉstartupˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-startup-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-startup-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉstartupˉtool";
    public const int MODULE_BYTES = 43_716;
    public const string MODULE_SHA256 =
        "4f8a731164676b0e4ac399f633073bd9798a867e738db3acf48173474611a915";
    public const int WINDOWS_APPLICATION_BYTES = 381_440;
    public const string WINDOWS_APPLICATION_SHA256 =
        "b7c3c04a482548092bb797eeed8d58b2b7cb68694e7aa101663a6a769e030c2e";
    public const int LINUX_APPLICATION_BYTES = 380_928;
    public const string LINUX_APPLICATION_SHA256 =
        "a46ebb92d18e19fd011dcd3905829b566633f7f60a2cf7b0c1ec8cd2a5a66024";
}

public static class Hostedˉcontainerˉstartupˉapplicationˉwriter
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
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-startup",
            "WVW2271");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container startup tool",
            "WVW2271");
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
            Hostedˉcontainerˉstartupˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-startup",
            "WVL2271");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉstartupˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container startup tool",
            "WVL2271");
    }
}
