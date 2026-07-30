using System.Collections.Immutable;
using System.Text;

namespace Windvale.Bytecode;

public static class Moduleˉverifier
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Verifiedˉmodule Verify(Bytecodeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        Verifyˉmoduleˉmetadata(module);

        var Verifiedˉfunctions = ImmutableArray.CreateBuilder<Verifiedˉfunction>(module.Functions.Length);
        foreach (var Function in module.Functions)
        {
            var Functionˉcode = module.Code.AsSpan(Function.Codeˉoffset, Function.Codeˉlength);
            var Instructions = Instructionˉcodec.Decode(Functionˉcode, Function.Name);
            Verifyˉfunction(module, Function, Instructions);
            Verifiedˉfunctions.Add(new(Function, Instructions));
        }

        return new(module, Verifiedˉfunctions.ToImmutable());
    }

    private static void Verifyˉmoduleˉmetadata(Bytecodeˉmodule module)
    {
        if (!Seedˉnames.Isˉidentifier(module.Name))
        {
            Fail("WVB2100", $"Module name '{module.Name}' is not a Seed identifier.");
        }

        if (!Enum.IsDefined(module.Profile))
        {
            Fail("WVB2101", $"Module profile '{module.Profile}' is invalid.");
        }

        if (module.Capabilities.Length > Bytecodeˉlimits.MAX_CAPABILITIES)
        {
            Fail("WVB2102", "The module has too many capabilities.");
        }

        if (module.Data.Length > Bytecodeˉlimits.MAX_DATA_DECLARATIONS)
        {
            Fail("WVB2103", "The module has too many data declarations.");
        }

        if (module.Functions.Length > Bytecodeˉlimits.MAX_FUNCTIONS)
        {
            Fail("WVB2104", "The module has too many functions.");
        }

        if (module.Code.Length > Bytecodeˉlimits.MAX_MODULE_BYTES)
        {
            Fail("WVB2105", "The code section exceeds the module-size limit.");
        }

        Verifyˉcapabilities(module);
        Verifyˉdata(module);
        Verifyˉfunctionˉmetadata(module);
        Verifyˉexports(module);
    }

    private static void Verifyˉcapabilities(Bytecodeˉmodule module)
    {
        if (module.Profile == Moduleˉprofile.Portable && module.Capabilities.Length != 0)
        {
            Fail("WVB2110", "A portable module cannot declare capabilities.");
        }

        Verifyˉstrictˉordering(
            module.Capabilities.Select(Capability => Capability.Name),
            "capability");

        foreach (var Capability in module.Capabilities)
        {
            if (!Seedˉnames.Isˉcapability(Capability.Name))
            {
                Fail("WVB2111", $"Capability name '{Capability.Name}' is invalid.");
            }

            if (!Capabilityˉcatalog.Tryˉget(Capability.Name, out var Canonical))
            {
                Fail("WVB2112", $"Capability '{Capability.Name}' is not defined by Windvale Seed.");
            }

            if (!Capability.Parameterˉtypes.SequenceEqual(Canonical.Parameterˉtypes) ||
                Capability.Returnˉtype != Canonical.Returnˉtype)
            {
                Fail("WVB2113", $"Capability '{Capability.Name}' has a non-canonical signature.");
            }

            foreach (var Parameterˉtype in Capability.Parameterˉtypes)
            {
                Verifyˉvalueˉtype(Parameterˉtype, allowˉvoid: false, "capability parameter");
            }

            Verifyˉvalueˉtype(Capability.Returnˉtype, allowˉvoid: true, "capability return");
        }
    }

    private static void Verifyˉdata(Bytecodeˉmodule module)
    {
        Verifyˉstrictˉordering(module.Data.Select(Data => Data.Name), "data declaration");
        foreach (var Data in module.Data)
        {
            if (!Seedˉnames.Isˉidentifier(Data.Name))
            {
                Fail("WVB2120", $"Data name '{Data.Name}' is not a Seed identifier.");
            }

            switch (Data)
            {
                case Textˉdataˉdeclaration Text when Text.Type == Dataˉtype.Text:
                    int Utf8ˉlength;
                    try
                    {
                        Utf8ˉlength = STRICT_UTF8.GetByteCount(Text.Value);
                    }
                    catch (EncoderFallbackException)
                    {
                        Fail("WVB2124", $"Text data '{Text.Name}' contains an unpaired Unicode surrogate.");
                        break;
                    }

                    if (Utf8ˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
                    {
                        Fail("WVB2121", $"Text data '{Text.Name}' exceeds the UTF-8 value limit.");
                    }

                    break;
                case I32ˉarrayˉdataˉdeclaration Array when Array.Type == Dataˉtype.I32ˉarray:
                    if (Array.Values.Length > Bytecodeˉlimits.MAX_I32_ARRAY_ELEMENTS)
                    {
                        Fail("WVB2122", $"Array data '{Array.Name}' exceeds the element limit.");
                    }

                    break;
                case Bytesˉdataˉdeclaration Bytes when Bytes.Type == Dataˉtype.Bytes:
                    if (Bytes.Values.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
                    {
                        Fail("WVB2125", $"Byte data '{Bytes.Name}' exceeds the byte-data limit.");
                    }

                    break;
                default:
                    Fail("WVB2123", $"Data declaration '{Data.Name}' has an inconsistent representation.");
                    break;
            }
        }
    }

    private static void Verifyˉfunctionˉmetadata(Bytecodeˉmodule module)
    {
        Verifyˉstrictˉordering(module.Functions.Select(Function => Function.Name), "function");
        var Expectedˉcodeˉoffset = 0;

        foreach (var Function in module.Functions)
        {
            if (!Seedˉnames.Isˉidentifier(Function.Name))
            {
                Fail("WVB2130", $"Function name '{Function.Name}' is not a Seed identifier.");
            }

            if (Function.Parameterˉtypes.Length > Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS ||
                Function.Localˉtypes.Length > Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS ||
                Function.Parameterˉtypes.Length + Function.Localˉtypes.Length >
                    Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS)
            {
                Fail("WVB2131", $"Function '{Function.Name}' exceeds the local-slot limit.");
            }

            foreach (var Parameterˉtype in Function.Parameterˉtypes)
            {
                Verifyˉvalueˉtype(Parameterˉtype, allowˉvoid: false, "function parameter");
            }

            foreach (var Localˉtype in Function.Localˉtypes)
            {
                Verifyˉvalueˉtype(Localˉtype, allowˉvoid: false, "function local");
            }

            Verifyˉvalueˉtype(Function.Returnˉtype, allowˉvoid: true, "function return");

            if (Function.Codeˉoffset != Expectedˉcodeˉoffset)
            {
                Fail(
                    "WVB2132",
                    $"Function '{Function.Name}' code does not begin at the canonical contiguous offset {Expectedˉcodeˉoffset}.");
            }

            if (Function.Codeˉlength <= 0 ||
                Function.Codeˉlength > Bytecodeˉlimits.MAX_CODE_BYTES_PER_FUNCTION)
            {
                Fail("WVB2133", $"Function '{Function.Name}' has an invalid code length.");
            }

            if (Function.Codeˉoffset < 0 ||
                Function.Codeˉlength > module.Code.Length - Function.Codeˉoffset)
            {
                Fail("WVB2134", $"Function '{Function.Name}' code range is outside the Code section.");
            }

            if (Function.Maximumˉstackˉdepth < 0 ||
                Function.Maximumˉstackˉdepth > Bytecodeˉlimits.MAX_OPERAND_STACK)
            {
                Fail("WVB2135", $"Function '{Function.Name}' has an invalid maximum stack depth.");
            }

            Expectedˉcodeˉoffset = checked(Expectedˉcodeˉoffset + Function.Codeˉlength);
        }

        if (Expectedˉcodeˉoffset != module.Code.Length)
        {
            Fail("WVB2136", "Function code ranges do not cover the complete Code section.");
        }
    }

    private static void Verifyˉexports(Bytecodeˉmodule module)
    {
        Verifyˉstrictˉordering(module.Exports.Select(Export => Export.Name), "export");
        foreach (var Export in module.Exports)
        {
            if (!Seedˉnames.Isˉidentifier(Export.Name))
            {
                Fail("WVB2140", $"Export name '{Export.Name}' is not a Seed identifier.");
            }

            if (Export.Kind != Exportˉkind.Function)
            {
                Fail("WVB2141", $"Export '{Export.Name}' has an unsupported kind.");
            }

            if ((uint)Export.Targetˉindex >= (uint)module.Functions.Length)
            {
                Fail("WVB2142", $"Export '{Export.Name}' references an invalid function index.");
            }

            if (!StringComparer.Ordinal.Equals(Export.Name, module.Functions[Export.Targetˉindex].Name))
            {
                Fail("WVB2143", $"Export '{Export.Name}' does not match its function name.");
            }
        }
    }

    private static void Verifyˉfunction(
        Bytecodeˉmodule module,
        Functionˉdeclaration function,
        ImmutableArray<Decodedˉinstruction> instructions)
    {
        var Instructionsˉbyˉoffset = instructions.ToDictionary(
            Instruction => Instruction.Offset,
            Instruction => Instruction);
        var Entryˉstacks = new Dictionary<int, ImmutableArray<Valueˉtype>>();
        var Pending = new Queue<int>();
        var Maximumˉstack = 0;
        Mergeˉentry(0, [], Entryˉstacks, Pending, function.Name);

        while (Pending.TryDequeue(out var Offset))
        {
            var Instruction = Instructionsˉbyˉoffset[Offset];
            var Stack = Entryˉstacks[Offset].ToList();
            Simulateˉinstruction(module, function, Instruction, Stack);
            Maximumˉstack = Math.Max(Maximumˉstack, Stack.Count);
            if (Maximumˉstack > Bytecodeˉlimits.MAX_OPERAND_STACK)
            {
                Fail("WVB2200", $"Function '{function.Name}' exceeds the operand-stack limit.", Offset);
            }

            var Resultˉstack = Stack.ToImmutableArray();
            var Nextˉoffset = checked(Instruction.Offset + Instruction.Size);
            switch (Instruction.Opcode)
            {
                case Opcode.Jump:
                    Mergeˉbranchˉtarget(
                        Instruction.Unsignedˉoperand,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    break;
                case Opcode.Branchˉfalse:
                    Mergeˉbranchˉtarget(
                        Instruction.Unsignedˉoperand,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    Mergeˉfallthrough(
                        Nextˉoffset,
                        function.Codeˉlength,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    break;
                case Opcode.Return:
                    break;
                default:
                    Mergeˉfallthrough(
                        Nextˉoffset,
                        function.Codeˉlength,
                        Resultˉstack,
                        Instructionsˉbyˉoffset,
                        Entryˉstacks,
                        Pending,
                        function.Name,
                        Instruction.Offset);
                    break;
            }
        }

        if (Entryˉstacks.Count != instructions.Length)
        {
            var Firstˉunreachable = instructions.First(Instruction => !Entryˉstacks.ContainsKey(Instruction.Offset));
            Fail(
                "WVB2201",
                $"Function '{function.Name}' contains unreachable instructions.",
                Firstˉunreachable.Offset);
        }

        if (Maximumˉstack != function.Maximumˉstackˉdepth)
        {
            Fail(
                "WVB2202",
                $"Function '{function.Name}' declares maximum stack {function.Maximumˉstackˉdepth}, but verification computed {Maximumˉstack}.");
        }
    }

    private static void Simulateˉinstruction(
        Bytecodeˉmodule module,
        Functionˉdeclaration function,
        Decodedˉinstruction instruction,
        List<Valueˉtype> stack)
    {
        switch (instruction.Opcode)
        {
            case Opcode.I32ˉconst:
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.Boolˉconst:
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U8ˉconst:
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.U32ˉconst:
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.Textˉconst:
                Requireˉdataˉtype(module, instruction, Dataˉtype.Text, function.Name);
                Push(stack, Valueˉtype.Text);
                break;
            case Opcode.Bytesˉconst:
                Requireˉdataˉtype(module, instruction, Dataˉtype.Bytes, function.Name);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Localˉload:
                Push(stack, Getˉlocalˉtype(function, instruction));
                break;
            case Opcode.Localˉstore:
                Pop(stack, Getˉlocalˉtype(function, instruction), function.Name, instruction.Offset);
                break;
            case Opcode.Dataˉlength:
                Requireˉdataˉtype(module, instruction, Dataˉtype.I32ˉarray, function.Name);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.Dataˉloadˉi32:
                Requireˉdataˉtype(module, instruction, Dataˉtype.I32ˉarray, function.Name);
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.Bytesˉlength:
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.Bytesˉslice:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bytes);
                break;
            case Opcode.Bytesˉreadˉu8:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U8);
                break;
            case Opcode.Bytesˉreadˉu16ˉlittle:
            case Opcode.Bytesˉreadˉu32ˉlittle:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bytes, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.I32ˉadd:
            case Opcode.I32ˉsubtract:
            case Opcode.I32ˉmultiply:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.I32ˉnegate:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.I32);
                break;
            case Opcode.U32ˉadd:
            case Opcode.U32ˉsubtract:
            case Opcode.U32ˉmultiply:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.U32);
                break;
            case Opcode.I32ˉequal:
            case Opcode.I32ˉnotˉequal:
            case Opcode.I32ˉless:
            case Opcode.I32ˉlessˉequal:
            case Opcode.I32ˉgreater:
            case Opcode.I32ˉgreaterˉequal:
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.I32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Boolˉequal:
            case Opcode.Boolˉnotˉequal:
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Boolˉnot:
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U32ˉequal:
            case Opcode.U32ˉnotˉequal:
            case Opcode.U32ˉless:
            case Opcode.U32ˉlessˉequal:
            case Opcode.U32ˉgreater:
            case Opcode.U32ˉgreaterˉequal:
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U32, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.U8ˉequal:
            case Opcode.U8ˉnotˉequal:
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Pop(stack, Valueˉtype.U8, function.Name, instruction.Offset);
                Push(stack, Valueˉtype.Bool);
                break;
            case Opcode.Jump:
                break;
            case Opcode.Branchˉfalse:
                Pop(stack, Valueˉtype.Bool, function.Name, instruction.Offset);
                break;
            case Opcode.Call:
                var Calledˉfunction = Getˉfunction(module, instruction, function.Name);
                Popˉparameters(stack, Calledˉfunction.Parameterˉtypes, function.Name, instruction.Offset);
                if (Calledˉfunction.Returnˉtype != Valueˉtype.Void)
                {
                    Push(stack, Calledˉfunction.Returnˉtype);
                }

                break;
            case Opcode.Callˉcapability:
                var Capability = Getˉcapability(module, instruction, function.Name);
                Popˉparameters(stack, Capability.Parameterˉtypes, function.Name, instruction.Offset);
                if (Capability.Returnˉtype != Valueˉtype.Void)
                {
                    Push(stack, Capability.Returnˉtype);
                }

                break;
            case Opcode.Pop:
                Popˉany(stack, function.Name, instruction.Offset);
                break;
            case Opcode.Return:
                if (function.Returnˉtype != Valueˉtype.Void)
                {
                    Pop(stack, function.Returnˉtype, function.Name, instruction.Offset);
                }

                if (stack.Count != 0)
                {
                    Fail(
                        "WVB2203",
                        $"Return in function '{function.Name}' leaves values on the operand stack.",
                        instruction.Offset);
                }

                break;
            default:
                Fail("WVB2204", $"Opcode '{instruction.Opcode}' has no verifier implementation.");
                break;
        }
    }

    private static Valueˉtype Getˉlocalˉtype(
        Functionˉdeclaration function,
        Decodedˉinstruction instruction)
    {
        if (instruction.Unsignedˉoperand >= (uint)function.Allˉlocalˉtypes.Length)
        {
            Fail(
                "WVB2210",
                $"Function '{function.Name}' references invalid local {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        return function.Allˉlocalˉtypes[(int)instruction.Unsignedˉoperand];
    }

    private static void Requireˉdataˉtype(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        Dataˉtype requiredˉtype,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Data.Length)
        {
            Fail(
                "WVB2211",
                $"Function '{functionˉname}' references invalid data {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        var Data = module.Data[(int)instruction.Unsignedˉoperand];
        if (Data.Type != requiredˉtype)
        {
            Fail(
                "WVB2212",
                $"Function '{functionˉname}' uses data '{Data.Name}' as {requiredˉtype}, but it is {Data.Type}.",
                instruction.Offset);
        }
    }

    private static Functionˉdeclaration Getˉfunction(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Functions.Length)
        {
            Fail(
                "WVB2213",
                $"Function '{functionˉname}' calls invalid function {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        return module.Functions[(int)instruction.Unsignedˉoperand];
    }

    private static Capabilityˉdeclaration Getˉcapability(
        Bytecodeˉmodule module,
        Decodedˉinstruction instruction,
        string functionˉname)
    {
        if (instruction.Unsignedˉoperand >= (uint)module.Capabilities.Length)
        {
            Fail(
                "WVB2214",
                $"Function '{functionˉname}' calls invalid capability {instruction.Unsignedˉoperand}.",
                instruction.Offset);
        }

        return module.Capabilities[(int)instruction.Unsignedˉoperand];
    }

    private static void Popˉparameters(
        List<Valueˉtype> stack,
        ImmutableArray<Valueˉtype> parameters,
        string functionˉname,
        int offset)
    {
        for (var Index = parameters.Length - 1; Index >= 0; Index--)
        {
            Pop(stack, parameters[Index], functionˉname, offset);
        }
    }

    private static void Push(List<Valueˉtype> stack, Valueˉtype type)
    {
        stack.Add(type);
    }

    private static void Pop(
        List<Valueˉtype> stack,
        Valueˉtype expectedˉtype,
        string functionˉname,
        int offset)
    {
        var Actualˉtype = Popˉany(stack, functionˉname, offset);
        if (Actualˉtype != expectedˉtype)
        {
            Fail(
                "WVB2220",
                $"Function '{functionˉname}' expected {expectedˉtype} on the stack but found {Actualˉtype}.",
                offset);
        }
    }

    private static Valueˉtype Popˉany(List<Valueˉtype> stack, string functionˉname, int offset)
    {
        if (stack.Count == 0)
        {
            Fail("WVB2221", $"Function '{functionˉname}' underflows the operand stack.", offset);
        }

        var Lastˉindex = stack.Count - 1;
        var Result = stack[Lastˉindex];
        stack.RemoveAt(Lastˉindex);
        return Result;
    }

    private static void Mergeˉfallthrough(
        int nextˉoffset,
        int codeˉlength,
        ImmutableArray<Valueˉtype> stack,
        Dictionary<int, Decodedˉinstruction> instructions,
        Dictionary<int, ImmutableArray<Valueˉtype>> entryˉstacks,
        Queue<int> pending,
        string functionˉname,
        int sourceˉoffset)
    {
        if (nextˉoffset >= codeˉlength || !instructions.ContainsKey(nextˉoffset))
        {
            Fail(
                "WVB2230",
                $"Control falls past the end of function '{functionˉname}'.",
                sourceˉoffset);
        }

        Mergeˉentry(nextˉoffset, stack, entryˉstacks, pending, functionˉname);
    }

    private static void Mergeˉbranchˉtarget(
        uint rawˉtarget,
        ImmutableArray<Valueˉtype> stack,
        Dictionary<int, Decodedˉinstruction> instructions,
        Dictionary<int, ImmutableArray<Valueˉtype>> entryˉstacks,
        Queue<int> pending,
        string functionˉname,
        int sourceˉoffset)
    {
        if (rawˉtarget > int.MaxValue || !instructions.ContainsKey((int)rawˉtarget))
        {
            Fail(
                "WVB2231",
                $"Function '{functionˉname}' branches to invalid instruction offset {rawˉtarget}.",
                sourceˉoffset);
        }

        Mergeˉentry((int)rawˉtarget, stack, entryˉstacks, pending, functionˉname);
    }

    private static void Mergeˉentry(
        int offset,
        ImmutableArray<Valueˉtype> stack,
        Dictionary<int, ImmutableArray<Valueˉtype>> entryˉstacks,
        Queue<int> pending,
        string functionˉname)
    {
        if (entryˉstacks.TryGetValue(offset, out var Existing))
        {
            if (!Existing.SequenceEqual(stack))
            {
                Fail(
                    "WVB2232",
                    $"Function '{functionˉname}' has inconsistent operand-stack types at offset {offset}.",
                    offset);
            }

            return;
        }

        entryˉstacks.Add(offset, stack);
        pending.Enqueue(offset);
    }

    private static void Verifyˉstrictˉordering(IEnumerable<string> names, string kind)
    {
        string? Previous = null;
        foreach (var Name in names)
        {
            if (Previous is not null && StringComparer.Ordinal.Compare(Previous, Name) >= 0)
            {
                Fail(
                    "WVB2240",
                    $"{kind} names must be unique and strictly sorted; '{Name}' follows '{Previous}'.");
            }

            Previous = Name;
        }
    }

    private static void Verifyˉvalueˉtype(Valueˉtype type, bool allowˉvoid, string position)
    {
        if (!Enum.IsDefined(type) || (!allowˉvoid && type == Valueˉtype.Void))
        {
            Fail("WVB2241", $"Value type '{type}' is invalid for a {position}.");
        }
    }

    private static void Fail(string code, string message, int? byteˉoffset = null)
    {
        throw new Moduleˉverificationˉexception(code, message, byteˉoffset);
    }
}
