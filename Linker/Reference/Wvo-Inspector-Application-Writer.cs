using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Wvoˉinspectorˉapplicationˉwriter
{
    private const string MODULE_NAME = "Wvoˉobjectˉcore";

    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector);
            var Verified = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1902",
                    "The independently verified Windows WVO inspector did not reproduce its profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVW1901" : "WVW1902",
                $"Hosted Windows WVO inspector verification failed: {Exception.Message}");
        }
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉinspector(
                fragment,
                Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector);
            var Verified = Linuxˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Linuxˉconsoleˉapplicationˉresult.Failed(
                    "WVL1902",
                    "The independently verified Linux WVO inspector did not reproduce its profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVL1901" : "WVL1902",
                $"Hosted Linux WVO inspector verification failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(Nativeˉfragment fragment, string moduleˉname)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!StringComparer.Ordinal.Equals(moduleˉname, MODULE_NAME))
        {
            throw new InvalidOperationException(
                "The hosted WVO inspector requires its canonical WVB module identity.");
        }
        var Entries = fragment.Symbols
            .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main")
            .ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted WVO inspector requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }
}
