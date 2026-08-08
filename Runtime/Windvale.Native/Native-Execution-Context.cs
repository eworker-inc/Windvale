using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal readonly record struct Nativeˉexecutionˉcontextˉcompletion(
    uint Recordˉarenaˉused,
    uint Textˉarenaˉused,
    Nativeˉserviceˉfailureˉdetail Serviceˉfailureˉdetail);

internal sealed class Nativeˉexecutionˉcontext : IDisposable
{
    private readonly Nativeˉexecutionˉcontextˉinputs Inputs;
    private readonly ImmutableArray<byte> Initialˉbytes;
    private bool Isˉdisposed;

    public Nativeˉexecutionˉcontext(
        Nativeˉexecutionˉcontextˉinputs inputs,
        bool serviceˉfreeˉbootstrap)
    {
        Inputs = inputs;
        Initialˉbytes = serviceˉfreeˉbootstrap
            ? Nativeˉstage0ˉexecutionˉcontextˉoracle.Build(inputs)
            : Nativeˉexecutionˉcontextˉbuilder.Build(inputs);
        Nativeˉexecutionˉcontextˉbuilder.Verifyˉcontextˉbytes(
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

    public Nativeˉexecutionˉcontextˉcompletion Readˉverifiedˉcompletion()
    {
        ObjectDisposedException.ThrowIf(Isˉdisposed, this);
        var Actual = new byte[Initialˉbytes.Length];
        Marshal.Copy(Address, Actual, 0, Actual.Length);
        var Recordˉused = BinaryPrimitives.ReadUInt32LittleEndian(
            Actual.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET));
        var Textˉused = BinaryPrimitives.ReadUInt32LittleEndian(
            Actual.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET));
        var Detailˉvalue = BinaryPrimitives.ReadUInt32LittleEndian(
            Actual.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET));
        if (Recordˉused > Inputs.Recordˉarenaˉlength ||
            Textˉused > Inputs.Textˉarenaˉlength ||
            Detailˉvalue > (uint)Nativeˉserviceˉfailureˉdetail.Bytesˉu16ˉoutˉofˉrange)
        {
            throw Invalidˉcompletion();
        }

        BinaryPrimitives.WriteUInt32LittleEndian(
            Actual.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET),
            0);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Actual.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET),
            0);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Actual.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET),
            0);
        if (!Actual.AsSpan().SequenceEqual(Initialˉbytes.AsSpan()))
        {
            throw Invalidˉcompletion();
        }
        return new(
            Recordˉused,
            Textˉused,
            (Nativeˉserviceˉfailureˉdetail)Detailˉvalue);
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

    private static InvalidOperationException Invalidˉcompletion() =>
        new("The native execution context changed outside its bounded mutable fields.");
}
