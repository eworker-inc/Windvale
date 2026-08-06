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
            "PASS  malformed-object\n" +
            "PASS  aggregate-limit\n" +
            "PASS  duplicate-export\n" +
            "PASS  undefined-import\n" +
            "PASS  kind-mismatch\n" +
            "PASS  missing-entry\n" +
            "PASS  layout-overflow\n" +
            "PASS  absolute-overflow\n" +
            "PASS  relative-overflow\n" +
            "Tests: 10, Passed: 10, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
