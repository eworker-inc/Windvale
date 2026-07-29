using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Runtime;

public sealed class Referenceˉruntime
{
    private readonly Verifiedˉmodule Verifiedˉmodule;
    private readonly ICapabilityˉhost Capabilityˉhost;
    private readonly Runtimeˉoptions Options;
    private readonly ImmutableArray<Dictionary<int, int>> Instructionˉindices;
    private long Executedˉinstructions;

    public Referenceˉruntime(
        Verifiedˉmodule verifiedˉmodule,
        ICapabilityˉhost capabilityˉhost,
        Runtimeˉoptions options)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        ArgumentNullException.ThrowIfNull(capabilityˉhost);
        ArgumentNullException.ThrowIfNull(options);
        if (options.Maximumˉinstructions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The maximum instruction count must be positive.");
        }

        if (options.Maximumˉcallˉdepth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The maximum call depth must be positive.");
        }

        Verifiedˉmodule = verifiedˉmodule;
        Capabilityˉhost = capabilityˉhost;
        Options = options;
        Instructionˉindices = verifiedˉmodule.Functions
            .Select(Function => Function.Instructions
                .Select((Instruction, Index) => (Instruction.Offset, Index))
                .ToDictionary(Item => Item.Offset, Item => Item.Index))
            .ToImmutableArray();
    }

    public Runtimeˉresult Runˉmain()
    {
        Requireˉauthorizedˉcapabilities();
        var Mainˉexport = Verifiedˉmodule.Module.Exports
            .FirstOrDefault(Export => StringComparer.Ordinal.Equals(Export.Name, "main"));
        if (Mainˉexport is null)
        {
            throw new Runtimeˉexception("WVR3002", "The module does not export 'main'.");
        }

        var Mainˉfunction = Verifiedˉmodule.Module.Functions[Mainˉexport.Targetˉindex];
        if (Mainˉfunction.Parameterˉtypes.Length != 0 || Mainˉfunction.Returnˉtype != Valueˉtype.I32)
        {
            throw new Runtimeˉexception(
                "WVR3003",
                "The exported main function must have signature fn() -> i32.");
        }

        Executedˉinstructions = 0;
        var Result = Executeˉfunction(Mainˉexport.Targetˉindex, [], 1);
        return new(Result!.Value.I32ˉvalue, Executedˉinstructions);
    }

    private Runtimeˉvalue? Executeˉfunction(
        int functionˉindex,
        ImmutableArray<Runtimeˉvalue> arguments,
        int callˉdepth)
    {
        if (callˉdepth > Options.Maximumˉcallˉdepth)
        {
            throw new Runtimeˉexception("WVR3004", "The maximum call depth was exceeded.");
        }

        var Verifiedˉfunction = Verifiedˉmodule.Functions[functionˉindex];
        var Function = Verifiedˉfunction.Declaration;
        var Localˉtypes = Function.Allˉlocalˉtypes;
        var Locals = new Runtimeˉvalue[Localˉtypes.Length];
        for (var Index = 0; Index < Locals.Length; Index++)
        {
            Locals[Index] = Runtimeˉvalue.Default(Localˉtypes[Index]);
        }

        for (var Index = 0; Index < arguments.Length; Index++)
        {
            Locals[Index] = arguments[Index];
        }

        var Stack = new List<Runtimeˉvalue>(Function.Maximumˉstackˉdepth);
        var Instructionˉindex = 0;
        while (true)
        {
            Countˉinstruction();
            var Instruction = Verifiedˉfunction.Instructions[Instructionˉindex];
            var Advance = true;

            try
            {
                switch (Instruction.Opcode)
                {
                    case Opcode.I32ˉconst:
                        Stack.Add(Runtimeˉvalue.Fromˉi32(Instruction.Signedˉoperand));
                        break;
                    case Opcode.Boolˉconst:
                        Stack.Add(Runtimeˉvalue.Fromˉbool(Instruction.Unsignedˉoperand == 1));
                        break;
                    case Opcode.Textˉconst:
                        var Text = (Textˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Add(Runtimeˉvalue.Fromˉtext(Text.Value));
                        break;
                    case Opcode.Localˉload:
                        Stack.Add(Locals[(int)Instruction.Unsignedˉoperand]);
                        break;
                    case Opcode.Localˉstore:
                        Locals[(int)Instruction.Unsignedˉoperand] = Pop(Stack);
                        break;
                    case Opcode.Dataˉlength:
                        var Lengthˉdata = (I32ˉarrayˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Add(Runtimeˉvalue.Fromˉi32(Lengthˉdata.Values.Length));
                        break;
                    case Opcode.Dataˉloadˉi32:
                        var Array = (I32ˉarrayˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        var Elementˉindex = Pop(Stack).I32ˉvalue;
                        if ((uint)Elementˉindex >= (uint)Array.Values.Length)
                        {
                            throw new Runtimeˉexception(
                                "WVR3005",
                                $"Index {Elementˉindex} is outside data '{Array.Name}' with length {Array.Values.Length}.");
                        }

                        Stack.Add(Runtimeˉvalue.Fromˉi32(Array.Values[Elementˉindex]));
                        break;
                    case Opcode.I32ˉadd:
                        Applyˉi32ˉbinary(Stack, (Left, Right) => checked(Left + Right));
                        break;
                    case Opcode.I32ˉsubtract:
                        Applyˉi32ˉbinary(Stack, (Left, Right) => checked(Left - Right));
                        break;
                    case Opcode.I32ˉmultiply:
                        Applyˉi32ˉbinary(Stack, (Left, Right) => checked(Left * Right));
                        break;
                    case Opcode.I32ˉnegate:
                        Stack.Add(Runtimeˉvalue.Fromˉi32(checked(-Pop(Stack).I32ˉvalue)));
                        break;
                    case Opcode.I32ˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.I32ˉnotˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.I32ˉless:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left < Right);
                        break;
                    case Opcode.I32ˉlessˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left <= Right);
                        break;
                    case Opcode.I32ˉgreater:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left > Right);
                        break;
                    case Opcode.I32ˉgreaterˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left >= Right);
                        break;
                    case Opcode.Boolˉequal:
                        Applyˉboolˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.Boolˉnotˉequal:
                        Applyˉboolˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.Boolˉnot:
                        Stack.Add(Runtimeˉvalue.Fromˉbool(!Pop(Stack).Boolˉvalue));
                        break;
                    case Opcode.Jump:
                        Instructionˉindex = Instructionˉindices[functionˉindex][(int)Instruction.Unsignedˉoperand];
                        Advance = false;
                        break;
                    case Opcode.Branchˉfalse:
                        if (!Pop(Stack).Boolˉvalue)
                        {
                            Instructionˉindex = Instructionˉindices[functionˉindex][(int)Instruction.Unsignedˉoperand];
                            Advance = false;
                        }

                        break;
                    case Opcode.Call:
                        var Calledˉfunction = Verifiedˉmodule.Module.Functions[(int)Instruction.Unsignedˉoperand];
                        var Callˉarguments = Popˉarguments(Stack, Calledˉfunction.Parameterˉtypes.Length);
                        var Callˉresult = Executeˉfunction(
                            (int)Instruction.Unsignedˉoperand,
                            Callˉarguments,
                            callˉdepth + 1);
                        if (Callˉresult is not null)
                        {
                            Stack.Add(Callˉresult.Value);
                        }

                        break;
                    case Opcode.Callˉcapability:
                        var Capability = Verifiedˉmodule.Module.Capabilities[(int)Instruction.Unsignedˉoperand];
                        var Capabilityˉarguments = Popˉarguments(Stack, Capability.Parameterˉtypes.Length);
                        var Capabilityˉresult = Capabilityˉhost.Invoke(Capability, Capabilityˉarguments);
                        if (Capabilityˉresult is not null)
                        {
                            Stack.Add(Capabilityˉresult.Value);
                        }

                        break;
                    case Opcode.Pop:
                        Pop(Stack);
                        break;
                    case Opcode.Return:
                        return Function.Returnˉtype == Valueˉtype.Void ? null : Pop(Stack);
                    default:
                        throw new Runtimeˉexception(
                            "WVR3006",
                            $"The verified opcode '{Instruction.Opcode}' is not implemented by the runtime.");
                }
            }
            catch (OverflowException)
            {
                throw new Runtimeˉexception(
                    "WVR3007",
                    $"Integer overflow in function '{Function.Name}' at bytecode offset {Instruction.Offset}.");
            }

            if (Advance)
            {
                Instructionˉindex++;
            }
        }
    }

    private void Requireˉauthorizedˉcapabilities()
    {
        foreach (var Capability in Verifiedˉmodule.Module.Capabilities)
        {
            if (!Options.Authorizedˉcapabilities.Contains(Capability.Name))
            {
                throw new Runtimeˉexception(
                    "WVR3010",
                    $"Capability '{Capability.Name}' was declared but not authorized.");
            }
        }
    }

    private void Countˉinstruction()
    {
        if (Executedˉinstructions >= Options.Maximumˉinstructions)
        {
            throw new Runtimeˉexception(
                "WVR3011",
                $"The instruction limit {Options.Maximumˉinstructions} was exceeded.");
        }

        Executedˉinstructions++;
    }

    private static Runtimeˉvalue Pop(List<Runtimeˉvalue> stack)
    {
        var Index = stack.Count - 1;
        var Value = stack[Index];
        stack.RemoveAt(Index);
        return Value;
    }

    private static ImmutableArray<Runtimeˉvalue> Popˉarguments(
        List<Runtimeˉvalue> stack,
        int count)
    {
        var Arguments = new Runtimeˉvalue[count];
        for (var Index = count - 1; Index >= 0; Index--)
        {
            Arguments[Index] = Pop(stack);
        }

        return [.. Arguments];
    }

    private static void Applyˉi32ˉbinary(
        List<Runtimeˉvalue> stack,
        Func<int, int, int> operation)
    {
        var Right = Pop(stack).I32ˉvalue;
        var Left = Pop(stack).I32ˉvalue;
        stack.Add(Runtimeˉvalue.Fromˉi32(operation(Left, Right)));
    }

    private static void Applyˉi32ˉcomparison(
        List<Runtimeˉvalue> stack,
        Func<int, int, bool> operation)
    {
        var Right = Pop(stack).I32ˉvalue;
        var Left = Pop(stack).I32ˉvalue;
        stack.Add(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉboolˉcomparison(
        List<Runtimeˉvalue> stack,
        Func<bool, bool, bool> operation)
    {
        var Right = Pop(stack).Boolˉvalue;
        var Left = Pop(stack).Boolˉvalue;
        stack.Add(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }
}
