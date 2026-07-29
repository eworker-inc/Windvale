using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

public static class Seedˉcompiler
{
    public const int MAX_SOURCE_CHARACTERS = 4 * 1024 * 1024;

    public static Compilationˉresult Compile(string source, string sourceˉname = "<memory>")
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceˉname);
        var Diagnostics = new Diagnosticˉbag();
        if (source.Length > MAX_SOURCE_CHARACTERS)
        {
            Diagnostics.Report(
                "WVC0001",
                "input",
                new(0, 0, 1, 1),
                $"Source '{sourceˉname}' exceeds the {MAX_SOURCE_CHARACTERS} character limit.");
            return new([], Diagnostics.Toˉimmutable());
        }

        var Parser = new Sourceˉparser(source, Diagnostics);
        var Syntax = Parser.Parseˉmodule();
        if (Diagnostics.Count != 0)
        {
            return new([], Diagnostics.Toˉimmutable());
        }

        var Wir = Semanticˉcompiler.Compile(Syntax, Diagnostics);
        if (Diagnostics.Count != 0)
        {
            return new([], Diagnostics.Toˉimmutable());
        }

        try
        {
            var Module = Bytecodeˉlowering.Lower(Wir);
            var Bytes = Moduleˉcodec.Write(Module);
            _ = Moduleˉcodec.Readˉandˉverify(Bytes);
            return new(Bytes.ToImmutableArray(), []);
        }
        catch (Bytecodeˉexception Exception)
        {
            Diagnostics.Report(
                "WVC9000",
                "backend",
                new(0, 0, 1, 1),
                $"The generated module failed validation: {Exception.Message}");
            return new([], Diagnostics.Toˉimmutable());
        }
    }
}
