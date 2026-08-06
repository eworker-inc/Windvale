namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉpublisherˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Publisher-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  console-application\n" +
            "PASS  wvo\n" +
            "Tests: 2, Passed: 2, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
