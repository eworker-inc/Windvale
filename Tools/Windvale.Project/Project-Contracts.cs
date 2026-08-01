using System.Collections.Immutable;

namespace Windvale.Project;

public static class Projectˉlimits
{
    public const int MAX_MANIFEST_BYTES = 64 * 1024;
    public const int MAX_PATH_BYTES = 4 * 1024;
    public const int MAX_SOURCE_MODULES = 64;
}

public enum Projectˉemissionˉkind
{
    Wvb,
}

public sealed record Projectˉdiagnostic(
    string Code,
    int Line,
    int Column,
    string Message);

public sealed record Projectˉsourceˉpath(
    string Value,
    int Line,
    int Column);

public sealed record Projectˉmanifest(
    Projectˉsourceˉpath Root,
    ImmutableArray<Projectˉsourceˉpath> Sources,
    Projectˉemissionˉkind Emission);

public sealed record Projectˉparseˉresult(
    Projectˉmanifest? Manifest,
    ImmutableArray<Projectˉdiagnostic> Diagnostics)
{
    public bool Success => Manifest is not null && Diagnostics.IsEmpty;
}

public sealed record Projectˉbuildˉplan(
    string Manifestˉpath,
    string Rootˉpath,
    ImmutableArray<string> Sourceˉpaths,
    Projectˉemissionˉkind Emission);

public sealed record Projectˉreadˉresult(
    Projectˉbuildˉplan? Plan,
    ImmutableArray<Projectˉdiagnostic> Diagnostics)
{
    public bool Success => Plan is not null && Diagnostics.IsEmpty;
}
