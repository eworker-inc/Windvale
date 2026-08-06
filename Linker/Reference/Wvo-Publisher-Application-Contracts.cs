using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Wvoˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME = "windows-x64-wvo-publisher-v1";
    public const string LINUX_TARGET_NAME = "linux-x64-wvo-publisher-v1";
    public const string MODULE_NAME = "Windvaleˉwvoˉpublisherˉtool";
    public const int MODULE_BYTES = 41_365;
    public const string MODULE_SHA256 =
        "4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5";
    public const int WINDOWS_APPLICATION_BYTES = 430_080;
    public const string WINDOWS_APPLICATION_SHA256 =
        "035a1baaada6be8d057b782804a8650d978da53dd008337ab00258f2ab597cb7";
    public const int LINUX_APPLICATION_BYTES = 426_949;
    public const string LINUX_APPLICATION_SHA256 =
        "ac2bb513e2145e9eb911a9be142fc2f1f990a1bab21f278dd841043042b51f7a";

    internal static readonly Nativeˉpublisherˉapplicationˉcontract CONSTRUCTION = new(
        MODULE_NAME,
        MODULE_BYTES,
        MODULE_SHA256,
        0x4F50_5657,
        "Wvoˉpublicationˉpublisherˉbegin",
        "Wvoˉpublicationˉpublisherˉapply",
        "WVO publisher",
        [
            Windvale.Compiler.Native.Nativeˉservice.Consoleˉwriteˉline,
            Windvale.Compiler.Native.Nativeˉservice.Processˉargumentˉcount,
            Windvale.Compiler.Native.Nativeˉservice.Processˉargument,
            Windvale.Compiler.Native.Nativeˉservice.Fileˉreadˉbytes,
            Windvale.Compiler.Native.Nativeˉservice.Diagnosticˉwriteˉline,
        ]);
}

public static class Wvoˉpublisherˉapplicationˉwriter
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
                Wvoˉpublisherˉapplicationˉcontract.CONSTRUCTION);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvoˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Wvoˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
                "Windows WVO");
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
                "WVW1421",
                $"Windows WVO publisher construction failed: {Exception.Message}");
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
                Wvoˉpublisherˉapplicationˉcontract.CONSTRUCTION);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvoˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Wvoˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
                "Linux WVO");
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
                "WVL1421",
                $"Linux WVO publisher construction failed: {Exception.Message}");
        }
    }
}
