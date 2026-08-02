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
    private const ulong BYTE_BOUNDS_STATUS = 0x0000_0006_0000_0000UL;
    private const ulong RECORD_ARENA_STATUS = 0x0000_0007_0000_0000UL;
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Nativeˉcompilation Compile(Verifiedˉmodule verifiedˉmodule)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        var Module = verifiedˉmodule.Module;
        var Isˉportable = Module.Profile == Moduleˉprofile.Portable && Module.Capabilities.IsEmpty;
        var Isˉhosted = Module.Profile == Moduleˉprofile.Hosted &&
            !Module.Capabilities.IsEmpty &&
            Module.Capabilities.All(Isˉsupportedˉnativeˉcapability);
        if ((!Isˉportable && !Isˉhosted) ||
            Module.Types.Any(Type => !Isˉsupportedˉnativeˉnominalˉtype(Type)) ||
            Module.Data.Any(Data => Data is not (
                I32ˉarrayˉdataˉdeclaration or
                Textˉdataˉdeclaration or
                Bytesˉdataˉdeclaration)))
        {
            Fail(
                "WVN2001",
                "The baseline native subset requires either a capability-free portable module or a hosted module declaring only the qualified console, diagnostic, process, and file capabilities, with bounded enum/record metadata and only immutable i32-array/text/bytes data.");
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
            Main.Declaration.Returnˉtype.Kind is not (Valueˉtype.I32 or Valueˉtype.Bytes) ||
            (Main.Declaration.Returnˉtype.Kind == Valueˉtype.Bytes && !Isˉportable))
        {
            Fail(
                "WVN2002",
                "The baseline native entry must be Main() -> i32, or capability-free portable Main() -> bytes; " +
                $"found name='{Main.Declaration.Name}', parameters={Main.Declaration.Parameterˉtypes.Length}, " +
                $"return={Main.Declaration.Returnˉtype}.");
        }
        foreach (var Function in verifiedˉmodule.Functions)
        {
            if (Function.Declaration.Parameterˉtypes.Any(Type => !Isˉnativeˉparameterˉtype(Type)) ||
                Function.Declaration.Parameterˉtypes.Length > Nativeˉcontract.MAXIMUM_CALL_PARAMETERS ||
                !Isˉnativeˉreturnˉtype(Function.Declaration.Returnˉtype) ||
                Function.Declaration.Localˉtypes.Any(Type => !Isˉnativeˉlocalˉtype(Type)) ||
                Function.Declaration.Allˉlocalˉtypes.Length >= Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
                Function.Declaration.Maximumˉstackˉdepth is < 0 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
            {
                Fail(
                    "WVN2002",
                    $"Native function '{Function.Declaration.Name}' must use bounded scalar/borrowed descriptor parameters and locals and a qualified scalar, descriptor, or void return.");
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
                Bytesˉdataˉdeclaration Bytes => new Nativeˉbytesˉdata(Bytes.Name, Bytes.Values),
                _ => throw new Nativeˉbackendˉexception(
                    "WVN2001",
                    $"Unsupported native data '{Declaration.Name}'."),
            })
            .ToImmutableArray();
        var Requiredˉservices = module.Module.Capabilities
            .Select(Capability => Capability.Name switch
            {
                Capabilityˉcatalog.CONSOLE_WRITE_LINE => Nativeˉservice.Consoleˉwriteˉline,
                Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT => Nativeˉservice.Processˉargumentˉcount,
                Capabilityˉcatalog.PROCESS_ARGUMENT => Nativeˉservice.Processˉargument,
                Capabilityˉcatalog.FILE_READ_BYTES => Nativeˉservice.Fileˉreadˉbytes,
                Capabilityˉcatalog.FILE_WRITE_BYTES => Nativeˉservice.Fileˉwriteˉbytes,
                Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE => Nativeˉservice.Diagnosticˉwriteˉline,
                _ => throw new Nativeˉbackendˉexception(
                    "WVN2001",
                    $"Unsupported native capability '{Capability.Name}'."),
            })
            .Concat(Functions
                .SelectMany(Function => Function.Blocks)
                .SelectMany(Block => Block.Operations)
                .SelectMany(Operation => Operation switch
                {
                    Nativeˉtextˉutf8ˉisˉvalid or Nativeˉtextˉfromˉutf8 =>
                        [Nativeˉservice.Textˉutf8ˉisˉvalid],
                    Nativeˉenumˉname => [Nativeˉservice.Enumˉname],
                    Nativeˉintegerˉformat { Kind: Nativeˉintegerˉformatˉkind.I32 } =>
                        [Nativeˉservice.I32ˉformat],
                    Nativeˉintegerˉformat => [Nativeˉservice.U32ˉformat],
                    Nativeˉtextˉconcat => [Nativeˉservice.Textˉconcat],
                    Nativeˉtextˉquote => [Nativeˉservice.Textˉquote],
                    _ => Array.Empty<Nativeˉservice>(),
                }))
            .Distinct()
            .Order()
            .ToImmutableArray();
        return new(Functions, Data, module.Module.Types, Requiredˉservices);
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
        var Referenceˉlocalˉinitialized = Allˉlocalˉtypes
            .Select((Type, Index) =>
                Index < Parameterˉtypes.Length && Isˉnativeˉrunˉreferenceˉtype(Type))
            .ToArray();
        var Valueˉtypes = ImmutableArray.CreateBuilder<Nativeˉvalueˉtype>();
        var Blockˉvalueˉranges = new List<(int First, int Count)>();
        var Hiddenˉresultˉslots = Isˉnativeˉdescriptorˉtype(
            Toˉnativeˉtype(function.Declaration.Returnˉtype)) ? 1 : 0;
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
            var Firstˉblockˉvalue = Valueˉtypes.Count;
            var Startˉindex = Instructionˉindices[Orderedˉleaders[Blockˉid]];
            var Endˉindex = Blockˉid + 1 < Orderedˉleaders.Length
                ? Instructionˉindices[Orderedˉleaders[Blockˉid + 1]]
                : Instructions.Length;
            var Operations = ImmutableArray.CreateBuilder<Nativeˉoperation>();
            var Stack = new Stack<Nativeˉstackˉvalue>();
            Nativeˉterminator? Terminator = null;

            int Newˉvalue(Nativeˉvalueˉtype type)
            {
                if (Valueˉtypes.Count >= Nativeˉcontract.MAXIMUM_VALUE_IDENTIFIERS)
                {
                    Fail(
                        "WVN2004",
                        $"Native function '{function.Declaration.Name}' exceeds the " +
                        $"{Nativeˉcontract.MAXIMUM_VALUE_IDENTIFIERS} bounded semantic-value identifiers.");
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
                    case Opcode.U8ˉconst:
                        var U8ˉconstant = Newˉvalue(Nativeˉvalueˉtype.U8);
                        Operations.Add(new Nativeˉu8ˉconstant(
                            U8ˉconstant,
                            checked((byte)Instruction.Unsignedˉoperand)));
                        Stack.Push(new(U8ˉconstant, Nativeˉvalueˉtype.U8));
                        break;
                    case Opcode.U32ˉconst:
                        var U32ˉconstant = Newˉvalue(Nativeˉvalueˉtype.U32);
                        Operations.Add(new Nativeˉu32ˉconstant(U32ˉconstant, Instruction.Unsignedˉoperand));
                        Stack.Push(new(U32ˉconstant, Nativeˉvalueˉtype.U32));
                        break;
                    case Opcode.Enumˉconst:
                        var Enumˉtypeˉindex = checked((int)Instruction.Unsignedˉoperand);
                        var Enumˉmemberˉindex = checked((int)Instruction.Secondˉunsignedˉoperand);
                        if ((uint)Enumˉtypeˉindex >= (uint)module.Module.Types.Length ||
                            module.Module.Types[Enumˉtypeˉindex] is not Enumˉtypeˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid enum metadata during native lowering.");
                        }
                        var Enumˉtype = (Enumˉtypeˉdeclaration)module.Module.Types[Enumˉtypeˉindex];
                        if ((uint)Enumˉmemberˉindex >= (uint)Enumˉtype.Members.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid enum member metadata during native lowering.");
                        }
                        var Enumˉconstant = Newˉvalue(Nativeˉvalueˉtype.Enum);
                        Operations.Add(new Nativeˉenumˉconstant(
                            Enumˉconstant,
                            Enumˉtypeˉindex,
                            Enumˉmemberˉindex,
                            Enumˉtype.Members[Enumˉmemberˉindex].Value));
                        Stack.Push(new(Enumˉconstant, Nativeˉvalueˉtype.Enum, Enumˉtypeˉindex));
                        break;
                    case Opcode.Textˉconst:
                        var Textˉdata = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Textˉdata >= (uint)module.Module.Data.Length ||
                            module.Module.Data[Textˉdata] is not Textˉdataˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid static text during native lowering.");
                        }
                        var Textˉconstant = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        Operations.Add(new Nativeˉstaticˉtextˉconstant(Textˉconstant, Textˉdata));
                        Stack.Push(new(Textˉconstant, Nativeˉvalueˉtype.Borrowedˉtext));
                        break;
                    case Opcode.Bytesˉconst:
                        var Bytesˉdata = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Bytesˉdata >= (uint)module.Module.Data.Length ||
                            module.Module.Data[Bytesˉdata] is not Bytesˉdataˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid static bytes during native lowering.");
                        }
                        var Bytesˉconstant = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉstaticˉbytesˉconstant(Bytesˉconstant, Bytesˉdata));
                        Stack.Push(new(Bytesˉconstant, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Localˉload:
                        var Loadˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Loadˉindex >= (uint)Allˉlocalˉtypes.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid local load during native lowering.");
                        }
                        var Loadˉtype = Allˉlocalˉtypes[Loadˉindex];
                        if (Isˉnativeˉrunˉreferenceˉtype(Loadˉtype) &&
                            !Referenceˉlocalˉinitialized[Loadˉindex])
                        {
                            Fail("WVN2003", "A native borrowed-descriptor or record local must be initialized before use.");
                        }
                        var Loadˉresult = Newˉvalue(Loadˉtype);
                        Operations.Add(new Nativeˉlocalˉload(Loadˉresult, Loadˉindex, Loadˉtype));
                        Stack.Push(new(
                            Loadˉresult,
                            Loadˉtype,
                            function.Declaration.Allˉlocalˉtypes[Loadˉindex].Nominalˉtypeˉindex));
                        break;
                    case Opcode.Localˉstore:
                        var Storeˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Storeˉindex >= (uint)Allˉlocalˉtypes.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid local store during native lowering.");
                        }
                        var Storeˉtype = Allˉlocalˉtypes[Storeˉindex];
                        var Storedˉvalue = Popˉvalue(Storeˉtype);
                        if (Isˉnativeˉrunˉreferenceˉtype(Storeˉtype))
                        {
                            Referenceˉlocalˉinitialized[Storeˉindex] = true;
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
                    case Opcode.U32ˉadd:
                    case Opcode.U32ˉsubtract:
                    case Opcode.U32ˉmultiply:
                        var U32ˉbinaryˉright = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var U32ˉbinaryˉleft = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var U32ˉbinaryˉresult = Newˉvalue(Nativeˉvalueˉtype.U32);
                        Operations.Add(new Nativeˉu32ˉbinary(
                            U32ˉbinaryˉresult,
                            Instruction.Opcode switch
                            {
                                Opcode.U32ˉadd => Nativeˉu32ˉbinaryˉkind.Add,
                                Opcode.U32ˉsubtract => Nativeˉu32ˉbinaryˉkind.Subtract,
                                _ => Nativeˉu32ˉbinaryˉkind.Multiply,
                            },
                            U32ˉbinaryˉleft.Value,
                            U32ˉbinaryˉright.Value));
                        Stack.Push(new(U32ˉbinaryˉresult, Nativeˉvalueˉtype.U32));
                        break;
                    case Opcode.U32ˉequal:
                    case Opcode.U32ˉnotˉequal:
                    case Opcode.U32ˉless:
                    case Opcode.U32ˉlessˉequal:
                    case Opcode.U32ˉgreater:
                    case Opcode.U32ˉgreaterˉequal:
                        var U32ˉcompareˉright = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var U32ˉcompareˉleft = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var U32ˉcompareˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉu32ˉcomparison(
                            U32ˉcompareˉresult,
                            Instruction.Opcode switch
                            {
                                Opcode.U32ˉequal => Nativeˉu32ˉcomparisonˉkind.Equal,
                                Opcode.U32ˉnotˉequal => Nativeˉu32ˉcomparisonˉkind.Notˉequal,
                                Opcode.U32ˉless => Nativeˉu32ˉcomparisonˉkind.Less,
                                Opcode.U32ˉlessˉequal => Nativeˉu32ˉcomparisonˉkind.Lessˉequal,
                                Opcode.U32ˉgreater => Nativeˉu32ˉcomparisonˉkind.Greater,
                                _ => Nativeˉu32ˉcomparisonˉkind.Greaterˉequal,
                            },
                            U32ˉcompareˉleft.Value,
                            U32ˉcompareˉright.Value));
                        Stack.Push(new(U32ˉcompareˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.U8ˉequal:
                    case Opcode.U8ˉnotˉequal:
                        var U8ˉcompareˉright = Popˉvalue(Nativeˉvalueˉtype.U8);
                        var U8ˉcompareˉleft = Popˉvalue(Nativeˉvalueˉtype.U8);
                        var U8ˉcompareˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉu8ˉcomparison(
                            U8ˉcompareˉresult,
                            Instruction.Opcode == Opcode.U8ˉequal
                                ? Nativeˉu8ˉcomparisonˉkind.Equal
                                : Nativeˉu8ˉcomparisonˉkind.Notˉequal,
                            U8ˉcompareˉleft.Value,
                            U8ˉcompareˉright.Value));
                        Stack.Push(new(U8ˉcompareˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.Enumˉequal:
                    case Opcode.Enumˉnotˉequal:
                        var Enumˉright = Popˉvalue(Nativeˉvalueˉtype.Enum);
                        var Enumˉleft = Popˉvalue(Nativeˉvalueˉtype.Enum);
                        if (Enumˉleft.Nominalˉtypeˉindex != Enumˉright.Nominalˉtypeˉindex)
                        {
                            Fail("WVN2003", "Verified WVB exposed mismatched enum identities during native lowering.");
                        }
                        var Enumˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉenumˉcomparison(
                            Enumˉresult,
                            Instruction.Opcode == Opcode.Enumˉequal
                                ? Nativeˉenumˉcomparisonˉkind.Equal
                                : Nativeˉenumˉcomparisonˉkind.Notˉequal,
                            Enumˉleft.Value,
                            Enumˉright.Value));
                        Stack.Push(new(Enumˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.U32ˉfromˉu8:
                        var U32ˉfromˉu8ˉvalue = Popˉvalue(Nativeˉvalueˉtype.U8);
                        var U32ˉfromˉu8ˉresult = Newˉvalue(Nativeˉvalueˉtype.U32);
                        Operations.Add(new Nativeˉu32ˉfromˉu8(
                            U32ˉfromˉu8ˉresult,
                            U32ˉfromˉu8ˉvalue.Value));
                        Stack.Push(new(U32ˉfromˉu8ˉresult, Nativeˉvalueˉtype.U32));
                        break;
                    case Opcode.Bytesˉfromˉu8:
                        var Encodedˉu8 = Popˉvalue(Nativeˉvalueˉtype.U8);
                        var Encodedˉu8ˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉbytesˉfromˉu8(
                            Encodedˉu8ˉresult,
                            Encodedˉu8.Value));
                        Stack.Push(new(Encodedˉu8ˉresult, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Bytesˉfromˉu16ˉlittle:
                        var Encodedˉu16 = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var Encodedˉu16ˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉbytesˉfromˉu16ˉlittle(
                            Encodedˉu16ˉresult,
                            Encodedˉu16.Value));
                        Stack.Push(new(Encodedˉu16ˉresult, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Bytesˉlength:
                        var Bytesˉlengthˉvalue = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Bytesˉlengthˉresult = Newˉvalue(Nativeˉvalueˉtype.U32);
                        Operations.Add(new Nativeˉbytesˉlength(
                            Bytesˉlengthˉresult,
                            Bytesˉlengthˉvalue.Value));
                        Stack.Push(new(Bytesˉlengthˉresult, Nativeˉvalueˉtype.U32));
                        break;
                    case Opcode.Textˉutf8ˉisˉvalid:
                        var Utf8ˉbytes = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Utf8ˉresult = Newˉvalue(Nativeˉvalueˉtype.Bool);
                        Operations.Add(new Nativeˉtextˉutf8ˉisˉvalid(Utf8ˉresult, Utf8ˉbytes.Value));
                        Stack.Push(new(Utf8ˉresult, Nativeˉvalueˉtype.Bool));
                        break;
                    case Opcode.Textˉfromˉutf8:
                        var Decodedˉbytes = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Decodedˉtext = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        Operations.Add(new Nativeˉtextˉfromˉutf8(Decodedˉtext, Decodedˉbytes.Value));
                        Stack.Push(new(Decodedˉtext, Nativeˉvalueˉtype.Borrowedˉtext));
                        break;
                    case Opcode.Textˉtoˉutf8:
                        var Encodedˉtext = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        var Encodedˉtextˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉtextˉtoˉutf8(
                            Encodedˉtextˉresult,
                            Encodedˉtext.Value));
                        Stack.Push(new(Encodedˉtextˉresult, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Enumˉname:
                        var Namedˉenum = Popˉvalue(Nativeˉvalueˉtype.Enum);
                        if ((uint)Namedˉenum.Nominalˉtypeˉindex >= (uint)module.Module.Types.Length ||
                            module.Module.Types[Namedˉenum.Nominalˉtypeˉindex] is not Enumˉtypeˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed an enum value without native type identity.");
                        }
                        var Enumˉnameˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        Operations.Add(new Nativeˉenumˉname(
                            Enumˉnameˉresult,
                            Namedˉenum.Nominalˉtypeˉindex,
                            Namedˉenum.Value));
                        Stack.Push(new(Enumˉnameˉresult, Nativeˉvalueˉtype.Borrowedˉtext));
                        break;
                    case Opcode.I32ˉformat:
                    case Opcode.U8ˉformat:
                    case Opcode.U32ˉformat:
                        var Formatˉkind = Instruction.Opcode switch
                        {
                            Opcode.I32ˉformat => Nativeˉintegerˉformatˉkind.I32,
                            Opcode.U8ˉformat => Nativeˉintegerˉformatˉkind.U8,
                            _ => Nativeˉintegerˉformatˉkind.U32,
                        };
                        var Formatˉtype = Formatˉkind switch
                        {
                            Nativeˉintegerˉformatˉkind.I32 => Nativeˉvalueˉtype.I32,
                            Nativeˉintegerˉformatˉkind.U8 => Nativeˉvalueˉtype.U8,
                            _ => Nativeˉvalueˉtype.U32,
                        };
                        var Formatˉvalue = Popˉvalue(Formatˉtype);
                        var Formatˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        Operations.Add(new Nativeˉintegerˉformat(Formatˉresult, Formatˉkind, Formatˉvalue.Value));
                        Stack.Push(new(Formatˉresult, Nativeˉvalueˉtype.Borrowedˉtext));
                        break;
                    case Opcode.Textˉconcat:
                        var Concatˉright = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        var Concatˉleft = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        var Concatˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        Operations.Add(new Nativeˉtextˉconcat(
                            Concatˉresult,
                            Concatˉleft.Value,
                            Concatˉright.Value));
                        Stack.Push(new(Concatˉresult, Nativeˉvalueˉtype.Borrowedˉtext));
                        break;
                    case Opcode.Textˉquote:
                        var Quoteˉvalue = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        var Quoteˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                        Operations.Add(new Nativeˉtextˉquote(Quoteˉresult, Quoteˉvalue.Value));
                        Stack.Push(new(Quoteˉresult, Nativeˉvalueˉtype.Borrowedˉtext));
                        break;
                    case Opcode.Bytesˉslice:
                        var Sliceˉlength = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var Sliceˉoffset = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var Sliceˉbytes = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Sliceˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉbytesˉslice(
                            Sliceˉresult,
                            Sliceˉbytes.Value,
                            Sliceˉoffset.Value,
                            Sliceˉlength.Value));
                        Stack.Push(new(Sliceˉresult, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Bytesˉreadˉu8:
                    case Opcode.Bytesˉreadˉu16ˉlittle:
                    case Opcode.Bytesˉreadˉu32ˉlittle:
                    case Opcode.Bytesˉreadˉi32ˉlittle:
                        var Readˉoffset = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var Readˉbytes = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Readˉkind = Instruction.Opcode switch
                        {
                            Opcode.Bytesˉreadˉu8 => Nativeˉbytesˉreadˉkind.U8,
                            Opcode.Bytesˉreadˉu16ˉlittle => Nativeˉbytesˉreadˉkind.U16ˉlittle,
                            Opcode.Bytesˉreadˉu32ˉlittle => Nativeˉbytesˉreadˉkind.U32ˉlittle,
                            _ => Nativeˉbytesˉreadˉkind.I32ˉlittle,
                        };
                        var Readˉtype = Readˉkind switch
                        {
                            Nativeˉbytesˉreadˉkind.U8 => Nativeˉvalueˉtype.U8,
                            Nativeˉbytesˉreadˉkind.I32ˉlittle => Nativeˉvalueˉtype.I32,
                            _ => Nativeˉvalueˉtype.U32,
                        };
                        var Readˉresult = Newˉvalue(Readˉtype);
                        Operations.Add(new Nativeˉbytesˉread(
                            Readˉresult,
                            Readˉkind,
                            Readˉbytes.Value,
                            Readˉoffset.Value));
                        Stack.Push(new(Readˉresult, Readˉtype));
                        break;
                    case Opcode.Bytesˉconcat:
                        var Concatˉbytesˉright = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Concatˉbytesˉleft = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        var Concatˉbytesˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉbytesˉconcat(
                            Concatˉbytesˉresult,
                            Concatˉbytesˉleft.Value,
                            Concatˉbytesˉright.Value));
                        Stack.Push(new(Concatˉbytesˉresult, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Bytesˉfromˉu32ˉlittle:
                        var Encodedˉu32 = Popˉvalue(Nativeˉvalueˉtype.U32);
                        var Encodedˉu32ˉresult = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                        Operations.Add(new Nativeˉbytesˉfromˉu32ˉlittle(
                            Encodedˉu32ˉresult,
                            Encodedˉu32.Value));
                        Stack.Push(new(Encodedˉu32ˉresult, Nativeˉvalueˉtype.Borrowedˉbytes));
                        break;
                    case Opcode.Recordˉcreate:
                        var Recordˉtypeˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Recordˉtypeˉindex >= (uint)module.Module.Types.Length ||
                            module.Module.Types[Recordˉtypeˉindex] is not Recordˉtypeˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed invalid record metadata during native lowering.");
                        }
                        var Recordˉtype = (Recordˉtypeˉdeclaration)module.Module.Types[Recordˉtypeˉindex];
                        var Recordˉfields = new Nativeˉstackˉvalue[Recordˉtype.Fields.Length];
                        for (var Fieldˉindex = Recordˉfields.Length - 1; Fieldˉindex >= 0; Fieldˉindex--)
                        {
                            Recordˉfields[Fieldˉindex] = Popˉvalue(Toˉnativeˉtype(Recordˉtype.Fields[Fieldˉindex].Type));
                        }
                        var Recordˉresult = Newˉvalue(Nativeˉvalueˉtype.Record);
                        Operations.Add(new Nativeˉrecordˉcreate(
                            Recordˉresult,
                            Recordˉtypeˉindex,
                            Recordˉfields.Select(Field => Field.Value).ToImmutableArray()));
                        Stack.Push(new(Recordˉresult, Nativeˉvalueˉtype.Record, Recordˉtypeˉindex));
                        break;
                    case Opcode.Recordˉfield:
                        var Recordˉsource = Popˉvalue(Nativeˉvalueˉtype.Record);
                        if ((uint)Recordˉsource.Nominalˉtypeˉindex >= (uint)module.Module.Types.Length ||
                            module.Module.Types[Recordˉsource.Nominalˉtypeˉindex] is not Recordˉtypeˉdeclaration)
                        {
                            Fail("WVN2003", "Verified WVB exposed a record value without native type identity.");
                        }
                        var Fieldˉrecord =
                            (Recordˉtypeˉdeclaration)module.Module.Types[Recordˉsource.Nominalˉtypeˉindex];
                        var Recordˉfieldˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Recordˉfieldˉindex >= (uint)Fieldˉrecord.Fields.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid record field during native lowering.");
                        }
                        var Recordˉfieldˉshape = Fieldˉrecord.Fields[Recordˉfieldˉindex].Type;
                        var Recordˉfieldˉtype = Toˉnativeˉtype(Recordˉfieldˉshape);
                        var Recordˉfieldˉresult = Newˉvalue(Recordˉfieldˉtype);
                        Operations.Add(new Nativeˉrecordˉfield(
                            Recordˉfieldˉresult,
                            Recordˉsource.Nominalˉtypeˉindex,
                            Recordˉfieldˉindex,
                            Recordˉsource.Value));
                        Stack.Push(new(
                            Recordˉfieldˉresult,
                            Recordˉfieldˉtype,
                            Recordˉfieldˉshape.Nominalˉtypeˉindex));
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
                            !Isˉsupportedˉnativeˉcapability(module.Module.Capabilities[Capabilityˉindex]))
                        {
                            Fail("WVN2003", "Verified WVB exposed an unsupported native capability call.");
                        }
                        var Capability = module.Module.Capabilities[Capabilityˉindex];
                        switch (Capability.Name)
                        {
                            case Capabilityˉcatalog.CONSOLE_WRITE_LINE:
                                var Consoleˉtext = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                                Operations.Add(new Nativeˉconsoleˉwriteˉline(Consoleˉtext.Value));
                                break;
                            case Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE:
                                var Diagnosticˉtext = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                                Operations.Add(new Nativeˉdiagnosticˉwriteˉline(Diagnosticˉtext.Value));
                                break;
                            case Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT:
                                var Argumentˉcount = Newˉvalue(Nativeˉvalueˉtype.U32);
                                Operations.Add(new Nativeˉprocessˉargumentˉcount(Argumentˉcount));
                                Stack.Push(new(Argumentˉcount, Nativeˉvalueˉtype.U32));
                                break;
                            case Capabilityˉcatalog.PROCESS_ARGUMENT:
                                var Argumentˉindex = Popˉvalue(Nativeˉvalueˉtype.U32);
                                var Argument = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                                Operations.Add(new Nativeˉprocessˉargument(Argument, Argumentˉindex.Value));
                                Stack.Push(new(Argument, Nativeˉvalueˉtype.Borrowedˉtext));
                                break;
                            case Capabilityˉcatalog.FILE_READ_BYTES:
                                var Resourceˉname = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                                var Fileˉbytes = Newˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                                Operations.Add(new Nativeˉfileˉreadˉbytes(Fileˉbytes, Resourceˉname.Value));
                                Stack.Push(new(Fileˉbytes, Nativeˉvalueˉtype.Borrowedˉbytes));
                                break;
                            case Capabilityˉcatalog.FILE_WRITE_BYTES:
                                var Outputˉbytes = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉbytes);
                                var Outputˉname = Popˉvalue(Nativeˉvalueˉtype.Borrowedˉtext);
                                Operations.Add(new Nativeˉfileˉwriteˉbytes(
                                    Outputˉname.Value,
                                    Outputˉbytes.Value));
                                break;
                        }
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
                        var Callˉshape = Calledˉfunction.Returnˉtype;
                        if (Callˉshape.Kind == Valueˉtype.Void)
                        {
                            Operations.Add(new Nativeˉvoidˉcall(
                                Calledˉfunctionˉindex,
                                Arguments.Select(Argument => Argument.Value).ToImmutableArray()));
                        }
                        else
                        {
                            var Callˉtype = Toˉnativeˉtype(Callˉshape);
                            var Callˉresult = Newˉvalue(Callˉtype);
                            Operations.Add(new Nativeˉcall(
                                Callˉresult,
                                Callˉtype,
                                Calledˉfunctionˉindex,
                                Arguments.Select(Argument => Argument.Value).ToImmutableArray()));
                            Stack.Push(new(Callˉresult, Callˉtype, Callˉshape.Nominalˉtypeˉindex));
                        }
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
                        if (function.Declaration.Returnˉtype.Kind == Valueˉtype.Void)
                        {
                            if (Stack.Count != 0)
                            {
                                Fail("WVN2003", "The baseline native subset requires an empty operand stack at return.");
                            }
                            Terminator = new Nativeˉreturnˉvoid();
                            break;
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
                            $"Native function '{function.Declaration.Name}' uses verified opcode " +
                            $"'{Instruction.Opcode}', which the baseline native subset does not support.");
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
            Blockˉvalueˉranges.Add((Firstˉblockˉvalue, Valueˉtypes.Count - Firstˉblockˉvalue));
        }

        var Frozenˉvalueˉtypes = Valueˉtypes.ToImmutable();
        var Valueˉslots = Allocateˉvalueˉslots(Frozenˉvalueˉtypes, Blockˉvalueˉranges);
        var Requiredˉframeˉslots = checked(
            Allˉlocalˉtypes.Length + Valueˉslots.Count + Hiddenˉresultˉslots);
        if (Requiredˉframeˉslots > Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
        {
            Fail(
                "WVN2004",
                $"Native function '{function.Declaration.Name}' requires at least " +
                $"{Requiredˉframeˉslots} physical frame slots for locals, block-scoped typed " +
                $"values, and any hidden result; the limit is {Nativeˉcontract.MAXIMUM_FRAME_SLOTS}.");
        }

        return new(
            function.Declaration.Name,
            Parameterˉtypes,
            Toˉnativeˉtype(function.Declaration.Returnˉtype),
            Localˉtypes,
            Frozenˉvalueˉtypes,
            Valueˉslots.Indices,
            Valueˉslots.Count,
            Blocks.ToImmutable());
    }

    private static Nativeˉfragment Selectˉx64(Nativeˉmodule module, int mainˉfunction)
    {
        if (module.Functions.IsDefaultOrEmpty ||
            (uint)mainˉfunction >= (uint)module.Functions.Length ||
            module.Functions[mainˉfunction] is not { Name: "Main", Blocks.Length: >= 1 } ||
            module.Data.IsDefault ||
            module.Types.IsDefault ||
            module.Requiredˉservices.IsDefault ||
            module.Requiredˉservices.Length > 12 ||
            module.Requiredˉservices.Any(Service => !Enum.IsDefined(Service)) ||
            module.Requiredˉservices.Distinct().Count() != module.Requiredˉservices.Length ||
            !module.Requiredˉservices.SequenceEqual(module.Requiredˉservices.Order()))
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }
        if (module.Types.Any(Type =>
                Type is null ||
                !Seedˉnames.Isˉidentifier(Type.Name) ||
                !Isˉsupportedˉnativeˉnominalˉtype(Type)))
        {
            Fail("WVN2901", "The x86-64 selector received invalid nominal type metadata.");
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
                    Nativeˉutf8ˉdata { Bytes.IsDefault: false } or
                    Nativeˉbytesˉdata { Bytes.IsDefault: false }))
            {
                Fail("WVN2901", "The x86-64 selector received invalid immutable data metadata.");
            }
        }
        var Usesˉtypedˉrecordˉtags = module.Types
            .OfType<Recordˉtypeˉdeclaration>()
            .Any(Record => Record.Fields.Any(Field =>
                Field.Type.Kind is Valueˉtype.Text or Valueˉtype.Bytes));

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
            var Usedˉslots = checked(Allˉlocals.Length + Function.Valueˉslotˉcount);
            var Hasˉhiddenˉresult = Isˉnativeˉdescriptorˉtype(Function.Returnˉtype);
            var Frameˉslots = checked(Usedˉslots + (Hasˉhiddenˉresult ? 1 : 0));
            var Frameˉbytes = checked(Frameˉslots * Nativeˉcontract.VALUE_SLOT_BYTES);
            var Overflowˉpatches = new List<int>();
            var Instructionˉlimitˉpatches = new List<int>();
            var Boundsˉpatches = new List<int>();
            var Byteˉboundsˉpatches = new List<int>();
            var Recordˉarenaˉpatches = new List<int>();
            var Runtimeˉserviceˉpatches = new List<int>();
            var Invalidˉutf8ˉpatches = new List<int>();
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
            if (Hasˉhiddenˉresult)
            {
                if (Isˉmain)
                {
                    // The managed bridge duplicates the result-cell pointer into the
                    // Windows first and System V fourth arguments, which are both RCX.
                    Code.AddRange([0x48, 0x89, 0xC8]); // mov rax, rcx
                }
                Emitˉstoreˉrax(Code, Usedˉslots);
            }
            Code.AddRange([0x31, 0xC0]);
            for (var Offset = 0; Offset < Usedˉslots * Nativeˉcontract.VALUE_SLOT_BYTES; Offset += sizeof(int))
            {
                Emitˉstoreˉeaxˉatˉoffset(Code, Offset);
            }
            for (var Parameter = 0; Parameter < Function.Parameterˉtypes.Length; Parameter++)
            {
                Emitˉstoreˉargument(
                    Code,
                    Parameter,
                    Function.Parameterˉtypes[Parameter],
                    Frameˉbytes);
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
                        case Nativeˉu8ˉconstant Constant:
                            Emitˉconstant(Code, Constant.Value, Valueˉslot(Function, Constant.Result));
                            break;
                        case Nativeˉu32ˉconstant Constant:
                            Emitˉconstant(Code, unchecked((int)Constant.Value), Valueˉslot(Function, Constant.Result));
                            break;
                        case Nativeˉenumˉconstant Constant:
                            Emitˉconstant(Code, Constant.Value, Valueˉslot(Function, Constant.Result));
                            break;
                        case Nativeˉstaticˉtextˉconstant Constant:
                            var Staticˉtext = (Nativeˉutf8ˉdata)module.Data[Constant.Data];
                            Emitˉstaticˉdescriptorˉconstant(
                                Code,
                                Valueˉslot(Function, Constant.Result),
                                Constant.Data,
                                Staticˉtext.Bytes.Length,
                                Dataˉreferences,
                                Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
                            break;
                        case Nativeˉstaticˉbytesˉconstant Constant:
                            var Staticˉbytes = (Nativeˉbytesˉdata)module.Data[Constant.Data];
                            Emitˉstaticˉdescriptorˉconstant(
                                Code,
                                Valueˉslot(Function, Constant.Result),
                                Constant.Data,
                                Staticˉbytes.Bytes.Length,
                                Dataˉreferences,
                                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
                            break;
                        case Nativeˉlocalˉload Load:
                            Emitˉcopy(Code, Load.Local, Valueˉslot(Function, Load.Result), Load.Type);
                            break;
                        case Nativeˉlocalˉstore Store:
                            Emitˉcopy(Code, Valueˉslot(Function, Store.Value), Store.Local, Store.Type);
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
                        case Nativeˉu32ˉbinary Binary:
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Binary.Left));
                            Emitˉloadˉecx(Code, Valueˉslot(Function, Binary.Right));
                            switch (Binary.Kind)
                            {
                                case Nativeˉu32ˉbinaryˉkind.Add:
                                    Code.AddRange([0x01, 0xC8]);
                                    Emitˉunsignedˉoverflowˉbranch(Code, Overflowˉpatches);
                                    break;
                                case Nativeˉu32ˉbinaryˉkind.Subtract:
                                    Code.AddRange([0x29, 0xC8]);
                                    Emitˉunsignedˉoverflowˉbranch(Code, Overflowˉpatches);
                                    break;
                                case Nativeˉu32ˉbinaryˉkind.Multiply:
                                    Code.AddRange([0xF7, 0xE1, 0x85, 0xD2, 0x0F, 0x85]);
                                    Overflowˉpatches.Add(Code.Count);
                                    Addˉi32(Code, 0);
                                    break;
                            }
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Binary.Result));
                            break;
                        case Nativeˉu32ˉcomparison Comparison:
                            Emitˉcomparison(
                                Code,
                                Valueˉslot(Function, Comparison.Left),
                                Valueˉslot(Function, Comparison.Right),
                                Comparison.Kind switch
                                {
                                    Nativeˉu32ˉcomparisonˉkind.Equal => 0x94,
                                    Nativeˉu32ˉcomparisonˉkind.Notˉequal => 0x95,
                                    Nativeˉu32ˉcomparisonˉkind.Less => 0x92,
                                    Nativeˉu32ˉcomparisonˉkind.Lessˉequal => 0x96,
                                    Nativeˉu32ˉcomparisonˉkind.Greater => 0x97,
                                    _ => 0x93,
                                },
                                Valueˉslot(Function, Comparison.Result));
                            break;
                        case Nativeˉu8ˉcomparison Comparison:
                            Emitˉcomparison(
                                Code,
                                Valueˉslot(Function, Comparison.Left),
                                Valueˉslot(Function, Comparison.Right),
                                Comparison.Kind == Nativeˉu8ˉcomparisonˉkind.Equal ? (byte)0x94 : (byte)0x95,
                                Valueˉslot(Function, Comparison.Result));
                            break;
                        case Nativeˉenumˉcomparison Comparison:
                            Emitˉcomparison(
                                Code,
                                Valueˉslot(Function, Comparison.Left),
                                Valueˉslot(Function, Comparison.Right),
                                Comparison.Kind == Nativeˉenumˉcomparisonˉkind.Equal ? (byte)0x94 : (byte)0x95,
                                Valueˉslot(Function, Comparison.Result));
                            break;
                        case Nativeˉu32ˉfromˉu8 Conversion:
                            Emitˉcopy(
                                Code,
                                Valueˉslot(Function, Conversion.Value),
                                Valueˉslot(Function, Conversion.Result),
                                Nativeˉvalueˉtype.U32);
                            break;
                        case Nativeˉbytesˉlength Length:
                            Emitˉloadˉeaxˉatˉfield(
                                Code,
                                Valueˉslot(Function, Length.Bytes),
                                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Length.Result));
                            break;
                        case Nativeˉbytesˉslice Slice:
                            Emitˉbytesˉslice(
                                Code,
                                Function,
                                Slice,
                                Byteˉboundsˉpatches);
                            break;
                        case Nativeˉbytesˉread Read:
                            Emitˉbytesˉread(
                                Code,
                                Function,
                                Read,
                                Byteˉboundsˉpatches);
                            break;
                        case Nativeˉbytesˉconcat Concat:
                            Emitˉbytesˉconcat(
                                Code,
                                Function,
                                Concat,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉbytesˉfromˉu8 Encode:
                            Emitˉbytesˉfromˉu8(
                                Code,
                                Function,
                                Encode,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉbytesˉfromˉu16ˉlittle Encode:
                            Emitˉbytesˉfromˉu16ˉlittle(
                                Code,
                                Function,
                                Encode,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉbytesˉfromˉu32ˉlittle Encode:
                            Emitˉbytesˉfromˉu32ˉlittle(
                                Code,
                                Function,
                                Encode,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉtextˉutf8ˉisˉvalid Validation:
                            Emitˉdescriptorˉserviceˉinput(
                                Code,
                                Valueˉslot(Function, Validation.Bytes),
                                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
                            Emitˉloadˉdescriptorˉoutputˉrcx(
                                Code,
                                Valueˉslot(Function, Validation.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.TEXT_UTF8_IS_VALID_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉtextˉfromˉutf8 Decode:
                            Emitˉdescriptorˉserviceˉinput(
                                Code,
                                Valueˉslot(Function, Decode.Bytes),
                                Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
                            Emitˉloadˉdescriptorˉoutputˉrcx(
                                Code,
                                Valueˉslot(Function, Decode.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.TEXT_UTF8_IS_VALID_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Decode.Result));
                            Code.AddRange([0x85, 0xC0, 0x0F, 0x84]);
                            Invalidˉutf8ˉpatches.Add(Code.Count);
                            Addˉi32(Code, 0);
                            Emitˉcopy(
                                Code,
                                Valueˉslot(Function, Decode.Bytes),
                                Valueˉslot(Function, Decode.Result),
                                Nativeˉvalueˉtype.Borrowedˉtext);
                            break;
                        case Nativeˉtextˉtoˉutf8 Encode:
                            Emitˉcopy(
                                Code,
                                Valueˉslot(Function, Encode.Text),
                                Valueˉslot(Function, Encode.Result),
                                Nativeˉvalueˉtype.Borrowedˉbytes);
                            break;
                        case Nativeˉenumˉname Name:
                            Code.Add(0x41);
                            Code.Add(0xB8);
                            Addˉi32(Code, Name.Type);
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Name.Value));
                            Code.AddRange([0x41, 0x89, 0xC1]);
                            Emitˉloadˉdescriptorˉoutputˉrcx(Code, Valueˉslot(Function, Name.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.ENUM_NAME_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉintegerˉformat Format:
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Format.Value));
                            Code.AddRange([0x41, 0x89, 0xC0]);
                            Emitˉloadˉdescriptorˉoutputˉr9(Code, Valueˉslot(Function, Format.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Format.Kind == Nativeˉintegerˉformatˉkind.I32
                                    ? Nativeˉserviceˉtableˉcontract.I32_FORMAT_POINTER_OFFSET
                                    : Nativeˉserviceˉtableˉcontract.U32_FORMAT_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉtextˉconcat Concat:
                            Emitˉloadˉdescriptorˉoutputˉr8(Code, Valueˉslot(Function, Concat.Left));
                            Emitˉloadˉdescriptorˉoutputˉr9(Code, Valueˉslot(Function, Concat.Right));
                            Emitˉloadˉdescriptorˉoutputˉrcx(Code, Valueˉslot(Function, Concat.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.TEXT_CONCAT_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉtextˉquote Quote:
                            Emitˉloadˉdescriptorˉoutputˉr8(Code, Valueˉslot(Function, Quote.Text));
                            Emitˉloadˉdescriptorˉoutputˉr9(Code, Valueˉslot(Function, Quote.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.TEXT_QUOTE_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉrecordˉcreate Create:
                            Emitˉrecordˉcreate(
                                Code,
                                Function,
                                Create,
                                Usesˉtypedˉrecordˉtags,
                                Recordˉarenaˉpatches);
                            break;
                        case Nativeˉrecordˉfield Field:
                            Emitˉrecordˉfield(
                                Code,
                                Function,
                                Field,
                                Usesˉtypedˉrecordˉtags,
                                Recordˉarenaˉpatches);
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
                            Emitˉdescriptorˉserviceˉinput(
                                Code,
                                Valueˉslot(Function, Write.Text),
                                Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉdiagnosticˉwriteˉline Write:
                            Emitˉdescriptorˉserviceˉinput(
                                Code,
                                Valueˉslot(Function, Write.Text),
                                Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.DIAGNOSTIC_WRITE_LINE_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉprocessˉargumentˉcount Count:
                            Emitˉserviceˉpointer(
                                Code,
                                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_COUNT_POINTER_OFFSET);
                            Code.AddRange([0xFF, 0xD0]);
                            Emitˉstoreˉeax(Code, Valueˉslot(Function, Count.Result));
                            break;
                        case Nativeˉprocessˉargument Argument:
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Argument.Index));
                            Code.AddRange([0x41, 0x89, 0xC0]);
                            Emitˉloadˉdescriptorˉoutputˉr9(Code, Valueˉslot(Function, Argument.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉfileˉreadˉbytes Read:
                            Emitˉdescriptorˉserviceˉinput(
                                Code,
                                Valueˉslot(Function, Read.Resourceˉname),
                                Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
                            Emitˉloadˉdescriptorˉoutputˉrcx(Code, Valueˉslot(Function, Read.Result));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉfileˉwriteˉbytes Write:
                            Emitˉdescriptorˉserviceˉinput(
                                Code,
                                Valueˉslot(Function, Write.Resourceˉname),
                                Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
                            Emitˉdescriptorˉserviceˉbytes(
                                Code,
                                Valueˉslot(Function, Write.Bytes));
                            Emitˉserviceˉcall(
                                Code,
                                Nativeˉserviceˉtableˉcontract.FILE_WRITE_BYTES_POINTER_OFFSET,
                                Runtimeˉserviceˉpatches);
                            break;
                        case Nativeˉcall Call:
                            var Callˉstackˉbytes = Stackˉcallˉbytes(Call.Arguments.Length);
                            for (var Argument = 0;
                                Argument < Math.Min(
                                    Call.Arguments.Length,
                                    Nativeˉcontract.REGISTER_CALL_PARAMETERS);
                                Argument++)
                            {
                                Emitˉloadˉargument(
                                    Code,
                                    Argument,
                                    Valueˉslot(Function, Call.Arguments[Argument]),
                                    module.Functions[Call.Function].Parameterˉtypes[Argument]);
                            }
                            if (Callˉstackˉbytes != 0)
                            {
                                Emitˉframeˉadjustment(Code, subtract: true, Callˉstackˉbytes);
                                for (var Argument = Nativeˉcontract.REGISTER_CALL_PARAMETERS;
                                    Argument < Call.Arguments.Length;
                                    Argument++)
                                {
                                    Emitˉloadˉstackˉargument(
                                        Code,
                                        Argument,
                                        Valueˉslot(Function, Call.Arguments[Argument]),
                                        module.Functions[Call.Function].Parameterˉtypes[Argument],
                                        Callˉstackˉbytes);
                                }
                            }
                            if (Isˉnativeˉdescriptorˉtype(Call.Type))
                            {
                                Emitˉloadˉdescriptorˉoutputˉrax(
                                    Code,
                                    Valueˉslot(Function, Call.Result),
                                    Callˉstackˉbytes);
                            }
                            Code.Add(0xE8);
                            Callˉpatches.Add(new(Code.Count, Call.Function));
                            Addˉi32(Code, 0);
                            if (Callˉstackˉbytes != 0)
                            {
                                Emitˉframeˉadjustment(Code, subtract: false, Callˉstackˉbytes);
                            }
                            Code.AddRange([0x48, 0x89, 0xC2, 0x48, 0xC1, 0xEA, 0x20, 0x48, 0x85, 0xD2, 0x0F, 0x85]);
                            Propagateˉpatches.Add(Code.Count);
                            Addˉi32(Code, 0);
                            if (!Isˉnativeˉdescriptorˉtype(Call.Type))
                            {
                                Emitˉstoreˉeax(Code, Valueˉslot(Function, Call.Result));
                            }
                            break;
                        case Nativeˉvoidˉcall Call:
                            var Voidˉcallˉstackˉbytes = Stackˉcallˉbytes(Call.Arguments.Length);
                            for (var Argument = 0;
                                Argument < Math.Min(
                                    Call.Arguments.Length,
                                    Nativeˉcontract.REGISTER_CALL_PARAMETERS);
                                Argument++)
                            {
                                Emitˉloadˉargument(
                                    Code,
                                    Argument,
                                    Valueˉslot(Function, Call.Arguments[Argument]),
                                    module.Functions[Call.Function].Parameterˉtypes[Argument]);
                            }
                            if (Voidˉcallˉstackˉbytes != 0)
                            {
                                Emitˉframeˉadjustment(Code, subtract: true, Voidˉcallˉstackˉbytes);
                                for (var Argument = Nativeˉcontract.REGISTER_CALL_PARAMETERS;
                                    Argument < Call.Arguments.Length;
                                    Argument++)
                                {
                                    Emitˉloadˉstackˉargument(
                                        Code,
                                        Argument,
                                        Valueˉslot(Function, Call.Arguments[Argument]),
                                        module.Functions[Call.Function].Parameterˉtypes[Argument],
                                        Voidˉcallˉstackˉbytes);
                                }
                            }
                            Code.Add(0xE8);
                            Callˉpatches.Add(new(Code.Count, Call.Function));
                            Addˉi32(Code, 0);
                            if (Voidˉcallˉstackˉbytes != 0)
                            {
                                Emitˉframeˉadjustment(Code, subtract: false, Voidˉcallˉstackˉbytes);
                            }
                            Code.AddRange([0x48, 0x89, 0xC2, 0x48, 0xC1, 0xEA, 0x20, 0x48, 0x85, 0xD2, 0x0F, 0x85]);
                            Propagateˉpatches.Add(Code.Count);
                            Addˉi32(Code, 0);
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
                        if (Isˉnativeˉdescriptorˉtype(Function.Returnˉtype))
                        {
                            Emitˉdescriptorˉfunctionˉreturn(
                                Code,
                                Valueˉslot(Function, Return.Value),
                                Usedˉslots,
                                Frameˉbytes,
                                Isˉmain);
                        }
                        else
                        {
                            Emitˉloadˉeax(Code, Valueˉslot(Function, Return.Value));
                            Emitˉfunctionˉreturn(Code, Frameˉbytes, Isˉmain);
                        }
                        break;
                    case Nativeˉreturnˉvoid:
                        Code.AddRange([0x31, 0xC0]);
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
            var Byteˉboundsˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, BYTE_BOUNDS_STATUS, Isˉmain);
            var Runtimeˉserviceˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, RUNTIME_SERVICE_STATUS, Isˉmain);
            var Invalidˉutf8ˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, 0x0000_0008_0000_0000UL, Isˉmain);
            var Recordˉarenaˉoffset = Code.Count;
            Emitˉstatusˉtrap(Code, Frameˉbytes, RECORD_ARENA_STATUS, Isˉmain);
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
            foreach (var Patchˉoffset in Byteˉboundsˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Byteˉboundsˉoffset);
            }
            foreach (var Patchˉoffset in Runtimeˉserviceˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Runtimeˉserviceˉoffset);
            }
            foreach (var Patchˉoffset in Invalidˉutf8ˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Invalidˉutf8ˉoffset);
            }
            foreach (var Patchˉoffset in Recordˉarenaˉpatches)
            {
                Writeˉrelativeˉi32(Code, Patchˉoffset, Recordˉarenaˉoffset);
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
                    case Nativeˉbytesˉdata Byteˉdata:
                        Code.AddRange(Byteˉdata.Bytes);
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
            Fail(
                "WVN2902",
                $"The selected x86-64 fragment is {Code.Count} bytes; the limit is {Nativeˉcontract.MAXIMUM_CODE_BYTES} bytes.");
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
                    Nativeˉbytesˉdata Byteˉdata => checked((uint)Byteˉdata.Bytes.Length),
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
            module.Types,
            module.Requiredˉservices);
        return Nativeˉfragmentˉverifier.Verify(Fragment);
    }

    private static void Validateˉfunction(Nativeˉmodule module, Nativeˉfunction function)
    {
        if (function.Parameterˉtypes.IsDefault ||
            function.Localˉtypes.IsDefault ||
            function.Valueˉtypes.IsDefault ||
            function.Valueˉslotˉindices.IsDefault ||
            function.Valueˉslotˉindices.Length != function.Valueˉtypes.Length ||
            function.Valueˉtypes.Length > Nativeˉcontract.MAXIMUM_VALUE_IDENTIFIERS ||
            function.Valueˉslotˉcount is < 0 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
            function.Blocks.IsDefaultOrEmpty ||
            function.Blocks.Length > Nativeˉcontract.MAXIMUM_BLOCKS ||
            function.Parameterˉtypes.Length > Nativeˉcontract.MAXIMUM_CALL_PARAMETERS ||
            function.Allˉlocalˉtypes.Length + function.Valueˉslotˉcount +
                (Isˉnativeˉdescriptorˉtype(function.Returnˉtype) ? 1 : 0) is < 0 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
            !Isˉnativeˉreturnˉtype(function.Returnˉtype) ||
            function.Parameterˉtypes.Any(Type =>
                !Isˉnativeˉscalarˉtype(Type) && !Isˉnativeˉdescriptorˉtype(Type)) ||
            function.Localˉtypes.Any(Type => Type is not (
                Nativeˉvalueˉtype.I32 or
                Nativeˉvalueˉtype.Bool or
                Nativeˉvalueˉtype.Borrowedˉtext or
                Nativeˉvalueˉtype.U8 or
                Nativeˉvalueˉtype.U32 or
                Nativeˉvalueˉtype.Borrowedˉbytes or
                Nativeˉvalueˉtype.Enum or
                Nativeˉvalueˉtype.Record)) ||
            function.Valueˉtypes.Any(Type => !Enum.IsDefined(Type) || Type == Nativeˉvalueˉtype.Void))
        {
            Fail("WVN2901", "The x86-64 selector received invalid native function metadata.");
        }

        var Nextˉvalue = 0;
        var Returnˉcount = 0;
        var Blockˉvalueˉranges = new List<(int First, int Count)>();
        for (var Blockˉindex = 0; Blockˉindex < function.Blocks.Length; Blockˉindex++)
        {
            var Firstˉblockˉvalue = Nextˉvalue;
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
                    case Nativeˉu8ˉconstant Constant:
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.U8, ref Nextˉvalue);
                        break;
                    case Nativeˉu32ˉconstant Constant:
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.U32, ref Nextˉvalue);
                        break;
                    case Nativeˉenumˉconstant Constant:
                        if ((uint)Constant.Type >= (uint)module.Types.Length ||
                            module.Types[Constant.Type] is not Enumˉtypeˉdeclaration Enumˉmetadata ||
                            (uint)Constant.Member >= (uint)Enumˉmetadata.Members.Length ||
                            Constant.Value != Enumˉmetadata.Members[Constant.Member].Value)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid enum constant metadata.");
                        }
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.Enum, ref Nextˉvalue);
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
                            Nativeˉvalueˉtype.Borrowedˉtext,
                            ref Nextˉvalue);
                        break;
                    case Nativeˉstaticˉbytesˉconstant Constant:
                        if ((uint)Constant.Data >= (uint)module.Data.Length ||
                            module.Data[Constant.Data] is not Nativeˉbytesˉdata)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid static bytes metadata.");
                        }
                        Requireˉresult(
                            function,
                            Constant.Result,
                            Nativeˉvalueˉtype.Borrowedˉbytes,
                            ref Nextˉvalue);
                        break;
                    case Nativeˉlocalˉload Load:
                        Requireˉlocal(function, Load.Local, Load.Type);
                        Requireˉresult(function, Load.Result, Load.Type, ref Nextˉvalue);
                        break;
                    case Nativeˉlocalˉstore Store:
                        Requireˉlocal(function, Store.Local, Store.Type);
                        Requireˉvalue(function, Store.Value, Store.Type, Firstˉblockˉvalue, Nextˉvalue);
                        break;
                    case Nativeˉi32ˉbinary Binary when Enum.IsDefined(Binary.Kind):
                        Requireˉvalue(function, Binary.Left, Nativeˉvalueˉtype.I32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Binary.Right, Nativeˉvalueˉtype.I32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Binary.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉi32ˉnegate Negate:
                        Requireˉvalue(function, Negate.Value, Nativeˉvalueˉtype.I32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Negate.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉi32ˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.I32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.I32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉboolˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.Bool, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.Bool, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉboolˉnot Not:
                        Requireˉvalue(function, Not.Value, Nativeˉvalueˉtype.Bool, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Not.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉu32ˉbinary Binary when Enum.IsDefined(Binary.Kind):
                        Requireˉvalue(function, Binary.Left, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Binary.Right, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Binary.Result, Nativeˉvalueˉtype.U32, ref Nextˉvalue);
                        break;
                    case Nativeˉu32ˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉu8ˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.U8, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.U8, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉenumˉcomparison Comparison when Enum.IsDefined(Comparison.Kind):
                        Requireˉvalue(function, Comparison.Left, Nativeˉvalueˉtype.Enum, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Comparison.Right, Nativeˉvalueˉtype.Enum, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Comparison.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉu32ˉfromˉu8 Conversion:
                        Requireˉvalue(function, Conversion.Value, Nativeˉvalueˉtype.U8, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Conversion.Result, Nativeˉvalueˉtype.U32, ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉlength Length:
                        Requireˉvalue(function, Length.Bytes, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Length.Result, Nativeˉvalueˉtype.U32, ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉslice Slice:
                        Requireˉvalue(function, Slice.Bytes, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Slice.Offset, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Slice.Length, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Slice.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉread Read when Enum.IsDefined(Read.Kind):
                        Requireˉvalue(function, Read.Bytes, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Read.Offset, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(
                            function,
                            Read.Result,
                            Read.Kind switch
                            {
                                Nativeˉbytesˉreadˉkind.U8 => Nativeˉvalueˉtype.U8,
                                Nativeˉbytesˉreadˉkind.I32ˉlittle => Nativeˉvalueˉtype.I32,
                                _ => Nativeˉvalueˉtype.U32,
                            },
                            ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉconcat Concat:
                        Requireˉvalue(function, Concat.Left, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Concat.Right, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Concat.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉfromˉu8 Encode:
                        Requireˉvalue(function, Encode.Value, Nativeˉvalueˉtype.U8, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Encode.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉfromˉu16ˉlittle Encode:
                        Requireˉvalue(function, Encode.Value, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Encode.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉbytesˉfromˉu32ˉlittle Encode:
                        Requireˉvalue(function, Encode.Value, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Encode.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉtextˉutf8ˉisˉvalid Validation:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Textˉutf8ˉisˉvalid))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid UTF-8 validation metadata.");
                        }
                        Requireˉvalue(function, Validation.Bytes, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Validation.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉtextˉfromˉutf8 Decode:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Textˉutf8ˉisˉvalid))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid UTF-8 decoding metadata.");
                        }
                        Requireˉvalue(function, Decode.Bytes, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Decode.Result, Nativeˉvalueˉtype.Borrowedˉtext, ref Nextˉvalue);
                        break;
                    case Nativeˉtextˉtoˉutf8 Encode:
                        Requireˉvalue(function, Encode.Text, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Encode.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉenumˉname Name:
                        if ((uint)Name.Type >= (uint)module.Types.Length ||
                            module.Types[Name.Type] is not Enumˉtypeˉdeclaration ||
                            !module.Requiredˉservices.Contains(Nativeˉservice.Enumˉname))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid enum-name metadata.");
                        }
                        Requireˉvalue(function, Name.Value, Nativeˉvalueˉtype.Enum, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Name.Result, Nativeˉvalueˉtype.Borrowedˉtext, ref Nextˉvalue);
                        break;
                    case Nativeˉintegerˉformat Format when Enum.IsDefined(Format.Kind):
                        var Formatˉtype = Format.Kind switch
                        {
                            Nativeˉintegerˉformatˉkind.I32 => Nativeˉvalueˉtype.I32,
                            Nativeˉintegerˉformatˉkind.U8 => Nativeˉvalueˉtype.U8,
                            _ => Nativeˉvalueˉtype.U32,
                        };
                        var Formatˉservice = Format.Kind == Nativeˉintegerˉformatˉkind.I32
                            ? Nativeˉservice.I32ˉformat
                            : Nativeˉservice.U32ˉformat;
                        if (!module.Requiredˉservices.Contains(Formatˉservice))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid integer-format metadata.");
                        }
                        Requireˉvalue(function, Format.Value, Formatˉtype, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Format.Result, Nativeˉvalueˉtype.Borrowedˉtext, ref Nextˉvalue);
                        break;
                    case Nativeˉtextˉconcat Concat:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Textˉconcat))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid text-concat metadata.");
                        }
                        Requireˉvalue(function, Concat.Left, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Concat.Right, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Concat.Result, Nativeˉvalueˉtype.Borrowedˉtext, ref Nextˉvalue);
                        break;
                    case Nativeˉtextˉquote Quote:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Textˉquote))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid text-quote metadata.");
                        }
                        Requireˉvalue(function, Quote.Text, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Quote.Result, Nativeˉvalueˉtype.Borrowedˉtext, ref Nextˉvalue);
                        break;
                    case Nativeˉrecordˉcreate Create:
                        if ((uint)Create.Type >= (uint)module.Types.Length ||
                            module.Types[Create.Type] is not Recordˉtypeˉdeclaration)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid record construction metadata.");
                        }
                        var Record = (Recordˉtypeˉdeclaration)module.Types[Create.Type];
                        if (Create.Fields.IsDefault || Create.Fields.Length != Record.Fields.Length)
                        {
                            Fail("WVN2901", "The x86-64 selector received noncanonical record fields.");
                        }
                        for (var Field = 0; Field < Create.Fields.Length; Field++)
                        {
                            Requireˉvalue(
                                function,
                                Create.Fields[Field],
                                Toˉnativeˉtype(Record.Fields[Field].Type),
                                Firstˉblockˉvalue,
                                Nextˉvalue);
                        }
                        Requireˉresult(function, Create.Result, Nativeˉvalueˉtype.Record, ref Nextˉvalue);
                        break;
                    case Nativeˉrecordˉfield Field:
                        if ((uint)Field.Type >= (uint)module.Types.Length ||
                            module.Types[Field.Type] is not Recordˉtypeˉdeclaration)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid record field metadata.");
                        }
                        var Fieldˉrecord = (Recordˉtypeˉdeclaration)module.Types[Field.Type];
                        if ((uint)Field.Field >= (uint)Fieldˉrecord.Fields.Length)
                        {
                            Fail("WVN2901", "The x86-64 selector received an invalid record field index.");
                        }
                        Requireˉvalue(function, Field.Record, Nativeˉvalueˉtype.Record, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(
                            function,
                            Field.Result,
                            Toˉnativeˉtype(Fieldˉrecord.Fields[Field.Field].Type),
                            ref Nextˉvalue);
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
                        Requireˉvalue(function, Load.Index, Nativeˉvalueˉtype.I32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Load.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉconsoleˉwriteˉline Write:
                        Requireˉvalue(function, Write.Text, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Consoleˉwriteˉline))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid console.write_line metadata.");
                        }
                        break;
                    case Nativeˉdiagnosticˉwriteˉline Write:
                        Requireˉvalue(function, Write.Text, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Diagnosticˉwriteˉline))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid diagnostic.write_line metadata.");
                        }
                        break;
                    case Nativeˉprocessˉargumentˉcount Count:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Processˉargumentˉcount))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid process.argument_count metadata.");
                        }
                        Requireˉresult(function, Count.Result, Nativeˉvalueˉtype.U32, ref Nextˉvalue);
                        break;
                    case Nativeˉprocessˉargument Argument:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Processˉargument))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid process.argument metadata.");
                        }
                        Requireˉvalue(function, Argument.Index, Nativeˉvalueˉtype.U32, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Argument.Result, Nativeˉvalueˉtype.Borrowedˉtext, ref Nextˉvalue);
                        break;
                    case Nativeˉfileˉreadˉbytes Read:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Fileˉreadˉbytes))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid file.read_bytes metadata.");
                        }
                        Requireˉvalue(function, Read.Resourceˉname, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉresult(function, Read.Result, Nativeˉvalueˉtype.Borrowedˉbytes, ref Nextˉvalue);
                        break;
                    case Nativeˉfileˉwriteˉbytes Write:
                        if (!module.Requiredˉservices.Contains(Nativeˉservice.Fileˉwriteˉbytes))
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid file.write_bytes metadata.");
                        }
                        Requireˉvalue(function, Write.Resourceˉname, Nativeˉvalueˉtype.Borrowedˉtext, Firstˉblockˉvalue, Nextˉvalue);
                        Requireˉvalue(function, Write.Bytes, Nativeˉvalueˉtype.Borrowedˉbytes, Firstˉblockˉvalue, Nextˉvalue);
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
                                Firstˉblockˉvalue,
                                Nextˉvalue);
                        }
                        Requireˉresult(function, Call.Result, Call.Type, ref Nextˉvalue);
                        break;
                    case Nativeˉvoidˉcall Call:
                        if ((uint)Call.Function >= (uint)module.Functions.Length ||
                            Call.Arguments.IsDefault ||
                            module.Functions[Call.Function].Returnˉtype != Nativeˉvalueˉtype.Void ||
                            Call.Arguments.Length != module.Functions[Call.Function].Parameterˉtypes.Length)
                        {
                            Fail("WVN2901", "The x86-64 selector received invalid native void-call metadata.");
                        }
                        for (var Argument = 0; Argument < Call.Arguments.Length; Argument++)
                        {
                            Requireˉvalue(
                                function,
                                Call.Arguments[Argument],
                                module.Functions[Call.Function].Parameterˉtypes[Argument],
                                Firstˉblockˉvalue,
                                Nextˉvalue);
                        }
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
                    Requireˉvalue(function, Branch.Condition, Nativeˉvalueˉtype.Bool, Firstˉblockˉvalue, Nextˉvalue);
                    Requireˉtarget(function, Branch.Trueˉblock);
                    Requireˉtarget(function, Branch.Falseˉblock);
                    break;
                case Nativeˉreturn Return:
                    if (!Chargeˉpending)
                    {
                        Fail("WVN2901", "A native return must consume one instruction charge.");
                    }
                    Requireˉvalue(function, Return.Value, function.Returnˉtype, Firstˉblockˉvalue, Nextˉvalue);
                    Returnˉcount++;
                    break;
                case Nativeˉreturnˉvoid:
                    if (!Chargeˉpending || function.Returnˉtype != Nativeˉvalueˉtype.Void)
                    {
                        Fail("WVN2901", "A native void return must consume one instruction charge in a void function.");
                    }
                    Returnˉcount++;
                    break;
                default:
                    Fail("WVN2901", "The x86-64 selector received an invalid native terminator.");
                    break;
            }
            var Blockˉvalueˉcount = Nextˉvalue - Firstˉblockˉvalue;
            Blockˉvalueˉranges.Add((Firstˉblockˉvalue, Blockˉvalueˉcount));
        }
        var Expectedˉvalueˉslots = Allocateˉvalueˉslots(function.Valueˉtypes, Blockˉvalueˉranges);
        if (Nextˉvalue != function.Valueˉtypes.Length ||
            Expectedˉvalueˉslots.Count != function.Valueˉslotˉcount ||
            !Expectedˉvalueˉslots.Indices.SequenceEqual(function.Valueˉslotˉindices) ||
            Returnˉcount == 0)
        {
            Fail("WVN2901", "The x86-64 selector requires a complete canonical block-scoped value graph with a return.");
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

    private static (ImmutableArray<int> Indices, int Count) Allocateˉvalueˉslots(
        IReadOnlyList<Nativeˉvalueˉtype> valueˉtypes,
        IReadOnlyList<(int First, int Count)> blockˉranges)
    {
        var Maximumˉcounts = new int[(int)Nativeˉvalueˉtype.Record + 1];
        foreach (var (First, Count) in blockˉranges)
        {
            var Counts = new int[Maximumˉcounts.Length];
            for (var Value = First; Value < First + Count; Value++)
            {
                var Type = (int)valueˉtypes[Value];
                Counts[Type]++;
                Maximumˉcounts[Type] = Math.Max(Maximumˉcounts[Type], Counts[Type]);
            }
        }

        var Typeˉoffsets = new int[Maximumˉcounts.Length];
        var Slotˉcount = 0;
        for (var Type = (int)Nativeˉvalueˉtype.I32; Type < Maximumˉcounts.Length; Type++)
        {
            Typeˉoffsets[Type] = Slotˉcount;
            Slotˉcount = checked(Slotˉcount + Maximumˉcounts[Type]);
        }

        var Indices = new int[valueˉtypes.Count];
        foreach (var (First, Count) in blockˉranges)
        {
            var Counts = new int[Maximumˉcounts.Length];
            for (var Value = First; Value < First + Count; Value++)
            {
                var Type = (int)valueˉtypes[Value];
                Indices[Value] = Typeˉoffsets[Type] + Counts[Type]++;
            }
        }
        return (Indices.ToImmutableArray(), Slotˉcount);
    }

    private static bool Isˉsupportedˉnativeˉcapability(Capabilityˉdeclaration capability) =>
        capability.Name switch
        {
            Capabilityˉcatalog.CONSOLE_WRITE_LINE =>
                capability.Parameterˉtypes.SequenceEqual([Valueˉtype.Text]) &&
                capability.Returnˉtype == Valueˉtype.Void,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT =>
                capability.Parameterˉtypes.IsEmpty &&
                capability.Returnˉtype == Valueˉtype.U32,
            Capabilityˉcatalog.PROCESS_ARGUMENT =>
                capability.Parameterˉtypes.SequenceEqual([Valueˉtype.U32]) &&
                capability.Returnˉtype == Valueˉtype.Text,
            Capabilityˉcatalog.FILE_READ_BYTES =>
                capability.Parameterˉtypes.SequenceEqual([Valueˉtype.Text]) &&
                capability.Returnˉtype == Valueˉtype.Bytes,
            Capabilityˉcatalog.FILE_WRITE_BYTES =>
                capability.Parameterˉtypes.SequenceEqual([Valueˉtype.Text, Valueˉtype.Bytes]) &&
                capability.Returnˉtype == Valueˉtype.Void,
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE =>
                capability.Parameterˉtypes.SequenceEqual([Valueˉtype.Text]) &&
                capability.Returnˉtype == Valueˉtype.Void,
            _ => false,
        };

    private static bool Isˉsupportedˉnativeˉnominalˉtype(Nominalˉtypeˉdeclaration type) =>
        type switch
        {
            Enumˉtypeˉdeclaration Enum =>
                !Enum.Members.IsDefaultOrEmpty && Enum.Members[0].Value == 0,
            Recordˉtypeˉdeclaration Record =>
                !Record.Fields.IsDefaultOrEmpty &&
                Record.Fields.All(Field => Field.Type.Kind is
                    Valueˉtype.I32 or
                    Valueˉtype.Bool or
                    Valueˉtype.U8 or
                    Valueˉtype.U32 or
                    Valueˉtype.Text or
                    Valueˉtype.Bytes or
                    Valueˉtype.Record or
                    Valueˉtype.Enum),
            _ => false,
        };

    private static bool Isˉnativeˉreturnˉtype(Valueˉshape type) =>
        type.Kind switch
        {
            Valueˉtype.I32 or Valueˉtype.Bool or Valueˉtype.U8 or Valueˉtype.U32 =>
                type.Nominalˉtypeˉindex == -1,
            Valueˉtype.Record or Valueˉtype.Enum => type.Nominalˉtypeˉindex >= 0,
            Valueˉtype.Text or Valueˉtype.Bytes or Valueˉtype.Void => type.Nominalˉtypeˉindex == -1,
            _ => false,
        };

    private static bool Isˉnativeˉparameterˉtype(Valueˉshape type) =>
        Isˉnativeˉreturnˉtype(type) ||
        type.Nominalˉtypeˉindex == -1 && type.Kind is Valueˉtype.Text or Valueˉtype.Bytes;

    private static bool Isˉnativeˉlocalˉtype(Valueˉshape type) =>
        type.Kind switch
        {
            Valueˉtype.I32 or Valueˉtype.Bool or Valueˉtype.Text or Valueˉtype.U8 or
                Valueˉtype.U32 or Valueˉtype.Bytes => type.Nominalˉtypeˉindex == -1,
            Valueˉtype.Record or Valueˉtype.Enum => type.Nominalˉtypeˉindex >= 0,
            _ => false,
        };

    private static bool Isˉnativeˉscalarˉtype(Nativeˉvalueˉtype type) =>
        type is Nativeˉvalueˉtype.I32 or
            Nativeˉvalueˉtype.Bool or
            Nativeˉvalueˉtype.U8 or
            Nativeˉvalueˉtype.U32 or
            Nativeˉvalueˉtype.Enum or
            Nativeˉvalueˉtype.Record;

    private static bool Isˉnativeˉreturnˉtype(Nativeˉvalueˉtype type) =>
        Isˉnativeˉscalarˉtype(type) || Isˉnativeˉdescriptorˉtype(type) || type == Nativeˉvalueˉtype.Void;

    private static Nativeˉvalueˉtype Toˉnativeˉtype(Valueˉshape type) =>
        type.Kind switch
        {
            Valueˉtype.I32 when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.I32,
            Valueˉtype.Bool when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Bool,
            Valueˉtype.Text when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Borrowedˉtext,
            Valueˉtype.U8 when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.U8,
            Valueˉtype.U32 when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.U32,
            Valueˉtype.Bytes when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Borrowedˉbytes,
            Valueˉtype.Enum when type.Nominalˉtypeˉindex >= 0 => Nativeˉvalueˉtype.Enum,
            Valueˉtype.Record when type.Nominalˉtypeˉindex >= 0 => Nativeˉvalueˉtype.Record,
            Valueˉtype.Void when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Void,
            _ => throw new Nativeˉbackendˉexception("WVN2002", $"Unsupported native local type '{type}'."),
        };

    private static bool Isˉnativeˉdescriptorˉtype(Nativeˉvalueˉtype type) =>
        type is Nativeˉvalueˉtype.Borrowedˉtext or Nativeˉvalueˉtype.Borrowedˉbytes;

    private static bool Isˉnativeˉrunˉreferenceˉtype(Nativeˉvalueˉtype type) =>
        Isˉnativeˉdescriptorˉtype(type) || type == Nativeˉvalueˉtype.Record;

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
        int firstˉavailable,
        int available)
    {
        if (value < firstˉavailable || (uint)value >= (uint)available || function.Valueˉtypes[value] != type)
        {
            Fail("WVN2901", "The x86-64 selector received an invalid block-local typed value reference.");
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
        checked(function.Allˉlocalˉtypes.Length + function.Valueˉslotˉindices[value]);

    private static void Emitˉconstant(List<byte> code, int value, int targetˉslot)
    {
        code.Add(0xB8);
        Addˉi32(code, value);
        Emitˉstoreˉeax(code, targetˉslot);
    }

    private static void Emitˉstaticˉdescriptorˉconstant(
        List<byte> code,
        int targetˉslot,
        int data,
        int length,
        List<Nativeˉdataˉreference> references,
        int lengthˉoffset)
    {
        code.AddRange([0x48, 0x8D, 0x05]);
        references.Add(new(code.Count, data));
        Addˉi32(code, 0);
        Emitˉstoreˉrax(code, targetˉslot);
        code.Add(0xB8);
        Addˉi32(code, length);
        Emitˉstoreˉeaxˉatˉfield(code, targetˉslot, lengthˉoffset);
    }

    private static void Emitˉcopy(
        List<byte> code,
        int sourceˉslot,
        int targetˉslot,
        Nativeˉvalueˉtype type)
    {
        if (Isˉnativeˉdescriptorˉtype(type))
        {
            Emitˉloadˉrax(code, sourceˉslot);
            Emitˉstoreˉrax(code, targetˉslot);
            var Lengthˉoffset = type == Nativeˉvalueˉtype.Borrowedˉtext
                ? Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET
                : Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET;
            Emitˉloadˉeaxˉatˉfield(code, sourceˉslot, Lengthˉoffset);
            Emitˉstoreˉeaxˉatˉfield(code, targetˉslot, Lengthˉoffset);
            return;
        }

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

    private static void Emitˉbytesˉslice(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉbytesˉslice slice,
        List<int> boundsˉpatches)
    {
        var Bytesˉslot = Valueˉslot(function, slice.Bytes);
        var Offsetˉslot = Valueˉslot(function, slice.Offset);
        var Lengthˉslot = Valueˉslot(function, slice.Length);
        var Resultˉslot = Valueˉslot(function, slice.Result);

        Emitˉloadˉeax(code, Offsetˉslot);
        Emitˉloadˉecxˉatˉfield(code, Bytesˉslot, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        code.AddRange([0x39, 0xC8, 0x0F, 0x87]);
        boundsˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange([0x29, 0xC1]);
        Emitˉloadˉedx(code, Lengthˉslot);
        code.AddRange([0x39, 0xCA, 0x0F, 0x87]);
        boundsˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        Emitˉloadˉrax(code, Bytesˉslot);
        Emitˉloadˉecx(code, Offsetˉslot);
        code.AddRange([0x48, 0x01, 0xC8]);
        Emitˉstoreˉrax(code, Resultˉslot);
        Emitˉloadˉeax(code, Lengthˉslot);
        Emitˉstoreˉeaxˉatˉfield(code, Resultˉslot, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
    }

    private static void Emitˉbytesˉread(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉbytesˉread read,
        List<int> boundsˉpatches)
    {
        var Bytesˉslot = Valueˉslot(function, read.Bytes);
        var Offsetˉslot = Valueˉslot(function, read.Offset);
        Emitˉloadˉeax(code, Offsetˉslot);
        Emitˉloadˉecxˉatˉfield(code, Bytesˉslot, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        code.AddRange([0x39, 0xC8, 0x0F, 0x87]);
        boundsˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange([0x29, 0xC1, 0x83, 0xF9, read.Kind switch
        {
            Nativeˉbytesˉreadˉkind.U8 => (byte)sizeof(byte),
            Nativeˉbytesˉreadˉkind.U16ˉlittle => (byte)sizeof(ushort),
            _ => (byte)sizeof(uint),
        }, 0x0F, 0x82]);
        boundsˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        Emitˉloadˉrdx(code, Bytesˉslot);
        Emitˉloadˉeax(code, Offsetˉslot);
        code.AddRange([0x48, 0x01, 0xC2]);
        code.AddRange(read.Kind switch
        {
            Nativeˉbytesˉreadˉkind.U8 => [0x0F, 0xB6, 0x02],
            Nativeˉbytesˉreadˉkind.U16ˉlittle => [0x0F, 0xB7, 0x02],
            _ => [0x8B, 0x02],
        });
        Emitˉstoreˉeax(code, Valueˉslot(function, read.Result));
    }

    private static void Emitˉbytesˉconcat(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉbytesˉconcat concat,
        List<int> runtimeˉserviceˉpatches)
    {
        var Limitˉpatches = new List<int>();
        var Arenaˉpatches = new List<int>();
        Emitˉloadˉeaxˉatˉfield(
            code,
            Valueˉslot(function, concat.Left),
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        Emitˉloadˉecxˉatˉfield(
            code,
            Valueˉslot(function, concat.Right),
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        code.AddRange([0x01, 0xC8, 0x0F, 0x82]);
        Limitˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.Add(0x3D);
        Addˉi32(code, Bytecodeˉlimits.MAX_BYTE_DATA_BYTES);
        code.AddRange([0x0F, 0x87]);
        Limitˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x89, 0xC0,
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x41, 0x89, 0xC1,
            0x89, 0xC1,
            0x44, 0x01, 0xC1,
            0x0F, 0x82,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            0x0F, 0x87,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
            0x4C, 0x01, 0xCA,
            0x48, 0x89, 0xD0,
        ]);
        Emitˉstoreˉrax(code, Valueˉslot(function, concat.Result));
        code.AddRange([0x44, 0x89, 0xC0]);
        Emitˉstoreˉeaxˉatˉfield(
            code,
            Valueˉslot(function, concat.Result),
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);

        code.AddRange([0x49, 0x89, 0xD1]);
        Emitˉbytesˉcopyˉloop(
            code,
            Valueˉslot(function, concat.Left));
        Emitˉbytesˉcopyˉloop(
            code,
            Valueˉslot(function, concat.Right));

        code.Add(0xE9);
        var Endˉpatch = code.Count;
        Addˉi32(code, 0);
        var Limitˉfailure = code.Count;
        Emitˉruntimeˉfailure(
            code,
            Nativeˉserviceˉfailureˉdetail.Bytesˉvalueˉlimit,
            runtimeˉserviceˉpatches);
        var Arenaˉfailure = code.Count;
        Emitˉruntimeˉfailure(
            code,
            Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
            runtimeˉserviceˉpatches);
        var End = code.Count;
        foreach (var Patch in Limitˉpatches)
        {
            Writeˉrelativeˉi32(code, Patch, Limitˉfailure);
        }
        foreach (var Patch in Arenaˉpatches)
        {
            Writeˉrelativeˉi32(code, Patch, Arenaˉfailure);
        }
        Writeˉrelativeˉi32(code, Endˉpatch, End);
    }

    private static void Emitˉbytesˉfromˉu32ˉlittle(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉbytesˉfromˉu32ˉlittle encode,
        List<int> runtimeˉserviceˉpatches)
    {
        var Arenaˉpatches = new List<int>();
        code.AddRange(
        [
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x41, 0x89, 0xC1,
            0x89, 0xC1,
            0x83, 0xC1, 0x04,
            0x0F, 0x82,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            0x0F, 0x87,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
            0x4C, 0x01, 0xCA,
            0x48, 0x89, 0xD0,
        ]);
        Emitˉstoreˉrax(code, Valueˉslot(function, encode.Result));
        code.Add(0xB8);
        Addˉi32(code, sizeof(uint));
        Emitˉstoreˉeaxˉatˉfield(
            code,
            Valueˉslot(function, encode.Result),
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        Emitˉloadˉeax(code, Valueˉslot(function, encode.Value));
        code.AddRange([0x89, 0x02, 0xE9]);
        var Endˉpatch = code.Count;
        Addˉi32(code, 0);
        var Arenaˉfailure = code.Count;
        Emitˉruntimeˉfailure(
            code,
            Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
            runtimeˉserviceˉpatches);
        var End = code.Count;
        foreach (var Patch in Arenaˉpatches)
        {
            Writeˉrelativeˉi32(code, Patch, Arenaˉfailure);
        }
        Writeˉrelativeˉi32(code, Endˉpatch, End);
    }

    private static void Emitˉbytesˉfromˉu16ˉlittle(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉbytesˉfromˉu16ˉlittle encode,
        List<int> runtimeˉserviceˉpatches)
    {
        var Rangeˉpatches = new List<int>();
        var Arenaˉpatches = new List<int>();
        Emitˉloadˉeax(code, Valueˉslot(function, encode.Value));
        code.Add(0x3D); // cmp eax, 65535
        Addˉi32(code, ushort.MaxValue);
        code.AddRange([0x0F, 0x87]); // ja range failure
        Rangeˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x41, 0x89, 0xC1,
            0x89, 0xC1,
            0x83, 0xC1, 0x02,
            0x0F, 0x82,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            0x0F, 0x87,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
            0x4C, 0x01, 0xCA,
            0x48, 0x89, 0xD0,
        ]);
        Emitˉstoreˉrax(code, Valueˉslot(function, encode.Result));
        code.Add(0xB8);
        Addˉi32(code, sizeof(ushort));
        Emitˉstoreˉeaxˉatˉfield(
            code,
            Valueˉslot(function, encode.Result),
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        Emitˉloadˉeax(code, Valueˉslot(function, encode.Value));
        code.AddRange([0x66, 0x89, 0x02, 0xE9]);
        var Endˉpatch = code.Count;
        Addˉi32(code, 0);
        var Rangeˉfailure = code.Count;
        Emitˉruntimeˉfailure(
            code,
            Nativeˉserviceˉfailureˉdetail.Bytesˉu16ˉoutˉofˉrange,
            runtimeˉserviceˉpatches);
        var Arenaˉfailure = code.Count;
        Emitˉruntimeˉfailure(
            code,
            Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
            runtimeˉserviceˉpatches);
        var End = code.Count;
        foreach (var Patch in Rangeˉpatches)
        {
            Writeˉrelativeˉi32(code, Patch, Rangeˉfailure);
        }
        foreach (var Patch in Arenaˉpatches)
        {
            Writeˉrelativeˉi32(code, Patch, Arenaˉfailure);
        }
        Writeˉrelativeˉi32(code, Endˉpatch, End);
    }

    private static void Emitˉbytesˉfromˉu8(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉbytesˉfromˉu8 encode,
        List<int> runtimeˉserviceˉpatches)
    {
        var Arenaˉpatches = new List<int>();
        code.AddRange(
        [
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x41, 0x89, 0xC1,
            0x89, 0xC1,
            0x83, 0xC1, 0x01,
            0x0F, 0x82,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET,
            0x0F, 0x87,
        ]);
        Arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET,
            0x4C, 0x01, 0xCA,
            0x48, 0x89, 0xD0,
        ]);
        Emitˉstoreˉrax(code, Valueˉslot(function, encode.Result));
        code.Add(0xB8);
        Addˉi32(code, sizeof(byte));
        Emitˉstoreˉeaxˉatˉfield(
            code,
            Valueˉslot(function, encode.Result),
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        Emitˉloadˉeax(code, Valueˉslot(function, encode.Value));
        code.AddRange([0x88, 0x02, 0xE9]);
        var Endˉpatch = code.Count;
        Addˉi32(code, 0);
        var Arenaˉfailure = code.Count;
        Emitˉruntimeˉfailure(
            code,
            Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted,
            runtimeˉserviceˉpatches);
        var End = code.Count;
        foreach (var Patch in Arenaˉpatches)
        {
            Writeˉrelativeˉi32(code, Patch, Arenaˉfailure);
        }
        Writeˉrelativeˉi32(code, Endˉpatch, End);
    }

    private static void Emitˉbytesˉcopyˉloop(List<byte> code, int sourceˉslot)
    {
        Emitˉloadˉrax(code, sourceˉslot);
        Emitˉloadˉecxˉatˉfield(
            code,
            sourceˉslot,
            Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        code.AddRange([0x85, 0xC9, 0x0F, 0x84]);
        var Endˉpatch = code.Count;
        Addˉi32(code, 0);
        var Loop = code.Count;
        code.AddRange(
        [
            0x44, 0x0F, 0xB6, 0x00,
            0x45, 0x88, 0x01,
            0x48, 0xFF, 0xC0,
            0x49, 0xFF, 0xC1,
            0xFF, 0xC9,
            0x0F, 0x85,
        ]);
        var Loopˉpatch = code.Count;
        Addˉi32(code, 0);
        var End = code.Count;
        Writeˉrelativeˉi32(code, Endˉpatch, End);
        Writeˉrelativeˉi32(code, Loopˉpatch, Loop);
    }

    private static void Emitˉruntimeˉfailure(
        List<byte> code,
        Nativeˉserviceˉfailureˉdetail detail,
        List<int> runtimeˉserviceˉpatches)
    {
        code.AddRange(
        [
            0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
        ]);
        Addˉi32(code, checked((int)detail));
        code.Add(0xE9);
        runtimeˉserviceˉpatches.Add(code.Count);
        Addˉi32(code, 0);
    }

    private static void Emitˉrecordˉcreate(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉrecordˉcreate create,
        bool emitˉtypeˉtag,
        List<int> arenaˉpatches)
    {
        if (emitˉtypeˉtag)
        {
            code.AddRange([0x41, 0xB8]);
            Addˉi32(code, create.Type);
        }
        var Allocationˉbytes = checked(create.Fields.Length * Nativeˉcontract.VALUE_SLOT_BYTES);
        code.AddRange(
        [
            0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
            0x89, 0xC1,
            0x81, 0xC1,
        ]);
        Addˉi32(code, Allocationˉbytes);
        code.AddRange([0x0F, 0x82]);
        arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET,
            0x0F, 0x87,
        ]);
        arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
        ]);
        Emitˉstoreˉeax(code, Valueˉslot(function, create.Result));
        code.AddRange(
        [
            0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET,
            0x48, 0x01, 0xC2,
        ]);
        for (var Field = 0; Field < create.Fields.Length; Field++)
        {
            var Sourceˉslot = Valueˉslot(function, create.Fields[Field]);
            var Targetˉoffset = checked(Field * Nativeˉcontract.VALUE_SLOT_BYTES);
            Emitˉloadˉrax(code, Sourceˉslot);
            code.AddRange([0x48, 0x89, 0x82]);
            Addˉi32(code, Targetˉoffset);
            Emitˉloadˉraxˉatˉfield(code, Sourceˉslot, sizeof(ulong));
            code.AddRange([0x48, 0x89, 0x82]);
            Addˉi32(code, checked(Targetˉoffset + sizeof(ulong)));
        }
    }

    private static void Emitˉrecordˉfield(
        List<byte> code,
        Nativeˉfunction function,
        Nativeˉrecordˉfield field,
        bool emitˉtypeˉtag,
        List<int> arenaˉpatches)
    {
        if (emitˉtypeˉtag)
        {
            code.AddRange([0x41, 0xB8]);
            Addˉi32(code, field.Type);
        }
        Emitˉloadˉeax(code, Valueˉslot(function, field.Record));
        code.AddRange([0x89, 0xC1, 0x81, 0xC1]);
        Addˉi32(code, checked((field.Field + 1) * Nativeˉcontract.VALUE_SLOT_BYTES));
        code.AddRange([0x0F, 0x82]);
        arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET,
            0x0F, 0x87,
        ]);
        arenaˉpatches.Add(code.Count);
        Addˉi32(code, 0);
        code.AddRange(
        [
            0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET,
            0x48, 0x01, 0xC2,
        ]);
        var Sourceˉoffset = checked(field.Field * Nativeˉcontract.VALUE_SLOT_BYTES);
        code.AddRange([0x48, 0x8B, 0x82]);
        Addˉi32(code, Sourceˉoffset);
        Emitˉstoreˉrax(code, Valueˉslot(function, field.Result));
        code.AddRange([0x48, 0x8B, 0x82]);
        Addˉi32(code, checked(Sourceˉoffset + sizeof(ulong)));
        Emitˉstoreˉraxˉatˉfield(code, Valueˉslot(function, field.Result), sizeof(ulong));
    }

    private static void Emitˉdescriptorˉserviceˉinput(
        List<byte> code,
        int slot,
        int lengthˉoffset)
    {
        code.AddRange([0x4C, 0x8B, 0x84, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
        code.AddRange([0x44, 0x8B, 0x8C, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + lengthˉoffset));
    }

    private static void Emitˉdescriptorˉserviceˉbytes(List<byte> code, int slot)
    {
        code.AddRange([0x48, 0x8B, 0x8C, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
        code.AddRange([0x8B, 0x94, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET));
    }

    private static void Emitˉloadˉdescriptorˉoutputˉr9(List<byte> code, int slot)
    {
        code.AddRange([0x4C, 0x8D, 0x8C, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉdescriptorˉoutputˉr8(List<byte> code, int slot)
    {
        code.AddRange([0x4C, 0x8D, 0x84, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉdescriptorˉoutputˉrax(
        List<byte> code,
        int slot,
        int stackˉadjustment = 0)
    {
        code.AddRange([0x48, 0x8D, 0x84, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + stackˉadjustment));
    }

    private static void Emitˉloadˉdescriptorˉoutputˉrcx(List<byte> code, int slot)
    {
        code.AddRange([0x48, 0x8D, 0x8C, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉserviceˉpointer(List<byte> code, int pointerˉoffset)
    {
        code.AddRange(
        [
            0x49, 0x8B, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET,
            0x48, 0x8B, 0x40,
            checked((byte)pointerˉoffset),
        ]);
    }

    private static void Emitˉserviceˉcall(
        List<byte> code,
        int pointerˉoffset,
        List<int> runtimeˉserviceˉpatches)
    {
        Emitˉserviceˉpointer(code, pointerˉoffset);
        code.AddRange([0xFF, 0xD0, 0x85, 0xC0, 0x0F, 0x85]);
        runtimeˉserviceˉpatches.Add(code.Count);
        Addˉi32(code, 0);
    }

    private static void Emitˉframeˉadjustment(List<byte> code, bool subtract, int bytes)
    {
        code.AddRange([0x48, 0x81, subtract ? (byte)0xEC : (byte)0xC4]);
        Addˉi32(code, bytes);
    }

    private static void Emitˉloadˉeax(List<byte> code, int slot)
    {
        code.AddRange([0x8B, 0x84, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉargument(
        List<byte> code,
        int argument,
        int slot,
        Nativeˉvalueˉtype type)
    {
        code.AddRange((argument, Isˉnativeˉdescriptorˉtype(type)) switch
        {
            (0, false) => [0x44, 0x8B, 0x84, 0x24],
            (1, false) => [0x44, 0x8B, 0x8C, 0x24],
            (2, false) => [0x8B, 0x8C, 0x24],
            (3, false) => [0x8B, 0x94, 0x24],
            (0, true) => [0x4C, 0x8D, 0x84, 0x24],
            (1, true) => [0x4C, 0x8D, 0x8C, 0x24],
            (2, true) => [0x48, 0x8D, 0x8C, 0x24],
            (3, true) => [0x48, 0x8D, 0x94, 0x24],
            _ => throw new Nativeˉbackendˉexception("WVN2901", "The native call exceeds its register-argument limit."),
        });
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉstackˉargument(
        List<byte> code,
        int argument,
        int slot,
        Nativeˉvalueˉtype type,
        int stackˉbytes)
    {
        var Outgoingˉoffset = checked(
            (argument - Nativeˉcontract.REGISTER_CALL_PARAMETERS) *
            Nativeˉcontract.VALUE_SLOT_BYTES);
        var Sourceˉoffset = checked(Slotˉoffset(slot) + stackˉbytes);
        if (Isˉnativeˉdescriptorˉtype(type))
        {
            code.AddRange([0x48, 0x8B, 0x84, 0x24]);
            Addˉi32(code, Sourceˉoffset);
            code.AddRange([0x48, 0x89, 0x84, 0x24]);
            Addˉi32(code, Outgoingˉoffset);
            code.AddRange([0x48, 0x8B, 0x84, 0x24]);
            Addˉi32(code, checked(Sourceˉoffset + sizeof(ulong)));
            code.AddRange([0x48, 0x89, 0x84, 0x24]);
            Addˉi32(code, checked(Outgoingˉoffset + sizeof(ulong)));
            return;
        }

        code.AddRange([0x8B, 0x84, 0x24]);
        Addˉi32(code, Sourceˉoffset);
        code.AddRange([0x89, 0x84, 0x24]);
        Addˉi32(code, Outgoingˉoffset);
    }

    private static void Emitˉstoreˉargument(
        List<byte> code,
        int argument,
        Nativeˉvalueˉtype type,
        int frameˉbytes)
    {
        if (argument >= Nativeˉcontract.REGISTER_CALL_PARAMETERS)
        {
            var Incomingˉoffset = checked(
                frameˉbytes + sizeof(ulong) +
                (argument - Nativeˉcontract.REGISTER_CALL_PARAMETERS) *
                Nativeˉcontract.VALUE_SLOT_BYTES);
            if (Isˉnativeˉdescriptorˉtype(type))
            {
                code.AddRange([0x48, 0x8B, 0x84, 0x24]);
                Addˉi32(code, Incomingˉoffset);
                Emitˉstoreˉrax(code, argument);
                code.AddRange([0x48, 0x8B, 0x84, 0x24]);
                Addˉi32(code, checked(Incomingˉoffset + sizeof(ulong)));
                Emitˉstoreˉraxˉatˉfield(code, argument, sizeof(ulong));
                return;
            }

            code.AddRange([0x8B, 0x84, 0x24]);
            Addˉi32(code, Incomingˉoffset);
            Emitˉstoreˉeax(code, argument);
            return;
        }

        if (Isˉnativeˉdescriptorˉtype(type))
        {
            code.AddRange(argument switch
            {
                0 => [0x49, 0x8B, 0x00],
                1 => [0x49, 0x8B, 0x01],
                2 => [0x48, 0x8B, 0x01],
                3 => [0x48, 0x8B, 0x02],
                _ => throw new Nativeˉbackendˉexception("WVN2901", "The native function exceeds its register-parameter limit."),
            });
            Emitˉstoreˉrax(code, argument);
            code.AddRange(argument switch
            {
                0 => [0x41, 0x8B, 0x40, (byte)Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET],
                1 => [0x41, 0x8B, 0x41, (byte)Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET],
                2 => [0x8B, 0x41, (byte)Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET],
                3 => [0x8B, 0x42, (byte)Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET],
                _ => [],
            });
            Emitˉstoreˉeaxˉatˉfield(code, argument, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
            return;
        }

        code.AddRange(argument switch
        {
            0 => [0x44, 0x89, 0x84, 0x24],
            1 => [0x44, 0x89, 0x8C, 0x24],
            2 => [0x89, 0x8C, 0x24],
            3 => [0x89, 0x94, 0x24],
            _ => throw new Nativeˉbackendˉexception("WVN2901", "The native function exceeds its register-parameter limit."),
        });
        Addˉi32(code, Slotˉoffset(argument));
    }

    private static int Stackˉcallˉbytes(int parameters)
    {
        if (parameters is < 0 or > Nativeˉcontract.MAXIMUM_CALL_PARAMETERS)
        {
            throw new Nativeˉbackendˉexception(
                "WVN2901",
                "The native call exceeds its bounded argument limit.");
        }
        return checked(
            Math.Max(0, parameters - Nativeˉcontract.REGISTER_CALL_PARAMETERS) *
            Nativeˉcontract.VALUE_SLOT_BYTES);
    }

    private static void Emitˉloadˉecx(List<byte> code, int slot)
    {
        code.AddRange([0x8B, 0x8C, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉstoreˉeax(List<byte> code, int slot)
    {
        code.AddRange([0x89, 0x84, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉeaxˉatˉfield(List<byte> code, int slot, int field)
    {
        code.AddRange([0x8B, 0x84, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + field));
    }

    private static void Emitˉloadˉecxˉatˉfield(List<byte> code, int slot, int field)
    {
        code.AddRange([0x8B, 0x8C, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + field));
    }

    private static void Emitˉloadˉedx(List<byte> code, int slot)
    {
        code.AddRange([0x8B, 0x94, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉrax(List<byte> code, int slot)
    {
        code.AddRange([0x48, 0x8B, 0x84, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉloadˉraxˉatˉfield(List<byte> code, int slot, int field)
    {
        code.AddRange([0x48, 0x8B, 0x84, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + field));
    }

    private static void Emitˉloadˉrdx(List<byte> code, int slot)
    {
        code.AddRange([0x48, 0x8B, 0x94, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉstoreˉrax(List<byte> code, int slot)
    {
        code.AddRange([0x48, 0x89, 0x84, 0x24]);
        Addˉi32(code, Slotˉoffset(slot));
    }

    private static void Emitˉstoreˉraxˉatˉfield(List<byte> code, int slot, int field)
    {
        code.AddRange([0x48, 0x89, 0x84, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + field));
    }

    private static void Emitˉstoreˉeaxˉatˉfield(List<byte> code, int slot, int field)
    {
        code.AddRange([0x89, 0x84, 0x24]);
        Addˉi32(code, checked(Slotˉoffset(slot) + field));
    }

    private static void Emitˉstoreˉeaxˉatˉoffset(List<byte> code, int offset)
    {
        code.AddRange([0x89, 0x84, 0x24]);
        Addˉi32(code, offset);
    }

    private static void Emitˉoverflowˉbranch(List<byte> code, List<int> patches)
    {
        code.AddRange([0x0F, 0x80]);
        patches.Add(code.Count);
        Addˉi32(code, 0);
    }

    private static void Emitˉunsignedˉoverflowˉbranch(List<byte> code, List<int> patches)
    {
        code.AddRange([0x0F, 0x82]);
        patches.Add(code.Count);
        Addˉi32(code, 0);
    }

    private static int Slotˉoffset(int slot) =>
        checked(slot * Nativeˉcontract.VALUE_SLOT_BYTES);

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

    private static void Emitˉdescriptorˉfunctionˉreturn(
        List<byte> code,
        int resultˉslot,
        int hiddenˉresultˉslot,
        int frameˉbytes,
        bool restoreˉcontext)
    {
        Emitˉloadˉrdx(code, hiddenˉresultˉslot);
        Emitˉloadˉrax(code, resultˉslot);
        code.AddRange([0x48, 0x89, 0x02]);
        Emitˉloadˉraxˉatˉfield(code, resultˉslot, sizeof(ulong));
        code.AddRange([0x48, 0x89, 0x42, 0x08, 0x31, 0xC0]);
        Emitˉfunctionˉreturn(code, frameˉbytes, restoreˉcontext);
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
        int Nominalˉtypeˉindex = -1);

    private readonly record struct Nativeˉbranchˉpatch(int Offset, int Targetˉblock);

    private readonly record struct Nativeˉcallˉpatch(int Offset, int Targetˉfunction);

    private readonly record struct Nativeˉdataˉreference(int Offset, int Data);

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
