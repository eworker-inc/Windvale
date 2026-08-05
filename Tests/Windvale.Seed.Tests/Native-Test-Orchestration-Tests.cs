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
            "PASS  function-only\n" +
            "PASS  data-text\n" +
            "PASS  nominal-types\n" +
            "Tests: 5, Passed: 5, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
