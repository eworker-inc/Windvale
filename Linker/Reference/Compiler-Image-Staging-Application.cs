using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Compilerˉimageˉstagingˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-compiler-image-staging-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-compiler-image-staging-v1";
    public const string MODULE_NAME =
        "Linkerˉcompilerˉwvoˉsegmentedˉflatˉimageˉstagingˉtool";
    public const int MODULE_BYTES = 75_337;
    public const string MODULE_SHA256 =
        "855983284c088cd795c119fe0c392308824066b10a9173dceb7cdc2daa219101";
    public const int WINDOWS_APPLICATION_BYTES = 849_920;
    public const string WINDOWS_APPLICATION_SHA256 =
        "c6315f74f0a674e8d0cbb6e64e80c97d409a500551f51b6ce3d7fa618ca00f6e";
    public const int LINUX_APPLICATION_BYTES = 851_968;
    public const string LINUX_APPLICATION_SHA256 =
        "f93db63052605ebb61ce934b351ad45fe7386d134325af8e1a8abb93bc64dd9f";
}

public static class Compilerˉimageˉstagingˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Entry = Validateˉinput(module, fragment, moduleˉbytes);
            var Bundle =
                X64ˉnativeˉserviceˉbundle
                    .Buildˉhostedˉcompilerˉimageˉstaging(
                        fragment,
                        Nativeˉserviceˉplatform.Windows);
            var Image = Windowsˉhostedˉcompilerˉapplicationˉbuilder.Build(
                module.Module.Capabilities,
                Bundle,
                Entry,
                Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
            var Verified =
                Windowsˉhostedˉcompilerˉapplicationˉverifier.Verify(
                    Image.AsSpan(),
                    Bundle,
                    Hostedˉcompilerˉapplicationˉprofile.Wvbˉtoˉwvo);
            Validateˉapplication(
                Verified.Nativeˉentryˉoffset,
                Entry,
                Verified.Bundleˉimage,
                Bundle);
            Requireˉidentity(
                Image.AsSpan(),
                Compilerˉimageˉstagingˉapplicationˉcontract
                    .WINDOWS_APPLICATION_BYTES,
                Compilerˉimageˉstagingˉapplicationˉcontract
                    .WINDOWS_APPLICATION_SHA256,
                "Windows");
            return Windowsˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (Exception is
            ArgumentException or
            InvalidDataException or
            InvalidOperationException or
            OverflowException or
            Nativeˉbackendˉexception)
        {
            return Windowsˉconsoleˉapplicationˉresult.Failed(
                "WVW3501",
                "Windows compiler-image staging construction failed: " +
                    Exception.Message);
        }
    }

    public static Linuxˉconsoleˉapplicationˉresult Writeˉlinux(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Entry = Validateˉinput(module, fragment, moduleˉbytes);
            var Bundle =
                X64ˉnativeˉserviceˉbundle
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
                Bundle);
            Requireˉidentity(
                Image.AsSpan(),
                Compilerˉimageˉstagingˉapplicationˉcontract
                    .LINUX_APPLICATION_BYTES,
                Compilerˉimageˉstagingˉapplicationˉcontract
                    .LINUX_APPLICATION_SHA256,
                "Linux");
            return Linuxˉconsoleˉapplicationˉresult.Succeeded(Image);
        }
        catch (Exception Exception) when (Exception is
            ArgumentException or
            InvalidDataException or
            InvalidOperationException or
            OverflowException or
            Nativeˉbackendˉexception)
        {
            return Linuxˉconsoleˉapplicationˉresult.Failed(
                "WVL3501",
                "Linux compiler-image staging construction failed: " +
                    Exception.Message);
        }
    }

    private static uint Validateˉinput(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(fragment);
        if (moduleˉbytes.Length !=
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_BYTES ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(moduleˉbytes),
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_SHA256) ||
            !StringComparer.Ordinal.Equals(
                module.Module.Name,
                Compilerˉimageˉstagingˉapplicationˉcontract.MODULE_NAME) ||
            module.Module.Profile != Moduleˉprofile.Hosted)
        {
            throw new ArgumentException(
                "The compiler-image staging module identity is invalid.",
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
                "The compiler-image staging capability profile is invalid.",
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
                "The compiler-image staging service profile is invalid.",
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
        Nativeˉserviceˉbundle expectedˉbundle)
    {
        if (actualˉentry != expectedˉentry ||
            !actualˉbundle.AsSpan().SequenceEqual(
                expectedˉbundle.Imageˉbytes.AsSpan()))
        {
            throw new InvalidDataException(
                "The compiler-image staging package did not preserve its entry and service bundle.");
        }
    }

    private static void Requireˉidentity(
        ReadOnlySpan<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256,
        string platform)
    {
        var Actualˉsha256 = Calculateˉsha256(bytes);
        if (bytes.Length != expectedˉbytes ||
            !StringComparer.Ordinal.Equals(Actualˉsha256, expectedˉsha256))
        {
            throw new InvalidDataException(
                $"The {platform} compiler-image staging application identity is invalid " +
                $"(bytes={bytes.Length}, sha256={Actualˉsha256}).");
        }
    }

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
