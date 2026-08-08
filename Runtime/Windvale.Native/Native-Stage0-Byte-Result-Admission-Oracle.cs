using Windvale.Bytecode;

namespace Windvale.Runtime.Native;

internal static class Nativeˉstage0ˉbyteˉresultˉadmissionˉoracle
{
    public static bool Admit(Nativeˉbyteˉresultˉadmissionˉinputs inputs)
    {
        Nativeˉbyteˉresultˉadmissionˉbuilder.Verifyˉinputs(inputs);
        var Descriptor = inputs.Descriptor;
        if (Descriptor.Reserved != 0 || Descriptor.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            return false;
        }
        if (Descriptor.Pointer == 0)
        {
            return Descriptor.Length == 0;
        }
        if (Isˉinside(Descriptor.Pointer, Descriptor.Length, inputs.Arenaˉstart, inputs.Arenaˉused) ||
            (inputs.Inputˉstart != 0 && Isˉinside(
                Descriptor.Pointer,
                Descriptor.Length,
                inputs.Inputˉstart,
                inputs.Inputˉlength)))
        {
            return true;
        }
        return inputs.Staticˉranges.Any(Range => Isˉinside(
            Descriptor.Pointer,
            Descriptor.Length,
            Range.Start,
            Range.Available));
    }

    private static bool Isˉinside(ulong pointer, uint length, ulong start, uint available)
    {
        if (pointer < start)
        {
            return false;
        }
        var Offset = pointer - start;
        return Offset <= available && length <= available - Offset;
    }
}
