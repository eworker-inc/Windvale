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
    public const int MODULE_BYTES = 115_107;
    public const string MODULE_SHA256 =
        "e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d";
    public const int WINDOWS_APPLICATION_BYTES = 1_158_656;
    public const string WINDOWS_APPLICATION_SHA256 =
        "e0ba662b480445ec10e03c9046f3fe394cb48e9d68c46fe705c779c09192dc74";
    public const int LINUX_APPLICATION_BYTES = 1_156_037;
    public const string LINUX_APPLICATION_SHA256 =
        "e58153168780a3a4dc0be6f16eaa71b2040ad4908f1ba6bf24e33a66c29d155b";

    internal static readonly Nativeˉpublisherˉapplicationˉcontract CONSTRUCTION = new(
        MODULE_NAME,
        MODULE_BYTES,
        MODULE_SHA256,
        0x4150_5657,
        "Applicationˉpublicationˉpublisherˉbegin",
        "Applicationˉpublicationˉpublisherˉapply",
        "console-application publisher",
        [
            Windvale.Compiler.Native.Nativeˉservice.Consoleˉwriteˉline,
            Windvale.Compiler.Native.Nativeˉservice.Processˉargumentˉcount,
            Windvale.Compiler.Native.Nativeˉservice.Processˉargument,
            Windvale.Compiler.Native.Nativeˉservice.Fileˉreadˉbytes,
            Windvale.Compiler.Native.Nativeˉservice.Diagnosticˉwriteˉline,
        ]);
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
