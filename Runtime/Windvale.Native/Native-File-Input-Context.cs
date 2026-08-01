using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

public static class Nativeˉfileˉinputˉtableˉcontract
{
    public const uint MAGIC = 0x4946_5657;
    public const uint FORMAT_VERSION = 1;
    public const uint SIZE = 136;
    public const uint SNAPSHOT_RECORD_BYTES = 32;
    public const uint SNAPSHOT_CAPACITY = Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS;
    public const uint NAME_STRIDE_BYTES = Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES;
    public const uint DATA_STRIDE_BYTES = Bytecodeˉlimits.MAX_BYTE_DATA_BYTES;
    public const int MAGIC_OFFSET = 0;
    public const int FORMAT_VERSION_OFFSET = 4;
    public const int SIZE_OFFSET = 8;
    public const int PLATFORM_OFFSET = 12;
    public const int SNAPSHOT_TABLE_POINTER_OFFSET = 16;
    public const int SNAPSHOT_CAPACITY_OFFSET = 24;
    public const int SNAPSHOT_COUNT_OFFSET = 28;
    public const int NAME_ARENA_POINTER_OFFSET = 32;
    public const int NAME_STRIDE_OFFSET = 40;
    public const int NAME_RESERVED_OFFSET = 44;
    public const int DATA_ARENA_POINTER_OFFSET = 48;
    public const int DATA_STRIDE_OFFSET = 56;
    public const int MAXIMUM_DATA_BYTES_OFFSET = 60;
    public const int SCRATCH_POINTER_OFFSET = 64;
    public const int SCRATCH_BYTES_OFFSET = 72;
    public const int RESERVED_OFFSET = 76;
    public const int WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET = 80;
    public const int WINDOWS_OPEN_POINTER_OFFSET = 88;
    public const int WINDOWS_SIZE_POINTER_OFFSET = 96;
    public const int WINDOWS_READ_POINTER_OFFSET = 104;
    public const int WINDOWS_CLOSE_POINTER_OFFSET = 112;
    public const int WINDOWS_COMMIT_POINTER_OFFSET = 120;
    public const int WINDOWS_LAST_ERROR_POINTER_OFFSET = 128;

    public const int SNAPSHOT_NAME_POINTER_OFFSET = 0;
    public const int SNAPSHOT_NAME_LENGTH_OFFSET = 8;
    public const int SNAPSHOT_NAME_RESERVED_OFFSET = 12;
    public const int SNAPSHOT_DATA_POINTER_OFFSET = 16;
    public const int SNAPSHOT_DATA_LENGTH_OFFSET = 24;
    public const int SNAPSHOT_DATA_RESERVED_OFFSET = 28;
}

internal sealed class Nativeˉfileˉinputˉcontext : IDisposable
{
    private const uint MEM_COMMIT = 0x0000_1000;
    private const uint MEM_RESERVE = 0x0000_2000;
    private const uint MEM_RELEASE = 0x0000_8000;
    private const uint PAGE_READWRITE = 0x04;
    private const int PROT_READ = 0x1;
    private const int PROT_WRITE = 0x2;
    private const int MAP_PRIVATE = 0x2;
    private const int MAP_ANONYMOUS = 0x20;
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    private readonly IntPtr Windowsˉlibrary;
    private readonly nuint Nameˉarenaˉbytes;
    private readonly nuint Dataˉarenaˉbytes;
    private readonly nuint Scratchˉbytes;
    private bool Isˉdisposed;

