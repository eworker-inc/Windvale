namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉlinkerˉmapˉlimitˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Linker-Map-Limit");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  canonical-map-limit\n" +
            "Tests: 1, Passed: 1, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
