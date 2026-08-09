using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int COMPILER_RECORD_DECLARATIONS = 63;
    private const int COMPILER_ENUM_DECLARATIONS = 16;

    private static void Nativeˉnominalˉtypeˉenvelopeˉlowers()
    {
        var Source = Buildˉnominalˉtypeˉenvelopeˉsource(
            COMPILER_RECORD_DECLARATIONS,
            COMPILER_ENUM_DECLARATIONS);
        var Wvb = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(
            COMPILER_RECORD_DECLARATIONS + COMPILER_ENUM_DECLARATIONS,
            Module.Module.Types.Length);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);

        var Tool = Moduleˉcodec.Readˉandˉverify(
            Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Result = Runˉnativeˉx64ˉloweringˉtool(
            Tool,
            Wvb,
            maximumˉinstructions: 100_000_000);
        Equal(0, Result.Exitˉcode);
        Equal(string.Empty, Result.Diagnostics);
        Sequenceˉequal(Expectedˉobject, Result.Writtenˉbytes);
    }

    private static string Buildˉnominalˉtypeˉenvelopeˉsource(
        int records,
        int enums)
    {
        var Source = new StringBuilder(
            "module Testsˉnativeˉx64ˉnominalˉtypeˉenvelope profile portable;\n\n");
        for (var Index = 0; Index < records; Index++)
        {
            Source.Append("record RecordˉR");
            Appendˉthreeˉdigits(Source, Index);
            Source.Append(" { Value: u32; }\n");
        }
        for (var Index = 0; Index < enums; Index++)
        {
            Source.Append("enum EnumˉE");
            Appendˉthreeˉdigits(Source, Index);
            Source.Append(" { Zero = 0; One = 1; }\n");
        }
        Source.Append("\nfn Keep(Value: EnumˉE015) -> EnumˉE015 { return Value; }\n");
        Source.Append("export fn Main() -> i32 {\n");
        Source.Append("    if Keep(EnumˉE015.One) != EnumˉE015.One { return 1; }\n");
        Source.Append("    return 42;\n}\n");
        return Source.ToString();
    }

    private static void Appendˉthreeˉdigits(StringBuilder output, int value) =>
        output.Append(value.ToString(
            "D3",
            System.Globalization.CultureInfo.InvariantCulture));
}
