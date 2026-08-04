using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Linker;

public sealed record Linkˉinput(ImmutableArray<byte> Objectˉbytes);

public enum Linkˉadmissionˉprofile : byte
{
    Standard = 1,
    Largeˉnative = 2,
}

public sealed record Linkˉoptions(
    uint Baseˉaddress,
    string Entryˉsymbol,
    Linkˉadmissionˉprofile Admissionˉprofile = Linkˉadmissionˉprofile.Standard);

public sealed record Linkˉdiagnostic(
    string Code,
    int Inputˉindex,
    string Message);

public sealed class Linkˉresult
{
    private Linkˉresult(
        ImmutableArray<byte> imageˉbytes,
        ImmutableArray<byte> mapˉbytes,
        uint baseˉaddress,
        uint entryˉaddress,
        int sectionˉcount,
        int codeˉsectionˉcount,
        int readˉonlyˉsectionˉcount,
        int definedˉsymbolˉcount,
        int importˉcount,
        int relocationˉcount,
        int absoluteˉrelocationˉcount,
        int relativeˉrelocationˉcount,
        ImmutableArray<Linkˉdiagnostic> diagnostics)
    {
        Imageˉbytes = imageˉbytes;
        Mapˉbytes = mapˉbytes;
        Baseˉaddress = baseˉaddress;
        Entryˉaddress = entryˉaddress;
        Sectionˉcount = sectionˉcount;
        Codeˉsectionˉcount = codeˉsectionˉcount;
        Readˉonlyˉsectionˉcount = readˉonlyˉsectionˉcount;
        Definedˉsymbolˉcount = definedˉsymbolˉcount;
        Importˉcount = importˉcount;
        Relocationˉcount = relocationˉcount;
        Absoluteˉrelocationˉcount = absoluteˉrelocationˉcount;
        Relativeˉrelocationˉcount = relativeˉrelocationˉcount;
        Diagnostics = diagnostics;
    }

    public bool Success => Diagnostics.IsEmpty;

    public ImmutableArray<byte> Imageˉbytes { get; }

    public ImmutableArray<byte> Mapˉbytes { get; }

    public uint Baseˉaddress { get; }

    public uint Entryˉaddress { get; }

    public int Sectionˉcount { get; }

    public int Codeˉsectionˉcount { get; }

    public int Readˉonlyˉsectionˉcount { get; }

    public int Definedˉsymbolˉcount { get; }

    public int Importˉcount { get; }

    public int Relocationˉcount { get; }

    public int Absoluteˉrelocationˉcount { get; }

    public int Relativeˉrelocationˉcount { get; }

    public ImmutableArray<Linkˉdiagnostic> Diagnostics { get; }

    internal static Linkˉresult Succeeded(Linkedˉimageˉcandidate candidate, ImmutableArray<byte> mapˉbytes) =>
        new(
            candidate.Imageˉbytes,
            mapˉbytes,
            candidate.Options.Baseˉaddress,
            candidate.Entryˉaddress,
            candidate.Sections.Length,
            candidate.Sections.Count(Section => Section.Section.Kind == Objectˉsectionˉkind.Code),
            candidate.Sections.Count(
                Section => Section.Section.Kind == Objectˉsectionˉkind.Readˉonlyˉdata),
            candidate.Definitions.Length,
            candidate.Imports.Length,
            candidate.Relocations.Length,
            candidate.Relocations.Count(
                Relocation => Relocation.Relocation.Kind == Objectˉrelocationˉkind.Absoluteˉu32),
            candidate.Relocations.Count(
                Relocation => Relocation.Relocation.Kind == Objectˉrelocationˉkind.Relativeˉi32),
            []);

    internal static Linkˉresult Failed(Linkˉdiagnostic diagnostic) =>
        new([], [], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, [diagnostic]);
}

public static class Linkˉcontract
{
    public const int FORMAT_VERSION = 1;
    public const string TARGET_NAME = "flat-x86-64-v1";
    public const string LARGE_NATIVE_TARGET_NAME = "flat-x86-64-large-v1";
    public const uint DEFAULT_BASE_ADDRESS = 1_048_576;
}

public static class Linkˉlimits
{
    public const int MAX_INPUT_OBJECTS = 64;
    public const int MAX_TOTAL_SECTIONS = 256;
    public const int MAX_TOTAL_SYMBOLS = 16_384;
    public const int MAX_TOTAL_RELOCATIONS = 65_536;
    public const int MAX_IMAGE_BYTES = 4 * 1024 * 1024;
    public const int LARGE_NATIVE_MAX_IMAGE_BYTES = 32 * 1024 * 1024;
    public const int MAX_TOTAL_INPUT_BYTES = MAX_INPUT_OBJECTS * Objectˉlimits.MAX_OBJECT_BYTES;
    public const int LARGE_NATIVE_MAX_TOTAL_INPUT_BYTES = 32 * 1024 * 1024;
    public const int MAX_MAP_BYTES = 1024 * 1024;

    public static int Maximumˉimageˉbytes(Linkˉadmissionˉprofile profile) => profile switch
    {
        Linkˉadmissionˉprofile.Standard => MAX_IMAGE_BYTES,
        Linkˉadmissionˉprofile.Largeˉnative => LARGE_NATIVE_MAX_IMAGE_BYTES,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };

    public static int Maximumˉtotalˉinputˉbytes(Linkˉadmissionˉprofile profile) => profile switch
    {
        Linkˉadmissionˉprofile.Standard => MAX_TOTAL_INPUT_BYTES,
        Linkˉadmissionˉprofile.Largeˉnative => LARGE_NATIVE_MAX_TOTAL_INPUT_BYTES,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };
}

internal sealed record Loadedˉobject(
    int Inputˉindex,
    ImmutableArray<byte> Objectˉbytes,
    Verifiedˉobject Object,
    string Sha256);

internal sealed record Sectionˉplacement(
    int Index,
    int Inputˉindex,
    int Sourceˉsectionˉindex,
    Objectˉsection Section,
    uint Imageˉoffset,
    uint Address);

internal sealed record Symbolˉdefinition(
    int Index,
    int Inputˉindex,
    int Sourceˉsymbolˉindex,
    Objectˉsymbol Symbol,
    uint Address);

internal sealed record Importˉresolution(
    int Index,
    int Inputˉindex,
    int Sourceˉsymbolˉindex,
    Objectˉsymbol Symbol,
    int Providerˉinputˉindex,
    int Providerˉsymbolˉindex,
    uint Address);

internal sealed record Appliedˉrelocation(
    int Index,
    int Inputˉindex,
    int Sourceˉrelocationˉindex,
    Objectˉrelocation Relocation,
    uint Patchˉimageˉoffset,
    uint Patchˉaddress,
    string Targetˉname,
    int Targetˉinputˉindex,
    int Targetˉsymbolˉindex,
    uint Targetˉaddress,
    long Value);

internal sealed record Linkedˉimageˉcandidate(
    Linkˉoptions Options,
    ImmutableArray<Loadedˉobject> Inputs,
    ImmutableArray<Sectionˉplacement> Sections,
    ImmutableArray<Symbolˉdefinition> Definitions,
    ImmutableArray<Importˉresolution> Imports,
    ImmutableArray<Appliedˉrelocation> Relocations,
    ImmutableArray<byte> Imageˉbytes,
    uint Entryˉaddress);

internal sealed class Linkˉfailure(Linkˉdiagnostic diagnostic) : Exception(diagnostic.Message)
{
    public Linkˉdiagnostic Diagnostic { get; } = diagnostic;
}
