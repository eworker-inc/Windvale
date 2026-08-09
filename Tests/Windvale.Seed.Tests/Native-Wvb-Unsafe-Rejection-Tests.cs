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
            "PASS  record-parameter-type\n" +
            "PASS  record-field-index\n" +
            "PASS  duplicate-record-field\n" +
            "PASS  mismatched-enum-comparison\n" +
            "PASS  duplicate-nominal-name\n" +
            "PASS  mismatched-merge\n" +
            "PASS  bytes-length-on-i32\n" +
            "PASS  record-create-wrong-field-type\n" +
            "PASS  invalid-enum-member\n" +
            "PASS  enum-const-on-record\n" +
            "PASS  duplicate-enum-value\n" +
            "PASS  stack-capacity\n" +
            "PASS  record-field-on-primitive\n" +
            "PASS  enum-name-on-primitive\n" +
            "PASS  wrong-nominal-kind\n" +
            "Tests: 20, Passed: 20, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
