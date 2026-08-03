using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Linker;

internal enum Hostedˉconsoleˉadapter : uint
{
    Windowsˉwriteˉfile = 1,
    Linuxˉwrite = 2,
}

internal sealed record Verifiedˉhostedˉconsoleˉmetadata(
    Consoleˉapplicationˉtarget Target,
    uint Nativeˉimageˉoffset,
    uint Nativeˉimageˉbytes,
    uint Nativeˉentryˉoffset,
    uint Outputˉserviceˉoffset,
    uint Outputˉserviceˉbytes,
    ImmutableArray<Nativeˉservice> Requiredˉservices);

internal static class Hostedˉconsoleˉapplicationˉmetadata
{
    internal const uint MAGIC = 0x4348_5657;
    internal const uint FORMAT_VERSION = 1;
    internal const int SIZE = 192;
    internal const uint SERVICE_RECORD_OFFSET = 96;
    internal const uint SERVICE_RECORD_BYTES = 32;
    internal const uint CONSOLE_WRITE_LINE_CAPABILITY = 1;
    internal const uint OUTPUT_SERVICE_SHA256_OFFSET = 128;
    internal const uint NATIVE_IMAGE_SHA256_OFFSET = 160;

    internal static ImmutableArray<byte> Build(
        Consoleˉapplicationˉtarget target,
        uint nativeˉimageˉoffset,
        ReadOnlySpan<byte> nativeˉimage,
        uint nativeˉentryˉoffset,
        uint outputˉserviceˉoffset,
        ReadOnlySpan<byte> outputˉservice)
    {
        var Bytes = new byte[SIZE];
        Writeˉu32(Bytes, 0, MAGIC);
        Writeˉu32(Bytes, 4, FORMAT_VERSION);
        Writeˉu32(Bytes, 8, SIZE);
        Writeˉu32(Bytes, 12, (uint)target);
        Writeˉu32(Bytes, 16, Nativeˉcontract.ABI_VERSION);
        Writeˉu32(Bytes, 20, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(Bytes, 24, Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(Bytes, 28, 2);
        Writeˉu32(Bytes, 32, 1);
        Writeˉu32(Bytes, 36, 1);
        Writeˉu32(Bytes, 40, SERVICE_RECORD_OFFSET);
        Writeˉu32(Bytes, 44, SERVICE_RECORD_BYTES);
        Writeˉu32(Bytes, 48, nativeˉimageˉoffset);
        Writeˉu32(Bytes, 52, checked((uint)nativeˉimage.Length));
        Writeˉu32(Bytes, 56, nativeˉentryˉoffset);
        Writeˉu32(Bytes, 60, outputˉserviceˉoffset);
        Writeˉu32(Bytes, 64, checked((uint)outputˉservice.Length));
        Writeˉu32(Bytes, 68, Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_HEADER_BYTES);
        Writeˉu32(Bytes, 72, Nativeˉconsoleˉapplicationˉcontract.HOSTED_RECORD_ARENA_OFFSET);
        Writeˉu32(Bytes, 76, Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(Bytes, 80, Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_OFFSET);
        Writeˉu32(Bytes, 84, Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES);
        Writeˉu32(Bytes, 88, 1);

        Writeˉu32(Bytes, 96, (uint)Nativeˉservice.Consoleˉwriteˉline);
        Writeˉu32(Bytes, 100, CONSOLE_WRITE_LINE_CAPABILITY);
        Writeˉu32(
            Bytes,
            104,
            Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET);
        Writeˉu32(
            Bytes,
            108,
            (uint)(target == Consoleˉapplicationˉtarget.Windowsˉx64
                ? Hostedˉconsoleˉadapter.Windowsˉwriteˉfile
                : Hostedˉconsoleˉadapter.Linuxˉwrite));
        Writeˉu32(Bytes, 112, Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET);
        Writeˉu32(Bytes, 116, 1);

        SHA256.HashData(outputˉservice).CopyTo(Bytes.AsSpan((int)OUTPUT_SERVICE_SHA256_OFFSET));
        SHA256.HashData(nativeˉimage).CopyTo(Bytes.AsSpan((int)NATIVE_IMAGE_SHA256_OFFSET));
        return Bytes.ToImmutableArray();
    }

    internal static Verifiedˉhostedˉconsoleˉmetadata Verify(
        ReadOnlySpan<byte> bytes,
        Consoleˉapplicationˉtarget expectedˉtarget,
        ReadOnlySpan<byte> nativeˉimage,
        ReadOnlySpan<byte> outputˉservice)
    {
        if (bytes.Length != SIZE)
        {
            throw Invalid("The hosted console metadata has an invalid size.");
        }

        Require(bytes, 0, MAGIC, "magic");
        Require(bytes, 4, FORMAT_VERSION, "metadata version");
        Require(bytes, 8, SIZE, "metadata size");
        Require(bytes, 12, (uint)expectedˉtarget, "target");
        Require(bytes, 16, Nativeˉcontract.ABI_VERSION, "native ABI version");
        Require(bytes, 20, Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION, "execution-context version");
        Require(bytes, 24, Nativeˉserviceˉtableˉcontract.FORMAT_VERSION, "service-table version");
        Require(bytes, 28, 2, "application format version");
        Require(bytes, 32, 1, "service count");
        Require(bytes, 36, 1, "capability count");
        Require(bytes, 40, SERVICE_RECORD_OFFSET, "service-record offset");
        Require(bytes, 44, SERVICE_RECORD_BYTES, "service-record size");
        var Nativeˉoffset = Readˉu32(bytes, 48);
        Require(bytes, 52, checked((uint)nativeˉimage.Length), "native-image size");
        var Nativeˉentry = Readˉu32(bytes, 56);
        var Outputˉoffset = Readˉu32(bytes, 60);
        Require(bytes, 64, checked((uint)outputˉservice.Length), "output-service size");
        Require(bytes, 68, Nativeˉconsoleˉapplicationˉcontract.HOSTED_DATA_HEADER_BYTES, "data-header size");
        Require(bytes, 72, Nativeˉconsoleˉapplicationˉcontract.HOSTED_RECORD_ARENA_OFFSET, "record-arena offset");
        Require(bytes, 76, Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES, "record-arena size");
        Require(bytes, 80, Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_OFFSET, "text-arena offset");
        Require(bytes, 84, Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES, "text-arena size");
        Require(bytes, 88, 1, "metadata flags");
        Require(bytes, 92, 0, "reserved header");

        Require(bytes, 96, (uint)Nativeˉservice.Consoleˉwriteˉline, "service identity");
        Require(bytes, 100, CONSOLE_WRITE_LINE_CAPABILITY, "capability identity");
        Require(bytes, 104, Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET, "service-table slot");
        Require(
            bytes,
            108,
            (uint)(expectedˉtarget == Consoleˉapplicationˉtarget.Windowsˉx64
                ? Hostedˉconsoleˉadapter.Windowsˉwriteˉfile
                : Hostedˉconsoleˉadapter.Linuxˉwrite),
            "host adapter");
        Require(bytes, 112, Nativeˉconsoleˉapplicationˉcontract.HOSTED_OUTPUT_TABLE_OFFSET, "output-table offset");
        Require(bytes, 116, 1, "output-target flags");
        Require(bytes, 120, 0, "reserved service field");
        Require(bytes, 124, 0, "reserved service tail");

        if (!bytes.Slice((int)OUTPUT_SERVICE_SHA256_OFFSET, 32)
                .SequenceEqual(SHA256.HashData(outputˉservice)) ||
            !bytes.Slice((int)NATIVE_IMAGE_SHA256_OFFSET, 32)
                .SequenceEqual(SHA256.HashData(nativeˉimage)))
        {
            throw Invalid("The hosted console metadata digest does not match its payload.");
        }

        if (nativeˉimage.IsEmpty || Nativeˉentry >= nativeˉimage.Length)
        {
            throw Invalid("The hosted console metadata entry is outside its native image.");
        }

        return new(
            expectedˉtarget,
            Nativeˉoffset,
            checked((uint)nativeˉimage.Length),
            Nativeˉentry,
            Outputˉoffset,
            checked((uint)outputˉservice.Length),
            [Nativeˉservice.Consoleˉwriteˉline]);
    }

    private static void Require(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint expected,
        string field)
    {
        if (Readˉu32(bytes, offset) != expected)
        {
            throw Invalid($"The hosted console {field} is invalid.");
        }
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static InvalidDataException Invalid(string message) => new(message);
}
