using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Assembler;

internal static class X64ˉobjectˉencoder
{
    public static byte[] Encode(Assemblyˉunit unit)
    {
        var Symbolˉindices = unit.Symbols
            .Select((Symbol, Index) => (Symbol.Name, Index))
            .ToDictionary(Item => Item.Name, Item => (uint)Item.Index, StringComparer.Ordinal);
        var Definitions = new Dictionary<string, Definitionˉrange>(StringComparer.Ordinal);
        var Relocations = ImmutableArray.CreateBuilder<Objectˉrelocation>();
        var Sections = ImmutableArray.CreateBuilder<Objectˉsection>(unit.Sections.Length);

        for (var Sectionˉindex = 0; Sectionˉindex < unit.Sections.Length; Sectionˉindex++)
        {
            var Section = unit.Sections[Sectionˉindex];
            var Output = new Byteˉbuffer();
            uint Memoryˉsize = 0;
            foreach (var Definition in Section.Definitions)
            {
                var Offset = Memoryˉsize;
                foreach (var Statement in Definition.Statements)
                {
                    if (Section.Kind == Objectˉsectionˉkind.Zeroˉfill)
                    {
                        Memoryˉsize = Addˉmemory(Memoryˉsize, (uint)Statement.Number, Statement.Span);
                        continue;
                    }

                    Encodeˉstatement(
                        Statement,
                        (uint)Sectionˉindex,
                        Output,
                        Symbolˉindices,
                        Relocations);
                    Memoryˉsize = (uint)Output.Count;
                }
                Definitions.Add(Definition.Name, new((uint)Sectionˉindex, Offset, Memoryˉsize - Offset));
            }

            Sections.Add(new(
                Section.Name,
                Section.Kind,
                Section.Alignment,
                Memoryˉsize,
                Section.Kind == Objectˉsectionˉkind.Zeroˉfill ? [] : Output.Toˉimmutable()));
        }

        var Symbols = ImmutableArray.CreateBuilder<Objectˉsymbol>(unit.Symbols.Length);
        foreach (var Symbol in unit.Symbols)
        {
            if (Symbol.Binding == Objectˉsymbolˉbinding.Import)
            {
                Symbols.Add(new(
                    Symbol.Name,
                    Symbol.Binding,
                    Symbol.Kind,
                    Objectˉlimits.UNDEFINED_SECTION,
                    0,
                    0));
                continue;
            }

            var Range = Definitions[Symbol.Name];
            Symbols.Add(new(
                Symbol.Name,
                Symbol.Binding,
                Symbol.Kind,
                Range.Sectionˉindex,
                Range.Offset,
                Range.Size));
        }

        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            Sections.ToImmutable(),
            Symbols.ToImmutable(),
            Relocations.ToImmutable());
        return Objectˉcodec.Write(Object);
    }

    private static void Encodeˉstatement(
        Assemblyˉstatement statement,
        uint sectionˉindex,
        Byteˉbuffer output,
        IReadOnlyDictionary<string, uint> symbolˉindices,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        switch (statement.Kind)
        {
            case Assemblyˉstatementˉkind.Nop:
                output.Writeˉu8(0x90, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Return:
                output.Writeˉu8(0xC3, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Trap:
                output.Writeˉu8(0xCC, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Disableˉinterrupts:
                output.Writeˉu8(0xFA, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Halt:
                output.Writeˉu8(0xF4, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Outˉu16:
                output.Writeˉu8(0x66, statement.Span);
                output.Writeˉu8(0xEF, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Pushˉi32:
                output.Writeˉu8(0x68, statement.Span);
                output.Writeˉi32((int)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Call:
            case Assemblyˉstatementˉkind.Jump:
                output.Writeˉu8(
                    statement.Kind == Assemblyˉstatementˉkind.Call ? (byte)0xE8 : (byte)0xE9,
                    statement.Span);
                var Relativeˉoffset = (uint)output.Count;
                output.Writeˉu32(0, statement.Span);
                relocations.Add(new(
                    Objectˉrelocationˉkind.Relativeˉi32,
                    sectionˉindex,
                    Relativeˉoffset,
                    symbolˉindices[statement.Name!],
                    -4));
                break;
            case Assemblyˉstatementˉkind.Moveˉi32:
                output.Writeˉu8((byte)(0xB8 + statement.Register), statement.Span);
                output.Writeˉi32((int)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Moveˉu32:
                output.Writeˉu8((byte)(0xB8 + statement.Register), statement.Span);
                output.Writeˉu32((uint)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Bytes:
                output.Writeˉbytes(statement.Bytes.AsSpan(), statement.Span);
                break;
            case Assemblyˉstatementˉkind.U32:
                output.Writeˉu32((uint)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.I32:
                output.Writeˉi32((int)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Addressˉu32:
                var Absoluteˉoffset = (uint)output.Count;
                output.Writeˉu32(0, statement.Span);
                relocations.Add(new(
                    Objectˉrelocationˉkind.Absoluteˉu32,
                    sectionˉindex,
                    Absoluteˉoffset,
                    symbolˉindices[statement.Name!],
                    0));
                break;
            case Assemblyˉstatementˉkind.Zero:
                throw new InvalidOperationException("Zero-fill statements must not materialize bytes.");
            default:
                throw new ArgumentOutOfRangeException(nameof(statement), statement.Kind, null);
        }
    }

    private static uint Addˉmemory(uint current, uint amount, Assemblyˉspan span)
    {
        if (amount > Objectˉlimits.MAX_MEMORY_BYTES - current)
        {
            throw new Assemblyˉencodingˉexception(
                new("WVA1011", span.Line, span.Column, "Zero-fill data exceeds the WVO memory limit."));
        }
        return current + amount;
    }

    private sealed record Definitionˉrange(uint Sectionˉindex, uint Offset, uint Size);

    private sealed class Byteˉbuffer
    {
        private readonly List<byte> Bytes = [];

        public int Count => Bytes.Count;

        public void Writeˉu8(byte value, Assemblyˉspan span)
        {
            Requireˉspace(1, span);
            Bytes.Add(value);
        }

        public void Writeˉu32(uint value, Assemblyˉspan span)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Buffer, value);
            Writeˉbytes(Buffer, span);
        }

        public void Writeˉi32(int value, Assemblyˉspan span)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(Buffer, value);
            Writeˉbytes(Buffer, span);
        }

        public void Writeˉbytes(ReadOnlySpan<byte> value, Assemblyˉspan span)
        {
            Requireˉspace(value.Length, span);
            foreach (var Item in value)
            {
                Bytes.Add(Item);
            }
        }

        public ImmutableArray<byte> Toˉimmutable() => [.. Bytes];

        private void Requireˉspace(int count, Assemblyˉspan span)
        {
            if (count < 0 || count > Objectˉlimits.MAX_OBJECT_BYTES - Bytes.Count)
            {
                throw new Assemblyˉencodingˉexception(
                    new("WVA1011", span.Line, span.Column, "Section data exceeds the WVO data limit."));
            }
        }
    }
}

internal sealed class Assemblyˉencodingˉexception(Assemblyˉdiagnostic diagnostic) : Exception(diagnostic.Message)
{
    public Assemblyˉdiagnostic Diagnostic { get; } = diagnostic;
}
