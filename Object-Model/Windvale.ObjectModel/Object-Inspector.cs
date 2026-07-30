using System.Security.Cryptography;
using System.Text;

namespace Windvale.ObjectModel;

public static class Objectˉdigest
{
    public static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public static class Objectˉinspector
{
    public static string Inspect(Verifiedˉobject value, ReadOnlySpan<byte> bytes)
    {
        ArgumentNullException.ThrowIfNull(value);
        var Output = new StringBuilder();
        Output.AppendLine($"Windvale object {Objectˉcodec.MAJOR_VERSION}.{Objectˉcodec.MINOR_VERSION}");
        Output.AppendLine($"Architecture: {value.Value.Architecture}");
        Output.AppendLine($"SHA-256: {Objectˉdigest.Calculateˉsha256(bytes)}");
        Output.AppendLine($"Sections ({value.Value.Sections.Length})");
        for (var Index = 0; Index < value.Value.Sections.Length; Index++)
        {
            var Section = value.Value.Sections[Index];
            Output.AppendLine(
                $"  [{Index}] {Section.Name} kind={Section.Kind} align={Section.Alignment} " +
                $"memory={Section.Memoryˉsize} data={Section.Data.Length}");
        }
        Output.AppendLine($"Symbols ({value.Value.Symbols.Length})");
        for (var Index = 0; Index < value.Value.Symbols.Length; Index++)
        {
            var Symbol = value.Value.Symbols[Index];
            var Section = Symbol.Sectionˉindex == Objectˉlimits.UNDEFINED_SECTION
                ? "undefined"
                : Symbol.Sectionˉindex.ToString();
            Output.AppendLine(
                $"  [{Index}] {Symbol.Name} binding={Symbol.Binding} kind={Symbol.Kind} " +
                $"section={Section} offset={Symbol.Offset} size={Symbol.Size}");
        }
        Output.AppendLine($"Relocations ({value.Value.Relocations.Length})");
        for (var Index = 0; Index < value.Value.Relocations.Length; Index++)
        {
            var Relocation = value.Value.Relocations[Index];
            Output.AppendLine(
                $"  [{Index}] kind={Relocation.Kind} section={Relocation.Sectionˉindex} " +
                $"offset={Relocation.Offset} symbol={Relocation.Symbolˉindex} addend={Relocation.Addend}");
        }
        return Output.ToString();
    }
}
