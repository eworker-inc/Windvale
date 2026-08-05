using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Wvbˉrunnerˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities)
    {
        try
        {
            var Entry = Nativeˉentry(fragment);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉrunner(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner);
            var Verified = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1602",
                    "The independently verified Windows WVB runner did not reproduce its profile, entry, and service bundle.");
            }
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (
            Exception is Nativeˉbackendˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException or
                InvalidOperationException)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                Exception is InvalidOperationException ? "WVW1601" : "WVW1602",
                $"Hosted Windows WVB runner verification failed: {Exception.Message}");
        }
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities)
    {
        try
        {
            var Entry = Nativeˉentry(fragment);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉrunner(
                fragment,
                Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner);
            var Verified = Linuxˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Linuxˉconsoleˉapplicationˉresult.Failed(
                    "WVL1602",
                    "The independently verified Linux WVB runner did not reproduce its profile, entry, and service bundle.");
            }
            return Linuxˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (
            Exception is Nativeˉbackendˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException or
                InvalidOperationException)
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                Exception is InvalidOperationException ? "WVL1601" : "WVL1602",
                $"Hosted Linux WVB runner verification failed: {Exception.Message}");
        }
    }

    private static uint Nativeˉentry(Nativeˉfragment fragment)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        var Entries = fragment.Symbols
            .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main")
            .ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted WVB runner requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }
}
