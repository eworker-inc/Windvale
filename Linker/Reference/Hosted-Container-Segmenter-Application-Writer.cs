using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Hostedˉcontainerˉsegmenterˉapplicationˉwriter
{
    // Stage 0 package wiring; the digest-bound native launcher owns its removal.
    private const string MODULE_NAME = "Nativeˉhostedˉcontainerˉsegmenterˉtool";

    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname) =>
        Writeˉwindowsˉorˉfailure(fragment, capabilities, moduleˉname);

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname) =>
        Writeˉlinuxˉorˉfailure(fragment, capabilities, moduleˉname);

    private static Windowsˉconsoleˉapplicationˉresult Writeˉwindowsˉorˉfailure(
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
                Hostedˉcompilerˉapplicationˉprofile.Hostedˉcontainerˉsegmenter);
            var Verified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Hostedˉcontainerˉsegmenter);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                throw new InvalidDataException(
                    "The Windows hosted-container segmenter did not reproduce its entry and service bundle.");
            }
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (Exception is
            Nativeˉbackendˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException or
                InvalidOperationException)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW2201",
                $"Hosted Windows container-segmenter construction failed: {Exception.Message}");
        }
    }

    private static Linuxˉconsoleˉapplicationˉresult Writeˉlinuxˉorˉfailure(
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
                Hostedˉcompilerˉapplicationˉprofile.Hostedˉcontainerˉsegmenter);
            var Verified = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Hostedˉcontainerˉsegmenter);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                throw new InvalidDataException(
                    "The Linux hosted-container segmenter did not reproduce its entry and service bundle.");
            }
            return Linuxˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (Exception is
            Nativeˉbackendˉexception or
                InvalidDataException or
                OverflowException or
                ArgumentException or
                InvalidOperationException)
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                "WVL2201",
                $"Hosted Linux container-segmenter construction failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(Nativeˉfragment fragment, string moduleˉname)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!StringComparer.Ordinal.Equals(moduleˉname, MODULE_NAME))
        {
            throw new InvalidOperationException(
                "The hosted-container segmenter requires its canonical WVB module identity.");
        }
        var Entries = fragment.Symbols.Where(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            Symbol.Name == "Main").ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted-container segmenter requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }
}
