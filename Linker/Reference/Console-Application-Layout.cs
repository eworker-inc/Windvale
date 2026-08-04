using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Linker;

internal enum Consoleˉapplicationˉtarget : uint
{
    Windowsˉx64 = 1,
    Linuxˉx64 = 2,
}

internal enum Consoleˉapplicationˉplanˉstatus : uint
{
    Valid = 0,
    Invalidˉsize = 1,
    Invalidˉmagic = 2,
    Invalidˉversion = 3,
    Invalidˉtarget = 4,
    Invalidˉnativeˉimage = 5,
    Invalidˉentry = 6,
    Invalidˉreserved = 7,
    Layoutˉlimit = 8,
}

internal sealed record Consoleˉapplicationˉplan(
    Consoleˉapplicationˉtarget Target,
    int Applicationˉbytes,
    int Headerˉbytes,
    int Textˉfileˉoffset,
    uint Textˉvirtualˉaddress,
    int Startupˉbytes,
    int Nativeˉimageˉoffset,
    int Nativeˉimageˉbytes,
    uint Nativeˉentryˉoffset,
    uint Nativeˉentryˉaddress,
    uint Textˉvirtualˉbytes,
    uint Textˉfileˉbytes,
    uint Dataˉfileˉoffset,
    uint Dataˉvirtualˉaddress,
    uint Dataˉfileˉbytes,
    uint Dataˉvirtualˉbytes,
    uint Metadataˉfileˉoffset,
    uint Metadataˉfileˉbytes,
    uint Metadataˉvirtualˉaddress,
    uint Metadataˉvirtualˉbytes,
    uint Imageˉvirtualˉbytes);

internal sealed class Consoleˉapplicationˉplanˉexception(string message) : Exception(message);

internal static class Consoleˉapplicationˉlayout
{
    internal const uint REQUEST_MAGIC = 0x5143_5657;
    internal const uint RESPONSE_MAGIC = 0x5043_5657;
    internal const uint FORMAT_VERSION = 1;
    internal const int REQUEST_BYTES = 32;
    internal const int RESPONSE_BYTES = 108;
    internal const int MAXIMUM_NATIVE_IMAGE_BYTES = 4 * 1024 * 1024;
    internal const int PLANNER_CANONICAL_SIZE = 8_503;
    internal const string PLANNER_CANONICAL_SHA256 =
        "ff611ffcede521728cb7c72f49822b5d869a5aa0a3d79a8e825d70e0e4b22222";

