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
                var Labels = new Dictionary<string, uint>(StringComparer.Ordinal);
                var Statementˉoffset = (uint)Output.Count;
                foreach (var Statement in Definition.Statements)
                {
                    if (Statement.Kind == Assemblyˉstatementˉkind.Label)
                    {
                        Labels.Add(Statement.Name!, Statementˉoffset);
                    }
                    else
                    {
                        Statementˉoffset = checked(Statementˉoffset + Encodedˉsize(Statement));
                    }
                }
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
                        Relocations,
                        Labels);
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
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        IReadOnlyDictionary<string, uint> labels)
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
            case Assemblyˉstatementˉkind.Inˉu8:
                output.Writeˉu8(0xEC, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Outˉu8:
                output.Writeˉu8(0xEE, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Pushˉi32:
                output.Writeˉu8(0x68, statement.Span);
                output.Writeˉi32((int)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Enableˉpageˉprotection:
                output.Writeˉbytes(
                    [
                        0xB9, 0x80, 0x00, 0x00, 0xC0,
                        0x0F, 0x32,
                        0x0F, 0xBA, 0xE8, 0x0B,
                        0x0F, 0x30,
                        0x0F, 0x20, 0xC0,
                        0x48, 0x0F, 0xBA, 0xE8, 0x10,
                        0x0F, 0x22, 0xC0,
                    ],
                    statement.Span);
                break;
            case Assemblyˉstatementˉkind.Activateˉpageˉtable:
                output.Writeˉbytes([0x0F, 0x22, 0xD8, 0x0F, 0x20, 0xD8], statement.Span);
                break;
            case Assemblyˉstatementˉkind.Syscall:
                output.Writeˉbytes([0x0F, 0x05], statement.Span);
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
                Writeˉrex(output, widthˉ64: false, register: 0, memory: statement.Register, statement.Span);
                output.Writeˉu8((byte)(0xB8 + (statement.Register & 7)), statement.Span);
                output.Writeˉi32((int)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Moveˉu32:
                Writeˉrex(output, widthˉ64: false, register: 0, memory: statement.Register, statement.Span);
                output.Writeˉu8((byte)(0xB8 + (statement.Register & 7)), statement.Span);
                output.Writeˉu32((uint)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Moveˉu8:
                Writeˉrex(
                    output,
                    widthˉ64: false,
                    register: 0,
                    memory: statement.Firstˉregister.Index,
                    statement.Span,
                    force: Requiresˉbyteˉrex(statement.Firstˉregister));
                output.Writeˉu8(
                    (byte)(0xB0 + (statement.Firstˉregister.Index & 7)),
                    statement.Span);
                output.Writeˉu8((byte)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Moveˉu16:
                Writeˉoperandˉsizeˉprefix(output, statement.Firstˉregister, statement.Span);
                Writeˉrex(
                    output,
                    widthˉ64: false,
                    register: 0,
                    memory: statement.Firstˉregister.Index,
                    statement.Span);
                output.Writeˉu8(
                    (byte)(0xB8 + (statement.Firstˉregister.Index & 7)),
                    statement.Span);
                output.Writeˉu16((ushort)statement.Number, statement.Span);
                break;
            case Assemblyˉstatementˉkind.Label:
                break;
            case Assemblyˉstatementˉkind.Jumpˉlabel:
                output.Writeˉu8(0xE9, statement.Span);
                Writeˉlocalˉdisplacement(output, labels[statement.Name!], statement.Span);
                break;
            case Assemblyˉstatementˉkind.Branch:
                output.Writeˉu8(0x0F, statement.Span);
                output.Writeˉu8((byte)(0x80 + statement.Condition), statement.Span);
                Writeˉlocalˉdisplacement(output, labels[statement.Name!], statement.Span);
                break;
            case Assemblyˉstatementˉkind.Moveˉregister:
            case Assemblyˉstatementˉkind.Add:
            case Assemblyˉstatementˉkind.Subtract:
            case Assemblyˉstatementˉkind.And:
            case Assemblyˉstatementˉkind.Or:
            case Assemblyˉstatementˉkind.Xor:
            case Assemblyˉstatementˉkind.Compare:
            case Assemblyˉstatementˉkind.Test:
                Writeˉregisterˉbinary(output, statement);
                break;
            case Assemblyˉstatementˉkind.Multiply:
                Writeˉmultiply(output, statement);
                break;
            case Assemblyˉstatementˉkind.Addˉi32:
            case Assemblyˉstatementˉkind.Subtractˉi32:
            case Assemblyˉstatementˉkind.Andˉi32:
            case Assemblyˉstatementˉkind.Orˉi32:
            case Assemblyˉstatementˉkind.Xorˉi32:
            case Assemblyˉstatementˉkind.Compareˉi32:
            case Assemblyˉstatementˉkind.Testˉi32:
            case Assemblyˉstatementˉkind.Addˉi8:
            case Assemblyˉstatementˉkind.Subtractˉi8:
            case Assemblyˉstatementˉkind.Andˉi8:
            case Assemblyˉstatementˉkind.Orˉi8:
            case Assemblyˉstatementˉkind.Xorˉi8:
            case Assemblyˉstatementˉkind.Compareˉi8:
            case Assemblyˉstatementˉkind.Testˉi8:
            case Assemblyˉstatementˉkind.Addˉi16:
            case Assemblyˉstatementˉkind.Subtractˉi16:
            case Assemblyˉstatementˉkind.Andˉi16:
            case Assemblyˉstatementˉkind.Orˉi16:
            case Assemblyˉstatementˉkind.Xorˉi16:
            case Assemblyˉstatementˉkind.Compareˉi16:
            case Assemblyˉstatementˉkind.Testˉi16:
                Writeˉimmediate(output, statement);
                break;
            case Assemblyˉstatementˉkind.Rotateˉleft:
            case Assemblyˉstatementˉkind.Rotateˉright:
            case Assemblyˉstatementˉkind.Shiftˉleft:
            case Assemblyˉstatementˉkind.Shiftˉright:
            case Assemblyˉstatementˉkind.Shiftˉrightˉsigned:
                Writeˉshift(output, statement);
                break;
            case Assemblyˉstatementˉkind.Pushˉregister:
            case Assemblyˉstatementˉkind.Popˉregister:
                Writeˉrex(
                    output,
                    widthˉ64: false,
                    register: 0,
                    memory: statement.Firstˉregister.Index,
                    statement.Span);
                output.Writeˉu8(
                    (byte)((statement.Kind == Assemblyˉstatementˉkind.Pushˉregister ? 0x50 : 0x58) +
                        (statement.Firstˉregister.Index & 7)),
                    statement.Span);
                break;
            case Assemblyˉstatementˉkind.Callˉregister:
            case Assemblyˉstatementˉkind.Jumpˉregister:
                Writeˉrex(
                    output,
                    widthˉ64: false,
                    register: 0,
                    memory: statement.Firstˉregister.Index,
                    statement.Span);
                output.Writeˉu8(0xFF, statement.Span);
                output.Writeˉu8(
                    (byte)((statement.Kind == Assemblyˉstatementˉkind.Callˉregister ? 0xD0 : 0xE0) +
                        (statement.Firstˉregister.Index & 7)),
                    statement.Span);
                break;
            case Assemblyˉstatementˉkind.Loadˉu32:
            case Assemblyˉstatementˉkind.Loadˉu64:
            case Assemblyˉstatementˉkind.Loadˉu8:
            case Assemblyˉstatementˉkind.Loadˉu16:
            case Assemblyˉstatementˉkind.Loadˉaddress:
            case Assemblyˉstatementˉkind.Storeˉu32:
            case Assemblyˉstatementˉkind.Storeˉu64:
            case Assemblyˉstatementˉkind.Storeˉu8:
            case Assemblyˉstatementˉkind.Storeˉu16:
                Writeˉripˉrelative(
                    output,
                    statement,
                    sectionˉindex,
                    symbolˉindices[statement.Name!],
                    relocations);
                break;
            case Assemblyˉstatementˉkind.Loadˉmemoryˉu32:
            case Assemblyˉstatementˉkind.Loadˉmemoryˉu64:
            case Assemblyˉstatementˉkind.Storeˉmemoryˉu32:
            case Assemblyˉstatementˉkind.Storeˉmemoryˉu64:
            case Assemblyˉstatementˉkind.Loadˉmemoryˉu8:
            case Assemblyˉstatementˉkind.Loadˉmemoryˉu16:
            case Assemblyˉstatementˉkind.Storeˉmemoryˉu8:
            case Assemblyˉstatementˉkind.Storeˉmemoryˉu16:
                Writeˉmemory(output, statement);
                break;
            case Assemblyˉstatementˉkind.Setˉcondition:
                Writeˉconditionˉresult(output, statement);
                break;
            case Assemblyˉstatementˉkind.Zeroˉextendˉu8:
            case Assemblyˉstatementˉkind.Zeroˉextendˉu16:
            case Assemblyˉstatementˉkind.Signˉextendˉi8:
            case Assemblyˉstatementˉkind.Signˉextendˉi16:
                Writeˉextension(output, statement);
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

    private static uint Encodedˉsize(Assemblyˉstatement statement) => statement.Kind switch
    {
        Assemblyˉstatementˉkind.Label => 0,
        Assemblyˉstatementˉkind.Nop or Assemblyˉstatementˉkind.Return or
            Assemblyˉstatementˉkind.Trap or Assemblyˉstatementˉkind.Disableˉinterrupts or
            Assemblyˉstatementˉkind.Halt or Assemblyˉstatementˉkind.Inˉu8 or
            Assemblyˉstatementˉkind.Outˉu8 => 1,
        Assemblyˉstatementˉkind.Outˉu16 or Assemblyˉstatementˉkind.Syscall => 2,
        Assemblyˉstatementˉkind.Call or Assemblyˉstatementˉkind.Jump or
            Assemblyˉstatementˉkind.Pushˉi32 or Assemblyˉstatementˉkind.Jumpˉlabel => 5,
        Assemblyˉstatementˉkind.Moveˉi32 or Assemblyˉstatementˉkind.Moveˉu32 =>
            (uint)(5 + (statement.Register >= 8 ? 1 : 0)),
        Assemblyˉstatementˉkind.Moveˉu8 =>
            (uint)(2 + (Requiresˉbyteˉrex(statement.Firstˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Moveˉu16 =>
            (uint)(4 + (statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Enableˉpageˉprotection => 24,
        Assemblyˉstatementˉkind.Activateˉpageˉtable => 6,
        Assemblyˉstatementˉkind.Branch => 6,
        Assemblyˉstatementˉkind.Moveˉregister or Assemblyˉstatementˉkind.Add or
            Assemblyˉstatementˉkind.Subtract or Assemblyˉstatementˉkind.And or
            Assemblyˉstatementˉkind.Or or Assemblyˉstatementˉkind.Xor or
            Assemblyˉstatementˉkind.Compare or Assemblyˉstatementˉkind.Test =>
            (uint)(2 + (statement.Firstˉregister.Width == 16 ? 1 : 0) +
                (Needsˉrex(statement.Firstˉregister, statement.Secondˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Multiply =>
            (uint)(3 + (statement.Firstˉregister.Width == 16 ? 1 : 0) +
                (Needsˉrex(statement.Firstˉregister, statement.Secondˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Addˉi32 or Assemblyˉstatementˉkind.Subtractˉi32 or
            Assemblyˉstatementˉkind.Andˉi32 or Assemblyˉstatementˉkind.Orˉi32 or
            Assemblyˉstatementˉkind.Xorˉi32 or Assemblyˉstatementˉkind.Compareˉi32 or
        Assemblyˉstatementˉkind.Testˉi32 =>
            (uint)(6 + (statement.Firstˉregister.Width == 64 || statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Addˉi8 or Assemblyˉstatementˉkind.Subtractˉi8 or
            Assemblyˉstatementˉkind.Andˉi8 or Assemblyˉstatementˉkind.Orˉi8 or
            Assemblyˉstatementˉkind.Xorˉi8 or Assemblyˉstatementˉkind.Compareˉi8 or
            Assemblyˉstatementˉkind.Testˉi8 =>
            (uint)(3 + (Requiresˉbyteˉrex(statement.Firstˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Addˉi16 or Assemblyˉstatementˉkind.Subtractˉi16 or
            Assemblyˉstatementˉkind.Andˉi16 or Assemblyˉstatementˉkind.Orˉi16 or
            Assemblyˉstatementˉkind.Xorˉi16 or Assemblyˉstatementˉkind.Compareˉi16 or
            Assemblyˉstatementˉkind.Testˉi16 =>
            (uint)(5 + (statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Rotateˉleft or Assemblyˉstatementˉkind.Rotateˉright or
            Assemblyˉstatementˉkind.Shiftˉleft or Assemblyˉstatementˉkind.Shiftˉright or
            Assemblyˉstatementˉkind.Shiftˉrightˉsigned =>
            (uint)(3 + (statement.Firstˉregister.Width == 16 ? 1 : 0) +
                (statement.Firstˉregister.Width == 64 || statement.Firstˉregister.Isˉextended ||
                    Requiresˉbyteˉrex(statement.Firstˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Pushˉregister or Assemblyˉstatementˉkind.Popˉregister =>
            (uint)(1 + (statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Callˉregister or Assemblyˉstatementˉkind.Jumpˉregister =>
            (uint)(2 + (statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Loadˉu32 or Assemblyˉstatementˉkind.Loadˉu64 or
            Assemblyˉstatementˉkind.Storeˉu32 or Assemblyˉstatementˉkind.Storeˉu64 or
            Assemblyˉstatementˉkind.Loadˉaddress =>
            (uint)(6 + (statement.Firstˉregister.Width == 64 || statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Loadˉu8 or Assemblyˉstatementˉkind.Storeˉu8 =>
            (uint)(6 + (Requiresˉbyteˉrex(statement.Firstˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Loadˉu16 or Assemblyˉstatementˉkind.Storeˉu16 =>
            (uint)(7 + (statement.Firstˉregister.Isˉextended ? 1 : 0)),
        Assemblyˉstatementˉkind.Loadˉmemoryˉu32 or Assemblyˉstatementˉkind.Loadˉmemoryˉu64 or
            Assemblyˉstatementˉkind.Storeˉmemoryˉu32 or Assemblyˉstatementˉkind.Storeˉmemoryˉu64 =>
            (uint)(7 + (Needsˉmemoryˉrex(statement) ? 1 : 0)),
        Assemblyˉstatementˉkind.Loadˉmemoryˉu8 or Assemblyˉstatementˉkind.Storeˉmemoryˉu8 =>
            (uint)(7 + (Needsˉmemoryˉrex(statement) ? 1 : 0)),
        Assemblyˉstatementˉkind.Loadˉmemoryˉu16 or Assemblyˉstatementˉkind.Storeˉmemoryˉu16 =>
            (uint)(8 + (Needsˉmemoryˉrex(statement) ? 1 : 0)),
        Assemblyˉstatementˉkind.Setˉcondition =>
            (uint)(3 + (Requiresˉbyteˉrex(statement.Firstˉregister) ? 1 : 0)),
        Assemblyˉstatementˉkind.Zeroˉextendˉu8 or Assemblyˉstatementˉkind.Signˉextendˉi8 or
            Assemblyˉstatementˉkind.Zeroˉextendˉu16 or Assemblyˉstatementˉkind.Signˉextendˉi16 =>
            (uint)(3 + (Needsˉextensionˉrex(statement) ? 1 : 0)),
        Assemblyˉstatementˉkind.Bytes => (uint)statement.Bytes.Length,
        Assemblyˉstatementˉkind.U32 or Assemblyˉstatementˉkind.I32 or
            Assemblyˉstatementˉkind.Addressˉu32 => 4,
        Assemblyˉstatementˉkind.Zero => (uint)statement.Number,
        _ => throw new ArgumentOutOfRangeException(nameof(statement), statement.Kind, null),
    };

    private static bool Needsˉrex(Assemblyˉregister first, Assemblyˉregister second) =>
        first.Width == 64 || first.Isˉextended || second.Isˉextended ||
        Requiresˉbyteˉrex(first) || Requiresˉbyteˉrex(second);

    private static bool Needsˉmemoryˉrex(Assemblyˉstatement statement) =>
        statement.Firstˉregister.Width == 64 || statement.Firstˉregister.Isˉextended ||
        Requiresˉbyteˉrex(statement.Firstˉregister) ||
        statement.Secondˉregister.Isˉextended ||
        statement.Hasˉindex && statement.Thirdˉregister.Isˉextended;

    private static bool Needsˉextensionˉrex(Assemblyˉstatement statement) =>
        statement.Firstˉregister.Width == 64 || statement.Firstˉregister.Isˉextended ||
        statement.Secondˉregister.Isˉextended || Requiresˉbyteˉrex(statement.Secondˉregister);

    private static bool Requiresˉbyteˉrex(Assemblyˉregister value) =>
        value.Width == 8 && value.Index >= 4;

    private static void Writeˉoperandˉsizeˉprefix(
        Byteˉbuffer output,
        Assemblyˉregister value,
        Assemblyˉspan span)
    {
        if (value.Width == 16)
        {
            output.Writeˉu8(0x66, span);
        }
    }

    private static void Writeˉrex(
        Byteˉbuffer output,
        bool widthˉ64,
        byte register,
        byte memory,
        Assemblyˉspan span,
        byte index = 0,
        bool force = false)
    {
        if (!force && !widthˉ64 && register < 8 && index < 8 && memory < 8)
        {
            return;
        }

        output.Writeˉu8(
            (byte)(0x40 |
                (widthˉ64 ? 0x08 : 0) |
                (register >= 8 ? 0x04 : 0) |
                (index >= 8 ? 0x02 : 0) |
                (memory >= 8 ? 0x01 : 0)),
            span);
    }

    private static void Writeˉmultiply(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Destination = statement.Firstˉregister;
        var Source = statement.Secondˉregister;
        Writeˉoperandˉsizeˉprefix(output, Destination, statement.Span);
        Writeˉrex(output, Destination.Width == 64, Destination.Index, Source.Index, statement.Span);
        output.Writeˉu8(0x0F, statement.Span);
        output.Writeˉu8(0xAF, statement.Span);
        output.Writeˉu8(
            (byte)(0xC0 | ((Destination.Index & 7) << 3) | (Source.Index & 7)),
            statement.Span);
    }

    private static void Writeˉimmediate(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Target = statement.Firstˉregister;
        Writeˉoperandˉsizeˉprefix(output, Target, statement.Span);
        Writeˉrex(
            output,
            Target.Width == 64,
            0,
            Target.Index,
            statement.Span,
            force: Requiresˉbyteˉrex(Target));
        var Isˉtest = statement.Kind is Assemblyˉstatementˉkind.Testˉi8 or
            Assemblyˉstatementˉkind.Testˉi16 or Assemblyˉstatementˉkind.Testˉi32;
        output.Writeˉu8(
            Target.Width == 8
                ? Isˉtest ? (byte)0xF6 : (byte)0x80
                : Isˉtest ? (byte)0xF7 : (byte)0x81,
            statement.Span);
        var Group = statement.Kind switch
        {
            Assemblyˉstatementˉkind.Addˉi8 or Assemblyˉstatementˉkind.Addˉi16 or
                Assemblyˉstatementˉkind.Addˉi32 or Assemblyˉstatementˉkind.Testˉi8 or
                Assemblyˉstatementˉkind.Testˉi16 or Assemblyˉstatementˉkind.Testˉi32 => 0,
            Assemblyˉstatementˉkind.Orˉi8 or Assemblyˉstatementˉkind.Orˉi16 or
                Assemblyˉstatementˉkind.Orˉi32 => 1,
            Assemblyˉstatementˉkind.Andˉi8 or Assemblyˉstatementˉkind.Andˉi16 or
                Assemblyˉstatementˉkind.Andˉi32 => 4,
            Assemblyˉstatementˉkind.Subtractˉi8 or Assemblyˉstatementˉkind.Subtractˉi16 or
                Assemblyˉstatementˉkind.Subtractˉi32 => 5,
            Assemblyˉstatementˉkind.Xorˉi8 or Assemblyˉstatementˉkind.Xorˉi16 or
                Assemblyˉstatementˉkind.Xorˉi32 => 6,
            Assemblyˉstatementˉkind.Compareˉi8 or Assemblyˉstatementˉkind.Compareˉi16 or
                Assemblyˉstatementˉkind.Compareˉi32 => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(statement), statement.Kind, null),
        };
        output.Writeˉu8((byte)(0xC0 | (Group << 3) | (Target.Index & 7)), statement.Span);
        if (Target.Width == 8)
        {
            output.Writeˉu8(unchecked((byte)statement.Number), statement.Span);
        }
        else if (Target.Width == 16)
        {
            output.Writeˉu16(unchecked((ushort)statement.Number), statement.Span);
        }
        else
        {
            output.Writeˉi32((int)statement.Number, statement.Span);
        }
    }

    private static void Writeˉshift(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Target = statement.Firstˉregister;
        Writeˉoperandˉsizeˉprefix(output, Target, statement.Span);
        Writeˉrex(
            output,
            Target.Width == 64,
            0,
            Target.Index,
            statement.Span,
            force: Requiresˉbyteˉrex(Target));
        output.Writeˉu8(Target.Width == 8 ? (byte)0xC0 : (byte)0xC1, statement.Span);
        var Group = statement.Kind switch
        {
            Assemblyˉstatementˉkind.Rotateˉleft => 0,
            Assemblyˉstatementˉkind.Rotateˉright => 1,
            Assemblyˉstatementˉkind.Shiftˉleft => 4,
            Assemblyˉstatementˉkind.Shiftˉright => 5,
            Assemblyˉstatementˉkind.Shiftˉrightˉsigned => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(statement), statement.Kind, null),
        };
        output.Writeˉu8((byte)(0xC0 | (Group << 3) | (Target.Index & 7)), statement.Span);
        output.Writeˉu8((byte)statement.Number, statement.Span);
    }

    private static void Writeˉmemory(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Value = statement.Firstˉregister;
        var Base = statement.Secondˉregister;
        var Index = statement.Hasˉindex ? statement.Thirdˉregister.Index : (byte)4;
        Writeˉoperandˉsizeˉprefix(output, Value, statement.Span);
        Writeˉrex(
            output,
            Value.Width == 64,
            Value.Index,
            Base.Index,
            statement.Span,
            statement.Hasˉindex ? statement.Thirdˉregister.Index : (byte)0,
            Requiresˉbyteˉrex(Value));
        output.Writeˉu8(
            statement.Kind is Assemblyˉstatementˉkind.Storeˉmemoryˉu8
                ? (byte)0x88
                : statement.Kind is Assemblyˉstatementˉkind.Loadˉmemoryˉu8
                    ? (byte)0x8A
                    : statement.Kind is Assemblyˉstatementˉkind.Storeˉmemoryˉu16 or
                        Assemblyˉstatementˉkind.Storeˉmemoryˉu32 or
                        Assemblyˉstatementˉkind.Storeˉmemoryˉu64
                        ? (byte)0x89
                        : (byte)0x8B,
            statement.Span);
        output.Writeˉu8((byte)(0x84 | ((Value.Index & 7) << 3)), statement.Span);
        var Scale = statement.Scale switch
        {
            1 => 0,
            2 => 1,
            4 => 2,
            8 => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(statement), statement.Scale, null),
        };
        output.Writeˉu8(
            (byte)((Scale << 6) | ((Index & 7) << 3) | (Base.Index & 7)),
            statement.Span);
        output.Writeˉi32((int)statement.Number, statement.Span);
    }

    private static void Writeˉregisterˉbinary(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Destination = statement.Firstˉregister;
        var Source = statement.Secondˉregister;
        Writeˉoperandˉsizeˉprefix(output, Destination, statement.Span);
        Writeˉrex(
            output,
            Destination.Width == 64,
            Source.Index,
            Destination.Index,
            statement.Span,
            force: Requiresˉbyteˉrex(Destination) || Requiresˉbyteˉrex(Source));
        var Opcode = statement.Kind switch
        {
            Assemblyˉstatementˉkind.Moveˉregister => 0x89,
            Assemblyˉstatementˉkind.Add => 0x01,
            Assemblyˉstatementˉkind.Subtract => 0x29,
            Assemblyˉstatementˉkind.And => 0x21,
            Assemblyˉstatementˉkind.Or => 0x09,
            Assemblyˉstatementˉkind.Xor => 0x31,
            Assemblyˉstatementˉkind.Compare => 0x39,
            Assemblyˉstatementˉkind.Test => 0x85,
            _ => throw new ArgumentOutOfRangeException(nameof(statement), statement.Kind, null),
        };
        if (Destination.Width == 8)
        {
            Opcode--;
        }
        output.Writeˉu8((byte)Opcode, statement.Span);
        output.Writeˉu8(
            (byte)(0xC0 | ((Source.Index & 7) << 3) | (Destination.Index & 7)),
            statement.Span);
    }

    private static void Writeˉlocalˉdisplacement(
        Byteˉbuffer output,
        uint target,
        Assemblyˉspan span)
    {
        var Nextˉinstruction = checked((long)output.Count + sizeof(int));
        var Displacement = checked((long)target - Nextˉinstruction);
        if (Displacement is < int.MinValue or > int.MaxValue)
        {
            throw new Assemblyˉencodingˉexception(
                new("WVA1011", span.Line, span.Column, "Local branch displacement exceeds the relative-i32 range."));
        }
        output.Writeˉi32((int)Displacement, span);
    }

    private static void Writeˉripˉrelative(
        Byteˉbuffer output,
        Assemblyˉstatement statement,
        uint sectionˉindex,
        uint symbolˉindex,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        var Register = statement.Firstˉregister;
        var Widthˉ64 = statement.Kind is Assemblyˉstatementˉkind.Loadˉu64 or
            Assemblyˉstatementˉkind.Storeˉu64 or Assemblyˉstatementˉkind.Loadˉaddress;
        Writeˉoperandˉsizeˉprefix(output, Register, statement.Span);
        Writeˉrex(
            output,
            Widthˉ64,
            Register.Index,
            0,
            statement.Span,
            force: Requiresˉbyteˉrex(Register));
        output.Writeˉu8(
            statement.Kind switch
            {
                Assemblyˉstatementˉkind.Storeˉu8 => 0x88,
                Assemblyˉstatementˉkind.Loadˉu8 => 0x8A,
                Assemblyˉstatementˉkind.Storeˉu16 or
                Assemblyˉstatementˉkind.Storeˉu32 or Assemblyˉstatementˉkind.Storeˉu64 => 0x89,
                Assemblyˉstatementˉkind.Loadˉaddress => 0x8D,
                _ => 0x8B,
            },
            statement.Span);
        output.Writeˉu8((byte)(((Register.Index & 7) << 3) | 0x05), statement.Span);
        var Relativeˉoffset = (uint)output.Count;
        output.Writeˉu32(0, statement.Span);
        relocations.Add(new(
            Objectˉrelocationˉkind.Relativeˉi32,
            sectionˉindex,
            Relativeˉoffset,
            symbolˉindex,
            -4));
    }

    private static void Writeˉconditionˉresult(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Target = statement.Firstˉregister;
        Writeˉrex(
            output,
            widthˉ64: false,
            register: 0,
            memory: Target.Index,
            statement.Span,
            force: Requiresˉbyteˉrex(Target));
        output.Writeˉu8(0x0F, statement.Span);
        output.Writeˉu8((byte)(0x90 + statement.Condition), statement.Span);
        output.Writeˉu8((byte)(0xC0 | (Target.Index & 7)), statement.Span);
    }

    private static void Writeˉextension(Byteˉbuffer output, Assemblyˉstatement statement)
    {
        var Destination = statement.Firstˉregister;
        var Source = statement.Secondˉregister;
        Writeˉrex(
            output,
            Destination.Width == 64,
            Destination.Index,
            Source.Index,
            statement.Span,
            force: Requiresˉbyteˉrex(Source));
        output.Writeˉu8(0x0F, statement.Span);
        output.Writeˉu8(
            statement.Kind switch
            {
                Assemblyˉstatementˉkind.Zeroˉextendˉu8 => 0xB6,
                Assemblyˉstatementˉkind.Zeroˉextendˉu16 => 0xB7,
                Assemblyˉstatementˉkind.Signˉextendˉi8 => 0xBE,
                _ => 0xBF,
            },
            statement.Span);
        output.Writeˉu8(
            (byte)(0xC0 | ((Destination.Index & 7) << 3) | (Source.Index & 7)),
            statement.Span);
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

        public void Writeˉu16(ushort value, Assemblyˉspan span)
        {
            Span<byte> Buffer = stackalloc byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(Buffer, value);
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
