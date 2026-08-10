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
    public const int MODULE_BYTES = 32_023;
    public const string MODULE_SHA256 =
        "4a3b8ebb68b8eb1237da42d699a6b83610f3fd7ffad2402581a7037c8daad99c";
    public const int WINDOWS_APPLICATION_BYTES = 385_024;
    public const string WINDOWS_APPLICATION_SHA256 =
        "da6f614c8e8a839580d3bc7ea7a93863ea5e992b56124ce318c7546853320c65";
    public const int LINUX_APPLICATION_BYTES = 381_885;
    public const string LINUX_APPLICATION_SHA256 =
        "89238198cedf88b843ea7f3b2680b716ab554dd2789f135a0c4c6ac7d1950345";
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
