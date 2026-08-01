using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windvale.Compiler.Native;
using Windvale.Runtime;

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

    public static int Executeˉi32(
        Nativeˉfragment fragment,
        string entry = "Main",
        long maximumˉinstructions = Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS,
        int maximumˉcallˉdepth = Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH,
        Nativeˉhostˉservices? hostˉservices = null)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        ArgumentNullException.ThrowIfNull(entry);
        if (maximumˉinstructions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumˉinstructions),
                "The maximum instruction count must be positive.");
        }
        if (maximumˉcallˉdepth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumˉcallˉdepth),
                "The maximum call depth must be positive.");
        }
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException("The first native executor requires an x86-64 process.");
        }
        Requireˉservices(fragment, hostˉservices);
        var Entry = fragment.Symbols.SingleOrDefault(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            StringComparer.Ordinal.Equals(Symbol.Name, entry));
        if (Entry is null || Entry.Size == 0)
        {
            throw new Nativeˉbackendˉexception("WVN4001", $"Native entry '{entry}' is missing or empty.");
        }

        Nativeˉserviceˉfailure? Serviceˉfailure = null;
        using var Buffers = new Nativeˉexecutionˉbuffers(hostˉservices?.Resources);
        var Callbacks = new List<Delegate>();
        var Callbackˉpointers = new Dictionary<Nativeˉservice, IntPtr>();
        var Address = IntPtr.Zero;
        if (fragment.Requiredˉservices.Contains(Nativeˉservice.Consoleˉwriteˉline))
        {
            Nativeˉconsoleˉwriteˉlineˉcallback Callback = (textˉaddress, textˉlength) =>
            {
                try
                {
                    var Text = Buffers.Readˉtext(textˉaddress, textˉlength, Address, fragment.Code.Length);
                    hostˉservices!.Standardˉoutput!.Write(Text);
                    hostˉservices.Standardˉoutput.Write('\n');
                    return 0;
                }
                catch (Exception Exception)
                {
                    Serviceˉfailure ??= Toˉserviceˉfailure(Exception);
                    return 1;
                }
            };
            Callbacks.Add(Callback);
            Callbackˉpointers.Add(
                Nativeˉservice.Consoleˉwriteˉline,
                Marshal.GetFunctionPointerForDelegate(Callback));
        }
        if (fragment.Requiredˉservices.Contains(Nativeˉservice.Processˉargumentˉcount))
        {
            Nativeˉprocessˉargumentˉcountˉcallback Callback = () => Buffers.Argumentˉcount;
            Callbacks.Add(Callback);
            Callbackˉpointers.Add(
                Nativeˉservice.Processˉargumentˉcount,
                Marshal.GetFunctionPointerForDelegate(Callback));
        }
        if (fragment.Requiredˉservices.Contains(Nativeˉservice.Processˉargument))
        {
            Nativeˉprocessˉargumentˉcallback Callback = (index, descriptor) =>
            {
                try
                {
                    Nativeˉexecutionˉbuffers.Writeˉdescriptor(descriptor, Buffers.Getˉargument(index));
                    return 0;
                }
                catch (Exception Exception)
                {
                    Serviceˉfailure ??= Toˉserviceˉfailure(Exception);
                    return 1;
                }
            };
            Callbacks.Add(Callback);
            Callbackˉpointers.Add(
                Nativeˉservice.Processˉargument,
                Marshal.GetFunctionPointerForDelegate(Callback));
        }
        if (fragment.Requiredˉservices.Contains(Nativeˉservice.Fileˉreadˉbytes))
        {
            Nativeˉfileˉreadˉbytesˉcallback Callback = (nameˉaddress, nameˉlength, descriptor) =>
            {
                try
                {
                    var Name = Buffers.Readˉtext(nameˉaddress, nameˉlength, Address, fragment.Code.Length);
                    Nativeˉexecutionˉbuffers.Writeˉdescriptor(descriptor, Buffers.Readˉfile(Name));
                    return 0;
                }
                catch (Exception Exception)
                {
                    Serviceˉfailure ??= Toˉserviceˉfailure(Exception);
                    return 1;
                }
            };
            Callbacks.Add(Callback);
            Callbackˉpointers.Add(
                Nativeˉservice.Fileˉreadˉbytes,
                Marshal.GetFunctionPointerForDelegate(Callback));
        }

        var Serviceˉthunkˉoffset = checked((fragment.Code.Length + 15) & ~15);
        var Serviceˉthunks = new List<byte>();
        var Serviceˉoffsets = new Dictionary<Nativeˉservice, int>();
        foreach (var Service in fragment.Requiredˉservices)
        {
            while ((Serviceˉthunks.Count & 15) != 0)
            {
                Serviceˉthunks.Add(0x90);
            }
            Serviceˉoffsets.Add(Service, checked(Serviceˉthunkˉoffset + Serviceˉthunks.Count));
            Serviceˉthunks.AddRange(Buildˉserviceˉthunk(Service, Callbackˉpointers[Service]));
        }
        var Allocationˉbytes = checked(Serviceˉthunkˉoffset + Serviceˉthunks.Count);
        Address = Allocateˉwritable((nuint)Allocationˉbytes);
        var Serviceˉtable = IntPtr.Zero;
        var Context = IntPtr.Zero;
        ulong Outcome;
        try
        {
            var Linkedˉcode = new byte[Allocationˉbytes];
            fragment.Code.CopyTo(Linkedˉcode);
            Applyˉpatches(fragment, Address, Linkedˉcode);
            Serviceˉthunks.CopyTo(Linkedˉcode, Serviceˉthunkˉoffset);
            Marshal.Copy(Linkedˉcode, 0, Address, Linkedˉcode.Length);
            Finalizeˉexecutable(Address, (nuint)Linkedˉcode.Length);

            if (Serviceˉthunks.Count != 0)
            {
                Serviceˉtable = Marshal.AllocHGlobal(checked((int)Nativeˉserviceˉtableˉcontract.SIZE));
                var Tableˉbytes = new byte[checked((int)Nativeˉserviceˉtableˉcontract.SIZE)];
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Tableˉbytes.AsSpan(Nativeˉserviceˉtableˉcontract.FORMAT_VERSION_OFFSET),
                    Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Tableˉbytes.AsSpan(Nativeˉserviceˉtableˉcontract.SIZE_OFFSET),
                    Nativeˉserviceˉtableˉcontract.SIZE);
                foreach (var Service in fragment.Requiredˉservices)
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(
                        Tableˉbytes.AsSpan(Serviceˉtableˉpointerˉoffset(Service)),
                        checked((ulong)(Address.ToInt64() + Serviceˉoffsets[Service])));
                }
                Marshal.Copy(Tableˉbytes, 0, Serviceˉtable, Tableˉbytes.Length);
            }

            Context = Marshal.AllocHGlobal(checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE));
            var Contextˉbytes = new byte[checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE)];
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET),
                Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET),
                Nativeˉexecutionˉcontextˉcontract.SIZE);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET),
                checked((ulong)maximumˉinstructions));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET),
                checked((ulong)maximumˉcallˉdepth));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET),
                Serviceˉtable == IntPtr.Zero ? 0 : checked((ulong)Serviceˉtable.ToInt64()));
            Marshal.Copy(Contextˉbytes, 0, Context, Contextˉbytes.Length);

            var Entryˉaddress = checked(Address.ToInt64() + Entry.Offset);
            var Function = Marshal.GetDelegateForFunctionPointer<Nativeˉi32ˉentry>(new(Entryˉaddress));
            var Contextˉpointer = checked((ulong)Context.ToInt64());
            Outcome = Function(0, Contextˉpointer, Contextˉpointer, 0, 0, 0);
            GC.KeepAlive(Callbacks);
        }
        finally
        {
            if (Context != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Context);
            }
            if (Serviceˉtable != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Serviceˉtable);
            }
            Release(Address, (nuint)Allocationˉbytes);
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
        if (Status == 2)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3011",
                $"The native instruction limit {maximumˉinstructions} was exceeded in entry '{entry}'.");
        }
        if (Status == 3)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3004",
                $"The native call-depth limit {maximumˉcallˉdepth} was exceeded in entry '{entry}'.");
        }
        if (Status == 4)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3005",
                $"A native static-data index was outside its immutable array in entry '{entry}'.");
        }
        if (Status == 5)
        {
            throw new Nativeˉtrapˉexception(
                Serviceˉfailure?.Code ?? "WVR3013",
                Serviceˉfailure is null
                    ? $"A native runtime service rejected its request in entry '{entry}'."
                    : $"A native runtime service failed in entry '{entry}': {Serviceˉfailure.Message}");
        }
        if (Status == 6)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3008",
                $"A native byte slice or fixed-width read was outside its immutable source in entry '{entry}'.");
        }
        throw new Nativeˉbackendˉexception(
            "WVN4005",
            $"Native entry '{entry}' returned unknown status {Status}.");
    }

    private static void Requireˉservices(
        Nativeˉfragment fragment,
        Nativeˉhostˉservices? hostˉservices)
    {
        foreach (var Service in fragment.Requiredˉservices)
        {
            if (hostˉservices is null || !hostˉservices.Isˉauthorized(Service))
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3010",
                    $"Native service '{Service}' was required but not authorized.");
            }
            if (!hostˉservices.Supports(Service))
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3001",
                    $"The host does not implement native service '{Service}'.");
            }
        }
    }

    private static byte[] Buildˉserviceˉthunk(
        Nativeˉservice service,
        IntPtr callbackˉaddress)
    {
        var Code = new List<byte>
        {
            0x48, 0x89, 0xE0,
            0x48, 0x83, 0xE4, 0xF0,
            0x48, 0x83, 0xEC, 0x40,
            0x48, 0x89, 0x44, 0x24, 0x20,
            0x4C, 0x89, 0x54, 0x24, 0x28,
            0x4C, 0x89, 0x5C, 0x24, 0x30,
            0x4C, 0x89, 0x7C, 0x24, 0x38,
        };
        var Arguments = Serviceˉargumentˉadapter(service);
        Code.AddRange(Arguments);
        Code.AddRange([0x48, 0xB8]);
        var Callbackˉoffset = Code.Count;
        Code.AddRange(new byte[sizeof(ulong)]);
        BinaryPrimitives.WriteUInt64LittleEndian(
            CollectionsMarshal.AsSpan(Code).Slice(Callbackˉoffset, sizeof(ulong)),
            checked((ulong)callbackˉaddress.ToInt64()));
        Code.AddRange(
        [
            0xFF, 0xD0,
            0x4C, 0x8B, 0x54, 0x24, 0x28,
            0x4C, 0x8B, 0x5C, 0x24, 0x30,
            0x4C, 0x8B, 0x7C, 0x24, 0x38,
            0x48, 0x8B, 0x64, 0x24, 0x20,
            0xC3,
        ]);
        var Bytes = Code.ToArray();
        Verifyˉserviceˉthunk(Bytes, service, callbackˉaddress);
        return Bytes;
    }

    private static void Verifyˉserviceˉthunk(
        ReadOnlySpan<byte> code,
        Nativeˉservice service,
        IntPtr callbackˉaddress)
    {
        ReadOnlySpan<byte> Prefix =
        [
            0x48, 0x89, 0xE0,
            0x48, 0x83, 0xE4, 0xF0,
            0x48, 0x83, 0xEC, 0x40,
            0x48, 0x89, 0x44, 0x24, 0x20,
            0x4C, 0x89, 0x54, 0x24, 0x28,
            0x4C, 0x89, 0x5C, 0x24, 0x30,
            0x4C, 0x89, 0x7C, 0x24, 0x38,
        ];
        ReadOnlySpan<byte> Arguments = Serviceˉargumentˉadapter(service);
        ReadOnlySpan<byte> Suffix =
        [
            0xFF, 0xD0,
            0x4C, 0x8B, 0x54, 0x24, 0x28,
            0x4C, 0x8B, 0x5C, 0x24, 0x30,
            0x4C, 0x8B, 0x7C, 0x24, 0x38,
            0x48, 0x8B, 0x64, 0x24, 0x20,
            0xC3,
        ];
        var Callbackˉoffset = Prefix.Length + Arguments.Length + 2;
        var Suffixˉoffset = Callbackˉoffset + sizeof(ulong);
        if (code.Length != Suffixˉoffset + Suffix.Length ||
            !code[..Prefix.Length].SequenceEqual(Prefix) ||
            !code.Slice(Prefix.Length, Arguments.Length).SequenceEqual(Arguments) ||
            code[Callbackˉoffset - 2] != 0x48 ||
            code[Callbackˉoffset - 1] != 0xB8 ||
            BinaryPrimitives.ReadUInt64LittleEndian(code.Slice(Callbackˉoffset, sizeof(ulong))) !=
                checked((ulong)callbackˉaddress.ToInt64()) ||
            !code[Suffixˉoffset..].SequenceEqual(Suffix))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4010",
                $"The native '{service}' service thunk violated its bounded platform adapter contract.");
        }
    }

    private static byte[] Serviceˉargumentˉadapter(Nativeˉservice service)
    {
        if (OperatingSystem.IsWindows())
        {
            return service switch
            {
                Nativeˉservice.Consoleˉwriteˉline => [0x4C, 0x89, 0xC1, 0x44, 0x89, 0xCA],
                Nativeˉservice.Processˉargumentˉcount => [],
                Nativeˉservice.Processˉargument => [0x44, 0x89, 0xC1, 0x4C, 0x89, 0xCA],
                Nativeˉservice.Fileˉreadˉbytes =>
                [
                    0x48, 0x89, 0xC8,
                    0x4C, 0x89, 0xC1,
                    0x44, 0x89, 0xCA,
                    0x49, 0x89, 0xC0,
                ],
                _ => throw new Nativeˉbackendˉexception("WVN4010", "Unknown native service thunk."),
            };
        }
        if (OperatingSystem.IsLinux())
        {
            return service switch
            {
                Nativeˉservice.Consoleˉwriteˉline => [0x4C, 0x89, 0xC7, 0x44, 0x89, 0xCE],
                Nativeˉservice.Processˉargumentˉcount => [],
                Nativeˉservice.Processˉargument => [0x44, 0x89, 0xC7, 0x4C, 0x89, 0xCE],
                Nativeˉservice.Fileˉreadˉbytes =>
                [
                    0x4C, 0x89, 0xC7,
                    0x44, 0x89, 0xCE,
                    0x48, 0x89, 0xCA,
                ],
                _ => throw new Nativeˉbackendˉexception("WVN4010", "Unknown native service thunk."),
            };
        }
        throw new PlatformNotSupportedException("The native service thunks support Windows and Linux.");
    }

    private static int Serviceˉtableˉpointerˉoffset(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline =>
                Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
            Nativeˉservice.Processˉargumentˉcount =>
                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_COUNT_POINTER_OFFSET,
            Nativeˉservice.Processˉargument =>
                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_POINTER_OFFSET,
            Nativeˉservice.Fileˉreadˉbytes =>
                Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
            _ => throw new Nativeˉbackendˉexception("WVN4010", "Unknown native service table entry."),
        };

    private static Nativeˉserviceˉfailure Toˉserviceˉfailure(Exception exception) =>
        exception is Runtimeˉexception Runtime
            ? new(Runtime.Code, Runtime.Message)
            : new("WVR3013", exception.Message);

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
    private delegate ulong Nativeˉi32ˉentry(
        ulong windowsˉpadding,
        ulong windowsˉcontext,
        ulong systemˉvˉcontext,
        ulong windowsˉpaddingˉfour,
        ulong systemˉvˉpadding,
        ulong systemˉvˉpaddingˉsix);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Nativeˉconsoleˉwriteˉlineˉcallback(
        IntPtr textˉaddress,
        uint textˉlength);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Nativeˉprocessˉargumentˉcountˉcallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Nativeˉprocessˉargumentˉcallback(
        uint index,
        IntPtr descriptor);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint Nativeˉfileˉreadˉbytesˉcallback(
        IntPtr resourceˉnameˉaddress,
        uint resourceˉnameˉlength,
        IntPtr descriptor);

    private sealed record Nativeˉserviceˉfailure(string Code, string Message);

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
