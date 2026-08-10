using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-segment-request-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-segment-request-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉsegmentˉrequestˉtool";
    public const int MODULE_BYTES = 45_295;
    public const string MODULE_SHA256 =
        "b2f34c802a55d54424ec60024284fb133f8900f0cd2aeffac6401e12cf00109d";
    public const int WINDOWS_APPLICATION_BYTES = 527_872;
    public const string WINDOWS_APPLICATION_SHA256 =
        "a81ae969ca9ba16259cef46fe9bdb970cbffb909f2a9cf8395b2c096479fb12d";
    public const int LINUX_APPLICATION_BYTES = 528_384;
    public const string LINUX_APPLICATION_SHA256 =
        "55dbc806d7293b5e493613e698de99d4b518419ee3670ee6856977511352e5f2";
}

public static class Hostedˉcontainerˉsegmentˉrequestˉapplicationˉwriter
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
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-segment-request",
            "WVW2701");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted container-segment-request tool",
            "WVW2701");
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
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-segment-request",
            "WVL2701");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉsegmentˉrequestˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted container-segment-request tool",
            "WVL2701");
    }
}
