using System.Collections.Immutable;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static ImmutableArray<byte> Readˉembeddedˉnativeˉartifact(
        Type owner,
        string resource)
    {
        using var Stream = owner.Assembly.GetManifestResourceStream(resource) ??
            throw new InvalidOperationException(
                $"The retained native service artifact '{resource}' was not embedded.");
        var Result = new byte[checked((int)Stream.Length)];
        Stream.ReadExactly(Result);
        return Result.ToImmutableArray();
    }
}
