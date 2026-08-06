namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉconsoleˉpackagerˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(
            Repository,
            "Test-Console-Packager-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  entry-at-end\n" +
            "PASS  invalid-entry\n" +
            "PASS  empty-image\n" +
            "Tests: 3, Passed: 3, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