    private const uint DATA_VIRTUAL_BYTES = 18_874_480;
    private const string REQUEST_NAME = "console-application-plan-request.bin";
    private const string PLANNER_RESOURCE = "Windvale.Linker.Console-Application-Plan-Bridge.wvb";
    private const long MAXIMUM_PLANNER_INSTRUCTIONS = 2_000_000;
    private static readonly ImmutableHashSet<string> AUTHORIZED_CAPABILITIES =
        ImmutableHashSet.Create(StringComparer.Ordinal, Capabilityˉcatalog.FILE_READ_BYTES);
    private static readonly Lazy<Verifiedˉmodule> PLANNER = new(
        Readˉplanner,
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static Consoleˉapplicationˉplan Plan(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        Validateˉinput(target, nativeˉimageˉbytes, nativeˉentryˉoffset);
        var Request = Buildˉrequest(target, nativeˉimageˉbytes, nativeˉentryˉoffset);
        var Response = Evaluateˉrequest(Request);
        return Verifyˉresponse(target, nativeˉimageˉbytes, nativeˉentryˉoffset, Response);
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        Validateˉinput(target, nativeˉimageˉbytes, nativeˉentryˉoffset);
        var Bytes = new byte[REQUEST_BYTES];
        Writeˉu32(Bytes, 0, REQUEST_MAGIC);
        Writeˉu32(Bytes, 4, FORMAT_VERSION);
        Writeˉu32(Bytes, 8, REQUEST_BYTES);
        Writeˉu32(Bytes, 12, (uint)target);
        Writeˉu32(Bytes, 16, checked((uint)nativeˉimageˉbytes));
        Writeˉu32(Bytes, 20, nativeˉentryˉoffset);
        return Bytes.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Evaluateˉrequest(ImmutableArray<byte> request)
    {
        if (request.IsDefault || request.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new ArgumentException(
                "The console-application plan request must be an initialized bounded byte value.",
                nameof(request));
        }

        var Resources = new Hostedˉresourceˉcontext(
            [],
            TextWriter.Null,
            TextWriter.Null,
            new Planˉrequestˉreader(request));
        return new Referenceˉruntime(
            PLANNER.Value,
            new Referenceˉcapabilityˉhost(Resources),
            new Runtimeˉoptions(
                AUTHORIZED_CAPABILITIES,
                MAXIMUM_PLANNER_INSTRUCTIONS,
                1_024))
            .Runˉmainˉbytes()
            .Bytes;
    }

    internal static Consoleˉapplicationˉplan Verifyˉresponse(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        uint nativeˉentryˉoffset,
        ImmutableArray<byte> response)
    {
        Validateˉinput(target, nativeˉimageˉbytes, nativeˉentryˉoffset);
        if (response.IsDefault || response.Length < RESPONSE_BYTES)
        {
            throw Invalidˉresponse("The Windvale console-application plan is truncated.");
        }

        var Span = response.AsSpan();
        if (Readˉu32(Span, 0) != RESPONSE_MAGIC ||
            Readˉu32(Span, 4) != FORMAT_VERSION)
        {
            throw Invalidˉresponse("The Windvale console-application plan envelope is invalid.");
        }
        var Status = (Consoleˉapplicationˉplanˉstatus)Readˉu32(Span, 12);
        var Failureˉoffset = Readˉu32(Span, 16);
        if (!Enum.IsDefined(Status))
        {
            throw Invalidˉresponse("The Windvale console-application plan status is unknown.");
        }
        if (Status != Consoleˉapplicationˉplanˉstatus.Valid)
        {
            throw new Consoleˉapplicationˉplanˉexception(
                $"The Windvale console-application planner rejected its host request with " +
                $"status {Status} at offset {Failureˉoffset}.");
        }
        if (response.Length != RESPONSE_BYTES ||
            Readˉu32(Span, 8) != RESPONSE_BYTES ||
            Failureˉoffset != REQUEST_BYTES ||
            Readˉu32(Span, 104) != 0)
        {
            throw Invalidˉresponse("The Windvale console-application plan shape is inconsistent.");
        }

        var Actual = new Consoleˉapplicationˉplan(
            (Consoleˉapplicationˉtarget)Readˉu32(Span, 20),
            Readˉint(Span, 24),
            Readˉint(Span, 28),
            Readˉint(Span, 32),
            Readˉu32(Span, 36),
            Readˉint(Span, 40),
            Readˉint(Span, 44),
            Readˉint(Span, 48),
            Readˉu32(Span, 52),
            Readˉu32(Span, 56),
            Readˉu32(Span, 60),
            Readˉu32(Span, 64),
            Readˉu32(Span, 68),
            Readˉu32(Span, 72),
            Readˉu32(Span, 76),
            Readˉu32(Span, 80),
            Readˉu32(Span, 84),
            Readˉu32(Span, 88),
            Readˉu32(Span, 92),
            Readˉu32(Span, 96),
            Readˉu32(Span, 100));
        var Expected = Calculateˉexpected(target, nativeˉimageˉbytes, nativeˉentryˉoffset);
        if (Actual != Expected)
        {
            throw Invalidˉresponse("The Windvale console-application plan disagrees with the oracle.");
        }
        return Actual;
    }

    private static Consoleˉapplicationˉplan Calculateˉexpected(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        if (target == Consoleˉapplicationˉtarget.Windowsˉx64)
        {
            var Textˉvirtualˉbytes = checked(
                (uint)Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
                (uint)nativeˉimageˉbytes);
            var Textˉfileˉbytes = Alignˉup(Textˉvirtualˉbytes, 0x200);
            var Dataˉfileˉoffset = checked(0x200u + Textˉfileˉbytes);
            var Metadataˉfileˉoffset = checked(Dataˉfileˉoffset + 0x200u);
            var Dataˉvirtualˉaddress = Alignˉup(0x1000u + Textˉvirtualˉbytes, 0x1000);
            var Metadataˉvirtualˉaddress = Alignˉup(
                Dataˉvirtualˉaddress + DATA_VIRTUAL_BYTES,
                0x1000);
            return new(
                target,
                checked((int)(Metadataˉfileˉoffset + 0x200u)),
                0x200,
                0x200,
                0x1000,
                Windowsˉconsoleˉapplicationˉcontract.STARTUP_BYTES,
                Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET,
                nativeˉimageˉbytes,
                nativeˉentryˉoffset,
                checked(0x1000u +
                    (uint)Windowsˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
                    nativeˉentryˉoffset),
                Textˉvirtualˉbytes,
                Textˉfileˉbytes,
                Dataˉfileˉoffset,
                Dataˉvirtualˉaddress,
                0x200,
                DATA_VIRTUAL_BYTES,
                Metadataˉfileˉoffset,
                0x200,
                Metadataˉvirtualˉaddress,
                12,
                Alignˉup(Metadataˉvirtualˉaddress + 12, 0x1000));
        }

        var Linuxˉtextˉvirtualˉbytes = checked(
            (uint)Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
            (uint)nativeˉimageˉbytes);
        var Linuxˉdataˉfileˉoffset = Alignˉup(
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS +
                Linuxˉtextˉvirtualˉbytes,
            0x1000);
        return new(
            target,
            checked((int)(Linuxˉdataˉfileˉoffset + Nativeˉexecutionˉcontextˉcontract.SIZE)),
            checked((int)Linuxˉconsoleˉapplicationˉcontract.HEADER_BYTES),
            checked((int)Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS),
            Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS,
            Linuxˉconsoleˉapplicationˉcontract.STARTUP_BYTES,
            Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET,
            nativeˉimageˉbytes,
            nativeˉentryˉoffset,
            checked(
                Linuxˉconsoleˉapplicationˉcontract.TEXT_VIRTUAL_ADDRESS +
                (uint)Linuxˉconsoleˉapplicationˉcontract.NATIVE_IMAGE_OFFSET +
                nativeˉentryˉoffset),
            Linuxˉtextˉvirtualˉbytes,
            Linuxˉtextˉvirtualˉbytes,
            Linuxˉdataˉfileˉoffset,
            Linuxˉdataˉfileˉoffset,
            Nativeˉexecutionˉcontextˉcontract.SIZE,
            DATA_VIRTUAL_BYTES,
            0x180,
            28,
            0x180,
            28,
            checked(Linuxˉdataˉfileˉoffset + DATA_VIRTUAL_BYTES));
    }

    private static void Validateˉinput(
        Consoleˉapplicationˉtarget target,
        int nativeˉimageˉbytes,
        uint nativeˉentryˉoffset)
    {
        if (!Enum.IsDefined(target))
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }
        if (nativeˉimageˉbytes is < 1 or > MAXIMUM_NATIVE_IMAGE_BYTES)
        {
            throw new ArgumentOutOfRangeException(nameof(nativeˉimageˉbytes));
        }
        if (nativeˉentryˉoffset >= (uint)nativeˉimageˉbytes)
        {
            throw new ArgumentOutOfRangeException(nameof(nativeˉentryˉoffset));
        }
    }

    private static Verifiedˉmodule Readˉplanner()
    {
        using var Stream = typeof(Consoleˉapplicationˉlayout).Assembly
            .GetManifestResourceStream(PLANNER_RESOURCE) ??
            throw Invalidˉplanner();
        if (Stream.Length != PLANNER_CANONICAL_SIZE)
        {
            throw Invalidˉplanner();
        }
        var Bytes = new byte[PLANNER_CANONICAL_SIZE];
        Stream.ReadExactly(Bytes);
        var Hash = Convert.ToHexString(SHA256.HashData(Bytes)).ToLowerInvariant();
        if (!StringComparer.Ordinal.Equals(Hash, PLANNER_CANONICAL_SHA256))
        {
            throw Invalidˉplanner();
        }
        return Moduleˉcodec.Readˉandˉverify(Bytes);
    }

    private static uint Alignˉup(uint value, uint alignment) =>
        checked((value + alignment - 1) & ~(alignment - 1));

    private static int Readˉint(ReadOnlySpan<byte> bytes, int offset)
    {
        var Value = Readˉu32(bytes, offset);
        if (Value > int.MaxValue)
        {
            throw Invalidˉresponse("A Windvale console-application plan field exceeds the host bound.");
        }
        return (int)Value;
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static Consoleˉapplicationˉplanˉexception Invalidˉresponse(string message) => new(message);

    private static InvalidOperationException Invalidˉplanner() =>
        new("The retained Windvale console-application planner failed its exact identity contract.");

    private sealed class Planˉrequestˉreader(ImmutableArray<byte> request) : IHostedˉfileˉreader
    {
        public ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes)
        {
            if (!StringComparer.Ordinal.Equals(resourceˉname, REQUEST_NAME))
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Notˉfound,
                    "The console-application planner requested an unknown resource.");
            }
            if (request.Length > maximumˉbytes)
            {
                throw new Hostedˉfileˉexception(
                    Hostedˉfileˉerror.Tooˉlarge,
                    "The console-application plan request exceeds the hosted byte limit.");
            }
            return request;
        }
    }
}
