using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.ObjectModel;

namespace Windvale.Linker;

public static class Linkˉcompiler
{
    public static Linkˉresult Link(ImmutableArray<Linkˉinput> inputs, Linkˉoptions options)
    {
        try
        {
            var Loaded = Loadˉinputs(inputs, options);
            var Candidate = Buildˉcandidate(Loaded, options);
            Flatˉimageˉverifier.Verify(Candidate);
            var Map = Linkˉmapˉwriter.Write(Candidate);
            return Linkˉresult.Succeeded(Candidate, Map);
        }
        catch (Linkˉfailure Failure)
        {
            return Linkˉresult.Failed(Failure.Diagnostic);
        }
    }

    private static ImmutableArray<Loadedˉobject> Loadˉinputs(
        ImmutableArray<Linkˉinput> inputs,
        Linkˉoptions options)
    {
        if (options is null || !Objectˉverifier.Isˉmachineˉname(options.Entryˉsymbol))
        {
            Fail("WVL1001", -1, "The link entry symbol must be a valid machine name.");
        }
        if (inputs.IsDefault || inputs.Length is < 1 or > Linkˉlimits.MAX_INPUT_OBJECTS)
        {
            Fail(
                "WVL1001",
                -1,
                $"A link requires 1 through {Linkˉlimits.MAX_INPUT_OBJECTS} input objects.");
        }

        var Result = ImmutableArray.CreateBuilder<Loadedˉobject>(inputs.Length);
        var Totalˉsections = 0;
        var Totalˉsymbols = 0;
        var Totalˉrelocations = 0;
        for (var Inputˉindex = 0; Inputˉindex < inputs.Length; Inputˉindex++)
        {
            var Input = inputs[Inputˉindex];
            if (Input is null || Input.Objectˉbytes.IsDefault)
            {
                Fail("WVL1002", Inputˉindex, "The input object byte value is not initialized.");
            }

            Verifiedˉobject Object;
            try
            {
                Object = Objectˉcodec.Readˉandˉverify(Input.Objectˉbytes.AsSpan());
            }
            catch (Objectˉexception Exception)
            {
                throw new Linkˉfailure(new(
                    "WVL1002",
                    Inputˉindex,
                    $"The input object is invalid: {Exception.Message}"));
            }

            Totalˉsections = Addˉbounded(
                Totalˉsections,
                Object.Value.Sections.Length,
                Linkˉlimits.MAX_TOTAL_SECTIONS,
                "section");
            Totalˉsymbols = Addˉbounded(
                Totalˉsymbols,
                Object.Value.Symbols.Length,
                Linkˉlimits.MAX_TOTAL_SYMBOLS,
                "symbol");
            Totalˉrelocations = Addˉbounded(
                Totalˉrelocations,
                Object.Value.Relocations.Length,
                Linkˉlimits.MAX_TOTAL_RELOCATIONS,
                "relocation");
            Result.Add(new(
                Inputˉindex,
                Input.Objectˉbytes,
                Object,
                Objectˉdigest.Calculateˉsha256(Input.Objectˉbytes.AsSpan())));
        }

        return Result.ToImmutable();
    }

    private static Linkedˉimageˉcandidate Buildˉcandidate(
        ImmutableArray<Loadedˉobject> inputs,
        Linkˉoptions options)
    {
        var Exports = Collectˉexports(inputs);
        Validateˉimports(inputs, Exports);
        if (!Exports.TryGetValue(options.Entryˉsymbol, out var Entry) ||
            Entry.Symbol.Kind != Objectˉsymbolˉkind.Function)
        {
            Fail(
                "WVL1007",
                -1,
                $"Entry symbol '{options.Entryˉsymbol}' is not a unique exported function.");
        }

        var Sections = Placeˉsections(inputs, options.Baseˉaddress, out var Imageˉlength);
        var Image = new byte[Imageˉlength];
        foreach (var Placement in Sections)
        {
            Placement.Section.Data.AsSpan().CopyTo(Image.AsSpan((int)Placement.Imageˉoffset));
        }

        var Definitions = Resolveˉdefinitions(inputs, Sections);
        var Definitionˉlookup = Definitions.ToDictionary(
            Definition => (Definition.Inputˉindex, Definition.Sourceˉsymbolˉindex));
        var Imports = Resolveˉimports(inputs, Exports, Definitionˉlookup);
        var Relocations = Applyˉrelocations(inputs, Sections, Definitionˉlookup, Exports, Image);
        var Entryˉdefinition = Definitionˉlookup[(Entry.Inputˉindex, Entry.Symbolˉindex)];
        return new(
            options,
            inputs,
            Sections,
            Definitions,
            Imports,
            Relocations,
            Image.ToImmutableArray(),
            Entryˉdefinition.Address);
    }

