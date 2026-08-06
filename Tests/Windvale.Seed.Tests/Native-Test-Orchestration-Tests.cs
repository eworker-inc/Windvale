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
            "PASS  malformed-typed-operator-stack-kind\n" +
            "PASS  malformed-typed-local-store-kind\n" +
            "PASS  malformed-typed-call-argument-identity\n" +
            "PASS  malformed-typed-record-receiver-identity\n" +
            "PASS  malformed-typed-enum-operand-identity\n" +
            "PASS  malformed-typed-branch-condition-kind\n" +
            "PASS  malformed-typed-declared-maximum-stack\n" +
            "PASS  malformed-typed-capability-argument-kind\n" +
            "PASS  malformed-control-unreachable-instruction\n" +
            "PASS  wvo-return-42\n" +
            "PASS  wvo-bad-magic\n" +
            "PASS  wvo-truncated\n" +
            "PASS  wvo-trailing\n" +
            "Tests: 26, Passed: 26, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
