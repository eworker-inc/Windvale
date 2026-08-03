using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace Windvale.ObjectModel;

public static class Objectˉcodec
{
    private static readonly byte[] MAGIC = "WVO1"u8.ToArray();
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public const ushort MAJOR_VERSION = 1;
    public const ushort MINOR_VERSION = 0;

    public static byte[] Write(
        Objectˉfile value,
        Objectˉadmissionˉprofile admissionˉprofile = Objectˉadmissionˉprofile.Standard)
    {
        var Maximumˉobjectˉbytes = Objectˉlimits.Maximumˉobjectˉbytes(admissionˉprofile);
        var Verified = Objectˉverifier.Verify(value, admissionˉprofile);
        using var Stream = new MemoryStream();
        var Writer = new Byteˉwriter(Stream);
        Writer.Writeˉbytes(MAGIC);
        Writer.Writeˉu16(MAJOR_VERSION);
        Writer.Writeˉu16(MINOR_VERSION);
        Writer.Writeˉu8((byte)Verified.Value.Architecture);
        Writer.Writeˉu8(0);
        Writer.Writeˉu16(0);
        Writer.Writeˉu32((uint)Verified.Value.Sections.Length);
        Writer.Writeˉu32((uint)Verified.Value.Symbols.Length);
        Writer.Writeˉu32((uint)Verified.Value.Relocations.Length);

        foreach (var Section in Verified.Value.Sections)
        {
            Writer.Writeˉu8((byte)Section.Kind);
            Writer.Writeˉu8(0);
            Writer.Writeˉu16(0);
            Writer.Writeˉu32(Section.Alignment);
            Writer.Writeˉu32(Section.Memoryˉsize);
            Writer.Writeˉu32((uint)Section.Data.Length);
            Writer.Writeˉstring(Section.Name);
            Writer.Writeˉbytes(Section.Data.AsSpan());
        }

        foreach (var Symbol in Verified.Value.Symbols)
        {
            Writer.Writeˉu8((byte)Symbol.Binding);
            Writer.Writeˉu8((byte)Symbol.Kind);
            Writer.Writeˉu16(0);
            Writer.Writeˉu32(Symbol.Sectionˉindex);
            Writer.Writeˉu32(Symbol.Offset);
            Writer.Writeˉu32(Symbol.Size);
            Writer.Writeˉstring(Symbol.Name);
        }

        foreach (var Relocation in Verified.Value.Relocations)
        {
            Writer.Writeˉu8((byte)Relocation.Kind);
            Writer.Writeˉu8(0);
            Writer.Writeˉu16(0);
            Writer.Writeˉu32(Relocation.Sectionˉindex);
            Writer.Writeˉu32(Relocation.Offset);
            Writer.Writeˉu32(Relocation.Symbolˉindex);
            Writer.Writeˉi32(Relocation.Addend);
        }

        var Result = Stream.ToArray();
        if (Result.Length > Maximumˉobjectˉbytes)
        {
            throw new Objectˉverificationˉexception("WVO2006", "The encoded object exceeds the object-size limit.");
        }
        return Result;
    }

