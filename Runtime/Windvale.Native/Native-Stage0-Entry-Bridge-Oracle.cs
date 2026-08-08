using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉstage0ˉentryˉbridgeˉoracle
{
    public static ImmutableArray<byte> Build(Nativeˉentryˉbridgeˉinputs inputs)
    {
        Nativeˉentryˉbridgeˉbuilder.Verifyˉinputs(inputs);
        var Result = new byte[
            inputs.Input == Nativeˉentryˉinputˉkind.Bytes
                ? 2 * Nativeˉcontract.VALUE_SLOT_BYTES
                : Nativeˉcontract.VALUE_SLOT_BYTES];
        if (inputs.Input == Nativeˉentryˉinputˉkind.Bytes)
        {
            var Descriptor = Result.AsSpan(Nativeˉcontract.VALUE_SLOT_BYTES);
            BinaryPrimitives.WriteUInt64LittleEndian(Descriptor, inputs.Inputˉpointer);
            BinaryPrimitives.WriteUInt32LittleEndian(Descriptor[8..], inputs.Inputˉlength);
        }
        return Result.ToImmutableArray();
    }
}
