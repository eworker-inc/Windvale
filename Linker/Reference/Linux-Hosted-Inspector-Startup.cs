using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Linuxˉhostedˉinspectorˉstartup
{
    internal const int BYTES = 743;
    internal const string WVO_SHA256 =
        "5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb";
    internal const string TEMPLATE_SHA256 =
        "2c75cbfa34b188d8a5847ac462a00c9e3f799a44fb09b6959bd2b310a289f20f";

    private const string TEMPLATE_BASE64 =
        "SYnkMf++AAAABLoDAAAAQboiAAIATTHASYHoAQAAAEUxybgJAAAADwVIgfgB8P//D4OiAgAASInESIHEAAAABEyNPQAAAABIjQUAAAAASYmEJxgAAABIjQUAAAAASYmEJyAAAABIjQUAAAAASYmEJzAAAABIjQUAAAAASYmEJ0gAAABIjQUAAAAASYmEJ1gAAABIjQUAAAAASYmEJ2AAAABIjR0AAAAASI0FAAAAAEiJhCMQAAAASI0FAAAAAEiJhCMgAAAASI0FAAAAAEiJhCMwAAAASI0FAAAAAEiJhCNAAAAASI0dAAAAAEiNBQAAAABIiYQjCAAAAEiNBQAAAABIiYQjEAAAAEiNBQAAAABIiYQjGAAAAEiNBQAAAABIiYQjIAAAAEiNBQAAAABIiYQjKAAAAEiNBQAAAABIiYQjMAAAAEiNBQAAAABIiYQjOAAAAEiNBQAAAABIiYQjQAAAAEiNBQAAAABIiYQjSAAAAEiNBQAAAABIiYQjUAAAAEiNBQAAAABIiYQjWAAAAEmLhCQAAAAASIH4AQAAAA+CMwEAAEiB6AEAAABIgfhDAAAAD4cfAQAAQYmEJ1AAAAAx2zHtTI0tAAAAAEyNNQAAAABBi4QnUAAAADnDD4PEAAAASYu03BAAAAAxyYqUDgAAAACE0g+EFwAAAIH5ABAAAA+D0AAAAIHBAQAAAOna////QYnKieoByoH6AAABAA+HsgAAAEiB7BAAAABIieFJifBFidHoAAAAAIuEJAAAAABIgcQQAAAAgfgBAAAAD4WDAAAASInaSMHiBEwB6kyJ90gB70iJvCIAAAAARImUIggAAAAxyUQ50Q+DGQAAAIqEDgAAAACIhA8AAAAAgcEBAAAA6d7///9EAdWBwwEAAADpLP///0iNFQAAAAAx/zHJRTHARTHJ6AAAAABIicJIweoghdIPhQwAAACB+P8AAAAPhgUAAAC4AQAAAInHuDwAAAAPBcw=";

    private static readonly ImmutableArray<Startupˉpatch> PATCHES =
    [
        new(67, Startupˉtarget.Executionˉcontext),
        new(74, Startupˉtarget.Serviceˉtable),
        new(89, Startupˉtarget.Recordˉarena),
        new(104, Startupˉtarget.Textˉarena),
        new(119, Startupˉtarget.Argumentˉtable),
        new(134, Startupˉtarget.Outputˉtable),
        new(149, Startupˉtarget.Fileˉinputˉtable),
        new(164, Startupˉtarget.Fileˉinputˉtable),
        new(171, Startupˉtarget.Snapshotˉtable),
        new(186, Startupˉtarget.Nameˉarena),
        new(201, Startupˉtarget.Dataˉarena),
        new(216, Startupˉtarget.Fileˉinputˉscratch),
        new(231, Startupˉtarget.Serviceˉtable),
        new(238, Startupˉtarget.Consoleˉwrite),
        new(253, Startupˉtarget.Argumentˉcount),
        new(268, Startupˉtarget.Argument),
        new(283, Startupˉtarget.Fileˉread),
        new(298, Startupˉtarget.Utf8),
        new(313, Startupˉtarget.Diagnosticˉwrite),
        new(328, Startupˉtarget.Enumˉname),
        new(343, Startupˉtarget.Textˉconcat),
        new(358, Startupˉtarget.Textˉquote),
        new(373, Startupˉtarget.I32ˉformat),
        new(388, Startupˉtarget.U32ˉformat),
        new(456, Startupˉtarget.Argumentˉtable),
        new(463, Startupˉtarget.Argumentˉbytes),
        new(567, Startupˉtarget.Utf8),
        new(682, Startupˉtarget.Executionˉcontext),
        new(697, Startupˉtarget.Nativeˉmain),
    ];

    internal static ImmutableArray<byte> Build(
        uint startupˉaddress,
        uint dataˉaddress,
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset, profile);
        var Bytes = Decodeˉtemplate();
        foreach (var Patch in PATCHES)
        {
            var Target = Address(Patch.Target, dataˉaddress, layout, bundle, nativeˉentryˉoffset);
            var Sourceˉend = checked(startupˉaddress + (uint)Patch.Offset + sizeof(int));
            BinaryPrimitives.WriteInt32LittleEndian(
                Bytes.AsSpan(Patch.Offset, sizeof(int)),
                checked((int)((long)Target - Sourceˉend)));
        }
        return Bytes.ToImmutableArray();
    }

    internal static void Verify(
        ReadOnlySpan<byte> bytes,
        uint startupˉaddress,
        uint dataˉaddress,
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset, profile);
        if (bytes.Length != BYTES)
        {
            throw Invalid("The Linux hosted-inspector startup has an invalid size.");
        }

        var Unpatched = bytes.ToArray();
        foreach (var Patch in PATCHES)
        {
            var Sourceˉend = checked(startupˉaddress + (uint)Patch.Offset + sizeof(int));
            var Actual = checked((long)Sourceˉend +
                BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(Patch.Offset, sizeof(int))));
            var Expected = Address(
                Patch.Target,
                dataˉaddress,
                layout,
                bundle,
                nativeˉentryˉoffset);
            if (Actual != Expected)
            {
                throw Invalid($"The Linux hosted-inspector {Patch.Target} target is invalid.");
            }
            Unpatched.AsSpan(Patch.Offset, sizeof(int)).Clear();
        }

        var Template = Decodeˉtemplate();
        if (!Unpatched.AsSpan().SequenceEqual(Template) ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Unpatched), TEMPLATE_SHA256))
        {
            throw Invalid("The Linux hosted-inspector startup template is noncanonical.");
        }
    }

    private static byte[] Decodeˉtemplate()
    {
        var Template = Convert.FromBase64String(TEMPLATE_BASE64);
        if (Template.Length != BYTES ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Template), TEMPLATE_SHA256))
        {
            throw new InvalidOperationException(
                "The retained Linux hosted-inspector startup template is corrupt.");
        }
        return Template;
    }

    internal static ImmutableArray<byte> Templateˉforˉverification() =>
        Decodeˉtemplate().ToImmutableArray();

    private static uint Address(
        Startupˉtarget target,
        uint dataˉaddress,
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset) => target switch
        {
            Startupˉtarget.Executionˉcontext =>
                checked(dataˉaddress + Hostedˉverifierˉruntimeˉdata.CONTEXT_OFFSET),
            Startupˉtarget.Serviceˉtable =>
                checked(dataˉaddress + Hostedˉverifierˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            Startupˉtarget.Recordˉarena => checked(dataˉaddress + layout.Recordˉarenaˉoffset),
            Startupˉtarget.Textˉarena => checked(dataˉaddress + layout.Textˉarenaˉoffset),
            Startupˉtarget.Argumentˉtable => checked(dataˉaddress + layout.Argumentˉtableˉoffset),
            Startupˉtarget.Argumentˉbytes => checked(dataˉaddress + layout.Argumentˉbytesˉoffset),
            Startupˉtarget.Outputˉtable =>
                checked(dataˉaddress + Hostedˉverifierˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            Startupˉtarget.Fileˉinputˉtable =>
                checked(dataˉaddress + Hostedˉverifierˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            Startupˉtarget.Snapshotˉtable => checked(dataˉaddress + layout.Snapshotˉtableˉoffset),
            Startupˉtarget.Nameˉarena => checked(dataˉaddress + layout.Nameˉarenaˉoffset),
            Startupˉtarget.Dataˉarena => checked(dataˉaddress + layout.Dataˉarenaˉoffset),
            Startupˉtarget.Fileˉinputˉscratch =>
                checked(dataˉaddress + layout.Fileˉinputˉscratchˉoffset),
            Startupˉtarget.Nativeˉmain => checked(
                Linuxˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS +
                Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
                nativeˉentryˉoffset),
            Startupˉtarget.Consoleˉwrite => Serviceˉaddress(
                bundle, Nativeˉservice.Consoleˉwriteˉline),
            Startupˉtarget.Argumentˉcount => Serviceˉaddress(
                bundle, Nativeˉservice.Processˉargumentˉcount),
            Startupˉtarget.Argument => Serviceˉaddress(bundle, Nativeˉservice.Processˉargument),
            Startupˉtarget.Fileˉread => Serviceˉaddress(bundle, Nativeˉservice.Fileˉreadˉbytes),
            Startupˉtarget.Utf8 => Serviceˉaddress(bundle, Nativeˉservice.Textˉutf8ˉisˉvalid),
            Startupˉtarget.Diagnosticˉwrite => Serviceˉaddress(
                bundle, Nativeˉservice.Diagnosticˉwriteˉline),
            Startupˉtarget.Enumˉname => Serviceˉaddress(bundle, Nativeˉservice.Enumˉname),
            Startupˉtarget.Textˉconcat => Serviceˉaddress(bundle, Nativeˉservice.Textˉconcat),
            Startupˉtarget.Textˉquote => Serviceˉaddress(bundle, Nativeˉservice.Textˉquote),
            Startupˉtarget.I32ˉformat => Serviceˉaddress(bundle, Nativeˉservice.I32ˉformat),
            Startupˉtarget.U32ˉformat => Serviceˉaddress(bundle, Nativeˉservice.U32ˉformat),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static uint Serviceˉaddress(
        Nativeˉserviceˉbundle bundle,
        Nativeˉservice service)
    {
        var Placement = bundle.Placements.SingleOrDefault(Item => Item.Service == service);
        if (Placement is null)
        {
            return 0;
        }
        return checked(
            Linuxˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS +
            Linuxˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)Placement.Imageˉoffset);
    }

    private static void Validateˉinputs(
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile)
    {
        var Services = Hostedˉverifierˉapplicationˉmetadata.Requiredˉservices(profile);
        if (layout.Target != Consoleˉapplicationˉtarget.Linuxˉx64 ||
            profile is not (Hostedˉverifierˉapplicationˉprofile.Wvbˉinspector or
                Hostedˉverifierˉapplicationˉprofile.Wvbˉrunner or
                Hostedˉverifierˉapplicationˉprofile.Wvoˉinspector) ||
            bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Linux ||
            !bundle.Placements.Select(Placement => Placement.Service).SequenceEqual(Services) ||
            nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentException("The Linux hosted-inspector startup inputs are invalid.");
        }
    }

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static InvalidDataException Invalid(string message) => new(message);

    private sealed record Startupˉpatch(int Offset, Startupˉtarget Target);

    private enum Startupˉtarget
    {
        Executionˉcontext,
        Serviceˉtable,
        Recordˉarena,
        Textˉarena,
        Argumentˉtable,
        Argumentˉbytes,
        Outputˉtable,
        Fileˉinputˉtable,
        Snapshotˉtable,
        Nameˉarena,
        Dataˉarena,
        Fileˉinputˉscratch,
        Nativeˉmain,
        Consoleˉwrite,
        Argumentˉcount,
        Argument,
        Fileˉread,
        Utf8,
        Diagnosticˉwrite,
        Enumˉname,
        Textˉconcat,
        Textˉquote,
        I32ˉformat,
        U32ˉformat,
    }
}
