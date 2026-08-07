using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_U32_BITWISE_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-U32-Bitwise.wv");

    private static void Nativeˉu32ˉbitwiseˉloweringˉagrees()
    {
        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Memory = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉmemoryˉsuccess());

        Assertˉu32ˉbitwiseˉcase(
            Tool,
            Memory,
            "and",
            "return Left & Right;");
        Assertˉu32ˉbitwiseˉcase(
            Tool,
            Memory,
            "not",
            "return ~Left;");
        Assertˉu32ˉbitwiseˉcase(
            Tool,
            Memory,
            "shift",
            "return Left << Right;");

        var Wvb = Compileˉsuccess(WVB_TO_WVO_U32_BITWISE_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Opcodes = Module.Functions
            .SelectMany(Function => Function.Instructions)
            .Select(Instruction => Instruction.Opcode)
            .ToHashSet();
        Opcode[] Required =
        [
            Opcode.U32ˉbitwiseˉand,
            Opcode.U32ˉbitwiseˉor,
            Opcode.U32ˉbitwiseˉxor,
            Opcode.U32ˉbitwiseˉnot,
            Opcode.U32ˉshiftˉleft,
            Opcode.U32ˉshiftˉright,
        ];
        foreach (var Opcode in Required)
        {
            True(Opcodes.Contains(Opcode),
                $"The unsigned-bitwise fixture omitted {Opcode}.");
        }

        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Wvb,
            Module,
            Interpreted.Executedˉinstructions,
            expectedˉexitˉcode: 42);

        var Invalidˉshiftˉwvb = Compileˉsuccess("""
            module Wvbˉtoˉwvoˉu32ˉinvalidˉshift profile portable;
            fn Shift(Value: u32, Count: u32) -> u32 {
                return Value << Count;
            }
            export fn Main() -> i32 {
                let Value: u32 = Shift(7u32, 32u32);
                if Value == 0u32 { return 1; }
                return 2;
            }
            """);
        var Invalidˉshiftˉmodule = Moduleˉcodec.Readˉandˉverify(
            Invalidˉshiftˉwvb);
        Throwsˉruntime(
            "WVR3033",
            () => _ = new Referenceˉruntime(
                Invalidˉshiftˉmodule,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        var Invalidˉshiftˉnative = X64ˉnativeˉbackend.Compile(
            Invalidˉshiftˉmodule);
        Throwsˉnativeˉtrap(
            "WVR3033",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(
                Invalidˉshiftˉnative.Fragment));
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Invalidˉshiftˉwvb,
            Invalidˉshiftˉmodule,
            maximumˉinstructions: 100_000,
            expectedˉexitˉcode: null);
    }

    private static void Assertˉu32ˉbitwiseˉcase(
        Verifiedˉmodule Tool,
        Verifiedˉmodule Memory,
        string Name,
        string Operation)
    {
        var Wvb = Compileˉsuccess($$"""
            module Wvbˉtoˉwvoˉu32ˉ{{Name}} profile portable;
            fn Apply(Left: u32, Right: u32) -> u32 {
                {{Operation}}
            }
            export fn Main() -> i32 {
                let Value: u32 = Apply(7u32, 1u32);
                if Value == 0u32 { return 1; }
                return 42;
            }
            """);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Wvb,
            Module,
            Interpreted.Executedˉinstructions,
            expectedˉexitˉcode: 42);
    }
}
