using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-hosted-container-publisher-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-hosted-container-publisher-v1";
    public const string MODULE_NAME =
        "Nativeˉhostedˉcontainerˉsegmentˉsetˉadmissionˉtool";
    public const int MODULE_BYTES = 31_837;
    public const string MODULE_SHA256 =
        "76af0586a964ec5b29f5f9ef06b67d3d5dcae1423672e03a7bacc4e7b0240747";
    public const int WINDOWS_APPLICATION_BYTES = 384_000;
    public const string WINDOWS_APPLICATION_SHA256 =
        "0c53e8b9b74119d4fd8711c974a9f168e74d1e2b49ec3746eff4cdbdecf5b0ae";
    public const int LINUX_APPLICATION_BYTES = 381_885;
    public const string LINUX_APPLICATION_SHA256 =
        "347a3e5cea46c80618648bd1d0383a0ebb8d1a11819ecfff776005b4e49e3fdb";
}

public static class Hostedˉcontainerˉpublisherˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Windvale.Bytecode.Verifiedˉmodule module,
        Windvale.Compiler.Native.Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Image =
                Windowsˉimmutableˉsnapshotˉpublisherˉapplicationˉbuilder.Build(
                    module,
                    fragment,
                    moduleˉbytes,
                    Immutableˉsnapshotˉpublisherˉapplicationˉbuilder
                        .HOSTED_CONTAINER_PROFILE);
            Immutableˉsnapshotˉpublisherˉapplicationˉbuilder
                .Requireˉapplicationˉidentity(
                    Image,
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract
                        .WINDOWS_APPLICATION_BYTES,
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract
                        .WINDOWS_APPLICATION_SHA256,
                    "Windows",
                    "hosted-container");
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(
                Image.ToImmutableArray());
        }
        catch (Exception Exception) when (Exception is
            ArgumentException or
            InvalidDataException or
            OverflowException or
            Windvale.Compiler.Native.Nativeˉbackendˉexception or
            Windvale.ObjectModel.Objectˉformatˉexception)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW2301",
                $"Windows hosted-container publisher construction failed: {Exception.Message}");
        }
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Windvale.Bytecode.Verifiedˉmodule module,
        Windvale.Compiler.Native.Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Image = Linuxˉimmutableˉsnapshotˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes,
                Immutableˉsnapshotˉpublisherˉapplicationˉbuilder
                    .HOSTED_CONTAINER_PROFILE);
            Immutableˉsnapshotˉpublisherˉapplicationˉbuilder
                .Requireˉapplicationˉidentity(
                    Image,
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract
                        .LINUX_APPLICATION_BYTES,
                    Hostedˉcontainerˉpublisherˉapplicationˉcontract
                        .LINUX_APPLICATION_SHA256,
                    "Linux",
                    "hosted-container");
            return Linuxˉconsoleˉapplicationˉresult.Succeeded(
                Image.ToImmutableArray());
        }
        catch (Exception Exception) when (Exception is
            ArgumentException or
            InvalidDataException or
            OverflowException or
            Windvale.Compiler.Native.Nativeˉbackendˉexception or
            Windvale.ObjectModel.Objectˉformatˉexception)
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                "WVL2301",
                $"Linux hosted-container publisher construction failed: {Exception.Message}");
        }
    }
}
