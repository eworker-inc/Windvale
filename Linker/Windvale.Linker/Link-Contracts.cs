using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Linker;

public sealed record Linkˉinput(ImmutableArray<byte> Objectˉbytes);

public sealed record Linkˉoptions(
    uint Baseˉaddress,
    string Entryˉsymbol);

public sealed record Linkˉdiagnostic(
    string Code,
    int Inputˉindex,
    string Message);

public sealed class Linkˉresult
{
    private Linkˉresult(
        ImmutableArray<byte> imageˉbytes,
        ImmutableArray<byte> mapˉbytes,
        uint entryˉaddress,
        int sectionˉcount,
        int definedˉsymbolˉcount,
        int importˉcount,
        int relocationˉcount,
        ImmutableArray<Linkˉdiagnostic> diagnostics)
    {
        Imageˉbytes = imageˉbytes;
        Mapˉbytes = mapˉbytes;
        Entryˉaddress = entryˉaddress;
        Sectionˉcount = sectionˉcount;
        Definedˉsymbolˉcount = definedˉsymbolˉcount;
        Importˉcount = importˉcount;
        Relocationˉcount = relocationˉcount;
        Diagnostics = diagnostics;
    }

    public bool Success => Diagnostics.IsEmpty;

    public ImmutableArray<byte> Imageˉbytes { get; }

    public ImmutableArray<byte> Mapˉbytes { get; }

    public uint Entryˉaddress { get; }

    public int Sectionˉcount { get; }

    public int Definedˉsymbolˉcount { get; }

    public int Importˉcount { get; }

    public int Relocationˉcount { get; }

    public ImmutableArray<Linkˉdiagnostic> Diagnostics { get; }

    internal static Linkˉresult Succeeded(Linkedˉimageˉcandidate candidate, ImmutableArray<byte> mapˉbytes) =>
        new(
            candidate.Imageˉbytes,
            mapˉbytes,
            candidate.Entryˉaddress,
            candidate.Sections.Length,
            candidate.Definitions.Length,
            candidate.Imports.Length,
            candidate.Relocations.Length,
            []);

    internal static Linkˉresult Failed(Linkˉdiagnostic diagnostic) =>
        new([], [], 0, 0, 0, 0, 0, [diagnostic]);
}

public static class Linkˉcontract
{
    public const int FORMAT_VERSION = 1;
    public const string TARGET_NAME = "flat-x86-64-v1";
    public const uint DEFAULT_BASE_ADDRESS = 1_048_576;
}

public static class Linkˉlimits
{
    public const int MAX_INPUT_OBJECTS = 64;
    public const int MAX_TOTAL_SECTIONS = 256;
    public const int MAX_TOTAL_SYMBOLS = 16_384;
    public const int MAX_TOTAL_RELOCATIONS = 65_536;
    public const int MAX_IMAGE_BYTES = 4 * 1024 * 1024;
    public const int MAX_MAP_BYTES = 1024 * 1024;
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
