using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Hostedˉconsoleˉsegmentedˉpackagerˉapplicationˉwriter
{
    private const string MODULE_NAME = "Consoleˉapplicationˉsegmentedˉpackager";

    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉpackager(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager);
            var Verified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW2102",
                    "The independently verified Windows segmented console packager did not reproduce its profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVW2101" : "WVW2102",
                $"Hosted Windows segmented console-packager verification failed: {Exception.Message}");
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
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉpackager(
                fragment,
                Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager);
            var Verified = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉcompilerˉapplicationˉprofile.Consoleˉpackager ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Linuxˉconsoleˉapplicationˉresult.Failed(
                    "WVL2102",
                    "The independently verified Linux segmented console packager did not reproduce its profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVL2101" : "WVL2102",
                $"Hosted Linux segmented console-packager verification failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(Nativeˉfragment fragment, string moduleˉname)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!StringComparer.Ordinal.Equals(moduleˉname, MODULE_NAME))
        {
            throw new InvalidOperationException(
                "The hosted segmented console packager requires its canonical WVB module identity.");
        }
        var Entries = fragment.Symbols
            .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main")
            .ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted segmented console packager requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }
}
