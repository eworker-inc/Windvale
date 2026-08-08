using System.Buffers.Binary;
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
    private readonly List<Nativeˉborrowedˉbuffer> Allocations = [];
    private bool Isˉdisposed;

    public Nativeˉexecutionˉbuffers(
        Hostedˉresourceˉcontext? resources,
        bool prepareˉarguments)
    {
        Resources = resources;
        try
        {
            Recordˉarena = Allocateˉuninitialized(Nativeˉcontract.MAXIMUM_RECORD_ARENA_BYTES);
            Textˉarena = Allocateˉuninitialized(Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES);
            if (prepareˉarguments)
            {
                Prepareˉarguments();
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public Nativeˉborrowedˉbuffer Recordˉarena { get; }

    public Nativeˉborrowedˉbuffer Textˉarena { get; }

    public Nativeˉborrowedˉbuffer Argumentˉtable { get; private set; }

    public uint Argumentˉcount { get; private set; }

    public Nativeˉborrowedˉbuffer Prepareˉentryˉinput(ImmutableArray<byte> input)
    {
        if (input.IsDefault || input.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4020",
                $"Native entry byte input must be initialized and no larger than " +
                    $"{Bytecodeˉlimits.MAX_BYTE_DATA_BYTES} bytes.");
        }

        var Buffer = Allocate(input);
        if (Buffer.Length != input.Length || Buffer.Allocationˉlength < Math.Max(1, input.Length))
        {
            throw new InvalidOperationException("The native entry input has an invalid bounded layout.");
        }
        var Actual = new byte[input.Length];
        if (Actual.Length != 0)
        {
            Marshal.Copy(Buffer.Address, Actual, 0, Actual.Length);
        }
        if (!Actual.AsSpan().SequenceEqual(input.AsSpan()))
        {
            throw new InvalidOperationException("The native entry input did not retain its immutable bytes.");
        }
        return Buffer;
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

    private void Prepareˉarguments()
    {
        var Arguments = Requireˉresources().Arguments;
        var Encoded = Arguments.Select(Argument =>
            STRICT_UTF8.GetBytes(Argument).ToImmutableArray()).ToImmutableArray();
        var Totalˉbytes = Encoded.Sum(Argument => Argument.Length);
        if (Arguments.Length == 0)
        {
            Argumentˉcount = 0;
            Argumentˉtable = default;
            return;
        }

        var Packedˉbytes = new byte[Totalˉbytes];
        var Offsets = new int[Encoded.Length];
        var Packedˉoffset = 0;
        for (var Index = 0; Index < Encoded.Length; Index++)
        {
            Offsets[Index] = Packedˉoffset;
            Encoded[Index].CopyTo(Packedˉbytes, Packedˉoffset);
            Packedˉoffset = checked(Packedˉoffset + Encoded[Index].Length);
        }

        var Packed = Allocate(Packedˉbytes.ToImmutableArray());
        var Tableˉbytes = new byte[checked(Arguments.Length * Nativeˉcontract.VALUE_SLOT_BYTES)];
        for (var Index = 0; Index < Arguments.Length; Index++)
        {
            var Descriptor = Tableˉbytes.AsSpan(Index * Nativeˉcontract.VALUE_SLOT_BYTES);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Descriptor[Nativeˉcontract.BORROWED_TEXT_POINTER_OFFSET..],
                checked((ulong)(Packed.Address.ToInt64() + Offsets[Index])));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Descriptor[Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET..],
                checked((uint)Encoded[Index].Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Descriptor[Nativeˉcontract.BORROWED_TEXT_RESERVED_OFFSET..],
                0);
        }

        var Table = Allocate(Tableˉbytes.ToImmutableArray());
        Verifyˉargumentˉtable(Encoded, Offsets, Packed, Table);
        Argumentˉtable = Table;
        Argumentˉcount = checked((uint)Arguments.Length);
    }

    private static void Verifyˉargumentˉtable(
        ImmutableArray<ImmutableArray<byte>> arguments,
        int[] offsets,
        Nativeˉborrowedˉbuffer packed,
        Nativeˉborrowedˉbuffer table)
    {
        if (arguments.Length != offsets.Length ||
            table.Length != checked(arguments.Length * Nativeˉcontract.VALUE_SLOT_BYTES))
        {
            throw new InvalidOperationException("The native argument table has an invalid bounded layout.");
        }

        var Tableˉbytes = new byte[table.Length];
        Marshal.Copy(table.Address, Tableˉbytes, 0, Tableˉbytes.Length);
        for (var Index = 0; Index < arguments.Length; Index++)
        {
            var Descriptor = Tableˉbytes.AsSpan(Index * Nativeˉcontract.VALUE_SLOT_BYTES);
            var Address = BinaryPrimitives.ReadUInt64LittleEndian(
                Descriptor[Nativeˉcontract.BORROWED_TEXT_POINTER_OFFSET..]);
            var Length = BinaryPrimitives.ReadUInt32LittleEndian(
                Descriptor[Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET..]);
            var Reserved = BinaryPrimitives.ReadUInt32LittleEndian(
                Descriptor[Nativeˉcontract.BORROWED_TEXT_RESERVED_OFFSET..]);
            var Expectedˉaddress = checked((ulong)(packed.Address.ToInt64() + offsets[Index]));
            if (Address != Expectedˉaddress ||
                Length != (uint)arguments[Index].Length ||
                Reserved != 0 ||
                offsets[Index] < 0 ||
                offsets[Index] > packed.Allocationˉlength ||
                Length > (uint)(packed.Allocationˉlength - offsets[Index]))
            {
                throw new InvalidOperationException(
                    $"Native argument descriptor {Index} does not match its verified immutable source.");
            }

            var Actual = new byte[checked((int)Length)];
            if (Actual.Length != 0)
            {
                Marshal.Copy(new IntPtr(checked((long)Address)), Actual, 0, Actual.Length);
            }
            if (!Actual.AsSpan().SequenceEqual(arguments[Index].AsSpan()))
            {
                throw new InvalidOperationException(
                    $"Native argument descriptor {Index} does not retain its exact UTF-8 bytes.");
            }
        }
    }

    private Hostedˉresourceˉcontext Requireˉresources() =>
        Resources ?? throw new InvalidOperationException("Native hosted resources are unavailable.");

}

internal readonly record struct Nativeˉborrowedˉbuffer(
    IntPtr Address,
    int Length,
    int Allocationˉlength);
