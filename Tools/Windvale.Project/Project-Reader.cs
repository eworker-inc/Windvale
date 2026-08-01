using System.Collections.Immutable;
using System.Text;

namespace Windvale.Project;

public static class Projectˉreader
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Projectˉreadˉresult Read(string manifestˉpath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestˉpath);

        var Manifestˉpath = Path.GetFullPath(manifestˉpath);
        var Buffer = new byte[Projectˉlimits.MAX_MANIFEST_BYTES + 1];
        var Length = 0;
        using (var Input = new FileStream(
            Manifestˉpath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read))
        {
            while (Length < Buffer.Length)
            {
                var Read = Input.Read(Buffer, Length, Buffer.Length - Length);
                if (Read == 0)
                {
                    break;
                }
                Length += Read;
            }
        }
        if (Length > Projectˉlimits.MAX_MANIFEST_BYTES)
        {
            return Invalid(
                "WVP1002",
                1,
                1,
                $"The project manifest exceeds the {Projectˉlimits.MAX_MANIFEST_BYTES} byte limit.");
        }

        string Text;
        try
        {
            Text = STRICT_UTF8.GetString(Buffer, 0, Length);
        }
        catch (DecoderFallbackException)
        {
            return Invalid("WVP1002", 1, 1, "The project manifest is not valid strict UTF-8 text.");
        }

        var Parsed = Projectˉparser.Parse(Text);
        if (!Parsed.Success)
        {
            return new(null, Parsed.Diagnostics);
        }

        var Manifest = Parsed.Manifest!;
        var Baseˉdirectory = Path.GetDirectoryName(Manifestˉpath)
            ?? throw new InvalidOperationException("The project manifest has no containing directory.");
        var Pathˉcomparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var Seen = new HashSet<string>(Pathˉcomparer);

        var Rootˉpath = Resolve(Baseˉdirectory, Manifest.Root.Value);
        Seen.Add(Rootˉpath);
        var Sourceˉpaths = ImmutableArray.CreateBuilder<string>(Manifest.Sources.Length);
        foreach (var Source in Manifest.Sources)
        {
            var Sourceˉpath = Resolve(Baseˉdirectory, Source.Value);
            if (!Seen.Add(Sourceˉpath))
            {
                return Invalid(
                    "WVP1007",
                    Source.Line,
                    Source.Column,
                    $"The project source path is supplied more than once: {Source.Value}");
            }

            Sourceˉpaths.Add(Sourceˉpath);
        }

        return new(
            new(
                Manifestˉpath,
                Rootˉpath,
                Sourceˉpaths.MoveToImmutable(),
                Manifest.Emission),
            []);
    }

    private static string Resolve(string baseˉdirectory, string relativeˉpath)
    {
        var Hostˉpath = relativeˉpath.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(baseˉdirectory, Hostˉpath));
    }

    private static Projectˉreadˉresult Invalid(
        string code,
        int line,
        int column,
        string message)
    {
        return new(null, [new(code, line, column, message)]);
    }
}
