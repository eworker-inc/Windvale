using System.Collections.Immutable;
using System.Text;
using Windvale.Bytecode;

namespace Windvale.Runtime;

public readonly struct Runtimeˉbyteˉslice
{
    internal Runtimeˉbyteˉslice(ImmutableArray<byte> storage, int offset, int length)
    {
        Storage = Runtimeˉbyteˉnode.From(storage, offset, length);
        Offset = 0;
        Length = Storage.Length;
    }

    internal Runtimeˉbyteˉslice(Runtimeˉbyteˉnode storage, int offset, int length)
    {
        ArgumentNullException.ThrowIfNull(storage);
        if (offset < 0 || length < 0 || length > storage.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        Storage = storage;
        Offset = offset;
        Length = length;
    }

    internal Runtimeˉbyteˉnode Storage { get; }

    internal int Offset { get; }

    public int Length { get; }

    internal byte[] Toˉarray()
    {
        var Result = new byte[Length];
        Storage.Copyˉto(Result, Offset, Length);
        return Result;
    }

    internal ImmutableArray<byte> Toˉimmutableˉarray()
    {
        return ImmutableArray.Create(Toˉarray());
    }
}

public readonly record struct Runtimeˉvalue
{
    private Runtimeˉvalue(
        Valueˉshape type,
        int i32,
        bool boolean,
        string? text,
        byte u8,
        uint u32,
        int enumˉvalue,
        Runtimeˉbyteˉslice bytes,
        Runtimeˉrecordˉvalue? record)
    {
        Type = type;
        I32ˉvalue = i32;
        Boolˉvalue = boolean;
        Textˉvalue = text;
        U8ˉvalue = u8;
        U32ˉvalue = u32;
        Enumˉvalue = enumˉvalue;
        Bytesˉvalue = bytes;
        Recordˉvalue = record;
    }

    public Valueˉshape Type { get; }

    public int I32ˉvalue { get; }

    public bool Boolˉvalue { get; }

    public string? Textˉvalue { get; }

    public byte U8ˉvalue { get; }

    public uint U32ˉvalue { get; }

    public int Enumˉvalue { get; }

    public Runtimeˉbyteˉslice Bytesˉvalue { get; }

    public Runtimeˉrecordˉvalue? Recordˉvalue { get; }

    public static Runtimeˉvalue Fromˉi32(int value) =>
        new(Valueˉtype.I32, value, false, null, 0, 0, 0, default, null);

    public static Runtimeˉvalue Fromˉbool(bool value) =>
        new(Valueˉtype.Bool, 0, value, null, 0, 0, 0, default, null);

    public static Runtimeˉvalue Fromˉtext(string value) =>
        new(Valueˉtype.Text, 0, false, value, 0, 0, 0, default, null);

    public static Runtimeˉvalue Fromˉu8(byte value) =>
        new(Valueˉtype.U8, 0, false, null, value, 0, 0, default, null);

    public static Runtimeˉvalue Fromˉu32(uint value) =>
        new(Valueˉtype.U32, 0, false, null, 0, value, 0, default, null);

    public static Runtimeˉvalue Fromˉbytes(ImmutableArray<byte> values) =>
        Fromˉbytes(new Runtimeˉbyteˉslice(values, 0, values.Length));

    public static Runtimeˉvalue Fromˉbytes(Runtimeˉbyteˉslice value) =>
        new(Valueˉtype.Bytes, 0, false, null, 0, 0, 0, value, null);

    public static Runtimeˉvalue Fromˉenum(int typeˉindex, int value) =>
        new(Valueˉshape.Forˉenum(typeˉindex), 0, false, null, 0, 0, value, default, null);

    public static Runtimeˉvalue Fromˉrecord(
        int typeˉindex,
        ImmutableArray<Runtimeˉvalue> fields) =>
        new(
            Valueˉshape.Forˉrecord(typeˉindex),
            0,
            false,
            null,
            0,
            0,
            0,
            default,
            new(typeˉindex, fields));

    public static Runtimeˉvalue Default(
        Valueˉshape type,
        ImmutableArray<Nominalˉtypeˉdeclaration> nominalˉtypes)
    {
        return type.Kind switch
        {
            Valueˉtype.I32 => Fromˉi32(0),
            Valueˉtype.Bool => Fromˉbool(false),
            Valueˉtype.Text => Fromˉtext(string.Empty),
            Valueˉtype.U8 => Fromˉu8(0),
            Valueˉtype.U32 => Fromˉu32(0),
            Valueˉtype.Bytes => Fromˉbytes(ImmutableArray<byte>.Empty),
            Valueˉtype.Record => Defaultˉrecord(type.Nominalˉtypeˉindex, nominalˉtypes),
            Valueˉtype.Enum => Defaultˉenum(type.Nominalˉtypeˉindex, nominalˉtypes),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Void has no runtime value."),
        };
    }

    private static Runtimeˉvalue Defaultˉrecord(
        int typeˉindex,
        ImmutableArray<Nominalˉtypeˉdeclaration> nominalˉtypes)
    {
        var Type = (Recordˉtypeˉdeclaration)nominalˉtypes[typeˉindex];
        return Fromˉrecord(
            typeˉindex,
            [.. Type.Fields.Select(Field => Default(Field.Type, nominalˉtypes))]);
    }

    private static Runtimeˉvalue Defaultˉenum(
        int typeˉindex,
        ImmutableArray<Nominalˉtypeˉdeclaration> nominalˉtypes)
    {
        var Type = (Enumˉtypeˉdeclaration)nominalˉtypes[typeˉindex];
        return Fromˉenum(typeˉindex, Type.Members[0].Value);
    }
}

public sealed record Runtimeˉrecordˉvalue(
    int Typeˉindex,
    ImmutableArray<Runtimeˉvalue> Fields);

public sealed record Runtimeˉoptions(
    ImmutableHashSet<string> Authorizedˉcapabilities,
    long Maximumˉinstructions = 1_000_000,
    int Maximumˉcallˉdepth = 1024,
    bool Collectˉfunctionˉsteps = false)
{
    public static Runtimeˉoptions Portableˉdefaults { get; } = new(
        ImmutableHashSet.Create<string>(StringComparer.Ordinal));
}

public sealed record Runtimeˉresult(int Exitˉcode, long Executedˉinstructions);

public sealed record Runtimeˉfunctionˉsteps(
    int Functionˉindex,
    string Functionˉname,
    long Executedˉinstructions);

public static class Hostedˉresourceˉlimits
{
    public const int MAX_ARGUMENTS = 67;
    public const int MAX_ARGUMENT_UTF8_BYTES = 4 * 1024;
    public const int MAX_ARGUMENT_TOTAL_UTF8_BYTES = 64 * 1024;
    public const int MAX_FILE_SNAPSHOTS = 64;
}

public enum Hostedˉfileˉerror
{
    Invalidˉname,
    Notˉfound,
    Permissionˉdenied,
    Unavailable,
    Tooˉlarge,
}

public sealed class Hostedˉfileˉexception : Exception
{
    public Hostedˉfileˉexception(Hostedˉfileˉerror error, string message)
        : base(message)
    {
        Error = error;
    }

    public Hostedˉfileˉerror Error { get; }
}

public interface IHostedˉfileˉreader
{
    ImmutableArray<byte> Readˉbytes(string resourceˉname, int maximumˉbytes);
}

public interface IHostedˉfileˉwriter
{
    void Writeˉbytes(
        string resourceˉname,
        ImmutableArray<byte> bytes,
        int maximumˉbytes);
}

public sealed class Hostedˉresourceˉcontext
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);
    private readonly Dictionary<string, ImmutableArray<byte>> Fileˉsnapshots =
        new(StringComparer.Ordinal);

    public Hostedˉresourceˉcontext(
        ImmutableArray<string> arguments,
        TextWriter standardˉoutput,
        TextWriter diagnosticˉoutput,
        IHostedˉfileˉreader? fileˉreader = null,
        IHostedˉfileˉwriter? fileˉwriter = null)
    {
        ArgumentNullException.ThrowIfNull(standardˉoutput);
        ArgumentNullException.ThrowIfNull(diagnosticˉoutput);
        if (arguments.IsDefault)
        {
            throw new ArgumentException("Hosted arguments must be an initialized immutable array.", nameof(arguments));
        }

        if (arguments.Length > Hostedˉresourceˉlimits.MAX_ARGUMENTS)
        {
            throw new Runtimeˉexception(
                "WVR3027",
                $"The launcher supplied {arguments.Length} arguments; the limit is {Hostedˉresourceˉlimits.MAX_ARGUMENTS}.");
        }

        var Totalˉbytes = 0;
        for (var Index = 0; Index < arguments.Length; Index++)
        {
            var Argument = arguments[Index] ?? throw new Runtimeˉexception(
                "WVR3027",
                $"Hosted argument {Index} is null.");
            int Utf8ˉbytes;
            try
            {
                Utf8ˉbytes = STRICT_UTF8.GetByteCount(Argument);
            }
            catch (EncoderFallbackException)
            {
                throw new Runtimeˉexception(
                    "WVR3027",
                    $"Hosted argument {Index} is not valid Unicode.");
            }

            if (Utf8ˉbytes > Hostedˉresourceˉlimits.MAX_ARGUMENT_UTF8_BYTES)
            {
                throw new Runtimeˉexception(
                    "WVR3027",
                    $"Hosted argument {Index} uses {Utf8ˉbytes} UTF-8 bytes; the per-argument limit is {Hostedˉresourceˉlimits.MAX_ARGUMENT_UTF8_BYTES}.");
            }

            Totalˉbytes = checked(Totalˉbytes + Utf8ˉbytes);
            if (Totalˉbytes > Hostedˉresourceˉlimits.MAX_ARGUMENT_TOTAL_UTF8_BYTES)
            {
                throw new Runtimeˉexception(
                    "WVR3027",
                    $"Hosted arguments use {Totalˉbytes} UTF-8 bytes; the total limit is {Hostedˉresourceˉlimits.MAX_ARGUMENT_TOTAL_UTF8_BYTES}.");
            }
        }

        Arguments = arguments;
        Standardˉoutput = standardˉoutput;
        Diagnosticˉoutput = diagnosticˉoutput;
        Fileˉreader = fileˉreader;
        Fileˉwriter = fileˉwriter;
    }

    public ImmutableArray<string> Arguments { get; }

    public TextWriter Standardˉoutput { get; }

    public TextWriter Diagnosticˉoutput { get; }

    public IHostedˉfileˉreader? Fileˉreader { get; }

    public IHostedˉfileˉwriter? Fileˉwriter { get; }

    public uint Getˉargumentˉcount() => checked((uint)Arguments.Length);

    public string Getˉargument(uint index)
    {
        if (index >= (uint)Arguments.Length)
        {
            throw new Runtimeˉexception(
                "WVR3020",
                $"Hosted argument index {index} is outside the supplied count {Arguments.Length}.");
        }

        return Arguments[(int)index];
    }

    public ImmutableArray<byte> Readˉfileˉbytes(string resourceˉname)
    {
        ArgumentNullException.ThrowIfNull(resourceˉname);
        if (Fileˉreader is null)
        {
            throw new Runtimeˉexception(
                "WVR3001",
                $"The host does not implement capability '{Capabilityˉcatalog.FILE_READ_BYTES}'.");
        }

        if (Fileˉsnapshots.TryGetValue(resourceˉname, out var Snapshot))
        {
            return Snapshot;
        }

        if (Fileˉsnapshots.Count >= Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS)
        {
            throw new Runtimeˉexception(
                "WVR3028",
                $"The hosted resource context already contains {Fileˉsnapshots.Count} distinct file snapshots; the limit is {Hostedˉresourceˉlimits.MAX_FILE_SNAPSHOTS}.");
        }

        try
        {
            var Bytes = Fileˉreader.Readˉbytes(resourceˉname, Bytecodeˉlimits.MAX_BYTE_DATA_BYTES);
            if (Bytes.IsDefault)
            {
                throw new Runtimeˉexception(
                    "WVR3026",
                    "The file adapter returned an uninitialized byte value.");
            }
            if (Bytes.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
            {
                throw new Runtimeˉexception(
                    "WVR3025",
                    $"The file adapter returned {Bytes.Length} bytes; the limit is {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}.");
            }

            Fileˉsnapshots.Add(resourceˉname, Bytes);
            return Bytes;
        }
        catch (Hostedˉfileˉexception Exception)
        {
            var Code = Exception.Error switch
            {
                Hostedˉfileˉerror.Invalidˉname => "WVR3021",
                Hostedˉfileˉerror.Notˉfound => "WVR3022",
                Hostedˉfileˉerror.Permissionˉdenied => "WVR3023",
                Hostedˉfileˉerror.Unavailable => "WVR3024",
                Hostedˉfileˉerror.Tooˉlarge => "WVR3025",
                _ => "WVR3026",
            };
            throw new Runtimeˉexception(Code, Exception.Message);
        }
    }
}

