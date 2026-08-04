using System.Collections.Immutable;
using System.Security;
using Windvale.Runtime;

namespace Windvale.Tool;

internal sealed class Nativeˉrandomˉaccessˉstorage : IRandomˉaccessˉstorage, IDisposable
{
    private const ulong GENERATION = 1;

    private readonly object Gate = new();
    private readonly FileStream Stream;
    private bool Isˉdisposed;

    public Nativeˉrandomˉaccessˉstorage(string storageˉpath)
    {
        if (string.IsNullOrEmpty(storageˉpath))
        {
            throw new IOException("Random-access storage binding path is empty.");
        }
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException(
                "The Stage 0 random-access storage adapter supports Windows and Linux.");
        }

        var Fullˉpath = Resolveˉpath(storageˉpath);
        try
        {
            var File = new FileInfo(Fullˉpath);
            File.Refresh();
            if (!File.Exists)
            {
                throw new FileNotFoundException(
                    $"Random-access storage binding '{Fullˉpath}' was not found.",
                    Fullˉpath);
            }
            if ((File.Attributes & (FileAttributes.Directory |
                    FileAttributes.ReparsePoint |
                    FileAttributes.Device)) != 0)
            {
                throw new IOException(
                    $"Random-access storage binding '{Fullˉpath}' is not an ordinary file.");
            }

            Stream = new FileStream(
                Fullˉpath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.Read,
                1,
                FileOptions.RandomAccess);
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    Stream.Lock(0, long.MaxValue);
                }
            }
            catch
            {
                Stream.Dispose();
                throw;
            }
        }
        catch (Exception Exception) when (
            Exception is UnauthorizedAccessException or SecurityException)
        {
            throw new UnauthorizedAccessException(
                $"Random-access storage binding '{Fullˉpath}' was denied.",
                Exception);
        }
    }

    public Randomˉaccessˉstorageˉresult Describe()
    {
        lock (Gate)
        {
            if (Isˉdisposed)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Revoked, 0, 0);
            }
            try
            {
                return Success(
                    GENERATION,
                    checked((ulong)Stream.Length),
                    0,
                    0,
                    Randomˉaccessˉstorageˉcompletion.None,
                    []);
            }
            catch (Exception Exception) when (Exception is IOException or NotSupportedException)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Unavailable, 0, 0);
            }
        }
    }

    public Randomˉaccessˉstorageˉresult Readˉat(
        ulong generation,
        ulong position,
        uint maximumˉbytes)
    {
        lock (Gate)
        {
            if (Isˉdisposed)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Revoked, generation, position);
            }
            if (generation != GENERATION)
            {
                return Stale(position);
            }

            try
            {
                var Length = checked((ulong)Stream.Length);
                if (position > Length)
                {
                    return new(
                        Randomˉaccessˉstorageˉstatus.Outsideˉstorage,
                        GENERATION,
                        Length,
                        position,
                        0,
                        Randomˉaccessˉstorageˉcompletion.None,
                        []);
                }

                var Count = checked((int)Math.Min((ulong)maximumˉbytes, Length - position));
                var Bytes = new byte[Count];
                var Read = 0;
                while (Read < Count)
                {
                    var Currentˉposition = checked(position + (ulong)Read);
                    if (Currentˉposition > long.MaxValue)
                    {
                        return Failure(
                            Randomˉaccessˉstorageˉstatus.Unsupported,
                            generation,
                            position);
                    }
                    var Current = RandomAccess.Read(
                        Stream.SafeFileHandle,
                        Bytes.AsSpan(Read),
                        checked((long)Currentˉposition));
                    if (Current == 0)
                    {
                        return Failure(
                            Randomˉaccessˉstorageˉstatus.Unavailable,
                            generation,
                            position);
                    }
                    Read = checked(Read + Current);
                }

                return Success(
                    GENERATION,
                    Length,
                    position,
                    checked((uint)Count),
                    Randomˉaccessˉstorageˉcompletion.None,
                    ImmutableArray.Create(Bytes));
            }
            catch (UnauthorizedAccessException)
            {
                return Failure(
                    Randomˉaccessˉstorageˉstatus.Permissionˉdenied,
                    generation,
                    position);
            }
            catch (Exception Exception) when (Exception is IOException or NotSupportedException)
            {
                return Failure(
                    Randomˉaccessˉstorageˉstatus.Unavailable,
                    generation,
                    position);
            }
        }
    }

    public Randomˉaccessˉstorageˉresult Writeˉat(
        ulong generation,
        ulong position,
        ImmutableArray<byte> bytes)
    {
        lock (Gate)
        {
            if (Isˉdisposed)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Revoked, generation, position);
            }
            if (generation != GENERATION)
            {
                return Stale(position);
            }
            if (position > long.MaxValue ||
                position + checked((ulong)bytes.Length) > long.MaxValue)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Unsupported, generation, position);
            }

            try
            {
                RandomAccess.Write(
                    Stream.SafeFileHandle,
                    bytes.AsSpan(),
                    checked((long)position));
                return Success(
                    GENERATION,
                    checked((ulong)Stream.Length),
                    position,
                    checked((uint)bytes.Length),
                    Randomˉaccessˉstorageˉcompletion.Completed,
                    []);
            }
            catch (Exception Exception) when (
                Exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return Indeterminate(generation, position);
            }
        }
    }

    public Randomˉaccessˉstorageˉresult Resize(ulong generation, ulong length)
    {
        lock (Gate)
        {
            if (Isˉdisposed)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Revoked, generation, length);
            }
            if (generation != GENERATION)
            {
                return Stale(length);
            }
            if (length > long.MaxValue)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Unsupported, generation, length);
            }

            try
            {
                Stream.SetLength(checked((long)length));
                return Success(
                    GENERATION,
                    length,
                    length,
                    0,
                    Randomˉaccessˉstorageˉcompletion.Completed,
                    []);
            }
            catch (Exception Exception) when (
                Exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return Indeterminate(generation, length);
            }
        }
    }

    public Randomˉaccessˉstorageˉresult Flush(
        ulong generation,
        Randomˉaccessˉstorageˉflush flush)
    {
        lock (Gate)
        {
            if (Isˉdisposed)
            {
                return Failure(Randomˉaccessˉstorageˉstatus.Revoked, generation, 0);
            }
            if (generation != GENERATION)
            {
                return Stale(0);
            }

            try
            {
                Stream.Flush(flushToDisk: true);
                return Success(
                    GENERATION,
                    checked((ulong)Stream.Length),
                    0,
                    0,
                    Randomˉaccessˉstorageˉcompletion.Completed,
                    []);
            }
            catch (Exception Exception) when (
                Exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return Indeterminate(generation, 0);
            }
        }
    }

    public void Dispose()
    {
        lock (Gate)
        {
            if (Isˉdisposed)
            {
                return;
            }
            Isˉdisposed = true;
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    Stream.Unlock(0, long.MaxValue);
                }
            }
            catch (Exception Exception) when (Exception is IOException or ObjectDisposedException)
            {
                // Closing the handle still terminates the launcher-owned writer lease.
            }
            Stream.Dispose();
        }
    }

    private static Randomˉaccessˉstorageˉresult Success(
        ulong generation,
        ulong storageˉlength,
        ulong position,
        uint progress,
        Randomˉaccessˉstorageˉcompletion completion,
        ImmutableArray<byte> bytes) =>
        new(
            Randomˉaccessˉstorageˉstatus.Valid,
            generation,
            storageˉlength,
            position,
            progress,
            completion,
            bytes);

    private static Randomˉaccessˉstorageˉresult Failure(
        Randomˉaccessˉstorageˉstatus status,
        ulong generation,
        ulong position) =>
        new(
            status,
            generation,
            0,
            position,
            0,
            Randomˉaccessˉstorageˉcompletion.None,
            []);

    private static Randomˉaccessˉstorageˉresult Stale(ulong position) =>
        Failure(Randomˉaccessˉstorageˉstatus.Stale, GENERATION, position);

    private static Randomˉaccessˉstorageˉresult Indeterminate(
        ulong generation,
        ulong position) =>
        new(
            Randomˉaccessˉstorageˉstatus.Valid,
            generation,
            0,
            position,
            0,
            Randomˉaccessˉstorageˉcompletion.Indeterminate,
            []);

    private static string Resolveˉpath(string storageˉpath)
    {
        try
        {
            return Path.GetFullPath(storageˉpath);
        }
        catch (Exception Exception) when (
            Exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new IOException(
                $"Random-access storage binding path '{storageˉpath}' is invalid.",
                Exception);
        }
    }
}
