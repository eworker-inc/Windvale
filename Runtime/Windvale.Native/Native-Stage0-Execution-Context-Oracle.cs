using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉstage0ˉexecutionˉcontextˉoracle
{
    public static ImmutableArray<byte> Build(Nativeˉexecutionˉcontextˉinputs inputs)
    {
        Nativeˉexecutionˉcontextˉbuilder.Verifyˉinputs(inputs);
        var Result = new byte[checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE)];
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET),
            Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET),
            Nativeˉexecutionˉcontextˉcontract.SIZE);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET),
            inputs.Maximumˉinstructions);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET),
            inputs.Maximumˉcallˉdepth);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET),
            inputs.Serviceˉtableˉpointer);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET),
            inputs.Recordˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET),
            inputs.Recordˉarenaˉlength);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET),
            inputs.Textˉarenaˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET),
            inputs.Textˉarenaˉlength);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.ARGUMENT_TABLE_POINTER_OFFSET),
            inputs.Argumentˉtableˉpointer);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET),
            inputs.Argumentˉcount);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.OUTPUT_TABLE_POINTER_OFFSET),
            inputs.Outputˉtableˉpointer);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET),
            inputs.Fileˉinputˉtableˉpointer);
        BinaryPrimitives.WriteUInt64LittleEndian(
            Result.AsSpan(Nativeˉexecutionˉcontextˉcontract.FILE_OUTPUT_TABLE_POINTER_OFFSET),
            inputs.Fileˉoutputˉtableˉpointer);
        return Result.ToImmutableArray();
    }
}