public interface ICapabilityˉhost
{
    bool Supports(string capabilityˉname);

    Runtimeˉvalue? Invoke(
        Capabilityˉdeclaration capability,
        ImmutableArray<Runtimeˉvalue> arguments);
}

public sealed class Referenceˉcapabilityˉhost : ICapabilityˉhost
{
    private readonly Hostedˉresourceˉcontext Resources;

    public Referenceˉcapabilityˉhost(TextWriter output)
        : this(new Hostedˉresourceˉcontext([], output, TextWriter.Null))
    {
    }

    public Referenceˉcapabilityˉhost(Hostedˉresourceˉcontext resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        Resources = resources;
    }

    public bool Supports(string capabilityˉname)
    {
        return capabilityˉname switch
        {
            Capabilityˉcatalog.CONSOLE_WRITE or
            Capabilityˉcatalog.CONSOLE_WRITE_LINE or
            Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE or
            Capabilityˉcatalog.PROCESS_ARGUMENT or
            Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT => true,
            Capabilityˉcatalog.FILE_READ_BYTES => Resources.Fileˉreader is not null,
            Capabilityˉcatalog.FILE_WRITE_BYTES => Resources.Fileˉwriter is not null,
            _ => false,
        };
    }

    public Runtimeˉvalue? Invoke(
        Capabilityˉdeclaration capability,
        ImmutableArray<Runtimeˉvalue> arguments)
    {
        switch (capability.Name)
        {
            case Capabilityˉcatalog.CONSOLE_WRITE:
                Resources.Standardˉoutput.Write(arguments[0].Textˉvalue!);
                return null;
            case Capabilityˉcatalog.CONSOLE_WRITE_LINE:
                Resources.Standardˉoutput.Write(arguments[0].Textˉvalue!);
                Resources.Standardˉoutput.Write('\n');
                return null;
            case Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE:
                Resources.Diagnosticˉoutput.Write(arguments[0].Textˉvalue!);
                Resources.Diagnosticˉoutput.Write('\n');
                return null;
            case Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT:
                return Runtimeˉvalue.Fromˉu32(Resources.Getˉargumentˉcount());
            case Capabilityˉcatalog.PROCESS_ARGUMENT:
                return Runtimeˉvalue.Fromˉtext(Resources.Getˉargument(arguments[0].U32ˉvalue));
            case Capabilityˉcatalog.FILE_READ_BYTES:
                return Runtimeˉvalue.Fromˉbytes(Resources.Readˉfileˉbytes(arguments[0].Textˉvalue!));
            case Capabilityˉcatalog.FILE_WRITE_BYTES:
                Writeˉfile(arguments[0].Textˉvalue!, arguments[1].Bytesˉvalue);
                return null;
            default:
                throw new Runtimeˉexception(
                    "WVR3001",
                    $"The host does not implement capability '{capability.Name}'.");
        }
    }

