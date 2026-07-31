using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class X64ˉnativeˉbackend
{
    public static Nativeˉcompilation Compile(Verifiedˉmodule verifiedˉmodule)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        var Module = verifiedˉmodule.Module;
        if (Module.Profile != Moduleˉprofile.Portable ||
            !Module.Capabilities.IsEmpty ||
            !Module.Data.IsEmpty ||
            !Module.Types.IsEmpty)
        {
            Fail("WVN2001", "The first general native subset requires a portable module without capabilities, data, or nominal types.");
        }
        if (verifiedˉmodule.Functions.Length != 1 ||
            Module.Exports.Length != 1 ||
            Module.Exports[0] is not { Name: "Main", Kind: Exportˉkind.Function, Targetˉindex: 0 })
        {
            Fail("WVN2002", "The first general native subset requires exactly one exported Main function.");
        }

        var Function = verifiedˉmodule.Functions[0];
        if (!StringComparer.Ordinal.Equals(Function.Declaration.Name, "Main") ||
            !Function.Declaration.Parameterˉtypes.IsEmpty ||
            Function.Declaration.Returnˉtype != Valueˉtype.I32 ||
            Function.Declaration.Localˉtypes.Length != 1 ||
            Function.Declaration.Localˉtypes[0] != Valueˉtype.I32 ||
            Function.Declaration.Maximumˉstackˉdepth != 1)
        {
            Fail(
                "WVN2002",
                "The first general native entry must be Main() -> i32 with one canonical i32 temporary and stack depth one; " +
                $"found name='{Function.Declaration.Name}', parameters={Function.Declaration.Parameterˉtypes.Length}, " +
                $"return={Function.Declaration.Returnˉtype}, locals={Function.Declaration.Localˉtypes.Length}, " +
                $"max-stack={Function.Declaration.Maximumˉstackˉdepth}.");
        }
        if (Function.Instructions.Length != 4 ||
            Function.Instructions[0].Opcode != Opcode.I32ˉconst ||
            Function.Instructions[1] is not { Opcode: Opcode.Localˉstore, Unsignedˉoperand: 0 } ||
            Function.Instructions[2] is not { Opcode: Opcode.Localˉload, Unsignedˉoperand: 0 } ||
            Function.Instructions[3].Opcode != Opcode.Return)
        {
            Fail(
                "WVN2003",
                "The first general native subset supports only the canonical i32.const, local.store 0, local.load 0, return sequence.");
        }

        const int Value = 0;
        var Nativeˉfunction = new Nativeˉfunction(
            "Main",
            [
                new Nativeˉi32ˉconstant(Value, Function.Instructions[0].Signedˉoperand),
                new Nativeˉreturn(Value),
            ]);
        var Nativeˉmodule = new Nativeˉmodule([Nativeˉfunction]);
        var Fragment = Selectˉx64(Nativeˉmodule);
        return new(Nativeˉmodule, Fragment);
    }

    private static Nativeˉfragment Selectˉx64(Nativeˉmodule module)
    {
        if (module.Functions.Length != 1 ||
            module.Functions[0] is not { Name: "Main", Operations.Length: 2 })
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }
        var Function = module.Functions[0];
        if (Function.Operations[0] is not Nativeˉi32ˉconstant ||
            Function.Operations[1] is not Nativeˉreturn)
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }
        var Constant = (Nativeˉi32ˉconstant)Function.Operations[0];
        var Return = (Nativeˉreturn)Function.Operations[1];
        if (Return.Value != Constant.Result)
        {
            Fail("WVN2901", "The x86-64 selector received an unsupported native machine-IR shape.");
        }

        var Code = new byte[6];
        Code[0] = 0xB8;
        BinaryPrimitives.WriteInt32LittleEndian(Code.AsSpan(1, sizeof(int)), Constant.Value);
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

    [DoesNotReturn]
    private static void Fail(string code, string message) =>
        throw new Nativeˉbackendˉexception(code, message);
}
