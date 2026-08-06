using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_U32_ARITHMETIC_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-U32-Arithmetic.wv");

    private static void Nativeˉu32ˉarithmeticˉloweringˉagrees()
    {
        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Memory = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉmemoryˉsuccess());

        var Wvb = Compileˉsuccess(WVB_TO_WVO_U32_ARITHMETIC_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Opcodes = Module.Functions
            .SelectMany(Function => Function.Instructions)
            .Select(Instruction => Instruction.Opcode)
            .ToHashSet();
        True(Opcodes.Contains(Opcode.U32ˉadd),
            "The unsigned-arithmetic fixture omitted u32.add.");
        True(Opcodes.Contains(Opcode.U32ˉsubtract),
            "The unsigned-arithmetic fixture omitted u32.subtract.");
        True(Opcodes.Contains(Opcode.U32ˉmultiply),
            "The unsigned-arithmetic fixture omitted u32.multiply.");

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

        var Overflowˉwvb = Compileˉsuccess("""
            module Wvbˉtoˉwvoˉu32ˉmultiplyˉoverflow profile portable;
            export fn Main() -> i32 {
                let Product: u32 = 65536u32 * 65536u32;
                if Product == 0u32 { return 1; }
                return 2;
            }
            """);
        var Overflowˉmodule = Moduleˉcodec.Readˉandˉverify(Overflowˉwvb);
        True(Overflowˉmodule.Functions.SelectMany(Function => Function.Instructions)
            .Any(Instruction => Instruction.Opcode == Opcode.U32ˉmultiply),
            "The overflow fixture omitted u32.multiply.");
        Throwsˉruntime(
            "WVR3007",
            () => _ = new Referenceˉruntime(
                Overflowˉmodule,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain());
        var Overflowˉnative = X64ˉnativeˉbackend.Compile(Overflowˉmodule);
        Throwsˉnativeˉtrap(
            "WVR3007",
            () => _ = X64ˉnativeˉexecutor.Executeˉi32(Overflowˉnative.Fragment));
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Overflowˉwvb,
            Overflowˉmodule,
            maximumˉinstructions: 100_000,
            expectedˉexitˉcode: null);
    }

    private static void Assertˉu32ˉloweringˉobject(
        Verifiedˉmodule Tool,
        Verifiedˉmodule Memory,
        byte[] Wvb,
        Verifiedˉmodule Module,
        long maximumˉinstructions,
        int? expectedˉexitˉcode)
    {
        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        if (expectedˉexitˉcode is int Expectedˉexitˉcode)
        {
            Equal(
                Expectedˉexitˉcode,
                X64ˉnativeˉexecutor.Executeˉi32(
                    Native.Fragment,
                    maximumˉinstructions: maximumˉinstructions));
        }
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;

        var Memoryˉresult = new Referenceˉruntime(
            Memory,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults with
            {
                Maximumˉinstructions = 100_000_000,
            }).Runˉmainˉbytes(Wvb.ToImmutableArray());
        Sequenceˉequal(Expectedˉobject, Memoryˉresult.Bytes);

        var Toolˉresult = Runˉnativeˉx64ˉloweringˉtool(
            Tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(0, Toolˉresult.Exitˉcode);
        Equal(string.Empty, Toolˉresult.Diagnostics);
        Equal(
            $"native x64 status=Valid abi=22 " +
            $"code-bytes={Expectedˉview.Sections[0].Data.Length} " +
            $"object-bytes={Expectedˉobject.Length}\n",
            Toolˉresult.Output);
        Sequenceˉequal(Expectedˉobject, Toolˉresult.Writtenˉbytes);
    }
}
