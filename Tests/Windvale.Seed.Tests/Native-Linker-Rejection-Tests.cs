namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉlinkerˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Linker-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  invalid-base\n" +
            "PASS  missing-entry\n" +
            "PASS  malformed-object\n" +
            "Tests: 3, Passed: 3, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
