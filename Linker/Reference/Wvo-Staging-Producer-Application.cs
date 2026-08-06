using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

public static class Wvoˉstagingˉproducerˉapplicationˉcontract
{
    public const string WINDOWS_TARGET_NAME =
        "windows-x64-wvo-staging-producer-v1";
    public const string LINUX_TARGET_NAME =
        "linux-x64-wvo-staging-producer-v1";
    public const string MODULE_NAME =
        "Compilerˉnativeˉx64ˉloweringˉstagingˉtool";
    public const int MODULE_BYTES = 394_780;
    public const string MODULE_SHA256 =
        "77158b228c204b587dbf559621ad7c717d4eb5b418c32b783204cd350525ac76";
    public const int WINDOWS_APPLICATION_BYTES = 5_723_136;
    public const string WINDOWS_APPLICATION_SHA256 =
        "993b2c5a531261cc5290e45edef0daa329de95b024f5ea749660895df84466de";
    public const int LINUX_APPLICATION_BYTES = 5_722_112;
    public const string LINUX_APPLICATION_SHA256 =
        "b38352b1e8d04bd3ac3f66e4ea27dde8391a738e9ce50031ad1f4927a53065d8";
}

public static class Wvoˉstagingˉproducerˉapplicationˉwriter
{
    public static Windowsˉconsoleˉapplicationˉresult Writeˉwindows(
        Verifiedˉmodule module,
        Nativeˉfragment fragment,
        ReadOnlySpan<byte> moduleˉbytes)
    {
        try
        {
            var Entry = Validateˉinput(module, fragment, moduleˉbytes);
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvbˉtoˉwvo(
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
            Validateˉapplication(Verified.Nativeˉentryˉoffset, Entry,
                Verified.Bundleˉimage, Bundle);
            Requireˉapplicationˉidentity(
                Image.AsSpan(),
                Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_APPLICATION_BYTES,
                Wvoˉstagingˉproducerˉapplicationˉcontract.WINDOWS_APPLICATION_SHA256,
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
                "WVW1601",
                $"Windows staged-WVO producer construction failed: {Exception.Message}");
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
            var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉwvbˉtoˉwvo(
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
            Validateˉapplication(Verified.Nativeˉentryˉoffset, Entry,
                Verified.Bundleˉimage, Bundle);
            Requireˉapplicationˉidentity(
                Image.AsSpan(),
                Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_APPLICATION_BYTES,
                Wvoˉstagingˉproducerˉapplicationˉcontract.LINUX_APPLICATION_SHA256,
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
                "WVL1601",
                $"Linux staged-WVO producer construction failed: {Exception.Message}");
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
                Wvoˉstagingˉproducerˉapplicationˉcontract.MODULE_BYTES ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(moduleˉbytes),
                Wvoˉstagingˉproducerˉapplicationˉcontract.MODULE_SHA256) ||
            !StringComparer.Ordinal.Equals(
                module.Module.Name,
                Wvoˉstagingˉproducerˉapplicationˉcontract.MODULE_NAME) ||
            module.Module.Profile != Moduleˉprofile.Hosted)
        {
            throw new ArgumentException(
                "The staged-WVO producer module identity is invalid.",
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
                "The staged-WVO producer capability profile is invalid.",
                nameof(module));
        }

        Nativeˉfragmentˉverifier.Verify(fragment);
        Nativeˉservice[] Expectedˉservices =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Textˉutf8ˉisˉvalid,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Enumˉname,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.U32ˉformat,
            Nativeˉservice.Fileˉwriteˉbytes,
        ];
        if (!fragment.Requiredˉservices.SequenceEqual(Expectedˉservices))
        {
            throw new ArgumentException(
                "The staged-WVO producer service profile is invalid.",
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
                "The staged-WVO producer package did not preserve its entry and service bundle.");
        }
    }

    internal static void Requireˉapplicationˉidentity(
        ReadOnlySpan<byte> bytes,
        int expectedˉbytes,
        string expectedˉsha256,
        string platform)
    {
        if (bytes.Length != expectedˉbytes ||
            !StringComparer.Ordinal.Equals(
                Calculateˉsha256(bytes),
                expectedˉsha256))
        {
            throw new InvalidDataException(
                $"The {platform} staged-WVO producer application identity is invalid.");
        }
    }

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
