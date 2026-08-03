using System.Collections.Immutable;

namespace Windvale.Bootstrap;

internal sealed record Kernelˉmemoryˉrelocation(uint Offset, uint Symbolˉindex);

internal sealed record Kernelˉmemoryˉcode(
    ImmutableArray<byte> Bytes,
    uint Enterˉbytes,
    uint Allocatorˉoffset,
    ImmutableArray<Kernelˉmemoryˉrelocation> Relocations);

internal static class Kernelˉmemoryˉx64
{
    private const string FAILURE_LABEL = "memory_failure";
    private const string RESTORE_LABEL = "memory_restore";
    private const string SCAN_LABEL = "memory_scan";
    private const string SCAN_NEXT_LABEL = "memory_scan_next";
    private const string SCAN_SELECT_LABEL = "memory_scan_select";
    private const string OVERLAP_LABEL = "memory_overlap";
    private const string OVERLAP_NEXT_LABEL = "memory_overlap_next";
    private const string ALLOCATOR_FAILURE_LABEL = "allocator_failure";
    private const string ALLOCATOR_MARK_LOOP_LABEL = "allocator_mark_loop";
    private const string ALLOCATOR_MARK_BYTE_LABEL = "allocator_mark_byte";
    private const string OWNED_STACK_FAILURE_LABEL = "owned_stack_failure";
    private const string OWNED_STACK_RESTORE_LABEL = "owned_stack_restore";

    private const byte CONDITION_BELOW = 0x82;
    private const byte CONDITION_EQUAL = 0x84;
    private const byte CONDITION_NOT_EQUAL = 0x85;
    private const byte CONDITION_ABOVE = 0x87;
    private const byte CONDITION_ABOVE_OR_EQUAL = 0x83;

