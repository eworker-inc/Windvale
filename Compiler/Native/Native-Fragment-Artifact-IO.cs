using System.Buffers.Binary;
using System.Text;

namespace Windvale.Compiler.Native;

internal sealed class Nativeˉfragmentˉartifactˉreader(ReadOnlySpan<byte> bytes)
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
    private readonly ReadOnlyMemory<byte> Bytes = bytes.ToArray();
    private int Position;

    public int Offset => Position;

    public byte Readˉu8() => Readˉbytes(1)[0];

    public ushort Readˉu16() =>
        BinaryPrimitives.ReadUInt16LittleEndian(Readˉbytes(sizeof(ushort)));

    public uint Readˉu32() =>
        BinaryPrimitives.ReadUInt32LittleEndian(Readˉbytes(sizeof(uint)));

    public int Readˉi32() =>
        BinaryPrimitives.ReadInt32LittleEndian(Readˉbytes(sizeof(int)));

    public int Readˉcount(int maximum, string kind)
    {
        var Start = Position;
        var Value = Readˉu32();
        if (Value > maximum)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1007",
                $"The native-fragment artifact exceeds the {kind}-count limit.",
                Start);
        }
        return checked((int)Value);
    }

    public string Readˉstring(int maximumˉbytes, string kind)
    {
        var Lengthˉoffset = Position;
        var Length = Readˉu32();
        if (Length is 0 || Length > maximumˉbytes)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1010",
                $"A native-fragment {kind} string has an invalid length.",
                Lengthˉoffset);
        }
        var Valueˉoffset = Position;
        try
        {
            return STRICT_UTF8.GetString(Readˉbytes(checked((int)Length)));
        }
        catch (DecoderFallbackException)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1010",
                $"A native-fragment {kind} string is not strict UTF-8.",
                Valueˉoffset);
        }
    }

    public ReadOnlySpan<byte> Readˉbytes(int length)
    {
        if (length < 0 || Position > Bytes.Length - length)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1002",
                "The native-fragment artifact is truncated.",
                Position);
        }
        var Result = Bytes.Span.Slice(Position, length);
        Position = checked(Position + length);
        return Result;
    }

    public void Requireˉend()
    {
        if (Position != Bytes.Length)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1011",
                "The native-fragment artifact contains trailing bytes.",
                Position);
        }
    }
}

internal sealed class Nativeˉfragmentˉartifactˉwriter
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
    private readonly MemoryStream Stream = new();

    public void Writeˉu8(byte value) => Stream.WriteByte(value);

    public void Writeˉu16(ushort value)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(Buffer, value);
        Stream.Write(Buffer);
    }

    public void Writeˉu32(uint value)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(Buffer, value);
        Stream.Write(Buffer);
    }

    public void Writeˉi32(int value)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(Buffer, value);
        Stream.Write(Buffer);
    }

    public void Writeˉstring(string value)
    {
        var Bytes = STRICT_UTF8.GetBytes(value);
        Writeˉu32(checked((uint)Bytes.Length));
        Stream.Write(Bytes);
    }

    public void Writeˉbytes(ReadOnlySpan<byte> value) => Stream.Write(value);

    public byte[] Toˉarray() => Stream.ToArray();
}
