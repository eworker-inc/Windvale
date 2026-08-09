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
        "77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f";
    public const int WINDOWS_APPLICATION_BYTES = 256_000;
    public const string WINDOWS_APPLICATION_SHA256 =
        "735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6";
    public const int LINUX_APPLICATION_BYTES = 254_917;
    public const string LINUX_APPLICATION_SHA256 =
        "de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a";

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
