using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class X64ˉnativeˉbackend
{
    private const ulong INTEGER_OVERFLOW_STATUS = 0x0000_0001_0000_0000UL;
    private const ulong INSTRUCTION_LIMIT_STATUS = 0x0000_0002_0000_0000UL;
    private const ulong CALL_DEPTH_STATUS = 0x0000_0003_0000_0000UL;
    private const ulong DATA_BOUNDS_STATUS = 0x0000_0004_0000_0000UL;
    private const ulong RUNTIME_SERVICE_STATUS = 0x0000_0005_0000_0000UL;
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Nativeˉcompilation Compile(Verifiedˉmodule verifiedˉmodule)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        var Module = verifiedˉmodule.Module;
        var Isˉportable = Module.Profile == Moduleˉprofile.Portable && Module.Capabilities.IsEmpty;
        var Isˉhostedˉconsole = Module.Profile == Moduleˉprofile.Hosted &&
            Module.Capabilities is
            [
                {
                    Name: Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                    Parameterˉtypes.Length: 1,
                    Returnˉtype: Valueˉtype.Void,
                },
            ] &&
            Module.Capabilities[0].Parameterˉtypes[0] == Valueˉtype.Text;
        if ((!Isˉportable && !Isˉhostedˉconsole) ||
            !Module.Types.IsEmpty ||
            Module.Data.Any(Data => Data is not (I32ˉarrayˉdataˉdeclaration or Textˉdataˉdeclaration)))
        {
            Fail(
                "WVN2001",
                "The baseline native subset requires either a capability-free portable module or a hosted module declaring only console.write_line, without nominal types and with only immutable i32-array/text data.");
        }
        if (verifiedˉmodule.Functions.IsEmpty ||
            Module.Exports.Length != 1 ||
            Module.Exports[0] is not { Name: "Main", Kind: Exportˉkind.Function } ||
            (uint)Module.Exports[0].Targetˉindex >= (uint)verifiedˉmodule.Functions.Length)
        {
            Fail("WVN2002", "The baseline native subset requires exactly one exported Main function.");
        }

        var Mainˉexport = Module.Exports[0];
        var Main = verifiedˉmodule.Functions[Mainˉexport.Targetˉindex];
        if (!StringComparer.Ordinal.Equals(Main.Declaration.Name, "Main") ||
            !Main.Declaration.Parameterˉtypes.IsEmpty ||
            Main.Declaration.Returnˉtype != Valueˉtype.I32)
        {
            Fail(
                "WVN2002",
                "The baseline native entry must be Main() -> i32; " +
                $"found name='{Main.Declaration.Name}', parameters={Main.Declaration.Parameterˉtypes.Length}, " +
                $"return={Main.Declaration.Returnˉtype}.");
        }
        foreach (var Function in verifiedˉmodule.Functions)
        {
            if (Function.Declaration.Parameterˉtypes.Any(Type => !Isˉnativeˉscalarˉtype(Type)) ||
                Function.Declaration.Parameterˉtypes.Length > Nativeˉcontract.MAXIMUM_CALL_PARAMETERS ||
                Function.Declaration.Returnˉtype.Kind is not (Valueˉtype.I32 or Valueˉtype.Bool) ||
                !Isˉnativeˉscalarˉtype(Function.Declaration.Returnˉtype) ||
                Function.Declaration.Localˉtypes.Any(Type => !Isˉnativeˉlocalˉtype(Type)) ||
                Function.Declaration.Allˉlocalˉtypes.Length >= Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
                Function.Declaration.Maximumˉstackˉdepth is < 1 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
            {
                Fail(
                    "WVN2002",
                    $"Native function '{Function.Declaration.Name}' must use bounded i32/bool parameters, i32/bool/static-text locals, and a non-void i32/bool return.");
            }
        }

        var Nativeˉmodule = Lowerˉverifiedˉwvb(verifiedˉmodule);
        var Fragment = Selectˉx64(Nativeˉmodule, Mainˉexport.Targetˉindex);
        return new(Nativeˉmodule, Fragment);
    }

    private static Nativeˉmodule Lowerˉverifiedˉwvb(Verifiedˉmodule module)
    {
        var Functions = module.Functions
            .Select((Function, Functionˉindex) => Lowerˉverifiedˉfunction(module, Function, Functionˉindex))
            .ToImmutableArray();
        var Data = module.Module.Data
            .Select<Dataˉdeclaration, Nativeˉdata>(Declaration => Declaration switch
            {
                I32ˉarrayˉdataˉdeclaration I32 => new Nativeˉi32ˉdata(I32.Name, I32.Values),
                Textˉdataˉdeclaration Text => new Nativeˉutf8ˉdata(
                    Text.Name,
                    STRICT_UTF8.GetBytes(Text.Value).ToImmutableArray()),
                _ => throw new Nativeˉbackendˉexception(
                    "WVN2001",
                    $"Unsupported native data '{Declaration.Name}'."),
            })
            .ToImmutableArray();
        var Requiredˉservices = module.Module.Capabilities.IsEmpty
            ? ImmutableArray<Nativeˉservice>.Empty
            : [Nativeˉservice.Consoleˉwriteˉline];
        return new(Functions, Data, Requiredˉservices);
    }

    private static Nativeˉfunction Lowerˉverifiedˉfunction(
        Verifiedˉmodule module,
        Verifiedˉfunction function,
        int functionˉindex)
    {
        var Parameterˉtypes = function.Declaration.Parameterˉtypes
            .Select(Toˉnativeˉtype)
            .ToImmutableArray();
        var Localˉtypes = function.Declaration.Localˉtypes
            .Select(Toˉnativeˉtype)
            .ToImmutableArray();
        var Allˉlocalˉtypes = Parameterˉtypes.AddRange(Localˉtypes);
        var Staticˉtextˉlocalˉdata = Enumerable.Repeat(-1, Allˉlocalˉtypes.Length).ToArray();
        var Valueˉtypes = ImmutableArray.CreateBuilder<Nativeˉvalueˉtype>();
        var Instructions = function.Instructions;
        var Leaders = new HashSet<int> { Instructions[0].Offset };
        foreach (var Instruction in Instructions)
        {
            if (Instruction.Opcode is Opcode.Jump or Opcode.Branchˉfalse)
            {
                Leaders.Add(checked((int)Instruction.Unsignedˉoperand));
            }
            if (Instruction.Opcode == Opcode.Branchˉfalse)
            {
                Leaders.Add(checked(Instruction.Offset + Instruction.Size));
            }
        }

        var Orderedˉleaders = Leaders.Order().ToImmutableArray();
        if (Orderedˉleaders.Length is < 1 or > Nativeˉcontract.MAXIMUM_BLOCKS)
        {
            Fail("WVN2004", "The baseline native function exceeds its basic-block limit.");
        }
        var Instructionˉindices = Instructions
            .Select((Instruction, Index) => (Instruction.Offset, Index))
            .ToDictionary(Item => Item.Offset, Item => Item.Index);
        if (Orderedˉleaders.Any(Leader => !Instructionˉindices.ContainsKey(Leader)))
        {
            Fail("WVN2003", "Verified WVB exposed a non-instruction branch target during native lowering.");
        }
        var Blockˉids = Orderedˉleaders
            .Select((Offset, Id) => (Offset, Id))
            .ToDictionary(Item => Item.Offset, Item => Item.Id);

        var Blocks = ImmutableArray.CreateBuilder<Nativeˉblock>(Orderedˉleaders.Length);
        for (var Blockˉid = 0; Blockˉid < Orderedˉleaders.Length; Blockˉid++)
        {
            var Startˉindex = Instructionˉindices[Orderedˉleaders[Blockˉid]];
            var Endˉindex = Blockˉid + 1 < Orderedˉleaders.Length
                ? Instructionˉindices[Orderedˉleaders[Blockˉid + 1]]
                : Instructions.Length;
            var Operations = ImmutableArray.CreateBuilder<Nativeˉoperation>();
            var Stack = new Stack<Nativeˉstackˉvalue>();
            Nativeˉterminator? Terminator = null;

            int Newˉvalue(Nativeˉvalueˉtype type)
            {
                if (Allˉlocalˉtypes.Length + Valueˉtypes.Count >= Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
                {
                    Fail("WVN2004", "The baseline native function exceeds its combined local/value frame-slot limit.");
                }
                var Result = Valueˉtypes.Count;
                Valueˉtypes.Add(type);
                return Result;
            }

            Nativeˉstackˉvalue Popˉvalue(Nativeˉvalueˉtype expected)
            {
                if (Stack.Count == 0)
                {
                    Fail("WVN2003", "Verified WVB unexpectedly underflowed during native lowering.");
                }
                var Value = Stack.Pop();
                if (Value.Type != expected)
                {
                    Fail("WVN2003", "Verified WVB exposed an unexpected value type during native lowering.");
                }
                return Value;
            }

            for (var Index = Startˉindex; Index < Endˉindex; Index++)
            {
                var Instruction = Instructions[Index];
                var Isˉlast = Index == Endˉindex - 1;
                Operations.Add(new Nativeˉinstructionˉcharge());
                switch (Instruction.Opcode)
                {
                    case Opcode.I32ˉconst:
                        var I32ˉconstant = Newˉvalue(Nativeˉvalueˉtype.I32);
                        Operations.Add(new Nativeˉi32ˉconstant(I32ˉconstant, Instruction.Signedˉoperand));
                        Stack.Push(new(I32ˉconstant, Nativeˉvalueˉtype.I32));
                        break;
                    case Opcode.Boolˉconst:
                        var Boolˉconstant = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉboolˉconstant(Boolˉconstant, Instruction.Unsignedˉoperand != 0));
                        Stack.Push(new(Boolˉconstant, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.Textˉconst:
                        var Textˉdata = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Textˉdata >= (uint)module.Module.Data.Length ||
                            module.Module.Data[Textˉdata] is not Textˉdataˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid static text during native lowering.");
                        }
                        var Textˉconstant = Newˉvalue(Nativeˉvalueˉtype.Staticˉtext);
                        Operations.Add(new Nativeˉstaticˉtextˉconstant(Textˉconstant, Textˉdata));
                        Stack.Push(new(Textˉconstant, Nativeˉvalueˉtype.Staticˉtext, Textˉdata));
                        break;
                    case Opcode.Localˉload:
                        var Loadˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Loadˉindex >= (uint)Allˉlocalˉtypes.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid local load during native lowering.");
                        }
                        var Loadˉtype = Allˉlocalˉtypes[Loadˉindex];
                        var Localˉtextˉdata = Loadˉtype == Nativeˉvalueˉtype.Staticˉtext
                            ? Staticˉtextˉlocalˉdata[Loadˉindex]
                            : -1;
                        if (Loadˉtype == Nativeˉvalueˉtype.Staticˉtext && Localˉtextˉdata < 0)
                        {
                            Fail("WVN2003", "A native static-text local must have one proven immutable data source before use.");
                        }
                        var Loadˉresult = Newˉvalue(Loadˉtype);
                        Operations.Add(new Nativeˉlocalˉload(Loadˉresult, Loadˉindex, Loadˉtype));
                        Stack.Push(new(Loadˉresult, Loadˉtype, Localˉtextˉdata));
                        break;
                    case Opcode.Localˉstore:
                        var Storeˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Storeˉindex >= (uint)Allˉlocalˉtypes.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid local store during native lowering.");
                        }
                        var Storeˉtype = Allˉlocalˉtypes[Storeˉindex];
                        var Storedˉvalue = Popˉvalue(Storeˉtype);
                        if (Storeˉtype == Nativeˉvalueˉtype.Staticˉtext)
                        {
                            if (Storedˉvalue.Data < 0 ||
                                (Staticˉtextˉlocalˉdata[Storeˉindex] >= 0 &&
                                    Staticˉtextˉlocalˉdata[Storeˉindex] != Storedˉvalue.Data))
                            {
                                Fail("WVN2003", "A native static-text local must retain one immutable data source.");
                            }
                            Staticˉtextˉlocalˉdata[Storeˉindex] = Storedˉvalue.Data;
                        }
                        Operations.Add(new Nativeˉlocalˉstore(Storeˉindex, Storeˉtype, Storedˉvalue.Value));
                        break;
                    case Opcode.I32ˉadd:
                    case Opcode.I32ˉsubtract:
                    case Opcode.I32ˉmultiply:
                        var Binaryˉright = Popˉvalue(Nativeˉvalueˉtype.I32);
                        var Binaryˉleft = Popˉvalue(Nativeˉvalueˉtype.I32);
                        var Binaryˉresult = Newˉvalue(Nativeˉvalueˉtype.I32);
                        Operations.Add(new Nativeˉi32ˉbinary(
                            Binaryˉresult,
                            Instruction.Opcode switch
                            {
                                Opcode.I32ˉadd => Nativeˉi32ˉbinaryˉkind.Add,
                                Opcode.I32ˉsubtract => Nativeˉi32ˉbinaryˉkind.Subtract,
                                _ => Nativeˉi32ˉbinaryˉkind.Multiply,
                            },
                            Binaryˉleft.Value,
                            Binaryˉright.Value));
                        Stack.Push(new(Binaryˉresult, Nativeˉvalueˉtype.I32));
                        break;
                    case Opcode.I32ˉnegate:
                        var Negatedˉvalue = Popˉvalue(Nativeˉvalueˉtype.I32);
                        var Negateˉresult = Newˉvalue(Nativeˉvalueˉtype.I32);
                        Operations.Add(new Nativeˉi32ˉnegate(Negateˉresult, Negatedˉvalue.Value));
                        Stack.Push(new(Negateˉresult, Nativeˉvalueˉtype.I32));
                        break;
                    case Opcode.I32ˉequal:
                    case Opcode.I32ˉnotˉequal:
                    case Opcode.I32ˉless:
                    case Opcode.I32ˉlessˉequal:
                    case Opcode.I32ˉgreater:
                    case Opcode.I32ˉgreaterˉequal:
                        var Compareˉright = Popˉvalue(Nativeˉvalueˉtype.I32);
                        var Compareˉleft = Popˉvalue(Nativeˉvalueˉtype.I32);
                        var Compareˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉi32ˉcomparison(
                            Compareˉresult,
                            Instruction.Opcode switch
                            {
                                Opcode.I32ˉequal => Nativeˉi32ˉcomparisonˉkind.Equal,
                                Opcode.I32ˉnotˉequal => Nativeˉi32ˉcomparisonˉkind.Notˉequal,
                                Opcode.I32ˉless => Nativeˉi32ˉcomparisonˉkind.Less,
                                Opcode.I32ˉlessˉequal => Nativeˉi32ˉcomparisonˉkind.Lessˉequal,
                                Opcode.I32ˉgreater => Nativeˉi32ˉcomparisonˉkind.Greater,
                                _ => Nativeˉi32ˉcomparisonˉkind.Greaterˉequal,
                            },
                            Compareˉleft.Value,
                            Compareˉright.Value));
                        Stack.Push(new(Compareˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.Boolˉequal:
                    case Opcode.Boolˉnotˉequal:
                        var Boolˉright = Popˉvalue(Nativeˉvalueˉtype.Bool);
                        var Boolˉleft = Popˉvalue(Nativeˉvalueˉtype.Bool);
                        var Boolˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉboolˉcomparison(
                            Boolˉresult,
                            Instruction.Opcode == Opcode.Boolˉequal
                                ? Nativeˉboolˉcomparisonˉkind.Equal
                                : Nativeˉboolˉcomparisonˉkind.Notˉequal,
                            Boolˉleft.Value,
                            Boolˉright.Value));
                        Stack.Push(new(Boolˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.Boolˉnot:
                        var Boolˉinput = Popˉvalue(Nativeˉvalueˉtype.Bool);
                        var Notˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉboolˉnot(Notˉresult, Boolˉinput.Value));
                        Stack.Push(new(Notˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.Dataˉlength:
                        var Lengthˉdata = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Lengthˉdata >= (uint)module.Module.Data.Length ||
                            module.Module.Data[Lengthˉdata] is not I32ˉarrayˉdataˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid i32 data length during native lowering.");
                        }
                        var Lengthˉdeclaration =
                            (I32ˉarrayˉdataˉdeclaration)module.Module.Data[Lengthˉdata];
                        var Lengthˉresult = Newˉvalue(Nativeˉvalueˉtype.I32);
                        Operations.Add(new Nativeˉdataˉlength(
                            Lengthˉresult,
                            Lengthˉdata,
                            Lengthˉdeclaration.Values.Length));
                        Stack.Push(new(Lengthˉresult, Nativeˉvalueˉtype.I32));
                        break;
                    case Opcode.Dataˉloadˉi32:
                        var Loadˉdata = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Loadˉdata >= (uint)module.Module.Data.Length ||
                            module.Module.Data[Loadˉdata] is not I32ˉarrayˉdataˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid i32 data during native lowering.");
                        }
                        var Dataˉindex = Popˉvalue(Nativeˉvalueˉtype.I32);
                        var Dataˉresult = Newˉvalue(Nativeˉvalueˉtype.I32);
                        Operations.Add(new Nativeˉdataˉloadˉi32(Dataˉresult, Loadˉdata, Dataˉindex.Value));
                        Stack.Push(new(Dataˉresult, Nativeˉvalueˉtype.I32));
                        break;
                    case Opcode.Callˉcapability:
                        var Capabilityˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Capabilityˉindex >= (uint)module.Module.Capabilities.Length ||
                            module.Module.Capabilities[Capabilityˉindex] is not
                            {
                                Name: Capabilityˉcatalog.CONSOLE_WRITE_LINE,
                                Parameterˉtypes.Length: 1,
                                Returnˉtype: Valueˉtype.Void,
                            } ||
                            module.Module.Capabilities[Capabilityˉindex].Parameterˉtypes[0] != Valueˉtype.Text)
                        {
                            Fail("WVN2003", "Verified WVB exposed an unsupported native capability call.");
                        }
                        var Consoleˉtext = Popˉvalue(Nativeˉvalueˉtype.Staticˉtext);
                        if (Consoleˉtext.Data < 0)
                        {
                            Fail("WVN2003", "The native console slice requires a static text argument.");
                        }
                        Operations.Add(new Nativeˉconsoleˉwriteˉline(Consoleˉtext.Value, Consoleˉtext.Data));
                        break;
                    case Opcode.Call:
                        var Calledˉfunctionˉindex = checked((int)Instruction.Unsignedˉoperand);
                        var Calledˉfunction = module.Functions[Calledˉfunctionˉindex].Declaration;
                        var Arguments = new Nativeˉstackˉvalue[Calledˉfunction.Parameterˉtypes.Length];
                        for (var Argumentˉindex = Arguments.Length - 1; Argumentˉindex >= 0; Argumentˉindex--)
                        {
                            Arguments[Argumentˉindex] = Popˉvalue(Toˉnativeˉtype(
                                Calledˉfunction.Parameterˉtypes[Argumentˉindex]));
                        }
                        var Callˉtype = Toˉnativeˉtype(Calledˉfunction.Returnˉtype);
                        var Callˉresult = Newˉvalue(Callˉtype);
                        Operations.Add(new Nativeˉcall(
                            Callˉresult,
                            Callˉtype,
                            Calledˉfunctionˉindex,
                            Arguments.Select(Argument => Argument.Value).ToImmutableArray()));
                        Stack.Push(new(Callˉresult, Callˉtype));
                        break;
                    case Opcode.Jump:
                        Requireˉterminator(Isˉlast, Stack, Instruction.Opcode);
                        Terminator = new Nativeˉjump(Blockˉids[checked((int)Instruction.Unsignedˉoperand)]);
                        break;
                    case Opcode.Branchˉfalse:
                        if (!Isˉlast)
                        {
                            Fail("WVN2003", "A native branch must terminate its basic block.");
                        }
                        var Condition = Popˉvalue(Nativeˉvalueˉtype.Bool);
                        if (Stack.Count != 0)
                        {
                            Fail("WVN2003", "The baseline native subset requires an empty operand stack at control-flow boundaries.");
                        }
                        var Fallthroughˉoffset = checked(Instruction.Offset + Instruction.Size);
                        Terminator = new Nativeˉbranch(
                            Condition.Value,
                            Blockˉids[Fallthroughˉoffset],
                            Blockˉids[checked((int)Instruction.Unsignedˉoperand)]);
                        break;
                    case Opcode.Return:
                        if (!Isˉlast)
                        {
                            Fail("WVN2003", "A native return must terminate its basic block.");
                        }
                        var Returnˉvalue = Popˉvalue(Toˉnativeˉtype(function.Declaration.Returnˉtype));
                        if (Stack.Count != 0)
                        {
                            Fail("WVN2003", "The baseline native subset requires an empty operand stack at return.");
                        }
                        Terminator = new Nativeˉreturn(Returnˉvalue.Value);
                        break;
                    default:
                        Fail(
                            "WVN2003",
                            $"The baseline native subset does not support verified opcode '{Instruction.Opcode}'.");
                        break;
                }
            }

            if (Terminator is null)
            {
                if (Stack.Count != 0 || Blockˉid + 1 >= Orderedˉleaders.Length)
                {
                    Fail("WVN2003", "The baseline native subset requires an empty-stack fallthrough into another basic block.");
                }
                Terminator = new Nativeˉjump(Blockˉid + 1);
            }
            Blocks.Add(new(Blockˉid, Operations.ToImmutable(), Terminator));
        }

        return new(
            function.Declaration.Name,
            Parameterˉtypes,
            Toˉnativeˉtype(function.Declaration.Returnˉtype),
            Localˉtypes,
            Valueˉtypes.ToImmutable(),
            Blocks.ToImmutable());
    }

    private static Nativeˉfragment Selectˉx64(Nativeˉmodule module, int mainˉfunction)
    {
        if (module.Functions.IsDefaultOrEmpty ||
            (uint)mainˉfunction >= (uint)module.Functions.Length ||
            module.Functions[mainˉfunction] is not { Name: "Main", Blocks.Length: >= 1 } ||
            module.Data.IsDefault ||
            module.Requiredˉservices.IsDefault ||
            module.Requiredˉservices.Length > 1 ||
            (module.Requiredˉservices.Length == 1 &&
                module.Requiredˉservices[0] != Nativeˉservice.Consoleˉwriteˉline))
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }
        foreach (var Function in module.Functions)
        {
            Validateˉfunction(module, Function);
        }
        foreach (var Data in module.Data)
        {
            if (Data is null ||
                !Seedˉnames.Isˉidentifier(Data.Name) ||
                Data is not (Nativeˉi32ˉdata { Values.IsDefault: false } or
                    Nativeˉutf8ˉdata { Bytes.IsDefault: false }))
            {
                Fail("WVN2901", "The x86-64 selector received invalid immutable data metadata.");
            }
        }

        var Code = new List<byte>();
        var Functionˉoffsets = new int[module.Functions.Length];
        var Functionˉsizes = new int[module.Functions.Length];
        var Callˉpatches = new List<Nativeˉcallˉpatch>();
        var Dataˉreferences = new List<Nativeˉdataˉreference>();

        for (var Functionˉindex = 0; Functionˉindex < module.Functions.Length; Functionˉindex++)
        {
            var Function = module.Functions[Functionˉindex];
            Functionˉoffsets[Functionˉindex] = Code.Count;
            var Allˉlocals = Function.Allˉlocalˉtypes;
            var Usedˉslots = checked(Allˉlocals.Length + Function.Valueˉtypes.Length);
            var Frameˉbytes = checked((Usedˉslots * sizeof(int) + 15) & ~15);
            var Overflowˉpatches = new List<int>();
            var Instructionˉlimitˉpatches = new List<int>();
            var Boundsˉpatches = new List<int>();
            var Runtimeˉserviceˉpatches = new List<int>();
            var Propagateˉpatches = new List<int>();
            var Depthˉpatches = new List<int>();
            var Branchˉpatches = new List<Nativeˉbranchˉpatch>();
            var Blockˉoffsets = new int[Function.Blocks.Length];

            var Isˉmain = Functionˉindex == mainˉfunction;
            if (Isˉmain)
            {
                Code.AddRange([0x41, 0x57]); // push r15: preserve host nonvolatile context register
                Code.AddRange([0x49, 0x89, 0xD7]); // mov r15, rdx: shared execution-context pointer
                Code.AddRange([0x4D, 0x8B, 0x5F, Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET]);
                Code.AddRange([0x4D, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET]);
            }
            Code.AddRange([0x49, 0x83, 0xEA, 0x01, 0x0F, 0x82]); // sub r10, 1; jb depth trap
            Depthˉpatches.Add(Code.Count);
            Addˉi32(Code, 0);
            Emitˉframeˉadjustment(Code, subtract: true, Frameˉbytes);
            Code.AddRange([0x31, 0xC0]);
            for (var Slot = 0; Slot < Frameˉbytes / sizeof(int); Slot++)
            {
                Emitˉstoreˉeax(Code, Slot);
            }
            for (var Parameter = 0; Parameter < Function.Parameterˉtypes.Length; Parameter++)
            {
                Emitˉstoreˉargument(Code, Parameter);
            }

            foreach (var Block in Function.Blocks)
            {
                Blockˉoffsets[Block.Id] = Code.Count;
                foreach (var Operation in Block.Operations)
                {
                    switch (Operation)
                    {
                        case Nativeˉinstructionˉcharge:
                            Emitˉinstructionˉcharge(Code, Instructionˉlimitˉpatches);
                            break;
                        case Nativeˉi32ˉconstant Constant:
                            Emitˉconstant(Code, Constant.Value, Valueˉslot(Function, Constant.Result));
                            break;
                        case Nativeˉboolˉconstant Constant:
                            Emitˉconstant(Code, Constant.Value ? 1 : 0, Valueˉslot(Function, Constant.Result));
                            break;
                        case Nativeˉstaticˉtextˉconstant Constant:
                            Emitˉconstant(Code, Constant.Data, Valueˉslot(Function, Constant.Result));
                            break;
                        case Nativeˉlocalˉload Load:
                            Emitˉcopy(Code, Load.Local, Valueˉslot(Function, Load.Result));
                            break;
                        case Nativeˉlocalˉstore Store:
                            Emitˉcopy(Code, Valueˉslot(Function, Store.Value), Store.Local);
                            break;
                        case Nativeˉi32ˉbinary Binary:
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Binary.Left));
                            Emitˉloadˉecx(Code, Valueˉslot(Function, Binary.Right));
                            switch (Binary.Kind)
                            {
                                case Nativeˉi32ˉbinaryˉkind.Add:
                                    Code.AddRange([0x01, 0xC8]);
                                    break;
                                case Nativeˉi32ˉbinaryˉkind.Subtract:
                                    Code.AddRange([0x29, 0xC8]);
                                    break;
                                case Nativeˉi32ˉbinaryˉkind.Multiply:
                                    Code.AddRange([0x0F, 0xAF, 0xC1]);
                                    break;
                            }
                            Emitˉoverflowˉbranch(Code, Overflowˉpatches);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Binary.Result));
                            break;
                        case Nativeˉi32ˉnegate Negate:
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Negate.Value));
                            Code.AddRange([0xF7, 0xD8]);
                            Emitˉoverflowˉbranch(Code, Overflowˉpatches);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Negate.Result));
                            break;
                        case Nativeˉi32ˉcomparison Comparison:
                            Emitˉcomparison(
                                Code,
                                Valueˉslot(Function, Comparison.Left),
                                Valueˉslot(Function, Comparison.Right),
                                Comparison.Kind switch
                                {
                                    Nativeˉi32ˉcomparisonˉkind.Equal => 0x94,
                                    Nativeˉi32ˉcomparisonˉkind.Notˉequal => 0x95,
                                    Nativeˉi32ˉcomparisonˉkind.Less => 0x9C,
                                    Nativeˉi32ˉcomparisonˉkind.Lessˉequal => 0x9E,
                                    Nativeˉi32ˉcomparisonˉkind.Greater => 0x9F,
                                    _ => 0x9D,
                                },
                                Valueˉslot(Function, Comparison.Result));
                            break;
                        case Nativeˉboolˉcomparison Comparison:
                            Emitˉcomparison(
                                Code,
                                Valueˉslot(Function, Comparison.Left),
                                Valueˉslot(Function, Comparison.Right),
                                Comparison.Kind == Nativeˉboolˉcomparisonˉkind.Equal ? (byte)0x94 : (byte)0x95,
                                Valueˉslot(Function, Comparison.Result));
                            break;
                        case Nativeˉboolˉnot Not:
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Not.Value));
                            Code.AddRange([0x83, 0xF0, 0x01]);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Not.Result));
                            break;
                        case Nativeˉdataˉlength Length:
                            Emitˉconstant(Code, Length.Length, Valueˉslot(Function, Length.Result));
                            break;
                        case Nativeˉdataˉloadˉi32 Load:
                            var I32ˉdata = (Nativeˉi32ˉdata)module.Data[Load.Data];
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Load.Index));
                            Code.Add(0x3D);
                            Addˉi32(Code, I32ˉdata.Values.Length);
                            Code.AddRange([0x0F, 0x83]);
                            Boundsˉpatches.Add(Code.Count);
                            Addˉi32(Code, 0);
                            Code.AddRange([0x48, 0x8D, 0x15]);
                            Dataˉreferences.Add(new(Code.Count, Load.Data));
                            Addˉi32(Code, 0);
                            Code.AddRange([0x8B, 0x04, 0x82]);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Load.Result));
                            break;
                        case Nativeˉconsoleˉwriteˉline Write:
                            var Text = (Nativeˉutf8ˉdata)module.Data[Write.Data];
                            Code.AddRange([0x4C, 0x8D, 0x05]);
                            Dataˉreferences.Add(new(Code.Count, Write.Data));
                            Addˉi32(Code, 0);
                            Code.AddRange([0x41, 0xB9]);
                            Addˉi32(Code, Text.Bytes.Length);
                            Code.AddRange(
                            [
                                0x49, 0x8B, 0x47,
                                Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
                                0x48, 0x8B, 0x40,
                                Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
                                0xFF, 0xD0,
                                0x85, 0xC0,
                                0x0F, 0x85,
                            ]);
                            Runtimeˉserviceˉpatches.Add(Code.Count);
                            Addˉi32(Code, 0);
                            break;
                        case Nativeˉcall Call:
                            for (var Argument = 0; Argument < Call.Arguments.Length; Argument++)
                            {
                                Emitˉloadˉargument(
                                    Code,
                                    Argument,
                                    Valueˉslot(Function, Call.Arguments[Argument]));
                            }
                            Code.Add(0xE8);
                            Callˉpatches.Add(new(Code.Count, Call.Function));
                            Addˉi32(Code, 0);
                            Code.AddRange([0x48, 0x89, 0xC2, 0x48, 0xC1, 0xEA, 0x20, 0x48, 0x85, 0xD2, 0x0F, 0x85]);
                            Propagateˉpatches.Add(Code.Count);
                            Addˉi32(Code, 0);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Call.Result));
                            break;
                    }
                }

                var Hasˉchargedˉterminator = !Block.Operations.IsEmpty &&
                    Block.Operations[^1] is Nativeˉinstructionˉcharge;
                switch (Block.Terminator)
                {
                    case Nativeˉjump Jump:
                        if (Hasˉchargedˉterminator || Jump.Targetˉblock != Block.Id + 1)
                        {
                            Emitˉdirectˉbranch(Code, 0xE9, Jump.Targetˉblock, Branchˉpatches);
                        }
                        break;
                    case Nativeˉbranch Branch:
                        Emitˉloadˉeax(Code, Valueˉslot(Function, Branch.Condition));
                        Code.AddRange([0x85, 0xC0, 0x0F, 0x85]);
                        Branchˉpatches.Add(new(Code.Count, Branch.Trueˉblock));
                        Addˉi32(Code, 0);
                        Emitˉdirectˉbranch(Code, 0xE9, Branch.Falseˉblock, Branchˉpatches);
                        break;
                    case Nativeˉreturn Return:
                        Emitˉloadˉeax(Code, Valueˉslot(Function, Return.Value));
                        Emitˉfunctionˉreturn(Code, Frameˉbytes, Isˉmain);
                        break;
                }
            }

            var Propagateˉoffset = Code.Count;
            Emitˉframeˉadjustment(Code, subtract: false, Frameˉbytes);
            Emitˉrestoreˉdepthˉandˉreturn(Code, Isˉmain);
            var Overflowˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, INTEGER_OVERFLOW_STATUS, Isˉmain);
            var Instructionˉlimitˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, INSTRUCTION_LIMIT_STATUS, Isˉmain);
            var Boundsˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, DATA_BOUNDS_STATUS, Isˉmain);
            var Runtimeˉserviceˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, RUNTIME_SERVICE_STATUS, Isˉmain);
            var Depthˉoffset = Code.Count;
            Code.AddRange([0x49, 0xFF, 0xC2, 0x48, 0xB8]);
            Addˉu64(Code, CALL_DEPTH_STATUS);
            if (Isˉmain)
            {
                Code.AddRange([0x41, 0x5F]);
            }
            Code.Add(0xC3);

            foreach (var Patchˉoffset in Overflowˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Overflowˉoffset);
            }
            foreach (var Patchˉoffset in Instructionˉlimitˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Instructionˉlimitˉoffset);
            }
            foreach (var Patchˉoffset in Boundsˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Boundsˉoffset);
            }
            foreach (var Patchˉoffset in Runtimeˉserviceˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Runtimeˉserviceˉoffset);
            }
            foreach (var Patchˉoffset in Propagateˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Propagateˉoffset);
            }
            foreach (var Patchˉoffset in Depthˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Depthˉoffset);
            }
            foreach (var Patch in Branchˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patch.Offset, Blockˉoffsets[Patch.Targetˉblock]);
            }
            Functionˉsizes[Functionˉindex] = checked(Code.Count - Functionˉoffsets[Functionˉindex]);
        }

        foreach (var Patch in Callˉpatches)
        {
            Writeˉrelativeˉi32(Code, Patch.Offset, Functionˉoffsets[Patch.Targetˉfunction]);
        }

        var Dataˉoffsets = new int[module.Data.Length];
        if (!module.Data.IsEmpty)
        {
            while ((Code.Count & 15) != 0)
            {
                Code.Add(0x90);
            }
            for (var Dataˉindex = 0; Dataˉindex < module.Data.Length; Dataˉindex++)
            {
                Dataˉoffsets[Dataˉindex] = Code.Count;
                switch (module.Data[Dataˉindex])
                {
                    case Nativeˉi32ˉdata I32:
                        foreach (var Value in I32.Values)
                        {
                            Addˉi32(Code, Value);
                        }
                        break;
                    case Nativeˉutf8ˉdata Text:
                        Code.AddRange(Text.Bytes);
                        break;
                }
            }
        }

        var Nativeˉpatches = ImmutableArray.CreateBuilder<Nativeˉpatch>(Dataˉreferences.Count);
        foreach (var Reference in Dataˉreferences)
        {
            Writeˉrelativeˉi32(Code, Reference.Offset, Dataˉoffsets[Reference.Data]);
            Nativeˉpatches.Add(new(
                Nativeˉpatchˉkind.Relativeˉi32,
                (uint)Reference.Offset,
                Dataˉsymbolˉname(Reference.Data),
                -sizeof(int)));
        }

        if (Code.Count > Nativeˉcontract.MAXIMUM_CODE_BYTES)
        {
            Fail("WVN2902", "The selected x86-64 fragment exceeds its code-size limit.");
        }
        var Bytes = Code.ToImmutableArray();
        var Symbols = ImmutableArray.CreateBuilder<Nativeˉsymbol>();
        for (var Functionˉindex = 0; Functionˉindex < module.Functions.Length; Functionˉindex++)
        {
            Symbols.Add(new(
                Functionˉindex == mainˉfunction ? "Main" : Functionˉsymbolˉname(Functionˉindex),
                Functionˉindex == mainˉfunction ? Nativeˉsymbolˉbinding.Export : Nativeˉsymbolˉbinding.Local,
                Nativeˉsymbolˉkind.Function,
                (uint)Functionˉoffsets[Functionˉindex],
                (uint)Functionˉsizes[Functionˉindex]));
        }
        for (var Dataˉindex = 0; Dataˉindex < module.Data.Length; Dataˉindex++)
        {
            Symbols.Add(new(
                Dataˉsymbolˉname(Dataˉindex),
                Nativeˉsymbolˉbinding.Local,
                Nativeˉsymbolˉkind.Data,
                (uint)Dataˉoffsets[Dataˉindex],
                module.Data[Dataˉindex] switch
                {
                    Nativeˉi32ˉdata I32 => checked((uint)I32.Values.Length * sizeof(int)),
                    Nativeˉutf8ˉdata Text => checked((uint)Text.Bytes.Length),
                    _ => throw new InvalidOperationException("Verified native data became invalid."),
                }));
        }

        var Fragment = new Nativeˉfragment(
            Nativeˉcontract.X64_BASELINE_TARGET,
            Nativeˉcontract.ABI_VERSION,
            Objectˉarchitecture.X86ˉ64,
            16,
            Bytes,
            Symbols
                .OrderBy(Symbol => Symbol.Binding)
                .ThenBy(Symbol => Symbol.Name, StringComparer.Ordinal)
                .ToImmutableArray(),
            Nativeˉpatches
                .OrderBy(Patch => Patch.Offset)
                .ToImmutableArray(),
            module.Requiredˉservices);
        return Nativeˉfragmentˉverifier.Verify(Fragment);
    }

    private static void Validateˉfunction(Nativeˉmodule module, Nativeˉfunction function)
    {
        if (function.Parameterˉtypes.IsDefault ||
            function.Localˉtypes.IsDefault ||
            function.Valueˉtypes.IsDefault ||
            function.Blocks.IsDefaultOrEmpty ||
            function.Blocks.Length > Nativeˉcontract.MAXIMUM_BLOCKS ||
            function.Parameterˉtypes.Length > Nativeˉcontract.MAXIMUM_CALL_PARAMETERS ||
            function.Allˉlocalˉtypes.Length + function.Valueˉtypes.Length is < 1 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
            function.Returnˉtype is not (Nativeˉvalueˉtype.I32 or Nativeˉvalueˉtype.Bool) ||
            function.Parameterˉtypes.Any(Type => Type is not (Nativeˉvalueˉtype.I32 or Nativeˉvalueˉtype.Bool)) ||
            function.Localˉtypes.Any(Type => Type is not (
                Nativeˉvalueˉtype.I32 or
                Nativeˉvalueˉtype.Bool or
                Nativeˉvalueˉtype.Staticˉtext)) ||
            function.Valueˉtypes.Any(Type => !Enum.IsDefined(Type)))
        {
            Fail("WVN2901", "The x86-64 selector received invalid native function metadata.");
        }

        var Nextˉvalue = 0;
        var Returnˉcount = 0;
        var Staticˉtextˉdata = new Dictionary<int, int>();
        var Staticˉtextˉlocalˉdata = new Dictionary<int, int>();
        for (var Blockˉindex = 0; Blockˉindex < function.Blocks.Length; Blockˉindex++)
        {
            var Block = function.Blocks[Blockˉindex];
            if (Block is null || Block.Id != Blockˉindex || Block.Operations.IsDefault || Block.Terminator is null)
            {
                Fail("WVN2901", "The x86-64 selector requires canonical initialized basic blocks.");
            }
            var Chargeˉpending = false;
            foreach (var Operation in Block.Operations)
            {
                if (Operation is Nativeˉinstructionˉcharge)
                {
                    if (Chargeˉpending)
                    {
                        Fail("WVN2901", "The x86-64 selector rejects consecutive native instruction charges.");
                    }
                    Chargeˉpending = true;
                    continue;
                }
                if (!Chargeˉpending)
                {
                    Fail("WVN2901", "Every native semantic operation must consume one instruction charge.");
                }
                Chargeˉpending = false;
                switch (Operation)
                {
                    case Nativeˉi32ˉconstant Constant:
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉboolˉconstant Constant:
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉstaticˉtextˉconstant Constant:
                        if ((uint)Constant.Data >= (uint)module.Data.Length ||
                            module.Data[Constant.Data] is not Nativeˉutf8ˉdata)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid static text metadata.");
                        }
                        Requireˉresult(
                            function,
                            Constant.Result,
                            Nativeˉvalueˉtype.Staticˉtext,
                            ref Nextˉvalue);
                        Staticˉtextˉdata.Add(Constant.Result, Constant.Data);
                        break;
                    case Nativeˉlocalˉload Load:
                        Requireˉlocal(function, Load.Local, Load.Type);
                        Requireˉresult(function, Load.Result, Load.Type, ref Nextˉvalue);
                        if (Load.Type == Nativeˉvalueˉtype.Staticˉtext)
                        {
                            if (!Staticˉtextˉlocalˉdata.TryGetValue(Load.Local, out var Data))
                            {
                                Fail("WVN2901", "The x86-64 selector received an unproven static-text local load.");
                            }
                            Staticˉtextˉdata.Add(Load.Result, Data);
                        }
                        break;
                    case Nativeˉlocalˉstore Store:
                        Requireˉlocal(function, Store.Local, Store.Type);
                        Requireˉvalue(function, Store.Value, Store.Type, Nextˉvalue);
                        if (Store.Type == Nativeˉvalueˉtype.Staticˉtext)
                        {
                            if (!Staticˉtextˉdata.TryGetValue(Store.Value, out var Data) ||
                                (Staticˉtextˉlocalˉdata.TryGetValue(Store.Local, out var Existing) &&
                                    Existing != Data))
                            {
                                Fail("WVN2901", "The x86-64 selector received an inconsistent static-text local store.");
                            }
                            Staticˉtextˉlocalˉdata[Store.Local] = Data;
                        }
                        break;
                    case Nativeˉi32ˉbinary Binary when Enum.IsDefined(Binary.Kind):
                        Requireˉvalue(function, Binary.Left, Nativeˉvalueˉtype.I32, Nextˉvalue);
                        Requireˉvalue(function, Binary.Right, Nativeˉvalueˉtype.I32, Nextˉvalue);
                        Requireˉresult(function, Binary.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉi32ˉnegate Negate:
                        Requireˉvalue(function, Negate.Value, Nativeˉvalueˉtype.I32, Nextˉvalue);
                        Requireˉresult(function, Negate.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉi32ˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.I32, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.I32, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉboolˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.Bool, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.Bool, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉboolˉnot Not:
                        Requireˉvalue(function, Not.Value, Nativeˉvalueˉtype.Bool, Nextˉvalue);
                        Requireˉresult(function, Not.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉdataˉlength Length:
                        Requireˉdata(module, Length.Data);
                        if (module.Data[Length.Data] is not Nativeˉi32ˉdata Lengthˉdata ||
                            Length.Length != Lengthˉdata.Values.Length)
                        {
                            Fail("WVN2901", "The x86-64 selector received a noncanonical data length.");
                        }
                        Requireˉresult(function, Length.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉdataˉloadˉi32 Load:
                        Requireˉdata(module, Load.Data);
                        if (module.Data[Load.Data] is not Nativeˉi32ˉdata)
                        {
                            Fail("WVN2901", "The x86-64 selector received a non-i32 indexed data load.");
                        }
                        Requireˉvalue(function, Load.Index, Nativeˉvalueˉtype.I32, Nextˉvalue);
                        Requireˉresult(function, Load.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉconsoleˉwriteˉline Write:
                        Requireˉvalue(function, Write.Text, Nativeˉvalueˉtype.Staticˉtext, Nextˉvalue);
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Consoleˉwriteˉline) ||
                            (uint)Write.Data >= (uint)module.Data.Length ||
                            module.Data[Write.Data] is not Nativeˉutf8ˉdata ||
                            !Staticˉtextˉdata.TryGetValue(Write.Text, out var Definedˉdata) ||
                            Definedˉdata != Write.Data)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid console.write_line metadata.");
                        }
                        break;
                    case Nativeˉcall Call:
                        if ((uint)Call.Function >= (uint)module.Functions.Length ||
                            Call.Arguments.IsDefault ||
                            Call.Type != module.Functions[Call.Function].Returnˉtype ||
                            Call.Arguments.Length != module.Functions[Call.Function].Parameterˉtypes.Length)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid native call metadata.");
                        }
                        for (var Argument = 0; Argument < Call.Arguments.Length; Argument++)
                        {
                            Requireˉvalue(
                                function,
                                Call.Arguments[Argument],
                                module.Functions[Call.Function].Parameterˉtypes[Argument],
                                Nextˉvalue);
                        }
                        Requireˉresult(function, Call.Result, Call.Type, ref Nextˉvalue);
                        break;
                    default:
                        Fail("WVN2901", "The x86-64 selector received an invalid native operation.");
                        break;
                }
            }

            switch (Block.Terminator)
            {
                case Nativeˉjump Jump:
                    Requireˉtarget(function, Jump.Targetˉblock);
                    if (!Chargeˉpending && Jump.Targetˉblock != Block.Id + 1)
                    {
                        Fail("WVN2901", "Only an implicit next-block fallthrough may omit an instruction charge.");
                    }
                    break;
                case Nativeˉbranch Branch:
                    if (!Chargeˉpending)
                    {
                        Fail("WVN2901", "A native conditional branch must consume one instruction charge.");
                    }
                    Requireˉvalue(function, Branch.Condition, Nativeˉvalueˉtype.Bool, Nextˉvalue);
                    Requireˉtarget(function, Branch.Trueˉblock);
                    Requireˉtarget(function, Branch.Falseˉblock);
                    break;
                case Nativeˉreturn Return:
                    if (!Chargeˉpending)
                    {
                        Fail("WVN2901", "A native return must consume one instruction charge.");
                    }
                    Requireˉvalue(function, Return.Value, function.Returnˉtype, Nextˉvalue);
                    Returnˉcount++;
                    break;
                default:
                    Fail("WVN2901", "The x86-64 selector received an invalid native terminator.");
                    break;
            }
        }
        if (Nextˉvalue != function.Valueˉtypes.Length || Returnˉcount == 0)
        {
            Fail("WVN2901", "The x86-64 selector requires a complete canonical value graph with a return.");
        }

        var Reachable = new bool[function.Blocks.Length];
        var Pending = new Queue<int>();
        Reachable[0] = true;
        Pending.Enqueue(0);
        while (Pending.TryDequeue(out var Blockˉid))
        {
            foreach (var Target in Targets(function.Blocks[Blockˉid].Terminator))
            {
                if (!Reachable[Target])
                {
                    Reachable[Target] = true;
                    Pending.Enqueue(Target);
                }
            }
        }
        if (Reachable.Any(Value => !Value))
        {
            Fail("WVN2901", "The x86-64 selector rejects unreachable native basic blocks.");
        }
    }

    private static bool Isˉnativeˉscalarˉtype(Valueˉshape type) =>
        type.Nominalˉtypeˉindex == -1 && type.Kind is Valueˉtype.I32 or Valueˉtype.Bool;

    private static bool Isˉnativeˉlocalˉtype(Valueˉshape type) =>
        type.Nominalˉtypeˉindex == -1 &&
        type.Kind is Valueˉtype.I32 or Valueˉtype.Bool or Valueˉtype.Text;

    private static Nativeˉvalueˉtype Toˉnativeˉtype(Valueˉshape type) =>
        type.Kind switch
        {
            Valueˉtype.I32 when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.I32,
            Valueˉtype.Bool when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Bool,
            Valueˉtype.Text when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Staticˉtext,
            _ => throw new Nativeˉbackendˉexception("WVN2002", $"Unsupported native local type '{type}'."),
        };

    private static IEnumerable<int> Targets(Nativeˉterminator terminator) =>
        terminator switch
        {
            Nativeˉjump Jump => [Jump.Targetˉblock],
            Nativeˉbranch Branch => [Branch.Trueˉblock, Branch.Falseˉblock],
            _ => [],
        };

    private static void Requireˉterminator(bool isˉlast, Stack<Nativeˉstackˉvalue> stack, Opcode opcode)
    {
        if (!isˉlast || stack.Count != 0)
        {
            Fail("WVN2003", $"Native {opcode} requires an empty stack and must terminate its basic block.");
        }
    }

    private static void Requireˉlocal(Nativeˉfunction function, int local, Nativeˉvalueˉtype type)
    {
        var Locals = function.Allˉlocalˉtypes;
        if ((uint)local >= (uint)Locals.Length || Locals[local] != type)
        {
            Fail("WVN2901", "The x86-64 selector received an invalid typed local reference.");
        }
    }

    private static void Requireˉvalue(
        Nativeˉfunction function,
        int value,
        Nativeˉvalueˉtype type,
        int available)
    {
        if ((uint)value >= (uint)available || function.Valueˉtypes[value] != type)
        {
            Fail("WVN2901", "The x86-64 selector received an invalid typed value reference.");
        }
    }

    private static void Requireˉresult(
        Nativeˉfunction function,
        int result,
        Nativeˉvalueˉtype type,
        ref int next)
    {
        if (result != next || (uint)result >= (uint)function.Valueˉtypes.Length || function.Valueˉtypes[result] != type)
        {
            Fail("WVN2901", "The x86-64 selector requires canonical typed result numbering.");
        }
        next++;
    }

    private static void Requireˉtarget(Nativeˉfunction function, int target)
    {
        if ((uint)target >= (uint)function.Blocks.Length)
        {
            Fail("WVN2901", "The x86-64 selector received an invalid basic-block target.");
        }
    }

    private static void Requireˉdata(Nativeˉmodule module, int data)
    {
        if ((uint)data >= (uint)module.Data.Length)
        {
            Fail("WVN2901", "The x86-64 selector received an invalid static-data reference.");
        }
    }

    private static int Valueˉslot(Nativeˉfunction function, int value) =>
        checked(function.Allˉlocalˉtypes.Length + value);

    private static void Emitˉconstant(List<byte> code, int value, int targetˉslot)
    {
        code.Add(0xB8);
        Addˉi32(code, value);
        Emitˉstoreˉeax(code, targetˉslot);
    }

    private static void Emitˉcopy(List<byte> code, int sourceˉslot, int targetˉslot)
    {
        Emitˉloadˉeax(code, sourceˉslot);
        Emitˉstoreˉeax(code, targetˉslot);
    }

    private static void Emitˉcomparison(
        List<byte> code,
        int leftˉslot,
        int rightˉslot,
        byte condition,
        int resultˉslot)
    {
        Emitˉloadˉeax(code, leftˉslot);
        Emitˉloadˉecx(code, rightˉslot);
        code.AddRange([0x39, 0xC8, 0x0F, condition, 0xC0, 0x0F, 0xB6, 0xC0]);
        Emitˉstoreˉeax(code, resultˉslot);
    }

    private static void Emitˉframeˉadjustment(List<byte> code, bool subtract, int bytes)
    {
        code.AddRange([0x48, 0x81, subtract ? (byte)0xEC : (byte)0xC4]);
        Addˉi32(code, bytes);
    }

    private static void Emitˉloadˉeax(List<byte> code, int slot)
    {
        code.AddRange([0x8B, 0x84, 0x24]);
        Addˉi32(code, checked(slot * sizeof(int)));
    }

    private static void Emitˉloadˉargument(List<byte> code, int argument, int slot)
    {
        code.AddRange(argument switch
        {
            0 => [0x44, 0x8B, 0x84, 0x24],
            1 => [0x44, 0x8B, 0x8C, 0x24],
            2 => [0x8B, 0x8C, 0x24],
            3 => [0x8B, 0x94, 0x24],
            _ => throw new Nativeˉbackendˉexception("WVN2901", "The native call exceeds its register-argument limit."),
        });
        Addˉi32(code, checked(slot * sizeof(int)));
    }

    private static void Emitˉstoreˉargument(List<byte> code, int argument)
    {
        code.AddRange(argument switch
        {
            0 => [0x44, 0x89, 0x84, 0x24],
            1 => [0x44, 0x89, 0x8C, 0x24],
            2 => [0x89, 0x8C, 0x24],
            3 => [0x89, 0x94, 0x24],
            _ => throw new Nativeˉbackendˉexception("WVN2901", "The native function exceeds its register-parameter limit."),
        });
        Addˉi32(code, checked(argument * sizeof(int)));
    }

    private static void Emitˉloadˉecx(List<byte> code, int slot)
    {
        code.AddRange([0x8B, 0x8C, 0x24]);
        Addˉi32(code, checked(slot * sizeof(int)));
    }

    private static void Emitˉstoreˉeax(List<byte> code, int slot)
    {
        code.AddRange([0x89, 0x84, 0x24]);
        Addˉi32(code, checked(slot * sizeof(int)));
    }

    private static void Emitˉoverflowˉbranch(List<byte> code, List<int> patches)
    {
        code.AddRange([0x0F, 0x80]);
        patches.Add(code.Count);
        Addˉi32(code, 0);
    }

    private static void Emitˉinstructionˉcharge(List<byte> code, List<int> patches)
    {
        code.AddRange([0x49, 0x83, 0xEB, 0x01, 0x0F, 0x82]);
        patches.Add(code.Count);
        Addˉi32(code, 0);
    }

    private static void Emitˉdirectˉbranch(
        List<byte> code,
        byte opcode,
        int targetˉblock,
        List<Nativeˉbranchˉpatch> patches)
    {
        code.Add(opcode);
        patches.Add(new(code.Count, targetˉblock));
        Addˉi32(code, 0);
    }

    private static void Emitˉfunctionˉreturn(List<byte> code, int frameˉbytes, bool restoreˉcontext)
    {
        Emitˉframeˉadjustment(code, subtract: false, frameˉbytes);
        Emitˉrestoreˉdepthˉandˉreturn(code, restoreˉcontext);
    }

    private static void Emitˉrestoreˉdepthˉandˉreturn(List<byte> code, bool restoreˉcontext)
    {
        code.AddRange([0x49, 0xFF, 0xC2]);
        if (restoreˉcontext)
        {
            code.AddRange([0x41, 0x5F]);
        }
        code.Add(0xC3);
    }

    private static void Emitˉstatusˉtrap(
        List<byte> code,
        int frameˉbytes,
        ulong status,
        bool restoreˉcontext)
    {
        Emitˉframeˉadjustment(code, subtract: false, frameˉbytes);
        code.AddRange([0x49, 0xFF, 0xC2, 0x48, 0xB8]);
        Addˉu64(code, status);
        if (restoreˉcontext)
        {
            code.AddRange([0x41, 0x5F]);
        }
        code.Add(0xC3);
    }

    private static void Writeˉrelativeˉi32(List<byte> code, int displacementˉoffset, int targetˉoffset)
    {
        var Displacement = checked(targetˉoffset - (displacementˉoffset + sizeof(int)));
        Span<byte> Bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(Bytes, Displacement);
        for (var Index = 0; Index < Bytes.Length; Index++)
        {
            code[displacementˉoffset + Index] = Bytes[Index];
        }
    }

    private static string Dataˉsymbolˉname(int index) => $"$data_{index:D4}";

    private static string Functionˉsymbolˉname(int index) => $"$function_{index:D4}";

    private static void Addˉi32(List<byte> code, int value)
    {
        Span<byte> Bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(Bytes, value);
        foreach (var Byte in Bytes)
        {
            code.Add(Byte);
        }
    }

    private static void Addˉu64(List<byte> code, ulong value)
    {
        Span<byte> Bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(Bytes, value);
        foreach (var Byte in Bytes)
        {
            code.Add(Byte);
        }
    }

    private readonly record struct Nativeˉstackˉvalue(
        int Value,
        Nativeˉvalueˉtype Type,
        int Data = -1);

    private readonly record struct Nativeˉbranchˉpatch(int Offset, int Targetˉblock);

    private readonly record struct Nativeˉcallˉpatch(int Offset, int Targetˉfunction);

    private readonly record struct Nativeˉdataˉreference(int Offset, int Data);

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
