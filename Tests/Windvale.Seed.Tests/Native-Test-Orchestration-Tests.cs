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
            "PASS  invalid-utf8\n" +
            "PASS  range-failure\n" +
            "PASS  u16-failure\n" +
            "PASS  malformed-bad-magic\n" +
            "PASS  malformed-bad-version\n" +
            "PASS  malformed-bad-utf8\n" +
            "PASS  malformed-truncated\n" +
            "PASS  malformed-trailing\n" +
            "Tests: 13, Passed: 13, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
