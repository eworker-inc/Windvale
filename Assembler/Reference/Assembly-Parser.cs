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
                        Assemblyˉstatementˉkind.Storeˉu64 or Assemblyˉstatementˉkind.Loadˉaddress;
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
                        if (Statement.Kind is Assemblyˉstatementˉkind.Loadˉu32 or Assemblyˉstatementˉkind.Loadˉu64 or
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
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Destination) ||
                    !Tryˉtypedˉregister(tokens[2].Text, out var Source) ||
                    Destination.Width != Source.Width)
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires two registers of the same 32- or 64-bit width."));
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
                    _ => Assemblyˉstatementˉkind.Test,
                };
                return (new(Registerˉkind, null, 0, 0, [], Span, Destination, Source), null);
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
            case "load_address":
                var Requiredˉloadˉwidth = tokens[0].Text == "load_u32" ? (byte)32 : (byte)64;
                if (tokens.Count != 3 ||
                    !Tryˉtypedˉregister(tokens[1].Text, out var Loadˉregister) ||
                    Loadˉregister.Width != Requiredˉloadˉwidth ||
                    !Objectˉverifier.Isˉmachineˉname(tokens[2].Text))
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires a {Requiredˉloadˉwidth}-bit register and symbol name."));
                }
                var Loadˉkind = tokens[0].Text switch
                {
                    "load_u32" => Assemblyˉstatementˉkind.Loadˉu32,
                    "load_u64" => Assemblyˉstatementˉkind.Loadˉu64,
                    _ => Assemblyˉstatementˉkind.Loadˉaddress,
                };
                return (new(Loadˉkind, tokens[2].Text, 0, 0, [], Span, Loadˉregister), null);
            case "store_u32":
            case "store_u64":
                var Requiredˉstoreˉwidth = tokens[0].Text == "store_u32" ? (byte)32 : (byte)64;
                if (tokens.Count != 3 ||
                    !Objectˉverifier.Isˉmachineˉname(tokens[1].Text) ||
                    !Tryˉtypedˉregister(tokens[2].Text, out var Storeˉregister) ||
                    Storeˉregister.Width != Requiredˉstoreˉwidth)
                {
                    return (null, Diagnostic("WVA1003", Span, $"'{tokens[0].Text}' requires a symbol name and {Requiredˉstoreˉwidth}-bit register."));
                }
                return (new(
                    tokens[0].Text == "store_u32" ? Assemblyˉstatementˉkind.Storeˉu32 : Assemblyˉstatementˉkind.Storeˉu64,
                    tokens[1].Text,
                    0,
                    0,
                    [],
                    Span,
                    Storeˉregister), null);
            case "nop":
            case "return":
            case "trap":
            case "disable_interrupts":
            case "halt":
            case "out_u16":
            case "enable_page_protection":
            case "activate_page_table":
            case "syscall":
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
                    "enable_page_protection" => Assemblyˉstatementˉkind.Enableˉpageˉprotection,
                    "activate_page_table" => Assemblyˉstatementˉkind.Activateˉpageˉtable,
                    _ => Assemblyˉstatementˉkind.Syscall,
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
