using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Wvbˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME = "windows-x64-wvb-publisher-v1";
    public const string LINUX_TARGET_NAME = "linux-x64-wvb-publisher-v1";
    public const string MODULE_NAME = "Windvaleˉwvbˉpublisherˉtool";
    public const int MODULE_BYTES = 136_698;
    public const string MODULE_SHA256 =
        "d8fcbebe7915542b0206900bcce5459957cee768470bf64a2999e6ee688af05d";
    public const int WINDOWS_APPLICATION_BYTES = 1_121_792;
    public const string WINDOWS_APPLICATION_SHA256 =
        "f2502ecf9143cfa1343c5f5cb1de066bdf1f82f0e4782afae178f11c41afd735";
    public const int LINUX_APPLICATION_BYTES = 1_119_173;
    public const string LINUX_APPLICATION_SHA256 =
        "71dccc29333b05cff71e4b36e5e41617e0df4f8d747747479e8a27f4a90ed3b0";
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
