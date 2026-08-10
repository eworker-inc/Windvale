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
        "9ee91e3044193e2e90461ecf4e7ddefa4b5583f55b041b31911044c6d65b92c7";
    public const int LINUX_APPLICATION_BYTES = 1_340_357;
    public const string LINUX_APPLICATION_SHA256 =
        "2ade91f624609c93a3b80a0802679bef79832c0a63db7996c889794d365f1188";
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
