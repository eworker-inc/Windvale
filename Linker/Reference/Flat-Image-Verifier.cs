using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Windvale.ObjectModel;

namespace Windvale.Linker;

internal static class Flatˉimageˉverifier
{
    public static void Verify(Linkedˉimageˉcandidate candidate)
    {
        var Placements = Verifyˉsections(candidate);
        var Definitions = Verifyˉdefinitions(candidate, Placements);
        var Exports = Verifyˉexports(candidate, Definitions);
        Verifyˉimports(candidate, Definitions, Exports);
        var Expectedˉimage = Copyˉsectionˉdata(candidate, Placements);
        Verifyˉrelocations(candidate, Placements, Definitions, Exports, Expectedˉimage);
        Require(
            Expectedˉimage.AsSpan().SequenceEqual(candidate.Imageˉbytes.AsSpan()),
            "The linked image bytes do not match independently reconstructed bytes.");

        Require(
            Exports.TryGetValue(candidate.Options.Entryˉsymbol, out var Entry) &&
                Entry.Definition.Symbol.Kind == Objectˉsymbolˉkind.Function &&
                Entry.Definition.Address == candidate.Entryˉaddress,
            "The linked entry address does not identify the requested exported function.");
    }

    private static Dictionary<(int Inputˉindex, int Sectionˉindex), Sectionˉplacement> Verifyˉsections(
        Linkedˉimageˉcandidate candidate)
    {
        var Expectedˉcount = candidate.Inputs.Sum(Input => Input.Object.Value.Sections.Length);
        Require(candidate.Sections.Length == Expectedˉcount, "The linked section count is inconsistent.");
        var Result = new Dictionary<(int Inputˉindex, int Sectionˉindex), Sectionˉplacement>();
        ulong Cursor = 0;
        var Layoutˉindex = 0;
        foreach (var Kind in Enum.GetValues<Objectˉsectionˉkind>())
        {
            foreach (var Input in candidate.Inputs)
            {
                for (var Sourceˉindex = 0; Sourceˉindex < Input.Object.Value.Sections.Length; Sourceˉindex++)
                {
                    var Section = Input.Object.Value.Sections[Sourceˉindex];
                    if (Section.Kind != Kind)
                    {
                        continue;
                    }

                    var Alignmentˉmask = (ulong)Section.Alignment - 1;
                    var Address = ((ulong)candidate.Options.Baseˉaddress + Cursor + Alignmentˉmask) & ~Alignmentˉmask;
                    var Offset = Address - candidate.Options.Baseˉaddress;
                    var Actual = candidate.Sections[Layoutˉindex];
                    Require(
                        Actual.Index == Layoutˉindex &&
                            Actual.Inputˉindex == Input.Inputˉindex &&
                            Actual.Sourceˉsectionˉindex == Sourceˉindex &&
                            ReferenceEquals(Actual.Section, Section) &&
                            Actual.Imageˉoffset == Offset &&
                            Actual.Address == Address,
                        "A linked section placement violates canonical layout order or alignment.");
                    Require(
                        Address % Section.Alignment == 0,
                        "A linked section address does not satisfy its alignment.");
                    Result.Add((Input.Inputˉindex, Sourceˉindex), Actual);
                    Cursor = Offset + Section.Memoryˉsize;
                    Layoutˉindex++;
                }
            }
        }
        Require(Cursor == (ulong)candidate.Imageˉbytes.Length, "The linked image length is inconsistent.");
        Require(
            Cursor <= (uint)Linkˉlimits.Maximumˉimageˉbytes(candidate.Options.Admissionˉprofile),
            "The linked image exceeds its byte limit.");
        return Result;
    }

