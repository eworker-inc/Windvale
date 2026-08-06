using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_RECORD_CAPABILITY_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Record-Capability.wv");

    private static void Nativeˉrecordˉcapabilityˉloweringˉagrees()
    {
        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Memory = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉmemoryˉsuccess());
        var Wvb = Compileˉsuccess(WVB_TO_WVO_RECORD_CAPABILITY_SOURCE);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(Moduleˉprofile.Hosted, Module.Module.Profile);
        Equal(1, Module.Module.Capabilities.Length);
        Equal(
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT,
            Module.Module.Capabilities[0].Name);
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.Callˉcapability),
            "The record-capability fixture omitted call.capability.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.Recordˉcreate),
            "The record-capability fixture omitted record.create.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉformat),
            "The record-capability fixture omitted u32.format.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉadd),
            "The record-capability fixture omitted u32.add.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉsubtract),
            "The record-capability fixture omitted u32.subtract.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉmultiply),
            "The record-capability fixture omitted u32.multiply.");

        var Authorized = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT);
        var Resources = new Hostedˉresourceˉcontext(
            ["first", "second"],
            TextWriter.Null,
            TextWriter.Null);
        var Interpreted = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(Resources),
            new(Authorized)).Runˉmain();
        Equal(42, Interpreted.Exitˉcode);
        var Native = X64ˉnativeˉbackend.Compile(Module);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                Native.Fragment,
                maximumˉinstructions: Interpreted.Executedˉinstructions,
                hostˉservices: new(null, Authorized, Resources)));
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Wvb,
            Module,
            maximumˉinstructions: Interpreted.Executedˉinstructions,
            expectedˉexitˉcode: null);
    }
}