    public static Objectˉfile Read(
        ReadOnlySpan<byte> bytes,
        Objectˉadmissionˉprofile admissionˉprofile = Objectˉadmissionˉprofile.Standard)
    {
        var Maximumˉobjectˉbytes = Objectˉlimits.Maximumˉobjectˉbytes(admissionˉprofile);
        if (bytes.Length > Maximumˉobjectˉbytes)
        {
            throw new Objectˉformatˉexception("WVO1001", "The object exceeds the object-size limit.");
        }

        var Reader = new Byteˉreader(bytes);
        if (!Reader.Readˉbytes(MAGIC.Length).SequenceEqual(MAGIC))
        {
            throw new Objectˉformatˉexception("WVO1002", "The object magic is invalid.", 0);
        }
        var Major = Reader.Readˉu16();
        var Minor = Reader.Readˉu16();
        if (Major != MAJOR_VERSION || Minor != MINOR_VERSION)
        {
            throw new Objectˉformatˉexception(
                "WVO1003",
                $"Unsupported object version {Major}.{Minor}.",
                MAGIC.Length);
        }
        var Architectureˉoffset = Reader.Absoluteˉoffset;
        var Rawˉarchitecture = Reader.Readˉu8();
        if (Rawˉarchitecture != (byte)Objectˉarchitecture.X86ˉ64)
        {
            throw new Objectˉformatˉexception("WVO1004", "The object architecture is unknown.", Architectureˉoffset);
        }
        var Headerˉflags = Reader.Readˉu8();
        var Headerˉreserved = Reader.Readˉu16();
        if (Headerˉflags != 0 || Headerˉreserved != 0)
        {
            throw new Objectˉformatˉexception("WVO1005", "The object header uses unsupported flags.", Architectureˉoffset + 1);
        }

        var Sectionˉcount = Reader.Readˉboundedˉcount(Objectˉlimits.MAX_SECTIONS, "section");
        var Symbolˉcount = Reader.Readˉboundedˉcount(Objectˉlimits.MAX_SYMBOLS, "symbol");
        var Relocationˉcount = Reader.Readˉboundedˉcount(Objectˉlimits.MAX_RELOCATIONS, "relocation");
        if (Sectionˉcount == 0)
        {
            throw new Objectˉformatˉexception("WVO1006", "The object has no sections.", 12);
        }

        var Sections = ImmutableArray.CreateBuilder<Objectˉsection>(Sectionˉcount);
        for (var Index = 0; Index < Sectionˉcount; Index++)
        {
            var Kindˉoffset = Reader.Absoluteˉoffset;
            var Rawˉkind = Reader.Readˉu8();
            if (Rawˉkind is < 1 or > 4)
            {
                throw new Objectˉformatˉexception("WVO1007", "A section kind is unknown.", Kindˉoffset);
            }
            var Flags = Reader.Readˉu8();
            var Reserved = Reader.Readˉu16();
            if (Flags != 0 || Reserved != 0)
            {
                throw new Objectˉformatˉexception("WVO1008", "A section uses unsupported flags.", Kindˉoffset + 1);
            }
            var Alignment = Reader.Readˉu32();
            var Memoryˉsize = Reader.Readˉu32();
            var Dataˉlength = Reader.Readˉboundedˉcount(Maximumˉobjectˉbytes, "section data byte");
            var Name = Reader.Readˉstring();
            var Data = ImmutableArray.Create(Reader.Readˉbytes(Dataˉlength).ToArray());
            Sections.Add(new(Name, (Objectˉsectionˉkind)Rawˉkind, Alignment, Memoryˉsize, Data));
        }

        var Symbols = ImmutableArray.CreateBuilder<Objectˉsymbol>(Symbolˉcount);
        for (var Index = 0; Index < Symbolˉcount; Index++)
        {
            var Bindingˉoffset = Reader.Absoluteˉoffset;
            var Rawˉbinding = Reader.Readˉu8();
            var Rawˉkind = Reader.Readˉu8();
            var Reserved = Reader.Readˉu16();
            if (Rawˉbinding is < 1 or > 3 || Rawˉkind is < 1 or > 2)
            {
                throw new Objectˉformatˉexception("WVO1009", "A symbol binding or kind is unknown.", Bindingˉoffset);
            }
            if (Reserved != 0)
            {
                throw new Objectˉformatˉexception("WVO1010", "A symbol uses reserved bits.", Bindingˉoffset + 2);
            }
            var Sectionˉindex = Reader.Readˉu32();
            var Offset = Reader.Readˉu32();
            var Size = Reader.Readˉu32();
            var Name = Reader.Readˉstring();
            Symbols.Add(new(
                Name,
                (Objectˉsymbolˉbinding)Rawˉbinding,
                (Objectˉsymbolˉkind)Rawˉkind,
                Sectionˉindex,
                Offset,
                Size));
        }

        var Relocations = ImmutableArray.CreateBuilder<Objectˉrelocation>(Relocationˉcount);
        for (var Index = 0; Index < Relocationˉcount; Index++)
        {
            var Kindˉoffset = Reader.Absoluteˉoffset;
            var Rawˉkind = Reader.Readˉu8();
            var Flags = Reader.Readˉu8();
            var Reserved = Reader.Readˉu16();
            if (Rawˉkind is < 1 or > 2)
            {
                throw new Objectˉformatˉexception("WVO1011", "A relocation kind is unknown.", Kindˉoffset);
            }
            if (Flags != 0 || Reserved != 0)
            {
                throw new Objectˉformatˉexception("WVO1012", "A relocation uses unsupported flags.", Kindˉoffset + 1);
            }
            Relocations.Add(new(
                (Objectˉrelocationˉkind)Rawˉkind,
                Reader.Readˉu32(),
                Reader.Readˉu32(),
                Reader.Readˉu32(),
                Reader.Readˉi32()));
        }

        Reader.Requireˉend();
        return new(
            (Objectˉarchitecture)Rawˉarchitecture,
            Sections.ToImmutable(),
            Symbols.ToImmutable(),
            Relocations.ToImmutable());
    }

