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
    public const int MODULE_BYTES = 44_019;
    public const string MODULE_SHA256 =
        "c18f71d2a20612dd10063e88a9ebb34ff1a416da207ad685d49fc0e92ed8e206";
    public const int WINDOWS_APPLICATION_BYTES = 519_168;
    public const string WINDOWS_APPLICATION_SHA256 =
        "c4690d57b85b951b5af2c7eefdbd81a805114f9a246c02bbf2b593ecec34da18";
    public const int LINUX_APPLICATION_BYTES = 520_192;
    public const string LINUX_APPLICATION_SHA256 =
        "4207ba76d4387ec3dce54210a9278e616ddf32ae41f36d3f478f8e134147f82d";
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
