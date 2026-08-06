using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Consoleˉapplicationˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-console-application-publisher-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-console-application-publisher-v1";
    public const string MODULE_NAME =
        "Windvaleˉconsoleˉapplicationˉpublisherˉtool";
    public const int MODULE_BYTES = 56_375;
    public const string MODULE_SHA256 =
        "1e35f7cc9e53322ebcc70c332486eef983ff59370246c62ec4e8cbcd144d8403";
    public const int WINDOWS_APPLICATION_BYTES = 642_048;
    public const string WINDOWS_APPLICATION_SHA256 =
        "1bd3bbd24fc22940b96badb7e809899d42e42a25a5247dfededb00048232675d";
    public const int LINUX_APPLICATION_BYTES = 639_941;
    public const string LINUX_APPLICATION_SHA256 =
        "2edc7ebe23660e299d9db4bf55d4537ec102b7a3b2d46ba833e549cd355a0af7";

    internal static readonly Nativeˉpublisherˉapplicationˉcontract CONSTRUCTION = new(
        MODULE_NAME,
        MODULE_BYTES,
        MODULE_SHA256,
        0x4150_5657,
        "Applicationˉpublicationˉpublisherˉbegin",
        "Applicationˉpublicationˉpublisherˉapply",
        "console-application publisher");
}

public static class Consoleˉapplicationˉpublisherˉapplicationˉwriter
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
                Consoleˉapplicationˉpublisherˉapplicationˉcontract.CONSTRUCTION);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Consoleˉapplicationˉpublisherˉapplicationˉcontract
                    .WINDOWS_APPLICATION_BYTES,
                Consoleˉapplicationˉpublisherˉapplicationˉcontract
                    .WINDOWS_APPLICATION_SHA256,
                "Windows console-application");
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
                "WVW1411",
                $"Windows console-application publisher construction failed: " +
                Exception.Message);
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
                Consoleˉapplicationˉpublisherˉapplicationˉcontract.CONSTRUCTION);
            Wvbˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Consoleˉapplicationˉpublisherˉapplicationˉcontract
                    .LINUX_APPLICATION_BYTES,
                Consoleˉapplicationˉpublisherˉapplicationˉcontract
                    .LINUX_APPLICATION_SHA256,
                "Linux console-application");
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
                "WVL1411",
                $"Linux console-application publisher construction failed: " +
                Exception.Message);
        }
    }
}
