namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvaˉassemblerˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Assembler-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  wva1001\n" +
            "PASS  wva1002\n" +
            "PASS  wva1003\n" +
            "PASS  wva1004\n" +
            "PASS  wva1005\n" +
            "PASS  wva1006\n" +
            "PASS  wva1007\n" +
            "PASS  wva1008\n" +
            "PASS  wva1009\n" +
            "PASS  wva1010\n" +
            "PASS  wva1011\n" +
            "Tests: 11, Passed: 11, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
