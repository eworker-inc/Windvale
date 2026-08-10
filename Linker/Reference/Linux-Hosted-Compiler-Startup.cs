using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Linuxˉhostedˉcompilerˉstartup
{
    internal const int BYTES = 809;
    internal const int WVO_BYTES = 2_454;
    internal const int SYMBOL_COUNT = 26;
    internal const string WVO_SHA256 =
        "1b8c08308d3f7320b741ae86022400ced6748352314b7f27954ec1c5a7345946";
    internal const string TEMPLATE_SHA256 =
        "04e637e82fc121d66de981ca9edfe53259dfb518dd8a813c415e89ccfbf352d0";

    private const string TEMPLATE_BASE64 =
        "SYnkMf++AAAABLoDAAAAQboiAAIATTHASYHoAQAAAEUxybgJAAAADwVIgfgB8P//D4PkAgAASInESIHEAAAABEyNPQAAAABIjQUAAAAASYmEJxgAAABIjQUAAAAASYmEJyAAAABIjQUAAAAASYmEJzAAAABIjQUAAAAASYmEJ0gAAABIjQUAAAAASYmEJ1gAAABIjQUAAAAASYmEJ2AAAABIjQUAAAAASYmEJ2gAAABIjR0AAAAASI0FAAAAAEiJhCMQAAAASI0FAAAAAEiJhCMgAAAASI0FAAAAAEiJhCMwAAAASI0FAAAAAEiJhCNAAAAASI0dAAAAAEiNBQAAAABIiYQjEAAAAEiNHQAAAABIjQUAAAAASImEIwgAAABIjQUAAAAASImEIxAAAABIjQUAAAAASImEIxgAAABIjQUAAAAASImEIyAAAABIjQUAAAAASImEIygAAABIjQUAAAAASImEIzAAAABIjQUAAAAASImEIzgAAABIjQUAAAAASImEI0AAAABIjQUAAAAASImEI1gAAABIjQUAAAAASImEI2AAAABJi4QkAAAAAEiB+AEAAAAPgl8BAABIgegBAAAASIH4QwAAAA+HSwEAAEGJhCdQAAAAMdsx7UyNLQAAAABMjTUAAAAAQYuEJ1AAAAA5ww+DxAAAAEmLtNwQAAAAMcmKlA4AAAAAhNIPhBcAAACB+QAQAAAPg/wAAACBwQEAAADp2v///0GJyonqAcqB+gAAAQAPh94AAABIgewQAAAASInhSYnwRYnR6AAAAACLhCQAAAAASIHEEAAAAIH4AQAAAA+FrwAAAEiJ2kjB4gRMAepMifdIAe9IibwiAAAAAESJlCIIAAAAMclEOdEPgxkAAACKhA4AAAAAiIQPAAAAAIHBAQAAAOne////RAHVgcMBAAAA6Sz///9IjRUAAAAAMf8xyUUxwEUxyegAAAAASInCSMHqIIXSD4QsAAAAgfoFAAAAD4UZAAAASI0NAAAAAIuEIUAAAACBwEAAAADpGAAAAInQ6REAAACB+P8AAAAPhgUAAAC4AQAAAInHuDwAAAAPBcw=";

    private static readonly ImmutableArray<Startupˉpatch> PATCHES =
    [
        new(67, Startupˉtarget.Executionˉcontext),
        new(74, Startupˉtarget.Serviceˉtable),
        new(89, Startupˉtarget.Recordˉarena),
        new(104, Startupˉtarget.Textˉarena),
        new(119, Startupˉtarget.Argumentˉtable),
        new(134, Startupˉtarget.Outputˉtable),
        new(149, Startupˉtarget.Fileˉinputˉtable),
        new(164, Startupˉtarget.Fileˉoutputˉtable),
        new(179, Startupˉtarget.Fileˉinputˉtable),
        new(186, Startupˉtarget.Snapshotˉtable),
        new(201, Startupˉtarget.Nameˉarena),
        new(216, Startupˉtarget.Dataˉarena),
        new(231, Startupˉtarget.Fileˉinputˉscratch),
        new(246, Startupˉtarget.Fileˉoutputˉtable),
        new(253, Startupˉtarget.Fileˉoutputˉscratch),
        new(268, Startupˉtarget.Serviceˉtable),
        new(275, Startupˉtarget.Consoleˉwrite),
        new(290, Startupˉtarget.Argumentˉcount),
        new(305, Startupˉtarget.Argument),
        new(320, Startupˉtarget.Fileˉread),
        new(335, Startupˉtarget.Utf8),
        new(350, Startupˉtarget.Diagnosticˉwrite),
        new(365, Startupˉtarget.Enumˉname),
        new(380, Startupˉtarget.Textˉconcat),
        new(395, Startupˉtarget.U32ˉformat),
        new(410, Startupˉtarget.Fileˉwrite),
        new(478, Startupˉtarget.Argumentˉtable),
        new(485, Startupˉtarget.Argumentˉbytes),
        new(589, Startupˉtarget.Utf8),
        new(704, Startupˉtarget.Executionˉcontext),
        new(719, Startupˉtarget.Nativeˉmain),
        new(753, Startupˉtarget.Executionˉcontext),
    ];

    internal static ImmutableArray<byte> Build(
        uint startupˉaddress,
        uint dataˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        var Inputs = Buildˉinputs(
            startupˉaddress,
            dataˉaddress,
            layout,
            bundle,
            nativeˉentryˉoffset);
        var Bytes = Nativeˉhostedˉstartupˉinstantiator.Build(Inputs);
        Verify(
            Bytes.AsSpan(),
            startupˉaddress,
            dataˉaddress,
            layout,
            bundle,
            nativeˉentryˉoffset);
        return Bytes;
    }

    internal static Nativeˉhostedˉstartupˉinputs Buildˉinputs(
        uint startupˉaddress,
        uint dataˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        var Targets = PATCHES.Select(Patch => Address(
            Patch.Target,
            dataˉaddress,
            layout,
            bundle,
            nativeˉentryˉoffset)).ToImmutableArray();
        return new(
            startupˉaddress,
            BYTES,
            SYMBOL_COUNT,
            Targets,
            Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Linuxˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Linux-X64-Hosted-Compiler.wvo",
                WVO_BYTES,
                WVO_SHA256));
    }

    internal static ImmutableArray<byte> Buildˉstage0(
        uint startupˉaddress,
        uint dataˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
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
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        if (bytes.Length != BYTES)
        {
            throw Invalid("The Linux hosted-compiler startup has an invalid size.");
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
                throw Invalid($"The Linux hosted-compiler {Patch.Target} target is invalid.");
            }
            Unpatched.AsSpan(Patch.Offset, sizeof(int)).Clear();
        }

        var Template = Decodeˉtemplate();
        if (!Unpatched.AsSpan().SequenceEqual(Template) ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Unpatched), TEMPLATE_SHA256))
        {
            var Difference = Enumerable.Range(0, Unpatched.Length)
                .FirstOrDefault(Index => Unpatched[Index] != Template[Index], -1);
            throw Invalid(
                Difference < 0
                    ? "The Linux hosted-compiler startup template digest is noncanonical."
                    : $"The Linux hosted-compiler startup template is noncanonical at " +
                        $"offset {Difference}.");
        }
    }

    private static byte[] Decodeˉtemplate()
    {
        var Template = Convert.FromBase64String(TEMPLATE_BASE64);
        if (Template.Length != BYTES ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Template), TEMPLATE_SHA256))
        {
            throw new InvalidOperationException(
                "The retained Linux hosted-compiler startup template is corrupt.");
        }
        return Template;
    }

    private static uint Address(
        Startupˉtarget target,
        uint dataˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset) => target switch
        {
            Startupˉtarget.Executionˉcontext =>
                checked(dataˉaddress + Hostedˉcompilerˉruntimeˉdata.CONTEXT_OFFSET),
            Startupˉtarget.Serviceˉtable =>
                checked(dataˉaddress + Hostedˉcompilerˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            Startupˉtarget.Recordˉarena => checked(dataˉaddress + layout.Recordˉarenaˉoffset),
            Startupˉtarget.Textˉarena => checked(dataˉaddress + layout.Textˉarenaˉoffset),
            Startupˉtarget.Argumentˉtable => checked(dataˉaddress + layout.Argumentˉtableˉoffset),
            Startupˉtarget.Argumentˉbytes => checked(dataˉaddress + layout.Argumentˉbytesˉoffset),
            Startupˉtarget.Outputˉtable =>
                checked(dataˉaddress + Hostedˉcompilerˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            Startupˉtarget.Fileˉinputˉtable =>
                checked(dataˉaddress + Hostedˉcompilerˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            Startupˉtarget.Fileˉoutputˉtable =>
                checked(dataˉaddress + Hostedˉcompilerˉruntimeˉdata.FILE_OUTPUT_TABLE_OFFSET),
            Startupˉtarget.Snapshotˉtable => checked(dataˉaddress + layout.Snapshotˉtableˉoffset),
            Startupˉtarget.Nameˉarena => checked(dataˉaddress + layout.Nameˉarenaˉoffset),
            Startupˉtarget.Dataˉarena => checked(dataˉaddress + layout.Dataˉarenaˉoffset),
            Startupˉtarget.Fileˉinputˉscratch =>
                checked(dataˉaddress + layout.Fileˉinputˉscratchˉoffset),
            Startupˉtarget.Fileˉoutputˉscratch =>
                checked(dataˉaddress + layout.Fileˉoutputˉscratchˉoffset),
            Startupˉtarget.Nativeˉmain => checked(
                Linuxˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS +
                Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
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
            Startupˉtarget.U32ˉformat => Serviceˉaddress(bundle, Nativeˉservice.U32ˉformat),
            Startupˉtarget.Fileˉwrite => Serviceˉaddress(bundle, Nativeˉservice.Fileˉwriteˉbytes),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static uint Serviceˉaddress(
        Nativeˉserviceˉbundle bundle,
        Nativeˉservice service)
    {
        var Placement = bundle.Placements.Single(Item => Item.Service == service);
        return checked(
            Linuxˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS +
            Linuxˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)Placement.Imageˉoffset);
    }

    private static void Validateˉinputs(
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        if (layout.Target != Consoleˉapplicationˉtarget.Linuxˉx64 ||
            bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Linux ||
            bundle.Placements.Length != Hostedˉcompilerˉapplicationˉmetadata.SERVICE_COUNT ||
            nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentException("The Linux hosted-compiler startup inputs are invalid.");
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
        Fileˉoutputˉtable,
        Snapshotˉtable,
        Nameˉarena,
        Dataˉarena,
        Fileˉinputˉscratch,
        Fileˉoutputˉscratch,
        Nativeˉmain,
        Consoleˉwrite,
        Argumentˉcount,
        Argument,
        Fileˉread,
        Utf8,
        Diagnosticˉwrite,
        Enumˉname,
        Textˉconcat,
        U32ˉformat,
        Fileˉwrite,
    }
}
