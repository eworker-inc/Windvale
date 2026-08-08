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
    public const int MODULE_BYTES = 31_271;
    public const string MODULE_SHA256 =
        "6ce0c3a4bf48b6d0db4c50574805655777be93f6a10555a4d423947b00bd0018";
    public const int WINDOWS_APPLICATION_BYTES = 379_904;
    public const string WINDOWS_APPLICATION_SHA256 =
        "823b9ed3bafdb4a8cb8e5a5a3fe4c9d834f6702771766add5fbf439d8d5d2b37";
    public const int LINUX_APPLICATION_BYTES = 377_725;
    public const string LINUX_APPLICATION_SHA256 =
        "02602e7fb552dafcb6bf2ed2a858eec9c17e257bfd4bc097c47f55fd155a50c9";
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
