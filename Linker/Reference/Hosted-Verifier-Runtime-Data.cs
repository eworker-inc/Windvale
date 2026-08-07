using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Hostedˉverifierˉruntimeˉlayout(
    Consoleˉapplicationˉtarget Target,
    uint Maximumˉarguments,
    uint Maximumˉargumentˉbytes,
    uint Headerˉbytes,
    uint Argumentˉtableˉoffset,
    uint Argumentˉtableˉbytes,
    uint Argumentˉbytesˉoffset,
    uint Argumentˉbytes,
    uint Snapshotˉtableˉoffset,
    uint Snapshotˉtableˉbytes,
    uint Snapshotˉcapacity,
    uint Recordˉarenaˉoffset,
    uint Textˉarenaˉoffset,
    uint Nameˉarenaˉoffset,
    uint Dataˉarenaˉoffset,
    uint Fileˉinputˉscratchˉoffset,
    uint Fileˉinputˉscratchˉbytes,
    uint Fileˉoutputˉscratchˉoffset,
    uint Fileˉoutputˉscratchˉbytes,
    uint Virtualˉbytes);

internal sealed record Verifiedˉhostedˉverifierˉruntimeˉdata(
    Hostedˉverifierˉruntimeˉlayout Layout,
    Verifiedˉhostedˉverifierˉmetadata Metadata);

internal static class Hostedˉverifierˉruntimeˉdata
{
    internal const uint HEADER_BYTES = 4096;
    internal const uint CONTEXT_OFFSET = 0;
    internal const uint SERVICE_TABLE_OFFSET = 112;
    internal const uint OUTPUT_TABLE_OFFSET = 216;
    internal const uint FILE_INPUT_TABLE_OFFSET = 264;
    internal const uint FILE_OUTPUT_TABLE_OFFSET = 400;
    internal const uint METADATA_OFFSET = 480;
    internal const uint BUNDLE_TEXT_OFFSET = 4096;
    internal const uint MAXIMUM_RUNTIME_DATA_BYTES = 512 * 1024 * 1024;

