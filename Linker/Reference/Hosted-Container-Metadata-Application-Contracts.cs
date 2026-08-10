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
    public const int MODULE_BYTES = 27_036;
    public const string MODULE_SHA256 =
        "184cce30bdc7d24a45175ab76dcee535ed9dc9d13dce6f56afe7986553b14aa9";
    public const int WINDOWS_APPLICATION_BYTES = 255_488;
    public const string WINDOWS_APPLICATION_SHA256 =
        "73b0873e9b37543320fcd8fe5639a48e8f1ca3b69acced10975422ddd0042da8";
    public const int LINUX_APPLICATION_BYTES = 253_952;
    public const string LINUX_APPLICATION_SHA256 =
        "dea5292090eecb4e0c93b645de186acb8d8ab9bfa4ab87ff8780c633479c17a8";
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
