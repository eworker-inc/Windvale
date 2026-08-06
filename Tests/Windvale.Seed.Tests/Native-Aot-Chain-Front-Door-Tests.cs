namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉaotˉchainˉfrontˉdoorˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Result = Runˉnativeˉwvbˉtool(Repository, "Test-Aot-Chain");
        Equal(0, Result.Exitˉcode);
        Equal("native aot chain status=Passed result=42\n", Result.Output);
        Equal(string.Empty, Result.Error);
    }
}
