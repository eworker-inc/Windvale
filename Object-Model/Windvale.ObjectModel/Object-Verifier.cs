using System.Text;

namespace Windvale.ObjectModel;

public static class Objectˉverifier
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Verifiedˉobject Verify(Objectˉfile value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Architecture != Objectˉarchitecture.X86ˉ64)
        {
            Fail("WVO2001", "WVO 1.0 supports only the x86-64 architecture.");
        }
        if (value.Sections.IsDefault || value.Symbols.IsDefault || value.Relocations.IsDefault)
        {
            Fail("WVO2002", "Object collections must be initialized.");
        }
        if (value.Sections.Length is < 1 or > Objectˉlimits.MAX_SECTIONS)
        {
            Fail("WVO2003", $"An object requires 1 through {Objectˉlimits.MAX_SECTIONS} sections.");
        }
        if (value.Symbols.Length > Objectˉlimits.MAX_SYMBOLS)
        {
            Fail("WVO2004", "The object exceeds the symbol-count limit.");
        }
        if (value.Relocations.Length > Objectˉlimits.MAX_RELOCATIONS)
        {
            Fail("WVO2005", "The object exceeds the relocation-count limit.");
        }

        Verifyˉsections(value);
        Verifyˉsymbols(value);
        Verifyˉrelocations(value);
        return new(value);
    }

    public static bool Isˉmachineˉname(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int Byteˉcount;
        try
        {
            Byteˉcount = STRICT_UTF8.GetByteCount(value);
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
        if (Byteˉcount > Objectˉlimits.MAX_NAME_BYTES || Byteˉcount != value.Length)
        {
            return false;
        }

        for (var Index = 0; Index < value.Length; Index++)
        {
            var Character = value[Index];
            var Allowed = Character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '.' or '$' ||
                (Index > 0 && Character is >= '0' and <= '9');
            if (!Allowed)
            {
                return false;
            }
        }
        return true;
    }

    private static void Verifyˉsections(Objectˉfile value)
    {
        var Names = new HashSet<string>(StringComparer.Ordinal);
        ulong Totalˉdata = 0;
        ulong Totalˉmemory = 0;
        Objectˉsection? Previous = null;
        foreach (var Section in value.Sections)
        {
            if (Section is null)
            {
                throw new Objectˉverificationˉexception("WVO2010", "An object section is null.");
            }
            if (!Enum.IsDefined(Section.Kind))
            {
                Fail("WVO2010", "An object section has an invalid kind.");
            }
            if (!Isˉmachineˉname(Section.Name) || !Names.Add(Section.Name))
            {
                Fail("WVO2011", $"Section name '{Section.Name}' is invalid or duplicated.");
            }
            if (Previous is not null &&
                (Section.Kind < Previous.Kind ||
                    (Section.Kind == Previous.Kind &&
                        StringComparer.Ordinal.Compare(Previous.Name, Section.Name) >= 0)))
            {
                Fail("WVO2012", "Sections must be strictly ordered by kind and name.");
            }
            Previous = Section;
            if (Section.Alignment is 0 or > Objectˉlimits.MAX_ALIGNMENT ||
                (Section.Alignment & (Section.Alignment - 1)) != 0)
            {
                Fail("WVO2013", $"Section '{Section.Name}' has an invalid alignment.");
            }
            if (Section.Data.IsDefault)
            {
                Fail("WVO2014", $"Section '{Section.Name}' has uninitialized data.");
            }
            if (Section.Kind == Objectˉsectionˉkind.Zeroˉfill)
            {
                if (Section.Data.Length != 0 || Section.Memoryˉsize == 0)
                {
                    Fail("WVO2015", $"Zero-fill section '{Section.Name}' has invalid data or size.");
                }
            }
            else if (Section.Memoryˉsize != (uint)Section.Data.Length)
            {
                Fail("WVO2016", $"Materialized section '{Section.Name}' requires equal data and memory sizes.");
            }

            Totalˉdata += (uint)Section.Data.Length;
            Totalˉmemory += Section.Memoryˉsize;
            if (Totalˉdata > Objectˉlimits.MAX_OBJECT_BYTES || Totalˉmemory > Objectˉlimits.MAX_MEMORY_BYTES)
            {
                Fail("WVO2017", "The object exceeds its data or memory-size limit.");
            }
        }
    }

    private static void Verifyˉsymbols(Objectˉfile value)
    {
        var Names = new HashSet<string>(StringComparer.Ordinal);
        Objectˉsymbol? Previous = null;
        foreach (var Symbol in value.Symbols)
        {
            if (Symbol is null)
            {
                throw new Objectˉverificationˉexception("WVO2020", "An object symbol is null.");
            }
            if (!Enum.IsDefined(Symbol.Binding) || !Enum.IsDefined(Symbol.Kind))
            {
                Fail("WVO2020", "An object symbol has an invalid binding or kind.");
            }
            if (!Isˉmachineˉname(Symbol.Name) || !Names.Add(Symbol.Name))
            {
                Fail("WVO2021", $"Symbol name '{Symbol.Name}' is invalid or duplicated.");
            }
            if (Previous is not null &&
                (Symbol.Binding < Previous.Binding ||
                    (Symbol.Binding == Previous.Binding &&
                        StringComparer.Ordinal.Compare(Previous.Name, Symbol.Name) >= 0)))
            {
                Fail("WVO2022", "Symbols must be strictly ordered by binding and name.");
            }
            Previous = Symbol;

            if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
            {
                if (Symbol.Sectionˉindex != Objectˉlimits.UNDEFINED_SECTION || Symbol.Offset != 0 || Symbol.Size != 0)
                {
                    Fail("WVO2023", $"Imported symbol '{Symbol.Name}' must use the undefined section and zero range.");
                }
                continue;
            }
            if (Symbol.Sectionˉindex >= (uint)value.Sections.Length)
            {
                Fail("WVO2024", $"Defined symbol '{Symbol.Name}' references an invalid section.");
            }

            var Section = value.Sections[(int)Symbol.Sectionˉindex];
            if (Symbol.Offset > Section.Memoryˉsize || Symbol.Size > Section.Memoryˉsize - Symbol.Offset)
            {
                Fail("WVO2025", $"Defined symbol '{Symbol.Name}' is outside its section.");
            }
            if (Symbol.Kind == Objectˉsymbolˉkind.Function && Section.Kind != Objectˉsectionˉkind.Code)
            {
                Fail("WVO2026", $"Function symbol '{Symbol.Name}' is not in code.");
            }
            if (Symbol.Kind == Objectˉsymbolˉkind.Data && Section.Kind == Objectˉsectionˉkind.Code)
            {
                Fail("WVO2027", $"Data symbol '{Symbol.Name}' is in code.");
            }
        }
    }

    private static void Verifyˉrelocations(Objectˉfile value)
    {
        Objectˉrelocation? Previous = null;
        foreach (var Relocation in value.Relocations)
        {
            if (Relocation is null)
            {
                throw new Objectˉverificationˉexception("WVO2030", "An object relocation is null.");
            }
            if (!Enum.IsDefined(Relocation.Kind))
            {
                Fail("WVO2030", "An object relocation has an invalid kind.");
            }
            if (Relocation.Sectionˉindex >= (uint)value.Sections.Length)
            {
                Fail("WVO2031", "A relocation references an invalid section.");
            }
            if (Relocation.Symbolˉindex >= (uint)value.Symbols.Length)
            {
                Fail("WVO2032", "A relocation references an invalid symbol.");
            }
            if (Previous is not null)
            {
                if (Relocation.Sectionˉindex < Previous.Sectionˉindex ||
                    (Relocation.Sectionˉindex == Previous.Sectionˉindex &&
                        Relocation.Offset < Previous.Offset + sizeof(uint)))
                {
                    Fail("WVO2033", "Relocations must be ordered and their patch ranges cannot overlap.");
                }
            }
            Previous = Relocation;

            var Section = value.Sections[(int)Relocation.Sectionˉindex];
            if (Relocation.Offset > (uint)Section.Data.Length ||
                sizeof(uint) > (uint)Section.Data.Length - Relocation.Offset)
            {
                Fail("WVO2034", "A relocation patch range is outside materialized section data.");
            }
            var Patch = Section.Data.AsSpan((int)Relocation.Offset, sizeof(uint));
            if (!Patch.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
            {
                Fail("WVO2035", "Relocation placeholder bytes must be zero.");
            }
        }
    }

    private static void Fail(string code, string message)
    {
        throw new Objectˉverificationˉexception(code, message);
    }
}