    internal static Hostedˉverifierˉruntimeˉlayout Plan(
        Consoleˉapplicationˉtarget target,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        if (!Enum.IsDefined(target))
        {
            throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }

        var Argumentˉtableˉbytes = checked(
            (uint)Hostedˉresourceˉlimits.MAX_ARGUMENTS * Nativeˉcontract.VALUE_SLOT_BYTES);
        var Argumentˉtableˉoffset = HEADER_BYTES;
        var Argumentˉbytesˉoffset = Alignˉup(
            checked(Argumentˉtableˉoffset + Argumentˉtableˉbytes),
            16);
        var Argumentˉbytes = checked(
            (uint)Hostedˉresourceˉlimits.MAX_ARGUMENT_TOTAL_UTF8_BYTES);
        var Snapshotˉtableˉoffset = Alignˉup(
            checked(Argumentˉbytesˉoffset + Argumentˉbytes),
            16);
        var Snapshotˉcapacity = profile ==
            Hostedˉverifierˉapplicationˉprofile.Consoleˉapplicationˉverifier
            ? 2u
            : 1u;
        var Snapshotˉtableˉbytes = checked(
            Snapshotˉcapacity *
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_RECORD_BYTES);
        var Recordˉarenaˉoffset = Alignˉup(
            checked(Snapshotˉtableˉoffset + Snapshotˉtableˉbytes),
            4096);
        var Textˉarenaˉoffset = checked(
            Recordˉarenaˉoffset + Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        var Nameˉarenaˉoffset = checked(
            Textˉarenaˉoffset + Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES);
        var Nameˉarenaˉbytes = checked(
            Snapshotˉcapacity *
            Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES);
        var Dataˉarenaˉoffset = checked(Nameˉarenaˉoffset + Nameˉarenaˉbytes);
        var Dataˉarenaˉbytes = checked(
            Snapshotˉcapacity *
            Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        var Inputˉscratchˉoffset = checked(Dataˉarenaˉoffset + Dataˉarenaˉbytes);
        var Inputˉscratchˉbytes = Pathˉscratchˉbytes(target);
        var Outputˉscratchˉoffset = Alignˉup(
            checked(Inputˉscratchˉoffset + Inputˉscratchˉbytes),
            4096);
        const uint Outputˉscratchˉbytes = 0;
        var Virtualˉbytes = Alignˉup(
            checked(Outputˉscratchˉoffset + Outputˉscratchˉbytes),
            4096);
        if (Virtualˉbytes > MAXIMUM_RUNTIME_DATA_BYTES)
        {
            throw new InvalidOperationException(
                "The hosted verifier runtime data exceeds its fixed 512 MiB bound.");
        }

        return new(
            target,
            checked((uint)Hostedˉresourceˉlimits.MAX_ARGUMENTS),
            checked((uint)Hostedˉresourceˉlimits.MAX_ARGUMENT_UTF8_BYTES),
            HEADER_BYTES,
            Argumentˉtableˉoffset,
            Argumentˉtableˉbytes,
            Argumentˉbytesˉoffset,
            Argumentˉbytes,
            Snapshotˉtableˉoffset,
            Snapshotˉtableˉbytes,
            Snapshotˉcapacity,
            Recordˉarenaˉoffset,
            Textˉarenaˉoffset,
            Nameˉarenaˉoffset,
            Dataˉarenaˉoffset,
            Inputˉscratchˉoffset,
            Inputˉscratchˉbytes,
            Outputˉscratchˉoffset,
            Outputˉscratchˉbytes,
            Virtualˉbytes);
    }

    internal static ImmutableArray<byte> Build(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<Capabilityˉdeclaration> capabilities,
        Nativeˉserviceˉbundle bundle,
        uint nativeˉentryˉoffset,
        Hostedˉverifierˉapplicationˉprofile profile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        var Layout = Plan(target, profile);
        var Bytes = new byte[checked((int)HEADER_BYTES)];
        Writeˉcontext(Bytes);
        Writeˉserviceˉtable(Bytes);
        Writeˉoutputˉtable(Bytes, target);
        Writeˉfileˉinputˉtable(Bytes, Layout);
        Requireˉzero(
            Bytes,
            checked((int)FILE_OUTPUT_TABLE_OFFSET),
            checked((int)(METADATA_OFFSET - FILE_OUTPUT_TABLE_OFFSET)),
            "reserved file-output table");
        var Metadata = Hostedˉverifierˉapplicationˉmetadata.Build(
            target,
            capabilities,
            bundle,
            BUNDLE_TEXT_OFFSET,
            nativeˉentryˉoffset,
            profile);
        Metadata.AsSpan().CopyTo(Bytes.AsSpan(checked((int)METADATA_OFFSET)));
        return Bytes.ToImmutableArray();
    }

    internal static Verifiedˉhostedˉverifierˉruntimeˉdata Verify(
        ReadOnlySpan<byte> bytes,
        Consoleˉapplicationˉtarget expectedˉtarget,
        Nativeˉserviceˉbundle expectedˉbundle,
        ReadOnlySpan<byte> actualˉbundleˉimage,
        Hostedˉverifierˉapplicationˉprofile expectedˉprofile =
            Hostedˉverifierˉapplicationˉprofile.Compilerˉwvbˉverifier)
    {
        if (bytes.Length != HEADER_BYTES)
        {
            throw Invalid("The hosted verifier runtime header has an invalid size.");
        }
        var Layout = Plan(expectedˉtarget, expectedˉprofile);
        Verifyˉcontext(bytes);
        Verifyˉserviceˉtable(bytes);
        Verifyˉoutputˉtable(bytes, expectedˉtarget);
        Verifyˉfileˉinputˉtable(bytes, Layout);
        Requireˉzero(
            bytes,
            checked((int)FILE_OUTPUT_TABLE_OFFSET),
            checked((int)(METADATA_OFFSET - FILE_OUTPUT_TABLE_OFFSET)),
            "reserved file-output table");
        var Metadata = Hostedˉverifierˉapplicationˉmetadata.Verify(
            bytes.Slice(
                checked((int)METADATA_OFFSET),
                Hostedˉverifierˉapplicationˉmetadata.SIZE),
            expectedˉtarget,
            expectedˉbundle,
            actualˉbundleˉimage,
            expectedˉprofile);
        Requireˉzero(
            bytes,
            checked((int)METADATA_OFFSET + Hostedˉverifierˉapplicationˉmetadata.SIZE),
            checked((int)HEADER_BYTES -
                (checked((int)METADATA_OFFSET) + Hostedˉverifierˉapplicationˉmetadata.SIZE)),
            "reserved header tail");
        return new(Layout, Metadata);
    }

    private static void Writeˉcontext(byte[] bytes)
    {
        var Base = checked((int)CONTEXT_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE);
        Writeˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            Hostedˉverifierˉapplicationˉmetadata.VERIFIER_MAXIMUM_INSTRUCTIONS);
        Writeˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH));
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES);
    }

    private static void Verifyˉcontext(ReadOnlySpan<byte> bytes)
    {
        var Base = checked((int)CONTEXT_OFFSET);
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION, "context version");
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE, "context size");
        Requireˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            Hostedˉverifierˉapplicationˉmetadata.VERIFIER_MAXIMUM_INSTRUCTIONS,
            "instruction budget");
        Requireˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH), "call-depth budget");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
            sizeof(ulong), "initial service-table pointer");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET,
            sizeof(ulong), "initial record-arena pointer");
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES, "record-arena length");
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
            0, "record-arena use");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
            sizeof(ulong), "initial text-arena pointer");
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES, "text-arena length");
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0, "text-arena use");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            8, "initial service failure and reserved context field");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.ARGUMENT_TABLE_POINTER_OFFSET,
            sizeof(ulong), "initial argument-table pointer");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET,
            8, "initial argument count and reserved field");
        Requireˉzero(bytes, Base + Nativeˉexecutionˉcontextˉcontract.OUTPUT_TABLE_POINTER_OFFSET,
            24, "initial hosted table pointers");
    }

    private static void Writeˉserviceˉtable(byte[] bytes)
    {
        var Base = checked((int)SERVICE_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉserviceˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉserviceˉtableˉcontract.SIZE_OFFSET,
            Nativeˉserviceˉtableˉcontract.SIZE);
    }

    private static void Verifyˉserviceˉtable(ReadOnlySpan<byte> bytes)
    {
        var Base = checked((int)SERVICE_TABLE_OFFSET);
        Require(bytes, Base + Nativeˉserviceˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION, "service-table version");
        Require(bytes, Base + Nativeˉserviceˉtableˉcontract.SIZE_OFFSET,
            Nativeˉserviceˉtableˉcontract.SIZE, "service-table size");
        Requireˉzero(bytes, Base + 8,
            checked((int)Nativeˉserviceˉtableˉcontract.SIZE - 8),
            "initial service-table pointers");
    }

    private static void Writeˉoutputˉtable(
        byte[] bytes,
        Consoleˉapplicationˉtarget target)
    {
        var Base = checked((int)OUTPUT_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉoutputˉtableˉcontract.MAGIC);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉoutputˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉoutputˉtableˉcontract.SIZE);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)Platform(target));
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.FLAGS_OFFSET,
            Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT |
                Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_PRESENT);
        if (target == Consoleˉapplicationˉtarget.Linuxˉx64)
        {
            Writeˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.CONSOLE_TARGET_OFFSET, 1);
            Writeˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_TARGET_OFFSET, 2);
        }
    }

    private static void Verifyˉoutputˉtable(
        ReadOnlySpan<byte> bytes,
        Consoleˉapplicationˉtarget target)
    {
        var Base = checked((int)OUTPUT_TABLE_OFFSET);
        Require(bytes, Base + Nativeˉoutputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉoutputˉtableˉcontract.MAGIC, "output-table magic");
        Require(bytes, Base + Nativeˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉoutputˉtableˉcontract.FORMAT_VERSION, "output-table version");
        Require(bytes, Base + Nativeˉoutputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉoutputˉtableˉcontract.SIZE, "output-table size");
        Require(bytes, Base + Nativeˉoutputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)Platform(target), "output-table platform");
        Require(bytes, Base + Nativeˉoutputˉtableˉcontract.FLAGS_OFFSET,
            Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT |
                Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_PRESENT,
            "output-table flags");
        Require(bytes, Base + Nativeˉoutputˉtableˉcontract.RESERVED_OFFSET,
            0, "output-table reserved field");
        Requireˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.CONSOLE_TARGET_OFFSET,
            target == Consoleˉapplicationˉtarget.Linuxˉx64 ? 1UL : 0,
            "initial console target");
        Requireˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_TARGET_OFFSET,
            target == Consoleˉapplicationˉtarget.Linuxˉx64 ? 2UL : 0,
            "initial diagnostic target");
        Requireˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.WRITE_FUNCTION_POINTER_OFFSET,
            0, "initial output function");
    }

    private static void Writeˉfileˉinputˉtable(
        byte[] bytes,
        Hostedˉverifierˉruntimeˉlayout layout)
    {
        var Base = checked((int)FILE_INPUT_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.MAGIC);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.SIZE);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)Fileˉplatform(layout.Target));
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY_OFFSET,
            layout.Snapshotˉcapacity);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.MAXIMUM_DATA_BYTES_OFFSET,
            Bytecodeˉlimits.MAX_BYTE_DATA_BYTES);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SCRATCH_BYTES_OFFSET,
            layout.Fileˉinputˉscratchˉbytes);
    }

    private static void Verifyˉfileˉinputˉtable(
        ReadOnlySpan<byte> bytes,
        Hostedˉverifierˉruntimeˉlayout layout)
    {
        var Base = checked((int)FILE_INPUT_TABLE_OFFSET);
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.MAGIC, "file-input magic");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION, "file-input version");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.SIZE, "file-input size");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)Fileˉplatform(layout.Target), "file-input platform");
        Requireˉu64(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_TABLE_POINTER_OFFSET,
            0, "initial snapshot-table pointer");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY_OFFSET,
            layout.Snapshotˉcapacity, "snapshot capacity");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET,
            0, "initial snapshot count");
        Requireˉu64(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_ARENA_POINTER_OFFSET,
            0, "initial name-arena pointer");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES, "name stride");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_RESERVED_OFFSET,
            0, "name reserved field");
        Requireˉu64(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.DATA_ARENA_POINTER_OFFSET,
            0, "initial data-arena pointer");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES, "data stride");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.MAXIMUM_DATA_BYTES_OFFSET,
            Bytecodeˉlimits.MAX_BYTE_DATA_BYTES, "maximum file bytes");
        Requireˉu64(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SCRATCH_POINTER_OFFSET,
            0, "initial file-input scratch pointer");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SCRATCH_BYTES_OFFSET,
            layout.Fileˉinputˉscratchˉbytes, "file-input scratch bytes");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.RESERVED_OFFSET,
            0, "file-input reserved field");
        Requireˉzero(bytes,
            Base + Nativeˉfileˉinputˉtableˉcontract.WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET,
            7 * sizeof(ulong), "initial file-input platform functions");
    }

    private static uint Pathˉscratchˉbytes(Consoleˉapplicationˉtarget target) =>
        target switch
        {
            Consoleˉapplicationˉtarget.Windowsˉx64 => checked(
                (uint)((Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES + 1) * 2)),
            Consoleˉapplicationˉtarget.Linuxˉx64 => checked(
                (uint)(Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES + 1)),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static Nativeˉoutputˉplatform Platform(Consoleˉapplicationˉtarget target) =>
        target switch
        {
            Consoleˉapplicationˉtarget.Windowsˉx64 => Nativeˉoutputˉplatform.Windows,
            Consoleˉapplicationˉtarget.Linuxˉx64 => Nativeˉoutputˉplatform.Linux,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static Nativeˉfileˉinputˉplatform Fileˉplatform(
        Consoleˉapplicationˉtarget target) => target switch
        {
            Consoleˉapplicationˉtarget.Windowsˉx64 => Nativeˉfileˉinputˉplatform.Windows,
            Consoleˉapplicationˉtarget.Linuxˉx64 => Nativeˉfileˉinputˉplatform.Linux,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static uint Alignˉup(uint value, uint alignment) => checked(
        (value + alignment - 1) & ~(alignment - 1));

    private static void Require(
        ReadOnlySpan<byte> bytes,
        int offset,
        uint expected,
        string field)
    {
        if (Readˉu32(bytes, offset) != expected)
        {
            throw Invalid($"The hosted verifier {field} is invalid.");
        }
    }

    private static void Requireˉu64(
        ReadOnlySpan<byte> bytes,
        int offset,
        ulong expected,
        string field)
    {
        if (Readˉu64(bytes, offset) != expected)
        {
            throw Invalid($"The hosted verifier {field} is invalid.");
        }
    }

    private static void Requireˉzero(
        ReadOnlySpan<byte> bytes,
        int offset,
        int length,
        string field)
    {
        if (!bytes.Slice(offset, length).SequenceEqual(new byte[length]))
        {
            throw Invalid($"The hosted verifier {field} is invalid.");
        }
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static ulong Readˉu64(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(offset, sizeof(ulong)));

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong)), value);

    private static InvalidDataException Invalid(string message) => new(message);
}
