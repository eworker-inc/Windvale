namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvbˉunsafeˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Wvb-Unsafe-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  unknown-opcode\n" +
            "PASS  truncated-operand\n" +
            "PASS  local-index\n" +
            "PASS  jump-target\n" +
            "PASS  after-return\n" +
            "Tests: 5, Passed: 5, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
