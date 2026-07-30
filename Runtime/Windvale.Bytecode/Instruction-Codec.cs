using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Bytecode;

public static class Instructionˉcodec
{
    public static ImmutableArray<Decodedˉinstruction> Decode(
        ReadOnlySpan<byte> code,
        string functionˉname)
    {
        if (code.Length > Bytecodeˉlimits.MAX_CODE_BYTES_PER_FUNCTION)
        {
            throw new Moduleˉverificationˉexception(
                "WVB2001",
                $"Function '{functionˉname}' exceeds the code-size limit.");
        }

        var Instructions = ImmutableArray.CreateBuilder<Decodedˉinstruction>();
        var Offset = 0;

        while (Offset < code.Length)
        {
            if (Instructions.Count >= Bytecodeˉlimits.MAX_INSTRUCTIONS_PER_FUNCTION)
            {
                throw new Moduleˉverificationˉexception(
                    "WVB2002",
                    $"Function '{functionˉname}' exceeds the instruction-count limit.",
                    Offset);
            }

            var Start = Offset;
            var Rawˉopcode = code[Offset++];
            if (!Enum.IsDefined(typeof(Opcode), Rawˉopcode))
            {
                throw new Moduleˉverificationˉexception(
                    "WVB2003",
                    $"Unknown opcode 0x{Rawˉopcode:X2} in function '{functionˉname}'.",
                    Start);
            }

            var Opcode = (Opcode)Rawˉopcode;
            var Signedˉoperand = 0;
            uint Unsignedˉoperand = 0;

            switch (Opcode)
            {
                case Opcode.I32ˉconst:
                    Requireˉoperand(code, Offset, sizeof(int), functionˉname, Start);
                    Signedˉoperand = BinaryPrimitives.ReadInt32LittleEndian(code[Offset..]);
                    Offset += sizeof(int);
                    break;

                case Opcode.Boolˉconst:
                    Requireˉoperand(code, Offset, sizeof(byte), functionˉname, Start);
                    Unsignedˉoperand = code[Offset++];
                    if (Unsignedˉoperand > 1)
                    {
                        throw new Moduleˉverificationˉexception(
                            "WVB2004",
                            "A bool.const operand must be zero or one.",
                            Start);
                    }

                    break;

                case Opcode.U8ˉconst:
                    Requireˉoperand(code, Offset, sizeof(byte), functionˉname, Start);
                    Unsignedˉoperand = code[Offset++];
                    break;

                case Opcode.Textˉconst:
                case Opcode.U32ˉconst:
                case Opcode.Bytesˉconst:
                case Opcode.Localˉload:
                case Opcode.Localˉstore:
                case Opcode.Dataˉlength:
                case Opcode.Dataˉloadˉi32:
                case Opcode.Jump:
                case Opcode.Branchˉfalse:
                case Opcode.Call:
                case Opcode.Callˉcapability:
                    Requireˉoperand(code, Offset, sizeof(uint), functionˉname, Start);
                    Unsignedˉoperand = BinaryPrimitives.ReadUInt32LittleEndian(code[Offset..]);
                    Offset += sizeof(uint);
                    break;
            }

            Instructions.Add(new(
                Start,
                Offset - Start,
                Opcode,
                Signedˉoperand,
                Unsignedˉoperand));
        }

        if (Instructions.Count == 0)
        {
            throw new Moduleˉverificationˉexception(
                "WVB2005",
                $"Function '{functionˉname}' has no instructions.");
        }

        return Instructions.ToImmutable();
    }

    public static int Getˉencodedˉsize(Opcode opcode)
    {
        return opcode switch
        {
            Opcode.I32ˉconst => 5,
            Opcode.Boolˉconst or Opcode.U8ˉconst => 2,
            Opcode.Textˉconst or
            Opcode.U32ˉconst or
            Opcode.Bytesˉconst or
            Opcode.Localˉload or
            Opcode.Localˉstore or
            Opcode.Dataˉlength or
            Opcode.Dataˉloadˉi32 or
            Opcode.Jump or
            Opcode.Branchˉfalse or
            Opcode.Call or
            Opcode.Callˉcapability => 5,
            _ => 1,
        };
    }

    private static void Requireˉoperand(
        ReadOnlySpan<byte> code,
        int offset,
        int requiredˉbytes,
        string functionˉname,
        int instructionˉoffset)
    {
        if (requiredˉbytes > code.Length - offset)
        {
            throw new Moduleˉverificationˉexception(
                "WVB2006",
                $"Instruction in function '{functionˉname}' has a truncated operand.",
                instructionˉoffset);
        }
    }
}
