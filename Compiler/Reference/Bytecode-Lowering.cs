using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

internal static class Bytecodeˉlowering
{
    public static Bytecodeˉmodule Lower(Wirˉmodule module)
    {
        var Dataˉindices = module.Data
            .Select((Data, Index) => (Data.Name, Index))
            .ToDictionary(Item => Item.Name, Item => Item.Index, StringComparer.Ordinal);
        var Functionˉindices = module.Functions
            .Select((Function, Index) => (Function.Name, Index))
            .ToDictionary(Item => Item.Name, Item => Item.Index, StringComparer.Ordinal);
        var Capabilityˉindices = module.Capabilities
            .Select((Capability, Index) => (Capability.Name, Index))
            .ToDictionary(Item => Item.Name, Item => Item.Index, StringComparer.Ordinal);

        var Moduleˉcode = ImmutableArray.CreateBuilder<byte>();
        var Functionˉdeclarations = ImmutableArray.CreateBuilder<Functionˉdeclaration>(module.Functions.Length);

        foreach (var Function in module.Functions)
        {
            var Emitter = new Codeˉemitter(
                Function,
                Dataˉindices,
                Functionˉindices,
                Capabilityˉindices);
            var Functionˉcode = Emitter.Emit();
            var Codeˉoffset = Moduleˉcode.Count;
            Moduleˉcode.AddRange(Functionˉcode);
            Functionˉdeclarations.Add(new(
                Function.Name,
                Function.Parameterˉtypes,
                Function.Returnˉtype,
                [.. Function.Userˉlocalˉtypes, .. Function.Temporaryˉtypes],
                Codeˉoffset,
                Functionˉcode.Length,
                Emitter.Maximumˉstack));
        }

        var Exports = module.Functions
            .Where(Function => Function.Isˉexported)
            .Select(Function => new Exportˉdeclaration(
                Function.Name,
                Exportˉkind.Function,
                Functionˉindices[Function.Name]))
            .OrderBy(Export => Export.Name, StringComparer.Ordinal)
            .ToImmutableArray();

        return new(
            module.Name,
            module.Profile,
            module.Capabilities,
            module.Data,
            Functionˉdeclarations.ToImmutable(),
            Moduleˉcode.ToImmutable(),
            Exports)
        {
            Types = module.Types,
            Metadata = module.Metadata,
        };
    }

