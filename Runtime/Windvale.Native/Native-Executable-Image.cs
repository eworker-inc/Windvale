using System.ComponentModel;
using System.Runtime.InteropServices;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal sealed class Nativeˉexecutableˉimage : IDisposable
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

    private readonly Nativeˉpublicationˉlifetimeˉplan Plan;
    private IntPtr Address;

    private Nativeˉexecutableˉimage(Nativeˉpublicationˉlifetimeˉplan plan)
    {
        Plan = plan;
    }

    internal Nativeˉpublicationˉstate State { get; private set; } = Nativeˉpublicationˉstate.Unallocated;

    internal IntPtr Executableˉaddress
    {
        get
        {
            if (Address == IntPtr.Zero ||
                State is not (Nativeˉpublicationˉstate.Executable or Nativeˉpublicationˉstate.Invoked))
            {
                throw Invalidˉtransition("Executable memory is unavailable in the current publication state.");
            }
            return Address;
        }
    }

    internal static Nativeˉexecutableˉimage Allocateˉwritable(Nativeˉpublicationˉlifetimeˉplan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        X64ˉnativeˉpublicationˉlifetime.Verifyˉplan(plan);
        var Image = new Nativeˉexecutableˉimage(plan);
        var Transition = Image.Requireˉtransition(Nativeˉpublicationˉaction.Allocateˉwritable);
        Image.Address = Allocateˉplatformˉwritable(checked((nuint)plan.Imageˉbytes));
        Image.State = Transition.Nextˉstate;
        return Image;
    }

    internal void Copyˉimage(byte[] image)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Length != Plan.Imageˉbytes)
        {
            throw Invalidˉtransition("The publication image does not match its planned extent.");
        }
        var Transition = Requireˉtransition(Nativeˉpublicationˉaction.Copyˉimage);
        Marshal.Copy(image, 0, Address, image.Length);
        State = Transition.Nextˉstate;
    }

    internal void Sealˉexecutable()
    {
        var Transition = Requireˉtransition(Nativeˉpublicationˉaction.Sealˉexecutable);
        Sealˉplatformˉexecutable(Address, checked((nuint)Plan.Imageˉbytes));
        State = Transition.Nextˉstate;
    }

    internal TResult Invoke<TResult>(Func<IntPtr, TResult> invocation)
    {
        ArgumentNullException.ThrowIfNull(invocation);
        var Transition = Requireˉtransition(Nativeˉpublicationˉaction.Invoke);
        var Result = invocation(Address);
        State = Transition.Nextˉstate;
        return Result;
    }

    public void Dispose()
    {
        if (Address == IntPtr.Zero)
        {
            return;
        }
        var Transition = Requireˉtransition(Nativeˉpublicationˉaction.Release);
        Releaseˉplatform(Address, checked((nuint)Plan.Imageˉbytes));
        Address = IntPtr.Zero;
        State = Transition.Nextˉstate;
        _ = Requireˉtransition(Nativeˉpublicationˉaction.Complete);
        GC.SuppressFinalize(this);
    }

    private Nativeˉpublicationˉtransition Requireˉtransition(Nativeˉpublicationˉaction action)
    {
        var Matches = Plan.Transitions.Where(Transition =>
            Transition.State == State && Transition.Action == action).ToArray();
        if (Matches.Length != 1)
        {
            throw Invalidˉtransition(
                $"Publication action {action} is not allowed from state {State}.");
        }
        return Matches[0];
    }

    private static IntPtr Allocateˉplatformˉwritable(nuint size)
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

    private static void Sealˉplatformˉexecutable(IntPtr address, nuint size)
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

    private static void Releaseˉplatform(IntPtr address, nuint size)
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

    private static Nativeˉbackendˉexception Invalidˉtransition(string message) =>
        new("WVN4017", message);

    private static Win32Exception Lastˉnativeˉerror(string operation) =>
        new(Marshal.GetLastPInvokeError(), $"{operation} failed.");

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
