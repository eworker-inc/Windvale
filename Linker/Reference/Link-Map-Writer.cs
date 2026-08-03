using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Windvale.ObjectModel;

namespace Windvale.Linker;

internal static class Linkˉmapˉwriter
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static ImmutableArray<byte> Write(Linkedˉimageˉcandidate candidate)
    {
        var Output = new StringBuilder();
        Add(Output, $"windvale-link-map {Linkˉcontract.FORMAT_VERSION}");
        Add(
            Output,
            $"target name={Targetˉname(candidate.Options.Admissionˉprofile)} architecture=x86-64 base-address={candidate.Options.Baseˉaddress} image-bytes={candidate.Imageˉbytes.Length}");
        Add(Output, $"entry name={candidate.Options.Entryˉsymbol} address={candidate.Entryˉaddress}");
        Add(Output, $"image sha256={Objectˉdigest.Calculateˉsha256(candidate.Imageˉbytes.AsSpan())}");
        Add(Output, $"inputs count={candidate.Inputs.Length}");
        foreach (var Input in candidate.Inputs)
        {
            Add(Output, $"input index={Input.Inputˉindex} sha256={Input.Sha256}");
        }

        Add(Output, $"sections count={candidate.Sections.Length}");
        foreach (var Placement in candidate.Sections)
        {
            Add(
                Output,
                $"section index={Placement.Index} input={Placement.Inputˉindex} source-index={Placement.Sourceˉsectionˉindex} kind={Sectionˉkind(Placement.Section.Kind)} name={Placement.Section.Name} image-offset={Placement.Imageˉoffset} address={Placement.Address} memory-bytes={Placement.Section.Memoryˉsize} data-bytes={Placement.Section.Data.Length} alignment={Placement.Section.Alignment}");
        }

        Add(Output, $"defined-symbols count={candidate.Definitions.Length}");
        foreach (var Definition in candidate.Definitions)
        {
            Add(
                Output,
                $"symbol index={Definition.Index} input={Definition.Inputˉindex} source-index={Definition.Sourceˉsymbolˉindex} binding={Binding(Definition.Symbol.Binding)} kind={Symbolˉkind(Definition.Symbol.Kind)} name={Definition.Symbol.Name} address={Definition.Address} size={Definition.Symbol.Size}");
        }

        Add(Output, $"imports count={candidate.Imports.Length}");
        foreach (var Import in candidate.Imports)
        {
            Add(
                Output,
                $"import index={Import.Index} input={Import.Inputˉindex} source-index={Import.Sourceˉsymbolˉindex} kind={Symbolˉkind(Import.Symbol.Kind)} name={Import.Symbol.Name} provider-input={Import.Providerˉinputˉindex} provider-source-index={Import.Providerˉsymbolˉindex} address={Import.Address}");
        }

        Add(Output, $"relocations count={candidate.Relocations.Length}");
        foreach (var Relocation in candidate.Relocations)
        {
            Add(
                Output,
                $"relocation index={Relocation.Index} input={Relocation.Inputˉindex} source-index={Relocation.Sourceˉrelocationˉindex} kind={Relocationˉkind(Relocation.Relocation.Kind)} patch-offset={Relocation.Patchˉimageˉoffset} patch-address={Relocation.Patchˉaddress} target={Relocation.Targetˉname} target-input={Relocation.Targetˉinputˉindex} target-source-index={Relocation.Targetˉsymbolˉindex} target-address={Relocation.Targetˉaddress} addend={Relocation.Relocation.Addend} value={Relocation.Value}");
        }

        var Bytes = STRICT_UTF8.GetBytes(Output.ToString());
        if (Bytes.Length > Linkˉlimits.MAX_MAP_BYTES)
        {
            throw new Linkˉfailure(new(
                "WVL1012",
                -1,
                $"The canonical link map exceeds {Linkˉlimits.MAX_MAP_BYTES} bytes."));
        }
        return Bytes.ToImmutableArray();
    }

    private static void Add(StringBuilder output, FormattableString line)
    {
        output.Append(line.ToString(CultureInfo.InvariantCulture));
        output.Append('\n');
    }

    private static string Targetˉname(Linkˉadmissionˉprofile profile) => profile switch
    {
        Linkˉadmissionˉprofile.Standard => Linkˉcontract.TARGET_NAME,
        Linkˉadmissionˉprofile.Largeˉnative => Linkˉcontract.LARGE_NATIVE_TARGET_NAME,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };

    private static string Sectionˉkind(Objectˉsectionˉkind kind) => kind switch
    {
        Objectˉsectionˉkind.Code => "code",
        Objectˉsectionˉkind.Readˉonlyˉdata => "read-only-data",
        Objectˉsectionˉkind.Writableˉdata => "writable-data",
        Objectˉsectionˉkind.Zeroˉfill => "zero-fill",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    private static string Binding(Objectˉsymbolˉbinding binding) => binding switch
    {
        Objectˉsymbolˉbinding.Local => "local",
        Objectˉsymbolˉbinding.Export => "export",
        Objectˉsymbolˉbinding.Import => "import",
        _ => throw new ArgumentOutOfRangeException(nameof(binding), binding, null),
    };

    private static string Symbolˉkind(Objectˉsymbolˉkind kind) => kind switch
    {
        Objectˉsymbolˉkind.Function => "function",
        Objectˉsymbolˉkind.Data => "data",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    private static string Relocationˉkind(Objectˉrelocationˉkind kind) => kind switch
    {
        Objectˉrelocationˉkind.Absoluteˉu32 => "absolute-u32",
        Objectˉrelocationˉkind.Relativeˉi32 => "relative-i32",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}
