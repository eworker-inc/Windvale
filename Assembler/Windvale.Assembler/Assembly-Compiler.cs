using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Assembler;

public static class Assemblyˉcompiler
{
    public const int FORMAT_VERSION = 1;

    public static Assemblyˉresult Assemble(string source)
    {
        var Parsed = Assemblyˉparser.Parse(source);
        if (Parsed.Diagnostic is not null)
        {
            return Assemblyˉresult.Failed(Parsed.Diagnostic);
        }

        try
        {
            return Assemblyˉresult.Succeeded(
                ImmutableArray.Create(X64ˉobjectˉencoder.Encode(Parsed.Unit!)));
        }
        catch (Assemblyˉencodingˉexception Exception)
        {
            return Assemblyˉresult.Failed(Exception.Diagnostic);
        }
        catch (Objectˉexception Exception)
        {
            return Assemblyˉresult.Failed(
                new("WVA1011", 1, 1, $"Generated object is invalid: {Exception.Message}"));
        }
        catch (OverflowException)
        {
            return Assemblyˉresult.Failed(
                new("WVA1011", 1, 1, "Assembly size arithmetic overflowed a defined limit."));
        }
    }
}
