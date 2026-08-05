namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int WINDOWS_TEST_APPLICATION_DELETE_ATTEMPTS = 20;
    private const int WINDOWS_TEST_APPLICATION_DELETE_RETRY_MILLISECONDS = 50;

    private static void Deleteˉwindowsˉtestˉapplication(string path)
    {
        for (var Attempt = 1; Attempt <= WINDOWS_TEST_APPLICATION_DELETE_ATTEMPTS;
            Attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException) when (Attempt < WINDOWS_TEST_APPLICATION_DELETE_ATTEMPTS)
            {
                Thread.Sleep(WINDOWS_TEST_APPLICATION_DELETE_RETRY_MILLISECONDS);
            }
            catch (UnauthorizedAccessException) when (
                Attempt < WINDOWS_TEST_APPLICATION_DELETE_ATTEMPTS)
            {
                Thread.Sleep(WINDOWS_TEST_APPLICATION_DELETE_RETRY_MILLISECONDS);
            }
        }
    }
}
