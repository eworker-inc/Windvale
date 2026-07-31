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
            Function.Declaration.Localˉtypes.Length > Nativeˉcontract.MAXIMUM_VALUE_SLOTS ||
            Function.Declaration.Localˉtypes.Any(Type => Type != Valueˉtype.I32) ||
            Function.Declaration.Maximumˉstackˉdepth is < 1 or > Nativeˉcontract.MAXIMUM_VALUE_SLOTS)
        {
            Fail(
                "WVN2002",
                "The baseline native entry must be Main() -> i32 with only bounded i32 temporaries; " +
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
        var Operations = ImmutableArray.CreateBuilder<Nativeˉoperation>();
        var Stack = new Stack<int>();
        var Locals = new int?[function.Declaration.Localˉtypes.Length];
        var Stored = new bool[Locals.Length];
        var Nextˉvalue = 0;

        int Newˉvalue()
        {
            if (Nextˉvalue >= Nativeˉcontract.MAXIMUM_VALUE_SLOTS)
            {
                Fail("WVN2004", "The baseline native function exceeds its value-slot limit.");
            }
            return Nextˉvalue++;
        }

        int Popˉvalue()
        {
            if (Stack.Count == 0)
            {
                Fail("WVN2003", "Verified WVB unexpectedly underflowed during native lowering.");
            }
            return Stack.Pop();
        }

        for (var Index = 0; Index < function.Instructions.Length; Index++)
        {
            var Instruction = function.Instructions[Index];
            switch (Instruction.Opcode)
            {
                case Opcode.I32ˉconst:
                    var Constantˉresult = Newˉvalue();
                    Operations.Add(new Nativeˉi32ˉconstant(Constantˉresult, Instruction.Signedˉoperand));
                    Stack.Push(Constantˉresult);
                    break;
                case Opcode.Localˉstore:
                    var Storeˉindex = checked((int)Instruction.Unsignedˉoperand);
                    if ((uint)Storeˉindex >= (uint)Locals.Length || Stored[Storeˉindex])
                    {
                        Fail("WVN2003", "The baseline native subset requires each i32 temporary to be stored exactly once.");
                    }
                    Locals[Storeˉindex] = Popˉvalue();
                    Stored[Storeˉindex] = true;
                    break;
                case Opcode.Localˉload:
                    var Loadˉindex = checked((int)Instruction.Unsignedˉoperand);
                    if ((uint)Loadˉindex >= (uint)Locals.Length)
                    {
                        Fail("WVN2003", "The baseline native subset requires an initialized i32 temporary load.");
                    }
                    var Loadedˉvalue = Locals[Loadˉindex];
                    if (Loadedˉvalue is null)
                    {
                        Fail("WVN2003", "The baseline native subset requires an initialized i32 temporary load.");
                    }
                    Stack.Push(Loadedˉvalue.Value);
                    break;
                case Opcode.I32ˉadd:
                case Opcode.I32ˉsubtract:
                case Opcode.I32ˉmultiply:
                    var Right = Popˉvalue();
                    var Left = Popˉvalue();
                    var Binaryˉresult = Newˉvalue();
                    Operations.Add(new Nativeˉi32ˉbinary(
                        Binaryˉresult,
                        Instruction.Opcode switch
                        {
                            Opcode.I32ˉadd => Nativeˉi32ˉbinaryˉkind.Add,
                            Opcode.I32ˉsubtract => Nativeˉi32ˉbinaryˉkind.Subtract,
                            _ => Nativeˉi32ˉbinaryˉkind.Multiply,
                        },
                        Left,
                        Right));
                    Stack.Push(Binaryˉresult);
                    break;
                case Opcode.I32ˉnegate:
                    var Negatedˉvalue = Popˉvalue();
                    var Negateˉresult = Newˉvalue();
                    Operations.Add(new Nativeˉi32ˉnegate(Negateˉresult, Negatedˉvalue));
                    Stack.Push(Negateˉresult);
                    break;
                case Opcode.Return:
                    if (Index != function.Instructions.Length - 1 || Stack.Count != 1)
                    {
                        Fail("WVN2003", "The baseline native subset requires one final i32 return.");
                    }
                    Operations.Add(new Nativeˉreturn(Popˉvalue()));
                    break;
                default:
                    Fail(
                        "WVN2003",
                        $"The baseline native subset does not support verified opcode '{Instruction.Opcode}'.");
                    break;
            }
        }

        if (Operations.Count < 2 || Operations[^1] is not Nativeˉreturn ||
            Stack.Count != 0 || Stored.Any(Value => !Value))
        {
            Fail("WVN2003", "The baseline native subset requires a complete single-return temporary graph.");
        }
        return new([new("Main", Operations.ToImmutable())]);
    }

    private static Nativeˉfragment Selectˉx64(Nativeˉmodule module)
    {
        if (module.Functions.Length != 1 ||
            module.Functions[0] is not { Name: "Main", Operations.Length: >= 2 })
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }
        var Function = module.Functions[0];
        if (Function.Operations is
            [Nativeˉi32ˉconstant Singleˉconstant, Nativeˉreturn Singleˉreturn] &&
            Singleˉreturn.Value == Singleˉconstant.Result)
        {
            return Selectˉconstant(Singleˉconstant.Value);
        }

        var Valueˉcount = 0;
        var Checkedˉoperationˉcount = 0;
        Nativeˉreturn? Returnˉoperation = null;
        for (var Index = 0; Index < Function.Operations.Length; Index++)
        {
            switch (Function.Operations[Index])
            {
                case Nativeˉi32ˉconstant Operationˉconstant when Operationˉconstant.Result == Valueˉcount:
                    Valueˉcount++;
                    break;
                case Nativeˉi32ˉbinary Binary when
                    Binary.Result == Valueˉcount &&
                    Enum.IsDefined(Binary.Kind) &&
                    Binary.Left >= 0 && Binary.Left < Valueˉcount &&
                    Binary.Right >= 0 && Binary.Right < Valueˉcount:
                    Valueˉcount++;
                    Checkedˉoperationˉcount++;
                    break;
                case Nativeˉi32ˉnegate Negate when
                    Negate.Result == Valueˉcount &&
                    Negate.Value >= 0 && Negate.Value < Valueˉcount:
                    Valueˉcount++;
                    Checkedˉoperationˉcount++;
                    break;
                case Nativeˉreturn Operationˉreturn when
                    Index == Function.Operations.Length - 1 &&
                    Operationˉreturn.Value >= 0 && Operationˉreturn.Value < Valueˉcount:
                    Returnˉoperation = Operationˉreturn;
                    break;
                default:
                    Fail("WVN2901", "The x86-64 selector received an invalid native value graph.");
                    break;
            }
        }
        if (Valueˉcount is < 1 or > Nativeˉcontract.MAXIMUM_VALUE_SLOTS ||
            Checkedˉoperationˉcount == 0 || Returnˉoperation is null)
        {
            Fail("WVN2901", "The x86-64 selector requires a bounded checked-arithmetic value graph.");
        }

        var Frameˉbytes = checked((Valueˉcount * sizeof(int) + 15) & ~15);
        var Code = new List<byte>();
        var Overflowˉpatches = new List<int>();
        Emitˉframeˉadjustment(Code, subtract: true, Frameˉbytes);
        foreach (var Operation in Function.Operations)
        {
            switch (Operation)
            {
                case Nativeˉi32ˉconstant Operationˉconstant:
                    Code.Add(0xB8);
                    Addˉi32(Code, Operationˉconstant.Value);
                    Emitˉstoreˉeax(Code, Operationˉconstant.Result);
                    break;
                case Nativeˉi32ˉbinary Binary:
                    Emitˉloadˉeax(Code, Binary.Left);
                    Emitˉloadˉecx(Code, Binary.Right);
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
                    Emitˉstoreˉeax(Code, Binary.Result);
                    break;
                case Nativeˉi32ˉnegate Negate:
                    Emitˉloadˉeax(Code, Negate.Value);
                    Code.AddRange([0xF7, 0xD8]);
                    Emitˉoverflowˉbranch(Code, Overflowˉpatches);
                    Emitˉstoreˉeax(Code, Negate.Result);
                    break;
            }
        }

        Emitˉloadˉeax(Code, Returnˉoperation.Value);
        Emitˉframeˉadjustment(Code, subtract: false, Frameˉbytes);
        Code.Add(0xC3);
        var Trapˉoffset = Code.Count;
        Emitˉframeˉadjustment(Code, subtract: false, Frameˉbytes);
        Code.AddRange([0x48, 0xB8]);
        Addˉu64(Code, INTEGER_OVERFLOW_STATUS);
        Code.Add(0xC3);

        var Bytes = Code.ToArray();
        foreach (var Patchˉoffset in Overflowˉpatches)
        {
            var Displacement = checked(Trapˉoffset - (Patchˉoffset + sizeof(int)));
            BinaryPrimitives.WriteInt32LittleEndian(Bytes.AsSpan(Patchˉoffset, sizeof(int)), Displacement);
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

    private static void Emitˉframeˉadjustment(List<byte> code, bool subtract, int bytes)
    {
        code.AddRange([0x48, 0x81, subtract ? (byte)0xEC : (byte)0xC4]);
        Addˉi32(code, bytes);
    }

    private static void Emitˉloadˉeax(List<byte> code, int value)
    {
        code.AddRange([0x8B, 0x84, 0x24]);
        Addˉi32(code, checked(value * sizeof(int)));
    }

    private static void Emitˉloadˉecx(List<byte> code, int value)
    {
        code.AddRange([0x8B, 0x8C, 0x24]);
        Addˉi32(code, checked(value * sizeof(int)));
    }

    private static void Emitˉstoreˉeax(List<byte> code, int value)
    {
        code.AddRange([0x89, 0x84, 0x24]);
        Addˉi32(code, checked(value * sizeof(int)));
    }

    private static void Emitˉoverflowˉbranch(List<byte> code, List<int> patches)
    {
        code.AddRange([0x0F, 0x80]);
        patches.Add(code.Count);
        Addˉi32(code, 0);
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

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
