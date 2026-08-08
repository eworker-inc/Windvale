using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Windvale.Bytecode;

namespace Windvale.Runtime.Native;

public static class Nativeˉfileˉoutputˉtableˉcontract
{
    public const uint MAGIC = 0x4F46_5657;
    public const uint FORMAT_VERSION = 1;
    public const uint SIZE = 80;
    public const int MAGIC_OFFSET = 0;
    public const int FORMAT_VERSION_OFFSET = 4;
    public const int SIZE_OFFSET = 8;
    public const int PLATFORM_OFFSET = 12;
    public const int SCRATCH_POINTER_OFFSET = 16;
    public const int SCRATCH_BYTES_OFFSET = 24;
    public const int RESERVED_OFFSET = 28;
    public const int WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET = 32;
    public const int WINDOWS_OPEN_POINTER_OFFSET = 40;
    public const int WINDOWS_WRITE_POINTER_OFFSET = 48;
    public const int WINDOWS_FLUSH_POINTER_OFFSET = 56;
    public const int WINDOWS_CLOSE_POINTER_OFFSET = 64;
    public const int WINDOWS_LAST_ERROR_POINTER_OFFSET = 72;
}

internal sealed class Nativeˉfileˉoutputˉcontext : IDisposable
{
    private readonly IntPtr Windowsˉlibrary;
    private bool Isˉdisposed;

    public Nativeˉfileˉoutputˉcontext(Nativeˉhostˉservices? services, bool required)
    {
        if (!required)
        {
            return;
        }

        try
        {
            Platform = services!.Fileˉoutput!.Platform;
            Scratchˉbytes = Platform switch
            {
                Nativeˉfileˉinputˉplatform.Windows => checked(
                    (int)((Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES + 1) * 2)),
                Nativeˉfileˉinputˉplatform.Linux => checked(
                    (int)(Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES + 1)),
                _ => throw new PlatformNotSupportedException(
                    "The native file-output table supports Windows and Linux."),
            };
            Scratch = Marshal.AllocHGlobal(Scratchˉbytes);

            var Functions = new ulong[6];
            if (Platform == Nativeˉfileˉinputˉplatform.Windows)
            {
                Windowsˉlibrary = NativeLibrary.Load("kernel32.dll");
                Functions =
                [
                    Export("MultiByteToWideChar"),
                    Export("CreateFileW"),
                    Export("WriteFile"),
                    Export("FlushFileBuffers"),
                    Export("CloseHandle"),
                    Export("GetLastError"),
                ];
            }

            var Bytes = Nativeˉfileˉoutputˉtableˉbuilder.Build(
                Platform,
                checked((ulong)Scratch.ToInt64()),
                checked((uint)Scratchˉbytes),
                Functions.ToImmutableArray());

            Initialˉtable = Bytes.ToArray();
            Address = Marshal.AllocHGlobal(Bytes.Length);
            Marshal.Copy(Initialˉtable, 0, Address, Bytes.Length);
            Verify();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public IntPtr Address { get; private set; }

    public Nativeˉfileˉinputˉplatform Platform { get; }

    private IntPtr Scratch { get; set; }

    private int Scratchˉbytes { get; set; }

    private byte[] Initialˉtable { get; set; } = [];

    public void Verifyˉcompleted()
    {
        if (Address != IntPtr.Zero)
        {
            Verify();
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
        if (Scratch != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(Scratch);
            Scratch = IntPtr.Zero;
        }
        if (Windowsˉlibrary != IntPtr.Zero)
        {
            NativeLibrary.Free(Windowsˉlibrary);
        }
    }

    private void Verify()
    {
        var Actual = new byte[Initialˉtable.Length];
        Marshal.Copy(Address, Actual, 0, Actual.Length);
        if (!Actual.AsSpan().SequenceEqual(Initialˉtable))
        {
            throw new InvalidOperationException(
                "The native file-output table violated its independently verified static layout.");
        }
    }

    private ulong Export(string name) =>
        checked((ulong)NativeLibrary.GetExport(Windowsˉlibrary, name).ToInt64());
}
