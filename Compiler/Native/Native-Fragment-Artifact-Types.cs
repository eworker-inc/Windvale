using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

internal static class Nativeˉfragmentˉartifactˉtypes
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static Nominalˉtypeˉdeclaration Read(
        Nativeˉfragmentˉartifactˉreader reader)
    {
        var Offset = reader.Offset;
        var Rawˉkind = reader.Readˉu8();
        var Flags = reader.Readˉu8();
        var Reserved = reader.Readˉu16();
        if (Rawˉkind is < 1 or > 2 || Flags != 0 || Reserved != 0)
        {
            throw Invalidˉrecord("A native-fragment artifact nominal type is invalid.", Offset);
        }
        var Name = reader.Readˉstring(Bytecodeˉlimits.MAX_NAME_BYTES, "type-name");
        if (Rawˉkind == (byte)Nominalˉtypeˉkind.Record)
        {
            var Count = reader.Readˉcount(Bytecodeˉlimits.MAX_RECORD_FIELDS, "record-field");
            var Fields = ImmutableArray.CreateBuilder<Recordˉfieldˉdeclaration>(Count);
            for (var Index = 0; Index < Count; Index++)
            {
                Fields.Add(new(
                    reader.Readˉstring(Bytecodeˉlimits.MAX_NAME_BYTES, "field-name"),
                    Readˉshape(reader)));
            }
            return new Recordˉtypeˉdeclaration(Name, Fields.MoveToImmutable());
        }

        var Memberˉcount = reader.Readˉcount(Bytecodeˉlimits.MAX_ENUM_MEMBERS, "enum-member");
        var Members = ImmutableArray.CreateBuilder<Enumˉmemberˉdeclaration>(Memberˉcount);
        for (var Index = 0; Index < Memberˉcount; Index++)
        {
            Members.Add(new(
                reader.Readˉstring(Bytecodeˉlimits.MAX_NAME_BYTES, "member-name"),
                reader.Readˉi32()));
        }
        return new Enumˉtypeˉdeclaration(Name, Members.MoveToImmutable());
    }

    public static void Write(
        Nativeˉfragmentˉartifactˉwriter writer,
        Nominalˉtypeˉdeclaration type)
    {
        writer.Writeˉu8((byte)type.Kind);
        writer.Writeˉu8(0);
        writer.Writeˉu16(0);
        writer.Writeˉstring(type.Name);
        switch (type)
        {
            case Recordˉtypeˉdeclaration Record:
                writer.Writeˉu32(checked((uint)Record.Fields.Length));
                foreach (var Field in Record.Fields)
                {
                    writer.Writeˉstring(Field.Name);
                    Writeˉshape(writer, Field.Type);
                }
                break;
            case Enumˉtypeˉdeclaration Enum:
                writer.Writeˉu32(checked((uint)Enum.Members.Length));
                foreach (var Member in Enum.Members)
                {
                    writer.Writeˉstring(Member.Name);
                    writer.Writeˉi32(Member.Value);
                }
                break;
            default:
                throw new InvalidOperationException("Verified native nominal metadata became invalid.");
        }
    }

    public static void Verifyˉserializableˉmetadata(Nativeˉfragment fragment)
    {
        if (fragment.Types.Length > Bytecodeˉlimits.MAX_NOMINAL_TYPES)
        {
            throw Invalidˉmetadata("The native fragment exceeds the nominal-type limit.");
        }
        Requireˉboundedˉstring(fragment.Target, Objectˉlimits.MAX_NAME_BYTES, "target");
        foreach (var Symbol in fragment.Symbols)
        {
            Requireˉboundedˉstring(Symbol.Name, Objectˉlimits.MAX_NAME_BYTES, "symbol name");
        }
        foreach (var Type in fragment.Types)
        {
            Requireˉboundedˉstring(Type.Name, Bytecodeˉlimits.MAX_NAME_BYTES, "type name");
            switch (Type)
            {
                case Recordˉtypeˉdeclaration Record
                    when Record.Fields.Length <= Bytecodeˉlimits.MAX_RECORD_FIELDS:
                    foreach (var Field in Record.Fields)
                    {
                        Requireˉboundedˉstring(
                            Field.Name,
                            Bytecodeˉlimits.MAX_NAME_BYTES,
                            "field name");
                        if (!Isˉcanonicalˉfieldˉshape(Field.Type))
                        {
                            throw Invalidˉmetadata(
                                "A native-fragment record field has a noncanonical value shape.");
                        }
                    }
                    break;
                case Enumˉtypeˉdeclaration Enum
                    when Enum.Members.Length <= Bytecodeˉlimits.MAX_ENUM_MEMBERS:
                    foreach (var Member in Enum.Members)
                    {
                        Requireˉboundedˉstring(
                            Member.Name,
                            Bytecodeˉlimits.MAX_NAME_BYTES,
                            "member name");
                    }
                    break;
                default:
                    throw Invalidˉmetadata(
                        "Native nominal metadata exceeds an artifact item-count limit.");
            }
        }
    }

    private static Valueˉshape Readˉshape(Nativeˉfragmentˉartifactˉreader reader)
    {
        var Offset = reader.Offset;
        var Rawˉkind = reader.Readˉu8();
        var Rawˉelementˉkind = reader.Readˉu8();
        var Reserved = reader.Readˉu16();
        var Nominalˉtypeˉindex = reader.Readˉi32();
        var Elementˉnominalˉtypeˉindex = reader.Readˉi32();
        var Maximum = reader.Readˉu32();
        var Shape = new Valueˉshape(
            (Valueˉtype)Rawˉkind,
            Nominalˉtypeˉindex,
            (Valueˉtype)Rawˉelementˉkind,
            Elementˉnominalˉtypeˉindex,
            Maximum);
        if (Rawˉkind > (byte)Valueˉtype.Builder ||
            Rawˉelementˉkind > (byte)Valueˉtype.Builder ||
            Reserved != 0 ||
            !Isˉcanonicalˉfieldˉshape(Shape))
        {
            throw Invalidˉrecord("A native-fragment artifact value shape is invalid.", Offset);
        }
        return Shape;
    }

    private static void Writeˉshape(
        Nativeˉfragmentˉartifactˉwriter writer,
        Valueˉshape shape)
    {
        writer.Writeˉu8((byte)shape.Kind);
        writer.Writeˉu8((byte)shape.Elementˉkind);
        writer.Writeˉu16(0);
        writer.Writeˉi32(shape.Nominalˉtypeˉindex);
        writer.Writeˉi32(shape.Elementˉnominalˉtypeˉindex);
        writer.Writeˉu32(shape.Maximum);
    }

    private static void Requireˉboundedˉstring(
        string value,
        int maximumˉbytes,
        string kind)
    {
        try
        {
            var Byteˉcount = STRICT_UTF8.GetByteCount(value);
            if (Byteˉcount is < 1 || Byteˉcount > maximumˉbytes)
            {
                throw Invalidˉmetadata($"A native-fragment {kind} has an invalid length.");
            }
        }
        catch (EncoderFallbackException)
        {
            throw Invalidˉmetadata($"A native-fragment {kind} is not strict UTF-8.");
        }
    }

    private static bool Isˉcanonicalˉfieldˉshape(Valueˉshape shape) =>
        shape.Elementˉkind == Valueˉtype.Void &&
        shape.Elementˉnominalˉtypeˉindex == -1 &&
        shape.Maximum == 0 &&
        (shape.Kind switch
        {
            Valueˉtype.I32 or
            Valueˉtype.Bool or
            Valueˉtype.Text or
            Valueˉtype.U8 or
            Valueˉtype.U32 or
            Valueˉtype.Bytes => shape.Nominalˉtypeˉindex == -1,
            Valueˉtype.Enum => shape.Nominalˉtypeˉindex >= 0,
            _ => false,
        });

    private static Nativeˉfragmentˉartifactˉexception Invalidˉrecord(
        string message,
        int offset) => new("WNF1009", message, offset);

    private static Nativeˉfragmentˉartifactˉexception Invalidˉmetadata(string message) =>
        new("WNF2002", message);
}
