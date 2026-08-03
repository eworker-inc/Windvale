using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Windowsˉhostedˉverifierˉstartup
{
    internal const int BYTES = 1275;
    internal const string WVO_SHA256 =
        "755ffb99cba6a838dd9eec353ce72d4adfb3af130ec4bce5a2278828dd136616";
    internal const string TEMPLATE_SHA256 =
        "b75a6601cae7bb96d42f85b68a3aedfdb40145af5ddbbf5b548c7f8c898559d9";

    private const string TEMPLATE_BASE64 =
        "U1VWV0FUQVVBVkFXSIHsiAAAAE0x5EyNPQAAAABIjQUAAAAASYmEJxgAAABIjQUAAAAASYmEJyAAAABIjQUAAAAASYmEJzAAAABIjQUAAAAASYmEJ0gAAABIjQUAAAAASYmEJ1gAAABIjQUAAAAASYmEJ2AAAABIjR0AAAAASI0FAAAAAEiJhCMQAAAASI0FAAAAAEiJhCMgAAAASI0FAAAAAEiJhCMwAAAASI0FAAAAAEiJhCNAAAAASI0FAAAAAEiLhCAAAAAASImEI1AAAABIjQUAAAAASIuEIAAAAABIiYQjWAAAAEiNBQAAAABIi4QgAAAAAEiJhCNgAAAASI0FAAAAAEiLhCAAAAAASImEI2gAAABIjQUAAAAASIuEIAAAAABIiYQjcAAAAEiNBQAAAABIiYQjeAAAAEiNBQAAAABIi4QgAAAAAEiJhCOAAAAAufX///9IjQUAAAAASIuEIAAAAAD/0EiNHQAAAABIiYQjGAAAALn0////SI0FAAAAAEiLhCAAAAAA/9BIiYQjIAAAAEiNBQAAAABIi4QgAAAAAEiJhCMoAAAASI0dAAAAAEiNBQAAAABIiYQjCAAAAEiNBQAAAABIiYQjEAAAAEiNBQAAAABIiYQjGAAAAEiNBQAAAABIiYQjIAAAAEiNBQAAAABIiYQjKAAAAEiNBQAAAABIiYQjMAAAAEiNBQAAAABIi4QgAAAAAP/QSIXAD4TfAQAASInBSIniSIHCSAAAAEiNBQAAAABIi4QgAAAAAP/QSIXAD4S4AQAASYnEi4QkSAAAAIH4AQAAAA+CogEAAIHoAQAAAIH4QwAAAA+HkAEAAEGJhCdQAAAAMdsx7UyNLQAAAABMjTUAAAAASI09AAAAAEGLhCdQAAAAOcMPgw8BAABJi7TcCAAAALnp/QAAuoAAAABJifBBuf////9IibwkIAAAALgBEAAASImEJCgAAAAxwEiJhCQwAAAASImEJDgAAABIjQUAAAAASIuEIAAAAAD/0IXAD4QEAQAAgfgBEAAAD4f4AAAAgegBAAAAiYQkWAAAAInqAcKB+gAAAQAPh9sAAAAxyYmMJFAAAABIieFIgcFQAAAASYn4QYnB6AAAAACLhCRQAAAAgfgBAAAAD4WqAAAARIuUJFgAAABIidpIweIETAHqTIn2SAHuSIm0IgAAAABEiZQiCAAAADHJRDnRD4MZAAAAioQPAAAAAIiEDgAAAACBwQEAAADp3v///0QB1YHDAQAAAOnh/v//TInhSI0FAAAAAEiLhCAAAAAA/9BIhcAPhVAAAABNMeRIjRUAAAAASDHJSYnQTTHJ6AAAAABIicJIweoghdIPhQwAAACB+P8AAAAPhiIAAABNheQPhBQAAABMieFIjQUAAAAASIuEIAAAAAD/0LgBAAAASIHEiAAAAEFfQV5BXUFcX15dW8NBgfgAEAAAD4WuAAAAQYH5BAAAAA+FoQAAAEiNBQAAAABIOcEPg1cAAABIjQUAAAAASDnBD4KBAAAASIH6AQAAAA+CdAAAAEiB+gAAEAAPh2cAAABI98H/DwAAD4VaAAAASYnKSQHSD4JOAAAASI0FAAAAAEk5wg+HPgAAAEiJyMNIgfoAAEAAD4UtAAAASPfB/w8AAA+FIAAAAEmJykkB0g+CFAAAAEiNBQAAAABJOcIPhwQAAABIicjDMcDD";

    private static readonly ImmutableArray<Startupˉpatch> PATCHES =
    [
        new(25, Startupˉtarget.Executionˉcontext),
        new(32, Startupˉtarget.Serviceˉtable),
        new(47, Startupˉtarget.Recordˉarena),
        new(62, Startupˉtarget.Textˉarena),
        new(77, Startupˉtarget.Argumentˉtable),
        new(92, Startupˉtarget.Outputˉtable),
        new(107, Startupˉtarget.Fileˉinputˉtable),
        new(122, Startupˉtarget.Fileˉinputˉtable),
        new(129, Startupˉtarget.Snapshotˉtable),
        new(144, Startupˉtarget.Nameˉarena),
        new(159, Startupˉtarget.Dataˉarena),
        new(174, Startupˉtarget.Fileˉinputˉscratch),
        new(189, Startupˉtarget.Multiˉbyteˉtoˉwideˉchar),
        new(212, Startupˉtarget.Createˉfile),
        new(235, Startupˉtarget.Getˉfileˉsize),
        new(258, Startupˉtarget.Readˉfile),
        new(281, Startupˉtarget.Closeˉhandle),
        new(304, Startupˉtarget.Commitˉruntime),
        new(319, Startupˉtarget.Getˉlastˉerror),
        new(347, Startupˉtarget.Getˉstdˉhandle),
        new(364, Startupˉtarget.Outputˉtable),
        new(384, Startupˉtarget.Getˉstdˉhandle),
        new(409, Startupˉtarget.Writeˉfile),
        new(432, Startupˉtarget.Serviceˉtable),
        new(439, Startupˉtarget.Consoleˉwrite),
        new(454, Startupˉtarget.Argumentˉcount),
        new(469, Startupˉtarget.Argument),
        new(484, Startupˉtarget.Fileˉread),
        new(499, Startupˉtarget.Utf8),
        new(514, Startupˉtarget.Diagnosticˉwrite),
        new(529, Startupˉtarget.Getˉcommandˉline),
        new(568, Startupˉtarget.Commandˉlineˉtoˉargv),
        new(646, Startupˉtarget.Argumentˉtable),
        new(653, Startupˉtarget.Argumentˉbytes),
        new(660, Startupˉtarget.Fileˉinputˉscratch),
        new(749, Startupˉtarget.Wideˉcharˉtoˉmultiˉbyte),
        new(838, Startupˉtarget.Utf8),
        new(957, Startupˉtarget.Localˉfree),
        new(986, Startupˉtarget.Executionˉcontext),
        new(1000, Startupˉtarget.Nativeˉmain),
        new(1046, Startupˉtarget.Localˉfree),
        new(1114, Startupˉtarget.Dataˉarena),
        new(1130, Startupˉtarget.Nameˉarena),
        new(1197, Startupˉtarget.Dataˉarena),
        new(1255, Startupˉtarget.Fileˉinputˉscratch),
    ];

    internal static ImmutableArray<byte> Build(
        uint startupˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        var Bytes = Decodeˉtemplate();
        foreach (var Patch in PATCHES)
        {
            var Target = Address(
                Patch.Target,
                startupˉaddress,
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
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        Validateˉinputs(layout, bundle, nativeˉentryˉoffset);
        if (bytes.Length != BYTES)
        {
            throw Invalid("The Windows hosted-verifier startup has an invalid size.");
        }

        var Unpatched = bytes.ToArray();
        foreach (var Patch in PATCHES)
        {
            var Sourceˉend = checked(startupˉaddress + (uint)Patch.Offset + sizeof(int));
            var Actual = checked((long)Sourceˉend +
                BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(Patch.Offset, sizeof(int))));
            var Expected = Address(
                Patch.Target,
                startupˉaddress,
                importˉaddress,
                runtimeˉaddress,
                layout,
                bundle,
                nativeˉentryˉoffset);
            if (Actual != Expected)
            {
                throw Invalid($"The Windows hosted-verifier {Patch.Target} target is invalid.");
            }
            Unpatched.AsSpan(Patch.Offset, sizeof(int)).Clear();
        }

        var Template = Decodeˉtemplate();
        if (!Unpatched.AsSpan().SequenceEqual(Template) ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Unpatched), TEMPLATE_SHA256))
        {
            throw Invalid("The Windows hosted-verifier startup template is noncanonical.");
        }
    }

    private static byte[] Decodeˉtemplate()
    {
        var Template = Convert.FromBase64String(TEMPLATE_BASE64);
        if (Template.Length != BYTES ||
            !StringComparer.Ordinal.Equals(Calculateˉsha256(Template), TEMPLATE_SHA256))
        {
            throw new InvalidOperationException(
                "The retained Windows hosted-verifier startup template is corrupt.");
        }
        return Template;
    }

    private static uint Address(
        Startupˉtarget target,
        uint startupˉaddress,
        uint importˉaddress,
        uint runtimeˉaddress,
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset) => target switch
        {
            Startupˉtarget.Executionˉcontext =>
                checked(runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.CONTEXT_OFFSET),
            Startupˉtarget.Serviceˉtable =>
                checked(runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.SERVICE_TABLE_OFFSET),
            Startupˉtarget.Recordˉarena => checked(runtimeˉaddress + layout.Recordˉarenaˉoffset),
            Startupˉtarget.Textˉarena => checked(runtimeˉaddress + layout.Textˉarenaˉoffset),
            Startupˉtarget.Argumentˉtable => checked(runtimeˉaddress + layout.Argumentˉtableˉoffset),
            Startupˉtarget.Argumentˉbytes => checked(runtimeˉaddress + layout.Argumentˉbytesˉoffset),
            Startupˉtarget.Outputˉtable =>
                checked(runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.OUTPUT_TABLE_OFFSET),
            Startupˉtarget.Fileˉinputˉtable =>
                checked(runtimeˉaddress + Hostedˉverifierˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET),
            Startupˉtarget.Snapshotˉtable => checked(runtimeˉaddress + layout.Snapshotˉtableˉoffset),
            Startupˉtarget.Nameˉarena => checked(runtimeˉaddress + layout.Nameˉarenaˉoffset),
            Startupˉtarget.Dataˉarena => checked(runtimeˉaddress + layout.Dataˉarenaˉoffset),
            Startupˉtarget.Fileˉinputˉscratch =>
                checked(runtimeˉaddress + layout.Fileˉinputˉscratchˉoffset),
            Startupˉtarget.Commitˉruntime => checked(startupˉaddress + 1085u),
            Startupˉtarget.Nativeˉmain => checked(
                Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS +
                Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
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
            Startupˉtarget.Closeˉhandle => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.CLOSE_HANDLE_IAT_OFFSET),
            Startupˉtarget.Commandˉlineˉtoˉargv => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.COMMAND_LINE_TO_ARGV_IAT_OFFSET),
            Startupˉtarget.Createˉfile => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.CREATE_FILE_IAT_OFFSET),
            Startupˉtarget.Getˉcommandˉline => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.GET_COMMAND_LINE_IAT_OFFSET),
            Startupˉtarget.Getˉfileˉsize => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.GET_FILE_SIZE_IAT_OFFSET),
            Startupˉtarget.Getˉlastˉerror => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.GET_LAST_ERROR_IAT_OFFSET),
            Startupˉtarget.Getˉstdˉhandle => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.GET_STD_HANDLE_IAT_OFFSET),
            Startupˉtarget.Localˉfree => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.LOCAL_FREE_IAT_OFFSET),
            Startupˉtarget.Multiˉbyteˉtoˉwideˉchar => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.MULTI_BYTE_TO_WIDE_CHAR_IAT_OFFSET),
            Startupˉtarget.Readˉfile => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.READ_FILE_IAT_OFFSET),
            Startupˉtarget.Wideˉcharˉtoˉmultiˉbyte => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.WIDE_CHAR_TO_MULTI_BYTE_IAT_OFFSET),
            Startupˉtarget.Writeˉfile => checked(importˉaddress +
                Windowsˉhostedˉverifierˉimports.WRITE_FILE_IAT_OFFSET),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static uint Serviceˉaddress(
        Nativeˉserviceˉbundle bundle,
        Nativeˉservice service)
    {
        var Placement = bundle.Placements.Single(Item => Item.Service == service);
        return checked(
            Windowsˉhostedˉverifierˉapplicationˉcontract.TEXT_ADDRESS +
            Windowsˉhostedˉverifierˉapplicationˉcontract.BUNDLE_TEXT_OFFSET +
            (uint)Placement.Imageˉoffset);
    }

    private static void Validateˉinputs(
        Hostedˉverifierˉruntimeˉlayout layout,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset)
    {
        if (layout.Target != Consoleˉapplicationˉtarget.Windowsˉx64 ||
            bundle is null ||
            bundle.Platform != Nativeˉserviceˉplatform.Windows ||
            bundle.Placements.Length != Hostedˉverifierˉapplicationˉmetadata.SERVICE_COUNT ||
            nativeˉentryˉoffset >= bundle.Nativeˉimageˉbytes)
        {
            throw new ArgumentException("The Windows hosted-verifier startup inputs are invalid.");
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
        Commitˉruntime,
        Nativeˉmain,
        Consoleˉwrite,
        Argumentˉcount,
        Argument,
        Fileˉread,
        Utf8,
        Diagnosticˉwrite,
        Closeˉhandle,
        Commandˉlineˉtoˉargv,
        Createˉfile,
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
