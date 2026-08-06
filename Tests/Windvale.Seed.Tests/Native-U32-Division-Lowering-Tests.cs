using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_U32_DIVISION_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-U32-Division.wv");

    private static void Nativeˉu32ˉdivisionˉloweringˉagrees()
    {
        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Memory = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉmemoryˉsuccess());
        var Wvb = Compileˉsuccess(WVB_TO_WVO_U32_DIVISION_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Opcodes = Module.Functions
            .SelectMany(Function => Function.Instructions)
            .Select(Instruction => Instruction.Opcode)
            .ToHashSet();
        True(Opcodes.Contains(Opcode.U32ˉdivide),
            "The unsigned-division fixture omitted u32.divide.");
        True(Opcodes.Contains(Opcode.U32ˉremainder),
            "The unsigned-division fixture omitted u32.remainder.");

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

        var Zeroˉwvb = Compileˉsuccess("""
            module Wvbˉtoˉwvoˉu32ˉdivisionˉzero profile portable;
            fn Divide(Value: u32, Divisor: u32) -> u32 {
                return Value / Divisor;
            }
            export fn Main() -> i32 {
                let Value: u32 = Divide(7u32, 0u32);
                if Value == 0u32 { return 1; }
                return 2;
            }
            """);
        var Zeroˉmodule = Moduleˉcodec.Readˉandˉverify(Zeroˉwvb);
        Throwsˉruntime(
            "WVR3032",
            () => _ = new Referenceˉruntime(
                Zeroˉmodule,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        var Zeroˉnative = X64ˉnativeˉbackend.Compile(Zeroˉmodule);
        Throwsˉnativeˉtrap(
            "WVR3032",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(Zeroˉnative.Fragment));
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Zeroˉwvb,
            Zeroˉmodule,
            maximumˉinstructions: 100_000,
            expectedˉexitˉcode: null);
    }
}
