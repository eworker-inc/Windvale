using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

// Deletion-bound Stage 0 package wiring shared by native hosted-container tools.
internal static class Hostedˉcontainerˉtoolˉapplicationˉbuilder
{
    internal static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname,
        string expectedˉmoduleˉname,
        Hostedˉcompilerˉapplicationˉprofile profile,
        string description,
        string diagnosticˉcode)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname, expectedˉmoduleˉname);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉpackager(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                profile);
            var Verified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                profile);
            Requireˉverified(Verified.Nativeˉentryˉoffset, Entry,
                Verified.Bundleˉimage, Bundle.Imageˉbytes, "Windows", description);
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
                diagnosticˉcode,
                $"Hosted Windows {description} construction failed: {Exception.Message}");
        }
    }

    internal static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname,
        string expectedˉmoduleˉname,
        Hostedˉcompilerˉapplicationˉprofile profile,
        string description,
        string diagnosticˉcode)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname, expectedˉmoduleˉname);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉconsoleˉpackager(
                fragment,
                Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                profile);
            var Verified = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                profile);
            Requireˉverified(Verified.Nativeˉentryˉoffset, Entry,
                Verified.Bundleˉimage, Bundle.Imageˉbytes, "Linux", description);
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
                diagnosticˉcode,
                $"Hosted Linux {description} construction failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(
        Nativeˉfragment fragment,
        string moduleˉname,
        string expectedˉmoduleˉname)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!StringComparer.Ordinal.Equals(moduleˉname, expectedˉmoduleˉname))
        {
            throw new InvalidOperationException(
                "The hosted-container tool requires its canonical WVB module identity.");
        }
        var Entries = fragment.Symbols.Where(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            Symbol.Name == "Main").ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted-container tool requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }

    private static void Requireˉverified(
        uint actualˉentry,
        uint expectedˉentry,
        ImmutableArray<byte> actualˉbundle,
        ImmutableArray<byte> expectedˉbundle,
        string platform,
        string description)
    {
        if (actualˉentry != expectedˉentry ||
            !actualˉbundle.AsSpan().SequenceEqual(expectedˉbundle.AsSpan()))
        {
            throw new InvalidDataException(
                $"The {platform} {description} did not reproduce its entry and service bundle.");
        }
    }
}
