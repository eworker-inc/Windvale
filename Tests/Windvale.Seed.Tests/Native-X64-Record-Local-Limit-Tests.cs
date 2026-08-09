using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int COMPILER_DECLARED_RECORD_LOCALS = 674;

    private static void Nativeˉrecordˉlocalˉenvelopeˉlowers()
    {
        const string Source = """
            module Testsˉnativeˉx64ˉrecordˉlocalˉenvelope profile portable;
            record Cell { Value: i32; }
            fn Make() -> Cell { return Cell(42); }
            export fn Main() -> i32 {
                let Result: Cell = Make();
                return Result.Value;
            }
            """;
        var Template = Moduleˉcodec.Read(Compileˉsuccess(Source));
        var Helper = Template.Functions.Single(Function => Function.Name == "Make");
        var Existingˉrecordˉlocals = Helper.Localˉtypes.Count(
            Type => Type.Kind == Valueˉtype.Record);
        var Recordˉlocals = Enumerable.Repeat(
            Valueˉshape.Forˉrecord(0),
            COMPILER_DECLARED_RECORD_LOCALS - Existingˉrecordˉlocals)
            .ToImmutableArray();
        var Wvb = Moduleˉcodec.Write(Template with
        {
            Functions = Template.Functions
                .Select(Function => Function == Helper
                    ? Function with
                    {
                        Localˉtypes = Function.Localˉtypes.AddRange(Recordˉlocals),
                    }
                    : Function)
                .ToImmutableArray(),
        });
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        var Verifiedˉhelper = Module.Functions.Single(
            Function => Function.Declaration.Name == "Make");
        Equal(
            COMPILER_DECLARED_RECORD_LOCALS,
            Verifiedˉhelper.Declaration.Localˉtypes.Count(
                Type => Type.Kind == Valueˉtype.Record));

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);

        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Result = Runˉnativeˉx64ˉloweringˉtool(
            Tool,
            Wvb,
            maximumˉinstructions: 500_000_000);
        Equal(0, Result.Exitˉcode);
        Equal(string.Empty, Result.Diagnostics);
        Sequenceˉequal(Expectedˉobject, Result.Writtenˉbytes);
    }
}
