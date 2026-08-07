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
    public const int MODULE_BYTES = 113_525;
    public const string MODULE_SHA256 =
        "39965e723bec6904c605c74123d5e4ef1590d1cd9af5cd52d6a94494435c8da5";
    public const int WINDOWS_APPLICATION_BYTES = 1_135_616;
    public const string WINDOWS_APPLICATION_SHA256 =
        "1ffab13c1b94ec57f31fbdfbced5465bf598dfb1a237552995fece1d43c2ba37";
    public const int LINUX_APPLICATION_BYTES = 1_135_557;
    public const string LINUX_APPLICATION_SHA256 =
        "fdfe5876f1217b747ec637a3a8407948f1402505ec27c91aa6a44fd3e06fcfa2";

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
