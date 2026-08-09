using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int COMPILER_STATIC_DATA_DECLARATIONS = 110;

    private static void Nativeˉstaticˉdataˉenvelopeˉlowers()
    {
        var Source = Buildˉstaticˉdataˉenvelopeˉsource(
            COMPILER_STATIC_DATA_DECLARATIONS);
        var Wvb = Compileˉsuccess(Source);
        var Module = Moduleˉcodec.Readˉandˉverify(Wvb);
        Equal(COMPILER_STATIC_DATA_DECLARATIONS, Module.Module.Data.Length);

        var Native = X64ˉnativeˉbackend.Compile(Module);
        _ = Nativeˉfragmentˉverifier.Verify(Native.Fragment);
        var Expectedˉobject = Nativeˉobjectˉsink.Writeˉwvo(Native.Fragment);
        var Expectedˉview = Objectˉcodec.Readˉandˉverify(
            Expectedˉobject.AsSpan()).Value;
        True(
            Expectedˉview.Symbols.Any(Symbol =>
                StringComparer.Ordinal.Equals(Symbol.Name, "$data_0109")),
            "The compiler-sized static-data envelope omitted its final symbol.");

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

    private static string Buildˉstaticˉdataˉenvelopeˉsource(int declarations)
    {
        var Source = new StringBuilder(
            "module Testsˉnativeˉx64ˉstaticˉdataˉenvelope profile portable;\n\n");
        for (var Index = 0; Index < declarations; Index++)
        {
            Source.Append("data ValueˉD");
            Source.Append(Index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture));
            Source.Append(": bytes = [");
            Source.Append((Index % 256).ToString(System.Globalization.CultureInfo.InvariantCulture));
            Source.Append("];\n");
        }
        Source.Append("\nexport fn Main() -> i32 { return 42; }\n");
        return Source.ToString();
    }
}
