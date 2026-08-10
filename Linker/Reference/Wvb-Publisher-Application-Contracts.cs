using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Wvbˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME = "windows-x64-wvb-publisher-v1";
    public const string LINUX_TARGET_NAME = "linux-x64-wvb-publisher-v1";
    public const string MODULE_NAME = "Windvaleˉwvbˉpublisherˉtool";
    public const int MODULE_BYTES = 159_328;
    public const string MODULE_SHA256 =
        "5da26ddb18cdb6511cb6c28b9603e79c7d318696a5371ca4410db47be7bcb219";
    public const int WINDOWS_APPLICATION_BYTES = 1_313_792;
    public const string WINDOWS_APPLICATION_SHA256 =
        "e95676eabf80e5230d39241a9967b47bf61b4c96bddca0280ff0abb772bae1d1";
    public const int LINUX_APPLICATION_BYTES = 1_311_685;
    public const string LINUX_APPLICATION_SHA256 =
        "3bb76b7ab4f5f5a00d9f949e70a65d49aac7b0973856e6a6148f2a9a5ca38c72";
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
