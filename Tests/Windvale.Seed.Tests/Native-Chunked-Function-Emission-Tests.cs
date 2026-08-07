using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉchunkedˉfunctionˉemissionˉagrees()
    {
        var Wvb = Buildˉchunkedˉfunctionˉfixture();
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Native = X64ˉnativeˉbackend.Compile(Module);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;
        True(
            Expectedˉview.Sections[0].Data.Length > 8_192,
            "The chunked-emission fixture did not cross two 4-KiB boundaries.");

        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Memory = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉmemoryˉsuccess());
        Assertˉu32ˉloweringˉobject(
            Tool,
            Memory,
            Wvb,
            Module,
            maximumˉinstructions: 100_000,
            expectedˉexitˉcode: 0);
    }

    private static byte[] Buildˉchunkedˉfunctionˉfixture()
    {
        const int LOCAL_COUNT = 192;
        var Template = Moduleˉcodec.Read(
            Compileˉsuccess(WVB_TO_WVO_RETURN_42_SOURCE));
        var Code = ImmutableArray.CreateBuilder<byte>();
        for (uint Local = 1; Local <= LOCAL_COUNT; Local++)
        {
            Code.AddRange(U32ˉinstruction(Opcode.Localˉload, Local - 1));
            Code.AddRange(U32ˉinstruction(Opcode.Localˉstore, Local));
        }
        Code.AddRange(U32ˉinstruction(Opcode.Localˉload, LOCAL_COUNT));
        Code.Add((byte)Opcode.Return);
        var Function = Template.Functions.Single() with
        {
            Localˉtypes = Enumerable.Repeat<Valueˉshape>(
                Valueˉtype.I32,
                LOCAL_COUNT + 1).ToImmutableArray(),
            Codeˉoffset = 0,
            Codeˉlength = Code.Count,
            Maximumˉstackˉdepth = 1,
        };
        return Moduleˉcodec.Write(Template with
        {
            Functions = [Function],
            Code = Code.ToImmutable(),
        });
    }
}