    public Nativeˉfileˉinputˉcontext(Nativeˉhostˉservices? services, bool required)
    {
        if (!required)
        {
            return;
        }

        try
        {
            Platform = services!.Fileˉinput!.Platform;
            Nameˉarenaˉbytes = checked(
                (nuint)(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY *
                    Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES));
            Dataˉarenaˉbytes = checked(
                (nuint)(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY *
                    Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES));
            Scratchˉbytes = Platform switch
            {
                Nativeˉfileˉinputˉplatform.Windows => checked(
                    (nuint)((Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES + 1) * 2)),
                Nativeˉfileˉinputˉplatform.Linux => checked(
                    (nuint)(Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES + 1)),
                _ => throw new PlatformNotSupportedException(
                    "The native file-input table supports Windows and Linux."),
            };

            Nameˉarena = Allocateˉarena(Nameˉarenaˉbytes, commit: false);
            Dataˉarena = Allocateˉarena(Dataˉarenaˉbytes, commit: false);
            Scratch = Allocateˉarena(Scratchˉbytes, commit: true);

            var Snapshotˉbytes = checked(
                (int)(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY *
                    Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_RECORD_BYTES));
            Snapshotˉtable = Marshal.AllocHGlobal(Snapshotˉbytes);
            Marshal.Copy(new byte[Snapshotˉbytes], 0, Snapshotˉtable, Snapshotˉbytes);

            var Functions = new ulong[7];
            if (Platform == Nativeˉfileˉinputˉplatform.Windows)
            {
                Windowsˉlibrary = NativeLibrary.Load("kernel32.dll");
                Functions =
                [
                    Export("MultiByteToWideChar"),
                    Export("CreateFileW"),
                    Export("GetFileSizeEx"),
                    Export("ReadFile"),
                    Export("CloseHandle"),
                    Export("VirtualAlloc"),
                    Export("GetLastError"),
                ];
            }

            var Bytes = new byte[checked((int)Nativeˉfileˉinputˉtableˉcontract.SIZE)];
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.MAGIC_OFFSET),
                Nativeˉfileˉinputˉtableˉcontract.MAGIC);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION_OFFSET),
                Nativeˉfileˉinputˉtableˉcontract.FORMAT_VERSION);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SIZE_OFFSET),
                Nativeˉfileˉinputˉtableˉcontract.SIZE);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.PLATFORM_OFFSET),
                (uint)Platform);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_TABLE_POINTER_OFFSET),
                checked((ulong)Snapshotˉtable.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY_OFFSET),
                Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.NAME_ARENA_POINTER_OFFSET),
                checked((ulong)Nameˉarena.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET),
                Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.DATA_ARENA_POINTER_OFFSET),
                checked((ulong)Dataˉarena.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_OFFSET),
                Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.MAXIMUM_DATA_BYTES_OFFSET),
                Bytecodeˉlimits.MAX_BYTE_DATA_BYTES);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SCRATCH_POINTER_OFFSET),
                checked((ulong)Scratch.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Bytes.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SCRATCH_BYTES_OFFSET),
                checked((uint)Scratchˉbytes));
            for (var Index = 0; Index < Functions.Length; Index++)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(
                    Bytes.AsSpan(
                        Nativeˉfileˉinputˉtableˉcontract.WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET +
                        (Index * sizeof(ulong))),
                    Functions[Index]);
            }

            Initialˉtable = Bytes;
            Address = Marshal.AllocHGlobal(Bytes.Length);
            Marshal.Copy(Bytes, 0, Address, Bytes.Length);
            Verifyˉtable(completed: false);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public IntPtr Address { get; private set; }

    public Nativeˉfileˉinputˉplatform Platform { get; }

    private IntPtr Snapshotˉtable { get; set; }

    private IntPtr Nameˉarena { get; set; }

    private IntPtr Dataˉarena { get; set; }

    private IntPtr Scratch { get; set; }

    private byte[] Initialˉtable { get; set; } = [];

    public void Verifyˉcompleted()
    {
        if (Address != IntPtr.Zero)
        {
            Verifyˉtable(completed: true);
        }
    }

    public void Dispose()
    {
        if (Isˉdisposed)
        {
            return;
        }
        Isˉdisposed = true;
        if (Address != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(Address);
            Address = IntPtr.Zero;
        }
        if (Snapshotˉtable != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(Snapshotˉtable);
            Snapshotˉtable = IntPtr.Zero;
        }
        Releaseˉarena(Scratch, Scratchˉbytes);
        Scratch = IntPtr.Zero;
        Releaseˉarena(Dataˉarena, Dataˉarenaˉbytes);
        Dataˉarena = IntPtr.Zero;
        Releaseˉarena(Nameˉarena, Nameˉarenaˉbytes);
        Nameˉarena = IntPtr.Zero;
        if (Windowsˉlibrary != IntPtr.Zero)
        {
            NativeLibrary.Free(Windowsˉlibrary);
        }
    }

    private void Verifyˉtable(bool completed)
    {
        var Actual = new byte[Initialˉtable.Length];
        Marshal.Copy(Address, Actual, 0, Actual.Length);
        var Count = BinaryPrimitives.ReadUInt32LittleEndian(
            Actual.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET));
        var Staticˉactual = Actual.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(
            Staticˉactual.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET),
            0);
        if (!Staticˉactual.AsSpan().SequenceEqual(Initialˉtable) ||
            Count > Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY ||
            (!completed && Count != 0))
        {
            throw new InvalidOperationException(
                "The native file-input table violated its independently verified static layout.");
        }

        var Names = new HashSet<string>(StringComparer.Ordinal);
        for (var Index = 0u; Index < Count; Index++)
        {
            var Record = new byte[checked((int)
                Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_RECORD_BYTES)];
            Marshal.Copy(
                new IntPtr(checked(
                    Snapshotˉtable.ToInt64() +
                    ((long)Index * Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_RECORD_BYTES))),
                Record,
                0,
                Record.Length);
            var Nameˉaddress = BinaryPrimitives.ReadUInt64LittleEndian(
                Record.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_POINTER_OFFSET));
            var Nameˉlength = BinaryPrimitives.ReadUInt32LittleEndian(
                Record.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_LENGTH_OFFSET));
            var Dataˉaddress = BinaryPrimitives.ReadUInt64LittleEndian(
                Record.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_POINTER_OFFSET));
            var Dataˉlength = BinaryPrimitives.ReadUInt32LittleEndian(
                Record.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_LENGTH_OFFSET));
            var Expectedˉname = checked(
                (ulong)(Nameˉarena.ToInt64() +
                    ((long)Index * Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES)));
            var Expectedˉdata = checked(
                (ulong)(Dataˉarena.ToInt64() +
                    ((long)Index * Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES)));
            if (Nameˉaddress != Expectedˉname ||
                Nameˉlength is 0 or > Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_BYTES ||
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Record.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_RESERVED_OFFSET)) != 0 ||
                Dataˉaddress != Expectedˉdata ||
                Dataˉlength > Nativeˉfileˉinputˉtableˉcontract.DATA_STRIDE_BYTES ||
                BinaryPrimitives.ReadUInt32LittleEndian(
                    Record.AsSpan(Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_RESERVED_OFFSET)) != 0)
            {
                throw new InvalidOperationException(
                    $"Native file snapshot {Index} violated its bounded descriptor layout.");
            }

            var Nameˉbytes = new byte[checked((int)Nameˉlength)];
            Marshal.Copy(new IntPtr(checked((long)Nameˉaddress)), Nameˉbytes, 0, Nameˉbytes.Length);
            string Name;
            try
            {
                Name = STRICT_UTF8.GetString(Nameˉbytes);
            }
            catch (DecoderFallbackException Exception)
            {
                throw new InvalidOperationException(
                    $"Native file snapshot {Index} contains invalid UTF-8.",
                    Exception);
            }
            if (!Names.Add(Name))
            {
                throw new InvalidOperationException(
                    $"Native file snapshot {Index} duplicates an earlier ordinal resource name.");
            }
        }
    }

    private ulong Export(string name) =>
        checked((ulong)NativeLibrary.GetExport(Windowsˉlibrary, name).ToInt64());

    private IntPtr Allocateˉarena(nuint bytes, bool commit)
    {
        if (Platform == Nativeˉfileˉinputˉplatform.Windows)
        {
            var Address = VirtualAlloc(
                IntPtr.Zero,
                bytes,
                MEM_RESERVE | (commit ? MEM_COMMIT : 0),
                PAGE_READWRITE);
            return Address != IntPtr.Zero
                ? Address
                : throw new InvalidOperationException(
                    $"VirtualAlloc could not reserve the {bytes}-byte native file-input arena.");
        }
        var Mapping = Mmap(
            IntPtr.Zero,
            bytes,
            PROT_READ | PROT_WRITE,
            MAP_PRIVATE | MAP_ANONYMOUS,
            -1,
            0);
        return Mapping != new IntPtr(-1)
            ? Mapping
            : throw new InvalidOperationException(
                $"mmap could not reserve the {bytes}-byte native file-input arena.");
    }

    private void Releaseˉarena(IntPtr address, nuint bytes)
    {
        if (address == IntPtr.Zero)
        {
            return;
        }
        if (Platform == Nativeˉfileˉinputˉplatform.Windows)
        {
            if (!VirtualFree(address, 0, MEM_RELEASE))
            {
                throw new InvalidOperationException("VirtualFree could not release a native file-input arena.");
            }
            return;
        }
        if (Munmap(address, bytes) != 0)
        {
            throw new InvalidOperationException("munmap could not release a native file-input arena.");
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(
        IntPtr address,
        nuint size,
        uint allocationˉtype,
        uint protection);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFree(IntPtr address, nuint size, uint freeˉtype);

    [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
    private static extern IntPtr Mmap(
        IntPtr address,
        nuint length,
        int protection,
        int flags,
        int descriptor,
        nint offset);

    [DllImport("libc", EntryPoint = "munmap", SetLastError = true)]
    private static extern int Munmap(IntPtr address, nuint length);
}
