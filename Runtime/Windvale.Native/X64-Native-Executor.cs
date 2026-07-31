using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉexecutor
{
    private const uint MEM_COMMIT = 0x0000_1000;
    private const uint MEM_RESERVE = 0x0000_2000;
    private const uint MEM_RELEASE = 0x0000_8000;
    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_EXECUTE_READ = 0x20;

    private const int PROT_READ = 0x1;
    private const int PROT_WRITE = 0x2;
    private const int PROT_EXEC = 0x4;
    private const int MAP_PRIVATE = 0x2;
    private const int MAP_ANONYMOUS = 0x20;

    public static int Executeˉi32(Nativeˉfragment fragment, string entry = "Main")
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        ArgumentNullException.ThrowIfNull(entry);
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException("The first native executor requires an x86-64 process.");
        }
        var Entry = fragment.Symbols.SingleOrDefault(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            StringComparer.Ordinal.Equals(Symbol.Name, entry));
        if (Entry is null || Entry.Size == 0)
        {
            throw new Nativeˉbackendˉexception("WVN4001", $"Native entry '{entry}' is missing or empty.");
        }

        var Address = Allocateˉwritable((nuint)fragment.Code.Length);
        ulong Outcome;
        try
        {
            var Linkedˉcode = fragment.Code.ToArray();
            Applyˉpatches(fragment, Address, Linkedˉcode);
            Marshal.Copy(Linkedˉcode, 0, Address, Linkedˉcode.Length);
            Finalizeˉexecutable(Address, (nuint)Linkedˉcode.Length);
            var Entryˉaddress = checked(Address.ToInt64() + Entry.Offset);
            var Function = Marshal.GetDelegateForFunctionPointer<Nativeˉi32ˉentry>(new(Entryˉaddress));
            Outcome = Function();
        }
        finally
        {
            Release(Address, (nuint)fragment.Code.Length);
        }

        var Status = (uint)(Outcome >> 32);
        if (Status == 0)
        {
            return unchecked((int)(uint)Outcome);
        }
        if (Status == 1)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3007",
                $"Integer overflow in native entry '{entry}'.");
        }
        throw new Nativeˉbackendˉexception(
            "WVN4005",
            $"Native entry '{entry}' returned unknown status {Status}.");
    }

    private static void Applyˉpatches(Nativeˉfragment fragment, IntPtr address, byte[] code)
    {
        var Symbols = fragment.Symbols.ToDictionary(Symbol => Symbol.Name, StringComparer.Ordinal);
        foreach (var Patch in fragment.Patches)
        {
            var Symbol = Symbols[Patch.Symbol];
            if (Symbol.Binding == Nativeˉsymbolˉbinding.Import)
            {
                throw new Nativeˉbackendˉexception(
                    "WVN4002",
                    $"Native entry execution cannot resolve import '{Symbol.Name}' yet.");
            }

            var Targetˉaddress = checked(address.ToInt64() + Symbol.Offset);
            var Patchˉaddress = checked(address.ToInt64() + Patch.Offset);
            if (Patch.Kind == Nativeˉpatchˉkind.Relativeˉi32)
            {
                var Value = checked(Targetˉaddress + Patch.Addend - Patchˉaddress);
                if (Value is < int.MinValue or > int.MaxValue)
                {
                    throw new Nativeˉbackendˉexception("WVN4003", "A native relative patch exceeds i32.");
                }
                BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan((int)Patch.Offset, sizeof(int)), (int)Value);
            }
            else
            {
                var Value = checked(Targetˉaddress + Patch.Addend);
                if (Value is < uint.MinValue or > uint.MaxValue)
                {
                    throw new Nativeˉbackendˉexception("WVN4004", "A native absolute patch exceeds u32.");
                }
                BinaryPrimitives.WriteUInt32LittleEndian(code.AsSpan((int)Patch.Offset, sizeof(uint)), (uint)Value);
            }
        }
    }

    private static IntPtr Allocateˉwritable(nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            var Address = VirtualAlloc(IntPtr.Zero, size, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
            return Address != IntPtr.Zero
                ? Address
                : throw Lastˉnativeˉerror("VirtualAlloc");
        }
        if (OperatingSystem.IsLinux())
        {
            var Address = Mmap(IntPtr.Zero, size, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
            return Address != new IntPtr(-1)
                ? Address
                : throw Lastˉnativeˉerror("mmap");
        }
        throw new PlatformNotSupportedException("The first native executor supports Windows and Linux.");
    }

    private static void Finalizeˉexecutable(IntPtr address, nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            if (!VirtualProtect(address, size, PAGE_EXECUTE_READ, out _))
            {
                throw Lastˉnativeˉerror("VirtualProtect");
            }
            if (!FlushInstructionCache(GetCurrentProcess(), address, size))
            {
                throw Lastˉnativeˉerror("FlushInstructionCache");
            }
            return;
        }
        if (Mprotect(address, size, PROT_READ | PROT_EXEC) != 0)
        {
            throw Lastˉnativeˉerror("mprotect");
        }
    }

    private static void Release(IntPtr address, nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            if (!VirtualFree(address, 0, MEM_RELEASE))
            {
                throw Lastˉnativeˉerror("VirtualFree");
            }
            return;
        }
        if (Munmap(address, size) != 0)
        {
            throw Lastˉnativeˉerror("munmap");
        }
    }

    private static Win32Exception Lastˉnativeˉerror(string operation) =>
        new(Marshal.GetLastPInvokeError(), $"{operation} failed.");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong Nativeˉi32ˉentry();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr address, nuint size, uint allocationˉtype, uint protection);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualProtect(IntPtr address, nuint size, uint protection, out uint oldˉprotection);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFree(IntPtr address, nuint size, uint freeˉtype);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlushInstructionCache(IntPtr process, IntPtr address, nuint size);

    [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
    private static extern IntPtr Mmap(
        IntPtr address,
        nuint length,
        int protection,
        int flags,
        int fileˉdescriptor,
        nint offset);

    [DllImport("libc", EntryPoint = "mprotect", SetLastError = true)]
    private static extern int Mprotect(IntPtr address, nuint length, int protection);

    [DllImport("libc", EntryPoint = "munmap", SetLastError = true)]
    private static extern int Munmap(IntPtr address, nuint length);
}