    private static Dictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition> Verifyˉdefinitions(
        Linkedˉimageˉcandidate candidate,
        IReadOnlyDictionary<(int Inputˉindex, int Sectionˉindex), Sectionˉplacement> placements)
    {
        var Result = new Dictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition>();
        var Definitionˉindex = 0;
        foreach (var Input in candidate.Inputs)
        {
            for (var Symbolˉindex = 0; Symbolˉindex < Input.Object.Value.Symbols.Length; Symbolˉindex++)
            {
                var Symbol = Input.Object.Value.Symbols[Symbolˉindex];
                if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
                {
                    continue;
                }
                var Placement = placements[(Input.Inputˉindex, (int)Symbol.Sectionˉindex)];
                var Address = (ulong)Placement.Address + Symbol.Offset;
                Require(Address <= uint.MaxValue, "A defined symbol address exceeds u32.");
                var Actual = candidate.Definitions[Definitionˉindex];
                Require(
                    Actual.Index == Definitionˉindex &&
                        Actual.Inputˉindex == Input.Inputˉindex &&
                        Actual.Sourceˉsymbolˉindex == Symbolˉindex &&
                        ReferenceEquals(Actual.Symbol, Symbol) &&
                        Actual.Address == Address,
                    "A linked symbol definition has an inconsistent address or identity.");
                Result.Add((Input.Inputˉindex, Symbolˉindex), Actual);
                Definitionˉindex++;
            }
        }
        Require(Definitionˉindex == candidate.Definitions.Length, "The linked definition count is inconsistent.");
        return Result;
    }

    private static Dictionary<string, Verifiedˉexport> Verifyˉexports(
        Linkedˉimageˉcandidate candidate,
        IReadOnlyDictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition> definitions)
    {
        var Result = new Dictionary<string, Verifiedˉexport>(StringComparer.Ordinal);
        foreach (var Input in candidate.Inputs)
        {
            for (var Symbolˉindex = 0; Symbolˉindex < Input.Object.Value.Symbols.Length; Symbolˉindex++)
            {
                var Symbol = Input.Object.Value.Symbols[Symbolˉindex];
                if (Symbol.Binding != Objectˉsymbolˉbinding.Export)
                {
                    continue;
                }
                Require(
                    Result.TryAdd(
                        Symbol.Name,
                        new(Input.Inputˉindex, Symbolˉindex, definitions[(Input.Inputˉindex, Symbolˉindex)])),
                    "The linked export set contains a duplicate name.");
            }
        }
        return Result;
    }

    private static void Verifyˉimports(
        Linkedˉimageˉcandidate candidate,
        IReadOnlyDictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition> definitions,
        IReadOnlyDictionary<string, Verifiedˉexport> exports)
    {
        var Importˉindex = 0;
        foreach (var Input in candidate.Inputs)
        {
            for (var Symbolˉindex = 0; Symbolˉindex < Input.Object.Value.Symbols.Length; Symbolˉindex++)
            {
                var Symbol = Input.Object.Value.Symbols[Symbolˉindex];
                if (Symbol.Binding != Objectˉsymbolˉbinding.Import)
                {
                    continue;
                }
                Require(exports.TryGetValue(Symbol.Name, out var Provider), "A linked import is unresolved.");
                Require(Provider.Definition.Symbol.Kind == Symbol.Kind, "A linked import has a kind mismatch.");
                Require(
                    definitions.ContainsKey((Provider.Inputˉindex, Provider.Symbolˉindex)),
                    "A linked import provider is not a definition.");
                var Actual = candidate.Imports[Importˉindex];
                Require(
                    Actual.Index == Importˉindex &&
                        Actual.Inputˉindex == Input.Inputˉindex &&
                        Actual.Sourceˉsymbolˉindex == Symbolˉindex &&
                        ReferenceEquals(Actual.Symbol, Symbol) &&
                        Actual.Providerˉinputˉindex == Provider.Inputˉindex &&
                        Actual.Providerˉsymbolˉindex == Provider.Symbolˉindex &&
                        Actual.Address == Provider.Definition.Address,
                    "A linked import resolution is inconsistent.");
                Importˉindex++;
            }
        }
        Require(Importˉindex == candidate.Imports.Length, "The linked import count is inconsistent.");
    }

