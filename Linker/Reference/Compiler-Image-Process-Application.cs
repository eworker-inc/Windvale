using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Compilerˉimageˉprocessˉapplicationˉwriter
{
    internal static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes,
        string expectedˉmoduleˉname,
        int expectedˉmoduleˉbytes,
        string expectedˉmoduleˉsha256,
        int expectedˉapplicationˉbytes,
        string expectedˉapplicationˉsha256,
        string diagnosticˉcode,
        string label)
    {
        try
        {
            var Entry = Validateˉinput(
                module,
                fragment,
                moduleˉbytes,
                expectedˉmoduleˉname,
                expectedˉmoduleˉbytes,
                expectedˉmoduleˉsha256,
                label);
            var Bundle = X64ˉnativeˉserviceˉbundle
                .Buildˉhostedˉcompilerˉimageˉstaging(
                    fragment,
                    Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
                module.Module.Capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
            var Verified = Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
            Validateˉapplication(
                Verified.Nativeˉentryˉoffset,
                Entry,
                Verified.Bundleˉimage,
                Bundle,
                label);
            Requireˉidentity(
                Image.AsSpan(),
                expectedˉapplicationˉbytes,
                expectedˉapplicationˉsha256,
                "Windows",
                label);
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (Isˉconstructionˉfailure(Exception))
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                diagnosticˉcode,
                $"Windows {label} construction failed: {Exception.Message}");
        }
    }

    internal static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes,
        string expectedˉmoduleˉname,
        int expectedˉmoduleˉbytes,
        string expectedˉmoduleˉsha256,
        int expectedˉapplicationˉbytes,
        string expectedˉapplicationˉsha256,
        string diagnosticˉcode,
        string label)
    {
        try
        {
            var Entry = Validateˉinput(
                module,
                fragment,
                moduleˉbytes,
                expectedˉmoduleˉname,
                expectedˉmoduleˉbytes,
                expectedˉmoduleˉsha256,
                label);
            var Bundle = X64ˉnativeˉserviceˉbundle
                .Buildˉhostedˉcompilerˉimageˉstaging(
                    fragment,
                    Nativeˉserviceˉplatform.Linux);
            var Image = Linuxˉhostedˉcompilerˉapplicationˉbuilder.Build(
                module.Module.Capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
            var Verified = Linuxˉhostedˉcompilerˉapplicationˉverifier.Verify(
                Image.AsSpan(),
                Bundle,
                Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
            Validateˉapplication(
                Verified.Nativeˉentryˉoffset,
                Entry,
                Verified.Bundleˉimage,
                Bundle,
                label);
            Requireˉidentity(
                Image.AsSpan(),
                expectedˉapplicationˉbytes,
                expectedˉapplicationˉsha256,
                "Linux",
                label);
            return Linuxˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (Isˉconstructionˉfailure(Exception))
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                diagnosticˉcode,
                $"Linux {label} construction failed: {Exception.Message}");
        }
    }

    private static uint Validateˉinput(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes,
        string expectedˉmoduleˉname,
        int expectedˉmoduleˉbytes,
        string expectedˉmoduleˉsha256,
        string label)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(fragment);
        if (moduleˉbytes.Length != expectedˉmoduleˉbytes ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(moduleˉbytes),
                expectedˉmoduleˉsha256) ||
            !StringComparer.Ordinal.Equals(
                module.Module.Name,
                expectedˉmoduleˉname) ||
            module.Module.Profile != Moduleˉprofile.Hosted)
        {
            throw new ArgumentException(
                $"The {label} module identity is invalid.",
                nameof(module));
        }

        string[] Expectedˉcapabilities =
        [
            Capabilityˉcatalog.CONSOLE_WRITE_LINE,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE,
            Capabilityˉcatalog.FILE_READ_BYTES,
            Capabilityˉcatalog.FILE_WRITE_BYTES,
            Capabilityˉcatalog.PROCESS_ARGUMENT,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
        ];
        if (!module.Module.Capabilities.Select(Item => Item.Name)
            .SequenceEqual(Expectedˉcapabilities))
        {
            throw new ArgumentException(
                $"The {label} capability profile is invalid.",
                nameof(module));
        }

        Nativeˉfragmentˉverifier.Verify(fragment);
        Nativeˉservice[] Expectedˉservices =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Enumˉname,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.U32ˉformat,
            Nativeˉservice.Fileˉwriteˉbytes,
        ];
        if (!fragment.Requiredˉservices.SequenceEqual(Expectedˉservices))
        {
            throw new ArgumentException(
                $"The {label} service profile is invalid.",
                nameof(fragment));
        }

        return fragment.Symbols.Single(Item =>
            Item.Binding == Nativeˉsymbolˉbinding.Export &&
            Item.Kind == Nativeˉsymbolˉkind.Function &&
            Item.Name == "Main").Offset;
    }

    private static void Validateˉapplication(
        uint actualˉentry,
        uint expectedˉentry,
        ImmutableArray<byte> actualˉbundle,
        Nativeˉserviceˉbundle expectedˉbundle,
        string label)
    {
        if (actualˉentry != expectedˉentry ||
            !actualˉbundle.AsSpan().SequenceEqual(
                expectedˉbundle.Imageˉbytes.AsSpan()))
        {
            throw new InvalidDataException(
                $"The {label} package did not preserve its entry and service bundle.");
        }
    }

    private static void Requireˉidentity(
        ReadOnlySpan<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256,
        string platform,
        string label)
    {
        var Actualˉsha256 = Calculateˉsha256(bytes);
        if (bytes.Length != expectedˉbytes ||
            !StringComparer.Ordinal.Equals(Actualˉsha256, expectedˉsha256))
        {
            throw new InvalidDataException(
                $"The {platform} {label} application identity is invalid " +
                $"(bytes={bytes.Length}, sha256={Actualˉsha256}).");
        }
    }

    private static bool Isˉconstructionˉfailure(Exception exception) =>
        exception is
            ArgumentException or
            InvalidDataException or
            InvalidOperationException or
            OverflowException or
            Nativeˉbackendˉexception;

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
