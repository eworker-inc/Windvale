using System.Collections.Immutable;

namespace Windvale.Linker;

public static class Wvoˉstagingˉpublisherˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-wvo-staging-publisher-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-wvo-staging-publisher-v1";
    public const string MODULE_NAME =
        "Compilerˉnativeˉx64ˉloweringˉstagingˉadmissionˉtool";
    public const int MODULE_BYTES = 440_994;
    public const string MODULE_SHA256 =
        "6ef23e0db58ecd788ca97218428dc7a131662f90f5875f7644f76592a7664acc";
    public const int WINDOWS_APPLICATION_BYTES = 6_458_880;
    public const string WINDOWS_APPLICATION_SHA256 =
        "edb6bf2c08117dfe3f62a5c368abe2b0708ed040fa9e17b98494e41edea1226a";
    public const int LINUX_APPLICATION_BYTES = 6_455_869;
    public const string LINUX_APPLICATION_SHA256 =
        "01f645e2a2a6f46e059eb7adcbba3d918b55a848e11c2c4f7a50271c7d734c22";
}

public static class Wvoˉstagingˉpublisherˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Windvale.Bytecode.Verifiedˉmodule module,
        Windvale.Compiler.Native.Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Image = Windowsˉwvoˉstagingˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes);
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
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
                "WVW1501",
                $"Windows staged-WVO publisher construction failed: {Exception.Message}");
        }
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Windvale.Bytecode.Verifiedˉmodule module,
        Windvale.Compiler.Native.Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Image = Linuxˉwvoˉstagingˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes);
            Wvoˉstagingˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
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
                "WVL1501",
                $"Linux staged-WVO publisher construction failed: {Exception.Message}");
        }
    }
}
