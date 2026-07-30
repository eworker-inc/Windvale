using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;

namespace Windvale.Bytecode;

public static class Moduleˉcodec
{
    private static readonly byte[] MAGIC = "WVB1"u8.ToArray();
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public const ushort MAJOR_VERSION = 1;
    public const ushort MINOR_VERSION = 5;

    public static byte[] Write(Bytecodeˉmodule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        Moduleˉverifier.Verify(module);

        var Moduleˉpayload = Buildˉpayload(Writer =>
        {
            Writer.Writeˉbyte((byte)module.Profile);
            Writer.Writeˉstring(module.Name, isˉname: true);
        });

        var Capabilityˉpayload = Buildˉpayload(Writer =>
        {
            Writer.Writeˉu32(module.Capabilities.Length);
            foreach (var Capability in module.Capabilities)
            {
                Writer.Writeˉstring(Capability.Name, isˉname: true);
                Writer.Writeˉu32(Capability.Parameterˉtypes.Length);
                foreach (var Parameterˉtype in Capability.Parameterˉtypes)
                {
                    Writer.Writeˉbyte((byte)Parameterˉtype);
                }

                Writer.Writeˉbyte((byte)Capability.Returnˉtype);
            }
        });

        var Dataˉpayload = Buildˉpayload(Writer =>
        {
            Writer.Writeˉu32(module.Data.Length);
            foreach (var Data in module.Data)
            {
                Writer.Writeˉstring(Data.Name, isˉname: true);
                Writer.Writeˉbyte((byte)Data.Type);
                switch (Data)
                {
                    case Textˉdataˉdeclaration Text:
                        Writer.Writeˉstring(Text.Value, isˉname: false);
                        break;
                    case I32ˉarrayˉdataˉdeclaration Array:
                        Writer.Writeˉu32(Array.Values.Length);
                        foreach (var Value in Array.Values)
                        {
                            Writer.Writeˉi32(Value);
                        }

                        break;
                    case Bytesˉdataˉdeclaration Bytes:
                        Writer.Writeˉu32(Bytes.Values.Length);
                        Writer.Writeˉbytes(Bytes.Values.AsSpan());
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown data declaration type '{Data.GetType().Name}'.");
                }
            }
        });

        var Functionˉpayload = Buildˉpayload(Writer =>
        {
            Writer.Writeˉu32(module.Functions.Length);
            foreach (var Function in module.Functions)
            {
                Writer.Writeˉstring(Function.Name, isˉname: true);
                Writer.Writeˉu32(Function.Parameterˉtypes.Length);
                foreach (var Parameterˉtype in Function.Parameterˉtypes)
                {
                    Writer.Writeˉvalueˉshape(Parameterˉtype);
                }

                Writer.Writeˉvalueˉshape(Function.Returnˉtype);
                Writer.Writeˉu32(Function.Localˉtypes.Length);
                foreach (var Localˉtype in Function.Localˉtypes)
                {
                    Writer.Writeˉvalueˉshape(Localˉtype);
                }

                Writer.Writeˉu32(Function.Codeˉoffset);
                Writer.Writeˉu32(Function.Codeˉlength);
                Writer.Writeˉu32(Function.Maximumˉstackˉdepth);
            }
        });

        var Codeˉpayload = module.Code.ToArray();

        var Exportˉpayload = Buildˉpayload(Writer =>
        {
            Writer.Writeˉu32(module.Exports.Length);
            foreach (var Export in module.Exports)
            {
                Writer.Writeˉstring(Export.Name, isˉname: true);
                Writer.Writeˉbyte((byte)Export.Kind);
                Writer.Writeˉu32(Export.Targetˉindex);
            }
        });

        var Typeˉpayload = Buildˉpayload(Writer =>
        {
            Writer.Writeˉu32(module.Types.Length);
            foreach (var Type in module.Types)
            {
                Writer.Writeˉbyte((byte)Type.Kind);
                Writer.Writeˉstring(Type.Name, isˉname: true);
                switch (Type)
                {
                    case Recordˉtypeˉdeclaration Record:
                        Writer.Writeˉu32(Record.Fields.Length);
                        foreach (var Field in Record.Fields)
                        {
                            Writer.Writeˉstring(Field.Name, isˉname: true);
                            Writer.Writeˉvalueˉshape(Field.Type);
                        }

                        break;
                    case Enumˉtypeˉdeclaration Enum:
                        Writer.Writeˉu32(Enum.Members.Length);
                        foreach (var Member in Enum.Members)
                        {
                            Writer.Writeˉstring(Member.Name, isˉname: true);
                            Writer.Writeˉi32(Member.Value);
                        }

                        break;
                    default:
                        throw new InvalidOperationException($"Unknown nominal type '{Type.GetType().Name}'.");
                }
            }
        });

        using var Stream = new MemoryStream();
        var Rootˉwriter = new Byteˉwriter(Stream);
        Rootˉwriter.Writeˉbytes(MAGIC);
        Rootˉwriter.Writeˉu16(MAJOR_VERSION);
        Rootˉwriter.Writeˉu16(MINOR_VERSION);
        Rootˉwriter.Writeˉu32(Bytecodeˉlimits.SECTION_COUNT);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Module, Moduleˉpayload);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Capabilities, Capabilityˉpayload);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Data, Dataˉpayload);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Functions, Functionˉpayload);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Code, Codeˉpayload);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Exports, Exportˉpayload);
        Writeˉsection(Rootˉwriter, Sectionˉkind.Types, Typeˉpayload);

        var Result = Stream.ToArray();
        if (Result.Length > Bytecodeˉlimits.MAX_MODULE_BYTES)
        {
            throw new Moduleˉverificationˉexception(
                "WVB1001",
                "The serialized module exceeds the module-size limit.");
        }

        return Result;
    }

    public static Bytecodeˉmodule Read(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > Bytecodeˉlimits.MAX_MODULE_BYTES)
        {
            throw new Moduleˉformatˉexception("WVB1001", "The module exceeds the module-size limit.");
        }

        var Reader = new Byteˉreader(bytes, 0);
        var Magic = Reader.Readˉbytes(MAGIC.Length);
        if (!Magic.SequenceEqual(MAGIC))
        {
            throw new Moduleˉformatˉexception("WVB1002", "The module magic is invalid.", 0);
        }

        var Majorˉversion = Reader.Readˉu16();
        var Minorˉversion = Reader.Readˉu16();
        if (Majorˉversion != MAJOR_VERSION || Minorˉversion != MINOR_VERSION)
        {
            throw new Moduleˉformatˉexception(
                "WVB1003",
                $"Unsupported module version {Majorˉversion}.{Minorˉversion}.",
                MAGIC.Length);
        }

        var Sectionˉcount = Reader.Readˉu32();
        if (Sectionˉcount != Bytecodeˉlimits.SECTION_COUNT)
        {
            throw new Moduleˉformatˉexception(
                "WVB1004",
                $"Seed requires exactly {Bytecodeˉlimits.SECTION_COUNT} sections.",
                MAGIC.Length + sizeof(ushort) + sizeof(ushort));
        }

        var Moduleˉreader = Readˉsection(ref Reader, Sectionˉkind.Module);
        var Profile = Readˉprofile(ref Moduleˉreader);
        var Moduleˉname = Moduleˉreader.Readˉstring(isˉname: true);
        Moduleˉreader.Requireˉend("Module");

        var Capabilityˉreader = Readˉsection(ref Reader, Sectionˉkind.Capabilities);
        var Capabilities = Readˉcapabilities(ref Capabilityˉreader);
        Capabilityˉreader.Requireˉend("Capabilities");

        var Dataˉreader = Readˉsection(ref Reader, Sectionˉkind.Data);
        var Data = Readˉdata(ref Dataˉreader);
        Dataˉreader.Requireˉend("Data");

        var Functionˉreader = Readˉsection(ref Reader, Sectionˉkind.Functions);
        var Functions = Readˉfunctions(ref Functionˉreader);
        Functionˉreader.Requireˉend("Functions");

        var Codeˉreader = Readˉsection(ref Reader, Sectionˉkind.Code);
        var Code = Codeˉreader.Readˉremaining().ToArray().ToImmutableArray();

        var Exportˉreader = Readˉsection(ref Reader, Sectionˉkind.Exports);
        var Exports = Readˉexports(ref Exportˉreader);
        Exportˉreader.Requireˉend("Exports");

        var Typeˉreader = Readˉsection(ref Reader, Sectionˉkind.Types);
        var Types = Readˉtypes(ref Typeˉreader);
        Typeˉreader.Requireˉend("Types");

        Reader.Requireˉend("module file");

        return new(
            Moduleˉname,
            Profile,
            Capabilities,
            Data,
            Functions,
            Code,
            Exports)
        {
            Types = Types,
        };
    }

    public static Verifiedˉmodule Readˉandˉverify(ReadOnlySpan<byte> bytes)
    {
        return Moduleˉverifier.Verify(Read(bytes));
    }

    private static ImmutableArray<Capabilityˉdeclaration> Readˉcapabilities(ref Byteˉreader reader)
    {
        var Count = reader.Readˉboundedˉcount(
            Bytecodeˉlimits.MAX_CAPABILITIES,
            "capability");
        var Result = ImmutableArray.CreateBuilder<Capabilityˉdeclaration>(Count);
        for (var Index = 0; Index < Count; Index++)
        {
            var Name = reader.Readˉstring(isˉname: true);
            var Parameterˉcount = reader.Readˉboundedˉcount(
                Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS,
                "capability parameter");
            var Parameterˉtypes = ImmutableArray.CreateBuilder<Valueˉtype>(Parameterˉcount);
            for (var Parameterˉindex = 0; Parameterˉindex < Parameterˉcount; Parameterˉindex++)
            {
                Parameterˉtypes.Add(reader.Readˉvalueˉtype(allowˉvoid: false));
            }

            var Returnˉtype = reader.Readˉvalueˉtype(allowˉvoid: true);
            Result.Add(new(Name, Parameterˉtypes.ToImmutable(), Returnˉtype));
        }

        return Result.ToImmutable();
    }

    private static ImmutableArray<Dataˉdeclaration> Readˉdata(ref Byteˉreader reader)
    {
        var Count = reader.Readˉboundedˉcount(
            Bytecodeˉlimits.MAX_DATA_DECLARATIONS,
            "data declaration");
        var Result = ImmutableArray.CreateBuilder<Dataˉdeclaration>(Count);
        for (var Index = 0; Index < Count; Index++)
        {
            var Name = reader.Readˉstring(isˉname: true);
            var Rawˉtype = reader.Readˉbyte();
            switch ((Dataˉtype)Rawˉtype)
            {
                case Dataˉtype.Text:
                    Result.Add(new Textˉdataˉdeclaration(
                        Name,
                        reader.Readˉstring(isˉname: false)));
                    break;
                case Dataˉtype.I32ˉarray:
                    var Elementˉcount = reader.Readˉboundedˉcount(
                        Bytecodeˉlimits.MAX_I32_ARRAY_ELEMENTS,
                        "i32 data element");
                    var Values = ImmutableArray.CreateBuilder<int>(Elementˉcount);
                    for (var Elementˉindex = 0; Elementˉindex < Elementˉcount; Elementˉindex++)
                    {
                        Values.Add(reader.Readˉi32());
                    }

                    Result.Add(new I32ˉarrayˉdataˉdeclaration(Name, Values.ToImmutable()));
                    break;
                case Dataˉtype.Bytes:
                    var Byteˉcount = reader.Readˉboundedˉcount(
                        Bytecodeˉlimits.MAX_BYTE_DATA_BYTES,
                        "byte data");
                    Result.Add(new Bytesˉdataˉdeclaration(
                        Name,
                        reader.Readˉbytes(Byteˉcount).ToArray().ToImmutableArray()));
                    break;
                default:
                    throw new Moduleˉformatˉexception(
                        "WVB1005",
                        $"Unknown data type {Rawˉtype}.",
                        reader.Absoluteˉoffset - 1);
            }
        }

        return Result.ToImmutable();
    }

    private static ImmutableArray<Functionˉdeclaration> Readˉfunctions(ref Byteˉreader reader)
    {
        var Count = reader.Readˉboundedˉcount(Bytecodeˉlimits.MAX_FUNCTIONS, "function");
        var Result = ImmutableArray.CreateBuilder<Functionˉdeclaration>(Count);
        for (var Index = 0; Index < Count; Index++)
        {
            var Name = reader.Readˉstring(isˉname: true);
            var Parameterˉcount = reader.Readˉboundedˉcount(
                Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS,
                "function parameter");
            var Parameterˉtypes = ImmutableArray.CreateBuilder<Valueˉshape>(Parameterˉcount);
            for (var Parameterˉindex = 0; Parameterˉindex < Parameterˉcount; Parameterˉindex++)
            {
                Parameterˉtypes.Add(reader.Readˉvalueˉshape(allowˉvoid: false));
            }

            var Returnˉtype = reader.Readˉvalueˉshape(allowˉvoid: true);
            var Localˉcount = reader.Readˉboundedˉcount(
                Bytecodeˉlimits.MAX_PARAMETERS_OR_LOCALS,
                "function local");
            var Localˉtypes = ImmutableArray.CreateBuilder<Valueˉshape>(Localˉcount);
            for (var Localˉindex = 0; Localˉindex < Localˉcount; Localˉindex++)
            {
                Localˉtypes.Add(reader.Readˉvalueˉshape(allowˉvoid: false));
            }

            var Codeˉoffset = reader.Readˉnonnegativeˉi32("function code offset");
            var Codeˉlength = reader.Readˉnonnegativeˉi32("function code length");
            var Maximumˉstack = reader.Readˉnonnegativeˉi32("function maximum stack");
            Result.Add(new(
                Name,
                Parameterˉtypes.ToImmutable(),
                Returnˉtype,
                Localˉtypes.ToImmutable(),
                Codeˉoffset,
                Codeˉlength,
                Maximumˉstack));
        }

        return Result.ToImmutable();
    }

    private static ImmutableArray<Exportˉdeclaration> Readˉexports(ref Byteˉreader reader)
    {
        var Count = reader.Readˉboundedˉcount(Bytecodeˉlimits.MAX_FUNCTIONS, "export");
        var Result = ImmutableArray.CreateBuilder<Exportˉdeclaration>(Count);
        for (var Index = 0; Index < Count; Index++)
        {
            var Name = reader.Readˉstring(isˉname: true);
            var Rawˉkind = reader.Readˉbyte();
            if (Rawˉkind != (byte)Exportˉkind.Function)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1006",
                    $"Unknown export kind {Rawˉkind}.",
                    reader.Absoluteˉoffset - 1);
            }

            var Targetˉindex = reader.Readˉnonnegativeˉi32("export target index");
            Result.Add(new(Name, Exportˉkind.Function, Targetˉindex));
        }

        return Result.ToImmutable();
    }

    private static ImmutableArray<Nominalˉtypeˉdeclaration> Readˉtypes(ref Byteˉreader reader)
    {
        var Count = reader.Readˉboundedˉcount(Bytecodeˉlimits.MAX_NOMINAL_TYPES, "nominal type");
        var Result = ImmutableArray.CreateBuilder<Nominalˉtypeˉdeclaration>(Count);
        for (var Index = 0; Index < Count; Index++)
        {
            var Kindˉoffset = reader.Absoluteˉoffset;
            var Rawˉkind = reader.Readˉbyte();
            if (!Enum.IsDefined(typeof(Nominalˉtypeˉkind), Rawˉkind))
            {
                throw new Moduleˉformatˉexception(
                    "WVB1020",
                    $"Unknown nominal type kind {Rawˉkind}.",
                    Kindˉoffset);
            }

            var Kind = (Nominalˉtypeˉkind)Rawˉkind;
            var Name = reader.Readˉstring(isˉname: true);
            if (Kind == Nominalˉtypeˉkind.Record)
            {
                var Fieldˉcount = reader.Readˉboundedˉcount(
                    Bytecodeˉlimits.MAX_RECORD_FIELDS,
                    "record field");
                var Fields = ImmutableArray.CreateBuilder<Recordˉfieldˉdeclaration>(Fieldˉcount);
                for (var Fieldˉindex = 0; Fieldˉindex < Fieldˉcount; Fieldˉindex++)
                {
                    var Fieldˉname = reader.Readˉstring(isˉname: true);
                    var Fieldˉtype = reader.Readˉvalueˉshape(allowˉvoid: false);
                    Fields.Add(new(Fieldˉname, Fieldˉtype));
                }

                Result.Add(new Recordˉtypeˉdeclaration(Name, Fields.ToImmutable()));
                continue;
            }

            var Memberˉcount = reader.Readˉboundedˉcount(
                Bytecodeˉlimits.MAX_ENUM_MEMBERS,
                "enum member");
            var Members = ImmutableArray.CreateBuilder<Enumˉmemberˉdeclaration>(Memberˉcount);
            for (var Memberˉindex = 0; Memberˉindex < Memberˉcount; Memberˉindex++)
            {
                Members.Add(new(reader.Readˉstring(isˉname: true), reader.Readˉi32()));
            }

            Result.Add(new Enumˉtypeˉdeclaration(Name, Members.ToImmutable()));
        }

        return Result.ToImmutable();
    }

    private static Moduleˉprofile Readˉprofile(ref Byteˉreader reader)
    {
        var Rawˉprofile = reader.Readˉbyte();
        if (!Enum.IsDefined(typeof(Moduleˉprofile), Rawˉprofile))
        {
            throw new Moduleˉformatˉexception(
                "WVB1007",
                $"Unknown module profile {Rawˉprofile}.",
                reader.Absoluteˉoffset - 1);
        }

        return (Moduleˉprofile)Rawˉprofile;
    }

    private static Byteˉreader Readˉsection(ref Byteˉreader reader, Sectionˉkind expectedˉkind)
    {
        var Headerˉoffset = reader.Absoluteˉoffset;
        var Rawˉkind = reader.Readˉbyte();
        if (Rawˉkind != (byte)expectedˉkind)
        {
            throw new Moduleˉformatˉexception(
                "WVB1008",
                $"Expected section {(byte)expectedˉkind} ({expectedˉkind}) but found {Rawˉkind}.",
                Headerˉoffset);
        }

        var Flags = reader.Readˉbyte();
        var Reserved = reader.Readˉu16();
        if (Flags != 0 || Reserved != 0)
        {
            throw new Moduleˉformatˉexception(
                "WVB1009",
                $"Section '{expectedˉkind}' uses unsupported flags or reserved bits.",
                Headerˉoffset);
        }

        var Length = reader.Readˉnonnegativeˉi32("section payload length");
        return reader.Readˉslice(Length);
    }

    private static byte[] Buildˉpayload(Action<Byteˉwriter> write)
    {
        using var Stream = new MemoryStream();
        write(new Byteˉwriter(Stream));
        return Stream.ToArray();
    }

    private static void Writeˉsection(Byteˉwriter writer, Sectionˉkind kind, byte[] payload)
    {
        writer.Writeˉbyte((byte)kind);
        writer.Writeˉbyte(0);
        writer.Writeˉu16(0);
        writer.Writeˉu32(payload.Length);
        writer.Writeˉbytes(payload);
    }

    private sealed class Byteˉwriter(Stream stream)
    {
        public void Writeˉbyte(byte value)
        {
            stream.WriteByte(value);
        }

        public void Writeˉbytes(ReadOnlySpan<byte> value)
        {
            stream.Write(value);
        }

        public void Writeˉu16(ushort value)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(Buffer, value);
            stream.Write(Buffer);
        }

        public void Writeˉu32(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            Writeˉu32((uint)value);
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

        public void Writeˉvalueˉshape(Valueˉshape shape)
        {
            Writeˉbyte((byte)shape.Kind);
            if (shape.Kind is Valueˉtype.Record or Valueˉtype.Enum)
            {
                Writeˉu32(shape.Nominalˉtypeˉindex);
            }
        }

        public void Writeˉstring(string value, bool isˉname)
        {
            ArgumentNullException.ThrowIfNull(value);
            var Bytes = STRICT_UTF8.GetBytes(value);
            var Limit = isˉname
                ? Bytecodeˉlimits.MAX_NAME_BYTES
                : Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES;
            if (Bytes.Length > Limit)
            {
                throw new Moduleˉverificationˉexception(
                    "WVB1010",
                    isˉname ? "A declaration name is too long." : "A UTF-8 value is too long.");
            }

            Writeˉu32(Bytes.Length);
            Writeˉbytes(Bytes);
        }
    }

    private ref struct Byteˉreader
    {
        private readonly ReadOnlySpan<byte> Bytes;
        private readonly int Baseˉoffset;
        private int Position;

        public Byteˉreader(ReadOnlySpan<byte> bytes, int baseˉoffset)
        {
            Bytes = bytes;
            Baseˉoffset = baseˉoffset;
            Position = 0;
        }

        public readonly int Absoluteˉoffset => checked(Baseˉoffset + Position);

        public byte Readˉbyte()
        {
            Requireˉbytes(sizeof(byte));
            return Bytes[Position++];
        }

        public ushort Readˉu16()
        {
            Requireˉbytes(sizeof(ushort));
            var Value = BinaryPrimitives.ReadUInt16LittleEndian(Bytes[Position..]);
            Position += sizeof(ushort);
            return Value;
        }

        public uint Readˉu32()
        {
            Requireˉbytes(sizeof(uint));
            var Value = BinaryPrimitives.ReadUInt32LittleEndian(Bytes[Position..]);
            Position += sizeof(uint);
            return Value;
        }

        public int Readˉi32()
        {
            Requireˉbytes(sizeof(int));
            var Value = BinaryPrimitives.ReadInt32LittleEndian(Bytes[Position..]);
            Position += sizeof(int);
            return Value;
        }

        public int Readˉnonnegativeˉi32(string fieldˉname)
        {
            var Fieldˉoffset = Absoluteˉoffset;
            var Value = Readˉu32();
            if (Value > int.MaxValue)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1011",
                    $"The {fieldˉname} exceeds the supported range.",
                    Fieldˉoffset);
            }

            return (int)Value;
        }

        public int Readˉboundedˉcount(int maximum, string itemˉname)
        {
            var Countˉoffset = Absoluteˉoffset;
            var Count = Readˉnonnegativeˉi32($"{itemˉname} count");
            if (Count > maximum)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1012",
                    $"The {itemˉname} count {Count} exceeds the limit {maximum}.",
                    Countˉoffset);
            }

            return Count;
        }

        public Valueˉtype Readˉvalueˉtype(bool allowˉvoid, bool allowˉnominal = false)
        {
            var Typeˉoffset = Absoluteˉoffset;
            var Rawˉtype = Readˉbyte();
            if (!Enum.IsDefined(typeof(Valueˉtype), Rawˉtype))
            {
                throw new Moduleˉformatˉexception(
                    "WVB1013",
                    $"Unknown value type {Rawˉtype}.",
                    Typeˉoffset);
            }

            var Type = (Valueˉtype)Rawˉtype;
            if (!allowˉvoid && Type == Valueˉtype.Void)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1014",
                    "Void is not valid in this type position.",
                    Typeˉoffset);
            }

            if (!allowˉnominal && Type is Valueˉtype.Record or Valueˉtype.Enum)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1019",
                    "A nominal type is not valid in this type position.",
                    Typeˉoffset);
            }

            return Type;
        }

        public Valueˉshape Readˉvalueˉshape(bool allowˉvoid)
        {
            var Type = Readˉvalueˉtype(allowˉvoid, allowˉnominal: true);
            var Nominalˉindex = Type is Valueˉtype.Record or Valueˉtype.Enum
                ? Readˉnonnegativeˉi32("nominal type index")
                : -1;
            return new(Type, Nominalˉindex);
        }

        public string Readˉstring(bool isˉname)
        {
            var Lengthˉoffset = Absoluteˉoffset;
            var Length = Readˉnonnegativeˉi32("UTF-8 byte length");
            var Limit = isˉname
                ? Bytecodeˉlimits.MAX_NAME_BYTES
                : Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES;
            if (Length > Limit)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1015",
                    isˉname ? "A declaration name is too long." : "A UTF-8 value is too long.",
                    Lengthˉoffset);
            }

            var Valueˉoffset = Absoluteˉoffset;
            var Valueˉbytes = Readˉbytes(Length);
            try
            {
                return STRICT_UTF8.GetString(Valueˉbytes);
            }
            catch (DecoderFallbackException Exception)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1016",
                    $"A string is not valid UTF-8: {Exception.Message}",
                    Valueˉoffset);
            }
        }

        public ReadOnlySpan<byte> Readˉbytes(int length)
        {
            Requireˉbytes(length);
            var Result = Bytes.Slice(Position, length);
            Position += length;
            return Result;
        }

        public ReadOnlySpan<byte> Readˉremaining()
        {
            return Readˉbytes(Bytes.Length - Position);
        }

        public Byteˉreader Readˉslice(int length)
        {
            var Sliceˉoffset = Absoluteˉoffset;
            return new(Readˉbytes(length), Sliceˉoffset);
        }

        public readonly void Requireˉend(string scope)
        {
            if (Position != Bytes.Length)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1017",
                    $"The {scope} contains {Bytes.Length - Position} trailing bytes.",
                    Absoluteˉoffset);
            }
        }

        private readonly void Requireˉbytes(int count)
        {
            if (count < 0 || count > Bytes.Length - Position)
            {
                throw new Moduleˉformatˉexception(
                    "WVB1018",
                    $"The module is truncated; {count} more bytes were required.",
                    Absoluteˉoffset);
            }
        }
    }
}
