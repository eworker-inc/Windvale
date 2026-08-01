using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class X64ˉnativeˉbackend
{
    private const ulong INTEGER_OVERFLOW_STATUS = 0x0000_0001_0000_0000UL;

    public static Nativeˉcompilation Compile(Verifiedˉmodule verifiedˉmodule)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        var Module = verifiedˉmodule.Module;
        if (Module.Profile != Moduleˉprofile.Portable ||
            !Module.Capabilities.IsEmpty ||
            !Module.Data.IsEmpty ||
            !Module.Types.IsEmpty)
        {
            Fail("WVN2001", "The baseline native subset requires a portable module without capabilities, data, or nominal types.");
        }
        if (verifiedˉmodule.Functions.Length != 1 ||
            Module.Exports.Length != 1 ||
            Module.Exports[0] is not { Name: "Main", Kind: Exportˉkind.Function, Targetˉindex: 0 })
        {
            Fail("WVN2002", "The baseline native subset requires exactly one exported Main function.");
        }

        var Function = verifiedˉmodule.Functions[0];
        if (!StringComparer.Ordinal.Equals(Function.Declaration.Name, "Main") ||
            !Function.Declaration.Parameterˉtypes.IsEmpty ||
            Function.Declaration.Returnˉtype != Valueˉtype.I32 ||
            Function.Declaration.Localˉtypes.IsEmpty ||
            Function.Declaration.Localˉtypes.Length >= Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
            Function.Declaration.Localˉtypes.Any(Type => !Isˉnativeˉtype(Type)) ||
            Function.Declaration.Maximumˉstackˉdepth is < 1 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
        {
            Fail(
                "WVN2002",
                "The baseline native entry must be Main() -> i32 with only bounded i32/bool locals; " +
                $"found name='{Function.Declaration.Name}', parameters={Function.Declaration.Parameterˉtypes.Length}, " +
                $"return={Function.Declaration.Returnˉtype}, locals={Function.Declaration.Localˉtypes.Length}, " +
                $"max-stack={Function.Declaration.Maximumˉstackˉdepth}.");
        }

        var Nativeˉmodule = Lowerˉverifiedˉwvb(Function);
        var Fragment = Selectˉx64(Nativeˉmodule);
        return new(Nativeˉmodule, Fragment);
    }

    private static Nativeˉmodule Lowerˉverifiedˉwvb(Verifiedˉfunction function)
    {
        if (function.Instructions is
            [
                { Opcode: Opcode.I32ˉconst } Constant,
                { Opcode: Opcode.Localˉstore, Unsignedˉoperand: 0 },
                { Opcode: Opcode.Localˉload, Unsignedˉoperand: 0 },
                { Opcode: Opcode.Return },
            ] &&
            function.Declaration.Localˉtypes is [{ Kind: Valueˉtype.I32 }])
        {
            return new([
                new(
                    "Main",
                    [],
                    [Nativeˉvalueˉtype.I32],
                    [new(0, [new Nativeˉi32ˉconstant(0, Constant.Signedˉoperand)], new Nativeˉreturn(0))])
            ]);
        }

        var Localˉtypes = function.Declaration.Localˉtypes
            .Select(Toˉnativeˉtype)
            .ToImmutableArray();
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
                if (Localˉtypes.Length + Valueˉtypes.Count >= Nativeˉcontract.MAXIMUM_FRAME_SLOTS)
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
                    case Opcode.Localˉload:
                        var Loadˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Loadˉindex >= (uint)Localˉtypes.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid local load during native lowering.");
                        }
                        var Loadˉtype = Localˉtypes[Loadˉindex];
                        var Loadˉresult = Newˉvalue(Loadˉtype);
                        Operations.Add(new Nativeˉlocalˉload(Loadˉresult, Loadˉindex, Loadˉtype));
                        Stack.Push(new(Loadˉresult, Loadˉtype));
                        break;
                    case Opcode.Localˉstore:
                        var Storeˉindex = checked((int)Instruction.Unsignedˉoperand);
                        if ((uint)Storeˉindex >= (uint)Localˉtypes.Length)
                        {
                            Fail("WVN2003", "Verified WVB exposed an invalid local store during native lowering.");
                        }
                        var Storeˉtype = Localˉtypes[Storeˉindex];
                        var Storedˉvalue = Popˉvalue(Storeˉtype);
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
                        var Returnˉvalue = Popˉvalue(Nativeˉvalueˉtype.I32);
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

        foreach (var Block in Blocks)
        {
            foreach (var Target in Targets(Block.Terminator))
            {
                if (Target <= Block.Id)
                {
                    Fail(
                        "WVN2005",
                        "The first native control-flow subset permits only forward acyclic branches until an execution-budget contract exists.");
                }
            }
        }

        return new([new("Main", Localˉtypes, Valueˉtypes.ToImmutable(), Blocks.ToImmutable())]);
    }

    private static Nativeˉfragment Selectˉx64(Nativeˉmodule module)
    {
        if (module.Functions.Length != 1 ||
            module.Functions[0] is not { Name: "Main", Blocks.Length: >= 1 })
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }
        var Function = module.Functions[0];
        if (Function is
            {
                Localˉtypes.IsEmpty: true,
                Valueˉtypes: [Nativeˉvalueˉtype.I32],
                Blocks: [
                    {
                        Id: 0,
                        Operations: [Nativeˉi32ˉconstant { Result: 0 } Singleˉconstant],
                        Terminator: Nativeˉreturn { Value: 0 },
                    },
                ],
            })
        {
            return Selectˉconstant(Singleˉconstant.Value);
        }

        Validateˉfunction(Function);
        var Usedˉslots = checked(Function.Localˉtypes.Length + Function.Valueˉtypes.Length);
        var Frameˉbytes = checked((Usedˉslots * sizeof(int) + 15) & ~15);
        var Code = new List<byte>();
        var Overflowˉpatches = new List<int>();
        var Branchˉpatches = new List<Nativeˉbranchˉpatch>();
        var Blockˉoffsets = new int[Function.Blocks.Length];

        Emitˉframeˉadjustment(Code, subtract: true, Frameˉbytes);
        Code.AddRange([0x31, 0xC0]);
        for (var Slot = 0; Slot < Frameˉbytes / sizeof(int); Slot++)
        {
            Emitˉstoreˉeax(Code, Slot);
        }

        foreach (var Block in Function.Blocks)
        {
            Blockˉoffsets[Block.Id] = Code.Count;
            foreach (var Operation in Block.Operations)
            {
                switch (Operation)
                {
                    case Nativeˉi32ˉconstant Constant:
                        Emitˉconstant(Code, Constant.Value, Valueˉslot(Function, Constant.Result));
                        break;
                    case Nativeˉboolˉconstant Constant:
                        Emitˉconstant(Code, Constant.Value ? 1 : 0, Valueˉslot(Function, Constant.Result));
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
                }
            }

            switch (Block.Terminator)
            {
                case Nativeˉjump Jump:
                    Emitˉdirectˉbranch(Code, 0xE9, Jump.Targetˉblock, Branchˉpatches);
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
                    Emitˉframeˉadjustment(Code, subtract: false, Frameˉbytes);
                    Code.Add(0xC3);
                    break;
            }
        }

        var Trapˉoffset = Code.Count;
        Emitˉframeˉadjustment(Code, subtract: false, Frameˉbytes);
        Code.AddRange([0x48, 0xB8]);
        Addˉu64(Code, INTEGER_OVERFLOW_STATUS);
        Code.Add(0xC3);

        var Bytes = Code.ToArray();
        if (Bytes.Length > Nativeˉcontract.MAXIMUM_CODE_BYTES)
        {
            Fail("WVN2902", "The selected x86-64 fragment exceeds its code-size limit.");
        }
        foreach (var Patchˉoffset in Overflowˉpatches)
        {
            Writeˉrelativeˉi32(Bytes, Patchˉoffset, Trapˉoffset);
        }
        foreach (var Patch in Branchˉpatches)
        {
            Writeˉrelativeˉi32(Bytes, Patch.Offset, Blockˉoffsets[Patch.Targetˉblock]);
        }

        var Fragment = new Nativeˉfragment(
            Nativeˉcontract.X64_BASELINE_TARGET,
            Nativeˉcontract.ABI_VERSION,
            Objectˉarchitecture.X86ˉ64,
            16,
            Bytes.ToImmutableArray(),
            [
                new("$overflow", Nativeˉsymbolˉbinding.Local, Nativeˉsymbolˉkind.Function,
                    (uint)Trapˉoffset, (uint)(Bytes.Length - Trapˉoffset)),
                new("Main", Nativeˉsymbolˉbinding.Export, Nativeˉsymbolˉkind.Function,
                    0, (uint)Trapˉoffset),
            ],
            []);
        return Nativeˉfragmentˉverifier.Verify(Fragment);
    }

    private static void Validateˉfunction(Nativeˉfunction function)
    {
        if (function.Localˉtypes.IsDefault ||
            function.Valueˉtypes.IsDefault ||
            function.Blocks.IsDefaultOrEmpty ||
            function.Blocks.Length > Nativeˉcontract.MAXIMUM_BLOCKS ||
            function.Localˉtypes.Length + function.Valueˉtypes.Length is < 1 or > Nativeˉcontract.MAXIMUM_FRAME_SLOTS ||
            function.Localˉtypes.Any(Type => !Enum.IsDefined(Type)) ||
            function.Valueˉtypes.Any(Type => !Enum.IsDefined(Type)))
        {
            Fail("WVN2901", "The x86-64 selector received invalid native function metadata.");
        }

        var Nextˉvalue = 0;
        var Returnˉcount = 0;
        for (var Blockˉindex = 0; Blockˉindex < function.Blocks.Length; Blockˉindex++)
        {
            var Block = function.Blocks[Blockˉindex];
            if (Block is null || Block.Id != Blockˉindex || Block.Operations.IsDefault || Block.Terminator is null)
            {
                Fail("WVN2901", "The x86-64 selector requires canonical initialized basic blocks.");
            }
            foreach (var Operation in Block.Operations)
            {
                switch (Operation)
                {
                    case Nativeˉi32ˉconstant Constant:
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.I32, ref Nextˉvalue);
                        break;
                    case Nativeˉboolˉconstant Constant:
                        Requireˉresult(function, Constant.Result, Nativeˉvalueˉtype.Bool, ref Nextˉvalue);
                        break;
                    case Nativeˉlocalˉload Load:
                        Requireˉlocal(function, Load.Local, Load.Type);
                        Requireˉresult(function, Load.Result, Load.Type, ref Nextˉvalue);
                        break;
                    case Nativeˉlocalˉstore Store:
                        Requireˉlocal(function, Store.Local, Store.Type);
                        Requireˉvalue(function, Store.Value, Store.Type, Nextˉvalue);
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
                    default:
                        Fail("WVN2901", "The x86-64 selector received an invalid native operation.");
                        break;
                }
            }

            switch (Block.Terminator)
            {
                case Nativeˉjump Jump:
                    Requireˉforwardˉtarget(function, Block.Id, Jump.Targetˉblock);
                    break;
                case Nativeˉbranch Branch:
                    Requireˉvalue(function, Branch.Condition, Nativeˉvalueˉtype.Bool, Nextˉvalue);
                    Requireˉforwardˉtarget(function, Block.Id, Branch.Trueˉblock);
                    Requireˉforwardˉtarget(function, Block.Id, Branch.Falseˉblock);
                    break;
                case Nativeˉreturn Return:
                    Requireˉvalue(function, Return.Value, Nativeˉvalueˉtype.I32, Nextˉvalue);
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

    private static Nativeˉfragment Selectˉconstant(int value)
    {
        var Code = new byte[6];
        Code[0] = 0xB8;
        BinaryPrimitives.WriteInt32LittleEndian(Code.AsSpan(1, sizeof(int)), value);
        Code[5] = 0xC3;
        var Fragment = new Nativeˉfragment(
            Nativeˉcontract.X64_BASELINE_TARGET,
            Nativeˉcontract.ABI_VERSION,
            Objectˉarchitecture.X86ˉ64,
            16,
            Code.ToImmutableArray(),
            [new("Main", Nativeˉsymbolˉbinding.Export, Nativeˉsymbolˉkind.Function, 0, (uint)Code.Length)],
            []);
        return Nativeˉfragmentˉverifier.Verify(Fragment);
    }

    private static bool Isˉnativeˉtype(Valueˉshape type) =>
        type.Nominalˉtypeˉindex == -1 && type.Kind is Valueˉtype.I32 or Valueˉtype.Bool;

    private static Nativeˉvalueˉtype Toˉnativeˉtype(Valueˉshape type) =>
        type.Kind switch
        {
            Valueˉtype.I32 when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.I32,
            Valueˉtype.Bool when type.Nominalˉtypeˉindex == -1 => Nativeˉvalueˉtype.Bool,
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
        if ((uint)local >= (uint)function.Localˉtypes.Length || function.Localˉtypes[local] != type)
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

    private static void Requireˉforwardˉtarget(Nativeˉfunction function, int source, int target)
    {
        if (target <= source || target >= function.Blocks.Length)
        {
            Fail("WVN2901", "The x86-64 selector permits only bounded forward basic-block targets.");
        }
    }

    private static int Valueˉslot(Nativeˉfunction function, int value) =>
        checked(function.Localˉtypes.Length + value);

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

    private static void Writeˉrelativeˉi32(byte[] code, int displacementˉoffset, int targetˉoffset)
    {
        var Displacement = checked(targetˉoffset - (displacementˉoffset + sizeof(int)));
        BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(displacementˉoffset, sizeof(int)), Displacement);
    }

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

    private readonly record struct Nativeˉstackˉvalue(int Value, Nativeˉvalueˉtype Type);

    private readonly record struct Nativeˉbranchˉpatch(int Offset, int Targetˉblock);

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
