namespace Windvale.Tool;

internal static class Atomicˉfileˉpublisher
{
    internal static void Publish(
        string outputˉpath,
        ReadOnlySpan<byte> bytes,
        Action<string>? prepareˉtemporary = null)
    {
        var Outputˉpath = Path.GetFullPath(outputˉpath);
        var Directoryˉpath = Path.GetDirectoryName(Outputˉpath) ??
            throw new IOException("The output path has no containing directory.");
        var Temporaryˉpath = Path.Combine(
            Directoryˉpath,
            $".{Path.GetFileName(Outputˉpath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (var Stream = new FileStream(
                Temporaryˉpath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.WriteThrough))
            {
                Stream.Write(bytes);
                Stream.Flush(flushToDisk: true);
            }

            prepareˉtemporary?.Invoke(Temporaryˉpath);
            File.Move(Temporaryˉpath, Outputˉpath, overwrite: true);
        }
        catch
        {
            try
            {
                File.Delete(Temporaryˉpath);
            }
            catch (Exception Exception) when (
                Exception is IOException or UnauthorizedAccessException)
            {
            }
            throw;
        }
    }
}
