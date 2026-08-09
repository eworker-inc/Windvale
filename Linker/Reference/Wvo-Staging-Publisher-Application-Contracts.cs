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
    public const int MODULE_BYTES = 433_523;
    public const string MODULE_SHA256 =
        "221b20ab5db8785ec495d2151088c532f53fc8c9c66fbb021156f05b62e32ca3";
    public const int WINDOWS_APPLICATION_BYTES = 6_390_784;
    public const string WINDOWS_APPLICATION_SHA256 =
        "adcdde6363b79e107f26e7042c2970996fabfa972d9a4fe91f5c8a5d5238faa6";
    public const int LINUX_APPLICATION_BYTES = 6_390_685;
    public const string LINUX_APPLICATION_SHA256 =
        "eed27297af45813c824558aaee8ac515f62b36fdcfdd76845e2ed15a0f924ce4";
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
            var Image = Windowsˉimmutableˉsnapshotˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes,
                Immutableˉsnapshotˉpublisherˉapplicationˉbuilder.WVO_STAGING_PROFILE);
            Immutableˉsnapshotˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
                "Windows",
                "staged-WVO");
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
            var Image = Linuxˉimmutableˉsnapshotˉpublisherˉapplicationˉbuilder.Build(
                module,
                fragment,
                moduleˉbytes,
                Immutableˉsnapshotˉpublisherˉapplicationˉbuilder.WVO_STAGING_PROFILE);
            Immutableˉsnapshotˉpublisherˉapplicationˉbuilder.Requireˉapplicationˉidentity(
                Image,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Wvoˉstagingˉpublisherˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
                "Linux",
                "staged-WVO");
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
