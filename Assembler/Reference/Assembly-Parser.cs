using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Windvale.ObjectModel;

namespace Windvale.Assembler;

internal sealed record Assemblyˉparseˉresult(
    Assemblyˉunit? Unit,
    Assemblyˉdiagnostic? Diagnostic);

internal static class Assemblyˉparser
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Assemblyˉparseˉresult Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        try
        {
            if (STRICT_UTF8.GetByteCount(source) > Assemblyˉlimits.MAX_SOURCE_BYTES)
            {
                return Fail("WVA1011", 1, 1, "Assembly source exceeds the 1 MiB limit.");
            }
        }
        catch (EncoderFallbackException)
        {
            return Fail("WVA1001", 1, 1, "Assembly source is not valid Unicode for strict UTF-8 encoding.");
        }

        var Symbols = new List<Assemblyˉsymbol>();
        var Sections = new List<Assemblyˉsection>();
        var Symbolˉnames = new HashSet<string>(StringComparer.Ordinal);
        var Sectionˉnames = new HashSet<string>(StringComparer.Ordinal);
        Assemblyˉsymbol? Previousˉsymbol = null;
        Assemblyˉsection? Previousˉsection = null;
        Sectionˉbuilder? Currentˉsection = null;
        Definitionˉbuilder? Currentˉdefinition = null;
        var Headerˉseen = false;
        var Sectionsˉstarted = false;

        using var Reader = new StringReader(source);
        string? Line;
        var Lineˉnumber = 0;
        while ((Line = Reader.ReadLine()) is not null)
        {
            Lineˉnumber++;
            if (STRICT_UTF8.GetByteCount(Line) > Assemblyˉlimits.MAX_LINE_BYTES)
            {
                return Fail("WVA1011", Lineˉnumber, 1, "Assembly line exceeds the 4 KiB limit.");
            }

            var Tokens = Tokenize(Line);
            if (Tokens.Count == 0)
            {
                continue;
            }

            if (!Headerˉseen)
            {
                if (Tokens.Count != 2 || Tokens[0].Text != "windvale-assembly" || Tokens[1].Text != "1")
                {
                    return Fail("WVA1001", Lineˉnumber, Tokens[0].Column, "Expected 'windvale-assembly 1'.");
                }
                Headerˉseen = true;
                continue;
            }

            if (Currentˉdefinition is not null)
            {
                if (Matches(Tokens, "end", "define"))
                {
                    Currentˉsection!.Definitions.Add(Currentˉdefinition.Build());
                    Currentˉdefinition = null;
                    continue;
                }

                var Statementˉresult = Parseˉstatement(Tokens, Lineˉnumber);
                if (Statementˉresult.Diagnostic is not null)
                {
                    return new(null, Statementˉresult.Diagnostic);
                }
                Currentˉdefinition.Statements.Add(Statementˉresult.Statement!);
                continue;
            }

            if (Currentˉsection is not null)
            {
                if (Tokens[0].Text == "define")
                {
                    if (Tokens.Count != 2)
                    {
                        return Fail("WVA1003", Lineˉnumber, Tokens[0].Column, "A definition requires exactly one symbol name.");
                    }
                    if (!Objectˉverifier.Isˉmachineˉname(Tokens[1].Text))
                    {
                        return Fail("WVA1004", Lineˉnumber, Tokens[1].Column, "Definition name is not a WVO machine name.");
                    }
                    Currentˉdefinition = new(Tokens[1].Text, new(Lineˉnumber, Tokens[1].Column));
                    continue;
                }
                if (Matches(Tokens, "end", "section"))
                {
                    var Section = Currentˉsection.Build();
                    Sections.Add(Section);
                    Previousˉsection = Section;
                    Currentˉsection = null;
                    continue;
                }
                return Fail("WVA1002", Lineˉnumber, Tokens[0].Column, "Expected 'define <Name>' or 'end section'.");
            }

            if (Tokens[0].Text == "symbol")
            {
                if (Sectionsˉstarted)
                {
                    return Fail("WVA1002", Lineˉnumber, Tokens[0].Column, "Symbol declarations must precede all sections.");
                }
                var Symbolˉresult = Parseˉsymbol(Tokens, Lineˉnumber);
                if (Symbolˉresult.Diagnostic is not null)
                {
                    return new(null, Symbolˉresult.Diagnostic);
                }
                var Symbol = Symbolˉresult.Symbol!;
                if (!Symbolˉnames.Add(Symbol.Name))
                {
                    return Fail("WVA1006", Symbol.Span.Line, Symbol.Span.Column, $"Symbol '{Symbol.Name}' is duplicated.");
                }
                if (Previousˉsymbol is not null &&
                    (Symbol.Binding < Previousˉsymbol.Binding ||
                        (Symbol.Binding == Previousˉsymbol.Binding &&
                            StringComparer.Ordinal.Compare(Previousˉsymbol.Name, Symbol.Name) >= 0)))
                {
                    return Fail("WVA1006", Symbol.Span.Line, Symbol.Span.Column, "Symbols are not in canonical binding/name order.");
                }
                Symbols.Add(Symbol);
                Previousˉsymbol = Symbol;
                if (Symbols.Count > Objectˉlimits.MAX_SYMBOLS)
                {
                    return Fail("WVA1011", Symbol.Span.Line, Symbol.Span.Column, "Assembly exceeds the WVO symbol-count limit.");
                }
                continue;
            }

            if (Tokens[0].Text == "section")
            {
                Sectionsˉstarted = true;
                var Sectionˉresult = Parseˉsection(Tokens, Lineˉnumber);
                if (Sectionˉresult.Diagnostic is not null)
                {
                    return new(null, Sectionˉresult.Diagnostic);
                }
                var Section = Sectionˉresult.Section!;
                if (!Sectionˉnames.Add(Section.Name))
                {
                    return Fail("WVA1006", Section.Span.Line, Section.Span.Column, $"Section '{Section.Name}' is duplicated.");
                }
                if (Previousˉsection is not null &&
                    (Section.Kind < Previousˉsection.Kind ||
                        (Section.Kind == Previousˉsection.Kind &&
                            StringComparer.Ordinal.Compare(Previousˉsection.Name, Section.Name) >= 0)))
                {
                    return Fail("WVA1006", Section.Span.Line, Section.Span.Column, "Sections are not in canonical kind/name order.");
                }
                if (Sections.Count >= Objectˉlimits.MAX_SECTIONS)
                {
                    return Fail("WVA1011", Section.Span.Line, Section.Span.Column, "Assembly exceeds the WVO section-count limit.");
                }
                Currentˉsection = new(Section.Name, Section.Kind, Section.Alignment, Section.Span);
                continue;
            }

            return Fail("WVA1002", Lineˉnumber, Tokens[0].Column, $"Unexpected assembly statement '{Tokens[0].Text}'.");
        }

        if (!Headerˉseen)
        {
            return Fail("WVA1001", 1, 1, "Assembly source has no version header.");
        }
        if (Currentˉdefinition is not null)
        {
            return Fail("WVA1010", Currentˉdefinition.Span.Line, Currentˉdefinition.Span.Column, "Definition is missing 'end define'.");
        }
        if (Currentˉsection is not null)
        {
            return Fail("WVA1010", Currentˉsection.Span.Line, Currentˉsection.Span.Column, "Section is missing 'end section'.");
        }
        if (Sections.Count == 0)
        {
            return Fail("WVA1007", 1, 1, "Assembly requires at least one section.");
        }

        var Unit = new Assemblyˉunit(Symbols.ToImmutableArray(), Sections.ToImmutableArray());
        var Semanticˉdiagnostic = Validate(Unit);
        return Semanticˉdiagnostic is null
            ? new(Unit, null)
            : new(null, Semanticˉdiagnostic);
    }

    private static Assemblyˉdiagnostic? Validate(Assemblyˉunit unit)
    {
        var Sections = unit.Sections.ToDictionary(Section => Section.Name, StringComparer.Ordinal);
        var Symbols = unit.Symbols.ToDictionary(Symbol => Symbol.Name, StringComparer.Ordinal);
        var Definitions = new Dictionary<string, (Assemblyˉdefinition Definition, Assemblyˉsection Section)>(StringComparer.Ordinal);
        var Relocationˉcount = 0;

        foreach (var Symbol in unit.Symbols)
        {
            if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
            {
                continue;
            }
            if (Symbol.Sectionˉname is null || !Sections.TryGetValue(Symbol.Sectionˉname, out var Section))
            {
                return Diagnostic("WVA1007", Symbol.Span, $"Symbol '{Symbol.Name}' references an unknown section.");
            }
            if (Symbol.Kind == Objectˉsymbolˉkind.Function && Section.Kind != Objectˉsectionˉkind.Code)
            {
                return Diagnostic("WVA1007", Symbol.Span, $"Function symbol '{Symbol.Name}' must belong to code.");
            }
            if (Symbol.Kind == Objectˉsymbolˉkind.Data && Section.Kind == Objectˉsectionˉkind.Code)
            {
                return Diagnostic("WVA1007", Symbol.Span, $"Data symbol '{Symbol.Name}' cannot belong to code.");
            }
        }

        foreach (var Section in unit.Sections)
        {
            foreach (var Definition in Section.Definitions)
            {
                if (!Symbols.TryGetValue(Definition.Name, out var Symbol))
                {
                    return Diagnostic("WVA1009", Definition.Span, $"Definition '{Definition.Name}' has no symbol declaration.");
                }
                if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
                {
                    return Diagnostic("WVA1007", Definition.Span, $"Imported symbol '{Definition.Name}' cannot have a definition.");
                }
                if (Symbol.Sectionˉname != Section.Name)
                {
                    return Diagnostic("WVA1007", Definition.Span, $"Definition '{Definition.Name}' is in the wrong section.");
                }
                if (!Definitions.TryAdd(Definition.Name, (Definition, Section)))
                {
                    return Diagnostic("WVA1006", Definition.Span, $"Definition '{Definition.Name}' is duplicated.");
                }

                var Labels = new Dictionary<string, Assemblyˉspan>(StringComparer.Ordinal);
                foreach (var Statement in Definition.Statements)
                {
                    if (Statement.Kind == Assemblyˉstatementˉkind.Label &&
                        !Labels.TryAdd(Statement.Name!, Statement.Span))
                    {
                        return Diagnostic("WVA1006", Statement.Span, $"Local label '{Statement.Name}' is duplicated.");
                    }
                }

                foreach (var Statement in Definition.Statements)
                {
                    var Codeˉstatement = Statement.Kind is
                        Assemblyˉstatementˉkind.Nop or Assemblyˉstatementˉkind.Return or
                        Assemblyˉstatementˉkind.Trap or Assemblyˉstatementˉkind.Call or
                        Assemblyˉstatementˉkind.Jump or Assemblyˉstatementˉkind.Moveˉi32 or
                        Assemblyˉstatementˉkind.Moveˉu32 or
                        Assemblyˉstatementˉkind.Disableˉinterrupts or
                        Assemblyˉstatementˉkind.Halt or Assemblyˉstatementˉkind.Outˉu16 or
                        Assemblyˉstatementˉkind.Pushˉi32 or
                        Assemblyˉstatementˉkind.Enableˉpageˉprotection or
                        Assemblyˉstatementˉkind.Activateˉpageˉtable or
                        Assemblyˉstatementˉkind.Syscall or
                        Assemblyˉstatementˉkind.Label or Assemblyˉstatementˉkind.Jumpˉlabel or
                        Assemblyˉstatementˉkind.Branch or Assemblyˉstatementˉkind.Moveˉregister or
                        Assemblyˉstatementˉkind.Add or Assemblyˉstatementˉkind.Subtract or
                        Assemblyˉstatementˉkind.And or Assemblyˉstatementˉkind.Or or
                        Assemblyˉstatementˉkind.Xor or Assemblyˉstatementˉkind.Compare or
                        Assemblyˉstatementˉkind.Test or Assemblyˉstatementˉkind.Pushˉregister or
                        Assemblyˉstatementˉkind.Popˉregister or Assemblyˉstatementˉkind.Callˉregister or
                        Assemblyˉstatementˉkind.Jumpˉregister or Assemblyˉstatementˉkind.Loadˉu32 or
                        Assemblyˉstatementˉkind.Loadˉu64 or Assemblyˉstatementˉkind.Storeˉu32 or
                        Assemblyˉstatementˉkind.Storeˉu64 or Assemblyˉstatementˉkind.Loadˉaddress or
                        Assemblyˉstatementˉkind.Multiply or Assemblyˉstatementˉkind.Addˉi32 or
                        Assemblyˉstatementˉkind.Subtractˉi32 or Assemblyˉstatementˉkind.Andˉi32 or
                        Assemblyˉstatementˉkind.Orˉi32 or Assemblyˉstatementˉkind.Xorˉi32 or
                        Assemblyˉstatementˉkind.Compareˉi32 or Assemblyˉstatementˉkind.Testˉi32 or
                        Assemblyˉstatementˉkind.Rotateˉleft or Assemblyˉstatementˉkind.Rotateˉright or
                        Assemblyˉstatementˉkind.Shiftˉleft or Assemblyˉstatementˉkind.Shiftˉright or
                        Assemblyˉstatementˉkind.Shiftˉrightˉsigned or
                        Assemblyˉstatementˉkind.Loadˉmemoryˉu32 or Assemblyˉstatementˉkind.Loadˉmemoryˉu64 or
                        Assemblyˉstatementˉkind.Storeˉmemoryˉu32 or Assemblyˉstatementˉkind.Storeˉmemoryˉu64 or
                        Assemblyˉstatementˉkind.Moveˉu8 or Assemblyˉstatementˉkind.Moveˉu16 or
                        Assemblyˉstatementˉkind.Addˉi8 or Assemblyˉstatementˉkind.Subtractˉi8 or
                        Assemblyˉstatementˉkind.Andˉi8 or Assemblyˉstatementˉkind.Orˉi8 or
                        Assemblyˉstatementˉkind.Xorˉi8 or Assemblyˉstatementˉkind.Compareˉi8 or
                        Assemblyˉstatementˉkind.Testˉi8 or Assemblyˉstatementˉkind.Addˉi16 or
                        Assemblyˉstatementˉkind.Subtractˉi16 or Assemblyˉstatementˉkind.Andˉi16 or
                        Assemblyˉstatementˉkind.Orˉi16 or Assemblyˉstatementˉkind.Xorˉi16 or
                        Assemblyˉstatementˉkind.Compareˉi16 or Assemblyˉstatementˉkind.Testˉi16 or
                        Assemblyˉstatementˉkind.Loadˉu8 or Assemblyˉstatementˉkind.Loadˉu16 or
                        Assemblyˉstatementˉkind.Storeˉu8 or Assemblyˉstatementˉkind.Storeˉu16 or
                        Assemblyˉstatementˉkind.Loadˉmemoryˉu8 or Assemblyˉstatementˉkind.Loadˉmemoryˉu16 or
                        Assemblyˉstatementˉkind.Storeˉmemoryˉu8 or Assemblyˉstatementˉkind.Storeˉmemoryˉu16 or
                        Assemblyˉstatementˉkind.Setˉcondition or Assemblyˉstatementˉkind.Zeroˉextendˉu8 or
                        Assemblyˉstatementˉkind.Zeroˉextendˉu16 or Assemblyˉstatementˉkind.Signˉextendˉi8 or
                        Assemblyˉstatementˉkind.Signˉextendˉi16 or Assemblyˉstatementˉkind.Inˉu8 or
                        Assemblyˉstatementˉkind.Outˉu8 or Assemblyˉstatementˉkind.Cpuid or
                        Assemblyˉstatementˉkind.Readˉtsc or Assemblyˉstatementˉkind.Readˉmsr or
                        Assemblyˉstatementˉkind.Swapˉgs or
                        Assemblyˉstatementˉkind.Interruptˉreturn;
                    var Materializedˉdataˉstatement = Statement.Kind is
                        Assemblyˉstatementˉkind.Bytes or Assemblyˉstatementˉkind.U32 or
                        Assemblyˉstatementˉkind.I32 or Assemblyˉstatementˉkind.Addressˉu32;
                    var Allowed = Section.Kind switch
                    {
                        Objectˉsectionˉkind.Code => Codeˉstatement,
                        Objectˉsectionˉkind.Readˉonlyˉdata or Objectˉsectionˉkind.Writableˉdata => Materializedˉdataˉstatement,
                        Objectˉsectionˉkind.Zeroˉfill => Statement.Kind == Assemblyˉstatementˉkind.Zero,
                        _ => false,
                    };
                    if (!Allowed)
                    {
                        return Diagnostic("WVA1008", Statement.Span, "Statement is not valid in this section kind.");
                    }

                    if (Statement.Kind == Assemblyˉstatementˉkind.Label)
                    {
                        continue;
                    }
                    if (Statement.Kind is Assemblyˉstatementˉkind.Jumpˉlabel or Assemblyˉstatementˉkind.Branch)
                    {
                        if (!Labels.ContainsKey(Statement.Name!))
                        {
                            return Diagnostic("WVA1009", Statement.Span, $"Local label '{Statement.Name}' is not defined in this definition.");
                        }
                        continue;
                    }
                    if (Statement.Name is not null)
                    {
                        if (!Symbols.TryGetValue(Statement.Name, out var Target))
                        {
                            return Diagnostic("WVA1009", Statement.Span, $"Reference target '{Statement.Name}' is not declared.");
                        }
                        if (Statement.Kind is Assemblyˉstatementˉkind.Call or Assemblyˉstatementˉkind.Jump &&
                            Target.Kind != Objectˉsymbolˉkind.Function)
                        {
                            return Diagnostic("WVA1009", Statement.Span, $"Instruction target '{Statement.Name}' is not a function.");
                        }
                        if (Statement.Kind is Assemblyˉstatementˉkind.Loadˉu8 or Assemblyˉstatementˉkind.Loadˉu16 or
                            Assemblyˉstatementˉkind.Loadˉu32 or Assemblyˉstatementˉkind.Loadˉu64 or
                            Assemblyˉstatementˉkind.Storeˉu8 or Assemblyˉstatementˉkind.Storeˉu16 or
                            Assemblyˉstatementˉkind.Storeˉu32 or Assemblyˉstatementˉkind.Storeˉu64 &&
                            Target.Kind != Objectˉsymbolˉkind.Data)
                        {
                            return Diagnostic("WVA1009", Statement.Span, $"Memory target '{Statement.Name}' is not data.");
                        }
                        Relocationˉcount++;
                        if (Relocationˉcount > Objectˉlimits.MAX_RELOCATIONS)
                        {
                            return Diagnostic("WVA1011", Statement.Span, "Assembly exceeds the WVO relocation-count limit.");
                        }
                    }
                }
            }
        }

        foreach (var Symbol in unit.Symbols)
        {
            if (Symbol.Binding != Objectˉsymbolˉbinding.Import && !Definitions.ContainsKey(Symbol.Name))
            {
                return Diagnostic("WVA1009", Symbol.Span, $"Symbol '{Symbol.Name}' has no definition.");
            }
        }
        return null;
    }

    private static (Assemblyˉsymbol? Symbol, Assemblyˉdiagnostic? Diagnostic) Parseˉsymbol(
        List<Token> tokens,
        int line)
    {
        if (tokens.Count < 4)
        {
            return (null, Diagnostic("WVA1003", line, tokens[0].Column, "Symbol declaration is incomplete."));
        }
        var Binding = tokens[1].Text switch
        {
            "local" => Objectˉsymbolˉbinding.Local,
            "export" => Objectˉsymbolˉbinding.Export,
            "import" => Objectˉsymbolˉbinding.Import,
            _ => (Objectˉsymbolˉbinding)0,
        };
        var Kind = tokens[2].Text switch
        {
            "function" => Objectˉsymbolˉkind.Function,
            "data" => Objectˉsymbolˉkind.Data,
            _ => (Objectˉsymbolˉkind)0,
        };
        if (!Enum.IsDefined(Binding) || !Enum.IsDefined(Kind))
        {
            return (null, Diagnostic("WVA1002", line, tokens[1].Column, "Symbol binding or kind is invalid."));
        }
        if (!Objectˉverifier.Isˉmachineˉname(tokens[3].Text))
        {
            return (null, Diagnostic("WVA1004", line, tokens[3].Column, "Symbol name is not a WVO machine name."));
        }

        string? Sectionˉname = null;
        if (Binding == Objectˉsymbolˉbinding.Import)
        {
            if (tokens.Count != 4)
            {
                return (null, Diagnostic("WVA1003", line, tokens[0].Column, "Imported symbols do not name a section."));
            }
        }
        else
        {
            if (tokens.Count != 6 || tokens[4].Text != "in")
            {
                return (null, Diagnostic("WVA1003", line, tokens[0].Column, "Defined symbols require 'in <Section>'."));
            }
            if (!Objectˉverifier.Isˉmachineˉname(tokens[5].Text))
            {
                return (null, Diagnostic("WVA1004", line, tokens[5].Column, "Section name is not a WVO machine name."));
            }
            Sectionˉname = tokens[5].Text;
        }
        return (new(tokens[3].Text, Binding, Kind, Sectionˉname, new(line, tokens[3].Column)), null);
    }

    private static (Assemblyˉsection? Section, Assemblyˉdiagnostic? Diagnostic) Parseˉsection(
        List<Token> tokens,
        int line)
    {
        if (tokens.Count != 5 || tokens[3].Text != "align")
        {
            return (null, Diagnostic("WVA1003", line, tokens[0].Column, "Section syntax is 'section <kind> <Name> align <value>'."));
        }
        var Kind = tokens[1].Text switch
        {
            "code" => Objectˉsectionˉkind.Code,
            "rodata" => Objectˉsectionˉkind.Readˉonlyˉdata,
            "data" => Objectˉsectionˉkind.Writableˉdata,
            "bss" => Objectˉsectionˉkind.Zeroˉfill,
            _ => (Objectˉsectionˉkind)0,
        };
        if (!Enum.IsDefined(Kind))
        {
            return (null, Diagnostic("WVA1002", line, tokens[1].Column, "Section kind is invalid."));
        }
        if (!Objectˉverifier.Isˉmachineˉname(tokens[2].Text))
        {
            return (null, Diagnostic("WVA1004", line, tokens[2].Column, "Section name is not a WVO machine name."));
        }
        if (!Tryˉu32(tokens[4].Text, out var Alignment) ||
            Alignment is 0 or > Objectˉlimits.MAX_ALIGNMENT ||
            (Alignment & (Alignment - 1)) != 0)
        {
            return (null, Diagnostic("WVA1005", line, tokens[4].Column, "Section alignment must be a power of two from 1 through 4096."));
        }
        return (new(tokens[2].Text, Kind, Alignment, [], new(line, tokens[2].Column)), null);
    }

    private static (Assemblyˉstatement? Statement, Assemblyˉdiagnostic? Diagnostic) Parseˉstatement(
        List<Token> tokens,
        int line)
    {
        var Span = new Assemblyˉspan(line, tokens[0].Column);
        switch (tokens[0].Text)
        {
            case "label":
            case "jump_label":
                if (tokens.Count != 2 || !Objectˉverifier.Isˉmachineˉname(tokens[1].Text))
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires one local-label name."));
                }
                return (new(
                    tokens[0].Text == "label" ? Assemblyˉstatementˉkind.Label : Assemblyˉstatementˉkind.Jumpˉlabel,
                    tokens[1].Text,
                    0,
                    0,
                    [],
                    Span), null);
            case "branch":
                if (tokens.Count != 3 ||
                    !Tryˉcondition(tokens[1].Text, out var Condition) ||
                    !Objectˉverifier.Isˉmachineˉname(tokens[2].Text))
                {
                    return (null, Diagnostic("WVA1003", Span, "'branch' requires a condition and one local-label name."));
                }
                return (new(
                    Assemblyˉstatementˉkind.Branch,
                    tokens[2].Text,
                    0,
                    0,
                    [],
                    Span,
                    Condition: Condition), null);
            case "move":
            case "add":
            case "subtract":
            case "and":
            case "or":
            case "xor":
            case "compare":
            case "test":
            case "multiply":
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Destination) ||
                    !Tryˉtypedˉregister(tokens[2].Text, out var Source) ||
                    Destination.Width != Source.Width ||
                    tokens[0].Text == "multiply" && Destination.Width == 8)
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires two registers of the same width; multiply supports 16, 32, or 64 bits."));
                }
                var Registerˉkind = tokens[0].Text switch
                {
                    "move" => Assemblyˉstatementˉkind.Moveˉregister,
                    "add" => Assemblyˉstatementˉkind.Add,
                    "subtract" => Assemblyˉstatementˉkind.Subtract,
                    "and" => Assemblyˉstatementˉkind.And,
                    "or" => Assemblyˉstatementˉkind.Or,
                    "xor" => Assemblyˉstatementˉkind.Xor,
                    "compare" => Assemblyˉstatementˉkind.Compare,
                    "multiply" => Assemblyˉstatementˉkind.Multiply,
                    _ => Assemblyˉstatementˉkind.Test,
                };
                return (new(Registerˉkind, null, 0, 0, [], Span, Destination, Source), null);
            case "add_i32":
            case "subtract_i32":
            case "and_i32":
            case "or_i32":
            case "xor_i32":
            case "compare_i32":
            case "test_i32":
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Immediateˉregister) ||
                    Immediateˉregister.Width is not (32 or 64))
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires a 32- or 64-bit register and signed 32-bit immediate."));
                }
                if (!Tryˉi32(tokens[2].Text, out var Immediate))
                {
                    return (null, Diagnostic("WVA1005", line, tokens[2].Column, "Immediate is outside the i32 range."));
                }
                var Immediateˉkind = tokens[0].Text switch
                {
                    "add_i32" => Assemblyˉstatementˉkind.Addˉi32,
                    "subtract_i32" => Assemblyˉstatementˉkind.Subtractˉi32,
                    "and_i32" => Assemblyˉstatementˉkind.Andˉi32,
                    "or_i32" => Assemblyˉstatementˉkind.Orˉi32,
                    "xor_i32" => Assemblyˉstatementˉkind.Xorˉi32,
                    "compare_i32" => Assemblyˉstatementˉkind.Compareˉi32,
                    _ => Assemblyˉstatementˉkind.Testˉi32,
                };
                return (new(Immediateˉkind, null, Immediate, 0, [], Span, Immediateˉregister), null);
            case "add_i8":
            case "subtract_i8":
            case "and_i8":
            case "or_i8":
            case "xor_i8":
            case "compare_i8":
            case "test_i8":
            case "add_i16":
            case "subtract_i16":
            case "and_i16":
            case "or_i16":
            case "xor_i16":
            case "compare_i16":
            case "test_i16":
                var Narrowˉwidth = tokens[0].Text.EndsWith("i8", StringComparison.Ordinal) ? (byte)8 : (byte)16;
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Narrowˉimmediateˉregister) ||
                    Narrowˉimmediateˉregister.Width != Narrowˉwidth)
                {
                    return (null, Diagnostic("WVA1003", Span,
                        $"'{tokens[0].Text}' requires a {Narrowˉwidth}-bit register and signed {Narrowˉwidth}-bit immediate."));
                }
                if (!Tryˉi32(tokens[2].Text, out var Narrowˉimmediate) ||
                    Narrowˉwidth == 8 && Narrowˉimmediate is < sbyte.MinValue or > sbyte.MaxValue ||
                    Narrowˉwidth == 16 && Narrowˉimmediate is < short.MinValue or > short.MaxValue)
                {
                    return (null, Diagnostic("WVA1005", line, tokens[2].Column,
                        $"Immediate is outside the i{Narrowˉwidth} range."));
                }
                var Narrowˉimmediateˉkind = tokens[0].Text switch
                {
                    "add_i8" => Assemblyˉstatementˉkind.Addˉi8,
                    "subtract_i8" => Assemblyˉstatementˉkind.Subtractˉi8,
                    "and_i8" => Assemblyˉstatementˉkind.Andˉi8,
                    "or_i8" => Assemblyˉstatementˉkind.Orˉi8,
                    "xor_i8" => Assemblyˉstatementˉkind.Xorˉi8,
                    "compare_i8" => Assemblyˉstatementˉkind.Compareˉi8,
                    "test_i8" => Assemblyˉstatementˉkind.Testˉi8,
                    "add_i16" => Assemblyˉstatementˉkind.Addˉi16,
                    "subtract_i16" => Assemblyˉstatementˉkind.Subtractˉi16,
                    "and_i16" => Assemblyˉstatementˉkind.Andˉi16,
                    "or_i16" => Assemblyˉstatementˉkind.Orˉi16,
                    "xor_i16" => Assemblyˉstatementˉkind.Xorˉi16,
                    "compare_i16" => Assemblyˉstatementˉkind.Compareˉi16,
                    _ => Assemblyˉstatementˉkind.Testˉi16,
                };
                return (new(Narrowˉimmediateˉkind, null, Narrowˉimmediate, 0, [], Span,
                    Narrowˉimmediateˉregister), null);
            case "rotate_left":
            case "rotate_right":
            case "shift_left":
            case "shift_right":
            case "shift_right_signed":
                if (tokens.Count != 3 || !Tryˉtypedˉregister(tokens[1].Text, out var Shiftˉregister))
                {
                    return (null, Diagnostic("WVA1003", Span,
                        $"'{tokens[0].Text}' requires an 8-, 16-, 32-, or 64-bit register and count."));
                }
                if (!Tryˉu32(tokens[2].Text, out var Shiftˉcount) || Shiftˉcount >= Shiftˉregister.Width)
                {
                    return (null, Diagnostic("WVA1005", line, tokens[2].Column, $"Shift count must be from 0 through {Shiftˉregister.Width - 1}."));
                }
                var Shiftˉkind = tokens[0].Text switch
                {
                    "rotate_left" => Assemblyˉstatementˉkind.Rotateˉleft,
                    "rotate_right" => Assemblyˉstatementˉkind.Rotateˉright,
                    "shift_left" => Assemblyˉstatementˉkind.Shiftˉleft,
                    "shift_right" => Assemblyˉstatementˉkind.Shiftˉright,
                    _ => Assemblyˉstatementˉkind.Shiftˉrightˉsigned,
                };
                return (new(Shiftˉkind, null, Shiftˉcount, 0, [], Span, Shiftˉregister), null);
            case "push":
            case "pop":
            case "call_register":
            case "jump_register":
                if (tokens.Count != 2 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Controlˉregister) ||
                    Controlˉregister.Width != 64)
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires one 64-bit register."));
                }
                var Controlˉkind = tokens[0].Text switch
                {
                    "push" => Assemblyˉstatementˉkind.Pushˉregister,
                    "pop" => Assemblyˉstatementˉkind.Popˉregister,
                    "call_register" => Assemblyˉstatementˉkind.Callˉregister,
                    _ => Assemblyˉstatementˉkind.Jumpˉregister,
                };
                return (new(Controlˉkind, null, 0, 0, [], Span, Controlˉregister), null);
            case "load_u32":
            case "load_u64":
            case "load_u8":
            case "load_u16":
            case "load_address":
                var Requiredˉloadˉwidth = tokens[0].Text switch
                {
                    "load_u8" => (byte)8,
                    "load_u16" => (byte)16,
                    "load_u32" => (byte)32,
                    _ => (byte)64,
                };
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Loadˉregister) ||
                    Loadˉregister.Width != Requiredˉloadˉwidth ||
                    !Objectˉverifier.Isˉmachineˉname(tokens[2].Text))
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires a {Requiredˉloadˉwidth}-bit register and symbol name."));
                }
                var Loadˉkind = tokens[0].Text switch
                {
                    "load_u8" => Assemblyˉstatementˉkind.Loadˉu8,
                    "load_u16" => Assemblyˉstatementˉkind.Loadˉu16,
                    "load_u32" => Assemblyˉstatementˉkind.Loadˉu32,
                    "load_u64" => Assemblyˉstatementˉkind.Loadˉu64,
                    _ => Assemblyˉstatementˉkind.Loadˉaddress,
                };
                return (new(Loadˉkind, tokens[2].Text, 0, 0, [], Span, Loadˉregister), null);
            case "store_u32":
            case "store_u64":
            case "store_u8":
            case "store_u16":
                var Requiredˉstoreˉwidth = tokens[0].Text switch
                {
                    "store_u8" => (byte)8,
                    "store_u16" => (byte)16,
                    "store_u32" => (byte)32,
                    _ => (byte)64,
                };
                if (tokens.Count != 3 ||
                    !Objectˉverifier.Isˉmachineˉname(tokens[1].Text) ||
                    !Tryˉtypedˉregister(tokens[2].Text, out var Storeˉregister) ||
                    Storeˉregister.Width != Requiredˉstoreˉwidth)
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires a symbol name and {Requiredˉstoreˉwidth}-bit register."));
                }
                var Storeˉkind = tokens[0].Text switch
                {
                    "store_u8" => Assemblyˉstatementˉkind.Storeˉu8,
                    "store_u16" => Assemblyˉstatementˉkind.Storeˉu16,
                    "store_u32" => Assemblyˉstatementˉkind.Storeˉu32,
                    _ => Assemblyˉstatementˉkind.Storeˉu64,
                };
                return (new(
                    Storeˉkind,
                    tokens[1].Text,
                    0,
                    0,
                    [],
                    Span,
                    Storeˉregister), null);
            case "load_memory_u32":
            case "load_memory_u64":
            case "store_memory_u32":
            case "store_memory_u64":
            case "load_memory_u8":
            case "load_memory_u16":
            case "store_memory_u8":
            case "store_memory_u16":
                var Isˉmemoryˉload = tokens[0].Text.StartsWith("load_", StringComparison.Ordinal);
                var Memoryˉwidth = tokens[0].Text switch
                {
                    "load_memory_u8" or "store_memory_u8" => (byte)8,
                    "load_memory_u16" or "store_memory_u16" => (byte)16,
                    "load_memory_u32" or "store_memory_u32" => (byte)32,
                    _ => (byte)64,
                };
                if (tokens.Count != 6)
                {
                    return (null, Diagnostic("WVA1003", Span,
                        $"'{tokens[0].Text}' requires value, base, index-or-none, scale, and displacement operands."));
                }
                var Valueˉtoken = Isˉmemoryˉload ? tokens[1] : tokens[5];
                var Baseˉtoken = Isˉmemoryˉload ? tokens[2] : tokens[1];
                var Indexˉtoken = Isˉmemoryˉload ? tokens[3] : tokens[2];
                var Scaleˉtoken = Isˉmemoryˉload ? tokens[4] : tokens[3];
                var Displacementˉtoken = Isˉmemoryˉload ? tokens[5] : tokens[4];
                if (!Tryˉtypedˉregister(Valueˉtoken.Text, out var Valueˉregister) ||
                    Valueˉregister.Width != Memoryˉwidth ||
                    !Tryˉtypedˉregister(Baseˉtoken.Text, out var Baseˉregister) ||
                    Baseˉregister.Width != 64)
                {
                    return (null, Diagnostic("WVA1003", Span,
                        $"'{tokens[0].Text}' requires a {Memoryˉwidth}-bit value register and 64-bit base register."));
                }
                var Hasˉindex = Indexˉtoken.Text != "none";
                var Indexˉregister = default(Assemblyˉregister);
                if (Hasˉindex &&
                    (!Tryˉtypedˉregister(Indexˉtoken.Text, out Indexˉregister) ||
                        Indexˉregister.Width != 64 || Indexˉregister.Index == 4 && !Indexˉregister.Isˉextended))
                {
                    return (null, Diagnostic("WVA1003", Span,
                        "Memory index must be 'none' or a 64-bit register other than rsp."));
                }
                if (!Tryˉu32(Scaleˉtoken.Text, out var Scale) || Scale is not (1 or 2 or 4 or 8) ||
                    !Hasˉindex && Scale != 1)
                {
                    return (null, Diagnostic("WVA1005", line, Scaleˉtoken.Column,
                        "Memory scale must be 1, 2, 4, or 8; 'none' requires scale 1."));
                }
                if (!Tryˉi32(Displacementˉtoken.Text, out var Displacement))
                {
                    return (null, Diagnostic("WVA1005", line, Displacementˉtoken.Column,
                        "Memory displacement is outside the i32 range."));
                }
                var Memoryˉkind = tokens[0].Text switch
                {
                    "load_memory_u8" => Assemblyˉstatementˉkind.Loadˉmemoryˉu8,
                    "load_memory_u16" => Assemblyˉstatementˉkind.Loadˉmemoryˉu16,
                    "load_memory_u32" => Assemblyˉstatementˉkind.Loadˉmemoryˉu32,
                    "load_memory_u64" => Assemblyˉstatementˉkind.Loadˉmemoryˉu64,
                    "store_memory_u8" => Assemblyˉstatementˉkind.Storeˉmemoryˉu8,
                    "store_memory_u16" => Assemblyˉstatementˉkind.Storeˉmemoryˉu16,
                    "store_memory_u32" => Assemblyˉstatementˉkind.Storeˉmemoryˉu32,
                    _ => Assemblyˉstatementˉkind.Storeˉmemoryˉu64,
                };
                return (new(
                    Memoryˉkind,
                    null,
                    Displacement,
                    0,
                    [],
                    Span,
                    Valueˉregister,
                    Baseˉregister,
                    Thirdˉregister: Indexˉregister,
                    Scale: (byte)Scale,
                    Hasˉindex: Hasˉindex), null);
            case "set_condition":
                if (tokens.Count != 3 ||
                    !Tryˉcondition(tokens[1].Text, out var Setˉcondition) ||
                    !Tryˉtypedˉregister(tokens[2].Text, out var Setˉregister) ||
                    Setˉregister.Width != 8)
                {
                    return (null, Diagnostic("WVA1003", Span,
                        "'set_condition' requires a condition and 8-bit register."));
                }
                return (new(Assemblyˉstatementˉkind.Setˉcondition, null, 0, 0, [], Span,
                    Setˉregister, Condition: Setˉcondition), null);
            case "zero_extend_u8":
            case "zero_extend_u16":
            case "sign_extend_i8":
            case "sign_extend_i16":
                var Extensionˉsourceˉwidth = tokens[0].Text.EndsWith("8", StringComparison.Ordinal) ? (byte)8 : (byte)16;
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Extensionˉdestination) ||
                    Extensionˉdestination.Width is not (32 or 64) ||
                    !Tryˉtypedˉregister(tokens[2].Text, out var Extensionˉsource) ||
                    Extensionˉsource.Width != Extensionˉsourceˉwidth)
                {
                    return (null, Diagnostic("WVA1003", Span,
                        $"'{tokens[0].Text}' requires a 32- or 64-bit destination and {Extensionˉsourceˉwidth}-bit source."));
                }
                var Extensionˉkind = tokens[0].Text switch
                {
                    "zero_extend_u8" => Assemblyˉstatementˉkind.Zeroˉextendˉu8,
                    "zero_extend_u16" => Assemblyˉstatementˉkind.Zeroˉextendˉu16,
                    "sign_extend_i8" => Assemblyˉstatementˉkind.Signˉextendˉi8,
                    _ => Assemblyˉstatementˉkind.Signˉextendˉi16,
                };
                return (new(Extensionˉkind, null, 0, 0, [], Span,
                    Extensionˉdestination, Extensionˉsource), null);
            case "nop":
            case "return":
            case "trap":
            case "disable_interrupts":
            case "halt":
            case "out_u16":
            case "in_u8":
            case "out_u8":
            case "enable_page_protection":
            case "activate_page_table":
            case "syscall":
            case "cpuid":
            case "read_tsc":
            case "read_msr":
            case "swap_gs":
            case "interrupt_return":
                if (tokens.Count != 1)
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' takes no operands."));
                }
                var Simpleˉkind = tokens[0].Text switch
                {
                    "nop" => Assemblyˉstatementˉkind.Nop,
                    "return" => Assemblyˉstatementˉkind.Return,
                    "trap" => Assemblyˉstatementˉkind.Trap,
                    "disable_interrupts" => Assemblyˉstatementˉkind.Disableˉinterrupts,
                    "halt" => Assemblyˉstatementˉkind.Halt,
                    "out_u16" => Assemblyˉstatementˉkind.Outˉu16,
                    "in_u8" => Assemblyˉstatementˉkind.Inˉu8,
                    "out_u8" => Assemblyˉstatementˉkind.Outˉu8,
                    "enable_page_protection" => Assemblyˉstatementˉkind.Enableˉpageˉprotection,
                    "activate_page_table" => Assemblyˉstatementˉkind.Activateˉpageˉtable,
                    "syscall" => Assemblyˉstatementˉkind.Syscall,
                    "cpuid" => Assemblyˉstatementˉkind.Cpuid,
                    "read_tsc" => Assemblyˉstatementˉkind.Readˉtsc,
                    "read_msr" => Assemblyˉstatementˉkind.Readˉmsr,
                    "swap_gs" => Assemblyˉstatementˉkind.Swapˉgs,
                    _ => Assemblyˉstatementˉkind.Interruptˉreturn,
                };
                return (new(Simpleˉkind, null, 0, 0, [], Span), null);
            case "call":
            case "jump":
            case "address_u32":
                if (tokens.Count != 2 || !Objectˉverifier.Isˉmachineˉname(tokens[1].Text))
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires one machine-name operand."));
                }
                var Referenceˉkind = tokens[0].Text switch
                {
                    "call" => Assemblyˉstatementˉkind.Call,
                    "jump" => Assemblyˉstatementˉkind.Jump,
                    _ => Assemblyˉstatementˉkind.Addressˉu32,
                };
                return (new(Referenceˉkind, tokens[1].Text, 0, 0, [], Span), null);
            case "move_i32":
            case "move_u32":
                if (tokens.Count != 3 || !Tryˉregister(tokens[1].Text, out var Register))
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires a 32-bit register and integer."));
                }
                if (tokens[0].Text == "move_i32")
                {
                    if (!Tryˉi32(tokens[2].Text, out var Signed))
                    {
                        return (null, Diagnostic("WVA1005", line, tokens[2].Column, "Value is outside the i32 range."));
                    }
                    return (new(Assemblyˉstatementˉkind.Moveˉi32, null, Signed, Register, [], Span), null);
                }
                if (!Tryˉu32(tokens[2].Text, out var Unsigned))
                {
                    return (null, Diagnostic("WVA1005", line, tokens[2].Column, "Value is outside the u32 range."));
                }
                return (new(Assemblyˉstatementˉkind.Moveˉu32, null, Unsigned, Register, [], Span), null);
            case "move_u8":
            case "move_u16":
                var Moveˉwidth = tokens[0].Text == "move_u8" ? (byte)8 : (byte)16;
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Moveˉregister) ||
                    Moveˉregister.Width != Moveˉwidth)
                {
                    return (null, Diagnostic("WVA1003", Span,
                        $"'{tokens[0].Text}' requires a {Moveˉwidth}-bit register and unsigned {Moveˉwidth}-bit integer."));
                }
                if (!Tryˉu32(tokens[2].Text, out var Moveˉvalue) ||
                    Moveˉwidth == 8 && Moveˉvalue > byte.MaxValue ||
                    Moveˉwidth == 16 && Moveˉvalue > ushort.MaxValue)
                {
                    return (null, Diagnostic("WVA1005", line, tokens[2].Column,
                        $"Value is outside the u{Moveˉwidth} range."));
                }
                return (new(
                    Moveˉwidth == 8 ? Assemblyˉstatementˉkind.Moveˉu8 : Assemblyˉstatementˉkind.Moveˉu16,
                    null,
                    Moveˉvalue,
                    0,
                    [],
                    Span,
                    Moveˉregister), null);
            case "push_i32":
                if (tokens.Count != 2)
                {
                    return (null, Diagnostic("WVA1003", Span, "'push_i32' requires one signed 32-bit integer."));
                }
                if (!Tryˉi32(tokens[1].Text, out var Pushˉvalue))
                {
                    return (null, Diagnostic("WVA1005", line, tokens[1].Column, "Value is outside the i32 range."));
                }
                return (new(Assemblyˉstatementˉkind.Pushˉi32, null, Pushˉvalue, 0, [], Span), null);
            case "bytes":
                if (tokens.Count is < 2 or > Assemblyˉlimits.MAX_BYTES_PER_STATEMENT + 1)
                {
                    return (null, Diagnostic("WVA1003", Span, "'bytes' requires 1 through 4096 byte values."));
                }
                var Bytes = ImmutableArray.CreateBuilder<byte>(tokens.Count - 1);
                for (var Index = 1; Index < tokens.Count; Index++)
                {
                    if (!byte.TryParse(tokens[Index].Text, NumberStyles.None, CultureInfo.InvariantCulture, out var Value))
                    {
                        return (null, Diagnostic("WVA1005", line, tokens[Index].Column, "Byte value is outside the u8 range."));
                    }
                    Bytes.Add(Value);
                }
                return (new(Assemblyˉstatementˉkind.Bytes, null, 0, 0, Bytes.ToImmutable(), Span), null);
            case "u32":
                if (tokens.Count != 2 || !Tryˉu32(tokens[1].Text, out var U32))
                {
                    return (null, Diagnostic("WVA1005", Span, "'u32' requires one unsigned 32-bit decimal value."));
                }
                return (new(Assemblyˉstatementˉkind.U32, null, U32, 0, [], Span), null);
            case "i32":
                if (tokens.Count != 2 || !Tryˉi32(tokens[1].Text, out var I32))
                {
                    return (null, Diagnostic("WVA1005", Span, "'i32' requires one signed 32-bit decimal value."));
                }
                return (new(Assemblyˉstatementˉkind.I32, null, I32, 0, [], Span), null);
            case "zero":
                if (tokens.Count != 2 || !Tryˉu32(tokens[1].Text, out var Count) || Count == 0)
                {
                    return (null, Diagnostic("WVA1005", Span, "'zero' requires one positive u32 byte count."));
                }
                return (new(Assemblyˉstatementˉkind.Zero, null, Count, 0, [], Span), null);
            default:
                return (null, Diagnostic("WVA1002", Span, $"Unknown assembly statement '{tokens[0].Text}'."));
        }
    }

    private static bool Tryˉu32(string text, out uint value)
    {
        value = 0;
        return text.Length > 0 && text[0] is not ('-' or '+') &&
            uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool Tryˉi32(string text, out int value)
    {
        value = 0;
        return text.Length > 0 && text[0] != '+' &&
            int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    private static bool Tryˉregister(string text, out byte value)
    {
        value = text switch
        {
            "eax" => 0,
            "ecx" => 1,
            "edx" => 2,
            "ebx" => 3,
            "esp" => 4,
            "ebp" => 5,
            "esi" => 6,
            "edi" => 7,
            "r8d" => 8,
            "r9d" => 9,
            "r10d" => 10,
            "r11d" => 11,
            "r12d" => 12,
            "r13d" => 13,
            "r14d" => 14,
            "r15d" => 15,
            _ => byte.MaxValue,
        };
        return value != byte.MaxValue;
    }

    private static bool Tryˉtypedˉregister(string text, out Assemblyˉregister value)
    {
        var Registerˉ8 = text switch
        {
            "al" => 0, "cl" => 1, "dl" => 2, "bl" => 3,
            "spl" => 4, "bpl" => 5, "sil" => 6, "dil" => 7,
            "r8b" => 8, "r9b" => 9, "r10b" => 10, "r11b" => 11,
            "r12b" => 12, "r13b" => 13, "r14b" => 14, "r15b" => 15,
            _ => -1,
        };
        if (Registerˉ8 >= 0)
        {
            value = new((byte)Registerˉ8, 8);
            return true;
        }

        var Registerˉ16 = text switch
        {
            "ax" => 0, "cx" => 1, "dx" => 2, "bx" => 3,
            "sp" => 4, "bp" => 5, "si" => 6, "di" => 7,
            "r8w" => 8, "r9w" => 9, "r10w" => 10, "r11w" => 11,
            "r12w" => 12, "r13w" => 13, "r14w" => 14, "r15w" => 15,
            _ => -1,
        };
        if (Registerˉ16 >= 0)
        {
            value = new((byte)Registerˉ16, 16);
            return true;
        }

        if (Tryˉregister(text, out var Registerˉ32))
        {
            value = new(Registerˉ32, 32);
            return true;
        }

        var Registerˉ64 = text switch
        {
            "rax" => 0,
            "rcx" => 1,
            "rdx" => 2,
            "rbx" => 3,
            "rsp" => 4,
            "rbp" => 5,
            "rsi" => 6,
            "rdi" => 7,
            "r8" => 8,
            "r9" => 9,
            "r10" => 10,
            "r11" => 11,
            "r12" => 12,
            "r13" => 13,
            "r14" => 14,
            "r15" => 15,
            _ => -1,
        };
        value = Registerˉ64 < 0 ? default : new((byte)Registerˉ64, 64);
        return Registerˉ64 >= 0;
    }

    private static bool Tryˉcondition(string text, out byte value)
    {
        value = text switch
        {
            "overflow" => 0,
            "not_overflow" => 1,
            "below" => 2,
            "above_equal" => 3,
            "equal" => 4,
            "not_equal" => 5,
            "below_equal" => 6,
            "above" => 7,
            "sign" => 8,
            "not_sign" => 9,
            "parity" => 10,
            "not_parity" => 11,
            "less" => 12,
            "greater_equal" => 13,
            "less_equal" => 14,
            "greater" => 15,
            _ => byte.MaxValue,
        };
        return value != byte.MaxValue;
    }

    private static List<Token> Tokenize(string line)
    {
        var Result = new List<Token>();
        var Index = 0;
        while (Index < line.Length)
        {
            while (Index < line.Length && line[Index] is ' ' or '\t')
            {
                Index++;
            }
            if (Index >= line.Length || line[Index] == '#')
            {
                break;
            }
            var Start = Index;
            while (Index < line.Length && line[Index] is not (' ' or '\t' or '#'))
            {
                Index++;
            }
            Result.Add(new(line[Start..Index], Start + 1));
            if (Index < line.Length && line[Index] == '#')
            {
                break;
            }
        }
        return Result;
    }

    private static bool Matches(List<Token> tokens, string first, string second) =>
        tokens.Count == 2 && tokens[0].Text == first && tokens[1].Text == second;

    private static Assemblyˉparseˉresult Fail(string code, int line, int column, string message) =>
        new(null, Diagnostic(code, line, column, message));

    private static Assemblyˉdiagnostic Diagnostic(string code, Assemblyˉspan span, string message) =>
        Diagnostic(code, span.Line, span.Column, message);

    private static Assemblyˉdiagnostic Diagnostic(string code, int line, int column, string message) =>
        new(code, line, column, message);

    private sealed record Token(string Text, int Column);

    private sealed class Sectionˉbuilder(
        string name,
        Objectˉsectionˉkind kind,
        uint alignment,
        Assemblyˉspan span)
    {
        public string Name { get; } = name;
        public Objectˉsectionˉkind Kind { get; } = kind;
        public uint Alignment { get; } = alignment;
        public Assemblyˉspan Span { get; } = span;
        public List<Assemblyˉdefinition> Definitions { get; } = [];

        public Assemblyˉsection Build() => new(Name, Kind, Alignment, Definitions.ToImmutableArray(), Span);
    }

    private sealed class Definitionˉbuilder(string name, Assemblyˉspan span)
    {
        public string Name { get; } = name;
        public Assemblyˉspan Span { get; } = span;
        public List<Assemblyˉstatement> Statements { get; } = [];

        public Assemblyˉdefinition Build() => new(Name, Statements.ToImmutableArray(), Span);
    }
}
