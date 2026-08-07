using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Consoleˉapplicationˉverifierˉapplicationˉwriter
{
    private const string MODULE_NAME =
        "Windvaleˉconsoleˉapplicationˉverifierˉtool";

    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname);
            var Bundle =
                X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉapplicationˉverifier(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier);
            var Verified = Windowsˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Layout.Snapshotˉcapacity != 2 ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1912",
                    "The independently verified Windows console-application verifier did not reproduce its two-input profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVW1911" : "WVW1912",
                $"Hosted Windows console-application verifier construction failed: {Exception.Message}");
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
            var Bundle =
                X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉapplicationˉverifier(
                fragment,
                Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier);
            var Verified = Linuxˉhostedˉverifierˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Layout.Snapshotˉcapacity != 2 ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Linuxˉconsoleˉapplicationˉresult.Failed(
                    "WVL1912",
                    "The independently verified Linux console-application verifier did not reproduce its two-input profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVL1911" : "WVL1912",
                $"Hosted Linux console-application verifier construction failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(Nativeˉfragment fragment, string moduleˉname)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!StringComparer.Ordinal.Equals(moduleˉname, MODULE_NAME))
        {
            throw new InvalidOperationException(
                "The hosted console-application verifier requires its canonical WVB module identity.");
        }
        var Entries = fragment.Symbols
            .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main")
            .ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted console-application verifier requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }
}
