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
        Output.AppendLine($"Windvale bytecode {Moduleˉcodec.MAJOR_VERSION}.{Module.Formatˉminorˉversion}");
        Output.AppendLine($"SHA-256: {Moduleˉdigest.Calculateˉsha256(originalˉbytes)}");
        Output.AppendLine($"Module: {Module.Name}");
        Output.AppendLine($"Profile: {Formatˉprofile(Module.Profile)}");
        if (Module.Metadata is { } Metadata)
        {
            Output.AppendLine($"Authority: {Metadata.Authority.ToString().ToLowerInvariant()}");
            Output.AppendLine($"Platforms: {string.Join(", ", Metadata.Platformˉscopes)}");
            Output.AppendLine($"Required capabilities: {Formatˉrequirements(Metadata.Requiredˉcapabilities)}");
            Output.AppendLine($"Optional capabilities: {Formatˉrequirements(Metadata.Optionalˉcapabilities)}");
        }
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
                case Bytesˉdataˉdeclaration Bytes:
                    Output.Append($"bytes length={Bytes.Values.Length} values=");
                    Output.AppendLine(Formatˉbytesˉpreview(Bytes.Values));
                    break;
            }
        }

        Output.AppendLine();
        Output.AppendLine($"Nominal types ({Module.Types.Length})");
        for (var Typeˉindex = 0; Typeˉindex < Module.Types.Length; Typeˉindex++)
        {
            var Type = Module.Types[Typeˉindex];
            switch (Type)
            {
                case Recordˉtypeˉdeclaration Record:
                    Output.AppendLine($"  [{Typeˉindex}] record {Record.Name}");
                    for (var Fieldˉindex = 0; Fieldˉindex < Record.Fields.Length; Fieldˉindex++)
                    {
                        var Field = Record.Fields[Fieldˉindex];
                        Output.AppendLine(
                            $"    [{Fieldˉindex}] {Field.Name}: {Formatˉshape(Module, Field.Type)}");
                    }

                    break;
                case Enumˉtypeˉdeclaration Enum:
                    Output.AppendLine($"  [{Typeˉindex}] enum {Enum.Name}");
                    for (var Memberˉindex = 0; Memberˉindex < Enum.Members.Length; Memberˉindex++)
                    {
                        var Member = Enum.Members[Memberˉindex];
                        Output.AppendLine($"    [{Memberˉindex}] {Member.Name} = {Member.Value}");
                    }

                    break;
                case Variantˉtypeˉdeclaration Variant:
                    Output.AppendLine($"  [{Typeˉindex}] variant {Variant.Name}");
                    for (var Caseˉindex = 0; Caseˉindex < Variant.Cases.Length; Caseˉindex++)
                    {
                        var Case = Variant.Cases[Caseˉindex];
                        var Payload = Case.Payloadˉtype is { } Payloadˉtype
                            ? $"({Case.Payloadˉname}: {Formatˉshape(Module, Payloadˉtype)})"
                            : string.Empty;
                        Output.AppendLine($"    [{Caseˉindex}] {Case.Name}{Payload}");
                    }

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
            Output.Append(string.Join(", ", Declaration.Parameterˉtypes.Select(
                Type => Formatˉshape(Module, Type))));
            Output.Append(") -> ");
            Output.Append(Formatˉshape(Module, Declaration.Returnˉtype));
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

    private static string Formatˉrequirements(
        System.Collections.Immutable.ImmutableArray<Capabilityˉrequirement> requirements)
    {
        return requirements.Length == 0
            ? "none"
            : string.Join(", ", requirements.Select(Requirement =>
                $"{Requirement.Name}@{Requirement.Majorˉversion}"));
    }

    public static string Formatˉtype(Valueˉtype type)
    {
        return type switch
        {
            Valueˉtype.Void => "void",
            Valueˉtype.I32 => "i32",
            Valueˉtype.I64 => "i64",
            Valueˉtype.Bool => "bool",
            Valueˉtype.Text => "text",
            Valueˉtype.U8 => "u8",
            Valueˉtype.U32 => "u32",
            Valueˉtype.U64 => "u64",
            Valueˉtype.Bytes => "bytes",
            Valueˉtype.Record => "record",
            Valueˉtype.Enum => "enum",
            Valueˉtype.Variant => "variant",
            _ => $"unknown({(byte)type})",
        };
    }

    private static string Formatˉshape(Bytecodeˉmodule module, Valueˉshape shape)
    {
        return (shape.Kind is Valueˉtype.Record or Valueˉtype.Enum or Valueˉtype.Variant) &&
            (uint)shape.Nominalˉtypeˉindex < (uint)module.Types.Length
                ? module.Types[shape.Nominalˉtypeˉindex].Name
                : Formatˉtype(shape.Kind);
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
            Opcode.I64ˉconst => $"i64.const {instruction.Signedˉwideˉoperand}",
            Opcode.Boolˉconst => $"bool.const {(instruction.Unsignedˉoperand == 1 ? "true" : "false")}",
            Opcode.U8ˉconst => $"u8.const {instruction.Unsignedˉoperand}",
            Opcode.U32ˉconst => $"u32.const {instruction.Unsignedˉoperand}",
            Opcode.U64ˉconst => $"u64.const {instruction.Unsignedˉwideˉoperand}",
            Opcode.Textˉconst => $"text.const data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Bytesˉconst => $"bytes.const data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Localˉload => $"local.load {instruction.Unsignedˉoperand}",
            Opcode.Localˉstore => $"local.store {instruction.Unsignedˉoperand}",
            Opcode.Dataˉlength => $"data.length data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Dataˉloadˉi32 => $"data.load.i32 data[{instruction.Unsignedˉoperand}] ({module.Data[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Bytesˉlength => "bytes.length",
            Opcode.Bytesˉslice => "bytes.slice",
            Opcode.Bytesˉreadˉu8 => "bytes.read_u8",
            Opcode.Bytesˉreadˉu16ˉlittle => "bytes.read_u16_little",
            Opcode.Bytesˉreadˉu32ˉlittle => "bytes.read_u32_little",
            Opcode.Bytesˉreadˉi32ˉlittle => "bytes.read_i32_little",
            Opcode.Bytesˉreadˉu64ˉlittle => "bytes.read_u64_little",
            Opcode.Recordˉcreate =>
                $"record.create type[{instruction.Unsignedˉoperand}] ({module.Types[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Recordˉfield => $"record.field {instruction.Unsignedˉoperand}",
            Opcode.Enumˉconst => Formatˉenumˉconstant(module, instruction),
            Opcode.Enumˉequal => "enum.equal",
            Opcode.Enumˉnotˉequal => "enum.not_equal",
            Opcode.Enumˉname => "enum.name",
            Opcode.Variantˉcreate => Formatˉvariantˉinstruction(module, instruction, "variant.create"),
            Opcode.Variantˉisˉcase => Formatˉvariantˉinstruction(module, instruction, "variant.is_case"),
            Opcode.Variantˉpayload => Formatˉvariantˉinstruction(module, instruction, "variant.payload"),
            Opcode.I32ˉformat => "i32.format",
            Opcode.I64ˉformat => "i64.format",
            Opcode.U8ˉformat => "u8.format",
            Opcode.U32ˉformat => "u32.format",
            Opcode.U64ˉformat => "u64.format",
            Opcode.Textˉconcat => "text.concat",
            Opcode.Textˉutf8ˉisˉvalid => "text.utf8_is_valid",
            Opcode.Textˉfromˉutf8 => "text.from_utf8",
            Opcode.Textˉquote => "text.quote",
            Opcode.U32ˉfromˉu8 => "u32.from_u8",
            Opcode.Bytesˉconcat => "bytes.concat",
            Opcode.Bytesˉfromˉu8 => "bytes.from_u8",
            Opcode.Bytesˉfromˉu16ˉlittle => "bytes.from_u16_little",
            Opcode.Bytesˉfromˉu32ˉlittle => "bytes.from_u32_little",
            Opcode.Bytesˉfromˉi32ˉlittle => "bytes.from_i32_little",
            Opcode.Bytesˉfromˉu64ˉlittle => "bytes.from_u64_little",
            Opcode.Bytesˉsha256ˉhex => "bytes.sha256_hex",
            Opcode.Textˉtoˉutf8 => "text.to_utf8",
            Opcode.I32ˉadd => "i32.add",
            Opcode.I32ˉsubtract => "i32.subtract",
            Opcode.I32ˉmultiply => "i32.multiply",
            Opcode.I32ˉnegate => "i32.negate",
            Opcode.I64ˉadd => "i64.add",
            Opcode.I64ˉsubtract => "i64.subtract",
            Opcode.I64ˉmultiply => "i64.multiply",
            Opcode.I64ˉnegate => "i64.negate",
            Opcode.U32ˉadd => "u32.add",
            Opcode.U32ˉsubtract => "u32.subtract",
            Opcode.U32ˉmultiply => "u32.multiply",
            Opcode.U64ˉadd => "u64.add",
            Opcode.U64ˉsubtract => "u64.subtract",
            Opcode.U64ˉmultiply => "u64.multiply",
            Opcode.I32ˉequal => "i32.equal",
            Opcode.I32ˉnotˉequal => "i32.not_equal",
            Opcode.I32ˉless => "i32.less",
            Opcode.I32ˉlessˉequal => "i32.less_equal",
            Opcode.I32ˉgreater => "i32.greater",
            Opcode.I32ˉgreaterˉequal => "i32.greater_equal",
            Opcode.I64ˉequal => "i64.equal",
            Opcode.I64ˉnotˉequal => "i64.not_equal",
            Opcode.I64ˉless => "i64.less",
            Opcode.I64ˉlessˉequal => "i64.less_equal",
            Opcode.I64ˉgreater => "i64.greater",
            Opcode.I64ˉgreaterˉequal => "i64.greater_equal",
            Opcode.Boolˉequal => "bool.equal",
            Opcode.Boolˉnotˉequal => "bool.not_equal",
            Opcode.Boolˉnot => "bool.not",
            Opcode.U32ˉequal => "u32.equal",
            Opcode.U32ˉnotˉequal => "u32.not_equal",
            Opcode.U32ˉless => "u32.less",
            Opcode.U32ˉlessˉequal => "u32.less_equal",
            Opcode.U32ˉgreater => "u32.greater",
            Opcode.U32ˉgreaterˉequal => "u32.greater_equal",
            Opcode.U64ˉequal => "u64.equal",
            Opcode.U64ˉnotˉequal => "u64.not_equal",
            Opcode.U64ˉless => "u64.less",
            Opcode.U64ˉlessˉequal => "u64.less_equal",
            Opcode.U64ˉgreater => "u64.greater",
            Opcode.U64ˉgreaterˉequal => "u64.greater_equal",
            Opcode.U8ˉequal => "u8.equal",
            Opcode.U8ˉnotˉequal => "u8.not_equal",
            Opcode.I32ˉdivide => "i32.divide",
            Opcode.I32ˉremainder => "i32.remainder",
            Opcode.U32ˉdivide => "u32.divide",
            Opcode.U32ˉremainder => "u32.remainder",
            Opcode.I64ˉdivide => "i64.divide",
            Opcode.I64ˉremainder => "i64.remainder",
            Opcode.U64ˉdivide => "u64.divide",
            Opcode.U64ˉremainder => "u64.remainder",
            Opcode.U8ˉbitwiseˉand => "u8.bitwise_and",
            Opcode.U8ˉbitwiseˉor => "u8.bitwise_or",
            Opcode.U8ˉbitwiseˉxor => "u8.bitwise_xor",
            Opcode.U8ˉbitwiseˉnot => "u8.bitwise_not",
            Opcode.U8ˉshiftˉleft => "u8.shift_left",
            Opcode.U8ˉshiftˉright => "u8.shift_right",
            Opcode.U32ˉbitwiseˉand => "u32.bitwise_and",
            Opcode.U32ˉbitwiseˉor => "u32.bitwise_or",
            Opcode.U32ˉbitwiseˉxor => "u32.bitwise_xor",
            Opcode.U32ˉbitwiseˉnot => "u32.bitwise_not",
            Opcode.U32ˉshiftˉleft => "u32.shift_left",
            Opcode.U32ˉshiftˉright => "u32.shift_right",
            Opcode.U64ˉbitwiseˉand => "u64.bitwise_and",
            Opcode.U64ˉbitwiseˉor => "u64.bitwise_or",
            Opcode.U64ˉbitwiseˉxor => "u64.bitwise_xor",
            Opcode.U64ˉbitwiseˉnot => "u64.bitwise_not",
            Opcode.U64ˉshiftˉleft => "u64.shift_left",
            Opcode.U64ˉshiftˉright => "u64.shift_right",
            Opcode.Textˉequal => "text.equal",
            Opcode.Textˉnotˉequal => "text.not_equal",
            Opcode.Bytesˉequal => "bytes.equal",
            Opcode.Bytesˉnotˉequal => "bytes.not_equal",
            Opcode.Jump => $"jump {instruction.Unsignedˉoperand:X4}",
            Opcode.Branchˉfalse => $"branch.false {instruction.Unsignedˉoperand:X4}",
            Opcode.Call => $"call function[{instruction.Unsignedˉoperand}] ({module.Functions[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Callˉcapability => $"call.capability capability[{instruction.Unsignedˉoperand}] ({module.Capabilities[(int)instruction.Unsignedˉoperand].Name})",
            Opcode.Pop => "pop",
            Opcode.Return => "return",
            _ => $"unknown 0x{(byte)instruction.Opcode:X2}",
        };
    }

    private static string Formatˉenumˉconstant(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction)
    {
        var Enum = (Enumˉtypeˉdeclaration)module.Types[(int)instruction.Unsignedˉoperand];
        var Member = Enum.Members[(int)instruction.Secondˉunsignedˉoperand];
        return $"enum.const type[{instruction.Unsignedˉoperand}] ({Enum.Name}) " +
            $"member[{instruction.Secondˉunsignedˉoperand}] ({Member.Name}={Member.Value})";
    }

    private static string Formatˉvariantˉinstruction(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string operation)
    {
        var Variant = (Variantˉtypeˉdeclaration)module.Types[(int)instruction.Unsignedˉoperand];
        var Case = Variant.Cases[(int)instruction.Secondˉunsignedˉoperand];
        return $"{operation} type[{instruction.Unsignedˉoperand}] ({Variant.Name}) " +
            $"case[{instruction.Secondˉunsignedˉoperand}] ({Case.Name})";
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

    private static string Formatˉbytesˉpreview(System.Collections.Immutable.ImmutableArray<byte> values)
    {
        const int MAX_PREVIEW_VALUES = 16;
        var Preview = string.Join(" ", values.Take(MAX_PREVIEW_VALUES).Select(Value => $"{Value:X2}"));
        return values.Length <= MAX_PREVIEW_VALUES
            ? $"[{Preview}]"
            : $"[{Preview} …]";
    }
}
