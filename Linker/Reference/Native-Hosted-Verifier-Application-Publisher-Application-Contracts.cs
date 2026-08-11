using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-native-hosted-verifier-application-publisher-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-native-hosted-verifier-application-publisher-v1";
    public const string MODULE_NAME =
        "Windvaleˉnativeˉhostedˉverifierˉapplicationˉpublisherˉtool";
    public const int MODULE_BYTES = 29_170;
    public const string MODULE_SHA256 =
        "7ecbd7f0b11bdd7ce0ab578767b1d697bc16653e4f8182858e0ad8b8d808fb9e";
    public const int WINDOWS_APPLICATION_BYTES = 256_000;
    public const string WINDOWS_APPLICATION_SHA256 =
        "2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12";
    public const int LINUX_APPLICATION_BYTES = 254_965;
    public const string LINUX_APPLICATION_SHA256 =
        "8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e";

    internal static readonly Nativeˉpublisherˉapplicationˉcontract CONSTRUCTION = new(
        MODULE_NAME,
        MODULE_BYTES,
        MODULE_SHA256,
        0x5056_5657,
        "Applicationˉpublicationˉpublisherˉbegin",
        "Applicationˉpublicationˉpublisherˉapply",
        "native hosted-verifier-application publisher",
        [
            Windvale.Compiler.Native.Nativeˉservice.Consoleˉwriteˉline,
            Windvale.Compiler.Native.Nativeˉservice.Processˉargumentˉcount,
            Windvale.Compiler.Native.Nativeˉservice.Processˉargument,
            Windvale.Compiler.Native.Nativeˉservice.Fileˉreadˉbytes,
            Windvale.Compiler.Native.Nativeˉservice.Diagnosticˉwriteˉline,
        ]);
}

public static class Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Windvale.Bytecode.Verifiedˉmodule module,
        Windvale.Compiler.Native.Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Image = Windowsˉwvbˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes,
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .CONSTRUCTION);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .WINDOWS_APPLICATION_BYTES,
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .WINDOWS_APPLICATION_SHA256,
                "Windows native hosted-verifier-application");
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
                "WVW1412",
                "Windows native hosted-verifier-application publisher " +
                "construction failed: " + Exception.Message);
        }
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Windvale.Bytecode.Verifiedˉmodule module,
        Windvale.Compiler.Native.Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Image = Linuxˉwvbˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes,
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .CONSTRUCTION);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .LINUX_APPLICATION_BYTES,
                Nativeˉhostedˉverifierˉapplicationˉpublisherˉapplicationˉcontract
                    .LINUX_APPLICATION_SHA256,
                "Linux native hosted-verifier-application");
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
                "WVL1412",
                "Linux native hosted-verifier-application publisher " +
                "construction failed: " + Exception.Message);
        }
    }
}
