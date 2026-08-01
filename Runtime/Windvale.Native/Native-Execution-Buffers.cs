using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

internal sealed class Nativeˉexecutionˉbuffers : IDisposable
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
    private readonly Hostedˉresourceˉcontext? Resources;
    private readonly Dictionary<uint, Nativeˉborrowedˉbuffer> Arguments = [];
    private readonly Dictionary<string, Nativeˉborrowedˉbuffer> Files = new(StringComparer.Ordinal);
    private readonly List<Nativeˉborrowedˉbuffer> Allocations = [];
    private bool Isˉdisposed;

    public Nativeˉexecutionˉbuffers(Hostedˉresourceˉcontext? resources)
    {
        Resources = resources;
        Recordˉarena = Allocateˉuninitialized(Nativeˉcontract.MAXIMUM_RECORD_ARENA_BYTES);
    }

    public Nativeˉborrowedˉbuffer Recordˉarena { get; }

    public uint Argumentˉcount => Requireˉresources().Getˉargumentˉcount();

    public Nativeˉborrowedˉbuffer Getˉargument(uint index)
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        if (Arguments.TryGetValue(index, out var Existing))
        {
            return Existing;
        }

        var Bytes = STRICT_UTF8.GetBytes(Requireˉresources().Getˉargument(index)).ToImmutableArray();
        var Buffer = Allocate(Bytes);
        Arguments.Add(index, Buffer);
        return Buffer;
    }

    public Nativeˉborrowedˉbuffer Readˉfile(string resourceˉname)
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        if (Files.TryGetValue(resourceˉname, out var Existing))
        {
            return Existing;
        }

        var Buffer = Allocate(Requireˉresources().Readˉfileˉbytes(resourceˉname));
        Files.Add(resourceˉname, Buffer);
        return Buffer;
    }

    public string Readˉtext(
        IntPtr address,
        uint length,
        IntPtr fragmentˉaddress,
        int fragmentˉlength)
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        if (length > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
        {
            throw new InvalidOperationException("The native text descriptor exceeds the UTF-8 text limit.");
        }

        var Isˉfragment = Contains(fragmentˉaddress, fragmentˉlength, address, length);
        var Isˉowned = Allocations.Any(Buffer =>
            Contains(Buffer.Address, Buffer.Allocationˉlength, address, length));
        if (!Isˉfragment && !Isˉowned)
        {
            throw new InvalidOperationException("The native text descriptor is outside verified immutable storage.");
        }

        var Bytes = new byte[checked((int)length)];
        if (Bytes.Length != 0)
        {
            Marshal.Copy(address, Bytes, 0, Bytes.Length);
        }
        return STRICT_UTF8.GetString(Bytes);
    }

    public bool Isˉvalidˉutf8(
        IntPtr address,
        uint length,
        IntPtr fragmentˉaddress,
        int fragmentˉlength)
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        var Isˉfragment = Contains(fragmentˉaddress, fragmentˉlength, address, length);
        var Isˉowned = Allocations.Any(Buffer =>
            Contains(Buffer.Address, Buffer.Allocationˉlength, address, length));
        if (!Isˉfragment && !Isˉowned)
        {
            throw new InvalidOperationException("The native byte descriptor is outside verified immutable storage.");
        }

        var Bytes = new byte[checked((int)length)];
        if (Bytes.Length != 0)
        {
            Marshal.Copy(address, Bytes, 0, Bytes.Length);
        }
        try
        {
            _ = STRICT_UTF8.GetCharCount(Bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    public static void Writeˉdescriptor(IntPtr descriptor, Nativeˉborrowedˉbuffer buffer)
    {
        Marshal.WriteInt64(descriptor, Nativeˉcontract.BORROWED_BYTES_POINTER_OFFSET, buffer.Address.ToInt64());
        Marshal.WriteInt32(descriptor, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET, buffer.Length);
        Marshal.WriteInt32(descriptor, Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET, 0);
    }

    public void Dispose()
    {
        if (Isˉdisposed)
        {
            return;
        }
        Isˉdisposed = true;
        for (var Index = Allocations.Count - 1; Index >= 0; Index--)
        {
            Marshal.FreeHGlobal(Allocations[Index].Address);
        }
        Allocations.Clear();
    }

    private Nativeˉborrowedˉbuffer Allocate(ImmutableArray<byte> bytes)
    {
        var Allocationˉlength = Math.Max(1, bytes.Length);
        var Address = Marshal.AllocHGlobal(Allocationˉlength);
        if (!bytes.IsEmpty)
        {
            Marshal.Copy(bytes.ToArray(), 0, Address, bytes.Length);
        }
        var Buffer = new Nativeˉborrowedˉbuffer(Address, bytes.Length, Allocationˉlength);
        Allocations.Add(Buffer);
        return Buffer;
    }

    private Nativeˉborrowedˉbuffer Allocateˉuninitialized(int length)
    {
        var Address = Marshal.AllocHGlobal(length);
        var Buffer = new Nativeˉborrowedˉbuffer(Address, length, length);
        Allocations.Add(Buffer);
        return Buffer;
    }

    private Hostedˉresourceˉcontext Requireˉresources() =>
        Resources ?? throw new InvalidOperationException("Native hosted resources are unavailable.");

    private static bool Contains(IntPtr owner, int ownerˉlength, IntPtr value, uint valueˉlength)
    {
        var Ownerˉstart = owner.ToInt64();
        var Ownerˉend = checked(Ownerˉstart + ownerˉlength);
        var Valueˉstart = value.ToInt64();
        var Valueˉend = checked(Valueˉstart + valueˉlength);
        return Valueˉstart >= Ownerˉstart && Valueˉend <= Ownerˉend;
    }
}

internal readonly record struct Nativeˉborrowedˉbuffer(
    IntPtr Address,
    int Length,
    int Allocationˉlength);
