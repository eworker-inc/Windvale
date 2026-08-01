using System.Collections.Immutable;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public sealed record Kernelˉpagingˉartifacts(
    ImmutableArray<byte> Objectˉbytes,
    ImmutableArray<byte> Codeˉbytes,
    ImmutableArray<Objectˉrelocation> Relocations);

public static class Kernelˉpagingˉx64
{
    private const string FAILURE_LABEL = "paging_failure";
    private const string OVERLAP_ACCEPTED_LABEL = "paging_overlap_accepted";
    private const string PD_LOOP_LABEL = "paging_pd_loop";
    private const string NULL_PT_LOOP_LABEL = "paging_null_pt_loop";
    private const string CODE_PT_LOOP_LABEL = "paging_code_pt_loop";
    private const string EXECUTABLE_LOOP_LABEL = "paging_executable_loop";

    private const byte CONDITION_BELOW = 0x82;
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const byte CONDITION_ABOVE = 0x87;
    private const byte CONDITION_ABOVE_OR_EQUAL = 0x83;
    private const byte CONDITION_BELOW_OR_EQUAL = 0x86;

    public static Kernelˉpagingˉartifacts Build()
    {
        var Output = new X64ˉcodeˉbuilder();
        var Relocations = ImmutableArray.CreateBuilder<Objectˉrelocation>();
        Emitˉinstaller(Output, Relocations);
        var Code = Output.Build();
        var Frozenˉrelocations = Relocations.ToImmutable();
        var Object = new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text", Objectˉsectionˉkind.Code, 16, (uint)Code.Length, Code)],
            [
                new(
                    Kernelˉpagingˉcontract.INSTALL_SYMBOL,
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    0,
                    (uint)Code.Length),
                Import(Firmwareˉprobe.ENTRY_SYMBOL),
                Import(Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL),
                Import(Kernelˉpagingˉcontract.PROTECTION_ENABLE_SYMBOL),
                Import(Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL),
            ],
            Frozenˉrelocations);
        var Objectˉbytes = Objectˉcodec.Write(Object).ToImmutableArray();
        Verifyˉobject(Objectˉbytes, Code, Frozenˉrelocations);
        return new(Objectˉbytes, Code, Frozenˉrelocations);
    }

    private static Objectˉsymbol Import(string name) =>
        new(
            name,
            Objectˉsymbolˉbinding.Import,
            Objectˉsymbolˉkind.Function,
            Objectˉlimits.UNDEFINED_SECTION,
            0,
            0);

    private static void Emitˉinstaller(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations)
    {
        // Preserve each nonvolatile register used here and retain 32 bytes of x64 call shadow space.
        output.Emit(0x53, 0x55, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56);
        output.Emit(0x48, 0x83, 0xEC, 0x40);
        output.Emit(0x49, 0x89, 0xCC);

        // Revalidate the memory-state header and the retained handoff map that remains live.
        output.Emit(0x4D, 0x85, 0xE4);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉmemoryˉcontract.STATE_MAGIC);
        output.Emit(0x49, 0x39, 0x04, 0x24);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7C, 0x24, 0x08, (byte)Kernelˉmemoryˉcontract.STATE_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7C, 0x24, 0x0C, (byte)Kernelˉmemoryˉcontract.STATE_HEADER_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x44, 0x24, 0x50);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x4C, 0x24, 0x58);
        output.Emit(0x48, 0x85, 0xC9);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x81, 0xF9);
        output.Emitˉu32(Compiler.X64ˉkernelˉcontract.MAX_MEMORY_MAP_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x48, 0x01, 0xC8);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        Emitˉcompareˉidentityˉceiling(output);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);

        // The active stack and live GDT must survive the identity-root transition.
        output.Emit(0x48, 0x89, 0xE0, 0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        Emitˉcompareˉidentityˉceiling(output);
        output.Jumpˉif(CONDITION_ABOVE_OR_EQUAL, FAILURE_LABEL);
        output.Emit(0x0F, 0x01, 0x44, 0x24, 0x20);
        output.Emit(0x66, 0x83, 0x7C, 0x24, 0x20, 0x00);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x8B, 0x44, 0x24, 0x22, 0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x0F, 0xB7, 0x4C, 0x24, 0x20, 0x48, 0x01, 0xC8);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        Emitˉcompareˉidentityˉceiling(output);
        output.Jumpˉif(CONDITION_ABOVE_OR_EQUAL, FAILURE_LABEL);

        // NX support is mandatory for this W^X root; fail before changing any control state.
        output.Emit(0xB8);
        output.Emitˉu32(0x8000_0000);
        output.Emit(0x0F, 0xA2, 0x3D);
        output.Emitˉu32(0x8000_0001);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        output.Emit(0xB8);
        output.Emitˉu32(0x8000_0001);
        output.Emit(0x0F, 0xA2, 0xF7, 0xC2);
        output.Emitˉu32(1U << 20);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);

        // The linked image starts at the page-aligned boot entry and must fit its fixed 64 KiB RX window.
        output.Emit(0x4C, 0x8D, 0x35);
        var Entryˉfield = output.Position;
        output.Emitˉu32(0);
        relocations.Add(Relocation(Entryˉfield, 1));
        output.Emit(0x41, 0xF7, 0xC6, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x81, 0xFE);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.LARGE_PAGE_BYTES);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        output.Emit(0x49, 0x81, 0xFE);
        output.Emitˉu32((uint)(Kernelˉpagingˉcontract.IDENTITY_BYTES - Kernelˉpagingˉcontract.LARGE_PAGE_BYTES));
        output.Jumpˉif(CONDITION_ABOVE_OR_EQUAL, FAILURE_LABEL);

        // Allocate exactly six contiguous zeroed pages through the existing kernel allocator.
        output.Emit(0x4C, 0x89, 0xE1, 0xBA);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.TABLE_PAGES);
        Emitˉexternalˉcall(output, relocations, 2);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x89, 0xC5);
        output.Emit(0x48, 0xF7, 0xC0, 0xFF, 0x0F, 0x00, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8D, 0x85);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.TABLE_BYTES);
        Emitˉcompareˉidentityˉceiling(output);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);

        // Reject any alias between the table allocation and the fixed executable window.
        output.Emit(0x4C, 0x39, 0xF0);
        output.Jumpˉif(CONDITION_BELOW_OR_EQUAL, OVERLAP_ACCEPTED_LABEL);
        output.Emit(0x49, 0x8D, 0x8E);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.EXECUTABLE_BYTES);
        output.Emit(0x49, 0x39, 0xCD);
        output.Jumpˉif(CONDITION_ABOVE_OR_EQUAL, OVERLAP_ACCEPTED_LABEL);
        output.Jump(FAILURE_LABEL);
        output.Mark(OVERLAP_ACCEPTED_LABEL);

        // Root and directory pointers are supervisor-writable non-leaf entries.
        output.Emit(0x49, 0x8D, 0x85, 0x03, 0x10, 0x00, 0x00, 0x49, 0x89, 0x45, 0x00);
        output.Emit(0x49, 0x8D, 0x85, 0x03, 0x20, 0x00, 0x00, 0x49, 0x89, 0x85);
        output.Emitˉu32(0x1000);

        // Begin with 512 writable/non-executable 2 MiB identity leaves.
        output.Emit(0x4D, 0x8D, 0x95);
        output.Emitˉu32(0x2000);
        output.Emit(0x49, 0xBB);
        output.Emitˉu64(Kernelˉpagingˉcontract.ENTRY_PRESENT |
            Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉpagingˉcontract.ENTRY_LARGE_PAGE |
            Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        output.Emit(0xB9, 0x00, 0x02, 0x00, 0x00);
        output.Mark(PD_LOOP_LABEL);
        output.Emit(0x4D, 0x89, 0x1A, 0x49, 0x83, 0xC2, 0x08, 0x49, 0x81, 0xC3);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.LARGE_PAGE_BYTES);
        output.Emit(0xFF, 0xC9);
        output.Jumpˉif(CONDITION_NOT_EQUAL, PD_LOOP_LABEL);

        // Replace directory zero with a 4 KiB table whose first leaf remains absent.
        output.Emit(0x49, 0x8D, 0x85, 0x03, 0x30, 0x00, 0x00, 0x49, 0x89, 0x85);
        output.Emitˉu32(0x2000);
        output.Emit(0x4D, 0x8D, 0x95, 0x08, 0x30, 0x00, 0x00, 0x49, 0xBB);
        output.Emitˉu64(0x1000 |
            Kernelˉpagingˉcontract.ENTRY_PRESENT |
            Kernelˉpagingˉcontract.ENTRY_WRITABLE |
            Kernelˉpagingˉcontract.ENTRY_NO_EXECUTE);
        output.Emit(0xB9, 0xFF, 0x01, 0x00, 0x00);
        output.Mark(NULL_PT_LOOP_LABEL);
        output.Emit(0x4D, 0x89, 0x1A, 0x49, 0x83, 0xC2, 0x08, 0x49, 0x81, 0xC3);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.PAGE_BYTES);
        output.Emit(0xFF, 0xC9);
        output.Jumpˉif(CONDITION_NOT_EQUAL, NULL_PT_LOOP_LABEL);

        // Materialize two consecutive code-region tables as writable/NX before narrowing leaves.
        output.Emit(0x4C, 0x89, 0xF3, 0x48, 0xC1, 0xEB, 0x15);
        output.Emit(0x4D, 0x8D, 0x95);
        output.Emitˉu32(0x4000);
        output.Emit(0x49, 0x89, 0xDB, 0x49, 0xC1, 0xE3, 0x15);
        output.Emit(0x49, 0x81, 0xCB);
        output.Emitˉu32(3);
        output.Emit(0x49, 0x0F, 0xBA, 0xEB, 0x3F);
        output.Emit(0xB9, 0x00, 0x04, 0x00, 0x00);
        output.Mark(CODE_PT_LOOP_LABEL);
        output.Emit(0x4D, 0x89, 0x1A, 0x49, 0x83, 0xC2, 0x08, 0x49, 0x81, 0xC3);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.PAGE_BYTES);
        output.Emit(0xFF, 0xC9);
        output.Jumpˉif(CONDITION_NOT_EQUAL, CODE_PT_LOOP_LABEL);

        // Point both directory slots at the consecutive code tables.
        output.Emit(0x48, 0x89, 0xD8, 0x48, 0xC1, 0xE0, 0x03);
        output.Emit(0x4D, 0x8D, 0x95);
        output.Emitˉu32(0x2000);
        output.Emit(0x49, 0x01, 0xC2);
        output.Emit(0x49, 0x8D, 0x85, 0x03, 0x40, 0x00, 0x00, 0x49, 0x89, 0x02);
        output.Emit(0x49, 0x8D, 0x85, 0x03, 0x50, 0x00, 0x00, 0x49, 0x89, 0x42, 0x08);

        // Narrow exactly sixteen code leaves to supervisor read-only/executable.
        output.Emit(0x4C, 0x89, 0xF0, 0x48, 0xC1, 0xE8, 0x09, 0x25, 0xF8, 0x0F, 0x00, 0x00);
        output.Emit(0x4D, 0x8D, 0x95);
        output.Emitˉu32(0x4000);
        output.Emit(0x49, 0x01, 0xC2, 0x4D, 0x89, 0xF3, 0x49, 0x83, 0xCB, 0x01);
        output.Emit(0xB9, 0x10, 0x00, 0x00, 0x00);
        output.Mark(EXECUTABLE_LOOP_LABEL);
        output.Emit(0x4D, 0x89, 0x1A, 0x49, 0x83, 0xC2, 0x08, 0x49, 0x81, 0xC3);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.PAGE_BYTES);
        output.Emit(0xFF, 0xC9);
        output.Jumpˉif(CONDITION_NOT_EQUAL, EXECUTABLE_LOOP_LABEL);

        // WVA owns the privileged protection and root-activation operations.
        Emitˉexternalˉcall(output, relocations, 3);
        output.Emit(0x4C, 0x89, 0xE8);
        Emitˉexternalˉcall(output, relocations, 4);
        output.Emit(0x4C, 0x39, 0xE8);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);

        // Publish ownership only after CR3 readback proves that the new root is active.
        Emitˉrecordˉstoreˉu64(output, Kernelˉpagingˉcontract.RECORD_OFFSET,
            Kernelˉpagingˉcontract.RECORD_MAGIC);
        Emitˉrecordˉstoreˉu32(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 8,
            Kernelˉpagingˉcontract.RECORD_VERSION);
        Emitˉrecordˉstoreˉu32(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 12,
            Kernelˉpagingˉcontract.RECORD_BYTES);
        Emitˉrecordˉstoreˉregister(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 16, 0x4D, 0xAC);
        Emitˉrecordˉstoreˉu64(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 24,
            Kernelˉpagingˉcontract.TABLE_PAGES);
        Emitˉrecordˉstoreˉu64(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 32,
            Kernelˉpagingˉcontract.IDENTITY_BYTES);
        Emitˉrecordˉstoreˉregister(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 40, 0x4D, 0xB4);
        Emitˉrecordˉstoreˉu64(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 48,
            Kernelˉpagingˉcontract.EXECUTABLE_BYTES);
        Emitˉrecordˉstoreˉu64(output, Kernelˉpagingˉcontract.RECORD_OFFSET + 56,
            Kernelˉpagingˉcontract.RECORD_FLAGS);
        output.Emit(0x31, 0xC0);
        Emitˉepilogue(output);

        output.Mark(FAILURE_LABEL);
        output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Emitˉepilogue(output);
    }

    private static void Emitˉcompareˉidentityˉceiling(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x48, 0x81, 0xF8);
        output.Emitˉu32((uint)Kernelˉpagingˉcontract.IDENTITY_BYTES);
    }

    private static void Emitˉrecordˉstoreˉu32(X64ˉcodeˉbuilder output, uint offset, uint value)
    {
        output.Emit(0x41, 0xC7, 0x84, 0x24);
        output.Emitˉu32(offset);
        output.Emitˉu32(value);
    }

    private static void Emitˉrecordˉstoreˉu64(X64ˉcodeˉbuilder output, uint offset, ulong value)
    {
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(value);
        Emitˉrecordˉstoreˉregister(output, offset, 0x49, 0x84);
    }

    private static void Emitˉrecordˉstoreˉregister(
        X64ˉcodeˉbuilder output,
        uint offset,
        byte rex,
        byte modrm)
    {
        output.Emit(rex, 0x89, modrm, 0x24);
        output.Emitˉu32(offset);
    }

    private static void Emitˉexternalˉcall(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Objectˉrelocation>.Builder relocations,
        uint symbolˉindex)
    {
        output.Emit(0xE8);
        var Fieldˉoffset = output.Position;
        output.Emitˉu32(0);
        relocations.Add(Relocation(Fieldˉoffset, symbolˉindex));
    }

    private static Objectˉrelocation Relocation(uint offset, uint symbolˉindex) =>
        new(Objectˉrelocationˉkind.Relativeˉi32, 0, offset, symbolˉindex, -4);

    private static void Emitˉepilogue(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x48, 0x83, 0xC4, 0x40, 0x41, 0x5E, 0x41, 0x5D, 0x41, 0x5C, 0x5D, 0x5B, 0xC3);
    }

    private static void Verifyˉobject(
        ImmutableArray<byte> objectˉbytes,
        ImmutableArray<byte> code,
        ImmutableArray<Objectˉrelocation> relocations)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Architecture != Objectˉarchitecture.X86ˉ64 ||
            Object.Sections.Length != 1 ||
            Object.Sections[0] is not { Name: ".text", Kind: Objectˉsectionˉkind.Code, Alignment: 16 } ||
            Object.Sections[0].Memoryˉsize != (uint)code.Length ||
            !Object.Sections[0].Data.AsSpan().SequenceEqual(code.AsSpan()) ||
            Object.Symbols.Length != 5 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉpagingˉcontract.INSTALL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
            } ||
            Object.Symbols[0].Size != (uint)code.Length ||
            Object.Symbols[1] is not { Name: Firmwareˉprobe.ENTRY_SYMBOL, Binding: Objectˉsymbolˉbinding.Import } ||
            Object.Symbols[2] is not { Name: Kernelˉmemoryˉcontract.ALLOCATE_PAGES_SYMBOL, Binding: Objectˉsymbolˉbinding.Import } ||
            Object.Symbols[3] is not { Name: Kernelˉpagingˉcontract.PROTECTION_ENABLE_SYMBOL, Binding: Objectˉsymbolˉbinding.Import } ||
            Object.Symbols[4] is not { Name: Kernelˉpagingˉcontract.PAGE_TABLE_ACTIVATE_SYMBOL, Binding: Objectˉsymbolˉbinding.Import } ||
            !Object.Relocations.AsSpan().SequenceEqual(relocations.AsSpan()) ||
            relocations.Length != 4 ||
            relocations[0].Symbolˉindex != 1 ||
            relocations[1].Symbolˉindex != 2 ||
            relocations[2].Symbolˉindex != 3 ||
            relocations[3].Symbolˉindex != 4)
        {
            throw new InvalidOperationException(
                $"The kernel paging object violated '{Kernelˉpagingˉcontract.TARGET_NAME}'.");
        }
    }
}
