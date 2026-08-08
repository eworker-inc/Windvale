using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;
using Windvale.ObjectModel;

namespace Windvale.Compiler.Native;

public static class Nativeˉfragmentˉartifactˉcontract
{
    public const ushort MAJOR_VERSION = 1;
    public const ushort MINOR_VERSION = 0;
    public const int HEADER_BYTES = 48;
    public const int MAXIMUM_ARTIFACT_BYTES = 64 * 1024 * 1024;
}

public sealed class Nativeˉfragmentˉartifactˉexception(
    string code,
    string message,
    int? offset = null) : Exception(message)
{
    public string Code { get; } = code;

    public int? Offset { get; } = offset;
}

public static class Nativeˉfragmentˉartifactˉcodec
{
    private static readonly byte[] MAGIC = "WVNF"u8.ToArray();
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    public static byte[] Write(Nativeˉfragment fragment)
    {
        _ = Nativeˉfragmentˉverifier.Verify(fragment);
        Nativeˉfragmentˉartifactˉtypes.Verifyˉserializableˉmetadata(fragment);

        var Symbolˉindices = fragment.Symbols
            .Select((Symbol, Index) => (Symbol.Name, Index))
            .ToDictionary(Item => Item.Name, Item => Item.Index, StringComparer.Ordinal);
        var Writer = new Nativeˉfragmentˉartifactˉwriter();
        Writer.Writeˉbytes(MAGIC);
        Writer.Writeˉu16(Nativeˉfragmentˉartifactˉcontract.MAJOR_VERSION);
        Writer.Writeˉu16(Nativeˉfragmentˉartifactˉcontract.MINOR_VERSION);
        Writer.Writeˉu32(0);
        Writer.Writeˉu32(checked((uint)fragment.Code.Length));
        Writer.Writeˉu32(checked((uint)fragment.Symbols.Length));
        Writer.Writeˉu32(checked((uint)fragment.Patches.Length));
        Writer.Writeˉu32(checked((uint)fragment.Types.Length));
        Writer.Writeˉu32(checked((uint)fragment.Requiredˉservices.Length));
        Writer.Writeˉu32(fragment.Alignment);
        Writer.Writeˉi32(fragment.Abiˉversion);
        Writer.Writeˉu8((byte)fragment.Architecture);
        Writer.Writeˉu8(0);
        Writer.Writeˉu16(0);
        Writer.Writeˉu32(checked((uint)STRICT_UTF8.GetByteCount(fragment.Target)));
        Writer.Writeˉbytes(STRICT_UTF8.GetBytes(fragment.Target));
        Writer.Writeˉbytes(fragment.Code.AsSpan());

        foreach (var Symbol in fragment.Symbols)
        {
            Writer.Writeˉu8((byte)Symbol.Binding);
            Writer.Writeˉu8((byte)Symbol.Kind);
            Writer.Writeˉu16(0);
            Writer.Writeˉu32(Symbol.Offset);
            Writer.Writeˉu32(Symbol.Size);
            Writer.Writeˉstring(Symbol.Name);
        }
        foreach (var Patch in fragment.Patches)
        {
            Writer.Writeˉu8((byte)Patch.Kind);
            Writer.Writeˉu8(0);
            Writer.Writeˉu16(0);
            Writer.Writeˉu32(Patch.Offset);
            Writer.Writeˉu32(checked((uint)Symbolˉindices[Patch.Symbol]));
            Writer.Writeˉi32(Patch.Addend);
        }
        foreach (var Type in fragment.Types)
        {
            Nativeˉfragmentˉartifactˉtypes.Write(Writer, Type);
        }
        foreach (var Service in fragment.Requiredˉservices)
        {
            Writer.Writeˉu8((byte)Service);
        }

        var Result = Writer.Toˉarray();
        if (Result.Length > Nativeˉfragmentˉartifactˉcontract.MAXIMUM_ARTIFACT_BYTES)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF2001",
                "The encoded native-fragment artifact exceeds its size limit.");
        }
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(8, sizeof(uint)),
            checked((uint)Result.Length));
        return Result;
    }

    public static Nativeˉfragment Read(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > Nativeˉfragmentˉartifactˉcontract.MAXIMUM_ARTIFACT_BYTES)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1001",
                "The native-fragment artifact exceeds its size limit.",
                0);
        }

        var Reader = new Nativeˉfragmentˉartifactˉreader(bytes);
        if (!Reader.Readˉbytes(MAGIC.Length).SequenceEqual(MAGIC))
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1003",
                "The native-fragment artifact magic is invalid.",
                0);
        }
        var Major = Reader.Readˉu16();
        var Minor = Reader.Readˉu16();
        if (Major != Nativeˉfragmentˉartifactˉcontract.MAJOR_VERSION ||
            Minor != Nativeˉfragmentˉartifactˉcontract.MINOR_VERSION)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1004",
                $"Unsupported native-fragment artifact version {Major}.{Minor}.",
                4);
        }
        var Declaredˉlength = Reader.Readˉu32();
        if (Declaredˉlength != bytes.Length)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1005",
                "The native-fragment artifact length is inconsistent.",
                8);
        }

        var Codeˉlength = Reader.Readˉcount(
            Nativeˉcontract.MAXIMUM_CODE_BYTES,
            "code-byte");
        if (Codeˉlength == 0)
        {
            throw Invalidˉrecord("The native-fragment artifact has no code.", 12);
        }
        var Symbolˉcount = Reader.Readˉcount(Objectˉlimits.MAX_SYMBOLS, "symbol");
        var Patchˉcount = Reader.Readˉcount(Objectˉlimits.MAX_RELOCATIONS, "patch");
        var Typeˉcount = Reader.Readˉcount(Bytecodeˉlimits.MAX_NOMINAL_TYPES, "nominal-type");
        var Serviceˉcount = Reader.Readˉcount(12, "required-service");
        var Alignment = Reader.Readˉu32();
        var Abiˉversion = Reader.Readˉi32();
        var Architectureˉoffset = Reader.Offset;
        var Rawˉarchitecture = Reader.Readˉu8();
        if (Rawˉarchitecture != (byte)Objectˉarchitecture.X86ˉ64)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1006",
                "The native-fragment artifact architecture is unknown.",
                Architectureˉoffset);
        }
        var Flags = Reader.Readˉu8();
        var Reserved = Reader.Readˉu16();
        if (Flags != 0 || Reserved != 0)
        {
            throw Invalidˉrecord(
                "The native-fragment artifact header uses unsupported bits.",
                Architectureˉoffset + 1);
        }
        var Targetˉlengthˉoffset = Reader.Offset;
        var Targetˉlength = Reader.Readˉcount(Objectˉlimits.MAX_NAME_BYTES, "target-byte");
        if (Targetˉlength == 0)
        {
            throw Invalidˉrecord(
                "The native-fragment artifact target is empty.",
                Targetˉlengthˉoffset);
        }
        var Target = Readˉtarget(Reader, Targetˉlength);
        var Code = Reader.Readˉbytes(Codeˉlength).ToArray().ToImmutableArray();

        var Symbols = ImmutableArray.CreateBuilder<Nativeˉsymbol>(Symbolˉcount);
        for (var Index = 0; Index < Symbolˉcount; Index++)
        {
            Symbols.Add(Readˉsymbol(Reader));
        }
        var Frozenˉsymbols = Symbols.MoveToImmutable();
        var Patches = ImmutableArray.CreateBuilder<Nativeˉpatch>(Patchˉcount);
        for (var Index = 0; Index < Patchˉcount; Index++)
        {
            Patches.Add(Readˉpatch(Reader, Frozenˉsymbols));
        }
        var Types = ImmutableArray.CreateBuilder<Nominalˉtypeˉdeclaration>(Typeˉcount);
        for (var Index = 0; Index < Typeˉcount; Index++)
        {
            Types.Add(Nativeˉfragmentˉartifactˉtypes.Read(Reader));
        }
        var Services = ImmutableArray.CreateBuilder<Nativeˉservice>(Serviceˉcount);
        for (var Index = 0; Index < Serviceˉcount; Index++)
        {
            var Offset = Reader.Offset;
            var Rawˉservice = Reader.Readˉu8();
            if (Rawˉservice is < 1 or > 12)
            {
                throw Invalidˉrecord(
                    "A native-fragment artifact service is unknown.",
                    Offset);
            }
            Services.Add((Nativeˉservice)Rawˉservice);
        }
        Reader.Requireˉend();
        return new(
            Target,
            Abiˉversion,
            (Objectˉarchitecture)Rawˉarchitecture,
            Alignment,
            Code,
            Frozenˉsymbols,
            Patches.MoveToImmutable(),
            Types.MoveToImmutable(),
            Services.MoveToImmutable());
    }

    public static Nativeˉfragment Readˉandˉverify(ReadOnlySpan<byte> bytes) =>
        Nativeˉfragmentˉverifier.Verify(Read(bytes));

    private static string Readˉtarget(
        Nativeˉfragmentˉartifactˉreader reader,
        int length)
    {
        var Offset = reader.Offset;
        try
        {
            return STRICT_UTF8.GetString(reader.Readˉbytes(length));
        }
        catch (DecoderFallbackException)
        {
            throw new Nativeˉfragmentˉartifactˉexception(
                "WNF1010",
                "The native-fragment artifact target is not strict UTF-8.",
                Offset);
        }
    }

    private static Nativeˉsymbol Readˉsymbol(Nativeˉfragmentˉartifactˉreader reader)
    {
        var Offset = reader.Offset;
        var Rawˉbinding = reader.Readˉu8();
        var Rawˉkind = reader.Readˉu8();
        var Reserved = reader.Readˉu16();
        if (Rawˉbinding is < 1 or > 3 || Rawˉkind is < 1 or > 2 || Reserved != 0)
        {
            throw Invalidˉrecord("A native-fragment artifact symbol is invalid.", Offset);
        }
        var Symbolˉoffset = reader.Readˉu32();
        var Size = reader.Readˉu32();
        return new(
            reader.Readˉstring(Objectˉlimits.MAX_NAME_BYTES, "symbol-name"),
            (Nativeˉsymbolˉbinding)Rawˉbinding,
            (Nativeˉsymbolˉkind)Rawˉkind,
            Symbolˉoffset,
            Size);
    }

    private static Nativeˉpatch Readˉpatch(
        Nativeˉfragmentˉartifactˉreader reader,
        ImmutableArray<Nativeˉsymbol> symbols)
    {
        var Offset = reader.Offset;
        var Rawˉkind = reader.Readˉu8();
        var Flags = reader.Readˉu8();
        var Reserved = reader.Readˉu16();
        var Patchˉoffset = reader.Readˉu32();
        var Symbolˉindex = reader.Readˉu32();
        var Addend = reader.Readˉi32();
        if (Rawˉkind is < 1 or > 2 ||
            Flags != 0 ||
            Reserved != 0 ||
            Symbolˉindex >= symbols.Length)
        {
            throw Invalidˉrecord("A native-fragment artifact patch is invalid.", Offset);
        }
        return new(
            (Nativeˉpatchˉkind)Rawˉkind,
            Patchˉoffset,
            symbols[checked((int)Symbolˉindex)].Name,
            Addend);
    }

    private static Nativeˉfragmentˉartifactˉexception Invalidˉrecord(
        string message,
        int offset) => new("WNF1009", message, offset);
}
