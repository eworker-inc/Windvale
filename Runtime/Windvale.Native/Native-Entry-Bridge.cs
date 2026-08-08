using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal sealed class Nativeˉentryˉbridge : IDisposable
{
    private readonly ImmutableArray<byte> Initialˉbytes;
    private bool Isˉdisposed;

    public Nativeˉentryˉbridge(
        Nativeˉentryˉbridgeˉinputs inputs,
        bool serviceˉfreeˉbootstrap)
    {
        Initialˉbytes = serviceˉfreeˉbootstrap
            ? Nativeˉstage0ˉentryˉbridgeˉoracle.Build(inputs)
            : Nativeˉentryˉbridgeˉbuilder.Build(inputs);
        Nativeˉentryˉbridgeˉbuilder.Verifyˉbridgeˉbytes(
            inputs,
            Initialˉbytes.AsSpan());
        Address = Marshal.AllocHGlobal(Initialˉbytes.Length);
        try
        {
            Marshal.Copy(Initialˉbytes.ToArray(), 0, Address, Initialˉbytes.Length);
        }
        catch
        {
            Marshal.FreeHGlobal(Address);
            Address = IntPtr.Zero;
            throw;
        }
    }

    public IntPtr Address { get; private set; }

    public Nativeˉentryˉresultˉdescriptor Readˉverifiedˉresultˉdescriptor()
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        var Actual = new byte[Initialˉbytes.Length];
        Marshal.Copy(Address, Actual, 0, Actual.Length);
        if (!Actual.AsSpan(Nativeˉcontract.VALUE_SLOT_BYTES).SequenceEqual(
            Initialˉbytes.AsSpan()[Nativeˉcontract.VALUE_SLOT_BYTES..]))
        {
            throw new InvalidOperationException(
                "The native entry changed its immutable input descriptor.");
        }
        return new(
            BinaryPrimitives.ReadUInt64LittleEndian(Actual),
            BinaryPrimitives.ReadUInt32LittleEndian(Actual.AsSpan(8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Actual.AsSpan(12)));
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
    }
}
