using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Hostedˉwvaˉassemblerˉapplicationˉwriter
{
    private const string MODULE_NAME = "Wvaˉassemblerˉcore";

    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Nativeˉfragment fragment,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        string moduleˉname)
    {
        try
        {
            var Entry = Validateˉinput(fragment, moduleˉname);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvaˉassembler(
                fragment,
                Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler);
            var Verified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Windowsˉconsoleˉapplicationˉresult.Failed(
                    "WVW1702",
                    "The independently verified Windows WVA assembler did not reproduce its profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVW1701" : "WVW1702",
                $"Hosted Windows WVA assembler verification failed: {Exception.Message}");
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
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvaˉassembler(
                fragment,
                Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉcompilerˉapplicationˉbuilder.Build(
                capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler);
            var Verified = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler);
            if (Verified.Nativeˉentryˉoffset != Entry ||
                Verified.Runtime.Metadata.Profile !=
                    Hostedˉcompilerˉapplicationˉprofile.Wvaˉassembler ||
                !Verified.Bundleˉimage.AsSpan().SequenceEqual(Bundle.Imageˉbytes.AsSpan()))
            {
                return Linuxˉconsoleˉapplicationˉresult.Failed(
                    "WVL1702",
                    "The independently verified Linux WVA assembler did not reproduce its profile, entry, and service bundle.");
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
                Exception is InvalidOperationException ? "WVL1701" : "WVL1702",
                $"Hosted Linux WVA assembler verification failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(Nativeˉfragment fragment, string moduleˉname)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!StringComparer.Ordinal.Equals(moduleˉname, MODULE_NAME))
        {
            throw new InvalidOperationException(
                "The hosted WVA assembler requires its canonical WVB module identity.");
        }
        var Entries = fragment.Symbols
            .Where(Symbol => Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main")
            .ToArray();
        if (Entries.Length != 1)
        {
            throw new InvalidOperationException(
                "The hosted WVA assembler requires exactly one exported Main function.");
        }
        return Entries[0].Offset;
    }
}
