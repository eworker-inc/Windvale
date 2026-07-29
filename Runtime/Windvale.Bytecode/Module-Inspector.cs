using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Windvale.Bytecode;

public static class Moduleˉdigest
{
    public static string Calculateˉsha256(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }
}

public static class Moduleˉinspector
{
    public static string Inspect(Verifiedˉmodule verifiedˉmodule, ReadOnlySpan<byte> originalˉbytes)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        var Module = verifiedˉmodule.Module;
        var Output = new StringBuilder();
        Output.AppendLine($"Windvale bytecode {Moduleˉcodec.MAJOR_VERSION}.{Moduleˉcodec.MINOR_VERSION}");
        Output.AppendLine($"SHA-256: {Moduleˉdigest.Calculateˉsha256(originalˉbytes)}");
        Output.AppendLine($"Module: {Module.Name}");
        Output.AppendLine($"Profile: {Formatˉprofile(Module.Profile)}");
        Output.AppendLine();

        Output.AppendLine($"Capabilities ({Module.Capabilities.Length})");
        foreach (var Capability in Module.Capabilities)
        {
            Output.Append("  ");
            Output.Append(Capability.Name);
            Output.Append('(');
            Output.Append(string.Join(", ", Capability.Parameterˉtypes.Select(Formatˉtype)));
            Output.Append(") -> ");
            Output.AppendLine(Formatˉtype(Capability.Returnˉtype));
        }

        Output.AppendLine();
        Output.AppendLine($"Data ({Module.Data.Length})");
        for (var Index = 0; Index < Module.Data.Length; Index++)
        {
            var Data = Module.Data[Index];
            Output.Append($"  [{Index}] {Data.Name}: ");
            switch (Data)
            {
                case Textˉdataˉdeclaration Text:
                    Output.Append("text = ");
                    Output.AppendLine(Formatˉtextˉpreview(Text.Value));
                    break;
                case I32ˉarrayˉdataˉdeclaration Array:
                    Output.Append($"[i32] length={Array.Values.Length} values=");
                    Output.AppendLine(Formatˉarrayˉpreview(Array.Values));
                    break;
            }
        }

        Output.AppendLine();
        Output.AppendLine($"Functions ({Module.Functions.Length})");
        for (var Functionˉindex = 0; Functionˉindex < verifiedˉmodule.Functions.Length; Functionˉindex++)
        {
            var Function = verifiedˉmodule.Functions[Functionˉindex];
            var Declaration = Function.Declaration;
            Output.Append($"  [{Functionˉindex}] {Declaration.Name}(");
            Output.Append(string.Join(", ", Declaration.Parameterˉtypes.Select(Formatˉtype)));
            Output.Append(") -> ");
            Output.Append(Formatˉtype(Declaration.Returnˉtype));
            Output.Append($" locals={Declaration.Localˉtypes.Length}");
            Output.Append($" max-stack={Declaration.Maximumˉstackˉdepth}");
            Output.AppendLine();

            foreach (var Instruction in Function.Instructions)
            {
                Output.Append($"    {Instruction.Offset:X4}  ");
                Output.AppendLine(Formatˉinstruction(Module, Instruction));
            }
        }

        Output.AppendLine();
        Output.AppendLine($"Exports ({Module.Exports.Length})");
        foreach (var Export in Module.Exports)
        {
            Output.AppendLine($"  {Export.Name} -> function[{Export.Targetˉindex}]");
        }

        return Output.ToString();
    }

    public static string Formatˉtype(Valueˉtype type)
    {
        return type switch
        {
            Valueˉtype.Void => "void",
            Valueˉtype.I32 => "i32",
            Valueˉtype.Bool => "bool",
            Valueˉtype.Text => "text",
            _ => $"unknown({(byte)type})",
        };
    }

    private static string Formatˉprofile(Moduleˉprofile profile)
    {
        return profile switch
        {
            Moduleˉprofile.Portable => "portable",
            Moduleˉprofile.Hosted => "hosted",
            Moduleˉprofile.System => "system",
            _ => $"unknown({(byte)profile})",
        };
    }

    private static string Formatˉinstruction(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction)
    {
        return instruction.Opcode switch
        {
            Opcode.I32ˉconst => $"i32.const {instruction.Signedˉoperand}",
            Opcode.Boolˉconst => $"bool.const {(instruction.Unsignedˉoperand == 1 ? "true" : "false")}",
            Opcode.Textˉconst => $"text.const data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Localˉload => $"local.load {instruction.Unsignedˉoperand}",
            Opcode.Localˉstore => $"local.store {instruction.Unsignedˉoperand}",
            Opcode.Dataˉlength => $"data.length data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Dataˉloadˉi32 => $"data.load.i32 data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.I32ˉadd => "i32.add",
            Opcode.I32ˉsubtract => "i32.subtract",
            Opcode.I32ˉmultiply => "i32.multiply",
            Opcode.I32ˉnegate => "i32.negate",
            Opcode.I32ˉequal => "i32.equal",
            Opcode.I32ˉnotˉequal => "i32.not_equal",
            Opcode.I32ˉless => "i32.less",
            Opcode.I32ˉlessˉequal => "i32.less_equal",
            Opcode.I32ˉgreater => "i32.greater",
            Opcode.I32ˉgreaterˉequal => "i32.greater_equal",
            Opcode.Boolˉequal => "bool.equal",
            Opcode.Boolˉnotˉequal => "bool.not_equal",
            Opcode.Boolˉnot => "bool.not",
            Opcode.Jump => $"jump {instruction.Unsignedˉoperand:X4}",
            Opcode.Branchˉfalse => $"branch.false {instruction.Unsignedˉoperand:X4}",
            Opcode.Call => $"call function[{instruction.Unsignedˉoperand}] ({module.Functions[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Callˉcapability => $"call.capability capability[{instruction.Unsignedˉoperand}] ({module.Capabilities[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Pop => "pop",
            Opcode.Return => "return",
            _ => $"unknown 0x{(byte)instruction.Opcode:X2}",
        };
    }

    private static string Formatˉtextˉpreview(string value)
    {
        const int MAX_PREVIEW_CHARACTERS = 80;
        var Builder = new StringBuilder();
        var Runeˉcount = 0;
        var Isˉtruncated = false;
        foreach (var Rune in value.EnumerateRunes())
        {
            if (Runeˉcount == MAX_PREVIEW_CHARACTERS)
            {
                Isˉtruncated = true;
                break;
            }

            Builder.Append(Rune.ToString());
            Runeˉcount++;
        }

        if (Isˉtruncated)
        {
            Builder.Append('…');
        }

        var Preview = Builder.ToString();
        return $"{JsonSerializer.Serialize(Preview)} utf8-bytes={Encoding.UTF8.GetByteCount(value)}";
    }

    private static string Formatˉarrayˉpreview(System.Collections.Immutable.ImmutableArray<int> values)
    {
        const int MAX_PREVIEW_VALUES = 16;
        var Preview = string.Join(", ", values.Take(MAX_PREVIEW_VALUES));
        return values.Length <= MAX_PREVIEW_VALUES
            ? $"[{Preview}]"
            : $"[{Preview}, …]";
    }
}
