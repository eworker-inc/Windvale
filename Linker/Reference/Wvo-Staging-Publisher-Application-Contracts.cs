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
    public const int MODULE_BYTES = 414_230;
    public const string MODULE_SHA256 =
        "07b9e0eff09927208980a0acdd7d88acc6cb3f40981d0c0a582951e7d30517f1";
    public const int WINDOWS_APPLICATION_BYTES = 6_010_880;
    public const string WINDOWS_APPLICATION_SHA256 =
        "7a319247b6f6aabbf185cb46b491650303840f1a30849f576af7cf2258b65b40";
    public const int LINUX_APPLICATION_BYTES = 6_008_537;
    public const string LINUX_APPLICATION_SHA256 =
        "b297da74e2fc6608023cd166f9abfa6f8543aef77dd1a7b01922079d38a61bd0";
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
