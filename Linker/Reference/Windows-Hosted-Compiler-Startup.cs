using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Windowsˉhostedˉcompilerˉstartup
{
    internal const int BYTES = 1510;
    internal const int WVO_BYTES = 4_334;
    internal const int SYMBOL_COUNT = 40;
    internal const string WVO_SHA256 =
        "55f4782e976038c2d68bb91aeabb75518103524e9d5caaf1cc9f0662ab5a0feb";
    internal const string TEMPLATE_SHA256 =
        "59a3f3b794c5b81bde8385aab77d86fae01bfc0c728bc5f412459cff5eb7310a";
    private const int LOCAL_COMMIT_RELOCATION_INDEX = 18;
    private const uint LOCAL_COMMIT_OFFSET = 1320;

    private const string TEMPLATE_BASE64 =
        "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";

    private static readonly ImmutableArray<Startupˉpatch> PATCHES =
    [
        new(25, Startupˉtarget.Executionˉcontext),
        new(32, Startupˉtarget.Serviceˉtable),
        new(47, Startupˉtarget.Recordˉarena),
        new(62, Startupˉtarget.Textˉarena),
        new(77, Startupˉtarget.Argumentˉtable),
        new(92, Startupˉtarget.Outputˉtable),
        new(107, Startupˉtarget.Fileˉinputˉtable),
        new(122, Startupˉtarget.Fileˉoutputˉtable),
        new(137, Startupˉtarget.Fileˉinputˉtable),
        new(144, Startupˉtarget.Snapshotˉtable),
        new(159, Startupˉtarget.Nameˉarena),
        new(174, Startupˉtarget.Dataˉarena),
        new(189, Startupˉtarget.Fileˉinputˉscratch),
        new(204, Startupˉtarget.Multiˉbyteˉtoˉwideˉchar),
        new(227, Startupˉtarget.Createˉfile),
        new(250, Startupˉtarget.Getˉfileˉsize),
        new(273, Startupˉtarget.Readˉfile),
        new(296, Startupˉtarget.Closeˉhandle),
        new(334, Startupˉtarget.Getˉlastˉerror),
        new(357, Startupˉtarget.Fileˉoutputˉtable),
        new(364, Startupˉtarget.Fileˉoutputˉscratch),
        new(379, Startupˉtarget.Multiˉbyteˉtoˉwideˉchar),
        new(402, Startupˉtarget.Createˉfile),
        new(425, Startupˉtarget.Writeˉfile),
        new(448, Startupˉtarget.Flushˉfileˉbuffers),
        new(471, Startupˉtarget.Closeˉhandle),
        new(494, Startupˉtarget.Getˉlastˉerror),
        new(522, Startupˉtarget.Getˉstdˉhandle),
        new(539, Startupˉtarget.Outputˉtable),
        new(559, Startupˉtarget.Getˉstdˉhandle),
        new(584, Startupˉtarget.Writeˉfile),
        new(607, Startupˉtarget.Serviceˉtable),
        new(614, Startupˉtarget.Consoleˉwrite),
        new(629, Startupˉtarget.Argumentˉcount),
        new(644, Startupˉtarget.Argument),
        new(659, Startupˉtarget.Fileˉread),
        new(674, Startupˉtarget.Utf8),
        new(689, Startupˉtarget.Diagnosticˉwrite),
        new(704, Startupˉtarget.Enumˉname),
        new(719, Startupˉtarget.Textˉconcat),
        new(734, Startupˉtarget.U32ˉformat),
        new(749, Startupˉtarget.Fileˉwrite),
        new(764, Startupˉtarget.Getˉcommandˉline),
        new(803, Startupˉtarget.Commandˉlineˉtoˉargv),
        new(881, Startupˉtarget.Argumentˉtable),
        new(888, Startupˉtarget.Argumentˉbytes),
        new(895, Startupˉtarget.Fileˉinputˉscratch),
        new(984, Startupˉtarget.Wideˉcharˉtoˉmultiˉbyte),
        new(1073, Startupˉtarget.Utf8),
        new(1192, Startupˉtarget.Localˉfree),
        new(1221, Startupˉtarget.Executionˉcontext),
        new(1235, Startupˉtarget.Nativeˉmain),
        new(1281, Startupˉtarget.Localˉfree),
        new(1349, Startupˉtarget.Dataˉarena),
        new(1365, Startupˉtarget.Nameˉarena),
        new(1432, Startupˉtarget.Dataˉarena),
        new(1490, Startupˉtarget.Fileˉinputˉscratch),
    ];

    internal static ImmutableArray<byte> Build(
        uint startupˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        var Inputs = Buildˉinputs(
            startupˉaddress,
            importˉaddress,
            runtimeˉaddress,
            layout,
            bundle,
            nativeˉentryˉoffset);
        var Bytes = Nativeˉhostedˉstartupˉinstantiator.Build(Inputs);
        Verify(
            Bytes.AsSpan(),
            startupˉaddress,
            importˉaddress,
            runtimeˉaddress,
            layout,
            bundle,
            nativeˉentryˉoffset);
        return Bytes;
    }

    internal static Nativeˉhostedˉstartupˉinputs Buildˉinputs(
        uint startupˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        var Targets = PATCHES.Select(Patch => Address(
            Patch.Target,
            importˉaddress,
            runtimeˉaddress,
            layout,
            bundle,
            nativeˉentryˉoffset)).ToList();
        Targets.Insert(
            LOCAL_COMMIT_RELOCATION_INDEX,
            checked(startupˉaddress + LOCAL_COMMIT_OFFSET));
        return new(
            startupˉaddress,
            BYTES,
            SYMBOL_COUNT,
            Targets.ToImmutableArray(),
            Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Windowsˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Windows-X64-Hosted-Compiler.wvo",
                WVO_BYTES,
                WVO_SHA256));
    }

    internal static ImmutableArray<byte> Buildˉstage0(
        uint startupˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        var Bytes = Decodeˉtemplate();
        foreach (var Patch in PATCHES)
        {
            var Target = Address(
                Patch.Target,
                importˉaddress,
                runtimeˉaddress,
                layout,
                bundle,
                nativeˉentryˉoffset);
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
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        if (bytes.Length != BYTES)
        {
            throw Invalid("The Windows hosted-compiler startup has an invalid size.");
        }

        var Unpatched = bytes.ToArray();
        foreach (var Patch in PATCHES)
        {
            var Sourceˉend = checked(startupˉaddress + (uint)Patch.Offset + sizeof(int));
            var Actual = checked((long)Sourceˉend +
                BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(Patch.Offset, sizeof(int))));
            var Expected = Address(
                Patch.Target,
                importˉaddress,
                runtimeˉaddress,
                layout,
                bundle,
                nativeˉentryˉoffset);
            if (Actual != Expected)
            {
                throw Invalid($"The Windows hosted-compiler {Patch.Target} target is invalid.");
            }
            Unpatched.AsSpan(Patch.Offset, sizeof(int)).Clear();
        }

        var Template = Decodeˉtemplate();
        if (!Unpatched.AsSpan().SequenceEqual(Template) ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Unpatched), TEMPLATE_SHA256))
        {
            throw Invalid("The Windows hosted-compiler startup template is noncanonical.");
        }
    }

    private static byte[] Decodeˉtemplate()
    {
        var Template = Convert.FromBase64String(TEMPLATE_BASE64);
        if (Template.Length != BYTES ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Template), TEMPLATE_SHA256))
        {
            throw new InvalidOperationException(
                "The retained Windows hosted-compiler startup template is corrupt.");
        }
        return Template;
    }

    private static uint Address(
        Startupˉtarget target,
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset) => target switch
        {
            Startupˉtarget.Executionˉcontext =>
                checked(runtimeˉaddress + Hostedˉcompilerˉruntimeˉdata.CONTEXT_OFFSET),
            Startupˉtarget.Serviceˉtable =>
                checked(runtimeˉaddress + Hostedˉcompilerˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            Startupˉtarget.Recordˉarena => checked(runtimeˉaddress + layout.Recordˉarenaˉoffset),
            Startupˉtarget.Textˉarena => checked(runtimeˉaddress + layout.Textˉarenaˉoffset),
            Startupˉtarget.Argumentˉtable => checked(runtimeˉaddress + layout.Argumentˉtableˉoffset),
            Startupˉtarget.Argumentˉbytes => checked(runtimeˉaddress + layout.Argumentˉbytesˉoffset),
            Startupˉtarget.Outputˉtable =>
                checked(runtimeˉaddress + Hostedˉcompilerˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            Startupˉtarget.Fileˉinputˉtable =>
                checked(runtimeˉaddress + Hostedˉcompilerˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            Startupˉtarget.Fileˉoutputˉtable =>
                checked(runtimeˉaddress + Hostedˉcompilerˉruntimeˉdata.FILE_OUTPUT_TABLE_OFFSET),
            Startupˉtarget.Snapshotˉtable => checked(runtimeˉaddress + layout.Snapshotˉtableˉoffset),
            Startupˉtarget.Nameˉarena => checked(runtimeˉaddress + layout.Nameˉarenaˉoffset),
            Startupˉtarget.Dataˉarena => checked(runtimeˉaddress + layout.Dataˉarenaˉoffset),
            Startupˉtarget.Fileˉinputˉscratch =>
                checked(runtimeˉaddress + layout.Fileˉinputˉscratchˉoffset),
            Startupˉtarget.Fileˉoutputˉscratch =>
                checked(runtimeˉaddress + layout.Fileˉoutputˉscratchˉoffset),
            Startupˉtarget.Nativeˉmain => checked(
                Windowsˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS +
                Windowsˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
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
            Startupˉtarget.Closeˉhandle => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.CLOSE_HANDLE_IAT_OFFSET),
            Startupˉtarget.Commandˉlineˉtoˉargv => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.COMMAND_LINE_TO_ARGV_IAT_OFFSET),
            Startupˉtarget.Createˉfile => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.CREATE_FILE_IAT_OFFSET),
            Startupˉtarget.Flushˉfileˉbuffers => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.FLUSH_FILE_BUFFERS_IAT_OFFSET),
            Startupˉtarget.Getˉcommandˉline => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.GET_COMMAND_LINE_IAT_OFFSET),
            Startupˉtarget.Getˉfileˉsize => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.GET_FILE_SIZE_IAT_OFFSET),
            Startupˉtarget.Getˉlastˉerror => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.GET_LAST_ERROR_IAT_OFFSET),
            Startupˉtarget.Getˉstdˉhandle => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.GET_STD_HANDLE_IAT_OFFSET),
            Startupˉtarget.Localˉfree => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.LOCAL_FREE_IAT_OFFSET),
            Startupˉtarget.Multiˉbyteˉtoˉwideˉchar => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.MULTI_BYTE_TO_WIDE_CHAR_IAT_OFFSET),
            Startupˉtarget.Readˉfile => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.READ_FILE_IAT_OFFSET),
            Startupˉtarget.Wideˉcharˉtoˉmultiˉbyte => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.WIDE_CHAR_TO_MULTI_BYTE_IAT_OFFSET),
            Startupˉtarget.Writeˉfile => checked(importˉaddress +
                Windowsˉhostedˉcompilerˉimports.WRITE_FILE_IAT_OFFSET),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static uint Serviceˉaddress(
        Nativeˉserviceˉbundle bundle,
        Nativeˉservice service)
    {
        var Placement = bundle.Placements.Single(Item => Item.Service == service);
        return checked(
            Windowsˉhostedˉcompilerˉapplicationˉcontract.TEXT_ADDRESS +
            Windowsˉhostedˉcompilerˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)Placement.Imageˉoffset);
    }

    private static void Validateˉinputs(
        Hostedˉcompilerˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        if (layout.Target != Consoleˉapplicationˉtarget.Windowsˉx64 ||
            bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Windows ||
            bundle.Placements.Length != Hostedˉcompilerˉapplicationˉmetadata.SERVICE_COUNT ||
            nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentException("The Windows hosted-compiler startup inputs are invalid.");
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
        Closeˉhandle,
        Commandˉlineˉtoˉargv,
        Createˉfile,
        Flushˉfileˉbuffers,
        Getˉcommandˉline,
        Getˉfileˉsize,
        Getˉlastˉerror,
        Getˉstdˉhandle,
        Localˉfree,
        Multiˉbyteˉtoˉwideˉchar,
        Readˉfile,
        Wideˉcharˉtoˉmultiˉbyte,
        Writeˉfile,
    }
}
