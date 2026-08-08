using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Linker;

internal static class Hostedˉcompilerˉruntimeˉheaderˉstage0ˉoracle
{
    internal static ImmutableArray<byte> Build(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<byte> metadata)
    {
        if (metadata.IsDefault || metadata.Length != Hostedˉcompilerˉapplicationˉmetadata.SIZE)
        {
            throw new ArgumentException("The Stage 0 hosted runtime metadata is invalid.");
        }
        var Layout = Hostedˉcompilerˉruntimeˉdata.Plan(target);
        var Bytes = new byte[checked((int)Hostedˉcompilerˉruntimeˉdata.HEADER_BYTES)];
        Writeˉcontext(Bytes);
        Writeˉserviceˉtable(Bytes);
        Writeˉoutputˉtable(Bytes, target);
        Writeˉfileˉinputˉtable(Bytes, Layout);
        Writeˉfileˉoutputˉtable(Bytes, Layout);
        metadata.CopyTo(Bytes, checked((int)Hostedˉcompilerˉruntimeˉdata.METADATA_OFFSET));
        return Bytes.ToImmutableArray();
    }

    private static void Writeˉcontext(byte[] bytes)
    {
        var Base = checked((int)Hostedˉcompilerˉruntimeˉdata.CONTEXT_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET,
            Nativeˉexecutionˉcontextˉcontract.SIZE);
        Writeˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET,
            Hostedˉcompilerˉapplicationˉmetadata.COMPILER_MAXIMUM_INSTRUCTIONS);
        Writeˉu64(bytes, Base + Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET,
            checked((ulong)Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH));
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.RECORD_ARENA_BYTES);
        Writeˉu32(bytes, Base + Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            Nativeˉconsoleˉapplicationˉcontract.HOSTED_TEXT_ARENA_BYTES);
    }

    private static void Writeˉserviceˉtable(byte[] bytes)
    {
        var Base = checked((int)Hostedˉcompilerˉruntimeˉdata.SERVICE_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉserviceˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉserviceˉtableˉcontract.SIZE_OFFSET,
            Nativeˉserviceˉtableˉcontract.SIZE);
    }

    private static void Writeˉoutputˉtable(
        byte[] bytes,
        Consoleˉapplicationˉtarget target)
    {
        var Base = checked((int)Hostedˉcompilerˉruntimeˉdata.OUTPUT_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉoutputˉtableˉcontract.MAGIC);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉoutputˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉoutputˉtableˉcontract.SIZE);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.PLATFORM_OFFSET, (uint)target);
        Writeˉu32(bytes, Base + Nativeˉoutputˉtableˉcontract.FLAGS_OFFSET,
            Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT |
                Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_PRESENT);
        if (target == Consoleˉapplicationˉtarget.Linuxˉx64)
        {
            Writeˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.CONSOLE_TARGET_OFFSET, 1);
            Writeˉu64(bytes, Base + Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_TARGET_OFFSET, 2);
        }
    }

    private static void Writeˉfileˉinputˉtable(
        byte[] bytes,
        Hostedˉcompilerˉruntimeˉlayout layout)
    {
        var Base = checked((int)Hostedˉcompilerˉruntimeˉdata.FILE_INPUT_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.MAGIC);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.SIZE);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)layout.Target);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_OFFSET,
            Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.MAXIMUM_DATA_BYTES_OFFSET,
            Bytecodeˉlimits.MAX_BYTE_DATA_BYTES);
        Writeˉu32(bytes, Base + Nativeˉfileˉinputˉtableˉcontract.SCRATCH_BYTES_OFFSET,
            layout.Fileˉinputˉscratchˉbytes);
    }

    private static void Writeˉfileˉoutputˉtable(
        byte[] bytes,
        Hostedˉcompilerˉruntimeˉlayout layout)
    {
        var Base = checked((int)Hostedˉcompilerˉruntimeˉdata.FILE_OUTPUT_TABLE_OFFSET);
        Writeˉu32(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.MAGIC_OFFSET,
            Nativeˉfileˉoutputˉtableˉcontract.MAGIC);
        Writeˉu32(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET,
            Nativeˉfileˉoutputˉtableˉcontract.FORMAT_VERSION);
        Writeˉu32(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.SIZE_OFFSET,
            Nativeˉfileˉoutputˉtableˉcontract.SIZE);
        Writeˉu32(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.PLATFORM_OFFSET,
            (uint)layout.Target);
        Writeˉu32(bytes, Base + Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_BYTES_OFFSET,
            layout.Fileˉoutputˉscratchˉbytes);
    }

    private static void Writeˉu32(byte[] bytes, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset, sizeof(uint)), value);

    private static void Writeˉu64(byte[] bytes, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset, sizeof(ulong)), value);
}
