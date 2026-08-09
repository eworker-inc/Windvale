using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static readonly string WVB_TO_WVO_RECORD_CAPABILITY_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Wvb-To-Wvo-Record-Capability.wv");
    private static readonly string NATIVE_RECORD_STORAGE_PACKED_RESULT_SOURCE =
        Readˉembeddedˉsource(
            "Windvale.Seed.Tests.Native-Record-Storage-Packed-Result.wv");

    private static void Nativeˉrecordˉcapabilityˉloweringˉagrees()
    {
        var Codecˉcompilation = Seedˉcompiler.Compileˉmodules(
            new(
                "Tests/Fixtures/Native-X64/Native-Record-Storage-Packed-Result.wv",
                NATIVE_RECORD_STORAGE_PACKED_RESULT_SOURCE),
            [
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Storage.wv",
                    NATIVE_X64_LOWERING_RECORD_STORAGE_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Types.wv",
                    NATIVE_X64_LOWERING_TYPES_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Layout.wv",
                    NATIVE_X64_LOWERING_LAYOUT_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Capabilities.wv",
                    NATIVE_X64_LOWERING_CAPABILITIES_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Allocation.wv",
                    NATIVE_X64_LOWERING_RECORD_ALLOCATION_SOURCE),
                new(
                    "Compiler/Windvale/Native-X64-Lowering-Record-Local-Liveness.wv",
                    NATIVE_X64_LOWERING_RECORD_LOCAL_LIVENESS_SOURCE),
            ]);
        if (!Codecˉcompilation.Success)
        {
            throw new InvalidOperationException(
                "Windvale record-storage packed-result compilation failed: " +
                string.Join(" | ", Codecˉcompilation.Diagnostics));
        }
        var Codec = Moduleˉcodec.Readˉandˉverify(
            Codecˉcompilation.Moduleˉbytes.AsSpan());
        Equal(
            42,
            new Referenceˉruntime(
                Codec,
                new Referenceˉcapabilityˉhost(TextWriter.Null),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
        Equal(
            42,
            X64ˉnativeˉexecutor.Executeˉi32(
                X64ˉnativeˉbackend.Compile(Codec).Fragment));

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
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉshiftˉleft),
            "The record-capability fixture omitted u32.shift_left.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉshiftˉright),
            "The record-capability fixture omitted u32.shift_right.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉbitwiseˉand),
            "The record-capability fixture omitted u32.bitwise_and.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉbitwiseˉor),
            "The record-capability fixture omitted u32.bitwise_or.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉbitwiseˉxor),
            "The record-capability fixture omitted u32.bitwise_xor.");
        True(
            Module.Functions.SelectMany(Function => Function.Instructions)
                .Any(Instruction => Instruction.Opcode == Opcode.U32ˉbitwiseˉnot),
            "The record-capability fixture omitted u32.bitwise_not.");

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
        Assertˉlargeˉrecordˉplannerˉenvelopeˉlowering(Tool, Memory);
    }
}
