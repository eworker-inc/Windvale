namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉtestˉorchestrationˉruns()
    {
        var Result = Runˉnativeˉwvbˉtool(
            Findˉrepositoryˉroot(),
            "Test-Seed");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  calls-control\n" +
            "PASS  scalar-core\n" +
            "Tests: 2, Passed: 2, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
