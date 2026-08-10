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
    public const int MODULE_BYTES = 23_475;
    public const string MODULE_SHA256 =
        "08533638a689db5b12628ec401c9d4c395507d2dcbda424dc40b36d1c29aa310";
    public const int WINDOWS_APPLICATION_BYTES = 248_832;
    public const string WINDOWS_APPLICATION_SHA256 =
        "5e1c241ca5c5e2fce166f2d88899dafbbab88a8dd90bfbb1a1d81863a9a1c286";
    public const int LINUX_APPLICATION_BYTES = 249_856;
    public const string LINUX_APPLICATION_SHA256 =
        "de729966eca15d95f5ae89b5416c6d3f061635a6634807556ac817b094548604";
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
