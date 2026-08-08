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
    public const int MODULE_BYTES = 431_568;
    public const string MODULE_SHA256 =
        "9ca9c1225eb5b9b9e95021b7ef897faf97e14121c5a94d72d9489b95b4d0e4c2";
    public const int WINDOWS_APPLICATION_BYTES = 6_364_672;
    public const string WINDOWS_APPLICATION_SHA256 =
        "5d9d2d8e899732b2821b6a07b98dde99532dce40d34f2e10eeb53104f3081635";
    public const int LINUX_APPLICATION_BYTES = 6_361_965;
    public const string LINUX_APPLICATION_SHA256 =
        "f2166008e744b856f9df18949230b47e0fceba3cdec65dfb6784be38edd5577b";
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
