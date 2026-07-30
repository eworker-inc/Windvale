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

        foreach (var Name in Included.OrderBy(Item => Item, StringComparer.Ordinal))
        {
            if (StringComparer.Ordinal.Equals(Name, Root.Name.Text))
            {
                continue;
            }

            Validateˉdependency(Modules[Name], diagnostics);
        }
        if (diagnostics.Count != 0)
        {
            return null;
        }

        foreach (var Name in Included.OrderBy(Item => Item, StringComparer.Ordinal))
        {
            if (StringComparer.Ordinal.Equals(Name, Root.Name.Text))
            {
                continue;
            }

            var Dependencyˉclosure = new HashSet<string>(StringComparer.Ordinal);
            Collectˉreachable(Modules[Name], Modules, Dependencyˉclosure);
            _ = Semanticˉcompiler.Compile(
                Buildˉcomposedˉsyntax(Modules[Name], Modules, Dependencyˉclosure),
                diagnostics);
            if (diagnostics.Count != 0)
            {
                return null;
            }
        }

        return Buildˉcomposedˉsyntax(Root, Modules, Included);
    }

    private static Moduleˉsyntax Buildˉcomposedˉsyntax(
        Moduleˉsyntax root,
        IReadOnlyDictionary<string, Moduleˉsyntax> modules,
        IReadOnlySet<string> included)
    {
        var Functions = ImmutableArray.CreateBuilder<Functionˉsyntax>();
        foreach (var Name in included.OrderBy(Item => Item, StringComparer.Ordinal))
        {
            if (StringComparer.Ordinal.Equals(Name, root.Name.Text))
            {
                continue;
            }

            Functions.AddRange(modules[Name].Functions.Select(Function => Function with { Isˉexported = false }));
        }
        Functions.AddRange(root.Functions);
        return root with
        {
            Imports = [],
            Functions = Functions.ToImmutable(),
        };
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

    private static void Validateˉdependency(Moduleˉsyntax module, Diagnosticˉbag diagnostics)
    {
        if (module.Profile.Kind != Tokenˉkind.Portable)
        {
            Report(
                diagnostics,
                "WVC0010",
                module.Profile.Span,
                $"Imported source module '{module.Name.Text}' must use profile portable.");
        }
        if (!module.Capabilities.IsEmpty || !module.Data.IsEmpty ||
            !module.Records.IsEmpty || !module.Enums.IsEmpty)
        {
            Report(
                diagnostics,
                "WVC0011",
                module.Name.Span,
                $"Imported source module '{module.Name.Text}' may contain only imports and functions in this Foundation slice.");
        }
        foreach (var Function in module.Functions)
        {
            if (!Function.Isˉexported)
            {
                Report(
                    diagnostics,
                    "WVC0012",
                    Function.Name.Span,
                    $"Function '{Function.Name.Text}' in imported module '{module.Name.Text}' must be declared export.");
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
