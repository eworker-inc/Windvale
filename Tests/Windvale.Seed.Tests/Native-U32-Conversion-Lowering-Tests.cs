using Windvale.Bytecode;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_U32_CONVERSION_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-U32-Conversion.wv");

    private static void Nativeˉu32ˉconversionˉloweringˉagrees()
    {
        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Memory = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉmemoryˉsuccess());
        var Wvb = Compileˉsuccess(WVB_TO_WVO_U32_CONVERSION_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        True(Module.Functions.SelectMany(Function => Function.Instructions)
            .Any(Instruction => Instruction.Opcode == Opcode.U32ˉfromˉu8),
            "The unsigned-conversion fixture omitted u32.from_u8.");

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
