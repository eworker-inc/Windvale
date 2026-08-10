using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal sealed record Hostedˉcompilerˉruntimeˉlayout(
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
    uint Recordˉarenaˉoffset,
    uint Textˉarenaˉoffset,
    uint Textˉarenaˉbytes,
    uint Nameˉarenaˉstrideˉbytes,
    uint Nameˉarenaˉoffset,
    uint Dataˉarenaˉoffset,
    uint Fileˉinputˉscratchˉoffset,
    uint Fileˉinputˉscratchˉbytes,
    uint Fileˉoutputˉscratchˉoffset,
    uint Fileˉoutputˉscratchˉbytes,
    uint Virtualˉbytes);

internal sealed record Verifiedˉhostedˉcompilerˉruntimeˉdata(
    Hostedˉcompilerˉruntimeˉlayout Layout,
    Verifiedˉhostedˉcompilerˉmetadata Metadata);

internal static class Hostedˉcompilerˉruntimeˉdata
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
    internal const uint BUILD_DRIVER_NAME_ARENA_STRIDE_BYTES = 8 * 1024;

    internal static Hostedˉcompilerˉruntimeˉlayout Plan(
        Consoleˉapplicationˉtarget target,
        Hostedˉcompilerˉapplicationˉprofile profile =
            Hostedˉcompilerˉapplicationˉprofile.Compiler)
    {
        if (!Enum.IsDefined(target) || !Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(
                !Enum.IsDefined(target) ? nameof(target) : nameof(profile));
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
        var Snapshotˉtableˉbytes = checked(
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY *
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_RECORD_BYTES);
        var Recordˉarenaˉoffset = Alignˉup(
            checked(Snapshotˉtableˉoffset + Snapshotˉtableˉbytes),
            4096);
        var Textˉarenaˉoffset = checked(
            Recordˉarenaˉoffset + Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        var Textˉarenaˉbytes =
            Hostedˉcompilerˉapplicationˉmetadata.Textˉarenaˉbytes(profile);
        var Nameˉarenaˉoffset = checked(
            Textˉarenaˉoffset + Textˉarenaˉbytes);
        var Nameˉarenaˉstrideˉbytes = profile ==
                Hostedˉcompilerˉapplicationˉprofile.Buildˉdriver
            ? BUILD_DRIVER_NAME_ARENA_STRIDE_BYTES
            : Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES;
        var Nameˉarenaˉbytes = checked(
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY *
            Nameˉarenaˉstrideˉbytes);
        var Dataˉarenaˉoffset = checked(Nameˉarenaˉoffset + Nameˉarenaˉbytes);
        var Dataˉarenaˉbytes = checked(
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY *
            Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        var Inputˉscratchˉoffset = checked(Dataˉarenaˉoffset + Dataˉarenaˉbytes);
        var Inputˉscratchˉbytes = Pathˉscratchˉbytes(target);
        var Outputˉscratchˉoffset = Alignˉup(
            checked(Inputˉscratchˉoffset + Inputˉscratchˉbytes),
            4096);
        var Outputˉscratchˉbytes = Pathˉscratchˉbytes(target);
        var Virtualˉbytes = Alignˉup(
            checked(Outputˉscratchˉoffset + Outputˉscratchˉbytes),
            4096);
        if (Virtualˉbytes > MAXIMUM_RUNTIME_DATA_BYTES)
        {
            throw new InvalidOperationException(
                "The hosted compiler runtime data exceeds its fixed 512 MiB bound.");
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
            Recordˉarenaˉoffset,
            Textˉarenaˉoffset,
            Textˉarenaˉbytes,
            Nameˉarenaˉstrideˉbytes,
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
        Hostedˉcompilerˉapplicationˉprofile profile =
            Hostedˉcompilerˉapplicationˉprofile.Compiler)
    {
        Hostedˉcompilerˉapplicationˉmetadata.Validateˉinputs(
            target,
            capabilities,
            bundle,
            BUNDLE_TEXT_OFFSET,
            nativeˉentryˉoffset);
        var Metadata = Nativeˉhostedˉtoolˉmetadataˉbuilder.Build(new(
            (uint)target,
            (uint)profile,
            BUNDLE_TEXT_OFFSET,
            nativeˉentryˉoffset,
            bundle));
        _ = Hostedˉcompilerˉapplicationˉmetadata.Verify(
            Metadata.AsSpan(),
            target,
            bundle,
            bundle.Imageˉbytes.AsSpan(),
            profile);
        var Header = Nativeˉhostedˉtoolˉruntimeˉheaderˉbuilder.Build(new(
            (uint)target,
            (uint)profile,
            Metadata));
        _ = Verify(Header.AsSpan(), target, bundle, bundle.Imageˉbytes.AsSpan(), profile);
        return Header;
    }

    internal static Verifiedˉhostedˉcompilerˉruntimeˉdata Verify(
        ReadOnlySpan<byte> bytes,
        Consoleˉapplicationˉtarget expectedˉtarget,
        Nativeˉserviceˉbundle expectedˉbundle,
        ReadOnlySpan<byte> actualˉbundleˉimage,
        Hostedˉcompilerˉapplicationˉprofile expectedˉprofile =
            Hostedˉcompilerˉapplicationˉprofile.Compiler)
    {
        if (bytes.Length != HEADER_BYTES)
        {
            throw Invalid("The hosted compiler runtime header has an invalid size.");
        }
        var Layout = Plan(expectedˉtarget, expectedˉprofile);
        Verifyˉcontext(bytes, Layout);
        Verifyˉserviceˉtable(bytes);
        Verifyˉoutputˉtable(bytes, expectedˉtarget);
        Verifyˉfileˉinputˉtable(bytes, Layout);
        Verifyˉfileˉoutputˉtable(bytes, Layout);
        var Metadata = Hostedˉcompilerˉapplicationˉmetadata.Verify(
            bytes.Slice(
                checked((int)METADATA_OFFSET),
                Hostedˉcompilerˉapplicationˉmetadata.SIZE),
            expectedˉtarget,
            expectedˉbundle,
            actualˉbundleˉimage,
            expectedˉprofile);
        Requireˉzero(
            bytes,
            checked((int)METADATA_OFFSET + Hostedˉcompilerˉapplicationˉmetadata.SIZE),
            checked((int)HEADER_BYTES -
                (checked((int)METADATA_OFFSET) + Hostedˉcompilerˉapplicationˉmetadata.SIZE)),
            "reserved header tail");
        return new(Layout, Metadata);
    }

    private static void Verifyˉcontext(
        ReadOnlySpan<byte> bytes,
        Hostedˉcompilerˉruntimeˉlayout layout)
    {
        var Base = checked((int)CONTEXT_OFFSET);
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION, "context version");
        Require(bytes, Base + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE, "context size");
        Requireˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            Hostedˉcompilerˉapplicationˉmetadata.COMPILER_MAXIMUM_INSTRUCTIONS,
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
            layout.Textˉarenaˉbytes, "text-arena length");
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

    private static void Verifyˉfileˉinputˉtable(
        ReadOnlySpan<byte> bytes,
        Hostedˉcompilerˉruntimeˉlayout layout)
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
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY, "snapshot capacity");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET,
            0, "initial snapshot count");
        Requireˉu64(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_ARENA_POINTER_OFFSET,
            0, "initial name-arena pointer");
        Require(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET,
            layout.Nameˉarenaˉstrideˉbytes, "name stride");
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

    private static void Verifyˉfileˉoutputˉtable(
        ReadOnlySpan<byte> bytes,
        Hostedˉcompilerˉruntimeˉlayout layout)
    {
        var Base = checked((int)FILE_OUTPUT_TABLE_OFFSET);
        Require(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉfileˉoutputˉtableˉcontract.MAGIC, "file-output magic");
        Require(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉfileˉoutputˉtableˉcontract.FORMAT_VERSION, "file-output version");
        Require(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉfileˉoutputˉtableˉcontract.SIZE, "file-output size");
        Require(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)Fileˉplatform(layout.Target), "file-output platform");
        Requireˉu64(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_POINTER_OFFSET,
            0, "initial file-output scratch pointer");
        Require(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_BYTES_OFFSET,
            layout.Fileˉoutputˉscratchˉbytes, "file-output scratch bytes");
        Require(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.RESERVED_OFFSET,
            0, "file-output reserved field");
        Requireˉzero(bytes,
            Base + Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET,
            6 * sizeof(ulong), "initial file-output platform functions");
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
            throw Invalid($"The hosted compiler {field} is invalid.");
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
            throw Invalid($"The hosted compiler {field} is invalid.");
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
            throw Invalid($"The hosted compiler {field} is invalid.");
        }
    }

    private static uint Readˉu32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, sizeof(uint)));

    private static ulong Readˉu64(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(offset, sizeof(ulong)));

    private static InvalidDataException Invalid(string message) => new(message);
}
