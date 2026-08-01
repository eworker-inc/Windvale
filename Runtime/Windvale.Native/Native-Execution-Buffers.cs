using System.Collections.Immutable;
using System.Globalization;
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
        Textˉarena = Allocateˉuninitialized(Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES);
    }

    public Nativeˉborrowedˉbuffer Recordˉarena { get; }

    public Nativeˉborrowedˉbuffer Textˉarena { get; }

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

    public string Readˉtextˉdescriptor(
        IntPtr descriptor,
        IntPtr fragmentˉaddress,
        int fragmentˉlength)
    {
        var Address = new IntPtr(Marshal.ReadInt64(descriptor, Nativeˉcontract.BORROWED_TEXT_POINTER_OFFSET));
        var Length = unchecked((uint)Marshal.ReadInt32(
            descriptor,
            Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET));
        return Readˉtext(Address, Length, fragmentˉaddress, fragmentˉlength);
    }

    public Nativeˉborrowedˉbuffer Allocateˉtext(string value, IntPtr context)
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        ArgumentNullException.ThrowIfNull(value);
        if (context == IntPtr.Zero)
        {
            throw new InvalidOperationException("The native execution context is unavailable.");
        }
        var Bytes = STRICT_UTF8.GetBytes(value);
        if (Bytes.Length > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
        {
            throw new Runtimeˉexception(
                "WVR3012",
                $"Native text result {Bytes.Length} exceeds the UTF-8 value limit.");
        }
        var Textˉarenaˉpointer = new IntPtr(Marshal.ReadInt64(
            context,
            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET));
        var Textˉarenaˉlength = Marshal.ReadInt32(
            context,
            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET);
        var Textˉarenaˉused = Marshal.ReadInt32(
            context,
            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        if (Textˉarenaˉpointer != Textˉarena.Address || Textˉarenaˉlength != Textˉarena.Length)
        {
            throw new InvalidOperationException("The native text-arena context is invalid.");
        }
        if (Textˉarenaˉused is < 0 || Textˉarenaˉused > Textˉarenaˉlength ||
            Bytes.Length > Textˉarenaˉlength - Textˉarenaˉused)
        {
            throw new Runtimeˉexception(
                "WVR3018",
                $"The native text arena exhausted its {Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES}-byte limit.");
        }

        var Address = IntPtr.Add(Textˉarena.Address, Textˉarenaˉused);
        if (Bytes.Length != 0)
        {
            Marshal.Copy(Bytes, 0, Address, Bytes.Length);
        }
        Textˉarenaˉused = checked(Textˉarenaˉused + Bytes.Length);
        Marshal.WriteInt32(
            context,
            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET,
            Textˉarenaˉused);
        return new(Address, Bytes.Length, Math.Max(1, Bytes.Length));
    }

    public Nativeˉborrowedˉbuffer Quoteˉtext(string value, IntPtr context)
    {
        var Outputˉlength = 2;
        foreach (var Character in value)
        {
            Outputˉlength = checked(Outputˉlength + Character switch
            {
                '"' or '\\' or '\b' or '\f' or '\n' or '\r' or '\t' => 2,
                >= ' ' and <= '~' => 1,
                _ => 6,
            });
            if (Outputˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
            {
                throw new Runtimeˉexception(
                    "WVR3012",
                    $"Quoted text result exceeds the UTF-8 value limit {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}.");
            }
        }

        var Result = new StringBuilder(Outputˉlength);
        Result.Append('"');
        foreach (var Character in value)
        {
            switch (Character)
            {
                case '"': Result.Append("\\\""); break;
                case '\\': Result.Append("\\\\"); break;
                case '\b': Result.Append("\\b"); break;
                case '\f': Result.Append("\\f"); break;
                case '\n': Result.Append("\\n"); break;
                case '\r': Result.Append("\\r"); break;
                case '\t': Result.Append("\\t"); break;
                case >= ' ' and <= '~': Result.Append(Character); break;
                default:
                    Result.Append("\\u");
                    Result.Append(((ushort)Character).ToString("X4", CultureInfo.InvariantCulture));
                    break;
            }
        }
        Result.Append('"');
        return Allocateˉtext(Result.ToString(), context);
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
