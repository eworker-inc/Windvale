using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Compiler;

public sealed record Sourceˉmoduleˉinput(string Sourceˉname, string Source);

internal static class Sourceˉmoduleˉcomposition
{
    public const int MAX_SOURCE_MODULES = 64;
    public const int MAX_TOTAL_SOURCE_CHARACTERS = 16 * 1024 * 1024;

    public static Moduleˉsyntax? Compose(
        Sourceˉmoduleˉinput root,
        IReadOnlyList<Sourceˉmoduleˉinput> dependencies,
        Diagnosticˉbag diagnostics)
    {
        if (dependencies.Count >= MAX_SOURCE_MODULES)
        {
            Report(
                diagnostics,
                "WVC0002",
                Spanˉatˉstart(root.Sourceˉname),
                $"A compilation may contain at most {MAX_SOURCE_MODULES} source modules.");
            return null;
        }

        var Inputs = new List<Sourceˉmoduleˉinput>(dependencies.Count + 1) { root };
        Inputs.AddRange(dependencies);

        var Seenˉsourceˉnames = new HashSet<string>(StringComparer.Ordinal);
        long Totalˉcharacters = 0;
        foreach (var Input in Inputs)
        {
            if (string.IsNullOrEmpty(Input.Sourceˉname))
            {
                Report(
                    diagnostics,
                    "WVC0003",
                    Spanˉatˉstart(root.Sourceˉname),
                    "Every source module input requires a nonempty source name.");
            }
            else if (!Seenˉsourceˉnames.Add(Input.Sourceˉname))
            {
                Report(
                    diagnostics,
                    "WVC0003",
                    Spanˉatˉstart(Input.Sourceˉname),
                    $"Source input '{Input.Sourceˉname}' is supplied more than once.");
            }

            if (Input.Source.Length > Seedˉcompiler.MAX_SOURCE_CHARACTERS)
            {
                Report(
                    diagnostics,
                    "WVC0001",
                    Spanˉatˉstart(Input.Sourceˉname),
                    $"Source '{Input.Sourceˉname}' exceeds the {Seedˉcompiler.MAX_SOURCE_CHARACTERS} character limit.");
            }
            Totalˉcharacters += Input.Source.Length;
        }

        if (Totalˉcharacters > MAX_TOTAL_SOURCE_CHARACTERS)
        {
            Report(
                diagnostics,
                "WVC0002",
                Spanˉatˉstart(root.Sourceˉname),
                $"The source-module set exceeds the {MAX_TOTAL_SOURCE_CHARACTERS} character limit.");
        }

        if (diagnostics.Count != 0)
        {
            return null;
        }

        var Parsed = new List<Moduleˉsyntax>(Inputs.Count);
        foreach (var Input in Inputs)
        {
            Parsed.Add(new Sourceˉparser(Input.Source, Input.Sourceˉname, diagnostics).Parseˉmodule());
        }
        if (diagnostics.Count != 0)
        {
            return null;
        }

        var Modules = new Dictionary<string, Moduleˉsyntax>(StringComparer.Ordinal);
        for (var Index = 0; Index < Parsed.Count; Index++)
        {
            var Module = Parsed[Index];
            if (Index != 0 && !Seedˉnames.Isˉidentifier(Module.Name.Text))
            {
                Report(
                    diagnostics,
                    "WVC0005",
                    Module.Name.Span,
                    $"Source module name '{Module.Name.Text}' is not a valid Windvale identifier.");
                continue;
            }
            if (!Modules.TryAdd(Module.Name.Text, Module))
            {
                Report(
                    diagnostics,
                    "WVC0004",
                    Module.Name.Span,
                    $"Source module '{Module.Name.Text}' is supplied more than once.");
            }
        }
        if (diagnostics.Count != 0)
        {
            return null;
        }

        var Root = Parsed[0];
        var States = new Dictionary<string, int>(StringComparer.Ordinal);
        var Included = new HashSet<string>(StringComparer.Ordinal);
        Visit(Root, Modules, States, Included, diagnostics);

        foreach (var Module in Parsed.Skip(1).OrderBy(Item => Item.Name.Text, StringComparer.Ordinal))
        {
            if (!Included.Contains(Module.Name.Text))
            {
                Report(
                    diagnostics,
                    "WVC0009",
                    Module.Name.Span,
                    $"Source module '{Module.Name.Text}' is supplied but is not reachable from root module '{Root.Name.Text}'.");
            }
        }
        if (diagnostics.Count != 0)
        {
            return null;
        }

        foreach (var Name in Included.OrderBy(Item => Item, StringComparer.Ordinal))
        {
            Validateˉcapabilityˉapproval(Modules[Name], Modules, diagnostics);
        }
        if (diagnostics.Count != 0)
        {
            return null;
        }

        return Buildˉqualifiedˉsyntax(Root, Modules, Included, diagnostics);
    }