    private static byte[] Copyˉsectionˉdata(
        Linkedˉimageˉcandidate candidate,
        IReadOnlyDictionary<(int Inputˉindex, int Sectionˉindex), Sectionˉplacement> placements)
    {
        var Result = new byte[candidate.Imageˉbytes.Length];
        foreach (var Input in candidate.Inputs)
        {
            for (var Sectionˉindex = 0; Sectionˉindex < Input.Object.Value.Sections.Length; Sectionˉindex++)
            {
                var Section = Input.Object.Value.Sections[Sectionˉindex];
                var Placement = placements[(Input.Inputˉindex, Sectionˉindex)];
                Section.Data.AsSpan().CopyTo(Result.AsSpan((int)Placement.Imageˉoffset));
            }
        }
        return Result;
    }

    private static void Verifyˉrelocations(
        Linkedˉimageˉcandidate candidate,
        IReadOnlyDictionary<(int Inputˉindex, int Sectionˉindex), Sectionˉplacement> placements,
        IReadOnlyDictionary<(int Inputˉindex, int Symbolˉindex), Symbolˉdefinition> definitions,
        IReadOnlyDictionary<string, Verifiedˉexport> exports,
        byte[] image)
    {
        var Appliedˉindex = 0;
        foreach (var Input in candidate.Inputs)
        {
            for (var Relocationˉindex = 0;
                Relocationˉindex < Input.Object.Value.Relocations.Length;
                Relocationˉindex++)
            {
                var Relocation = Input.Object.Value.Relocations[Relocationˉindex];
                var Placement = placements[(Input.Inputˉindex, (int)Relocation.Sectionˉindex)];
                var Symbol = Input.Object.Value.Symbols[(int)Relocation.Symbolˉindex];
                Symbolˉdefinition Target;
                if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
                {
                    Target = exports[Symbol.Name].Definition;
                }
                else
                {
                    Target = definitions[(Input.Inputˉindex, (int)Relocation.Symbolˉindex)];
                }
                var Patchˉoffset = Placement.Imageˉoffset + Relocation.Offset;
                var Patchˉaddress = Placement.Address + Relocation.Offset;
                long Value;
                if (Relocation.Kind == Objectˉrelocationˉkind.Absoluteˉu32)
                {
                    Value = (long)Target.Address + Relocation.Addend;
                    Require(Value is >= uint.MinValue and <= uint.MaxValue, "An absolute relocation exceeds u32.");
                    BinaryPrimitives.WriteUInt32LittleEndian(
                        image.AsSpan((int)Patchˉoffset, sizeof(uint)),
                        (uint)Value);
                }
                else
                {
                    Value = (long)Target.Address + Relocation.Addend - Patchˉaddress;
                    Require(Value is >= int.MinValue and <= int.MaxValue, "A relative relocation exceeds i32.");
                    BinaryPrimitives.WriteInt32LittleEndian(
                        image.AsSpan((int)Patchˉoffset, sizeof(int)),
                        (int)Value);
                }
                var Actual = candidate.Relocations[Appliedˉindex];
                Require(
                    Actual.Index == Appliedˉindex &&
                        Actual.Inputˉindex == Input.Inputˉindex &&
                        Actual.Sourceˉrelocationˉindex == Relocationˉindex &&
                        ReferenceEquals(Actual.Relocation, Relocation) &&
                        Actual.Patchˉimageˉoffset == Patchˉoffset &&
                        Actual.Patchˉaddress == Patchˉaddress &&
                        Actual.Targetˉname == Symbol.Name &&
                        Actual.Targetˉinputˉindex == Target.Inputˉindex &&
                        Actual.Targetˉsymbolˉindex == Target.Sourceˉsymbolˉindex &&
                        Actual.Targetˉaddress == Target.Address &&
                        Actual.Value == Value,
                    "An applied relocation has inconsistent evidence.");
                Appliedˉindex++;
            }
        }
        Require(Appliedˉindex == candidate.Relocations.Length, "The applied relocation count is inconsistent.");
    }

    private static void Require([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            throw new Linkˉfailure(new("WVL1011", -1, message));
        }
    }

    private sealed record Verifiedˉexport(
        int Inputˉindex,
        int Symbolˉindex,
        Symbolˉdefinition Definition);
}
