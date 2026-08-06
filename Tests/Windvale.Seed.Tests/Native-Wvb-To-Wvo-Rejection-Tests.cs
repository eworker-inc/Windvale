namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvbˉtoˉwvoˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Lowerer-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  malformed\n" +
            "PASS  unsupported-function\n" +
            "Tests: 2, Passed: 2, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