    public static Kernelˉmemoryˉcode Build(Firmwareˉprobeˉscenario scenario)
    {
        if (scenario is not Firmwareˉprobeˉscenario.Normal and
            not Firmwareˉprobeˉscenario.Invalidˉopcode and
            not Firmwareˉprobeˉscenario.Generalˉprotection and
            not Firmwareˉprobeˉscenario.Userˉfault and
            not Firmwareˉprobeˉscenario.Serviceˉfault)
        {
            throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var Output = new X64ˉcodeˉbuilder();
        var Relocations = ImmutableArray.CreateBuilder<Kernelˉmemoryˉrelocation>();

        Emitˉenter(Output, Relocations, scenario);
        var Enterˉbytes = Output.Position;
        Output.Align(16);
        var Allocatorˉoffset = Output.Position;
        Emitˉallocator(Output);

        return new(
            Output.Build(), Enterˉbytes, Allocatorˉoffset,
            Relocations.ToImmutable());
    }

    private static void Emitˉenter(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Kernelˉmemoryˉrelocation>.Builder relocations,
        Firmwareˉprobeˉscenario scenario)
    {
        // Preserve every nonvolatile register used by the memory boundary and reserve call shadow space.
        output.Emit(0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57, 0x57);
        output.Emit(0x48, 0x83, 0xEC, 0x20);
        output.Emit(0x49, 0x89, 0xE4);
        output.Emit(0x49, 0x89, 0xCD);

        // Independently validate the versioned handoff and its complete memory-map envelope.
        output.Emit(0x4D, 0x85, 0xED);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Compiler.X64ˉkernelˉcontract.HANDOFF_MAGIC);
        output.Emit(0x49, 0x39, 0x45, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7D, 0x08, (byte)Compiler.X64ˉkernelˉcontract.HANDOFF_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7D, 0x0C, (byte)Compiler.X64ˉkernelˉcontract.HANDOFF_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x83, 0x7D, 0x10, 0x00);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x45, 0x18);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x3D);
        output.Emitˉu32(Compiler.X64ˉkernelˉcontract.MAX_MEMORY_MAP_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x45, 0x20);
        output.Emit(0x48, 0x83, 0xF8, (byte)Compiler.X64ˉkernelˉcontract.MINIMUM_DESCRIPTOR_BYTES);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);
        output.Emit(0x48, 0x3D);
        output.Emitˉu32(Compiler.X64ˉkernelˉcontract.MAXIMUM_DESCRIPTOR_BYTES);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7D, 0x28, (byte)Compiler.X64ˉkernelˉcontract.DESCRIPTOR_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x41, 0x83, 0x7D, 0x2C, 0x00);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x45, 0x18, 0x31, 0xD2, 0x49, 0xF7, 0x75, 0x20);
        output.Emit(0x48, 0x85, 0xD2);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);

        // R10 walks descriptors, R11 is the remaining count, R8 is the stride.
        output.Emit(0x4D, 0x8B, 0x55, 0x10);
        output.Emit(0x49, 0x89, 0xC3);
        output.Emit(0x4D, 0x8B, 0x45, 0x20);
        output.Emit(0x45, 0x31, 0xF6, 0x45, 0x31, 0xFF);
        output.Mark(SCAN_LABEL);

        // Reject malformed descriptors before making an ownership decision.
        output.Emit(0x49, 0xF7, 0x42, 0x08);
        output.Emitˉu32(0x0FFF);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0xF7, 0x42, 0x10);
        output.Emitˉu32(0x0FFF);
        output.Jumpˉif(CONDITION_NOT_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x83, 0x7A, 0x18, 0x00);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x8B, 0x4A, 0x18);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(0x0010_0000_0000_0000);
        output.Emit(0x48, 0x39, 0xC1);
        output.Jumpˉif(CONDITION_ABOVE, FAILURE_LABEL);
        output.Emit(0x48, 0xFF, 0xC9, 0x48, 0xC1, 0xE1, 0x0C, 0x49, 0x03, 0x4A, 0x08);
        output.Jumpˉif(CONDITION_BELOW, FAILURE_LABEL);

        // Choose the lowest complete contracted EfiConventionalMemory arena in [1 MiB, 4 GiB).
        output.Emit(0x41, 0x83, 0x3A, (byte)Kernelˉmemoryˉcontract.EFI_CONVENTIONAL_MEMORY);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SCAN_NEXT_LABEL);
        output.Emit(0x49, 0x8B, 0x42, 0x08, 0x48, 0x3D);
        output.Emitˉu32((uint)Kernelˉmemoryˉcontract.MINIMUM_PHYSICAL_ADDRESS);
        output.Jumpˉif(CONDITION_BELOW, SCAN_NEXT_LABEL);
        output.Emit(0x49, 0x81, 0x7A, 0x18);
        output.Emitˉu32((uint)Kernelˉmemoryˉcontract.ARENA_PAGES);
        output.Jumpˉif(CONDITION_BELOW, SCAN_NEXT_LABEL);

        // The process roots each own one 2 MiB page-table region. Align the complete
        // arena so every bounded process extent selected from it stays in that region.
        output.Emit(0x48, 0x05);
        output.Emitˉu32((uint)(Kernelˉmemoryˉcontract.ARENA_ALIGNMENT_BYTES - 1));
        output.Jumpˉif(CONDITION_BELOW, SCAN_NEXT_LABEL);
        output.Emit(0x48, 0x25);
        output.Emitˉu32(unchecked((uint)~(Kernelˉmemoryˉcontract.ARENA_ALIGNMENT_BYTES - 1)));
        output.Emit(0x48, 0x89, 0xC2, 0x48, 0x81, 0xC2);
        output.Emitˉu32((uint)(Kernelˉmemoryˉcontract.ARENA_BYTES - Kernelˉmemoryˉcontract.PAGE_BYTES));
        output.Jumpˉif(CONDITION_BELOW, SCAN_NEXT_LABEL);
        output.Emit(0x48, 0x39, 0xCA);
        output.Jumpˉif(CONDITION_ABOVE, SCAN_NEXT_LABEL);
        output.Emit(0x48, 0xB9);
        output.Emitˉu64(Kernelˉmemoryˉcontract.MAXIMUM_PHYSICAL_ADDRESS_EXCLUSIVE - Kernelˉmemoryˉcontract.ARENA_BYTES);
        output.Emit(0x48, 0x39, 0xC8);
        output.Jumpˉif(CONDITION_ABOVE, SCAN_NEXT_LABEL);
        output.Emit(0x4D, 0x85, 0xF6);
        output.Jumpˉif(CONDITION_EQUAL, SCAN_SELECT_LABEL);
        output.Emit(0x4C, 0x39, 0xF0);
        output.Jumpˉif(CONDITION_ABOVE_OR_EQUAL, SCAN_NEXT_LABEL);
        output.Mark(SCAN_SELECT_LABEL);
        output.Emit(0x49, 0x89, 0xC6, 0x4D, 0x89, 0xD7);

        output.Mark(SCAN_NEXT_LABEL);
        output.Emit(0x4D, 0x01, 0xC2, 0x49, 0xFF, 0xCB);
        output.Jumpˉif(CONDITION_NOT_EQUAL, SCAN_LABEL);
        output.Emit(0x4D, 0x85, 0xF6);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);

        // A second complete pass proves that no other descriptor aliases the chosen arena.
        output.Emit(0x4D, 0x8B, 0x55, 0x10, 0x4D, 0x8B, 0x45, 0x20);
        output.Emit(0x49, 0x8B, 0x45, 0x18, 0x31, 0xD2, 0x49, 0xF7, 0x75, 0x20);
        output.Emit(0x49, 0x89, 0xC3);
        output.Mark(OVERLAP_LABEL);
        output.Emit(0x4D, 0x39, 0xFA);
        output.Jumpˉif(CONDITION_EQUAL, OVERLAP_NEXT_LABEL);
        output.Emit(0x49, 0x8B, 0x42, 0x08);
        output.Emit(0x49, 0x8D, 0x8E);
        output.Emitˉu32((uint)Kernelˉmemoryˉcontract.ARENA_BYTES);
        output.Emit(0x48, 0x39, 0xC8);
        output.Jumpˉif(CONDITION_ABOVE_OR_EQUAL, OVERLAP_NEXT_LABEL);
        output.Emit(0x49, 0x8B, 0x4A, 0x18, 0x48, 0xFF, 0xC9, 0x48, 0xC1, 0xE1, 0x0C);
        output.Emit(0x49, 0x03, 0x4A, 0x08, 0x4C, 0x39, 0xF1);
        output.Jumpˉif(CONDITION_BELOW, OVERLAP_NEXT_LABEL);
        output.Jump(FAILURE_LABEL);
        output.Mark(OVERLAP_NEXT_LABEL);
        output.Emit(0x4D, 0x01, 0xC2, 0x49, 0xFF, 0xCB);
        output.Jumpˉif(CONDITION_NOT_EQUAL, OVERLAP_LABEL);

        // Clear the owned arena, initialize its durable header, and copy the 48-byte handoff.
        output.Emit(0x4C, 0x89, 0xF7, 0x31, 0xC0, 0xB9);
        output.Emitˉu32((uint)(Kernelˉmemoryˉcontract.ARENA_BYTES / sizeof(ulong)));
        output.Emit(0xFC, 0xF3, 0x48, 0xAB);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉmemoryˉcontract.STATE_MAGIC);
        output.Emit(0x49, 0x89, 0x06);
        Emitˉstoreˉstateˉu32(output, 0x08, Kernelˉmemoryˉcontract.STATE_VERSION);
        Emitˉstoreˉstateˉu32(output, 0x0C, Kernelˉmemoryˉcontract.STATE_HEADER_BYTES);
        output.Emit(0x4D, 0x89, 0x76, 0x10);
        Emitˉstoreˉstateˉu64ˉsmall(output, 0x18, Kernelˉmemoryˉcontract.ARENA_PAGES);
        Emitˉstoreˉstateˉu64ˉsmall(output, 0x20, Kernelˉmemoryˉcontract.FIRST_FREE_PAGE);
        Emitˉstoreˉstateˉu64ˉsmall(output, 0x28, Kernelˉmemoryˉcontract.INITIAL_FREE_PAGES);
        output.Emit(0x49, 0x8D, 0x46, (byte)Kernelˉmemoryˉcontract.HANDOFF_COPY_OFFSET);
        output.Emit(0x49, 0x89, 0x46, 0x30);
        Emitˉstoreˉstateˉu64ˉsmall(output, 0x38, 0);
        // Allocation-map bits outside the 157-page arena are permanently set.
        // The five state/stack pages begin reserved and kernel-owned; later raw
        // IDT/paging allocations mark themselves before process objects start.
        Emitˉstoreˉstateˉu32(output,
            Kernelˉmemoryˉcontract.ALLOCATION_BITMAP_OFFSET, 0x1F);
        Emitˉstoreˉstateˉu8(output,
            Kernelˉmemoryˉcontract.ALLOCATION_BITMAP_OFFSET + 19, 0xE0);
        Emitˉstoreˉstateˉu32(output,
            Kernelˉmemoryˉcontract.PAGE_OWNER_OFFSET, uint.MaxValue);
        Emitˉstoreˉstateˉu8(output,
            Kernelˉmemoryˉcontract.PAGE_OWNER_OFFSET + 4,
            Kernelˉmemoryˉcontract.PAGE_OWNER_KERNEL);
        for (byte Offset = 0; Offset < Compiler.X64ˉkernelˉcontract.HANDOFF_BYTES; Offset += sizeof(ulong))
        {
            output.Emit(0x49, 0x8B, 0x55, Offset, 0x48, 0x89, 0x50, Offset);
        }

        // Exercise the exported allocator once before relying on its state.
        output.Emit(0x4C, 0x89, 0xF1, 0xBA, 0x01, 0x00, 0x00, 0x00);
        Emitˉexternalˉcall(output, relocations, 0);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, FAILURE_LABEL);
        output.Emit(0x49, 0x89, 0x46, 0x38);

        // Install exceptions and paging, then call Main on the two-page kernel-owned stack.
        output.Emit(0x49, 0x8D, 0xA6);
        output.Emitˉu32((uint)((Kernelˉmemoryˉcontract.STATE_PAGES + Kernelˉmemoryˉcontract.STACK_PAGES) * Kernelˉmemoryˉcontract.PAGE_BYTES));
        output.Emit(0x48, 0x83, 0xEC, 0x20);
        output.Emit(0x49, 0x8B, 0x4E, 0x38);
        Emitˉexternalˉcall(output, relocations, 4);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, OWNED_STACK_FAILURE_LABEL);
        output.Emit(0x4C, 0x89, 0xF1);
        Emitˉexternalˉcall(output, relocations, 5);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, OWNED_STACK_FAILURE_LABEL);
        output.Emit(0x49, 0x8D, 0x4E, (byte)Kernelˉmemoryˉcontract.HANDOFF_COPY_OFFSET);
        Emitˉexternalˉcall(output, relocations, 3);
        output.Emit(0x49, 0x89, 0xC7);
        output.Emit(0x48, 0x85, 0xC0);
        output.Jumpˉif(CONDITION_NOT_EQUAL, OWNED_STACK_RESTORE_LABEL);
        output.Emit(0x45, 0x31, 0xFF);
        if (scenario == Firmwareˉprobeˉscenario.Invalidˉopcode)
        {
            output.Emit(0x0F, 0x0B);
            output.Emit(0x41, 0xBF, 0x01, 0x00, 0x00, 0x00);
        }
        else if (scenario == Firmwareˉprobeˉscenario.Generalˉprotection)
        {
            // Dereferencing an address that is noncanonical under both four- and five-level
            // x86-64 paging deterministically raises #GP(0) before translation.
            output.Emit(0x48, 0xB8);
            output.Emitˉu64(0x0100_0000_0000_0000);
            output.Emit(0x8A, 0x00);
            output.Emit(0x41, 0xBF, 0x01, 0x00, 0x00, 0x00);
        }
        output.Jump(OWNED_STACK_RESTORE_LABEL);

        output.Mark(OWNED_STACK_FAILURE_LABEL);
        output.Emit(0x41, 0xBF, 0x01, 0x00, 0x00, 0x00);
        output.Mark(OWNED_STACK_RESTORE_LABEL);
        output.Emit(0x4C, 0x89, 0xE4, 0x4C, 0x89, 0xF8);
        output.Jump(RESTORE_LABEL);

        output.Mark(FAILURE_LABEL);
        output.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        output.Mark(RESTORE_LABEL);
        output.Emit(0x48, 0x83, 0xC4, 0x20, 0x5F, 0x41, 0x5F, 0x41, 0x5E, 0x41, 0x5D, 0x41, 0x5C, 0xC3);
    }

    private static void Emitˉallocator(X64ˉcodeˉbuilder output)
    {
        output.Emit(0x57, 0x48, 0x85, 0xC9);
        output.Jumpˉif(CONDITION_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x48, 0xB8);
        output.Emitˉu64(Kernelˉmemoryˉcontract.STATE_MAGIC);
        output.Emit(0x48, 0x39, 0x01);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x83, 0x79, 0x08, (byte)Kernelˉmemoryˉcontract.STATE_VERSION);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x83, 0x79, 0x0C, (byte)Kernelˉmemoryˉcontract.STATE_HEADER_BYTES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x48, 0x39, 0x49, 0x10);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x48, 0x81, 0x79, 0x18);
        output.Emitˉu32((uint)Kernelˉmemoryˉcontract.ARENA_PAGES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x85, 0xD2);
        output.Jumpˉif(CONDITION_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x44, 0x8B, 0xC2);
        output.Emit(0x4C, 0x3B, 0x41, 0x28);
        output.Jumpˉif(CONDITION_ABOVE, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x4C, 0x8B, 0x49, 0x20, 0x4D, 0x89, 0xCA, 0x4C, 0x03, 0x51, 0x28);
        output.Emit(0x49, 0x81, 0xFA);
        output.Emitˉu32((uint)Kernelˉmemoryˉcontract.ARENA_PAGES);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x4C, 0x89, 0xC8, 0x4C, 0x01, 0xC0);
        output.Emit(0x48, 0x3D);
        output.Emitˉu32((uint)Kernelˉmemoryˉcontract.ARENA_PAGES);
        output.Jumpˉif(CONDITION_ABOVE, ALLOCATOR_FAILURE_LABEL);
        // Mark each fixed bootstrap page in the shared 160-bit map and owner
        // table. R9 is the first page and R8 is the exact count.
        output.Emit(0x4D, 0x89, 0xCA, 0x44, 0x89, 0xC7);
        output.Mark(ALLOCATOR_MARK_LOOP_LABEL);
        // bts qword ptr [rcx + r10/8 + bitmap], r10 is awkward for the fixed
        // raw encoder, so select the exact bit through a bounded byte walk.
        output.Emit(0x4C, 0x89, 0xD0, 0x48, 0xC1, 0xE8, 0x03, 0x4C, 0x8D, 0x99);
        output.Emitˉu32(Kernelˉmemoryˉcontract.ALLOCATION_BITMAP_OFFSET);
        output.Emit(0x49, 0x01, 0xC3, 0x44, 0x89, 0xD0, 0x83, 0xE0, 0x07, 0xB2, 0x01);
        output.Mark(ALLOCATOR_MARK_BYTE_LABEL);
        output.Emit(0x84, 0xC0);
        output.Jumpˉif(CONDITION_EQUAL, ALLOCATOR_MARK_BYTE_LABEL + "_ready");
        output.Emit(0x00, 0xD2, 0xFF, 0xC8);
        output.Jump(ALLOCATOR_MARK_BYTE_LABEL);
        output.Mark(ALLOCATOR_MARK_BYTE_LABEL + "_ready");
        output.Emit(0x41, 0x08, 0x13);
        // mov byte ptr [rcx+r10+PAGE_OWNER_OFFSET], 255
        output.Emit(0x42, 0xC6, 0x84, 0x11);
        output.Emitˉu32(Kernelˉmemoryˉcontract.PAGE_OWNER_OFFSET);
        output.Emit(Kernelˉmemoryˉcontract.PAGE_OWNER_KERNEL);
        output.Emit(0x49, 0xFF, 0xC2, 0x41, 0xFF, 0xC8);
        output.Jumpˉif(CONDITION_NOT_EQUAL, ALLOCATOR_MARK_LOOP_LABEL);
        output.Emit(0x41, 0x89, 0xF8);
        output.Emit(0x49, 0xC1, 0xE1, 0x0C, 0x49, 0x01, 0xC9);
        output.Emit(0x4C, 0x01, 0x41, 0x20, 0x4C, 0x29, 0x41, 0x28);
        output.Emit(0x4D, 0x89, 0xCA, 0x4D, 0x89, 0xC1, 0x49, 0xC1, 0xE1, 0x09);
        output.Emit(0x4C, 0x89, 0xD7, 0x31, 0xC0, 0x4C, 0x89, 0xC9, 0xFC, 0xF3, 0x48, 0xAB);
        output.Emit(0x4C, 0x89, 0xD0, 0x5F, 0xC3);
        output.Mark(ALLOCATOR_FAILURE_LABEL);
        output.Emit(0x31, 0xC0, 0x5F, 0xC3);
    }

    private static void Emitˉexternalˉcall(
        X64ˉcodeˉbuilder output,
        ImmutableArray<Kernelˉmemoryˉrelocation>.Builder relocations,
        uint symbolˉindex)
    {
        relocations.Add(new(output.Emitˉcallˉplaceholder(), symbolˉindex));
    }

    private static void Emitˉstoreˉstateˉu32(
        X64ˉcodeˉbuilder output,
        uint offset,
        uint value)
    {
        if (offset <= byte.MaxValue)
        {
            output.Emit(0x41, 0xC7, 0x46, checked((byte)offset));
        }
        else
        {
            output.Emit(0x41, 0xC7, 0x86);
            output.Emitˉu32(offset);
        }
        output.Emitˉu32(value);
    }

    private static void Emitˉstoreˉstateˉu8(
        X64ˉcodeˉbuilder output,
        uint offset,
        byte value)
    {
        output.Emit(0x41, 0xC6, 0x86);
        output.Emitˉu32(offset);
        output.Emit(value);
    }

    private static void Emitˉstoreˉstateˉu64ˉsmall(
        X64ˉcodeˉbuilder output,
        byte offset,
        ulong value)
    {
        output.Emit(0x49, 0xC7, 0x46, offset);
        output.Emitˉu32(checked((uint)value));
    }
}