    private sealed class Codeˉemitter(
        Wirˉfunction function,
        IReadOnlyDictionary<string, int> dataˉindices,
        IReadOnlyDictionary<string, int> functionˉindices,
        IReadOnlyDictionary<string, int> capabilityˉindices)
    {
        private readonly List<byte> Bytes = [];
        private readonly Dictionary<int, int> Blockˉoffsets = [];
        private readonly List<(int Operandˉoffset, int Blockˉid)> Patches = [];
        private int Stackˉdepth;

        public int Maximumˉstack { get; private set; }

        public ImmutableArray<byte> Emit()
        {
            foreach (var Block in function.Blocks.OrderBy(Block => Block.Id))
            {
                if (Stackˉdepth != 0)
                {
                    throw new InvalidOperationException(
                        $"WIR block {Block.Id} in function '{function.Name}' begins with a nonempty stack.");
                }

                Blockˉoffsets.Add(Block.Id, Bytes.Count);
                foreach (var Instruction in Block.Instructions)
                {
                    Emitˉinstruction(Instruction);
                }

                Emitˉterminator(Block);
                if (Stackˉdepth != 0)
                {
                    throw new InvalidOperationException(
                        $"WIR block {Block.Id} in function '{function.Name}' ends with a nonempty stack.");
                }
            }

            foreach (var Patch in Patches)
            {
                if (!Blockˉoffsets.TryGetValue(Patch.Blockˉid, out var Target))
                {
                    throw new InvalidOperationException(
                        $"WIR branch in function '{function.Name}' references block {Patch.Blockˉid}.");
                }

                Patchˉu32(Patch.Operandˉoffset, Target);
            }

            return [.. Bytes];
        }

        private void Emitˉinstruction(Wirˉinstruction instruction)
        {
            switch (instruction.Operation)
            {
                case Wirˉoperation.I32ˉconstant:
                    Emitˉi32(Opcode.I32ˉconst, instruction.Integerˉoperand, pop: 0, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.I64ˉconstant:
                    Emitˉi64(Opcode.I64ˉconst, instruction.Wideˉintegerˉoperand, pop: 0, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.U8ˉconstant:
                    Emitˉbyte(
                        Opcode.U8ˉconst,
                        checked((byte)instruction.Unsignedˉintegerˉoperand),
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.U32ˉconstant:
                    Emitˉu32(Opcode.U32ˉconst, instruction.Unsignedˉintegerˉoperand, pop: 0, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.U64ˉconstant:
                    Emitˉu64(
                        Opcode.U64ˉconst,
                        instruction.Unsignedˉwideˉintegerˉoperand,
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Boolˉconstant:
                    Emitˉbyte(
                        Opcode.Boolˉconst,
                        checked((byte)instruction.Integerˉoperand),
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Textˉconstant:
                    Emitˉu32(
                        Opcode.Textˉconst,
                        Resolve(dataˉindices, instruction.Nameˉoperand, "data"),
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Bytesˉconstant:
                    Emitˉu32(
                        Opcode.Bytesˉconst,
                        Resolve(dataˉindices, instruction.Nameˉoperand, "data"),
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Loadˉlocal:
                    Emitˉu32(Opcode.Localˉload, instruction.Integerˉoperand, pop: 0, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Storeˉlocal:
                    Loadˉtemporary(instruction.Operands[0]);
                    Emitˉu32(Opcode.Localˉstore, instruction.Integerˉoperand, pop: 1, push: 0);
                    break;
                case Wirˉoperation.Dataˉlength:
                    Emitˉu32(
                        Opcode.Dataˉlength,
                        Resolve(dataˉindices, instruction.Nameˉoperand, "data"),
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Dataˉloadˉi32:
                    Loadˉtemporary(instruction.Operands[0]);
                    Emitˉu32(
                        Opcode.Dataˉloadˉi32,
                        Resolve(dataˉindices, instruction.Nameˉoperand, "data"),
                        pop: 1,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Bytesˉlength:
                case Wirˉoperation.Bytesˉslice:
                case Wirˉoperation.Bytesˉreadˉu8:
                case Wirˉoperation.Bytesˉreadˉu16ˉlittle:
                case Wirˉoperation.Bytesˉreadˉu32ˉlittle:
                case Wirˉoperation.Bytesˉreadˉi32ˉlittle:
                    Loadˉarguments(instruction.Operands);
                    Emitˉnone(
                        Mapˉopcode(instruction.Operation),
                        pop: instruction.Operands.Length,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Recordˉcreate:
                    Loadˉarguments(instruction.Operands);
                    Emitˉu32(
                        Opcode.Recordˉcreate,
                        instruction.Unsignedˉintegerˉoperand,
                        pop: instruction.Operands.Length,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Recordˉfield:
                    Loadˉtemporary(instruction.Operands[0]);
                    Emitˉu32(
                        Opcode.Recordˉfield,
                        instruction.Unsignedˉintegerˉoperand,
                        pop: 1,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Enumˉconstant:
                    Emitˉtwoˉu32(
                        Opcode.Enumˉconst,
                        instruction.Unsignedˉintegerˉoperand,
                        instruction.Secondˉunsignedˉintegerˉoperand,
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Variantˉcreate:
                    Loadˉarguments(instruction.Operands);
                    Emitˉtwoˉu32(
                        Opcode.Variantˉcreate,
                        instruction.Unsignedˉintegerˉoperand,
                        instruction.Secondˉunsignedˉintegerˉoperand,
                        pop: instruction.Operands.Length,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Variantˉisˉcase:
                case Wirˉoperation.Variantˉpayload:
                    Loadˉtemporary(instruction.Operands[0]);
                    Emitˉtwoˉu32(
                        instruction.Operation == Wirˉoperation.Variantˉisˉcase
                            ? Opcode.Variantˉisˉcase
                            : Opcode.Variantˉpayload,
                        instruction.Unsignedˉintegerˉoperand,
                        instruction.Secondˉunsignedˉintegerˉoperand,
                        pop: 1,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Builderˉcreate:
                    Emitˉtwoˉu32(
                        Opcode.Builderˉcreate,
                        instruction.Unsignedˉintegerˉoperand,
                        instruction.Secondˉunsignedˉintegerˉoperand,
                        pop: 0,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Builderˉpush:
                    Loadˉarguments(instruction.Operands);
                    Emitˉnone(Opcode.Builderˉpush, pop: 2, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Builderˉfreeze:
                case Wirˉoperation.Sequenceˉlength:
                    Loadˉtemporary(instruction.Operands[0]);
                    Emitˉnone(
                        instruction.Operation == Wirˉoperation.Builderˉfreeze
                            ? Opcode.Builderˉfreeze
                            : Opcode.Sequenceˉlength,
                        pop: 1,
                        push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Sequenceˉelement:
                    Loadˉarguments(instruction.Operands);
                    Emitˉnone(Opcode.Sequenceˉelement, pop: 2, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.I32ˉadd:
                case Wirˉoperation.I32ˉsubtract:
                case Wirˉoperation.I32ˉmultiply:
                case Wirˉoperation.I32ˉequal:
                case Wirˉoperation.I32ˉnotˉequal:
                case Wirˉoperation.I32ˉless:
                case Wirˉoperation.I32ˉlessˉequal:
                case Wirˉoperation.I32ˉgreater:
                case Wirˉoperation.I32ˉgreaterˉequal:
                case Wirˉoperation.I64ˉadd:
                case Wirˉoperation.I64ˉsubtract:
                case Wirˉoperation.I64ˉmultiply:
                case Wirˉoperation.I64ˉequal:
                case Wirˉoperation.I64ˉnotˉequal:
                case Wirˉoperation.I64ˉless:
                case Wirˉoperation.I64ˉlessˉequal:
                case Wirˉoperation.I64ˉgreater:
                case Wirˉoperation.I64ˉgreaterˉequal:
                case Wirˉoperation.Boolˉequal:
                case Wirˉoperation.Boolˉnotˉequal:
                case Wirˉoperation.U32ˉadd:
                case Wirˉoperation.U32ˉsubtract:
                case Wirˉoperation.U32ˉmultiply:
                case Wirˉoperation.U32ˉequal:
                case Wirˉoperation.U32ˉnotˉequal:
                case Wirˉoperation.U32ˉless:
                case Wirˉoperation.U32ˉlessˉequal:
                case Wirˉoperation.U32ˉgreater:
                case Wirˉoperation.U32ˉgreaterˉequal:
                case Wirˉoperation.U64ˉadd:
                case Wirˉoperation.U64ˉsubtract:
                case Wirˉoperation.U64ˉmultiply:
                case Wirˉoperation.U64ˉequal:
                case Wirˉoperation.U64ˉnotˉequal:
                case Wirˉoperation.U64ˉless:
                case Wirˉoperation.U64ˉlessˉequal:
                case Wirˉoperation.U64ˉgreater:
                case Wirˉoperation.U64ˉgreaterˉequal:
                case Wirˉoperation.U8ˉequal:
                case Wirˉoperation.U8ˉnotˉequal:
                case Wirˉoperation.I32ˉdivide:
                case Wirˉoperation.I32ˉremainder:
                case Wirˉoperation.U32ˉdivide:
                case Wirˉoperation.U32ˉremainder:
                case Wirˉoperation.I64ˉdivide:
                case Wirˉoperation.I64ˉremainder:
                case Wirˉoperation.U64ˉdivide:
                case Wirˉoperation.U64ˉremainder:
                case Wirˉoperation.U8ˉbitwiseˉand:
                case Wirˉoperation.U8ˉbitwiseˉor:
                case Wirˉoperation.U8ˉbitwiseˉxor:
                case Wirˉoperation.U8ˉshiftˉleft:
                case Wirˉoperation.U8ˉshiftˉright:
                case Wirˉoperation.U32ˉbitwiseˉand:
                case Wirˉoperation.U32ˉbitwiseˉor:
                case Wirˉoperation.U32ˉbitwiseˉxor:
                case Wirˉoperation.U32ˉshiftˉleft:
                case Wirˉoperation.U32ˉshiftˉright:
                case Wirˉoperation.U64ˉbitwiseˉand:
                case Wirˉoperation.U64ˉbitwiseˉor:
                case Wirˉoperation.U64ˉbitwiseˉxor:
                case Wirˉoperation.U64ˉshiftˉleft:
                case Wirˉoperation.U64ˉshiftˉright:
                case Wirˉoperation.Textˉequal:
                case Wirˉoperation.Textˉnotˉequal:
                case Wirˉoperation.Bytesˉequal:
                case Wirˉoperation.Bytesˉnotˉequal:
                case Wirˉoperation.Enumˉequal:
                case Wirˉoperation.Enumˉnotˉequal:
                case Wirˉoperation.Textˉconcat:
                    Loadˉtemporary(instruction.Operands[0]);
                    Loadˉtemporary(instruction.Operands[1]);
                    Emitˉnone(Mapˉopcode(instruction.Operation), pop: 2, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.I32ˉnegate:
                case Wirˉoperation.I64ˉnegate:
                case Wirˉoperation.Boolˉnot:
                case Wirˉoperation.U8ˉbitwiseˉnot:
                case Wirˉoperation.U32ˉbitwiseˉnot:
                case Wirˉoperation.U64ˉbitwiseˉnot:
                case Wirˉoperation.Enumˉname:
                case Wirˉoperation.I32ˉformat:
                case Wirˉoperation.I64ˉformat:
                case Wirˉoperation.U8ˉformat:
                case Wirˉoperation.U32ˉformat:
                case Wirˉoperation.U64ˉformat:
                case Wirˉoperation.U32ˉfromˉu8:
                case Wirˉoperation.Textˉutf8ˉisˉvalid:
                case Wirˉoperation.Textˉfromˉutf8:
                case Wirˉoperation.Textˉquote:
                case Wirˉoperation.Bytesˉfromˉu8:
                case Wirˉoperation.Bytesˉfromˉu16ˉlittle:
                case Wirˉoperation.Bytesˉfromˉu32ˉlittle:
                case Wirˉoperation.Bytesˉfromˉi32ˉlittle:
                case Wirˉoperation.Bytesˉsha256ˉhex:
                case Wirˉoperation.Textˉtoˉutf8:
                    Loadˉtemporary(instruction.Operands[0]);
                    Emitˉnone(Mapˉopcode(instruction.Operation), pop: 1, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Bytesˉconcat:
                    Loadˉarguments(instruction.Operands);
                    Emitˉnone(Opcode.Bytesˉconcat, pop: 2, push: 1);
                    Storeˉresult(instruction);
                    break;
                case Wirˉoperation.Callˉfunction:
                    Loadˉarguments(instruction.Operands);
                    var Calledˉfunctionˉindex = Resolve(
                        functionˉindices,
                        instruction.Nameˉoperand,
                        "function");
                    var Returnˉcount = instruction.Result is null ? 0 : 1;
                    Emitˉu32(
                        Opcode.Call,
                        Calledˉfunctionˉindex,
                        pop: instruction.Operands.Length,
                        push: Returnˉcount);
                    Storeˉoptionalˉresult(instruction);
                    break;
                case Wirˉoperation.Callˉcapability:
                    Loadˉarguments(instruction.Operands);
                    Emitˉu32(
                        Opcode.Callˉcapability,
                        Resolve(capabilityˉindices, instruction.Nameˉoperand, "capability"),
                        pop: instruction.Operands.Length,
                        push: instruction.Result is null ? 0 : 1);
                    Storeˉoptionalˉresult(instruction);
                    break;
                case Wirˉoperation.Boolˉphi:
                    // Each predecessor writes the selected operand directly into this result's
                    // temporary slot immediately before its jump to the phi block.
                    break;
                default:
                    throw new InvalidOperationException(
                        $"WIR operation '{instruction.Operation}' has no bytecode lowering.");
            }
        }

        private void Emitˉterminator(Wirˉblock block)
        {
            switch (block.Terminator)
            {
                case Wirˉjump Jump:
                    Emitˉphiˉassignments(block.Id, Jump.Targetˉblock);
                    Emitˉbranch(Opcode.Jump, Jump.Targetˉblock, pop: 0);
                    break;
                case Wirˉbranch Branch:
                    Requireˉnoˉphiˉtarget(Branch.Trueˉblock);
                    Requireˉnoˉphiˉtarget(Branch.Falseˉblock);
                    Loadˉtemporary(Branch.Condition);
                    Emitˉbranch(Opcode.Branchˉfalse, Branch.Falseˉblock, pop: 1);
                    Emitˉbranch(Opcode.Jump, Branch.Trueˉblock, pop: 0);
                    break;
                case Wirˉreturn Return:
                    if (Return.Value is not null)
                    {
                        Loadˉtemporary(Return.Value.Value);
                        Emitˉnone(Opcode.Return, pop: 1, push: 0);
                    }
                    else
                    {
                        Emitˉnone(Opcode.Return, pop: 0, push: 0);
                    }

                    break;
                default:
                    throw new InvalidOperationException(
                        $"WIR terminator '{block.Terminator.GetType().Name}' has no bytecode lowering.");
            }
        }

        private void Emitˉphiˉassignments(int predecessorˉblock, int targetˉblock)
        {
            var Target = function.Blocks.SingleOrDefault(Block => Block.Id == targetˉblock)
                ?? throw new InvalidOperationException(
                    $"WIR jump in function '{function.Name}' references block {targetˉblock}.");
            foreach (var Phi in Target.Instructions.TakeWhile(
                         Instruction => Instruction.Operation == Wirˉoperation.Boolˉphi))
            {
                if (Phi.Result is null || Phi.Operands.Length != 2)
                {
                    throw new InvalidOperationException(
                        $"WIR bool phi in function '{function.Name}' has an invalid result or operand contract.");
                }

                int Operand;
                if (Phi.Integerˉoperand == predecessorˉblock)
                {
                    Operand = Phi.Operands[0];
                }
                else if (Phi.Unsignedˉintegerˉoperand == checked((uint)predecessorˉblock))
                {
                    Operand = Phi.Operands[1];
                }
                else
                {
                    throw new InvalidOperationException(
                        $"WIR bool phi in block {targetˉblock} does not admit predecessor {predecessorˉblock}.");
                }

                Loadˉtemporary(Operand);
                Storeˉresult(Phi);
            }
        }

        private void Requireˉnoˉphiˉtarget(int targetˉblock)
        {
            var Target = function.Blocks.SingleOrDefault(Block => Block.Id == targetˉblock)
                ?? throw new InvalidOperationException(
                    $"WIR branch in function '{function.Name}' references block {targetˉblock}.");
            if (Target.Instructions.FirstOrDefault()?.Operation == Wirˉoperation.Boolˉphi)
            {
                throw new InvalidOperationException(
                    $"WIR conditional branch in function '{function.Name}' targets phi block {targetˉblock} directly.");
            }
        }

        private void Loadˉarguments(ImmutableArray<int> operands)
        {
            foreach (var Operand in operands)
            {
                Loadˉtemporary(Operand);
            }
        }

        private void Loadˉtemporary(int temporary)
        {
            Emitˉu32(Opcode.Localˉload, Temporaryˉslot(temporary), pop: 0, push: 1);
        }

        private void Storeˉresult(Wirˉinstruction instruction)
        {
            if (instruction.Result is null)
            {
                throw new InvalidOperationException(
                    $"WIR operation '{instruction.Operation}' must produce a result.");
            }

            Emitˉu32(
                Opcode.Localˉstore,
                Temporaryˉslot(instruction.Result.Value),
                pop: 1,
                push: 0);
        }

        private void Storeˉoptionalˉresult(Wirˉinstruction instruction)
        {
            if (instruction.Result is not null)
            {
                Storeˉresult(instruction);
            }
        }

        private int Temporaryˉslot(int temporary)
        {
            if ((uint)temporary >= (uint)function.Temporaryˉtypes.Length)
            {
                throw new InvalidOperationException(
                    $"Function '{function.Name}' references temporary {temporary}.");
            }

            return checked(
                function.Parameterˉtypes.Length +
                function.Userˉlocalˉtypes.Length +
                temporary);
        }

        private void Emitˉbranch(Opcode opcode, int blockˉid, int pop)
        {
            Bytes.Add((byte)opcode);
            var Operandˉoffset = Bytes.Count;
            Writeˉu32(0);
            Patches.Add((Operandˉoffset, blockˉid));
            Applyˉstack(pop, push: 0);
        }

        private void Emitˉnone(Opcode opcode, int pop, int push)
        {
            Bytes.Add((byte)opcode);
            Applyˉstack(pop, push);
        }

        private void Emitˉbyte(Opcode opcode, byte operand, int pop, int push)
        {
            Bytes.Add((byte)opcode);
            Bytes.Add(operand);
            Applyˉstack(pop, push);
        }

        private void Emitˉi32(Opcode opcode, int operand, int pop, int push)
        {
            Bytes.Add((byte)opcode);
            Span<byte> Buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(Buffer, operand);
            Bytes.AddRange(Buffer);
            Applyˉstack(pop, push);
        }

        private void Emitˉi64(Opcode opcode, long operand, int pop, int push)
        {
            Bytes.Add((byte)opcode);
            Span<byte> Buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(Buffer, operand);
            Bytes.AddRange(Buffer);
            Applyˉstack(pop, push);
        }

        private void Emitˉu32(Opcode opcode, int operand, int pop, int push)
        {
            if (operand < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(operand));
            }

            Emitˉu32(opcode, (uint)operand, pop, push);
        }

        private void Emitˉu32(Opcode opcode, uint operand, int pop, int push)
        {
            Bytes.Add((byte)opcode);
            Writeˉu32(operand);
            Applyˉstack(pop, push);
        }

        private void Emitˉu64(Opcode opcode, ulong operand, int pop, int push)
        {
            Bytes.Add((byte)opcode);
            Span<byte> Buffer = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64LittleEndian(Buffer, operand);
            Bytes.AddRange(Buffer);
            Applyˉstack(pop, push);
        }

        private void Emitˉtwoˉu32(
            Opcode opcode,
            uint firstˉoperand,
            uint secondˉoperand,
            int pop,
            int push)
        {
            Bytes.Add((byte)opcode);
            Writeˉu32(firstˉoperand);
            Writeˉu32(secondˉoperand);
            Applyˉstack(pop, push);
        }

        private void Writeˉu32(uint value)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Buffer, value);
            Bytes.AddRange(Buffer);
        }

        private void Patchˉu32(int offset, int value)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Buffer, checked((uint)value));
            for (var Index = 0; Index < Buffer.Length; Index++)
            {
                Bytes[offset + Index] = Buffer[Index];
            }
        }

        private void Applyˉstack(int pop, int push)
        {
            Stackˉdepth -= pop;
            if (Stackˉdepth < 0)
            {
                throw new InvalidOperationException(
                    $"Bytecode lowering underflowed the stack in function '{function.Name}'.");
            }

            Stackˉdepth += push;
            Maximumˉstack = Math.Max(Maximumˉstack, Stackˉdepth);
        }

        private static int Resolve(
            IReadOnlyDictionary<string, int> indices,
            string? name,
            string kind)
        {
            if (name is null || !indices.TryGetValue(name, out var Index))
            {
                throw new InvalidOperationException($"WIR references unknown {kind} '{name}'.");
            }

            return Index;
        }

        private static Opcode Mapˉopcode(Wirˉoperation operation)
        {
            return operation switch
            {
                Wirˉoperation.I32ˉadd => Opcode.I32ˉadd,
                Wirˉoperation.I32ˉsubtract => Opcode.I32ˉsubtract,
                Wirˉoperation.I32ˉmultiply => Opcode.I32ˉmultiply,
                Wirˉoperation.I32ˉnegate => Opcode.I32ˉnegate,
                Wirˉoperation.I32ˉequal => Opcode.I32ˉequal,
                Wirˉoperation.I32ˉnotˉequal => Opcode.I32ˉnotˉequal,
                Wirˉoperation.I32ˉless => Opcode.I32ˉless,
                Wirˉoperation.I32ˉlessˉequal => Opcode.I32ˉlessˉequal,
                Wirˉoperation.I32ˉgreater => Opcode.I32ˉgreater,
                Wirˉoperation.I32ˉgreaterˉequal => Opcode.I32ˉgreaterˉequal,
                Wirˉoperation.I64ˉadd => Opcode.I64ˉadd,
                Wirˉoperation.I64ˉsubtract => Opcode.I64ˉsubtract,
                Wirˉoperation.I64ˉmultiply => Opcode.I64ˉmultiply,
                Wirˉoperation.I64ˉnegate => Opcode.I64ˉnegate,
                Wirˉoperation.I64ˉequal => Opcode.I64ˉequal,
                Wirˉoperation.I64ˉnotˉequal => Opcode.I64ˉnotˉequal,
                Wirˉoperation.I64ˉless => Opcode.I64ˉless,
                Wirˉoperation.I64ˉlessˉequal => Opcode.I64ˉlessˉequal,
                Wirˉoperation.I64ˉgreater => Opcode.I64ˉgreater,
                Wirˉoperation.I64ˉgreaterˉequal => Opcode.I64ˉgreaterˉequal,
                Wirˉoperation.Boolˉequal => Opcode.Boolˉequal,
                Wirˉoperation.Boolˉnotˉequal => Opcode.Boolˉnotˉequal,
                Wirˉoperation.Boolˉnot => Opcode.Boolˉnot,
                Wirˉoperation.Bytesˉlength => Opcode.Bytesˉlength,
                Wirˉoperation.Bytesˉslice => Opcode.Bytesˉslice,
                Wirˉoperation.Bytesˉreadˉu8 => Opcode.Bytesˉreadˉu8,
                Wirˉoperation.Bytesˉreadˉu16ˉlittle => Opcode.Bytesˉreadˉu16ˉlittle,
                Wirˉoperation.Bytesˉreadˉu32ˉlittle => Opcode.Bytesˉreadˉu32ˉlittle,
                Wirˉoperation.Bytesˉreadˉi32ˉlittle => Opcode.Bytesˉreadˉi32ˉlittle,
                Wirˉoperation.U32ˉadd => Opcode.U32ˉadd,
                Wirˉoperation.U32ˉsubtract => Opcode.U32ˉsubtract,
                Wirˉoperation.U32ˉmultiply => Opcode.U32ˉmultiply,
                Wirˉoperation.U32ˉequal => Opcode.U32ˉequal,
                Wirˉoperation.U32ˉnotˉequal => Opcode.U32ˉnotˉequal,
                Wirˉoperation.U32ˉless => Opcode.U32ˉless,
                Wirˉoperation.U32ˉlessˉequal => Opcode.U32ˉlessˉequal,
                Wirˉoperation.U32ˉgreater => Opcode.U32ˉgreater,
                Wirˉoperation.U32ˉgreaterˉequal => Opcode.U32ˉgreaterˉequal,
                Wirˉoperation.U64ˉadd => Opcode.U64ˉadd,
                Wirˉoperation.U64ˉsubtract => Opcode.U64ˉsubtract,
                Wirˉoperation.U64ˉmultiply => Opcode.U64ˉmultiply,
                Wirˉoperation.U64ˉequal => Opcode.U64ˉequal,
                Wirˉoperation.U64ˉnotˉequal => Opcode.U64ˉnotˉequal,
                Wirˉoperation.U64ˉless => Opcode.U64ˉless,
                Wirˉoperation.U64ˉlessˉequal => Opcode.U64ˉlessˉequal,
                Wirˉoperation.U64ˉgreater => Opcode.U64ˉgreater,
                Wirˉoperation.U64ˉgreaterˉequal => Opcode.U64ˉgreaterˉequal,
                Wirˉoperation.U8ˉequal => Opcode.U8ˉequal,
                Wirˉoperation.U8ˉnotˉequal => Opcode.U8ˉnotˉequal,
                Wirˉoperation.I32ˉdivide => Opcode.I32ˉdivide,
                Wirˉoperation.I32ˉremainder => Opcode.I32ˉremainder,
                Wirˉoperation.U32ˉdivide => Opcode.U32ˉdivide,
                Wirˉoperation.U32ˉremainder => Opcode.U32ˉremainder,
                Wirˉoperation.I64ˉdivide => Opcode.I64ˉdivide,
                Wirˉoperation.I64ˉremainder => Opcode.I64ˉremainder,
                Wirˉoperation.U64ˉdivide => Opcode.U64ˉdivide,
                Wirˉoperation.U64ˉremainder => Opcode.U64ˉremainder,
                Wirˉoperation.U8ˉbitwiseˉand => Opcode.U8ˉbitwiseˉand,
                Wirˉoperation.U8ˉbitwiseˉor => Opcode.U8ˉbitwiseˉor,
                Wirˉoperation.U8ˉbitwiseˉxor => Opcode.U8ˉbitwiseˉxor,
                Wirˉoperation.U8ˉbitwiseˉnot => Opcode.U8ˉbitwiseˉnot,
                Wirˉoperation.U8ˉshiftˉleft => Opcode.U8ˉshiftˉleft,
                Wirˉoperation.U8ˉshiftˉright => Opcode.U8ˉshiftˉright,
                Wirˉoperation.U32ˉbitwiseˉand => Opcode.U32ˉbitwiseˉand,
                Wirˉoperation.U32ˉbitwiseˉor => Opcode.U32ˉbitwiseˉor,
                Wirˉoperation.U32ˉbitwiseˉxor => Opcode.U32ˉbitwiseˉxor,
                Wirˉoperation.U32ˉbitwiseˉnot => Opcode.U32ˉbitwiseˉnot,
                Wirˉoperation.U32ˉshiftˉleft => Opcode.U32ˉshiftˉleft,
                Wirˉoperation.U32ˉshiftˉright => Opcode.U32ˉshiftˉright,
                Wirˉoperation.U64ˉbitwiseˉand => Opcode.U64ˉbitwiseˉand,
                Wirˉoperation.U64ˉbitwiseˉor => Opcode.U64ˉbitwiseˉor,
                Wirˉoperation.U64ˉbitwiseˉxor => Opcode.U64ˉbitwiseˉxor,
                Wirˉoperation.U64ˉbitwiseˉnot => Opcode.U64ˉbitwiseˉnot,
                Wirˉoperation.U64ˉshiftˉleft => Opcode.U64ˉshiftˉleft,
                Wirˉoperation.U64ˉshiftˉright => Opcode.U64ˉshiftˉright,
                Wirˉoperation.Textˉequal => Opcode.Textˉequal,
                Wirˉoperation.Textˉnotˉequal => Opcode.Textˉnotˉequal,
                Wirˉoperation.Bytesˉequal => Opcode.Bytesˉequal,
                Wirˉoperation.Bytesˉnotˉequal => Opcode.Bytesˉnotˉequal,
                Wirˉoperation.Enumˉequal => Opcode.Enumˉequal,
                Wirˉoperation.Enumˉnotˉequal => Opcode.Enumˉnotˉequal,
                Wirˉoperation.Enumˉname => Opcode.Enumˉname,
                Wirˉoperation.I32ˉformat => Opcode.I32ˉformat,
                Wirˉoperation.I64ˉformat => Opcode.I64ˉformat,
                Wirˉoperation.U8ˉformat => Opcode.U8ˉformat,
                Wirˉoperation.U32ˉformat => Opcode.U32ˉformat,
                Wirˉoperation.U64ˉformat => Opcode.U64ˉformat,
                Wirˉoperation.U32ˉfromˉu8 => Opcode.U32ˉfromˉu8,
                Wirˉoperation.Textˉconcat => Opcode.Textˉconcat,
                Wirˉoperation.Textˉutf8ˉisˉvalid => Opcode.Textˉutf8ˉisˉvalid,
                Wirˉoperation.Textˉfromˉutf8 => Opcode.Textˉfromˉutf8,
                Wirˉoperation.Textˉquote => Opcode.Textˉquote,
                Wirˉoperation.Bytesˉconcat => Opcode.Bytesˉconcat,
                Wirˉoperation.Bytesˉfromˉu8 => Opcode.Bytesˉfromˉu8,
                Wirˉoperation.Bytesˉfromˉu16ˉlittle => Opcode.Bytesˉfromˉu16ˉlittle,
                Wirˉoperation.Bytesˉfromˉu32ˉlittle => Opcode.Bytesˉfromˉu32ˉlittle,
                Wirˉoperation.Bytesˉfromˉi32ˉlittle => Opcode.Bytesˉfromˉi32ˉlittle,
                Wirˉoperation.Bytesˉsha256ˉhex => Opcode.Bytesˉsha256ˉhex,
                Wirˉoperation.Textˉtoˉutf8 => Opcode.Textˉtoˉutf8,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
            };
        }
    }
}