    private void Writeˉfile(string resourceˉname, Runtimeˉbyteˉslice bytes)
    {
        if (Resources.Fileˉwriter is null)
        {
            throw new Runtimeˉexception(
                "WVR3001",
                $"The host does not implement capability '{Capabilityˉcatalog.FILE_WRITE_BYTES}'.");
        }

        if (bytes.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new Runtimeˉexception(
                "WVR3025",
                $"The file value uses {bytes.Length} bytes; the limit is {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}.");
        }

        try
        {
            Resources.Fileˉwriter.Writeˉbytes(
                resourceˉname,
                bytes.Toˉimmutableˉarray(),
                Bytecodeˉlimits.MAX_BYTE_DATA_BYTES);
        }
        catch (Hostedˉfileˉexception Exception)
        {
            var Code = Exception.Error switch
            {
                Hostedˉfileˉerror.Invalidˉname => "WVR3021",
                Hostedˉfileˉerror.Notˉfound => "WVR3022",
                Hostedˉfileˉerror.Permissionˉdenied => "WVR3023",
                Hostedˉfileˉerror.Unavailable => "WVR3024",
                Hostedˉfileˉerror.Tooˉlarge => "WVR3025",
                _ => "WVR3026",
            };
            throw new Runtimeˉexception(Code, Exception.Message);
        }
    }
}

public sealed class Runtimeˉexception : Exception
{
    public Runtimeˉexception(string code, string message)
        : base($"{code}: {message}")
    {
        Code = code;
    }

    public string Code { get; }
}