    private static Dictionary<string, Exportˉreference> Collectˉexports(
        ImmutableArray<Loadedˉobject> inputs)
    {
        var Result = new Dictionary<string, Exportˉreference>(StringComparer.Ordinal);
        foreach (var Input in inputs)
        {
            for (var Symbolˉindex = 0; Symbolˉindex < Input.Object.Value.Symbols.Length; Symbolˉindex++)
            {
                var Symbol = Input.Object.Value.Symbols[Symbolˉindex];
                if (Symbol.Binding != Objectˉsymbolˉbinding.Export)
                {
                    continue;
                }
                if (!Result.TryAdd(Symbol.Name, new(Input.Inputˉindex, Symbolˉindex, Symbol)))
                {
                    Fail(
                        "WVL1004",
                        Input.Inputˉindex,
                        $"Exported symbol '{Symbol.Name}' has more than one definition.");
                }
            }
        }
        return Result;
    }

    private static void Validateˉimports(
        ImmutableArray<Loadedˉobject> inputs,
        IReadOnlyDictionary<string, Exportˉreference> exports)
    {
        foreach (var Input in inputs)
        {
            foreach (var Symbol in Input.Object.Value.Symbols)
            {
                if (Symbol.Binding != Objectˉsymbolˉbinding.Import)
                {
                    continue;
                }
                if (!exports.TryGetValue(Symbol.Name, out var Provider))
                {
                    Fail(
                        "WVL1005",
                        Input.Inputˉindex,
                        $"Imported symbol '{Symbol.Name}' has no exported definition.");
                }
                if (Provider.Symbol.Kind != Symbol.Kind)
                {
                    Fail(
                        "WVL1006",
                        Input.Inputˉindex,
                        $"Imported symbol '{Symbol.Name}' does not match its exported symbol kind.");
                }
            }
        }
    }

    private static ImmutableArray<Sectionˉplacement> Placeˉsections(
        ImmutableArray<Loadedˉobject> inputs,
        uint baseˉaddress,
        out int imageˉlength)
    {
        var Result = ImmutableArray.CreateBuilder<Sectionˉplacement>();
        ulong Cursor = 0;
        foreach (var Kind in Enum.GetValues<Objectˉsectionˉkind>())
        {
            foreach (var Input in inputs)
            {
                for (var Sectionˉindex = 0; Sectionˉindex < Input.Object.Value.Sections.Length; Sectionˉindex++)
                {
                    var Section = Input.Object.Value.Sections[Sectionˉindex];
                    if (Section.Kind != Kind)
                    {
                        continue;
                    }

                    var Currentˉaddress = (ulong)baseˉaddress + Cursor;
                    var Alignedˉaddress = Alignˉup(Currentˉaddress, Section.Alignment);
                    if (Alignedˉaddress > uint.MaxValue)
                    {
                        Fail("WVL1008", Input.Inputˉindex, "Section alignment exceeds the u32 address space.");
                    }
                    var Offset = Alignedˉaddress - baseˉaddress;
                    var End = Offset + Section.Memoryˉsize;
                    if (End > Linkˉlimits.MAX_IMAGE_BYTES || (ulong)baseˉaddress + End > (ulong)uint.MaxValue + 1)
                    {
                        Fail(
                            "WVL1008",
                            Input.Inputˉindex,
                            $"Section '{Section.Name}' exceeds the flat-image size or u32 address space.");
                    }

                    Result.Add(new(
                        Result.Count,
                        Input.Inputˉindex,
                        Sectionˉindex,
                        Section,
                        (uint)Offset,
                        (uint)Alignedˉaddress));
                    Cursor = End;
                }
            }
        }
        imageˉlength = (int)Cursor;
        return Result.ToImmutable();
    }

    private static ImmutableArray<Symbolˉdefinition> Resolveˉdefinitions(
        ImmutableArray<Loadedˉobject> inputs,
        ImmutableArray<Sectionˉplacement> sections)
    {
        var Placements = sections.ToDictionary(
            Section => (Section.Inputˉindex, Section.Sourceˉsectionˉindex));
        var Result = ImmutableArray.CreateBuilder<Symbolˉdefinition>();
        foreach (var Input in inputs)
        {
            for (var Symbolˉindex = 0; Symbolˉindex < Input.Object.Value.Symbols.Length; Symbolˉindex++)
            {
                var Symbol = Input.Object.Value.Symbols[Symbolˉindex];
                if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
                {
                    continue;
                }
                var Placement = Placements[(Input.Inputˉindex, (int)Symbol.Sectionˉindex)];
                var Address = (ulong)Placement.Address + Symbol.Offset;
                if (Address > uint.MaxValue)
                {
                    Fail(
                        "WVL1008",
                        Input.Inputˉindex,
                        $"Symbol '{Symbol.Name}' exceeds the u32 address space.");
                }
                Result.Add(new(
                    Result.Count,
                    Input.Inputˉindex,
                    Symbolˉindex,
                    Symbol,
                    (uint)Address));
            }
        }
        return Result.ToImmutable();
    }

