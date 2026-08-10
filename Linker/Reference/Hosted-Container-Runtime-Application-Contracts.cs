using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉruntimeˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-runtime-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-runtime-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉruntimeˉtool";
    public const int MODULE_BYTES = 23_289;
    public const string MODULE_SHA256 =
        "93d54c99575df902588f3ab59e6e80f6ae767cf3240dbf8ea30e5806d932ffc4";
    public const int WINDOWS_APPLICATION_BYTES = 247_808;
    public const string WINDOWS_APPLICATION_SHA256 =
        "6489d8672803afc8a5e7121c2d8e720146725a518be15929460738b9069db333";
    public const int LINUX_APPLICATION_BYTES = 249_856;
    public const string LINUX_APPLICATION_SHA256 =
        "acc832a003c5862e3a507abdc25265e1a2610a247d06b58fe0f3a7f04b877617";
}

public static class Hostedˉcontainerˉruntimeˉapplicationˉwriter
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
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-runtime",
            "WVW2281");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container runtime tool",
            "WVW2281");
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
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-runtime",
            "WVL2281");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉruntimeˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container runtime tool",
            "WVL2281");
    }
}
