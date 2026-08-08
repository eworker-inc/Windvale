using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class Nativeˉoutputˉtableˉcontract
{
    public const uint MAGIC = 0x4F49_5657;
    public const uint FORMAT_VERSION = 1;
    public const uint SIZE = 48;
    public const uint CONSOLE_PRESENT = 1;
    public const uint DIAGNOSTIC_PRESENT = 2;
    public const int MAGIC_OFFSET = 0;
    public const int FORMAT_VERSION_OFFSET = 4;
    public const int SIZE_OFFSET = 8;
    public const int PLATFORM_OFFSET = 12;
    public const int FLAGS_OFFSET = 16;
    public const int RESERVED_OFFSET = 20;
    public const int CONSOLE_TARGET_OFFSET = 24;
    public const int DIAGNOSTIC_TARGET_OFFSET = 32;
    public const int WRITE_FUNCTION_POINTER_OFFSET = 40;
}

internal sealed class Nativeˉoutputˉcontext : IDisposable
{
    private readonly SafeFileHandle? Consoleˉhandle;
    private readonly SafeFileHandle? Diagnosticˉhandle;
    private readonly bool Consoleˉreference;
    private readonly bool Diagnosticˉreference;
    private readonly IntPtr Writeˉlibrary;
    private bool Isˉdisposed;

    public Nativeˉoutputˉcontext(
        Nativeˉhostˉservices? services,
        bool requireˉconsole,
        bool requireˉdiagnostic)
    {
        if (!requireˉconsole && !requireˉdiagnostic)
        {
            return;
        }

        try
        {
            Platform = Nativeˉoutputˉchannel.Currentˉplatform();
            if (requireˉconsole)
            {
                Consoleˉhandle = services!.Standardˉoutput!.Handle;
                var Added = false;
                Consoleˉhandle.DangerousAddRef(ref Added);
                Consoleˉreference = Added;
            }
            if (requireˉdiagnostic)
            {
                Diagnosticˉhandle = services!.Diagnosticˉoutput!.Handle;
                var Added = false;
                Diagnosticˉhandle.DangerousAddRef(ref Added);
                Diagnosticˉreference = Added;
            }

            var Writeˉfunction = IntPtr.Zero;
            if (Platform == Nativeˉoutputˉplatform.Windows)
            {
                Writeˉlibrary = NativeLibrary.Load("kernel32.dll");
                Writeˉfunction = NativeLibrary.GetExport(Writeˉlibrary, "WriteFile");
            }

            var Flags = (requireˉconsole ? Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT : 0) |
                (requireˉdiagnostic ? Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_PRESENT : 0);
            var Bytes = Nativeˉoutputˉtableˉbuilder.Build(
                Platform,
                Flags,
                requireˉconsole ? Handleˉvalue(Consoleˉhandle!) : 0,
                requireˉdiagnostic ? Handleˉvalue(Diagnosticˉhandle!) : 0,
                Writeˉfunction == IntPtr.Zero ? 0 : checked((ulong)Writeˉfunction.ToInt64()));

            Address = Marshal.AllocHGlobal(Bytes.Length);
            Marshal.Copy(Bytes.ToArray(), 0, Address, Bytes.Length);
            Verifyˉtable(Bytes.AsSpan(), requireˉconsole, requireˉdiagnostic, Platform);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public IntPtr Address { get; private set; }

    public Nativeˉoutputˉplatform Platform { get; }

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
        if (Writeˉlibrary != IntPtr.Zero)
        {
            NativeLibrary.Free(Writeˉlibrary);
        }
        if (Diagnosticˉreference)
        {
            Diagnosticˉhandle!.DangerousRelease();
        }
        if (Consoleˉreference)
        {
            Consoleˉhandle!.DangerousRelease();
        }
    }

    private void Verifyˉtable(
        ReadOnlySpan<byte> expected,
        bool requireˉconsole,
        bool requireˉdiagnostic,
        Nativeˉoutputˉplatform platform)
    {
        var Actual = new byte[expected.Length];
        Marshal.Copy(Address, Actual, 0, Actual.Length);
        var Flags = BinaryPrimitives.ReadUInt32LittleEndian(
            Actual.AsSpan(Nativeˉoutputˉtableˉcontract.FLAGS_OFFSET));
        var Console = BinaryPrimitives.ReadUInt64LittleEndian(
            Actual.AsSpan(Nativeˉoutputˉtableˉcontract.CONSOLE_TARGET_OFFSET));
        var Diagnostic = BinaryPrimitives.ReadUInt64LittleEndian(
            Actual.AsSpan(Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_TARGET_OFFSET));
        var Writer = BinaryPrimitives.ReadUInt64LittleEndian(
            Actual.AsSpan(Nativeˉoutputˉtableˉcontract.WRITE_FUNCTION_POINTER_OFFSET));
        if (!Actual.AsSpan().SequenceEqual(expected) ||
            BinaryPrimitives.ReadUInt32LittleEndian(
                Actual.AsSpan(Nativeˉoutputˉtableˉcontract.MAGIC_OFFSET)) !=
                Nativeˉoutputˉtableˉcontract.MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(
                Actual.AsSpan(Nativeˉoutputˉtableˉcontract.FORMAT_VERSION_OFFSET)) !=
                Nativeˉoutputˉtableˉcontract.FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(
                Actual.AsSpan(Nativeˉoutputˉtableˉcontract.SIZE_OFFSET)) !=
                Nativeˉoutputˉtableˉcontract.SIZE ||
            BinaryPrimitives.ReadUInt32LittleEndian(
                Actual.AsSpan(Nativeˉoutputˉtableˉcontract.PLATFORM_OFFSET)) != (uint)platform ||
            BinaryPrimitives.ReadUInt32LittleEndian(
                Actual.AsSpan(Nativeˉoutputˉtableˉcontract.RESERVED_OFFSET)) != 0 ||
            Flags != ((requireˉconsole ? Nativeˉoutputˉtableˉcontract.CONSOLE_PRESENT : 0) |
                (requireˉdiagnostic ? Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_PRESENT : 0)) ||
            (requireˉconsole ? Console != Handleˉvalue(Consoleˉhandle!) : Console != 0) ||
            (requireˉdiagnostic ? Diagnostic != Handleˉvalue(Diagnosticˉhandle!) : Diagnostic != 0) ||
            (platform == Nativeˉoutputˉplatform.Windows ? Writer == 0 : Writer != 0))
        {
            throw new InvalidOperationException(
                "The native output table does not match its independently verified host inputs.");
        }
    }

    private static ulong Handleˉvalue(SafeFileHandle handle) =>
        unchecked((ulong)handle.DangerousGetHandle().ToInt64());
}
