using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Windvale.Bytecode;

namespace Windvale.Runtime;

public sealed class Referenceˉruntime
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

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
            .FirstOrDefault(Export => StringComparer.Ordinal.Equals(Export.Name, "Main"));
        if (Mainˉexport is null)
        {
            throw new Runtimeˉexception("WVR3002", "The module does not export 'Main'.");
        }

        var Mainˉfunction = Verifiedˉmodule.Module.Functions[Mainˉexport.Targetˉindex];
        if (Mainˉfunction.Parameterˉtypes.Length != 0 || Mainˉfunction.Returnˉtype != Valueˉtype.I32)
        {
            throw new Runtimeˉexception(
                "WVR3003",
                "The exported Main function must have signature fn() -> i32.");
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
            Locals[Index] = Runtimeˉvalue.Default(Localˉtypes[Index], Verifiedˉmodule.Module.Types);
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
                    case Opcode.U8ˉconst:
                        Stack.Add(Runtimeˉvalue.Fromˉu8(checked((byte)Instruction.Unsignedˉoperand)));
                        break;
                    case Opcode.U32ˉconst:
                        Stack.Add(Runtimeˉvalue.Fromˉu32(Instruction.Unsignedˉoperand));
                        break;
                    case Opcode.Textˉconst:
                        var Text = (Textˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Add(Runtimeˉvalue.Fromˉtext(Text.Value));
                        break;
                    case Opcode.Bytesˉconst:
                        var Bytes = (Bytesˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(Bytes.Values));
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
                    case Opcode.Bytesˉlength:
                        Stack.Add(Runtimeˉvalue.Fromˉu32(checked((uint)Pop(Stack).Bytesˉvalue.Length)));
                        break;
                    case Opcode.Bytesˉslice:
                        var Sliceˉlength = Pop(Stack).U32ˉvalue;
                        var Sliceˉoffset = Pop(Stack).U32ˉvalue;
                        var Sliceˉsource = Pop(Stack).Bytesˉvalue;
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(Sliceˉbytes(Sliceˉsource, Sliceˉoffset, Sliceˉlength)));
                        break;
                    case Opcode.Bytesˉreadˉu8:
                        var U8ˉoffset = Pop(Stack).U32ˉvalue;
                        var U8ˉsource = Pop(Stack).Bytesˉvalue;
                        Stack.Add(Runtimeˉvalue.Fromˉu8(
                            U8ˉsource.Storage.Read(Requireˉbyteˉrange(
                                U8ˉsource,
                                U8ˉoffset,
                                sizeof(byte)))));
                        break;
                    case Opcode.Bytesˉreadˉu16ˉlittle:
                        var U16ˉoffset = Pop(Stack).U32ˉvalue;
                        var U16ˉsource = Pop(Stack).Bytesˉvalue;
                        var U16ˉabsolute = Requireˉbyteˉrange(U16ˉsource, U16ˉoffset, sizeof(ushort));
                        Stack.Add(Runtimeˉvalue.Fromˉu32(Readˉu16(U16ˉsource.Storage, U16ˉabsolute)));
                        break;
                    case Opcode.Bytesˉreadˉu32ˉlittle:
                        var U32ˉoffset = Pop(Stack).U32ˉvalue;
                        var U32ˉsource = Pop(Stack).Bytesˉvalue;
                        var U32ˉabsolute = Requireˉbyteˉrange(U32ˉsource, U32ˉoffset, sizeof(uint));
                        Stack.Add(Runtimeˉvalue.Fromˉu32(Readˉu32(U32ˉsource.Storage, U32ˉabsolute)));
                        break;
                    case Opcode.Bytesˉreadˉi32ˉlittle:
                        var Readˉi32ˉoffset = Pop(Stack).U32ˉvalue;
                        var Readˉi32ˉsource = Pop(Stack).Bytesˉvalue;
                        var Readˉi32ˉabsolute = Requireˉbyteˉrange(
                            Readˉi32ˉsource,
                            Readˉi32ˉoffset,
                            sizeof(int));
                        Stack.Add(Runtimeˉvalue.Fromˉi32(Readˉi32(
                            Readˉi32ˉsource.Storage,
                            Readˉi32ˉabsolute)));
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
                    case Opcode.U32ˉadd:
                        Applyˉu32ˉbinary(Stack, (Left, Right) => checked(Left + Right));
                        break;
                    case Opcode.U32ˉsubtract:
                        Applyˉu32ˉbinary(Stack, (Left, Right) => checked(Left - Right));
                        break;
                    case Opcode.U32ˉmultiply:
                        Applyˉu32ˉbinary(Stack, (Left, Right) => checked(Left * Right));
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
                    case Opcode.U32ˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.U32ˉnotˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.U32ˉless:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left < Right);
                        break;
                    case Opcode.U32ˉlessˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left <= Right);
                        break;
                    case Opcode.U32ˉgreater:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left > Right);
                        break;
                    case Opcode.U32ˉgreaterˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left >= Right);
                        break;
                    case Opcode.U8ˉequal:
                        Applyˉu8ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.U8ˉnotˉequal:
                        Applyˉu8ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.Enumˉconst:
                        var Enumˉtype = (Enumˉtypeˉdeclaration)Verifiedˉmodule.Module.Types[
                            (int)Instruction.Unsignedˉoperand];
                        Stack.Add(Runtimeˉvalue.Fromˉenum(
                            (int)Instruction.Unsignedˉoperand,
                            Enumˉtype.Members[(int)Instruction.Secondˉunsignedˉoperand].Value));
                        break;
                    case Opcode.Enumˉequal:
                        Applyˉenumˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.Enumˉnotˉequal:
                        Applyˉenumˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.Enumˉname:
                        var Enumˉvalue = Pop(Stack);
                        var Namedˉenum = (Enumˉtypeˉdeclaration)Verifiedˉmodule.Module.Types[
                            Enumˉvalue.Type.Nominalˉtypeˉindex];
                        Stack.Add(Runtimeˉvalue.Fromˉtext(Namedˉenum.Members.Single(
                            Member => Member.Value == Enumˉvalue.Enumˉvalue).Name));
                        break;
                    case Opcode.I32ˉformat:
                        Stack.Add(Runtimeˉvalue.Fromˉtext(
                            Pop(Stack).I32ˉvalue.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case Opcode.U8ˉformat:
                        Stack.Add(Runtimeˉvalue.Fromˉtext(
                            Pop(Stack).U8ˉvalue.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case Opcode.U32ˉformat:
                        Stack.Add(Runtimeˉvalue.Fromˉtext(
                            Pop(Stack).U32ˉvalue.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case Opcode.U32ˉfromˉu8:
                        Stack.Add(Runtimeˉvalue.Fromˉu32(Pop(Stack).U8ˉvalue));
                        break;
                    case Opcode.Textˉconcat:
                        var Rightˉtext = Pop(Stack).Textˉvalue!;
                        var Leftˉtext = Pop(Stack).Textˉvalue!;
                        var Utf8ˉlength = checked(
                            Encoding.UTF8.GetByteCount(Leftˉtext) + Encoding.UTF8.GetByteCount(Rightˉtext));
                        if (Utf8ˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
                        {
                            throw new Runtimeˉexception(
                                "WVR3012",
                                $"Text concatenation result {Utf8ˉlength} exceeds the UTF-8 value limit.");
                        }

                        Stack.Add(Runtimeˉvalue.Fromˉtext(string.Concat(Leftˉtext, Rightˉtext)));
                        break;
                    case Opcode.Textˉutf8ˉisˉvalid:
                        var Utf8ˉcandidate = Pop(Stack).Bytesˉvalue;
                        Stack.Add(Runtimeˉvalue.Fromˉbool(Isˉvalidˉutf8(Utf8ˉcandidate)));
                        break;
                    case Opcode.Textˉfromˉutf8:
                        var Utf8ˉsource = Pop(Stack).Bytesˉvalue;
                        if (Utf8ˉsource.Length > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
                        {
                            throw new Runtimeˉexception(
                                "WVR3012",
                                $"Decoded text result exceeds the UTF-8 value limit {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}.");
                        }

                        try
                        {
                            Stack.Add(Runtimeˉvalue.Fromˉtext(STRICT_UTF8.GetString(Utf8ˉsource.Toˉarray())));
                        }
                        catch (DecoderFallbackException)
                        {
                            throw new Runtimeˉexception(
                                "WVR3014",
                                "Textˉfromˉutf8 received an invalid UTF-8 byte sequence.");
                        }

                        break;
                    case Opcode.Textˉquote:
                        Stack.Add(Runtimeˉvalue.Fromˉtext(Quoteˉtext(Pop(Stack).Textˉvalue!)));
                        break;
                    case Opcode.Bytesˉconcat:
                        var Rightˉbytes = Pop(Stack).Bytesˉvalue;
                        var Leftˉbytes = Pop(Stack).Bytesˉvalue;
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(Concatˉbytes(Leftˉbytes, Rightˉbytes)));
                        break;
                    case Opcode.Bytesˉfromˉu8:
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(ImmutableArray.Create(Pop(Stack).U8ˉvalue)));
                        break;
                    case Opcode.Bytesˉfromˉu16ˉlittle:
                        var U16ˉvalue = Pop(Stack).U32ˉvalue;
                        if (U16ˉvalue > ushort.MaxValue)
                        {
                            throw new Runtimeˉexception(
                                "WVR3016",
                                $"Bytesˉfromˉu16ˉlittle received {U16ˉvalue}; the maximum is {ushort.MaxValue}.");
                        }

                        var U16ˉbytes = new byte[sizeof(ushort)];
                        BinaryPrimitives.WriteUInt16LittleEndian(U16ˉbytes, (ushort)U16ˉvalue);
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(ImmutableArray.Create(U16ˉbytes)));
                        break;
                    case Opcode.Bytesˉfromˉu32ˉlittle:
                        var U32ˉbytes = new byte[sizeof(uint)];
                        BinaryPrimitives.WriteUInt32LittleEndian(U32ˉbytes, Pop(Stack).U32ˉvalue);
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(ImmutableArray.Create(U32ˉbytes)));
                        break;
                    case Opcode.Bytesˉfromˉi32ˉlittle:
                        var I32ˉbytes = new byte[sizeof(int)];
                        BinaryPrimitives.WriteInt32LittleEndian(I32ˉbytes, Pop(Stack).I32ˉvalue);
                        Stack.Add(Runtimeˉvalue.Fromˉbytes(ImmutableArray.Create(I32ˉbytes)));
                        break;
                    case Opcode.Bytesˉsha256ˉhex:
                        Stack.Add(Runtimeˉvalue.Fromˉtext(
                            Convert.ToHexStringLower(SHA256.HashData(Pop(Stack).Bytesˉvalue.Toˉarray()))));
                        break;
                    case Opcode.Textˉtoˉutf8:
                        try
                        {
                            Stack.Add(Runtimeˉvalue.Fromˉbytes(
                                ImmutableArray.Create(STRICT_UTF8.GetBytes(Pop(Stack).Textˉvalue!))));
                        }
                        catch (EncoderFallbackException)
                        {
                            throw new Runtimeˉexception(
                                "WVR3014",
                                "Textˉtoˉutf8 received an invalid Unicode value.");
                        }

                        break;
                    case Opcode.Recordˉcreate:
                        var Recordˉtype = (Recordˉtypeˉdeclaration)Verifiedˉmodule.Module.Types[
                            (int)Instruction.Unsignedˉoperand];
                        var Recordˉfields = Popˉarguments(Stack, Recordˉtype.Fields.Length);
                        Stack.Add(Runtimeˉvalue.Fromˉrecord(
                            (int)Instruction.Unsignedˉoperand,
                            Recordˉfields));
                        break;
                    case Opcode.Recordˉfield:
                        var Record = Pop(Stack).Recordˉvalue!;
                        Stack.Add(Record.Fields[(int)Instruction.Unsignedˉoperand]);
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
                        Validateˉcapabilityˉresult(
                            Capability,
                            Capabilityˉresult,
                            Function.Name,
                            Instruction.Offset);
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

            if (!Capabilityˉhost.Supports(Capability.Name))
            {
                throw new Runtimeˉexception(
                    "WVR3001",
                    $"The host does not implement capability '{Capability.Name}'.");
            }
        }
    }

    private static void Validateˉcapabilityˉresult(
        Capabilityˉdeclaration capability,
        Runtimeˉvalue? result,
        string functionˉname,
        int offset)
    {
        if (capability.Returnˉtype == Valueˉtype.Void)
        {
            if (result is not null)
            {
                Failˉcapabilityˉresult(capability, functionˉname, offset, "returned a value for void");
            }

            return;
        }

        if (result is null)
        {
            Failˉcapabilityˉresult(capability, functionˉname, offset, "returned no value");
        }

        var Value = result!.Value;
        if (Value.Type != capability.Returnˉtype)
        {
            Failˉcapabilityˉresult(
                capability,
                functionˉname,
                offset,
                $"returned {Value.Type} instead of {capability.Returnˉtype}");
        }

        if (Value.Type.Kind == Valueˉtype.Text)
        {
            if (Value.Textˉvalue is null)
            {
                Failˉcapabilityˉresult(capability, functionˉname, offset, "returned null text");
            }

            int Utf8ˉlength;
            try
            {
                Utf8ˉlength = STRICT_UTF8.GetByteCount(Value.Textˉvalue!);
            }
            catch (EncoderFallbackException)
            {
                Failˉcapabilityˉresult(capability, functionˉname, offset, "returned invalid Unicode text");
                return;
            }

            if (Utf8ˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
            {
                Failˉcapabilityˉresult(
                    capability,
                    functionˉname,
                    offset,
                    $"returned {Utf8ˉlength} UTF-8 bytes; the text limit is {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}");
            }
        }

        if (Value.Type.Kind == Valueˉtype.Bytes &&
            Value.Bytesˉvalue.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            Failˉcapabilityˉresult(
                capability,
                functionˉname,
                offset,
                $"returned {Value.Bytesˉvalue.Length} bytes; the byte-value limit is {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}");
        }
    }

    private static void Failˉcapabilityˉresult(
        Capabilityˉdeclaration capability,
        string functionˉname,
        int offset,
        string reason)
    {
        throw new Runtimeˉexception(
            "WVR3013",
            $"Capability '{capability.Name}' {reason} in function '{functionˉname}' at bytecode offset {offset}.");
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

    private static void Applyˉu32ˉbinary(
        List<Runtimeˉvalue> stack,
        Func<uint, uint, uint> operation)
    {
        var Right = Pop(stack).U32ˉvalue;
        var Left = Pop(stack).U32ˉvalue;
        stack.Add(Runtimeˉvalue.Fromˉu32(operation(Left, Right)));
    }

    private static void Applyˉu32ˉcomparison(
        List<Runtimeˉvalue> stack,
        Func<uint, uint, bool> operation)
    {
        var Right = Pop(stack).U32ˉvalue;
        var Left = Pop(stack).U32ˉvalue;
        stack.Add(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉu8ˉcomparison(
        List<Runtimeˉvalue> stack,
        Func<byte, byte, bool> operation)
    {
        var Right = Pop(stack).U8ˉvalue;
        var Left = Pop(stack).U8ˉvalue;
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

    private static void Applyˉenumˉcomparison(
        List<Runtimeˉvalue> stack,
        Func<int, int, bool> operation)
    {
        var Right = Pop(stack).Enumˉvalue;
        var Left = Pop(stack).Enumˉvalue;
        stack.Add(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static Runtimeˉbyteˉslice Sliceˉbytes(
        Runtimeˉbyteˉslice source,
        uint offset,
        uint length)
    {
        if (offset > (uint)source.Length || length > (uint)source.Length - offset)
        {
            throw new Runtimeˉexception(
                "WVR3008",
                $"Byte slice offset {offset} and length {length} exceed source length {source.Length}.");
        }

        return new(
            source.Storage,
            checked(source.Offset + (int)offset),
            checked((int)length));
    }

    private static bool Isˉvalidˉutf8(Runtimeˉbyteˉslice value)
    {
        try
        {
            _ = STRICT_UTF8.GetCharCount(value.Toˉarray());
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static Runtimeˉbyteˉslice Concatˉbytes(
        Runtimeˉbyteˉslice left,
        Runtimeˉbyteˉslice right)
    {
        var Length = checked(left.Length + right.Length);
        if (Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new Runtimeˉexception(
                "WVR3015",
                $"Concatenated byte result exceeds the byte-value limit {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}.");
        }

        var Leftˉnode = Runtimeˉbyteˉnode.Slice(left.Storage, left.Offset, left.Length);
        var Rightˉnode = Runtimeˉbyteˉnode.Slice(right.Storage, right.Offset, right.Length);
        return new(Runtimeˉbyteˉnode.Concat(Leftˉnode, Rightˉnode), 0, Length);
    }

    private static string Quoteˉtext(string value)
    {
        var Outputˉlength = 2;
        foreach (var Character in value)
        {
            Outputˉlength = checked(Outputˉlength + Character switch
            {
                '"' or '\\' or '\b' or '\f' or '\n' or '\r' or '\t' => 2,
                >= ' ' and <= '~' => 1,
                _ => 6,
            });
            if (Outputˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
            {
                throw new Runtimeˉexception(
                    "WVR3012",
                    $"Quoted text result exceeds the UTF-8 value limit {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}.");
            }
        }

        var Result = new StringBuilder(Outputˉlength);
        Result.Append('"');
        foreach (var Character in value)
        {
            switch (Character)
            {
                case '"':
                    Result.Append("\\\"");
                    break;
                case '\\':
                    Result.Append("\\\\");
                    break;
                case '\b':
                    Result.Append("\\b");
                    break;
                case '\f':
                    Result.Append("\\f");
                    break;
                case '\n':
                    Result.Append("\\n");
                    break;
                case '\r':
                    Result.Append("\\r");
                    break;
                case '\t':
                    Result.Append("\\t");
                    break;
                case >= ' ' and <= '~':
                    Result.Append(Character);
                    break;
                default:
                    Result.Append("\\u");
                    Result.Append(((ushort)Character).ToString("X4", CultureInfo.InvariantCulture));
                    break;
            }
        }

        Result.Append('"');
        return Result.ToString();
    }

    private static int Requireˉbyteˉrange(
        Runtimeˉbyteˉslice source,
        uint offset,
        int width)
    {
        if (offset > (uint)source.Length || width > source.Length - (int)offset)
        {
            throw new Runtimeˉexception(
                "WVR3008",
                $"A {width}-byte read at offset {offset} exceeds source length {source.Length}.");
        }

        return checked(source.Offset + (int)offset);
    }

    private static ushort Readˉu16(Runtimeˉbyteˉnode source, int offset)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(ushort)];
        source.Copyˉto(Buffer, offset, Buffer.Length);
        return BinaryPrimitives.ReadUInt16LittleEndian(Buffer);
    }

    private static uint Readˉu32(Runtimeˉbyteˉnode source, int offset)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(uint)];
        source.Copyˉto(Buffer, offset, Buffer.Length);
        return BinaryPrimitives.ReadUInt32LittleEndian(Buffer);
    }

    private static int Readˉi32(Runtimeˉbyteˉnode source, int offset)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(int)];
        source.Copyˉto(Buffer, offset, Buffer.Length);
        return BinaryPrimitives.ReadInt32LittleEndian(Buffer);
    }
}
