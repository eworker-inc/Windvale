namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉwvoˉreadˉonlyˉrejectionsˉrun()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(
            Repository,
            "Test-Wvo-Read-Only-Rejections");
        Equal(0, Result.Exitˉcode);
        Equal(
            "PASS  short-header\n" +
            "PASS  bad-magic\n" +
            "PASS  bad-version\n" +
            "PASS  bad-architecture\n" +
            "PASS  unsupported-flags\n" +
            "PASS  limit-exceeded\n" +
            "PASS  out-of-bounds\n" +
            "PASS  invalid-name\n" +
            "PASS  invalid-section\n" +
            "PASS  invalid-symbol\n" +
            "PASS  invalid-relocation\n" +
            "PASS  noncanonical-order\n" +
            "PASS  trailing-bytes\n" +
            "Tests: 13, Passed: 13, Failed: 0\n",
            Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
