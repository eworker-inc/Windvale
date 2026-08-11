using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Wvbˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME = "windows-x64-wvb-publisher-v1";
    public const string LINUX_TARGET_NAME = "linux-x64-wvb-publisher-v1";
    public const string MODULE_NAME = "Windvaleˉwvbˉpublisherˉtool";
    public const int MODULE_BYTES = 159_770;
    public const string MODULE_SHA256 =
        "8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96";
    public const int WINDOWS_APPLICATION_BYTES = 1_340_928;
    public const string WINDOWS_APPLICATION_SHA256 =
        "71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3";
    public const int LINUX_APPLICATION_BYTES = 1_340_405;
    public const string LINUX_APPLICATION_SHA256 =
        "7024fc5f96181f819e01bc41bc5c34d9eaed4301ea459c0c2bc43b7f52b21095";
}

public static class Wvbˉpublisherˉapplicationˉwriter
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
                moduleˉbytes);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvbˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Wvbˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
                "Windows");
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
                "WVW1401",
                $"Windows WVB publisher construction failed: {Exception.Message}");
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
                moduleˉbytes);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvbˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Wvbˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
                "Linux");
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
                "WVL1401",
                $"Linux WVB publisher construction failed: {Exception.Message}");
        }
    }
}