    public static Verifiedˉobject Readˉandˉverify(
        ReadOnlySpan<byte> bytes,
        Objectˉadmissionˉprofile admissionˉprofile = Objectˉadmissionˉprofile.Standard) =>
        Objectˉverifier.Verify(Read(bytes, admissionˉprofile), admissionˉprofile);

    private sealed class Byteˉwriter(Stream stream)
    {
        public void Writeˉu8(byte value) => stream.WriteByte(value);

        public void Writeˉu16(ushort value)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(Buffer, value);
            stream.Write(Buffer);
        }

        public void Writeˉu32(uint value)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Buffer, value);
            stream.Write(Buffer);
        }

        public void Writeˉi32(int value)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(Buffer, value);
            stream.Write(Buffer);
        }

        public void Writeˉbytes(ReadOnlySpan<byte> value) => stream.Write(value);

        public void Writeˉstring(string value)
        {
            var Bytes = STRICT_UTF8.GetBytes(value);
            Writeˉu32((uint)Bytes.Length);
            Writeˉbytes(Bytes);
        }
    }

    private ref struct Byteˉreader
    {
        private readonly ReadOnlySpan<byte> Bytes;
        private int Position;

        public Byteˉreader(ReadOnlySpan<byte> bytes)
        {
            Bytes = bytes;
            Position = 0;
        }

        public readonly int Absoluteˉoffset => Position;

        public byte Readˉu8()
        {
            Requireˉbytes(sizeof(byte));
            return Bytes[Position++];
        }

        public ushort Readˉu16()
        {
            Requireˉbytes(sizeof(ushort));
            var Result = BinaryPrimitives.ReadUInt16LittleEndian(Bytes[Position..]);
            Position += sizeof(ushort);
            return Result;
        }

        public uint Readˉu32()
        {
            Requireˉbytes(sizeof(uint));
            var Result = BinaryPrimitives.ReadUInt32LittleEndian(Bytes[Position..]);
            Position += sizeof(uint);
            return Result;
        }

        public int Readˉi32()
        {
            Requireˉbytes(sizeof(int));
            var Result = BinaryPrimitives.ReadInt32LittleEndian(Bytes[Position..]);
            Position += sizeof(int);
            return Result;
        }

        public int Readˉboundedˉcount(int maximum, string itemˉname)
        {
            var Offset = Position;
            var Value = Readˉu32();
            if (Value > maximum)
            {
                throw new Objectˉformatˉexception(
                    "WVO1013",
                    $"The {itemˉname} count {Value} exceeds the limit {maximum}.",
                    Offset);
            }
            return (int)Value;
        }

        public string Readˉstring()
        {
            var Lengthˉoffset = Position;
            var Length = Readˉboundedˉcount(Objectˉlimits.MAX_NAME_BYTES, "name byte");
            var Valueˉoffset = Position;
            var Valueˉbytes = Readˉbytes(Length);
            try
            {
                return STRICT_UTF8.GetString(Valueˉbytes);
            }
            catch (DecoderFallbackException)
            {
                throw new Objectˉformatˉexception("WVO1014", "An object name is not strict UTF-8.", Valueˉoffset);
            }
        }

        public ReadOnlySpan<byte> Readˉbytes(int count)
        {
            Requireˉbytes(count);
            var Result = Bytes.Slice(Position, count);
            Position += count;
            return Result;
        }

        public readonly void Requireˉend()
        {
            if (Position != Bytes.Length)
            {
                throw new Objectˉformatˉexception("WVO1015", "The object contains trailing bytes.", Position);
            }
        }

        private readonly void Requireˉbytes(int count)
        {
            if (count < 0 || count > Bytes.Length - Position)
            {
                throw new Objectˉformatˉexception("WVO1016", "The object is truncated.", Position);
            }
        }
    }
}
