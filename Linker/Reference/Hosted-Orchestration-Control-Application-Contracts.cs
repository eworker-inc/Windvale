using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉorchestrationˉcontrolˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-orchestration-control-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-orchestration-control-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉorchestrationˉcontrolˉtool";
    public const int MODULE_BYTES = 21_214;
    public const string MODULE_SHA256 =
        "1d9f86cf636de119bde26a7b5fda5977e032db336d07c3937f0dd42df000e4bf";
    public const int WINDOWS_APPLICATION_BYTES = 236_032;
    public const string WINDOWS_APPLICATION_SHA256 =
        "d8b10130bc946261526ee0accc9fcbd42dbe2a5d9fd3e4d4f349038550c8c559";
    public const int LINUX_APPLICATION_BYTES = 237_568;
    public const string LINUX_APPLICATION_SHA256 =
        "45c8bf1163556c851db8b7fecb2556e899c816d06bd39209d65db942fea3c44a";
}

public static class Hostedˉorchestrationˉcontrolˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉwindows(
            fragment, capabilities, moduleˉname,
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "orchestration-control", "WVW2910");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted orchestration-control tool", "WVW2910");
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        var Result = Hostedˉcontainerˉtoolˉapplicationˉbuilder.Writeˉlinux(
            fragment, capabilities, moduleˉname,
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "orchestration-control", "WVL2910");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉorchestrationˉcontrolˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted orchestration-control tool", "WVL2910");
    }
}