    private enum Ownedˉdeclarationˉkind
    {
        Data,
        Constant,
        Record,
        Enum,
        Variant,
        Function,
    }

    private sealed record Ownedˉdeclaration(
        string Sourceˉname,
        string Internalˉname,
        bool Isˉexported,
        Ownedˉdeclarationˉkind Kind,
        Sourceˉspan Span);

    private sealed class Moduleˉbinding(Moduleˉsyntax syntax, int index)
    {
        public Moduleˉsyntax Syntax { get; } = syntax;
        public int Index { get; } = index;
        public Dictionary<string, Ownedˉdeclaration> Data { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Ownedˉdeclaration> Constants { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Ownedˉdeclaration> Records { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Ownedˉdeclaration> Enums { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Ownedˉdeclaration> Variants { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Ownedˉdeclaration> Functions { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Moduleˉbinding> Imports { get; } = new(StringComparer.Ordinal);
    }

    private static Moduleˉsyntax? Buildˉqualifiedˉsyntax(
        Moduleˉsyntax root,
        IReadOnlyDictionary<string, Moduleˉsyntax> modules,
        IReadOnlySet<string> included,
        Diagnosticˉbag diagnostics)
    {
        var Orderedˉmodules = new List<Moduleˉsyntax> { root };
        Orderedˉmodules.AddRange(included
            .Where(Name => !StringComparer.Ordinal.Equals(Name, root.Name.Text))
            .OrderBy(Name => Name, StringComparer.Ordinal)
            .Select(Name => modules[Name]));

        var Rootˉnames = root.Data.Select(Item => Item.Name.Text)
            .Concat(root.Constants.Select(Item => Item.Name.Text))
            .Concat(root.Records.Select(Item => Item.Name.Text))
            .Concat(root.Enums.Select(Item => Item.Name.Text))
            .Concat(root.Variants.Select(Item => Item.Name.Text))
            .Concat(root.Functions.Select(Item => Item.Name.Text))
            .ToHashSet(StringComparer.Ordinal);
        var Generatedˉnames = new HashSet<string>(StringComparer.Ordinal);
        var Bindings = new Dictionary<string, Moduleˉbinding>(StringComparer.Ordinal);
        for (var Moduleˉindex = 0; Moduleˉindex < Orderedˉmodules.Count; Moduleˉindex++)
        {
            var Module = Orderedˉmodules[Moduleˉindex];
            var Binding = new Moduleˉbinding(Module, Moduleˉindex);
            Bindings.Add(Module.Name.Text, Binding);
            Buildˉdeclarations(Binding, Moduleˉindex == 0, Rootˉnames, Generatedˉnames, diagnostics);
        }

        foreach (var Binding in Bindings.Values.OrderBy(Item => Item.Index))
        {
            foreach (var Import in Binding.Syntax.Imports)
            {
                if (!Isˉalias(Import.Alias.Text))
                {
                    Report(
                        diagnostics,
                        "WVC0015",
                        Import.Alias.Span,
                        $"Import alias '{Import.Alias.Text}' must be a capitalized Windvale identifier.");
                    continue;
                }
                if (!Binding.Imports.TryAdd(Import.Alias.Text, Bindings[Import.Name.Text]))
                {
                    Report(
                        diagnostics,
                        "WVC0015",
                        Import.Alias.Span,
                        $"Module '{Binding.Syntax.Name.Text}' declares import alias '{Import.Alias.Text}' more than once.");
                }
            }
        }
        if (diagnostics.Count != 0)
        {
            return null;
        }

        var Dependencyˉorder = new List<Moduleˉbinding>();
        var Orderedˉset = new HashSet<string>(StringComparer.Ordinal);
        Addˉdependencyˉorder(Bindings[root.Name.Text], Orderedˉset, Dependencyˉorder);

        var Rewritten = Dependencyˉorder
            .Select(Binding => Rewriteˉmodule(Binding, Binding.Index == 0, diagnostics))
            .ToArray();
        if (diagnostics.Count != 0)
        {
            return null;
        }

        var Data = ImmutableArray.CreateBuilder<Dataˉsyntax>();
        var Constants = ImmutableArray.CreateBuilder<Constantˉsyntax>();
        var Records = ImmutableArray.CreateBuilder<Recordˉsyntax>();
        var Enums = ImmutableArray.CreateBuilder<Enumˉsyntax>();
        var Variants = ImmutableArray.CreateBuilder<Variantˉsyntax>();
        var Functions = ImmutableArray.CreateBuilder<Functionˉsyntax>();
        foreach (var Module in Rewritten)
        {
            Data.AddRange(Module.Data);
            Constants.AddRange(Module.Constants);
            Records.AddRange(Module.Records);
            Enums.AddRange(Module.Enums);
            Variants.AddRange(Module.Variants);
            Functions.AddRange(Module.Functions);
        }

        return root with
        {
            Imports = [],
            Data = Data.ToImmutable(),
            Constants = Constants.ToImmutable(),
            Records = Records.ToImmutable(),
            Enums = Enums.ToImmutable(),
            Variants = Variants.ToImmutable(),
            Functions = Functions.ToImmutable(),
        };
    }

    private static void Addˉdependencyˉorder(
        Moduleˉbinding binding,
        HashSet<string> included,
        List<Moduleˉbinding> order)
    {
        if (!included.Add(binding.Syntax.Name.Text))
        {
            return;
        }
        foreach (var Dependency in binding.Imports.Values
                     .Distinct()
                     .OrderBy(Item => Item.Syntax.Name.Text, StringComparer.Ordinal))
        {
            Addˉdependencyˉorder(Dependency, included, order);
        }
        order.Add(binding);
    }

    private static void Buildˉdeclarations(
        Moduleˉbinding binding,
        bool isˉroot,
        IReadOnlySet<string> rootˉnames,
        HashSet<string> generatedˉnames,
        Diagnosticˉbag diagnostics)
    {
        var Values = new HashSet<string>(StringComparer.Ordinal);
        var Nominals = new HashSet<string>(StringComparer.Ordinal);
        var Callables = new HashSet<string>(StringComparer.Ordinal);

        for (var Index = 0; Index < binding.Syntax.Data.Length; Index++)
        {
            var Item = binding.Syntax.Data[Index];
            Addˉdeclaration(
                binding.Data,
                Values,
                binding,
                Item.Name,
                Item.Isˉexported,
                Ownedˉdeclarationˉkind.Data,
                isˉroot ? Item.Name.Text : Makeˉinternalˉname(binding.Index, 'D', Index, rootˉnames, generatedˉnames),
                diagnostics,
                "value");
        }
        for (var Index = 0; Index < binding.Syntax.Constants.Length; Index++)
        {
            var Item = binding.Syntax.Constants[Index];
            Addˉdeclaration(
                binding.Constants,
                Values,
                binding,
                Item.Name,
                Item.Isˉexported,
                Ownedˉdeclarationˉkind.Constant,
                isˉroot ? Item.Name.Text : Makeˉinternalˉname(binding.Index, 'C', Index, rootˉnames, generatedˉnames),
                diagnostics,
                "value");
        }
        for (var Index = 0; Index < binding.Syntax.Records.Length; Index++)
        {
            var Item = binding.Syntax.Records[Index];
            var Internal = isˉroot
                ? Item.Name.Text
                : Makeˉinternalˉname(binding.Index, 'R', Index, rootˉnames, generatedˉnames);
            Addˉdeclaration(
                binding.Records,
                Nominals,
                binding,
                Item.Name,
                Item.Isˉexported,
                Ownedˉdeclarationˉkind.Record,
                Internal,
                diagnostics,
                "nominal type");
            if (!Callables.Add(Item.Name.Text))
            {
                Reportˉduplicate(binding, Item.Name, "callable", diagnostics);
            }
        }
        for (var Index = 0; Index < binding.Syntax.Enums.Length; Index++)
        {
            var Item = binding.Syntax.Enums[Index];
            Addˉdeclaration(
                binding.Enums,
                Nominals,
                binding,
                Item.Name,
                Item.Isˉexported,
                Ownedˉdeclarationˉkind.Enum,
                isˉroot ? Item.Name.Text : Makeˉinternalˉname(binding.Index, 'E', Index, rootˉnames, generatedˉnames),
                diagnostics,
                "nominal type");
        }
        for (var Index = 0; Index < binding.Syntax.Variants.Length; Index++)
        {
            var Item = binding.Syntax.Variants[Index];
            Addˉdeclaration(
                binding.Variants,
                Nominals,
                binding,
                Item.Name,
                Item.Isˉexported,
                Ownedˉdeclarationˉkind.Variant,
                isˉroot ? Item.Name.Text : Makeˉinternalˉname(binding.Index, 'V', Index, rootˉnames, generatedˉnames),
                diagnostics,
                "nominal type");
        }
        for (var Index = 0; Index < binding.Syntax.Functions.Length; Index++)
        {
            var Item = binding.Syntax.Functions[Index];
            Addˉdeclaration(
                binding.Functions,
                Callables,
                binding,
                Item.Name,
                Item.Isˉexported,
                Ownedˉdeclarationˉkind.Function,
                isˉroot ? Item.Name.Text : Makeˉinternalˉname(binding.Index, 'F', Index, rootˉnames, generatedˉnames),
                diagnostics,
                "callable");
        }
    }

    private static void Addˉdeclaration(
        Dictionary<string, Ownedˉdeclaration> declarations,
        HashSet<string> namespaceˉnames,
        Moduleˉbinding binding,
        Syntaxˉtoken name,
        bool isˉexported,
        Ownedˉdeclarationˉkind kind,
        string internalˉname,
        Diagnosticˉbag diagnostics,
        string namespaceˉname)
    {
        if (!namespaceˉnames.Add(name.Text) ||
            !declarations.TryAdd(
                name.Text,
                new(name.Text, internalˉname, isˉexported, kind, name.Span)))
        {
            Reportˉduplicate(binding, name, namespaceˉname, diagnostics);
        }
    }

    private static void Reportˉduplicate(
        Moduleˉbinding binding,
        Syntaxˉtoken name,
        string namespaceˉname,
        Diagnosticˉbag diagnostics)
    {
        Report(
            diagnostics,
            "WVC0016",
            name.Span,
            $"Module '{binding.Syntax.Name.Text}' declares '{name.Text}' more than once in its {namespaceˉname} namespace.");
    }

    private static string Makeˉinternalˉname(
        int module,
        char kind,
        int declaration,
        IReadOnlySet<string> rootˉnames,
        HashSet<string> generatedˉnames)
    {
        var Name = kind == 'C'
            ? $"__WV_M{module}_C{declaration}"
            : $"__WvM{module}{kind}{declaration}";
        while (rootˉnames.Contains(Name) || !generatedˉnames.Add(Name))
        {
            Name += "_";
        }
        return Name;
    }

    private static bool Isˉalias(string alias) =>
        Seedˉnames.Isˉidentifier(alias) && alias[0] is >= 'A' and <= 'Z';

    private static Moduleˉsyntax Rewriteˉmodule(
        Moduleˉbinding binding,
        bool isˉroot,
        Diagnosticˉbag diagnostics)
    {
        return binding.Syntax with
        {
            Imports = [],
            Capabilities = isˉroot ? binding.Syntax.Capabilities : [],
            Data = [.. binding.Syntax.Data.Select(Item => Item with
            {
                Isˉexported = isˉroot && Item.Isˉexported,
                Name = Renameˉtoken(Item.Name, binding.Data[Item.Name.Text].Internalˉname),
            })],
            Constants = [.. binding.Syntax.Constants.Select(Item => Item with
            {
                Isˉexported = isˉroot && Item.Isˉexported,
                Name = Renameˉtoken(Item.Name, binding.Constants[Item.Name.Text].Internalˉname),
                Type = Rewriteˉtype(binding, Item.Type, diagnostics),
                Initializer = Rewriteˉexpression(binding, Item.Initializer, diagnostics),
            })],
            Records = [.. binding.Syntax.Records.Select(Item => Item with
            {
                Isˉexported = isˉroot && Item.Isˉexported,
                Name = Renameˉtoken(Item.Name, binding.Records[Item.Name.Text].Internalˉname),
                Fields = [.. Item.Fields.Select(Field => Field with
                {
                    Type = Rewriteˉtype(binding, Field.Type, diagnostics),
                })],
            })],
            Enums = [.. binding.Syntax.Enums.Select(Item => Item with
            {
                Isˉexported = isˉroot && Item.Isˉexported,
                Name = Renameˉtoken(Item.Name, binding.Enums[Item.Name.Text].Internalˉname),
            })],
            Variants = [.. binding.Syntax.Variants.Select(Item => Item with
            {
                Isˉexported = isˉroot && Item.Isˉexported,
                Name = Renameˉtoken(Item.Name, binding.Variants[Item.Name.Text].Internalˉname),
                Cases = [.. Item.Cases.Select(Case => Case with
                {
                    Payloadˉtype = Case.Payloadˉtype is null
                        ? null
                        : Rewriteˉtype(binding, Case.Payloadˉtype, diagnostics),
                })],
            })],
            Functions = [.. binding.Syntax.Functions.Select(Item => Item with
            {
                Isˉexported = isˉroot && Item.Isˉexported,
                Name = Renameˉtoken(Item.Name, binding.Functions[Item.Name.Text].Internalˉname),
                Parameters = [.. Item.Parameters.Select(Parameter => Parameter with
                {
                    Type = Rewriteˉtype(binding, Parameter.Type, diagnostics),
                })],
                Returnˉtype = Rewriteˉtype(binding, Item.Returnˉtype, diagnostics),
                Body = Rewriteˉblock(binding, Item.Body, diagnostics),
            })],
        };
    }

    private static Syntaxˉtoken Renameˉtoken(Syntaxˉtoken token, string name) =>
        token with { Text = name };

    private static Typeˉsyntax Rewriteˉtype(
        Moduleˉbinding binding,
        Typeˉsyntax type,
        Diagnosticˉbag diagnostics)
    {
        if (type.Kind is Typeˉsyntaxˉkind.Sequence or Typeˉsyntaxˉkind.Builder)
        {
            return type with
            {
                Elementˉtype = type.Elementˉtype is null
                    ? null
                    : Rewriteˉtype(binding, type.Elementˉtype, diagnostics),
            };
        }
        if (type.Kind != Typeˉsyntaxˉkind.Named || type.Name is null)
        {
            return type;
        }
        return type with { Name = Resolveˉnominal(binding, type.Name, type.Span, diagnostics) };
    }

    private static Blockˉstatementˉsyntax Rewriteˉblock(
        Moduleˉbinding binding,
        Blockˉstatementˉsyntax block,
        Diagnosticˉbag diagnostics) =>
        block with
        {
            Statements = [.. block.Statements.Select(Statement =>
                Rewriteˉstatement(binding, Statement, diagnostics))],
        };

    private static Statementˉsyntax Rewriteˉstatement(
        Moduleˉbinding binding,
        Statementˉsyntax statement,
        Diagnosticˉbag diagnostics)
    {
        return statement switch
        {
            Blockˉstatementˉsyntax Block => Rewriteˉblock(binding, Block, diagnostics),
            Localˉdeclarationˉstatementˉsyntax Local => Local with
            {
                Type = Local.Type is null ? null : Rewriteˉtype(binding, Local.Type, diagnostics),
                Initializer = Rewriteˉexpression(binding, Local.Initializer, diagnostics),
            },
            Assignmentˉstatementˉsyntax Assignment => Assignment with
            {
                Value = Rewriteˉexpression(binding, Assignment.Value, diagnostics),
            },
            Expressionˉstatementˉsyntax Expression => Expression with
            {
                Expression = Rewriteˉexpression(binding, Expression.Expression, diagnostics),
            },
            Ifˉstatementˉsyntax If => If with
            {
                Condition = Rewriteˉexpression(binding, If.Condition, diagnostics),
                Then = Rewriteˉblock(binding, If.Then, diagnostics),
                Else = If.Else is null ? null : Rewriteˉblock(binding, If.Else, diagnostics),
            },
            Whileˉstatementˉsyntax While => While with
            {
                Condition = Rewriteˉexpression(binding, While.Condition, diagnostics),
                Body = Rewriteˉblock(binding, While.Body, diagnostics),
            },
            Pushˉstatementˉsyntax Push => Push with
            {
                Value = Rewriteˉexpression(binding, Push.Value, diagnostics),
            },
            Forˉstatementˉsyntax For => For with
            {
                Sequence = Rewriteˉexpression(binding, For.Sequence, diagnostics),
                Body = Rewriteˉblock(binding, For.Body, diagnostics),
            },
            Matchˉstatementˉsyntax Match => Match with
            {
                Value = Rewriteˉexpression(binding, Match.Value, diagnostics),
                Cases = [.. Match.Cases.Select(Case => Case with
                {
                    Nominalˉname = Resolveˉnominal(binding, Case.Nominalˉname, Case.Span, diagnostics),
                    Body = Rewriteˉblock(binding, Case.Body, diagnostics),
                })],
            },
            Returnˉstatementˉsyntax Return => Return with
            {
                Value = Return.Value is null ? null : Rewriteˉexpression(binding, Return.Value, diagnostics),
            },
            _ => statement,
        };
    }

    private static Expressionˉsyntax Rewriteˉexpression(
        Moduleˉbinding binding,
        Expressionˉsyntax expression,
        Diagnosticˉbag diagnostics)
    {
        switch (expression)
        {
            case Nameˉexpressionˉsyntax Name:
                return Rewriteˉname(binding, Name, diagnostics);
            case Unaryˉexpressionˉsyntax Unary:
                return Unary with { Operand = Rewriteˉexpression(binding, Unary.Operand, diagnostics) };
            case Binaryˉexpressionˉsyntax Binary:
                return Binary with
                {
                    Left = Rewriteˉexpression(binding, Binary.Left, diagnostics),
                    Right = Rewriteˉexpression(binding, Binary.Right, diagnostics),
                };
            case Callˉexpressionˉsyntax Call:
                return Call with
                {
                    Name = Resolveˉcallable(binding, Call.Name, Call.Span, diagnostics),
                    Arguments = [.. Call.Arguments.Select(Argument =>
                        Rewriteˉexpression(binding, Argument, diagnostics))],
                };
            case Builderˉexpressionˉsyntax Builder:
                return Builder with
                {
                    Type = Rewriteˉtype(binding, Builder.Type, diagnostics),
                };
            case Recordˉexpressionˉsyntax Record:
                return Record with
                {
                    Name = Resolveˉrecord(binding, Record.Name, Record.Span, diagnostics),
                    Fields = [.. Record.Fields.Select(Field => Field with
                    {
                        Value = Rewriteˉexpression(binding, Field.Value, diagnostics),
                    })],
                };
            case Indexˉexpressionˉsyntax Index:
                return Index with
                {
                    Name = Resolveˉvalue(binding, Index.Name, Index.Span, diagnostics, allowˉlocal: true),
                    Index = Rewriteˉexpression(binding, Index.Index, diagnostics),
                };
            case Fieldˉexpressionˉsyntax Field:
                if (binding.Imports.TryGetValue(Field.Target, out var Imported))
                {
                    var Value = Resolveˉexported(
                        binding,
                        Imported,
                        Field.Field,
                        Field.Span,
                        diagnostics,
                        Imported.Data,
                        Imported.Constants);
                    return new Nameˉexpressionˉsyntax(Value, Field.Span);
                }
                if (binding.Enums.TryGetValue(Field.Target, out var Enum))
                {
                    return Field with { Target = Enum.Internalˉname };
                }
                if (binding.Variants.TryGetValue(Field.Target, out var Variant))
                {
                    return Field with { Target = Variant.Internalˉname };
                }
                return Field;
            default:
                return expression;
        }
    }

    private static Expressionˉsyntax Rewriteˉname(
        Moduleˉbinding binding,
        Nameˉexpressionˉsyntax expression,
        Diagnosticˉbag diagnostics)
    {
        var Parts = expression.Name.Split('.');
        if (Parts.Length == 1)
        {
            if (binding.Data.TryGetValue(expression.Name, out var Data))
            {
                return expression with { Name = Data.Internalˉname };
            }
            if (binding.Constants.TryGetValue(expression.Name, out var Constant))
            {
                return expression with { Name = Constant.Internalˉname };
            }
            return expression;
        }
        if (Parts.Length == 2 && binding.Imports.TryGetValue(Parts[0], out var Imported))
        {
            return expression with
            {
                Name = Resolveˉexported(
                    binding,
                    Imported,
                    Parts[1],
                    expression.Span,
                    diagnostics,
                    Imported.Data,
                    Imported.Constants),
            };
        }
        if (Parts.Length == 3 && binding.Imports.TryGetValue(Parts[0], out Imported))
        {
            var Enum = Resolveˉexported(
                binding,
                Imported,
                Parts[1],
                expression.Span,
                diagnostics,
                Imported.Enums);
            return new Fieldˉexpressionˉsyntax(Enum, Parts[2], expression.Span);
        }
        Report(
            diagnostics,
            "WVC0019",
            expression.Span,
            $"Qualified name '{expression.Name}' does not begin with a direct import alias.");
        return expression;
    }

    private static string Resolveˉnominal(
        Moduleˉbinding binding,
        string name,
        Sourceˉspan span,
        Diagnosticˉbag diagnostics)
    {
        var Parts = name.Split('.');
        if (Parts.Length == 1)
        {
            if (binding.Records.TryGetValue(name, out var Record))
            {
                return Record.Internalˉname;
            }
            if (binding.Enums.TryGetValue(name, out var Enum))
            {
                return Enum.Internalˉname;
            }
            if (binding.Variants.TryGetValue(name, out var Variant))
            {
                return Variant.Internalˉname;
            }
            return name;
        }
        if (Parts.Length == 2 && binding.Imports.TryGetValue(Parts[0], out var Imported))
        {
            return Resolveˉexported(
                binding,
                Imported,
                Parts[1],
                span,
                diagnostics,
                Imported.Records,
                Imported.Enums,
                Imported.Variants);
        }
        Report(
            diagnostics,
            "WVC0019",
            span,
            $"Qualified type '{name}' does not name a declaration through a direct import alias.");
        return name;
    }

    private static string Resolveˉrecord(
        Moduleˉbinding binding,
        string name,
        Sourceˉspan span,
        Diagnosticˉbag diagnostics)
    {
        var Parts = name.Split('.');
        if (Parts.Length == 1)
        {
            return binding.Records.TryGetValue(name, out var Record) ? Record.Internalˉname : name;
        }
        if (Parts.Length == 2 && binding.Imports.TryGetValue(Parts[0], out var Imported))
        {
            return Resolveˉexported(binding, Imported, Parts[1], span, diagnostics, Imported.Records);
        }
        Report(diagnostics, "WVC0019", span, $"Record constructor '{name}' does not use a direct import alias.");
        return name;
    }

    private static string Resolveˉcallable(
        Moduleˉbinding binding,
        string name,
        Sourceˉspan span,
        Diagnosticˉbag diagnostics)
    {
        var Parts = name.Split('.');
        if (Parts.Length == 1)
        {
            if (binding.Functions.TryGetValue(name, out var Function))
            {
                return Function.Internalˉname;
            }
            if (binding.Records.TryGetValue(name, out var Record))
            {
                return Record.Internalˉname;
            }
            if (name is "length" or Foundationˉintrinsics.ENUM_NAME ||
                Foundationˉintrinsics.Tryˉget(name, out _))
            {
                return name;
            }
            Report(
                diagnostics,
                "WVC2065",
                span,
                $"Function or capability '{name}' is not declared in module '{binding.Syntax.Name.Text}'.");
            return name;
        }
        if (Parts.Length == 2 && binding.Variants.TryGetValue(Parts[0], out var Variant))
        {
            return Variant.Internalˉname + "." + Parts[1];
        }
        if (Parts.Length == 2 && binding.Imports.TryGetValue(Parts[0], out var Imported))
        {
            return Resolveˉexported(
                binding,
                Imported,
                Parts[1],
                span,
                diagnostics,
                Imported.Functions,
                Imported.Records);
        }
        if (Parts.Length == 3 && binding.Imports.TryGetValue(Parts[0], out Imported))
        {
            var Importedˉvariant = Resolveˉexported(
                binding,
                Imported,
                Parts[1],
                span,
                diagnostics,
                Imported.Variants);
            return Importedˉvariant + "." + Parts[2];
        }
        if (Capabilityˉcatalog.Tryˉget(name, out _))
        {
            return name;
        }
        Report(diagnostics, "WVC0019", span, $"Callable '{name}' does not use a direct import alias.");
        return name;
    }

    private static string Resolveˉvalue(
        Moduleˉbinding binding,
        string name,
        Sourceˉspan span,
        Diagnosticˉbag diagnostics,
        bool allowˉlocal)
    {
        var Parts = name.Split('.');
        if (Parts.Length == 1)
        {
            if (binding.Data.TryGetValue(name, out var Data))
            {
                return Data.Internalˉname;
            }
            if (binding.Constants.TryGetValue(name, out var Constant))
            {
                return Constant.Internalˉname;
            }
            return name;
        }
        if (Parts.Length == 2 && binding.Imports.TryGetValue(Parts[0], out var Imported))
        {
            return Resolveˉexported(
                binding,
                Imported,
                Parts[1],
                span,
                diagnostics,
                Imported.Data,
                Imported.Constants);
        }
        if (!allowˉlocal)
        {
            Report(diagnostics, "WVC0019", span, $"Value '{name}' does not use a direct import alias.");
        }
        return name;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060")]
    private static string Resolveˉexported(
        Moduleˉbinding owner,
        Moduleˉbinding target,
        string name,
        Sourceˉspan span,
        Diagnosticˉbag diagnostics,
        params Dictionary<string, Ownedˉdeclaration>[] namespaces)
    {
        foreach (var Namespace in namespaces)
        {
            if (!Namespace.TryGetValue(name, out var Declaration))
            {
                continue;
            }
            if (!Declaration.Isˉexported)
            {
                Report(
                    diagnostics,
                    "WVC0017",
                    span,
                    $"Declaration '{name}' in module '{target.Syntax.Name.Text}' is private.");
            }
            return Declaration.Internalˉname;
        }
        Report(
            diagnostics,
            "WVC0018",
            span,
            $"Module '{target.Syntax.Name.Text}' has no exported declaration named '{name}'.");
        return name;
    }

    private static void Collectˉreachable(
        Moduleˉsyntax module,
        IReadOnlyDictionary<string, Moduleˉsyntax> modules,
        HashSet<string> included)
    {
        if (!included.Add(module.Name.Text))
        {
            return;
        }
        foreach (var Import in module.Imports)
        {
            Collectˉreachable(modules[Import.Name.Text], modules, included);
        }
    }

    private static void Visit(
        Moduleˉsyntax module,
        IReadOnlyDictionary<string, Moduleˉsyntax> modules,
        Dictionary<string, int> states,
        HashSet<string> included,
        Diagnosticˉbag diagnostics)
    {
        states[module.Name.Text] = 1;
        included.Add(module.Name.Text);
        var Seenˉimports = new HashSet<string>(StringComparer.Ordinal);
        foreach (var Import in module.Imports)
        {
            if (!Seedˉnames.Isˉidentifier(Import.Name.Text))
            {
                Report(
                    diagnostics,
                    "WVC0005",
                    Import.Name.Span,
                    $"Imported module name '{Import.Name.Text}' is not a valid Windvale identifier.");
                continue;
            }
            if (!Seenˉimports.Add(Import.Name.Text))
            {
                Report(
                    diagnostics,
                    "WVC0006",
                    Import.Name.Span,
                    $"Module '{module.Name.Text}' imports '{Import.Name.Text}' more than once.");
                continue;
            }
            if (!modules.TryGetValue(Import.Name.Text, out var Dependency))
            {
                Report(
                    diagnostics,
                    "WVC0007",
                    Import.Name.Span,
                    $"Imported source module '{Import.Name.Text}' was not supplied.");
                continue;
            }

            if (!Canˉimportˉprofile(module.Profile.Kind, Dependency.Profile.Kind))
            {
                Report(
                    diagnostics,
                    "WVC0010",
                    Import.Name.Span,
                    $"Module '{module.Name.Text}' with profile {module.Profile.Text} cannot import module " +
                    $"'{Dependency.Name.Text}' with profile {Dependency.Profile.Text}.");
            }

            if (states.GetValueOrDefault(Dependency.Name.Text) == 1)
            {
                Report(
                    diagnostics,
                    "WVC0008",
                    Import.Name.Span,
                    $"Source-module import cycle reaches '{Dependency.Name.Text}'.");
                continue;
            }
            if (states.GetValueOrDefault(Dependency.Name.Text) == 0)
            {
                Visit(Dependency, modules, states, included, diagnostics);
            }
        }
        states[module.Name.Text] = 2;
    }

    private static bool Canˉimportˉprofile(Tokenˉkind importer, Tokenˉkind dependency) =>
        dependency == Tokenˉkind.Portable ||
        importer == Tokenˉkind.System ||
        importer == dependency;

    private static void Validateˉcapabilityˉapproval(
        Moduleˉsyntax module,
        IReadOnlyDictionary<string, Moduleˉsyntax> modules,
        Diagnosticˉbag diagnostics)
    {
        var Approved = module.Capabilities
            .Select(Capability => Capability.Name)
            .ToHashSet(StringComparer.Ordinal);
        var Closure = new HashSet<string>(StringComparer.Ordinal);
        Collectˉreachable(module, modules, Closure);
        var Required = Closure
            .Where(Name => !StringComparer.Ordinal.Equals(Name, module.Name.Text))
            .SelectMany(Name => modules[Name].Capabilities)
            .Select(Capability => Capability.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(Name => Name, StringComparer.Ordinal);
        foreach (var Capability in Required)
        {
            if (!Approved.Contains(Capability))
            {
                Report(
                    diagnostics,
                    "WVC0013",
                    module.Name.Span,
                    $"Module '{module.Name.Text}' must explicitly approve transitive capability '{Capability}'.");
            }
        }
    }

    private static Sourceˉspan Spanˉatˉstart(string sourceˉname) => new(0, 0, 1, 1, sourceˉname);

    private static void Report(
        Diagnosticˉbag diagnostics,
        string code,
        Sourceˉspan span,
        string message)
    {
        diagnostics.Report(code, "composition", span, message);
    }
}
