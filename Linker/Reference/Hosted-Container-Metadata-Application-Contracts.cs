using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉmetadataˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-metadata-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-metadata-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉmetadataˉtool";
    public const int MODULE_BYTES = 27_222;
    public const string MODULE_SHA256 =
        "459e9b820b788adeedcfb1dda9dc92301e60c7add754622c81fb9b24fd8418ab";
    public const int WINDOWS_APPLICATION_BYTES = 256_512;
    public const string WINDOWS_APPLICATION_SHA256 =
        "c176388e8bd6d5e60f352339d37d71610ebc27e14a6979d106043900dacc61cb";
    public const int LINUX_APPLICATION_BYTES = 258_048;
    public const string LINUX_APPLICATION_SHA256 =
        "ffa54f3bbd7596763f944bea47a98b22bf137b755274ddd0540625a6ef8a34d2";
}

public static class Hostedˉcontainerˉmetadataˉapplicationˉwriter
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
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-metadata",
            "WVW2291");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉwindowsˉidentity(
            Result,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
            "hosted-container metadata tool",
            "WVW2291");
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
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.MODULE_NAME,
            Hostedˉcompilerˉapplicationˉprofile.Compiler,
            "container-metadata",
            "WVL2291");
        return Hostedˉcontainerˉtoolˉapplicationˉbuilder.Requireˉlinuxˉidentity(
            Result,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
            Hostedˉcontainerˉmetadataˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
            "hosted-container metadata tool",
            "WVL2291");
    }
}
