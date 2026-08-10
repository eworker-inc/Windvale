using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.Runtime;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Compilerˉsourceˉwvbˉexportˉdirectoryˉregression()
    {
        const string Source = """
            module Sourceˉwvbˉexports profile portable;

            export fn Alpha() -> i32 { return 1; }
            fn Hidden() -> i32 { return 39; }
            export fn Macronˉvalue() -> i32 { return Hidden(); }
            export fn Main() -> i32 { return Alpha() + Macronˉvalue() + 2; }
            """;

        var Tool = EXACT_COMPILER_WVB.Value.Module;
        var Sourceˉbytes = Encoding.UTF8.GetBytes(Source).ToImmutableArray();
        var Output = new StringWriter();
        var Diagnostics = new StringWriter();
        var Writer = new Capturingˉfileˉwriter();
        var Reader = new Testˉfileˉreader((Name, Maximumˉbytes) =>
        {
            Equal("exports.wv", Name);
            True(Sourceˉbytes.Length <= Maximumˉbytes,
                "The source-to-WVB hosted byte limit was too small for exports.");
            return Sourceˉbytes;
        });
        var Authorized = Tool.Module.Capabilities
            .Select(Capability => Capability.Name)
            .ToImmutableHashSet(StringComparer.Ordinal);
        var Result = new Referenceˉruntime(
            Tool,
            new Referenceˉcapabilityˉhost(new Hostedˉresourceˉcontext(
                ["exports.wv", "exports.wvb"],
                Output,
                Diagnostics,
                Reader,
                Writer)),
            new(Authorized, Maximumˉinstructions: 4_000_000_000)).Runˉmain();

        Equal(0, Result.Exitˉcode);
        Equal(string.Empty, Diagnostics.ToString());
        Contains(Output.ToString(), "source wvb status=Valid");
        Equal(1, Reader.Readˉcount);
        Equal(1, Writer.Writeˉcount);
        Equal("exports.wvb", Writer.Resourceˉname);

        var Stageˉzero = Compileˉsuccess(Source);
        Sequenceˉequal(Stageˉzero, Writer.Bytes);
        var Generated = Moduleˉcodec.Readˉandˉverify(Writer.Bytes.AsSpan());
        Sequenceˉequal(
            ["Alpha", "Macronˉvalue", "Main"],
            Generated.Module.Exports.Select(Export => Export.Name));
        True(
            Generated.Module.Functions.Any(Function => Function.Name == "Hidden"),
            "The private function disappeared while constructing export metadata.");
        Equal(
            42,
            new Referenceˉruntime(
                Generated,
                new Referenceˉcapabilityˉhost(new StringWriter()),
                Runtimeˉoptions.Portableˉdefaults).Runˉmain().Exitˉcode);
    }
}
