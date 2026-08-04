using System.Collections.Immutable;
using System.Security;
using Windvale.Runtime;

namespace Windvale.Tool;

internal sealed class Nativeˉreadˉonlyˉdirectory : IReadˉonlyˉdirectory
{
    internal const int MAX_ENTRIES = 4 * 1024;
    internal const long MAX_SNAPSHOT_BYTES = 64L * 1024 * 1024;

    private readonly ImmutableDictionary<string, Snapshotˉentry> Entries;

    public Nativeˉreadˉonlyˉdirectory(string directoryˉpath)
    {
        if (string.IsNullOrEmpty(directoryˉpath))
        {
            throw new IOException("Read-only directory binding path is empty.");
        }
        var Fullˉpath = Resolveˉpath(directoryˉpath);
        var Snapshot = ImmutableDictionary.CreateBuilder<string, Snapshotˉentry>(
            StringComparer.Ordinal);
        long Totalˉbytes = 0;

        try
        {
            var Directory = new DirectoryInfo(Fullˉpath);
            if (!Directory.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"Read-only directory binding '{Fullˉpath}' was not found.");
            }

            foreach (var Item in Directory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
            {
                var Name = Item.Name;
                if (!Readˉonlyˉdirectoryˉcontract.Isˉnameˉvalid(Name))
                {
                    continue;
                }
                if (Snapshot.Count >= MAX_ENTRIES)
                {
                    throw new IOException(
                        $"Read-only directory binding exceeds the {MAX_ENTRIES}-entry snapshot limit.");
                }

                Item.Refresh();
                if ((Item.Attributes & (FileAttributes.Directory |
                        FileAttributes.ReparsePoint |
                        FileAttributes.Device)) != 0 ||
                    Item is not FileInfo File)
                {
                    Snapshot.Add(Name, Snapshotˉentry.Notˉfile);
                    continue;
                }

                using var Input = new FileStream(
                    File.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    64 * 1024,
                    FileOptions.SequentialScan);
                var Length = Input.Length;
                if (Length < 0 || Length > uint.MaxValue)
                {
                    throw new IOException(
                        $"Read-only directory entry '{Name}' exceeds the u32 file-length contract.");
                }
                if (Length > MAX_SNAPSHOT_BYTES - Totalˉbytes)
                {
                    throw new IOException(
                        $"Read-only directory binding exceeds the {MAX_SNAPSHOT_BYTES}-byte snapshot limit.");
                }

                var Bytes = new byte[checked((int)Length)];
                Input.ReadExactly(Bytes);
                Totalˉbytes = checked(Totalˉbytes + Length);
                Snapshot.Add(Name, new(true, ImmutableArray.Create(Bytes)));
            }
        }
        catch (Exception Exception) when (
            Exception is UnauthorizedAccessException or SecurityException)
        {
            throw new UnauthorizedAccessException(
                $"Read-only directory binding '{Fullˉpath}' was denied.",
                Exception);
        }

        Entries = Snapshot.ToImmutable();
    }

    public Readˉonlyˉdirectoryˉresult Readˉbytes(
        string name,
        uint offset,
        uint maximumˉbytes)
    {
        if (!Entries.TryGetValue(name, out var Entry))
        {
            return new(Readˉonlyˉdirectoryˉstatus.Notˉfound, 0, []);
        }
        if (!Entry.Isˉfile)
        {
            return new(Readˉonlyˉdirectoryˉstatus.Notˉfile, 0, []);
        }

        var Fileˉlength = checked((uint)Entry.Bytes.Length);
        if (offset > Fileˉlength)
        {
            return new(Readˉonlyˉdirectoryˉstatus.Invalidˉoffset, Fileˉlength, []);
        }

        var Length = Math.Min(maximumˉbytes, Fileˉlength - offset);
        return new(
            Readˉonlyˉdirectoryˉstatus.Valid,
            Fileˉlength,
            ImmutableArray.Create(Entry.Bytes.AsSpan((int)offset, (int)Length).ToArray()));
    }

    private static string Resolveˉpath(string directoryˉpath)
    {
        try
        {
            return Path.GetFullPath(directoryˉpath);
        }
        catch (Exception Exception) when (
            Exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new IOException(
                $"Read-only directory binding path '{directoryˉpath}' is invalid.",
                Exception);
        }
    }

    private sealed record Snapshotˉentry(bool Isˉfile, ImmutableArray<byte> Bytes)
    {
        public static Snapshotˉentry Notˉfile { get; } = new(false, []);
    }
}