    private static ImmutableArray<Importˉresolution> Resolveˉimports(
        ImmutableArray<Loadedˉobject> inputs,
        IReadOnlyDictionary<string, Exportˉreference> exports,
        IReadOnlyDictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition> definitions)
    {
        var Result = ImmutableArray.CreateBuilder<Importˉresolution>();
        foreach (var Input in inputs)
        {
            for (var Symbolˉindex = 0; Symbolˉindex < Input.Object.Value.Symbols.Length; Symbolˉindex++)
            {
                var Symbol = Input.Object.Value.Symbols[Symbolˉindex];
                if (Symbol.Binding != Objectˉsymbolˉbinding.Import)
                {
                    continue;
                }
                var Provider = exports[Symbol.Name];
                var Definition = definitions[(Provider.Inputˉindex, Provider.Symbolˉindex)];
                Result.Add(new(
                    Result.Count,
                    Input.Inputˉindex,
                    Symbolˉindex,
                    Symbol,
                    Provider.Inputˉindex,
                    Provider.Symbolˉindex,
                    Definition.Address));
            }
        }
        return Result.ToImmutable();
    }

    private static ImmutableArray<Appliedˉrelocation> Applyˉrelocations(
        ImmutableArray<Loadedˉobject> inputs,
        ImmutableArray<Sectionˉplacement> sections,
        IReadOnlyDictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition> definitions,
        IReadOnlyDictionary<string, Exportˉreference> exports,
        byte[] image)
    {
        var Placements = sections.ToDictionary(
            Section => (Section.Inputˉindex, Section.Sourceˉsectionˉindex));
        var Result = ImmutableArray.CreateBuilder<Appliedˉrelocation>();
        foreach (var Input in inputs)
        {
            for (var Relocationˉindex = 0;
                Relocationˉindex < Input.Object.Value.Relocations.Length;
                Relocationˉindex++)
            {
                var Relocation = Input.Object.Value.Relocations[Relocationˉindex];
                var Placement = Placements[(Input.Inputˉindex, (int)Relocation.Sectionˉindex)];
                var Sourceˉsymbol = Input.Object.Value.Symbols[(int)Relocation.Symbolˉindex];
                int Targetˉinput;
                int Targetˉsymbol;
                if (Sourceˉsymbol.Binding == Objectˉsymbolˉbinding.Import)
                {
                    var Provider = exports[Sourceˉsymbol.Name];
                    Targetˉinput = Provider.Inputˉindex;
                    Targetˉsymbol = Provider.Symbolˉindex;
                }
                else
                {
                    Targetˉinput = Input.Inputˉindex;
                    Targetˉsymbol = (int)Relocation.Symbolˉindex;
                }

                var Target = definitions[(Targetˉinput, Targetˉsymbol)];
                var Patchˉoffset = checked(Placement.Imageˉoffset + Relocation.Offset);
                var Patchˉaddress = checked(Placement.Address + Relocation.Offset);
                long Value;
                if (Relocation.Kind == Objectˉrelocationˉkind.Absoluteˉu32)
                {
                    Value = (long)Target.Address + Relocation.Addend;
                    if (Value is < uint.MinValue or > uint.MaxValue)
                    {
                        Fail(
                            "WVL1009",
                            Input.Inputˉindex,
                            $"Absolute relocation to '{Sourceˉsymbol.Name}' exceeds u32.");
                    }
                    BinaryPrimitives.WriteUInt32LittleEndian(
                        image.AsSpan((int)Patchˉoffset, sizeof(uint)),
                        (uint)Value);
                }
                else
                {
                    Value = (long)Target.Address + Relocation.Addend - Patchˉaddress;
                    if (Value is < int.MinValue or > int.MaxValue)
                    {
                        Fail(
                            "WVL1010",
                            Input.Inputˉindex,
                            $"Relative relocation to '{Sourceˉsymbol.Name}' exceeds i32.");
                    }
                    BinaryPrimitives.WriteInt32LittleEndian(
                        image.AsSpan((int)Patchˉoffset, sizeof(int)),
                        (int)Value);
                }

                Result.Add(new(
                    Result.Count,
                    Input.Inputˉindex,
                    Relocationˉindex,
                    Relocation,
                    Patchˉoffset,
                    Patchˉaddress,
                    Sourceˉsymbol.Name,
                    Targetˉinput,
                    Targetˉsymbol,
                    Target.Address,
                    Value));
            }
        }
        return Result.ToImmutable();
    }

    private static int Addˉbounded(int current, int amount, int maximum, string name)
    {
        if (amount > maximum - current)
        {
            Fail("WVL1003", -1, $"The link exceeds the aggregate {name}-count limit {maximum}.");
        }
        return current + amount;
    }

    private static ulong Alignˉup(ulong value, uint alignment)
    {
        var Mask = (ulong)alignment - 1;
        return (value + Mask) & ~Mask;
    }

    [DoesNotReturn]
    private static void Fail(string code, int inputˉindex, string message) =>
        throw new Linkˉfailure(new(code, inputˉindex, message));

    private sealed record Exportˉreference(
        int Inputˉindex,
        int Symbolˉindex,
        Objectˉsymbol Symbol);
}
